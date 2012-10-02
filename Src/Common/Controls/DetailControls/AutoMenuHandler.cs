// --------------------------------------------------------------------------------------------
#region // Copyright (c) 2003, SIL International. All Rights Reserved.
// <copyright from='2003' to='2003' company='SIL International'>
//		Copyright (c) 2003, SIL International. All Rights Reserved.
//
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
#endregion
//
// File: AutoMenuHandler.cs
// Responsibility:
// Last reviewed:
//
// <remarks>
//	this code is a bit of a mess.  I extracted code from three or four other classes
//	and stuck them in here, so at least the mess would be gathered in one place.
//		this code is not expected to be used for anything other than object browser...
//		i.e., it is not "production code".
//
//	!!!do not re-work this for localization purposes!!!
// </remarks>
// --------------------------------------------------------------------------------------------
using System;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

using SIL.FieldWorks.Common.COMInterfaces;
using SIL.FieldWorks.Common.Framework.DetailControls.Resources;
using SIL.FieldWorks.FDO;
using SIL.FieldWorks.FdoUi;

namespace SIL.FieldWorks.Common.Framework.DetailControls
{
	/// <summary>
	/// This provides an automatically constructed context menu for data tree slices.
	/// Normally, we use hand constructed ones to more carefully control what is available in the menu.
	/// This automatic one makes programs which cannot require customization (like the object browser) possible.
	/// </summary>
	public class AutoDataTreeMenuHandler
	{
		//protected SliceTreeNode sliceTreeNode;
		protected List<ClassAndPropInfo> m_rgcpiCreateOptions; // array of ClassAndPropInfo corresponding to things we can create.
		protected ContextMenuHelper m_helper = null;  // ideally set only while menu displayed.

		/// <summary>
		/// Tree form.
		/// </summary>
		protected DataTree m_dataEntryForm=null;


		private System.Windows.Forms.MenuItem m_mnuCreate;
		private System.Windows.Forms.MenuItem m_mnuDelete;
		private System.Windows.Forms.ContextMenu m_contextMenu;

		public  AutoDataTreeMenuHandler(DataTree dataEntryForm)
		{
			m_dataEntryForm = dataEntryForm;

		}

		/// <summary>
		/// set up the menu (not the items)
		/// </summary>
		/// <remarks> this is called repeatedly only because they create menu was always coming up empty
		/// after the first time it was shown.  So, essentially, this is covering up either some  bug
		/// or something I really don't understand (JH).
		/// </remarks>
		protected void Initialize()
		{
			this.m_contextMenu = new System.Windows.Forms.ContextMenu();
			this.m_mnuCreate = new System.Windows.Forms.MenuItem();
			this.m_mnuDelete = new System.Windows.Forms.MenuItem();
			//
			// m_contextMenu
			//
			MenuItem notice = new MenuItem("Using auto menu");
			notice.Enabled=false;
			this.m_contextMenu.MenuItems.AddRange(
				new System.Windows.Forms.MenuItem[]{notice, new MenuItem("-"),
													   this.m_mnuCreate, this.m_mnuDelete});
			//
			// m_mnuCreate
			//
			this.m_mnuCreate.Text = DetailControlsStrings.ksCreate;
			this.m_mnuDelete.Text = DetailControlsStrings.ksDelete;
		}

		/// <summary>
		/// Set up the submenu items for 'Create' and 'Insert' menus.
		/// </summary>
		protected void SetupContextMenu(Slice slice, SliceTreeNode sliceTreeNode)
		{
			// Get rid of old sub-menu items.
			m_mnuCreate.MenuItems.Clear();

			m_helper = new ContextMenuHelper(sliceTreeNode);
			m_rgcpiCreateOptions = slice.Object.PropsAndClassesOwnedBy;
			// Fill in info about which object will be the new owner.
			for (int index = m_rgcpiCreateOptions.Count; --index >= 0; )
			{
				ClassAndPropInfo cpi = m_rgcpiCreateOptions[index];
				cpi.hvoOwner = slice.Object.Hvo;
			}
			// Drop atomic property options if not empty
			for (int index = m_rgcpiCreateOptions.Count; --index >= 0; )
			{
				ClassAndPropInfo cpi = (ClassAndPropInfo)m_rgcpiCreateOptions[index];
				if (cpi.fieldType == (int)FieldType.kcptOwningAtom)
				{
					FDO.FdoCache cache = slice.ContainingDataTree.Cache;
					if (cache.GetObjProperty(cpi.hvoOwner, (int)(cpi.flid)) != 0)
						m_rgcpiCreateOptions.RemoveAt(index);
				}
			}
			int iBefore = m_rgcpiCreateOptions.Count;
			GetCreateSiblingOptions(slice, m_rgcpiCreateOptions, true);
			bool fSeq = m_rgcpiCreateOptions.Count > iBefore &&
				m_rgcpiCreateOptions[iBefore].fieldType == (int)FieldType.kcptOwningSequence;
			int iAfter = m_rgcpiCreateOptions.Count;
			if (fSeq)
			{
				// In a sequence we make another set of options to insert after the current item.
				GetCreateSiblingOptions(slice, m_rgcpiCreateOptions, false);
			}
			for(int index = 0; index < m_rgcpiCreateOptions.Count; index++)
			{
				ClassAndPropInfo cpi = (ClassAndPropInfo)m_rgcpiCreateOptions[index];
				String format = DetailControlsStrings.ksNewItem;
				if (index >= iAfter)
				{
					format = DetailControlsStrings.ksNewItemAfter; // can't happen if not sequence
				}
				else if (index >= iBefore)
				{
					if (fSeq)
						format = DetailControlsStrings.ksNewItemBefore;
					else // sibling collection
						format = DetailControlsStrings.ksNewItemWithin;
				}
				m_mnuCreate.MenuItems.Add(new MenuItem(
					String.Format(format, new object[] {cpi.signatureClassName, cpi.fieldName}),
					new EventHandler(this.HandleCreateMenuItem)));
			}
			m_mnuCreate.Enabled = m_mnuCreate.MenuItems.Count > 0;
			m_helper.SetupDeleteMenu(m_mnuDelete);
		}


		/// <summary>
		/// Append to options a sequence of ClassAndPropInfo objects indicating classes of object
		/// that can be added as siblings of the object represented by this slice.
		/// The ihvoPosition setting in the object will be for inserting before the object
		/// of the current slice if fBefore is true, after if it is false.
		/// </summary>
		/// <param name="options"></param>
		/// <param name="fBefore"></param>
		public void GetCreateSiblingOptions(Slice slice, List<ClassAndPropInfo> options, bool fBefore)
		{
			int hvoOwner;
			int flid;
			int ihvoPosition;
			if (!slice.GetSeqContext(out hvoOwner, out flid, out ihvoPosition))
				return; // empty
			// If in a sequence, and if we want to insert after this object, increment the position.
			// In a collection ihvoPosition is not used.
			if (!fBefore)
				ihvoPosition++;
			int firstSibling = options.Count;
			slice.Cache.AddClassesForField((uint)flid, true, options);

			// NB: Do NOT change this to a ForEach!! It must NOT operate on all items in the collection,
			// ONLY on the sibling options just added.
			for (int i = firstSibling; i < options.Count; ++i)
			{
				ClassAndPropInfo cpi = (ClassAndPropInfo) options[i];
				cpi.hvoOwner = hvoOwner;
				cpi.ihvoPosition = ihvoPosition;
			}
		}
		public void HandleCreateMenuItem(object sender, EventArgs ea)
		{
			Slice slice = m_dataEntryForm.CurrentSlice;

			if (slice != null)
			{
				ClassAndPropInfo cpi = (ClassAndPropInfo)m_rgcpiCreateOptions[((MenuItem)sender).Index];
				FDO.FdoCache cache = slice.ContainingDataTree.Cache;
				int hvoOwner = cpi.hvoOwner;
				int ihvoPosition = cpi.ihvoPosition;
				if (ihvoPosition == ClassAndPropInfo.kposNotSet && cpi.fieldType == (int)FieldType.kcptOwningSequence)
				{
					// insert at end of sequence.
					ihvoPosition = cache.GetVectorSize(hvoOwner, (int)cpi.flid);
				} // otherwise we already worked out the position or it doesn't matter
				// Note: ihvoPosition ignored if sequence or atomic.
				int hvoNew = cache.CreateObject((int)(cpi.signatureClsid), hvoOwner, (int)(cpi.flid), ihvoPosition);
				cache.MainCacheAccessor.PropChanged(null, (int)PropChangeType.kpctNotifyAll, hvoOwner, (int)(cpi.flid), ihvoPosition, 1, 0);
				m_helper = null; // allow old one to be garbage.
				if (hvoOwner == slice.Object.Hvo && slice.Expansion == DataTree.TreeItemState.ktisCollapsed)
				{
					// We added something to the object of the current slice...almost certainly it
					// will be something that will display under this node...if it is still collapsed,
					// expand it to show the thing inserted.
					slice.TreeNode.ToggleExpansion(slice.IndexInContainer);
				}
				Slice child = slice.ExpandSubItem(hvoNew);
				if (child != null)
					child.FocusSliceOrChild();
			}
		}

		/// <summary>
		/// Invoked by a DataTree (which is in turn invoked by the slice)
		/// when the user does something to bring up a context menu
		/// </summary>
		public ContextMenu GetSliceContextMenu(object sender, SIL.FieldWorks.Common.Framework.DetailControls.SliceMenuRequestArgs e)
		{
			Initialize();

			SetupContextMenu(e.Slice, e.Slice.TreeNode);

			return m_contextMenu;
		}

		/// <summary>
		/// Summary description for ContextMenuHelper.
		/// </summary>
		/// <remarks>if we come up with a need for this beyond the auto menu and other, this can become
		/// a more visible class in its own right.</remarks>
		public class ContextMenuHelper
		{
			// Properties used to store information about object we will delete if that
			// item is chosen.
			protected int m_hvoDeleteTarget;

			protected SliceTreeNode m_sliceTreeNode;
			protected FdoCache m_cache;
			protected IFwMetaDataCache m_mdc; // allows us to interpret class and field names and trace superclasses.

			/// -----------------------------------------------------------------------------------
			/// <summary>
			/// Initializes a new instance of the <see cref="ContextMenuHelper"/> class.
			/// </summary>
			/// -----------------------------------------------------------------------------------
			public ContextMenuHelper(FdoCache cache)
			{
				m_cache = cache;
				m_mdc = m_cache.MetaDataCacheAccessor;
			}
			/// -----------------------------------------------------------------------------------
			/// <summary>
			/// Initializes a new instance of the <see cref="ContextMenuHelper"/> class, linked
			/// to a particular tree node.
			/// </summary>
			/// -----------------------------------------------------------------------------------
			public ContextMenuHelper(SliceTreeNode stn)
			{
				m_sliceTreeNode = stn;
				m_cache= m_sliceTreeNode.Slice.ContainingDataTree.Cache;
				m_mdc = m_cache.MetaDataCacheAccessor;
			}

			public int GetFlid(int classId, string fieldName)
			{
				return GetFlid(m_mdc, classId, fieldName);
			}
			public static int GetFlid(IFwMetaDataCache mdc, int classId, string fieldName)
			{
				return (int)mdc.GetFieldId2((uint) classId, fieldName, true);
			}
			public int GetFlid(string stClassName, string stFieldName)
			{
				return GetFlid(m_mdc, stClassName, stFieldName);
			}
			//
			public static int GetFlid(IFwMetaDataCache mdc, string stClassName, string stFieldName)
			{
				int clsid = (int) mdc.GetClassId(stClassName);
				return GetFlid(mdc, clsid, stFieldName);
			}
			public void SetupDeleteMenu(MenuItem mnuDelete)
			{
				mnuDelete.MenuItems.Clear();
				int hvoDeleteOwner;
				int flidDelete;
				int ihvoDelete = -1; // default for atomic
				Slice slice = m_sliceTreeNode.Slice;

				bool isAtomic = slice.GetAtomicContext(out hvoDeleteOwner, out flidDelete);
				if (isAtomic
					|| slice.GetSeqContext(out hvoDeleteOwner, out flidDelete, out ihvoDelete))
				{
					if (ihvoDelete >= 0)
						m_hvoDeleteTarget = m_cache.GetVectorItem(hvoDeleteOwner, flidDelete, ihvoDelete);
					else
						m_hvoDeleteTarget = m_cache.GetObjProperty(hvoDeleteOwner, flidDelete);
					int clsid = m_cache.GetClassOfObject(m_hvoDeleteTarget);
					string targetClassName = m_cache.GetClassName((uint)clsid);
					string attrName = m_mdc.GetFieldName((uint)flidDelete);
					MenuItem item = new MenuItem(String.Format(DetailControlsStrings.ksItemFrom,
													 new object[] {targetClassName, attrName}),
						new EventHandler(this.HandleDeleteMenuItem));

					item.Enabled = OkToDelete(hvoDeleteOwner, flidDelete , isAtomic);
					mnuDelete.MenuItems.Add(item);


				}
				mnuDelete.Enabled = mnuDelete.MenuItems.Count > 0;
			}

			/// <summary>
			///
			/// </summary>
			/// <remarks>NB: this assumes that the user interfaces only lets the user delete one at a time.
			/// that is, if the user interface was going to let them delete the last two items of
			/// a required the sequence,
			/// this would mistakenly say it was OK.</remarks>
			/// <param name="hvoDeleteOwner"></param>
			/// <param name="flidDelete"></param>
			/// <param name="isAtomic"></param>
			/// <returns></returns>
			protected bool OkToDelete(int hvoDeleteOwner, int flidDelete, bool isAtomic)
			{
				ICmObject owner = CmObject.CreateFromDBObject(this.m_cache, hvoDeleteOwner);
				if(!owner.IsFieldRequired(flidDelete))
					return true;
				if (isAtomic)
					return false;
				else	//still OK to delete so long as it is not the last item.
					return this.m_cache.GetVectorSize(hvoDeleteOwner, flidDelete) > 1;
			}

			public void HandleDeleteMenuItem(Object src, System.EventArgs ea)
			{
				m_cache.BeginUndoTask(DetailControlsStrings.ksUndoDelete,
					DetailControlsStrings.ksRedoDelete);
				using (CmObjectUi ui = CmObjectUi.MakeUi(m_cache, m_hvoDeleteTarget))
				{
					ui.Mediator = m_sliceTreeNode.Slice.ContainingDataTree.Mediator;
					ui.DeleteUnderlyingObject();
				}
				m_cache.EndUndoTask();
			}
		}

	}



}

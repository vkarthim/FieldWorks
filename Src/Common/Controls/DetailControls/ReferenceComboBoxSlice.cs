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
// File: ReferenceComboBoxSlice.cs
// Responsibility: RandyR
// Last reviewed:
//
// <remarks>
// Implements the "referenceComboBox" XDE editor.
// </remarks>
// --------------------------------------------------------------------------------------------
using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices; // needed for Marshal
using SIL.FieldWorks.FDO;
using SIL.FieldWorks.FDO.Ling;
using SIL.FieldWorks.Common.COMInterfaces;
using SIL.FieldWorks.Common.Framework.DetailControls.Resources;
using SIL.FieldWorks.LexText.Controls;
using SIL.FieldWorks.Common.Controls;
using SIL.FieldWorks.Common.Widgets;
using SIL.FieldWorks.FdoUi;
using SIL.Utils;
using XCore;
using SIL.FieldWorks.Common.Utils;

namespace SIL.FieldWorks.Common.Framework.DetailControls
{
	/// <summary>
	/// Summary description for ReferenceComboBoxSlice.
	/// </summary>
	public class ReferenceComboBoxSlice : FieldSlice
	{
		protected bool m_processSelectionEvent = true;
		protected int m_currentSelectedIndex;
		protected FwComboBox m_combo;
		protected IPersistenceProvider m_persistProvider;
		protected Mediator m_mediator;

		private string m_sNullItemLabel;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="obj">CmObject that is being displayed.</param>
		/// <param name="flid">The field identifier for the attribute we are displaying.</param>
		public ReferenceComboBoxSlice(FdoCache cache, ICmObject obj, int flid,
			IPersistenceProvider persistenceProvider, Mediator mediator)
			: base(new UserControl(), cache, obj, flid)
		{
			m_mediator = mediator;
			m_persistProvider = persistenceProvider;
			m_combo = new FwComboBox();
			m_combo.WritingSystemFactory = cache.LanguageWritingSystemFactoryAccessor;
			m_combo.DropDownStyle = ComboBoxStyle.DropDownList;
			m_combo.Font = new System.Drawing.Font(
				cache.LangProject.DefaultVernacularWritingSystemFont, 10);
			if (!Application.RenderWithVisualStyles)
				m_combo.HasBorder = false;
			m_combo.Dock = DockStyle.Left;
			m_combo.Width = 200;
			Control.Height = m_combo.Height; // Combo has sensible default height, UserControl does not.
			Control.Controls.Add(m_combo);

			m_combo.SelectedIndexChanged += new EventHandler(SelectionChanged);

			// Load the special strings from the string table if possible.  If not, use the
			// default (English) values.
			if (mediator != null && mediator.HasStringTable)
			{
				StringTbl = mediator.StringTbl;
				if (StringTbl != null)
				{
					m_sNullItemLabel = StringTbl.GetString("NullItemLabel",
						"DetailControls/ReferenceComboBox");
				}
			}
			if (m_sNullItemLabel == null || m_sNullItemLabel == "" ||
				m_sNullItemLabel == "*NullItemLabel*")
			{
				m_sNullItemLabel = DetailControlsStrings.ksNullLabel;
			}
		}

		#region IDisposable override

		/// <summary>
		/// Executes in two distinct scenarios.
		///
		/// 1. If disposing is true, the method has been called directly
		/// or indirectly by a user's code via the Dispose method.
		/// Both managed and unmanaged resources can be disposed.
		///
		/// 2. If disposing is false, the method has been called by the
		/// runtime from inside the finalizer and you should not reference (access)
		/// other managed objects, as they already have been garbage collected.
		/// Only unmanaged resources can be disposed.
		/// </summary>
		/// <param name="disposing"></param>
		/// <remarks>
		/// If any exceptions are thrown, that is fine.
		/// If the method is being done in a finalizer, it will be ignored.
		/// If it is thrown by client code calling Dispose,
		/// it needs to be handled by fixing the bug.
		///
		/// If subclasses override this method, they should call the base implementation.
		/// </remarks>
		protected override void Dispose(bool disposing)
		{
			//Debug.WriteLineIf(!disposing, "****************** " + GetType().Name + " 'disposing' is false. ******************");
			// Must not be run more than once.
			if (IsDisposed)
				return;

			if (disposing)
			{
				// Dispose managed resources here.
				if (m_combo != null)
				{
					m_combo.SelectedIndexChanged -= new EventHandler(SelectionChanged);
					Control.Controls.Remove(m_combo);
					m_combo.Dispose();
				}
			}

			// Dispose unmanaged resources here, whether disposing is true or false.
			m_combo = null;
			m_mediator = null;
			m_persistProvider = null;

			base.Dispose(disposing);
		}

		#endregion IDisposable override

		protected void UpdateDisplayFromDatabase(string displayNameProperty)
		{
			DoUpdateDisplayFromDatabase(displayNameProperty);
		}
		protected override void UpdateDisplayFromDatabase()
		{
			DoUpdateDisplayFromDatabase(null);
		}
		private void DoUpdateDisplayFromDatabase(string displayNameProperty)
		{
			m_processSelectionEvent = false;
			m_currentSelectedIndex = 0;
			m_combo.Items.Clear();
			Set<int> candidates = Object.ReferenceTargetCandidates(m_flid);
			ObjectLabelCollection labels = new ObjectLabelCollection(m_cache, candidates,
				displayNameProperty);
			int currentValue = m_cache.GetObjProperty(Object.Hvo, m_flid);
			int idx = 0;
			foreach(ObjectLabel ol in labels)
			{
				m_combo.Items.Add(ol);
				if (ol.Hvo == currentValue)
				{
					m_combo.SelectedItem = ol;
					m_currentSelectedIndex = idx;
				}
				idx++;
			}
			idx = m_combo.Items.Add(NullItemLabel);
			if (currentValue == 0)
			{
				m_combo.SelectedIndex = idx;
				m_currentSelectedIndex = idx;
			}
			m_processSelectionEvent = true;
		}

		/// <summary>
		/// what is the default label for the null state
		/// </summary>
		protected virtual string NullItemLabel
		{
			get { return m_sNullItemLabel; }
		}

		/// <summary>
		/// Event handler for selection changed in combo box.
		/// </summary>
		/// <param name="sender">Source control</param>
		/// <param name="e"></param>
		protected virtual void SelectionChanged(object sender, EventArgs e)
		{
			Debug.Assert(m_combo != null);

			if (!m_processSelectionEvent)
				return;

			int newValue;
			if (m_combo.SelectedItem.ToString() == NullItemLabel)
				newValue = 0;
			else
				newValue = ((ObjectLabel)m_combo.SelectedItem).Hvo;

			m_cache.BeginUndoTask(
				String.Format(DetailControlsStrings.ksUndoSet, m_fieldName),
				String.Format(DetailControlsStrings.ksRedoSet, m_fieldName));
			m_cache.SetObjProperty(Object.Hvo, m_flid, newValue);
			m_cache.EndUndoTask();
		}
		public override void RegisterWithContextHelper ()
		{
			CheckDisposed();
			if (this.Control != null)
			{
				Mediator mediator = this.Mediator;
				StringTable tbl = null;
				if (mediator.HasStringTable)
					tbl = mediator.StringTbl;
				string caption = XmlUtils.GetLocalizedAttributeValue(tbl, ConfigurationNode, "label", "");
				mediator.SendMessage("RegisterHelpTargetWithId",
					new object[]{m_combo.Controls[0], caption, HelpId}, false);
				//balloon was making it hard to actually click this
				//Mediator.SendMessage("RegisterHelpTargetWithId",
				//	new object[]{launcher.Controls[1], caption, HelpId, "Button"}, false);
			}
		}
	}
}

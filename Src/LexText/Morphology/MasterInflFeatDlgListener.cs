// --------------------------------------------------------------------------------------------
#region // Copyright (c) 2005, SIL International. All Rights Reserved.
// <copyright from='2005' to='2005' company='SIL International'>
//		Copyright (c) 2005, SIL International. All Rights Reserved.
//
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
#endregion
//
// File: MasterInflFeatDlgListener.cs
// Responsibility: Andy Black
// Last reviewed:
//
// <remarks>
// </remarks>
// --------------------------------------------------------------------------------------------
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using SIL.FieldWorks.Common.Utils;
using SIL.Utils;
using SIL.FieldWorks.FDO;
using SIL.FieldWorks.LexText.Controls;

namespace SIL.FieldWorks.XWorks.MorphologyEditor
{
	/// <summary>
	/// Listener class for adding POSes via Insert menu.
	/// </summary>
	[XCore.MediatorDispose]
	public class MasterInflFeatDlgListener : MasterDlgListener
	{

		#region Properties

		protected override string PersistentLabel
		{
			get { return "InsertInflectionFeature"; }
		}

		#endregion Properties

		#region Construction and Initialization

		/// <summary>
		/// Constructor.
		/// </summary>
		public MasterInflFeatDlgListener()
		{
		}

		#endregion Construction and Initialization

		#region IDisposable & Co. implementation
		// Region last reviewed: never

		/// <summary>
		/// Finalizer, in case client doesn't dispose it.
		/// Force Dispose(false) if not already called (i.e. m_isDisposed is true)
		/// </summary>
		/// <remarks>
		/// In case some clients forget to dispose it directly.
		/// </remarks>
		~MasterInflFeatDlgListener()
		{
			Dispose(false);
			// The base class finalizer is called automatically.
		}


		#endregion IDisposable & Co. implementation

		#region XCORE Message Handlers

		/// <summary>
		/// Handles the xWorks message to insert a new FsFeatDefn.
		/// Invoked by the RecordClerk via a main menu.
		/// </summary>
		/// <param name="argument">The xCore Command object.</param>
		/// <returns>true, if we handled the message, otherwise false, if there was an unsupported 'classname' parameter</returns>
		public override bool OnDialogInsertItemInVector(object argument)
		{
			CheckDisposed();

			Debug.Assert(argument != null && argument is XCore.Command);
			string className = XmlUtils.GetOptionalAttributeValue(
				(argument as XCore.Command).Parameters[0], "className");
			if (className == null || ((className != "FsClosedFeature") && (className != "FsComplexFeature")))
				return false;
			if (className == "FsClosedFeature" && (argument as XCore.Command).Id != "CmdInsertClosedFeature")
				return false;

			using (MasterInflectionFeatureListDlg dlg = new MasterInflectionFeatureListDlg(className))
			{
				FdoCache cache = (FdoCache)m_mediator.PropertyTable.GetValue("cache");
				Debug.Assert(cache != null);
				dlg.SetDlginfo(cache.LangProject.MsFeatureSystemOA, m_mediator, true);
				switch (dlg.ShowDialog((Form)m_mediator.PropertyTable.GetValue("window")))
				{
					case DialogResult.OK: // Fall through.
					case DialogResult.Yes:
						//m_mediator.SendMessage("JumpToRecord", dlg.SelectedFeatDefn.Hvo);
						// This is the equivalent functionality, but is deferred processing.
						// This is done so that the JumpToRecord can be processed last.
						m_mediator.BroadcastMessageUntilHandled("JumpToRecord", dlg.SelectedFeatDefn.Hvo);
						// LT-6412: this call will now cause the Mediator to be disposed while it is busy processing
						// this call, so there is code in the Mediator to handle in the middle of a msg the case
						// where the object is nolonger valid.  This has happend before and was being handled, this
						// call "SendMessageToAllNow" has not had the code to handle the exception, so it was added.
						m_mediator.SendMessageToAllNow("MasterRefresh", cache.LangProject.MsFeatureSystemOA);
						break;
				}
			}
			return true; // We "handled" the message, regardless of what happened.
		}

		#endregion XCORE Message Handlers
	}
}

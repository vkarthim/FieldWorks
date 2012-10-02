using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.Diagnostics;
using System.Reflection;

using SIL.FieldWorks.FDO;
using SIL.FieldWorks.FDO.Ling;
using SIL.FieldWorks.FDO.Cellar;
using SIL.FieldWorks.Common.Controls;
using SIL.FieldWorks.Common.COMInterfaces;
using SIL.FieldWorks.XWorks;
using SIL.FieldWorks.FdoUi;
using SIL.Utils;
using XCore;

namespace SIL.FieldWorks.IText
{
	/// <summary>
	/// This class is like its superclass, except it reomves the superclass' top control and pane bar.
	/// </summary>
	public class InterlinMasterNoTitleBar : InterlinMaster
	{
		private System.ComponentModel.IContainer components = null;

		public InterlinMasterNoTitleBar()
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			//Debug.WriteLineIf(!disposing, "****************** " + GetType().Name + " 'disposing' is false. ******************");
			// Must not be run more than once.
			if (IsDisposed)
				return;

			if( disposing )
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}
		#endregion

		/// <summary>
		/// Override method to get rid of up to two controls.
		/// </summary>
		protected override void AddPaneBar()
		{
			base.AddPaneBar();

			if (m_informationBar != null)
			{
				Controls.Remove(m_informationBar);
				m_informationBar.Dispose();
				m_informationBar = null;
			}

			if (m_tcPane != null)
			{
				Controls.Remove(m_tcPane);
				m_tcPane.Dispose();
				m_tcPane = null;
			}
		}
	}
}

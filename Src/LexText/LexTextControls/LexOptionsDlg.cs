// --------------------------------------------------------------------------------------------
#region // Copyright (c) 2007, SIL International. All Rights Reserved.
// <copyright from='2007' to='2007' company='SIL International'>
//		Copyright (c) 2007, SIL International. All Rights Reserved.
//
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
#endregion
//
// File: LexOptionsDlg.cs
// Responsibility: Steve McConnel
// Last reviewed:
//
// <remarks>
// This implements the "Tools/Options" command dialog for Language Explorer.
// </remarks>
// --------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Xml;

using SIL.FieldWorks.Common.Utils;
using SIL.FieldWorks.FDO;
using SIL.FieldWorks.Common.FwUtils;
using SIL.FieldWorks.Common.Framework;
using XCore;
using SIL.Utils;

namespace SIL.FieldWorks.LexText.Controls
{
	public partial class LexOptionsDlg : Form, IFwExtension
	{
		private Mediator m_mediator = null;
		private FdoCache m_cache = null;
		private string m_sUserWs = null;
		private string m_sNewUserWs = null;
		private bool m_pluginsUpdated = false;
		private Dictionary<string, bool> m_plugins = new Dictionary<string, bool>();
		private const string s_helpTopic = "khtpLexOptions";
		private System.Windows.Forms.HelpProvider helpProvider;

		public LexOptionsDlg()
		{
			InitializeComponent();

			if (FwApp.App != null) // Will be null when running tests
			{
				this.helpProvider = new System.Windows.Forms.HelpProvider();
				this.helpProvider.HelpNamespace = FwApp.App.HelpFile;
				this.helpProvider.SetHelpKeyword(this, FwApp.App.GetHelpString(s_helpTopic, 0));
				this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
			}

		}

		private void m_btnOK_Click(object sender, EventArgs e)
		{
			m_sNewUserWs = m_userInterfaceChooser.NewUserWs;
			if (m_sUserWs != m_sNewUserWs)
			{
				CultureInfo ci = MiscUtils.GetCultureForWs(m_sNewUserWs);
				if (ci != null)
				{
					FormLanguageSwitchSingleton.Instance.ChangeCurrentThreadUICulture(ci);
					FormLanguageSwitchSingleton.Instance.ChangeLanguage(this);
					Microsoft.Win32.RegistryKey settingsKey = FwApp.App.SettingsKey;
					settingsKey.SetValue("UserWs", m_sNewUserWs);
				}
				// Reload the mediator's string table with the appropriate language data.
				m_mediator.StringTbl.Reload(m_sNewUserWs);
			}

			// Handle installing/uninstalling plugins.
			if (m_lvPlugins.Items.Count > 0)
			{
				List<XmlDocument> pluginsToInstall = new List<XmlDocument>();
				List<XmlDocument> pluginsToUninstall = new List<XmlDocument>();
				foreach (ListViewItem lvi in m_lvPlugins.Items)
				{
					string name = lvi.Text;
					XmlDocument managerDoc = lvi.Tag as XmlDocument;
					if (lvi.Checked && !m_plugins[name])
					{
						// Remember we need to install it.
						pluginsToInstall.Add(managerDoc);
					}
					else if (!lvi.Checked && m_plugins[name])
					{
						// Remember we need to uninstall it.
						pluginsToUninstall.Add(managerDoc);
					}
				}
				m_pluginsUpdated = pluginsToInstall.Count > 0 || pluginsToUninstall.Count > 0;
				string basePluginPath = DirectoryFinder.GetFWCodeSubDirectory(@"Language Explorer\Configuration\Available Plugins");
				// The extension XML files should be stored in the data area, not in the code area.
				// This reduces the need for users to have administrative privileges.
				string baseExtensionPath = Path.Combine(DirectoryFinder.FWDataDirectory, @"Language Explorer\Configuration");
				// Really do the install now.
				foreach (XmlDocument managerDoc in pluginsToInstall)
				{
					XmlNode managerNode = managerDoc.SelectSingleNode("/manager");
					string srcDir = Path.Combine(basePluginPath, managerNode.Attributes["name"].Value);
					XmlNode configfilesNode = managerNode.SelectSingleNode("configfiles");
					string extensionPath = Path.Combine(baseExtensionPath, configfilesNode.Attributes["targetdir"].Value);
					Directory.CreateDirectory(extensionPath);
					foreach (XmlNode fileNode in configfilesNode.SelectNodes("file"))
					{
						string filename = fileNode.Attributes["name"].Value;
						string extensionPathname = Path.Combine(extensionPath, filename);
						try
						{
							File.Copy(
								Path.Combine(srcDir, filename),
								extensionPathname,
								true);
							File.SetAttributes(extensionPathname, FileAttributes.Normal);
						}
						catch
						{
							// Eat copy exception.
						}
					}
					string fwInstallDir = DirectoryFinder.FWCodeDirectory;
					foreach (XmlNode dllNode in managerNode.SelectNodes("dlls/file"))
					{
						string filename = dllNode.Attributes["name"].Value;
						string dllPathname = Path.Combine(fwInstallDir, filename);
						try
						{
							File.Copy(
								Path.Combine(srcDir, filename),
								dllPathname,
								true);
							File.SetAttributes(dllPathname, FileAttributes.Normal);
						}
						catch
						{
							// Eat copy exception.
						}
					}
				}
				// Really do the uninstall now.
				foreach (XmlDocument managerDoc in pluginsToUninstall)
				{
					XmlNode managerNode = managerDoc.SelectSingleNode("/manager");
					string shutdownMsg = XmlUtils.GetOptionalAttributeValue(managerNode, "shutdown");
					if (!String.IsNullOrEmpty(shutdownMsg))
						m_mediator.SendMessage(shutdownMsg, null);
					XmlNode configfilesNode = managerNode.SelectSingleNode("configfiles");
					string extensionPath = Path.Combine(baseExtensionPath, configfilesNode.Attributes["targetdir"].Value);
					Directory.Delete(extensionPath, true);
					// Leave any dlls in place since they may be shared, or in use for the moment.
				}
			}

			//The writing system the user selects for the user interface may not be loaded yet into the project
			//database. Therefore we need to check this first and if it is not we need to load it.
			if (m_sUserWs != m_sNewUserWs)
			{
				CreateWsIfNeeded(m_sNewUserWs);
				m_cache.LanguageWritingSystemFactoryAccessor.UserWs =
					m_cache.LanguageWritingSystemFactoryAccessor.GetWsFromStr(m_sNewUserWs);

			}
			DialogResult = DialogResult.OK;
		}

		/// <summary>
		/// If the user selected UI language has not been added to the project database yet, then
		/// add it. Otherwise cache.LanguageWritingSystemFactoryAccessor.UserWs will be set
		/// to English('en' the default value).
		/// </summary>
		/// <param name="m_sNewUserWs">User Interface writing system the user selected</param>
		private void CreateWsIfNeeded(string sNewUserWs)
		{
			int hvoNewUIws = m_cache.LanguageWritingSystemFactoryAccessor.GetWsFromStr(sNewUserWs);

			//The writing system the user has selected is not in the project database yet.
			//Therefore load the Ws XML file from C:\...\DistFiles\Languages.
			//If the XML file is already there we do not want to overwrite it with the template
			//so that user modification will not be over written.
			if (hvoNewUIws == 0)
			{
				bool fFoundXML = AddWsFromXML(sNewUserWs);
				if (!fFoundXML) //The XML for the Ws was not found so copy the template file for it
					//and then add it.
				{
					//copy the XML file from the Template directory to the Languages directory
					string source = String.Format("{0}\\{1}.xml", DirectoryFinder.TemplateDirectory, sNewUserWs);
					string destination = String.Format("{0}\\{1}.xml", DirectoryFinder.LanguagesDirectory, sNewUserWs);
					if (File.Exists(source))
					{
						File.Copy(source, destination);
						File.SetAttributes(destination, FileAttributes.Normal);
						fFoundXML = AddWsFromXML(sNewUserWs);
					}
				}
			}
		}

		private bool AddWsFromXML(string sNewUserWs)
		{
			List<NamedWritingSystem> al = new List<NamedWritingSystem>(m_cache.LangProject.GetAllNamedWritingSystems().ToArray());
			bool fFoundXML = false;
			foreach (NamedWritingSystem namedWs in al)
			{
				if (namedWs.IcuLocale.Equals(sNewUserWs))
				{
					//This creates the WS object in the database from XML.
					namedWs.GetLgWritingSystem(m_cache);
					fFoundXML = true;
					break;
				}
			}
			return fFoundXML;
		}

		private void m_btnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		private void m_btnHelp_Click(object sender, EventArgs e)
		{
			// TODO: Implement.
			ShowHelp.ShowHelpTopic(FwApp.App, s_helpTopic);
		}

		#region IFwExtension Members

		void IFwExtension.Init(SIL.FieldWorks.FDO.FdoCache cache, XCore.Mediator mediator)
		{
			m_mediator = mediator;
			m_cache = cache;
			m_sUserWs = SIL.FieldWorks.Common.Framework.FwApp.UserWs;
			m_sNewUserWs = m_sUserWs;
			m_userInterfaceChooser.Init(m_sUserWs);
			//PopulateLanguagesCombo();

			// Populate Plugins tab page list.
			string baseConfigPath = DirectoryFinder.GetFWCodeSubDirectory(@"Language Explorer\Configuration");
			string basePluginPath = Path.Combine(baseConfigPath, "Available Plugins");
			// The extension XML files should be stored in the data area, not in the code area.
			// This reduces the need for users to have administrative privileges.
			string baseExtensionPath = Path.Combine(DirectoryFinder.FWDataDirectory, @"Language Explorer\Configuration");
			foreach (string dir in Directory.GetDirectories(basePluginPath))
			{
				Debug.WriteLine(dir);
				string managerPath = Path.Combine(dir, "ExtensionManager.xml");
				if (File.Exists(managerPath))
				{
					XmlDocument managerDoc = new XmlDocument();
					managerDoc.Load(managerPath);
					XmlNode managerNode = managerDoc.SelectSingleNode("/manager");
					m_lvPlugins.SuspendLayout();
					ListViewItem lvi = new ListViewItem();
					lvi.Tag = managerDoc;
					lvi.Text = managerNode.Attributes["name"].Value;
					lvi.SubItems.Add(managerNode.Attributes["description"].Value);
					// See if it is installed and check the lvi if it is.
					XmlNode configfilesNode = managerNode.SelectSingleNode("configfiles");
					string extensionPath = Path.Combine(baseExtensionPath, configfilesNode.Attributes["targetdir"].Value);
					lvi.Checked = Directory.Exists(extensionPath);
					m_plugins.Add(lvi.Text, lvi.Checked); // Remember original installed state.
					m_lvPlugins.Items.Add(lvi);
					m_lvPlugins.ResumeLayout();
				}
			}
		}

		#endregion

		public string NewUserWs
		{
			get { return m_sNewUserWs; }
		}

		public bool PluginsUpdated
		{
			get { return m_pluginsUpdated; }
		}
	}
}
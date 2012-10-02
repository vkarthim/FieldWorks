# AppCore object modules

LINK_LIBS=xmlparse.lib Version.lib Htmlhelp.lib atl.lib winmm.lib mstask.lib $(LINK_LIBS)

OBJ_AFCORE=\
	$(INT_DIR)\autopch\OrientationManager.obj\
	$(INT_DIR)\autopch\AfMainWnd.obj\
	$(INT_DIR)\autopch\RecMainWndSupportWnds.obj\
	$(INT_DIR)\autopch\AfBars.obj\
	$(INT_DIR)\autopch\AfCmd.obj\
	$(INT_DIR)\autopch\AfUtil.obj\
	$(INT_DIR)\autopch\AfDbInfo.obj\
	$(INT_DIR)\autopch\AfApp.obj\
	$(INT_DIR)\autopch\AfWnd.obj\
	$(INT_DIR)\autopch\AfHeaderWnd.obj\
	$(INT_DIR)\autopch\AfMenuMgr.obj\
	$(INT_DIR)\autopch\AfGfx.obj\
	$(INT_DIR)\autopch\AfDialog.obj\
	$(INT_DIR)\autopch\AfContextHelp.obj\
	$(INT_DIR)\autopch\AfSplitter.obj\
	$(INT_DIR)\autopch\VwBaseDataAccess.obj\
	$(INT_DIR)\autopch\VwCacheDa.obj\
	$(INT_DIR)\autopch\VwOleDbDa.obj\
	$(INT_DIR)\autopch\DelObjUndoAction.obj\
	$(INT_DIR)\autopch\VwUndo.obj\
	$(INT_DIR)\autopch\CustViewDa.obj\
	$(INT_DIR)\autopch\AfStyleSheet.obj\
	$(INT_DIR)\autopch\DbStringCrawler.obj\
	$(INT_DIR)\autopch\FwDbChangeOverlayTags.obj\
	$(INT_DIR)\autopch\FilPgSetDlg.obj\
	$(INT_DIR)\autopch\TlsListsDlg.obj\
	$(INT_DIR)\autopch\TlsStatsDlg.obj\
	$(INT_DIR)\autopch\MiscDlgs.obj\
	$(INT_DIR)\autopch\FmtBulNumDlg.obj\
	$(INT_DIR)\autopch\AfStylesDlg.obj\
	$(INT_DIR)\autopch\FmtParaDlg.obj\
	$(INT_DIR)\autopch\FmtBdrDlg.obj\
	$(INT_DIR)\autopch\FmtGenDlg.obj\
	$(INT_DIR)\autopch\AfColorTable.obj\
	$(INT_DIR)\autopch\UiColor.obj\
	$(INT_DIR)\autopch\FmtFntDlg.obj\
	$(INT_DIR)\autopch\AfVwWnd.obj\
	$(INT_DIR)\autopch\AfDocSplitChild.obj\
	$(INT_DIR)\autopch\AfBrowseSplitChild.obj\
	$(INT_DIR)\autopch\AfTagOverlay.obj\
	$(INT_DIR)\autopch\PossChsrDlg.obj\
	$(INT_DIR)\autopch\VwBaseVc.obj\
	$(INT_DIR)\autopch\VwCustomVc.obj\
	$(INT_DIR)\autopch\StVc.obj\
	$(INT_DIR)\autopch\AfWizardDlg.obj\
	$(INT_DIR)\autopch\FwFilterDlg.obj\
	$(INT_DIR)\autopch\AfSortMethod.obj\
	$(INT_DIR)\autopch\TlsOptDlg.obj\
	$(INT_DIR)\autopch\TlsOptView.obj\
	$(INT_DIR)\autopch\TlsOptGen.obj\
	$(INT_DIR)\autopch\TlsOptCust.obj\
	$(INT_DIR)\autopch\RecMainWnd.obj\
	$(INT_DIR)\autopch\ClientWindows.obj\
	$(INT_DIR)\autopch\IconCombo.obj\
	$(INT_DIR)\autopch\AfSysLangList.obj\
	$(INT_DIR)\autopch\FmtWrtSysDlg.obj\
	$(INT_DIR)\autopch\AfFindDlg.obj\
	$(INT_DIR)\autopch\CmDataObject.obj\
	$(INT_DIR)\autopch\AfAnimateCtrl.obj\
	$(INT_DIR)\autopch\GeneralPropDlg.obj\
	$(INT_DIR)\autopch\ListsPropDlg.obj\
	$(INT_DIR)\autopch\AfProgressDlg.obj\
	$(INT_DIR)\autopch\AfChangeWatcher.obj\
	$(INT_DIR)\autopch\AfExplicitInstantiation.obj\
	$(INT_DIR)\autopch\AfPrjNotFndDlg.obj\
	$(INT_DIR)\autopch\WriteXml.obj\
	$(INT_DIR)\autopch\TreeDragDrop.obj\
	$(INT_DIR)\autopch\AfCustomExport.obj\
	$(INT_DIR)\autopch\FwStyledText.obj\
	$(INT_DIR)\autopch\UtilSil2.obj\
	$(INT_DIR)\autopch\DlgProps.obj\

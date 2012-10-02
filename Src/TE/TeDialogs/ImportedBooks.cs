// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2007, SIL International. All Rights Reserved.
// <copyright from='2007' to='2007' company='SIL International'>
//		Copyright (c) 2007, SIL International. All Rights Reserved.
//
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
#endregion
//
// File: MergeImportedDifferences.cs
// Responsibility: TE Team
// ---------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Windows.Forms;

using SIL.FieldWorks.Common.Controls;
using SIL.FieldWorks.Common.Framework;
using SIL.FieldWorks.Common.FwUtils;
using SIL.FieldWorks.FDO;
using SIL.FieldWorks.FDO.Scripture;
using SIL.FieldWorks.Common.COMInterfaces;
using SIL.FieldWorks.Common.Utils;
using SILUBS.SharedScrUtils;
using System.Collections;
using System.Diagnostics;
using SIL.FieldWorks.FDO.Cellar;
using System.Text;

namespace SIL.FieldWorks.TE
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Dialog box for the user.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public partial class ImportedBooks : Form, IBookVersionAgent
	{
		#region Constants
		private const int kStatusCol = 2; // index of the status column in the list view
		#endregion

		#region Data members
		/// <summary>The FDO Cache</summary>
		protected FdoCache m_cache;
		/// <summary>The Scripture object</summary>
		protected Scripture m_scr;
		private FwStyleSheet m_styleSheet;
		private float m_zoomPercentageDraft;
		private float m_zoomPercentageFootnote;
		/// <summary>The saved version containing the imported books.</summary>
		protected IScrDraft m_booksImported;
		/// <summary>The version to use for originals of merged or overwritten books.</summary>
		protected IScrDraft m_backupVersion;
		private FilteredScrBooks m_bookFilter;
		private Form m_owningForm;

		List<int> m_newBooks = new List<int>();
		List<int> m_overwrittenBooks = new List<int>();
		#endregion

		#region ImportedBookStatus enum
		/// <summary>
		/// Status of book in import list.
		/// </summary>
		public enum ImportedBookStatus
		{
			/// <summary>Book has been overwritten with imported book</summary>
			Overwritten,
			/// <summary>Imported book is new (no corresponding current book).</summary>
			New,
			/// <summary>Imported book contained new non-conflicting sections that have been
			/// automatically merged.</summary>
			AutoMerged,
		}
		#endregion

		#region Contructors
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="ImportedBooks"/> class.
		/// </summary>
		/// <param name="cache">The cache.</param>
		/// <param name="booksImported">The books that have been imported.</param>
		/// <param name="backupVersion">where to store stuff overwritten or merged.</param>
		/// ------------------------------------------------------------------------------------
		protected ImportedBooks(FdoCache cache, IScrDraft booksImported, IScrDraft backupVersion)
		{
			InitializeComponent();

			m_cache = cache;
			m_scr = m_cache.LangProject.TranslatedScriptureOA as Scripture;
			m_backupVersion = backupVersion;
			m_booksImported = booksImported;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="ImportedBooks"/> class.
		/// </summary>
		/// <param name="cache">The cache.</param>
		/// <param name="styleSheet">The style sheet.</param>
		/// <param name="booksImported">The books that have been imported.</param>
		/// <param name="zoomPercentageDraft">The zoom percentage of the draft view.</param>
		/// <param name="zoomPercentageFootnote">The zoom percentage of the footnote view.</param>
		/// <param name="backupVersion">where to store stuff overwritten or merged.</param>
		/// <param name="bookFilter">bookFilter to which to add new books</param>
		/// ------------------------------------------------------------------------------------
		public ImportedBooks(FdoCache cache, FwStyleSheet styleSheet,
			IScrDraft booksImported, float zoomPercentageDraft, float zoomPercentageFootnote,
			IScrDraft backupVersion, FilteredScrBooks bookFilter) :
			this(cache, booksImported, backupVersion)
		{
			m_styleSheet = styleSheet;
			m_zoomPercentageDraft = zoomPercentageDraft;
			m_zoomPercentageFootnote = zoomPercentageFootnote;
			m_bookFilter = bookFilter;

			foreach (IScrBook rev in booksImported.BooksOS)
			{
				ListViewItem item = new ListViewItem(
					new string[] {rev.Name.UserDefaultWritingSystem, GetBookInfo(rev)});
				item.SubItems.Add(DlgResources.ResourceString("kstidUnknown"));

				IScrBook curBook = m_scr.FindBook(rev.CanonicalNum);
				if (curBook == null)
				{
					SetItemStatus(item, ImportedBookStatus.New);
					int newBookHvo = m_scr.CopyBookToCurrent(rev);
					m_newBooks.Add(newBookHvo);
				}
				else
					item.Tag = new BookMerger(m_cache, styleSheet, rev);
				lstImportedBooks.Items.Add(item);
			}
			lstImportedBooks.Items[0].Selected = true;
		}
		#endregion

		#region Event handlers
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.VisibleChanged"></see> event.
		/// </summary>
		/// <param name="e">The <see cref="T:System.EventArgs"></see> that contains the event
		/// data.</param>
		/// ------------------------------------------------------------------------------------
		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			if (Visible && lstImportedBooks.Items.Count > 0)
				lstImportedBooks.Items[0].Selected = true;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"></see> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event
		/// data.</param>
		/// ------------------------------------------------------------------------------------
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			UpdateDifferenceCounts();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles the SelectedIndexChanged event of the lstBooksToMerge control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event
		/// data.</param>
		/// ------------------------------------------------------------------------------------
		private void lstBooksToMerge_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateButtonStatus(CurrentBookMerger);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Update the status of the Compare and Overwrite buttons on the dialog.
		/// </summary>
		/// <param name="bookMerger"></param>
		/// ------------------------------------------------------------------------------------
		private void UpdateButtonStatus(BookMerger bookMerger)
		{
			btnCompare.Enabled = (bookMerger != null && bookMerger.BookCurr != null
				&& bookMerger.NumberOfDifferences > 0);
			btnOverwrite.Enabled = (bookMerger != null && bookMerger.BookCurr != null);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles the Click event of the btnCompare control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event
		/// data.</param>
		/// ------------------------------------------------------------------------------------
		private void btnCompare_Click(object sender, EventArgs e)
		{
			BookMerger bookMerger = CurrentBookMerger;
			if (bookMerger == null || bookMerger.BookCurr == null || bookMerger.NumberOfDifferences == 0)
				return;

			MakeBackupIfNeeded(bookMerger);

			using (DiffDialog dlg = new DiffDialog(bookMerger, m_cache, m_styleSheet,
				m_zoomPercentageDraft, m_zoomPercentageFootnote, false))
			{
				dlg.ShowDialog();
			}
			UpdateDiffCount(lstImportedBooks.SelectedItems[0], bookMerger);
			lstBooksToMerge_SelectedIndexChanged(null, null);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Makes a backup (saved version) of the current version of the book in the given
		/// BookMerger if we don't already have it backed up.
		/// </summary>
		/// <remarks>This is called when the user clicks the Compare and Merge button as well as
		/// before auto-merging.</remarks>
		/// <param name="bookMerger">The book merger.</param>
		/// ------------------------------------------------------------------------------------
		public void MakeBackupIfNeeded(BookMerger bookMerger)
		{
			// Copy original to backup
			using (WaitCursor wc = new WaitCursor(this))
			{
				if (NoExistingBackup(bookMerger.BookCurr))
					m_scr.AddBookToSavedVersion(m_backupVersion, bookMerger.BookCurr);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles the Click event of the btnOverwrite control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		/// ------------------------------------------------------------------------------------
		protected void btnOverwrite_Click(object sender, EventArgs e)
		{
			foreach (ListViewItem item in SelectedBookItems)
			{
				BookMerger bookMerger = item.Tag as BookMerger;
				if (bookMerger == null)
					continue;

				ScrBook currentBook = GetBookCur(bookMerger);
				string sDetails;
				List<IScrSection> sectionsToRemove;
				List<int> missingBtWs;
				OverwriteType typeOfOverwrite = DetermineOverwritability(bookMerger,
					out sDetails, out sectionsToRemove, out missingBtWs);

				if (bookMerger.OriginalNumberOfDifferences != bookMerger.NumberOfDifferences)
				{
					if (!ConfirmOverwriteOfMergedVersion(currentBook.BestUIName))
						continue;
				}

				if (typeOfOverwrite == OverwriteType.DataLoss)
				{
					// There would be data loss if the user overwrites, so inform them that overwrite
					// is not possible and go on to the next book.
					ReportDataLoss((ScrBook)currentBook, ScrDraftType.ImportedVersion, this, sDetails);
					continue;
				}

				if (missingBtWs == null || ConfirmBtOverwrite(currentBook, ScrDraftType.ImportedVersion,
					sectionsToRemove, missingBtWs, this))
				{
					// Either no back translations might be lost, or the user has confirmed that they
					// want to overwrite them.
					if (typeOfOverwrite == OverwriteType.Partial)
					{
						PartialOverwrite(bookMerger, sectionsToRemove);
					}
					else
					{
						m_newBooks.Add(OverwriteBook(bookMerger));
						m_overwrittenBooks.Add(currentBook.Hvo); // this is the original current book
					}

					SetItemStatus(item, ImportedBookStatus.Overwritten);
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Determines the overwritability.
		/// </summary>
		/// <param name="merger">The merger.</param>
		/// <param name="sDetails">The details of what would be lost if the user overwrites.</param>
		/// <param name="sectionsToRemove">The sections to remove.</param>
		/// <param name="missingBtWs">The back translation writing systems in use
		/// in the Current book, but missing in the saved version.</param>
		/// <returns>The type of overwrite.</returns>
		/// ------------------------------------------------------------------------------------
		protected virtual OverwriteType DetermineOverwritability(BookMerger merger,
			out string sDetails, out List<IScrSection> sectionsToRemove, out List<int> missingBtWs)
		{
			return GetBookCur(merger).DetermineOverwritability(
			  GetBookRev(merger), out sDetails, out sectionsToRemove, out missingBtWs);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the item status.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void SetItemStatus(ListViewItem item, ImportedBookStatus status)
		{
			string label;
			switch (status)
			{
				case ImportedBookStatus.Overwritten:
					label = TeResourceHelper.GetResourceString("kstidOverwrittenFull");
					break;
				case ImportedBookStatus.New:
					label = TeResourceHelper.GetResourceString("kstidNewBook");
					break;
				case ImportedBookStatus.AutoMerged:
					label = TeResourceHelper.GetResourceString("kstidAutoMerged");
					break;
				default:
					throw new ArgumentException("Invalid value", "status");
			}

			item.Tag = status;
			item.SubItems[kStatusCol].Text = label;
			if (item.Selected)
				btnCompare.Enabled = btnOverwrite.Enabled = false;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles the Click event of the btnHelp control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event
		/// data.</param>
		/// ------------------------------------------------------------------------------------
		private void btnHelp_Click(object sender, EventArgs e)
		{
			ShowHelp.ShowHelpTopic(FwApp.App, "khtpImportedBooks");
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void btnClose_Click(object sender, EventArgs e)
		{
			Close();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// If all of the books are "new" or auto-merged, then don't actually show the dialog.
		/// See TE-7072 and TE-2126. Otherwise, show the dialog and process normally.
		/// </summary>
		/// <param name="wnd"></param>
		/// ------------------------------------------------------------------------------------
		public void ShowOrSave(IWin32Window wnd)
		{
			int cNewOrAutoMerged = 0;
			m_owningForm = wnd as Form;

			foreach (ListViewItem item in lstImportedBooks.Items)
			{
				BookMerger bookMerger = item.Tag as BookMerger;
				if (bookMerger == null)
				{
					++cNewOrAutoMerged;
					continue;
				}

				using (ProgressDialogWithTask progressDlg = new ProgressDialogWithTask(m_owningForm))
				{
					bookMerger.AttemptAutoMerge = true;
					bookMerger.BookVersionAgent = this;
					bookMerger.UseFilteredDiffList();
					progressDlg.Title = DlgResources.ResourceString("kstidCompareCaption");
					Debug.Assert(bookMerger.BookRev != null);
					progressDlg.Message = string.Format(
						DlgResources.ResourceString("kstidMergeProgress"), bookMerger.BookRev.BestUIName);
#if DEBUG_SINGLE_THREADED
					progressDlg.RunTask_DebuggingOnly(true, new BackgroundTaskInvoker(
						bookMerger.DetectDifferences));
#else
					progressDlg.RunTask(true, new BackgroundTaskInvoker(bookMerger.DetectDifferences));
#endif
				}
				if (bookMerger.AutoMerged)
				{
					SetItemStatus(item, ImportedBookStatus.AutoMerged);
					cNewOrAutoMerged++;
				}
			}

			if (cNewOrAutoMerged < lstImportedBooks.Items.Count)
				ShowDialog(wnd);
			else
				OnClosed(null);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// When dialog closes, finish up the pending stuff.
		/// </summary>
		/// <param name="e">The <see cref="T:System.EventArgs"/> that contains the event data.</param>
		/// ------------------------------------------------------------------------------------
		protected override void OnClosed(EventArgs e)
		{
			using (new WaitCursor(this))
			{
				// Enhance JohnT: Normally we would make sure all these changes were a single Undo action,
				// but for now we know this is part of a still larger Undoable task, the whole import.
				// CollapseToMark is used to convert them all into one.

				// Insert any new books into bookfilter that need to be visible
				m_bookFilter.Add(m_newBooks.ToArray());
				if (m_cache.ActionHandlerAccessor != null)
				{
					UndoChangeFilter undoItem = new UndoChangeFilter(m_bookFilter, m_newBooks, m_overwrittenBooks);
					m_cache.ActionHandlerAccessor.AddAction(undoItem);
				}
				// Delete empty revisions created for this import.
				//if (m_booksImported.BooksOS.Count == 0)
				//    m_booksImported.DeleteUnderlyingObject();
				if (m_backupVersion.BooksOS.Count == 0)
					m_backupVersion.DeleteUnderlyingObject();
				if (e != null)
					base.OnClosed(e);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Overwrites the current version of a book with an imported version.
		/// </summary>
		/// <param name="bookMerger">The book merger containing the current version and the
		/// imported version to replace it with.</param>
		/// ------------------------------------------------------------------------------------
		private int OverwriteBook(BookMerger bookMerger)
		{
			return OverwriteBook(bookMerger.BookCurr, bookMerger.BookRev);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Overwrites the current version of a book with an imported version.
		/// </summary>
		/// <param name="bookCurr">The current version of the book.</param>
		/// <param name="bookImported">The imported version of the book.</param>
		/// ------------------------------------------------------------------------------------
		protected int OverwriteBook(IScrBook bookCurr, IScrBook bookImported)
		{
			if (bookCurr != null)
			{
				if (NoExistingBackup(bookCurr))
				{
					// Move existing book to backup.
					// Do NOT use bookMerger.BookCurr.OwnOrd, ownord$ is not guranteed to be contiguous.
					int sourceIndex = bookCurr.IndexInOwner;
					m_cache.MoveOwningSequence(
						m_scr.Hvo,
						(int)Scripture.ScriptureTags.kflidScriptureBooks,
						sourceIndex,
						sourceIndex,
						m_backupVersion.Hvo,
						(int)ScrDraft.ScrDraftTags.kflidBooks,
						m_backupVersion.BooksOS.Count); // TODO (TE-7207) This looks dubious. Is the list always in canonical order. Should we count on that?
				}
				else
				{
					// We already have saved the current version before we did a merge. Now the
					// user has decided that he wants to overwrite instead, so we blow away
					// the merged version before we copy the imported book to current (TE-7214).
					m_cache.DeleteObject(bookCurr.Hvo);
				}
			}
			return m_scr.CopyBookToCurrent(bookImported);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Update the difference counts for all imported books.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void UpdateDifferenceCounts()
		{
			foreach (ListViewItem item in lstImportedBooks.Items)
			{
				BookMerger bookMerger = item.Tag as BookMerger;
				if (bookMerger != null)
				{
					UpdateDiffCount(item, bookMerger);
					UpdateButtonStatus(bookMerger);
				}
			}
		}
		#endregion

		#region Internal static methods
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Reports potential data loss to the user if they could overwrite the book.
		/// </summary>
		/// <param name="originalBook">The original book.</param>
		/// <param name="scrDraftType">whether the draft is from a saved version or an imported
		/// version.</param>
		/// <param name="owner">The window's owner.</param>
		/// <param name="sDetails">The details about which verses would be deleted with an
		/// overwrite.</param>
		/// ------------------------------------------------------------------------------------
		internal static void ReportDataLoss(ScrBook originalBook, ScrDraftType scrDraftType,
			IWin32Window owner, string sDetails)
		{
			string sType = (scrDraftType == ScrDraftType.ImportedVersion) ?
				TeResourceHelper.GetResourceString("kstidImported") :
				TeResourceHelper.GetResourceString("kstidSaved");
			string sMsg = TeResourceHelper.FormatResourceString("kstidDataLossMsg",
				sType, originalBook.BestUIName, sDetails);
			MessageBox.Show(owner, sMsg,
				TeResourceHelper.GetResourceString("kstidDataLossCaption"),
				MessageBoxButtons.OK, MessageBoxIcon.None,
				MessageBoxDefaultButton.Button1);
		}

		/// --------------------------------------------------------------------------------
		/// <summary>
		/// Confirms whether the back translations in the current book should be overwritten.
		/// </summary>
		/// <param name="originalBook">The original book.</param>
		/// <param name="scrDraftType">whether the draft is from a saved version or an imported
		/// version.</param>
		/// <param name="sectionsToRemove">sections that will need to be removed from this book
		/// before a partial overwrite can be executed (will be null unless return type is
		/// Partial).</param>
		/// <param name="missingBtWs">list of back translation writing systems that are used in
		/// this book but not in the savedVersion</param>
		/// <param name="owner">The window's owner.</param>
		/// <returns>
		/// 	<c>true</c> if the user wants to proceed with book overwrite;
		/// <c>false</c> otherwise
		/// </returns>
		/// --------------------------------------------------------------------------------
		internal static bool ConfirmBtOverwrite(ScrBook originalBook, ScrDraftType scrDraftType,
			List<IScrSection> sectionsToRemove,	List<int> missingBtWs, IWin32Window owner)
		{
			string sType = (scrDraftType == ScrDraftType.ImportedVersion) ?
				TeResourceHelper.GetResourceString("kstidImported") :
				TeResourceHelper.GetResourceString("kstidSaved");

			// Get the list of language name(s) used for back translations in the current book,
			// but not in the revision.
			string sLanguages = GetLanguageNames(originalBook.Cache, missingBtWs);

			// Get the Scripture data (intro and/or verses) that will be replaced.
			// When sectionsToRemove is null, this indicates that we want a full book overwrite.
			// In this case, we report all sections in the current book.
			string sReferences = GetScriptureReferences((sectionsToRemove != null) ?
				sectionsToRemove : new List<IScrSection>(originalBook.SectionsOS));

			string sMsg = TeResourceHelper.FormatResourceString("kstidConfirmOverwriteBackTrans",
				sType, sLanguages, originalBook.Name.UserDefaultWritingSystem, sReferences);

			return (MessageBox.Show(owner, sMsg,
				TeResourceHelper.GetResourceString("kstidConfirmOverwriteBackTransCaption"),
				MessageBoxButtons.YesNo, MessageBoxIcon.None,
				MessageBoxDefaultButton.Button2) == DialogResult.Yes);
		}
		#endregion

		#region Private helpers
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the scripture references from the given list of Scripture sections separated
		/// by new lines.
		/// </summary>
		/// <param name="sections">The sections from which we retrieve Scripture reference
		/// information.</param>
		/// ------------------------------------------------------------------------------------
		private static string GetScriptureReferences(List<IScrSection> sections)
		{
			StringBuilder strBldr = new StringBuilder();
			int iSection = 0;
			IScrSection section;
			while (iSection < sections.Count)
			{
				section = sections[iSection];
				// Add string for intro section(s)
				if (section.IsIntro)
				{
					if (strBldr.ToString().IndexOf("Intro") < 0)
					{
						// only add "Intro" for first intro section
						strBldr.Append("   Intro" + Environment.NewLine);
					}
					iSection++;
					continue;
				}

				BCVRef firstRef;
				BCVRef lastRef;
				iSection = GetReferenceRange(sections, iSection, out firstRef, out lastRef);
				strBldr.Append("   " + ReferenceStr(firstRef) + "-" +
					ReferenceStr(lastRef, firstRef.Chapter != lastRef.Chapter) + Environment.NewLine);
			}

			return strBldr.ToString();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the contiguous reference range for the sections beginning at iSection.
		/// </summary>
		/// <param name="sections">The sections.</param>
		/// <param name="iSection">The current index into the sections.</param>
		/// <param name="firstRef">The first reference.</param>
		/// <param name="lastRef">The last reference of the contiguous sections.</param>
		/// <returns>Index to the section after the lastRef.</returns>
		/// ------------------------------------------------------------------------------------
		private static int GetReferenceRange(List<IScrSection> sections, int iSection,
			out BCVRef firstRef, out BCVRef lastRef)
		{
			firstRef = new BCVRef(sections[iSection].VerseRefStart);

			if (iSection == sections.Count - 1)
			{
				// We're on the last section, so the end reference is in this section.
				lastRef = new BCVRef(sections[iSection].VerseRefEnd);
				return ++iSection;
			}

			Paratext.ScrVers versification =
				sections[iSection].Cache.LangProject.TranslatedScriptureOA.Versification;
			ScrBook book = new ScrBook(sections[iSection].Cache, sections[iSection].OwnerHVO);

			while (iSection < sections.Count - 1)
			{
				// if the ending reference of the current section is not contiguous with the
				// start of the next section, we've found the last contiguous section.
				BCVRef endRefOfSection = new BCVRef(sections[iSection].VerseRefEnd);
				BCVRef startRefOfNextSection = new BCVRef(sections[iSection + 1].VerseRefStart);
				if (RefHasGap(endRefOfSection, startRefOfNextSection, book.CanonicalNum, versification))
				{
					lastRef = new BCVRef(sections[iSection].VerseRefEnd);
					return iSection + 1;
				}
				iSection++;
			}

			// We came to the last section, so set the ending verse to its end.
			lastRef = new BCVRef(sections[iSection].VerseRefEnd);
			return iSection + 1;
		}

		/// --------------------------------------------------------------------------------
		/// <summary>
		/// Determines whether the ending reference of the previous section and the starting
		/// reference of the following section are contiguous.
		/// </summary>
		/// <param name="endRefOfSection">The ending reference of section.</param>
		/// <param name="startRefOfNextSection">The starting reference of following section.</param>
		/// <param name="bookId">The canonical number for the current book.</param>
		/// <param name="versification">The versification.</param>
		/// <returns>
		/// 	<c>true</c> if the sections do not have contiguous references, <c>false</c>
		/// otherwise
		/// </returns>
		/// --------------------------------------------------------------------------------
		private static bool RefHasGap(BCVRef endRefOfSection, BCVRef startRefOfNextSection,
			int bookId, Paratext.ScrVers versification)
		{
			if (endRefOfSection.Chapter == startRefOfNextSection.Chapter)
			{
				// For references in the same chapter, determine whether the starting verse
				// in the next section is the same or just one more than the end of the
				// current section.
				return (endRefOfSection.Verse + 1) < startRefOfNextSection.Verse;
			}

			VersificationTable verseTable = VersificationTable.Get(versification);
			if ((endRefOfSection.Chapter + 1) == startRefOfNextSection.Chapter)
			{
				if (endRefOfSection.Verse != verseTable.LastVerse(bookId, endRefOfSection.Verse) ||
					startRefOfNextSection.Verse == 1)
				{
					// The current section's last verse is the end of the chapter and the
					// next section starts with the first verse of the next chapter.
					return false;
				}
			}

			return true;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the language names (separated by newlines if more than one) given a list of
		/// writing systems.
		/// </summary>
		/// <param name="cache">The cache.</param>
		/// <param name="wsList">The list of writing systems.</param>
		/// ------------------------------------------------------------------------------------
		private static string GetLanguageNames(FdoCache cache, List<int> wsList)
		{
			Debug.Assert(wsList != null && wsList.Count > 0);

			StringBuilder strBldr = new StringBuilder();
			foreach (int wsHvo in wsList)
			{
				ILgWritingSystem lgws;
				try
				{
					lgws = new LgWritingSystem(cache, wsHvo);
					strBldr.Append("   " + lgws.Name.UserDefaultWritingSystem + Environment.NewLine);
				}
				catch
				{
					// invalid writing system
				}
			}
			return strBldr.ToString();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the book merger for the currently selected item.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private BookMerger CurrentBookMerger
		{
			get
			{
				if (lstImportedBooks.SelectedItems.Count == 0)
					return null;

				ListViewItem item = lstImportedBooks.SelectedItems[0];

				return (item.Tag is BookMerger) ? (BookMerger)item.Tag : null;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Create a book name.
		/// </summary>
		/// <param name="book">specified book</param>
		/// <returns>If the specified book is a full book, only the name of the book will be returned.
		/// Otherwise, the name of the book followed by its first and last reference in parentheses
		/// will be returned.</returns>
		/// ------------------------------------------------------------------------------------
		internal static string GetBookInfo(IScrBook book)
		{
			// Determine the first and last reference and whether the book has an introduction
			BCVRef verseMin;
			BCVRef verseMax;
			bool fHasIntro;
			GetBookMinMaxVerse(book, out verseMin, out verseMax, out fHasIntro);

			if (fHasIntro)
			{
				return (verseMin == null) ? TeResourceHelper.GetResourceString("kstidIntroOnly") :
					ReferenceRangeStr(verseMin, verseMax) + " " +
					TeResourceHelper.GetResourceString("kstidIntro");
			}
			else if (verseMin != null)
				return ReferenceRangeStr(verseMin, verseMax);
			else
			{
				// A book with no sections should never happen (but unfortunately has). This
				// text ("no sections") is used in this case to prevent a crash, but is not localized.
				string bookName = !string.IsNullOrEmpty(book.BestUIName) ? book.BestUIName :
					"book";
				Debug.Fail("No introduction or Scripture sections in " + bookName);
				return "(no sections)";
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Create a reference range string for the specified beginning and ending verse numbers.
		/// </summary>
		/// <param name="verseMin">beginning verse number</param>
		/// <param name="verseMax">ending verse number</param>
		/// <returns>Chapter/Verse range in this format: CC:VV-CC:VV if the beginning and
		/// ending chapters are different OR CC:VV-VV if the chapters are the same</returns>
		/// ------------------------------------------------------------------------------------
		private static string ReferenceRangeStr(BCVRef verseMin, BCVRef verseMax)
		{
			return ReferenceStr(verseMin, true) + "-" +
				ReferenceStr(verseMax, (verseMin.Chapter != verseMax.Chapter));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the chapter:verse as a string.
		/// </summary>
		/// <param name="verse">The verse.</param>
		/// ------------------------------------------------------------------------------------
		private static string ReferenceStr(BCVRef verse)
		{
			return ReferenceStr(verse, true);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the chapter:verse as a string.
		/// </summary>
		/// <param name="verse">The verse.</param>
		/// <param name="fIncludeChapter">if set to <c>true</c> include the chapter.</param>
		/// ------------------------------------------------------------------------------------
		private static string ReferenceStr(BCVRef verse, bool fIncludeChapter)
		{
			if (fIncludeChapter)
				return verse.Chapter + ":" + verse.Verse;
			return verse.Verse.ToString();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Get the minimum and maximum reference for the book. If all the verses are present
		/// for a book or no sections are present, the verseMin and VerseMax will be null.
		/// </summary>
		/// <param name="book">book to check for verse range.</param>
		/// <param name="verseMin">out: beginning Scripture reference.</param>
		/// <param name="verseMax">out: ending Scripture reference.</param>
		/// <param name="fBookHasIntro">out: <c>true</c> if the book has an introduction section</param>
		/// ------------------------------------------------------------------------------------
		private static void GetBookMinMaxVerse(IScrBook book, out BCVRef verseMin,
			out BCVRef verseMax, out bool fBookHasIntro)
		{
			verseMin = null;
			verseMax = null;
			fBookHasIntro = false;
			if (book.SectionsOS.Count == 0)
			{
				Debug.Fail("Book has zero sections.");
				return;
			}

			// Determine if the first sentence has an introduction
			fBookHasIntro = book.SectionsOS[0].IsIntro;

			// Get the beginning Scripture reference in the book.
			for (int iSection = 0; iSection < book.SectionsOS.Count; iSection++)
			{
				if (!book.SectionsOS[iSection].IsIntro)
				{
					verseMin = book.SectionsOS[iSection].VerseRefMin;
					break;
				}
			}

			// Get the beginning Scripture reference in the book.
			for (int iSection = book.SectionsOS.Count - 1; iSection >= 0; iSection--)
			{
				if (!book.SectionsOS[iSection].IsIntro)
				{
					verseMax = book.SectionsOS[iSection].VerseRefMax;
					break;
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Updates the diff count.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="bookMerger">The book merger.</param>
		/// ------------------------------------------------------------------------------------
		private void UpdateDiffCount(ListViewItem item, BookMerger bookMerger)
		{
			int diffCountOrig = bookMerger.OriginalNumberOfDifferences;
			int diffCount = bookMerger.NumberOfDifferences;
			if (diffCount == 0)
			{
				item.SubItems[kStatusCol].Text = TeResourceHelper.GetResourceString(
					(diffCountOrig == 0) ? "kstidIdentical" : "kstidNoDifferencesLeft");
				item.ImageIndex = 0;
			}
			else
			{
				string stid = diffCount == 1 ? "kstidOne{0}Difference" : "kstid{0}Differences";
				stid = String.Format(stid, (diffCount == diffCountOrig) ? "Initial" : "Remaining");
				item.SubItems[kStatusCol].Text = TeResourceHelper.GetResourceString(stid);
				if (diffCount > 1)
					item.SubItems[kStatusCol].Text = String.Format(item.SubItems[kStatusCol].Text, diffCount);
				item.ImageIndex = -1;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Determines whether there is an existing backup for the specified book.
		/// </summary>
		/// <param name="book">The current book.</param>
		/// <returns><c>true</c> if there is no existing backup of the specified book
		/// </returns>
		/// ------------------------------------------------------------------------------------
		private bool NoExistingBackup(IScrBook book)
		{
			return m_backupVersion.FindBook(book.CanonicalNum) == null;
		}
		#endregion

		#region Protected stuff to facilitate testing
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the selected listview items. This is needed to allow override for testing
		/// because lstBooksToMerge.SelectedItems doesn't contain any items unless the window
		/// is actually displayed.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected virtual IEnumerable SelectedBookItems
		{
			get { return lstImportedBooks.SelectedItems; }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Confirms the overwrite of merged version.
		/// </summary>
		/// <returns><c>true</c> if the overwrite was cleared for take-off</returns>
		/// ------------------------------------------------------------------------------------
		protected virtual bool ConfirmOverwriteOfMergedVersion(string bookName)
		{
			return (MessageBox.Show(this,
				TeResourceHelper.FormatResourceString("kstidConfirmBookOverwriteAfterMerge", bookName),
				TeResourceHelper.GetResourceString("kstidConfirmBookOverwriteAfterMergeCaption"),
				MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button2) ==
				DialogResult.Yes);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Performs a partial overwrite of the book using the imported portion of the book.
		/// </summary>
		/// <param name="bookMerger">The book merger.</param>
		/// <param name="sectionsToRemove">The sections to remove as the first step (before
		/// calling the auto-merge code).</param>
		/// ------------------------------------------------------------------------------------
		protected virtual void PartialOverwrite(BookMerger bookMerger,
			List<IScrSection> sectionsToRemove)
		{
			using (ProgressDialogWithTask progress = new ProgressDialogWithTask(this))
			{
				progress.Title = DlgResources.ResourceString("kstidOverwriteCaption");
				progress.RunTask(true, new BackgroundTaskInvoker(bookMerger.DoPartialOverwrite),
					sectionsToRemove);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the current book from the BookMerger.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected virtual ScrBook GetBookCur(BookMerger merger)
		{
			return (ScrBook)merger.BookCurr;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the revision book from the BookMerger.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected virtual ScrBook GetBookRev(BookMerger merger)
		{
			return (ScrBook)merger.BookRev;
		}
		#endregion
	}

	#region class UndoChangeFilter
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Implements Undo to remove added books from filter.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	internal class UndoChangeFilter : IUndoAction
	{
		private FilteredScrBooks m_filter;
		List<int> m_newBooks;
		List<int> m_overwrittenBooksToRestore;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Make one.
		/// </summary>
		/// <param name="filter">The filter.</param>
		/// <param name="newBooks">The new books.</param>
		/// ------------------------------------------------------------------------------------
		public UndoChangeFilter(FilteredScrBooks filter, List<int> newBooks)
		{
			m_newBooks = newBooks;
			m_filter = filter;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Make one.
		/// </summary>
		/// <param name="filter">The filter.</param>
		/// <param name="m_newBooks">The new books.</param>
		/// <param name="overwrittenBooksToRestore">The books to put back in the filter if the
		/// user performs an undo of imported books that overwrote existing books.</param>
		/// ------------------------------------------------------------------------------------
		public UndoChangeFilter(FilteredScrBooks filter, List<int> m_newBooks,
			List<int> overwrittenBooksToRestore) : this(filter, m_newBooks)
		{
			m_overwrittenBooksToRestore = overwrittenBooksToRestore;
		}

		#region IUndoAction Members

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Irreversibly commits an action.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void Commit()
		{
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// True for most actions, which make changes to data; false for actions that represent
		/// updates to the user interface, like replacing the selection.
		/// </summary>
		/// <returns></returns>
		/// ------------------------------------------------------------------------------------
		public bool IsDataChange()
		{
			return false;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// False, because Scripture import can't be redone.
		/// </summary>
		/// <returns></returns>
		/// ------------------------------------------------------------------------------------
		public bool IsRedoable()
		{
			return false;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Reapplies (or "redoes") an action.
		/// </summary>
		/// <param name="fRefreshPending">Set to true if app will call refresh after all Undo actions are
		/// finished. This means the UndoAction doesn't have to call PropChanged.</param>
		/// <returns></returns>
		/// ------------------------------------------------------------------------------------
		public bool Redo(bool fRefreshPending)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// False, because we call PropChanged.
		/// </summary>
		/// <returns></returns>
		/// ------------------------------------------------------------------------------------
		public bool RequiresRefresh()
		{
			return false;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets whether this undo action should notify the world that the action has been undone
		/// or redone. For ISqlUndoAction, this supresses the PropChanged notifications.
		/// </summary>
		/// <value></value>
		/// <returns>A System.Boolean </returns>
		/// ------------------------------------------------------------------------------------
		public bool SuppressNotification
		{
			set { }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Reverses (or "undoes") an action. Sets pfSuccess to true if successful. If not successful
		/// because the database state has changed unexpectedly, sets pfSuccess to false but still
		/// returns S_OK. More catastrophic errors may produce error result codes.
		/// </summary>
		/// <param name="fRefreshPending">Set to true if app will call refresh after all Undo actions are
		/// finished. This means the UndoAction doesn't have to call PropChanged.</param>
		/// <returns></returns>
		/// ------------------------------------------------------------------------------------
		public bool Undo(bool fRefreshPending)
		{
			foreach (int hvo in m_newBooks)
			{
				int index = m_filter.BookHvos.IndexOf(hvo);
				if (index >= 0)
					m_filter.Remove(index);
			}

			if (m_overwrittenBooksToRestore != null && m_overwrittenBooksToRestore.Count > 0)
				m_filter.Add(m_overwrittenBooksToRestore.ToArray());

			return true;
		}
		#endregion
	}
	#endregion
}
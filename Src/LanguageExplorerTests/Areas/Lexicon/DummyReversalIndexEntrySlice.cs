// Copyright (c) 2018 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html)

using System;
using System.Windows.Forms;
using LanguageExplorer.Areas.Lexicon.Tools.Edit;
using SIL.LCModel;
using SIL.LCModel.Core.KernelInterfaces;
using SIL.LCModel.Core.Text;

namespace LanguageExplorerTests.Areas.Lexicon
{
	/// <summary>
	/// A dummy class for <see cref="ReversalIndexEntrySlice"/> for testing purposes.
	/// </summary>
	internal class DummyReversalIndexEntrySlice : ReversalIndexEntrySlice
	{
		public DummyReversalIndexEntrySlice(ICmObject obj) : base(obj)
		{
		}

		/// <summary>
		/// Allows access to the view.
		/// </summary>
		public DummyReversalIndexEntrySliceView DummyControl { get; set; }

		public override void FinishInit()
		{
			DummyReversalIndexEntrySliceView ctrl = new DummyReversalIndexEntrySliceView(MyCmObject.Hvo);
			ctrl.Cache = Cache;
			DummyControl = ctrl;

			if (ctrl.RootBox == null)
				ctrl.MakeRoot();
		}

	}

	/// <summary>
	/// A dummy class for <see cref="ReversalIndexEntrySliceView"/> for testing purposes.
	/// </summary>
	internal class DummyReversalIndexEntrySliceView : ReversalIndexEntrySliceView
	{
		public DummyReversalIndexEntrySliceView(int hvo) : base(hvo)
		{
		}

		/// <summary>
		/// Gets the number of reversal index entries for a given reversal index.
		/// </summary>
		/// <param name="hvoIndex"></param>
		public int GetIndexSize(int hvoIndex)
		{
			return m_sdaRev.get_VecSize(hvoIndex, kFlidEntries);
		}

		/// <summary>
		/// Exposes the OnKillFocus and OnLostFocus methods for testing purposes.
		/// </summary>
		/// <param name="newWindow"></param>
		public void KillFocus(Control newWindow)
		{
			OnKillFocus(newWindow, false);
			OnLostFocus(new EventArgs());
		}

		/// <summary>
		/// Helper method to create a new empty dummy reversal index entry in the dummy cache.
		/// </summary>
		public void CacheNewDummyEntry(int hvoIndex, int ws)
		{
			int count = m_sdaRev.get_VecSize(hvoIndex, kFlidEntries);
			m_sdaRev.CacheReplace(hvoIndex, kFlidEntries, count, count, new int[] {m_dummyId}, 1);
			m_sdaRev.CacheStringAlt(m_dummyId--, ReversalIndexEntryTags.kflidReversalForm, ws, TsStringUtils.EmptyString(ws));
		}

		/// <summary>
		/// Helper method to edit a TsString in the dummy cache at a given position.
		/// </summary>
		public void EditRevIndexEntryInCache(int hvoIndex, int index, int ws, ITsString tss)
		{
			int hvoEntry = m_sdaRev.get_VecItem(hvoIndex, kFlidEntries, index);
			m_sdaRev.SetMultiStringAlt(hvoEntry, ReversalIndexEntryTags.kflidReversalForm, ws, tss);
		}

	}
}

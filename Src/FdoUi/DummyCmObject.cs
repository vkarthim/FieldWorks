using System;

namespace SIL.FieldWorks.FdoUi
{
	/// <summary>
	/// A light weight object used in the merge dlg to show relevant objects.
	/// </summary>
	public class DummyCmObject
	{
		private int m_hvo;
		private string m_displayName;
		private int m_ws;

		/// <summary>
		/// Get the HVO for the dummy object.
		/// </summary>
		public int Hvo
		{
			get { return m_hvo; }
		}

		/// <summary>
		/// Get the writing system integer.
		/// </summary>
		public int WS
		{
			get { return m_ws; }
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="hvo"></param>
		/// <param name="displayName"></param>
		/// <param name="fontName"></param>
		public DummyCmObject(int hvo, string displayName, int ws)
		{
			m_hvo = hvo;
			m_displayName = displayName;
			m_ws = ws;
		}

		/// <summary>
		/// Override to show the object in the list view.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return m_displayName;
		}
	}
}

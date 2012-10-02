using System;

namespace SIL.FieldWorks.Common.Controls
{
	/// <summary>
	/// Interface for utilities.
	/// </summary>
	public interface IUtility
	{
		/// <summary>
		/// Get the main label describing the utility.
		/// </summary>
		string Label {get;}

		/// <summary>
		/// Set the UtilityDlg.
		/// </summary>
		UtilityDlg Dialog {set;}

		/// <summary>
		/// Load any items in list box.
		/// </summary>
		void LoadUtilities();

		/// <summary>
		/// Notify the utility is has been selected in the dlg.
		/// </summary>
		void OnSelection();

		/// <summary>
		/// Have the utility do what it does.
		/// </summary>
		void Process();
	}
}

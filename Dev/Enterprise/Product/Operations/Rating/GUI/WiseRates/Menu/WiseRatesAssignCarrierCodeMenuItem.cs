using System.Diagnostics.CodeAnalysis;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.GUI
{
	/// <summary>
	/// Menu for "Assign SCAC / C1C Code to Carrier" command
	/// </summary>
	[SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "menuItem is disposed by parent control")]
	public sealed class WiseRatesAssignCarrierCodeMenuItem : WiseRatesMenuItem
	{
		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		public static void Create(ZGrid gridBoundToWiseEntryViewList, IWiseRatesCommand assignCommand)
		{
			new WiseRatesAssignCarrierCodeMenuItem(gridBoundToWiseEntryViewList, assignCommand);
		}

		WiseRatesAssignCarrierCodeMenuItem(ZGrid gridBoundToWiseEntryViewList, IWiseRatesCommand assignCommand)
			: base(gridBoundToWiseEntryViewList, assignCommand)
		{
			InitializeMenuItem();
		}

		protected override MultilingualString GetMenuCaption(WiseEntryView entry = default)
		{
			var carrierCode = AssignCarrierCodeCommand.GetSCACOrC1C(entry);

			return !string.IsNullOrEmpty(carrierCode)
				? RatingConstants.WiseRatesViewContextMenu.AssignCarrierCode(carrierCode)
				: RatingConstants.WiseRatesViewContextMenu.AssignCarrierCode();
		}
	}
}

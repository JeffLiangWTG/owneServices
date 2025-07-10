using System.Diagnostics.CodeAnalysis;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.GUI
{
	/// <summary>
	/// Menu for "Assign IATA Code to Carrier" command
	/// </summary>
	[SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "menuItem is disposed by parent control")]
	sealed public class WiseRatesAssignIATACodeMenuItem : WiseRatesMenuItem
	{
		WiseRatesAssignIATACodeMenuItem(ZGrid gridBoundToWiseEntryViewList, IWiseRatesCommand assignCommand)
			: base(gridBoundToWiseEntryViewList, assignCommand)
		{
			InitializeMenuItem();
		}

		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		public static void Create(ZGrid gridBoundToWiseEntryViewList, IWiseRatesCommand assignCommand)
		{
			new WiseRatesAssignIATACodeMenuItem(gridBoundToWiseEntryViewList, assignCommand);
		}

		protected override MultilingualString GetMenuCaption(WiseEntryView entry = default)
		{
			var iataCode = CreateAirlineAndAssignIATACodeCommand.GetRateEntryIATACode(entry);

			return !string.IsNullOrEmpty(iataCode)
				? RatingConstants.WiseRatesViewContextMenu.AssignIATACode(iataCode)
				: RatingConstants.WiseRatesViewContextMenu.AssignIATACode();
		}
	}
}

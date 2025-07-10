using System.Diagnostics.CodeAnalysis;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.GUI
{
	/// <summary>
	/// Menu for "Create Airline and assign to Carrier for IATA Code" command
	/// </summary>
	[SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "menuItem is disposed by parent control")]
	public sealed class WiseRatesCreateAirlineAndAssignIATACodeMenuItem : WiseRatesMenuItem
	{
		WiseRatesCreateAirlineAndAssignIATACodeMenuItem(ZGrid gridBoundToWiseEntryViewList, IWiseRatesCommand assignCommand) : base(gridBoundToWiseEntryViewList, assignCommand)
		{
			InitializeMenuItem();
		}

		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		public static void Create(ZGrid gridBoundToWiseEntryViewList, IWiseRatesCommand assignCommand)
		{
			new WiseRatesCreateAirlineAndAssignIATACodeMenuItem(gridBoundToWiseEntryViewList, assignCommand);
		}

		protected override MultilingualString GetMenuCaption(WiseEntryView entry = default)
		{
			var iataCode = CreateAirlineAndAssignIATACodeCommand.GetRateEntryIATACode(entry);

			return !string.IsNullOrEmpty(iataCode)
				? RatingConstants.WiseRatesViewContextMenu.CreateAirlineAndAssignIATACode(iataCode)
				: RatingConstants.WiseRatesViewContextMenu.CreateAirlineAndAssignIATACode();
		}
	}
}

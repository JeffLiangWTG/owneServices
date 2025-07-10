using System.Diagnostics.CodeAnalysis;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.GUI
{
	/// <summary>
	/// Menu for "Assign Carrier Service Levels to Carrier" command
	/// </summary>
	[SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "menuItem is disposed by parent control")]
	public sealed class WiseRatesAssignCarrierServiceLevelsMenuItem : WiseRatesMenuItem
	{
		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		public static void Create(ZGrid gridBoundToWiseEntryViewList, IWiseRatesCommand assignCommand)
		{
			new WiseRatesAssignCarrierServiceLevelsMenuItem(gridBoundToWiseEntryViewList, assignCommand);
		}

		WiseRatesAssignCarrierServiceLevelsMenuItem(ZGrid gridBoundToWiseEntryViewList, IWiseRatesCommand assignCommand)
			: base(gridBoundToWiseEntryViewList, assignCommand)
		{
			InitializeMenuItem();
		}

		protected override MultilingualString GetMenuCaption(WiseEntryView entry = default)
		{
			return RatingConstants.WiseRatesViewContextMenu.AssignCarrierServiceLevel;
		}
	}
}

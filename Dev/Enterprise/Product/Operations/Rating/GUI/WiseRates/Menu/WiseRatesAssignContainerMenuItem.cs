using System.Diagnostics.CodeAnalysis;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.GUI
{
	[SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "menuItem is disposed by parent control")]
	public class WiseRatesAssignContainerMenuItem : WiseRatesMenuItem
	{
		public static void Create(ZGrid gridBoundToWiseEntryViewList, IWiseRatesCommand assignCommand)
		{
			var menuItem = new WiseRatesAssignContainerMenuItem(gridBoundToWiseEntryViewList, assignCommand);
		}

		WiseRatesAssignContainerMenuItem(ZGrid gridBoundToWiseEntryViewList, IWiseRatesCommand assignCommand)
			: base(gridBoundToWiseEntryViewList, assignCommand)
		{
			InitializeMenuItem();
		}

		protected override MultilingualString GetMenuCaption(WiseEntryView entry = default)
		{
			var (isSEARate, containerCodeOrISOType) = AssignContainerCodeCommand.GetIsSeaModeAndContainerCodeFromEntryView(entry);
			return isSEARate
				? RatingConstants.WiseRatesViewContextMenu.AssignSeaContainer(containerCodeOrISOType)
				: RatingConstants.WiseRatesViewContextMenu.CreateAirContainer(containerCodeOrISOType);
		}
	}
}

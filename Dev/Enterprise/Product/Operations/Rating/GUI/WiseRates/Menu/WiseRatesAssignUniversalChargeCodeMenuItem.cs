using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	/// <summary>
	/// Menu for "Assign Universal Charge Code to [Global] Charge Code" command.
	/// </summary>
	[SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "menuItem is disposed by parent control")]
	sealed public class WiseRatesAssignUniversalChargeCodeMenuItem : WiseRatesMenuItem
	{
		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		public static void CreateForGlobalChargeCode(ZGrid gridBoundToWiseEntryViewList, IWiseRatesCommand assignCommand)
		{
			new WiseRatesAssignUniversalChargeCodeMenuItem(gridBoundToWiseEntryViewList, toGlobal: true, assignCommand);
		}

		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		public static void CreateForLocalChargeCode(ZGrid gridBoundToWiseEntryViewList, IWiseRatesCommand assignCommand)
		{
			new WiseRatesAssignUniversalChargeCodeMenuItem(gridBoundToWiseEntryViewList, toGlobal: false, assignCommand);
		}

		WiseRatesAssignUniversalChargeCodeMenuItem(ZGrid gridBoundToWiseEntryViewList, bool toGlobal, IWiseRatesCommand assignCommand)
			: base(gridBoundToWiseEntryViewList, assignCommand)
		{
			this.toGlobal = toGlobal;
			InitializeMenuItem();
		}

		readonly bool toGlobal;

		protected override void InitializeMenuItem()
		{
			base.InitializeMenuItem();

			// Placeholder so menu shows the popup icon
			var childMenu = new ZMenuItem("...", OnMenuItemClick);
			menuItem.MenuItems.Add(childMenu);
		}

		protected override void ContextMenu_PopupCore()
		{
			var unmappedCodes = AssignUniversalChargeCodeCommand.UnmappedUniversalChargeCodes(SelectedRateEntry).ToList();
			if (unmappedCodes.Count > 0)
			{
				var childItems = menuItem.MenuItems;
				int childItemCount = childItems.Count;
				for (var i = 0; i < unmappedCodes.Count; ++i)
				{
					ZMenuItem childItem;
					if (i < childItemCount)
					{
						childItem = (ZMenuItem)childItems[i];
						childItem.Caption = (NoResString)unmappedCodes[i];
					}
					else
					{
						childItem = new ZMenuItem(unmappedCodes[i], OnMenuItemClick);
						menuItem.MenuItems.Add(childItem);
					}
					childItem.Tag = unmappedCodes[i];
				}

				while (childItemCount > unmappedCodes.Count)
				{
					--childItemCount;
					menuItem.MenuItems.RemoveAt(childItemCount);
				}
			}
		}

		protected override MultilingualString GetMenuCaption(WiseEntryView entry = null)
		{
			return toGlobal
				? RatingConstants.WiseRatesViewContextMenu.AssignGlobalChargeCode
				: RatingConstants.WiseRatesViewContextMenu.AssignLocalChargeCode;
		}
	}
}

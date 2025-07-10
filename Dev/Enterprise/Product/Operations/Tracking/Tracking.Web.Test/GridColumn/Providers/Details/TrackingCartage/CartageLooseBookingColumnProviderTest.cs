using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(CartageLooseBookingColumnProvider))]
	sealed class CartageLooseBookingColumnProviderTest : GridColumnProviderTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "ExampleClass.Schema to AutoExampleClass.Schema are not safe in general")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddDefaultsColumn(new ZCalcEditColumn("Order", CommonBookedCtgMove.Schema.EW_DisplayOrder) { ColumnKey = WebTracker.Grids.TrackingCartageLooseBooking.Order });
			AddDefaultsColumn(new ZCalcEditColumn("Packs", CommonBookedCtgMove.Schema.EW_BookedPackCount) { ColumnKey = WebTracker.Grids.TrackingCartageLooseBooking.Packs });
			AddDefaultsColumn(new ZTextEditColumn("Type", CommonBookedCtgMove.Schema.EW_F3_NKPackType) { ColumnKey = WebTracker.Grids.TrackingCartageLooseBooking.Type });
			AddDefaultsColumn(new ZCalcEditColumn("Weight", CommonBookedCtgMove.Schema.EW_BookedWeight) { ColumnKey = WebTracker.Grids.TrackingCartageLooseBooking.Weight });
			AddDefaultsColumn(new ZTextEditColumn("UW", CommonBookedCtgMove.Schema.EW_WeightUQ) { ColumnKey = WebTracker.Grids.TrackingCartageLooseBooking.WeightUnit });
			AddDefaultsColumn(new ZCalcEditColumn("Volume", CommonBookedCtgMove.Schema.EW_BookedVolume) { ColumnKey = WebTracker.Grids.TrackingCartageLooseBooking.Volume });
			AddDefaultsColumn(new ZTextEditColumn("UV", CommonBookedCtgMove.Schema.EW_VolumeUQ) { ColumnKey = WebTracker.Grids.TrackingCartageLooseBooking.VolumeUnit });
			AddDefaultsColumn(new ZGuidDropDownListColumn("First Party", CommonBookedCtgMove.Schema.EW_E2PickupAddressID, OComboBoxDropDownStyle.CodeOnly) { ColumnKey = WebTracker.Grids.TrackingCartageLooseBooking.FirstParty });
			AddDefaultsColumn(new ZGuidDropDownListColumn("Second Party", CommonBookedCtgMove.Schema.EW_E2WaitPointAddressID, OComboBoxDropDownStyle.CodeOnly) { ColumnKey = WebTracker.Grids.TrackingCartageLooseBooking.SecondParty });
			AddDefaultsColumn(new ZTextEditColumn("Drop Mode", CommonBookedCtgMove.Schema.EW_DropMode) { ColumnKey = WebTracker.Grids.TrackingCartageLooseBooking.DropMode });
		}

		protected override bool SupportsOldLayoutFix
		{
			get { return false; }
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new CartageLooseBookingColumnProvider();
		}
	}
}

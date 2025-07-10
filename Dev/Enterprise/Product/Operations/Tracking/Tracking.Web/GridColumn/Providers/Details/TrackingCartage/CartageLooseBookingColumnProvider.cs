using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class CartageLooseBookingColumnProvider : GridColumnProvider
	{
		public CartageLooseBookingColumnProvider()
		{
		}

		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("bb1aa00c-a30a-4f26-8014-ddf0dacaa59b", "Order"), CommonBookedCtgMove.Schema.EW_DisplayOrder) { ColumnKey = WebTracker.Grids.TrackingCartageLooseBooking.Order });
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("4a895374-8114-4ef6-8556-24fa2ad5594a", "Packs"), CommonBookedCtgMove.Schema.EW_BookedPackCount) { ColumnKey = WebTracker.Grids.TrackingCartageLooseBooking.Packs });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("5841a42a-b736-41d5-a173-3d90ab4737ef", "Type"), CommonBookedCtgMove.Schema.EW_F3_NKPackType) { ColumnKey = WebTracker.Grids.TrackingCartageLooseBooking.Type });
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("55f88c53-5182-43af-a3a3-7a6b3757afb3", "Weight"), CommonBookedCtgMove.Schema.EW_BookedWeight) { ColumnKey = WebTracker.Grids.TrackingCartageLooseBooking.Weight });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("a0d72a46-4cdf-4d2e-8ada-1cae3c7ffc7a", "UW"), CommonBookedCtgMove.Schema.EW_WeightUQ) { ColumnKey = WebTracker.Grids.TrackingCartageLooseBooking.WeightUnit });
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("750e792f-18b3-401d-b92f-eb996db8554a", "Volume"), CommonBookedCtgMove.Schema.EW_BookedVolume) { ColumnKey = WebTracker.Grids.TrackingCartageLooseBooking.Volume });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("727490a8-cc07-4d35-bbf8-63088b745ff6", "UV"), CommonBookedCtgMove.Schema.EW_VolumeUQ) { ColumnKey = WebTracker.Grids.TrackingCartageLooseBooking.VolumeUnit });
			AddToDictionaryAsDefault(new ZGuidDropDownListColumn(Res.GetString("6466b77f-4a96-4af2-a3e8-bf69349e494d", "First Party"), CommonBookedCtgMove.Schema.EW_E2PickupAddressID, OComboBoxDropDownStyle.CodeOnly) { ColumnKey = WebTracker.Grids.TrackingCartageLooseBooking.FirstParty });
			AddToDictionaryAsDefault(new ZGuidDropDownListColumn(Res.GetString("cb2928c5-95a8-40cc-a3cc-fc08558a075a", "Second Party"), CommonBookedCtgMove.Schema.EW_E2WaitPointAddressID, OComboBoxDropDownStyle.CodeOnly) { ColumnKey = WebTracker.Grids.TrackingCartageLooseBooking.SecondParty });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("29e9375b-52da-4f0f-bd1b-814f27b9142f", "Drop Mode"), CommonBookedCtgMove.Schema.EW_DropMode) { ColumnKey = WebTracker.Grids.TrackingCartageLooseBooking.DropMode });
		}
	}
}

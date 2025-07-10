using System.Collections.Generic;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI
{
	public class WHSRateEntryCollectionGUIInfo : RateEntryCollectionGUIInfo
	{
		public override string Category
		{
			get { return RatingConstants.RateCategory.WHS; }
		}

		public override List<ZGridColumnInfo> Columns => new List<ZGridColumnInfo>
		{
			AllWarehouses,
			Warehouse,
			{ Currency, true, null, false },
			{ Container, true, Res.GetString("9218b420-bd5a-4657-988a-f1f470324fa2", "Container/Equipment Type") },
			MatchContainerClass,
			Consignor,
			{ CartagePickupAddress, true, Res.GetString("3b6d5955-6f55-467b-9ae2-1378c2602fe8", "Pickup Address") },
			{ CartagePickupPostcode, true, Res.GetString("594bb75e-9105-4a23-9f5f-9d8837f87c3d", "Pickup Postcode") },
			Consignee,
			{ CartageDeliveryAddress, true, Res.GetString("cd236f7d-31b7-49dd-bac2-f357f228ff54", "Delivery Address") },
			{ CartageDeliveryPostcode, true, Res.GetString("40949a94-312e-4beb-9235-f103b9830f12", "Delivery Postcode") },
			CarrierServiceLevel,
			ServiceLevel,
			{ GatewayServiceLevel, IsIntercompanyTariff },
			{ ShipmentGatewayServiceLevel, IsIntercompanyTariff },
			{ Supplier, !IsCosting },
			CommodityCode,
			{ CommodityDescription, null, false },
			CommodityLocalCode,
			{ RateStartDate, !IsQuote },
			{ RateEndDate, !IsQuote },
			{ DataChecked, IsQuote },
			ContractNumber,
			{ IsPublished, ShowIsPublishedColumn },
			{ Publisher, isGlobal },
			{ CreationSource, Env.CurrentUser.IsSupportUser }
		};
	}
}


using System.Collections.Generic;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI
{
	public class TBCRateEntryCollectionGUIInfo : RateEntryCollectionGUIInfo
	{
		public override string Category
		{
			get { return RatingConstants.RateCategory.TBC; }
		}

		public override List<ZGridColumnInfo> Columns => new List<ZGridColumnInfo>
		{
			{ Origin, true, Res.GetString("222feda1-cfb5-4625-ac8f-a73445597daa", "Location") },
			OriginZone,
			OriginSuburb,
			DestinationZone,
			DestinationSuburb,
			Mode,
			{ Currency, true, null, false },
			{ Container, true, Res.GetString("9218b420-bd5a-4657-988a-f1f470324fa2", "Container/Equipment Type") },
			MatchContainerClass,
			ServiceLevel,
			CarrierServiceLevel,
			{ GatewayServiceLevel, IsIntercompanyTariff },
			{ ShipmentGatewayServiceLevel, IsIntercompanyTariff },
			{ Consignor, true, Res.GetString("7a07ed1a-2e6e-4196-b0cf-72f2eb566226", "From Organization") },
			{ CartagePickupAddress, true, Res.GetString("c15b3206-48dd-4887-9215-40d9f58403c3", "From Address") },
			{ CartagePickupPostcode, true, Res.GetString("6656f7c8-57ad-4a9e-a969-f103534169d2", "From Postcode") },
			{ Consignee, true, Res.GetString("65fa5084-7697-478c-b9b1-a78bbdc74be2", "To Organization") },
			{ CartageDeliveryAddress, true, Res.GetString("d1e58267-9753-4bcd-92aa-f8f2a81a392d", "To Address") },
			{ CartageDeliveryPostcode, true, Res.GetString("08561b0d-2c76-4f9b-996c-fa0cbf9679c2", "To Postcode") },
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


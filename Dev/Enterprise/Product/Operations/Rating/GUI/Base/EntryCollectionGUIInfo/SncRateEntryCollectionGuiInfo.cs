using System.Collections.Generic;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI
{
	public class SNCRateEntryCollectionGUIInfo : RateEntryCollectionGUIInfo
	{
		public override string Category
		{
			get { return RatingConstants.RateCategory.SNC; }
		}

		public override List<ZGridColumnInfo> Columns => new List<ZGridColumnInfo>
		{
			Origin,
			Destination,
			Via,
			Currency,
			{ () => Carrier("Lookups.ShippingPrincipals"), true, Res.GetString("1c4fd387-0d1c-462a-9bd8-90b26e06298b", "Principal") },
			CommodityCode,
			{ CommodityDescription, null, false },
			CommodityLocalCode,
			Consignor,
			Consignee,
			Unit,
			TransitTime("Lookups.SeaTransitTimes"),
			Frequency,
			FrequencyUnit,
			{ RateStartDate, !IsQuote },
			{ RateEndDate, !IsQuote },
			{ DataChecked, IsQuote },
			{ IncoTerm, IsQuote },
			ContractNumber,
			ServiceLevel,
			{ GatewayServiceLevel, IsIntercompanyTariff },
			{ ShipmentGatewayServiceLevel, IsIntercompanyTariff },
			Supplier,
			{ IsPublished, ShowIsPublishedColumn },
			{ Publisher, isGlobal },
			{ CreationSource, Env.CurrentUser.IsSupportUser }
		};
	}
}


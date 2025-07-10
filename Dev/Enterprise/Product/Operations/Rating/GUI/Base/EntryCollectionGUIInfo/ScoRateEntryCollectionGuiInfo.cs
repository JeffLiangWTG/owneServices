using System.Collections.Generic;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI
{
	public class SCORateEntryCollectionGUIInfo : RateEntryCollectionGUIInfo
	{
		public override string Category
		{
			get { return RatingConstants.RateCategory.SCO; }
		}

		public override List<ZGridColumnInfo> Columns => new List<ZGridColumnInfo>
		{
			Origin,
			Destination,
			Via,
			Currency,
			{ () => Carrier("Lookups.ShippingPrincipals"), true, Res.GetString("40d76b53-cbc5-4e18-99b7-697fd984acca", "Principal") },
			{ Container, true, Res.GetString("f562b72e-5930-4865-94a9-0d5cf0fbc9fe", "Container") },
			MatchContainerClass,
			IsNonOperatedReefer,
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


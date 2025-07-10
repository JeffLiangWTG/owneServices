using System.Collections.Generic;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI
{
	public class SDERateEntryCollectionGUIInfo : RateEntryCollectionGUIInfo
	{
		public override string Category
		{
			get { return RatingConstants.RateCategory.SDE; }
		}

		public override List<ZGridColumnInfo> Columns => new List<ZGridColumnInfo>
		{
			Destination,
			Origin,
			Mode,
			{ Currency, true, null, false },
			{ Container, true, Res.GetString("fc7eec57-04fc-4ff7-a8cf-37f1becf2725", "Container") },
			MatchContainerClass,
			IsNonOperatedReefer,
			Consignee,
			{ Consignor, true, null, false },
			{ () => Carrier("Lookups.ShippingProviders"), true, Res.GetString("33208fe7-1b13-440d-a786-5259bc46dd38", "Principal") },
			CommodityCode,
			{ CommodityDescription, null, false },
			CommodityLocalCode,
			{ RateStartDate, !IsQuote },
			{ RateEndDate, !IsQuote },
			{ DataChecked, IsQuote },
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


using System.Collections.Generic;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI
{
	public class TWURateEntryCollectionGUIInfo : RateEntryCollectionGUIInfo
	{
		public override string Category => RatingConstants.RateCategory.TWU;

		public override List<ZGridColumnInfo> Columns => new List<ZGridColumnInfo>
		{
			AllWarehouses,
			Warehouse,
			Mode,
			{ Container, true, Res.GetString("9218b420-bd5a-4657-988a-f1f470324fa2", "Container/Equipment Type") },
			MatchContainerClass,
			Carrier("Lookups.ShippingProviders"),
			CommodityCode,
			{ CommodityDescription, null, false },
			CommodityLocalCode,
			{ RateStartDate, !IsQuote },
			{ RateEndDate, !IsQuote },
			{ IsPublished, ShowIsPublishedColumn },
			{ Publisher, isGlobal },
			{ CreationSource, Env.CurrentUser.IsSupportUser }
		};
	}
}

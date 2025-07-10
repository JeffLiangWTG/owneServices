using System.Collections.Generic;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI
{
	public class PACRateEntryCollectionGUIInfo : RateEntryCollectionGUIInfo
	{
		public override string Category
		{
			get { return RatingConstants.RateCategory.PAC; }
		}

		public override List<ZGridColumnInfo> Columns => new List<ZGridColumnInfo>
		{
			{ Origin, true, Res.GetString("61a7c965-bc93-4df1-a566-5381f6a361fd", "Port") },
			Destination,
			Mode,
			{ Currency, true, null, false },
			Container,
			MatchContainerClass,
			IsNonOperatedReefer,
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


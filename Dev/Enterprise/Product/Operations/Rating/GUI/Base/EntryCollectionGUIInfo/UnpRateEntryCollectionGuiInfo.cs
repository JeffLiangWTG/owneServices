using System.Collections.Generic;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI
{
	public class UNPRateEntryCollectionGUIInfo : RateEntryCollectionGUIInfo
	{
		public override string Category
		{
			get { return RatingConstants.RateCategory.UNP; }
		}

		public override List<ZGridColumnInfo> Columns => new List<ZGridColumnInfo>
		{
			{ Destination, true, Res.GetString("013f14a4-4c18-42f9-bdba-334e6f6bfa37", "Port") },
			Origin,
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


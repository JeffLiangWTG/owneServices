using System.Collections.Generic;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI
{
	public class CYDRateEntryCollectionGUIInfo : RateEntryCollectionGUIInfo
	{
		public override string Category
		{
			get { return RatingConstants.RateCategory.CYD; }
		}

		public override List<ZGridColumnInfo> Columns => new List<ZGridColumnInfo>
		{
			{ Mode, true, Res.GetString("0B9E8488-EFEF-4E3C-ADAE-0E5D818ADDF1", "Transport Mode") },
			Yard,
			YardUnitType,
			{ Container, true, Res.GetString("7ae181f6-97ec-45ab-833c-c289e577494b", "Type Size") },
			MatchContainerClass,
			YardUnitLoad,
			{ RateStartDate, !IsQuote },
			{ RateEndDate, !IsQuote },
			{ DataChecked, IsQuote },
			{ IsPublished, ShowIsPublishedColumn },
			{ Publisher, isGlobal },
			{ CreationSource, Env.CurrentUser.IsSupportUser }
		};
	}
}


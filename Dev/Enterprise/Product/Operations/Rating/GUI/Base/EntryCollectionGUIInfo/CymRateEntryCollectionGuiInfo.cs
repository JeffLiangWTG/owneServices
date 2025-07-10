using System.Collections.Generic;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI
{
	public class CYMRateEntryCollectionGUIInfo : RateEntryCollectionGUIInfo
	{
		public override string Category
		{
			get { return RatingConstants.RateCategory.CYM; }
		}

		public override List<ZGridColumnInfo> Columns => new List<ZGridColumnInfo>
		{
			{ Mode, true, Res.GetString("0B9E8488-EFEF-4E3C-ADAE-0E5D818ADDF1", "Transport Mode") },
			Yard,
			{ RefContainerComponent, !IsQuote },
			{ RefUnitSection, !IsQuote },
			{ RefContainerMaterial, !IsQuote },
			{ RefContainerRepair, !IsQuote },
			{ RateStartDate, !IsQuote },
			{ RateEndDate, !IsQuote },
			{ DataChecked, IsQuote },
			{ IsPublished, ShowIsPublishedColumn },
			{ Publisher, isGlobal },
			{ CreationSource, Env.CurrentUser.IsSupportUser }
		};
	}
}

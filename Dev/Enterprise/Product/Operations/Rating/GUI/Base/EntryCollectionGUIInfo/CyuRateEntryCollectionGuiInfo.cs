using System.Collections.Generic;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI
{
	public class CYURateEntryCollectionGUIInfo : RateEntryCollectionGUIInfo
	{
		public override string Category
		{
			get { return RatingConstants.RateCategory.CYU; }
		}

		public override List<ZGridColumnInfo> Columns => new List<ZGridColumnInfo>
		{
			{ Mode, true, Res.GetString("689d352c-9659-401d-b459-99ce46bf6d5c", "Transport Mode") },
			Yard,
			YardUnitType,
			{ Container, true, Res.GetString("09a7ca91-4b92-4fb3-b446-c9ee27a0da23", "Type Size") },
			MatchContainerClass,
			YardUnitLoad,
			{ RateStartDate, !IsQuote },
			{ RateEndDate, !IsQuote },
			{ DataChecked, IsQuote },
			{ IsPublished, ShowIsPublishedColumn },
			{ Publisher, isGlobal },
			{ CreationSource, Env.CurrentUser.IsSupportUser },
			{ ControllingCustomer, true, Res.GetString("bcd64dd4-481c-46cb-8508-7b94239a91ca", "Drop off/Pick up Client") },
		};
	}
}


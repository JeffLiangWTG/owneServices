using System.Collections.Generic;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI
{
	public class SIDRateEntryCollectionGUIInfo : RateEntryCollectionGUIInfo
	{
		public override string Category
		{
			get { return RatingConstants.RateCategory.SID; }
		}

		public override List<ZGridColumnInfo> Columns => new List<ZGridColumnInfo>
		{
			Destination,
			{ Currency, true, null, false },
			{ () => Carrier("Lookups.ShippingPrincipals"), true, Res.GetString("c8b21cbe-b7a3-4523-a2b0-919bec6134f8", "Principal") },
			{ Container, true, Res.GetString("be078945-a672-4829-933b-6af0037455a4", "Container") },
			MatchContainerClass,
			IsNonOperatedReefer,
			{ Consignee, true, Res.GetString("43dd454f-43f7-4962-ad9f-3910fda82d16", "Client") },
			Unit,
			{ RateStartDate, !IsQuote },
			{ RateEndDate, !IsQuote },
			{ DataChecked, IsQuote },
			ContractNumber,
			Supplier,
			{ IsPublished, ShowIsPublishedColumn },
			{ Publisher, isGlobal },
			{ CreationSource, Env.CurrentUser.IsSupportUser }
		};
	}
}


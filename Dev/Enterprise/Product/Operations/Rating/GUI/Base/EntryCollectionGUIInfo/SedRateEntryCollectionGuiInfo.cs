using System.Collections.Generic;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI
{
	public class SEDRateEntryCollectionGUIInfo : RateEntryCollectionGUIInfo
	{
		public override string Category
		{
			get { return RatingConstants.RateCategory.SED; }
		}

		public override List<ZGridColumnInfo> Columns => new List<ZGridColumnInfo>
		{
			Origin,
			{ Currency, true, null, false },
			{ () => Carrier("Lookups.ShippingPrincipals"), true, Res.GetString("40d76b53-cbc5-4e18-99b7-697fd984acca", "Principal") },
			{ Container, true, Res.GetString("3e89a051-7ad8-4451-b75d-f757f7ac6597", "Container") },
			MatchContainerClass,
			IsNonOperatedReefer,
			{ Consignor, true, Res.GetString("468b8991-ec6f-44a3-a516-ce3d7bb30b9f", "Client") },
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


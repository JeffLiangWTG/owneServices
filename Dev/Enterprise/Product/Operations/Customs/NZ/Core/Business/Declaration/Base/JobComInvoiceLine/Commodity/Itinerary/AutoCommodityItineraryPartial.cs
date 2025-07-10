
namespace Enterprise.Customs.NZ.Business.Declaration
{
	public partial class AutoCommodityItinerary
	{
		NZCommodityItineraryAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new NZCommodityItineraryAddInfo(B7_AddInfoDataInfo);
					fAddInfo.CommodityItinerary = this;
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		NZCommodityItineraryAddInfo fAddInfo;

		protected void updateAddinfoProperties()
		{
			if (AddInfo != null && HasChanges)
			{
				AddInfo.UpdateRelatedPropertyInfo();
			}
		}
	}
}

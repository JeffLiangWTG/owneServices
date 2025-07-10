
namespace Enterprise.Customs.NZ.Business.Declaration
{
	public partial class AutoCommodityConstituent
	{
		NZCommodityConstituentAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new NZCommodityConstituentAddInfo(B7_AddInfoDataInfo);
					fAddInfo.CommodityConstituent = this;
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		NZCommodityConstituentAddInfo fAddInfo;

		protected void updateAddinfoProperties()
		{
			if (AddInfo != null && HasChanges)
			{
				AddInfo.UpdateRelatedPropertyInfo();
			}
		}
	}
}

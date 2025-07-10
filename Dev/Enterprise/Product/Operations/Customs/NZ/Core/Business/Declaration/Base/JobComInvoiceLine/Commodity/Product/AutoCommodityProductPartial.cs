
namespace Enterprise.Customs.NZ.Business.Declaration
{
	public partial class AutoCommodityProduct
	{
		NZCommodityProductAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new NZCommodityProductAddInfo(B7_AddInfoDataInfo);
					fAddInfo.CommodityProduct = this;
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		NZCommodityProductAddInfo fAddInfo;

		protected void updateAddinfoProperties()
		{
			if (AddInfo != null && HasChanges)
			{
				AddInfo.UpdateRelatedPropertyInfo();
			}
		}
	}
}

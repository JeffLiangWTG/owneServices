
namespace Enterprise.Customs.NZ.Business.Declaration
{
	public partial class AutoCommodityLine
	{
		NZCommodityAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new NZCommodityAddInfo(B7_AddInfoDataInfo);
					fAddInfo.CommodityLine = this;
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		NZCommodityAddInfo fAddInfo;

		protected void updateAddinfoProperties()
		{
			if (AddInfo != null && HasChanges)
			{
				AddInfo.UpdateRelatedPropertyInfo();
			}
		}
	}
}

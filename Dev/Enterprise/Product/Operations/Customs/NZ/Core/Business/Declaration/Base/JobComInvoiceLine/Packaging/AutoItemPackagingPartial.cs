
namespace Enterprise.Customs.NZ.Business.Declaration
{
	public partial class AutoItemPackaging
	{
		ItemPackagingAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new ItemPackagingAddInfo(B7_AddInfoDataInfo);
					fAddInfo.ItemPackaging = this;
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		ItemPackagingAddInfo fAddInfo;

		protected void updateAddinfoProperties()
		{
			if (AddInfo != null && HasChanges)
			{
				AddInfo.UpdateRelatedPropertyInfo();
			}
		}
	}
}

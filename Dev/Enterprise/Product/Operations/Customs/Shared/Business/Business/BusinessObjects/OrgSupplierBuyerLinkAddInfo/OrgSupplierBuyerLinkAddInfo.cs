using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class OrgSupplierBuyerLinkAddInfo : AutoOrgSupplierBuyerLinkAddInfo
	{
		public OrgSupplierBuyerLinkAddInfo(ZPropertyInfoString parentPropertyInfo)
			: base(parentPropertyInfo.BizObj.Factory)
		{
			this.ParentPropertyInfo = parentPropertyInfo;
			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				Deserialise();
			}
			this.parentBO = parentPropertyInfo.BizObj;
		}

		public BusinessObject Parent
		{
			get { return parentBO; }
		}
		readonly BusinessObject parentBO;
	}
}

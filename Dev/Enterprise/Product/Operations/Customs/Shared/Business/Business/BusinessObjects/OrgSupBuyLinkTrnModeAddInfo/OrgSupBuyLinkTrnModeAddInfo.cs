using CargoWise.EntityFramework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Business
{
	public class OrgSupBuyLinkTrnModeAddInfo : AutoOrgSupBuyLinkTrnModeAddInfo, IOrgSupBuyLinkTrnModeAddInfo
	{
		public OrgSupBuyLinkTrnModeAddInfo(ZPropertyInfoString parentPropertyInfo) : base(parentPropertyInfo.BizObj.Factory)
		{
			parent = parentPropertyInfo.BizObj;
			ParentPropertyInfo = parentPropertyInfo;
			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				Deserialise();
			}
		}

		BusinessObject parent { get; }
		public BusinessObject Parent => parent;
	}
}

using CargoWise.EntityFramework;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.Access.Business.Testing
{
	public static class SGManifestTestHelper
	{
		public static SGGlbStaffWrapper SetUpSGAccessTestUser(BusinessObjectFactory factory)
		{
			var sgStaffWrapper = SGGlbStaffWrapper.Get(factory.Load<GlbStaff>(Env.CurrentUser.PK));
			sgStaffWrapper.AccessPassword.GP_GC = GlbCompany.CurrentCompany.PK;
			sgStaffWrapper.AccessPassword.GP_UserID = "VWGT002";
			sgStaffWrapper.AccessPassword.GP_PasswordStatus = Core.Constants.PasswordOK;
			return sgStaffWrapper;
		}
	}
}

using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Customs.NZ.GUI.Declaration
{
	public class NZGlbStaffWrapperProvider : GlbStaffWrapperProvider, Enterprise.MasterFiles.Integration.Customs.NZ.INZGlbStaffWrapperProvider
	{
		protected override StaffCredentialsUserControl GetNewUserControlCore()
		{
			return new NZStaffCredentialsUserControl();
		}

		protected override GlbStaffWrapper GetWrapperCore(GlbStaff staff)
		{
			return NZGlbStaffWrapper.Get(staff);
		}
	}
}

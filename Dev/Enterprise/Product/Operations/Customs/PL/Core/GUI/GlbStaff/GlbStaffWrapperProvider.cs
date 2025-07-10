using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.GUI;

public class GlbStaffWrapperProvider : MasterFiles.GUI.GlbStaffWrapperProvider, MasterFiles.Integration.Customs.PL.IPLGlbStaffWrapperProvider
{
	protected override MasterFiles.GUI.StaffCredentialsUserControl GetNewUserControlCore()
	{
		return new StaffCredentialsUserControl();
	}

	protected override GlbStaffWrapper GetWrapperCore(GlbStaff staff)
	{
		return Business.GlbStaffWrapper.Get(staff);
	}
}

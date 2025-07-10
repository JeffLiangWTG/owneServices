using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.GUI
{
	public class TWGlbStaffWrapperProvider : MasterFiles.GUI.GlbStaffWrapperProvider, MasterFiles.Integration.Customs.TW.ITWGlbStaffWrapperProvider
	{
		protected override MasterFiles.GUI.StaffCredentialsUserControl GetNewUserControlCore()
		{
			return new StaffCredentialsUserControl();
		}

		protected override GlbStaffWrapper GetWrapperCore(GlbStaff staff)
		{
			return TWGlbStaffWrapper.Get(staff);
		}
	}
}

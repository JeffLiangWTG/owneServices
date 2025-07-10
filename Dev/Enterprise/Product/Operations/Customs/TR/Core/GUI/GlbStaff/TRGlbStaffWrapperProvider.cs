using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.GUI.GlbStaff;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration.Customs.TR;

namespace Enterprise.Customs.TR.GUI
{
	public class TRGlbStaffWrapperProvider : GlbStaffWrapperProvider, ITRGlbStaffWrapperProvider
	{
		protected override StaffCredentialsUserControl GetNewUserControlCore()
		{
			return new TRStaffCredentialsUserControl();
		}

		protected override GlbStaffWrapper GetWrapperCore(MasterFiles.Business.GlbStaff staff)
		{
			return TRGlbStaffWrapper.Get(staff);
		}
	}
}

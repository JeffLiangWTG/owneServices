namespace Enterprise.Customs.US.GUI
{
	public class GlbStaffWrapperProvider : MasterFiles.GUI.GlbStaffWrapperProvider, MasterFiles.Integration.Customs.US.IUSGlbStaffWrapperProvider
	{
		protected override MasterFiles.GUI.StaffCredentialsUserControl GetNewUserControlCore()
		{
			return new StaffCredentialsUserControl();
		}

		protected override MasterFiles.Business.GlbStaffWrapper GetWrapperCore(MasterFiles.Business.GlbStaff staff)
		{
			return Business.GlbStaffWrapper.Get(staff);
		}
	}
}

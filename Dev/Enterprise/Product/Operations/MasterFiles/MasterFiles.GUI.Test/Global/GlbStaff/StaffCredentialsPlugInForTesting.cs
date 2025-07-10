using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class StaffCredentialsPlugInForTesting : StaffCredentialsPlugIn
	{
		public StaffCredentialsPlugInForTesting(GlbStaff staff)
			: base(staff, new GlbStaffWrapperProviderForTesting())
		{
		}
	}
}

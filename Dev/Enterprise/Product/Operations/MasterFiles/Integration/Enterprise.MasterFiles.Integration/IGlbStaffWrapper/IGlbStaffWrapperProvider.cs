namespace Enterprise.MasterFiles.Integration
{
	public interface IGlbStaffWrapperProvider
	{
		IGlbStaffWrapper GetWrapper(IGlbStaff staff);
	}
}

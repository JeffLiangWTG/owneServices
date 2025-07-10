namespace Enterprise.MasterFiles.Integration
{
	public interface IGlbCompanyWrapperProvider
	{
		IGlbCompanyWrapper GetWrapper(IGlbCompany company);
	}
}

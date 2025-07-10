namespace Enterprise.MasterFiles.Integration.Customs.US
{
	public interface IUSGlbCompanyWrapper : IGlbCompanyWrapper
	{
		IGlbExternalPasswordCollection_US PasswordCollection { get; }
	}
}

namespace Enterprise.MasterFiles.Integration.Customs.US
{
	public interface IUSGlbStaffWrapper : IGlbStaffWrapper
	{
		IGlbExternalPasswordCollection_US PasswordCollection { get; }
	}
}

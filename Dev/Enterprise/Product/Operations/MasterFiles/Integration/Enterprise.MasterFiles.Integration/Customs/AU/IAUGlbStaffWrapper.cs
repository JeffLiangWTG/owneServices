namespace Enterprise.MasterFiles.Integration.Customs.AU
{
	public interface IAUGlbStaffWrapper : IGlbStaffWrapper
	{
		IGlbExternalPassword NUTPassword { get; }
	}
}

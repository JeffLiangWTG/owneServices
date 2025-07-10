namespace Enterprise.MasterFiles.Integration.Customs.BR
{
	public interface IBRGlbStaffWrapper : IGlbStaffWrapper
	{
		IGlbExternalPassword CCTPassword { get; }
	}
}

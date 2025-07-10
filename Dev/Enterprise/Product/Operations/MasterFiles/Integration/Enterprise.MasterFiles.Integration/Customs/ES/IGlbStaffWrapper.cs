namespace Enterprise.MasterFiles.Integration.Customs.ES
{
	public interface IGlbStaffWrapper : Integration.IGlbStaffWrapper
	{
		IGlbExternalPasswordCollection ESBPasswordCollection { get; }
	}
}

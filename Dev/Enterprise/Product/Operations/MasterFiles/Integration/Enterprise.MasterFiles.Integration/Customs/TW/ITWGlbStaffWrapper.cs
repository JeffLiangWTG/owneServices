namespace Enterprise.MasterFiles.Integration.Customs.TW
{
	public interface ITWGlbStaffWrapper : IGlbStaffWrapper
	{
		IGlbExternalPasswordCollection_TW TWPasswordCollection { get; }
	}
}

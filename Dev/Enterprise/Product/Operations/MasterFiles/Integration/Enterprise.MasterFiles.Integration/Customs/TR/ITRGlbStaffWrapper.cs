namespace Enterprise.MasterFiles.Integration.Customs.TR
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
	public interface ITRGlbStaffWrapper : IGlbStaffWrapper
	{
		IGlbExternalPassword TRBPassword { get; }
	}
}

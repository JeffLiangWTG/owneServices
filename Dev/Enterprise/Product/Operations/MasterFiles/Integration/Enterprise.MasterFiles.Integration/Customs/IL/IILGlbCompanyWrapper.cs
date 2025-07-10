namespace Enterprise.MasterFiles.Integration.Customs.IL;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
public interface IILGlbCompanyWrapper : IGlbCompanyWrapper
{
	IGlbExternalPasswordWithCertificate GetGlbExternalPasswordOrCreateNew();
}

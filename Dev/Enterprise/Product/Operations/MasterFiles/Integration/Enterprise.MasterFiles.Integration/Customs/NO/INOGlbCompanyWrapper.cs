namespace Enterprise.MasterFiles.Integration.Customs.NO;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
public interface INOGlbCompanyWrapper : IGlbCompanyWrapper
{
	IGlbExternalPasswordWithCertificate Credential { get; }
}

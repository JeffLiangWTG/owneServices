namespace CargoWise.RefDbRepo.ComplianceListReferenceData.CmdLine
{
	public class AppConfiguration
	{
		public string OutputPath { get; set; }
		public string DpsWebServiceBaseUrl { get; set; }
		public string TenantId { get; set; }
		public string ClientId { get; set; }
		public string ServiceId { get; set; }
		public string PrivateKeyFileName { get; set; }
		public string CertificateFileName { get; set; }
	}
}

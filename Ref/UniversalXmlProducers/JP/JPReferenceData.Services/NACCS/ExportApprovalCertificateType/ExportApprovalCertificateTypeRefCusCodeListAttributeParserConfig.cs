namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class ExportApprovalCertificateTypeRefCusCodeListAttributeParserConfig : RefCusCodeListAttributeParserConfig
	{
		public override string ZZE_ZXE_NKName => "CertificateType";
		public override string GetZZE_Value(string[] columns) => "TOKG";
	}
}

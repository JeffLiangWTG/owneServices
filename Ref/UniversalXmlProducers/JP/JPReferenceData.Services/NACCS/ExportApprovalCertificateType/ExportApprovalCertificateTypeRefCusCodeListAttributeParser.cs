namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class ExportApprovalCertificateTypeRefCusCodeListAttributeParser : RefCusCodeListAttributeParser
	{
		protected override RefCusCodeListAttributeParserConfig[] GetConfigsCore() => new RefCusCodeListAttributeParserConfig[] { new ExportApprovalCertificateTypeRefCusCodeListAttributeParserConfig() };
	}
}

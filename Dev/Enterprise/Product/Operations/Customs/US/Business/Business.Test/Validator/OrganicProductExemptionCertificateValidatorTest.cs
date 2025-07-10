
namespace Enterprise.Customs.US.Business.Testing
{
	internal class OrganicProductExemptionCertificateValidatorTest : PermitValidatorTest<OrganicProductExemptionCertificateValidator>
	{
		protected override string[] ValidNumbers => new[] { "12345", "ABCD", "12345ABCD" };

		protected override string[] InvalidNumbers => new[] { "1234567890", "ABCDEFGHIJ", "12345 ABCD" };

		protected override string LicenseTypeCode => LicencePermitTypeList.Codes._22;

		protected override string LicenseTypeDescription => "Organic Product Exemption Certificate";
	}
}

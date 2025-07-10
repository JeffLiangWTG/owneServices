using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class OrganicProductExemptionCertificateValidator : PermitValidator
	{
		public OrganicProductExemptionCertificateValidator(JobComInvoiceLine invoiceLine)
			: base(LicencePermitTypeList.Codes._22, invoiceLine)
		{
		}

		public override void ValidateForExtraCondition(ZPropertyInfo propertyInfo)
		{
			new LicenceValidator().ValidateCottonCertificate(propertyInfo, InvoiceLine);
		}
	}
}

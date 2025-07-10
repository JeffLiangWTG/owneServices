using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public interface IOriginalInvoiceNumberAndDateValidationDecider
	{
		bool ShouldValidateOriginalTransactionNumberAndDate(ZString complianceSubType = default);
	}
}

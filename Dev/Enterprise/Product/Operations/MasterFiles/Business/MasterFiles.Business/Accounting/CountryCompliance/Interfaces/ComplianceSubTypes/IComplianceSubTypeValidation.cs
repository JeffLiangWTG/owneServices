using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public interface IComplianceSubTypeValidation
	{
		ZString ErrorMessageForComplianceSubTypeValidation(AccTransactionHeader transactionHeader);

		ZString WarningMessageForComplianceSubTypeValidation(AccTransactionHeader transactionHeader);
	}
}

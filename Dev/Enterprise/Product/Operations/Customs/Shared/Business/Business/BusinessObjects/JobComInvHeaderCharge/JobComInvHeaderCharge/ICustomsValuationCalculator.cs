using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public interface ICustomsValuationCalculator
	{
		ZDecimal GetAmountToAddToITOTForVatableGstable(RefCurrency currency);
		ZDecimal GetAmountToAddToITOTForDutiable(RefCurrency currency);
		ZDecimal GetAmountToAddToITOTForStatistical(RefCurrency currency);
	}
}

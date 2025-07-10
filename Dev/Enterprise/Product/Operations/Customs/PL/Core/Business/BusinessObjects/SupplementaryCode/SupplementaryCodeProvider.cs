using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

sealed class SupplementaryCodeProvider : EU.Business.SupplementaryCodeProvider
{
	public SupplementaryCodeProvider(ZString countryCode) : base(countryCode)
	{
	}

	public SupplementaryCodeProvider(ZString countryCode, ISupplementaryCodeSupporter master) : base(countryCode, master)
	{
	}

	protected override BaseSupplementaryCodeValidation GetNewValidationCore(BaseSupplementaryCode supplementaryCode) =>
		supplementaryCode.SupplementaryCodeSupporter is JobComInvoiceLine { Declaration: { } declaration } && declaration.IsExitSummary
			? new EU.Business.SupplementaryCodeValidation(supplementaryCode)
			: new SupplementaryCodeValidation(supplementaryCode);
}

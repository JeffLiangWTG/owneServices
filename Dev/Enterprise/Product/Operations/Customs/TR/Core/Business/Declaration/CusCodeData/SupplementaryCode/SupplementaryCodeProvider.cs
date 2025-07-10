using CargoWise.Types;
using Enterprise.Customs.Business;
namespace Enterprise.Customs.TR.Business;
sealed class SupplementaryCodeProvider : EU.Business.SupplementaryCodeProvider
{
	public SupplementaryCodeProvider(ZString countryCode) : base(countryCode)
	{
	}

	public SupplementaryCodeProvider(ZString countryCode, ISupplementaryCodeSupporter master) : base(countryCode, master)
	{
	}

	protected override BaseSupplementaryCodeLookups GetNewLookupsCore(BaseSupplementaryCode supplementaryCode) => new SupplementaryCodeLookups(supplementaryCode);

	protected override BaseSupplementaryCodeValidation GetNewValidationCore(BaseSupplementaryCode supplementaryCode) => new SupplementaryCodeValidation(supplementaryCode);
}

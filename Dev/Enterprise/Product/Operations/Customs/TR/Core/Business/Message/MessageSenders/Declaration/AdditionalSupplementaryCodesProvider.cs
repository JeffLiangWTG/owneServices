using CargoWise.Common;
using CargoWise.Customs.TR.MessageContracts.Interfaces.Declaration;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.TR.Business
{
	public class AdditionalSupplementaryCodesProvider : ITaxImmunitys
	{
		public AdditionalSupplementaryCodesProvider(SupplementaryCode supplementaryCode)
		{
			SupplementaryCode = Argument.NotNull(supplementaryCode, nameof(supplementaryCode));
		}
		SupplementaryCode SupplementaryCode { get; }

		public string TaxImmunityCode => SupplementaryCode.CY_Code;
	}
}

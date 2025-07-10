using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public interface IFiscalTaxCodeProvider
	{
		CodeDescriptionPairList GetFiscalTaxCodeForOutputNetAmount();
		CodeDescriptionPairList GetFiscalTaxCodeForOutputTaxAmount();
		CodeDescriptionPairList GetFiscalTaxCodeForInputTaxAmount();
		CodeDescriptionWithThreeGroupsCollection GetValidFiscalTaxCodeCombinations(ReadOnlyCodeDescriptionPairList ont, ReadOnlyCodeDescriptionPairList otx, ReadOnlyCodeDescriptionPairList itx);
	}
}

using System;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public class FRNCustomsOfficesCodeList : CodeDescriptionPairList, Integration.Customs.ZA.IFRNCustomsOfficesProvider, DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		public FRNCustomsOfficesCodeList()
		{
			var mappings = ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.GetValueWithoutFallback(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty);
			foreach (FinancialAccountNumberPortMap mapping in mappings)
			{
				AddPair(mapping.FinancialAccountNumber, CargoWise.Types.ZString.Format("FAN:{0} Office:{1}", mapping.FinancialAccountNumber, mapping.CustomsOfficeCode));
			}
		}

		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return this;
		}
	}
}

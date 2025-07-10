using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusGuaranteeHeaderLookups : SharedCusPermitHeaderLookups
	{
		public CusGuaranteeHeaderLookups(BaseCusGuaranteeHeader parent)
			: base(parent)
		{
		}

		public new BaseCusGuaranteeHeader Parent => (BaseCusGuaranteeHeader)base.Parent;

		public override CodeDescriptionPairList PermitTypes => Parent.CountrySpecificInstruction.GetTypeList(Parent.Country.Code);

		public RefCurrencyCollection Currencies => Parent.CountrySpecificInstruction.Currencies;
	}
}

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusGuaranteeReferenceNumberLookups : CusCodeDataLookups
	{
		public CusGuaranteeReferenceNumberLookups(CusGuaranteeReferenceNumber parent)
			: base(parent)
		{
		}

		public new CusGuaranteeReferenceNumber Parent => (CusGuaranteeReferenceNumber)base.Parent;

		public override CodeDescriptionPairList CY_CodeList => ((BaseCusGuaranteeHeader)Parent.Parent).CountrySpecificInstruction.AdditionalCustomsReferenceTypes;
	}
}

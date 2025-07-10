using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business
{
	public class GuaranteeCountrySpecificInstruction : EU.Business.GuaranteeCountrySpecificInstruction
	{
		public GuaranteeCountrySpecificInstruction(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override CodeDescriptionPairList GetTypeCodeDescriptionPairList() => Factory.GetCachedValue<GuaranteeTypeList>();
	}
}

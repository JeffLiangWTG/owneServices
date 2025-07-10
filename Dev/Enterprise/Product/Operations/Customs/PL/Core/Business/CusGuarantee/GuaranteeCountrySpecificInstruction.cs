
using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.Business;

public class GuaranteeCountrySpecificInstruction : EU.Business.GuaranteeCountrySpecificInstruction
{
	public GuaranteeCountrySpecificInstruction(BusinessObjectFactory factory) : base(factory)
	{
	}

	protected override CodeDescriptionPairList GetTypeCodeDescriptionPairList() => Factory.GetCachedValue<PLGuaranteeTypeList>();
}

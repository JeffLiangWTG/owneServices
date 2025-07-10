
using Enterprise.Customs.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.Business;

public class CusEngineLookups : Customs.Business.CusEngineLookups
{
	public CusEngineLookups(AutoCusEngine parent) : base(parent)
	{
	}

	public CodeDescriptionPairList FuelTypeList => Factory.GetCachedValue<FuelTypeList>();
}

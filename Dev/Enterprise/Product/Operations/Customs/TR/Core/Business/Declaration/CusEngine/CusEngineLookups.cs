using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class CusEngineLookups : Customs.Business.CusEngineLookups
	{
		public CusEngineLookups(AutoCusEngine parent) : base(parent)
		{
		}

		public CodeDescriptionPairList EngineTypeList => Factory.GetCachedValue<EngineTypeList>();
	}
}

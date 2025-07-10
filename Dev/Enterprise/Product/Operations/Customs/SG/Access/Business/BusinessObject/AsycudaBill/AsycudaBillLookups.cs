using CargoWise.EntityFramework;
using Enterprise.Customs.Common.SG;
using Enterprise.MasterFiles.Business.Customs.Asycuda;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.Access.Business
{
	public class AsycudaBillLookups : ASYCUDA.Business.AsycudaBillLookups
	{
		public AsycudaBillLookups(AsycudaBill parent)
			: base(parent)
		{
		}

		public static CodeDescriptionPairList GetSGPayeeIndicatorList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<SGPayeeIndicatorList>();
		}

		public CodeDescriptionPairList SGPayeeIndicatorList => GetSGPayeeIndicatorList(Factory);

		public static CodeDescriptionPairList GetSGPartyStatusList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<SGPartyStatusList>();
		}

		public CodeDescriptionPairList SGPartyStatusList => GetSGPartyStatusList(Factory);

		public CodeDescriptionPairList CycleNumbers => Factory.GetCycleNumbers();
	}
}

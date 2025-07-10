using Enterprise.Customs.Common.ZA;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Module
{
	public class JobDeclarationFilterLookups : Customs.Module.JobDeclarationFilterLookups
	{
		public JobDeclarationFilterLookups(JobDeclarationFilterBusinessObject filterBizObj)
			: base(filterBizObj)
		{
		}

		protected override CodeDescriptionPairList GetMainMessageStatusList() => Factory.GetCachedValue<ZAMessageStatusList>();

		public override CodeDescriptionPairList MessageTypeList => Factory.GetCachedValue<ZAJobMessageTypeList>();
	}
}

using Enterprise.Customs.Module;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Module.Declaration.ECIWriteOff
{
	public class ECIWriteOffFilterLookups : JobDeclarationFilterLookups
	{
		public ECIWriteOffFilterLookups(JobDeclarationFilterBusinessObject filterBizObj)
			: base(filterBizObj)
		{
		}

		public override CodeDescriptionPairList MessageSubTypeList()
		{
			return Factory.GetCachedValue<JobMessageSubTypeList>();
		}

		public override CodeDescriptionPairList EntryStatusList()
		{
			return Factory.GetCachedValue<LowValueConsignmentStatusList>();
		}

		public override CodeDescriptionPairList ContainerModeList
		{
			get { return Factory.GetCachedValue<ContainerModeList>(); }
		}
	}
}

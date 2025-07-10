using System;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Module;
using Enterprise.Customs.NZ.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NZ.Module.Declaration.FormalEntry
{
	public class NZJobDeclarationFilterLookups : JobDeclarationFilterLookups
	{
		public NZJobDeclarationFilterLookups(JobDeclarationFilterBusinessObject filterBizObj)
			: base(filterBizObj)
		{
		}

		public override CodeDescriptionPairList MessageSubTypeList()
		{
			return Factory.GetCachedValue<CodeDescriptionPairList>("JobMessageSubTypeList for TSWIM1Active ",
				delegate
				{
					var result = new JobMessageSubTypeList();
					result.AddPair(MessageSubTypeCombinedList.Codes.IPI, MessageSubTypeCombinedList.Descriptions.IPI);
					return result;
				});
		}

		public override CodeDescriptionPairList EntryStatusList()
		{
			var result = new CodeDescriptionPairList();
			result.AddRange(Factory.GetCachedValue<AmalgamatedStatusList>());

			var consolidatedEntriesEnabled = RawDataRegistry.Instance.EnableConsolidatedEntries.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			if (consolidatedEntriesEnabled)
			{
				var consolidationStatusList = Factory.GetCachedValue<ConsolidatedEntryStatusList>();
				foreach (CodeDescriptionPair consolidationStatus in consolidationStatusList)
				{
					result.AddPairIfNotExist(consolidationStatus.Code, consolidationStatus.Description);
				}
			}

			result.Sort();
			return result;
		}

		public override CodeDescriptionPairList ContainerModeList
		{
			get { return Factory.GetCachedValue<ContainerModeCustomsList>(); }
		}

		public override CodeDescriptionPairList PaymentPartyList()
		{
			return Factory.GetCachedValue<PaymentMethodList>();
		}

		public override CodeDescriptionPairList ApplicationCodeList()
		{
			return Factory.GetCachedValue<JobApplicationCodeList>();
		}
	}
}

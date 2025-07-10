using Enterprise.Freight.Common.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Business
{
	public class ContainerPenaltyLookup : JobContainerPenaltyLookups
	{
		public ContainerPenaltyLookup(AutoJobContainerPenalty parent) : base(parent)
		{
		}

		public CodeDescriptionPairList PenaltyTypeList =>
			Factory.GetCachedValue("ContainerPenalty.Lookups.PenaltyTypeList",
				delegate
				{
					var result = new CodeDescriptionPairList
					{
						new CodeDescriptionPair(Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention,
							Core.Constants.ContainerPenaltyPenaltyType.Descriptions.Detention),
						new CodeDescriptionPair(Core.Constants.ContainerPenaltyPenaltyType.Codes.Storage,
							Core.Constants.ContainerPenaltyPenaltyType.Descriptions.Storage),
						new CodeDescriptionPair(Core.Constants.ContainerPenaltyPenaltyType.Codes.TruckWaitTime,
							Core.Constants.ContainerPenaltyPenaltyType.Descriptions.TruckWaitTime),
						new CodeDescriptionPair(Core.Constants.ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention,
							Core.Constants.ContainerPenaltyPenaltyType.Descriptions.MergedDemurrageAndDetention)
					};

					return result;
				});

		public CodeDescriptionPairList CreditorTypeList =>
			Factory.GetCachedValue("ContainerPenalty.Lookups.CreditorTypeList",
				delegate
				{
					var result = new CodeDescriptionPairList
					{
						new CodeDescriptionPair(Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier,
							Core.Constants.ContainerPenaltyCreditorType.Descriptions.Carrier),
						new CodeDescriptionPair(Core.Constants.ContainerPenaltyCreditorType.Codes.CTO,
							Core.Constants.ContainerPenaltyCreditorType.Descriptions.CTO),
						new CodeDescriptionPair(Core.Constants.ContainerPenaltyCreditorType.Codes.Transport,
							Core.Constants.ContainerPenaltyCreditorType.Descriptions.Transport)
					};

					return result;
				});

		public CodeDescriptionPairList TimeUnitList =>
			Factory.GetCachedValue("ContainerPenalty.Lookups.TimeUnitList",
				delegate
				{
					var result = new CodeDescriptionPairList
					{
						new CodeDescriptionPair(Core.Constants.ContainerPenaltyTimeUnit.Codes.Days,
							Core.Constants.ContainerPenaltyTimeUnit.Descriptions.Days),
						new CodeDescriptionPair(Core.Constants.ContainerPenaltyTimeUnit.Codes.Hours,
							Core.Constants.ContainerPenaltyTimeUnit.Descriptions.Hours)
					};

					return result;
				});

		public CodeDescriptionPairList ProcessTypeList =>
			Factory.GetCachedValue("ContainerPenalty.Lookups.ProcessTypeList",
				delegate
				{
					var result = new CodeDescriptionPairList
					{
						new CodeDescriptionPair(Core.Constants.ContainerPenaltyProcessType.Export,
							Core.Constants.ContainerPenaltyProcessType.Export),
						new CodeDescriptionPair(Core.Constants.ContainerPenaltyProcessType.Import,
							Core.Constants.ContainerPenaltyProcessType.Import)
					};

					return result;
				});
	}
}

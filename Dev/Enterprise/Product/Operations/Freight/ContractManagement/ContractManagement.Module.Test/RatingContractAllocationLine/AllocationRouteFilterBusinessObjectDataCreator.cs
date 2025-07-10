using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ContractManagement.Module.Testing
{
	class AllocationRouteFilterBusinessObjectDataCreator
	{
		BusinessObjectFactory Factory { get; }

		public AllocationRouteFilterBusinessObjectDataCreator(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		public RatingContractAllocationLine CreateUnlinkedAllocationRoute(ZString load, ZString discharge)
		{
			var allocationRoute = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			allocationRoute.RCA_LoadLocation = load;
			allocationRoute.RCA_DischargeLocation = discharge;

			return allocationRoute;
		}

		public void CreateRelatedUNLOCOs(short groupNumber, params ZString[] unlocoCodes)
		{
			foreach (var code in unlocoCodes)
			{
				GetOrCreateUNLOCO(code);

				var relatedPortGroup = Factory.New<RefUNLOCORelatedPort>();
				relatedPortGroup.RLR_GroupNumber = groupNumber;
				relatedPortGroup.RLR_RL_NKRelatedPort = code;
			}

			Factory.Save();
		}

		public RefUNLOCO GetOrCreateUNLOCO(string code)
		{
			var refUnloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, code);
			if (refUnloco != null)
			{
				return refUnloco;
			}

			refUnloco = Factory.NewWithValidTestData<RefUNLOCO>();
			refUnloco.RL_Code = code;
			return refUnloco;
		}

		public RatingContractAllocationLine CreateAllocation(double weightLimit, string limitType, string unitQuantity)
		{
			var allocation = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			allocation.RCA_ContainerWeightLimit = weightLimit;
			allocation.RCA_ContainerWeightLimitType = limitType;
			allocation.RCA_ContainerWeightLimitUQ = unitQuantity;
			return allocation;
		}
	}
}

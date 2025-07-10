//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccChargeBranchOverrideLookups
//
//    This class should be used for overriding collections in AutoAccChargeBranchOverrideLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccChargeBranchOverrideLookups : AutoAccChargeBranchOverrideLookups
	{
		public AccChargeBranchOverrideLookups(AutoAccChargeBranchOverride parent) : base(parent)
		{
			Parent = parent as AccChargeBranchOverride;
		}

		new AccChargeBranchOverride Parent { get; }

		public override GlbBranchCollection SpecificBranches
		{
			get
			{
				if (specificBranches == null)
				{
					var company = Parent?.ChargeCode?.Company;

					if (company != null)
					{
						var branchesQuery = new ZQuery(GlbBranchSchema.GB_GC, company.PK);
						branchesQuery.AddToFilter(GlbBranchSchema.GB_IsActive, ZBool.True);
						specificBranches = new GlbBranchCollection(Factory, branchesQuery);
					}
					else
					{
						specificBranches = new GlbBranchCollection(Factory, new ZQuery() { IsNoResultQuery = true });
					}
				}
				return specificBranches;
			}
		}
		GlbBranchCollection specificBranches;

		#region JobTypeList

		public CodeDescriptionPairList JobTypeList => Parent.JobTypeDirectionAndTransportListProvider.JobTypeList;

		public static class JobTypeAdditionalCodes
		{
			public const string All = "ALL";
		}

		#endregion

		public CodeDescriptionPairList DirectionList => Parent.JobTypeDirectionAndTransportListProvider.DirectionList;

		#region TransportModeList

		public CodeDescriptionPairList TransportModeList => Parent.JobTypeDirectionAndTransportListProvider.TransportModeList;

		public static class TransportModeAdditionalCodes
		{
			public const string All = "ALL";
		}

		#endregion

		#region DefaultingRuleList

		public CodeDescriptionPairList DefaultingRuleList
		{
			get
			{
				var resultList = DefaultingRuleListForInvalidJobType;
				var consumerType = Parent?.JobType;

				switch (consumerType)
				{
					case BaseShipmentConsumerType bst:
						resultList = Factory.GetCachedValue("BaseShipmentConsumerType.ChargeBranchDefaultingRuleList", () =>
						{
							var result = DefaultingRuleListForInvalidJobType;
							result.Add(ShipmentChargeBranchDefaultingRules.ReceivingAgent);
							result.Add(ShipmentChargeBranchDefaultingRules.SendingAgent);
							result.Add(ShipmentChargeBranchDefaultingRules.ShipmentDeliveryAgentWithFallbackToReceivingAgent);
							result.Add(ShipmentChargeBranchDefaultingRules.ArrivalCTO);
							result.Add(ShipmentChargeBranchDefaultingRules.DepartureCTO);
							result.Add(ShipmentChargeBranchDefaultingRules.ConsolArrivalLocalTransport);
							result.Add(ShipmentChargeBranchDefaultingRules.ConsolDepartureLocalTransport);
							result.Add(ShipmentChargeBranchDefaultingRules.ShipmentPickupLocalTransportCompany);
							result.Add(ShipmentChargeBranchDefaultingRules.ShipmentDeliveryLocalTransportCompany);
							result.Add(ShipmentChargeBranchDefaultingRules.ShipmentImportBroker);
							result.Add(ShipmentChargeBranchDefaultingRules.ShipmentExportBroker);

							return result;
						});
						break;

					case GatewayConsolConsumerType gcct:
						resultList = Factory.GetCachedValue("GatewayConsolConsumerType.ChargeBranchDefaultingRuleList", () =>
						{
							var result = DefaultingRuleListForInvalidJobType;
							result.Add(ShipmentChargeBranchDefaultingRules.ReceivingAgent);
							result.Add(ShipmentChargeBranchDefaultingRules.SendingAgent);

							return result;
						});
						break;
				}

				return resultList;
			}
		}

		CodeDescriptionPairList DefaultingRuleListForInvalidJobType => new CodeDescriptionPairList { JobInvoicingConsumerType.ChargeBranchDefaultingRulesBase.SpecificBranchAlways };

		#endregion
	}
}

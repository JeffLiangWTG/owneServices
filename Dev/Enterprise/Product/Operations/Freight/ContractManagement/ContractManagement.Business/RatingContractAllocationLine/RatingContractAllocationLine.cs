using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ContractManagement.Business
{
	[CodeProperty(Schema.RCA_AllocationLineID), DescriptionProperty(Schema.ParentContractNumber)]
	[UniversalCopyIgnoreElement("JobConsols", "JobContainers")]
	public class RatingContractAllocationLine : AutoRatingContractAllocationLine, IRatingContractAllocationLine
	{
		public new abstract class Schema : AutoRatingContractAllocationLine.Schema
		{
			public const string ParentContractNumber = "ParentContractNumber";
		}

		public RatingContractAllocationLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[RelatedBusinessObject(nameof(Contract))]
		public override ZGuid RCA_RCT_RatingContract
		{
			get => base.RCA_RCT_RatingContract;
			set => base.RCA_RCT_RatingContract = value;
		}

		public ZString ParentContractNumber => Contract?.RCT_ContractNumber ?? ZString.Empty;

		public RatingContract Contract => Factory.Load<RatingContract>(RCA_RCT_RatingContract);

		public ZDate StartDateWithContractFallback
		{
			get
			{
				if (RCA_StartDate.IsEmpty)
				{
					return Contract?.RCT_StartDate ?? ZDate.Empty;
				}

				return RCA_StartDate;
			}
		}

		public ZDate ExpiryDateWithContractFallback
		{
			get
			{
				if (RCA_ExpiryDate.IsEmpty)
				{
					return Contract?.RCT_EndDate ?? ZDate.Empty;
				}

				return RCA_ExpiryDate;
			}
		}

		public IJobSailing JobSailing => Factory.Load<IJobSailing>(RCA_JX_SailingSchedule);
		public IJobTradeLane TradeLane => Factory.Load<IJobTradeLane>(RCA_EJ_TradeLane);

		IVoyageOrigin VoyageOrigin => JobSailing != null ? Factory.Load<IVoyageOrigin>(JobSailing.JX_JA) : null;

		IVoyageDestination VoyageDestination => JobSailing != null ? Factory.Load<IVoyageDestination>(JobSailing.JX_JB) : null;

		IJobVoyage JobVoyageFromOrigin => VoyageOrigin != null ? Factory.Load<IJobVoyage>(VoyageOrigin.JA_JV) : null;

		IRatingContract IRatingContractAllocationLine.Contract => Contract;

		[ChildEditable(true)]
		public IRelatedNamedAccountsPivotCollection<IRatingContractNamedAccountPivot> NamedAccountPivots
		{
			get
			{
				if (namedAccountPivots == null)
				{
					namedAccountPivots = new RelatedNamedAccountsPivotCollection(this);
					RegisterEditableChildObject(namedAccountPivots);
				}

				return namedAccountPivots;
			}
		}
		RelatedNamedAccountsPivotCollection namedAccountPivots;

		public ZString NamedAccountsFormatted
		{
			get
			{
				return string.Join(", ", NamedAccountPivots.GetAllNamedAccounts().Select(namedAccount => namedAccount.OH_Code));
			}
		}

		[ChildEditable(true)]
		public IRelatedAgentsPivotCollection<IAllocationRouteAgentPivot> AgentPivots
		{
			get
			{
				if (agentPivots == null)
				{
					agentPivots = new RelatedAgentsPivotCollection(this);
					RegisterEditableChildObject(agentPivots);
				}

				return agentPivots;
			}
		}
		RelatedAgentsPivotCollection agentPivots;

		public ZString AgentsFormatted
		{
			get
			{
				return string.Join(", ", AgentPivots.GetAllAgents().Select(agent => agent.OH_Code));
			}
		}

		public ZDecimal Utilization
		{
			get
			{
				return RCA_AllocatedUQ == Constants.AllocationQuantityUnits.Containers
					? Convert.ToDecimal(ViewRatingContractAllocationLineSummary?.RAV_ContainerCount ?? 0)
					: Convert.ToDecimal(ViewRatingContractAllocationLineSummary?.RAV_TEUCount ?? 0);
			}
		}

		public ZDecimal CapacityWithVariance => RCA_AllocatedQuantity * (1 + RCA_BookingVariance / 100);

		public ZDecimal OutstandingCommitted => RCA_AllocatedQuantity - Utilization;

		public ZDecimal OutstandingWithVariance => CapacityWithVariance - Utilization;

		public ZString RCA_Calc_LoadLocation => VoyageOrigin?.JA_RL_NKPortOfLoading ?? RCA_LoadLocation;

		public ZString RCA_Calc_DischargeLocation => VoyageDestination?.JB_RL_NKPortOfDischarge ?? RCA_DischargeLocation;

		public ZString RCA_Calc_VesselName => JobVoyageFromOrigin?.JV_RV_NKVessel ?? RCA_RV_NKVessel;

		public ZString RCA_Calc_VoyageNumber => JobVoyageFromOrigin?.JV_VoyageFlight ?? RCA_VoyageNumber;

		public ZString RCA_Calc_ServiceString => JobSailing?.JX_ServiceString ?? RCA_ServiceLoop;

		public ZString LinkedSchedule => JobSailing?.JX_UniqueReference ?? ZString.Empty;

		public ZDateTime LinkedScheduleETD => VoyageOrigin?.JA_E_DEP ?? ZDateTime.Empty;

		public ZBool LinkedScheduleETDUpdated => LinkedScheduleETD != LinkedScheduleSTD;

		public ZDateTime LinkedScheduleSTD => VoyageOrigin?.JA_S_DEP ?? ZDateTime.Empty;

		public ZDateTime LinkedScheduleETA => VoyageDestination?.JB_E_ARV ?? ZDateTime.Empty;

		public ZDateTime LinkedScheduleSTA => VoyageDestination?.JB_S_ARV ?? ZDateTime.Empty;

		public ZString RCA_Calc_TradeLaneCode => TradeLane?.EJ_Code ?? ZString.Empty;
		public ZString RCA_Calc_TradeLaneDescription => TradeLane?.EJ_Description ?? ZString.Empty;

		public IRefContainer RefContainerType => ContainerType;

		public IRatingContractAllocationLine ParentAllocationRoute => Factory.Load<IRatingContractAllocationLine>(RCA_RCA_ParentAllocationRoute);

		IAllocationDistributionCollection IRatingContractAllocationLine.AllocationDistributions => AllocationDistributions;
		public AllocationDistributionCollection AllocationDistributions
		{
			get
			{
				return allocationDistributions ??= new AllocationDistributionCollection(this);
			}
		}
		AllocationDistributionCollection allocationDistributions;

		internal ViewRatingContractAllocationLineSummary ViewRatingContractAllocationLineSummary => viewRatingContractAllocationLineSummary
			?? (viewRatingContractAllocationLineSummary = Factory.LoadTop1<ViewRatingContractAllocationLineSummary>(new ZQuery(ViewRatingContractAllocationLineSummarySchema.RAV_RCA_AllocationLine, PK)));

		ViewRatingContractAllocationLineSummary viewRatingContractAllocationLineSummary;

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return Res.GetString("424f1a2a-2154-3fa7-4cc1-5782aad76884", "Allocation Route {0}", RCA_AllocationLineID);
			}
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			RCA_StartDate = ZDate.Today;
			RCA_ExpiryDate = ZDate.Today.AddDays(30);
			RCA_AllocatedQuantity = 1;
			RCA_AllocatedUQ = Constants.AllocationQuantityUnits.TwentyFootUnits;
		}

#endif
	}
}

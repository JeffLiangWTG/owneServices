using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Business
{
	public abstract class ContainerPenaltyCollection : ActiveBusinessObjectCollection<ContainerPenalty>
	{
		protected ContainerPenaltyCollection(CommonContainer container, string processType)
			: base(container.Factory, new DependentRelationship(container, typeof(ContainerPenalty), new ZQuery(JobContainerPenaltySchema.CPY_JC_Container, container.PK), JobContainerPenaltySchema.CPY_JC_Container))
		{
			this.container = container;
			this.processType = processType;
			AdditionalFilter = new ZQuery(JobContainerPenaltySchema.CPY_ProcessType, processType);
		}

		protected readonly CommonContainer container;
		protected readonly string processType;

		protected override void OnAdded(ContainerPenalty businessObject)
		{
			base.OnAdded(businessObject);

			businessObject.CPY_ProcessType = processType;
		}

		protected ContainerPenalty FindOrCreateCarrierDetentionPenalty(bool createIfNotExists, bool notCreateIfNotMatch = true, bool returnNullIfShouldNotDefault = false, CommonShipment shipment = null)
		{
			var found = FindContainerPenalty(ContainerPenaltyPenaltyType.Codes.Detention, ContainerPenaltyCreditorType.Codes.Carrier, shipment);
			if (found != null)
			{
				if (returnNullIfShouldNotDefault && FindContainerPenalty(ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention, ContainerPenaltyCreditorType.Codes.Carrier, shipment) != null)
				{
					return null;
				}

				return found;
			}

			if (createIfNotExists)
			{
				if (notCreateIfNotMatch && !IsMatchDetention(shipment)
					|| FindContainerPenalty(ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention, ContainerPenaltyCreditorType.Codes.Carrier, shipment) != null)
				{
					return null;
				}

				return FindOrCreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.Detention, ContainerPenaltyCreditorType.Codes.Carrier, timeUnit: ContainerPenaltyTimeUnit.Codes.Days, shipment: shipment);
			}

			return null;
		}

		protected ContainerPenalty FindOrCreateMDDPenalty(bool createIfNotExists, bool notCreateIfNotMatch = true, CommonShipment shipment = null)
		{
			var found = FindContainerPenalty(ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention, ContainerPenaltyCreditorType.Codes.Carrier, shipment);
			if (found != null)
			{
				return found;
			}

			if (createIfNotExists)
			{
				if (notCreateIfNotMatch && !IsMatchMDD(shipment))
				{
					return null;
				}

				if (FindContainerPenalty(ContainerPenaltyPenaltyType.Codes.Detention, ContainerPenaltyCreditorType.Codes.Carrier, shipment) != null
					|| FindContainerPenalty(ContainerPenaltyPenaltyType.Codes.Storage, ContainerPenaltyCreditorType.Codes.Carrier, shipment) != null)
				{
					return null;
				}

				return FindOrCreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention, ContainerPenaltyCreditorType.Codes.Carrier, timeUnit: ContainerPenaltyTimeUnit.Codes.Days, shipment: shipment);
			}

			return null;
		}

		public ContainerPenalty FindContainerPenalty(ZString penaltyType, ZString creditorType, CommonShipment shipment = null)
		{
			if (shipment != null)
			{
				return this.FirstOrDefault(p => p.CPY_JS_Shipment == shipment.PK && p.CPY_PenaltyType == penaltyType && p.CPY_CreditorType == creditorType);
			}

			return this.FirstOrDefault(p => p.CPY_PenaltyType == penaltyType && p.CPY_CreditorType == creditorType);
		}

		public virtual ContainerPenalty FindOrCreateContainerPenalty(ZString penaltyType,
			ZString creditorType,
			ZString timeUnit = default,
			OrgAddress creditor = null,
			CommonShipment shipment = null)
		{
			var penalty = FindContainerPenalty(penaltyType, creditorType, shipment);
			if (penalty == null)
			{
				penalty = CreateContainerPenalty(penaltyType, creditorType, timeUnit, creditor, shipment);
				Add(penalty);
			}

			return penalty;
		}

		public abstract RefUNLOCO PenaltyPort { get; }

		public virtual ContainerPenalty CreateContainerPenalty(ZString penaltyType, ZString creditorType,
			ZString timeUnit = default,
			OrgAddress creditor = null,
			CommonShipment shipment = null)
		{
			var penalty = this.AddNew();
			penalty.CPY_ProcessType = processType;
			penalty.CPY_PenaltyType = penaltyType;
			penalty.CPY_CreditorType = creditorType;
			penalty.CPY_RL_NKLocation = PenaltyPort?.Code ?? ZString.Empty;
			penalty.CPY_TimeUnit = timeUnit;
			penalty.CPY_RX_NKCurrency = ContainerPenalty.GetPenaltyCurrency(Factory, PenaltyPort?.RL_RN_NKCountryCode ?? ZString.Empty);
			penalty.CPY_JS_Shipment = shipment?.PK ?? ZGuid.Empty;

			if (creditor?.OA_OH.IsValid ?? false)
			{
				penalty.CPY_OH_Creditor = creditor.OA_OH;
			}

			return penalty;
		}

		protected bool IsMatchStorage(ZString direction, ZString creditorType, CommonShipment shipment)
		{
			var strategy = container?.NewContainerDefaultingStrategy();
			if (strategy == null)
			{
				return false;
			}

			return strategy.GetMatchedStoragePenalty(direction, creditorType, processType, shipment) != null;
		}

		protected bool IsMatchDetention(CommonShipment shipment)
		{
			var strategy = container?.NewContainerDefaultingStrategy();
			if (strategy == null)
			{
				return false;
			}

			return strategy.GetMatchedDetentionPenalty(PenaltyPort?.Code ?? ZString.Empty, ContainerPenalty.GetDirection(processType), processType, shipment) != null;
		}

		protected bool IsMatchMDD(CommonShipment shipment)
		{
			var strategy = container?.NewContainerDefaultingStrategy();
			if (strategy == null)
			{
				return false;
			}

			return strategy.GetMatchedMDDPenalty(PenaltyPort?.Code ?? ZString.Empty, ContainerPenalty.GetDirection(processType), processType, shipment) != null;
		}
	}
}

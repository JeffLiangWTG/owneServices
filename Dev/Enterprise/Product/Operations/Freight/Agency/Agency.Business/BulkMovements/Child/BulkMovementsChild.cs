using System.Diagnostics;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	public class BulkMovementsChild : AutoBulkMovementsChild
	{
		public BulkMovementsChild(BulkMovementsHeader header)
			: base(header.Factory)
		{
			this.header = header;
		}

		[List("Lookups.MovementCodeList")]
		public override ZString MovementType
		{
			[DebuggerStepThrough]
			get { return base.MovementType; }
			set
			{
				base.MovementType = value;
				DepotAddressPK_ZAddress.DefaultAddressType = ContainerMovementTypes.GetDefaultAddressType(value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateMovementDate();
					Validation.ValidateContainerNum();
				}
			}
		}

		public override ZDateTime MovementDate
		{
			[DebuggerStepThrough]
			get { return base.MovementDate; }
			set
			{
				base.MovementDate = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateMovementType();
					Validation.ValidateContainerNum();
				}
			}
		}

		public override ZString ContainerNum
		{
			[DebuggerStepThrough]
			get { return base.ContainerNum; }
			set
			{
				base.ContainerNum = value;

				RefContainerStock stock = Factory.LoadFromNaturalKey<RefContainerStock>(RefContainerStockSchema.R6_ContainerNum, value);

				if (stock != null)
				{
					ForcedContainerType = stock.R6_RC;
					ForcedOwnerType = stock.R6_OwnerType;
				}
				else
				{
					ForcedContainerType = ZGuid.Empty;
					ForcedOwnerType = ZString.Empty;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateMovementType();
					Validation.ValidateMovementDate();
				}
			}
		}

		[List("Lookups.CleanCodeList")]
		public override ZString Condition
		{
			[DebuggerStepThrough]
			get { return base.Condition; }
			[DebuggerStepThrough]
			set { base.Condition = value; }
		}

		[List("Lookups.DamageCodeList")]
		public override ZString Damage
		{
			[DebuggerStepThrough]
			get { return base.Damage; }
			[DebuggerStepThrough]
			set { base.Damage = value; }
		}

		[List("Lookups.Principals")]
		public override ZGuid PrincipalPK
		{
			[DebuggerStepThrough]
			get { return base.PrincipalPK; }
			[DebuggerStepThrough]
			set { base.PrincipalPK = value; }
		}

		[List("Lookups.ResponsibleParties")]
		public override ZGuid ResponsiblePartyPK
		{
			[DebuggerStepThrough]
			get { return base.ResponsiblePartyPK; }
			[DebuggerStepThrough]
			set { base.ResponsiblePartyPK = value; }
		}

		protected override ZAddress GetNewDepotAddressPK_ZAddress()
		{
			ZAddress result = base.GetNewDepotAddressPK_ZAddress();

			result.OrgPKValidation = delegate(ZPropertyInfo info)
			{
				TypeValidation.CheckValidGuid(info);
				MandatoryValidation.CheckEntered(info);
			};

			return result;
		}

		[List("Lookups.ContainerTypeList")]
		[ReadOnlyMember(nameof(ForceContainerTypeSpecified))]
		public override ZGuid ContainerType
		{
			get
			{
				ZGuid force = ForcedContainerType;
				return force.IsEmpty ? base.ContainerType : force;
			}
			[DebuggerStepThrough]
			set { base.ContainerType = value; }
		}

		public override ZGuid ForcedContainerType
		{
			[DebuggerStepThrough]
			get { return base.ForcedContainerType; }
			set
			{
				base.ForcedContainerType = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateContainerType();
				}
			}
		}

		public ZBool ForceContainerTypeSpecified
		{
			get { return !ForcedContainerType.IsEmpty; }
		}

		[List("Lookups.OwnerTypeList")]
		[ReadOnlyMember(nameof(ForceOwnerTypeSpecified))]
		public override ZString OwnerType
		{
			get
			{
				ZString force = ForcedOwnerType;
				return force.IsEmpty ? base.OwnerType : force;
			}
			[DebuggerStepThrough]
			set { base.OwnerType = value; }
		}

		public override ZString ForcedOwnerType
		{
			[DebuggerStepThrough]
			get { return base.ForcedOwnerType; }
			set
			{
				base.ForcedOwnerType = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateOwnerType();
				}
			}
		}

		public ZBool ForceOwnerTypeSpecified
		{
			get { return !ForcedOwnerType.IsEmpty; }
		}

		public override ZBool IsGenerated
		{
			[DebuggerStepThrough]
			get { return base.IsGenerated; }
			set
			{
				base.IsGenerated = value;
				ReadOnly = value;
			}
		}

		public override ZString VesselName
		{
			get
			{
				JobVoyage voyage = Voyage;
				return voyage == null ? ZString.Empty : voyage.JV_RV_NKVessel;
			}
		}

		public override ZString VoyageNo
		{
			get
			{
				JobVoyage voyage = Voyage;
				return voyage == null ? ZString.Empty : voyage.JV_VoyageFlight;
			}
		}

		public BulkMovementsHeader Header
		{
			[DebuggerStepThrough]
			get { return header; }
		}

		public JobVoyage Voyage
		{
			get { return Factory.Load<JobVoyage>(VoyagePK); }
		}

		public BulkMovementsChildLookups Lookups
		{
			get { return lookups ?? (lookups = new BulkMovementsChildLookups(this)); }
		}
		BulkMovementsChildLookups lookups;

		public void Generate(BusinessObjectFactory createFactory)
		{
			if (!IsGenerated)
			{
				RefContainerStock stock = createFactory.LoadFromNaturalKey<RefContainerStock>(RefContainerStockSchema.R6_ContainerNum, ContainerNum);

				if (stock == null)
				{
					stock = createFactory.New<RefContainerStock>();
					stock.R6_ContainerNum = ContainerNum;
					stock.R6_RC = ContainerType;
					stock.R6_OwnerType = OwnerType;
				}

				ContainerMovement movement = createFactory.New<ContainerMovement>();
				movement.E9_R6 = stock.PK;
				movement.E9_OA_Depot = DepotAddressPK;
				movement.E9_OH_Principal = PrincipalPK;
				movement.E9_OH_ResponsibleParty = ResponsiblePartyPK;
				movement.E9_JV = VoyagePK;
				movement.E9_MovementType = MovementType;
				movement.E9_MovementDate = MovementDate;
				movement.E9_ContainerQuality = Condition;
				movement.E9_ContainerCondition = Damage;
				movement.E9_ContainerIsEmpty = ContainerIsEmpty;
				movement.E9_LeaseNumber = LeaseContractNo;
			}
		}

		protected override ZBool GetIsDuplicate()
		{
			foreach (BulkMovementsChild sibling in Header.Children)
			{
				if (sibling.ContainerNum == ContainerNum &&
					sibling.MovementType == MovementType &&
					sibling.MovementDate == MovementDate &&
					sibling.PK != PK)
				{
					return true;
				}
			}

			return false;
		}

		protected override ZBool GetMovementExists()
		{
			RefContainerStock stock = RefContainerStock.Load(Factory, ContainerNum);

			return stock != null
				&& !MovementType.IsEmpty
				&& MovementDate.IsValidSmallDateTime
				&& ContainerMovementHelper.FindDuplicate(stock, MovementType, MovementDate) != null;
		}

		readonly BulkMovementsHeader header;
	}
}

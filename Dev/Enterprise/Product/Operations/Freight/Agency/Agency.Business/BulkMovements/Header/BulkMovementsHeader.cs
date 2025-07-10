using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Agency.Business
{
	public class BulkMovementsHeader : AutoBulkMovementsHeader
	{
		public BulkMovementsHeader(BusinessObjectFactory factory)
			: base(factory) { }

		[List("Lookups.MovementCodeList")]
		public override ZString MovementType
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.MovementType; }
			set
			{
				base.MovementType = value;
				DepotAddressPK_ZAddress.DefaultAddressType = ContainerMovementTypes.GetDefaultAddressType(value);
			}
		}

		[List("Lookups.Principals")]
		public override ZGuid PrincipalPK
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.PrincipalPK; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.PrincipalPK = value; }
		}

		[List("Lookups.ResponsibleParties")]
		public override ZGuid ResponsiblePartyPK
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.ResponsiblePartyPK; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.ResponsiblePartyPK = value; }
		}

		public BulkMovementsChildCollection Children
		{
			get
			{
				if (children == null)
				{
					children = new BulkMovementsChildCollection(this);
					RegisterEditableChildObject(children);
				}

				return children;
			}
		}
		BulkMovementsChildCollection children;

		public BulkMovementsHeaderLookups Lookups
		{
			get { return lookups ?? (lookups = new BulkMovementsHeaderLookups(this)); }
		}
		BulkMovementsHeaderLookups lookups;

		public void Generate(BusinessObjectFactory createFactory)
		{
			foreach (BulkMovementsChild child in Children)
			{
				child.Generate(createFactory);
			}
		}
	}
}

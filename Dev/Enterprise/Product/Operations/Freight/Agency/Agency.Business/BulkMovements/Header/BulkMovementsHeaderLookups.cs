using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class BulkMovementsHeaderLookups : ZLookups
	{
		public BulkMovementsHeaderLookups(BulkMovementsHeader parent)
			: base(parent) { }

		public ContainerMovementTypes MovementCodeList
		{
			get { return movementCodeList ?? (movementCodeList = Factory.GetCachedValue<ContainerMovementTypes>()); }
		}
		ContainerMovementTypes movementCodeList;

		public OrganisationsFindBoxCollection DepotOrgList
		{
			get { return ContainerMovementTypes.GetDepotLookupCollection(Factory, Parent.MovementType); }
		}

		public OrganisationsFindBoxCollection Principals
		{
			get { return new ShipsAgencyPrincipalCollection(Factory); }
		}

		public OrganisationsFindBoxCollection ResponsibleParties
		{
			get { return new OrganisationsFindBoxCollection(Factory); }
		}

		#region Implementation

		protected new BulkMovementsHeader Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (BulkMovementsHeader)base.Parent; }
		}

		#endregion
	}
}

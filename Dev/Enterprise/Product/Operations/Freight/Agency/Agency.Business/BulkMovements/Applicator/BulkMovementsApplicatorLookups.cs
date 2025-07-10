using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class BulkMovementsApplicatorLookups : ZLookups
	{
		public BulkMovementsApplicatorLookups(BulkMovementsApplicator parent)
			: base(parent) { }

		public ContainerMovementTypes MovementTypes
		{
			get { return movementTypes ?? (movementTypes = Factory.GetCachedValue<ContainerMovementTypes>()); }
		}
		ContainerMovementTypes movementTypes;

		public OrganisationsFindBoxCollection Depots
		{
			get { return ContainerMovementTypes.GetDepotLookupCollection(Factory, Parent.MovementType); }
		}

		#region Implementation

		protected new BulkMovementsApplicator Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (BulkMovementsApplicator)base.Parent; }
		}

		#endregion
	}
}

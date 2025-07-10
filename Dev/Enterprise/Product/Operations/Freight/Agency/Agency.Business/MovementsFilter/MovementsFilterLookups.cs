using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class MovementsFilterLookups : ZLookups
	{
		public MovementsFilterLookups(MovementsFilter parent)
			: base(parent) { }

		public ContainerMovementTypes MovementTypes
		{
			get { return movementTypes ?? (movementTypes = Factory.GetCachedValue<ContainerMovementTypes>()); }
		}
		ContainerMovementTypes movementTypes;

		public OrganisationsFindBoxCollection Depots
		{
			get { return new OrganisationsFindBoxCollection(Factory); }
		}

		public BillOfLadingCollection Shipments
		{
			get { return new BillOfLadingCollection(Factory); }
		}

		public RefVesselCollection Vessels
		{
			get { return new RefVesselCollection(Factory); }
		}

		#region Implementation

		protected new MovementsFilter Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (MovementsFilter)base.Parent; }
		}

		#endregion
	}
}



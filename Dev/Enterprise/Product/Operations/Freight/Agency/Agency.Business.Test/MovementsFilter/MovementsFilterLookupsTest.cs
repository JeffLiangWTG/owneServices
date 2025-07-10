using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class MovementsFilterLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMovementTypes()
		{
			AssertType(typeof(ContainerMovementTypes), Filter.Lookups.MovementTypes);
		}

		#region Implementation
		MovementsFilter Filter
		{
			get
			{
				return filter ?? (filter = new MovementsFilter(Factory, new CollectionRelationship(typeof(ContainerMovement))));
			}
		}

		MovementsFilter filter;
		#endregion
	}
}

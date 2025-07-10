using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AllocationCalcWrapper))]
	internal class AllocationCalcWrapperBOTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			return new AllocationCalcWrapper(shipment);
		}
		#endregion
	}
}

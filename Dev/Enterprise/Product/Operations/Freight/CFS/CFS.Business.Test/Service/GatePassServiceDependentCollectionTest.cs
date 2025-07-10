using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(GatePassServiceDependentCollection))]
	public class GatePassServiceDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCollectionIsReadOnly()
		{
			Assert(GatePassServiceCollection.ReadOnly);
		}

		public void TestCollectionDoesNotAllowNew()
		{
			AssertEquals(false, GatePassServiceCollection.AllowNew);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return GatePassServiceCollection;
		}

		GatePassServiceDependentCollection GatePassServiceCollection
		{
			get
			{
				if (fGatePassServiceCollection == null)
				{
					GatePassShipment shipment = Factory.New<GatePassShipment>();
					fGatePassServiceCollection = new GatePassServiceDependentCollection(shipment.DocsAndCartage, shipment.Factory);
				}

				return fGatePassServiceCollection;
			}
		}
		GatePassServiceDependentCollection fGatePassServiceCollection;

		#endregion
	}
}

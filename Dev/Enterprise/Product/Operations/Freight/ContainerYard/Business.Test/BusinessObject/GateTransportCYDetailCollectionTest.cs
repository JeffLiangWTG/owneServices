using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.ContainerYard.Business;
using NUnit.Framework;

namespace Enterprise.Freight.ContainerYard.Testing
{
	[TestedType(typeof(GateTransportCYDetailCollection))]
	sealed class GateTransportCYDetailCollectionTest : ActiveBusinessObjectCollectionTestCase<GateTransportCYDetailCollection>
	{
		#region Implementation

		protected override GateTransportCYDetailCollection GetCollectionToTest()
		{
			var gateTransport = Factory.NewWithValidTestData<GateTransport>();
			return new GateTransportCYDetailCollection(gateTransport);
		}

		#endregion
	}
}

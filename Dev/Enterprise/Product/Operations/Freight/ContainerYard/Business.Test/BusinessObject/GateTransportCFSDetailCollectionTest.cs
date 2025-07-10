using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.ContainerYard.Business;
using NUnit.Framework;

namespace Enterprise.Freight.ContainerYard.Testing
{
	[TestedType(typeof(GateTransportCFSDetailCollection))]
	sealed class GateTransportCFSDetailCollectionTest : ActiveBusinessObjectCollectionTestCase<GateTransportCFSDetailCollection>
	{
		#region Implementation

		protected override GateTransportCFSDetailCollection GetCollectionToTest()
		{
			var gateTransport = Factory.NewWithValidTestData<GateTransport>();
			return new GateTransportCFSDetailCollection(gateTransport);
		}

		#endregion
	}
}

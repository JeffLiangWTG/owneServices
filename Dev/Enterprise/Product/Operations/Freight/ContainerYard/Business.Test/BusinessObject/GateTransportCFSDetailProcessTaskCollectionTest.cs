using Enterprise.Freight.ContainerYard.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.ContainerYard.Testing
{
	[TestedType(typeof(GateTransportCFSDetailProcessTaskCollection))]
	sealed class GateTransportCFSDetailProcessTaskCollectionTest : ProcessTaskCollectionTest<GateTransportCFSDetailProcessTaskCollection>
	{
		protected override GateTransportCFSDetailProcessTaskCollection GetCollectionToTestCore()
		{
			var parent = Factory.New<GateTransportCFSDetail>();
			return new GateTransportCFSDetailProcessTaskCollection(parent);
		}
	}
}

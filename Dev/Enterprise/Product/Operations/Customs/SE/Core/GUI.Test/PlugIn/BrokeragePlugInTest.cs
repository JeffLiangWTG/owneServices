using Enterprise.Customs.Business;
using Enterprise.Customs.SE.Business.Declaration;

namespace Enterprise.Customs.SE.GUI.Testing
{
	sealed class BrokeragePlugInTest : EU.GUI.Testing.BrokeragePlugInTest
	{
		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugInToTest() => new BrokeragePlugIn(Shipment);

		protected override BaseJobDeclaration GetDeclaration() => Factory.New<JobDeclaration>();
	}
}

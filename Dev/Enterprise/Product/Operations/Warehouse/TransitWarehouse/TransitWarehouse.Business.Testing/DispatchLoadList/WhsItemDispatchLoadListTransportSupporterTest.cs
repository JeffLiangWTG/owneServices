using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Security;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public class WhsItemDispatchLoadListTransportSupporterTest : TransportSupporterTestCase<WhsItemDispatchLoadListTransportSupporter>
	{
		protected override SecurityCheckpoint ExpectedDistanceCalculationCheckpoint => Env.Security.RoadDistanceCalculationServiceTransitWarehouse;

		protected override ZString TestingCountry => Core.Constants.CountryCodes.Australia;

		protected override TransportSupporter GetNewTransportSupporter()
		{
			ITransportParent parent = Factory.New<WhsItemDispatchLoadList>();
			return parent.TransportSupporter;
		}
	}
}

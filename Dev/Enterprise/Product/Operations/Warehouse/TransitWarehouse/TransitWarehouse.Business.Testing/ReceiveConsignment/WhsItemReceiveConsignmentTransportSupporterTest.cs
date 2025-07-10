using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Security;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public class WhsItemReceiveConsignmentTransportSupporterTest : TransportSupporterTestCase<WhsItemReceiveConsignmentTransportSupporter>
	{
		protected override SecurityCheckpoint ExpectedDistanceCalculationCheckpoint => Env.Security.RoadDistanceCalculationServiceTransitWarehouse;

		protected override ZString TestingCountry => Core.Constants.CountryCodes.Australia;

		protected override TransportSupporter GetNewTransportSupporter()
		{
			ITransportParent parent = Factory.New<WhsItemReceiveConsignment>();
			return parent.TransportSupporter;
		}
	}
}

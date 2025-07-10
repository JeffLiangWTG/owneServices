using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Security;

namespace Enterprise.Freight.CFS.Business.Testing
{
	internal sealed class CFSLoadListConsolTransportSupporterTest : TransportSupporterTestCase<CFSLoadListConsolTransportSupporter>
	{
		protected override SecurityCheckpoint ExpectedDistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceCFS; }
		}

		protected override TransportSupporter GetNewTransportSupporter()
		{
			ITransportParent parent = Factory.New<CFSLoadListConsol>();
			return parent.TransportSupporter;
		}

		protected override ZString TestingCountry
		{
			get { return Core.Constants.CountryCodes.Australia; }
		}
	}
}

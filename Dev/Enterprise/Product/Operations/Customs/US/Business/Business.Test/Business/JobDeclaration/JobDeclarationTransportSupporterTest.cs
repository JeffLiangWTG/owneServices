using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Security;

namespace Enterprise.Customs.US.Business.Testing
{
	class JobDeclarationTransportSupporterTest : TransportSupporterTestCase<JobDeclarationTransportSupporter>
	{
		public void TestUpdatePortOfArrivalDataByFindingSuitableTransportIfMultiTransports()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("", declaration.JE_RL_NKPortOfArrival);

			_ = ((ITransportParent)declaration).TransportSupporter;

			var transport1 = declaration.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "HKHKG";
			transport1.JW_RL_NKDiscPort = "GBLON";
			transport1.JW_LegOrder = 1;
			AssertEquals("GBLON", declaration.JE_RL_NKPortOfArrival);

			var transport2 = declaration.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "GBLON";
			transport2.JW_RL_NKDiscPort = "USPHL";
			transport2.JW_LegOrder = 2;
			AssertEquals("should show USPHL by using first Routing leg of the destination country", "USPHL", declaration.JE_RL_NKPortOfArrival);
		}

		protected override ZString TestingCountry => Core.Constants.CountryCodes.UnitedStates;

		protected override SecurityCheckpoint ExpectedDistanceCalculationCheckpoint => Env.Security.RoadDistanceCalculationServiceCustoms;

		protected override TransportSupporter GetNewTransportSupporter()
		{
			ITransportParent parent = Factory.New<JobDeclaration>();
			return parent.TransportSupporter;
		}
	}
}

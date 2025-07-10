using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseJobDeclarationTransportValidationTest : BusinessObjectValidationTestCase
	{
		public void TestJW_RL_NKLoadPort()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_RL_NKOrigin = "AUBNE";
			declaration.JE_RL_NKFinalDestination = "NLAMS";
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_VoyageFlightNo = "QF202";
			declaration.JE_ExportDate = new ZDateTime(2009, 1, 2);
			declaration.JE_DateOfArrival = new ZDateTime(2009, 1, 3);

			Transport transport = declaration.Transports[0];
			transport.JW_RL_NKLoadPort = "NLAMS";
			AssertHasError(transport.JW_RL_NKLoadPortInfo, "A routing leg cannot load at the declaration's destination.");

			transport.JW_RL_NKLoadPort = "GBLON";
			AssertNoErrors(transport.JW_RL_NKLoadPortInfo);
		}

		public void TestJW_RL_NKDiscPort()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_RL_NKOrigin = "AUBNE";
			declaration.JE_RL_NKFinalDestination = "NLAMS";
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_VoyageFlightNo = "QF202";
			declaration.JE_ExportDate = new ZDateTime(2009, 1, 2);
			declaration.JE_DateOfArrival = new ZDateTime(2009, 1, 3);

			Transport transport = declaration.Transports[0];
			transport.JW_RL_NKDiscPort = "AUBNE";
			AssertHasError(transport.JW_RL_NKDiscPortInfo, "A routing leg cannot discharge at the declaration's origin.");

			transport.JW_RL_NKDiscPort = "GBLON";
			AssertNoErrors(transport.JW_RL_NKDiscPortInfo);
		}
	}
}

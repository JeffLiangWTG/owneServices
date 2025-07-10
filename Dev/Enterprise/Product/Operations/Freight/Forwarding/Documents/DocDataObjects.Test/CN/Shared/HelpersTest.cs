using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;

namespace Enterprise.Freight.Forwarding.Documents.CN.Testing
{
	sealed class HelpersTest : TestCaseWithFactory
	{
		#region TestGetOperationalPort

		public void TestGetOperationalPort()
		{
			var context = new CommonContext(Factory);
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "CNSGH";
			consol.JK_RL_NKDischargePort = "SGSIN";

			var transport1 = consol
				.Transports
				.OfType<Freight.Business.Transport>()
				.Single();

			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_RL_NKLoadPort = "CNSGH";
			transport1.JW_RL_NKDiscPort = "CNNBO";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_RL_NKLoadPort = "CNNBO";
			transport2.JW_RL_NKDiscPort = "TWTPE";

			var transport3 = consol.Transports.AddNew();
			transport3.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport3.JW_RL_NKLoadPort = "TWTPE";
			transport3.JW_RL_NKDiscPort = "SGSIN";

			var operationalPort = consol.GetOperationalPort(context);

			AssertEquals("operationalPort.Code", "CNSGH", operationalPort.Code);
			AssertEquals("operationalPort.Name", "Shanghai", operationalPort.Name);
		}

		public void TestGetOperationalPort_NullConsol()
		{
			ForwardingConsol consol = null;
			var context = new CommonContext(Factory);

			var operationalPort = consol.GetOperationalPort(context);

			AssertEquals("operationalPort.Code", "", operationalPort.Code);
			AssertEquals("operationalPort.Name", "", operationalPort.Name);
		}

		public void TestGetOperationalPort_NullContext()
		{
			var consol = Factory.New<ForwardingConsol>();
			IContext context = null;

			var operationalPort = consol.GetOperationalPort(context);
			AssertNull("operationalPort", operationalPort);
		}

		#endregion
	}
}

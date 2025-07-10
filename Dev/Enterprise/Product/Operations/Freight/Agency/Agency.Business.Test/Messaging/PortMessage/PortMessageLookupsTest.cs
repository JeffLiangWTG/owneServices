using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class PortMessageLookupsTest : BaseAgencyTest
	{
		public void TestPortList()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "UAIEV";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NZAKL";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";

			var portMessage = new PortMessage(voyage);
			var lookup = new PortMessageLookups(portMessage);

			AssertEquals("Should include all ports with PA settings", "AUSYD, NZAKL, UAIEV", lookup.Port_List.CodesAsString);
		}

		public void TestDirectionList()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "UAIEV";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "UAIEV";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";

			var portMessage = new PortMessage(voyage);
			var lookup = new PortMessageLookups(portMessage);

			portMessage.Port = "";
			AssertEquals("invalid port", "", lookup.Direction_List.CodesAsString);

			portMessage.Port = "UAIEV";
			AssertEquals("port with both load and discharge", "LOAD, DISCHARGE", lookup.Direction_List.CodesAsString);

			portMessage.Port = "AUSYD";
			AssertEquals("port with just load", "LOAD", lookup.Direction_List.CodesAsString);

			portMessage.Port = "NZAKL";
			AssertEquals("port with just discharge", "DISCHARGE", lookup.Direction_List.CodesAsString);

			portMessage.Port = "SGSIN";
			AssertEquals("invalid port", "", lookup.Direction_List.CodesAsString);
		}
	}
}

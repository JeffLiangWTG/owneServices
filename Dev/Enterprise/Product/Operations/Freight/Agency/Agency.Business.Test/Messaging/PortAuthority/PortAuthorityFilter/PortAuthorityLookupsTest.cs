using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal sealed class PortAuthorityLookupsTest : BaseAgencyTest
	{
		public void TestPortList_DeliverTo3rdPartIsFalse_ApplySettingsToPortList()
		{
			SetPortAuthoritySettings("AUBNE", "AUMEL");

			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUMEL";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "SGSIN";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "MYBAG";

			var portMessage = new PortAuthority(voyage);
			portMessage.DeliverTo3rdParty = false;

			var lookup = new PortAuthorityLookups(portMessage);
			AssertEquals("Should include all ports with PA settings", "AUBNE, AUMEL", lookup.Port_List.CodesAsString);
		}

		public void TestPortList_DeliverTo3rdPartIsTrue_ReturnPortListAsItIs()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUMEL";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "SGSIN";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "MYBAG";

			var portMessage = new PortMessage(voyage);
			portMessage.DeliverTo3rdParty = true;

			var lookup = new PortMessageLookups(portMessage);
			AssertEquals("Should include all ports with PA settings", "AUBNE, AUMEL, MYBAG, NZAKL, SGSIN", lookup.Port_List.CodesAsString);
		}

		public void TestPort_ListOnlyAppearForPortsConfiguredInRegistryAndLoginCompayIsAU()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			SetPortAuthoritySettings("AUBNE", "AUMEL");

			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUMEL";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "SGSIN";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "MYBAG";

			var portMessage = new PortAuthority(voyage);
			portMessage.DeliverTo3rdParty = false;

			var lookup = new PortAuthorityLookups(portMessage);
			AssertEquals("Should include all ports with PA settings", "AUBNE, AUMEL", lookup.Port_List.CodesAsString);

			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.AlandIslands);
			lookup = new PortAuthorityLookups(portMessage);
			AssertEquals("", lookup.Port_List.CodesAsString);
		}
	}
}

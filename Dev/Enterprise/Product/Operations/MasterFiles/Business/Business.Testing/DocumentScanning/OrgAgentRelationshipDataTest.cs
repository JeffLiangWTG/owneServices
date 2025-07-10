using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	class OrgAgentRelationshipDataTest : TestCaseWithFactory
	{
		public void TestEDocsViaUniversalXml()
		{
			var assemblyData = new OrgAgentRelationshipData();
			var edocsAdapter = assemblyData.GetEDocsViaUniversalXmlSupport();

			var onlySending = CreateAgentRelationship("SENDAGENT");
			var onlyReceiving = CreateAgentRelationship(null, "RECVAGENT");
			var onlyHead = CreateAgentRelationship(null, null, "HEAD");
			var all = CreateAgentRelationship("A", "B", "C");

			AssertNull(edocsAdapter.LoadBusinessObjectFromCode(Factory, ""));
			AssertNull(edocsAdapter.LoadBusinessObjectFromCode(Factory, "----invalid"));
			AssertNull(edocsAdapter.LoadBusinessObjectFromCode(Factory, "SENDAGENT-DOESNTEXIST-ORTHIS"));
			AssertEquals(onlySending, edocsAdapter.LoadBusinessObjectFromCode(Factory, "SENDAGENT"));
			AssertEquals(onlySending, edocsAdapter.LoadBusinessObjectFromCode(Factory, "SENDAGENT--"));
			AssertEquals(onlyReceiving, edocsAdapter.LoadBusinessObjectFromCode(Factory, "-RECVAGENT-"));
			AssertEquals(onlyReceiving, edocsAdapter.LoadBusinessObjectFromCode(Factory, "-RECVAGENT"));
			AssertEquals(onlyHead, edocsAdapter.LoadBusinessObjectFromCode(Factory, "--HEAD"));
			AssertEquals(all, edocsAdapter.LoadBusinessObjectFromCode(Factory, "A-B-C"));
			AssertNull(edocsAdapter.LoadBusinessObjectFromCode(Factory, "A--C"));
		}

		OrgAgentRelationship CreateAgentRelationship(string sendingAgentCode, string receivingAgentCode = null, string headOfficeCode = null)
		{
			OrgHeader sendingAgent = null, receivingAgent = null, headOffice = null;

			if (sendingAgentCode != null)
			{
				sendingAgent = Factory.NewWithValidTestData<OrgHeader>();
				sendingAgent.OH_Code = sendingAgentCode;
			}

			if (receivingAgentCode != null)
			{
				receivingAgent = Factory.NewWithValidTestData<OrgHeader>();
				receivingAgent.OH_Code = receivingAgentCode;
			}

			if (headOfficeCode != null)
			{
				headOffice = Factory.NewWithValidTestData<OrgHeader>();
				headOffice.OH_Code = headOfficeCode;
			}

			var agentRelationship = Factory.New<OrgAgentRelationship>();
			agentRelationship.O3_OH_ReceivingAgent = receivingAgent?.PK ?? ZGuid.Empty;
			agentRelationship.O3_OH_SendingAgent = sendingAgent?.PK ?? ZGuid.Empty;
			agentRelationship.O3_OH_GroupNetworkOrFranchise = headOffice?.PK ?? ZGuid.Empty;
			return agentRelationship;
		}
	}
}

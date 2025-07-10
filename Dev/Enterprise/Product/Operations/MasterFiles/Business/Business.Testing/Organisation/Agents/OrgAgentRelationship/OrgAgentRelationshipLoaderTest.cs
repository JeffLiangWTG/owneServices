using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgAgentRelationship.Loader))]
	sealed class OrgAgentRelationshipLoaderTest : LoaderTestCase
	{
		public void TestLoader_StandardProfile()
		{
			var relationships = new Dictionary<string, OrgAgentRelationship>();

			OrgHeader CreateOrgHeader(string code)
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.OH_Code = code;
				return orgHeader;
			}

			var sendingAgent1 = CreateOrgHeader("SND1");
			var receivingAgent1 = CreateOrgHeader("RCV1");
			var pickupAgent1 = CreateOrgHeader("PUA1");

			var sendingAgent2 = CreateOrgHeader("SND2");
			var receivingAgent2 = CreateOrgHeader("RCV2");
			var pickupAgent2 = CreateOrgHeader("PUA2");

			void CreateProfitShareSetups(OrgHeader sendingAgent, OrgHeader receivingAgent)
			{
				var relationship = Factory.New<OrgAgentRelationship>();

				relationship.O3_OH_SendingAgent = sendingAgent?.PK ?? ZGuid.Empty;
				var sendingAgentCode = sendingAgent?.OH_Code ?? "null";

				relationship.O3_OH_ReceivingAgent = receivingAgent?.PK ?? ZGuid.Empty;
				var receivingAgentCode = receivingAgent?.OH_Code ?? "null";

				var setup0 = relationship.ClientSpecificProfitShareDetails.AddNew();
				setup0.O4_OH_OrgOverride = ZGuid.Empty;
				setup0.O4_OrgOverrideType = "ALL";
				// The same relationship will be added 3 times with 3 different pickup agents.
				relationships.Add($"{sendingAgentCode}|{receivingAgentCode}|null", relationship);

				var setup1 = relationship.ClientSpecificProfitShareDetails.AddNew();
				setup1.O4_OH_OrgOverride = pickupAgent1.PK;
				setup1.O4_OrgOverrideType = "PUA";
				relationships.Add($"{sendingAgentCode}|{receivingAgentCode}|{pickupAgent1.OH_Code}", relationship);

				var setup2 = relationship.ClientSpecificProfitShareDetails.AddNew();
				setup2.O4_OH_OrgOverride = pickupAgent2.PK;
				setup2.O4_OrgOverrideType = "PUA";
				relationships.Add($"{sendingAgentCode}|{receivingAgentCode}|{pickupAgent2.OH_Code}", relationship);
			}

			CreateProfitShareSetups(sendingAgent1, receivingAgent1);
			CreateProfitShareSetups(sendingAgent1, receivingAgent2);
			CreateProfitShareSetups(sendingAgent1, null);
			CreateProfitShareSetups(sendingAgent2, receivingAgent1);
			CreateProfitShareSetups(sendingAgent2, receivingAgent2);
			CreateProfitShareSetups(sendingAgent2, null);
			CreateProfitShareSetups(null, receivingAgent1);
			CreateProfitShareSetups(null, receivingAgent2);

			Factory.Save();

			var loader = new OrgAgentRelationship.Loader(Factory);

			AssertEquals(relationships["SND1|RCV1|null"], loader.Load(sendingAgent1, receivingAgent1, null));
			AssertEquals(relationships["SND1|RCV1|PUA1"], loader.Load(sendingAgent1, receivingAgent1, pickupAgent1));
			AssertEquals(relationships["SND1|RCV2|PUA1"], loader.Load(sendingAgent1, receivingAgent2, pickupAgent1));
			AssertEquals(relationships["SND1|null|null"], loader.Load(sendingAgent1, null, null));
			AssertEquals(relationships["SND1|null|PUA1"], loader.Load(sendingAgent1, null, pickupAgent1));
			AssertEquals(relationships["SND1|null|PUA2"], loader.Load(sendingAgent1, null, pickupAgent2));
			AssertEquals(relationships["null|RCV1|PUA1"], loader.Load(null, receivingAgent1, pickupAgent1));
			AssertEquals(relationships["null|RCV2|null"], loader.Load(null, receivingAgent2, null));
			AssertEquals(relationships["null|RCV2|PUA2"], loader.Load(null, receivingAgent2, pickupAgent2));
		}

		public void TestLoader_AgencyProfile()
		{
			OrgHeader agent = Factory.NewWithValidTestData<OrgHeader>();
			agent.OH_Code = "RCV";

			OrgHeader agent2 = Factory.NewWithValidTestData<OrgHeader>();
			agent2.OH_Code = "SEND";

			OrgAgentRelationship rel1 = Factory.New<OrgAgentRelationship>();
			rel1.O3_OH_SendingAgent = agent.PK;
			rel1.O3_ProfitShareType = OrgAgentRelationship.ProfitShareTypes.AgencyProfile;

			OrgAgentRelationship rel2 = Factory.New<OrgAgentRelationship>();
			rel2.O3_OH_SendingAgent = agent2.PK;
			rel2.O3_ProfitShareType = OrgAgentRelationship.ProfitShareTypes.AgencyProfile;

			Factory.Save();
			AssertEquals(rel1, new OrgAgentRelationship.Loader(Factory).LoadAgencyProfile(agent));
			AssertEquals(rel2, new OrgAgentRelationship.Loader(Factory).LoadAgencyProfile(agent2));

			rel1.O3_ProfitShareType = OrgAgentRelationship.ProfitShareTypes.Standard;
			Factory.Save();
			AssertNull(new OrgAgentRelationship.Loader(Factory).LoadAgencyProfile(agent));
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new OrgAgentRelationship.Loader(Factory);
		}
	}
}

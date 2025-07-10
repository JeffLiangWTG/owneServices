using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgAgentRelationship))]
	sealed class OrgAgentRelationshipTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTemplateCopy()
		{
			OrgHeader recAgent = Factory.New<OrgHeader>();
			recAgent.OH_Code = "RCV";

			OrgHeader sendAgent = Factory.New<OrgHeader>();
			sendAgent.OH_Code = "SEND";

			OrgAgentRelationship relationship = Factory.New<OrgAgentRelationship>();
			relationship.O3_OH_SendingAgent = sendAgent.PK;
			relationship.O3_OH_ReceivingAgent = recAgent.PK;

			OrgProfitShareDetails details = relationship.ProfitShareDetails.AddNew();
			details.PartyDetails.AddNew();
			details.PartyDetails.AddNew();

			OrgProfitShareDetails details2 = relationship.ProfitShareDetails.AddNew();
			details2.PartyDetails.AddNew();

			OrgAgentRelationship copied = (OrgAgentRelationship)((ITemplateCopyable)relationship).TemplateCopy();
			AssertEquals(sendAgent.PK, copied.O3_OH_SendingAgent);
			AssertEquals(recAgent.PK, copied.O3_OH_ReceivingAgent);
			AssertEquals(2, copied.ProfitShareDetails.Count);
			AssertEquals(2, copied.ProfitShareDetails[0].PartyDetails.Count);
			AssertEquals(1, copied.ProfitShareDetails[1].PartyDetails.Count);
		}

		public void TestDescription()
		{
			OrgHeader recAgent = Factory.New<OrgHeader>();
			recAgent.OH_Code = "RCV";

			OrgHeader sendAgent = Factory.New<OrgHeader>();
			sendAgent.OH_Code = "SEND";

			OrgAgentRelationship relationship = Factory.New<OrgAgentRelationship>();
			relationship.O3_OH_SendingAgent = ZGuid.Empty;
			relationship.O3_OH_ReceivingAgent = ZGuid.Empty;
			AssertEquals("All Agents - All Agents", relationship.Description);

			relationship.O3_OH_ReceivingAgent = recAgent.PK;
			AssertEquals("All Agents - RCV", relationship.Description);

			relationship.O3_OH_SendingAgent = sendAgent.PK;
			AssertEquals("SEND - RCV", relationship.Description);

			relationship.O3_OH_ReceivingAgent = ZGuid.Empty;
			AssertEquals("SEND - All Agents", relationship.Description);
		}

		public void TestLogging()
		{
			OrgHeader recAgent = Factory.NewWithValidTestData<OrgHeader>();
			recAgent.OH_FullName = "ppp";
			OrgHeader sendAgent = Factory.NewWithValidTestData<OrgHeader>();
			sendAgent.OH_FullName = "lll";

			OrgAgentRelationship relationship = Factory.NewWithValidTestData<OrgAgentRelationship>();
			relationship.O3_OH_ReceivingAgent = recAgent.PK;
			relationship.O3_OH_SendingAgent = sendAgent.PK;

			Factory.Save();

			AssertEquals("Should have one log", 1, relationship.Logs.GetAllLogs().Count);
			ZString expectedReference = "Receiving Agent: " + recAgent.OH_FullName + " Sending Agent: " + sendAgent.OH_FullName;
			AssertEquals("Log should have reference", expectedReference, relationship.Logs.MostRecentLogByEventTime(Events.AddedARecordToTheSystem).SL_Reference);
		}

		public void TestOrgAgentRelationshipLogging_IfOnlyChildrenHaveChanges()
		{
			var recAgent = Factory.NewWithValidTestData<OrgHeader>();
			recAgent.OH_FullName = "ppp";
			var sendAgent = Factory.NewWithValidTestData<OrgHeader>();
			sendAgent.OH_FullName = "lll";

			var relationship = Factory.NewWithValidTestData<OrgAgentRelationship>();
			relationship.O3_OH_ReceivingAgent = recAgent.PK;
			relationship.O3_OH_SendingAgent = sendAgent.PK;
			Factory.Save();

			var detail = relationship.ProfitShareDetails.AddNew();
			detail.O4_FreightMode = "ALL";
			Factory.Save();

			var expectedReference = "Receiving Agent: " + recAgent.OH_FullName + " Sending Agent: " + sendAgent.OH_FullName;
			var editLog = relationship.Logs.MostRecentLogByEventTime(Events.EditedARecord);
			AssertNotNull("Edit log of relationship", editLog);
			AssertEquals("Log should have reference", expectedReference, editLog.SL_Reference);
		}

		public void TestSendingAgentDefaults()
		{
			AssertNotNull("Precondition: Current company's org proxy exists", GlbCompany.CurrentCompany.OrgProxy);

			OrgAgentRelationship relationship = Factory.New<OrgAgentRelationship>();
			AssertEquals("Sending Agent is set to current company's org proxy", GlbCompany.CurrentCompany.OrgProxy.PK, relationship.SendingAgent.PK);
		}

		public void TestProfitSharePropagation()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgAgentRelationship relationship = Factory.New<OrgAgentRelationship>();

			OrgProfitShareDetails prof1 = relationship.ProfitShareDetails.AddNew();
			OrgProfitShareParty profi1Party1 = prof1.PartyDetails.AddNew();
			profi1Party1.PS_PartyType = "CON";
			profi1Party1.PS_PartyRate = 500m;

			OrgProfitShareParty profi1Party2 = prof1.PartyDetails.AddNew();
			profi1Party2.PS_PartyType = "SEN";
			profi1Party2.PS_PartyRate = 300m;

			prof1.O4_OH_ControllingAgent = org.PK;
			AssertEquals(500m, profi1Party1.PS_PartyRate);
			AssertEquals(300m, profi1Party2.PS_PartyRate);

			relationship.O3_OH_SendingAgent = org.PK;
			AssertEquals(300m, profi1Party1.PS_PartyRate);
			AssertEquals(300m, profi1Party2.PS_PartyRate);
		}

		public void TestO3_ProfitShareType()
		{
			var recvAgent = Factory.New<OrgHeader>();
			var relationship = Factory.New<OrgAgentRelationship>();
			relationship.O3_OH_ReceivingAgent = recvAgent.PK;

			relationship.O3_ProfitShareType = OrgAgentRelationship.ProfitShareTypes.AgencyProfile;
			var profitShareDetails = relationship.ProfitShareDetails.AddNew();
			profitShareDetails.O4_GatewayAgentType = "GCN";
			profitShareDetails.O4_GatewayProfitApportionmentMethod = "SHP";
			AssertEquals(ZGuid.Empty, relationship.O3_OH_ReceivingAgent);

			relationship.O3_OH_ReceivingAgent = recvAgent.PK;
			relationship.O3_ProfitShareType = OrgAgentRelationship.ProfitShareTypes.Standard;
			AssertEquals(recvAgent.PK, relationship.O3_OH_ReceivingAgent);
			AssertEquals("Since type is STD, apportionment method should be empty", ZString.Empty, relationship.ProfitShareDetails[0].O4_GatewayProfitApportionmentMethod);
		}

		#region ReadOnly Security

		public void TestReadOnlySecurity()
		{
			bool oldProfitShareValue = Env.Security.OrgForwarderModifyProfitShare.IsAllowed;

			try
			{
				OrgAgentRelationship testRelationship = OrgInDB.AgentRelationships.AddNew();
				OrgInDB.AgentRelationships.SetOrganisationReadOnly(OrgInDB);

				Env.Security.OrgForwarderModifyProfitShare.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !testRelationship.O3_OH_GroupNetworkOrFranchiseInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testRelationship.O3_OH_ReceivingAgentInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testRelationship.O3_OH_SendingAgentInfo.ReadOnly);

				Env.Security.OrgForwarderModifyProfitShare.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", testRelationship.O3_OH_GroupNetworkOrFranchiseInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testRelationship.O3_OH_ReceivingAgentInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testRelationship.O3_OH_SendingAgentInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgForwarderModifyProfitShare.IsAllowed = oldProfitShareValue;
			}
		}

		public void TestProfitShareCollectionIsReadOnly()
		{
			bool oldProfitShareValue = Env.Security.OrgForwarderModifyProfitShare.IsAllowed;
			AssertNotNull("PRE: Touch CompanyData", OrgInDB.CompanyData);
			Factory.Save();
			try
			{
				OrgAgentRelationship testRelationship = OrgInDB.AgentRelationships.AddNew();

				Env.Security.OrgForwarderModifyProfitShare.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !testRelationship.ProfitShareDetails.ReadOnly);

				Env.Security.OrgForwarderModifyProfitShare.IsAllowed = false;
				ResetOrgInDB();
				testRelationship = OrgInDB.AgentRelationships.AddNew();
				Assert("Access Disallowed - ReadOnly", testRelationship.ProfitShareDetails.ReadOnly);
			}
			finally
			{
				Env.Security.OrgForwarderModifyProfitShare.IsAllowed = oldProfitShareValue;
			}
		}

		public void TestDocManagerInfo()
		{
			var relationship = Factory.New<OrgAgentRelationship>();

			AssertEquals("DocManagerInfo's DocManagerCode should be OAR.", "OAR", ((IDocManagerSupport)relationship).DocManagerInfo.DocManagerCode);
			AssertEquals("DocManagerInfo's BusinessEntity should be our relationship.", relationship, ((IDocManagerSupport)relationship).DocManagerInfo.BusinessEntity);
		}

		OrgHeader OrgInDB
		{
			get
			{
				if (fOrgInDB == null)
				{
					ResetOrgInDB();
				}

				return fOrgInDB;
			}
		}
		OrgHeader fOrgInDB;

		void ResetOrgInDB()
		{
			fOrgInDB = new BusinessObjectFactory().LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
		}

		#endregion
	}
}

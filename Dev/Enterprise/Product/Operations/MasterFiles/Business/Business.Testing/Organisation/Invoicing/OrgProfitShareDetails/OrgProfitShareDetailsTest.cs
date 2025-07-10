using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using FluentAssertions;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgProfitShareDetails))]
	sealed class OrgProfitShareDetailsTest : EnterpriseBusinessObjectTestCase
	{
		public void TestValuesPropagateToRelatedParty()
		{
			OrgHeader org = Factory.New<OrgHeader>();

			OrgAgentRelationship relationship = Factory.New<OrgAgentRelationship>();
			relationship.O3_OH_SendingAgent = org.PK;
			OrgProfitShareDetails profitShare = relationship.ProfitShareDetails.AddNew();

			OrgProfitShareParty sendParty = profitShare.PartyDetails.AddNew();
			sendParty.PS_PartyType = "SEN";

			OrgProfitShareParty conParty = profitShare.PartyDetails.AddNew();
			conParty.PS_PartyType = "CON";

			sendParty.PS_PartyRate = 100m;
			sendParty.PS_PartyRateBasis = "XXX";
			sendParty.PS_PartyProfitSharePercent = 20m;

			conParty.PS_PartyRate = 200m;
			conParty.PS_PartyRateBasis = "ZZZ";
			conParty.PS_PartyProfitSharePercent = 40m;

			AssertEquals(100m, sendParty.PS_PartyRate);
			AssertEquals("XXX", sendParty.PS_PartyRateBasis);
			AssertEquals(20m, sendParty.PS_PartyProfitSharePercent);

			AssertEquals(200m, conParty.PS_PartyRate);
			AssertEquals("ZZZ", conParty.PS_PartyRateBasis);
			AssertEquals(40m, conParty.PS_PartyProfitSharePercent);

			profitShare.O4_OH_ControllingAgent = org.PK;

			AssertEquals("Should be the same as the sending agent", 100m, conParty.PS_PartyRate);
			AssertEquals(100m, sendParty.PS_PartyRate);

			AssertEquals("Should be the same as the sending agent", "XXX", conParty.PS_PartyRateBasis);
			AssertEquals("XXX", sendParty.PS_PartyRateBasis);

			AssertEquals("Should be the same as the sending agent", 20m, conParty.PS_PartyProfitSharePercent);
			AssertEquals(20m, sendParty.PS_PartyProfitSharePercent);
		}

		public void TestDelete()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var relationship = Factory.New<OrgAgentRelationship>();
			relationship.O3_OH_SendingAgent = org.PK;
			var details = relationship.ProfitShareDetails.AddNew();

			details.O4_AgreementType = OrgProfitShareDetailsLookups.AgreementTypeUserDefined;
			var party1 = details.PartyDetails.AddNew();
			var party2 = details.PartyDetailsForGatewayProfitShareRedistribution.AddNew();
			details.AgreementTypeDescription = "FRT, ODOC";

			Factory.Save();

			var pivotsQuery = new ZQuery(GenPivotSchema.XX_Relation1ID, details.PK);
			pivotsQuery.AddToFilter(GenPivotSchema.XX_RelationType, Core.Constants.GenPivotTypes.ProfitShareUserCharges);
			var pivots = Factory.Load<GenPivot>(pivotsQuery);

			Assert(pivots.Any());
			Assert(!pivots.Any(x => x.IsDeleted));

			details.Delete();
			Assert(details.IsDeleted);
			Assert(party1.IsDeleted);
			Assert(party2.IsDeleted);
			Assert("Pivots should be deleted", pivots.All(x => x.IsDeleted));
		}

		public void TestLogging()
		{
			var details = (OrgProfitShareDetails)GetNewBusinessObject();
			var initialBusinessObjectsWithRelatedEvents = details.BusinessObjectsWithRelatedEvents.Length;
			details.PartyDetails.AddNew();
			AssertEquals("There should be one extra business object with related logs", initialBusinessObjectsWithRelatedEvents + 1, details.BusinessObjectsWithRelatedEvents.Length);

			details.PartyDetails.AddNew();
			AssertEquals("There should be two extra business objects with related logs", initialBusinessObjectsWithRelatedEvents + 2, details.BusinessObjectsWithRelatedEvents.Length);

			details.PartyDetailsForGatewayProfitShareRedistribution.AddNew();
			AssertEquals("There should be three extra business objects with related logs", initialBusinessObjectsWithRelatedEvents + 3, details.BusinessObjectsWithRelatedEvents.Length);

			var details2 = (OrgProfitShareDetails)GetNewBusinessObject();
			AssertEquals("Profit Share Details should have the same number of initial logs when they are created", initialBusinessObjectsWithRelatedEvents, details2.BusinessObjectsWithRelatedEvents.Length);
		}

		public void TestIsChargeGroupProfitShared()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();

			OrgAgentRelationship relationship = Factory.New<OrgAgentRelationship>();
			relationship.O3_OH_SendingAgent = org.PK;
			OrgProfitShareDetails details = relationship.ProfitShareDetails.AddNew();

			var paymentTerm = new PaymentTermInfos();

			details.O4_AgreementType = OrgProfitShareDetailsLookups.AgreementTypeFreight;
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			Assert("Freight charges included", details.IsChargeGroupProfitShared(chargeCode, paymentTerm));
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			Assert("Origin charges not included", !details.IsChargeGroupProfitShared(chargeCode, paymentTerm));
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;
			Assert("Destination charges not included", !details.IsChargeGroupProfitShared(chargeCode, paymentTerm));

			details.O4_AgreementType = OrgProfitShareDetailsLookups.AgreementTypeFreightOrigin;
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			Assert("Freight charges included", details.IsChargeGroupProfitShared(chargeCode, paymentTerm));
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			Assert("Origin charges included", details.IsChargeGroupProfitShared(chargeCode, paymentTerm));
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;
			Assert("Destination charges not included", !details.IsChargeGroupProfitShared(chargeCode, paymentTerm));

			details.O4_AgreementType = OrgProfitShareDetailsLookups.AgreementTypeCollectFreight;
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			paymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, "FCA"));

			Assert("Freight Collect charges included", details.IsChargeGroupProfitShared(chargeCode, paymentTerm));

			details.O4_AgreementType = OrgProfitShareDetailsLookups.AgreementTypeCollectFreight;
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			paymentTerm.Remove(PaymentTermType.Incoterm);
			paymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, "CFR"));

			Assert("Freight Prepaid charges not included", !details.IsChargeGroupProfitShared(chargeCode, paymentTerm));
			paymentTerm.Remove(PaymentTermType.Incoterm);

			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			Assert("Origin charges not included", !details.IsChargeGroupProfitShared(chargeCode, paymentTerm));
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;
			Assert("Destination charges not included", !details.IsChargeGroupProfitShared(chargeCode, paymentTerm));

			details.O4_AgreementType = OrgProfitShareDetailsLookups.AgreementTypeFreightDestination;
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			Assert("Freight charges included", details.IsChargeGroupProfitShared(chargeCode, paymentTerm));
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			Assert("Origin charges not included", !details.IsChargeGroupProfitShared(chargeCode, paymentTerm));
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;
			Assert("Destination charges included", details.IsChargeGroupProfitShared(chargeCode, paymentTerm));

			details.O4_AgreementType = OrgProfitShareDetailsLookups.AgreementTypeAll;
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			Assert("Freight charges included", details.IsChargeGroupProfitShared(chargeCode, paymentTerm));
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			Assert("Origin charges included", details.IsChargeGroupProfitShared(chargeCode, paymentTerm));
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;
			Assert("Destination charges included", details.IsChargeGroupProfitShared(chargeCode, paymentTerm));

			details.O4_AgreementType = OrgProfitShareDetailsLookups.AgreementTypeOrigin;
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			Assert("Freight charges not included", !details.IsChargeGroupProfitShared(chargeCode, paymentTerm));
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			Assert("Origin charges included", details.IsChargeGroupProfitShared(chargeCode, paymentTerm));
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;
			Assert("Destination charges not included", !details.IsChargeGroupProfitShared(chargeCode, paymentTerm));

			details.O4_AgreementType = OrgProfitShareDetailsLookups.AgreementTypeDestination;
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			Assert("Freight charges not included", !details.IsChargeGroupProfitShared(chargeCode, paymentTerm));
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			Assert("Origin charges not included", !details.IsChargeGroupProfitShared(chargeCode, paymentTerm));
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;
			Assert("Destination charges included", details.IsChargeGroupProfitShared(chargeCode, paymentTerm));

			details.O4_AgreementType = OrgProfitShareDetailsLookups.AgreementTypeFreightChargeOnly;
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			Assert("Freight charges not included", !details.IsChargeGroupProfitShared(chargeCode, paymentTerm));
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			Assert("Origin charges not included", !details.IsChargeGroupProfitShared(chargeCode, paymentTerm));
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;
			Assert("Destination charges not included", !details.IsChargeGroupProfitShared(chargeCode, paymentTerm));

			Env.Registry.FreightChargeCode = chargeCode.PK.ToGuid();
			Assert("Freight charge included", details.IsChargeGroupProfitShared(chargeCode, paymentTerm));

			details.O4_AgreementType = OrgProfitShareDetailsLookups.AgreementTypeCustom;
			Assert("Custom charges not included", !details.IsChargeGroupProfitShared(chargeCode, paymentTerm));
			AccChargeCode chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			OrganisationRegistry.Instance.CustomProfitShareAgreementTypeChargeCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(),
					Guid.Empty, Guid.Empty, chargeCode.PK.ToString());
			Assert("Custom charges included", details.IsChargeGroupProfitShared(chargeCode, paymentTerm));
			Assert("Custom charges not included", !details.IsChargeGroupProfitShared(chargeCode2, paymentTerm));
			OrganisationRegistry.Instance.CustomProfitShareAgreementTypeChargeCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(),
					Guid.Empty, Guid.Empty, chargeCode.PK.ToString() + "," + chargeCode2.PK.ToString());
			Assert("Custom charges included", details.IsChargeGroupProfitShared(chargeCode, paymentTerm));
			Assert("Custom charges included", details.IsChargeGroupProfitShared(chargeCode2, paymentTerm));

			details.O4_AgreementType = OrgProfitShareDetailsLookups.AgreementTypeUserDefined;
			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode3 = Factory.NewWithValidTestData<AccChargeCode>();

			chargeCode1.AC_Code = "CHG1";
			chargeCode2.AC_Code = "CHG2";
			chargeCode3.AC_Code = "CHG3";

			Factory.Save();

			details.AgreementTypeDescription = string.Format("{0},{1},{2}", chargeCode1.AC_Code, chargeCode2.AC_Code, chargeCode3.AC_Code);

			Factory.Save();

			details = Factory.Load<OrgProfitShareDetails>(details.PK);

			Assert("CHG1 is profit shared", details.IsChargeGroupProfitShared(chargeCode1, paymentTerm));
			Assert("CHG2 is profit shared", details.IsChargeGroupProfitShared(chargeCode2, paymentTerm));
			Assert("CHG3 is profit shared", details.IsChargeGroupProfitShared(chargeCode3, paymentTerm));

			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);

			var secondLinkedChargeCode = globalChargeCode.ChildChargeCodes.FirstOrDefault(c => c.PK != normalChargeCodeLinked.PK);
			AssertNotEquals(normalChargeCodeLinked.PK, secondLinkedChargeCode.PK);

			details.AgreementTypeDescription = normalChargeCodeLinked.AC_Code;

			Factory.Save();

			details = Factory.Load<OrgProfitShareDetails>(details.PK);

			Assert("CC2 is profit shared", details.IsChargeGroupProfitShared(secondLinkedChargeCode, paymentTerm));

			var chargeCode4 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode4.AC_Code = "CHG4";

			Factory.Save();

			//Link CHG1 to CHG4 via InterCompany Global Charge - Account Payable

			var yg_pk = Guid.NewGuid();
			var sql = $"INSERT INTO dbo.AccGlobalChargeCodeMap (YG_PK, YG_Code, YG_Desc, YG_IsActive, YG_SystemCreateTimeUtc, YG_SystemCreateUser, YG_SystemLastEditTimeUtc, YG_SystemLastEditUser) VALUES ('{yg_pk}', '{chargeCode1.AC_Code}', 'Test', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			CargoWise.Data.Db.Connection.ExecuteNonQuery(sql);

			var sqlPivot = $"INSERT INTO dbo.AccGlobalChargeCodeMapPivot (YP_PK, YP_YG, YP_AC, YP_TYPE, YP_SystemCreateTimeUtc, YP_SystemCreateUser, YP_SystemLastEditTimeUtc, YP_SystemLastEditUser) VALUES('{Guid.NewGuid()}', '{yg_pk}', '{chargeCode4.PK.ToString()}', 'AP', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			CargoWise.Data.Db.Connection.ExecuteNonQuery(sqlPivot);

			details.AgreementTypeDescription = chargeCode1.AC_Code;
			Factory.Save();

			Assert("CHG4 is profit shared", details.IsChargeGroupProfitShared(chargeCode4, paymentTerm));

			var chargeCode5 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode5.AC_Code = "CHG5";

			Factory.Save();

			//Link CHG1 to CHG5 via InterCompany Global Charge - Account Receivable

			sqlPivot = $"INSERT INTO dbo.AccGlobalChargeCodeMapPivot (YP_PK, YP_YG, YP_AC, YP_TYPE, YP_SystemCreateTimeUtc, YP_SystemCreateUser, YP_SystemLastEditTimeUtc, YP_SystemLastEditUser) VALUES('{Guid.NewGuid()}', '{yg_pk}', '{chargeCode5.PK.ToString()}', 'AR', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			CargoWise.Data.Db.Connection.ExecuteNonQuery(sqlPivot);

			details.AgreementTypeDescription = chargeCode1.AC_Code;
			Factory.Save();

			Assert("CHG5 is profit shared", details.IsChargeGroupProfitShared(chargeCode5, paymentTerm));

			//Check that charge from another company with same charge code is also Profit Shared

			var testUser = Factory.NewWithValidTestData<GlbStaff>();
			var anotherCompany = Factory.NewWithValidTestData<GlbCompany>();
			var testBranch = anotherCompany.Branches.AddNew();

			var chargeCodeAnotherCompany = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCodeAnotherCompany.AC_GC = anotherCompany.PK;
			chargeCodeAnotherCompany.AC_Code = "CHG1";

			Factory.Save();

			using (Env.SetTemporaryUserContext(testUser.PK.ToGuid(), testBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var newFactory = new BusinessObjectFactory();
				AssertEquals(anotherCompany.PK, GlbCompany.CurrentCompany.PK);
				var detailsLoadedAsOtherCompany = newFactory.Load<OrgProfitShareDetails>(details.PK);

				Assert("Charge from another company with same code is profit shared", detailsLoadedAsOtherCompany.IsChargeGroupProfitShared(chargeCodeAnotherCompany, paymentTerm));
			}

			Assert("Charge from another company is not matched when not logged in under this company", !details.IsChargeGroupProfitShared(chargeCodeAnotherCompany, paymentTerm));
		}

		public void TestDefaultValues()
		{
			OrgProfitShareDetails details = (OrgProfitShareDetails)GetNewBusinessObject();
			AssertEquals("Start Date today", ZDateTime.Today, details.O4_StartDate);
			AssertEquals("End Date today + 1 yr", ZDateTime.Today.AddYears(1), details.O4_EndDate);
		}

		public void TestControllingAgentProperties()
		{
			OrgHeader sendingAgent = Factory.New<OrgHeader>();
			OrgHeader receivingAgent = Factory.New<OrgHeader>();
			OrgHeader controllingAgent = Factory.New<OrgHeader>();

			OrgAgentRelationship agentRelationship = Factory.New<OrgAgentRelationship>();
			agentRelationship.O3_OH_SendingAgent = sendingAgent.PK;
			agentRelationship.O3_OH_ReceivingAgent = receivingAgent.PK;

			OrgProfitShareDetails details = agentRelationship.GenericProfitShareDetails.AddNew();
			Assert(details.ControllingAgentIsDifferentToSendingAndReceiving);
			Assert(!details.ControllingAgentIsSendingAgent);
			Assert(!details.ControllingAgentIsReceivingAgent);

			details.O4_OH_ControllingAgent = controllingAgent.PK;
			Assert(details.ControllingAgentIsDifferentToSendingAndReceiving);
			Assert(!details.ControllingAgentIsSendingAgent);
			Assert(!details.ControllingAgentIsReceivingAgent);

			details.O4_OH_ControllingAgent = sendingAgent.PK;
			Assert(!details.ControllingAgentIsDifferentToSendingAndReceiving);
			Assert(details.ControllingAgentIsSendingAgent);
			Assert(!details.ControllingAgentIsReceivingAgent);

			details.O4_OH_ControllingAgent = receivingAgent.PK;
			Assert(!details.ControllingAgentIsDifferentToSendingAndReceiving);
			Assert(!details.ControllingAgentIsSendingAgent);
			Assert(details.ControllingAgentIsReceivingAgent);

			agentRelationship.O3_OH_SendingAgent = ZGuid.Empty;
			agentRelationship.O3_OH_ReceivingAgent = ZGuid.Empty;
			details.O4_OH_ControllingAgent = ZGuid.Empty;
			Assert(details.ControllingAgentIsDifferentToSendingAndReceiving);
			Assert(!details.ControllingAgentIsReceivingAgent);
			Assert(!details.ControllingAgentIsSendingAgent);
		}

		public void TestGetUserChargeCodes()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

			OrgAgentRelationship relationship = Factory.New<OrgAgentRelationship>();
			relationship.O3_OH_SendingAgent = org.PK;
			OrgProfitShareDetails details = relationship.ProfitShareDetails.AddNew();

			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode3 = Factory.NewWithValidTestData<AccChargeCode>();

			chargeCode1.AC_Code = "CHG1";
			chargeCode2.AC_Code = "CHG2";
			chargeCode3.AC_Code = "CHG3";

			Factory.Save();

			details.O4_AgreementType = OrgProfitShareDetailsLookups.AgreementTypeUserDefined;
			details.AgreementTypeDescription = "CHG1, CHG2, CHG3";

			Factory.Save();

			var expectedValue = new[] { chargeCode1, chargeCode2, chargeCode3 };
			AssertContainsExactElementsInAnyOrder(expectedValue, details.GetRelatedObjectsViaPivot<AccChargeCode>(Core.Constants.GenPivotTypes.ProfitShareUserCharges));

			var loadedProfitShareDetails = Factory.Load<OrgProfitShareDetails>(details.PK);
			AssertEquals("CHG1, CHG2, CHG3", loadedProfitShareDetails.AgreementTypeDescription);
		}

		public void TestUpdateUserChargeCodes()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

			OrgAgentRelationship relationship = Factory.New<OrgAgentRelationship>();
			relationship.O3_OH_SendingAgent = org.PK;
			OrgProfitShareDetails detail = relationship.ProfitShareDetails.AddNew();
			detail.O4_AgreementType = OrgProfitShareDetailsLookups.AgreementTypeUserDefined;

			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode3 = Factory.NewWithValidTestData<AccChargeCode>();

			chargeCode1.AC_Code = "CHG1";
			chargeCode2.AC_Code = "CHG2";
			chargeCode3.AC_Code = "CHG3";

			Factory.Save();

			detail.AgreementTypeDescription = "CHG1, CHG2";

			Factory.Save();

			var relatedCharges = detail.GetRelatedObjectsViaPivot<AccChargeCode>(Core.Constants.GenPivotTypes.ProfitShareUserCharges);
			AssertContainsExactElementsInAnyOrder(new[] { chargeCode1, chargeCode2 }, relatedCharges);

			var detailToLoad = Factory.Load<OrgProfitShareDetails>(detail.PK);
			AssertEquals("CHG1, CHG2", detailToLoad.AgreementTypeDescription);

			detail.AgreementTypeDescription = "CHG2, CHG3";
			Factory.Save();

			relatedCharges = detail.GetRelatedObjectsViaPivot<AccChargeCode>(Core.Constants.GenPivotTypes.ProfitShareUserCharges);
			AssertContainsExactElementsInAnyOrder(new[] { chargeCode2, chargeCode3 }, relatedCharges);

			detail.O4_AgreementType = OrgProfitShareDetailsLookups.AgreementTypeOrigin;
			Factory.Save();

			relatedCharges = detail.GetRelatedObjectsViaPivot<AccChargeCode>(Core.Constants.GenPivotTypes.ProfitShareUserCharges);
			AssertEquals(0, relatedCharges.Length);

			detail.O4_AgreementType = OrgProfitShareDetailsLookups.AgreementTypeUserDefined;
			detail.AgreementTypeDescription = "CHG1, CHG2";

			Factory.Save();

			relatedCharges = detail.GetRelatedObjectsViaPivot<AccChargeCode>(Core.Constants.GenPivotTypes.ProfitShareUserCharges);
			Assert(relatedCharges.Length > 0);

			detail.Delete();
			Factory.Save();
			relatedCharges = detail.GetRelatedObjectsViaPivot<AccChargeCode>(Core.Constants.GenPivotTypes.ProfitShareUserCharges);
			AssertEquals(0, relatedCharges.Length);
		}

		public void TestUserChargeCodesNotModifiedWhenCompanyChanges()
		{
			var testUser = Factory.NewWithValidTestData<GlbStaff>();
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var testBranch = company1.Branches.AddNew();

			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode3 = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode1AnotherCompany = Factory.NewWithValidTestData<AccChargeCode>();

			chargeCode1.AC_Code = "CHG1";
			chargeCode2.AC_Code = "CHG2";
			chargeCode3.AC_Code = "CHG3";
			chargeCode1.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode2.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode3.AC_GC = GlbCompany.CurrentCompany.PK;

			chargeCode1AnotherCompany.AC_Code = "CHG1";
			chargeCode1AnotherCompany.AC_GC = company1.PK;

			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var relationship = Factory.New<OrgAgentRelationship>();
			relationship.O3_OH_SendingAgent = org.PK;

			var detail = relationship.ProfitShareDetails.AddNew();
			detail.O4_AgreementType = OrgProfitShareDetailsLookups.AgreementTypeUserDefined;

			detail.AgreementTypeDescription = "CHG1, CHG2, CHG3";
			Factory.Save();

			using (Env.SetTemporaryUserContext(testUser.PK.ToGuid(), testBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var newFactory1 = new BusinessObjectFactory();
				AssertEquals(company1.PK, GlbCompany.CurrentCompany.PK);
				var detailLoadedAsOtherCompany = newFactory1.Load<OrgProfitShareDetails>(detail.PK);
				AssertEquals("CHG1, CHG2, CHG3", detailLoadedAsOtherCompany.AgreementTypeDescription);

				//Save to trigger the update
				newFactory1.Save();
			}

			var newFactory2 = new BusinessObjectFactory();

			var detailInOtherFactory = newFactory2.Load<OrgProfitShareDetails>(detail.PK);
			AssertEquals("CHG1, CHG2, CHG3", detailInOtherFactory.AgreementTypeDescription);
		}

		public void TestCloneWithUserChargeCodes()
		{
			var details = (OrgProfitShareDetails)GetNewBusinessObject();
			details.O4_AgreementType = OrgProfitShareDetailsLookups.AgreementTypeUserDefined;
			var party1 = details.PartyDetails.AddNew();
			var party2 = details.PartyDetailsForGatewayProfitShareRedistribution.AddNew();
			details.AgreementTypeDescription = "FRT, ODOC";

			var detailsClone = (OrgProfitShareDetails)details.Clone();

			detailsClone.PartyDetails.Should().BeEquivalentTo(new[] { new { party1.PS_PartyType } });
			detailsClone.PartyDetailsForGatewayProfitShareRedistribution.Should().BeEquivalentTo(new[] { new { party2.PS_PartyType } });
			AssertEquals(detailsClone.AgreementTypeDescription, "FRT, ODOC");
		}

		public void TestO4_GatewayAgentType_ReadOnly()
		{
			var details = (OrgProfitShareDetails)GetNewBusinessObject();

			details.O4_JobType = "";
			Assert(details.O4_GatewayAgentTypeInfo.ReadOnly);

			details.O4_JobType = "SHP";
			Assert(details.O4_GatewayAgentTypeInfo.ReadOnly);

			details.O4_JobType = "GCN";
			Assert(!details.O4_GatewayAgentTypeInfo.ReadOnly);
		}

		public void TestSetO4_JobType()
		{
			var details = (OrgProfitShareDetails)GetNewBusinessObject();

			details.O4_JobType = "GCN";
			details.O4_GatewayAgentType = "BGW";

			details.O4_JobType = "";
			AssertEquals("Should clear O4_GatewayAgentType", "", details.O4_GatewayAgentType);

			details.O4_JobType = "GCN";
			details.O4_GatewayAgentType = "BGW";
			details.O4_GatewayProfitApportionmentMethod = "CHG";
			var gatewayProfitshare = details.PartyDetailsForGatewayProfitShareRedistribution.AddNew();
			gatewayProfitshare.PS_PartyProfitSharePercent = 100m;
			gatewayProfitshare.PS_PartyType = "SEN";

			details.O4_JobType = "SHP";
			AssertEquals("Should clear GatewayAgentType", "", details.O4_GatewayAgentType);
			AssertEquals("Should clear Apportionment method", "", details.O4_GatewayProfitApportionmentMethod);
			AssertEquals("should delete gateway profit share details", 0, details.PartyDetailsForGatewayProfitShareRedistribution.Count);
		}

		public void TestSetO4_GatewayProfitApportionmentMethod()
		{
			var details = (OrgProfitShareDetails)GetNewBusinessObject();

			details.O4_JobType = "GCN";
			details.O4_GatewayProfitApportionmentMethod = "CHG";
			var gatewayProfitshare = details.PartyDetailsForGatewayProfitShareRedistribution.AddNew();
			gatewayProfitshare.PS_PartyProfitSharePercent = 100m;
			gatewayProfitshare.PS_PartyType = "SEN";

			details.O4_GatewayProfitApportionmentMethod = "SHP";
			AssertEquals("Should not delete gateway profit share details", 1, details.PartyDetailsForGatewayProfitShareRedistribution.Count);

			details.O4_GatewayProfitApportionmentMethod = "";
			AssertEquals("should delete gateway profit share details", 0, details.PartyDetailsForGatewayProfitShareRedistribution.Count);
		}

		public void TestGetShipmentPickupAgentProfitShare_And_GetShipmentDeliveryAgentProfitShare()
		{
			var profitShare = (OrgProfitShareDetails)GetNewBusinessObject();

			AssertEquals(ZDecimal.Zero, profitShare.ShipmentPickupAgentProfitShare);
			AssertEquals(ZDecimal.Zero, profitShare.ShipmentDeliveryAgentProfitShare);

			OrgProfitShareParty party1 = profitShare.PartyDetailsForGatewayProfitShareRedistribution.AddNew();
			party1.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.ShipmentDeliveryAgent;
			party1.PS_PartyProfitSharePercent = 30;

			AssertEquals(ZDecimal.Zero, profitShare.ShipmentPickupAgentProfitShare);
			AssertEquals((ZDecimal)30, profitShare.ShipmentDeliveryAgentProfitShare);

			party1.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.ShipmentPickupAgent;

			AssertEquals((ZDecimal)30, profitShare.ShipmentPickupAgentProfitShare);
			AssertEquals(ZDecimal.Zero, profitShare.ShipmentDeliveryAgentProfitShare);

			OrgProfitShareParty party2 = profitShare.PartyDetailsForGatewayProfitShareRedistribution.AddNew();
			party2.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.ShipmentDeliveryAgent;
			party2.PS_PartyProfitSharePercent = 40;

			AssertEquals((ZDecimal)30, profitShare.ShipmentPickupAgentProfitShare);
			AssertEquals((ZDecimal)40, profitShare.ShipmentDeliveryAgentProfitShare);

			party1.PS_PartyProfitSharePercent = 40;
			party2.PS_PartyProfitSharePercent = 30;

			AssertEquals((ZDecimal)40, profitShare.ShipmentPickupAgentProfitShare);
			AssertEquals((ZDecimal)30, profitShare.ShipmentDeliveryAgentProfitShare);
		}

		public void TestLoadPartyDetailsIntoSplitCollections()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var relationship = Factory.New<OrgAgentRelationship>();
			relationship.O3_OH_SendingAgent = org.PK;
			var details = relationship.ProfitShareDetails.AddNew();
			var testCodesGeneral = new[]
			{
				OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent,
				OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent,
				OrgProfitSharePartyLookups.PartyTypeCodes.HeadOfficeFranchisor,
				OrgProfitSharePartyLookups.PartyTypeCodes.ControllingAgent,
			};
			foreach (var testCode in testCodesGeneral)
			{
				var party = details.PartyDetails.AddNew();
				party.PS_PartyType = testCode;
			}

			var testCodesRedistribution = new[]
			{
				OrgProfitSharePartyLookups.PartyTypeCodes.ShipmentPickupAgent,
				OrgProfitSharePartyLookups.PartyTypeCodes.ShipmentDeliveryAgent,
			};
			foreach (var testCode in testCodesRedistribution)
			{
				var party = details.PartyDetailsForGatewayProfitShareRedistribution.AddNew();
				party.PS_PartyType = testCode;
			}

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var detailsInNewFactory = newFactory.Load<OrgProfitShareDetails>(details.PK);
			detailsInNewFactory.PartyDetails
				.OfType<OrgProfitShareParty>()
				.Select(p => p.PS_PartyType.ToString())
				.Should().BeEquivalentTo("SEN", "RCV", "HDF", "CON");
			detailsInNewFactory.PartyDetailsForGatewayProfitShareRedistribution
				.OfType<OrgProfitShareParty>()
				.Select(p => p.PS_PartyType.ToString())
				.Should().BeEquivalentTo("SDA", "SPA");

			Assert("This test uses FluentAssertions", true);
		}

		#region ReadOnly Security

		public void TestReadOnlySecurity()
		{
			bool oldProfitShareValue = Env.Security.OrgForwarderModifyProfitShare.IsAllowed;

			try
			{
				OrgAgentRelationship testRelationship = OrgInDB.AgentRelationships.AddNew();
				OrgInDB.AgentRelationships.SetOrganisationReadOnly(OrgInDB);
				OrgProfitShareDetails testProfitShare = testRelationship.ProfitShareDetails.AddNew();

				Env.Security.OrgForwarderModifyProfitShare.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !testProfitShare.O4_AgreementTypeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testProfitShare.O4_EndDateInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testProfitShare.O4_FreightModeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testProfitShare.O4_O3_OrgProfitShareHeaderInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testProfitShare.O4_OH_OrgOverrideInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testProfitShare.O4_OH_ControllingAgentInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testProfitShare.O4_ReceivingPortOrCountryInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testProfitShare.O4_SendingPortOrCountryInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testProfitShare.O4_ShareLossesInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testProfitShare.O4_StartDateInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testProfitShare.O4_OrgOverrideTypeInfo.ReadOnly);

				Env.Security.OrgForwarderModifyProfitShare.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", testProfitShare.O4_AgreementTypeInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testProfitShare.O4_EndDateInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testProfitShare.O4_FreightModeInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testProfitShare.O4_O3_OrgProfitShareHeaderInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testProfitShare.O4_OH_OrgOverrideInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testProfitShare.O4_OH_ControllingAgentInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testProfitShare.O4_ReceivingPortOrCountryInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testProfitShare.O4_SendingPortOrCountryInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testProfitShare.O4_ShareLossesInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testProfitShare.O4_StartDateInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testProfitShare.O4_OrgOverrideTypeInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgForwarderModifyProfitShare.IsAllowed = oldProfitShareValue;
			}
		}

		public void TestPartyDetailsCollectionIsReadOnly()
		{
			var oldProfitShareValue = Env.Security.OrgForwarderModifyProfitShare.IsAllowed;

			try
			{
				var testRelationship = OrgInDB.AgentRelationships.AddNew();
				OrgInDB.AgentRelationships.SetOrganisationReadOnly(OrgInDB);
				var testProfitShare = testRelationship.ProfitShareDetails.AddNew();

				Env.Security.OrgForwarderModifyProfitShare.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !testProfitShare.PartyDetails.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testProfitShare.PartyDetailsForGatewayProfitShareRedistribution.ReadOnly);

				Env.Security.OrgForwarderModifyProfitShare.IsAllowed = false;
				ResetOrgInDB();
				testProfitShare = testRelationship.ProfitShareDetails.AddNew();
				Assert("Access Disallowed - ReadOnly", testProfitShare.PartyDetails.ReadOnly);
				Assert("Access Disallowed - ReadOnly", testProfitShare.PartyDetailsForGatewayProfitShareRedistribution.ReadOnly);
			}
			finally
			{
				Env.Security.OrgForwarderModifyProfitShare.IsAllowed = oldProfitShareValue;
			}
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

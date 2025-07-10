using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgProfitShareDetailsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestSendingReceivingPortOrCountry()
		{
			OrgAgentRelationship relationship = Factory.New<OrgAgentRelationship>();
			OrgProfitShareDetails profitShare = relationship.GenericProfitShareDetails.AddNew();
			profitShare.RunPreSaveValidation();

			AssertNoErrors("Sending Port is not mandatory", profitShare.O4_SendingPortOrCountryInfo);
			AssertNoErrors("Receiving Port is not mandatory", profitShare.O4_ReceivingPortOrCountryInfo);

			profitShare.O4_SendingPortOrCountry = "ZUBIN";
			AssertHasErrors("Invalid port", profitShare.O4_SendingPortOrCountryInfo);
			AssertNoErrors("No errors", profitShare.O4_ReceivingPortOrCountryInfo);

			profitShare.O4_ReceivingPortOrCountry = "RAKHS";
			AssertHasErrors("Invalid port", profitShare.O4_SendingPortOrCountryInfo);
			AssertHasErrors("Invalid port", profitShare.O4_ReceivingPortOrCountryInfo);
		}

		public void TestO4_StartDateShouldBeforeO4_EndDate()
		{
			OrgAgentRelationship relationship = Factory.New<OrgAgentRelationship>();
			OrgProfitShareDetails profitShare = relationship.GenericProfitShareDetails.AddNew();
			profitShare.O4_StartDate = ZDateTime.Today.AddDays(-1);

			profitShare.O4_EndDate = ZDateTime.Today;
			profitShare.RunPreSaveValidation();
			AssertNoErrors("The start date is before end date, should not have error", profitShare.O4_StartDateInfo);
			AssertNoErrors("The start date is before end date, should not have error", profitShare.O4_EndDateInfo);

			profitShare.O4_StartDate = ZDateTime.Today.AddDays(1);
			profitShare.RunPreSaveValidation();
			AssertHasErrors("The 'Start Date' must be before the 'End Date'.", profitShare.O4_StartDateInfo);
			AssertHasErrors("The 'End Date' must be after the 'Start Date'.", profitShare.O4_EndDateInfo);
		}

		public void TestCheckO4_OverrideType()
		{
			OrgAgentRelationship relationship = Factory.New<OrgAgentRelationship>();
			OrgProfitShareDetails profitShare = relationship.GenericProfitShareDetails.AddNew();
			profitShare.RunPreSaveValidation();

			AssertNoErrors("Org Type should default to LOC, should not have error", profitShare.O4_OrgOverrideTypeInfo);
			profitShare.O4_OrgOverrideType = string.Empty;
			AssertHasErrors("Should have error when empty", profitShare.O4_OrgOverrideTypeInfo);
			profitShare.O4_OrgOverrideType = "LOC";
			AssertNoErrors("Should have no errors when a valid code", profitShare.O4_OrgOverrideTypeInfo);
			profitShare.O4_OrgOverrideType = "AAA";
			AssertHasErrors("Should have error when not a valid code", profitShare.O4_OrgOverrideTypeInfo);
		}

		public void TestO4_OH_OrgOverrideMandatoryWhenClientSpecific()
		{
			var relationship = Factory.New<OrgAgentRelationship>();
			var profitShareGeneral = relationship.GenericProfitShareDetails.AddNew();
			profitShareGeneral.O4_FreightMode = "ALL";
			var profitShareClinetSpecific = relationship.ClientSpecificProfitShareDetails.AddNew();
			profitShareClinetSpecific.O4_FreightMode = "ALL";
			AssertEquals("Precondition: profitShareGeneral.O4_OH_OrgOverride", ZGuid.Empty, profitShareGeneral.O4_OH_OrgOverride);
			AssertEquals("Precondition: profitShareClinetSpecific.O4_OH_OrgOverride", ZGuid.Empty, profitShareClinetSpecific.O4_OH_OrgOverride);
			relationship.RunPreSaveValidation();
			var expectedError = "Please enter an Organization Override.";
			AssertNoErrors("Should never be mandatory for general rule", profitShareGeneral.O4_OH_OrgOverrideInfo);
			AssertHasError("Validation after collection AddNew", profitShareClinetSpecific.O4_OH_OrgOverrideInfo, expectedError);
			Assert("", relationship.HasErrors);

			profitShareClinetSpecific.O4_OH_OrgOverride = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertEquals("Precondition: profitShareGeneral.O4_OH_OrgOverride", ZGuid.Empty, profitShareGeneral.O4_OH_OrgOverride);
			AssertNotEquals("Precondition: profitShareClinetSpecific.O4_OH_OrgOverride", ZGuid.Empty, profitShareClinetSpecific.O4_OH_OrgOverride);
			relationship.RunPreSaveValidation();
			AssertNoErrors(relationship);

			Factory.Save();
			ReleaseFactory();
			var relationshipInNewFactory = Factory.Load<OrgAgentRelationship>(relationship.PK);
			AssertEquals("Precondition: GenericProfitShareDetails.Count", 1, relationshipInNewFactory.GenericProfitShareDetails.Count);
			AssertEquals("Precondition: ClientSpecificProfitShareDetails.Count", 1, relationshipInNewFactory.ClientSpecificProfitShareDetails.Count);
			var profitShareGeneralInNewFactory = relationshipInNewFactory.GenericProfitShareDetails[0];
			var profitShareClinetSpecificInNewFactory = relationshipInNewFactory.ClientSpecificProfitShareDetails[0];
			AssertEquals("Precondition: profitShareGeneralInNewFactory.O4_OH_OrgOverride", ZGuid.Empty, profitShareGeneralInNewFactory.O4_OH_OrgOverride);
			AssertNotEquals("Precondition: profitShareClinetSpecificInNewFactory.O4_OH_OrgOverride", ZGuid.Empty, profitShareClinetSpecificInNewFactory.O4_OH_OrgOverride);
			relationshipInNewFactory.RunPreSaveValidation();
			AssertNoErrors(relationship);

			profitShareClinetSpecificInNewFactory.O4_OH_OrgOverride = ZGuid.Empty;
			AssertEquals("Precondition: profitShareGeneralInNewFactory.O4_OH_OrgOverride", ZGuid.Empty, profitShareGeneralInNewFactory.O4_OH_OrgOverride);
			AssertEquals("Precondition: profitShareClinetSpecificInNewFactory.O4_OH_OrgOverride", ZGuid.Empty, profitShareClinetSpecificInNewFactory.O4_OH_OrgOverride);
			relationship.RunPreSaveValidation();
			AssertNoErrors("Should never be mandatory for general rule", profitShareGeneralInNewFactory.O4_OH_OrgOverrideInfo);
			AssertHasError("Validation after collection load", profitShareClinetSpecificInNewFactory.O4_OH_OrgOverrideInfo, expectedError);
			Assert("relationshipInNewFactory.HasErrors", relationshipInNewFactory.HasErrors);
		}

		public void TestUserChargeCodesValidation()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

			OrgAgentRelationship relationship = Factory.New<OrgAgentRelationship>();
			relationship.O3_OH_SendingAgent = org.PK;
			OrgProfitShareDetails profitShare = relationship.ProfitShareDetails.AddNew();
			profitShare.O4_AgreementType = OrgProfitShareDetailsLookups.AgreementTypeUserDefined;

			Factory.Save();

			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode3 = Factory.NewWithValidTestData<AccChargeCode>();

			chargeCode1.AC_Code = "CHG1";
			chargeCode2.AC_Code = "CHG2";
			chargeCode3.AC_Code = "CHG3";

			Factory.Save();

			var errorMessage = "The following Charge Codes are invalid: ZZZ";
			profitShare.AgreementTypeDescription = "CHG1, ZZZ, CHG2";
			profitShare.Validation.ValidateAll();

			AssertHasError(profitShare.AgreementTypeDescriptionInfo, errorMessage);

			profitShare.AgreementTypeDescription = "CHG1, CHG2, CHG3";
			profitShare.Validation.ValidateAll();

			AssertNoError(profitShare.AgreementTypeDescriptionInfo, errorMessage);

			errorMessage = "Please enter a Charge Code.";
			profitShare.AgreementTypeDescription = "";

			AssertHasError(profitShare.AgreementTypeDescriptionInfo, errorMessage);

			profitShare.O4_AgreementType = OrgProfitShareDetailsLookups.AgreementTypeOrigin;
			AssertNoError(profitShare.AgreementTypeDescriptionInfo, errorMessage);
		}

		public void TestO4_JobTypeValidation()
		{
			var relationship = Factory.New<OrgAgentRelationship>();
			var details = relationship.GenericProfitShareDetails.AddNew();

			details.O4_JobType = JobTypesList.Codes.GCN;
			AssertNoErrors("Valid Job Type", details.O4_JobTypeInfo);

			details.O4_JobType = "bla";
			AssertHasError(details.O4_JobTypeInfo, "Enter a valid Job Type.");
		}

		public void TestO4_GatewayAgentTypeValidation()
		{
			var relationship = Factory.New<OrgAgentRelationship>();
			var details = relationship.GenericProfitShareDetails.AddNew();

			details.O4_GatewayAgentType = GatewayAgentTypesList.Codes.SGW;
			AssertNoErrors("Valid Gateway Agent Type Condition", details.O4_GatewayAgentTypeInfo);

			details.O4_GatewayAgentType = "bla";
			AssertHasError(details.O4_GatewayAgentTypeInfo, "Enter a valid Condition.");
		}

		public void TestO4_GatewayProfitApportionmentMethodValidation()
		{
			var relationship = Factory.New<OrgAgentRelationship>();
			var details = relationship.GenericProfitShareDetails.AddNew();

			details.O4_GatewayProfitApportionmentMethod = ZString.Empty;
			AssertNoErrors(details.O4_GatewayProfitApportionmentMethodInfo);

			details.O4_GatewayProfitApportionmentMethod = GatewayProfitApportionmentMethodList.Codes.SHP;
			AssertHasError(details.O4_GatewayProfitApportionmentMethodInfo, "'SHP' Gateway Profit Apportionment Method requires setup in 'G/W Consol Profit Redistribution' tab.");

			var party = details.PartyDetailsForGatewayProfitShareRedistribution.AddNew();
			party.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.ShipmentDeliveryAgent;
			details.Validation.ValidateAll();
			AssertNoErrors(details.O4_GatewayProfitApportionmentMethodInfo);

			details.O4_GatewayProfitApportionmentMethod = "bla";
			AssertHasError(details.O4_GatewayProfitApportionmentMethodInfo, "Enter a valid GW Profit App. Method.");
		}
	}
}

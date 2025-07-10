using CargoWise.Types;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	sealed class ExportAWBSecurityStatusLineValidationTest : Forwarding.AWB.Business.Testing.ExportAWBSecurityStatusLineValidationTest
	{
		public void TestEAS_ExemptionGroundInvalid()
		{
			var line = Factory.New<ExportAWBSecurityStatusLine>();
			line.EAS_ExemptionGround = "EXM";

			AssertHasMessageError(line.EAS_ExemptionGroundInfo, "EXM - is an invalid IATA code.");
		}

		public void TestEAS_ApprovalCategory_Empty()
		{
			var line = Factory.New<ExportAWBSecurityStatusLine>();

			line.EAS_ApprovalCategory = "";
			line.EAS_ApprovalNumber = "";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				line.Validation.ValidateEAS_ApprovalCategory();
				AssertNoError(line.EAS_ApprovalCategoryInfo, "You have to enter Known Type or Approval Number.");
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				line.Validation.ValidateEAS_ApprovalCategory();
				AssertHasError(line.EAS_ApprovalCategoryInfo, "You have to enter Known Type or Approval Number.");
			}

			line.EAS_ApprovalNumber = "1234";
			line.Validation.ValidateEAS_ApprovalCategory();

			AssertNoError(line.EAS_ApprovalCategoryInfo, "You have to enter Known Type or Approval Number.");
			AssertHasMessageError(line.EAS_ApprovalCategoryInfo, "Enter a Known Type - AC, KC or RA.");

			line.EAS_ApprovalCategory = AviationSecuritySchemeMembership.Codes.RegulatedAgent;

			AssertNoMessageError(line.EAS_ApprovalCategoryInfo, "Enter a Known Type - AC, KC or RA.");
		}

		public void TestEAS_ApprovalCategory_Empty_WhenApprovalCategoryListIsEmpty()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;

			var line = consol.AWBHeader.CargoSecurityKnownShippers.AddNew();
			line.EAS_ApprovalCategory = "";
			line.EAS_ApprovalNumber = "";
			line.EAS_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;

			AssertEquals(2, line.ApprovalCategoryList.Count);
			AssertEquals(true, line.ApprovalCategoryList.ContainsCode(AviationSecuritySchemeMembership.Codes.KnownConsignor));
			AssertEquals(true, line.ApprovalCategoryList.ContainsCode(AviationSecuritySchemeMembership.Codes.RegulatedAgent));
			AssertHasError(line.EAS_ApprovalCategoryInfo, "You have to enter Known Type or Approval Number.");

			line.EAS_ApprovalNumber = "1234";
			AssertNoError(line.EAS_ApprovalNumberInfo, "You have to enter Approval Number (Code).");

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			line.EAS_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			line.EAS_ApprovalNumber = "";
			AssertHasError(line.EAS_ApprovalCategoryInfo, "You have to enter Known Type or Approval Number.");

			line.EAS_ApprovalCategory = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
			AssertNoError(line.EAS_ApprovalCategoryInfo, "You have to enter Known Type or Approval Number.");
			AssertNoError(line.EAS_ApprovalNumberInfo, "You have to enter Approval Number (Code).");

			line.EAS_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			AssertHasError(line.EAS_ApprovalNumberInfo, "You have to enter Approval Number (Code).");
		}

		public void TestEAS_ApprovalCategory_ValidSelection()
		{
			var line = Factory.New<ExportAWBSecurityStatusLine>();
			line.EAS_ApprovalCategory = AviationSecuritySchemeMembership.Codes.RegulatedAgent;

			AssertNoErrors(line.EAS_ApprovalCategoryInfo);

			line.EAS_ApprovalCategory = "XX";

			AssertHasErrors(line.EAS_ApprovalCategoryInfo);

			line.EAS_ApprovalCategory = AviationSecuritySchemeMembership.Codes.KnownConsignor;

			AssertNoErrors(line.EAS_ApprovalCategoryInfo);

			line.EAS_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			line.EAS_ApprovalCategory = AviationSecuritySchemeMembership.Codes.AccreditedAirCargoAgent;
			AssertHasErrors(line.EAS_ApprovalCategoryInfo);

			line.EAS_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			line.EAS_ApprovalCategory = string.Empty;
			AssertHasErrors(line.EAS_ApprovalCategoryInfo);

			line.EAS_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			line.EAS_ApprovalCategory = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
			AssertNoErrors(line.EAS_ApprovalCategoryInfo);

			line.EAS_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;
			line.EAS_ApprovalCategory = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
			AssertNoErrors(line.EAS_ApprovalCategoryInfo);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;
			line.EAS_ApprovalNumber = "123";
			line.EAS_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;
			line.EAS_ApprovalCategory = string.Empty;
			AssertNoErrors(line.EAS_ApprovalCategoryInfo);
		}

		public void TestEAS_ApprovalCategory_DirectConsolidations()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;

			var line1 = consol.AWBHeader.CargoSecurityKnownShippers.AddNew();
			line1.EAS_ApprovalCategory = AviationSecuritySchemeMembership.Codes.RegulatedAgent;

			var line2 = consol.AWBHeader.CargoSecurityKnownShippers.AddNew();
			line2.EAS_ApprovalCategory = AviationSecuritySchemeMembership.Codes.KnownConsignor;

			line1.Validation.ValidateEAS_ApprovalCategory();
			line2.Validation.ValidateEAS_ApprovalCategory();

			AssertNoMessageError(line1.EAS_ApprovalCategoryInfo, "Only one entry is allowed for direct consolidations.");
			AssertNoMessageError(line2.EAS_ApprovalCategoryInfo, "Only one entry is allowed for direct consolidations.");

			consol.JK_AgentType = Core.Constants.AgentType.Direct;

			line1.Validation.ValidateEAS_ApprovalCategory();
			line2.Validation.ValidateEAS_ApprovalCategory();

			AssertHasMessageError(line1.EAS_ApprovalCategoryInfo, "Only one entry is allowed for direct consolidations.");
			AssertHasMessageError(line2.EAS_ApprovalCategoryInfo, "Only one entry is allowed for direct consolidations.");

			line2.Delete();

			line1.Validation.ValidateEAS_ApprovalCategory();

			AssertNoMessageError(line1.EAS_ApprovalCategoryInfo, "Only one entry is allowed for direct consolidations.");
		}

		public void TestEAS_ApprovalCategory_PreventDuplicates()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;

			var line1 = consol.AWBHeader.CargoSecurityKnownShippers.AddNew();
			line1.EAS_ApprovalCategory = AviationSecuritySchemeMembership.Codes.RegulatedAgent;

			var line2 = consol.AWBHeader.CargoSecurityKnownShippers.AddNew();
			line2.EAS_ApprovalCategory = AviationSecuritySchemeMembership.Codes.KnownConsignor;

			line1.Validation.ValidateEAS_ApprovalCategory();
			line2.Validation.ValidateEAS_ApprovalCategory();

			AssertNoMessageError(line1.EAS_ApprovalCategoryInfo, "Duplicate entries are not allowed.");
			AssertNoMessageError(line2.EAS_ApprovalCategoryInfo, "Duplicate entries are not allowed.");

			line2.EAS_ApprovalCategory = AviationSecuritySchemeMembership.Codes.RegulatedAgent;

			line1.Validation.ValidateEAS_ApprovalCategory();
			line2.Validation.ValidateEAS_ApprovalCategory();

			AssertHasMessageError(line1.EAS_ApprovalCategoryInfo, "Duplicate entries are not allowed.");
			AssertHasMessageError(line2.EAS_ApprovalCategoryInfo, "Duplicate entries are not allowed.");

			line1.EAS_ApprovalNumber = "111";

			line1.Validation.ValidateEAS_ApprovalCategory();
			line2.Validation.ValidateEAS_ApprovalCategory();

			AssertNoMessageError(line1.EAS_ApprovalCategoryInfo, "Duplicate entries are not allowed.");
			AssertNoMessageError(line2.EAS_ApprovalCategoryInfo, "Duplicate entries are not allowed.");

			line2.EAS_ApprovalNumber = "111";

			line1.Validation.ValidateEAS_ApprovalCategory();
			line2.Validation.ValidateEAS_ApprovalCategory();

			AssertHasMessageError(line1.EAS_ApprovalCategoryInfo, "Duplicate entries are not allowed.");
			AssertHasMessageError(line2.EAS_ApprovalCategoryInfo, "Duplicate entries are not allowed.");
		}

		public void TestAES_ApprovalCategory_ShouldRaiseWarning_WhenCategoryIsRegulatedAgent()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S002";

			var raLine = GetLine();
			raLine.EAS_ApprovalCategory = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
			raLine.EAS_ApprovalNumber = "111";
			raLine.Shipments.Add(shipment);
			raLine.Validation.ValidateEAS_ApprovalCategory();
			AssertHasWarning(raLine.EAS_ApprovalCategoryInfo, "Ensure this RA is the screening organization of at least one linked Shipment. Their details will be sent in the message with the 'OSS' identifier.");

			var kcLine = GetLine();
			kcLine.EAS_ApprovalCategory = AviationSecuritySchemeMembership.Codes.KnownConsignor;
			kcLine.EAS_ApprovalNumber = "112";
			kcLine.Shipments.Add(shipment);
			kcLine.Validation.ValidateEAS_ApprovalCategory();
			AssertNoWarning(kcLine.EAS_ApprovalCategoryInfo, "Ensure this RA is the screening organization of at least one linked Shipment. Their details will be sent in the message with the 'OSS' identifier.");
		}

		public void TestEAS_RN_NKCountryCode()
		{
			var line = GetLine();

			line.EAS_ScreeningMethod = ScreeningMethods.Codes.ExplosiveDetectionSystem;
			line.EAS_ApprovalCategory = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
			line.Validation.ValidateEAS_RN_NKCountryCode();
			AssertNotEquals(line.Type, SecurityStatusLineType.KnownConsignor);
			AssertNoWarning(line.EAS_RN_NKCountryCodeInfo, "Country should be entered before type.");

			line.EAS_ScreeningMethod = null;
			line.EAS_ApprovalCategory = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
			line.Validation.ValidateEAS_RN_NKCountryCode();
			AssertEquals(line.Type, SecurityStatusLineType.KnownConsignor);
			AssertHasWarning(line.EAS_RN_NKCountryCodeInfo, "Country should be entered before type.");

			line.EAS_RN_NKCountryCode = "XX";
			line.Validation.ValidateEAS_RN_NKCountryCode();

			AssertHasError(line.EAS_RN_NKCountryCodeInfo, "Enter a valid selection.");

			line.EAS_RN_NKCountryCode = Core.Constants.CountryCodes.Thailand;

			AssertNoErrors("Valid country code.", line.EAS_RN_NKCountryCodeInfo);
			AssertNoWarning(line.EAS_RN_NKCountryCodeInfo, "Country should be entered before type.");

			line.EAS_RN_NKCountryCode = ZString.Empty;

			AssertHasWarning(line.EAS_RN_NKCountryCodeInfo, "Country should be entered before type.");
			AssertNoErrors("Country code is not required.", line.EAS_RN_NKCountryCodeInfo);
		}

		public void TestEAS_ApprovalNumber_Empty_NoShipments()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_ConsolMode = Core.Constants.AgentType.Agent;

			var line = Factory.New<ExportAWBSecurityStatusLine>();
			line.EAS_EH = consol.AWBHeader.PK;
			line.EAS_ApprovalCategory = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
			line.EAS_ApprovalNumber = "";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				line.Validation.ValidateEAS_ApprovalNumber();
				AssertNoMessageErrors(line.EAS_ApprovalNumberInfo);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				line.Validation.ValidateEAS_ApprovalNumber();
				AssertHasMessageErrors("Enter the Approval Number of the Known Party", line.EAS_ApprovalNumberInfo);
			}

			line.EAS_ApprovalNumber = "123";
			AssertNoMessageErrors("Enter the Approval Number of the Known Party.", line.EAS_ApprovalNumberInfo);
		}

		public void TestEAS_ApprovalNumber_NoAddress_ThreeShipments()
		{
			var line = GetLine();

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_UniqueConsignRef = "S002";

			var shipment3 = Factory.New<ForwardingShipment>();
			shipment3.JS_UniqueConsignRef = "S003";

			line.Shipments.Add(shipment2);
			line.Shipments.Add(shipment3);

			line.EAS_ApprovalNumber = "";

			line.Validation.ValidateEAS_ApprovalNumber();

			AssertHasMessageErrors("Enter the Approval Number of the Known Party or add Consignor to S001, S002, S003 and configure Supply Chain Security.", line.EAS_ApprovalNumberInfo);

			line.EAS_ApprovalNumber = "123";

			AssertNoMessageErrors("Enter the Approval Number of the Known Party or add Consignor to S001, S002, S003 and configure Supply Chain Security.", line.EAS_ApprovalNumberInfo);
		}

		public void TestEAS_ApprovalNumber_NoAddress_FourShipments()
		{
			var line = GetLine();

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_UniqueConsignRef = "S002";

			var shipment3 = Factory.New<ForwardingShipment>();
			shipment3.JS_UniqueConsignRef = "S003";

			var shipment4 = Factory.New<ForwardingShipment>();
			shipment4.JS_UniqueConsignRef = "S004";

			line.Shipments.Add(shipment2);
			line.Shipments.Add(shipment3);
			line.Shipments.Add(shipment4);

			line.EAS_ApprovalNumber = "";

			line.Validation.ValidateEAS_ApprovalNumber();

			AssertHasMessageErrors("Enter the Approval Number of the Known Party or add Consignor to S001, S002 and 2 others and configure Supply Chain Security.", line.EAS_ApprovalNumberInfo);

			line.EAS_ApprovalNumber = "123";

			AssertNoMessageErrors("Enter the Approval Number of the Known Party or add Consignor to S001, S002 and 2 others and configure Supply Chain Security.", line.EAS_ApprovalNumberInfo);
		}

		public void TestEAS_ApprovalNumber_Address_TwoShipments()
		{
			var line = GetLine();

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_UniqueConsignRef = "S002";
			line.Shipments.Add(shipment2);

			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "ABCD";

			line.Organization = org;

			line.EAS_ApprovalNumber = "";

			line.Validation.ValidateEAS_ApprovalNumber();

			AssertHasMessageErrors("Enter the Approval Number of the Known Party or update Supply Chain Security on ABCD.", line.EAS_ApprovalNumberInfo);

			line.EAS_ApprovalNumber = "123";

			AssertNoMessageErrors("Enter the Approval Number of the Known Party or update Supply Chain Security on ABCD.", line.EAS_ApprovalNumberInfo);
		}

		#region Expiry Date

		public void TestEAS_ApprovalExpiryDate()
		{
			var line = GetLine();

			line.EAS_ApprovalExpiryDate = ZDate.Invalid;
			AssertHasError(line.EAS_ApprovalExpiryDateInfo, "Enter a valid selection.");

			line.EAS_ApprovalExpiryDate = ZDate.Today.AddDays(1);
			AssertNoErrors("Valid date", line.EAS_ApprovalExpiryDateInfo);

			line.EAS_ApprovalExpiryDate = ZDate.Empty;
			AssertNoErrors("Empty date", line.EAS_ApprovalExpiryDateInfo);
		}

		public void TestEAS_ApprovalExpiryDate_WillExpirePriorToShipmentDateForAviationSecurity()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S001";
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "MYKUL";
			shipment.JS_E_DEP = ZDate.Today.AddDays(10);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = "AIR";
			consol.Transports[0].JW_ETD = ZDateTime.Empty;

			var line = Factory.New<ExportAWBSecurityStatusLine>();
			line.EAS_EH = consol.AWBHeader.PK;
			line.EAS_ApprovalExpiryDate = ZDate.Today.AddDays(2);
			AssertEquals(line.Type, SecurityStatusLineType.KnownConsignor);
			AssertHasError(line.EAS_ApprovalExpiryDateInfo, "This organization's known status will expire prior to the ETD of Shipment S001.");

			line.EAS_ScreeningMethod = ScreeningMethods.Codes.ExplosiveDetectionSystem;
			line.Validation.ValidateEAS_ApprovalExpiryDate();
			AssertNotEquals(line.Type, SecurityStatusLineType.KnownConsignor);
			AssertNoErrors("This organization's known status will expire prior to the ETD of Shipment S001.", line.EAS_ApprovalExpiryDateInfo);

			consol.Transports[0].JW_TransportMode = "AIR";
			consol.Transports[0].JW_RL_NKLoadPort = "AUBNE";
			consol.Transports[0].JW_RL_NKDiscPort = "MYKUL";
			consol.Transports[0].JW_ETD = ZDate.Today.AddDays(3);

			line.EAS_ScreeningMethod = null;
			line.Validation.ValidateEAS_ApprovalExpiryDate();
			AssertEquals(line.Type, SecurityStatusLineType.KnownConsignor);
			AssertHasError(line.EAS_ApprovalExpiryDateInfo, "This organization's known status will expire prior to the consol ETD.");

			consol.JK_MasterBillIssueDate = ZDate.Today.AddDays(4);

			line.Validation.ValidateEAS_ApprovalExpiryDate();
			AssertHasError(line.EAS_ApprovalExpiryDateInfo, "This organization's known status will expire prior to the master bill issue date.");

			consol.JK_MasterBillIssueDate = ZDate.Today.AddDays(1);

			line.Validation.ValidateEAS_ApprovalExpiryDate();
			AssertNoErrors(line.EAS_ApprovalExpiryDateInfo);
		}

		public void TestEAS_ApprovalExpiryDate_ExpiredPriorToShipmentDateForAviationSecurity()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S001";
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "MYKUL";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = "AIR";

			var line = Factory.New<ExportAWBSecurityStatusLine>();
			line.EAS_EH = consol.AWBHeader.PK;
			line.EAS_ApprovalExpiryDate = ZDate.Today.AddDays(-1);
			AssertHasError(line.EAS_ApprovalExpiryDateInfo, "This organization's known status has expired.");

			shipment.JS_E_DEP = ZDate.Today.AddDays(1);

			line.Validation.ValidateEAS_ApprovalExpiryDate();
			AssertHasError(line.EAS_ApprovalExpiryDateInfo, "This organization's known status expired prior to the ETD of Shipment S001.");

			consol.Transports[0].JW_TransportMode = "AIR";
			consol.Transports[0].JW_RL_NKLoadPort = "AUBNE";
			consol.Transports[0].JW_RL_NKDiscPort = "MYKUL";
			consol.Transports[0].JW_ETD = ZDate.Today.AddDays(2);

			line.Validation.ValidateEAS_ApprovalExpiryDate();
			AssertHasError(line.EAS_ApprovalExpiryDateInfo, "This organization's known status expired prior to the consol ETD.");

			consol.JK_MasterBillIssueDate = ZDate.Today.AddDays(3);

			line.Validation.ValidateEAS_ApprovalExpiryDate();
			AssertHasError(line.EAS_ApprovalExpiryDateInfo, "This organization's known status expired prior to the master bill issue date.");

			line.EAS_ApprovalExpiryDate = ZDate.Today.AddDays(4);
			AssertNoErrors(line.EAS_ApprovalExpiryDateInfo);
		}

		public void TestEAS_ApprovalExpiryDate_DoesNotMatchOrgApproval()
		{
			const string expectedWarning = "This date has been manually added here so does not reflect that saved against the organization.";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.HongKong))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = "AIR";
				consol.JK_RL_NKLoadPort = "HKHKG";
				consol.JK_RL_NKDischargePort = "JPOSA";

				var consignor = Factory.NewWithValidTestData<OrgHeader>();
				consignor.OH_Code = "TESTORG1";
				consignor.CountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				consignor.CountryData.OV_EXApprovalNumber = "KC34576";
				consignor.CountryData.OV_OA_ApprovedLocation = consignor.MainAddress.PK;
				consignor.CountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(10);

				var shipment = consol.Shipments.AddNew();
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "HKHKG";
				shipment.JS_RL_NKDestination = "JPOSA";

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;
				header.Populate();

				var line = header.ExportAWBSecurityStatusLines[0];

				AssertEquals("Precondition: approval number", "KC34576", line.EAS_ApprovalNumber);
				AssertEquals("Precondition: expiry date", ZDate.Today.AddDays(10), line.EAS_ApprovalExpiryDate);
				AssertNoWarning("Expiry date matches approval", line.EAS_ApprovalExpiryDateInfo, expectedWarning);

				line.EAS_ApprovalExpiryDate = ZDate.Today.AddDays(11);

				AssertHasWarning("Expiry date does not match approval", line.EAS_ApprovalExpiryDateInfo, expectedWarning);

				line.EAS_ApprovalNumber = "KC99999";

				AssertNoWarning("No match for the approval itself", line.EAS_ApprovalExpiryDateInfo, expectedWarning);
			}
		}

		#endregion

		public void TestEAS_ScreeningMethod_Unsecured()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				var line = GetLine();
				line.EAS_ScreeningMethod = FreightDataRegistry.AviationSecurity_Unknown_Code;

				AssertHasMessageError(line.EAS_ScreeningMethodInfo, "Shipment/s S001 have not been secured (or there is no Screening Method or Grounds for Exemption) and their Consignors are not approved to screen air cargo.");

				line.EAS_ScreeningMethod = ScreeningMethods.Codes.ExplosiveDetectionDogs;

				AssertNoErrors(line.EAS_ScreeningMethodInfo);
			}
		}

		public void TestEAS_ScreeningMethod_Unsecured_Direct()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Direct;

			var line = Factory.New<ExportAWBSecurityStatusLine>();
			line.EAS_EH = consol.AWBHeader.PK;

			line.EAS_ScreeningMethod = FreightDataRegistry.AviationSecurity_Unknown_Code;

			AssertHasMessageError(line.EAS_ScreeningMethodInfo, "For Direct Consolidation this should either be a Known Consignor, Account Consignor or Regulated Agent or the Screening Method or Grounds for Exemption should be specified.");

			line.EAS_ScreeningMethod = ScreeningMethods.Codes.ExplosiveDetectionDogs;

			AssertNoErrors(line.EAS_ScreeningMethodInfo);
		}

		public void TestEAS_ScreeningMethod_InvalidIATACode()
		{
			var line = GetLine();
			line.EAS_ScreeningMethod = "BBB";

			AssertHasMessageError(line.EAS_ScreeningMethodInfo, "BBB - is an invalid IATA code.");
		}

		public void TestEAS_ScreeningMethod_PackLevelScreening()
		{
			const string expectedError = "Shipment/s  are destined for the United States so in line with 100% screening legislation, a screening status must be recorded for all packages, unless the shipment is APP.";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CN"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_ConsolMode = Core.Constants.AgentType.Agent;
				consol.JK_RL_NKLoadPort = "CNSHA";
				consol.JK_RL_NKDischargePort = "USLAX";

				var shipment = consol.Shipments.AddNew();
				shipment.JS_UniqueConsignRef = "S001";
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_InspectionTypeCode = FreightDataRegistry.AviationSecurity_Unknown_Code;

				var line = Factory.New<ExportAWBSecurityStatusLine>();
				line.EAS_EH = consol.AWBHeader.PK;

				line.EAS_ScreeningMethod = "UNK";
				AssertHasMessageError(line.EAS_ScreeningMethodInfo, expectedError);

				line.EAS_ScreeningMethod = "XRY";
				AssertNoMessageError(line.EAS_ScreeningMethodInfo, expectedError);
			}
		}

		public void TestEAS_ScreeningMethod_PackLevelScreening_USTranshipment()
		{
			const string expectedError = "Shipment/s  are destined for the United States so in line with 100% screening legislation, a screening status must be recorded for all packages, unless the shipment is APP.";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CN"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_ConsolMode = Core.Constants.AgentType.Agent;
				consol.JK_RL_NKLoadPort = "CNSHA";
				consol.JK_RL_NKDischargePort = "BRSAO";

				var transport1 = consol.Transports[0];
				transport1.JW_TransportMode = "AIR";
				transport1.JW_RL_NKLoadPort = "CNSHA";
				transport1.JW_RL_NKDiscPort = "USLAX";

				var transport2 = consol.Transports.AddNew();
				transport2.JW_TransportMode = "AIR";
				transport2.JW_RL_NKLoadPort = "USLAX";
				transport2.JW_RL_NKDiscPort = "BRSAO";

				var shipment = consol.Shipments.AddNew();
				shipment.JS_UniqueConsignRef = "S001";
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_InspectionTypeCode = FreightDataRegistry.AviationSecurity_Unknown_Code;

				var line = Factory.New<ExportAWBSecurityStatusLine>();
				line.EAS_EH = consol.AWBHeader.PK;

				line.EAS_ScreeningMethod = "UNK";
				AssertHasMessageError(line.EAS_ScreeningMethodInfo, expectedError);

				line.EAS_ScreeningMethod = "XRY";
				AssertNoMessageError(line.EAS_ScreeningMethodInfo, expectedError);
			}
		}

		public void TestEAS_ScreeningMethod_PackLevelScreening_AUExport()
		{
			const string expectedError = "Shipment/s  require a screening status to be recorded for all packages, in line with Air Cargo Piece Level Security Screening legislation, unless the shipment is APP.";

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_ConsolMode = Core.Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "MYKLM";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "S001";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_InspectionTypeCode = FreightDataRegistry.AviationSecurity_Unknown_Code;

			var line = Factory.New<ExportAWBSecurityStatusLine>();
			line.EAS_EH = consol.AWBHeader.PK;

			line.EAS_ScreeningMethod = "UNK";
			AssertHasMessageError(line.EAS_ScreeningMethodInfo, expectedError);

			line.EAS_ScreeningMethod = "XRY";
			AssertNoMessageError(line.EAS_ScreeningMethodInfo, expectedError);
		}

		public void TestEAS_ScreeningMethod_PackLevelScreening_HKExport()
		{
			const string expectedError = "Shipment/s  require a screening status to be recorded for all packages, in line with Air Cargo Piece Level Security Screening legislation, unless the shipment is APP.";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_ConsolMode = Core.Constants.AgentType.Agent;
				consol.JK_RL_NKLoadPort = "HKHKG";
				consol.JK_RL_NKDischargePort = "MYKLM";

				var shipment = consol.Shipments.AddNew();
				shipment.JS_UniqueConsignRef = "S001";
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_InspectionTypeCode = FreightDataRegistry.AviationSecurity_Unknown_Code;

				var line = Factory.New<ExportAWBSecurityStatusLine>();
				line.EAS_EH = consol.AWBHeader.PK;

				line.EAS_ScreeningMethod = "UNK";
				AssertHasMessageError(line.EAS_ScreeningMethodInfo, expectedError);

				line.EAS_ScreeningMethod = "XRY";
				AssertNoMessageError(line.EAS_ScreeningMethodInfo, expectedError);
			}
		}

		public void TestValidateRegulatedAgentApprovalNumber()
		{
			var line = GetLine();
			line.Master.EH_AgentApprovalNumber = "";
			line.EAS_ApprovalCategory = AviationSecuritySchemeMembership.Codes.AccountConsignor;
			line.EAS_ApprovalNumber = "000";

			AssertNoErrors(line.EAS_ApprovalCategoryInfo);
			AssertNoErrors(line.EAS_ApprovalNumberInfo);

			line.Master.EH_AgentApprovalNumber = "000";
			line.EAS_ApprovalCategory = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
			AssertHasMessageErrors("The Approval Number of the Known Party is same as Regulated Agent Identifier.", line.EAS_ApprovalCategoryInfo);

			line.Validation.ValidateEAS_ApprovalNumber();
			AssertHasMessageErrors("The Approval Number of the Known Party is same as Regulated Agent Identifier.", line.EAS_ApprovalNumberInfo);

			line.Master.EH_AgentApprovalNumber = "111";
			line.Validation.ValidateEAS_ApprovalNumber();
			line.Validation.ValidateEAS_ApprovalCategory();
			AssertNoErrors(line.EAS_ApprovalCategoryInfo);
			AssertNoErrors(line.EAS_ApprovalNumberInfo);
		}

		#region Implementation

		ExportAWBSecurityStatusLine GetLine()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_ConsolMode = Core.Constants.AgentType.Agent;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "S001";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_InspectionTypeCode = FreightDataRegistry.AviationSecurity_Unknown_Code;

			var line = Factory.New<ExportAWBSecurityStatusLine>();
			line.EAS_EH = consol.AWBHeader.PK;

			return line;
		}

		#endregion
	}
}

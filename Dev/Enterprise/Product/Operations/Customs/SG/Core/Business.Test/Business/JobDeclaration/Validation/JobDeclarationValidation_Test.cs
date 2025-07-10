using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	class JobDeclarationValidation_Test : Customs.Business.Testing.BaseJobDeclarationValidationTest<JobDeclaration>
	{
		public void TestCheckJE_OH_Claimant()
		{
			var claimant = Factory.New<OrgHeader>();
			claimant.OH_Code = "CLAIMTST";
			Factory.Save();
			Declaration.JE_OH_Claimant = ZGuid.NewZGuid();
			AssertHasErrorContaining(Declaration.JE_OH_ClaimantInfo, ListValidation.InvalidCodeError);
			Declaration.JE_OH_Claimant = claimant.PK;
			AssertNoErrorContaining(Declaration.JE_OH_ClaimantInfo, ListValidation.InvalidCodeError);
			Declaration.JE_OH_Claimant = ZGuid.Empty;
			AssertNoMessageErrors(Declaration.JE_OH_ClaimantInfo);
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.GTR;
			Declaration.SG_ClaimantCode = "TEST";
			Declaration.ClaimantAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(Declaration.JE_OH_ClaimantInfo, "Claimant is required when a Claimant Code is entered");
			Declaration.JE_OH_Claimant = claimant.PK;
			AssertHasMessageError(Declaration.JE_OH_ClaimantInfo, "Claimant does not have a UEN reference, (set up in Organisation > Config)");
			claimant.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "TEST");
			claimant.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "TEST-UEN");
			Declaration.ClaimantAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageErrors(Declaration.JE_OH_ClaimantInfo);
			Declaration.ClaimantAddress.E2_AddressOverride = true;
			AssertHasMessageError(Declaration.JE_OH_ClaimantInfo, "The Claimant Address has been overridden. Set this Organization again, or create a New Organization using the Address Details on the Addresses Tab.");
		}

		public void TestCheckJE_OH_HandlingAgent()
		{
			var handlingAgent = Factory.New<OrgHeader>();
			handlingAgent.OH_Code = "HNDLAGTTST";
			handlingAgent.OH_IsForwarder = true;
			Factory.Save();
			Declaration.JE_OH_HandlingAgent = ZGuid.Empty;
			AssertNoErrors(Declaration.JE_OH_HandlingAgentInfo);
			Declaration.JE_OH_HandlingAgent = ZGuid.NewZGuid();
			AssertHasErrorContaining(Declaration.JE_OH_HandlingAgentInfo, ListValidation.InvalidCodeError);
			Declaration.JE_OH_HandlingAgent = handlingAgent.PK;
			AssertNoErrors(Declaration.JE_OH_HandlingAgentInfo);
			AssertHasMessageError(Declaration.JE_OH_HandlingAgentInfo, "Handling Agent does not have a UEN reference, (set up in Organisation > Config)");
			handlingAgent.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "TEST");
			handlingAgent.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "TEST-UEN");
			Declaration.HandlingAgentAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageErrors(Declaration.JE_OH_HandlingAgentInfo);
			Declaration.HandlingAgentAddress.E2_AddressOverride = true;
			AssertHasMessageError(Declaration.JE_OH_HandlingAgentInfo, "The Handling Agent Address has been overridden. Set this Organization again, or create a New Organization using the Address Details on the Addresses Tab.");
		}

		public void TestCheckJE_OH_InwardCarrierAgent()
		{
			var agent = Factory.New<OrgHeader>();
			agent.OH_Code = "AGNTTEST";
			Factory.Save();
			Declaration.JE_OH_InwardCarrierAgent = ZGuid.Empty;
			AssertNoErrors(Declaration.JE_OH_InwardCarrierAgentInfo);
			Declaration.JE_OH_InwardCarrierAgent = ZGuid.NewZGuid();
			AssertHasErrorContaining(Declaration.JE_OH_InwardCarrierAgentInfo, ListValidation.InvalidCodeError);
			Declaration.JE_OH_InwardCarrierAgent = agent.PK;
			AssertNoErrors(Declaration.JE_OH_InwardCarrierAgentInfo);
			AssertHasMessageError(Declaration.JE_OH_InwardCarrierAgentInfo, "Inward Carrier Agent does not have a UEN reference, (set up in Organisation > Config)");
			agent.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "TEST");
			agent.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "TEST-UEN");
			Declaration.InwardCarrierAgentAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageErrors(Declaration.JE_OH_InwardCarrierAgentInfo);
			Declaration.InwardCarrierAgentAddress.E2_AddressOverride = true;
			AssertHasMessageError(Declaration.JE_OH_InwardCarrierAgentInfo, "The Inward Carrier Agent Address has been overridden. Set this Organization again, or create a New Organization using the Address Details on the Addresses Tab.");
		}

		public void TestCheckOutwardShippingLineForwarderPK()
		{
			var shippingLine = Factory.New<OrgHeader>();
			shippingLine.OH_Code = "SHPLNTEST";
			Factory.Save();
			Declaration.OutwardShippingLineForwarderPK = ZGuid.Empty;
			AssertNoErrors(Declaration.OutwardShippingLineForwarderPKInfo);
			Declaration.OutwardShippingLineForwarderPK = ZGuid.NewZGuid();
			AssertHasErrorContaining(Declaration.OutwardShippingLineForwarderPKInfo, ListValidation.InvalidCodeError);
			Declaration.OutwardShippingLineForwarderPK = shippingLine.PK;
			AssertNoErrors(Declaration.OutwardShippingLineForwarderPKInfo);
			AssertHasMessageError(Declaration.OutwardShippingLineForwarderPKInfo, "Outward Carrier Agent does not have a UEN reference, (set up in Organisation > Config)");
			shippingLine.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "TEST");
			shippingLine.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "TEST-UEN");
			Declaration.OutwardShippingLineForwarderDocAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageErrors(Declaration.OutwardShippingLineForwarderPKInfo);
			Declaration.OutwardShippingLineForwarderDocAddress.E2_AddressOverride = true;
			AssertHasMessageError(Declaration.OutwardShippingLineForwarderPKInfo, "The Outward Carrier Agent Address has been overridden. Set this Organization again, or create a New Organization using the Address Details on the Addresses Tab.");
		}

		public void TestCheckJE_Calc_InvoicesCount()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			for (int index = 0; index < 20; index++)
			{
				declaration.Invoices.AddNew();
			}

			AssertNoErrors(declaration);
			declaration.Invoices.AddNew();
			AssertHasError(declaration.JE_Calc_InvoicesCountInfo, Customs.SG.V4.Business.JobDeclarationValidation.Only20InvoicesAreAllowed);
			declaration.Invoices[0].Delete();
			AssertNoError(declaration.JE_Calc_InvoicesCountInfo, Customs.SG.V4.Business.JobDeclarationValidation.Only20InvoicesAreAllowed);
			declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			AssertEquals("Should still be 20 invoices against this declaration.", 20, declaration.JE_Calc_InvoicesCount);
			declaration.Invoices.AddNew();
			AssertNoError("OUT dec does not have a limit on invoices.", declaration.JE_Calc_InvoicesCountInfo, Customs.SG.V4.Business.JobDeclarationValidation.Only20InvoicesAreAllowed);
			declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			declaration.Invoices.AddNew();
			AssertHasError("IPT should not allow > 20 invoices", declaration.JE_Calc_InvoicesCountInfo, Customs.SG.V4.Business.JobDeclarationValidation.Only20InvoicesAreAllowed);
			declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			declaration.Invoices.AddNew();
			AssertNoError("TNP dec does not have a limit on invoices.", declaration.JE_Calc_InvoicesCountInfo, Customs.SG.V4.Business.JobDeclarationValidation.Only20InvoicesAreAllowed);
		}

		public void TestDeclaration()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertEquals(declaration.Validation.Declaration, declaration);
		}

		public void TestMessageType()
		{
			Declaration.JE_MessageType = "";
			Declaration.Validation.ValidateJE_MessageType();
			AssertEquals(true, Declaration.JE_MessageTypeInfo.HasErrors());
			Declaration.JE_MessageType = "ABC";
			Declaration.Validation.ValidateJE_MessageType();
			AssertEquals(true, Declaration.JE_MessageTypeInfo.HasMessageErrors());
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.COO;
			Declaration.Validation.ValidateJE_MessageType();
			AssertEquals(false, Declaration.JE_MessageTypeInfo.HasMessageErrors());
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			Declaration.Validation.ValidateJE_MessageType();
			AssertEquals(false, Declaration.JE_MessageTypeInfo.HasMessageErrors());
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			Declaration.Validation.ValidateJE_MessageType();
			AssertEquals(false, Declaration.JE_MessageTypeInfo.HasMessageErrors());
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			Declaration.Validation.ValidateJE_MessageType();
			AssertEquals(false, Declaration.JE_MessageTypeInfo.HasMessageErrors());
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			Declaration.Validation.ValidateJE_MessageType();
			AssertEquals(false, Declaration.JE_MessageTypeInfo.HasMessageErrors());
		}

		public override void TestJE_OH_SupplierValidationWithOrgOnCreditHold()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TestOrg";
			org.CompanyData.OB_IsDebtor = ZBool.True;
			org.CompanyData.OB_AROnCreditHold = ZBool.True;
			Factory.Save();
			Assert("IsCreditOnHold", org.CreditChecker.IsCreditOnHold());
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = org.PK;
			declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			AssertHasWarningContaining(declaration.JE_OH_SupplierInfo, "TestOrg is on Credit Hold.");
			declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			AssertNoWarningContaining(declaration.JE_OH_SupplierInfo, "TestOrg is on Credit Hold.");
		}

		public override void TestJE_OH_ImporterValidationWithOrgOnCreditHold()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TestOrg";
			org.CompanyData.OB_IsDebtor = ZBool.True;
			org.CompanyData.OB_AROnCreditHold = ZBool.True;
			Factory.Save();
			Assert("IsCreditOnHold", org.CreditChecker.IsCreditOnHold());
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			AssertHasWarningContaining(declaration.JE_OH_ImporterInfo, "TestOrg is on Credit Hold.");
			declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			AssertNoWarningContaining(declaration.JE_OH_ImporterInfo, "TestOrg is on Credit Hold.");
		}

		public override void TestCheckJE_DateOfArrival()
		{
			Declaration.Validation.ValidateJE_DateOfArrival();
			AssertEquals(false, Declaration.JE_DateOfArrivalInfo.HasMessageErrors());
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Declaration.Validation.ValidateJE_DateOfArrival();
			AssertEquals(true, Declaration.JE_DateOfArrivalInfo.HasMessageErrors());
			Declaration.JE_DateOfArrival = ZDateTime.Today;
			Declaration.Validation.ValidateJE_DateOfArrival();
			AssertEquals(false, Declaration.JE_DateOfArrivalInfo.HasMessageErrors());
		}

		public void TestInwardMAWB_()
		{
			Declaration.JE_TransportMode = "";
			Validation.ValidateJE_MasterBill();
			AssertEquals(false, Declaration.JE_MasterBillInfo.HasMessageErrors());
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_7_Pipeline;
			Validation.ValidateJE_MasterBill();
			AssertEquals(false, Declaration.JE_MasterBillInfo.HasMessageErrors());
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			Validation.ValidateJE_MasterBill();
			AssertEquals(true, Declaration.JE_MasterBillInfo.HasMessageErrors());
			Declaration.JE_VoyageFlightNo = "QF123";
			Declaration.JE_MasterBill = "23211111111";
			AssertEquals(true, Declaration.JE_MasterBillInfo.HasWarnings());
			Declaration.JE_VoyageFlightNo = "QF123";
			Declaration.JE_MasterBill = "08111111111";
			AssertEquals(false, Declaration.JE_MasterBillInfo.HasWarnings());
			Declaration.JE_MasterBill = "";
			AssertEquals(true, Declaration.JE_MasterBillInfo.HasWarnings());
			Declaration.SG_IsInwardHandCarried = true;
			AssertEquals(false, Declaration.JE_MasterBillInfo.HasWarnings());
			Declaration.SG_IsInwardHandCarried = false;
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Declaration.JE_MasterBill = "";
			AssertEquals(true, Declaration.JE_MasterBillInfo.HasMessageErrors());
			Declaration.JE_MasterBill = "InwardMAWB";
			Validation.ValidateJE_MasterBill();
			AssertEquals(false, Declaration.JE_MasterBillInfo.HasMessageErrors());
		}

		public void TestImporter()
		{
			OrgHeader importer = Factory.New<OrgHeader>();
			Declaration.JE_OH_Importer = importer.PK;
			Validation.ValidateJE_OH_Importer();
			AssertEquals(true, Declaration.JE_OH_ImporterInfo.HasMessageErrors());
			importer.OH_IsConsignee = true;
			Validation.ValidateJE_OH_Importer();
			AssertEquals(true, Declaration.JE_OH_ImporterInfo.HasMessageErrors());
			importer.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "TEST");
			importer.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "TEST-UEN");
			Validation.ValidateJE_OH_Importer();
			AssertEquals(false, Declaration.JE_OH_ImporterInfo.HasMessageErrors());
		}

		public void TestMajorExporterImporter()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			Factory.Save();
			OrgHeader importer = Factory.New<OrgHeader>();
			importer.OH_IsConsignee = true;
			importer.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "12345678901J");
			importer.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "198203607W");
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			Declaration.JE_OH_Importer = importer.PK;
			Declaration.SG_US_NKPlaceOfReceipt = SGCPlaces.Constants.MajorExporterScheme;
			Validation.ValidateJE_OH_Importer();
			AssertEquals("Put MES error only on inward decs", false, Declaration.JE_OH_ImporterInfo.HasErrors());
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			Validation.ValidateJE_OH_Importer();
			AssertEquals(false, Declaration.JE_OH_ImporterInfo.HasErrors());
		}

		public void TestForwarder()
		{
			OrgHeader forwarder = Factory.New<OrgHeader>();
			Declaration.JE_OH_Forwarder = forwarder.PK;
			Validation.ValidateJE_OH_Forwarder();
			AssertEquals(true, Declaration.JE_OH_ForwarderInfo.HasMessageErrors());
			forwarder.OH_IsConsignee = true;
			Validation.ValidateJE_OH_Forwarder();
			AssertEquals(true, Declaration.JE_OH_ForwarderInfo.HasMessageErrors());
			forwarder.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "TEST");
			forwarder.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "TEST-UEN");
			Validation.ValidateJE_OH_Forwarder();
			AssertEquals(false, Declaration.JE_OH_ForwarderInfo.HasMessageErrors());
		}

		public void TestBGIndicator()
		{
			Declaration.JE_PaymentMethod = BGIndicatorCodeList.Codes.D;
			Validation.ValidateJE_PaymentMethod();
			AssertEquals(false, Declaration.JE_PaymentMethodInfo.HasMessageErrors());
			Declaration.JE_PaymentMethod = "Q";
			Validation.ValidateJE_PaymentMethod();
			AssertEquals(true, Declaration.JE_PaymentMethodInfo.HasMessageErrors());
		}

		public void TestJE_ApplicationCode()
		{
			AssertEquals("Pre-condition: by default new form should be opened with TN version 4.1", SGConstants.TradeNetVersion.FourPointOne, Declaration.JE_ApplicationCode);
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
			AssertHasMessageError(Declaration.JE_ApplicationCodeInfo, "TradeNet version 4.0 is no longer available.");
			Declaration.JE_ApplicationCode = "CZ";
			AssertNoMessageError(Declaration.JE_ApplicationCodeInfo, "TradeNet version 4.0 is no longer available.");
			AssertHasError("Should not be able to save a declaration with an invalid value in Application Code.", Declaration.JE_ApplicationCodeInfo, "Invalid TradeNet version entered. Choose from the drop down list.");
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			AssertNoError(Declaration.JE_ApplicationCodeInfo, "Invalid TradeNet version entered. Choose from the drop down list.");
		}

		public void TestPortOfLoading_()
		{
			Declaration.Lookups.SGLocoList.Load();
			Declaration.JE_MessageType = "";
			Declaration.JE_RL_NKPortOfLoading = "ZACPT";
			Declaration.Validation.ValidateJE_RL_NKPortOfLoading();
			AssertEquals(false, Declaration.JE_RL_NKPortOfLoadingInfo.HasMessageErrors());
			AssertNoWarning(Declaration.JE_RL_NKPortOfLoadingInfo, JobDeclarationValidation.Circular18_2010);
			Declaration.JE_RL_NKPortOfLoading = "ZAABC";
			Declaration.Validation.ValidateJE_RL_NKPortOfLoading();
			AssertEquals(true, Declaration.JE_RL_NKPortOfLoadingInfo.HasMessageErrors());
			AssertNoWarning(Declaration.JE_RL_NKPortOfLoadingInfo, JobDeclarationValidation.Circular18_2010);
			Declaration.JE_RL_NKPortOfLoading = "IRTHR";
			AssertHasWarning(Declaration.JE_RL_NKPortOfLoadingInfo, JobDeclarationValidation.Circular18_2010);
			Declaration.JE_RL_NKPortOfLoading = "KPHGN";
			AssertHasWarning(Declaration.JE_RL_NKPortOfLoadingInfo, JobDeclarationValidation.Circular18_2010);
			Declaration.JE_EntryAuthorisationDate = ZDateTime.Today;
			Declaration.JE_RL_NKPortOfLoading = "IRTHR";
			AssertNoWarning("Warning should not be shown if declaration has already cleared TradeNet", Declaration.JE_RL_NKPortOfLoadingInfo, JobDeclarationValidation.Circular18_2010);
			Declaration.JE_RL_NKPortOfLoading = "ANSXM"; // ST MAARTEN
			AssertNoErrors("This port code is no longer valid in RefUnloco table, (Country code has changed from AN (NETHERLANDS ANTILLES) to CW (CURACAO)), but the ANxxx port codes are still valid for Singapore Customs", Declaration.JE_RL_NKPortOfLoadingInfo);
		}

		public void TestOrigin_()
		{
			Declaration.JE_MessageType = "";
			Declaration.JE_RL_NKOrigin = "ZAAXC";
			Declaration.Validation.ValidateJE_RL_NKOrigin();
			AssertEquals(false, Declaration.JE_RL_NKOriginInfo.HasMessageErrors());
		}

		public void TestFinalDestination_()
		{
			Declaration.JE_RL_NKFinalDestination = "ZAXBC";
			Declaration.Validation.ValidateJE_RL_NKFinalDestination();
			AssertEquals(false, Declaration.JE_RL_NKFinalDestinationInfo.HasMessageErrors());
		}

		public void TestPortOfArrival_()
		{
			Declaration.Lookups.SGLocoList.Load();
			Declaration.JE_RL_NKPortOfArrival = "ZACPT";
			Declaration.Validation.ValidateJE_RL_NKPortOfArrival();
			AssertEquals(false, Declaration.JE_RL_NKPortOfArrivalInfo.HasMessageErrors());
			AssertNoWarning(Declaration.JE_RL_NKPortOfArrivalInfo, JobDeclarationValidation.Circular18_2010);
			Declaration.JE_RL_NKPortOfArrival = "ZAABC";
			Declaration.Validation.ValidateJE_RL_NKPortOfArrival();
			AssertEquals(true, Declaration.JE_RL_NKPortOfArrivalInfo.HasMessageErrors());
			AssertNoWarning(Declaration.JE_RL_NKPortOfArrivalInfo, JobDeclarationValidation.Circular18_2010);
			Declaration.JE_RL_NKPortOfArrival = "IRTHR";
			AssertHasWarning(Declaration.JE_RL_NKPortOfArrivalInfo, JobDeclarationValidation.Circular18_2010);
			Declaration.JE_RL_NKPortOfArrival = "KPHGN";
			AssertHasWarning(Declaration.JE_RL_NKPortOfArrivalInfo, JobDeclarationValidation.Circular18_2010);
			Declaration.JE_EntryAuthorisationDate = ZDateTime.Today;
			Declaration.JE_RL_NKPortOfArrival = "IRTHR";
			AssertNoWarning("Warning should not be shown if declaration has already cleared TradeNet", Declaration.JE_RL_NKPortOfArrivalInfo, JobDeclarationValidation.Circular18_2010);
			Declaration.JE_RL_NKPortOfArrival = "ANCUR";
			AssertNoErrors("This port code is no longer valid in RefUnloco table, (Country code has changed from AN (NETHERLANDS ANTILLES) to CW (CURACAO)), but the ANxxx port codes are still valid for Singapore Customs", Declaration.JE_RL_NKPortOfArrivalInfo);
		}

		public void TestVessel()
		{
			//wrong transport mode
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			Validation.ValidateJE_VesselName();
			AssertEquals(false, Declaration.JE_VesselNameInfo.HasMessageErrors());
			//empty vessel
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Validation.ValidateJE_VesselName();
			AssertEquals(true, Declaration.JE_VesselNameInfo.HasMessageErrors());
			//list validation
			Declaration.JE_VesselName = "ADM";
			Validation.ValidateJE_VesselName();
			AssertEquals(true, Declaration.JE_VesselNameInfo.HasMessageErrors());
			//vessel entered
			Declaration.JE_VesselName = "ADMIRALENGRACHT";
			Validation.ValidateJE_VesselName();
			AssertEquals(false, Declaration.JE_VesselNameInfo.HasMessageErrors());
		}

		public void TestCheckJE_VesselName_WarningOnDuplicates()
		{
			/*
			 * TODO: This test can only be implemented once the change to the RefVessel table removes the vessel name natural key constraint
			 *	set up multiple test vessels, some with the same name
			 *	test when entering a RefVessel with a vessel name that is duplicated, the SG_OutwardVessel field contains a message error advising of duplicate vessels with this name.
			 *	
			 *	Test (when implemented) will need to validate this warning message is thrown on duplicate vessels:
			 *	Inward Vessel entered has duplicate entries in the Vessel Reference file.\r\nPlease use the <F4> module search functionality to select the appropriate vessel.
			 */
			if (RefVesselSchema.CsvColumnList(RefVesselSchema.Instance).Contains(RefVesselSchema.Constants.RV_Code))
			{
				Assert(true);
			}
			else
			{
				Assert("When changing RV_Code to RV_Name and removing the unique constraint, this test case needs to be implemented", false);
			}
		}

		public void TestVoyageNo()
		{
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_2_Rail;
			Validation.ValidateJE_VoyageFlightNo();
			AssertEquals(false, Declaration.JE_VoyageFlightNoInfo.HasMessageErrors());
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Validation.ValidateJE_VoyageFlightNo();
			AssertEquals(true, Declaration.JE_VoyageFlightNoInfo.HasMessageErrors());
			Declaration.JE_VoyageFlightNo = "123";
			Validation.ValidateJE_VoyageFlightNo();
			AssertEquals(false, Declaration.JE_VoyageFlightNoInfo.HasMessageErrors());
			Declaration.JE_VoyageFlightNo = "25JTU9SG";
			Validation.ValidateJE_VoyageFlightNo();
			AssertEquals(false, Declaration.JE_VoyageFlightNoInfo.HasMessageErrors());
			Declaration.JE_VoyageFlightNo = "";
			Declaration.JE_Folio = "";
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			Validation.ValidateJE_VoyageFlightNo();
			AssertEquals("Air transport requires flight no &/or chartered aircraft rego", true, Declaration.JE_VoyageFlightNoInfo.HasMessageErrors());
			Validation.ValidateJE_Folio();
			AssertEquals("Air transport field for chartered aircraft rego", true, Declaration.JE_FolioInfo.HasMessageErrors());
			Declaration.JE_Folio = "VGE-119";
			Validation.ValidateJE_Folio();
			AssertEquals("Chartered aircraft rego entered", false, Declaration.JE_FolioInfo.HasMessageErrors());
			AssertEquals("Field should have been re-validated", false, Declaration.JE_VoyageFlightNoInfo.HasMessageErrors());
			Declaration.JE_TransportMode = "";
			Declaration.JE_VoyageFlightNo = "";
			Declaration.JE_Folio = "";
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Validation.ValidateJE_VoyageFlightNo();
			Validation.ValidateJE_Folio();
			AssertEquals("When Declaration is Outward transport only, validation not requried for Inward details", false, Declaration.JE_VoyageFlightNoInfo.HasMessageErrors());
			AssertEquals("When Declaration is Outward transport only, validation not requried for Inward details", false, Declaration.JE_FolioInfo.HasMessageErrors());
		}

		public void TestFlightNo()
		{
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_2_Rail;
			Validation.ValidateJE_VoyageFlightNo();
			AssertEquals(false, Declaration.JE_VoyageFlightNoInfo.HasMessageErrors());
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			Validation.ValidateJE_VoyageFlightNo();
			AssertEquals(true, Declaration.JE_VoyageFlightNoInfo.HasMessageErrors());
			Declaration.JE_VoyageFlightNo = "QF123";
			Validation.ValidateJE_VoyageFlightNo();
			AssertEquals(false, Declaration.JE_VoyageFlightNoInfo.HasMessageErrors());
			Declaration.JE_TransportMode = "";
			Declaration.JE_VoyageFlightNo = "";
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			Validation.ValidateJE_VoyageFlightNo();
			AssertEquals("When Declaration is Outward transport only, validation not requried for Inward details", false, Declaration.JE_VoyageFlightNoInfo.HasMessageErrors());
		}

		public void TestJE_TotalWeight()
		{
			Declaration.JE_TotalWeight = 0.776m;
			Declaration.JE_TotalWeightUnit = Core.Constants.Weight.Tonnes;
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "93020000";
			invoiceLine.JI_CustomsUnitQty = UnitOfQuantityCodeList.Codes.TNE;
			invoiceLine.JI_CustomsQuantity = 0.778m;
			Validation.ValidateJE_TotalWeight();
			AssertHasWarning(Declaration.JE_TotalWeightInfo, "Total Weight declared is less than the Customs Qty entered on the invoice line(s).\r\n(Total Weight, in reportable unit of qty: 776 KG - Line weight, in equivalent unit of qty: 778.000 KG)");
			Declaration.JE_TotalWeight = 0.778m;
			Validation.ValidateJE_TotalWeight();
			AssertNoWarning(Declaration.JE_TotalWeightInfo, "Total Weight declared is less than the Customs Qty entered on the invoice line(s).\r\n(Total Weight, in reportable unit of qty: 776 KG - Line weight, in equivalent unit of qty: 778.000 KG)");
			Declaration.JE_TotalWeight = 150m;
			Declaration.JE_TotalWeightUnit = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_CustomsUnitQty = UnitOfQuantityCodeList.Codes.KGM;
			invoiceLine.JI_CustomsQuantity = 160m;
			Validation.ValidateJE_TotalWeight();
			AssertHasWarning(Declaration.JE_TotalWeightInfo, "Total Weight declared is less than the Customs Qty entered on the invoice line(s).\r\n(Total Weight, in reportable unit of qty: 150 KG - Line weight, in equivalent unit of qty: 160 KG)");
			Declaration.JE_TotalWeight = 160m;
			Validation.ValidateJE_TotalWeight();
			AssertNoWarning(Declaration.JE_TotalWeightInfo, "Total Weight declared is less than the Customs Qty entered on the invoice line(s).\r\n(Total Weight, in reportable unit of qty: 150 KG - Line weight, in equivalent unit of qty: 160 KG)");
			Declaration.JE_TotalWeight = 160m;
			invoiceLine.JI_CustomsUnitQty = UnitOfQuantityCodeList.Codes.BOX;
			invoiceLine.JI_CustomsQuantity = 200m;
			Validation.ValidateJE_TotalWeight();
			AssertNoErrors("When unit of quantities differ, validation is irrelevant", Declaration.JE_TotalWeightInfo);
		}

		public void TestCustomsLineQtyErrorsTotalWeightIfGreaterThanShipmentQty()
		{
			Declaration.JE_TotalWeight = 0.776m;
			Declaration.JE_TotalWeightUnit = Core.Constants.Weight.Tonnes;
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "93020000";
			invoiceLine.JI_CustomsUnitQty = UnitOfQuantityCodeList.Codes.TNE;
			invoiceLine.JI_CustomsQuantity = 0.778m;
			Validation.ValidateJE_TotalWeight();
			AssertHasWarning(Declaration.JE_TotalWeightInfo, "Total Weight declared is less than the Customs Qty entered on the invoice line(s).\r\n(Total Weight, in reportable unit of qty: 776 KG - Line weight, in equivalent unit of qty: 778.000 KG)");
			invoiceLine.JI_CustomsQuantity = 0.776m;
			Validation.ValidateJE_TotalWeight();
			AssertNoWarning(Declaration.JE_TotalWeightInfo, "Total Weight declared is less than the Customs Qty entered on the invoice line(s).\r\n(Total Weight, in reportable unit of qty: 776 KG - Line weight, in equivalent unit of qty: 778.000 KG)");
			Declaration.JE_TotalWeight = 150m;
			Declaration.JE_TotalWeightUnit = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_CustomsUnitQty = UnitOfQuantityCodeList.Codes.KGM;
			invoiceLine.JI_CustomsQuantity = 150m;
			Validation.ValidateJE_TotalWeight();
			AssertNoWarning(Declaration.JE_TotalWeightInfo, "Total Weight declared is less than the Customs Qty entered on the invoice line(s).\r\n(Total Weight, in reportable unit of qty: 150 KG - Line weight, in equivalent unit of qty: 160 KG)");
			invoiceLine.JI_CustomsQuantity = 160m;
			Validation.ValidateJE_TotalWeight();
			AssertHasWarning(Declaration.JE_TotalWeightInfo, "Total Weight declared is less than the Customs Qty entered on the invoice line(s).\r\n(Total Weight, in reportable unit of qty: 150 KG - Line weight, in equivalent unit of qty: 160 KG)");
			Declaration.JE_TotalWeight = 150m;
			invoiceLine.JI_CustomsUnitQty = UnitOfQuantityCodeList.Codes.BOX;
			invoiceLine.JI_CustomsQuantity = 200m;
			Validation.ValidateJE_TotalWeight();
			AssertNoMessageErrors("When unit of quantities differ, validation is irrelevant", Declaration.JE_TotalWeightInfo);
		}

		public void TestCustomsLineQtyErrorsTotalWeightIfGreaterThanShipmentQtyWhenMultipleLines()
		{
			Declaration.JE_TotalWeight = 0.776m;
			Declaration.JE_TotalWeightUnit = Core.Constants.Weight.Tonnes;
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "93020000";
			invoiceLine.JI_CustomsUnitQty = UnitOfQuantityCodeList.Codes.TNE;
			invoiceLine.JI_CustomsQuantity = 0.778m;
			Validation.ValidateJE_TotalWeight();
			AssertHasWarning(Declaration.JE_TotalWeightInfo, "Total Weight declared is less than the Customs Qty entered on the invoice line(s).\r\n(Total Weight, in reportable unit of qty: 776 KG - Line weight, in equivalent unit of qty: 778.000 KG)");
			invoiceLine.JI_CustomsQuantity = 0.776m;
			Validation.ValidateJE_TotalWeight();
			AssertNoWarning(Declaration.JE_TotalWeightInfo, "Total Weight declared is less than the Customs Qty entered on the invoice line(s).\r\n(Total Weight, in reportable unit of qty: 776 KG - Line weight, in equivalent unit of qty: 778.000 KG)");
			invoiceLine.JI_CustomsQuantity = 0.5m;
			Validation.ValidateJE_TotalWeight();
			AssertNoWarning(Declaration.JE_TotalWeightInfo, "Total Weight declared is less than the Customs Qty entered on the invoice line(s).\r\n(Total Weight, in reportable unit of qty: 776 KG - Line weight, in equivalent unit of qty: 778.000 KG)");
			JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "93020000";
			invoiceLine2.JI_CustomsUnitQty = UnitOfQuantityCodeList.Codes.KGM;
			invoiceLine2.JI_CustomsQuantity = 300m;
			Validation.ValidateJE_TotalWeight();
			AssertHasWarning(Declaration.JE_TotalWeightInfo, "Total Weight declared is less than the Customs Qty entered on the invoice line(s).\r\n(Total Weight, in reportable unit of qty: 776 KG - Line weight, in equivalent unit of qty: 800.0 KG)");
			invoiceLine2.JI_CustomsQuantity = 276m;
			Validation.ValidateJE_TotalWeight();
			AssertNoWarning(Declaration.JE_TotalWeightInfo, "Total Weight declared is less than the Customs Qty entered on the invoice line(s).\r\n(Total Weight, in reportable unit of qty: 776 KG - Line weight, in equivalent unit of qty: 800.0 KG)");
		}

		public void TestCustomsLineQtyErrorsTotalWeightWhenShipmentWeightNeedsConversion()
		{
			Declaration.JE_TotalWeight = 2000m;
			Declaration.JE_TotalWeightUnit = Core.Constants.Weight.Pounds;
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "93020000";
			invoiceLine.JI_CustomsUnitQty = UnitOfQuantityCodeList.Codes.TNE;
			invoiceLine.JI_CustomsQuantity = 10m;
			Validation.ValidateJE_TotalWeight();
			AssertHasWarning(Declaration.JE_TotalWeightInfo, "Total Weight declared is less than the Customs Qty entered on the invoice line(s).\r\n(Total Weight, in reportable unit of qty: 907.18474 KG - Line weight, in equivalent unit of qty: 10000 KG)");
			invoiceLine.JI_CustomsQuantity = 0.9m;
			Validation.ValidateJE_TotalWeight();
			AssertNoWarning(Declaration.JE_TotalWeightInfo, "Total Weight declared is less than the Customs Qty entered on the invoice line(s).\r\n(Total Weight, in reportable unit of qty: 907.18474 KG - Line weight, in equivalent unit of qty: 10000 KG)");
			JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "93020000";
			invoiceLine2.JI_CustomsUnitQty = UnitOfQuantityCodeList.Codes.KGM;
			invoiceLine2.JI_CustomsQuantity = 4000m;
			Validation.ValidateJE_TotalWeight();
			AssertHasWarning(Declaration.JE_TotalWeightInfo, "Total Weight declared is less than the Customs Qty entered on the invoice line(s).\r\n(Total Weight, in reportable unit of qty: 907.18474 KG - Line weight, in equivalent unit of qty: 4900.0 KG)");
			invoiceLine2.JI_CustomsQuantity = 7m;
			Validation.ValidateJE_TotalWeight();
			AssertNoWarning(Declaration.JE_TotalWeightInfo, "Total Weight declared is less than the Customs Qty entered on the invoice line(s).\r\n(Total Weight, in reportable unit of qty: 907.18474 KG - Line weight, in equivalent unit of qty: 4900.0 KG)");
		}

		public void TestCustomsLineQtyForSea()
		{
			Declaration.JE_TotalWeight = 2000m;
			Declaration.JE_TotalWeightUnit = Core.Constants.Weight.Pounds;
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "93020000";
			invoiceLine.JI_CustomsUnitQty = UnitOfQuantityCodeList.Codes.TNE;
			invoiceLine.JI_CustomsQuantity = 10m;
			Validation.ValidateJE_TotalWeight();
			AssertHasWarning(Declaration.JE_TotalWeightInfo, "Total Weight declared is less than the Customs Qty entered on the invoice line(s).\r\n(Total Weight, in reportable unit of qty: 0.90718474 T - Line weight, in equivalent unit of qty: 10 T)");
			invoiceLine.JI_CustomsQuantity = 0.9m;
			Validation.ValidateJE_TotalWeight();
			AssertNoWarning(Declaration.JE_TotalWeightInfo, "Total Weight declared is less than the Customs Qty entered on the invoice line(s).\r\n(Total Weight, in reportable unit of qty: 0.90718474 T - Line weight, in equivalent unit of qty: 10 T)");
			JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "93020000";
			invoiceLine2.JI_CustomsUnitQty = UnitOfQuantityCodeList.Codes.KGM;
			invoiceLine2.JI_CustomsQuantity = 4000m;
			Validation.ValidateJE_TotalWeight();
			AssertHasWarning(Declaration.JE_TotalWeightInfo, "Total Weight declared is less than the Customs Qty entered on the invoice line(s).\r\n(Total Weight, in reportable unit of qty: 0.90718474 T - Line weight, in equivalent unit of qty: 4.9 T)");
			invoiceLine2.JI_CustomsQuantity = 7m;
			Validation.ValidateJE_TotalWeight();
			AssertNoWarning(Declaration.JE_TotalWeightInfo, "Total Weight declared is less than the Customs Qty entered on the invoice line(s).\r\n(Total Weight, in reportable unit of qty: 0.90718474 T - Line weight, in equivalent unit of qty: 4.9 T)");
		}

		public void TestExporter_()
		{
			ZGuid deletedOrgPK = ZGuid.NewZGuid();
			Declaration.JE_OH_Exporter = deletedOrgPK;
			AssertEquals(false, Declaration.JE_OH_ExporterInfo.HasErrors());
			OrgHeader exporter = Factory.New<OrgHeader>();
			Declaration.JE_OH_Exporter = exporter.PK;
			Declaration.Validation.ValidateJE_OH_Exporter();
			AssertEquals(true, Declaration.JE_OH_ExporterInfo.HasMessageErrors());
			exporter.OH_IsForwarder = true;
			Declaration.Validation.ValidateJE_OH_Exporter();
			AssertEquals(true, Declaration.JE_OH_ExporterInfo.HasMessageErrors());
			exporter.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "TEST");
			exporter.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "TEST-UEN");
			Declaration.Validation.ValidateJE_OH_Exporter();
			AssertEquals(false, Declaration.JE_OH_ExporterInfo.HasMessageErrors());
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			exporter.MainAddress.OA_PostCode = "ABCD123456";
			Declaration.Validation.ValidateJE_OH_Exporter();
			AssertEquals("Consignee for Out dec without CO should validate with warning that PostCode length too big for SG Customs", true, Declaration.JE_OH_ExporterInfo.HasWarning("This Organization has a Post Code that is too long for Singapore Customs to accept, (ABCD123456).\r\nUnless altered on the Organization record, it will be adjusted/truncated to ABCD12345 when sent in the TradeNet message."));
			Declaration.SG_ApplicationProductType = ApplicationProductTypeCodeList.Codes.NA;
			Declaration.Validation.ValidateJE_OH_Exporter();
			AssertEquals("Consignee for Out dec with CO should validate with warning", false, Declaration.JE_OH_ExporterInfo.HasWarning("This Organization has a Post Code that is too long for Singapore Customs to accept, (ABCD123456).\r\nUnless altered on the Organization record, it will be adjusted/truncated to ABCD12345 when sent in the TradeNet message."));
			Declaration.SG_ApplicationProductType = ZString.Empty;
			exporter.MainAddress.OA_PostCode = "ABCD12345";
			Declaration.Validation.ValidateJE_OH_Exporter();
			AssertEquals(false, Declaration.JE_OH_ExporterInfo.HasWarning("This Organization has a Post Code that is too long for Singapore Customs to accept, (ABCD123456).\r\nUnless altered on the Organization record, it will be adjusted/truncated to ABCD12345 when sent in the TradeNet message."));
			AssertEquals(false, Declaration.JE_OH_ExporterInfo.HasWarnings());
		}

		public void TestConsignee_()
		{
			ZGuid deletedOrgPK = ZGuid.NewZGuid();
			Declaration.JE_OH_Consignee = deletedOrgPK;
			AssertEquals(false, Declaration.JE_OH_ConsigneeInfo.HasErrors());
			OrgHeader consignee = Factory.New<OrgHeader>();
			Declaration.JE_OH_Consignee = consignee.PK;
			Validation.ValidateJE_OH_Consignee();
			AssertEquals(true, Declaration.JE_OH_ConsigneeInfo.HasErrors());
			consignee.OH_IsConsignee = true;
			Validation.ValidateJE_OH_Consignee();
			AssertEquals(false, Declaration.JE_OH_ConsigneeInfo.HasErrors());
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.COO;
			consignee.MainAddress.OA_PostCode = "WS1-179200";
			Validation.ValidateJE_OH_Consignee();
			AssertEquals("Consignee for Stand-alone CO should validate without warning", false, Declaration.JE_OH_ConsigneeInfo.HasWarning("This Organization has a Post Code that is too long for Singapore Customs to accept, (WS1-179200).\r\nUnless altered on the Organization record, it will be adjusted/truncated to WS1179200 when sent in the TradeNet message."));
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			Validation.ValidateJE_OH_Consignee();
			AssertEquals("Consignee should validate with PostCode length too big for SG Customs", true, Declaration.JE_OH_ConsigneeInfo.HasWarning("This Organization has a Post Code that is too long for Singapore Customs to accept, (WS1-179200).\r\nUnless altered on the Organization record, it will be adjusted/truncated to WS1179200 when sent in the TradeNet message."));
			consignee.MainAddress.OA_PostCode = "WS1179200";
			Validation.ValidateJE_OH_Consignee();
			AssertEquals(false, Declaration.JE_OH_ConsigneeInfo.HasWarning("This Organization has a Post Code that is too long for Singapore Customs to accept, (WS1-179200).\r\nUnless altered on the Organization record, it will be adjusted/truncated to WS1179200 when sent in the TradeNet message."));
			AssertEquals(false, Declaration.JE_OH_ConsigneeInfo.HasWarnings());
		}

		public void TestManufacturer_()
		{
			var manufacturer = Factory.New<OrgHeader>();
			Declaration.JE_OH_Manufacturer = manufacturer.PK;
			Declaration.Validation.ValidateJE_OH_Manufacturer();
			AssertEquals(false, Declaration.JE_OH_ManufacturerInfo.HasErrors());
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.COO;
			manufacturer.MainAddress.OA_PostCode = "WS3 179200";
			Declaration.Validation.ValidateJE_OH_Manufacturer();
			AssertEquals("PostCode length too big for SG Customs", true, Declaration.JE_OH_ManufacturerInfo.HasWarning("This Organization has a Post Code that is too long for Singapore Customs to accept, (WS3 179200).\r\nUnless altered on the Organization record, it will be adjusted/truncated to WS3179200 when sent in the TradeNet message."));
			manufacturer.MainAddress.OA_PostCode = "WS1179200";
			Declaration.Validation.ValidateJE_OH_Manufacturer();
			AssertEquals(false, Declaration.JE_OH_ManufacturerInfo.HasWarning("This Organization has a Post Code that is too long for Singapore Customs to accept, (WS3 179200).\r\nUnless altered on the Organization record, it will be adjusted/truncated to WS3179200 when sent in the TradeNet message."));
			AssertEquals(false, Declaration.JE_OH_ManufacturerInfo.HasWarnings());
		}

		public void TestBuyer_()
		{
			ZGuid deletedOrgPK = ZGuid.NewZGuid();
			Declaration.JE_OH_Buyer = deletedOrgPK;
			AssertEquals(false, Declaration.JE_OH_BuyerInfo.HasErrors());
			OrgHeader endUser = Factory.New<OrgHeader>();
			Declaration.JE_OH_Buyer = endUser.PK;
			Declaration.Validation.ValidateJE_OH_Buyer();
			AssertEquals(true, Declaration.JE_OH_BuyerInfo.HasErrors());
			endUser.OH_IsConsignee = true;
			Declaration.Validation.ValidateJE_OH_Buyer();
			AssertEquals(false, Declaration.JE_OH_BuyerInfo.HasErrors());
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			endUser.MainAddress.OA_PostCode = "WS1-179200";
			Declaration.Validation.ValidateJE_OH_Buyer();
			AssertEquals("PostCode length too big for SG Customs", true, Declaration.JE_OH_BuyerInfo.HasWarning("This Organization has a Post Code that is too long for Singapore Customs to accept, (WS1-179200).\r\nUnless altered on the Organization record, it will be adjusted/truncated to WS1179200 when sent in the TradeNet message."));
			endUser.MainAddress.OA_PostCode = "WS1179200";
			Declaration.Validation.ValidateJE_OH_Buyer();
			AssertEquals(false, Declaration.JE_OH_BuyerInfo.HasWarning("This Organization has a Post Code that is too long for Singapore Customs to accept, (WS1-179200).\r\nUnless altered on the Organization record, it will be adjusted/truncated to WS1179200 when sent in the TradeNet message."));
			AssertEquals(false, Declaration.JE_OH_BuyerInfo.HasWarnings());
		}

		#region overrides of tests in base that are not valid
		public override void TestEmptyWeightUnit()
		{
			Assert(true);
		}

		#endregion
		#region Implementation
		protected JobDeclaration Declaration
		{
			get
			{
				return declaration ?? (declaration = Factory.New<JobDeclaration>());
			}
		}

		new JobDeclaration declaration;
		protected JobDeclarationValidation Validation
		{
			get
			{
				return Declaration.Validation;
			}
		}

		#endregion
		#region Overrides
		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			AddInfoCUSDECValidationTest.CreatePortCusCodeList(helper, "ZACPT", "CAPETOWN");
			Factory.Save();
		}

		protected override string DefaultExportMessageType
		{
			get
			{
				return MessageTypeCodeList.Codes.OUT;
			}
		}

		protected override string DefaultImportMessageType
		{
			get
			{
				return MessageTypeCodeList.Codes.IPT;
			}
		}

		#endregion
	}
}

using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Testing;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	sealed class JobDeclarationValidationFormalEntryTest : JobDeclarationValidationTest
	{
		public void TestVARIOIsAllowedForDischargeAndDestinationForExportPeriodicDrawbacks()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Drawback;
			Declaration.OtherInfos.AddNew(HeaderOtherInfoList.Codes.PeriodicDrawbackEntry, "");
			Declaration.JE_RL_NKPortOfArrival = "VARIO";
			AssertNoNotifications(Declaration.JE_RL_NKPortOfArrivalInfo);
			Declaration.JE_RL_NKFinalDestination = "VARIO";
			AssertNoNotifications(Declaration.JE_RL_NKFinalDestinationInfo);

			Declaration.OtherInfos.RemoveAndDeleteAll();
			Declaration.Validation.ValidateJE_RL_NKPortOfArrival();
			AssertHasMessageErrors(Declaration.JE_RL_NKPortOfArrivalInfo);
			Declaration.Validation.ValidateJE_RL_NKFinalDestination();
			AssertHasMessageErrors(Declaration.JE_RL_NKFinalDestinationInfo);
		}

		public void TestNoErrorsOrWarningsOnceAMessageHasBeenSent()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			Declaration.JE_DateOfArrival = Declaration.CachedTodaysDate.AddDays(-100);
			Declaration.JE_EDITransmitDate = Declaration.CachedTodaysDate.AddDays(-150);
			AssertHasMessageErrors(Declaration.JE_EDITransmitDateInfo);

			Declaration.CusEntryHeader.CH_IsRestored = true;
			Declaration.Validation.ValidateJE_EDITransmitDate();
			AssertNoNotifications(Declaration.JE_EDITransmitDateInfo);

			Declaration.CusEntryHeader.CH_IsRestored = false;
			NZCMessage message = Declaration.CusEntryHeader.Messages.AddNew();
			Declaration.Validation.ValidateJE_EDITransmitDate();
			AssertNoNotifications(Declaration.JE_EDITransmitDateInfo);
		}

		public void TestValidateOriginalEntryNumber()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertNoNotifications(Declaration.JE_OriginalEntryNumberInfo);
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;
			AssertHasMessageError(Declaration.JE_OriginalEntryNumberInfo, "You have not entered an Original Entry Number.");
			Declaration.JE_OriginalEntryNumber = "12345678";
			AssertNoNotifications(Declaration.JE_OriginalEntryNumberInfo);
		}

		public void TestValidateOriginalEntryType()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_OriginalEntryType = "";
			AssertNoMessageErrors(Declaration.JE_OriginalEntryTypeInfo);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;
			AssertHasMessageError(Declaration.JE_OriginalEntryTypeInfo, "You have not entered an Original Entry Type.");
			Declaration.JE_OriginalEntryType = "ZZZ";
			AssertHasMessageError(Declaration.JE_OriginalEntryTypeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.JE_OriginalEntryType = JobMessageSubTypeList.Codes.Sight;
			AssertNoNotifications(Declaration.JE_OriginalEntryTypeInfo);
		}

		public void TestValidateJE_SendMCDContainerQuarantineDeclaration()
		{
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_SendMCDContainerQuarantineDeclaration = false;
			AssertNoMessageErrors(Declaration.JE_SendMCDContainerQuarantineDeclarationInfo);
			CusContainer container = Declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			AssertHasMessageError(Declaration.JE_SendMCDContainerQuarantineDeclarationInfo, MessageErrorImportFCLEntryRequiresMCD);
			container.CO_FCL_LCL_AIR = ContainerModeList.Codes.LCL;
			AssertNoMessageErrors(Declaration.JE_SendMCDContainerQuarantineDeclarationInfo);
			container.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			AssertHasMessageError(Declaration.JE_SendMCDContainerQuarantineDeclarationInfo, MessageErrorImportFCLEntryRequiresMCD);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertNoMessageErrors(Declaration.JE_SendMCDContainerQuarantineDeclarationInfo);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertHasMessageError(Declaration.JE_SendMCDContainerQuarantineDeclarationInfo, MessageErrorImportFCLEntryRequiresMCD);
			Declaration.JE_SendMCDContainerQuarantineDeclaration = true;
			AssertNoMessageErrors(Declaration.JE_SendMCDContainerQuarantineDeclarationInfo);
			Declaration.JE_SendMCDContainerQuarantineDeclaration = false;
			AssertHasMessageError(Declaration.JE_SendMCDContainerQuarantineDeclarationInfo, MessageErrorImportFCLEntryRequiresMCD);
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			AssertNoMessageErrors(Declaration.JE_SendMCDContainerQuarantineDeclarationInfo);
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			AssertHasMessageError(Declaration.JE_SendMCDContainerQuarantineDeclarationInfo, MessageErrorImportFCLEntryRequiresMCD);

			Factory.Save();
			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			JobDeclaration loadedDeclaration = secondFactory.Load<JobDeclaration>(Declaration.PK);
			AssertNoMessageErrors(loadedDeclaration.JE_SendMCDContainerQuarantineDeclarationInfo);
			loadedDeclaration.Validation.ValidateAll();
			AssertHasMessageError(Declaration.JE_SendMCDContainerQuarantineDeclarationInfo, MessageErrorImportFCLEntryRequiresMCD);
		}

		public void TestValidateExportDate()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_DateOfArrival = ZDateTime.Empty;
			Declaration.JE_ExportDate = ZDateTime.Empty;
			AssertEquals("Import date can be empty for export", false, Declaration.JE_DateOfArrivalInfo.HasMessageErrors());
			AssertEquals("Export date should be filled", true, Declaration.JE_ExportDateInfo.HasMessageErrors());

			Declaration.JE_ExportDate = ZDateTime.Today;
			AssertEquals("Export date should be filled", false, Declaration.JE_ExportDateInfo.HasMessageErrors());
		}

		public void TestValidateImportDate()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_DateOfArrival = ZDateTime.Empty;
			Declaration.JE_ExportDate = ZDateTime.Empty;
			AssertEquals("Export date can be empty", false, Declaration.JE_ExportDateInfo.HasMessageErrors());
			AssertEquals("Import date should be filled", true, Declaration.JE_DateOfArrivalInfo.HasMessageErrors());

			Declaration.JE_DateOfArrival = ZDateTime.Today;
			AssertEquals("Import date should be filled", false, Declaration.JE_DateOfArrivalInfo.HasMessageErrors());
		}

		public void TestIsMasterBillMandatory()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("IsMasterBillMandatory - TSW Declaration does not require master bill", false, Declaration.Validation.IsMasterBillMandatory);
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			AssertEquals("IsMasterBillMandatory - Legacy Declaration", false, Declaration.Validation.IsMasterBillMandatory);
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Post;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("IsMasterBillMandatory - TSW Postal Declaration does not require this field", false, Declaration.Validation.IsMasterBillMandatory);
		}

		public void TestValidateJE_RL_NKOrigin()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_RL_NKOrigin = "";
			AssertHasMessageError("Import TSW Declaration required Shipment Origin", Declaration.JE_RL_NKOriginInfo, JobDeclarationValidationFormalEntry.CountryOfExportRequired);
			Declaration.JE_RL_NKOrigin = "GBTIL";
			AssertNoMessageErrors(Declaration.JE_RL_NKOriginInfo);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			Declaration.JE_RL_NKOrigin = "";
			AssertNoMessageError(Declaration.JE_RL_NKOriginInfo, JobDeclarationValidationFormalEntry.CountryOfExportRequired);
		}

		public void TestValidatePackType()
		{
			UniversalReferenceHelperTest.InitialiseUNEPackageTypeList(Factory, "PK");
			Declaration.JE_TotalNoOfPacksPackType = ZString.Empty;
			AssertEquals("Empty pack type valid", false, Declaration.JE_TotalNoOfPacksPackTypeInfo.HasMessageErrors());

			Declaration.JE_TotalNoOfPacksPackType = "XX";
			AssertEquals("Invalid Code", false, Declaration.JE_TotalNoOfPacksPackTypeInfo.HasMessageErrors());

			Declaration.JE_TotalNoOfPacksPackType = "PK";
			AssertEquals("Valid Code", false, Declaration.JE_TotalNoOfPacksPackTypeInfo.HasMessageErrors());
		}

		public void TestValidatePortOfLoadDischargeForImport()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_RL_NKPortOfLoading = "NZAKL";
			Declaration.JE_RL_NKPortOfArrival = "AUSYD";
			AssertEquals("Wrong Port of load for import", true, Declaration.JE_RL_NKPortOfLoadingInfo.HasMessageErrors());
			AssertEquals("Wrong port of discharge for import", true, Declaration.JE_RL_NKPortOfArrivalInfo.HasMessageErrors());

			Declaration.JE_RL_NKPortOfLoading = "AUSYD";
			Declaration.JE_RL_NKPortOfArrival = "NZAKL";
			AssertEquals("Port of load for import", false, Declaration.JE_RL_NKPortOfLoadingInfo.HasMessageErrors());
			AssertEquals("port of discharge for import", false, Declaration.JE_RL_NKPortOfArrivalInfo.HasMessageErrors());
		}

		public void TestValidatePortOfLoadDischargeForExport()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_RL_NKPortOfLoading = "NZAKL";
			Declaration.JE_RL_NKPortOfArrival = "AUSYD";
			AssertEquals("Wrong Port of load for export", false, Declaration.JE_RL_NKPortOfLoadingInfo.HasMessageErrors());
			AssertEquals("Wrong port of discharge for export", false, Declaration.JE_RL_NKPortOfArrivalInfo.HasMessageErrors());

			Declaration.JE_RL_NKPortOfLoading = "AUSYD";
			Declaration.JE_RL_NKPortOfArrival = "NZAKL";
			AssertEquals("Port of load for export", true, Declaration.JE_RL_NKPortOfLoadingInfo.HasMessageErrors());
			AssertEquals("port of discharge for export", true, Declaration.JE_RL_NKPortOfArrivalInfo.HasMessageErrors());
		}

		public void TestMandatoryValidationForLoadDischargePorts()
		{
			Declaration.JE_RL_NKPortOfLoading = ZString.Empty;
			Declaration.JE_RL_NKPortOfArrival = ZString.Empty;

			AssertEquals("Discharge port should be entered", true, Declaration.JE_RL_NKPortOfArrivalInfo.HasMessageErrors());
			AssertEquals("Load port should be entered", true, Declaration.JE_RL_NKPortOfLoadingInfo.HasMessageErrors());
		}

		public void TestValidateJE_TotalWeight()
		{
			Declaration.JE_TotalWeight = 0m;
			Declaration.JE_TotalWeightUnit = ZString.Empty;

			AssertEquals("Weight should be greater than zero", true, Declaration.JE_TotalWeightInfo.HasMessageErrors());
			AssertEquals("Weight unit should be filled", true, Declaration.JE_TotalWeightUnitInfo.HasMessageErrors());

			Declaration.JE_TotalWeightUnit = "ZZ";
			AssertEquals("Invalid weight unit", true, Declaration.JE_TotalWeightUnitInfo.HasMessageErrors());

			Declaration.JE_TotalWeight = 3m;
			Declaration.JE_TotalWeightUnit = "KG";
			AssertEquals("Valid weight", false, Declaration.JE_TotalWeightInfo.HasMessageErrors());
			AssertEquals("Valid Weight unit", false, Declaration.JE_TotalWeightUnitInfo.HasMessageErrors());
		}

		public void TestValidateJE_MasterBill()
		{
			Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			Declaration.JE_MasterBill = ZString.Empty;
			AssertEquals("Master bill may be empty", false, Declaration.JE_MasterBillInfo.HasMessageErrors());

			Declaration.JE_MasterBill = "08111111111";
			ZString message = new Freight.Business.AirWayBillValidator().GetWarningMessage(Declaration.JE_MasterBill);
			AssertEquals("Master bill valid", false, Declaration.JE_MasterBillInfo.HasMessageError(message));
			AssertEquals("Master bill valid", false, Declaration.JE_MasterBillInfo.HasWarning(message));

			Declaration.JE_MasterBill = "08111111112";
			message = new Freight.Business.AirWayBillValidator().GetWarningMessage(Declaration.JE_MasterBill);
			AssertEquals("Master bill valid", false, Declaration.JE_MasterBillInfo.HasMessageError(message));
			AssertEquals("Master bill valid", true, Declaration.JE_MasterBillInfo.HasWarning(message));

			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_MasterBill = ZString.Empty;
			AssertEquals("Master bill is NOT required for TSW", false, Declaration.JE_MasterBillInfo.HasMessageErrors());

			Declaration.JE_MasterBill = "123456789ABCDEFG00";
			AssertNoMessageErrors(Declaration.JE_MasterBillInfo);
		}

		public void TestMasterBillHasNoLeadingSpaces()
		{
			Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			Declaration.JE_MasterBill = ZString.Empty;

			Declaration.JE_MasterBill = " Y938847";
			AssertEquals("Master bill has error message", true, Declaration.JE_MasterBillInfo.HasMessageErrors());

			Declaration.JE_MasterBill = "Y938847";
			AssertEquals("Master bill has error message", false, Declaration.JE_MasterBillInfo.HasMessageErrors());
		}

		public void TestValidateJE_HouseBill()
		{
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_HouseBill = "123456789B123456789C123456789D12345";
			AssertNoMessageErrors("HouseBill can now utilise full 35 characters", Declaration.JE_HouseBillInfo);
		}

		public void TestHouseBillHasNoLeadingSpaces()
		{
			Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			Declaration.JE_HouseBill = ZString.Empty;

			Declaration.JE_HouseBill = " HB-3998";
			AssertEquals("House bill has error message", true, Declaration.JE_HouseBillInfo.HasMessageErrors());

			Declaration.JE_HouseBill = "Y938847";
			AssertEquals("House bill has error message", false, Declaration.JE_HouseBillInfo.HasMessageErrors());
		}

		public void TestValidateJE_VoyageFlightNo()
		{
			Declaration.JE_VoyageFlightNo = ZString.Empty;
			AssertEquals("Empty VoyageFlight No invalid", true, Declaration.JE_VoyageFlightNoInfo.HasMessageErrors());

			Declaration.JE_VoyageFlightNo = "QF23";
			AssertEquals("Valid", false, Declaration.JE_VoyageFlightNoInfo.HasMessageErrors());
		}

		public void TestValidateJE_RV_Vessel()
		{
			Declaration.JE_TransportMode = "SEA";
			Declaration.JE_VesselName = ZString.Empty;
			AssertHasMessageError("Empty Vessel for sea invalid", Declaration.JE_VesselNameInfo, "You have not entered a Vessel.");

			Declaration.JE_TransportMode = "AIR";
			Declaration.JE_VesselName = ZString.Empty;
			AssertNoMessageErrors("Empty Vessel for air OK", Declaration.JE_VesselNameInfo);

			Declaration.JE_TransportMode = "SEA";
			Declaration.JE_VesselName = "FAKE VESSEL";
			AssertHasMessageError("Invalid Vessel name", Declaration.JE_VesselNameInfo, "The code you have selected is not in the list.");

			var vessel = Factory.Load<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, "8811924")).FirstOrDefault();
			Declaration.JE_VesselName = vessel.RV_Code;
			AssertNoMessageError("Empty Vessel for sea invalid", Declaration.JE_VesselNameInfo, "The code you have selected is not in the list.");
		}

		public void TestValidateJE_RL_NKDestination()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_RL_NKFinalDestination = ZString.Empty;
			AssertEquals("Empty Destination is OK for IMP", false, Declaration.JE_RL_NKFinalDestinationInfo.HasMessageErrors());

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_RL_NKFinalDestination = ZString.Empty;

			Declaration.Validation.ValidateJE_RL_NKFinalDestination();
			AssertEquals("Empty is not a valid destination for Export", true, Declaration.JE_RL_NKFinalDestinationInfo.HasMessageErrors());

			Declaration.JE_RL_NKFinalDestination = "NZAKL";
			AssertEquals("NZAKL is not a valid destination for Export", true, Declaration.JE_RL_NKFinalDestinationInfo.HasMessageErrors());

			Declaration.JE_RL_NKFinalDestination = "AUSYD";
			AssertEquals("Valid", false, Declaration.JE_RL_NKFinalDestinationInfo.HasMessageErrors());
		}

		public void TestValidateJE_OA_WarehouseAddress()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.WarehouseDocAddress.E2_OA_Address = ZGuid.Empty;
			AssertEquals("Is valid to have no Bond Store for Import Jobs", false, Declaration.WarehouseDocAddress.E2_OA_AddressInfo.HasMessageErrors());

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Excise;
			Declaration.WarehouseDocAddress.Validation.ValidateE2_OA_Address();
			AssertEquals("Not valid to have no Bond Store for Excise Jobs", true, Declaration.WarehouseDocAddress.E2_OA_AddressInfo.HasMessageErrors());

			OrgHeader bondStore = OrgHeader.New(Factory);
			bondStore.OH_Code = "ZZAKBOND";
			bondStore.OH_FullName = "AUCKLAND BOND STORE";
			bondStore.MainAddress.OA_Address1 = "TEST CODE FOR NZ CUSTOMS";
			bondStore.MainAddress.OA_Address2 = "LOCATED IN NZAKL";
			bondStore.OH_RL_NKClosestPort = "NZAKL";

			Declaration.WarehouseDocAddress.E2_OA_Address = bondStore.MainAddress.PK;
			AssertEquals("Is not valid to have a Bond Store with no Bond ID", true, Declaration.WarehouseDocAddress.E2_OA_AddressInfo.HasMessageErrors());

			OrgCusCode cusCode = bondStore.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			cusCode.OK_OA_PremisesAddress = bondStore.MainAddress.PK;
			cusCode.OK_CustomsRegNo = "1234Z";
			cusCode.OK_RN_NKCodeCountry = "NZ";

			Declaration.WarehouseDocAddress.Validation.ValidateE2_OA_Address();
			AssertEquals("Is valid to have a Bond Store with a Bond ID", false, Declaration.WarehouseDocAddress.E2_OA_AddressInfo.HasMessageErrors());
		}

		public void TestValidateJE_RL_NKProcessingPort()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Excise;
			Declaration.JE_RL_NKProcessingPort = ZString.Empty;
			AssertHasMessageError(Declaration.JE_RL_NKProcessingPortInfo, NZAddInfoValidation.ExciseEntryMissingProcessingPort);
		}

		public void TestValidateJE_EntryAuthorisationDate()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Periodic;
			Declaration.JE_EntryAuthorisationDate = ZDateTime.Empty;
			AssertEquals("Entry authorisation date", true, Declaration.JE_EntryAuthorisationDateInfo.HasMessageErrors());
		}

		public void TestValidateJE_EntryAuthorisationDateForExcise()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Excise;
			Declaration.JE_EntryAuthorisationDate = ZDateTime.Empty;
			AssertEquals("Entry authorisation date", true, Declaration.JE_EntryAuthorisationDateInfo.HasMessageErrors());
		}

		public void TestValidateJE_OH_Importer()
		{
			OrgHeader importer = OrgHeader.New(Factory);
			importer.OH_FullName = "Test Importer";

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_OH_Importer = importer.PK;
			AssertEquals("JE_OH_ImporterInfo.HasMessageError(JobDeclarationValidation.OrganisationMissingCCDCustomsCode)", true, Declaration.JE_OH_ImporterInfo.HasMessageError(JobDeclarationValidationFormalEntry.OrganisationCCDCustomsCodeMissing));
			AssertEquals("JE_OH_ImporterInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat)", false, Declaration.JE_OH_ImporterInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat));

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			Declaration.JE_OH_Importer = importer.PK;
			AssertEquals("JE_OH_ImporterInfo.HasMessageError(JobDeclarationValidation.OrganisationMissingCCDCustomsCode)", false, Declaration.JE_OH_ImporterInfo.HasMessageError(JobDeclarationValidationFormalEntry.OrganisationCCDCustomsCodeMissing));
			AssertEquals("JE_OH_ImporterInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat)", false, Declaration.JE_OH_ImporterInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat));

			OrgCusCode cusCode = importer.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
			cusCode.OK_CountryDefault = true;
			cusCode.OK_CustomsRegNo = "989083B";
			cusCode.OK_RN_NKCodeCountry = "NZ";

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_OH_Importer = importer.PK;
			AssertEquals("JE_OH_ImporterInfo.HasMessageError(JobDeclarationValidation.OrganisationMissingCCDCustomsCode)", false, Declaration.JE_OH_ImporterInfo.HasMessageError(JobDeclarationValidationFormalEntry.OrganisationCCDCustomsCodeMissing));
			AssertEquals("JE_OH_ImporterInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat)", false, Declaration.JE_OH_ImporterInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat));

			cusCode.OK_CustomsRegNo = "1001001K";
			Declaration.JE_OH_Importer = importer.PK;
			AssertEquals("JE_OH_ImporterInfo.HasMessageError(JobDeclarationValidation.OrganisationMissingCCDCustomsCode)", true, Declaration.JE_OH_ImporterInfo.HasMessageError(JobDeclarationValidationFormalEntry.OrganisationCCDCustomsCodeMissing));
			AssertEquals("JE_OH_ImporterInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat)", true, Declaration.JE_OH_ImporterInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat));

			importer.CustomsCodes.RemoveAndDelete(cusCode);
			Declaration.JE_OH_Importer = importer.PK;
			AssertEquals("JE_OH_ImporterInfo.HasMessageError(JobDeclarationValidation.OrganisationMissingCCDCustomsCode)", true, Declaration.JE_OH_ImporterInfo.HasMessageError(JobDeclarationValidationFormalEntry.OrganisationCCDCustomsCodeMissing));
			AssertEquals("JE_OH_ImporterInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat)", false, Declaration.JE_OH_ImporterInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat));

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_OH_Importer = importer.PK;
			AssertEquals("JE_OH_ImporterInfo.HasMessageError(JobDeclarationValidation.OrganisationMissingCCDCustomsCode)", false, Declaration.JE_OH_ImporterInfo.HasMessageError(JobDeclarationValidationFormalEntry.OrganisationCCDCustomsCodeMissing));
			AssertEquals("JE_OH_ImporterInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat)", false, Declaration.JE_OH_ImporterInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat));

			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;

			Declaration.JE_OH_Importer = importer.PK;
			AssertEquals("Allocated contact not present", true, Declaration.JE_OH_ImporterInfo.HasMessageError("This Organization has no allocated contact person for New Zealand Customs and no communication information entered.\r\nEdit the Organization to add the company telephone/mobile, fax and/or email details and add or edit a contact as this organizations primary contact representative."));

			var importerContact = importer.Contacts.AddNew();
			importerContact.OC_ContactName = "Bill Brown";
			importerContact.OC_Phone = "+61 2 7820 5600";
			importerContact.OC_Email = "bill.brown@importer.org.au";
			var allocatedContact = importerContact.Allocations.AddNew();
			allocatedContact.PC_Type = OrgConstants.ContactAllocationType.NZCustoms;

			Declaration.JE_OH_Importer = importer.PK;
			AssertEquals("Allocated contact present", false, Declaration.JE_OH_ImporterInfo.HasMessageError("This Organization has no mandatory communication information details, required by New Zealand Customs, entered.\r\nEdit the Organization to add the company telephone/mobile, fax and/or email details.\r\nAlternatively, update Bill Brown, the allocated contact for this Organization, with their phone/fax/email details."));

			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			importer.CustomsCodes.RemoveAndDeleteAll();
			Declaration.JE_OH_Importer = importer.PK;
			AssertEquals("MissingCCDCustomsCode error does not apply for TSW IPI entry)", false, Declaration.JE_OH_ImporterInfo.HasMessageError(JobDeclarationValidationFormalEntry.OrganisationCCDCustomsCodeMissing));

			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			AssertEquals("MissingCCDCustomsCode error should still apply to legacy IPI entry", true, Declaration.JE_OH_ImporterInfo.HasMessageError(JobDeclarationValidationFormalEntry.OrganisationCCDCustomsCodeMissing));

			Declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.Simplified;
			AssertEquals("MissingCCDCustomsCode error should not apply to Simplified entry", false, Declaration.JE_OH_ImporterInfo.HasMessageError(JobDeclarationValidationFormalEntry.OrganisationCCDCustomsCodeMissing));
		}

		public void TestCheckContactEmailMaxLength()
		{
			AssertContactEmailLength(Declaration.JE_OH_ImporterInfo, "bill.brown@importer.org.au", hasMessageError: false);
			AssertContactEmailLength(Declaration.JE_OH_ImporterInfo, "bill.brown.brown.brown.brown.brown.brown.brown.brown.brown.brown.brown@importer.org.au", hasMessageError: true);
			AssertContactEmailLength(Declaration.JE_OH_SupplierInfo, "bill.brown@importer.org.au", hasMessageError: false);
			AssertContactEmailLength(Declaration.JE_OH_SupplierInfo, "bill.brown.brown.brown.brown.brown.brown.brown.brown.brown.brown.brown@importer.org.au", hasMessageError: true);
		}

		void AssertContactEmailLength(ZPropertyInfo propertyInfo, string email, bool hasMessageError)
		{
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;

			OrgHeader header = OrgHeader.New(Factory);
			header.OH_FullName = "Test Header";
			var contact = header.Contacts.AddNew();
			contact.OC_ContactName = "Bill Brown";
			contact.OC_Phone = "+61 2 7820 5600";
			contact.OC_Email = email;
			var allocatedContact = contact.Allocations.AddNew();
			allocatedContact.PC_Type = OrgConstants.ContactAllocationType.NZCustoms;
			propertyInfo.Value = header.PK;
			AssertEquals(hasMessageError, propertyInfo.HasMessageError("Contact email address exceeds 50 characters."));
		}

		public void TestValidateMiscOrgContactDetails()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.Validation.ValidateJE_OH_Importer();
			AssertNoMessageError("Importer contact details no error", Declaration.JE_OH_ImporterInfo, "Importer contact with phone, fax or email is mandatory for NZ Customs.\r\nPlease override the 'Consignee Documentary Address' values in the 'Addresses' tab for this Importer and enter the required contact name and contact details.");

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Declaration.JE_JS = shipment.PK;
			shipment.ConsignorPK = Factory.New<OrgHeader>().PK;
			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			ZGuid miscOrgPK = Declaration.CachedMiscOrgPK;
			Declaration.JE_OH_Importer = miscOrgPK;
			Declaration.Validation.ValidateJE_OH_Importer();
			AssertHasMessageError("Importer contact details has error", Declaration.JE_OH_ImporterInfo, "Importer contact with phone, fax or email is mandatory for NZ Customs.\r\nPlease override the 'Consignee Documentary Address' values in the 'Addresses' tab for this Importer and enter the required contact name and contact details.");
		}

		public void TestValidateJE_OH_Supplier()
		{
			OrgHeader supplier = OrgHeader.New(Factory);
			supplier.OH_FullName = "Test Supplier";

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidation.OrganisationCCDCustomsCodeMissing)", false, Declaration.JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidationFormalEntry.OrganisationCCDCustomsCodeMissing));
			AssertEquals("JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidation.OrganisationCSCCustomsCodeMissing)", true, Declaration.JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidationFormalEntry.OrganisationCSCCustomsCodeMissing));
			AssertEquals("JE_OH_SupplierInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat)", false, Declaration.JE_OH_SupplierInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat));

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			Declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidation.OrganisationCCDCustomsCodeMissing)", false, Declaration.JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidationFormalEntry.OrganisationCCDCustomsCodeMissing));
			AssertEquals("JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidation.OrganisationCSCCustomsCodeMissing)", false, Declaration.JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidationFormalEntry.OrganisationCSCCustomsCodeMissing));
			AssertEquals("JE_OH_SupplierInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat)", false, Declaration.JE_OH_SupplierInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat));

			OrgCusCode cusCode = supplier.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.SupplierCode;
			cusCode.OK_CountryDefault = true;
			cusCode.OK_CustomsRegNo = "00970890N";
			cusCode.OK_RN_NKCodeCountry = "NZ";

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidation.OrganisationCCDCustomsCodeMissing)", false, Declaration.JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidationFormalEntry.OrganisationCCDCustomsCodeMissing));
			AssertEquals("JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidation.OrganisationCSCCustomsCodeMissing)", false, Declaration.JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidationFormalEntry.OrganisationCSCCustomsCodeMissing));
			AssertEquals("JE_OH_SupplierInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat)", false, Declaration.JE_OH_SupplierInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat));

			cusCode.OK_CustomsRegNo = "1001001K";
			Declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidation.OrganisationCCDCustomsCodeMissing)", false, Declaration.JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidationFormalEntry.OrganisationCCDCustomsCodeMissing));
			AssertEquals("JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidation.OrganisationCSCCustomsCodeMissing)", true, Declaration.JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidationFormalEntry.OrganisationCSCCustomsCodeMissing));
			AssertEquals("JE_OH_SupplierInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat)", true, Declaration.JE_OH_SupplierInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat));

			supplier.CustomsCodes.RemoveAndDelete(cusCode);
			Declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidation.OrganisationCCDCustomsCodeMissing)", false, Declaration.JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidationFormalEntry.OrganisationCCDCustomsCodeMissing));
			AssertEquals("JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidation.OrganisationCSCCustomsCodeMissing)", true, Declaration.JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidationFormalEntry.OrganisationCSCCustomsCodeMissing));
			AssertEquals("JE_OH_SupplierInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat)", false, Declaration.JE_OH_SupplierInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat));

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_OH_Supplier = supplier.PK;

			AssertEquals("JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidation.OrganisationCCDCustomsCodeMissing)", true, Declaration.JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidationFormalEntry.OrganisationCCDCustomsCodeMissing));
			AssertEquals("JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidation.OrganisationCSCCustomsCodeMissing)", false, Declaration.JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidationFormalEntry.OrganisationCSCCustomsCodeMissing));
			AssertEquals("JE_OH_SupplierInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat)", false, Declaration.JE_OH_SupplierInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat));

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			Declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidation.OrganisationCCDCustomsCodeMissing)", false, Declaration.JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidationFormalEntry.OrganisationCCDCustomsCodeMissing));
			AssertEquals("JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidation.OrganisationCSCCustomsCodeMissing)", false, Declaration.JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidationFormalEntry.OrganisationCSCCustomsCodeMissing));
			AssertEquals("JE_OH_SupplierInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat)", false, Declaration.JE_OH_SupplierInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat));

			cusCode = supplier.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
			cusCode.OK_CountryDefault = true;
			cusCode.OK_CustomsRegNo = "989083B";
			cusCode.OK_RN_NKCodeCountry = "NZ";

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidation.OrganisationCCDCustomsCodeMissing)", false, Declaration.JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidationFormalEntry.OrganisationCCDCustomsCodeMissing));
			AssertEquals("JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidation.OrganisationCSCCustomsCodeMissing)", false, Declaration.JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidationFormalEntry.OrganisationCSCCustomsCodeMissing));
			AssertEquals("JE_OH_SupplierInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat)", false, Declaration.JE_OH_SupplierInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat));

			cusCode.OK_CustomsRegNo = "1001001K";
			Declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidation.OrganisationCCDCustomsCodeMissing)", true, Declaration.JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidationFormalEntry.OrganisationCCDCustomsCodeMissing));
			AssertEquals("JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidation.OrganisationCSCCustomsCodeMissing)", false, Declaration.JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidationFormalEntry.OrganisationCSCCustomsCodeMissing));
			AssertEquals("JE_OH_SupplierInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat)", true, Declaration.JE_OH_SupplierInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat));

			supplier.CustomsCodes.RemoveAndDelete(cusCode);
			Declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidation.OrganisationCCDCustomsCodeMissing)", true, Declaration.JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidationFormalEntry.OrganisationCCDCustomsCodeMissing));
			AssertEquals("JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidation.OrganisationCSCCustomsCodeMissing)", false, Declaration.JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidationFormalEntry.OrganisationCSCCustomsCodeMissing));
			AssertEquals("JE_OH_SupplierInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat)", false, Declaration.JE_OH_SupplierInfo.HasMessageError(NZCustomsCodeValidator.CustomsCodeInWrongBasicFormat));

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = Enterprise.Customs.NZ.TradeSingleWindow.MessageTypeList.Codes.IPI;
			cusCode = supplier.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.SupplierCode;
			cusCode.OK_CountryDefault = true;
			cusCode.OK_RN_NKCodeCountry = "NZ";
			cusCode.OK_CustomsRegNo = "";
			Declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidation.OrganisationCSCCustomsCodeMissing)", false, Declaration.JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidationFormalEntry.OrganisationCSCCustomsCodeMissing));

			supplier.LocalCustomsSupplierCode = "Abc";
			Declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidation.OrganisationCSCCustomsCodeMissing)", false, Declaration.JE_OH_SupplierInfo.HasMessageError(JobDeclarationValidationFormalEntry.OrganisationCSCCustomsCodeMissing));
		}

		public void TestSettingJE_MessageTypeTriggersValidationOnJE_EDITransmitDate()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			declaration.JE_ExportDate = declaration.CachedTodaysDate;
			declaration.JE_EDITransmitDate = declaration.CachedTodaysDate.AddDays(-1);
			using (declaration.SuspendValidationTesting())
			{
				declaration.JE_EDITransmitDateInfo.ClearAllNotifications();
				AssertNoMessageError(declaration.JE_EDITransmitDateInfo, EDITransmitDateManager.ErrorEDITransmitDateInThePast);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertHasMessageErrorContaining(declaration.JE_EDITransmitDateInfo, EDITransmitDateManager.ErrorEDITransmitDateInThePast);
			}
		}

		public void TestSettingJE_TransportModeTriggersValidationOnJE_EDITransmitDate()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ExportDate = declaration.CachedTodaysDate.AddDays(-100);
			using (declaration.SuspendValidationTesting())
			{
				declaration.JE_EDITransmitDateInfo.ClearAllNotifications();
				AssertNoMessageErrors(declaration.JE_EDITransmitDateInfo);
				declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
				AssertNoMessageErrors(declaration.JE_EDITransmitDateInfo);
			}
		}

		public void TestSettingJE_ExportDateTriggersValidationOnJE_EDITransmitDate()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			declaration.JE_EDITransmitDate = declaration.CachedTodaysDate.AddDays(-1);
			using (declaration.SuspendValidationTesting())
			{
				declaration.JE_EDITransmitDateInfo.ClearAllNotifications();
				AssertNoMessageErrors(declaration.JE_EDITransmitDateInfo);
				declaration.JE_ExportDate = declaration.CachedTodaysDate.AddDays(-100);
				AssertNoMessageErrors(declaration.JE_EDITransmitDateInfo);
			}
		}

		public void TestValidateAllValidatesProcessingPort()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Excise;
			declaration.JE_RL_NKProcessingPort = "";
			using (declaration.SuspendValidationTesting())
			{
				declaration.JE_RL_NKProcessingPortInfo.ClearAllNotifications();
				AssertNoMessageError(declaration.JE_RL_NKProcessingPortInfo, NZAddInfoValidation.ExciseEntryMissingProcessingPort);
				declaration.RunPreSaveValidation();
				AssertHasMessageError(declaration.JE_RL_NKProcessingPortInfo, NZAddInfoValidation.ExciseEntryMissingProcessingPort);
			}
		}

		public void TestValidateJE_PaymentParty()
		{
			Declaration.JE_PaymentMethod = "";
			AssertHasMessageError(Declaration.JE_PaymentMethodInfo, "You have not entered a Payment Method.");
			Declaration.JE_PaymentMethod = "ZZZ";
			AssertHasMessageError(Declaration.JE_PaymentMethodInfo, ListValidation.InvalidCodeMessageError);
			Declaration.JE_PaymentMethod = PaymentMethodList.Codes.BrokerDeferred;
			AssertNoNotifications(Declaration.JE_PaymentMethodInfo);
		}

		public void TestCheckJE_PaymentMethod()
		{
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_PaymentMethod = "";
			AssertHasMessageError(Declaration.JE_PaymentMethodInfo, "You have not entered a Payment Method.");

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_PaymentMethod = "";
			AssertNoMessageError(Declaration.JE_PaymentMethodInfo, "You have not entered a Payment Method.");

			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_PaymentMethod = "";
			AssertNoMessageError("For TSW Export, payment method can validly be left blank", Declaration.JE_PaymentMethodInfo, "You have not entered a Payment Method.");

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Drawback;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_PaymentMethod = "";
			AssertHasMessageError(Declaration.JE_PaymentMethodInfo, Enterprise.Customs.NZ.Business.Declaration.JobDeclarationValidationFormalEntry.TSWMessageError.PaymentMethodInvalid);

			Declaration.JE_PaymentMethod = PaymentMethodList.Codes.BrokerDeferred;
			AssertHasMessageError(Declaration.JE_PaymentMethodInfo, Enterprise.Customs.NZ.Business.Declaration.JobDeclarationValidationFormalEntry.TSWMessageError.PaymentMethodInvalid);

			Declaration.JE_PaymentMethod = PaymentMethodList.Codes.CashPaidByBroker;
			AssertNoMessageError(Declaration.JE_PaymentMethodInfo, Enterprise.Customs.NZ.Business.Declaration.JobDeclarationValidationFormalEntry.TSWMessageError.PaymentMethodInvalid);

			Declaration.JE_PaymentMethod = PaymentMethodList.Codes.CashPaidByClient;
			AssertNoMessageError(Declaration.JE_PaymentMethodInfo, Enterprise.Customs.NZ.Business.Declaration.JobDeclarationValidationFormalEntry.TSWMessageError.PaymentMethodInvalid);

			Declaration.JE_PaymentMethod = PaymentMethodList.Codes.ClientDeferred;
			AssertNoMessageError(Declaration.JE_PaymentMethodInfo, Enterprise.Customs.NZ.Business.Declaration.JobDeclarationValidationFormalEntry.TSWMessageError.PaymentMethodInvalid);
		}

		public void TestPaymentCannotBeChangedIfCleared()
		{
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_PaymentMethod = PaymentMethodList.Codes.BrokerDeferred;
			Factory.Save();
			AssertNoError(Declaration.JE_PaymentMethodInfo, JobDeclarationValidationFormalEntry.ErrorIfPaymentMethodChanged);

			Declaration.JE_TSWCombinedStatus = "000";
			Declaration.JE_PaymentMethod = PaymentMethodList.Codes.CashPaidByClient;
			AssertHasError(Declaration.JE_PaymentMethodInfo, JobDeclarationValidationFormalEntry.ErrorIfPaymentMethodChanged);
		}

		public void TestCheckJE_MessageType()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertNoMessageError(Declaration.JE_MessageTypeInfo, Enterprise.Customs.NZ.Business.Declaration.JobDeclarationValidationFormalEntry.MessageErorWhenExciseIsSelected);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertNoMessageError(Declaration.JE_MessageTypeInfo, Enterprise.Customs.NZ.Business.Declaration.JobDeclarationValidationFormalEntry.MessageErorWhenExciseIsSelected);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Excise;
			AssertHasMessageError(Declaration.JE_MessageTypeInfo, Enterprise.Customs.NZ.Business.Declaration.JobDeclarationValidationFormalEntry.MessageErorWhenExciseIsSelected);
		}

		protected override JobDeclaration GetNewJobDeclaration()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.DisableDefaultPackingInformation = true;
			return declaration;
		}
	}
}

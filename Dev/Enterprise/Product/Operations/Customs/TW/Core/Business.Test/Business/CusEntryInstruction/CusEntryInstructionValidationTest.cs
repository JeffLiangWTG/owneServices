using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusEntryInstructionValidation))]
	sealed class CusEntryInstructionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCEI_RORPaymentMethod()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var cusEntryInstruction = jobDeclaration.CusEntryInstruction;
			var targetInfo = cusEntryInstruction.CEI_RORPaymentMethodInfo;
			ValidationTestHelper.AssertInvalidCodeMessageError(targetInfo, "CAS", "ROR");

			jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew().JI_Procedure = Constants.ProcedureCodes._38;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);
		}

		public void TestCheckCEI_OA_Warehouse()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var cusEntryInstruction = jobDeclaration.CusEntryInstruction;
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg1.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			var testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg2.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			testOrg2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "11111111", Core.Constants.CountryCodes.Taiwan);
			var testOrg3 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg3.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			testOrg3.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "88888888", Core.Constants.CountryCodes.Taiwan);
			testOrg3.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "22222222", Core.Constants.CountryCodes.Canada);
			var testOrg4 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg4.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			testOrg4.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "00612345", Core.Constants.CountryCodes.Taiwan);
			testOrg4.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "33333333", Core.Constants.CountryCodes.Canada);
			var testOrg5 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg5.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			testOrg5.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "", Core.Constants.CountryCodes.Taiwan);
			testOrg5.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "11111111", Core.Constants.CountryCodes.Taiwan);
			var warehouseInfo = cusEntryInstruction.CEI_OA_WarehouseInfo;

			CombineAssertions("Testing From Bonded Warehouse Should Not Enter", () =>
			{
				AssertFromBondedWarehouseShouldNotEnter(Constants.DeclarationTypes.Export.B1);
				AssertFromBondedWarehouseShouldNotEnter(Constants.DeclarationTypes.Export.B2);
				AssertFromBondedWarehouseShouldNotEnter(Constants.DeclarationTypes.Import.B6);
				AssertFromBondedWarehouseShouldNotEnter(Constants.DeclarationTypes.Export.B8);
				AssertFromBondedWarehouseShouldNotEnter(Constants.DeclarationTypes.Export.B9);
				AssertFromBondedWarehouseShouldNotEnter(Constants.DeclarationTypes.Export.D1);
				AssertFromBondedWarehouseShouldNotEnter(Constants.DeclarationTypes.Import.D8);
				AssertFromBondedWarehouseShouldNotEnter(Constants.DeclarationTypes.Import.L1);
				AssertFromBondedWarehouseShouldNotEnter(Constants.DeclarationTypes.Import.F1);
				AssertFromBondedWarehouseShouldNotEnter(Constants.DeclarationTypes.Import.F2);
				AssertFromBondedWarehouseShouldNotEnter(Constants.DeclarationTypes.Import.F3);
				AssertFromBondedWarehouseShouldNotEnter(Constants.DeclarationTypes.Export.F4);
				AssertFromBondedWarehouseShouldNotEnter(Constants.DeclarationTypes.Export.F5);
				AssertFromBondedWarehouseShouldNotEnter(Constants.DeclarationTypes.Import.G1);
				AssertFromBondedWarehouseShouldNotEnter(Constants.DeclarationTypes.Import.G2);
				AssertFromBondedWarehouseShouldNotEnter(Constants.DeclarationTypes.Import.G7);
			});

			CombineAssertions("Testing From Bonded Warehouse does not have a Government VAT", () =>
			{
				AssertFromBondedWarehouseDoesNotHaveAGovernmentVAT(Constants.DeclarationTypes.Import.D2);
				AssertFromBondedWarehouseDoesNotHaveAGovernmentVAT(Constants.DeclarationTypes.Export.D5);
				AssertFromBondedWarehouseDoesNotHaveAGovernmentVAT(Constants.DeclarationTypes.Import.D7);
			});

			CombineAssertions("Testing From Bonded Warehouse is required", () =>
			{
				AssertFromBondedWarehouseIsRequired(Constants.DeclarationTypes.Import.D2);
				AssertFromBondedWarehouseIsRequired(Constants.DeclarationTypes.Export.D5);
				AssertFromBondedWarehouseIsRequired(Enterprise.Customs.TW.Business.Constants.DeclarationTypes.Import.D7);
			});

			void AssertFromBondedWarehouseShouldNotEnter(ZString ceiStyle)
			{
				var error = string.Format("From Bonded Warehouse Bonded ID is not required for Declaration Type {0}.", ceiStyle);
				cusEntryInstruction.CEI_Style = ceiStyle;
				cusEntryInstruction.CEI_OA_Warehouse = testOrg1.MainAddress.PK;
				AssertHasMessageError(warehouseInfo, error);

				cusEntryInstruction.CEI_OA_Warehouse = Guid.Empty;
				AssertNoMessageError(warehouseInfo, error);
			}

			void AssertFromBondedWarehouseDoesNotHaveAGovernmentVAT(ZString ceiStyle)
			{
				var error = "A valid TW-VAT number is required for From Bonded Warehouse. To create a valid TW-VAT, visit Organization > Details > Config > Registration.";
				cusEntryInstruction.CEI_Style = ceiStyle;
				cusEntryInstruction.CEI_OA_Warehouse = testOrg1.MainAddress.PK;
				AssertHasMessageError(warehouseInfo, error);

				cusEntryInstruction.CEI_OA_Warehouse = testOrg2.MainAddress.PK;
				AssertNoMessageError(warehouseInfo, error);

				cusEntryInstruction.CEI_OA_Warehouse = testOrg3.MainAddress.PK;
				AssertHasMessageError(warehouseInfo, error);

				cusEntryInstruction.CEI_OA_Warehouse = testOrg4.MainAddress.PK;
				AssertHasMessageError(warehouseInfo, error);

				cusEntryInstruction.CEI_OA_Warehouse = testOrg5.MainAddress.PK;
				AssertNoMessageError(warehouseInfo, error);
			}

			void AssertFromBondedWarehouseIsRequired(ZString ceiStyle)
			{
				var error = string.Format("From Bonded Warehouse is required for Declaration Type {0}.", ceiStyle);
				cusEntryInstruction.CEI_Style = ceiStyle;
				cusEntryInstruction.CEI_OA_Warehouse = ZGuid.Empty;
				cusEntryInstruction.CEI_OA_Warehouse = testOrg1.MainAddress.PK;
				AssertNoMessageError(warehouseInfo, error);

				cusEntryInstruction.CEI_OA_Warehouse = ZGuid.Empty;
				AssertHasMessageError(warehouseInfo, error);
			}
		}

		public void TestCheckCEI_OA_WarehouseWithoutCCPAndCPW()
		{
			var messageText = "From Bonded Warehouse should be of Organization Registration Code CCP or CPW. To create a valid TW-CCP or TW-CPW, visit Organization > Details > Config > Registration.";
			var jobDeclaration = Factory.New<JobDeclaration>();
			var cusEntryInstruction = jobDeclaration.CusEntryInstruction;
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			var mainAddress = testOrg.MainAddress;
			mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			mainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "CCP111", Core.Constants.CountryCodes.Taiwan);

			var address1 = testOrg.Addresses.AddNew();
			address1.OA_Address1 = "TEST1";
			address1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "CPW111", Core.Constants.CountryCodes.Taiwan);

			var address2 = testOrg.Addresses.AddNew();
			address2.OA_Address1 = "TEST1";
			address2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsCPPermitCode, "CPC111", Core.Constants.CountryCodes.Taiwan);

			cusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B1;
			cusEntryInstruction.CEI_OA_Warehouse = mainAddress.PK;
			AssertNoMessageError(cusEntryInstruction.CEI_OA_WarehouseInfo, messageText);

			cusEntryInstruction.CEI_OA_Warehouse = address1.PK;
			AssertNoMessageError(cusEntryInstruction.CEI_OA_WarehouseInfo, messageText);

			cusEntryInstruction.CEI_OA_Warehouse = address2.PK;
			AssertHasMessageError(cusEntryInstruction.CEI_OA_WarehouseInfo, messageText);
		}

		public void TestCheckCEI_OA_Warehouse2()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var cusEntryInstruction = jobDeclaration.CusEntryInstruction;
			var importerDocumentaryAddress = jobDeclaration.ImporterDocumentaryAddress;
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg1.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			var testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg2.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			testOrg2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "11111111", Core.Constants.CountryCodes.Taiwan);
			var testOrg3 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg3.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			testOrg3.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "88888888", Core.Constants.CountryCodes.Taiwan);
			testOrg3.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "22222222", Core.Constants.CountryCodes.Canada);
			var testOrg4 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg4.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			testOrg4.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "00612345", Core.Constants.CountryCodes.Taiwan);
			testOrg4.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "33333333", Core.Constants.CountryCodes.Canada);
			var testOrg5 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg5.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			testOrg5.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "", Core.Constants.CountryCodes.Taiwan);
			testOrg5.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "11111111", Core.Constants.CountryCodes.Taiwan);
			var warehouse2Info = cusEntryInstruction.CEI_OA_Warehouse2Info;
			jobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			CombineAssertions("Testing To Bonded Warehouse Should Not Enter", () =>
			{
				AssertToBondedWarehouseShouldNotEnter(Constants.DeclarationTypes.Export.B1);
				AssertToBondedWarehouseShouldNotEnter(Constants.DeclarationTypes.Import.B6);
				AssertToBondedWarehouseShouldNotEnter(Constants.DeclarationTypes.Export.B8);
				AssertToBondedWarehouseShouldNotEnter(Constants.DeclarationTypes.Export.B9);
				AssertToBondedWarehouseShouldNotEnter(Constants.DeclarationTypes.Import.D2);
				AssertToBondedWarehouseShouldNotEnter(Constants.DeclarationTypes.Export.D5);
				AssertToBondedWarehouseShouldNotEnter(Constants.DeclarationTypes.Import.F1);
				AssertToBondedWarehouseShouldNotEnter(Constants.DeclarationTypes.Import.F2);
				AssertToBondedWarehouseShouldNotEnter(Constants.DeclarationTypes.Import.F3);
				AssertToBondedWarehouseShouldNotEnter(Constants.DeclarationTypes.Export.F4);
				AssertToBondedWarehouseShouldNotEnter(Constants.DeclarationTypes.Export.F5);
				AssertToBondedWarehouseShouldNotEnter(Constants.DeclarationTypes.Import.G1);
				AssertToBondedWarehouseShouldNotEnter(Constants.DeclarationTypes.Import.G2);
				AssertToBondedWarehouseShouldNotEnter(Constants.DeclarationTypes.Import.G7);
			});

			CombineAssertions("Testing To Bonded Warehouse is required", () =>
			{
				AssertToBondedWarehouseIsRequired(Constants.DeclarationTypes.Export.D1);
				AssertToBondedWarehouseIsRequired(Constants.DeclarationTypes.Import.D8);
			});

			CombineAssertions("Testing To Bonded Warehouse does not have a Government VAT", () =>
			{
				AssertToBondedWarehouseDoesNotHaveAGovernmentVAT(Constants.DeclarationTypes.Export.D1);
				AssertToBondedWarehouseDoesNotHaveAGovernmentVAT(Constants.DeclarationTypes.Import.D8);
				AssertToBondedWarehouseDoesNotHaveAGovernmentVAT(Constants.DeclarationTypes.Export.B1);
				AssertToBondedWarehouseDoesNotHaveAGovernmentVAT(Constants.DeclarationTypes.Import.B6);
				AssertToBondedWarehouseDoesNotHaveAGovernmentVAT(Constants.DeclarationTypes.Export.B2);
				AssertToBondedWarehouseDoesNotHaveAGovernmentVAT(Constants.DeclarationTypes.Import.D7);
				AssertToBondedWarehouseDoesNotHaveAGovernmentVAT(Constants.DeclarationTypes.Export.B8);
				AssertToBondedWarehouseDoesNotHaveAGovernmentVAT(Constants.DeclarationTypes.Export.B9);
				AssertToBondedWarehouseDoesNotHaveAGovernmentVAT(Constants.DeclarationTypes.Export.D5);
				AssertToBondedWarehouseDoesNotHaveAGovernmentVAT(Constants.DeclarationTypes.Import.F1);
				AssertToBondedWarehouseDoesNotHaveAGovernmentVAT(Constants.DeclarationTypes.Import.F3);
				AssertToBondedWarehouseDoesNotHaveAGovernmentVAT(Constants.DeclarationTypes.Export.F4);
				AssertToBondedWarehouseDoesNotHaveAGovernmentVAT(Constants.DeclarationTypes.Import.G1);
				AssertToBondedWarehouseDoesNotHaveAGovernmentVAT(Constants.DeclarationTypes.Import.G7);
				AssertToBondedWarehouseDoesNotHaveAGovernmentVAT(Constants.DeclarationTypes.Import.L1);
			});

			CombineAssertions("Testing To Bonded Warehouse Can Not CoExist Or Empty With ImporterBondedId", () =>
			{
				AssertToBondedWarehouseCanNotCoExistOrEmptyWithImporterBondedId(Constants.DeclarationTypes.Export.B2);
				AssertToBondedWarehouseCanNotCoExistOrEmptyWithImporterBondedId(Constants.DeclarationTypes.Import.D7);
			});

			CombineAssertions("To Bonded Warehouse is required for Declaration Type L1.", () =>
			{
				cusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Import.L1;
				cusEntryInstruction.CEI_OA_Warehouse2 = Guid.Empty;
				AssertHasMessageError(warehouse2Info, "To Bonded Warehouse is required for Declaration Type L1.");

				cusEntryInstruction.CEI_OA_Warehouse2 = testOrg1.MainAddress.PK;
				AssertNoMessageError(warehouse2Info, "To Bonded Warehouse is required for Declaration Type L1.");

				cusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G2;
				cusEntryInstruction.CEI_OA_Warehouse2 = Guid.Empty;
				AssertNoMessageError(warehouse2Info, "To Bonded Warehouse is required for Declaration Type L1.");
			});

			void AssertToBondedWarehouseShouldNotEnter(ZString ceiStyle)
			{
				var error = string.Format("To Bonded Warehouse Bonded ID is not required for Declaration Type {0}.", ceiStyle);
				cusEntryInstruction.CEI_Style = ceiStyle;
				cusEntryInstruction.CEI_OA_Warehouse2 = testOrg1.MainAddress.PK;
				AssertHasMessageError(warehouse2Info, error);

				cusEntryInstruction.CEI_OA_Warehouse2 = Guid.Empty;
				AssertNoMessageError(warehouse2Info, error);
			}

			void AssertToBondedWarehouseDoesNotHaveAGovernmentVAT(ZString ceiStyle)
			{
				var error = "A valid TW-VAT number is required for To Bonded Warehouse. To create a valid TW-VAT, visit Organization > Details > Config > Registration.";
				cusEntryInstruction.CEI_Style = ceiStyle;
				cusEntryInstruction.CEI_OA_Warehouse2 = testOrg1.MainAddress.PK;
				AssertHasMessageError(warehouse2Info, error);

				cusEntryInstruction.CEI_OA_Warehouse2 = testOrg2.MainAddress.PK;
				AssertNoMessageError(warehouse2Info, error);

				cusEntryInstruction.CEI_OA_Warehouse2 = testOrg3.MainAddress.PK;
				AssertHasMessageError(warehouse2Info, error);

				cusEntryInstruction.CEI_OA_Warehouse2 = testOrg4.MainAddress.PK;
				AssertHasMessageError(warehouse2Info, error);

				cusEntryInstruction.CEI_OA_Warehouse2 = testOrg5.MainAddress.PK;
				AssertNoMessageError(warehouse2Info, error);
			}

			void AssertToBondedWarehouseIsRequired(ZString ceiStyle)
			{
				var error = string.Format("To Bonded Warehouse is required for Declaration Type {0}.", ceiStyle);
				cusEntryInstruction.CEI_Style = ceiStyle;
				cusEntryInstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
				cusEntryInstruction.CEI_OA_Warehouse2 = testOrg1.MainAddress.PK;
				AssertNoMessageError(warehouse2Info, error);

				cusEntryInstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
				AssertHasMessageError(warehouse2Info, error);
			}

			void AssertToBondedWarehouseCanNotCoExistOrEmptyWithImporterBondedId(ZString ceiStyle)
			{
				var error = ValidationConstants.TWJobDocAddress.ImporterBondedIdCanNotCoExistOrEmptyWithToBondedWarehouse;
				importerDocumentaryAddress.E2_AddressOverride = true;
				importerDocumentaryAddress.CBPCodeType = "EPZ";
				importerDocumentaryAddress.CBPCode = ZString.Empty;
				cusEntryInstruction.CEI_Style = ceiStyle;
				cusEntryInstruction.CEI_OA_Warehouse2 = testOrg1.MainAddress.PK;
				AssertNoMessageError(warehouse2Info, error);

				importerDocumentaryAddress.CBPCode = "12345678";
				cusEntryInstruction.Validation.ValidateCEI_OA_Warehouse2();
				AssertHasMessageError(warehouse2Info, error);

				cusEntryInstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
				AssertNoMessageError(warehouse2Info, error);

				importerDocumentaryAddress.CBPCode = ZString.Empty;
				cusEntryInstruction.Validation.ValidateCEI_OA_Warehouse2();
				AssertHasMessageError(warehouse2Info, error);
			}
		}

		public void TestCheckCEI_OA_Warehouse2WithoutCCPAndCPW()
		{
			string messageText = "To Bonded Warehouse should be of Organization Registration Code CCP or CPW. To create a valid TW-CCP or TW-CPW, visit Organization > Details > Config > Registration.";
			var jobDeclaration = Factory.New<JobDeclaration>();
			var cusEntryInstruction = jobDeclaration.CusEntryInstruction;
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			var mainAddress = testOrg.MainAddress;
			mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			mainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "CCP111", Core.Constants.CountryCodes.Taiwan);

			var address1 = testOrg.Addresses.AddNew();
			address1.OA_Address1 = "TEST1";
			address1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "CPW111", Core.Constants.CountryCodes.Taiwan);

			var address2 = testOrg.Addresses.AddNew();
			address2.OA_Address1 = "TEST1";
			address2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsCPPermitCode, "CPC111", Core.Constants.CountryCodes.Taiwan);

			cusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B1;
			cusEntryInstruction.CEI_OA_Warehouse2 = mainAddress.PK;
			AssertNoMessageError(cusEntryInstruction.CEI_OA_Warehouse2Info, messageText);

			cusEntryInstruction.CEI_OA_Warehouse2 = address1.PK;
			AssertNoMessageError(cusEntryInstruction.CEI_OA_Warehouse2Info, messageText);

			cusEntryInstruction.CEI_OA_Warehouse2 = address2.PK;
			AssertHasMessageError(cusEntryInstruction.CEI_OA_Warehouse2Info, messageText);
		}

		public void TestCheckTW_TradersRemarks()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var cusEntryInstruction = jobDeclaration.CusEntryInstruction;

			cusEntryInstruction.TW_TradersRemarks = new ZString('X', 512);
			AssertNoNotifications(cusEntryInstruction.TW_TradersRemarksInfo);

			cusEntryInstruction.TW_TradersRemarks = new ZString('X', 513);
			AssertHasWarningContaining(cusEntryInstruction.TW_TradersRemarksInfo, "Only the first 512 characters will be sent to the customs.");

			cusEntryInstruction.TW_TradersRemarks = ZString.Empty;
			cusEntryInstruction.TW_OverrideTradersRemarks = true;
			AssertHasErrorContaining(cusEntryInstruction.TW_TradersRemarksInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckTW_ICIExamLocation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CommodityInspection, "CommodityInspection");
			var placeAA01 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CommodityInspection, "AA01", "Place AA 01", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(placeAA01.PK, RefCusCodeListAttributeTypes.Codes.CustomsOffice, "AA");
			var placeAA02 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CommodityInspection, "AA02", "Place AA 02", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(placeAA02.PK, RefCusCodeListAttributeTypes.Codes.CustomsOffice, "AA");
			var placeAB01 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CommodityInspection, "AB01", "Place AB 01", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(placeAB01.PK, RefCusCodeListAttributeTypes.Codes.CustomsOffice, "AB");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.EntryNumber = "123456";
			var mergedLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = mergedLine.PK;
			var entryInstruction = declaration.CusEntryInstruction;
			invoiceLine.JI_CEI = entryInstruction.PK;
			entryInstruction.CEI_CustomsOffice = "AA";

			entryInstruction.TW_ICIExamTime = ZDateTime.BrettsBirthday;
			entryInstruction.TW_ICIExamLocation = "AA01";
			AssertNoMessageError(entryInstruction.TW_ICIExamLocationInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(entryInstruction.TW_ICIExamLocationInfo, "Examination Zone is expected when Time is set.");

			entryInstruction.TW_ICIExamLocation = "ZZ01";
			AssertHasMessageError(entryInstruction.TW_ICIExamLocationInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(entryInstruction.TW_ICIExamLocationInfo, "Examination Zone is expected when Time is set.");

			entryInstruction.TW_ICIExamLocation = ZString.Empty;
			AssertNoMessageError(entryInstruction.TW_ICIExamLocationInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(entryInstruction.TW_ICIExamLocationInfo, "Examination Zone is expected when Time is set.");
		}

		public void TestCheckTW_ICIExamTime()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.EntryNumber = "123456";
			var mergedLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = mergedLine.PK;
			var entryInstruction = declaration.CusEntryInstruction;
			invoiceLine.JI_CEI = entryInstruction.PK;
			declaration.JE_EntrySubmittedDate = ZDateTime.Today;

			entryInstruction.TW_ICIExamTime = ZDateTime.Today;
			entryInstruction.TW_ICIExamLocation = "AA01";
			AssertNoMessageError(entryInstruction.TW_ICIExamTimeInfo, "Examination Time should be after the Declaration Date.");
			AssertNoMessageError(entryInstruction.TW_ICIExamTimeInfo, "Examination Time is expected when Zone is set.");

			entryInstruction.TW_ICIExamTime = ZDateTime.Today.AddDays(-1);
			AssertHasMessageError(entryInstruction.TW_ICIExamTimeInfo, "Examination Time should be after the Declaration Date.");
			AssertNoMessageError(entryInstruction.TW_ICIExamTimeInfo, "Examination Time is expected when Zone is set.");

			entryInstruction.TW_ICIExamTime = ZDateTime.Empty;
			AssertNoMessageError(entryInstruction.TW_ICIExamTimeInfo, "Examination Time should be after the Declaration Date.");
			AssertHasMessageError(entryInstruction.TW_ICIExamTimeInfo, "Examination Time is expected when Zone is set.");
		}

		public void TestCheckUCRNumber()
		{
			var errorMsg = "UCR Number only accepts Western European languages characters.";
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var testInstruction1 = testDeclaration.CusEntryInstruction;

			testInstruction1.UCROverride = true;
			testInstruction1.UCRNumber = "01A你好";
			AssertHasError(testInstruction1.UCRNumberInfo, errorMsg);

			testInstruction1.UCRNumber = "AAA01";
			AssertNoError(testInstruction1.UCRNumberInfo, errorMsg);
		}

		[TestDate(2020, 9, 15)]
		public void TestCheckCEI_DateForDuty()
		{
			var errorMsg = "Please enter a 'Declaration Date' that is today.";
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = testDeclaration.CusEntryInstruction;
			entryInstruction.Validation.ValidateCEI_DateForDuty();
			AssertNoMessageErrorContaining(entryInstruction.CEI_DateForDutyInfo, errorMsg);

			entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
			AssertHasMessageErrorContaining(entryInstruction.CEI_DateForDutyInfo, errorMsg);
			entryInstruction.CEI_DateForDuty = ZDateTime.Invalid;
			AssertHasMessageErrorContaining(entryInstruction.CEI_DateForDutyInfo, errorMsg);
			entryInstruction.CEI_DateForDuty = new ZDateTime(2020, 9, 14);
			AssertHasMessageErrorContaining(entryInstruction.CEI_DateForDutyInfo, errorMsg);
			entryInstruction.CEI_DateForDuty = new ZDateTime(2020, 9, 15);
			AssertNoMessageErrorContaining(entryInstruction.CEI_DateForDutyInfo, errorMsg);
			entryInstruction.CEI_DateForDuty = new ZDateTime(2020, 9, 16);
			AssertNoMessageErrorContaining(entryInstruction.CEI_DateForDutyInfo, errorMsg);

			var entryHeader = testDeclaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.CH_Status = ZString.Empty;
			entryHeader.CH_EntryStatus = ZString.Empty;
			entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
			AssertHasMessageErrorContaining(entryInstruction.CEI_DateForDutyInfo, errorMsg);
			entryInstruction.CEI_DateForDuty = ZDateTime.Invalid;
			AssertHasMessageErrorContaining(entryInstruction.CEI_DateForDutyInfo, errorMsg);
			entryInstruction.CEI_DateForDuty = new ZDateTime(2020, 9, 14);
			AssertHasMessageErrorContaining(entryInstruction.CEI_DateForDutyInfo, errorMsg);
			entryInstruction.CEI_DateForDuty = new ZDateTime(2020, 9, 15);
			AssertNoMessageErrorContaining(entryInstruction.CEI_DateForDutyInfo, errorMsg);
			entryInstruction.CEI_DateForDuty = new ZDateTime(2020, 9, 16);
			AssertNoMessageErrorContaining(entryInstruction.CEI_DateForDutyInfo, errorMsg);

			var waitingCodes = new ZString[]
			{
				JobDeclarationMessageStatusList.Codes.AWO,
				JobDeclarationMessageStatusList.Codes.AWC,
				JobDeclarationMessageStatusList.Codes.AWG,
				JobDeclarationMessageStatusList.Codes.AWE,
			};
			entryHeader.CH_EntryStatus = ZString.Empty;
			foreach (var code in waitingCodes)
			{
				entryHeader.CH_Status = code;
				entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
				AssertNoMessageErrorContaining(entryInstruction.CEI_DateForDutyInfo, errorMsg);
				entryInstruction.CEI_DateForDuty = ZDateTime.Invalid;
				AssertNoMessageErrorContaining(entryInstruction.CEI_DateForDutyInfo, errorMsg);
				entryInstruction.CEI_DateForDuty = new ZDateTime(2020, 9, 14);
				AssertNoMessageErrorContaining(entryInstruction.CEI_DateForDutyInfo, errorMsg);
				entryInstruction.CEI_DateForDuty = new ZDateTime(2020, 9, 15);
				AssertNoMessageErrorContaining(entryInstruction.CEI_DateForDutyInfo, errorMsg);
				entryInstruction.CEI_DateForDuty = new ZDateTime(2020, 9, 16);
				AssertNoMessageErrorContaining(entryInstruction.CEI_DateForDutyInfo, errorMsg);
			}

			entryHeader.CH_Status = ZString.Empty;
			entryHeader.CH_EntryStatus = EntryStatusCodeList.Codes.RFM;
			entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
			AssertNoMessageErrorContaining(entryInstruction.CEI_DateForDutyInfo, errorMsg);
			entryInstruction.CEI_DateForDuty = ZDateTime.Invalid;
			AssertNoMessageErrorContaining(entryInstruction.CEI_DateForDutyInfo, errorMsg);
			entryInstruction.CEI_DateForDuty = new ZDateTime(2020, 9, 14);
			AssertNoMessageErrorContaining(entryInstruction.CEI_DateForDutyInfo, errorMsg);
			entryInstruction.CEI_DateForDuty = new ZDateTime(2020, 9, 15);
			AssertNoMessageErrorContaining(entryInstruction.CEI_DateForDutyInfo, errorMsg);
			entryInstruction.CEI_DateForDuty = new ZDateTime(2020, 9, 16);
			AssertNoMessageErrorContaining(entryInstruction.CEI_DateForDutyInfo, errorMsg);

			entryHeader.CH_EntryStatus = EntryStatusCodeList.Codes.ARM;
			entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
			AssertNoMessageErrorContaining(entryInstruction.CEI_DateForDutyInfo, errorMsg);
			entryInstruction.CEI_DateForDuty = ZDateTime.Invalid;
			AssertNoMessageErrorContaining(entryInstruction.CEI_DateForDutyInfo, errorMsg);
			entryInstruction.CEI_DateForDuty = new ZDateTime(2020, 9, 14);
			AssertNoMessageErrorContaining(entryInstruction.CEI_DateForDutyInfo, errorMsg);
			entryInstruction.CEI_DateForDuty = new ZDateTime(2020, 9, 15);
			AssertNoMessageErrorContaining(entryInstruction.CEI_DateForDutyInfo, errorMsg);
			entryInstruction.CEI_DateForDuty = new ZDateTime(2020, 9, 16);
			AssertNoMessageErrorContaining(entryInstruction.CEI_DateForDutyInfo, errorMsg);

			var disposition = entryHeader.CusDispositions.AddNew();
			disposition.CDI_StatusKey = EntryStatusCodeList.Codes.ARM;
			disposition.CDI_Type = Common.CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			disposition.CDI_Status = "A01";
			entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
			AssertHasMessageErrorContaining(entryInstruction.CEI_DateForDutyInfo, errorMsg);
			entryInstruction.CEI_DateForDuty = ZDateTime.Invalid;
			AssertHasMessageErrorContaining(entryInstruction.CEI_DateForDutyInfo, errorMsg);
			entryInstruction.CEI_DateForDuty = new ZDateTime(2020, 9, 14);
			AssertHasMessageErrorContaining(entryInstruction.CEI_DateForDutyInfo, errorMsg);
			entryInstruction.CEI_DateForDuty = new ZDateTime(2020, 9, 15);
			AssertNoMessageErrorContaining(entryInstruction.CEI_DateForDutyInfo, errorMsg);
			entryInstruction.CEI_DateForDuty = new ZDateTime(2020, 9, 16);
			AssertNoMessageErrorContaining(entryInstruction.CEI_DateForDutyInfo, errorMsg);
		}

		[TestDate(2020, 3, 30)]
		public void TestCheckCEI_CustomsOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeList("TW", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BT", "Nanjing Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var jobDeclaration = Factory.New<JobDeclaration>();
			var orgHerder = new TestTWCreator(Factory).CreateOrganizationForPowerOfAttorneyDocument();
			jobDeclaration.JE_MessageType = "EXP";
			jobDeclaration.JE_OH_Supplier = orgHerder.PK;
			jobDeclaration.JE_OH_Importer = orgHerder.PK;
			var cusEntryInstruction = jobDeclaration.CusEntryInstruction;
			jobDeclaration.JE_MessageType = "EXP";
			cusEntryInstruction.CEI_CustomsOffice = "BJ";
			AssertHasMessageErrorContaining(cusEntryInstruction.CEI_CustomsOfficeInfo, ListValidation.InvalidCodeMessageError);
			cusEntryInstruction.CEI_CustomsOffice = "BT";
			AssertNoMessageErrorContaining(cusEntryInstruction.CEI_CustomsOfficeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoWarnings(jobDeclaration.JE_OH_SupplierInfo);
			cusEntryInstruction.CEI_CustomsOffice = ZString.Empty;
			AssertHasMessageErrorContaining(cusEntryInstruction.CEI_CustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasWarningContaining(jobDeclaration.JE_OH_SupplierInfo, "There is a Power of Attorney Document on the organization (eDocs > Document Tracking), but it is not valid for TW and/or this direction.");
			jobDeclaration.JE_MessageType = "IMP";
			cusEntryInstruction.CEI_CustomsOffice = "BJ";
			AssertNoWarnings(jobDeclaration.JE_OH_ImporterInfo);
			cusEntryInstruction.CEI_CustomsOffice = ZString.Empty;
			AssertHasWarningContaining(jobDeclaration.JE_OH_ImporterInfo, "There is a Power of Attorney Document on the organization (eDocs > Document Tracking), but it is not valid for TW and/or this direction.");
		}

		public void TestCheckCEI_GoodsLocation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var codeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "AO", "Air CustomsOffice", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTransportModeForCusCodeList(codeList.PK, TransportTypeList.Codes.Air);
			codeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "SO", "Sea CustomsOffice", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTransportModeForCusCodeList(codeList.PK, TransportTypeList.Codes.Sea);
			Factory.Save();
			var customsOffice = Factory.New<ZZRefCusCodeListCombined>();
			customsOffice.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Taiwan;
			customsOffice.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
			customsOffice.ZZD_Code = "BT";
			customsOffice.ZZD_StartDate = ZDateTime.Today;
			customsOffice.ZZD_EndDate = ZDateTime.Today.AddYears(1);
			var facility1 = Factory.New<ZZRefCusCodeListCombined>();
			facility1.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Taiwan;
			facility1.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities;
			facility1.ZZD_Code = "ANP0060N";
			facility1.ZZD_StartDate = ZDateTime.Today;
			facility1.ZZD_EndDate = ZDateTime.Today.AddYears(1);
			facility1.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.CustomsOffice, "BT");
			var facility2 = Factory.New<ZZRefCusCodeListCombined>();
			facility2.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Taiwan;
			facility2.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities;
			facility2.ZZD_Code = "BNP0060N";
			facility2.ZZD_StartDate = ZDateTime.Today;
			facility2.ZZD_EndDate = ZDateTime.Today.AddYears(1);
			facility2.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.CustomsOffice, "AT");
			Factory.Save();
			var jobDeclaration = Factory.New<JobDeclaration>();
			var cusEntryInstruction = jobDeclaration.CusEntryInstruction;
			jobDeclaration.JE_MessageType = "MSC";
			cusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Export.F5;
			cusEntryInstruction.CEI_CustomsOffice = "AT";
			cusEntryInstruction.CEI_GoodsLocation = "ANP0060N";
			AssertNoMessageErrorContaining(cusEntryInstruction.CEI_GoodsLocationInfo, ListValidation.InvalidCodeMessageError);
			cusEntryInstruction.CEI_CustomsOffice = "AT";
			cusEntryInstruction.CEI_GoodsLocation = "BNP0060N";
			AssertNoMessageErrorContaining(cusEntryInstruction.CEI_GoodsLocationInfo, ListValidation.InvalidCodeMessageError);
			cusEntryInstruction.CEI_CustomsOffice = "BT";
			cusEntryInstruction.CEI_GoodsLocation = "BNP0060N";
			AssertHasWarningContaining(cusEntryInstruction.CEI_GoodsLocationInfo, ValidationConstants.Declaration.ExportLocationDoesNotBelongToCustomsOffice(cusEntryInstruction.CEI_GoodsLocation, cusEntryInstruction.CEI_CustomsOffice));
			cusEntryInstruction.CEI_GoodsLocation = "####";
			AssertHasMessageErrorContaining(cusEntryInstruction.CEI_GoodsLocationInfo, ListValidation.InvalidCodeMessageError);
			jobDeclaration.JE_MessageType = "IMP";
			cusEntryInstruction.CEI_CustomsOffice = "BT";
			cusEntryInstruction.CEI_GoodsLocation = "BNP0060N";
			AssertHasWarningContaining(cusEntryInstruction.CEI_GoodsLocationInfo, ValidationConstants.Declaration.ReceiptLocationDoesNotBelongToCustomsOffice(cusEntryInstruction.CEI_GoodsLocation, cusEntryInstruction.CEI_CustomsOffice));
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "MSC";
			var entryInstruction = declaration.CusEntryInstruction;
			declaration.JE_CustomsOffice = "BA";
			entryInstruction.CEI_CustomsOffice = "BA";
			entryInstruction.CEI_GoodsLocation = ZString.Empty;
			AssertNoMessageErrorContaining(entryInstruction.CEI_GoodsLocationInfo, MandatoryValidation.YouHaveNotEntered);
			entryInstruction.CEI_CustomsOffice = "AS";
			entryInstruction.CEI_GoodsLocation = ZString.Empty;
			AssertHasMessageErrorContaining(entryInstruction.CEI_GoodsLocationInfo, MandatoryValidation.YouHaveNotEntered);
			entryInstruction.CEI_GoodsLocation = "AA";
			AssertNoMessageErrorContaining(entryInstruction.CEI_GoodsLocationInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_CustomsOffice = "AS";
			entryInstruction.CEI_GoodsLocation = ZString.Empty;
			AssertNoMessageErrorContaining(entryInstruction.CEI_GoodsLocationInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_MessageType = "EXP";
			declaration.JE_CustomsOffice = "AS";
			entryInstruction.CEI_GoodsLocation = ZString.Empty;
			AssertHasMessageErrorContaining(entryInstruction.CEI_GoodsLocationInfo, MandatoryValidation.YouHaveNotEntered);
			entryInstruction.CEI_GoodsLocation = "555";
			AssertNoMessageErrorContaining(entryInstruction.CEI_GoodsLocationInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_MessageType = "IMP";
			declaration.JE_CustomsOffice = "AS";
			entryInstruction.CEI_GoodsLocation = ZString.Empty;
			AssertHasMessageErrorContaining(entryInstruction.CEI_GoodsLocationInfo, MandatoryValidation.YouHaveNotEntered);
			entryInstruction.CEI_GoodsLocation = "555";
			AssertNoMessageErrorContaining(entryInstruction.CEI_GoodsLocationInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCEI_ExamMode()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			jobDeclaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			jobDeclaration.JE_MessageType = "IMP";
			var entryInstruction = jobDeclaration.CusEntryInstruction;
			entryInstruction.CEI_Style = "D1";
			var invoice = jobDeclaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			entryInstruction.CEI_ExamMode = "X";
			AssertHasMessageErrorContaining(entryInstruction.CEI_ExamModeInfo, ListValidation.InvalidCodeMessageError);
			entryInstruction.CEI_ExamMode = ExamModeList.Codes.FactoryInspection;
			AssertNoMessageErrorContaining(entryInstruction.CEI_ExamModeInfo, ListValidation.InvalidCodeMessageError);
			var longerThan512Message = "Examination Mode must be 8 when the declaration has an invoice line where the goods description is longer than 512 bytes.";
			var applicableForDutyRefundsMessage = "Exam. Mode should be 8 when re-exporting unused foreign goods that are applicable for duty refunds.";
			invoiceLine2.JI_DeclarationGoodsDescription = ZString.Replicate('z', 512);
			entryInstruction.Validation.ValidateCEI_ExamMode();
			AssertNoWarning(entryInstruction.CEI_ExamModeInfo, longerThan512Message);
			invoiceLine2.JI_DeclarationGoodsDescription = ZString.Replicate('z', 513);
			entryInstruction.Validation.ValidateCEI_ExamMode();
			AssertHasWarning(entryInstruction.CEI_ExamModeInfo, longerThan512Message);
			invoiceLine1.JI_Procedure = TW.Business.Constants.ProcedureCodes._92;
			entryInstruction.Validation.ValidateCEI_ExamMode();
			AssertNoWarning(entryInstruction.CEI_ExamModeInfo, applicableForDutyRefundsMessage);
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.G3;
			entryInstruction.Validation.ValidateCEI_ExamMode();
			AssertHasWarning(entryInstruction.CEI_ExamModeInfo, applicableForDutyRefundsMessage);
			invoiceLine1.JI_Procedure = TW.Business.Constants.ProcedureCodes._31;
			entryInstruction.Validation.ValidateCEI_ExamMode();
			AssertNoWarning(entryInstruction.CEI_ExamModeInfo, applicableForDutyRefundsMessage);
			entryInstruction.CEI_ExamMode = ExamModeList.Codes.WrittenReview;
			entryInstruction.Validation.ValidateCEI_ExamMode();
			AssertNoWarning(entryInstruction.CEI_ExamModeInfo, longerThan512Message);
			AssertNoWarning(entryInstruction.CEI_ExamModeInfo, applicableForDutyRefundsMessage);
			var tariffquotaMessage = "Examination Mode cannot be '9' because the declaration job contains an invoice line applying for a tariff quota.";
			entryInstruction.CEI_ExamMode = ExamModeList.Codes.ShallBe;
			entryInstruction.Validation.ValidateCEI_ExamMode();
			AssertNoMessageErrorContaining(entryInstruction.CEI_ExamModeInfo, tariffquotaMessage);
			invoiceLine1.JI_Tariff = "98";
			entryInstruction.Validation.ValidateCEI_ExamMode();
			AssertHasMessageErrorContaining(entryInstruction.CEI_ExamModeInfo, tariffquotaMessage);
			invoiceLine1.JI_Tariff = "97";
			invoiceLine1.JI_ConcessionOrder = Constants.ConcessionOrder.Quota;
			entryInstruction.Validation.ValidateCEI_ExamMode();
			AssertHasMessageErrorContaining(entryInstruction.CEI_ExamModeInfo, tariffquotaMessage);
			invoiceLine1.JI_ConcessionOrder = ZString.Empty;
			entryInstruction.Validation.ValidateCEI_ExamMode();
			AssertNoMessageErrorContaining(entryInstruction.CEI_ExamModeInfo, tariffquotaMessage);
		}

		public void TestCheckCEI_ExamModeWhenImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var charge = entryHeader.DutyTaxFeeCharges.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();

			CombineAssertions("A20, A30, A40, A50", () =>
			{
				var message = "Exam. Mode might need to be set to '8' when the entry is subject to Anti-Dumping Duty, Countervailing Duty, Additional Duty, or Retaliatory Duty.";
				charge.ChargeType = DutyTaxFeeCodeList.Codes.A20;
				entryInstruction.CEI_ExamMode = ExamModeList.Codes.ShallBe;
				AssertHasWarning(entryInstruction.CEI_ExamModeInfo, message);

				entryInstruction.CEI_ExamMode = ExamModeList.Codes.WrittenReview;
				AssertNoWarning(entryInstruction.CEI_ExamModeInfo, message);

				charge.ChargeType = DutyTaxFeeCodeList.Codes.A30;
				entryInstruction.CEI_ExamMode = ExamModeList.Codes.ShallBe;
				AssertHasWarning(entryInstruction.CEI_ExamModeInfo, message);

				entryInstruction.CEI_ExamMode = ExamModeList.Codes.WrittenReview;
				AssertNoWarning(entryInstruction.CEI_ExamModeInfo, message);

				charge.ChargeType = DutyTaxFeeCodeList.Codes.A40;
				entryInstruction.CEI_ExamMode = ExamModeList.Codes.ShallBe;
				AssertHasWarning(entryInstruction.CEI_ExamModeInfo, message);

				entryInstruction.CEI_ExamMode = ExamModeList.Codes.WrittenReview;
				AssertNoWarning(entryInstruction.CEI_ExamModeInfo, message);

				charge.ChargeType = DutyTaxFeeCodeList.Codes.A50;
				entryInstruction.CEI_ExamMode = ExamModeList.Codes.ShallBe;
				AssertHasWarning(entryInstruction.CEI_ExamModeInfo, message);

				entryInstruction.CEI_ExamMode = ExamModeList.Codes.WrittenReview;
				AssertNoWarning(entryInstruction.CEI_ExamModeInfo, message);

				charge.ChargeType = DutyTaxFeeCodeList.Codes.A19;
				entryInstruction.CEI_ExamMode = ExamModeList.Codes.ShallBe;
				AssertNoWarning(entryInstruction.CEI_ExamModeInfo, message);

				entryInstruction.CEI_ExamMode = ExamModeList.Codes.WrittenReview;
				AssertNoWarning(entryInstruction.CEI_ExamModeInfo, message);

				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				charge.ChargeType = DutyTaxFeeCodeList.Codes.A50;
				entryInstruction.CEI_ExamMode = ExamModeList.Codes.ShallBe;
				AssertNoWarning(entryInstruction.CEI_ExamModeInfo, message);
			});

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			CombineAssertions("B10, B19 (Exam Mode is 8)", () =>
			{
				var message = "Exam. Mode must be '8' or 'A' when at least one of the entry lines is subject to Commodity Tax.";
				charge.ChargeType = DutyTaxFeeCodeList.Codes.B10;
				entryInstruction.CEI_ExamMode = ExamModeList.Codes.ShallBe;
				AssertHasMessageError(entryInstruction.CEI_ExamModeInfo, message);

				entryInstruction.CEI_ExamMode = ExamModeList.Codes.WrittenReview;
				AssertNoMessageError(entryInstruction.CEI_ExamModeInfo, message);

				charge.ChargeType = DutyTaxFeeCodeList.Codes.B19;
				entryInstruction.CEI_ExamMode = ExamModeList.Codes.ShallBe;
				AssertHasMessageError(entryInstruction.CEI_ExamModeInfo, message);

				entryInstruction.CEI_ExamMode = ExamModeList.Codes.WrittenReview;
				AssertNoMessageError(entryInstruction.CEI_ExamModeInfo, message);

				charge.ChargeType = DutyTaxFeeCodeList.Codes.A40;
				entryInstruction.CEI_ExamMode = ExamModeList.Codes.ShallBe;
				AssertNoMessageError(entryInstruction.CEI_ExamModeInfo, message);

				entryInstruction.CEI_ExamMode = ExamModeList.Codes.WrittenReview;
				AssertNoMessageError(entryInstruction.CEI_ExamModeInfo, message);

				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				charge.ChargeType = DutyTaxFeeCodeList.Codes.B19;
				entryInstruction.CEI_ExamMode = ExamModeList.Codes.ShallBe;
				AssertNoMessageError(entryInstruction.CEI_ExamModeInfo, message);
			});

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			CombineAssertions("B10, B19 (Exam Mode is A)", () =>
			{
				var message = "Exam. Mode must be '8' or 'A' when at least one of the entry lines is subject to Commodity Tax.";
				charge.ChargeType = DutyTaxFeeCodeList.Codes.B10;
				entryInstruction.CEI_ExamMode = ExamModeList.Codes.ShallBe;
				AssertHasMessageError(entryInstruction.CEI_ExamModeInfo, message);

				entryInstruction.CEI_ExamMode = ExamModeList.Codes.CommodityTax;
				AssertNoMessageError(entryInstruction.CEI_ExamModeInfo, message);

				charge.ChargeType = DutyTaxFeeCodeList.Codes.B19;
				entryInstruction.CEI_ExamMode = ExamModeList.Codes.ShallBe;
				AssertHasMessageError(entryInstruction.CEI_ExamModeInfo, message);

				entryInstruction.CEI_ExamMode = ExamModeList.Codes.CommodityTax;
				AssertNoMessageError(entryInstruction.CEI_ExamModeInfo, message);

				charge.ChargeType = DutyTaxFeeCodeList.Codes.A40;
				entryInstruction.CEI_ExamMode = ExamModeList.Codes.ShallBe;
				AssertNoMessageError(entryInstruction.CEI_ExamModeInfo, message);

				entryInstruction.CEI_ExamMode = ExamModeList.Codes.CommodityTax;
				AssertNoMessageError(entryInstruction.CEI_ExamModeInfo, message);

				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				charge.ChargeType = DutyTaxFeeCodeList.Codes.B19;
				entryInstruction.CEI_ExamMode = ExamModeList.Codes.ShallBe;
				AssertNoMessageError(entryInstruction.CEI_ExamModeInfo, message);
			});

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			CombineAssertions("B31, B69", () =>
			{
				var message = "Exam. Mode must be '8' when at least one of the entry lines is subject to Tobacco and Alcohol Tax.";
				charge.ChargeType = DutyTaxFeeCodeList.Codes.B31;
				entryInstruction.CEI_ExamMode = ExamModeList.Codes.ShallBe;
				AssertHasMessageError(entryInstruction.CEI_ExamModeInfo, message);

				entryInstruction.CEI_ExamMode = ExamModeList.Codes.WrittenReview;
				AssertNoMessageError(entryInstruction.CEI_ExamModeInfo, message);

				charge.ChargeType = DutyTaxFeeCodeList.Codes.B69;
				entryInstruction.CEI_ExamMode = ExamModeList.Codes.ShallBe;
				AssertHasMessageError(entryInstruction.CEI_ExamModeInfo, message);

				entryInstruction.CEI_ExamMode = ExamModeList.Codes.WrittenReview;
				AssertNoMessageError(entryInstruction.CEI_ExamModeInfo, message);

				charge.ChargeType = DutyTaxFeeCodeList.Codes.A40;
				entryInstruction.CEI_ExamMode = ExamModeList.Codes.ShallBe;
				AssertNoMessageError(entryInstruction.CEI_ExamModeInfo, message);

				entryInstruction.CEI_ExamMode = ExamModeList.Codes.WrittenReview;
				AssertNoMessageError(entryInstruction.CEI_ExamModeInfo, message);

				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				charge.ChargeType = DutyTaxFeeCodeList.Codes.B69;
				entryInstruction.CEI_ExamMode = ExamModeList.Codes.ShallBe;
				AssertNoMessageError(entryInstruction.CEI_ExamModeInfo, message);
			});

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			CombineAssertions("B60, B89", () =>
			{
				var message = "Exam. Mode must be '8' when at least one of the entry lines is subject to Specifically Selected Goods and Services Tax.";
				charge.ChargeType = DutyTaxFeeCodeList.Codes.B60;
				entryInstruction.CEI_ExamMode = ExamModeList.Codes.ShallBe;
				AssertHasMessageError(entryInstruction.CEI_ExamModeInfo, message);

				entryInstruction.CEI_ExamMode = ExamModeList.Codes.WrittenReview;
				AssertNoMessageError(entryInstruction.CEI_ExamModeInfo, message);

				charge.ChargeType = DutyTaxFeeCodeList.Codes.B89;
				entryInstruction.CEI_ExamMode = ExamModeList.Codes.ShallBe;
				AssertHasMessageError(entryInstruction.CEI_ExamModeInfo, message);

				entryInstruction.CEI_ExamMode = ExamModeList.Codes.WrittenReview;
				AssertNoMessageError(entryInstruction.CEI_ExamModeInfo, message);

				charge.ChargeType = DutyTaxFeeCodeList.Codes.A40;
				entryInstruction.CEI_ExamMode = ExamModeList.Codes.ShallBe;
				AssertNoMessageError(entryInstruction.CEI_ExamModeInfo, message);

				entryInstruction.CEI_ExamMode = ExamModeList.Codes.WrittenReview;
				AssertNoMessageError(entryInstruction.CEI_ExamModeInfo, message);

				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				charge.ChargeType = DutyTaxFeeCodeList.Codes.B89;
				entryInstruction.CEI_ExamMode = ExamModeList.Codes.ShallBe;
				AssertNoMessageError(entryInstruction.CEI_ExamModeInfo, message);
			});

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			CombineAssertions("Tariff code starts with 98", () =>
			{
				var message = "Exam. Mode might need to be set to '8' when the entry is subject to Quota Duty.";
				invoiceLine2.JI_Tariff = "980001";
				entryInstruction.CEI_ExamMode = ExamModeList.Codes.ShallBe;
				AssertHasWarning(entryInstruction.CEI_ExamModeInfo, message);

				entryInstruction.CEI_ExamMode = ExamModeList.Codes.WrittenReview;
				AssertNoWarning(entryInstruction.CEI_ExamModeInfo, message);

				invoiceLine2.JI_Tariff = "970001";
				entryInstruction.CEI_ExamMode = ExamModeList.Codes.ShallBe;
				AssertNoWarning(entryInstruction.CEI_ExamModeInfo, message);

				entryInstruction.CEI_ExamMode = ExamModeList.Codes.WrittenReview;
				AssertNoWarning(entryInstruction.CEI_ExamModeInfo, message);

				invoiceLine2.JI_Tariff = "980001";
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				entryInstruction.CEI_ExamMode = ExamModeList.Codes.ShallBe;
				AssertNoWarning(entryInstruction.CEI_ExamModeInfo, message);
			});

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			CombineAssertions("SHTC Import Permit is empty", () =>
			{
				var message = "Exam. Mode must be '8' when at least one of the Invoice Line is subject to SHTC import goods.";
				invoiceLine1.HighTechLicense = "XX1";
				entryInstruction.CEI_ExamMode = ExamModeList.Codes.ShallBe;
				AssertHasMessageError(entryInstruction.CEI_ExamModeInfo, message);

				entryInstruction.CEI_ExamMode = ExamModeList.Codes.WrittenReview;
				AssertNoMessageError(entryInstruction.CEI_ExamModeInfo, message);

				invoiceLine1.HighTechLicense = ZString.Empty;
				entryInstruction.CEI_ExamMode = ExamModeList.Codes.ShallBe;
				AssertNoMessageError(entryInstruction.CEI_ExamModeInfo, message);

				entryInstruction.CEI_ExamMode = ExamModeList.Codes.WrittenReview;
				AssertNoMessageError(entryInstruction.CEI_ExamModeInfo, message);

				invoiceLine1.HighTechLicense = "XX1";
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				entryInstruction.CEI_ExamMode = ExamModeList.Codes.ShallBe;
				AssertNoMessageError(entryInstruction.CEI_ExamModeInfo, message);
			});
		}

		public void TestCheckCEI_ReasonForDuty()
		{
			var cusEntryInstruction = Factory.New<CusEntryInstruction>();
			cusEntryInstruction.CEI_ReasonForDuty = "X";
			AssertHasMessageErrorContaining(cusEntryInstruction.CEI_ReasonForDutyInfo, ListValidation.InvalidCodeMessageError);
			cusEntryInstruction.CEI_ReasonForDuty = ReasonforDutyList.Codes.FinishedProductDomesticSales;
			AssertNoMessageErrorContaining(cusEntryInstruction.CEI_ReasonForDutyInfo, ListValidation.InvalidCodeMessageError);
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_CEI = entryInstruction.PK;
			var info = entryInstruction.CEI_ReasonForDutyInfo;
			var messageErrorAllInvoiceLineDutyTreatmentMustBe35ForShortInventory = ValidationConstants.EntryInstruction.AllInvoiceLineDutyTreatmentMustBe35ForShortInventory;
			entryInstruction.CEI_ReasonForDuty = ReasonforDutyList.Codes.PoorDiscSupplementary;
			invoiceLine1.JI_Procedure = TW.Business.Constants.ProcedureCodes._35;
			invoiceLine2.JI_Procedure = "11";
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G2;
			AssertHasMessageErrorContaining(info, messageErrorAllInvoiceLineDutyTreatmentMustBe35ForShortInventory);
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G2;
			entryInstruction.CEI_ReasonForDuty = ReasonforDutyList.Codes.PoorDiscSupplementary;
			invoiceLine1.JI_Procedure = TW.Business.Constants.ProcedureCodes._35;
			invoiceLine2.JI_Procedure = TW.Business.Constants.ProcedureCodes._35;
			AssertNoMessageErrorContaining(info, messageErrorAllInvoiceLineDutyTreatmentMustBe35ForShortInventory);
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G2;
			entryInstruction.CEI_ReasonForDuty = ReasonforDutyList.Codes.PoorDiscSupplementary;
			invoiceLine1.JI_Procedure = "13";
			invoiceLine2.JI_Procedure = "11";
			AssertNoMessageErrorContaining(info, messageErrorAllInvoiceLineDutyTreatmentMustBe35ForShortInventory);
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G2;
			entryInstruction.CEI_ReasonForDuty = ReasonforDutyList.Codes.FinishedProductDomesticSales;
			invoiceLine1.JI_Procedure = TW.Business.Constants.ProcedureCodes._35;
			invoiceLine2.JI_Procedure = "11";
			AssertNoMessageErrorContaining(info, messageErrorAllInvoiceLineDutyTreatmentMustBe35ForShortInventory);
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D7;
			entryInstruction.CEI_ReasonForDuty = ReasonforDutyList.Codes.PoorDiscSupplementary;
			invoiceLine1.JI_Procedure = TW.Business.Constants.ProcedureCodes._35;
			invoiceLine2.JI_Procedure = "11";
			AssertNoMessageErrorContaining(info, messageErrorAllInvoiceLineDutyTreatmentMustBe35ForShortInventory);
			var messageErrorReasonForDutyMustBe01or13or99 = ValidationConstants.EntryInstruction.ReasonForDutyMustBe01or13or99;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			invoiceLine1.JI_Procedure = TW.Business.Constants.ProcedureCodes._31;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G2;
			AssertHasMessageErrorContaining(info, messageErrorReasonForDutyMustBe01or13or99);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G2;
			invoiceLine1.JI_Procedure = TW.Business.Constants.ProcedureCodes._35;
			AssertHasMessageErrorContaining(info, messageErrorReasonForDutyMustBe01or13or99);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G2;
			invoiceLine1.JI_Procedure = TW.Business.Constants.ProcedureCodes._50;
			AssertHasMessageErrorContaining(info, messageErrorReasonForDutyMustBe01or13or99);
			entryInstruction.CEI_ReasonForDuty = ReasonforDutyList.Codes.FinishedProductDomesticSales;
			AssertNoMessageErrorContaining(info, messageErrorReasonForDutyMustBe01or13or99);
			entryInstruction.CEI_ReasonForDuty = ReasonforDutyList.Codes.PortEnterpriseSupplementary;
			AssertNoMessageErrorContaining(info, messageErrorReasonForDutyMustBe01or13or99);
			entryInstruction.CEI_ReasonForDuty = ReasonforDutyList.Codes.Other;
			AssertNoMessageErrorContaining(info, messageErrorReasonForDutyMustBe01or13or99);
			entryInstruction.CEI_ReasonForDuty = ReasonforDutyList.Codes.GiftSupplementary;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertNoMessageErrorContaining(info, messageErrorReasonForDutyMustBe01or13or99);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D7;
			AssertNoMessageErrorContaining(info, messageErrorReasonForDutyMustBe01or13or99);
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G2;
			invoiceLine1.JI_Procedure = "44";
			AssertNoMessageErrorContaining(info, messageErrorReasonForDutyMustBe01or13or99);
			var messageErrorReasonForDutyMustBe04or05or06or07or13 = ValidationConstants.EntryInstruction.ReasonForDutyMustBe04or05or06or07or13;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.F2;
			invoiceLine1.JI_Procedure = TW.Business.Constants.ProcedureCodes._35;
			AssertHasMessageErrorContaining(info, messageErrorReasonForDutyMustBe04or05or06or07or13);
			entryInstruction.CEI_ReasonForDuty = ReasonforDutyList.Codes.SupplementaryEquipment;
			AssertNoMessageErrorContaining(info, messageErrorReasonForDutyMustBe04or05or06or07or13);
			entryInstruction.CEI_ReasonForDuty = ReasonforDutyList.Codes.PoorDiscSupplementary;
			AssertNoMessageErrorContaining(info, messageErrorReasonForDutyMustBe04or05or06or07or13);
			entryInstruction.CEI_ReasonForDuty = ReasonforDutyList.Codes.StolenFinishedGoodsSupplementary;
			AssertNoMessageErrorContaining(info, messageErrorReasonForDutyMustBe04or05or06or07or13);
			entryInstruction.CEI_ReasonForDuty = ReasonforDutyList.Codes.StolenEquipmentSupplementary;
			AssertNoMessageErrorContaining(info, messageErrorReasonForDutyMustBe04or05or06or07or13);
			entryInstruction.CEI_ReasonForDuty = ReasonforDutyList.Codes.PortEnterpriseSupplementary;
			AssertNoMessageErrorContaining(info, messageErrorReasonForDutyMustBe04or05or06or07or13);
			entryInstruction.CEI_ReasonForDuty = ReasonforDutyList.Codes.GiftSupplementary;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D7;
			AssertNoMessageErrorContaining(info, messageErrorReasonForDutyMustBe04or05or06or07or13);
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.F2;
			invoiceLine1.JI_Procedure = "11";
			AssertNoMessageErrorContaining(info, messageErrorReasonForDutyMustBe04or05or06or07or13);
		}

		public void TestCheckCEI_WHSMonth()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_WHSMonth = "1";
			AssertNoMessageError(entryInstruction.CEI_WHSMonthInfo, ValidationConstants.EntryInstruction.WHSMonthAllowedValue);
			entryInstruction.CEI_WHSMonth = "12";
			AssertNoMessageError(entryInstruction.CEI_WHSMonthInfo, ValidationConstants.EntryInstruction.WHSMonthAllowedValue);
			entryInstruction.CEI_WHSMonth = "-1";
			AssertHasMessageError(entryInstruction.CEI_WHSMonthInfo, ValidationConstants.EntryInstruction.WHSMonthAllowedValue);
			entryInstruction.CEI_WHSMonth = "13";
			AssertHasMessageError(entryInstruction.CEI_WHSMonthInfo, ValidationConstants.EntryInstruction.WHSMonthAllowedValue);
		}

		public void TestCEI_DaysOfDelayedDeclaration()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			jobDeclaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			jobDeclaration.JE_MessageType = "IMP";
			var entryInstruction = jobDeclaration.CusEntryInstruction;
			entryInstruction.CEI_Style = "D1";
			var invoice = jobDeclaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			jobDeclaration.DoMerge();
			entryInstruction.CEI_DaysOfDelayedDeclaration = -1;
			AssertHasMessageErrorContaining(entryInstruction.CEI_DaysOfDelayedDeclarationInfo, ValidationConstants.EntryInstruction.DaysOfDelayedDeclarationAllowedValue);
			jobDeclaration.JE_MessageType = "EXP";
			AssertNoMessageErrorContaining(entryInstruction.CEI_DaysOfDelayedDeclarationInfo, ValidationConstants.EntryInstruction.DaysOfDelayedDeclarationAllowedValue);
			jobDeclaration.JE_MessageType = "IMP";
			entryInstruction.CEI_DaysOfDelayedDeclaration = 0;
			AssertNoMessageErrorContaining(entryInstruction.CEI_DaysOfDelayedDeclarationInfo, ValidationConstants.EntryInstruction.DaysOfDelayedDeclarationAllowedValue);
			entryInstruction.CEI_DaysOfDelayedDeclaration = 20;
			AssertNoMessageErrorContaining(entryInstruction.CEI_DaysOfDelayedDeclarationInfo, ValidationConstants.EntryInstruction.DaysOfDelayedDeclarationAllowedValue);
			entryInstruction.CEI_DaysOfDelayedDeclaration = 21;
			AssertHasMessageErrorContaining(entryInstruction.CEI_DaysOfDelayedDeclarationInfo, ValidationConstants.EntryInstruction.DaysOfDelayedDeclarationAllowedValue);
			entryInstruction.CEI_Style = "D2";
			AssertNoMessageErrorContaining(entryInstruction.CEI_DaysOfDelayedDeclarationInfo, ValidationConstants.EntryInstruction.DaysOfDelayedDeclarationAllowedValue);
		}

		public void TestCheckCEI_DutyRefund()
		{
			var declaration = Factory.New<JobDeclaration>();
			var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.CBPCodeType = OrgCusCode.TaiwanCodeTypes.FTZ;
			supplierDocumentaryAddress.CBPCode = "1111";
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.F4;
			entryInstruction.CEI_DutyRefund = true;
			AssertHasMessageErrorContaining(entryInstruction.CEI_DutyRefundInfo, "When Goods is imported from Free Trade Zone, Request Duty Refund shouldn't be ticked.");
			entryInstruction.CEI_DutyRefund = false;
			AssertNoMessageErrorContaining(entryInstruction.CEI_DutyRefundInfo, "When Goods is imported from Free Trade Zone, Request Duty Refund shouldn't be ticked.");
			AssertNoMessageErrorContaining(entryInstruction.CEI_DutyRefundInfo, "When Goods is Not imported from Free Trade Zone, Request Duty Refund should be ticked.");

			supplierDocumentaryAddress.CBPCodeType = OrgCusCode.TaiwanCodeTypes.CBF;
			entryInstruction.Validation.ValidateCEI_DutyRefund();
			AssertHasMessageErrorContaining(entryInstruction.CEI_DutyRefundInfo, "When Goods is Not imported from Free Trade Zone, Request Duty Refund should be ticked.");
		}

		public void TestCheckBI_PackagingDescription()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var entryInstruction = jobDeclaration.CusEntryInstruction;
			entryInstruction.CEI_IsCoPackaged = false;
			entryInstruction.CEI_PackageDescription = ZString.Empty;
			entryInstruction.Validation.ValidateAll();
			AssertNoMessageErrorContaining(entryInstruction.CEI_PackageDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			entryInstruction.CEI_PackageDescription = "XXX";
			AssertNoMessageErrorContaining(entryInstruction.CEI_PackageDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			entryInstruction.CEI_IsCoPackaged = true;
			entryInstruction.CEI_PackageDescription = ZString.Empty;
			AssertHasMessageErrorContaining(entryInstruction.CEI_PackageDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			entryInstruction.CEI_PackageDescription = "XXX";
			AssertNoMessageErrorContaining(entryInstruction.CEI_PackageDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCEI_BoxNumber()
		{
			new TestTWCreator(Factory).CreateRegistryItemCusBrokerageBoxNumber();
			var jobDeclaration = Factory.New<JobDeclaration>();
			var entryInstruction = jobDeclaration.CusEntryInstruction;
			var targetInfo = entryInstruction.CEI_BoxNumberInfo;
			entryInstruction.CEI_CustomsOffice = "BF";
			entryInstruction.CEI_BoxNumber = ZString.Empty;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			entryInstruction.CEI_BoxNumber = "XXX";
			AssertHasMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
			entryInstruction.CEI_BoxNumber = "100";
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
		}
	}
}

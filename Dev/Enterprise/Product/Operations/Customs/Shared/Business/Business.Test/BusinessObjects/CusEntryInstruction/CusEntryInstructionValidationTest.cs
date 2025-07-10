using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusEntryInstructionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCEI_Style()
		{
			// Create a row in ZZ6 so that the lookups has a value

			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			universalHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Eritrea, "CAT", "11", "", "", "Eleven", JobMessageTypeList.Codes.Import, "");
			Factory.Save();

			var testDeclaration = Factory.NewWithValidTestData<DeclarationForEritreaWithCEISupport>();
			testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var testInstruction = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			CombineAssertions(() =>
			{
				testInstruction.CEI_Style = "12";
				AssertHasMessageError("Invalid Style", testInstruction.CEI_StyleInfo, ListValidation.InvalidCodeMessageError);
				testInstruction.CEI_Style = "11";
				AssertNoMessageError("Valid Style", testInstruction.CEI_StyleInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckCEI_OA_Warehouse2_IntoInwardProcessing()
		{
			var messageError = "When Job is Into Inward Processing, the To Warehouse must have an Inward Processing Location.";

			var whsHelper = new WhsDataTestHelper(Factory);
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "BOB1";
			importer.OH_FullName = "BOB THE BUILDER";
			importer.CompanyData.OB_CusInventoryForInwardProcessing = true;
			importer.OH_IsWarehouseClient = true;

			((OrgHeader)whsHelper.WhsWarehouse2.WarehouseAddress.Header).CompanyData.OB_CusInventoryForInwardProcessing = true;

			whsHelper.UniversalTariffHelper.CreateNewOrGetExistingDataGrouping("ER");
			var intoInwardProcessing = whsHelper.UniversalTariffHelper.CreateRefCusProcedure("ER", "", "AB", "20", "", "AB DESC", "", group: "JC");
			intoInwardProcessing.ZZ6_IntoInwardProcessing = "Y";
			Factory.Save();

			TestWarehouseWithGoodBoolean(importer, intoInwardProcessing, out var entryInstruction, true, "ENT");

			var declaration = entryInstruction.JobDeclaration;
			var invoiceLine = declaration.InvoiceLines[0];

			entryInstruction.Validation.ValidateCEI_OA_Warehouse2();
			Assert("Precondition IsWarehouse2RequiredForWarehouseValidation", entryInstruction.IsWarehouse2RequiredForWarehouseValidation);
			Assert("Precondition IsIntoInwardProcessing", invoiceLine.IsIntoInwardProcessing);
			AssertNoMessageError("No warehouse, so there should be no error.", entryInstruction.CEI_OA_Warehouse2Info, messageError);

			entryInstruction.CEI_OA_Warehouse2 = whsHelper.WhsWarehouse2.WarehouseAddress.PK;
			AssertHasMessageError(entryInstruction.CEI_OA_Warehouse2Info, messageError);

			var area = (IWhsArea)whsHelper.WhsWarehouse2.Areas.AddNew();
			area.WA_AreaType = "IPR";
			area.WA_Name = "AREA";
			Factory.Save();
			entryInstruction.Validation.ValidateCEI_OA_Warehouse2();
			AssertNoMessageError(entryInstruction.CEI_OA_Warehouse2Info, messageError);

			area.WA_AreaType = "OTH";
			entryInstruction.Validation.ValidateCEI_OA_Warehouse2();
			AssertHasMessageError(entryInstruction.CEI_OA_Warehouse2Info, messageError);
		}

		public void TestCheckCEI_OA_Warehouse_OutOfInwardProcessing()
		{
			const string messageError = "When Job is Out of Inward Processing, the From Warehouse must have an Inward Processing Location.";

			var whsHelper = new WhsDataTestHelper(Factory);
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "BOB1";
			importer.OH_FullName = "BOB THE BUILDER";
			importer.CompanyData.OB_IMUsedBondedWhs = true;

			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "BOB2";
			supplier.OH_FullName = "BOB THE BUILDER2";
			supplier.OH_IsWarehouseClient = true;

			((OrgHeader)whsHelper.WhsWarehouse.WarehouseAddress.Header).CompanyData.OB_CusInventoryForInwardProcessing = true;

			whsHelper.UniversalTariffHelper.CreateNewOrGetExistingDataGrouping("ER");
			var outOfInwardProcessing = whsHelper.UniversalTariffHelper.CreateRefCusProcedure("ER", "", "AB", "20", "", "AB DESC", "", group: "JC");
			outOfInwardProcessing.ZZ6_OutOfInwardProcessing = "Y";
			Factory.Save();

			TestWarehouseWithGoodBoolean(importer, outOfInwardProcessing, out var entryInstruction, true, "ENT");

			var declaration = entryInstruction.JobDeclaration;
			declaration.JE_OH_Supplier = supplier.PK;

			Mock.Get((BaseJobDeclarationWithEntryInstructions)declaration).Protected().SetupGet<bool>("SupportMultipleWarehouseEntryCore").Returns(true);
			var invoiceLine = declaration.InvoiceLines[0];

			entryInstruction.Validation.ValidateCEI_OA_Warehouse();
			AssertNoMessageError("No warehouse, so there should be no error.", entryInstruction.CEI_OA_WarehouseInfo, messageError);

			entryInstruction.CEI_OA_Warehouse = whsHelper.WhsWarehouse.WarehouseAddress.PK;
			entryInstruction.Validation.ValidateCEI_OA_Warehouse();

			Assert("Precondition IsWarehouseRequiredForWarehouseValidation", entryInstruction.IsWarehouseRequiredForWarehouseValidation);
			Assert("Precondition IsBondedWarehousingFieldValidationRequired", entryInstruction.EntryHeader.IsBondedWarehousingFieldValidationRequired);
			Assert("Precondition IsOutOfInwardProcessing", invoiceLine.IsOutOfInwardProcessing);
			AssertHasMessageError(entryInstruction.CEI_OA_WarehouseInfo, messageError);

			var area = (IWhsArea)whsHelper.WhsWarehouse.Areas.AddNew();
			area.WA_AreaType = "IPR";
			area.WA_Name = "AREA";
			Factory.Save();
			entryInstruction.Validation.ValidateCEI_OA_Warehouse();
			AssertNoMessageError(entryInstruction.CEI_OA_WarehouseInfo, messageError);

			area.WA_AreaType = "OTH";
			entryInstruction.Validation.ValidateCEI_OA_Warehouse();
			AssertHasMessageError(entryInstruction.CEI_OA_WarehouseInfo, messageError);
		}

		public void TestCheckCEI_OA_Warehouse_OutOfInwardProcessing_Supplier()
		{
			const string messageError = "When Job is Out of Inward Processing, the From Warehouse must have an Inward Processing Location.";

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "BOB1";
			importer.OH_FullName = "BOB THE BUILDER";
			importer.CompanyData.OB_IMUsedBondedWhs = true;

			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "BOB2";
			supplier.OH_FullName = "BOB THE BUILDER2";
			supplier.OH_IsWarehouseClient = true;

			var whsHelper = new WhsDataTestHelper(Factory);
			((OrgHeader)whsHelper.WhsWarehouse.WarehouseAddress.Header).CompanyData.OB_CusInventoryForInwardProcessing = true;
			whsHelper.UniversalTariffHelper.CreateNewOrGetExistingDataGrouping("ER");
			var outOfInwardProcessing = whsHelper.UniversalTariffHelper.CreateRefCusProcedure("ER", "", "AB", "20", "", "AB DESC", "", group: "JC");
			outOfInwardProcessing.ZZ6_OutOfInwardProcessing = "Y";
			Factory.Save();

			TestWarehouseWithGoodBoolean(importer, outOfInwardProcessing, out var entryInstruction, true, "ENT");

			var declaration = entryInstruction.JobDeclaration;
			declaration.JE_OH_Supplier = supplier.PK;

			Mock.Get((BaseJobDeclarationWithEntryInstructions)declaration).Protected().SetupGet<bool>("SupportMultipleWarehouseEntryCore").Returns(true);
			entryInstruction.CEI_OA_Warehouse = whsHelper.WhsWarehouse.WarehouseAddress.PK;

			AssertHasMessageError(entryInstruction.CEI_OA_WarehouseInfo, messageError);

			supplier.OH_IsWarehouseClient = false;
			entryInstruction.Validation.ValidateCEI_OA_Warehouse();
			AssertNoMessageError(entryInstruction.CEI_OA_WarehouseInfo, messageError);
		}

		public void TestCheckCEI_OA_Warehouse2_IntoWarehouse()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);

				var importer = Factory.New<OrgHeader>();
				importer.OH_Code = "BOB1";
				importer.OH_FullName = "BOB THE BUILDER";
				importer.CompanyData.OB_IMUsedBondedWhs = true;
				var warehouse1 = Factory.New<OrgHeader>();
				warehouse1.OH_Code = "WAR1";
				warehouse1.OH_FullName = "WAREHOUSE 1";
				warehouse1.OH_RL_NKClosestPort = "AUSYD";
				warehouse1.MainAddress.OA_Address1 = "ADD 1";
				var warehouse2 = Factory.New<OrgHeader>();
				warehouse2.OH_Code = "WAR2";
				warehouse2.OH_FullName = "WAREHOUSE 2";
				warehouse2.OH_RL_NKClosestPort = "ZAJNB";
				warehouse2.MainAddress.OA_Address1 = "ADD 1";
				helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "AB", "10", "", "AB DESC", "", intoWarehouse: true);
				helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "CD", "10", "", "CD DESC", "", outOfWarehouse: true);
				Factory.Save();

				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_OH_Importer = importer.PK;
				var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = "AB";
				var entry = declaration.CustomsEntryHeaders.AddNew();
				var entryLine = entry.MergedLines.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				invoiceLine.JI_Procedure = "0010";
				entry.CH_CEI_Instruction = entryInstruction.PK;
				entry.EntryNumber = "ENT1020";
				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(true);

				entryInstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
				var bondedWarehouseIsRequiredForInwardBondedWarehousingMessageError = CusEntryHeader.BondedWarehouseIsRequiredForInwardBondedWarehousing("BOB1", entry.EntryHeaderDescriptiveMenuItemText);
				AssertHasMessageError(entryInstruction.CEI_OA_Warehouse2Info, bondedWarehouseIsRequiredForInwardBondedWarehousingMessageError);
				entryInstruction.CEI_OA_Warehouse2 = warehouse1.MainAddress.PK;
				AssertNoMessageError(entryInstruction.CEI_OA_Warehouse2Info, bondedWarehouseIsRequiredForInwardBondedWarehousingMessageError);
				var bondedWarehouseAddressShouldBeInsideDeclarationCountryMessageError = CusEntryHeader.BondedWarehouseAddressShouldBeInsideDeclarationCountry("BOB1", entry.EntryHeaderDescriptiveMenuItemText, "South Africa");
				AssertHasMessageError(entryInstruction.CEI_OA_Warehouse2Info, bondedWarehouseAddressShouldBeInsideDeclarationCountryMessageError);
				entryInstruction.CEI_OA_Warehouse2 = warehouse2.MainAddress.PK;
				AssertNoMessageError(entryInstruction.CEI_OA_Warehouse2Info, bondedWarehouseIsRequiredForInwardBondedWarehousingMessageError);
				AssertNoMessageError(entryInstruction.CEI_OA_Warehouse2Info, bondedWarehouseAddressShouldBeInsideDeclarationCountryMessageError);

				declaration.JE_OH_Importer = ZGuid.Empty;
				AssertEquals(false, declaration.IsBondedWarehousingFieldValidationRequired);
				entryInstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
				AssertNoMessageError(entryInstruction.CEI_OA_Warehouse2Info, bondedWarehouseIsRequiredForInwardBondedWarehousingMessageError);
				AssertNoMessageError(entryInstruction.CEI_OA_Warehouse2Info, bondedWarehouseAddressShouldBeInsideDeclarationCountryMessageError);
				entryInstruction.CEI_OA_Warehouse2 = warehouse1.MainAddress.PK;
				AssertNoMessageError(entryInstruction.CEI_OA_Warehouse2Info, bondedWarehouseIsRequiredForInwardBondedWarehousingMessageError);
				AssertNoMessageError(entryInstruction.CEI_OA_Warehouse2Info, bondedWarehouseAddressShouldBeInsideDeclarationCountryMessageError);
				AssertNoError(entryInstruction.CEI_OA_Warehouse2Info, WarehouseTransactionExistsNeedsCancel);
				entry.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreated;
				Factory.Save();
				entryInstruction.CEI_OA_Warehouse2 = warehouse2.MainAddress.PK;
				AssertNoMessageError(entryInstruction.CEI_OA_Warehouse2Info, bondedWarehouseIsRequiredForInwardBondedWarehousingMessageError);
				AssertNoMessageError(entryInstruction.CEI_OA_Warehouse2Info, bondedWarehouseAddressShouldBeInsideDeclarationCountryMessageError);
				AssertHasError(entryInstruction.CEI_OA_Warehouse2Info, WarehouseTransactionExistsNeedsCancel);
			}
		}

		public void TestCheckCEI_OA_Warehouse_OutOfWarehouse()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);

				var importer = Factory.New<OrgHeader>();
				importer.OH_Code = "BOB1";
				importer.OH_FullName = "BOB THE BUILDER";
				importer.CompanyData.OB_IMUsedBondedWhs = true;
				var warehouse1 = Factory.New<OrgHeader>();
				warehouse1.OH_Code = "WAR1";
				warehouse1.OH_FullName = "WAREHOUSE 1";
				warehouse1.OH_RL_NKClosestPort = "AUSYD";
				warehouse1.MainAddress.OA_Address1 = "ADD 1";
				var warehouse2 = Factory.New<OrgHeader>();
				warehouse2.OH_Code = "WAR2";
				warehouse2.OH_FullName = "WAREHOUSE 2";
				warehouse2.OH_RL_NKClosestPort = "ZAJNB";
				warehouse2.MainAddress.OA_Address1 = "ADD 1";
				helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "AB", "10", "", "AB DESC", "", intoWarehouse: true);
				helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "CD", "10", "", "CD DESC", "", outOfWarehouse: true);
				Factory.Save();

				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_OH_Importer = importer.PK;
				var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = "CD";
				var entry = declaration.CustomsEntryHeaders.AddNew();
				var entryLine = entry.MergedLines.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				invoiceLine.JI_Procedure = "0010";
				entry.CH_CEI_Instruction = entryInstruction.PK;
				entry.EntryNumber = "ENT1020";
				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine.SetUseBondedWarehouseAutomationForTesting(true);

				entryInstruction.CEI_OA_Warehouse = ZGuid.Empty;
				var bondedWarehouseIsRequiredForOutwardBondedWarehousingMessageError = CusEntryHeader.BondedWarehouseIsRequiredForOutwardBondedWarehousing("BOB1", entry.EntryHeaderDescriptiveMenuItemText);
				AssertHasMessageError(entryInstruction.CEI_OA_WarehouseInfo, bondedWarehouseIsRequiredForOutwardBondedWarehousingMessageError);
				entryInstruction.CEI_OA_Warehouse = warehouse1.MainAddress.PK;
				AssertNoMessageError(entryInstruction.CEI_OA_WarehouseInfo, bondedWarehouseIsRequiredForOutwardBondedWarehousingMessageError);
				var bondedWarehouseAddressShouldBeInsideDeclarationCountryMessageError = CusEntryHeader.BondedWarehouseAddressShouldBeInsideDeclarationCountry("BOB1", entry.EntryHeaderDescriptiveMenuItemText, "South Africa");
				AssertHasMessageError(entryInstruction.CEI_OA_WarehouseInfo, bondedWarehouseAddressShouldBeInsideDeclarationCountryMessageError);
				entryInstruction.CEI_OA_Warehouse = warehouse2.MainAddress.PK;
				AssertNoMessageError(entryInstruction.CEI_OA_WarehouseInfo, bondedWarehouseIsRequiredForOutwardBondedWarehousingMessageError);
				AssertNoMessageError(entryInstruction.CEI_OA_WarehouseInfo, bondedWarehouseAddressShouldBeInsideDeclarationCountryMessageError);

				declaration.JE_OH_Importer = ZGuid.Empty;
				AssertEquals(false, declaration.IsBondedWarehousingFieldValidationRequired);
				entryInstruction.CEI_OA_Warehouse = ZGuid.Empty;
				AssertNoMessageError(entryInstruction.CEI_OA_WarehouseInfo, bondedWarehouseIsRequiredForOutwardBondedWarehousingMessageError);
				AssertNoMessageError(entryInstruction.CEI_OA_WarehouseInfo, bondedWarehouseAddressShouldBeInsideDeclarationCountryMessageError);
				entryInstruction.CEI_OA_Warehouse = warehouse1.MainAddress.PK;
				AssertNoMessageError(entryInstruction.CEI_OA_WarehouseInfo, bondedWarehouseIsRequiredForOutwardBondedWarehousingMessageError);
				AssertNoMessageError(entryInstruction.CEI_OA_WarehouseInfo, bondedWarehouseAddressShouldBeInsideDeclarationCountryMessageError);
				AssertNoError(entryInstruction.CEI_OA_WarehouseInfo, WarehouseTransactionExistsNeedsCancel);
				entry.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreated;
				Factory.Save();
				entryInstruction.CEI_OA_Warehouse = warehouse2.MainAddress.PK;
				AssertNoMessageError(entryInstruction.CEI_OA_WarehouseInfo, bondedWarehouseIsRequiredForOutwardBondedWarehousingMessageError);
				AssertNoMessageError(entryInstruction.CEI_OA_WarehouseInfo, bondedWarehouseAddressShouldBeInsideDeclarationCountryMessageError);
				AssertHasError(entryInstruction.CEI_OA_WarehouseInfo, WarehouseTransactionExistsNeedsCancel);
			}
		}

		public void TestCheckRowError_ValidateNotAllowedMultipleLinkedEntryHeaders()
		{
			const string expectedErrorMessage = "The Entry Instruction is linked to multiple Entry Headers. Adding a new Entry Instruction could solve the problem. If you are attempting to save after making a change please also perform 'Generate Entries (Merge)' before attempting to save.";
			var (declaration, entryInstruction) = SetUpDeclarationAndInstruction();
			entryInstruction.AllowedToBeLinkedToMultipleEntryHeadersCoreExposed = false;
			CombineAssertions(() =>
			{
				entryInstruction.Validation.ValidateAll();
				AssertNoRowError("Entry Instruction is linked to 0 entry headers", entryInstruction, expectedErrorMessage);

				var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
				entryHeader1.CH_MessageType = "AAA";
				entryHeader1.CH_CEI_Instruction = entryInstruction.PK;
				entryInstruction.Validation.ValidateAll();
				AssertNoRowError("Entry Instruction is linked to 1 entry header", entryInstruction, expectedErrorMessage);

				var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
				entryHeader2.CH_MessageType = "BBB";
				entryHeader2.CH_CEI_Instruction = entryInstruction.PK;
				entryInstruction.Validation.ValidateAll();
				AssertNoRowError("Entry Instruction is linked to 2 entry headers, but CH_MessageType are different", entryInstruction, expectedErrorMessage);

				var entryHeader3 = declaration.CustomsEntryHeaders.AddNew();
				entryHeader3.CH_MessageType = "CCC";
				entryHeader3.CH_CEI_Instruction = entryInstruction.PK;
				entryInstruction.Validation.ValidateAll();
				AssertNoRowError("Entry Instruction is linked to 3 entry headers, but CH_MessageType are different", entryInstruction, expectedErrorMessage);

				entryHeader2.CH_MessageType = "AAA";
				entryInstruction.Validation.ValidateAll();
				AssertHasRowError("Entry Instruction is linked to 3 entry headers, but 2 of the 3 CH_MessageType are equal", entryInstruction, expectedErrorMessage);

				entryHeader3.CH_MessageType = "AAA";
				entryInstruction.Validation.ValidateAll();
				AssertHasRowError("Entry Instruction is linked to 3 entry headers, but 3 of the 3 CH_MessageType are equal", entryInstruction, expectedErrorMessage);
			});
		}

		public void TestCheckRowError_ValidateAllowedMultipleLinkedEntryHeaders()
		{
			const string expectedErrorMessage = "The Entry Instruction is linked to multiple Entry Headers. Adding a new Entry Instruction could solve the problem. If you are attempting to save after making a change please also perform 'Generate Entries (Merge)' before attempting to save.";
			var (declaration, entryInstruction) = SetUpDeclarationAndInstruction();
			entryInstruction.AllowedToBeLinkedToMultipleEntryHeadersCoreExposed = true;
			CombineAssertions(() =>
			{
				entryInstruction.Validation.ValidateAll();
				AssertNoRowError("Entry Instruction is linked to 0 entry headers", entryInstruction, expectedErrorMessage);

				var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
				entryHeader1.CH_MessageType = "AAA";
				entryHeader1.CH_CEI_Instruction = entryInstruction.PK;
				entryInstruction.Validation.ValidateAll();
				AssertNoRowError("Entry Instruction is linked to 1 entry header", entryInstruction, expectedErrorMessage);

				var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
				entryHeader2.CH_MessageType = "BBB";
				entryHeader2.CH_CEI_Instruction = entryInstruction.PK;
				entryInstruction.Validation.ValidateAll();
				AssertNoRowError("Entry Instruction is linked to 2 entry headers, but CH_MessageType are different", entryInstruction, expectedErrorMessage);

				var entryHeader3 = declaration.CustomsEntryHeaders.AddNew();
				entryHeader3.CH_MessageType = "CCC";
				entryHeader3.CH_CEI_Instruction = entryInstruction.PK;
				entryInstruction.Validation.ValidateAll();
				AssertNoRowError("Entry Instruction is linked to 3 entry headers, but CH_MessageType are different", entryInstruction, expectedErrorMessage);

				entryHeader2.CH_MessageType = "AAA";
				entryInstruction.Validation.ValidateAll();
				AssertNoRowError("Entry Instruction is linked to 3 entry headers, but 2 of the 3 CH_MessageType are equal", entryInstruction, expectedErrorMessage);

				entryHeader3.CH_MessageType = "AAA";
				entryInstruction.Validation.ValidateAll();
				AssertNoRowError("Entry Instruction is linked to 3 entry headers, but 3 of the 3 CH_MessageType are equal", entryInstruction, expectedErrorMessage);
			});
		}

		public void TestCheckRowError_ValidateAgainstLinkedEntryHeadersAfterMerge()
		{
			var expectedErrorMessage = "The Entry Instruction is linked to multiple Entry Headers. Adding a new Entry Instruction could solve the problem. If you are attempting to save after making a change please also perform 'Generate Entries (Merge)' before attempting to save.";
			var declaration = Factory.New<DeclarationForEritreaWithCEISupport>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction = Factory.New<EntryInstructionForTesting>();
			entryInstruction.CEI_JE = declaration.PK;
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_ValuationDateOverride = new ZDateTime(2020, 01, 01);
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_ValuationDateOverride = new ZDateTime(2020, 12, 31);
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			CombineAssertions(() =>
			{
				entryInstruction.AllowedToBeLinkedToMultipleEntryHeadersCoreExposed = true;
				declaration.DoMerge();
				AssertNoRowError("When merge results in multiple entries linked to a single entry instruction (and this behaviour is allowed)", entryInstruction, expectedErrorMessage);
				entryInstruction.AllowedToBeLinkedToMultipleEntryHeadersCoreExposed = false;
				declaration.DoMerge();
				AssertHasRowError("When merge results in multiple entries linked to a single entry instruction (and this behaviour isn't allowed)", entryInstruction, expectedErrorMessage);
				invoice2.JZ_ValuationDateOverride = new ZDateTime(2020, 01, 01);
				declaration.DoMerge();
				AssertNoRowError("When merge doesn't result in multiple entries linked to a single entry instruction", entryInstruction, expectedErrorMessage);
			});
		}

		public void TestCheckRowError_ValidateMultipleLinkedEntry_ErrorAndMessageError()
		{
			var expectedErrorMessage = "The Entry Instruction is linked to multiple Entry Headers. Adding a new Entry Instruction could solve the problem. If you are attempting to save after making a change please also perform 'Generate Entries (Merge)' before attempting to save.";
			var declaration = Factory.New<DeclarationForEritreaWithCEISupport>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction = Factory.New<EntryInstructionForTesting>();
			entryInstruction.CEI_JE = declaration.PK;
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_ValuationDateOverride = new ZDateTime(2020, 01, 01);
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_ValuationDateOverride = new ZDateTime(2020, 12, 31);
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			CombineAssertions(() =>
			{
				entryInstruction.AddBlueRowMessageErrorForMultipleLinkedEntriesInsteadOfRedErrorExposed = false;
				declaration.DoMerge();
				AssertHasRowError("Expecting RED Row Error", entryInstruction, expectedErrorMessage);
				invoice2.JZ_ValuationDateOverride = new ZDateTime(2020, 01, 01);
				declaration.DoMerge();
				AssertNoRowError("Expecting No RED Row Error", entryInstruction, expectedErrorMessage);

				entryInstruction.AddBlueRowMessageErrorForMultipleLinkedEntriesInsteadOfRedErrorExposed = true;
				invoice2.JZ_ValuationDateOverride = new ZDateTime(2020, 12, 31);
				declaration.DoMerge();
				AssertHasRowMessageError("Expecting BLUE Row Message Error", entryInstruction, expectedErrorMessage);
				invoice2.JZ_ValuationDateOverride = new ZDateTime(2020, 01, 01);
				declaration.DoMerge();
				AssertNoRowMessageError("Expecting No BLUE Row Message Error", entryInstruction, expectedErrorMessage);
			});
		}

		public void TestValidateNoDuplicateInstructions()
		{
			var testDeclaration = Factory.NewWithValidTestData<DeclarationForEritreaWithCEISupport>();
			testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var testInstruction1 = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var testInstruction2 = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();

			testInstruction1.CEI_Style = "A";
			testInstruction2.CEI_Style = "A";
			var duplicateEntryMessage = "This Entry Instruction already exists in the instructions list";
			testInstruction2.Validation.ValidateAll();
			AssertHasRowWarningContaining(testInstruction2, duplicateEntryMessage);
			testInstruction2.CEI_Style = "B";
			testInstruction2.Validation.ValidateAll();
			AssertNoRowWarningContaining(testInstruction2, duplicateEntryMessage);
		}

		public void TestCheckCEI_OA_Warehouse2()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "BOB1";
			importer.OH_FullName = "BOB THE BUILDER";
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			var warehouse2 = Factory.New<OrgHeader>();
			warehouse2.OH_Code = "WAR2";
			warehouse2.OH_FullName = "WAREHOUSE 2";
			warehouse2.OH_RL_NKClosestPort = "ZAJNB";
			warehouse2.MainAddress.OA_Address1 = "ADD 1";
			var cusProcedureHelper = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.UnitedKingdom, "", "AB", "10", "", "AB DESC", "", intoWarehouse: true, group: "JC");
			var cusProcedureHelper2 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.UnitedKingdom, "", "AB", "20", "", "AB DESC", "", group: "JC");
			cusProcedureHelper2.ZZ6_IntoInwardProcessing = "Y";
			var cusProcedureHelper3 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.UnitedKingdom, "", "AB", "30", "", "AB DESC", "", group: "JC");
			cusProcedureHelper3.ZZ6_IntoOutwardProcessing = "Y";
			Factory.Save();

			TestWarehouseWithGoodBoolean(importer, cusProcedureHelper, out var entryInstruction, false, "ENT1020");
			Assert(entryInstruction.IsWarehouse2RequiredForWarehouseValidation);
			entryInstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
			AssertNoMessageErrors(entryInstruction.CEI_OA_Warehouse2Info);

			TestWarehouseWithGoodBoolean(importer, cusProcedureHelper, out entryInstruction, true, "ENT1020");
			Assert(entryInstruction.IsWarehouse2RequiredForWarehouseValidation);
			entryInstruction.CEI_OA_Warehouse2 = warehouse2.MainAddress.PK;
			AssertHasMessageErrors(entryInstruction.CEI_OA_Warehouse2Info);

			importer.CompanyData.OB_IMUsedBondedWhs = false;
			importer.CompanyData.OB_CusInventoryForInwardProcessing = true;
			TestWarehouseWithGoodBoolean(importer, cusProcedureHelper2, out entryInstruction, false, "ENT1020");

			Assert(entryInstruction.IsWarehouse2RequiredForWarehouseValidation);

			importer.CompanyData.OB_CusInventoryForInwardProcessing = false;
			importer.CompanyData.OB_CusInventoryForOutwardProcessing = true;
			TestWarehouseWithGoodBoolean(importer, cusProcedureHelper3, out entryInstruction, false, "ENT1020");

			Assert(entryInstruction.IsWarehouse2RequiredForWarehouseValidation);
		}

		public void TestCheckCEI_OA_Warehouse_Warehouse()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "BOB1";
			importer.OH_FullName = "BOB THE BUILDER";
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			var warehouse1 = Factory.New<OrgHeader>();
			warehouse1.OH_Code = "WAR1";
			warehouse1.OH_FullName = "WAREHOUSE 1";
			warehouse1.OH_RL_NKClosestPort = "AUSYD";
			var cusProcedureHelper = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.UnitedKingdom, "", "AB", "10", "", "AB DESC", "", intoWarehouse: true, outOfWarehouse: true, group: "JC");
			var cusProcedureHelper2 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.UnitedKingdom, "", "AB", "20", "", "AB DESC", "", group: "JC");
			cusProcedureHelper2.ZZ6_OutOfInwardProcessing = "Y";
			var cusProcedureHelper3 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.UnitedKingdom, "", "AB", "30", "", "AB DESC", "", group: "JC");
			cusProcedureHelper3.ZZ6_OutofOutwardProcessing = "Y";
			Factory.Save();

			TestWarehouseWithGoodBoolean(importer, cusProcedureHelper, out var entryInstruction, false, "ENT1020");

			Assert(entryInstruction.IsWarehouseRequiredForWarehouseValidation);
			entryInstruction.CEI_OA_Warehouse = ZGuid.Empty;
			AssertNoMessageErrors(entryInstruction.CEI_OA_WarehouseInfo);

			TestWarehouseWithGoodBoolean(importer, cusProcedureHelper, out entryInstruction, true, "ENT1020");

			Assert(entryInstruction.IsWarehouseRequiredForWarehouseValidation);
			entryInstruction.CEI_OA_Warehouse = warehouse1.MainAddress.PK;
			AssertHasMessageErrors(entryInstruction.CEI_OA_WarehouseInfo);

			TestWarehouseWithGoodBoolean(importer, cusProcedureHelper2, out entryInstruction, false, "ENT1020");

			Assert(entryInstruction.IsWarehouseRequiredForWarehouseValidation);

			TestWarehouseWithGoodBoolean(importer, cusProcedureHelper3, out entryInstruction, false, "ENT1020");

			Assert(entryInstruction.IsWarehouseRequiredForWarehouseValidation);
		}

		public void TestCheckCEI_OA_Warehouse2_NoMessageErrorShownWhenDeclarantAndImporterNotWarehoused()
		{
			const string errorMsg = "When Job is Into Inward Processing, the To Warehouse must have an Inward Processing Location.";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_IsWarehouseClient = true;
			importer.CompanyData.OB_CusInventoryForInwardProcessing = true;
			var procedure = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.UnitedKingdom, "", "AB", "10", "", "AB DESC", "", intoWarehouse: false, outOfWarehouse: false, group: "JC");
			procedure.ZZ6_IntoInwardProcessing = "Y";

			var warehouse1 = Factory.New<OrgHeader>();
			warehouse1.OH_Code = "WAR1";
			warehouse1.OH_FullName = "WAREHOUSE 1";
			warehouse1.OH_RL_NKClosestPort = "AUSYD";

			Factory.Save();

			TestWarehouseWithGoodBoolean(importer, procedure, out var entryInstruction, true, "ENT1234");
			var whsInfo = entryInstruction.CEI_OA_Warehouse2Info;

			var declaration = entryInstruction.JobDeclaration;

			CombineAssertions(() =>
			{
				entryInstruction.Validation.ValidateCEI_OA_Warehouse2();
				AssertNoMessageError("There should be no message error when CEI_OA_Warehouse2 is empty.", whsInfo, errorMsg);

				entryInstruction.CEI_OA_Warehouse2 = warehouse1.PK;
				entryInstruction.Validation.ValidateCEI_OA_Warehouse2();
				AssertHasMessageError("There should be a message error when Importer is Warehouse Client and no Inward Processing Locations are present", whsInfo, errorMsg);

				importer.OH_IsWarehouseClient = false;
				entryInstruction.Validation.ValidateCEI_OA_Warehouse2();
				AssertNoMessageError("Importer is not Warehouse Client, no message error expected", whsInfo, errorMsg);

				declaration.JE_OA_DeclarantAddress = importer.MainAddress.PK;
				entryInstruction.Validation.ValidateCEI_OA_Warehouse2();
				AssertNoMessageError("Declarant is not Warehouse Client, no message error expected", whsInfo, errorMsg);

				importer.OH_IsWarehouseClient = true;
				entryInstruction.Validation.ValidateCEI_OA_Warehouse2();
				AssertHasMessageError("There should be a message error when Declarant is Warehouse Client and no Inward Processing Locations are present", whsInfo, errorMsg);
			});
		}

		(BaseJobDeclaration Declaration, EntryInstructionForTesting EntryInstruction) SetUpDeclarationAndInstruction()
		{
			var declaration = Factory.New<DeclarationForEritreaWithCEISupport>();
			var entryInstruction = Factory.New<EntryInstructionForTesting>();
			entryInstruction.CEI_JE = declaration.PK;
			return (declaration, entryInstruction);
		}

		internal class DeclarationForEritreaWithCEISupport : BaseJobDeclaration
		{
			public DeclarationForEritreaWithCEISupport(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}

			protected override EntryInstructionProvider GetCustomsEntryInstructionProviderCore()
			{
				return new EntryInstructionProviderForEritrea(this);
			}
		}

		internal class EntryInstructionProviderForEritrea : EntryInstructionProvider
		{
			public EntryInstructionProviderForEritrea(BaseJobDeclaration parentDeclaration)
				: base(parentDeclaration)
			{ }
		}

		internal class EntryInstructionForTesting : CusEntryInstruction
		{
			public EntryInstructionForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public ZBool AllowedToBeLinkedToMultipleEntryHeadersCoreExposed { get; set; }
			protected override ZBool AllowedToBeLinkedToMultipleEntryHeadersCore => AllowedToBeLinkedToMultipleEntryHeadersCoreExposed;

			public ZBool AddBlueRowMessageErrorForMultipleLinkedEntriesInsteadOfRedErrorExposed { get; set; }
			public override ZBool AddBlueRowMessageErrorForMultipleLinkedEntriesInsteadOfRedError => AddBlueRowMessageErrorForMultipleLinkedEntriesInsteadOfRedErrorExposed;
		}

		void TestWarehouseWithGoodBoolean(OrgHeader importer, RefCusProcedure cusProcedureHelper, out CusEntryInstruction entryInstruction, bool areMultipleEntryInstructionsAllowed, string entryNumber)
		{
			var declarationMock = Factory.NewMoq<BaseJobDeclarationWithEntryInstructions>();
			var declarationMockProtected = declarationMock.Protected();
			declarationMockProtected.Setup<bool>("GetIsWHSUniversalXMLActive").Returns(true);
			declarationMock.Setup(m => m.AreMultipleEntryInstructionsAllowed).Returns(areMultipleEntryInstructionsAllowed);
			declarationMockProtected.Setup<bool>("SupportsBondedWarehousingCore").Returns(true);
			var declaration = declarationMock.Object;
			cusProcedureHelper.ZZ6_ZZZ_NKDataGrouping = declaration.GetDefaultDataGroupingCode();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = cusProcedureHelper.ZZ6_Group;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = cusProcedureHelper.ZZ6_ProcedureCode + cusProcedureHelper.ZZ6_PreviousProcedureCode;
			entry.CH_CEI_Instruction = entryInstruction.PK;
			entry.EntryNumber = entryNumber;
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.SetUseBondedWarehouseAutomationForTesting(true);
		}

		const string WarehouseTransactionExistsNeedsCancel = "There is an Inventory transaction created against this Entry Instruction.\r\nPlease cancel it before changing this value.";
	}
}

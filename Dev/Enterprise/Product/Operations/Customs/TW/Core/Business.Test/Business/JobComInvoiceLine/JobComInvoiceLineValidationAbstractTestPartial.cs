using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.TW.Business.Testing.InvoiceLineLinkControllingMsgHeaderCollectionTest;

namespace Enterprise.Customs.TW.Business.Testing
{
	abstract partial class JobComInvoiceLineValidationAbstractTest<TJobComInvoiceLineValidation> : BusinessObjectValidationTestCase
		where TJobComInvoiceLineValidation : JobComInvoiceLineValidation
	{
		public void TestCheckJI_CustomPermitUQ()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var messageHeader = instruction.ControllingMessageHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			messageHeader.AssignHeaderToInvoiceLines();
			messageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			messageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			invoiceLine.Validation.ValidateJI_CustomPermitUQ();
			AssertNoWarningContaining("NX101, Code15", invoiceLine.JI_CustomPermitUQInfo, MandatoryValidation.YouHaveNotEntered);

			messageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			invoiceLine.Validation.ValidateJI_CustomPermitUQ();
			AssertHasWarningContaining("NX101, Code1", invoiceLine.JI_CustomPermitUQInfo, MandatoryValidation.YouHaveNotEntered);

			messageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301;
			invoiceLine.Validation.ValidateJI_CustomPermitUQ();
			AssertNoWarningContaining("NX301, Code1", invoiceLine.JI_CustomPermitUQInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_CompositionsWithImportOrExportRegulations581Or541()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();

			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			var tariff_26179090101 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "26179090101", minDate, maxDate);
			var tariff_26179090101_Attribute_IMP_581 = helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportRegulations, "581", tariff_26179090101);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWImportRegulations, tariff_26179090101_Attribute_IMP_581.ZZ3_Value, "應檢附行政院原子能委員會同意文件。", minDate, maxDate);

			var tariff_26179090101_Attribute_EXP_541 = helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ExportRegulations, "541", tariff_26179090101);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWExportRegulations, tariff_26179090101_Attribute_EXP_541.ZZ3_Value, "應檢附行政院原子能委員會同意文件。", minDate, maxDate);

			var tariff_84716090208 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "84716090208", minDate, maxDate);
			var tariff_84716090208_Attribute_IMP_602 = helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportRegulations, "602", tariff_84716090208);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWImportRegulations, tariff_84716090208_Attribute_IMP_602.ZZ3_Value, "（一）進口屬電信管制射頻器材應經許可之項目，由海關驗憑國家通訊傳播委員會核發之電信管制射頻器材進口許可證後放行。但屬軍事專用者，由海關驗憑國防部核發之電信管制射頻器材進口許可證後放行。（二）如非屬應經許可之電信管制射頻器材，不論民用或軍事專用，均可免憑前述許可證放行。（三）經國家通訊傳播委員會或其認可委託之驗證機構型式認證合格或符合性聲明證明之無線電信終端設備或低功率射頻電機，免請領進口許可證。但應憑國家通訊傳播委員會或其認可委託之驗證機構核發之電信終端設備審定證明、電信終端設備符合性聲明證明、低功率射頻電機型式認證證明或低功率射頻電機符合性聲明證明辦理通關。（備註：原由電信總局核發之前揭證明文件亦適用本規定。）", minDate, maxDate);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var targetInfo = invoiceLine.JI_CompositionsInfo;
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			invoiceLine.JI_Tariff = tariff_26179090101.ZZ1_TariffCode;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_Tariff = tariff_84716090208.ZZ1_TariffCode;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			invoiceLine.JI_Tariff = tariff_26179090101.ZZ1_TariffCode;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_Tariff = tariff_84716090208.ZZ1_TariffCode;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			var controllingMessageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			invoiceLine.AssignCMHeaderToInvoices(controllingMessageHeader);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);
		}

		public void TestCheckJI_GroupLength()
		{
			var warning = "Grouping plus Declaration Goods Description Only the first 512 characters will be sent to the customs.";
			var targetInfo = InvoiceLine.JI_GroupInfo;
			InvoiceLine.JI_DeclarationGoodsDescription = "AAA";
			InvoiceLine.JI_Group = new ZString('A', 510);
			AssertHasWarning(targetInfo, warning);

			InvoiceLine.JI_Group = new ZString('A', 509);
			AssertNoWarning(targetInfo, warning);

			InvoiceLine.JI_DeclarationGoodsDescription = "AAAA";
			InvoiceLine.Validation.ValidateJI_Group();
			AssertHasWarning(targetInfo, warning);
		}

		public void TestCheckJI_TextileWidth()
		{
			InvoiceLine.JI_TextileWidthUQ = "MM";
			InvoiceLine.JI_TextileWidth = 1.111111m;
			AssertNoMessageErrorContaining(InvoiceLine.JI_TextileWidthInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(InvoiceLine.JI_TextileWidthInfo, ValidationConstants.InvoiceLine.TextileWidthIsNotRequired);

			InvoiceLine.JI_TextileWidth = 0m;
			AssertHasMessageErrorContaining(InvoiceLine.JI_TextileWidthInfo, MandatoryValidation.YouHaveNotEntered);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType("TW", "HSN");
			Factory.Save();

			var tariff0000000021 = helper.CreateTariff("TW", hsnTariffType.PK, "0000000021", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffUOM(tariff0000000021.PK, "CU1", "A");
			helper.CreateTariffUOM(tariff0000000021.PK, "CU2", "MTK");

			var tariff1000000021 = helper.CreateTariff("TW", hsnTariffType.PK, "1000000021", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffUOM(tariff1000000021.PK, "CU1", "A");
			helper.CreateTariffUOM(tariff1000000021.PK, "CU2", "MTO");
			Factory.Save();

			InvoiceLine.JI_Tariff = "1000000021";
			InvoiceLine.JI_TextileWidth = 1.22222m;
			AssertHasMessageError(InvoiceLine.JI_TextileWidthInfo, ValidationConstants.InvoiceLine.TextileWidthIsNotRequired);

			InvoiceLine.JI_Tariff = "0000000021";
			InvoiceLine.JI_TextileWidth = 1.111111m;
			AssertNoMessageError(InvoiceLine.JI_TextileWidthInfo, ValidationConstants.InvoiceLine.TextileWidthIsNotRequired);
		}

		public void TestCheckJI_TextileWidthUQ()
		{
			InvoiceLine.JI_TextileWidth = 1.111111m;
			InvoiceLine.JI_TextileWidthUQ = "MM";
			AssertNoMessageErrorContaining(InvoiceLine.JI_TextileWidthUQInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(InvoiceLine.JI_TextileWidthUQInfo, ListValidation.InvalidCodeMessageError);

			InvoiceLine.JI_TextileWidthUQ = ZString.Empty;
			AssertHasMessageErrorContaining(InvoiceLine.JI_TextileWidthUQInfo, MandatoryValidation.YouHaveNotEntered);

			InvoiceLine.JI_TextileWidth = 0m;
			InvoiceLine.Validation.ValidateJI_TextileWidthUQ();
			AssertNoMessageErrorContaining(InvoiceLine.JI_TextileWidthUQInfo, MandatoryValidation.YouHaveNotEntered);

			InvoiceLine.JI_TextileWidth = 1.111111m;
			InvoiceLine.JI_TextileWidthUQ = "DD";
			AssertHasMessageErrorContaining(InvoiceLine.JI_TextileWidthUQInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJI_BottledDate()
		{
			var controllingMsgHeaderHelper = new ControllingMsgHeaderTestHelper(Factory);
			var jobDeclartion = controllingMsgHeaderHelper.New(new string[] { "IF", "DN", "CI" });
			var header = jobDeclartion.Invoices.AddNew();
			var line1 = header.JobComInvoiceLines.AddNew();

			line1.JI_AlcoholPercentage = 6M;
			line1.JI_ExpirationDate = ZDateTime.Empty;
			ControllingMsgHeaderTestHelper.AssertHasMessageErrorContainingByControllingAgency(line1, line1.JI_BottledDateInfo, ZDateTime.Now, ZDateTime.Empty, "DN", MandatoryValidation.YouHaveNotEntered);
			ControllingMsgHeaderTestHelper.AssertNoMessageErrorContainingByControllingAgency(line1, line1.JI_BottledDateInfo, ZDateTime.Now, ZDateTime.Empty, "IF", MandatoryValidation.YouHaveNotEntered);

			line1.JI_ExpirationDate = ZDateTime.Now;
			ControllingMsgHeaderTestHelper.AssertNoMessageErrorContainingByControllingAgency(line1, line1.JI_BottledDateInfo, ZDateTime.Now, ZDateTime.Empty, "DN", MandatoryValidation.YouHaveNotEntered);

			line1.JI_ExpirationDate = ZDateTime.Empty;
			line1.JI_AlcoholPercentage = 0M;
			ControllingMsgHeaderTestHelper.AssertNoMessageErrorContainingByControllingAgency(line1, line1.JI_BottledDateInfo, ZDateTime.Now, ZDateTime.Empty, "DN", MandatoryValidation.YouHaveNotEntered);

			line1.JI_AlcoholPercentage = 7M;
			ControllingMsgHeaderTestHelper.AssertNoMessageErrorContainingByControllingAgency(line1, line1.JI_BottledDateInfo, ZDateTime.Now, ZDateTime.Empty, "DN", MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_ExpirationDate()
		{
			var controllingMsgHeaderHelper = new ControllingMsgHeaderTestHelper(Factory);
			var jobDeclartion = controllingMsgHeaderHelper.New(new string[] { "IF", "DN", "CI" });
			var header = jobDeclartion.Invoices.AddNew();
			var line1 = header.JobComInvoiceLines.AddNew();

			line1.JI_AlcoholPercentage = 6M;
			line1.JI_BottledDate = ZDateTime.Empty;
			ControllingMsgHeaderTestHelper.AssertHasMessageErrorContainingByControllingAgency(line1, line1.JI_ExpirationDateInfo, ZDateTime.Now, ZDateTime.Empty, "DN", MandatoryValidation.YouHaveNotEntered);
			ControllingMsgHeaderTestHelper.AssertNoMessageErrorContainingByControllingAgency(line1, line1.JI_ExpirationDateInfo, ZDateTime.Now, ZDateTime.Empty, "IF", MandatoryValidation.YouHaveNotEntered);

			line1.JI_BottledDate = ZDateTime.Now;
			ControllingMsgHeaderTestHelper.AssertNoMessageErrorContainingByControllingAgency(line1, line1.JI_ExpirationDateInfo, ZDateTime.Now, ZDateTime.Empty, "DN", MandatoryValidation.YouHaveNotEntered);

			line1.JI_BottledDate = ZDateTime.Empty;
			line1.JI_AlcoholPercentage = 0M;
			ControllingMsgHeaderTestHelper.AssertNoMessageErrorContainingByControllingAgency(line1, line1.JI_ExpirationDateInfo, ZDateTime.Now, ZDateTime.Empty, "DN", MandatoryValidation.YouHaveNotEntered);

			line1.JI_AlcoholPercentage = 7M;
			ControllingMsgHeaderTestHelper.AssertNoMessageErrorContainingByControllingAgency(line1, line1.JI_ExpirationDateInfo, ZDateTime.Now, ZDateTime.Empty, "DN", MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_AlcoholEndOfShelfLife()
		{
			var controllingMsgHeaderHelper = new ControllingMsgHeaderTestHelper(Factory);
			var jobDeclartion = controllingMsgHeaderHelper.New(new string[] { "IF", "DN", "CI" });
			var header = jobDeclartion.Invoices.AddNew();
			var line1 = header.JobComInvoiceLines.AddNew();

			line1.JI_BottledDate = ZDateTime.Now;
			ControllingMsgHeaderTestHelper.AssertHasMessageErrorContainingByControllingAgency(line1, line1.JI_AlcoholEndOfShelfLifeInfo, ZDateTime.Now, ZDateTime.Empty, "DN", MandatoryValidation.YouHaveNotEntered);
			ControllingMsgHeaderTestHelper.AssertNoMessageErrorContainingByControllingAgency(line1, line1.JI_AlcoholEndOfShelfLifeInfo, ZDateTime.Now, ZDateTime.Empty, "IF", MandatoryValidation.YouHaveNotEntered);

			line1.JI_BottledDate = ZDateTime.Empty;
			ControllingMsgHeaderTestHelper.AssertNoMessageErrorContainingByControllingAgency(line1, line1.JI_AlcoholEndOfShelfLifeInfo, ZDateTime.Now, ZDateTime.Empty, "DN", MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckAssignedJobComInvLineRefs()
		{
			var invoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_EPTDigit1 = "";
			invoiceLine.JI_EPTDigit2 = "";
			invoiceLine.JI_EPTDigit3 = "";
			for (int i = 0; i < 10; i++)
			{
				var assignedNumber = invoiceLine.AssignedJobComInvLineRefsCollection.AddNew();
				assignedNumber.JG_ReferenceNumber = "XXX";
			}
			invoiceLine.JI_EPTDigit1 = "A";
			AssertNoRowMessageErrorContaining(invoiceLine, ValidationConstants.InvoiceLine.MoreThan10AssignedNumbersPerInvoiceLine);
			invoiceLine.JI_EPTDigit2 = "0";
			AssertNoRowMessageErrorContaining(invoiceLine, ValidationConstants.InvoiceLine.MoreThan10AssignedNumbersPerInvoiceLine);
			invoiceLine.JI_EPTDigit3 = "1";
			AssertHasRowMessageErrorContaining(invoiceLine, ValidationConstants.InvoiceLine.MoreThan10AssignedNumbersPerInvoiceLine);
			invoiceLine.JI_EPTDigit3 = "";
			AssertNoRowMessageErrorContaining(invoiceLine, ValidationConstants.InvoiceLine.MoreThan10AssignedNumbersPerInvoiceLine);
		}

		public void TestCheckJI_NewOwnerPartNo()
		{
			var helper = new TWWhsDataTestHelper(Factory);
			helper.Importer.CompanyData.OB_IMUsedBondedWhs = true;
			helper.Warehouse.CompanyData.OB_IMUsedBondedWhs = false;
			helper.Owner.CompanyData.OB_IMUsedBondedWhs = true;
			helper.Part.RelatedOrganisations.AddSupplier(helper.Supplier);
			helper.Part2.RelatedOrganisations.AddSupplier(helper.Supplier);
			helper.OwnerPart.RelatedOrganisations.AddSupplier(helper.Supplier);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = helper.Importer.PK;
			declaration.JE_OH_Supplier = helper.Supplier.PK;
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_Style = "CP";
			entryInstruction.CEI_OH_Owner = helper.Owner.PK;
			entryInstruction.CEI_OA_Warehouse = helper.Warehouse.MainAddress.PK;
			entryInstruction.CEI_OA_Warehouse2 = entryInstruction.CEI_OA_Warehouse;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_CL = entryLine.PK;

			helper.Owner.CompanyData.OB_IMUsedBondedWhs = false;
			helper.Warehouse.CompanyData.OB_IMUsedBondedWhs = true;
			invoiceLine.Validation.ValidateJI_NewOwnerPartNo();
			AssertNoWarning(invoiceLine.JI_NewOwnerPartNoInfo, JobComInvoiceLineValidation.WarningPartCodeNotFoundAtAll);

			invoiceLine.JI_NewOwnerPartNo = "SDS#@D";
			AssertHasWarning(invoiceLine.JI_NewOwnerPartNoInfo, JobComInvoiceLineValidation.WarningPartCodeNotFoundAtAll);

			invoiceLine.JI_NewOwnerPartNo = helper.Part.OP_PartNum;
			AssertNoWarning(invoiceLine.JI_NewOwnerPartNoInfo, ValidationConstants.InvoiceLine.WarningPartCodeFoundButNotRelatedToOwner);
			AssertNoWarning(invoiceLine.JI_NewOwnerPartNoInfo, ValidationConstants.InvoiceLine.WarningPartCodesFoundButNotRelatedToOwner);
			AssertNoWarning(invoiceLine.JI_NewOwnerPartNoInfo, JobComInvoiceLineValidation.WarningPartCodeNotFoundAtAll);
			AssertNoWarning(invoiceLine.JI_NewOwnerPartNoInfo, JobComInvoiceLineValidation.WarningMoreThanOneProductMatchFound);

			var ownerPart2 = helper.CreateProduct(helper.Owner.PK, helper.OwnerPart.OP_PartNum);
			ownerPart2.RelatedOrganisations.AddSupplier(helper.Warehouse);
			invoiceLine.NewOwnerProductSyncManager.Refresh();
			invoiceLine.Validation.ValidateJI_NewOwnerPartNo();
			AssertNoWarning(invoiceLine.JI_NewOwnerPartNoInfo, ValidationConstants.InvoiceLine.WarningPartCodeFoundButNotRelatedToOwner);
			AssertNoWarning(invoiceLine.JI_NewOwnerPartNoInfo, ValidationConstants.InvoiceLine.WarningPartCodesFoundButNotRelatedToOwner);
			AssertNoWarning(invoiceLine.JI_NewOwnerPartNoInfo, JobComInvoiceLineValidation.WarningPartCodeNotFoundAtAll);
			AssertHasWarning(invoiceLine.JI_NewOwnerPartNoInfo, JobComInvoiceLineValidation.WarningMoreThanOneProductMatchFound);

			invoiceLine.JI_NewOwnerPartNo = helper.Part2.OP_PartNum;
			AssertHasWarning(invoiceLine.JI_NewOwnerPartNoInfo, ValidationConstants.InvoiceLine.WarningPartCodeFoundButNotRelatedToOwner);
			AssertNoWarning(invoiceLine.JI_NewOwnerPartNoInfo, ValidationConstants.InvoiceLine.WarningPartCodesFoundButNotRelatedToOwner);
			AssertNoWarning(invoiceLine.JI_NewOwnerPartNoInfo, JobComInvoiceLineValidation.WarningPartCodeNotFoundAtAll);
			AssertNoWarning(invoiceLine.JI_NewOwnerPartNoInfo, JobComInvoiceLineValidation.WarningMoreThanOneProductMatchFound);

			var part2 = helper.CreateProduct(helper.Importer.PK, helper.Part2.OP_PartNum);
			part2.RelatedOrganisations.AddSupplier(helper.Warehouse);
			invoiceLine.NewOwnerProductSyncManager.Refresh();
			invoiceLine.Validation.ValidateJI_NewOwnerPartNo();
			AssertNoWarning(invoiceLine.JI_NewOwnerPartNoInfo, ValidationConstants.InvoiceLine.WarningPartCodeFoundButNotRelatedToOwner);
			AssertHasWarning(invoiceLine.JI_NewOwnerPartNoInfo, ValidationConstants.InvoiceLine.WarningPartCodesFoundButNotRelatedToOwner);
			AssertNoWarning(invoiceLine.JI_NewOwnerPartNoInfo, JobComInvoiceLineValidation.WarningPartCodeNotFoundAtAll);
			AssertNoWarning(invoiceLine.JI_NewOwnerPartNoInfo, JobComInvoiceLineValidation.WarningMoreThanOneProductMatchFound);
		}

		public void TestCheckJI_NewPartAttribute1()
		{
			var invoiceLine = SetupInvoiceLineWithOwnerPart();
			CheckPartAttributeValidation(invoiceLine.JI_NewPartAttribute1Info, invoiceLine.Validation.ValidateJI_NewPartAttribute1, invoiceLine.EntryInstruction.Owner, invoiceLine.NewOwnerProduct, 1);
		}

		public void TestCheckJI_NewPartAttribute2()
		{
			var invoiceLine = SetupInvoiceLineWithOwnerPart();
			CheckPartAttributeValidation(invoiceLine.JI_NewPartAttribute2Info, invoiceLine.Validation.ValidateJI_NewPartAttribute2, invoiceLine.EntryInstruction.Owner, invoiceLine.NewOwnerProduct, 2);
		}

		public void TestCheckJI_NewPartAttribute3()
		{
			var invoiceLine = SetupInvoiceLineWithOwnerPart();
			CheckPartAttributeValidation(invoiceLine.JI_NewPartAttribute3Info, invoiceLine.Validation.ValidateJI_NewPartAttribute3, invoiceLine.EntryInstruction.Owner, invoiceLine.NewOwnerProduct, 3);
		}

		#region TestCheckJI_NewSerialNumber

		public void TestCheckJI_NewSerialNumber()
		{
			var invoiceLine = SetupInvoiceLineWithOwnerPart();
			invoiceLine.JI_NewSerialNumber = "ABC";
			invoiceLine.Validation.ValidateJI_NewSerialNumber();
			AssertHasError(invoiceLine.JI_NewSerialNumberInfo, "The part '~~1' is not set up to use Serial Number with the Importer ''. Please either remove the value 'ABC' from the Serial Number field, or set up the Product and Importer to use Serial Number.");
		}

		public void TestCheckJI_NewSerialNumber_MandatorySerialNumber()
		{
			var helper = new TWWhsDataTestHelper(Factory);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = helper.Importer.PK;
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_Style = "CP";
			entryInstruction.CEI_OH_Owner = helper.Owner.PK;
			entryInstruction.CEI_OA_Warehouse = helper.Warehouse.MainAddress.PK;
			helper.Owner.MiscServ.OM_IMUseSerialNumber = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_NewOwnerPartNo = helper.OwnerPart.OP_PartNum;
			helper.OwnerPart.RelatedOrganisations[0].OU_UseSerialNumber = true;

			invoiceLine.Validation.ValidateJI_NewSerialNumber();
			AssertHasError(invoiceLine.JI_NewSerialNumberInfo, "Please enter a Serial Number.");

			invoiceLine.JI_NewSerialNumber = "ABC";
			invoiceLine.Validation.ValidateJI_NewSerialNumber();
			AssertNoErrors(invoiceLine.JI_NewSerialNumberInfo);
		}

		#endregion

		public void TestCheckJI_CustomsSupplierPartNo()
		{
			var entryInstruction = Declaration.CusEntryInstruction;
			InvoiceLine.JI_CEI = entryInstruction.PK;
			CombineAssertions(() =>
			{
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.D1;
				InvoiceLine.JI_CustomsSupplierPartNo = ZString.Empty;
				AssertNoMessageErrorContaining(InvoiceLine.JI_CustomsSupplierPartNoInfo, ValidationConstants.InvoiceLine.CustomsSupplierPartNoShouldNotBeEmpty);
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.D5;
				InvoiceLine.JI_CustomsSupplierPartNo = ZString.Empty;
				AssertHasMessageErrorContaining(InvoiceLine.JI_CustomsSupplierPartNoInfo, ValidationConstants.InvoiceLine.CustomsSupplierPartNoShouldNotBeEmpty);
				InvoiceLine.JI_CustomsSupplierPartNo = "AAA123";
				AssertNoMessageErrorContaining(InvoiceLine.JI_CustomsSupplierPartNoInfo, ValidationConstants.InvoiceLine.CustomsSupplierPartNoShouldNotBeEmpty);

				InvoiceLine.JI_CustomsSupplierPartNo = ZString.Empty;
				InvoiceLine.JI_BondedGoodsCode = BondedGoodsCodeList.Codes.CN;
				AssertHasMessageErrorContaining(InvoiceLine.JI_CustomsSupplierPartNoInfo, ValidationConstants.InvoiceLine.CustomsSupplierPartNoShouldNotBeEmpty);

				InvoiceLine.JI_BondedGoodsCode = BondedGoodsCodeList.Codes.YB;
				AssertHasMessageErrorContaining(InvoiceLine.JI_CustomsSupplierPartNoInfo, ValidationConstants.InvoiceLine.CustomsSupplierPartNoShouldNotBeEmpty);

				InvoiceLine.JI_BondedGoodsCode = BondedGoodsCodeList.Codes.NB;
				AssertNoMessageErrorContaining(InvoiceLine.JI_CustomsSupplierPartNoInfo, ValidationConstants.InvoiceLine.CustomsSupplierPartNoShouldNotBeEmpty);

				entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.F4;
				InvoiceLine.Validation.ValidateJI_CustomsSupplierPartNo();
				AssertNoMessageErrorContaining(InvoiceLine.JI_CustomsSupplierPartNoInfo, ValidationConstants.InvoiceLine.CustomsSupplierPartNoShouldNotBeEmpty);
			});

			var header = new TestTWCreator(Factory).CreateOrganizationForJobDocAddress();
			var address = header.MainAddress;
			var jobDocAddress = Declaration.SupplierDocumentaryAddress;
			jobDocAddress.OrganisationPK = header.PK;
			jobDocAddress.E2_OA_Address = address.PK;
			CombineAssertions(() =>
			{
				address.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(MasterFiles.Business.OrgCusCode.TaiwanCodeTypes.FTZ, "FTZ001", Core.Constants.CountryCodes.Taiwan);
				Assert(jobDocAddress.IsFreeTradeZone);

				Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D8;
				InvoiceLine.JI_CustomsSupplierPartNo = ZString.Empty;

				AssertHasMessageErrorContaining(InvoiceLine.JI_CustomsSupplierPartNoInfo, ValidationConstants.InvoiceLine.CustomsSupplierPartNoIsRequiredWhenSupplierIsFTZ);

				InvoiceLine.JI_CustomsSupplierPartNo = "AAA123";
				AssertNoMessageErrorContaining(InvoiceLine.JI_CustomsSupplierPartNoInfo, ValidationConstants.InvoiceLine.CustomsSupplierPartNoIsRequiredWhenSupplierIsFTZ);

				entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.B6;
				InvoiceLine.JI_CustomsSupplierPartNo = ZString.Empty;
				AssertHasMessageErrorContaining(InvoiceLine.JI_CustomsSupplierPartNoInfo, ValidationConstants.InvoiceLine.CustomsSupplierPartNoIsRequiredWhenSupplierIsFTZ);

				InvoiceLine.JI_CustomsSupplierPartNo = "AAA123";
				AssertNoMessageErrorContaining(InvoiceLine.JI_CustomsSupplierPartNoInfo, ValidationConstants.InvoiceLine.CustomsSupplierPartNoIsRequiredWhenSupplierIsFTZ);

				Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.F4;
				InvoiceLine.JI_CustomsSupplierPartNo = ZString.Empty;
				AssertHasMessageErrorContaining(InvoiceLine.JI_CustomsSupplierPartNoInfo, ValidationConstants.InvoiceLine.CustomsSupplierPartNoIsRequiredWhenSupplierIsFTZ);

				InvoiceLine.JI_CustomsSupplierPartNo = "AAA123";
				AssertNoMessageErrorContaining(InvoiceLine.JI_CustomsSupplierPartNoInfo, ValidationConstants.InvoiceLine.CustomsSupplierPartNoIsRequiredWhenSupplierIsFTZ);
			});
		}

		public void TestCheckJI_CustomsOwnerPartNo()
		{
			var entryInstruction = Declaration.CusEntryInstruction;
			InvoiceLine.JI_CEI = entryInstruction.PK;
			CombineAssertions(() =>
			{
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.D5;
				InvoiceLine.JI_CustomsOwnerPartNo = ZString.Empty;
				AssertNoMessageErrorContaining(InvoiceLine.JI_CustomsOwnerPartNoInfo, ValidationConstants.InvoiceLine.CustomsOwnerPartNoShouldNotBeEmptyForDeclarationType);
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.D1;
				InvoiceLine.JI_CustomsOwnerPartNo = ZString.Empty;
				AssertHasMessageErrorContaining(InvoiceLine.JI_CustomsOwnerPartNoInfo, ValidationConstants.InvoiceLine.CustomsOwnerPartNoShouldNotBeEmptyForDeclarationType);
				InvoiceLine.JI_CustomsOwnerPartNo = "AAA123";
				AssertNoMessageErrorContaining(InvoiceLine.JI_CustomsOwnerPartNoInfo, ValidationConstants.InvoiceLine.CustomsOwnerPartNoShouldNotBeEmptyForDeclarationType);
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.B6;
				InvoiceLine.JI_CustomsOwnerPartNo = ZString.Empty;
				AssertNoMessageErrorContaining(InvoiceLine.JI_CustomsOwnerPartNoInfo, ValidationConstants.InvoiceLine.CustomsOwnerPartNoShouldNotBeEmptyForDeclarationType);
			});

			var header = new TestTWCreator(Factory).CreateOrganizationForJobDocAddress();
			var address = header.MainAddress;
			var jobDocAddress = Declaration.ImporterDocumentaryAddress;
			jobDocAddress.OrganisationPK = header.PK;
			jobDocAddress.E2_OA_Address = address.PK;
			CombineAssertions(() =>
			{
				address.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(MasterFiles.Business.OrgCusCode.TaiwanCodeTypes.FTZ, "FTZ001", Core.Constants.CountryCodes.Taiwan);
				Assert(jobDocAddress.IsFreeTradeZone);

				Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.D5;
				InvoiceLine.JI_CustomsOwnerPartNo = ZString.Empty;
				AssertHasMessageErrorContaining(InvoiceLine.JI_CustomsOwnerPartNoInfo, ValidationConstants.InvoiceLine.CustomsOwnerPartNoIsRequiredWhenImporterIsFTZ);

				InvoiceLine.JI_CustomsOwnerPartNo = "AAA123";
				AssertNoMessageErrorContaining(InvoiceLine.JI_CustomsOwnerPartNoInfo, ValidationConstants.InvoiceLine.CustomsOwnerPartNoIsRequiredWhenImporterIsFTZ);

				entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B8;
				InvoiceLine.JI_CustomsOwnerPartNo = ZString.Empty;
				AssertHasMessageErrorContaining(InvoiceLine.JI_CustomsOwnerPartNoInfo, ValidationConstants.InvoiceLine.CustomsOwnerPartNoIsRequiredWhenImporterIsFTZ);

				InvoiceLine.JI_CustomsOwnerPartNo = "AAA123";
				AssertNoMessageErrorContaining(InvoiceLine.JI_CustomsOwnerPartNoInfo, ValidationConstants.InvoiceLine.CustomsOwnerPartNoIsRequiredWhenImporterIsFTZ);

				entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B9;
				InvoiceLine.JI_CustomsOwnerPartNo = ZString.Empty;
				AssertHasMessageErrorContaining(InvoiceLine.JI_CustomsOwnerPartNoInfo, ValidationConstants.InvoiceLine.CustomsOwnerPartNoIsRequiredWhenImporterIsFTZ);

				InvoiceLine.JI_CustomsOwnerPartNo = "AAA123";
				AssertNoMessageErrorContaining(InvoiceLine.JI_CustomsOwnerPartNoInfo, ValidationConstants.InvoiceLine.CustomsOwnerPartNoIsRequiredWhenImporterIsFTZ);

				entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.F4;
				InvoiceLine.JI_CustomsOwnerPartNo = ZString.Empty;
				AssertHasMessageErrorContaining(InvoiceLine.JI_CustomsOwnerPartNoInfo, ValidationConstants.InvoiceLine.CustomsOwnerPartNoIsRequiredWhenImporterIsFTZ);

				InvoiceLine.JI_CustomsOwnerPartNo = "AAA123";
				AssertNoMessageErrorContaining(InvoiceLine.JI_CustomsOwnerPartNoInfo, ValidationConstants.InvoiceLine.CustomsOwnerPartNoIsRequiredWhenImporterIsFTZ);
			});
		}

		public void TestCheckJI_CustomsOwnerPartNo_JI_Procedure()
		{
			CombineAssertions(() =>
			{
				InvoiceLine.JI_CustomsOwnerPartNo = ZString.Empty;
				InvoiceLine.JI_Procedure = Constants.ProcedureCodes._35;
				AssertNoMessageErrorContaining(InvoiceLine.JI_CustomsOwnerPartNoInfo, ValidationConstants.InvoiceLine.CustomsOwnerPartNoShouldNotBeEmptyForProcedure);
				InvoiceLine.JI_Procedure = Constants.ProcedureCodes._56;
				AssertHasMessageErrorContaining(InvoiceLine.JI_CustomsOwnerPartNoInfo, ValidationConstants.InvoiceLine.CustomsOwnerPartNoShouldNotBeEmptyForProcedure);
				InvoiceLine.JI_CustomsOwnerPartNo = "AAA123";
				AssertNoMessageErrorContaining(InvoiceLine.JI_CustomsOwnerPartNoInfo, ValidationConstants.InvoiceLine.CustomsOwnerPartNoShouldNotBeEmptyForProcedure);

				InvoiceLine.JI_Procedure = Constants.ProcedureCodes._5Y;
				InvoiceLine.JI_CustomsOwnerPartNo = ZString.Empty;
				AssertHasMessageErrorContaining(InvoiceLine.JI_CustomsOwnerPartNoInfo, ValidationConstants.InvoiceLine.CustomsOwnerPartNoShouldNotBeEmptyForProcedure);
				InvoiceLine.JI_CustomsOwnerPartNo = "AAA123";
				AssertNoMessageErrorContaining(InvoiceLine.JI_CustomsOwnerPartNoInfo, ValidationConstants.InvoiceLine.CustomsOwnerPartNoShouldNotBeEmptyForProcedure);

				InvoiceLine.JI_Procedure = Constants.ProcedureCodes._58;
				InvoiceLine.JI_CustomsOwnerPartNo = ZString.Empty;
				AssertHasMessageErrorContaining(InvoiceLine.JI_CustomsOwnerPartNoInfo, ValidationConstants.InvoiceLine.CustomsOwnerPartNoShouldNotBeEmptyForProcedure);
				InvoiceLine.JI_CustomsOwnerPartNo = "AAA123";
				AssertNoMessageErrorContaining(InvoiceLine.JI_CustomsOwnerPartNoInfo, ValidationConstants.InvoiceLine.CustomsOwnerPartNoShouldNotBeEmptyForProcedure);

				InvoiceLine.JI_Procedure = Constants.ProcedureCodes._5C;
				InvoiceLine.JI_CustomsOwnerPartNo = ZString.Empty;
				AssertHasMessageErrorContaining(InvoiceLine.JI_CustomsOwnerPartNoInfo, ValidationConstants.InvoiceLine.CustomsOwnerPartNoShouldNotBeEmptyForProcedure);
				InvoiceLine.JI_CustomsOwnerPartNo = "AAA123";
				AssertNoMessageErrorContaining(InvoiceLine.JI_CustomsOwnerPartNoInfo, ValidationConstants.InvoiceLine.CustomsOwnerPartNoShouldNotBeEmptyForProcedure);

				InvoiceLine.JI_Procedure = Constants.ProcedureCodes._98;
				InvoiceLine.JI_CustomsOwnerPartNo = ZString.Empty;
				AssertHasMessageErrorContaining(InvoiceLine.JI_CustomsOwnerPartNoInfo, ValidationConstants.InvoiceLine.CustomsOwnerPartNoShouldNotBeEmptyForProcedure);
				InvoiceLine.JI_CustomsOwnerPartNo = "AAA123";
				AssertNoMessageErrorContaining(InvoiceLine.JI_CustomsOwnerPartNoInfo, ValidationConstants.InvoiceLine.CustomsOwnerPartNoShouldNotBeEmptyForProcedure);

				InvoiceLine.JI_Procedure = Constants.ProcedureCodes._99;
				InvoiceLine.JI_CustomsOwnerPartNo = ZString.Empty;
				AssertHasMessageErrorContaining(InvoiceLine.JI_CustomsOwnerPartNoInfo, ValidationConstants.InvoiceLine.CustomsOwnerPartNoShouldNotBeEmptyForProcedure);
				InvoiceLine.JI_CustomsOwnerPartNo = "AAA123";
				AssertNoMessageErrorContaining(InvoiceLine.JI_CustomsOwnerPartNoInfo, ValidationConstants.InvoiceLine.CustomsOwnerPartNoShouldNotBeEmptyForProcedure);
			});
		}

		public virtual void TestCheckJI_InnerPackType()
		{
			var targetInfo = InvoiceLine.JI_InnerPackTypeInfo;
			TestCheckJI_InnerPackTypeMandatory(targetInfo);
			TestCheckJI_InnerPackTypeLinkedToSameNX301_DNMessage(targetInfo);
		}

		void TestCheckJI_InnerPackTypeMandatory(ZPropertyInfo targetInfo)
		{
			ControllingMsgHeaderTestHelper.AddControllingAgencysTo(EntryInstruction.ControllingMessageHeaders, new string[] { "20", "CI", "2Q", "CD", "IF", "DH", "VP", "DN" });

			var notEmptyValue = new ZString("1");
			ControllingMsgHeaderTestHelper.AssertHasMessageErrorContainingByControllingAgency(InvoiceLine, targetInfo, notEmptyValue, ZString.Empty, "DN", MandatoryValidation.YouHaveNotEntered);
			ControllingMsgHeaderTestHelper.AssertHasMessageErrorContainingByControllingAgency(InvoiceLine, targetInfo, notEmptyValue, ZString.Empty, "IF", MandatoryValidation.YouHaveNotEntered);
			ControllingMsgHeaderTestHelper.AssertHasMessageErrorContainingByControllingAgency(InvoiceLine, targetInfo, notEmptyValue, ZString.Empty, "DH", MandatoryValidation.YouHaveNotEntered);
		}

		void TestCheckJI_InnerPackTypeLinkedToSameNX301_DNMessage(ZPropertyInfo targetInfo)
		{
			var controllingMessageHeader = EntryInstruction.ControllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = "NX301_DN";
			var waringMessage = "Invoice lines of different Packaging Type cannot be linked to the same NX301_DN message.";
			var invoiceLine2 = Invoice.JobComInvoiceLines.AddNew();
			var invoiceLine3 = Invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = EntryInstruction.PK;
			invoiceLine3.JI_CEI = EntryInstruction.PK;
			invoiceLine2.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(c => c.ControllingMessageHeader.IsNX301_DN).IsLinkedCMHeader = true;
			InvoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(c => c.ControllingMessageHeader.IsNX301_DN).IsLinkedCMHeader = false;
			invoiceLine3.JI_InnerPackType = CPT_114_InnerPackingMaterialList.Codes._3;
			invoiceLine2.JI_InnerPackType = CPT_114_InnerPackingMaterialList.Codes._1;
			InvoiceLine.JI_InnerPackType = CPT_114_InnerPackingMaterialList.Codes._2;
			AssertNoWarning("Invoice lines of different Packaging Type, but do not linked to the NX301_DN message.", targetInfo, waringMessage);
			InvoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(c => c.ControllingMessageHeader.IsNX301_DN).IsLinkedCMHeader = true;
			InvoiceLine.JI_InnerPackType = CPT_114_InnerPackingMaterialList.Codes._3;
			AssertHasWarning("Invoice lines of different Packaging Type linked to the same NX301_DN message.", targetInfo, waringMessage);
			InvoiceLine.JI_InnerPackType = CPT_114_InnerPackingMaterialList.Codes._1;
			AssertNoWarning("Invoice lines of same Packaging Type linked to the same NX301_DN message.", targetInfo, waringMessage);
		}

		public virtual void TestCheckJI_InnerPackingMaterial()
		{
			var targetInfo = InvoiceLine.JI_InnerPackingMaterialInfo;
			TestCheckJI_InnerPackingMaterialMandatory(targetInfo);
			TestCheckJI_InnerPackingMaterialLinkedToSameNX301_DNMessage(targetInfo);
		}

		void TestCheckJI_InnerPackingMaterialMandatory(ZPropertyInfo targetInfo)
		{
			var entryInstruction = Declaration.CusEntryInstruction;
			InvoiceLine.JI_CEI = entryInstruction.PK;
			ControllingMsgHeaderTestHelper.AddControllingAgencysTo(entryInstruction.ControllingMessageHeaders, new string[] { "20", "CI", "2Q", "CD", "IF", "DH", "VP", "DN" });

			var notEmptyValue = new ZString("1");
			ControllingMsgHeaderTestHelper.AssertHasMessageErrorContainingByControllingAgency(InvoiceLine, targetInfo, notEmptyValue, ZString.Empty, "DN", MandatoryValidation.YouHaveNotEntered);
			ControllingMsgHeaderTestHelper.AssertHasMessageErrorContainingByControllingAgency(InvoiceLine, targetInfo, notEmptyValue, ZString.Empty, "IF", MandatoryValidation.YouHaveNotEntered);
			ControllingMsgHeaderTestHelper.AssertHasMessageErrorContainingByControllingAgency(InvoiceLine, targetInfo, notEmptyValue, ZString.Empty, "DH", MandatoryValidation.YouHaveNotEntered);
		}

		void TestCheckJI_InnerPackingMaterialLinkedToSameNX301_DNMessage(ZPropertyInfo targetInfo)
		{
			var controllingMessageHeader = EntryInstruction.ControllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = "NX301_DN";
			var waringMessage = "Invoice lines of different Packaging Material cannot be linked to the same NX301_DN message.";
			var invoiceLine2 = Invoice.JobComInvoiceLines.AddNew();
			var invoiceLine3 = Invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = EntryInstruction.PK;
			invoiceLine3.JI_CEI = EntryInstruction.PK;
			invoiceLine2.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(c => c.ControllingMessageHeader.IsNX301_DN).IsLinkedCMHeader = true;
			InvoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(c => c.ControllingMessageHeader.IsNX301_DN).IsLinkedCMHeader = false;
			invoiceLine3.JI_InnerPackingMaterial = CPT_114_InnerPackingMaterialList.Codes._3;
			invoiceLine2.JI_InnerPackingMaterial = CPT_114_InnerPackingMaterialList.Codes._1;
			InvoiceLine.JI_InnerPackingMaterial = CPT_114_InnerPackingMaterialList.Codes._2;
			AssertNoWarning("Invoice lines of different Packaging Material, but do not linked to the NX301_DN message.", targetInfo, waringMessage);
			InvoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(c => c.ControllingMessageHeader.IsNX301_DN).IsLinkedCMHeader = true;
			InvoiceLine.JI_InnerPackingMaterial = CPT_114_InnerPackingMaterialList.Codes._3;
			AssertHasWarning("Invoice lines of different Packaging Material linked to the same NX301_DN message.", targetInfo, waringMessage);
			InvoiceLine.JI_InnerPackingMaterial = CPT_114_InnerPackingMaterialList.Codes._1;
			AssertNoWarning("Invoice lines of same Packaging Material linked to the same NX301_DN message.", targetInfo, waringMessage);
		}

		void CheckPartAttributeValidation(ZPropertyInfo attributeInfo, Action validate, OrgHeader owner, OrgSupplierPart part, int attributeNo)
		{
			using (new PartAttributeValidationChecker.AttributeCallChecker(owner, part, attributeInfo, attributeNo))
			{
				validate();
			}
		}

		public void TestCheckJI_MicrochipID()
		{
			InvoiceLine.JI_MicrochipID = "!XXX";
			AssertHasMessageError(InvoiceLine.JI_MicrochipIDInfo, ValidationConstants.InvoiceLine.InvalidValue(InvoiceLine.JI_MicrochipIDInfo.HumanReadableName));

			InvoiceLine.JI_MicrochipID = "1X23";
			AssertNoMessageErrors(InvoiceLine.JI_MicrochipIDInfo);
		}

		public void TestCheckJI_AnimalMaleQty()
		{
			var info = InvoiceLine.JI_AnimalMaleQtyInfo;
			AssertNumberBetweenMinValueAndMaxValue(info, 0, 999999);
			InvoiceLine.JI_AnimalMaleQty = 666666;
			AssertNoMessageErrors(info);
		}

		public void TestCheckJI_AnimalFemaleQty()
		{
			var info = InvoiceLine.JI_AnimalFemaleQtyInfo;
			AssertNumberBetweenMinValueAndMaxValue(info, 0, 999999);
			InvoiceLine.JI_AnimalFemaleQty = 666666;
			AssertNoMessageErrors(info);
		}

		public void TestCheckJI_AnimalAgeMonth()
		{
			var info = InvoiceLine.JI_AnimalAgeMonthInfo;
			AssertNumberBetweenMinValueAndMaxValue(info, 0, 12);
			InvoiceLine.JI_AnimalAgeMonth = 6;
			AssertNoMessageErrors(info);
		}

		public void TestCheckJI_AnimalAgeYear()
		{
			var info = InvoiceLine.JI_AnimalAgeYearInfo;
			AssertNumberBetweenMinValueAndMaxValue(info, 0, 9999);
			InvoiceLine.JI_AnimalAgeYear = 6666;
			AssertNoMessageErrors(info);
		}

		protected void AssertNumberBetweenMinValueAndMaxValue(ZPropertyInfo info, ZInt minValue, ZInt maxValue)
		{
			var messageErrorInvalidValue = ValidationConstants.InvoiceLine.InvalidValue(info.HumanReadableName);
			info.Value = new ZInt(minValue - 1);
			AssertHasMessageError(info, messageErrorInvalidValue);
			info.Value = minValue;
			AssertNoMessageError(info, messageErrorInvalidValue);
			info.Value = new ZInt(maxValue + 1);
			AssertHasMessageError(info, messageErrorInvalidValue);
			info.Value = maxValue;
			AssertNoMessageError(info, messageErrorInvalidValue);
		}

		public void TestCheckJI_AlcoholPercentageWhenSelectAnAlcoholTaxRate()
		{
			var errorMessage = "You have selected an alcohol tax rate that requires alcohol by volume information.";

			var universalTestHelper = new UniversalReferenceTestDataHelper(Factory);
			var atTariffType = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "AT");
			var rateType = universalTestHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Taiwan, "DTY");
			var rateCode = universalTestHelper.LoadOrCreateNewCusRateCode(Factory, "TAT", rateType.PK);
			var tradeGroupAllCountry = universalTestHelper.LoadOrCreateTradeGroup("TW", "ALL", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			var alcoholChildtariff1 = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, atTariffType.PK, "REPROCESSED1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.CreateTariffRelationship(alcoholChildtariff1.PK, atTariffType.PK, "21039090201");
			var rate1 = universalTestHelper.CreateRate(alcoholChildtariff1, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "If(AlcoholPercentage > 20, 185 * [LTR], 7 * [LTR] * AlcoholPercentage)");
			universalTestHelper.CreateCusApplicability(rate1, tradeGroupAllCountry, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var alcoholChildtariff2 = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, atTariffType.PK, "REPROCESSED2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.CreateTariffRelationship(alcoholChildtariff2.PK, atTariffType.PK, "21039090202");
			var rate2 = universalTestHelper.CreateRate(alcoholChildtariff2, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "185 * [LTR]");
			universalTestHelper.CreateCusApplicability(rate2, tradeGroupAllCountry, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			atTariffType.ZZI_Description = "Alcohol Tax";
			Factory.Save();

			InvoiceLine.JI_CountryOfOrigin = "US";
			var invoiceLineTax = InvoiceLine.Taxes.AddNew();
			invoiceLineTax.JLT_Type = "AT";
			invoiceLineTax.JLT_Tariff = "REPROCESSED1";
			AssertEquals("If(AlcoholPercentage > 20, 185 * [LTR], 7 * [LTR] * AlcoholPercentage)", invoiceLineTax.FormattedTariffRate);

			InvoiceLine.JI_AlcoholPercentage = 0;
			AssertHasMessageError(InvoiceLine.JI_AlcoholPercentageInfo, errorMessage);

			InvoiceLine.JI_AlcoholPercentage = 1;
			AssertNoMessageError(InvoiceLine.JI_AlcoholPercentageInfo, errorMessage);

			InvoiceLine.JI_AlcoholPercentage = -1;
			AssertNoMessageError(InvoiceLine.JI_AlcoholPercentageInfo, errorMessage);

			invoiceLineTax.JLT_Tariff = "REPROCESSED2";
			AssertEquals("185 * [LTR]", invoiceLineTax.FormattedTariffRate);

			InvoiceLine.JI_AlcoholPercentage = 0;
			AssertNoMessageError(InvoiceLine.JI_AlcoholPercentageInfo, errorMessage);

			InvoiceLine.JI_AlcoholPercentage = 1;
			AssertNoMessageError(InvoiceLine.JI_AlcoholPercentageInfo, errorMessage);

			InvoiceLine.JI_AlcoholPercentage = -1;
			AssertNoMessageError(InvoiceLine.JI_AlcoholPercentageInfo, errorMessage);
		}

		public virtual void TestCheckJI_GoodsType()
		{
			var targetInfo = InvoiceLine.JI_GoodsTypeInfo;
			var controllingMessageHeader = EntryInstruction.ControllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_DN;
			TestCheckJI_GoodsTypeMandatory(targetInfo);
			TestCheckJI_GoodsTypeLinkedToSameNX301_DNMessage(targetInfo);

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			TestCheckJI_GoodsTypeMandatory(targetInfo);
		}

		void TestCheckJI_GoodsTypeMandatory(ZPropertyInfo targetInfo)
		{
			Declaration.JE_MessageType = "EXP";
			InvoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = false;
			InvoiceLine.JI_GoodsType = "100";
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.JI_GoodsType = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			InvoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;
			InvoiceLine.JI_GoodsType = "100";
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.JI_GoodsType = ZString.Empty;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			Declaration.JE_MessageType = "IMP";
			InvoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = false;
			InvoiceLine.JI_GoodsType = "100";
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.JI_GoodsType = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			InvoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;
			InvoiceLine.JI_GoodsType = "100";
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.JI_GoodsType = ZString.Empty;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		void TestCheckJI_GoodsTypeLinkedToSameNX301_DNMessage(ZPropertyInfo targetInfo)
		{
			var waringMessage = "Invoice lines of different Goods Type cannot be linked to the same NX301_DN message.";
			var invoiceLine2 = Invoice.JobComInvoiceLines.AddNew();
			var invoiceLine3 = Invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = EntryInstruction.PK;
			invoiceLine3.JI_CEI = EntryInstruction.PK;
			invoiceLine2.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;
			InvoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = false;
			invoiceLine3.JI_GoodsType = CPT_107_301_GoodsTypeList.Codes._300;
			invoiceLine2.JI_GoodsType = CPT_107_301_GoodsTypeList.Codes._100;
			InvoiceLine.JI_GoodsType = CPT_107_301_GoodsTypeList.Codes._201;
			AssertNoWarning("Invoice lines of different Goods Type, but do not linked to the NX301_DN message.", targetInfo, waringMessage);
			InvoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;
			InvoiceLine.JI_GoodsType = CPT_107_301_GoodsTypeList.Codes._505;
			AssertHasWarning("Invoice lines of different Goods Type linked to the same NX301_DN message.", targetInfo, waringMessage);
			InvoiceLine.JI_GoodsType = CPT_107_301_GoodsTypeList.Codes._100;
			AssertNoWarning("Invoice lines of same Goods Type linked to the same NX301_DN message.", targetInfo, waringMessage);
		}

		public void TestCheckJI_PermitQty()
		{
			CombineAssertions(() =>
			{
				var info = InvoiceLine.JI_PermitQtyInfo;
				var controllingMessageHeader = EntryInstruction.ControllingMessageHeaders.AddNew();
				controllingMessageHeader.TW1_ControllingMessageType = "NX101";
				InvoiceLine.Validation.ValidateJI_PermitQty();
				AssertNoWarningContaining("Should not do validation when not linked", info, MandatoryValidation.YouHaveNotEntered);
				InvoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(x => x.MessageType == "NX101").IsLinkedCMHeader = true;
				InvoiceLine.Validation.ValidateJI_PermitQty();
				AssertHasWarningContaining(info, MandatoryValidation.YouHaveNotEntered);

				InvoiceLine.JI_PermitQty = 1d;
				AssertNoWarningContaining(info, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJI_PermitUQ()
		{
			CombineAssertions(() =>
			{
				var message = "You have not selected a Permit Quantity UQ.";
				var info = InvoiceLine.JI_PermitUQInfo;
				var controllingMessageHeader = EntryInstruction.ControllingMessageHeaders.AddNew();
				controllingMessageHeader.TW1_ControllingMessageType = "NX101";
				controllingMessageHeader.TW1_CertificateType = "15";
				InvoiceLine.Validation.ValidateJI_PermitUQ();
				AssertNoWarningContaining("Should not do validation when not linked", info, message);
				InvoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(x => x.MessageType == "NX101").IsLinkedCMHeader = true;
				InvoiceLine.Validation.ValidateJI_PermitUQ();
				AssertHasWarningContaining(info, message);

				InvoiceLine.JI_PermitUQ = "aa";
				AssertNoWarningContaining(info, message);
			});
		}

		public void TestCheckJI_OriginCriteria()
		{
			CombineAssertions(() =>
			{
				var info = InvoiceLine.JI_OriginCriteriaInfo;
				var controllingMessageHeader = EntryInstruction.ControllingMessageHeaders.AddNew();
				controllingMessageHeader.TW1_ControllingMessageType = "NX101";
				InvoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;
				controllingMessageHeader.TW1_CertificateType = "01";
				InvoiceLine.Validation.ValidateJI_OriginCriteria();
				AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "07";
				InvoiceLine.Validation.ValidateJI_OriginCriteria();
				AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "10";
				InvoiceLine.Validation.ValidateJI_OriginCriteria();
				AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "11";
				InvoiceLine.Validation.ValidateJI_OriginCriteria();
				AssertNoMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "10";
				InvoiceLine.JI_OriginCriteria = "xx";
				AssertNoMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);

				InvoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = false;
				controllingMessageHeader.TW1_CertificateType = "01";
				InvoiceLine.JI_OriginCriteria = ZString.Empty;
				AssertNoMessageErrorContaining("Should not do validation when not linked, TW1_CertificateType = 01", info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "07";
				InvoiceLine.Validation.ValidateJI_OriginCriteria();
				AssertNoMessageErrorContaining("Should not do validation when not linked, TW1_CertificateType = 07", info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "10";
				InvoiceLine.Validation.ValidateJI_OriginCriteria();
				AssertNoMessageErrorContaining("Should not do validation when not linked, TW1_CertificateType = 10", info, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJI_PTCriteria()
		{
			CombineAssertions(() =>
			{
				var info = InvoiceLine.JI_PTCriteriaInfo;
				var controllingMessageHeader = EntryInstruction.ControllingMessageHeaders.AddNew();
				controllingMessageHeader.TW1_ControllingMessageType = "NX101";
				controllingMessageHeader.TW1_CertificateType = "09";
				InvoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;
				InvoiceLine.Validation.ValidateJI_PTCriteria();
				AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "11";
				InvoiceLine.Validation.ValidateJI_PTCriteria();
				AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "13";
				InvoiceLine.Validation.ValidateJI_PTCriteria();
				AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "14";
				InvoiceLine.Validation.ValidateJI_PTCriteria();
				AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "15";
				InvoiceLine.Validation.ValidateJI_PTCriteria();
				AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "19";
				InvoiceLine.Validation.ValidateJI_PTCriteria();
				AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "16";
				InvoiceLine.Validation.ValidateJI_PTCriteria();
				AssertNoMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "15";
				InvoiceLine.JI_PTCriteria = "xx";
				AssertNoMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);

				InvoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = false;
				InvoiceLine.JI_PTCriteria = ZString.Empty;
				AssertNoMessageErrorContaining("Should not do validation when not linked", info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "11";
				InvoiceLine.Validation.ValidateJI_PTCriteria();
				AssertNoMessageErrorContaining("Should not do validation when not linked, TW1_CertificateType = 11", info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "13";
				InvoiceLine.Validation.ValidateJI_PTCriteria();
				AssertNoMessageErrorContaining("Should not do validation when not linked, TW1_CertificateType = 13", info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "14";
				InvoiceLine.Validation.ValidateJI_PTCriteria();
				AssertNoMessageErrorContaining("Should not do validation when not linked, TW1_CertificateType = 14", info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "15";
				InvoiceLine.Validation.ValidateJI_PTCriteria();
				AssertNoMessageErrorContaining("Should not do validation when not linked, TW1_CertificateType = 15", info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "19";
				InvoiceLine.Validation.ValidateJI_PTCriteria();
				AssertNoMessageErrorContaining("Should not do validation when not linked, TW1_CertificateType = 19", info, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJI_PTCriteria2()
		{
			CombineAssertions(() =>
			{
				var info = InvoiceLine.JI_PTCriteria2Info;
				var controllingMessageHeader = EntryInstruction.ControllingMessageHeaders.AddNew();
				controllingMessageHeader.TW1_ControllingMessageType = "NX101";
				controllingMessageHeader.TW1_CertificateType = "09";
				InvoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;
				InvoiceLine.Validation.ValidateJI_PTCriteria2();
				AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "11";
				InvoiceLine.Validation.ValidateJI_PTCriteria2();
				AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "13";
				InvoiceLine.Validation.ValidateJI_PTCriteria2();
				AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "14";
				InvoiceLine.Validation.ValidateJI_PTCriteria2();
				AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "19";
				InvoiceLine.Validation.ValidateJI_PTCriteria2();
				AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "15";
				InvoiceLine.Validation.ValidateJI_PTCriteria2();
				AssertNoMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "14";
				InvoiceLine.JI_PTCriteria2 = "xx";
				AssertNoMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);

				InvoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = false;
				InvoiceLine.JI_PTCriteria2 = ZString.Empty;
				AssertNoMessageErrorContaining("Should not do validation when not linked", info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "11";
				InvoiceLine.Validation.ValidateJI_PTCriteria2();
				AssertNoMessageErrorContaining("Should not do validation when not linked, TW1_CertificateType = 11", info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "13";
				InvoiceLine.Validation.ValidateJI_PTCriteria2();
				AssertNoMessageErrorContaining("Should not do validation when not linked, TW1_CertificateType = 13", info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "14";
				InvoiceLine.Validation.ValidateJI_PTCriteria2();
				AssertNoMessageErrorContaining("Should not do validation when not linked, TW1_CertificateType = 14", info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "19";
				InvoiceLine.Validation.ValidateJI_PTCriteria2();
				AssertNoMessageErrorContaining("Should not do validation when not linked, TW1_CertificateType = 19", info, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJI_ManufacturerRelationship()
		{
			CombineAssertions(() =>
			{
				var info = InvoiceLine.JI_ManufacturerRelationshipInfo;
				var controllingMessageHeader = EntryInstruction.ControllingMessageHeaders.AddNew();
				controllingMessageHeader.TW1_ControllingMessageType = "NX101";
				controllingMessageHeader.TW1_CertificateType = "09";
				InvoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;
				InvoiceLine.Validation.ValidateJI_ManufacturerRelationship();
				AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "11";
				InvoiceLine.Validation.ValidateJI_ManufacturerRelationship();
				AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "13";
				InvoiceLine.Validation.ValidateJI_ManufacturerRelationship();
				AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "14";
				InvoiceLine.Validation.ValidateJI_ManufacturerRelationship();
				AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "19";
				InvoiceLine.Validation.ValidateJI_ManufacturerRelationship();
				AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "15";
				InvoiceLine.Validation.ValidateJI_ManufacturerRelationship();
				AssertNoMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "14";
				InvoiceLine.JI_ManufacturerRelationship = "xx";
				AssertNoMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);

				InvoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = false;
				InvoiceLine.JI_ManufacturerRelationship = ZString.Empty;
				AssertNoMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "11";
				InvoiceLine.Validation.ValidateJI_ManufacturerRelationship();
				AssertNoMessageErrorContaining("Should not do validation when not linked, TW1_CertificateType = 11", info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "13";
				InvoiceLine.Validation.ValidateJI_ManufacturerRelationship();
				AssertNoMessageErrorContaining("Should not do validation when not linked, TW1_CertificateType = 13", info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "14";
				InvoiceLine.Validation.ValidateJI_ManufacturerRelationship();
				AssertNoMessageErrorContaining("Should not do validation when not linked, TW1_CertificateType = 14", info, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "19";
				InvoiceLine.Validation.ValidateJI_ManufacturerRelationship();
				AssertNoMessageErrorContaining("Should not do validation when not linked, TW1_CertificateType = 19", info, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJI_TariffPrintLength()
		{
			CombineAssertions(() =>
			{
				var info = InvoiceLine.JI_TariffPrintLengthInfo;
				var controllingMessageHeader = EntryInstruction.ControllingMessageHeaders.AddNew();
				controllingMessageHeader.TW1_ControllingMessageType = "NX101";
				InvoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;
				InvoiceLine.JI_TariffPrintLength = ZString.Empty;
				AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);
				InvoiceLine.JI_TariffPrintLength = "x";
				AssertNoMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);

				controllingMessageHeader.TW1_CertificateType = "14";
				InvoiceLine.JI_TariffPrintLength = "x";
				AssertNoMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);

				controllingMessageHeader.TW1_CertificateType = "15";
				InvoiceLine.JI_TariffPrintLength = "N";
				AssertHasMessageErrorContaining(info, "When Certificate Type is '15', Tariff Printing cannot be 'N'.");

				InvoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = false;
				InvoiceLine.JI_TariffPrintLength = ZString.Empty;
				AssertNoMessageErrorContaining("Should not do validation when not linked", info, MandatoryValidation.YouHaveNotEntered);
				InvoiceLine.JI_TariffPrintLength = "x";
				AssertNoMessageErrorContaining("Should not do validation when not linked", info, ListValidation.InvalidCodeMessageError.ToString());

				controllingMessageHeader.TW1_CertificateType = "14";
				InvoiceLine.JI_TariffPrintLength = ZString.Empty;
				AssertNoMessageErrorContaining("Should not do validation when not linked, TW1_CertificateType = 14", info, MandatoryValidation.YouHaveNotEntered);
				InvoiceLine.JI_TariffPrintLength = "x";
				AssertNoMessageErrorContaining("Should not do validation when not linked, TW1_CertificateType = 14", info, ListValidation.InvalidCodeMessageError.ToString());

				controllingMessageHeader.TW1_CertificateType = "15";
				InvoiceLine.JI_TariffPrintLength = "N";
				AssertNoMessageErrorContaining("Should not do validation when not linked, TW1_CertificateType = 15", info, "When Certificate Type is '15', Tariff Printing cannot be 'N'.");
			});
		}

		public void TestCheckJI_IMPTariff()
		{
			CombineAssertions(() =>
			{
				var msg = "Import Country's Tariff should be 8 characters long.";
				var info = InvoiceLine.JI_IMPTariffInfo;
				var controllingMessageHeader = EntryInstruction.ControllingMessageHeaders.AddNew();
				controllingMessageHeader.TW1_ControllingMessageType = "NX101";
				controllingMessageHeader.TW1_CertificateType = "15";
				InvoiceLine.JI_IMPTariff = "1234567";
				AssertNoMessageErrorContaining(info, msg);
				InvoiceLine.JI_IMPTariff = "12345678";
				AssertNoMessageErrorContaining(info, msg);
				InvoiceLine.JI_IMPTariff = "123456789";
				AssertNoMessageErrorContaining(info, msg);

				InvoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;
				InvoiceLine.JI_IMPTariff = "1234567";
				AssertHasMessageErrorContaining(info, msg);
				InvoiceLine.JI_IMPTariff = "12345678";
				AssertNoMessageErrorContaining(info, msg);
				InvoiceLine.JI_IMPTariff = "123456789";
				AssertHasMessageErrorContaining(info, msg);
			});
		}

		JobComInvoiceLine SetupInvoiceLineWithOwnerPart()
		{
			var helper = new TWWhsDataTestHelper(Factory);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = helper.Importer.PK;
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_Style = "CP";
			entryInstruction.CEI_OH_Owner = helper.Owner.PK;
			entryInstruction.CEI_OA_Warehouse = helper.Warehouse.MainAddress.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_NewOwnerPartNo = helper.OwnerPart.OP_PartNum;
			return invoiceLine;
		}
	}
}

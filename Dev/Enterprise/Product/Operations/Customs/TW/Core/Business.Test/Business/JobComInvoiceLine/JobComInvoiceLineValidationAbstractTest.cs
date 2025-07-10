using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.TW.Business.Testing.InvoiceLineLinkControllingMsgHeaderCollectionTest;

namespace Enterprise.Customs.TW.Business.Testing
{
	abstract partial class JobComInvoiceLineValidationAbstractTest<TJobComInvoiceLineValidation> : BusinessObjectValidationTestCase
		where TJobComInvoiceLineValidation : JobComInvoiceLineValidation
	{
		public void TestParent()
		{
			AssertEquals(InvoiceLine.Validation.Parent, InvoiceLine);
		}

		public void TestCheckJI_BrandNameWithImportOrExportRegulations581Or541()
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
			var line = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			line.JI_Tariff = tariff_26179090101.ZZ1_TariffCode;
			AssertHasMessageErrorContaining(line.JI_BrandNameInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoWarning(line.JI_BrandNameInfo, "Brand Name might be required when the goods are subject to Import Regulation: 602.");

			line.JI_Tariff = tariff_84716090208.ZZ1_TariffCode;
			AssertNoMessageErrorContaining(line.JI_BrandNameInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasWarning(line.JI_BrandNameInfo, "Brand Name might be required when the goods are subject to Import Regulation: 602.");

			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			line.JI_Tariff = tariff_26179090101.ZZ1_TariffCode;
			AssertHasMessageErrorContaining(line.JI_BrandNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_ModelWithImportOrExportRegulations581Or541()
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
			var line = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			line.JI_Tariff = tariff_26179090101.ZZ1_TariffCode;
			AssertHasMessageErrorContaining(line.JI_ModelInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoWarning(line.JI_ModelInfo, "Model might be required when the goods are subject to Import Regulation: 602.");

			line.JI_Tariff = tariff_84716090208.ZZ1_TariffCode;
			AssertNoMessageErrorContaining(line.JI_ModelInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasWarning(line.JI_ModelInfo, "Model might be required when the goods are subject to Import Regulation: 602.");

			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			line.JI_Tariff = tariff_26179090101.ZZ1_TariffCode;
			AssertHasMessageErrorContaining(line.JI_ModelInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckPartyIdentifier()
		{
			var controllingMsgHeaderHelper = new ControllingMsgHeaderTestHelper(Factory);
			var jobDeclartion = controllingMsgHeaderHelper.New(new string[] { "IF", "DN", "CD" });
			var header = jobDeclartion.Invoices.AddNew();
			var line1 = header.JobComInvoiceLines.AddNew();

			ControllingMsgHeaderTestHelper.SetControllingAgencyIsForCAHeader(line1, "CD", true, ControllingMessageTypeList.Codes.NX603);
			line1.PartyIdentifier = ZString.Empty;
			AssertHasMessageErrorContaining(line1.PartyIdentifierInfo, MandatoryValidation.YouHaveNotEntered);
			line1.PartyIdentifier = "AA";
			AssertNoMessageErrorContaining(line1.PartyIdentifierInfo, MandatoryValidation.YouHaveNotEntered);

			line1.PartyIdentifier = "X";
			AssertHasMessageErrorContaining(line1.PartyIdentifierInfo, ListValidation.InvalidCodeMessageError);

			line1.PartyIdentifier = PartyIdentifierCodeList.Codes._53;
			AssertNoMessageErrorContaining(line1.PartyIdentifierInfo, ListValidation.InvalidCodeMessageError);

			ControllingMsgHeaderTestHelper.SetControllingAgencyIsForCAHeader(line1, "CD", false, ControllingMessageTypeList.Codes.NX603);
			line1.PartyIdentifier = ZString.Empty;
			AssertNoMessageErrors(line1.PartyIdentifierInfo);
		}

		public void TestCheckAuthorizedPerson()
		{
			var controllingMsgHeaderHelper = new ControllingMsgHeaderTestHelper(Factory);
			var jobDeclartion = controllingMsgHeaderHelper.New(new string[] { "IF", "DN", "CD" });
			var header = jobDeclartion.Invoices.AddNew();
			var line1 = header.JobComInvoiceLines.AddNew();

			ControllingMsgHeaderTestHelper.SetControllingAgencyIsForCAHeader(line1, "CD", true, ControllingMessageTypeList.Codes.NX603);
			line1.AuthorizedPerson = ZString.Empty;
			AssertHasMessageErrorContaining(line1.AuthorizedPersonInfo, MandatoryValidation.YouHaveNotEntered);
			line1.AuthorizedPerson = "AA";
			AssertNoMessageErrorContaining(line1.AuthorizedPersonInfo, MandatoryValidation.YouHaveNotEntered);

			ControllingMsgHeaderTestHelper.SetControllingAgencyIsForCAHeader(line1, "CD", false, ControllingMessageTypeList.Codes.NX603);
			line1.AuthorizedPerson = ZString.Empty;
			AssertNoMessageErrors(line1.AuthorizedPersonInfo);
		}

		public void TestCheckCertificateNo()
		{
			var controllingMsgHeaderHelper = new ControllingMsgHeaderTestHelper(Factory);
			var jobDeclartion = controllingMsgHeaderHelper.New(new string[] { "IF", "DN", "CD" });
			var header = jobDeclartion.Invoices.AddNew();
			var line1 = header.JobComInvoiceLines.AddNew();

			ControllingMsgHeaderTestHelper.SetControllingAgencyIsForCAHeader(line1, "CD", true, ControllingMessageTypeList.Codes.NX603);
			line1.CertificateNo = ZString.Empty;
			AssertNoMessageErrors(line1.CertificateNoInfo);

			line1.CertificateNo = "XXXXXXXXXXXXX1";
			AssertNoMessageErrorContaining(line1.CertificateNoInfo, ValidationConstants.MedicalInstrumentOrFood.CertificateNumberLength);
			line1.CertificateNo = "XXX";
			AssertHasMessageErrorContaining(line1.CertificateNoInfo, ValidationConstants.MedicalInstrumentOrFood.CertificateNumberLength);

			ControllingMsgHeaderTestHelper.SetControllingAgencyIsForCAHeader(line1, "CD", false, ControllingMessageTypeList.Codes.NX603);
			line1.CertificateNo = "XXX";
			AssertNoMessageErrors(line1.CertificateNoInfo);
		}

		public void TestCheckJI_NetWeightUQ()
		{
			var targetInfo = InvoiceLine.JI_NetWeightUQInfo;
			InvoiceLine.JI_NetWeightUQ = "XX";
			AssertHasMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			InvoiceLine.JI_NetWeightUQ = InvoiceLine.Lookups.WeightUQList[0].Code;
			AssertNoMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			InvoiceLine.JI_NetWeightUQ = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_InvoiceUQ()
		{
			new TestTWCreator(Factory).CreateInvoiceUQ();
			var info = InvoiceLine.JI_InvoiceUQInfo;

			InvoiceLine.JI_InvoiceUQ = "XX";
			AssertHasMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);

			InvoiceLine.JI_InvoiceUQ = InvoiceLine.Lookups.InvoiceUQList[0].Code;
			AssertNoMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);

			InvoiceLine.JI_InvoiceUQ = ZString.Empty;
			AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);

			InvoiceLine.JI_InvoiceUQ = InvoiceLine.Lookups.InvoiceUQList[0].Code;
			AssertNoMessageErrors(info);
		}

		public void TestCheckJI_PrimaryPreference()
		{
			var declaration = InvoiceLine.Declaration;
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			InvoiceLine.JI_PrimaryPreference = "X1";
			AssertHasMessageErrorContaining(InvoiceLine.JI_PrimaryPreferenceInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.JI_PrimaryPreference = ZString.Empty;
			AssertHasMessageErrorContaining(InvoiceLine.JI_PrimaryPreferenceInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.JI_PrimaryPreference = "XX";
			AssertNoMessageErrorContaining(InvoiceLine.JI_PrimaryPreferenceInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			InvoiceLine.JI_PrimaryPreference = ZString.Empty;
			AssertNoMessageErrorContaining(InvoiceLine.JI_PrimaryPreferenceInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public virtual void TestCheckJI_Procedure()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure("TW", "EX", "90", ZString.Empty, ZString.Empty, "三角貿易之外貨復出口", "EXP", group: "G3,G5,D1,D5,B1,B2,B8,B9,F4,F5");
			helper.CreateRefCusProcedure("TW", "IM", "65", ZString.Empty, ZString.Empty, "預估稅捐", "IMP", group: "F3,G1,G2,D2,D7,D8,B6");

			CombineAssertions("Export", () =>
			{
				var invoiceLineExport = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
				invoiceLineExport.Declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
				invoiceLineExport.Validation.ValidateAll();
				AssertHasMessageErrorContaining(invoiceLineExport.JI_ProcedureInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLineExport.JI_Procedure = "XX";
				AssertHasMessageErrorContaining(invoiceLineExport.JI_ProcedureInfo, ListValidation.InvalidCodeMessageError);

				invoiceLineExport.JI_Procedure = "90";
				AssertNoMessageErrorContaining(invoiceLineExport.JI_ProcedureInfo, MandatoryValidation.YouHaveNotEntered);
			});

			CombineAssertions("Import", () =>
			{
				var invoiceLineImport = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
				invoiceLineImport.Declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
				invoiceLineImport.Validation.ValidateAll();
				AssertHasMessageErrorContaining(invoiceLineImport.JI_ProcedureInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLineImport.JI_Procedure = "XX";
				AssertHasMessageErrorContaining(invoiceLineImport.JI_ProcedureInfo, ListValidation.InvalidCodeMessageError);

				invoiceLineImport.JI_Procedure = "65";
				AssertNoMessageErrorContaining(invoiceLineImport.JI_ProcedureInfo, MandatoryValidation.YouHaveNotEntered);
			});

			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			var warehouseAddress = testOrg.Addresses.AddNew();
			warehouseAddress.Address1 = "Address1";
			warehouseAddress.Address2 = "Address2";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			var info = invoiceLine.JI_ProcedureInfo;
			invoiceLine.JI_CEI = entryInstruction.PK;

			CombineAssertions("D5", () =>
			{
				entryInstruction.CEI_OA_Warehouse2 = testOrg.MainAddress.PK;
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.D5;

				invoiceLine.JI_Procedure = "92";
				AssertHasMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForD5);

				invoiceLine.JI_Procedure = "97";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForD5);

				invoiceLine.JI_Procedure = "98";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForD5);

				invoiceLine.JI_Procedure = "9U";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForD5);

				invoiceLine.JI_Procedure = "1A";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForD5);

				invoiceLine.JI_Procedure = "8A";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForD5);

				invoiceLine.JI_Procedure = "82";
				AssertHasMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForD5);

				entryInstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
				invoiceLine.JI_Procedure = "92";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForD5);

				entryInstruction.CEI_OA_Warehouse2 = testOrg.MainAddress.PK;
				invoiceLine.JI_Procedure = "9M";
				AssertHasMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForD5);
			});

			CombineAssertions("B8", () =>
			{
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B8;
				invoiceLine.JI_Procedure = "03";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForD5);
				AssertHasMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForB8);

				invoiceLine.JI_Procedure = "81";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForB8);

				invoiceLine.JI_Procedure = "8A";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForB8);

				invoiceLine.JI_Procedure = "82";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForB8);

				invoiceLine.JI_Procedure = "92";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForB8);

				invoiceLine.JI_Procedure = "9M";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForB8);

				invoiceLine.JI_Procedure = "97";
				AssertHasMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForB8);
			});

			CombineAssertions("L1", () =>
			{
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.F5;
				invoiceLine.JI_Procedure = "98";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForB8);
				AssertHasMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypes);

				invoiceLine.JI_Procedure = "01";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypes);

				invoiceLine.JI_Procedure = "1A";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypes);

				invoiceLine.JI_Procedure = "02";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypes);

				invoiceLine.JI_Procedure = "03";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypes);

				invoiceLine.JI_Procedure = "04";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypes);

				invoiceLine.JI_Procedure = "05";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypes);

				invoiceLine.JI_Procedure = "06";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypes);

				invoiceLine.JI_Procedure = "08";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypes);

				invoiceLine.JI_Procedure = "81";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypes);

				invoiceLine.JI_Procedure = "82";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypes);

				invoiceLine.JI_Procedure = "8A";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypes);

				invoiceLine.JI_Procedure = "8B";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypes);

				invoiceLine.JI_Procedure = "8C";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypes);

				invoiceLine.JI_Procedure = "90";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypes);

				invoiceLine.JI_Procedure = "91";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypes);

				invoiceLine.JI_Procedure = "92";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypes);

				invoiceLine.JI_Procedure = "94";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypes);

				invoiceLine.JI_Procedure = "95";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypes);

				invoiceLine.JI_Procedure = "9A";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypes);

				invoiceLine.JI_Procedure = "9B";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypes);

				invoiceLine.JI_Procedure = "9C";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypes);

				invoiceLine.JI_Procedure = "9D";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypes);

				invoiceLine.JI_Procedure = "9E";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypes);

				invoiceLine.JI_Procedure = "9F";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypes);

				invoiceLine.JI_Procedure = "9G";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypes);

				invoiceLine.JI_Procedure = "9H";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypes);

				invoiceLine.JI_Procedure = "9K";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypes);

				invoiceLine.JI_Procedure = "9M";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypes);

				invoiceLine.JI_Procedure = "9N";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypes);

				invoiceLine.JI_Procedure = "9P";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypes);

				invoiceLine.JI_Procedure = "9L";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypes);

				invoiceLine.JI_Procedure = "9S";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypes);

				invoiceLine.JI_Procedure = "98";
				AssertHasMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypes);
			});

			CombineAssertions("D8", () =>
			{
				var messageErrorIncorrectDutyTreatmentTypesForD8 = ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForD8;
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D8;
				entryInstruction.CEI_OA_Warehouse = ZGuid.Empty;
				invoiceLine.JI_Procedure = "XX";
				AssertHasMessageErrorContaining(info, messageErrorIncorrectDutyTreatmentTypesForD8);
				invoiceLine.JI_Procedure = Constants.ProcedureCodes._98;
				AssertNoMessageErrorContaining(info, messageErrorIncorrectDutyTreatmentTypesForD8);
				invoiceLine.JI_Procedure = Constants.ProcedureCodes._92;
				AssertNoMessageErrorContaining(info, messageErrorIncorrectDutyTreatmentTypesForD8);
				invoiceLine.JI_Procedure = "XX";
				entryInstruction.CEI_OA_Warehouse = warehouseAddress.PK;
				AssertNoMessageErrorContaining(info, messageErrorIncorrectDutyTreatmentTypesForD8);
				entryInstruction.CEI_OA_Warehouse = ZGuid.Empty;
				entryInstruction.CEI_Style = "XX";
				AssertNoMessageErrorContaining(info, messageErrorIncorrectDutyTreatmentTypesForD8);
			});

			CombineAssertions("L1", () =>
			{
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.L1;
				var messageErrorIncorrectDutyTreatmentTypesForL1 = ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForL1;

				invoiceLine.JI_Procedure = "XX";
				AssertHasMessageErrorContaining(info, messageErrorIncorrectDutyTreatmentTypesForL1);
				invoiceLine.JI_Procedure = Constants.ProcedureCodes._98;
				AssertHasMessageErrorContaining(info, messageErrorIncorrectDutyTreatmentTypesForL1);
				invoiceLine.JI_Procedure = Constants.ProcedureCodes._92;
				AssertHasMessageErrorContaining(info, messageErrorIncorrectDutyTreatmentTypesForL1);
				invoiceLine.JI_Procedure = Constants.ProcedureCodes._99;
				AssertNoMessageErrorContaining(info, messageErrorIncorrectDutyTreatmentTypesForL1);
			});

			CombineAssertions("D1", () =>
			{
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.D1;

				invoiceLine.JI_Procedure = "97";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForD1);

				invoiceLine.JI_Procedure = "98";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForD1);

				invoiceLine.JI_Procedure = "9T";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForD1);

				invoiceLine.JI_Procedure = "9M";
				AssertHasMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForD1);
			});

			CombineAssertions("B1", () =>
			{
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B1;

				invoiceLine.JI_Procedure = "97";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForB1);

				invoiceLine.JI_Procedure = "9T";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForB1);

				invoiceLine.JI_Procedure = "9U";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForB1);

				invoiceLine.JI_Procedure = "9M";
				AssertHasMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForB1);
			});

			CombineAssertions("B6", () =>
			{
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.B6;

				invoiceLine.JI_Procedure = "56";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForB6);

				invoiceLine.JI_Procedure = "5Y";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForB6);

				invoiceLine.JI_Procedure = "58";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForB6);

				invoiceLine.JI_Procedure = "5C";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForB6);

				invoiceLine.JI_Procedure = "99";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForB6);

				invoiceLine.JI_Procedure = "9M";
				AssertHasMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForB6);

				invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.JI_LineNo = 3;
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForB6);

				invoiceLine.JI_LineNo = 1;
				AssertHasMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForB6);

				declaration.Invoices.AddNew();
				invoiceHeader.JZ_InvoiceDisplaySequence = 3;
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForB6);
			});

			CombineAssertions("F2", () =>
			{
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.F2;

				invoiceLine.JI_Procedure = "EF";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForF2);

				invoiceLine.JI_Procedure = "99";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForF2);

				var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_Procedure = "99";
				AssertNoMessageErrorContaining(invoiceLine2.JI_ProcedureInfo, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForF2);

				invoiceLine2.JI_Procedure = "EF";
				AssertNoMessageErrorContaining(invoiceLine2.JI_ProcedureInfo, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForF2);
				AssertHasMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForF2);

				invoiceLine2.JI_Procedure = "99";
				invoiceLine.JI_Procedure = "EF";
				AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForF2);
				AssertHasMessageErrorContaining(invoiceLine2.JI_ProcedureInfo, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForF2);
			});
		}

		public void TestCheckJI_InvoiceQuantity()
		{
			var info = InvoiceLine.JI_InvoiceQuantityInfo;
			string messageErrorNumOfChassisNotEqual = "The number of chassis number must be equal to invoice quantity.";
			string messageErrorPleaseEnterGreaterThanZero = "Please enter a 'Quantity' greater than 0.";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				InvoiceLine.JI_InvoiceQuantity = 1;
				AssertNoMessageErrorContaining(info, messageErrorNumOfChassisNotEqual);

				InvoiceLine.JI_CarType = "X";
				InvoiceLine.JI_InvoiceQuantity = 1;
				AssertHasMessageErrorContaining(info, messageErrorNumOfChassisNotEqual);

				var chassis = InvoiceLine.ChassisJobComInvLineRefsCollection.AddNew();
				chassis.JG_ReferenceNumber = "123456";

				InvoiceLine.JI_InvoiceQuantity = 1;
				AssertNoMessageErrorContaining(info, messageErrorNumOfChassisNotEqual);

				chassis.JG_ReferenceNumber = ZString.Empty;
				InvoiceLine.JI_InvoiceQuantity = 1;
				AssertHasMessageErrorContaining(info, messageErrorNumOfChassisNotEqual);

				chassis.JG_ReferenceNumber = "123456";
			}

			InvoiceLine.JI_InvoiceQuantity = -1;
			AssertHasMessageErrorContaining(info, messageErrorNumOfChassisNotEqual);
			AssertHasMessageErrorContaining(info, messageErrorPleaseEnterGreaterThanZero);

			InvoiceLine.JI_InvoiceQuantity = 0;
			AssertHasMessageErrorContaining(info, messageErrorNumOfChassisNotEqual);
			AssertHasMessageErrorContaining(info, messageErrorPleaseEnterGreaterThanZero);

			InvoiceLine.JI_InvoiceQuantity = 1;
			AssertNoMessageErrors(info);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			declaration.JE_MasterBill = "X";

			var bill = declaration.PrimaryMasterBill;
			var packageGroup = bill.PackingGroups.AddNew();

			packageGroup.Packages.AddNew();
			packageGroup.Packages.AddNew();

			var linePackageCollection = (InvoiceLineCusLinkPackageCollection)invoiceLine1.PackagesForInvoiceLinesForBindingOnly;
			var lineLinkPackage1 = linePackageCollection[0];
			lineLinkPackage1.IsLinked = true;
			lineLinkPackage1.Quantity = 30;
			invoiceLine1.JI_InvoiceQuantity = 100;

			var messageError2 = ValidationConstants.InvoiceLine.TotalQuantityForPackagesPivotNotEqualToInvoiceQuantity(30, invoiceLine1.JI_InvoiceQuantity);
			AssertHasMessageErrors(messageError2, invoiceLine1.JI_InvoiceQuantityInfo);

			var lineLinkPackage2 = linePackageCollection[1];
			lineLinkPackage2.IsLinked = true;
			lineLinkPackage2.Quantity = 70;
			invoiceLine1.JI_InvoiceQuantity = 100;
			AssertNoMessageErrors(messageError2, invoiceLine1.JI_InvoiceQuantityInfo);

			lineLinkPackage2.IsLinked = false;
			invoiceLine1.JI_InvoiceQuantity = 100;
			AssertHasMessageErrors(messageError2, invoiceLine1.JI_InvoiceQuantityInfo);

			lineLinkPackage1.IsLinked = false;
			lineLinkPackage2.IsLinked = false;
			invoiceLine1.JI_InvoiceQuantity = 100;
			AssertNoMessageErrors(messageError2, invoiceLine1.JI_InvoiceQuantityInfo);
		}

		public virtual void TestCheckJI_PreviousEntryNumber()
		{
			var messageError = "Previous Entry Number should be 14 characters long";

			InvoiceLine.JI_PreviousEntryNumber = ZString.Empty;
			InvoiceLine.RunPreSaveValidation();
			AssertNoMessageError(InvoiceLine.JI_PreviousEntryNumberInfo, messageError);
			AssertNoMessageErrorContaining(InvoiceLine.JI_PreviousEntryNumberInfo, MandatoryValidation.YouHaveNotEntered);

			InvoiceLine.JI_PreviousEntryNumber = "123456789";
			AssertHasMessageError(InvoiceLine.JI_PreviousEntryNumberInfo, messageError);

			InvoiceLine.JI_PreviousEntryNumber = "01234567890123";
			AssertNoMessageError(InvoiceLine.JI_PreviousEntryNumberInfo, messageError);

			InvoiceLine.JI_PreviousEntryNumber = ZString.Empty;
			InvoiceLine.JI_PreviousEntryLineNumber = 1;
			InvoiceLine.RunPreSaveValidation();
			AssertHasMessageErrorContaining(InvoiceLine.JI_PreviousEntryNumberInfo, MandatoryValidation.YouHaveNotEntered);

			InvoiceLine.JI_PreviousEntryNumber = "01234567890123";
			InvoiceLine.RunPreSaveValidation();
			AssertNoMessageErrorContaining(InvoiceLine.JI_PreviousEntryNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_PreviousEntryLineNumber()
		{
			var caption = "Previous Entry Line No.";
			var invoiceLine = (JobComInvoiceLine)Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew();

			invoiceLine.JI_PreviousEntryNumber = ZString.Empty;
			invoiceLine.JI_PreviousEntryLineNumber = 0;
			AssertNoMessageErrorContaining(invoiceLine.JI_PreviousEntryLineNumberInfo, MandatoryValidation.YouHaveNotEnteredMessage(caption));

			invoiceLine.JI_PreviousEntryNumber = "123465";
			invoiceLine.JI_PreviousEntryLineNumber = 0;
			AssertHasMessageErrorContaining(invoiceLine.JI_PreviousEntryLineNumberInfo, MandatoryValidation.YouHaveNotEnteredMessage(caption));

			invoiceLine.JI_PreviousEntryLineNumber = 20;
			AssertNoMessageErrorContaining(invoiceLine.JI_PreviousEntryLineNumberInfo, MandatoryValidation.YouHaveNotEnteredMessage(caption));
		}

		public void TestCheckJI_CEI()
		{
			var declaration = InvoiceLine.Declaration;
			var instruction1 = InvoiceLine.Declaration.CusEntryInstruction;
			var info = invoiceLine.JI_CEIInfo;

			invoiceLine.JI_CEI = ZGuid.Empty;
			AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_CEI = instruction1.PK;
			AssertNoMessageErrors(info);

			declaration.MakeNonPersistent();
			invoiceLine.JI_CEI = ZGuid.Empty;
			AssertNoMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_NetWeight()
		{
			var messageErrorPleaseEnterGreaterThanZero = "Please enter a 'Net Weight' greater than 0.";
			var info = InvoiceLine.JI_NetWeightInfo;
			InvoiceLine.JI_NetWeight = 1;
			AssertNoMessageError(info, messageErrorPleaseEnterGreaterThanZero);
			InvoiceLine.JI_NetWeight = 0;
			AssertHasMessageError(info, messageErrorPleaseEnterGreaterThanZero);
			InvoiceLine.JI_NetWeight = -1;
			AssertHasMessageError(info, messageErrorPleaseEnterGreaterThanZero);
		}

		public void TestCheckTariffIfInvalid()
		{
			var info = InvoiceLine.JI_TariffInfo;

			InvoiceLine.JI_Tariff = ZString.Empty;
			AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.TariffDoesnotExist);

			InvoiceLine.JI_Tariff = "20064000004";
			AssertHasMessageErrorContaining(info, ValidationConstants.InvoiceLine.TariffDoesnotExist);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, cusTariffType.PK, "10064000004", ZDateTime.BrettsBirthday, ZDateTime.Today);
			InvoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
			AssertNoMessageErrorContaining(info, ValidationConstants.InvoiceLine.TariffDoesnotExist);
		}

		public void TestCheckJI_TariffHasPackingHouseIfRequired()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanPackingHouse, "Taiwan Packing House");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(UniversalReferenceConstants.RefCusCodeListAttributes.Tariff, "Tariff", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanPackingHouse, Core.Constants.CountryCodes.Taiwan);
			var code = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanPackingHouse, "XXXX0123", "XXXXXX", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			code.Attributes.AddNew(UniversalReferenceConstants.RefCusCodeListAttributes.Tariff, "07061000005");
			Factory.Save();

			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = jobDeclartion.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var line = (JobComInvoiceLine)jobDeclartion.Invoices.AddNew().InvoiceLines.AddNew();
			line.JI_CEI = entryInstruction.PK;
			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = "NX401";
			line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;
			line.JI_Tariff = "07061000005";
			AssertHasMessageErrorContaining(line.JI_TariffInfo, "Please enter a Packing House for Tariff");

			line.JI_Tariff = "07061000004";
			AssertNoMessageErrorContaining(line.JI_TariffInfo, "Please enter a Packing House for Tariff");
		}

		public void TestCheckJI_LinePrice()
		{
			CombineAssertions(() =>
			{
				var equalityWarning = "Unit Price * Quantity must equal to Line Price.";
				var notNegativeWarning = "Line Price must be greater than or equal to zero.";
				var info = InvoiceLine.JI_LinePriceInfo;

				InvoiceLine.JI_LinePrice = -1m;
				AssertHasWarning("JI_LinePrice is -1", info, notNegativeWarning);

				InvoiceLine.JI_LinePrice = 0m;
				AssertHasWarning("JI_LinePrice = 0", info, notNegativeWarning);

				InvoiceLine.JI_LinePrice = 1m;
				AssertNoWarning("JI_LinePrice = 1", info, notNegativeWarning);

				InvoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;

				InvoiceLine.JI_InvoiceQuantity = 3m;
				InvoiceLine.JI_EnteredUnitPrice = 33.3m;
				InvoiceLine.JI_LinePrice = 99.99;
				AssertHasWarning("3 * 33.3 = 99.99", info, equalityWarning);

				InvoiceLine.JI_InvoiceQuantity = 3m;
				InvoiceLine.JI_EnteredUnitPrice = 33.33m;
				InvoiceLine.JI_LinePrice = 99.99;
				AssertNoWarnings("3 * 33.33 = 99.99", info);

				InvoiceLine.JI_InvoiceQuantity = 3m;
				InvoiceLine.JI_EnteredUnitPrice = 33.333m;
				InvoiceLine.JI_LinePrice = 99.99;
				AssertHasWarning("3 * 33.333 = 99.99", info, equalityWarning);

				InvoiceLine.JI_InvoiceQuantity = 3m;
				InvoiceLine.JI_EnteredUnitPrice = 33.3333m;
				InvoiceLine.JI_LinePrice = 99.99;
				AssertHasWarning("3 * 33.3333 = 99.99", info, equalityWarning);

				InvoiceLine.JI_InvoiceQuantity = 3m;
				InvoiceLine.JI_EnteredUnitPrice = 33.33333m;
				InvoiceLine.JI_LinePrice = 99.99;
				AssertHasWarning("3 * 33.33333 = 99.99", info, equalityWarning);

				InvoiceLine.JI_InvoiceQuantity = 3m;
				InvoiceLine.JI_EnteredUnitPrice = 33.333333m;
				InvoiceLine.JI_LinePrice = 99.99;
				AssertHasWarning("3 * 33.333333 = 99.99", info, equalityWarning);

				InvoiceLine.JI_InvoiceQuantity = 218m;
				InvoiceLine.JI_EnteredUnitPrice = 105.4545m;
				InvoiceLine.JI_LinePrice = 22989;
				AssertNoWarnings("218 * 105.4545 = 22989", info);

				InvoiceLine.JI_InvoiceQuantity = 3m;
				InvoiceLine.JI_EnteredUnitPrice = 33.3m;
				InvoiceLine.JI_LinePrice = 100;
				AssertNoWarnings("3 * 33.3 = 100", info);

				InvoiceLine.JI_InvoiceQuantity = 3m;
				InvoiceLine.JI_EnteredUnitPrice = 33.33m;
				InvoiceLine.JI_LinePrice = 100;
				AssertNoWarnings("3 * 33.33 = 100", info);

				InvoiceLine.JI_InvoiceQuantity = 3m;
				InvoiceLine.JI_EnteredUnitPrice = 33.333m;
				InvoiceLine.JI_LinePrice = 100;
				AssertNoWarnings("3 * 33.333 = 100", info);

				InvoiceLine.JI_InvoiceQuantity = 3m;
				InvoiceLine.JI_EnteredUnitPrice = 33.3333m;
				InvoiceLine.JI_LinePrice = 100;
				AssertNoWarnings("3 * 33.3333 = 100", info);

				InvoiceLine.JI_InvoiceQuantity = 3m;
				InvoiceLine.JI_EnteredUnitPrice = 33.33333m;
				InvoiceLine.JI_LinePrice = 100;
				AssertNoWarnings("3 * 33.33333 = 100", info);

				InvoiceLine.JI_InvoiceQuantity = 3m;
				InvoiceLine.JI_EnteredUnitPrice = 33.333333m;
				InvoiceLine.JI_LinePrice = 100;
				AssertNoWarnings("3 * 33.333333 = 100", info);

				InvoiceLine.JI_InvoiceQuantity = 33.3m;
				InvoiceLine.JI_EnteredUnitPrice = 3m;
				InvoiceLine.JI_LinePrice = 100;
				AssertNoWarnings("33.3 * 3 = 100", info);

				InvoiceLine.JI_InvoiceQuantity = 33.33m;
				InvoiceLine.JI_EnteredUnitPrice = 3m;
				InvoiceLine.JI_LinePrice = 100;
				AssertNoWarnings("33.33 * 3 = 100", info);

				InvoiceLine.JI_InvoiceQuantity = 33.333m;
				InvoiceLine.JI_EnteredUnitPrice = 3m;
				InvoiceLine.JI_LinePrice = 100;
				AssertNoWarnings("33.333 * 3 = 100", info);

				InvoiceLine.JI_InvoiceQuantity = 33.3333m;
				InvoiceLine.JI_EnteredUnitPrice = 3m;
				InvoiceLine.JI_LinePrice = 100;
				AssertNoWarnings("33.3333 * 3 = 100", info);

				InvoiceLine.JI_InvoiceQuantity = 33m;
				InvoiceLine.JI_EnteredUnitPrice = 3m;
				InvoiceLine.JI_LinePrice = 100;
				AssertHasWarning("33 * 3 = 100", info, equalityWarning);

				InvoiceLine.JI_InvoiceQuantity = 3m;
				InvoiceLine.JI_EnteredUnitPrice = 33.34m;
				InvoiceLine.JI_LinePrice = 99.99;
				AssertHasWarning("3 * 33.4 = 99.99", info, equalityWarning);

				InvoiceLine.JI_InvoiceQuantity = 3m;
				InvoiceLine.JI_EnteredUnitPrice = 34m;
				InvoiceLine.JI_LinePrice = 99.99;
				AssertHasWarning("3 * 34 = 99.99", info, equalityWarning);

				InvoiceLine.JI_InvoiceQuantity = 4m;
				InvoiceLine.JI_EnteredUnitPrice = 33.33m;
				InvoiceLine.JI_LinePrice = 99.99;
				AssertHasWarning("4 * 33.33 = 99.99", info, equalityWarning);

				InvoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

				InvoiceLine.JI_InvoiceQuantity = 218m;
				InvoiceLine.JI_EnteredUnitPrice = 105.4545m;
				InvoiceLine.JI_LinePrice = 22989;
				AssertHasWarning("218 * 105.4545 = 22989 USD", info, equalityWarning);
			});
		}

		public void TestCheckJI_ExtraInfoForClassification()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			invoiceLine = (JobComInvoiceLine)jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			var cusEntryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			cusEntryInstruction.CEI_Style = "G8";
			cusEntryInstruction.CEI_ExamMode = ExamModeList.Codes.ShallBe;
			invoiceLine.JI_CEI = cusEntryInstruction.PK;
			Factory.Save();

			invoiceLine.JI_ExtraInfoForClassification = "XX";
			AssertHasMessageError(invoiceLine.JI_ExtraInfoForClassificationInfo, ValidationConstants.InvoiceLine.ExtraInfoErrorMessage);

			cusEntryInstruction.CEI_ExamMode = ExamModeList.Codes.WrittenReview;
			invoiceLine.JI_ExtraInfoForClassification = "ff";
			AssertNoMessageError(invoiceLine.JI_ExtraInfoForClassificationInfo, ValidationConstants.InvoiceLine.ExtraInfoErrorMessage);
		}

		public void TestCheckJI_ConcessionOrder()
		{
			TariffDataForTestHelper.GenerateTariffAndRateIncludingConcessionOrderData(Factory);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationDateOverride = new ZDateTime(2020, 02, 15, 0, 0, 0);
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "98050000009";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.JI_PrimaryPreference = "PRE";
			invoiceLine.JI_ConcessionOrder = "QUOTA";
			AssertHasMessageErrorContaining(invoiceLine.JI_ConcessionOrderInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.JI_PrimaryPreference = "PR1";
			invoiceLine.JI_ConcessionOrder = "QUOTA";
			AssertNoMessageErrorContaining(invoiceLine.JI_ConcessionOrderInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.JI_ConcessionOrder = "TEST";
			AssertHasMessageErrorContaining(invoiceLine.JI_ConcessionOrderInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJI_CustomsThirdQuantity()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var controllingMessageHeaders = jobDeclaration.CusEntryInstruction.ControllingMessageHeaders;
			controllingMessageHeaders.AddNew().TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX201_01;
			controllingMessageHeaders.AddNew().TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301;
			controllingMessageHeaders.AddNew().TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_AX;
			controllingMessageHeaders.AddNew().TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_DN;
			controllingMessageHeaders.AddNew().TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			controllingMessageHeaders.AddNew().TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			controllingMessageHeaders.AddNew().TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX603;
			var line = jobDeclaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var zdecimal1 = new ZDecimal(1);
			ControllingMsgHeaderTestHelper.AssertHasMessageErrorContainingByMessageType(line, line.JI_CustomsThirdQuantityInfo, zdecimal1, ZDecimal.Zero, ControllingMessageTypeList.Codes.NX201_01, MandatoryValidation.ValueCannotBeZero);
			ControllingMsgHeaderTestHelper.AssertHasMessageErrorContainingByMessageType(line, line.JI_CustomsThirdQuantityInfo, zdecimal1, ZDecimal.Zero, ControllingMessageTypeList.Codes.NX301, MandatoryValidation.ValueCannotBeZero);
			ControllingMsgHeaderTestHelper.AssertHasMessageErrorContainingByMessageType(line, line.JI_CustomsThirdQuantityInfo, zdecimal1, ZDecimal.Zero, ControllingMessageTypeList.Codes.NX301_AX, MandatoryValidation.ValueCannotBeZero);
			ControllingMsgHeaderTestHelper.AssertHasMessageErrorContainingByMessageType(line, line.JI_CustomsThirdQuantityInfo, zdecimal1, ZDecimal.Zero, ControllingMessageTypeList.Codes.NX301_DN, MandatoryValidation.ValueCannotBeZero);
			ControllingMsgHeaderTestHelper.AssertHasMessageErrorContainingByMessageType(line, line.JI_CustomsThirdQuantityInfo, zdecimal1, ZDecimal.Zero, ControllingMessageTypeList.Codes.NX401, MandatoryValidation.ValueCannotBeZero);
			ControllingMsgHeaderTestHelper.AssertHasMessageErrorContainingByMessageType(line, line.JI_CustomsThirdQuantityInfo, zdecimal1, ZDecimal.Zero, ControllingMessageTypeList.Codes.NX601, MandatoryValidation.ValueCannotBeZero);
			ControllingMsgHeaderTestHelper.AssertHasMessageErrorContainingByMessageType(line, line.JI_CustomsThirdQuantityInfo, zdecimal1, ZDecimal.Zero, ControllingMessageTypeList.Codes.NX603, MandatoryValidation.ValueCannotBeZero);
		}

		public void TestCheckJI_CustomsThirdUnitQty()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var controllingMessageHeaders = jobDeclaration.CusEntryInstruction.ControllingMessageHeaders;
			controllingMessageHeaders.AddNew().TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX201_01;
			controllingMessageHeaders.AddNew().TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301;
			controllingMessageHeaders.AddNew().TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_AX;
			controllingMessageHeaders.AddNew().TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_DN;
			controllingMessageHeaders.AddNew().TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			controllingMessageHeaders.AddNew().TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			controllingMessageHeaders.AddNew().TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX603;
			var line = jobDeclaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var zstringPKG = new ZString("PKG");
			ControllingMsgHeaderTestHelper.AssertHasMessageErrorContainingByMessageType(line, line.JI_CustomsThirdUnitQtyInfo, zstringPKG, ZString.Empty, ControllingMessageTypeList.Codes.NX201_01, MandatoryValidation.YouHaveNotEntered);
			ControllingMsgHeaderTestHelper.AssertHasMessageErrorContainingByMessageType(line, line.JI_CustomsThirdUnitQtyInfo, zstringPKG, ZString.Empty, ControllingMessageTypeList.Codes.NX301, MandatoryValidation.YouHaveNotEntered);
			ControllingMsgHeaderTestHelper.AssertHasMessageErrorContainingByMessageType(line, line.JI_CustomsThirdUnitQtyInfo, zstringPKG, ZString.Empty, ControllingMessageTypeList.Codes.NX301_AX, MandatoryValidation.YouHaveNotEntered);
			ControllingMsgHeaderTestHelper.AssertHasMessageErrorContainingByMessageType(line, line.JI_CustomsThirdUnitQtyInfo, zstringPKG, ZString.Empty, ControllingMessageTypeList.Codes.NX301_DN, MandatoryValidation.YouHaveNotEntered);
			ControllingMsgHeaderTestHelper.AssertHasMessageErrorContainingByMessageType(line, line.JI_CustomsThirdUnitQtyInfo, zstringPKG, ZString.Empty, ControllingMessageTypeList.Codes.NX401, MandatoryValidation.YouHaveNotEntered);
			ControllingMsgHeaderTestHelper.AssertHasMessageErrorContainingByMessageType(line, line.JI_CustomsThirdUnitQtyInfo, zstringPKG, ZString.Empty, ControllingMessageTypeList.Codes.NX601, MandatoryValidation.YouHaveNotEntered);
			ControllingMsgHeaderTestHelper.AssertHasMessageErrorContainingByMessageType(line, line.JI_CustomsThirdUnitQtyInfo, zstringPKG, ZString.Empty, ControllingMessageTypeList.Codes.NX603, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_Calc_RAPRORUnitPrice()
		{
			var targetInfo = InvoiceLine.JI_Calc_RAPRORUnitPriceInfo;
			var messageValueCannotBeNegative = MandatoryValidation.ValueCannotBeNegative;
			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._37;
			InvoiceLine.JI_InvoiceQuantity = 10;
			InvoiceLine.JI_Calc_RAPRORUnitPrice = -1;
			AssertHasMessageErrorContaining(targetInfo, messageValueCannotBeNegative);
			InvoiceLine.JI_Calc_RAPRORUnitPrice = 0;
			AssertNoMessageErrorContaining(targetInfo, messageValueCannotBeNegative);
			InvoiceLine.JI_Calc_RAPRORUnitPrice = 1;
			AssertNoMessageErrorContaining(targetInfo, messageValueCannotBeNegative);

			AssertNoMessageErrors(targetInfo);
		}

		public void TestCheckJI_DeclarationGoodsDescription()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var invoiceLine = (JobComInvoiceLine)jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.OverrideDeclarationGoodsDescription = true;
			invoiceLine.JI_DeclarationGoodsDescription = "A";
			invoiceLine.JI_DeclarationGoodsDescription = ZString.Empty;
			var info = invoiceLine.JI_DeclarationGoodsDescriptionInfo;
			AssertHasErrorContaining(info, MandatoryValidation.MustBeEntered);
			AssertNoMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_DeclarationGoodsDescription = "AA";
			AssertNoMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);
			AssertNoErrorContaining(info, MandatoryValidation.MustBeEntered);

			invoiceLine.OverrideDeclarationGoodsDescription = false;
			invoiceLine.Validation.ValidateJI_DeclarationGoodsDescription();
			AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);
			AssertNoErrorContaining(info, MandatoryValidation.MustBeEntered);

			invoiceLine.JI_NDescription = "A";
			invoiceLine.Validation.ValidateJI_DeclarationGoodsDescription();
			AssertNoMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);
			AssertNoErrorContaining(info, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckJI_DeclarationGoodsDescriptionLength()
		{
			var warning = "Grouping plus Declaration Goods Description Only the first 512 characters will be sent to the customs.";
			var targetInfo = InvoiceLine.JI_DeclarationGoodsDescriptionInfo;
			InvoiceLine.JI_Group = "AAA";
			InvoiceLine.JI_DeclarationGoodsDescription = new ZString('A', 510);
			AssertHasWarning(targetInfo, warning);

			InvoiceLine.JI_DeclarationGoodsDescription = new ZString('A', 509);
			AssertNoWarning(targetInfo, warning);

			InvoiceLine.JI_Group = "AAAA";
			InvoiceLine.Validation.ValidateJI_DeclarationGoodsDescription();
			AssertHasWarning(targetInfo, warning);
		}

		public void TestCheckJI_DeclarationGoodsDescriptionLinkedToSameNX301_DNMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var invoiceLine3 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine3.JI_CEI = entryInstruction.PK;
			var targetInfo = invoiceLine1.JI_DeclarationGoodsDescriptionInfo;

			var controllingMessageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_DN;
			var waringMessage = "Invoice lines of different Declaration Goods Description cannot be linked to the same NX301_DN message.";
			invoiceLine1.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(c => c.ControllingMessageHeader.IsNX301_DN).IsLinkedCMHeader = false;
			invoiceLine2.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(c => c.ControllingMessageHeader.IsNX301_DN).IsLinkedCMHeader = true;
			invoiceLine3.JI_DeclarationGoodsDescription = "DeclarationGoodsDescription 3";
			invoiceLine2.JI_DeclarationGoodsDescription = "DeclarationGoodsDescription 2";
			invoiceLine1.JI_DeclarationGoodsDescription = "DeclarationGoodsDescription 1";
			AssertNoWarning("Invoice lines of different Declaration Goods Description, but do not linked to the NX301_DN message.", targetInfo, waringMessage);
			invoiceLine1.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(c => c.ControllingMessageHeader.IsNX301_DN).IsLinkedCMHeader = true;
			invoiceLine1.Validation.ValidateJI_DeclarationGoodsDescription();
			AssertHasWarning("Invoice lines of different Declaration Goods Description linked to the same NX301_DN message.", targetInfo, waringMessage);
			invoiceLine1.JI_DeclarationGoodsDescription = "DeclarationGoodsDescription 2";
			AssertNoWarning("Invoice lines of same Declaration Goods Description linked to the same NX301_DN message.", targetInfo, waringMessage);
		}

		public void TestCheckJI_CountryOfOrigin()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var invoiceLine3 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine3.JI_CEI = entryInstruction.PK;
			var targetInfo = invoiceLine1.JI_CountryOfOriginInfo;

			var controllingMessageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_DN;
			var waringMessage = "Invoice lines of different Goods Origin cannot be linked to the same NX301_DN message.";
			invoiceLine1.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(c => c.ControllingMessageHeader.IsNX301_DN).IsLinkedCMHeader = false;
			invoiceLine2.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(c => c.ControllingMessageHeader.IsNX301_DN).IsLinkedCMHeader = true;
			invoiceLine3.JI_CountryOfOrigin = "AU";
			invoiceLine2.JI_CountryOfOrigin = "TW";
			invoiceLine1.JI_CountryOfOrigin = "US";
			AssertNoWarning("Invoice lines of different Goods Origin, but do not linked to the NX301_DN message.", targetInfo, waringMessage);
			invoiceLine1.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(c => c.ControllingMessageHeader.IsNX301_DN).IsLinkedCMHeader = true;
			invoiceLine1.Validation.ValidateJI_CountryOfOrigin();
			AssertHasWarning("Invoice lines of different Goods Origin linked to the same NX301_DN message.", targetInfo, waringMessage);
			invoiceLine1.JI_CountryOfOrigin = "TW";
			AssertNoWarning("Invoice lines of same Goods Origin linked to the same NX301_DN message.", targetInfo, waringMessage);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCheckTrademarkStorageDocsGuid()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "supplier";
			supplier.OH_IsConsignor = true;
			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "PRO1";
			var relatedOrganization = part1.RelatedOrganisations.AddNew();
			relatedOrganization.OU_OH = supplier.PK;
			relatedOrganization.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "HBL1";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_OH_Supplier = supplier.PK;

			var decDocManagerInfo = ((IDocManagerSupport)declaration).DocManagerInfo;
			var shipmentDocManagerInfo = ((IDocManagerSupport)shipment).DocManagerInfo;
			var partDocManagerInfo = part1.DocManagerInfo();

			var doc1 = decDocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\sample.pdf"), MessageConstants.DocumentTypes.CAT);
			var doc2 = decDocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\TestBitmap.bmp"), MessageConstants.DocumentTypes.TDM);

			var doc3 = partDocManagerInfo.AddFileOrDocument(doc2.ImageData, "sample1.pdf", MessageConstants.DocumentTypes.CAT);
			var doc4 = partDocManagerInfo.AddFileOrDocument(doc2.ImageData, "TestBitmap1.png", MessageConstants.DocumentTypes.TDM);

			var doc5 = shipmentDocManagerInfo.AddFileOrDocument(doc2.ImageData, "sample2.pdf", MessageConstants.DocumentTypes.CAT);
			var doc6 = shipmentDocManagerInfo.AddFileOrDocument(doc2.ImageData, "TestBitmap2.jpg", MessageConstants.DocumentTypes.TDM);
			Factory.Save();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var info = invoiceLine.TrademarkStorageDocsGuidInfo;

			invoiceLine.TrademarkStorageDocsGuid = ZGuid.Invalid;
			AssertHasErrorContaining(info, ListValidation.InvalidCodeError);
			invoiceLine.TrademarkStorageDocsGuid = doc1.UniqueKey;
			AssertHasErrorContaining(info, ListValidation.InvalidCodeError);
			invoiceLine.TrademarkStorageDocsGuid = doc2.UniqueKey;
			AssertNoErrorContaining(info, ListValidation.InvalidCodeError);

			invoiceLine.TrademarkStorageDocsGuid = doc3.UniqueKey;
			AssertHasErrorContaining(info, ListValidation.InvalidCodeError);
			invoiceLine.TrademarkStorageDocsGuid = doc4.UniqueKey;
			AssertHasErrorContaining(info, ListValidation.InvalidCodeError);

			invoiceLine.TrademarkStorageDocsGuid = doc5.UniqueKey;
			AssertHasErrorContaining(info, ListValidation.InvalidCodeError);
			invoiceLine.TrademarkStorageDocsGuid = doc6.UniqueKey;
			AssertNoErrorContaining(info, ListValidation.InvalidCodeError);

			invoiceLine.JI_PartNo = "PRO1";
			invoiceLine.TrademarkStorageDocsGuid = doc3.UniqueKey;
			AssertHasErrorContaining(info, ListValidation.InvalidCodeError);
			invoiceLine.TrademarkStorageDocsGuid = doc4.UniqueKey;
			AssertNoErrorContaining(info, ListValidation.InvalidCodeError);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCheckTrademarkStorageDocsFileExtention()
		{
			string errorMessage = "Please select an image.";
			var declaration = Factory.New<JobDeclaration>();
			var decDocManagerInfo = ((IDocManagerSupport)declaration).DocManagerInfo;

			var pdfDoc = decDocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\eat_glass.pdf"), MessageConstants.DocumentTypes.TDM);
			var bmpDoc = decDocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\TestBitmap.bmp"), MessageConstants.DocumentTypes.TDM);
			var pngDoc = decDocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\cn1.png"), MessageConstants.DocumentTypes.TDM);
			var tifDoc = decDocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\200kb.tif"), MessageConstants.DocumentTypes.TDM);
			var jpgDoc = decDocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\compressed.jpg"), MessageConstants.DocumentTypes.TDM);
			var jpegDoc = decDocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\trademark.jpeg"), MessageConstants.DocumentTypes.TDM);
			var gifDoc = decDocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Subfolder\AnotherSubfolder\small.gif"), MessageConstants.DocumentTypes.TDM);

			Factory.Save();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var info = invoiceLine.TrademarkStorageDocsGuidInfo;

			invoiceLine.TrademarkStorageDocsGuid = ZGuid.Empty;
			AssertNoMessageErrorContaining(info, errorMessage);

			invoiceLine.TrademarkStorageDocsGuid = pdfDoc.UniqueKey;
			AssertHasMessageErrorContaining(info, errorMessage);

			invoiceLine.TrademarkStorageDocsGuid = bmpDoc.UniqueKey;
			AssertNoMessageErrorContaining(info, errorMessage);

			invoiceLine.TrademarkStorageDocsGuid = pngDoc.UniqueKey;
			AssertNoMessageErrorContaining(info, errorMessage);

			invoiceLine.TrademarkStorageDocsGuid = tifDoc.UniqueKey;
			AssertNoMessageErrorContaining(info, errorMessage);

			invoiceLine.TrademarkStorageDocsGuid = jpgDoc.UniqueKey;
			AssertNoMessageErrorContaining(info, errorMessage);

			invoiceLine.TrademarkStorageDocsGuid = jpegDoc.UniqueKey;
			AssertNoMessageErrorContaining(info, errorMessage);

			invoiceLine.TrademarkStorageDocsGuid = gifDoc.UniqueKey;
			AssertNoMessageErrorContaining(info, errorMessage);
		}

		public virtual void TestCheckJI_BrandName()
		{
			var messageError = "'Brand' cannot be empty when the goods is subject to F01 or F02 import regulation. For further details, please hit F3 on the 'Tariff' field and go to Details > Attribute.";

			var info = InvoiceLine.JI_BrandNameInfo;
			var declaration = InvoiceLine.Declaration;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "2713200000", minDate, maxDate);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "2713200001", minDate, maxDate);
			var tariff3 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "2713200002", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportRegulations, Constants.ImportExportRegulationCodes.RegulationsCodeF01, tariff2);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportRegulations, Constants.ImportExportRegulationCodes.RegulationsCodeF02, tariff3);
			Factory.Save();

			declaration.JE_MessageType = "IMP";
			InvoiceLine.Validation.ValidateJI_BrandName();
			AssertNoMessageErrorContaining(info, messageError);

			declaration.JE_MessageType = "EXP";
			InvoiceLine.Validation.ValidateJI_BrandName();
			AssertNoMessageErrorContaining(info, messageError);

			declaration.JE_MessageType = "IMP";
			InvoiceLine.JI_Tariff = "2713200000";
			InvoiceLine.Validation.ValidateJI_BrandName();
			AssertNoMessageErrorContaining(info, messageError);

			declaration.JE_MessageType = "EXP";
			InvoiceLine.Validation.ValidateJI_BrandName();
			AssertNoMessageErrorContaining(info, messageError);

			declaration.JE_MessageType = "IMP";
			InvoiceLine.JI_Tariff = "2713200001";
			InvoiceLine.Validation.ValidateJI_BrandName();
			AssertHasMessageErrorContaining(info, messageError);

			declaration.JE_MessageType = "EXP";
			InvoiceLine.Validation.ValidateJI_BrandName();
			AssertNoMessageErrorContaining(info, messageError);

			declaration.JE_MessageType = "IMP";
			InvoiceLine.JI_Tariff = "2713200002";
			InvoiceLine.Validation.ValidateJI_BrandName();
			AssertHasMessageErrorContaining(info, messageError);

			declaration.JE_MessageType = "EXP";
			InvoiceLine.Validation.ValidateJI_BrandName();
			AssertNoMessageErrorContaining(info, messageError);
		}

		public void TestCheckJI_BrandNameLinkedToSameNX301_AXMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var invoiceLine3 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine3.JI_CEI = entryInstruction.PK;
			var targetInfo = invoiceLine1.JI_BrandNameInfo;

			var controllingMessageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_AX;
			var waringMessage = "Invoice lines of different Brand name cannot be linked to the same NX301_AX message.";
			invoiceLine1.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(c => c.ControllingMessageHeader.IsNX301_AX).IsLinkedCMHeader = false;
			invoiceLine2.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(c => c.ControllingMessageHeader.IsNX301_AX).IsLinkedCMHeader = true;
			invoiceLine3.JI_BrandName = "Oppo";
			invoiceLine2.JI_BrandName = "Apple";
			invoiceLine1.JI_BrandName = "Samsung";
			AssertNoWarning("Invoice lines of different Brand name, but do not linked to the NX301_AX message.", targetInfo, waringMessage);
			invoiceLine1.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(c => c.ControllingMessageHeader.IsNX301_AX).IsLinkedCMHeader = true;
			invoiceLine1.Validation.ValidateJI_BrandName();
			AssertHasWarning("Invoice lines of different Brand name linked to the same NX301_AX message.", targetInfo, waringMessage);
			invoiceLine1.JI_BrandName = "Apple";
			AssertNoWarning("Invoice lines of same Brand name linked to the same NX301_AX message.", targetInfo, waringMessage);
		}

		public void TestCheckJI_BrandNameLinkedToSameNX601Message()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var invoiceLine3 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine3.JI_CEI = entryInstruction.PK;
			var targetInfo = invoiceLine1.JI_BrandNameInfo;

			var controllingMessageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			var waringMessage = "Invoice lines of different Brand name cannot be linked to the same NX601 message.";
			invoiceLine1.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(c => c.ControllingMessageHeader.IsNX601).IsLinkedCMHeader = false;
			invoiceLine2.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(c => c.ControllingMessageHeader.IsNX601).IsLinkedCMHeader = true;
			invoiceLine3.JI_BrandName = "Oppo";
			invoiceLine2.JI_BrandName = "Apple";
			invoiceLine1.JI_BrandName = "Samsung";
			AssertNoWarning("Invoice lines of different Brand name, but do not linked to the NX601 message.", targetInfo, waringMessage);
			invoiceLine1.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(c => c.ControllingMessageHeader.IsNX601).IsLinkedCMHeader = true;
			invoiceLine1.Validation.ValidateJI_BrandName();
			AssertHasWarning("Invoice lines of different Brand name linked to the same NX601 message.", targetInfo, waringMessage);
			invoiceLine1.JI_BrandName = "Apple";
			AssertNoWarning("Invoice lines of same Brand name linked to the same NX601 message.", targetInfo, waringMessage);
		}

		public void TestCheckJI_ZZF_NKTaxType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var today = ZDateTime.Today;
			var cusProcedure = helper.CreateRefCusProcedure("TW", "EX", "90", ZString.Empty, ZString.Empty, "三角貿易之外貨復出口", "EXP", group: "G3,G5,D1,D5,B1,B2,B8,B9,F4,F5");
			helper.CreateTaxOrFee("TW1", 0, Core.Constants.CountryCodes.Taiwan, today.AddDays(-3), today.AddDays(-2), "DESC1");
			cusProcedure.ZZ6_CalculateVAT = false;
			Factory.Save();
			InvoiceLine.JI_Procedure = "90";
			InvoiceLine.JI_ZZF_NKTaxType = "TW1";

			AssertNoNotifications(InvoiceLine.JI_ZZF_NKTaxTypeInfo);
		}

		public void TestCheckJI_HazMatCode()
		{
			var undgSubstance = Factory.New<UNDGSubstance>();
			undgSubstance.DG_UNNO = "1234";

			var info = InvoiceLine.JI_HazMatCodeInfo;
			var declaration = InvoiceLine.Declaration;
			declaration.JE_TransportMode = "AIR";
			InvoiceLine.JI_HazMatCode = "4321";
			AssertHasMessageError(info, "The entered DG Code does not exist.");

			InvoiceLine.JI_HazMatCode = "1234";
			AssertNoMessageError(info, "The entered DG Code does not exist.");

			declaration.JE_TransportMode = "SEA";
			InvoiceLine.JI_HazMatCode = "4321";
			AssertHasMessageError(info, "The entered DG Code does not exist.");

			InvoiceLine.JI_HazMatCode = "1234";
			AssertNoMessageError(info, "The entered DG Code does not exist.");
		}

		public void TestCheckNX101ShippingMarks()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = jobDeclartion.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var line = (JobComInvoiceLine)jobDeclartion.Invoices.AddNew().InvoiceLines.AddNew();
			line.JI_CEI = entryInstruction.PK;
			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = "NX101";
			controllingMessageHeader.TW1_CertificateType = "15";

			CombineAssertions(() =>
			{
				var targetInfo = line.NX101ShippingMarksInfo;
				line.Validation.ValidateNX101ShippingMarks();
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

				line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;

				line.Validation.ValidateNX101ShippingMarks();
				AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

				line.NX101ShippingMarks = "XX";
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = "09";
				line.NX101ShippingMarks = ZString.Empty;
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJI_InnerPackDescription()
		{
			var decl = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = decl.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var line = (JobComInvoiceLine)decl.Invoices.AddNew().InvoiceLines.AddNew();
			line.JI_CEI = entryInstruction.PK;
			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;

			CombineAssertions(() =>
			{
				var targetInfo = line.JI_InnerPackDescriptionInfo;
				line.Validation.ValidateJI_InnerPackDescription();
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

				line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;

				line.Validation.ValidateJI_InnerPackDescription();
				AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

				line.JI_InnerPackDescription = "XX";
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code10;
				line.JI_InnerPackDescription = ZString.Empty;
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckNX101PermitGoodsDescription()
		{
			var info = InvoiceLine.NX101PermitGoodsDescriptionInfo;
			invoiceLine.Validation.ValidateNX101PermitGoodsDescription();
			AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);

			InvoiceLine.OverrideNX101PermitGoodsDescription = true;
			InvoiceLine.NX101PermitGoodsDescription = "xx";
			AssertNoMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckTW1_CertificateType_JI_CountryOfOrigin()
		{
			AssertCheckTW1_CertificateType_JI_CountryOfOrigin(true, CertificateTypeList.Codes.Code8, ControllingMessageTypeList.Codes.NX101, "");
			AssertCheckTW1_CertificateType_JI_CountryOfOrigin(true, CertificateTypeList.Codes.Code16, ControllingMessageTypeList.Codes.NX101, "");
			AssertCheckTW1_CertificateType_JI_CountryOfOrigin(true, CertificateTypeList.Codes.Code17, ControllingMessageTypeList.Codes.NX101, "");
			AssertCheckTW1_CertificateType_JI_CountryOfOrigin(false, CertificateTypeList.Codes.Code17, ControllingMessageTypeList.Codes.X101, "");
			AssertCheckTW1_CertificateType_JI_CountryOfOrigin(false, CertificateTypeList.Codes.Code17, ControllingMessageTypeList.Codes.NX101, "AU");
			AssertCheckTW1_CertificateType_JI_CountryOfOrigin(false, CertificateTypeList.Codes.Code18, ControllingMessageTypeList.Codes.NX101, "");
		}

		void AssertCheckTW1_CertificateType_JI_CountryOfOrigin(bool expectShowError, ZString certificateType, ZString messageType, ZString countryOfOrigin)
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var line1 = declaration.FilteredInvoiceLines.AddNew();
			var line2 = declaration.FilteredInvoiceLines.AddNew();
			line1.JI_CountryOfOrigin = countryOfOrigin;
			line2.JI_CountryOfOrigin = countryOfOrigin;

			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader.TW1_ControllingMessageType = messageType;
			messageHeader.TW1_CertificateType = certificateType;
			line1.JI_CEI = entryInstruction.PK;
			messageHeader.TW1_ControllingAgency = "XX";
			var link = line1.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(x => x.ControllingAgency == "XX");
			link.IsLinkedCMHeader = true;

			messageHeader.Validation.ValidateTW1_CertificateType();
			CombineAssertions(() =>
			{
				AssertEquals("line1 Goods Origin", expectShowError, line1.JI_CountryOfOriginInfo.HasMessageError(MandatoryValidation.YouHaveNotEnteredMessage("Goods Origin")));
				AssertEquals("line2 : no link to CMHeader", false, line2.JI_CountryOfOriginInfo.HasMessageError(MandatoryValidation.YouHaveNotEnteredMessage("Goods Origin")));
			});
		}

		public void TestCheckJI_CustomsSecondUnitQty()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TWPUM", "Taiwan Packing Units of Measurement");
			helper.CreateCusCodeList("TW", "TWPUM", "FAH", "Degree Fahrenheit", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var targetInfo = InvoiceLine.JI_CustomsSecondUnitQtyInfo;
			InvoiceLine.JI_Tariff = "0000000021";
			InvoiceLine.JI_CustomsSecondUnitQty = "QTI";
			AssertHasMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);

			InvoiceLine.JI_CustomsSecondUnitQty = "FAH";
			AssertNoMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckPackagingQTY()
		{
			const string expectedErrorMessage = "Please enter a 'Number of Package' greater than 0.";
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var controllingMessageHeaders = jobDeclaration.CusEntryInstruction.ControllingMessageHeaders;
			var msgHeader = controllingMessageHeaders.AddNew();

			var line = jobDeclaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			line.InvoiceLineLinkControllingMsgHeaders[0].IsLinkedCMHeader = true;

			var msgTypes = new string[] { ControllingMessageTypeList.Codes.NX301_DN, ControllingMessageTypeList.Codes.NX401, ControllingMessageTypeList.Codes.NX603 };
			var propertyInfo = line.JI_PackagingQTYInfo;
			CombineAssertions(() =>
			{
				foreach (var msgType in new ControllingMessageTypeList().GetAllCodes())
				{
					msgHeader.TW1_ControllingMessageType = msgType;
					line.JI_PackagingQTY = 0;
					if (msgTypes.Contains(msgType))
					{
						AssertHasMessageError($"{msgType}: JI_PackagingQTY is 0", propertyInfo, expectedErrorMessage);
						line.JI_PackagingQTY = 1;
						AssertNoMessageError($"{msgType}: JI_PackagingQTY is 1", propertyInfo, expectedErrorMessage);
					}
					else
					{
						AssertNoMessageError($"{msgType}: JI_PackagingQTY is 0", propertyInfo, expectedErrorMessage);
					}
				}
			});
		}

		public void TestCheckPackagingUQ()
		{
			var expectedNotEnteredErrorMessage = MandatoryValidation.YouHaveNotEnteredMessage("Packaging UQ");
			var expectedNotInListEnteredErrorMessage = ListValidation.InvalidCodeMessageError.ToString();

			new TestTWCreator(Factory).CreatePackagingUQ();
			Factory.Save();

			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var controllingMessageHeaders = jobDeclaration.CusEntryInstruction.ControllingMessageHeaders;
			var msgHeader = controllingMessageHeaders.AddNew();
			var line = jobDeclaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			line.InvoiceLineLinkControllingMsgHeaders[0].IsLinkedCMHeader = true;

			var msgTypes = new string[] { ControllingMessageTypeList.Codes.NX301_DN, ControllingMessageTypeList.Codes.NX401, ControllingMessageTypeList.Codes.NX603 };
			var propertyInfo = line.JI_PackagingUQInfo;
			CombineAssertions(() =>
			{
				foreach (var msgType in new ControllingMessageTypeList().GetAllCodes())
				{
					msgHeader.TW1_ControllingMessageType = msgType;
					line.JI_PackagingQTY = ZDecimal.Zero;
					line.JI_PackagingUQ = "UNK";
					AssertHasMessageError($"{msgType}: JI_PackagingUQ is UNK", propertyInfo, expectedNotInListEnteredErrorMessage);

					line.JI_PackagingQTY = 1M;
					line.JI_PackagingUQ = ZString.Empty;
					AssertHasMessageError($"{msgType}: JI_PackagingUQ is empty, but JI_PackagingQTY is not equal to 0", propertyInfo, expectedNotEnteredErrorMessage);

					if (msgTypes.Contains(msgType))
					{
						line.JI_PackagingQTY = ZDecimal.Zero;
						line.JI_PackagingUQ = ZString.Empty;
						AssertHasMessageError($"{msgType}: JI_PackagingUQ is Empty", propertyInfo, expectedNotEnteredErrorMessage);

						line.JI_PackagingUQ = "AMP";
						AssertNoMessageError($"{msgType}: JI_PackagingUQ is AMP", propertyInfo, expectedNotEnteredErrorMessage);
						AssertNoMessageError($"{msgType}: JI_PackagingUQ is AMP", propertyInfo, expectedNotInListEnteredErrorMessage);
					}
				}
			});
		}

		protected JobDeclaration Declaration => declaration ??= CreateNewDeclaration();
		JobDeclaration declaration;

		protected virtual JobDeclaration CreateNewDeclaration() => Factory.New<JobDeclaration>();

		protected JobComInvoiceHeader Invoice => invoice ??= Declaration.Invoices.AddNew();
		JobComInvoiceHeader invoice;

		protected CusEntryInstruction EntryInstruction => entryInstruction ??= Declaration.CustomsEntryInstructions.AddNew();
		protected CusEntryInstruction entryInstruction;

		protected JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					var instruction = EntryInstruction;
					invoiceLine = Invoice.JobComInvoiceLines.AddNew();
					InvoiceLine.JI_CEI = instruction.PK;
				}
				return invoiceLine;
			}
		}
		JobComInvoiceLine invoiceLine;
	}
}

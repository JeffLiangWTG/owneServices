using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ImportJobComInvoiceLineValidation))]
	sealed partial class ImportJobComInvoiceLineValidationTest : JobComInvoiceLineValidationAbstractTest<ImportJobComInvoiceLineValidation>
	{
		public void TestCheckJI_Model()
		{
			var wanringMessage = "System will automatically declare 'NIL' when 'Model' is empty.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.L1;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Model = "XX";
			AssertNoWarning(invoiceLine.JI_ModelInfo, wanringMessage);

			invoiceLine.JI_Model = ZString.Empty;
			AssertHasWarning(invoiceLine.JI_ModelInfo, wanringMessage);

			invoiceLine.JI_Model = "X2";
			AssertNoWarning(invoiceLine.JI_ModelInfo, wanringMessage);

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G7;
			invoiceLine.JI_Model = ZString.Empty;
			AssertNoWarning(invoiceLine.JI_ModelInfo, wanringMessage);
		}

		public void TestCheckCheckJI_ModelForImporterRegulation()
		{
			CreatetariffForTestImportRegulations();
			InvoiceLine.Declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			var targetInfo = InvoiceLine.JI_ModelInfo;
			var validation = InvoiceLine.Validation;
			InvoiceLine.JI_Tariff = "02089029204";
			validation.ValidateJI_Model();
			AssertHasWarning(targetInfo, "'Model' might be required when the goods is subject to F01 or F02 importer regulation.");

			InvoiceLine.JI_Tariff = "02089029206";
			validation.ValidateJI_Model();
			AssertNoWarning(targetInfo, "'Model' might be required when the goods is subject to F01 or F02 importer regulation.");

			InvoiceLine.JI_Tariff = "02089029210";
			validation.ValidateJI_Model();
			AssertHasWarning(targetInfo, "'Model' might be required when the goods is subject to F01 or F02 importer regulation.");
		}

		public void TestCheckJI_CompositionsForImporterRegulation()
		{
			CreatetariffForTestImportRegulations();
			InvoiceLine.Declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			var targetInfo = InvoiceLine.JI_CompositionsInfo;
			var validation = InvoiceLine.Validation;
			InvoiceLine.JI_Tariff = "02089029204";
			validation.ValidateJI_Compositions();
			AssertHasWarning(targetInfo, "'Specification' might be required when the goods is subject to F01 or F02 importer regulation.");

			InvoiceLine.JI_Tariff = "02089029206";
			validation.ValidateJI_Compositions();
			AssertNoWarning(targetInfo, "'Specification' might be required when the goods is subject to F01 or F02 importer regulation.");

			InvoiceLine.JI_Tariff = "02089029210";
			validation.ValidateJI_Compositions();
			AssertHasWarning(targetInfo, "'Specification' might be required when the goods is subject to F01 or F02 importer regulation.");
		}

		public override void TestCheckJI_BrandName()
		{
			base.TestCheckJI_BrandName();

			var wanringMessage = "System will automatically declare 'NIL' when 'Brand Name' is empty.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.L1;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_BrandName = "XX";
			AssertNoWarning(invoiceLine.JI_BrandNameInfo, wanringMessage);

			invoiceLine.JI_BrandName = ZString.Empty;
			AssertHasWarning(invoiceLine.JI_BrandNameInfo, wanringMessage);

			invoiceLine.JI_BrandName = "X2";
			AssertNoWarning(invoiceLine.JI_BrandNameInfo, wanringMessage);

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G7;
			invoiceLine.JI_BrandName = ZString.Empty;
			AssertNoWarning(invoiceLine.JI_BrandNameInfo, wanringMessage);
		}

		public void TestCheckJI_TariffAfterGeneratingEntry()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			var tariffIncludingT = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "2713200001", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, "T", tariffIncludingT);
			var tariffIncludingTPartially = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "2713200003", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, "T*", tariffIncludingTPartially);
			var tariffIncludingB = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "2713200002", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, "B", tariffIncludingB);
			var tariffIncludingBPartially = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "2713200004", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, "B*", tariffIncludingBPartially);
			var tariffIncludingC = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "2713200005", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, "C", tariffIncludingC);
			var tariffIncludingLPartially = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "2713200006", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, "L*", tariffIncludingLPartially);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var charge = entryHeader.DutyTaxFeeCharges.AddNew();

			CombineAssertions("T", () =>
			{
				var message = "The Invoice Line is subject to Commodity Tax when the customs requirement of tariff is 'T'.";
				charge.ChargeType = DutyTaxFeeCodeList.Codes.B29;
				invoiceLine.JI_Tariff = "2713200001";
				AssertHasMessageError(invoiceLine.JI_TariffInfo, message);

				charge.ChargeType = DutyTaxFeeCodeList.Codes.B10;
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertNoMessageError(invoiceLine.JI_TariffInfo, message);

				charge.ChargeType = DutyTaxFeeCodeList.Codes.B19;
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertNoMessageError(invoiceLine.JI_TariffInfo, message);
			});

			CombineAssertions("T*", () =>
			{
				var message = "The Invoice Line might be subject to Commodity Tax when the customs requirement of tariff is 'T*'.";
				charge.ChargeType = DutyTaxFeeCodeList.Codes.B29;
				invoiceLine.JI_Tariff = "2713200003";
				AssertHasWarning(invoiceLine.JI_TariffInfo, message);

				charge.ChargeType = DutyTaxFeeCodeList.Codes.B10;
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertNoWarning(invoiceLine.JI_TariffInfo, message);

				charge.ChargeType = DutyTaxFeeCodeList.Codes.B19;
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertNoWarning(invoiceLine.JI_TariffInfo, message);
			});

			CombineAssertions("B", () =>
			{
				var message = "The Invoice Line is subject to Tobacco and Alcohol Tax when the customs requirement of tariff is 'B' or 'C'.";
				charge.ChargeType = DutyTaxFeeCodeList.Codes.B29;
				invoiceLine.JI_Tariff = "2713200002";
				AssertHasMessageError(invoiceLine.JI_TariffInfo, message);

				charge.ChargeType = DutyTaxFeeCodeList.Codes.B31;
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertNoMessageError(invoiceLine.JI_TariffInfo, message);

				charge.ChargeType = DutyTaxFeeCodeList.Codes.B69;
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertNoMessageError(invoiceLine.JI_TariffInfo, message);
			});

			CombineAssertions("C", () =>
			{
				var message = "The Invoice Line is subject to Tobacco and Alcohol Tax when the customs requirement of tariff is 'B' or 'C'.";
				charge.ChargeType = DutyTaxFeeCodeList.Codes.B29;
				invoiceLine.JI_Tariff = "2713200005";
				AssertHasMessageError(invoiceLine.JI_TariffInfo, message);

				charge.ChargeType = DutyTaxFeeCodeList.Codes.B31;
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertNoMessageError(invoiceLine.JI_TariffInfo, message);

				charge.ChargeType = DutyTaxFeeCodeList.Codes.B69;
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertNoMessageError(invoiceLine.JI_TariffInfo, message);
			});

			CombineAssertions("B*", () =>
			{
				var message = "The Invoice Line might be subject to Tobacco and Alcohol Tax when the customs requirement of tariff is 'B*'.";
				charge.ChargeType = DutyTaxFeeCodeList.Codes.B29;
				invoiceLine.JI_Tariff = "2713200004";
				AssertHasWarning(invoiceLine.JI_TariffInfo, message);

				charge.ChargeType = DutyTaxFeeCodeList.Codes.B31;
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertNoWarning(invoiceLine.JI_TariffInfo, message);

				charge.ChargeType = DutyTaxFeeCodeList.Codes.B69;
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertNoWarning(invoiceLine.JI_TariffInfo, message);
			});

			CombineAssertions("L*", () =>
			{
				var message = "The Invoice Line might be subject to Specifically Selected Goods and Services Tax when the customs requirement of tariff is 'L*'.";
				charge.ChargeType = DutyTaxFeeCodeList.Codes.B29;
				invoiceLine.JI_Tariff = "2713200006";
				AssertHasWarning(invoiceLine.JI_TariffInfo, message);

				charge.ChargeType = DutyTaxFeeCodeList.Codes.B60;
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertNoWarning(invoiceLine.JI_TariffInfo, message);

				charge.ChargeType = DutyTaxFeeCodeList.Codes.B89;
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertNoWarning(invoiceLine.JI_TariffInfo, message);
			});
		}

		public void TestCheckJI_TariffHasImportRegulationOrExportRegulation()
		{
			CreatetariffForTestImportRegulations();
			InvoiceLine.Declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				InvoiceLine.JI_Tariff = "02089029204";
				AssertHasRowWarning(InvoiceLine, "The selected Tariff has Import Regulation 111, F01. Permit Number might be required. 'NIL' will be declared if no permit number is entered.");

				var permit1 = InvoiceLine.PermitCusSupportingCollection.AddNew();
				permit1.CSI_ReferenceNumber = "ref1";
				permit1.CSI_LineNo = 1;
				var permit2 = InvoiceLine.PermitCusSupportingCollection.AddNew();
				permit2.CSI_ReferenceNumber = "ref2";
				permit2.CSI_LineNo = 2;
				InvoiceLine.RunPreSaveValidation();
				AssertNoRowWarningContaining(InvoiceLine, "The selected Tariff has Import Regulation 111, F01. Permit Number might be required. 'NIL' will be declared if no permit number is entered.");

				InvoiceLine.PermitCusSupportingCollection.RemoveAndDeleteAll();
				InvoiceLine.JI_Tariff = "02089029206";
				AssertHasRowWarning(InvoiceLine, "The selected Tariff has Import Regulation 112. Permit Number might be required. 'NIL' will be declared if no permit number is entered.");

				var exemptionOfControllingAgency1 = InvoiceLine.ExemptionOfControllingAgenciesCusSupportings.AddNew();
				exemptionOfControllingAgency1.CSI_ReferenceNumber = "ref1";
				exemptionOfControllingAgency1.CSI_LineNo = 1;
				InvoiceLine.RunPreSaveValidation();
				AssertNoRowWarningContaining(InvoiceLine, "The selected Tariff has Import Regulation 112. Permit Number might be required. 'NIL' will be declared if no permit number is entered.");

				var permitCusSupportingCollection = InvoiceLine.PermitCusSupportingCollection.AddNew();
				permitCusSupportingCollection.CSI_ReferenceNumber = "ref2";
				permitCusSupportingCollection.CSI_LineNo = 2;
				InvoiceLine.RunPreSaveValidation();
				AssertHasRowWarning(InvoiceLine, "The selected Tariff has Import Regulation 112. Permit Number might be required. 'NIL' will be declared if no permit number is entered.");

				InvoiceLine.JI_Tariff = "02089029208";
				AssertHasRowWarning(InvoiceLine, "The selected Tariff doesn't have Import Regulation. Permit Number might not be required.");

				InvoiceLine.ExemptionOfControllingAgenciesCusSupportings.RemoveAndDeleteAll();
				InvoiceLine.PermitCusSupportingCollection.RemoveAndDeleteAll();
				InvoiceLine.JI_Tariff = "02089029208";
				AssertNoRowWarnings(InvoiceLine);

				InvoiceLine.JI_CountryOfOrigin = "CN";
				InvoiceLine.JI_Tariff = "02089029210";
				AssertHasRowWarning(InvoiceLine, "The selected Tariff has Import Regulation F02, MP1, MW0. Permit Number might be required. 'NIL' will be declared if no permit number is entered.");

				InvoiceLine.JI_CountryOfOrigin = "TW";
				InvoiceLine.RunPreSaveValidation();
				AssertHasRowWarning(InvoiceLine, "The selected Tariff has Import Regulation F02. Permit Number might be required. 'NIL' will be declared if no permit number is entered.");

				permit1 = InvoiceLine.PermitCusSupportingCollection.AddNew();
				permit1.CSI_ReferenceNumber = "ref1";
				permit1.CSI_LineNo = 1;

				InvoiceLine.RunPreSaveValidation();
				AssertNoRowWarnings(InvoiceLine);

				InvoiceLine.JI_CountryOfOrigin = "CN";
				InvoiceLine.RunPreSaveValidation();
				AssertHasRowWarning(InvoiceLine, "The selected Tariff has Import Regulation F02, MP1, MW0. Permit Number might be required. 'NIL' will be declared if no permit number is entered.");

				permit1 = InvoiceLine.PermitCusSupportingCollection.AddNew();
				permit1.CSI_ReferenceNumber = "ref2";
				permit1.CSI_LineNo = 2;

				permit1 = InvoiceLine.PermitCusSupportingCollection.AddNew();
				permit1.CSI_ReferenceNumber = "ref3";
				permit1.CSI_LineNo = 3;
				InvoiceLine.RunPreSaveValidation();
				AssertNoRowWarnings(InvoiceLine);
			});
		}

		public override void TestCheckJI_PreviousEntryNumber()
		{
			base.TestCheckJI_PreviousEntryNumber();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_Style = "G7";
			invoiceLine.JI_PreviousEntryNumber = "XXX";
			var targetInfo = invoiceLine.JI_PreviousEntryNumberInfo;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_PreviousEntryNumber = ZString.Empty;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			entryInstruction.CEI_Style = "G1";
			invoiceLine.JI_PreviousEntryNumber = "XXX";
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_PreviousEntryNumber = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_CustomsSecondQuantityAndUnit()
		{
			var info = InvoiceLine.JI_CustomsSecondQuantityInfo;
			var declaration = InvoiceLine.Declaration;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "2713200000", minDate, maxDate);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "2713200001", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, Constants.UniversalReferenceConstants.CusTariffAttributeValue.Z, tariff2);
			Factory.Save();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			InvoiceLine.JI_Tariff = tariff2.ZZ1_TariffCode;
			InvoiceLine.JI_CustomsSecondUnitQty = "ADT";
			InvoiceLine.JI_CustomsSecondQuantity = 0m;
			AssertHasWarningContaining(info, ValidationConstants.InvoiceLine.CustomsSecondQuantityWarningMessage);
			InvoiceLine.JI_CustomsSecondQuantity = 1m;
			AssertNoWarningContaining(info, ValidationConstants.InvoiceLine.CustomsSecondQuantityWarningMessage);
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			InvoiceLine.JI_CustomsSecondUnitQty = "ADT";
			InvoiceLine.JI_CustomsSecondQuantity = 0m;
			AssertNoWarningContaining(info, ValidationConstants.InvoiceLine.CustomsSecondQuantityWarningMessage);
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			InvoiceLine.JI_Tariff = tariff1.ZZ1_TariffCode;
			InvoiceLine.JI_CustomsSecondUnitQty = "ADT";
			InvoiceLine.Validation.ValidateJI_CustomsSecondQuantity();
			AssertNoWarningContaining(info, ValidationConstants.InvoiceLine.CustomsSecondQuantityWarningMessage);
			InvoiceLine.JI_Tariff = ZString.Empty;
			InvoiceLine.JI_CustomsSecondUnitQty = "ADT";
			InvoiceLine.Validation.ValidateJI_CustomsSecondQuantity();
			AssertNoWarningContaining(info, ValidationConstants.InvoiceLine.CustomsSecondQuantityWarningMessage);
			string messageError = "Please enter a 'Statistical Quantity' greater than 0.";
			AssertHasMessageErrorContaining(info, messageError);
			InvoiceLine.JI_CustomsSecondQuantity = 1m;
			AssertNoMessageErrorContaining(info, messageError);
			InvoiceLine.JI_Tariff = tariff2.ZZ1_TariffCode;
			InvoiceLine.JI_CustomsSecondQuantity = 0m;
			AssertHasWarningContaining(info, ValidationConstants.InvoiceLine.CustomsSecondQuantityWarningMessage);
			AssertNoMessageErrorContaining(info, messageError);
		}

		public void TestCustomsRequirements()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			var tariffIncludingT = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "2713200001", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, "T", tariffIncludingT);
			var tariffIncludingB = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "2713200002", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, "B", tariffIncludingB);
			var tariffIncludingC = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "2713200005", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, "C", tariffIncludingC);
			Factory.Save();
			var invoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertCustomsRequirementsMessageError(invoiceLine, "2713200001", "CT", ValidationConstants.InvoiceLine.CustomsRequirementT);
			AssertCustomsRequirementsMessageError(invoiceLine, "2713200002", "AT", ValidationConstants.InvoiceLine.CustomsRequirementB);
			AssertCustomsRequirementsMessageError(invoiceLine, "2713200005", "TT", ValidationConstants.InvoiceLine.CustomsRequirementC);
		}

		void AssertCustomsRequirementsMessageError(JobComInvoiceLine invoiceLine, ZString tariffCode, ZString tariffType, ZString message)
		{
			invoiceLine.Taxes.RemoveAndDeleteAll();
			invoiceLine.JI_Tariff = tariffCode;
			AssertHasMessageError(invoiceLine.JI_TariffInfo, message);
			var invoiceLineTax1 = invoiceLine.Taxes.AddNew();
			invoiceLineTax1.JLT_Type = tariffType;
			invoiceLineTax1.JLT_Tariff = "XXX";
			invoiceLine.RunPreSaveValidation();
			AssertNoMessageError(invoiceLine.JI_TariffInfo, message);
		}

		public void TestCheckPreviousPermitNo()
		{
			var messageError = "The length of Previous Permit Number must be 14 characters.";
			var invoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			invoiceLine.PreviousPermitNo = "XXXXXXXXXXXXXX";
			AssertNoMessageError(invoiceLine.PreviousPermitNoInfo, messageError);
			invoiceLine.PreviousPermitNo = "XXX";
			AssertHasMessageError(invoiceLine.PreviousPermitNoInfo, messageError);
			invoiceLine.PreviousPermitNo = ZString.Empty;
			AssertEquals(false, invoiceLine.PreviousPermitNoInfo.HasMessageErrors());
			invoiceLine.Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			invoiceLine.PreviousPermitNo = "XXX";
			AssertNoMessageError(invoiceLine.PreviousPermitNoInfo, messageError);
			invoiceLine.Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			invoiceLine.PreviousPermitNo = "\u5927\u5ba2\u8eca";
			AssertHasError(invoiceLine.PreviousPermitNoInfo, EnglishCharactersValidation.GetNotificationMessage(invoiceLine.PreviousPermitNoInfo));
		}

		public void TestValidateFoodDataMaximumRows()
		{
			var invoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var row1 = invoiceLine.FoodDataCollection.AddNew();
			invoiceLine.Validation.ValidateAll();
			AssertNoRowError(invoiceLine, ValidationConstants.InvoiceLine.FoodContentMaximumRows);
			for (var i = 0; i < 99; i++)
			{
				invoiceLine.FoodDataCollection.AddNew();
			}

			invoiceLine.Validation.ValidateAll();
			AssertHasRowMessageError(invoiceLine, ValidationConstants.InvoiceLine.FoodContentMaximumRows);
		}

		public void TestCheckJI_CountryOfOrigin_Import()
		{
			var invoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var info = invoiceLine.JI_CountryOfOriginInfo;
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.Code = "AU";
			invoiceLine.JI_CountryOfOrigin = ZString.Empty;
			invoiceLine.Validation.ValidateAll();
			AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_CountryOfOrigin = "XX";
			invoiceLine.Validation.ValidateAll();
			AssertHasMessageError(info, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_CountryOfOrigin = country.Code;
			invoiceLine.Validation.ValidateAll();
			AssertNoMessageErrors(info);
		}

		public void TestCheckDeclarationGoodsDescriptionLengthWhenReportingAircraftParts()
		{
			var errorMessage = "Declaration Goods Description must be equal to or less than 512 characters when reporting aircraft parts.";
			var invoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var jobTWComInvoiceLine = invoiceLine.AddInfoChild;
			jobTWComInvoiceLine.TWL_AircraftPartsCategory = "6";
			jobTWComInvoiceLine.TWL_AircraftPartsCode = "2";
			jobTWComInvoiceLine.TWL_AircraftIPC = "49-22-11";
			var currentLength = invoiceLine.JI_DeclarationGoodsDescription.Length;
			invoiceLine.JI_Description = ZString.Replicate('A', 512 - currentLength);
			invoiceLine.Validation.ValidateJI_DeclarationGoodsDescription();
			AssertNoMessageError(invoiceLine.JI_DeclarationGoodsDescriptionInfo, errorMessage);

			invoiceLine.JI_Description += "A";
			invoiceLine.Validation.ValidateJI_DeclarationGoodsDescription();
			AssertHasMessageError(invoiceLine.JI_DeclarationGoodsDescriptionInfo, errorMessage);

			invoiceLine.Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			invoiceLine.Validation.ValidateJI_DeclarationGoodsDescription();
			AssertNoMessageError(invoiceLine.JI_DeclarationGoodsDescriptionInfo, errorMessage);
		}

		public void TestCheckJI_Procedure_NonDutyFreeTariff()
		{
			var hsnTariffType = UniversalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "HSN");
			var rateType = UniversalTestHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Taiwan, "DTY", "Duty");
			var rateCodeDTA = UniversalTestHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.DTA, rateType.PK);
			var rateCodeDTS = UniversalTestHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.DTS, rateType.PK);
			var preference = UniversalTestHelper.CreatePreferenceForCountry("PR1", "PR1", Core.Constants.CountryCodes.Taiwan);
			var tradeGroup = UniversalTestHelper.CreateTradeGroup("TW", "US", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			UniversalTestHelper.AddCountry(tradeGroup, "US", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();

			var tariffDTA = UniversalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "21039090200", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rateDTA = UniversalTestHelper.CreateRate(tariffDTA, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.15 * VFD", preference.PK, "0.15", Core.Constants.CountryCodes.Taiwan);
			UniversalTestHelper.CreateCusApplicability(rateDTA, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var tariff201 = UniversalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "21039090201", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariff21039090202 = UniversalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "21039090202", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rate21039090202 = UniversalTestHelper.CreateRate(tariff21039090202, rateCodeDTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.16 * VFD", preference.PK, "0.16", Core.Constants.CountryCodes.Taiwan);
			UniversalTestHelper.CreateCusApplicability(rate21039090202, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var tariffDTA203 = UniversalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "21039090203", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rateDTA203 = UniversalTestHelper.CreateRate(tariffDTA203, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0 * VFD", preference.PK, "0", Core.Constants.CountryCodes.Taiwan);
			UniversalTestHelper.CreateCusApplicability(rateDTA203, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();

			var declaration = CreateNewDeclaration();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "21039090200";
			invoiceLine.JI_CountryOfOrigin = "US";
			invoiceLine.JI_PrimaryPreference = "PR1";
			invoiceLine.JI_Procedure = Constants.ProcedureCodes._50;
			CombineAssertions(() =>
			{
				AssertHasMessageError("Show error when Ad_valorem duty rate is not 0%", invoiceLine.JI_ProcedureInfo, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForNonDutyFreeTariff);

				invoiceLine.JI_Tariff = "21039090202";
				AssertHasMessageError("Show error specific duty rate is not empty", invoiceLine.JI_ProcedureInfo, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForNonDutyFreeTariff);

				invoiceLine.JI_Tariff = "21039090203";
				AssertNoMessageError("No error for Ad_valorem duty rate is 0%", invoiceLine.JI_ProcedureInfo, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForNonDutyFreeTariff);

				declaration.JE_MessageType = "EXP";
				AssertNoMessageError("No error for Export", invoiceLine.JI_ProcedureInfo, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForNonDutyFreeTariff);

				declaration.JE_MessageType = "IMP";
				invoiceLine.JI_Tariff = "21039090201";
				AssertHasMessageError("Show error specific duty rate is not 0%", invoiceLine.JI_ProcedureInfo, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForNonDutyFreeTariff);

				invoiceLine.JI_Tariff = "21039090202";
				invoiceLine.JI_Procedure = Constants.ProcedureCodes._31;
				AssertNoMessageError(invoiceLine.JI_ProcedureInfo, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForNonDutyFreeTariff);
			});
		}

		public void TestCheckJI_DtyPymntMthd()
		{
			AssertCheckPaymentMethod(invoiceLine => invoiceLine.JI_DtyPymntMthdInfo);
		}

		public void TestCheckJI_VatPymntMthd()
		{
			AssertCheckPaymentMethod(invoiceLine => invoiceLine.JI_VatPymntMthdInfo);
		}

		public void TestCheckJI_TpfPymntMthd()
		{
			AssertCheckPaymentMethod(invoiceLine => invoiceLine.JI_TpfPymntMthdInfo);
		}

		void AssertCheckPaymentMethod(Func<JobComInvoiceLine, ZPropertyInfo> getPropertyInfo)
		{
			var declaration = CreateNewDeclaration();
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_RORPaymentMethod = RORPaymentMethodList.Codes.RorPayment;

			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var targetInfo = getPropertyInfo(invoiceLine);
			var warningMessage = $"The selected {targetInfo.HumanReadableName} is different from the ROR Payment Method under Declaration > Entry Details.";
			invoiceLine.JI_Procedure = Constants.ProcedureCodes._38;
			targetInfo.Value = (ZString)DutyTaxPaymentMethodList.Codes.RorPayment;
			AssertNoWarning(targetInfo, warningMessage);

			targetInfo.Value = (ZString)DutyTaxPaymentMethodList.Codes.NonCashPayment;
			AssertHasWarning(targetInfo, warningMessage);

			invoiceLine.JI_Procedure = Constants.ProcedureCodes._39;
			targetInfo.Value = (ZString)DutyTaxPaymentMethodList.Codes.CashPayment;
			AssertNoWarning(targetInfo, warningMessage);
		}

		void CreatetariffForTestImportRegulations()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();

			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			var tariff_02089029204 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "02089029204", minDate, maxDate);
			var tariff_02089029204_Attribute_IMP_111 = helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportRegulations, "111", tariff_02089029204);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWImportRegulations, tariff_02089029204_Attribute_IMP_111.ZZ3_Value, "管制輸入。", minDate, maxDate);

			var tariff_02089029204_Attribute_IMP_F01 = helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportRegulations, "F01", tariff_02089029204);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWImportRegulations, tariff_02089029204_Attribute_IMP_F01.ZZ3_Value, "輸入商品應依照「食品及相關產品輸入查驗辦法」規定，向衛生福利部食品藥物管理署申請辦理輸入查驗。【註：相關規定應洽衛生福利部食品藥物管理署】。", minDate, maxDate);

			var tariff_02089029204_Attribute_EXP_111 = helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ExportRegulations, "111", tariff_02089029204);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWExportRegulations, tariff_02089029204_Attribute_EXP_111.ZZ3_Value, "管制輸出。", minDate, maxDate);

			var tariff_02089029206 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "02089029206", minDate, maxDate);
			var tariff_02089029206_Attribute_IMP_112 = helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportRegulations, "112", tariff_02089029206);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWImportRegulations, tariff_02089029206_Attribute_IMP_112.ZZ3_Value, "大陸物品不准輸入。", minDate, maxDate);

			var tariff_02089029208 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "02089029208", minDate, maxDate);

			var tariff_02089029210 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "02089029210", minDate, maxDate);
			var tariff_02089029210_Attribute_IMP_MP1 = helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportRegulations, "MP1", tariff_02089029210);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWImportRegulations, tariff_02089029210_Attribute_IMP_MP1.ZZ3_Value, "大陸物品有條件准許輸入。", minDate, maxDate);

			var tariff_02089029210_Attribute_IMP_F02 = helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportRegulations, "F02", tariff_02089029210);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWImportRegulations, tariff_02089029210_Attribute_IMP_F02.ZZ3_Value, "本項下商品如屬食品、食品器具、食品容器或包裝、食品用洗潔劑等食品相關用途或含有前述物品者，應依照「食品及相關產品輸入查驗辦法」規定，向衛生福利部食品藥物管理署申請辦理輸入查驗。【註：相關規定應洽衛生福利部食品藥物管理署】。", minDate, maxDate);

			var tariff_02089029210_Attribute_IMP_MW0 = helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportRegulations, "MW0", tariff_02089029210);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWImportRegulations, tariff_02089029210_Attribute_IMP_MW0.ZZ3_Value, "大陸物品不准輸入。", minDate, maxDate);
			Factory.Save();
		}

		protected override JobDeclaration CreateNewDeclaration()
		{
			var declaration = base.CreateNewDeclaration();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			return declaration;
		}

		UniversalReferenceTestDataHelper UniversalTestHelper => universalTestHelper ?? (universalTestHelper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper universalTestHelper;
	}
}

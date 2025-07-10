using System;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class JobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestJE_CEIRequired()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			invoiceLine.JI_CEI = ZGuid.Empty;
			AssertEquals("IsNoEntryInstruction", true, declaration.CustomsEntryInstructionProvider.IsNoEntryInstruction);
			AssertNoErrors("Entry Instruction is not required when not supported", invoiceLine.JI_CEIInfo);

			declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			invoiceLine.JI_CEI = ZGuid.Empty;
			AssertHasMessageError("Entry Instruction is required", invoiceLine.JI_CEIInfo, "Entry Instruction should be selected on an Invoice Line");
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			invoiceLine.Validation.ValidateJI_CEI();
			AssertNoMessageError("Entry Instruction is not required for ITF", invoiceLine.JI_CEIInfo, "Entry Instruction should be selected on an Invoice Line");

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.MakeNonPersistent();
			invoiceLine.Validation.ValidateJI_CEI();
			AssertNoErrors("Entry Instruction is not required for StandAlone", invoiceLine.JI_CEIInfo);

			invoice.JZ_JE = ZGuid.Invalid;
			invoiceLine.Validation.ValidateJI_CEI();
			AssertNoErrors("Entry Instruction is not required for StandAlone", invoiceLine.JI_CEIInfo);
		}

		public void TestCheckJI_IsClassUsageCommentRead()
		{
			var expectedNotification = "Usage Comment exist, please confirm these have been read by ticking the 'Is Usage Comment Read?' field.";

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			void AssertHasNotificationForUsageComment(string action)
			{
				using (CustomsDataRegistry.Instance.SeverityLevelOfUsageCommentValidation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, action))
				{
					var validation = (BaseJobComInvoiceLineValidation)invoiceLine.Validation;
					invoiceLine.JI_ClassUsageComment = string.Empty;
					invoiceLine.JI_IsClassUsageCommentRead = false;

					validation.ValidateJI_IsClassUsageCommentRead();
					AssertNoNotifications("Should not has any validation message when the Usage Comment is empty.", invoiceLine.JI_IsClassUsageCommentReadInfo);

					invoiceLine.JI_ClassUsageComment = "Test Comment";
					validation.ValidateJI_IsClassUsageCommentRead();

					switch (action)
					{
						case ProductAuditActions.Codes.NoAction:
							{
								AssertNoNotifications("Should not has any validation notifications when the value of SeverityLevelOfUsageCommentValidation is NON.", invoiceLine.JI_IsClassUsageCommentReadInfo);
								break;
							}

						case ProductAuditActions.Codes.AddWarningValidation:
							{
								AssertHasWarning($"Should has the expected warning when the value of SeverityLevelOfUsageCommentValidation is {action}.", invoiceLine.JI_IsClassUsageCommentReadInfo, expectedNotification);
								break;
							}

						case ProductAuditActions.Codes.AddMessageErrorValidation:
							{
								AssertHasMessageError($"Should has the expected message error when the value of SeverityLevelOfUsageCommentValidation is {action}.", invoiceLine.JI_IsClassUsageCommentReadInfo, expectedNotification);
								break;
							}
					}

					invoiceLine.JI_IsClassUsageCommentRead = true;
					AssertNoNotifications("Should not has any validation notifications when the IsClassUsageCommentRead is true.", invoiceLine.JI_IsClassUsageCommentReadInfo);
				}
			}

			foreach (var actionCode in new ProductAuditActions().GetAllCodes())
			{
				AssertHasNotificationForUsageComment(actionCode);
			}
		}

		public void TestJI_TariffNotMatchedToProductField()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.FillWithValidTestData();
			consignor.OH_Code = "ORGAUSYD";
			consignor.OH_IsConsignor = ZBool.True;
			consignor.OH_RL_NKClosestPort = "AUSYD";
			var consignee = Factory.New<OrgHeader>();
			consignee.FillWithValidTestData();
			consignee.OH_Code = "ORGUSCHI";
			consignee.OH_IsConsignee = ZBool.True;
			consignee.OH_RL_NKClosestPort = "USCHI";

			var foreignExporterOnPart = Factory.New<OrgHeader>();
			foreignExporterOnPart.FillWithValidTestData();
			foreignExporterOnPart.OH_Code = "FREXPART";
			foreignExporterOnPart.MainAddress.AddressCode = "2213";

			var foreignExporter = Factory.New<OrgHeader>();
			foreignExporter.FillWithValidTestData();
			foreignExporter.OH_Code = "FRNEXPER";
			foreignExporter.MainAddress.AddressCode = "2000";

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "WEIGHTTEST";
			product.OP_Desc = "WEIGHTTEST";
			product.RelatedOrganisations.AddOwner(importer);
			product.OP_Weight = 10;
			product.OP_WeightUQ = Core.Constants.Weight.Kilograms;
			product.OP_NetWeight = 9;
			product.OP_StockKeepingUnit = "UNT";

			var supplierRelation = product.RelatedOrganisations.AddSupplier(consignor);
			product.RelatedOrganisations.AddOwner(consignee);

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "9101000010";
			pivot.CI_SupplementalTariff = "9101000011";
			pivot.CI_OH = supplierRelation.OU_OH;
			pivot.CI_RN_NKCountry = "ER";
			Factory.Save();
			var msg = "Tariff does not match product code file.";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Buyer = consignee.PK;
			invoice.JZ_OH_Supplier = consignor.PK;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = product.OP_PartNum;
			invoiceLine.SetPartForTesting(product);

			using (CustomsDataRegistry.Instance.SeverityLevelOfInvoiceLineValidationAgainstProductData.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, ProductAuditActions.Codes.AddWarningValidation))
			{
				var validation = (BaseJobComInvoiceLineValidation)invoiceLine.Validation;
				invoiceLine.JI_Tariff = "9201000010";
				AssertHasWarning(invoiceLine.JI_TariffInfo, msg);

				invoiceLine.JI_Tariff = "9101000010";
				AssertNoWarning(invoiceLine.JI_TariffInfo, msg);
			}

			using (CustomsDataRegistry.Instance.SeverityLevelOfInvoiceLineValidationAgainstProductData.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, ProductAuditActions.Codes.AddMessageErrorValidation))
			{
				var validation = (BaseJobComInvoiceLineValidation)invoiceLine.Validation;
				invoiceLine.JI_PartNo = product.OP_PartNum;
				invoiceLine.JI_Tariff = "9301000010";
				AssertHasMessageError(invoiceLine.JI_TariffInfo, msg);

				invoiceLine.JI_Tariff = "9101000010";
				AssertNoMessageError(invoiceLine.JI_TariffInfo, msg);
			}
		}

		public void TestCheckJI_MatchingKey()
		{
			var expectedMessage = "Duplicate matching key is entered on other line which is also associated with the same invoice header.";

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV001";

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "INV002";

			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_MatchingKey = "MK000";

			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_MatchingKey = "MK000";

			AssertNoError("Should not have the expected error as these invoice lines dont belong to a invoice header.", invoiceLine1.JI_MatchingKeyInfo, expectedMessage);
			AssertNoError("Should not have the expected error as these invoice lines dont belong to a invoice header.", invoiceLine2.JI_MatchingKeyInfo, expectedMessage);

			var invoiceLine3 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_MatchingKey = "MK000";

			invoiceLine1.Validation.ValidateJI_MatchingKey();
			invoiceLine2.Validation.ValidateJI_MatchingKey();
			invoiceLine3.Validation.ValidateJI_MatchingKey();

			AssertNoError("Should not have the expected error as these invoice lines dont belong to a invoice header.", invoiceLine1.JI_MatchingKeyInfo, expectedMessage);
			AssertHasError("Should have the expected error as invoiceLine1 and invoiceLine3 belong to same invoice header.", invoiceLine2.JI_MatchingKeyInfo, expectedMessage);
			AssertHasError("Should have the expected error as invoiceLine1 and invoiceLine3 belong to same invoice header.", invoiceLine3.JI_MatchingKeyInfo, expectedMessage);

			invoiceLine3.JI_MatchingKey = "MK001";

			invoiceLine1.Validation.ValidateJI_MatchingKey();
			invoiceLine2.Validation.ValidateJI_MatchingKey();
			invoiceLine3.Validation.ValidateJI_MatchingKey();

			AssertNoError("Should not have the expected error as these invoice lines dont belong to a invoice header.", invoiceLine1.JI_MatchingKeyInfo, expectedMessage);
			AssertNoError("Should not have the expected error as these matching keys are unique.", invoiceLine2.JI_MatchingKeyInfo, expectedMessage);
			AssertNoError("Should not have the expected error as these matching keys are unique.", invoiceLine3.JI_MatchingKeyInfo, expectedMessage);
		}

		public void TestCheckJI_ZZF_NKTaxTypeWhenVATNotEmpty()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateRefCusProcedure(currentCountry, "A", "11", "11", "111", "One", "IMP", group: "IFD");
			procedure1.ZZ6_CalculateVAT = false;
			Factory.Save();

			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_MessageType = "IMP";

			var header = dec.Invoices.AddNew();
			var line = header.InvoiceLines.AddNew();
			line.JI_Procedure = "1111111";
			line.JI_ZZF_NKTaxType = "605";
			AssertHasWarning(line.JI_ZZF_NKTaxTypeInfo, "Procedure 1111111 indicates that VAT does not apply, but value 605 in this field means that VAT is calculated; To disable the calculation set this field's value to blank.");

			procedure1.ZZ6_CalculateVAT = true;
			Factory.Save();
			line.Validation.ValidateJI_ZZF_NKTaxType();
			AssertNoWarning(line.JI_ZZF_NKTaxTypeInfo, "Procedure 1111111 indicates that VAT does not apply, but value 605 in this field means that VAT is calculated; To disable the calculation set this field's value to blank.");
		}

		public void TestCheckJI_Tariff()
		{
			var date1 = new ZDate(2010, 12, 10);
			var date4 = new ZDate(2079, 06, 06);

			var testHelper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var tradeGroupStandard = testHelper.CreateTradeGroup(currentCountryCode, "STANDARD", date1, date4);
			testHelper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Australia, date1, date4);
			Factory.Save();

			var hsnTariffType = testHelper.CreateNewOrGetExistingTariffType(currentCountryCode, "HSN");
			Factory.Save();
			var cusTariff = testHelper.CreateTariff(currentCountryCode, hsnTariffType.PK, "DUMMYTRF", date1, date4, "dummy Description 0");
			var dutyRateType = testHelper.CreateNewOrGetExistingRateType(currentCountryCode, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode1 = testHelper.LoadOrCreateNewCusRateCode(Factory, "RC1", dutyRateType.PK);
			var rateCode2 = testHelper.LoadOrCreateNewCusRateCode(Factory, "RC2", dutyRateType.PK);
			var preferenceSTD = testHelper.CreatePreferenceForCountry("STD", "Standard", currentCountryCode);
			var preferenceTWO = testHelper.CreatePreferenceForCountry("TWO", "TWO Matched", currentCountryCode);
			var preferenceZRO = testHelper.CreatePreferenceForCountry("ZRO", "ZERO Matched", currentCountryCode);
			Factory.Save();

			var testRate1 = testHelper.CreateRate(cusTariff, rateCode1.PK, date1, date4, "0", preferencePk: preferenceSTD.PK);
			testHelper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date4, orderNumber: "Order1");
			var testRate2 = testHelper.CreateRate(cusTariff, rateCode1.PK, date1, date4, "0", preferencePk: preferenceTWO.PK);
			testHelper.CreateCusApplicability(testRate2, tradeGroupStandard, date1, date4);
			var testRate3 = testHelper.CreateRate(cusTariff, rateCode1.PK, date1, date4, "0", preferencePk: preferenceTWO.PK);
			testHelper.CreateCusApplicability(testRate3, tradeGroupStandard, date1, date4);
			var testRate4 = testHelper.CreateRate(cusTariff, rateCode2.PK, date1, date4, "0", preferencePk: preferenceSTD.PK);
			testHelper.CreateCusApplicability(testRate4, tradeGroupStandard, date1, date4);
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "DUMMYTRF";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;

			invoiceLine.JI_PrimaryPreference = "STD";
			AssertNoMessageErrors("Single Result", invoiceLine.JI_TariffInfo);
			invoiceLine.JI_PrimaryPreference = "TWO";
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, $"There is more than one applicable Duty rate with rate code RC1 for the Tariff 'DUMMYTRF' and Country Of Origin 'AU' as at {invoiceLine.EffectiveAssessmentDate} in combination with other data entered on the form.");
			invoiceLine.JI_PrimaryPreference = "ZRO";
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, $"There is no applicable Duty rate for the Tariff 'DUMMYTRF' and Country Of Origin 'AU' as at {invoiceLine.EffectiveAssessmentDate} in combination with other data entered on the form.");
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "Valid Duty rates exist where\r\n1: Preference = STD\r\n2: Preference = STD and Order Number = Order1\r\n3: Preference = TWO\r\n");

			invoiceLine.JI_PrimaryPreference = "STD";
			AssertNoMessageErrors("Single Result", invoiceLine.JI_TariffInfo);
			invoiceLine.JI_ConcessionOrder = "TT";
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, $"There is no applicable Duty rate for the Tariff 'DUMMYTRF' and Country Of Origin 'AU' as at {invoiceLine.EffectiveAssessmentDate} in combination with other data entered on the form.");
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "Valid Duty rates exist where\r\n1: Preference = STD\r\n");

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Canada;
			AssertNoMessageErrors("No rate for Canada", invoiceLine.JI_TariffInfo);

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, $"There is no applicable Duty rate for the Tariff 'DUMMYTRF' and Country Of Origin 'AU' as at {invoiceLine.EffectiveAssessmentDate} in combination with other data entered on the form.");
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageErrors("does not do rate validation for Export declaration", invoiceLine.JI_TariffInfo);
		}

		public void TestCheckJI_Tariff_WithSecondTradeGroup()
		{
			var date1 = new ZDate(2010, 12, 10);
			var date4 = new ZDate(2079, 06, 06);

			var testHelper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var tradeGroupStandard = testHelper.CreateTradeGroup(currentCountryCode, "STANDARD", date1, date4);
			testHelper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Australia, date1, date4);
			var tradeGroupReduced = testHelper.CreateTradeGroup(currentCountryCode, "REDUCED", date1, date4);
			Factory.Save();

			var hsnTariffType = testHelper.CreateNewOrGetExistingTariffType(currentCountryCode, "HSN");
			Factory.Save();
			var cusTariff = testHelper.CreateTariff(currentCountryCode, hsnTariffType.PK, "DUMMYTRF", date1, date4, "dummy Description 0");
			var dutyRateType = testHelper.CreateNewOrGetExistingRateType(currentCountryCode, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode1 = testHelper.LoadOrCreateNewCusRateCode(Factory, "RC1", dutyRateType.PK);
			var rateCode2 = testHelper.LoadOrCreateNewCusRateCode(Factory, "RC2", dutyRateType.PK);
			var preferenceSTD = testHelper.CreatePreferenceForCountry("STD", "Standard", currentCountryCode);
			var preferenceTWO = testHelper.CreatePreferenceForCountry("TWO", "TWO Matched", currentCountryCode);
			var preferenceZRO = testHelper.CreatePreferenceForCountry("ZRO", "ZERO Matched", currentCountryCode);
			Factory.Save();

			var testRate1 = testHelper.CreateRate(cusTariff, rateCode1.PK, date1, date4, "0", preferencePk: preferenceSTD.PK);
			testHelper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date4, orderNumber: "Order1", secondTradeGroup: tradeGroupStandard);
			var testRate2 = testHelper.CreateRate(cusTariff, rateCode1.PK, date1, date4, "0", preferencePk: preferenceTWO.PK);
			testHelper.CreateCusApplicability(testRate2, tradeGroupStandard, date1, date4, orderNumber: "Order2", secondTradeGroup: tradeGroupStandard);
			var testRate3 = testHelper.CreateRate(cusTariff, rateCode1.PK, date1, date4, "0", preferencePk: preferenceTWO.PK);
			testHelper.CreateCusApplicability(testRate3, tradeGroupStandard, date1, date4, secondTradeGroup: tradeGroupReduced);
			var testRate4 = testHelper.CreateRate(cusTariff, rateCode2.PK, date1, date4, "0", preferencePk: preferenceZRO.PK);
			testHelper.CreateCusApplicability(testRate4, tradeGroupStandard, date1, date4);
			var testRate5 = testHelper.CreateRate(cusTariff, rateCode2.PK, date1, date4, "0");
			testHelper.CreateCusApplicability(testRate5, tradeGroupStandard, date1, date4, secondTradeGroup: tradeGroupStandard);
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoiceLine.JI_Tariff = "DUMMYTRF";
			string error = $"There is no applicable Duty rate for the Tariff 'DUMMYTRF' and Country Of Origin 'AU' as at {invoiceLine.EffectiveAssessmentDate} in combination with other data entered on the form.";
			CombineAssertions(() =>
			{
				AssertHasMessageErrorContaining("no applicable Duty rate", invoiceLine.JI_TariffInfo, error);
				AssertHasMessageErrorContaining("All Valid criterias for applicable Duty rate", invoiceLine.JI_TariffInfo,
					@"Valid Duty rates exist where
1: Application Territory = STANDARD
2: Preference = STD and Order Number = Order1 and Application Territory = STANDARD
3: Preference = TWO and Application Territory = REDUCED
4: Preference = TWO and Order Number = Order2 and Application Territory = STANDARD
5: Preference = ZRO");

				invoiceLine.JI_ConcessionOrder = "Order1";
				AssertHasMessageErrorContaining("no applicable Duty rate", invoiceLine.JI_TariffInfo, error);
				AssertHasMessageErrorContaining("Valid criterias where Order Number = Order1 or regardless Order Number", invoiceLine.JI_TariffInfo,
					@"Valid Duty rates exist where
1: Application Territory = STANDARD
2: Preference = STD and Order Number = Order1 and Application Territory = STANDARD");

				invoiceLine.JI_PrimaryPreference = "ZRO";
				AssertHasMessageErrorContaining("no applicable Duty rate", invoiceLine.JI_TariffInfo, error);
				AssertHasMessageErrorContaining("Valid criterias where Preference = ZRO", invoiceLine.JI_TariffInfo,
@"Valid Duty rates exist where
1: Application Territory = STANDARD");
			});
		}

		public void TestCheckJI_Tariff_ValidUniversalTariff()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			if (invoiceLine.UseUniversalTariff)
			{
				var tariffFormatter = invoiceLine.GetType().GetProperty("TariffFormatter", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(invoiceLine, null) as TariffFormatter;
				var validUniversalTariffCode = tariffFormatter.Format("20064000004").Left(invoiceLine.JI_TariffInfo.MaxLength);
				SetupUniversalTariff(validUniversalTariffCode, invoiceLine.UniversalTariffType);

				CombineAssertions(() =>
				{
					invoiceLine.JI_Tariff = "10064000004";
					var error = invoiceLine.Validation.GetType().GetProperty("UniversalTariffNotExistedError", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(invoiceLine.Validation, null);
					AssertHasMessageError("Invalid Tariff", invoiceLine.JI_TariffInfo, (ZString)error);

					invoiceLine.JI_Tariff = validUniversalTariffCode;
					AssertNoMessageErrors("Valid Tariff", invoiceLine.JI_TariffInfo);
				});
			}
			else
			{
				Assert(true);
			}
		}

		[TestDate(2016, 5, 5)]
		public void TestCheckJI_Tariff_CheckConditionsAreMet()
		{
			var tariff = SetupTariff();
			SetupRateConditionData(tariff);
			SetupFormulaConditionData(tariff);
			SetupConditionData(tariff);

			var invoiceLine = CreateInvoiceLine();

			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, @"The Customs Control condition is not satisfied
    Test Ctrl Condition Type 1:
        AdditionalCode: AC, OrderNumber: ON: (([KGM] < 2000.00) or ([NO] > 2.00)) and (([NO] > 20.00) or ([TNE] < 20.00)) AND
        Direction:Either: YBDesc AND
        Direction:Import: (XADesc or XBDesc) and XCDesc and (XDDesc or XEDesc)
            (Please refer to: www.google.com)
    Test Ctrl Condition Type 2:
        cond2_1: ZADesc

The Rate condition is not satisfied
    Test Rate Condition Type 1:
        rate1_1: R1Desc or R2Desc");

			invoiceLine.JI_CustomsQuantity = 200;
			invoiceLine.JI_CustomsUnitQty = "NO";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, @"The Customs Control condition is not satisfied
    Test Ctrl Condition Type 1:
        Direction:Either: YBDesc AND
        Direction:Import: (XADesc or XBDesc) and XCDesc and (XDDesc or XEDesc)
            (Please refer to: www.google.com)
    Test Ctrl Condition Type 2:
        cond2_1: ZADesc

The Rate condition is not satisfied
    Test Rate Condition Type 1:
        rate1_1: R1Desc or R2Desc");
		}

		[TestDate(2016, 5, 5)]
		public void TestCheckJI_Tariff_CheckConditionsAreMet_FormulaCheck()
		{
			var tariff = SetupTariff();
			SetupFormulaConditionData(tariff);

			var invoiceLine = CreateInvoiceLine();
			invoiceLine.JI_CustomsQuantity = 200;
			invoiceLine.JI_CustomsUnitQty = "NO";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageErrors(invoiceLine.JI_TariffInfo);

			invoiceLine.JI_CustomsQuantity = 10;
			invoiceLine.JI_CustomsUnitQty = "NO";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, @"The Customs Control condition is not satisfied
    Test Ctrl Condition Type 1:
        AdditionalCode: AC, OrderNumber: ON: (([KGM] < 2000.00) or ([NO] > 2.00)) and (([NO] > 20.00) or ([TNE] < 20.00))");

			invoiceLine.JI_CustomsSecondQuantity = 10;
			invoiceLine.JI_CustomsSecondUnitQty = "TNE";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageErrors(invoiceLine.JI_TariffInfo);
		}

		[TestDate(2016, 5, 5)]
		public void TestCheckJI_Tariff_CheckConditionsAreMet_HasInformationCondition()
		{
			var tariff = SetupTariff();
			SetupFormulaConditionData(tariff);
			SetupInformationConditionData(tariff);

			var invoiceLine = CreateInvoiceLine();
			invoiceLine.JI_CustomsQuantity = 200;
			invoiceLine.JI_CustomsUnitQty = "NO";
			invoiceLine.Validation.ValidateJI_Tariff();

			CombineAssertions(() =>
			{
				AssertNoMessageErrors("NoMessageError: NO = 200", invoiceLine.JI_TariffInfo);
				AssertHasWarningContaining("HasWarning1: No TNE", invoiceLine.JI_TariffInfo, @"The Customs Control condition is not satisfied
    Test Ctrl Condition Type 1:
        Information condition: ([TNE] < 20.00) or InformationDesc");

				invoiceLine.JI_CustomsQuantity = 10;
				invoiceLine.JI_CustomsUnitQty = "NO";
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertHasMessageErrorContaining("Has MessageError: No TNE", invoiceLine.JI_TariffInfo, @"The Customs Control condition is not satisfied
    Test Ctrl Condition Type 1:
        AdditionalCode: AC, OrderNumber: ON: (([KGM] < 2000.00) or ([NO] > 2.00)) and (([NO] > 20.00) or ([TNE] < 20.00))");
				AssertHasWarningContaining("HasWarning2: : No TNE", invoiceLine.JI_TariffInfo, @"The Customs Control condition is not satisfied
    Test Ctrl Condition Type 1:
        Information condition: ([TNE] < 20.00) or InformationDesc");

				invoiceLine.JI_CustomsSecondQuantity = 10;
				invoiceLine.JI_CustomsSecondUnitQty = "TNE";
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertNoMessageErrors("NoMessageError: TNE = 10", invoiceLine.JI_TariffInfo);
				AssertNoWarnings("NoWarning: TNE = 10", invoiceLine.JI_TariffInfo);
			});
		}

		[TestDate(2023, 12, 01)]
		public void TestCheckJI_TariffWithTRFRule()
		{
			var date = new ZDateTime(2023, 11, 30);

			var customsRule = Factory.New<CustomsRule>();
			customsRule.CPH_StartDate = date.Date.AddMonths(-1);
			customsRule.CPH_EndDate = date.Date.AddMonths(1);
			customsRule.CPH_PermitDescription = "TEST";
			Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = date;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var expected = string.Format(CustomsRuleHelper.TRFRuleMessageError, customsRule.HumanReadableName);
			invoiceLine.JI_Tariff = "3333333333";
			AssertNoMessageError(invoiceLine.JI_TariffInfo, expected);

			var trfRule = customsRule.Rules.AddNew();
			trfRule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TariffNumber;
			trfRule.CPR_ValueFrom = "33";
			Factory.Save();
			invoiceLine.JI_Tariff = "4444444444";
			AssertNoMessageError(invoiceLine.JI_TariffInfo, expected);
			invoiceLine.JI_Tariff = "3333333333";
			AssertHasMessageError(invoiceLine.JI_TariffInfo, expected);

			trfRule.CPR_ValueFrom = "333";
			Factory.Save();
			invoiceLine.JI_Tariff = "4444444444";
			AssertNoMessageError(invoiceLine.JI_TariffInfo, expected);
			invoiceLine.JI_Tariff = "3333333333";
			AssertHasMessageError(invoiceLine.JI_TariffInfo, expected);

			trfRule.CPR_ValueFrom = "3333";
			Factory.Save();
			invoiceLine.JI_Tariff = "4444444444";
			AssertNoMessageError(invoiceLine.JI_TariffInfo, expected);
			invoiceLine.JI_Tariff = "3333333333";
			AssertHasMessageError(invoiceLine.JI_TariffInfo, expected);

			trfRule.CPR_ValueFrom = "3333333332";
			trfRule.CPR_ValueTo = "3333333334";
			Factory.Save();
			invoiceLine.JI_Tariff = "3333333331";
			AssertNoMessageError(invoiceLine.JI_TariffInfo, expected);
			invoiceLine.JI_Tariff = "3333333332";
			AssertHasMessageError(invoiceLine.JI_TariffInfo, expected);
			invoiceLine.JI_Tariff = "3333333333";
			AssertHasMessageError(invoiceLine.JI_TariffInfo, expected);
			invoiceLine.JI_Tariff = "3333333334";
			AssertHasMessageError(invoiceLine.JI_TariffInfo, expected);
			invoiceLine.JI_Tariff = "3333333335";
			AssertNoMessageError(invoiceLine.JI_TariffInfo, expected);
		}

		public void TestCheckJI_PrimaryPreference()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var testHelper = new UniversalReferenceTestDataHelper(Factory);
				var preferenceSTD = testHelper.CreatePreferenceForCountry("STD", "Standard", Core.Constants.CountryCodes.China);
				Factory.Save();

				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();

				invoiceLine.JI_PrimaryPreference = "";
				AssertHasMessageErrorContaining(invoiceLine.JI_PrimaryPreferenceInfo, "entered");
				invoiceLine.JI_PrimaryPreference = "XXX";
				AssertHasMessageErrorContaining(invoiceLine.JI_PrimaryPreferenceInfo, ListValidation.InvalidCodeMessageError);
				invoiceLine.JI_PrimaryPreference = "STD";
				AssertNoMessageErrors(invoiceLine.JI_PrimaryPreferenceInfo);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				invoiceLine.JI_PrimaryPreference = "";
				AssertNoMessageErrors("Does not do validation for Export declaration", invoiceLine.JI_PrimaryPreferenceInfo);
				invoiceLine.JI_PrimaryPreference = "XXX";
				AssertNoMessageErrors("Does not do validation for Export declaration", invoiceLine.JI_PrimaryPreferenceInfo);
			}
		}

		public void TestCheckJI_ZZF_NKTaxType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var today = ZDateTime.Today;

				var testHelper = new UniversalReferenceTestDataHelper(Factory);
				var tariffType = testHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.China, Universal.Constants.TariffTypes.HarmonizedSystem);
				testHelper.CreateTaxOrFee("CN1", 0, Core.Constants.CountryCodes.China, today.AddDays(-3), today.AddDays(-2), "DESC1");
				testHelper.CreateTaxOrFee("CN2", 0, Core.Constants.CountryCodes.China, today.AddDays(-1), today.AddDays(1), "DESC2");
				testHelper.CreateTaxOrFee("CN3", 0, Core.Constants.CountryCodes.China, today.AddDays(-1), today.AddDays(1), "DESC3");
				testHelper.CreateTaxOrFee("CN4", 0, Core.Constants.CountryCodes.China, today.AddDays(2), today.AddDays(3), "DESC4");
				testHelper.CreateTaxOrFee("AU1", 0, Core.Constants.CountryCodes.Italy, today.AddDays(-1), today.AddDays(1), "AU1");
				Factory.Save();

				var tariff = testHelper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "99999999", ZDateTime.BrettsBirthday, ZDateTime.Today, "Alpha Bravo", compositeKey: "99...99.99", taxOrFeeCode: "CN1");
				testHelper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.China, "CN2", startDate: ZDateTime.BrettsBirthday, endDate: ZDateTime.Today);
				testHelper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.China, "CN4", startDate: ZDateTime.BrettsBirthday, endDate: ZDateTime.Today);

				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "99999999";
				var list = invoiceLine.Lookups.TaxOrFeeCodeList;

				AssertContainsExactElementsInAnyOrder(
					new ZString[]
					{
						"CN2 - DESC2"
					},
					list.ToArray().Select(x => $"{x.Code} - {x.Description}")
				);

				invoiceLine.JI_ZZF_NKTaxType = "CN1";
				AssertHasMessageErrorContaining(invoiceLine.JI_ZZF_NKTaxTypeInfo, ListValidation.InvalidCodeMessageError);

				invoiceLine.JI_ZZF_NKTaxType = "CN2";
				AssertNoNotifications(invoiceLine.JI_ZZF_NKTaxTypeInfo);
			}
		}

		[TestDate(2018, 01, 01)]
		public void TestReferenceDutyRateList()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var testHelper = new UniversalReferenceTestDataHelper(Factory);

				var cnHSNTariffType = testHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.China, "HSN");
				Factory.Save();
				var cusTariff = testHelper.CreateTariff(Core.Constants.CountryCodes.China, cnHSNTariffType.PK, "HSN", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0", 0, "DTY");
				var preference1 = testHelper.CreatePreferenceForCountryAndGrouping("MFN", "Most-favored Nation Duty", "CN", "CN");
				var preference2 = testHelper.CreatePreferenceForCountryAndGrouping("STANDARD", "Standard Duty", "CN", "CN");
				var preference3 = testHelper.CreatePreferenceForCountryAndGrouping("XXX", "XXX", "CN", "CN");

				var dtyRateType = testHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.China, Customs.Universal.Constants.RateTypes.Duty, "Duty");
				var excRateType = testHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.China, Customs.Universal.Constants.RateTypes.Excise, "EXC");

				var dtyRateCode1 = testHelper.LoadOrCreateNewCusRateCode(Factory, "DT1", dtyRateType.PK);
				var dtyRateCode2 = testHelper.LoadOrCreateNewCusRateCode(Factory, "DT2", dtyRateType.PK);
				var excRateCode = testHelper.LoadOrCreateNewCusRateCode(Factory, "EXC", excRateType.PK);
				Factory.Save();

				var testRate1 = testHelper.CreateRate(cusTariff, dtyRateCode1.PK, new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "0", preference1.PK);
				var testRate2 = testHelper.CreateRate(cusTariff, dtyRateCode2.PK, new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "0", preference2.PK);
				var testRate3 = testHelper.CreateRate(cusTariff, excRateCode.PK, new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "0", preference3.PK);

				var tradeGroup1 = testHelper.CreateTradeGroup(Core.Constants.CountryCodes.China, "X1", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06));
				var tradeGroup1Country1 = testHelper.AddCountry(tradeGroup1, Core.Constants.CountryCodes.Australia, new ZDate(2010, 12, 10), new ZDate(2079, 06, 06));

				var tradeGroup2 = testHelper.CreateTradeGroup(Core.Constants.CountryCodes.China, "X2", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06));
				var tradeGroup2Country1 = testHelper.AddCountry(tradeGroup2, Core.Constants.CountryCodes.Australia, new ZDate(2010, 12, 10), new ZDate(2079, 06, 06));
				var tradeGroup2Country2 = testHelper.AddCountry(tradeGroup2, Core.Constants.CountryCodes.NewZealand, new ZDate(2010, 12, 10), new ZDate(2079, 06, 06));

				var applic1 = testHelper.CreateCusApplicability(testRate1, tradeGroup1, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				var applic2 = testHelper.CreateCusApplicability(testRate2, tradeGroup2, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

				Factory.Save();

				var importer = Factory.New<OrgHeader>();
				importer.FillWithValidTestData();
				Factory.Save();

				var cNCompany = Factory.New<GlbCompany>();
				cNCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;
				var cNBranch = cNCompany.Branches.AddNew();
				cNBranch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.China)).RL_Code;

				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				declaration.JE_OH_Importer = importer.PK;
				declaration.JE_GB = cNBranch.PK;
				var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
				invoiceLine.JI_Tariff = cusTariff.ZZ1_TariffCode;
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
				Factory.Save();

				CombineAssertions(() =>
				{
					invoiceLine.JI_PrimaryPreference = ZString.Empty;
					AssertHasMessageErrorContaining(invoiceLine.JI_PrimaryPreferenceInfo, "entered");
					invoiceLine.JI_PrimaryPreference = "whatever";
					AssertHasMessageErrorContaining(invoiceLine.JI_PrimaryPreferenceInfo, ListValidation.InvalidCodeMessageError);
					invoiceLine.JI_PrimaryPreference = "MFN";
					AssertNoNotifications(invoiceLine.JI_PrimaryPreferenceInfo);
					invoiceLine.JI_PrimaryPreference = "STANDARD";
					AssertNoNotifications(invoiceLine.JI_PrimaryPreferenceInfo);
					invoiceLine.JI_PrimaryPreference = "XXX";
					AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "There is no applicable Duty rate for the Tariff 'HSN' and Country Of Origin 'AU' as at 01-Jan-18 00:00:00 in combination with other data entered on the form.");
					AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "Valid Duty rates exist where\r\n1: Preference = MFN\r\n2: Preference = STANDARD\r\n");
				});
			}
		}

		public void TestCheckJI_PartNo_MatchPivot_Import()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_EntryAuthorisationDate = new ZDate(2000, 1, 2);
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PARTNUM";
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "Supplier";
			var supRelation = part.RelatedOrganisations.AddSupplier(supplier);

			var pivot1 = AddPivotForPart(ClassificationTypeList.Codes.HTI, new ZDate(1999, 12, 1), new ZDate(2000, 2, 1));

			invoice.JZ_OH_Supplier = supplier.PK;
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "Importer";
			invoice.JZ_OH_Buyer = importer.PK;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine.Validation.ValidateJI_PartNo();
			AssertNoWarnings(invoiceLine.JI_PartNoInfo);

			invoiceLine.JI_PartNo = part.OP_PartNum;
			invoiceLine.Validation.ValidateJI_PartNo();
			var warningWithNoAttribValues = $"Cannot match classification for product (PARTNUM) based on Importer (), Supplier (Supplier),  (),  (),  (), Serial Number () and Effective Date ({ZDate.Today}).";
			AssertHasWarningContaining(invoiceLine.JI_PartNoInfo, warningWithNoAttribValues);

			invoiceLine.JI_PartAttrib1 = "1";
			invoiceLine.JI_PartAttrib2 = "2";
			invoiceLine.JI_PartAttrib3 = "3";
			invoiceLine.JI_SerialNumber = "SN";
			invoiceLine.Validation.ValidateJI_PartNo();
			var warningWithAttribValues = $"Cannot match classification for product (PARTNUM) based on Importer (), Supplier (Supplier),  (1),  (2),  (3), Serial Number (SN) and Effective Date ({ZDate.Today}).";
			AssertHasWarningContaining(invoiceLine.JI_PartNoInfo, warningWithAttribValues);

			pivot1.CI_DateEnd = ZDateTime.Today.AddDays(2);
			invoiceLine.JI_PartNo = part.OP_PartNum;
			invoiceLine.Validation.ValidateJI_PartNo();
			AssertNoWarnings(invoiceLine.JI_PartNoInfo);

			AddPivotForPart(ClassificationTypeList.Codes.HTB, ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2));
			invoiceLine.JI_PartNo = part.OP_PartNum;
			invoiceLine.Validation.ValidateJI_PartNo();
			var warningOfMultipleMatching = $"Multiple classification matching for product (PARTNUM) based on Importer (), Supplier (Supplier),  (1),  (2),  (3), Serial Number (SN) and Effective Date ({ZDate.Today}).";
			AssertHasWarningContaining(invoiceLine.JI_PartNoInfo, warningOfMultipleMatching);

			BaseCusClassPartPivot AddPivotForPart(ZString type, ZDate startDate, ZDate endDate)
			{
				var importPivot = part.PivotsForBinding.AddNew();
				importPivot.CI_ChildType = type;
				importPivot.CI_OH = supRelation.OU_OH;
				importPivot.CI_TariffNum = "1234567890";
				importPivot.CI_DateStart = startDate;
				importPivot.CI_DateEnd = endDate;
				var attrib1 = importPivot.Attributes1.AddNew();
				attrib1.BG_AttributeValue1 = "1";
				var attrib2 = importPivot.Attributes2.AddNew();
				attrib2.BG_AttributeValue1 = "2";
				var attrib3 = importPivot.Attributes3.AddNew();
				attrib3.BG_AttributeValue1 = "3";

				return importPivot;
			}
		}

		public void TestCheckJI_PartNo_MatchPivot_Export()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_EntryAuthorisationDate = new ZDate(2000, 1, 2);
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PARTNUM";
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "Supplier";
			var supRelation = part.RelatedOrganisations.AddSupplier(supplier);

			var pivot1 = AddPivotForPart(ClassificationTypeList.Codes.HTE, new ZDate(1999, 12, 1), new ZDate(2000, 2, 1));

			invoice.JZ_OH_Supplier = supplier.PK;
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "Importer";
			invoice.JZ_OH_Buyer = importer.PK;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.Validation.ValidateJI_PartNo();
			AssertNoWarnings(invoiceLine.JI_PartNoInfo);

			invoiceLine.JI_PartNo = part.OP_PartNum;
			invoiceLine.Validation.ValidateJI_PartNo();
			var warningWithNoAttribValues = $"Cannot match classification for product (PARTNUM) based on Importer (), Supplier (Supplier) and Effective Date ({ZDate.Today}).";
			AssertHasWarningContaining(invoiceLine.JI_PartNoInfo, warningWithNoAttribValues);

			pivot1.CI_DateEnd = ZDateTime.Today.AddDays(2);
			invoiceLine.JI_PartNo = part.OP_PartNum;
			invoiceLine.Validation.ValidateJI_PartNo();
			AssertNoWarnings(invoiceLine.JI_PartNoInfo);

			AddPivotForPart(ClassificationTypeList.Codes.HTB, ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2));
			invoiceLine.JI_PartNo = part.OP_PartNum;
			invoiceLine.Validation.ValidateJI_PartNo();
			var warningOfMultipleMatching = $"Multiple classification matching for product (PARTNUM) based on Importer (), Supplier (Supplier) and Effective Date ({ZDate.Today}).";
			AssertHasWarningContaining(invoiceLine.JI_PartNoInfo, warningOfMultipleMatching);

			BaseCusClassPartPivot AddPivotForPart(ZString type, ZDate startDate, ZDate endDate)
			{
				var exportPivot = part.PivotsForBinding.AddNew();
				exportPivot.CI_ChildType = type;
				exportPivot.CI_OH = supRelation.OU_OH;
				exportPivot.CI_TariffNum = "1234567890";
				exportPivot.CI_DateStart = startDate;
				exportPivot.CI_DateEnd = endDate;
				return exportPivot;
			}
		}

		public void TestCheckJI_Procedure()
		{
			// The list not defined in base - list validation is tested in coutry solutions where necessary
			// So if no list, have no validation
			var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine.Validation.ValidateJI_Procedure();
			Assert(!invoiceLine.JI_ProcedureInfo.GetMessageErrors().Any());
			invoiceLine.JI_Procedure = "X";
			Assert(!invoiceLine.JI_ProcedureInfo.GetMessageErrors().Any());
		}

		public void TestExposedCusEntryLineErrorProperty()
		{
			var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
			var entryLine = Factory.New<CusEntryLine>();
			invoiceLine.JI_CL = entryLine.PK;
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "NUM";
			product.OP_Desc = "DES 開";
			invoiceLine.JI_OP = product.PK;
			entryLine.CL_Description = entryLine.Description;
			((BaseJobComInvoiceLineValidation)invoiceLine.Validation).ValidateExposedCusEntryLineErrorProperty();
			if (!entryLine.DescriptionSource.IsEmpty)
			{
				AssertHasErrorContaining(invoiceLine.ExposedCusEntryLineErrorPropertyInfo, "Entry line description has non-Western European characters that are copied from");
			}
			else
			{
				Assert(true);
			}
		}

		public void TestInvoiceQuantityAndUnitForBondedWarehousing()
		{
			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				DataRegistry.Business.CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());
				var importer = GetNewImporter();
				var supplier = GetNewSupplier();
				var part = GetNewPart("PART23423", importer, supplier);
				var declaration = Factory.New<BaseJobDeclarationForTesting>();
				declaration.IsInvoiceQuantityRequiredForBondedWarehouseReturns = true;
				var helperMock = new Mock<BondedWarehousingHelper>(declaration) { CallBase = true };
				helperMock
					.Protected().Setup<bool>("IsMarkedForBondedWarehousingCore", ItExpr.IsAny<BaseJobComInvoiceLine>())
					.Returns((BaseJobComInvoiceLine targetInvoiceLine) =>
					{
						return targetInvoiceLine.Declaration.IsExWarehouse ? targetInvoiceLine.UseBondedWarehouseAutomation : targetInvoiceLine.IsGoingIntoBondedWarehouse;
					});
				declaration.GetNewBondedWarehousingHelperReturns = helperMock.Object;
				declaration.JE_OH_Importer = importer.PK;
				declaration.JE_OH_Supplier = supplier.PK;
				declaration.Importer.CompanyData.OB_IMUsedBondedWhs = true;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();

				var invoiceLine = Factory.New<BaseJobComInvoiceLineForTesting>();
				var invoiceLineValidationMock = new Mock<BaseJobComInvoiceLineValidation>(invoiceLine) { CallBase = true };
				invoiceLine.GetNewValidationReturns = invoiceLineValidationMock.Object;
				invoiceLine.JI_JZ = invoice.PK;
				invoice.JobComInvoiceLines.Add(invoiceLine);
				invoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				invoiceLine.JI_PartNo = part.OP_PartNum;
				invoiceLine.JI_InvoiceQuantity = ZDecimal.Zero;
				AssertNoMessageError(invoiceLine.JI_InvoiceQuantityInfo, BaseJobComInvoiceLineValidation.InvoiceQtyIsRequiredForWarehouse(declaration.TermNameForBondedWarehouse));
				invoiceLine.JI_InvoiceUQ = ZString.Empty;
				AssertNoMessageError(invoiceLine.JI_InvoiceUQInfo, BaseJobComInvoiceLineValidation.InvoiceUQIsRequiredForWarehouse(declaration.TermNameForBondedWarehouse));

				declaration.SetSupportsBondedWarehousingForTesting(true);
				invoiceLineValidationMock
					.Protected()
					.Setup<bool>("IsBondedWarehouseValidationMode")
					.Returns(false);
				invoiceLine.Validation.ValidateJI_InvoiceQuantity();
				AssertNoMessageError(invoiceLine.JI_InvoiceQuantityInfo, BaseJobComInvoiceLineValidation.InvoiceQtyIsRequiredForWarehouse(declaration.TermNameForBondedWarehouse));
				invoiceLine.Validation.ValidateJI_InvoiceUQ();
				AssertNoMessageError(invoiceLine.JI_InvoiceUQInfo, BaseJobComInvoiceLineValidation.InvoiceUQIsRequiredForWarehouse(declaration.TermNameForBondedWarehouse));

				invoiceLineValidationMock.Reset();
				invoiceLine.Validation.ValidateJI_InvoiceQuantity();
				AssertHasMessageError(invoiceLine.JI_InvoiceQuantityInfo, BaseJobComInvoiceLineValidation.InvoiceQtyIsRequiredForWarehouse(declaration.TermNameForBondedWarehouse));
				invoiceLine.Validation.ValidateJI_InvoiceUQ();
				AssertHasMessageError(invoiceLine.JI_InvoiceUQInfo, BaseJobComInvoiceLineValidation.InvoiceUQIsRequiredForWarehouse(declaration.TermNameForBondedWarehouse));

				declaration.JE_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(-2);
				invoiceLine.Validation.ValidateJI_InvoiceQuantity();
				AssertNoMessageError(invoiceLine.JI_InvoiceQuantityInfo, BaseJobComInvoiceLineValidation.InvoiceQtyIsRequiredForWarehouse(declaration.TermNameForBondedWarehouse));
				invoiceLine.Validation.ValidateJI_InvoiceUQ();
				AssertNoMessageError(invoiceLine.JI_InvoiceUQInfo, BaseJobComInvoiceLineValidation.InvoiceUQIsRequiredForWarehouse(declaration.TermNameForBondedWarehouse));

				declaration.JE_SystemCreateTimeUtc = ZDateTime.Today;
				invoiceLine.Validation.ValidateJI_InvoiceQuantity();
				AssertHasMessageError(invoiceLine.JI_InvoiceQuantityInfo, BaseJobComInvoiceLineValidation.InvoiceQtyIsRequiredForWarehouse(declaration.TermNameForBondedWarehouse));
				invoiceLine.Validation.ValidateJI_InvoiceUQ();
				AssertHasMessageError(invoiceLine.JI_InvoiceUQInfo, BaseJobComInvoiceLineValidation.InvoiceUQIsRequiredForWarehouse(declaration.TermNameForBondedWarehouse));

				declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				invoiceLine.SetUseBondedWarehouseAutomationForTesting(true);
				invoiceLineValidationMock
					.Protected()
					.Setup<bool>("IsBondedWarehouseValidationMode")
					.Returns(false);
				invoiceLine.Validation.ValidateJI_InvoiceQuantity();
				AssertNoMessageError(invoiceLine.JI_InvoiceQuantityInfo, BaseJobComInvoiceLineValidation.InvoiceQtyIsRequiredForWarehouse(declaration.TermNameForBondedWarehouse));
				invoiceLine.Validation.ValidateJI_InvoiceUQ();
				AssertNoMessageError(invoiceLine.JI_InvoiceUQInfo, BaseJobComInvoiceLineValidation.InvoiceUQIsRequiredForWarehouse(declaration.TermNameForBondedWarehouse));

				invoiceLineValidationMock.Reset();
				invoiceLine.Validation.ValidateJI_InvoiceQuantity();
				AssertHasMessageError(invoiceLine.JI_InvoiceQuantityInfo, BaseJobComInvoiceLineValidation.InvoiceQtyIsRequiredForWarehouse(declaration.TermNameForBondedWarehouse));
				invoiceLine.Validation.ValidateJI_InvoiceUQ();
				AssertHasMessageError(invoiceLine.JI_InvoiceUQInfo, BaseJobComInvoiceLineValidation.InvoiceUQIsRequiredForWarehouse(declaration.TermNameForBondedWarehouse));

				declaration.SetSupportsBondedWarehousingForTesting(false);
				invoiceLine.Validation.ValidateJI_InvoiceQuantity();
				AssertNoMessageError(invoiceLine.JI_InvoiceQuantityInfo, BaseJobComInvoiceLineValidation.InvoiceQtyIsRequiredForWarehouse(declaration.TermNameForBondedWarehouse));
				invoiceLine.Validation.ValidateJI_InvoiceUQ();
				AssertNoMessageError(invoiceLine.JI_InvoiceUQInfo, BaseJobComInvoiceLineValidation.InvoiceUQIsRequiredForWarehouse(declaration.TermNameForBondedWarehouse));
			}
		}

		public void TestBondedWhsQuantityAndUnitForBondedWarehousing()
		{
			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				DataRegistry.Business.CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());
				var importer = GetNewImporter();
				var supplier = GetNewSupplier();
				var part = GetNewPart("PART23423", importer, supplier);
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_OH_Importer = importer.PK;
				declaration.JE_OH_Supplier = supplier.PK;
				declaration.Importer.CompanyData.OB_IMUsedBondedWhs = true;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = Factory.New<BaseJobComInvoiceLineForTesting>();
				var invoiceLineValidationMock = new Mock<BaseJobComInvoiceLineValidation>(invoiceLine) { CallBase = true };
				invoiceLine.GetNewValidationReturns = invoiceLineValidationMock.Object;
				invoiceLine.JI_JZ = invoice.PK;
				invoice.JobComInvoiceLines.Add(invoiceLine);
				invoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				invoiceLine.JI_PartNo = part.OP_PartNum;
				invoiceLine.JI_BondedWhsQuantity = ZDecimal.Zero;
				AssertNoMessageError(invoiceLine.JI_BondedWhsQuantityInfo, BaseJobComInvoiceLineValidation.BondedWhsQuantityIsRequiredForWarehouse("Inventory Management"));
				invoiceLine.JI_BondedWhsUnitQty = ZString.Empty;
				AssertNoMessageError(invoiceLine.JI_BondedWhsUnitQtyInfo, BaseJobComInvoiceLineValidation.BondedWhsUnitQtyIsRequiredForWarehouse("Inventory Management"));

				declaration.SetSupportsBondedWarehousingForTesting(true);
				invoiceLineValidationMock
					.Protected()
					.Setup<bool>("IsBondedWarehouseValidationMode")
					.Returns(false);
				invoiceLine.Validation.ValidateJI_BondedWhsQuantity();
				AssertNoMessageError(invoiceLine.JI_BondedWhsQuantityInfo, BaseJobComInvoiceLineValidation.BondedWhsQuantityIsRequiredForWarehouse("Inventory Management"));
				invoiceLine.Validation.ValidateJI_BondedWhsUnitQty();
				AssertNoMessageError(invoiceLine.JI_BondedWhsUnitQtyInfo, BaseJobComInvoiceLineValidation.BondedWhsUnitQtyIsRequiredForWarehouse("Inventory Management"));

				invoiceLineValidationMock.Reset();
				invoiceLine.Validation.ValidateJI_BondedWhsQuantity();
				AssertHasMessageError(invoiceLine.JI_BondedWhsQuantityInfo, BaseJobComInvoiceLineValidation.BondedWhsQuantityIsRequiredForWarehouse("Inventory Management"));
				invoiceLine.Validation.ValidateJI_BondedWhsUnitQty();
				AssertHasMessageError(invoiceLine.JI_BondedWhsUnitQtyInfo, BaseJobComInvoiceLineValidation.BondedWhsUnitQtyIsRequiredForWarehouse("Inventory Management"));

				declaration.JE_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(-2);
				invoiceLine.Validation.ValidateJI_BondedWhsQuantity();
				AssertNoMessageError(invoiceLine.JI_BondedWhsQuantityInfo, BaseJobComInvoiceLineValidation.BondedWhsQuantityIsRequiredForWarehouse("Inventory Management"));
				invoiceLine.Validation.ValidateJI_BondedWhsUnitQty();
				AssertNoMessageError(invoiceLine.JI_BondedWhsUnitQtyInfo, BaseJobComInvoiceLineValidation.BondedWhsUnitQtyIsRequiredForWarehouse("Inventory Management"));

				declaration.JE_SystemCreateTimeUtc = ZDateTime.Today;
				invoiceLine.Validation.ValidateJI_BondedWhsQuantity();
				AssertHasMessageError(invoiceLine.JI_BondedWhsQuantityInfo, BaseJobComInvoiceLineValidation.BondedWhsQuantityIsRequiredForWarehouse("Inventory Management"));
				invoiceLine.Validation.ValidateJI_BondedWhsUnitQty();
				AssertHasMessageError(invoiceLine.JI_BondedWhsUnitQtyInfo, BaseJobComInvoiceLineValidation.BondedWhsUnitQtyIsRequiredForWarehouse("Inventory Management"));

				invoiceLine.ComponentInventoryCollection.AddNew();
				invoiceLine.JI_BondedWhsQuantity = ZDecimal.Zero;
				invoiceLine.JI_BondedWhsUnitQty = ZString.Empty;
				AssertNoMessageError(invoiceLine.JI_BondedWhsQuantityInfo, BaseJobComInvoiceLineValidation.BondedWhsQuantityIsRequiredForWarehouse("Inventory Management"));
				AssertNoMessageError(invoiceLine.JI_BondedWhsUnitQtyInfo, BaseJobComInvoiceLineValidation.BondedWhsUnitQtyIsRequiredForWarehouse("Inventory Management"));
			}
		}

		public void TestJI_PartNoHasWarningOrErrorForChangeOfRegime()
		{
			var helper = new WhsDataTestHelper(Factory);
			var data = helper.CreateChangeOfRegimeEntryData();
			var invoiceLine = data.InvoiceLine;
			invoiceLine.JI_PartNo = ZString.Empty;
			AssertEquals("IsChangeOfRegimeWarehousing", true, invoiceLine.IsChangeOfRegimeWarehousing);
			var productIsRequiredForWarehouseMessage = BaseJobComInvoiceLineValidation.ProductIsRequiredForWarehouse("Inventory Management");
			AssertHasMessageError(invoiceLine.JI_PartNoInfo, productIsRequiredForWarehouseMessage);

			invoiceLine.JI_PartNo = helper.Part.OP_PartNum;
			AssertNoMessageError(invoiceLine.JI_PartNoInfo, productIsRequiredForWarehouseMessage);
		}

		public void TestJI_PartNoHasWarningOrErrorForBondedWarehousing()
		{
			string warning = "Please choose or create a product if you would like this line to be recorded in the bonded warehousing system.";
			string error = "Please choose or create a product so that this line can be recorded in the bonded warehousing system.";

			var decWithBondMock = Factory.NewMoq<DummyDeclarationWithIntegrationSupport>();
			var declarationMockProtected = decWithBondMock.Protected();
			declarationMockProtected.Setup<bool>("GetIsWHSUniversalXMLActive").Returns(false);
			var decWithBond = decWithBondMock.Object;
			var helperMock = new Mock<BondedWarehousingHelper>(decWithBond) { CallBase = true };
			helperMock
				.Protected()
				.Setup<bool>("IsMarkedForBondedWarehousingCore", ItExpr.IsAny<BaseJobComInvoiceLine>())
				.Returns((BaseJobComInvoiceLine targetInvoiceLine) =>
				{
					return targetInvoiceLine.Declaration.IsExWarehouse ? targetInvoiceLine.UseBondedWarehouseAutomation : targetInvoiceLine.IsGoingIntoBondedWarehouse;
				});
			declarationMockProtected.Setup<BondedWarehousingHelper>("GetNewBondedWarehousingHelper").Returns(helperMock.Object);
			decWithBond.JE_OH_Importer = GetNewImporterPK();
			decWithBond.JE_OH_Supplier = GetNewSupplierPK();
			decWithBond.JE_MessageType = JobMessageTypeList.Codes.Import;
			decWithBond.SupportMultipleWarehouseEntryCoreExposed = false;
			decWithBond.Importer.CompanyData.OB_IMUsedBondedWhs = true;
			var testValidPart = GetNewPartWithClassification("NOODLE", decWithBond.JE_OH_Importer, decWithBond.JE_OH_Supplier);

			var invoice = decWithBond.Invoices.AddNew();
			var lineMock = Factory.NewMoq<BaseJobComInvoiceLine>();
			var lineMockProtected = lineMock.Protected();
			lineMockProtected.Setup<bool>("SupportsBondedWarehousingCore")
				.Returns(() =>
				{
					return decWithBond.SupportsBondedWarehousing;
				});
			var line = lineMock.Object;
			var lineValidationMock = new Mock<BaseJobComInvoiceLineValidation>(line) { CallBase = true };
			lineMockProtected.Setup<JobComInvoiceLineValidation>("GetNewValidation").Returns(lineValidationMock.Object);
			line.JI_JZ = invoice.PK;
			invoice.JobComInvoiceLines.Add(line);
			line.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			line.JI_PartNo = "";
			AssertHasWarning(line.JI_PartNoInfo, warning);

			line.JI_PartNo = "!!!";
			AssertHasWarning(line.JI_PartNoInfo, warning);

			decWithBond.SupportsBondedWarehousingCoreExposed = false;
			line.Validation.ValidateJI_PartNo();
			Assert("Doesn't have warning", !line.JI_PartNoInfo.HasWarning(warning));
			Assert("Doesn't have error", !line.JI_PartNoInfo.HasError(error));

			decWithBond.SupportsBondedWarehousingCoreExposed = true;
			line.JI_PartNo = "NOODLE";
			Assert("Doesn't have warning", !line.JI_PartNoInfo.HasWarning(warning));
			Assert("Doesn't have error", !line.JI_PartNoInfo.HasError(error));

			decWithBond.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			line.JI_PartNo = "!!!";
			Assert("Has error", line.JI_PartNoInfo.HasError(error));
			Assert("Doesn't have warning", !line.JI_PartNoInfo.HasWarning(warning));

			line.SetIsGoingIntoBondedWarehouseCoreForTesting(false);
			line.Validation.ValidateJI_PartNo();
			Assert("Has error", line.JI_PartNoInfo.HasError(error));
			Assert("Doesn't have warning", !line.JI_PartNoInfo.HasWarning(warning));

			decWithBond.JE_MessageType = JobMessageTypeList.Codes.Import;
			line.JI_PartNo = "!!!";
			AssertEquals("Has bonded warehouse warning", false, line.JI_PartNoInfo.HasWarning(warning));
			AssertEquals("Has bonded warehouse error", false, line.JI_PartNoInfo.HasError(error));

			declarationMockProtected.Setup<bool>("GetIsWHSUniversalXMLActive").Returns(true);
			line.Declaration.JE_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
			line.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			line.JI_PartNo = "";
			AssertHasMessageError(line.JI_PartNoInfo, BaseJobComInvoiceLineValidation.ProductIsRequiredForWarehouse("Inventory Management"));
			line.JI_PartNo = "!!!";
			AssertHasMessageError(line.JI_PartNoInfo, BaseJobComInvoiceLineValidation.ProductIsRequiredForWarehouse("Inventory Management"));

			lineValidationMock
				.Protected()
				.Setup<bool>("IsBondedWarehouseValidationMode")
				.Returns(false);
			line.JI_PartNo = "";
			AssertNoMessageError(line.JI_PartNoInfo, BaseJobComInvoiceLineValidation.ProductIsRequiredForWarehouse("Inventory Management"));
			line.JI_PartNo = "!!!";
			AssertNoMessageError(line.JI_PartNoInfo, BaseJobComInvoiceLineValidation.ProductIsRequiredForWarehouse("Inventory Management"));

			lineValidationMock.Reset();
			line.JI_PartNo = "";
			AssertHasMessageError(line.JI_PartNoInfo, BaseJobComInvoiceLineValidation.ProductIsRequiredForWarehouse("Inventory Management"));
			line.JI_PartNo = "!!!";
			AssertHasMessageError(line.JI_PartNoInfo, BaseJobComInvoiceLineValidation.ProductIsRequiredForWarehouse("Inventory Management"));

			decWithBond.SupportsBondedWarehousingCoreExposed = false;
			line.Validation.ValidateJI_PartNo();
			Assert("Doesn't have message error", !line.JI_PartNoInfo.HasMessageError(BaseJobComInvoiceLineValidation.ProductIsRequiredForWarehouse("Inventory Management")));
			Assert("Doesn't have error", !line.JI_PartNoInfo.HasError(error));

			decWithBond.SupportsBondedWarehousingCoreExposed = true;
			line.JI_PartNo = "NOODLE";
			Assert("Doesn't have message error", !line.JI_PartNoInfo.HasMessageError(BaseJobComInvoiceLineValidation.ProductIsRequiredForWarehouse("Inventory Management")));
			Assert("Doesn't have error", !line.JI_PartNoInfo.HasError(error));

			decWithBond.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			line.JI_PartNo = "!!!";
			Assert("Has error", line.JI_PartNoInfo.HasError(error));
			Assert("Doesn't have message error", !line.JI_PartNoInfo.HasMessageError(BaseJobComInvoiceLineValidation.ProductIsRequiredForWarehouse("Inventory Management")));

			line.SetIsGoingIntoBondedWarehouseCoreForTesting(false);
			line.Validation.ValidateJI_PartNo();
			Assert("Has error", line.JI_PartNoInfo.HasError(error));
			Assert("Doesn't have message error", !line.JI_PartNoInfo.HasMessageError(BaseJobComInvoiceLineValidation.ProductIsRequiredForWarehouse("Inventory Management")));

			decWithBond.JE_MessageType = JobMessageTypeList.Codes.Import;
			line.JI_PartNo = "!!!";
			AssertEquals("Has bonded warehouse message error", false, line.JI_PartNoInfo.HasMessageError(BaseJobComInvoiceLineValidation.ProductIsRequiredForWarehouse("Inventory Management")));
			AssertEquals("Has bonded warehouse error", false, line.JI_PartNoInfo.HasError(error));

			decWithBond.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			line.SetUseBondedWarehouseAutomationForTesting(true);
			line.Validation.ValidateJI_PartNo();
			AssertHasMessageError(line.JI_PartNoInfo, BaseJobComInvoiceLineValidation.ProductIsRequiredForWarehouse("Inventory Management"));
			lineValidationMock
				.Protected()
				.Setup<bool>("IsBondedWarehouseValidationMode")
				.Returns(false);
			line.Validation.ValidateJI_PartNo();
			AssertNoMessageError(line.JI_PartNoInfo, BaseJobComInvoiceLineValidation.ProductIsRequiredForWarehouse("Inventory Management"));
			lineValidationMock.Reset();
			line.Validation.ValidateJI_PartNo();
			AssertHasMessageError(line.JI_PartNoInfo, BaseJobComInvoiceLineValidation.ProductIsRequiredForWarehouse("Inventory Management"));
			decWithBond.SupportsBondedWarehousingCoreExposed = false;
			line.Validation.ValidateJI_PartNo();
			AssertNoMessageError(line.JI_PartNoInfo, BaseJobComInvoiceLineValidation.ProductIsRequiredForWarehouse("Inventory Management"));
		}

		public void TestJI_PartNoValidPartHasNoNotifications()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader header = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			declaration.JE_OH_Importer = GetNewImporterPK();
			header.JZ_OH_Supplier = GetNewSupplierPK();
			OrgSupplierPart testValidPart = GetNewPartWithClassification(TestValidPartNo, declaration.JE_OH_Importer, header.JZ_OH_Supplier);

			BaseJobComInvoiceLine line = header.JobComInvoiceLines.AddNew();
			line.JI_PartNo = TestValidPartNo;

			AssertNoNotifications(line.JI_PartNoInfo);
		}

		public void TestJI_PartNoWarnsIfThereIsMoreThanOneMatch()
		{
			CustomsDataRegistry.Instance.EnableExactMatchForProduct.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader header = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			declaration.JE_OH_Importer = GetNewImporterPK();
			header.JZ_OH_Supplier = GetNewSupplierPK();

			OrgHeader supplier2 = Factory.New<OrgHeader>();
			supplier2.FillWithValidTestData();

			OrgHeader supplier3 = Factory.New<OrgHeader>();
			supplier3.FillWithValidTestData();

			OrgSupplierPart testValidPart = Factory.New<OrgSupplierPart>();
			testValidPart.OP_PartNum = TestValidPartNo;
			testValidPart.RelatedOrganisations.AddOrganisationIfNotExist(supplier2.PK, OrgPartRelation.RelationshipTypes.Supplier);
			testValidPart.RelatedOrganisations.AddOrganisationIfNotExist(declaration.JE_OH_Importer, OrgPartRelation.RelationshipTypes.Owner);

			OrgSupplierPart testValidPart2 = Factory.New<OrgSupplierPart>();
			testValidPart2.OP_PartNum = TestValidPartNo;
			testValidPart2.RelatedOrganisations.AddOrganisationIfNotExist(supplier3.PK, OrgPartRelation.RelationshipTypes.Supplier);
			testValidPart2.RelatedOrganisations.AddOrganisationIfNotExist(declaration.JE_OH_Importer, OrgPartRelation.RelationshipTypes.Owner);

			BaseJobComInvoiceLine line = header.JobComInvoiceLines.AddNew();
			line.JI_PartNo = TestValidPartNo;
			AssertEquals("Line.PartSyncManager.TotalMatchCount", 2, line.PartSyncManager.TotalMatchCount);
			AssertHasWarningContaining(line.JI_PartNoInfo, BaseJobComInvoiceLineValidation.WarningMoreThanOneProductMatchFound);

			CustomsDataRegistry.Instance.EnableExactMatchForProduct.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			line.JI_PartNo = "";
			line.JI_PartNo = TestValidPartNo;
			AssertEquals("Line.PartSyncManager.TotalMatchCount", 0, line.PartSyncManager.TotalMatchCount);
			AssertNoWarningContaining(line.JI_PartNoInfo, BaseJobComInvoiceLineValidation.WarningMoreThanOneProductMatchFound);
		}

		public void TestJI_PartNoWarnsIfThereIsMoreThanOneUNDG()
		{
			var dec = Factory.New<BaseJobDeclarationForTesting>();
			dec.IsUNDGSupportedOnInvoiceLinesReturns = true;
			dec.JE_OH_Supplier = GetNewSupplierPK();
			dec.JE_OH_Importer = GetNewImporterPK();
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = TestValidPartNo;
			part.RelatedOrganisations.AddOrganisationIfNotExist(dec.JE_OH_Supplier, OrgPartRelation.RelationshipTypes.Supplier);
			part.RelatedOrganisations.AddOrganisationIfNotExist(dec.JE_OH_Importer, OrgPartRelation.RelationshipTypes.Owner);
			var undg2 = part.UNDGs.TryGetOrCreate("2015A", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO);
			var undg1 = part.UNDGs.TryGetOrCreate("3208A", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO);

			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = TestValidPartNo;
			AssertHasWarningContaining(invoiceLine.JI_PartNoInfo, BaseJobComInvoiceLineValidation.WarningPartHasMultipleUNDGRecords);

			undg2.Delete();
			invoiceLine.JI_PartNo = "";
			invoiceLine.JI_PartNo = TestValidPartNo;
			AssertNoWarningContaining(invoiceLine.JI_PartNoInfo, BaseJobComInvoiceLineValidation.WarningPartHasMultipleUNDGRecords);
		}

		public void TestJI_PartNoWarnsIfSupplierOrImporterAreNotEntered()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			BaseJobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.PartSyncManager.Enabled = false;
			invoiceLine.JI_PartNo = "IHOPENOBODYADDSTHIS";
			AssertNoWarning(invoiceLine.JI_PartNoInfo, BaseJobComInvoiceLineValidation.WarningPartCannotBeFoundBeforeEnteringASupplierAndImporter);

			invoiceLine.PartSyncManager.Enabled = true;
			invoiceLine.Validation.ValidateJI_PartNo();
			AssertHasWarning(invoiceLine.JI_PartNoInfo, BaseJobComInvoiceLineValidation.WarningPartCannotBeFoundBeforeEnteringASupplierAndImporter);

			declaration.JE_OH_Importer = GetNewImporterPK();
			invoiceHeader.JZ_OH_Supplier = GetNewSupplierPK();
			invoiceLine.JI_PartNo = "ORTHISONEEITHER";
			AssertNoWarning(invoiceLine.JI_PartNoInfo, BaseJobComInvoiceLineValidation.WarningPartCannotBeFoundBeforeEnteringASupplierAndImporter);
		}

		public void TestJI_PartNoWarnsWhenFoundButNotRelatedOrInactive()
		{
			CustomsDataRegistry.Instance.EnableExactMatchForProduct.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			var importer1 = GetNewImporter();
			var supplier1 = GetNewSupplier();
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var declaration = newFactory.New<BaseJobDeclaration>();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			JobComInvoiceLinePartSynchronisationManager.SetCurrentPartSyncManagerActiveDeciderPK(invoiceLine.Factory, invoiceLine.PartSyncManagerActiveDeciderPK);
			declaration.JE_OH_Importer = importer1.PK;
			invoiceHeader.JZ_OH_Supplier = supplier1.PK;

			var importer2 = Factory.NewWithValidTestData<OrgHeader>();
			var supplier2 = Factory.New<OrgHeader>();
			supplier2.FillWithValidTestData();

			var importer3 = Factory.NewWithValidTestData<OrgHeader>();
			var supplier3 = Factory.New<OrgHeader>();
			supplier3.FillWithValidTestData();

			var validPart1 = GetNewPart(TestValidPartNo, importer2, supplier2);

			Factory.Save();

			invoiceLine.JI_PartNo = TestValidPartNo;
			AssertHasWarning(invoiceLine.JI_PartNoInfo, BaseJobComInvoiceLineValidation.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
			AssertNoWarning(invoiceLine.JI_PartNoInfo, BaseJobComInvoiceLineValidation.WarningPartCodesFoundButNotRelatedToSupplierImporterCombination);

			var validPart2 = GetNewPart(TestValidPartNo, importer3, supplier3);
			Factory.Save();
			AssertNoWarning(invoiceLine.JI_PartNoInfo, BaseJobComInvoiceLineValidation.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
			AssertHasWarning(invoiceLine.JI_PartNoInfo, BaseJobComInvoiceLineValidation.WarningPartCodesFoundButNotRelatedToSupplierImporterCombination);

			var validPart3 = GetNewPart(TestValidPartNo, importer1, supplier1);
			Factory.Save();
			AssertNoWarning(invoiceLine.JI_PartNoInfo, BaseJobComInvoiceLineValidation.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
			AssertNoWarning(invoiceLine.JI_PartNoInfo, BaseJobComInvoiceLineValidation.WarningPartCodesFoundButNotRelatedToSupplierImporterCombination);

			validPart3.OP_IsActive = false;
			Factory.Save();
			invoiceLine.Validation.ValidateJI_PartNo();
			AssertNoWarning(invoiceLine.JI_PartNoInfo, BaseJobComInvoiceLineValidation.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
			AssertHasWarning(invoiceLine.JI_PartNoInfo, BaseJobComInvoiceLineValidation.WarningPartCodesFoundButNotRelatedToSupplierImporterCombination);
			invoiceLine.PartSyncManager.Enabled = false;
			invoiceLine.Validation.ValidateJI_PartNo();
			AssertNoWarning(invoiceLine.JI_PartNoInfo, BaseJobComInvoiceLineValidation.WarningPartCodesFoundButNotRelatedToSupplierImporterCombination);

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			validPart3.OP_PartNum = "ANYTHING";
			Factory.Save();
			invoiceLine2.JI_PartNo = "ANYTHING";
			AssertHasWarning(invoiceLine2.JI_PartNoInfo, BaseJobComInvoiceLineValidation.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
			AssertNoWarning(invoiceLine2.JI_PartNoInfo, BaseJobComInvoiceLineValidation.WarningPartCodesFoundButNotRelatedToSupplierImporterCombination);
			validPart3.OP_IsActive = true;
			Factory.Save();
			var invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_PartNo = "ANYTHING";
			AssertNoWarning(invoiceLine3.JI_PartNoInfo, BaseJobComInvoiceLineValidation.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
			AssertNoWarning(invoiceLine3.JI_PartNoInfo, BaseJobComInvoiceLineValidation.WarningPartCodesFoundButNotRelatedToSupplierImporterCombination);
		}

		public void TestJI_PartNoWarnsWhenFoundButNotRelatedWithComplexRelationships()
		{
			CustomsDataRegistry.Instance.EnableExactMatchForProduct.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			OrgHeader importer1 = GetNewImporter();
			OrgHeader supplier1 = GetNewSupplier();

			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			declaration.JE_OH_Importer = importer1.PK;
			invoiceHeader.JZ_OH_Supplier = supplier1.PK;

			OrgHeader supplier2 = Factory.New<OrgHeader>();
			supplier2.FillWithValidTestData();

			OrgHeader supplier3 = Factory.New<OrgHeader>();
			supplier3.FillWithValidTestData();

			OrgSupplierPart validPart = GetNewPart(TestValidPartNo, supplier1, supplier1);
			validPart.RelatedOrganisations.AddOrganisationIfNotExist(supplier2.PK, OrgPartRelation.RelationshipTypes.Both);
			validPart.RelatedOrganisations.AddOrganisationIfNotExist(supplier3.PK, OrgPartRelation.RelationshipTypes.Owner);
			OrgPartRelation validOwner = validPart.RelatedOrganisations.AddOrganisationIfNotExist(importer1.PK, OrgPartRelation.RelationshipTypes.Owner);
			Factory.Save();
			invoiceLine.JI_PartNo = TestValidPartNo;
			AssertNoWarning(invoiceLine.JI_PartNoInfo, BaseJobComInvoiceLineValidation.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);

			validOwner.Delete();
			Factory.Save();
			invoiceLine.JI_PartNo = "";
			invoiceLine.JI_PartNo = TestValidPartNo;
			AssertHasWarning(invoiceLine.JI_PartNoInfo, BaseJobComInvoiceLineValidation.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);

			invoiceLine.PartSyncManager.Enabled = false;
			invoiceLine.Validation.ValidateJI_PartNo();
			AssertNoWarning(invoiceLine.JI_PartNoInfo, BaseJobComInvoiceLineValidation.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
		}

		public void TestJI_PartNoWarnsWhenPartCodeNotFoundAtAll()
		{
			var importer1 = GetNewImporter();
			var supplier1 = GetNewSupplier();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var declaration = newFactory.New<BaseJobDeclaration>();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			JobComInvoiceLinePartSynchronisationManager.SetCurrentPartSyncManagerActiveDeciderPK(invoiceLine.Factory, invoiceLine.PartSyncManagerActiveDeciderPK);
			declaration.JE_OH_Importer = importer1.PK;
			invoiceHeader.JZ_OH_Supplier = supplier1.PK;

			invoiceLine.JI_PartNo = TestValidPartNo;
			invoiceLine.PartSyncManager.Enabled = false;
			invoiceLine.Validation.ValidateJI_PartNo();
			AssertNoWarning(invoiceLine.JI_PartNoInfo, BaseJobComInvoiceLineValidation.WarningPartCodeNotFoundAtAll);
			invoiceLine.PartSyncManager.Enabled = true;
			invoiceLine.Validation.ValidateJI_PartNo();
			AssertHasWarning(invoiceLine.JI_PartNoInfo, BaseJobComInvoiceLineValidation.WarningPartCodeNotFoundAtAll);

			var validPart = GetNewPart(TestValidPartNo, importer1, supplier1);
			Factory.Save();
			AssertNoWarning(invoiceLine.JI_PartNoInfo, BaseJobComInvoiceLineValidation.WarningPartCodeNotFoundAtAll);
		}

		public void TestJI_PartNoWarnsWhenProductHasNotBeenAudited()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			CustomsDataRegistry.Instance.ImportProductAuditAction.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ProductAuditActions.Codes.AddWarningValidation);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PARTNUM";
			var pivot = part.PivotsForBinding.AddNew();
			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_OH = Factory.New<OrgHeader>().PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			var classification = Factory.New<BaseCusClassification>();
			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			pivot.CI_CC = classification.PK;
			invoice.JZ_OH_Supplier = relation.OU_OH;
			invoiceLine.JI_PartNo = part.OP_PartNum;

			AssertHasWarning(invoiceLine.JI_PartNoInfo, BaseJobComInvoiceLineValidation.ProductHasNotBeenAudited);
			pivot.CI_LastAuditedDate = ZDateTime.Now;
			invoiceLine.Validation.ValidateJI_PartNo();
			AssertNoWarning(invoiceLine.JI_PartNoInfo, BaseJobComInvoiceLineValidation.ProductHasNotBeenAudited);
		}

		public void TestJI_CCValidation()
		{
			BaseCusClassification classification = Factory.New<BaseCusClassification>();
			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.Both;
			classification.CC_LookupCode = "Code";
			classification.CC_IsActive = false;

			var mock = Factory.NewMoq<BaseJobComInvoiceLine>();
			var invoiceLine = mock.Object;
			var mockProtected = mock.Protected();

			invoiceLine.JI_CC = classification.PK;
			AssertHasWarning(invoiceLine.JI_CCInfo, BaseJobComInvoiceLineValidation.ClassificationIsNotActive);

			classification.CC_IsActive = true;
			invoiceLine.JI_CC = classification.PK;
			AssertNoWarning(invoiceLine.JI_CCInfo, BaseJobComInvoiceLineValidation.ClassificationIsNotActive);

			mock.Setup(x => x.UseExportClassification).Returns(false);
			invoiceLine.JI_CC = classification.PK;
			AssertNoMessageError(invoiceLine.JI_CCInfo, BaseJobComInvoiceLineValidation.ClassificationIsNotValid);

			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.EXP;
			invoiceLine.JI_CC = classification.PK;
			AssertHasMessageError(invoiceLine.JI_CCInfo, BaseJobComInvoiceLineValidation.ClassificationIsNotValid);

			mock.Setup(x => x.UseExportClassification).Returns(true);

			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.EXP;
			invoiceLine.JI_CC = classification.PK;
			AssertNoMessageError(invoiceLine.JI_CCInfo, BaseJobComInvoiceLineValidation.ClassificationIsNotValid);

			mock.Setup(x => x.UseImportClassification).Returns(false);

			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			invoiceLine.JI_CC = classification.PK;
			AssertHasMessageError(invoiceLine.JI_CCInfo, BaseJobComInvoiceLineValidation.ClassificationIsNotValid);

			mock.Setup(x => x.UseImportClassification).Returns(true);

			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			invoiceLine.JI_CC = classification.PK;
			AssertNoMessageError(invoiceLine.JI_CCInfo, BaseJobComInvoiceLineValidation.ClassificationIsNotValid);

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			CustomsDataRegistry.Instance.ImportProductAuditAction.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ProductAuditActions.Codes.AddWarningValidation);
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CC = classification.PK;
			invoiceLine.Validation.ValidateJI_CC();
			AssertHasWarning(invoiceLine.JI_CCInfo, BaseJobComInvoiceLineValidation.ClassificationLookupHasNotBeenAudited);
			classification.CC_LastAuditedDate = ZDateTime.Now;
			invoiceLine.Validation.ValidateJI_CC();
			AssertNoWarning(invoiceLine.JI_CCInfo, BaseJobComInvoiceLineValidation.ClassificationLookupHasNotBeenAudited);
		}

		public void TestEmptyTariffHasMessageError()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			var header = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var line = header.JobComInvoiceLines.AddNew();

			line.JI_Tariff = "";
			AssertEquals(true, line.JI_TariffInfo.HasMessageErrors());
		}

		public void TestJI_OrderNumber()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader header = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceLine line = header.JobComInvoiceLines.AddNew();
			line.AutoAddOrderNumberOnSet = false;

			var order = declaration.AttachedOrders.AddNew();
			order.JD_OrderNumber = "xxx";
			var orderItem = declaration.DocsAndCartage.OrderItems.AddNew();
			orderItem.JT_OrderReference = "yyy";

			line.JI_OrderNumber = "111";
			AssertEquals("Should be an error since the order number doesn't exist", true, line.JI_OrderNumberInfo.HasNotifications());

			line.JI_OrderNumber = "xxx";
			AssertEquals("Order number doesn't exist", false, line.JI_OrderNumberInfo.HasNotifications());

			line.JI_OrderNumber = "yyy";
			AssertEquals("Order reference exists, should be ok", false, line.JI_OrderNumberInfo.HasNotifications());

			line.JI_OrderNumber = "111";
			AssertEquals("Should be an error since the order number doesn't exist", true, line.JI_OrderNumberInfo.HasNotifications());

			line.JI_OrderNumber = "";
			AssertEquals("Should be allowed to enter no order number", false, line.JI_OrderNumberInfo.HasNotifications());

			line.JI_OrderNumber = "dsa^21";
			AssertEquals("Should not be allowed to enter an invalid order number", true, line.JI_OrderNumberInfo.HasNotifications());

			order.JD_OrderNumber = "1";
			order.JD_OrderNumberSplit = 1;
			order.OrderLines.AddNew();
			line.JI_JO = order.OrderLines[0].PK;
			line.Validation.ValidateAll();
			AssertEquals("Should not have notifications on Order Number due to split", false, line.JI_OrderNumberInfo.HasNotifications());
		}

		public void TestValidateJI_VolumeUQ()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader header = declaration.Invoices.AddNew();
			BaseJobComInvoiceLine line = header.JobComInvoiceLines.AddNew();

			line.JI_VolumeUQ = "XX";
			AssertEquals("ZZ not in the list", true, line.JI_VolumeUQInfo.HasErrors());

			line.JI_VolumeUQ = line.Lookups.VolumeUQList[0].Code;
			AssertEquals("Valid item", false, line.JI_VolumeUQInfo.HasErrors());
		}

		public void TestValidateJI_ContainerMode()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.CusContainers.AddNew();
			BaseJobComInvoiceHeader header = declaration.Invoices.AddNew();
			BaseJobComInvoiceLine line = header.JobComInvoiceLines.AddNew();

			declaration.JE_ContainerMode = line.JI_ContainerMode = Enterprise.Core.Constants.ContainerModes.Bulk;
			line.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
			AssertEquals("Invalid item which is not in containerised mode but is linked to a container", true, line.JI_ContainerModeInfo.HasWarning("Invoice Line is not in containerized mode but is linked to a container"));

			line.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = false;
			AssertEquals("Valid item", false, line.JI_ContainerModeInfo.HasWarnings());

			line.JI_ContainerMode = "XXX";
			AssertEquals("XXX not in the list", true, line.JI_ContainerModeInfo.HasErrors());

			line.JI_ContainerMode = string.Empty;
			AssertEquals("Blank is valid item", false, line.JI_ContainerModeInfo.HasNotifications());

			line.JI_ContainerMode = Enterprise.Core.Constants.ContainerModes.Containerised;
			AssertEquals("Invalid item which is in containerised mode but is not linked to a container", true, line.JI_ContainerModeInfo.HasWarning("Invoice Line is in containerized mode but is not linked to a container, a container can be associated to all invoice lines from the context menu on the Containers grid on the container sub tab"));

			line.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
			AssertEquals("Valid item", false, line.JI_ContainerModeInfo.HasWarnings());

			var invoiceLineMock = Factory.NewMoq<BaseJobComInvoiceLine>();
			line = invoiceLineMock.Object;
			line.JI_JZ = header.PK;
			header.JobComInvoiceLines.Add(line);
			invoiceLineMock.Setup(x => x.IsContainerLinkMandatory).Returns(false);

			line.JI_ContainerMode = Enterprise.Core.Constants.ContainerModes.Bulk;
			line.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
			AssertEquals("Shouldn't validate if IsContainerLinkMandatory = false", false, line.JI_ContainerModeInfo.HasWarnings());
		}

		public void TestJI_ClassificationInvoiceUQDescriptionWhenNewPart()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			var header = declaration.Invoices.AddNew();
			var invoiceLine = header.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "NEWPART";
			invoiceLine.JI_CC = ZGuid.Empty;
			invoiceLine.JI_InvoiceUQ = ZString.Empty;
			invoiceLine.JI_Description = ZString.Empty;
			invoiceLine.JI_Tariff = ZString.Empty;

			invoiceLine.Validation.ValidateAll();

			AssertHasWarning("Classification has warning", invoiceLine.JI_CCInfo, BaseJobComInvoiceLineValidation.MandatoryCCOrTariffForAutoCreateProduct);
			AssertHasWarning("Tariff has warning", invoiceLine.JI_TariffInfo, BaseJobComInvoiceLineValidation.MandatoryCCOrTariffForAutoCreateProduct);
			AssertHasWarning("InvoiceUQ has warning", invoiceLine.JI_InvoiceUQInfo, BaseJobComInvoiceLineValidation.MandatoryForAutoCreateProduct);
			AssertHasWarning("Description has warning", invoiceLine.JI_DescriptionInfo, BaseJobComInvoiceLineValidation.MandatoryForAutoCreateProduct);

			invoiceLine.JI_Tariff = "8008";
			AssertNoWarning("Classification has warning", invoiceLine.JI_CCInfo, BaseJobComInvoiceLineValidation.MandatoryCCOrTariffForAutoCreateProduct);
			AssertNoWarning("Tariff has warning", invoiceLine.JI_TariffInfo, BaseJobComInvoiceLineValidation.MandatoryCCOrTariffForAutoCreateProduct);

			var classification = Factory.New<BaseCusClassification>();
			classification.CC_LookupCode = "NEWCODE";
			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			classification.CC_IsActive = true;
			Factory.Save();

			invoiceLine.JI_CC = classification.PK;
			invoiceLine.JI_Tariff = ZString.Empty;
			AssertNoWarning("Classification has warning", invoiceLine.JI_CCInfo, BaseJobComInvoiceLineValidation.MandatoryCCOrTariffForAutoCreateProduct);
			AssertNoWarning("Tariff has warning", invoiceLine.JI_TariffInfo, BaseJobComInvoiceLineValidation.MandatoryCCOrTariffForAutoCreateProduct);

			invoiceLine.JI_CC = classification.PK;
			invoiceLine.JI_InvoiceUQ = "KG";
			invoiceLine.JI_Description = "123";
			invoiceLine.JI_Tariff = "";

			invoiceLine.Validation.ValidateAll();

			AssertNoWarning("Classification has no warning", invoiceLine.JI_CCInfo, BaseJobComInvoiceLineValidation.MandatoryCCOrTariffForAutoCreateProduct);
			AssertNoWarning("InvoiceUQ has no warning", invoiceLine.JI_InvoiceUQInfo, BaseJobComInvoiceLineValidation.MandatoryForAutoCreateProduct);
			AssertNoWarning("Description has no warning", invoiceLine.JI_DescriptionInfo, BaseJobComInvoiceLineValidation.MandatoryForAutoCreateProduct);
		}

		public void TestChangingUnitsOrPartValidatesCustomsQty()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			var mockInvoiceLine = Factory.NewMoq<BaseJobComInvoiceLine>();
			mockInvoiceLine.Setup(x => x.NeedsCustomsQuantity).Returns(true);

			var invoiceLine = mockInvoiceLine.Object;
			invoiceLine.JI_JZ = invoice.PK;
			invoice.JobComInvoiceLines.Add(invoiceLine);
			invoiceLine.JI_JZ = invoice.PK;
			const string messageErrorText = "Customs Qty should be greater than zero. Please enter an Invoice Qty and the value will be translated into a customs qty.";
			invoiceLine.JI_InvoiceUQ = "KG";
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.Validation.ValidateJI_CustomsQuantity();
			Assert("Does have MessageErrors", invoiceLine.JI_CustomsQuantityInfo.GetMessageErrors().ContainsNotificationContaining(messageErrorText));
			invoiceLine.JI_InvoiceUQ = "";
			Assert("Now Does not have MessageErrors", !invoiceLine.JI_CustomsQuantityInfo.GetMessageErrors().ContainsNotificationContaining(messageErrorText));
			invoiceLine.JI_InvoiceUQ = "KG";
			Assert("Now Does have MessageErrors", invoiceLine.JI_CustomsQuantityInfo.GetMessageErrors().ContainsNotificationContaining(messageErrorText));
			using (invoiceLine.GetValidationSuspender())
			{
				invoiceLine.JI_CustomsUnitQty = "";
			}
			Assert("Still has MessageErrors", invoiceLine.JI_CustomsQuantityInfo.GetMessageErrors().ContainsNotificationContaining(messageErrorText));
			invoiceLine.JI_PartNo = "XXXX";
			Assert("Now Does not have MessageErrors", !invoiceLine.JI_CustomsQuantityInfo.GetMessageErrors().ContainsNotificationContaining(messageErrorText));
		}

		public void TestCheckJI_NetWeight()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_Weight = 0;
			invoiceLine.JI_WeightUQ = ZString.Empty;
			invoiceLine.JI_NetWeight = 0;
			invoiceLine.JI_NetWeightUQ = ZString.Empty;
			AssertNoWarning("NetWeight has no warning", invoiceLine.JI_NetWeightInfo, BaseJobComInvoiceLineValidation.WarningNetWeightIsGreaterThanGrossWeight);

			invoiceLine.JI_Weight = 100;
			invoiceLine.JI_NetWeight = 200;
			AssertNoWarning("NetWeight has no warning", invoiceLine.JI_NetWeightInfo, BaseJobComInvoiceLineValidation.WarningNetWeightIsGreaterThanGrossWeight);

			invoiceLine.JI_WeightUQ = "XX";
			invoiceLine.JI_NetWeightUQ = "KK";
			AssertNoWarning("NetWeight has no warning", invoiceLine.JI_NetWeightInfo, BaseJobComInvoiceLineValidation.WarningNetWeightIsGreaterThanGrossWeight);

			invoiceLine.JI_Weight = 100;
			invoiceLine.JI_WeightUQ = "KG";
			invoiceLine.JI_NetWeightUQ = "KG";
			invoiceLine.JI_NetWeight = 0;
			AssertNoWarning("NetWeight has no warning", invoiceLine.JI_NetWeightInfo, BaseJobComInvoiceLineValidation.WarningNetWeightIsGreaterThanGrossWeight);

			invoiceLine.JI_NetWeight = 200;
			AssertHasWarning("NetWeight has warning(NetWeight:200KG, GrossWeight:100KG)", invoiceLine.JI_NetWeightInfo, BaseJobComInvoiceLineValidation.WarningNetWeightIsGreaterThanGrossWeight);

			invoiceLine.JI_NetWeightUQ = "HG";
			AssertNoWarning("NetWeight has no warning(NetWeight:200HG, GrossWeight:100KG)", invoiceLine.JI_NetWeightInfo, BaseJobComInvoiceLineValidation.WarningNetWeightIsGreaterThanGrossWeight);

			invoiceLine.JI_WeightUQ = "HG";
			AssertHasWarning("NetWeight has warning(NetWeight:200HG, GrossWeight:100HG)", invoiceLine.JI_NetWeightInfo, BaseJobComInvoiceLineValidation.WarningNetWeightIsGreaterThanGrossWeight);

			invoiceLine.JI_Weight = 300;
			AssertNoWarning("NetWeight has no warning(NetWeight:200HG, GrossWeight:300HG)", invoiceLine.JI_NetWeightInfo, BaseJobComInvoiceLineValidation.WarningNetWeightIsGreaterThanGrossWeight);
		}

		public void TestCheckJI_AddInfo()
		{
			BaseJobComInvoiceLine invoiceLine = Factory.New<BaseJobComInvoiceLineWithBaseAddInfoAndNAddInfoSupporterForTesting>();
			invoiceLine.JI_AddInfo = "DutyMode=1*CIQBrand=品牌*CIQModel=型号*CIQSpec=规格";
			AssertNoError(invoiceLine.JI_AddInfoInfo, EnglishCharactersValidation.GetNotificationMessage(invoiceLine.JI_AddInfoInfo));

			invoiceLine = Factory.New<BaseJobComInvoiceLineWithBaseAddInfoForTesting>();
			invoiceLine.JI_AddInfo = "DutyMode=1*CIQBrand=品牌*CIQModel=型号*CIQSpec=规格";
			AssertHasError(invoiceLine.JI_AddInfoInfo, EnglishCharactersValidation.GetNotificationMessage(invoiceLine.JI_AddInfoInfo));
		}

		public void TestCheckJI_LineNo()
		{
			var errMsgLineExceedingMaxValue = $"Please enter a valid Line Number; valid number should be greater than zero and no greater than {short.MaxValue}.";

			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			var invalidLineNumberZero = Factory.New<BaseJobComInvoiceLine>();
			var invalidNegativeLineNumber = Factory.New<BaseJobComInvoiceLine>();
			var validaLineNumber = Factory.New<BaseJobComInvoiceLine>();
			((INeedRow)invalidNegativeLineNumber).Row["JI_LineNo"] = -1;
			((INeedRow)invalidNegativeLineNumber).Row["JI_JZ"] = invoice.PK.ToGuid();

			((INeedRow)invalidLineNumberZero).Row["JI_LineNo"] = 0;
			((INeedRow)invalidLineNumberZero).Row["JI_JZ"] = invoice.PK.ToGuid();

			((INeedRow)validaLineNumber).Row["JI_LineNo"] = 1;
			((INeedRow)validaLineNumber).Row["JI_JZ"] = invoice.PK.ToGuid();
			Factory.Save();

			validaLineNumber.Validation.ValidateJI_LineNo();
			AssertNoMessageError(validaLineNumber.JI_LineNoInfo, errMsgLineExceedingMaxValue);

			invalidLineNumberZero.Validation.ValidateJI_LineNo();
			AssertHasMessageError(invalidLineNumberZero.JI_LineNoInfo, errMsgLineExceedingMaxValue);

			invalidNegativeLineNumber.Validation.ValidateJI_LineNo();
			AssertHasMessageError(invalidNegativeLineNumber.JI_LineNoInfo, errMsgLineExceedingMaxValue);
		}

		public void TestCheckJI_CustomsThirdQuantity()
		{
			var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
			ValidationTestHelper.AssertErrorIfValueIsNegative(invoiceLine.JI_CustomsThirdQuantityInfo);
		}

		public void TestCheckJI_CustomsFourthQuantity()
		{
			var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
			ValidationTestHelper.AssertErrorIfValueIsNegative(invoiceLine.JI_CustomsFourthQuantityInfo);
		}

		public void TestCheckJI_CustomsSeconddQuantity()
		{
			var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
			ValidationTestHelper.AssertErrorIfValueIsNegative(invoiceLine.JI_CustomsSecondQuantityInfo);
		}

		public void TestCheckJI_CustomsFifthQuantity()
		{
			var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
			ValidationTestHelper.AssertErrorIfValueIsNegative(invoiceLine.JI_CustomsFifthQuantityInfo);
		}

		public void TestCheckJI_WeightUQ_Mandatory()
		{
			var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine.JI_Weight = 100m;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.JI_WeightUQInfo);
		}

		public void TestCheckJI_NetWeightUQ_Mandatory()
		{
			var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine.JI_NetWeight = 100m;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.JI_NetWeightUQInfo);
		}

		public void TestCheckJI_VolumeUQ_Mandatory()
		{
			var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine.JI_Volume = 100m;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.JI_VolumeUQInfo);
		}

		public void TestPartAttribute1Validation()
		{
			SetUpPartAttributeValidation();
			CheckPartAttributeValidation(line.JI_PartAttrib1Info, () => lineValidation.ValidateJI_PartAttrib1(), 1);
		}

		public void TestPartAttribute2Validation()
		{
			SetUpPartAttributeValidation();
			CheckPartAttributeValidation(line.JI_PartAttrib2Info, () => lineValidation.ValidateJI_PartAttrib2(), 2);
		}

		public void TestPartAttribute3Validation()
		{
			SetUpPartAttributeValidation();
			CheckPartAttributeValidation(line.JI_PartAttrib3Info, () => lineValidation.ValidateJI_PartAttrib3(), 3);
		}

		public void TestCheckJI_SerialNumber()
		{
			AssertSerialNumberValidation();
		}

		public void TestCheckJI_NewSerialNumber_MandatorySerialNumber()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Importer = GetNewImporterPK();
			declaration.Importer.CompanyData.OB_IMUsedBondedWhs = true;
			declaration.Importer.MiscServ.OM_IMUseSerialNumber = true;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "~~~";
			part.RelatedOrganisations.AddOrganisationIfNotExist(declaration.Importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			part.RelatedOrganisations[0].OU_UseSerialNumber = true;
			invoiceLine.JI_PartNo = "~~~";

			AssertNotNull("Not null part", invoiceLine.Part);

			var lineValidation = new BaseJobComInvoiceLineValidation(invoiceLine);

			lineValidation.ValidateJI_SerialNumber();
			AssertHasError(invoiceLine.JI_SerialNumberInfo, "Please enter a Serial Number.");

			invoiceLine.JI_SerialNumber = "ABC";
			lineValidation.ValidateJI_SerialNumber();
			AssertNoErrors(invoiceLine.JI_SerialNumberInfo);
		}

		public void TestCheckJI_PreviousEntryLineNumber()
		{
			const string messageError = "Either an Allocation Key should be provided (in the case of processed goods) or a reference to the Previous Entry Line Number should be provided but not both";
			var invoiceLine = Factory.NewWithValidTestData<BaseJobComInvoiceLine>();
			invoiceLine.ComponentInventoryCollection.RemoveAndDeleteAll();
			invoiceLine.JI_PreviousEntryLineNumber = 15;
			AssertNoMessageError(invoiceLine.JI_PreviousEntryLineNumberInfo, messageError);
			invoiceLine.ComponentInventoryCollection.AddNew();
			invoiceLine.JI_PreviousEntryLineNumber = 16;
			AssertHasMessageError(invoiceLine.JI_PreviousEntryLineNumberInfo, messageError);
			invoiceLine.JI_PreviousEntryLineNumber = 0;
			AssertNoMessageError(invoiceLine.JI_PreviousEntryLineNumberInfo, messageError);
		}

		public void TestCheckJI_PreviousEntryNumber()
		{
			const string messageError = "Either an Allocation Key should be provided (in the case of processed goods) or a reference to the Previous Entry Number should be provided but not both";
			var invoiceLine = Factory.NewWithValidTestData<BaseJobComInvoiceLine>();
			invoiceLine.ComponentInventoryCollection.RemoveAndDeleteAll();
			invoiceLine.JI_PreviousEntryNumber = "ENT01";
			AssertNoMessageError(invoiceLine.JI_PreviousEntryNumberInfo, messageError);
			invoiceLine.ComponentInventoryCollection.AddNew();
			invoiceLine.JI_PreviousEntryNumber = "ENT02";
			AssertHasMessageError(invoiceLine.JI_PreviousEntryNumberInfo, messageError);
			invoiceLine.JI_PreviousEntryNumber = "";
			AssertNoMessageError(invoiceLine.JI_PreviousEntryNumberInfo, messageError);
		}

		public void TestCheckJI_BondedWHSOrderNumber()
		{
			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				var procedure = GenerateProcedureForBondedWarehouse(procedureCode: "AB", "71", WarehouseMoveStatus.Codes.Yes);
				Factory.Save();
				var docket = GenerateWHSOrder("DocketID", 14);

				var invoiceLine = GenerateInvoiceLineForWarehouseEnable(true);
				invoiceLine.JI_Procedure = "AB71";
				invoiceLine.JI_BondedWHSOrderNumber = ZString.Empty;
				AssertNoMessageErrorContaining("JI_BondedWHSOrderLineNumber is empty then JI_BondedWHSOrderNumber is not mandatory.", invoiceLine.JI_BondedWHSOrderNumberInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.JI_BondedWHSOrderLineNumber = 14;
				AssertHasMessageErrorContaining("JI_BondedWHSOrderLineNumber is not empty then JI_BondedWHSOrderNumber is mandatory.", invoiceLine.JI_BondedWHSOrderNumberInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.JI_BondedWHSOrderNumber = "DocketRCV";
				AssertNoMessageErrorContaining("JI_BondedWHSOrderLineNumber and JI_BondedWHSOrderNumber are not empty then there should be no error.", invoiceLine.JI_BondedWHSOrderNumberInfo, MandatoryValidation.YouHaveNotEntered);

				var message = "Please enter a valid Whs. Order No.";
				AssertHasMessageErrorContaining("JI_BondedWHSOrderNumber doesn't match any WHSdocketID so there should be an error.", invoiceLine.JI_BondedWHSOrderNumberInfo, message);

				invoiceLine.JI_BondedWHSOrderNumber = "DocketID";
				AssertNoMessageErrorContaining("JI_BondedWHSOrderNumber matches an existing WHSdocketID so there should be no error.", invoiceLine.JI_BondedWHSOrderNumberInfo, message);

				var invoiceLine2 = GenerateInvoiceLineForWarehouseEnable(false);
				invoiceLine2.JI_Procedure = "AB71";

				invoiceLine2.EntryInstruction.Warehouse.Header.CompanyData.OB_IMUsedBondedWhs = false;
				invoiceLine2.JI_BondedWHSOrderNumber = ZString.Empty;
				invoiceLine2.JI_BondedWHSOrderLineNumber = 14;
				AssertNoMessageErrorContaining("SupportWarehouseOrderLines is false then there is no validation on JI_BondedWHSOrderNumber.", invoiceLine2.JI_BondedWHSOrderNumberInfo, MandatoryValidation.YouHaveNotEntered);
			}
		}

		public void TestCheckJI_BondedWHSOrderNumber_FilteredOnType()
		{
			var procedure = GenerateProcedureForBondedWarehouse(procedureCode: "AB", "71", WarehouseMoveStatus.Codes.Yes);
			Factory.Save();
			var docketORD = GenerateWHSOrder("DocketORD", 5);
			var docketRCV = GenerateWHSReceive("DocketRCV", 6);

			var invoiceLine = GenerateInvoiceLineForWarehouseEnable(true);
			invoiceLine.JI_Procedure = "AB71";

			invoiceLine.JI_BondedWHSOrderLineNumber = 14;
			invoiceLine.JI_BondedWHSOrderNumber = "DocketRCV";
			AssertHasMessageErrorContaining("DocketRCV is not linked to an order so there should be an error.", invoiceLine.JI_BondedWHSOrderNumberInfo, "Please enter a valid Whs. Order No.");

			invoiceLine.JI_BondedWHSOrderNumber = "DocketORD";
			AssertNoMessageErrorContaining("DocketORD is linked to an order so there should be no error.", invoiceLine.JI_BondedWHSOrderNumberInfo, "Please enter a valid Whs. Order No.");
		}

		public void TestCheckJI_BondedWHSOrderLineNumber()
		{
			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				GenerateProcedureForBondedWarehouse(procedureCode: "AB", "71", WarehouseMoveStatus.Codes.Yes);
				Factory.Save();

				var docket = GenerateWHSOrder("DocketID", 12);

				var invoiceLine = GenerateInvoiceLineForWarehouseEnable(true);
				invoiceLine.JI_Procedure = "AB71";

				invoiceLine.JI_BondedWHSOrderLineNumber = 0;
				AssertNoMessageErrorContaining("JI_BondedWHSOrderNumber is empty then JI_BondedWHSOrderLineNumber is not mandatory.", invoiceLine.JI_BondedWHSOrderLineNumberInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.JI_BondedWHSOrderNumber = "DocketID";
				AssertHasMessageErrorContaining("JI_BondedWHSOrderNumber is not empty then JI_BondedWHSOrderLineNumber is mandatory.", invoiceLine.JI_BondedWHSOrderLineNumberInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.JI_BondedWHSOrderLineNumber = 10;
				AssertNoMessageErrorContaining("JI_BondedWHSOrderLineNumber and JI_BondedWHSOrderNumber are not empty then there should be no error.", invoiceLine.JI_BondedWHSOrderLineNumberInfo, MandatoryValidation.YouHaveNotEntered);

				var message = "Please enter a valid Whs. Order Line No.;";
				AssertHasMessageErrorContaining("JI_BondedWHSOrderLineNumber doesn't match any WHSDocketLine.WE_LineNo so there should be an error.", invoiceLine.JI_BondedWHSOrderLineNumberInfo, message);

				invoiceLine.JI_BondedWHSOrderLineNumber = 12;
				AssertNoMessageErrorContaining("JI_BondedWHSOrderLineNumber matches an existing WHSDocketLine.WE_LineNo so there should be no error.", invoiceLine.JI_BondedWHSOrderLineNumberInfo, message);

				var invoiceLine2 = GenerateInvoiceLineForWarehouseEnable(false);
				invoiceLine2.JI_BondedWHSOrderNumber = "DocketID";
				invoiceLine2.JI_Procedure = "AB71";
				AssertNoMessageErrorContaining("SupportWarehouseOrderLines is false then there is no validation on JI_BondedWHSOrderLineNumber.", invoiceLine2.JI_BondedWHSOrderLineNumberInfo, MandatoryValidation.YouHaveNotEntered);
			}
		}

		BaseJobComInvoiceLine GenerateInvoiceLineForWarehouseEnable(bool isWarehouseOrderEnabled)
		{
			var direction = isWarehouseOrderEnabled ? JobMessageTypeList.Codes.Import : JobMessageTypeList.Codes.Export;
			var whsDataTestHelper = new WhsDataTestHelper(Factory);
			whsDataTestHelper.Importer.CompanyData.OB_IMUsedBondedWhs = false;
			whsDataTestHelper.Owner.CompanyData.OB_IMUsedBondedWhs = false;
			whsDataTestHelper.Warehouse.CompanyData.OB_IMUsedBondedWhs = true;
			whsDataTestHelper.Warehouse2.CompanyData.OB_IMUsedBondedWhs = true;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			helper.CreateNewOrGetExistingDataGrouping(currentCountry);
			var declarationMock = Factory.NewMoq<BaseJobDeclarationWithEntryInstructions>();
			var declaration = declarationMock.Object;
			declarationMock.Protected()
				.Setup<bool>("IsWarehouseOrderFunctionActivatedCore")
				.Returns(true);

			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "AB";
			instruction.CEI_OH_Owner = whsDataTestHelper.Owner.PK;
			instruction.CEI_OA_Warehouse = whsDataTestHelper.Warehouse.MainAddress.PK;
			instruction.CEI_OA_Warehouse2 = whsDataTestHelper.Warehouse2.MainAddress.PK;

			declaration.JE_MessageType = direction;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			return invoiceLine;
		}

		RefCusProcedure GenerateProcedureForBondedWarehouse(string procedureCode, string previousProcedureCode, string outOfWarehouse)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			helper.CreateNewOrGetExistingDataGrouping(currentCountry);

			var procedure = helper.CreateRefCusProcedure(currentCountry, ZString.Empty, procedureCode, previousProcedureCode, ZString.Empty, "description", "IMP");
			procedure.ZZ6_OutOfWarehouse = outOfWarehouse;
			return procedure;
		}

		IWhsOrder GenerateWHSOrder(string docketID, ZShort lineNo)
		{
			var orgheader = Factory.NewWithValidTestData<OrgHeader>();
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var helper = new WhsDataTestHelper(Factory);
			var whsHelper = helper.WhsHelper;
			var whsWarehouse = helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, true, "N10");
			var whsOrder = (IWhsOrder)whsHelper.CreateWhsOrder(helper.Importer.PK, whsWarehouse.PK, orgheader.PK, "01");
			whsOrder.WD_DocketID = docketID;
			whsHelper.SetOrderType(whsOrder.PK, "ORD");

			var docketLine = (IWhsDocketLine)whsHelper.CreateWhsOrderLine(whsOrder.PK, helper.Part.PK, 5m, "", "XJ5-00003877", "2001.10.0000", "US", "E", 7m, 15m, "KG", 13m, "CPS", "VisaNo=2", 5m, manufacturer.MainAddress.PK, "P");
			docketLine.WE_LineNo = lineNo;
			Factory.Save();

			return whsOrder;
		}

		IWhsReceive GenerateWHSReceive(string docketID, ZShort lineNo)
		{
			var helper = new WhsDataTestHelper(Factory);
			var whsWarehouse = helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, true, "N11");
			var whsReceive = helper.GetNewWhsReceive(whsWarehouse.PK, helper.Importer.PK);
			whsReceive.WD_DocketID = docketID;
			var whsReceiveLine1 = helper.GetNewWhsReceiveLine(whsReceive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, "ENT3243-1");
			whsReceiveLine1.WE_LineNo = lineNo;
			var whsReceiveLine1CustomsData = helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine1.PK, 1000m, 50m, "KG", "AU", 100m, "NO", "", "EN00123", (ZShort)1);
			var whsInventory = whsReceiveLine1.Inventory;
			Factory.Save();

			return whsReceive;
		}

		BaseJobDeclaration dec;
		BaseJobComInvoiceLine line;
		BaseJobComInvoiceLineValidation lineValidation;

		void SetUpPartAttributeValidation()
		{
			dec = Factory.New<BaseJobDeclaration>();
			dec.JE_OH_Importer = GetNewImporterPK();
			line = dec.Invoices.AddNew().JobComInvoiceLines.AddNew();

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "~~~";
			part.RelatedOrganisations.AddOrganisationIfNotExist(dec.Importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			line.JI_PartNo = "~~~";

			AssertNotNull("Not null part", line.Part);

			lineValidation = new BaseJobComInvoiceLineValidation(line);
		}

		const string TestValidPartNo = "T44330";
		const string TestTariffNumber = "2203.00.30";

		ZGuid GetNewImporterPK()
		{
			OrgHeader importer = GetNewImporter();
			return importer.PK;
		}

		OrgHeader GetNewImporter()
		{
			OrgHeader importer = OrgHeader.New(Factory);
			importer.FillWithValidTestData();
			importer.OH_IsConsignee = true;
			importer.OH_FullName = "Test Importer";
			importer.MainAddress.OA_Address1 = "1 Importer Lane";
			importer.OH_Code = "IMP2293SYD";
			return importer;
		}

		ZGuid GetNewSupplierPK()
		{
			OrgHeader supplier = GetNewSupplier();
			return supplier.PK;
		}

		OrgHeader GetNewSupplier()
		{
			OrgHeader supplier = OrgHeader.New(Factory);
			supplier.FillWithValidTestData();
			supplier.OH_IsConsignor = true;
			supplier.OH_FullName = "Test Supplier";
			supplier.MainAddress.OA_Address1 = "1 Supplier Court";
			supplier.OH_Code = "SUP3348USA";
			return supplier;
		}

		OrgSupplierPart GetNewPart(ZString partNo, OrgHeader importer, OrgHeader supplier)
		{
			OrgSupplierPart result = Factory.New<OrgSupplierPart>();
			result.OP_PartNum = partNo;
			result.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);
			result.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			return result;
		}

		OrgSupplierPart GetNewPartWithClassification(string partNo, ZGuid importerPK, ZGuid supplierPK)
		{
			OrgSupplierPart result = Factory.New<OrgSupplierPart>();
			result.OP_PartNum = partNo;

			OrgPartRelation importerRelation = result.RelatedOrganisations.AddNew();
			importerRelation.OU_OH = importerPK;
			importerRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			OrgPartRelation supplierRelation = result.RelatedOrganisations.AddNew();
			supplierRelation.OU_OH = supplierPK;
			supplierRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			BaseCusClassification classification = Factory.New<BaseCusClassification>();
			classification.CC_LookupCode = "TestLookup";
			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.Both;
			classification.CC_TariffNum = TestTariffNumber;
			classification.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			BaseCusClassPartPivot partClassPivot = Factory.New<BaseCusClassPartPivot>();
			partClassPivot.CI_OP = result.PK;
			partClassPivot.CI_CC = classification.PK;

			Factory.Save();
			return result;
		}

		TariffView SetupTariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var countryCode = GlbCompany.CurrentCompany.Country.RN_Code;
			var tariffType = helper.CreateNewOrGetExistingTariffType(countryCode, "HSN");
			Factory.Save();
			var tariff = helper.CreateTariff(countryCode, tariffType.PK, "Tariff1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();
			return tariff;
		}

		void SetupConditionData(TariffView tariff)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var countryCode = GlbCompany.CurrentCompany.Country.RN_Code;

			var ctrlType1 = helper.CreateOrGetExistingRefCusConditionType(countryCode, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, "TSTC1", "Test Ctrl Condition Type 1");
			var ctrlType2 = helper.CreateOrGetExistingRefCusConditionType(countryCode, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, "TSTC2", "Test Ctrl Condition Type 2");
			Factory.Save();

			var testValueType = helper.CreateOrGetExistingRefCusConditionValueType(countryCode, "TSTVT");

			var testCondCtrl1_1 = helper.CreateOrGetExistingRefCusCondition(countryCode, ctrlType1.PK, tariff.PK, "Direction:Import", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			testCondCtrl1_1.ZX1_Source = "www.google.com";
			testCondCtrl1_1.Factory.Save();
			helper.CreateOrGetExistingRefCusConditionValue(testValueType.PK, testCondCtrl1_1.PK, "XA", c => c.ZX3_LogicalORWithinGroup = 0);
			helper.CreateOrGetExistingRefCusConditionValue(testValueType.PK, testCondCtrl1_1.PK, "XB", c => c.ZX3_LogicalORWithinGroup = 0);
			helper.CreateOrGetExistingRefCusConditionValue(testValueType.PK, testCondCtrl1_1.PK, "XC", c => c.ZX3_LogicalORWithinGroup = 1);
			helper.CreateOrGetExistingRefCusConditionValue(testValueType.PK, testCondCtrl1_1.PK, "XD", c => c.ZX3_LogicalORWithinGroup = 2);
			helper.CreateOrGetExistingRefCusConditionValue(testValueType.PK, testCondCtrl1_1.PK, "XE", c => c.ZX3_LogicalORWithinGroup = 2);

			var testCondCtrl1_2 = helper.CreateOrGetExistingRefCusCondition(countryCode, ctrlType1.PK, tariff.PK, "Direction:Export", false, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusConditionValue(testValueType.PK, testCondCtrl1_2.PK, "YA");

			var testCondCtrl1_3 = helper.CreateOrGetExistingRefCusCondition(countryCode, ctrlType1.PK, tariff.PK, "Direction:Either", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusConditionValue(testValueType.PK, testCondCtrl1_3.PK, "YB");

			var testCondCtrl1_4 = helper.CreateOrGetExistingRefCusCondition(countryCode, ctrlType1.PK, tariff.PK, "Date: ExpiredYesterday", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-1));
			helper.CreateOrGetExistingRefCusConditionValue(testValueType.PK, testCondCtrl1_4.PK, "YC");

			var testCondCtrl2_1 = helper.CreateOrGetExistingRefCusCondition(countryCode, ctrlType2.PK, tariff.PK, "cond2_1", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			testCondCtrl2_1.Factory.Save();
			helper.CreateOrGetExistingRefCusConditionValue(testValueType.PK, testCondCtrl2_1.PK, "ZA");

			Factory.Save();
		}

		void SetupFormulaConditionData(TariffView tariff)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var countryCode = GlbCompany.CurrentCompany.Country.RN_Code;

			var ctrlType1 = helper.CreateOrGetExistingRefCusConditionType(countryCode, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, "TSTC1", "Test Ctrl Condition Type 1");
			var preference = helper.CreatePreferenceForCountry("Pref1", "TestPref1", countryCode);
			Factory.Save();

			var tradeGroupPAC = helper.LoadOrCreateTradeGroup(countryCode, "PAC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.AddCountry(tradeGroupPAC, "AU", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();

			var formulaValueType = helper.CreateOrGetExistingRefCusConditionValueType(countryCode, "FRMVT", afterCreate: f => f.ZX4_IsFormula = true);
			var testCondCtrl1_8 = helper.CreateOrGetExistingRefCusCondition(countryCode, ctrlType1.PK, tariff.PK, "AdditionalCode: AC, OrderNumber: ON", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			testCondCtrl1_8.ZX1_ZZS_Preference = preference.PK;
			helper.CreateOrGetExistingRefCusConditionValue(formulaValueType.PK, testCondCtrl1_8.PK, "[NO] > 2.00", c => c.ZX3_LogicalORWithinGroup = 0);
			helper.CreateOrGetExistingRefCusConditionValue(formulaValueType.PK, testCondCtrl1_8.PK, "[KGM] < 2000.00", c => c.ZX3_LogicalORWithinGroup = 0);
			helper.CreateOrGetExistingRefCusConditionValue(formulaValueType.PK, testCondCtrl1_8.PK, "[NO] > 20.00", c => c.ZX3_LogicalORWithinGroup = 1);
			helper.CreateOrGetExistingRefCusConditionValue(formulaValueType.PK, testCondCtrl1_8.PK, "[TNE] < 20.00", c => c.ZX3_LogicalORWithinGroup = 1);
			testCondCtrl1_8.Factory.Save();
			helper.CreateCusApplicability(testCondCtrl1_8, tradeGroupPAC, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, additionalCode: "AC", orderNumber: "ON");
			Factory.Save();
		}

		void SetupInformationConditionData(TariffView tariff)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var countryCode = GlbCompany.CurrentCompany.Country.RN_Code;

			var ctrlType1 = helper.CreateOrGetExistingRefCusConditionType(countryCode, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, "TSTC1", "Test Ctrl Condition Type 1");
			Factory.Save();

			var informationValueType = helper.CreateOrGetExistingRefCusConditionValueType(countryCode, "INF");
			var formulaValueType = helper.CreateOrGetExistingRefCusConditionValueType(countryCode, "FRMVT", afterCreate: f => f.ZX4_IsFormula = true);
			var testCondINF1_1 = helper.CreateOrGetExistingRefCusCondition(countryCode, ctrlType1.PK, tariff.PK, "Information condition", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusConditionValue(informationValueType.PK, testCondINF1_1.PK, "Information");
			helper.CreateOrGetExistingRefCusConditionValue(formulaValueType.PK, testCondINF1_1.PK, "[TNE] < 20.00", c => c.ZX3_LogicalORWithinGroup = 0);
			testCondINF1_1.Factory.Save();
		}

		void SetupRateConditionData(TariffView tariff)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var countryCode = GlbCompany.CurrentCompany.Country.RN_Code;
			var rateType1 = helper.CreateOrGetExistingRefCusConditionType(countryCode, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Rate, "TSTR1", "Test Rate Condition Type 1");
			Factory.Save();

			var testValueType = helper.CreateOrGetExistingRefCusConditionValueType(countryCode, "TSTVT");
			var testCondRate1_1 = helper.CreateOrGetExistingRefCusCondition(countryCode, rateType1.PK, tariff.PK, "rate1_1", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			testCondRate1_1.Factory.Save();
			helper.CreateOrGetExistingRefCusConditionValue(testValueType.PK, testCondRate1_1.PK, "R1");
			helper.CreateOrGetExistingRefCusConditionValue(testValueType.PK, testCondRate1_1.PK, "R2");

			Factory.Save();
		}

		JobComInvoiceLineForConditionTest CreateInvoiceLine()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = Factory.New<JobComInvoiceLineForConditionTest>();
			invoiceLine.JI_JZ = declaration.Invoices.AddNew().PK;
			invoiceLine.JI_PrimaryPreference = "Pref1";
			invoiceLine.JI_SecondaryPreference = "AC";
			invoiceLine.JI_ConcessionOrder = "ON";
			invoiceLine.JI_CountryOfOrigin = "AU";
			invoiceLine.JI_Tariff = "Tariff1";
			return invoiceLine;
		}

		void AssertSerialNumberValidation()
		{
			SetUpPartAttributeValidation();
			dec.Importer.CompanyData.OB_IMUsedBondedWhs = true;
			line.JI_SerialNumber = "ABC";
			lineValidation.ValidateJI_SerialNumber();
			AssertHasError(line.JI_SerialNumberInfo, "The part '~~~' is not set up to use Serial Number with the Importer 'Test Importer'. Please either remove the value 'ABC' from the Serial Number field, or set up the Product and Importer to use Serial Number.");
		}

		void CheckPartAttributeValidation(ZPropertyInfo attributeInfo, Action validationInvoker, int attributeNo)
		{
			using (new PartAttributeValidationChecker.AttributeCallChecker(dec.Importer, line.Part, attributeInfo, attributeNo))
			{
				validationInvoker.Invoke();
			}
		}

		void SetupUniversalTariff(ZString tariffCode, ZString universalTariffType)
		{
			var newFactory = new BusinessObjectFactory();
			var testHelper = new UniversalReferenceTestDataHelper(newFactory);
			var startDate = new ZDate(2022, 1, 10);
			var endDate = new ZDate(2079, 06, 06);
			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var tradeGroupStandard = testHelper.CreateTradeGroup(currentCountryCode, "STANDARD", startDate, endDate);
			testHelper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Australia, startDate, endDate);
			var hsnTariffType = testHelper.CreateNewOrGetExistingTariffType(currentCountryCode, universalTariffType);
			newFactory.Save();
			testHelper.CreateTariff(currentCountryCode, hsnTariffType.PK, tariffCode, startDate, endDate, tariffCode + " Description");
			newFactory.Save();
		}

		sealed class JobComInvoiceLineForConditionTest : BaseJobComInvoiceLine
		{
			public JobComInvoiceLineForConditionTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override ZString AdditionalCode => JI_SecondaryPreference;

			protected internal override bool UseUniversalConditionCheck => true;

			public override ConditionChecker.EvaluateConditionValue EvaluateConditionValue => (_, __, x) => false;
			public override ConditionChecker.GetFriendlyConditionValue GetFriendlyConditionValue => (_, __, input) => input + "Desc";
		}

		sealed class BaseJobDeclarationForTesting : BaseJobDeclaration
		{
			public BaseJobDeclarationForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool IsInvoiceQuantityRequiredForBondedWarehouseReturns { get; set; }
			public bool IsUNDGSupportedOnInvoiceLinesReturns { get; set; }
			public BondedWarehousingHelper GetNewBondedWarehousingHelperReturns { get; set; }

			protected internal override bool IsInvoiceQuantityRequiredForBondedWarehouse => IsInvoiceQuantityRequiredForBondedWarehouseReturns;
			protected internal override bool IsUNDGSupportedOnInvoiceLines => IsUNDGSupportedOnInvoiceLinesReturns;
			protected override BondedWarehousingHelper GetNewBondedWarehousingHelper() => GetNewBondedWarehousingHelperReturns;
		}

		sealed class BaseJobComInvoiceLineForTesting : BaseJobComInvoiceLine
		{
			public BaseJobComInvoiceLineForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
			public JobComInvoiceLineValidation GetNewValidationReturns { get; set; }

			protected override JobComInvoiceLineValidation GetNewValidation() => GetNewValidationReturns;
		}

		sealed class BaseJobComInvoiceLineWithBaseAddInfoAndNAddInfoSupporterForTesting : BaseJobComInvoiceLine, IAddInfoManager, INAddInfoSupporter
		{
			public BaseJobComInvoiceLineWithBaseAddInfoAndNAddInfoSupporterForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public TestAddInfo AddInfo => addInfo ??= new TestAddInfo(this);
			TestAddInfo addInfo;

			IAddInfo IAddInfoManager.AddInfo => AddInfo;
			public ZPropertyInfoString NAddInfoProperty => JI_NAddInfoInfo as ZPropertyInfoString;
		}

		sealed class BaseJobComInvoiceLineWithBaseAddInfoForTesting : BaseJobComInvoiceLine, IAddInfoManager
		{
			public BaseJobComInvoiceLineWithBaseAddInfoForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public TestAddInfo AddInfo => addInfo ??= new TestAddInfo(this);
			TestAddInfo addInfo;

			IAddInfo IAddInfoManager.AddInfo => AddInfo;
		}
	}
}

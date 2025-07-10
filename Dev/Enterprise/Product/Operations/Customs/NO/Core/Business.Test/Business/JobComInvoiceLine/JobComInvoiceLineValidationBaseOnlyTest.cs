using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(JobComInvoiceLineValidation))]
sealed class JobComInvoiceLineValidationBaseOnlyTest : JobComInvoiceLineValidationAbstractTest
{
	public void TestCheckJI_Weight()
	{
		CombineAssertions(() =>
		{
			var message = "Please enter a 'Gross Weight' greater than 0.";

			invoiceLine.JI_Weight = -1;
			AssertHasMessageErrorContaining(invoiceLine.JI_WeightInfo, message);

			invoiceLine.JI_Weight = 0;
			AssertHasMessageErrorContaining(invoiceLine.JI_WeightInfo, message);

			invoiceLine.JI_Weight = 1;
			AssertNoMessageErrors(invoiceLine.JI_WeightInfo);
		});
	}

	public void TestCheckJI_WeightUQ()
	{
		invoiceLine.JI_Weight = 1;
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceLine.JI_WeightUQInfo, "XX", Core.Constants.Weight.Kilograms);
	}

	public void TestCheckJI_NetWeight()
	{
		CombineAssertions(() =>
		{
			var message = $"Please enter a '{invoiceLine.JI_NetWeightInfo.HumanReadableName}' greater than 0.";

			invoiceLine.JI_NetWeight = -1;
			AssertHasMessageErrorContaining(invoiceLine.JI_NetWeightInfo, message);

			invoiceLine.JI_NetWeight = 0;
			AssertHasMessageErrorContaining(invoiceLine.JI_NetWeightInfo, message);

			invoiceLine.JI_NetWeight = 1;
			AssertNoMessageErrors(invoiceLine.JI_NetWeightInfo);
		});
	}

	public void TestCheckJI_NetWeightUQ()
	{
		invoiceLine.JI_NetWeight = 1;
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceLine.JI_NetWeightUQInfo, "XX", Core.Constants.Weight.Kilograms);
	}

	public void TestCheckJI_Procedure_NoMixOfProceduresWhen6021IsUsed() => CombineAssertions(() =>
	{
		var messageErrorText = "Procedure 6021 cannot be mixed with other procedures on same entry.";
		var entry = declaration.CustomsEntryInstructions.AddNew();
		entry.CEI_Procedure = "6021";
		invoiceLine.JI_CEI = entry.PK;

		invoiceLine.JI_Procedure = "6000";
		AssertHasMessageError("Procedure 6021 cannot be mixed", invoiceLine.JI_ProcedureInfo, messageErrorText);

		invoiceLine.JI_Procedure = "6021";
		AssertNoMessageError("No mix, no error", invoiceLine.JI_ProcedureInfo, messageErrorText);

		invoiceLine.JI_Procedure = string.Empty;
		AssertNoMessageError("Blank procedure is allowed", invoiceLine.JI_ProcedureInfo, messageErrorText);
	});

	public void TestCheckJI_ValuationCode()
	{
		var errorMessage = "The code you have selected is not in the list";
		CombineAssertions(() =>
		{
			invoiceLine.JI_ValuationCode = ValuationMethodList.Codes.ValueOfImportedGoods;
			AssertNoMessageErrorContaining("Valid", invoiceLine.JI_ValuationCodeInfo, errorMessage);

			invoiceLine.JI_ValuationCode = "x";
			AssertHasMessageErrorContaining("Invalid", invoiceLine.JI_ValuationCodeInfo, errorMessage);

			invoiceLine.JI_ValuationCode = "";
			AssertNoMessageErrorContaining("Empty", invoiceLine.JI_ValuationCodeInfo, errorMessage);
		});
	}

	public void TestCheckJI_CustomsSecondUnitQty_ShouldAddMessageErrorWhenInvalidCode() => CombineAssertions(() =>
	{
		var testHelper = new RefCusTariffTestHelper(Factory);
		testHelper.SetupGenericTariffData();

		invoiceLine.JI_Tariff = RefCusTariffTestHelper.TariffCodes.TariffWithOneExciseCode;
		ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_CustomsSecondUnitQtyInfo, "XX", "LTR");

		invoiceLine.JI_Tariff = RefCusTariffTestHelper.TariffCodes.TariffWithTwoExciseCode;
		ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_CustomsSecondUnitQtyInfo, "ZZ", ZString.Empty);
	});

	public void TestCheckJI_CustomsSecondUnitQty_ShouldBeMandatoryIfNotEnteredAndIfNotPresentAtOtherPlace()
	{
		AssertUnitQuantityShouldBeMandatoryIfNotEnteredAndIfNotPresentAtOtherPlace(
			invoiceLine.JI_CustomsSecondUnitQtyInfo,
			[invoiceLine.JI_CustomsThirdUnitQtyInfo, invoiceLine.JI_CustomsFourthUnitQtyInfo, invoiceLine.JI_CustomsFifthUnitQtyInfo]);
	}

	public void TestCheckJI_CustomsThirdUnitQty_ShouldAddMessageErrorWhenInvalidCode() => CombineAssertions(() =>
	{
		var codeDescriptionPairList = new CodeDescriptionPairList();
		codeDescriptionPairList.AddPair("COD", "Dummy Code");
		Factory.SetCachedValue(invoiceLine.Lookups.CustomsUQListCacheKey, codeDescriptionPairList);
		ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_CustomsThirdUnitQtyInfo, "XYZ", "COD");
	});

	public void TestCheckJI_CustomsThirdUnitQty_ShouldBeMandatoryIfNotEnteredAndIfNotPresentAtOtherPlace()
	{
		AssertUnitQuantityShouldBeMandatoryIfNotEnteredAndIfNotPresentAtOtherPlace(
			invoiceLine.JI_CustomsThirdUnitQtyInfo,
			[invoiceLine.JI_CustomsSecondUnitQtyInfo, invoiceLine.JI_CustomsFourthUnitQtyInfo, invoiceLine.JI_CustomsFifthUnitQtyInfo]);
	}

	public void TestCheckJI_CustomsThirdUnitQty_ShouldShowMessageErrorWhenQuantityIsEntered()
	{
		ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(invoiceLine.JI_CustomsThirdUnitQtyInfo, invoiceLine.JI_CustomsThirdQuantityInfo);
	}

	public void TestCheckJI_CustomsFourthUnitQty_ShouldAddMessageErrorWhenInvalidCode() => CombineAssertions(() =>
	{
		var codeDescriptionPairList = new CodeDescriptionPairList();
		codeDescriptionPairList.AddPair("COD", "Dummy Code");
		Factory.SetCachedValue(invoiceLine.Lookups.CustomsUQListCacheKey, codeDescriptionPairList);
		ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_CustomsFourthUnitQtyInfo, "XYZ", "COD");
	});

	public void TestCheckJI_CustomsFourthUnitQty_ShouldBeMandatoryIfNotEnteredAndIfNotPresentAtOtherPlace()
	{
		AssertUnitQuantityShouldBeMandatoryIfNotEnteredAndIfNotPresentAtOtherPlace(
			invoiceLine.JI_CustomsFourthUnitQtyInfo,
			[invoiceLine.JI_CustomsSecondUnitQtyInfo, invoiceLine.JI_CustomsThirdUnitQtyInfo, invoiceLine.JI_CustomsFifthUnitQtyInfo]);
	}

	public void TestCheckJI_CustomsFourthUnitQty_ShouldShowMessageErrorWhenQuantityIsEntered()
	{
		ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(invoiceLine.JI_CustomsFourthUnitQtyInfo, invoiceLine.JI_CustomsFourthQuantityInfo);
	}

	public void TestCheckJI_CustomsFifthUnitQty_ShouldAddMessageErrorWhenInvalidCode() => CombineAssertions(() =>
	{
		var codeDescriptionPairList = new CodeDescriptionPairList();
		codeDescriptionPairList.AddPair("COD", "Dummy Code");
		Factory.SetCachedValue(invoiceLine.Lookups.CustomsUQListCacheKey, codeDescriptionPairList);
		ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_CustomsFifthUnitQtyInfo, "XYZ", "COD");
	});

	public void TestCheckJI_CustomsFifthUnitQty_ShouldBeMandatoryIfNotEnteredAndIfNotPresentAtOtherPlace()
	{
		AssertUnitQuantityShouldBeMandatoryIfNotEnteredAndIfNotPresentAtOtherPlace(
			invoiceLine.JI_CustomsFifthUnitQtyInfo,
			[invoiceLine.JI_CustomsSecondUnitQtyInfo, invoiceLine.JI_CustomsThirdUnitQtyInfo, invoiceLine.JI_CustomsFourthUnitQtyInfo]);
	}

	public void TestCheckJI_CustomsFifthUnitQty_ShouldShowMessageErrorWhenQuantityIsEntered()
	{
		ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(invoiceLine.JI_CustomsFifthUnitQtyInfo, invoiceLine.JI_CustomsFifthQuantityInfo);
	}

	void AssertUnitQuantityShouldBeMandatoryIfNotEnteredAndIfNotPresentAtOtherPlace(ZPropertyInfo additionalQuantity, ZPropertyInfo[] otherAdditionalQuantities)
	{
		otherAdditionalQuantities = otherAdditionalQuantities ?? [];
		AssertEquals("[PRE-CONDITION] Other Additional Quantities should be provided", expected: true, otherAdditionalQuantities.Length > 0);

		additionalQuantity.Value = ZString.Empty;
		CombineAssertions($"When lookup list for {additionalQuantity.Name} is empty", () =>
		{
			var emptyCodeList = new CodeDescriptionPairList();
			SetLookup(emptyCodeList);
			AssertEquals($"[PRE-CONDITION] {additionalQuantity.Name} should be empty, not defaulted via tariff", expected: ZString.Empty, additionalQuantity.Value);
			ValidationTestHelper.AssertFieldIsNotMandatory(additionalQuantity);
		});

		var populatedCodeList = new CodeDescriptionPairList();
		for (var i = 0; i < otherAdditionalQuantities.Length; i++)
		{
			populatedCodeList.AddPair($"FK{i + 1}", "Fake Quantity");
		}
		SetLookup(populatedCodeList);

		CombineAssertions($"When lookup list for {additionalQuantity.Name} is not empty", () =>
		{
			AssertEquals($"[PRE-CONDITION] {additionalQuantity.Name} should be empty, not defaulted via tariff", expected: ZString.Empty, additionalQuantity.Value);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(additionalQuantity);

			additionalQuantity.Value = (ZString)populatedCodeList[0].Code;
			AssertNoMessageError("When one of the value from the lookup is set", additionalQuantity, MandatoryValidation.YouHaveNotEntered);

			otherAdditionalQuantities.Select((propertyInfo, index) => (propertyInfo, index))
				.ForEach(v => v.propertyInfo.Value = (ZString)populatedCodeList[v.index].Code);
			additionalQuantity.Value = ZString.Empty;
			ValidationTestHelper.AssertFieldIsNotMandatory(additionalQuantity, MandatoryValidation.YouHaveNotEntered, "When all other quantities are set with required UOMs, should not show any message error");

			var anotherCodeList = new CodeDescriptionPairList(populatedCodeList);
			anotherCodeList.AddPair("NCD", "New Code");
			SetLookup(anotherCodeList);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(additionalQuantity);
		});

		void SetLookup(CodeDescriptionPairList codeDescriptionPairList)
		{
			Factory.SetCachedValue(invoiceLine.Lookups.CustomsUQListCacheKey, codeDescriptionPairList);
			Factory.SetCachedValue(invoiceLine.Lookups.CustomsSecondUnitQtyListCacheKey, codeDescriptionPairList);
		}
	}

	public void TestCheckJI_CustomsSecondQuantity()
	{
		CombineAssertions(() =>
		{
			invoiceLine.JI_CustomsSecondUnitQty = "LTR";
			invoiceLine.JI_CustomsSecondQuantity = -5m;
			AssertHasMessageError(invoiceLine.JI_CustomsSecondQuantityInfo, "This tariff number requires other unit (see type) to be greater than zero.");

			invoiceLine.JI_CustomsSecondQuantity = 0m;
			AssertHasMessageError(invoiceLine.JI_CustomsSecondQuantityInfo, "This tariff number requires other unit (see type) to be greater than zero.");

			invoiceLine.JI_CustomsSecondQuantity = 5m;
			AssertNoMessageError(invoiceLine.JI_CustomsSecondQuantityInfo, "This tariff number requires other unit (see type) to be greater than zero.");

			invoiceLine.JI_CustomsSecondUnitQty = ZString.Empty;
			invoiceLine.JI_CustomsSecondQuantity = -5m;
			AssertNoMessageError(invoiceLine.JI_CustomsSecondQuantityInfo, "Other unit cannot be given when type is blank.");

			invoiceLine.JI_CustomsSecondQuantity = 0m;
			AssertNoMessageError(invoiceLine.JI_CustomsSecondQuantityInfo, "Other unit cannot be given when type is blank.");

			invoiceLine.JI_CustomsSecondQuantity = 5m;
			AssertHasMessageError(invoiceLine.JI_CustomsSecondQuantityInfo, "Other unit cannot be given when type is blank.");
		});
	}

	public void TestCheckJI_Description()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.JI_DescriptionInfo);
	}

	public void TestRateSelectionCriteriaLists() => CombineAssertions(() =>
	{
		var validation = new JobComInvoiceLineValidationForTesting(invoiceLine);
		var criteria = validation.RateSelectionCriteriaListsExposed;

		AssertType<List<IZZRateSelectionCriteria>>(criteria);
		AssertContainsExactElementsInAnyOrder([invoiceLine.DutyRateSelectionCriteria, invoiceLine.NormalTariffDutyRateSelectionCriteria, invoiceLine.ExciseRateSelectionCriteria], criteria);
	});

	public void TestValidateSupportingDocumentType()
	{
		const string expectedMessageError = "ALL invoice lines must have a supporting document of type FOR or TXT when Entry Instruction Decl. Sub Type is Preliminary";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		var supportingDocument = invoiceLine.SupportingDocuments.AddNew();

		CombineAssertions(() =>
		{
			entryInstruction.CEI_SubStyle = ImportDeclarationSubTypes.Codes.P;
			supportingDocument.CSI_Code = "NON";
			invoiceLine.Validation.ValidateAll();
			AssertHasRowMessageError("When CEI_SubStyle: P, CSI_Code: NON", invoiceLine, expectedMessageError);

			entryInstruction.CEI_SubStyle = ImportDeclarationSubTypes.Codes.N;
			invoiceLine.Validation.ValidateAll();
			AssertNoRowMessageError("When CEI_SubStyle: N, CSI_Code: NON", invoiceLine, expectedMessageError);

			entryInstruction.CEI_SubStyle = ImportDeclarationSubTypes.Codes.P;
			supportingDocument.CSI_Code = "FOR";
			invoiceLine.Validation.ValidateAll();
			AssertNoRowMessageError("When CEI_SubStyle: P, CSI_Code: FOR", invoiceLine, expectedMessageError);

			supportingDocument.CSI_Code = "TXT";
			invoiceLine.Validation.ValidateAll();
			AssertNoRowMessageError("When CEI_SubStyle: P, CSI_Code: TXT", invoiceLine, expectedMessageError);
		});
	}

	public void TestValidRatesNotificationSeverity()
	{
		var validation = new JobComInvoiceLineValidationForTesting(invoiceLine);
		AssertEquals("ValidRatesNotificationSeverity should be Warning", CargoWise.EntityFramework.NotificationType.Warning, validation.ValidRatesNotificationSeverityExposed);
	}

	public void TestCheckJI_Tariff_WarningWhereValidRatesExist()
	{
		var dateStart = new ZDate(2025, 1, 1);
		var dateEnd = new ZDate(2025, 12, 31);
		var norway = Core.Constants.CountryCodes.Norway;
		var sweden = Core.Constants.CountryCodes.Sweden;
		var tariffCode = "12345611";
		var fa100Code = "FA100";

		var testHelper = new UniversalReferenceTestDataHelper(Factory);

		var tradeGroupStandard = testHelper.CreateTradeGroup(norway, "TALL", dateStart, dateEnd);
		_ = testHelper.AddCountry(tradeGroupStandard, sweden, dateStart, dateEnd);
		Factory.Save();

		var hsnTariffType = testHelper.CreateNewOrGetExistingTariffType(norway, Universal.Constants.TariffTypes.HarmonizedSystem);
		Factory.Save();
		var cusTariff = testHelper.CreateTariff(norway, hsnTariffType.PK, tariffCode, dateStart, dateEnd, "dummy Description");
		var dutyRateType = testHelper.CreateNewOrGetExistingRateType(norway, Universal.Constants.RateTypes.Excise, "Excise");
		var rateCode = testHelper.LoadOrCreateNewCusRateCode(Factory, fa100Code, dutyRateType.PK, description: "FA100 description");
		var rate = testHelper.CreateRate(cusTariff, rateCode.PK, dateStart, dateEnd, "0.05 * VFD", dataGrouping: norway);
		var preferenceN = testHelper.CreatePreferenceForCountry("N", "Standard", norway);
		Factory.Save();

		var testRate1 = testHelper.CreateRate(cusTariff, rateCode.PK, dateStart, dateEnd, "0.10 * VFD", preferencePk: preferenceN.PK, dataGrouping: norway);
		_ = testHelper.CreateCusApplicability(testRate1, tradeGroupStandard, dateStart, dateEnd, additionalCode: fa100Code);
		Factory.Save();

		var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
		declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
		invoiceLine.JI_Tariff = tariffCode;
		invoiceLine.JI_CountryOfOrigin = sweden;

		CombineAssertions(() =>
		{
			const string ratesExistWhere = "rates exist where";

			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining("We should have Message Error: 'There is no applicable Excise rate....'", invoiceLine.JI_TariffInfo, "applicable Excise rate");
			AssertHasWarningContaining("We should have Message warning: 'Valid Excise rates exist...'", invoiceLine.JI_TariffInfo, ratesExistWhere);

			invoiceLine.JI_PrimaryPreference = "N";
			invoiceLine.JI_SupplementaryCode1 = fa100Code;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoWarningContaining("Warning is removed", invoiceLine.JI_TariffInfo, ratesExistWhere);
		});
	}

	public void TestCheckReducedCustomsFlagValidCode()
	{
		ValidationTestHelper.AssertErrorIfInvalidCode(invoiceLine.JI_ReducedCustomsFlagInfo, "X", "S");
	}

	public void TestCheckJI_PackageType()
	{
		var testHelper = new RefCusTariffTestHelper(Factory);
		testHelper.SetupGenericTariffData();
		invoiceLine.JI_Tariff = "33333333";
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceLine.JI_PackageTypeInfo, "X", "A");
	}

	public void TestCheckJI_CustomsRateOverrideType()
	{
		CombineAssertions(() =>
		{
			invoiceLine.CustomsRateIsOverridden = true;
			invoiceLine.JI_CustomsRateOverrideType = RateTypeCodeList.Codes.Kilogram;
			AssertNoMessageErrorContaining(invoiceLine.JI_CustomsRateOverrideTypeInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.CustomsRateIsOverridden = true;
			invoiceLine.JI_CustomsRateOverrideType = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.JI_CustomsRateOverrideTypeInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.CustomsRateIsOverridden = false;
			invoiceLine.JI_CustomsRateOverrideType = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceLine.JI_CustomsRateOverrideTypeInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckJI_CustomsRateOverrideType_ShouldValidateList()
	{
		SetupTariffs();
		invoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		invoiceLine.JI_PrimaryPreference = "N";
		invoiceLine.JI_CountryOfOrigin = "US";
		invoiceLine.JI_Tariff = TariffConstants.KGTariff;

		CombineAssertions(() =>
		{
			invoiceLine.CustomsRateIsOverridden = true;
			invoiceLine.JI_CustomsRateOverrideType = "L";
			AssertHasMessageError(invoiceLine.JI_CustomsRateOverrideTypeInfo, "The code you have selected is not in the list.");

			invoiceLine.JI_CustomsRateOverrideType = "K";
			AssertNoMessageError(invoiceLine.JI_CustomsRateOverrideTypeInfo, "The code you have selected is not in the list.");

			invoiceLine.JI_CustomsRateOverrideType = "L";
			invoiceLine.CustomsRateIsOverridden = false;
			AssertNoMessageError(invoiceLine.JI_CustomsRateOverrideTypeInfo, "The code you have selected is not in the list.");
		});
	}

	static class TariffConstants
	{
		public const string KGTariff = "10001000";
	}

	void SetupTariffs()
	{
		var tariffTestHelper = new RefCusTariffTestHelper(Factory);
		var kgTariff = tariffTestHelper.CreateImportTariff(TariffConstants.KGTariff);
		tariffTestHelper.AddRate(kgTariff, PrimaryPreferenceCodeList.Codes.N, "2.5 * [KGM]", Core.Constants.CountryCodes.UnitedStates);
	}

	protected override string MessageType => JobMessageTypeList.Codes.Export;
}

class JobComInvoiceLineValidationForTesting(JobComInvoiceLine parent) : JobComInvoiceLineValidation(parent)
{
	public INotificationType ValidRatesNotificationSeverityExposed => ValidRatesNotificationSeverity;

	public IEnumerable<IZZRateSelectionCriteria> RateSelectionCriteriaListsExposed => RateSelectionCriteriaLists;
}

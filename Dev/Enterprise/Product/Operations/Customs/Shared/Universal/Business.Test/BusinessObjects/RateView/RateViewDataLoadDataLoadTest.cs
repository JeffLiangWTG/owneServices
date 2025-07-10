using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.Universal.RateViewDataLoad;
using static Enterprise.Integration.Customs;
using CustomsUniversal = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RateViewDataLoad))]
	class RateViewDataLoadDataLoadTest : DataLoadTestCase<RateViewDataLoad>
	{
		protected override RateViewDataLoad GetNewDataLoader() => new RateViewDataLoad();

		[ExpectException(typeof(FileNotFoundException))]
		public void TestValidationOfFile()
		{
			loader.ImportRateData("non-existant file");
		}

		#region Test ValidationOfHeader

		public void TestValidationOfHeader_WrongHeader()
		{
			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile, "Wrong Header Info");
				loader.ImportRateData(tempFile.Filename);
				AssertValidationResult(loader, 1, 0, 0, 0, 0, 4, false, new[] { "Unknown column heading : Wrong Header Info" });
			}
		}

		public void TestValidationOfHeader_NoMandatoryFields()
		{
			foreach (var missingMandatoryField in FieldNames.MandatoryFields)
			{
				using (var tempFile = TempFile.New())
				{
					var mandatoryFields = new List<string>();
					mandatoryFields.AddRange(FieldNames.MandatoryFields);
					mandatoryFields.Remove(missingMandatoryField);
					PopulateTestFile(tempFile, string.Join(",", mandatoryFields));
					var newLoader = new RateViewDataLoad();
					newLoader.ImportRateData(tempFile.Filename);
					AssertValidationResult(newLoader, 1, 0, 0, 0, 0, 4, false, new[] { $"Does not contain mandatory column heading(s): {missingMandatoryField}." });
				}
			}
		}

		public void TestValidationOfHeader_NoMandatoryFields_MissingMoreThanOneFields()
		{
			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile, "RateCode, Preference, TradeGroup, OrderNumber");
				loader.ImportRateData(tempFile.Filename);
				AssertValidationResult(loader, 1, 0, 0, 0, 0, 4, false, new[] { "Does not contain mandatory column heading(s): Version,TariffType,TariffCode,TariffStartDate." });
			}
		}

		#endregion

		#region Test invalid fields when DataLoad

		public void TestValidationOfStringField_ExceedMaxLength()
		{
			var dateToday = ZDateTime.Today.Date;
			var endDate = dateToday.AddDays(1);
			SetupUniversalReferenceData(dateToday, endDate);
			AssertInvalidValueValidation(rateDataLoadFields, "1234567, HSN, 123, 20210420, RC1, PRE, EU, O123, 20210420, 20210425, 0.5*VFD, 0.5, 0.5, KG", GetExceedMaxLengthError(FieldNames.Version, "1234567", 6));
			AssertInvalidValueValidation(rateDataLoadFields, "123456, HSN123, 123, 20210420, RC1, PRE, EU, O123, 20210420, 20210425, 0.5*VFD, 0.5, 0.5, KG", GetExceedMaxLengthError(FieldNames.TariffType, "HSN123", 5));
			AssertInvalidValueValidation(rateDataLoadFields, "123456, HSN, 0123456789012345678901234567890123456, 20210420, RC1, PRE, EU, O123, 20210420, 20210425, 0.5*VFD, 0.5, 0.5, KG", GetExceedMaxLengthError(FieldNames.TariffCode, "0123456789012345678901234567890123456", 35));
			AssertInvalidValueValidation(rateDataLoadFields, "123456, HSN, 123, 20210420, RC1234, PRE, EU, O123, 20210420, 20210425, 0.5*VFD, 0.5, 0.5, KG", GetExceedMaxLengthError(FieldNames.RateCode, "RC1234", 5));
			AssertInvalidValueValidation(rateDataLoadFields, "123456, HSN, 123, 20210420, RC1, PR123456789, EU, O123, 20210420, 20210425, 0.5*VFD, 0.5, 0.5, KG", GetExceedMaxLengthError(FieldNames.Preference, "PR123456789", 10));
			AssertInvalidValueValidation(rateDataLoadFields, "123456, HSN, 123, 20210420, RC1, PRE, EU123456789, O123, 20210420, 20210425, 0.5*VFD, 0.5, 0.5, KG", GetExceedMaxLengthError(FieldNames.TradeGroup, "EU123456789", 10));
			AssertInvalidValueValidation(rateDataLoadFields, "123456, HSN, 123, 20210420, RC1, PRE, EU, Order12345678901, 20210420, 20210425, 0.5*VFD, 0.5, 0.5, KG", GetExceedMaxLengthError(FieldNames.OrderNumber, "Order12345678901", 15));
			AssertInvalidValueValidation(rateDataLoadFields, "123456, HSN, 123, 20210420, RC1, PRE, EU, O123, 20210420, 20210425, 0.5*VFD, 0.5, 0.5, KG123456789", GetExceedMaxLengthError(FieldNames.SpecificRateUOM, "KG123456789", 10));
			var rateFormula = new string('X', 501);
			AssertInvalidValueValidation(rateDataLoadFields, $"123456, HSN, 123, 20210420, RC1, PRE, EU, O123, 20210420, 20210425, {rateFormula}, 0.5, 0.5, KG", GetExceedMaxLengthError(FieldNames.RateFormula, rateFormula, 500));
		}

		public void TestValidationOfDecimalField_TooLarge()
		{
			var dateToday = ZDateTime.Today.Date;
			var endDate = dateToday.AddDays(1);
			SetupUniversalReferenceData(dateToday, endDate);
			AssertInvalidValueValidation(rateDataLoadFields, "123456, HSN, 123, 20210420, RC1, PRE, EU, O123, 20210420, 20210425, 0.5*VFD, 1234567899.555, 0.5, KG", GetTooLargeDecimalError(FieldNames.ADValoremRate, "1234567899.555"));
			AssertInvalidValueValidation(rateDataLoadFields, "123456, HSN, 123, 20210420, RC1, PRE, EU, O123, 20210420, 20210425, 0.5*VFD, 0.5, 1234567899.555, KG", GetTooLargeDecimalError(FieldNames.SpecificRate, "1234567899.555"));
		}

		public void TestValidationOfField_EmptyValue()
		{
			var dateToday = ZDateTime.Today.Date;
			var endDate = dateToday.AddDays(1);
			SetupUniversalReferenceData(dateToday, endDate);
			AssertInvalidValueValidation(rateDataLoadFields, $", HSN, 123, {dateToday:yyyyMMdd}, RC1, PRE, EU, O123, {dateToday:yyyyMMdd}, {endDate:yyyyMMdd}, 0.5*VFD, 0.5, 0.5, KG", MessageGenerator.GetEmptyValueError(FieldNames.Version));
			AssertInvalidValueValidation(rateDataLoadFields, $"123456, , 123, {dateToday:yyyyMMdd}, RC1, PRE, EU, O123, {dateToday:yyyyMMdd}, {endDate:yyyyMMdd}, 0.5*VFD, 0.5, 0.5, KG", MessageGenerator.GetEmptyValueError(FieldNames.TariffType));
			AssertInvalidValueValidation(rateDataLoadFields, $"123456, HSN, , {dateToday:yyyyMMdd}, RC1, PRE, EU, O123, {dateToday:yyyyMMdd}, {endDate:yyyyMMdd}, 0.5*VFD, 0.5, 0.5, KG", MessageGenerator.GetEmptyValueError(FieldNames.TariffCode));
			AssertInvalidValueValidation(rateDataLoadFields, $"123456, HSN, 123, , RC1, PRE, EU, O123, {dateToday:yyyyMMdd}, {endDate:yyyyMMdd}, 0.5*VFD, 0.5, 0.5, KG", MessageGenerator.GetEmptyValueError(FieldNames.TariffStartDate));
			AssertInvalidValueValidation(rateDataLoadFields, $"123456, HSN, 123, {dateToday:yyyyMMdd}, , PRE, EU, O123, {dateToday:yyyyMMdd}, {endDate:yyyyMMdd}, 0.5*VFD, 0.5, 0.5, KG", MessageGenerator.GetEmptyValueError(FieldNames.RateCode));
			AssertInvalidValueValidation(rateDataLoadFields, $"123456, HSN, 123, {dateToday:yyyyMMdd}, RC1, , EU, O123, {dateToday:yyyyMMdd}, {endDate:yyyyMMdd}, 0.5*VFD, 0.5, 0.5, KG", MessageGenerator.GetEmptyValueError(FieldNames.Preference));
			AssertInvalidValueValidation(rateDataLoadFields, $"123456, HSN, 123, {dateToday:yyyyMMdd}, RC1, PRE, , O123, {dateToday:yyyyMMdd}, {endDate:yyyyMMdd}, 0.5*VFD, 0.5, 0.5, KG", MessageGenerator.GetEmptyValueError(FieldNames.TradeGroup));
		}

		public void TestValidationOfTariffCode_Numeric()
		{
			var dateToday = ZDateTime.Today.Date;
			var endDate = dateToday.AddDays(1);
			SetupUniversalReferenceData(dateToday, endDate);
			AssertInvalidValueValidation(rateDataLoadFields, $"123456, HSN, SS12, {dateToday:yyyyMMdd}, RC1, PRE, EU, O123, {dateToday:yyyyMMdd}, {endDate:yyyyMMdd}, 0.5*VFD, 0.5, 0.5, KG", MessageGenerator.GetMustBeNumericError(FieldNames.TariffCode, "SS12"));
		}

		public void TestValidationOfDate_InvalidFormat()
		{
			var dateToday = ZDateTime.Today.Date;
			var endDate = dateToday.AddDays(1);
			SetupUniversalReferenceData(dateToday, endDate);
			AssertInvalidValueValidation(rateDataLoadFields, $"123456, HSN, 123, {dateToday:yyyyMMdd}, RC1, PRE, EU, O123, 2021-04-20, 20210425, 0.5*VFD, 0.5, 0.5, KG", GetInvalidDateFormatError(FieldNames.StartDate, "2021-04-20"));
			AssertInvalidValueValidation(rateDataLoadFields, $"123456, HSN, 123, {dateToday:yyyyMMdd}, RC1, PRE, EU, O123, SS, 20210425, 0.5*VFD, 0.5, 0.5, KG", GetInvalidDateFormatError(FieldNames.StartDate, "SS"));
			AssertInvalidValueValidation(rateDataLoadFields, $"123456, HSN, 123, {dateToday:yyyyMMdd}, RC1, PRE, EU, O123, 20210420, 2021-04-25, 0.5*VFD, 0.5, 0.5, KG", GetInvalidDateFormatError(FieldNames.EndDate, "2021-04-25"));
			AssertInvalidValueValidation(rateDataLoadFields, $"123456, HSN, 123, {dateToday:yyyyMMdd}, RC1, PRE, EU, O123, 20210420, SS, 0.5*VFD, 0.5, 0.5, KG", GetInvalidDateFormatError(FieldNames.EndDate, "SS"));
		}

		public void TestValidationOfDate_EndDateEarlierThanStartDate()
		{
			var dateToday = ZDateTime.Today.Date;
			var endDate = dateToday.AddDays(1);
			SetupUniversalReferenceData(dateToday, endDate);
			var rateEndDate = dateToday.AddDays(-1);
			AssertInvalidValueValidation(rateDataLoadFields, $"123456, HSN, 123, {dateToday:yyyyMMdd}, RC1, PRE, EU, O123, {dateToday:yyyyMMdd}, {rateEndDate:yyyyMMdd}, 0.5*VFD, 0.5, 0.5, KG", MessageGenerator.GetEndDateEarlierThanStartDateError(dateToday, rateEndDate));
		}

		public void TestValidationOfRateCode_NotExist()
		{
			var dateToday = ZDateTime.Today.Date;
			var endDate = dateToday.AddDays(1);
			SetupUniversalReferenceData(dateToday, endDate);
			AssertInvalidValueValidation(rateDataLoadFields, $"HS2021, HSN, 123, {dateToday:yyyyMMdd}, RC2, PRE, EU, O123, {dateToday:yyyyMMdd}, {endDate:yyyyMMdd}, 0.5*VFD, , , ", MessageGenerator.GetNotExistedError(FieldNames.RateCode, "RC2"));
		}

		public void TestValidationOfPreference_NotExist()
		{
			var dateToday = ZDateTime.Today.Date;
			var endDate = dateToday.AddDays(1);
			SetupUniversalReferenceData(dateToday, endDate);
			AssertInvalidValueValidation(rateDataLoadFields, $"HS2021, HSN, 123, {dateToday:yyyyMMdd}, RC1, PR0, EU, O123, {dateToday:yyyyMMdd}, {endDate:yyyyMMdd}, 0.5*VFD, , , ", MessageGenerator.GetNotExistedError(FieldNames.Preference, "PR0"));
		}

		public void TestValidationOfTradeGroup_NotExist()
		{
			var dateToday = ZDateTime.Today.Date;
			var endDate = dateToday.AddDays(1);
			SetupUniversalReferenceData(dateToday, endDate);
			AssertInvalidValueValidation(rateDataLoadFields, $"HS2021, HSN, 123, {dateToday:yyyyMMdd}, RC1, PRE, TT, O123, {dateToday:yyyyMMdd}, {endDate:yyyyMMdd}, 0.5*VFD, , , ", MessageGenerator.GetNotExistedError(FieldNames.TradeGroup, "TT"));
		}

		#endregion

		#region Test invalid fields when ProcessRateData

		public void TestProcessRateData_NoVersion()
		{
			var dateToday = ZDateTime.Today.Date;
			var endDate = dateToday.AddDays(1);
			SetupUniversalReferenceData(dateToday, endDate);
			AssertInvalidValueValidation(rateDataLoadFields, $"123456, HSN, 123, {dateToday:yyyyMMdd}, RC1, PRE, EU, O123, {dateToday:yyyyMMdd}, {endDate:yyyyMMdd}, 0.5*VFD, 0.5, 0.5, KG", MessageGenerator.GetNotExistedError(FieldNames.Version, "123456"));
		}

		public void TestProcessRateData_NoTariff()
		{
			var dateToday = ZDateTime.Today.Date;
			var endDate = dateToday.AddDays(1);
			SetupUniversalReferenceData(dateToday, endDate);
			CreateVersion("HS2021", "HS2021 DESC", ZDateTime.Today.Date);
			var tariffLoader = new TariffView.Loader(Factory);
			AssertEquals("PreCondition", null, tariffLoader.LoadTariffByVersion(CurrentCountry, "HSN", "123", "HS2021", ZDateTime.Today.Date));
			AssertInvalidValueValidation(rateDataLoadFields, $"HS2021, HSN, 123, {dateToday:yyyyMMdd}, RC1, PRE, EU, O123, {dateToday:yyyyMMdd}, {endDate:yyyyMMdd}, 0.5*VFD, 0.5, 0.5, KG", MessageGenerator.GetNoTariffError(CurrentCountry, "HSN", "123", "HS2021"));
		}

		public void TestProcessRateData_NoMatchedRate_MissingMandatoryFields()
		{
			var dateToday = ZDateTime.Today.Date;
			var endDate = dateToday.AddDays(1);
			SetupUniversalReferenceData(dateToday, endDate);
			CreateVersion("HS2021", "HS2021 DESC", dateToday);
			CreateTariff(CurrentCountry, "HSN", "123", dateToday, dateToday.AddDays(1), "Tariff1", "HS2021");
			var headingsWithoutAnyRateColumn = new List<string>();
			headingsWithoutAnyRateColumn.AddRange(rateDataLoadFieldsWithoutRateColumns);
			AssertInvalidValueValidation(headingsWithoutAnyRateColumn, $"HS2021, HSN, 123, {dateToday:yyyyMMdd}, RC1, PRE, EU, O123, {dateToday:yyyyMMdd}, {endDate:yyyyMMdd}", MessageGenerator.GetNoMandatoryFieldsForInsertError());
			headingsWithoutAnyRateColumn.Add(FieldNames.SpecificRate);
			AssertInvalidValueValidation(headingsWithoutAnyRateColumn, $"HS2021, HSN, 123, {dateToday:yyyyMMdd}, RC1, PRE, EU, O123, {dateToday:yyyyMMdd}, {endDate:yyyyMMdd}, 0.5", MessageGenerator.GetNoMandatoryFieldsForInsertError());
			headingsWithoutAnyRateColumn.Remove(FieldNames.SpecificRate);
			headingsWithoutAnyRateColumn.Add(FieldNames.SpecificRateUOM);
			AssertInvalidValueValidation(headingsWithoutAnyRateColumn, $"HS2021, HSN, 123, {dateToday:yyyyMMdd}, RC1, PRE, EU, O123, {dateToday:yyyyMMdd}, {endDate:yyyyMMdd}, KG", MessageGenerator.GetNoMandatoryFieldsForInsertError());
			headingsWithoutAnyRateColumn.Add(FieldNames.ADValoremRate);
			AssertInvalidValueValidation(headingsWithoutAnyRateColumn, $"HS2021, HSN, 123, {dateToday:yyyyMMdd}, RC1, PRE, EU, O123, {dateToday:yyyyMMdd}, {endDate:yyyyMMdd}, KG, 0", MessageGenerator.GetNoMandatoryFieldsForInsertError());
			AssertInvalidValueValidation(headingsWithoutAnyRateColumn, $"HS2021, HSN, 123, {dateToday:yyyyMMdd}, RC1, PRE, EU, O123, {dateToday:yyyyMMdd}, {endDate:yyyyMMdd}, KG, ss", MessageGenerator.GetNoMandatoryFieldsForInsertError());
			headingsWithoutAnyRateColumn.Add(FieldNames.RateFormula);
			AssertInvalidValueValidation(headingsWithoutAnyRateColumn, $"HS2021, HSN, 123, {dateToday:yyyyMMdd}, RC1, PRE, EU, O123, {dateToday:yyyyMMdd}, {endDate:yyyyMMdd}, KG, 0, ", MessageGenerator.GetNoMandatoryFieldsForInsertError());
		}

		public void TestProcessRateData_NoMatchedRate_StartDateNotEarlierThanFromDateOfTariff()
		{
			var dateToday = ZDateTime.Today.Date;
			var endDate = dateToday.AddDays(1);
			SetupUniversalReferenceData(dateToday, endDate);
			CreateVersion("HS2021", "HS2021 DESC", dateToday);
			CreateTariff(CurrentCountry, "HSN", "123", dateToday, dateToday.AddDays(1), "Tariff1", "HS2021");
			var startDate = dateToday.AddDays(-1);
			AssertInvalidValueValidation(rateDataLoadFields, $"HS2021, HSN, 123, {dateToday:yyyyMMdd}, RC1, PRE, EU, O123, {startDate:yyyyMMdd}, {dateToday:yyyyMMdd}, 0.5*VFD, 0.5, 0.5, KG", MessageGenerator.GetStartDateNotEarlierThanFromDateOfTariffError(startDate, dateToday));
		}

		public void TestProcessRateData_NoMatchedRate_EndDateNotLaterThanEndDateOfTariff()
		{
			var dateToday = ZDateTime.Today.Date;
			var tariffEndDate = dateToday.AddDays(1);
			SetupUniversalReferenceData(dateToday, tariffEndDate);
			CreateVersion("HS2021", "HS2021 DESC", dateToday);
			CreateTariff(CurrentCountry, "HSN", "123", dateToday, tariffEndDate, "Tariff1", "HS2021");
			var rateEndDate = tariffEndDate.AddDays(2);
			AssertInvalidValueValidation(rateDataLoadFields, $"HS2021, HSN, 123, {dateToday:yyyyMMdd}, RC1, PRE, EU, O123, {dateToday:yyyyMMdd}, {rateEndDate:yyyyMMdd}, 0.5*VFD, 0.5, 0.5, KG", MessageGenerator.GetEndDateNotLaterThanEndDateOfTariffError(rateEndDate, tariffEndDate));
		}

		public void TestProcessRateData_MatchedTariff_EndDateEarlierThanStartDate()
		{
			var dateToday = ZDateTime.Today;
			var tariffEndDate = dateToday.AddDays(1);
			SetupUniversalReferenceData(dateToday, tariffEndDate);
			CreateVersion("HS2021", "HS2021 DESC", dateToday);
			CreateTariff(CurrentCountry, "HSN", "123", dateToday, dateToday.AddDays(1), "Tariff1", "HS2021");
			var endDate = dateToday.AddDays(-1);
			AssertInvalidValueValidation(rateDataLoadFields, $"HS2021, HSN, 123, {dateToday:yyyyMMdd}, RC1, PRE, EU, O123, , {endDate:yyyyMMdd}, 0.5*VFD, 0.5, 0.5, KG", MessageGenerator.GetEndDateEarlierThanStartDateError(dateToday, endDate));
		}

		public void TestProcessRateData_NoRateAndApplicabilityByStartDate()
		{
			var dateToday = ZDateTime.Today.Date;
			var endDate = dateToday.AddDays(1);
			CreateVersion("HS2021", "HS2021 DESC", dateToday);
			var existedTariff = CreateTariff(CurrentCountry, "HSN", "123", dateToday, endDate, "Tariff1", "HS2021");
			var rate = CreateRate(existedTariff, CurrentCountry, "DTY", "RC1", "PRE", dateToday, endDate, "0.5");
			CreateCusRefApplicabilityView(rate, "EU", "O123", dateToday, endDate);
			AssertInvalidValueValidation(rateDataLoadFields, $"HS2021, HSN, 123, {dateToday:yyyyMMdd}, RC1, PRE, EU, O123, {dateToday.AddDays(1):yyyyMMdd}, {endDate:yyyyMMdd}, 0.5*VFD, 0.5, 0.5, KG", MessageGenerator.GetNoRateAndApplicabilityByStartDateError(dateToday.AddDays(1)));
		}

		public void TestProcessRateData_InvalidRateFormula()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(CustomsUniversal.RefCusCodeListTypes.Codes.CustomsUQ, "CUSUQ");
			helper.CreateNewOrGetExistingCusCodeList(CurrentCountry, CustomsUniversal.RefCusCodeListTypes.Codes.CustomsUQ, "KG", "Kilogram", new ZDateTime(2016, 1, 1), new ZDateTime(2079, 06, 06));
			helper.CreateNewOrGetExistingCusCodeList(CurrentCountry, CustomsUniversal.RefCusCodeListTypes.Codes.CustomsUQ, "NO", "Number", new ZDateTime(2016, 1, 1), new ZDateTime(2079, 06, 06));
			Factory.Save();
			var dateToday = ZDateTime.Today.Date;
			var endDate = dateToday.AddDays(1);
			CreateVersion("HS2021", "HS2021 DESC", dateToday);
			var existedTariff = CreateTariff(CurrentCountry, "HSN", "123", dateToday, endDate, "Tariff1", "HS2021");
			var rate = CreateRate(existedTariff, CurrentCountry, "DTY", "RC1", "PRE", dateToday, endDate, "0.5");
			CreateCusRefApplicabilityView(rate, "EU", "O123", dateToday, endDate);
			AssertInvalidValueValidation(rateDataLoadFields, $"HS2021, HSN, 123, {dateToday:yyyyMMdd}, RC1, PRE, EU, O123, {dateToday:yyyyMMdd}, {endDate:yyyyMMdd}, @30Z, , , ", MessageGenerator.GetInvalidRateFormulaError());
			AssertInvalidValueValidation(rateDataLoadFields, $"HS2021, HSN, 123, {dateToday:yyyyMMdd}, RC1, PRE, EU, O123, {dateToday:yyyyMMdd}, {endDate:yyyyMMdd}, #$%#$^, , , ", MessageGenerator.GetInvalidRateFormulaError());
			AssertInvalidValueValidation(rateDataLoadFields, $"HS2021, HSN, 123, {dateToday:yyyyMMdd}, RC1, PRE, EU, O123, {dateToday:yyyyMMdd}, {endDate:yyyyMMdd}, 999/(1 && 2), , , ", MessageGenerator.GetInvalidRateFormulaError());
		}

		public void TestProcessRateData_InvalidUOM()
		{
			var dateToday = ZDateTime.Today.Date;
			var endDate = dateToday.AddDays(1);
			CreateVersion("HS2021", "HS2021 DESC", dateToday);
			var existedTariff = CreateTariff(CurrentCountry, "HSN", "123", dateToday, endDate, "Tariff1", "HS2021");
			var rate = CreateRate(existedTariff, CurrentCountry, "DTY", "RC1", "PRE", dateToday, endDate, "0.5ABC");
			CreateCusRefApplicabilityView(rate, "EU", "O123", dateToday, endDate);
			AssertInvalidValueValidation(rateDataLoadFields, $"HS2021, HSN, 123, {dateToday:yyyyMMdd}, RC1, PRE, EU, O123, {dateToday:yyyyMMdd}, {endDate:yyyyMMdd}, , , 0.5, ABC", MessageGenerator.GetInvalidUOMError());
		}

		public void TestProcessRateData_InvalidRateCode()
		{
			var dateToday = ZDateTime.Today.Date;
			var tariffEndDate = dateToday.AddDays(10);
			var endDate2 = dateToday.AddDays(5);
			CreateVersion("HS2021", "HS2021 DESC", dateToday);
			var existedTariff = CreateTariff(CurrentCountry, "HSN", "123", dateToday, tariffEndDate, "Tariff1", "HS2021");
			var rate = CreateRate(existedTariff, CurrentCountry, "DTY", "RC1", "PRE", dateToday, endDate2, "0.5");
			CreateCusRefApplicabilityView(rate, "EU", "O123", dateToday, endDate2);
			var rate2 = CreateRate(existedTariff, CurrentCountry, "DTY", "RC1", "PRE", endDate2, tariffEndDate, "0.3");
			CreateCusRefApplicabilityView(rate2, "EU", "O123", endDate2, tariffEndDate);
			AssertInvalidValueValidation(rateDataLoadFields, $"HS2021, HSN, 123, {dateToday:yyyyMMdd}, RC1, PRE, EU, O123, {dateToday:yyyyMMdd}, {tariffEndDate:yyyyMMdd}, 0.5*VFD, , , ", MessageGenerator.GetRateOverlapsError());
		}

		public void TestProcessRateData_DateOverlap_WithOrderNumber()
		{
			var dateToday = ZDateTime.Today.Date;
			var endDate = dateToday.AddDays(10);
			AssertProcessRateData_Overlap(rateDataLoadFields, $"HS2021, HSN, 123, {dateToday:yyyyMMdd}, RC1, PRE, EU, O123, {dateToday:yyyyMMdd}, {endDate:yyyyMMdd}, 0.5*VFD, , , ", "O123");
		}

		public void TestProcessRateData_DateOverlap_WithEmptyOrderNumber()
		{
			var dateToday = ZDateTime.Today.Date;
			var endDate = dateToday.AddDays(10);
			AssertProcessRateData_Overlap(rateDataLoadFields, $"HS2021, HSN, 123, {dateToday:yyyyMMdd}, RC1, PRE, EU, , {dateToday:yyyyMMdd}, {endDate:yyyyMMdd}, 0.5*VFD, , , ", "");
		}

		public void TestProcessRateData_DateOverlap_WithNoOrderNumberColumn()
		{
			var dateToday = ZDateTime.Today.Date;
			var endDate = dateToday.AddDays(10);
			var filteredRateDataLoadFields = rateDataLoadFields.Where(fn => fn != FieldNames.OrderNumber).ToList();

			AssertProcessRateData_Overlap(filteredRateDataLoadFields, $"HS2021, HSN, 123, {dateToday:yyyyMMdd}, RC1, PRE, EU, {dateToday:yyyyMMdd}, {endDate:yyyyMMdd}, 0.5*VFD, , , ", "");
		}

		void AssertProcessRateData_Overlap(List<string> title, string content, string orderNumber)
		{
			var dateToday = ZDateTime.Today.Date;
			var endDate = dateToday.AddDays(10);
			CreateVersion("HS2021", "HS2021 DESC", dateToday);
			var existedTariff = CreateTariff(CurrentCountry, "HSN", "123", dateToday, endDate, "Tariff1", "HS2021");
			var rate = CreateRate(existedTariff, CurrentCountry, "DTY", "RC1", "PRE", dateToday, endDate, "0.5");
			var endDate2 = dateToday.AddDays(5);
			CreateCusRefApplicabilityView(rate, "EU", orderNumber, dateToday, endDate2);
			CreateCusRefApplicabilityView(rate, "EU", orderNumber, endDate2, endDate);
			AssertInvalidValueValidation(title, content, MessageGenerator.GetRateApplicabilityOverlapsError());
		}

		#endregion

		#region Test Process Import

		public void TestProcessRateData_CreateRate()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.LoadOrCreateNewCusRateCode(Factory, "RC1", ZGuid.Empty, cusRateType: "DTY", countryCode: CurrentCountry, isSystem: false);
			helper.LoadOrCreateNewCusRateCode(Factory, "RC2", ZGuid.Empty, cusRateType: "DTY", countryCode: CurrentCountry, isSystem: false);
			helper.LoadOrCreateNewCusRateCode(Factory, "RC3", ZGuid.Empty, cusRateType: "DTY", countryCode: CurrentCountry, isSystem: false);
			helper.LoadOrCreateNewCusRateCode(Factory, "RC4", ZGuid.Empty, cusRateType: "DTY", countryCode: CurrentCountry, isSystem: false);
			helper.LoadOrCreateNewCusRateCode(Factory, "RC5", ZGuid.Empty, cusRateType: "DTY", countryCode: CurrentCountry, isSystem: false);
			helper.CreatePreferenceView("PRE", "PRE DESC", CurrentCountry, false);
			var dateToday = ZDateTime.Today;
			var tariffEndDate = dateToday.AddDays(10);
			helper.LoadOrCreateTradeGroup(CurrentCountry, "EU", dateToday, tariffEndDate, "EU DESC", true, false);
			Factory.Save();

			CreateVersion("HS2021", "HS2021 DESC", dateToday);
			var existedTariff = CreateTariff(CurrentCountry, "HSN", "123", dateToday, tariffEndDate, "Tariff1", "HS2021");
			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile, string.Join(",", rateDataLoadFields), $"HS2021, HSN, 123, {dateToday:yyyyMMdd}, RC1, PRE, EU, O123, {dateToday:yyyyMMdd}, {tariffEndDate:yyyyMMdd}, 0.5*VFD, , , ", $"HS2021, HSN, 123, {dateToday:yyyyMMdd}, RC2, PRE, EU, O123, {dateToday:yyyyMMdd}, {tariffEndDate:yyyyMMdd}, 0.5*[KG], , , KG", $"HS2021, HSN, 123, {dateToday:yyyyMMdd}, RC3, PRE, EU, O123, {dateToday:yyyyMMdd}, , 0.5*VFD, , , ", $"HS2021, HSN, 123, {dateToday:yyyyMMdd}, RC4, PRE, EU, O123, {dateToday:yyyyMMdd}, {tariffEndDate:yyyyMMdd}, , 0.5, , ", $"HS2021, HSN, 123, {dateToday:yyyyMMdd}, RC5, PRE, EU, O123, {dateToday:yyyyMMdd}, {tariffEndDate:yyyyMMdd}, , , 0.5, KG");
				loader.ImportRateData(tempFile.Filename);
				AssertValidationResult(loader, 6, 4, 0, 4, 1, 7, true, new[] {
					("Row 2 created: " + MessageGenerator.GetCreateRateMessage("RC1", "PRE", "EU", "O123", CurrentCountry, "HSN", "123", "HS2021", dateToday)).Replace("&nbsp;", " "),
					("Row 3 created: " + MessageGenerator.GetCreateRateMessage("RC2", "PRE", "EU", "O123", CurrentCountry, "HSN", "123", "HS2021", dateToday)).Replace("&nbsp;", " "),
					("Row 4 created: " + MessageGenerator.GetCreateRateMessage("RC3", "PRE", "EU", "O123", CurrentCountry, "HSN", "123", "HS2021", dateToday)).Replace("&nbsp;", " "),
					("Row 5 created: " + MessageGenerator.GetCreateRateMessage("RC4", "PRE", "EU", "O123", CurrentCountry, "HSN", "123", "HS2021", dateToday)).Replace("&nbsp;", " "),
					("Row 6 excluded: " + MessageGenerator.GetInvalidUOMError()).Replace("&nbsp;", " ") });
				var newFactory = new BusinessObjectFactory();
				var tariff = newFactory.Load<TariffView>(existedTariff.PK);
				AssertEquals(4, tariff.Rates.Count);
				AssertRateAndApplicabilityUpdate(tariff.Rates[0], dateToday, tariffEndDate, "0.5*VFD", "RC1", "PRE", 1, "EU", "O123");
				AssertRateAndApplicabilityUpdate(tariff.Rates[1], dateToday, tariffEndDate, "0.5*[KG]", "RC2", "PRE", 1, "EU", "O123");
				AssertRateAndApplicabilityUpdate(tariff.Rates[2], dateToday, tariffEndDate, "0.5*VFD", "RC3", "PRE", 1, "EU", "O123");
				AssertRateAndApplicabilityUpdate(tariff.Rates[3], dateToday, tariffEndDate, "0.5*VFD", "RC4", "PRE", 1, "EU", "O123");
			}
		}

		public void TestProcessRateData_EmptyOrderNumber()
		{
			var dateToday = ZDateTime.Today;
			var tariffEndDate = dateToday.AddDays(10);
			AssertProcessRateDataWithContents(dateToday, tariffEndDate, string.Join(",", rateDataLoadFields)
				, $"HS2021, HSN, 123, {dateToday:yyyyMMdd}, RC1, PRE, EU, , {dateToday:yyyyMMdd}, {tariffEndDate:yyyyMMdd}, 0.5*VFD, , , "
				, $"HS2021, HSN, 123, {dateToday:yyyyMMdd}, RC2, PRE, EU, , {dateToday:yyyyMMdd}, {tariffEndDate:yyyyMMdd}, 0.5*VFD, , , ");
		}

		public void TestProcessRateData_NoOrderNumber()
		{
			var dateToday = ZDateTime.Today;
			var tariffEndDate = dateToday.AddDays(10);
			var filteredRateDataLoadFields = rateDataLoadFields.Where(fn => fn != FieldNames.OrderNumber).ToList();
			AssertProcessRateDataWithContents(dateToday, tariffEndDate, string.Join(",", filteredRateDataLoadFields)
				, $"HS2021, HSN, 123, {dateToday:yyyyMMdd}, RC1, PRE, EU, {dateToday:yyyyMMdd}, {tariffEndDate:yyyyMMdd}, 0.5*VFD, , , "
				, $"HS2021, HSN, 123, {dateToday:yyyyMMdd}, RC2, PRE, EU, {dateToday:yyyyMMdd}, {tariffEndDate:yyyyMMdd}, 0.5*VFD, , , ");
		}

		void AssertProcessRateDataWithContents(ZDateTime dateToday, ZDateTime tariffEndDate, string titleString, params string[] contentString)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.LoadOrCreateNewCusRateCode(Factory, "RC1", ZGuid.Empty, cusRateType: "DTY", countryCode: CurrentCountry, isSystem: false);
			helper.LoadOrCreateNewCusRateCode(Factory, "RC2", ZGuid.Empty, cusRateType: "DTY", countryCode: CurrentCountry, isSystem: false);
			helper.CreatePreferenceView("PRE", "PRE DESC", CurrentCountry, false);
			helper.LoadOrCreateTradeGroup(CurrentCountry, "EU", dateToday, tariffEndDate, "EU DESC", true, false);
			Factory.Save();

			CreateVersion("HS2021", "HS2021 DESC", dateToday);
			var existedTariff = CreateTariff(CurrentCountry, "HSN", "123", dateToday, tariffEndDate, "Tariff1", "HS2021");

			var rate = CreateRate(existedTariff, CurrentCountry, "DTY", "RC1", "PRE", dateToday, tariffEndDate, "0.1");
			CreateCusRefApplicabilityView(rate, "EU", "", dateToday, tariffEndDate);

			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile, titleString, contentString[0], contentString[1]);
				loader.ImportRateData(tempFile.Filename);
				AssertValidationResult(loader, 3, 1, 1, 2, 0, 4, true, new[] {
					("Row 2 updated: " + MessageGenerator.GetUpdateRateMessage("RC1", "PRE", "EU", "", CurrentCountry, "HSN", "123", "HS2021", dateToday)).Replace("&nbsp;", " "),
					("Row 3 created: " + MessageGenerator.GetCreateRateMessage("RC2", "PRE", "EU", "", CurrentCountry, "HSN", "123", "HS2021", dateToday)).Replace("&nbsp;", " ") });
				var newFactory = new BusinessObjectFactory();
				var tariff = newFactory.Load<TariffView>(existedTariff.PK);
				AssertEquals(2, tariff.Rates.Count);
				AssertRateAndApplicabilityUpdate(tariff.Rates[0], dateToday, tariffEndDate, "0.5*VFD", "RC1", "PRE", 1, "EU", "");
				AssertRateAndApplicabilityUpdate(tariff.Rates[1], dateToday, tariffEndDate, "0.5*VFD", "RC2", "PRE", 1, "EU", "");
			}
		}

		public void TestProcessRateData_UpdateRate()
		{
			var dateToday = ZDateTime.Today.Date;
			var tariffEndDate = dateToday.AddDays(10);
			CreateVersion("HS2021", "HS2021 DESC", dateToday);
			var existedTariff = CreateTariff(CurrentCountry, "HSN", "123", dateToday, tariffEndDate, "Tariff1", "HS2021");
			var rateEndDate = tariffEndDate.AddDays(-1);
			var rate1 = CreateRate(existedTariff, CurrentCountry, "DTY", "RC1", "PRE", dateToday, rateEndDate, "0.1");
			CreateCusRefApplicabilityView(rate1, "EU", "O123", dateToday, rateEndDate);
			var rate2 = CreateRate(existedTariff, CurrentCountry, "DTY", "RC2", "PRE", dateToday, rateEndDate, "0.2");
			CreateCusRefApplicabilityView(rate2, "EU", "O123", dateToday, rateEndDate);
			var rate3 = CreateRate(existedTariff, CurrentCountry, "DTY", "RC3", "PRE", dateToday, rateEndDate, "0.3");
			CreateCusRefApplicabilityView(rate3, "EU", "O123", dateToday, rateEndDate);
			var rate4 = CreateRate(existedTariff, CurrentCountry, "DTY", "RC4", "PRE", dateToday, rateEndDate, "0.4");
			CreateCusRefApplicabilityView(rate4, "EU", "O123", dateToday, rateEndDate);
			var rate5 = CreateRate(existedTariff, CurrentCountry, "DTY", "RC5", "PRE", dateToday, rateEndDate, "0.5");
			CreateCusRefApplicabilityView(rate5, "EU", "O123", dateToday, rateEndDate);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRateUOM(rate5.PK, "KG", false);
			var rate6 = CreateRate(existedTariff, CurrentCountry, "DTY", "RC6", "PRE", dateToday, rateEndDate, "0.6");
			CreateCusRefApplicabilityView(rate6, "EU", "O123", dateToday, rateEndDate);
			helper.CreateRateUOM(rate6.PK, "KG", false);
			Factory.Save();

			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile, string.Join(",", rateDataLoadFields), $"HS2021, HSN, 123, {dateToday:yyyyMMdd}, RC1, PRE, EU, O123, {dateToday:yyyyMMdd}, {tariffEndDate:yyyyMMdd}, 0.5*VFD, , , ", $"HS2021, HSN, 123, {dateToday:yyyyMMdd}, RC2, PRE, EU, O123, {dateToday:yyyyMMdd}, {tariffEndDate:yyyyMMdd}, 0.5*[KG], , ,KG ", $"HS2021, HSN, 123, {dateToday:yyyyMMdd}, RC3, PRE, EU, O123, {dateToday:yyyyMMdd}, , 0.5*VFD, , , ", $"HS2021, HSN, 123, {dateToday:yyyyMMdd}, RC4, PRE, EU, O123, {dateToday:yyyyMMdd}, {tariffEndDate:yyyyMMdd}, , 0.5, , ", $"HS2021, HSN, 123, {dateToday:yyyyMMdd}, RC5, PRE, EU, O123, {dateToday:yyyyMMdd}, {tariffEndDate:yyyyMMdd}, , , 0.5, KG", $"HS2021, HSN, 123, {dateToday:yyyyMMdd}, RC6, PRE, EU, O123, {dateToday:yyyyMMdd}, {tariffEndDate:yyyyMMdd}, 0.5*[KG], , , ");
				loader.ImportRateData(tempFile.Filename);
				AssertValidationResult(loader, 7, 0, 6, 6, 0, 8, true, new[] {
					("Row 2 updated: " + MessageGenerator.GetUpdateRateMessage("RC1", "PRE", "EU", "O123", CurrentCountry, "HSN", "123", "HS2021", dateToday)).Replace("&nbsp;", " "),
					("Row 3 updated: " + MessageGenerator.GetUpdateRateMessage("RC2", "PRE", "EU", "O123", CurrentCountry, "HSN", "123", "HS2021", dateToday)).Replace("&nbsp;", " "),
					("Row 4 updated: " + MessageGenerator.GetUpdateRateMessage("RC3", "PRE", "EU", "O123", CurrentCountry, "HSN", "123", "HS2021", dateToday)).Replace("&nbsp;", " "),
					("Row 5 updated: " + MessageGenerator.GetUpdateRateMessage("RC4", "PRE", "EU", "O123", CurrentCountry, "HSN", "123", "HS2021", dateToday)).Replace("&nbsp;", " "),
					("Row 6 updated: " + MessageGenerator.GetUpdateRateMessage("RC5", "PRE", "EU", "O123", CurrentCountry, "HSN", "123", "HS2021", dateToday)).Replace("&nbsp;", " "),
					("Row 7 updated: " + MessageGenerator.GetUpdateRateMessage("RC6", "PRE", "EU", "O123", CurrentCountry, "HSN", "123", "HS2021", dateToday)).Replace("&nbsp;", " ") });
				var newFactory = new BusinessObjectFactory();
				var tariff = newFactory.Load<TariffView>(existedTariff.PK);
				AssertEquals(6, tariff.Rates.Count);
				AssertRateAndApplicabilityUpdate(tariff.Rates[0], dateToday, tariffEndDate, "0.5*VFD", "RC1", "PRE", 1, "EU", "O123");
				AssertRateAndApplicabilityUpdate(tariff.Rates[1], dateToday, tariffEndDate, "0.5*[KG]", "RC2", "PRE", 1, "EU", "O123");
				AssertRateAndApplicabilityUpdate(tariff.Rates[2], dateToday, rateEndDate, "0.5*VFD", "RC3", "PRE", 1, "EU", "O123");
				AssertRateAndApplicabilityUpdate(tariff.Rates[3], dateToday, tariffEndDate, "0.5*VFD", "RC4", "PRE", 1, "EU", "O123");
				AssertRateAndApplicabilityUpdate(tariff.Rates[4], dateToday, tariffEndDate, "0.5*[KG]", "RC5", "PRE", 1, "EU", "O123");
				AssertRateAndApplicabilityUpdate(tariff.Rates[5], dateToday, tariffEndDate, "0.5*[KG]", "RC6", "PRE", 1, "EU", "O123");
			}
		}

		static void AssertRateAndApplicabilityUpdate(RateView rate, ZDateTime startDate, ZDateTime endDate, ZString rateFormula, ZString rateCode, ZString preference, int count, ZString tradeGroup, ZString orderNumber)
		{
			AssertEquals("ZZ2_EndDate", endDate, rate.ZZ2_EndDate);
			AssertEquals("ZZ2_RateFormula", rateFormula, rate.ZZ2_RateFormula);
			AssertEquals("ZZ2_StartDate", startDate, rate.ZZ2_StartDate);
			AssertEquals("PreferenceCode", preference, rate.PreferenceCode);
			AssertEquals("RateCode", rateCode, rate.RateCode);
			AssertEquals("Count", count, rate.RateApplicabilities.Count);
			AssertEquals("ZZT_EndDate", endDate, rate.RateApplicabilities[0].ZZT_EndDate);
			AssertEquals("ZZT_StartDate", startDate, rate.RateApplicabilities[0].ZZT_StartDate);
			AssertEquals("ZZT_OrderNumber", orderNumber, rate.RateApplicabilities[0].ZZT_OrderNumber);
			AssertEquals("TradeGroupCode", tradeGroup, rate.RateApplicabilities[0].TradeGroupCode);
		}

		#endregion

		static void PopulateTestFile(TempFile tempFile, params string[] contents)
		{
			using (StreamWriter sw = new StreamWriter(tempFile.Filename))
			{
				foreach (var content in contents)
				{
					sw.WriteLine(content);
				}

				sw.Flush();
			}
		}

		void SetupUniversalReferenceData(ZDateTime startDate, ZDateTime endDate, string rateTypeCode = "DTY", string rateCode = "RC1", string preferece = "PRE", string tradeGroup = "EU")
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.LoadOrCreateNewCusRateCode(Factory, rateCode, ZGuid.Empty, cusRateType: rateTypeCode, countryCode: CurrentCountry, isSystem: false);
			helper.CreatePreferenceView(preferece, preferece + " DESC", CurrentCountry, false);
			helper.LoadOrCreateTradeGroup(CurrentCountry, tradeGroup, startDate, endDate, "EU DESC", true, false);
			Factory.Save();
		}

		void CreateVersion(ZString versionCode, ZString versionDescription, ZDateTime effectiveDate)
		{
			if (!Factory.ExistsInDatabase(CusRefTariffVersionSchema.Constants.TableName, new ZQuery(CusRefTariffVersionSchema.CRT_Version, versionCode)))
			{
				var version = Factory.New<ICusRefTariffVersion>();
				version.CRT_Version = versionCode;
				version.CRT_Description = versionDescription;
				version.CRT_EffectiveDate = effectiveDate.Date;
				Factory.Save();
			}
		}

		TariffView CreateTariff(ZString dataGrouping, ZString type, ZString code, ZDateTime tariffStartDate, ZDateTime tariffEndDate, ZString description, ZString version)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(dataGrouping, type);
			Factory.Save();
			var tariff = helper.LoadOrCreateNewTariff(dataGrouping, tariffType.PK, code, tariffStartDate, tariffEndDate, description, isSystem: false, versionCode: version);
			Factory.Save();
			return tariff;
		}

		RateView CreateRate(TariffView tariff, ZString dataGrouping, ZString type, ZString code, ZString preferenceCode, ZDateTime startDate, ZDateTime endDate, ZString rateFormula)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, code, ZGuid.Empty, cusRateType: type, countryCode: dataGrouping, isSystem: false);
			var preference = helper.CreatePreferenceView(preferenceCode, preferenceCode + " DESC", CurrentCountry, false);
			Factory.Save();
			var rate = helper.CreateRate(tariff, rateCode.PK, startDate, endDate, rateFormula, preference.PK, dataGrouping: dataGrouping, isSystem: false);
			Factory.Save();
			return rate;
		}

		void CreateCusRefApplicabilityView(RateView rate, ZString tradeGroupCode, ZString orderNumber, ZDateTime startDate, ZDateTime endDate)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tradeGroup = helper.LoadOrCreateTradeGroup(CurrentCountry, tradeGroupCode, startDate, endDate, tradeGroupCode + " DESC", true, false);
			helper.CreateCusApplicability(rate, tradeGroup, startDate, endDate, "", orderNumber, false);
			Factory.Save();
		}

		static void AssertValidationResult(RateViewDataLoad loader, int recordsToImportPlusHeader, int recsCreated, int recsUpdated, int recsToUpdate, int recsExcluded, int logCount, bool fileHeaderIsValid, string[] messages)
		{
			CombineAssertions("AssertValidationResult", () =>
			{
				AssertEquals("RunCountersRecordsToImportPlusHeader", recordsToImportPlusHeader, loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals("RunCounters.RecsCreated", recsCreated, loader.RunCounters.RecsCreated);
				AssertEquals("RunCounters.RecsUpdated", recsUpdated, loader.RunCounters.RecsUpdated);
				AssertEquals("RunCounters.RecsToUpdate", recsToUpdate, loader.RunCounters.RecsToUpdate);
				AssertEquals("RunCounters.RecsExcluded", recsExcluded, loader.RunCounters.RecsExcluded);
				AssertEquals("Log.Count", logCount, loader.Log.Count);
				AssertEquals("FileHeaderIsValid", fileHeaderIsValid, loader.FileHeaderIsValid);
				AssertContains("Log.Message", $"Rates to Import = {recordsToImportPlusHeader - 1}", loader.Log[0]);

				foreach (var message in messages)
				{
					AssertEquals("Has process Message", true, loader.Log.Contains(message));
				}
			});
		}

		void AssertInvalidValueValidation(IEnumerable<string> headings, string lineValue, string errorMessage)
		{
			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile, string.Join(",", headings), lineValue);
				var newLoader = new RateViewDataLoad();
				newLoader.ImportRateData(tempFile.Filename);
				AssertValidationResult(newLoader, 2, 0, 0, 0, 1, 3, true, new[] { "Row 2 excluded: " + errorMessage });
			}
		}

		static ZString GetExceedMaxLengthError(ZString fieldName, ZString fieldValue, ZInt maxLength) => $"Specified argument was out of the range of valid values.\r\nParameter name: Value of {fieldName} exceeds the max length({maxLength}): {fieldValue}";

		static ZString GetTooLargeDecimalError(ZString fieldName, ZString fieldValue) => $"Specified argument was out of the range of valid values.\r\nParameter name: Value of {fieldValue} is too large to store in {fieldName}";

		static ZString GetInvalidDateFormatError(ZString fieldName, ZString fieldValue) => $"Specified argument was out of the range of valid values.\r\nParameter name: Invalid Date format for {fieldValue}, cannot save to {fieldName}. The expected format is 'yyyyMMdd'.";

		readonly List<string> rateDataLoadFields = new List<string>()
		{
			FieldNames.Version,
			FieldNames.TariffType,
			FieldNames.TariffCode,
			FieldNames.TariffStartDate,
			FieldNames.RateCode,
			FieldNames.Preference,
			FieldNames.TradeGroup,
			FieldNames.OrderNumber,
			FieldNames.StartDate,
			FieldNames.EndDate,
			FieldNames.RateFormula,
			FieldNames.ADValoremRate,
			FieldNames.SpecificRate,
			FieldNames.SpecificRateUOM
		};

		readonly List<string> rateDataLoadFieldsWithoutRateColumns = new List<string>()
		{
			FieldNames.Version,
			FieldNames.TariffType,
			FieldNames.TariffCode,
			FieldNames.TariffStartDate,
			FieldNames.RateCode,
			FieldNames.Preference,
			FieldNames.TradeGroup,
			FieldNames.OrderNumber,
			FieldNames.StartDate,
			FieldNames.EndDate
		};

		protected override void SetUp()
		{
			base.SetUp();
			loader = new RateViewDataLoad();
		}

		RateViewDataLoad loader;

		static ZString CurrentCountry => GlbCompany.CurrentCompany.Country.Code;
	}
}

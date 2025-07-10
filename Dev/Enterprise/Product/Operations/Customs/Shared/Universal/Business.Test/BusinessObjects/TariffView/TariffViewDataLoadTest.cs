using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.Universal.TariffViewDataLoad;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(TariffViewDataLoad))]
	internal class TariffViewDataLoadTest : DataLoadTestCase<TariffViewDataLoad>
	{
		protected override TariffViewDataLoad GetNewDataLoader() => new TariffViewDataLoad();

		[ExpectException(typeof(FileNotFoundException))]
		public void TestValidationOfFile()
		{
			loader.ImportTariffData("non-existant file");
		}

		#region Test ValidationOfHeader

		public void TestValidationOfHeader_WrongHeader()
		{
			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile, "Wrong Header Info");
				loader.ImportTariffData(tempFile.Filename);
				AssertValidationResult(loader, 1, 0, 0, 0, 0, 4, false, "Unknown column heading : Wrong Header Info");
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
					var newLoader = new TariffViewDataLoad();
					newLoader.ImportTariffData(tempFile.Filename);
					AssertValidationResult(newLoader, 1, 0, 0, 0, 0, 4, false, MessageGenerator.GetNoMandatoryColumnsError(new HashSet<string> { missingMandatoryField }));
				}
			}
		}

		public void TestValidationOfHeader_NoMandatoryFields_MissingMoreThanOneFields()
		{
			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile, "Description");
				loader.ImportTariffData(tempFile.Filename);
				AssertValidationResult(loader, 1, 0, 0, 0, 0, 4, false, MessageGenerator.GetNoMandatoryColumnsError(FieldNames.MandatoryFields.ToHashSet()));
			}
		}

		public void TestValidationOfHeader_HasMandatoryFields()
		{
			using (var tempFile = TempFile.New())
			{
				var mandatoryFields = new List<string>();
				mandatoryFields.AddRange(FieldNames.MandatoryFields);
				PopulateTestFile(tempFile, string.Join(",", mandatoryFields));
				loader.ImportTariffData(tempFile.Filename);
				AssertValidationResult(loader, 1, 0, 0, 0, 0, 2, true);
			}
		}

		public void TestValidationOfHeader_NoMandatoryFieldsForInsert_UOM1()
		{
			CreateVersion("123456", "123456 DESC", ZDateTime.Today);
			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile, "Version, TariffType, TariffCode, CountryCode, StartDate, Description, UOM2", $"123456, HSN, 123, NA, {ZDateTime.Today:yyyyMMdd},Tariff DESC, uom2");
				loader.ImportTariffData(tempFile.Filename);
				AssertValidationResult(loader, 2, 0, 0, 0, 1, 3, true, "Row 2 excluded: " + MessageGenerator.GetNoMandatoryColumnsError(new HashSet<string> { "UOM1" }));
			}
		}

		public void TestValidationOfHeader_NoMandatoryFieldsForInsert_Description()
		{
			CreateVersion("123456", "123456 DESC", ZDateTime.Today);
			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile, "Version, TariffType, TariffCode, CountryCode, StartDate, UOM1", $"123456, HSN, 123, NA, {ZDateTime.Today:yyyyMMdd}, KG");
				loader.ImportTariffData(tempFile.Filename);
				AssertValidationResult(loader, 2, 0, 0, 0, 1, 3, true, "Row 2 excluded: " + MessageGenerator.GetNoMandatoryColumnsError(new HashSet<string> { "Description" }));
			}
		}

		public void TestValidationOfHeader_NoMandatoryFieldsForInsert_MissingMoreThanOneFields()
		{
			CreateVersion("123456", "123456 DESC", ZDateTime.Today);
			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile, "Version, TariffType, TariffCode, CountryCode, StartDate, UOM2", $"123456, HSN, 123, NA, {ZDateTime.Today:yyyyMMdd}, uom2");
				loader.ImportTariffData(tempFile.Filename);
				AssertValidationResult(loader, 2, 0, 0, 0, 1, 3, true, "Row 2 excluded: " + MessageGenerator.GetNoMandatoryColumnsError(new HashSet<string> { "Description", "UOM1" }));
			}
		}

		#endregion

		#region Test field values Exceed MaxLength

		public void TestValidationOfContent_ExceedMaxLength_Version()
		{
			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile, "Version, TariffType, TariffCode, CountryCode, StartDate", $"1234567, HSN, 123, NA, {ZDateTime.Today:yyyyMMdd}");
				loader.ImportTariffData(tempFile.Filename);
				AssertValidationResult(loader, 2, 0, 0, 0, 1, 3, true, "Row 2 excluded: " + GetExceedMaxLengthError(FieldNames.Version, "1234567", 6));
			}
		}

		public void TestValidationOfContent_ExceedMaxLength_TariffCode()
		{
			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile, "Version, TariffType, TariffCode, CountryCode, StartDate", $"123456, HSN, 012345678901234567890123456789012345, NA, {ZDateTime.Today:yyyyMMdd}");
				loader.ImportTariffData(tempFile.Filename);
				AssertValidationResult(loader, 2, 0, 0, 0, 1, 3, true, "Row 2 excluded: " + GetExceedMaxLengthError(FieldNames.TariffCode, "012345678901234567890123456789012345", 35));
			}
		}

		public void TestValidationOfContent_ExceedMaxLength_TaxOrFeeCode()
		{
			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile, "Version, TariffType, TariffCode, CountryCode, StartDate, TaxOrFeeCode", $"123456, HSN, 123, NA, {ZDateTime.Today:yyyyMMdd}, TTT1");
				loader.ImportTariffData(tempFile.Filename);
				AssertValidationResult(loader, 2, 0, 0, 0, 1, 3, true, "Row 2 excluded: " + GetExceedMaxLengthError(FieldNames.TaxOrFeeCode, "TTT1", 3));
			}
		}

		public void TestValidationOfContent_ExceedMaxLength_CountryCode()
		{
			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile, "Version, TariffType, TariffCode, CountryCode, StartDate", $"123456, HSN, 123, CN1, {ZDateTime.Today:yyyyMMdd}");
				loader.ImportTariffData(tempFile.Filename);
				AssertValidationResult(loader, 2, 0, 0, 0, 1, 3, true, "Row 2 excluded: " + GetExceedMaxLengthError(FieldNames.CountryCode, "CN1", 2));
			}
		}

		public void TestValidationOfContent_ExceedMaxLength_UOM1()
		{
			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile, "Version, TariffType, TariffCode, CountryCode, StartDate, UOM1", $"123456, HSN, 123, NA, {ZDateTime.Today:yyyyMMdd}, 01234567890");
				loader.ImportTariffData(tempFile.Filename);
				AssertValidationResult(loader, 2, 0, 0, 0, 1, 3, true, "Row 2 excluded: " + GetExceedMaxLengthError(FieldNames.UOM1, "01234567890", 10));
			}
		}

		public void TestValidationOfContent_ExceedMaxLength_UOM2()
		{
			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile, "Version, TariffType, TariffCode, CountryCode, StartDate, UOM2", $"123456, HSN, 123, NA, {ZDateTime.Today:yyyyMMdd}, 01234567890");
				loader.ImportTariffData(tempFile.Filename);
				AssertValidationResult(loader, 2, 0, 0, 0, 1, 3, true, "Row 2 excluded: " + GetExceedMaxLengthError(FieldNames.UOM2, "01234567890", 10));
			}
		}

		public void TestValidationOfContent_ExceedMaxLength_UOM3()
		{
			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile, "Version, TariffType, TariffCode, CountryCode, StartDate, UOM3", $"123456, HSN, 123, NA, {ZDateTime.Today:yyyyMMdd}, 01234567890");
				loader.ImportTariffData(tempFile.Filename);
				AssertValidationResult(loader, 2, 0, 0, 0, 1, 3, true, "Row 2 excluded: " + GetExceedMaxLengthError(FieldNames.UOM3, "01234567890", 10));
			}
		}

		public void TestValidationOfContent_ExceedMaxLength_Description()
		{
			using (var tempFile = TempFile.New())
			{
				var description = new string('X', 4001);
				PopulateTestFile(tempFile, "Version, TariffType, TariffCode, CountryCode, StartDate, Description", $"123456, HSN, 123, NA, {ZDateTime.Today:yyyyMMdd}, {description}");
				loader.ImportTariffData(tempFile.Filename);
				AssertValidationResult(loader, 2, 0, 0, 0, 1, 3, true, $"Row 2 excluded: " + GetExceedMaxLengthError(FieldNames.Description, description, 4000));
			}
		}

		#endregion

		#region Test invalid fields

		public void TestValidationOfContent_InvalidDate_StartDate()
		{
			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile, "Version, TariffType, TariffCode, CountryCode, StartDate", "123456, HSN, 123, NA, 10/12/21");
				loader.ImportTariffData(tempFile.Filename);
				AssertValidationResult(loader, 2, 0, 0, 0, 1, 3, true, @"Row 2 excluded: " + GetInvalidDateFormatError(FieldNames.StartDate, "10/12/21"));
			}
		}

		public void TestValidationOfContent_InvalidDate_EndDate()
		{
			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile, "Version, TariffType, TariffCode, CountryCode, StartDate, EndDate", $"123456, HSN, 123, NA, {ZDateTime.Today:yyyyMMdd}, 10/12/21");
				loader.ImportTariffData(tempFile.Filename);
				AssertValidationResult(loader, 2, 0, 0, 0, 1, 3, true, @"Row 2 excluded: " + GetInvalidDateFormatError(FieldNames.EndDate, "10/12/21"));
			}
		}

		public void TestValidationOfContent_InvalidDate_EndDateEarlierThanStartDate()
		{
			using (var tempFile = TempFile.New())
			{
				var startDate = new ZDateTime(2021, 3, 2);
				var endDate = new ZDateTime(2021, 3, 1);
				PopulateTestFile(tempFile, "Version, TariffType, TariffCode, CountryCode, StartDate, EndDate, UOM1", $"123456, HSN, 123, NA, {startDate:yyyyMMdd}, {endDate:yyyyMMdd}, KG");
				loader.ImportTariffData(tempFile.Filename);
				AssertValidationResult(loader, 2, 0, 0, 0, 1, 3, true, "Row 2 excluded: " + MessageGenerator.GetEndDateError(endDate, "start date", startDate));
			}
		}

		public void TestValidationOfContent_InvalidTariffType()
		{
			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile, "Version, TariffType, TariffCode, CountryCode, StartDate, UOM1", $"000001, OTH, 123, NA, {ZDateTime.Today:yyyyMMdd}, KG");
				loader.ImportTariffData(tempFile.Filename);
				AssertValidationResult(loader, 2, 0, 0, 0, 1, 3, true, "Row 2 excluded: " + MessageGenerator.GetInvalidTariffTypeError("OTH"));
			}
		}

		public void TestValidationOfContent_EmptyVersion()
		{
			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile, "Version, TariffType, TariffCode, CountryCode, StartDate, UOM1", $", HSN, 123, NA, {ZDateTime.Today:yyyyMMdd}, KG");
				loader.ImportTariffData(tempFile.Filename);
				AssertValidationResult(loader, 2, 0, 0, 0, 1, 3, true, "Row 2 excluded: " + MessageGenerator.GetEmptyVersionError());
			}
		}

		public void TestValidationOfContent_InvalidCountryCode()
		{
			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile, "Version, TariffType, TariffCode, CountryCode, StartDate, UOM1",
					$"000001, HSN, 123, , {ZDateTime.Today:yyyyMMdd}, KG",
					$"000001, HSN, 123, N, {ZDateTime.Today:yyyyMMdd}, KG");
				loader.ImportTariffData(tempFile.Filename);
				AssertValidationResult(loader, 3, 0, 0, 0, 2, 4, true,
					"Row 2 excluded: " + MessageGenerator.GetInvalidCountryCodeError(""),
					"Row 3 excluded: " + MessageGenerator.GetInvalidCountryCodeError("N"));
			}
		}

		public void TestValidationOfContent_InvalidTariffCode()
		{
			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile, "Version, TariffType, TariffCode, CountryCode, StartDate, UOM1",
					$"000001, HSN, TTT, NA, {ZDateTime.Today:yyyyMMdd}, KG",
					$"000001, HSN, , NA, {ZDateTime.Today:yyyyMMdd}, KG");
				loader.ImportTariffData(tempFile.Filename);
				AssertValidationResult(loader, 3, 0, 0, 0, 2, 4, true,
					"Row 2 excluded: " + MessageGenerator.GetInvalidTariffCodeError("TTT"),
					"Row 3 excluded: " + MessageGenerator.GetInvalidTariffCodeError(""));
			}
		}

		public void TestValidationOfContent_DuplicateUOMs()
		{
			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile, "Version, TariffType, TariffCode, CountryCode, StartDate, Description, TaxOrFeeCode, UOM1, UOM2, UOM3",
					$"000001, HSN, 123, NA, {ZDateTime.Today:yyyyMMdd}, Tariff DESC, DV1, uom, uom, uom3",
					$"000001, HSN, 123, NA, {ZDateTime.Today:yyyyMMdd}, Tariff DESC, DV1, uom1, uom, uom",
					$"000001, HSN, 123, NA, {ZDateTime.Today:yyyyMMdd}, Tariff DESC, DV1, uom, uom2, uom",
					$"000001, HSN, 123, NA, {ZDateTime.Today:yyyyMMdd}, Tariff DESC, DV1, uom, uom, uom");
				loader.ImportTariffData(tempFile.Filename);
				AssertValidationResult(loader, 5, 0, 0, 0, 4, 6, true,
					"Row 2 excluded: " + MessageGenerator.GetDuplicateUOMsError(),
					"Row 3 excluded: " + MessageGenerator.GetDuplicateUOMsError(),
					"Row 4 excluded: " + MessageGenerator.GetDuplicateUOMsError(),
					"Row 5 excluded: " + MessageGenerator.GetDuplicateUOMsError());
			}
		}

		public void TestValidationOfContent_DuplicateUOMs_TwoUOMColumns_UOM1UOM2()
		{
			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile, "Version, TariffType, TariffCode, CountryCode, StartDate, Description, TaxOrFeeCode, UOM1, UOM2",
					$"000001, HSN, 123, NA, {ZDateTime.Today:yyyyMMdd}, Tariff DESC, DV1, uom, uom");
				loader.ImportTariffData(tempFile.Filename);
				AssertValidationResult(loader, 2, 0, 0, 0, 1, 3, true, "Row 2 excluded: " + MessageGenerator.GetDuplicateUOMsError());
			}
		}

		public void TestValidationOfContent_DuplicateUOMs_TwoUOMColumns_UOM1UOM3()
		{
			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile, "Version, TariffType, TariffCode, CountryCode, StartDate, Description, TaxOrFeeCode, UOM1, UOM3", $"000001, HSN, 123, NA, {ZDateTime.Today:yyyyMMdd}, Tariff DESC, DV1, uom, uom");
				loader.ImportTariffData(tempFile.Filename);
				AssertValidationResult(loader, 2, 0, 0, 0, 1, 3, true, "Row 2 excluded: " + MessageGenerator.GetDuplicateUOMsError());
			}
		}

		public void TestValidationOfContent_DuplicateUOMs_TwoUOMColumns_UOM2UOM3()
		{
			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile, "Version, TariffType, TariffCode, CountryCode, StartDate, Description, TaxOrFeeCode, UOM2, UOM3", $"000001, HSN, 123, NA, {ZDateTime.Today:yyyyMMdd}, Tariff DESC, DV1, uom, uom");
				loader.ImportTariffData(tempFile.Filename);
				AssertValidationResult(loader, 2, 0, 0, 0, 1, 3, true, "Row 2 excluded: " + MessageGenerator.GetDuplicateUOMsError());
			}
		}

		#endregion

		#region Test Process Import

		public void TestImportTariffCSV_NoVersion()
		{
			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile, "Version, TariffType, TariffCode, CountryCode, StartDate, UOM1", $"123456, HSN, 123, NA, {ZDateTime.Today:yyyyMMdd}, uom1");
				loader.ImportTariffData(tempFile.Filename);
				AssertValidationResult(loader, 2, 0, 0, 0, 1, 3, true, "Row 2 excluded: " + MessageGenerator.GetNotExistedError(FieldNames.Version, "123456"));
			}
		}

		public void TestImportTariffCSV_InsertNewTariff()
		{
			var startDate = ZDateTime.Today;
			var endDate = startDate.AddMonths(1);
			CreateVersion("000001", "000001 DESC", startDate.Date);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusTaxOrFeeType("VAT");
			helper.CreateTaxOrFee("DV1", 0.5m, "NA", 0.1m, 10m, "VAT");
			Factory.Save();

			var tariffLoader = new TariffView.Loader(Factory);
			AssertEquals(null, tariffLoader.LoadTariffByVersion("NA", "HSN", "123", "000001", startDate));

			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile,
					"Version, TariffType, TariffCode, CountryCode, Description, StartDate, EndDate, TaxOrFeeCode, UOM1, UOM2, UOM3",
					$"000001, HSN, 123, NA, Tariff DESC, {startDate:yyyyMMdd}, {endDate:yyyyMMdd}, DV1, uom1, uom2, uom3",
					$"000001, HSN, 223, NA, Tariff2 DESC, {startDate:yyyyMMdd}, , DV1, uom1, uom2, uom3");
				loader.ImportTariffData(tempFile.Filename);

				AssertValidationResult(loader, 3, 2, 0, 2, 0, 4, true,
					"Row 2 created: " + MessageGenerator.GetCreateTariffMessage("NA", "HSN", "123", "000001", startDate),
					"Row 3 created: " + MessageGenerator.GetCreateTariffMessage("NA", "HSN", "223", "000001", startDate));

				var tariff = tariffLoader.LoadTariffByVersion("NA", "HSN", "123", "000001", startDate);
				AssertImportedTariff(tariff, "Tariff DESC", startDate, endDate, "DV1");
				AssertTariffUOM(tariff.PK, UOMTypeList.Codes.CU1, "uom1");
				AssertTariffUOM(tariff.PK, UOMTypeList.Codes.CU2, "uom2");
				AssertTariffUOM(tariff.PK, UOMTypeList.Codes.CU3, "uom3");

				var tariff2 = tariffLoader.LoadTariffByVersion("NA", "HSN", "223", "000001", startDate);
				AssertImportedTariff(tariff2, "Tariff2 DESC", startDate, new ZDateTime(9999, 12, 31), "DV1");
			}
		}

		public void TestImportTariffCSV_UpdateExistedTariff()
		{
			var effectiveDate = ZDateTime.Today;
			var startDate = effectiveDate.AddDays(1);
			var endDate = startDate.AddMonths(1);
			var existedTariff = CreateExistedTariff("000001", effectiveDate, startDate, "HSN", "123", "NA");

			var tariffLoader = new TariffView.Loader(Factory);
			var tariff = tariffLoader.LoadTariffByVersion("NA", "HSN", "123", "000001", startDate);
			AssertEquals(existedTariff.PK, tariff.PK);

			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile,
					"Version, TariffType, TariffCode, CountryCode, Description, StartDate, EndDate, TaxOrFeeCode, UOM1, UOM2, UOM3",
					$"000001, HSN, 123, NA, Tariff DESC, {startDate:yyyyMMdd}, {endDate:yyyyMMdd}, DV1, uom1, uom2, uom3");
				loader.ImportTariffData(tempFile.Filename);

				AssertValidationResult(loader, 2, 0, 1, 1, 0, 3, true, "Row 2 updated: " + MessageGenerator.GetUpdateTariffMessage("NA", "HSN", "123", "000001", startDate));

				tariff.Reload();
				AssertImportedTariff(tariff, "Tariff DESC", startDate, endDate, "DV1");
				AssertTariffUOM(tariff.PK, UOMTypeList.Codes.CU1, "uom1");
				AssertTariffUOM(tariff.PK, UOMTypeList.Codes.CU2, "uom2");
				AssertTariffUOM(tariff.PK, UOMTypeList.Codes.CU3, "uom3");
			}
		}

		public void TestImportTariffCSV_MultiLinesImport()
		{
			var effectiveDate = new ZDate(2021, 3, 11);
			var startDate = effectiveDate.AddDays(1);
			var endDate = effectiveDate.AddMonths(1);
			var existedTariff = CreateExistedTariff("000001", effectiveDate, startDate, "HSN", "123", "NA");
			CreateExistedTariff("000002", effectiveDate.AddDays(1), startDate, "HSN", "124", "NA");

			var tariffLoader = new TariffView.Loader(Factory);
			var tariff1 = tariffLoader.LoadTariffByVersion("NA", "HSN", "123", "000001", startDate);
			AssertEquals(existedTariff.PK, tariff1.PK);

			var invalidDate = effectiveDate.AddDays(-1);
			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile,
					"Version, TariffType, TariffCode, CountryCode, Description, StartDate, EndDate, TaxOrFeeCode, UOM1, UOM2, UOM3",
					$"000001, HSN, 123, NA, Tariff1 DESC, {startDate:yyyyMMdd}, {endDate:yyyyMMdd}, DV1, uom1, uom2, uom3",
					$"000001, HSN, 223, NA, Tariff2 DESC, {startDate:yyyyMMdd}, , , uom1, , ",
					$"000001, XXX, 323, NA, Tariff3 DESC, {endDate:yyyyMMdd}, {startDate:yyyyMMdd}, XXX, , , ",
					$"000001, , 423, ER, Tariff4 DESC, {startDate:yyyyMMdd}, , DV2, uom1, uom2, uom3",
					$"000003, HSN, 323, NA, Tariff3 DESC, {startDate:yyyyMMdd}, {endDate:yyyyMMdd}, DV1, uom1, , ",
					$"000001, HSN, 423, NA, Tariff3 DESC, {invalidDate.AddDays(-1):yyyyMMdd}, {invalidDate:yyyyMMdd}, XXX, uom1, , ",
					$"000001, HSN, 123, CN, , {invalidDate.AddDays(-1):yyyyMMdd}, {invalidDate:yyyyMMdd}, XXX, , , ",
					$"000002, HSN, 124, NA, 124 DESC, {startDate:yyyyMMdd}, {endDate:yyyyMMdd}, XXX, uom1, uom2, uom3");
				loader.ImportTariffData(tempFile.Filename);

				AssertValidationResult(loader, 9, 2, 1, 3, 5, 10, true,
					"Row 2 updated: " + MessageGenerator.GetUpdateTariffMessage("NA", "HSN", "123", "000001", startDate),
					"Row 3 created: " + MessageGenerator.GetCreateTariffMessage("NA", "HSN", "223", "000001", startDate),
					"Row 4 excluded: " + MessageGenerator.GetInvalidTariffTypeError("XXX") + MessageGenerator.GetEndDateError(startDate, "start date", endDate),
					"Row 5 created: " + MessageGenerator.GetCreateTariffMessage("ER", "HSN", "423", "000001", startDate),
					"Row 6 excluded: " + MessageGenerator.GetNotExistedError(FieldNames.Version, "000003"),
					"Row 7 excluded: " + MessageGenerator.GetInvalidVATRateCodeError("XXX", "NA") + MessageGenerator.GetEndDateError(invalidDate, Constant.VersionEffectiveDate, effectiveDate),
					"Row 8 excluded: " + MessageGenerator.GetEmptyValueForInsertError(new HashSet<ZString> { "Description", "UOM1" }),
					"Row 9 excluded: " + MessageGenerator.GetInvalidVATRateCodeError("XXX", "NA"));

				tariff1.Reload();
				AssertImportedTariff(tariff1, "Tariff1 DESC", startDate, endDate, "DV1");
				AssertTariffUOM(tariff1.PK, UOMTypeList.Codes.CU1, "uom1");
				AssertTariffUOM(tariff1.PK, UOMTypeList.Codes.CU2, "uom2");
				AssertTariffUOM(tariff1.PK, UOMTypeList.Codes.CU3, "uom3");

				var tariff2 = tariffLoader.LoadTariffByVersion("NA", "HSN", "223", "000001", startDate);
				AssertImportedTariff(tariff2, "Tariff2 DESC", startDate, null, "");
				AssertTariffUOM(tariff1.PK, UOMTypeList.Codes.CU1, "uom1");

				var tariff3 = tariffLoader.LoadTariffByVersion("ER", "HSN", "423", "000001", startDate);
				AssertImportedTariff(tariff3, "Tariff4 DESC", startDate, null, "DV2");
				AssertTariffUOM(tariff3.PK, UOMTypeList.Codes.CU1, "uom1");
				AssertTariffUOM(tariff3.PK, UOMTypeList.Codes.CU2, "uom2");
				AssertTariffUOM(tariff3.PK, UOMTypeList.Codes.CU3, "uom3");
			}
		}

		public void TestImportTariffCSV_NoStartDateColumn()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusTaxOrFeeType("VAT");
			helper.CreateTaxOrFee("DV1", 0.5m, "NA", 0.1m, 10m, "VAT");

			var effectiveDate = new ZDateTime(2021, 3, 16);
			var endDate = effectiveDate.AddMonths(1);
			CreateExistedTariff("000001", effectiveDate, effectiveDate, "HSN", "123", "NA");
			Factory.Save();

			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile, "Version, TariffType, TariffCode, CountryCode, Description, EndDate, TaxOrFeeCode, UOM1",
					$"000001, HSN, 123, NA, Tariff1 DESC, {endDate:yyyyMMdd}, DV1, uom1",
					$"000001, HSN, 223, NA, Tariff2 DESC, {endDate:yyyyMMdd}, DV1, uom1");
				loader.ImportTariffData(tempFile.Filename);

				AssertValidationResult(loader, 3, 1, 1, 2, 0, 4, true,
					"Row 2 updated: " + MessageGenerator.GetStarDateUpdateMessage(effectiveDate) + MessageGenerator.GetUpdateTariffMessage("NA", "HSN", "123", "000001", effectiveDate),
					"Row 3 created: " + MessageGenerator.GetStarDateUpdateMessage(effectiveDate) + MessageGenerator.GetCreateTariffMessage("NA", "HSN", "223", "000001", effectiveDate));

				var tariffLoader = new TariffView.Loader(Factory);
				var tariff1 = tariffLoader.LoadTariffByVersion("NA", "HSN", "123", "000001", effectiveDate);
				var tariff2 = tariffLoader.LoadTariffByVersion("NA", "HSN", "223", "000001", effectiveDate);
				tariff1.Reload();
				tariff2.Reload();
				AssertImportedTariff(tariff1, "Tariff1 DESC", effectiveDate, endDate, "DV1");
				AssertImportedTariff(tariff2, "Tariff2 DESC", effectiveDate, endDate, "DV1");
			}
		}

		public void TestImportTariffCSV_HasStartDateColumn()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusTaxOrFeeType("VAT");
			helper.CreateTaxOrFee("DV1", 0.5m, "NA", 0.1m, 10m, "VAT");

			var effectiveDate = new ZDateTime(2021, 3, 16);
			var startDate = effectiveDate.AddDays(1);
			var endDate = effectiveDate.AddMonths(1);
			CreateExistedTariff("000001", effectiveDate, effectiveDate, "HSN", "123", "NA");
			CreateExistedTariff("000002", effectiveDate.AddDays(1), startDate, "HSN", "223", "NA");
			Factory.Save();

			var newStartDate = effectiveDate.AddDays(2);
			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile, "Version, TariffType, TariffCode, CountryCode, Description, StartDate, EndDate, TaxOrFeeCode, UOM1",
					$"000001, HSN, 123, NA, Tariff1 DESC, , {endDate:yyyyMMdd}, DV1, uom1",
					$"000002, HSN, 223, NA, Tariff2 DESC, {startDate:yyyyMMdd}, {endDate:yyyyMMdd}, DV1, uom1",
					$"000001, HSN, 323, NA, Tariff3 DESC, , {endDate:yyyyMMdd}, DV1, uom1",
					$"000001, HSN, 423, NA, Tariff4 DESC, {newStartDate:yyyyMMdd}, {endDate:yyyyMMdd}, DV1, uom1");

				loader.ImportTariffData(tempFile.Filename);
				AssertValidationResult(loader, 5, 2, 2, 4, 0, 6, true,
					"Row 2 updated: " + MessageGenerator.GetStarDateUpdateMessage(effectiveDate) + MessageGenerator.GetUpdateTariffMessage("NA", "HSN", "123", "000001", effectiveDate),
					"Row 3 updated: " + MessageGenerator.GetUpdateTariffMessage("NA", "HSN", "223", "000002", startDate),
					"Row 4 created: " + MessageGenerator.GetStarDateUpdateMessage(effectiveDate) + MessageGenerator.GetCreateTariffMessage("NA", "HSN", "323", "000001", effectiveDate),
					"Row 5 created: " + MessageGenerator.GetCreateTariffMessage("NA", "HSN", "423", "000001", newStartDate));

				var tariffLoader = new TariffView.Loader(Factory);
				var tariff1 = tariffLoader.LoadTariffByVersion("NA", "HSN", "123", "000001", effectiveDate);
				var tariff2 = tariffLoader.LoadTariffByVersion("NA", "HSN", "223", "000002", startDate);
				var tariff3 = tariffLoader.LoadTariffByVersion("NA", "HSN", "323", "000001", effectiveDate);
				var tariff4 = tariffLoader.LoadTariffByVersion("NA", "HSN", "423", "000001", newStartDate);
				tariff1.Reload();
				tariff2.Reload();
				AssertImportedTariff(tariff1, "Tariff1 DESC", effectiveDate, endDate, "DV1");
				AssertImportedTariff(tariff2, "Tariff2 DESC", startDate, endDate, "DV1");
				AssertImportedTariff(tariff3, "Tariff3 DESC", effectiveDate, endDate, "DV1");
				AssertImportedTariff(tariff4, "Tariff4 DESC", newStartDate, endDate, "DV1");
			}
		}

		public void TestImportTariffCSV_NoEndDateColumn()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusTaxOrFeeType("VAT");
			helper.CreateTaxOrFee("DV1", 0.5m, "NA", 0.1m, 10m, "VAT");

			var effectiveDate = new ZDateTime(2021, 3, 16);
			var startDate = effectiveDate.AddDays(1);
			CreateExistedTariff("000001", effectiveDate, startDate, "HSN", "123", "NA");
			Factory.Save();

			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile,
					"Version, TariffType, TariffCode, CountryCode, Description, StartDate, TaxOrFeeCode, UOM1",
					$"000001, HSN, 123, NA, Tariff1 DESC, {startDate:yyyyMMdd}, DV1, uom1",
					$"000001, HSN, 223, NA, Tariff2 DESC, {startDate:yyyyMMdd}, DV1, uom1");
				loader.ImportTariffData(tempFile.Filename);

				AssertValidationResult(loader, 3, 1, 1, 2, 0, 4, true,
					"Row 2 updated: " + MessageGenerator.GetUpdateTariffMessage("NA", "HSN", "123", "000001", startDate),
					"Row 3 created: " + MessageGenerator.GetCreateTariffMessage("NA", "HSN", "223", "000001", startDate));

				var tariffLoader = new TariffView.Loader(Factory);
				var tariff1 = tariffLoader.LoadTariffByVersion("NA", "HSN", "123", "000001", startDate);
				var tariff2 = tariffLoader.LoadTariffByVersion("NA", "HSN", "223", "000001", startDate);
				tariff1.Reload();
				tariff2.Reload();
				AssertImportedTariff(tariff1, "Tariff1 DESC", startDate, new ZDate(2079, 06, 06), "DV1");
				AssertImportedTariff(tariff2, "Tariff2 DESC", startDate, new ZDate(9999, 12, 31), "DV1");
			}
		}

		public void TestImportTariffCSV_EmptyEndDateColumn()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusTaxOrFeeType("VAT");
			helper.CreateTaxOrFee("DV1", 0.5m, "NA", 0.1m, 10m, "VAT");

			var effectiveDate = new ZDateTime(2021, 3, 16);
			var startDate = effectiveDate.AddDays(1);
			CreateExistedTariff("000001", effectiveDate, startDate, "HSN", "123", "NA");
			Factory.Save();

			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile, "Version, TariffType, TariffCode, CountryCode, Description, StartDate, EndDate, TaxOrFeeCode, UOM1",
					$"000001, HSN, 123, NA, Tariff1 DESC, {startDate:yyyyMMdd}, , DV1, uom1",
					$"000001, HSN, 223, NA, Tariff2 DESC, {startDate:yyyyMMdd}, , DV1, uom1");
				loader.ImportTariffData(tempFile.Filename);

				AssertValidationResult(loader, 3, 1, 1, 2, 0, 4, true,
					"Row 2 updated: " + MessageGenerator.GetUpdateTariffMessage("NA", "HSN", "123", "000001", startDate),
					"Row 3 created: " + MessageGenerator.GetCreateTariffMessage("NA", "HSN", "223", "000001", startDate));

				var tariffLoader = new TariffView.Loader(Factory);
				var tariff1 = tariffLoader.LoadTariffByVersion("NA", "HSN", "123", "000001", startDate);
				var tariff2 = tariffLoader.LoadTariffByVersion("NA", "HSN", "223", "000001", startDate);
				tariff1.Reload();
				tariff2.Reload();
				AssertImportedTariff(tariff1, "Tariff1 DESC", startDate, new ZDate(2079, 06, 06), "DV1");
				AssertImportedTariff(tariff2, "Tariff2 DESC", startDate, new ZDate(9999, 12, 31), "DV1");
			}
		}

		#endregion

		#region Import Validation

		public void TestImportTariffCSV_Update_UniqueValidation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusTaxOrFeeType("VAT");
			helper.CreateTaxOrFee("DV1", 0.5m, "NA", 0.1m, 10m, "VAT");

			var effectiveDate = new ZDateTime(2021, 3, 16);
			var tariff1StartDate = effectiveDate.AddDays(1);
			var tariff1EndDate = tariff1StartDate.AddDays(2);
			var tariff1 = CreateExistedTariff("000001", effectiveDate, tariff1StartDate, "HSN", "123", "NA");
			tariff1.ZZ1_EndDate = tariff1EndDate;

			var endDate2 = tariff1EndDate.AddDays(2);
			Factory.Save();

			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile,
					"Version, TariffType, TariffCode, CountryCode, Description, StartDate, EndDate, TaxOrFeeCode, UOM1",
					$"000001, HSN, 123, NA, Tariff1 DESC, {tariff1StartDate.AddDays(1):yyyyMMdd}, {tariff1EndDate:yyyyMMdd}, DV1, uom1",
					$"000001, HSN, 123, NA, Tariff1 DESC, {tariff1StartDate:yyyyMMdd}, {endDate2:yyyyMMdd}, DV1, uom1",
					$"000001, HSN, 123, NA, Tariff1 DESC, {tariff1StartDate.AddDays(1):yyyyMMdd}, {endDate2:yyyyMMdd}, DV1, uom1",
					$"000001, HSN, 223, NA, Tariff2 DESC, {tariff1StartDate.AddDays(1):yyyyMMdd}, {endDate2:yyyyMMdd}, DV1, uom1");
				loader.ImportTariffData(tempFile.Filename);

				AssertValidationResult(loader, 5, 1, 1, 2, 2, 6, true,
					"Row 2 excluded: " + MessageGenerator.GetDuplicateTariffError(tariff1StartDate.AddDays(1), tariff1EndDate, "HSN", "123", "NA", "000001"),
					"Row 3 updated: " + MessageGenerator.GetUpdateTariffMessage("NA", "HSN", "123", "000001", tariff1StartDate),
					"Row 4 excluded: " + MessageGenerator.GetDuplicateTariffError(tariff1StartDate.AddDays(1), endDate2, "HSN", "123", "NA", "000001"),
					"Row 5 created: " + MessageGenerator.GetCreateTariffMessage("NA", "HSN", "223", "000001", tariff1StartDate.AddDays(1)));
			}
		}

		public void TestImportTariffCSV_Update_DateOverlapValidation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusTaxOrFeeType("VAT");
			helper.CreateTaxOrFee("DV1", 0.5m, "NA", 0.1m, 10m, "VAT");

			var effectiveDate = new ZDateTime(2021, 3, 16);
			var tariff1StartDate = effectiveDate.AddYears(1);
			var tariff1EndDate = tariff1StartDate.AddYears(1);
			var tariff1 = CreateExistedTariff("000001", effectiveDate, tariff1StartDate, "HSN", "123", "NA");
			tariff1.ZZ1_EndDate = tariff1EndDate;

			Factory.Save();

			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile,
					"Version, TariffType, TariffCode, CountryCode, Description, StartDate, EndDate, TaxOrFeeCode, UOM1",
					$"000001, HSN, 123, NA, Tariff4 DESC, {tariff1StartDate.AddDays(-1):yyyyMMdd}, {tariff1StartDate.AddDays(1):yyyyMMdd}, DV1, uom1",
					$"000001, HSN, 123, NA, Tariff1 DESC, {tariff1EndDate.AddDays(-1):yyyyMMdd}, {tariff1EndDate.AddDays(1):yyyyMMdd}, DV1, uom1",
					$"000001, HSN, 123, NA, Tariff2 DESC, {tariff1StartDate:yyyyMMdd}, {tariff1EndDate.AddDays(2):yyyyMMdd}, DV1, uom1",
					$"000001, HSN, 123, NA, Tariff2 DESC, {tariff1EndDate.AddDays(1):yyyyMMdd}, {tariff1EndDate.AddDays(3):yyyyMMdd}, DV1, uom1");
				loader.ImportTariffData(tempFile.Filename);

				AssertValidationResult(loader, 5, 0, 1, 1, 3, 6, true,
					"Row 2 excluded: " + MessageGenerator.GetDateOverlapError(tariff1StartDate.AddDays(-1), tariff1StartDate.AddDays(1), "HSN", "123", "NA", "000001"),
					"Row 3 excluded: " + MessageGenerator.GetDateOverlapError(tariff1EndDate.AddDays(-1), tariff1EndDate.AddDays(1), "HSN", "123", "NA", "000001"),
					"Row 4 updated: " + MessageGenerator.GetUpdateTariffMessage("NA", "HSN", "123", "000001", tariff1StartDate),
					"Row 5 excluded: " + MessageGenerator.GetDateOverlapError(tariff1EndDate.AddDays(1), tariff1EndDate.AddDays(3), "HSN", "123", "NA", "000001"));
			}
		}

		public void TestImportTariffCSV_Update_UOMValidation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusTaxOrFeeType("VAT");
			helper.CreateTaxOrFee("DV1", 0.5m, "NA", 0.1m, 10m, "VAT");

			var effectiveDate = new ZDateTime(2021, 3, 16);
			var startDate = effectiveDate.AddDays(3);
			CreateExistedTariff("000001", effectiveDate, startDate, "HSN", "123", "NA");
			var existedTariff2 = CreateExistedTariff("000002", effectiveDate.AddDays(1), startDate, "HSN", "223", "NA");
			var existedTariff3 = CreateExistedTariff("000003", effectiveDate.AddDays(2), startDate, "HSN", "323", "NA");
			Factory.Save();

			helper.CreateTariffUOM(existedTariff2, UOMTypeList.Codes.CU1, "CU1");
			helper.CreateTariffUOM(existedTariff2, UOMTypeList.Codes.CU2, "CU2");
			helper.CreateTariffUOM(existedTariff2, UOMTypeList.Codes.CU3, "CU3");
			helper.CreateTariffUOM(existedTariff3, UOMTypeList.Codes.CU1, "CU1");
			helper.CreateTariffUOM(existedTariff3, UOMTypeList.Codes.CU2, "CU2");
			helper.CreateTariffUOM(existedTariff3, UOMTypeList.Codes.CU3, "CU3");
			Factory.Save();

			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile,
					"Version, TariffType, TariffCode, CountryCode, StartDate, Description, TaxOrFeeCode, UOM1, UOM2, UOM3",
					$"000001, HSN, 123, NA, {startDate:yyyyMMdd}, Tariff1 DESC, DV1, ,uom2,",
					$"000001, HSN, 123, NA, {startDate:yyyyMMdd}, Tariff1 DESC, DV1, , ,uom3",
					$"000001, HSN, 123, NA, {startDate:yyyyMMdd}, Tariff1 DESC, DV1, uom1, uom1, uom3",
					$"000001, HSN, 123, NA, {startDate:yyyyMMdd}, Tariff1 DESC, DV1, , ,",
					$"000002, HSN, 223, NA, {startDate:yyyyMMdd}, Tariff2 DESC, DV1, , uom2, uom3",
					$"000002, HSN, 223, NA, {startDate:yyyyMMdd}, Tariff2 DESC, DV1, , , uom3",
					$"000002, HSN, 223, NA, {startDate:yyyyMMdd}, Tariff2 DESC, DV1, uom1, CU2, CU2",
					$"000002, HSN, 223, NA, {startDate:yyyyMMdd}, Tariff2 DESC, DV1, uom1, , ",
					$"000003, HSN, 323, NA, {startDate:yyyyMMdd}, Tariff2 DESC, DV1, , , ");
				loader.ImportTariffData(tempFile.Filename);

				AssertValidationResult(loader, 10, 0, 2, 2, 7, 11, true,
					"Row 2 excluded: " + MessageGenerator.GetNoUOM1Error(),
					"Row 3 excluded: " + MessageGenerator.GetNoUOM2Error(),
					"Row 4 excluded: " + MessageGenerator.GetDuplicateUOMsError(),
					"Row 5 updated: " + MessageGenerator.GetUpdateTariffMessage("NA", "HSN", "123", "000001", startDate),
					"Row 6 excluded: " + MessageGenerator.GetNoUOM1Error(),
					"Row 7 excluded: " + MessageGenerator.GetNoUOM2Error(),
					"Row 8 excluded: " + MessageGenerator.GetDuplicateUOMsError(),
					"Row 9 updated: " + MessageGenerator.GetUpdateTariffMessage("NA", "HSN", "223", "000002", startDate),
					"Row 10 excluded: " + MessageGenerator.GetEmptyUOMsError());

				var newFactory = new BusinessObjectFactory();
				var tariffLoader = new TariffView.Loader(newFactory);
				var tariff1 = tariffLoader.LoadTariffByVersion("NA", "HSN", "123", "000001", startDate);
				var tariff2 = tariffLoader.LoadTariffByVersion("NA", "HSN", "223", "000002", startDate);
				var tariff3 = tariffLoader.LoadTariffByVersion("NA", "HSN", "323", "000003", startDate);
				AssertImportedTariff(tariff1, "Tariff1 DESC", startDate, null, "DV1");
				AssertImportedTariff(tariff2, "Tariff2 DESC", startDate, null, "DV1");
				AssertEquals(0, tariff1.UnitsOfMeasure.Count);
				AssertEquals(1, tariff2.UnitsOfMeasure.Count);
				AssertEquals(3, tariff3.UnitsOfMeasure.Count);
				AssertEquals(UOMTypeList.Codes.CU1, tariff2.UnitsOfMeasure[0].ZZ8_Type);
				AssertEquals("uom1", tariff2.UnitsOfMeasure[0].ZZ8_UOM);
			}
		}

		public void TestImportTariffCSV_Update_UOMValidation_NoUOM2()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusTaxOrFeeType("VAT");
			helper.CreateTaxOrFee("DV1", 0.5m, "NA", 0.1m, 10m, "VAT");

			var effectiveDate = new ZDateTime(2021, 3, 16);
			var startDate = effectiveDate.AddDays(1);
			var existedTariff1 = CreateExistedTariff("000002", effectiveDate, startDate, "HSN", "223", "NA");
			helper.CreateTariffUOM(existedTariff1, UOMTypeList.Codes.CU1, "CU1");
			helper.CreateTariffUOM(existedTariff1, UOMTypeList.Codes.CU2, "CU2");
			helper.CreateTariffUOM(existedTariff1, UOMTypeList.Codes.CU3, "CU3");
			var existedTariff2 = CreateExistedTariff("000003", effectiveDate.AddDays(1), startDate, "HSN", "323", "NA");
			helper.CreateTariffUOM(existedTariff2, UOMTypeList.Codes.CU1, "CU1");
			helper.CreateTariffUOM(existedTariff2, UOMTypeList.Codes.CU2, "CU2");
			helper.CreateTariffUOM(existedTariff2, UOMTypeList.Codes.CU3, "CU3");
			Factory.Save();

			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile,
					"Version, TariffType, TariffCode, CountryCode, StartDate, Description, TaxOrFeeCode, UOM1, UOM3",
					$"000002, HSN, 223, NA, {startDate:yyyyMMdd}, Tariff2 DESC, DV1, , uom3",
					$"000002, HSN, 223, NA, {startDate:yyyyMMdd},Tariff2 DESC, DV1, uom1, uom1",
					$"000002, HSN, 223, NA, {startDate:yyyyMMdd},Tariff2 DESC, DV1, uom1, CU2",
					$"000002, HSN, 223, NA, {startDate:yyyyMMdd},Tariff2 DESC, DV1, uom1, ",
					$"000003, HSN, 323, NA, {startDate:yyyyMMdd},Tariff2 DESC, DV1, , ");
				loader.ImportTariffData(tempFile.Filename);

				AssertValidationResult(loader, 6, 0, 1, 1, 4, 7, true,
					"Row 2 excluded: " + MessageGenerator.GetNoUOM1Error(),
					"Row 3 excluded: " + MessageGenerator.GetDuplicateUOMsError(),
					"Row 4 excluded: " + MessageGenerator.GetDuplicateUOMsError(),
					"Row 5 updated: " + MessageGenerator.GetUpdateTariffMessage("NA", "HSN", "223", "000002", startDate),
					"Row 6 excluded: " + MessageGenerator.GetNoUOM1Error());

				var newFactory = new BusinessObjectFactory();
				var tariffLoader = new TariffView.Loader(newFactory);
				var tariff1 = tariffLoader.LoadTariffByVersion("NA", "HSN", "223", "000002", startDate);
				AssertImportedTariff(tariff1, "Tariff2 DESC", startDate, null, "DV1");

				var uoms = tariff1.UnitsOfMeasure.OrderBy(x => x.ZZ8_Type).ToArray();
				AssertEquals(2, uoms.Length);
				AssertEquals(UOMTypeList.Codes.CU1, uoms[0].ZZ8_Type);
				AssertEquals("uom1", uoms[0].ZZ8_UOM);
				AssertEquals(UOMTypeList.Codes.CU2, uoms[1].ZZ8_Type);
				AssertEquals("CU2", uoms[1].ZZ8_UOM);
			}
		}

		public void TestImportTariffCSV_Update_UOMValidation_NoUOM3()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusTaxOrFeeType("VAT");
			helper.CreateTaxOrFee("DV1", 0.5m, "NA", 0.1m, 10m, "VAT");

			var effectiveDate = new ZDateTime(2021, 3, 16);
			var startDate = effectiveDate.AddDays(1);
			var existedTariff1 = CreateExistedTariff("000002", effectiveDate, startDate, "HSN", "223", "NA");
			helper.CreateTariffUOM(existedTariff1, UOMTypeList.Codes.CU1, "CU1");
			helper.CreateTariffUOM(existedTariff1, UOMTypeList.Codes.CU2, "CU2");
			helper.CreateTariffUOM(existedTariff1, UOMTypeList.Codes.CU3, "CU3");
			Factory.Save();

			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile,
					"Version, TariffType, TariffCode, CountryCode, StartDate, Description, TaxOrFeeCode, UOM1, UOM2",
					$"000002, HSN, 223, NA, {startDate:yyyyMMdd},Tariff2 DESC, DV1, , ",
					$"000002, HSN, 223, NA, {startDate:yyyyMMdd},Tariff2 DESC, DV1, , uom2",
					$"000002, HSN, 223, NA, {startDate:yyyyMMdd},Tariff2 DESC, DV1, uom1, uom1",
					$"000002, HSN, 223, NA, {startDate:yyyyMMdd},Tariff2 DESC, DV1, CU2, ",
					$"000002, HSN, 223, NA, {startDate:yyyyMMdd},Tariff2 DESC, DV1, uom1, CU3",
					$"000002, HSN, 223, NA, {startDate:yyyyMMdd},Tariff2 DESC, DV1, uom1, uom2");
				loader.ImportTariffData(tempFile.Filename);

				AssertValidationResult(loader, 7, 0, 1, 1, 5, 8, true,
					"Row 2 excluded: " + MessageGenerator.GetNoUOM2Error(),
					"Row 3 excluded: " + MessageGenerator.GetNoUOM1Error(),
					"Row 4 excluded: " + MessageGenerator.GetDuplicateUOMsError(),
					"Row 5 excluded: " + MessageGenerator.GetNoUOM2Error(),
					"Row 6 excluded: " + MessageGenerator.GetDuplicateUOMsError(),
					"Row 7 updated: " + MessageGenerator.GetUpdateTariffMessage("NA", "HSN", "223", "000002", startDate));

				var newFactory = new BusinessObjectFactory();
				var tariffLoader = new TariffView.Loader(newFactory);
				var tariff1 = tariffLoader.LoadTariffByVersion("NA", "HSN", "223", "000002", startDate);
				AssertImportedTariff(tariff1, "Tariff2 DESC", startDate, null, "DV1");

				var uoms = tariff1.UnitsOfMeasure.OrderBy(x => x.ZZ8_Type).ToArray();
				AssertEquals(3, uoms.Length);
				AssertEquals(UOMTypeList.Codes.CU1, uoms[0].ZZ8_Type);
				AssertEquals("uom1", uoms[0].ZZ8_UOM);
				AssertEquals(UOMTypeList.Codes.CU2, uoms[1].ZZ8_Type);
				AssertEquals("uom2", uoms[1].ZZ8_UOM);
				AssertEquals(UOMTypeList.Codes.CU3, uoms[2].ZZ8_Type);
				AssertEquals("CU3", uoms[2].ZZ8_UOM);
			}
		}

		public void TestImportTariffCSV_Update_UOMValidation_NoUOM1()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusTaxOrFeeType("VAT");
			helper.CreateTaxOrFee("DV1", 0.5m, "NA", 0.1m, 10m, "VAT");

			var effectiveDate = new ZDateTime(2021, 3, 16);
			var startDate = effectiveDate.AddDays(1);
			var existedTariff1 = CreateExistedTariff("000002", effectiveDate, startDate, "HSN", "223", "NA");
			helper.CreateTariffUOM(existedTariff1, UOMTypeList.Codes.CU1, "CU1");
			helper.CreateTariffUOM(existedTariff1, UOMTypeList.Codes.CU2, "CU2");
			helper.CreateTariffUOM(existedTariff1, UOMTypeList.Codes.CU3, "CU3");
			var existedTariff2 = CreateExistedTariff("000003", effectiveDate.AddDays(1), startDate, "HSN", "323", "NA");
			helper.CreateTariffUOM(existedTariff2, UOMTypeList.Codes.CU1, "CU1");
			helper.CreateTariffUOM(existedTariff2, UOMTypeList.Codes.CU2, "CU2");
			helper.CreateTariffUOM(existedTariff2, UOMTypeList.Codes.CU3, "CU3");
			Factory.Save();

			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile,
					"Version, TariffType, TariffCode, CountryCode, StartDate, Description, TaxOrFeeCode, UOM2, UOM3",
					$"000002, HSN, 223, NA, {startDate:yyyyMMdd},Tariff2 DESC, DV1, , ",
					$"000002, HSN, 223, NA, {startDate:yyyyMMdd},Tariff2 DESC, DV1, , uom",
					$"000002, HSN, 223, NA, {startDate:yyyyMMdd},Tariff2 DESC, DV1, uom, uom",
					$"000003, HSN, 323, NA, {startDate:yyyyMMdd},Tariff2 DESC, DV1, uom, ");
				loader.ImportTariffData(tempFile.Filename);

				AssertValidationResult(loader, 5, 0, 2, 2, 2, 6, true,
					"Row 2 updated: " + MessageGenerator.GetUpdateTariffMessage("NA", "HSN", "223", "000002", startDate),
					"Row 3 excluded: " + MessageGenerator.GetNoUOM2Error(),
					"Row 4 excluded: " + MessageGenerator.GetDuplicateUOMsError(),
					"Row 5 updated: " + MessageGenerator.GetUpdateTariffMessage("NA", "HSN", "323", "000003", startDate));

				var newFactory = new BusinessObjectFactory();
				var tariffLoader = new TariffView.Loader(newFactory);
				var tariff1 = tariffLoader.LoadTariffByVersion("NA", "HSN", "223", "000002", startDate);
				AssertImportedTariff(tariff1, "Tariff2 DESC", startDate, null, "DV1");
				var uoms = tariff1.UnitsOfMeasure.OrderBy(x => x.ZZ8_Type).ToArray();
				AssertEquals(1, uoms.Length);
				AssertEquals(UOMTypeList.Codes.CU1, uoms[0].ZZ8_Type);
				AssertEquals("CU1", uoms[0].ZZ8_UOM);

				var tariff2 = tariffLoader.LoadTariffByVersion("NA", "HSN", "323", "000003", startDate);
				AssertImportedTariff(tariff2, "Tariff2 DESC", startDate, null, "DV1");
				var uoms2 = tariff2.UnitsOfMeasure.OrderBy(x => x.ZZ8_Type).ToArray();
				AssertEquals(2, uoms2.Length);
				AssertEquals(UOMTypeList.Codes.CU1, uoms2[0].ZZ8_Type);
				AssertEquals("CU1", uoms2[0].ZZ8_UOM);
				AssertEquals(UOMTypeList.Codes.CU2, uoms2[1].ZZ8_Type);
				AssertEquals("uom", uoms2[1].ZZ8_UOM);
			}
		}

		public void TestImportTariffCSV_Update_NoUOMs()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusTaxOrFeeType("VAT");
			helper.CreateTaxOrFee("DV1", 0.5m, "NA", 0.1m, 10m, "VAT");

			var effectiveDate = new ZDateTime(2021, 3, 16);
			var tariffStartDate = effectiveDate.AddDays(1);
			var existedTariff1 = CreateExistedTariff("000002", effectiveDate, tariffStartDate, "HSN", "223", "NA");
			helper.CreateTariffUOM(existedTariff1, UOMTypeList.Codes.CU1, "CU1");
			helper.CreateTariffUOM(existedTariff1, UOMTypeList.Codes.CU2, "CU2");
			helper.CreateTariffUOM(existedTariff1, UOMTypeList.Codes.CU3, "CU3");
			Factory.Save();

			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile,
					"Version, TariffType, TariffCode, CountryCode, StartDate, Description, TaxOrFeeCode",
					$"000002, HSN, 223, NA, {tariffStartDate:yyyyMMdd},Tariff2 DESC, DV1");
				loader.ImportTariffData(tempFile.Filename);

				AssertValidationResult(loader, 2, 0, 1, 1, 0, 3, true, "Row 2 updated: " + MessageGenerator.GetUpdateTariffMessage("NA", "HSN", "223", "000002", tariffStartDate));

				var newFactory = new BusinessObjectFactory();
				var tariffLoader = new TariffView.Loader(newFactory);
				var tariff1 = tariffLoader.LoadTariffByVersion("NA", "HSN", "223", "000002", tariffStartDate);
				AssertImportedTariff(tariff1, "Tariff2 DESC", tariffStartDate, null, "DV1");

				var uoms = tariff1.UnitsOfMeasure.OrderBy(x => x.ZZ8_Type).ToArray();
				AssertEquals(3, uoms.Length);
				AssertEquals(UOMTypeList.Codes.CU1, uoms[0].ZZ8_Type);
				AssertEquals("CU1", uoms[0].ZZ8_UOM);
				AssertEquals(UOMTypeList.Codes.CU2, uoms[1].ZZ8_Type);
				AssertEquals("CU2", uoms[1].ZZ8_UOM);
				AssertEquals(UOMTypeList.Codes.CU3, uoms[2].ZZ8_Type);
				AssertEquals("CU3", uoms[2].ZZ8_UOM);
			}
		}

		public void TestImportTariffCSV_Insert_UOM2Validation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusTaxOrFeeType("VAT");
			helper.CreateTaxOrFee("DV1", 0.5m, "NA", 0.1m, 10m, "VAT");
			Factory.Save();

			var effectiveDate = new ZDateTime(2021, 3, 16);
			CreateVersion("123456", "123456 DESC", effectiveDate);

			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile,
					"Version, TariffType, TariffCode, CountryCode, StartDate, Description, UOM1, UOM2, UOM3",
					$"123456, HSN, 123, NA, {effectiveDate:yyyyMMdd},desc, KG, ,KG3");
				loader.ImportTariffData(tempFile.Filename);
				AssertValidationResult(loader, 2, 0, 0, 0, 1, 3, true, "Row 2 excluded: " + MessageGenerator.GetNoUOM2Error());
			}
		}

		public void TestImportTariffCSV_Insert_NoUOMs()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusTaxOrFeeType("VAT");
			helper.CreateTaxOrFee("DV1", 0.5m, "NA", 0.1m, 10m, "VAT");
			Factory.Save();

			var effectiveDate = new ZDateTime(2021, 3, 16);
			CreateVersion("123456", "123456 DESC", effectiveDate);

			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile,
					"Version, TariffType, TariffCode, CountryCode, StartDate, Description",
					$"123456, HSN, 123, NA, {effectiveDate:yyyyMMdd},desc");
				loader.ImportTariffData(tempFile.Filename);
				AssertValidationResult(loader, 2, 0, 0, 0, 1, 3, true, "Row 2 excluded: " + MessageGenerator.GetNoMandatoryColumnsError(new HashSet<string> { "UOM1" }));
			}
		}

		public void TestImportTariffCSV_Insert_EmptyDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusTaxOrFeeType("VAT");
			helper.CreateTaxOrFee("DV1", 0.5m, "NA", 0.1m, 10m, "VAT");
			Factory.Save();

			var effectiveDate = new ZDateTime(2021, 3, 16);
			CreateVersion("123456", "123456 DESC", effectiveDate);

			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile,
					"Version, TariffType, TariffCode, CountryCode, StartDate, Description, UOM1",
					$"123456, HSN, 123, NA, {effectiveDate:yyyyMMdd}, , KG");
				loader.ImportTariffData(tempFile.Filename);
				AssertValidationResult(loader, 2, 0, 0, 0, 1, 3, true, "Row 2 excluded: " + MessageGenerator.GetEmptyValueForInsertError(new HashSet<ZString> { "Description" }));
			}
		}

		public void TestImportTariffCSV_Insert_OnlyOneEmptyUOM1()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusTaxOrFeeType("VAT");
			helper.CreateTaxOrFee("DV1", 0.5m, "NA", 0.1m, 10m, "VAT");
			Factory.Save();

			var effectiveDate = new ZDateTime(2021, 3, 16);
			CreateVersion("123456", "123456 DESC", effectiveDate);

			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile,
					"Version, TariffType, TariffCode, CountryCode, StartDate, Description, UOM1",
					$"123456, HSN, 123, NA, {effectiveDate:yyyyMMdd},desc,");
				loader.ImportTariffData(tempFile.Filename);
				AssertValidationResult(loader, 2, 0, 0, 0, 1, 3, true, "Row 2 excluded: " + MessageGenerator.GetEmptyValueForInsertError(new HashSet<ZString> { "UOM1" }));
			}
		}

		public void TestImportTariffCSV_Insert_OnlyOneEmptyUOM2()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusTaxOrFeeType("VAT");
			helper.CreateTaxOrFee("DV1", 0.5m, "NA", 0.1m, 10m, "VAT");
			Factory.Save();

			var effectiveDate = new ZDateTime(2021, 3, 16);
			CreateVersion("123456", "123456 DESC", effectiveDate);

			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile,
					"Version, TariffType, TariffCode, CountryCode, StartDate, Description, UOM2",
					$"123456, HSN, 123, NA, {effectiveDate:yyyyMMdd},desc,");
				loader.ImportTariffData(tempFile.Filename);
				AssertValidationResult(loader, 2, 0, 0, 0, 1, 3, true, "Row 2 excluded: " + MessageGenerator.GetNoMandatoryColumnsError(new HashSet<string> { "UOM1" }));
			}
		}

		public void TestImportTariffCSV_Insert_OnlyOneEmptyUOM3()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusTaxOrFeeType("VAT");
			helper.CreateTaxOrFee("DV1", 0.5m, "NA", 0.1m, 10m, "VAT");
			Factory.Save();

			var effectiveDate = new ZDateTime(2021, 3, 16);
			CreateVersion("123456", "123456 DESC", effectiveDate);

			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile,
					"Version, TariffType, TariffCode, CountryCode, StartDate, Description, UOM3",
					$"123456, HSN, 123, NA, {effectiveDate:yyyyMMdd}, desc,");
				loader.ImportTariffData(tempFile.Filename);
				AssertValidationResult(loader, 2, 0, 0, 0, 1, 3, true, "Row 2 excluded: " + MessageGenerator.GetNoMandatoryColumnsError(new HashSet<string> { "UOM1" }));
			}
		}

		public void TestImportTariffCSV_NoError_StartDateEarlierThanVersionEffectiveDate()
		{
			var effectiveDate = ZDateTime.Today;
			var startDate = effectiveDate.AddDays(-1);
			CreateVersion("123456", "123456 DESC", effectiveDate);

			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile,
					"Version, TariffType, TariffCode, CountryCode, StartDate, Description, UOM1",
					$"123456, HSN, 123, NA, {startDate:yyyyMMdd}, desc, KG");
				loader.ImportTariffData(tempFile.Filename);
				AssertValidationResult(loader, 2, 1, 0, 1, 0, 3, true);
			}
		}

		public void TestImportTariffCSV_InvalidDate_EndDateEarlierThanVersionEffectiveDate()
		{
			var effectiveDate = ZDateTime.Today;
			var endDate = effectiveDate.AddDays(-1);
			CreateVersion("123456", "123456 DESC", effectiveDate);

			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile,
					"Version, TariffType, TariffCode, CountryCode, StartDate, EndDate, Description, UOM1",
					$"123456, HSN, 123, NA, {endDate:yyyyMMdd}, {endDate:yyyyMMdd}, desc, KG");
				loader.ImportTariffData(tempFile.Filename);
				AssertValidationResult(loader, 2, 0, 0, 0, 1, 3, true, "Row 2 excluded: " + MessageGenerator.GetEndDateError(endDate, Constant.VersionEffectiveDate, effectiveDate));
			}
		}

		public void TestImportTariffCSV_InvalidTaxOrFeeCode()
		{
			CreateVersion("000001", "000001 DESC", ZDateTime.Today);

			var tariffLoader = new TariffView.Loader(Factory);
			AssertEquals(null, tariffLoader.LoadTariffByVersion("NA", "HSN", "123", "000001", ZDateTime.Today));

			using (var tempFile = TempFile.New())
			{
				PopulateTestFile(tempFile,
					"Version, TariffType, TariffCode, CountryCode, StartDate, TaxOrFeeCode, Description, UOM1",
					$"000001, HSN, 123, NA, {ZDateTime.Today:yyyyMMdd}, XXX, Desc, KG");
				loader.ImportTariffData(tempFile.Filename);
				AssertValidationResult(loader, 2, 0, 0, 0, 1, 3, true, "Row 2 excluded: " + MessageGenerator.GetInvalidVATRateCodeError("XXX", "NA"));
			}
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

		TariffView CreateExistedTariff(ZString version, ZDateTime versionEffectiveDate, ZDateTime tariffStartDate, ZString type, ZString code, ZString dataGrouping)
		{
			CreateVersion(version, "Version Desc" + version, versionEffectiveDate);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusTaxOrFeeType("VAT");
			helper.CreateTaxOrFee("DV1", 0.5m, dataGrouping, 0.1m, 10m, "VAT");
			helper.CreateTaxOrFee("DV2", 1m, Core.Constants.CountryCodes.Eritrea, 0.1m, 10m, "VAT");
			var tariffType = helper.CreateNewOrGetExistingTariffType(dataGrouping, type);
			Factory.Save();

			var existedTariff = helper.LoadOrCreateNewTariff(dataGrouping, tariffType.PK, code, tariffStartDate, new ZDate(2079, 06, 06), "tariff desc", isSystem: false, versionCode: version);
			Factory.Save();

			return existedTariff;
		}

		static void AssertValidationResult(TariffViewDataLoad loader, int recordsToImportPlusHeader, int recsCreated, int recsUpdated, int recsToUpdate, int recsExcluded, int logCount, bool fileHeaderIsValid, params string[] messages)
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
				AssertContains("Log.Message", $"Tariffs to Import = {recordsToImportPlusHeader - 1}", loader.Log[0]);

				for (int i = 0; i < messages.Length; i++)
				{
					AssertEquals("log", messages[i], loader.Log[i + 1]);
				}
			});
		}

		static void AssertImportedTariff(TariffView importedTariff, ZString description, ZDateTime? startDate, ZDateTime? endDate, ZString taxOrFeeCode)
		{
			CombineAssertions("AssertImportedTariff", () =>
			{
				AssertEquals("The tariff has been saved in DB", true, importedTariff.IsInDatabase);
				AssertEquals("The tariff is not system data and saved in CusRefTariff table", false, importedTariff.ZZ1_IsSystem);
				AssertEquals("ZZ1_Description", description, importedTariff.ZZ1_Description);
				if (startDate != null)
				{
					AssertEquals("ZZ1_StartDate", startDate.Value, importedTariff.ZZ1_StartDate);
				}

				if (endDate != null)
				{
					AssertEquals("ZZ1_EndDate", endDate.Value, importedTariff.ZZ1_EndDate);
				}

				AssertEquals("ZZ1_ZZF_NKTaxOrFeeCode", taxOrFeeCode, importedTariff.ZZ1_ZZF_NKTaxOrFeeCode);
			});
		}

		void AssertTariffUOM(ZGuid tariffPK, ZString uomType, ZString uomValue)
		{
			var uomLoader = new TariffUOMView.Loader(Factory);
			var uom = uomLoader.LoadTariffUomView(tariffPK, uomType, false);
			CombineAssertions("AssertTariffUOM", () =>
			{
				AssertEquals("IsInDatabase", true, uom.IsInDatabase);
				AssertEquals("ZZ8_UOM", uomValue, uom.ZZ8_UOM);
			});
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

		static ZString GetExceedMaxLengthError(ZString fieldName, ZString fieldValue, ZInt maxLength) => $"Specified argument was out of the range of valid values.\r\nParameter name: Value of {fieldName} exceeds the max length({maxLength}): {fieldValue}";

		static ZString GetInvalidDateFormatError(ZString fieldName, ZString fieldValue) => $"Specified argument was out of the range of valid values.\r\nParameter name: Invalid Date format for {fieldValue}, cannot save to {fieldName}. The expected format is 'yyyyMMdd'.";

		protected override void SetUp()
		{
			base.SetUp();
			loader = new TariffViewDataLoad();
		}

		TariffViewDataLoad loader;
	}
}

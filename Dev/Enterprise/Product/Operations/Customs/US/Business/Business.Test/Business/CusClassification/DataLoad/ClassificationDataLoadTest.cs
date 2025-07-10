using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ClassificationDataLoad))]
	sealed class ClassificationDataLoadTest : DataLoadTestCase<ClassificationDataLoad>
	{
		[ExpectException(typeof(FileNotFoundException))]
		public void TestValidationOfFile()
		{
			loader.ImportClassificationData("non-existant file");
		}

		public void TestValidationOfHeader()
		{
			using (TempFile testFileName = TempFile.New())
			{
				File.WriteAllText(testFileName.Filename, "Invalid Header Info");
				loader.ImportClassificationData(testFileName.Filename);
				AssertEquals(1, loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(0, loader.RunCounters.RecsCreated);
				AssertEquals(0, loader.RunCounters.RecsUpdated);
				AssertEquals(0, loader.RunCounters.RecsExcluded);
				AssertEquals(4, loader.Log.Count);
				AssertEquals("FileHeaderIsValid", false, loader.FileHeaderIsValid);
			}
		}

		public void TestValidationOfContent()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("CODE,TYPE,DESCRIPTION,TARIFF");
					sw.WriteLine("This is invalid data.");
				}

				loader.ImportClassificationData(testFileName.Filename);
				AssertEquals(2, loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(0, loader.RunCounters.RecsCreated);
				AssertEquals(0, loader.RunCounters.RecsUpdated);
				AssertEquals(1, loader.RunCounters.RecsExcluded);
				AssertEquals(3, loader.Log.Count);
			}
		}

		public void TestImportExcelClassifications()
		{
			ClearClassificationRecordsBeforeTesting();
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("CODE,TYPE,DESCRIPTION,TARIFF");
					sw.WriteLine("TEST,IMP,Test Lookup,6205900054");
					sw.WriteLine("Sports Shoes,IMP,Tennis shoes,6205900054");
					sw.WriteLine("Shoes,EXP,Leather Tennis shoes,1703900000");
					sw.WriteLine("Lookup with no desc,EXP,,1703900000");
					sw.WriteLine("Lookup with no tariff,EXP,No Tariff Description,");
				}

				var helper = new UniversalReferenceTestDataHelper(Factory);
				var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.ScheduleB);
				Factory.Save();
				var expTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "1703900000", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "DB Desc");
				Factory.Save();
				loader.ImportClassificationData(testFileName.Filename);
				ZQuery checkFilter = new ZQuery(CusClassificationSchema.CC_LookupCode, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterpriseClassificationsCreated = Factory.Load(typeof(CusClassification), checkFilter);
				AssertEquals("There should have been 4 Classification records created", 4, enterpriseClassificationsCreated.Length);
				AssertEquals(6, loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(4, loader.RunCounters.RecsCreated);
				AssertEquals(0, loader.RunCounters.RecsUpdated);
				AssertEquals(1, loader.RunCounters.RecsExcluded);
				AssertEquals(3, loader.Log.Count);
				CusClassification testClass = LoadClassification("Shoes");
				AssertEquals(Enterprise.Core.Constants.CountryCodes.UnitedStates, testClass.CC_RN_NKCountryCode);
				AssertEquals("Classification Description", "Leather Tennis shoes", testClass.CC_Description);
				AssertEquals("Tariff Number", "1703900000", testClass.CC_TariffNum);
				AssertEquals("Class. Type", CusClassification.ClassificationType.EXP, testClass.CC_ClassificationType);
				Assert("Add Info", testClass.CC_AddInfo.IsEmpty);
				Assert(testClass.CC_IsActive);
				testClass = LoadClassification("Sports Shoes");
				AssertEquals(Enterprise.Core.Constants.CountryCodes.UnitedStates, testClass.CC_RN_NKCountryCode);
				AssertEquals("Classification Description", "Tennis shoes", testClass.CC_Description);
				AssertEquals("Tariff Number", "6205900054", testClass.CC_TariffNum);
				AssertEquals("Class. Type", CusClassification.ClassificationType.IMP, testClass.CC_ClassificationType);
				Assert("Add Info", testClass.CC_AddInfo.IsEmpty);
				Assert(testClass.CC_IsActive);
				testClass = LoadClassification("Lookup with no desc");
				AssertEquals(Enterprise.Core.Constants.CountryCodes.UnitedStates, testClass.CC_RN_NKCountryCode);
				var tariff = new Universal.TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.UnitedStates, "1703900000", ZDateTime.Today);
				AssertEquals("Classification Description", tariff.ZZ1_Description.Left(testClass.CC_DescriptionInfo.MaxLength), testClass.CC_Description);
				AssertEquals("Tariff Number", "1703900000", testClass.CC_TariffNum);
				AssertEquals("Class. Type", CusClassification.ClassificationType.EXP, testClass.CC_ClassificationType);
				Assert("Add Info", testClass.CC_AddInfo.IsEmpty);
				Assert(testClass.CC_IsActive);
			}
		}

		public void TestClassificationIsNotCreatedWhenItAlreadyExists()
		{
			ClearClassificationRecordsBeforeTesting();
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("CODE,TYPE,DESCRIPTION,TARIFF");
					sw.WriteLine("TEST,IMP,Test Lookup,8529900100");
					sw.WriteLine("TEST,IMP,Test Lookup,8529900100");
					sw.WriteLine("TEST,IMP,Test Lookup,8529900100");
				}

				loader.ImportClassificationData(testFileName.Filename);
				ZQuery checkFilter = new ZQuery(CusClassificationSchema.CC_LookupCode, "TEST");
				BusinessObject[] enterpriseClassificationsCreated = Factory.Load(typeof(CusClassification), checkFilter);
				AssertEquals("There should have only been 1 Classification record created", 1, enterpriseClassificationsCreated.Length);
				AssertEquals(4, loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(1, loader.RunCounters.RecsCreated);
				AssertEquals(0, loader.RunCounters.RecsUpdated);
				AssertEquals(2, loader.RunCounters.RecsExcluded);
				AssertEquals(4, loader.Log.Count);
				Assert(loader.Log.Any(entry => entry == "Row 3: Record Excluded - Classification 'TEST' already exists"));
			}
		}

		public void TestClassificationIsNotCreatedWhenDataHasError()
		{
			ClearClassificationRecordsBeforeTesting();
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("CODE,TYPE,DESCRIPTION,TARIFF");
					sw.WriteLine("TEST,IMP,Test Lookup,AAAA");
				}

				AssertInvalidDataIsNotLoaded(testFileName.Filename);
			}
		}

		public void TestClassificationTypeDoesNotExceedMaxLength()
		{
			ClearClassificationRecordsBeforeTesting();
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("CODE,TYPE,DESCRIPTION,TARIFF");
					sw.WriteLine("TEST,BADTYPE,Test Lookup,8529900100");
				}

				loader.ImportClassificationData(testFileName.Filename);
				Assert(loader.Log.Any(entry => entry == "Row 2: Record Excluded - Invalid classification type"));
			}
		}

		protected override ClassificationDataLoad GetNewDataLoader() => new ClassificationDataLoad();

		protected override void SetUp()
		{
			base.SetUp();
			loader = new ClassificationDataLoad();
		}

		ClassificationDataLoad loader;

		void ClearClassificationRecordsBeforeTesting()
		{
			TestCaseHelper.ClearTable("JobComInvoiceLine");
			TestCaseHelper.ClearTable("CusClassPartPivot");
			TestCaseHelper.ClearTable("CusClassification");
		}

		CusClassification LoadClassification(ZString lookupCode)
		{
			ZQuery filter = new ZQuery(CusClassificationSchema.CC_LookupCode, lookupCode);
			CusClassification testClassification = Factory.LoadTop1<CusClassification>(filter);
			AssertNotNull("Expecting Classification " + lookupCode + " to be found", testClassification);
			return testClassification;
		}

		void AssertInvalidDataIsNotLoaded(string testFilePath)
		{
			loader.ImportClassificationData(testFilePath);
			ZQuery checkFilter = new ZQuery(CusClassificationSchema.CC_LookupCode, SQLComparisonOperator.GreaterThan, "");
			BusinessObject[] enterpriseClassificationsCreated = Factory.Load(typeof(CusClassification), checkFilter);
			AssertEquals("There should have been no Classification records created", 0, enterpriseClassificationsCreated.Length);
			AssertEquals(2, loader.RunCounters.RecordsToImportPlusHeader);
			AssertEquals(0, loader.RunCounters.RecsCreated);
			AssertEquals(0, loader.RunCounters.RecsUpdated);
			AssertEquals(1, loader.RunCounters.RecsExcluded);
			AssertEquals(3, loader.Log.Count);
			Assert(loader.Log.Any(entry => entry == "Row 2: Please enter a Tariff Number.")); // straight from the validation of CC_Tariff.
		}
	}
}

using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.MasterFiles.Testing
{
	using System.Linq;
	using CargoWise.BrandManager;
	using Enterprise.MasterFiles.Business.Testing;
	using NUnit.Framework;

	[TestedType(typeof(ClassificationDataLoad))]
	public class ClassificationDataLoadTest : DataLoadTestCase<ClassificationDataLoad>
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
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Invalid Header Info");
					sw.Flush();
				}

				loader.ImportClassificationData(testFileName.Filename);
				AssertEquals(1, loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(0, loader.RunCounters.RecsCreated);
				AssertEquals(0, loader.RunCounters.RecsUpdated);
				AssertEquals(0, loader.RunCounters.RecsExcluded);
				AssertEquals(3, loader.Log.Count);
				AssertEquals("FileHeaderIsValid", false, loader.FileHeaderIsValid);
			}
		}

		public void TestValidationOfContent()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					OutputHeaderLine(sw);
					sw.WriteLine("This is invalid data.");
					sw.Flush();
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
					OutputHeaderLine(sw);
					sw.WriteLine("Lookup with concession,LAMINATING POUCH FILM,3926.10.90.19B,984581L,,,,,,,,,");
					sw.WriteLine("PVC SHEETING,RIGID PVC SHEETING NOT EXC 1MM,3920.43.00.11C,740403E,,,,,,,,,");
					sw.WriteLine("FOUR,OVERHEAD TRANSPARENCY FILM (OHP FILM),3926.10.90.19B,504017F,,,,,,,,,");
					sw.WriteLine("FIVE,SYNTHETIC STITCHBONDED DYED FABRIC SAMPLES CARDS (FABRICS) FOR CLIENTS,6005.32.11.00G,,,,,,,,,,");
					sw.Flush();
				}

				loader.ImportClassificationData(testFileName.Filename);
				ZQuery checkFilter = new ZQuery(CusClassificationSchema.CC_LookupCode, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterpriseClassificationsCreated = Factory.Load(typeof(CusClassification), checkFilter);
				AssertEquals("There should have been 4 Classification records created", 4, enterpriseClassificationsCreated.Length);
				AssertEquals(5, loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(4, loader.RunCounters.RecsCreated);
				AssertEquals(0, loader.RunCounters.RecsUpdated);
				AssertEquals(0, loader.RunCounters.RecsExcluded);
				AssertEquals(2, loader.Log.Count);

				CusClassification testClass = LoadClassification("Lookup with concession");
				AssertEquals(Enterprise.Core.Constants.CountryCodes.NewZealand, testClass.CC_RN_NKCountryCode);
				AssertEquals("Classification Description", "LAMINATING POUCH FILM", testClass.CC_Description);
				AssertEquals("Tariff Number", "3926.10.90.19B", testClass.CC_TariffNum);
				AssertEquals("Class. Type", "BTH", testClass.CC_ClassificationType);
				AssertEquals("Concession", "984581L", testClass.CC_ConcessionCode);
				Assert(testClass.CC_IsActive);

				testClass = LoadClassification("PVC SHEETING");
				AssertEquals(Enterprise.Core.Constants.CountryCodes.NewZealand, testClass.CC_RN_NKCountryCode);
				AssertEquals("Classification Description", "RIGID PVC SHEETING NOT EXC 1MM", testClass.CC_Description);
				AssertEquals("Tariff Number", "3920.43.00.11C", testClass.CC_TariffNum);
				AssertEquals("Class. Type", "BTH", testClass.CC_ClassificationType);
				AssertEquals("Concession", "740403E", testClass.CC_ConcessionCode);
				Assert(testClass.CC_IsActive);
			}
		}

		public void TestImportClassificationsWithInconsistentData()
		{
			ClearClassificationRecordsBeforeTesting();
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					OutputHeaderLine(sw);
					sw.WriteLine("FOUR,OVERHEAD TRANSPARENCY FILM (OHP FILM),3926.10.90.19B,504017F");
					sw.WriteLine("FIVE,SYNTHETIC STITCHBONDED DYED FABRIC SAMPLES CARDS (FABRICS) FOR CLIENTS,6005.32.11.00G,,,");
					sw.Flush();
				}

				loader.ImportClassificationData(testFileName.Filename);
				ZQuery checkFilter = new ZQuery(CusClassificationSchema.CC_LookupCode, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterpriseClassificationsCreated = Factory.Load(typeof(CusClassification), checkFilter);
				AssertEquals("There should have been 2 Classification record created", 2, enterpriseClassificationsCreated.Length);
				AssertEquals(3, loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(2, loader.RunCounters.RecsCreated);
				AssertEquals(0, loader.RunCounters.RecsUpdated);
				AssertEquals(0, loader.RunCounters.RecsExcluded);
				AssertEquals(2, loader.Log.Count);
			}
		}

		public void TestClassificationIsNotCreatedWhenItAlreadyExists()
		{
			ClearClassificationRecordsBeforeTesting();
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					OutputHeaderLine(sw);
					sw.WriteLine("PVC SHEETING,RIGID PVC SHEETING NOT EXC 1MM,3920.43.00.11C,740403E,,,,,,,,,");
					sw.WriteLine("PVC SHEETING,RIGID PVC SHEETING NOT EXC 1MM,3920.43.00.11C,740403E,,,,,,,,,");
					sw.WriteLine("PVC SHEETING,RIGID PVC SHEETING NOT EXC 1MM,3920.43.00.11C,740403E,,,,,,,,,");
					sw.Flush();
				}

				loader.ImportClassificationData(testFileName.Filename);
				ZQuery checkFilter = new ZQuery(CusClassificationSchema.CC_LookupCode, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterpriseClassificationsCreated = Factory.Load(typeof(CusClassification), checkFilter);
				AssertEquals("There should have only been 1 Classification record created", 1, enterpriseClassificationsCreated.Length);
				AssertEquals(4, loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(1, loader.RunCounters.RecsCreated);
				AssertEquals(0, loader.RunCounters.RecsUpdated);
				AssertEquals(2, loader.RunCounters.RecsExcluded);
				AssertEquals(4, loader.Log.Count);
				Assert(loader.Log.Any(entry => entry == $"Row 3: Record Excluded - Classification 'PVC SHEETING' already exists in {BrandingFactory.Instance.ProductName}"));
			}
		}

		public void TestClassificationIsNotCreatedWhenDataHasError()
		{
			ClearClassificationRecordsBeforeTesting();
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					OutputHeaderLine(sw);
					sw.WriteLine("TEST,IMP,Test Lookup,AAAA,,,");
					sw.Flush();
				}

				AssertInvalidDataIsNotLoaded(testFileName.Filename);
			}
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
			Assert(loader.Log.Any(entry => entry == $"Row 2: Record Excluded - Tariff values provided are not valid in {BrandingFactory.Instance.ProductName}"));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.NewZealand);
			loader = new ClassificationDataLoad();
		}

		protected override ClassificationDataLoad GetNewDataLoader()
		{
			return new ClassificationDataLoad();
		}

		protected ClassificationDataLoad loader;

		protected void ClearClassificationRecordsBeforeTesting()
		{
			TestCaseHelper.ClearTable("CusClassification");
		}

		protected CusClassification LoadClassification(ZString lookupCode)
		{
			ZQuery filter = new ZQuery(CusClassificationSchema.CC_LookupCode, lookupCode);
			var testClassification = Factory.LoadTop1<CusClassification>(filter);
			AssertNotNull("Expecting Classification " + lookupCode + " to be found", testClassification);

			return testClassification;
		}

		void OutputHeaderLine(StreamWriter sw)
		{
			sw.WriteLine("Code,Description,Tariff,Concession,PartsOfTariff,PermitCode1,PermitNo1,PermitCode2,PermitNo2,PermitCode3,PermitNo3,ProhibitedCode1,ProhibitedCode2");
		}

		#endregion
	}
}

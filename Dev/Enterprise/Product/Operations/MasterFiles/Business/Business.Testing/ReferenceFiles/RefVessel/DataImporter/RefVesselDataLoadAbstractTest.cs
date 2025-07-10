using System.IO;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestsSubclassesOf(typeof(RefVesselDataLoad))]
	public abstract class RefVesselDataLoadAbstractTest<T> : DataLoadTestCase<T>
		where T : RefVesselDataLoad, new()
	{
		[ExpectException(typeof(FileNotFoundException))]
		public void TestValidationOfFile()
		{
			loader.ImportVesselData("non-existant file");
		}

		public void TestValidationOfHeader()
		{
			ClearVesselRecordsBeforeTesting();
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(GetWrongHeader());
				}

				loader.ImportVesselData(testFileName.Filename);
				AssertEquals(1, loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(0, loader.RunCounters.RecsCreated);
				AssertEquals(0, loader.RunCounters.RecsUpdated);
				AssertEquals(0, loader.RunCounters.RecsExcluded);
				AssertEquals(3, loader.Log.Count);
				AssertEquals("FileHeaderIsValid", false, loader.FileHeaderIsValid);
			}
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(GetCorrectHeader());
					sw.WriteLine(GetValidData());
					sw.Flush();
				}

				loader.ImportVesselData(testFileName.Filename);
				AssertEquals(2, loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(1, loader.RunCounters.RecsCreated);
				AssertEquals(1, loader.RunCounters.RecsUpdated);
				AssertEquals(0, loader.RunCounters.RecsExcluded);
				AssertEquals(5, loader.Log.Count);
				AssertEquals("FileHeaderIsValid", true, loader.FileHeaderIsValid);
			}
		}

		public void TestValidationOfContent()
		{
			ClearVesselRecordsBeforeTesting();
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(GetCorrectHeader());
					sw.WriteLine(GetInvalidData());
				}

				loader.ImportVesselData(testFileName.Filename);
				AssertEquals(2, loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(0, loader.RunCounters.RecsCreated);
				AssertEquals(0, loader.RunCounters.RecsUpdated);
				AssertEquals(1, loader.RunCounters.RecsExcluded);
				AssertEquals(3, loader.Log.Count);
			}
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(GetCorrectHeader());
					sw.WriteLine(GetValidData());
				}

				loader.ImportVesselData(testFileName.Filename);
				AssertEquals(2, loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(1, loader.RunCounters.RecsCreated);
				AssertEquals(1, loader.RunCounters.RecsUpdated);
				AssertEquals(0, loader.RunCounters.RecsExcluded);
				AssertEquals(5, loader.Log.Count);
			}
		}

		public void TestImportVesselsWithInconsistentData()
		{
			ClearVesselRecordsBeforeTesting();
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(GetCorrectHeader());
					sw.WriteLine(GetValidData());
					sw.WriteLine(GetInvalidData());
					sw.Flush();
				}

				loader.ImportVesselData(testFileName.Filename);
				AssertEquals("There should have been 1 vessel records created", 1, Factory.GetDatabaseCount(typeof(RefVessel)));
				AssertEquals(3, loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(1, loader.RunCounters.RecsCreated);
				AssertEquals(1, loader.RunCounters.RecsUpdated);
				AssertEquals(1, loader.RunCounters.RecsExcluded);
				AssertEquals(3, loader.Log.Count);
			}
		}

		public void TestVesselIsNotCreatedWhenItAlreadyExists()
		{
			ClearVesselRecordsBeforeTesting();
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(GetCorrectHeader());
					sw.WriteLine(GetValidData());
					sw.WriteLine(GetValidData());
					sw.WriteLine(GetValidData());
					sw.Flush();
				}

				loader.ImportVesselData(testFileName.Filename);
				AssertEquals("There should have been 1 vessel records created", 1, Factory.GetDatabaseCount(typeof(RefVessel)));
				AssertEquals(4, loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(1, loader.RunCounters.RecsCreated);
				AssertEquals(3, loader.RunCounters.RecsUpdated);
				AssertEquals(0, loader.RunCounters.RecsExcluded);
				AssertEquals(2, loader.Log.Count);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			loader = GetNewDataLoader();
		}

		protected T loader;

		protected sealed override T GetNewDataLoader() => new T();

		protected void ClearVesselRecordsBeforeTesting()
		{
			TestCaseHelper.ClearTable(RefVesselSchema.Constants.TableName);
		}

		protected virtual string GetWrongHeader() => "Wrong Header Info";

		protected virtual string GetCorrectHeader() => "NAME,COUNTRYOFREGO,LLOYDSID";

		protected virtual string GetInvalidData() => ",,,";

		protected virtual string GetValidData() => "1, 2, 3";
	}
}

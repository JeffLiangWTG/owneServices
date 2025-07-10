using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DataLoadBaseOnlyTest : TestCaseWithFactory
	{
		public void TestInvalidHeaderAndCSVTemplate()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Header line");
					sw.Flush();
				}
				testDataLoad.ImportData(testFileName.Filename, "TestingType");
				AssertEquals(1, testDataLoad.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(0, testDataLoad.RunCounters.RecsCreated);
				AssertEquals(0, testDataLoad.RunCounters.RecsUpdated);
				AssertEquals(0, testDataLoad.RunCounters.RecsExcluded);
				AssertEquals("FileHeaderIsValid", false, testDataLoad.FileHeaderIsValid);
			}
		}

		public void TestSetupBeforeImport()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("TEST1,TEST2,TEST3");
					sw.Flush();
				}
				testDataLoad.ImportData(testFileName.Filename, "TestingType");
			}
			AssertEquals(Factory.LoadFromNaturalKey(typeof(RefVessel), RefVesselSchema.RV_Code, "ADMIRALENGRACHT").PK, testDataLoad.VesselGuid);
		}

		public void TestRecsToImportAndCurrentRow()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("TEST1,TEST2,TEST3");
					sw.WriteLine("A,B,C");
					sw.WriteLine("A,B,C");
					sw.WriteLine("A,B,C");
					sw.Flush();
				}
				testDataLoad.ImportData(testFileName.Filename, "TestingType");
				AssertEquals(4, testDataLoad.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(0, testDataLoad.RunCounters.RecsCreated);
				AssertEquals(0, testDataLoad.RunCounters.RecsUpdated);
				AssertEquals(0, testDataLoad.RunCounters.RecsExcluded);
				AssertEquals(4, testDataLoad.RunCounters.CurrentRow);
			}
		}

		public void TestUmlauteWordsWorkOK()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("TEST1,TEST2,TEST3");
					sw.WriteLine("A,Rückwand,C");
					sw.Flush();
				}
				testDataLoad.ImportData(testFileName.Filename, "TestingType");
				AssertEquals(2, testDataLoad.RunCounters.RecordsToImportPlusHeader);

				List<string> result = testDataLoad.GetResult();
				AssertEquals(1, result.Count);

				byte[] bytes = System.Text.Encoding.Default.GetBytes(result[0]);
				result[0] = System.Text.Encoding.UTF8.GetString(bytes);
				AssertEquals("A,Rückwand,C", result[0]);
			}
		}

		public void TestEmbeddedLineBreaksOK()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("TEST1,TEST2,TEST3");
					sw.WriteLine("\"col1\",\"col2\",\"col3\"");
					sw.WriteLine("\"col1,row1" + System.Environment.NewLine + "col1,row2\",\"col2\",\"col3\"");
					sw.WriteLine("\"col1,row1" + '\n' + "col1,row2\",\"col2\",\"col3\"");
					sw.WriteLine("\"col1\",\"col2\",\"col3\"");
					sw.Flush();
				}
				testDataLoad.ImportData(testFileName.Filename, "TestingType");
				AssertEquals(5, testDataLoad.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(0, testDataLoad.RunCounters.RecsCreated);
				AssertEquals(0, testDataLoad.RunCounters.RecsUpdated);
				AssertEquals(0, testDataLoad.RunCounters.RecsExcluded);
				AssertEquals(5, testDataLoad.RunCounters.CurrentRow);
			}

			List<string> result = testDataLoad.GetResult();
			AssertEquals(4, result.Count);
			AssertEquals("\"col1\",\"col2\",\"col3\"", result[0]);
			AssertEquals("\"col1,row1" + System.Environment.NewLine + "col1,row2\",\"col2\",\"col3\"", result[1]);
			AssertEquals("\"col1,row1" + System.Environment.NewLine + "col1,row2\",\"col2\",\"col3\"", result[2]);
			AssertEquals("\"col1\",\"col2\",\"col3\"", result[3]);
		}

		public void TestLoadOK()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("TEST1,TEST2,TEST3");
					sw.WriteLine("Data line");
					sw.Flush();
				}
				testDataLoad.ImportData(testFileName.Filename, "TestingType");
				AssertEquals(2, testDataLoad.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(0, testDataLoad.RunCounters.RecsCreated);
				AssertEquals(0, testDataLoad.RunCounters.RecsUpdated);
				AssertEquals(0, testDataLoad.RunCounters.RecsExcluded);
				AssertEquals(2, testDataLoad.RunCounters.CurrentRow);
			}
		}

		public void TestUpdateAndDisplayIfRequiredRecsToUpdate()
		{
			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "TestVessel";
			testDataLoad.RunCounters.RecsToUpdate = 1;
			AssertEquals(1, ((IBusinessObjectFactoryInternals)Factory).AllBusinessObjects.Count);
			testDataLoad.UpdateAndDisplayIfRequiredExposed(vessel.PK.ToGuid(), RefVesselSchema.Constants.TableName);
			AssertEquals(0, testDataLoad.RunCounters.RecsToUpdate);
		}

		public void TestUpdateAndDisplayIfRequiredRecsExcluded()
		{
			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "TestVessel2";
			AssertEquals(1, ((IBusinessObjectFactoryInternals)Factory).AllBusinessObjects.Count);
			testDataLoad.RunCounters.RecsExcluded = 500;
			testDataLoad.UpdateAndDisplayIfRequiredExposed(vessel.PK.ToGuid(), RefVesselSchema.Constants.TableName);
			AssertEquals(500, testDataLoad.RunCounters.RecsExcluded);
		}

		public void TestOutputFinalTotals()
		{
			var newTestDataLoad = new DataLoadForTest();
			newTestDataLoad.RunCounters.RecsCreated = 2;
			newTestDataLoad.RunCounters.RecsUpdated = 5;
			newTestDataLoad.RunCounters.RecsExcluded = 7;
			newTestDataLoad.OutputFinalTotalsExposed("TestDataType");
			AssertEquals(1, newTestDataLoad.Log.Count);
			AssertEquals(System.Environment.NewLine + "T O T A L : TestDataTypes created = 2, TestDataTypes updated = 5, TestDataTypes excluded = 7" + System.Environment.NewLine, newTestDataLoad.Log[0]);
		}

		public void TestUserMessages_InvalidFileHeader()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Header line");
					sw.Flush();
				}

				testDataLoad.InvalidFileHeaderProcessed += (sender, e) => testDataLoad.DisplayLogMessage("Please check the format in the template file copied to the clipboard, and also available for Download above this message.");
				testDataLoad.ImportData(testFileName.Filename, "TestDataType");
				AssertEquals("TestDataTypes to Import = 0", testDataLoad.Log[0]);
				AssertEquals("File Header information is incorrect. The import of TestDataType data requires a specific .CSV format file.", testDataLoad.Log[1]);
				AssertEquals("Please check the format in the template file copied to the clipboard, and also available for Download above this message.", testDataLoad.Log[2]);
				AssertEquals("FileHeaderIsValid", false, testDataLoad.FileHeaderIsValid);
			}
		}

		public void TestParseHeaderOK()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("TEST1,TEST2,TEST1");
					sw.WriteLine("Data line");
					sw.Flush();
				}
				MockDataLoadWithFlexibleColumns mockDataLoad = new MockDataLoadWithFlexibleColumns();
				mockDataLoad.ImportData(testFileName.Filename, "TestingType");
				AssertEquals("Header line parsing failed", true, mockDataLoad.Log.Contains("Duplicate column detected : columnHeading"));
				TempFile.Delete(testFileName.Filename);
			}
		}

		public void TestWriteCSVTemplate()
		{
			var fileName = Env.GetTempFileName();
			using (var stream = File.OpenWrite(fileName))
			{
				testDataLoad.WriteCSVTemplate(stream);
			}

			Assert("File should exist", File.Exists(fileName));
			try
			{
				using (StreamReader sr = new StreamReader(fileName, System.Text.Encoding.GetEncoding(950), true))
				{
					sr.Read();
					AssertEquals("utf-8", sr.CurrentEncoding.BodyName);
				}
				AssertMultilineASCIIEquals("File Text", "TEST1,TEST2,TEST3", File.ReadAllText(fileName));
			}
			finally
			{
				File.Delete(fileName);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			testDataLoad = new DataLoadForTest();
		}

		DataLoadForTest testDataLoad;

		sealed class MockDataLoadWithFlexibleColumns : DataLoad
		{
			protected override void ParseHeaderLine(OCsvLine line)
			{
				throw new ArgumentException("Duplicate column detected : columnHeading");
			}

			protected override bool IsFileHeaderValid(OCsvLine line)
			{
				throw new NotImplementedException();
			}

			public override string CSVTemplateHeading
			{
				get { throw new NotImplementedException(); }
			}

			protected override void ProcessDataForThisLine(OCsvLine line)
			{
				throw new NotImplementedException();
			}
		}
	}
}

using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefVesselDataLoad))]
	sealed class RefVesselDataLoadBaseOnlyTest : RefVesselDataLoadAbstractTest<RefVesselDataLoadForTest>
	{
		public void TestImportCSVwithNoLLOYDSID()
		{
			ClearVesselRecordsBeforeTesting();
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(GetCorrectHeader());
					sw.WriteLine("TestVessel, AU,");
					sw.WriteLine("TestVessel 2,,");
				}

				loader.ImportVesselData(testFileName.Filename);
				AssertEquals("There should have been 2 vessel records created", 2, Factory.GetDatabaseCount(typeof(RefVessel)));
				AssertEquals(3, loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(2, loader.RunCounters.RecsCreated);
				AssertEquals(2, loader.RunCounters.RecsUpdated);
				AssertEquals(0, loader.RunCounters.RecsExcluded);
				AssertEquals(2, loader.Log.Count);
			}
		}

		public void TestCheckVesselThatAlreadyExistsRecordIsUpdated()
		{
			ClearVesselRecordsBeforeTesting();
			var testVessel = Factory.NewWithValidTestData<RefVessel>();
			testVessel.RV_Code = "MSC SARISKA ZZ";
			testVessel.RV_LloydsNumber = "710778Z";
			var filter = new ZQuery(RefCountrySchema.RN_Code, "AU");
			var country = Factory.LoadTop1<RefCountry>(filter);
			AssertNotNull(country);
			testVessel.RV_RN_NKCountryOfReg = country.Code;
			Factory.Save();
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("NAME,COUNTRYOFREGO,LLOYDSID");
					sw.WriteLine("MSC SARISKA ZZ,,710779Z");
					sw.Flush();
				}

				loader.ImportVesselData(testFileName.Filename);
				var factory2 = new BusinessObjectFactory();
				var checkFilter = new ZQuery(RefVesselSchema.RV_LloydsNumber, SQLComparisonOperator.GreaterThan, "");
				var enterpriseVesselsCreated = factory2.Load(typeof(RefVessel), checkFilter);
				AssertEquals("There should only be 1 vessel record no extras created.", 1, enterpriseVesselsCreated.Length);
				var vessel = (RefVessel)enterpriseVesselsCreated[0];
				AssertEquals("710779Z", vessel.RV_LloydsNumber);
				AssertEquals("MSC SARISKA ZZ", vessel.RV_Code);
				AssertEquals(ZString.Empty, vessel.RV_RN_NKCountryOfReg);
				AssertEquals(2, loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(0, loader.RunCounters.RecsCreated);
				AssertEquals(1, loader.RunCounters.RecsUpdated);
				AssertEquals(0, loader.RunCounters.RecsExcluded);
				AssertEquals(2, loader.Log.Count);
			}
		}

		public void TestProcessDataForThisLineUserMessages()
		{
			var newLoader = GetNewDataLoader();
			newLoader.RunCounters.CurrentRow = 99;
			newLoader.ProcessDataForThisLineExposed(new OCsvLine("Test Data"));
			AssertEquals(1, newLoader.Log.Count);
			AssertEquals("File contains inconsistent data. Row 99 contains the following data : Test Data", newLoader.Log[0]);
		}
	}

	sealed class RefVesselDataLoadForTest : RefVesselDataLoad
	{
		internal void ProcessDataForThisLineExposed(OCsvLine line) => ProcessDataForThisLine(line);
	}
}

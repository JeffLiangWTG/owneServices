using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(RefVesselDataLoad))]
	sealed class ZARefVesselDataLoadTest : MasterFiles.Business.Testing.RefVesselDataLoadAbstractTest<RefVesselDataLoad>
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
				AssertEquals("There should have been 2 vessel records created", 1, Factory.GetDatabaseCount(typeof(RefVessel)));
				AssertEquals(3, loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(1, loader.RunCounters.RecsCreated);
				AssertEquals(1, loader.RunCounters.RecsUpdated);
				AssertEquals(1, loader.RunCounters.RecsExcluded);
				AssertEquals(3, loader.Log.Count);
			}
		}

		public void TestCheckVesselThatAlreadyExistsRecordIsUpdated()
		{
			ClearVesselRecordsBeforeTesting();
			var testVessel = Factory.NewWithValidTestData<RefVessel>();
			testVessel.RV_Code = "MSC SARISKA ZZ";
			testVessel.RV_CarrierCode = "7108";
			testVessel.RV_RadioCallSign = "XX";
			Factory.Save();
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(GetCorrectHeader());
					sw.WriteLine("Radio,MSC SARISKA ZZ,7107,Carrier Name");
					sw.Flush();
				}

				loader.ImportVesselData(testFileName.Filename);
				var factory2 = new BusinessObjectFactory();
				var enterpriseVesselsCreated = factory2.Load(typeof(RefVessel), new ZQuery());
				AssertEquals("There should only be 1 vessel record no extras created.", 1, enterpriseVesselsCreated.Length);
				var vessel = (RefVessel)enterpriseVesselsCreated[0];
				AssertEquals("MSC SARISKA ZZ", vessel.RV_Code);
				AssertEquals("7107", vessel.RV_CarrierCode);
				AssertEquals("Radio", vessel.RV_RadioCallSign);
				AssertEquals(2, loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(0, loader.RunCounters.RecsCreated);
				AssertEquals(1, loader.RunCounters.RecsUpdated);
				AssertEquals(0, loader.RunCounters.RecsExcluded);
				AssertEquals(2, loader.Log.Count);
			}
		}

		public void TestHeaderIsValidWithEitherOfTwoIDColumnNames()
		{
			var vesselLoad = new RefVesselDataLoadForTest();
			AssertEquals(true, vesselLoad.IsFileHeaderValid(new OCsvLine("IDOFMEANSOFTRANSPORT,NAME,CARRIERCODE,CARRIERNAME")));
			AssertEquals(true, vesselLoad.IsFileHeaderValid(new OCsvLine("TRANSPORTID,NAME,CARRIERCODE,CARRIERNAME")));
			AssertEquals(true, vesselLoad.IsFileHeaderValid(new OCsvLine("idofmeansoftransport,name,carriercode,carriername")));
			AssertEquals(true, vesselLoad.IsFileHeaderValid(new OCsvLine("transportid,name,carriercode,carriername")));
			AssertEquals(false, vesselLoad.IsFileHeaderValid(new OCsvLine("SOMETHING,NAME,CARRIERCODE,CARRIERNAME")));
		}

		protected override string GetWrongHeader() => "Invalid Header Info";

		protected override string GetCorrectHeader() => "IDOFMEANSOFTRANSPORT,NAME,CARRIERCODE,CARRIERNAME";

		protected override string GetValidData() => "1, 2, 3, 4";

		protected override string GetInvalidData() => "Invalid Data,,";

		sealed class RefVesselDataLoadForTest : RefVesselDataLoad
		{
			public RefVesselDataLoadForTest()
			{
			}

			public new bool IsFileHeaderValid(OCsvLine line) => base.IsFileHeaderValid(line);
		}
	}
}

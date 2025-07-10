using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EDICodeMappingDataLoader))]
	sealed class EDICodeMappingDataLoadTest : DataLoadTestCase<EDICodeMappingDataLoader>
	{
		[ExpectException(typeof(FileNotFoundException))]
		public void TestValidationOfFile()
		{
			Loader.ImportData("non-existant file", "EDI Code Mapping");
		}

		public void TestValidationOfContent()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("This is the header for the file.");
					sw.WriteLine("This is the first part - invalid text");
				}
				Loader.ImportData(testFileName.Filename, "EDI Code Mapping");
				AssertEquals(2, Loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(0, Loader.RunCounters.RecsCreated);
				AssertEquals(0, Loader.RunCounters.RecsUpdated);
				AssertEquals(0, Loader.RunCounters.RecsExcluded);
				AssertEquals(4, Loader.Log.Count);
				AssertEquals("FileHeaderIsValid", false, Loader.FileHeaderIsValid);
			}
		}

		public void TestImportCSVWithInvalidPackageType()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					// Add some text to the file. This test file will have the header line + 2 lines of data, the rest should error.
					sw.WriteLine("ForeignCode,Relationship,CargoWiseOneCode");
					sw.WriteLine("GDS-CNT,PKG,CNT");
					sw.Flush();
				}

				Loader.ImportData(testFileName.Filename, "EDI Code Mapping");
				Factory.Save();
				AssertNewMappings("0 records added", 0);
				AssertEquals("Total lines", 2, Loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals("Recs created", 0, Loader.RunCounters.RecsCreated);
				AssertEquals("Recs updatd", 0, Loader.RunCounters.RecsUpdated);
				AssertEquals("Recs excluded", 1, Loader.RunCounters.RecsExcluded);
			}
		}

		public void TestImportDodgieCSVMappings()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					// Add some text to the file. This test file will have the header line + 2 lines of data, the rest should error.
					sw.WriteLine("ForeignCode,Relationship,CargoWiseOneCode");
					sw.WriteLine("111,\"ORG\",\"" + TestOrg1.OH_Code + "\"");
					sw.WriteLine("222,\"ORG\",\"" + TestOrg1.OH_Code + "\"");
					sw.WriteLine("111,\"ORG\",\"" + OrgProxy.OH_Code + "\"");
					sw.WriteLine("123,,\"" + TestOrg1.OH_Code + "\"");
					sw.WriteLine("333,\"xxx\",\"" + TestOrg1.OH_Code + "\"");
					sw.WriteLine(",\"ORG\",\"" + TestOrg1.OH_Code + "\"");
					sw.WriteLine("444,\"ORG\",");
					sw.WriteLine("555,\"ORG\",\"" + OrgProxy.OH_Code + "\"");
					sw.Flush();
				}

				Loader.ImportData(testFileName.Filename, "EDI Code Mapping");

				CombineAssertions(delegate
				{
					AssertNewMappings("Only 3 records added", 3);
					AssertOrgMappingEquals("Mapped correctly", "111", "Org", TestOrg1.OH_Code);
					AssertOrgMappingEquals("Mapped correctly", "555", "Org", OrgProxy.OH_Code);

					var testLog = Loader.Log;
					AssertEquals("Total lines", 9, Loader.RunCounters.RecordsToImportPlusHeader);
					AssertEquals("Recs created", 3, Loader.RunCounters.RecsCreated);
					AssertEquals("Recs updatd", 0, Loader.RunCounters.RecsUpdated);
					AssertEquals("Recs excluded", 5, Loader.RunCounters.RecsExcluded);
					AssertEquals("Log count", 7, Loader.Log.Count);
				});
			}
		}

		public void TestImportCSVMappingsOfEachSupportedType()
		{
			var testContainer = Factory.LoadTop1<RefContainer>(new ZQuery());
			var testCountry = Factory.LoadTop1<RefCountry>(new ZQuery());
			var testCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery());
			var testUNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			var testPackageType = Factory.LoadTop1<RefPackType>(new ZQuery());
			var testServiceLevel = Factory.LoadTop1<RefServiceLevel>(new ZQuery());
			var testZone = Factory.LoadTop1<RefZoneHeader>(new ZQuery());
			var testChargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery());
			var testCommodity = Factory.LoadTop1<RefCommodityCode>(new ZQuery());
			var testEvent = Events.ArrivalCode;
			var testDropMode = new CombinedEquipmentNeededList().ToArray().First();
			var testIncoTerm = IncoTermRegistry.Keys.First();
			var testEquipment = Factory.NewWithValidTestData<RefEquipment>();

			OrgProxy.MiscServ.CarrierServiceLevels.RemoveAndDeleteAll();
			var testSvcLvl = OrgProxy.MiscServ.CarrierServiceLevels.AddNew();
			testSvcLvl.PL_Code = "LH1";
			testSvcLvl.PL_CarrierServiceLevelDescription = "LH40";
			Factory.Save();

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					// Add some text to the file. This test file will have the header line + 2 lines of data, the rest should error.
					sw.WriteLine("ForeignCode,Relationship,CargoWiseOneCode");
					sw.WriteLine("100,\"" + Constants.OrgPatternMatchOverrideRelationships.ContainerType + "\",\"" + testContainer.RC_Code + "\"");
					sw.WriteLine("200,\"" + Constants.OrgPatternMatchOverrideRelationships.Country + "\",\"" + testCountry.RN_Code + "\"");
					sw.WriteLine("300,\"" + Constants.OrgPatternMatchOverrideRelationships.Currency + "\",\"" + testCurrency.RX_Code + "\"");
					sw.WriteLine("400,\"" + Constants.OrgPatternMatchOverrideRelationships.Port + "\",\"" + testUNLOCO.RL_Code + "\"");
					sw.WriteLine("500,\"" + Constants.OrgPatternMatchOverrideRelationships.ChargeCodes + "\",\"" + testChargeCode.AC_Code + "\"");
					sw.WriteLine("600,\"" + Constants.OrgPatternMatchOverrideRelationships.EventCode + "\",\"" + testEvent + "\"");
					sw.WriteLine("700,\"" + Constants.OrgPatternMatchOverrideRelationships.IncoTerm + "\",\"" + testIncoTerm + "\"");
					sw.WriteLine("800,\"" + Constants.OrgPatternMatchOverrideRelationships.ServiceLevel + "\",\"" + testServiceLevel.RS_Code + "\"");
					sw.WriteLine("900,\"" + Constants.OrgPatternMatchOverrideRelationships.PackageType + "\",\"" + testPackageType.F3_Code + "\"");
					sw.WriteLine("1000,\"" + Constants.OrgPatternMatchOverrideRelationships.PackageType + "\",\"" + testPackageType.F3_Code + "\"");
					sw.WriteLine("1100,\"" + Constants.OrgPatternMatchOverrideRelationships.Commodities + "\",\"" + testCommodity.RH_Code + "\"");
					sw.WriteLine("1200,\"" + Constants.OrgPatternMatchOverrideRelationships.DropMode + "\",\"" + testDropMode + "\"");
					sw.WriteLine("1300,\"" + Constants.OrgPatternMatchOverrideRelationships.Equipment + "\",\"" + testEquipment.RQ_ShortCode + "\"");
					sw.WriteLine("1400,\"" + Constants.OrgPatternMatchOverrideRelationships.CarrierServiceLevel + "\", \"" + testSvcLvl.PL_Code + "\"");

					sw.Flush();
				}

				Loader.ImportData(testFileName.Filename, "EDI Code Mapping");

				CombineAssertions(delegate
				{
					AssertNewMappings("14 records added", 14);
					AssertEquals("Total lines", 15, Loader.RunCounters.RecordsToImportPlusHeader);
					AssertEquals("Recs created", 14, Loader.RunCounters.RecsCreated);
					AssertEquals("Recs excluded", 0, Loader.RunCounters.RecsExcluded);
				});
			}
		}

		public void TestImportCSVMappingsSavedToDatabase()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("ForeignCode,Relationship,CargoWiseOneCode");
					sw.WriteLine($"V-0000559,ORG,{TestOrg1.OH_Code}");
					sw.Flush();
				}

				Loader.ImportData(testFileName.Filename, "EDI Code Mapping");

				var patternMatchingOverride = Factory.Load<OrgPatternMatchOverride>(new ZQuery(OrgPatternMatchOverrideSchema.OO_OH, OrgProxy.PK)).SingleOrDefault();
				AssertNotNull(patternMatchingOverride);
				AssertEquals("Saved to database", true, patternMatchingOverride.IsInDatabase);
				AssertEquals("V-0000559", patternMatchingOverride.OO_ForeignCode);
				AssertEquals("ORG", patternMatchingOverride.OO_Relationship);
				AssertEquals(TestOrg1.PK, patternMatchingOverride.OO_LocalGuid);
			}
		}

		#region Implementation

		void AssertNewMappings(ZString message, int expectedRecords)
		{
			OrgPatternMatchOverride[] mapping = Factory.Load<OrgPatternMatchOverride>(new ZQuery());
			AssertEquals(message, expectedRecords, mapping.Length);
		}

		void AssertOrgMappingEquals(ZString message, ZString foreignCode, ZString relationship, ZString enterpriseCode)
		{
			OrgHeader org = Factory.LoadFromUniqueKey<OrgHeader>(OrgHeaderSchema.OH_Code, enterpriseCode);
			AssertNotNull(message + " enterprise org code exists", org);

			ZQuery filter = new ZQuery(OrgPatternMatchOverrideSchema.OO_ForeignCode, foreignCode);
			filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_Relationship, relationship);
			filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_LocalGuid, org.PK);
			OrgPatternMatchOverride[] mapping = Factory.Load<OrgPatternMatchOverride>(filter);
			AssertNotNull(message + " pattern found", mapping[0]);
			AssertEquals(message + " only 1 mapping found", 1, mapping.Length);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable("OrgPatternMatchOverride");
		}

		protected override EDICodeMappingDataLoader GetNewDataLoader()
		{
			return new EDICodeMappingDataLoader(OrgProxy);
		}

		EDICodeMappingDataLoader Loader
		{
			get { return loader ?? (loader = new EDICodeMappingDataLoader(OrgProxy)); }
		}
		EDICodeMappingDataLoader loader;

		OrgHeader[] TestOrgs
		{
			get { return testOrgs ?? (testOrgs = Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "JAC"))); }
		}
		OrgHeader[] testOrgs;

		OrgHeader OrgProxy
		{
			get { return orgProxy ?? (orgProxy = TestOrgs[0]); }
		}
		OrgHeader orgProxy;

		OrgHeader TestOrg1
		{
			get { return testOrg1 ?? (testOrg1 = TestOrgs[1]); }
		}
		OrgHeader testOrg1;

		#endregion
	}
}

using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class NonGenericVesselDataLoadTest : TestCaseWithFactory
	{
		public void TestImportExcelVessels()
		{
			ClearVesselRecordsBeforeTesting();
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("NAME,COUNTRYOFREGO,LLOYDSID");
					sw.WriteLine("MSC SARISKA ZZ,AU,710778Z");
					sw.WriteLine("MSC TERESA ZZ,,732025Z");
					sw.WriteLine("MSC FEDERICA ZZ,,734751Z");
					sw.Flush();
				}

				Loader.ImportVesselData(testFileName.Filename);
				ZQuery checkFilter = new ZQuery(RefVesselSchema.RV_LloydsNumber, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterpriseVesselsCreated = Factory.Load(typeof(RefVessel), checkFilter);
				AssertEquals("There should have been 3 vessel records created", 3, enterpriseVesselsCreated.Length);
				AssertEquals(4, Loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(3, Loader.RunCounters.RecsCreated);
				AssertEquals(3, Loader.RunCounters.RecsUpdated);
				AssertEquals(0, Loader.RunCounters.RecsExcluded);
				AssertEquals(2, Loader.Log.Count);

				RefVessel vessel = LoadVessel("710778Z");
				AssertEquals("MSC SARISKA ZZ", vessel.RV_Code);
				AssertEquals(Enterprise.Core.Constants.CountryCodes.Australia, vessel.RV_RN_NKCountryOfReg);
				vessel = LoadVessel("732025Z");
				AssertEquals("MSC TERESA ZZ", vessel.RV_Code);
				AssertEquals(ZString.Empty, vessel.RV_RN_NKCountryOfReg);
				vessel = LoadVessel("734751Z");
				AssertEquals("MSC FEDERICA ZZ", vessel.RV_Code);
				AssertEquals(ZString.Empty, vessel.RV_RN_NKCountryOfReg);
			}
		}

		RefVessel LoadVessel(ZString lloydsID)
		{
			ZQuery filter = new ZQuery(RefVesselSchema.RV_LloydsNumber, lloydsID);
			RefVessel vessel = Factory.LoadTop1<RefVessel>(filter);
			AssertNotNull("Expecting Vessel with LloydsID " + lloydsID + " to be found", vessel);
			return vessel;
		}

		void ClearVesselRecordsBeforeTesting()
		{
			TestCaseHelper.ClearTable(RefVesselSchema.Constants.TableName);
		}

		RefVesselDataLoad Loader;
		protected override void SetUp()
		{
			base.SetUp();
			Loader = new RefVesselDataLoad();
		}
	}
}

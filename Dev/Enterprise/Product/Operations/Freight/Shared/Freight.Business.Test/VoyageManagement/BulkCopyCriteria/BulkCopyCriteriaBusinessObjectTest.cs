using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	public class BulkCopyCriteriaBusinessObjectTest : TestCaseWithFactory
	{
		public void TestSettingPatternResetsPatternDependentFields()
		{
			bulkCopyCriteriaForTest.RepititionSelection.UseDailyPattern = false;
			bulkCopyCriteriaForTest.RepititionSelection.UseWeeklyPattern = false;
			bulkCopyCriteriaForTest.RepititionSelection.UseMonthlyPattern = false;

			ZPropertyInfo[] patternInfos =
			{
				bulkCopyCriteriaForTest.RepititionSelection.UseDailyPatternInfo,
				bulkCopyCriteriaForTest.RepititionSelection.UseWeeklyPatternInfo,
				bulkCopyCriteriaForTest.RepititionSelection.UseMonthlyPatternInfo
			};

			foreach (ZPropertyInfo info in patternInfos)
			{
				info.Value = ZBool.False;
				bulkCopyCriteriaForTest.RepititionSelection.RecurrenceInterval = 5;
				bulkCopyCriteriaForTest.RepititionSelection.DayOfTheMonth = 7;
				AssertEquals(info.Name + ": should have the new value", 5, bulkCopyCriteriaForTest.RepititionSelection.RecurrenceInterval);
				AssertEquals(info.Name + ": should have the new value", 7, bulkCopyCriteriaForTest.RepititionSelection.DayOfTheMonth);

				info.Value = ZBool.False;
				AssertEquals(info.Name + ": should not have reset as the value didnt change", 5, bulkCopyCriteriaForTest.RepititionSelection.RecurrenceInterval);
				AssertEquals(info.Name + ": should not have reset as the value didnt change", 7, bulkCopyCriteriaForTest.RepititionSelection.DayOfTheMonth);

				info.Value = ZBool.True;
				AssertEquals(info.Name + ": should have reset as the value changed", 1, bulkCopyCriteriaForTest.RepititionSelection.RecurrenceInterval);
				AssertEquals(info.Name + ": should have reset as the value changed", 1, bulkCopyCriteriaForTest.RepititionSelection.DayOfTheMonth);

				info.Value = ZBool.False;
			}
		}

		[TestDate(2006, 3, 2, 12, 0, 0)]
		public void TestSetDefaultValues()
		{
			BulkCopyCriteria newBulkCopyObject = new BulkCopyCriteriaForTest(sailing.PK);
			AssertEquals("BulkCopyFirstETD is set to true", true, newBulkCopyObject.RepititionSelection.FromFirstETD);
			AssertEquals("BulkCopyDailyPattern is set to true", true, newBulkCopyObject.RepititionSelection.UseDailyPattern);
			AssertEquals("BulkCopyDateFrom should be 12/03/06/ 12:0:0", new ZDateTime(2006, 3, 2, 12, 0, 0), newBulkCopyObject.RepititionSelection.FromDate);
		}

		public void TestConsolGeneration()
		{
			bulkCopyCriteriaForTest.ConsolDetails.Weight = 200m;
			bulkCopyCriteriaForTest.RepititionSelection.ToDate = ZDateTime.Today.AddDays(1);

			AssertEquals(0, bulkCopyCriteriaForTest.ConsolDetails.CreatedConsols.Count);
			bulkCopyCriteriaForTest.Generate();
			AssertEquals("Not creating consol details", 0, bulkCopyCriteriaForTest.ConsolDetails.CreatedConsols.Count);

			bulkCopyCriteriaForTest.ConsolDetails.CreateConsol = true;
			bulkCopyCriteriaForTest.Generate();
			Assert(bulkCopyCriteriaForTest.ConsolDetails.CreatedConsols.Count > 0);
		}

		public void TestDispose()
		{
			bool disposeCalled = false;

			using (BulkCopyCriteriaForTest bulkCopyCriteria = new BulkCopyCriteriaForTest(sailing.PK))
			{
				bulkCopyCriteria.DisposeImplementaion = () => disposeCalled = true;
			}

			Assert(disposeCalled);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			CreateSailing();
			bulkCopyCriteriaForTest = new BulkCopyCriteriaForTest(sailing.PK);
		}

		void CreateSailing()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage.JV_FlightDate = ZDateTime.Today;

			JobVoyage voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage2.JV_FlightDate = ZDateTime.Today;

			VoyageOrigin origin = Factory.New<VoyageOrigin>();
			origin.JA_JV = voyage.PK;
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = ZDateTime.Today;

			VoyageOrigin origin2 = Factory.New<VoyageOrigin>();
			origin2.JA_JV = voyage2.PK;
			origin2.JA_RL_NKPortOfLoading = "AUSYD";
			origin2.JA_E_DEP = ZDateTime.Today;

			VoyageDestination destination = Factory.New<VoyageDestination>();
			destination.JB_JV = voyage.PK;
			destination.JB_RL_NKPortOfDischarge = "USLAX";
			destination.JB_E_ARV = ZDateTime.Today.AddDays(10);

			VoyageDestination destination2 = Factory.New<VoyageDestination>();
			destination2.JB_JV = voyage2.PK;
			destination2.JB_RL_NKPortOfDischarge = "USLAX";
			destination2.JB_E_ARV = ZDateTime.Today.AddDays(10);

			sailing = Factory.New<JobSailing>();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;

			sailing2 = Factory.New<JobSailing>();
			sailing2.JX_JA = origin2.PK;
			sailing2.JX_JB = destination2.PK;

			Factory.Save();
		}

		BaseJobSailing sailing;
		BaseJobSailing sailing2;
		BulkCopyCriteria bulkCopyCriteriaForTest;

		#endregion
	}
}

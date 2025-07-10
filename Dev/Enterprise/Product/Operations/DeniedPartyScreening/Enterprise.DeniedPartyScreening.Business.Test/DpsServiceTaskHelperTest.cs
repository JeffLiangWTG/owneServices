using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Newtonsoft.Json;

namespace Enterprise.DeniedPartyScreening.Business.Test
{
	public class DpsServiceTaskHelperTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => _ = new DpsServiceTaskHelper(null));
		}

		public void TestCreateNewAndSaveOrGetExistingStmData()
		{
			var dpsServiceTaskHelper = new DpsServiceTaskHelper("TST");
			AssertEquals("ServiceTaskTST_LastSuccessfulRuntime", 1, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.StmData WHERE SD_Name = 'ServiceTaskTST_LastSuccessfulRuntime'"));

			var runningStatus = GetRunningStatus(dpsServiceTaskHelper);
			AssertEquals(0, runningStatus.FailedToRunCount);
			AssertNotEquals(DateTime.MinValue, runningStatus.LastSuccessfulRuntime);
			AssertEquals("Data Saved To Database", true, dpsServiceTaskHelper.StmDataInfo.IsInDatabase);
			AssertEquals("Data Saved To Database", false, dpsServiceTaskHelper.StmDataInfo.HasChanges);
		}

		public void TestUpdateLastSuccessfulRuntime()
		{
			var dpsServiceTaskHelper = new DpsServiceTaskHelper("TST");
			var runTime = ZDateTime.UtcNow.AddSeconds(-10).ToDateTime();
			dpsServiceTaskHelper.StmDataInfo.SD_BinaryValue = ZBlob.FromUTF8(JsonConvert.SerializeObject(new ServiceTaskRunningStatus
			{
				FailedToRunCount = 1,
				LastSuccessfulRuntime = runTime,
			}));

			var runningStatus = GetRunningStatus(dpsServiceTaskHelper);

			AssertGreaterThan("Pre-Condition", 1.0, Math.Abs((runTime - runningStatus.LastSuccessfulRuntime).TotalSeconds));
			AssertEquals("Pre-Condition", 1, runningStatus.FailedToRunCount);

			dpsServiceTaskHelper.UpdateLastSuccessfulRuntime();
			runningStatus = GetRunningStatus(dpsServiceTaskHelper);

			AssertLessThan("UpdateLastSuccessfulRuntime", 9.0, Math.Abs((runTime - runningStatus.LastSuccessfulRuntime).TotalSeconds));
			AssertEquals("FailedToRunCount", 0, runningStatus.FailedToRunCount);
			AssertEquals("Data Saved To Database", true, dpsServiceTaskHelper.StmDataInfo.IsInDatabase);
			AssertEquals("Data Saved To Database", false, dpsServiceTaskHelper.StmDataInfo.HasChanges);
		}

		public void TestIncreaseFailedToRunCount()
		{
			var dpsServiceTaskHelper = new DpsServiceTaskHelper("TST");
			dpsServiceTaskHelper.IncreaseFailedToRunCount();
			var runningStatus = GetRunningStatus(dpsServiceTaskHelper);

			AssertEquals("FailedToRunCount", 1, runningStatus.FailedToRunCount);
			AssertEquals("Data Saved To Database", true, dpsServiceTaskHelper.StmDataInfo.IsInDatabase);
			AssertEquals("Data Saved To Database", false, dpsServiceTaskHelper.StmDataInfo.HasChanges);
		}

		public void TestLogAsWarning()
		{
			var dpsServiceTaskHelper = new DpsServiceTaskHelper("TST");
			AssertEquals(true, dpsServiceTaskHelper.IsLogAsWarning);

			dpsServiceTaskHelper.StmDataInfo.SD_BinaryValue = ZBlob.FromUTF8(JsonConvert.SerializeObject(new ServiceTaskRunningStatus
			{
				FailedToRunCount = 1,
				LastSuccessfulRuntime = ZDateTime.UtcNow.AddMinutes(-121).ToDateTime(),
			}));

			AssertEquals("When FailedToRunCount is 1, LastSuccessfulRuntime is 121 minutes ago", true, dpsServiceTaskHelper.IsLogAsWarning);

			dpsServiceTaskHelper.StmDataInfo.SD_BinaryValue = ZBlob.FromUTF8(JsonConvert.SerializeObject(new ServiceTaskRunningStatus
			{
				FailedToRunCount = 2,
				LastSuccessfulRuntime = ZDateTime.UtcNow.AddMinutes(-90).ToDateTime(),
			}));
			AssertEquals("When FailedToRunCount is 2, LastSuccessfulRuntime is 90 minutes ago", true, dpsServiceTaskHelper.IsLogAsWarning);
		}

		public void TestLogAsError()
		{
			var dpsServiceTaskHelper = new DpsServiceTaskHelper("TST");
			dpsServiceTaskHelper.StmDataInfo.SD_BinaryValue = ZBlob.FromUTF8(JsonConvert.SerializeObject(new ServiceTaskRunningStatus
			{
				FailedToRunCount = 2,
				LastSuccessfulRuntime = ZDateTime.UtcNow.AddMinutes(-121).ToDateTime(),
			}));

			AssertEquals("When FailedToRunCount larger than 1, LastSuccessfulRuntime is 121 minutes ago", false, dpsServiceTaskHelper.IsLogAsWarning);
		}

		public void TestProcessingHistoricalStmDataCorrectly()
		{
			var dpsServiceTaskHelper = new DpsServiceTaskHelper("TST");
			dpsServiceTaskHelper.StmDataInfo.SD_BinaryValue = ZBlob.Empty;
			var runningStatus = GetRunningStatus(dpsServiceTaskHelper);
			AssertNull("Pre-Condition: Historical StmData with empty binary value", runningStatus);

			dpsServiceTaskHelper.UpdateLastSuccessfulRuntime();
			runningStatus = GetRunningStatus(dpsServiceTaskHelper);
			AssertNotNull("Create Default Running Status when binary is empty", runningStatus);

			dpsServiceTaskHelper.StmDataInfo.SD_BinaryValue = ZBlob.Empty;
			dpsServiceTaskHelper.IncreaseFailedToRunCount();
			runningStatus = GetRunningStatus(dpsServiceTaskHelper);
			AssertNotNull("Create Default Running Status when binary is empty", runningStatus);
			AssertEquals(1, runningStatus.FailedToRunCount);
		}

		ServiceTaskRunningStatus GetRunningStatus(DpsServiceTaskHelper dpsServiceTaskHelper)
		{
			return JsonConvert.DeserializeObject<ServiceTaskRunningStatus>(dpsServiceTaskHelper.StmDataInfo.SD_BinaryValue.ToUTF8());
		}
	}
}

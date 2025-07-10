using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;

namespace Enterprise.DeniedPartyScreening.Business
{
	public class DpsServiceTaskHelper
	{
		public DpsServiceTaskHelper(string serviceTaskCode)
		{
			Argument.NotNullOrEmpty(serviceTaskCode, nameof(serviceTaskCode));

			Factory = new BusinessObjectFactory();

			var taskLastSuccessfulRuntimeKey = $@"ServiceTask{serviceTaskCode}_LastSuccessfulRuntime";
			StmDataInfo = Factory.LoadTop1<StmData>(new ZQuery(StmDataSchema.SD_Name, taskLastSuccessfulRuntimeKey));
			if (StmDataInfo == null)
			{
				StmDataInfo = Factory.New<StmData>();
				StmDataInfo.SD_Name = taskLastSuccessfulRuntimeKey;
				UpdateLastSuccessfulRuntime();
			}
		}

		public void UpdateLastSuccessfulRuntime()
		{
			StmDataInfo.SD_BinaryValue = ZBlob.FromUTF8(JsonConvert.SerializeObject(GetDefaultSuccessfulRunTime()));
			StmDataInfo.Factory.Save();
		}

		public void IncreaseFailedToRunCount()
		{
			var runningStatus = GetRunningStatus();
			runningStatus.FailedToRunCount = runningStatus.FailedToRunCount + 1;
			StmDataInfo.SD_BinaryValue = ZBlob.FromUTF8(JsonConvert.SerializeObject(runningStatus));

			StmDataInfo.Factory.Save();
		}

		/// <summary>
		/// If the DPS server is not functioning (500 - Internal Server Error) for less than 2 hours or failed to run count less or equal to 1, we will log it as a warning.
		/// </summary>
		public bool IsLogAsWarning
		{
			get
			{
				var runningStatus = GetRunningStatus();
				return runningStatus.LastSuccessfulRuntime.AddMinutes(120) > ZDateTime.UtcNow.ToDateTime() || runningStatus.FailedToRunCount <= 1;
			}
		}

		public StmData StmDataInfo { get; private set; }

		ServiceTaskRunningStatus GetRunningStatus()
		{
			ServiceTaskRunningStatus runningStatus = null;

			try
			{
				var dataValue = StmDataInfo.SD_BinaryValue.ToUTF8();
				if (!dataValue.IsEmpty)
				{
					runningStatus = JsonConvert.DeserializeObject<ServiceTaskRunningStatus>(dataValue);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
			}

			return runningStatus ?? GetDefaultSuccessfulRunTime();
		}

		ServiceTaskRunningStatus GetDefaultSuccessfulRunTime() => new ServiceTaskRunningStatus
		{
			FailedToRunCount = 0,
			LastSuccessfulRuntime = ZDateTime.UtcNow.ToDateTime(),
		};

		BusinessObjectFactory Factory { get; }
	}

	class ServiceTaskRunningStatus
	{
		public int FailedToRunCount { get; set; }
		public DateTime LastSuccessfulRuntime { get; set; }
	}
}

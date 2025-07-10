using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	internal class JobScheduleChangeLogger
	{
		public JobScheduleChange[] LogVoyageDateChanges(JobVoyage voyage)
		{
			List<JobScheduleChange> result = new List<JobScheduleChange>();
			foreach (JobSailing sailing in voyage.Sailings)
			{
				result.AddRange(LogSailingDateChanges(sailing));
			}
			foreach (VoyageOrigin origin in voyage.Origins)
			{
				result.AddRange(LogOriginDateChanges(origin));
			}
			foreach (VoyageDestination destination in voyage.Destinations)
			{
				result.AddRange(LogDestinationDateChanges(destination));
			}
			return result.ToArray();
		}

		public JobScheduleChange[] LogSailingDateChanges(JobSailing sailing)
		{
			List<JobScheduleChange> result = new List<JobScheduleChange>();
			return result.ToArray();
		}

		public JobScheduleChange[] LogOriginDateChanges(VoyageOrigin origin)
		{
			List<JobScheduleChange> result = new List<JobScheduleChange>();

			var scheduleChangeEmailSupporter = origin as IScheduleChangeEmailSupporter;
			if (scheduleChangeEmailSupporter != null)
			{
				LogDateChange(ScheduleDateTypes.Codes.ETD, origin.JA_E_DEPInfo, result, scheduleChangeEmailSupporter);
				LogDateChange(ScheduleDateTypes.Codes.ATD, origin.JA_A_DEPInfo, result, scheduleChangeEmailSupporter);
				LogDateChange(ScheduleDateTypes.Codes.FCLReceivalCommences, origin.JA_ReceivalCommencesInfo, result, scheduleChangeEmailSupporter);
				LogDateChange(ScheduleDateTypes.Codes.FCLCutOff, origin.JA_CutOffInfo, result, scheduleChangeEmailSupporter);
				LogDateChange(ScheduleDateTypes.Codes.EmptyReceivalCommences, origin.JA_EmptyReceivalCommencesInfo, result, scheduleChangeEmailSupporter);
				LogDateChange(ScheduleDateTypes.Codes.EmptyCutOff, origin.JA_EmptyCutOffInfo, result, scheduleChangeEmailSupporter);
				LogDateChange(ScheduleDateTypes.Codes.ReeferReceivalCommences, origin.JA_ReeferReceivalCommencesInfo, result, scheduleChangeEmailSupporter);
				LogDateChange(ScheduleDateTypes.Codes.ReeferCutOff, origin.JA_ReeferCutOffInfo, result, scheduleChangeEmailSupporter);
			}
			return result.ToArray();
		}

		public JobScheduleChange[] LogDestinationDateChanges(VoyageDestination destination)
		{
			List<JobScheduleChange> result = new List<JobScheduleChange>();

			var scheduleChangeEmailSupporter = destination as IScheduleChangeEmailSupporter;
			if (scheduleChangeEmailSupporter != null)
			{
				LogDateChange(ScheduleDateTypes.Codes.ETA, destination.JB_E_ARVInfo, result, scheduleChangeEmailSupporter);
				LogDateChange(ScheduleDateTypes.Codes.ATA, destination.JB_A_ARVInfo, result, scheduleChangeEmailSupporter);
				LogDateChange(ScheduleDateTypes.Codes.FCLAvailable, destination.JB_AvailabilityDateInfo, result, scheduleChangeEmailSupporter);
				LogDateChange(ScheduleDateTypes.Codes.FCLStorage, destination.JB_StorageDateInfo, result, scheduleChangeEmailSupporter);
			}

			return result.ToArray();
		}

		#region Implementation

		void LogDateChange(string dateType, ZPropertyInfo property, List<JobScheduleChange> scheduleChanges, IScheduleChangeEmailSupporter scheduleChangeEmailSupporter)
		{
			if (property.BizObj.IsInDatabase && property.HasChanges)
			{
				JobScheduleChange.Loader loader = new JobScheduleChange.Loader(property.BizObj.Factory);
				JobScheduleChange scheduleChange = loader.LoadOrCreate(property.BizObj, dateType);

				scheduleChange.E7_ChangedAt = ZDateTime.UtcNow;

				if (!scheduleChange.IsInDatabase)
				{
					scheduleChange.E7_PreviousValue = (ZDateTime)property.OriginalValue;
				}

				scheduleChange.E7_UpdatedValue = (ZDateTime)property.Value;
				scheduleChange.E7_DataProvider = scheduleChangeEmailSupporter.GetDataProvider(dateType);

				scheduleChanges.Add(scheduleChange);
			}
		}

		#endregion
	}
}

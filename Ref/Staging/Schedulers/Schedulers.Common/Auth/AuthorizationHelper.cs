using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.Schedulers.Common
{
	public class AuthorizationHelper : IAuthorizationHelper
	{
		readonly bool checkAuthorization;
		readonly ISafeRepository safeRepo;
		readonly IStagingRepository stagingRepo;

		public AuthorizationHelper(ISafeRepository safeRepo, IStagingRepository stagingRepo, bool checkAuthorization)
		{
			this.safeRepo = safeRepo;
			this.stagingRepo = stagingRepo;
			this.checkAuthorization = checkAuthorization;
		}

		IEnumerable<string> GetAuthorizedGroups(string user)
		{
			var jobGroups = safeRepo.GetLatest<UserAuthorization>().Where(x => x.UA_User == user && x.UA_DataSetName == "Quartz");
			return jobGroups.AsEnumerable().Select(x => x.UA_TableName);
		}

		public bool IsAuthorized(string user, IFormCollectionService formCollectionService)
		{
			if (formCollectionService == null)
			{
				return true;
			}

			var command = formCollectionService.Command;
			if (string.IsNullOrEmpty(command) || formCollectionService.IsGetCommand)
			{
				return true;
			}

			var jobGroup = formCollectionService.Group;
			if (formCollectionService.IsTriggerRelatedCommand)
			{
				var triggerName = formCollectionService.Trigger;
				jobGroup = string.IsNullOrEmpty(triggerName) ? jobGroup : GetJobGroupByTriggerName(triggerName);
			}
			if (checkAuthorization || formCollectionService.IsSchedulerOrGroupRelatedCommand || string.Equals(jobGroup, "Reference Data", StringComparison.OrdinalIgnoreCase))
			{
				var authorizedGroups = GetAuthorizedGroups(user);
				return authorizedGroups.Contains(jobGroup, StringComparer.OrdinalIgnoreCase);
			}
			else
			{
				return true;
			}
		}

		string GetJobGroupByTriggerName(string triggerName)
		{
			return stagingRepo.Get<QRTZ_TRIGGERS>().FirstOrDefault(x => x.TRIGGER_NAME == triggerName)?.JOB_GROUP;
		}
	}
}

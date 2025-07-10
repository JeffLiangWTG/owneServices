using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Staging.NewService.Controllers;

namespace CargoWise.RefDbRepo.Staging.NewService
{
	public static class QuartzJobGroupHelper
	{
		static ConcurrentDictionary<string, IEnumerable<string>> jobGroupsDictionary = new ConcurrentDictionary<string, IEnumerable<string>>();

		public static bool IsValidJobGroup(QRTZ_JOB_DETAILSUpdateController quartzController, string jobGroup, string configFilePath)
		{
			var isValid = false;
			if (!string.IsNullOrEmpty(jobGroup) && !string.IsNullOrEmpty(configFilePath))
			{
				configFilePath = configFilePath.Replace(".exe.config", "").Replace(".config.json", "");
				var programPaths = jobGroupsDictionary.GetOrAdd(jobGroup, (key) => GetProgramPathsByJobGroup(quartzController, jobGroup));
				if (programPaths != null && programPaths.Contains(configFilePath))
				{
					isValid = true;
				}
			}
			return isValid;
		}

		static IEnumerable<string> GetProgramPathsByJobGroup(QRTZ_JOB_DETAILSUpdateController quartzController, string jobGroup)
		{
			return quartzController.Get().Where(x => x.JOB_GROUP == jobGroup).ToList().Where(x => !string.IsNullOrEmpty(x.ProgramExePath)).Select(x => x.ProgramExePath.Substring(x.ProgramExePath.LastIndexOf('\\') + 1).Replace(".exe", ""));
		}
	}
}

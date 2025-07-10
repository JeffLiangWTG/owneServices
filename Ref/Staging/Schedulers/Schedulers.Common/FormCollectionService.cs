using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using Microsoft.AspNetCore.Http;

namespace CargoWise.RefDbRepo.Staging.Schedulers.Common
{
	public class FormCollectionService : IFormCollectionService
	{
		readonly IFormCollection formCollection;
		public FormCollectionService(IHttpContextAccessor accessor)
		{
			Argument.NotNull(accessor, nameof(accessor));
			var request = accessor.HttpContext?.Request;
			if (request != null && request.HasFormContentType)
			{
				formCollection = request.ReadFormAsync().Result;
			}
		}

		public string Command => formCollection?["command"].ToString();
		public string Job => formCollection?["job"].ToString();
		public string Group => formCollection?["group"].ToString();
		public string Name => formCollection?["name"].ToString();
		public string Trigger => formCollection?["trigger"].ToString();
		public string TriggerType => formCollection?["triggerType"].ToString();
		public string CronExpression => formCollection?["cronExpression"].ToString();
		public string RepeatCount => formCollection?["repeatCount"].ToString();
		public string RepeatInterval => formCollection?["repeatInterval"].ToString();
		public string RepeatForever => formCollection?["repeatForever"].ToString();
		public ICollection<string> Keys => formCollection?.Keys;

		public bool IsGetCommand => Command != null && Command.StartsWith("get", StringComparison.OrdinalIgnoreCase);
		public bool IsAddTriggerCommand => Command != null && Command.Equals("add_trigger", StringComparison.OrdinalIgnoreCase);
		public bool IsTriggerRelatedCommand => Command != null && Command.Contains("trigger", StringComparison.OrdinalIgnoreCase);
		public bool IsSchedulerOrGroupRelatedCommand => Command != null && (Command.Contains("scheduler", StringComparison.OrdinalIgnoreCase) || Command.Contains("group", StringComparison.OrdinalIgnoreCase));
		public bool IsAllowedCommand => IsGetCommand || allowedCommands.Contains(Command, StringComparer.OrdinalIgnoreCase);

		public string GetValue(string key)
		{
			return formCollection?[key].ToString();
		}

		readonly List<string> allowedCommands = new List<string>
		{
			"delete_trigger",
			"delete_job",
			"execute_job",
			"pause_trigger",
			"pause_group",
			"resume_trigger",
			"resume_group"
		};
	}
}

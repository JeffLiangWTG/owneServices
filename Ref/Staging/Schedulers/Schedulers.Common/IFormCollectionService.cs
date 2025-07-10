using System.Collections.Generic;

namespace CargoWise.RefDbRepo.Staging.Schedulers.Common
{
	public interface IFormCollectionService
	{
		string Command { get; }
		string Job { get; }
		string Group { get; }
		string Name { get; }
		string Trigger { get; }
		string TriggerType { get; }
		string CronExpression { get; }
		string RepeatCount { get; }
		string RepeatInterval { get; }
		string RepeatForever { get; }
		ICollection<string> Keys { get; }

		bool IsGetCommand { get; }
		bool IsAddTriggerCommand { get; }
		bool IsTriggerRelatedCommand { get; }
		bool IsSchedulerOrGroupRelatedCommand { get; }
		bool IsAllowedCommand { get; }
		string GetValue(string key);
	}
}

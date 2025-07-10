
namespace Enterprise.Freight.Business.Extensions
{
	using Enterprise.MasterFiles.Business;

	public static class ProcessTaskCollectionExtensions
	{
		public static ProcessTask AddMilestone(this ProcessTaskCollection processTasks, string linkedToEvent, bool includingCascaded = false)
		{
			var processTask = processTasks.AddNew();
			processTask.TriggerConditions.TriggerEventCode = linkedToEvent;
			processTask.P9_RespondToCascadedEvents = includingCascaded;
			processTask.IsMilestone = true;

			return processTask;
		}
	}
}

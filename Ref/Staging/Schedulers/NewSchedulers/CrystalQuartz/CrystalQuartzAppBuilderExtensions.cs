using CrystalQuartz.Application;
using CrystalQuartz.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Quartz;

namespace CargoWise.RefDbRepo.Staging.NewSchedulers.CrystalQuartz
{
	public static class CrystalQuartzAppBuilderExtensions
	{
		private const string DefaultCrystalQuartzEventListenerName = "CrystalQuartzTriggersListener";

		/// <summary>
		/// A custom extension method to use CrystalQuartz with custom options.
		/// By default, CrystalQuartz uses DefaultCrystalQuartzEventListenerName as the event listener name. This leads
		/// to a conflict when multiple instances of CrystalQuartz are used in the same application. This method allows
		/// us to specify a custom event listener name to avoid conflicts and call app.UseCrystalQuartz multiple times.
		/// </summary>
		/// <param name="app"></param>
		/// <param name="scheduler"></param>
		/// <param name="options">The options to pass into the default UseCrystalQuartz</param>
		/// <param name="overrideOptions">Specify a unique name for the listener for each CrystalQuartz instance</param>
		public static void UseCustomCrystalQuartz(this IApplicationBuilder app, IScheduler scheduler,
			CrystalQuartzOptions options = null, OverrideCrystalQuartzOptions overrideOptions = null)
		{
			app.UseCrystalQuartz(() => scheduler, options);

			if (overrideOptions == null || string.IsNullOrWhiteSpace(overrideOptions.EventListenerName))
			{
				return;
			}

			var jobListener = scheduler.ListenerManager.GetJobListener(DefaultCrystalQuartzEventListenerName);
			var wrapJobListener =
				new CustomCrystalQuartzJobListener(overrideOptions.EventListenerName, jobListener);
			scheduler.ListenerManager.RemoveJobListener(DefaultCrystalQuartzEventListenerName);
			scheduler.ListenerManager.AddJobListener(wrapJobListener);

			var triggerListener = scheduler.ListenerManager.GetTriggerListener(DefaultCrystalQuartzEventListenerName);
			var wrapTriggerListener =
				new CustomCrystalQuartzTriggerListener(overrideOptions.EventListenerName, triggerListener);
			scheduler.ListenerManager.RemoveTriggerListener(DefaultCrystalQuartzEventListenerName);
			scheduler.ListenerManager.AddTriggerListener(wrapTriggerListener);
		}
	}
}

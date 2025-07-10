using System;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Staging.NewSchedulers.CrystalQuartz;
using CargoWise.RefDbRepo.Staging.ProcessorRunner;
using CargoWise.RefDbRepo.Staging.Schedulers.Common;
using CrystalQuartz.Application;
using Microsoft.AspNetCore.Builder;
using Quartz;
using Quartz.Impl;
using Topshelf;

namespace CargoWise.RefDbRepo.Staging.NewSchedulers
{
	public class QuartzServer : ServiceControl, IQuartzServer
	{
		readonly ILogHelper logHelper;
		ISchedulerFactory schedulerFactory;
		IScheduler scheduler;
		readonly WebApplication app;

		public QuartzServer(ILogHelper logHelper, WebApplication app)
		{
			Argument.NotNull(logHelper, nameof(logHelper));
			this.logHelper = logHelper;
			this.app = app;
		}

		public async Task Start()
		{
			try
			{
				RunWebApp(scheduler);
				RunWebAuthApp(scheduler);
				await scheduler.Start();
				var quartzPanelPort = ConfigurationProvider.QuartzPanelPort;
				app.Run($"http://+:{quartzPanelPort}/");
			}
			catch (Exception ex)
			{
				logHelper.LogFatal($"NewScheduler start failed: {ex.Message}", ex);
				throw;
			}

			logHelper.LogInfo("NewScheduler started successfully");
		}

#if DEBUG
		public async Task StartScheduler()
		{
			await scheduler.Start();
			await app.RunAsync();
		}
#endif

		public async Task Stop()
		{
			try
			{
				ProcessMonitor.Instance.KillAll(logHelper);
				await scheduler.Shutdown(true);
			}
			catch (Exception ex)
			{
				logHelper.LogError($"NewScheduler stop failed: {ex.Message}", ex);
				throw;
			}
			finally
			{
				ProcessMonitor.Instance.Dispose();
			}

			logHelper.LogInfo("NewScheduler shutdowned completely");
		}

		public async Task Initialize()
		{
			try
			{
				schedulerFactory = CreateSchedulerFactory();
				scheduler = await CreateScheduler();
				var quartzJobDetailsSynchronizer = new QuartzJobDetailsSynchronizer(logHelper);
				await quartzJobDetailsSynchronizer.Synchronize(scheduler);
				app.UseMiddleware<QuartzLogMiddleware>(logHelper, scheduler);
			}
			catch (Exception e)
			{
				logHelper.LogError("NewScheduler initialization failed:" + e.Message, e);
				throw;
			}

			logHelper.LogInfo("NewScheduler initialized completely");
		}

		public async Task Pause()
		{
			try
			{
				await scheduler.PauseAll();
			}
			catch (Exception ex)
			{
				logHelper.LogError($"NewScheduler pause failed: {ex.Message}", ex);
				throw;
			}

			logHelper.LogInfo("NewScheduler paused completely");
		}

		public async Task Resume()
		{
			try
			{
				await scheduler.ResumeAll();
			}
			catch (Exception ex)
			{
				logHelper.LogError($"NewScheduler resume failed: {ex.Message}", ex);
				throw;
			}

			logHelper.LogInfo("NewScheduler resumed completely");
		}

		public bool Start(HostControl hostControl)
		{
			Task.Run(() => Start());
			return true;
		}

		public bool Stop(HostControl hostControl)
		{
			Stop().Wait();
			return true;
		}

		static ISchedulerFactory CreateSchedulerFactory()
		{
			return new StdSchedulerFactory(ConfigurationProvider.QuartzProps);
		}

		async Task<IScheduler> CreateScheduler()
		{
			return await schedulerFactory.GetScheduler();
		}

		void RunWebApp(IScheduler scheduler)
		{
			var quartzOptions = new CrystalQuartzOptions { CustomCssUrl = "CustomQuartzPanel.css" };
			var overrideOptions = new OverrideCrystalQuartzOptions { EventListenerName = "WebAppListener" };

			app.UseCustomCrystalQuartz(scheduler, quartzOptions, overrideOptions);
		}

		void RunWebAuthApp(IScheduler scheduler)
		{
			var authQuartzOptions =
				new CrystalQuartzOptions { CustomCssUrl = "CustomQuartzPanel.css", Path = "/auth/quartz" };
			var overrideOptions = new OverrideCrystalQuartzOptions { EventListenerName = "WebAuthAppListener" };
			app.UseCustomCrystalQuartz(scheduler, authQuartzOptions, overrideOptions);
		}
	}
}

using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Staging.NewSchedulers.CrystalQuartz;
using CrystalQuartz.Application;
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;
using Quartz;
using Quartz.Impl;

namespace CargoWise.RefDbRepo.Staging.NewSchedulers.Test
{
	[TestFixture]
	public class CrystalQuartzAppBuilderExtensionsFixture
	{
		private ISchedulerFactory _schedulerFactory;
		private IScheduler _scheduler;
		private CrystalQuartzOptions _defaultOptions;
		private const string DefaultCrystalQuartzEventListenerName = "CrystalQuartzTriggersListener";

		[SetUp]
		public async Task SetUp()
		{
			_schedulerFactory = new StdSchedulerFactory();
			_scheduler = await _schedulerFactory.GetScheduler();
			_defaultOptions = new CrystalQuartzOptions();
		}

		[TearDown]
		public async Task TearDown()
		{
			await _scheduler.Shutdown();
		}

		[Test]
		public void SetDefaultEventListenersIfCalledWithoutCustomOptions()
		{
			using (var app = WebApplication.Create())
			{
				app.UseCustomCrystalQuartz(_scheduler, _defaultOptions);

				var jobListeners = _scheduler.ListenerManager.GetJobListeners();
				Assert.That(jobListeners, Has.Count.EqualTo(1));
				Assert.That(jobListeners.First().Name, Is.EqualTo(DefaultCrystalQuartzEventListenerName));

				var triggerListeners = _scheduler.ListenerManager.GetTriggerListeners();
				Assert.That(triggerListeners, Has.Count.EqualTo(1));
				Assert.That(triggerListeners.First().Name, Is.EqualTo(DefaultCrystalQuartzEventListenerName));
			}
		}

		[Test]
		public void SetOneEventListenerWithCorrectNameIfCalledOnce()
		{
			using (var app = WebApplication.Create())
			{
				var listenerName = "gCb6zdDVck";
				app.UseCustomCrystalQuartz(_scheduler, _defaultOptions,
					new OverrideCrystalQuartzOptions { EventListenerName = listenerName });

				var jobListeners = _scheduler.ListenerManager.GetJobListeners();
				Assert.That(jobListeners, Has.Count.EqualTo(1));
				Assert.That(jobListeners.First().Name, Is.EqualTo(listenerName));

				var triggerListeners = _scheduler.ListenerManager.GetTriggerListeners();
				Assert.That(triggerListeners, Has.Count.EqualTo(1));
				Assert.That(triggerListeners.First().Name, Is.EqualTo(listenerName));
			}
		}

		[Test]
		public void SetTwoEventListenersIfCalledTwiceWithDifferentNames()
		{
			using (var app = WebApplication.Create())
			{
				var listenerName1 = "eWI31HnB56";
				var listenerName2 = "aZzkGeWXVy";
				app.UseCustomCrystalQuartz(_scheduler, _defaultOptions,
					new OverrideCrystalQuartzOptions { EventListenerName = listenerName1 });
				app.UseCustomCrystalQuartz(_scheduler, _defaultOptions,
					new OverrideCrystalQuartzOptions { EventListenerName = listenerName2 });

				var jobListeners = _scheduler.ListenerManager.GetJobListeners();
				Assert.That(jobListeners, Has.Count.EqualTo(2));
				Assert.That(jobListeners.ElementAt(0).Name, Is.EqualTo(listenerName1));
				Assert.That(jobListeners.ElementAt(1).Name, Is.EqualTo(listenerName2));

				var triggerListeners = _scheduler.ListenerManager.GetTriggerListeners();
				Assert.That(triggerListeners, Has.Count.EqualTo(2));
				Assert.That(triggerListeners.ElementAt(0).Name, Is.EqualTo(listenerName1));
				Assert.That(triggerListeners.ElementAt(1).Name, Is.EqualTo(listenerName2));
			}
		}

		[Test]
		public void SetOneEventListenerIfCalledTwiceWithSameName()
		{
			using (var app = WebApplication.Create())
			{
				var listenerName = "kFtxLgXAK1";
				app.UseCustomCrystalQuartz(_scheduler, _defaultOptions,
					new OverrideCrystalQuartzOptions { EventListenerName = listenerName });
				app.UseCustomCrystalQuartz(_scheduler, _defaultOptions,
					new OverrideCrystalQuartzOptions { EventListenerName = listenerName });

				var jobListeners = _scheduler.ListenerManager.GetJobListeners();
				Assert.That(jobListeners, Has.Count.EqualTo(1));
				Assert.That(jobListeners.First().Name, Is.EqualTo(listenerName));

				var triggerListeners = _scheduler.ListenerManager.GetTriggerListeners();
				Assert.That(triggerListeners, Has.Count.EqualTo(1));
				Assert.That(triggerListeners.First().Name, Is.EqualTo(listenerName));
			}
		}
	}
}

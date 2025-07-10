using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Staging.ProcessorRunner;
using CargoWise.RefDbRepo.Staging.Schedulers.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;
using Quartz;
using Quartz.Impl.Triggers;

namespace CargoWise.RefDbRepo.Staging.NewSchedulers.Test
{
	[TestFixture]
	class QuartzLogMiddlewareFixture
	{
		[Test]
		public async Task InvokeAsync_JobLogs()
		{
			var jobName = "Job A";
			var jobGroup = "Group A";
			var command = "execute_job";
			var formCollectionDictionary = new Dictionary<string, StringValues>
			{
				{ "command", command },
				{ "job", jobName },
				{ "group", jobGroup }
			};
			context.Request.Form = new FormCollection(formCollectionDictionary);
			SetupRequestServices(context);

			await logMiddleware.InvokeAsync(context);
			mockNext.Verify(x => x.Invoke(context), Times.Once);
			mockLogHelper.Verify(x => x.LogInfo($"User: User A, Command: {command}, Job Name: {jobName}, Job Group: {jobGroup}"));
		}

		[TestCase("add_trigger")]
		[TestCase("pause_trigger")]
		[TestCase("resume_trigger")]
		[TestCase("delete_trigger")]
		public async Task InvokeAsync_SimpleTriggerLogs(string command)
		{
			var triggerName = "trigger A";
			var triggerGroup = "DEFAULT";
			var jobName = "Job A";
			var jobGroup = "Group A";
			var triggerType = "Simple";
			var repeatInterval = 86400000;
			var formCollectionDictionary = new Dictionary<string, StringValues>
			{
				{ "command", command },
				{ "group", triggerGroup },
				{ "trigger", triggerName }
			};
			if (command == "add_trigger")
			{
				formCollectionDictionary = new Dictionary<string, StringValues>
				{
					{ "command", command },
					{ "job", jobName },
					{ "group", jobGroup },
					{ "name", triggerName },
					{ "triggerType", triggerType },
					{ "repeatInterval", repeatInterval.ToString(CultureInfo.InvariantCulture) }
				};
			}
			context.Request.Form = new FormCollection(formCollectionDictionary);
			SetupRequestServices(context);

			var triggerKey = new TriggerKey(triggerName, triggerGroup);
			ITrigger simpleTrigger = new SimpleTriggerImpl(triggerName, triggerGroup, jobName, jobGroup, new DateTimeOffset(DateTime.Now), null, 10, TimeSpan.FromMilliseconds(repeatInterval));
			mockScheduler.Setup(x => x.GetTrigger(triggerKey, CancellationToken.None)).Returns(Task.FromResult(simpleTrigger));
			await logMiddleware.InvokeAsync(context);
			mockNext.Verify(x => x.Invoke(context), Times.Once);
			if (command == "add_trigger")
			{
				mockScheduler.Verify(x => x.GetTrigger(It.IsAny<TriggerKey>(), CancellationToken.None), Times.Never);
			}
			else
			{
				mockScheduler.Verify(x => x.GetTrigger(It.IsAny<TriggerKey>(), CancellationToken.None), Times.Once);
			}
			mockLogHelper.Verify(x => x.LogInfo($"User: User A, Command: {command}, Job Name: {jobName}, Job Group: {jobGroup}, Trigger Name: {triggerName}, Trigger Type: {triggerType}, Repeat Interval: {repeatInterval / 1000 / 60} min, Cron Expression: "));
		}

		[TestCase("add_trigger")]
		[TestCase("pause_trigger")]
		[TestCase("resume_trigger")]
		[TestCase("delete_trigger")]
		public async Task InvokeAsync_CronTriggerLogs(string command)
		{
			var triggerName = "trigger A";
			var triggerGroup = "DEFAULT";
			var jobName = "Job A";
			var jobGroup = "Group A";
			var triggerType = "Cron";
			var cronExpression = "0 0 10 * * ?";
			var formCollectionDictionary = new Dictionary<string, StringValues>
			{
				{ "command", command },
				{ "group", triggerGroup },
				{ "trigger", triggerName }
			};
			if (command == "add_trigger")
			{
				formCollectionDictionary = new Dictionary<string, StringValues>
				{
					{ "command", command },
					{ "job", jobName },
					{ "group", jobGroup },
					{ "name", triggerName },
					{ "triggerType", triggerType },
					{ "cronExpression", cronExpression }
				};
			}
			context.Request.Form = new FormCollection(formCollectionDictionary);
			SetupRequestServices(context);

			var triggerKey = new TriggerKey(triggerName, triggerGroup);
			ITrigger cronTrigger = new CronTriggerImpl(triggerName, triggerGroup, jobName, jobGroup, new DateTimeOffset(DateTime.Now), null, cronExpression);
			mockScheduler.Setup(x => x.GetTrigger(triggerKey, CancellationToken.None)).Returns(Task.FromResult(cronTrigger));
			await logMiddleware.InvokeAsync(context);
			mockNext.Verify(x => x.Invoke(context), Times.Once);
			if (command == "add_trigger")
			{
				mockScheduler.Verify(x => x.GetTrigger(It.IsAny<TriggerKey>(), CancellationToken.None), Times.Never);
			}
			else
			{
				mockScheduler.Verify(x => x.GetTrigger(It.IsAny<TriggerKey>(), CancellationToken.None), Times.Once);
			}
			mockLogHelper.Verify(x => x.LogInfo($"User: User A, Command: {command}, Job Name: {jobName}, Job Group: {jobGroup}, Trigger Name: {triggerName}, Trigger Type: {triggerType}, Repeat Interval: 0 min, Cron Expression: {cronExpression}"));
		}

		void SetupRequestServices(HttpContext context)
		{
			mockContextAccessor.Setup(x => x.HttpContext).Returns(context);
			var formCollectionService = new FormCollectionService(mockContextAccessor.Object);
			var mockServiceProvider = new Mock<IServiceProvider>();
			mockServiceProvider.Setup(x => x.GetService(typeof(IFormCollectionService))).Returns(formCollectionService);
			context.RequestServices = mockServiceProvider.Object;
		}

		[SetUp]
		public void Setup()
		{
			context = new DefaultHttpContext();
			context.Request.Method = "POST";
			context.Request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
			context.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "User A") }));
			mockContextAccessor = new Mock<IHttpContextAccessor>();

			mockNext = new Mock<RequestDelegate>();
			mockLogHelper = new Mock<ILogHelper>();
			mockScheduler = new Mock<IScheduler>();
			logMiddleware = new QuartzLogMiddleware(mockNext.Object, mockLogHelper.Object, mockScheduler.Object);
		}

		HttpContext context;
		Mock<RequestDelegate> mockNext;
		Mock<ILogHelper> mockLogHelper;
		Mock<IScheduler> mockScheduler;
		Mock<IHttpContextAccessor> mockContextAccessor;
		QuartzLogMiddleware logMiddleware;
	}
}

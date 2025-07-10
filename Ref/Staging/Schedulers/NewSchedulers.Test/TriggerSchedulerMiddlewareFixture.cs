using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Staging.Schedulers.Common;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.NewSchedulers.Test
{
	[TestFixture]
	public class TriggerSchedulerMiddlewareFixture
	{

		readonly string expectedTriggerMessage = "{ \"_err\":\"Triggers with JobDataMap are not allowed.\"}";
		readonly string expectedScheduleMessage = "{ \"_err\":\"New Schedule jobs are not allowed.\"}";
		readonly string expectedTriggerNameMessage = "{ \"_err\":\"Invalid Trigger Name.\"}";
		readonly string expectedNotAllowedEndPointMessage = "{ \"_err\":\"Action is not allowed.\"}";
		readonly string expectedInvalidCornExpression = "{ \"_err\":\"Invalid Cron Expression.\"}";
		readonly string expectedInvalidDataExpression = "{ \"_err\":\"Invalid RepeatCount or RepeatInterval.\"}";

		[Test]
		public async Task InvokeAsync_TriggerWithJobDataMap()
		{
			var context = new DefaultHttpContext();
			context.Request.Form = new FormCollection(new Dictionary<string, StringValues>
			{
				{ "command", "add_trigger" },
				{ "name", "Test_Trigger" },
				{ "job", "AU Exchange Rate Parser" },
				{ "triggerType", "Simple" },
				{ "jobDataMap[0].Key", "Data_one" },
				{ "jobDataMap[0].Value", "Data_value_one" },
				{ "group", "AU Customs" }
			});

			context.Response.Body = new MemoryStream();
			SetupRequestServices(context);

			await instance.InvokeAsync(context);
			var actualResponseMessage = await ReadResponseAsync(context);
			Assert.That(actualResponseMessage, Is.EqualTo(expectedTriggerMessage));
			mockNext.Verify(x => x.Invoke(context), Times.Never);
		}

		[Test]
		public async Task InvokeAsync_TriggerWithoutJobDataMap()
		{
			var context = new DefaultHttpContext();
			context.Request.Form = new FormCollection(new Dictionary<string, StringValues>
			{
				{ "command", "add_trigger" },
				{ "name", "Test_Trigger" },
				{ "job", "AU Exchange Rate Parser" },
				{ "triggerType", "Simple" },
				{ "group", "AU Customs" },
				{ "repeatCount", "1" },
				{ "repeatInterval", "1" }
			});
			SetupRequestServices(context);
			await instance.InvokeAsync(context);
			mockNext.Verify(x => x.Invoke(context), Times.Once);
		}

		[Test]
		public async Task InvokeAsync_TriggerWithNewScheduler()
		{
			var context = new DefaultHttpContext();
			context.Request.Form = new FormCollection(new Dictionary<string, StringValues>
			{
				{ "command", "add_trigger" },
				{ "name", "Test_Trigger" },
				{ "job", "Job_One" },
				{ "triggerType", "Simple" },
				{ "group", "AU Customs" }
			});

			context.Response.Body = new MemoryStream();
			SetupRequestServices(context);

			await instance.InvokeAsync(context);
			var actualResponseMessage = await ReadResponseAsync(context);
			Assert.That(actualResponseMessage, Is.EqualTo(expectedScheduleMessage));
			mockNext.Verify(x => x.Invoke(context), Times.Never);
		}

		[Test]
		public async Task InvokeAsync_NotAllowedEndpoint()
		{
			var context = new DefaultHttpContext();
			context.Request.Form = new FormCollection(new Dictionary<string, StringValues>
			{
				{ "command", "standby_scheduler" }
			});

			context.Response.Body = new MemoryStream();
			SetupRequestServices(context);

			await instance.InvokeAsync(context);
			var actualResponseMessage = await ReadResponseAsync(context);
			Assert.That(actualResponseMessage, Is.EqualTo(expectedNotAllowedEndPointMessage));
			mockNext.Verify(x => x.Invoke(context), Times.Never);
		}

		[Test]
		public async Task InvokeAsync_AllowedEndpoint()
		{
			var context = new DefaultHttpContext();
			context.Request.Form = new FormCollection(new Dictionary<string, StringValues>
			{
				{ "command", "get_job_Details" }
			});
			SetupRequestServices(context);

			await instance.InvokeAsync(context);
			mockNext.Verify(x => x.Invoke(context), Times.Once);
		}

		async Task<string> ReadResponseAsync(HttpContext context)
		{
			context.Response.Body.Seek(0, SeekOrigin.Begin);
			using (var reader = new StreamReader(context.Response.Body, Encoding.UTF8))
			{
				return await reader.ReadToEndAsync();
			}
		}

		[Test]
		public async Task InvokeAsync_InvalidRepeatCount()
		{
			var context = new DefaultHttpContext();
			context.Request.Form = new FormCollection(new Dictionary<string, StringValues>
			{
				{ "command", "add_trigger" },
				{ "name", "Test_Trigger" },
				{ "job", "AU Exchange Rate Parser" },
				{ "triggerType", "Simple" },
				{ "group", "AU Customs" },
				{ "repeatCount", "a" },
				{ "repeatInterval", "1" }
			});

			context.Response.Body = new MemoryStream();
			SetupRequestServices(context);

			await instance.InvokeAsync(context);
			var actualResponseMessage = await ReadResponseAsync(context);
			Assert.That(actualResponseMessage, Is.EqualTo(expectedInvalidDataExpression));
			mockNext.Verify(x => x.Invoke(context), Times.Never);
		}

		[Test]
		public async Task InvokeAsync_InvalidRepeatInterval()
		{
			var context = new DefaultHttpContext();
			context.Request.Form = new FormCollection(new Dictionary<string, StringValues>
			{
				{ "command", "add_trigger" },
				{ "name", "Test_Trigger" },
				{ "job", "AU Exchange Rate Parser" },
				{ "triggerType", "Simple" },
				{ "group", "AU Customs" },
				{ "repeatCount", "1" },
				{ "repeatInterval", "a" }
			});

			context.Response.Body = new MemoryStream();
			SetupRequestServices(context);

			await instance.InvokeAsync(context);
			var actualResponseMessage = await ReadResponseAsync(context);
			Assert.That(actualResponseMessage, Is.EqualTo(expectedInvalidDataExpression));
			mockNext.Verify(x => x.Invoke(context), Times.Never);
		}

		[Test]
		public async Task InvokeAsync_InvalidCronExpression()
		{
			var context = new DefaultHttpContext();
			context.Request.Form = new FormCollection(new Dictionary<string, StringValues>
			{
				{ "command", "add_trigger" },
				{ "name", "Test_Trigger" },
				{ "job", "AU Exchange Rate Parser" },
				{ "triggerType", "Cron" },
				{ "group", "AU Customs" },
				{ "repeatCount", "1" },
				{ "repeatInterval", "1" },
				{ "cronExpression", "a" }
			});

			context.Response.Body = new MemoryStream();
			SetupRequestServices(context);

			await instance.InvokeAsync(context);
			var actualResponseMessage = await ReadResponseAsync(context);
			Assert.That(actualResponseMessage, Is.EqualTo(expectedInvalidCornExpression));
			mockNext.Verify(x => x.Invoke(context), Times.Never);
		}

		[Test]
		public async Task InvokeAsync_InvalidTriggerName()
		{
			var context = new DefaultHttpContext();
			context.Request.Form = new FormCollection(new Dictionary<string, StringValues>
			{
				{ "command", "add_trigger" },
				{ "name", "test<img src = x onerror=sonsole.log('trigger')>" },
				{ "job", "AU Exchange Rate Parser" },
				{ "triggerType", "Simple" },
				{ "group", "AU Customs" },
				{ "repeatCount", "1" },
				{ "repeatInterval", "1" },
			});

			context.Response.Body = new MemoryStream();
			SetupRequestServices(context);

			await instance.InvokeAsync(context);
			var actualResponseMessage = await ReadResponseAsync(context);
			Assert.That(actualResponseMessage, Is.EqualTo(expectedTriggerNameMessage));
			mockNext.Verify(x => x.Invoke(context), Times.Never);
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
			mockContextAccessor = new Mock<IHttpContextAccessor>();
			var mockStagingRepo = new Mock<IStagingRepository>();
			var mockResult = new List<QRTZ_JOB_DETAILS>() {
				new QRTZ_JOB_DETAILS
				{
				JOB_PK = Guid.NewGuid(),
				JOB_NAME = "AU Exchange Rate Parser",
				JOB_GROUP = "AU Customs"
				}};
			mockStagingRepo.Setup(o => o.Get<QRTZ_JOB_DETAILS>()).Returns(mockResult.AsQueryable());
			schedulerValidator = new SchedulerValidator(mockStagingRepo.Object);
			mockNext = new Mock<RequestDelegate>();
			instance = new TriggerSchedulerMiddleware(mockNext.Object, schedulerValidator);
		}

		Mock<IHttpContextAccessor> mockContextAccessor;
		ISchedulerValidator schedulerValidator;
		Mock<RequestDelegate> mockNext;
		TriggerSchedulerMiddleware instance;
	}
}

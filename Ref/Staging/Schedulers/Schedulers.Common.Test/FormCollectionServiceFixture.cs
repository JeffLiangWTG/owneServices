using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Schedulers.Common.Test
{
	[TestFixture]
	class FormCollectionServiceFixture
	{
		[Test]
		public void GetValue()
		{
			Assert.IsNull(formCollectionService.GetValue("command"));

			context.Request.Form = new FormCollection(new Dictionary<string, StringValues>
			{
				{ "command", "get_data" },
				{ "job", "AA" }
			});
			mockContextAccessor.Setup(x => x.HttpContext).Returns(context);
			formCollectionService = new FormCollectionService(mockContextAccessor.Object);
			var value = formCollectionService.GetValue("command");
			Assert.AreEqual("get_data", value);
			value = formCollectionService.GetValue("job");
			Assert.AreEqual("AA", value);
			Assert.IsEmpty(formCollectionService.GetValue("not exist"));
		}

		[Test]
		public void CheckProperties()
		{
			Assert.IsNull(formCollectionService.Command);
			Assert.IsNull(formCollectionService.Job);
			Assert.IsNull(formCollectionService.Group);
			Assert.IsNull(formCollectionService.Name);
			Assert.IsNull(formCollectionService.Trigger);
			Assert.IsNull(formCollectionService.TriggerType);
			Assert.IsNull(formCollectionService.CronExpression);
			Assert.IsNull(formCollectionService.RepeatCount);
			Assert.IsNull(formCollectionService.RepeatInterval);
			Assert.IsNull(formCollectionService.RepeatForever);
			Assert.IsNull(formCollectionService.Keys);

			context.Request.Form = new FormCollection(new Dictionary<string, StringValues>
			{
				{ "command", "get_data" },
				{ "job", "AA" },
				{ "group", "BB" },
				{ "name", "CC" },
				{ "trigger", "DD" },
				{ "triggerType", "Simple" },
				{ "cronExpression", "" },
				{ "repeatCount", "10" },
				{ "repeatInterval", "1000" },
				{ "repeatForever", "false" }
			});
			mockContextAccessor.Setup(x => x.HttpContext).Returns(context);
			formCollectionService = new FormCollectionService(mockContextAccessor.Object);

			Assert.AreEqual("get_data", formCollectionService.Command);
			Assert.AreEqual("AA", formCollectionService.Job);
			Assert.AreEqual("BB", formCollectionService.Group);
			Assert.AreEqual("CC", formCollectionService.Name);
			Assert.AreEqual("DD", formCollectionService.Trigger);
			Assert.AreEqual("Simple", formCollectionService.TriggerType);
			Assert.AreEqual("", formCollectionService.CronExpression);
			Assert.AreEqual("10", formCollectionService.RepeatCount);
			Assert.AreEqual("1000", formCollectionService.RepeatInterval);
			Assert.AreEqual("false", formCollectionService.RepeatForever);
			Assert.AreEqual(10, formCollectionService.Keys.Count);
		}

		[Test]
		public void IsGetCommand()
		{
			Assert.AreEqual(false, formCollectionService.IsGetCommand);
			context.Request.Form = new FormCollection(new Dictionary<string, StringValues>
			{
				{ "command", "execute_job" },
			});
			mockContextAccessor.Setup(x => x.HttpContext).Returns(context);
			formCollectionService = new FormCollectionService(mockContextAccessor.Object);
			Assert.AreEqual(false, formCollectionService.IsGetCommand);

			context.Request.Form = new FormCollection(new Dictionary<string, StringValues>
			{
				{ "command", "get_data" },
			});
			mockContextAccessor.Setup(x => x.HttpContext).Returns(context);
			formCollectionService = new FormCollectionService(mockContextAccessor.Object);
			Assert.AreEqual(true, formCollectionService.IsGetCommand);
		}

		[Test]
		public void IsAddTriggerCommand()
		{
			Assert.AreEqual(false, formCollectionService.IsAddTriggerCommand);
			context.Request.Form = new FormCollection(new Dictionary<string, StringValues>
			{
				{ "command", "get_data" },
			});
			mockContextAccessor.Setup(x => x.HttpContext).Returns(context);
			formCollectionService = new FormCollectionService(mockContextAccessor.Object);
			Assert.AreEqual(false, formCollectionService.IsAddTriggerCommand);

			context.Request.Form = new FormCollection(new Dictionary<string, StringValues>
			{
				{ "command", "add_trigger" },
			});
			mockContextAccessor.Setup(x => x.HttpContext).Returns(context);
			formCollectionService = new FormCollectionService(mockContextAccessor.Object);
			Assert.AreEqual(true, formCollectionService.IsAddTriggerCommand);
		}

		[Test]
		public void IsTriggerRelatedCommand()
		{
			Assert.AreEqual(false, formCollectionService.IsTriggerRelatedCommand);
			context.Request.Form = new FormCollection(new Dictionary<string, StringValues>
			{
				{ "command", "get_data" },
			});
			mockContextAccessor.Setup(x => x.HttpContext).Returns(context);
			formCollectionService = new FormCollectionService(mockContextAccessor.Object);
			Assert.AreEqual(false, formCollectionService.IsTriggerRelatedCommand);

			context.Request.Form = new FormCollection(new Dictionary<string, StringValues>
			{
				{ "command", "pause_trigger" },
			});
			mockContextAccessor.Setup(x => x.HttpContext).Returns(context);
			formCollectionService = new FormCollectionService(mockContextAccessor.Object);
			Assert.AreEqual(true, formCollectionService.IsTriggerRelatedCommand);
		}

		[Test]
		public void IsSchedulerOrGroupRelatedCommand()
		{
			Assert.AreEqual(false, formCollectionService.IsSchedulerOrGroupRelatedCommand);
			context.Request.Form = new FormCollection(new Dictionary<string, StringValues>
			{
				{ "command", "get_data" },
			});
			mockContextAccessor.Setup(x => x.HttpContext).Returns(context);
			formCollectionService = new FormCollectionService(mockContextAccessor.Object);
			Assert.AreEqual(false, formCollectionService.IsSchedulerOrGroupRelatedCommand);

			context.Request.Form = new FormCollection(new Dictionary<string, StringValues>
			{
				{ "command", "pause_scheduler" },
			});
			mockContextAccessor.Setup(x => x.HttpContext).Returns(context);
			formCollectionService = new FormCollectionService(mockContextAccessor.Object);
			Assert.AreEqual(true, formCollectionService.IsSchedulerOrGroupRelatedCommand);

			context.Request.Form = new FormCollection(new Dictionary<string, StringValues>
			{
				{ "command", "resume_group" },
			});
			mockContextAccessor.Setup(x => x.HttpContext).Returns(context);
			formCollectionService = new FormCollectionService(mockContextAccessor.Object);
			Assert.AreEqual(true, formCollectionService.IsSchedulerOrGroupRelatedCommand);
		}

		[TestCase("get_data", true)]
		[TestCase("delete_trigger", true)]
		[TestCase("delete_job", true)]
		[TestCase("execute_job", true)]
		[TestCase("pause_trigger", true)]
		[TestCase("pause_group", true)]
		[TestCase("resume_trigger", true)]
		[TestCase("resume_group", true)]
		[TestCase("delete_group", false)]
		[TestCase("pause_scheduler", false)]
		[TestCase("resume_scheduler", false)]
		[TestCase("stop_scheduler", false)]
		public void IsAllowedCommand(string command, bool isAllowed)
		{
			context.Request.Form = new FormCollection(new Dictionary<string, StringValues>
			{
				{ "command", command },
			});
			mockContextAccessor.Setup(x => x.HttpContext).Returns(context);
			formCollectionService = new FormCollectionService(mockContextAccessor.Object);
			Assert.AreEqual(isAllowed, formCollectionService.IsAllowedCommand);
		}

		[SetUp]
		public void SetUp()
		{
			context = new DefaultHttpContext();
			mockContextAccessor = new Mock<IHttpContextAccessor>();
			mockContextAccessor.Setup(x => x.HttpContext).Returns(context);
			formCollectionService = new FormCollectionService(mockContextAccessor.Object);
		}

		HttpContext context;
		Mock<IHttpContextAccessor> mockContextAccessor;
		FormCollectionService formCollectionService;
	}
}

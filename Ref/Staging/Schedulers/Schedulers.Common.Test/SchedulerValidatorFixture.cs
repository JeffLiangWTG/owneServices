using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Schedulers.Common.Test
{
	[TestFixture]
	public class SchedulerValidatorFixture
	{
		[Test]
		public void IsANewScheduleJob_ExistingJobAndGroup()
		{
			var isNewScheduleJob = schedulerValidator.IsNewScheduleJob("ZZ Tariff Parser Test", "AU Customs");
			Assert.That(isNewScheduleJob, Is.False);
		}

		[Test]
		public void IsANewScheduleJob_ExistingJobAndGroupInDifferentObjects()
		{
			var isNewScheduleJob = schedulerValidator.IsNewScheduleJob("AA Tariff Parser Test1", "AU Customs");
			Assert.That(isNewScheduleJob, Is.True);
		}

		[Test]
		public void IsANewScheduleJob_ExistingJob_NoGroup()
		{
			var isNewScheduleJob = schedulerValidator.IsNewScheduleJob("ZZ Tariff Parser Test", "AU Customs Not Exist");
			Assert.That(isNewScheduleJob, Is.True);
		}

		[Test]
		public void IsANewScheduleJob_ExistingGroup_NoJob()
		{
			var isNewScheduleJob = schedulerValidator.IsNewScheduleJob("Tariff Parser Test Not Exist", "AU Customs");
			Assert.That(isNewScheduleJob, Is.True);
		}

		[Test]
		public void IsANewScheduleJob_NoJobAndGroup()
		{
			var isNewScheduleJob = schedulerValidator.IsNewScheduleJob("Tariff Parser Test", "New AU Customs Test");
			Assert.That(isNewScheduleJob, Is.True);
		}

		[Test]
		public void Validate_LostData()
		{
			var formCollection = new FormCollection(new Dictionary<string, StringValues>
			{
				{ "command", "add_trigger" },
				{ "name", "Test_Trigger" },
				{ "job", "AU Exchange Rate Parser" },
				{ "triggerType", "Simple" },
				{ "group", "AU Customs" }
			});
			SetupFormCollection(formCollection);

			var isValid = schedulerValidator.Validate(formCollectionService, out var errorMessage);
			Assert.That(isValid, Is.False);
			Assert.That(errorMessage, Is.EqualTo("{ \"_err\":\"Invalid RepeatCount or RepeatInterval.\"}"));
		}

		[Test]
		public void Validate_NewTriggerWithJobDataMap()
		{
			var formCollection = new FormCollection(new Dictionary<string, StringValues>
			{
				{ "command", "add_trigger" },
				{ "name", "Test_Trigger" },
				{ "job", "AU Exchange Rate Parser" },
				{ "triggerType", "Simple" },
				{ "jobDataMap[0].Key", "Data_one" },
				{ "jobDataMap[0].Value", "Data_value_one" },
				{ "group", "AU Customs" }
			});
			SetupFormCollection(formCollection);

			var isValid = schedulerValidator.Validate(formCollectionService, out var errorMessage);
			Assert.That(isValid, Is.False);
			Assert.That(errorMessage, Is.EqualTo("{ \"_err\":\"Triggers with JobDataMap are not allowed.\"}"));
		}

		[Test]
		public void Validate_AddTriggerForNewJob()
		{
			var formCollection = new FormCollection(new Dictionary<string, StringValues>
			{
				{ "command", "add_trigger" },
				{ "name", "Test_Trigger" },
				{ "job", "Job_One" },
				{ "triggerType", "Simple" },
				{ "group", "AU Customs" }
			});
			SetupFormCollection(formCollection);

			var isValid = schedulerValidator.Validate(formCollectionService, out var errorMessage);
			Assert.That(isValid, Is.False);
			Assert.That(errorMessage, Is.EqualTo("{ \"_err\":\"New Schedule jobs are not allowed.\"}"));
		}

		[TestCase(null, true)]
		[TestCase(" 0 0 18 L * ?", true)]
		[TestCase("18:00 Evey Day", true)]
		[TestCase("007ae79b-ef57-4eaf-a5bb-a88ef4b4b182", true)]
		[TestCase("trigger1%", false)]
		[TestCase("<a href= />trigger2", false)]
		public void Validate_TriggerName(string triggerName, bool isValid)
		{
			var collectionDictionary = new Dictionary<string, StringValues>()
			{
				{ "command", "add_trigger" },
				{ "job", "ZZ Tariff Parser Test" },
				{ "group", "AU Customs" },
				{ "triggerType", "Simple" },
				{ "repeatCount", "1" },
				{ "repeatInterval", "1000" }
			};
			if (!string.IsNullOrEmpty(triggerName))
			{
				collectionDictionary["name"] = triggerName;
			}

			var formCollection = new FormCollection(collectionDictionary);
			SetupFormCollection(formCollection);

			var result = false;
			var errorMessage = string.Empty;
			Assert.DoesNotThrow(() => result = schedulerValidator.Validate(formCollectionService, out errorMessage));
			Assert.AreEqual(isValid, result);
			if (isValid)
			{
				Assert.True(string.IsNullOrEmpty(errorMessage));
			}
			else
			{
				Assert.AreEqual("{ \"_err\":\"Invalid Trigger Name.\"}", errorMessage);
			}
		}

		[Test]
		public void Validate_NotAllowedAction()
		{
			var formCollection = new FormCollection(new Dictionary<string, StringValues>
			{
				{ "command", "standby_scheduler" }
			});
			SetupFormCollection(formCollection);

			var isValid = schedulerValidator.Validate(formCollectionService, out var errorMessage);
			Assert.That(!isValid);
			Assert.That(errorMessage, Is.EqualTo("{ \"_err\":\"Action is not allowed.\"}"));
		}

		[Test]
		public void Validate_AllowedAction()
		{
			var formCollection = new FormCollection(new Dictionary<string, StringValues>
			{
				{ "command", "get_job_Details" }
			});
			SetupFormCollection(formCollection);

			var isValid = schedulerValidator.Validate(formCollectionService, out var errorMessage);
			Assert.That(isValid, Is.True);
			Assert.That(errorMessage, Is.Empty);
		}

		[Test]
		public void Validate_RepeatCount()
		{
			var formDictionary = new Dictionary<string, StringValues>
			{
				{ "command", "add_trigger" },
				{ "name", "Test_Trigger" },
				{ "job", "AU Exchange Rate Parser" },
				{ "triggerType", "Simple" },
				{ "group", "AU Customs" },
				{ "repeatCount", "a" },
				{ "repeatInterval", "1" }
			};

			var formCollection = new FormCollection(formDictionary);
			SetupFormCollection(formCollection);

			var isValid = schedulerValidator.Validate(formCollectionService, out var errorMessage);
			Assert.That(isValid, Is.False);
			Assert.That(errorMessage, Is.EqualTo("{ \"_err\":\"Invalid RepeatCount or RepeatInterval.\"}"));

			formDictionary["repeatCount"] = "-1";
			formCollection = new FormCollection(formDictionary);
			SetupFormCollection(formCollection);
			isValid = schedulerValidator.Validate(formCollectionService, out errorMessage);
			Assert.That(isValid, Is.False);
			Assert.That(errorMessage, Is.EqualTo("{ \"_err\":\"Invalid RepeatCount or RepeatInterval.\"}"));

			formDictionary["repeatCount"] = "1";
			formCollection = new FormCollection(formDictionary);
			SetupFormCollection(formCollection);
			isValid = schedulerValidator.Validate(formCollectionService, out errorMessage);
			Assert.That(isValid, Is.True);
			Assert.That(errorMessage, Is.Empty);

			formDictionary["repeatCount"] = "a";
			formDictionary["repeatForever"] = "false";
			formCollection = new FormCollection(formDictionary);
			SetupFormCollection(formCollection);
			isValid = schedulerValidator.Validate(formCollectionService, out errorMessage);
			Assert.That(isValid, Is.False);
			Assert.That(errorMessage, Is.EqualTo("{ \"_err\":\"Invalid RepeatCount or RepeatInterval.\"}"));

			formDictionary["repeatForever"] = "true";
			formCollection = new FormCollection(formDictionary);
			SetupFormCollection(formCollection);
			isValid = schedulerValidator.Validate(formCollectionService, out errorMessage);
			Assert.That(isValid, Is.True);
			Assert.That(errorMessage, Is.Empty);
		}

		[Test]
		public void Validate_RepeatInterval()
		{
			var formDictionary = new Dictionary<string, StringValues>
			{
				{ "command", "add_trigger" },
				{ "name", "Test_Trigger" },
				{ "job", "AU Exchange Rate Parser" },
				{ "triggerType", "Simple" },
				{ "group", "AU Customs" },
				{ "repeatCount", "1" },
				{ "repeatInterval", "a" }
			};

			var formCollection = new FormCollection(formDictionary);
			SetupFormCollection(formCollection);
			var isValid = schedulerValidator.Validate(formCollectionService, out var errorMessage);
			Assert.That(isValid, Is.False);
			Assert.That(errorMessage, Is.EqualTo("{ \"_err\":\"Invalid RepeatCount or RepeatInterval.\"}"));

			formDictionary["repeatInterval"] = "-1";
			formCollection = new FormCollection(formDictionary);
			SetupFormCollection(formCollection);
			isValid = schedulerValidator.Validate(formCollectionService, out errorMessage);
			Assert.That(isValid, Is.False);
			Assert.That(errorMessage, Is.EqualTo("{ \"_err\":\"Invalid RepeatCount or RepeatInterval.\"}"));

			formDictionary["repeatInterval"] = "1";
			formCollection = new FormCollection(formDictionary);
			SetupFormCollection(formCollection);
			isValid = schedulerValidator.Validate(formCollectionService, out errorMessage);
			Assert.That(isValid, Is.True);
			Assert.That(errorMessage, Is.Empty);
		}

		[Test]
		public void Validate_InvalidCronExpression()
		{
			var formCollection = new FormCollection(new Dictionary<string, StringValues>
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
			SetupFormCollection(formCollection);

			var isValid = schedulerValidator.Validate(formCollectionService, out var errorMessage);
			Assert.That(isValid, Is.False);
			Assert.That(errorMessage, Is.EqualTo("{ \"_err\":\"Invalid Cron Expression.\"}"));
		}

		void SetupFormCollection(IFormCollection formCollection)
		{
			mockRequest.Setup(x => x.ReadFormAsync(CancellationToken.None)).Returns(Task.FromResult(formCollection));
			mockContext.Setup(x => x.Request).Returns(mockRequest.Object);
			contextAccessor.Setup(x => x.HttpContext).Returns(mockContext.Object);
			formCollectionService = new FormCollectionService(contextAccessor.Object);
		}

		[SetUp]
		public void Setup()
		{
			stagingRepo = new Mock<IStagingRepository>();
			stagingRepo.Setup(x => x.Get<QRTZ_JOB_DETAILS>()).Returns(triggers.AsQueryable());
			schedulerValidator = new SchedulerValidator(stagingRepo.Object);

			mockRequest = new Mock<HttpRequest>();
			mockRequest.Setup(x => x.HasFormContentType).Returns(true);
			mockContext = new Mock<HttpContext>();
			mockContext.Setup(x => x.Request).Returns(mockRequest.Object);
			contextAccessor = new Mock<IHttpContextAccessor>();
			contextAccessor.Setup(x => x.HttpContext).Returns(mockContext.Object);
			formCollectionService = new FormCollectionService(contextAccessor.Object);
		}

		Mock<IStagingRepository> stagingRepo;
		Mock<HttpContext> mockContext;
		Mock<HttpRequest> mockRequest;
		Mock<IHttpContextAccessor> contextAccessor;
		IFormCollectionService formCollectionService;
		SchedulerValidator schedulerValidator;

		readonly QRTZ_JOB_DETAILS[] triggers = new[]
		{
			new QRTZ_JOB_DETAILS { JOB_NAME = "ZZ Tariff Parser Test", SCHED_NAME = "RefDbRepoQuartzServer", JOB_GROUP = "AU Customs" },
			new QRTZ_JOB_DETAILS { JOB_NAME = "AA Tariff Parser Test1", SCHED_NAME = "RefDbRepoQuartzServer1", JOB_GROUP = "AU Customs AHECC Parser" },
			new QRTZ_JOB_DETAILS{JOB_NAME = "AU Exchange Rate Parser", JOB_GROUP = "AU Customs" }
		};
	}
}

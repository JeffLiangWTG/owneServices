using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Common.Web.Auth;
using CargoWise.RefDbRepo.Staging.Common;
using CargoWise.RefDbRepo.Staging.NewService.Controllers;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.NewService.Test
{
	[TestFixture]
	class QuartzJobGroupHelperFixture
	{
		[TestCase(null, null, false)]
		[TestCase("", "", false)]
		[TestCase("", "CargoWise.RefDbRepo.AUReferenceData.CmdLine.exe.config", false)]
		[TestCase("AU Customs", "", false)]
		[TestCase("ABC Customs", "CargoWise.RefDbRepo.AUReferenceData.CmdLine.exe.config", false)]
		[TestCase("AU Customs", "CargoWise.RefDbRepo.AUReferenceData.CmdLine.exe.config", true)]
		[TestCase("AU Customs", "CargoWise.RefDbRepo.AUReferenceData.CmdLine.config.json", true)]
		public void IsValidJobGroup(string jobGroup, string configFilePath, bool isValid)
		{
			var jobDetails = new QRTZ_JOB_DETAILS
			{
				JOB_PK = Guid.Parse("BB7584FC-3DCB-4B59-B4A9-3E619DD7D733"),
				SCHED_NAME = "RefDbRepoQuartzServer",
				JOB_NAME = "AU Tariff",
				JOB_GROUP = "AU Customs",
				CountryCode = "AU",
				ProgramExePath = @"..\..\UniversalXMLProducers\net8.0\CargoWise.RefDbRepo.AUReferenceData.CmdLine.exe"
			};
			var stagingRepo = new Mock<IStagingRepository>();
			stagingRepo.Setup(x => x.Get<QRTZ_JOB_DETAILS>()).Returns(new[] { jobDetails }.AsQueryable());
			var updateController = new QRTZ_JOB_DETAILSUpdateController(null) { ControllerContext = IntegrationTestHelper.SetupControllerContext(stagingRepo.Object) };
			var result = QuartzJobGroupHelper.IsValidJobGroup(updateController, jobGroup, configFilePath);
			Assert.AreEqual(isValid, result);
		}

		[Test, TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task IsValidJobGroup_DoesNotThrowException()
		{
			await InsertJobDetailsToDb();
			using (var stagingRepo = new StagingRepository(connectionString))
			{
				var controller = new QRTZ_JOB_DETAILSUpdateController(authorizationHelper.Object);
				controller.ControllerContext = IntegrationTestHelper.SetupControllerContext(stagingRepo);
				var configFilePath = "AAACmdLine.config.json";
				var jobGroups = controller.Get().ToList().Select(x => x.JOB_GROUP).Distinct();
				foreach (var jobGroup in jobGroups)
				{
					Assert.DoesNotThrow(() => QuartzJobGroupHelper.IsValidJobGroup(controller, jobGroup, configFilePath));
				}
			}
		}

		async Task InsertJobDetailsToDb()
		{
			var configurationFolder = Path.Combine(AppContext.BaseDirectory, @"..\..\Staging\net8.0\Configuration");
			var quartzXmlFiles = Directory.GetFiles(configurationFolder, "*.xml");
			var jobDetails = await QRTZ_JOB_DETAILSHelper.GetJobDetailsFromXmlFiles(quartzXmlFiles);
			using (var stagingRepo = new StagingRepository(connectionString))
			{
				foreach (var jobDetail in jobDetails)
				{
					var quartzJob = new QRTZ_JOB_DETAILS
					{
						JOB_PK = Guid.NewGuid(),
						JOB_NAME = jobDetail.name,
						JOB_GROUP = jobDetail.group,
						JOB_CLASS_NAME = jobDetail.jobtype,
						IS_DURABLE = jobDetail.durable,
						REQUESTS_RECOVERY = jobDetail.recover,
						SCHED_NAME = "RefDbRepoQuartzServer",
						JOB_DATA = QRTZ_JOB_DETAILSHelper.SerializeToJobData(jobDetail.jobdatamap?.entry?.ToDictionary(e => e.key, e => e.value))
					};

					stagingRepo.Add(quartzJob);
				}
				await stagingRepo.SaveChangesAsync();
			}
		}

		[SetUp]
		public void SetUp()
		{
			dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
			connectionString = TestConnectionString.GetAdmin(dbName);
		}

		string dbName;
		string connectionString;
		readonly Mock<IAuthorizationHelper> authorizationHelper = new Mock<IAuthorizationHelper>();
	}
}

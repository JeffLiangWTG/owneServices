using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Common.Web.Auth;
using CargoWise.RefDbRepo.Staging.NewService.Controllers;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.NewService.Test
{
	class QRTZ_JOB_DETAILSUpdateControllerFixture
	{
		[Test]
		public void Get()
		{
			var job = GetQrtzJobDetails(jobDataStr: JobDataStr);
			var stagingRepo = new Mock<IStagingRepository>();
			stagingRepo.Setup(x => x.Get<QRTZ_JOB_DETAILS>()).Returns(new[] { job }.AsQueryable());
			var controller = GetQrtzJobDetailsUpdateController(stagingRepo.Object);
			var result = controller.Get().First();
			Assert.NotNull(result);
			Assert.AreEqual(COUNTRYCODE, result.CountryCode);
			Assert.AreEqual(PROGRAMARGS, result.ProgramArgs);
			Assert.AreEqual(PROGRAMEXEPATH, result.ProgramExePath);
		}

		[Test]
		public void GetByPK()
		{
			var job = GetQrtzJobDetails(jobDataStr: JobDataStr);
			var stagingRepo = new Mock<IStagingRepository>();
			stagingRepo.Setup(x => x.Get<QRTZ_JOB_DETAILS>()).Returns(new[] { job }.AsQueryable());
			var controller = GetQrtzJobDetailsUpdateController(stagingRepo.Object);
			var result = controller.Get(jobPK).First();
			Assert.NotNull(result);
			Assert.AreEqual(COUNTRYCODE, result.CountryCode);
			Assert.AreEqual(PROGRAMARGS, result.ProgramArgs);
			Assert.AreEqual(PROGRAMEXEPATH, result.ProgramExePath);
		}

		[Test, TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public void QrtzJobShouldNotUpdatedAfterSettingCalculatedProperties()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
			var connectionString = TestConnectionString.GetAdmin(dbName);
			using (var stagingRepo = new StagingRepository(connectionString))
			{
				var qrtzJob = GetQrtzJobDetails(jobDataStr: JobDataStr);
				stagingRepo.Add(qrtzJob);
				stagingRepo.SaveChanges();
			}

			using (var stagingRepo = new StagingRepository(connectionString))
			{
				var controller = GetQrtzJobDetailsUpdateController(stagingRepo);
				controller.Get();
				var affectedRecords = stagingRepo.SaveChanges();
				Assert.AreEqual(0, affectedRecords);
			}

			using (var stagingRepo = new StagingRepository(connectionString))
			{
				var controller = GetQrtzJobDetailsUpdateController(stagingRepo);
				var result = controller.Get().ToArray();
				Assert.AreEqual(1, result.Length);
				Assert.NotNull(result[0].JOB_DATA);
			}
		}

		QRTZ_JOB_DETAILSUpdateController GetQrtzJobDetailsUpdateController(IStagingRepository stagingRepo)
		{
			var controller = new QRTZ_JOB_DETAILSUpdateController(authorizationHelper.Object);
			controller.ControllerContext = IntegrationTestHelper.SetupControllerContext(stagingRepo);
			return controller;
		}

		QRTZ_JOB_DETAILS GetQrtzJobDetails(string countryCode = null, string programArgs = null, string programExePath = null, string jobDataStr = null)
		{
			byte[] jobData = null;
			if (!string.IsNullOrEmpty(jobDataStr))
			{
				jobData = Convert.FromHexString(jobDataStr);
			}

			var job = new QRTZ_JOB_DETAILS
			{
				JOB_PK = jobPK,
				SCHED_NAME = "Quartz",
				JOB_NAME = "Test",
				JOB_GROUP = "Test Group",
				JOB_CLASS_NAME = "Test Class",
				JOB_DATA = jobData,
				CountryCode = countryCode,
				ProgramArgs = programArgs,
				ProgramExePath = programExePath
			};
			return job;
		}

		readonly Mock<IAuthorizationHelper> authorizationHelper = new Mock<IAuthorizationHelper>();
		readonly Guid jobPK = Guid.NewGuid();
		const string COUNTRYCODE = "EUN";
		const string PROGRAMARGS = "NOMENCLATURE";
		const string PROGRAMEXEPATH = @"..\UniversalXMLProducers\CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.exe";
		const string JobDataStr = @"7B22436F756E747279436F6465223A2245554E222C2250726F6772616D45786550617468223A222E2E5C5C556E6976657273616C584D4C50726F6475636572735C5C436172676F576973652E52656644625265706F2E556E6976657273616C584D4C50726F6475636572732E45554E5461726966664461746150726F64756365722E657865222C2250726F6772616D41726773223A224E4F4D454E434C4154555245227D";
	}
}

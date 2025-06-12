using System;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.eServices.Billing.DataAccess;
using CargoWise.eServices.Billing.Tests.Common;
using CargoWise.eServices.Billing.WcfService.Hangfire.BillingDatabase;
using Castle.Windsor;
using Common.Logging;
using Hangfire.Server;
using Moq;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.WcfService.Tests
{
	public class BillingDatabaseProcessorTest
	{
		[Test]
		public void TestBillingDatabaseProcessor_Success()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "SYD");
				BillingDataTestHelper.AddDatabaseAndCompany(con, 2, 1, "SYD", enterpriseCode: "EDI");
				var transactionTime = DateTime.UtcNow;

				var trn1 = new CargoWise.Billing.API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "WUTCMPSVR",
					ClientNumber = "A",
					ClientStaffCode = "",
					Category = "STL",
					PriceItemCode = "SHP",
					Reference1 = "Ref 1",
					Reference2 = "Ref 2",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transactionTime,
					Version = 0,
				};

				var trn2 = new CargoWise.Billing.API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENT???SRV",
					ClientNumber = null,
					Category = "DOS",
					PriceItemCode = "IN1",
					Reference1 = "Invoice~EN-US",
					Reference2 = "71905906-e5f2-4a87-b5e1-a82593c58a08",
					Reference3 = "AccTransactionHeader",
					Reference4 = "Success",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transactionTime,
					Version = 1,
				};

				var trn3 = new CargoWise.Billing.API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "EDI???SRV",
					ClientNumber = null,
					Category = "DOS",
					PriceItemCode = "IN1",
					Reference1 = "Invoice~EN-US",
					Reference2 = "71905906-e5f2-4a87-b5e1-a82593c58a08",
					Reference3 = "AccTransactionHeader",
					Reference4 = "Success",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transactionTime,
					Version = 1,
				};

				BillingDataTestHelper.AddTransaction(trn1);
				BillingDataTestHelper.AddTransaction(trn2);
				BillingDataTestHelper.AddTransaction(trn3);


				var table = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				var stagingTable = BillingDataTestHelper.LoadTableByName(con, "dbo.Staging");

				Assert.That(stagingTable.Rows.Count, Is.EqualTo(3), "Staging rows");
				Assert.That(table.Rows.Count, Is.EqualTo(0), "Chargeable rows");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(0), "Usage rows");

				// Not test server
				var billingRepository = BillingDataTestHelper.GetBillingRepository(logger: loggerMock.Object, logErrorsAsInfoMsgEvent: true);
				var processorMock = GetProcessor();
				processorMock.Setup(x => x.CreateRepository(It.IsAny<CancellationToken>())).Returns(billingRepository);

				processorMock.Object.StartProcess(CancellationToken.None, null);

				table = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				stagingTable = BillingDataTestHelper.LoadTableByName(con, "dbo.Staging");

				Assert.That(stagingTable.Rows.Count, Is.EqualTo(0), "Staging transactions did not get processed");
				Assert.That(table.Rows.Count, Is.EqualTo(1), "Sucessful records should be processed into Chargeable table");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(1), "Usage transactions should be processed into Usage table");

				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn1, expectedDatabaseNumber: 0, expectedCompanyNumber: 0, usageTable.Rows[0]);
				BillingDataTestHelper.AssertChargeablePropertiesAreEqual(trn2, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, table.Rows[0]);

				// Test server
				Task.Delay(500).Wait();
				BillingDataTestHelper.AddTransaction(trn3);
				billingRepository = BillingDataTestHelper.GetBillingRepository("TestBillingContext");
				processorMock.Setup(x => x.CreateRepository(It.IsAny<CancellationToken>())).Returns(billingRepository);
				processorMock.Object.StartProcess(CancellationToken.None, null);

				table = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				stagingTable = BillingDataTestHelper.LoadTableByName(con, "dbo.Staging");
				Assert.That(stagingTable.Rows.Count, Is.EqualTo(0), "Staging transactions did not get processed");
				Assert.That(table.Rows.Count, Is.EqualTo(2), "Transaction from internal system should be processed into Chargeable table in test server");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(1), "Usage transactions should be processed into Usage table");
				BillingDataTestHelper.AssertChargeablePropertiesAreEqual(trn3, expectedDatabaseNumber: 2, expectedCompanyNumber: 1, table.Rows[0]);
				AssertUpdateChargeableLogs();
			}
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void TestBillingDatabaseProcessor_ProduceMonthlyChargeableRecords(bool performMonthlyAggregation)
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "MEL", serverCode: "MEL", hostedLocation: "SYD");

				var now = DateTime.Now;

				var trn1 = new CargoWise.Billing.API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENTMELMEL",
					ClientNumber = null,
					Category = "STL",
					PriceItemCode = "TX2",
					Reference1 = "43518651",
					Reference2 = "8600284806",
					Reference3 = "CN",
					Reference4 = "INV",
					Reference5 = "2DA42803-3D2B-4B81-8A0B-0D626E339FF7",
					ReportingSource = "ENT",
					ServiceOccuredUTC = now.AddMonths(-1).ToUniversalTime(),
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trn1);

				var billingRepository = BillingDataTestHelper.GetBillingRepository(logger: loggerMock.Object, logErrorsAsInfoMsgEvent: true);
				var dtProviderMock = new Mock<IDateTimeProvider>();
				dtProviderMock.Setup(x => x.DateTimeNow).Returns(new DateTime(now.Year, now.Month, 1, 1, 0, 0, DateTimeKind.Local));
				var processorMock = GetProcessor(performMonthlyAggregation);
				processorMock.Setup(x => x.CreateRepository(It.IsAny<CancellationToken>())).Returns(billingRepository);
				processorMock.Setup(x => x.DateTimeProvider).Returns(dtProviderMock.Object);

				processorMock.Object.StartProcess(CancellationToken.None, null);

				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				var chargeableTable = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				var stagingTable = BillingDataTestHelper.LoadTableByName(con, "dbo.Staging");

				Assert.That(usageTable.Rows.Count, Is.EqualTo(1), "Records should be processed into Usage table");
				Assert.That(stagingTable.Rows.Count, Is.EqualTo(0), "No records should remain in the staging table");
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn1, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, usageTable.Rows[0]);
				if (!performMonthlyAggregation)
				{
					Assert.That(chargeableTable.Rows.Count, Is.EqualTo(0), "No records should be processed into Chargeable by the job");
				}
				else
				{
					Assert.That(chargeableTable.Rows.Count, Is.EqualTo(1), "Records should be processed into Chargeable by the job");
					BillingDataTestHelper.AssertChargeablePropertiesAreEqual(trn1, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, chargeableTable.Rows[0]);
				}
				AssertUpdateChargeableLogs(performMonthlyAggregation);
			}
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void TestBillingDatabaseProcessor_UpdateBillingCubePartial(bool updateBillingCubePartial)
		{
			var billingRepositoryMock = new Mock<IBillingRepository>();
			var processorMock = GetProcessor(false, updateBillingCubePartial);
			processorMock.Setup(x => x.CreateRepository(It.IsAny<CancellationToken>())).Returns(billingRepositoryMock.Object);
			processorMock.Object.StartProcess(CancellationToken.None, null);
			billingRepositoryMock.Verify(x => x.UpdateChargeable(It.IsAny<DateTime?>(), 202110, false, updateBillingCubePartial));
		}

		[Test]
		public void TestBillingDatabaseProcessor_Fail()
		{
			var exception = new Exception("Test exception");
			var billingRepository = new Mock<IBillingRepository>();
			billingRepository.Setup(x => x.UpdateChargeable(null, 202110, false, false)).Throws(exception);
			var processorMock = GetProcessor();
			processorMock.Setup(x => x.CreateRepository(It.IsAny<CancellationToken>())).Returns(billingRepository.Object);
			processorMock.Setup(x => x.DateTimeProvider).Returns((DateTimeProvider)null);

			var ex = Assert.Throws<Exception>(() => processorMock.Object.StartProcess(CancellationToken.None, null));
			Assert.AreEqual("Test exception", ex?.Message);
			loggerMock.Verify(x => x.Info("Start processing billing database"), Times.Once);
			loggerMock.Verify(x => x.Error("Reached maximum retries. Stop processing."), Times.Once);
			loggerMock.Verify(x => x.ErrorFormat("Error processing billing database. Retry", exception), Times.Exactly(4));
		}

		[SetUp]
		public void Setup()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.SetUpBillingRules(con);
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
			}

			loggerMock = new Mock<ILog>();
			configurationMock = new Mock<IConfigurationProvider>();
			configurationMock.Setup(x => x.BilledELKKafkaTopic).Returns("billed-topic");
			configurationMock.Setup(x => x.UsageELKKafkaTopic).Returns("usage-topic");
			configurationMock.Setup(x => x.MaxRetries).Returns(3);
			configurationMock.Setup(x => x.RetryTimeoutInMilliseconds).Returns(10);
			configurationMock.Setup(x => x.ELKResubmissionBatchSize).Returns(3);
			var containerMock = new Mock<IWindsorContainer>();
			containerMock.Setup(x => x.Resolve<IConfigurationProvider>()).Returns(configurationMock.Object);
			Global.WindsorContainer = containerMock.Object;
		}

		Mock<BillingDatabaseProcessor> GetProcessor(bool performMonthlyAggregation = false, bool updateBillingCubePartial = false, string loggerName = "")
		{
			var mock = new Mock<BillingDatabaseProcessor>(performMonthlyAggregation, updateBillingCubePartial, LogManager.GetLogger(loggerName)) { CallBase = true };
			mock.Setup(x => x.Logger).Returns(loggerMock.Object);
			mock.Setup(x => x.GetBackgroundJobId(It.IsAny<PerformContext>())).Returns(string.Empty);
			return mock;
		}

		Mock<IConfigurationProvider> configurationMock;
		Mock<ILog> loggerMock;

		void AssertUpdateChargeableLogs(bool performMonthlyAggregation = false, bool updateBillingCubePartial = false)
		{
			loggerMock.Verify(l => l.Info("Starting process staging"), Times.Once);
			loggerMock.Verify(l => l.Info("Process staging completed"), Times.Once);

			loggerMock.Verify(l => l.Info("Starting process first message"), Times.Once);
			loggerMock.Verify(l => l.Info("Process first message completed"), Times.Once);

			if (updateBillingCubePartial)
			{
				loggerMock.Verify(l => l.Info("Starting update billing cube"), Times.Once);
				loggerMock.Verify(l => l.Info("Update billing cube completed"), Times.Once);
			}

			if (performMonthlyAggregation)
			{
				loggerMock.Verify(l => l.Info("Starting produce aggregate monthly chargeables"), Times.Once);
				loggerMock.Verify(l => l.Info("Produce aggregate monthly chargeables completed"), Times.Once);
			}
		}

		SqlError GenerateSqlError()
		{
			var sqlErrorConstructors = typeof(SqlError).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
			var firstSqlErrorConstructor = sqlErrorConstructors.FirstOrDefault(x => x.GetParameters().Count() == 7);
			SqlError generatedError = firstSqlErrorConstructor.Invoke(new object[] { 1, new byte(), new byte(), string.Empty, "I'm a real exception!", string.Empty, new int() }) as SqlError;
			return generatedError;
		}
	}
}

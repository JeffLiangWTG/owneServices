using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using CargoWise.eServices.Billing.Tests.Common;
using Common.Logging;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using API = CargoWise.Billing.API;

namespace CargoWise.eServices.Billing.DataAccess.Tests
{
	[TestFixture]
	public class BillingRepositoryTest
	{
		[Test]
		public void TestAddTransaction()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);

				var transaction = GetValidTransaction();
				BillingDataTestHelper.AddTransaction(transaction);

				var actual = BillingDataTestHelper.LoadAllStagingTransactionsMostRecentFirst(con);

				Assert.That(actual.Rows.Count, Is.EqualTo(1));
				var transactionRow = actual.Rows[0];
				BillingDataTestHelper.AssertPropertiesAreEqual(transaction, transactionRow);
			}
		}

		[Test]
		public void TestSqlConnectionSettings()
		{
			Mock<ILog> loggerMock = new Mock<ILog>();
			using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["BillingContext"].ConnectionString))
			{
				new BillingRepository(() => con, loggerMock.Object, true, default).UpdateChargeable();
				Assert.AreEqual(true, con.FireInfoMessageEventOnUserErrors, "FireInfoMessageEventOnUserErrors should be true.");
				loggerMock.Verify(l => l.Info("Starting process staging"), Times.Once);
			}

			loggerMock.Reset();
			using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["BillingContext"].ConnectionString))
			{
				new BillingRepository(() => con, null, true, default).UpdateChargeable();
				Assert.AreEqual(false, con.FireInfoMessageEventOnUserErrors, "FireInfoMessageEventOnUserErrors should be false.");
				loggerMock.Verify(l => l.Info("Starting process staging"), Times.Never);
			}

			loggerMock.Reset();
			using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["BillingContext"].ConnectionString))
			{
				new BillingRepository(() => con, loggerMock.Object, false, default).UpdateChargeable();
				Assert.AreEqual(false, con.FireInfoMessageEventOnUserErrors, "FireInfoMessageEventOnUserErrors should be false.");
				loggerMock.Verify(l => l.Info("Starting process staging"), Times.Once);
			}

			loggerMock.Reset();
			using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["BillingContext"].ConnectionString))
			{
				new BillingRepository(() => con, null, false, default).UpdateChargeable();
				Assert.AreEqual(false, con.FireInfoMessageEventOnUserErrors, "FireInfoMessageEventOnUserErrors should be false.");
				loggerMock.Verify(l => l.Info("Starting process staging"), Times.Never);
			}
		}

		[Test]
		public void TestCorrectHandlingOfSqlError()
		{
			Mock<ILog> loggerMock = new Mock<ILog>();
			Exception ex = Assert.Throws<InvalidOperationException>(() => new BillingRepository(loggerMock.Object, true).UpdateChargeable(new DateTime(year: 1800, month: 1, day: 1)));
			Assert.That(ex.Message, Does.StartWith("System.Data.SqlClient.SqlError: The conversion of a datetime data type to a smalldatetime data type resulted in an out-of-range value."));

			using (var con = new SqlConnection("data source=invalid.com;initial catalog=CargoWise.eServices.Billing.UnitTesting;integrated security=True;Connection Timeout=1"))
			{
				ex = Assert.Throws<SqlException>(() => new BillingRepository(() => con, loggerMock.Object, true, default).UpdateChargeable(new DateTime(year: 1800, month: 1, day: 1)));
				Assert.That(ex.Message, Does.StartWith("A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible."));
			}

			ex = Assert.Throws<SqlException>(() => new BillingRepository(loggerMock.Object).UpdateChargeable(new DateTime(year: 1800, month: 1, day: 1)));
			Assert.That(ex.Message, Does.StartWith("The conversion of a datetime data type to a smalldatetime data type resulted in an out-of-range value."));

			ex = Assert.Throws<SqlException>(() => new BillingRepository(null, true).UpdateChargeable(new DateTime(year: 1800, month: 1, day: 1)));
			Assert.That(ex.Message, Does.StartWith("The conversion of a datetime data type to a smalldatetime data type resulted in an out-of-range value."));

			ex = Assert.Throws<SqlException>(() => new BillingRepository().UpdateChargeable(new DateTime(year: 1800, month: 1, day: 1)));
			Assert.That(ex.Message, Does.StartWith("The conversion of a datetime data type to a smalldatetime data type resulted in an out-of-range value."));

		}

		[Test]
		public void TestELKResubmitTransaction()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				var transaction = "{ \"ABC\" : \"XYZ\" }";
				BillingDataTestHelper.TruncateTable(con, "dbo.ELKResubmitTransaction");
				BillingDataTestHelper.AddELKTransaction(transaction);

				var dt = BillingDataTestHelper.SelectELKTransactions();

				Assert.That(dt.Rows.Count, Is.EqualTo(1));
				var transactionRow = dt.Rows[0];
				Assert.AreEqual(transaction, transactionRow["RT_JsonData"]);

				BillingDataTestHelper.DeleteELKTransaction(transactionRow["RT_PK"].ToString());

				dt = BillingDataTestHelper.SelectELKTransactions();
				Assert.AreEqual(0, dt.Rows.Count);
			}
		}

		[Test]
		public void TestAddDuplicates()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);

				var transaction = GetValidTransaction();
				BillingDataTestHelper.AddTransaction(transaction);
				BillingDataTestHelper.AddTransaction(transaction);

				var actual = BillingDataTestHelper.LoadAllStagingTransactionsMostRecentFirst(con);
				Assert.That(actual.Rows.Count, Is.EqualTo(2), "Transaction was added twice.");
			}
		}

		[Test]
		public void TestAddTransactionWithIncorrectData()
		{
			var transaction = new API.BillingTransaction
			{
				BillableCount = 2,
				ClientID = "ABCDEFXYZ_",
				ClientNumber = "1234567891234567891234567891234567891234567891234_.",
				ClientStaffCode = "ABC_",
				PriceItemCode = "DEF_",
				Reference2 = "REFERENCE 22222222222222222222222222222222222222222",
				Reference3 = "REFERENCE 3________________________________________",
				Reference4 = "REFERENCE 4________________________________________",
				Reference5 = "REFERENCE 5________________________________________",
				MessageTrackingID = "MessageTrackingID_______________________________________%",
				ReportingSource = "XYZ_",
				ServiceOccuredUTC = DateTime.MinValue
			};
			Assert.That(() => BillingDataTestHelper.AddTransaction(transaction), Throws.TypeOf<API.ValidationException>().With.Property("Errors").EqualTo(new[]
			{
				"The Category field is required.",
				"The field PriceItemCode must be a string with a maximum length of 3.",
				"The field ReportingSource must be a string with a maximum length of 3.",
				"The field ServiceOccuredUTC is required and must be no more than 5 years in the past and no more than one month in the future.",
				"The field ClientID must be a string with a maximum length of 9.",
				"The field ClientNumber must be a string with a maximum length of 50.",
				"The field ClientStaffCode must be a string with a maximum length of 3.",
				"The Reference1 field is required.",
				"The field Reference2 must be a string with a maximum length of 50.",
				"The field Reference3 must be a string with a maximum length of 50.",
				"The field Reference4 must be a string with a maximum length of 50.",
				"The field Reference5 must be a string with a maximum length of 50.",
				"The field MessageTrackingID must be a string with a maximum length of 36."
			}));
		}

		[Test]
		public void TestAddTransactionWithIncorrectASCIIData()
		{
			var transaction = new API.BillingTransaction
			{
				BillableCount = 2,
				ClientID = "ABCDE XYZ",
				ClientNumber = "9876543210012345678?",
				ClientStaffCode = "A¦%",
				Category = "T#T",
				PriceItemCode = "DE:",
				Reference1 = "REFERENCE1á45\r\n\t6",
				Reference2 = "REFERENCE2!\u0008^&¦",
				Reference3 = "REference3Ü!@5\u0020",
				ReportingSource = "X*Z",
				ServiceOccuredUTC = DateTime.UtcNow,
				Version = 1,
				MessageTrackingID = "55B2D0DA-8230-43BE-83BF-5C7D5766!EÜ",
			};
			Assert.That(() => BillingDataTestHelper.AddTransaction(transaction), Throws.TypeOf<API.ValidationException>().With.Property("Errors").EqualTo(new[]
			{
				"Field Category has invalid characters. It must contain only ASCII letters, digits, fullstop, hyphen or underscore.",
				"Field PriceItemCode has invalid characters. It must contain only ASCII letters, digits, fullstop, hyphen, hash or underscore.",
				"Field ReportingSource has invalid characters. It must contain only ASCII letters, digits, fullstop, hyphen or underscore.",
				"Field ClientID has invalid characters. It must contain only ASCII letters, digits, fullstop, hyphen, question mark or underscore.",
				"Field ClientNumber has invalid characters. It must contain only ASCII letters, digits, fullstop, hyphen or underscore.",
				"Field Reference2 has invalid characters. It must not contain ASCII control characters except whitespace."
			}));
		}

		[Test]
		public void TestAddRange()
		{
			var t1 = GetValidTransaction();
			var t2 = GetValidTransaction();
			var t3 = GetValidTransaction();

			var dtNow = DateTime.Now;
			var serviceOccuredUTC1 = dtNow.AddYears(-5);
			var serviceOccuredUTC2 = dtNow.AddYears(-5).AddDays(1);
			var serviceOccuredUTC3 = dtNow.AddYears(-5).AddDays(2);
			t1.ServiceOccuredUTC = serviceOccuredUTC1;
			t2.ServiceOccuredUTC = serviceOccuredUTC2;
			t3.ServiceOccuredUTC = serviceOccuredUTC3;

			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);

				BillingDataTestHelper.AddTransactionRange(new[] { t1, t2, t3, t3 });

				var actual = BillingDataTestHelper.LoadAllStagingTransactionsMostRecentFirst(con);
				Assert.That(actual.Rows.Count, Is.EqualTo(4), "Transactions added.");

				var actual1 = actual.Rows.Cast<DataRow>().Single(x => (DateTime)x["TX_ServiceOccuredUTC"] == serviceOccuredUTC1);
				var actual2 = actual.Rows.Cast<DataRow>().Single(x => (DateTime)x["TX_ServiceOccuredUTC"] == serviceOccuredUTC2);
				var actual3 = actual.Rows.Cast<DataRow>().First(x => (DateTime)x["TX_ServiceOccuredUTC"] == serviceOccuredUTC3);

				BillingDataTestHelper.AssertPropertiesAreEqual(t1, actual1);
				BillingDataTestHelper.AssertPropertiesAreEqual(t2, actual2);
				BillingDataTestHelper.AssertPropertiesAreEqual(t3, actual3);
			}
		}

		[Test]
		public void TestAddRangeValidation()
		{
			var t1 = GetValidTransaction();
			var t2 = GetValidTransaction();
			var t3 = GetValidTransaction();
			var dtNow = DateTime.Now;
			t1.ServiceOccuredUTC = dtNow.AddYears(-5);
			t2.ServiceOccuredUTC = dtNow.AddYears(-5).AddDays(1);
			t3.ServiceOccuredUTC = dtNow.AddYears(-5).AddDays(2);

			t2.PriceItemCode = null;
			t3.PriceItemCode = null;

			try
			{
				BillingDataTestHelper.AddTransactionRange(new[] { t1, t2, t3 });
				Assert.Fail("Exception must be thrown.");
			}
			catch (API.ValidationException ex)
			{
				Assert.That(ex.Message, Is.EqualTo("Validation failed for transaction 2 of 3"));
			}
		}

		[Test]
		public void TestAddTransactionWithInvalidServiceOccuredUTCInTheFuture()
		{
			var transaction = GetValidTransaction();
			transaction.ServiceOccuredUTC = DateTime.Now.AddMonths(1).AddDays(-1);
			try
			{
				BillingDataTestHelper.AddTransaction(transaction);
			}
			catch (Exception ex)
			{
				Assert.Fail("Expected no exception, but got: " + ex.Message);
			}

			transaction.ServiceOccuredUTC = DateTime.Now.AddMonths(1);
			Assert.That(() => BillingDataTestHelper.AddTransaction(transaction), Throws.TypeOf<API.ValidationException>().With.Property("Errors").EqualTo(new[]
			{
				"The field ServiceOccuredUTC is required and must be no more than 5 years in the past and no more than one month in the future.",
			}));
		}

		[Test]
		public void TestAddTransactionWithInvalidServiceOccuredUTCInThePast()
		{
			var transaction = GetValidTransaction();
			transaction.ServiceOccuredUTC = DateTime.Now.AddYears(-5);
			try
			{
				BillingDataTestHelper.AddTransaction(transaction);
			}
			catch (Exception ex)
			{
				Assert.Fail("Expected no exception, but got: " + ex.Message);
			}

			transaction.ServiceOccuredUTC = DateTime.Now.AddYears(-5).AddDays(-1);
			Assert.That(() => BillingDataTestHelper.AddTransaction(transaction), Throws.TypeOf<API.ValidationException>().With.Property("Errors").EqualTo(new[]
			{
				"The field ServiceOccuredUTC is required and must be no more than 5 years in the past and no more than one month in the future.",
			}));
		}

		[Test]
		public void TestCountStaging()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				Assert.AreEqual(0, BillingDataTestHelper.CountStaging());

				var transaction = GetValidTransaction();
				BillingDataTestHelper.AddTransaction(transaction);

				Assert.AreEqual(1, BillingDataTestHelper.CountStaging());
			}
		}

		[Test]
                public void TestOldestSystemCreateUTCInStaging()
                {
                        using (var con = BillingDataTestHelper.GetNewOpenConnection())
                        {
                                BillingDataTestHelper.TruncateStagingTable(con);
                                Assert.IsFalse(BillingDataTestHelper.OldestSystemCreateUTCInStaging().HasValue);

				var addedTime = DateTime.UtcNow.AddMinutes(-1); //Bypass the possibly tiny gap of time between the SQL server and .Net
				var transaction = GetValidTransaction();
				BillingDataTestHelper.AddTransaction(transaction);
				BillingDataTestHelper.AddTransaction(transaction);

				var oldest = BillingDataTestHelper.OldestSystemCreateUTCInStaging();
				Assert.IsTrue(oldest.HasValue);
                                Assert.GreaterOrEqual(oldest.Value.Ticks, addedTime.Ticks);
                        }
                }

                [Test]
                public void TestGetLatestLicenses()
                {
                        using (var con = BillingDataTestHelper.GetNewOpenConnection())
                        {
                                BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
                                BillingDataTestHelper.ExecuteNonQuery(con,
                                        "INSERT INTO edi.LicenceDatabaseCodeHistory(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, ValidFromUtc, HostedLocation, IsActive, IsTeardownInProgress) " +
                                        "VALUES('AAA', 1, 'ENT', 'SRV', '2000-01-01', 'OLD', 1, 0)");
                                BillingDataTestHelper.ExecuteNonQuery(con,
                                        "INSERT INTO edi.LicenceDatabaseCodeHistory(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, ValidFromUtc, HostedLocation, IsActive, IsTeardownInProgress) " +
                                        "VALUES('AAA', 1, 'ENT', 'SRV', '2001-01-01', 'NEW', 1, 0)");

                                var licenses = BillingDataTestHelper.GetLatestLicenses().ToList();
                                Assert.That(licenses.Any(l => l.DatabaseNumber == 1 && l.HostedLocation == "NEW"));
                        }
                }

		static API.BillingTransaction GetValidTransaction()
		{
			return new API.BillingTransaction
			{
				BillableCount = 2,
				ClientID = "ABCDEFXYZ",
				ClientNumber = "98765432100123456789",
				ClientStaffCode = "ABC",
				Category = "TST",
				PriceItemCode = "DEF",
				Reference1 = "REFERENCE 1",
				Reference2 = "REFERENCE 2",
				Reference3 = "REFERENCE 3",
				ReportingSource = "XYZ",
				ServiceOccuredUTC = DateTime.UtcNow,
				Version = 1,
				MessageTrackingID = "55B2D0DA-8230-43BE-83BF-5C7D5766343E",
			};
		}
	}
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using CargoWise.Billing.API;
using CargoWise.Billing.Kafka.API;
using CargoWise.Billing.Client;
using CargoWise.eServices.Billing.Tests.Common;
using Confluent.Kafka;
using NUnit.Framework;
using BillingTransaction = CargoWise.Billing.API.BillingTransaction;

namespace CargoWise.eServices.Billing.WcfService.IntegrationTests
{
	[TestFixture]
	[WithBillingWcfService]
	public class BillingServiceTest : TestBase
	{
		[Test]
		public void TestProcessBilling()
		{
			var transaction = new BillingTransaction
			{
				BillableCount = 1,
				ClientID = "ENTMELMEL",
				ClientNumber = null,
				Category = "STL",
				PriceItemCode = "DUM",
				Reference1 = "43518651",
				Reference2 = "8600284806",
				Reference3 = "CN",
				Reference4 = "INV",
				Reference5 = "2DA42803-3D2B-4B81-8A0B-0D626E339FF7",
				ReportingSource = "ENT",
				ServiceOccuredUTC = DateTime.UtcNow,
				Version = 0,
			};

			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				var existing = BillingDataTestHelper.LoadAllStagingTransactionsMostRecentFirst(con);
				var initialTransactionsCount = existing.Rows.Count;

				using (var client = CreateBillingServiceClient())
				{
					client.AddTransaction(transaction);
				}

				var actual = BillingDataTestHelper.LoadAllStagingTransactionsMostRecentFirst(con);

				Assert.That(actual.Rows.Count, Is.EqualTo(initialTransactionsCount + 1));
				var transactionRow = actual.Rows[0];
				Assert.That(transactionRow, Is.Not.Null);
				BillingDataTestHelper.AssertPropertiesAreEqual(transaction, transactionRow);

				RecurringJobManager.Trigger("ProcessBillingDatabase");
				TestHelper.DoWithRetry(() => CheckRecurringJobRunsAndCompleted("ProcessBillingDatabase"), "AwaitProcessBillingDatabaseFinished", TimeSpan.FromSeconds(5));

				var table = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				Assert.That(table.Rows.Count, Is.EqualTo(1), "first message count");
				BillingDataTestHelper.AssertPropertiesAreEqual(transaction, table.Rows.Cast<DataRow>().First(), "CH_");
			}
		}

		[Test]
		public void TestAddTransaction()
		{
			var transaction = new BillingTransaction
			{
				BillableCount = 2,
				Category = "TST",
				ClientID = "ABCDEFXYZ",
				ClientNumber = "98765432100123456789",
				ClientStaffCode = "ABC",
				PriceItemCode = "DEF",
				Reference1 = "REFERENCE 1",
				Reference2 = "REFERENCE 2",
				Reference3 = "REFERENCE 3",
				ReportingSource = "XYZ",
				ServiceOccuredUTC = DateTime.UtcNow,
				MessageTrackingID = "55B2D0DA-8230-43BE-83BF-5C7D5766343E",
			};
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				var existing = BillingDataTestHelper.LoadAllStagingTransactionsMostRecentFirst(con);
				var initialTransactionsCount = existing.Rows.Count;

				using (var client = CreateBillingServiceClient())
				{
					client.AddTransaction(transaction);
				}

				var actual = BillingDataTestHelper.LoadAllStagingTransactionsMostRecentFirst(con);

				Assert.That(actual.Rows.Count, Is.EqualTo(initialTransactionsCount + 1));
				var transactionRow = actual.Rows[0];
				Assert.That(transactionRow, Is.Not.Null);
				BillingDataTestHelper.AssertPropertiesAreEqual(transaction, transactionRow);
			}
		}

		[Test]
		public void TestAddTransactionViaKafka()
		{
			var config = new Dictionary<string, string>()
			{
				{ "bootstrap.servers", WithBillingWcfServiceAttribute.Current.KafkaBrokers},
				{ "linger.ms", "50" },
				{"message.timeout.ms", "10000"}
			};

			var producerConfig = new ProducerConfig(config);

			var transactions = new List<BillingTransaction>();
			using (var kafkaClient = new BillingKafkaClient(producerConfig))
			{
				var dt = new DateTime(2024, 8, 1, 0, 0, 0, DateTimeKind.Utc);
				for (int i = 0; i < 500; i++)
				{
					var transaction = new BillingTransaction
					{
						BillableCount = 2,
						Category = "TST",
						ClientID = "ABCDEFXYZ",
						ClientNumber = "98765432100123456789",
						ClientStaffCode = "ABC",
						PriceItemCode = "DEF",
						Reference1 = "REFERENCE 1",
						Reference2 = "REFERENCE 2",
						Reference3 = "REFERENCE 3",
						ReportingSource = "XYZ",
						ServiceOccuredUTC = dt.AddSeconds(i),
						MessageTrackingID = Guid.NewGuid().ToString(),
					};

					kafkaClient.SendBillingInfoToKafka("billing-topic", transaction.MessageTrackingID, transaction,
						report =>
						{
							Assert.AreEqual(ErrorCode.NoError, report.Error.Code);
						});
					transactions.Add(transaction);
				}
			}

			using (var client = CreateBillingServiceClient())
			{
				client.Ping();
			}

			RecurringJobManager.Trigger("BillingJobManager");
			TestHelper.DoWithRetry(() => CheckKafkaBacklogProcessed(), "AwaitKafkaBacklogProcessed", TimeSpan.FromSeconds(5));
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				var existing = BillingDataTestHelper.LoadAllStagingTransactionsMostOccurrenceFirst(con);
				Assert.AreEqual(500, existing.Rows.Count);
				var i = transactions.Count - 1;
				foreach (DataRow dataRow in existing.Rows)
				{
					BillingDataTestHelper.AssertPropertiesAreEqual(transactions[i], dataRow);
					i--;
				}
			}
		}

		[Test]
		public void TestAddUsageTransaction()
		{
			var transaction = new UsageTransaction
			{
				BranchCode = "KLM",
				CompanyCode = "ABC",
				CompanyName = "Test",
				EnterpriseCode = "TST",
				Environment = "TST",
				ServerCode = "TST",
				UsageCode = "USG",
				UsageCount = 2,
				ServiceOccuredUTC = DateTimeOffset.Parse("2024-01-01T00:00:00").DateTime,
				AdditionalRefs = @"{
	""TestProp"":""TestValue""
}"
			};

			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			using (var client = CreateBillingServiceClient())
			{
				BillingDataTestHelper.TruncateTable(con, "dbo.ELKResubmitTransaction");
				client.AddUsageTransaction(transaction);
			}

			var actual = BillingDataTestHelper.SelectELKTransactions();
			var expected = "{\"@timestamp\":\"2024-01-01T00:00:00Z\",\"UsageCount\":2,\"ServiceOccured\":\"2024-01-01T00:00:00Z\",\"EnterpriseCode\":\"TST\",\"ServerCode\":\"TST\",\"Environment\":\"TST\",\"CompanyCode\":\"ABC\",\"CompanyName\":\"Test\",\"BranchCode\":\"KLM\",\"UsageCode\":\"USG\",\"TestProp\":\"TestValue\"}";
			Assert.That(actual.Rows.Count, Is.EqualTo(1));
			Assert.That(actual.Rows[0]["RT_JsonData"], Is.Not.Null.And.EqualTo(expected));
		}

		[Test]
		public void TestAddUsageTransactionRange()
		{
			int SendCount = 50;
			var toSend = new List<UsageTransaction>();
			var expectedTransactions = new List<string>();
			var occurredDateTime = new DateTime(2024, 1, 1, 2, 2, 2);
			var expected = "{{\"@timestamp\":\"{1}\",\"UsageCount\":{0},\"ServiceOccured\":\"{1}\",\"EnterpriseCode\":\"TST\",\"ServerCode\":\"TST\",\"Environment\":\"TST\",\"CompanyCode\":\"ABC\",\"CompanyName\":\"Test{0}\",\"BranchCode\":\"KLM\",\"UsageCode\":\"USG\",\"TestProp\":\"TestValue\"}}";
			for (int i = 1; i <= SendCount; ++i)
			{
				var transaction = new UsageTransaction
				{
					BranchCode = "KLM",
					CompanyCode = "ABC",
					CompanyName = $"Test{i}",
					EnterpriseCode = "TST",
					Environment = "TST",
					ServerCode = "TST",
					UsageCode = "USG",
					UsageCount = i,
					ServiceOccuredUTC = occurredDateTime,
					AdditionalRefs = @"{
	""TestProp"":""TestValue""
}"
				};

				toSend.Add(transaction);
				expectedTransactions.Add(string.Format(expected, i, occurredDateTime.ToString("yyyy-MM-ddTHH:mm:ss" + "Z")));
			}

			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateTable(con, "dbo.ELKResubmitTransaction");

				using (var client = CreateBillingServiceClient())
				{
					client.AddUsageTransactionRange(toSend);
				}

				var actual = BillingDataTestHelper.SelectELKTransactions();
				Assert.That(actual.Rows.Count, Is.EqualTo(SendCount));
				Assert.That(actual.Select().Select(row => row["RT_JsonData"].ToString()), Is.EqualTo(expectedTransactions));
			}
		}

		[Test]
		public void TestAddTransactionValidationFail()
		{
			var transaction = new BillingTransaction
			{
				BillableCount = 2,
				ClientID = "ABCDEFXYZ_",
				ClientNumber = "12345678912345678912345678912345678912345678912345_",
				ClientStaffCode = "ABC_",
				PriceItemCode = "DEF_",
				Reference2 = "REFERENCE 2________________________________________",
				Reference3 = "REFERENCE 3________________________________________",
				Reference4 = "REFERENCE 4________________________________________",
				Reference5 = "REFERENCE 5________________________________________",
				MessageTrackingID = "MessageTrackingID________________________________________",
				ReportingSource = "XYZ_"
			};
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				var existing = BillingDataTestHelper.LoadAllStagingTransactionsMostRecentFirst(con);
				var initialTransactionsCount = existing.Rows.Count;

				try
				{
					using (var client = CreateBillingServiceClient())
					{
						client.AddTransaction(transaction);
					}
					Assert.Fail("FaultException must be thrown.");
				}
				catch (ValidationException e)
				{
					Assert.That(e.Message, Is.EqualTo("Transaction validation failed."));
					Assert.That(e.Errors, Is.EqualTo(new[]
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
						"The field MessageTrackingID must be a string with a maximum length of 36.",
					}));
					var actual = BillingDataTestHelper.LoadAllStagingTransactionsMostRecentFirst(con);

					Assert.That(actual.Rows.Count, Is.EqualTo(initialTransactionsCount));
				}
			}
		}

		[Test]
		public void TestAddTransactionRange()
		{
			int SendCount = 50;
			var toSend = new List<BillingTransaction>();
			var utcNow = DateTime.UtcNow;
			var ref1 = Guid.NewGuid().ToString();
			for (int i = 0; i < SendCount; ++i)
			{
				var transaction = new BillingTransaction
				{
					BillableCount = i + 1,
					Category = "TST",
					ClientID = "ABCDEFXYZ",
					ClientNumber = "98765432100123456789",
					ClientStaffCode = "ABC",
					PriceItemCode = "DEF",
					Reference1 = ref1,
					Reference2 = "REFERENCE 2",
					Reference3 = "REFERENCE 3",
					ReportingSource = "XYZ",
					ServiceOccuredUTC = utcNow,
					MessageTrackingID = Guid.NewGuid().ToString(),
				};

				toSend.Add(transaction);
			}

			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				using (var client = CreateBillingServiceClient())
				{
					client.AddTransactionRange(toSend);
				}

				var actual = BillingDataTestHelper.LoadAllStagingTransactionsMostRecentFirst(con);
				var rowTrackingIdToRow = actual.Rows.Cast<DataRow>()
					.Where(x => x["TX_Reference1"].ToString() == ref1)
					.ToDictionary(x => x["TX_MessageTrackingID"].ToString());

				for (int i = 0; i < SendCount; ++i)
				{
					if (rowTrackingIdToRow.TryGetValue(toSend[i].MessageTrackingID, out DataRow transactionRow))
					{
						BillingDataTestHelper.AssertPropertiesAreEqual(toSend[i], transactionRow);
					}
					else
					{
						Assert.Fail(i.ToString());
					}
				}
			}
		}

		[Test]
		public void TestAddTransactionRangeValidationFail()
		{
			int SendCount = 50;
			var toSend = new List<BillingTransaction>();
			var utcNow = DateTime.UtcNow;
			for (int i = 0; i < SendCount - 1; ++i)
			{
				var transaction = new BillingTransaction
				{
					BillableCount = i + 1,
					Category = "TST",
					ClientID = "ABCDEFXYZ",
					ClientNumber = "98765432100123456789",
					ClientStaffCode = "ABC",
					PriceItemCode = "DEF",
					Reference1 = "REFERENCE 1",
					Reference2 = "REFERENCE 2",
					Reference3 = "REFERENCE 3",
					ReportingSource = "XYZ",
					ServiceOccuredUTC = utcNow,
					MessageTrackingID = (i + 1).ToString(),
				};

				toSend.Add(transaction);
			}

			var invalidTransaction = new BillingTransaction
			{
				BillableCount = 999,
				ClientID = "ABCDEFXYZ_",
				ClientNumber = "12345678912345678912345678912345678912345678912345_",
				ClientStaffCode = "ABC_",
				PriceItemCode = "DEF_",
				Reference2 = "REFERENCE 2________________________________________",
				Reference3 = "REFERENCE 3________________________________________",
				Reference4 = "REFERENCE 4________________________________________",
				Reference5 = "REFERENCE 5________________________________________",
				MessageTrackingID = "MessageTrackingID________________________________________",
				ReportingSource = "XYZ_"
			};

			toSend.Insert(10, invalidTransaction);

			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);

				try
				{
					using (var client = CreateBillingServiceClient())
					{
						client.AddTransactionRange(toSend);
					}
					Assert.Fail("FaultException must be thrown.");
				}
				catch (ValidationException e)
				{
					Assert.That(e.Message, Is.EqualTo("Validation failed for transaction 11 of 50"));
					Assert.That(e.Errors, Is.EqualTo(new[]
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
						"The field MessageTrackingID must be a string with a maximum length of 36.",
					}));
					var actual = BillingDataTestHelper.LoadAllStagingTransactionsMostRecentFirst(con);

					Assert.That(actual.Rows.Count, Is.EqualTo(0));

					BillingDataTestHelper.TruncateStagingTable(con);
				}
			}
		}

		[Test]
                public void TestAddTransactionRangeLarge()
		{
			int SendCount = 2000;
			var toSend = new List<BillingTransaction>();
			var utcNow = DateTime.UtcNow;
			var ref1 = Guid.NewGuid().ToString();
			for (int i = 0; i < SendCount; ++i)
			{
				var transaction = new BillingTransaction
				{
					BillableCount = i + 1,
					Category = "TST",
					ClientID = "ABCDEFXYZ",
					ClientNumber = "98765432100123456789",
					ClientStaffCode = "ABC",
					PriceItemCode = "DEF",
					Reference1 = ref1,
					Reference2 = "REFERENCE 2",
					Reference3 = "REFERENCE 3",
					ReportingSource = "XYZ",
					ServiceOccuredUTC = utcNow,
					MessageTrackingID = Guid.NewGuid().ToString(),
				};

				toSend.Add(transaction);
                }

                [Test]
                public void TestGetLatestLicenses()
                {
                        using (var con = BillingDataTestHelper.GetNewOpenConnection())
                        {
                                BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
                                BillingDataTestHelper.ExecuteNonQuery(con,
                                        "INSERT INTO edi.LicenceDatabaseCodeHistory(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, ValidFromUtc, HostedLocation, IsActive, IsTeardownInProgress, LicenceType) " +
                                        "VALUES('AAA', 1, 'ENT', 'SRV', '2000-01-01', 'OLD', 1, 0, 'PRD')");
                                BillingDataTestHelper.ExecuteNonQuery(con,
                                        "INSERT INTO edi.LicenceDatabaseCodeHistory(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, ValidFromUtc, HostedLocation, IsActive, IsTeardownInProgress, LicenceType) " +
                                        "VALUES('AAA', 1, 'ENT', 'SRV', '2001-01-01', 'NEW', 1, 0, 'DEV')");

                                using (var client = CreateBillingServiceClient())
                                {
                                        var licenses = client.GetLatestLicenses();
                                        Assert.That(licenses.Length, Is.EqualTo(1));
                                        Assert.That(licenses[0].DatabaseNumber, Is.EqualTo(1));
                                        Assert.That(licenses[0].HostedLocation, Is.EqualTo("NEW"));
                                }
                        }
                }

			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				using (var client = CreateBillingServiceClient())
				{
					client.AddTransactionRange(toSend);
				}

				var actual = BillingDataTestHelper.LoadAllStagingTransactionsMostRecentFirst(con);
				var rowTrackingIdToRow = actual.Rows.Cast<DataRow>()
					.Where(x => x["TX_Reference1"].ToString() == ref1)
					.ToDictionary(x => x["TX_MessageTrackingID"].ToString());

				for (int i = 0; i < SendCount; ++i)
				{
					if (rowTrackingIdToRow.TryGetValue(toSend[i].MessageTrackingID, out DataRow transactionRow))
					{
						BillingDataTestHelper.AssertPropertiesAreEqual(toSend[i], transactionRow);
					}
					else
					{
						Assert.Fail(i.ToString());
					}
				}
			}
		}

		[SetUp]
		public void SetUp()
		{
			ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
		}

		BillingServiceClient CreateBillingServiceClient()
		{
			return WithBillingWcfServiceAttribute.Current.CreateClient();
		}
	}
}

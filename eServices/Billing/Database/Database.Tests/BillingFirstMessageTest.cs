using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using CargoWise.eServices.Billing.Tests.Common;
using NUnit.Framework;
using API = CargoWise.Billing.API;

namespace CargoWise.eServices.Billing.Database.Tests
{
	[TestFixture]
	public class BillingFirstMessageTest
	{
		[Test]
		public void TestFirstMessage()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "SYD");

				BillingDataTestHelper.ExecuteNonQuery(con,
					"delete from edi.ConfigFirstMessage where FM_Category = 'Z_Z'; " +
					"insert edi.ConfigFirstMessage(FM_Category, FM_PriceItemCode, FM_IncludeRef1, FM_IncludeRef2) " +
					"values('Z_Z', 'Z_Z', 1, 1);");

				var currentYear = GetCurrentYear();

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENTSYDSRV",
					ClientNumber = "C.SYD",
					ClientStaffCode = "ABC",
					Category = "Z_Z",
					PriceItemCode = "Z_Z",
					Reference1 = "REF 1",
					Reference2 = null,
					Reference3 = "REF 3",
					ReportingSource = "XYZ",
					ServiceOccuredUTC = new DateTime(currentYear - 3, 7, 19, 9, 0, 0),
					Version = 1,
					MessageTrackingID = "Message1"
				};

				BillingDataTestHelper.AddTransaction(trn1);

				var trn2 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENTSYDSRV",
					ClientNumber = "C.SYD",
					ClientStaffCode = "ABC",
					Category = "Z_Z",
					PriceItemCode = "Z_Z",
					Reference1 = "REF 1",
					Reference2 = null,
					Reference3 = "REF 3",
					ReportingSource = "XYZ",
					ServiceOccuredUTC = new DateTime(currentYear - 3, 7, 20, 9, 0, 0),
					Version = 1,
					MessageTrackingID = "Message2"
				};

				BillingDataTestHelper.AddTransaction(trn2);

				var trnUnmatched = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENTSYDZZZ",
					ClientNumber = null,
					ClientStaffCode = "ABC",
					Category = "Z_Z",
					PriceItemCode = "Z_Z",
					Reference1 = "REF 1",
					Reference2 = null,
					Reference3 = "REF 3",
					ReportingSource = "XYZ",
					ServiceOccuredUTC = new DateTime(currentYear - 3, 7, 21, 9, 0, 0),
					Version = 1,
					MessageTrackingID = "Message3"
				};

				BillingDataTestHelper.AddTransaction(trnUnmatched);

				ExecuteProcessStaging(con);

				// run again
				ExecuteProcessStaging(con);

				var table = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				var stagingUnknownTable = BillingDataTestHelper.LoadTableByName(con, "edi.StagingUnknownSystems");

				Assert.That(table.Rows.Count, Is.EqualTo(1), "first message count");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(1), "second message");
				Assert.That(stagingUnknownTable.Rows.Count, Is.EqualTo(1), "unmatched message");

				BillingDataTestHelper.AssertPropertiesAreEqual(trn1, table.Rows[0], "CH_");

				var usageForTrn2 = usageTable.Rows.Cast<DataRow>().Single(x => (string)x["US_MessageTrackingID"] == "Message2");
				var usageForUnmatched = stagingUnknownTable.Rows.Cast<DataRow>().Single(x => (string)x["TX_MessageTrackingID"] == "Message3");
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn2, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, usageForTrn2);

				BillingDataTestHelper.AddDatabaseAndCompany(con, 2, 1, "SYD", serverCode: "ZZZ");
				ExecuteProcessStaging(con);

				table = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				stagingUnknownTable = BillingDataTestHelper.LoadTableByName(con, "edi.StagingUnknownSystems");

				Assert.That(table.Rows.Count, Is.EqualTo(2), "first message count on each client Id");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(1), "second message");
				Assert.That(stagingUnknownTable.Rows.Count, Is.EqualTo(0), "no unmatched messages now that the system is licensed");

				BillingDataTestHelper.AssertPropertiesAreEqual(trnUnmatched, table.Rows.Cast<DataRow>().Single(x => (string)x["CH_MessageTrackingID"] == "Message3"), "CH_");
			}
		}

		[Test]
		public void TestFirstMessageNonSequentialRefFields()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "SYD");

				BillingDataTestHelper.ExecuteNonQuery(con,
					"delete from edi.ConfigFirstMessage where FM_Category = 'Z_Z'; " +
					"insert edi.ConfigFirstMessage(FM_Category, FM_PriceItemCode, FM_IncludeRef1, FM_IncludeRef3, FM_IncludeRef5) " +
					"values('Z_Z', 'Z_Z', 1, 1, 1);");

				var currentYear = GetCurrentYear();

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENTSYDSRV",
					ClientNumber = "C.SYD",
					ClientStaffCode = "ABC",
					Category = "Z_Z",
					PriceItemCode = "Z_Z",
					Reference1 = "REF 1",
					Reference2 = "REF 2A",
					Reference3 = "REF 3",
					Reference4 = "REF 4A",
					Reference5 = "REF 5",
					ReportingSource = "XYZ",
					ServiceOccuredUTC = new DateTime(currentYear - 3, 7, 19, 9, 0, 0),
					Version = 1,
					MessageTrackingID = "Message1"
				};

				BillingDataTestHelper.AddTransaction(trn1);

				var trn2 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENTSYDSRV",
					ClientNumber = "C.SYD",
					ClientStaffCode = "ABC",
					Category = "Z_Z",
					PriceItemCode = "Z_Z",
					Reference1 = "REF 1",
					Reference2 = "REF 2B",
					Reference3 = "REF 3",
					Reference4 = "REF 4B",
					Reference5 = "REF 5",
					ReportingSource = "XYZ",
					ServiceOccuredUTC = new DateTime(currentYear - 3, 7, 20, 9, 0, 0),
					Version = 1,
					MessageTrackingID = "Message2"
				};

				var trn3 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENTSYDSRV",
					ClientNumber = "C.SYD",
					ClientStaffCode = "ABC",
					Category = "Z_Z",
					PriceItemCode = "Z_Z",
					Reference1 = "REF 1",
					Reference2 = null,
					Reference3 = "REF 3",
					Reference5 = "REF 5",
					ReportingSource = "XYZ",
					ServiceOccuredUTC = new DateTime(currentYear - 3, 7, 20, 9, 0, 0),
					Version = 1,
					MessageTrackingID = "Message2"
				};

				BillingDataTestHelper.AddTransaction(trn2);

				var trnUnmatched = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENTSYDZZZ",
					ClientNumber = null,
					ClientStaffCode = "ABC",
					Category = "Z_Z",
					PriceItemCode = "Z_Z",
					Reference1 = "REF 1",
					Reference2 = null,
					Reference3 = "REF 3",
					Reference5 = "REF 5",
					ReportingSource = "XYZ",
					ServiceOccuredUTC = new DateTime(currentYear - 3, 7, 21, 9, 0, 0),
					Version = 1,
					MessageTrackingID = "Message3"
				};

				BillingDataTestHelper.AddTransaction(trnUnmatched);

				ExecuteProcessStaging(con);

				// run again
				ExecuteProcessStaging(con);

				var table = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				var stagingUnknownTable = BillingDataTestHelper.LoadTableByName(con, "edi.StagingUnknownSystems");

				Assert.That(table.Rows.Count, Is.EqualTo(1), "first message count");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(1), "second message");
				Assert.That(stagingUnknownTable.Rows.Count, Is.EqualTo(1), "unmatched message");

				BillingDataTestHelper.AssertPropertiesAreEqual(trn1, table.Rows[0], "CH_");

				var usageForTrn2 = usageTable.Rows.Cast<DataRow>().Single(x => (string)x["US_MessageTrackingID"] == "Message2");
				var usageForUnmatched = stagingUnknownTable.Rows.Cast<DataRow>().Single(x => (string)x["TX_MessageTrackingID"] == "Message3");
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn2, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, usageForTrn2);

				BillingDataTestHelper.AddDatabaseAndCompany(con, 2, 1, "SYD", serverCode: "ZZZ");
				ExecuteProcessStaging(con);

				table = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				stagingUnknownTable = BillingDataTestHelper.LoadTableByName(con, "edi.StagingUnknownSystems");

				Assert.That(table.Rows.Count, Is.EqualTo(2), "first message count on each client Id");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(1), "second message");
				Assert.That(stagingUnknownTable.Rows.Count, Is.EqualTo(0), "no unmatched messages now that the system is licensed");

				BillingDataTestHelper.AssertPropertiesAreEqual(trnUnmatched, table.Rows.Cast<DataRow>().Single(x => (string)x["CH_MessageTrackingID"] == "Message3"), "CH_");
			}
		}

		[Test]
		public void TestFirstMessageSPM()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");

				var currentYear = GetCurrentYear();

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ABCDEFXYZ",
					ClientNumber = "C.DEF",
					ClientStaffCode = "ABC",
					Category = "SPM",
					PriceItemCode = "SPA",
					Reference1 = "REF 1",
					Reference2 = "REF 2",
					Reference3 = "REF 3",
					Reference4 = "JOB 1",
					Reference5 = "3",
					ReportingSource = "XYZ",
					ServiceOccuredUTC = new DateTime(currentYear - 3, 6, 20, 9, 0, 0),
					Version = 1,
				};

				// Job 1 - time 1: 3 containers
				BillingDataTestHelper.AddTransaction(trn1);

				// Job 1 - time 1: 5 containers
				trn1.Reference5 = "5";
				BillingDataTestHelper.AddTransaction(trn1);

				// Job 2 - time 1: 7 containers
				trn1.Reference4 = "JOB 2";
				trn1.Reference5 = "7";
				BillingDataTestHelper.AddTransaction(trn1);

				// Job 2 - time 1: 9 containers
				trn1.Reference5 = "9";
				BillingDataTestHelper.AddTransaction(trn1);

				// Job 1 - time 2: 11 containers
				// Ignored as not the first message
				trn1.Reference5 = "11";
				trn1.Reference4 = "JOB 1";
				trn1.ServiceOccuredUTC = trn1.ServiceOccuredUTC.AddMonths(1);

				BillingDataTestHelper.ExecuteNonQuery(con, @"
					delete edi.ConfigFirstMessage where FM_Category = 'SPM' and FM_PriceItemCode = 'SPA';
					insert edi.ConfigFirstMessage(FM_Category, FM_PriceItemCode, FM_IncludeRef1)
					  values('SPM', 'SPA', 1);
					delete edi.ConfigReferenceSwap where Category = 'SPM' and PriceItemCode = 'SPA';
					insert edi.ConfigReferenceSwap(Category, PriceItemCode, RefIndex1, RefIndex2, RefIndex3, RefIndex4, RefIndex5)
					  values('SPM', 'SPA', 4, 2, 3, 1, 5);
					delete edi.ClientCompanyCodeHistory;
					delete edi.ClientCompany;
					delete edi.LicenceDatabaseCodeHistory;
					insert edi.ClientCompany(DatabaseNumber, CompanyNumber, LCC_PK, CountryCode, ValidFromUtc)
					  values(1, 1, 'df45c37b-bdab-4bf8-accc-d25601f73753', 'AU', '2000-01-01T00:00:00');
					insert edi.ClientCompanyCodeHistory(DatabaseNumber, CompanyNumber, CompanyCode, ValidFromUtc)
					  values(1, 1, 'DEF', '2000-01-01T00:00:00');
					insert edi.LicenceDatabaseCodeHistory(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, ValidFromUtc)
					  values('1', 1, 'ENT', 'SRV', '2000-01-01T00:00:00');");

				ExecuteProcessStaging(con);

				// run again
				ExecuteProcessStaging(con);

				var table = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);

				Assert.That(table.Rows.Count, Is.EqualTo(2), "first message count");

				var job1 = table.Rows.Cast<DataRow>().Single(x => (string)x["CH_Reference1"] == "JOB 1");
				var job2 = table.Rows.Cast<DataRow>().Single(x => (string)x["CH_Reference1"] == "JOB 2");

				Assert.That(job1["CH_BillableCount"], Is.EqualTo(8));
				Assert.That(job1["CH_ClientID"], Is.EqualTo("ABCDEFXYZ"));
				Assert.That(job1["CH_ClientNumber"], Is.EqualTo("C.DEF"));
				Assert.That(job1["CH_ClientStaffCode"], Is.EqualTo("ABC"));
				Assert.That(job1["CH_Category"], Is.EqualTo("SPM"));
				Assert.That(job1["CH_PriceItemCode"], Is.EqualTo("SPA"));
				Assert.That(job1["CH_Reference2"], Is.EqualTo("REF 2"));
				Assert.That(job1["CH_Reference3"], Is.EqualTo("REF 3"));
				Assert.That(job1["CH_Reference4"], Is.EqualTo("REF 1"));
				Assert.That(job1["CH_Reference5"], Is.EqualTo(""));
				Assert.That(job1["CH_ReportingSource"], Is.EqualTo("XYZ"));
				Assert.That(job1["CH_Version"], Is.EqualTo(1));
				Assert.That(job1["CH_ServiceOccuredUTC"], Is.EqualTo(new DateTime(currentYear - 3, 6, 20, 9, 0, 0)));


				Assert.That(job2["CH_BillableCount"], Is.EqualTo(16));
				Assert.That(job2["CH_Category"], Is.EqualTo("SPM"));
				Assert.That(job2["CH_PriceItemCode"], Is.EqualTo("SPA"));
			}
		}

		[Test]
		public void TestFirstMessagePer30DaysAndThreeReferences()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "SYD");
				BillingDataTestHelper.ExecuteNonQuery(con,
					"delete from edi.ConfigFirstMessage where FM_Category = 'ZAC' and FM_PriceItemCode = 'ZX3'; " +
					"insert edi.ConfigFirstMessage(FM_Category, FM_PriceItemCode, FM_IncludeRef1, FM_IncludeRef2, FM_IncludeRef3, FM_Days) " +
					"values('ZAC', 'ZX3', 1, 1, 1, 30);" +
					"delete from edi.ConfigReferenceSwap where Category = 'ZAC' and PriceItemCode = 'ZX3'; ");

				var currentYear = GetCurrentYear();
				var date0807 = new DateTime(currentYear - 2, 8, 7).ToString("yyyyMMdd");
				var date0810 = new DateTime(currentYear - 2, 8, 10).ToString("yyyyMMdd");

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENTSYDSRV",
					ClientNumber = "C.SYD",
					ClientStaffCode = "ABC",
					Category = "ZAC",
					PriceItemCode = "ZX3",
					Reference1 = "Carrier 1",
					Reference2 = "Flight 1",
					Reference3 = date0807,
					Reference4 = "",
					Reference5 = "JOB 1",
					ReportingSource = "HUB",
					ServiceOccuredUTC = new DateTime(currentYear - 2, 8, 7, 9, 0, 0),
				};

				// Job 1 - message 1 & 2
				BillingDataTestHelper.AddTransaction(trn1);
				trn1.ServiceOccuredUTC = new DateTime(currentYear - 2, 8, 8, 9, 0, 0);
				BillingDataTestHelper.AddTransaction(trn1);

				// Job 2 (new date) - message 1 & 2
				trn1.Reference5 = "JOB 2";
				trn1.Reference3 = date0810;
				BillingDataTestHelper.AddTransaction(trn1);
				trn1.ServiceOccuredUTC = new DateTime(currentYear - 2, 8, 9, 9, 0, 0);
				BillingDataTestHelper.AddTransaction(trn1);

				// Job 3 (new flight) - message 1 & 2
				trn1.Reference5 = "JOB 3";
				trn1.Reference2 = "Flight 2";
				BillingDataTestHelper.AddTransaction(trn1);
				trn1.ServiceOccuredUTC = new DateTime(currentYear - 2, 8, 10, 9, 0, 0);
				BillingDataTestHelper.AddTransaction(trn1);

				// Job 4 (new carrier) - message 1 & 2 (29 days later)
				trn1.Reference5 = "JOB 4";
				trn1.Reference1 = "Carrier 2";
				trn1.ServiceOccuredUTC = new DateTime(currentYear - 2, 7, 31, 15, 0, 0);
				BillingDataTestHelper.AddTransaction(trn1);
				trn1.ServiceOccuredUTC = trn1.ServiceOccuredUTC.AddDays(29);
				BillingDataTestHelper.AddTransaction(trn1);

				// Job 5 (30 days, 20 hours later), 2 messages at same time
				trn1.Reference5 = "JOB 5";
				trn1.ServiceOccuredUTC = trn1.ServiceOccuredUTC.AddHours(24 + 20);
				BillingDataTestHelper.AddTransaction(trn1);
				trn1.Reference4 = "foo";
				BillingDataTestHelper.AddTransaction(trn1);

				ExecuteProcessStaging(con);

				// run again
				ExecuteProcessStaging(con);

				trn1.ServiceOccuredUTC = trn1.ServiceOccuredUTC.AddMinutes(5);
				BillingDataTestHelper.AddTransaction(trn1);
				trn1.ServiceOccuredUTC = trn1.ServiceOccuredUTC.AddMinutes(5);
				BillingDataTestHelper.AddTransaction(trn1);
				ExecuteProcessStaging(con);

				var table = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);

				Assert.That(table.Rows.Count, Is.EqualTo(5), "first message count");

				var rows = table.Rows.Cast<DataRow>().ToArray();
				var job1 = rows.Single(x => (string)x["CH_Reference5"] == "JOB 1");
				var job2 = rows.Single(x => (string)x["CH_Reference5"] == "JOB 2");
				var job3 = rows.Single(x => (string)x["CH_Reference5"] == "JOB 3");
				var job4 = rows.Single(x => (string)x["CH_Reference5"] == "JOB 4");
				var job5 = rows.Single(x => (string)x["CH_Reference5"] == "JOB 5");

				Assert.That(job1["CH_Reference1"], Is.EqualTo("Carrier 1"));
				Assert.That(job1["CH_Reference2"], Is.EqualTo("Flight 1"));
				Assert.That(job1["CH_Reference3"], Is.EqualTo(date0807));
				Assert.That(job1["CH_ServiceOccuredUTC"], Is.EqualTo(new DateTime(currentYear - 2, 8, 7, 9, 0, 0)));

				Assert.That(job2["CH_Reference1"], Is.EqualTo("Carrier 1"));
				Assert.That(job2["CH_Reference2"], Is.EqualTo("Flight 1"));
				Assert.That(job2["CH_Reference3"], Is.EqualTo(date0810));
				Assert.That(job2["CH_ServiceOccuredUTC"], Is.EqualTo(new DateTime(currentYear - 2, 8, 8, 9, 0, 0)));

				Assert.That(job3["CH_Reference1"], Is.EqualTo("Carrier 1"));
				Assert.That(job3["CH_Reference2"], Is.EqualTo("Flight 2"));
				Assert.That(job3["CH_Reference3"], Is.EqualTo(date0810));
				Assert.That(job3["CH_ServiceOccuredUTC"], Is.EqualTo(new DateTime(currentYear - 2, 8, 9, 9, 0, 0)));

				Assert.That(job4["CH_Reference1"], Is.EqualTo("Carrier 2"));
				Assert.That(job4["CH_Reference2"], Is.EqualTo("Flight 2"));
				Assert.That(job4["CH_Reference3"], Is.EqualTo(date0810));
				Assert.That(job4["CH_ServiceOccuredUTC"], Is.EqualTo(new DateTime(currentYear - 2, 7, 31, 15, 0, 0)));

				Assert.That(job5["CH_Reference1"], Is.EqualTo("Carrier 2"));
				Assert.That(job5["CH_Reference2"], Is.EqualTo("Flight 2"));
				Assert.That(job5["CH_Reference3"], Is.EqualTo(date0810));
				Assert.That(job5["CH_ServiceOccuredUTC"], Is.EqualTo(new DateTime(currentYear - 2, 8, 31, 11, 0, 0)));
			}
		}

		[Test]
		public void TestProcessEAD()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "SYD");

				var utcNow = DateTime.UtcNow;
				var currentYear = GetCurrentYear();

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ABCDEFXYZ",
					ClientNumber = "C.SYD",
					ClientStaffCode = "ABC",
					Category = "EAD",
					PriceItemCode = "ICJ",
					Reference1 = "Line ??",
					Reference2 = "Docket 1",
					Reference3 = "Warehouse 1",
					Reference4 = "Tracking ID 1",
					Reference5 = "",
					ReportingSource = "ENT",
					ServiceOccuredUTC = new DateTime(currentYear - 3, 6, 20),
					Version = 0,
				};

				// 25 at one time
				AddLines(con, 1, 25, trn1, utcNow);

				// 24 spread over three months.
				// Reported in reverse order.
				trn1.PriceItemCode = "ICK";
				trn1.ServiceOccuredUTC = new DateTime(currentYear - 3, 4, 20);
				AddLines(con, 1, 5, trn1, new DateTime(currentYear - 3, 6, 23));

				trn1.ServiceOccuredUTC = new DateTime(currentYear - 3, 5, 20);
				AddLines(con, 6, 20, trn1, new DateTime(currentYear - 3, 6, 22));

				trn1.ServiceOccuredUTC = new DateTime(currentYear - 3, 6, 20);
				AddLines(con, 21, 24, trn1, new DateTime(currentYear - 3, 6, 21));

				var endUtc = utcNow.AddDays(1);

				ExecuteProcessStaging(con);

				// run again
				ExecuteProcessStaging(con);

				var table = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);

				Assert.That(table.Rows.Count, Is.EqualTo(29), "order 1 excess line count");

				var order1Rows = table.Rows.Cast<DataRow>().Where(x => (string)x["CH_PriceItemCode"] == "ICJ").OrderBy(x => (string)x["CH_Reference3"]).ToArray();
				var order2Rows = table.Rows.Cast<DataRow>().Where(x => (string)x["CH_PriceItemCode"] == "ICK").OrderBy(x => (string)x["CH_Reference3"]).ToArray();

				Assert.That(order1Rows.Length, Is.EqualTo(15), "order 1 excess line count");
				Assert.That(order2Rows.Length, Is.EqualTo(14), "order 2 excess line count");

				Assert.That((string)order1Rows[0]["CH_Reference3"], Is.EqualTo("Line 011"));
				Assert.That((string)order1Rows[14]["CH_Reference3"], Is.EqualTo("Line 025"));
				Assert.That((DateTime)order1Rows[0]["CH_ServiceOccuredUTC"], Is.EqualTo(new DateTime(currentYear - 3, 6, 20)));
				Assert.That((DateTime)order1Rows[14]["CH_ServiceOccuredUTC"], Is.EqualTo(new DateTime(currentYear - 3, 6, 20)));

				Assert.That((string)order2Rows[0]["CH_Reference3"], Is.EqualTo("Line 011"));
				Assert.That((string)order2Rows[13]["CH_Reference3"], Is.EqualTo("Line 024"));
				Assert.That((DateTime)order2Rows[0]["CH_ServiceOccuredUTC"], Is.EqualTo(new DateTime(currentYear - 3, 5, 20)));
				Assert.That((DateTime)order2Rows[13]["CH_ServiceOccuredUTC"], Is.EqualTo(new DateTime(currentYear - 3, 6, 20)));
			}
		}

		[Test]
		public void TestProcessSHI()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "SYD");

				var utcNow = DateTime.UtcNow;
				var currentYear = GetCurrentYear();

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ABCDEFXYZ",
					ClientNumber = "C.SYD",
					ClientStaffCode = "ABC",
					Category = "SHI",
					PriceItemCode = "SHI",
					Reference1 = "Message ID 1",
					Reference2 = "ABCDEFXYZ",
					Reference3 = "Consol 1",
					Reference4 = "Carrier 1",
					Reference5 = "Booking Ref",
					ReportingSource = "HUB",
					ServiceOccuredUTC = new DateTime(currentYear - 3, 6, 20),
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trn1);

				trn1.Reference2 = "Provider";
				trn1.ServiceOccuredUTC = new DateTime(currentYear - 3, 6, 21);
				BillingDataTestHelper.AddTransaction(trn1);

				ExecuteProcessStaging(con);

				// run again
				ExecuteProcessStaging(con);

				var table = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");

				Assert.That(table.Rows.Count, Is.EqualTo(1), "only process if Ref2 != ClientID");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(0), "skipped row is not in Usage");

				var row = table.Rows[0];

				BillingDataTestHelper.AssertPropertiesAreEqual(trn1, table.Rows[0], "CH_");
				Assert.That(row["CH_DatabaseNumber"], Is.EqualTo(1));
				Assert.That(row["CH_CompanyNumber"], Is.EqualTo(1));
			}
		}

		[Test]
		public void TestProcessClientNumber()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "SYD", "AU");
				BillingDataTestHelper.AddCompany(con, 1, 2, "SIN", "SG");
				int currentYear = GetCurrentYear();


				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENTSINSRV",
					ClientNumber = null,
					ClientStaffCode = "",
					Category = "ZZZ",
					PriceItemCode = "ZZ1",
					Reference1 = "Ref 1",
					Reference2 = "",
					Reference3 = "",
					Reference4 = "",
					Reference5 = "",
					ReportingSource = "ERT",
					ServiceOccuredUTC = new DateTime(currentYear - 4, 1, 15),
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trn1);

				var trn2 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENTZZZSRV",
					ClientNumber = "C.SYD",
					ClientStaffCode = "",
					Category = "ZZZ",
					PriceItemCode = "ZZ1",
					Reference1 = "Ref 1",
					Reference2 = "",
					Reference3 = "",
					Reference4 = "",
					Reference5 = "",
					ReportingSource = "ERT",
					ServiceOccuredUTC = new DateTime(currentYear - 3, 1, 15),
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trn2);

				var trn3 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENTSINSRV",
					ClientNumber = "",
					ClientStaffCode = "",
					Category = "ZZZ",
					PriceItemCode = "ZZ1",
					Reference1 = "Ref 1",
					Reference2 = "",
					Reference3 = "",
					Reference4 = "",
					Reference5 = "",
					ReportingSource = "ERT",
					ServiceOccuredUTC = new DateTime(currentYear - 2, 1, 15),
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trn3);

				ExecuteProcessStaging(con);

				var table = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");

				Assert.That(table.Rows.Count, Is.EqualTo(3), "Chargeable rows");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(0), "Usage rows");

				var row1 = table.Rows.Cast<DataRow>().Single(x => (DateTime)x["CH_ServiceOccuredUtc"] == new DateTime(currentYear - 4, 1, 15));
				var row2 = table.Rows.Cast<DataRow>().Single(x => (DateTime)x["CH_ServiceOccuredUtc"] == new DateTime(currentYear - 3, 1, 15));
				var row3 = table.Rows.Cast<DataRow>().Single(x => (DateTime)x["CH_ServiceOccuredUtc"] == new DateTime(currentYear - 2, 1, 15));

				Assert.That(row1["CH_DatabaseNumber"], Is.EqualTo(1));
				Assert.That(row1["CH_CompanyNumber"], Is.EqualTo(2), "SIN company");
				Assert.That(row2["CH_DatabaseNumber"], Is.EqualTo(1));
				Assert.That(row2["CH_CompanyNumber"], Is.EqualTo(1), "SYD company");
				Assert.That(row3["CH_DatabaseNumber"], Is.EqualTo(1));
				Assert.That(row3["CH_CompanyNumber"], Is.EqualTo(2), "SIN company");
			}
		}

		[Test]
		public void TestProcessClientNumber_UpdateDatabaseNumberFromClientNumber_WhenNoCompanyFound()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.TruncateTable(con, "edi.StagingUnknownSystems");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabase(con, 1);
				int currentYear = GetCurrentYear();

				var trn = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENTZZZSRV",
					ClientNumber = "C.UNK",
					ClientStaffCode = "",
					Category = "ZZZ",
					PriceItemCode = "ZZ1",
					Reference1 = "Ref 1",
					Reference2 = "",
					Reference3 = "",
					Reference4 = "",
					Reference5 = "",
					ReportingSource = "ERT",
					ServiceOccuredUTC = new DateTime(currentYear - 3, 1, 15),
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trn);

				ExecuteProcessStaging(con);

				var table = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				var stagingUnknownTable = BillingDataTestHelper.LoadTableByName(con, "edi.StagingUnknownSystems");

				Assert.That(table.Rows.Count, Is.EqualTo(0), "Chargeable rows");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(0), "Usage rows");
				Assert.That(stagingUnknownTable.Rows.Count, Is.EqualTo(1), "StagingUnknownSystems rows");

				var row = stagingUnknownTable.Rows.Cast<DataRow>().Single(x => (DateTime)x["TX_ServiceOccuredUtc"] == new DateTime(currentYear - 3, 1, 15));

				Assert.That(row["DatabaseNumber"], Is.EqualTo(1));
				Assert.That(row["CompanyNumber"], Is.EqualTo(0));
			}
		}

		[Test]
		public void TestProcessClientNumberCMD()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "SYD", "AU");
				BillingDataTestHelper.AddCompany(con, 1, 2, "SIN", "SG");
				int currentYear = GetCurrentYear();

				var trnOld = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENTSRV",
					ClientNumber = Base27Encoding.Encode(1),
					ClientStaffCode = "",
					Category = "CMD",
					PriceItemCode = "CMD",
					Reference1 = "Ref 1",
					Reference2 = "",
					Reference3 = "",
					Reference4 = "",
					Reference5 = "",
					ReportingSource = "ERT",
					ServiceOccuredUTC = new DateTime(currentYear - 4, 1, 15),
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trnOld);

				var trnNew = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENT???SRV",
					ClientNumber = null,
					ClientStaffCode = "",
					Category = "CMD",
					PriceItemCode = "CMD",
					Reference1 = "Ref 1",
					Reference2 = "",
					Reference3 = "",
					Reference4 = "",
					Reference5 = "",
					ReportingSource = "ERT",
					ServiceOccuredUTC = new DateTime(currentYear - 3, 1, 15),
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trnNew);

				var trnUnmatched = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENZ???DBZ",
					ClientNumber = null,
					ClientStaffCode = "",
					Category = "CMD",
					PriceItemCode = "CMD",
					Reference1 = "Blah",
					Reference2 = "",
					Reference3 = "",
					Reference4 = "",
					Reference5 = "",
					ReportingSource = "ERT",
					ServiceOccuredUTC = new DateTime(currentYear - 3, 1, 16),
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trnUnmatched);

				ExecuteProcessStaging(con);

				var table = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				var stagingUnknownTable = BillingDataTestHelper.LoadTableByName(con, "edi.StagingUnknownSystems");

				Assert.That(table.Rows.Count, Is.EqualTo(2), "Chargeable rows");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(0), "Usage rows");
				Assert.That(stagingUnknownTable.Rows.Count, Is.EqualTo(1), "StagingUnknownSystems rows");

				var rowOld = table.Rows.Cast<DataRow>().Single(x => (DateTime)x["CH_ServiceOccuredUtc"] == new DateTime(currentYear - 4, 1, 15));
				var rowNew = table.Rows.Cast<DataRow>().Single(x => (DateTime)x["CH_ServiceOccuredUtc"] == new DateTime(currentYear - 3, 1, 15));
				var rowUnmatched = stagingUnknownTable.Rows.Cast<DataRow>().Single(x => (DateTime)x["TX_ServiceOccuredUtc"] == new DateTime(currentYear - 3, 1, 16));

				Assert.That(rowOld["CH_DatabaseNumber"], Is.EqualTo(1));
				Assert.That(rowOld["CH_CompanyNumber"], Is.EqualTo(2), "SG company");
				Assert.That(rowNew["CH_DatabaseNumber"], Is.EqualTo(1));
				Assert.That(rowNew["CH_CompanyNumber"], Is.EqualTo(2), "SG company");
			}
		}

		[Test]
		public void TestProcessClientNumberAMS()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "SYD", "AU");
				BillingDataTestHelper.AddCompany(con, 1, 2, "SIN", "SG");
				int currentYear = GetCurrentYear();

				var trnOld = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENTSYD",
					ClientNumber = null,
					ClientStaffCode = "",
					Category = "AMS",
					PriceItemCode = "AMS",
					Reference1 = "Ref 1",
					Reference2 = "",
					Reference3 = "",
					Reference4 = "",
					Reference5 = "",
					ReportingSource = "ERT",
					ServiceOccuredUTC = new DateTime(currentYear - 4, 1, 15),
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trnOld);

				var trnNew = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENTSIN???",
					ClientNumber = null,
					ClientStaffCode = "",
					Category = "AMS",
					PriceItemCode = "AMS",
					Reference1 = "Ref 1",
					Reference2 = "",
					Reference3 = "",
					Reference4 = "",
					Reference5 = "",
					ReportingSource = "ERT",
					ServiceOccuredUTC = new DateTime(currentYear - 3, 1, 15),
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trnNew);

				ExecuteProcessStaging(con);

				var table = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");

				Assert.That(table.Rows.Count, Is.EqualTo(2), "Chargeable rows");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(0), "Usage rows");

				var rowOld = table.Rows.Cast<DataRow>().Single(x => (DateTime)x["CH_ServiceOccuredUtc"] == new DateTime(currentYear - 4, 1, 15));
				var rowNew = table.Rows.Cast<DataRow>().Single(x => (DateTime)x["CH_ServiceOccuredUtc"] == new DateTime(currentYear - 3, 1, 15));

				Assert.That(rowOld["CH_DatabaseNumber"], Is.EqualTo(1));
				Assert.That(rowOld["CH_CompanyNumber"], Is.EqualTo(1), "SYD company");
				Assert.That(rowNew["CH_DatabaseNumber"], Is.EqualTo(1));
				Assert.That(rowNew["CH_CompanyNumber"], Is.EqualTo(2), "SIN company");
			}
		}

		[TestCase("SMF", "")]
		[TestCase("SMF", "SMF")]
		[TestCase("SPM", "SHP")]
		public void TestProcessClientNumberBtTenantId(string testProduct, string testCategory)
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabase(con, 1, product: "CW1");
				BillingDataTestHelper.AddDatabase(con, 2, product: testProduct, tenantId: "C", category: testCategory);
				int currentYear = GetCurrentYear();

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "?????????",
					ClientNumber = "C",
					ClientStaffCode = "",
					Category = string.IsNullOrEmpty(testCategory) ? testProduct : testCategory,
					PriceItemCode = "ABC",
					Reference1 = "Ref 1",
					Reference2 = "",
					Reference3 = "",
					Reference4 = "",
					Reference5 = "",
					ReportingSource = testProduct,
					ServiceOccuredUTC = new DateTime(currentYear - 1, 1, 15),
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trn1);

				ExecuteProcessStaging(con);

				var table = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");

				Assert.That(table.Rows.Count, Is.EqualTo(1), "Chargeable rows");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(0), "Usage rows");

				var row1 = table.Rows[0];

				Assert.That(row1["CH_DatabaseNumber"], Is.EqualTo(2));
				Assert.That(row1["CH_CompanyNumber"], Is.EqualTo(1), "SYD company");
			}
		}

		[Test]
		public void TestProcessClientNumber_CompanyCodeUnknown()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "SYD", "AU", "SRV");
				BillingDataTestHelper.AddCompany(con, 1, 2, "SIN", "SG", 2);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 2, 1, "SIN", "SG", "DB2");
				int currentYear = GetCurrentYear();

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENT???SRV",
					ClientNumber = null,
					ClientStaffCode = "",
					Category = "HOS",
					PriceItemCode = "#HG",
					Reference1 = "Ref 1",
					Reference2 = "",
					Reference3 = "",
					Reference4 = "",
					Reference5 = "",
					ReportingSource = "MSC",
					ServiceOccuredUTC = new DateTime(currentYear - 4, 1, 15),
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trn1);

				ExecuteProcessStaging(con);

				var table = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");

				Assert.That(table.Rows.Count, Is.EqualTo(1), "Chargeable rows");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(0), "Usage rows");

				var row1 = table.Rows.Cast<DataRow>().Single(x => (DateTime)x["CH_ServiceOccuredUtc"] == new DateTime(currentYear - 4, 1, 15));

				Assert.That(row1["CH_DatabaseNumber"], Is.EqualTo(1));
				Assert.That(row1["CH_CompanyNumber"], Is.EqualTo(1), "SYD company");
			}
		}

		[Test]
		public void TestProcessClientNumber_ProductWithNoClientCompany()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);

				var currentYear = GetCurrentYear();

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "?????????",
					ClientNumber = Base27Encoding.Encode(7),
					ClientStaffCode = "",
					Category = "BOR",
					PriceItemCode = "BOR",
					Reference1 = "Ref 1",
					Reference2 = "Ref 2",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "BOR",
					ServiceOccuredUTC = new DateTime(currentYear - 1, 12, 11),
					Version = 0,
				};

				var trn2 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "?????????",
					ClientNumber = Base27Encoding.Encode(10),
					ClientStaffCode = "",
					Category = "WTA",
					PriceItemCode = "TST",
					Reference1 = "6bf585c3-e2b8-444e-95c6-0813d01aad6f",
					Reference2 = "",
					Reference3 = "",
					Reference4 = "",
					Reference5 = "",
					ReportingSource = "WTA",
					ServiceOccuredUTC = new DateTime(currentYear - 1, 12, 14),
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trn1);
				BillingDataTestHelper.AddTransaction(trn2);

				ExecuteProcessStaging(con);

				var table = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");

				Assert.That(table.Rows.Count, Is.EqualTo(2), "Chargeable rows");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(0), "Usage rows");

				var row1 = table.Rows.Cast<DataRow>().Single(x => (DateTime)x["CH_ServiceOccuredUtc"] == new DateTime(currentYear - 1, 12, 11));

				Assert.That(row1["CH_DatabaseNumber"], Is.EqualTo(7));
				Assert.That(row1["CH_CompanyNumber"], Is.EqualTo(1));

				var row2 = table.Rows.Cast<DataRow>().Single(x => (DateTime)x["CH_ServiceOccuredUtc"] == new DateTime(currentYear - 1, 12, 14));

				Assert.That(row2["CH_DatabaseNumber"], Is.EqualTo(10));
				Assert.That(row2["CH_CompanyNumber"], Is.EqualTo(1));
			}
		}

		[Test]
		public void TestProcessClientNumber_InvalidClientNumber()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);

				var currentYear = GetCurrentYear();
				var transactionTime = new DateTime(currentYear - 1, 12, 11);
				var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(transactionTime);

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "???COMBOR",
					ClientNumber = "A",
					ClientStaffCode = "",
					Category = "BOR",
					PriceItemCode = "BOR",
					Reference1 = "Ref 1",
					Reference2 = "Ref 2",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "BOR",
					ServiceOccuredUTC = transactionTime,
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trn1);

				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, daysAfter: 1, performMonthlyAggregation: false, isTestServer: false);

				var table = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				var stagingUnknownTable = BillingDataTestHelper.LoadTableByName(con, "edi.StagingUnknownSystems");

				Assert.That(table.Rows.Count, Is.EqualTo(0), "Chargeable rows");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(0), "Usage rows");
				Assert.That(stagingUnknownTable.Rows.Count, Is.EqualTo(1), "StagingUnknownSystems rows");

				var row1 = stagingUnknownTable.Rows.Cast<DataRow>().Single(x => (DateTime)x["TX_ServiceOccuredUtc"] == new DateTime(currentYear - 1, 12, 11));
			}
		}

		[Test]
		public void TestProcessClientNumber_InvalidClientNumberOnTestServer()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);

				var currentYear = GetCurrentYear();
				var transactionTime = new DateTime(currentYear - 1, 12, 11);
				var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(transactionTime);

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "???COMBOR",
					ClientNumber = "A",
					ClientStaffCode = "",
					Category = "BOR",
					PriceItemCode = "BOR",
					Reference1 = "Ref 1",
					Reference2 = "Ref 2",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "BOR",
					ServiceOccuredUTC = transactionTime,
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trn1);

				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, daysAfter: 1, performMonthlyAggregation: false, isTestServer: true);

				var table = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");

				Assert.That(table.Rows.Count, Is.EqualTo(1), "Chargeable rows");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(0), "Usage rows");

				var row1 = table.Rows.Cast<DataRow>().Single(x => (DateTime)x["CH_ServiceOccuredUtc"] == new DateTime(currentYear - 1, 12, 11));

				Assert.That(row1["CH_DatabaseNumber"], Is.EqualTo(1));
				Assert.That(row1["CH_CompanyNumber"], Is.EqualTo(1));
			}
		}

		[Test]
		public void TestFirstMessageRIC()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "DEF");
				BillingDataTestHelper.AddDatabaseAndCompany(con, 3141, 2, "PDO", "NZ", "HST");

				var currentYear = GetCurrentYear();

				// Message 1 - RIM-RIM is not chargeable
				var transaction = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ABCDEFXYZ",
					ClientNumber = "C.DEF",
					ClientStaffCode = "ABC",
					Category = "RIM",
					PriceItemCode = "RIM",
					Reference1 = "M02",
					Reference2 = "C01",
					ReportingSource = "XYZ",
					ServiceOccuredUTC = new DateTime(currentYear - 3, 8, 1, 10, 0, 0),
					Version = 1,
				};

				BillingDataTestHelper.AddTransaction(transaction);

				// Message 2
				transaction.PriceItemCode = "RIC";
				transaction.Reference1 = "M03";
				transaction.Reference3 = "CLM";
				transaction.Version = 2;
				transaction.ServiceOccuredUTC = new DateTime(currentYear - 3, 7, 31, 14, 0, 0);
				BillingDataTestHelper.AddTransaction(transaction);

				// Message 3
				transaction.Version = 3;
				transaction.ServiceOccuredUTC = new DateTime(currentYear - 3, 7, 31, 15, 0, 0);
				BillingDataTestHelper.AddTransaction(transaction);

				// Message 4
				transaction.Reference1 = "M04";
				transaction.Reference3 = "CLM";
				transaction.Version = 1;
				transaction.ServiceOccuredUTC = new DateTime(currentYear - 3, 9, 1, 10, 0, 0);
				BillingDataTestHelper.AddTransaction(transaction);

				// Message 5 - CLU is not chargeable
				transaction.Reference1 = "M98";
				transaction.Reference3 = "CLU";
				transaction.ServiceOccuredUTC = new DateTime(currentYear - 3, 8, 2, 10, 0, 0);
				BillingDataTestHelper.AddTransaction(transaction);

				ExecuteProcessStaging(con);

				// run again
				ExecuteProcessStaging(con);

				var table = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				Assert.That(table.Rows.Count, Is.EqualTo(3), "CLM goes to edi.Chargeable");

				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(2), "CLU and RIM-RIM goes to edi.Usage");

				var message1 = usageTable.Rows.Cast<DataRow>().Single(x => (string)x["US_Reference1"] == "M02");
				Assert.That(message1["US_PriceItemCode"], Is.EqualTo("RIM"));
				Assert.That(message1["US_ServiceOccuredUTC"], Is.EqualTo(new DateTime(currentYear - 3, 8, 1, 10, 0, 0)));

				var message2 = table.Rows.Cast<DataRow>().Single(x => (string)x["CH_Reference1"] == "M03" && (int)x["CH_Version"] == 2);
				Assert.That(message2["CH_BillableCount"], Is.EqualTo(1));
				Assert.That(message2["CH_ClientID"], Is.EqualTo("ABCDEFXYZ"));
				Assert.That(message2["CH_ClientNumber"], Is.EqualTo("C.DEF"));
				Assert.That(message2["CH_ClientStaffCode"], Is.EqualTo("ABC"));
				Assert.That(message2["CH_Category"], Is.EqualTo("RIM"));
				Assert.That(message2["CH_PriceItemCode"], Is.EqualTo("RIC"));
				Assert.That(message2["CH_Reference2"], Is.EqualTo("C01"));
				Assert.That(message2["CH_Reference3"], Is.EqualTo("CLM"));
				Assert.That(message2["CH_ReportingSource"], Is.EqualTo("XYZ"));
				Assert.That(message2["CH_ServiceOccuredUTC"], Is.EqualTo(new DateTime(currentYear - 3, 7, 31, 14, 0, 0)));

				var message3 = table.Rows.Cast<DataRow>().Single(x => (string)x["CH_Reference1"] == "M03" && (int)x["CH_Version"] == 3);
				Assert.That(message3["CH_PriceItemCode"], Is.EqualTo("RIC"));
				Assert.That(message3["CH_ServiceOccuredUTC"], Is.EqualTo(new DateTime(currentYear - 3, 7, 31, 15, 0, 0)));

				var message4 = table.Rows.Cast<DataRow>().Single(x => (string)x["CH_Reference1"] == "M04");
				Assert.That(message4["CH_PriceItemCode"], Is.EqualTo("RIC"));
				Assert.That(message4["CH_ServiceOccuredUTC"], Is.EqualTo(new DateTime(currentYear - 3, 9, 1, 10, 0, 0)));

				var message5 = usageTable.Rows.Cast<DataRow>().Single(x => (string)x["US_Reference1"] == "M98");
				Assert.That(message5["US_PriceItemCode"], Is.EqualTo("RIC"));
				Assert.That(message5["US_ServiceOccuredUTC"], Is.EqualTo(new DateTime(currentYear - 3, 8, 2, 10, 0, 0)));
			}
		}

		[Test]
		public void TestProcessCMP()
		{
			// Note, references are swapped in processing
			// Ref4 is interface name -> 1; Ref3 is Element -> 2; Ref2 = file name -> 3; Ref1 = tracking ID -> 4
			// ('CMP', 'CMP', 0, NULL, 4, 3, 2, 1, 5)

			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.TruncateTable(con, "edi.ClientMappingInterface");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "DEF");

				var utcStart = DateTime.UtcNow;
				var currentYear = GetCurrentYear();

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENTDEFSRV",
					ClientNumber = "C.DEF",
					ClientStaffCode = "ABC",
					Category = "CMP",
					PriceItemCode = "CMP",
					Reference1 = "ID 1",
					Reference2 = "FileName",
					Reference3 = "Element",
					Reference4 = "Interface 1",
					ReportingSource = "HUB",
					Version = 1,
					ServiceOccuredUTC = new DateTime(currentYear - 3, 8, 20)
				};

				BillingDataTestHelper.AddTransaction(trn1);

				trn1.Reference1 = "ID 2";
				trn1.Reference4 = "Interface 2";
				BillingDataTestHelper.AddTransaction(trn1);

				ExecuteProcessStaging(con);

				trn1.Reference1 = "ID 3";
				trn1.ServiceOccuredUTC = new DateTime(currentYear - 3, 8, 21);
				trn1.Reference4 = "Interface 3";
				BillingDataTestHelper.AddTransaction(trn1);

				trn1.Reference1 = "ID 4";
				trn1.Reference4 = "Interface 2";
				BillingDataTestHelper.AddTransaction(trn1);

				var utcEnd = DateTime.UtcNow;

				ExecuteProcessStaging(con);

				var table = BillingDataTestHelper.LoadTableByName(con, "edi.ClientMappingInterface");

				Assert.That(table.Rows.Count, Is.EqualTo(3), "number of interfaces");

				var row1 = table.Rows.Cast<DataRow>().Single(x => (string)x["Name"] == "Interface 1");
				var row2 = table.Rows.Cast<DataRow>().Single(x => (string)x["Name"] == "Interface 2");
				var row3 = table.Rows.Cast<DataRow>().Single(x => (string)x["Name"] == "Interface 3");

				Assert.GreaterOrEqual((DateTime)row1["FirstCapturedUtc"], utcStart.AddMinutes(-5));
				Assert.GreaterOrEqual((DateTime)row2["FirstCapturedUtc"], (DateTime)row1["FirstCapturedUtc"]);
				Assert.GreaterOrEqual((DateTime)row3["FirstCapturedUtc"], (DateTime)row2["FirstCapturedUtc"]);
			}
		}

		[Test]
		public void TestProcessCMPWhenRef4IsNull()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.TruncateTable(con, "edi.ClientMappingInterface");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "DEF");

				var utcStart = DateTime.UtcNow;
				var currentYear = GetCurrentYear();

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENTDEFSRV",
					ClientNumber = "C.DEF",
					ClientStaffCode = "ABC",
					Category = "CMP",
					PriceItemCode = "CMP",
					Reference1 = "ID 1",
					Reference2 = "FileName",
					Reference3 = "Element",
					Reference4 = null,
					ReportingSource = "HUB",
					Version = 1,
					ServiceOccuredUTC = new DateTime(currentYear - 3, 8, 20)
				};

				BillingDataTestHelper.AddTransaction(trn1);

				ExecuteProcessStaging(con);

				var mappingInterfaces = BillingDataTestHelper.LoadTableByName(con, "edi.ClientMappingInterface");
				Assert.That(mappingInterfaces.Rows.Count, Is.EqualTo(0), "number of interfaces");

				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				var chargeableTable = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				Assert.That(usageTable.Rows.Count, Is.EqualTo(1), "One record processed into Usage table");
				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(0), "No records should be processed into Chargeable table");
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn1, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, usageTable.Rows[0]);
			}
		}

		[Test]
        public void TestProcessWGR()
        {
            using (var con = BillingDataTestHelper.GetNewOpenConnection())
            {
                BillingDataTestHelper.TruncateStagingTable(con);
                BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
                BillingDataTestHelper.TruncateTable(con, "edi.Usage");
                BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
                BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "SYD");

				var utcNow = DateTime.UtcNow;
                var currentYear = GetCurrentYear();

                var trn1 = new API.BillingTransaction
                {
                    BillableCount = 123,
					ClientID = "ENT???SRV",
                    ClientNumber = null,
					Category = "WGR",
                    PriceItemCode = "WGR",
                    Reference1 = "900",
                    Reference2 = "ClientDirect",
                    Reference3 = "Client",
                    Reference4 = "LONWP-SSQL-4A.wisecloud.zone/INSTANCEP1",
                    Reference5 = "8788",
                    ReportingSource = "MSC",
                    ServiceOccuredUTC = new DateTime(currentYear - 3, 6, 20),
                    Version = 0,
                };

				var trn2 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "PFS???HST",
					ClientNumber = null,
					Category = "WGR",
					PriceItemCode = "WGR",
					Reference1 = "Ref 1",
					Reference2 = "Ref 2",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "ENT",
					ServiceOccuredUTC = new DateTime(currentYear - 3, 6, 20),
					Version = 0,
				};

				BillingDataTestHelper.AddDatabaseAndCompany(con, 5093, 1, "RED", serverCode: "HST", hostedLocation: "SYD", enterpriseCode: "PFS");
				
				BillingDataTestHelper.AddTransaction(trn1);
				BillingDataTestHelper.AddTransaction(trn2);

				ExecuteProcessStaging(con);

				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				var chargeableTable = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);

				Assert.That(usageTable.Rows.Count, Is.EqualTo(2), "Records should only be processed into Usage table");
				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(0), "No records should be processed into Chargeable table");
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn1, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, usageTable.Rows[0]);
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn2, expectedDatabaseNumber: 5093, expectedCompanyNumber: 1, usageTable.Rows[1]);
            }
        }

		[Test]
		public void TestProcessWGRWithSP()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 2, 1, "TST", licenceType: "TST");

				BillingDataTestHelper.ExecuteNonQuery(con,
					$@"
						INSERT [CargoWise.eServices.Billing.UnitTesting].edi.StagingBatch(TX_ID, TX_Category, TX_PriceItemCode, TX_BillableCount, TX_ReportingSource, TX_ServiceOccuredUTC, TX_ClientID, TX_ClientNumber, TX_ClientStaffCode, TX_Reference1, TX_Reference2, TX_Reference3, TX_Reference4, TX_Reference5, TX_SystemCreateUTC, TX_Version, TX_Branch, TX_MessageTrackingID, TX_Period, IsRefSwapped, ProcessingStatus, DatabaseNumber)
						VALUES(1, 'WGR', 'WGR', 1, 'HUB', '01/01/0001 00:00:00', 'ABCDEFXYZ', 'C.SYD', 'ABC', '', '', '', '', '', '01/01/0001 00:00:00', 0, '', '', 0, 0, 0, 2)
					");

				BillingDataTestHelper.ExecuteStoredProcedure(con, "edi.ProcessWGR");

				var row = BillingDataTestHelper.LoadTableByName(con, "edi.StagingBatch").Rows[0];
				Assert.That(row["ProcessingStatus"], Is.EqualTo(255));
			}
		}

		[Test]
		public void TestProcessWGRUnknownSystem()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "SYD");

				var utcNow = DateTime.UtcNow;
				var currentYear = GetCurrentYear();

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 123,
					ClientID = "UNK???UNK",
					ClientNumber = null,
					Category = "WGR",
					PriceItemCode = "WGR",
					Reference1 = "900",
					Reference2 = "ClientDirect",
					Reference3 = "Client",
					Reference4 = "LONWP-SSQL-4A.wisecloud.zone/INSTANCEP1",
					Reference5 = "8788",
					ReportingSource = "MSC",
					ServiceOccuredUTC = new DateTime(currentYear - 3, 6, 20),
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trn1);

				ExecuteProcessStaging(con);

				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				var stagingUnknownTable = BillingDataTestHelper.LoadTableByName(con, "edi.StagingUnknownSystems");
				var chargeableTable = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);

				Assert.That(usageTable.Rows.Count, Is.EqualTo(0), "No records should be processed into Usage table");
				Assert.That(stagingUnknownTable.Rows.Count, Is.EqualTo(1), "Records should only be processed into StagingUnkownSystems table");
				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(0), "No records should be processed into Chargeable table");
				var row1 = stagingUnknownTable.Rows.Cast<DataRow>().Single(x => (DateTime)x["TX_ServiceOccuredUtc"] == new DateTime(currentYear - 3, 6, 20));
			}
		}

		[Test]
		public void TestProcessWGRAfterCompanyAdded()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabase(con, 1);

				var utcNow = DateTime.UtcNow;
				var currentYear = GetCurrentYear();

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 123,
					ClientID = "ENT???SRV",
					ClientNumber = null,
					Category = "WGR",
					PriceItemCode = "WGR",
					Reference1 = "900",
					Reference2 = "ClientDirect",
					Reference3 = "Client",
					Reference4 = "LONWP-SSQL-4A.wisecloud.zone/INSTANCEP1",
					Reference5 = "8788",
					ReportingSource = "MSC",
					ServiceOccuredUTC = new DateTime(currentYear - 3, 6, 20),
					Version = 0,
				};

				var trn2 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "PFS???HST",
					ClientNumber = null,
					Category = "WGR",
					PriceItemCode = "WGR",
					Reference1 = "Ref 1",
					Reference2 = "Ref 2",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "ENT",
					ServiceOccuredUTC = new DateTime(currentYear - 3, 6, 20),
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trn1);
				BillingDataTestHelper.AddTransaction(trn2);

				ExecuteProcessStaging(con);

				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				var stagingUnknownTable = BillingDataTestHelper.LoadTableByName(con, "edi.StagingUnknownSystems");
				var chargeableTable = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);

				Assert.That(usageTable.Rows.Count, Is.EqualTo(0), "No records should be processed into Usage table");
				Assert.That(stagingUnknownTable.Rows.Count, Is.EqualTo(2), "Records should only be processed into StagingUnknownSystems table");
				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(0), "No records should be processed into Chargeable table");

				BillingDataTestHelper.AddCompany(con, 1, 1, "SYD");
				ExecuteProcessStaging(con);

				usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				stagingUnknownTable = BillingDataTestHelper.LoadTableByName(con, "edi.StagingUnknownSystems");
				chargeableTable = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);

				Assert.That(usageTable.Rows.Count, Is.EqualTo(1), "Record with valid company should be processed into the usage table");
				Assert.That(stagingUnknownTable.Rows.Count, Is.EqualTo(1), "Only record with invalid company should be in StagingUnknownSystems table now");
				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(0), "No records should be processed into Chargeable table");
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn1, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, usageTable.Rows[0]);
			}
		}

		[TestCase("PRT")]
		[TestCase("PRS")]
		[TestCase("STS")]
		public void TestProcessSelfHostedUsageData(string priceItemCode)
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "SYD");
				BillingDataTestHelper.AddDatabaseAndCompany(con, 2, 2, "DK1", serverCode: "GER", hostedLocation: "NCW");

				var utcNow = DateTime.UtcNow;
				var currentYear = GetCurrentYear();
				var transactionTime = new DateTime(currentYear - 3, 6, 20);

				var trn1 = BillingDataTestHelper.CreateAndAddTransaction(priceItemCode, transactionTime, clientId: "ENTSYDSRV", reference1: "Tran1", reference5: "F5DF1514-4D7F-4B24-971C-EC10549EED60");
				var trn2 = BillingDataTestHelper.CreateAndAddTransaction(priceItemCode, transactionTime, clientId: "ENTDK1GER", reference1: "Tran2", reference5: "2F4BBF1D-EE6B-42C5-A07B-A62085CCCB6C");

				ExecuteProcessStaging(con);

				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				var chargeableTable = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);

				Assert.That(usageTable.Rows.Count, Is.EqualTo(1), "Self Hosted Records should be processed into Usage table");
				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(1), "Wistech Hosted Records should be processed into Chargeable table");
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn2, expectedDatabaseNumber: 2, expectedCompanyNumber: 2, usageTable.Rows[0]);
				BillingDataTestHelper.AssertChargeablePropertiesAreEqual(trn1, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, chargeableTable.Rows[0]);
			}
		}

		[Test]
        public void TestProcessOldWareHousePackaging()
        {
            using (var con = BillingDataTestHelper.GetNewOpenConnection())
            {
                BillingDataTestHelper.TruncateStagingTable(con);
                BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
                BillingDataTestHelper.TruncateTable(con, "edi.Usage");
                BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
                BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "SYD");

                var utcNow = DateTime.UtcNow;
                var currentYear = GetCurrentYear();

                var trn1 = new API.BillingTransaction
                {
                    BillableCount = 123,
                    ClientID = "ENT???SRV",
                    ClientNumber = null,
                    Category = "STL",
                    PriceItemCode = "WTP",
                    Reference1 = " PBJ//-*/4",
                    Reference2 = "RC00011590",
                    Reference3 = "",
                    Reference4 = "",
                    Reference5 = "E5135320-6820-42D7-9033-4B9529171AC0",
                    ReportingSource = "ENT",
                    ServiceOccuredUTC = new DateTime(currentYear - 3, 6, 20),
                    Version = 0,
                };

                BillingDataTestHelper.AddTransaction(trn1);

                ExecuteProcessStaging(con);

                var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
                var chargeableTable = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);

                Assert.That(usageTable.Rows.Count, Is.EqualTo(1), "Records should only be processed into Usage table");
                Assert.That(chargeableTable.Rows.Count, Is.EqualTo(0), "No records should be processed into Chargeable table");
                BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn1, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, usageTable.Rows[0]);
            }
        }

        [Test]
		public void TestProcessOldEInvoicing()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "SYD");

				var utcNow = DateTime.UtcNow;
				var currentYear = GetCurrentYear();

				var serviceOccuredUtc = new DateTime(currentYear - 3, 6, 20);
				var transaction6Time = new DateTime(currentYear - 3, 6, 25);
				var acc = "ACC";
				var trn1 = BillingDataTestHelper.CreateAndAddTransaction(priceItemCode: "HU1", serviceOccuredUtc, category: acc, reference1: "AC2100041", reference2: "SHIB21001219/A", reference3: "True", reference4: "ARINV");
				var trn2 = BillingDataTestHelper.CreateAndAddTransaction(priceItemCode: "HU2", serviceOccuredUtc, category: acc, reference1: "AC2100041", reference2: "SHIB21001219/A", reference3: "True", reference4: "ARINV");
				var trn3 = BillingDataTestHelper.CreateAndAddTransaction(priceItemCode: "IN1", serviceOccuredUtc, category: acc, reference1: "PMBFIA2200001327", reference2: "S00753912", reference3: "True", reference4: "ARINV");
				var trn4 = BillingDataTestHelper.CreateAndAddTransaction(priceItemCode: "IN2", serviceOccuredUtc, category: acc, reference1: "PMBFIA2200001327", reference2: "S00753912", reference3: "True", reference4: "ARINV");
				var trn5 = BillingDataTestHelper.CreateAndAddTransaction(priceItemCode: "IT1", serviceOccuredUtc, category: acc, reference1: "PSB00003870", reference2: "STMIL5640032/B", reference4: "ARCRD");
				var trn6 = BillingDataTestHelper.CreateAndAddTransaction(priceItemCode: "IT2", serviceOccuredUtc, category: acc, reference1: "PSB00003870", reference2: "STMIL5640032/B", reference4: "ARCRD");
				var trn7 = BillingDataTestHelper.CreateAndAddTransaction(priceItemCode: "TW1", serviceOccuredUtc, category: acc, reference1: "VF45617509", reference4: "ARCRD");
				var trn8 = BillingDataTestHelper.CreateAndAddTransaction(priceItemCode: "TW2", serviceOccuredUtc, category: acc, reference1: "VF45617509", reference4: "ARCRD");
				var trn9 = BillingDataTestHelper.CreateAndAddTransaction(priceItemCode: "WS1", serviceOccuredUtc, category: acc, reference1: "00002941", reference2: "VPFVAKLAPW108863/A/A", reference3: "True", reference4: "ARCRD");
				var trn10 = BillingDataTestHelper.CreateAndAddTransaction(priceItemCode: "WS2", serviceOccuredUtc, category: acc, reference1: "00002941", reference2: "VPFVAKLAPW108863/A/A", reference3: "True", reference4: "ARCRD");
				var trn11 = BillingDataTestHelper.CreateAndAddTransaction(priceItemCode: "DM1", transaction6Time, category: acc, reference1: "BLA1", reference2: "BLABLA/A/A", reference4: "ARCRD");

				ExecuteProcessStaging(con);

				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				var chargeableTable = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);

				Assert.That(usageTable.Rows.Count, Is.EqualTo(10), "Old eInvoicing should be processed into Usage table");
				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(1), "non-old eInvoicing should be processed into Chargeable table");

				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn1, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, usageTable.Rows[0]);
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn2, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, usageTable.Rows[1]);
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn3, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, usageTable.Rows[2]);
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn4, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, usageTable.Rows[3]);
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn5, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, usageTable.Rows[4]);
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn6, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, usageTable.Rows[5]);
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn7, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, usageTable.Rows[6]);
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn8, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, usageTable.Rows[7]);
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn9, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, usageTable.Rows[8]);
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn10, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, usageTable.Rows[9]);

				var chargeableRow = chargeableTable.Rows[0];
				Assert.That(chargeableRow["CH_BillableCount"], Is.EqualTo(1));
				Assert.That(chargeableRow["CH_ClientID"], Is.EqualTo("ENT???SRV"));
				Assert.That(chargeableRow["CH_Category"], Is.EqualTo("ACC"));
				Assert.That(chargeableRow["CH_PriceItemCode"], Is.EqualTo("DM1"));
				Assert.That(chargeableRow["CH_Reference1"], Is.EqualTo("BLA1"));
				Assert.That(chargeableRow["CH_Reference2"], Is.EqualTo("BLABLA/A/A"));
				Assert.That(chargeableRow["CH_Reference3"], Is.EqualTo(DBNull.Value));
				Assert.That(chargeableRow["CH_Reference4"], Is.EqualTo("ARCRD"));
				Assert.That(chargeableRow["CH_ReportingSource"], Is.EqualTo("ENT"));
				Assert.That(chargeableRow["CH_ServiceOccuredUTC"], Is.EqualTo(transaction6Time));
				Assert.That(chargeableRow["CH_Version"], Is.EqualTo(1));
				Assert.That(chargeableRow["CH_DatabaseNumber"], Is.EqualTo(1));
				Assert.That(chargeableRow["CH_CompanyNumber"], Is.EqualTo(1));
				Assert.That(chargeableRow["CH_Period"], Is.EqualTo((transaction6Time.Year * 100) + transaction6Time.Month));
			}
		}

		[Test]
		public void TestProcessECommerceHVL()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "SYD");

				var utcNow = DateTime.UtcNow;
				var currentYear = GetCurrentYear();
				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENT???SRV",
					ClientNumber = null,
					Category = "STL",
					PriceItemCode = "AM2",
					Reference1 = "AMS0000156",
					Reference2 = "5163624000",
					Reference3 = "284931345760",
					Reference4 = "HVL",
					Reference5 = "9571B29C-48CD-495E-9400-38F1524D5E52",
					ReportingSource = "ENT",
					ServiceOccuredUTC = new DateTime(currentYear - 3, 6, 20),
					Version = 0,
				};

				var trn2 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENT???SRV",
					ClientNumber = null,
					Category = "STL",
					PriceItemCode = "AM2",
					Reference1 = "C00072779",
					Reference2 = "MOIPEV08132",
					Reference3 = "00205014",
					Reference4 = "PGAA",
					Reference5 = "FCC9ED66-B7FF-4003-86F8-25E301632B20",
					ReportingSource = "ENT",
					ServiceOccuredUTC = new DateTime(currentYear - 3, 6, 21),
					Version = 0,
				};

				var trn3 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENT???SRV",
					ClientNumber = null,
					Category = "STL",
					PriceItemCode = "IS2",
					Reference1 = "AMS0000157",
					Reference2 = "4257505000",
					Reference3 = "YW211031131",
					Reference4 = "HVL",
					Reference5 = "B64F6F2C-EB4B-45E5-8607-D4CB4F8F1BCE",
					ReportingSource = "ENT",
					ServiceOccuredUTC = new DateTime(currentYear - 3, 6, 22),
					Version = 0,
				};

				var trn4 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENT???SRV",
					ClientNumber = null,
					Category = "STL",
					PriceItemCode = "IS2",
					Reference1 = "CM00149010",
					Reference2 = "TT118170",
					Reference3 = "SM00192443",
					Reference4 = "MZLM",
					Reference5 = "934673CA-44E8-4456-8310-4862837C4465",
					ReportingSource = "ENT",
					ServiceOccuredUTC = new DateTime(currentYear - 3, 6, 23),
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trn1);
				BillingDataTestHelper.AddTransaction(trn2);
				BillingDataTestHelper.AddTransaction(trn3);
				BillingDataTestHelper.AddTransaction(trn4);

				ExecuteProcessStaging(con);

				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				var chargeableTable = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);

				Assert.That(usageTable.Rows.Count, Is.EqualTo(2), "HVL records should be processed into Usage table");
				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(2), "Non HVL records should be processed into Chargeable table");
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn1, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, usageTable.Rows[0]);
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn3, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, usageTable.Rows[1]);

				var chargeableRow1 = chargeableTable.Rows[0];
				Assert.That(chargeableRow1["CH_BillableCount"], Is.EqualTo(1));
				Assert.That(chargeableRow1["CH_ClientID"], Is.EqualTo("ENT???SRV"));
				Assert.That(chargeableRow1["CH_Category"], Is.EqualTo("STL"));
				Assert.That(chargeableRow1["CH_PriceItemCode"], Is.EqualTo("AM2"));
				Assert.That(chargeableRow1["CH_Reference1"], Is.EqualTo("C00072779"));
				Assert.That(chargeableRow1["CH_Reference2"], Is.EqualTo("MOIPEV08132"));
				Assert.That(chargeableRow1["CH_Reference3"], Is.EqualTo("00205014"));
				Assert.That(chargeableRow1["CH_Reference4"], Is.EqualTo("PGAA"));
				Assert.That(chargeableRow1["CH_ReportingSource"], Is.EqualTo("ENT"));
				Assert.That(chargeableRow1["CH_ServiceOccuredUTC"], Is.EqualTo(trn2.ServiceOccuredUTC));
				Assert.That(chargeableRow1["CH_Version"], Is.EqualTo(0));
				Assert.That(chargeableRow1["CH_DatabaseNumber"], Is.EqualTo(1));
				Assert.That(chargeableRow1["CH_CompanyNumber"], Is.EqualTo(1));
				Assert.That(chargeableRow1["CH_Period"], Is.EqualTo((trn2.ServiceOccuredUTC.Year * 100) + trn2.ServiceOccuredUTC.Month));

				var chargeableRow2 = chargeableTable.Rows[1];
				Assert.That(chargeableRow2["CH_BillableCount"], Is.EqualTo(1));
				Assert.That(chargeableRow2["CH_ClientID"], Is.EqualTo("ENT???SRV"));
				Assert.That(chargeableRow2["CH_Category"], Is.EqualTo("STL"));
				Assert.That(chargeableRow2["CH_PriceItemCode"], Is.EqualTo("IS2"));
				Assert.That(chargeableRow2["CH_Reference1"], Is.EqualTo("CM00149010"));
				Assert.That(chargeableRow2["CH_Reference2"], Is.EqualTo("TT118170"));
				Assert.That(chargeableRow2["CH_Reference3"], Is.EqualTo("SM00192443"));
				Assert.That(chargeableRow2["CH_Reference4"], Is.EqualTo("MZLM"));
				Assert.That(chargeableRow2["CH_ReportingSource"], Is.EqualTo("ENT"));
				Assert.That(chargeableRow2["CH_ServiceOccuredUTC"], Is.EqualTo(trn4.ServiceOccuredUTC));
				Assert.That(chargeableRow2["CH_Version"], Is.EqualTo(0));
				Assert.That(chargeableRow2["CH_DatabaseNumber"], Is.EqualTo(1));
				Assert.That(chargeableRow2["CH_CompanyNumber"], Is.EqualTo(1));
				Assert.That(chargeableRow2["CH_Period"], Is.EqualTo((trn4.ServiceOccuredUTC.Year * 100) + trn4.ServiceOccuredUTC.Month));
			}
		}

		[Test]
		public void TestProcessAccountsPayableReceivable()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "SYD");

				var utcNow = DateTime.UtcNow;
				var currentYear = GetCurrentYear();
				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENT???SRV",
					ClientNumber = null,
					Category = "STL",
					PriceItemCode = "TX1",
					Reference1 = "1000672075",
					Reference2 = "S22AATL0363233",
					Reference3 = "US",
					Reference4 = "INV",
					Reference5 = "9557C17A-0AE5-4C66-8D4C-52B28462B92D",
					ReportingSource = "ENT",
					ServiceOccuredUTC = new DateTime(currentYear - 3, 6, 20),
					Version = 0,
				};

				var trn2 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENT???SRV",
					ClientNumber = null,
					Category = "STL",
					PriceItemCode = "TX2",
					Reference1 = "43518651",
					Reference2 = "8600284806",
					Reference3 = "CN",
					Reference4 = "INV",
					Reference5 = "2DA42803-3D2B-4B81-8A0B-0D626E339FF7",
					ReportingSource = "ENT",
					ServiceOccuredUTC = new DateTime(currentYear - 3, 6, 21),
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trn1);
				BillingDataTestHelper.AddTransaction(trn2);

				ExecuteProcessStaging(con);

				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				var chargeableTable = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);

				Assert.That(usageTable.Rows.Count, Is.EqualTo(2), "Accounting records should be processed into Usage table");
				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(0), "No records should be processed into Chargeable table");
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn1, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, usageTable.Rows[0]);
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn2, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, usageTable.Rows[1]);
			}
		}


		[Test]
		public void TestPeriod()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "DEF");

				var currentYear = GetCurrentYear();

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ABCDEFXYZ",
					ClientNumber = "C.DEF",
					ClientStaffCode = "ABC",
					Category = "Z_Z",
					PriceItemCode = "Z_Z",
					Reference1 = "REF 1",
					Reference2 = null,
					Reference3 = "REF 3",
					ReportingSource = "XYZ",
					ServiceOccuredUTC = new DateTime(currentYear - 3, 10, 31, 12, 59, 59, 999),
					Version = 1,
					MessageTrackingID = "Message2"
				};

				BillingDataTestHelper.AddTransaction(trn1);

				ExecuteProcessStaging(con);

				var table = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);

				Assert.That(table.Rows.Count, Is.EqualTo(1), "record count");

				var expectedPeriod = (currentYear - 3) * 100 + 10;
				Assert.AreEqual(expectedPeriod, (int)table.Rows[0]["CH_Period"]);
			}
		}

		[Test]
		public void TestProcessDOS()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "SYD");

				var utcNow = DateTime.UtcNow;
				var currentYear = GetCurrentYear();

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENT???SRV",
					ClientNumber = null,
					Category = "DOS",
					PriceItemCode = "IN1",
					Reference1 = "Invoice~EN-US",
					Reference2 = "520a1dc1-bd79-408c-9abc-41cb239b4bbd",
					Reference3 = "AccTransactionHeader",
					Reference4 = "Fail",
					ReportingSource = "ENT",
					ServiceOccuredUTC = new DateTime(currentYear - 3, 6, 20),
					Version = 1,
				};

				var trn2 = new API.BillingTransaction
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
					ServiceOccuredUTC = new DateTime(currentYear - 3, 6, 20),
					Version = 1,
				};

				BillingDataTestHelper.AddTransaction(trn1);
				BillingDataTestHelper.AddTransaction(trn2);

				ExecuteProcessStaging(con);

				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				var chargeableTable = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);

				Assert.That(usageTable.Rows.Count, Is.EqualTo(1), "Failed records should be processed into usage table");
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn1, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, usageTable.Rows[0]);
				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(1), "Sucessful records should be processed into Chargeable table");

				var chargeableRow = chargeableTable.Rows[0];
				Assert.That(chargeableRow["CH_BillableCount"], Is.EqualTo(trn2.BillableCount));
				Assert.That(chargeableRow["CH_ClientID"], Is.EqualTo(trn2.ClientID));
				Assert.That(chargeableRow["CH_Category"], Is.EqualTo(trn2.Category));
				Assert.That(chargeableRow["CH_PriceItemCode"], Is.EqualTo(trn2.PriceItemCode));
				Assert.That(chargeableRow["CH_Reference1"], Is.EqualTo(trn2.Reference1));
				Assert.That(chargeableRow["CH_Reference2"], Is.EqualTo(trn2.Reference2));
				Assert.That(chargeableRow["CH_Reference3"], Is.EqualTo(trn2.Reference3));
				Assert.That(chargeableRow["CH_Reference4"], Is.EqualTo(trn2.Reference4));
				Assert.That(chargeableRow["CH_ReportingSource"], Is.EqualTo(trn2.ReportingSource));
				Assert.That(chargeableRow["CH_ServiceOccuredUTC"], Is.EqualTo(trn2.ServiceOccuredUTC));
				Assert.That(chargeableRow["CH_Version"], Is.EqualTo(trn2.Version));
				Assert.That(chargeableRow["CH_DatabaseNumber"], Is.EqualTo(1));
				Assert.That(chargeableRow["CH_CompanyNumber"], Is.EqualTo(1));
				Assert.That(chargeableRow["CH_Period"], Is.EqualTo((trn2.ServiceOccuredUTC.Year * 100) + trn2.ServiceOccuredUTC.Month));
			}
		}

		[Test]
		public void TestInternalSystemUsageDeletedOnProductionServer()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "SYD", enterpriseCode: "EDI");
				BillingDataTestHelper.AddDatabaseAndCompany(con, 2, 1, "SYD", enterpriseCode: "HYE");
				BillingDataTestHelper.AddDatabaseAndCompany(con, 3, 1, "SYD", enterpriseCode: "WTL");
				BillingDataTestHelper.AddDatabaseAndCompany(con, 4, 1, "SYD", enterpriseCode: "EHW");

				var utcNow = DateTime.UtcNow;
				var currentYear = GetCurrentYear();
				var transactionTime = new DateTime(currentYear - 3, 6, 20);
				var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(transactionTime);

				var trn1 = new API.BillingTransaction
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

				var trn2 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "HYE???SRV",
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

				var trn3 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "WTL???SRV",
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

				var trn4 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "EHW???SRV",
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
				BillingDataTestHelper.AddTransaction(trn4);

				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, daysAfter: 1, performMonthlyAggregation: false, isTestServer: false);
				var chargeableTable = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(0), "No records in chargeable table as transactions are from internal systems");
			}
		}

		[Test]
		public void TestInternalSystemUsageProcessedOnTestServer()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "SYD", enterpriseCode: "EDI");
				BillingDataTestHelper.AddDatabaseAndCompany(con, 2, 1, "SYD", enterpriseCode: "HYE");
				BillingDataTestHelper.AddDatabaseAndCompany(con, 3, 1, "SYD", enterpriseCode: "WTL");
				BillingDataTestHelper.AddDatabaseAndCompany(con, 4, 1, "SYD", enterpriseCode: "EHW");

				var utcNow = DateTime.UtcNow;
				var currentYear = GetCurrentYear();
				var transactionTime = new DateTime(currentYear - 3, 6, 20);
				var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(transactionTime);

				var trn1 = new API.BillingTransaction
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

				var trn2 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "HYE???SRV",
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

				var trn3 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "WTL???SRV",
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

				var trn4 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "EHW???SRV",
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
				BillingDataTestHelper.AddTransaction(trn4);

				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, daysAfter: 1, performMonthlyAggregation: false, isTestServer: true);
				var chargeableTable = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(4), "All records in chargeable table for internal systems on test server");

				var chargeableRow1 = chargeableTable.Rows[0];
				BillingDataTestHelper.AssertPropertiesAreEqual(trn1, chargeableRow1, "CH_");
				Assert.That(chargeableRow1["CH_DatabaseNumber"], Is.EqualTo(1));
				Assert.That(chargeableRow1["CH_CompanyNumber"], Is.EqualTo(1));

				var chargeableRow2 = chargeableTable.Rows[1];
				BillingDataTestHelper.AssertPropertiesAreEqual(trn2, chargeableRow2, "CH_");
				Assert.That(chargeableRow2["CH_DatabaseNumber"], Is.EqualTo(2));
				Assert.That(chargeableRow2["CH_CompanyNumber"], Is.EqualTo(1));

				var chargeableRow3 = chargeableTable.Rows[2];
				BillingDataTestHelper.AssertPropertiesAreEqual(trn3, chargeableRow3, "CH_");
				Assert.That(chargeableRow3["CH_DatabaseNumber"], Is.EqualTo(3));
				Assert.That(chargeableRow3["CH_CompanyNumber"], Is.EqualTo(1));

				var chargeableRow4 = chargeableTable.Rows[3];
				BillingDataTestHelper.AssertPropertiesAreEqual(trn4, chargeableRow4, "CH_");
				Assert.That(chargeableRow4["CH_DatabaseNumber"], Is.EqualTo(4));
				Assert.That(chargeableRow4["CH_CompanyNumber"], Is.EqualTo(1));
			}
		}

		[Test]
		public void TestCW1SystemsBehindOnSTLMilestones()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "SYD", enterpriseCode: "PAS");
				BillingDataTestHelper.AddDatabaseAndCompany(con, 2, 1, "SYD", enterpriseCode: "MIS");
				BillingDataTestHelper.AddOldDatabaseLocation(con, 2, "CHI", enterpriseCode: "MIS");
				BillingDataTestHelper.AddDatabaseAndCompany(con, 3, 1, "SYD", enterpriseCode: "NEW");
				BillingDataTestHelper.AddDatabaseAndCompany(con, 4, 1, "SYD", enterpriseCode: "OLD");
				BillingDataTestHelper.AddDatabaseAndCompany(con, 5, 1, "SYD", enterpriseCode: "INA", isActive: false);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 6, 1, "SYD", enterpriseCode: "TER", isTeardownInProgress: true);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 7, 1, "SYD", enterpriseCode: "TES", licenceType: "TST");

				var dateToCheck = DateTime.Now.AddHours(-4).AddDays(-1);
				var milestonesExpected = dateToCheck.Day;
				var previousPeriod = dateToCheck.AddMonths(-1);

				AddMilestone(previousPeriod.Year, previousPeriod.Month, day: DateTime.DaysInMonth(previousPeriod.Year, previousPeriod.Month), clientId: "PASSYDSRV");
				AddMilestone(previousPeriod.Year, previousPeriod.Month, day: DateTime.DaysInMonth(previousPeriod.Year, previousPeriod.Month), clientId: "MISSYDSRV");
				AddMilestone(previousPeriod.Year, previousPeriod.Month, day: DateTime.DaysInMonth(previousPeriod.Year, previousPeriod.Month), clientId: "OLDSYDSRV");
				AddMilestone(previousPeriod.Year, previousPeriod.Month, day: DateTime.DaysInMonth(previousPeriod.Year, previousPeriod.Month), clientId: "INASYDSRV");
				AddMilestone(previousPeriod.Year, previousPeriod.Month, day: DateTime.DaysInMonth(previousPeriod.Year, previousPeriod.Month), clientId: "TERSYDSRV");
				AddMilestone(previousPeriod.Year, previousPeriod.Month, day: DateTime.DaysInMonth(previousPeriod.Year, previousPeriod.Month), clientId: "TESSYDSRV");

				for (int i = 1; i <= milestonesExpected; i++)
				{
					AddMilestone(dateToCheck.Year, dateToCheck.Month, day: i, clientId: "PASSYDSRV");
					if (i < milestonesExpected)
					{
						AddMilestone(dateToCheck.Year, dateToCheck.Month, day: i, clientId: "MISSYDSRV");
						AddMilestone(dateToCheck.Year, dateToCheck.Month, day: i, clientId: "MISSYDSRV", additionalProperty: "Some extra data that duplicate has");
						AddMilestone(dateToCheck.Year, dateToCheck.Month, day: i, clientId: "INASYDSRV");
						AddMilestone(dateToCheck.Year, dateToCheck.Month, day: i, clientId: "TERSYDSRV");
						AddMilestone(dateToCheck.Year, dateToCheck.Month, day: i, clientId: "TESSYDSRV");
					}
				}

				AddMilestone(dateToCheck.Year, dateToCheck.Month, day: DateTime.DaysInMonth(dateToCheck.Year, dateToCheck.Month), clientId: "NEWSYDSRV");

				ExecuteProcessStaging(con);

				var systemsBehindTable = BillingDataTestHelper.LoadTable(con, "declare @t table(MB_Milestones int, MB_EnterpriseCode varchar(3), MB_ServerCode varchar(3), MB_HostedLocation varchar(3)) insert @t exec edi.CW1SystemsBehindOnSTLMilestones select * from @t");			
				Assert.That(systemsBehindTable.Rows.Count, Is.EqualTo(3), "Should be three systems found to be missing milestones");
				var misBehindRow = systemsBehindTable.Rows.Cast<DataRow>().Single(x => (String)x["MB_EnterpriseCode"] == "MIS");
				Assert.That(misBehindRow["MB_Milestones"], Is.EqualTo(milestonesExpected - 1));
				Assert.That(misBehindRow["MB_ServerCode"], Is.EqualTo("SRV"));
				Assert.That(misBehindRow["MB_HostedLocation"], Is.EqualTo("SYD"));
				var oldBehindRow = systemsBehindTable.Rows.Cast<DataRow>().Single(x => (String)x["MB_EnterpriseCode"] == "OLD");
				Assert.That(oldBehindRow["MB_Milestones"], Is.EqualTo(0));
				Assert.That(oldBehindRow["MB_ServerCode"], Is.EqualTo("SRV"));
				Assert.That(oldBehindRow["MB_HostedLocation"], Is.EqualTo("SYD"));
				var tstBehindRow = systemsBehindTable.Rows.Cast<DataRow>().Single(x => (String)x["MB_EnterpriseCode"] == "TES");
				Assert.That(tstBehindRow["MB_Milestones"], Is.EqualTo(milestonesExpected - 1));
				Assert.That(tstBehindRow["MB_ServerCode"], Is.EqualTo("SRV"));
				Assert.That(tstBehindRow["MB_HostedLocation"], Is.EqualTo("SYD"));
			}
		}

		[Test]
		public void TestCw1SystemsBillingUnknownSystem()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1234, 2, "SYD",enterpriseCode: "AAA", serverCode: "BBB", isActive: true);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 2234, 2, "SYD",enterpriseCode: "AAA", serverCode: "CCC", isActive: false);
				var currentYear = GetCurrentYear();
				var trnMatched = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "AAASYDBBB",
					ClientNumber = null,
					ClientStaffCode = "ABC",
					Category = "Z_Z",
					PriceItemCode = "Z_Z",
					Reference1 = "REF 1",
					Reference2 = null,
					Reference3 = "REF 3",
					ReportingSource = "XYZ",
					ServiceOccuredUTC = new DateTime(currentYear - 1, 8, 07, 0, 0, 0),
					Version = 1,
					MessageTrackingID = "Message1"
				};
				var trnUnknownCompanyActive = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "AAAANYBBB",
					ClientNumber = null,
					ClientStaffCode = "ABC",
					Category = "Z_Z",
					PriceItemCode = "Z_Z",
					Reference1 = "REF 1",
					Reference2 = null,
					Reference3 = "REF 3",
					ReportingSource = "XYZ",
					ServiceOccuredUTC = new DateTime(currentYear - 1, 8, 07, 0, 0, 0),
					Version = 1,
					MessageTrackingID = "Message2"
				};
				var trnUnknownCompanyInActive = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "AAAANYCCC",
					ClientNumber = null,
					ClientStaffCode = "ABC",
					Category = "Z_Z",
					PriceItemCode = "Z_Z",
					Reference1 = "REF 1",
					Reference2 = null,
					Reference3 = "REF 3",
					ReportingSource = "XYZ",
					ServiceOccuredUTC = new DateTime(currentYear - 1, 8, 07, 0, 0, 0),
					Version = 1,
					MessageTrackingID = "Message3"
				};
				var trnUnmatched = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "CCCSDYDDD",
					ClientNumber = null,
					ClientStaffCode = "ABC",
					Category = "Z_Z",
					PriceItemCode = "Z_Z",
					Reference1 = "REF 1",
					Reference2 = null,
					Reference3 = "REF 3",
					ReportingSource = "XYZ",
					ServiceOccuredUTC = new DateTime(currentYear - 1, 8, 07, 0, 0, 0),
					Version = 1,
					MessageTrackingID = "Message4"
				};
				BillingDataTestHelper.AddTransaction(trnMatched);
				BillingDataTestHelper.AddTransaction(trnUnmatched);
				BillingDataTestHelper.AddTransaction(trnUnknownCompanyActive);
				BillingDataTestHelper.AddTransaction(trnUnknownCompanyInActive);

				ExecuteProcessStaging(con);

				var stagingUnknownTable = BillingDataTestHelper.LoadTableByName(con, "edi.StagingUnknownSystems");
				Assert.That(stagingUnknownTable.Rows.Count, Is.EqualTo(3), "unmatched message");
				var row1 = stagingUnknownTable.Rows[0];
				var row2 = stagingUnknownTable.Rows[1];
				var row3 = stagingUnknownTable.Rows[2];
				Assert.That(row1["DatabaseNumber"], Is.EqualTo(0));
				Assert.That(row1["CompanyNumber"], Is.EqualTo(0));
				Assert.That(row2["DatabaseNumber"], Is.EqualTo(1234));
				Assert.That(row2["CompanyNumber"], Is.EqualTo(0));
				Assert.That(row3["DatabaseNumber"], Is.EqualTo(2234));
				Assert.That(row3["CompanyNumber"], Is.EqualTo(0));

				var billingUnknownSystem = BillingDataTestHelper.LoadTable(con, "declare @t table(UNK_Category varchar(3), UNK_PriceItemCode varchar(3), UNK_BillableCount int, UNK_ServiceOccuredUTC datetime2(7), UNK_SystemCreateUTC datetime2(0), UNK_ClientID varchar(9), UNK_ClientNumber varchar(50), UNK_DatabaseNumber int, UNK_CompanyNumber smallint) insert @t exec edi.CW1SystemsBillingUnknownSystem @DaysThreshold = -1; select * from @t");
				Assert.That(billingUnknownSystem.Rows.Count, Is.EqualTo(2), "Should not report the client with inactive databaseCode. ");
				var reportedRecord1 = billingUnknownSystem.Rows[0];
				var reportedRecord2 = billingUnknownSystem.Rows[1];
				Assert.That(reportedRecord1["UNK_DatabaseNumber"], Is.EqualTo(0));
				Assert.That(reportedRecord1["UNK_CompanyNumber"], Is.EqualTo(0));
				Assert.NotNull(reportedRecord1["UNK_SystemCreateUTC"]);
				Assert.That(reportedRecord2["UNK_DatabaseNumber"], Is.EqualTo(1234));
				Assert.That(reportedRecord2["UNK_CompanyNumber"], Is.EqualTo(0));
				Assert.NotNull(reportedRecord2["UNK_SystemCreateUTC"]);
			}
		}
		[Test]
		public void TestCW1SystemsNotReportingCPU()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "SYD", enterpriseCode: "PAS");
				BillingDataTestHelper.AddDatabaseAndCompany(con, 2, 1, "SYD", enterpriseCode: "MIS");
				BillingDataTestHelper.AddOldDatabaseLocation(con, 2, "CHI", enterpriseCode: "MIS");
				BillingDataTestHelper.AddDatabaseAndCompany(con, 3, 1, "SYD", enterpriseCode: "NEW");
				BillingDataTestHelper.AddDatabaseAndCompany(con, 4, 1, "SYD", enterpriseCode: "OLD");
				BillingDataTestHelper.AddDatabaseAndCompany(con, 5, 1, "SYD", enterpriseCode: "INA", isActive: false);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 6, 1, "SYD", enterpriseCode: "TER", isTeardownInProgress: true);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 7, 1, "SYD", enterpriseCode: "TES", licenceType: "TST");
				BillingDataTestHelper.AddDatabaseAndCompany(con, 8, 1, "SLF", enterpriseCode: "SLF", hostedLocation: "NCW");
				BillingDataTestHelper.AddDatabaseAndCompany(con, 9, 1, "SYD", enterpriseCode: "NCR");
				BillingDataTestHelper.AddDatabaseAndCompany(con, 10, 1, "SYD", enterpriseCode: "OCP");

				var dateToCheck = DateTime.Now.AddHours(-4).AddDays(-1);
				var milestonesExpected = dateToCheck.Day;
				var previousPeriod = dateToCheck.AddMonths(-1);

				AddMilestone(previousPeriod.Year, previousPeriod.Month, day: DateTime.DaysInMonth(previousPeriod.Year, previousPeriod.Month), clientId: "PASSYDSRV");
				AddMilestone(previousPeriod.Year, previousPeriod.Month, day: DateTime.DaysInMonth(previousPeriod.Year, previousPeriod.Month), clientId: "MISSYDSRV");
				AddMilestone(previousPeriod.Year, previousPeriod.Month, day: DateTime.DaysInMonth(previousPeriod.Year, previousPeriod.Month), clientId: "OLDSYDSRV");
				AddMilestone(previousPeriod.Year, previousPeriod.Month, day: DateTime.DaysInMonth(previousPeriod.Year, previousPeriod.Month), clientId: "INASYDSRV");
				AddMilestone(previousPeriod.Year, previousPeriod.Month, day: DateTime.DaysInMonth(previousPeriod.Year, previousPeriod.Month), clientId: "TERSYDSRV");
				AddMilestone(previousPeriod.Year, previousPeriod.Month, day: DateTime.DaysInMonth(previousPeriod.Year, previousPeriod.Month), clientId: "TESSYDSRV");
				AddMilestone(previousPeriod.Year, previousPeriod.Month, day: DateTime.DaysInMonth(previousPeriod.Year, previousPeriod.Month), clientId: "SLFSLFSRV");
				AddMilestone(previousPeriod.Year, previousPeriod.Month, day: DateTime.DaysInMonth(previousPeriod.Year, previousPeriod.Month), clientId: "NCRSYDSRV");
				AddMilestone(previousPeriod.Year, previousPeriod.Month, day: DateTime.DaysInMonth(previousPeriod.Year, previousPeriod.Month), clientId: "OCPSYDSRV");

				for (int i = 1; i <= milestonesExpected; i++)
				{
					AddMilestone(dateToCheck.Year, dateToCheck.Month, day: i, clientId: "PASSYDSRV");
					AddMilestone(dateToCheck.Year, dateToCheck.Month, day: i, clientId: "SLFSLFSRV");
					AddMilestone(dateToCheck.Year, dateToCheck.Month, day: i, clientId: "INASYDSRV");
					AddMilestone(dateToCheck.Year, dateToCheck.Month, day: i, clientId: "TERSYDSRV");
					AddMilestone(dateToCheck.Year, dateToCheck.Month, day: i, clientId: "NCRSYDSRV");
					AddMilestone(dateToCheck.Year, dateToCheck.Month, day: i, clientId: "OCPSYDSRV");
					if (i < milestonesExpected)
					{
						AddMilestone(dateToCheck.Year, dateToCheck.Month, day: i, clientId: "MISSYDSRV");
						AddMilestone(dateToCheck.Year, dateToCheck.Month, day: i, clientId: "MISSYDSRV", additionalProperty: "Some extra data that duplicate has");
						AddMilestone(dateToCheck.Year, dateToCheck.Month, day: i, clientId: "TESSYDSRV");
					}
				}

				AddMilestone(dateToCheck.Year, dateToCheck.Month, day: DateTime.DaysInMonth(dateToCheck.Year, dateToCheck.Month), clientId: "NEWSYDSRV");

				var utcNow = DateTime.UtcNow;

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 123,
					ClientID = "PAS???SRV",
					ClientNumber = null,
					Category = "WGR",
					PriceItemCode = "WGR",
					Reference1 = "900",
					Reference2 = "ClientDirect",
					Reference3 = "Client",
					Reference4 = "LONWP-SSQL-4A.wisecloud.zone/INSTANCEP1",
					Reference5 = "8788",
					ReportingSource = "MSC",
					ServiceOccuredUTC = utcNow,
					Version = 0,
				};
				var trn2 = new API.BillingTransaction
				{
					BillableCount = 123,
					ClientID = "OCP???SRV",
					ClientNumber = null,
					Category = "WGR",
					PriceItemCode = "WGR",
					Reference1 = "900",
					Reference2 = "ClientDirect",
					Reference3 = "Client",
					Reference4 = "LONWP-SSQL-4A.wisecloud.zone/INSTANCEP1",
					Reference5 = "8788",
					ReportingSource = "MSC",
					ServiceOccuredUTC = utcNow.AddDays(-3),
					Version = 0,
				};
				BillingDataTestHelper.AddTransaction(trn1);
				BillingDataTestHelper.AddTransaction(trn2);

				ExecuteProcessStaging(con);

				var systemsBehindTable = BillingDataTestHelper.LoadTable(con, "declare @t table(MC_EnterpriseCode varchar(3), MC_ServerCode varchar(3), MC_HostedLocation varchar(3)) insert @t exec edi.CW1SystemsNotReportingCPU select * from @t");
				Assert.That(systemsBehindTable.Rows.Count, Is.EqualTo(2), "Should be two systems found to be missing CPU");
				var noCPURow = systemsBehindTable.Rows.Cast<DataRow>().Single(x => (String)x["MC_EnterpriseCode"] == "NCR");
				Assert.That(noCPURow["MC_ServerCode"], Is.EqualTo("SRV"));
				Assert.That(noCPURow["MC_HostedLocation"], Is.EqualTo("SYD"));
				var oldCPURow = systemsBehindTable.Rows.Cast<DataRow>().Single(x => (String)x["MC_EnterpriseCode"] == "OCP");
				Assert.That(oldCPURow["MC_ServerCode"], Is.EqualTo("SRV"));
				Assert.That(oldCPURow["MC_HostedLocation"], Is.EqualTo("SYD"));
			}
		}

		[Test]
		public void TestProcessTestTransactionsAsUsage()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);

				var currentYear = GetCurrentYear();
				var transactionTime = new DateTime(currentYear - 1, 12, 11);
				var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(transactionTime);

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "?????????",
					ClientNumber = "A",
					ClientStaffCode = "",
					Category = "TST",
					PriceItemCode = "TST",
					Reference1 = "Ref 1",
					Reference2 = "Ref 2",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transactionTime,
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trn1);

				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, daysAfter: 1, performMonthlyAggregation: false, isTestServer: false);

				var table = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				var stagingUnknownTable = BillingDataTestHelper.LoadTableByName(con, "edi.StagingUnknownSystems");

				Assert.That(table.Rows.Count, Is.EqualTo(0), "Chargeable rows");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(1), "Usage rows");
				Assert.That(stagingUnknownTable.Rows.Count, Is.EqualTo(0), "StagingUnknownSystems rows");

				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn1, expectedDatabaseNumber: 0, expectedCompanyNumber: 0, usageTable.Rows[0]);
			}
		}

		[TestCase("PROES_EAD")]
		[TestCase("ENVAS_E1D")]
		[TestCase("TSTTSTTST")]
		[TestCase("UATUATUAT")]
		[TestCase("PLKES_EAD")]
		[TestCase("B92ME_EAD")]
		[TestCase("?????????")]
		public void TestProcessInternalAndTestClientsAsUsage(string clientId)
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);

				var currentYear = GetCurrentYear();
				var transactionTime = new DateTime(currentYear - 1, 12, 11);
				var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(transactionTime);

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = clientId,
					ClientNumber = "",
					ClientStaffCode = "",
					Category = "ANY",
					PriceItemCode = "ANY",
					Reference1 = "Ref 1",
					Reference2 = "Ref 2",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transactionTime,
					Version = 0,
				};

				var trn2 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = clientId,
					ClientNumber = "A",
					ClientStaffCode = "",
					Category = "ANY",
					PriceItemCode = "ANY",
					Reference1 = "Ref 1",
					Reference2 = "Ref 2",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transactionTime,
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trn1);
				BillingDataTestHelper.AddTransaction(trn2);

				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, daysAfter: 1, performMonthlyAggregation: false, isTestServer: false);

				var table = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				var stagingUnknownTable = BillingDataTestHelper.LoadTableByName(con, "edi.StagingUnknownSystems");

				Assert.That(table.Rows.Count, Is.EqualTo(0), "Chargeable rows");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(1), "Usage rows");
				Assert.That(stagingUnknownTable.Rows.Count, Is.EqualTo(1), "StagingUnknownSystems rows");
			}
		}

		[Test]
		public void TestProcessBordwiseTransactionsWithNoClientAsUsage()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);

				var currentYear = GetCurrentYear();
				var transactionTime = new DateTime(currentYear - 1, 12, 11);
				var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(transactionTime);

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "?????????",
					ClientNumber = "",
					ClientStaffCode = "",
					Category = "BOR",
					PriceItemCode = "BOR",
					Reference1 = "Ref 1",
					Reference2 = "Ref 2",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "BOR",
					ServiceOccuredUTC = transactionTime,
					Version = 0,
				};

				var trn2 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "?????????",
					ClientNumber = null,
					ClientStaffCode = "",
					Category = "BOR",
					PriceItemCode = "MIL",
					Reference1 = "Ref 1",
					Reference2 = "Ref 2",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "BOR",
					ServiceOccuredUTC = transactionTime,
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trn1);
				BillingDataTestHelper.AddTransaction(trn2);

				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, daysAfter: 1, performMonthlyAggregation: false, isTestServer: false);

				var table = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				var stagingUnknownTable = BillingDataTestHelper.LoadTableByName(con, "edi.StagingUnknownSystems");

				Assert.That(table.Rows.Count, Is.EqualTo(0), "Chargeable rows");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(2), "Usage rows");
				Assert.That(stagingUnknownTable.Rows.Count, Is.EqualTo(0), "StagingUnknownSystems rows");

				var row1 = usageTable.Rows.Cast<DataRow>().Single(x => (String)x["US_PriceItemCode"] == "BOR");
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn1, expectedDatabaseNumber: 0, expectedCompanyNumber: 0, row1);
				var row2 = usageTable.Rows.Cast<DataRow>().Single(x => (String)x["US_PriceItemCode"] == "MIL");
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn2, expectedDatabaseNumber: 0, expectedCompanyNumber: 0, row2);
			}
		}

		[Test]
		public void TestProcessInternalTestSystemsAsUsage()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);

				var currentYear = GetCurrentYear();
				var transactionTime = new DateTime(currentYear - 1, 12, 11);
				var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(transactionTime);

				var trn1 = new API.BillingTransaction
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

				BillingDataTestHelper.AddTransaction(trn1);

				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, daysAfter: 1, performMonthlyAggregation: false, isTestServer: false);

				var table = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				var stagingUnknownTable = BillingDataTestHelper.LoadTableByName(con, "edi.StagingUnknownSystems");

				Assert.That(table.Rows.Count, Is.EqualTo(0), "Chargeable rows");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(1), "Usage rows");
				Assert.That(stagingUnknownTable.Rows.Count, Is.EqualTo(0), "StagingUnknownSystems rows");

				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn1, expectedDatabaseNumber: 0, expectedCompanyNumber: 0, usageTable.Rows[0]);
			}
		}

		[TestCase("WPC")]
		[TestCase("INZ")]
		[TestCase("CEW")]
		public void TestProcessProductionTestSystemsAsUsage(string enterpriseCode)
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);

				var companyCode = "SYD";
				var serverCode = "SYD";
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, companyCode, enterpriseCode: enterpriseCode, serverCode: serverCode);

				var currentYear = GetCurrentYear();
				var transactionTime = new DateTime(currentYear - 1, 12, 11);
				var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(transactionTime);

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = enterpriseCode + companyCode + serverCode,
					ClientNumber = "",
					ClientStaffCode = "",
					Category = "ANY",
					PriceItemCode = "ANY",
					Reference1 = "Ref 1",
					Reference2 = "Ref 2",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transactionTime,
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trn1);

				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, daysAfter: 1, performMonthlyAggregation: false, isTestServer: false);

				var table = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				var stagingUnknownTable = BillingDataTestHelper.LoadTableByName(con, "edi.StagingUnknownSystems");

				Assert.That(table.Rows.Count, Is.EqualTo(0), "Chargeable rows");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(1), "Usage rows");
				Assert.That(stagingUnknownTable.Rows.Count, Is.EqualTo(0), "StagingUnknownSystems rows");

				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn1, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, usageTable.Rows[0]);
			}
		}

		[Test]
		public void TestProcessWGRInternalSystemsAsUsage()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);

				var currentYear = GetCurrentYear();
				var transactionTime = new DateTime(currentYear - 1, 12, 11);
				var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(transactionTime);

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "XXX???YYY",
					ClientNumber = "A",
					ClientStaffCode = "",
					Category = "WGR",
					PriceItemCode = "WGR",
					Reference1 = "Ref 1",
					Reference2 = "Ref 2",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transactionTime,
					Version = 0,
				};

				var trn2 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ead???ers",
					ClientNumber = "A",
					ClientStaffCode = "",
					Category = "WGR",
					PriceItemCode = "WGR",
					Reference1 = "Ref 1",
					Reference2 = "Ref 2",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transactionTime,
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trn1);
				BillingDataTestHelper.AddTransaction(trn2);

				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, daysAfter: 1, performMonthlyAggregation: false, isTestServer: false);

				var table = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				var stagingUnknownTable = BillingDataTestHelper.LoadTableByName(con, "edi.StagingUnknownSystems");

				Assert.That(table.Rows.Count, Is.EqualTo(0), "Chargeable rows");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(2), "Usage rows");
				Assert.That(stagingUnknownTable.Rows.Count, Is.EqualTo(0), "StagingUnknownSystems rows");

				var row1 = usageTable.Rows.Cast<DataRow>().Single(x => (String)x["US_ClientID"] == "XXX???YYY");
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn1, expectedDatabaseNumber: 0, expectedCompanyNumber: 0, row1);
				var row2 = usageTable.Rows.Cast<DataRow>().Single(x => (String)x["US_ClientID"] == "ead???ers");
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn2, expectedDatabaseNumber: 0, expectedCompanyNumber: 0, row2);
			}
		}

		[Test]
		public void TestAddFakeLicenceCodeHistoryEntries()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				var currentYear = GetCurrentYear();
				var transactionTime = new DateTime(currentYear - 1, 12, 11);
				var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(transactionTime);

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "AAA???HHH",
					ClientNumber = null,
					ClientStaffCode = "R",
					Category = "ANY",
					PriceItemCode = "ANY",
					Reference1 = "Ref 1",
					Reference2 = "Ref 2",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transactionTime,
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trn1);

				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, daysAfter: 1, performMonthlyAggregation: false, isTestServer: true);

				var historyTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.LicenceDatabaseCodeHistory");
				Assert.That(historyTable.Rows.Count, Is.EqualTo(1), "Should add a fake entry to db");

				var row = historyTable.Rows[0];
				Assert.That(row["EnterpriseCode"], Is.EqualTo("AAA"));
				Assert.That(row["ServerCode"], Is.EqualTo("HHH"));
			}
		}

		[Test]
		public void TestAddFakeCompanyCodeHistoryEntries()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				var currentYear = GetCurrentYear();
				var transactionTime = new DateTime(currentYear - 1, 12, 11);
				var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(transactionTime);

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "YOITYABOI",
					ClientNumber = null,
					ClientStaffCode = "R",
					Category = "ANY",
					PriceItemCode = "ANY",
					Reference1 = "Ref 1",
					Reference2 = "Ref 2",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transactionTime,
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trn1);

				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, daysAfter: 1, performMonthlyAggregation: false, isTestServer: true);

				var historyTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.ClientCompanyCodeHistory");
				Assert.That(historyTable.Rows.Count, Is.EqualTo(1), "Should add a fake entry to db");

				var row = historyTable.Rows[0];
				Assert.That(row["CompanyCode"], Is.EqualTo("TYA"));

				var chargeableTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.Chargeable");
				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(1), "Should add a fake entry to db");

				var row2 = chargeableTable.Rows[0];
				BillingDataTestHelper.AssertChargeablePropertiesAreEqual(trn1, 1, 1, row2 );

				var companyTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.ClientCompany");
				Assert.That(companyTable.Rows.Count, Is.EqualTo(1), "Should add a fake entry to db");
			}
		}

		[Test]
		public void TestAddFakeCodeHistoryEntriesWithClientNumber()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				var currentYear = GetCurrentYear();
				var transactionTime = new DateTime(currentYear - 1, 12, 11);
				var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(transactionTime);

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "AAABBBHHH",
					ClientNumber = "CLIENT",
					ClientStaffCode = "R",
					Category = "ANY",
					PriceItemCode = "ANY",
					Reference1 = "Ref 1",
					Reference2 = "Ref 2",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transactionTime,
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trn1);

				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, daysAfter: 1, performMonthlyAggregation: false, isTestServer: true);

				var historyTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.LicenceDatabaseCodeHistory");
				Assert.That(historyTable.Rows.Count, Is.EqualTo(1), "Should add a fake entry to db");

				var row = historyTable.Rows[0];
				Assert.That(row["EnterpriseCode"], Is.EqualTo("AAA"));
				Assert.That(row["ServerCode"], Is.EqualTo("HHH"));

				var companyHistoryTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.ClientCompanyCodeHistory");
				Assert.That(companyHistoryTable.Rows.Count, Is.EqualTo(1), "Should add a fake entry to db");

				var row2 = companyHistoryTable.Rows[0];
				Assert.That(row2["CompanyCode"], Is.EqualTo("BBB"));

				var chargeableTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.Chargeable");
				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(1), "Should add a fake entry to db");

				var row3 = chargeableTable.Rows[0];
				BillingDataTestHelper.AssertChargeablePropertiesAreEqual(trn1, 1, 1, row3);

				var companyTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.ClientCompany");
				Assert.That(companyTable.Rows.Count, Is.EqualTo(1), "Should add a fake entry to db");
			}
		}

		[Test]
		public void TestAddBulkFakeCodeHistoryEntries()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				var currentYear = GetCurrentYear();
				var transactionTime = new DateTime(currentYear - 1, 12, 11);
				var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(transactionTime);

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "KJHNDFSSA",
					ClientNumber = null,
					ClientStaffCode = "R",
					Category = "ANY",
					PriceItemCode = "ANY",
					Reference1 = "Ref 1",
					Reference2 = "Ref 2",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transactionTime.AddMonths(-1),
					Version = 0,
				};

				var trn2 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "KJHNDFSSA",
					ClientNumber = null,
					ClientStaffCode = "R",
					Category = "ANY",
					PriceItemCode = "ANY",
					Reference1 = "Ref 1",
					Reference2 = "Ref 2",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transactionTime.AddMonths(-2),
					Version = 0,
				};

				var trn3 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "KJHNDFSSA",
					ClientNumber = null,
					ClientStaffCode = "R",
					Category = "ANY",
					PriceItemCode = "ANY",
					Reference1 = "Ref 1",
					Reference2 = "Ref 2",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transactionTime.AddDays(-2),
					Version = 0,
				};

				var trn4 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "GHYTNFGGB",
					ClientNumber = null,
					ClientStaffCode = "R",
					Category = "ANY",
					PriceItemCode = "ANY",
					Reference1 = "Ref 1",
					Reference2 = "Ref 2",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transactionTime,
					Version = 0,
				};

				var trn5 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "GJK???ITN",
					ClientNumber = null,
					ClientStaffCode = "R",
					Category = "ANY",
					PriceItemCode = "ANY",
					Reference1 = "Ref 1",
					Reference2 = "Ref 2",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transactionTime.AddDays(-1),
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trn1);
				BillingDataTestHelper.AddTransaction(trn2);
				BillingDataTestHelper.AddTransaction(trn3);
				BillingDataTestHelper.AddTransaction(trn4);
				BillingDataTestHelper.AddTransaction(trn5);

				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, daysAfter: 1, performMonthlyAggregation: false, isTestServer: true);

				var companyHistoryTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.ClientCompanyCodeHistory");
				Assert.That(companyHistoryTable.Rows.Count, Is.EqualTo(3), "Should add a fake entries to db");

				var licenceHistoryTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.LicenceDatabaseCodeHistory");
				Assert.That(licenceHistoryTable.Rows.Count, Is.EqualTo(3), "Should add a fake entries to db");

				var companyTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.ClientCompany");
				Assert.That(companyTable.Rows.Count, Is.EqualTo(3), "Should add fake entries to db");

				Assert.That(licenceHistoryTable.Rows[0]["EnterpriseCode"], Is.EqualTo("GHY"));
				Assert.That(licenceHistoryTable.Rows[0]["ServerCode"], Is.EqualTo("GGB"));
				Assert.That(licenceHistoryTable.Rows[1]["EnterpriseCode"], Is.EqualTo("GJK"));
				Assert.That(licenceHistoryTable.Rows[1]["ServerCode"], Is.EqualTo("ITN"));
				Assert.That(licenceHistoryTable.Rows[2]["EnterpriseCode"], Is.EqualTo("KJH"));
				Assert.That(licenceHistoryTable.Rows[2]["ServerCode"], Is.EqualTo("SSA"));

				Assert.That(companyHistoryTable.Rows[0]["CompanyCode"], Is.EqualTo("NDF"));
				Assert.That(companyHistoryTable.Rows[1]["CompanyCode"], Is.EqualTo("TNF"));
				Assert.That(companyHistoryTable.Rows[2]["CompanyCode"], Is.EqualTo("AAA"));

				var chargeableTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.Chargeable");
				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(5), "Should add all entries to chargeable");
			}
		}

		[Test]
		public void TestAddBulkFakeCodeHistoryEntriesForWGRTransactions()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				var currentYear = GetCurrentYear();
				var transactionTime = new DateTime(currentYear - 1, 12, 11);
				var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(transactionTime);

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "GJK???ITN",
					ClientNumber = null,
					ClientStaffCode = null,
					Category = "WGR",
					PriceItemCode = "WGR",
					Reference1 = "Ref 1-1",
					Reference2 = "Ref 1-2",
					Reference3 = "Ref 1-3",
					Reference4 = "Ref 1-4",
					Reference5 = "Ref 1-5",
					ReportingSource = "MSC",
					ServiceOccuredUTC = transactionTime.AddDays(-1),
					Version = 0,
				};

				var trn2 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "GJK???ITN",
					ClientNumber = null,
					ClientStaffCode = null,
					Category = "WGR",
					PriceItemCode = "WGR",
					Reference1 = "Ref 2-1",
					Reference2 = "Ref 2-2",
					Reference3 = "Ref 2-3",
					Reference4 = "Ref 2-4",
					Reference5 = "Ref 2-5",
					ReportingSource = "MSC",
					ServiceOccuredUTC = transactionTime.AddDays(-1),
					Version = 0,
				};

				var trn3 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "KJH???SSA",
					ClientNumber = null,
					ClientStaffCode = null,
					Category = "WGR",
					PriceItemCode = "WGR",
					Reference1 = "Ref 3-1",
					Reference2 = "Ref 3-2",
					Reference3 = "Ref 3-3",
					Reference4 = "Ref 3-4",
					Reference5 = "Ref 3-5",
					ReportingSource = "MSC",
					ServiceOccuredUTC = transactionTime.AddDays(-1),
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trn1);
				BillingDataTestHelper.AddTransaction(trn2);
				BillingDataTestHelper.AddTransaction(trn3);

				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, daysAfter: 1, performMonthlyAggregation: false, isTestServer: true);

				var companyHistoryTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.ClientCompanyCodeHistory");
				Assert.That(companyHistoryTable.Rows.Count, Is.EqualTo(2), "Should add a fake entries to db");

				var licenceHistoryTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.LicenceDatabaseCodeHistory");
				Assert.That(licenceHistoryTable.Rows.Count, Is.EqualTo(2), "Should add a fake entries to db");

				var companyTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.ClientCompany");
				Assert.That(companyTable.Rows.Count, Is.EqualTo(2), "Should add fake entries to db");

				Assert.That(licenceHistoryTable.Rows[0]["EnterpriseCode"], Is.EqualTo("GJK"));
				Assert.That(licenceHistoryTable.Rows[0]["ServerCode"], Is.EqualTo("ITN"));
				Assert.That(licenceHistoryTable.Rows[0]["DatabaseNumber"], Is.EqualTo(1));
				Assert.That(licenceHistoryTable.Rows[1]["EnterpriseCode"], Is.EqualTo("KJH"));
				Assert.That(licenceHistoryTable.Rows[1]["ServerCode"], Is.EqualTo("SSA"));
				Assert.That(licenceHistoryTable.Rows[1]["DatabaseNumber"], Is.EqualTo(2));

				Assert.That(companyHistoryTable.Rows[0]["CompanyCode"], Is.EqualTo("AAA"));
				Assert.That(companyHistoryTable.Rows[0]["CompanyNumber"], Is.EqualTo(1));
				Assert.That(companyHistoryTable.Rows[0]["DatabaseNumber"], Is.EqualTo(1));
				Assert.That(companyHistoryTable.Rows[1]["CompanyCode"], Is.EqualTo("AAA"));
				Assert.That(companyHistoryTable.Rows[1]["CompanyNumber"], Is.EqualTo(1));
				Assert.That(companyHistoryTable.Rows[1]["DatabaseNumber"], Is.EqualTo(2));

				Assert.That(companyTable.Rows[0]["CompanyNumber"], Is.EqualTo(1));
				Assert.That(companyTable.Rows[0]["DatabaseNumber"], Is.EqualTo(1));
				Assert.That(companyTable.Rows[1]["CompanyNumber"], Is.EqualTo(1));
				Assert.That(companyTable.Rows[1]["DatabaseNumber"], Is.EqualTo(2));

				var usageTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.Usage");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(3), "Should add all entries to Usage");

				var chargeableTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.Chargeable");
				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(0), "Should add no entries to chargeable");
			}
		}

		[Test]
		public void TestDoNotAddFakeCodeHistoryEntriesForWGRTransactions_WhenACompanyExists()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				var currentYear = GetCurrentYear();
				var transactionTime = new DateTime(currentYear - 1, 12, 11);
				var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(transactionTime);

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "GJK???ITN",
					ClientNumber = null,
					ClientStaffCode = null,
					Category = "WGR",
					PriceItemCode = "WGR",
					Reference1 = "Ref 1-1",
					Reference2 = "Ref 1-2",
					Reference3 = "Ref 1-3",
					Reference4 = "Ref 1-4",
					Reference5 = "Ref 1-5",
					ReportingSource = "MSC",
					ServiceOccuredUTC = transactionTime.AddDays(-1),
					Version = 0,
				};

				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "BBB", serverCode: "ITN", enterpriseCode: "GJK");

				BillingDataTestHelper.AddTransaction(trn1);

				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, daysAfter: 1, performMonthlyAggregation: false, isTestServer: true);

				var companyHistoryTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.ClientCompanyCodeHistory");
				Assert.That(companyHistoryTable.Rows.Count, Is.EqualTo(1), "Should not add fake entries to db");

				var licenceHistoryTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.LicenceDatabaseCodeHistory");
				Assert.That(licenceHistoryTable.Rows.Count, Is.EqualTo(1), "Should not add fake entries to db");

				var companyTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.ClientCompany");
				Assert.That(companyTable.Rows.Count, Is.EqualTo(1), "Should not add fake entries to db");

				Assert.That(licenceHistoryTable.Rows[0]["EnterpriseCode"], Is.EqualTo("GJK"));
				Assert.That(licenceHistoryTable.Rows[0]["ServerCode"], Is.EqualTo("ITN"));
				Assert.That(licenceHistoryTable.Rows[0]["DatabaseNumber"], Is.EqualTo(1));

				Assert.That(companyHistoryTable.Rows[0]["CompanyCode"], Is.EqualTo("BBB"));
				Assert.That(companyHistoryTable.Rows[0]["CompanyNumber"], Is.EqualTo(1));
				Assert.That(companyHistoryTable.Rows[0]["DatabaseNumber"], Is.EqualTo(1));

				Assert.That(companyTable.Rows[0]["CompanyNumber"], Is.EqualTo(1));
				Assert.That(companyTable.Rows[0]["DatabaseNumber"], Is.EqualTo(1));

				var usageTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.Usage");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(1), "Should add all entries to Usage");

				var chargeableTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.Chargeable");
				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(0), "Should add no entries to chargeable");
			}
		}

		[Test]
		public void TestDontAddFakeCodeHistoryEntries()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				var currentYear = GetCurrentYear();
				var transactionTime = new DateTime(currentYear - 1, 12, 11);
				var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(transactionTime);

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENTJVCKSW",
					ClientNumber = null,
					ClientStaffCode = "R",
					Category = "ANY",
					PriceItemCode = "ANY",
					Reference1 = "Ref 1",
					Reference2 = "Ref 2",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transactionTime,
					Version = 0,
				};

				BillingDataTestHelper.AddDatabaseAndCompany(con, 1234, 4321, "JVC", serverCode: "KSW");

				BillingDataTestHelper.AddTransaction(trn1);

				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, daysAfter: 1, performMonthlyAggregation: false, isTestServer: true);

				var licenceHistoryTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.LicenceDatabaseCodeHistory");
				var companyHistoryTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.ClientCompanyCodeHistory");
				var chargeableTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.Chargeable");
				var companyTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.ClientCompany");
				Assert.That(licenceHistoryTable.Rows.Count, Is.EqualTo(1), "Should not have made an extra entry");
				Assert.That(companyHistoryTable.Rows.Count, Is.EqualTo(1), "Should not have made an extra entry");
				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(1), "Should have processed to chargeable");
				Assert.That(companyTable.Rows.Count, Is.EqualTo(1), "Should not have made an extra entry");
			}
		}

		[Test]
		public void TestDontAddFakeCodeHistoryEntriesWhenNotTest()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				var currentYear = GetCurrentYear();
				var transactionTime = new DateTime(currentYear - 1, 12, 11);
				var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(transactionTime);

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENTJVCKSW",
					ClientNumber = null,
					ClientStaffCode = "R",
					Category = "ANY",
					PriceItemCode = "ANY",
					Reference1 = "Ref 1",
					Reference2 = "Ref 2",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transactionTime,
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trn1);

				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, daysAfter: 1, performMonthlyAggregation: false, isTestServer: false);

				var licenceHistoryTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.LicenceDatabaseCodeHistory");
				var companyHistoryTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.ClientCompanyCodeHistory");
				var chargeableTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.Chargeable");
				var companyTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.ClientCompany");
				Assert.That(licenceHistoryTable.Rows.Count, Is.EqualTo(0), "Should not have made an extra entry");
				Assert.That(companyHistoryTable.Rows.Count, Is.EqualTo(0), "Should not have made an extra entry");
				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(0), "Should not have made an extra entry");
				Assert.That(companyTable.Rows.Count, Is.EqualTo(0), "Should not have made an extra entry");
			}
		}

		[Test]
		public void TestDontAddFakeCodeHistoryEntriesForStrangeClientIDCases()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				var currentYear = GetCurrentYear();
				var transactionTime = new DateTime(currentYear - 1, 12, 11);
				var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(transactionTime);

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "FOGHT",
					ClientNumber = null,
					ClientStaffCode = "R",
					Category = "ANY",
					PriceItemCode = "ANY",
					Reference1 = "Ref 1",
					Reference2 = "Ref 2",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transactionTime,
					Version = 0,
				};

				var trn2 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "?????????",
					ClientNumber = null,
					ClientStaffCode = "R",
					Category = "ANY",
					PriceItemCode = "ANY",
					Reference1 = "Ref 1",
					Reference2 = "Ref 2",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transactionTime,
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trn1);
				BillingDataTestHelper.AddTransaction(trn2);

				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, daysAfter: 1, performMonthlyAggregation: false, isTestServer: true);

				var licenceHistoryTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.LicenceDatabaseCodeHistory");
				var companyHistoryTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.ClientCompanyCodeHistory");
				var companyTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.ClientCompany");
				Assert.That(licenceHistoryTable.Rows.Count, Is.EqualTo(0), "Should not have made an extra entry");
				Assert.That(companyHistoryTable.Rows.Count, Is.EqualTo(0), "Should not have made an extra entry");
				Assert.That(companyTable.Rows.Count, Is.EqualTo(0), "Should not have made an extra entry");
			}
		}

		[Test]
		public void TestFakeSyncCodeHistoryDatabaseAndCompanyNumberValues()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				var currentYear = GetCurrentYear();
				var transactionTime = new DateTime(currentYear - 1, 12, 11);
				var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(transactionTime);

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "AAABBBCCC",
					ClientNumber = null,
					ClientStaffCode = "R",
					Category = "ANY",
					PriceItemCode = "ANY",
					Reference1 = "Ref 1",
					Reference2 = "Ref 2",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transactionTime,
					Version = 0,
				};

				var trn2 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "AAADDDCCC",
					ClientNumber = null,
					ClientStaffCode = "R",
					Category = "ANY",
					PriceItemCode = "ANY",
					Reference1 = "Ref 1",
					Reference2 = "Ref 2",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transactionTime,
					Version = 0,
				};

				var trn3 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "XXXBBBYYY",
					ClientNumber = null,
					ClientStaffCode = "R",
					Category = "ANY",
					PriceItemCode = "ANY",
					Reference1 = "Ref 1",
					Reference2 = "Ref 2",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transactionTime,
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trn1);
				BillingDataTestHelper.AddTransaction(trn2);
				BillingDataTestHelper.AddTransaction(trn3);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1234, 965, "JVC",enterpriseCode: "AAA", serverCode: "CCC");
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1163, 4321, "YOY", enterpriseCode: "FNM", serverCode: "MNF");

				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, daysAfter: 1, performMonthlyAggregation: false, isTestServer: true);

				var licenceHistoryTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.LicenceDatabaseCodeHistory");
				var companyHistoryTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.ClientCompanyCodeHistory");
				var chargeableTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.Chargeable");
				var companyTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.ClientCompany");

				Assert.That(licenceHistoryTable.Rows.Count, Is.EqualTo(3), "Should have added fake entries to db");
				Assert.That(companyHistoryTable.Rows.Count, Is.EqualTo(5), "Should have added fake entries to db");
				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(3), "Should have processed to chargeable table");
				Assert.That(companyTable.Rows.Count, Is.EqualTo(5), "Should have added fake entries to db");

				var licenceRow = licenceHistoryTable.Rows.Cast<DataRow>().Single(x => (string)x["EnterpriseCode"] == "XXX" && (string)x["ServerCode"] == "YYY");
				Assert.That(licenceRow["DatabaseNumber"], Is.EqualTo(1235), "DatabaseNumber should be +1 from highest existing");
				Assert.That(licenceRow["SystemID"], Is.EqualTo("CY2"), "SystemID should be Base27Encoded DatabaseNumber");

				var companyHistoryRow = companyHistoryTable.Rows.Cast<DataRow>().Single(x => (string)x["CompanyCode"] == "BBB" && Convert.ToInt32(x["DatabaseNumber"]) == 1234);
				Assert.That(companyHistoryRow["CompanyNumber"], Is.EqualTo(966), "CompanyNumber should be +1 from highest existing for this database number");

				var companyHistoryRow2 = companyHistoryTable.Rows.Cast<DataRow>().Single(x => (string)x["CompanyCode"] == "DDD");
				Assert.That(companyHistoryRow2["DatabaseNumber"], Is.EqualTo(1234), "DatabaseNumber should be taken from licence row");
				Assert.That(companyHistoryRow2["CompanyNumber"], Is.EqualTo(967), "CompanyNumber should be +1 from highest existing for this database number");

				var companyHistoryRow3 = companyHistoryTable.Rows.Cast<DataRow>().Single(x => (string)x["CompanyCode"] == "BBB" && Convert.ToInt32(x["DatabaseNumber"]) == 1235);
				Assert.That(companyHistoryRow3["CompanyNumber"], Is.EqualTo(1), "CompanyNumber should be 1 as no other company number exists for this database number");

				Assert.DoesNotThrow(() => companyTable.Rows.Cast<DataRow>().Single(x => Convert.ToInt32(x["CompanyNumber"]) == 965 && Convert.ToInt32(x["DatabaseNumber"]) == 1234), "Should have added fake company");
				Assert.DoesNotThrow(() => companyTable.Rows.Cast<DataRow>().Single(x => Convert.ToInt32(x["CompanyNumber"]) == 966 && Convert.ToInt32(x["DatabaseNumber"]) == 1234), "Should have added fake company");
				Assert.DoesNotThrow(() => companyTable.Rows.Cast<DataRow>().Single(x => Convert.ToInt32(x["CompanyNumber"]) == 1 && Convert.ToInt32(x["DatabaseNumber"]) == 1235), "Should have added fake company");

				var chargeableRow = chargeableTable.Rows.Cast<DataRow>().Single(x => (string)x["CH_ClientID"] == "AAABBBCCC");
				Assert.That(chargeableRow["CH_DatabaseNumber"], Is.EqualTo(1234), "Chargeable should have correct DatabaseNumber");
				Assert.That(chargeableRow["CH_CompanyNumber"], Is.EqualTo(966), "Chargeable should have correct CompanyNumber");

				var chargeableRow2 = chargeableTable.Rows.Cast<DataRow>().Single(x => (string)x["CH_ClientID"] == "AAADDDCCC");
				Assert.That(chargeableRow2["CH_DatabaseNumber"], Is.EqualTo(1234), "Chargeable should have correct DatabaseNumber");
				Assert.That(chargeableRow2["CH_CompanyNumber"], Is.EqualTo(967), "Chargeable should have correct CompanyNumber");

				var chargeableRow3 = chargeableTable.Rows.Cast<DataRow>().Single(x => (string)x["CH_ClientID"] == "XXXBBBYYY");
				Assert.That(chargeableRow3["CH_DatabaseNumber"], Is.EqualTo(1235), "Chargeable should have correct DatabaseNumber");
				Assert.That(chargeableRow3["CH_CompanyNumber"], Is.EqualTo(1), "Chargeable should have correct CompanyNumber");
			}
		}

		[Test]
		public void TestAddFakeCodeHistoryEntriesWithExistingData()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				var currentYear = GetCurrentYear();
				var transactionTime = new DateTime(currentYear - 1, 12, 11);
				var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(transactionTime);

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENTBBBWWW",
					ClientNumber = null,
					ClientStaffCode = "R",
					Category = "ANY",
					PriceItemCode = "ANY",
					Reference1 = "Ref 1",
					Reference2 = "Ref 2",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transactionTime,
					Version = 0,
				};

				BillingDataTestHelper.AddDatabase(con, 1, serverCode: "WWW", product: "ANY");
				BillingDataTestHelper.AddDatabaseAndCompany(con, 2, 1, "BBB", enterpriseCode: "123", serverCode: "456");

				BillingDataTestHelper.AddTransaction(trn1);
				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, daysAfter: 1, performMonthlyAggregation: false, isTestServer: true);

				var licenceHistoryTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.LicenceDatabaseCodeHistory");
				Assert.That(licenceHistoryTable.Rows.Count, Is.EqualTo(2), "Should find existing licenceHistory and not add one");

				var companyTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.ClientCompany");
				Assert.That(companyTable.Rows.Count, Is.EqualTo(2), "Should add a fake entry to db");
				var compRow = companyTable.Rows[0];
				Assert.That(compRow["CompanyNumber"], Is.EqualTo(1));
				Assert.That(compRow["DatabaseNumber"], Is.EqualTo(1));

				var companyHistoryTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.ClientCompanyCodeHistory");
				Assert.That(companyHistoryTable.Rows.Count, Is.EqualTo(2), "Should add a fake entry to db");
				var compHRow = companyHistoryTable.Rows[0];
				Assert.That(compHRow["CompanyCode"], Is.EqualTo("BBB"));
				Assert.That(compHRow["CompanyNumber"], Is.EqualTo(1));
				Assert.That(compHRow["DatabaseNumber"], Is.EqualTo(1));

				var chargeableTable = BillingDataTestHelper.LoadTable(con, "SELECT * FROM edi.Chargeable");
				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(1), "Should add a fake entry to db");
				var chargeableRow = chargeableTable.Rows[0];
				BillingDataTestHelper.AssertChargeablePropertiesAreEqual(trn1, 1, 1, chargeableRow);
			}
		}

		[TestCase("CW1")]
		[TestCase("CWN")]
		public void TestProcessTestTrainingTransaction(string productCode)
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.TruncateTable(con, "edi.ConfigLastMessageFilter");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);

				var currentYear = GetCurrentYear();
				var transactionTime = new DateTime(currentYear - 1, 12, 11);
				var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(transactionTime);

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENT???OKI",
					ClientNumber = "",
					ClientStaffCode = null,
					Category = "ANY",
					PriceItemCode = "STS",
					Reference1 = "Ref 1",
					Reference2 = "Ref 2",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transactionTime,
					Version = 0,
				};

				var trn2 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENTWEWGFD",
					ClientNumber = "",
					ClientStaffCode = null,
					Category = "STL",
					PriceItemCode = "STS",
					Reference1 = "Ref 1",
					Reference2 = "Ref 2",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transactionTime,
					Version = 0,
				};

				var trn3 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENTKJHPOK",
					ClientNumber = "",
					ClientStaffCode = null,
					Category = "STL",
					PriceItemCode = "ANY",
					Reference1 = "Ref 1",
					Reference2 = "Ref 2",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transactionTime,
					Version = 0,
				};

				var trn4 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENTJIHYTT",
					ClientNumber = "",
					ClientStaffCode = null,
					Category = "STL",
					PriceItemCode = "STS",
					Reference1 = "Ref 1",
					Reference2 = "Ref 2",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transactionTime,
					Version = 0,
				};

				var trn5 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENTFEWRDS",
					ClientNumber = "",
					ClientStaffCode = null,
					Category = "STL",
					PriceItemCode = "STL",
					Reference1 = "Ref 1",
					Reference2 = "Ref 2",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transactionTime,
					Version = 0,
				};

				var trn6 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENTLLLKKK",
					ClientNumber = "",
					ClientStaffCode = null,
					Category = "ANY",
					PriceItemCode = "ANY",
					Reference1 = "Ref 1",
					Reference2 = "Ref 2",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transactionTime,
					Version = 0,
				};

				BillingDataTestHelper.AddDatabaseAndCompany(con, 333, 444, "KHJ", product: productCode, serverCode: "OKI", licenceType: "ANY");
				BillingDataTestHelper.AddDatabaseAndCompany(con, 666, 888, "WEW", product: productCode, serverCode: "GFD", licenceType: "ANY", hostedLocation: "NCW");
				BillingDataTestHelper.AddDatabaseAndCompany(con, 222, 111, "KJH", product: productCode, serverCode: "POK", licenceType: "ANY");
				BillingDataTestHelper.AddDatabaseAndCompany(con, 444, 222, "JIH", product: productCode, serverCode: "YTT", licenceType: "ANY");
				BillingDataTestHelper.AddDatabaseAndCompany(con, 999, 666, "FEW", product: productCode, serverCode: "RDS", licenceType: "ANY");
				BillingDataTestHelper.AddDatabaseAndCompany(con, 777, 555, "LLL", product: productCode, serverCode: "KKK", licenceType: "PRD");

				BillingDataTestHelper.AddTransaction(trn1);
				BillingDataTestHelper.AddTransaction(trn2);
				BillingDataTestHelper.AddTransaction(trn3);
				BillingDataTestHelper.AddTransaction(trn4);
				BillingDataTestHelper.AddTransaction(trn5);
				BillingDataTestHelper.AddTransaction(trn6);

				ExecuteProcessStaging(con);

				var chargeableTable = BillingDataTestHelper.LoadTableByName(con, "edi.Chargeable");
				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");

				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(3), "Chargeable rows");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(3), "Usage rows");

				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn1, 333, 444, usageTable.Rows.Cast<DataRow>().Single(x => (String)x["US_ClientID"] == "ENT???OKI"));
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn2, 666, 888, usageTable.Rows.Cast<DataRow>().Single(x => (String)x["US_ClientID"] == "ENTWEWGFD"));
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn3, 222, 111, usageTable.Rows.Cast<DataRow>().Single(x => (String)x["US_ClientID"] == "ENTKJHPOK"));
				BillingDataTestHelper.AssertChargeablePropertiesAreEqual(trn4, 444, 222, chargeableTable.Rows.Cast<DataRow>().Single(x => (String)x["CH_ClientID"] == "ENTJIHYTT"));
				BillingDataTestHelper.AssertChargeablePropertiesAreEqual(trn5, 999, 666, chargeableTable.Rows.Cast<DataRow>().Single(x => (String)x["CH_ClientID"] == "ENTFEWRDS"));
				BillingDataTestHelper.AssertChargeablePropertiesAreEqual(trn6, 777, 555, chargeableTable.Rows.Cast<DataRow>().Single(x => (String)x["CH_ClientID"] == "ENTLLLKKK"));
			}
		}

		[Test]
		public void TestProcessFirstMessage_NotIncludeCompanyNumber()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "SYD");
				BillingDataTestHelper.AddCompany(con, 1, 2, "BNE");
				BillingDataTestHelper.AddCompany(con, 1, 3, "MEL");

				BillingDataTestHelper.ExecuteNonQuery(con,
					"delete from edi.ConfigFirstMessage where FM_Category = 'Z_Z'; " +
					"insert edi.ConfigFirstMessage(FM_Category, FM_PriceItemCode, FM_IncludeRef1, FM_IncludeRef2, FM_IncludeCompanyNumber) " +
					"values('Z_Z', 'Z_Z', 1, 1, 0);");

				var currentYear = GetCurrentYear();

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENTSYDSRV",
					ClientNumber = "C.SYD",
					ClientStaffCode = "ABC",
					Category = "Z_Z",
					PriceItemCode = "Z_Z",
					Reference1 = "REF 1",
					Reference2 = null,
					Reference3 = "REF 3",
					ReportingSource = "XYZ",
					ServiceOccuredUTC = new DateTime(currentYear - 3, 7, 19, 9, 0, 0),
					Version = 1,
					MessageTrackingID = "Message1"
				};

				BillingDataTestHelper.AddTransaction(trn1);

				var trn2 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENTBNESRV",
					ClientNumber = "C.BNE",
					ClientStaffCode = "ABC",
					Category = "Z_Z",
					PriceItemCode = "Z_Z",
					Reference1 = "REF 1",
					Reference2 = null,
					Reference3 = "REF 3",
					ReportingSource = "XYZ",
					ServiceOccuredUTC = new DateTime(currentYear - 3, 7, 20, 9, 0, 0),
					Version = 1,
					MessageTrackingID = "Message2"
				};

				BillingDataTestHelper.AddTransaction(trn2);

				var trnUnmatched = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENTSYDZZZ",
					ClientNumber = null,
					ClientStaffCode = "ABC",
					Category = "Z_Z",
					PriceItemCode = "Z_Z",
					Reference1 = "REF 1",
					Reference2 = null,
					Reference3 = "REF 3",
					ReportingSource = "XYZ",
					ServiceOccuredUTC = new DateTime(currentYear - 3, 7, 21, 9, 0, 0),
					Version = 1,
					MessageTrackingID = "Message3"
				};

				BillingDataTestHelper.AddTransaction(trnUnmatched);

				ExecuteProcessStaging(con);

				// run again
				ExecuteProcessStaging(con);

				var table = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				var stagingUnknownTable = BillingDataTestHelper.LoadTableByName(con, "edi.StagingUnknownSystems");

				Assert.That(table.Rows.Count, Is.EqualTo(1), "first message count");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(1), "second message");
				Assert.That(stagingUnknownTable.Rows.Count, Is.EqualTo(1), "unmatched message");

				BillingDataTestHelper.AssertPropertiesAreEqual(trn1, table.Rows[0], "CH_");

				var usageForTrn2 = usageTable.Rows.Cast<DataRow>().Single(x => (string)x["US_MessageTrackingID"] == "Message2");
				var usageForUnmatched = stagingUnknownTable.Rows.Cast<DataRow>().Single(x => (string)x["TX_MessageTrackingID"] == "Message3");
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn2, expectedDatabaseNumber: 1, expectedCompanyNumber: 2, usageForTrn2);

				BillingDataTestHelper.AddDatabaseAndCompany(con, 3, 1, "SYD", serverCode: "ZZZ");
				ExecuteProcessStaging(con);

				table = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				stagingUnknownTable = BillingDataTestHelper.LoadTableByName(con, "edi.StagingUnknownSystems");

				Assert.That(table.Rows.Count, Is.EqualTo(2), "first message count on each client Id");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(1), "second message");
				Assert.That(stagingUnknownTable.Rows.Count, Is.EqualTo(0), "no unmatched messages now that the system is licensed");

				BillingDataTestHelper.AssertPropertiesAreEqual(trnUnmatched, table.Rows.Cast<DataRow>().Single(x => (string)x["CH_MessageTrackingID"] == "Message3"), "CH_");

				var trn4 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENTMELSRV",
					ClientNumber = "C.MEL",
					ClientStaffCode = "ABC",
					Category = "Z_Z",
					PriceItemCode = "Z_Z",
					Reference1 = "REF 1",
					Reference2 = null,
					Reference3 = "REF 3",
					ReportingSource = "XYZ",
					ServiceOccuredUTC = new DateTime(currentYear - 3, 7, 22, 9, 0, 0),
					Version = 1,
					MessageTrackingID = "Message4"
				};

				BillingDataTestHelper.AddTransaction(trn4);
				ExecuteProcessStaging(con);

				table = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				stagingUnknownTable = BillingDataTestHelper.LoadTableByName(con, "edi.StagingUnknownSystems");

				Assert.That(table.Rows.Count, Is.EqualTo(2), "first message count on each client Id");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(2), "second and fourth message");
				Assert.That(stagingUnknownTable.Rows.Count, Is.EqualTo(0), "no unmatched messages");

				usageForTrn2 = usageTable.Rows.Cast<DataRow>().Single(x => (string)x["US_MessageTrackingID"] == "Message2");
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn2, expectedDatabaseNumber: 1, expectedCompanyNumber: 2, usageForTrn2);
				var usageForTrn4 = usageTable.Rows.Cast<DataRow>().Single(x => (string)x["US_MessageTrackingID"] == "Message4");
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn4, expectedDatabaseNumber: 1, expectedCompanyNumber: 3, usageForTrn4);
			}
		}

		void AddMilestone(int year, int month, int day, string clientId, string additionalProperty = null)
		{
			BillingDataTestHelper.CreateAndAddTransaction(priceItemCode: "STL", new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc), clientId: clientId, reference1: $"{year}-{month}-{day}", reference2: "Day", reference3: additionalProperty);
		}

		void AddLines(SqlConnection con, int startLine, int endLine, API.BillingTransaction trn, DateTime systemCreateUTC)
		{
			for (int i = startLine; i <= endLine; ++i)
			{
				trn.Reference1 = "Line " + i.ToString("00#");
				BillingDataTestHelper.AddTransaction(trn);
			}

			using (var cmd = con.CreateCommand())
			{
				cmd.CommandText = "update dbo.Staging set TX_SystemCreateUTC = @SystemCreateUTC where TX_PriceItemCode = 'ICK' and TX_SystemCreateUTC > '2016-7-1'";
				cmd.Parameters.AddWithValue("@SystemCreateUTC", systemCreateUTC);
				cmd.ExecuteNonQuery();
			}
		}

		public void ExecuteProcessStaging(SqlConnection con, bool isTestServer = false)
		{
			using (var cmd = con.CreateCommand())
			{
				cmd.CommandText = "edi.ProcessStaging";
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@isTestServer", SqlDbType.Bit).Value = isTestServer ? 1 : 0;
				cmd.ExecuteNonQuery();
			}
		}

		int GetCurrentYear() => DateTime.Now.Year;

		[SetUp]
		public void SetUp()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.SetUpBillingRules(con);
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.TruncateTable(con, "edi.ClientCompanyEdiProdCache");
				BillingDataTestHelper.TruncateTable(con, "edi.LicenceDatabaseEdiProdCache");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
			}
		}
	}
}

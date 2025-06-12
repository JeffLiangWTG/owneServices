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
	public class BillingLastMessageTest
	{
		[Test]
		public void TestProcessLastMessageTransactions()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.TruncateTable(con, "edi.ConfigLastMessageFilter");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "NWF", serverCode: "TPP");
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 2, "DSA", serverCode: "YRT");
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 3, "HER", serverCode: "WER");
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 4, "NGT", serverCode: "BRF");

				AddLastMessageFilter(con, "STL", "STS");

				var currentYear = GetCurrentYear();
				var transactionTime = new DateTime(currentYear - 1, 12, 11);
				var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(transactionTime);

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENTNWFTPP",
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

				var trn2 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENTDSAYRT",
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

				var trn3 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENTHERWER",
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
					ClientID = "ENTNGTBRF",
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

				BillingDataTestHelper.AddTransaction(trn1);
				BillingDataTestHelper.AddTransaction(trn2);
				BillingDataTestHelper.AddTransaction(trn3);
				BillingDataTestHelper.AddTransaction(trn4);

				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, 1, performMonthlyAggregation: false);

				var chargeableTable = BillingDataTestHelper.LoadTableByName(con, "edi.Chargeable");
				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");

				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(3), "Chargeable rows");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(1), "Usage rows");
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn1, 1, 1, usageTable.Rows[0]);
				BillingDataTestHelper.TruncateTable(con, "edi.ConfigLastMessageFilter");
			}
		}

		[Test]
		public void TestProcessLastMessage()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.TruncateTable(con, "edi.ConfigLastMessageFilter");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "NWF", serverCode: "TPP");

				AddLastMessageFilter(con, "STL", "STS");

				var currentYear = GetCurrentYear();
				var transactionTime = new DateTime(currentYear - 1, 12, 11);
				var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(transactionTime);

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENTNWFTPP",
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

				BillingDataTestHelper.AddTransaction(trn1);
				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, 1, performMonthlyAggregation: false);

				var chargeableTable = BillingDataTestHelper.LoadTableByName(con, "edi.Chargeable");
				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");

				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(0), "Chargeable rows");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(1), "Usage rows");

				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, 1, performMonthlyAggregation: true);

				chargeableTable = BillingDataTestHelper.LoadTableByName(con, "edi.Chargeable");
				usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");

				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(1), "Chargeable rows");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(1), "Usage rows");
				BillingDataTestHelper.TruncateTable(con, "edi.ConfigLastMessageFilter");

			}
		}

		[Test]
		public void TestProcessLastMessageOneRecordPerCustomerPerBucketPerFilter()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.TruncateTable(con, "edi.ConfigLastMessageFilter");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "BBB", enterpriseCode: "AAA", serverCode: "CCC");
				BillingDataTestHelper.AddDatabaseAndCompany(con, 2, 2, "EEE", enterpriseCode: "DDD", serverCode: "FFF");

				AddLastMessageFilter(con, "STL", "STS");
				AddLastMessageFilter(con, "SMF", "HDS");

				var currentYear = GetCurrentYear();
				var transactionTime = new DateTime(currentYear - 1, 12, 11);
				var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(transactionTime);

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "AAABBBCCC",
					ClientNumber = "",
					ClientStaffCode = null,
					Category = "STL",
					PriceItemCode = "STS",
					Reference1 = "Bucket1",
					Reference2 = "trn1",
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
					ClientID = "DDDEEEFFF",
					ClientNumber = "",
					ClientStaffCode = null,
					Category = "STL",
					PriceItemCode = "STS",
					Reference1 = "Bucket1",
					Reference2 = "trn2",
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
					ClientID = "AAABBBCCC",
					ClientNumber = "",
					ClientStaffCode = null,
					Category = "STL",
					PriceItemCode = "STS",
					Reference1 = "Bucket1",
					Reference2 = "trn3",
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
					ClientID = "AAABBBCCC",
					ClientNumber = "",
					ClientStaffCode = null,
					Category = "STL",
					PriceItemCode = "STS",
					Reference1 = "Bucket2",
					Reference2 = "trn4",
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
					ClientID = "AAABBBCCC",
					ClientNumber = "",
					ClientStaffCode = null,
					Category = "SMF",
					PriceItemCode = "HDS",
					Reference1 = "Bucket1",
					Reference2 = "trn5",
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
				BillingDataTestHelper.AddTransaction(trn4);
				BillingDataTestHelper.AddTransaction(trn5);
				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, 1, performMonthlyAggregation: false);

				var chargeableTable = BillingDataTestHelper.LoadTableByName(con, "edi.Chargeable");
				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");

				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(0), "Chargeable rows");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(5), "Usage rows");

				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, 1, performMonthlyAggregation: true);

				chargeableTable = BillingDataTestHelper.LoadTableByName(con, "edi.Chargeable");
				usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");

				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(4), "Chargeable rows");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(5), "Usage rows");

				BillingDataTestHelper.AssertChargeablePropertiesAreEqual(trn1, 1, 1, chargeableTable.Rows.Cast<DataRow>().Single(x => (String)x["CH_Reference2"] == "trn1"));
				BillingDataTestHelper.AssertChargeablePropertiesAreEqual(trn2, 2, 2, chargeableTable.Rows.Cast<DataRow>().Single(x => (String)x["CH_Reference2"] == "trn2"));
				BillingDataTestHelper.AssertChargeablePropertiesAreEqual(trn4, 1, 1, chargeableTable.Rows.Cast<DataRow>().Single(x => (String)x["CH_Reference2"] == "trn4"));
				BillingDataTestHelper.AssertChargeablePropertiesAreEqual(trn5, 1, 1, chargeableTable.Rows.Cast<DataRow>().Single(x => (String)x["CH_Reference2"] == "trn5"));

				BillingDataTestHelper.TruncateTable(con, "edi.ConfigLastMessageFilter");
			}
		}

		[Test]
		public void TestProcessLastMessageOnlyIfInConfig()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.TruncateTable(con, "edi.ConfigLastMessageFilter");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);

				AddLastMessageFilter(con, "STL", "STS");
				AddLastMessageFilter(con, "SMF", "HDS");

				var currentYear = GetCurrentYear();
				var transactionTime = new DateTime(currentYear - 1, 12, 11);
				var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(transactionTime);

				BillingDataTestHelper.ExecuteNonQuery(con, $"INSERT edi.Usage(US_Period, US_Category, US_PriceItemCode, US_BillableCount, US_ReportingSource, US_ServiceOccuredUtc, US_ClientID, US_Reference1, US_CapturedUtc) VALUES({chargeablePeriod}, 'STL', 'STS', 1, 'ENT', '{transactionTime}', 'AAABBBCCC', 'Bucket1', '{transactionTime}')");
				BillingDataTestHelper.ExecuteNonQuery(con, $"INSERT edi.Usage(US_Period, US_Category, US_PriceItemCode, US_BillableCount, US_ReportingSource, US_ServiceOccuredUtc, US_ClientID, US_Reference1, US_CapturedUtc) VALUES({chargeablePeriod}, 'SMF', 'HDS', 1, 'ENT', '{transactionTime}', 'AAABBBCCC', 'Bucket2', '{transactionTime}')");
				BillingDataTestHelper.ExecuteNonQuery(con, $"INSERT edi.Usage(US_Period, US_Category, US_PriceItemCode, US_BillableCount, US_ReportingSource, US_ServiceOccuredUtc, US_ClientID, US_Reference1, US_CapturedUtc) VALUES({chargeablePeriod}, 'STL', 'YTR', 1, 'ENT', '{transactionTime}', 'AAABBBCCC', 'Bucket3', '{transactionTime}')");
				BillingDataTestHelper.ExecuteNonQuery(con, $"INSERT edi.Usage(US_Period, US_Category, US_PriceItemCode, US_BillableCount, US_ReportingSource, US_ServiceOccuredUtc, US_ClientID, US_Reference1, US_CapturedUtc) VALUES({chargeablePeriod}, 'KEH', 'HDS', 1, 'ENT', '{transactionTime}', 'AAABBBCCC', 'Bucket4', '{transactionTime}')");

				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, 1, performMonthlyAggregation: true);

				var chargeableTable = BillingDataTestHelper.LoadTableByName(con, "edi.Chargeable");
				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");

				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(2), "Chargeable rows");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(4), "Usage rows");

				BillingDataTestHelper.TruncateTable(con, "edi.ConfigLastMessageFilter");
			}
		}

		[Test]
		public void TestProcessLastMessageOnlyMostRecent()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.TruncateTable(con, "edi.ConfigLastMessageFilter");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "BBB", enterpriseCode: "AAA", serverCode: "CCC");

				AddLastMessageFilter(con, "STL", "STS");

				var currentYear = GetCurrentYear();
				var transactionTime = new DateTime(currentYear - 1, 12, 11);
				var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(transactionTime);

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "AAABBBCCC",
					ClientNumber = "",
					ClientStaffCode = null,
					Category = "STL",
					PriceItemCode = "STS",
					Reference1 = "Bucket1",
					Reference2 = "trn1",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transactionTime.AddDays(-5),
					Version = 0,
				};

				var trn2 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "AAABBBCCC",
					ClientNumber = "",
					ClientStaffCode = null,
					Category = "STL",
					PriceItemCode = "STS",
					Reference1 = "Bucket1",
					Reference2 = "trn2",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transactionTime.AddDays(-7),
					Version = 0,
				};

				var trn3 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "AAABBBCCC",
					ClientNumber = "",
					ClientStaffCode = null,
					Category = "STL",
					PriceItemCode = "STS",
					Reference1 = "Bucket1",
					Reference2 = "trn3",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transactionTime.AddDays(-1),
					Version = 0,
				};

				var trn4 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "AAABBBCCC",
					ClientNumber = "",
					ClientStaffCode = null,
					Category = "STL",
					PriceItemCode = "STS",
					Reference1 = "Bucket1",
					Reference2 = "trn4",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transactionTime.AddDays(-2),
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trn1);
				BillingDataTestHelper.AddTransaction(trn2);
				BillingDataTestHelper.AddTransaction(trn3);
				BillingDataTestHelper.AddTransaction(trn4);
				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, 1, performMonthlyAggregation: false);

				var chargeableTable = BillingDataTestHelper.LoadTableByName(con, "edi.Chargeable");
				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");

				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(0), "Chargeable rows");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(4), "Usage rows");

				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, 1, performMonthlyAggregation: true);

				chargeableTable = BillingDataTestHelper.LoadTableByName(con, "edi.Chargeable");
				usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");

				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(1), "Chargeable rows");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(4), "Usage rows");

				BillingDataTestHelper.AssertChargeablePropertiesAreEqual(trn3, 1, 1, chargeableTable.Rows.Cast<DataRow>().Single(x => (String)x["CH_Reference2"] == "trn3"));

				BillingDataTestHelper.TruncateTable(con, "edi.ConfigLastMessageFilter");
			}
		}

		[Test]
		public void TestProcessLastMessageCorrectlyDeduplicatesReference2IfInConfig()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.TruncateTable(con, "edi.ConfigLastMessageFilter");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "BBB", enterpriseCode: "AAA", serverCode: "CCC");
				BillingDataTestHelper.AddDatabaseAndCompany(con, 2, 2, "EEE", enterpriseCode: "DDD", serverCode: "FFF");

				AddLastMessageFilter(con, "STL", "STS", 1);

				var currentYear = GetCurrentYear();
				var transactionTime = new DateTime(currentYear - 1, 12, 11);
				var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(transactionTime);

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "AAABBBCCC",
					ClientNumber = "",
					ClientStaffCode = null,
					Category = "STL",
					PriceItemCode = "STS",
					Reference1 = "Bucket1",
					Reference2 = "trn1",
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
					ClientID = "DDDEEEFFF",
					ClientNumber = "",
					ClientStaffCode = null,
					Category = "STL",
					PriceItemCode = "STS",
					Reference1 = "Bucket1",
					Reference2 = "trn2",
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
					ClientID = "AAABBBCCC",
					ClientNumber = "",
					ClientStaffCode = null,
					Category = "STL",
					PriceItemCode = "STS",
					Reference1 = "Bucket1",
					Reference2 = "trn1",
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
					ClientID = "AAABBBCCC",
					ClientNumber = "",
					ClientStaffCode = null,
					Category = "STL",
					PriceItemCode = "STS",
					Reference1 = "Bucket1",
					Reference2 = "trn4",
					Reference3 = "Ref 3",
					Reference4 = "Ref 4",
					Reference5 = "Ref 5",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transactionTime.AddDays(-2),
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trn1);
				BillingDataTestHelper.AddTransaction(trn2);
				BillingDataTestHelper.AddTransaction(trn3);
				BillingDataTestHelper.AddTransaction(trn4);
				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, 1, performMonthlyAggregation: false);

				var chargeableTable = BillingDataTestHelper.LoadTableByName(con, "edi.Chargeable");
				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");

				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(0), "Chargeable rows");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(4), "Usage rows");

				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, 1, performMonthlyAggregation: true);

				chargeableTable = BillingDataTestHelper.LoadTableByName(con, "edi.Chargeable");
				usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");

				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(3), "Chargeable rows");
				Assert.That(usageTable.Rows.Count, Is.EqualTo(4), "Usage rows");

				BillingDataTestHelper.AssertChargeablePropertiesAreEqual(trn1, 1, 1, chargeableTable.Rows.Cast<DataRow>().Single(x => (String)x["CH_Reference2"] == "trn1"));
				BillingDataTestHelper.AssertChargeablePropertiesAreEqual(trn2, 2, 2, chargeableTable.Rows.Cast<DataRow>().Single(x => (String)x["CH_Reference2"] == "trn2"));
				BillingDataTestHelper.AssertChargeablePropertiesAreEqual(trn4, 1, 1, chargeableTable.Rows.Cast<DataRow>().Single(x => (String)x["CH_Reference2"] == "trn4"));

				BillingDataTestHelper.TruncateTable(con, "edi.ConfigLastMessageFilter");
			}
		}

		void AddLastMessageFilter(SqlConnection con, string category, string priceItemCode, int deduplicateRef2 = 0)
		{
			BillingDataTestHelper.ExecuteNonQuery(con, $"insert edi.ConfigLastMessageFilter(ML_Category, ML_PriceItemCode, ML_DeduplicateRef2) values('{category}','{priceItemCode}','{deduplicateRef2}');");
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

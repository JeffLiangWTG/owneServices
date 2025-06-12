using CargoWise.eServices.Billing.Tests.Common;
using NUnit.Framework;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using API = CargoWise.Billing.API;

namespace CargoWise.eServices.Billing.Database.Tests
{
	[TestFixture]
	public class EDIUpdateChargeableTest
	{
		[Test]
		public void TestCompanyAndDatabaseSync()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);

				var pk1 = "E40E516D-040D-4E81-BCF8-DF44214DCFD7";

				BillingDataTestHelper.ExecuteNonQuery(con,
					@"truncate table edi.LicenceDatabaseEdiProdCache;
					truncate table edi.ClientCompanyEdiProdCache;
					insert edi.LicenceDatabaseEdiProdCache(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, SystemCreateUTC)
						values('C', 1, 'ENT', 'SRV', '2016-06-01');
					insert edi.ClientCompanyEdiProdCache(DatabaseNumber, LCC_PK, CompanyCode, ValidFromUtc, CountryCode) values
						(1, '" + pk1 + @"', 'AA1', '2016-06-01', 'AU')");


				var trn = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENTAA1SRV",
					ClientNumber = null,
					ClientStaffCode = "",
					Category = "ZAZ",
					PriceItemCode = "ZUZ",
					Reference1 = "FOO",
					ReportingSource = "XYZ",
					ServiceOccuredUTC = DateTime.UtcNow.AddMonths(-1),
					Version = 1,
				};

				BillingDataTestHelper.AddTransaction(trn);

				BillingDataTestHelper.ExecuteUpdateChargeableAtTime(con, DateTime.UtcNow);

				var table = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				Assert.That(table.Rows.Count, Is.EqualTo(1));
				BillingDataTestHelper.AssertPropertiesAreEqual(trn, table.Rows[0], "CH_");
			}
		}

		[Test]
		public void TestWGRMonthlyChargeablesWithNoUserDiscount()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				var transaction2Time = DateTime.UtcNow.AddMonths(-1);
				var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(transaction2Time);

				WGRMonthlyChargeablesSetupTables(con, chargeablePeriod, users: 0);
				AssertWGRMonthlyChargeables(con, 7, transaction2Time, chargeablePeriod);
			}
		}

		[Test]
		public void TestWGRMonthlyChargeablesWithSmallUserDiscount()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				var transaction2Time = DateTime.UtcNow.AddMonths(-1);
				var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(transaction2Time);

				WGRMonthlyChargeablesSetupTables(con, chargeablePeriod, users: 150);
				AssertWGRMonthlyChargeables(con, 5, transaction2Time, chargeablePeriod);
			}
		}

		[Test]
		public void TestWGRMonthlyChargeablesUserDiscountCompletelyCoversCharge()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				var transaction2Time = DateTime.UtcNow.AddMonths(-1);
				var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(transaction2Time);

				WGRMonthlyChargeablesSetupTables(con, chargeablePeriod, users: 700);
				AssertWGRMonthlyChargeables(con, 0, transaction2Time, chargeablePeriod);
			}
		}

		[Test]
		public void TestWGRMonthlyChargeablesNotCalculatedForNonProductionSystems()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				var transaction2Time = DateTime.UtcNow.AddMonths(-1);
				var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(transaction2Time);

				WGRMonthlyChargeablesSetupTables(con, chargeablePeriod, users: 0, "TST");
				AssertWGRMonthlyChargeables(con, 0, transaction2Time, chargeablePeriod);
			}
		}

		void WGRMonthlyChargeablesSetupTables(SqlConnection con, int chargeablePeriod, int users, string licenceType = "PRD")
		{
			var chargedDatabaseNumber = 1;
			var chargedCompanyNumber = 1;

			BillingDataTestHelper.TruncateStagingTable(con);
			BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
			BillingDataTestHelper.TruncateTable(con, "edi.Usage");
			BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
			BillingDataTestHelper.AddDatabaseAndCompany(con, chargedDatabaseNumber, chargedCompanyNumber, "SYD", licenceType: licenceType);
			BillingDataTestHelper.AddDatabase(con, chargedDatabaseNumber, serverCode: "SV2");
			BillingDataTestHelper.AddDatabaseAndCompany(con, 2, 2, "MEL", serverCode: "PVR", licenceType: licenceType);

			BillingDataTestHelper.AddUserTransactions(con, users, chargeablePeriod, chargedDatabaseNumber, chargedCompanyNumber, clientId: "ENT???SV2");
		}

		void AssertWGRMonthlyChargeables(SqlConnection con, int expectedBillableCount, DateTime transaction2Time, int chargeablePeriod)
		{
			var transaction1Time = new DateTime(chargeablePeriod / 100, chargeablePeriod % 100, 1, 14, 33, 23);
			var transaction3Time = BillingDataTestHelper.GetChargeablePeriodAsDate(chargeablePeriod, 7, DateTimeKind.Utc).AddMonths(1);

			var trn1 = new API.BillingTransaction
			{
				BillableCount = 5423,
				ClientID = "ENT???UKN",
				ClientNumber = null,
				Category = "WGR",
				PriceItemCode = "WGR",
				Reference1 = "900",
				Reference2 = "ClientDirect",
				Reference3 = "Client",
				Reference4 = "LONWP-SSQL-7A.wisecloud.zone/INSTANCEP4",
				Reference5 = "4743",
				ReportingSource = "MSC",
				ServiceOccuredUTC = transaction1Time,
				Version = 0,
			};

			var trn2 = new API.BillingTransaction
			{
				BillableCount = 5678,
				ClientID = "ENT???SV2",
				ClientNumber = null,
				Category = "WGR",
				PriceItemCode = "WGR",
				Reference1 = "900",
				Reference2 = "ClientDirect",
				Reference3 = "Client",
				Reference4 = "LONWP-SSQL-4A.wisecloud.zone/INSTANCEP1",
				Reference5 = "8788",
				ReportingSource = "MSC",
				ServiceOccuredUTC = transaction1Time,
				Version = 0,
			};

			var trn3 = new API.BillingTransaction
			{
				BillableCount = 1230,
				ClientID = "ENT???SRV",
				ClientNumber = null,
				Category = "WGR",
				PriceItemCode = "WGR",
				Reference1 = "900",
				Reference2 = "ClientDirect",
				Reference3 = "Client",
				Reference4 = "LONWP-SSQL-4A.wisecloud.zone/INSTANCEP1",
				Reference5 = "9999",
				ReportingSource = "MSC",
				ServiceOccuredUTC = transaction2Time,
				Version = 0,
			};

			var trn4 = new API.BillingTransaction
			{
				BillableCount = 345,
				ClientID = "ENT???PVR",
				ClientNumber = null,
				Category = "WGR",
				PriceItemCode = "WGR",
				Reference1 = "900",
				Reference2 = "ClientDirect",
				Reference3 = "Client",
				Reference4 = "LONWP-SSQL-5A.wisecloud.zone/INSTANCEP2",
				Reference5 = "2222",
				ReportingSource = "BLA",
				ServiceOccuredUTC = transaction2Time,
				Version = 0,
			};

			var trn5 = new API.BillingTransaction
			{
				BillableCount = 40000,
				ClientID = "ENT???SRV",
				ClientNumber = null,
				Category = "WGR",
				PriceItemCode = "WGR",
				Reference1 = "900",
				Reference2 = "ClientDirect",
				Reference3 = "Client",
				Reference4 = "LONWP-SSQL-4A.wisecloud.zone/INSTANCEP1",
				Reference5 = "1111",
				ReportingSource = "MSC",
				ServiceOccuredUTC = transaction3Time,
				Version = 0,
			};

			BillingDataTestHelper.AddTransaction(trn1);
			BillingDataTestHelper.AddTransaction(trn2);
			BillingDataTestHelper.AddTransaction(trn3);
			BillingDataTestHelper.AddTransaction(trn4);
			BillingDataTestHelper.AddTransaction(trn5);
			BillingDataTestHelper.ExecuteUpdateChargeableAtTime(con, transaction1Time);

			var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
			var stagingUnknownTable = BillingDataTestHelper.LoadTableByName(con, "edi.StagingUnknownSystems");
			var chargeableTable = BillingDataTestHelper.LoadAllChargeableOfCategory(con, "WGR");
			var stagingTable = BillingDataTestHelper.LoadTableByName(con, "dbo.Staging");

			Assert.That(usageTable.Rows.Count, Is.EqualTo(4), "Known records should be processed into Usage table");
			Assert.That(stagingUnknownTable.Rows.Count, Is.EqualTo(1), "Unknown records should be processed into StagingUnknownSystems table");
			Assert.That(stagingTable.Rows.Count, Is.EqualTo(0), "No records should remain in the staging table");
			Assert.That(chargeableTable.Rows.Count, Is.EqualTo(0), "No records should be processed into Chargeable table");

			BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn2, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, usageTable.Rows[0]);
			BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn3, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, usageTable.Rows[1]);
			BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn4, expectedDatabaseNumber: 2, expectedCompanyNumber: 2, usageTable.Rows[2]);
			BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn5, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, usageTable.Rows[3]);

			BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, daysAfter: 2);
			usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
			chargeableTable = BillingDataTestHelper.LoadAllChargeableOfCategory(con, "WGR");
			stagingTable = BillingDataTestHelper.LoadTableByName(con, "dbo.Staging");

			Assert.That(usageTable.Rows.Count, Is.EqualTo(4), "Still the same records in Usage table");
			Assert.That(stagingUnknownTable.Rows.Count, Is.EqualTo(1), "Still the same records in StagingUnknownSystems table");
			Assert.That(stagingTable.Rows.Count, Is.EqualTo(0), "Still no records in the staging table");
			Assert.That(chargeableTable.Rows.Count, Is.EqualTo(0), "No records should be processed into Chargeable table as it should only run on the first day of the month");

			BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, daysAfter: 1);
			usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
			chargeableTable = BillingDataTestHelper.LoadAllChargeableOfCategory(con, "WGR");
			stagingTable = BillingDataTestHelper.LoadTableByName(con, "dbo.Staging");

			Assert.That(usageTable.Rows.Count, Is.EqualTo(4), "Still the same records in Usage table");
			Assert.That(stagingUnknownTable.Rows.Count, Is.EqualTo(1), "Still the same records in StagingUnknownSystems table");
			Assert.That(stagingTable.Rows.Count, Is.EqualTo(0), "Still no records in the staging table");

			if (expectedBillableCount > 0)
			{
				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(1), "Records should be processed into Chargeable table");

				var chargeableRow = chargeableTable.Rows[0];

				Assert.That(chargeableRow["CH_BillableCount"], Is.EqualTo(expectedBillableCount));
				Assert.That(chargeableRow["CH_ClientID"], Is.EqualTo("ENT???SV2"));
				Assert.That(chargeableRow["CH_Category"], Is.EqualTo("WGR"));
				Assert.That(chargeableRow["CH_PriceItemCode"], Is.EqualTo("WGR"));
				Assert.That(chargeableRow["CH_Reference1"], Is.EqualTo(""));
				Assert.That(chargeableRow["CH_Reference2"], Is.EqualTo("ClientDirect"));
				Assert.That(chargeableRow["CH_Reference3"], Is.EqualTo("Client"));
				Assert.That(chargeableRow["CH_Reference4"], Is.EqualTo("LONWP-SSQL-4A.wisecloud.zone/INSTANCEP1"));
				Assert.That(chargeableRow["CH_Reference5"], Is.EqualTo("8788"));
				Assert.That(chargeableRow["CH_ReportingSource"], Is.EqualTo("MSC"));
				Assert.That(chargeableRow["CH_ServiceOccuredUTC"], Is.EqualTo(transaction1Time));
				Assert.That(chargeableRow["CH_Version"], Is.EqualTo(0));
				Assert.That(chargeableRow["CH_DatabaseNumber"], Is.EqualTo(1));
				Assert.That(chargeableRow["CH_CompanyNumber"], Is.EqualTo(1));
				Assert.That(chargeableRow["CH_Period"], Is.EqualTo(chargeablePeriod));

				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, daysAfter: 1);
				usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				chargeableTable = BillingDataTestHelper.LoadAllChargeableOfCategory(con, "WGR");
				stagingTable = BillingDataTestHelper.LoadTableByName(con, "dbo.Staging");

				Assert.That(usageTable.Rows.Count, Is.EqualTo(4), "Still the same records in Usage table");
				Assert.That(stagingUnknownTable.Rows.Count, Is.EqualTo(1), "Still the same records in StagingUnknownSystems table");
				Assert.That(stagingTable.Rows.Count, Is.EqualTo(0), "Still no records in the staging table");
				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(1), "Still only one record in the Chargeable table when aggregration is run more than once");
			}
			else
			{
				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(0), "No records should be processed into Chargeable table the charge was not above zero");
			}
		}

		[Test]
		public void TestProcessWGRSummedCoresGreaterThanSingleCore()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "SYD");

				var utcNow = DateTime.UtcNow;
				var currentYear = utcNow.Year;
				var singleEventTime = new DateTime(currentYear - 3, 6, 15);
				var multiEventTime = new DateTime(currentYear - 3, 6, 14);
				var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(singleEventTime);

				var singleHighCPUEvent = new API.BillingTransaction
				{
					BillableCount = 595,
					ClientID = "ENT???SRV",
					ClientNumber = null,
					Category = "WGR",
					PriceItemCode = "WGR",
					Reference1 = "300",
					Reference2 = "ClientDirect",
					Reference3 = "Client",
					Reference4 = "us2wp-ssql-425b.wisecloud.zone/INSTANCE1",
					Reference5 = "316751",
					ReportingSource = "MSC",
					ServiceOccuredUTC = singleEventTime,
					Version = 0,
				};

				var multiHighCPUEvent1 = new API.BillingTransaction
				{
					BillableCount = 220,
					ClientID = "ENT???SRV",
					ClientNumber = null,
					Category = "WGR",
					PriceItemCode = "WGR",
					Reference1 = "300",
					Reference2 = "GlowWriter",
					Reference3 = "System",
					Reference4 = "us2wp-ssql-425b.wisecloud.zone/INSTANCE1",
					Reference5 = "316588",
					ReportingSource = "MSC",
					ServiceOccuredUTC = multiEventTime,
					Version = 0,
				};

				var multiHighCPUEvent2 = new API.BillingTransaction
				{
					BillableCount = 587,
					ClientID = "ENT???SRV",
					ClientNumber = null,
					Category = "WGR",
					PriceItemCode = "WGR",
					Reference1 = "300",
					Reference2 = "CW1Writer",
					Reference3 = "System",
					Reference4 = "us2wp-ssql-425b.wisecloud.zone/INSTANCE1",
					Reference5 = "316588",
					ReportingSource = "MSC",
					ServiceOccuredUTC = multiEventTime,
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(singleHighCPUEvent);
				BillingDataTestHelper.AddTransaction(multiHighCPUEvent1);
				BillingDataTestHelper.AddTransaction(multiHighCPUEvent2);

				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, daysAfter: 1);

				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				var chargeableTable = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);

				Assert.That(usageTable.Rows.Count, Is.EqualTo(3), "All Records should be processed into Usage table");
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(singleHighCPUEvent, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, usageTable.Rows.Cast<DataRow>().Single(x => (int)x["US_BillableCount"] == 595));
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(multiHighCPUEvent1, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, usageTable.Rows.Cast<DataRow>().Single(x => (int)x["US_BillableCount"] == 220));
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(multiHighCPUEvent2, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, usageTable.Rows.Cast<DataRow>().Single(x => (int)x["US_BillableCount"] == 587));

				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(1), "One record should be processed into Chargeable table");
				var chargeableRow = chargeableTable.Rows[0];
				Assert.That(chargeableRow["CH_BillableCount"], Is.EqualTo(3));
				Assert.That(chargeableRow["CH_ClientID"], Is.EqualTo("ENT???SRV"));
				Assert.That(chargeableRow["CH_Category"], Is.EqualTo("WGR"));
				Assert.That(chargeableRow["CH_PriceItemCode"], Is.EqualTo("WGR"));
				Assert.That(chargeableRow["CH_Reference1"], Is.EqualTo(""));
				Assert.That(chargeableRow["CH_Reference2"], Is.EqualTo("CW1Writer"));
				Assert.That(chargeableRow["CH_Reference3"], Is.EqualTo("System"));
				Assert.That(chargeableRow["CH_Reference4"], Is.EqualTo("us2wp-ssql-425b.wisecloud.zone/INSTANCE1"));
				Assert.That(chargeableRow["CH_Reference5"], Is.EqualTo("316588"));
				Assert.That(chargeableRow["CH_ReportingSource"], Is.EqualTo("MSC"));
				Assert.That(chargeableRow["CH_ServiceOccuredUTC"], Is.EqualTo(multiEventTime));
				Assert.That(chargeableRow["CH_Version"], Is.EqualTo(0));
				Assert.That(chargeableRow["CH_DatabaseNumber"], Is.EqualTo(1));
				Assert.That(chargeableRow["CH_CompanyNumber"], Is.EqualTo(1));
				Assert.That(chargeableRow["CH_Period"], Is.EqualTo(chargeablePeriod));
			}
		}

		[Test]
		public void TestProduceOldWareHousePackagingMonthlyChargeables()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "SYD");

				var utcNow = DateTime.UtcNow;
				var transaction1Time = utcNow.AddMonths(-1);
				var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(transaction1Time);

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENT???SRV",
					ClientNumber = null,
					Category = "STL",
					PriceItemCode = "WTP",
					Reference1 = "tdv16032-2",
					Reference2 = "RC00000021",
					Reference3 = "",
					Reference4 = "",
					Reference5 = "F90580FD-4CA4-4410-A5C6-13901AA54878",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transaction1Time,
					Version = 0,
				};

				var trn2 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENT???SRV",
					ClientNumber = null,
					Category = "STL",
					PriceItemCode = "WTP",
					Reference1 = "S00001113-5",
					Reference2 = "RC00000252",
					Reference3 = "",
					Reference4 = "",
					Reference5 = "06B1B442-9802-4B4A-9767-8AE5BF056C6C",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transaction1Time,
					Version = 0,
				};

				var trn3 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENT???SRV",
					ClientNumber = null,
					Category = "STL",
					PriceItemCode = "WTU",
					Reference1 = "S00001113-5",
					Reference2 = "RC00000252",
					Reference3 = "D9E63C52-33C5-41ED-A9EB-CA9D096E87E5",
					Reference4 = "1",
					Reference5 = "06B1B442-9802-4B4A-9767-8AE5BF056C6C",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transaction1Time,
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trn1);
				BillingDataTestHelper.AddTransaction(trn2);
				BillingDataTestHelper.AddTransaction(trn3);

				BillingDataTestHelper.ExecuteUpdateChargeableAtTime(con, transaction1Time);

				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				var chargeableTable = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				var stagingTable = BillingDataTestHelper.LoadTableByName(con, "dbo.Staging");

				Assert.That(usageTable.Rows.Count, Is.EqualTo(2), "Records should only be processed into Usage table");
				Assert.That(stagingTable.Rows.Count, Is.EqualTo(0), "No records should remain in the staging table");
				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(1), "One should be processed into Chargeable table");

				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn2, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, usageTable.Rows[0]);
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn1, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, usageTable.Rows[1]);

				var chargeableRow1 = chargeableTable.Rows[0];
				Assert.That(chargeableRow1["CH_BillableCount"], Is.EqualTo(1));
				Assert.That(chargeableRow1["CH_ClientID"], Is.EqualTo("ENT???SRV"));
				Assert.That(chargeableRow1["CH_Category"], Is.EqualTo("STL"));
				Assert.That(chargeableRow1["CH_PriceItemCode"], Is.EqualTo("WTU"));
				Assert.That(chargeableRow1["CH_Reference1"], Is.EqualTo("S00001113-5"));
				Assert.That(chargeableRow1["CH_Reference2"], Is.EqualTo("RC00000252"));
				Assert.That(chargeableRow1["CH_Reference3"], Is.EqualTo("D9E63C52-33C5-41ED-A9EB-CA9D096E87E5"));
				Assert.That(chargeableRow1["CH_Reference4"], Is.EqualTo("1"));
				Assert.That(chargeableRow1["CH_Reference5"], Is.EqualTo("06B1B442-9802-4B4A-9767-8AE5BF056C6C"));
				Assert.That(chargeableRow1["CH_ReportingSource"], Is.EqualTo("ENT"));
				Assert.That(chargeableRow1["CH_ServiceOccuredUTC"], Is.EqualTo(transaction1Time));
				Assert.That(chargeableRow1["CH_Version"], Is.EqualTo(0));
				Assert.That(chargeableRow1["CH_DatabaseNumber"], Is.EqualTo(1));
				Assert.That(chargeableRow1["CH_CompanyNumber"], Is.EqualTo(1));
				Assert.That(chargeableRow1["CH_Period"], Is.EqualTo(chargeablePeriod));

				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, daysAfter: 2);
				usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				chargeableTable = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				stagingTable = BillingDataTestHelper.LoadTableByName(con, "dbo.Staging");

				Assert.That(usageTable.Rows.Count, Is.EqualTo(2), "Still the same records in Usage table");
				Assert.That(stagingTable.Rows.Count, Is.EqualTo(0), "Still no records in the staging table");
				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(1), "Only the WTU record in the Chargeable table as WTP should only be moved on the first day of the month");

				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, daysAfter: 1);
				usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				chargeableTable = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				stagingTable = BillingDataTestHelper.LoadTableByName(con, "dbo.Staging");

				Assert.That(usageTable.Rows.Count, Is.EqualTo(2), "Still the same records in Usage table");
				Assert.That(stagingTable.Rows.Count, Is.EqualTo(0), "Still no records in the staging table");
				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(2), "Records should be processed into Chargeable table");

				var chargeableRow2 = chargeableTable.Rows[0];
				Assert.That(chargeableRow2["CH_BillableCount"], Is.EqualTo(1));
				Assert.That(chargeableRow2["CH_ClientID"], Is.EqualTo("ENT???SRV"));
				Assert.That(chargeableRow2["CH_Category"], Is.EqualTo("STL"));
				Assert.That(chargeableRow2["CH_PriceItemCode"], Is.EqualTo("WTP"));
				Assert.That(chargeableRow2["CH_Reference1"], Is.EqualTo("tdv16032-2"));
				Assert.That(chargeableRow2["CH_Reference2"], Is.EqualTo("RC00000021"));
				Assert.That(chargeableRow2["CH_Reference3"], Is.EqualTo(""));
				Assert.That(chargeableRow2["CH_Reference4"], Is.EqualTo(""));
				Assert.That(chargeableRow2["CH_Reference5"], Is.EqualTo("F90580FD-4CA4-4410-A5C6-13901AA54878"));
				Assert.That(chargeableRow2["CH_ReportingSource"], Is.EqualTo("ENT"));
				Assert.That(chargeableRow2["CH_ServiceOccuredUTC"], Is.EqualTo(transaction1Time));
				Assert.That(chargeableRow2["CH_Version"], Is.EqualTo(0));
				Assert.That(chargeableRow2["CH_DatabaseNumber"], Is.EqualTo(1));
				Assert.That(chargeableRow2["CH_CompanyNumber"], Is.EqualTo(1));
				Assert.That(chargeableRow2["CH_Period"], Is.EqualTo(chargeablePeriod));
			}
		}

		[Test]
		public void TestAccountsPayableIntoMonthlyChargeables()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				AssertAccountsPayableIntoMonthlyChargeables(con, "CN", "TX2");
				AssertAccountsPayableIntoMonthlyChargeables(con, "ES", "TX4");
				AssertAccountsPayableIntoMonthlyChargeables(con, "TR", "TX4");
			}
		}

		void AssertAccountsPayableIntoMonthlyChargeables(SqlConnection con, string countryCode, string expectedPriceItemCode)
		{
			BillingDataTestHelper.TruncateStagingTable(con);
			BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
			BillingDataTestHelper.TruncateTable(con, "edi.Usage");
			BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
			BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "MEL", serverCode: "MEL", hostedLocation: "SYD");

			var utcNow = DateTime.UtcNow;
			var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(utcNow.AddMonths(-1));
			var transaction1Time = new DateTime(chargeablePeriod / 100, chargeablePeriod % 100, 1, 10, 33, 23);

			var trn1 = new API.BillingTransaction
			{
				BillableCount = 2,
				ClientID = "ENTMELMEL",
				ClientNumber = null,
				Category = "STL",
				PriceItemCode = "TX2",
				Reference1 = "43518651",
				Reference2 = "8600284806",
				Reference3 = countryCode,
				Reference4 = "INV",
				Reference5 = "2DA42803-3D2B-4B81-8A0B-0D626E339FF7",
				ReportingSource = "ENT",
				ServiceOccuredUTC = transaction1Time,
				Version = 0,
			};

			BillingDataTestHelper.AddTransaction(trn1);

			BillingDataTestHelper.ExecuteUpdateChargeableAtTime(con, transaction1Time);

			var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
			var chargeableTable = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
			var stagingTable = BillingDataTestHelper.LoadTableByName(con, "dbo.Staging");

			Assert.That(usageTable.Rows.Count, Is.EqualTo(1), "Records should only be processed into Usage table");
			Assert.That(stagingTable.Rows.Count, Is.EqualTo(0), "No records should remain in the staging table");
			Assert.That(chargeableTable.Rows.Count, Is.EqualTo(0), "No records in Chargeable table yet");

			BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn1, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, usageTable.Rows[0]);
			BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, daysAfter: 2);
			usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
			chargeableTable = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
			stagingTable = BillingDataTestHelper.LoadTableByName(con, "dbo.Staging");

			Assert.That(usageTable.Rows.Count, Is.EqualTo(1), "Still the same records in Usage table");
			Assert.That(stagingTable.Rows.Count, Is.EqualTo(0), "Still no records in the staging table");
			Assert.That(chargeableTable.Rows.Count, Is.EqualTo(0), "No records in Chargeable table yet");

			BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, daysAfter: 1);
			usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
			chargeableTable = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
			stagingTable = BillingDataTestHelper.LoadTableByName(con, "dbo.Staging");

			Assert.That(usageTable.Rows.Count, Is.EqualTo(1), "Still the same records in Usage table");
			Assert.That(stagingTable.Rows.Count, Is.EqualTo(0), "Still no records in the staging table");
			Assert.That(chargeableTable.Rows.Count, Is.EqualTo(1), "Records have been moved to chargeable table");

			var chargeableRow1 = chargeableTable.Rows[0];
			Assert.That(chargeableRow1["CH_BillableCount"], Is.EqualTo(1));
			Assert.That(chargeableRow1["CH_ClientID"], Is.EqualTo("ENTMELMEL"));
			Assert.That(chargeableRow1["CH_Category"], Is.EqualTo("STL"));
			Assert.That(chargeableRow1["CH_PriceItemCode"], Is.EqualTo(expectedPriceItemCode));
			Assert.That(chargeableRow1["CH_Reference1"], Is.EqualTo("43518651"));
			Assert.That(chargeableRow1["CH_Reference2"], Is.EqualTo("8600284806"));
			Assert.That(chargeableRow1["CH_Reference3"], Is.EqualTo(countryCode));
			Assert.That(chargeableRow1["CH_Reference4"], Is.EqualTo("INV"));
			Assert.That(chargeableRow1["CH_Reference5"], Is.EqualTo("2DA42803-3D2B-4B81-8A0B-0D626E339FF7"));
			Assert.That(chargeableRow1["CH_ReportingSource"], Is.EqualTo("ENT"));
			Assert.That(chargeableRow1["CH_ServiceOccuredUTC"], Is.EqualTo(transaction1Time));
			Assert.That(chargeableRow1["CH_Version"], Is.EqualTo(0));
			Assert.That(chargeableRow1["CH_DatabaseNumber"], Is.EqualTo(1));
			Assert.That(chargeableRow1["CH_CompanyNumber"], Is.EqualTo(1));
			Assert.That(chargeableRow1["CH_Period"], Is.EqualTo(chargeablePeriod));
		}

		[Test]
		public void TestAccountsPayableDuplicatesRemoved()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "MEL", serverCode: "MEL", hostedLocation: "SYD");

				var utcNow = DateTime.UtcNow;
				var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(utcNow.AddMonths(-1));
				var transaction1Time = new DateTime(chargeablePeriod / 100, chargeablePeriod % 100, 1, 10, 33, 23);

				var trn1 = new API.BillingTransaction
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
					ServiceOccuredUTC = transaction1Time,
					Version = 0,
				};

				var trn2 = new API.BillingTransaction
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
					ServiceOccuredUTC = transaction1Time.AddMinutes(20),
					Version = 0,
				};


				BillingDataTestHelper.AddTransaction(trn1);
				BillingDataTestHelper.AddTransaction(trn2);

				BillingDataTestHelper.ExecuteUpdateChargeableAtTime(con, transaction1Time);

				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				var chargeableTable = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				var stagingTable = BillingDataTestHelper.LoadTableByName(con, "dbo.Staging");

				Assert.That(usageTable.Rows.Count, Is.EqualTo(2), "Records should only be processed into Usage table");
				Assert.That(stagingTable.Rows.Count, Is.EqualTo(0), "No records should remain in the staging table");
				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(0), "No records in Chargeable table yet");

				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn1, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, usageTable.Rows[0]);
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn2, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, usageTable.Rows[1]);
				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, daysAfter: 2);
				usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				chargeableTable = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				stagingTable = BillingDataTestHelper.LoadTableByName(con, "dbo.Staging");

				Assert.That(usageTable.Rows.Count, Is.EqualTo(2), "Still the same records in Usage table");
				Assert.That(stagingTable.Rows.Count, Is.EqualTo(0), "Still no records in the staging table");
				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(0), "No records in Chargeable table yet");

				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, daysAfter: 1);
				usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				chargeableTable = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				stagingTable = BillingDataTestHelper.LoadTableByName(con, "dbo.Staging");

				Assert.That(usageTable.Rows.Count, Is.EqualTo(2), "Still the same records in Usage table");
				Assert.That(stagingTable.Rows.Count, Is.EqualTo(0), "Still no records in the staging table");
				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(1), "Records have been moved to chargeable table");

				var chargeableRow1 = chargeableTable.Rows[0];
				Assert.That(chargeableRow1["CH_BillableCount"], Is.EqualTo(1));
				Assert.That(chargeableRow1["CH_ClientID"], Is.EqualTo("ENTMELMEL"));
				Assert.That(chargeableRow1["CH_Category"], Is.EqualTo("STL"));
				Assert.That(chargeableRow1["CH_PriceItemCode"], Is.EqualTo("TX2"));
				Assert.That(chargeableRow1["CH_Reference1"], Is.EqualTo("43518651"));
				Assert.That(chargeableRow1["CH_Reference2"], Is.EqualTo("8600284806"));
				Assert.That(chargeableRow1["CH_Reference3"], Is.EqualTo("CN"));
				Assert.That(chargeableRow1["CH_Reference4"], Is.EqualTo("INV"));
				Assert.That(chargeableRow1["CH_Reference5"], Is.EqualTo("2DA42803-3D2B-4B81-8A0B-0D626E339FF7"));
				Assert.That(chargeableRow1["CH_ReportingSource"], Is.EqualTo("ENT"));
				Assert.That(chargeableRow1["CH_ServiceOccuredUTC"], Is.EqualTo(transaction1Time));
				Assert.That(chargeableRow1["CH_Version"], Is.EqualTo(0));
				Assert.That(chargeableRow1["CH_DatabaseNumber"], Is.EqualTo(1));
				Assert.That(chargeableRow1["CH_CompanyNumber"], Is.EqualTo(1));
				Assert.That(chargeableRow1["CH_Period"], Is.EqualTo(chargeablePeriod));
			}
		}

		[Test]
		public void TestAccountsReceivableIntoMonthlyChargeables()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "MEL", serverCode: "MEL", hostedLocation: "SYD");
				AssertAccountsReceivableIntoMonthlyChargeables(con, "HU", "TX3");
				AssertAccountsReceivableIntoMonthlyChargeables(con, "IT", "TX3");
				AssertAccountsReceivableIntoMonthlyChargeables(con, "IN", "TX3");
				AssertAccountsReceivableIntoMonthlyChargeables(con, "WS", "TX3");
				AssertAccountsReceivableIntoMonthlyChargeables(con, "VN", "TX3");
				AssertAccountsReceivableIntoMonthlyChargeables(con, "TW", "TX3");
				AssertAccountsReceivableIntoMonthlyChargeables(con, "FJ", "TX3");
				AssertAccountsReceivableIntoMonthlyChargeables(con, "EG", "TX3");
				AssertAccountsReceivableIntoMonthlyChargeables(con, "US", "TX1");
				AssertAccountsReceivableIntoMonthlyChargeables(con, "CN", "TX3");
				AssertAccountsReceivableIntoMonthlyChargeables(con, "SA", "TX3");
				AssertAccountsReceivableIntoMonthlyChargeables(con, "RO", "TX3");
				AssertAccountsReceivableIntoMonthlyChargeables(con, "AR", "TX3");
			}
		}

		void AssertAccountsReceivableIntoMonthlyChargeables(SqlConnection con, string countryCode, string expectedPriceItemCode)
		{
			BillingDataTestHelper.TruncateStagingTable(con);
			BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
			BillingDataTestHelper.TruncateTable(con, "edi.Usage");

			var utcNow = DateTime.UtcNow;
			var transaction1Time = utcNow.AddMonths(-1);
			var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(transaction1Time);

			var trn1 = new API.BillingTransaction
			{
				BillableCount = 1,
				ClientID = "ENTMELMEL",
				ClientNumber = null,
				Category = "STL",
				PriceItemCode = "TX1",
				Reference1 = "1000672075",
				Reference2 = "S22AATL0363233",
				Reference3 = countryCode,
				Reference4 = "INV",
				Reference5 = "9557C17A-0AE5-4C66-8D4C-52B28462B92D",
				ReportingSource = "ENT",
				ServiceOccuredUTC = transaction1Time,
				Version = 0,
			};

			BillingDataTestHelper.AddTransaction(trn1);
			BillingDataTestHelper.ExecuteUpdateChargeableAtTime(con, transaction1Time);

			var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
			var chargeableTable = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
			var stagingTable = BillingDataTestHelper.LoadTableByName(con, "dbo.Staging");

			Assert.That(usageTable.Rows.Count, Is.EqualTo(1), "Records should only be processed into Usage table");
			Assert.That(stagingTable.Rows.Count, Is.EqualTo(0), "No records should remain in the staging table");
			Assert.That(chargeableTable.Rows.Count, Is.EqualTo(0), "No records in Chargeable table yet");

			BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn1, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, usageTable.Rows[0]);

			BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, daysAfter: 2);
			usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
			chargeableTable = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
			stagingTable = BillingDataTestHelper.LoadTableByName(con, "dbo.Staging");

			Assert.That(usageTable.Rows.Count, Is.EqualTo(1), "Still the same records in Usage table");
			Assert.That(stagingTable.Rows.Count, Is.EqualTo(0), "Still no records in the staging table");
			Assert.That(chargeableTable.Rows.Count, Is.EqualTo(0), "No records in Chargeable table yet");

			BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, daysAfter: 1);
			usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
			chargeableTable = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
			stagingTable = BillingDataTestHelper.LoadTableByName(con, "dbo.Staging");

			Assert.That(usageTable.Rows.Count, Is.EqualTo(1), "Still the same records in Usage table");
			Assert.That(stagingTable.Rows.Count, Is.EqualTo(0), "Still no records in the staging table");
			Assert.That(chargeableTable.Rows.Count, Is.EqualTo(1), "All records moved to Chargeable");

			var chargeableRow1 = chargeableTable.Rows[0];
			Assert.That(chargeableRow1["CH_BillableCount"], Is.EqualTo(1));
			Assert.That(chargeableRow1["CH_ClientID"], Is.EqualTo("ENTMELMEL"));
			Assert.That(chargeableRow1["CH_Category"], Is.EqualTo("STL"));
			Assert.That(chargeableRow1["CH_PriceItemCode"], Is.EqualTo(expectedPriceItemCode));
			Assert.That(chargeableRow1["CH_Reference1"], Is.EqualTo("1000672075"));
			Assert.That(chargeableRow1["CH_Reference2"], Is.EqualTo("S22AATL0363233"));
			Assert.That(chargeableRow1["CH_Reference3"], Is.EqualTo(countryCode));
			Assert.That(chargeableRow1["CH_Reference4"], Is.EqualTo("INV"));
			Assert.That(chargeableRow1["CH_Reference5"], Is.EqualTo("9557C17A-0AE5-4C66-8D4C-52B28462B92D"));
			Assert.That(chargeableRow1["CH_ReportingSource"], Is.EqualTo("ENT"));
			Assert.That(chargeableRow1["CH_ServiceOccuredUTC"], Is.EqualTo(transaction1Time));
			Assert.That(chargeableRow1["CH_Version"], Is.EqualTo(0));
			Assert.That(chargeableRow1["CH_DatabaseNumber"], Is.EqualTo(1));
			Assert.That(chargeableRow1["CH_CompanyNumber"], Is.EqualTo(1));
			Assert.That(chargeableRow1["CH_Period"], Is.EqualTo(chargeablePeriod));
		}

		[Test]
		public void TestDisableMonthlyAggregation()
		{
			AssertDisableMonthlyAggregation(performMonthlyAggregation: false, isTestServer: false);
		}

		[Test]
		public void TestDisableMonthlyAggregationOnTestServer()
		{
			AssertDisableMonthlyAggregation(performMonthlyAggregation: true, isTestServer: true);
		}

		void AssertDisableMonthlyAggregation(bool performMonthlyAggregation, bool isTestServer)
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "MEL", serverCode: "MEL", hostedLocation: "SYD");

				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");

				var utcNow = DateTime.UtcNow;
				var transaction1Time = utcNow.AddMonths(-1);
				var chargeablePeriod = BillingDataTestHelper.GetChargeablePeriod(transaction1Time);

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ENTMELMEL",
					ClientNumber = null,
					Category = "STL",
					PriceItemCode = "TX1",
					Reference1 = "1000672075",
					Reference2 = "S22AATL0363233",
					Reference3 = "HU",
					Reference4 = "INV",
					Reference5 = "9557C17A-0AE5-4C66-8D4C-52B28462B92D",
					ReportingSource = "ENT",
					ServiceOccuredUTC = transaction1Time,
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trn1);

				BillingDataTestHelper.ExecuteUpdateChargeableAfterPeriod(con, chargeablePeriod, daysAfter: 1, performMonthlyAggregation: performMonthlyAggregation, isTestServer: isTestServer);
				var usageTable = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				var chargeableTable = BillingDataTestHelper.LoadAllChargeableMostRecentFirst(con);
				var stagingTable = BillingDataTestHelper.LoadTableByName(con, "dbo.Staging");

				Assert.That(usageTable.Rows.Count, Is.EqualTo(1), "Record was moved to Usage table");
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn1, expectedDatabaseNumber: 1, expectedCompanyNumber: 1, usageTable.Rows[0]);
				Assert.That(stagingTable.Rows.Count, Is.EqualTo(0), "No records in the staging table");
				Assert.That(chargeableTable.Rows.Count, Is.EqualTo(0), "No records in chargeable as monthly aggregation is disabled");
			}
		}

		[SetUp]
		public void SetUp()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.SetUpBillingRules(con);
			}
		}
	}
}

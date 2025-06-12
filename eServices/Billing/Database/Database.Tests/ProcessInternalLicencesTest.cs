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
	public class ProcessInternalLicencesTest
	{
		[Test]
		public void TestProcessInternalLicences()
		{
			using(var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.ExecuteNonQuery(con, @"
INSERT edi.InternalLicenceDatabaseCodeHistory(SystemID, DatabaseNumber, EnterpriseCode, ServerCode, ValidFromUtc, Category, TenantID, Product)
VALUES
('C', 1, 'ENT', 'SRV', '2024-12-01', 'SMF', 'ABCDEFXYZ', ''),
('D', 2, 'ENT', 'SRV', '2024-12-01', '', 'JEFLNDSHK', 'SHP')");

				BillingDataTestHelper.ExecuteNonQuery(con, @"
INSERT edi.StagingBatch(TX_ID, TX_Category, TX_PriceItemCode, TX_BillableCount, TX_ReportingSource, TX_ServiceOccuredUTC, TX_ClientID, TX_Reference1, TX_ClientNumber)
VALUES
(1, 'SMF', 'WGR', 1, 'HUB', '2025-01-13', 'RTYUIOPLK', 'Category matches category', 'ABCDEFXYZ'),
(2, 'SHP', 'WGR', 1, 'HUB', '2025-01-13', 'EDEDEDEDE', 'Category matches product', 'JEFLNDSHK')");

				BillingDataTestHelper.ExecuteNonQuery(con, "exec edi.ProcessInternalLicences 1");

				var table = BillingDataTestHelper.LoadTableByName(con, "edi.StagingBatch");
				Assert.That(table.Rows.Count, Is.EqualTo(2));
				var row = table.Rows[0];
				Assert.That(row["ProcessingStatus"], Is.EqualTo(255));
				var row2 = table.Rows[1];
				Assert.That(row2["ProcessingStatus"], Is.EqualTo(255));
			}
		}

		[Test]
		public void TestProcessInternalLicences_ShouldntProcessCases()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.ExecuteNonQuery(con, @"
INSERT edi.InternalLicenceDatabaseCodeHistory(SystemID, DatabaseNumber, EnterpriseCode, ServerCode, ValidFromUtc, Category, TenantID, Product)
VALUES
('C', 1, 'ENT', 'SRV', '2024-12-01', 'SMF', 'JEFLNDSHK', 'BBL'),
('D', 2, 'ENT', 'SRV', '2024-12-01', '', 'NOMNOMNOM', 'SHP'),
('D', 2, 'ENT', 'SRV', '2024-12-01', 'YAR', 'YOTUJKCVF', 'ANY')");

				BillingDataTestHelper.ExecuteNonQuery(con, @"
INSERT edi.StagingBatch(TX_ID, TX_Category, TX_PriceItemCode, TX_BillableCount, TX_ReportingSource, TX_ServiceOccuredUTC, TX_ClientID, TX_Reference1)
VALUES
(1, 'SMF', 'WGR', 1, 'HUB', '2025-01-13', 'DIFFERENT', 'No Matching ClientID to TenantID'),
(2, 'DIF', 'WGR', 1, 'HUB', '2025-01-13', 'JEFLNDSHK', 'No Matching Category'),
(4, 'DIF', 'WGR', 1, 'HUB', '2025-01-13', 'NOMNOMNOM', 'No Matching Category to Product'),
(4, 'YAR', 'WGR', 1, 'HUB', '2025-01-13', 'YOTUJKCVF', 'ClientNumber Is Not TenantID')");

				BillingDataTestHelper.ExecuteNonQuery(con, "exec edi.ProcessInternalLicences 1");

				var table = BillingDataTestHelper.LoadTableByName(con, "edi.StagingBatch");
				Assert.That(table.Rows.Count, Is.EqualTo(4));
				var row = table.Rows[0];
				Assert.That(row["ProcessingStatus"], Is.EqualTo(0));
				var row2 = table.Rows[1];
				Assert.That(row2["ProcessingStatus"], Is.EqualTo(0));
				var row3 = table.Rows[2];
				Assert.That(row3["ProcessingStatus"], Is.EqualTo(0));
				var row4 = table.Rows[3];
				Assert.That(row4["ProcessingStatus"], Is.EqualTo(0));
			}
		}

		[Test]
		public void TestProcessStagingInternalLicences()
		{
			using(var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.AddDatabaseAndCompany(con, 22, 2, "ERL", enterpriseCode: "EXT", serverCode: "MAL");
				BillingDataTestHelper.ExecuteNonQuery(con, @"
INSERT edi.InternalLicenceDatabaseCodeHistory (SystemId, DatabaseNumber, EnterpriseCode, ServerCode, ValidFromUtc, TenantID, Category, Product)
VALUES
('C', 11, 'INT', 'AL1', '2025-01-13', 'INTERNAL1', 'SMF', ''),
('D', 33, 'AAA', 'AAA', '2025-01-13', 'ANOTHRINT', '', 'SMF')");

				var trn1 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "INTERNAL5",
					ClientNumber = "INTERNAL1",
					Category = "SMF",
					PriceItemCode = "ANY",
					Reference1 = "This is internal",
					ReportingSource = "MSC",
					ServiceOccuredUTC = new DateTime(DateTime.Now.Year - 3, 6, 20),
					Version = 0,
				};

				var trn2 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "EXTERLMAL",
					ClientNumber = "",
					Category = "SMF",
					PriceItemCode = "ANY",
					Reference1 = "This is not internal",
					ReportingSource = "MSC",
					ServiceOccuredUTC = new DateTime(DateTime.Now.Year - 3, 6, 20),
					Version = 0,
				};

				var trn3 = new API.BillingTransaction
				{
					BillableCount = 1,
					ClientID = "THEANDTHE",
					ClientNumber = "ANOTHRINT",
					Category = "SMF",
					PriceItemCode = "ANY",
					Reference1 = "This is also internal",
					ReportingSource = "MSC",
					ServiceOccuredUTC = new DateTime(DateTime.Now.Year - 3, 6, 20),
					Version = 0,
				};

				BillingDataTestHelper.AddTransaction(trn1);
				BillingDataTestHelper.AddTransaction(trn2);
				BillingDataTestHelper.AddTransaction(trn3);

				ExecuteProcessStaging(con);

				var usage = BillingDataTestHelper.LoadTableByName(con, "edi.Usage");
				var charge = BillingDataTestHelper.LoadTableByName(con, "edi.Chargeable");
				var unkown = BillingDataTestHelper.LoadTableByName(con, "edi.StagingUnknownSystems");
				Assert.That(usage.Rows.Count, Is.EqualTo(2));
				Assert.That(charge.Rows.Count, Is.EqualTo(1));
				Assert.That(unkown.Rows.Count, Is.EqualTo(0));

				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn1, 11, 0, usage.Rows.Cast<DataRow>().Single(x => (string)x["US_ClientID"] == "INTERNAL5"));
				BillingDataTestHelper.AssertUsagePropertiesAreEqual(trn3, 33, 0, usage.Rows.Cast<DataRow>().Single(x => (string)x["US_ClientID"] == "THEANDTHE"));
			}
		}

		public void ExecuteProcessStaging(SqlConnection con, bool isTestServer = false)
		{
			using (var cmd = con.CreateCommand())
			{
				cmd.CommandText = "edi.ProcessStaging";
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@isTestServer", SqlDbType.Bit).Value = isTestServer;
				cmd.ExecuteNonQuery();
			}
		}

		[SetUp]
		public void SetUp()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.SetUpBillingRules(con);
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.TruncateTable(con, "edi.StagingUnknownSystems");
				BillingDataTestHelper.TruncateTable(con, "edi.ClientCompanyEdiProdCache");
				BillingDataTestHelper.TruncateTable(con, "edi.LicenceDatabaseEdiProdCache");
				BillingDataTestHelper.TruncateTable(con, "edi.InternalLicenceDatabaseCodeHistory");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
			}
		}

	}
}

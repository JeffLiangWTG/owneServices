using System;
using System.Data;
using System.Linq;
using CargoWise.eServices.Billing.Tests.Common;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Database.Tests
{
	[TestFixture]
	public class LicenceCodeTest
	{
		[Test]
		public void TestLicenceDatabaseSync()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);

				BillingDataTestHelper.ExecuteNonQuery(con,
					@"insert edi.LicenceDatabaseEdiProdCache(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, SystemCreateUTC, HostedLocation)
						values('C', 1, 'ENT', 'SYD', '2016-08-01', 'SYD');");

				BillingDataTestHelper.ExecuteStoredProcedure(con, "edi.LicenceDatabaseSync");

				// Change server code SYD -> MEL
				BillingDataTestHelper.ExecuteNonQuery(con,
					@"insert edi.LicenceDatabaseEdiProdCache(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, SystemCreateUTC, HostedLocation)
						values('C', 1, 'ENT', 'MEL', '2016-08-02', 'SYD');");

				BillingDataTestHelper.ExecuteStoredProcedure(con, "edi.LicenceDatabaseSync");

				// Add new server with code SYD
				BillingDataTestHelper.ExecuteNonQuery(con,
					@"insert edi.LicenceDatabaseEdiProdCache(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, SystemCreateUTC, HostedLocation)
						values('C', 1, 'ENT', 'MEL', '2016-08-03', 'SYD'),
						      ('D', 2, 'ENT', 'SYD', '2016-08-04', 'NCW');");

				BillingDataTestHelper.ExecuteStoredProcedure(con, "edi.LicenceDatabaseSync");

				var table = BillingDataTestHelper.LoadTable(con, "select * from edi.LicenceDatabaseCodeHistory order by ValidFromUtc");
				var prodTable = BillingDataTestHelper.LoadTable(con, "select * from edi.LicenceDatabaseEdiProdCache");

				Assert.That(table.Rows.Count, Is.EqualTo(3), "combination count");
				var row1 = table.Rows[0];
				Assert.That(row1["DatabaseNumber"], Is.EqualTo(1));
				Assert.That(row1["EnterpriseCode"], Is.EqualTo("ENT"));
				Assert.That(row1["ServerCode"], Is.EqualTo("SYD"));
				Assert.That(row1["ValidFromUtc"], Is.EqualTo(new DateTime(2016, 8, 1)));
				Assert.That(row1["ValidToUtc"], Is.EqualTo(new DateTime(2016, 8, 4)));
				Assert.That(row1["HostedLocation"], Is.EqualTo("SYD"));

				var row2 = table.Rows[1];
				Assert.That(row2["DatabaseNumber"], Is.EqualTo(1));
				Assert.That(row2["EnterpriseCode"], Is.EqualTo("ENT"));
				Assert.That(row2["ServerCode"], Is.EqualTo("MEL"));
				Assert.That(row2["ValidFromUtc"], Is.EqualTo(new DateTime(2016, 8, 2)));
				Assert.That(row2["ValidToUtc"], Is.EqualTo(DBNull.Value));
				Assert.That(row2["HostedLocation"], Is.EqualTo("SYD"));

				var row3 = table.Rows[2];
				Assert.That(row3["DatabaseNumber"], Is.EqualTo(2));
				Assert.That(row3["EnterpriseCode"], Is.EqualTo("ENT"));
				Assert.That(row3["ServerCode"], Is.EqualTo("SYD"));
				Assert.That(row3["ValidFromUtc"], Is.EqualTo(new DateTime(2016, 8, 4)));
				Assert.That(row3["ValidToUtc"], Is.EqualTo(DBNull.Value));
				Assert.That(row3["HostedLocation"], Is.EqualTo("NCW"));

				Assert.That(prodTable.Rows.Count, Is.EqualTo(0), "cache rows");
			}
		}

		[Test]
		public void TestLicenceDatabaseSyncHostedLocation()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);

				BillingDataTestHelper.ExecuteNonQuery(con,
					@"insert edi.LicenceDatabaseEdiProdCache(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, SystemCreateUTC, HostedLocation)
						values('C', 1, 'ENT', 'SYD', '2016-08-01', 'SYD');");

				BillingDataTestHelper.ExecuteStoredProcedure(con, "edi.LicenceDatabaseSync");

				// No change
				BillingDataTestHelper.ExecuteNonQuery(con,
					@"insert edi.LicenceDatabaseEdiProdCache(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, SystemCreateUTC, HostedLocation)
						values('C', 1, 'ENT', 'SYD', '2016-08-02', 'SYD');");

				BillingDataTestHelper.ExecuteStoredProcedure(con, "edi.LicenceDatabaseSync");

				// Change hosted location SYD => NCW
				BillingDataTestHelper.ExecuteNonQuery(con,
					@"insert edi.LicenceDatabaseEdiProdCache(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, SystemCreateUTC, HostedLocation)
						values('C', 1, 'ENT', 'SYD', '2016-08-03', 'NCW');");

				BillingDataTestHelper.ExecuteStoredProcedure(con, "edi.LicenceDatabaseSync");

				var table = BillingDataTestHelper.LoadTable(con, "select * from edi.LicenceDatabaseCodeHistory order by ValidFromUtc");
				var prodTable = BillingDataTestHelper.LoadTable(con, "select * from edi.LicenceDatabaseEdiProdCache");

				Assert.That(table.Rows.Count, Is.EqualTo(2), "combination count");
				var row1 = table.Rows[0];
				Assert.That(row1["DatabaseNumber"], Is.EqualTo(1));
				Assert.That(row1["EnterpriseCode"], Is.EqualTo("ENT"));
				Assert.That(row1["ServerCode"], Is.EqualTo("SYD"));
				Assert.That(row1["ValidFromUtc"], Is.EqualTo(new DateTime(2016, 8, 1)));
				Assert.That(row1["ValidToUtc"], Is.EqualTo(new DateTime(2016, 8, 3)));
				Assert.That(row1["HostedLocation"], Is.EqualTo("SYD"));

				var row2 = table.Rows[1];
				Assert.That(row2["DatabaseNumber"], Is.EqualTo(1));
				Assert.That(row2["EnterpriseCode"], Is.EqualTo("ENT"));
				Assert.That(row2["ServerCode"], Is.EqualTo("SYD"));
				Assert.That(row2["ValidFromUtc"], Is.EqualTo(new DateTime(2016, 8, 3)));
				Assert.That(row2["ValidToUtc"], Is.EqualTo(DBNull.Value));
				Assert.That(row2["HostedLocation"], Is.EqualTo("NCW"));

				Assert.That(prodTable.Rows.Count, Is.EqualTo(0), "cache rows");
			}
		}

		[Test]
		public void TestLicenceDatabaseSyncProduct()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);

				BillingDataTestHelper.ExecuteNonQuery(con,
					@"insert edi.LicenceDatabaseEdiProdCache(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, SystemCreateUTC, HostedLocation, Product, TenantId)
						values('C', 1, 'ENT', 'SYD', '2016-08-01', 'SYD', 'SMF', 'SomeTenantId1');");

				BillingDataTestHelper.ExecuteStoredProcedure(con, "edi.LicenceDatabaseSync");

				// No change
				BillingDataTestHelper.ExecuteNonQuery(con,
					@"insert edi.LicenceDatabaseEdiProdCache(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, SystemCreateUTC, HostedLocation, Product, TenantId)
						values('C', 1, 'ENT', 'SYD', '2016-08-02', 'SYD', 'SMF', 'SomeTenantId1');");

				BillingDataTestHelper.ExecuteStoredProcedure(con, "edi.LicenceDatabaseSync");

				// Change product SMF => WTA
				BillingDataTestHelper.ExecuteNonQuery(con,
					@"insert edi.LicenceDatabaseEdiProdCache(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, SystemCreateUTC, HostedLocation, Product, TenantId)
						values('C', 1, 'ENT', 'SYD', '2016-08-03', 'SYD', 'WTA', 'SomeTenantId1');");

				BillingDataTestHelper.ExecuteStoredProcedure(con, "edi.LicenceDatabaseSync");

				var table = BillingDataTestHelper.LoadTable(con, "select * from edi.LicenceDatabaseCodeHistory order by ValidFromUtc");
				var prodTable = BillingDataTestHelper.LoadTable(con, "select * from edi.LicenceDatabaseEdiProdCache");

				Assert.That(table.Rows.Count, Is.EqualTo(2), "combination count");
				var row1 = table.Rows[0];
				Assert.That(row1["DatabaseNumber"], Is.EqualTo(1));
				Assert.That(row1["EnterpriseCode"], Is.EqualTo("ENT"));
				Assert.That(row1["ServerCode"], Is.EqualTo("SYD"));
				Assert.That(row1["ValidFromUtc"], Is.EqualTo(new DateTime(2016, 8, 1)));
				Assert.That(row1["ValidToUtc"], Is.EqualTo(DBNull.Value));
				Assert.That(row1["HostedLocation"], Is.EqualTo("SYD"));
				Assert.That(row1["Product"], Is.EqualTo("SMF"));
				Assert.That(row1["TenantId"], Is.EqualTo("SomeTenantId1"));

				var row2 = table.Rows[1];
				Assert.That(row2["DatabaseNumber"], Is.EqualTo(1));
				Assert.That(row2["EnterpriseCode"], Is.EqualTo("ENT"));
				Assert.That(row2["ServerCode"], Is.EqualTo("SYD"));
				Assert.That(row2["ValidFromUtc"], Is.EqualTo(new DateTime(2016, 8, 3)));
				Assert.That(row2["ValidToUtc"], Is.EqualTo(DBNull.Value));
				Assert.That(row2["HostedLocation"], Is.EqualTo("SYD"));
				Assert.That(row2["Product"], Is.EqualTo("WTA"));
				Assert.That(row2["TenantId"], Is.EqualTo("SomeTenantId1"));

				Assert.That(prodTable.Rows.Count, Is.EqualTo(0), "cache rows");
			}
		}

		[Test]
		public void TestLicenceDatabaseSyncTenantId()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);

				BillingDataTestHelper.ExecuteNonQuery(con,
					@"insert edi.LicenceDatabaseEdiProdCache(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, SystemCreateUTC, HostedLocation, Product, TenantId)
						values('C', 1, 'ENT', 'SYD', '2016-08-01', 'SYD', 'SMF', 'SomeTenantId1');");

				BillingDataTestHelper.ExecuteStoredProcedure(con, "edi.LicenceDatabaseSync");

				// No change
				BillingDataTestHelper.ExecuteNonQuery(con,
					@"insert edi.LicenceDatabaseEdiProdCache(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, SystemCreateUTC, HostedLocation, Product, TenantId)
						values('C', 1, 'ENT', 'SYD', '2016-08-02', 'SYD', 'SMF', 'SomeTenantId1');");

				BillingDataTestHelper.ExecuteStoredProcedure(con, "edi.LicenceDatabaseSync");

				// Change tenantId SomeTenantId1 => SomeTenantId2 and add SomeTenantId3
				BillingDataTestHelper.ExecuteNonQuery(con,
					@"insert edi.LicenceDatabaseEdiProdCache(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, SystemCreateUTC, HostedLocation, Product, TenantId)
						values	('C', 1, 'ENT', 'SYD', '2016-08-03', 'SYD', 'SMF', 'SomeTenantId2'),
								('D', 2, 'ENT', 'SYD', '2016-08-03', 'SYD', 'SMF', 'SomeTenantId3');");

				BillingDataTestHelper.ExecuteStoredProcedure(con, "edi.LicenceDatabaseSync");

				var table = BillingDataTestHelper.LoadTable(con, "select * from edi.LicenceDatabaseCodeHistory order by ValidFromUtc");
				var prodTable = BillingDataTestHelper.LoadTable(con, "select * from edi.LicenceDatabaseEdiProdCache");

				Assert.That(table.Rows.Count, Is.EqualTo(3), "combination count");
				var row1 = table.Rows[0];
				Assert.That(row1["DatabaseNumber"], Is.EqualTo(1));
				Assert.That(row1["EnterpriseCode"], Is.EqualTo("ENT"));
				Assert.That(row1["ServerCode"], Is.EqualTo("SYD"));
				Assert.That(row1["ValidFromUtc"], Is.EqualTo(new DateTime(2016, 8, 1)));
				Assert.That(row1["ValidToUtc"], Is.EqualTo(DBNull.Value));
				Assert.That(row1["HostedLocation"], Is.EqualTo("SYD"));
				Assert.That(row1["Product"], Is.EqualTo("SMF"));
				Assert.That(row1["TenantId"], Is.EqualTo("SomeTenantId1"));

				var row2 = table.Rows[1];
				Assert.That(row2["DatabaseNumber"], Is.EqualTo(1));
				Assert.That(row2["EnterpriseCode"], Is.EqualTo("ENT"));
				Assert.That(row2["ServerCode"], Is.EqualTo("SYD"));
				Assert.That(row2["ValidFromUtc"], Is.EqualTo(new DateTime(2016, 8, 3)));
				Assert.That(row2["ValidToUtc"], Is.EqualTo(DBNull.Value));
				Assert.That(row2["HostedLocation"], Is.EqualTo("SYD"));
				Assert.That(row2["Product"], Is.EqualTo("SMF"));
				Assert.That(row2["TenantId"], Is.EqualTo("SomeTenantId2"));

				var row3 = table.Rows[2];
				Assert.That(row3["DatabaseNumber"], Is.EqualTo(2));
				Assert.That(row3["EnterpriseCode"], Is.EqualTo("ENT"));
				Assert.That(row3["ServerCode"], Is.EqualTo("SYD"));
				Assert.That(row3["ValidFromUtc"], Is.EqualTo(new DateTime(2016, 8, 3)));
				Assert.That(row3["ValidToUtc"], Is.EqualTo(DBNull.Value));
				Assert.That(row3["HostedLocation"], Is.EqualTo("SYD"));
				Assert.That(row3["Product"], Is.EqualTo("SMF"));
				Assert.That(row3["TenantId"], Is.EqualTo("SomeTenantId3"));

				Assert.That(prodTable.Rows.Count, Is.EqualTo(0), "cache rows");
			}
		}

		[Test]
		public void TestLicenceDatabaseSyncCategory()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);

				BillingDataTestHelper.ExecuteNonQuery(con,
					@"insert edi.LicenceDatabaseEdiProdCache(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, SystemCreateUTC, HostedLocation, Product, TenantId, Category)
						values('C', 1, 'ENT', 'SYD', '2016-08-01', 'SYD', 'SMF', 'SomeTenantId1', 'CAT');");

				BillingDataTestHelper.ExecuteStoredProcedure(con, "edi.LicenceDatabaseSync");

				// No change
				BillingDataTestHelper.ExecuteNonQuery(con,
					@"insert edi.LicenceDatabaseEdiProdCache(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, SystemCreateUTC, HostedLocation, Product, TenantId, Category)
						values('C', 1, 'ENT', 'SYD', '2016-08-01', 'SYD', 'SMF', 'SomeTenantId1', 'CAT');");

				BillingDataTestHelper.ExecuteStoredProcedure(con, "edi.LicenceDatabaseSync");

				// Change Category and add new
				BillingDataTestHelper.ExecuteNonQuery(con,
					@"insert edi.LicenceDatabaseEdiProdCache(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, SystemCreateUTC, HostedLocation, Product, TenantId, Category)
						values	('C', 1, 'ENT', 'SYD', '2016-08-03', 'SYD', 'SMF', 'SomeTenantId1', 'CA2'),
								('D', 2, 'ENT', 'SYD', '2016-08-03', 'SYD', 'SMF', 'SomeTenantId2', 'CA3');");

				BillingDataTestHelper.ExecuteStoredProcedure(con, "edi.LicenceDatabaseSync");

				var table = BillingDataTestHelper.LoadTable(con, "select * from edi.LicenceDatabaseCodeHistory order by ValidFromUtc");
				var prodTable = BillingDataTestHelper.LoadTable(con, "select * from edi.LicenceDatabaseEdiProdCache");

				Assert.That(table.Rows.Count, Is.EqualTo(3), "combination count");
				var row1 = table.Rows[0];
				Assert.That(row1["DatabaseNumber"], Is.EqualTo(1));
				Assert.That(row1["EnterpriseCode"], Is.EqualTo("ENT"));
				Assert.That(row1["ServerCode"], Is.EqualTo("SYD"));
				Assert.That(row1["ValidFromUtc"], Is.EqualTo(new DateTime(2016, 8, 1)));
				Assert.That(row1["ValidToUtc"], Is.EqualTo(new DateTime(2016, 8, 3)));
				Assert.That(row1["HostedLocation"], Is.EqualTo("SYD"));
				Assert.That(row1["Product"], Is.EqualTo("SMF"));
				Assert.That(row1["TenantId"], Is.EqualTo("SomeTenantId1"));
				Assert.That(row1["Category"], Is.EqualTo("CAT"));

				var row2 = table.Rows[1];
				Assert.That(row2["DatabaseNumber"], Is.EqualTo(1));
				Assert.That(row2["EnterpriseCode"], Is.EqualTo("ENT"));
				Assert.That(row2["ServerCode"], Is.EqualTo("SYD"));
				Assert.That(row2["ValidFromUtc"], Is.EqualTo(new DateTime(2016, 8, 3)));
				Assert.That(row2["ValidToUtc"], Is.EqualTo(DBNull.Value));
				Assert.That(row2["HostedLocation"], Is.EqualTo("SYD"));
				Assert.That(row2["Product"], Is.EqualTo("SMF"));
				Assert.That(row2["TenantId"], Is.EqualTo("SomeTenantId1"));
				Assert.That(row2["Category"], Is.EqualTo("CA2"));

				var row3 = table.Rows[2];
				Assert.That(row3["DatabaseNumber"], Is.EqualTo(2));
				Assert.That(row3["EnterpriseCode"], Is.EqualTo("ENT"));
				Assert.That(row3["ServerCode"], Is.EqualTo("SYD"));
				Assert.That(row3["ValidFromUtc"], Is.EqualTo(new DateTime(2016, 8, 3)));
				Assert.That(row3["ValidToUtc"], Is.EqualTo(DBNull.Value));
				Assert.That(row3["HostedLocation"], Is.EqualTo("SYD"));
				Assert.That(row3["Product"], Is.EqualTo("SMF"));
				Assert.That(row3["TenantId"], Is.EqualTo("SomeTenantId2"));
				Assert.That(row3["Category"], Is.EqualTo("CA3"));

				Assert.That(prodTable.Rows.Count, Is.EqualTo(0), "cache rows");
			}
		}

		[Test]
		public void TestLicenceDatabaseSyncIsActive()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);

				BillingDataTestHelper.ExecuteNonQuery(con,
					@"insert edi.LicenceDatabaseEdiProdCache(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, SystemCreateUTC, HostedLocation, Product, TenantId, IsActive)
						values('C', 1, 'ENT', 'SYD', '2016-08-01', 'SYD', 'SMF', 'SomeTenantId', 1);");

				BillingDataTestHelper.ExecuteStoredProcedure(con, "edi.LicenceDatabaseSync");

				// No change
				BillingDataTestHelper.ExecuteNonQuery(con,
					@"insert edi.LicenceDatabaseEdiProdCache(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, SystemCreateUTC, HostedLocation, Product, TenantId, IsActive)
						values('C', 1, 'ENT', 'SYD', '2016-08-02', 'SYD', 'SMF', 'SomeTenantId', 1);");

				BillingDataTestHelper.ExecuteStoredProcedure(con, "edi.LicenceDatabaseSync");

				// Change IsActive
				BillingDataTestHelper.ExecuteNonQuery(con,
					@"insert edi.LicenceDatabaseEdiProdCache(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, SystemCreateUTC, HostedLocation, Product, TenantId, IsActive)
						values('C', 1, 'ENT', 'SYD', '2016-08-03', 'SYD', 'SMF', 'SomeTenantId', 0);");

				BillingDataTestHelper.ExecuteStoredProcedure(con, "edi.LicenceDatabaseSync");

				var table = BillingDataTestHelper.LoadTable(con, "select * from edi.LicenceDatabaseCodeHistory order by ValidFromUtc");
				var prodTable = BillingDataTestHelper.LoadTable(con, "select * from edi.LicenceDatabaseEdiProdCache");

				Assert.That(table.Rows.Count, Is.EqualTo(2), "combination count");
				var row1 = table.Rows[0];
				Assert.That(row1["DatabaseNumber"], Is.EqualTo(1));
				Assert.That(row1["EnterpriseCode"], Is.EqualTo("ENT"));
				Assert.That(row1["ServerCode"], Is.EqualTo("SYD"));
				Assert.That(row1["ValidFromUtc"], Is.EqualTo(new DateTime(2016, 8, 1)));
				Assert.That(row1["ValidToUtc"], Is.EqualTo(new DateTime(2016, 8, 3)));
				Assert.That(row1["HostedLocation"], Is.EqualTo("SYD"));
				Assert.That(row1["Product"], Is.EqualTo("SMF"));
				Assert.That(row1["TenantId"], Is.EqualTo("SomeTenantId"));
				Assert.That(row1["IsActive"], Is.EqualTo(true));

				var row2 = table.Rows[1];
				Assert.That(row2["DatabaseNumber"], Is.EqualTo(1));
				Assert.That(row2["EnterpriseCode"], Is.EqualTo("ENT"));
				Assert.That(row2["ServerCode"], Is.EqualTo("SYD"));
				Assert.That(row2["ValidFromUtc"], Is.EqualTo(new DateTime(2016, 8, 3)));
				Assert.That(row2["ValidToUtc"], Is.EqualTo(DBNull.Value));
				Assert.That(row2["HostedLocation"], Is.EqualTo("SYD"));
				Assert.That(row2["Product"], Is.EqualTo("SMF"));
				Assert.That(row2["TenantId"], Is.EqualTo("SomeTenantId"));
				Assert.That(row2["IsActive"], Is.EqualTo(false));

				Assert.That(prodTable.Rows.Count, Is.EqualTo(0), "cache rows");
			}
		}

		[Test]
		public void TestLicenceDatabaseSyncIsTeardownInProgress()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);

				BillingDataTestHelper.ExecuteNonQuery(con,
					@"insert edi.LicenceDatabaseEdiProdCache(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, SystemCreateUTC, HostedLocation, Product, TenantId, IsActive, IsTeardownInProgress)
						values('C', 1, 'ENT', 'SYD', '2016-08-01', 'SYD', 'SMF', 'SomeTenantId', 1, 0);");

				BillingDataTestHelper.ExecuteStoredProcedure(con, "edi.LicenceDatabaseSync");

				// No change
				BillingDataTestHelper.ExecuteNonQuery(con,
					@"insert edi.LicenceDatabaseEdiProdCache(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, SystemCreateUTC, HostedLocation, Product, TenantId, IsActive, IsTeardownInProgress)
						values('C', 1, 'ENT', 'SYD', '2016-08-02', 'SYD', 'SMF', 'SomeTenantId', 1, 0);");

				BillingDataTestHelper.ExecuteStoredProcedure(con, "edi.LicenceDatabaseSync");

				// Change IsTeardownInProgress
				BillingDataTestHelper.ExecuteNonQuery(con,
					@"insert edi.LicenceDatabaseEdiProdCache(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, SystemCreateUTC, HostedLocation, Product, TenantId, IsActive, IsTeardownInProgress)
						values('C', 1, 'ENT', 'SYD', '2016-08-03', 'SYD', 'SMF', 'SomeTenantId', 1, 1);");

				BillingDataTestHelper.ExecuteStoredProcedure(con, "edi.LicenceDatabaseSync");

				var table = BillingDataTestHelper.LoadTable(con, "select * from edi.LicenceDatabaseCodeHistory order by ValidFromUtc");
				var prodTable = BillingDataTestHelper.LoadTable(con, "select * from edi.LicenceDatabaseEdiProdCache");

				Assert.That(table.Rows.Count, Is.EqualTo(2), "combination count");
				var row1 = table.Rows[0];
				Assert.That(row1["DatabaseNumber"], Is.EqualTo(1));
				Assert.That(row1["EnterpriseCode"], Is.EqualTo("ENT"));
				Assert.That(row1["ServerCode"], Is.EqualTo("SYD"));
				Assert.That(row1["ValidFromUtc"], Is.EqualTo(new DateTime(2016, 8, 1)));
				Assert.That(row1["ValidToUtc"], Is.EqualTo(new DateTime(2016, 8, 3)));
				Assert.That(row1["HostedLocation"], Is.EqualTo("SYD"));
				Assert.That(row1["Product"], Is.EqualTo("SMF"));
				Assert.That(row1["TenantId"], Is.EqualTo("SomeTenantId"));
				Assert.That(row1["IsActive"], Is.EqualTo(true));
				Assert.That(row1["IsTeardownInProgress"], Is.EqualTo(false));

				var row2 = table.Rows[1];
				Assert.That(row2["DatabaseNumber"], Is.EqualTo(1));
				Assert.That(row2["EnterpriseCode"], Is.EqualTo("ENT"));
				Assert.That(row2["ServerCode"], Is.EqualTo("SYD"));
				Assert.That(row2["ValidFromUtc"], Is.EqualTo(new DateTime(2016, 8, 3)));
				Assert.That(row2["ValidToUtc"], Is.EqualTo(DBNull.Value));
				Assert.That(row2["HostedLocation"], Is.EqualTo("SYD"));
				Assert.That(row2["Product"], Is.EqualTo("SMF"));
				Assert.That(row2["TenantId"], Is.EqualTo("SomeTenantId"));
				Assert.That(row2["IsActive"], Is.EqualTo(true));
				Assert.That(row2["IsTeardownInProgress"], Is.EqualTo(true));

				Assert.That(prodTable.Rows.Count, Is.EqualTo(0), "cache rows");
			}
		}

		[Test]
		public void TestLicenceDatabaseSyncLicenceType()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);

				BillingDataTestHelper.ExecuteNonQuery(con,
					@"insert edi.LicenceDatabaseEdiProdCache(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, SystemCreateUTC, HostedLocation, Product, TenantId, IsActive, IsTeardownInProgress, LicenceType)
						values('C', 1, 'ENT', 'SYD', '2016-08-01', 'SYD', 'SMF', 'SomeTenantId', 1, 0, 'TST');");

				BillingDataTestHelper.ExecuteStoredProcedure(con, "edi.LicenceDatabaseSync");

				// No change
				BillingDataTestHelper.ExecuteNonQuery(con,
					@"insert edi.LicenceDatabaseEdiProdCache(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, SystemCreateUTC, HostedLocation, Product, TenantId, IsActive, IsTeardownInProgress, LicenceType)
						values('C', 1, 'ENT', 'SYD', '2016-08-02', 'SYD', 'SMF', 'SomeTenantId', 1, 0, 'TST');");

				BillingDataTestHelper.ExecuteStoredProcedure(con, "edi.LicenceDatabaseSync");

				// Change LicenceType
				BillingDataTestHelper.ExecuteNonQuery(con,
					@"insert edi.LicenceDatabaseEdiProdCache(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, SystemCreateUTC, HostedLocation, Product, TenantId, IsActive, IsTeardownInProgress, LicenceType)
						values('C', 1, 'ENT', 'SYD', '2016-08-03', 'SYD', 'SMF', 'SomeTenantId', 1, 0, 'PRD');");

				BillingDataTestHelper.ExecuteStoredProcedure(con, "edi.LicenceDatabaseSync");

				var table = BillingDataTestHelper.LoadTable(con, "select * from edi.LicenceDatabaseCodeHistory order by ValidFromUtc");
				var prodTable = BillingDataTestHelper.LoadTable(con, "select * from edi.LicenceDatabaseEdiProdCache");

				Assert.That(table.Rows.Count, Is.EqualTo(2), "combination count");
				var row1 = table.Rows[0];
				Assert.That(row1["DatabaseNumber"], Is.EqualTo(1));
				Assert.That(row1["EnterpriseCode"], Is.EqualTo("ENT"));
				Assert.That(row1["ServerCode"], Is.EqualTo("SYD"));
				Assert.That(row1["ValidFromUtc"], Is.EqualTo(new DateTime(2016, 8, 1)));
				Assert.That(row1["ValidToUtc"], Is.EqualTo(new DateTime(2016, 8, 3)));
				Assert.That(row1["HostedLocation"], Is.EqualTo("SYD"));
				Assert.That(row1["Product"], Is.EqualTo("SMF"));
				Assert.That(row1["TenantId"], Is.EqualTo("SomeTenantId"));
				Assert.That(row1["IsActive"], Is.EqualTo(true));
				Assert.That(row1["IsTeardownInProgress"], Is.EqualTo(false));
				Assert.That(row1["LicenceType"], Is.EqualTo("TST"));

				var row2 = table.Rows[1];
				Assert.That(row2["DatabaseNumber"], Is.EqualTo(1));
				Assert.That(row2["EnterpriseCode"], Is.EqualTo("ENT"));
				Assert.That(row2["ServerCode"], Is.EqualTo("SYD"));
				Assert.That(row2["ValidFromUtc"], Is.EqualTo(new DateTime(2016, 8, 3)));
				Assert.That(row2["ValidToUtc"], Is.EqualTo(DBNull.Value));
				Assert.That(row2["HostedLocation"], Is.EqualTo("SYD"));
				Assert.That(row2["Product"], Is.EqualTo("SMF"));
				Assert.That(row2["TenantId"], Is.EqualTo("SomeTenantId"));
				Assert.That(row2["IsActive"], Is.EqualTo(true));
				Assert.That(row1["IsTeardownInProgress"], Is.EqualTo(false));
				Assert.That(row2["LicenceType"], Is.EqualTo("PRD"));

				Assert.That(prodTable.Rows.Count, Is.EqualTo(0), "cache rows");
			}
		}

		[Test]
		public void TestNoLicenseDatabaseSyncOnTestServer()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);

				BillingDataTestHelper.ExecuteNonQuery(con,
					@"insert edi.LicenceDatabaseEdiProdCache(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, SystemCreateUTC, HostedLocation)
						values('C', 1, 'ENT', 'SYD', '2016-08-01', 'SYD');");

				BillingDataTestHelper.ExecuteUpdateChargeableAtTime(con, DateTime.UtcNow, isTestServer: true);

				var table = BillingDataTestHelper.LoadTable(con, "select * from edi.LicenceDatabaseCodeHistory order by ValidFromUtc");
				var prodTable = BillingDataTestHelper.LoadTable(con, "select * from edi.LicenceDatabaseEdiProdCache");

				Assert.That(table.Rows.Count, Is.EqualTo(0), "combination count");
				Assert.That(prodTable.Rows.Count, Is.EqualTo(1), "cache rows");
			}
		}

		[Test]
		public void TestClientCompanySync()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);

				var pk1 = "E40E516D-040D-4E81-BCF8-DF44214DCFD7";
				var pk2 = "AE7E97B7-9B8D-4378-ACC6-FB1146ED34FA";
				var pk3 = "A535378D-0164-483D-B8E9-38BCE5267FD6";

				// Two companies, one with a code change
				BillingDataTestHelper.ExecuteNonQuery(con,
					@"truncate table edi.ClientCompanyEdiProdCache;
					truncate table edi.ClientCompanyCodeHistory;
					delete from edi.ClientCompany;
					insert edi.ClientCompanyEdiProdCache(DatabaseNumber, LCC_PK, CompanyCode, ValidFromUtc, CountryCode) values
						(1, '" + pk1 + @"', 'AA1', '2016-06-01', 'AU'),
						(1, '" + pk1 + @"', 'AA2', '2016-07-01', 'AU'),
						(2, '" + pk2 + @"', 'AA1', '2016-06-01', 'AU');");

				BillingDataTestHelper.ExecuteStoredProcedure(con, "edi.ClientCompanySync");

				// Another code change, a new company, and a repeated record to ignore
				BillingDataTestHelper.ExecuteNonQuery(con,
					@"insert edi.ClientCompanyEdiProdCache(DatabaseNumber, LCC_PK, CompanyCode, ValidFromUtc, CountryCode) values
						(1, '" + pk1 + @"', 'AA2', '2016-07-01', 'AU'),
						(1, '" + pk1 + @"', 'AA3', '2016-08-01', 'AU'),
						(1, '" + pk3 + @"', 'AA2', '2016-08-01', 'AU');");

				BillingDataTestHelper.ExecuteStoredProcedure(con, "edi.ClientCompanySync");

				var companyTable = BillingDataTestHelper.LoadTable(con, "select * from edi.ClientCompany");
				var historyTable = BillingDataTestHelper.LoadTable(con,
					@"select c.LCC_PK, h.* from edi.ClientCompany c
					join edi.ClientCompanyCodeHistory h on c.DatabaseNumber = h.DatabaseNumber
						and c.CompanyNumber = h.CompanyNumber order by h.DatabaseNumber, h.CompanyNumber, h.ValidFromUtc");
				var cacheTable = BillingDataTestHelper.LoadTable(con, "select * from edi.ClientCompanyEdiProdCache");

				Assert.That(companyTable.Rows.Count, Is.EqualTo(3), "company rows");
				Assert.That(historyTable.Rows.Count, Is.EqualTo(5), "history rows");
				Assert.That(cacheTable.Rows.Count, Is.EqualTo(0), "cache rows");

				var historyPk1 = historyTable.Rows.Cast<DataRow>()
					.Where(x => (Guid)x["LCC_PK"] == new Guid(pk1))
					.OrderBy(x => (DateTime)x["ValidFromUtc"])
					.ToArray();
				var historyPk2 = historyTable.Rows.Cast<DataRow>()
					.Where(x => (Guid)x["LCC_PK"] == new Guid(pk2))
					.OrderBy(x => (DateTime)x["ValidFromUtc"])
					.ToArray();
				var historyPk3 = historyTable.Rows.Cast<DataRow>()
					.Where(x => (Guid)x["LCC_PK"] == new Guid(pk3))
					.OrderBy(x => (DateTime)x["ValidFromUtc"])
					.ToArray();

				AssertHistory("1/0", historyPk1[0], 1, 1, "AA1", new DateTime(2016, 6, 1), null);
				AssertHistory("1/1", historyPk1[1], 1, 1, "AA2", new DateTime(2016, 7, 1), new DateTime(2016, 8, 1));
				AssertHistory("1/2", historyPk1[2], 1, 1, "AA3", new DateTime(2016, 8, 1), null);
				AssertHistory("2/0", historyPk2[0], 2, 1, "AA1", new DateTime(2016, 6, 1), null);
				AssertHistory("3/0", historyPk3[0], 1, 2, "AA2", new DateTime(2016, 8, 1), null);

				//CountryCode update
				BillingDataTestHelper.ExecuteNonQuery(con,
					@"insert edi.ClientCompanyEdiProdCache(DatabaseNumber, LCC_PK, CompanyCode, ValidFromUtc, CountryCode) values
						(1, '" + pk1 + @"', 'AA3', '2016-08-01', 'US'),
						(1, '" + pk3 + @"', 'AA2', '2016-08-01', 'GB');");
				BillingDataTestHelper.ExecuteStoredProcedure(con, "edi.ClientCompanySync");
				var rowsAsText = String.Join("\r\n",
					BillingDataTestHelper
						.LoadTable(con, "select * from edi.ClientCompany")
						.Rows
						.OfType<DataRow>()
						.Select(r => $"[{r["DatabaseNumber"]}|{r["CompanyNumber"]}|{r["CountryCode"]}]")
						.OrderBy(r => r));

				Assert.AreEqual(
@"[1|1|US]
[1|2|GB]
[2|1|AU]", rowsAsText);
			}
		}

		[Test]
		public void TestNoClientCompanySyncOnTestServer()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);

				var pk1 = "E40E516D-040D-4E81-BCF8-DF44214DCFD7";
				var pk2 = "AE7E97B7-9B8D-4378-ACC6-FB1146ED34FA";

				// Two companies, one with a code change
				BillingDataTestHelper.ExecuteNonQuery(con,
					@"truncate table edi.ClientCompanyEdiProdCache;
					truncate table edi.ClientCompanyCodeHistory;
					delete from edi.ClientCompany;
					insert edi.ClientCompanyEdiProdCache(DatabaseNumber, LCC_PK, CompanyCode, ValidFromUtc, CountryCode) values
						(1, '" + pk1 + @"', 'AA1', '2016-06-01', 'AU'),
						(1, '" + pk1 + @"', 'AA2', '2016-07-01', 'AU'),
						(2, '" + pk2 + @"', 'AA1', '2016-06-01', 'AU');");

				BillingDataTestHelper.ExecuteUpdateChargeableAtTime(con, DateTime.UtcNow, isTestServer: true);

				var companyTable = BillingDataTestHelper.LoadTable(con, "select * from edi.ClientCompany");
				var historyTable = BillingDataTestHelper.LoadTable(con,
					@"select c.LCC_PK, h.* from edi.ClientCompany c
					join edi.ClientCompanyCodeHistory h on c.DatabaseNumber = h.DatabaseNumber
						and c.CompanyNumber = h.CompanyNumber order by h.DatabaseNumber, h.CompanyNumber, h.ValidFromUtc");
				var cacheTable = BillingDataTestHelper.LoadTable(con, "select * from edi.ClientCompanyEdiProdCache");

				Assert.That(companyTable.Rows.Count, Is.EqualTo(0), "company rows");
				Assert.That(historyTable.Rows.Count, Is.EqualTo(0), "history rows");
				Assert.That(cacheTable.Rows.Count, Is.EqualTo(3), "cache rows");
			}
		}

		[Test]
		public void TestLicenceDatabaseSyncInternalSMFLicences()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);

				BillingDataTestHelper.ExecuteNonQuery(con,
					@"insert edi.LicenceDatabaseEdiProdCache(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, SystemCreateUTC, HostedLocation, IsInternal, Product)
			values('C', 1, 'ENT', 'SYD', '2024-11-01', 'SYD', 1, 'SMF');");

				BillingDataTestHelper.ExecuteStoredProcedure(con, "edi.LicenceDatabaseSync");

				// Change server code SYD -> MEL
				BillingDataTestHelper.ExecuteNonQuery(con,
					@"insert edi.LicenceDatabaseEdiProdCache(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, SystemCreateUTC, HostedLocation, IsInternal, Product)
			values('C', 1, 'ENT', 'MEL', '2024-11-02', 'SYD', 1, 'SMF');");

				BillingDataTestHelper.ExecuteStoredProcedure(con, "edi.LicenceDatabaseSync");

				// Add new server with code SYD
				// Should only sync specified internal licences such as Category = 'SMF', and IsInternal = 0 should sync normally (not into internal table)
				BillingDataTestHelper.ExecuteNonQuery(con,
					@"insert edi.LicenceDatabaseEdiProdCache(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, SystemCreateUTC, HostedLocation, IsInternal, Product)
			values('C', 1, 'ENT', 'MEL', '2024-11-03', 'SYD', 1, 'SMF'), --Same as existing internal licence, should be skipped
			      ('D', 2, 'ENT', 'SYD', '2024-11-04', 'NCW', 1, 'SMF'), --New Interal licence Product = 'SMF' should go to internal table
			      ('F', 3, 'ENT', 'SNF', '2024-11-05', 'SYD', 0, 'SMF'), --IsInternal = 0 should go to normal licence table
			      ('G', 4, 'ENT', 'EXT', '2024-11-06', 'SYD', 1, 'KZJ'); --IsInternal = 1 but Product = 'KZJ' should go to internal table");

				BillingDataTestHelper.ExecuteStoredProcedure(con, "edi.LicenceDatabaseSync");

				var intTable = BillingDataTestHelper.LoadTable(con, "select * from edi.InternalLicenceDatabaseCodeHistory order by ValidFromUtc");

				Assert.That(intTable.Rows.Count, Is.EqualTo(4), "combination count");

				var row1 = intTable.Rows[0];
				Assert.That(row1["DatabaseNumber"], Is.EqualTo(1));
				Assert.That(row1["EnterpriseCode"], Is.EqualTo("ENT"));
				Assert.That(row1["ServerCode"], Is.EqualTo("SYD"));
				Assert.That(row1["ValidFromUtc"], Is.EqualTo(new DateTime(2024, 11, 1)));
				Assert.That(row1["ValidToUtc"], Is.EqualTo(new DateTime(2024, 11, 4)));
				Assert.That(row1["HostedLocation"], Is.EqualTo("SYD"));
				Assert.That(row1["Product"], Is.EqualTo("SMF"));

				var row2 = intTable.Rows[1];
				Assert.That(row2["DatabaseNumber"], Is.EqualTo(1));
				Assert.That(row2["EnterpriseCode"], Is.EqualTo("ENT"));
				Assert.That(row2["ServerCode"], Is.EqualTo("MEL"));
				Assert.That(row2["ValidFromUtc"], Is.EqualTo(new DateTime(2024, 11, 2)));
				Assert.That(row2["ValidToUtc"], Is.EqualTo(DBNull.Value));
				Assert.That(row2["HostedLocation"], Is.EqualTo("SYD"));
				Assert.That(row2["Product"], Is.EqualTo("SMF"));

				var row3 = intTable.Rows[2];
				Assert.That(row3["DatabaseNumber"], Is.EqualTo(2));
				Assert.That(row3["EnterpriseCode"], Is.EqualTo("ENT"));
				Assert.That(row3["ServerCode"], Is.EqualTo("SYD"));
				Assert.That(row3["ValidFromUtc"], Is.EqualTo(new DateTime(2024, 11, 4)));
				Assert.That(row3["ValidToUtc"], Is.EqualTo(DBNull.Value));
				Assert.That(row3["HostedLocation"], Is.EqualTo("NCW"));
				Assert.That(row3["Product"], Is.EqualTo("SMF"));

				var row4 = intTable.Rows[3];
				Assert.That(row4["DatabaseNumber"], Is.EqualTo(4));
				Assert.That(row4["EnterpriseCode"], Is.EqualTo("ENT"));
				Assert.That(row4["ServerCode"], Is.EqualTo("EXT"));
				Assert.That(row4["ValidFromUtc"], Is.EqualTo(new DateTime(2024, 11, 6)));
				Assert.That(row4["ValidToUtc"], Is.EqualTo(DBNull.Value));
				Assert.That(row4["HostedLocation"], Is.EqualTo("SYD"));
				Assert.That(row4["Product"], Is.EqualTo("KZJ"));

				var extTable = BillingDataTestHelper.LoadTable(con, "select * from edi.LicenceDatabaseCodeHistory order by ValidFromUtc");
				Assert.That(extTable.Rows.Count, Is.EqualTo(1), "Should only sync normal licences where IsInternal <> 1");

				var extRow1 = extTable.Rows[0];
				Assert.That(extRow1["DatabaseNumber"], Is.EqualTo(3));
				Assert.That(extRow1["EnterpriseCode"], Is.EqualTo("ENT"));
				Assert.That(extRow1["ServerCode"], Is.EqualTo("SNF"));
				Assert.That(extRow1["ValidFromUtc"], Is.EqualTo(new DateTime(2024, 11, 5)));
				Assert.That(extRow1["ValidToUtc"], Is.EqualTo(DBNull.Value));
				Assert.That(extRow1["HostedLocation"], Is.EqualTo("SYD"));
				Assert.That(extRow1["Product"], Is.EqualTo("SMF"));

				var prodTable = BillingDataTestHelper.LoadTable(con, "select * from edi.LicenceDatabaseEdiProdCache");
				Assert.That(prodTable.Rows.Count, Is.EqualTo(0), "cache rows");
			}
		}

		void AssertHistory(string msg, DataRow row, int databaseNumber, int companyNumber, string code, DateTime validFrom, DateTime? validTo)
		{
			Assert.That(row["DatabaseNumber"], Is.EqualTo(databaseNumber), msg + " DB#");
			Assert.That(row["CompanyNumber"], Is.EqualTo(companyNumber), msg + " Company#");
			Assert.That(row["CompanyCode"], Is.EqualTo(code), msg + " Code");
			Assert.That(row["ValidFromUtc"], Is.EqualTo(validFrom), msg + " ValidFromUtc");
			Assert.That(row["ValidToUtc"], Is.EqualTo((object)validTo ?? DBNull.Value), msg + " ValidToUtc");
		}

		[SetUp]
		public void SetUp()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
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

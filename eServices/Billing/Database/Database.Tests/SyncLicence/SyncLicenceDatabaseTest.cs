using System;
using System.Data;
using System.Data.SqlClient;
using CargoWise.eServices.Billing.Tests.Common;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Database.Tests
{
	[TestFixture]
	public class SyncLicenceDatabaseTest
	{
		private readonly Guid LD_PK = new Guid("12345678-90ab-cdef-1234-567890abcdef");
		private readonly Guid LD_LE = new Guid("abcdef12-3456-7890-abcd-ef1234567890");
		private const string LD_ServerCode = "SRV";
		private const string LD_LicenceType = "TRN";
		private const bool LD_IsActive = true;
		private const int LD_DatabaseNumber = 100;
		private const string LD_Product = "CW1";
		private const string LD_HostedLocation = "LOC";
		private const string LD_TenantID = "TENANT123";
		private const string LD_ServerCode2 = "NEW";
		private const string LD_LicenceType2 = "TST";
		private const bool LD_IsActive2 = false;
		private const int LD_DatabaseNumber2 = 200;
		private const string LD_Product2 = "CWN";
		private const string LD_HostedLocation2 = "NLC";
		private const string LD_TenantID2 = "TENANT456";

		[SetUp]
		public void SetUp()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateTable(con, "ediProd.LicenceDatabase");
			}
		}

		[Test]
		public void TestAdded()
		{
			DataTable dataTable;

			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = "ediProd.SyncLicenceDatabase";
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add(new SqlParameter("@RowState", SqlDbType.VarChar) { Value = "Added" });
					cmd.Parameters.Add(new SqlParameter("@LD_PK", SqlDbType.UniqueIdentifier) { Value = LD_PK });
					cmd.Parameters.Add(new SqlParameter("@LD_ServerCode", SqlDbType.VarChar) { Value = LD_ServerCode });
					cmd.Parameters.Add(new SqlParameter("@LD_LicenceType", SqlDbType.VarChar) { Value = LD_LicenceType });
					cmd.Parameters.Add(new SqlParameter("@LD_LE", SqlDbType.UniqueIdentifier) { Value = LD_LE });
					cmd.Parameters.Add(new SqlParameter("@LD_IsActive", SqlDbType.Bit) { Value = LD_IsActive });
					cmd.Parameters.Add(new SqlParameter("@LD_DatabaseNumber", SqlDbType.Int) { Value = LD_DatabaseNumber });
					cmd.Parameters.Add(new SqlParameter("@LD_Product", SqlDbType.VarChar) { Value = LD_Product });
					cmd.Parameters.Add(new SqlParameter("@LD_HostedLocation", SqlDbType.Char) { Value = LD_HostedLocation });
					cmd.Parameters.Add(new SqlParameter("@LD_TenantID", SqlDbType.VarChar) { Value = LD_TenantID });
					cmd.ExecuteNonQuery();
				}

				dataTable = BillingDataTestHelper.LoadTableByName(con, "ediProd.LicenceDatabase");
			}

			Assert.That(dataTable.Rows.Count, Is.EqualTo(1));
			Assert.That(dataTable.Rows[0]["LD_PK"], Is.EqualTo(LD_PK));
			Assert.That(dataTable.Rows[0]["LD_ServerCode"], Is.EqualTo(LD_ServerCode));
			Assert.That(dataTable.Rows[0]["LD_LicenceType"], Is.EqualTo(LD_LicenceType));
			Assert.That(dataTable.Rows[0]["LD_LE"], Is.EqualTo(LD_LE));
			Assert.That(dataTable.Rows[0]["LD_IsActive"], Is.EqualTo(LD_IsActive));
			Assert.That(dataTable.Rows[0]["LD_DatabaseNumber"], Is.EqualTo(LD_DatabaseNumber));
			Assert.That(dataTable.Rows[0]["LD_Product"], Is.EqualTo(LD_Product));
			Assert.That(dataTable.Rows[0]["LD_HostedLocation"], Is.EqualTo(LD_HostedLocation));
			Assert.That(dataTable.Rows[0]["LD_TenantID"], Is.EqualTo(LD_TenantID));
		}

		[Test]
		public void TestModified()
		{
			DataTable dataTable;

			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = "ediProd.SyncLicenceDatabase";
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add(new SqlParameter("@RowState", SqlDbType.VarChar) { Value = "Added" });
					cmd.Parameters.Add(new SqlParameter("@LD_PK", SqlDbType.UniqueIdentifier) { Value = LD_PK });
					cmd.Parameters.Add(new SqlParameter("@LD_ServerCode", SqlDbType.VarChar) { Value = LD_ServerCode });
					cmd.Parameters.Add(new SqlParameter("@LD_LicenceType", SqlDbType.VarChar) { Value = LD_LicenceType });
					cmd.Parameters.Add(new SqlParameter("@LD_LE", SqlDbType.UniqueIdentifier) { Value = LD_LE });
					cmd.Parameters.Add(new SqlParameter("@LD_IsActive", SqlDbType.Bit) { Value = LD_IsActive });
					cmd.Parameters.Add(new SqlParameter("@LD_DatabaseNumber", SqlDbType.Int) { Value = LD_DatabaseNumber });
					cmd.Parameters.Add(new SqlParameter("@LD_Product", SqlDbType.VarChar) { Value = LD_Product });
					cmd.Parameters.Add(new SqlParameter("@LD_HostedLocation", SqlDbType.Char) { Value = LD_HostedLocation });
					cmd.Parameters.Add(new SqlParameter("@LD_TenantID", SqlDbType.VarChar) { Value = LD_TenantID });
					cmd.ExecuteNonQuery();
				}

				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = "ediProd.SyncLicenceDatabase";
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add(new SqlParameter("@RowState", SqlDbType.VarChar) { Value = "Modified" });
					cmd.Parameters.Add(new SqlParameter("@LD_PK", SqlDbType.UniqueIdentifier) { Value = LD_PK });
					cmd.Parameters.Add(new SqlParameter("@LD_ServerCode", SqlDbType.VarChar) { Value = LD_ServerCode2 });
					cmd.Parameters.Add(new SqlParameter("@LD_LicenceType", SqlDbType.VarChar) { Value = LD_LicenceType2 });
					cmd.Parameters.Add(new SqlParameter("@LD_LE", SqlDbType.UniqueIdentifier) { Value = LD_LE });
					cmd.Parameters.Add(new SqlParameter("@LD_IsActive", SqlDbType.Bit) { Value = LD_IsActive2 });
					cmd.Parameters.Add(new SqlParameter("@LD_DatabaseNumber", SqlDbType.Int) { Value = LD_DatabaseNumber2 });
					cmd.Parameters.Add(new SqlParameter("@LD_Product", SqlDbType.VarChar) { Value = LD_Product2 });
					cmd.Parameters.Add(new SqlParameter("@LD_HostedLocation", SqlDbType.Char) { Value = LD_HostedLocation2 });
					cmd.Parameters.Add(new SqlParameter("@LD_TenantID", SqlDbType.VarChar) { Value = LD_TenantID2 });
					cmd.ExecuteNonQuery();
				}

				dataTable = BillingDataTestHelper.LoadTableByName(con, "ediProd.LicenceDatabase");
			}

			Assert.That(dataTable.Rows.Count, Is.EqualTo(1));
			Assert.That(dataTable.Rows[0]["LD_PK"], Is.EqualTo(LD_PK));
			Assert.That(dataTable.Rows[0]["LD_ServerCode"], Is.EqualTo(LD_ServerCode2));
			Assert.That(dataTable.Rows[0]["LD_LicenceType"], Is.EqualTo(LD_LicenceType2));
			Assert.That(dataTable.Rows[0]["LD_LE"], Is.EqualTo(LD_LE));
			Assert.That(dataTable.Rows[0]["LD_IsActive"], Is.EqualTo(LD_IsActive2));
			Assert.That(dataTable.Rows[0]["LD_DatabaseNumber"], Is.EqualTo(LD_DatabaseNumber2));
			Assert.That(dataTable.Rows[0]["LD_Product"], Is.EqualTo(LD_Product2));
			Assert.That(dataTable.Rows[0]["LD_HostedLocation"], Is.EqualTo(LD_HostedLocation2));
			Assert.That(dataTable.Rows[0]["LD_TenantID"], Is.EqualTo(LD_TenantID2));
		}

		[Test]
		public void TestDeleted()
		{
			DataTable dataTable;

			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = "ediProd.SyncLicenceDatabase";
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add(new SqlParameter("@RowState", SqlDbType.VarChar) { Value = "Added" });
					cmd.Parameters.Add(new SqlParameter("@LD_PK", SqlDbType.UniqueIdentifier) { Value = LD_PK });
					cmd.Parameters.Add(new SqlParameter("@LD_ServerCode", SqlDbType.VarChar) { Value = LD_ServerCode });
					cmd.Parameters.Add(new SqlParameter("@LD_LicenceType", SqlDbType.VarChar) { Value = LD_LicenceType });
					cmd.Parameters.Add(new SqlParameter("@LD_LE", SqlDbType.UniqueIdentifier) { Value = LD_LE });
					cmd.Parameters.Add(new SqlParameter("@LD_IsActive", SqlDbType.Bit) { Value = LD_IsActive });
					cmd.Parameters.Add(new SqlParameter("@LD_DatabaseNumber", SqlDbType.Int) { Value = LD_DatabaseNumber });
					cmd.Parameters.Add(new SqlParameter("@LD_Product", SqlDbType.VarChar) { Value = LD_Product });
					cmd.Parameters.Add(new SqlParameter("@LD_HostedLocation", SqlDbType.Char) { Value = LD_HostedLocation });
					cmd.Parameters.Add(new SqlParameter("@LD_TenantID", SqlDbType.VarChar) { Value = LD_TenantID });
					cmd.ExecuteNonQuery();
				}

				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = "ediProd.SyncLicenceDatabase";
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add(new SqlParameter("@RowState", SqlDbType.VarChar) { Value = "Deleted" });
					cmd.Parameters.Add(new SqlParameter("@LD_PK", SqlDbType.UniqueIdentifier) { Value = LD_PK });
					cmd.Parameters.Add(new SqlParameter("@LD_ServerCode", SqlDbType.VarChar) { Value = LD_ServerCode });
					cmd.Parameters.Add(new SqlParameter("@LD_LicenceType", SqlDbType.VarChar) { Value = LD_LicenceType });
					cmd.Parameters.Add(new SqlParameter("@LD_LE", SqlDbType.UniqueIdentifier) { Value = LD_LE });
					cmd.Parameters.Add(new SqlParameter("@LD_IsActive", SqlDbType.Bit) { Value = LD_IsActive });
					cmd.Parameters.Add(new SqlParameter("@LD_DatabaseNumber", SqlDbType.Int) { Value = LD_DatabaseNumber });
					cmd.Parameters.Add(new SqlParameter("@LD_Product", SqlDbType.VarChar) { Value = LD_Product });
					cmd.Parameters.Add(new SqlParameter("@LD_HostedLocation", SqlDbType.Char) { Value = LD_HostedLocation });
					cmd.Parameters.Add(new SqlParameter("@LD_TenantID", SqlDbType.VarChar) { Value = LD_TenantID });
					cmd.ExecuteNonQuery();
				}

				dataTable = BillingDataTestHelper.LoadTableByName(con, "ediProd.LicenceDatabase");
			}

			Assert.That(dataTable.Rows.Count, Is.EqualTo(0));
		}

		[Test]
		public void TestInvalidRowState()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = "ediProd.SyncLicenceDatabase";
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add(new SqlParameter("@RowState", SqlDbType.VarChar) { Value = "Invalid" });
					cmd.Parameters.Add(new SqlParameter("@LD_PK", SqlDbType.UniqueIdentifier) { Value = LD_PK });
					cmd.Parameters.Add(new SqlParameter("@LD_ServerCode", SqlDbType.VarChar) { Value = LD_ServerCode });
					cmd.Parameters.Add(new SqlParameter("@LD_LicenceType", SqlDbType.VarChar) { Value = LD_LicenceType });
					cmd.Parameters.Add(new SqlParameter("@LD_LE", SqlDbType.UniqueIdentifier) { Value = LD_LE });
					cmd.Parameters.Add(new SqlParameter("@LD_IsActive", SqlDbType.Bit) { Value = LD_IsActive });
					cmd.Parameters.Add(new SqlParameter("@LD_DatabaseNumber", SqlDbType.Int) { Value = LD_DatabaseNumber });
					cmd.Parameters.Add(new SqlParameter("@LD_Product", SqlDbType.VarChar) { Value = LD_Product });
					cmd.Parameters.Add(new SqlParameter("@LD_HostedLocation", SqlDbType.Char) { Value = LD_HostedLocation });
					cmd.Parameters.Add(new SqlParameter("@LD_TenantID", SqlDbType.VarChar) { Value = LD_TenantID });

					var ex = Assert.Throws<SqlException>(() => cmd.ExecuteNonQuery());
					Assert.That(ex.Message, Does.Contain("Invalid RowState value. Must be Added, Modified, or Deleted."));
				}
			}
		}

		[Test]
		public void TestNullLD_PK()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = "ediProd.SyncLicenceDatabase";
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add(new SqlParameter("@RowState", SqlDbType.VarChar) { Value = "Added" });
					cmd.Parameters.Add(new SqlParameter("@LD_PK", SqlDbType.UniqueIdentifier) { Value = DBNull.Value });
					cmd.Parameters.Add(new SqlParameter("@LD_ServerCode", SqlDbType.VarChar) { Value = LD_ServerCode });
					cmd.Parameters.Add(new SqlParameter("@LD_LicenceType", SqlDbType.VarChar) { Value = LD_LicenceType });
					cmd.Parameters.Add(new SqlParameter("@LD_LE", SqlDbType.UniqueIdentifier) { Value = LD_LE });
					cmd.Parameters.Add(new SqlParameter("@LD_IsActive", SqlDbType.Bit) { Value = LD_IsActive });
					cmd.Parameters.Add(new SqlParameter("@LD_DatabaseNumber", SqlDbType.Int) { Value = LD_DatabaseNumber });
					cmd.Parameters.Add(new SqlParameter("@LD_Product", SqlDbType.VarChar) { Value = LD_Product });
					cmd.Parameters.Add(new SqlParameter("@LD_HostedLocation", SqlDbType.Char) { Value = LD_HostedLocation });
					cmd.Parameters.Add(new SqlParameter("@LD_TenantID", SqlDbType.VarChar) { Value = LD_TenantID });

					var ex = Assert.Throws<SqlException>(() => cmd.ExecuteNonQuery());
					Assert.That(ex.Message, Does.Contain("LD_PK cannot be null."));
				}
			}
		}

		[Test]
		public void TestModifyNonExistentRecord()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = "ediProd.SyncLicenceDatabase";
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add(new SqlParameter("@RowState", SqlDbType.VarChar) { Value = "Modified" });
					cmd.Parameters.Add(new SqlParameter("@LD_PK", SqlDbType.UniqueIdentifier) { Value = LD_PK });
					cmd.Parameters.Add(new SqlParameter("@LD_ServerCode", SqlDbType.VarChar) { Value = LD_ServerCode });
					cmd.Parameters.Add(new SqlParameter("@LD_LicenceType", SqlDbType.VarChar) { Value = LD_LicenceType });
					cmd.Parameters.Add(new SqlParameter("@LD_LE", SqlDbType.UniqueIdentifier) { Value = LD_LE });
					cmd.Parameters.Add(new SqlParameter("@LD_IsActive", SqlDbType.Bit) { Value = LD_IsActive });
					cmd.Parameters.Add(new SqlParameter("@LD_DatabaseNumber", SqlDbType.Int) { Value = LD_DatabaseNumber });
					cmd.Parameters.Add(new SqlParameter("@LD_Product", SqlDbType.VarChar) { Value = LD_Product });
					cmd.Parameters.Add(new SqlParameter("@LD_HostedLocation", SqlDbType.Char) { Value = LD_HostedLocation });
					cmd.Parameters.Add(new SqlParameter("@LD_TenantID", SqlDbType.VarChar) { Value = LD_TenantID });

					var ex = Assert.Throws<SqlException>(() => cmd.ExecuteNonQuery());
					Assert.That(ex.Message, Does.Contain("No record found to modify."));
				}
			}
		}

		[Test]
		public void TestDeleteNonExistentRecord()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = "ediProd.SyncLicenceDatabase";
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add(new SqlParameter("@RowState", SqlDbType.VarChar) { Value = "Deleted" });
					cmd.Parameters.Add(new SqlParameter("@LD_PK", SqlDbType.UniqueIdentifier) { Value = LD_PK });
					cmd.Parameters.Add(new SqlParameter("@LD_ServerCode", SqlDbType.VarChar) { Value = LD_ServerCode });
					cmd.Parameters.Add(new SqlParameter("@LD_LicenceType", SqlDbType.VarChar) { Value = LD_LicenceType });
					cmd.Parameters.Add(new SqlParameter("@LD_LE", SqlDbType.UniqueIdentifier) { Value = LD_LE });
					cmd.Parameters.Add(new SqlParameter("@LD_IsActive", SqlDbType.Bit) { Value = LD_IsActive });
					cmd.Parameters.Add(new SqlParameter("@LD_DatabaseNumber", SqlDbType.Int) { Value = LD_DatabaseNumber });
					cmd.Parameters.Add(new SqlParameter("@LD_Product", SqlDbType.VarChar) { Value = LD_Product });
					cmd.Parameters.Add(new SqlParameter("@LD_HostedLocation", SqlDbType.Char) { Value = LD_HostedLocation });
					cmd.Parameters.Add(new SqlParameter("@LD_TenantID", SqlDbType.VarChar) { Value = LD_TenantID });

					var ex = Assert.Throws<SqlException>(() => cmd.ExecuteNonQuery());
					Assert.That(ex.Message, Does.Contain("No record found to delete."));
				}
			}
		}
	}
}

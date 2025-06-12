using System;
using System.Data;
using System.Data.SqlClient;
using CargoWise.eServices.Billing.Tests.Common;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Database.Tests
{
	[TestFixture]
	public class SyncLicenceEnterpriseTest
	{
		private readonly Guid LE_PK = new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
		private const string LE_EnterpriseCode = "ORG";
		private const bool LE_IsInternal = true;
		private const string LE_EnterpriseCode2 = "NEW";
		private const bool LE_IsInternal2 = false;

		[SetUp]
		public void SetUp()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateTable(con, "ediProd.LicenceEnterprise");
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
					cmd.CommandText = "ediProd.SyncLicenceEnterprise";
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add(new SqlParameter("@RowState", SqlDbType.VarChar) { Value = "Added" });
					cmd.Parameters.Add(new SqlParameter("@LE_PK", SqlDbType.UniqueIdentifier) { Value = LE_PK });
					cmd.Parameters.Add(new SqlParameter("@LE_EnterpriseCode", SqlDbType.VarChar) { Value = LE_EnterpriseCode });
					cmd.Parameters.Add(new SqlParameter("@LE_IsInternal", SqlDbType.Bit) { Value = LE_IsInternal });
					cmd.ExecuteNonQuery();
				}

				dataTable = BillingDataTestHelper.LoadTableByName(con, "ediProd.LicenceEnterprise");
			}

			Assert.That(dataTable.Rows.Count, Is.EqualTo(1));
			Assert.That(dataTable.Rows[0]["LE_PK"], Is.EqualTo(LE_PK));
			Assert.That(dataTable.Rows[0]["LE_EnterpriseCode"], Is.EqualTo(LE_EnterpriseCode));
			Assert.That(dataTable.Rows[0]["LE_IsInternal"], Is.EqualTo(LE_IsInternal));
		}

		[Test]
		public void TestModified()
		{
			DataTable dataTable;

			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = "ediProd.SyncLicenceEnterprise";
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add(new SqlParameter("@RowState", SqlDbType.VarChar) { Value = "Added" });
					cmd.Parameters.Add(new SqlParameter("@LE_PK", SqlDbType.UniqueIdentifier) { Value = LE_PK });
					cmd.Parameters.Add(new SqlParameter("@LE_EnterpriseCode", SqlDbType.VarChar) { Value = LE_EnterpriseCode });
					cmd.Parameters.Add(new SqlParameter("@LE_IsInternal", SqlDbType.Bit) { Value = LE_IsInternal });
					cmd.ExecuteNonQuery();
				}

				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = "ediProd.SyncLicenceEnterprise";
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add(new SqlParameter("@RowState", SqlDbType.VarChar) { Value = "Modified" });
					cmd.Parameters.Add(new SqlParameter("@LE_PK", SqlDbType.UniqueIdentifier) { Value = LE_PK });
					cmd.Parameters.Add(new SqlParameter("@LE_EnterpriseCode", SqlDbType.VarChar) { Value = LE_EnterpriseCode2 });
					cmd.Parameters.Add(new SqlParameter("@LE_IsInternal", SqlDbType.Bit) { Value = LE_IsInternal2 });
					cmd.ExecuteNonQuery();
				}

				dataTable = BillingDataTestHelper.LoadTableByName(con, "ediProd.LicenceEnterprise");
			}

			Assert.That(dataTable.Rows.Count, Is.EqualTo(1));
			Assert.That(dataTable.Rows[0]["LE_PK"], Is.EqualTo(LE_PK));
			Assert.That(dataTable.Rows[0]["LE_EnterpriseCode"], Is.EqualTo(LE_EnterpriseCode2));
			Assert.That(dataTable.Rows[0]["LE_IsInternal"], Is.EqualTo(LE_IsInternal2));
		}

		[Test]
		public void TestDeleted()
		{
			DataTable dataTable;

			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = "ediProd.SyncLicenceEnterprise";
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add(new SqlParameter("@RowState", SqlDbType.VarChar) { Value = "Added" });
					cmd.Parameters.Add(new SqlParameter("@LE_PK", SqlDbType.UniqueIdentifier) { Value = LE_PK });
					cmd.Parameters.Add(new SqlParameter("@LE_EnterpriseCode", SqlDbType.VarChar) { Value = LE_EnterpriseCode });
					cmd.Parameters.Add(new SqlParameter("@LE_IsInternal", SqlDbType.Bit) { Value = LE_IsInternal });
					cmd.ExecuteNonQuery();
				}

				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = "ediProd.SyncLicenceEnterprise";
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add(new SqlParameter("@RowState", SqlDbType.VarChar) { Value = "Deleted" });
					cmd.Parameters.Add(new SqlParameter("@LE_PK", SqlDbType.UniqueIdentifier) { Value = LE_PK });
					cmd.Parameters.Add(new SqlParameter("@LE_EnterpriseCode", SqlDbType.VarChar) { Value = LE_EnterpriseCode });
					cmd.Parameters.Add(new SqlParameter("@LE_IsInternal", SqlDbType.Bit) { Value = LE_IsInternal });
					cmd.ExecuteNonQuery();
				}

				dataTable = BillingDataTestHelper.LoadTableByName(con, "ediProd.LicenceEnterprise");
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
					cmd.CommandText = "ediProd.SyncLicenceEnterprise";
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add(new SqlParameter("@RowState", SqlDbType.VarChar) { Value = "Invalid" });
					cmd.Parameters.Add(new SqlParameter("@LE_PK", SqlDbType.UniqueIdentifier) { Value = LE_PK });
					cmd.Parameters.Add(new SqlParameter("@LE_EnterpriseCode", SqlDbType.VarChar) { Value = LE_EnterpriseCode });
					cmd.Parameters.Add(new SqlParameter("@LE_IsInternal", SqlDbType.Bit) { Value = LE_IsInternal });

					var ex = Assert.Throws<SqlException>(() => cmd.ExecuteNonQuery());
					Assert.That(ex.Message, Does.Contain("Invalid RowState value. Must be Added, Modified, or Deleted."));
				}
			}
		}

		[Test]
		public void TestNullLE_PK()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = "ediProd.SyncLicenceEnterprise";
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add(new SqlParameter("@RowState", SqlDbType.VarChar) { Value = "Added" });
					cmd.Parameters.Add(new SqlParameter("@LE_PK", SqlDbType.UniqueIdentifier) { Value = DBNull.Value });
					cmd.Parameters.Add(new SqlParameter("@LE_EnterpriseCode", SqlDbType.VarChar) { Value = LE_EnterpriseCode });
					cmd.Parameters.Add(new SqlParameter("@LE_IsInternal", SqlDbType.Bit) { Value = LE_IsInternal });

					var ex = Assert.Throws<SqlException>(() => cmd.ExecuteNonQuery());
					Assert.That(ex.Message, Does.Contain("LE_PK cannot be null."));
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
					cmd.CommandText = "ediProd.SyncLicenceEnterprise";
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add(new SqlParameter("@RowState", SqlDbType.VarChar) { Value = "Modified" });
					cmd.Parameters.Add(new SqlParameter("@LE_PK", SqlDbType.UniqueIdentifier) { Value = LE_PK });
					cmd.Parameters.Add(new SqlParameter("@LE_EnterpriseCode", SqlDbType.VarChar) { Value = LE_EnterpriseCode });
					cmd.Parameters.Add(new SqlParameter("@LE_IsInternal", SqlDbType.Bit) { Value = LE_IsInternal });

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
					cmd.CommandText = "ediProd.SyncLicenceEnterprise";
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add(new SqlParameter("@RowState", SqlDbType.VarChar) { Value = "Deleted" });
					cmd.Parameters.Add(new SqlParameter("@LE_PK", SqlDbType.UniqueIdentifier) { Value = LE_PK });
					cmd.Parameters.Add(new SqlParameter("@LE_EnterpriseCode", SqlDbType.VarChar) { Value = LE_EnterpriseCode });
					cmd.Parameters.Add(new SqlParameter("@LE_IsInternal", SqlDbType.Bit) { Value = LE_IsInternal });

					var ex = Assert.Throws<SqlException>(() => cmd.ExecuteNonQuery());
					Assert.That(ex.Message, Does.Contain("No record found to delete."));
				}
			}
		}
	}
}

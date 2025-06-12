using System;
using System.Data;
using System.Data.SqlClient;
using CargoWise.eServices.Billing.Tests.Common;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Database.Tests
{
	[TestFixture]
	public class SyncLicenceHeaderTest
	{
		private readonly Guid LA_PK = new Guid("fedcba98-7654-3210-fedc-ba9876543210");
		private readonly Guid LA_LD = new Guid("01234567-89ab-cdef-0123-456789abcdef");
		private readonly Guid LA_LD2 = new Guid("89abcdef-0123-4567-89ab-cdef01234567");

		[SetUp]
		public void SetUp()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateTable(con, "ediProd.LicenceHeader");
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
					cmd.CommandText = "ediProd.SyncLicenceHeader";
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add(new SqlParameter("@RowState", SqlDbType.VarChar) { Value = "Added" });
					cmd.Parameters.Add(new SqlParameter("@LA_PK", SqlDbType.UniqueIdentifier) { Value = LA_PK });
					cmd.Parameters.Add(new SqlParameter("@LA_LD", SqlDbType.UniqueIdentifier) { Value = LA_LD });
					cmd.ExecuteNonQuery();
				}

				dataTable = BillingDataTestHelper.LoadTableByName(con, "ediProd.LicenceHeader");
			}

			Assert.That(dataTable.Rows.Count, Is.EqualTo(1));
			Assert.That(dataTable.Rows[0]["LA_PK"], Is.EqualTo(LA_PK));
			Assert.That(dataTable.Rows[0]["LA_LD"], Is.EqualTo(LA_LD));
		}

		[Test]
		public void TestModified()
		{
			DataTable dataTable;

			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = "ediProd.SyncLicenceHeader";
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add(new SqlParameter("@RowState", SqlDbType.VarChar) { Value = "Added" });
					cmd.Parameters.Add(new SqlParameter("@LA_PK", SqlDbType.UniqueIdentifier) { Value = LA_PK });
					cmd.Parameters.Add(new SqlParameter("@LA_LD", SqlDbType.UniqueIdentifier) { Value = LA_LD });
					cmd.ExecuteNonQuery();
				}

				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = "ediProd.SyncLicenceHeader";
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add(new SqlParameter("@RowState", SqlDbType.VarChar) { Value = "Modified" });
					cmd.Parameters.Add(new SqlParameter("@LA_PK", SqlDbType.UniqueIdentifier) { Value = LA_PK });
					cmd.Parameters.Add(new SqlParameter("@LA_LD", SqlDbType.UniqueIdentifier) { Value = LA_LD2 });
					cmd.ExecuteNonQuery();
				}

				dataTable = BillingDataTestHelper.LoadTableByName(con, "ediProd.LicenceHeader");
			}

			Assert.That(dataTable.Rows.Count, Is.EqualTo(1));
			Assert.That(dataTable.Rows[0]["LA_PK"], Is.EqualTo(LA_PK));
			Assert.That(dataTable.Rows[0]["LA_LD"], Is.EqualTo(LA_LD2));
		}

		[Test]
		public void TestDeleted()
		{
			DataTable dataTable;

			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = "ediProd.SyncLicenceHeader";
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add(new SqlParameter("@RowState", SqlDbType.VarChar) { Value = "Added" });
					cmd.Parameters.Add(new SqlParameter("@LA_PK", SqlDbType.UniqueIdentifier) { Value = LA_PK });
					cmd.Parameters.Add(new SqlParameter("@LA_LD", SqlDbType.UniqueIdentifier) { Value = LA_LD });
					cmd.ExecuteNonQuery();
				}

				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = "ediProd.SyncLicenceHeader";
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add(new SqlParameter("@RowState", SqlDbType.VarChar) { Value = "Deleted" });
					cmd.Parameters.Add(new SqlParameter("@LA_PK", SqlDbType.UniqueIdentifier) { Value = LA_PK });
					cmd.Parameters.Add(new SqlParameter("@LA_LD", SqlDbType.UniqueIdentifier) { Value = LA_LD });
					cmd.ExecuteNonQuery();
				}

				dataTable = BillingDataTestHelper.LoadTableByName(con, "ediProd.LicenceHeader");
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
					cmd.CommandText = "ediProd.SyncLicenceHeader";
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add(new SqlParameter("@RowState", SqlDbType.VarChar) { Value = "Invalid" });
					cmd.Parameters.Add(new SqlParameter("@LA_PK", SqlDbType.UniqueIdentifier) { Value = LA_PK });
					cmd.Parameters.Add(new SqlParameter("@LA_LD", SqlDbType.UniqueIdentifier) { Value = LA_LD });

					var ex = Assert.Throws<SqlException>(() => cmd.ExecuteNonQuery());
					Assert.That(ex.Message, Does.Contain("Invalid RowState value. Must be Added, Modified, or Deleted."));
				}
			}
		}

		[Test]
		public void TestNullLA_PK()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = "ediProd.SyncLicenceHeader";
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add(new SqlParameter("@RowState", SqlDbType.VarChar) { Value = "Added" });
					cmd.Parameters.Add(new SqlParameter("@LA_PK", SqlDbType.UniqueIdentifier) { Value = DBNull.Value });
					cmd.Parameters.Add(new SqlParameter("@LA_LD", SqlDbType.UniqueIdentifier) { Value = LA_LD });

					var ex = Assert.Throws<SqlException>(() => cmd.ExecuteNonQuery());
					Assert.That(ex.Message, Does.Contain("LA_PK cannot be null."));
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
					cmd.CommandText = "ediProd.SyncLicenceHeader";
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add(new SqlParameter("@RowState", SqlDbType.VarChar) { Value = "Modified" });
					cmd.Parameters.Add(new SqlParameter("@LA_PK", SqlDbType.UniqueIdentifier) { Value = LA_PK });
					cmd.Parameters.Add(new SqlParameter("@LA_LD", SqlDbType.UniqueIdentifier) { Value = LA_LD });

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
					cmd.CommandText = "ediProd.SyncLicenceHeader";
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add(new SqlParameter("@RowState", SqlDbType.VarChar) { Value = "Deleted" });
					cmd.Parameters.Add(new SqlParameter("@LA_PK", SqlDbType.UniqueIdentifier) { Value = LA_PK });
					cmd.Parameters.Add(new SqlParameter("@LA_LD", SqlDbType.UniqueIdentifier) { Value = LA_LD });

					var ex = Assert.Throws<SqlException>(() => cmd.ExecuteNonQuery());
					Assert.That(ex.Message, Does.Contain("No record found to delete."));
				}
			}
		}
	}
}

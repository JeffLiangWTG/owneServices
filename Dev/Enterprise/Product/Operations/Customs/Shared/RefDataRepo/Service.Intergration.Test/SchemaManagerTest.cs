using NUnit.Framework;

namespace CargoWise.RefDataRepo.Service.Schema.Test
{
	/*
	* This test class is meant to keep the RefDbRepo's schema "in sync" with ZZ Reference Data. Any changes to ZZ Reference Data without 
	* making changes to RefDbRepo schema will make tests fail.
	* Please contact Tim Van
	*/
	sealed class SchemaManagerTest : TransactionedTestCase
	{
		public void TestInitialized()
		{
			AssertNotNull("Test Comment out as per request by Tim Van, This line exists to temporary resolve unused reference UT failure @Victor 20160415", new object());
			/**
			using (var connection = Db.NewAdminConnection(ZZRefDbName))
			{
				try
				{
					AssertEquals(1, connection.ExecuteScalar<int>("SELECT 1 WHERE OBJECT_ID('RefDbVersionControl', 'U') IS NULL"));
					AssertEquals(1, connection.ExecuteScalar<int>("SELECT 1 WHERE OBJECT_ID('RefCusRateVersion', 'V') IS NULL"));
					AssertEquals(1, connection.ExecuteScalar<int>("SELECT 1 WHERE OBJECT_ID('RefCusTariffVersion', 'V') IS NULL"));
					AssertEquals(1, connection.ExecuteScalar<int>("SELECT 1 WHERE OBJECT_ID('RefCusTariffRangeVersion', 'V') IS NULL"));
					AssertEquals(1, connection.ExecuteScalar<int>("SELECT 1 WHERE OBJECT_ID('RefCusTariffUOMVersion', 'V') IS NULL"));
					AssertEquals(1, connection.ExecuteScalar<int>("SELECT 1 WHERE OBJECT_ID('RefCusTariffAttributeVersion', 'V') IS NULL"));
					AssertEquals(1, connection.ExecuteScalar<int>("SELECT 1 WHERE OBJECT_ID('RefCusRateAttributeVersion', 'V') IS NULL"));
					var manager = new SchemaManager(() => ((IDbConnectionInternals)Db.NewAdminConnection(ZZRefDbName)).ADOConnection);
					Assert(manager.Initialize());
					AssertEquals(1, connection.ExecuteScalar<int>("SELECT 1 WHERE OBJECT_ID('RefDbVersionControl', 'U') IS NOT NULL"));
					AssertEquals(1, connection.ExecuteScalar<int>("SELECT 1 WHERE OBJECT_ID('RefCusRateVersion', 'V') IS NOT NULL"));
					AssertEquals(1, connection.ExecuteScalar<int>("SELECT 1 WHERE OBJECT_ID('RefCusTariffVersion', 'V') IS NOT NULL"));
					AssertEquals(1, connection.ExecuteScalar<int>("SELECT 1 WHERE OBJECT_ID('RefCusTariffRangeVersion', 'V') IS NOT NULL"));
					AssertEquals(1, connection.ExecuteScalar<int>("SELECT 1 WHERE OBJECT_ID('RefCusTariffUOMVersion', 'V') IS NOT NULL"));
					AssertEquals(1, connection.ExecuteScalar<int>("SELECT 1 WHERE OBJECT_ID('RefCusTariffAttributeVersion', 'V') IS NOT NULL"));
					AssertEquals(1, connection.ExecuteScalar<int>("SELECT 1 WHERE OBJECT_ID('RefCusRateAttributeVersion', 'V') IS NOT NULL"));
				}
				finally
				{
					connection.ExecuteNonQuery("DROP VIEW RefCusRateVersion");
					connection.ExecuteNonQuery("DROP VIEW RefCusTariffVersion");
					connection.ExecuteNonQuery("DROP VIEW RefCusTariffRangeVersion");
					connection.ExecuteNonQuery("DROP VIEW RefCusTariffUOMVersion");
					connection.ExecuteNonQuery("DROP VIEW RefCusTariffAttributeVersion");
					connection.ExecuteNonQuery("DROP VIEW RefCusRateAttributeVersion");
				}
			}
			**/
		}
	}
}

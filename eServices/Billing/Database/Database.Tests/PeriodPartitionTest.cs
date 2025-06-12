using CargoWise.eServices.Billing.Tests.Common;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Database.Tests
{
	[TestFixture]
	public class PeriodPartitionTest
	{
		[Test]
		public void TestPartitioningSetupAndSlide()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "DEF");

				BillingDataTestHelper.ExecuteStoredProcedure(con, "edi.PeriodPartitionSetupScheme");

				BillingDataTestHelper.ExecuteNonQuery(con, "exec edi.PeriodPartitionSlide 1");
				BillingDataTestHelper.ExecuteNonQuery(con, "exec edi.PeriodPartitionSlide 1");
				BillingDataTestHelper.ExecuteNonQuery(con, "exec edi.PeriodPartitionSlide 1");
				BillingDataTestHelper.ExecuteNonQuery(con, "exec edi.PeriodPartitionSlide 1");
				BillingDataTestHelper.ExecuteNonQuery(con, "exec edi.PeriodPartitionSlide 1");
				BillingDataTestHelper.ExecuteNonQuery(con, "exec edi.PeriodPartitionSlide 1");
				BillingDataTestHelper.ExecuteNonQuery(con, "exec edi.PeriodPartitionSlide 1");
				BillingDataTestHelper.ExecuteNonQuery(con, "exec edi.PeriodPartitionSlide 1");
				BillingDataTestHelper.ExecuteNonQuery(con, "exec edi.PeriodPartitionSlide 1");
				BillingDataTestHelper.ExecuteNonQuery(con, "exec edi.PeriodPartitionSlide 1");
				BillingDataTestHelper.ExecuteNonQuery(con, "exec edi.PeriodPartitionSlide 1");
				BillingDataTestHelper.ExecuteNonQuery(con, "exec edi.PeriodPartitionSlide 1");
				BillingDataTestHelper.ExecuteNonQuery(con, "exec edi.PeriodPartitionSlide 1");
			}
		}
	}
}

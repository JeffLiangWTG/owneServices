using System;
using System.Data;
using CargoWise.eServices.Billing.Tests.Common;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Database.Tests
{
	[TestFixture]
	public class GetNextPeriodTest
	{
		[TestCase(202311, TestName = "Get next period of a month before the end of a year", ExpectedResult = 202312)]
		[TestCase(202312, TestName = "Get next period of a month on the end of a year", ExpectedResult = 202401)]
		[TestCase(202401, TestName = "Get next period of a month at the beginning of a year", ExpectedResult = 202402)]
		[TestCase(202404, TestName = "Get next period of a random month", ExpectedResult = 202405)]
		public int TestGetNextPeriod(int period)
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				var sql = @"SELECT edi.GetNextPeriod(@Period)";
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandType = CommandType.Text;
					cmd.Parameters.AddWithValue("@Period", period);
					cmd.CommandText = sql;
					var result = Convert.ToInt32(cmd.ExecuteScalar());
					return result;
				}
			}
		}
	}
}

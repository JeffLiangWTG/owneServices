using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using CargoWise.eServices.Billing.Tests.Common;
using NUnit.Framework;
using API = CargoWise.Billing.API;

namespace CargoWise.eServices.Billing.Database.Tests
{
	[TestFixture]
	public class GetConfigFirstMessageWithPeriodTest
	{
		[Test]
		public void TestGetConfigFirstMessageWithPeriod()
		{
			int period = 202304;
			List<ConfigFirstMessage> cfmList = null;

			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				cfmList = GetConfigFirstMessageWithPeriod(period, con);
			}

			Assert.GreaterOrEqual(cfmList.Count(), 26, "(cfmList.Count()");

			var cfm_JPC_AFR = cfmList.Where(_ => _.FM_Category == "JPC" && _.FM_PriceItemCode == "AFR").FirstOrDefault();
			Assert.NotNull(cfm_JPC_AFR, "cfm_JPC_AFR");
			Assert.AreEqual("JPC", cfm_JPC_AFR.FM_Category, "FM_Category");
			Assert.AreEqual("AFR", cfm_JPC_AFR.FM_PriceItemCode, "FM_PriceItemCode");
			Assert.AreEqual(180, cfm_JPC_AFR.FM_Days, "FM_Days");
			Assert.AreEqual(true, cfm_JPC_AFR.FM_IncludeRef1, "FM_IncludeRef1");
			Assert.AreEqual(true, cfm_JPC_AFR.FM_IncludeRef2, "FM_IncludeRef2");
			Assert.AreEqual(false, cfm_JPC_AFR.FM_IncludeRef3, "FM_IncludeRef3");
			Assert.AreEqual(false, cfm_JPC_AFR.FM_IncludeRef4, "FM_IncludeRef4");
			Assert.AreEqual(false, cfm_JPC_AFR.FM_IncludeRef5, "FM_IncludeRef5");
			Assert.AreEqual(true, cfm_JPC_AFR.FM_IncludeCompanyNumber, "FM_IncludeCompanyNumber");
			Assert.AreEqual(202210, cfm_JPC_AFR.FM_FromPeriod, "FM_FromPeriod");
			Assert.AreEqual(202305, cfm_JPC_AFR.FM_ToPeriod, "FM_ToPeriod");
		}

		[Test]
		public void TestGetConfigFirstMessageWithPeriod_LastMonthOfYear()
		{
			int period = 202312;
			List<ConfigFirstMessage> cfmList = null;

			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.ExecuteNonQuery(con,
"delete from edi.ConfigFirstMessage where FM_Category = 'Z_Z'; " +
"insert edi.ConfigFirstMessage(FM_Category, FM_PriceItemCode, FM_Days, FM_IncludeRef1, FM_IncludeRef2) " +
"values('Z_Z', 'Z_1', 31, 1, 1);"
);

				cfmList = GetConfigFirstMessageWithPeriod(period, con);
			}

			var cfm_Z_1 = cfmList.Where(_ => _.FM_PriceItemCode == "Z_1").FirstOrDefault();

			Assert.NotNull(cfm_Z_1, "cfm_Z_1");
			Assert.AreEqual(202310, cfm_Z_1.FM_FromPeriod, "FM_FromPeriod");
			Assert.AreEqual(202401, cfm_Z_1.FM_ToPeriod, "FM_ToPeriod");
		}


		[Test]
		public void TestGetConfigFirstMessageWithPeriod_FirstMonthOfYear()
		{
			int period = 202401;
			List<ConfigFirstMessage> cfmList = null;

			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.ExecuteNonQuery(con,
"delete from edi.ConfigFirstMessage where FM_Category = 'Z_Z'; " +
"insert edi.ConfigFirstMessage(FM_Category, FM_PriceItemCode, FM_Days, FM_IncludeRef1, FM_IncludeRef2) " +
"values('Z_Z', 'Z_1', 31, 1, 1);"
);

				cfmList = GetConfigFirstMessageWithPeriod(period, con);
			}

			var cfm_Z_1 = cfmList.Where(_ => _.FM_PriceItemCode == "Z_1").FirstOrDefault();

			Assert.NotNull(cfm_Z_1, "cfm_Z_1");
			Assert.AreEqual(202312, cfm_Z_1.FM_FromPeriod, "FM_FromPeriod");
			Assert.AreEqual(202402, cfm_Z_1.FM_ToPeriod, "FM_ToPeriod");
		}

		private static List<ConfigFirstMessage> GetConfigFirstMessageWithPeriod(int period, SqlConnection con)
		{
			var result = new List<ConfigFirstMessage>();
			var sql = @"SELECT * FROM edi.GetConfigFirstMessageWithPeriod(@Period)";
			using (var cmd = con.CreateCommand())
			{
				cmd.CommandType = CommandType.Text;
				cmd.Parameters.AddWithValue("@Period", period);
				cmd.CommandText = sql;
				using (var adapter = new SqlDataAdapter(cmd))
				{
					using (var dataTable = new System.Data.DataTable())
					{
						adapter.Fill(dataTable);
						foreach (DataRow row in dataTable.Rows)
						{
							var cfm = new ConfigFirstMessage();
							cfm.FM_Category = row["FM_Category"].ToString();
							cfm.FM_PriceItemCode = row["FM_PriceItemCode"].ToString();
							cfm.FM_Days = Convert.ToInt32(row["FM_Days"]);
							cfm.FM_IncludeRef1 = Convert.ToBoolean(row["FM_IncludeRef1"]);
							cfm.FM_IncludeRef2 = Convert.ToBoolean(row["FM_IncludeRef2"]);
							cfm.FM_IncludeRef3 = Convert.ToBoolean(row["FM_IncludeRef3"]);
							cfm.FM_IncludeRef4 = Convert.ToBoolean(row["FM_IncludeRef4"]);
							cfm.FM_IncludeRef5 = Convert.ToBoolean(row["FM_IncludeRef5"]);
							cfm.FM_IncludeCompanyNumber = Convert.ToBoolean(row["FM_IncludeCompanyNumber"]);
							cfm.FM_FromPeriod = Convert.ToInt32(row["FM_FromPeriod"]);
							cfm.FM_ToPeriod = Convert.ToInt32(row["FM_ToPeriod"]);
							result.Add(cfm);
						}
					}
				}
			}
			return result;
		}

		private class ConfigFirstMessage
		{
			internal string FM_Category { get; set; }
			internal string FM_PriceItemCode { get; set; }
			internal int FM_Days { get; set; }
			internal bool FM_IncludeRef1 { get; set; }
			internal bool FM_IncludeRef2 { get; set; }
			internal bool FM_IncludeRef3 { get; set; }
			internal bool FM_IncludeRef4 { get; set; }
			internal bool FM_IncludeRef5 { get; set; }
			internal bool FM_IncludeCompanyNumber { get; set; }
			internal int FM_FromPeriod { get; set; }
			internal int FM_ToPeriod { get; set; }
		}
	}
}

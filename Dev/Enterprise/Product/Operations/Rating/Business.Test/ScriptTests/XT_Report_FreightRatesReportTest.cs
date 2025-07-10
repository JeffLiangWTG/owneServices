using System;
using System.Data;
using System.Linq;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Rating.Business.Testing;
using static Enterprise.Rating.Business.RatingConstants;

namespace Enterprise.Rating.Business.Test.ScriptTests
{
	public class XT_Report_FreightRatesReportTest : RatingTestCase
	{
		public void TestFCLModeContractNumbers()
		{
			var costing = Helper.NewCosting(TransportProvider1);
			decimal baseAmount = 100;
			foreach (var contractNumber in new[] { "", "CON2", "CON3" })
			{
				decimal amount = baseAmount;
				foreach (var containerType in new[] { "20GP", "20HC", "20PL", "20RE", "40GP", "40HC", "40PL", "40RE" })
				{
					costing.AddRateEntryWithUnitRateLine(RateCategory.FCL, Constants.RateMode.SEA, "AU", "US", "FRT", amount, QuantityUnit.CN, "AUD", containerType)
						.TI_ContractNumber = contractNumber;
					++amount;
				}
				baseAmount += 100;
			}

			Factory.Save();

			DataTable result = new DataTable();
			result.Load(GetCommand(RatingHeaderTypes.Costing, RateCategory.FCL).ExecuteReader());

			var rows = result.Rows.Cast<DataRow>().ToList();
			var actual = string.Join("; ", rows.Select(x => x["ContractNumber"].ToString() + ": " + string.Join(", ",
				((int)(decimal)x["Value_01"]).ToString(),
				((int)(decimal)x["Value_02"]).ToString(),
				((int)(decimal)x["Value_03"]).ToString(),
				((int)(decimal)x["Value_04"]).ToString(),
				((int)(decimal)x["Value_05"]).ToString(),
				((int)(decimal)x["Value_06"]).ToString(),
				((int)(decimal)x["Value_07"]).ToString(),
				((int)(decimal)x["Value_08"]).ToString())
			).OrderBy(x => x));
			AssertEquals(
				": 100, 101, 102, 103, 104, 105, 106, 107; " +
				"CON2: 200, 201, 202, 203, 204, 205, 206, 207; " +
				"CON3: 300, 301, 302, 303, 304, 305, 306, 307", actual);
		}

		CargoWise.Data.DbCommand GetCommand(string rateType, string rateCategory, string salesRepCode = "")
		{
			var command = TestConnection.Command(@"
				EXEC XT_Report_FreightRatesReport
					@CompanyPK,
					@RateType,
					@Mode,
					@OriginPK,
					@DestinationPK,
					@ServiceLevelPK,
					@CommodityCodePK,
					@OriginCountryPK,
					@DestinationCountryPK,
					@SupplierPK,
					@CarrierPK,
					@SalesRepCode");
			command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, Env.CurrentCompanyPK);
			command.AddParameter("@RateType", SqlDbType.Char, rateType);
			command.AddParameter("@Mode", SqlDbType.Char, rateCategory);
			command.AddParameter("@OriginPK", SqlDbType.UniqueIdentifier, DBNull.Value);
			command.AddParameter("@DestinationPK", SqlDbType.UniqueIdentifier, DBNull.Value);
			command.AddParameter("@ServiceLevelPK", SqlDbType.UniqueIdentifier, DBNull.Value);
			command.AddParameter("@CommodityCodePK", SqlDbType.UniqueIdentifier, DBNull.Value);
			command.AddParameter("@OriginCountryPK", SqlDbType.UniqueIdentifier, DBNull.Value);
			command.AddParameter("@DestinationCountryPK", SqlDbType.UniqueIdentifier, DBNull.Value);
			command.AddParameter("@SupplierPK", SqlDbType.UniqueIdentifier, DBNull.Value);
			command.AddParameter("@CarrierPK", SqlDbType.UniqueIdentifier, DBNull.Value);
			command.AddParameter("@SalesRepCode", SqlDbType.VarChar, salesRepCode);
			return command;
		}
	}
}

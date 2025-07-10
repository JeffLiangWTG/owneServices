using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataFixWI00214784Transformation : DataTransformation, IDataTransformationTask
	{
		public DataFixWI00214784Transformation(int version) : base(version)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public void Run(IDbTransaction trans)
		{
			var sql = @"
UPDATE r SET ZZ2_RateFormula = REPLACE(ZZ2_RateFormula,'[ME]','[SM]')  FROM RefCusTariff t  JOIN RefCusTariffAttribute ta on ta.ZZ3_ZZ1_Tariff = t.ZZ1_PK  JOIN RefCusRate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK  Where t.ZZ1_TariffCode = '213030308'  AND t.ZZ1_StartDate = '2015-01-01 00:00:00'  AND ta.ZZ3_Name = 'CheckDigit'  AND ta.ZZ3_Value = '80';
UPDATE r SET ZZ2_RateFormula = REPLACE(ZZ2_RateFormula,'[ME]','[SM]')  FROM RefCusTariff t  JOIN RefCusTariffAttribute ta on ta.ZZ3_ZZ1_Tariff = t.ZZ1_PK  JOIN RefCusRate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK  Where t.ZZ1_TariffCode = '213031108'  AND t.ZZ1_StartDate = '2015-01-01 00:00:00'  AND ta.ZZ3_Name = 'CheckDigit'  AND ta.ZZ3_Value = '88';
UPDATE r SET ZZ2_RateFormula = REPLACE(ZZ2_RateFormula,'[ME]','[SM]')  FROM RefCusTariff t  JOIN RefCusTariffAttribute ta on ta.ZZ3_ZZ1_Tariff = t.ZZ1_PK  JOIN RefCusRate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK  Where t.ZZ1_TariffCode = '213030108'  AND t.ZZ1_StartDate = '2015-01-01 00:00:00'  AND ta.ZZ3_Name = 'CheckDigit'  AND ta.ZZ3_Value = '85';
UPDATE r SET ZZ2_RateFormula = REPLACE(ZZ2_RateFormula,'[ME]','[SM]')  FROM RefCusTariff t  JOIN RefCusTariffAttribute ta on ta.ZZ3_ZZ1_Tariff = t.ZZ1_PK  JOIN RefCusRate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK  Where t.ZZ1_TariffCode = '213030208'  AND t.ZZ1_StartDate = '2014-02-14 00:00:00'  AND ta.ZZ3_Name = 'CheckDigit'  AND ta.ZZ3_Value = '82';
UPDATE r SET ZZ2_RateFormula = REPLACE(ZZ2_RateFormula,'[ME]','[SM]')  FROM RefCusTariff t  JOIN RefCusTariffAttribute ta on ta.ZZ3_ZZ1_Tariff = t.ZZ1_PK  JOIN RefCusRate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK  Where t.ZZ1_TariffCode = '213030208'  AND t.ZZ1_StartDate = '2015-01-01 00:00:00'  AND ta.ZZ3_Name = 'CheckDigit'  AND ta.ZZ3_Value = '82';
UPDATE r SET ZZ2_RateFormula = REPLACE(ZZ2_RateFormula,'[ME]','[SM]')  FROM RefCusTariff t  JOIN RefCusTariffAttribute ta on ta.ZZ3_ZZ1_Tariff = t.ZZ1_PK  JOIN RefCusRate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK  Where t.ZZ1_TariffCode = '213030108'  AND t.ZZ1_StartDate = '2015-01-01 00:00:00'  AND ta.ZZ3_Name = 'CheckDigit'  AND ta.ZZ3_Value = '81';
UPDATE r SET ZZ2_RateFormula = REPLACE(ZZ2_RateFormula,'[ME]','[SM]')  FROM RefCusTariff t  JOIN RefCusTariffAttribute ta on ta.ZZ3_ZZ1_Tariff = t.ZZ1_PK  JOIN RefCusRate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK  Where t.ZZ1_TariffCode = '213030408'  AND t.ZZ1_StartDate = '2015-01-01 00:00:00'  AND ta.ZZ3_Name = 'CheckDigit'  AND ta.ZZ3_Value = '88';
UPDATE r SET ZZ2_RateFormula = REPLACE(ZZ2_RateFormula,'[ME]','[SM]')  FROM RefCusTariff t  JOIN RefCusTariffAttribute ta on ta.ZZ3_ZZ1_Tariff = t.ZZ1_PK  JOIN RefCusRate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK  Where t.ZZ1_TariffCode = '213030608'  AND t.ZZ1_StartDate = '2015-01-01 00:00:00'  AND ta.ZZ3_Name = 'CheckDigit'  AND ta.ZZ3_Value = '87';
UPDATE r SET ZZ2_RateFormula = REPLACE(ZZ2_RateFormula,'[ME]','[SM]')  FROM RefCusTariff t  JOIN RefCusTariffAttribute ta on ta.ZZ3_ZZ1_Tariff = t.ZZ1_PK  JOIN RefCusRate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK  Where t.ZZ1_TariffCode = '213030808'  AND t.ZZ1_StartDate = '2015-01-01 00:00:00'  AND ta.ZZ3_Name = 'CheckDigit'  AND ta.ZZ3_Value = '86';
UPDATE r SET ZZ2_RateFormula = REPLACE(ZZ2_RateFormula,'[ME]','[SM]')  FROM RefCusTariff t  JOIN RefCusTariffAttribute ta on ta.ZZ3_ZZ1_Tariff = t.ZZ1_PK  JOIN RefCusRate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK  Where t.ZZ1_TariffCode = '213030308'  AND t.ZZ1_StartDate = '2015-01-01 00:00:00'  AND ta.ZZ3_Name = 'CheckDigit'  AND ta.ZZ3_Value = '83';
UPDATE r SET ZZ2_RateFormula = REPLACE(ZZ2_RateFormula,'[ME]','[SM]')  FROM RefCusTariff t  JOIN RefCusTariffAttribute ta on ta.ZZ3_ZZ1_Tariff = t.ZZ1_PK  JOIN RefCusRate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK  Where t.ZZ1_TariffCode = '213030108'  AND t.ZZ1_StartDate = '2015-01-01 00:00:00'  AND ta.ZZ3_Name = 'CheckDigit'  AND ta.ZZ3_Value = '82';
UPDATE r SET ZZ2_RateFormula = REPLACE(ZZ2_RateFormula,'[ME]','[SM]')  FROM RefCusTariff t  JOIN RefCusTariffAttribute ta on ta.ZZ3_ZZ1_Tariff = t.ZZ1_PK  JOIN RefCusRate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK  Where t.ZZ1_TariffCode = '213030108'  AND t.ZZ1_StartDate = '2015-01-01 00:00:00'  AND ta.ZZ3_Name = 'CheckDigit'  AND ta.ZZ3_Value = '84';
UPDATE r SET ZZ2_RateFormula = REPLACE(ZZ2_RateFormula,'[ME]','[SM]')  FROM RefCusTariff t  JOIN RefCusTariffAttribute ta on ta.ZZ3_ZZ1_Tariff = t.ZZ1_PK  JOIN RefCusRate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK  Where t.ZZ1_TariffCode = '213030108'  AND t.ZZ1_StartDate = '2014-02-14 00:00:00'  AND ta.ZZ3_Name = 'CheckDigit'  AND ta.ZZ3_Value = '88';
UPDATE r SET ZZ2_RateFormula = REPLACE(ZZ2_RateFormula,'[ME]','[SM]')  FROM RefCusTariff t  JOIN RefCusTariffAttribute ta on ta.ZZ3_ZZ1_Tariff = t.ZZ1_PK  JOIN RefCusRate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK  Where t.ZZ1_TariffCode = '213030908'  AND t.ZZ1_StartDate = '2015-01-01 00:00:00'  AND ta.ZZ3_Name = 'CheckDigit'  AND ta.ZZ3_Value = '80';
UPDATE r SET ZZ2_RateFormula = REPLACE(ZZ2_RateFormula,'[ME]','[SM]')  FROM RefCusTariff t  JOIN RefCusTariffAttribute ta on ta.ZZ3_ZZ1_Tariff = t.ZZ1_PK  JOIN RefCusRate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK  Where t.ZZ1_TariffCode = '213030308'  AND t.ZZ1_StartDate = '2015-01-01 00:00:00'  AND ta.ZZ3_Name = 'CheckDigit'  AND ta.ZZ3_Value = '81';
UPDATE r SET ZZ2_RateFormula = REPLACE(ZZ2_RateFormula,'[ME]','[SM]')  FROM RefCusTariff t  JOIN RefCusTariffAttribute ta on ta.ZZ3_ZZ1_Tariff = t.ZZ1_PK  JOIN RefCusRate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK  Where t.ZZ1_TariffCode = '213030308'  AND t.ZZ1_StartDate = '2015-01-01 00:00:00'  AND ta.ZZ3_Name = 'CheckDigit'  AND ta.ZZ3_Value = '89';
";

			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.Connection = trans.Connection;
				cmd.Transaction = trans;
				cmd.CommandText = sql;
				cmd.ExecuteNonQuery();
			}
		}
	}
}

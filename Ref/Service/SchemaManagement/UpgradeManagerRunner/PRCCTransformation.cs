using System;
using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class PRCCTransformation : DataTransformation, IDataTransformationTask
	{
		public PRCCTransformation(int version) : base(version)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public void Run(IDbTransaction trans)
		{
			Tuple<string, string>[] queryValues = {
				new Tuple<string,string>("460170300","1P1"),
				new Tuple<string,string>("460170302","1P1"),
				new Tuple<string,string>("317030304","1P1+12A"),
				new Tuple<string,string>("317030104","1P1+12A"),
				new Tuple<string,string>("317030504","1P1+12A"),
				new Tuple<string,string>("317030604","1P1+12A"),
				new Tuple<string,string>("317030204","1P1+12A"),
				new Tuple<string,string>("460170402","1P1+12A"),
				new Tuple<string,string>("460170206","1P1+12A"),
				new Tuple<string,string>("460170204","1P1+12A"),
				new Tuple<string,string>("460170304","1P1+12A")
			};

			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.Connection = trans.Connection;
				cmd.Transaction = trans;
				cmd.CommandText = @"IF (SELECT count(1) FROM RefCusTariffRule WHERE ZZ1_TariffCode = @TariffCode) = 0 
BEGIN 
INSERT INTO RefCusTariffRule (ZZ1_PK,ZZ1_TariffCode,ZZ1_ZZZ_NKDataGrouping,ZZ1_Applied) VALUES (newid(),@TariffCode,'ZA',0) 
END 
ELSE
BEGIN
UPDATE TR 
	SET ZZ1_Applied = 0 
FROM RefCusTariffRule TR 
WHERE ZZ1_TariffCode = @TariffCode
END
IF (SELECT COUNT(t.ZZ1_PK) FROM RefCusTariffRule t LEFT JOIN RefCusRateRule r ON t.ZZ1_PK = r.ZZ2_ZZ1_Tariff WHERE ZZ1_TariffCode = @TariffCode AND r.ZZ2_PK is null) <> 0 
BEGIN 
INSERT INTO RefCusRateRule 
(ZZ2_ZZ1_Tariff,ZZ2_RateFormula,ZZ2_SelectorFormula,ZZ2_ZZZ_NKDataGrouping) 
SELECT t.ZZ1_PK,@RateFormula,'','ZA'
FROM RefCusTariffRule t 
LEFT JOIN RefCusRateRule r ON t.ZZ1_PK = r.ZZ2_ZZ1_Tariff
WHERE ZZ1_TariffCode = @TariffCode
AND r.ZZ2_PK is null
END
UPDATE RefCusRateRule 
SET ZZ2_RateFormula = @RateFormula
WHERE ZZ2_ZZ1_Tariff in (SELECT ZZ1_PK FROM RefCusTariffRule WHERE ZZ1_TariffCode = @TariffCode)
";
				foreach (var valueSet in queryValues)
				{
					var tariffCodeParameter = cmd.CreateParameter();
					tariffCodeParameter.ParameterName = "TariffCode";
					tariffCodeParameter.Value = valueSet.Item1;
					tariffCodeParameter.DbType = DbType.String;
					cmd.Parameters.Add(tariffCodeParameter);
					var rateFormulaParameter = cmd.CreateParameter();
					rateFormulaParameter.ParameterName = "RateFormula";
					rateFormulaParameter.Value = valueSet.Item2;
					rateFormulaParameter.DbType = DbType.String;
					cmd.Parameters.Add(rateFormulaParameter);
					cmd.ExecuteNonQuery();
					cmd.Parameters.Clear();
				}
			}
		}
	}
}

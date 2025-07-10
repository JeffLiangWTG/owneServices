using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataFixExchangeRateZWD : DataTransformation, IDataTransformationTask
	{
		public DataFixExchangeRateZWD(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = $@"
IF NOT EXISTS( select Top 1 ZZN_PK
From RefExchangeRateZZ
WHERE ZZN_RN_NKCountry in ('ZA', 'NA', 'LS', 'SZ')
AND ZZN_RX_NKExCurrency = 'ZWL')
Begin
	INSERT INTO RefExchangeRateZZ
	(ZZN_PK,ZZN_ExRateType,ZZN_StartDate,ZZN_EndDate,ZZN_Rate,ZZN_RX_NKExCurrency,ZZN_RN_NKCountry)
	Select newid(),ZZN_ExRateType,ZZN_StartDate,ZZN_EndDate,ZZN_Rate,'ZWL' ,ZZN_RN_NKCountry
	From RefExchangeRateZZ
	WHERE ZZN_RN_NKCountry in ('ZA', 'NA', 'LS', 'SZ')
	AND ZZN_RX_NKExCurrency = 'ZWD'
End

UPDATE vc
Set vc.RVC_Deleted = 1, vc.RVC_IsPublished = 0
from RefExchangeRateZZ ex
join RefDbVersionControl vc on vc.RVC_ParentCode = 'ZZN' and vc.RVC_ParentPK = ex.ZZN_PK
WHERE ZZN_RN_NKCountry in ('ZA', 'NA', 'LS', 'SZ')
AND ZZN_RX_NKExCurrency = 'ZWD'
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

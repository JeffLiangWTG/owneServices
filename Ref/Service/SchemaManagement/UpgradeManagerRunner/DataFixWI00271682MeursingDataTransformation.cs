using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataFixWI00271682MeursingDataTransformation : DataTransformation, IDataTransformationTask
	{
		public DataFixWI00271682MeursingDataTransformation(int version) : base(version)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public void Run(IDbTransaction trans)
		{
			var sql = @"
--Update rateCode
Update r set r.ZZ2_ZY1_RateCode = case when tg.ZZA_TradeGroup = '1011' then rc.ErgoPK else rc.OtherPK end
from RefCusRate r
join (
Select rcErgo.ZY1_RateCode as ErgoRateCode, rcErgo.ZY1_PK as ErgoPK,rcOther.ZY1_RateCode as OtherRateCode, rcOther.ZY1_PK as OtherPK
from RefCusRateCode rcErgo 
join  RefCusRateCode rcOther on rcErgo.ZY1_RateCode + 'R' = rcOther.ZY1_RateCode
where rcErgo.ZY1_RateCode in ('EA', 'ADFM', 'ADSZ')
) rc on r.ZZ2_ZY1_RateCode in (rc.ErgoPK, rc.OtherPK)
join RefCusApplicability ap on ap.ZZT_ZZ2_Rate = r.ZZ2_PK
join RefCusTradeGroup tg on tg.ZZA_PK = ap.ZZT_ZZA_TradeGroup
Where r.ZZ2_ZZZ_NKDataGrouping = 'EUN';

--Remove order numbers from RefCusApplicability
update ap set ap.ZZT_OrderNumber = '' 
from RefCusRate r
join RefCusRateCode rc on rc.ZY1_RateCode in ('EA', 'ADFM', 'ADSZ','EAR', 'ADFMR', 'ADSZR') and rc.ZY1_PK = r.ZZ2_ZY1_RateCode
join RefCusApplicability ap on ap.ZZT_ZZ2_Rate = r.ZZ2_PK
Where r.ZZ2_ZZZ_NKDataGrouping = 'EUN'
and ap.ZZT_OrderNumber <> '';
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

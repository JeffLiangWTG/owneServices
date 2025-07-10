using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataFixHLTConditionalRateFormulaEUNTransformation : DataTransformation, IDataTransformationTask
	{
		public DataFixHLTConditionalRateFormulaEUNTransformation(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = $@"
update r 
	set ZZ2_RateFormula = Replace(zz2_rateformula,'If(VFD/[DTN]','If(VFD/[HLT]')
from 
	refcusrate r
	join refcustariff t 
		on t.zz1_pk = r.zz2_zz1_tariff
Where 
	zz2_zzz_nkdatagrouping = 'eun'
	and zz2_rateformula like 'if(VFD/[[]DTN]%* [[]HLT]%'
	and zz2_startdate <= getutcdate() and zz2_enddate >= getutcdate()";

			DbHelper.ExecuteNonQuery(trans, sql);
		}
	}
}

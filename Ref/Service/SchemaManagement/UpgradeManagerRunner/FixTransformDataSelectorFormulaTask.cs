using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class FixTransformDataSelectorFormulaTask : DataTransformation, IDataTransformationTask
	{
		public FixTransformDataSelectorFormulaTask(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sqlText = @"
delete a
from RefCusRate r
join RefCusApplicability a on a.ZZT_ZZ2_Rate = r.ZZ2_PK
join RefCusTradeGroup tg on tg.ZZA_PK = a.ZZT_ZZA_TradeGroup
where r.ZZ2_SelectorFormula = 'pp=''MERCOSUR'''
and tg.ZZA_TradeGroup <> 'MERCOSUR'
";
			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.Transaction = trans;
				cmd.CommandText = sqlText;
				cmd.ExecuteNonQuery();
			}
		}
	}
}

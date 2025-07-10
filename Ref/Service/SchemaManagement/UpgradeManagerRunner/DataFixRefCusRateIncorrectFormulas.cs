using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataFixRefCusRateIncorrectFormulas : DataTransformation, IDataTransformationTask
	{
		public DataFixRefCusRateIncorrectFormulas(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = $@"
UPDATE RefCusRate SET ZZ2_RateFormula = 'MIN(VFD * 0.055 + #EAR(1)#, VFD * 0.155 +#ADSZR(1)#)' WHERE ZZ2_RateFormula = 'MIN(VFD * 0.055 + #EAR(1)#, 1VFD * 0.055 +#ADSZR(1)#)'
UPDATE RefCusRate SET ZZ2_RateFormula = 'MIN(VFD * 0.055 + #EAR(2)#, VFD * 0.155 +#ADFMR(1)#)' WHERE ZZ2_RateFormula = 'MIN(VFD * 0.055 + #EAR(2)#, 1VFD * 0.055 +#ADFMR(2)#)'
";
			DbHelper.ExecuteNonQuery(trans, sql);
		}
	}
}

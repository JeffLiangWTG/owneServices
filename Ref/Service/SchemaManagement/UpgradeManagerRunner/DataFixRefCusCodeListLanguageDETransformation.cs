using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataFixRefCusCodeListLanguageDETransformation : DataTransformation, IDataTransformationTask
	{
		public DataFixRefCusCodeListLanguageDETransformation(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = $@"
UPDATE s
SET s.ZXA_ZX6_NKLanguage = 'DE'
FROM RefCusCodeListLanguage s
LEFT JOIN RefCusCodeListLanguage t ON t.ZXA_ZX6_NKLanguage = 'DE' AND t.ZXA_ZZD_CodeList = s.ZXA_ZZD_CodeList
WHERE t.ZXA_PK is null AND s.ZXA_ZX6_NKLanguage = 'GRM'

DELETE FROM RefCusCodeListLanguage WHERE ZXA_ZX6_NKLanguage = 'GRM'";

			DbHelper.ExecuteNonQuery(trans, sql);
		}
	}
}

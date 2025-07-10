using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class PopulateRefDocOrgCusCodeTransformation : DataTransformation, IDataTransformationTask
	{
		public PopulateRefDocOrgCusCodeTransformation(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = @"
UPDATE 
	RefDocOrgCusCode
SET
	DOC_Direction = CASE 
	WHEN DOC_RN_NKRegulatingCountry IN ('AT', 'BE', 'BG', 'CY', 'CZ', 'DE', 'DK', 'EE', 'ES', 'FI', 'FR', 'GR', 'HR', 'HU', 'IE', 'IT', 'LT', 'LU', 'LV', 'MC', 'MT', 'NL', 'PL', 'PT', 'RO', 'SE', 'SI', 'SK', 'CH', 'NO') 
		AND DOC_RN_NKCodeCountry = DOC_RN_NKRegulatingCountry 
		AND DOC_CodeType = 'EOR' 
		AND DOC_DocumentType = 'ESI' 
	THEN 'IMP'
	ELSE 'BTH' END
";
			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.Transaction = trans;
				cmd.CommandText = sql;
				cmd.ExecuteNonQuery();
			}
		}
	}
}

using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class WI00220678Transformation : DataTransformation, IDataTransformationTask
	{
		public WI00220678Transformation(int version) : base(version) { }

		public void Run(IDbTransaction trans)
		{
			var sql = @"
UPDATE RefCusApplicability Set ZZT_ZZA_NKTradeGroup = 'STANDARD'
Where ZZT_ZZA_ZZZ_NKDataGrouping = 'ZA'
And ZZT_ZZA_NKTradeGroup = '';

UPDATE dpi set DPI_Status = 'QUE'
FROM DataProcessingInformation dpi
JOIN SourceData sd 
ON sd.SDA_PK = DPI_SourceId 
AND sd.SDA_Contenttype = 'pro'
AND sd.SDA_Status = 'PRS'
WHERE dpi.DPI_Status= 'ERR';
";
			DbHelper.ExecuteNonQuery(trans, sql, 0);
		}
	}
}

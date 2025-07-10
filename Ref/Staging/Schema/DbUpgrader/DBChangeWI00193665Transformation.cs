using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class DBChangeWI00193665Transformation : DataTransformation, IDataTransformationTask
	{
		public DBChangeWI00193665Transformation(int version) : base(version) { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public void Run(IDbTransaction trans)
		{
			var sql = @"
	INSERT INTO DataProcessingInformation 
	(DPI_ID,DPI_Status,DPI_Message,DPI_SourceId,DPI_ParentTableCode,DPI_ParentPk)
	SELECT SRC_PK,CASE WHEN RIGHT(SRC_TransactionType,5) = 'Error' THEN 'ERR' ELSE 'QUE' END,SRC_TransactionType,sd.SourcePK,'ZZ1',SRC_ZZ1_PK
	FROM RefTariffSource ts
	JOIN (Select max(SDA_PK) as SourcePK,SDA_SourceTime From SourceData Where SDA_ContentType = 'PRO' Group By SDA_SourceTime) sd 
	ON sd.SDA_SourceTime = ts.SRC_SourceDate
	left join DataProcessingInformation on DPI_ID = SRC_PK
	Where DPI_ID is null
";
			DbHelper.ExecuteNonQuery(trans, sql, 0);
		}
	}
}

using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class WI00473541Transformation : DataTransformation, IDataTransformationTask
	{
		public WI00473541Transformation(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = @"
INSERT INTO DataSourceInformation (DSI_PK, DSI_SubSource, DSI_EnableAutoExpiration)
SELECT NEWID(), 'Export ' + DSI_SubSource, '1' FROM DataSourceInformation WHERE DSI_SubSource like 'EUN Tariffs chapters%';
UPDATE DataSourceInformation SET DSI_SubSource='Import ' + DSI_SubSource WHERE DSI_SubSource like 'EUN Tariffs chapters%';

DECLARE @i INT
SET @i = 0
WHILE @i < 10
BEGIN
	DECLARE @source VARCHAR(50) = CONCAT('EUN Tariffs chapters ', @i, '0-', @i, '9');
	DELETE A FROM DataProcessingResult A
	INNER JOIN DataProcessingResult B ON A.DPR_ParentPK = B.DPR_ParentPK
	WHERE A.DPR_SubSource=@source AND B.DPR_SubSource='Import ' + @source
	AND A.DPR_PublicationTime < B.DPR_PublicationTime;
	SET @i = @i + 1;
END

UPDATE DataProcessingResult SET DPR_SubSource='Import ' + DPR_SubSource WHERE DPR_SubSource like 'EUN Tariffs chapters%';
";
			DbHelper.ExecuteNonQuery(trans, sql, 0);
		}
	}
}

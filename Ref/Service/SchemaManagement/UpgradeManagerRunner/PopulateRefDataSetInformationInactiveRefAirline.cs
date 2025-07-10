using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class PopulateRefDataSetInformationInactiveRefAirline : DataTransformation, IDataTransformationTask
	{
		public PopulateRefDataSetInformationInactiveRefAirline(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = @"
IF (SELECT COUNT(*) FROM RefDataSetInformation WHERE RDS_TableName = 'RefAirline' and RDS_DataSetName <> 'RefAirline') = 0
BEGIN
	INSERT RefDataSetInformation(RDS_PK, RDS_DataSetId, RDS_DataSetName, RDS_TableName, RDS_DataSetTableCode, RDS_PriorityLevel)
	VALUES(newid(), 202, 'InactiveRefAirline', 'RefAirline', 'RM', 1);

-- Inactive RefAirline DataSet.
	INSERT INTO RefDataSetInformationDefinition (RDD_PK, RDD_DataSetId, RDD_ColumnName, RDD_ColumnValue)
	VALUES (newid(), 202, 'RM_IsActive','0');

--Cater for existing InactiveRefAirline records.
	UPDATE rvc SET rvc.RVC_DataSetId = 202
		FROM RefDbVersionControl rvc
		JOIN RefAirline air ON rvc.RVC_ParentPK = air.RM_PK
		WHERE air.RM_IsActive = 0; 

	UPDATE RefDataSetInformation SET RDS_LastUpdatedUTC = (SELECT MAX(RVC_LastUpdatedUTC)
		FROM RefDbVersionControl WHERE RVC_DataSetId = 202) WHERE RDS_DataSetName = 'InactiveRefAirline';

END
";

			DbHelper.ExecuteNonQuery(trans, sql, 300);
		}
	}
}

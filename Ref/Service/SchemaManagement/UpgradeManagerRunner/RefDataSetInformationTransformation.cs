using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public abstract class RefDataSetInformationTransformation : DataTransformation, IDataTransformationTask
	{
		protected RefDataSetInformationTransformation(int version) : base(version)
		{
		}

		public abstract int DataSetId { get; }
		public abstract string TableName { get; }
		public abstract string DataSetName { get; }
		public abstract string TableCode { get; }

		public void Run(IDbTransaction trans)
		{
			var sql = $@"
IF (SELECT COUNT(*) FROM RefDataSetInformation WHERE RDS_TableName = '{TableName}') = 0
BEGIN
	INSERT RefDataSetInformation(RDS_PK, RDS_DataSetId, RDS_DataSetName, RDS_TableName, RDS_DataSetTableCode, RDS_PriorityLevel)
	VALUES(newid(), {DataSetId}, '{TableName}', '{TableName}', '{TableCode}', 0)
END
";

			DbHelper.ExecuteNonQuery(trans, sql);
		}
	}
}

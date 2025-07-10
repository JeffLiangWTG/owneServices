using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class PopulateDataChangeHistoryForExistingRecords : DataTransformation, IDataTransformationTask
	{
		public PopulateDataChangeHistoryForExistingRecords(int version) : base(version)
		{ }

		public void Run(IDbTransaction trans)
		{
			var sql = @"
IF NOT EXISTS (SELECT TOP 1 1 FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZAT')
BEGIN
	DECLARE @sourcePK1 SourcePKList;
	INSERT @sourcePK1
	SELECT ZAT_PK, 'ZAT' FROM RefAccTaxRate

	EXEC dbo.RecordDataSetChangeHistory @sourcePK1
END
IF NOT EXISTS (SELECT TOP 1 1 FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZZD')
BEGIN
	DECLARE @sourcePK2 SourcePKList;
	INSERT @sourcePK2
	SELECT ZZD_PK, 'ZZD' FROM RefCusCodeList

	EXEC dbo.RecordDataSetChangeHistory @sourcePK2
END
";
			DbHelper.ExecuteNonQuery(trans, sql);
		}
	}
}

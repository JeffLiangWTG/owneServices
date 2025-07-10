using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	class DataFixWI00196253Transformation : DataTransformation, IDataTransformationTask
	{
		public DataFixWI00196253Transformation(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = @"
IF (SELECT COUNT(*) FROM RefCusTaxOrFeeType) = 0
BEGIN
	DELETE FROM RefDbVersionControl WHERE ParentCode = 'ZZF';

	INSERT INTO RefCusTaxOrFeeType(ZX0_PK, ZX0_Description, ZX0_TaxOrFeeType)
	VALUES(NEWID(), 'Other', 'OTH');

	UPDATE RefCusTaxOrFee SET ZZF_ZX0_NKTaxOrFeeType = 'OTH';
END
";

			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.Connection = trans.Connection;
				cmd.Transaction = trans;
				cmd.CommandText = sql;
				cmd.ExecuteNonQuery();
			}
		}
	}
}

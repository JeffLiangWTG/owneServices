using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataFixWI00212900Transformation : DataTransformation, IDataTransformationTask
	{
		public DataFixWI00212900Transformation(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sqlText = @"
IF((SELECT COUNT(*) FROM RefCusTaxOrFeeType WHERE ZX0_TaxOrFeeType = 'DLT' AND ZX0_Description = 'Delete') = 0)
BEGIN
	DECLARE @TypePK UNIQUEIDENTIFIER = NEWID()
	INSERT INTO RefCusTaxOrFeeType (ZX0_PK, ZX0_TaxOrFeeType, ZX0_Description)
	VALUES(@TypePK, 'DLT', 'Delete')

	UPDATE RefCusTaxOrFee SET ZZF_ZX0_NKTaxOrFeeType = 'DLT' WHERE ZZF_ZZZ_NKDataGrouping = 'GB' AND ISNUMERIC(ZZF_Code) = 0
	UPDATE RefCusTaxOrFee SET ZZF_ZX0_NKTaxOrFeeType = 'DLT' WHERE ZZF_ZZZ_NKDataGrouping = 'SG' AND ZZF_Code = 'VAT'

	UPDATE RefDbVersionControl SET Deleted = 1, LastUpdatedUTC = SYSUTCDATETIME() WHERE ParentPK = @TypePK AND ParentCode = 'ZX0'
END
";
			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.Transaction = trans;
				cmd.CommandText = sqlText;
				cmd.ExecuteNonQuery();
			}
		}
	}
}

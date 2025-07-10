using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataFixWI00190456Transformation : DataTransformation, IDataTransformationTask
	{
		public DataFixWI00190456Transformation(int version) : base(version)
		{ }

		public void Run(IDbTransaction trans)
		{
			var sql = @"
UPDATE t
SET t.ZZU_ZZD_CodeList = c.ZZD_PK, t.ZZU_ZZE_Attribute = NULL
FROM RefCusCodeList  c
JOIN RefCusCodeListAttribute a ON a.ZZE_ZZD_CodeList = c.ZZD_PK AND c.ZZD_ZZK_NKCodeType = 'CUSOF' AND c.ZZD_ZZZ_NKDataGrouping = 'US'
JOIN RefCusCodeOrAttributeTransportMode t ON t.ZZU_ZZE_Attribute = a.ZZE_PK
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

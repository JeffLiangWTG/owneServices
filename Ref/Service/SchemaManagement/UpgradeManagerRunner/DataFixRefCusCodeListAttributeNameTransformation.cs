using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataFixRefCusCodeListAttributeNameTransformation : DataTransformation, IDataTransformationTask
	{
		public DataFixRefCusCodeListAttributeNameTransformation(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = $@"UPDATE RefCusCodeListAttributeName SET ZXE_ZZK_NKCodeTypeForValueList = NULL WHERE ZXE_ZZK_NKCodeTypeForValueList = ''";

			DbHelper.ExecuteNonQuery(trans, sql);
		}
	}
}

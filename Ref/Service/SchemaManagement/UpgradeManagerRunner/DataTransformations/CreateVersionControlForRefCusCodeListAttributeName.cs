using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataTransformations
{
	public class CreateVersionControlForRefCusCodeListAttributeName : DataTransformation, IDataTransformationTask
	{
		public CreateVersionControlForRefCusCodeListAttributeName(int version) : base(version)
		{ }

		public void Run(IDbTransaction trans)
		{
			var sql = @"
IF NOT EXISTS (SELECT TOP 1 1 FROM RefDbVersionControl WHERE RVC_ParentCode = 'ZXE')
BEGIN
	INSERT RefDbVersionControl (RVC_ParentPK, RVC_ParentCode)
	SELECT ZXE_PK, 'ZXE'
	FROM RefCusCodeListAttributeName
END
";
			DbHelper.ExecuteNonQuery(trans, sql);
		}
	}
}

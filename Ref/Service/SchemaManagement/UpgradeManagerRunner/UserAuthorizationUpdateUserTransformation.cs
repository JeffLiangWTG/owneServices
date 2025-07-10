using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class UserAuthorizationUpdateUserTransformation : DataTransformation, IDataTransformationTask
	{
		public UserAuthorizationUpdateUserTransformation(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = @"
UPDATE dbo.UserAuthorization
SET UA_User = REPLACE(UA_User, 'CORP\', 'WTG.')
WHERE UA_DataSetName != 'Quartz' AND UA_User LIKE 'CORP\%';
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

using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class UserAuthorizationUpdateUserTransformationFixture : TransformationFixture
	{

		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = "Select UA_User from dbo.UserAuthorization Where UA_DataSetName = 'TSW Export Permit'";
				Assert.That(cmd.ExecuteScalar(), Is.EqualTo("WTG.Jas.Ted"));
				cmd.CommandText = "Select UA_User from dbo.UserAuthorization Where UA_TableName = 'RefShippingLineMessagingRequirement'";
				Assert.That(cmd.ExecuteScalar(), Is.EqualTo("WTG.Asd.Edf"));
				cmd.CommandText = "Select UA_User from dbo.UserAuthorization Where UA_TableName = 'RefCusCodeListAttributeUserView'";
				Assert.That(cmd.ExecuteScalar(), Is.EqualTo("WTG.Tsd.Sdf"));
				cmd.CommandText = "Select UA_User from dbo.UserAuthorization Where UA_TableName = 'Reference Data'";
				Assert.That(cmd.ExecuteScalar(), Is.EqualTo("PROD\\Odf.Gjk"));
				cmd.CommandText = "Select UA_User from dbo.UserAuthorization Where UA_TableName = 'RefCusProcedureAttributeUserView'";
				Assert.That(cmd.ExecuteScalar(), Is.EqualTo("WTG.Pad.Ion"));
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new UserAuthorizationUpdateUserTransformation(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
INSERT INTO dbo.UserAuthorization (UA_User, UA_DataSetName, UA_TableName, UA_ColumnName, UA_ColumnValue)
VALUES
('corp\Jas.Ted', 'TSW Export Permit', 'RefCusCodeListUserView', 'ZZD_CodeType', 'TEPRM'),
('CORP\Asd.Edf', 'Ref Shipping Line', 'RefShippingLineMessagingRequirement', '', ''),
('CORP\Tsd.Sdf', 'ZA FAC', 'RefCusCodeListAttributeUserView', 'ZZE_CodeType', 'FAC'),
('PROD\Odf.Gjk', 'Quartz', 'Reference Data', '', ''),
('CORP\Pad.Ion', 'CDS Procedure Codes', 'RefCusProcedureAttributeUserView', 'ZXB_CountryOrGrouping', 'CDS');
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}

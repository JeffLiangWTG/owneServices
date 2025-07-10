using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader.Test
{
	class UpdateRefApplicationAttributeTransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			var sql = @"SELECT RAA_JobGroup FROM dbo.RefApplicationAttribute WHERE RAA_ConfigFilePath = 'CargoWise.RefDbRepo.DoesNotExist.exe.config'";
			Assert.AreEqual(string.Empty, (string)DbHelper.ExecuteScalar(Transaction, sql));
			sql = @"SELECT RAA_JobGroup FROM dbo.RefApplicationAttribute WHERE RAA_ConfigFilePath = 'CargoWise.RefDbRepo.UniversalXMLProducers.RefAirlineProducer.exe.config'";
			Assert.AreEqual("Air Team", (string)DbHelper.ExecuteScalar(Transaction, sql));
			sql = @"SELECT RAA_JobGroup FROM dbo.RefApplicationAttribute WHERE RAA_ConfigFilePath = 'CargoWise.RefDbRepo.AUReferenceData.CmdLine.config.json'";
			Assert.AreEqual("AU Customs", (string)DbHelper.ExecuteScalar(Transaction, sql));
		}

		protected override IDataTransformationTask GetTask()
		{
			return new UpdateRefApplicationAttributeTransformation(0);
		}

		protected override void PrepareTestData()
		{
			var sql = @"INSERT INTO dbo.RefApplicationAttributeType (RAT_PK, RAT_Type, RAT_Description) VALUES
('11CEB019-F838-490D-96CC-14FBA9638415', 'Number', 'number type'),
('AC70B968-1295-41C3-A191-2CEC98CFF97D', 'String', 'string type');

INSERT INTO dbo.RefApplicationAttribute (RAA_PK, RAA_ConfigFilePath, RAA_AttributeName, RAA_Value, RAA_RAT_NKType) VALUES
('166FAC00-CDA4-11EB-BB40-63F393E1CC86', 'CargoWise.RefDbRepo.UniversalXMLProducers.RefAirlineProducer.exe.config', 'IssueReportingExitCode', '7', 'Number'),
('28109C80-CDA4-11EB-BB40-63F393E1CC86', 'CargoWise.RefDbRepo.AUReferenceData.CmdLine.config.json', 'OutputFilePath', 'UxmlFiles', 'String'),
('E5D955AA-6BD6-440C-BBEC-4534563EEEB8', 'CargoWise.RefDbRepo.DoesNotExist.exe.config', 'FileName', 'AAA.txt', 'String');
";
			DbHelper.ExecuteNonQuery(Transaction, sql);
		}
	}
}

using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataTransformations;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class CreateVersionControlForRefCusCodeListAttributeNameFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"Select Count(1) from RefDbVersionControl WHERE RVC_ParentCode = 'ZXE' ";
				Assert.AreEqual(cmd.ExecuteScalar(), 1);
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new CreateVersionControlForRefCusCodeListAttributeName(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"DROP TRIGGER RefCusCodeListAttributeName_Version_Create;";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
				cmd.CommandText = @"
INSERT RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_NKGrouping)
VALUES('595F5657-9AC9-4381-B990-F37D1940FCBE', 'ZA', 'South Africa', NULL);

INSERT RefCusCodeType(ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_ZZZ_NKDataGrouping)
VALUES(N'6a10b0de-d88e-44ec-b640-9d05bbb48d8b', N'ADDIN', N'Additional Information', 'ZA');

INSERT RefCusCodeListAttributeName(ZXE_PK, ZXE_Name, ZXE_Description, ZXE_ZZK_NKCodeType, ZXE_ZZZ_NKDataGrouping)
VALUES('FBC8C291-1357-49B0-BBE6-B1893EA78D7E', 'Export', 'Desc.', 'ADDIN', 'ZA')
				";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}

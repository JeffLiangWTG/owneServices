using System;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class DataFixRefCusCodeListAttributeNameTransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			var sql = "SELECT COUNT(1) FROM RefCusCodeListAttributeName WHERE ZXE_ZZK_NKCodeTypeForValueList = ''";
			using (var cmd = Transaction?.Connection?.CreateCommand())
			{
				Assert.AreEqual(0, DbHelper.ExecuteScalar(Transaction, sql));
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DataFixRefCusCodeListAttributeNameTransformation(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
IF EXISTS (SELECT * FROM sysobjects WHERE name = 'FK_RefCusCodeListAttributeName_RefCusCodeType' AND type='F' AND parent_obj = OBJECT_ID(N'[dbo].[RefCusCodeListAttributeName]'))
ALTER TABLE [dbo].[RefCusCodeListAttributeName] DROP CONSTRAINT [FK_RefCusCodeListAttributeName_RefCusCodeType];

IF EXISTS (SELECT * FROM sysobjects WHERE name = 'FK_RefCusCodeListAttributeName_RefCusCodeType_ValueList' AND type='F' AND parent_obj = OBJECT_ID(N'[dbo].[RefCusCodeListAttributeName]'))
ALTER TABLE [dbo].[RefCusCodeListAttributeName] DROP CONSTRAINT [FK_RefCusCodeListAttributeName_RefCusCodeType_ValueList];

INSERT INTO RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES (NEWID(), 'ZA', 'South Africa');

INSERT INTO RefCusCodeType (ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_IsReadOnly, ZZK_ZZZ_NKDataGrouping)
VALUES (NEWID(), 'TEST', 'X' , 1, 'ZA');

INSERT INTO RefCusCodeListAttributeName (ZXE_PK, ZXE_Name, ZXE_Description, ZXE_ZZK_NKCodeType, ZXE_ZZZ_NKDataGrouping, ZXE_ZZK_NKCodeTypeForValueList)
VALUES (NEWID(), 'Name1', 'Desc.', 'TEST', 'ZA', 'TEST'),
(NEWID(), 'Name2', 'Desc.', 'TEST', 'ZA', NULL),
(NEWID(), 'Name3', 'Desc.', 'TEST', 'ZA', '');
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}

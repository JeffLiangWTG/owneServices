using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class DataFixWI00212326TransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"SELECT COUNT(*) FROM RefDbVersionControl WHERE RVC_ParentCode = 'ZZD'
AND RVC_ParentPK IN ('DCEDF991-FDF6-4880-9EDB-23B6EAD313B5', 'CBF3264B-542A-45F7-8BF9-2B35040DEBC9', 'ED942838-31CC-47EB-8220-4DB05CA7D070') AND RVC_Deleted = 1";
				Assert.AreEqual(2, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DataFixWI00212326Transformation(29);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
INSERT INTO RefDataGrouping (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
VALUES ('ED0CE102-2C22-4995-9F99-8D5825517E52','AU','AU','AU');

INSERT INTO RefCusCodeType(ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_IsReadonly, ZZK_ZZZ_NKDataGrouping)
VALUES(NEWID(), 'AQISP', 'EXDOCS Quarantine Place Code', 1, 'AU')

insert into RefCusCodeListAttributeName (ZXE_PK, ZXE_Name, ZXE_Description, ZXE_ZZK_NKCodeType, ZXE_ZZZ_NKDataGrouping)
values (newid(), 'Testing', 'Testing', 'AQISP', 'AU')

INSERT INTO RefCusCodeList(ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_ZZZ_NKDataGrouping, ZZD_StartDate, ZZD_EndDate)
VALUES('DCEDF991-FDF6-4880-9EDB-23B6EAD313B5', 'AQISP', 'TST', 'Testing1', 'AU', '2018-04-09 00:00:00', '2018-07-23 00:00:00')
INSERT INTO RefCusCodeList(ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_ZZZ_NKDataGrouping, ZZD_StartDate, ZZD_EndDate)
VALUES('CBF3264B-542A-45F7-8BF9-2B35040DEBC9', 'AQISP', 'CD2', 'Testing2', 'AU', '2018-04-09 00:00:00', '2018-07-22 23:59:00')
INSERT INTO RefCusCodeList(ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_ZZZ_NKDataGrouping, ZZD_StartDate, ZZD_EndDate)
VALUES('ED942838-31CC-47EB-8220-4DB05CA7D070', 'AQISP', 'CD3', 'Testing3', 'AU', '2018-04-09 00:00:00', '2018-07-22 23:59:00')


INSERT INTO RefCusCodeListAttribute(ZZE_PK, ZZE_Value, ZZE_ZXE_NKName, ZZE_ZZD_CodeList)
VALUES(NEWID(), 'ANY', 'Testing', 'DCEDF991-FDF6-4880-9EDB-23B6EAD313B5')
INSERT INTO RefCusCodeListAttribute(ZZE_PK, ZZE_Value, ZZE_ZXE_NKName, ZZE_ZZD_CodeList)
VALUES(NEWID(), 'ANY2', 'Testing', 'DCEDF991-FDF6-4880-9EDB-23B6EAD313B5')

INSERT INTO RefCusCodeListAttribute(ZZE_PK, ZZE_Value, ZZE_ZXE_NKName, ZZE_ZZD_CodeList)
VALUES(NEWID(), 'ANY3', 'Testing', 'CBF3264B-542A-45F7-8BF9-2B35040DEBC95')
INSERT INTO RefCusCodeListAttribute(ZZE_PK, ZZE_Value, ZZE_ZXE_NKName, ZZE_ZZD_CodeList)
VALUES(NEWID(), '       ', 'Testing', 'CBF3264B-542A-45F7-8BF9-2B35040DEBC9')

INSERT INTO RefCusCodeListAttribute(ZZE_PK, ZZE_Value, ZZE_ZXE_NKName, ZZE_ZZD_CodeList)
VALUES(NEWID(), 'ANY4', 'Testing', 'ED942838-31CC-47EB-8220-4DB05CA7D070')
INSERT INTO RefCusCodeListAttribute(ZZE_PK, ZZE_Value, ZZE_ZXE_NKName, ZZE_ZZD_CodeList)
VALUES(NEWID(), '       ', 'Testing', 'ED942838-31CC-47EB-8220-4DB05CA7D070')
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}

using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class DataFixWI00190456TransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"Select Count(1) from RefCusCodeOrAttributeTransportMode Where ZZU_ZZE_Attribute = '7597d956-b6ad-4e93-9516-af6ff3c075c3'";
				Assert.AreEqual(0, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));

				cmd.Transaction = Transaction;
				cmd.CommandText = @"Select Count(1) from RefCusCodeOrAttributeTransportMode Where ZZU_ZZD_CodeList = '7c87a968-66aa-48f3-9caf-5e36fd246a80'";
				Assert.AreEqual(2, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DataFixWI00190456Transformation(11);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
INSERT INTO RefDataGrouping (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
VALUES ('1d10da96-13cb-49be-ba7a-6d9099363dea','US','United States','US');

INSERT INTO RefCusCodeType (ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_IsReadOnly, ZZK_ZZZ_NKDataGrouping)
VALUES ('2b131abc-77e8-4488-98f8-25227dfad721', 'CUSOF', 'Customs Office Code', 1, 'US')

INSERT INTO RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping)
VALUES ('7c87a968-66aa-48f3-9caf-5e36fd246a80', 'CUSOF', '4602', 'PERTH AMBOY, NJ', '1900-01-01 00:00:00', '2079-06-06 23:59:00', 'US');

INSERT INTO RefCusCodeListAttributeName(ZXE_PK, ZXE_Name, ZXE_Description, ZXE_ZZK_NKCodeType, ZXE_ZZZ_NKDataGrouping, ZXE_IsMandatory, ZXE_AllowDuplicates, ZXE_IsValueMandatory)
VALUES ('481E2D99-CA7F-4F65-8E08-32806A57B41A', 'ROLE', 'Role', 'CUSOF', 'US', 0, 0, 0)

INSERT INTO RefCusCodeListAttribute(ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value)
VALUES ('7597d956-b6ad-4e93-9516-af6ff3c075c3', '7c87a968-66aa-48f3-9caf-5e36fd246a80', 'ROLE', 'EXP')

INSERT INTO RefCusCodeOrAttributeTransportMode(ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList, ZZU_ZZE_Attribute, ZZU_DataSetPK, ZZU_DataSetCode)
VALUES ('3a602ef7-986d-4eb8-9aa1-6187dcde90df', 'RAI', null, '7597d956-b6ad-4e93-9516-af6ff3c075c3', '7c87a968-66aa-48f3-9caf-5e36fd246a80', 'ZZD')

INSERT INTO RefCusCodeOrAttributeTransportMode(ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList, ZZU_ZZE_Attribute, ZZU_DataSetPK, ZZU_DataSetCode)
VALUES ('ff0ee7c2-2e0e-4656-a66b-be2e8a3e3198', 'SEA', null, '7597d956-b6ad-4e93-9516-af6ff3c075c3', '7c87a968-66aa-48f3-9caf-5e36fd246a80', 'ZZD')
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}

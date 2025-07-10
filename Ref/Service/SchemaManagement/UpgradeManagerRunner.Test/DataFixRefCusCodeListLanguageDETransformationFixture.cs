using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class DataFixRefCusCodeListLanguageDETransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Transaction?.Connection?.CreateCommand())
			{
				Assert.AreEqual(0, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(*) FROM RefCusCodeListLanguage WHERE ZXA_ZX6_NKLanguage='GRM'"));
				Assert.AreEqual("DE", DbHelper.ExecuteScalar(Transaction, "SELECT ZXA_ZX6_NKLanguage FROM RefCusCodeListLanguage WHERE ZXA_ZZD_CodeList = '05E4CE24-B6AD-4F7B-B7ED-3C78FBC8B6C3'"));
				Assert.AreEqual("DE", DbHelper.ExecuteScalar(Transaction, "SELECT ZXA_ZX6_NKLanguage FROM RefCusCodeListLanguage WHERE ZXA_ZZD_CodeList = 'CD079D46-EB75-4AB3-B73C-497A3C86C0E9'"));
				Assert.AreEqual("DE", DbHelper.ExecuteScalar(Transaction, "SELECT ZXA_ZX6_NKLanguage FROM RefCusCodeListLanguage WHERE ZXA_ZZD_CodeList = '9C608FBE-BB06-431C-A455-43550F39808E'"));
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DataFixRefCusCodeListLanguageDETransformation(0);
		}

		protected override void PrepareTestData()
		{
			var sql = @"
INSERT INTO RefDataGrouping (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
VALUES ('ED0CE102-2C22-4995-9F99-8D5825517E52','DE','Germany','DE');

INSERT INTO RefCusCodeType(ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_IsReadOnly, ZZK_ZZZ_NKDataGrouping)
VALUES(newid(), 'USFPC', 'X' , 1, 'DE')

INSERT INTO RefCusCodeList(ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping)
VALUES('05E4CE24-B6AD-4F7B-B7ED-3C78FBC8B6C3', 'USFPC', 'TS1', 'Desc', '2019-03-05', '2079-06-06 23:59:00', 'DE'),
('CD079D46-EB75-4AB3-B73C-497A3C86C0E9', 'USFPC', 'TS2', 'Desc', '2019-03-06', '2079-06-06 23:59:00', 'DE'),
('9C608FBE-BB06-431C-A455-43550F39808E', 'USFPC', 'TS3', 'Desc', '2019-03-07', '2079-06-06 23:59:00', 'DE');

INSERT INTO RefLanguageType(ZX6_PK, ZX6_Language, ZX6_Description)
VALUES(newid(), 'DE', 'German'),
(newid(), 'GRM', 'German')

INSERT INTO RefCusCodeListLanguage(ZXA_PK, ZXA_ZX6_NKLanguage, ZXA_ZZD_CodeList, ZXA_Description)
VALUES (newid(), 'GRM', '05E4CE24-B6AD-4F7B-B7ED-3C78FBC8B6C3', 'Desc1GRM'),
(newid(), 'DE', '05E4CE24-B6AD-4F7B-B7ED-3C78FBC8B6C3', 'Desc1DE'),
(newid(), 'GRM', 'CD079D46-EB75-4AB3-B73C-497A3C86C0E9', 'Desc2'),
(newid(), 'GRM', '9C608FBE-BB06-431C-A455-43550F39808E', 'Desc3')";

			using (var cmd = Transaction?.Connection?.CreateCommand())
			{
				cmd.CommandText = sql;
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}

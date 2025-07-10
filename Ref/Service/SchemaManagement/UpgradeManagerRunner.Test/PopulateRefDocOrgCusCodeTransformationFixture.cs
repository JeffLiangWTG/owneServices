using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	[TestFixture]
	class PopulateRefDocOrgCusCodeTransformationFixture: TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			var sql = @"
SELECT DOC_Direction FROM RefDocOrgCusCode
WHERE DOC_RN_NKRegulatingCountry = 'AT' AND DOC_RN_NKCodeCountry = 'AT' AND DOC_CodeType='EOR' AND DOC_DocumentType='ESI'";
			Assert.AreEqual("IMP", DbHelper.ExecuteScalar(Transaction, sql));

			sql = @"
SELECT DOC_Direction FROM RefDocOrgCusCode
WHERE DOC_RN_NKRegulatingCountry = 'NO' AND DOC_RN_NKCodeCountry = 'NO' AND DOC_CodeType='EOR' AND DOC_DocumentType='ESI'";
			Assert.AreEqual("IMP", DbHelper.ExecuteScalar(Transaction, sql));

			sql = @"
SELECT DOC_Direction FROM RefDocOrgCusCode
WHERE DOC_RN_NKRegulatingCountry = 'CN' AND DOC_RN_NKCodeCountry = 'CN' AND DOC_CodeType='EOR' AND DOC_DocumentType='ESI'";
			Assert.AreEqual("BTH", DbHelper.ExecuteScalar(Transaction, sql));

			sql = @"
SELECT DOC_Direction FROM RefDocOrgCusCode
WHERE DOC_RN_NKRegulatingCountry = 'AT' AND DOC_RN_NKCodeCountry = 'NO' AND DOC_CodeType='EOR' AND DOC_DocumentType='ESI'";
			Assert.AreEqual("BTH", DbHelper.ExecuteScalar(Transaction, sql));
		}

		protected override IDataTransformationTask GetTask()
		{
			return new PopulateRefDocOrgCusCodeTransformation(0);
		}

		protected override void PrepareTestData()
		{
			new PopulateRefDataSetInitialInformation(0).Run(Transaction);
			var sql = @"
INSERT RefDocOrgCusCode(DOC_PK,DOC_RN_NKRegulatingCountry,DOC_RN_NKCodeCountry,DOC_CodeType,DOC_DocumentType,DOC_ShortLabel,DOC_LongLabel,DOC_Description,DOC_Priority,DOC_Notes,DOC_Direction)
	VALUES(newid(), 'AT', 'AT', 'EOR', 'ESI', 'CRN', '', '', '1', '', ''),
		  (newid(), 'NO', 'NO', 'EOR', 'ESI', 'CRN', '', '', '1', '', ''),
		  (newid(), 'CN', 'CN', 'EOR', 'ESI', 'CRN', '', '', '1', '', ''),
		  (newid(), 'AT', 'NO', 'EOR', 'ESI', 'CRN', '', '', '1', '', '');
";
			DbHelper.ExecuteNonQuery(Transaction, sql);
		}
	}
}

using System.Linq;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class DeleteDuplicateVATApplicabilitiesTransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"
SELECT COUNT(*) FROM RefCusVATApplicability WHERE ZX5_AdditionalCode='' AND ZX5_ZZ1_Tariff IN (
SELECT ZX5_ZZ1_Tariff FROM RefCusVATApplicability
WHERE ZX5_ZZZ_NKDataGrouping='IT' AND ZX5_StartDate<GETDATE() AND ZX5_EndDate>GETDATE()
AND ZX5_ZZF_NKTaxOrFeeCode IN (SELECT ZZF_Code FROM RefCusTaxOrFee WHERE ZZF_ZZZ_NKDataGrouping='IT')
GROUP BY ZX5_ZZ1_Tariff,ZX5_ZZF_NKTaxOrFeeCode HAVING COUNT(ZX5_ZZ1_Tariff)>1 AND COUNT(ZX5_ZZF_NKTaxOrFeeCode)>1
)";
				Assert.AreEqual(0, cmd.ExecuteScalar());
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DeleteDuplicateVATApplicabilitiesTransformation(0);
		}

		protected override void PrepareTestData()
		{
			var sql = @"
INSERT INTO RefDataGrouping (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
VALUES ('20E244E7-D6FE-447E-B81B-486FC06E549D','EUN','European Union',NULL),
('9232ACD1-89D3-4135-BDE5-45E38D0F2866','IT','Italy','EUN');

INSERT INTO RefCusTariffType (ZZI_PK,ZZI_TariffType,ZZI_Description,ZZI_ZZ9_NKNomenclatureGroupType,ZZI_ZZZ_NKDataGrouping)
VALUES ('FC6DB681-F5F4-4B2F-9020-84AFF4F8BE98','IMP','Import Tariff','','EUN');

INSERT INTO RefCusTaxOrFeeType (ZX0_PK, ZX0_TaxOrFeeType, ZX0_Description)
VALUES ('D5AAB67E-D76D-4A35-95E4-5A8274C6E2A7', 'OTH', 'Other');

INSERT INTO RefCusTaxOrFee (ZZF_PK,ZZF_Code,ZZF_Description,ZZF_Value,ZZF_StartDate,ZZF_EndDate,ZZF_ZZZ_NKDataGrouping,ZZF_ZX0_NKTaxOrFeeType)
VALUES ('FBC9D1DB-B995-411A-BCE4-1D849DA928B7','MIN','Minima',0.04000000,'1900-01-01 00:00:00','2079-06-06 00:00:00','IT','OTH'),
('15A2CCF0-16C6-4739-87A9-22E7AFB1B22F','RID','Ridotta',0.10000000,'1900-01-01 00:00:00','2079-06-06 00:00:00','IT','OTH'),
('3DCA1896-22C6-4189-A2AE-75BDD0CF4C02','ORD','Ordinaria',0.22000000,'1900-01-01 00:00:00','2079-06-06 00:00:00','IT','OTH'),
('F6C87D0C-30E1-4733-90C6-EC8A5F3C676C','ESE','Esente',0.00000000,'1900-01-01 00:00:00','2079-06-06 00:00:00','IT','OTH');

INSERT INTO RefCusTariff (ZZ1_PK,ZZ1_ZZI_TariffType,ZZ1_TariffCode,ZZ1_Description,ZZ1_StartDate,ZZ1_EndDate,ZZ1_PublishedDate,ZZ1_ZZF_NKTaxOrFeeCode,ZZ1_ZZZ_NKDataGrouping)
VALUES ('9977AB29-5B7A-4E6A-9BDF-BF0B0BB6240D','FC6DB681-F5F4-4B2F-9020-84AFF4F8BE98','8713900000','Other','2020-07-01 00:00:00','2079-06-06 23:59:00','2020-07-30 00:00:00','','EUN'),
('D012C1F8-4AD9-3211-C1AB-08D8432016B8','FC6DB681-F5F4-4B2F-9020-84AFF4F8BE98','3406000000','Other','2020-07-01 00:00:00','2079-06-06 23:59:00','2020-07-30 00:00:00','','EUN');

INSERT INTO RefCusVATApplicability (ZX5_PK,ZX5_ZZ1_Tariff,ZX5_ZZW_TariffNationalCode,ZX5_ZZF_NKTaxOrFeeCode,ZX5_StartDate,ZX5_EndDate,ZX5_AdditionalCode,ZX5_Description,ZX5_ZZZ_NKDataGrouping,ZX5_DataSetPK,ZX5_DataSetCode)
VALUES 
(NEWID(), '9977AB29-5B7A-4E6A-9BDF-BF0B0BB6240D', NULL, 'MIN', '2020-07-10 00:00:00', '2079-06-06 23:59:00', '', '', 'IT', '9977AB29-5B7A-4E6A-9BDF-BF0B0BB6240D','ZZ1'),
(NEWID(), '9977AB29-5B7A-4E6A-9BDF-BF0B0BB6240D', NULL, 'MIN', '2020-07-10 00:00:00', '2079-06-06 23:59:00', 'Q056', '', 'IT', '9977AB29-5B7A-4E6A-9BDF-BF0B0BB6240D','ZZ1'),
(NEWID(), '9977AB29-5B7A-4E6A-9BDF-BF0B0BB6240D', NULL, 'ORD', '2020-07-10 00:00:00', '2079-06-06 23:59:00', '', '', 'IT', '9977AB29-5B7A-4E6A-9BDF-BF0B0BB6240D','ZZ1'),
(NEWID(), 'D012C1F8-4AD9-3211-C1AB-08D8432016B8', NULL, 'ESE', '2020-07-10 00:00:00', '2079-06-06 23:59:00', '', '', 'IT', '9977AB29-5B7A-4E6A-9BDF-BF0B0BB6240D','ZZ1'),
(NEWID(), 'D012C1F8-4AD9-3211-C1AB-08D8432016B8', NULL, 'ESE', '2020-07-10 00:00:00', '2079-06-06 23:59:00', 'Q101', '', 'IT', '9977AB29-5B7A-4E6A-9BDF-BF0B0BB6240D','ZZ1'),
(NEWID(), 'D012C1F8-4AD9-3211-C1AB-08D8432016B8', NULL, 'RID', '2020-07-10 00:00:00', '2079-06-06 23:59:00', 'Q050', '', 'IT', '9977AB29-5B7A-4E6A-9BDF-BF0B0BB6240D','ZZ1');
";

			DbHelper.ExecuteNonQuery(Transaction, sql);
		}
	}
}

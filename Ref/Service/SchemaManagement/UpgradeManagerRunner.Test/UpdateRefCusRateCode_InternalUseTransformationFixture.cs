using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class UpdateRefCusRateCode_InternalUseTransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"SELECT COUNT(1) FROM RefCusRateCode WHERE ZY1_InternalUse = 1";
				Assert.AreEqual(6, cmd.ExecuteScalar());
				cmd.CommandText = @"SELECT COUNT(1) FROM RefCusRateCode WHERE ZY1_InternalUse = 0";
				Assert.AreEqual(2, cmd.ExecuteScalar());
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new UpdateRefCusRateCode_InternalUseTransformation(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"

INSERT INTO RefDataGrouping (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
VALUES ('20E244E7-D6FE-447E-B81B-486FC06E549D','EUN','European Union',NULL),
('1242E9BB-33C5-4E6A-BAB8-021BB7CA2CFE','ES','Spain','EUN');

INSERT INTO RefCusRateType (ZZR_PK,ZZR_RateType,ZZR_Description,ZZR_IsPayable,ZZR_ZZZ_NKDataGrouping,ZZR_RX_NKFormulaCurrency,ZZR_CustomsValueFormula)
VALUES ('58F19F31-B7BB-48E3-9ED9-76733B831C1B', 'DTY', 'Duty', 1, 'EUN', '', ''),
('56B11480-8664-4304-A665-EB74ACFAD13D', 'REA', 'REA - AY Tax Rebate', 0, 'ES', '', '');

INSERT INTO RefCusRateCode (ZY1_PK,ZY1_RateCode,ZY1_ZZR_RateType,ZY1_Description) VALUES
('9FBDD063-4839-4C9F-A602-8A28A5897724','ADFM','58F19F31-B7BB-48E3-9ED9-76733B831C1B','Flour Duty'),
('E4475805-DFE1-45B7-9D1C-BB05AFBEF055','ADFMR','58F19F31-B7BB-48E3-9ED9-76733B831C1B','Flour Duty - Reduced'),
('F1492021-9C2D-428D-8908-44320008DA78','ADSZ','58F19F31-B7BB-48E3-9ED9-76733B831C1B','Sugar Duty'),
('4171CF17-F872-4FF4-9655-C1A5824B91A1','ADSZR','58F19F31-B7BB-48E3-9ED9-76733B831C1B','Sugar Duty - Reduced'),
('ADB77F98-AF9B-4027-9F9F-8DA9FCBF13F3','EA','58F19F31-B7BB-48E3-9ED9-76733B831C1B','Agricultural Component'),
('FA4A7901-2EA6-474C-9578-1047E1B4CB13','EAR','58F19F31-B7BB-48E3-9ED9-76733B831C1B','Agricultural Component - Reduced'),
('F5080BAE-0E98-4569-AC12-1B6593A6D5E3','AYD','58F19F31-B7BB-48E3-9ED9-76733B831C1B','AY REA Rebate-Consumo Directo'),
('7401470D-11D4-4A3A-96A9-FB7379EB3770','AYT','56B11480-8664-4304-A665-EB74ACFAD13D','AY REA Rebate-Trans/Ins. Agrario');";

				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}

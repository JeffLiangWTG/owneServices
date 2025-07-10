using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class DataFixWI00205047TransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"SELECT COUNT(*) FROM UNDGSubstance
WHERE DG_UNNO = '3268' AND DG_Variant IN ('a', 'b', 'c') AND DG_IsActive = 0";
				Assert.AreEqual(3, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
			}

			using (var cmd2 = Connection.CreateCommand())
			{
				cmd2.Transaction = Transaction;
				cmd2.CommandText = @"SELECT DG_Variant, DG_Variation, DG_PSN, DG_PG, DG_ExceptedQuantityCode FROM UNDGSubstance WHERE DG_UNNO = '3268' AND DG_Variant = ''";

				using (var reader = cmd2.ExecuteReader())
				{
					Assert.IsTrue(reader.Read());
					Assert.AreEqual(string.Empty, reader.GetString(0));
					Assert.AreEqual(string.Empty, reader.GetString(1));
					Assert.AreEqual("Safety devices, electrically initiated", reader.GetString(2));
					Assert.AreEqual(string.Empty, reader.GetString(3));
					Assert.AreEqual("E0", reader.GetString(4));
				}
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DataFixWI00205047Transformation(29);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
INSERT INTO UNDGSubstance (DG_PK, DG_UNNO, DG_Variant, DG_Variation, DG_Class, DG_SubLabel1, DG_SubLabel2, DG_PSN, DG_PG, DG_EMS, DG_MP, DG_FlashPoint, DG_LQMaxAmt, DG_LQMaxAmtUQ, DG_LQSpecProvIndex, DG_TechName, DG_TreatAs, DG_DglPhrase, DG_PackIns, DG_PackProv, DG_IBCIns, DG_IBCProv, DG_IMOTankIns, DG_UNTankIns, DG_TankProv, DG_Markers, DG_Pointers, DG_EXVector, DG_StowCat, DG_CodedStow, DG_State, DG_ExpLim, DG_UlineEMS, DG_UsrUSDOTShippingName, DG_ExceptedQuantityCode, DG_IsActive)
VALUES('428371DC-6D0E-4FAB-8CD5-0F381C504DF6', '3268', 'a', 'Air bag inflators.', '9', '', '', 'AIR BAG INFLATORS', 'III', 'F-B,S-X', '', '', 0, '', 0, '', '', '', 'P902 LP902', '', '', '', '', '', '', '', '', '', 'A', '', 'S', '', '1', '', '', 1)

INSERT INTO UNDGSubstance (DG_PK, DG_UNNO, DG_Variant, DG_Variation, DG_Class, DG_SubLabel1, DG_SubLabel2, DG_PSN, DG_PG, DG_EMS, DG_MP, DG_FlashPoint, DG_LQMaxAmt, DG_LQMaxAmtUQ, DG_LQSpecProvIndex, DG_TechName, DG_TreatAs, DG_DglPhrase, DG_PackIns, DG_PackProv, DG_IBCIns, DG_IBCProv, DG_IMOTankIns, DG_UNTankIns, DG_TankProv, DG_Markers, DG_Pointers, DG_EXVector, DG_StowCat, DG_CodedStow, DG_State, DG_ExpLim, DG_UlineEMS, DG_UsrUSDOTShippingName, DG_ExceptedQuantityCode, DG_IsActive)
VALUES('D9C7CB2A-CFB8-4DAF-AB51-7C8D140BABB5', '3268', 'b', 'Air bag modulers.', '9', '', '', 'AIR BAG MODULES', 'III', 'F-B,S-X', '', '', 0, '', 0, '', '', '', 'P902 LP902', '', '', '', '', '', '', '', '', '', 'A', '', 'S', '', '1', '', '', 1)

INSERT INTO UNDGSubstance (DG_PK, DG_UNNO, DG_Variant, DG_Variation, DG_Class, DG_SubLabel1, DG_SubLabel2, DG_PSN, DG_PG, DG_EMS, DG_MP, DG_FlashPoint, DG_LQMaxAmt, DG_LQMaxAmtUQ, DG_LQSpecProvIndex, DG_TechName, DG_TreatAs, DG_DglPhrase, DG_PackIns, DG_PackProv, DG_IBCIns, DG_IBCProv, DG_IMOTankIns, DG_UNTankIns, DG_TankProv, DG_Markers, DG_Pointers, DG_EXVector, DG_StowCat, DG_CodedStow, DG_State, DG_ExpLim, DG_UlineEMS, DG_UsrUSDOTShippingName, DG_ExceptedQuantityCode, DG_IsActive)
VALUES('9AEEF082-1245-43A5-90A0-6ABDA5CFC06C', '3268', 'c', 'Seat-belt pretensioners.', '9', '', '', 'SEAT-BELT PRETENSIONERS', 'III', 'F-B,S-X', '', '', 0, '', 0, '', '', '', 'P902 LP902', '', '', '', '', '', '', '', '', '', 'A', '', 'S', '', '1', '', '', 1)
			";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}

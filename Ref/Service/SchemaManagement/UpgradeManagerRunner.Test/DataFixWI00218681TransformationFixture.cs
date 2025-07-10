using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class DataFixWI00218681TransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"Select Count(1) from UNDGAttribute t Where DA_Language = 'ENG'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 0);
				cmd.CommandText = @"SELECT COUNT(*) FROM RefDbVersionControl WHERE RVC_ParentCode = 'DC'
AND RVC_ParentPK = '55F8F050-52D2-4503-9C88-D338A98635F0' AND RVC_Deleted = 1";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
				cmd.CommandText = @"SELECT COUNT(*) FROM UNDGCommonData WHERE DC_Language = 'EN' AND DC_Type = 'STS' AND DC_Index='10'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DataFixWI00218681Transformation(10);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
INSERT INTO UNDGSubstance(DG_PK, DG_UNNO, DG_Class, DG_PSN)
VALUES('F130AD98-425B-42DB-97FE-3493190302DD', '1010', '2', 'Testing'); 

INSERT INTO UNDGAttribute(DA_PK, DA_Language, DA_Type, DA_Index, DA_Descriptor, DA_DG)
VALUES(NEWID(), 'ENG', 'CPV', 10, 'Testing', 'F130AD98-425B-42DB-97FE-3493190302DD');

INSERT INTO UNDGCommonData(DC_PK, DC_Language, DC_Type, DC_Index, DC_Descriptor)
VALUES('55F8F050-52D2-4503-9C88-D338A98635F0', 'ENG', 'STS', 10, 'Testing');
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}

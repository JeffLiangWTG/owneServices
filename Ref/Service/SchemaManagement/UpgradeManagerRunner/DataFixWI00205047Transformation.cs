using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataFixWI00205047Transformation : DataTransformation, IDataTransformationTask
	{
		public DataFixWI00205047Transformation(int version) : base(version)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public void Run(IDbTransaction trans)
		{
			var sql = @"
IF NOT EXISTS(SELECT 1 FROM UNDGSubstance WHERE DG_UNNO = '3268' AND DG_Variant = '')
BEGIN
	UPDATE UNDGSubstance
	SET DG_IsActive = 0
	WHERE DG_UNNO = '3268' AND DG_Variant IN ('a', 'b', 'c')

	DECLARE @DG_PK UNIQUEIDENTIFIER = NEWID()
	INSERT INTO UNDGSubstance (DG_PK, DG_UNNO, DG_Variant, DG_Variation, DG_Class, DG_SubLabel1, DG_SubLabel2, DG_PSN, DG_PG, DG_EMS, DG_MP, DG_FlashPoint, DG_LQMaxAmt, DG_LQMaxAmtUQ, DG_LQSpecProvIndex, DG_TechName, DG_TreatAs, DG_DglPhrase, DG_PackIns, DG_PackProv, DG_IBCIns, DG_IBCProv, DG_IMOTankIns, DG_UNTankIns, DG_TankProv, DG_Markers, DG_Pointers, DG_EXVector, DG_StowCat, DG_CodedStow, DG_State, DG_ExpLim, DG_UlineEMS, DG_UsrUSDOTShippingName, DG_ExceptedQuantityCode, DG_IsActive)
	SELECT @DG_PK, DG_UNNO, '', '', DG_Class, DG_SubLabel1, DG_SubLabel2, 'Safety devices, electrically initiated', '', DG_EMS, DG_MP, DG_FlashPoint, DG_LQMaxAmt, DG_LQMaxAmtUQ, DG_LQSpecProvIndex, DG_TechName, DG_TreatAs, DG_DglPhrase, DG_PackIns, DG_PackProv, DG_IBCIns, DG_IBCProv, DG_IMOTankIns, DG_UNTankIns, DG_TankProv, DG_Markers, DG_Pointers, DG_EXVector, DG_StowCat, DG_CodedStow, DG_State, DG_ExpLim, DG_UlineEMS, DG_UsrUSDOTShippingName, 'E0', 1 
	FROM UNDGSubstance WHERE DG_UNNO = '3268' AND DG_Variant = 'c'

	INSERT INTO UNDGAttribute(DA_PK, DA_Language, DA_Type, DA_Index, DA_Descriptor, DA_DG)
	SELECT NEWID(), DA_Language, DA_Type, DA_Index, DA_Descriptor, @DG_PK
	FROM UNDGAttribute WHERE DA_DG = (SELECT DG_PK FROM UNDGSubstance where DG_UNNO = '3268' AND DG_Variant = 'c')
END
";

			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.Connection = trans.Connection;
				cmd.Transaction = trans;
				cmd.CommandText = sql;
				cmd.ExecuteNonQuery();
			}
		}
	}
}

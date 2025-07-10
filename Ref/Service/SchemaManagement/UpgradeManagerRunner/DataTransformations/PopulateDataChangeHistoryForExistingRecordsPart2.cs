using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class PopulateDataChangeHistoryForExistingRecordsPart2 : DataTransformation, IDataTransformationTask
	{
		public PopulateDataChangeHistoryForExistingRecordsPart2(int version) : base(version)
		{ }

		public void Run(IDbTransaction trans)
		{
			var sql = @"
DECLARE @sourcePK1 SourcePKList;
IF NOT EXISTS (SELECT TOP 1 1 FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZZ4')
BEGIN
	INSERT @sourcePK1
	SELECT ZZ4_PK, 'ZZ4' FROM RefCarrierCode

	EXEC dbo.RecordDataSetChangeHistory @sourcePK1
END

IF NOT EXISTS (SELECT TOP 1 1 FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZZ1')
BEGIN
	INSERT @sourcePK1
	SELECT ZZ1_PK, 'ZZ1' FROM RefCusTariff

	EXEC dbo.RecordDataSetChangeHistory @sourcePK1
END

IF NOT EXISTS (SELECT TOP 1 1 FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZY5')
BEGIN
	INSERT @sourcePK1
	SELECT ZY5_PK, 'ZY5' FROM RefCusAUNexdocECMCode

	EXEC dbo.RecordDataSetChangeHistory @sourcePK1
END

IF NOT EXISTS (SELECT TOP 1 1 FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZZK')
BEGIN
	INSERT @sourcePK1
	SELECT ZZK_PK, 'ZZK' FROM RefCusCodeType

	EXEC dbo.RecordDataSetChangeHistory @sourcePK1
END

IF NOT EXISTS (SELECT TOP 1 1 FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZZ5')
BEGIN
	INSERT @sourcePK1
	SELECT ZZ5_PK, 'ZZ5' FROM RefCusNomenclatureGroup

	EXEC dbo.RecordDataSetChangeHistory @sourcePK1
END

IF NOT EXISTS (SELECT TOP 1 1 FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZZS')
BEGIN
	INSERT @sourcePK1
	SELECT ZZS_PK, 'ZZS' FROM RefCusPreference

	EXEC dbo.RecordDataSetChangeHistory @sourcePK1
END

IF NOT EXISTS (SELECT TOP 1 1 FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZZ6')
BEGIN
	INSERT @sourcePK1
	SELECT ZZ6_PK, 'ZZ6' FROM RefCusProcedure

	EXEC dbo.RecordDataSetChangeHistory @sourcePK1
END

IF NOT EXISTS (SELECT TOP 1 1 FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZZR')
BEGIN
	INSERT @sourcePK1
	SELECT ZZR_PK, 'ZZR' FROM RefCusRateType

	EXEC dbo.RecordDataSetChangeHistory @sourcePK1
END

IF NOT EXISTS (SELECT TOP 1 1 FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZZX')
BEGIN
	INSERT @sourcePK1
	SELECT ZZX_PK, 'ZZX' FROM RefCusRuling

	EXEC dbo.RecordDataSetChangeHistory @sourcePK1
END

IF NOT EXISTS (SELECT TOP 1 1 FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZY3')
BEGIN
	INSERT @sourcePK1
	SELECT ZY3_PK, 'ZY3' FROM RefCusTariffAdditionalCodeCategory

	EXEC dbo.RecordDataSetChangeHistory @sourcePK1
END

IF NOT EXISTS (SELECT TOP 1 1 FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZZI')
BEGIN
	INSERT @sourcePK1
	SELECT ZZI_PK, 'ZZI' FROM RefCusTariffType

	EXEC dbo.RecordDataSetChangeHistory @sourcePK1
END

IF NOT EXISTS (SELECT TOP 1 1 FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZX0')
BEGIN
	INSERT @sourcePK1
	SELECT ZX0_PK, 'ZX0' FROM RefCusTaxOrFeeType

	EXEC dbo.RecordDataSetChangeHistory @sourcePK1
END

IF NOT EXISTS (SELECT TOP 1 1 FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZZA')
BEGIN
	INSERT @sourcePK1
	SELECT ZZA_PK, 'ZZA' FROM RefCusTradeGroup

	EXEC dbo.RecordDataSetChangeHistory @sourcePK1
END

IF NOT EXISTS (SELECT TOP 1 1 FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZZZ')
BEGIN
	INSERT @sourcePK1
	SELECT ZZZ_PK, 'ZZZ' FROM RefDataGrouping

	EXEC dbo.RecordDataSetChangeHistory @sourcePK1
END

IF NOT EXISTS (SELECT TOP 1 1 FROM DataSetChangeHistory WHERE DCH_ParentCode = 'DOC')
BEGIN
	INSERT @sourcePK1
	SELECT DOC_PK, 'DOC' FROM RefDocOrgCusCode

	EXEC dbo.RecordDataSetChangeHistory @sourcePK1
END

IF NOT EXISTS (SELECT TOP 1 1 FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZXF')
BEGIN
	INSERT @sourcePK1
	SELECT ZXF_PK, 'ZXF' FROM RefHarbourRate

	EXEC dbo.RecordDataSetChangeHistory @sourcePK1
END

IF NOT EXISTS (SELECT TOP 1 1 FROM DataSetChangeHistory WHERE DCH_ParentCode = 'RSL')
BEGIN
	INSERT @sourcePK1
	SELECT RSL_PK, 'RSL' FROM RefShippingLine

	EXEC dbo.RecordDataSetChangeHistory @sourcePK1
END

IF NOT EXISTS (SELECT TOP 1 1 FROM DataSetChangeHistory WHERE DCH_ParentCode = 'RL')
BEGIN
	INSERT @sourcePK1
	SELECT RL_PK, 'RL' FROM RefUNLOCO

	EXEC dbo.RecordDataSetChangeHistory @sourcePK1
END

IF NOT EXISTS (SELECT TOP 1 1 FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ADR')
BEGIN
	INSERT @sourcePK1
	SELECT ADR_PK, 'ADR' FROM UNDGSubstanceADR

	EXEC dbo.RecordDataSetChangeHistory @sourcePK1
END
";
			DbHelper.ExecuteNonQuery(trans, sql);
		}
	}
}

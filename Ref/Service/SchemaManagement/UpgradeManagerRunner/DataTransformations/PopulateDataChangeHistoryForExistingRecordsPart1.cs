using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class PopulateDataChangeHistoryForExistingRecordsPart1 : DataTransformation, IDataTransformationTask
	{
		public PopulateDataChangeHistoryForExistingRecordsPart1(int version) : base(version)
		{ }

		public void Run(IDbTransaction trans)
		{
			var sql = @"
DECLARE @sourcePK1 SourcePKList;
IF NOT EXISTS (SELECT TOP 1 1 FROM DataSetChangeHistory WHERE DCH_ParentCode = 'RN')
BEGIN
	INSERT @sourcePK1
	SELECT RN_PK, 'RN' FROM RefCountry

	EXEC dbo.RecordDataSetChangeHistory @sourcePK1
END
IF NOT EXISTS (SELECT TOP 1 1 FROM DataSetChangeHistory WHERE DCH_ParentCode = 'RW')
BEGIN
	INSERT @sourcePK1
	SELECT RW_PK, 'RW' FROM RefCountryStates

	EXEC dbo.RecordDataSetChangeHistory @sourcePK1
END
IF NOT EXISTS (SELECT TOP 1 1 FROM DataSetChangeHistory WHERE DCH_ParentCode = 'RX')
BEGIN
	INSERT @sourcePK1
	SELECT RX_PK, 'RX' FROM RefCurrency

	EXEC dbo.RecordDataSetChangeHistory @sourcePK1
END
IF NOT EXISTS (SELECT TOP 1 1 FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZX2')
BEGIN
	INSERT @sourcePK1
	SELECT ZX2_PK, 'ZX2' FROM RefCusConditionType

	EXEC dbo.RecordDataSetChangeHistory @sourcePK1
END
IF NOT EXISTS (SELECT TOP 1 1 FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZX4')
BEGIN
	INSERT @sourcePK1
	SELECT ZX4_PK, 'ZX4' FROM RefCusConditionValueType

	EXEC dbo.RecordDataSetChangeHistory @sourcePK1
END
IF NOT EXISTS (SELECT TOP 1 1 FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZZM')
BEGIN
	INSERT @sourcePK1
	SELECT ZZM_PK, 'ZZM' FROM RefCusMap

	EXEC dbo.RecordDataSetChangeHistory @sourcePK1
END
IF NOT EXISTS (SELECT TOP 1 1 FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZZN')
BEGIN
	INSERT @sourcePK1
	SELECT ZZN_PK, 'ZZN' FROM RefExchangeRateZZ

	EXEC dbo.RecordDataSetChangeHistory @sourcePK1
END
IF NOT EXISTS (SELECT TOP 1 1 FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZZP')
BEGIN
	INSERT @sourcePK1
	SELECT ZZP_PK, 'ZZP' FROM RefCusMapType

	EXEC dbo.RecordDataSetChangeHistory @sourcePK1
END
IF NOT EXISTS (SELECT TOP 1 1 FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZZ9')
BEGIN
	INSERT @sourcePK1
	SELECT ZZ9_PK, 'ZZ9' FROM RefCusNomenclatureGroupType

	EXEC dbo.RecordDataSetChangeHistory @sourcePK1
END
IF NOT EXISTS (SELECT TOP 1 1 FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZX6')
BEGIN
	INSERT @sourcePK1
	SELECT ZX6_PK, 'ZX6' FROM RefLanguageType

	EXEC dbo.RecordDataSetChangeHistory @sourcePK1
END
IF NOT EXISTS (SELECT TOP 1 1 FROM DataSetChangeHistory WHERE DCH_ParentCode = 'R3')
BEGIN
	INSERT @sourcePK1
	SELECT R3_PK, 'R3' FROM RefTimeZoneSet

	EXEC dbo.RecordDataSetChangeHistory @sourcePK1
END
IF NOT EXISTS (SELECT TOP 1 1 FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZZO')
BEGIN
	INSERT @sourcePK1
	SELECT ZZO_PK, 'ZZO' FROM RefVesselZZ

	EXEC dbo.RecordDataSetChangeHistory @sourcePK1
END
IF NOT EXISTS (SELECT TOP 1 1 FROM DataSetChangeHistory WHERE DCH_ParentCode = 'DC')
BEGIN
	INSERT @sourcePK1
	SELECT DC_PK, 'DC' FROM UNDGCommonData

	EXEC dbo.RecordDataSetChangeHistory @sourcePK1
END
IF NOT EXISTS (SELECT TOP 1 1 FROM DataSetChangeHistory WHERE DCH_ParentCode = 'DG')
BEGIN
	INSERT @sourcePK1
	SELECT DG_PK, 'DG' FROM UNDGSubstance

	EXEC dbo.RecordDataSetChangeHistory @sourcePK1
END
";
			DbHelper.ExecuteNonQuery(trans, sql);
		}
	}
}

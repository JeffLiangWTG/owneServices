CREATE VIEW RefCusConditionTableView_V3 AS
SELECT ZX1_PK,
ZX1_ZX2_ConditionType,
ZX1_ZZ1_Tariff,
ZX1_ZZ5_Nomenclature,
ZX1_StartDate,
ZX1_EndDate,
ZX1_Source,
ZX1_Comment,
ZX1_IsImport,
ZX1_IsExport,
ZX1_ConditionValueTrueMeansStop,
ZX1_ZZS_Preference,
ZX1_ZZZ_NKDataGrouping,
ZX1_LogicalANDWithinGroup,
ZX1_ZY7_NKConditionCode,
ZX1_AdditionalComment,
ZX1_Severity
FROM RefCusCondition

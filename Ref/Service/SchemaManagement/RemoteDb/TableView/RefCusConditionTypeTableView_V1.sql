CREATE VIEW RefCusConditionTypeTableView_V1 AS
SELECT ZX2_PK,
ZX2_ConditionClass,
ZX2_ConditionType,
ZX2_Description,
ZX2_ZZZ_NKDataGrouping
FROM RefCusConditionType
WHERE ZX2_ConditionClass != 'RISK' AND LEN(ZX2_ConditionType) <= 5

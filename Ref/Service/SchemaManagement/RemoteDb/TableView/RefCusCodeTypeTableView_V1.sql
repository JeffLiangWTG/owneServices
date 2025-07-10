CREATE VIEW RefCusCodeTypeTableView_V1 AS
SELECT ZZK_PK,
ZZK_CodeTypeComputed AS ZZK_CodeType,
ZZK_Description,
ZZK_IsReadonly,
ZZK_MaxLength,
ZZK_ZZZ_NKDataGrouping
FROM RefCusCodeType
WHERE ZZK_CodeTypeComputed <> ''

CREATE VIEW RefCusCodeListTableView_V1 AS
SELECT ZZD_PK,
ZZD_ZZK_NKCodeTypeComputed AS ZZD_ZZK_NKCodeType,
ZZD_Code,
ZZD_Description,
ZZD_StartDate,
ZZD_EndDate,
ZZD_ZZZ_NKDataGrouping
FROM RefCusCodeList
WHERE ZZD_ZZK_NKCodeTypeComputed <> ''

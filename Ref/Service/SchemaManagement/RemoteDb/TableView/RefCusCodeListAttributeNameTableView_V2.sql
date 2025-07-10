CREATE VIEW RefCusCodeListAttributeNameTableView_V2 AS
SELECT ZXE_PK,
ZXE_Name,
ZXE_Description,
ZXE_ZZK_NKCodeTypeComputed AS ZXE_ZZK_NKCodeType,
ZXE_ZZZ_NKDataGrouping,
ZXE_IsMandatory,
ZXE_AllowDuplicates,
ZXE_IsValueMandatory,
LEFT(ZXE_ZZK_NKCodeTypeForValueList, 5) AS ZXE_ZZK_NKCodeTypeForValueList,
ZXE_ValueDataType,
ZXE_MinLengthOrValue,
ZXE_MaxLengthOrValue,
ZXE_DecimalPlaces,
ZXE_ColumnCaption,
ZXE_IsDateRangeUsed
FROM RefCusCodeListAttributeName
WHERE ZXE_ZZK_NKCodeTypeComputed <> ''

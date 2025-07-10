CREATE VIEW RefCusTariffBRCharacteristicTableView_V2 AS
SELECT ZB1_PK,
ZB1_CharacteristicType,
ZB1_ZZ1_Tariff,
ZB1_ZZ5_Nomenclature,
ZB1_Style,
ZB1_MaxLength,
ZB1_DecimalPlaces,
ZB1_Code,
ZB1_Text,
ZB1_StartDate,
ZB1_EndDate,
ZB1_IsImport,
ZB1_IsExport,
ZB1_IsMandatory,
ZB1_IsConditioningAttribute
FROM RefCusTariffBRCharacteristic

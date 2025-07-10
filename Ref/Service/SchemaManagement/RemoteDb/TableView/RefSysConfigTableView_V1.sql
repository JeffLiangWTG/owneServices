CREATE VIEW RefSysConfigTableView_V1 AS
SELECT [ZRC_PK],
[ZRC_ZRT_NKConfigCode],
[ZRC_DecimalValue],
[ZRC_StringValue],
[ZRC_BitValue],
[ZRC_BinaryValue],
[ZRC_StartDate],
[ZRC_EndDate]
FROM RefSysConfig

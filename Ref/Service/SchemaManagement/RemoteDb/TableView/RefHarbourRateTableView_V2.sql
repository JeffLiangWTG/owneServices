CREATE VIEW RefHarbourRateTableView_V2 AS
SELECT ZXF_PK,
ZXF_Type,
ZXF_Port,
ZXF_Mode,
ZXF_Commodity,
ZXF_PortTaxType,
ZXF_StartDate,
ZXF_EndDate,
ZXF_RateFormula,
ZXF_ZZZ_NKDataGrouping
FROM RefHarbourRate

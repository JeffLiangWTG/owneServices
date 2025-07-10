CREATE VIEW RefCusApplicabilityTableView_V1 AS
SELECT ZZT_PK,
ZZT_ZZ2_Rate,
ZZT_ZX1_Conditions,
ZZT_ZY2_AdditionalCode,
ZZT_StartDate,
ZZT_EndDate,
ZZT_ZZA_TradeGroup,
ZZT_AdditionalCode,
ZZT_OrderNumber
FROM
(SELECT ZZT_PK,
ZZT_ZZ2_Rate,
ZZT_ZX1_Conditions,
ZZT_ZY2_AdditionalCode,
ZZT_StartDate,
ZZT_EndDate,
ZZT_ZZA_TradeGroup,
ZZT_AdditionalCode,
ZZT_OrderNumber,
ROW_NUMBER() OVER(PARTITION BY ZZT_ZY2_AdditionalCode, ZZT_StartDate, ZZT_ZZA_TradeGroup ORDER BY ZZT_PK) AS ord
FROM RefCusApplicability
) DuplicatedApp WHERE ord = 1

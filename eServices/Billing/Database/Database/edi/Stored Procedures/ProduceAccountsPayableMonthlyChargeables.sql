CREATE PROCEDURE edi.ProduceAccountsPayableMonthlyChargeables
	@period int
AS

SET NOCOUNT ON;

WITH US as
(
	SELECT *,
	ROW_NUMBER() OVER(PARTITION BY US_DatabaseNumber, US_Reference5 ORDER BY US_ID ASC) AS row_number
	FROM edi.Usage
	INNER JOIN edi.BillingRules BR ON BR_SPName = 'ProduceAccountsPayableMonthlyChargeables'
		AND (
				BR_Fields = 'Category-PriceItemCode'
				AND US_Category = BR.BR_Value1
				AND US_PriceItemCode = BR.BR_Value2
			)
	WHERE US_Period = @period
)

INSERT INTO edi.Chargeable (CH_ID, CH_PERIOD, CH_Category, CH_PriceItemCode, CH_BillableCount, CH_ReportingSource, CH_ServiceOccuredUtc, CH_ClientID, CH_ClientNumber, CH_DatabaseNumber, CH_CompanyNumber, CH_ClientStaffCode, CH_Reference1, CH_Reference2, CH_Reference3, CH_Reference4, CH_Reference5, CH_Version, CH_Branch, CH_SystemCreateUtc, CH_SystemLastEditUtc, CH_CapturedUtc, CH_MessageTrackingID)
SELECT
  US_ID, US_PERIOD, US_Category, CASE WHEN (US_Reference3 IN (SELECT BR_Value1 FROM edi.BillingRules WHERE BR_SPName = 'ProduceAccountsPayableMonthlyChargeables' AND BR_Fields = 'Reference3')) THEN 'TX4' ELSE US_PriceItemCode END, 1, US_ReportingSource, US_ServiceOccuredUtc, US_ClientID, US_ClientNumber, US_DatabaseNumber, US_CompanyNumber, US_ClientStaffCode, US_Reference1, US_Reference2, US_Reference3, US_Reference4, US_Reference5, US_Version, US_Branch, US_SystemCreateUtc, US_SystemLastEditUtc, US_CapturedUtc, US_MessageTrackingID
FROM US
WHERE row_number = 1

RETURN 0

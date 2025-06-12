CREATE PROCEDURE edi.ProduceOldWareHousePackagingMonthlyChargeables
	@period int
AS

SET NOCOUNT ON;

WITH US as
(
	SELECT *
	FROM edi.Usage
	WHERE US_Category = 'STL' AND US_PriceItemCode = 'WTP' AND US_Period = @period
),
CH as
(
	SELECT *
	FROM edi.Chargeable
	WHERE CH_Category = 'STL' AND CH_PriceItemCode = 'WTU' AND CH_Period = @period
)

INSERT INTO edi.Chargeable (CH_ID, CH_PERIOD, CH_Category, CH_PriceItemCode, CH_BillableCount, CH_ReportingSource, CH_ServiceOccuredUtc, CH_ClientID, CH_ClientNumber, CH_DatabaseNumber, CH_CompanyNumber, CH_ClientStaffCode, CH_Reference1, CH_Reference2, CH_Reference3, CH_Reference4, CH_Reference5, CH_Version, CH_Branch, CH_SystemCreateUtc, CH_SystemLastEditUtc, CH_CapturedUtc, CH_MessageTrackingID)
SELECT
  US_ID, US_PERIOD, US_Category, US_PriceItemCode, US_BillableCount, US_ReportingSource, US_ServiceOccuredUtc, US_ClientID, US_ClientNumber, US_DatabaseNumber, US_CompanyNumber, US_ClientStaffCode, US_Reference1, US_Reference2, US_Reference3, US_Reference4, US_Reference5, US_Version, US_Branch, US_SystemCreateUtc, US_SystemLastEditUtc, US_CapturedUtc, US_MessageTrackingID
FROM US
WHERE NOT EXISTS
(
	SELECT *
	FROM CH
	WHERE CH.CH_DatabaseNumber = US.US_DatabaseNumber AND CH.CH_CompanyNumber = US.US_CompanyNumber AND CH.CH_Reference5 = US.US_Reference5
)

RETURN 0
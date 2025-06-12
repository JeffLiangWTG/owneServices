CREATE PROCEDURE [edi].[ProcessUsageData]
	@period int,
	@isTestServer bit
AS

SET NOCOUNT ON;

--In this SP, update all matching records in staging to have a ProcessingStatus=255, which tells the broader scripts to move the records into the edi.Usage table.

--Update all transactions that should be from known production systems but require further calculation or de-duplication before they can transition to chargeable.
UPDATE SB
SET ProcessingStatus = 255
FROM edi.StagingBatch SB
INNER JOIN edi.BillingRules BR ON BR_SPName = 'ProcessUsageData'
	AND (
			(
			BR_Fields = 'Category-PriceItemCode'
			AND SB.TX_Category = BR.BR_Value1 
			AND SB.TX_PriceItemCode = BR.BR_Value2
			)
			OR (
			BR_Fields = 'Category-PriceItemCode-Reference3'
			AND SB.TX_Category = BR.BR_Value1 
			AND SB.TX_PriceItemCode = BR.BR_Value2
			AND SB.TX_Reference3 = BR.BR_Value3
			)
			OR (
			BR_Fields = 'Category-PriceItemCode-Reference4'
			AND SB.TX_Category = BR.BR_Value1 
			AND SB.TX_PriceItemCode = BR.BR_Value2
			AND SB.TX_Reference4 = BR.BR_Value3
			)
		)
WHERE TX_Period = @period 
	AND ProcessingStatus != 255 AND DatabaseNumber != 0 AND CompanyNumber != 0;

--Update all transactions that are irrelevant to billing regardless of whether the system they are for was identified
UPDATE SB
SET ProcessingStatus = 255
FROM edi.StagingBatch SB
INNER JOIN edi.BillingRules BR ON BR_SPName = 'ProcessUsageData'
	AND (
			(
			BR_Fields = 'Category-PriceItemCode-ClientID'
			AND (SB.TX_Category = BR.BR_Value1 OR BR.BR_Value1 = '') 
			AND (SB.TX_PriceItemCode = BR.BR_Value2 OR BR.BR_Value2 = '') 
			AND (SB.TX_ClientID LIKE BR.BR_Value3 OR BR.BR_Value3 = '')
			)
			OR (
			BR_Fields = 'Category-Reference4'
			AND SB.TX_Category = BR.BR_Value1
			AND SB.TX_Reference4 = BR.BR_Value2
			)
		)
WHERE TX_Period = @period 
	AND ProcessingStatus != 255

UPDATE SB
SET ProcessingStatus = 255
FROM edi.StagingBatch SB
INNER JOIN edi.BillingRules BR ON BR_SPName = 'ProcessUsageData'
	AND (
		BR_Fields = 'ClientID'
		AND SB.TX_ClientID = BR.BR_Value1
		)
WHERE TX_Period = @period
	AND ProcessingStatus != 255
	AND (TX_ClientNumber is null OR TX_ClientNumber = '')

UPDATE edi.StagingBatch
SET ProcessingStatus = 255
WHERE TX_Period = @period
	AND ProcessingStatus != 255
	AND (@isTestServer = 0 AND TX_ClientID like 'WUT%');

WITH H as
(
select *,
	ROW_NUMBER() OVER(PARTITION BY DatabaseNumber ORDER BY ValidFromUtc DESC) AS LicenseRow
	from edi.LicenceDatabaseCodeHistory
)

UPDATE SB
SET ProcessingStatus = 255
FROM edi.StagingBatch SB
INNER JOIN H
	ON SB.DatabaseNumber = H.DatabaseNumber AND H.LicenseRow = 1

WHERE TX_Period = @period
	AND ProcessingStatus != 255
	AND SB.DatabaseNumber != 0
	AND SB.CompanyNumber != 0
	AND (Product = 'CW1' OR Product = 'CWN')
	AND LicenceType != 'PRD'
	AND (	HostedLocation = 'NCW' OR
			NOT (TX_Category = 'STL' AND (TX_PriceItemCode = 'STS' OR TX_PriceItemCode = 'STL')))

-- Update all transactions that only need the latest transactions billed for each month
UPDATE SB
SET ProcessingStatus = 255
FROM edi.StagingBatch SB
INNER JOIN edi.ConfigLastMessageFilter F
	ON F.ML_Category = SB.TX_Category AND F.ML_PriceItemCode = SB.TX_PriceItemCode

RETURN 0

CREATE PROCEDURE [edi].[ProcessSelfHostedUsageData]
	@period int
AS

SET NOCOUNT ON;

--In this SP, update all matching records in staging to have a ProcessingStatus=255, which tells the broader scripts to move the records into the edi.Usage table.

WITH H as
(
    select *,
	ROW_NUMBER() OVER(PARTITION BY DatabaseNumber ORDER BY ValidFromUtc DESC) AS LicenseRow
	from edi.LicenceDatabaseCodeHistory
)

UPDATE SB
SET ProcessingStatus = 255
FROM edi.StagingBatch SB
INNER JOIN H ON SB.DatabaseNumber = H.DatabaseNumber AND H.LicenseRow = 1
	INNER JOIN edi.BillingRules BR ON BR_SPName = 'ProcessSelfHostedUsageData'
		AND (
				(
				BR_Fields = 'Category-PriceItemCode'
				AND SB.TX_Category = BR.BR_Value1 
				AND SB.TX_PriceItemCode = BR.BR_Value2
				)
			)
	WHERE TX_Period = @Period
		AND ProcessingStatus != 255
		AND HostedLocation = 'NCW'
RETURN 0

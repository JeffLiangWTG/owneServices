CREATE PROCEDURE [edi].[ProcessSPM]
	@Period int
AS

SET NOCOUNT ON;

-- Multiple messages for the same client, ref1, ref4 and service time are converted to a single message
-- by summing Ref5 container counts.
-- Ref2, Ref3 and other columns are also the same so can just pick the MIN value for the result.
-- Note this is done before reference swaps
-- Reference5 is set to blank in the new message to prevent double processing.

-- Change the price item code of the original records to a dummy value since we don't want the originals recorded under that code
update edi.StagingBatch
set TX_PriceItemCode = 'XX1',
	ProcessingStatus = 99 -- no further processing
where
	TX_Period = @Period
	AND TX_Category = 'SPM'
	AND TX_PriceItemCode = 'SPA'
	AND TX_Reference5 != '';

-- Calculate a single record from each group and insert with the original price code
insert edi.StagingBatch(
	TX_ID,
	TX_Period,
	TX_Category,
	TX_PriceItemCode,
	DatabaseNumber,
	CompanyNumber,
	TX_ClientNumber,
	TX_Reference1,
	TX_Reference4,
	TX_ServiceOccuredUTC,
	TX_BillableCount,
	TX_ReportingSource,
	TX_ClientID,
	TX_Reference2,
	TX_Reference3,
	TX_Reference5,
	TX_ClientStaffCode,
	TX_Version,
	TX_Branch,
	TX_SystemCreateUTC,
	TX_MessageTrackingID)
SELECT
	TX_ID = MIN(TX_ID),
	TX_Period = @Period,
	TX_Category = 'SPM',
	TX_PriceItemCode = 'SPA',
	DatabaseNumber,
	CompanyNumber,
	TX_ClientNumber,
	TX_Reference1,
	TX_Reference4,
	TX_ServiceOccuredUTC,
	TX_BillableCount = SUM(CAST(TX_Reference5 AS INT)),
	TX_ReportingSource = MIN(TX_ReportingSource),
	TX_ClientID = MIN(TX_ClientID),
	TX_Reference2 = MIN(TX_Reference2),
	TX_Reference3 = MIN(TX_Reference3),
	TX_Reference5 = '',
	TX_ClientStaffCode = MIN(TX_ClientStaffCode),
	TX_Version = MIN(TX_Version),
	TX_Branch = MIN(TX_Branch),
	TX_SystemCreateUTC = MIN(TX_SystemCreateUTC),
	TX_MessageTrackingID = MIN(TX_MessageTrackingID)
FROM
	edi.StagingBatch
WHERE
	TX_Period = @Period
	and TX_Category = 'SPM'
	AND TX_PriceItemCode = 'XX1'
GROUP BY
	DatabaseNumber,
	CompanyNumber,
	TX_Reference1,
	TX_Reference4,
	TX_ClientNumber,
	TX_ClientID,
	TX_ServiceOccuredUTC

RETURN 0

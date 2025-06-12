CREATE PROCEDURE edi.ProcessSimpleChargeable
	@Period int,
	@isTestServer bit
AS
/*
Transactions with a simple charge basis go directly in the Chargeable table,
except for duplicates which are ignored.
Those that don't match a Database and Company go into usage.

Chargeable <= ProcessingStatus = 0 or 1, and Database != 0 and Company != 0  
StagingUnknownSystems <= ProcessingStatus = 0 or 1, and (Database = 0 or Company = 0)
Usage <= ProcessingStatus = 255
*/

declare @utcNow DATETIME2(0) = sysutcdatetime();

insert edi.Chargeable(CH_ID
	, CH_Period
	, CH_Category
	, CH_PriceItemCode
	, CH_DatabaseNumber
	, CH_CompanyNumber
	, CH_BillableCount
	, CH_ReportingSource
	, CH_ServiceOccuredUtc
	, CH_ClientID
	, CH_ClientNumber
	, CH_ClientStaffCode
	, CH_Reference1
	, CH_Reference2
	, CH_Reference3
	, CH_Reference4
	, CH_Reference5
	, CH_Version
	, CH_Branch
	, CH_CapturedUtc
	, CH_SystemCreateUtc
	, CH_SystemLastEditUtc
	, CH_MessageTrackingID)
select TX_ID
	, TX_Period
	, TX_Category
	, TX_PriceItemCode
	, DatabaseNumber
	, CompanyNumber
	, TX_BillableCount
	, TX_ReportingSource
	, TX_ServiceOccuredUTC
	, TX_ClientID
	, TX_ClientNumber
	, TX_ClientStaffCode
	, TX_Reference1
	, TX_Reference2
	, TX_Reference3
	, TX_Reference4
	, TX_Reference5
	, TX_Version
	, TX_Branch
	, CapturedUtc = TX_SystemCreateUTC
	, @utcNow
	, @utcNow
	, TX_MessageTrackingID
from edi.StagingBatch
where TX_Period = @Period
	and ((@isTestServer = 1) OR (DatabaseNumber != 0 and CompanyNumber != 0))
	and ProcessingStatus in (0, 1);

insert edi.Usage(US_ID
	, US_Period
	, US_Category
	, US_PriceItemCode
	, US_DatabaseNumber
	, US_CompanyNumber
	, US_BillableCount
	, US_ReportingSource
	, US_ServiceOccuredUtc
	, US_ClientID
	, US_ClientNumber
	, US_ClientStaffCode
	, US_Reference1
	, US_Reference2
	, US_Reference3
	, US_Reference4
	, US_Reference5
	, US_Version
	, US_Branch
	, US_CapturedUtc
	, US_SystemCreateUtc
	, US_SystemLastEditUtc
	, US_MessageTrackingID)
select TX_ID 
	, @Period
	, TX_Category
	, TX_PriceItemCode
	, DatabaseNumber
	, CompanyNumber
	, TX_BillableCount
	, TX_ReportingSource
	, TX_ServiceOccuredUTC
	, TX_ClientID
	, TX_ClientNumber
	, TX_ClientStaffCode
	, TX_Reference1
	, TX_Reference2
	, TX_Reference3
	, TX_Reference4
	, TX_Reference5
	, TX_Version
	, TX_Branch
	, CapturedUtc = TX_SystemCreateUTC
	, @utcNow
	, @utcNow
	, TX_MessageTrackingID
from edi.StagingBatch
where TX_Period = @period and ProcessingStatus = 255;

insert edi.StagingUnknownSystems(TX_ID
	, TX_Category
	, TX_PriceItemCode
	, DatabaseNumber
	, CompanyNumber
	, TX_BillableCount
	, TX_ReportingSource
	, TX_ServiceOccuredUtc
	, TX_ClientID
	, TX_ClientNumber
	, TX_ClientStaffCode
	, TX_Reference1
	, TX_Reference2
	, TX_Reference3
	, TX_Reference4
	, TX_Reference5
	, TX_Version
	, TX_Branch
	, TX_SystemCreateUtc
	, TX_MessageTrackingID
	, TX_Period)
select TX_ID
	, TX_Category
	, TX_PriceItemCode
	, DatabaseNumber
	, CompanyNumber
	, TX_BillableCount
	, TX_ReportingSource
	, TX_ServiceOccuredUTC
	, TX_ClientID
	, TX_ClientNumber
	, TX_ClientStaffCode
	, TX_Reference1
	, TX_Reference2
	, TX_Reference3
	, TX_Reference4
	, TX_Reference5
	, TX_Version
	, TX_Branch
	, TX_SystemCreateUTC
	, TX_MessageTrackingID
	, TX_Period
from edi.StagingBatch batch
where TX_Period = @period
	and (((@isTestServer = 0) AND (DatabaseNumber = 0 or CompanyNumber = 0)) and ProcessingStatus in (0, 1))
	and NOT EXISTS (SELECT * FROM edi.StagingUnknownSystems existingUnknown WHERE batch.TX_ID = existingUnknown.TX_ID)

DELETE UK FROM edi.StagingUnknownSystems UK
INNER JOIN edi.StagingBatch batch ON UK.TX_ID = batch.TX_ID
WHERE UK.TX_Period = @period AND batch.TX_Period = @period and @isTestServer = 0 AND batch.DatabaseNumber != 0 AND batch.CompanyNumber != 0

RETURN 0

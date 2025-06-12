CREATE PROCEDURE [dbo].[BillingTransactionInsertMany]
	@Transactions TVP_BillingTransaction READONLY 
AS
SET NOCOUNT ON;

declare @utcNow DATETIME2(0) = sysutcdatetime();

insert dbo.Staging
      (TX_PriceItemCode, TX_BillableCount, TX_ReportingSource, TX_ServiceOccuredUTC, TX_ClientID, TX_ClientNumber, TX_ClientStaffCode, TX_Reference1, TX_Reference2, TX_Reference3, TX_Reference4, TX_SystemCreateUTC, TX_Version, TX_Category, TX_Branch, TX_Reference5, TX_MessageTrackingID)
select    PriceItemCode,    BillableCount,    ReportingSource,    ServiceOccuredUTC,    ClientID,    ClientNumber,    ClientStaffCode,    Reference1,    Reference2,    Reference3,    Reference4, @utcNow,               Version,    Category,    Branch,    Reference5,    messageTrackingID
from @Transactions
	
RETURN 0

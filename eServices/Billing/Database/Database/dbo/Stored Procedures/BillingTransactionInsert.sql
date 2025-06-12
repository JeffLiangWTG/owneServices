CREATE PROCEDURE [dbo].[BillingTransactionInsert]
    @PriceItemCode     VARCHAR (3),
    @BillableCount     INT,
    @ReportingSource   VARCHAR (3),
    @ServiceOccuredUTC DATETIME2,
    @ClientID          VARCHAR (9),
    @ClientNumber      VARCHAR (50),
    @ClientStaffCode   VARCHAR (3),
    @Version           INT,
    @Category          VARCHAR (3),
    @Branch            VARCHAR (3),
    @Reference1        VARCHAR (50),
    @Reference2        VARCHAR (50),
    @Reference3        VARCHAR (50),
    @Reference4        VARCHAR (50),
    @Reference5        VARCHAR (50),
	@messageTrackingID VARCHAR (36) = NULL
AS
SET NOCOUNT ON;

insert dbo.Staging
      (TX_PriceItemCode, TX_BillableCount, TX_ReportingSource, TX_ServiceOccuredUTC, TX_ClientID, TX_ClientNumber, TX_ClientStaffCode, TX_Reference1, TX_Reference2, TX_Reference3, TX_Reference4, TX_SystemCreateUTC, TX_Version, TX_Category, TX_Branch, TX_Reference5, TX_MessageTrackingID)
VALUES ( @PriceItemCode,   @BillableCount,   @ReportingSource,   @ServiceOccuredUTC,   @ClientID,   @ClientNumber,   @ClientStaffCode,   @Reference1,   @Reference2,   @Reference3,   @Reference4, sysutcdatetime()  ,   @Version,   @Category,   @Branch,   @Reference5,   @messageTrackingID)

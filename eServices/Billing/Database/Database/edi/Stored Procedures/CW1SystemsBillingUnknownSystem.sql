CREATE PROCEDURE [edi].[CW1SystemsBillingUnknownSystem]
	@DaysThreshold INT = 1
AS
BEGIN
    WITH StuckTransactions AS (
        SELECT *
        FROM edi.StagingUnknownSystems
        WHERE DATEDIFF(day, TX_SystemCreateUTC, GETDATE()) > @DaysThreshold
    ),

    RecentActiveLicenses AS (
        SELECT *
        FROM dbo.GetLatestLicense()
    )

    SELECT 
        ST.TX_Category, 
        ST.TX_PriceItemCode, 
        ST.TX_BillableCount, 
        ST.TX_ServiceOccuredUTC,
		ST.TX_SystemCreateUTC,
        ST.TX_ClientID, 
        ST.TX_ClientNumber, 
        ST.DatabaseNumber, 
        ST.CompanyNumber
    FROM 
        StuckTransactions ST
    LEFT JOIN 
        RecentActiveLicenses RAL
    ON 
        ST.DatabaseNumber = RAL.DatabaseNumber
	WHERE 
        (ST.DatabaseNumber IS NULL OR RAL.DatabaseNumber IS NULL)
		OR  RAL.IsActive = 1
END;

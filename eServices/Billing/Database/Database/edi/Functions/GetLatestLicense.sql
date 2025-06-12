CREATE FUNCTION dbo.GetLatestLicense()
RETURNS TABLE
AS
RETURN
(
    WITH H AS
    (
        SELECT 
            EnterpriseCode, 
            DatabaseNumber, 
            ServerCode, 
            HostedLocation, 
            IsActive, 
            IsTeardownInProgress,
            ROW_NUMBER() OVER(PARTITION BY DatabaseNumber ORDER BY ValidFromUtc DESC) AS LicenseRow
        FROM edi.LicenceDatabaseCodeHistory
    )
    SELECT 
        EnterpriseCode, 
        DatabaseNumber, 
        ServerCode, 
        HostedLocation, 
        IsActive, 
        IsTeardownInProgress
    FROM H
    WHERE LicenseRow = 1
);

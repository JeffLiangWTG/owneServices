CREATE PROCEDURE ediProd.SyncLicenceDatabase
    @RowState VARCHAR(10),
    @LD_PK UNIQUEIDENTIFIER,
    @LD_ServerCode VARCHAR(3),
    @LD_LicenceType VARCHAR(3),
    @LD_LE UNIQUEIDENTIFIER,
    @LD_IsActive BIT,
    @LD_DatabaseNumber INT,
    @LD_Product VARCHAR(3),
    @LD_HostedLocation CHAR(3),
    @LD_TenantID VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        IF @RowState NOT IN ('Added', 'Modified', 'Deleted')
            RAISERROR ('Invalid RowState value. Must be Added, Modified, or Deleted.', 16, 1);
            
        IF @LD_PK IS NULL
            RAISERROR ('LD_PK cannot be null.', 16, 1);

        IF @RowState = 'Added'
        BEGIN
            INSERT INTO ediProd.LicenceDatabase (
                LD_PK,
                LD_ServerCode,
                LD_LicenceType,
                LD_LE,
                LD_IsActive,
                LD_DatabaseNumber,
                LD_Product,
                LD_HostedLocation,
                LD_TenantID
            )
            VALUES (
                @LD_PK,
                @LD_ServerCode,
                @LD_LicenceType,
                @LD_LE,
                @LD_IsActive,
                @LD_DatabaseNumber,
                @LD_Product,
                @LD_HostedLocation,
                @LD_TenantID
            );
        END
        ELSE IF @RowState = 'Modified'
        BEGIN
            UPDATE ediProd.LicenceDatabase
            SET LD_ServerCode = @LD_ServerCode,
                LD_LicenceType = @LD_LicenceType,
                LD_LE = @LD_LE,
                LD_IsActive = @LD_IsActive,
                LD_DatabaseNumber = @LD_DatabaseNumber,
                LD_Product = @LD_Product,
                LD_HostedLocation = @LD_HostedLocation,
                LD_TenantID = @LD_TenantID
            WHERE LD_PK = @LD_PK;
            
            IF @@ROWCOUNT = 0
                RAISERROR ('No record found to modify.', 16, 1);
        END
        ELSE IF @RowState = 'Deleted'
        BEGIN
            DELETE FROM ediProd.LicenceDatabase
            WHERE LD_PK = @LD_PK;
            
            IF @@ROWCOUNT = 0
                RAISERROR ('No record found to delete.', 16, 1);
        END
        
        RETURN 0;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();

        RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
        RETURN -1;
    END CATCH
END;
GO

CREATE PROCEDURE ediProd.SyncLicenceEnterprise
    @RowState VARCHAR(10),
    @LE_PK UNIQUEIDENTIFIER,
    @LE_EnterpriseCode VARCHAR(3),
    @LE_IsInternal BIT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        IF @RowState NOT IN ('Added', 'Modified', 'Deleted')
            RAISERROR ('Invalid RowState value. Must be Added, Modified, or Deleted.', 16, 1);
            
        IF @LE_PK IS NULL
            RAISERROR ('LE_PK cannot be null.', 16, 1);

        IF @RowState = 'Added'
        BEGIN
            INSERT INTO ediProd.LicenceEnterprise (
                LE_PK,
                LE_EnterpriseCode,
                LE_IsInternal
            )
            VALUES (
                @LE_PK,
                @LE_EnterpriseCode,
                @LE_IsInternal
            );
        END
        ELSE IF @RowState = 'Modified'
        BEGIN
            UPDATE ediProd.LicenceEnterprise
            SET LE_EnterpriseCode = @LE_EnterpriseCode,
                LE_IsInternal = @LE_IsInternal
            WHERE LE_PK = @LE_PK;
            
            IF @@ROWCOUNT = 0
                RAISERROR ('No record found to modify.', 16, 1);
        END
        ELSE IF @RowState = 'Deleted'
        BEGIN
            DELETE FROM ediProd.LicenceEnterprise
            WHERE LE_PK = @LE_PK;
            
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

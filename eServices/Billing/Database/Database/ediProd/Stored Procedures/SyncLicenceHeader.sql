CREATE PROCEDURE ediProd.SyncLicenceHeader
    @RowState VARCHAR(10),
    @LA_PK UNIQUEIDENTIFIER,
    @LA_LD UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        IF @RowState NOT IN ('Added', 'Modified', 'Deleted')
            RAISERROR ('Invalid RowState value. Must be Added, Modified, or Deleted.', 16, 1);
            
        IF @LA_PK IS NULL
            RAISERROR ('LA_PK cannot be null.', 16, 1);

        IF @RowState = 'Added'
        BEGIN
            INSERT INTO ediProd.LicenceHeader (
                LA_PK,
                LA_LD
            )
            VALUES (
                @LA_PK,
                @LA_LD
            );
        END
        ELSE IF @RowState = 'Modified'
        BEGIN
            UPDATE ediProd.LicenceHeader
            SET LA_LD = @LA_LD
            WHERE LA_PK = @LA_PK;
            
            IF @@ROWCOUNT = 0
                RAISERROR ('No record found to modify.', 16, 1);
        END
        ELSE IF @RowState = 'Deleted'
        BEGIN
            DELETE FROM ediProd.LicenceHeader
            WHERE LA_PK = @LA_PK;
            
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

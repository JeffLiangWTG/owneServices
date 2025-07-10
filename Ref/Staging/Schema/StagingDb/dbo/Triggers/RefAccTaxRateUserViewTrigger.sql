CREATE TRIGGER RefAccTaxRateUserView_Version_Create ON RefAccTaxRateUserView
INSTEAD OF INSERT
AS
BEGIN

	DECLARE @sourceId uniqueidentifier;
	SELECT TOP 1 @sourceId = SDA_PK FROM SourceData
	WHERE SDA_Source = 'USR' AND SDA_SubSource = 'RefAccTaxRateUserView'
	
	IF @sourceId IS NULL
	BEGIN
		SET @sourceId = newid();
		INSERT INTO SourceData (SDA_PK, SDA_Source, SDA_Filetype, SDA_ContentText, SDA_Status, SDA_SourceTime, SDA_SubSource)
		VALUES (@sourceID, 'USR', 'TXT', '', 'PRS', GETUTCDATE(), 'RefAccTaxRateUserView')
	END
	
	INSERT dbo.RefAccTaxRate (ZAT_PK, ZAT_RN_NKCountry, ZAT_ReferenceRateType, ZAT_StartDate, ZAT_EndDate, ZAT_RateNumerator, ZAT_RateDenominator)
	SELECT ZAT_PK, ZAT_RN_NKCountry, ZAT_ReferenceRateType, ZAT_StartDate, ZAT_EndDate, ZAT_RateNumerator, ZAT_RateDenominator
	FROM inserted
	
	INSERT dbo.DataProcessingInformation (DPI_ID, DPI_ParentPk, DPI_ParentTableCode, DPI_Status, DPI_SourceId, DPI_Message)
	SELECT newid(), ZAT_PK, 'ZAT', 'QUE', @sourceId, CASE WHEN ZAT_IsPublished = 0 THEN 'Delete' ELSE '' END
	FROM inserted

END
GO

CREATE TRIGGER RefAccTaxRateUserView_Version_Update ON RefAccTaxRateUserView
INSTEAD OF UPDATE
AS
BEGIN
	UPDATE r SET
		r.ZAT_RN_NKCountry = i.ZAT_RN_NKCountry,
		r.ZAT_ReferenceRateType = i.ZAT_ReferenceRateType,
		r.ZAT_StartDate = i.ZAT_StartDate,
		r.ZAT_EndDate = i.ZAT_EndDate,
		r.ZAT_RateNumerator = i.ZAT_RateNumerator,
		r.ZAT_RateDenominator = i.ZAT_RateDenominator
	FROM RefAccTaxRate r
	JOIN inserted i ON r.ZAT_PK = i.ZAT_PK

	UPDATE d SET d.DPI_Message = CASE WHEN ZAT_IsPublished = 0 THEN 'Delete' ELSE '' END
	FROM DataProcessingInformation d
	JOIN inserted i ON d.DPI_ParentPk = i.ZAT_PK
	WHERE DPI_ParentTableCode = 'ZAT'
END
GO
CREATE TRIGGER RefAccTaxRateUserView_Version_Delete ON RefAccTaxRateUserView
INSTEAD OF DELETE
AS
BEGIN
	DELETE r
	FROM deleted d
	JOIN RefAccTaxRate r on r.ZAT_PK = d.ZAT_PK
	DELETE r
	FROM deleted d
	JOIN DataProcessingInformation r on r.DPI_ParentPk = d.ZAT_PK
	WHERE DPI_ParentTableCode = 'ZAT'
END
GO

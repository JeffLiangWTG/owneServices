CREATE VIEW [dbo].[RefAccTaxRateUserView]
AS

SELECT
	ZAT_PK AS ZAT_PK,
	ZAT_RN_NKCountry AS ZAT_RN_NKCountry,
	ZAT_ReferenceRateType AS ZAT_ReferenceRateType,
	ZAT_StartDate AS ZAT_StartDate,
	ZAT_EndDate AS ZAT_EndDate,
	ZAT_RateNumerator AS ZAT_RateNumerator,
	ZAT_RateDenominator AS ZAT_RateDenominator,
	CAST(0 AS BIT) AS ZAT_IsSystem,
	CASE WHEN DPI_Message = 'Delete' THEN CAST (0 AS BIT) ELSE CAST(1 AS BIT) END AS ZAT_IsPublished,
	ISNULL(CAST (1 AS BIT), 1) AS ZAT_IsEditable
FROM [dbo].RefAccTaxRate
JOIN [dbo].DataProcessingInformation ON DPI_ParentPk = ZAT_PK AND DPI_ParentTableCode ='ZAT'
JOIN [dbo].SourceData ON DPI_SourceId = SDA_PK
WHERE SDA_Source = 'USR' AND SDA_SubSource = 'RefAccTaxRateUserView'
GO

CREATE PROCEDURE [dbo].[RefAccTaxRateUserView_Ins]
	@ZAT_PK uniqueidentifier,
	@ZAT_RN_NKCountry char(2),
	@ZAT_ReferenceRateType varchar(10),
	@ZAT_StartDate smalldatetime,
	@ZAT_EndDate smalldatetime,
	@ZAT_RateNumerator int,
	@ZAT_RateDenominator int,
	@ZAT_IsPublished bit
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
VALUES (@ZAT_PK, @ZAT_RN_NKCountry, @ZAT_ReferenceRateType, @ZAT_StartDate, @ZAT_EndDate, @ZAT_RateNumerator, @ZAT_RateDenominator)

INSERT dbo.DataProcessingInformation (DPI_ID, DPI_ParentPk, DPI_ParentTableCode, DPI_Status, DPI_SourceId, DPI_Message)
VALUES (newid(), @ZAT_PK, 'ZAT', 'QUE', @sourceId, CASE WHEN @ZAT_IsPublished = 0 THEN 'Delete' ELSE '' END)

END
GO

CREATE PROCEDURE [dbo].[RefAccTaxRateUserView_Del]
	@ZAT_PK uniqueidentifier
AS
DELETE RefAccTaxRate WHERE ZAT_PK = @ZAT_PK
DELETE DataProcessingInformation WHERE DPI_ParentPk = @ZAT_PK AND DPI_ParentTableCode = 'ZAT'
GO

CREATE PROCEDURE [dbo].[RefAccTaxRateUserView_Ups]
	@ZAT_PK uniqueidentifier,
	@ZAT_RN_NKCountry char(2),
	@ZAT_ReferenceRateType varchar(10),
	@ZAT_StartDate smalldatetime,
	@ZAT_EndDate smalldatetime,
	@ZAT_RateNumerator int,
	@ZAT_RateDenominator int,
	@ZAT_IsPublished bit
AS
BEGIN

UPDATE RefAccTaxRate SET
	ZAT_RN_NKCountry = @ZAT_RN_NKCountry,
	ZAT_ReferenceRateType = @ZAT_ReferenceRateType,
	ZAT_StartDate = @ZAT_StartDate,
	ZAT_EndDate = @ZAT_EndDate,
	ZAT_RateNumerator = @ZAT_RateNumerator,
	ZAT_RateDenominator = @ZAT_RateDenominator
WHERE ZAT_PK = @ZAT_PK

UPDATE DataProcessingInformation SET DPI_Message = CASE WHEN @ZAT_IsPublished = 0 THEN 'Delete' ELSE '' END
WHERE DPI_ParentPk = @ZAT_PK AND DPI_ParentTableCode = 'ZAT'
END

CREATE VIEW [dbo].[RefCusCodeListUserView]
AS

SELECT
	ZZD_PK AS ZZD_PK,
	ZZD_ZZZ_NKDataGrouping AS ZZD_CountryOrGrouping,
	ZZD_ZZK_NKCodeType AS ZZD_CodeType,
	ZZD_Code AS ZZD_Code,
	ZZD_Description AS ZZD_Description,
	ZZD_StartDate AS ZZD_StartDate,
	ZZD_EndDate AS ZZD_EndDate,
	ISNULL(CAST(ZZD_IsAir AS BIT), 0) AS ZZD_IsAir,
	ISNULL(CAST(ZZD_IsSea AS BIT), 0) AS ZZD_IsSea,
	ISNULL(CAST(ZZD_IsFix AS BIT), 0) AS ZZD_IsFix,
	ISNULL(CAST(ZZD_IsRai AS BIT), 0) AS ZZD_IsRai,
	ISNULL(CAST(ZZD_IsRoa AS BIT), 0) AS ZZD_IsRoa,
	ISNULL(CAST(ZZD_IsMai AS BIT), 0) AS ZZD_IsMai,
	ISNULL(CAST(ZZD_IsInw AS BIT), 0) AS ZZD_IsInw,
	ISNULL(CAST(0 AS BIT), 0) AS ZZD_IsSystem,
	CASE WHEN DPI_Message = 'Delete' THEN CAST(0 AS BIT) ELSE CAST(1 AS BIT) END AS ZZD_IsPublished,
	ISNULL(CAST (1 AS BIT), 1) AS ZZD_IsEditable
FROM [dbo].RefCusCodeList
JOIN [dbo].DataProcessingInformation ON DPI_ParentPk = ZZD_PK AND DPI_ParentTableCode = 'ZZD'
JOIN [dbo].SourceData ON DPI_SourceId = SDA_PK

LEFT JOIN
(
	SELECT ZZU_ZZD_CodeList,
		   MAX(CASE WHEN ZZU_TransportMode = 'AIR' THEN 1 END) AS ZZD_IsAir,
		   MAX(CASE WHEN ZZU_TransportMode = 'SEA' THEN 1 END) AS ZZD_IsSea,
		   MAX(CASE WHEN ZZU_TransportMode = 'FIX' THEN 1 END) AS ZZD_IsFix,
		   MAX(CASE WHEN ZZU_TransportMode = 'RAI' THEN 1 END) AS ZZD_IsRai,
		   MAX(CASE WHEN ZZU_TransportMode = 'ROA' THEN 1 END) AS ZZD_IsRoa,
		   MAX(CASE WHEN ZZU_TransportMode = 'MAI' THEN 1 END) AS ZZD_IsMai,
		   MAX(CASE WHEN ZZU_TransportMode = 'INW' THEN 1 END) AS ZZD_IsInw

	FROM [dbo].RefCusCodeOrAttributeTransportMode
	GROUP BY ZZU_ZZD_CodeList
) TransportModes ON ZZU_ZZD_CodeList = ZZD_PK
WHERE SDA_Source = 'USR' AND SDA_SubSource = 'RefCusCodeListUserView'

GO

CREATE PROCEDURE [dbo].[RefCusCodeListUserView_Ins]
	@ZZD_PK uniqueidentifier,
	@ZZD_CodeType varchar(5),
	@ZZD_Code varchar(35),
	@ZZD_Description nvarchar(MAX),
	@ZZD_StartDate smalldatetime,
	@ZZD_EndDate smalldatetime,
	@ZZD_CountryOrGrouping varchar(3),
	@ZZD_IsAir bit,
	@ZZD_IsSea bit,
	@ZZD_IsFix bit,
	@ZZD_IsRai bit,
	@ZZD_IsRoa bit,
	@ZZD_IsMai bit,
	@ZZD_IsInw bit,
	@ZZD_IsPublished bit
AS
BEGIN

DECLARE @sourceId uniqueidentifier;
SELECT TOP 1 @sourceId = SDA_PK FROM SourceData
WHERE SDA_Source = 'USR' AND SDA_SubSource = 'RefCusCodeListUserView'

IF @sourceId IS NULL
BEGIN
	SET @sourceId = newid();
	INSERT INTO SourceData (SDA_PK, SDA_Source, SDA_Filetype, SDA_ContentText, SDA_Status, SDA_SourceTime, SDA_SubSource)
	VALUES (@sourceId, 'USR', 'TXT', '', 'PRS', GETUTCDATE(), 'RefCusCodeListUserView')
END

INSERT dbo.RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping)
VALUES (@ZZD_PK, @ZZD_CodeType, @ZZD_Code, @ZZD_Description, @ZZD_StartDate, @ZZD_EndDate, @ZZD_CountryOrGrouping)

INSERT dbo.DataProcessingInformation (DPI_ID, DPI_ParentPk, DPI_ParentTableCode, DPI_Status, DPI_SourceId, DPI_Message)
VALUES (newid(), @ZZD_PK, 'ZZD', 'QUE', @sourceId, CASE WHEN @ZZD_IsPublished = 0 THEN 'Delete' ELSE '' END)

INSERT dbo.RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)
SELECT newid(), TransportMode, @ZZD_PK
FROM
(
	SELECT TransportMode,
		CASE WHEN TransportMode = 'AIR' AND @ZZD_IsAir = 1 THEN 'Y'
				WHEN TransportMode = 'SEA' AND @ZZD_IsSea = 1 THEN 'Y'
				WHEN TransportMode = 'FIX' AND @ZZD_IsFix = 1 THEN 'Y'
				WHEN TransportMode = 'RAI' AND @ZZD_IsRai = 1 THEN 'Y'
				WHEN TransportMode = 'ROA' AND @ZZD_IsRoa = 1 THEN 'Y'
				WHEN TransportMode = 'MAI' AND @ZZD_IsMai = 1 THEN 'Y'
				WHEN TransportMode = 'INW' AND @ZZD_IsInw = 1 THEN 'Y'
		END AS Flag
	FROM (SELECT 'ROA' TransportMode UNION SELECT 'SEA' UNION SELECT 'AIR' UNION SELECT 'RAI' UNION SELECT 'MAI' UNION SELECT 'FIX' UNION SELECT 'INW') TransportModeList
) TransportModes
WHERE Flag IS NOT NULL
END
GO

CREATE PROCEDURE [dbo].[RefCusCodeListUserView_Del]
	@ZZD_PK uniqueidentifier
AS
DELETE RefCusCodeOrAttributeTransportMode WHERE ZZU_ZZD_CodeList = @ZZD_PK
DELETE RefCusCodeList WHERE ZZD_PK = @ZZD_PK
DELETE DataProcessingInformation WHERE DPI_ParentPk = @ZZD_PK AND DPI_ParentTableCode ='ZZD'

GO

CREATE PROCEDURE [dbo].[RefCusCodeListUserView_Ups]
	@ZZD_PK uniqueidentifier,
	@ZZD_CodeType varchar(5),
	@ZZD_Code varchar(35),
	@ZZD_Description nvarchar(MAX),
	@ZZD_StartDate smalldatetime,
	@ZZD_EndDate smalldatetime,
	@ZZD_CountryOrGrouping varchar(3),
	@ZZD_IsAir bit,
	@ZZD_IsSea bit,
	@ZZD_IsFix bit,
	@ZZD_IsRai bit,
	@ZZD_IsRoa bit,
	@ZZD_IsMai bit,
	@ZZD_IsInw bit,
	@ZZD_IsPublished bit
AS
BEGIN

UPDATE RefCusCodeList SET
	ZZD_ZZZ_NKDataGrouping = @ZZD_CountryOrGrouping,
	ZZD_ZZK_NKCodeType = @ZZD_CodeType,
	ZZD_Code = @ZZD_Code,
	ZZD_Description = @ZZD_Description,
	ZZD_StartDate = @ZZD_StartDate,
	ZZD_EndDate = @ZZD_EndDate
WHERE ZZD_PK = @ZZD_PK

UPDATE DataProcessingInformation SET DPI_Message =  CASE WHEN @ZZD_IsPublished = 0 THEN 'Delete' ELSE '' END
WHERE DPI_ParentPk = @ZZD_PK AND DPI_ParentTableCode = 'ZZD'

MERGE dbo.RefCusCodeOrAttributeTransportMode AS T
USING 
(
	SELECT TransportMode, 
			CASE WHEN TransportMode = 'AIR' AND @ZZD_IsAir = 1 THEN 'Y'
						WHEN TransportMode = 'SEA' AND @ZZD_IsSea = 1 THEN 'Y'
						WHEN TransportMode = 'FIX' AND @ZZD_IsFix = 1 THEN 'Y'
						WHEN TransportMode = 'RAI' AND @ZZD_IsRai = 1 THEN 'Y'
						WHEN TransportMode = 'ROA' AND @ZZD_IsRoa = 1 THEN 'Y'
						WHEN TransportMode = 'MAI' AND @ZZD_IsMai = 1 THEN 'Y'
						WHEN TransportMode = 'INW' AND @ZZD_IsInw = 1 THEN 'Y'
		ELSE 'N' END AS Flag FROM
		(SELECT 'ROA' TransportMode UNION SELECT 'SEA' UNION SELECT 'AIR' UNION SELECT 'RAI' UNION SELECT 'MAI' UNION SELECT 'FIX' UNION SELECT 'INW') TransportModeList
) S
ON T.ZZU_ZZD_CodeList = @ZZD_PK AND T.ZZU_TransportMode = S.TransportMode
WHEN NOT MATCHED AND S.Flag = 'Y'
	THEN INSERT (ZZU_PK, ZZU_ZZD_CodeList, ZZU_TransportMode)
		VALUES (newid(), @ZZD_PK, S.TransportMode)
WHEN MATCHED AND S.Flag = 'N'
	THEN DELETE;

END

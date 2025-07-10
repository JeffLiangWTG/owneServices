CREATE VIEW [dbo].[RefCusCodeListAttributeUserView]
WITH SCHEMABINDING
AS
SELECT
	ZZE_PK AS ZZE_PK,
	ZZE_ZZD_CodeList AS ZZE_ZZD_CodeList,
	ZZE_ZXE_NKName AS ZZE_ZXE_NKName,
	ZZE_Value AS ZZE_Value,
	ZZE_StartDate AS ZZE_StartDate,
	ZZE_EndDate AS ZZE_EndDate,
	ISNULL(CAST(ZZE_IsAir AS BIT), 0) AS ZZE_IsAir,
	ISNULL(CAST(ZZE_IsSea AS BIT), 0) AS ZZE_IsSea,
	ISNULL(CAST(ZZE_IsFix AS BIT), 0) AS ZZE_IsFix,
	ISNULL(CAST(ZZE_IsRai AS BIT), 0) AS ZZE_IsRai,
	ISNULL(CAST(ZZE_IsRoa AS BIT), 0) AS ZZE_IsRoa,
	ISNULL(CAST(ZZE_IsMai AS BIT), 0) AS ZZE_IsMai,
	ISNULL(CAST(ZZE_IsInw AS BIT), 0) AS ZZE_IsInw,
	ZZD_ZZK_NKCodeType AS ZZE_CodeType,
	ZZD_ZZZ_NKDataGrouping AS ZZE_CountryOrGrouping

FROM [dbo].RefCusCodeListAttribute
JOIN [dbo].RefCusCodeList ON ZZE_ZZD_CodeList = ZZD_PK

LEFT JOIN
(
	SELECT ZZU_ZZE_Attribute,
		   MAX(CASE WHEN ZZU_TransportMode = 'AIR' THEN 1 END) AS ZZE_IsAir,
		   MAX(CASE WHEN ZZU_TransportMode = 'SEA' THEN 1 END) AS ZZE_IsSea,
		   MAX(CASE WHEN ZZU_TransportMode = 'FIX' THEN 1 END) AS ZZE_IsFix,
		   MAX(CASE WHEN ZZU_TransportMode = 'RAI' THEN 1 END) AS ZZE_IsRai,
		   MAX(CASE WHEN ZZU_TransportMode = 'ROA' THEN 1 END) AS ZZE_IsRoa,
		   MAX(CASE WHEN ZZU_TransportMode = 'MAI' THEN 1 END) AS ZZE_IsMai,
		   MAX(CASE WHEN ZZU_TransportMode = 'INW' THEN 1 END) AS ZZE_IsInw
	FROM [dbo].RefCusCodeOrAttributeTransportMode
	GROUP BY ZZU_ZZE_Attribute
) TransportModes ON ZZU_ZZE_Attribute = ZZE_PK

GO

CREATE PROCEDURE [dbo].[RefCusCodeListAttributeUserView_Ins]
	@ZZE_PK uniqueidentifier,
	@ZZE_Value nvarchar(255),
	@ZZE_ZXE_NKName varchar(32),
	@ZZE_ZZD_CodeList uniqueidentifier,
	@ZZE_IsAir bit,
	@ZZE_IsSea bit,
	@ZZE_IsFix bit,
	@ZZE_IsRai bit,
	@ZZE_IsRoa bit,
	@ZZE_IsMai bit,
	@ZZE_IsInw bit,
	@ZZE_CodeType varchar(5),
	@ZZE_CountryOrGrouping varchar(3),
	@ZZE_StartDate smalldatetime,
	@ZZE_EndDate smalldatetime
AS
BEGIN

INSERT dbo.RefCusCodeListAttribute (ZZE_PK, ZZE_Value, ZZE_ZXE_NKName, ZZE_ZZD_CodeList, ZZE_StartDate, ZZE_EndDate)
SELECT @ZZE_PK, @ZZE_Value, @ZZE_ZXE_NKName, @ZZE_ZZD_CodeList, @ZZE_StartDate, @ZZE_EndDate
FROM dbo.RefCusCodeList WHERE ZZD_PK = @ZZE_ZZD_CodeList AND ZZD_ZZK_NKCodeType = @ZZE_CodeType AND ZZD_ZZZ_NKDataGrouping = @ZZE_CountryOrGrouping

IF @@ROWCOUNT = 0 THROW 51000, 'INVALID DATA', 1;

INSERT dbo.RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZE_Attribute)
SELECT newid(), TransportMode, @ZZE_PK
FROM
(
	SELECT TransportMode,
		CASE WHEN TransportMode = 'AIR' AND @ZZE_IsAir = 1 THEN 'Y'
				WHEN TransportMode = 'SEA' AND @ZZE_IsSea = 1 THEN 'Y'
				WHEN TransportMode = 'FIX' AND @ZZE_IsFix = 1 THEN 'Y'
				WHEN TransportMode = 'RAI' AND @ZZE_IsRai = 1 THEN 'Y'
				WHEN TransportMode = 'ROA' AND @ZZE_IsRoa = 1 THEN 'Y'
				WHEN TransportMode = 'MAI' AND @ZZE_IsMai = 1 THEN 'Y'
				WHEN TransportMode = 'INW' AND @ZZE_IsInw = 1 THEN 'Y'
		END AS Flag
	FROM
	(SELECT 'ROA' TransportMode UNION SELECT 'SEA' UNION SELECT 'AIR' UNION SELECT 'RAI' UNION SELECT 'MAI' UNION SELECT 'FIX' UNION SELECT 'INW') TransportModeList
) TransportModes
WHERE Flag IS NOT NULL

END

GO

CREATE PROCEDURE [dbo].[RefCusCodeListAttributeUserView_Del]
	@ZZE_PK uniqueidentifier,
	@ZZE_CodeType varchar(5),
	@ZZE_CountryOrGrouping varchar(3)
AS
DELETE dbo.RefCusCodeOrAttributeTransportMode
WHERE ZZU_ZZE_Attribute = @ZZE_PK

DELETE attr
FROM dbo.RefCusCodeListAttribute attr
JOIN dbo.RefCusCodeList ON ZZE_ZZD_CodeList = ZZD_PK
WHERE ZZE_PK = @ZZE_PK AND ZZD_ZZK_NKCodeType = @ZZE_CodeType AND ZZD_ZZZ_NKDataGrouping = @ZZE_CountryOrGrouping

IF @@ROWCOUNT = 0 THROW 51000, 'INVALID DATA', 1;

GO

CREATE PROCEDURE [dbo].[RefCusCodeListAttributeUserView_Ups]
	@ZZE_PK uniqueidentifier,
	@ZZE_Value nvarchar(255),
	@ZZE_ZXE_NKName varchar(32),
	@ZZE_IsAir bit,
	@ZZE_IsSea bit,
	@ZZE_IsFix bit,
	@ZZE_IsRai bit,
	@ZZE_IsRoa bit,
	@ZZE_IsMai bit,
	@ZZE_IsInw bit,
	@ZZE_CodeType varchar(5),
	@ZZE_CountryOrGrouping varchar(3),
	@ZZE_StartDate smalldatetime,
	@ZZE_EndDate smalldatetime
AS
BEGIN

UPDATE attr SET
	ZZE_Value = @ZZE_Value,
	ZZE_ZXE_NKName = @ZZE_ZXE_NKName,
	ZZE_StartDate = @ZZE_StartDate,
	ZZE_EndDate = @ZZE_EndDate
FROM dbo.RefCusCodeListAttribute attr
JOIN dbo.RefCusCodeList ON ZZE_ZZD_CodeList = ZZD_PK
WHERE ZZE_PK = @ZZE_PK AND ZZD_ZZK_NKCodeType = @ZZE_CodeType AND ZZD_ZZZ_NKDataGrouping = @ZZE_CountryOrGrouping

IF @@ROWCOUNT = 0 THROW 51000, 'INVALID DATA', 1;

INSERT dbo.RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZE_Attribute)
SELECT newid(), TransportMode, @ZZE_PK
FROM
(
	SELECT TransportMode, 
			CASE WHEN TransportMode = 'AIR' AND @ZZE_IsAir = 1 THEN 'Y'
						WHEN TransportMode = 'SEA' AND @ZZE_IsSea = 1 THEN 'Y'
						WHEN TransportMode = 'FIX' AND @ZZE_IsFix = 1 THEN 'Y'
						WHEN TransportMode = 'RAI' AND @ZZE_IsRai = 1 THEN 'Y'
						WHEN TransportMode = 'ROA' AND @ZZE_IsRoa = 1 THEN 'Y'
						WHEN TransportMode = 'MAI' AND @ZZE_IsMai = 1 THEN 'Y'
						WHEN TransportMode = 'INW' AND @ZZE_IsInw = 1 THEN 'Y'
		ELSE 'N' END AS Flag FROM
		(SELECT 'ROA' TransportMode UNION SELECT 'SEA' UNION SELECT 'AIR' UNION SELECT 'RAI' UNION SELECT 'MAI' UNION SELECT 'FIX' UNION SELECT 'INW') TransportModeList
) S
LEFT JOIN RefCusCodeOrAttributeTransportMode AS T ON T.ZZU_ZZE_Attribute = @ZZE_PK AND T.ZZU_TransportMode = S.TransportMode
WHERE ZZU_PK IS NULL AND S.Flag = 'Y'

DELETE trans
FROM
(
	SELECT TransportMode, 
			CASE WHEN TransportMode = 'AIR' AND @ZZE_IsAir = 1 THEN 'Y'
						WHEN TransportMode = 'SEA' AND @ZZE_IsSea = 1 THEN 'Y'
						WHEN TransportMode = 'FIX' AND @ZZE_IsFix = 1 THEN 'Y'
						WHEN TransportMode = 'RAI' AND @ZZE_IsRai = 1 THEN 'Y'
						WHEN TransportMode = 'ROA' AND @ZZE_IsRoa = 1 THEN 'Y'
						WHEN TransportMode = 'MAI' AND @ZZE_IsMai = 1 THEN 'Y'
						WHEN TransportMode = 'INW' AND @ZZE_IsInw = 1 THEN 'Y'
		ELSE 'N' END AS Flag FROM
		(SELECT 'ROA' TransportMode UNION SELECT 'SEA' UNION SELECT 'AIR' UNION SELECT 'RAI' UNION SELECT 'MAI' UNION SELECT 'FIX' UNION SELECT 'INW') TransportModeList
) S
JOIN RefCusCodeOrAttributeTransportMode trans ON ZZU_ZZE_Attribute = @ZZE_PK AND ZZU_TransportMode = S.TransportMode
WHERE S.Flag = 'N'

END

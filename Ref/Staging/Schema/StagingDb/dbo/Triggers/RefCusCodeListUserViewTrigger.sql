CREATE TRIGGER RefCusCodeListUserView_Version_Create ON RefCusCodeListUserView
INSTEAD OF INSERT
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
	SELECT ZZD_PK, ZZD_CodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_CountryOrGrouping
	FROM inserted
	
	INSERT dbo.DataProcessingInformation (DPI_ID, DPI_ParentPk, DPI_ParentTableCode, DPI_Status, DPI_SourceId, DPI_Message)
	SELECT newid(), ZZD_PK, 'ZZD', 'QUE', @sourceId, CASE WHEN ZZD_IsPublished = 0 THEN 'Delete' ELSE '' END
	FROM inserted
	
	INSERT dbo.RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)
	SELECT newid(), TransportMode, i.ZZD_PK
	FROM inserted i
	JOIN
	(
		SELECT TransportMode,
			CASE WHEN TransportMode = 'AIR' AND i.ZZD_IsAir = 1 THEN 'Y'
					WHEN TransportMode = 'SEA' AND i.ZZD_IsSea = 1 THEN 'Y'
					WHEN TransportMode = 'FIX' AND i.ZZD_IsFix = 1 THEN 'Y'
					WHEN TransportMode = 'RAI' AND i.ZZD_IsRai = 1 THEN 'Y'
					WHEN TransportMode = 'ROA' AND i.ZZD_IsRoa = 1 THEN 'Y'
					WHEN TransportMode = 'MAI' AND i.ZZD_IsMai = 1 THEN 'Y'
					WHEN TransportMode = 'INW' AND i.ZZD_IsInw = 1 THEN 'Y'
			END AS Flag FROM inserted i JOIN 
		(SELECT 'ROA' TransportMode UNION SELECT 'SEA' UNION SELECT 'AIR' UNION SELECT 'RAI' UNION SELECT 'MAI' UNION SELECT 'FIX' UNION SELECT 'INW') TransportModeList
		ON 1=1
	) TransportModes ON 1=1
	WHERE Flag IS NOT NULL
END
GO

CREATE TRIGGER RefCusCodeListUserView_Version_Update ON RefCusCodeListUserView
INSTEAD OF UPDATE
AS
BEGIN
	UPDATE r SET
	r.ZZD_ZZZ_NKDataGrouping = i.ZZD_CountryOrGrouping,
	r.ZZD_ZZK_NKCodeType = i.ZZD_CodeType,
	r.ZZD_Code = i.ZZD_Code,
	r.ZZD_Description = i.ZZD_Description,
	r.ZZD_StartDate = i.ZZD_StartDate,
	r.ZZD_EndDate = i.ZZD_EndDate
	FROM RefCusCodeList r
	JOIN inserted i ON r.ZZD_PK = i.ZZD_PK
	
	UPDATE d SET d.DPI_Message =  CASE WHEN ZZD_IsPublished = 0 THEN 'Delete' ELSE '' END
	FROM DataProcessingInformation d
	JOIN inserted i ON d.DPI_ParentPk = i.ZZD_PK
	WHERE DPI_ParentTableCode = 'ZZD'
	
	MERGE dbo.RefCusCodeOrAttributeTransportMode AS T
	USING 
	(
		SELECT TransportMode, i.ZZD_PK,
				CASE WHEN TransportMode = 'AIR' AND i.ZZD_IsAir = 1 THEN 'Y'
							WHEN TransportMode = 'SEA' AND i.ZZD_IsSea = 1 THEN 'Y'
							WHEN TransportMode = 'FIX' AND i.ZZD_IsFix = 1 THEN 'Y'
							WHEN TransportMode = 'RAI' AND i.ZZD_IsRai = 1 THEN 'Y'
							WHEN TransportMode = 'ROA' AND i.ZZD_IsRoa = 1 THEN 'Y'
							WHEN TransportMode = 'MAI' AND i.ZZD_IsMai = 1 THEN 'Y'
							WHEN TransportMode = 'INW' AND i.ZZD_IsInw = 1 THEN 'Y'
			ELSE 'N' END AS Flag FROM inserted i JOIN 
			(SELECT 'ROA' TransportMode UNION SELECT 'SEA' UNION SELECT 'AIR' UNION SELECT 'RAI' UNION SELECT 'MAI' UNION SELECT 'FIX' UNION SELECT 'INW') TransportModeList
			ON 1=1
	) S
	ON T.ZZU_ZZD_CodeList = ZZD_PK AND T.ZZU_TransportMode = S.TransportMode
	WHEN NOT MATCHED AND S.Flag = 'Y'
		THEN INSERT (ZZU_PK, ZZU_ZZD_CodeList, ZZU_TransportMode)
			VALUES (newid(), ZZD_PK, S.TransportMode)
	WHEN MATCHED AND S.Flag = 'N'
		THEN DELETE;
END
GO

CREATE TRIGGER RefCusCodeListUserView_Version_Delete ON RefCusCodeListUserView
INSTEAD OF DELETE
AS
BEGIN
	DELETE r
	FROM deleted d
	JOIN RefCusCodeOrAttributeTransportMode r on r.ZZU_ZZD_CodeList = d.ZZD_PK
	DELETE r
	FROM deleted d
	JOIN RefCusCodeList r on r.ZZD_PK = d.ZZD_PK
	DELETE r
	FROM deleted d
	JOIN DataProcessingInformation r on r.DPI_ParentPk = d.ZZD_PK
	WHERE DPI_ParentTableCode = 'ZZD'
END
GO

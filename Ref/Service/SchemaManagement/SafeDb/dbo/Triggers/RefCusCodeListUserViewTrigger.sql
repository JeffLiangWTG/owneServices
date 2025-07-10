CREATE TRIGGER RefCusCodeListUserView_Version_Create ON RefCusCodeListUserView
INSTEAD OF INSERT
AS
BEGIN
	INSERT dbo.RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping)
	SELECT ZZD_PK, ZZD_CodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_CountryOrGrouping
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
			END AS Flag
		FROM inserted i
		JOIN (SELECT 'ROA' TransportMode UNION SELECT 'SEA' UNION SELECT 'AIR' UNION SELECT 'RAI' UNION SELECT 'MAI' UNION SELECT 'FIX' UNION SELECT 'INW') TransportModeList ON 1=1
	) TransportModes ON 1=1
	WHERE Flag IS NOT NULL

	UPDATE r
	SET RVC_Deleted = ~i.ZZD_IsPublished, r.RVC_LastEditedUser = dbo.GetUserId()
	FROM RefDbVersionControl r
	JOIN inserted i ON r.RVC_ParentPK = i.ZZD_PK AND RVC_ParentCode = 'ZZD'
END
GO

CREATE TRIGGER RefCusCodeListUserView_Version_Update ON RefCusCodeListUserView
INSTEAD OF UPDATE
AS
BEGIN
	UPDATE r SET
		ZZD_ZZZ_NKDataGrouping = i.ZZD_CountryOrGrouping,
		ZZD_ZZK_NKCodeType = i.ZZD_CodeType,
		ZZD_Code = i.ZZD_Code,
		ZZD_Description = i.ZZD_Description,
		ZZD_StartDate = i.ZZD_StartDate,
		ZZD_EndDate = i.ZZD_EndDate
	FROM inserted i
	JOIN RefCusCodeList r on r.ZZD_PK = i.ZZD_PK

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
			END AS Flag
		FROM inserted i
		JOIN (SELECT 'ROA' TransportMode UNION SELECT 'SEA' UNION SELECT 'AIR' UNION SELECT 'RAI' UNION SELECT 'MAI' UNION SELECT 'FIX' UNION SELECT 'INW') TransportModeList ON 1=1
		-- WI00856814 LEFT JOIN with WHERE to skip the records that have already existed in RefCusCodeOrAttributeTransportMode
		LEFT JOIN dbo.RefCusCodeOrAttributeTransportMode ZZU
		ON TransportModeList.TransportMode = ZZU.ZZU_TransportMode AND i.ZZD_PK = ZZU.ZZU_ZZD_CodeList
		WHERE ZZU.ZZU_TransportMode IS NULL
	) TransportModes ON 1=1
	WHERE Flag IS NOT NULL

	UPDATE r
	SET RVC_Deleted = ~i.ZZD_IsPublished, r.RVC_LastEditedUser = dbo.GetUserId()
	FROM RefDbVersionControl r
	JOIN inserted i ON r.RVC_ParentPK = i.ZZD_PK AND RVC_ParentCode = 'ZZD'

	DELETE trans
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
			ELSE 'N' END AS Flag FROM inserted i
			JOIN (SELECT 'ROA' TransportMode UNION SELECT 'SEA' UNION SELECT 'AIR' UNION SELECT 'RAI' UNION SELECT 'MAI' UNION SELECT 'FIX' UNION SELECT 'INW') TransportModeList ON 1=1
	) S ON 1=1
	JOIN RefCusCodeOrAttributeTransportMode trans ON ZZU_ZZD_CodeList = i.ZZD_PK AND ZZU_TransportMode = S.TransportMode
	WHERE S.Flag = 'N'
END
GO

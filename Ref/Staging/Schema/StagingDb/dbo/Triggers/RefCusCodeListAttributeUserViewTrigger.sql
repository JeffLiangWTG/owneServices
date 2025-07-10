CREATE TRIGGER RefCusCodeListAttributeUserView_Version_Create ON RefCusCodeListAttributeUserView
INSTEAD OF INSERT
AS
BEGIN
	INSERT dbo.RefCusCodeListAttribute (ZZE_PK, ZZE_Value, ZZE_ZXE_NKName, ZZE_ZZD_CodeList)
	SELECT ZZE_PK, ZZE_Value, ZZE_ZXE_NKName, ZZE_ZZD_CodeList
	FROM inserted i

	INSERT dbo.RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZE_Attribute)
	SELECT newid(), TransportMode, i.ZZE_PK
	FROM inserted i
	JOIN
	(
		SELECT TransportMode,
			CASE WHEN TransportMode = 'AIR' AND i.ZZE_IsAir = 1 THEN 'Y'
					WHEN TransportMode = 'SEA' AND i.ZZE_IsSea = 1 THEN 'Y'
					WHEN TransportMode = 'FIX' AND i.ZZE_IsFix = 1 THEN 'Y'
					WHEN TransportMode = 'RAI' AND i.ZZE_IsRai = 1 THEN 'Y'
					WHEN TransportMode = 'ROA' AND i.ZZE_IsRoa = 1 THEN 'Y'
					WHEN TransportMode = 'MAI' AND i.ZZE_IsMai = 1 THEN 'Y'
					WHEN TransportMode = 'INW' AND i.ZZE_IsInw = 1 THEN 'Y'
			END AS Flag FROM inserted i JOIN 
		(SELECT 'ROA' TransportMode UNION SELECT 'SEA' UNION SELECT 'AIR' UNION SELECT 'RAI' UNION SELECT 'MAI' UNION SELECT 'FIX' UNION SELECT 'INW') TransportModeList
		ON 1=1
	) TransportModes ON 1=1
	WHERE Flag IS NOT NULL
END
GO

CREATE TRIGGER RefCusCodeListAttributeUserView_Version_Update ON RefCusCodeListAttributeUserView
INSTEAD OF UPDATE
AS
BEGIN
	UPDATE r SET
	ZZE_ZZD_CodeList = i.ZZE_ZZD_CodeList,
	ZZE_Value = i.ZZE_Value,
	ZZE_ZXE_NKName = i.ZZE_ZXE_NKName
	FROM RefCusCodeListAttribute r
	JOIN inserted i ON r.ZZE_PK = i.ZZE_PK
	JOIN dbo.RefCusCodeList ON r.ZZE_ZZD_CodeList = ZZD_PK
	
	MERGE dbo.RefCusCodeOrAttributeTransportMode AS T
	USING 
	(
		SELECT TransportMode, i.ZZE_PK,
				CASE WHEN TransportMode = 'AIR' AND i.ZZE_IsAir = 1 THEN 'Y'
							WHEN TransportMode = 'SEA' AND i.ZZE_IsSea = 1 THEN 'Y'
							WHEN TransportMode = 'FIX' AND i.ZZE_IsFix = 1 THEN 'Y'
							WHEN TransportMode = 'RAI' AND i.ZZE_IsRai = 1 THEN 'Y'
							WHEN TransportMode = 'ROA' AND i.ZZE_IsRoa = 1 THEN 'Y'
							WHEN TransportMode = 'MAI' AND i.ZZE_IsMai = 1 THEN 'Y'
							WHEN TransportMode = 'INW' AND i.ZZE_IsInw = 1 THEN 'Y'
			ELSE 'N' END AS Flag FROM inserted i JOIN 
			(SELECT 'ROA' TransportMode UNION SELECT 'SEA' UNION SELECT 'AIR' UNION SELECT 'RAI' UNION SELECT 'MAI' UNION SELECT 'FIX' UNION SELECT 'INW') TransportModeList
			ON 1=1
	) S
	ON T.ZZU_ZZE_Attribute = ZZE_PK AND T.ZZU_TransportMode = S.TransportMode
	WHEN NOT MATCHED AND S.Flag = 'Y'
		THEN INSERT (ZZU_PK, ZZU_ZZE_Attribute, ZZU_TransportMode)
			VALUES (newid(), ZZE_PK, S.TransportMode)
	WHEN MATCHED AND S.Flag = 'N'
		THEN DELETE;
END
GO
CREATE TRIGGER RefCusCodeListAttributeUserView_Version_Delete ON RefCusCodeListAttributeUserView
INSTEAD OF DELETE
AS
BEGIN
	DELETE r
	FROM deleted d
	JOIN RefCusCodeOrAttributeTransportMode r on r.ZZU_ZZE_Attribute = d.ZZE_PK
	DELETE r
	FROM deleted d
	JOIN RefCusCodeListAttribute r on r.ZZE_PK = d.ZZE_PK
END
GO

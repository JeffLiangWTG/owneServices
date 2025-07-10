CREATE TRIGGER RefCusCodeListAttributeUserView_Version_Create ON RefCusCodeListAttributeUserView
INSTEAD OF INSERT
AS
BEGIN
	INSERT dbo.RefCusCodeListAttribute (ZZE_PK, ZZE_Value, ZZE_ZXE_NKName, ZZE_ZZD_CodeList)
	SELECT ZZE_PK, ZZE_Value, ZZE_ZXE_NKName, ZZE_ZZD_CodeList
	FROM inserted i
	JOIN dbo.RefCusCodeList ON ZZD_PK = i.ZZE_ZZD_CodeList AND ZZD_ZZK_NKCodeType = i.ZZE_CodeType AND ZZD_ZZZ_NKDataGrouping = i.ZZE_CountryOrGrouping

	IF @@ROWCOUNT = 0 THROW 51000, 'INVALID DATA', 1;

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
			END AS Flag
		FROM inserted i
		JOIN (SELECT 'ROA' TransportMode UNION SELECT 'SEA' UNION SELECT 'AIR' UNION SELECT 'RAI' UNION SELECT 'MAI' UNION SELECT 'FIX' UNION SELECT 'INW') TransportModeList ON 1=1
	) TransportModes ON 1=1
	WHERE Flag IS NOT NULL
END
GO

CREATE TRIGGER RefCusCodeListAttributeUserView_Version_Update ON RefCusCodeListAttributeUserView
INSTEAD OF UPDATE
AS
BEGIN
	UPDATE attr SET
		ZZE_Value = i.ZZE_Value,
		ZZE_ZXE_NKName = i.ZZE_ZXE_NKName
	FROM inserted i
	JOIN dbo.RefCusCodeListAttribute attr ON i.ZZE_PK = attr.ZZE_PK
	JOIN dbo.RefCusCodeList ON attr.ZZE_ZZD_CodeList = ZZD_PK
	WHERE ZZD_ZZK_NKCodeType = i.ZZE_CodeType AND ZZD_ZZZ_NKDataGrouping = i.ZZE_CountryOrGrouping

	IF @@ROWCOUNT = 0 THROW 51000, 'INVALID DATA', 1;

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
			ELSE 'N' END AS Flag FROM inserted i
			JOIN (SELECT 'ROA' TransportMode UNION SELECT 'SEA' UNION SELECT 'AIR' UNION SELECT 'RAI' UNION SELECT 'MAI' UNION SELECT 'FIX' UNION SELECT 'INW') TransportModeList ON 1=1
	) S ON 1=1
	LEFT JOIN RefCusCodeOrAttributeTransportMode AS T ON T.ZZU_ZZE_Attribute = i.ZZE_PK AND T.ZZU_TransportMode = S.TransportMode
	WHERE ZZU_PK IS NULL AND S.Flag = 'Y'

	DELETE trans
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
			ELSE 'N' END AS Flag FROM inserted i
			JOIN (SELECT 'ROA' TransportMode UNION SELECT 'SEA' UNION SELECT 'AIR' UNION SELECT 'RAI' UNION SELECT 'MAI' UNION SELECT 'FIX' UNION SELECT 'INW') TransportModeList ON 1=1
	) S ON 1=1
	JOIN RefCusCodeOrAttributeTransportMode trans ON ZZU_ZZE_Attribute = i.ZZE_PK AND ZZU_TransportMode = S.TransportMode
	WHERE S.Flag = 'N'
END
GO
CREATE TRIGGER RefCusCodeListAttributeUserView_Version_Delete ON RefCusCodeListAttributeUserView
INSTEAD OF DELETE
AS
BEGIN
	DELETE r
	FROM deleted d
	JOIN dbo.RefCusCodeOrAttributeTransportMode r ON r.ZZU_ZZE_Attribute = d.ZZE_PK

	DELETE attr
	FROM deleted d
	JOIN dbo.RefCusCodeListAttribute attr ON attr.ZZE_PK = d.ZZE_PK
	JOIN dbo.RefCusCodeList ON attr.ZZE_ZZD_CodeList = ZZD_PK
	WHERE ZZD_ZZK_NKCodeType = d.ZZE_CodeType AND ZZD_ZZZ_NKDataGrouping = d.ZZE_CountryOrGrouping

	IF @@ROWCOUNT = 0 THROW 51000, 'INVALID DATA', 1;
END
GO

CREATE TRIGGER TG_RefDbVersionControl_UPD_ZXE ON RefDbVersionControl
FOR UPDATE
AS
BEGIN
	IF UPDATE(RVC_Deleted)
	BEGIN
		IF EXISTS
		(
			SELECT 1 FROM inserted WHERE RVC_ParentCode = 'ZXE' AND RVC_Deleted = 1
		)
		BEGIN
			IF EXISTS
			(
				SELECT top 1 1
				FROM inserted c
					INNER JOIN RefCusCodeListAttributeName n on c.RVC_ParentPK = n.ZXE_PK
					INNER JOIN RefCusCodeList l ON n.ZXE_ZZZ_NKDataGrouping = l.ZZD_ZZZ_NKDataGrouping AND n.ZXE_ZZK_NKCodeType = l.ZZD_ZZK_NKCodeType
					INNER JOIN RefCusCodeListAttribute i ON n.ZXE_Name = i.ZZE_ZXE_NKName AND i.ZZE_ZZD_CodeList = l.ZZD_PK
					INNER JOIN RefDbVersionControl v ON v.RVC_ParentPK = l.ZZD_PK AND v.RVC_Deleted = 0
				WHERE c.RVC_ParentCode = 'ZXE' AND c.RVC_Deleted = 1
			)
			THROW 508112024, 'A RefCusCodeListAttributeName was not deleted due to existing RefCusCodeListAttribute.', 1;
		END
	END
END
GO

CREATE TRIGGER TG_RefDbVersionControl_UPD_ZZK ON RefDbVersionControl
FOR UPDATE
AS
BEGIN
	IF UPDATE(RVC_Deleted)
	BEGIN
		IF EXISTS
		(
			SELECT 1 FROM inserted WHERE RVC_ParentCode = 'ZZK' AND RVC_Deleted = 1
		)
		BEGIN
			IF EXISTS
			(
				SELECT top 1 1
				FROM inserted c
					INNER JOIN RefCusCodeType t on t.ZZK_PK = c.RVC_ParentPK
					INNER JOIN RefCusCodeList l ON t.ZZK_ZZZ_NKDataGrouping = l.ZZD_ZZZ_NKDataGrouping AND t.ZZK_CodeType = l.ZZD_ZZK_NKCodeType
					INNER JOIN RefDbVersionControl v ON v.RVC_ParentPK = l.ZZD_PK AND v.RVC_Deleted = 0
				WHERE c.RVC_ParentCode = 'ZZK' AND c.RVC_Deleted = 1
			)
			THROW 509112024, 'A RefCusCodeType was not deleted due to existing RefCusCodeList.', 1;

			IF EXISTS
			(
				SELECT top 1 1
				FROM inserted c
					INNER JOIN RefCusCodeType t on t.ZZK_PK = c.RVC_ParentPK
					INNER JOIN RefCusCodeListAttributeName l ON t.ZZK_ZZZ_NKDataGrouping = l.ZXE_ZZZ_NKDataGrouping AND (t.ZZK_CodeType = l.ZXE_ZZK_NKCodeType OR t.ZZK_CodeType = l.ZXE_ZZK_NKCodeTypeForValueList)
					INNER JOIN RefDbVersionControl v ON v.RVC_ParentPK = l.ZXE_PK AND v.RVC_Deleted = 0
				WHERE c.RVC_ParentCode = 'ZZK' AND c.RVC_Deleted = 1
			)
			THROW 510112024, 'A RefCusCodeType was not deleted due to existing RefCusCodeListAttributeName.', 1;
		END
	END
END
GO

CREATE TRIGGER RefCusProcedureAttributeUserView_Version_Create ON RefCusProcedureAttributeUserView
INSTEAD OF INSERT
AS
BEGIN
	INSERT dbo.RefCusProcedureAttribute (ZXB_PK, ZXB_ZZ6_ProcedureCode, ZXB_Name, ZXB_Value)
	SELECT ZXB_PK, ZXB_ZZ6_ProcedureCode, ZXB_Name, ZXB_Value
	FROM inserted
	JOIN dbo.RefCusProcedure ON ZZ6_PK = ZXB_ZZ6_ProcedureCode AND ZZ6_ZZZ_NKDataGrouping = ZXB_CountryOrGrouping

	IF @@ROWCOUNT = 0 THROW 51000, 'INVALID DATA', 1;
END
GO

CREATE TRIGGER RefCusProcedureAttributeUserView_Version_Update ON RefCusProcedureAttributeUserView
INSTEAD OF UPDATE
AS
BEGIN
	UPDATE attr SET
		ZXB_Name = i.ZXB_Name,
		ZXB_Value = i.ZXB_Value
	FROM inserted i
	JOIN dbo.RefCusProcedureAttribute attr ON attr.ZXB_PK = i.ZXB_PK
	JOIN dbo.RefCusProcedure ON attr.ZXB_ZZ6_ProcedureCode = ZZ6_PK
	WHERE ZZ6_ZZZ_NKDataGrouping = i.ZXB_CountryOrGrouping

	IF @@ROWCOUNT = 0 THROW 51000, 'INVALID DATA', 1;
END
GO
CREATE TRIGGER RefCusProcedureAttributeUserView_Version_Delete ON RefCusProcedureAttributeUserView
INSTEAD OF DELETE
AS
BEGIN
	DELETE attr
	FROM deleted d
	JOIN dbo.RefCusProcedureAttribute attr ON d.ZXB_PK = attr.ZXB_PK
	JOIN dbo.RefCusProcedure ON attr.ZXB_ZZ6_ProcedureCode = ZZ6_PK
	WHERE ZZ6_ZZZ_NKDataGrouping = d.ZXB_CountryOrGrouping

	IF @@ROWCOUNT = 0 THROW 51000, 'INVALID DATA', 1;
END
GO

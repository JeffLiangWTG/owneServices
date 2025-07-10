CREATE TRIGGER RefPortPolygonUserView_Version_Create ON RefPortPolygonUserView
INSTEAD OF INSERT
AS
BEGIN
	INSERT dbo.RefPortPolygon (RPP_PK, RPP_PortId, RPP_SerializedPolygon)
	SELECT RPP_PK, RPP_PortId, RPP_SerializedPolygon
	FROM inserted

	UPDATE r SET RVC_Deleted = ~RPP_IsPublished
	FROM RefDbVersionControl r
	JOIN inserted ON RVC_ParentPK = RPP_PK AND RVC_ParentCode = 'RPP'
END
GO

CREATE TRIGGER RefPortPolygonUserView_Version_Update ON RefPortPolygonUserView
INSTEAD OF UPDATE
AS
BEGIN
	UPDATE r SET
		RPP_PortId = i.RPP_PortId,
		RPP_SerializedPolygon = i.RPP_SerializedPolygon
	FROM RefPortPolygon r
	JOIN inserted i ON r.RPP_PK = i.RPP_PK

	UPDATE r
	SET RVC_Deleted = ~RPP_IsPublished
	FROM RefDbVersionControl r
	JOIN inserted ON RVC_ParentPK = RPP_PK and RVC_ParentCode = 'RPP'
END
GO

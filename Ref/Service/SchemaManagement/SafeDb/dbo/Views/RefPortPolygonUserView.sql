CREATE VIEW [dbo].[RefPortPolygonUserView]
WITH SCHEMABINDING
AS

SELECT
	RPP_PK AS RPP_PK,
	RPP_PortId AS RPP_PortId,
	RPP_SerializedPolygon AS RPP_SerializedPolygon,
	RPP_SysStartTime AS RPP_SysStartTime,
	RPP_SysEndTime AS RPP_SysEndTime,
	~RVC_Deleted AS RPP_IsPublished
	
FROM [dbo].RefPortPolygon
JOIN [dbo].RefDbVersionControl ON RVC_ParentPK = RPP_PK AND RVC_ParentCode = 'RPP'
GO

CREATE PROCEDURE [dbo].[RefPortPolygonUserView_Ins]
	@RPP_PK UNIQUEIDENTIFIER,
	@RPP_PortId INT,
	@RPP_SerializedPolygon geography,
	@RPP_IsPublished BIT
AS
BEGIN

INSERT dbo.RefPortPolygon (RPP_PK, RPP_PortId, RPP_SerializedPolygon)
VALUES (@RPP_PK, @RPP_PortId, @RPP_SerializedPolygon)

UPDATE RefDbVersionControl SET RVC_Deleted = ~@RPP_IsPublished WHERE RVC_ParentPK = @RPP_PK AND RVC_ParentCode = 'RPP'
END
GO

CREATE PROCEDURE [dbo].[RefPortPolygonUserView_Del]
	@RPP_PK UNIQUEIDENTIFIER
AS
DELETE RefPortPolygon WHERE RPP_PK = @RPP_PK
GO

CREATE PROCEDURE [dbo].[RefPortPolygonUserView_Ups]
	@RPP_PK UNIQUEIDENTIFIER,
	@RPP_PortId INT,
	@RPP_SerializedPolygon geography,
	@RPP_IsPublished BIT
AS
BEGIN

UPDATE RefPortPolygon SET
	RPP_PortId = @RPP_PortId,
	RPP_SerializedPolygon = @RPP_SerializedPolygon
WHERE RPP_PK = @RPP_PK

UPDATE RefDbVersionControl SET RVC_Deleted = ~@RPP_IsPublished WHERE RVC_ParentPK = @RPP_PK and RVC_ParentCode = 'RPP'
END

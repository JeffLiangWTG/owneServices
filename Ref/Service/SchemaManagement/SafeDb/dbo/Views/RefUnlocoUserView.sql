CREATE VIEW [dbo].[RefUNLOCOUserView]
WITH SCHEMABINDING
AS

SELECT
	RL_PK AS RL_PK,
	RL_Code AS RL_Code,
	RL_IsActive AS RL_IsActive,
	RL_PortName AS RL_PortName,
	RL_NameWithDiacriticals AS RL_NameWithDiacriticals,
	RL_IATA AS RL_IATA,
	RL_CoOrdinates AS RL_CoOrdinates,
	RL_HasAirport AS RL_HasAirport,
	RL_HasSeaport AS RL_HasSeaport,
	RL_HasRail AS RL_HasRail,
	RL_HasRoad AS RL_HasRoad,
	RL_HasPost AS RL_HasPost,
	RL_HasCustomsLodge AS RL_HasCustomsLodge,
	RL_HasUnload AS RL_HasUnload,
	RL_HasStore AS RL_HasStore,
	RL_HasTerminal AS RL_HasTerminal,
	RL_HasDischarge AS RL_HasDischarge,
	RL_HasOutport AS RL_HasOutport,
	RL_HasBorderCrossing AS RL_HasBorderCrossing,
	RL_R3 AS RL_R3,
	RL_RN_NKCountryCode AS RL_RN_NKCountryCode,
	RL_RW AS RL_RW,
	RL_IATARegionCode AS RL_IATARegionCode,
	RL_GeoLocation AS RL_GeoLocation,
	RL_UserOverride AS RL_UserOverride,
	RL_SysStartTime AS RL_SysStartTime,
	RL_SysEndTime AS RL_SysEndTime,
	~RVC_Deleted AS RL_IsPublished
	
FROM [dbo].RefUNLOCO
JOIN [dbo].RefDbVersionControl ON RVC_ParentPK = RL_PK AND RVC_ParentCode = 'RL'
GO

CREATE PROCEDURE [dbo].[RefUNLOCOUserView_Ins]
	@RL_PK UNIQUEIDENTIFIER,
	@RL_Code VARCHAR(5),
	@RL_IsActive BIT,
	@RL_PortName VARCHAR(35),
	@RL_NameWithDiacriticals NVARCHAR(35),
	@RL_IATA VARCHAR(3),
	@RL_CoOrdinates VARCHAR(12),
	@RL_HasAirport BIT,
	@RL_HasSeaport BIT,
	@RL_HasRail BIT,
	@RL_HasRoad BIT,
	@RL_HasPost BIT,
	@RL_HasCustomsLodge BIT,
	@RL_HasUnload BIT,
	@RL_HasStore BIT,
	@RL_HasTerminal BIT,
	@RL_HasDischarge BIT,
	@RL_HasOutport BIT,
	@RL_HasBorderCrossing BIT,
	@RL_R3 UNIQUEIDENTIFIER,
	@RL_RN_NKCountryCode VARCHAR(2),
	@RL_RW UNIQUEIDENTIFIER,
	@RL_IATARegionCode VARCHAR(3),
	@RL_GeoLocation geography,
	@RL_UserOverride BIT,
	@RL_IsPublished BIT
AS
BEGIN

INSERT dbo.RefUNLOCO (RL_PK, RL_Code, RL_IsActive, RL_PortName, RL_NameWithDiacriticals, RL_IATA, RL_CoOrdinates, RL_HasAirport, RL_HasSeaport, RL_HasRail, RL_HasRoad, RL_HasPost, RL_HasCustomsLodge, RL_HasUnload, RL_HasStore, RL_HasTerminal, RL_HasDischarge, RL_HasOutport, RL_HasBorderCrossing, RL_R3, RL_RN_NKCountryCode, RL_RW, RL_IATARegionCode, RL_GeoLocation, RL_UserOverride)
VALUES (@RL_PK, @RL_Code, @RL_IsActive, @RL_PortName, @RL_NameWithDiacriticals, @RL_IATA, @RL_CoOrdinates, @RL_HasAirport, @RL_HasSeaport, @RL_HasRail, @RL_HasRoad, @RL_HasPost, @RL_HasCustomsLodge, @RL_HasUnload, @RL_HasStore, @RL_HasTerminal, @RL_HasDischarge, @RL_HasOutport, @RL_HasBorderCrossing, @RL_R3, @RL_RN_NKCountryCode, @RL_RW, @RL_IATARegionCode, @RL_GeoLocation, @RL_UserOverride)

UPDATE RefDbVersionControl SET RVC_Deleted = ~@RL_IsPublished WHERE RVC_ParentPK = @RL_PK AND RVC_ParentCode = 'RL'
END
GO

CREATE PROCEDURE [dbo].[RefUNLOCOUserView_Del]
	@RL_PK UNIQUEIDENTIFIER
AS
DELETE RefUNLOCO WHERE RL_PK = @RL_PK
GO

CREATE PROCEDURE [dbo].[RefUNLOCOUserView_Ups]
	@RL_PK UNIQUEIDENTIFIER,
	@RL_Code VARCHAR(5),
	@RL_IsActive BIT,
	@RL_PortName VARCHAR(35),
	@RL_NameWithDiacriticals NVARCHAR(35),
	@RL_IATA VARCHAR(3),
	@RL_CoOrdinates VARCHAR(12),
	@RL_HasAirport BIT,
	@RL_HasSeaport BIT,
	@RL_HasRail BIT,
	@RL_HasRoad BIT,
	@RL_HasPost BIT,
	@RL_HasCustomsLodge BIT,
	@RL_HasUnload BIT,
	@RL_HasStore BIT,
	@RL_HasTerminal BIT,
	@RL_HasDischarge BIT,
	@RL_HasOutport BIT,
	@RL_HasBorderCrossing BIT,
	@RL_R3 UNIQUEIDENTIFIER,
	@RL_RN_NKCountryCode VARCHAR(2),
	@RL_RW UNIQUEIDENTIFIER,
	@RL_IATARegionCode VARCHAR(3),
	@RL_GeoLocation geography,
	@RL_UserOverride BIT,
	@RL_IsPublished BIT
AS
BEGIN

UPDATE RefUNLOCO SET
	RL_Code = @RL_Code,
	RL_IsActive = @RL_IsActive,
	RL_PortName = @RL_PortName,
	RL_NameWithDiacriticals = @RL_NameWithDiacriticals,
	RL_IATA = @RL_IATA,
	RL_CoOrdinates = @RL_CoOrdinates,
	RL_HasAirport = @RL_HasAirport,
	RL_HasSeaport = @RL_HasSeaport,
	RL_HasRail = @RL_HasRail,
	RL_HasRoad = @RL_HasRoad,
	RL_HasPost = @RL_HasPost,
	RL_HasCustomsLodge = @RL_HasCustomsLodge,
	RL_HasUnload = @RL_HasUnload,
	RL_HasStore = @RL_HasStore,
	RL_HasTerminal = @RL_HasTerminal,
	RL_HasDischarge = @RL_HasDischarge,
	RL_HasOutport = @RL_HasOutport,
	RL_HasBorderCrossing = @RL_HasBorderCrossing,
	RL_R3 = @RL_R3,
	RL_RN_NKCountryCode = @RL_RN_NKCountryCode,
	RL_RW = @RL_RW,
	RL_IATARegionCode = @RL_IATARegionCode,
	RL_GeoLocation = @RL_GeoLocation,
	RL_UserOverride = @RL_UserOverride
WHERE RL_PK = @RL_PK

UPDATE RefDbVersionControl SET RVC_Deleted = ~@RL_IsPublished WHERE RVC_ParentPK = @RL_PK and RVC_ParentCode = 'RL'
END

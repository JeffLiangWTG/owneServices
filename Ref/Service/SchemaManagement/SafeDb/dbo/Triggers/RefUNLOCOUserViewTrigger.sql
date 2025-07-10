CREATE TRIGGER RefUNLOCOUserView_Version_Create ON RefUNLOCOUserView
INSTEAD OF INSERT
AS
BEGIN
	INSERT dbo.RefUNLOCO (RL_PK, RL_Code, RL_IsActive, RL_PortName, RL_NameWithDiacriticals, RL_IATA, RL_CoOrdinates, RL_HasAirport, RL_HasSeaport, RL_HasRail, RL_HasRoad, RL_HasPost, RL_HasCustomsLodge, RL_HasUnload, RL_HasStore, RL_HasTerminal, RL_HasDischarge, RL_HasOutport, RL_HasBorderCrossing, RL_R3, RL_RN_NKCountryCode, RL_RW, RL_IATARegionCode, RL_GeoLocation, RL_UserOverride)
	SELECT RL_PK, RL_Code, RL_IsActive, RL_PortName, RL_NameWithDiacriticals, RL_IATA, RL_CoOrdinates, RL_HasAirport, RL_HasSeaport, RL_HasRail, RL_HasRoad, RL_HasPost, RL_HasCustomsLodge, RL_HasUnload, RL_HasStore, RL_HasTerminal, RL_HasDischarge, RL_HasOutport, RL_HasBorderCrossing, RL_R3, RL_RN_NKCountryCode, RL_RW, RL_IATARegionCode, RL_GeoLocation, RL_UserOverride
	FROM inserted

	UPDATE r SET RVC_Deleted = ~RL_IsPublished
	FROM RefDbVersionControl r
	JOIN inserted ON RVC_ParentPK = RL_PK AND RVC_ParentCode = 'RL'
END
GO

CREATE TRIGGER RefUNLOCOUserView_Version_Update ON RefUNLOCOUserView
INSTEAD OF UPDATE
AS
BEGIN
	UPDATE r SET
		RL_Code = i.RL_Code,
		RL_IsActive = i.RL_IsActive,
		RL_PortName = i.RL_PortName,
		RL_NameWithDiacriticals = i.RL_NameWithDiacriticals,
		RL_IATA = i.RL_IATA,
		RL_CoOrdinates = i.RL_CoOrdinates,
		RL_HasAirport = i.RL_HasAirport,
		RL_HasSeaport = i.RL_HasSeaport,
		RL_HasRail = i.RL_HasRail,
		RL_HasRoad = i.RL_HasRoad,
		RL_HasPost = i.RL_HasPost,
		RL_HasCustomsLodge = i.RL_HasCustomsLodge,
		RL_HasUnload = i.RL_HasUnload,
		RL_HasStore = i.RL_HasStore,
		RL_HasTerminal = i.RL_HasTerminal,
		RL_HasDischarge = i.RL_HasDischarge,
		RL_HasOutport = i.RL_HasOutport,
		RL_HasBorderCrossing = i.RL_HasBorderCrossing,
		RL_R3 = i.RL_R3,
		RL_RN_NKCountryCode = i.RL_RN_NKCountryCode,
		RL_RW = i.RL_RW,
		RL_IATARegionCode = i.RL_IATARegionCode,
		RL_GeoLocation = i.RL_GeoLocation,
		RL_UserOverride = i.RL_UserOverride
	FROM RefUNLOCO r
	JOIN inserted i ON r.RL_PK = i.RL_PK

	UPDATE r
	SET RVC_Deleted = ~RL_IsPublished
	FROM RefDbVersionControl r
	JOIN inserted ON RVC_ParentPK = RL_PK and RVC_ParentCode = 'RL'
END
GO

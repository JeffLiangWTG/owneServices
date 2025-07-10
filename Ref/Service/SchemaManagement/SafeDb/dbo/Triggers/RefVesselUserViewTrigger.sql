CREATE TRIGGER RefVesselUserView_Version_Create ON RefVesselUserView
INSTEAD OF INSERT
AS
BEGIN
	INSERT dbo.RefVessel (RV_PK, RV_Code, RV_IsActive, RV_LloydsNumber, RV_MalaysiaVesselId, RV_RadioCallSign, RV_NetRegisterTon, RV_VesselType, RV_YearOfConstruction, RV_CustomAttrib1, RV_CustomAttrib2, RV_CustomAttrib3, RV_CustomFlag1, RV_CustomDecimal1, RV_CarrierCode, RV_ScreeningStatus, RV_RN_NKCountryOfReg, RV_MaritimeMobileServiceIdentity, RV_StatusCode, RV_StatCode5, RV_IsGearless, RV_Length, RV_Breadth, RV_Draught, RV_Deadweight, RV_GrossTonnage, RV_GrainCapacity, RV_LiquidCapacity, RV_RoroLanesLength, RV_RoroLanesWidth, RV_RoroLanesClearHeight, RV_RoroLanesNumber, RV_RoroRampsNumber, RV_TEU, RV_CarsNumber, RV_ReeferPointsNumber, RV_TanksNumber)
	SELECT RV_PK, RV_Code, RV_IsActive, RV_LloydsNumber, RV_MalaysiaVesselId, RV_RadioCallSign, RV_NetRegisterTon, RV_VesselType, RV_YearOfConstruction, RV_CustomAttrib1, RV_CustomAttrib2, RV_CustomAttrib3, RV_CustomFlag1, RV_CustomDecimal1, RV_CarrierCode, RV_ScreeningStatus, RV_RN_NKCountryOfReg, RV_MaritimeMobileServiceIdentity, RV_StatusCode, RV_StatCode5, RV_IsGearless, RV_Length, RV_Breadth, RV_Draught, RV_Deadweight, RV_GrossTonnage, RV_GrainCapacity, RV_LiquidCapacity, RV_RoroLanesLength, RV_RoroLanesWidth, RV_RoroLanesClearHeight, RV_RoroLanesNumber, RV_RoroRampsNumber, RV_TEU, RV_CarsNumber, RV_ReeferPointsNumber, RV_TanksNumber
	FROM inserted

	UPDATE r SET RVC_Deleted = ~RV_IsPublished
	FROM RefDbVersionControl r
	JOIN inserted ON RVC_ParentPK = RV_PK AND RVC_ParentCode = 'RV'
END
GO

CREATE TRIGGER RefVesselUserView_Version_Update ON RefVesselUserView
INSTEAD OF UPDATE
AS
BEGIN
	UPDATE r SET
		RV_Code = i.RV_Code,
		RV_IsActive = i.RV_IsActive,
		RV_LloydsNumber = i.RV_LloydsNumber,
		RV_MalaysiaVesselId = i.RV_MalaysiaVesselId,
		RV_RadioCallSign = i.RV_RadioCallSign,
		RV_NetRegisterTon = i.RV_NetRegisterTon,
		RV_VesselType = i.RV_VesselType,
		RV_YearOfConstruction = i.RV_YearOfConstruction,
		RV_CustomAttrib1 = i.RV_CustomAttrib1,
		RV_CustomAttrib2 = i.RV_CustomAttrib2,
		RV_CustomAttrib3 = i.RV_CustomAttrib3,
		RV_CustomFlag1 = i.RV_CustomFlag1,
		RV_CustomDecimal1 = i.RV_CustomDecimal1,
		RV_CarrierCode = i.RV_CarrierCode,
		RV_ScreeningStatus = i.RV_ScreeningStatus,
		RV_RN_NKCountryOfReg = i.RV_RN_NKCountryOfReg,
		RV_MaritimeMobileServiceIdentity = i.RV_MaritimeMobileServiceIdentity,
		RV_StatusCode = i.RV_StatusCode,
		RV_StatCode5 = i.RV_StatCode5,
		RV_IsGearless = i.RV_IsGearless,
		RV_Length = i.RV_Length,
		RV_Breadth = i.RV_Breadth,
		RV_Draught = i.RV_Draught,
		RV_Deadweight = i.RV_Deadweight,
		RV_GrossTonnage = i.RV_GrossTonnage,
		RV_GrainCapacity = i.RV_GrainCapacity,
		RV_LiquidCapacity = i.RV_LiquidCapacity,
		RV_RoroLanesLength = i.RV_RoroLanesLength,
		RV_RoroLanesWidth = i.RV_RoroLanesWidth,
		RV_RoroLanesClearHeight = i.RV_RoroLanesClearHeight,
		RV_RoroLanesNumber = i.RV_RoroLanesNumber,
		RV_RoroRampsNumber = i.RV_RoroRampsNumber,
		RV_TEU = i.RV_TEU,
		RV_CarsNumber = i.RV_CarsNumber,
		RV_ReeferPointsNumber = i.RV_ReeferPointsNumber,
		RV_TanksNumber = i.RV_TanksNumber
	FROM RefVessel r
	JOIN inserted i ON r.RV_PK = i.RV_PK

	UPDATE r
	SET RVC_Deleted = ~RV_IsPublished
	FROM RefDbVersionControl r
	JOIN inserted ON RVC_ParentPK = RV_PK and RVC_ParentCode = 'RV'
END
GO

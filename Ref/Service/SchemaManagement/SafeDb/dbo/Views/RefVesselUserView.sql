CREATE VIEW [dbo].[RefVesselUserView]
WITH SCHEMABINDING
AS

SELECT
	RV_PK AS RV_PK,
	RV_Code AS RV_Code,
	RV_IsActive AS RV_IsActive,
	RV_LloydsNumber AS RV_LloydsNumber,
	RV_MalaysiaVesselId AS RV_MalaysiaVesselId,
	RV_RadioCallSign AS RV_RadioCallSign,
	RV_NetRegisterTon AS RV_NetRegisterTon,
	RV_VesselType AS RV_VesselType,
	RV_YearOfConstruction AS RV_YearOfConstruction,
	RV_CustomAttrib1 AS RV_CustomAttrib1,
	RV_CustomAttrib2 AS RV_CustomAttrib2,
	RV_CustomAttrib3 AS RV_CustomAttrib3,
	RV_CustomFlag1 AS RV_CustomFlag1,
	RV_CustomDecimal1 AS RV_CustomDecimal1,
	RV_CarrierCode AS RV_CarrierCode,
	RV_ScreeningStatus AS RV_ScreeningStatus,
	RV_RN_NKCountryOfReg AS RV_RN_NKCountryOfReg,
	RV_MaritimeMobileServiceIdentity AS RV_MaritimeMobileServiceIdentity,
	RV_StatusCode AS RV_StatusCode,
	RV_StatCode5 AS RV_StatCode5,
	RV_IsGearless AS RV_IsGearless,
	RV_Length AS RV_Length,
	RV_Breadth AS RV_Breadth,
	RV_Draught AS RV_Draught,
	RV_Deadweight AS RV_Deadweight,
	RV_GrossTonnage AS RV_GrossTonnage,
	RV_GrainCapacity AS RV_GrainCapacity,
	RV_LiquidCapacity AS RV_LiquidCapacity,
	RV_RoroLanesLength AS RV_RoroLanesLength,
	RV_RoroLanesWidth AS RV_RoroLanesWidth,
	RV_RoroLanesClearHeight AS RV_RoroLanesClearHeight,
	RV_RoroLanesNumber AS RV_RoroLanesNumber,
	RV_RoroRampsNumber AS RV_RoroRampsNumber,
	RV_TEU AS RV_TEU,
	RV_CarsNumber AS RV_CarsNumber,
	RV_ReeferPointsNumber AS RV_ReeferPointsNumber,
	RV_TanksNumber AS RV_TanksNumber,
	RV_SysStartTime AS RV_SysStartTime,
	RV_SysEndTime AS RV_SysEndTime,
	~RVC_Deleted AS RV_IsPublished
	
FROM [dbo].RefVessel
JOIN [dbo].RefDbVersionControl ON RVC_ParentPK = RV_PK AND RVC_ParentCode = 'RV'
GO

CREATE PROCEDURE [dbo].[RefVesselUserView_Ins]
	@RV_PK UNIQUEIDENTIFIER,
	@RV_Code VARCHAR(50),
	@RV_IsActive BIT,
	@RV_LloydsNumber CHAR(7),
	@RV_MalaysiaVesselId VARCHAR(9),
	@RV_RadioCallSign VARCHAR(10),
	@RV_NetRegisterTon INT,
	@RV_VesselType CHAR(3),
	@RV_YearOfConstruction SMALLINT,
	@RV_CustomAttrib1 VARCHAR(10),
	@RV_CustomAttrib2 VARCHAR(10),
	@RV_CustomAttrib3 VARCHAR(10),
	@RV_CustomFlag1 BIT,
	@RV_CustomDecimal1 DECIMAL(9,3),
	@RV_CarrierCode VARCHAR(4),
	@RV_ScreeningStatus CHAR(3),
	@RV_RN_NKCountryOfReg VARCHAR(2),
	@RV_MaritimeMobileServiceIdentity CHAR(9),
	@RV_StatusCode CHAR(3),
	@RV_StatCode5 CHAR(7),
	@RV_IsGearless BIT,
	@RV_Length DECIMAL(7,3),
	@RV_Breadth DECIMAL(7,3),
	@RV_Draught DECIMAL(7,3),
	@RV_Deadweight DECIMAL(10,3),
	@RV_GrossTonnage DECIMAL(10,3),
	@RV_GrainCapacity DECIMAL(10,3),
	@RV_LiquidCapacity DECIMAL(10,3),
	@RV_RoroLanesLength DECIMAL(8,3),
	@RV_RoroLanesWidth DECIMAL(8,3),
	@RV_RoroLanesClearHeight DECIMAL(6,3),
	@RV_RoroLanesNumber SMALLINT,
	@RV_RoroRampsNumber TINYINT,
	@RV_TEU INT,
	@RV_CarsNumber INT,
	@RV_ReeferPointsNumber SMALLINT,
	@RV_TanksNumber SMALLINT,
	@RV_IsPublished BIT
AS
BEGIN

INSERT dbo.RefVessel (RV_PK, RV_Code, RV_IsActive, RV_LloydsNumber, RV_MalaysiaVesselId, RV_RadioCallSign, RV_NetRegisterTon, RV_VesselType, RV_YearOfConstruction, RV_CustomAttrib1, RV_CustomAttrib2, RV_CustomAttrib3, RV_CustomFlag1, RV_CustomDecimal1, RV_CarrierCode, RV_ScreeningStatus, RV_RN_NKCountryOfReg, RV_MaritimeMobileServiceIdentity, RV_StatusCode, RV_StatCode5, RV_IsGearless, RV_Length, RV_Breadth, RV_Draught, RV_Deadweight, RV_GrossTonnage, RV_GrainCapacity, RV_LiquidCapacity, RV_RoroLanesLength, RV_RoroLanesWidth, RV_RoroLanesClearHeight, RV_RoroLanesNumber, RV_RoroRampsNumber, RV_TEU, RV_CarsNumber, RV_ReeferPointsNumber, RV_TanksNumber)
VALUES (@RV_PK, @RV_Code, @RV_IsActive, @RV_LloydsNumber, @RV_MalaysiaVesselId, @RV_RadioCallSign, @RV_NetRegisterTon, @RV_VesselType, @RV_YearOfConstruction, @RV_CustomAttrib1, @RV_CustomAttrib2, @RV_CustomAttrib3, @RV_CustomFlag1, @RV_CustomDecimal1, @RV_CarrierCode, @RV_ScreeningStatus, @RV_RN_NKCountryOfReg, @RV_MaritimeMobileServiceIdentity, @RV_StatusCode, @RV_StatCode5, @RV_IsGearless, @RV_Length, @RV_Breadth, @RV_Draught, @RV_Deadweight, @RV_GrossTonnage, @RV_GrainCapacity, @RV_LiquidCapacity, @RV_RoroLanesLength, @RV_RoroLanesWidth, @RV_RoroLanesClearHeight, @RV_RoroLanesNumber, @RV_RoroRampsNumber, @RV_TEU, @RV_CarsNumber, @RV_ReeferPointsNumber, @RV_TanksNumber)

UPDATE RefDbVersionControl SET RVC_Deleted = ~@RV_IsPublished WHERE RVC_ParentPK = @RV_PK AND RVC_ParentCode = 'RV'
END
GO

CREATE PROCEDURE [dbo].[RefVesselUserView_Del]
	@RV_PK UNIQUEIDENTIFIER
AS
DELETE RefVessel WHERE RV_PK = @RV_PK
GO

CREATE PROCEDURE [dbo].[RefVesselUserView_Ups]
	@RV_PK UNIQUEIDENTIFIER,
	@RV_Code VARCHAR(50),
	@RV_IsActive BIT,
	@RV_LloydsNumber CHAR(7),
	@RV_MalaysiaVesselId VARCHAR(9),
	@RV_RadioCallSign VARCHAR(10),
	@RV_NetRegisterTon INT,
	@RV_VesselType CHAR(3),
	@RV_YearOfConstruction SMALLINT,
	@RV_CustomAttrib1 VARCHAR(10),
	@RV_CustomAttrib2 VARCHAR(10),
	@RV_CustomAttrib3 VARCHAR(10),
	@RV_CustomFlag1 BIT,
	@RV_CustomDecimal1 DECIMAL(9,3),
	@RV_CarrierCode VARCHAR(4),
	@RV_ScreeningStatus CHAR(3),
	@RV_RN_NKCountryOfReg VARCHAR(2),
	@RV_MaritimeMobileServiceIdentity CHAR(9),
	@RV_StatusCode CHAR(3),
	@RV_StatCode5 CHAR(7),
	@RV_IsGearless BIT,
	@RV_Length DECIMAL(7,3),
	@RV_Breadth DECIMAL(7,3),
	@RV_Draught DECIMAL(7,3),
	@RV_Deadweight DECIMAL(10,3),
	@RV_GrossTonnage DECIMAL(10,3),
	@RV_GrainCapacity DECIMAL(10,3),
	@RV_LiquidCapacity DECIMAL(10,3),
	@RV_RoroLanesLength DECIMAL(8,3),
	@RV_RoroLanesWidth DECIMAL(8,3),
	@RV_RoroLanesClearHeight DECIMAL(6,3),
	@RV_RoroLanesNumber SMALLINT,
	@RV_RoroRampsNumber TINYINT,
	@RV_TEU INT,
	@RV_CarsNumber INT,
	@RV_ReeferPointsNumber SMALLINT,
	@RV_TanksNumber SMALLINT,
	@RV_IsPublished BIT
AS
BEGIN

UPDATE RefVessel SET
	RV_Code = @RV_Code,
	RV_IsActive = @RV_IsActive,
	RV_LloydsNumber = @RV_LloydsNumber,
	RV_MalaysiaVesselId = @RV_MalaysiaVesselId,
	RV_RadioCallSign = @RV_RadioCallSign,
	RV_NetRegisterTon = @RV_NetRegisterTon,
	RV_VesselType = @RV_VesselType,
	RV_YearOfConstruction = @RV_YearOfConstruction,
	RV_CustomAttrib1 = @RV_CustomAttrib1,
	RV_CustomAttrib2 = @RV_CustomAttrib2,
	RV_CustomAttrib3 = @RV_CustomAttrib3,
	RV_CustomFlag1 = @RV_CustomFlag1,
	RV_CustomDecimal1 = @RV_CustomDecimal1,
	RV_CarrierCode = @RV_CarrierCode,
	RV_ScreeningStatus = @RV_ScreeningStatus,
	RV_RN_NKCountryOfReg = @RV_RN_NKCountryOfReg,
	RV_MaritimeMobileServiceIdentity = @RV_MaritimeMobileServiceIdentity,
	RV_StatusCode = @RV_StatusCode,
	RV_StatCode5 = @RV_StatCode5,
	RV_IsGearless = @RV_IsGearless,
	RV_Length = @RV_Length,
	RV_Breadth = @RV_Breadth,
	RV_Draught = @RV_Draught,
	RV_Deadweight = @RV_Deadweight,
	RV_GrossTonnage = @RV_GrossTonnage,
	RV_GrainCapacity = @RV_GrainCapacity,
	RV_LiquidCapacity = @RV_LiquidCapacity,
	RV_RoroLanesLength = @RV_RoroLanesLength,
	RV_RoroLanesWidth = @RV_RoroLanesWidth,
	RV_RoroLanesClearHeight = @RV_RoroLanesClearHeight,
	RV_RoroLanesNumber = @RV_RoroLanesNumber,
	RV_RoroRampsNumber = @RV_RoroRampsNumber,
	RV_TEU = @RV_TEU,
	RV_CarsNumber = @RV_CarsNumber,
	RV_ReeferPointsNumber = @RV_ReeferPointsNumber,
	RV_TanksNumber = @RV_TanksNumber
WHERE RV_PK = @RV_PK

UPDATE RefDbVersionControl SET RVC_Deleted = ~@RV_IsPublished WHERE RVC_ParentPK = @RV_PK and RVC_ParentCode = 'RV'
END

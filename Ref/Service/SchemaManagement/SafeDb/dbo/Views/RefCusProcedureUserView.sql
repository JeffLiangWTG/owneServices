CREATE VIEW [dbo].[RefCusProcedureUserView]
WITH SCHEMABINDING
AS

SELECT
    ZZ6_PK AS ZZ6_PK,
    ZZ6_Category AS ZZ6_Category,
    ZZ6_ProcedureCode AS ZZ6_ProcedureCode,
    ZZ6_PreviousProcedureCode AS ZZ6_PreviousProcedureCode,
    ZZ6_Concession AS ZZ6_Concession,
    ZZ6_Description AS ZZ6_Description,
    ZZ6_ZZZ_NKDataGrouping AS ZZ6_CountryOrGrouping,
    ZZ6_ShipmentType AS ZZ6_ShipmentType,
    ZZ6_CalculateDuty AS ZZ6_CalculateDuty,
    ZZ6_Group AS ZZ6_Group,
    ZZ6_LandedCost AS ZZ6_LandedCost,
    ZZ6_IntoWarehouse AS ZZ6_IntoWarehouse,
    ZZ6_OutOfWarehouse AS ZZ6_OutOfWarehouse,
    ZZ6_IntoInwardProcessing AS ZZ6_IntoInwardProcessing,
    ZZ6_OutOfInwardProcessing AS ZZ6_OutOfInwardProcessing,
    ZZ6_IntoOutwardProcessing AS ZZ6_IntoOutwardProcessing,
    ZZ6_OutofOutwardProcessing AS ZZ6_OutofOutwardProcessing,
    ZZ6_IntoTemporaryImport AS ZZ6_IntoTemporaryImport,
    ZZ6_OutOfTemporaryImport AS ZZ6_OutOfTemporaryImport,
    ZZ6_IntoTemporaryExport AS ZZ6_IntoTemporaryExport,
    ZZ6_OutOfTemporaryExport AS ZZ6_OutOfTemporaryExport,
    ZZ6_StartDate AS ZZ6_StartDate,
    ZZ6_EndDate AS ZZ6_EndDate,
    ZZ6_CalculateVAT AS ZZ6_CalculateVAT,
    ZZ6_IsGuaranteeConsumed AS ZZ6_IsGuaranteeConsumed,
    ZZ6_IsGuaranteeReleased AS ZZ6_IsGuaranteeReleased,
    ZZ6_IsTransit AS ZZ6_IsTransit,
	ZZ6_SysStartTime AS ZZ6_SysStartTime,
	ZZ6_SysEndTime AS ZZ6_SysEndTime,
    ~RVC_Deleted AS ZZ6_IsPublished
FROM [dbo].RefCusProcedure
JOIN [dbo].RefDbVersionControl ON RVC_ParentPK = ZZ6_PK AND RVC_ParentCode = 'ZZ6'
GO

CREATE PROCEDURE [dbo].[RefCusProcedureUserView_Ins]
    @ZZ6_PK UNIQUEIDENTIFIER,
    @ZZ6_Category VARCHAR(3),
    @ZZ6_ProcedureCode VARCHAR(5),
    @ZZ6_PreviousProcedureCode VARCHAR(5),
    @ZZ6_Concession VARCHAR(50),
    @ZZ6_Description NVARCHAR(MAX),
    @ZZ6_CountryOrGrouping VARCHAR(3),
    @ZZ6_ShipmentType VARCHAR(50),
    @ZZ6_CalculateDuty BIT,
    @ZZ6_Group VARCHAR(200),
    @ZZ6_LandedCost BIT,
    @ZZ6_IntoWarehouse CHAR(1),
    @ZZ6_OutOfWarehouse CHAR(1),
    @ZZ6_IntoInwardProcessing CHAR(1),
    @ZZ6_OutOfInwardProcessing CHAR(1),
    @ZZ6_IntoOutwardProcessing CHAR(1),
    @ZZ6_OutofOutwardProcessing CHAR(1),
    @ZZ6_IntoTemporaryImport CHAR(1),
    @ZZ6_OutOfTemporaryImport CHAR(1),
    @ZZ6_IntoTemporaryExport CHAR(1),
    @ZZ6_OutOfTemporaryExport CHAR(1),
    @ZZ6_StartDate SMALLDATETIME,
    @ZZ6_EndDate SMALLDATETIME,
    @ZZ6_CalculateVAT BIT,
    @ZZ6_IsGuaranteeConsumed CHAR(1),
    @ZZ6_IsGuaranteeReleased CHAR(1),
    @ZZ6_IsTransit CHAR(1),
    @ZZ6_IsPublished BIT
AS
BEGIN

INSERT dbo.RefCusProcedure (ZZ6_PK, ZZ6_Category, ZZ6_ProcedureCode, ZZ6_PreviousProcedureCode, ZZ6_Concession, ZZ6_Description, ZZ6_ZZZ_NKDataGrouping, ZZ6_ShipmentType, ZZ6_CalculateDuty, ZZ6_Group, ZZ6_LandedCost, ZZ6_IntoWarehouse, ZZ6_OutOfWarehouse, ZZ6_IntoInwardProcessing, ZZ6_OutOfInwardProcessing, ZZ6_IntoOutwardProcessing, ZZ6_OutofOutwardProcessing, ZZ6_IntoTemporaryImport, ZZ6_OutOfTemporaryImport, ZZ6_IntoTemporaryExport, ZZ6_OutOfTemporaryExport, ZZ6_StartDate, ZZ6_EndDate, ZZ6_CalculateVAT, ZZ6_IsGuaranteeConsumed, ZZ6_IsGuaranteeReleased, ZZ6_IsTransit)
VALUES (@ZZ6_PK, @ZZ6_Category, @ZZ6_ProcedureCode, @ZZ6_PreviousProcedureCode, @ZZ6_Concession, @ZZ6_Description, @ZZ6_CountryOrGrouping, @ZZ6_ShipmentType, @ZZ6_CalculateDuty, @ZZ6_Group, @ZZ6_LandedCost, @ZZ6_IntoWarehouse, @ZZ6_OutOfWarehouse, @ZZ6_IntoInwardProcessing, @ZZ6_OutOfInwardProcessing, @ZZ6_IntoOutwardProcessing, @ZZ6_OutofOutwardProcessing, @ZZ6_IntoTemporaryImport, @ZZ6_OutOfTemporaryImport, @ZZ6_IntoTemporaryExport, @ZZ6_OutOfTemporaryExport, @ZZ6_StartDate, @ZZ6_EndDate, @ZZ6_CalculateVAT, @ZZ6_IsGuaranteeConsumed, @ZZ6_IsGuaranteeReleased, @ZZ6_IsTransit)

UPDATE RefDbVersionControl SET RVC_Deleted = ~@ZZ6_IsPublished WHERE RVC_ParentPK = @ZZ6_PK AND RVC_ParentCode = 'ZZ6'
END
GO

CREATE PROCEDURE [dbo].[RefCusProcedureUserView_Ups]
    @ZZ6_PK UNIQUEIDENTIFIER,
    @ZZ6_Category VARCHAR(3),
    @ZZ6_ProcedureCode VARCHAR(5),
    @ZZ6_PreviousProcedureCode VARCHAR(5),
    @ZZ6_Concession VARCHAR(50),
    @ZZ6_Description NVARCHAR(MAX),
    @ZZ6_CountryOrGrouping VARCHAR(3),
    @ZZ6_ShipmentType VARCHAR(50),
    @ZZ6_CalculateDuty BIT,
    @ZZ6_Group VARCHAR(200),
    @ZZ6_LandedCost BIT,
    @ZZ6_IntoWarehouse CHAR(1),
    @ZZ6_OutOfWarehouse CHAR(1),
    @ZZ6_IntoInwardProcessing CHAR(1),
    @ZZ6_OutOfInwardProcessing CHAR(1),
    @ZZ6_IntoOutwardProcessing CHAR(1),
    @ZZ6_OutofOutwardProcessing CHAR(1),
    @ZZ6_IntoTemporaryImport CHAR(1),
    @ZZ6_OutOfTemporaryImport CHAR(1),
    @ZZ6_IntoTemporaryExport CHAR(1),
    @ZZ6_OutOfTemporaryExport CHAR(1),
    @ZZ6_StartDate SMALLDATETIME,
    @ZZ6_EndDate SMALLDATETIME,
    @ZZ6_CalculateVAT BIT,
    @ZZ6_IsGuaranteeConsumed CHAR(1),
    @ZZ6_IsGuaranteeReleased CHAR(1),
    @ZZ6_IsTransit CHAR(1),
    @ZZ6_IsPublished BIT
AS
BEGIN

UPDATE RefCusProcedure SET
    ZZ6_Category = @ZZ6_Category,
    ZZ6_ProcedureCode = @ZZ6_ProcedureCode,
    ZZ6_PreviousProcedureCode = @ZZ6_PreviousProcedureCode,
    ZZ6_Concession = @ZZ6_Concession,
    ZZ6_Description = @ZZ6_Description,
    ZZ6_ZZZ_NKDataGrouping = @ZZ6_CountryOrGrouping,
    ZZ6_ShipmentType = @ZZ6_ShipmentType,
    ZZ6_CalculateDuty = @ZZ6_CalculateDuty,
    ZZ6_Group = @ZZ6_Group,
    ZZ6_LandedCost = @ZZ6_LandedCost,
    ZZ6_IntoWarehouse = @ZZ6_IntoWarehouse,
    ZZ6_OutOfWarehouse = @ZZ6_OutOfWarehouse,
    ZZ6_IntoInwardProcessing = @ZZ6_IntoInwardProcessing,
    ZZ6_OutOfInwardProcessing = @ZZ6_OutOfInwardProcessing,
    ZZ6_IntoOutwardProcessing = @ZZ6_IntoOutwardProcessing,
    ZZ6_OutofOutwardProcessing = @ZZ6_OutofOutwardProcessing,
    ZZ6_IntoTemporaryImport = @ZZ6_IntoTemporaryImport,
    ZZ6_OutOfTemporaryImport = @ZZ6_OutOfTemporaryImport,
    ZZ6_IntoTemporaryExport = @ZZ6_IntoTemporaryExport,
    ZZ6_OutOfTemporaryExport = @ZZ6_OutOfTemporaryExport,
    ZZ6_StartDate = @ZZ6_StartDate,
    ZZ6_EndDate = @ZZ6_EndDate,
    ZZ6_CalculateVAT = @ZZ6_CalculateVAT,
    ZZ6_IsGuaranteeConsumed = @ZZ6_IsGuaranteeConsumed,
    ZZ6_IsGuaranteeReleased = @ZZ6_IsGuaranteeReleased,
    ZZ6_IsTransit = @ZZ6_IsTransit
WHERE ZZ6_PK = @ZZ6_PK

UPDATE RefDbVersionControl SET RVC_Deleted = ~@ZZ6_IsPublished WHERE RVC_ParentPK = @ZZ6_PK AND RVC_ParentCode = 'ZZ6'
END

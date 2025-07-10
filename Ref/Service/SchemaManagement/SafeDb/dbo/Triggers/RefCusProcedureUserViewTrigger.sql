CREATE TRIGGER RefCusProcedureUserView_Version_Create ON RefCusProcedureUserView
INSTEAD OF INSERT
AS
BEGIN
	INSERT dbo.RefCusProcedure (ZZ6_PK, ZZ6_Category, ZZ6_ProcedureCode, ZZ6_PreviousProcedureCode, ZZ6_Concession, ZZ6_Description, ZZ6_ZZZ_NKDataGrouping, ZZ6_ShipmentType, ZZ6_CalculateDuty, ZZ6_Group, ZZ6_LandedCost, ZZ6_IntoWarehouse, ZZ6_OutOfWarehouse, ZZ6_IntoInwardProcessing, ZZ6_OutOfInwardProcessing, ZZ6_IntoOutwardProcessing, ZZ6_OutofOutwardProcessing, ZZ6_IntoTemporaryImport, ZZ6_OutOfTemporaryImport, ZZ6_IntoTemporaryExport, ZZ6_OutOfTemporaryExport, ZZ6_StartDate, ZZ6_EndDate, ZZ6_CalculateVAT, ZZ6_IsGuaranteeConsumed, ZZ6_IsGuaranteeReleased, ZZ6_IsTransit)
	SELECT ZZ6_PK, ZZ6_Category, ZZ6_ProcedureCode, ZZ6_PreviousProcedureCode, ZZ6_Concession, ZZ6_Description, ZZ6_CountryOrGrouping, ZZ6_ShipmentType, ZZ6_CalculateDuty, ZZ6_Group, ZZ6_LandedCost, ZZ6_IntoWarehouse, ZZ6_OutOfWarehouse, ZZ6_IntoInwardProcessing, ZZ6_OutOfInwardProcessing, ZZ6_IntoOutwardProcessing, ZZ6_OutofOutwardProcessing, ZZ6_IntoTemporaryImport, ZZ6_OutOfTemporaryImport, ZZ6_IntoTemporaryExport, ZZ6_OutOfTemporaryExport, ZZ6_StartDate, ZZ6_EndDate, ZZ6_CalculateVAT, ZZ6_IsGuaranteeConsumed, ZZ6_IsGuaranteeReleased, ZZ6_IsTransit
	FROM inserted

	UPDATE r
	SET RVC_Deleted = ~ZZ6_IsPublished, r.RVC_LastEditedUser = dbo.GetUserId()
	FROM RefDbVersionControl r
	JOIN inserted ON RVC_ParentPK = ZZ6_PK AND RVC_ParentCode = 'ZZ6'
END
GO

CREATE TRIGGER RefCusProcedureUserView_Version_Update ON RefCusProcedureUserView
INSTEAD OF UPDATE
AS
BEGIN
	UPDATE r SET
		ZZ6_Category = i.ZZ6_Category,
		ZZ6_ProcedureCode = i.ZZ6_ProcedureCode,
		ZZ6_PreviousProcedureCode = i.ZZ6_PreviousProcedureCode,
		ZZ6_Concession = i.ZZ6_Concession,
		ZZ6_Description = i.ZZ6_Description,
		ZZ6_ZZZ_NKDataGrouping = i.ZZ6_CountryOrGrouping,
		ZZ6_ShipmentType = i.ZZ6_ShipmentType,
		ZZ6_CalculateDuty = i.ZZ6_CalculateDuty,
		ZZ6_Group = i.ZZ6_Group,
		ZZ6_LandedCost = i.ZZ6_LandedCost,
		ZZ6_IntoWarehouse = i.ZZ6_IntoWarehouse,
		ZZ6_OutOfWarehouse = i.ZZ6_OutOfWarehouse,
		ZZ6_IntoInwardProcessing = i.ZZ6_IntoInwardProcessing,
		ZZ6_OutOfInwardProcessing = i.ZZ6_OutOfInwardProcessing,
		ZZ6_IntoOutwardProcessing = i.ZZ6_IntoOutwardProcessing,
		ZZ6_OutofOutwardProcessing = i.ZZ6_OutofOutwardProcessing,
		ZZ6_IntoTemporaryImport = i.ZZ6_IntoTemporaryImport,
		ZZ6_OutOfTemporaryImport = i.ZZ6_OutOfTemporaryImport,
		ZZ6_IntoTemporaryExport = i.ZZ6_IntoTemporaryExport,
		ZZ6_OutOfTemporaryExport = i.ZZ6_OutOfTemporaryExport,
		ZZ6_StartDate = i.ZZ6_StartDate,
		ZZ6_EndDate = i.ZZ6_EndDate,
		ZZ6_CalculateVAT = i.ZZ6_CalculateVAT,
		ZZ6_IsGuaranteeConsumed = i.ZZ6_IsGuaranteeConsumed,
		ZZ6_IsGuaranteeReleased = i.ZZ6_IsGuaranteeReleased,
		ZZ6_IsTransit = i.ZZ6_IsTransit
	FROM RefCusProcedure r
	JOIN inserted i ON r.ZZ6_PK = i.ZZ6_PK

	UPDATE r
	SET RVC_Deleted = ~ZZ6_IsPublished, r.RVC_LastEditedUser = dbo.GetUserId()
	FROM RefDbVersionControl r
	JOIN inserted ON RVC_ParentPK = ZZ6_PK AND RVC_ParentCode = 'ZZ6'
END
GO

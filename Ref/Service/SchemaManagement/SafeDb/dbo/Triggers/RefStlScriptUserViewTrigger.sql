CREATE TRIGGER RefStlScriptUserView_Version_Create ON RefStlScriptUserView
INSTEAD OF INSERT
AS
BEGIN
	INSERT [dbo].[RefStlScript] (STL_PK, STL_FeatureCode, STL_RoleName, STL_ModuleName, STL_FunctionName, STL_FeatureName, STL_DataGranularity, STL_CompanyCode, STL_BranchCode, STL_TransactionDateUtc, STL_CreatingUserCode, STL_GuidReference, STL_BillingReference1, STL_BillingReference2, STL_BillingReference3, STL_BillingReference4, STL_AdditionalRefs, STL_TransactionCount, STL_PreparationScript, STL_FromClause, STL_WhereClause, STL_WithOptionRecompile, STL_UsedInBilling, STL_ActiveOn, STL_MinCW1Version, STL_MaxCW1Version, STL_DateType, STL_CollectionStartDateUtc)
	SELECT STL_PK, STL_FeatureCode, STL_RoleName, STL_ModuleName, STL_FunctionName, STL_FeatureName, STL_DataGranularity, STL_CompanyCode, STL_BranchCode, STL_TransactionDateUtc, STL_CreatingUserCode, STL_GuidReference, STL_BillingReference1, STL_BillingReference2, STL_BillingReference3, STL_BillingReference4, STL_AdditionalRefs, STL_TransactionCount, STL_PreparationScript, STL_FromClause, STL_WhereClause, STL_WithOptionRecompile, STL_UsedInBilling, STL_ActiveOn, STL_MinCW1Version, STL_MaxCW1Version, STL_DateType, STL_CollectionStartDateUtc
	FROM inserted

	UPDATE r SET RVC_Deleted = ~STL_IsPublished, r.RVC_LastEditedUser = dbo.GetUserId()
	FROM RefDbVersionControl r
	JOIN inserted ON RVC_ParentPK = STL_PK AND RVC_ParentCode = 'STL'
END
GO

CREATE TRIGGER RefStlScriptUserView_Version_Update ON RefStlScriptUserView
INSTEAD OF UPDATE
AS
BEGIN
	UPDATE r SET
		STL_PK = i.STL_PK,
		STL_FeatureCode = i.STL_FeatureCode,
		STL_RoleName = i.STL_RoleName,
		STL_ModuleName = i.STL_ModuleName,
		STL_FunctionName = i.STL_FunctionName,
		STL_FeatureName = i.STL_FeatureName,
		STL_DataGranularity = i.STL_DataGranularity,
		STL_CompanyCode = i.STL_CompanyCode,
		STL_BranchCode = i.STL_BranchCode,
		STL_TransactionDateUtc = i.STL_TransactionDateUtc,
		STL_CreatingUserCode = i.STL_CreatingUserCode,
		STL_GuidReference = i.STL_GuidReference,
		STL_BillingReference1 = i.STL_BillingReference1,
		STL_BillingReference2 = i.STL_BillingReference2,
		STL_BillingReference3 = i.STL_BillingReference3,
		STL_BillingReference4 = i.STL_BillingReference4,
		STL_AdditionalRefs = i.STL_AdditionalRefs,
		STL_TransactionCount = i.STL_TransactionCount,
		STL_PreparationScript = i.STL_PreparationScript,
		STL_FromClause = i.STL_FromClause,
		STL_WhereClause = i.STL_WhereClause,
		STL_WithOptionRecompile = i.STL_WithOptionRecompile,
		STL_UsedInBilling = i.STL_UsedInBilling,
		STL_ActiveOn = i.STL_ActiveOn,
		STL_MinCW1Version = i.STL_MinCW1Version,
		STL_MaxCW1Version = i.STL_MaxCW1Version,
		STL_DateType = i.STL_DateType,
		STL_CollectionStartDateUtc = i.STL_CollectionStartDateUtc
	FROM RefStlScript r
	JOIN inserted i ON r.STL_PK = i.STL_PK

	UPDATE r
	SET RVC_Deleted = ~STL_IsPublished, r.RVC_LastEditedUser = dbo.GetUserId()
	FROM RefDbVersionControl r
	JOIN inserted ON RVC_ParentPK = STL_PK and RVC_ParentCode = 'STL'
END
GO

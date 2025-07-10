CREATE VIEW [dbo].[RefStlScriptUserView]
WITH SCHEMABINDING
AS

SELECT
	STL_PK,
	STL_FeatureCode,
	STL_RoleName,
	STL_ModuleName,
	STL_FunctionName,
	STL_FeatureName,
	STL_DataGranularity,
	STL_CompanyCode,
	STL_BranchCode,
	STL_TransactionDateUtc,
	STL_CreatingUserCode,
	STL_GuidReference,
	STL_BillingReference1,
	STL_BillingReference2,
	STL_BillingReference3,
	STL_BillingReference4,
	STL_AdditionalRefs,
	STL_TransactionCount,
	STL_PreparationScript,
	STL_FromClause,
	STL_WhereClause,
	STL_WithOptionRecompile,
	STL_UsedInBilling,
	STL_ActiveOn,
	STL_MinCW1Version,
	STL_MaxCW1Version,
	STL_DateType,
	STL_CollectionStartDateUtc,
	STL_SysStartTime,
	STL_SysEndTime,
	CAST (1 AS BIT) AS STL_IsSystem,
	~RVC_Deleted AS STL_IsPublished
FROM [dbo].[RefStlScript]
JOIN [dbo].[RefDbVersionControl] ON RVC_ParentPK = STL_PK AND RVC_ParentCode = 'STL'
GO

CREATE PROCEDURE [dbo].[RefStlScriptUserView_Ins]
	@STL_PK UNIQUEIDENTIFIER,
	@STL_FeatureCode CHAR(3),
	@STL_RoleName NVARCHAR(50),
	@STL_ModuleName NVARCHAR(50),
	@STL_FunctionName NVARCHAR(50),
	@STL_FeatureName NVARCHAR(75),
	@STL_DataGranularity CHAR(3),
	@STL_CompanyCode NVARCHAR(1000),
	@STL_BranchCode NVARCHAR(1000),
	@STL_TransactionDateUtc NVARCHAR(1000),
	@STL_CreatingUserCode NVARCHAR(1000),
	@STL_GuidReference NVARCHAR(1000),
	@STL_BillingReference1 NVARCHAR(1000),
	@STL_BillingReference2 NVARCHAR(1000),
	@STL_BillingReference3 NVARCHAR(1000),
	@STL_BillingReference4 NVARCHAR(1000),
	@STL_AdditionalRefs NVARCHAR(1000),
	@STL_TransactionCount NVARCHAR(1000),
	@STL_PreparationScript NVARCHAR(MAX),
	@STL_FromClause NVARCHAR(MAX),
	@STL_WhereClause NVARCHAR(MAX),
	@STL_WithOptionRecompile BIT,
	@STL_UsedInBilling BIT,
	@STL_ActiveOn CHAR(3),
	@STL_MinCW1Version NVARCHAR(20),
	@STL_MaxCW1Version NVARCHAR(20),
	@STL_DateType CHAR(3),
	@STL_CollectionStartDateUtc DATETIME,
	@STL_IsPublished BIT
AS
BEGIN

INSERT [dbo].[RefStlScript] (STL_PK, STL_FeatureCode, STL_RoleName, STL_ModuleName, STL_FunctionName, STL_FeatureName, STL_DataGranularity, STL_CompanyCode, STL_BranchCode, STL_TransactionDateUtc, STL_CreatingUserCode, STL_GuidReference, STL_BillingReference1, STL_BillingReference2, STL_BillingReference3, STL_BillingReference4, STL_AdditionalRefs, STL_TransactionCount, STL_PreparationScript, STL_FromClause, STL_WhereClause, STL_WithOptionRecompile, STL_UsedInBilling, STL_ActiveOn, STL_MinCW1Version, STL_MaxCW1Version, STL_DateType, STL_CollectionStartDateUtc)
VALUES (@STL_PK, @STL_FeatureCode, @STL_RoleName, @STL_ModuleName, @STL_FunctionName, @STL_FeatureName, @STL_DataGranularity, @STL_CompanyCode, @STL_BranchCode, @STL_TransactionDateUtc, @STL_CreatingUserCode, @STL_GuidReference, @STL_BillingReference1, @STL_BillingReference2, @STL_BillingReference3, @STL_BillingReference4, @STL_AdditionalRefs, @STL_TransactionCount, @STL_PreparationScript, @STL_FromClause, @STL_WhereClause, @STL_WithOptionRecompile, @STL_UsedInBilling, @STL_ActiveOn, @STL_MinCW1Version, @STL_MaxCW1Version, @STL_DateType, @STL_CollectionStartDateUtc)

UPDATE RefDbVersionControl SET RVC_Deleted = ~@STL_IsPublished WHERE RVC_ParentPK = @STL_PK AND RVC_ParentCode = 'STL'
END
GO

CREATE PROCEDURE [dbo].[RefStlScriptUserView_Del]
	@STL_PK UNIQUEIDENTIFIER
AS
DELETE RefStlScript WHERE STL_PK = @STL_PK
GO

CREATE PROCEDURE [dbo].[RefStlScriptUserView_Ups]
	@STL_PK UNIQUEIDENTIFIER,
	@STL_FeatureCode CHAR(3),
	@STL_RoleName NVARCHAR(50),
	@STL_ModuleName NVARCHAR(50),
	@STL_FunctionName NVARCHAR(50),
	@STL_FeatureName NVARCHAR(75),
	@STL_DataGranularity CHAR(3),
	@STL_CompanyCode NVARCHAR(1000),
	@STL_BranchCode NVARCHAR(1000),
	@STL_TransactionDateUtc NVARCHAR(1000),
	@STL_CreatingUserCode NVARCHAR(1000),
	@STL_GuidReference NVARCHAR(1000),
	@STL_BillingReference1 NVARCHAR(1000),
	@STL_BillingReference2 NVARCHAR(1000),
	@STL_BillingReference3 NVARCHAR(1000),
	@STL_BillingReference4 NVARCHAR(1000),
	@STL_AdditionalRefs NVARCHAR(1000),
	@STL_TransactionCount NVARCHAR(1000),
	@STL_PreparationScript NVARCHAR(MAX),
	@STL_FromClause NVARCHAR(MAX),
	@STL_WhereClause NVARCHAR(MAX),
	@STL_WithOptionRecompile BIT,
	@STL_UsedInBilling BIT,
	@STL_ActiveOn CHAR(3),
	@STL_MinCW1Version NVARCHAR(20),
	@STL_MaxCW1Version NVARCHAR(20),
	@STL_DateType CHAR(3),
	@STL_CollectionStartDateUtc DATETIME,
	@STL_IsPublished BIT
AS
BEGIN

UPDATE RefStlScript SET
STL_PK = @STL_PK,
STL_FeatureCode = @STL_FeatureCode,
STL_RoleName = @STL_RoleName,
STL_ModuleName = @STL_ModuleName,
STL_FunctionName = @STL_FunctionName,
STL_FeatureName = @STL_FeatureName,
STL_DataGranularity = @STL_DataGranularity,
STL_CompanyCode = @STL_CompanyCode,
STL_BranchCode = @STL_BranchCode,
STL_TransactionDateUtc = @STL_TransactionDateUtc,
STL_CreatingUserCode = @STL_CreatingUserCode,
STL_GuidReference = @STL_GuidReference,
STL_BillingReference1 = @STL_BillingReference1,
STL_BillingReference2 = @STL_BillingReference2,
STL_BillingReference3 = @STL_BillingReference3,
STL_BillingReference4 = @STL_BillingReference4,
STL_AdditionalRefs = @STL_AdditionalRefs,
STL_TransactionCount = @STL_TransactionCount,
STL_PreparationScript = @STL_PreparationScript,
STL_FromClause = @STL_FromClause,
STL_WhereClause = @STL_WhereClause,
STL_WithOptionRecompile = @STL_WithOptionRecompile,
STL_UsedInBilling = @STL_UsedInBilling,
STL_ActiveOn = @STL_ActiveOn,
STL_MinCW1Version = @STL_MinCW1Version,
STL_MaxCW1Version = @STL_MaxCW1Version,
STL_DateType = @STL_DateType,
STL_CollectionStartDateUtc = @STL_CollectionStartDateUtc
WHERE STL_PK = @STL_PK

UPDATE RefDbVersionControl SET RVC_Deleted = ~@STL_IsPublished WHERE RVC_ParentPK = @STL_PK and RVC_ParentCode = 'STL'
END

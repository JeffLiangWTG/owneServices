CREATE VIEW [dbo].[RefAccTaxRateUserView]
WITH SCHEMABINDING
AS

SELECT
	ZAT_PK AS ZAT_PK,
	ZAT_RN_NKCountry AS ZAT_RN_NKCountry,
	ZAT_ReferenceRateType AS ZAT_ReferenceRateType,
	ZAT_StartDate AS ZAT_StartDate,
	ZAT_EndDate AS ZAT_EndDate,
	ZAT_RateNumerator AS ZAT_RateNumerator,
	ZAT_RateDenominator AS ZAT_RateDenominator,
	ZAT_SysStartTime AS ZAT_SysStartTime,
	ZAT_SysEndTime AS ZAT_SysEndTime,
	CAST (1 AS BIT) AS ZAT_IsSystem,
	~RVC_Deleted AS ZAT_IsPublished

FROM [dbo].RefAccTaxRate
JOIN [dbo].RefDbVersionControl ON RVC_ParentPK = ZAT_PK AND RVC_ParentCode = 'ZAT'
GO

CREATE PROCEDURE [dbo].[RefAccTaxRateUserView_Ins]
	@ZAT_PK uniqueidentifier,
	@ZAT_RN_NKCountry char(2),
	@ZAT_ReferenceRateType varchar(10),
	@ZAT_StartDate datetime2,
	@ZAT_EndDate datetime2,
	@ZAT_RateNumerator int,
	@ZAT_RateDenominator int,
	@ZAT_IsPublished bit
AS
BEGIN

INSERT dbo.RefAccTaxRate (ZAT_PK, ZAT_RN_NKCountry, ZAT_ReferenceRateType, ZAT_StartDate, ZAT_EndDate, ZAT_RateNumerator, ZAT_RateDenominator)
VALUES (@ZAT_PK, @ZAT_RN_NKCountry, @ZAT_ReferenceRateType, @ZAT_StartDate, @ZAT_EndDate, @ZAT_RateNumerator, @ZAT_RateDenominator)

UPDATE RefDbVersionControl SET RVC_Deleted = ~@ZAT_IsPublished WHERE RVC_ParentPK = @ZAT_PK AND RVC_ParentCode = 'ZAT'
END
GO

CREATE PROCEDURE [dbo].[RefAccTaxRateUserView_Del]
	@ZAT_PK uniqueidentifier
AS
DELETE RefAccTaxRate WHERE ZAT_PK = @ZAT_PK
GO

CREATE PROCEDURE [dbo].[RefAccTaxRateUserView_Ups]
	@ZAT_PK uniqueidentifier,
	@ZAT_RN_NKCountry char(2),
	@ZAT_ReferenceRateType varchar(10),
	@ZAT_StartDate datetime2,
	@ZAT_EndDate datetime2,
	@ZAT_RateNumerator int,
	@ZAT_RateDenominator int,
	@ZAT_IsPublished bit
AS
BEGIN

UPDATE RefAccTaxRate SET
	ZAT_RN_NKCountry = @ZAT_RN_NKCountry,
	ZAT_ReferenceRateType = @ZAT_ReferenceRateType,
	ZAT_StartDate = @ZAT_StartDate,
	ZAT_EndDate = @ZAT_EndDate,
	ZAT_RateNumerator = @ZAT_RateNumerator,
	ZAT_RateDenominator = @ZAT_RateDenominator
WHERE ZAT_PK = @ZAT_PK

UPDATE RefDbVersionControl SET RVC_Deleted = ~@ZAT_IsPublished WHERE RVC_ParentPK = @ZAT_PK and RVC_ParentCode = 'ZAT'
END

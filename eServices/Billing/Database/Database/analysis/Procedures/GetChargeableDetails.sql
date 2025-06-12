--For Multidimensional Billing Cube
CREATE PROCEDURE analysis.GetChargeableDetails

@BillingPeriod VARCHAR(MAX),
@CodeFeature VARCHAR(MAX),
@PriceItemCode VARCHAR(MAX),
@Category VARCHAR(MAX),
@DatabaseNumber VARCHAR(MAX),
@ClientCompanyPK VARCHAR(MAX)

AS

BEGIN

SET NOCOUNT ON;

DECLARE @BillingPeriodID INT = 0
DECLARE @PriceItemCodeID CHAR(3), @CategoryID CHAR(3), @DatabaseNumberID INT, @ClientCompanyID UNIQUEIDENTIFIER, @CompanyNumberID SMALLINT

IF ISNUMERIC(@BillingPeriod) = 1
	SET @BillingPeriodID = CAST(@BillingPeriod AS INT)

IF @CodeFeature <> '0'
BEGIN
	SELECT TOP 1 @PriceItemCodeID = v.PriceItemCode, @CategoryID = v.Category 
	FROM [analysis].[vwPriceItem] v
	WHERE v.CodeFeature = @CodeFeature
END ELSE
BEGIN
	SET @PriceItemCodeID = @PriceItemCode
	SET @CategoryID = @Category
END

IF @DatabaseNumber <> '0' AND ISNUMERIC(@DatabaseNumber) = 1
BEGIN
	SET @DatabaseNumberID = CAST(@DatabaseNumber AS INT)
END

IF TRY_CONVERT(UNIQUEIDENTIFIER, @ClientCompanyPK) IS NOT NULL
	SET @ClientCompanyID = CAST(@ClientCompanyPK AS UNIQUEIDENTIFIER)

SELECT TOP 1 @DatabaseNumberID = cc.DatabaseNumber, @CompanyNumberID = cc.CompanyNumber
FROM edi.ClientCompany cc
WHERE cc.LCC_PK = @ClientCompanyID

SELECT TOP 10000
	ch.[CH_Period] AS BillingPeriod,
	ch.[CH_Category] AS Category,
	ch.[CH_PriceItemCode] AS PriceItemCode,
	ch.[CH_ReportingSource] AS ReportingSource,
	ch.[CH_ClientStaffCode] AS ClientStaffCode,
	ch.[CH_ClientID] AS LicenceClientKey,
	ch.[CH_ClientNumber] AS ClientNumber,
	ch.[CH_BillableCount] AS BillableCount,
	ch.[CH_Reference1] AS Reference1,
	ch.[CH_Reference2] AS Reference2,
	ch.[CH_Reference3] AS Reference3,
	ch.[CH_Reference4] AS Reference4,
	ch.[CH_Reference5] AS Reference5,
	ch.[CH_Branch] AS Branch,
	CAST(AuSydServiceDateTime.Value AS DATE) AS DateServiceOccurred,
	CAST([CH_ServiceOccuredUtc] AS DATE) AS DateServiceOccurredUTC

FROM [edi].[Chargeable] ch
	CROSS APPLY [dbo].[AuSydTime](ch.CH_ServiceOccuredUTC) AS AuSydServiceDateTime
WHERE ch.CH_Period = @BillingPeriodID
	AND ch.CH_Category = @CategoryID
	AND ch.CH_PriceItemCode = @PriceItemCodeID
	AND (@DatabaseNumberID IS NULL OR ch.CH_DatabaseNumber = @DatabaseNumberID)
	AND (@CompanyNumberID IS NULL OR ch.CH_CompanyNumber = @CompanyNumberID)

END

--GO

--GRANT EXECUTE ON analysis.GetChargeableDetails TO [billing_reader]

--GO
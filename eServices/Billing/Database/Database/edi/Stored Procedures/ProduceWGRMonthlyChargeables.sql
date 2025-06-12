CREATE PROCEDURE edi.ProduceWGRMonthlyChargeables
	@period int
AS

SET NOCOUNT ON;

WITH H as
(
    select *,
	ROW_NUMBER() OVER(PARTITION BY DatabaseNumber ORDER BY ValidFromUtc DESC) AS LicenseRow
	from edi.LicenceDatabaseCodeHistory
),
d as
(
    select *, cast(US_BillableCount as decimal) / cast(US_reference1 as int) CPUCores,
	ROW_NUMBER() OVER(PARTITION BY US_DatabaseNumber, US_CompanyNumber, US_ServiceOccuredUtc ORDER BY cast(US_BillableCount as decimal) / cast(US_Reference1 as int) DESC) AS row_number
	from edi.Usage
	INNER JOIN H ON US_DatabaseNumber = H.DatabaseNumber AND H.LicenseRow = 1
    where US_PriceItemCode = 'WGR' and US_Category = 'WGR' and US_BillableCount > 0 AND US_Period = @period AND US_DatabaseNumber <> 0 AND US_CompanyNumber <> 0 AND LicenceType = 'PRD'
),
d2 as
(
    select US_ServiceOccuredUtc, US_DatabaseNumber, US_CompanyNumber, sum(CPUCores) SumCPUCores,
	ROW_NUMBER() OVER(PARTITION BY US_DatabaseNumber, US_CompanyNumber ORDER BY sum(CPUCores) DESC) AS SumCoresRow
    from d
    group by US_ServiceOccuredUtc, US_DatabaseNumber, US_CompanyNumber
),
d3 as
(
	select *
	FROM d2 WHERE SumCPUCores > 1 AND SumCoresRow = 1
),
usr as
(
	select sum(CH_BillableCount) UserCount, CH_DatabaseNumber
	from edi.Chargeable
	WHERE CH_Period = @period AND CH_Category = 'STL' AND CH_PriceItemCode = 'USR'
	group by CH_DatabaseNumber
)

INSERT INTO edi.Chargeable (CH_ID, CH_PERIOD, CH_Category, CH_PriceItemCode, CH_BillableCount, CH_ReportingSource, CH_ServiceOccuredUtc, CH_ClientID, CH_ClientNumber, CH_DatabaseNumber, CH_CompanyNumber, CH_ClientStaffCode, CH_Reference1, CH_Reference2, CH_Reference3, CH_Reference4, CH_Reference5, CH_Version, CH_Branch, CH_SystemCreateUtc, CH_SystemLastEditUtc, CH_CapturedUtc, CH_MessageTrackingID)
SELECT
  US_ID, US_PERIOD, US_Category, US_PriceItemCode, CASE WHEN UserCount is NULL THEN ceiling(SumCPUCores) ELSE ceiling(SumCPUCores - (cast(UserCount as decimal)/100.0)) END BillableCount, US_ReportingSource, d3.US_ServiceOccuredUtc, US_ClientID, US_ClientNumber, d.US_DatabaseNumber, d.US_CompanyNumber, US_ClientStaffCode, '', US_Reference2, US_Reference3, US_Reference4, US_Reference5, US_Version, US_Branch, US_SystemCreateUtc, US_SystemLastEditUtc, US_CapturedUtc, US_MessageTrackingID
FROM d
Inner join d3
ON d.US_DatabaseNumber = d3.US_DatabaseNumber AND d.US_CompanyNumber = d3.US_CompanyNumber AND d.US_ServiceOccuredUtc = d3.US_ServiceOccuredUtc
left join usr
ON d.US_DatabaseNumber = usr.CH_DatabaseNumber
WHERE row_number = 1 AND CASE WHEN UserCount is NULL THEN ceiling(SumCPUCores) ELSE ceiling(SumCPUCores - (cast(UserCount as decimal)/100.0)) END > 0

RETURN 0

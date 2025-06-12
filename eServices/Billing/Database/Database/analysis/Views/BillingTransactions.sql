--For Multidimensional Billing Cube
CREATE VIEW [analysis].[BillingTransactions]

AS

SELECT

	ch.CH_Period AS BillingPeriod,
	ch.CH_Category + '.' + ch.CH_PriceItemCode AS PriceItemKey,
	ch.CH_ReportingSource AS ReportingSource,
	ch.CH_Category AS Category,
	ch.CH_PriceItemCode AS PriceItemCode,
	ch.CH_ClientID AS LicenceClientKey,
	ch.CH_DatabaseNumber AS DatabaseNumber,
	cc.LCC_PK AS ClientCompanyPK,
	ch.CH_ClientStaffCode AS ClientStaffCode,
	CAST(ch.CH_BillableCount AS BIGINT) AS BillableCount,
	CAST(CASE
		WHEN ch.CH_Category = 'STL' AND ch.CH_PriceItemCode = 'USR' THEN ch.CH_BillableCount
		ELSE NULL
	END AS BIGINT) AS ActiveStaff,
	CAST(CASE
		WHEN p.[Function] = 'General Forwarding Engine' THEN ch.CH_BillableCount
		ELSE NULL
	END AS BIGINT) AS GeneralForwardingEngine
FROM
	[edi].[Chargeable] ch
	INNER JOIN edi.ClientCompany cc ON ch.CH_DatabaseNumber = cc.DatabaseNumber and ch.CH_CompanyNumber = cc.CompanyNumber
	LEFT JOIN analysis.PriceItem p ON p.Category = ch.CH_Category AND p.PriceItemCode = ch.CH_PriceItemCode
WHERE
	ch.CH_Period >= 201001

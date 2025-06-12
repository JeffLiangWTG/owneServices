CREATE VIEW edi.ViewChargeable
AS
SELECT TX_ID                = CH_ID
	, TX_Period            = CH_Period
	, TX_Category          = CH_Category
	, TX_PriceItemCode     = CH_PriceItemCode
	, TX_DatabaseNumber    = CH_DatabaseNumber
	, TX_BillableCount     = CH_BillableCount
	, TX_ReportingSource   = CH_ReportingSource
	, TX_ServiceOccuredUTC = CH_ServiceOccuredUtc
	, TX_ClientID          = CH_ClientID
	, TX_ClientNumber      = CH_ClientNumber
	, TX_ClientStaffCode   = CH_ClientStaffCode
	, TX_Reference1        = CH_Reference1
	, TX_Reference2        = CH_Reference2
	, TX_Reference3        = CH_Reference3
	, TX_Reference4        = CH_Reference4
	, TX_Reference5        = CH_Reference5
	, TX_Version           = CH_Version
	, TX_Branch            = CH_Branch
	, TX_SystemCreateUTC   = CH_SystemCreateUtc
	, TX_SystemLastEditUTC = CH_SystemLastEditUtc
	, TX_CapturedUtc       = CH_CapturedUtc
	, TX_MessageTrackingID = CH_MessageTrackingID
	, TX_SystemId = vw.SystemId
	, TX_LCC = c.LCC_PK
FROM edi.Chargeable with (readuncommitted)
-- left join rather than inner join so if the caller doesn't need those columns the join can be skipped
left join edi.ClientCompany c on CH_DatabaseNumber = DatabaseNumber and CH_CompanyNumber = CompanyNumber
left join edi.ViewLicenceDatabaseId vw on CH_DatabaseNumber = vw.DatabaseNumber

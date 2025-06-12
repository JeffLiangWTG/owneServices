CREATE PROCEDURE edi.ProcessCMP
	@Period int
AS
	-- Maintain a list of Interface Names used in Client Mapping (CMP) usage.
	-- ediProd uses the list for validation and option lists
	-- Must be called before reference swaps, since it assumes the interface name is in ref 4.

	declare @utcNow DATETIME2(0) = sysutcdatetime();

	insert edi.Usage(US_ID
		, US_Period
		, US_Category
		, US_PriceItemCode
		, US_DatabaseNumber
		, US_CompanyNumber
		, US_BillableCount
		, US_ReportingSource
		, US_ServiceOccuredUtc
		, US_ClientID
		, US_ClientNumber
		, US_ClientStaffCode
		, US_Reference1
		, US_Reference2
		, US_Reference3
		, US_Reference4
		, US_Reference5
		, US_Version
		, US_Branch
		, US_CapturedUtc
		, US_SystemCreateUtc
		, US_SystemLastEditUtc
		, US_MessageTrackingID)
	select TX_ID 
		, @Period
		, TX_Category
		, TX_PriceItemCode
		, DatabaseNumber
		, CompanyNumber
		, TX_BillableCount
		, TX_ReportingSource
		, TX_ServiceOccuredUTC
		, TX_ClientID
		, TX_ClientNumber
		, TX_ClientStaffCode
		, TX_Reference1
		, TX_Reference2
		, TX_Reference3
		, TX_Reference4
		, TX_Reference5
		, TX_Version
		, TX_Branch
		, CapturedUtc = TX_SystemCreateUTC
		, @utcNow
		, @utcNow
		, TX_MessageTrackingID
	from edi.StagingBatch SB
	INNER JOIN edi.BillingRules BR ON BR_SPName = 'ProcessCMP'
		AND (
				(
				BR_Fields = 'Category-PriceItemCode'
				AND SB.TX_Category = BR.BR_Value1 
				AND SB.TX_PriceItemCode = BR.BR_Value2
				)
			)
	where TX_Period = @Period and TX_Reference4 is null

	delete SB from edi.StagingBatch SB
	INNER JOIN edi.BillingRules BR ON BR_SPName = 'ProcessCMP'
		AND (
				(
				BR_Fields = 'Category-PriceItemCode'
				AND SB.TX_Category = BR.BR_Value1 
				AND SB.TX_PriceItemCode = BR.BR_Value2
				)
			)
	where TX_Period = @Period and TX_Reference4 is null

	select top 0 NULL from edi.ClientMappingInterface with (TABLOCKX);

	insert edi.ClientMappingInterface(Name, FirstCapturedUtc)
	select TX_Reference4, TX_SystemCreateUTC
	from
	(
		select TX_Reference4, TX_SystemCreateUTC = min(TX_SystemCreateUTC)
		from edi.StagingBatch SB
		INNER JOIN edi.BillingRules BR ON BR_SPName = 'ProcessCMP'
			AND (
					(
					BR_Fields = 'Category-PriceItemCode'
					AND SB.TX_Category = BR.BR_Value1 
					AND SB.TX_PriceItemCode = BR.BR_Value2
					)
				)
		where TX_Period = @Period
		group by TX_Reference4
	) a
	left join edi.ClientMappingInterface on Name = TX_Reference4
	where Name is null
	

RETURN 0

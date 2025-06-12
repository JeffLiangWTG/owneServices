CREATE PROCEDURE [edi].[ProcessLastMessage]
	@Period int
AS
begin try
	begin tran;
		WITH StorageBins AS(
			SELECT *, ROW_NUMBER() OVER (PARTITION BY US_Reference1, CASE WHEN ML_DeduplicateRef2 = 1 THEN US_Reference2 ELSE '' END, US_DatabaseNumber, ML_ID ORDER BY US_ServiceOccuredUtc DESC) AS rowNum
			FROM edi.Usage
			INNER JOIN edi.ConfigLastMessageFilter
			ON US_Category = ML_Category AND US_PriceItemCode = ML_PriceItemCode
			WHERE @Period = US_Period)
		INSERT edi.Chargeable(
			  CH_ID
			, CH_Period
			, CH_Category
			, CH_PriceItemCode
			, CH_DatabaseNumber
			, CH_CompanyNumber
			, CH_BillableCount
			, CH_ReportingSource
			, CH_ServiceOccuredUtc
			, CH_ClientID
			, CH_ClientNumber
			, CH_ClientStaffCode
			, CH_Reference1
			, CH_Reference2
			, CH_Reference3
			, CH_Reference4
			, CH_Reference5
			, CH_Version
			, CH_Branch
			, CH_CapturedUtc
			, CH_SystemCreateUtc
			, CH_SystemLastEditUtc
			, CH_MessageTrackingID)
		SELECT US_ID
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
			, US_MessageTrackingID FROM StorageBins
		WHERE rowNum = 1
	commit;
end try
begin catch
	rollback;
	throw;
end catch
RETURN 0

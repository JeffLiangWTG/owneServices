CREATE PROCEDURE [edi].[ProcessEAD]
	@Period int
AS

SET NOCOUNT ON;

-- Order lines in excess of 10
-- Warehouse lines in excess of 10
-- Ref1 and 2 contain the key fields

-- First 10 lines received go in the Usage table.
-- Excess lines go in the Chargeable table.

-- Elimate duplicates in the first 10
update edi.StagingBatch
	set ProcessingStatus = 99
from edi.StagingBatch
join edi.Usage on US_Period = @Period
	and US_Category = TX_Category
	and US_PriceItemCode = TX_PriceItemCode
	and DatabaseNumber = US_DatabaseNumber
	and CompanyNumber = US_CompanyNumber
	and US_ServiceOccuredUtc = TX_ServiceOccuredUTC
	and US_Reference1 = TX_Reference1
	and ISNULL(US_Reference2, '') = ISNULL(TX_Reference2, '')
	and ISNULL(US_Reference3, '') = ISNULL(TX_Reference3, '')
	and ISNULL(US_Reference4, '') = ISNULL(TX_Reference4, '')
	and ISNULL(US_Reference5, '') = ISNULL(TX_Reference5, '')
	and US_ReportingSource = TX_ReportingSource
	and US_ClientID = TX_ClientID
	and US_ClientNumber = TX_ClientNumber
	and ISNULL(US_ClientStaffCode, '') = ISNULL(TX_ClientStaffCode, '')
	and ISNULL(US_MessageTrackingID, '') = ISNULL(TX_MessageTrackingID, '')
where TX_Period = @Period and TX_Category = 'EAD' and TX_PriceItemCode in ('ICA', 'IUA', 'ICJ', 'ICK', 'IUJ', 'IUK')
	and ProcessingStatus != 99;


---- Job IDs
select TX_PriceItemCode, DatabaseNumber, CompanyNumber, TX_Reference1, TX_Reference2, NumExisting = 0
into #job
from edi.StagingBatch
where TX_Period = @Period and TX_Category = 'EAD' and TX_PriceItemCode in ('ICA', 'IUA', 'ICJ', 'ICK', 'IUJ', 'IUK')
	and DatabaseNumber != 0
	and CompanyNumber != 0
	and ProcessingStatus != 99
group by TX_PriceItemCode, DatabaseNumber, CompanyNumber, TX_Reference1, TX_Reference2;

if exists(select top 1 1 from #job)
begin
	declare @monthIndex int = edi.PeriodToIndex(@period);

	-- Look 6 months back and 1 month forward
	declare @monthOffset int = -6;
	while @monthOffset <= 1
	begin
		declare @searchPeriod int = edi.IndexToPeriod(@monthIndex + @monthOffset);

		-- count the number of existing lines
		update #job
			set NumExisting = NumExisting + n
		from #job j
		join
		(
			select n = count(*), TX_PriceItemCode, DatabaseNumber, CompanyNumber, TX_Reference1, TX_Reference2
			from #job
			join edi.Usage on US_Period = @searchPeriod
				and US_Category = 'EAD'
				and US_PriceItemCode in ('ICA', 'IUA', 'ICJ', 'ICK', 'IUJ', 'IUK')
				and US_PriceItemCode = TX_PriceItemCode
				and US_DatabaseNumber = DatabaseNumber
				and US_CompanyNumber = CompanyNumber
				and US_Reference1 = TX_Reference1
				and US_Reference2 = TX_Reference2
			group by TX_PriceItemCode, DatabaseNumber, CompanyNumber, TX_Reference1, TX_Reference2
		) a on j.TX_PriceItemCode = a.TX_PriceItemCode
			and j.DatabaseNumber = a.DatabaseNumber
			and j.CompanyNumber = a.CompanyNumber
			and j.TX_Reference1 = a.TX_Reference1
			and j.TX_Reference2 = a.TX_Reference2
			option (recompile);

		set @monthOffset = @monthOffset + 1
	end;

	-- Give each line in the batch for this period a sequence number 1..10 or 11
	with cte as 
	(
		select NumExisting, Seq = NumExisting + (ROW_NUMBER() OVER (PARTITION BY b.TX_PriceItemCode, b.DatabaseNumber, b.CompanyNumber, b.TX_Reference1, b.TX_Reference2 ORDER BY TX_ServiceOccuredUTC)), b.ProcessingStatus
		from edi.StagingBatch b
		join #job j on j.TX_PriceItemCode = b.TX_PriceItemCode
			and j.DatabaseNumber = b.DatabaseNumber
			and j.CompanyNumber = b.CompanyNumber
			and j.TX_Reference1 = b.TX_Reference1
			and j.TX_Reference2 = b.TX_Reference2
		where b.TX_Period = @Period
			and b.TX_Category = 'EAD' and b.TX_PriceItemCode in ('ICA', 'IUA', 'ICJ', 'ICK', 'IUJ', 'IUK')
			and ProcessingStatus != 99
	)
	update cte set ProcessingStatus = case when Seq <= 10 then 255 else 1 end;

end;

RETURN 0

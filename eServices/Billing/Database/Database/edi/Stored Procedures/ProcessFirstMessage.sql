create procedure edi.ProcessFirstMessage
	@Period int
as

/*
Determine the first message on a job.
Looks back up to configurable period in days for first message.
The first message processed becomes the designated "first" message.
The record is not updated even if messages arrive out of order.
If the message with the first service time arrives late, it is treated as a later message and the record is not updated.
This avoids the need to make updates, while being accurate enough.
It also prevents problems where the first processed message has been billed and later arrivals are for an earlier billing period.
*/


SET NOCOUNT ON;

/*
	all first messages -> ProcessingStatus = 1 (so ProcessSimpleChargeable will put it in edi.Chargeable)
	all filtered out messages -> ProcessingStatus = 99
	all other non-first messages -> ProcessingStatus = 255 (so ProcessSimpleChargeable will put it in edi.Usage)
*/

-- Filter out non-chargeable records
update edi.StagingBatch
	set ProcessingStatus = 99
from edi.StagingBatch
join edi.ConfigFirstMessage on TX_Category = FM_Category and TX_PriceItemCode = FM_PriceItemCode
join edi.ConfigFirstMessageFilter on MF_Category = TX_Category and MF_PriceItemCode = TX_PriceItemCode
cross apply
(
	select Ref = TX_Reference1 where MF_RefIndex = 1 union all
	select Ref = TX_Reference2 where MF_RefIndex = 2 union all
	select Ref = TX_Reference3 where MF_RefIndex = 3 union all
	select Ref = TX_Reference4 where MF_RefIndex = 4 union all
	select Ref = TX_Reference5 where MF_RefIndex = 5
) a
where TX_Period = @Period
	and MF_Operator = '!=' and Ref = MF_RefValue
	and ProcessingStatus != 99;

declare @fromPeriod int;
declare @toPeriod int;
select @fromPeriod = MIN(FM_FromPeriod), @toPeriod = MAX(FM_ToPeriod) from [edi].[GetConfigFirstMessageWithPeriod](@Period);
declare @searchPeriod int = @fromPeriod;

-- Loop through periods and Check if messages in the batch are between FM_FromPeriod and FM_ToPeriod of a first message in edi.Chargeable.
while @searchPeriod <= @toPeriod
begin
	update edi.StagingBatch
		set ProcessingStatus = 255
	from edi.StagingBatch
	join [edi].[GetConfigFirstMessageWithPeriod](@Period) on TX_Category = FM_Category and TX_PriceItemCode = FM_PriceItemCode and @searchPeriod between FM_FromPeriod and FM_ToPeriod
	join edi.Chargeable on CH_Period = @searchPeriod
		and CH_Category = TX_Category
		and CH_PriceItemCode = TX_PriceItemCode
		and CH_DatabaseNumber = DatabaseNumber
		and (FM_IncludeCompanyNumber = 0 or CH_CompanyNumber = CompanyNumber)
		and (FM_IncludeRef1 = 0 or ISNULL(CH_Reference1, '') = ISNULL(TX_Reference1, ''))
		and (FM_IncludeRef2 = 0 or ISNULL(CH_Reference2, '') = ISNULL(TX_Reference2, ''))
		and (FM_IncludeRef3 = 0 or  ISNULL(CH_Reference3, '') = ISNULL(TX_Reference3, ''))
		and (FM_IncludeRef4 = 0 or  ISNULL(CH_Reference4, '') = ISNULL(TX_Reference4, ''))
		and (FM_IncludeRef5 = 0 or  ISNULL(CH_Reference5, '') = ISNULL(TX_Reference5, ''))
	where TX_Period = @Period
		and ProcessingStatus = 0
		and TX_ServiceOccuredUTC < DATEADD(day, FM_Days, CH_ServiceOccuredUtc)
		and CH_ServiceOccuredUtc < DATEADD(day, FM_Days, TX_ServiceOccuredUTC)
		option (recompile);

	set @searchPeriod = [edi].[GetNextPeriod](@searchPeriod);
end;

-- Process all messages remaining in staging with ProcessingStatus = 0.
-- The earliest in each job must be a first message.
-- Subsequent messages within FM_Days must not be a first message.
-- Later messages may also be a first message if the time gap exceeds FM_Days, and need an iteration of the loop.
while EXISTS(select top 1 1
	from edi.StagingBatch
	join edi.ConfigFirstMessage on TX_Category = FM_Category and TX_PriceItemCode = FM_PriceItemCode
	where TX_Period = @Period and ProcessingStatus = 0 and DatabaseNumber != 0)
begin
	with cte as
	(
		select ProcessingStatus, TX_ServiceOccuredUTC, FM_Days,
			rownum = ROW_NUMBER()
								OVER (PARTITION BY TX_Category, TX_PriceItemCode, DatabaseNumber, 
								case when FM_IncludeCompanyNumber = 0 then 0 else CompanyNumber end, 
								TX_Reference1, 
								case when FM_IncludeRef1 <> 0 then TX_Reference1 else '' end,
								case when FM_IncludeRef2 <> 0 then TX_Reference2 else '' end,
								case when FM_IncludeRef3 <> 0 then TX_Reference3 else '' end,
								case when FM_IncludeRef4 <> 0 then TX_Reference4 else '' end,
								case when FM_IncludeRef5 <> 0 then TX_Reference5 else '' end
								ORDER BY TX_ServiceOccuredUTC),
			timeOfFirstMessage = FIRST_VALUE(TX_ServiceOccuredUTC)
								OVER (PARTITION BY TX_Category, TX_PriceItemCode, DatabaseNumber, 
								case when FM_IncludeCompanyNumber = 0 then 0 else CompanyNumber end,
								TX_Reference1, 
								case when FM_IncludeRef1 <> 0 then TX_Reference1 else '' end,
								case when FM_IncludeRef2 <> 0 then TX_Reference2 else '' end,
								case when FM_IncludeRef3 <> 0 then TX_Reference3 else '' end,
								case when FM_IncludeRef4 <> 0 then TX_Reference4 else '' end,
								case when FM_IncludeRef5 <> 0 then TX_Reference5 else '' end
								ORDER BY TX_ServiceOccuredUTC)
		from edi.StagingBatch
		join edi.ConfigFirstMessage on TX_Category = FM_Category and TX_PriceItemCode = FM_PriceItemCode
		where TX_Period = @Period and ProcessingStatus = 0 and DatabaseNumber != 0
	)
	update cte
	set ProcessingStatus = case 
			when rownum = 1 then 1
			when rownum != 1 and TX_ServiceOccuredUTC < DATEADD(day, FM_Days, timeOfFirstMessage) then 255
			else 0 end;
end;

return 0;

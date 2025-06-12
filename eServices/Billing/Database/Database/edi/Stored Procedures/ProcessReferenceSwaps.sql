CREATE PROCEDURE [edi].[ProcessReferenceSwaps]
	@Period int
AS
-- we want the reference fields in a consistent order, such as key fields coming first
update edi.StagingBatch
set
	TX_Reference1 = case RefIndex1 when 1 then TX_Reference1 when 2 then TX_Reference2 when 3 then TX_Reference3 when 4 then TX_Reference4 else TX_Reference5 end,
	TX_Reference2 = case RefIndex2 when 1 then TX_Reference1 when 2 then TX_Reference2 when 3 then TX_Reference3 when 4 then TX_Reference4 else TX_Reference5 end,
	TX_Reference3 = case RefIndex3 when 1 then TX_Reference1 when 2 then TX_Reference2 when 3 then TX_Reference3 when 4 then TX_Reference4 else TX_Reference5 end,
	TX_Reference4 = case RefIndex4 when 1 then TX_Reference1 when 2 then TX_Reference2 when 3 then TX_Reference3 when 4 then TX_Reference4 else TX_Reference5 end,
	TX_Reference5 = case RefIndex5 when 1 then TX_Reference1 when 2 then TX_Reference2 when 3 then TX_Reference3 when 4 then TX_Reference4 else TX_Reference5 end,
    IsRefSwapped = 1
FROM edi.StagingBatch
join edi.ConfigReferenceSwap on
	TX_Period = @Period
	and TX_Category = Category
	and TX_PriceItemCode = PriceItemCode
	and TX_Version >= FirstVersion
	and TX_Version <= ISNULL(LastVersion, TX_Version)
	and IsRefSwapped != 1

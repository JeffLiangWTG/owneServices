CREATE FUNCTION [edi].[GetNextPeriod]
(
	@Period int
)
RETURNS INT
AS
BEGIN
	RETURN edi.IndexToPeriod(edi.PeriodToIndex(@Period) + 1);
END

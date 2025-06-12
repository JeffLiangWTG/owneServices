CREATE FUNCTION [edi].[GetPreviousPeriod]
(
	@Period int
)
RETURNS INT
AS
BEGIN
	RETURN edi.IndexToPeriod(edi.PeriodToIndex(@Period) - 1);
END

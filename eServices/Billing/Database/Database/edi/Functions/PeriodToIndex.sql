CREATE FUNCTION [edi].[PeriodToIndex]
(
	@period int
)
RETURNS INT
AS
BEGIN
	RETURN (@period / 100) * 12 + ((@period - 1) % 100);
END

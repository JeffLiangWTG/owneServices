CREATE FUNCTION [edi].[IndexToPeriod]
(
	@index int
)
RETURNS INT
AS
BEGIN
	RETURN (@index / 12) * 100 + (@index % 12) + 1;
END

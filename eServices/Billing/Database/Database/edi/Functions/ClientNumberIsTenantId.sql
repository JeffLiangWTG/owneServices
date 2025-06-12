CREATE FUNCTION [edi].[ClientNumberIsTenantId]
(
	@category varchar(MAX)
)
RETURNS int
AS
BEGIN
	RETURN CASE WHEN @category in ('SMF', 'SHP') THEN 1 ELSE 0 END;
END

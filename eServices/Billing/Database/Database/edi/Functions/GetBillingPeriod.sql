CREATE FUNCTION [edi].[GetBillingPeriod]
(
	@UtcDateTime datetime2
)
RETURNS TABLE WITH SCHEMABINDING
AS
RETURN
	with t as (select sydTime = Value from dbo.AuSydTime(@UtcDateTime))
	SELECT Value = 
		-- Feb 2016 was the transition month for UTC to Sydney time billing. It began in UTC and ended in Sydney time
		case when @UtcDateTime < '2016-2-29 13:00' 
			then YEAR(@UtcDateTime) * 100 + MONTH(@UtcDateTime)
			else YEAR(sydTime) * 100 + MONTH(sydTime) end from t
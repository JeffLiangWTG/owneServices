CREATE FUNCTION [dbo].[AuSydTime]
(
	@UtcDateTime datetime2
)
RETURNS TABLE WITH SCHEMABINDING
AS
RETURN
	SELECT Value =
		DATEADD(
			hour,
			CASE
				WHEN
					@UtcDateTime >= DATEADD(hour, 15, CONVERT(DATETIME2, DATEADD(day, ((8 - DATEPART(weekday, DATEFROMPARTS(year(@UtcDateTime), 4, 1))) % 7) - 1, DATEFROMPARTS(year(@UtcDateTime), 4, 1))))
					AND @UtcDateTime < DATEADD(hour, 16, CONVERT(DATETIME2, DATEADD(day, ((8 - DATEPART(weekday, DATEFROMPARTS(year(@UtcDateTime), 10, 1))) % 7) - 1, DATEFROMPARTS(year(@UtcDateTime), 10, 1))))
					THEN 10
				ELSE 11
			END,
			@UtcDateTime
		);

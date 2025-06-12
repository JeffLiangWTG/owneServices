CREATE FUNCTION [edi].[GetConfigFirstMessageWithPeriod]
(
	@Period int
)
RETURNS TABLE WITH SCHEMABINDING
AS
RETURN
(
	SELECT
		CFM.FM_Category AS [FM_Category],
		CFM.FM_PriceItemCode AS [FM_PriceItemCode],
		CFM.FM_Days AS [FM_Days],
		CFM.FM_IncludeRef1 AS [FM_IncludeRef1],
		CFM.FM_IncludeRef2 AS [FM_IncludeRef2],
		CFM.FM_IncludeRef3 AS [FM_IncludeRef3],
		CFM.FM_IncludeRef4 AS [FM_IncludeRef4],
		CFM.FM_IncludeRef5 AS [FM_IncludeRef5],
		CFM.FM_IncludeCompanyNumber AS [FM_IncludeCompanyNumber],
		CAFromToPeriod.FromPeriod AS [FM_FromPeriod],
		CAFromToPeriod.ToPeriod AS [FM_ToPeriod]
	FROM
		edi.ConfigFirstMessage CFM
	CROSS APPLY
		(
			SELECT DATEFROMPARTS(@Period / 100, @Period % 100, 1) AS StartDate
		) AS CAPeriod
	CROSS APPLY
		(
			SELECT DATEADD(DAY, -CFM.FM_Days, CAPeriod.StartDate) AS FromDate, 
			DATEADD(MONTH, 1, CAPeriod.StartDate) AS ToDate
		) AS CAFromToDate
	CROSS APPLY
		(
			SELECT YEAR(CAFromToDate.FromDate) * 100 + MONTH(CAFromToDate.FromDate) AS FromPeriod,
			YEAR(CAFromToDate.ToDate) * 100 + MONTH(CAFromToDate.ToDate) AS ToPeriod
		) AS CAFromToPeriod
);

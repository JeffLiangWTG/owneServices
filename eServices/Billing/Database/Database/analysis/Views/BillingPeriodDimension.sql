--For Multidimensional Billing Cube
CREATE VIEW [analysis].[BillingPeriodDimension]

AS
	SELECT
		d.BillingPeriod,
		d.MonthNumber,
		d.FullMonthName,
		d.YearNumber,
		d.MonthDate,
		d.QuarterDate,
		d.YearDate,
		d.[Month],
		d.[Quarter],
		d.[Year],
		d.FiscalMonthDate,
		d.FiscalQuarterDate,
		d.FiscalYearDate,
		d.FiscalMonth,
		d.FiscalQuarter,
		d.FiscalYear,
		d.SortOrder
		
	FROM
		[analysis].[GetBillingPeriodDimension] (2010, 2025) AS d
	

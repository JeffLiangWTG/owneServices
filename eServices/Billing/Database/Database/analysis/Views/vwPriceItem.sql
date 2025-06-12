--For Multidimensional Billing Cube
CREATE VIEW [analysis].[vwPriceItem]

AS

SELECT 
	Category + '.' + PriceItemCode AS PriceItemKey,
	Category,
	PriceItemCode,
	[Role],
	Module,
	[Function],
	Feature,
	UnitOfMeasure,
	IsSemiAggregate,
	'(' + PriceItemCode + ') ' + Feature AS CodeFeature
FROM [analysis].[PriceItem]

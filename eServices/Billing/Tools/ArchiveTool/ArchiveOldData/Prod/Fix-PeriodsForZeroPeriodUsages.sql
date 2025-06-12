USE [Billing]
GO

BEGIN TRAN

;WITH T AS
(
	SELECT US_Period, Value AS CorrectPeriod
	  FROM [edi].[Usage]
	  CROSS APPLY edi.GetBillingPeriod(US_ServiceOccuredUTC)
	  WHERE US_Period = 0
)
UPDATE T
SET US_Period = CorrectPeriod

SELECT * 
  FROM [edi].[Usage]
  WHERE US_Period = 0

ROLLBACK
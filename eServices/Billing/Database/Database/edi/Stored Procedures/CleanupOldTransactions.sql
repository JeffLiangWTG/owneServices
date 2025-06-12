CREATE PROCEDURE [edi].[CleanupOldTransactions] AS
BEGIN
	DECLARE @sixMonthsAgo datetime = DateADD(month, -6, Getdate());
	DECLARE @cutOffPeriod int = YEAR(@sixMonthsAgo) * 100 + MONTH(@sixMonthsAgo);

	DELETE FROM [edi].[Chargeable] WHERE CH_Period < @cutOffPeriod
	DELETE FROM [edi].[Usage] WHERE US_Period < @cutOffPeriod
END

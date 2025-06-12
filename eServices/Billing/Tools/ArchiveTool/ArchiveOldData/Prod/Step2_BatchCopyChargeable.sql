USE [Billing2017]

-- Disable constraints etc if there is any
ALTER TABLE [edi].[Chargeable] NOCHECK CONSTRAINT ALL;
DISABLE TRIGGER ALL ON [edi].[Chargeable];
GO

-- Create table TempProgressForBatchCopyChargeable
CREATE TABLE TempProgressForBatchCopyChargeable
(
	ID int PRIMARY KEY IDENTITY(1,1),
	[Period] int,
    [NoOfRowsInBatch] int,
    [Count] bigint,
	CompleteUtc datetime2
);

-- Batch copy
DECLARE @partitionFromPeriod int = 201701
DECLARE @partitionToPeriod int = 201801
DECLARE @minPeriod int
DECLARE @maxPeriod int
DECLARE @period int
DECLARE @count int
DECLARE @total int

IF @@TRANCOUNT > 0
BEGIN
    ROLLBACK TRANSACTION;
END

SELECT @minPeriod= MIN(CH_Period), @maxPeriod = MAX(CH_Period)
  FROM [Billing].[edi].[Chargeable]
  WHERE (@partitionFromPeriod IS NULL OR @partitionFromPeriod <= CH_Period)
	AND CH_Period < @partitionToPeriod

SELECT @period = ISNULL(MAX([Period]), @minPeriod)
  FROM TempProgressForBatchCopyChargeable

WHILE @period <= @maxPeriod
BEGIN
	SELECT @total = COUNT(*)
	  FROM [Billing].[edi].[Chargeable]
	  WHERE CH_Period = @period

	SELECT @count = ISNULL(MAX([Count]), 0)
	  FROM TempProgressForBatchCopyChargeable
	  WHERE [Period] = @period

	WHILE @count < @total
	BEGIN
		
		BEGIN TRAN

		INSERT INTO [edi].[Chargeable] WITH (TABLOCKX)
		SELECT *
		FROM [Billing].[edi].[Chargeable]
		WHERE CH_Period = @period
		ORDER BY [CH_Category],
			[CH_PriceItemCode],
			[CH_DatabaseNumber],
			[CH_CompanyNumber],
			[CH_Reference1],
			[CH_Reference2],
			[CH_Reference3],
			[CH_Reference4],
			[CH_Reference5],
			[CH_ServiceOccuredUtc],
			[CH_ReportingSource],
			[CH_ClientStaffCode],
			[CH_MessageTrackingID]
		OFFSET @count ROWS
		FETCH NEXT 50000 ROWS ONLY

		INSERT TempProgressForBatchCopyChargeable
		SELECT @period, @@ROWCOUNT, @count + @@ROWCOUNT, sysutcdatetime()

		COMMIT TRAN
            
		SET @count = @count + 50000
	END

	SET @period = edi.IndexToPeriod((edi.PeriodToIndex(@period) + 1))
END

-- Drop table TempProgressForBatchCopyChargeable
--DROP TABLE TempProgressForBatchCopyChargeable

-- Enable constraints etc
ALTER TABLE [edi].[Chargeable] CHECK CONSTRAINT ALL;
ENABLE TRIGGER ALL ON [edi].[Chargeable];
GO

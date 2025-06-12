USE [Billing2017]

-- Disable constraints etc if there is any
ALTER TABLE [edi].[Usage] NOCHECK CONSTRAINT ALL;
DISABLE TRIGGER ALL ON [edi].[Usage];
GO

-- Create table TempProgressForBatchCopyUsage
CREATE TABLE TempProgressForBatchCopyUsage
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

SELECT @minPeriod= MIN(US_Period), @maxPeriod = MAX(US_Period)
  FROM [Billing].[edi].[Usage]
  WHERE (@partitionFromPeriod IS NULL OR @partitionFromPeriod <= US_Period)
	AND US_Period < @partitionToPeriod

SELECT @period = ISNULL(MAX([Period]), @minPeriod)
  FROM TempProgressForBatchCopyUsage

WHILE @period <= @maxPeriod
BEGIN
	SELECT @total = COUNT(*)
	  FROM [Billing].[edi].[Usage]
	  WHERE US_Period = @period

	SELECT @count = ISNULL(MAX([Count]), 0)
	  FROM TempProgressForBatchCopyUsage
	  WHERE [Period] = @period

	WHILE @count < @total
	BEGIN
		
		BEGIN TRAN

		INSERT INTO [edi].[Usage] WITH (TABLOCKX)
		SELECT *
		FROM [Billing].[edi].[Usage]
		WHERE US_Period = @period
		ORDER BY [US_Category],
			[US_PriceItemCode],
			[US_DatabaseNumber],
			[US_CompanyNumber],
			[US_Reference1],
			[US_Reference2],
			[US_Reference3],
			[US_Reference4],
			[US_Reference5],
			[US_ServiceOccuredUtc],
			[US_ReportingSource],
			[US_ClientNumber],
			[US_ClientID],
			[US_ClientStaffCode],
			[US_MessageTrackingID]
		OFFSET @count ROWS
		FETCH NEXT 50000 ROWS ONLY

		INSERT TempProgressForBatchCopyUsage
		SELECT @period, @@ROWCOUNT, @count + @@ROWCOUNT, sysutcdatetime()

		COMMIT TRAN
            
		SET @count = @count + 50000
	END

	SET @period = edi.IndexToPeriod((edi.PeriodToIndex(@period) + 1))
END

-- Drop table TempProgressForBatchCopyUsage
--DROP TABLE TempProgressForBatchCopyUsage

-- Enable constraints etc
ALTER TABLE [edi].[Usage] CHECK CONSTRAINT ALL;
ENABLE TRIGGER ALL ON [edi].[Usage];
GO

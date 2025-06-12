CREATE PROCEDURE [edi].[ProcessStaging]
	@allowTransaction bit = 0, @isTestServer bit = 0
AS
SET NOCOUNT ON;

IF (@allowTransaction = 0 and @@trancount != 0)
BEGIN
	RAISERROR ('Connection must NOT be in a transaction or inserts to Staging will be blocked during processing.', 16, 1);
	RETURN -1;
END;


-- prevent concurrency
declare @gotLock int;
EXEC @gotLock = edi.LockStagingForSession;

BEGIN TRY

	IF NOT EXISTS(select top 1 1 from edi.StagingBatch)
	BEGIN
		IF EXISTS (SELECT 1 FROM sys.indexes  WHERE name='IX_StagingBatch_Processing' 
				AND object_id = OBJECT_ID('edi.StagingBatch'))
		begin
			DROP INDEX IX_StagingBatch_Processing ON edi.StagingBatch;
		end

		alter table Staging switch to edi.StagingBatch;

		INSERT INTO [edi].[StagingBatch]
           ([TX_ID]
           ,[TX_Category]
           ,[TX_PriceItemCode]
           ,[TX_BillableCount]
           ,[TX_ReportingSource]
           ,[TX_ServiceOccuredUTC]
           ,[TX_ClientID]
           ,[TX_ClientNumber]
           ,[TX_ClientStaffCode]
           ,[TX_Reference1]
           ,[TX_Reference2]
           ,[TX_Reference3]
           ,[TX_Reference4]
           ,[TX_Reference5]
           ,[TX_SystemCreateUTC]
           ,[TX_Version]
           ,[TX_Branch]
           ,[TX_MessageTrackingID])

		   SELECT [TX_ID]
			  ,[TX_Category]
			  ,[TX_PriceItemCode]
			  ,[TX_BillableCount]
			  ,[TX_ReportingSource]
			  ,[TX_ServiceOccuredUTC]
			  ,[TX_ClientID]
			  ,[TX_ClientNumber]
			  ,[TX_ClientStaffCode]
			  ,[TX_Reference1]
			  ,[TX_Reference2]
			  ,[TX_Reference3]
			  ,[TX_Reference4]
			  ,[TX_Reference5]
			  ,[TX_SystemCreateUTC]
			  ,[TX_Version]
			  ,[TX_Branch]
			  ,[TX_MessageTrackingID]
		 FROM [edi].[StagingUnknownSystems];
	END

	BEGIN TRAN;
		select top 0 NULL from edi.StagingBatch WITH (TABLOCKX);

		--
		-- Processing to be done before applying index - i.e., that affects index columns
		--

		-- Delete internal usage
		IF @isTestServer = 0
			delete from edi.StagingBatch where LEFT(TX_ClientID, 3) in ('EDI', 'HYE', 'WTL', 'EHW') and LEN(TX_ClientID) = 9;

		BEGIN
			RAISERROR ('Starting process fixes', 10, 1) WITH NOWAIT
			exec edi.ProcessFixes with recompile;
			RAISERROR ('Process fixes completed', 10, 1) WITH NOWAIT
		END

		--Synchronise Fake Licences for test billing database
		IF @isTestServer = 1
			BEGIN
				RAISERROR ('Starting sync fake licences for testing', 10, 1) WITH NOWAIT
				exec edi.SyncFakeLicencesForTesting with recompile;
				RAISERROR ('Sync fake licences for testing completed', 10, 1) WITH NOWAIT
			END

		BEGIN
			RAISERROR ('Starting process client number', 10, 1) WITH NOWAIT
			exec edi.ProcessClientNumber @isTestServer with recompile;
			RAISERROR ('Process client number completed', 10, 1) WITH NOWAIT
		END

		update edi.StagingBatch set TX_Period = Value from edi.StagingBatch cross apply edi.GetBillingPeriod(TX_ServiceOccuredUTC) where TX_Period = 0

		--
		-- Apply index
		--
		IF NOT EXISTS (SELECT 1 FROM sys.indexes  WHERE name='IX_StagingBatch_Processing'
				AND object_id = OBJECT_ID('edi.StagingBatch'))
		begin
			-- partioning and ordering in the "remove duplicates" section below should match these clustered index columns
			create clustered index IX_StagingBatch_Processing on edi.StagingBatch (
				TX_Period, TX_Category, TX_PriceItemCode, DatabaseNumber, CompanyNumber, TX_Reference1, TX_Reference2, TX_Reference3, TX_Reference4, TX_Reference5, TX_ServiceOccuredUTC, TX_ReportingSource, TX_ClientID, TX_ClientNumber, TX_ClientStaffCode, TX_MessageTrackingID);

			-- remove duplicates within the batch
			with cte as 
			(	select ndupe = ROW_NUMBER() OVER (
				PARTITION BY 
				TX_Period, TX_Category, TX_PriceItemCode, DatabaseNumber, CompanyNumber, TX_Reference1, TX_Reference2, TX_Reference3, TX_Reference4, TX_Reference5, TX_ServiceOccuredUTC, TX_ReportingSource, TX_ClientID, TX_ClientNumber, TX_ClientStaffCode, TX_MessageTrackingID
				ORDER BY
				TX_Period, TX_Category, TX_PriceItemCode, DatabaseNumber, CompanyNumber, TX_Reference1, TX_Reference2, TX_Reference3, TX_Reference4, TX_Reference5, TX_ServiceOccuredUTC, TX_ReportingSource, TX_ClientID, TX_ClientNumber, TX_ClientStaffCode, TX_MessageTrackingID)
				from edi.StagingBatch
			)
			delete cte where ndupe > 1;
		end

		declare @periods table (Period int not null)
		insert @periods(Period) select distinct TX_Period from edi.StagingBatch

		while exists(select top 1 1 from @periods)
		begin
			-- Process one period at a time since the data is all clustered by period
			BEGIN
				declare @period int = (select top 1 Period from @periods order by Period);
				PRINT 'Period ' + CAST(@period AS VARCHAR(MAX));
			END

			delete from @periods where Period = @period;

			--
			-- Procesing to be done after applying index.
			--

			-- Interface connector usage is not needed and takes a lot of space
			delete from edi.StagingBatch where TX_Period = @period and TX_Category = 'ICN';
			-- also AI3 (CoreFinance3rdPartyInterface)
			delete from edi.StagingBatch where TX_Period = @period and TX_Category = 'STL' and TX_PriceItemCode = 'AI3';

			-- Land transport KG moved numbers are impossibly high and cause arithmetic overflow
			-- ProcessSimpleChargable will put records with ProcessingStatus = 255 straight into the Usage table
			update edi.StagingBatch set ProcessingStatus = 255 where TX_Period = @period and TX_Category = 'STL' and TX_PriceItemCode = 'LTK';

			-- Shipping instruction - remove records where TX_ClientID = TX_Reference2
			delete from edi.StagingBatch where TX_Period = @period and TX_Category = 'SHI' and TX_PriceItemCode = 'SHI' and TX_ClientID = TX_Reference2;

			BEGIN
				RAISERROR ('Starting process SPM', 10, 1) WITH NOWAIT;
				exec edi.ProcessSPM @period with recompile;
				RAISERROR ('Process SPM completed', 10, 1) WITH NOWAIT;
			END

			BEGIN
				RAISERROR ('Starting process CMP', 10, 1) WITH NOWAIT;
				exec edi.ProcessCMP @period with recompile;
				RAISERROR ('Process CMP completed', 10, 1) WITH NOWAIT;
			END

			BEGIN
				RAISERROR ('Starting process reference swaps', 10, 1) WITH NOWAIT;
				exec edi.ProcessReferenceSwaps @period with recompile;
				RAISERROR ('Process reference swaps completed', 10, 1) WITH NOWAIT;
			END

			BEGIN
				RAISERROR ('Starting process EAD', 10, 1) WITH NOWAIT;
				exec edi.ProcessEAD @period with recompile;
				RAISERROR ('Process EAD completed', 10, 1) WITH NOWAIT;
			END

			BEGIN
				RAISERROR ('Starting process usage data', 10, 1) WITH NOWAIT;
				exec edi.ProcessUsageData @period, @isTestServer with recompile;
				RAISERROR ('Process usage data completed', 10, 1) WITH NOWAIT;
			END

			BEGIN
				RAISERROR ('Starting process self hosted usage data', 10, 1) WITH NOWAIT;
				exec edi.ProcessSelfHostedUsageData @period with recompile;
				RAISERROR ('Process self hosted usage data completed', 10, 1) WITH NOWAIT;
			END

			BEGIN
				RAISERROR ('Starting process Internal Licences', 10, 1) WITH NOWAIT;
				exec edi.ProcessInternalLicences @period with recompile;
				RAISERROR ('Process Internal Licences completed', 10, 1) WITH NOWAIT;
			END

			BEGIN
				RAISERROR ('Starting process first message', 10, 1) WITH NOWAIT;
			exec edi.ProcessFirstMessage @period with recompile;
				RAISERROR ('Process first message completed', 10, 1) WITH NOWAIT;
			END

			BEGIN
				RAISERROR ('Starting process simple chargeable', 10, 1) WITH NOWAIT;
				exec edi.ProcessSimpleChargeable @period, @isTestServer with recompile;
				RAISERROR ('Process simple chargeable completed', 10, 1) WITH NOWAIT;
			END
		end;

		truncate table edi.StagingBatch;
	COMMIT;

	EXEC edi.UnlockStagingForSession;
END TRY
BEGIN CATCH
	if @@TRANCOUNT > 0
		ROLLBACK;

	EXEC edi.UnlockStagingForSession;

	THROW;
END CATCH
		
RETURN 0


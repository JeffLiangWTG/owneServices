/*
 Slide the PF_Period partition function forward one month.
 Assumes the edi.Chargeable and edi.Usage tables use this partitioning.
 Assumes the partitioning consists of:
	A partition per month for the current month and the previous 11 months.
	A partition per year for the previous 5 calendar years.
	Note, the partition for last year will only contain those months more than 11 months ago.
	An implicit partition for all data occuring more than 5 calendar years ago.

- The month that drops out is switched to a staging table on the same filegroup so the partition is empty.
- If the month is not January it is merged with the previous partition, which will be the yearly one
	Note: switching data out, merging with an empty partition and inserting data back
	seems to be much faster than just merging a non-empty partition
- If the month is January we need to move the years forward
	- The year that drops out is switched to a staging table on the same filegroup so the partition is empty
	- Partition is merged
	- Staging data moved so it is now in the archive file group
	- The month that is dropping out needs to become a yearly partition on the freed up yearly file group
		- Merge the month (temporarily with the previous year so it becomes a 13 month range)
		- Set NEXT USED to the newly empty year filegroup
		- Split the range by adding back the month - the month is now a year on the correct filegroup
- Staging month is moved back in (expensive data transfer)
- The monthly filegroup is now empty
- Set NEXT USED as the empty month
- Split the range by inserting the new month
- If there was any data from the future it will now incur an expensive filegroup transfer
*/

CREATE PROCEDURE edi.PeriodPartitionSlide
	@allowFutureMonthsForTesting bit = 0
AS
BEGIN

SET NOCOUNT ON;

declare @gotLock int;
EXEC @gotLock = edi.LockStagingForSession @LockTimeoutMs = 30000;

BEGIN TRY

	DECLARE @initialTranCount int
	SET @initialTranCount = @@TRANCOUNT
	IF @initialTranCount = 0
		BEGIN TRAN;

	-- Acquires exclusive table lock to prevent deadlocking with concurrent activity.
	EXEC(N'
		DECLARE @Dummy bit;
		SET LOCK_TIMEOUT 3000;
		SET @Dummy = (SELECT TOP 0 null FROM edi.Chargeable WITH (TABLOCKX, HOLDLOCK));
		SET @Dummy = (SELECT TOP 0 null FROM edi.Usage WITH (TABLOCKX, HOLDLOCK));
	');

	declare @functionId int = (select function_id FROM sys.partition_functions where name = 'PF_Period');
	declare @latestMonthPeriod int = (SELECT MAX(CAST(value AS int)) from sys.partition_range_values where function_id = @functionId);

	if @allowFutureMonthsForTesting = 0
	begin
		-- allow sliding 24 hours before the end of the month in Sydney standard time
		declare @sydneyNow smalldatetime = DATEADD(HOUR, 10, sysutcdatetime());
		declare @slideTime smalldatetime = DATEADD(HOUR, 24, @sydneyNow);
		declare @periodSlide int = YEAR(@slideTime) * 100 + MONTH(@slideTime);
		if @latestMonthPeriod >= @periodSlide
		begin
			;THROW 50000, 'Last partition range is in the future - further sliding is not allowed.', 1;
		end;
	end;

	declare @latestMonthIndex int = edi.PeriodToIndex(@latestMonthPeriod);
	declare @monthPeriodToRemove int = edi.IndexToPeriod(@latestMonthIndex - 11);
	declare @monthPartionNumberToRemove int = (SELECT boundary_id + 1 from sys.partition_range_values where function_id = @functionId and value = @monthPeriodToRemove)

	DECLARE @monthToRemoveFileGroup sysname =
		(SELECT distinct fg.name FROM sys.partitions p INNER JOIN sys.allocation_units au ON au.container_id = p.hobt_id INNER JOIN sys.filegroups fg ON fg.data_space_id = au.data_space_id
		WHERE p.object_id = OBJECT_ID('edi.Chargeable') and p.partition_number = @monthPartionNumberToRemove);

	-- dynamically create tables to hold the sliding month partition
	select top 0 * into edi.SlidingChargeableMonth from edi.Chargeable;
	select top 0 * into edi.SlidingUsageMonth from edi.Usage;

	-- dynamically create clustered indices on the correct filegroup
	DECLARE @sql nvarchar(max);
	set @sql = edi.GetCreateClusteredIndexSql('edi', 'Chargeable', @monthPartionNumberToRemove, 'SlidingChargeableMonth') + ' ON ' + @monthToRemoveFileGroup;
	exec sp_executesql @sql;
	set @sql = edi.GetCreateClusteredIndexSql('edi', 'Usage', @monthPartionNumberToRemove, 'SlidingUsageMonth') + ' ON ' + @monthToRemoveFileGroup;
	exec sp_executesql @sql;

	-- Switch data into the sliding tables
	alter table edi.Chargeable switch partition @monthPartionNumberToRemove to edi.SlidingChargeableMonth;
	alter table edi.Usage switch partition @monthPartionNumberToRemove to edi.SlidingUsageMonth;

	if (@monthPeriodToRemove % 100) != 1
	begin
		-- Not january so year partitions stay
		ALTER PARTITION FUNCTION PF_Period() MERGE RANGE(@monthPeriodToRemove)
	end
	else
	begin
		-- January - so slide the year too
		declare @yearPeriodToRemove int = @monthPeriodToRemove - 500;
		declare @yearPartionNumberToRemove int = (SELECT boundary_id + 1 from sys.partition_range_values where function_id = @functionId and value = @yearPeriodToRemove)
			-- $PARTITION syntax not supported by SSDT 2014
			-- (select $PARTITION.PF_Period(@yearPeriodToRemove));

		DECLARE @yearToRemoveFileGroup sysname = 
			(SELECT distinct fg.name FROM sys.partitions p INNER JOIN sys.allocation_units au ON au.container_id = p.hobt_id INNER JOIN sys.filegroups fg ON fg.data_space_id = au.data_space_id
			WHERE p.object_id = OBJECT_ID('edi.Chargeable') and p.partition_number = @yearPartionNumberToRemove);

		-- dynamically create tables to hold the sliding year partition
		select top 0 * into edi.SlidingChargeableYear from edi.Chargeable;
		select top 0 * into edi.SlidingUsageYear from edi.Usage;

		-- dynamically create clustered indices on the correct filegroup
		set @sql = edi.GetCreateClusteredIndexSql('edi', 'Chargeable', @yearPartionNumberToRemove, 'SlidingChargeableYear') + ' ON ' + @yearToRemoveFileGroup;
		exec sp_executesql @sql;
		set @sql = edi.GetCreateClusteredIndexSql('edi', 'Usage', @yearPartionNumberToRemove, 'SlidingUsageYear') + ' ON ' + @yearToRemoveFileGroup;
		exec sp_executesql @sql;

		-- Switch data into the sliding tables
		alter table edi.Chargeable switch partition @yearPartionNumberToRemove to edi.SlidingChargeableYear;
		alter table edi.Usage switch partition @yearPartionNumberToRemove to edi.SlidingUsageYear;

		-- remove the last year
		ALTER PARTITION FUNCTION PF_Period() MERGE RANGE(@yearPeriodToRemove)

		-- Move the month to the empty yearly filegroup
		-- merge the month (temporarily with the previous year so it becomes a 13 month range)
		ALTER PARTITION FUNCTION PF_Period() MERGE RANGE(@monthPeriodToRemove)
	
		-- set NEXT USED to the newly empty year filegroup
		set @sql = N'ALTER PARTITION SCHEME PS_period NEXT USED ' + @yearToRemoveFileGroup;
		exec sp_executesql @sql;

		-- split the range by adding back the month - the month is now a year on the correct filegroup
		ALTER PARTITION FUNCTION PF_Period() SPLIT RANGE(@monthPeriodToRemove);
	end;

	-- set NEXT USED as the empty month
	set @sql = N'ALTER PARTITION SCHEME PS_period NEXT USED ' + @monthToRemoveFileGroup;
	exec sp_executesql @sql;

	-- split the range by inserting the new month
	declare @monthPeriodToAdd int = edi.IndexToPeriod(@latestMonthIndex + 1);
	ALTER PARTITION FUNCTION PF_Period() SPLIT RANGE(@monthPeriodToAdd);

	-- insert staging month (expensive data move)
	insert edi.Chargeable select * from edi.SlidingChargeableMonth;
	insert edi.Usage select * from edi.SlidingUsageMonth;

	drop table edi.SlidingChargeableMonth;
	drop table edi.SlidingUsageMonth;

	-- insert staging year (expensive data move)
	if OBJECT_ID(N'edi.SlidingChargeableYear', N'U') is not null
	begin
		insert edi.Chargeable select * from edi.SlidingChargeableYear;
		insert edi.Usage select * from edi.SlidingUsageYear;

		drop table edi.SlidingChargeableYear;
		drop table edi.SlidingUsageYear;
	end

	IF (@initialTranCount = 0 AND @@TRANCOUNT > 0)
		COMMIT;

	EXEC edi.UnlockStagingForSession;
END TRY
BEGIN CATCH
	IF (@initialTranCount = 0 AND @@TRANCOUNT > 0)
		ROLLBACK;

	EXEC edi.UnlockStagingForSession;
	
	THROW;
END CATCH

END
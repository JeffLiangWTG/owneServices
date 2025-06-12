-- Called from ediProd service task periodically to do all updates.
-- sync database/company 
-- maintain partitions
-- process staging data
CREATE PROCEDURE edi.UpdateChargeable
 @utcNow DateTime = NULL, @firstPeriodOfNewCollection int = 202110, @performMonthlyAggregation bit = 1, @isTestServer bit = 0, @SyncLicences bit = 1, @ProcessStaging bit = 1, @UpdateBillingCube bit = 1
AS  
  
SET NOCOUNT ON;  
  
declare @gotLock int;  
EXEC @gotLock = edi.LockStagingForSession @LockTimeoutMs = 300000;  
  
if (@utcNow IS NULL)  
 set @utcNow = sysutcdatetime();  
  
BEGIN TRY  
  
 -- update partitions after 2am Sydney time on the last day of the month  
 declare @functionId int = (select function_id FROM sys.partition_functions where name = 'PF_Period');  
 declare @latestMonthPeriod int = (SELECT MAX(CAST(value AS int)) from sys.partition_range_values where function_id = @functionId);  
 declare @sydneyNow smalldatetime = DATEADD(HOUR, 10, @utcNow);  
 declare @slideTime smalldatetime = DATEADD(HOUR, 22, @sydneyNow);  
 declare @periodNow int = YEAR(@slideTime) * 100 + MONTH(@slideTime);  
 declare @sydneyLastMonth smalldatetime = DATEADD(MONTH, -1, @sydneyNow);  
 declare @previousPeriod int = YEAR(@sydneyLastMonth) * 100 + MONTH(@sydneyLastMonth);  
  
 --if @latestMonthPeriod < @periodNow  
 --begin  
 -- exec edi.PeriodPartitionSlide;  
 --end  
  
 -- sync database and company  
 if @isTestServer = 0 AND @SyncLicences = 1 AND exists(select top 1 1 from edi.LicenceDatabaseEdiProdCache)
  BEGIN
  RAISERROR ('Starting licence database sync', 10, 1) WITH NOWAIT;
  exec edi.LicenceDatabaseSync with recompile; 
  RAISERROR ('Licence database sync completed', 10, 1) WITH NOWAIT;
  END

 if @isTestServer = 0 AND @SyncLicences = 1 AND exists(select top 1 1 from edi.ClientCompanyEdiProdCache)
  BEGIN
  RAISERROR ('Starting client company sync', 10, 1) WITH NOWAIT;
  exec edi.ClientCompanySync with recompile;  
  RAISERROR ('Client company sync completed', 10, 1) WITH NOWAIT;
  END

-- process staging
 if @ProcessStaging = 1
  BEGIN
  RAISERROR ('Starting process staging', 10, 1) WITH NOWAIT;
  exec edi.ProcessStaging 0, @isTestServer with recompile;
  RAISERROR ('Process staging completed', 10, 1) WITH NOWAIT;
  END

 if @UpdateBillingCube = 1
  BEGIN
  RAISERROR ('Starting update billing cube', 10, 1) WITH NOWAIT;
  exec edi.UpdateBillingCube @isTestServer with recompile;
  RAISERROR ('Update billing cube completed', 10, 1) WITH NOWAIT;
  END

-- produce monthly aggregates for the previous month on the first day of the new month  
 if (@performMonthlyAggregation = 1 AND @isTestServer = 0 AND DAY(@sydneyNow) = 1)
  BEGIN
  RAISERROR ('Starting produce aggregate monthly chargeables', 10, 1) WITH NOWAIT;
  exec edi.ProduceAggregatedMonthlyChargeables @previousPeriod, @firstPeriodOfNewCollection with recompile;
  RAISERROR ('Produce aggregate monthly chargeables completed', 10, 1) WITH NOWAIT;
  END

-- update statistics if they are 4 hours old  
 if 4 <= DATEDIFF(HOUR, (SELECT MIN(STATS_DATE(object_id, stats_id)) FROM sys.stats WHERE object_id = OBJECT_ID('edi.Chargeable')), getdate()) 
 begin
  RAISERROR ('Starting statistics update', 10, 1) WITH NOWAIT;
  update statistics edi.Chargeable;  
  update statistics edi.Usage;
  RAISERROR ('Statistics update completed', 10, 1) WITH NOWAIT;
 end;

 EXEC edi.UnlockStagingForSession;  
  
END TRY  
BEGIN CATCH  
 EXEC edi.UnlockStagingForSession;  
  
 THROW;  
END CATCH  
  
RETURN 0  

/*
  Setup the partition scheme.
*/
CREATE PROCEDURE [edi].[PeriodPartitionSetupScheme]
AS
BEGIN TRY;
	DECLARE @initialTranCount int
	SET @initialTranCount = @@TRANCOUNT
	IF @initialTranCount = 0
		BEGIN TRAN;

	if exists(select top 1 1 from edi.Chargeable with (TABLOCKX)) or exists(select top 1 1 from edi.Usage WITH (TABLOCKX))
	begin
		;THROW 50000, 'Partitioned tables are not empty', 1;
	end;

	declare @foundUX_ChargeableData int;
	declare @foundUX_UsageData int;
	declare @totalPartitioned int;

	select 
		@foundUX_ChargeableData = ISNULL(MAX(case when i.name = 'UX_ChargeableData' and object_schema_name(i.object_id) = 'edi' and object_name(i.object_id) = 'Chargeable' then 1 else 0 end), 0),
		@foundUX_UsageData = ISNULL(MAX(case when i.name = 'UX_UsageData' and object_schema_name(i.object_id) = 'edi' and object_name(i.object_id) = 'Usage' then 1 else 0 end), 0),
		@totalPartitioned = count(*)
	from sys.indexes i 
	join sys.partition_schemes s 
	on i.data_space_id = s.data_space_id 
	where s.name = 'PS_Period'

	if @foundUX_ChargeableData + @foundUX_UsageData != @totalPartitioned
	begin
		;THROW 50000, 'PROCEDURE edi.PeriodPartitionSetupScheme - Unexpected index on PS_Period. Expected UX_ChargeableData, UX_UsageData', 1;
	end;

	-- Drop and re-create the indices without the partition scheme
	-- so we can drop and re-create the partition scheme
	declare @index1 nvarchar(max);
	declare @index2 nvarchar(max);
	set @index1 = edi.GetCreateClusteredIndexSql('edi', 'Chargeable', 1, 'Chargeable');
	set @index2 = edi.GetCreateClusteredIndexSql('edi', 'Usage', 1, 'Usage');

	drop index UX_ChargeableData on edi.Chargeable;
	drop index UX_UsageData on edi.Usage;

	declare @sql nvarchar(max);
	set @sql = @index1 + ' ON [PRIMARY]';
	exec sp_executesql @sql;

	set @sql = @index2 + ' ON [PRIMARY]';
	exec sp_executesql @sql;

	drop index UX_ChargeableData on edi.Chargeable;
	drop index UX_UsageData on edi.Usage;

	drop partition scheme PS_Period;

	declare @latestMonthPeriod int = 
		(SELECT MAX(CAST(prv.value AS int)) from sys.partition_range_values prv
		where prv.function_id = (select function_id FROM sys.partition_functions where name = 'PF_Period'));
	declare @date date = DATEFROMPARTS(@latestMonthPeriod / 100, @latestMonthPeriod % 100, 1);

	set @sql =
	'CREATE PARTITION SCHEME [PS_Period]
	AS PARTITION [PF_Period]
	TO ([YearArchive]' 
		+ ', [Year' + cast(((YEAR(@date) - 5) % 5) as varchar(2)) + ']' +
		+ ', [Year' + cast(((YEAR(@date) - 4) % 5) as varchar(2)) + ']' +
		+ ', [Year' + cast(((YEAR(@date) - 3) % 5) as varchar(2)) + ']' +
		+ ', [Year' + cast(((YEAR(@date) - 2) % 5) as varchar(2)) + ']' +
		+ ', [Year' + cast(((YEAR(@date) - 1) % 5) as varchar(2)) + ']' +
		+ ', [Month' + RIGHT('0' + cast(MONTH(DATEADD(MONTH, -11, @date)) as varchar(2)), 2) + ']' +
		+ ', [Month' + RIGHT('0' + cast(MONTH(DATEADD(MONTH, -10, @date)) as varchar(2)), 2) + ']' +
		+ ', [Month' + RIGHT('0' + cast(MONTH(DATEADD(MONTH,  -9, @date)) as varchar(2)), 2) + ']' +
		+ ', [Month' + RIGHT('0' + cast(MONTH(DATEADD(MONTH,  -8, @date)) as varchar(2)), 2) + ']' +
		+ ', [Month' + RIGHT('0' + cast(MONTH(DATEADD(MONTH,  -7, @date)) as varchar(2)), 2) + ']' +
		+ ', [Month' + RIGHT('0' + cast(MONTH(DATEADD(MONTH,  -6, @date)) as varchar(2)), 2) + ']' +
		+ ', [Month' + RIGHT('0' + cast(MONTH(DATEADD(MONTH,  -5, @date)) as varchar(2)), 2) + ']' +
		+ ', [Month' + RIGHT('0' + cast(MONTH(DATEADD(MONTH,  -4, @date)) as varchar(2)), 2) + ']' +
		+ ', [Month' + RIGHT('0' + cast(MONTH(DATEADD(MONTH,  -3, @date)) as varchar(2)), 2) + ']' +
		+ ', [Month' + RIGHT('0' + cast(MONTH(DATEADD(MONTH,  -2, @date)) as varchar(2)), 2) + ']' +
		+ ', [Month' + RIGHT('0' + cast(MONTH(DATEADD(MONTH,  -1, @date)) as varchar(2)), 2) + ']' +
		+ ', [Month' + RIGHT('0' + cast(MONTH(DATEADD(MONTH,   0, @date)) as varchar(2)), 2) + ']' +
		')'

	exec sp_executesql @sql;

	set @sql = @index1 + ' ON PS_Period(CH_Period)';
	exec sp_executesql @sql;

	set @sql = @index2 + ' ON PS_Period(US_Period)';
	exec sp_executesql @sql;

	COMMIT;
END TRY
BEGIN CATCH
	ROLLBACK;
	THROW;
END CATCH

RETURN 0

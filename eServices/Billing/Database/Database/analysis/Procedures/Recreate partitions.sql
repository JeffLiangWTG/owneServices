--This SP is being used in SQL Job that does Full Processing of Billing Cube
--It read the current cube partition cofiguration, drops all existing partitions and re-create them
--Linked server to Billing cube must be created before running this SP
CREATE PROCEDURE analysis.RecreateCubePartitions

AS

BEGIN

	SET NOCOUNT ON

	DECLARE @CubeName VARCHAR(100), @CatalogName VARCHAR(100), @InstanceName VARCHAR(100), @MeasureGroup VARCHAR(100), @LinkedServerName VARCHAR(100)
	DECLARE @GetPartitionsScript NVARCHAR(MAX), @DeletePartitionTemplate NVARCHAR(MAX), @DeletePartitionScript NVARCHAR(MAX), @CreatePartitionTemplate NVARCHAR(MAX), @CreatePartitionScript NVARCHAR(MAX)

	SET @CubeName = 'BillingCube'
	SET @InstanceName = 'SYDCO-WANV-1\MSAS2016MD'
	SET @LinkedServerName = '[SYDCO-WANV-1\MSAS2016MD]'
	SET @CatalogName = 'BillingSemanticModelMD'
	SET @MeasureGroup = 'Billing Transactions'

	SET @GetPartitionsScript = N'N''

	SELECT Object_ID
	FROM $system.DISCOVER_OBJECT_MEMORY_USAGE
	WHERE OBJECT_TYPE_ID = 100021 AND
		OBJECT_PARENT_PATH = ''''' + @InstanceName + '.Databases.' + @CatalogName + '.Cubes.' + @CubeName + '.Measure Groups.' + @MeasureGroup + '.Partitions''''
	''
	'

	SET @GetPartitionsScript = REPLACE(REPLACE('
	SELECT Object_ID
	FROM OPENQUERY(<ServerName>,<Query>)', '<ServerName>', @LinkedServerName), '<Query>', @GetPartitionsScript)

	CREATE TABLE #Partitions (IDC INT IDENTITY(1,1) PRIMARY KEY, PartitionName VARCHAR(250))
	INSERT INTO #Partitions (PartitionName)
	EXEC(@GetPartitionsScript)

	SET @DeletePartitionTemplate = '
	<Delete xmlns="http://schemas.microsoft.com/analysisservices/2003/engine">
	  <Object>
		<DatabaseID><Catalog></DatabaseID>
		<CubeID><Cube></CubeID>
		<MeasureGroupID><MeasureGroup></MeasureGroupID>
		<PartitionID><PartitionName></PartitionID>
	  </Object>
	</Delete>'

	SET @DeletePartitionTemplate = REPLACE(REPLACE(REPLACE(@DeletePartitionTemplate, '<Catalog>', @CatalogName), '<Cube>', @CubeName), '<MeasureGroup>', @MeasureGroup)

	DECLARE @Counter INT = 1, @MaxRows INT, @PartitionName VARCHAR(100)
	SELECT @MaxRows = COUNT(*) FROM #Partitions

	WHILE @Counter <= @MaxRows
	BEGIN
		SELECT @PartitionName = PartitionName FROM #Partitions WHERE IDC = @Counter

		SET @DeletePartitionScript = REPLACE(@DeletePartitionTemplate, '<PartitionName>', @PartitionName)
		SET @DeletePartitionScript = REPLACE('EXEC(''<Script>'') AT ' + @LinkedServerName,'<Script>', @DeletePartitionScript)	

		EXEC (@DeletePartitionScript)

		SET @Counter = @Counter + 1

	END

	DROP TABLE #Partitions 

	SET @CreatePartitionTemplate = N'
	<Create xmlns="http://schemas.microsoft.com/analysisservices/2003/engine">
		<ParentObject>
			<DatabaseID><Catalog></DatabaseID>
			<CubeID><Cube></CubeID>
			<MeasureGroupID><MeasureGroup></MeasureGroupID>
		</ParentObject>
		<ObjectDefinition>
			<Partition xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:ddl2="http://schemas.microsoft.com/analysisservices/2003/engine/2" xmlns:ddl2_2="http://schemas.microsoft.com/analysisservices/2003/engine/2/2" xmlns:ddl100_100="http://schemas.microsoft.com/analysisservices/2008/engine/100/100" xmlns:ddl200="http://schemas.microsoft.com/analysisservices/2010/engine/200" xmlns:ddl200_200="http://schemas.microsoft.com/analysisservices/2010/engine/200/200" xmlns:ddl300="http://schemas.microsoft.com/analysisservices/2011/engine/300" xmlns:ddl300_300="http://schemas.microsoft.com/analysisservices/2011/engine/300/300" xmlns:ddl400="http://schemas.microsoft.com/analysisservices/2012/engine/400" xmlns:ddl400_400="http://schemas.microsoft.com/analysisservices/2012/engine/400/400" xmlns:ddl500="http://schemas.microsoft.com/analysisservices/2013/engine/500" xmlns:ddl500_500="http://schemas.microsoft.com/analysisservices/2013/engine/500/500">
				<ID><PartitionID></ID>
				<Name><PartitionName></Name>
				<Source xsi:type="QueryBinding">
					<DataSourceID>Billing</DataSourceID>
					<QueryDefinition>SELECT * 
	FROM [analysis].[BillingTransactions]
	WHERE [analysis].[BillingTransactions].[BillingPeriod] &gt;= <StartPeriod> AND [analysis].[BillingTransactions].[BillingPeriod] &lt; <EndPeriod></QueryDefinition>
				</Source>
				<StorageMode>Molap</StorageMode>
				<ProcessingMode>Regular</ProcessingMode>
				<ProactiveCaching>
					<SilenceInterval>-PT1S</SilenceInterval>
					<Latency>-PT1S</Latency>
					<SilenceOverrideInterval>-PT1S</SilenceOverrideInterval>
					<ForceRebuildInterval>-PT1S</ForceRebuildInterval>
					<Source xsi:type="ProactiveCachingInheritedBinding" />
				</ProactiveCaching>
			</Partition>
		</ObjectDefinition>
	</Create>'

	SET @CreatePartitionTemplate = REPLACE(REPLACE(REPLACE(@CreatePartitionTemplate, '<Catalog>', @CatalogName), '<Cube>', @CubeName), '<MeasureGroup>', @MeasureGroup)

	CREATE TABLE #NewPartitions (IDC INT IDENTITY(1,1) PRIMARY KEY, PartitionName VARCHAR(250), PartitionID VARCHAR(100), StartPeriod VARCHAR(6), EndPeriod VARCHAR(6))

	INSERT INTO #NewPartitions (PartitionID, PartitionName, StartPeriod, EndPeriod)
	VALUES
	('BillingTransactions1', 'Billing Transactions 2010-2014', '201001', '201501'),
	('BillingTransactions2', 'Billing Transactions 2015 H1', '201501', '201507'),
	('BillingTransactions3', 'Billing Transactions 2015 H2', '201507', '201601')

	SET @Counter = 1
	SELECT @MaxRows = COUNT(*) FROM #NewPartitions

	DECLARE @StartYear INT, @StartDate DATE, @Today DATE
	SET @StartYear = 2016
	SET @StartDate = DATEFROMPARTS(@StartYear, 1, 1)
	SET @Today = GETDATE()

	--For quarters
	WHILE @StartDate <= DATEADD(qq, -1, DATEADD(mm, - 1, @Today))
	BEGIN

		INSERT INTO #NewPartitions (PartitionID, PartitionName, StartPeriod, EndPeriod)
		VALUES
		('BillingTransactions' + CAST(@MaxRows + @Counter AS VARCHAR(10)),
		 'Billing Transactions ' + CAST(2016 + (@Counter - 1) / 4 AS VARCHAR(10)) + '  Q' + CAST((@Counter - 1) % 4 + 1 AS VARCHAR(1)), 
		 CONVERT(VARCHAR(6), DATEFROMPARTS(2016 + (@Counter - 1) / 4, ((@Counter - 1) % 4) * 3 + 1, 1), 112),
		 CONVERT(VARCHAR(6), DATEFROMPARTS(2016 + (@Counter+1 - 1) / 4, ((@Counter+1 - 1) % 4) * 3 + 1, 1), 112)
		 )

		SET @Counter = @Counter + 1
		SET @StartDate = DATEADD(qq, 1, @StartDate)

	END

	--For months
	WHILE @StartDate <= @Today  
	BEGIN

		INSERT INTO #NewPartitions (PartitionID, PartitionName, StartPeriod, EndPeriod)
		VALUES
		('BillingTransactions' + CAST(@MaxRows + @Counter AS VARCHAR(10)),
		'Billing Transactions ' + CAST(YEAR(@StartDate) AS VARCHAR(10)) + ' M' + CAST(MONTH(@StartDate) AS VARCHAR(2)), 
		CONVERT(VARCHAR(6), DATEFROMPARTS(YEAR(@StartDate), MONTH(@StartDate), 1), 112),
		CASE
			WHEN DATEADD(mm, 1, @StartDate) <= @Today THEN
				CONVERT(VARCHAR(6), DATEADD(mm, 1, DATEFROMPARTS(YEAR(@StartDate), MONTH(@StartDate), 1)), 112)
			ELSE
				'205001'
		END
		)

		SET @Counter = @Counter + 1
		SET @StartDate = DATEADD(mm, 1, @StartDate)

	END


	SELECT @MaxRows = COUNT(*) FROM #NewPartitions

	UPDATE #NewPartitions
	SET PartitionID = 'BillingTransactionsCurrent'
	WHERE IDC = @MaxRows

	UPDATE #NewPartitions
	SET PartitionID = 'BillingTransactionsPrevious'
	WHERE IDC = @MaxRows-1

	SET @Counter = 1
	DECLARE @StartPeriod VARCHAR(6), @EndPeriod VARCHAR(6), @PartitionID VARCHAR(100)

	SELECT @StartDate, *
	FROM #NewPartitions



	WHILE @Counter <= @MaxRows
	BEGIN

		SELECT 
			@PartitionName = PartitionName,
			@PartitionID = PartitionID,
			@StartPeriod = StartPeriod,
			@EndPeriod = EndPeriod
		FROM #NewPartitions WHERE IDC = @Counter

		SET @CreatePartitionScript = REPLACE(@CreatePartitionTemplate, '<PartitionName>', @PartitionName)
		SET @CreatePartitionScript = REPLACE(@CreatePartitionScript, '<PartitionID>', @PartitionID)
		SET @CreatePartitionScript = REPLACE(@CreatePartitionScript, '<StartPeriod>', @StartPeriod)
		SET @CreatePartitionScript = REPLACE(@CreatePartitionScript, '<EndPeriod>', @EndPeriod)
		SET @CreatePartitionScript = REPLACE('EXEC(''<Script>'') AT ' + @LinkedServerName,'<Script>', @CreatePartitionScript)	

		EXEC (@CreatePartitionScript)

		SET @Counter = @Counter + 1

	END

	DROP TABLE #NewPartitions

END
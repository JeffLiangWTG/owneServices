-- if you do this on test or prod, you're gonna have a bad time
IF (@@servername like 'wg1-vsql-1%' or @@SERVERNAME = 'ehubtransactions.db.wisegrid.net' or @@servername like 'au2sp-sbts-401%'  or @@servername like 'eHub-test%' or @@servername like 'SYDWP-SSQL-6%')
BEGIN
	RAISERROR(N'You are running this on the wrong server.', 16, 1) WITH NOWAIT
	RETURN
END

-- ensure production linked server is setup
IF NOT EXISTS(SELECT * FROM sys.servers WHERE name = 'eHubTransactions.db.wisegrid.net') BEGIN
	EXECUTE sp_addlinkedserver @server = 'eHubTransactions.db.wisegrid.net', @srvproduct = 'eHubTransactions.db.wisegrid.net', @provider='SQLOLEDB', @provstr = 'ApplicationIntent=ReadOnly', @datasrc = 'eHubTransactions.db.wisegrid.net'
	EXECUTE sp_addlinkedsrvlogin @rmtsrvname = 'eHubTransactions.db.wisegrid.net', @useself = 'FALSE', @rmtuser = 'eHubReader', @rmtpassword='ehubrocks'
END

USE eHubTransactions

-- temporarily disable constraints
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'

-- clear ALL tables
EXEC sp_MSforeachtable 'SET QUOTED_IDENTIFIER ON; DELETE FROM ?'

DECLARE @tableName nvarchar(128)
DECLARE @sqlCommand nvarchar(max)
DECLARE @sqlColumnsInto nvarchar(max)
DECLARE @sqlColumnsSelect nvarchar(max)

-- get cursor excluding message, irrelevant, or too large tables
DECLARE table_cursor CURSOR FOR 
SELECT table_name FROM information_schema.tables 
WHERE table_name not in ('eHubError','eHubInboxMessage','eHubInboxMessage2','eHubInboxMessageArchive','eHubInboxXmlContent','eHubOutboxMessage','eHubOutboxMessage2','eHubOutboxMessageArchive','eHubReferenceFileCache','eHubRegistrationLog','eHubSubscriptionValue','MarkLog')
AND TABLE_TYPE = 'BASE TABLE'
ORDER BY table_name

OPEN table_cursor
FETCH NEXT FROM table_cursor INTO @tableName

WHILE @@FETCH_STATUS = 0
BEGIN

	PRINT char(13) + char(10) + @tableName

	-- get column names excluding uncopyable types (e.g. 'timestamp')
	-- Need to split into ColumnsInto and ColumnsSelect variants so we can handle xml columns over an OPENQUERY
	SET @sqlColumnsInto = ''
	SELECT @sqlColumnsInto = @sqlColumnsInto + ', ' + column_name FROM INFORMATION_SCHEMA.COLUMNS
	 WHERE TABLE_NAME = @tableName AND DATA_TYPE <> 'timestamp'

	SET @sqlColumnsSelect = ''
	SELECT @sqlColumnsSelect = @sqlColumnsSelect + ', ' +
		CASE DATA_TYPE
			WHEN 'xml' THEN 'CAST(' + column_name + ' AS NVARCHAR(MAX))'
			ELSE column_name
		END
	FROM INFORMATION_SCHEMA.COLUMNS
	WHERE TABLE_NAME = @tableName AND DATA_TYPE <> 'timestamp'

	 -- format the sql and run it
	SET @sqlCommand = 'SET QUOTED_IDENTIFIER ON; INSERT INTO @TableName (@ColumnsInto) SELECT * FROM OPENQUERY([eHubTransactions.db.wisegrid.net], ''SELECT @ColumnsSelect FROM [eHubTransactions].[dbo].[@TableName] WITH (NOLOCK)'')'

	SET @sqlCommand = REPLACE(@sqlCommand, '@TableName', @tableName)
	SET @sqlCommand = REPLACE(@sqlCommand, '@ColumnsInto', STUFF(@sqlColumnsInto, 1, 2, ''))
	SET @sqlCommand = REPLACE(@sqlCommand, '@ColumnsSelect', STUFF(@sqlColumnsSelect, 1, 2, ''))

	PRINT @sqlCommand
	EXECUTE(@sqlCommand)

	FETCH NEXT FROM table_cursor
	INTO @tableName
END

CLOSE table_cursor
DEALLOCATE table_cursor

-- reenable constraints
EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL'
GO

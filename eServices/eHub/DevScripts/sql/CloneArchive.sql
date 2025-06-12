
DECLARE @eHubArchiveOnlinetableName nvarchar(128)
DECLARE @sqlCommand nvarchar(max)
DECLARE @sqlColumns nvarchar(max)
DECLARE @numberOfRecord INT 
DECLARE @eHubArchiveOnlineBackupDB nvarchar(128) = 'eHubArchiveOnline'

DECLARE @sql varchar(100)
DECLARE @ApplicationCode varchar(100)
DECLARE @INTTRA varchar(100)
DECLARE @EVERGREEN varchar(100)
DECLARE @GTNEXUS varchar(100)


DECLARE @SHIPPINGINSTRCTION_PK varchar(100)
DECLARE @startTime varchar(100)
DECLARE @endTime varchar(100)

select @sql = 'USE ' + @eHubArchiveOnlineBackupDB
EXEC sp_sqlexec @Sql

-- ======================================================================================================== 
-- ======================================================================================================== 
-- ======================================================================================================== 
-- ======================================================================================================== 
--Put the providers you care about here
		SET @INTTRA = '%INTTRA%'
		SET @EVERGREEN = '%EVERGREEN%'
		SET @GTNEXUS = '%GTNEXUS%'
		SET @numberOfRecord = 1000

		SET @startTime = '2019-05-01 09:31:59.913'
		SET @endTime = '2019-05-19 09:31:59.913'
-- ======================================================================================================== 
-- ======================================================================================================== 
-- ======================================================================================================== 

	-- If you want to clear your DB put this back.
	EXEC sp_MSforeachtable 'SET QUOTED_IDENTIFIER ON; DELETE FROM ?'
-- ======================================================================================================== 

	-- if you do this on test or prod, you're gonna have a bad time
	IF (@@servername like 'wg1-vsql-1%' or @@SERVERNAME = 'eHubArchiveOnline.db.wisegrid.net' or @@servername like 'au2sp-sbts-401%' or @@servername like 'eHub-test%')
	BEGIN
		RAISERROR(N'You are running this on the wrong server.', 16, 1) WITH NOWAIT
		RETURN
	END

	IF NOT EXISTS(SELECT * FROM sys.servers WHERE name = 'WG1-VSQL-1.WG.CARGOWISE.COM') BEGIN
		EXECUTE sp_addlinkedserver @server = 'WG1-VSQL-1.WG.CARGOWISE.COM', @srvproduct = 'wg1-vsql-1.wg.cargowise.com', @provider='SQLOLEDB', @datasrc = 'wg1-vsql-1.wg.cargowise.com'
		EXECUTE sp_addlinkedsrvlogin @rmtsrvname = 'WG1-VSQL-1.WG.CARGOWISE.COM', @useself = 'FALSE', @rmtuser = 'eHubReader', @rmtpassword='ehubrocks'
	END
	

	EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'


	DECLARE table_cursor CURSOR FOR 
	SELECT table_name FROM information_schema.tables 
	WHERE table_name = 'eHubArchiveMessage'
	AND TABLE_TYPE = 'BASE TABLE'
	ORDER BY table_name

	OPEN table_cursor
	FETCH NEXT FROM table_cursor INTO @eHubArchiveOnlinetableName

	WHILE @@FETCH_STATUS = 0
	BEGIN

		PRINT char(13) + char(10) + @eHubArchiveOnlinetableName
		SET @SHIPPINGINSTRCTION_PK = 'A8F34356-F459-4805-8026-9CB072B7E0A3'
		SET @ApplicationCode = 'UDM'

		SET @sqlColumns = ''
		SELECT @sqlColumns = @sqlColumns + ', ' + column_name FROM INFORMATION_SCHEMA.COLUMNS  
		 WHERE TABLE_NAME = 'eHubArchiveMessage' AND DATA_TYPE <> 'timestamp' AND column_name not in('AM_RecipientMessageXML', 'AM_SenderMessageXML', 'AM_RecipientMessageRaw')
		SET @sqlCommand = 'SET QUOTED_IDENTIFIER ON; INSERT INTO @eHubArchiveOnlinetableName (@Columns) SELECT * FROM OPENQUERY([WG1-VSQL-1.WG.CARGOWISE.COM], ''SELECT top @numberOfRecord @Columns FROM 
		[eHubArchiveOnline].[dbo].[eHubArchiveMessage] WITH (NOLOCK) WHERE
		 AM_Status = 3  AND 
		 AM_BatchEnvelopeTrackingID is null  AND 
		 AM_CC_RecipientInbox = '''''+ @SHIPPINGINSTRCTION_PK+ '''''  AND 
		 (AM_CC_RecipientOutbox in  (SELECT CC_PK FROM [eHubArchiveOnline].[dbo].[ehubclient] where CC_ID like  '''''+ @INTTRA+ ''''' ) OR AM_CC_RecipientOutbox in  (SELECT CC_PK FROM [eHubArchiveOnline].[dbo].[ehubclient] where CC_ID like  '''''+ @EVERGREEN+ ''''' )  OR AM_CC_RecipientOutbox in  (SELECT CC_PK FROM [eHubArchiveOnline].[dbo].[ehubclient] where CC_ID like  '''''+ @GTNEXUS+ ''''' ) )AND
		 AM_ApplicationCode = '''''+ @ApplicationCode+ ''''' AND
		 AM_ReceivedFromSenderUTC >= '''''+ @startTime+ ''''' AND
		 AM_ReceivedFromSenderUTC <= '''''+ @endTime+ ''''' 
		 Order by AM_ReceivedFromSenderUTC DESC '')'
		
		SET @sqlCommand = REPLACE(@sqlCommand, '@ApplicationCode', @ApplicationCode)

		SET @sqlCommand = REPLACE(@sqlCommand, '@SHIPPINGINSTRCTION_PK', @SHIPPINGINSTRCTION_PK)
		SET @sqlCommand = REPLACE(@sqlCommand, '@eHubArchiveOnlinetableName', @eHubArchiveOnlinetableName)
		SET @sqlCommand = REPLACE(@sqlCommand, '@eHubArchiveOnlineBackupDB', @eHubArchiveOnlineBackupDB)
		SET @sqlCommand = REPLACE(@sqlCommand, '@numberOfRecord', @numberOfRecord)
		SET @sqlCommand = REPLACE(@sqlCommand, '@startTime', @startTime)
		SET @sqlCommand = REPLACE(@sqlCommand, '@endTime', @endTime)
		SET @sqlCommand = REPLACE(@sqlCommand, '@Columns', STUFF(@sqlColumns, 1, 2, ''))

-- ======================================================================================================== 
-- ======================================================================================================== 
-- ======================================================================================================== 
-- ======================================================================================================== 
-- Replace it by using the string above
		SET @sqlCommand = REPLACE(@sqlCommand, '@INTTRA', @INTTRA)
		SET @sqlCommand = REPLACE(@sqlCommand, '@GTNEXUS', @GTNEXUS)
		SET @sqlCommand = REPLACE(@sqlCommand, '@EVERGREEN', @EVERGREEN)
-- ======================================================================================================== 
-- ======================================================================================================== 
-- ======================================================================================================== 

		PRINT @sqlCommand
		EXECUTE(@sqlCommand)

		FETCH NEXT FROM table_cursor 
		INTO @eHubArchiveOnlinetableName
	END

	CLOSE table_cursor
	DEALLOCATE table_cursor

EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL'
GO


UPDATE eHubArchiveMessage SET
    AM_SenderMessageXML =  REPLACE(eHubArchiveOnline.dbo.DecodeAndDecompress(AM_SenderMessageRaw), 'encoding="UTF-8"','')

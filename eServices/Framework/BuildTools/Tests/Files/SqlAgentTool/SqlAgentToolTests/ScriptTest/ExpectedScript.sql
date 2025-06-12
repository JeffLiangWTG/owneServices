-- Environment: Default
:connect localhost

USE [msdb]
SET XACT_ABORT ON

------------------------------------------------------------------------
PRINT 'Adding Categories'

IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'Database Maintenance' AND category_class=1)
    EXEC msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'Database Maintenance'

------------------------------------------------------------------------
PRINT 'Adding [AlertDeadlock]'

BEGIN TRANSACTION

EXEC msdb.dbo.sp_add_job @job_name=N'AlertDeadlock',
    @description=N'No description available.',
    @owner_login_name=N'sa',
    @category_name=N'[Uncategorized (Local)]',
    @notify_level_email=2,
    @notify_email_operator_name=N'eServices',
    @enabled=1

DECLARE @AlertDeadlock_0_Command nvarchar(max) = CONCAT(N'SET ANSI_NULLS, QUOTED_IDENTIFIER ON;

DECLARE @errorPK uniqueidentifier = NEWID();
DECLARE @source varchar(3) = ''SQL'';
DECLARE @errorType varchar(3) = ''Fai'';
DECLARE @description varchar(max);
DECLARE @inboxPK varchar(36) = NULL;
DECLARE @currentDateTimeUTC varchar(32) = convert(varchar(32), getutcdate(), 127);
DECLARE @issueMessageDelimiter nvarchar(50) = ''
----------BIZTALK PROPERTIES----------
'';

DECLARE @textData nvarchar(max);
DECLARE @graphXml xml;
DECLARE @issueManagerKey nvarchar(max);

SET @textData = REPLACE(''', '$', '(ESCAPE_NONE(WMI(TextData)))'', ''&#x0A;'', CHAR(10));
SET @graphXml = @textData

DECLARE @databaseName nvarchar(1000) = @graphXml.value(''(/TextData/deadlock-list/deadlock/process-list/process/@currentdbname)[1]'', ''nvarchar(1000)'');
DECLARE @logMessage nvarchar(max) = ''Deadlock in database ['' + @databaseName + '']: '' + @textData;
DECLARE @pos int = 1, @len int = 1000;
WHILE @pos <= LEN(@logMessage) BEGIN PRINT SUBSTRING(@logMessage, @pos, @len); SET @pos += @len; END
IF @databaseName NOT IN (''BAMArchive'',''BAMPrimaryImport'',''BizTalkDTADb'',''BizTalkMgmtDb'',''BizTalkMsgBoxDb'',''SSISDB'',''SSODB'')
  RETURN;

SELECT @issueManagerKey = @graphXml.value(''(/TextData/deadlock-list/deadlock/process-list/process/inputbuf)[string-length()>0][1]'', ''nvarchar(max)'');
IF @issueManagerKey IS NULL
BEGIN
	SELECT @issueManagerKey = @graphXml.value(''(/TextData/deadlock-list/deadlock/process-list/process[1]/executionStack/frame)[last()]'', ''nvarchar(max)'');
END

SET @issueManagerKey = REPLACE(CONCAT(''Deadlock: '', @issueManagerKey), CHAR(10), '' '')
WHILE CHARINDEX(''  '', @issueManagerKey) > 0 SET @issueManagerKey = REPLACE(@issueManagerKey, ''  '', '' '')
SET @description = CONCAT(@issueManagerKey, @issueMessageDelimiter, ''SERVERNAME: ', '$', '(ESCAPE_NONE(A-SVR))'', CHAR(10), ''REPORT:'', CHAR(10),  @textData);

EXECUTE [eHubTransactionsServer].[eHubTransactions].[dbo].[InsertError] 
	 @ErrorPK = @errorPK
	,@Source = @source
	,@ErrorType = @errorType
	,@Description = @description
	,@InboxPK = @inboxPK
	,@CurrentDateTimeUTC = @currentDateTimeUTC;

PRINT ''Sent to issue manager.''
')
EXEC msdb.dbo.sp_add_jobstep @step_name=N'SendAlert',
    @job_name=N'AlertDeadlock',
    @on_success_action=1,
    @on_fail_action=2,
    @subsystem=N'TSQL',
    @database_name=N'msdb',
    @command=@AlertDeadlock_0_Command

EXEC msdb.dbo.sp_add_alert @name=N'Deadlock',
    @job_name=N'AlertDeadlock',
    @wmi_namespace='\\.\root\Microsoft\SqlServer\ServerEvents\INSTANCE1',
    @wmi_query='SELECT * FROM DEADLOCK_GRAPH',
    @delay_between_responses='0',
    @enabled=1

EXEC msdb.dbo.sp_add_jobserver @job_name=N'AlertDeadlock', @server_name=N'(local)'

COMMIT TRANSACTION

------------------------------------------------------------------------
PRINT 'Adding [BackupBizTalkServerBizTalkMgmtDb]'

BEGIN TRANSACTION

EXEC msdb.dbo.sp_add_job @job_name=N'Backup BizTalk Server (BizTalkMgmtDb)',
    @description=N'This job performs full database backups (step 1) and log backups (step 2) of BizTalk Server databases.',
    @owner_login_name=N'sa',
    @category_name=N'Database Maintenance',
    @notify_level_email=2,
    @notify_email_operator_name=N'eServices',
    @enabled=0

EXEC msdb.dbo.sp_add_jobstep @step_name=N'CheckIsPrimary',
    @job_name=N'Backup BizTalk Server (BizTalkMgmtDb)',
    @on_success_action=3,
    @on_fail_action=1,
    @subsystem=N'TSQL',
    @database_name=N'master',
    @command=N'IF sys.fn_hadr_is_primary_replica(''BizTalkMgmtDb'') = 0
	RAISERROR(''Instance is secondary replica'',11,0)
'

EXEC msdb.dbo.sp_add_jobstep @step_name=N'SetCompressionOption',
    @job_name=N'Backup BizTalk Server (BizTalkMgmtDb)',
    @on_success_action=3,
    @on_fail_action=3,
    @subsystem=N'TSQL',
    @database_name=N'BizTalkMgmtDb',
    @command=N'exec [dbo].[sp_SetBackupCompression] @bCompression = 1 /*0 - Do not use Compression, 1 - Use Compression */
'

EXEC msdb.dbo.sp_add_jobstep @step_name=N'BackupFull',
    @job_name=N'Backup BizTalk Server (BizTalkMgmtDb)',
    @on_success_action=3,
    @on_fail_action=2,
    @subsystem=N'TSQL',
    @database_name=N'BizTalkMgmtDb',
    @command=N'exec [dbo].[sp_BackupAllFull_Schedule] 
	''d'' /* Frequency */, 
	''BTS'' /* Name */, 
	''\\sydwp-sbak-2.wisecloud.zone\Other\Non_DPM_ToTape\WormHole\BIZTLK'' /* location of backup files */, 
	0 /* Force full backup after partial failure */,
	21 /* backup hour */,
	1 /* use local time */
'

EXEC msdb.dbo.sp_add_jobstep @step_name=N'MarkAndBackupLog',
    @job_name=N'Backup BizTalk Server (BizTalkMgmtDb)',
    @on_success_action=3,
    @on_fail_action=2,
    @subsystem=N'TSQL',
    @database_name=N'BizTalkMgmtDb',
    @command=N'exec [dbo].[sp_MarkAll] ''BTS'' /*  Log mark name */, 
		      ''\\sydwp-sbak-2.wisecloud.zone\Other\Non_DPM_ToTape\WormHole\BIZTLK'' /* location of backup files */
'

EXEC msdb.dbo.sp_add_jobstep @step_name=N'ClearBackupHistory',
    @job_name=N'Backup BizTalk Server (BizTalkMgmtDb)',
    @on_success_action=1,
    @on_fail_action=2,
    @subsystem=N'TSQL',
    @database_name=N'BizTalkMgmtDb',
    @command=N'exec [dbo].[sp_DeleteBackupHistoryAndMarkLogsHistory] @DaysToKeep=14
'

EXEC msdb.dbo.sp_add_jobschedule @name=N'MarkAndBackupLogSched',
    @job_name=N'Backup BizTalk Server (BizTalkMgmtDb)',
    @freq_type='4',
    @freq_interval='1',
    @freq_subday_type='4',
    @freq_subday_interval='15',
    @enabled=1

EXEC msdb.dbo.sp_add_jobserver @job_name=N'Backup BizTalk Server (BizTalkMgmtDb)', @server_name=N'(local)'

COMMIT TRANSACTION

------------------------------------------------------------------------
PRINT 'Adding [CleanupBTFExpiredEntriesJob_BizTalkMgmtDb]'

BEGIN TRANSACTION

EXEC msdb.dbo.sp_add_job @job_name=N'CleanupBTFExpiredEntriesJob_BizTalkMgmtDb',
    @description=N'cleanup expired btf entries',
    @owner_login_name=N'sa',
    @category_name=N'Database Maintenance',
    @notify_level_email=2,
    @notify_email_operator_name=N'eServices',
    @enabled=1

EXEC msdb.dbo.sp_add_jobstep @step_name=N'CheckIsPrimary',
    @job_name=N'CleanupBTFExpiredEntriesJob_BizTalkMgmtDb',
    @on_success_action=3,
    @on_fail_action=1,
    @subsystem=N'TSQL',
    @database_name=N'master',
    @command=N'IF sys.fn_hadr_is_primary_replica(''BizTalkMgmtDb'') = 0
	RAISERROR(''Instance is secondary replica'',11,0)
'

EXEC msdb.dbo.sp_add_jobstep @step_name=N'step1',
    @job_name=N'CleanupBTFExpiredEntriesJob_BizTalkMgmtDb',
    @on_success_action=1,
    @on_fail_action=2,
    @subsystem=N'TSQL',
    @database_name=N'BizTalkMgmtDb',
    @command=N'exec btf_PurgeExpiredMessages
'

EXEC msdb.dbo.sp_add_jobschedule @name=N'sched',
    @job_name=N'CleanupBTFExpiredEntriesJob_BizTalkMgmtDb',
    @freq_type='4',
    @freq_interval='1',
    @freq_subday_type='8',
    @freq_subday_interval='12',
    @enabled=1

EXEC msdb.dbo.sp_add_jobserver @job_name=N'CleanupBTFExpiredEntriesJob_BizTalkMgmtDb', @server_name=N'(local)'

COMMIT TRANSACTION

------------------------------------------------------------------------
PRINT 'Adding [CommandLogCleanup]'

BEGIN TRANSACTION

EXEC msdb.dbo.sp_add_job @job_name=N'CommandLog Cleanup',
    @description=N'Source: https://ola.hallengren.com',
    @owner_login_name=N'sa',
    @category_name=N'Database Maintenance',
    @notify_level_email=2,
    @notify_email_operator_name=N'eServices',
    @enabled=1

DECLARE @CommandLogCleanup_0_OutputFile nvarchar(max) = CONCAT(N'', '$', '(ESCAPE_SQUOTE(SQLLOGDIR))\', '$', '(ESCAPE_SQUOTE(JOBNAME))_', '$', '(ESCAPE_SQUOTE(STEPID))_', '$', '(ESCAPE_SQUOTE(DATE))_', '$', '(ESCAPE_SQUOTE(TIME)).txt')
EXEC msdb.dbo.sp_add_jobstep @step_name=N'CommandLogCleanup',
    @job_name=N'CommandLog Cleanup',
    @on_success_action=1,
    @on_fail_action=2,
    @output_file_name=@CommandLogCleanup_0_OutputFile,
    @subsystem=N'TSQL',
    @database_name=N'master',
    @command=N'DELETE FROM [dbo].[CommandLog]
WHERE StartTime < DATEADD(dd,-30,GETDATE())
'

EXEC msdb.dbo.sp_add_jobschedule @name=N'Cleanup',
    @job_name=N'CommandLog Cleanup',
    @freq_type='8',
    @freq_interval='1',
    @freq_subday_type='1',
    @freq_recurrence_factor='1',
    @enabled=1

EXEC msdb.dbo.sp_add_jobserver @job_name=N'CommandLog Cleanup', @server_name=N'(local)'

COMMIT TRANSACTION

------------------------------------------------------------------------
PRINT 'Adding [DatabaseIntegrityCheckSYSTEM_DATABASES]'

BEGIN TRANSACTION

EXEC msdb.dbo.sp_add_job @job_name=N'DatabaseIntegrityCheck - SYSTEM_DATABASES',
    @description=N'Source: https://ola.hallengren.com',
    @owner_login_name=N'sa',
    @category_name=N'Database Maintenance',
    @notify_level_email=2,
    @notify_email_operator_name=N'eServices',
    @enabled=1

EXEC msdb.dbo.sp_add_jobstep @step_name=N'DatabaseIntegrityCheckSYSTEM_DATABASES',
    @job_name=N'DatabaseIntegrityCheck - SYSTEM_DATABASES',
    @on_success_action=1,
    @on_fail_action=2,
    @subsystem=N'TSQL',
    @database_name=N'master',
    @command=N'EXECUTE [dbo].[DatabaseIntegrityCheck]
@Databases = ''SYSTEM_DATABASES'',
@LogToTable = ''Y''
'

EXEC msdb.dbo.sp_add_jobschedule @name=N'IntegrityCheck_System',
    @job_name=N'DatabaseIntegrityCheck - SYSTEM_DATABASES',
    @freq_type='8',
    @freq_interval='1',
    @freq_subday_type='1',
    @freq_recurrence_factor='1',
    @active_start_time='013000',
    @enabled=1

EXEC msdb.dbo.sp_add_jobserver @job_name=N'DatabaseIntegrityCheck - SYSTEM_DATABASES', @server_name=N'(local)'

COMMIT TRANSACTION

------------------------------------------------------------------------
PRINT 'Adding [DatabaseIntegrityCheckUSER_DATABASES]'

BEGIN TRANSACTION

EXEC msdb.dbo.sp_add_job @job_name=N'DatabaseIntegrityCheck - USER_DATABASES',
    @description=N'Source: https://ola.hallengren.com',
    @owner_login_name=N'sa',
    @category_name=N'Database Maintenance',
    @notify_level_email=2,
    @notify_email_operator_name=N'eServices',
    @enabled=1

EXEC msdb.dbo.sp_add_jobstep @step_name=N'DatabaseIntegrityCheckUSER_DATABASES',
    @job_name=N'DatabaseIntegrityCheck - USER_DATABASES',
    @on_success_action=1,
    @on_fail_action=2,
    @subsystem=N'TSQL',
    @database_name=N'master',
    @command=N'EXECUTE [dbo].[DatabaseIntegrityCheck]
@Databases = ''BAMArchive,BAMPrimaryImport,BizTalkDTADb,BizTalkMgmtDb,BizTalkMsgBoxDb,SSISDB,SSODB'',
@LogToTable = ''Y''
'

EXEC msdb.dbo.sp_add_jobschedule @name=N'IntegrityCheck_User',
    @job_name=N'DatabaseIntegrityCheck - USER_DATABASES',
    @freq_type='8',
    @freq_interval='1',
    @freq_subday_type='1',
    @freq_recurrence_factor='1',
    @active_start_time='010000',
    @enabled=1

EXEC msdb.dbo.sp_add_jobserver @job_name=N'DatabaseIntegrityCheck - USER_DATABASES', @server_name=N'(local)'

COMMIT TRANSACTION

------------------------------------------------------------------------
PRINT 'Adding [DTAPurgeandArchiveBizTalkDTADb]'

BEGIN TRANSACTION

EXEC msdb.dbo.sp_add_job @job_name=N'DTA Purge and Archive (BizTalkDTADb)',
    @description=N'Job to automate the purging and archiving of the tracking database',
    @owner_login_name=N'sa',
    @category_name=N'Database Maintenance',
    @notify_level_email=2,
    @notify_email_operator_name=N'eServices',
    @enabled=1

EXEC msdb.dbo.sp_add_jobstep @step_name=N'CheckIsPrimary',
    @job_name=N'DTA Purge and Archive (BizTalkDTADb)',
    @on_success_action=3,
    @on_fail_action=1,
    @subsystem=N'TSQL',
    @database_name=N'master',
    @command=N'IF sys.fn_hadr_is_primary_replica(''BizTalkMgmtDb'') = 0
	RAISERROR(''Instance is secondary replica'',11,0)
'

EXEC msdb.dbo.sp_add_jobstep @step_name=N'ArchiveandPurge',
    @job_name=N'DTA Purge and Archive (BizTalkDTADb)',
    @on_success_action=1,
    @on_fail_action=2,
    @subsystem=N'TSQL',
    @database_name=N'BizTalkDTADb',
    @command=N'declare @dtLastBackup datetime = GetUTCDate()

exec dtasp_PurgeTrackingDatabase
	2, --@nLiveHours tinyint -- Any completed instance older than the live hours +live days
	1, --@nLiveDays tinyint -- will be deleted along with all associated data
	30, --@nHardDays tinyint -- All data older than this will be deleted.
	@dtLastBackup, --@dtLastBackup datetime,
	1  -- @fHardDeleteRunningInstances int
'

EXEC msdb.dbo.sp_add_jobschedule @name=N'Archive and Purge Schedule',
    @job_name=N'DTA Purge and Archive (BizTalkDTADb)',
    @freq_type='4',
    @freq_interval='1',
    @freq_subday_type='4',
    @freq_subday_interval='1',
    @enabled=1

EXEC msdb.dbo.sp_add_jobserver @job_name=N'DTA Purge and Archive (BizTalkDTADb)', @server_name=N'(local)'

COMMIT TRANSACTION

------------------------------------------------------------------------
PRINT 'Adding [IndexOptimizeUSER_DATABASES]'

BEGIN TRANSACTION

EXEC msdb.dbo.sp_add_job @job_name=N'IndexOptimize - USER_DATABASES',
    @description=N'Source: https://ola.hallengren.com',
    @owner_login_name=N'sa',
    @category_name=N'Database Maintenance',
    @notify_level_email=2,
    @notify_email_operator_name=N'eServices',
    @enabled=1

EXEC msdb.dbo.sp_add_jobstep @step_name=N'IndexOptimizeUSER_DATABASES',
    @job_name=N'IndexOptimize - USER_DATABASES',
    @on_success_action=1,
    @on_fail_action=2,
    @subsystem=N'TSQL',
    @database_name=N'master',
    @command=N'EXECUTE [dbo].[IndexOptimize]
@Databases = ''BAMArchive,BAMPrimaryImport,BizTalkMgmtDb,SSISDB,SSODB'',
@FragmentationMedium = ''INDEX_REORGANIZE,INDEX_REBUILD_ONLINE'',
@FragmentationHigh = ''INDEX_REBUILD_ONLINE'',
@LogToTable = ''Y''
'

EXEC msdb.dbo.sp_add_jobschedule @name=N'IndexOptimize_User',
    @job_name=N'IndexOptimize - USER_DATABASES',
    @freq_type='8',
    @freq_interval='1',
    @freq_subday_type='1',
    @freq_recurrence_factor='1',
    @active_start_time='020000',
    @enabled=1

EXEC msdb.dbo.sp_add_jobserver @job_name=N'IndexOptimize - USER_DATABASES', @server_name=N'(local)'

COMMIT TRANSACTION

------------------------------------------------------------------------
PRINT 'Adding [MessageBox_DeadProcesses_Cleanup_BizTalkMsgBoxDb]'

BEGIN TRANSACTION

EXEC msdb.dbo.sp_add_job @job_name=N'MessageBox_DeadProcesses_Cleanup_BizTalkMsgBoxDb',
    @description=N'This job detects when a host instance has stopped and releases all work locked by that host instance so it can be worked on by another host instance.',
    @owner_login_name=N'PROD\mxu',
    @category_name=N'Database Maintenance',
    @notify_level_email=2,
    @notify_email_operator_name=N'eServices',
    @enabled=1

EXEC msdb.dbo.sp_add_jobstep @step_name=N'CheckIsPrimary',
    @job_name=N'MessageBox_DeadProcesses_Cleanup_BizTalkMsgBoxDb',
    @on_success_action=3,
    @on_fail_action=1,
    @subsystem=N'TSQL',
    @database_name=N'master',
    @command=N'IF sys.fn_hadr_is_primary_replica(''BizTalkMgmtDb'') = 0
	RAISERROR(''Instance is secondary replica'',11,0)
'

EXEC msdb.dbo.sp_add_jobstep @step_name=N'Purge',
    @job_name=N'MessageBox_DeadProcesses_Cleanup_BizTalkMsgBoxDb',
    @on_success_action=1,
    @on_fail_action=2,
    @subsystem=N'TSQL',
    @database_name=N'BizTalkMsgBoxDb',
    @command=N'exec bts_CleanupDeadProcesses
'

EXEC msdb.dbo.sp_add_jobschedule @name=N'Schedule',
    @job_name=N'MessageBox_DeadProcesses_Cleanup_BizTalkMsgBoxDb',
    @freq_type='4',
    @freq_interval='1',
    @freq_subday_type='4',
    @freq_subday_interval='1',
    @enabled=1

EXEC msdb.dbo.sp_add_jobserver @job_name=N'MessageBox_DeadProcesses_Cleanup_BizTalkMsgBoxDb', @server_name=N'(local)'

COMMIT TRANSACTION

------------------------------------------------------------------------
PRINT 'Adding [MessageBox_Message_Cleanup_BizTalkMsgBoxDb]'

BEGIN TRANSACTION

EXEC msdb.dbo.sp_add_job @job_name=N'MessageBox_Message_Cleanup_BizTalkMsgBoxDb',
    @description=N'This job removes all messages that are no longer being referenced by any subscribers in the BizTalk MessageBox database tables. This is an unscheduled job and is automatically started by the ManageRefCountLog job.',
    @owner_login_name=N'PROD\mxu',
    @category_name=N'Database Maintenance',
    @notify_level_email=2,
    @notify_email_operator_name=N'eServices',
    @enabled=1

EXEC msdb.dbo.sp_add_jobstep @step_name=N'Purge',
    @job_name=N'MessageBox_Message_Cleanup_BizTalkMsgBoxDb',
    @on_success_action=1,
    @on_fail_action=2,
    @subsystem=N'TSQL',
    @database_name=N'BizTalkMsgBoxDb',
    @command=N'exec bts_PurgeMessages
'

EXEC msdb.dbo.sp_add_jobserver @job_name=N'MessageBox_Message_Cleanup_BizTalkMsgBoxDb', @server_name=N'(local)'

COMMIT TRANSACTION

------------------------------------------------------------------------
PRINT 'Adding [MessageBox_Message_ManageRefCountLog_BizTalkMsgBoxDb]'

BEGIN TRANSACTION

EXEC msdb.dbo.sp_add_job @job_name=N'MessageBox_Message_ManageRefCountLog_BizTalkMsgBoxDb',
    @description=N'This job manages the reference count logs for messages and determines when a message is no longer referenced by any subscriber.',
    @owner_login_name=N'PROD\mxu',
    @category_name=N'Database Maintenance',
    @notify_level_email=2,
    @notify_email_operator_name=N'eServices',
    @enabled=1

EXEC msdb.dbo.sp_add_jobstep @step_name=N'CheckIsPrimary',
    @job_name=N'MessageBox_Message_ManageRefCountLog_BizTalkMsgBoxDb',
    @on_success_action=3,
    @on_fail_action=1,
    @subsystem=N'TSQL',
    @database_name=N'master',
    @command=N'IF sys.fn_hadr_is_primary_replica(''BizTalkMgmtDb'') = 0
	RAISERROR(''Instance is secondary replica'',11,0)
'

EXEC msdb.dbo.sp_add_jobstep @step_name=N'Purge',
    @job_name=N'MessageBox_Message_ManageRefCountLog_BizTalkMsgBoxDb',
    @on_success_action=1,
    @on_fail_action=2,
    @subsystem=N'TSQL',
    @database_name=N'BizTalkMsgBoxDb',
    @command=N'exec bts_ManageMessageRefCountLog
'

EXEC msdb.dbo.sp_add_jobschedule @name=N'Schedule',
    @job_name=N'MessageBox_Message_ManageRefCountLog_BizTalkMsgBoxDb',
    @freq_type='4',
    @freq_interval='1',
    @freq_subday_type='4',
    @freq_subday_interval='1',
    @enabled=1

EXEC msdb.dbo.sp_add_jobserver @job_name=N'MessageBox_Message_ManageRefCountLog_BizTalkMsgBoxDb', @server_name=N'(local)'

COMMIT TRANSACTION

------------------------------------------------------------------------
PRINT 'Adding [MessageBox_Parts_Cleanup_BizTalkMsgBoxDb]'

BEGIN TRANSACTION

EXEC msdb.dbo.sp_add_job @job_name=N'MessageBox_Parts_Cleanup_BizTalkMsgBoxDb',
    @description=N'This job removes all message parts that are no longer being referenced by any messages in the BizTalk MessageBox database tables.',
    @owner_login_name=N'PROD\mxu',
    @category_name=N'Database Maintenance',
    @notify_level_email=2,
    @notify_email_operator_name=N'eServices',
    @enabled=1

EXEC msdb.dbo.sp_add_jobstep @step_name=N'CheckIsPrimary',
    @job_name=N'MessageBox_Parts_Cleanup_BizTalkMsgBoxDb',
    @on_success_action=3,
    @on_fail_action=1,
    @subsystem=N'TSQL',
    @database_name=N'master',
    @command=N'IF sys.fn_hadr_is_primary_replica(''BizTalkMgmtDb'') = 0
	RAISERROR(''Instance is secondary replica'',11,0)
'

EXEC msdb.dbo.sp_add_jobstep @step_name=N'Purge',
    @job_name=N'MessageBox_Parts_Cleanup_BizTalkMsgBoxDb',
    @on_success_action=1,
    @on_fail_action=2,
    @subsystem=N'TSQL',
    @database_name=N'BizTalkMsgBoxDb',
    @command=N'exec bts_PurgeParts
'

EXEC msdb.dbo.sp_add_jobschedule @name=N'Schedule',
    @job_name=N'MessageBox_Parts_Cleanup_BizTalkMsgBoxDb',
    @freq_type='4',
    @freq_interval='1',
    @freq_subday_type='4',
    @freq_subday_interval='1',
    @enabled=1

EXEC msdb.dbo.sp_add_jobserver @job_name=N'MessageBox_Parts_Cleanup_BizTalkMsgBoxDb', @server_name=N'(local)'

COMMIT TRANSACTION

------------------------------------------------------------------------
PRINT 'Adding [MessageBox_UpdateStats_BizTalkMsgBoxDb]'

BEGIN TRANSACTION

EXEC msdb.dbo.sp_add_job @job_name=N'MessageBox_UpdateStats_BizTalkMsgBoxDb',
    @description=N'This job manually updates the statistics for the BizTalk MessageBox database.',
    @owner_login_name=N'PROD\mxu',
    @category_name=N'Database Maintenance',
    @notify_level_email=2,
    @notify_email_operator_name=N'eServices',
    @enabled=1

EXEC msdb.dbo.sp_add_jobstep @step_name=N'CheckIsPrimary',
    @job_name=N'MessageBox_UpdateStats_BizTalkMsgBoxDb',
    @on_success_action=3,
    @on_fail_action=1,
    @subsystem=N'TSQL',
    @database_name=N'master',
    @command=N'IF sys.fn_hadr_is_primary_replica(''BizTalkMgmtDb'') = 0
	RAISERROR(''Instance is secondary replica'',11,0)
'

EXEC msdb.dbo.sp_add_jobstep @step_name=N'Purge',
    @job_name=N'MessageBox_UpdateStats_BizTalkMsgBoxDb',
    @on_success_action=1,
    @on_fail_action=2,
    @subsystem=N'TSQL',
    @database_name=N'BizTalkMsgBoxDb',
    @command=N'set deadlock_priority low  exec sp_updatestats
'

EXEC msdb.dbo.sp_add_jobschedule @name=N'Schedule',
    @job_name=N'MessageBox_UpdateStats_BizTalkMsgBoxDb',
    @freq_type='4',
    @freq_interval='1',
    @freq_subday_type='4',
    @freq_subday_interval='5',
    @enabled=1

EXEC msdb.dbo.sp_add_jobserver @job_name=N'MessageBox_UpdateStats_BizTalkMsgBoxDb', @server_name=N'(local)'

COMMIT TRANSACTION

------------------------------------------------------------------------
PRINT 'Adding [MonitorBizTalkServerBizTalkMgmtDb]'

BEGIN TRANSACTION

EXEC msdb.dbo.sp_add_job @job_name=N'Monitor BizTalk Server (BizTalkMgmtDb)',
    @description=N'This job runs once every week and finds known issues in all MessageBox and DTA',
    @owner_login_name=N'sa',
    @category_name=N'Database Maintenance',
    @notify_level_email=2,
    @notify_email_operator_name=N'eServices',
    @enabled=1

EXEC msdb.dbo.sp_add_jobstep @step_name=N'CheckIsPrimary',
    @job_name=N'Monitor BizTalk Server (BizTalkMgmtDb)',
    @on_success_action=3,
    @on_fail_action=1,
    @subsystem=N'TSQL',
    @database_name=N'master',
    @command=N'IF sys.fn_hadr_is_primary_replica(''BizTalkMgmtDb'') = 0
	RAISERROR(''Instance is secondary replica'',11,0)
'

EXEC msdb.dbo.sp_add_jobstep @step_name=N'CheckissuesinallMessageBoxesandDTA',
    @job_name=N'Monitor BizTalk Server (BizTalkMgmtDb)',
    @on_success_action=3,
    @on_fail_action=2,
    @subsystem=N'TSQL',
    @database_name=N'BizTalkMgmtDb',
    @command=N'exec [dbo].[btsmon_Inconsistent]
'

EXEC msdb.dbo.sp_add_jobstep @step_name=N'Generateerrorstringincaseofanyissue',
    @job_name=N'Monitor BizTalk Server (BizTalkMgmtDb)',
    @on_success_action=1,
    @on_fail_action=2,
    @subsystem=N'TSQL',
    @database_name=N'BizTalkMgmtDb',
    @command=N'begin try
            exec [dbo].[btsmon_GenerateErrorString]
end try
begin catch
            declare @error nvarchar(max) = ''The ''''Monitor BizTalk Server (BizTalkMgmtDb)'''' job has detected inconsistencies in the BizTalk databases. '' + ''Run BizTalk Health Monitor to correct them.'' + char(13) + char(10) + + char(13) + char(10) + error_message();

            exec msdb.dbo.sp_send_dbmail
                        @profile_name = ''eservices@wisegrid.net'',
                        @recipients = ''brett.lyons@wisetechglobal.com'',
                        @from_address = ''eHubNoReply@wisecloud.zone'',
                        @subject = ''BizTalk Database Inconsistencies Report'',
                        @body = @error;
end catch
'

EXEC msdb.dbo.sp_add_jobschedule @name=N'WeeklySchedule',
    @job_name=N'Monitor BizTalk Server (BizTalkMgmtDb)',
    @freq_type='8',
    @freq_interval='1',
    @freq_subday_type='1',
    @freq_recurrence_factor='1',
    @active_start_time='000500',
    @enabled=1

EXEC msdb.dbo.sp_add_jobserver @job_name=N'Monitor BizTalk Server (BizTalkMgmtDb)', @server_name=N'(local)'

COMMIT TRANSACTION

------------------------------------------------------------------------
PRINT 'Adding [Operations_OperateOnInstances_OnMaster_BizTalkMsgBoxDb]'

BEGIN TRANSACTION

EXEC msdb.dbo.sp_add_job @job_name=N'Operations_OperateOnInstances_OnMaster_BizTalkMsgBoxDb',
    @description=N'This job is used for administration in multiple MessageBox deployments. It asynchronously performs operational actions (like bulk terminate) on the master MessageBox after it performs them on the subordinate MessageBox.',
    @owner_login_name=N'sa',
    @category_name=N'Database Maintenance',
    @notify_level_email=2,
    @notify_email_operator_name=N'eServices',
    @enabled=1

EXEC msdb.dbo.sp_add_jobstep @step_name=N'CheckIsPrimary',
    @job_name=N'Operations_OperateOnInstances_OnMaster_BizTalkMsgBoxDb',
    @on_success_action=3,
    @on_fail_action=1,
    @subsystem=N'TSQL',
    @database_name=N'master',
    @command=N'IF sys.fn_hadr_is_primary_replica(''BizTalkMgmtDb'') = 0
	RAISERROR(''Instance is secondary replica'',11,0)
'

EXEC msdb.dbo.sp_add_jobstep @step_name=N'OperateOnInstsOnMaster',
    @job_name=N'Operations_OperateOnInstances_OnMaster_BizTalkMsgBoxDb',
    @on_success_action=1,
    @on_fail_action=2,
    @subsystem=N'TSQL',
    @database_name=N'BizTalkMsgBoxDb',
    @command=N'exec ops_OperateOnInstancesOnMasterMsgBox ''biztalk.db.wisegrid.net'', ''BizTalkMsgBoxDb''
'

EXEC msdb.dbo.sp_add_jobschedule @name=N'sched',
    @job_name=N'Operations_OperateOnInstances_OnMaster_BizTalkMsgBoxDb',
    @freq_type='4',
    @freq_interval='1',
    @freq_subday_type='4',
    @freq_subday_interval='1',
    @enabled=1

EXEC msdb.dbo.sp_add_jobserver @job_name=N'Operations_OperateOnInstances_OnMaster_BizTalkMsgBoxDb', @server_name=N'(local)'

COMMIT TRANSACTION

------------------------------------------------------------------------
PRINT 'Adding [OutputFileCleanup]'

BEGIN TRANSACTION

EXEC msdb.dbo.sp_add_job @job_name=N'Output File Cleanup',
    @description=N'Source: https://ola.hallengren.com',
    @owner_login_name=N'sa',
    @category_name=N'Database Maintenance',
    @notify_level_email=2,
    @notify_email_operator_name=N'eServices',
    @enabled=1

DECLARE @OutputFileCleanup_0_OutputFile nvarchar(max) = CONCAT(N'', '$', '(ESCAPE_SQUOTE(SQLLOGDIR))\', '$', '(ESCAPE_SQUOTE(JOBNAME))_', '$', '(ESCAPE_SQUOTE(STEPID))_', '$', '(ESCAPE_SQUOTE(DATE))_', '$', '(ESCAPE_SQUOTE(TIME)).txt')
DECLARE @OutputFileCleanup_0_Command nvarchar(max) = CONCAT(N'cmd /q /c "For /F "tokens=1 delims=" %v In (''ForFiles /P "', '$', '(ESCAPE_SQUOTE(SQLLOGDIR))" /m *_*_*_*.txt /d -30 2^>^&1'') do if EXIST "', '$', '(ESCAPE_SQUOTE(SQLLOGDIR))"\%v echo del "', '$', '(ESCAPE_SQUOTE(SQLLOGDIR))"\%v& del "', '$', '(ESCAPE_SQUOTE(SQLLOGDIR))"\%v"
')
EXEC msdb.dbo.sp_add_jobstep @step_name=N'OutputFileCleanup',
    @job_name=N'Output File Cleanup',
    @on_success_action=1,
    @on_fail_action=2,
    @output_file_name=@OutputFileCleanup_0_OutputFile,
    @subsystem=N'CmdExec',
    @command=@OutputFileCleanup_0_Command

EXEC msdb.dbo.sp_add_jobschedule @name=N'Output_File_Cleanup',
    @job_name=N'Output File Cleanup',
    @freq_type='8',
    @freq_interval='1',
    @freq_subday_type='1',
    @freq_recurrence_factor='1',
    @enabled=1

EXEC msdb.dbo.sp_add_jobserver @job_name=N'Output File Cleanup', @server_name=N'(local)'

COMMIT TRANSACTION

------------------------------------------------------------------------
PRINT 'Adding [PurgeSubscriptionsJob_BizTalkMsgBoxDb]'

BEGIN TRANSACTION

EXEC msdb.dbo.sp_add_job @job_name=N'PurgeSubscriptionsJob_BizTalkMsgBoxDb',
    @description=N'This job purges unused subscription predicates from the BizTalk MessageBox database.',
    @owner_login_name=N'PROD\mxu',
    @category_name=N'Database Maintenance',
    @notify_level_email=2,
    @notify_email_operator_name=N'eServices',
    @enabled=1

EXEC msdb.dbo.sp_add_jobstep @step_name=N'CheckIsPrimary',
    @job_name=N'PurgeSubscriptionsJob_BizTalkMsgBoxDb',
    @on_success_action=3,
    @on_fail_action=1,
    @subsystem=N'TSQL',
    @database_name=N'master',
    @command=N'IF sys.fn_hadr_is_primary_replica(''BizTalkMgmtDb'') = 0
	RAISERROR(''Instance is secondary replica'',11,0)
'

EXEC msdb.dbo.sp_add_jobstep @step_name=N'step1',
    @job_name=N'PurgeSubscriptionsJob_BizTalkMsgBoxDb',
    @on_success_action=1,
    @on_fail_action=2,
    @subsystem=N'TSQL',
    @database_name=N'BizTalkMsgBoxDb',
    @command=N'exec bts_PurgeSubscriptions
'

EXEC msdb.dbo.sp_add_jobschedule @name=N'sched',
    @job_name=N'PurgeSubscriptionsJob_BizTalkMsgBoxDb',
    @freq_type='4',
    @freq_interval='1',
    @freq_subday_type='4',
    @freq_subday_interval='1',
    @enabled=1

EXEC msdb.dbo.sp_add_jobserver @job_name=N'PurgeSubscriptionsJob_BizTalkMsgBoxDb', @server_name=N'(local)'

COMMIT TRANSACTION

------------------------------------------------------------------------
PRINT 'Adding [sp_purge_jobhistory]'

BEGIN TRANSACTION

EXEC msdb.dbo.sp_add_job @job_name=N'sp_purge_jobhistory',
    @description=N'Source: https://ola.hallengren.com',
    @owner_login_name=N'sa',
    @category_name=N'Database Maintenance',
    @notify_level_email=2,
    @notify_email_operator_name=N'eServices',
    @enabled=1

DECLARE @sp_purge_jobhistory_0_OutputFile nvarchar(max) = CONCAT(N'', '$', '(ESCAPE_SQUOTE(SQLLOGDIR))\', '$', '(ESCAPE_SQUOTE(JOBNAME))_', '$', '(ESCAPE_SQUOTE(STEPID))_', '$', '(ESCAPE_SQUOTE(DATE))_', '$', '(ESCAPE_SQUOTE(TIME)).txt')
EXEC msdb.dbo.sp_add_jobstep @step_name=N'sp_purge_jobhistory',
    @job_name=N'sp_purge_jobhistory',
    @on_success_action=1,
    @on_fail_action=2,
    @output_file_name=@sp_purge_jobhistory_0_OutputFile,
    @subsystem=N'TSQL',
    @database_name=N'msdb',
    @command=N'DECLARE @CleanupDate datetime
SET @CleanupDate = DATEADD(dd,-30,GETDATE())
EXECUTE dbo.sp_purge_jobhistory @oldest_date = @CleanupDate
'

EXEC msdb.dbo.sp_add_jobschedule @name=N'Cleanup',
    @job_name=N'sp_purge_jobhistory',
    @freq_type='8',
    @freq_interval='1',
    @freq_subday_type='1',
    @freq_recurrence_factor='1',
    @enabled=1

EXEC msdb.dbo.sp_add_jobserver @job_name=N'sp_purge_jobhistory', @server_name=N'(local)'

COMMIT TRANSACTION

------------------------------------------------------------------------
PRINT 'Adding [SSISFailoverMonitorJob]'

BEGIN TRANSACTION

EXEC msdb.dbo.sp_add_job @job_name=N'SSIS Failover Monitor Job',
    @description=N'Runs every 2 minutes. This job execute master.dbo.sp_ssis_startup if detect AlwaysOn failover on SSISDB.',
    @owner_login_name=N'sa',
    @category_name=N'[Uncategorized (Local)]',
    @notify_level_email=2,
    @notify_email_operator_name=N'eServices',
    @enabled=1

EXEC msdb.dbo.sp_add_jobstep @step_name=N'CheckIsPrimary',
    @job_name=N'SSIS Failover Monitor Job',
    @on_success_action=3,
    @on_fail_action=1,
    @subsystem=N'TSQL',
    @database_name=N'master',
    @command=N'IF sys.fn_hadr_is_primary_replica(''BizTalkMgmtDb'') = 0
	RAISERROR(''Instance is secondary replica'',11,0)
'

EXEC msdb.dbo.sp_add_jobstep @step_name=N'AlwaysOnFailoverMonitor',
    @job_name=N'SSIS Failover Monitor Job',
    @on_success_action=1,
    @on_fail_action=2,
    @retry_attempts=3,
    @retry_interval=3,
    @subsystem=N'TSQL',
    @database_name=N'msdb',
    @command=N'
	DECLARE @role int
	DECLARE @status tinyint
	SET @role = (SELECT [role] FROM [sys].[dm_hadr_availability_replica_states] hars INNER JOIN [sys].[availability_databases_cluster] adc ON hars.[group_id] = adc.[group_id] WHERE hars.[is_local] = 1 AND adc.[database_name] =''SSISDB'')
	IF @role = 1
	BEGIN
		EXEC [SSISDB].[internal].[refresh_replica_status] @server_name = @@SERVERNAME, @status = @status OUTPUT
		IF @status = 1
			EXEC [SSISDB].[catalog].[startup]
	END
'

EXEC msdb.dbo.sp_add_jobschedule @name=N'Monitor Scheduler',
    @job_name=N'SSIS Failover Monitor Job',
    @freq_type='4',
    @freq_interval='1',
    @freq_subday_type='4',
    @freq_subday_interval='2',
    @enabled=1

EXEC msdb.dbo.sp_add_jobserver @job_name=N'SSIS Failover Monitor Job', @server_name=N'(local)'

COMMIT TRANSACTION

------------------------------------------------------------------------
PRINT 'Adding [SSISServerMaintenanceJob]'

BEGIN TRANSACTION

EXEC msdb.dbo.sp_add_job @job_name=N'SSIS Server Maintenance Job',
    @description=N'Runs every day. The job removes operation records from the database that are outside the retention window and maintains a maximum number of versions per project.',
    @owner_login_name=N'sa',
    @category_name=N'[Uncategorized (Local)]',
    @notify_level_email=2,
    @notify_email_operator_name=N'eServices',
    @enabled=1

EXEC msdb.dbo.sp_add_jobstep @step_name=N'CheckIsPrimary',
    @job_name=N'SSIS Server Maintenance Job',
    @on_success_action=3,
    @on_fail_action=1,
    @subsystem=N'TSQL',
    @database_name=N'master',
    @command=N'IF sys.fn_hadr_is_primary_replica(''BizTalkMgmtDb'') = 0
	RAISERROR(''Instance is secondary replica'',11,0)
'

EXEC msdb.dbo.sp_add_jobstep @step_name=N'SSISServerOperationRecordsMaintenance',
    @job_name=N'SSIS Server Maintenance Job',
    @on_success_action=3,
    @on_fail_action=2,
    @retry_attempts=3,
    @retry_interval=3,
    @subsystem=N'TSQL',
    @database_name=N'msdb',
    @command=N'
	DECLARE @role int
	SET @role = (SELECT [role] FROM [sys].[dm_hadr_availability_replica_states] hars INNER JOIN [sys].[availability_databases_cluster] adc ON hars.[group_id] = adc.[group_id] WHERE hars.[is_local] = 1 AND adc.[database_name] =''SSISDB'')
	IF DB_ID(''SSISDB'') IS NOT NULL AND (@role IS NULL OR @role = 1)
		EXEC [SSISDB].[internal].[cleanup_server_retention_window]
'

EXEC msdb.dbo.sp_add_jobstep @step_name=N'SSISServerMaxVersionPerProjectMaintenance',
    @job_name=N'SSIS Server Maintenance Job',
    @on_success_action=1,
    @on_fail_action=2,
    @retry_attempts=3,
    @retry_interval=3,
    @subsystem=N'TSQL',
    @database_name=N'msdb',
    @command=N'
	DECLARE @role int
	SET @role = (SELECT [role] FROM [sys].[dm_hadr_availability_replica_states] hars INNER JOIN [sys].[availability_databases_cluster] adc ON hars.[group_id] = adc.[group_id] WHERE hars.[is_local] = 1 AND adc.[database_name] =''SSISDB'')
	IF DB_ID(''SSISDB'') IS NOT NULL AND (@role IS NULL OR @role = 1)
		EXEC [SSISDB].[internal].[cleanup_server_project_version]
'

EXEC msdb.dbo.sp_add_jobschedule @name=N'SSISDB Scheduler',
    @job_name=N'SSIS Server Maintenance Job',
    @freq_type='4',
    @freq_interval='1',
    @freq_subday_type='1',
    @active_end_time='120000',
    @enabled=1

EXEC msdb.dbo.sp_add_jobserver @job_name=N'SSIS Server Maintenance Job', @server_name=N'(local)'

COMMIT TRANSACTION

------------------------------------------------------------------------
PRINT 'Adding [syspolicy_purge_history]'

BEGIN TRANSACTION

EXEC msdb.dbo.sp_add_job @job_name=N'syspolicy_purge_history',
    @description=N'No description available.',
    @owner_login_name=N'sa',
    @category_name=N'[Uncategorized (Local)]',
    @enabled=1

EXEC msdb.dbo.sp_add_jobstep @step_name=N'Verifythatautomationisenabled',
    @job_name=N'syspolicy_purge_history',
    @on_success_action=3,
    @on_fail_action=1,
    @subsystem=N'TSQL',
    @database_name=N'master',
    @command=N'IF (msdb.dbo.fn_syspolicy_is_automation_enabled() != 1)
        BEGIN
            RAISERROR(34022, 16, 1)
        END
'

EXEC msdb.dbo.sp_add_jobstep @step_name=N'Purgehistory',
    @job_name=N'syspolicy_purge_history',
    @on_success_action=3,
    @on_fail_action=2,
    @subsystem=N'TSQL',
    @database_name=N'master',
    @command=N'EXEC msdb.dbo.sp_syspolicy_purge_history
'

DECLARE @syspolicy_purge_history_2_Command nvarchar(max) = CONCAT(N'if (''', '$', '(ESCAPE_SQUOTE(INST))'' -eq ''MSSQLSERVER'') {$a = ''\DEFAULT''} ELSE {$a = ''''};
(Get-Item SQLSERVER:\SQLPolicy\', '$', '(ESCAPE_NONE(SRVR))$a).EraseSystemHealthPhantomRecords()
')
EXEC msdb.dbo.sp_add_jobstep @step_name=N'ErasePhantomSystemHealthRecords',
    @job_name=N'syspolicy_purge_history',
    @on_success_action=1,
    @on_fail_action=2,
    @subsystem=N'PowerShell',
    @database_name=N'master',
    @command=@syspolicy_purge_history_2_Command

EXEC msdb.dbo.sp_add_jobschedule @name=N'syspolicy_purge_history_schedule',
    @job_name=N'syspolicy_purge_history',
    @freq_type='4',
    @freq_interval='1',
    @freq_subday_type='1',
    @active_start_time='020000',
    @enabled=1

EXEC msdb.dbo.sp_add_jobserver @job_name=N'syspolicy_purge_history', @server_name=N'(local)'

COMMIT TRANSACTION

------------------------------------------------------------------------
PRINT 'Adding [TrackedMessages_Copy_BizTalkMsgBoxDb]'

BEGIN TRANSACTION

EXEC msdb.dbo.sp_add_job @job_name=N'TrackedMessages_Copy_BizTalkMsgBoxDb',
    @description=N'This job copies the messages bodies of tracked messages from the BizTalk MessageBox database to the BizTalk Tracking database.',
    @owner_login_name=N'sa',
    @category_name=N'Database Maintenance',
    @notify_level_email=2,
    @notify_email_operator_name=N'eServices',
    @enabled=1

EXEC msdb.dbo.sp_add_jobstep @step_name=N'CheckIsPrimary',
    @job_name=N'TrackedMessages_Copy_BizTalkMsgBoxDb',
    @on_success_action=3,
    @on_fail_action=1,
    @subsystem=N'TSQL',
    @database_name=N'master',
    @command=N'IF sys.fn_hadr_is_primary_replica(''BizTalkMgmtDb'') = 0
	RAISERROR(''Instance is secondary replica'',11,0)
'

EXEC msdb.dbo.sp_add_jobstep @step_name=N'Purge',
    @job_name=N'TrackedMessages_Copy_BizTalkMsgBoxDb',
    @on_success_action=1,
    @on_fail_action=2,
    @subsystem=N'TSQL',
    @database_name=N'BizTalkMsgBoxDb',
    @command=N'exec bts_CopyTrackedMessagesToDTA ''biztalk.db.wisegrid.net'', ''BizTalkDTADb''
'

EXEC msdb.dbo.sp_add_jobschedule @name=N'Schedule',
    @job_name=N'TrackedMessages_Copy_BizTalkMsgBoxDb',
    @freq_type='4',
    @freq_interval='1',
    @freq_subday_type='4',
    @freq_subday_interval='1',
    @enabled=1

EXEC msdb.dbo.sp_add_jobserver @job_name=N'TrackedMessages_Copy_BizTalkMsgBoxDb', @server_name=N'(local)'

COMMIT TRANSACTION



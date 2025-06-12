USE [msdb]
SET XACT_ABORT ON

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
WHERE StartTime < DATEADD(dd,-30,GETDATE())'

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
PRINT 'Updating [BackupBizTalkServerBizTalkMgmtDb]'

BEGIN TRANSACTION

EXEC msdb.dbo.sp_delete_job @job_name=N'Backup BizTalk Server (BizTalkMgmtDb)', @delete_unused_schedule=1

EXEC msdb.dbo.sp_add_job @job_name=N'Backup BizTalk Server (BizTalkMgmtDb)',
    @description=N'This job performs full database backups (step 1) and log backups (step 2) of BizTalk Server databases.',
    @owner_login_name=N'sa',
    @category_name=N'Database Maintenance',
    @enabled=0

EXEC msdb.dbo.sp_add_jobstep @step_name=N'SetCompressionOption',
    @job_name=N'Backup BizTalk Server (BizTalkMgmtDb)',
    @on_success_action=3,
    @on_fail_action=3,
    @subsystem=N'TSQL',
    @database_name=N'BizTalkMgmtDb',
    @command=N'exec [dbo].[sp_SetBackupCompression] @bCompression = 1 /*0 - Do not use Compression, 1 - Use Compression */'

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
    @command=N'-- exec [dbo].[sp_MarkAll] ''BTS'' /*  Log mark name */, ''<destination path>'' /* location of backup files */'

EXEC msdb.dbo.sp_add_jobstep @step_name=N'ClearBackupHistory',
    @job_name=N'Backup BizTalk Server (BizTalkMgmtDb)',
    @on_success_action=1,
    @on_fail_action=2,
    @subsystem=N'TSQL',
    @database_name=N'BizTalkMgmtDb',
    @command=N'exec [dbo].[sp_DeleteBackupHistoryAndMarkLogsHistory] @DaysToKeep=14'

EXEC msdb.dbo.sp_add_jobschedule @name=N'MarkAndBackupLogSched',
    @job_name=N'Backup BizTalk Server (BizTalkMgmtDb)',
    @freq_type='4',
    @freq_interval='1',
    @freq_subday_type='4',
    @freq_subday_interval='15',
    @enabled=1

EXEC msdb.dbo.sp_add_jobserver @job_name=N'Backup BizTalk Server (BizTalkMgmtDb)', @server_name=N'(local)'

COMMIT TRANSACTION


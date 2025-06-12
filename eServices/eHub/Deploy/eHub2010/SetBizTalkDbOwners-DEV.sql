USE BAMArchive
GO
EXEC sp_changedbowner @loginame='sa'
GO
USE BAMPrimaryImport
GO
EXEC sp_dropuser @name_in_db='CORPORATE\biztalkportalappdev'
GO
EXEC sp_changedbowner @loginame='CORPORATE\biztalkportalappdev'
GO
USE BAMStarSchema
GO
EXEC sp_changedbowner @loginame='sa'
GO
USE BizTalkDTADb
GO
EXEC sp_changedbowner @loginame='sa'
GO
USE BizTalkMgmtDb
GO
EXEC sp_changedbowner @loginame='sa'
GO
USE BizTalkMsgBoxDb
GO
EXEC sp_changedbowner @loginame='sa'
GO
USE SSODB
GO
EXEC sp_changedbowner @loginame='sa'
GO
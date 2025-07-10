CREATE TABLE ProcessorStatus
(
	PRC_PK uniqueidentifier NOT NULL constraint DF_ProcessorStatus DEFAULT NEWID(),
	PRC_SchedName nvarchar(120) NOT NULL,
	PRC_JobName nvarchar(150) NOT NULL,
	PRC_JobGroup nvarchar(150) NOT NULL,
	PRC_LastRunTime datetime2 NOT NULL,
	PRC_LastSuccessRunTime datetime2,
	PRC_LastSuccessRecordUpdatedCount int,
	PRC_LastDataSetUpdatedTime datetime2,
	PRC_Status VARCHAR(3) NOT NULL CONSTRAINT DF_ProcessorStatus_PRC_Status DEFAULT 'PRS'
	CONSTRAINT PK_ProcessorStatus PRIMARY KEY CLUSTERED (PRC_PK),
	CONSTRAINT FK_ProcessorStatus_QrtzJobDetails FOREIGN KEY (PRC_SchedName, PRC_JobName, PRC_JobGroup) REFERENCES QRTZ_JOB_DETAILS (SCHED_NAME, JOB_NAME, JOB_GROUP) ON DELETE CASCADE,
)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_ProcessorStatus_PRC_SchedName_PRC_JobGroup_PRC_JobName ON ProcessorStatus (PRC_SchedName, PRC_JobGroup, PRC_JobName)
GO

CREATE TRIGGER ProcessorStatus_Modify
ON ProcessorStatus
FOR UPDATE, INSERT
AS
	INSERT INTO DataChangeCapture(DCC_Column, DCC_EventTimeUTC, DCC_NewValue, DCC_OldValue, DCC_ParentCode, DCC_ParentPK, DCC_PK)
	SELECT 'PRC_Status', SYSUTCDATETIME(), inserted.PRC_Status, deleted.PRC_Status, 'PRC', inserted.PRC_PK, NEWID() FROM inserted
	LEFT JOIN deleted ON deleted.PRC_PK = inserted.PRC_PK
	WHERE inserted.PRC_Status = 'ERR' AND (deleted.PRC_Status IS NULL OR deleted.PRC_Status <> inserted.PRC_Status)

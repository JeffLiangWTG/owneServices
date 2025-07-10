CREATE TABLE [dbo].[DataSetChangeHistory]
(
	DCH_PK uniqueidentifier NOT NULL CONSTRAINT DF_DataSetChangeHistory_DCH_PK DEFAULT newid(),
	DCH_User VARCHAR(50) CONSTRAINT DF_DataSetChangeHistory_DCH_LastEditedUser DEFAULT dbo.GetUserId(),
	DCH_ChangeTime datetime2 NOT NULL CONSTRAINT DF_DataSetChangeHistory_DCH_ChangeTime DEFAULT SYSUTCDATETIME(),
	DCH_ParentCode VARCHAR(3) NOT NULL,
	DCH_ParentPK uniqueidentifier NOT NULL,
	DCH_TransactionId bigint NOT NULL,
	DCH_TransactionIdStartTime datetime NOT NULL CONSTRAINT DF_DataSetChangeHistory_DCH_TransactionIdStartTime DEFAULT '1900-01-01'
)
GO
CREATE UNIQUE NONCLUSTERED INDEX UX_DataSetChangeHistory_DCH_ParentCode_DCH_ParentPK_DCH_TransactionIdLevel_DCH_TransactionIdStartTime ON DataSetChangeHistory (DCH_ParentCode, DCH_ParentPK, DCH_TransactionIdStartTime, DCH_TransactionId)
GO
ALTER TABLE DataSetChangeHistory SET (LOCK_ESCALATION = DISABLE);

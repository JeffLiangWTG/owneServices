CREATE TABLE [dbo].[DataPushSubscription]
(
	DPS_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_DataPushSubscription_DPS_PK DEFAULT newid(),
	DPS_ClientRecipientId VARCHAR(30) NOT NULL,
	DPS_DataSetId SMALLINT NOT NULL,
	
	CONSTRAINT PK_DataPushSubscription PRIMARY KEY CLUSTERED (DPS_PK ASC)
)
GO
CREATE UNIQUE NONCLUSTERED INDEX UX_DataPushSubscription_DPS_ClientRecipientId_DPS_DataSetId ON DataPushSubscription (DPS_ClientRecipientId, DPS_DataSetId)
GO
ALTER TABLE DataPushSubscription SET (LOCK_ESCALATION = DISABLE);

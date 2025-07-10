CREATE TABLE [ClientRefDbVersionControl]
(
	[CVC_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_ClientRefDbVersionControl_CVC_PK] DEFAULT NEWID(),
	[CVC_DataSet] VARCHAR(50) NOT NULL,
	[CVC_ClientId] VARCHAR(50) NOT NULL,
	[CVC_DataSetTimestamp] DATETIME2,
	[CVC_DataSetCheckpoint] VARCHAR(400),
	[CVC_SystemType] VARCHAR(3),
	[CVC_LastUpdatedTimeUTC] DATETIME2,
	[CVC_IsInUse] BIT NOT NULL CONSTRAINT [DF_ClientRefDbVersionControl_CVC_IsInUse] DEFAULT 1,
	CONSTRAINT [PK_ClientRefDbVersionControl] PRIMARY KEY CLUSTERED ([CVC_PK])
)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_ClientRefDbVersionControl_CVC_DataSet_CVC_ClientId_CVC_IsInUse ON ClientRefDbVersionControl (CVC_DataSet, CVC_ClientId, CVC_IsInUse)
GO
ALTER TABLE ClientRefDbVersionControl SET (LOCK_ESCALATION = DISABLE);

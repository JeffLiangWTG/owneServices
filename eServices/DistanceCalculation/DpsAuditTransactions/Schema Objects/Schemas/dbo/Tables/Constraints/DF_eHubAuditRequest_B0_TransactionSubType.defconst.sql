ALTER TABLE [dbo].[eHubAuditRequest]
    ADD CONSTRAINT [DF_eHubAuditRequest_B0_TransactionSubType] DEFAULT ('') FOR [B0_TransactionSubType];


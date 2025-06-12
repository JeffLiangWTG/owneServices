ALTER TABLE [dbo].[eHubAuditRequest]
    ADD CONSTRAINT [DF_eHubAuditRequest_B0_TransactionType] DEFAULT ('') FOR [B0_TransactionType];


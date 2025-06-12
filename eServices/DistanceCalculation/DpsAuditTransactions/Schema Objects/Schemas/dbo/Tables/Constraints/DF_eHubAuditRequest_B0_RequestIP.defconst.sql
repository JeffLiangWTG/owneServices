ALTER TABLE [dbo].[eHubAuditRequest]
    ADD CONSTRAINT [DF_eHubAuditRequest_B0_RequestIP] DEFAULT ('') FOR [B0_RequestIP];


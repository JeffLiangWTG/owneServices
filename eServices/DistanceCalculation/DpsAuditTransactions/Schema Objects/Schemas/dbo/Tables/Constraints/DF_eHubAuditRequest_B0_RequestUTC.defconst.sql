ALTER TABLE [dbo].[eHubAuditRequest]
    ADD CONSTRAINT [DF_eHubAuditRequest_B0_RequestUTC] DEFAULT (getutcdate()) FOR [B0_RequestUTC];


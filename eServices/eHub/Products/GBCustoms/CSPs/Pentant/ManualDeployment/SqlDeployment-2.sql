SET NOCOUNT ON;

BEGIN TRANSACTION

-- --------------------------------------------------------------------------------------------------------------------
-- 00. Declare parameters.
-- --------------------------------------------------------------------------------------------------------------------

DECLARE @PentantSubsciptionExpiryDays INT = 180

-- --------------------------------------------------------------------------------------------------------------------
-- 01. Create subscription types.
-- --------------------------------------------------------------------------------------------------------------------

DECLARE @PentantSubscriptionTypePk UNIQUEIDENTIFIER = 'CD4A23DA-A726-4C4A-99FD-F1ACE758643F'

INSERT INTO eHubTransactions..eHubSubscriptionType(ST_PK, ST_ID, ST_Name, ST_ExpiryDays)
SELECT @PentantSubscriptionTypePk, 'GBCDPE', 'GB Customs (Pentant) Conversation ID', @PentantSubsciptionExpiryDays

PRINT '1> Created GB Customs (Pentant) Conversation ID subscription types...'

-- COMMIT TRANSACTION
ROLLBACK TRANSACTION
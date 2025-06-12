SET NOCOUNT ON;

BEGIN TRANSACTION

-- --------------------------------------------------------------------------------------------------------------------
-- 00. Declare parameters.
-- --------------------------------------------------------------------------------------------------------------------

DECLARE @CnsSubsciptionExpiryDays INT = 180

-- --------------------------------------------------------------------------------------------------------------------
-- 01. Create subscription types.
-- --------------------------------------------------------------------------------------------------------------------

DECLARE @CnsSubscriptionTypePk UNIQUEIDENTIFIER = 'BF944017-A4F3-492B-976E-2B814B016F5F'

INSERT INTO eHubTransactions..eHubSubscriptionType(ST_PK, ST_ID, ST_Name, ST_ExpiryDays)
SELECT @CnsSubscriptionTypePk, 'GBCCNC', 'GB Customs (CNS) Conversation ID', @CnsSubsciptionExpiryDays

PRINT '1> Created GB Customs (CNS) Conversation ID subscription types...'

-- COMMIT TRANSACTION
ROLLBACK TRANSACTION
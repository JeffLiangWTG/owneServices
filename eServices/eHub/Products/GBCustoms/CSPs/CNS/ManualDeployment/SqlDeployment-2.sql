SET NOCOUNT ON;

BEGIN TRANSACTION

-- --------------------------------------------------------------------------------------------------------------------
-- 00. Declare parameters.
-- --------------------------------------------------------------------------------------------------------------------

DECLARE @CnsSubsciptionExpiryDays INT = 180

-- --------------------------------------------------------------------------------------------------------------------
-- 01. Create subscription types.
-- --------------------------------------------------------------------------------------------------------------------

DECLARE @CnsSubscriptionTypePk UNIQUEIDENTIFIER = '26DF989A-85F7-4A75-8B7D-75E1426C47EC'

INSERT INTO eHubTransactions..eHubSubscriptionType(ST_PK, ST_ID, ST_Name, ST_ExpiryDays)
SELECT @CnsSubscriptionTypePk, 'GBCCNS', 'GB Customs (CNS) X-CNS-ID', @CnsSubsciptionExpiryDays

PRINT '1> Created X-CNS-ID subscription types...'

-- COMMIT TRANSACTION
ROLLBACK TRANSACTION
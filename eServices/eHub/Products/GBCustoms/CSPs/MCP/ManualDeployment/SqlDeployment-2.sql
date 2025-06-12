SET NOCOUNT ON;

BEGIN TRANSACTION

-- --------------------------------------------------------------------------------------------------------------------
-- 00. Declare parameters.
-- --------------------------------------------------------------------------------------------------------------------

DECLARE @McpSubsciptionExpiryDays INT = 180

-- --------------------------------------------------------------------------------------------------------------------
-- 01. Create subscription types.
-- --------------------------------------------------------------------------------------------------------------------

DECLARE @McpSubscriptionTypePk UNIQUEIDENTIFIER = '977e5e18-9b92-4669-b564-a7a4ec55c2a1'

INSERT INTO eHubTransactions..eHubSubscriptionType(ST_PK, ST_ID, ST_Name, ST_ExpiryDays)
SELECT @McpSubscriptionTypePk, 'GBCMCP', 'GB Customs (MCP) X-MCP-ID', @McpSubsciptionExpiryDays

PRINT '1> Created X-MCP-ID subscription types...'

-- COMMIT TRANSACTION
ROLLBACK TRANSACTION
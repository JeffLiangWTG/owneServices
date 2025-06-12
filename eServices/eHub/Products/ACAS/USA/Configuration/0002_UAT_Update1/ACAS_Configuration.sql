USE eHubTransactions
GO

SET XACT_ABORT ON

BEGIN TRANSACTION


--Clients
DECLARE @CC_ADVANCE_AIR_CARGO_REPORT uniqueidentifier = '7f31d6d3-91ce-44b6-9a1c-a6e808d99992'
DECLARE @CC_ACAS_US uniqueidentifier = 'ec29559d-57aa-4563-9f59-072048e4e16c'
DECLARE @CC_ACAS_US_FRI uniqueidentifier = 'd6657f37-81e6-4b79-9db1-f642e02649a4'
DECLARE @CC_ACAS_US_FRI_TST uniqueidentifier = 'ab952e80-5245-44a9-9319-251b55908143'
DECLARE @CC_ACAS_US_FHL uniqueidentifier = '093e64da-280d-464c-b49b-fef252ee2a38'
DECLARE @CC_ACAS_US_FHL_TST uniqueidentifier = 'eed40a1b-df35-47f8-8709-771a28c5967e'
DECLARE @CC_ACAS_US_ASN uniqueidentifier = '6ad89007-a140-4065-adb9-7f9d45c59367'
DECLARE @CC_ACAS_US_ASN_TST uniqueidentifier = '9952ee2f-d7e5-4aef-892d-cf1cab133f19'

--Routing Rules
DECLARE @RR_ADVANCE_AIR_CARGO_REPORT uniqueidentifier = '1526bad0-61fb-41b3-830a-3ae92b24942d'
DECLARE @RR_SP_ACAS_US uniqueidentifier = '286e90c2-8541-4845-ac4f-f6a416829478'
DECLARE @RR_DEST_US uniqueidentifier = 'b4a3a586-649f-45a4-b069-67566d307cb5'
DECLARE @SP_ACAS_US uniqueidentifier = '9c3ecde2-c59e-4438-9291-084251a91d00'

--Registration Type 'ACAS_US'
DECLARE @RT_ACAS_US uniqueidentifier = 'f5721967-bd3b-450d-93c0-794ed4626ce6'

--Subscription Type 'ACASUS'
DECLARE @ST_ACASUS uniqueidentifier = '1526bad0-61fb-41b3-830a-3ae92b24942d'



-- Message Types




-- Transformations




-- Fix Routing Rules
/*
-- already applied to test server

UPDATE [dbo].[eHubRoutingRuleFact]
   SET [RX_Type] = 'XPATHNAV'
 WHERE [RX_RR] = @RR_ADVANCE_AIR_CARGO_REPORT
   AND [RX_Type] = 'XPATH'

UPDATE [dbo].[eHubRoutingRuleFact]
   SET [RX_Query] = '/*[local-name()=''UniversalInterchange'']/*[local-name()=''Body'']/*[local-name()=''UniversalShipment'']/*[local-name()=''Shipment'']/*[local-name()=''PortOfFirstArrival'']'
 WHERE [RX_PK] = '42278924-f5e8-40b3-a41f-be87239c4697'

*/

--COMMIT

WHILE @@TRANCOUNT > 0
	ROLLBACK

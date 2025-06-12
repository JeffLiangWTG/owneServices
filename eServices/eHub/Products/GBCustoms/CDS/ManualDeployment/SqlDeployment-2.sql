USE eHubTransactions

SET XACT_ABORT ON

BEGIN TRAN

--eHubRegistrationType

DECLARE @RT_PK1 uniqueIdentifier = '84B70B52-1471-4EBE-A7CB-851E41F07964' -- NEWID()
DECLARE @RT_PK2 uniqueIdentifier = '2D3D4631-DAA2-42D2-AEBE-32C1FDB45EF8' -- NEWID()

INSERT INTO eHubTransactions..eHubRegistrationType([RT_PK], [RT_ID], [RT_Description], [RT_RegistrantType])
SELECT @RT_PK1, N'GBCustomsAuthorisationToken', 'GB Customs Cilent Authorisation Token', 'ClientSystem' UNION ALL
SELECT @RT_PK2, N'GBCustomsAccessToken', 'GB Customs Cilent Access Token', 'ClientSystem'

-- Subscriptions

DECLARE @GBCCID_SubscriptionType uniqueIdentifier = '5549594E-F3E2-44A7-A8B7-90D2BE7A337D'
 
INSERT INTO [eHubTransactions].[dbo].[eHubSubscriptionType] ([ST_PK],[ST_ID],[ST_Name],[ST_ExpiryDays])
     VALUES (@GBCCID_SubscriptionType, 'GBCCID', 'GB Customs Conversation ID', 180)

-- Message Types
INSERT INTO [dbo].[eHubMessageType] ([DT_PK], [DT_Code], [DT_IsFlatFile], [DT_IsEDI], [DT_Charset], [DT_EnvelopeXpath], [DT_DT_InnerType], [DT_ReprocessSubMessage], [DT_PostAssembleMapping], [DT_DT_PostAssembleWrapper], [DT_IsJson]) 
VALUES (N'26451c5f-3f0f-47c2-8e47-3053b0be52a3', 'http://cargowise.com/ehub/products/GBCustoms#GBCustoms', 0, 0, NULL, NULL, NULL, 0, 0, NULL, 0),
	   (N'4dbcb0cf-90a0-4ea8-84ca-32b69b1b3d5a', 'http://cargowise.com/ehub/products/GBCustoms#GBCustomsTransportResponse', 0, 0, NULL, NULL, NULL, 0, 0, NULL, 0),
	   (N'313b842c-2ed9-4092-87f6-20bcc957f94a', 'http://www.wisetechglobal.com/Schemas/Configuration#Configuration', 0, 0, NULL, NULL, NULL, 0, 0, NULL, 0),
	   (N'12f1150e-4f3f-410f-990c-baab866edc29', 'http://cargowise.com/ehub/products/GBCustoms#GBCustomsBusinessResponse', 0, 0, NULL, NULL, NULL, 0, 0, NULL, 0)

-- Transformations

INSERT INTO [dbo].[eHubTransformationSet] ([TS_PK], [TS_Name], [TS_CC_Sender], [TS_CC_Recipient], [TS_DT_Source], [TS_XPathPredicate], [TS_BillingInterfaceName], [TS_BillingElement], [TS_BillingXPathSource], [TS_BillingXPathTarget], [TS_BillSender], [TS_BillRecipient], [TS_CC_BillOther], [TS_BillingNumMessagesIncluded], [TS_BillingFee]) 
	VALUES (N'34fc8110-70da-4822-987b-6ff07f3dbc58', N'GB Customs Send To CDS Test', NULL, N'6b41ece4-aba6-479b-a74f-92b19717f474', N'26451c5f-3f0f-47c2-8e47-3053b0be52a3', NULL, NULL, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL),
		   (N'9d47f6d0-11b2-4e21-b6a5-eb6a00ac595d', N'GB Customs Send To CDS', NULL, N'a09a4c93-17a8-4f86-ba35-fe55c824cb7b', N'26451c5f-3f0f-47c2-8e47-3053b0be52a3', NULL, NULL, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL),
		   (N'B745AD5C-B3D4-4C82-B1F1-A341236C1EE7', N'GB Customs Receive Transport Reponse', NULL, NULL, N'4DBCB0CF-90A0-4EA8-84CA-32B69B1B3D5A', NULL, NULL, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL)

INSERT INTO [dbo].[eHubTransformationType] ([TT_PK], [TT_DT_Source], [TT_DT_Target], [TT_TransformationType], [TT_Target_Version]) 
	VALUES (N'e498e197-6737-46b0-8bf9-f13b26b798f8', N'26451c5f-3f0f-47c2-8e47-3053b0be52a3', N'26451c5f-3f0f-47c2-8e47-3053b0be52a3', 'CargoWise.eHub.Products.GBCustoms.BT.Transforms.GBCustomsToCDS, CargoWise.eHub.Products.GBCustoms.BT.Transforms.GBCustomsToCDS, Version=1.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350', NULL),
		   (N'21F6B924-8438-455D-8C03-0899441DBE1C', N'4DBCB0CF-90A0-4EA8-84CA-32B69B1B3D5A', N'BDFC6E17-5D5C-4E9B-9BAA-C1E016EEF1F4', 'CargoWise.eHub.Products.GBCustoms.BT.Transforms.TransportResponse2UniversalEvent, CargoWise.eHub.Products.GBCustoms.BT.Transforms.TransportResponse2UniversalEvent, Version=1.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350', NULL)

INSERT INTO [dbo].[eHubTransformationMapping] ([TM_TS_PK], [TM_Order], [TM_TT_PK]) 
	VALUES (N'34fc8110-70da-4822-987b-6ff07f3dbc58', 0, N'e498e197-6737-46b0-8bf9-f13b26b798f8'),
		   (N'9d47f6d0-11b2-4e21-b6a5-eb6a00ac595d', 0, N'e498e197-6737-46b0-8bf9-f13b26b798f8'),
		   (N'B745AD5C-B3D4-4C82-B1F1-A341236C1EE7', 0, N'21F6B924-8438-455D-8C03-0899441DBE1C'),
		   (N'B745AD5C-B3D4-4C82-B1F1-A341236C1EE7', 1, N'B4A9D80B-BE4B-419F-9452-A4322794B91C')

-- Update Code Mapping Values

UPDATE eHubTransactions..eHubCodeMapKey SET [CK_Key2Value] = N'CancelDeclaration' WHERE [CK_PK] = N'c509f371-b4f0-4c3b-9d79-532e7be197f6'
UPDATE eHubTransactions..eHubCodeMapKey SET [CK_Key2Value] = N'ExportInventory' WHERE [CK_PK] = N'b928de1f-c23a-4107-946e-f351ec0456bf'
UPDATE eHubTransactions..eHubCodeMapKey SET [CK_Key2Value] = N'DocumentUpload' WHERE [CK_PK] = N'46685e52-6590-4bcc-b4f8-fad5b80757b3'
UPDATE eHubTransactions..eHubCodeMapKey SET [CK_Key2Value] = N'NewDeclaration' WHERE [CK_PK] = N'0df92135-a274-4a0f-bf33-774948be3240'
UPDATE eHubTransactions..eHubCodeMapKey SET [CK_Key2Value] = N'CancelDeclaration' WHERE [CK_PK] = N'f0992c38-366a-42db-bd92-aa2841c16318'
UPDATE eHubTransactions..eHubCodeMapKey SET [CK_Key2Value] = N'ExportInventory' WHERE [CK_PK] = N'b0d1fb35-5ff6-4611-a85c-534949e3ecd2'
UPDATE eHubTransactions..eHubCodeMapKey SET [CK_Key2Value] = N'DocumentUpload' WHERE [CK_PK] = N'b2cc3ffc-8ef7-4b18-a983-986d5ba37141'
UPDATE eHubTransactions..eHubCodeMapKey SET [CK_Key2Value] = N'NewDeclaration' WHERE [CK_PK] = N'38c7be12-ea1c-4324-83c8-8baf9e9dae1b'

ROLLBACK
--COMMIT

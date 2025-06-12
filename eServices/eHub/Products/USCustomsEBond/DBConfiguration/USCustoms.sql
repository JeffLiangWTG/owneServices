use eHubTransactions

set xact_abort on

begin tran

DECLARE @USCustomsEBond uniqueIdentifier = 'EFD4DC4C-817C-4769-81CE-B6508C07EE01' -- NEWID()
DECLARE @USCustomsEBondTest uniqueIdentifier = 'EB69791D-8173-44BF-B263-6FFD24BEB7B1' -- NEWID()

INSERT INTO eHubTransactions..eHubClient
(CC_PK, CC_ID, CC_FriendlyName, CC_Odyssey_OH, CC_DistributionZone, CC_EmailAddress, CC_Password, CC_OwnerCategory, CC_SystemCategory)
select @USCustomsEBond,'USCustomsEBond','USCustoms EBond','00000000-0000-0000-0000-000000000000','75419f4c-c522-4890-bd5d-bca5e12268f6','','','Service Provider','Third Party'

INSERT INTO eHubTransactions..eHubClient
(CC_PK, CC_ID, CC_FriendlyName, CC_Odyssey_OH, CC_DistributionZone, CC_EmailAddress, CC_Password, CC_OwnerCategory, CC_SystemCategory)
select @USCustomsEBondTest,'USCustomsEBondTest','USCustoms EBond Test','00000000-0000-0000-0000-000000000000','75419f4c-c522-4890-bd5d-bca5e12268f6','','','Service Provider','Third Party'

DECLARE @RT_PK1 uniqueIdentifier = '3E7A435F-AAD1-4613-B5F5-2DD3D0AF679E' -- NEWID()
DECLARE @TT_PK uniqueIdentifier = '1244000F-A2FB-4A5B-8EFA-7732B5A7F307' -- NEWID()
DECLARE @SuretyToBrokerMessagePK uniqueIdentifier = '2FDE8256-24ED-4A17-A2FC-5FA3F3ECC7C4' -- NEWID()
DECLARE @BrokerToSuretyMessagePK uniqueIdentifier = 'e2898b9b-4601-4a8e-a62e-52bcea8bce07' -- NEWID()
DECLARE @SoapEnvelope UNIQUEIDENTIFIER = N'e8ac8ce1-b327-4128-852a-832280f21d64'  --(SELECT DT_PK FROM eHubMessageType WHERE DT_Code = 'http://schemas.xmlsoap.org/soap/envelope/#Envelope')

-- Message Types
INSERT INTO [dbo].[eHubMessageType] ([DT_PK], [DT_Code], [DT_IsFlatFile], [DT_IsEDI], [DT_Charset], [DT_EnvelopeXpath], [DT_DT_InnerType], [DT_ReprocessSubMessage], [DT_PostAssembleMapping], [DT_DT_PostAssembleWrapper], [DT_IsJson]) 
VALUES (@BrokerToSuretyMessagePK, 'BrokerToSuretyMessage', 0, 0, NULL, NULL, NULL, 0, 0, NULL, 0)
	  ,(@SuretyToBrokerMessagePK, 'SuretyToBrokerMessage', 0, 0, NULL, NULL, NULL, 0, 0, NULL, 0)

 --Transformations
DECLARE @TransType UNIQUEIDENTIFIER = '5B32FDFA-0175-410D-81E7-859A2EF20F99'		
DECLARE @MessageTypeUniversalInterchangePK UNIQUEIDENTIFIER = '6C73FDC6-0A91-46F2-9EB1-3E98816E87F0' --(SELECT DT_PK FROM eHubMessageType WHERE DT_Code = 'http://www.cargowise.com/Schemas/Universal/2011/11#Universalinterchange')

INSERT INTO [dbo].[eHubTransformationSet] ([TS_PK], [TS_Name], [TS_CC_Sender], [TS_CC_Recipient], [TS_DT_Source]) 
	VALUES (N'E1DC4ED1-B1D8-41E7-B202-110E1D03BD88', N'eBond - SuretyToBroker from USCustoms', @USCustomsEBond, NULL, @SuretyToBrokerMessagePK),
		   (N'6DBDFF28-150F-4373-982C-A69499941313', N'eBond Test - SuretyToBroker from USCustoms', @USCustomsEBondTest, NULL, @SuretyToBrokerMessagePK)

DECLARE @TransSet_Snd UNIQUEIDENTIFIER = '18CEB3DC-9DD8-4950-B7FD-C9D2460EDBDC'				--SELECT NEWID()
DECLARE @TransSetTest_Snd UNIQUEIDENTIFIER = 'D3D1E5A5-3D87-474E-993D-3F0A6A74ADD0'				--SELECT NEWID()

INSERT INTO eHubTransformationSet
(TS_PK, TS_Name, TS_CC_Sender, TS_CC_Recipient, TS_DT_Source)
SELECT @TransSet_Snd,'eBond - Universal Interchange to SoapEnvelope', NULL, @USCustomsEBond, @MessageTypeUniversalInterchangePK UNION ALL
SELECT @TransSetTest_Snd,'eBond Test - Universal Interchange to SoapEnvelope', NULL, @USCustomsEBondTest, @MessageTypeUniversalInterchangePK


INSERT INTO [dbo].[eHubTransformationType] ([TT_PK], [TT_DT_Source], [TT_DT_Target], [TT_TransformationType], [TT_Target_Version]) 
	VALUES (N'1BDD8009-42A9-4907-8DB6-D3D042F97882', @SuretyToBrokerMessagePK, @MessageTypeUniversalInterchangePK, 'CargoWise.eHub.Products.USCustoms.eBond.Transforms.SuretyToBroker2UniversalEvent.SuretyToBroker_to_UniversalEvent, CargoWise.eHub.Products.USCustoms.eBond.Transforms.SuretyToBroker2UniversalEvent, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350', 1),
			(@TransType, @MessageTypeUniversalInterchangePK, @SoapEnvelope, 'CargoWise.eHub.Products.USCustoms.eBond.Transforms.UniversalShipment2BrokerToSurety.UShipment2BrokerToSurety, CargoWise.eHub.Products.USCustoms.eBond.Transforms.UniversalShipment2BrokerToSurety, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350', NULL) 

INSERT INTO [dbo].[eHubTransformationMapping] ([TM_TS_PK], [TM_Order], [TM_TT_PK]) 
	VALUES (N'E1DC4ED1-B1D8-41E7-B202-110E1D03BD88', 0, N'1BDD8009-42A9-4907-8DB6-D3D042F97882'),
		   (N'6DBDFF28-150F-4373-982C-A69499941313', 0, N'1BDD8009-42A9-4907-8DB6-D3D042F97882'),
		   (@TransSet_Snd, 0, @TransType),
		   (@TransSetTest_Snd, 0, @TransType)

 --Subscriptions

DECLARE @SubscriptionType uniqueIdentifier = 'B5C54E60-3600-47F6-8954-041F7ADA4D20'
DECLARE @SubscriptionLookup uniqueIdentifier = 'F3FAC414-BFF0-4F8F-9662-1D243FFBEACE'	-- select NEWID()
 
INSERT INTO [eHubTransactions].[dbo].[eHubSubscriptionType] ([ST_PK],[ST_ID],[ST_Name],[ST_ExpiryDays])
     VALUES (@SubscriptionType, 'USCEB', 'US Customs eBond', 90)

INSERT INTO  [dbo].[eHubSubscriptionLookup](SL_PK, SL_ST, SL_DT, SL_ValueXPath) 
SELECT @SubscriptionLookup, @SubscriptionType, @SuretyToBrokerMessagePK, '/*[local-name()="SuretyToBrokerMessage"]/*[local-name()="MessageLevelResult"]/*[local-name()="BrokerReferenceNumber"]'

INSERT INTO eHubCounter
(CN_Name, CN_Value)
SELECT 'USCustomsEBond.BrokerReferenceNumber', 0


-- Orchestration

DECLARE @TS_PK UNIQUEIDENTIFIER = '54358AC1-7766-45A9-9AA5-A87E7C20FF67'
DECLARE @eBond_CC_PK UNIQUEIDENTIFIER = (SELECT CC_PK FROM eHubClient WHERE CC_ID = 'USCustomsEBond')

INSERT INTO eHubTransformationSet (TS_PK, TS_Name, TS_CC_Recipient)
VALUES (@TS_PK, 'US Customs eBond System Configurations', @eBond_CC_PK)

INSERT INTO [dbo].[eHubCodeSet]([CS_PK], [CS_Name], [CS_TS], [CS_CC_Sender], [CS_CC_Recipient], [CS_Key1Name], [CS_Key2Name], [CS_Key3Name], [CS_Key4Name], [CS_Key5Name])
SELECT N'1153bfb5-c3a4-42a7-8bc8-4e6a8ba16641', N'Connection Details', @TS_PK, @eBond_CC_PK, @eBond_CC_PK, N'ABI Code', N'Test Flag', NULL, NULL, NULL

INSERT INTO [dbo].[eHubCodeMapKey]([CK_PK], [CK_CS], [CK_Order], [CK_Key1Value], [CK_Key2Value], [CK_Key3Value], [CK_Key4Value], [CK_Key5Value])
SELECT N'7566a3c3-8c44-4f20-8382-9f71ba247551', N'1153bfb5-c3a4-42a7-8bc8-4e6a8ba16641', 1, N'WDU', N'Test', NULL, NULL, NULL

INSERT INTO [dbo].[eHubCodeSetResult]([CR_PK], [CR_CS], [CR_Order], [CR_Name])
SELECT N'5d6edc73-123e-4388-9e43-f138188bfc9b', N'1153bfb5-c3a4-42a7-8bc8-4e6a8ba16641', 3, N'Password' UNION ALL
SELECT N'34074c74-117f-4cf3-8936-f1c19669d472', N'1153bfb5-c3a4-42a7-8bc8-4e6a8ba16641', 1, N'URL' UNION ALL
SELECT N'3ffcbc93-d8cb-41ad-a5f2-6e88a1f35334', N'1153bfb5-c3a4-42a7-8bc8-4e6a8ba16641', 2, N'Username'

INSERT INTO [dbo].[eHubCodeMapValue]([CV_CK], [CV_CR], [CV_OutputCode], [CV_PassThroughKey])
SELECT N'7566a3c3-8c44-4f20-8382-9f71ba247551', N'3ffcbc93-d8cb-41ad-a5f2-6e88a1f35334', N'wisetech', NULL UNION ALL
SELECT N'7566a3c3-8c44-4f20-8382-9f71ba247551', N'5d6edc73-123e-4388-9e43-f138188bfc9b', N'wtABI1210', NULL UNION ALL
SELECT N'7566a3c3-8c44-4f20-8382-9f71ba247551', N'34074c74-117f-4cf3-8936-f1c19669d472', N'https://api.intlbondmarine.com/ABIEBondServices_dev/ABIRequest.aspx', NULL

-- DeliveryNotification Transformation
DECLARE @NotificationTypePK UNIQUEIDENTIFIER = '2B7E298B-A93E-4A0F-B462-FEE65F031A88' -- SELECT DT_PK FROM eHubMessageType WHERE DT_Code = 'http://cargowise.com/ehub/core/2018/06#DeliveryNotificationMessage'
DECLARE @UniversalIncludeTypePK UNIQUEIDENTIFIER = '9E14A465-C969-485B-9B13-8418AB1CCC4B' -- SELECT DT_PK FROM eHubMessageType WHERE DT_Code = 'http://www.cargowise.com/Schemas/Universal/2012/11#UniversalInterchangeInclude'
DECLARE @NotificationTransSet  UNIQUEIDENTIFIER = '1A5E0220-F36E-4D09-B7F5-8222E34D27DD' -- SELECT NEWID()
DECLARE @NotificationTransSet_Test  UNIQUEIDENTIFIER = '86F05D5D-2C99-46E4-BD9E-BCDFAEB0F9D1' -- SELECT NEWID()
DECLARE @IncludeToInterchangeTranSet UNIQUEIDENTIFIER = 'B9AA413D-EF06-4357-ABAA-535FD4237AD6'  -- select * from eHubTransformationType where TT_TransformationType = 'CargoWise.eHub.Clients.EDI.Universal_2012_11.Transforms.UniversalInterchangeInclude2UniversalInterchange, CargoWise.eHub.Clients.EDI.Universal_2012_11, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350' 

INSERT INTO eHubTransformationSet
(TS_PK, TS_Name, TS_CC_Sender, TS_CC_Recipient, TS_DT_Source)
SELECT @NotificationTransSet,'eBond - Delivery Notification to Universal', @USCustomsEBond, null, @NotificationTypePK UNION ALL
SELECT @NotificationTransSet_Test,'eBond Test - Delivery Notification to Universal', @USCustomsEBondTest, null, @NotificationTypePK

INSERT INTO [dbo].[eHubTransformationType] ([TT_PK], [TT_DT_Source], [TT_DT_Target], [TT_TransformationType]) 
	VALUES (N'41450620-E502-4BC6-9D2C-9B510ACC7C14', @NotificationTypePK, @UniversalIncludeTypePK, 'CargoWise.eHub.Products.USCustoms.eBond.Transforms.DeliveryNotification2UEvent.DeliveryNotification2UEvent, CargoWise.eHub.Products.USCustoms.eBond.Transforms.DeliveryNotification2UEvent, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350')

INSERT INTO [dbo].[eHubTransformationMapping] ([TM_TS_PK], [TM_Order], [TM_TT_PK]) 
	VALUES (@NotificationTransSet, 0, '41450620-E502-4BC6-9D2C-9B510ACC7C14'),
		   (@NotificationTransSet_Test, 0, '41450620-E502-4BC6-9D2C-9B510ACC7C14'),
		   (@NotificationTransSet, 1, @IncludeToInterchangeTranSet),
		   (@NotificationTransSet_Test, 1, @IncludeToInterchangeTranSet)

rollback
--commit
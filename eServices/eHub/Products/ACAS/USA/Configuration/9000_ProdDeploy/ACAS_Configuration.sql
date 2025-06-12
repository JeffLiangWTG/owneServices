USE eHubTransactions
GO

SET XACT_ABORT ON

BEGIN TRANSACTION

ALTER TABLE eHubClient NOCHECK CONSTRAINT ALL
ALTER TABLE eHubRoutingRule NOCHECK CONSTRAINT ALL

DECLARE @CC_ADVANCE_AIR_CARGO_REPORT uniqueidentifier = '7f31d6d3-91ce-44b6-9a1c-a6e808d99992'
DECLARE @CC_ACAS_US uniqueidentifier = 'ec29559d-57aa-4563-9f59-072048e4e16c'
DECLARE @CC_ACAS_US_FRI uniqueidentifier = 'd6657f37-81e6-4b79-9db1-f642e02649a4'
DECLARE @CC_ACAS_US_FRI_TST uniqueidentifier = 'ab952e80-5245-44a9-9319-251b55908143'
DECLARE @CC_ACAS_US_FHL uniqueidentifier = '093e64da-280d-464c-b49b-fef252ee2a38'
DECLARE @CC_ACAS_US_FHL_TST uniqueidentifier = 'eed40a1b-df35-47f8-8709-771a28c5967e'
DECLARE @CC_ACAS_US_ASN uniqueidentifier = '6ad89007-a140-4065-adb9-7f9d45c59367'
DECLARE @CC_ACAS_US_ASN_TST uniqueidentifier = '9952ee2f-d7e5-4aef-892d-cf1cab133f19'

DECLARE @RR_ADVANCE_AIR_CARGO_REPORT uniqueidentifier = '1526bad0-61fb-41b3-830a-3ae92b24942d'
DECLARE @RR_SP_ACAS_US uniqueidentifier = '286e90c2-8541-4845-ac4f-f6a416829478'
DECLARE @RR_DEST_US uniqueidentifier = 'b4a3a586-649f-45a4-b069-67566d307cb5'

DECLARE @RT_ACAS_US uniqueidentifier = 'f5721967-bd3b-450d-93c0-794ed4626ce6'
DECLARE @SP_ACAS_US uniqueidentifier = '9c3ecde2-c59e-4438-9291-084251a91d00'

-- Clients
INSERT INTO [dbo].[eHubClient] ([CC_PK],[CC_ID],[CC_FriendlyName],[CC_Odyssey_OH],[CC_DistributionZone],[CC_EmailAddress],[CC_Password],[CC_OwnerCategory],[CC_SystemCategory],[CC_RR],[CC_RequireStatusResponse],[CC_NotificationForInboxRecipient])
	 VALUES (@CC_ADVANCE_AIR_CARGO_REPORT, 'ADVANCE_AIR_CARGO_REPORT', 'Air Cargo Advance Screening', '00000000-0000-0000-0000-000000000000', NULL, '', '', 'Service', 'Third Party', @RR_ADVANCE_AIR_CARGO_REPORT, 0, 1)
		   ,(@CC_ACAS_US, 'ACAS_US', 'ACAS US Service', '00000000-0000-0000-0000-000000000000', NULL, '', '', 'Service Provider', 'Third Party', NULL, 0, 0)
		   ,(@CC_ACAS_US_FRI, 'ACAS_US_FRI', 'ACAS US Freight Report', '00000000-0000-0000-0000-000000000000', '75419F4C-C522-4890-BD5D-BCA5E12268F6', '', '', 'Service', 'Third Party', NULL, 0, 0)
		   ,(@CC_ACAS_US_FRI_TST, 'ACAS_US_FRI_TST', 'ACAS US Test Freight Report', '00000000-0000-0000-0000-000000000000', '75419F4C-C522-4890-BD5D-BCA5E12268F6', '', '', 'Service', 'Third Party', NULL, 0, 0)
		   ,(@CC_ACAS_US_FHL, 'ACAS_US_FHL', 'ACAS US House Check List', '00000000-0000-0000-0000-000000000000', '75419F4C-C522-4890-BD5D-BCA5E12268F6', '', '', 'Service', 'Third Party', NULL, 0, 0)
		   ,(@CC_ACAS_US_FHL_TST, 'ACAS_US_FHL_TST', 'ACAS US Test House Check List', '00000000-0000-0000-0000-000000000000', '75419F4C-C522-4890-BD5D-BCA5E12268F6', '', '', 'Service', 'Third Party', NULL, 0, 0)
		   ,(@CC_ACAS_US_ASN, 'ACAS_US_ASN', 'ACAS US Acknowledgement of Hold', '00000000-0000-0000-0000-000000000000', '75419F4C-C522-4890-BD5D-BCA5E12268F6', '', '', 'Service', 'Third Party', NULL, 0, 0)
		   ,(@CC_ACAS_US_ASN_TST, 'ACAS_US_ASN_TST', 'ACAS US Test Acknowledgement of Hold', '00000000-0000-0000-0000-000000000000', '75419F4C-C522-4890-BD5D-BCA5E12268F6', '', '', 'Service', 'Third Party', NULL, 0, 0)


-- Registrations
INSERT INTO [dbo].[eHubRegistrationType] ([RT_PK],[RT_ID],[RT_Description],[RT_RegistrantType])
	 VALUES (@RT_ACAS_US, 'ACAS_US', 'US Air Cargo Advance Screening', 'Client')

INSERT INTO [dbo].[eHubServiceProvider] ([SP_CC_Service],[SP_CC_Provider],[SP_RR],[SP_PK])
	 VALUES (@CC_ADVANCE_AIR_CARGO_REPORT, @CC_ACAS_US, @RR_SP_ACAS_US, @SP_ACAS_US)

INSERT INTO [dbo].[eHubServiceProviderRequiredRegistration] ([SX_RT],[SX_LookupFactName],[SX_QualifierFactName],[SX_SP])
	 VALUES (@RT_ACAS_US, 'SourceParty', 'EventBranch', @SP_ACAS_US)


-- Routing Rules
INSERT INTO [dbo].[eHubRoutingRule] ([RR_PK],[RR_Condition_Expression],[RR_Group_RR_GroupRule],[RR_Group_MatchMultiple],[RR_Success_CC_Recipient],[RR_Success_RR_SubRule],[RR_Failed_ErrorCode],[RR_Failed_ErrorDescription],[RR_Success_SP_Provider],[RR_Group_Ordering],[RR_Group_Name])
	 VALUES (@RR_ADVANCE_AIR_CARGO_REPORT,NULL,NULL,0,NULL,NULL,'IRJ','Department=WiseTechGlobal|Reason=Air Cargo Advance Screening not supported for destination.',NULL,NULL,NULL)
		   ,('bd80be37-d3f4-4ca8-87e3-ef8d0aadc8d2','[@PortOfFirstArrival,StartsWith,US]',@RR_ADVANCE_AIR_CARGO_REPORT,NULL,NULL,@RR_DEST_US,NULL,NULL,NULL,1000,NULL)
		   ,(@RR_DEST_US,'[@PortOfFirstArrival,StartsWith,US]',NULL,NULL,NULL,NULL,'IRJ','Department=WiseTechGlobal|Reason=You are not registered at destination for Air Cargo Advance Screening. Contact WTG to register.',@SP_ACAS_US,NULL,NULL)
		   ,(@RR_SP_ACAS_US,NULL,NULL,0,NULL,NULL,NULL,NULL,NULL,NULL,NULL)
		   ,('21CBC2F4-EC2A-459C-96CF-B094C5C78DEE','[@Namespace,Equal,http://www.cargowise.com/Schemas/Universal/2012/11/AirCargoAdvancedScreening/1] && [@LicenceType,Equal,PRD]',@RR_SP_ACAS_US,NULL,@CC_ACAS_US_FRI,NULL,NULL,NULL,NULL,1000,NULL)
		   ,('917DD10D-5827-4AAD-B8D2-27DD24769E4F','[@Namespace,Equal,http://www.cargowise.com/Schemas/Universal/2012/11/AirCargoAdvancedScreening/1] && [@LicenceType,NotEqual,PRD]',@RR_SP_ACAS_US,NULL,@CC_ACAS_US_FRI_TST,NULL,NULL,NULL,NULL,2000,NULL)
		   ,('7406F07F-EDA8-46AE-9D1B-1EFE646C4B58','[@Namespace,Equal,http://www.cargowise.com/Schemas/Universal/2012/11/HouseCheckList/1] && [@LicenceType,Equal,PRD]',@RR_SP_ACAS_US,NULL,@CC_ACAS_US_FHL,NULL,NULL,NULL,NULL,3000,NULL)
		   ,('512E0EE0-C900-407F-B8C4-6BC0482E1207','[@Namespace,Equal,http://www.cargowise.com/Schemas/Universal/2012/11/HouseCheckList/1] && [@LicenceType,NotEqual,PRD]',@RR_SP_ACAS_US,NULL,@CC_ACAS_US_FHL_TST,NULL,NULL,NULL,NULL,4000,NULL)
		   ,('29AC6337-E067-42D7-B885-FB572F7A5C70','[@Namespace,Equal,http://www.cargowise.com/Schemas/Universal/2012/11/AcknowledgementOfHold/1] && [@LicenceType,Equal,PRD]',@RR_SP_ACAS_US,NULL,@CC_ACAS_US_ASN,NULL,NULL,NULL,NULL,5000,NULL)
		   ,('F626B3D6-8C5B-41A1-9F2E-08B89E641926','[@Namespace,Equal,http://www.cargowise.com/Schemas/Universal/2012/11/AcknowledgementOfHold/1] && [@LicenceType,NotEqual,PRD]',@RR_SP_ACAS_US,NULL,@CC_ACAS_US_ASN_TST,NULL,NULL,NULL,NULL,6000,NULL)

INSERT INTO [dbo].[eHubRoutingRuleFact] ([RX_PK],[RX_RR],[RX_Name],[RX_Type],[RX_Query])
	 VALUES ('21d47cbc-37b8-4b6a-b087-9391fcc70586', @RR_ADVANCE_AIR_CARGO_REPORT, 'SourceParty', 'PROPERTY', 'http://schemas.microsoft.com/BizTalk/2003/system-properties#SourceParty')
		   ,('5d27f195-2b59-4a1e-a229-91d94da5f7a1', @RR_ADVANCE_AIR_CARGO_REPORT, 'EventBranch', 'XPATHNAV', '/*[local-name()=''UniversalInterchange'']/*[local-name()=''Body'']/*[local-name()=''UniversalShipment'']/*[local-name()=''Shipment'']/*[local-name()=''DataContext'']/*[local-name()=''Workflow'']/*[local-name()=''EventBranch'']')
		   ,('ab8a921f-b6cf-4eef-b7e5-45828e5454ec', @RR_ADVANCE_AIR_CARGO_REPORT, 'LicenceType', 'SQL', 'EXEC [ediProdCache]..[SelectLicenceTypeWithClientID] @SourceParty;')
		   ,('7a5b862c-9043-4620-ac39-ad098e7fdc62', @RR_ADVANCE_AIR_CARGO_REPORT, 'Namespace', 'XPATHNAV', '/*[local-name()=''UniversalInterchange'']/*[local-name()=''Body'']/*[local-name()=''UniversalShipment'']/namespace::*[name()='''']')
		   ,('42278924-f5e8-40b3-a41f-be87239c4697', @RR_ADVANCE_AIR_CARGO_REPORT, 'PortOfFirstArrival', 'XPATHNAV', '/*[local-name()=''UniversalInterchange'']/*[local-name()=''Body'']/*[local-name()=''UniversalShipment'']/*[local-name()=''Shipment'']/*[local-name()=''PortOfFirstArrival'']')


-- Message Types
DECLARE @DT_FRI uniqueidentifier = 'A6A226C5-F0EE-4FBB-BD60-12448A6A5A47'
DECLARE @DT_ASN uniqueidentifier = 'ED507D90-7FA8-445E-989D-0DEE22417E72'
DECLARE @DT_FHL uniqueidentifier = '6C28FA69-C886-47B9-899F-3AC8A865951B'
DECLARE @DT_PSN	uniqueidentifier = '730D7614-8C4F-45CC-A005-606CA36C6448'
DECLARE @DT_PER	uniqueidentifier = '84230791-D692-460F-BE9B-F54424F15088'
DECLARE @DT_ACAS_Envelope uniqueidentifier = '7112D528-05E4-45BD-816C-0C30F70AD6CC'
DECLARE @DT_UShipment_FRI uniqueIdentifier = 'D747EF94-A436-44A8-A881-84DBE80E5AC1'
DECLARE @DT_UShipment_ASN uniqueIdentifier = 'BA6AD07B-822F-475E-811D-6C8B23462BCA'
DECLARE @DT_UShipment_FHL uniqueIdentifier = '9C1A5A3F-2430-4F2B-93C2-6AA995C7F2B6'

INSERT INTO [dbo].[eHubMessageType] ([DT_PK], [DT_Code], [DT_IsEDI], [DT_IsFlatFile])
	VALUES (@DT_FRI, 'http://wisetechglobal.com/ehub/acas/us#FRI', 0, 1)
		  ,(@DT_UShipment_FRI, 'http://www.cargowise.com/Schemas/Universal/2012/11/AirCargoAdvancedScreening/1', 0, 0)
		  ,(@DT_ASN, 'http://wisetechglobal.com/ehub/acas/us#ASN', 0, 1)
		  ,(@DT_UShipment_ASN, 'http://www.cargowise.com/Schemas/Universal/2012/11/AcknowledgementOfHold/1#UniversalShipment', 0, 0)
		  ,(@DT_FHL, 'http://wisetechglobal.com/ehub/acas/us#FHL', 0, 1)
		  ,(@DT_UShipment_FHL, 'http://www.cargowise.com/Schemas/Universal/2012/11/HouseCheckList/1#UniversalShipment', 0, 0)
		  ,(@DT_PSN, 'http://wisetechglobal.com/ehub/acas/us#PSN', 0, 1)
		  ,(@DT_PER, 'http://wisetechglobal.com/ehub/acas/us#PER', 0, 1)
		  ,(@DT_ACAS_Envelope, 'http://wisetechglobal.com/ehub/acas/us/envelope#ACAS_US_Envelope', 0, 0)

-- Subscriptions
DECLARE @ST_ACASUS uniqueidentifier = '1526bad0-61fb-41b3-830a-3ae92b24942d'
DECLARE @ST_ACASUSID uniqueidentifier = '4F142989-8B75-47B3-A475-444E4F8B80DF'

INSERT INTO [dbo].[eHubSubscriptionType] ([ST_PK],[ST_ID],[ST_Name],[ST_ExpiryDays])
	 VALUES (@ST_ACASUS, 'ACASUS', 'Air Cargo Advance Screening US', 180)
		   ,(@ST_ACASUSID, 'ACASID', 'ACAS US ID', 180)

INSERT INTO [dbo].[eHubSubscriptionLookup]([SL_PK], [SL_ST], [SL_DT], [SL_ValueXpath])
	 VALUES ('19EDC842-55F0-4351-9F4C-A9EB295C40E6', @ST_ACASUSID, @DT_ACAS_Envelope, '/*[local-name()=''ACAS_US_Envelope'']/*[local-name()=''MessageHeader'']/*[local-name()=''Recipient'']')
		   ,('91B8B77A-868A-470B-8F02-541920912F93', @ST_ACASUS, @DT_ACAS_Envelope, '/*[local-name()=''ACAS_US_Envelope'']/*[local-name()=''MessageBody'']/*[local-name()=''PSN'']/*[local-name()=''AirWayBill'']/*[local-name()=''PackageTrackingIdentifier'']')
		   ,('3C0C853F-5EFB-474F-82DD-8263CFF0AB54', @ST_ACASUS, @DT_ACAS_Envelope, '/*[local-name()=''ACAS_US_Envelope'']/*[local-name()=''MessageBody'']/*[local-name()=''PSN'']/*[local-name()=''AirWayBill'']/*[local-name()=''AWBNumbers'']/*[local-name()=''HAWBNumber'']')
		   ,('29B58710-C21D-45BB-B5E8-2254825E3CBB', @ST_ACASUS, @DT_ACAS_Envelope, '/*[local-name()=''ACAS_US_Envelope'']/*[local-name()=''MessageBody'']/*[local-name()=''PSN'']/*[local-name()=''AirWayBill'']/*[local-name()=''AWBNumbers'']/*[local-name()=''AWBNumber'']')
		   ,('4E6DEF26-D3C8-4C69-B03A-4E90C95E0BEA', @ST_ACASUS, @DT_ACAS_Envelope, '/*[local-name()=''ACAS_US_Envelope'']/*[local-name()=''MessageBody'']/*[local-name()=''PER'']/*[local-name()=''AirWayBill'']/*[local-name()=''PackageTrackingIdentifier'']')
		   ,('CA818000-131B-443E-AA21-AA2929ED560C', @ST_ACASUS, @DT_ACAS_Envelope, '/*[local-name()=''ACAS_US_Envelope'']/*[local-name()=''MessageBody'']/*[local-name()=''PER'']/*[local-name()=''AirWayBill'']/*[local-name()=''AWBNumbers'']/*[local-name()=''HAWBNumber'']')
		   ,('54444711-33A4-4D41-8A86-895575379C8C', @ST_ACASUS, @DT_ACAS_Envelope, '/*[local-name()=''ACAS_US_Envelope'']/*[local-name()=''MessageBody'']/*[local-name()=''PER'']/*[local-name()=''AirWayBill'']/*[local-name()=''AWBNumbers'']/*[local-name()=''AWBNumber'']')

-- Transformations
DECLARE @TT_UI2US_FRI uniqueidentifier = 'A6A226C5-F0EE-4FBB-BD60-12448A6A5A47'
DECLARE @TT_US2FRI uniqueidentifier = '317B754C-3573-43FA-9DF7-862520AA3F43'
DECLARE @TT_UI2US_ASN uniqueidentifier = '05758DE4-B2D0-4CA7-9F77-919B4CABC54B'
DECLARE @TT_US2ASN uniqueidentifier = '471014DE-DB91-4680-9F7C-2039CED87690'
DECLARE @TT_UI2US_FHL uniqueidentifier = '293A77A2-9F4D-4D2B-BD68-61DF1F148457'
DECLARE @TT_US2FHL uniqueidentifier = '328D5603-3889-4CBD-A395-D99EC3658C03'
DECLARE @TT_PSN2UII	uniqueidentifier = '997B3AF7-7DB1-4FCD-A315-5D6E806F0DB7'
DECLARE @TT_PER2UII uniqueidentifier = 'CA8C7A65-B7EA-42F5-A5B0-22BA650B1ED1'
DECLARE @TT_UII2UI_2012 uniqueidentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Clients.EDI.Universal_2012_11.Transforms.UniversalInterchangeInclude2UniversalInterchangeEnvelope%')
DECLARE @TT_U2DN uniqueidentifier = '84fa4074-2e2d-4266-a9c9-1158108ece78'
DECLARE @TT_DN2UE uniqueidentifier = '76f5ae22-d9c8-4682-8d14-80b74eafdac0'
DECLARE @TT_VC21_2 uniqueidentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType = 'CargoWise.eHub.Clients.EDI.Universal.Transforms.VersionConversion.UniversalInterchangeShpV2_1ToV2, CargoWise.eHub.Clients.EDI.Universal.Transforms.VersionConversion, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350')


DECLARE @DT_UI_2011 uniqueIdentifier = (SELECT DT_PK from eHubMessageType WHERE DT_Code = 'http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange')
DECLARE @DT_UII_2012 uniqueidentifier = (SELECT DT_PK FROM eHubMessageType WHERE DT_Code = 'http://www.cargowise.com/Schemas/Universal/2012/11#UniversalInterchangeInclude')
DECLARE @DT_DN uniqueIdentifier = (SELECT DT_PK from eHubMessageType WHERE DT_Code = 'http://cargowise.com/ehub/core/2018/06#DeliveryNotificationMessage')

DECLARE @TS_FRI uniqueidentifier = 'AD9A97AE-3B72-4051-9C15-2ED58A8B7A35'
DECLARE @TS_ASN uniqueidentifier = 'A1F8ED65-052C-47CA-86DF-E8351E909DCA'
DECLARE @TS_FHL uniqueidentifier = '5EAD115D-6EC5-4865-93A0-3FB0A25C184F'
DECLARE @TS_PSN	uniqueidentifier = '66D57A52-4BC8-4DB5-87D2-B21F8C271D10'
DECLARE @TS_PER uniqueidentifier = '9D37DB12-FA79-437A-8A15-A026A904454A'
DECLARE @TS_FRI_TST uniqueidentifier = 'EDDCF0D4-0AE6-4A85-9E01-71F69EC40CBC'
DECLARE @TS_ASN_TST uniqueidentifier = 'E443106B-DF85-4305-8771-0928E2589E93'
DECLARE @TS_FHL_TST uniqueidentifier = 'DF827725-6AFE-44D3-8F15-E397124B4F3B'
DECLARE @TS_ACASUS uniqueidentifier = 'AD0860F1-055C-4DC0-814C-6321F7437E3A'
DECLARE @TS_DN uniqueidentifier = 'fb1d799d-135d-44c4-b68a-37bb8a53cc42'
DECLARE @TS_ERR uniqueidentifier = 'b52d1f47-d9d7-41eb-85ff-51c52c70b805'

INSERT INTO [dbo].[eHubTransformationType] ([TT_PK], [TT_DT_Source], [TT_DT_Target], [TT_TransformationType])
	VALUES (@TT_UI2US_FRI, @DT_UI_2011, @DT_UShipment_FRI, 'CargoWise.eHub.Products.ACAS.US.Transforms.UInterchange2UShipment.ACAS_US.UInterchange2UShipment_FRI, CargoWise.eHub.Products.ACAS.US.Transforms.UInterchange2UShipment, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350')
		  ,(@TT_US2FRI, @DT_UShipment_FRI, @DT_FRI, 'CargoWise.eHub.Products.ACAS.US.Transforms.UShipment2FRI_ACAS_US.UShipment2FRI_ACAS_US, CargoWise.eHub.Products.ACAS.US.Transforms.UShipment2FRI_ACAS_US, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350')
		  ,(@TT_UI2US_ASN, @DT_UI_2011, @DT_UShipment_ASN, 'CargoWise.eHub.Products.ACAS.US.Transforms.UInterchange2UShipment.ACAS_US.UInterchange2UShipment_ASN, CargoWise.eHub.Products.ACAS.US.Transforms.UInterchange2UShipment, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350')
		  ,(@TT_US2ASN, @DT_UShipment_ASN, @DT_ASN, 'CargoWise.eHub.Products.ACAS.US.Transforms.UShipment2ASN_ACAS_US.UShipment2ASN_ACAS_US, CargoWise.eHub.Products.ACAS.US.Transforms.UShipment2ASN_ACAS_US, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350')
		  ,(@TT_UI2US_FHL, @DT_UI_2011, @DT_UShipment_FHL, 'CargoWise.eHub.Products.ACAS.US.Transforms.UInterchange2UShipment.ACAS_US.UInterchange2UShipment_FHL, CargoWise.eHub.Products.ACAS.US.Transforms.UInterchange2UShipment, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350')
		  ,(@TT_US2FHL, @DT_UShipment_FHL, @DT_FHL, 'CargoWise.eHub.Products.ACAS.US.Transforms.UShipment2FHL_ACAS_US.UShipment2FHL_ACAS_US, CargoWise.eHub.Products.ACAS.US.Transforms.UShipment2FHL_ACAS_US, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350')
		  ,(@TT_PSN2UII, @DT_ACAS_Envelope, @DT_UII_2012, 'CargoWise.eHub.Products.ACAS.US.Transforms.PSN2UInterchangeInclude_ACAS_US.PSN2UInterchangeInclude_ACAS_US, CargoWise.eHub.Products.ACAS.US.Transforms.PSN2UInterchangeInclude_ACAS_US, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350')
		  ,(@TT_PER2UII, @DT_ACAS_Envelope, @DT_UII_2012, 'CargoWise.eHub.Products.ACAS.US.Transforms.PER2UInterchangeInclude_ACAS_US.PER2UInterchangeInclude_ACAS_US, CargoWise.eHub.Products.ACAS.US.Transforms.PER2UInterchangeInclude_ACAS_US, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350')
		  ,(@TT_U2DN, @DT_UI_2011, @DT_DN, 'CargoWise.eHub.Products.ACAS.US.Universal2DeliveryNotification.UniversalInterchange2DeliveryNotification, CargoWise.eHub.Products.ACAS.US.Universal2DeliveryNotification, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350')
		  ,(@TT_DN2UE, @DT_DN, @DT_UI_2011, 'CargoWise.eHub.Products.ACAS.US.DeliveryNotification2UEvent.DeliveryNotification2UEvent, CargoWise.eHub.Products.ACAS.US.DeliveryNotification2UEvent, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350')

INSERT INTO [dbo].[eHubTransformationSet]([TS_PK], [TS_Name], [TS_CC_Sender], [TS_CC_Recipient], [TS_DT_Source], [TS_BillingInterfaceName], [TS_BillSender], [TS_BillRecipient], [TS_XPathPredicate])
	VALUES (@TS_FRI, 'ACAS US Freight Report', null, @CC_ACAS_US_FRI, @DT_UI_2011, 'ACAS US Freight Report', 1, 0, null)
		  ,(@TS_ASN, 'ACAS Hold Acknowledgement', null, @CC_ACAS_US_ASN, @DT_UI_2011, 'ACAS Hold Acknowledgement', 1, 0, null)
		  ,(@TS_FHL, 'ACAS House Checklist', null, @CC_ACAS_US_FHL, @DT_UI_2011, 'ACAS House Checklist', 1, 0, null)
		  ,(@TS_FRI_TST, 'ACAS US Freight Report - Test', null, @CC_ACAS_US_FRI_TST, @DT_UI_2011, 'ACAS US Freight Report - Test', 1, 0, null)
		  ,(@TS_ASN_TST, 'ACAS Hold Acknowledgement - Test', null, @CC_ACAS_US_ASN_TST, @DT_UI_2011, 'ACAS Hold Acknowledgement - Test', 1, 0, null)
		  ,(@TS_FHL_TST, 'ACAS House Checklist - Test', null, @CC_ACAS_US_FHL_TST, @DT_UI_2011, 'ACAS House Checklist - Test', 1, 0, null)
		  ,(@TS_PSN, 'Preliminary Status Notification from ACAS US', @CC_ACAS_US, null, @DT_ACAS_Envelope, 'Preliminary Status Notification from ACAS US', 0, 1, '/*[local-name()=''ACAS_US_Envelope'']/*[local-name()=''MessageBody'']/*[local-name()=''PSN'']')
		  ,(@TS_PER, 'Preliminary Error Report from ACAS US', @CC_ACAS_US, null, @DT_ACAS_Envelope, 'Preliminary Error Report from ACAS US', 0, 1, '/*[local-name()=''ACAS_US_Envelope'']/*[local-name()=''MessageBody'']/*[local-name()=''PER'']')
		  ,(@TS_ACASUS, 'ACAS System Configuration', NULL, @CC_ACAS_US, NULL, 'ACAS System Configuration', 0, 0, null)
		  ,(@TS_DN, 'ACAS Delivery Notification', @CC_ADVANCE_AIR_CARGO_REPORT, NULL, NULL, NULL, 0, 0, null)
		  ,(@TS_ERR, 'ACAS ERROR', @CC_ADVANCE_AIR_CARGO_REPORT, NULL, NULL, NULL, 0, 0, null)

INSERT INTO [dbo].[eHubTransformationMapping] ([TM_TS_PK], [TM_Order], [TM_TT_PK]) 
	VALUES (@TS_FRI_TST, 0, @TT_UI2US_FRI)
		  ,(@TS_FRI_TST, 1, @TT_US2FRI)
		  ,(@TS_FRI, 0, @TT_UI2US_FRI)
		  ,(@TS_FRI, 1, @TT_US2FRI)
		  ,(@TS_ASN_TST, 0, @TT_UI2US_ASN)
		  ,(@TS_ASN_TST, 1, @TT_US2ASN)
		  ,(@TS_ASN, 0, @TT_UI2US_ASN)
		  ,(@TS_ASN, 1, @TT_US2ASN)	
		  ,(@TS_FHL_TST, 0, @TT_UI2US_FHL)
		  ,(@TS_FHL_TST, 1, @TT_US2FHL)
		  ,(@TS_FHL, 0, @TT_UI2US_FHL)
		  ,(@TS_FHL, 1, @TT_US2FHL)
		  ,(@TS_PSN, 0, @TT_PSN2UII)
		  ,(@TS_PSN, 1, @TT_UII2UI_2012)
		  ,(@TS_PER, 0, @TT_PER2UII)
		  ,(@TS_PER, 1, @TT_UII2UI_2012)
		  ,(@TS_DN, 0, @TT_DN2UE)
		  ,(@TS_ERR, 0, @TT_VC21_2)
		  ,(@TS_ERR, 1, @TT_U2DN)
		  ,(@TS_ERR, 2, @TT_DN2UE)

-- Code Sets
DECLARE @CS_EventType uniqueidentifier = '83C9F9C7-4ECD-4041-A0D6-A0585E2DA646'
DECLARE @CR_EventType uniqueidentifier = '89AF91D4-D758-4B4E-BCDD-71948D9C3BCF'
DECLARE @CS_Error uniqueidentifier = 'CFA0035F-1501-43D9-BD45-D144A9237871'
DECLARE @CR_Error uniqueidentifier = 'C5636565-806D-4576-8318-70CE963507A6'

INSERT INTO [dbo].[eHubCodeSet]([CS_PK], [CS_Name], [CS_TS], [CS_CC_Sender], [CS_CC_Recipient], [CS_Key1Name])
	VALUES (@CS_EventType, 'Event Type', @TS_ACASUS, @CC_ACAS_US, @CC_ACAS_US, 'ACAS Code')
		  ,(@CS_Error, 'Error', @TS_ACASUS, @CC_ACAS_US, @CC_ACAS_US, 'Error Code')

INSERT INTO [dbo].[eHubCodeSetResult]([CR_PK], [CR_CS], [CR_Order], [CR_Name])
	VALUES (@CR_EventType, @CS_EventType, 1, 'Event Type')
		  ,(@CR_Error, @CS_Error, 1, 'Description')

INSERT INTO [dbo].[eHubCodeMapKey]([CK_PK], [CK_CS], [CK_Order], [CK_Key1Value])
	VALUES ('7E9EC2C9-5E96-4B93-AA49-031983AE113E', @CS_EventType, 1, 'SR')
		  ,('52EA19B0-BA10-4072-A4A5-EB6931C7E6DE', @CS_EventType, 2, 'SF')
		  ,('465BCEE1-CF40-4379-A1B6-4BF967DF72C8', @CS_EventType, 3, '6H')
		  ,('F4D815EA-7DF1-4D6B-BA5E-D0934851E978', @CS_EventType, 4, '6J')
		  ,('CADE1082-A476-449E-8196-E209F3EA1B99', @CS_EventType, 5, '6I')
		  ,('B88553E9-702D-4E6C-966E-07EFA6B21066', @CS_EventType, 6, '7H')
		  ,('742705E9-2729-4FF2-B4E0-E9C3F2BE144D', @CS_EventType, 7, '7J')
		  ,('6E7B3860-0E6A-4601-B036-D63618AEBAF9', @CS_EventType, 8, '7I')
		  ,('CA574045-03F2-4E4E-A545-90B59A27D65E', @CS_EventType, 9, '8H')
		  ,('A6B8D052-FB7A-417E-8160-3FE55E8401D5', @CS_EventType, 10, '8J')
		  ,('BF2F089C-BD56-4F38-80EC-844E36C2727C', @CS_EventType, 11, '8I')
		  ,('FC68E9A7-D0A4-4A7F-82FD-67E1CE31E155', @CS_EventType, 12, '%')
		  ,('8CBE366B-79F3-455E-BEB4-275F2FCD93BD', @CS_Error, 1, 'HBS_BAD_FORMAT')
		  ,('7C10F123-8CC5-4BB5-B68A-5A47C0ECFB60', @CS_Error, 2, 'INVALID_BILL_QTY')
		  ,('D8CD6E4E-E872-42FE-BD34-52B5BECCE1A5', @CS_Error, 3, 'INVALID_CNE_ISO_CTRY')
		  ,('A729D79C-0AB9-46A8-98F3-78489F9B7BB9', @CS_Error, 4, 'INVALID_HOUSE_BILL_NBR')
		  ,('C3C85BCE-C4F5-4412-9EFA-B4E6B945F9E5', @CS_Error, 5, 'INVALID_MESSAGE_TYPE')
		  ,('59D0B1E9-9255-4FA3-994F-52B44A9D4FA4', @CS_Error, 6, 'INVALID_SHP_ISO_CTRY')
		  ,('683B3987-F99E-4958-8C73-3727D87CD90F', @CS_Error, 7, 'INVALID_WEIGHT ')
		  ,('F69A6902-F0D5-4037-B5FB-D97715D9F808', @CS_Error, 8, 'MISSING_BILL_QTY')
		  ,('F33DE524-7004-40D8-8157-01917FF6B9F0', @CS_Error, 9, 'MISSING_CARGO_DESC')
		  ,('E56DFC3B-793F-4FDF-B417-350105252225', @CS_Error, 10, 'MISSING_CNE_ADDR1')
		  ,('9CC78B18-CA19-4ACB-9878-D9DF0F2D8AC5', @CS_Error, 11, 'MISSING_CNE_ADDR2')
		  ,('F14B2911-5C0F-457D-B5D1-3FC8F5843D7E', @CS_Error, 12, 'MISSING_CNE_ADDR3')
		  ,('49852BA3-1DB4-4B71-8854-ADC9BDCED6ED', @CS_Error, 13, 'MISSING_CNE_CTRY')
		  ,('8173E57E-3CDA-45BA-BCF6-F280B19E06F9', @CS_Error, 14, 'MISSING_CNE_LINE')
		  ,('0C2BDB72-BDF1-4CCE-BF22-86EBA75D7F71', @CS_Error, 15, 'MISSING_CNE_NAME')
		  ,('561A09A4-5F1B-4344-BC93-D0FC23B41685', @CS_Error, 16, 'MISSING_HOUSE_BILL_NBR')
		  ,('4E9E59D8-5CEB-4412-94F7-5E73374C3062', @CS_Error, 17, 'MISSING_SHP_ADDR1')
		  ,('D0BB022E-6ABB-479F-A25C-6E7ECAB6EA73', @CS_Error, 18, 'MISSING_SHP_ADDR2')
		  ,('FC3E464F-8306-4E39-9A88-AE797E862F31', @CS_Error, 19, 'MISSING_SHP_ADDR3')
		  ,('9C6832A6-CF98-4D2B-B997-2CAB48624B76', @CS_Error, 20, 'MISSING_SHP_CTRY')
		  ,('28A5BD6A-D954-4D79-92A7-782E57B6EBE2', @CS_Error, 21, 'MISSING_SHP_LINE')
		  ,('DB1C59EC-DD1C-4B97-ABF3-80080AB9F094', @CS_Error, 22, 'MISSING_SHP_NAME')
		  ,('8F191B4D-DEF1-4B6C-B3AE-E7C7441B8E2B', @CS_Error, 23, 'MISSING_WBL_LINE')
		  ,('B792EE06-18A3-4796-96B5-B8A940ED7BA8', @CS_Error, 24, 'MISSING_WT_UNITS')
		  ,('BF557006-2E6D-46C6-8E39-DC93A72B1E81', @CS_Error, 25, '%')

INSERT INTO [dbo].[eHubCodeMapValue]([CV_CK], [CV_CR], [CV_OutputCode])
	VALUES ('7E9EC2C9-5E96-4B93-AA49-031983AE113E', @CR_EventType, 'MPP')
		  ,('52EA19B0-BA10-4072-A4A5-EB6931C7E6DE', @CR_EventType, 'SCM')
		  ,('465BCEE1-CF40-4379-A1B6-4BF967DF72C8', @CR_EventType, 'SHL')
		  ,('F4D815EA-7DF1-4D6B-BA5E-D0934851E978', @CR_EventType, 'SHL')
		  ,('CADE1082-A476-449E-8196-E209F3EA1B99', @CR_EventType, 'SCH')
		  ,('B88553E9-702D-4E6C-966E-07EFA6B21066', @CR_EventType, 'SHL')
		  ,('742705E9-2729-4FF2-B4E0-E9C3F2BE144D', @CR_EventType, 'SHL')
		  ,('6E7B3860-0E6A-4601-B036-D63618AEBAF9', @CR_EventType, 'SCM')
		  ,('CA574045-03F2-4E4E-A545-90B59A27D65E', @CR_EventType, 'SHL')
		  ,('A6B8D052-FB7A-417E-8160-3FE55E8401D5', @CR_EventType, 'SHL')
		  ,('BF2F089C-BD56-4F38-80EC-844E36C2727C', @CR_EventType, 'SCM')
		  ,('FC68E9A7-D0A4-4A7F-82FD-67E1CE31E155', @CR_EventType, '')
		  ,('8CBE366B-79F3-455E-BEB4-275F2FCD93BD', @CR_Error, 'HBS record missing from FHL detail message')
		  ,('7C10F123-8CC5-4BB5-B68A-5A47C0ECFB60', @CR_Error, 'Bill quantity is zero or not numeric')
		  ,('D8CD6E4E-E872-42FE-BD34-52B5BECCE1A5', @CR_Error, 'Consignee country code is not a valid ISO country code')
		  ,('A729D79C-0AB9-46A8-98F3-78489F9B7BB9', @CR_Error, 'HAWB number contains invalid characters or exceeds 12 characters')
		  ,('C3C85BCE-C4F5-4412-9EFA-B4E6B945F9E5', @CR_Error, 'Message type is not a recognized message type')
		  ,('59D0B1E9-9255-4FA3-994F-52B44A9D4FA4', @CR_Error, 'Shipper country code is not a valid ISO country code')
		  ,('683B3987-F99E-4958-8C73-3727D87CD90F', @CR_Error, 'Weight is zero or not numeric')
		  ,('F69A6902-F0D5-4037-B5FB-D97715D9F808', @CR_Error, 'Bill quantity is missing')
		  ,('F33DE524-7004-40D8-8157-01917FF6B9F0', @CR_Error, 'Cargo description is missing or invalid')
		  ,('E56DFC3B-793F-4FDF-B417-350105252225', @CR_Error, 'Consignee address line 1 is missing')
		  ,('9CC78B18-CA19-4ACB-9878-D9DF0F2D8AC5', @CR_Error, 'Consignee address line 2 is missing')
		  ,('F14B2911-5C0F-457D-B5D1-3FC8F5843D7E', @CR_Error, 'Consignee address line 3 is missing')
		  ,('49852BA3-1DB4-4B71-8854-ADC9BDCED6ED', @CR_Error, 'Consignee country code is missing')
		  ,('8173E57E-3CDA-45BA-BCF6-F280B19E06F9', @CR_Error, 'Consignee data is missing')
		  ,('0C2BDB72-BDF1-4CCE-BF22-86EBA75D7F71', @CR_Error, 'Consignee name is missing')
		  ,('561A09A4-5F1B-4344-BC93-D0FC23B41685', @CR_Error, 'Bill number (both MAWB and HAWB elements) missing')
		  ,('4E9E59D8-5CEB-4412-94F7-5E73374C3062', @CR_Error, 'Shipper address line 1 is missing')
		  ,('D0BB022E-6ABB-479F-A25C-6E7ECAB6EA73', @CR_Error, 'Shipper address line 2 is missing')
		  ,('FC3E464F-8306-4E39-9A88-AE797E862F31', @CR_Error, 'Shipper address line 3 is missing')
		  ,('9C6832A6-CF98-4D2B-B997-2CAB48624B76', @CR_Error, 'Shipper country code is missing')
		  ,('28A5BD6A-D954-4D79-92A7-782E57B6EBE2', @CR_Error, 'Shipper data is missing')
		  ,('DB1C59EC-DD1C-4B97-ABF3-80080AB9F094', @CR_Error, 'Shipper name is missing')
		  ,('8F191B4D-DEF1-4B6C-B3AE-E7C7441B8E2B', @CR_Error, 'WBL (waybill data) record is missing')
		  ,('B792EE06-18A3-4796-96B5-B8A940ED7BA8', @CR_Error, 'Weight Code is missing')
		  ,('BF557006-2E6D-46C6-8E39-DC93A72B1E81', @CR_Error, '')

-- eHubCounter
INSERT eHubCounter SELECT 'CargoWise.eHub.Products.ACAS.US.Transforms.ACAS_US', 1

ALTER TABLE eHubClient WITH CHECK CHECK CONSTRAINT ALL
ALTER TABLE eHubRoutingRule WITH CHECK CHECK CONSTRAINT ALL

--COMMIT

WHILE @@TRANCOUNT > 0
	ROLLBACK

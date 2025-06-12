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
		   ,('5d27f195-2b59-4a1e-a229-91d94da5f7a1', @RR_ADVANCE_AIR_CARGO_REPORT, 'EventBranch', 'XPATH', '/*[local-name()=''UniversalInterchange'']/*[local-name()=''Body'']/*[local-name()=''UniversalShipment'']/*[local-name()=''Shipment'']/*[local-name()=''DataContext'']/*[local-name()=''Workflow'']/*[local-name()=''EventBranch'']')
		   ,('ab8a921f-b6cf-4eef-b7e5-45828e5454ec', @RR_ADVANCE_AIR_CARGO_REPORT, 'LicenceType', 'SQL', 'EXEC [ediProdCache]..[SelectLicenceTypeWithClientID] @SourceParty;')
		   ,('7a5b862c-9043-4620-ac39-ad098e7fdc62', @RR_ADVANCE_AIR_CARGO_REPORT, 'Namespace', 'XPATH', '/*[local-name()=''UniversalInterchange'']/*[local-name()=''Body'']/*[local-name()=''UniversalShipment'']/namespace::*[name()='''']')
		   ,('42278924-f5e8-40b3-a41f-be87239c4697', @RR_ADVANCE_AIR_CARGO_REPORT, 'PortOfFirstArrival', 'XPATH', '/*[local-name()=''UniversalInterchange'']/*[local-name()=''Body'']/*[local-name()=''UniversalShipment'']/*[local-name()=''PortOfFirstArrival'']')



-- Subscriptions
DECLARE @ST_ACASUS uniqueidentifier = '1526bad0-61fb-41b3-830a-3ae92b24942d'
INSERT INTO [dbo].[eHubSubscriptionType] ([ST_PK],[ST_ID],[ST_Name],[ST_ExpiryDays])
	 VALUES (@ST_ACASUS, 'ACASUS', 'Air Cargo Advance Screening US', 180)



-- Message Types




-- Transformations




ALTER TABLE eHubClient WITH CHECK CHECK CONSTRAINT ALL
ALTER TABLE eHubRoutingRule WITH CHECK CHECK CONSTRAINT ALL

--COMMIT

WHILE @@TRANCOUNT > 0
	ROLLBACK

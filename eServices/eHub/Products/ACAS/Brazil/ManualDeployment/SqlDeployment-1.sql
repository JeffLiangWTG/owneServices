USE eHubTransactions
GO

SET XACT_ABORT ON

BEGIN TRANSACTION

ALTER TABLE eHubClient NOCHECK CONSTRAINT ALL
ALTER TABLE eHubRoutingRule NOCHECK CONSTRAINT ALL

DECLARE @CC_ADVANCE_AIR_CARGO_REPORT uniqueidentifier = (select cc_pk from eHubClient where CC_ID = 'ADVANCE_AIR_CARGO_REPORT')
DECLARE @CC_ACAS_BR uniqueidentifier = '6FF1EF8B-0959-40C5-8252-3B360D47F0BB'
DECLARE @CC_ACAS_BRTest uniqueidentifier = '19BE9D44-3C6A-4B7D-B8D5-A676FD30BADA'
DECLARE @CC_ACAS_BR_FZB uniqueidentifier = '40BE0EC6-5459-4342-8842-30E7EC33A377'
DECLARE @CC_ACAS_BR_FZB_TST uniqueidentifier = '251F8C1C-687E-4D21-835D-603FCE98F674'
DECLARE @CC_ACAS_BR_FHL uniqueidentifier = '6BE9D4C6-1844-438F-B23A-E06E89716EEA'
DECLARE @CC_ACAS_BR_FHL_TST uniqueidentifier = '5063CD99-552C-4620-A929-4B4FE1FF3D13'

-- Clients
INSERT INTO [dbo].[eHubClient] ([CC_PK],[CC_ID],[CC_FriendlyName],[CC_Odyssey_OH],[CC_DistributionZone],[CC_EmailAddress],[CC_Password],[CC_OwnerCategory],[CC_SystemCategory],[CC_RR],[CC_RequireStatusResponse],[CC_NotificationForInboxRecipient])
VALUES (@CC_ACAS_BR, 'ACAS_BR', 'ACAS BR Service', '00000000-0000-0000-0000-000000000000', NULL, '', '', 'Service Provider', 'Third Party', NULL, 0, 0)
,(@CC_ACAS_BRTest, 'ACAS_BRTest', 'ACAS BR Test Service', '00000000-0000-0000-0000-000000000000', NULL, '', '', 'Service Provider', 'Third Party', NULL, 0, 0)
,(@CC_ACAS_BR_FHL, 'ACAS_BR_FHL', 'ACAS BR House Check List', '00000000-0000-0000-0000-000000000000', '75419F4C-C522-4890-BD5D-BCA5E12268F6', '', '', 'Service', 'Third Party', NULL, 0, 0)
,(@CC_ACAS_BR_FHL_TST, 'ACAS_BR_FHL_TST', 'ACAS BR Test House Check List', '00000000-0000-0000-0000-000000000000', '75419F4C-C522-4890-BD5D-BCA5E12268F6', '', '', 'Service', 'Third Party', NULL, 0, 0)
,(@CC_ACAS_BR_FZB, 'ACAS_BR_FZB', 'ACAS BR Shipment Report', '00000000-0000-0000-0000-000000000000', '75419F4C-C522-4890-BD5D-BCA5E12268F6', '', '', 'Service', 'Third Party', NULL, 0, 0)
,(@CC_ACAS_BR_FZB_TST, 'ACAS_BR_FZB_TST', 'ACAS BR Test Shipment Report', '00000000-0000-0000-0000-000000000000', '75419F4C-C522-4890-BD5D-BCA5E12268F6', '', '', 'Service', 'Third Party', NULL, 0, 0)
		    
-- Registrations
DECLARE @RR_ADVANCE_AIR_CARGO_REPORT uniqueidentifier = '1526bad0-61fb-41b3-830a-3ae92b24942d'
DECLARE @RR_SP_ACAS_BR uniqueidentifier = '8A1C76E5-FBAD-4CBF-A4DD-40A5E7F62F8A'
DECLARE @RT_ACAS_BR uniqueidentifier = '85D0B5F6-2221-4F34-8DD8-A5A9D3AA8A3C'
DECLARE @SP_ACAS_BR uniqueidentifier = '6DACFE3D-1A43-4334-BD9A-1049A8F2D501'

INSERT INTO [dbo].[eHubRegistrationType] ([RT_PK],[RT_ID],[RT_Description],[RT_RegistrantType])
	 VALUES (@RT_ACAS_BR, 'ACAS_BR', 'BR Air Cargo Advance Screening', 'Client')

INSERT INTO [dbo].[eHubServiceProvider] ([SP_CC_Service],[SP_CC_Provider],[SP_RR],[SP_PK])
	 VALUES (@CC_ADVANCE_AIR_CARGO_REPORT, @CC_ACAS_BR, @RR_SP_ACAS_BR, @SP_ACAS_BR)

INSERT INTO [dbo].[eHubServiceProviderRequiredRegistration] ([SX_RT],[SX_LookupFactName],[SX_QualifierFactName],[SX_SP])
	 VALUES (@RT_ACAS_BR, 'SourceParty', 'EventBranch', @SP_ACAS_BR)

-- eHubRoutingRule
INSERT INTO [dbo].[eHubRoutingRule] ([RR_PK],[RR_Condition_Expression],[RR_Group_RR_GroupRule],[RR_Group_MatchMultiple],[RR_Success_CC_Recipient],[RR_Success_RR_SubRule],[RR_Failed_ErrorCode],[RR_Failed_ErrorDescription],[RR_Success_SP_Provider],[RR_Group_Ordering],[RR_Group_Name])
	 VALUES 
		   ('63E7AC12-CBF6-4C50-B57D-E8D1832AF564','[@PortOfFirstArrival,StartsWith,BR]',@RR_ADVANCE_AIR_CARGO_REPORT,NULL,NULL,NULL,'IRJ','Department=WiseTechGlobal|Reason=You are not registered at destination for Air Cargo Advance Screening. Contact WTG to register.',@SP_ACAS_BR,2000,NULL)
		   ,(@RR_SP_ACAS_BR,NULL,NULL,0,NULL,NULL,NULL,NULL,NULL,NULL,NULL)
		   ,('0E9069A6-5D4E-4DD3-B8F2-FC7955881346','[@Namespace,Equal,http://www.cargowise.com/Schemas/Universal/2012/11/CCTHouseCheckList/1] && [@LicenceType,Equal,PRD]',@RR_SP_ACAS_BR,NULL,@CC_ACAS_BR_FHL,NULL,NULL,NULL,NULL,1000,NULL)
		   ,('B85D6388-5682-4AC7-A3E3-65DC3954B74E','[@Namespace,Equal,http://www.cargowise.com/Schemas/Universal/2012/11/CCTHouseCheckList/1] && [@LicenceType,NotEqual,PRD]',@RR_SP_ACAS_BR,NULL,@CC_ACAS_BR_FHL_TST,NULL,NULL,NULL,NULL,2000,NULL)
		   ,('33DE9C90-BEB1-4483-B3F4-E3A40CADFB5A','[@Namespace,Equal,http://www.cargowise.com/Schemas/Universal/2012/11/CCTShipmentReport/1] && [@LicenceType,Equal,PRD]',@RR_SP_ACAS_BR,NULL,@CC_ACAS_BR_FZB,NULL,NULL,NULL,NULL,3000,NULL)
		   ,('7B4C6175-B63E-4FE4-80AB-DF21EEA99062','[@Namespace,Equal,http://www.cargowise.com/Schemas/Universal/2012/11/CCTShipmentReport/1] && [@LicenceType,NotEqual,PRD]',@RR_SP_ACAS_BR,NULL,@CC_ACAS_BR_FZB_TST,NULL,NULL,NULL,NULL,4000,NULL)


-- eHubSubscriptionType
DECLARE @ST_ACASBR uniqueidentifier = '51A893D0-89A8-426B-AFAD-CEC416F8BA3D'
INSERT INTO [dbo].[eHubSubscriptionType] ([ST_PK],[ST_ID],[ST_Name],[ST_ExpiryDays])
	 VALUES (@ST_ACASBR, 'ACASBR', 'Air Cargo Advance Screening BR', 30)

-- eHubRegistrationType
DECLARE @RT_ASACBR uniqueidentifier = '2F47A76F-15D7-4368-817C-34A2E8CF6B03'
INSERT INTO [dbo].[eHubRegistrationType] ([RT_PK], [RT_ID], [RT_Description], [RT_RegistrantType])
           VALUES(@RT_ASACBR,'ACAS_BRToken','ACAS BR Token','ClientSystem')
           

-- Transformation Sets
DECLARE @TS_ACASBR uniqueidentifier = '724791BB-B40F-4D32-B308-2F532E3C6685'
INSERT INTO [dbo].[eHubTransformationSet]([TS_PK], [TS_Name], [TS_CC_Sender], [TS_CC_Recipient], [TS_DT_Source], [TS_BillingInterfaceName], [TS_BillSender], [TS_BillRecipient], [TS_XPathPredicate])
	VALUES (@TS_ACASBR, 'ACAS (BR) System Configuration', @CC_ACAS_BR, @CC_ACAS_BR, NULL, 'ACAS (BR) System Configuration', 0, 0, null)
           
--Code mappings   

 DECLARE @CS_EndPoint uniqueidentifier = '08BD63B8-2204-45EC-8884-3C3702B5C1CA'
 DECLARE @CR_Endpoint uniqueidentifier = '08945482-F140-47F4-A9AF-8C422DBD7F4B'

INSERT INTO [dbo].[eHubCodeSet]([CS_PK], [CS_Name], [CS_TS], [CS_CC_Sender], [CS_CC_Recipient], [CS_Key1Name])
	VALUES (@CS_EndPoint, 'Endpoints', @TS_ACASBR, @CC_ACAS_BR, @CC_ACAS_BR, 'Name')

INSERT INTO [dbo].[eHubCodeSetResult]([CR_PK], [CR_CS], [CR_Order], [CR_Name])
	VALUES (@CR_Endpoint, @CS_EndPoint, 1, 'Endpoint URL')


	INSERT INTO [dbo].[eHubCodeMapKey]([CK_PK], [CK_CS], [CK_Order], [CK_Key1Value])
	VALUES ('8876E73B-0A7F-46A4-A7FC-130EAAF73F29', @CS_EndPoint, 1, 'ACAS_BRTest_Authentication')
	,('E2AF6C29-2D5C-43A5-A38B-7BA4F2D7C85B', @CS_EndPoint, 2, 'ACAS_BRTest_FHL')
	,('091AB501-1FDA-4F0F-BBB0-BB2E82DBB3D0', @CS_EndPoint, 3, 'ACAS_BRTest_FZB')
	,('3AB4AD2C-024B-4737-81E3-8372223DBDC2', @CS_EndPoint, 4, 'ACAS_BRTest_CheckStatus')

	INSERT INTO [dbo].[eHubCodeMapValue]([CV_CK], [CV_CR], [CV_OutputCode])
	VALUES ('8876E73B-0A7F-46A4-A7FC-130EAAF73F29', @CR_Endpoint, 'https://hom.pucomex.serpro.gov.br/ccta/api/autenticar')
	,('E2AF6C29-2D5C-43A5-A38B-7BA4F2D7C85B', @CR_Endpoint, 'https://val.portalunico.siscomex.gov.br/ccta/api/ext/incoming/xfhl?cnpj={cnpjValue}')
	,('091AB501-1FDA-4F0F-BBB0-BB2E82DBB3D0', @CR_Endpoint, 'https://val.portalunico.siscomex.gov.br/ccta/api/ext/incoming/xfzb?cnpj={cnpjValue}')
	,('3AB4AD2C-024B-4737-81E3-8372223DBDC2', @CR_Endpoint, 'https://val.portalunico.siscomex.gov.br/ccta/api/ext/check/received-files?date={yyyy-MM-dd}&cnpj={cnpjValue}')


DECLARE @CS_RetryConfig uniqueidentifier = 'FBE50827-D76D-488C-A0A1-3C6AF09DB439'
DECLARE @CR_RetryConfig uniqueidentifier = '41BA061F-D176-4374-9AFC-2AACEF63B114'

INSERT INTO [dbo].[eHubCodeSet]([CS_PK], [CS_Name], [CS_TS], [CS_CC_Sender], [CS_CC_Recipient], [CS_Key1Name])
	VALUES (@CS_RetryConfig, 'Retry Configuration', @TS_ACASBR, @CC_ACAS_BR, @CC_ACAS_BR, 'Type')

INSERT INTO [dbo].[eHubCodeSetResult]([CR_PK], [CR_CS], [CR_Order], [CR_Name])
	VALUES (@CS_RetryConfig, @CR_RetryConfig, 1, 'Value')


	INSERT INTO [dbo].[eHubCodeMapKey]([CK_PK], [CK_CS], [CK_Order], [CK_Key1Value])
	VALUES ('E9C0C304-C47D-4409-AC09-82E038CC6F53', @CS_RetryConfig, 1, 'RetryIntervalInSeconds')
	,('47B1C3A0-428A-4C62-8B49-A15C48D8C8D0', @CS_RetryConfig, 2, 'RetryTimes')
	,('DA789E99-CB8C-4BB0-A4C4-30CBA96833A3', @CS_RetryConfig, 3, '%')

	INSERT INTO [dbo].[eHubCodeMapValue]([CV_CK], [CV_CR], [CV_OutputCode])
	VALUES ('E9C0C304-C47D-4409-AC09-82E038CC6F53', @CR_RetryConfig, '60')
	,('47B1C3A0-428A-4C62-8B49-A15C48D8C8D0', @CR_RetryConfig, '1000')
	,('DA789E99-CB8C-4BB0-A4C4-30CBA96833A3', @CR_RetryConfig, '')


ALTER TABLE eHubClient WITH CHECK CHECK CONSTRAINT ALL
ALTER TABLE eHubRoutingRule WITH CHECK CHECK CONSTRAINT ALL

--COMMIT

WHILE @@TRANCOUNT > 0
	ROLLBACK



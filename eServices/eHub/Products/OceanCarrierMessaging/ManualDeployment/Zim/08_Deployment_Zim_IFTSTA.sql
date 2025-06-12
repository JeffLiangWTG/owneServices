use eHubTransactions;
GO  
SET XACT_ABORT ON;  
GO  
BEGIN TRANSACTION;

DECLARE @CarrierName				varchar(50) = 'ZIM'

DECLARE @ZIM_CCPK					uniqueidentifier = (SELECT CC_PK PK FROM eHubClient WHERE CC_ID = @CarrierName)

DECLARE @ZIM_IFTSTA_TSPK			uniqueIdentifier = '30BC35CB-FEC2-4214-B14B-3F1E959E6940'	-- SELECT NEWID()

DECLARE @IFTSTAGeneric_TransType	uniqueIdentifier = (SELECT TT_PK FROM eHubTransformationType where TT_TransformationType = 'CargoWise.eHub.Products.GCT.Transforms.IFTSTA2UInterchange_Generic, CargoWise.eHub.Products.GCT.Transforms.IFTSTA2UInterchange_Generic, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350')
DECLARE @UII2UI_TransType			uniqueIdentifier = (SELECT TT_PK FROM eHubTransformationType where TT_TransformationType = 'CargoWise.eHub.Clients.EDI.Universal_2012_11.Transforms.UniversalInterchangeInclude2UniversalInterchange, CargoWise.eHub.Clients.EDI.Universal_2012_11, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350')

DECLARE @D99B_IFTSTA_DTPK			uniqueIdentifier = (SELECT DT_PK from eHubMessageType WHERE DT_Code = 'http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006#EFACT_D99B_IFTSTA')

------------------ eHubTransformationSet / eHubTransformationMapping
INSERT INTO eHubTransformationSet
(TS_PK, TS_Name, TS_CC_Sender, TS_CC_Recipient, TS_DT_Source, TS_BillingInterfaceName, TS_BillSender, TS_BillRecipient)
SELECT @ZIM_IFTSTA_TSPK, 'ZIM IFTSTA to UniversalInterchange', @ZIM_CCPK, null, @D99B_IFTSTA_DTPK , 'ZIM IFTSTA to UniversalInterchange', NULL, 1

INSERT INTO eHubTransformationMapping
(TM_TS_PK, TM_order, TM_TT_PK)
SELECT @ZIM_IFTSTA_TSPK, 0, @IFTSTAGeneric_TransType	UNION ALL
SELECT @ZIM_IFTSTA_TSPK, 1, @UII2UI_TransType

-- Code Set ZIM : Container Status
DECLARE @ContainerStatusCodeSet			uniqueIdentifier = '9B29534C-D8BA-47D1-B2A0-A1F530F00D88'	-- SELECT NEWID()
DECLARE @ResultSet_EventTypesPK			uniqueIdentifier = '906F2C94-E989-4568-8A68-A34F90B7593D'	-- SELECT NEWID()
DECLARE @ResultSet_EventRefrnPK			uniqueIdentifier = '1BB809B9-1203-428D-9EC8-E14E9E15D2CC'	-- SELECT NEWID()
DECLARE @ResultSet_EventParamPK			uniqueIdentifier = '4C3559CE-A24F-4A8D-B5B9-94CD13D1B3E4'	-- SELECT NEWID()
DECLARE @ResultSet_EventIsEstPK			uniqueIdentifier = 'A29CBCAC-57E7-40D5-A6E3-3E330710A3B6'	-- SELECT NEWID()

INSERT eHubCodeSet 
(CS_PK, CS_Name, CS_TS, CS_CC_Sender, CS_CC_Recipient, CS_Key1Name) 
SELECT @ContainerStatusCodeSet, 'Container Status', @ZIM_IFTSTA_TSPK, @ZIM_CCPK, @ZIM_CCPK, 'Code'

INSERT eHubCodeSetResult 
(CR_PK, CR_CS, CR_Order, CR_Name) 
SELECT @ResultSet_EventTypesPK, @ContainerStatusCodeSet, 1, 'Event Type'			UNION ALL
SELECT @ResultSet_EventRefrnPK, @ContainerStatusCodeSet, 2, 'Event Reference'		UNION ALL
SELECT @ResultSet_EventParamPK, @ContainerStatusCodeSet, 3, 'Event Parameters'		UNION ALL
SELECT @ResultSet_EventIsEstPK, @ContainerStatusCodeSet, 4, 'Is Estimate'

INSERT eHubCodeMapKey (CK_PK, CK_CS, CK_Order, CK_Key1Value)             SELECT '47E88E48-2D73-4F39-AC85-30A43CF04822', @ContainerStatusCodeSet, 1, '1'
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '47E88E48-2D73-4F39-AC85-30A43CF04822', @ResultSet_EventTypesPK, 'ARV', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '47E88E48-2D73-4F39-AC85-30A43CF04822', @ResultSet_EventRefrnPK, '', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '47E88E48-2D73-4F39-AC85-30A43CF04822', @ResultSet_EventParamPK, '|Facility=CTO', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '47E88E48-2D73-4F39-AC85-30A43CF04822', @ResultSet_EventIsEstPK, 'FALSE', NULL

INSERT eHubCodeMapKey (CK_PK, CK_CS, CK_Order, CK_Key1Value)             SELECT '5F14BF00-9197-462C-8EE5-568EB76AE251', @ContainerStatusCodeSet, 2, '12'
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '5F14BF00-9197-462C-8EE5-568EB76AE251', @ResultSet_EventTypesPK, 'RLS', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '5F14BF00-9197-462C-8EE5-568EB76AE251', @ResultSet_EventRefrnPK, '', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '5F14BF00-9197-462C-8EE5-568EB76AE251', @ResultSet_EventParamPK, '|Department=Customs and Carrier', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '5F14BF00-9197-462C-8EE5-568EB76AE251', @ResultSet_EventIsEstPK, 'FALSE', NULL

INSERT eHubCodeMapKey (CK_PK, CK_CS, CK_Order, CK_Key1Value)             SELECT 'B1AA6A11-7436-4D79-A02C-B9FE03F1D944', @ContainerStatusCodeSet, 3, '20'
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT 'B1AA6A11-7436-4D79-A02C-B9FE03F1D944', @ResultSet_EventTypesPK, 'ARV', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT 'B1AA6A11-7436-4D79-A02C-B9FE03F1D944', @ResultSet_EventRefrnPK, '', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT 'B1AA6A11-7436-4D79-A02C-B9FE03F1D944', @ResultSet_EventParamPK, '|Facility=CTO', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT 'B1AA6A11-7436-4D79-A02C-B9FE03F1D944', @ResultSet_EventIsEstPK, 'TRUE', NULL

INSERT eHubCodeMapKey (CK_PK, CK_CS, CK_Order, CK_Key1Value)             SELECT '5B9D1D20-56F4-4CC6-ADE1-71E060CC5D7C', @ContainerStatusCodeSet, 4, '21'
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '5B9D1D20-56F4-4CC6-ADE1-71E060CC5D7C', @ResultSet_EventTypesPK, 'FLO', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '5B9D1D20-56F4-4CC6-ADE1-71E060CC5D7C', @ResultSet_EventRefrnPK, '', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '5B9D1D20-56F4-4CC6-ADE1-71E060CC5D7C', @ResultSet_EventParamPK, '|Facility=CTO', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '5B9D1D20-56F4-4CC6-ADE1-71E060CC5D7C', @ResultSet_EventIsEstPK, 'FALSE', NULL

INSERT eHubCodeMapKey (CK_PK, CK_CS, CK_Order, CK_Key1Value)             SELECT '03426AB0-D3B6-475F-BE11-57E2DADFA58B', @ContainerStatusCodeSet, 5, '24'
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '03426AB0-D3B6-475F-BE11-57E2DADFA58B', @ResultSet_EventTypesPK, 'DEP', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '03426AB0-D3B6-475F-BE11-57E2DADFA58B', @ResultSet_EventRefrnPK, '', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '03426AB0-D3B6-475F-BE11-57E2DADFA58B', @ResultSet_EventParamPK, '|Facility=CTO', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '03426AB0-D3B6-475F-BE11-57E2DADFA58B', @ResultSet_EventIsEstPK, 'FALSE', NULL

INSERT eHubCodeMapKey (CK_PK, CK_CS, CK_Order, CK_Key1Value)             SELECT '73C59406-6602-4DE7-AF46-496951571C09', @ContainerStatusCodeSet, 6, '27'
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '73C59406-6602-4DE7-AF46-496951571C09', @ResultSet_EventTypesPK, 'GOU', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '73C59406-6602-4DE7-AF46-496951571C09', @ResultSet_EventRefrnPK, '', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '73C59406-6602-4DE7-AF46-496951571C09', @ResultSet_EventParamPK, '|Facility=CY', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '73C59406-6602-4DE7-AF46-496951571C09', @ResultSet_EventIsEstPK, 'FALSE', NULL

INSERT eHubCodeMapKey (CK_PK, CK_CS, CK_Order, CK_Key1Value)             SELECT '4ECF70F8-C960-4B96-A698-5CF1A05C2EBA', @ContainerStatusCodeSet, 7, '29'
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '4ECF70F8-C960-4B96-A698-5CF1A05C2EBA', @ResultSet_EventTypesPK, 'FUL', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '4ECF70F8-C960-4B96-A698-5CF1A05C2EBA', @ResultSet_EventRefrnPK, '', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '4ECF70F8-C960-4B96-A698-5CF1A05C2EBA', @ResultSet_EventParamPK, '|Facility=CTO', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '4ECF70F8-C960-4B96-A698-5CF1A05C2EBA', @ResultSet_EventIsEstPK, 'FALSE', NULL

INSERT eHubCodeMapKey (CK_PK, CK_CS, CK_Order, CK_Key1Value)             SELECT '14DE859B-A1FE-4A8C-8711-9686EAC774EA', @ContainerStatusCodeSet, 8, '31'
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '14DE859B-A1FE-4A8C-8711-9686EAC774EA', @ResultSet_EventTypesPK, 'FLO', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '14DE859B-A1FE-4A8C-8711-9686EAC774EA', @ResultSet_EventRefrnPK, 'On Rail', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '14DE859B-A1FE-4A8C-8711-9686EAC774EA', @ResultSet_EventParamPK, '|Facility=CTO', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '14DE859B-A1FE-4A8C-8711-9686EAC774EA', @ResultSet_EventIsEstPK, 'FALSE', NULL

INSERT eHubCodeMapKey (CK_PK, CK_CS, CK_Order, CK_Key1Value)             SELECT '4E2DDE75-63D4-436E-ACF7-6F14FEA23D11', @ContainerStatusCodeSet, 9, '40'
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '4E2DDE75-63D4-436E-ACF7-6F14FEA23D11', @ResultSet_EventTypesPK, 'GOU', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '4E2DDE75-63D4-436E-ACF7-6F14FEA23D11', @ResultSet_EventRefrnPK, '', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '4E2DDE75-63D4-436E-ACF7-6F14FEA23D11', @ResultSet_EventParamPK, '|Facility=CTO', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '4E2DDE75-63D4-436E-ACF7-6F14FEA23D11', @ResultSet_EventIsEstPK, 'FALSE', NULL

INSERT eHubCodeMapKey (CK_PK, CK_CS, CK_Order, CK_Key1Value)             SELECT 'E7BEEB54-331C-4879-85C1-F1BAE1CDD8F3', @ContainerStatusCodeSet, 10, '48'
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT 'E7BEEB54-331C-4879-85C1-F1BAE1CDD8F3', @ResultSet_EventTypesPK, 'FLO', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT 'E7BEEB54-331C-4879-85C1-F1BAE1CDD8F3', @ResultSet_EventRefrnPK, '', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT 'E7BEEB54-331C-4879-85C1-F1BAE1CDD8F3', @ResultSet_EventParamPK, '|Facility=CTO', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT 'E7BEEB54-331C-4879-85C1-F1BAE1CDD8F3', @ResultSet_EventIsEstPK, 'FALSE', NULL

INSERT eHubCodeMapKey (CK_PK, CK_CS, CK_Order, CK_Key1Value)             SELECT 'B94BA23A-808E-43EE-92CE-E61C08550CC8', @ContainerStatusCodeSet, 11, '59'
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT 'B94BA23A-808E-43EE-92CE-E61C08550CC8', @ResultSet_EventTypesPK, 'FUL', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT 'B94BA23A-808E-43EE-92CE-E61C08550CC8', @ResultSet_EventRefrnPK, 'Rail', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT 'B94BA23A-808E-43EE-92CE-E61C08550CC8', @ResultSet_EventParamPK, '|Facility=CTO', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT 'B94BA23A-808E-43EE-92CE-E61C08550CC8', @ResultSet_EventIsEstPK, 'FALSE', NULL

INSERT eHubCodeMapKey (CK_PK, CK_CS, CK_Order, CK_Key1Value)             SELECT '6D1792C9-F252-49D3-B04A-AA13971209D0', @ContainerStatusCodeSet, 12, '71'
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '6D1792C9-F252-49D3-B04A-AA13971209D0', @ResultSet_EventTypesPK, 'CAV', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '6D1792C9-F252-49D3-B04A-AA13971209D0', @ResultSet_EventRefrnPK, '', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '6D1792C9-F252-49D3-B04A-AA13971209D0', @ResultSet_EventParamPK, '|Facility=CTO', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '6D1792C9-F252-49D3-B04A-AA13971209D0', @ResultSet_EventIsEstPK, 'FALSE', NULL

INSERT eHubCodeMapKey (CK_PK, CK_CS, CK_Order, CK_Key1Value)             SELECT '50F64CB4-5BC7-4242-BABF-A3F980724D85', @ContainerStatusCodeSet, 13, '74'
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '50F64CB4-5BC7-4242-BABF-A3F980724D85', @ResultSet_EventTypesPK, 'GIN', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '50F64CB4-5BC7-4242-BABF-A3F980724D85', @ResultSet_EventRefrnPK, '', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '50F64CB4-5BC7-4242-BABF-A3F980724D85', @ResultSet_EventParamPK, '|Facility=CTO', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '50F64CB4-5BC7-4242-BABF-A3F980724D85', @ResultSet_EventIsEstPK, 'FALSE', NULL

INSERT eHubCodeMapKey (CK_PK, CK_CS, CK_Order, CK_Key1Value)             SELECT '839C8010-F78E-40C8-A993-274FAB497BAC', @ContainerStatusCodeSet, 14, '80'
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '839C8010-F78E-40C8-A993-274FAB497BAC', @ResultSet_EventTypesPK, 'DHR', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '839C8010-F78E-40C8-A993-274FAB497BAC', @ResultSet_EventRefrnPK, '', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '839C8010-F78E-40C8-A993-274FAB497BAC', @ResultSet_EventParamPK, '|Facility=CY', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '839C8010-F78E-40C8-A993-274FAB497BAC', @ResultSet_EventIsEstPK, 'FALSE', NULL


ROLLBACK
--COMMIT


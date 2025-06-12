BEGIN TRAN

DECLARE @GEI_TAIWAN_ClientPK   uniqueIdentifier = (Select CC_PK from eHubClient where CC_ID = 'GEI_TAIWAN')
DECLARE @GEI_TAIWANTest_ClientPK   uniqueIdentifier = (Select CC_PK from eHubClient where CC_ID = 'GEI_TAIWANTest')
DECLARE @TransformationSetPK1   uniqueIdentifier = '01C2EB15-14FC-401B-BF1D-1CF3C69AF415'
DECLARE @TransformationSetPK2   uniqueIdentifier = '87A701C2-76A2-4049-A733-2C271096BD44'
DECLARE @TransformationTypePK   uniqueIdentifier = '2034E568-B8F5-43DA-A24A-7B998BC97E86'
DECLARE @GEITaiwanMessageTypePK	uniqueIdentifier = (select DT_PK from eHubMessageType where DT_Code = 'http://cargowise.com/ehub/products/Accounting/GlobalElectronicInvoicing#GlobalElectronicInvoicing')
DECLARE @GEITaiwanPayloadMessageTypePK	uniqueIdentifier = (select DT_PK from eHubMessageType where DT_Code = 'http://cargowise.com/ehub/products/GlobalInvoice/Taiwan#Payload')

------------eHubTransformationSet

INSERT INTO eHubTransactions..eHubTransformationSet
(TS_PK, TS_Name, TS_CC_Sender, TS_CC_Recipient, TS_DT_Source, TS_BillingInterfaceName, TS_BillSender, TS_BillRecipient)
SELECT @TransformationSetPK1, 'Global Electronic Invoicing to Taiwan Payload', null , @GEI_TAIWAN_ClientPK, @GEITaiwanMessageTypePK , 'Global Electronic Invoicing to Taiwan Payload',1,NULL

INSERT INTO eHubTransactions..eHubTransformationSet
(TS_PK, TS_Name, TS_CC_Sender, TS_CC_Recipient, TS_DT_Source, TS_BillingInterfaceName, TS_BillSender, TS_BillRecipient)
SELECT @TransformationSetPK2, 'Global Electronic Invoicing Test to Taiwan Payload', null , @GEI_TAIWANTest_ClientPK, @GEITaiwanMessageTypePK , 'Global Electronic Invoicing Test to Taiwan Payload',1,NULL

------------eHubTransformationType

INSERT INTO eHubTransformationType (TT_PK, TT_DT_Source, TT_DT_Target, TT_TransformationType)
select @TransformationTypePK, @GEITaiwanMessageTypePK, @GEITaiwanPayloadMessageTypePK, 
'CargoWise.eHub.Products.GlobalInvoice.Taiwan.Transforms.GlobalElectronicInvoice2Payload, CargoWise.eHub.Products.GlobalInvoice.Taiwan.Transforms.GlobalElectronicInvoice2Payload, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350'

------------eHubTransformationMapping

INSERT INTO eHubTransactions..eHubTransformationMapping
(TM_TS_PK, TM_order, TM_TT_PK)
SELECT @TransformationSetPK1, 0, @TransformationTypePK

INSERT INTO eHubTransactions..eHubTransformationMapping
(TM_TS_PK, TM_order, TM_TT_PK)
SELECT @TransformationSetPK2, 0, @TransformationTypePK

ROLLBACK;
--COMMIT

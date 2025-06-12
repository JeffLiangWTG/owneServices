Use eHubTransactions

BEGIN TRAN
DECLARE @GEI_TAIWAN_ClientPK				uniqueidentifier = '3124EE76-AC76-4D05-BB41-6EBA13282434'	--SELECT NEWID()
DECLARE @GEI_TAIWAN_Test_ClientPK			uniqueidentifier = '5A9A0F1D-EDAD-409E-909A-DE7017AE401B'	--SELECT NEWID()
DECLARE @GEI_TAIWAN_GLB_ELEC_INVOICING_Rule		uniqueidentifier = 'E6978FA9-B760-441A-9BE4-FCD47EFB5E55'	--SELECT NEWID()
DECLARE @GEI_TAIWAN_GLB_ELEC_INVOICINGTest_Rule		uniqueidentifier = '3F4CD119-C6F9-4EA5-B74F-DEE0D8E5A898'	--SELECT NEWID()

INSERT INTO eHubClient
(CC_PK, CC_ID, CC_FriendlyName, CC_Odyssey_OH, CC_DistributionZone, CC_EmailAddress, CC_Password, CC_OwnerCategory, CC_SystemCategory)
SELECT @GEI_TAIWAN_ClientPK, 'GEI_TAIWAN', 'Global Electronic Invoicing - Taiwan','00000000-0000-0000-0000-000000000000','75419F4C-C522-4890-BD5D-BCA5E12268F6','','','Service Provider','Third Party' 

INSERT INTO eHubClient
(CC_PK, CC_ID, CC_FriendlyName, CC_Odyssey_OH, CC_DistributionZone, CC_EmailAddress, CC_Password, CC_OwnerCategory, CC_SystemCategory)
SELECT @GEI_TAIWAN_Test_ClientPK, 'GEI_TAIWANTest', 'Global Electronic Invoicing - Taiwan - Test','00000000-0000-0000-0000-000000000000','75419F4C-C522-4890-BD5D-BCA5E12268F6','','','Service Provider','Third Party' 

INSERT INTO eHubRoutingRule (RR_PK, RR_Condition_Expression, RR_Group_RR_GroupRule, RR_Group_MatchMultiple, RR_Success_CC_Recipient, RR_Group_Ordering)
VALUES (@GEI_TAIWAN_GLB_ELEC_INVOICING_Rule, '[@MessagingSystem,Equal,Taiwan electronic invoicing system]', '7ED31F42-F8C4-40A2-B7D3-695FAF56DC65', null, @GEI_TAIWAN_ClientPK, 200)

INSERT INTO eHubRoutingRule (RR_PK, RR_Condition_Expression, RR_Group_RR_GroupRule, RR_Group_MatchMultiple, RR_Success_CC_Recipient, RR_Group_Ordering)
VALUES (@GEI_TAIWAN_GLB_ELEC_INVOICINGTest_Rule, '[@MessagingSystem,Equal,Taiwan electronic invoicing system]', '993A77C0-0527-4751-A0A3-77187E238ED7', null, @GEI_TAIWAN_Test_ClientPK, 200)

ROLLBACK
--COMMIT
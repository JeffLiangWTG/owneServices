begin tran

DECLARE @GBCustoms uniqueIdentifier = 'A09A4C93-17A8-4F86-BA35-FE55C824CB7B' -- NEWID()
DECLARE @GBCustomsTest uniqueIdentifier = '6B41ECE4-ABA6-479B-A74F-92B19717F474' -- NEWID()
DECLARE @GBCustoms_Direct uniqueIdentifier = '1009DE17-E13A-4D89-A3BF-F703EABA38BD' -- NEWID()
DECLARE @GBCustomsTest_Direct uniqueIdentifier = 'AC9FE213-29C8-4273-A5B6-F74988B175BE' -- NEWID()

INSERT INTO eHubTransactions..eHubClient
(CC_PK, CC_ID, CC_FriendlyName, CC_Odyssey_OH, CC_DistributionZone, CC_EmailAddress, CC_Password, CC_OwnerCategory, CC_SystemCategory)
select @GBCustoms,'GBCustoms','Great Britain Customs','00000000-0000-0000-0000-000000000000',null,'','','Service Provider','Third Party'

INSERT INTO eHubTransactions..eHubClient
(CC_PK, CC_ID, CC_FriendlyName, CC_Odyssey_OH, CC_DistributionZone, CC_EmailAddress, CC_Password, CC_OwnerCategory, CC_SystemCategory)
select @GBCustomsTest,'GBCustomsTest','Great Britain Customs Test','00000000-0000-0000-0000-000000000000',null,'','','Service Provider','Third Party'

INSERT INTO eHubTransactions..eHubClient
(CC_PK, CC_ID, CC_FriendlyName, CC_Odyssey_OH, CC_DistributionZone, CC_EmailAddress, CC_Password, CC_OwnerCategory, CC_SystemCategory)
select @GBCustoms_Direct,'GBCustoms-Direct','Great Britain Customs - CDS Direct','00000000-0000-0000-0000-000000000000','75419F4C-C522-4890-BD5D-BCA5E12268F6','','','Service Provider','Third Party'

INSERT INTO eHubTransactions..eHubClient
(CC_PK, CC_ID, CC_FriendlyName, CC_Odyssey_OH, CC_DistributionZone, CC_EmailAddress, CC_Password, CC_OwnerCategory, CC_SystemCategory)
select @GBCustomsTest_Direct,'GBCustomsTest-Direct','Great Britain Customs Test - CDS Direct','00000000-0000-0000-0000-000000000000','75419F4C-C522-4890-BD5D-BCA5E12268F6','','','Service Provider','Third Party'

--- TransformationSet

DECLARE @GBTranset uniqueIdentifier = '066EE0BB-4E8B-4FFA-86BE-30633AD97F32' -- NEWID()

INSERT INTO eHubTransactions..eHubTransformationSet
(TS_PK, TS_Name, TS_CC_Sender, TS_CC_Recipient)
select @GBTranset,'GBCustoms System Configuration', @GBCustoms, @GBCustoms

--- Code Mapping

INSERT INTO eHubTransactions..eHubCodeSet([CS_PK], [CS_Name], [CS_TS], [CS_CC_Sender], [CS_CC_Recipient], [CS_Key1Name], [CS_Key2Name], [CS_Key3Name], [CS_Key4Name], [CS_Key5Name])
SELECT N'3efa0148-5ec3-4ecb-8904-c2ebdc700e6a', N'Endpoints', @GBTranset, @GBCustoms, @GBCustoms, N'DestinationParty', N'Service', NULL, NULL, NULL

INSERT INTO eHubTransactions..eHubCodeSetResult([CR_PK], [CR_CS], [CR_Order], [CR_Name])
SELECT N'd19a9616-3872-4457-867b-0588affd034a', N'3efa0148-5ec3-4ecb-8904-c2ebdc700e6a', 1, N'Endpoint URL'

INSERT INTO eHubTransactions..eHubCodeMapKey([CK_PK], [CK_CS], [CK_Order], [CK_Key1Value], [CK_Key2Value], [CK_Key3Value], [CK_Key4Value], [CK_Key5Value])
SELECT N'3d0e1b53-fcf0-4401-98e4-63de6c2c836f', N'3efa0148-5ec3-4ecb-8904-c2ebdc700e6a', 9, N'%', N'%', NULL, NULL, NULL UNION ALL
SELECT N'c509f371-b4f0-4c3b-9d79-532e7be197f6', N'3efa0148-5ec3-4ecb-8904-c2ebdc700e6a', 2, N'GBCustoms-Direct', N'Cancel', NULL, NULL, NULL UNION ALL
SELECT N'b928de1f-c23a-4107-946e-f351ec0456bf', N'3efa0148-5ec3-4ecb-8904-c2ebdc700e6a', 3, N'GBCustoms-Direct', N'Export inventory', NULL, NULL, NULL UNION ALL
SELECT N'46685e52-6590-4bcc-b4f8-fad5b80757b3', N'3efa0148-5ec3-4ecb-8904-c2ebdc700e6a', 4, N'GBCustoms-Direct', N'File upload', NULL, NULL, NULL UNION ALL
SELECT N'0df92135-a274-4a0f-bf33-774948be3240', N'3efa0148-5ec3-4ecb-8904-c2ebdc700e6a', 1, N'GBCustoms-Direct', N'New', NULL, NULL, NULL UNION ALL
SELECT N'f0992c38-366a-42db-bd92-aa2841c16318', N'3efa0148-5ec3-4ecb-8904-c2ebdc700e6a', 6, N'GBCustomsTest-Direct', N'Cancel', NULL, NULL, NULL UNION ALL
SELECT N'b0d1fb35-5ff6-4611-a85c-534949e3ecd2', N'3efa0148-5ec3-4ecb-8904-c2ebdc700e6a', 7, N'GBCustomsTest-Direct', N'Export inventory', NULL, NULL, NULL UNION ALL
SELECT N'b2cc3ffc-8ef7-4b18-a983-986d5ba37141', N'3efa0148-5ec3-4ecb-8904-c2ebdc700e6a', 8, N'GBCustomsTest-Direct', N'File upload', NULL, NULL, NULL UNION ALL
SELECT N'38c7be12-ea1c-4324-83c8-8baf9e9dae1b', N'3efa0148-5ec3-4ecb-8904-c2ebdc700e6a', 5, N'GBCustomsTest-Direct', N'New', NULL, NULL, NULL

INSERT INTO eHubTransactions..eHubCodeMapValue([CV_CK], [CV_CR], [CV_OutputCode], [CV_PassThroughKey])
SELECT N'46685e52-6590-4bcc-b4f8-fad5b80757b3', N'd19a9616-3872-4457-867b-0588affd034a', N'https://api.service.hmrc.gov.uk/customs/declarations/file-upload/', NULL UNION ALL
SELECT N'0df92135-a274-4a0f-bf33-774948be3240', N'd19a9616-3872-4457-867b-0588affd034a', N'https://api.service.hmrc.gov.uk/customs/declarations/', NULL UNION ALL
SELECT N'b928de1f-c23a-4107-946e-f351ec0456bf', N'd19a9616-3872-4457-867b-0588affd034a', N'https://api.service.hmrc.gov.uk/customs/inventory-linking/exports/', NULL UNION ALL
SELECT N'f0992c38-366a-42db-bd92-aa2841c16318', N'd19a9616-3872-4457-867b-0588affd034a', N'https://test-api.service.hmrc.gov.uk/customs/declarations/cancellation-requests/', NULL UNION ALL
SELECT N'3d0e1b53-fcf0-4401-98e4-63de6c2c836f', N'd19a9616-3872-4457-867b-0588affd034a', N'', NULL UNION ALL
SELECT N'c509f371-b4f0-4c3b-9d79-532e7be197f6', N'd19a9616-3872-4457-867b-0588affd034a', N'https://api.service.hmrc.gov.uk/customs/declarations/cancellation-requests/', NULL UNION ALL
SELECT N'b0d1fb35-5ff6-4611-a85c-534949e3ecd2', N'd19a9616-3872-4457-867b-0588affd034a', N'https://test-api.service.hmrc.gov.uk/customs/inventory-linking/exports/', NULL UNION ALL
SELECT N'38c7be12-ea1c-4324-83c8-8baf9e9dae1b', N'd19a9616-3872-4457-867b-0588affd034a', N'https://test-api.service.hmrc.gov.uk/customs/declarations/', NULL UNION ALL
SELECT N'b2cc3ffc-8ef7-4b18-a983-986d5ba37141', N'd19a9616-3872-4457-867b-0588affd034a', N'https://test-api.service.hmrc.gov.uk/customs/declarations/file-upload/', NULL

-- Client Registration

DECLARE @GBC_Authorisation uniqueIdentifier = 'f84fd390-96f1-4b7c-810b-b45320c5a71b' -- NEWID()

INSERT INTO eHubTransactions..eHubRegistrationType([RT_PK], [RT_ID], [RT_Description], [RT_RegistrantType])
SELECT @GBC_Authorisation, N'GBCustoms-Direct', N'Authorisation Headers', N'Client'

INSERT INTO eHubTransactions..eHubClientRegistration([CX_PK], [CX_CC], [CX_RT], [CX_Qualifier], [CX_Code])
SELECT N'dbf7fdc9-4e56-4443-a1bf-161e857413e6', @GBCustomsTest_Direct, @GBC_Authorisation, N'CURRENT', N'f96987a166fdce908d8b51a4eee1123' UNION ALL
SELECT N'b3577c1b-c1c2-44c5-9d84-164386087583', @GBCustomsTest_Direct, @GBC_Authorisation, N'NEXT', N'' UNION ALL
SELECT N'ea0e1989-e8d5-4794-ac9d-992235bc59b5', @GBCustomsTest_Direct, @GBC_Authorisation, N'OLD', N'' UNION ALL
SELECT N'5227961f-cc93-4542-9455-426833115853', @GBCustoms_Direct, @GBC_Authorisation, N'CURRENT', N'f96987a166fdce908d8b51a4eee1123' UNION ALL
SELECT N'd1f30eba-d630-4444-8ff3-1f5b76df4220', @GBCustoms_Direct, @GBC_Authorisation, N'NEXT', N'' UNION ALL
SELECT N'd260dbb9-ad6a-44e1-9049-d27266fb5802', @GBCustoms_Direct, @GBC_Authorisation, N'OLD', N''


rollback
--commit
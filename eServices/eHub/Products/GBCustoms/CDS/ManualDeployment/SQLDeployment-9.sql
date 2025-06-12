BEGIN TRAN
DECLARE @CC_PK_GBCustoms UNIQUEIDENTIFIER = (SELECT CC_PK FROM eHubClient WHERE CC_ID = 'GBCustoms')
DECLARE @TS_PK UNIQUEIDENTIFIER = (SELECT TS_PK FROM eHubTransformationSet WHERE TS_Name = 'GBCustoms System Configuration')
DECLARE @CK_CS UNIQUEIDENTIFIER = (SELECT CS_PK FROM eHubCodeSet WHERE CS_CC_Sender = @CC_PK_GBCustoms and CS_TS = @TS_PK)
DECLARE @CR_PK_Endpoint UNIQUEIDENTIFIER = (SELECT CR_PK FROM eHubCodeSetResult WHERE CR_CS = @CK_CS and CR_Name = 'Endpoint URL')
DECLARE @CR_PK_Version UNIQUEIDENTIFIER = (SELECT CR_PK FROM eHubCodeSetResult WHERE CR_CS = @CK_CS and CR_Name = 'Version')

--Update Order
UPDATE eHubCodeMapKey SET CK_Order = CK_Order + 1 WHERE CK_CS = @CK_CS and CK_Order > 10

--Update last record
DECLARE @LastOrder INT = (SELECT CK_Order FROM eHubCodeMapKey WHERE CK_CS = @CK_CS and CK_Key1Value = '%')
UPDATE eHubCodeMapKey SET CK_Order = CK_Order + 1 WHERE CK_CS = @CK_CS and CK_Order = @LastOrder

--INSERT GoodsPresentationNotification PROD
DECLARE @CK_PK_Prod UNIQUEIDENTIFIER = 'EB7F13E4-3C24-4392-AFF0-479E557B33A9' --SELECT NEWID()

INSERT INTO eHubCodeMapKey 
SELECT @CK_PK_Prod, @CK_CS, 11, 'GBCustoms-Direct', 'GoodsPresentationNotification', NULL, NULL, NULL

INSERT INTO eHubCodeMapValue
SELECT @CK_PK_Prod, @CR_PK_Endpoint, 'https://api.service.hmrc.gov.uk/customs/declarations/arrival-notification', NULL
UNION
SELECT @CK_PK_Prod, @CR_PK_Version, '2.0', NULL

--INSERT GoodsPresentationNotification TEST
DECLARE @CK_PK_Test UNIQUEIDENTIFIER = 'B138C1D2-79DD-4951-9B9C-77E5716B891D' --SELECT NEWID()

INSERT INTO eHubCodeMapKey 
SELECT @CK_PK_Test, @CK_CS, @LastOrder, 'GBCustomsTest-Direct', 'GoodsPresentationNotification', NULL, NULL, NULL

INSERT INTO eHubCodeMapValue
SELECT @CK_PK_Test, @CR_PK_Endpoint, 'https://test-api.service.hmrc.gov.uk/customs/declarations/arrival-notification', NULL
UNION
SELECT @CK_PK_Test, @CR_PK_Version, '2.0', NULL

SELECT * FROM [dbo].[eHubCodeMapKey] WHERE [CK_CS] = '3EFA0148-5EC3-4ECB-8904-C2EBDC700E6A' order by CK_Order
SELECT * FROM [dbo].[eHubCodeMapValue] WHERE [CV_CK] IN (@CK_PK_Prod, @CK_PK_Test)
ROLLBACK TRAN
--COMMIT TRAN

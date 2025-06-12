USE eHubTransactions

BEGIN TRAN

-- Code Mapping
DECLARE @GBCustomsDirect_PullNotifications_CK_ORDER int = (SELECT CK_Order FROM eHubTransactions..eHubCodeMapKey WHERE CK_Key1Value = 'GBCustoms-Direct' AND CK_Key2Value = 'PullNotifications')

IF @GBCustomsDirect_PullNotifications_CK_ORDER IS NOT NULL
	BEGIN
		UPDATE eHubTransactions..eHubCodeMapKey
		SET CK_Order = CK_Order + 2
		WHERE CK_Order > @GBCustomsDirect_PullNotifications_CK_ORDER AND CK_CS = '3EFA0148-5EC3-4ECB-8904-C2EBDC700E6A'
	END
ELSE
	BEGIN
		SELECT @GBCustomsDirect_PullNotifications_CK_ORDER = MAX(CK_Order) FROM eHubTransactions..eHubCodeMapKey WHERE CK_Key1Value = 'GBCustoms-Direct'
	END

INSERT INTO eHubTransactions..eHubCodeMapKey([CK_PK], [CK_CS], [CK_Order], [CK_Key1Value], [CK_Key2Value])
SELECT N'BA9B25B8-ED07-48F2-8149-5A6BB1E54C55', N'3efa0148-5ec3-4ecb-8904-c2ebdc700e6a', @GBCustomsDirect_PullNotifications_CK_ORDER+1, N'GBCustoms-Direct', N'PullNotifications-ResponseToDeclarations' UNION ALL
SELECT N'E08F7B46-F943-4513-AFE7-F2D29BDC4DF4', N'3efa0148-5ec3-4ecb-8904-c2ebdc700e6a', @GBCustomsDirect_PullNotifications_CK_ORDER+2, N'GBCustoms-Direct', N'PullNotifications-ResponseToInventoryRequest'

INSERT INTO eHubTransactions..eHubCodeMapValue([CV_CK], [CV_CR], [CV_OutputCode], [CV_PassThroughKey])
SELECT N'BA9B25B8-ED07-48F2-8149-5A6BB1E54C55', N'd19a9616-3872-4457-867b-0588affd034a', N'REPLACEhttps://gbcdsws-test.wisegrid.net/InboundMessage/ReceiveResponseToDeclarations', NULL UNION ALL
SELECT N'E08F7B46-F943-4513-AFE7-F2D29BDC4DF4', N'd19a9616-3872-4457-867b-0588affd034a', N'REPLACEhttps://gbcdsws-test.wisegrid.net/InboundMessage/ReceiveResponseToInventoryRequests', NULL

DECLARE @GBCustomsTest_Direct_PullNotifications_CK_ORDER int = (SELECT CK_Order FROM eHubTransactions..eHubCodeMapKey WHERE CK_Key1Value = 'GBCustomsTest-Direct' AND CK_Key2Value = 'PullNotifications')

IF @GBCustomsTest_Direct_PullNotifications_CK_ORDER IS NOT NULL

	BEGIN
		UPDATE eHubTransactions..eHubCodeMapKey
		SET CK_Order = CK_Order + 2
		WHERE CK_Order > @GBCustomsTest_Direct_PullNotifications_CK_ORDER AND CK_CS = '3EFA0148-5EC3-4ECB-8904-C2EBDC700E6A'
	END
ELSE
	BEGIN
		SELECT @GBCustomsDirect_PullNotifications_CK_ORDER = MAX(CK_Order) FROM eHubTransactions..eHubCodeMapKey WHERE CK_Key1Value = 'GBCustoms-Direct'
	END

INSERT INTO eHubTransactions..eHubCodeMapKey([CK_PK], [CK_CS], [CK_Order], [CK_Key1Value], [CK_Key2Value])
SELECT N'DDAA70D3-3D86-4451-B298-60A43316B1B1', N'3efa0148-5ec3-4ecb-8904-c2ebdc700e6a', @GBCustomsTest_Direct_PullNotifications_CK_ORDER+1, N'GBCustomsTest-Direct', N'PullNotifications-ResponseToDeclarations' UNION ALL
SELECT N'4F9DD4EF-31E0-4778-97DC-8E4CA0F828E3', N'3efa0148-5ec3-4ecb-8904-c2ebdc700e6a', @GBCustomsTest_Direct_PullNotifications_CK_ORDER+2, N'GBCustomsTest-Direct', N'PullNotifications-ResponseToInventoryRequest'

INSERT INTO eHubTransactions..eHubCodeMapValue([CV_CK], [CV_CR], [CV_OutputCode], [CV_PassThroughKey])
SELECT N'DDAA70D3-3D86-4451-B298-60A43316B1B1', N'd19a9616-3872-4457-867b-0588affd034a', N'https://gbcdsws-test.wisegrid.net/InboundMessage/ReceiveResponseToDeclarations', NULL UNION ALL
SELECT N'4F9DD4EF-31E0-4778-97DC-8E4CA0F828E3', N'd19a9616-3872-4457-867b-0588affd034a', N'https://gbcdsws-test.wisegrid.net/InboundMessage/ReceiveResponseToInventoryRequests', NULL

ROLLBACK
--COMMIT
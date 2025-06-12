Use eHubTransactions;
BEGIN TRAN

DECLARE @RT_PK				UNIQUEIDENTIFIER = (SELECT RT_PK FROM eHubRegistrationType WHERE RT_ID = 'GBCustoms-Transport')
DECLARE @RT_EMCS_PK			UNIQUEIDENTIFIER = (SELECT RT_PK FROM eHubRegistrationType WHERE RT_ID = 'GBCustoms-EMCS')
DECLARE @CC_ICSGB_PK		UNIQUEIDENTIFIER = (SELECT CC_PK FROM eHubClient WHERE CC_ID = 'GBCustoms-ICSGB')
DECLARE @CC_ICSGB_Test_PK	UNIQUEIDENTIFIER = (SELECT CC_PK FROM eHubClient WHERE CC_ID = 'GBCustomsTest-ICSGB')
DECLARE @CC_CTCGB_PK		UNIQUEIDENTIFIER = (SELECT CC_PK FROM eHubClient WHERE CC_ID = 'GBCustoms-CTCGB')
DECLARE @CC_CTCGB_Test_PK	UNIQUEIDENTIFIER = (SELECT CC_PK FROM eHubClient WHERE CC_ID = 'GBCustomsTest-CTCGB')
DECLARE @CC_GVMS_PK			UNIQUEIDENTIFIER = (SELECT CC_PK FROM eHubClient WHERE CC_ID = 'GBCustoms-GVMS')
DECLARE @CC_GVMS_Test_PK	UNIQUEIDENTIFIER = (SELECT CC_PK FROM eHubClient WHERE CC_ID = 'GBCustomsTest-GVMS')
DECLARE @CC_EMCS_PK			UNIQUEIDENTIFIER = (SELECT CC_PK FROM eHubClient WHERE CC_ID = 'GBCustoms-EMCS')
DECLARE @CC_EMCS_Test_PK	UNIQUEIDENTIFIER = (SELECT CC_PK FROM eHubClient WHERE CC_ID = 'GBCustomsTest-EMCS')
DECLARE @CX_PK1				UNIQUEIDENTIFIER = '9A3DBD80-F156-4ABF-9445-A3189A5A0A62' --SELECT NEWID()
DECLARE @CX_PK2				UNIQUEIDENTIFIER = '680EC58F-DD02-45B1-9798-EA0030A82CC7' --SELECT NEWID()
DECLARE @CX_PK3				UNIQUEIDENTIFIER = 'AF403862-3413-4344-9F24-CBAB40DC4091' --SELECT NEWID()
DECLARE @CX_PK4				UNIQUEIDENTIFIER = '68CA6857-F1B1-45ED-B6CD-1E843562FA51' --SELECT NEWID()
DECLARE @CX_PK5				UNIQUEIDENTIFIER = 'F6709054-0555-4A81-9C03-57F2B60B9B22' --SELECT NEWID()
DECLARE @CX_PK6				UNIQUEIDENTIFIER = '83052775-2D98-4F83-A8E3-5311BFCCF3AC' --SELECT NEWID()
DECLARE @CX_PK7				UNIQUEIDENTIFIER = 'A60DE02B-0990-4DC8-977E-B1AB54F26C2B' --SELECT NEWID()
DECLARE @CX_PK8				UNIQUEIDENTIFIER = '0FEF747A-CDB6-44FB-BE68-E16758157178' --SELECT NEWID()
DECLARE @CX_PK9				UNIQUEIDENTIFIER = '2D3122DD-9AD0-49AB-884B-97D802340119' --SELECT NEWID()
DECLARE @CX_PK10			UNIQUEIDENTIFIER = '2451EEFC-2BC4-46ED-A5DA-72AB9FA11F7A' --SELECT NEWID()
DECLARE @CX_PK11			UNIQUEIDENTIFIER = 'C2561DE7-2A77-41F5-B64A-35840C9299A0' --SELECT NEWID()
DECLARE @CX_PK12			UNIQUEIDENTIFIER = '80EC08EE-54FE-403F-B22E-494289CDF5D0' --SELECT NEWID()
DECLARE @CX_PK13			UNIQUEIDENTIFIER = 'D97C7A04-2DA1-4FEA-8A9E-6B2193AC0C6B' --SELECT NEWID()
DECLARE @CX_PK14			UNIQUEIDENTIFIER = '402196DB-13CA-4AF8-B059-21E03B235BA2' --SELECT NEWID()
DECLARE @CX_Test_PK1		UNIQUEIDENTIFIER = '4D3E9290-18C8-4318-9B00-1C3C6D2BE7A9' --SELECT NEWID()
DECLARE @CX_Test_PK2		UNIQUEIDENTIFIER = 'F7EB97A2-0A11-4498-A0FC-B7191812704E' --SELECT NEWID()
DECLARE @CX_Test_PK3		UNIQUEIDENTIFIER = 'D7F01168-DF1F-47CD-9878-5224B384DAAE' --SELECT NEWID()
DECLARE @CX_Test_PK4		UNIQUEIDENTIFIER = '11ADF174-85D0-4CF6-AF6B-D0E8DD278ED6' --SELECT NEWID()
DECLARE @CX_Test_PK5		UNIQUEIDENTIFIER = '766FE2F1-6597-42EA-998D-14A6BA5F8C38' --SELECT NEWID()
DECLARE @CX_Test_PK6		UNIQUEIDENTIFIER = 'D0233DCC-9C6B-4A4D-A30F-489AA16EC6F1' --SELECT NEWID()
DECLARE @CX_Test_PK7		UNIQUEIDENTIFIER = 'AB8F5896-E22C-4B5E-8B2E-FF954E1A7257' --SELECT NEWID()
DECLARE @CX_Test_PK8		UNIQUEIDENTIFIER = 'E3B30402-EED1-4181-89EE-376E9CB29D56' --SELECT NEWID()
DECLARE @CX_Test_PK9		UNIQUEIDENTIFIER = '3BB161C4-4E24-4299-95B5-66828901D961' --SELECT NEWID()
DECLARE @CX_Test_PK10		UNIQUEIDENTIFIER = '9981008A-C7CB-453B-86AD-6AF5BDBCA76B' --SELECT NEWID()
DECLARE @CX_Test_PK11		UNIQUEIDENTIFIER = '8F209AEA-72E6-4D17-8603-B9403A1AD000' --SELECT NEWID()
DECLARE @CX_Test_PK12		UNIQUEIDENTIFIER = '414CB01E-7C7C-40CC-AA8D-932861F4F972' --SELECT NEWID()
DECLARE @CX_Test_PK13		UNIQUEIDENTIFIER = 'A603F5CF-C54F-4050-B5E6-9D8510E20D90' --SELECT NEWID()
DECLARE @CX_Test_PK14		UNIQUEIDENTIFIER = '4DE54E18-5C76-4456-94FF-479715666B7A' --SELECT NEWID()

IF @RT_PK IS NULL
	BEGIN
		SELECT @RT_PK = 'A4E6F419-D876-4451-9ABD-564DAD2DF5E8' --NEWID()
	INSERT INTO eHubRegistrationType
	SELECT @RT_PK, 'GBCustoms-Transport', 'GBCustoms Transport Layer ID Type', 'Client'
END

--GBCustoms-EMCS Registration Type
IF @RT_EMCS_PK IS NULL
	BEGIN
		SELECT @RT_EMCS_PK = 'EEBE33BE-4DD3-4021-9C8F-58907DDFD711' --NEWID()
	INSERT INTO eHubRegistrationType
	SELECT @RT_EMCS_PK, 'GBCustoms-EMCS', 'GBCustoms Transport Layer EMCS credentials', 'Client'
END

--GBCustoms-ICSGB CLIENT
If (@CC_ICSGB_PK Is Null)
Begin
	SELECT @CC_ICSGB_PK = 'B0B33CD3-C5F9-4843-9F8A-251C768BEC04' --NEWID()
	INSERT INTO eHubClient (CC_PK, CC_ID, CC_FriendlyName, CC_Odyssey_OH, CC_DistributionZone, CC_EmailAddress, CC_Password, CC_OwnerCategory, CC_SystemCategory, CC_RequireStatusResponse, CC_NotificationForInboxRecipient, CC_IsLegacyXmlAllowed, CC_PermitInboxSender, CC_PermitInboxRecipient)
	SELECT @CC_ICSGB_PK, 'GBCustoms-ICSGB', 'Great Britain Customs - ICS GB', '00000000-0000-0000-0000-000000000000', '75419F4C-C522-4890-BD5D-BCA5E12268F6', '', '', 'Service Provider', 'Third Party', 0, 0, 0, 1, 1
End

--GBCustomsTest-ICSGB CLIENT
If (@CC_ICSGB_Test_PK Is Null)
Begin
	SELECT @CC_ICSGB_Test_PK = '2741C6DD-A156-4F43-B4D8-6A45C24521C2' --NEWID()
	INSERT INTO eHubClient (CC_PK, CC_ID, CC_FriendlyName, CC_Odyssey_OH, CC_DistributionZone, CC_EmailAddress, CC_Password, CC_OwnerCategory, CC_SystemCategory, CC_RequireStatusResponse, CC_NotificationForInboxRecipient, CC_IsLegacyXmlAllowed, CC_PermitInboxSender, CC_PermitInboxRecipient)
	SELECT @CC_ICSGB_Test_PK, 'GBCustomsTest-ICSGB', 'Great Britain Customs Test - ICS GB', '00000000-0000-0000-0000-000000000000', '75419F4C-C522-4890-BD5D-BCA5E12268F6', '', '', 'Service Provider', 'Third Party', 0, 0, 0, 1, 1
End

--GBCustoms-CTCGB CLIENT
If (@CC_CTCGB_PK Is Null)
Begin
	SELECT @CC_CTCGB_PK = 'E3D6010D-CF67-4AA2-BC7B-B3D5B7E2ECD6' --NEWID()
	INSERT INTO eHubClient (CC_PK, CC_ID, CC_FriendlyName, CC_Odyssey_OH, CC_DistributionZone, CC_EmailAddress, CC_Password, CC_OwnerCategory, CC_SystemCategory, CC_RequireStatusResponse, CC_NotificationForInboxRecipient, CC_IsLegacyXmlAllowed, CC_PermitInboxSender, CC_PermitInboxRecipient)
	SELECT @CC_CTCGB_PK, 'GBCustoms-CTCGB', 'Great Britain Customs - CTC GB', '00000000-0000-0000-0000-000000000000', '75419F4C-C522-4890-BD5D-BCA5E12268F6', '', '', 'Service Provider', 'Third Party', 0, 0, 0, 1, 1
End

--GBCustomsTest-CTCGB CLIENT
If (@CC_CTCGB_Test_PK Is Null)
Begin
	SELECT @CC_CTCGB_Test_PK = 'C54A7723-61CD-43A3-98FF-8EF5E5F29EA8' --NEWID()
	INSERT INTO eHubClient (CC_PK, CC_ID, CC_FriendlyName, CC_Odyssey_OH, CC_DistributionZone, CC_EmailAddress, CC_Password, CC_OwnerCategory, CC_SystemCategory, CC_RequireStatusResponse, CC_NotificationForInboxRecipient, CC_IsLegacyXmlAllowed, CC_PermitInboxSender, CC_PermitInboxRecipient)
	SELECT @CC_CTCGB_Test_PK, 'GBCustomsTest-CTCGB', 'Great Britain Customs Test - CTC GB', '00000000-0000-0000-0000-000000000000', '75419F4C-C522-4890-BD5D-BCA5E12268F6', '', '', 'Service Provider', 'Third Party', 0, 0, 0, 1, 1
End

--GBCustoms-GVMS CLIENT
If (@CC_GVMS_PK Is Null)
Begin
	SELECT @CC_GVMS_PK = 'E3D6010D-CF67-4AA2-BC7B-B3D5B7E2ECD6' --NEWID()
	INSERT INTO eHubClient (CC_PK, CC_ID, CC_FriendlyName, CC_Odyssey_OH, CC_DistributionZone, CC_EmailAddress, CC_Password, CC_OwnerCategory, CC_SystemCategory, CC_RequireStatusResponse, CC_NotificationForInboxRecipient, CC_IsLegacyXmlAllowed, CC_PermitInboxSender, CC_PermitInboxRecipient)
	SELECT @CC_GVMS_PK, 'GBCustoms-GVMS', 'Great Britain Customs - GVMS', '00000000-0000-0000-0000-000000000000', '75419F4C-C522-4890-BD5D-BCA5E12268F6', '', '', 'Service Provider', 'Third Party', 0, 0, 0, 1, 1
End

--GBCustomsTest-GVMS CLIENT
If (@CC_GVMS_Test_PK Is Null)
Begin
	SELECT @CC_GVMS_Test_PK = 'C54A7723-61CD-43A3-98FF-8EF5E5F29EA8' --NEWID()
	INSERT INTO eHubClient (CC_PK, CC_ID, CC_FriendlyName, CC_Odyssey_OH, CC_DistributionZone, CC_EmailAddress, CC_Password, CC_OwnerCategory, CC_SystemCategory, CC_RequireStatusResponse, CC_NotificationForInboxRecipient, CC_IsLegacyXmlAllowed, CC_PermitInboxSender, CC_PermitInboxRecipient)
	SELECT @CC_GVMS_Test_PK, 'GBCustomsTest-GVMS', 'Great Britain Customs Test - GVMS', '00000000-0000-0000-0000-000000000000', '75419F4C-C522-4890-BD5D-BCA5E12268F6', '', '', 'Service Provider', 'Third Party', 0, 0, 0, 1, 1
End

--GBCustoms-EMCS CLIENT
If (@CC_EMCS_PK Is Null)
Begin
	SELECT @CC_EMCS_PK = '4C96D6D4-89BC-4D5B-AE9B-A2F88B18ECE5' --NEWID()
	INSERT INTO eHubClient (CC_PK, CC_ID, CC_FriendlyName, CC_Odyssey_OH, CC_DistributionZone, CC_EmailAddress, CC_Password, CC_OwnerCategory, CC_SystemCategory, CC_RequireStatusResponse, CC_NotificationForInboxRecipient, CC_IsLegacyXmlAllowed, CC_PermitInboxSender, CC_PermitInboxRecipient)
	SELECT @CC_EMCS_PK, 'GBCustoms-EMCS', 'Great Britain Customs - EMCS', '00000000-0000-0000-0000-000000000000', '75419F4C-C522-4890-BD5D-BCA5E12268F6', '', '', 'Service Provider', 'Third Party', 0, 0, 0, 1, 1
End

--GBCustomsTest-EMCS CLIENT
If (@CC_EMCS_Test_PK Is Null)
Begin
	SELECT @CC_EMCS_Test_PK = 'A8161B53-6B56-479F-910D-12217DF36A93' --NEWID()
	INSERT INTO eHubClient (CC_PK, CC_ID, CC_FriendlyName, CC_Odyssey_OH, CC_DistributionZone, CC_EmailAddress, CC_Password, CC_OwnerCategory, CC_SystemCategory, CC_RequireStatusResponse, CC_NotificationForInboxRecipient, CC_IsLegacyXmlAllowed, CC_PermitInboxSender, CC_PermitInboxRecipient)
	SELECT @CC_EMCS_Test_PK, 'GBCustomsTest-EMCS', 'Great Britain Customs Test - EMCS', '00000000-0000-0000-0000-000000000000', '75419F4C-C522-4890-BD5D-BCA5E12268F6', '', '', 'Service Provider', 'Third Party', 0, 0, 0, 1, 1
End

--CLIENT REGISTRATION
INSERT INTO eHubClientRegistration (CX_PK, CX_CC, CX_RT, CX_Qualifier, CX_Attr1, CX_Code, CX_Flag1, CX_Flag2)
SELECT @CX_PK1, @CC_ICSGB_PK, @RT_PK, 'Create', 'CorrelationId', 'XMLBody://*[local-name() = ''SuccessResponse'']/*[local-name() = ''ResponseData'']/*[local-name() = ''CorrelationId'']/text()', 0, 0
INSERT INTO eHubClientRegistration (CX_PK, CX_CC, CX_RT, CX_Qualifier, CX_Attr1, CX_Code, CX_Flag1, CX_Flag2)
SELECT @CX_Test_PK1, @CC_ICSGB_Test_PK, @RT_PK, 'Create', 'CorrelationId', 'XMLBody://*[local-name() = ''SuccessResponse'']/*[local-name() = ''ResponseData'']/*[local-name() = ''CorrelationId'']/text()', 0, 0

INSERT INTO eHubClientRegistration (CX_PK, CX_CC, CX_RT, CX_Qualifier, CX_Attr1, CX_Code, CX_Flag1, CX_Flag2)
SELECT @CX_PK2, @CC_ICSGB_PK, @RT_PK, 'Amend', 'CorrelationId', 'XMLBody://*[local-name() = ''SuccessResponse'']/*[local-name() = ''ResponseData'']/*[local-name() = ''CorrelationId'']/text()', 0, 0
INSERT INTO eHubClientRegistration (CX_PK, CX_CC, CX_RT, CX_Qualifier, CX_Attr1, CX_Code, CX_Flag1, CX_Flag2)
SELECT @CX_Test_PK2, @CC_ICSGB_Test_PK, @RT_PK, 'Amend', 'CorrelationId', 'XMLBody://*[local-name() = ''SuccessResponse'']/*[local-name() = ''ResponseData'']/*[local-name() = ''CorrelationId'']/text()', 0, 0

INSERT INTO eHubClientRegistration (CX_PK, CX_CC, CX_RT, CX_Qualifier, CX_Attr1, CX_Code, CX_Flag1, CX_Flag2)
SELECT @CX_PK3, @CC_CTCGB_PK, @RT_PK, 'Arrive', 'ArrivalId', 'Parameter:/customs/transits/movements/arrivals/(?<CorrelationId>\d+)/messages/1', 0, 0
INSERT INTO eHubClientRegistration (CX_PK, CX_CC, CX_RT, CX_Qualifier, CX_Attr1, CX_Code, CX_Flag1, CX_Flag2)
SELECT @CX_Test_PK3, @CC_CTCGB_Test_PK, @RT_PK, 'Arrive', 'ArrivalId', 'Parameter:/customs/transits/movements/arrivals/(?<CorrelationId>\d+)/messages/1', 0, 0

INSERT INTO eHubClientRegistration (CX_PK, CX_CC, CX_RT, CX_Qualifier, CX_Attr1, CX_Code, CX_Flag1, CX_Flag2)
SELECT @CX_PK4, @CC_CTCGB_PK, @RT_PK, 'Depart', 'DepartureId', 'Parameter:/customs/transits/movements/departures/(?<CorrelationId>\d+)/messages/1', 0, 0
INSERT INTO eHubClientRegistration (CX_PK, CX_CC, CX_RT, CX_Qualifier, CX_Attr1, CX_Code, CX_Flag1, CX_Flag2)
SELECT @CX_Test_PK4, @CC_CTCGB_Test_PK, @RT_PK, 'Depart', 'DepartureId', 'Parameter:/customs/transits/movements/departures/(?<CorrelationId>\d+)/messages/1', 0, 0

INSERT INTO eHubClientRegistration (CX_PK, CX_CC, CX_RT, CX_Qualifier, CX_Attr1, CX_Code, CX_Flag1, CX_Flag2)
SELECT @CX_PK5, @CC_GVMS_PK, @RT_PK, 'Create-1', 'NotificationMessageId', 'Header:Notification-Message-Id', 0, 0
INSERT INTO eHubClientRegistration (CX_PK, CX_CC, CX_RT, CX_Qualifier, CX_Attr1, CX_Code, CX_Flag1, CX_Flag2)
SELECT @CX_Test_PK5, @CC_GVMS_Test_PK, @RT_PK, 'Create-1', 'NotificationMessageId', 'Header:Notification-Message-Id', 0, 0

INSERT INTO eHubClientRegistration (CX_PK, CX_CC, CX_RT, CX_Qualifier, CX_Attr1, CX_Code, CX_Flag1, CX_Flag2)
SELECT @CX_PK6, @CC_GVMS_PK, @RT_PK, 'Create-2', 'NotificationBoxId', 'Header:Notification-Box-Id', 0, 1
INSERT INTO eHubClientRegistration (CX_PK, CX_CC, CX_RT, CX_Qualifier, CX_Attr1, CX_Code, CX_Flag1, CX_Flag2)
SELECT @CX_Test_PK6, @CC_GVMS_Test_PK, @RT_PK, 'Create-2', 'NotificationBoxId', 'Header:Notification-Box-Id', 0, 1

INSERT INTO eHubClientRegistration (CX_PK, CX_CC, CX_RT, CX_Qualifier, CX_Attr1, CX_Code, CX_Flag1, CX_Flag2)
SELECT @CX_PK7, @CC_CTCGB_PK, @RT_PK, 'Notification-Headers', 'NotificationBoxId', 'Header:Notification-Box-Id', 1, 0
INSERT INTO eHubClientRegistration (CX_PK, CX_CC, CX_RT, CX_Qualifier, CX_Attr1, CX_Code, CX_Flag1, CX_Flag2)
SELECT @CX_Test_PK7, @CC_CTCGB_Test_PK, @RT_PK, 'Notification-Headers', 'NotificationBoxId', 'Header:Notification-Box-Id', 1, 0

INSERT INTO eHubClientRegistration (CX_PK, CX_CC, CX_RT, CX_Qualifier, CX_Attr1, CX_Code, CX_Flag1, CX_Flag2)
SELECT @CX_PK8, @CC_CTCGB_PK, @RT_PK, 'Notification-Body', 'NotificationBoxId', 'JSONBody:boxId', 1, 0
INSERT INTO eHubClientRegistration (CX_PK, CX_CC, CX_RT, CX_Qualifier, CX_Attr1, CX_Code, CX_Flag1, CX_Flag2)
SELECT @CX_Test_PK8, @CC_CTCGB_Test_PK, @RT_PK, 'Notification-Body', 'NotificationBoxId', 'JSONBody:boxId', 1, 0

INSERT INTO eHubClientRegistration (CX_PK, CX_CC, CX_RT, CX_Qualifier, CX_Attr1, CX_Code, CX_Flag1, CX_Flag2)
SELECT @CX_PK9, @CC_GVMS_PK, @RT_PK, 'Notification-Headers-1', 'NotificationMessageId', 'Header:Notification-Message-Id', 1, 1
INSERT INTO eHubClientRegistration (CX_PK, CX_CC, CX_RT, CX_Qualifier, CX_Attr1, CX_Code, CX_Flag1, CX_Flag2)
SELECT @CX_Test_PK9, @CC_GVMS_Test_PK, @RT_PK, 'Notification-Headers-1', 'NotificationMessageId', 'Header:Notification-Message-Id', 1, 1

INSERT INTO eHubClientRegistration (CX_PK, CX_CC, CX_RT, CX_Qualifier, CX_Attr1, CX_Code, CX_Flag1, CX_Flag2)
SELECT @CX_PK10, @CC_GVMS_PK, @RT_PK, 'Notification-Headers-2', 'NotificationBoxId', 'Header:Notification-Box-Id', 1, 1
INSERT INTO eHubClientRegistration (CX_PK, CX_CC, CX_RT, CX_Qualifier, CX_Attr1, CX_Code, CX_Flag1, CX_Flag2)
SELECT @CX_Test_PK10, @CC_GVMS_Test_PK, @RT_PK, 'Notification-Headers-2', 'NotificationBoxId', 'Header:Notification-Box-Id', 1, 1

INSERT INTO eHubClientRegistration (CX_PK, CX_CC, CX_RT, CX_Qualifier, CX_Attr1, CX_Code, CX_Flag1, CX_Flag2)
SELECT @CX_PK11, @CC_GVMS_PK, @RT_PK, 'Notification-Body-1', 'NotificationBoxId', 'JSONBody:boxId', 1, 1
INSERT INTO eHubClientRegistration (CX_PK, CX_CC, CX_RT, CX_Qualifier, CX_Attr1, CX_Code, CX_Flag1, CX_Flag2)
SELECT @CX_Test_PK11, @CC_GVMS_Test_PK, @RT_PK, 'Notification-Body-1', 'NotificationBoxId', 'JSONBody:boxId', 1, 1

INSERT INTO eHubClientRegistration (CX_PK, CX_CC, CX_RT, CX_Qualifier, CX_Attr1, CX_Code, CX_Flag1, CX_Flag2)
SELECT @CX_PK12, @CC_GVMS_PK, @RT_PK, 'Notification-Body-2', 'ContentType', 'JSONBody: messageContentType', 1, 1
INSERT INTO eHubClientRegistration (CX_PK, CX_CC, CX_RT, CX_Qualifier, CX_Attr1, CX_Code, CX_Flag1, CX_Flag2)
SELECT @CX_Test_PK12, @CC_GVMS_Test_PK, @RT_PK, 'Notification-Body-2', 'ContentType', 'JSONBody: messageContentType', 1, 1

INSERT INTO eHubClientRegistration (CX_PK, CX_CC, CX_RT, CX_Qualifier, CX_Attr1, CX_Code, CX_Flag1, CX_Flag2)
SELECT @CX_PK13, @CC_GVMS_PK, @RT_PK, 'Notification-Json', 'MessageId', 'JSONBody:messageId', 1, 0
INSERT INTO eHubClientRegistration (CX_PK, CX_CC, CX_RT, CX_Qualifier, CX_Attr1, CX_Code, CX_Flag1, CX_Flag2)
SELECT @CX_Test_PK13, @CC_GVMS_Test_PK, @RT_PK, 'Notification-Json', 'MessageId', 'JSONBody:messageId', 1, 0

INSERT INTO eHubClientRegistration (CX_PK, CX_CC, CX_RT, CX_Qualifier, CX_Attr1, CX_Code, CX_Flag1, CX_Flag2)
SELECT @CX_PK14, @CC_GVMS_PK, @RT_PK, 'Notification-Xml', 'MessageId', 'XMLBody://*[local-name() = ''messageId'']/text()', 1, 0
INSERT INTO eHubClientRegistration (CX_PK, CX_CC, CX_RT, CX_Qualifier, CX_Attr1, CX_Code, CX_Flag1, CX_Flag2)
SELECT @CX_Test_PK14, @CC_GVMS_Test_PK, @RT_PK, 'Notification-Xml', 'MessageId', 'XMLBody://*[local-name() = ''messageId'']/text()', 1, 0



ROLLBACK TRAN
--COMMIT TRAN

USE eHubTransactions
BEGIN TRANSACTION

	DECLARE @CCDocumentProdPK	uniqueidentifier = 'DF5BF7F0-EA7C-4A09-85A1-B8FF8A998B0B', --SELECT NEWID()
			@CCDocumentTestPK	uniqueidentifier = 'F1992A01-FFAD-446A-81D0-57E0DEAC8116', --SELECT NEWID()
			@ZZDistributionZone uniqueidentifier = '75419F4C-C522-4890-BD5D-BCA5E12268F6', --SELECT ZZ_PK FROM eHubZone WHERE ZZ_ID = 'AU'
			@TTTransTypePK		uniqueidentifier = '120D1BBA-ABC7-4657-B6A4-FB249E5D2156', --SELECT NEWID()
			@DTUniversalEventPK	uniqueidentifier = '6C73FDC6-0A91-46F2-9EB1-3E98816E87F0', -- SELECT DT_PK FROM eHubMessageType WHERE DT_Code = 'http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange'
			@TSTransSetPK		uniqueidentifier = 'F53A2FB6-F07E-4AEA-AFE3-F2269EFB0F2E', --SELECT NEWID()
			@TSTransSet2PK		uniqueidentifier = '9E5557AC-F1CF-4DDE-A8F4-59607FB89CBA', --SELECT NEWID()
			@CCGBCustmoProdPK	uniqueidentifier = 'A09A4C93-17A8-4F86-BA35-FE55C824CB7B', --SELECT CC_PK FROM eHubClient WHERE CC_ID = 'GBCustoms'
			@CCGBCustmoTestPK	uniqueidentifier = '6B41ECE4-ABA6-479B-A74F-92B19717F474' --SELECT CC_PK FROM eHubClient WHERE CC_ID = 'GBCustomsTest'

		
	INSERT INTO eHubClient (CC_PK, CC_ID, CC_FriendlyName, CC_Odyssey_OH, CC_DistributionZone, CC_EmailAddress, CC_Password, CC_IsAirServiceProvider, CC_AirlineCode, 
			CC_AirServiceProvider, CC_AirlinePrefix, CC_USCustomsRecipient, CC_AS2_Code, CC_SCAC_Code, CC_OwnerCategory, CC_SystemCategory, CC_RR, CC_RequireStatusResponse, CC_NotificationForInboxRecipient)
	VALUES (@CCDocumentProdPK, 'GBCustoms-DirectDocument', 'Great Britain Customs Supporting Documents', '00000000-0000-0000-0000-000000000000', @ZZDistributionZone, '', '', null, null, 
			null, null, null, null, null, 'Service Provider', 'Third Party', null, 0, 0)
	INSERT INTO eHubClient (CC_PK, CC_ID, CC_FriendlyName, CC_Odyssey_OH, CC_DistributionZone, CC_EmailAddress, CC_Password, CC_IsAirServiceProvider, CC_AirlineCode, 
			CC_AirServiceProvider, CC_AirlinePrefix, CC_USCustomsRecipient, CC_AS2_Code, CC_SCAC_Code, CC_OwnerCategory, CC_SystemCategory, CC_RR, CC_RequireStatusResponse, CC_NotificationForInboxRecipient)
	VALUES (@CCDocumentTestPK, 'GBCustomsTest-DirectDocument', 'Great Britain Customs Supporting Documents Test', '00000000-0000-0000-0000-000000000000', @ZZDistributionZone, '', '', null, null, 
			null, null, null, null, null, 'Service Provider', 'Third Party', null, 0, 0)

	INSERT INTO eHubTransformationType (TT_PK, TT_DT_Source, TT_DT_Target, TT_TransformationType, TT_Target_Version)
		VALUES (@TTTransTypePK, @DTUniversalEventPK, @DTUniversalEventPK, 'CargoWise.eHub.Products.GBCustoms.CDS.BT.Orchestrations.Transformations.UniversalEvent2UniversalEvent, CargoWise.eHub.Products.GBCustoms.CDS.BT.Orchestrations.DocumentUpload, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350', null)

	INSERT INTO eHubTransformationSet (TS_PK, TS_Name, TS_CC_Sender, TS_CC_Recipient, TS_DT_Source, TS_XPathPredicate, TS_BillingInterfaceName, TS_BillingElement, TS_BillingXPathSource, TS_BillingXPathTarget, TS_BillSender, TS_BillRecipient, TS_CC_BillOther, TS_BillingNumMessagesIncluded, TS_BillingFee)
		VALUES (@TSTransSetPK, 'GB Customs Document Upload', null, @CCGBCustmoProdPK, @DTUniversalEventPK, null, null, null, null, null, 0, 0, null, null, null)

	INSERT INTO eHubTransformationSet (TS_PK, TS_Name, TS_CC_Sender, TS_CC_Recipient, TS_DT_Source, TS_XPathPredicate, TS_BillingInterfaceName, TS_BillingElement, TS_BillingXPathSource, TS_BillingXPathTarget, TS_BillSender, TS_BillRecipient, TS_CC_BillOther, TS_BillingNumMessagesIncluded, TS_BillingFee)
		VALUES (@TSTransSet2PK, 'GB Customs Document Upload Test', null, @CCGBCustmoTestPK, @DTUniversalEventPK, null, null, null, null, null, 0, 0, null, null, null)

	INSERT INTO eHubTransformationMapping (TM_TS_PK, TM_Order, TM_TT_PK)
		VALUES (@TSTransSetPK, 0, @TTTransTypePK)

	INSERT INTO eHubTransformationMapping (TM_TS_PK, TM_Order, TM_TT_PK)
		VALUES (@TSTransSet2PK, 0, @TTTransTypePK)

ROLLBACK TRANSACTION
--COMMIT TRANSACTION

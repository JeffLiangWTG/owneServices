begin transaction

DECLARE @RX_SCACUniShipC1CPK			uniqueIdentifier = 'DF86E6E8-B5AA-44BC-AF56-916004856507'	-- SELECT NEWID()
DECLARE @RX_SCACCoLoadUniShipC1CPK			uniqueIdentifier = '563F506A-984F-46C7-8BDB-1596848BA124'	-- SELECT NEWID()

INSERT INTO [dbo].[eHubRoutingRuleFact]
           ([RX_PK]
           ,[RX_RR]
           ,[RX_Name]
           ,[RX_Type]
           ,[RX_Query]
           ,[RX_RR_ComputeRule])
     VALUES
           (@RX_SCACUniShipC1CPK
           ,'D74CF7D3-6F85-4F06-AFC7-EDBEA8BBB7FC'
           ,'SCACUniShipC1C'
           ,'XPATHNAVFUNC'
           ,'translate(/*[local-name()=''UniversalInterchange'']/*[local-name()=''Body'']/*[local-name()=''UniversalShipment'']/*[local-name()=''Shipment'']/*[local-name()=''OrganizationAddressCollection'']/*[local-name()=''OrganizationAddress''][*[local-name()=''AddressType'']/text()=''ShippingLineAddress'']/*[local-name()=''RegistrationNumberCollection'']/*[local-name()=''RegistrationNumber''][*[local-name()=''Type'']/text()=''C1C'']/*[local-name()=''Value''], ''abcdefghijklmnopqrstuvwxyz'', ''ABCDEFGHIJKLMNOPQRSTUVWXYZ'')'
           ,NULL)

INSERT INTO [dbo].[eHubRoutingRuleFact]
           ([RX_PK]
           ,[RX_RR]
           ,[RX_Name]
           ,[RX_Type]
           ,[RX_Query]
           ,[RX_RR_ComputeRule])
     VALUES
           (@RX_SCACCoLoadUniShipC1CPK
           ,'D74CF7D3-6F85-4F06-AFC7-EDBEA8BBB7FC'
           ,'SCACCoLoadUniShipC1C'
           ,'XPATHNAVFUNC'
           ,'translate(/*[local-name()=''UniversalInterchange'']/*[local-name()=''Body'']/*[local-name()=''UniversalShipment'']/*[local-name()=''Shipment'']/*[local-name()=''OrganizationAddressCollection'']/*[local-name()=''OrganizationAddress''][*[local-name()=''AddressType'']/text()=''CoLoadWith'']/*[local-name()=''RegistrationNumberCollection'']/*[local-name()=''RegistrationNumber''][*[local-name()=''Type'']/text()=''C1C'']/*[local-name()=''Value''], ''abcdefghijklmnopqrstuvwxyz'', ''ABCDEFGHIJKLMNOPQRSTUVWXYZ'')'
           ,NULL)

DECLARE @RR_CoLoadPK			uniqueIdentifier = '960C9391-78F1-4BBE-BD27-A314185D1157'	-- SELECT NEWID()
DECLARE @RR_PK			uniqueIdentifier = '23AD3F6D-F761-43FE-A5FA-3E10EF03C3FA'	-- SELECT NEWID()

INSERT INTO [dbo].[eHubRoutingRule]
           ([RR_PK],[RR_Condition_Expression],[RR_Group_RR_GroupRule],[RR_Result_Value],[RR_Group_Ordering])
     VALUES
           (@RR_CoLoadPK,'[@NVOCC,Equal,Y] && [@SCACCoLoadUniShipC1C,NotEqual,]','B7C13CF6-0F09-447F-9A13-72D728A81F2E','@SCACCoLoadUniShipC1C',2000)

INSERT INTO [dbo].[eHubRoutingRule]
           ([RR_PK],[RR_Condition_Expression],[RR_Group_RR_GroupRule],[RR_Result_Value],[RR_Group_Ordering])
     VALUES
           (@RR_PK,'[@SCACUniShipC1C,NotEqual,]','B7C13CF6-0F09-447F-9A13-72D728A81F2E','@SCACUniShipC1C',4000)

UPDATE [dbo].[eHubRoutingRule]
SET RR_Group_Ordering = 5000
WHERE RR_PK = 'A0B72A44-4479-438F-BA02-4CD6349E68BA'

UPDATE [dbo].[eHubRoutingRule]
SET RR_Group_Ordering = 3000
WHERE RR_PK = 'ECEEA9E2-9B28-4966-8F29-2F1431913531'

rollback
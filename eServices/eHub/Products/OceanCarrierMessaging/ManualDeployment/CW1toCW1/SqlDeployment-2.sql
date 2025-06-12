-- Run CW1toCW1Clients.sql first
begin transaction

DECLARE @RR_PKBK UNIQUEIDENTIFIER = '6D1CBA67-BB80-4E19-B0A2-8E87907252B4'
DECLARE @RR_PKSI UNIQUEIDENTIFIER = '23C62F83-1AFD-4DED-97DA-4F2B04BF9269'
DECLARE @RR_PKVM UNIQUEIDENTIFIER = '27BA0BFA-FC5B-4EE9-9B3C-697A25A9F730'

DECLARE @CARGOWISE_BK_CCPK UNIQUEIDENTIFIER = (SELECT CC_PK from eHubClient where CC_ID = 'CARGOWISE_BK')
DECLARE @CARGOWISE_SI_CCPK UNIQUEIDENTIFIER = (SELECT CC_PK from eHubClient where CC_ID = 'CARGOWISE_SI')
DECLARE @CARGOWISE_VM_CCPK UNIQUEIDENTIFIER = (SELECT CC_PK from eHubClient where CC_ID = 'CARGOWISE_VM')
DECLARE @RR_Group_RR_GroupRule UNIQUEIDENTIFIER = 'DC245985-DA94-4C8A-8AE8-DCC5E8459D35'


INSERT INTO [dbo].[eHubRoutingRule]
           ([RR_PK]
           ,[RR_Condition_Expression]
           ,[RR_Group_RR_GroupRule]
           ,[RR_Success_CC_Recipient]
           ,[RR_Group_Ordering])
     VALUES
           (@RR_PKVM
           ,'[@UShipmentNamespace,Equal,http://www.cargowise.com/Schemas/Universal/2012/11/ContainerGrossWeightVerification/2]'
           , @RR_Group_RR_GroupRule
		   , @CARGOWISE_VM_CCPK
           ,1000)


INSERT INTO [dbo].[eHubRoutingRule]
           ([RR_PK]
           ,[RR_Condition_Expression]
           ,[RR_Group_RR_GroupRule]
           ,[RR_Success_CC_Recipient]
           ,[RR_Group_Ordering])
     VALUES
           (@RR_PKBK
           ,'[@UShipmentNamespace,Equal,http://www.cargowise.com/Schemas/Universal/2012/11/BookingRequest/1]'
           , @RR_Group_RR_GroupRule
		   , @CARGOWISE_BK_CCPK
           ,2000)

		   INSERT INTO [dbo].[eHubRoutingRule]
           ([RR_PK]
           ,[RR_Condition_Expression]
           ,[RR_Group_RR_GroupRule]
           ,[RR_Success_CC_Recipient]
           ,[RR_Group_Ordering])
     VALUES
           (@RR_PKSI
           ,'[@UShipmentNamespace,Equal,http://www.cargowise.com/Schemas/Universal/2012/11/ShippingInstruction/1]'
           , @RR_Group_RR_GroupRule
		   , @CARGOWISE_SI_CCPK
           ,3000)

Rollback
--Commit

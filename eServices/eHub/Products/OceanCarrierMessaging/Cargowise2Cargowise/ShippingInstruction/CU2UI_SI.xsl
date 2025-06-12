<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl ns0 CodeMapper ContextAccessor DataModelAccessor OCMHelper ListHelper StringHelper userCSharp SubscriptionHelper" version="1.0"
                xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
                xmlns:DataModelAccessor="http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"
                xmlns:OCMHelper="http://schemas.microsoft.com/BizTalk/2003/OCMHelper"
                xmlns:ListHelper="http://schemas.microsoft.com/BizTalk/2003/ListHelper"
                xmlns:StringHelper="http://schemas.microsoft.com/BizTalk/2003/StringHelper"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp"
                xmlns:SubscriptionHelper="http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />

  <xsl:variable name="SenderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="RecipientID" select="ContextAccessor:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>

  <xsl:variable name="shipment" select="/ns0:UniversalShipment/ns0:Shipment"/>
  <xsl:variable name="consolID" select="$shipment/ns0:DataContext/ns0:DataSourceCollection/ns0:DataSource[1]/ns0:Key/text()"/>
  <xsl:variable name="forwardingType" select="$shipment/ns0:DataContext/ns0:DataSourceCollection/ns0:DataSource[1]/ns0:Type/text()"/>
  <xsl:variable name="shipmentType" select="$shipment/ns0:ShipmentType/ns0:Code/text()"/>
  <xsl:variable name="portOfLoadingCountryCode" select="substring(/ns0:UniversalShipment/ns0:Shipment/ns0:PortOfLoading/ns0:Code/text(), 1, 2)"/>
  <xsl:variable name="portOfDischargeCountryCode" select="substring(/ns0:UniversalShipment/ns0:Shipment/ns0:PortOfDischarge/ns0:Code/text(), 1, 2)"/>
  <xsl:variable name="portOfDestinationCountryCode" select="substring(/ns0:UniversalShipment/ns0:Shipment/ns0:PortOfDestination/ns0:Code/text(), 1, 2)"/>

  <xsl:variable name="isCoLoad" select="OCMHelper:IsCoLoad($shipmentType) = 'TRUE'" />

  <xsl:variable name="addressType">
    <xsl:choose>
      <xsl:when test="$isCoLoad">CoLoadWith</xsl:when>
      <xsl:otherwise>ShippingLineAddress</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="eHubClientLookupCode">
    <xsl:call-template name="GetOrgAddressRegNumber">
      <xsl:with-param name="addressType" select="$addressType" />
    </xsl:call-template>
  </xsl:variable>

  <xsl:variable name="eHubCargowiseClientID" select="DataModelAccessor:GeteHubIDByQualifier($eHubClientLookupCode, 'CARGOWISE')"/>
  <xsl:variable name="eHubPartyTypeFlag">
    <xsl:choose>
      <xsl:when test="$eHubCargowiseClientID != ''">
        <xsl:value-of select="DataModelAccessor:GetClientRegistrationFlag1AsString($eHubCargowiseClientID, $eHubClientLookupCode, 'CARGOWISE')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="userCSharp:ThrowPartyReceiverIDNotFound($eHubClientLookupCode)"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="eHubPartyType">
    <xsl:choose>
      <xsl:when test="$eHubPartyTypeFlag='1'">NVOCC</xsl:when>
      <xsl:otherwise>ShippingLine</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="recipientRole">
    <xsl:choose>
      <xsl:when test="$eHubPartyType='NVOCC'">NVO</xsl:when>
      <xsl:otherwise>CAR</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="SetRecipientID" select="ContextAccessor:SetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties', $eHubCargowiseClientID)"/>

  <xsl:variable name="InterchangeNum" select="CodeMapper:CallActionProcedureHelper('GetCounterValue','@value','@name','CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CARGOWISE2CARGOWISE','@maxlength','14')" />
  <xsl:variable name="previousConsolReference" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $eHubCargowiseClientID, '@recipientId', $SenderID , '@ST_ID', 'CW1MSG', '@value', $consolID, '@referenceType', 'JobNumber')" />
  <xsl:variable name="InboxPK" select="ContextAccessor:GetContextProperty('InternalTrackingID', 'http://cargowise.com/ehub/tracking/2010/06')"/>
  <xsl:variable name="SubscribeInboxPK" select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $InboxPK, $InterchangeNum)" />
  <xsl:variable name="SubscriberConsolReference">
    <xsl:choose>
      <xsl:when test="$previousConsolReference!=''">
        <xsl:value-of select="$previousConsolReference" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:variable name="formattedCounter" select='format-number($InterchangeNum, "0000000000")' />
        <xsl:variable name="NewConsolReference">
          <xsl:choose>
            <xsl:when test="$forwardingType = 'ForwardingShipment'">
              <xsl:value-of select="concat('SHP', $formattedCounter)"/>
            </xsl:when>
            <xsl:when test="$forwardingType = 'ForwardingConsol'">
              <xsl:value-of select="concat('CON', $formattedCounter)"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="concat('WTG', $formattedCounter)"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:variable name="SubScriberValue1" select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $NewConsolReference, $consolID, 'JobNumber')" />
        <xsl:variable name="SubScriberValue2" select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $consolID, $NewConsolReference, 'JobNumber')" />
        <xsl:value-of select="$NewConsolReference" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="SubscribeInterchangeNum" select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $InterchangeNum, $SubscriberConsolReference, 'InterchangeNumber')" />
  <xsl:variable name="purposeCode" select="$shipment/ns0:DataContext/ns0:DocumentaryOverride/ns0:Purpose/ns0:Code/text()"/>
  <xsl:variable name="SubscribePurpose" select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $SubscriberConsolReference, $purposeCode, 'ActionPurpose')" />
  <xsl:variable name="subscribeShipmentType" select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $SubscriberConsolReference, $shipmentType, 'ShipmentType')" />
  <xsl:variable name="SubscribeDocumentName" select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $SubscriberConsolReference, 'Shipping Instruction', 'DocumentName')" />
  <xsl:variable name="SubscribeForwardingType" select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $SubscriberConsolReference, $forwardingType, 'ForwardingType')" />

  <xsl:variable name="previousPartyType" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $eHubCargowiseClientID, '@recipientId', $SenderID , '@ST_ID', 'CW1MSG', '@value', $SubscriberConsolReference, '@referenceType', 'PartyType')" />
  <xsl:variable name="SubscribeNewPartyType">
    <xsl:choose>
      <xsl:when test="$previousPartyType=''">
        <xsl:value-of select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $SubscriberConsolReference, $eHubPartyType, 'PartyType')" />
      </xsl:when>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="formVersion" select="$shipment/ns0:AddInfoCollection/ns0:AddInfo[ns0:Key/text()='FormVersion']/ns0:Value/text()" />
  <xsl:variable name="SubscribeFormVersion">
    <xsl:if test="$formVersion !=''">
      <xsl:value-of select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $SubscriberConsolReference, $formVersion, 'FormVersion')"/>
    </xsl:if>
  </xsl:variable>

  <xsl:variable name="InsertBoleroSubscription" select="SubscriptionHelper:InsertBoleroSubscription($SenderID, $eHubCargowiseClientID, 'CARGOWISE', $SubscriberConsolReference, $consolID)" />

  <xsl:template match="/ns0:UniversalShipment">
    <ns0:UniversalInterchange>
      <ns0:Header>
        <ns0:SenderID>
          <xsl:value-of select="$eHubCargowiseClientID"/>
        </ns0:SenderID>
        <ns0:RecipientID>
          <xsl:value-of select="'SHIPPING_INSTRUCTION'"/>
        </ns0:RecipientID>
        <ns0:Acknowledgement>
          <ns0:Required>OnAll</ns0:Required>
          <ns0:Channel>eHub</ns0:Channel>
          <ns0:RecipientID>CARGOWISE_AC</ns0:RecipientID>
          <ns0:ContextCollection>
            <ns0:Context>
              <ns0:Type>eHub Interchange Reference</ns0:Type>
              <ns0:Value>
                <xsl:value-of select="$SubscriberConsolReference"/>
              </ns0:Value>
            </ns0:Context>
            <ns0:Context>
              <ns0:Type>MessageReference</ns0:Type>
              <ns0:Value>
                <xsl:value-of select="$consolID"/>
              </ns0:Value>
            </ns0:Context>
            <ns0:Context>
              <ns0:Type>DocumentName</ns0:Type>
              <ns0:Value>Shipping Instruction</ns0:Value>
            </ns0:Context>
          </ns0:ContextCollection>
        </ns0:Acknowledgement>
      </ns0:Header>
      <ns0:Body>
        <ns0:UniversalShipment>
          <xsl:apply-templates/>
        </ns0:UniversalShipment>
      </ns0:Body>
    </ns0:UniversalInterchange>
  </xsl:template>
  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="ns0:DataTargetCollection|ns0:DataSourceCollection|ns0:ActionPurpose|ns0:Company|ns0:DataProvider|ns0:EnterpriseID|ns0:EventBranch|ns0:ServerID|ns0:AddInfoCollection|ns0:Shipment/ns0:PackingLineCollection|ns0:PaymentHandlingInstructionCollection|ns0:ShipmentIncoTerm|ns0:WayBillType" />

  <xsl:template match="/ns0:UniversalShipment/ns0:Shipment/ns0:DataContext">
    <ns0:DataContext>
      <xsl:variable name="serviceCode" select="/ns0:UniversalShipment/ns0:Shipment/ns0:DataContext/ns0:RecipientRoleCollection/ns0:RecipientRole/ns0:ServiceCode/text()"/>
      <xsl:if test="$recipientRole != 'CAR' and $formVersion != '' and StringHelper:IsNewFormMessage($formVersion, '2.5.0')">
        <ns0:Action>LinkOnly</ns0:Action>
      </xsl:if>
      <xsl:apply-templates select="@*|*"/>
    </ns0:DataContext>
  </xsl:template>

  <xsl:template match="/ns0:UniversalShipment/ns0:Shipment/ns0:BookingConfirmationReference">
    <xsl:if test="$eHubPartyType='ShippingLine'">
      <ns0:BookingConfirmationReference>
        <xsl:call-template name="GetBookingConfirmationRef" />
      </ns0:BookingConfirmationReference>
    </xsl:if>
  </xsl:template>

  <xsl:template match="/ns0:UniversalShipment/ns0:Shipment/ns0:CoLoadBookingConfirmationReference">
    <xsl:if test="$eHubPartyType='NVOCC'">
      <ns0:CoLoadBookingConfirmationReference>
        <xsl:call-template name="GetBookingConfirmationRef" />
      </ns0:CoLoadBookingConfirmationReference>
    </xsl:if>
  </xsl:template>

  <xsl:template match="/ns0:UniversalShipment/ns0:Shipment/ns0:CoLoadMasterBillNumber">
    <xsl:if test="$eHubPartyType='NVOCC'">
      <ns0:CoLoadMasterBillNumber>
        <xsl:call-template name="GetWaybillNumber" />
      </ns0:CoLoadMasterBillNumber>
    </xsl:if>
  </xsl:template>

  <xsl:template match="/ns0:UniversalShipment/ns0:Shipment/ns0:WayBillNumber">
    <xsl:if test="$eHubPartyType='ShippingLine'">
      <ns0:WayBillNumber>
        <xsl:call-template name="GetWaybillNumber" />
      </ns0:WayBillNumber>

      <xsl:copy-of select ="/ns0:UniversalShipment/ns0:Shipment/ns0:WayBillType"/>
    </xsl:if>
  </xsl:template>

  <xsl:template name="GetBookingConfirmationRef">
    <xsl:variable name="bookingConfirmationRef">
      <xsl:choose>
        <xsl:when test="$isCoLoad">
          <xsl:value-of select="$shipment/ns0:CoLoadBookingConfirmationReference/text()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$shipment/ns0:BookingConfirmationReference/text()"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:value-of select="$bookingConfirmationRef"/>
  </xsl:template>

  <xsl:template name="GetWaybillNumber">
    <xsl:variable name="waybillNumber">
      <xsl:choose>
        <xsl:when test="$isCoLoad">
          <xsl:value-of select="$shipment/ns0:CoLoadMasterBillNumber/text()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$shipment/ns0:WayBillNumber/text()"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:value-of select="$waybillNumber"/>
  </xsl:template>

  <xsl:template match="/ns0:UniversalShipment/ns0:Shipment/ns0:PaymentMethod">
    <xsl:choose>
      <xsl:when test="$recipientRole = 'CAR'">
        <xsl:variable name="paymentMethod" select="/ns0:UniversalShipment/ns0:Shipment/ns0:PaymentMethod/ns0:Code/text()"/>
        <ns0:PaymentMethod>
          <ns0:Code>
            <xsl:choose>
              <xsl:when test="$paymentMethod = 'CCX'">CLT</xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$paymentMethod"/>
              </xsl:otherwise>
            </xsl:choose>
          </ns0:Code>
          <xsl:copy-of select ="/ns0:UniversalShipment/ns0:Shipment/ns0:PaymentMethod/ns0:Description"/>
        </ns0:PaymentMethod>
      </xsl:when>
      <xsl:otherwise>
        <xsl:copy-of select ="/ns0:UniversalShipment/ns0:Shipment/ns0:PaymentMethod"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="ns0:RecipientRoleCollection">
    <ns0:RecipientRoleCollection>
      <ns0:RecipientRole>
        <ns0:Code>
          <xsl:value-of select="$recipientRole"/>
        </ns0:Code>
        <ns0:Description>
          <xsl:value-of select="$eHubPartyType"/>
        </ns0:Description>
        <xsl:copy-of select ="ns0:RecipientRole/ns0:ServiceCode"/>
        <xsl:copy-of select ="ns0:RecipientRole/ns0:ServiceDescription"/>
      </ns0:RecipientRole>
    </ns0:RecipientRoleCollection>
  </xsl:template>

  <xsl:variable name="containerMode" select="/ns0:UniversalShipment/ns0:Shipment/ns0:ContainerMode/ns0:Code/text()" />
  <xsl:variable name="deliveryMode" select="/ns0:UniversalShipment/ns0:Shipment/ns0:ContainerCollection/ns0:Container[1]/ns0:DeliveryMode/text()"/>

  <xsl:template match="/ns0:UniversalShipment/ns0:Shipment/ns0:HBLContainerPackModeOverride">
    <xsl:variable name="hblContainerPackModeOverride">
      <xsl:choose>
        <xsl:when test="$eHubPartyType='NVOCC'">
          <xsl:choose>
            <xsl:when test="$deliveryMode='CFS/CFS'">DOOR/DOOR</xsl:when>
            <xsl:when test="$containerMode='FCL' and $deliveryMode='CFS/CY'">DOOR/CY</xsl:when>
            <xsl:when test="$containerMode='FCL' and $deliveryMode='CY/CFS'">CY/DOOR</xsl:when>
            <xsl:when test="$containerMode='FCL' and $deliveryMode='CY/CY'">CY/CY</xsl:when>
            <xsl:when test="$containerMode='LCL' and $deliveryMode='CFS/CY'">DOOR/CFS</xsl:when>
            <xsl:when test="$containerMode='LCL' and $deliveryMode='CY/CFS'">CFS/DOOR</xsl:when>
            <xsl:when test="$containerMode='LCL' and $deliveryMode='CY/CY'">CFS/CFS</xsl:when>
          </xsl:choose>
        </xsl:when>
        <xsl:otherwise>
          <xsl:copy-of select ="."/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <ns0:HBLContainerPackModeOverride>
      <xsl:value-of select="$hblContainerPackModeOverride"/>
    </ns0:HBLContainerPackModeOverride>
  </xsl:template>

  <xsl:template match="/ns0:UniversalShipment/ns0:Shipment/ns0:ShipmentType">
    <ns0:ShipmentIncoTerm>
      <xsl:choose>
        <xsl:when test="$eHubPartyType='NVOCC'">
          <xsl:variable name="freightCharge" select="$shipment/ns0:PaymentHandlingInstructionCollection/ns0:PaymentHandlingInstruction[ns0:Category/ns0:Code/text()='FRT']/ns0:PaymentMethod/ns0:Code/text()"/>
          <ns0:Code>
            <xsl:choose>
              <xsl:when test="$freightCharge='PPD'">CFR</xsl:when>
              <xsl:otherwise>FOB</xsl:otherwise>
            </xsl:choose>
          </ns0:Code>
          <ns0:Description>
            <xsl:choose>
              <xsl:when test="$freightCharge='PPD'">Cost And Freight</xsl:when>
              <xsl:otherwise>Free On Board</xsl:otherwise>
            </xsl:choose>
          </ns0:Description>
        </xsl:when>
        <xsl:otherwise>
          <xsl:copy-of select ="$shipment/ns0:ShipmentIncoTerm/ns0:Code"/>
          <xsl:copy-of select ="$shipment/ns0:ShipmentIncoTerm/ns0:Description"/>
        </xsl:otherwise>
      </xsl:choose>
    </ns0:ShipmentIncoTerm>

    <xsl:choose>
      <xsl:when test="$eHubPartyType='NVOCC'">
        <ns0:ShipmentType>
          <ns0:Code>STD</ns0:Code>
          <ns0:Description>Standard</ns0:Description>
        </ns0:ShipmentType>
      </xsl:when>
      <xsl:otherwise>
        <xsl:copy-of select ="."/>
      </xsl:otherwise>
    </xsl:choose>

    <xsl:if test="$recipientRole = 'CAR'">
      <xsl:variable name="agreed" select="/ns0:UniversalShipment/ns0:Shipment/ns0:BillOfLadingClauseCollection/ns0:BillOfLadingClause[ns0:Type/ns0:Code/text() = 'FAA']"/>
      <xsl:variable name="collect" select="/ns0:UniversalShipment/ns0:Shipment/ns0:BillOfLadingClauseCollection/ns0:BillOfLadingClause[ns0:Type/ns0:Code/text() = 'FCL']"/>
      <xsl:variable name="prepaid" select="/ns0:UniversalShipment/ns0:Shipment/ns0:BillOfLadingClauseCollection/ns0:BillOfLadingClause[ns0:Type/ns0:Code/text() = 'FPP']"/>
      <ns0:HBLAWBChargesDisplay>
        <ns0:Code>
          <xsl:choose>
            <xsl:when test="$agreed">AGR</xsl:when>
            <xsl:when test="$collect or $prepaid">ALL</xsl:when>
            <xsl:otherwise>NON</xsl:otherwise>
          </xsl:choose>
        </ns0:Code>
      </ns0:HBLAWBChargesDisplay>
    </xsl:if>
  </xsl:template>

  <xsl:template match="ns0:LocalProcessing">
    <xsl:if test="$eHubPartyType='NVOCC'">
      <xsl:variable name="departureEstimatedPickup" select="$shipment/ns0:ContainerCollection/ns0:Container/ns0:DepartureEstimatedPickup/text()" />
      <xsl:variable name="estimatedDelivery" select="$shipment/ns0:DateCollection/ns0:Date[ns0:Type/text()='LatestDelivery']/ns0:Value/text()" />

      <ns0:LocalProcessing>
        <ns0:DeliveryRequiredBy>
          <xsl:value-of select="$estimatedDelivery"/>
        </ns0:DeliveryRequiredBy>
        <ns0:PickupRequiredBy>
          <xsl:value-of select="$departureEstimatedPickup"/>
        </ns0:PickupRequiredBy>
      </ns0:LocalProcessing>
    </xsl:if>
  </xsl:template>

  <xsl:template match="ns0:PortOfOrigin">
    <xsl:variable name="placeOfReceipt" select="$shipment/ns0:PlaceOfReceipt/ns0:Code/text()" />

    <xsl:choose>
      <xsl:when test="$placeOfReceipt!=''">
        <ns0:PortOfOrigin>
          <ns0:Code>
            <xsl:value-of select="$placeOfReceipt"/>
          </ns0:Code>
          <ns0:Name>
            <xsl:value-of select="$shipment/ns0:PlaceOfReceipt/ns0:Name/text()"/>
          </ns0:Name>
        </ns0:PortOfOrigin>
      </xsl:when>
      <xsl:otherwise>
        <xsl:copy-of select="ns0:PortOfOrigin"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="ns0:PortOfDestination">
    <xsl:variable name="placeOfDelivery" select="$shipment/ns0:PlaceOfDelivery/ns0:Code/text()" />

    <xsl:choose>
      <xsl:when test="$placeOfDelivery!=''">
        <ns0:PortOfDestination>
          <ns0:Code>
            <xsl:value-of select="$placeOfDelivery"/>
          </ns0:Code>
          <ns0:Name>
            <xsl:value-of select="$shipment/ns0:PlaceOfDelivery/ns0:Name/text()"/>
          </ns0:Name>
        </ns0:PortOfDestination>
      </xsl:when>
      <xsl:otherwise>
        <xsl:copy-of select="ns0:PortOfDestination"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="/ns0:UniversalShipment/ns0:Shipment/ns0:ReleaseType">
    <xsl:choose>
      <xsl:when test="$recipientRole = 'CAR'">
        <xsl:variable name="releaseType" select="/ns0:UniversalShipment/ns0:Shipment/ns0:ReleaseType/ns0:Code/text()"/>
        <ns0:ReleaseType>
          <ns0:Code>
            <xsl:choose>
              <xsl:when test="$releaseType = 'BOL'">OBR</xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$releaseType"/>
              </xsl:otherwise>
            </xsl:choose>
          </ns0:Code>
          <xsl:copy-of select ="/ns0:UniversalShipment/ns0:Shipment/ns0:ReleaseType/ns0:Description"/>
        </ns0:ReleaseType>
      </xsl:when>
      <xsl:otherwise>
        <xsl:copy-of select ="/ns0:UniversalShipment/ns0:Shipment/ns0:ReleaseType"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="ns0:AdditionalReferenceCollection">
    <ns0:AdditionalReferenceCollection>
      <xsl:for-each select="ns0:AdditionalReference[ns0:Type/ns0:Code/text()!='HIR']">
        <xsl:copy-of select ="."/>
      </xsl:for-each>

      <ns0:AdditionalReference>
        <ns0:Type>
          <ns0:Code>HIR</ns0:Code>
          <ns0:Description>eHub Interchange Reference</ns0:Description>
        </ns0:Type>
        <ns0:ReferenceNumber>
          <xsl:value-of select="$SubscriberConsolReference"/>
        </ns0:ReferenceNumber>
      </ns0:AdditionalReference>
    </ns0:AdditionalReferenceCollection>
  </xsl:template>

  <xsl:template match="ns0:ContainerCollection">
    <ns0:ContainerCollection>
      <xsl:for-each select="ns0:Container">
        <xsl:if test="userCSharp:ShouldCreateContainer($eHubPartyType, $containerMode, ns0:ContainerNumber/text())">
          <ns0:Container>
            <xsl:apply-templates select="@*|node()"/>
          </ns0:Container>
        </xsl:if>
      </xsl:for-each>
    </ns0:ContainerCollection>
  </xsl:template>

  <xsl:template match="ns0:DateCollection">
    <ns0:DateCollection>
      <xsl:for-each select="ns0:Date">
        <xsl:if test="userCSharp:ShouldCreateDate(ns0:Type/text())">
          <xsl:copy-of select ="."/>
        </xsl:if>
      </xsl:for-each>

      <xsl:variable name="latestDeliveryDate" select="ns0:Date[ns0:Type/text()='LatestDelivery']/ns0:Value/text()" />
      <xsl:variable name="arrivalDate">
        <xsl:choose>
          <xsl:when test="$latestDeliveryDate!=''">
            <xsl:value-of select="$latestDeliveryDate"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="ns0:Date[ns0:Type/text()='Arrival']/ns0:Value/text()" />
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:if test="$arrivalDate!=''">
        <ns0:Date>
          <ns0:Type>Arrival</ns0:Type>
          <ns0:IsEstimate>true</ns0:IsEstimate>
          <ns0:Value>
            <xsl:value-of select="$arrivalDate"/>
          </ns0:Value>
        </ns0:Date>
      </xsl:if>

      <xsl:variable name="earliestDepartureDate" select="ns0:Date[ns0:Type/text()='EarliestDeparture']/ns0:Value/text()" />
      <xsl:variable name="departureDate">
        <xsl:choose>
          <xsl:when test="$earliestDepartureDate!=''">
            <xsl:value-of select="$earliestDepartureDate"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="ns0:Date[ns0:Type/text()='Departure']/ns0:Value/text()" />
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:if test="$departureDate!=''">
        <ns0:Date>
          <ns0:Type>Departure</ns0:Type>
          <ns0:IsEstimate>true</ns0:IsEstimate>
          <ns0:Value>
            <xsl:value-of select="$departureDate"/>
          </ns0:Value>
        </ns0:Date>
      </xsl:if>
    </ns0:DateCollection>
  </xsl:template>

  <xsl:template match="/ns0:UniversalShipment/ns0:Shipment/ns0:NoteCollection">
    <ns0:NoteCollection>
      <xsl:for-each select="ns0:Note">
        <ns0:Note>
          <xsl:copy-of select ="ns0:Description"/>
          <xsl:copy-of select ="ns0:NoteText"/>

          <xsl:variable name="isCustomDescriptionByNoteType" select="userCSharp:GetNoteIsCustomDescription(ns0:Description/text())" />
          <xsl:variable name="isCustomDescription">
            <xsl:choose>
              <xsl:when test="$isCustomDescriptionByNoteType!=''">
                <xsl:value-of select="$isCustomDescriptionByNoteType"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="ns0:IsCustomDescription/text()"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <xsl:if test="$isCustomDescription!=''">
            <ns0:IsCustomDescription>
              <xsl:value-of select="$isCustomDescription"/>
            </ns0:IsCustomDescription>
          </xsl:if>
        </ns0:Note>
      </xsl:for-each>

      <xsl:variable name="clearList1" select="ListHelper:ClearList()"/>
      <xsl:for-each select="$shipment/ns0:PaymentHandlingInstructionCollection/ns0:PaymentHandlingInstruction[ns0:Category/ns0:Description/text()!='' and ns0:PaymentMethod/ns0:Description/text()!='']">
        <xsl:variable name="addPaymentHandlingInstructionToList" select="ListHelper:AddToListIfNotExists(concat(ns0:Category/ns0:Description/text(), ' - ', ns0:PaymentMethod/ns0:Description/text()))" />
      </xsl:for-each>
      <xsl:variable name="paymentInstructionNote" select="ListHelper:ToStringWithNewLine()"/>
      <xsl:if test="$paymentInstructionNote!=''">
        <ns0:Note>
          <ns0:Description>Payment Handling Instructions</ns0:Description>
          <ns0:NoteText>
            <xsl:value-of select="$paymentInstructionNote"/>
          </ns0:NoteText>
          <ns0:IsCustomDescription>false</ns0:IsCustomDescription>
        </ns0:Note>
      </xsl:if>

      <xsl:variable name="consignorRegNumber">
        <xsl:call-template name="GetRegistrationNumberInString">
          <xsl:with-param name="orgAddress" select="$shipment/ns0:OrganizationAddressCollection/ns0:OrganizationAddress[ns0:AddressType/text()='ConsignorDocumentaryAddress']"/>
        </xsl:call-template>
      </xsl:variable>

      <xsl:variable name="consigneeRegNumber">
        <xsl:call-template name="GetRegistrationNumberInString">
          <xsl:with-param name="orgAddress" select="$shipment/ns0:OrganizationAddressCollection/ns0:OrganizationAddress[ns0:AddressType/text()='ConsigneeDocumentaryAddress']"/>
        </xsl:call-template>
      </xsl:variable>

      <xsl:variable name="notifyPartyRegNumber">
        <xsl:call-template name="GetRegistrationNumberInString">
          <xsl:with-param name="orgAddress" select="$shipment/ns0:OrganizationAddressCollection/ns0:OrganizationAddress[ns0:AddressType/text()='NotifyParty']"/>
        </xsl:call-template>
      </xsl:variable>

      <xsl:variable name="clearList2" select="ListHelper:ClearList()"/>
      <xsl:for-each select="$shipment/ns0:BillOfLadingClauseCollection/ns0:BillOfLadingClause[ns0:Type/ns0:Description/text()!='']">
        <xsl:variable name="addBillOfLadingClauseToList" select="ListHelper:AddToListIfNotExists(ns0:Type/ns0:Description/text())" />
      </xsl:for-each>

      <xsl:if test="$consignorRegNumber!=''">
        <xsl:variable name="addRegistrationNumberIntoList" select="ListHelper:AddToListIfNotExists(concat('Consignor Tax ID: ', $consignorRegNumber))" />
      </xsl:if>

      <xsl:if test="$consigneeRegNumber!=''">
        <xsl:variable name="addRegistrationNumberIntoList" select="ListHelper:AddToListIfNotExists(concat('Consignee Tax ID: ', $consigneeRegNumber))" />
      </xsl:if>

      <xsl:if test="$notifyPartyRegNumber!=''">
        <xsl:variable name="addRegistrationNumberIntoList" select="ListHelper:AddToListIfNotExists(concat('Notify Party Tax ID: ', $notifyPartyRegNumber))" />
      </xsl:if>

      <xsl:variable name="billOfLadingClauseNote" select="ListHelper:ToStringWithNewLine()"/>
      <xsl:if test="$billOfLadingClauseNote!=''">
        <ns0:Note>
          <ns0:Description>Additional Bill Clauses</ns0:Description>
          <ns0:NoteText>
            <xsl:value-of select="$billOfLadingClauseNote"/>
          </ns0:NoteText>
          <ns0:IsCustomDescription>false</ns0:IsCustomDescription>
        </ns0:Note>
      </xsl:if>
    </ns0:NoteCollection>
  </xsl:template>

  <xsl:template name="GetOrgAddressRegNumber">
    <xsl:param name="addressType" />

    <xsl:variable name="orgAddress" select="$shipment/ns0:OrganizationAddressCollection/ns0:OrganizationAddress[ns0:AddressType/text()=$addressType]"/>

    <xsl:choose>
      <xsl:when test="$orgAddress">
        <xsl:variable name="USCCC_Code" select="$orgAddress/ns0:RegistrationNumberCollection/ns0:RegistrationNumber[ns0:Type/ns0:Code/text()='CCC' and ns0:CountryOfIssue/ns0:Code/text()='US']/ns0:Value/text()"/>
        <xsl:choose>
          <xsl:when test="$USCCC_Code!=''">
            <xsl:value-of select="$USCCC_Code"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$orgAddress/ns0:RegistrationNumberCollection/ns0:RegistrationNumber[ns0:Type/ns0:Code/text()='C1C']/ns0:Value/text()"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="ns0:Shipment/ns0:OrganizationAddressCollection/ns0:OrganizationAddress">

    <xsl:if test="ns0:CompanyName/text()!=''">

      <xsl:variable name="orgAddressType" select="ns0:AddressType/text()" />
      <xsl:variable name="addressType">
        <xsl:choose>
          <xsl:when test="$orgAddressType='CurrentUser'">BookingPartyDocumentaryAddress</xsl:when>
          <xsl:when test="$orgAddressType='BookingPartyDocumentaryAddress'">Forwarder</xsl:when>
          <xsl:when test="$orgAddressType='CurrentUser'"></xsl:when>
          <xsl:when test="$orgAddressType='Forwarder'"></xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$orgAddressType"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:if test="$addressType!=''">
        <xsl:call-template name="PopulateOrganisationAddressCore">
          <xsl:with-param name="org" select="." />
          <xsl:with-param name="elementName" select="'ns0:OrganizationAddress'" />
          <xsl:with-param name="addressType" select="$addressType" />
        </xsl:call-template>
      </xsl:if>
    </xsl:if>
  </xsl:template>

  <xsl:template match="ns0:Carrier">
    <xsl:if test="ns0:CompanyName/text()!=''">
      <xsl:call-template name="PopulateOrganisationAddressCore">
        <xsl:with-param name="org" select="." />
        <xsl:with-param name="elementName" select="'ns0:Carrier'" />
        <xsl:with-param name="addressType" select="ns0:AddressType/text()" />
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name="PopulateOrganisationAddressCore">
    <xsl:param name="org"/>
    <xsl:param name="addressType"/>
    <xsl:param name="elementName"/>

    <xsl:element name="{$elementName}">
      <ns0:AddressType>
        <xsl:value-of select="$addressType"/>
      </ns0:AddressType>
      <xsl:copy-of select ="$org/ns0:OrganizationCode"/>
      <xsl:copy-of select ="$org/ns0:Address1"/>
      <xsl:copy-of select ="$org/ns0:Address2"/>
      <xsl:copy-of select ="$org/ns0:City"/>
      <xsl:copy-of select ="$org/ns0:CompanyName"/>
      <xsl:copy-of select ="$org/ns0:Contact"/>
      <xsl:copy-of select ="$org/ns0:Country"/>
      <xsl:copy-of select ="$org/ns0:Email"/>
      <xsl:copy-of select ="$org/ns0:Fax"/>
      <xsl:copy-of select ="$org/ns0:Phone"/>
      <xsl:copy-of select ="$org/ns0:Port"/>
      <xsl:copy-of select ="$org/ns0:Postcode"/>
      <xsl:copy-of select ="$org/ns0:State"/>

      <xsl:element name ="ns0:RegistrationNumberCollection">
        <xsl:if test="$addressType='ShippingLineAddress' or $addressType='Carrier' or $addressType='CoLoadWith'">
          <xsl:for-each select="$org/ns0:RegistrationNumberCollection/ns0:RegistrationNumber">
            <ns0:RegistrationNumber>
              <xsl:copy-of select ="*[local-name()='CountryOfIssue']"/>
              <xsl:copy-of select ="*[local-name()='Type']"/>
              <xsl:copy-of select ="*[local-name()='Value']"/>
            </ns0:RegistrationNumber>
          </xsl:for-each>
        </xsl:if>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:variable name="groupingMethod" select="$shipment/ns0:AddInfoCollection/ns0:AddInfo[ns0:Key/text()='GroupingMethod']/ns0:Value/text()" />
  <xsl:template match="ns0:SubShipmentCollection">
    <ns0:PackingLineCollection>
      <xsl:for-each select="ns0:SubShipment">
        <xsl:for-each select="ns0:PackingLineCollection/ns0:PackingLine">
          <xsl:call-template name="CreatePackingLine">
            <xsl:with-param name="packingLine" select="." />
          </xsl:call-template>
        </xsl:for-each>
      </xsl:for-each>
    </ns0:PackingLineCollection>
  </xsl:template>

  <xsl:template name="CreatePackingLine">
    <xsl:param name="packingLine"/>

    <ns0:PackingLine>
      <xsl:copy-of select ="$packingLine/ns0:ContainerLink"/>
      <xsl:copy-of select ="$packingLine/ns0:ContainerNumber"/>
      <xsl:copy-of select ="$packingLine/ns0:DetailedDescription"/>
      <xsl:copy-of select ="$packingLine/ns0:ExportReferenceNumber"/>
      <xsl:copy-of select ="$packingLine/ns0:GoodsDescription"/>
      <xsl:copy-of select ="$packingLine/ns0:HarmonisedCode"/>
      <xsl:copy-of select ="$packingLine/ns0:MarksAndNos"/>
      <xsl:copy-of select ="$packingLine/ns0:PackQty"/>
      <xsl:copy-of select ="$packingLine/ns0:PackType"/>
      <xsl:copy-of select ="$packingLine/ns0:ReferenceNumber"/>
      <xsl:copy-of select ="$packingLine/ns0:Vehicle"/>
      <xsl:copy-of select ="$packingLine/ns0:Volume"/>
      <xsl:copy-of select ="$packingLine/ns0:VolumeUnit"/>
      <xsl:copy-of select ="$packingLine/ns0:Weight"/>
      <xsl:copy-of select ="$packingLine/ns0:WeightUnit"/>
      <xsl:copy-of select ="$packingLine/ns0:ClassificationCollection"/>
      <xsl:copy-of select ="$packingLine/ns0:UNDGCollection"/>
    </ns0:PackingLine>
  </xsl:template>

  <xsl:template name="GetRegistrationNumberInString">
    <xsl:param name="orgAddress" />

    <xsl:variable name="clearList" select="ListHelper:ClearList()" />

    <xsl:for-each select="$orgAddress/ns0:RegistrationNumberCollection/ns0:RegistrationNumber">
      <xsl:variable name="regulatingCountryCode" select="ns0:RegulatingCountry/text()" />
      <xsl:variable name="registrationNumberText">
        <xsl:choose>
          <xsl:when test="$regulatingCountryCode!=''">
            <xsl:value-of select="concat(ns0:Value/text(), ' [', $regulatingCountryCode, ']')" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="ns0:Value/text()" />
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="addRegistrationNumber" select="ListHelper:AddToListIfNotExists($registrationNumberText)" />
    </xsl:for-each>

    <xsl:value-of select="ListHelper:ToStringWithDelimiter(', ')"/>
  </xsl:template>

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[
    public string GetNoteIsCustomDescription(string noteType)
    {
        var isTrueNoteTypes = new System.Collections.Generic.List<string>() {"USCanadaManifestSelfFilerID", "WoodenPackageProcessType", "OtherBillClauses", "ChargesFreighted"};
        var isFalseNoteTypes = new System.Collections.Generic.List<string>() {"Payment Handling Instructions", "Additional Bill Clauses", "Goods Handling Instructions", "Forwarding Instruction Notes", "Special Instructions"};

        var result = "";

        if (isTrueNoteTypes.Contains(noteType))
        {
          result = "true";
        }
        else if (isFalseNoteTypes.Contains(noteType))
        {
          result = "false";
        }

        return result;
    }

    public bool ShouldCreateContainer(string partyType, string containerMode, string containerNumber)
    {
      var result = true;
      if (partyType == "NVOCC" && containerMode == "LCL" && string.IsNullOrEmpty(containerNumber))
      {
        result = false;
      }
      return result;
    }

    public bool ShouldCreateDate(string dateType)
    {
      var result = false;
      if (!string.IsNullOrEmpty(dateType))
      {
        var disallowDateTypes = new System.Collections.Generic.List<string>() {"EarliestDeparture", "LatestDelivery", "Arrival", "Departure"};

        if (!disallowDateTypes.Contains(dateType))
        {
          result = true;
        }
      }
      return result;
    }

    public string ThrowPartyReceiverIDNotFound(string SCACCode)
    {
      throw new ArgumentException(string.Format(@"Could not found matching PartyReceiverID in the Client Registration Lookup.(Client Registration: CARGOWISE, Input: [SCAC/C1C:{0}])", SCACCode));
    }

    public string ToUpper(string input)
    {
      string result = "";
      if (input != null)
      {
        result = input;
      }
      return result.ToUpper();
    }
]]>
  </msxsl:script>
</xsl:stylesheet>

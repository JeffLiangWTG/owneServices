<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 CodeMapper DataModelAccessor StringMapper ContextAccessor DateMapper userCSharp StringHelper ListHelper UnitConverter OCMHelper SubscriptionHelper" version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
                xmlns:DateMapper="http://schemas.microsoft.com/BizTalk/2003/DateMapper"
                xmlns:DataModelAccessor="http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"
                xmlns:StringMapper="http://schemas.microsoft.com/BizTalk/2003/StringMapper"
                xmlns:StringHelper="http://schemas.microsoft.com/BizTalk/2003/StringHelper"
                xmlns:ListHelper="http://schemas.microsoft.com/BizTalk/2003/ListHelper"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp"
                xmlns:UnitConverter="http://schemas.microsoft.com/BizTalk/2003/UnitConverter"
                xmlns:OCMHelper="http://schemas.microsoft.com/BizTalk/2003/OCMHelper"
                xmlns:SubscriptionHelper="http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes" />

  <xsl:template match="/">
    <xsl:apply-templates select="s0:UniversalShipment/s0:Shipment" />
  </xsl:template>

  <xsl:variable name="MappingId" select="'OCMCargowise'"/>
  <xsl:variable name="MappingDescription" select="'OCM Cargowise System Configuration'"/>

  <xsl:variable name="SenderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="RecipientID" select="ContextAccessor:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>

  <xsl:variable name="ServiceProvider" select="OCMHelper:GetServiceProvider($RecipientID)" />

  <xsl:variable name="CarrierName" select="CodeMapper:GetRecipientCode('SHIPPING_INSTRUCTION', 'SHIPPING_INSTRUCTION', 'OCM System Configuration', 'CarrierSettings', 'CarrierName', $ServiceProvider)" />
  <xsl:variable name="CarrierID" select="CodeMapper:GetRecipientCode('SHIPPING_INSTRUCTION', 'SHIPPING_INSTRUCTION', 'OCM System Configuration', 'CarrierSettings', 'ID', $ServiceProvider)" />
  <xsl:variable name="CarrierMSGID" select="CodeMapper:GetRecipientCode('SHIPPING_INSTRUCTION', 'SHIPPING_INSTRUCTION', 'OCM System Configuration', 'CarrierSettings', 'MSGID', $ServiceProvider)" />
  <xsl:variable name="CarrierBRSID" select="CodeMapper:GetRecipientCode('SHIPPING_INSTRUCTION', 'SHIPPING_INSTRUCTION', 'OCM System Configuration', 'CarrierSettings', 'BRSID', $ServiceProvider)" />
  <xsl:variable name="CarrierSubscriptionPrefix" select="CodeMapper:GetRecipientCode('SHIPPING_INSTRUCTION', 'SHIPPING_INSTRUCTION', 'OCM System Configuration', 'CarrierSettings', 'SubscriptionPrefix', $ServiceProvider)" />
  <xsl:variable name="combineRecipientMappingNamev1" select="concat($CarrierName, ' Provider Configuration')"/>
  <xsl:variable name="combineRecipientCodeField" select="concat($CarrierName, ' Code')"/>

  <xsl:template match="s0:UniversalShipment/s0:Shipment">

    <xsl:variable name="consolID" select="s0:DataContext/s0:DataSourceCollection/s0:DataSource/s0:Key[1]/text()" />
    <xsl:variable name="InterchangeNum" select="CodeMapper:CallActionProcedureHelper('GetCounterValue','@value','@name',concat('CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.', $CarrierName),'@maxlength','14')" />
    <xsl:variable name="dataVersion" select="s0:DataContext/s0:DocumentaryOverride/s0:DataVersion/text()" />

    <xsl:variable name="previousConsolReference" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $CarrierName, '@recipientId', $SenderID , '@ST_ID', $CarrierMSGID, '@value', $consolID, '@referenceType' , 'OCMBR')" />
    <xsl:variable name="SubscriberConsolReference">
      <xsl:choose>
        <xsl:when test="$previousConsolReference!=''">
          <xsl:value-of select="$previousConsolReference" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:variable name="formattedCounter" select='format-number($InterchangeNum, "0000000000")' />
          <xsl:variable name="NewConsolReference" select="concat($CarrierSubscriptionPrefix, $formattedCounter)" />
          <xsl:variable name="SubScriberValue1" select="DataModelAccessor:InsertSubscriptionValue($CarrierMSGID, $CarrierName, $SenderID, $NewConsolReference, $consolID, 'OCMBR')" />
          <xsl:variable name="SubScriberValue2" select="DataModelAccessor:InsertSubscriptionValue($CarrierMSGID, $CarrierName, $SenderID, $consolID, $NewConsolReference, 'OCMBR')" />
          <xsl:value-of select="$NewConsolReference"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="shipmentType" select="s0:ShipmentType/s0:Code/text()"/>
    <xsl:variable name="deliveryMode" select="s0:ContainerCollection/s0:Container/s0:DeliveryMode/text()" />

    <xsl:variable name="isCoLoad" select="StringHelper:ToUpper(CodeMapper:GetRecipientCode('SHIPPING_INSTRUCTION', 'SHIPPING_INSTRUCTION', 'OCM System Configuration', 'NVOCC', 'Is Co-load', $shipmentType))" />
    <xsl:variable name="addInfo_IsCoLoad">
      <xsl:variable name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='IsCoLoad']/s0:Value/text()"/>
      <xsl:choose>
        <xsl:when test="$value != ''">
          <xsl:value-of select="StringHelper:ToUpper($value)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$isCoLoad" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="carrierAddressType">
      <xsl:choose>
        <xsl:when test="$isCoLoad = 'TRUE'">CoLoadWith</xsl:when>
        <xsl:otherwise>ShippingLineAddress</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="carrierOrgAddress" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text() = $carrierAddressType]"/>

    <xsl:variable name="ClientID" select="DataModelAccessor:GetClientRegistrationCode($SenderID, s0:DataContext/s0:EventBranch/s0:Code/text(), $CarrierName)" />

    <xsl:variable name="InboxPK" select="ContextAccessor:GetContextProperty('InternalTrackingID', 'http://cargowise.com/ehub/tracking/2010/06')"/>
    <xsl:variable name="SubscribeInboxPK" select="DataModelAccessor:InsertSubscriptionValue($CarrierMSGID, $CarrierName, $SenderID, $InboxPK, $InterchangeNum)" />

    <xsl:variable name="incomingDocName" select="s0:DataContext/s0:DocumentaryOverride/s0:DocumentName/text()" />

    <xsl:variable name="SubscribeClientID" select="DataModelAccessor:InsertSubscriptionValue($CarrierID, $CarrierName, $SenderID, $ClientID)" />
    <xsl:variable name="purposeCode" select="s0:DataContext/s0:DocumentaryOverride/s0:Purpose/s0:Code/text()"/>
    <xsl:variable name="SubscribeActionPurpose" select="DataModelAccessor:InsertSubscriptionValue($CarrierMSGID, $CarrierName, $SenderID, $SubscriberConsolReference, $purposeCode, 'ActionPurpose')" />
    <xsl:variable name="subscribeShipmentType" select="DataModelAccessor:InsertSubscriptionValue($CarrierMSGID, $CarrierName, $SenderID, $SubscriberConsolReference, $shipmentType, 'ShipmentType')" />
    <xsl:variable name="SubscribeDocumentName" select="DataModelAccessor:InsertSubscriptionValue($CarrierMSGID, $CarrierName, $SenderID, $SubscriberConsolReference, 'Booking Request', 'DocumentName')" />
    <xsl:variable name="SubscribeForwardingType" select="DataModelAccessor:InsertSubscriptionValue($CarrierMSGID, $CarrierName, $SenderID, $SubscriberConsolReference, 'ForwardingConsol', 'ForwardingType')" />
    <xsl:variable name="formVersion" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='FormVersion']/s0:Value/text()" />
    <xsl:if test="$formVersion !=''">
      <xsl:variable name="SubscribeFormVersion" select="DataModelAccessor:InsertSubscriptionValue($CarrierMSGID, $CarrierName, $SenderID, $SubscriberConsolReference, $formVersion, 'FormVersion')" />
    </xsl:if>

    <xsl:variable name="FileName" select="ContextAccessor:SetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06', concat('BR_',$ClientID,'_',$InterchangeNum))"/>

    <xsl:variable name="placeOfReceipt" select="s0:PlaceOfReceipt"/>
    <xsl:variable name="placeOfDelivery" select="s0:PlaceOfDelivery"/>

    <xsl:variable name="IsGroupedPackingLine">
      <xsl:choose>
        <xsl:when test="s0:AddInfoCollection/s0:AddInfo[s0:Key='GroupingMethod']/s0:Value/text()">
          <xsl:value-of select="'true'"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="'false'"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="InsertBoleroSubscription" select="SubscriptionHelper:InsertBoleroSubscription($SenderID, $ClientID, $ServiceProvider, $SubscriberConsolReference, $consolID)" />

    <BookingRequest xmlns="http://www.cargowise.com/Schemas/OCM/BookingRequest/1">
      <MessageContext>
        <SenderID>
          <xsl:value-of select="CodeMapper:GetRecipientCode($MappingId, $MappingId, $MappingDescription, 'MessageParty', 'SenderID', $RecipientID)" />
        </SenderID>
        <RecipientID>
          <xsl:value-of select="CodeMapper:GetRecipientCode($MappingId, $MappingId, $MappingDescription, 'MessageParty', 'RecipientID', $RecipientID)" />
        </RecipientID>
        <MessageDateTime>
          <xsl:value-of select="DateMapper:CurrentDateTimeUTC('yyyy-MM-ddTHH:mm:ss')"/>
        </MessageDateTime>
        <MessageNumber>
          <xsl:value-of select="$InterchangeNum" />
        </MessageNumber>

        <ServiceType>
          <xsl:choose>
            <xsl:when test="$addInfo_IsCoLoad = 'TRUE'">NVOCC</xsl:when>
            <xsl:otherwise>VOCC</xsl:otherwise>
          </xsl:choose>
        </ServiceType>

        <xsl:variable name="requestType">
          <xsl:choose>
            <xsl:when test="$purposeCode = 'WTH'">C</xsl:when>
            <xsl:when test="s0:DataContext/s0:DocumentaryOverride/s0:DataVersion/text() = 1">N</xsl:when>
            <xsl:otherwise>U</xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <RequestType>
          <xsl:value-of select="$requestType" />
        </RequestType>
      </MessageContext>

      <BookingDateTime>
        <xsl:value-of select="s0:DataContext/s0:TriggerDate/text()" />
      </BookingDateTime>

      <xsl:variable name="currentUser" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='CurrentUser']" />
      <xsl:if test="$currentUser!= ''">
        <Contact>
          <Name>
            <xsl:value-of select="$currentUser/s0:Contact/text()" />
          </Name>
          <Phone>
            <xsl:value-of select="$currentUser/s0:Phone/text()" />
          </Phone>
          <Email>
            <xsl:value-of select="$currentUser/s0:Email/text()" />
          </Email>
          <Fax>
            <xsl:value-of select="$currentUser/s0:Fax/text()" />
          </Fax>
        </Contact>
      </xsl:if>

      <MovementType>
        <xsl:choose>
          <xsl:when test="$deliveryMode = 'CFS/CFS'">Door-Door</xsl:when>
          <xsl:when test="$deliveryMode = 'CFS/CY'">Door-Port</xsl:when>
          <xsl:when test="$deliveryMode = 'CY/CFS'">Port-Door</xsl:when>
          <xsl:when test="$deliveryMode = 'CY/CY'">Port-Port</xsl:when>
        </xsl:choose>
      </MovementType>

      <CargoMode>
        <xsl:value-of select="s0:ContainerMode/s0:Code/text()"/>
      </CargoMode>

      <ReferenceNumberCollection>
        <xsl:call-template name="ReferenceTypeValuePair">
          <xsl:with-param name="type" select="'MessageReference'" />
          <xsl:with-param name="value" select="$SubscriberConsolReference" />
        </xsl:call-template>
        <xsl:call-template name="ReferenceTypeValuePair">
          <xsl:with-param name="type" select="'ForwarderReference'" />
          <xsl:with-param name="value" select="$consolID" />
        </xsl:call-template>
        <xsl:call-template name="ReferenceTypeValuePair">
          <xsl:with-param name="type" select="'BookingNumber'" />
          <xsl:with-param name="value">
            <xsl:choose>
              <xsl:when test="$isCoLoad = 'TRUE'">
                <xsl:value-of select="s0:CoLoadBookingConfirmationReference/text()" />
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="s0:BookingConfirmationReference/text()" />
              </xsl:otherwise>
            </xsl:choose>
          </xsl:with-param>
        </xsl:call-template>
        <xsl:call-template name="ReferenceTypeValuePair">
          <xsl:with-param name="type" select="'BillOfLadingNumber'" />
          <xsl:with-param name="value">
            <xsl:choose>
              <xsl:when test="$isCoLoad = 'TRUE'">
                <xsl:value-of select="s0:CoLoadMasterBillNumber/text()" />
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="s0:WayBillNumber/text()" />
              </xsl:otherwise>
            </xsl:choose>
          </xsl:with-param>
        </xsl:call-template>
        <xsl:call-template name="ReferenceTypeValuePair">
          <xsl:with-param name="type" select="'ShipperReference'" />
          <xsl:with-param name="value" select="s0:AdditionalReferenceCollection/s0:AdditionalReference[s0:Type/s0:Code='OAG']/s0:ReferenceNumber/text()" />
        </xsl:call-template>

        <xsl:variable name="clearList" select="ListHelper:ClearList()" />
        <xsl:variable name="cqnNumber" select="s0:AdditionalReferenceCollection/s0:AdditionalReference[s0:Type/s0:Code/text()='CQN']/s0:ReferenceNumber/text()" />
        <xsl:choose>
          <xsl:when test="$cqnNumber">
            <xsl:for-each select="$cqnNumber">
              <xsl:if test="ListHelper:AddToListIfNotExists(.)">
                <xsl:call-template name="ReferenceTypeValuePair">
                  <xsl:with-param name="type" select="'ContractNumber'" />
                  <xsl:with-param name="value" select="." />
                </xsl:call-template>
              </xsl:if>
            </xsl:for-each>
          </xsl:when>
          <xsl:otherwise>
            <xsl:for-each select="s0:AdditionalReferenceCollection/s0:AdditionalReference[s0:Type/s0:Code/text()='CON']/s0:ReferenceNumber/text()">
              <xsl:if test="ListHelper:AddToListIfNotExists(.)">
                <xsl:call-template name="ReferenceTypeValuePair">
                  <xsl:with-param name="type" select="'ContractNumber'" />
                  <xsl:with-param name="value" select="." />
                </xsl:call-template>
              </xsl:if>
            </xsl:for-each>
          </xsl:otherwise>
        </xsl:choose>

        <xsl:call-template name="ReferenceTypeValuePair">
          <xsl:with-param name="type" select="'LCNumber'" />
          <xsl:with-param name="value" select="s0:AdditionalReferenceCollection/s0:AdditionalReference[s0:Type/s0:Code='LCR']/s0:ReferenceNumber/text()" />
        </xsl:call-template>

        <xsl:call-template name="ReferenceTypeValuePair">
          <xsl:with-param name="type" select="'ContractNamedAccount'" />
          <xsl:with-param name="value" select="s0:AdditionalReferenceCollection/s0:AdditionalReference[s0:Type/s0:Code='NAC']/s0:ReferenceNumber/text()" />
        </xsl:call-template>

      </ReferenceNumberCollection>

      <RemarkCollection>
        <xsl:variable name="goodsHandlingInstruction" select="s0:NoteCollection/s0:Note[s0:Description/text() = 'Goods Handling Instructions']" />
        <xsl:call-template name="RemarkTypeValuePair">
          <xsl:with-param name="type" select="'GoodsHandlingInstruction'" />
          <xsl:with-param name="value" select="$goodsHandlingInstruction/s0:NoteText/text()" />
        </xsl:call-template>

        <xsl:variable name="specialInstruction" select="s0:NoteCollection/s0:Note[s0:Description/text() = 'Special Instructions']" />
        <xsl:call-template name="RemarkTypeValuePair">
          <xsl:with-param name="type" select="'SpecialInstruction'" />
          <xsl:with-param name="value" select="$specialInstruction/s0:NoteText/text()" />
        </xsl:call-template>

        <xsl:variable name="reasonForAmendment" select="s0:NoteCollection/s0:Note[s0:Description/text() = 'ReasonForMessageAmendment']" />
        <xsl:call-template name="RemarkTypeValuePair">
          <xsl:with-param name="type" select="'ReasonForAmendment'" />
          <xsl:with-param name="value" select="$reasonForAmendment/s0:NoteText/text()" />
        </xsl:call-template>

        <xsl:variable name="reasonForCancellation" select="s0:NoteCollection/s0:Note[s0:Description/text() = 'ReasonForMessageCancellation']" />
        <xsl:call-template name="RemarkTypeValuePair">
          <xsl:with-param name="type" select="'ReasonForCancellation'" />
          <xsl:with-param name="value" select="$reasonForCancellation/s0:NoteText/text()" />
        </xsl:call-template>
      </RemarkCollection>

      <xsl:variable name ="usCanadaManifestSelfFilerIDNode" select="s0:NoteCollection/s0:Note[s0:Description/text()='USCanadaManifestSelfFilerID']"/>
      <xsl:if test="$usCanadaManifestSelfFilerIDNode">

        <xsl:variable name ="usCanadaManifestSelfFilerIDValue" select="$usCanadaManifestSelfFilerIDNode/s0:NoteText/text()"/>
        <xsl:variable name="placeOfDeliveryCode">
          <xsl:choose>
            <xsl:when test="$placeOfDelivery/s0:Code/text() != ''">
              <xsl:value-of select="substring($placeOfDelivery/s0:Code/text(), 1, 2)"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="substring(s0:PortOfDischarge/s0:Code/text(), 1, 2)"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <CustomsFilingInstruction>
          <Country>
            <xsl:value-of select="$placeOfDeliveryCode" />
          </Country>

          <FilingParty>
            <xsl:choose>
              <xsl:when test="$usCanadaManifestSelfFilerIDValue!=''">Self</xsl:when>
              <xsl:otherwise>Carrier</xsl:otherwise>
            </xsl:choose>
          </FilingParty>

          <FilingPartyCode>
            <xsl:value-of select="$usCanadaManifestSelfFilerIDValue" />
          </FilingPartyCode>
        </CustomsFilingInstruction>
      </xsl:if>

      <LocationCollection>
        <xsl:call-template name="Location">
          <xsl:with-param name="type" select="'PlaceOfReceipt'"/>
          <xsl:with-param name="unlocode" select="$placeOfReceipt/s0:Code/text()" />
          <xsl:with-param name="name" select="$placeOfReceipt/s0:Name/text()" />
        </xsl:call-template>

        <xsl:call-template name="Location">
          <xsl:with-param name="type" select="'PlaceOfDelivery'"/>
          <xsl:with-param name="unlocode" select="$placeOfDelivery/s0:Code/text()" />
          <xsl:with-param name="name" select="$placeOfDelivery/s0:Name/text()" />
        </xsl:call-template>

        <xsl:variable name="carrierBookingOffice">
          <xsl:call-template name="GetCarrierBookingOffice">
            <xsl:with-param name="node" select="."/>
            <xsl:with-param name="addressType" select="$carrierAddressType"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:if test="msxsl:node-set($carrierBookingOffice)/Code/text() != ''">
          <xsl:call-template name="Location">
            <xsl:with-param name="type" select="'CarrierBookingOffice'"/>
            <xsl:with-param name="unlocode" select="msxsl:node-set($carrierBookingOffice)/Code/text()" />
            <xsl:with-param name="name" select="msxsl:node-set($carrierBookingOffice)/Name/text()" />
          </xsl:call-template>
        </xsl:if>
      </LocationCollection>

      <xsl:for-each select="s0:TransportLegCollection/s0:TransportLeg">
        <xsl:variable name="findFirstAndLastLegOrder" select="userCSharp:FindFirstAndLastLegOrder(s0:LegOrder/text())"/>
      </xsl:for-each>

      <xsl:variable name="firstLegOrder" select="userCSharp:GetFirstLegOrder()"/>
      <xsl:variable name="lastLegOrder" select="userCSharp:GetLastLegOrder()"/>
      <xsl:variable name="firstLeg" select="s0:TransportLegCollection/s0:TransportLeg[s0:LegOrder/text() = $firstLegOrder]"/>
      <xsl:variable name="lastLeg" select="s0:TransportLegCollection/s0:TransportLeg[s0:LegOrder/text() = $lastLegOrder]"/>

      <DateTimeCollection>
        <xsl:variable name="earliestDeparture" select="s0:DateCollection/s0:Date[s0:Type/text()='EarliestDeparture']/s0:Value/text()" />
        <xsl:variable name="earliestDepartureValue">
          <xsl:choose>
            <xsl:when test="$earliestDeparture!=''">
              <xsl:value-of select="$earliestDeparture"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$firstLeg/s0:EstimatedDeparture/text()"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:call-template name="DateTimeTypeValuePair">
          <xsl:with-param name="type" select="'EarliestDeparture'" />
          <xsl:with-param name="value" select="userCSharp:ConvertDateToString($earliestDepartureValue)" />
        </xsl:call-template>

        <xsl:variable name="latestDelivery" select="s0:DateCollection/s0:Date[s0:Type/text()='LatestDelivery']/s0:Value/text()" />
        <xsl:variable name="latestDeliveryValue">
          <xsl:choose>
            <xsl:when test="$latestDelivery!=''">
              <xsl:value-of select="$latestDelivery"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$lastLeg/s0:EstimatedArrival/text()"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:call-template name="DateTimeTypeValuePair">
          <xsl:with-param name="type" select="'LatestDelivery'" />
          <xsl:with-param name="value" select="userCSharp:ConvertDateToString($latestDeliveryValue)" />
        </xsl:call-template>

        <xsl:if test="$addInfo_IsCoLoad = 'TRUE' and substring-before($deliveryMode, '/')='CFS'">
          <xsl:variable name="pickupDate" select="s0:DateCollection/s0:Date[s0:Type/text()='Pickup']/s0:Value/text()" />
          <xsl:variable name="firstContainerDepartureEstimatedPickup" select="s0:ContainerCollection/s0:Container/s0:DepartureEstimatedPickup/text()" />

          <xsl:variable name="estPickupDateTime">
            <xsl:choose>
              <xsl:when test="$pickupDate!=''">
                <xsl:value-of select="$pickupDate" />
              </xsl:when>
              <xsl:when test="$firstContainerDepartureEstimatedPickup!=''">
                <xsl:value-of select="$firstContainerDepartureEstimatedPickup" />
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$firstLeg/s0:LCLReceivalCommences/text()" />
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <xsl:call-template name="DateTimeTypeValuePair">
            <xsl:with-param name="type" select="'EstimatedPickupFrom'" />
            <xsl:with-param name="value" select="$estPickupDateTime" />
            <xsl:with-param name="reqFormattedDate" select="'Y'"/>
          </xsl:call-template>
        </xsl:if>
      </DateTimeCollection>

      <OrganizationAddressCollection>
        <xsl:call-template name="address">
          <xsl:with-param name="elementName" select="'OrganizationAddress'"/>
          <xsl:with-param name="org" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text() = 'CurrentUser']"/>
          <xsl:with-param name="type" select="'BookingParty'"/>
          <xsl:with-param name="clientID" select="$ClientID"/>
        </xsl:call-template>

        <xsl:call-template name="address">
          <xsl:with-param name="elementName" select="'OrganizationAddress'"/>
          <xsl:with-param name="org" select="$carrierOrgAddress"/>
          <xsl:with-param name="type" select="'Carrier'"/>
        </xsl:call-template>

        <xsl:call-template name="address">
          <xsl:with-param name="elementName" select="'OrganizationAddress'"/>
          <xsl:with-param name="org" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text() = 'BookingPartyDocumentaryAddress']"/>
          <xsl:with-param name="type" select="'Forwarder'"/>
        </xsl:call-template>

        <xsl:call-template name="address">
          <xsl:with-param name="elementName" select="'OrganizationAddress'"/>
          <xsl:with-param name="org" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text() = 'ConsignorDocumentaryAddress']"/>
          <xsl:with-param name="type" select="'Consignor'"/>
        </xsl:call-template>

        <xsl:call-template name="address">
          <xsl:with-param name="elementName" select="'OrganizationAddress'"/>
          <xsl:with-param name="org" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text() = 'ConsigneeDocumentaryAddress']"/>
          <xsl:with-param name="type" select="'Consignee'"/>
        </xsl:call-template>

        <xsl:call-template name="address">
          <xsl:with-param name="elementName" select="'OrganizationAddress'"/>
          <xsl:with-param name="org" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text() = 'NotifyParty']"/>
          <xsl:with-param name="type" select="'MainNotifyParty'"/>
        </xsl:call-template>

        <xsl:call-template name="address">
          <xsl:with-param name="elementName" select="'OrganizationAddress'"/>
          <xsl:with-param name="org" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text() = 'NotifyParty2']"/>
          <xsl:with-param name="type" select="'SecondNotifyParty'"/>
        </xsl:call-template>

        <xsl:call-template name="address">
          <xsl:with-param name="elementName" select="'OrganizationAddress'"/>
          <xsl:with-param name="org" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text() = 'NotifyParty3']"/>
          <xsl:with-param name="type" select="'ThirdNotifyParty'"/>
        </xsl:call-template>

        <xsl:call-template name="address">
          <xsl:with-param name="elementName" select="'OrganizationAddress'"/>
          <xsl:with-param name="org" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text() = 'ContractParty']"/>
          <xsl:with-param name="type" select="'ContractParty'"/>
        </xsl:call-template>

        <xsl:call-template name="address">
          <xsl:with-param name="elementName" select="'OrganizationAddress'"/>
          <xsl:with-param name="org" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text() = 'FreightPayer']"/>
          <xsl:with-param name="type" select="'FreightPayer'"/>
        </xsl:call-template>

        <xsl:if test="$addInfo_IsCoLoad = 'TRUE' and substring-before($deliveryMode, '/')='CFS'">
          <xsl:call-template name="address">
            <xsl:with-param name="elementName" select="'OrganizationAddress'"/>
            <xsl:with-param name="org" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text() = 'ConsignorPickupDeliveryAddress']"/>
            <xsl:with-param name="type" select="'PickupFrom'"/>
          </xsl:call-template>
        </xsl:if>

        <xsl:if test="$addInfo_IsCoLoad = 'TRUE' and substring-after($deliveryMode, '/')='CFS'">
          <xsl:call-template name="address">
            <xsl:with-param name="elementName" select="'OrganizationAddress'"/>
            <xsl:with-param name="org" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text() = 'ConsigneePickupDeliveryAddress']"/>
            <xsl:with-param name="type" select="'DeliverTo'"/>
          </xsl:call-template>
        </xsl:if>
      </OrganizationAddressCollection>

      <xsl:variable name="addInfo_FreightPayableAt_Code" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='FreightPayableAt_Code']/s0:Value/text()"/>
      <xsl:variable name="addInfo_FreightPayableAt_Name" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='FreightPayableAt_Name']/s0:Value/text()"/>

      <PaymentTermCollection>
        <xsl:for-each select="s0:PaymentHandlingInstructionCollection/s0:PaymentHandlingInstruction">

          <xsl:variable name="paymentMethod" select="s0:PaymentMethod/s0:Code/text()" />
          <xsl:variable name="paymentCategoryCode" select="s0:Category/s0:Code/text()" />

          <xsl:variable name="unlocode">
            <xsl:choose>
              <xsl:when test="$paymentCategoryCode='FRT' and $addInfo_FreightPayableAt_Code!=''">
                <xsl:value-of select="$addInfo_FreightPayableAt_Code" />
              </xsl:when>
              <xsl:when test="$paymentMethod='PPD'">
                <xsl:value-of select="$placeOfReceipt/s0:Code/text()" />
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$placeOfDelivery/s0:Code/text()" />
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <xsl:variable name="unlocodeName">
            <xsl:choose>
              <xsl:when test="$paymentCategoryCode='FRT' and $addInfo_FreightPayableAt_Code!=''">
                <xsl:value-of select="$addInfo_FreightPayableAt_Name" />
              </xsl:when>
              <xsl:when test="$paymentMethod='PPD'">
                <xsl:value-of select="$placeOfReceipt/s0:Name/text()" />
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$placeOfDelivery/s0:Name/text()" />
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <xsl:call-template name="PaymentTerm">
            <xsl:with-param name="category" select="$paymentCategoryCode" />
            <xsl:with-param name="paymentMethod" select="$paymentMethod" />
            <xsl:with-param name="unlocode" select="$unlocode" />
            <xsl:with-param name="unloconame" select="$unlocodeName" />
          </xsl:call-template>
        </xsl:for-each>
      </PaymentTermCollection>

      <TransportLegCollection>
        <xsl:for-each select="s0:TransportLegCollection/s0:TransportLeg">
          <TransportLeg>
            <Type>
              <xsl:value-of select="s0:LegType/text()" />
            </Type>
            <Mode>
              <xsl:value-of select="s0:TransportMode/text()"></xsl:value-of>
            </Mode>
            <LegOrder>
              <xsl:value-of select="s0:LegOrder/text()"></xsl:value-of>
            </LegOrder>
            <VesselName>
              <xsl:value-of select="s0:VesselName/text()"></xsl:value-of>
            </VesselName>
            <VoyageNumber>
              <xsl:value-of select="s0:VoyageFlightNo/text()"></xsl:value-of>
            </VoyageNumber>
            <LloydsNumber>
              <xsl:value-of select="s0:VesselLloydsIMO/text()"></xsl:value-of>
            </LloydsNumber>
            <CarrierCode>
              <xsl:value-of select="s0:Carrier/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text() = 'CCC' and s0:CountryOfIssue/s0:Code/text() = 'US']/s0:Value/text()"/>
            </CarrierCode>
            <LocationCollection>
              <xsl:call-template name="Location">
                <xsl:with-param name="type" select="'PortOfLoading'"/>
                <xsl:with-param name="unlocode" select="s0:PortOfLoading/s0:Code/text()" />
                <xsl:with-param name="name" select="s0:PortOfLoading/s0:Name/text()" />
              </xsl:call-template>

              <xsl:call-template name="Location">
                <xsl:with-param name="type" select="'PortOfDischarge'"/>
                <xsl:with-param name="unlocode" select="s0:PortOfDischarge/s0:Code/text()" />
                <xsl:with-param name="name" select="s0:PortOfDischarge/s0:Name/text()" />
              </xsl:call-template>
            </LocationCollection>
            <DateTimeCollection>
              <DateTime>
                <Type>EstimatedDeparture</Type>
                <Value>
                  <xsl:value-of select="userCSharp:ConvertDateToString(s0:EstimatedDeparture/text())"></xsl:value-of>
                </Value>
              </DateTime>
              <DateTime>
                <Type>EstimatedArrival</Type>
                <Value>
                  <xsl:value-of select="userCSharp:ConvertDateToString(s0:EstimatedArrival/text())"></xsl:value-of>
                </Value>
              </DateTime>
            </DateTimeCollection>
          </TransportLeg>
        </xsl:for-each>
      </TransportLegCollection>

      <xsl:variable name="includeContainerInfo">
        <xsl:choose>
          <xsl:when test="$addInfo_IsCoLoad = 'TRUE' and s0:ContainerMode/s0:Code/text()='LCL'">N</xsl:when>
          <xsl:otherwise>Y</xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:if test="$includeContainerInfo='Y'">
        <EquipmentCollection>
          <xsl:call-template name="EquipmentCollection">
            <xsl:with-param name="containerCollection" select="s0:ContainerCollection" />
            <xsl:with-param name="organizationAddressCollection" select="s0:OrganizationAddressCollection" />
            <xsl:with-param name="deliveryMode" select="$deliveryMode" />
            <xsl:with-param name="isCoLoad" select="$addInfo_IsCoLoad = 'TRUE'" />
            <xsl:with-param name="isGroupedPackingLine" select="$IsGroupedPackingLine" />
          </xsl:call-template>
        </EquipmentCollection>
      </xsl:if>

      <xsl:if test="$IsGroupedPackingLine='true'">
         <xsl:call-template name="GoodsLineCollection">
          <xsl:with-param name="goodsLineCollection" select="s0:PackingLineCollection/s0:PackingLine/s0:PackingLineCollection" />
          <xsl:with-param name="includeContainerInfo" select="$includeContainerInfo" />
        </xsl:call-template>
      </xsl:if>

      <xsl:if test="$IsGroupedPackingLine='false'">
        <xsl:call-template name="GoodsLineCollection">
          <xsl:with-param name="goodsLineCollection" select="s0:PackingLineCollection"/>
          <xsl:with-param name="includeContainerInfo" select="$includeContainerInfo" />
        </xsl:call-template>
      </xsl:if>
    </BookingRequest>
  </xsl:template>

  <xsl:template name="GetCarrierBookingOffice">
    <xsl:param name="node"/>
    <xsl:param name="addressType"/>

    <xsl:variable name="carrierBookingOffice" select="$node/s0:CarrierBookingOffice"/>
    <xsl:variable name="orgAddressPort" select="$node/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()=$addressType]/s0:Port"/>

    <xsl:choose>
      <xsl:when test="$carrierBookingOffice/s0:Code/text()!= ''">
        <Code>
          <xsl:value-of select="$carrierBookingOffice/s0:Code/text() "/>
        </Code>
        <Name>
          <xsl:value-of select="$carrierBookingOffice/s0:Name/text() "/>
        </Name>
      </xsl:when>
      <xsl:otherwise>
        <Code>
          <xsl:value-of select="$orgAddressPort/s0:Code/text()"/>
        </Code>
        <Name>
          <xsl:value-of select="$orgAddressPort/s0:Name/text()"/>
        </Name>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="address">
    <xsl:param name="elementName"/>
    <xsl:param name="org"/>
    <xsl:param name="type"/>
    <xsl:param name="clientID"/>

    <xsl:variable name="companyName" select="$org/s0:CompanyName/text()"/>

    <xsl:if test="$companyName!='' ">
      <xsl:variable name="address1" select="$org/s0:Address1/text()"/>
      <xsl:variable name="address2" select="$org/s0:Address2/text()"/>
      <xsl:variable name="city" select="$org/s0:City/text()"/>
      <xsl:variable name="postcode" select="$org/s0:Postcode/text()"/>
      <xsl:variable name="state" select="$org/s0:State/text()"/>
      <xsl:variable name="countryCode" select="$org/s0:Country/s0:Code/text()"/>
      <xsl:variable name="countryName" select="$org/s0:Country/s0:Name/text()"/>

      <xsl:variable name="contact" select="$org/s0:Contact/text()"/>
      <xsl:variable name="phone" select="$org/s0:Phone/text()"/>
      <xsl:variable name="email" select="$org/s0:Email/text()"/>
      <xsl:variable name="fax" select="$org/s0:Fax/text()"/>

      <xsl:variable name="addressType" select="$org/s0:AddressType/text()" />
      <xsl:variable name="govTaxNumber">
        <xsl:choose>
          <xsl:when test="$countryCode != '' and not(contains('ConsigneeDocumentaryAddress;ConsignorDocumentaryAddress;NotifyParty', $addressType))">
            <xsl:call-template name="GetOrgRegistrationNumber">
              <xsl:with-param name="org" select="$org"/>
              <xsl:with-param name="orgType" select="$addressType"/>
              <xsl:with-param name="countryCode" select="$countryCode"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise></xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="orgCode">
        <xsl:choose>
          <xsl:when test="$clientID!=''">
            <xsl:value-of select="$clientID"/>
          </xsl:when>
          <xsl:when test="$type='Carrier'">
            <xsl:value-of select="$org/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text()='CCC' and s0:CountryOfIssue/s0:Code/text()='US']/s0:Value/text()" />
          </xsl:when>
          <xsl:otherwise></xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:element name="{$elementName}" xmlns="http://www.cargowise.com/Schemas/OCM/BookingRequest/1">
        <Type>
          <xsl:value-of select="$type" />
        </Type>

        <OrganizationCode>
          <xsl:value-of select="$orgCode"/>
        </OrganizationCode>

        <Name>
          <xsl:value-of select="$companyName" />
        </Name>
        <StreetAddress1>
          <xsl:value-of select="$address1" />
        </StreetAddress1>
        <StreetAddress2>
          <xsl:value-of select="$address2" />
        </StreetAddress2>
        <City>
          <xsl:value-of select="$city" />
        </City>
        <Postcode>
          <xsl:value-of select="$postcode" />
        </Postcode>
        <State>
          <xsl:value-of select="$state" />
        </State>
        <Country>
          <xsl:attribute name="Name">
            <xsl:value-of select="$countryName" />
          </xsl:attribute>
          <xsl:value-of select="$countryCode" />
        </Country>
        <xsl:if test="$contact!='' and ($phone!='' or $email!='' or $fax!='')">
          <Contact>
            <Name>
              <xsl:value-of select="$contact" />
            </Name>
            <Phone>
              <xsl:value-of select="$phone" />
            </Phone>
            <Email>
              <xsl:value-of select="$email" />
            </Email>
            <Fax>
              <xsl:value-of select="$fax" />
            </Fax>
          </Contact>
        </xsl:if>
        <xsl:if test="$govTaxNumber!=''">
          <ReferenceNumberCollection>
            <ReferenceNumber>
              <Type>GovernmentTaxID</Type>
              <Value>
                <xsl:value-of select="$govTaxNumber"/>
              </Value>
            </ReferenceNumber>
          </ReferenceNumberCollection>
        </xsl:if>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="Location">
    <xsl:param name="type" />
    <xsl:param name="unlocode" />
    <xsl:param name="name" />

    <xsl:if test="$unlocode!=''">
      <Location xmlns="http://www.cargowise.com/Schemas/OCM/BookingRequest/1">
        <Type>
          <xsl:value-of select="$type" />
        </Type>
        <UNLOCode>
          <xsl:value-of select="$unlocode" />
        </UNLOCode>
        <Name>
          <xsl:value-of select="$name" />
        </Name>
        <Country>
          <xsl:value-of select="substring($unlocode, 1, 2)" />
        </Country>
      </Location>
    </xsl:if>
  </xsl:template>

  <xsl:template name="PaymentTerm">
    <xsl:param name="category" />
    <xsl:param name="paymentMethod" />
    <xsl:param name="location" />
    <xsl:param name="unlocode" />
    <xsl:param name="unloconame" />

    <xsl:variable name="paymentType">
      <xsl:choose>
        <xsl:when test="$category='FRT'">OceanFreight</xsl:when>
        <xsl:when test="$category='DHC'">DestinationHaulage</xsl:when>
        <xsl:when test="$category='DPC'">DestinationTerminalHandling</xsl:when>
        <xsl:when test="$category='OPC'">OriginHaulage</xsl:when>
        <xsl:when test="$category='OHC'">OriginTerminalHandling</xsl:when>
        <xsl:otherwise></xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:if test="$paymentType!='' and $paymentMethod!=''">
      <PaymentTerm xmlns="http://www.cargowise.com/Schemas/OCM/BookingRequest/1">
        <Type>
          <xsl:value-of select="$paymentType"/>
        </Type>
        <Method>
          <xsl:choose>
            <xsl:when test="$paymentMethod='PPD'">PrePaid</xsl:when>
            <xsl:otherwise>
              <xsl:variable name="payableElseWhereCode">
                <xsl:choose>
                  <xsl:when test="$category='FRT' and $paymentMethod='ELS'">
                    <xsl:value-of select="OCMHelper:GetPayableElseWhereOutputCode($ServiceProvider)" />
                  </xsl:when>
                  <xsl:otherwise></xsl:otherwise>
                </xsl:choose>
              </xsl:variable>

              <xsl:choose>
                <xsl:when test="$payableElseWhereCode!=''">
                  <xsl:value-of select="$payableElseWhereCode"/>
                </xsl:when>
                <xsl:otherwise>Collect</xsl:otherwise>
              </xsl:choose>
            </xsl:otherwise>
          </xsl:choose>
        </Method>

        <xsl:call-template name="Location">
          <xsl:with-param name="type" select="'PaymentLocation'"/>
          <xsl:with-param name="unlocode" select="$unlocode" />
          <xsl:with-param name="name" select="$unloconame" />
        </xsl:call-template>
      </PaymentTerm>
    </xsl:if>
  </xsl:template>

  <xsl:template name="EquipmentCollection">
    <xsl:param name="containerCollection"/>
    <xsl:param name="organizationAddressCollection"/>
    <xsl:param name="deliveryMode"/>
    <xsl:param name="isCoLoad"/>
    <xsl:param name="isGroupedPackingLine"/>

    <xsl:variable name="volumeUnit">
      <xsl:choose>
        <xsl:when test="$isGroupedPackingLine='true'">
          <xsl:value-of select="/s0:UniversalShipment/s0:Shipment/s0:PackingLineCollection/s0:PackingLine/s0:PackingLineCollection/s0:PackingLine[1]/s0:VolumeUnit/s0:Code/text()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="/s0:UniversalShipment/s0:Shipment/s0:PackingLineCollection/s0:PackingLine[1]/s0:VolumeUnit/s0:Code/text()"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:for-each select="$containerCollection/s0:Container">
      <Equipment xmlns="http://www.cargowise.com/Schemas/OCM/BookingRequest/1">
        <xsl:variable name="containerISOCode" select="s0:ContainerType/s0:ISOCode/text()"/>

        <xsl:variable name="carrierContainerISOMappedCode" select="CodeMapper:GetRecipientCode($CarrierName, $CarrierName, $combineRecipientMappingNamev1, 'ContainerTypeToISOCode' , $combineRecipientCodeField, $containerISOCode)"/>
        <xsl:variable name="ContainerTypeMappingCode">
          <xsl:choose>
            <xsl:when test="$carrierContainerISOMappedCode != ''">
              <xsl:value-of select="$carrierContainerISOMappedCode"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="CodeMapper:GetRecipientCode('SHIPPING_INSTRUCTION' , 'SHIPPING_INSTRUCTION' , 'OCM System Configuration', 'ContainerTypeToISOCode' , 'Carrier Code', s0:ContainerType/s0:ISOCode/text())"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <xsl:variable name="isShipperOwned" select="s0:IsShipperOwned/text()" />
        <xsl:variable name="containerNumber" select="s0:ContainerNumber/text()" />
        <xsl:variable name="isActualNumber" select="userCSharp:MatchPattern($containerNumber, '^([\D]{4}[\d]{7})$')" />

        <xsl:variable name="containerLink" select="s0:Link/text()"/>
        <xsl:variable name="totalWeight">
          <xsl:choose>
            <xsl:when test="$isGroupedPackingLine='true'">
              <xsl:value-of select="sum(/s0:UniversalShipment/s0:Shipment/s0:PackingLineCollection/s0:PackingLine/s0:PackingLineCollection/s0:PackingLine[s0:ContainerLink/text()=$containerLink]/s0:Weight/text())"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="sum(/s0:UniversalShipment/s0:Shipment/s0:PackingLineCollection/s0:PackingLine[s0:ContainerLink/text()=$containerLink]/s0:Weight/text())"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:variable name="totalVolume">
          <xsl:choose>
            <xsl:when test="$isGroupedPackingLine='true'">
              <xsl:value-of select="sum(/s0:UniversalShipment/s0:Shipment/s0:PackingLineCollection/s0:PackingLine/s0:PackingLineCollection/s0:PackingLine[s0:ContainerLink/text()=$containerLink]/s0:Volume/text())"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="sum(/s0:UniversalShipment/s0:Shipment/s0:PackingLineCollection/s0:PackingLine[s0:ContainerLink/text()=$containerLink]/s0:Volume/text())"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <xsl:if test="$containerNumber!=''">
          <xsl:call-template name="TypeValuePair">
            <xsl:with-param name="element" select="'EquipmentNumber'" />
            <xsl:with-param name="type">
              <xsl:choose>
                <xsl:when test="$isShipperOwned='true' or $isActualNumber">Actual</xsl:when>
                <xsl:otherwise>Logical</xsl:otherwise>
              </xsl:choose>
            </xsl:with-param>
            <xsl:with-param name="value" select="$containerNumber" />
          </xsl:call-template>
        </xsl:if>

        <EquipmentType>
          <xsl:attribute name="Description">
            <xsl:value-of select="s0:ContainerType/s0:Description/text()"/>
          </xsl:attribute>
          <xsl:value-of select="$ContainerTypeMappingCode" />
        </EquipmentType>
        <EquipmentOwner>
          <xsl:choose>
            <xsl:when test="$isShipperOwned='true'">Shipper</xsl:when>
            <xsl:otherwise>Carrier</xsl:otherwise>
          </xsl:choose>
        </EquipmentOwner>
        <EquipmentShippedStatus>
          <xsl:choose>
            <xsl:when test="s0:IsEmptyContainer/text()='true'">Empty</xsl:when>
            <xsl:otherwise>Full</xsl:otherwise>
          </xsl:choose>
        </EquipmentShippedStatus>
        <EquipmentCount>
          <xsl:value-of select="s0:ContainerCount/text()"/>
        </EquipmentCount>

        <xsl:variable name="haulageArrangement">
          <xsl:choose>
            <xsl:when test="$deliveryMode = 'CFS/CFS'">CC</xsl:when>
            <xsl:when test="$deliveryMode = 'CFS/CY'">CM</xsl:when>
            <xsl:when test="$deliveryMode = 'CY/CFS'">MC</xsl:when>
            <xsl:when test="$deliveryMode = 'CY/CY'">MM</xsl:when>
            <xsl:otherwise></xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:if test="$haulageArrangement != ''" >
          <HaulageArrangement>
            <xsl:value-of select="$haulageArrangement"/>
          </HaulageArrangement>
        </xsl:if>

        <xsl:variable name="isControlledAtmosphere" select="s0:IsControlledAtmosphere/text()" />

        <MeasurementCollection>
          <xsl:call-template name="TypeValuePair">
            <xsl:with-param name="element" select="'Measurement'" />
            <xsl:with-param name="unit" select="s0:WeightUnit/s0:Code/text()" />
            <xsl:with-param name="type" select="'GoodsWeight'" />
            <xsl:with-param name="value" select="StringMapper:FormatDecimal($totalWeight, '0.000', false())" />
          </xsl:call-template>

          <xsl:call-template name="TypeValuePair">
            <xsl:with-param name="element" select="'Measurement'" />
            <xsl:with-param name="unit" select="$volumeUnit" />
            <xsl:with-param name="type" select="'GrossVolume'" />
            <xsl:with-param name="value" select="StringMapper:FormatDecimal($totalVolume, '0.000', false())" />
          </xsl:call-template>

          <xsl:variable name="airVentFlow" select="s0:AirVentFlow/text()"/>
          <xsl:if test="$isControlledAtmosphere='true' and $airVentFlow!=''">
            <xsl:variable name="airVentFlowRateUnit" select="s0:AirVentFlowRateUnit/s0:Code/text()"/>

            <xsl:variable name="airVentFlowValue">
              <xsl:choose>
                <xsl:when test="$airVentFlowRateUnit = 'MQH' or $airVentFlowRateUnit = 'CBM' or $airVentFlowRateUnit = 'P1'">
                  <xsl:value-of select="$airVentFlow"/>
                </xsl:when>
                <xsl:when test="$airVentFlowRateUnit = '2L'">
                  <xsl:value-of select="userCSharp:MultiplyDouble($airVentFlow, '1.6990108', '0.##')"/>
                </xsl:when>
                <xsl:otherwise></xsl:otherwise>
              </xsl:choose>
            </xsl:variable>

            <xsl:if test="$airVentFlowValue != ''">
              <xsl:call-template name="TypeValuePair">
                <xsl:with-param name="element" select="'Measurement'" />
                <xsl:with-param name="unit" select="'M3'" />
                <xsl:with-param name="type" select="'AirFlow'" />
                <xsl:with-param name="value">
                  <xsl:choose>
                    <xsl:when test="$airVentFlowValue=0">
                      <xsl:value-of select="$airVentFlowValue"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="StringMapper:FormatDecimal($airVentFlowValue, '0', false())" />
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:with-param>
              </xsl:call-template>
            </xsl:if>

            <xsl:call-template name="TypeValuePair">
              <xsl:with-param name="element" select="'Measurement'" />
              <xsl:with-param name="unit" select="'PER'" />
              <xsl:with-param name="type" select="'Humidity'" />
              <xsl:with-param name="value" select="StringMapper:FormatDecimal(s0:HumidityPercent/text(), '0', false())" />
            </xsl:call-template>
          </xsl:if>
        </MeasurementCollection>

        <xsl:if test="$isControlledAtmosphere='true' and s0:SetPointTemp/text()!=''">
          <xsl:call-template name="TypeValuePair">
            <xsl:with-param name="element" select="'Temperature'" />
            <xsl:with-param name="type">
              <xsl:choose>
                <xsl:when test="s0:SetPointTempUnit/text()='C'">CEL</xsl:when>
                <xsl:otherwise>FAH</xsl:otherwise>
              </xsl:choose>
            </xsl:with-param>
            <xsl:with-param name="value" select="StringMapper:FormatDecimal(s0:SetPointTemp/text(),'0.0',false())" />
          </xsl:call-template>
        </xsl:if>

        <SpecialHandlingCollection>
          <xsl:if test="$isControlledAtmosphere='true'">
            <xsl:call-template name="TypeValuePair">
              <xsl:with-param name="element" select="'SpecialHandling'" />
              <xsl:with-param name="type" select="'VentSetting'" />
              <xsl:with-param name="value">
                <xsl:choose>
                  <xsl:when test="s0:AirVentFlowRateUnit/s0:Code/text()!='' and s0:AirVentFlow/text()>0">Open</xsl:when>
                  <xsl:otherwise>Closed</xsl:otherwise>
                </xsl:choose>
              </xsl:with-param>
            </xsl:call-template>
          </xsl:if>

          <xsl:if test="s0:NonOperatingReefer/text()='true'">
            <xsl:call-template name="TypeValuePair">
              <xsl:with-param name="element" select="'SpecialHandling'" />
              <xsl:with-param name="type" select="'NonOperativeReefer'" />
              <xsl:with-param name="value" select="'Y'" />
            </xsl:call-template>
          </xsl:if>
        </SpecialHandlingCollection>

        <SealNumberCollection>
          <xsl:call-template name="SealNumber">
            <xsl:with-param name="type" select="s0:SealPartyType/s0:Code/text()" />
            <xsl:with-param name="value" select="s0:Seal/text()" />
          </xsl:call-template>

          <xsl:call-template name="SealNumber">
            <xsl:with-param name="type" select="s0:SecondSealPartyType/s0:Code/text()" />
            <xsl:with-param name="value" select="s0:SecondSeal/text()" />
          </xsl:call-template>

          <xsl:call-template name="SealNumber">
            <xsl:with-param name="type" select="s0:ThirdSealPartyType/s0:Code/text()" />
            <xsl:with-param name="value" select="s0:ThirdSeal/text()" />
          </xsl:call-template>
        </SealNumberCollection>

        <xsl:if test="number(s0:OverhangHeight/text())>0 or
                    number(s0:OverhangBack/text())>0 or
                    number(s0:OverhangFront/text())>0 or
                    number(s0:OverhangLeft/text())>0 or
                    number(s0:OverhangRight/text())>0">
          <Dimensions>
            <Unit>
              <xsl:value-of select="s0:LengthUnit/s0:Code/text()"/>
            </Unit>
            <Length>
              <xsl:value-of select="s0:OverhangFront/text() + s0:OverhangBack/text()"/>
            </Length>
            <Width>
              <xsl:value-of select="s0:OverhangLeft/text() + s0:OverhangRight/text()"/>
            </Width>
            <Height>
              <xsl:value-of select="s0:OverhangHeight/text()"/>
            </Height>
          </Dimensions>
        </xsl:if>

        <OrganizationAddressCollection>
          <xsl:if test="not($isCoLoad = 'TRUE') and substring-before($deliveryMode, '/')='CFS'">
            <xsl:call-template name="address">
              <xsl:with-param name="elementName" select="'OrganizationAddress'"/>
              <xsl:with-param name="org" select="$organizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text() = 'ConsignorPickupDeliveryAddress']"/>
              <xsl:with-param name="type" select="'PickupFrom'"/>
            </xsl:call-template>
          </xsl:if>

          <xsl:if test="not($isCoLoad = 'TRUE') and substring-after($deliveryMode, '/')='CFS'">
            <xsl:call-template name="address">
              <xsl:with-param name="elementName" select="'OrganizationAddress'"/>
              <xsl:with-param name="org" select="$organizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text() = 'ConsigneePickupDeliveryAddress']"/>
              <xsl:with-param name="type" select="'DeliverTo'"/>
            </xsl:call-template>
          </xsl:if>
          <xsl:if test="substring-before($deliveryMode, '/')='CY'">
            <xsl:call-template name="address">
              <xsl:with-param name="elementName" select="'OrganizationAddress'"/>
              <xsl:with-param name="org" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text() = 'ContainerYardEmptyPickupAddress']"/>
              <xsl:with-param name="type" select="'EmptyPickup'"/>
            </xsl:call-template>
          </xsl:if>
        </OrganizationAddressCollection>

        <DateTimeCollection>
          <xsl:if test="not($isCoLoad = 'TRUE') and substring-before($deliveryMode, '/')='CFS'">
            <xsl:call-template name="DateTimeTypeValuePair">
              <xsl:with-param name="type" select="'EmptyPositionAtDoor'" />
              <xsl:with-param name="value" select="s0:EmptyRequired/text()" />
              <xsl:with-param name="reqFormattedDate" select="'Y'"/>
            </xsl:call-template>

            <xsl:call-template name="DateTimeTypeValuePair">
              <xsl:with-param name="type" select="'EstimatedFullPickup'" />
              <xsl:with-param name="value" select="s0:DepartureEstimatedPickup/text()" />
              <xsl:with-param name="reqFormattedDate" select="'Y'"/>
            </xsl:call-template>
          </xsl:if>

          <xsl:if test="substring-before($deliveryMode, '/')='CY'">
            <xsl:call-template name="DateTimeTypeValuePair">
              <xsl:with-param name="type" select="'RequestedEmptyPickup'" />
              <xsl:with-param name="value" select="s0:EmptyRequired/text()" />
              <xsl:with-param name="reqFormattedDate" select="'Y'"/>
            </xsl:call-template>
          </xsl:if>

          <xsl:if test="not($isCoLoad = 'TRUE') and substring-after($deliveryMode, '/')='CFS'">
            <xsl:call-template name="DateTimeTypeValuePair">
              <xsl:with-param name="type" select="'RequestedFullDelivery'" />
              <xsl:with-param name="value" select="s0:ArrivalDeliveryRequiredBy/text()" />
              <xsl:with-param name="reqFormattedDate" select="'Y'"/>
            </xsl:call-template>
          </xsl:if>
        </DateTimeCollection>

      </Equipment>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="SealNumber">
    <xsl:param name="type" />
    <xsl:param name="value" />

    <xsl:if test="$value!=''">
      <xsl:call-template name="TypeValuePair">
        <xsl:with-param name="element" select="'SealNumber'" />
        <xsl:with-param name="type">
          <xsl:choose>
            <xsl:when test="$type='QRT'">Quarantine</xsl:when>
            <xsl:when test="$type='CAR'">Carrier</xsl:when>
            <xsl:when test="$type='CUS'">Customs</xsl:when>
            <xsl:when test="$type='CRD'">Shipper</xsl:when>
            <xsl:when test="$type='CTO'">Terminal</xsl:when>
            <xsl:otherwise></xsl:otherwise>
          </xsl:choose>
        </xsl:with-param>
        <xsl:with-param name="value" select="$value" />
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name="TypeValuePair">
    <xsl:param name="element" />
    <xsl:param name="unit" />
    <xsl:param name="type" />
    <xsl:param name="value" />

    <xsl:element name="{$element}" xmlns="http://www.cargowise.com/Schemas/OCM/BookingRequest/1">
      <Type>
        <xsl:value-of select="$type" />
      </Type>
      <xsl:if test="$unit">
        <Unit>
          <xsl:value-of select="$unit" />
        </Unit>
      </xsl:if>
      <Value>
        <xsl:value-of select="$value" />
      </Value>
    </xsl:element>
  </xsl:template>

  <xsl:template name="GoodsLineCollection">
    <xsl:param name="goodsLineCollection" />
    <xsl:param name="includeContainerInfo" />

    <GoodsLineCollection xmlns="http://www.cargowise.com/Schemas/OCM/BookingRequest/1">
      <xsl:for-each select="$goodsLineCollection/s0:PackingLine">
        <GoodsLine>
          <LineNumber>
            <xsl:value-of select="position()" />
          </LineNumber>
          <NumberOfPackages>
            <xsl:value-of select="s0:PackQty/text()" />
          </NumberOfPackages>

          <xsl:variable name="packageTypeCode" select="s0:PackType/s0:Code/text()"/>
          <xsl:variable name="packageType" select="CodeMapper:GetRecipientCode($CarrierName , $CarrierName , $combineRecipientMappingNamev1, 'Package Type' , $combineRecipientCodeField, $packageTypeCode)"/>
          <xsl:variable name="mappedPackageType">
            <xsl:choose>
              <xsl:when test="$packageType!=''">
                <xsl:value-of select="$packageType"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="CodeMapper:GetRecipientCode('SHIPPING_INSTRUCTION' , 'SHIPPING_INSTRUCTION' , 'OCM System Configuration' , 'Package Type ISO' , 'Package Type', $packageTypeCode)"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <PackageType>
            <xsl:attribute name="Description">
              <xsl:value-of select="s0:PackType/s0:Description/text()" />
            </xsl:attribute>
            <xsl:choose>
              <xsl:when test="$mappedPackageType!=''">
                <xsl:value-of select="$mappedPackageType" />
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$packageTypeCode" />
              </xsl:otherwise>
            </xsl:choose>
          </PackageType>

          <MeasurementCollection>
            <xsl:call-template name="TypeValuePair">
              <xsl:with-param name="element" select="'Measurement'" />
              <xsl:with-param name="type" select="'GoodsWeight'" />
              <xsl:with-param name="unit" select="s0:WeightUnit/s0:Code/text()" />
              <xsl:with-param name="value" select="StringMapper:FormatDecimal(s0:Weight/text(), '0.000', false())" />
            </xsl:call-template>

            <xsl:call-template name="TypeValuePair">
              <xsl:with-param name="element" select="'Measurement'" />
              <xsl:with-param name="type" select="'GoodsVolume'" />
              <xsl:with-param name="unit" select="s0:VolumeUnit/s0:Code/text()" />
              <xsl:with-param name="value" select="StringMapper:FormatDecimal(s0:Volume/text(), '0.000', false())" />
            </xsl:call-template>
          </MeasurementCollection>

          <ClassificationCollection>
            <xsl:variable name="clearClassificationCodes" select="ListHelper:ClearList()" />
            <xsl:variable name="harmonisedCode" select="userCSharp:StringReplace(s0:HarmonisedCode/text(), '.', '')"/>
            <xsl:if test="$harmonisedCode!=''">
              <xsl:variable name="addCodeToList" select="ListHelper:AddToListIfNotExists($harmonisedCode)" />
              <Classification>
                <Type>HS</Type>
                <Country></Country>
                <Code>
                  <xsl:value-of select="userCSharp:StringReplace($harmonisedCode, '.', '')"/>
                </Code>
              </Classification>
            </xsl:if>

            <xsl:for-each select="s0:ClassificationCollection/s0:Classification">
              <xsl:variable name="classificationCode" select="userCSharp:StringReplace(s0:Code/text(), '.', '')"/>
              <xsl:variable name="needToAddClassificationCode" select="ListHelper:AddToListIfNotExists($classificationCode)"/>

              <xsl:if test="$needToAddClassificationCode">
                <Classification>
                  <Type>HS</Type>
                  <Country>
                    <xsl:value-of select="s0:Country/s0:Code/text()"/>
                  </Country>
                  <Code>
                    <xsl:value-of select="$classificationCode"/>
                  </Code>
                </Classification>
              </xsl:if>
            </xsl:for-each>
          </ClassificationCollection>

          <GoodsDescription>
            <xsl:variable name="detailedDescription" select="s0:DetailedDescription/text()" />
            <xsl:choose>
              <xsl:when test="$detailedDescription!=''">
                <xsl:value-of select="$detailedDescription" />
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="s0:GoodsDescription/text()" />
              </xsl:otherwise>
            </xsl:choose>
          </GoodsDescription>
          <MarksAndNumbers>
            <xsl:value-of select="s0:MarksAndNos/text()" />
          </MarksAndNumbers>

          <xsl:variable name="fromUnit" select="s0:LengthUnit/s0:Code/text()"/>
          <xsl:variable name="toUnit">
            <xsl:choose>
              <xsl:when test="contains('FT;YD;IN', $fromUnit)">IN</xsl:when>
              <xsl:otherwise>CM</xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <xsl:variable name="length">
            <xsl:value-of select="StringMapper:FormatDecimal(UnitConverter:Convert(s0:Length/text(), $fromUnit, $toUnit), '0.000', true())"/>
          </xsl:variable>
          <xsl:variable name="width">
            <xsl:value-of select="StringMapper:FormatDecimal(UnitConverter:Convert(s0:Width/text(), $fromUnit, $toUnit), '0.000', true())"/>
          </xsl:variable>
          <xsl:variable name="height">
            <xsl:value-of select="StringMapper:FormatDecimal(UnitConverter:Convert(s0:Height/text(), $fromUnit, $toUnit), '0.000', true())"/>
          </xsl:variable>
          <xsl:if test="number($length) > 0 or number($width) > 0 or number($height) > 0">
            <Dimensions>
              <Unit>
                <xsl:value-of select="$toUnit"/>
              </Unit>
              <xsl:if test="number($length) > 0">
                <Length>
                  <xsl:value-of select="$length"/>
                </Length>
              </xsl:if>
              <xsl:if test="number($width) > 0">
                <Width>
                  <xsl:value-of select="$width"/>
                </Width>
              </xsl:if>
              <xsl:if test="number($height) > 0">
                <Height>
                  <xsl:value-of select="$height"/>
                </Height>
              </xsl:if>
            </Dimensions>
          </xsl:if>

          <xsl:if test="$includeContainerInfo='Y' and s0:ContainerNumber/text()!=''">
            <GoodsPlacementCollection>
              <GoodsPlacement>
                <EquipmentNumber>
                  <xsl:value-of select="s0:ContainerNumber/text()"/>
                </EquipmentNumber>
                <NumberOfPackages>
                  <xsl:value-of select="s0:PackQty/text()"/>
                </NumberOfPackages>

                <MeasurementCollection>
                  <xsl:call-template name="TypeValuePair">
                    <xsl:with-param name="element" select="'Measurement'" />
                    <xsl:with-param name="type" select="'GoodsWeight'" />
                    <xsl:with-param name="unit" select="s0:WeightUnit/s0:Code/text()" />
                    <xsl:with-param name="value" select="s0:Weight/text()" />
                  </xsl:call-template>

                  <xsl:call-template name="TypeValuePair">
                    <xsl:with-param name="element" select="'Measurement'" />
                    <xsl:with-param name="type" select="'GoodsVolume'" />
                    <xsl:with-param name="unit" select="s0:VolumeUnit/s0:Code/text()" />
                    <xsl:with-param name="value" select="s0:Volume/text()" />
                  </xsl:call-template>
                </MeasurementCollection>
              </GoodsPlacement>
            </GoodsPlacementCollection>
          </xsl:if>

          <HazardousGoodsItemCollection>
            <xsl:for-each select="s0:UNDGCollection/s0:UNDG">
              <xsl:call-template name="HazardousGoodsItem">
                <xsl:with-param name="item" select="." />
              </xsl:call-template>
            </xsl:for-each>
          </HazardousGoodsItemCollection>

          <ReferenceNumberCollection>
            <xsl:variable name="firstLegOrder" select="userCSharp:GetFirstLegOrder()"/>
            <xsl:variable name="lastLegOrder" select="userCSharp:GetLastLegOrder()"/>
            <xsl:variable name="firstLegLoadPort" select="/s0:UniversalShipment/s0:Shipment/s0:TransportLegCollection/s0:TransportLeg[s0:LegOrder/text()=$firstLegOrder]/s0:PortOfLoading/s0:Code/text()"/>
            <xsl:variable name="lastLegDischargePort" select="/s0:UniversalShipment/s0:Shipment/s0:TransportLegCollection/s0:TransportLeg[s0:LegOrder/text()=$lastLegOrder]/s0:PortOfDischarge/s0:Code/text()"/>
            <xsl:variable name="ITNNumber" select="s0:ExportReferenceNumber/text()"/>
            <xsl:if test="substring($firstLegLoadPort, 1, 2)='US' and substring($lastLegDischargePort, 1, 2)!='US' and $ITNNumber!=''">
              <xsl:call-template name="TypeValuePair">
                <xsl:with-param name="element" select="'ReferenceNumber'" />
                <xsl:with-param name="type" select="'ITN'" />
                <xsl:with-param name="value" select="$ITNNumber" />
              </xsl:call-template>
            </xsl:if>
          </ReferenceNumberCollection>
        </GoodsLine>
      </xsl:for-each>
    </GoodsLineCollection>
  </xsl:template>

  <xsl:template name="HazardousGoodsItem">
    <xsl:param name="item" />

    <HazardousGoodsItem xmlns="http://www.cargowise.com/Schemas/OCM/BookingRequest/1">
      <IMOCode>
        <xsl:value-of select="$item/s0:IMOClass/text()"/>
      </IMOCode>
      <PageNumber>
        <xsl:value-of select="$item/s0:SubLabel1/text()"/>
      </PageNumber>
      <UNDGNumber>
        <xsl:value-of select="$item/s0:UNDGCode/text()"/>
      </UNDGNumber>

      <xsl:variable name="packingGroup" select="$item/s0:PackingGroup/text()"/>
      <PackingGroupCode>
        <xsl:choose>
          <xsl:when test="$packingGroup='I'">1</xsl:when>
          <xsl:when test="$packingGroup='II'">2</xsl:when>
          <xsl:when test="$packingGroup='III'">3</xsl:when>
          <xsl:otherwise></xsl:otherwise>
        </xsl:choose>
      </PackingGroupCode>

      <xsl:variable name="flashPoint" select="normalize-space($item/s0:FlashPoint/text())"/>
      <xsl:if test="$flashPoint!='' and number($flashPoint)>0">
        <FlashPoint>
          <Unit>CEL</Unit>
          <Value>
            <xsl:value-of select="$flashPoint" />
          </Value>
        </FlashPoint>
      </xsl:if>

      <ProperShippingName>
        <xsl:value-of select="normalize-space($item/s0:ProperShippingName/text())"/>
      </ProperShippingName>
      <TechnicalName>
        <xsl:value-of select="normalize-space($item/s0:TechicalName/text())"/>
      </TechnicalName>

      <xsl:variable name="marinePolutantCode" select="normalize-space($item/s0:MarinePollutant/s0:Code/text())"/>
      <MarinePollutantClassification>
        <xsl:choose>
          <xsl:when test="$marinePolutantCode='Y'">MarinePollutant</xsl:when>
          <xsl:when test="$marinePolutantCode='S'">SevereMarinePollutant</xsl:when>
          <xsl:otherwise>NonMarinePollutant</xsl:otherwise>
        </xsl:choose>
      </MarinePollutantClassification>

      <EmergencyContact>
        <Name>
          <xsl:value-of select="$item/s0:Contact/s0:FullName/text()"/>
        </Name>
        <Phone>
          <xsl:value-of select="$item/s0:Contact/s0:Phone/text()"/>
        </Phone>
      </EmergencyContact>
    </HazardousGoodsItem>
  </xsl:template>

  <xsl:template name="ReferenceTypeValuePair">
    <xsl:param name="type" />
    <xsl:param name="value" />

    <xsl:if test="$value != ''">
      <xsl:call-template name="TypeValuePair">
        <xsl:with-param name="element" select="'ReferenceNumber'" />
        <xsl:with-param name="type" select="$type" />
        <xsl:with-param name="value" select="$value" />
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name="RemarkTypeValuePair">
    <xsl:param name="type" />
    <xsl:param name="value" />

    <xsl:if test="$value != ''">
      <xsl:call-template name="TypeValuePair">
        <xsl:with-param name="element" select="'Remark'" />
        <xsl:with-param name="type" select="$type" />
        <xsl:with-param name="value" select="$value" />
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name="DateTimeTypeValuePair">
    <xsl:param name="type" />
    <xsl:param name="value" />
    <xsl:param name="reqFormattedDate" />

    <xsl:variable name="dateValue">
      <xsl:choose>
        <xsl:when test="$reqFormattedDate='Y'">
          <xsl:variable name="date" select="DateMapper:FormatXmlDateTime($value, 'yyyy-MM-dd')"/>
          <xsl:variable name="time" select="DateMapper:FormatXmlDateTime($value, 'HH:mm')"/>

          <xsl:choose>
            <xsl:when test="$time!='' and $time!='00:00'">
              <xsl:value-of select="concat($date, 'T', $time)"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$date"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$value"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:if test="$dateValue != ''">
      <xsl:call-template name="TypeValuePair">
        <xsl:with-param name="element" select="'DateTime'" />
        <xsl:with-param name="type" select="$type" />
        <xsl:with-param name="value" select="$dateValue" />
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name="GetOrgRegistrationNumber">
    <xsl:param name="org"/>
    <xsl:param name="orgType"/>
    <xsl:param name="countryCode"/>

    <xsl:variable name="portOfLoadingCountryCode" select="substring(/s0:UniversalShipment/s0:Shipment/s0:PortOfLoading/s0:Code/text(), 1, 2)"/>
    <xsl:variable name="portOfDischargeCountryCode" select="substring(/s0:UniversalShipment/s0:Shipment/s0:PortOfDischarge/s0:Code/text(), 1, 2)"/>

    <xsl:variable name="requestedCountry1">
      <xsl:choose>
        <xsl:when test="$orgType='ConsignorDocumentaryAddress'">
          <xsl:value-of select="$portOfLoadingCountryCode"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$portOfDischargeCountryCode"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="requestedCountry2">
      <xsl:choose>
        <xsl:when test="$orgType = 'ConsignorDocumentaryAddress' ">
          <xsl:value-of select="$portOfDischargeCountryCode"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$portOfLoadingCountryCode"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="resetPriorityCounter1" select="userCSharp:ResetPriorityCounter($requestedCountry1)"/>
    <xsl:variable name="requestedCountry1TaxRegPriority">
      <xsl:call-template name="GetTaxRegistrationPriority">
        <xsl:with-param name="RequestedCountry" select="$requestedCountry1"/>
        <xsl:with-param name="CountryOfIssue" select="$countryCode"/>
        <xsl:with-param name="registrationNumbers" select="$org/s0:RegistrationNumberCollection"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="taxRegistrationPriority">
      <xsl:choose>
        <xsl:when test="$requestedCountry1TaxRegPriority!=''">
          <xsl:value-of select="$requestedCountry1TaxRegPriority"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:variable name="resetPriorityCounter2" select="userCSharp:ResetPriorityCounter($requestedCountry2)"/>
          <xsl:call-template name="GetTaxRegistrationPriority">
            <xsl:with-param name="RequestedCountry" select="$requestedCountry2"/>
            <xsl:with-param name="CountryOfIssue" select="$countryCode"/>
            <xsl:with-param name="registrationNumbers" select="$org/s0:RegistrationNumberCollection"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="requestedCountry" select="userCSharp:GetRequestedCountryCode()"/>
    <xsl:variable name="regType">
      <xsl:choose>
        <xsl:when test="$taxRegistrationPriority!=''">
          <xsl:value-of select="CodeMapper:GetRecipientCode('SHIPPING_INSTRUCTION' , 'SHIPPING_INSTRUCTION' , 'OCM System Configuration' , 'Gov Reference Number' , 'Registration Type', $requestedCountry, $countryCode, $taxRegistrationPriority)" />
        </xsl:when>
        <xsl:otherwise></xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="UseLongReference" select="CodeMapper:GetRecipientCode('SHIPPING_INSTRUCTION' , 'SHIPPING_INSTRUCTION' , 'OCM System Configuration' , 'Reference Label Type', 'Use Long Reference', $RecipientID)" />
    <xsl:variable name="referenceLabelName">
      <xsl:choose>
        <xsl:when test="$UseLongReference='true'">Reference Label Long</xsl:when>
        <xsl:otherwise>Reference Label</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="referenceLabel">
      <xsl:choose>
        <xsl:when test="$taxRegistrationPriority!=''">
          <xsl:value-of select="CodeMapper:GetRecipientCode('SHIPPING_INSTRUCTION' , 'SHIPPING_INSTRUCTION' , 'OCM System Configuration' , 'Gov Reference Number', $referenceLabelName, $requestedCountry, $countryCode, $taxRegistrationPriority)" />
        </xsl:when>
        <xsl:otherwise></xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="companyRegNumber" select="$org/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text()=$regType and s0:CountryOfIssue/s0:Code/text()=$countryCode]/s0:Value/text()" />
    <xsl:choose>
      <xsl:when test="$companyRegNumber!=''">
        <xsl:value-of select="concat($referenceLabel,'+',$companyRegNumber)"/>
      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="GetTaxRegistrationPriority">
    <xsl:param name="RequestedCountry"/>
    <xsl:param name="CountryOfIssue"/>
    <xsl:param name="registrationNumbers"/>

    <xsl:variable name="priority" select="userCSharp:GetPriorityCounter()"/>
    <xsl:variable name="regType" select="CodeMapper:GetRecipientCode('SHIPPING_INSTRUCTION' , 'SHIPPING_INSTRUCTION' , 'OCM System Configuration', 'Gov Reference Number', 'Registration Type', $RequestedCountry, $CountryOfIssue, $priority)" />
    <xsl:if test="$regType">
      <xsl:variable name ="regNumber" select="$registrationNumbers/s0:RegistrationNumber[s0:Type/s0:Code/text()=$regType and s0:CountryOfIssue/s0:Code/text()=$CountryOfIssue]/s0:Value/text()"/>
      <xsl:choose>
        <xsl:when test="$regNumber">
          <xsl:value-of select="$priority"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="GetTaxRegistrationPriority">
            <xsl:with-param name="RequestedCountry" select="$RequestedCountry"/>
            <xsl:with-param name="CountryOfIssue" select="$CountryOfIssue"/>
            <xsl:with-param name="registrationNumbers" select="$registrationNumbers"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>
  </xsl:template>

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[
public string ConvertDateToString(string dateValue)
{
string[] formats= {"M/d/yyyy h:mm:ss tt", "M/d/yyyy h:mm tt",
                   "MM/dd/yyyy hh:mm:ss", "M/d/yyyy h:mm:ss",
                   "M/d/yyyy hh:mm tt", "M/d/yyyy hh tt",
                   "M/d/yyyy h:mm", "M/d/yyyy h:mm",
                   "MM/dd/yyyy hh:mm", "M/dd/yyyy hh:mm",
                   "MM/d/yyyy HH:mm:ss.ffffff",
                   "yyyy-mm-dd"};

  if (!string.IsNullOrEmpty(dateValue))
  {
    DateTime outputDate;
    if (DateTime.TryParseExact(dateValue, formats, null, System.Globalization.DateTimeStyles.None, out outputDate))
    {
      return outputDate.ToString("s");
    }
    else
    {
      return dateValue.Substring(0, 10);
    }
  }
  return string.Empty;
}

public string StringReplace(string text, string oldValue, string newValue)
{
  if (text != null && text.Length > 0)
  {
    return text.Replace(oldValue, newValue);
  }
  return "";
}

public bool MatchPattern(string text, string format)
{
  return Regex.IsMatch(text, format);
}

public string FindFirstAndLastLegOrder(string legOrder)
{
  var number = int.Parse(legOrder);

  if (!firstLegOrder.HasValue || number < firstLegOrder.Value)
  {
    firstLegOrder = number;
  }

  if (!lastLegOrder.HasValue || number > lastLegOrder.Value)
  {
    lastLegOrder = number;
  }

  return string.Empty;
}

public int GetFirstLegOrder()
{
  return firstLegOrder.Value;
}
public int? firstLegOrder;

public int GetLastLegOrder()
{
  return lastLegOrder.Value;
}
public int? lastLegOrder;

int priorityCounter = 1;
public int GetPriorityCounter()
{
  return priorityCounter++;
}

public int ResetPriorityCounter(string countryCode)
{
  if (countryCode != null)
  {
    requestedCountryCode = countryCode;
  }
  return priorityCounter = 1;
}

string requestedCountryCode;
public string GetRequestedCountryCode()
{
  return requestedCountryCode;
}

public string MultiplyDouble(string valueText1, string valueText2, string format)
{
  Double value1;
  Double value2;
  if (!Double.TryParse(valueText1, out value1)) return string.Empty;
  if (!Double.TryParse(valueText2, out value2)) return string.Empty;
  return (value1 * value2).ToString(format);
}
]]>
  </msxsl:script>
</xsl:stylesheet>

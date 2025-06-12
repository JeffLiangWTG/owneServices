<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 CodeMapper ContextAccessor DataModelAccessor DateMapper" version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2012/11/CargoReceiptAdvice/1"
                xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2012/11"
                xmlns:ns1="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
                xmlns:DataModelAccessor="http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"
                xmlns:DateMapper="http://schemas.microsoft.com/BizTalk/2003/DateMapper">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes"/>

  <xsl:variable name="SenderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="RecipientID" select="ContextAccessor:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>

  <xsl:variable name="shipment" select="/s0:UniversalShipment/s0:Shipment"/>
  <xsl:variable name="shipmentNumber" select="$shipment/s0:DataContext/s0:DataSource/s0:Key/text()"/>
  <xsl:variable name="forwardingType" select="$shipment/s0:DataContext/s0:DataSource/s0:Type/text()"/>
  <xsl:variable name="shipmentType" select="$shipment/s0:ShipmentType/text()"/>

  <xsl:variable name="HIRReference" select="$shipment/s0:AdditionalReferenceCollection/s0:AdditionalReference[s0:Type/text() = 'HIR']/s0:ReferenceNumber/text()"/>
  <xsl:variable name="eHubCargowiseClientID" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedClients', '', '@senderId', $SenderID, '@ST_ID', 'CW1MSG', '@value', $HIRReference)" />
  <xsl:variable name="SetRecipientID" select="ContextAccessor:SetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties', $eHubCargowiseClientID)"/>

  <xsl:variable name="eHubPartyTypeFlag">
    <xsl:choose>
      <xsl:when test="$eHubCargowiseClientID != ''">
        <xsl:value-of select="DataModelAccessor:GetClientRegistrationFlag1AsString($eHubCargowiseClientID, '', 'CARGOWISE')"/>
      </xsl:when>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="eHubPartyType">
    <xsl:choose>
      <xsl:when test="$eHubPartyTypeFlag='1'">NVOCC</xsl:when>
      <xsl:otherwise>ShippingLine</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:template match="s0:*">
    <xsl:element name="ns0:{local-name()}">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="@*">
    <xsl:copy>
      <xsl:apply-templates select="@*"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="/s0:UniversalShipment">
    <xsl:choose>
      <xsl:when test="$eHubCargowiseClientID = ''">
        <xsl:call-template name="createIRJEvent"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="createUniversalInterchange"/>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <xsl:template name="createIRJEvent">
    <xsl:variable name="SetDestinationParty" select="ContextAccessor:SetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties', $SenderID)"/>
    <xsl:variable name="SetSourceParty" select="ContextAccessor:SetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties', $RecipientID)"/>

    <ns1:UniversalInterchange>
      <ns1:Body>
        <ns0:UniversalEvent>
          <ns0:Event>
            <ns0:DataContext>
              <ns0:DocumentaryOverride>
                <ns0:DocumentName>Cargo Receipt Advice</ns0:DocumentName>
              </ns0:DocumentaryOverride>
              <ns0:DataTargetCollection>
                <ns0:DataTarget>
                  <ns0:Key>
                    <xsl:value-of select="s0:Shipment/s0:DataContext/s0:DataSource/s0:Key/text()"/>
                  </ns0:Key>
                  <ns0:Type>
                    <xsl:value-of select="s0:Shipment/s0:DataContext/s0:DataSource/s0:Type/text()"/>
                  </ns0:Type>
                </ns0:DataTarget>
              </ns0:DataTargetCollection>
            </ns0:DataContext>
            <ns0:EventTime>
              <xsl:value-of select="DateMapper:CurrentDateTimeUTC('yyyy-MM-ddTHH:mm:ss.fff')"/>
            </ns0:EventTime>
            <ns0:EventType>IRJ</ns0:EventType>
            <ns0:EventParameters>
              <ns0:Department>CargoWise</ns0:Department>
              <ns0:Reason>
                <xsl:value-of select="concat('Could not found matching PartyReceiverID in the Client/Subscription Lookup.(Client Registration: CARGOWISE, Input: ', $HIRReference, ')')"/>
              </ns0:Reason>
              <ns0:MessageType>Cargo Receipt Advice</ns0:MessageType>
            </ns0:EventParameters>
            <ns0:ContextCollection>
              <ns0:Context>
                <ns0:Type>MessageReference</ns0:Type>
                <ns0:value>
                  <xsl:value-of select="$shipmentNumber"/>
                </ns0:value>
              </ns0:Context>
            </ns0:ContextCollection>
          </ns0:Event>
        </ns0:UniversalEvent>
      </ns1:Body>
    </ns1:UniversalInterchange>
  </xsl:template>

  <xsl:template name="createUniversalInterchange">
    <xsl:variable name="InterchangeNum" select="CodeMapper:CallActionProcedureHelper('GetCounterValue','@value','@name','CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CARGOWISE2CARGOWISE','@maxlength','14')" />
    <xsl:variable name="previousConsolReference" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $eHubCargowiseClientID, '@recipientId', $SenderID , '@ST_ID', 'CW1MSG', '@value', $shipmentNumber, '@referenceType', 'JobNumber')" />
    <xsl:variable name="InboxPK" select="ContextAccessor:GetContextProperty('InternalTrackingID', 'http://cargowise.com/ehub/tracking/2010/06')"/>
    <xsl:variable name="SubscribeInboxPK" select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $InboxPK, $InterchangeNum)" />
    <xsl:variable name="SubscriberShipmentReference">
      <xsl:choose>
        <xsl:when test="$previousConsolReference!=''">
          <xsl:value-of select="$previousConsolReference" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:variable name="formattedCounter" select='format-number($InterchangeNum, "0000000000")' />
          <xsl:variable name="newShipmentReference" select="concat('CRA', $formattedCounter)"/>
          <xsl:variable name="SubScriberValue1" select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $newShipmentReference, $shipmentNumber, 'JobNumber')" />
          <xsl:variable name="SubScriberValue2" select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $shipmentNumber, $newShipmentReference, 'JobNumber')" />
          <xsl:value-of select="$newShipmentReference" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="SubscribeInterchangeNum" select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $InterchangeNum, $SubscriberShipmentReference, 'InterchangeNumber')" />
    <xsl:variable name="purposeCode" select="$shipment/s0:DataContext/s0:DocumentaryOverride/s0:Purpose/text()"/>
    <xsl:variable name="SubscribePurpose" select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $SubscriberShipmentReference, $purposeCode, 'ActionPurpose')" />
    <xsl:variable name="subscribeShipmentType" select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $SubscriberShipmentReference, $shipmentType, 'ShipmentType')" />
    <xsl:variable name="SubscribeDocumentName" select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $SubscriberShipmentReference, 'Cargo Receipt Advice', 'DocumentName')" />
    <xsl:variable name="SubscribeForwardingType" select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $SubscriberShipmentReference, $forwardingType, 'ForwardingType')" />

    <xsl:variable name="previousPartyType" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $eHubCargowiseClientID, '@recipientId', $SenderID , '@ST_ID', 'CW1MSG', '@value', $SubscriberShipmentReference, '@referenceType', 'PartyType')" />
    <xsl:variable name="SubscribeNewPartyType">
      <xsl:choose>
        <xsl:when test="$previousPartyType=''">
          <xsl:value-of select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $SubscriberShipmentReference, $eHubPartyType, 'PartyType')" />
        </xsl:when>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="formVersion" select="$shipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='FormVersion']/s0:Value/text()" />
    <xsl:variable name="SubscribeFormVersion">
      <xsl:if test="$formVersion !=''">
        <xsl:value-of select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $SubscriberShipmentReference, $formVersion, 'FormVersion')"/>
      </xsl:if>
    </xsl:variable>

    <ns1:UniversalInterchange>
      <ns1:Header>
        <ns1:SenderID>
          <xsl:value-of select="$eHubCargowiseClientID"/>
        </ns1:SenderID>
        <ns1:RecipientID>
          <xsl:value-of select="'SHIPPING_INSTRUCTION'"/>
        </ns1:RecipientID>
        <ns1:Acknowledgement>
          <ns1:Required>OnAll</ns1:Required>
          <ns1:Channel>eHub</ns1:Channel>
          <ns1:RecipientID>CARGOWISE_AC</ns1:RecipientID>
          <ns1:ContextCollection>
            <ns1:Context>
              <ns1:Type>eHub Interchange Reference</ns1:Type>
              <ns1:Value>
                <xsl:value-of select="$SubscriberShipmentReference"/>
              </ns1:Value>
            </ns1:Context>
            <ns1:Context>
              <ns1:Type>MessageReference</ns1:Type>
              <ns1:Value>
                <xsl:value-of select="$shipmentNumber"/>
              </ns1:Value>
            </ns1:Context>
            <ns1:Context>
              <ns1:Type>DocumentName</ns1:Type>
              <ns1:Value>Cargo Receipt Advice</ns1:Value>
            </ns1:Context>
          </ns1:ContextCollection>
        </ns1:Acknowledgement>
      </ns1:Header>
      <ns1:Body>
        <ns0:UniversalShipment xmlns="http://www.cargowise.com/Schemas/Universal/2012/11">
          <xsl:apply-templates/>
        </ns0:UniversalShipment>
      </ns1:Body>
    </ns1:UniversalInterchange>
  </xsl:template>

  <xsl:template match="s0:DataSource|s0:Workflow|s0:AdditionalReferenceCollection|s0:OrganizationAddressCollection"/>

  <xsl:template match="s0:DataContext">
    <xsl:variable name="key" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', $eHubCargowiseClientID, '@ST_ID', 'CW1MSG', '@value', $HIRReference, '@referenceType', 'JobNumber')"/>

    <xsl:element name="ns0:{local-name()}">
      <ns0:DataTargetCollection>
        <ns0:DataTarget>
          <ns0:Key>
            <xsl:value-of select="$key"/>
          </ns0:Key>
          <ns0:Type>
            <xsl:value-of select="$forwardingType"/>
          </ns0:Type>
        </ns0:DataTarget>
      </ns0:DataTargetCollection>

      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>
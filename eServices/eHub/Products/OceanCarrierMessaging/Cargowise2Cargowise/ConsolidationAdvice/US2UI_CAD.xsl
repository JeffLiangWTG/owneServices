<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 CodeMapper ContextAccessor DataModelAccessor DateMapper" version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2012/11/ConsolidationAdvice/1"
                xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2012/11"
                xmlns:ns1="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
                xmlns:DataModelAccessor="http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"
                xmlns:DateMapper="http://schemas.microsoft.com/BizTalk/2003/DateMapper">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0"/>

  <xsl:variable name="SenderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="RecipientID" select="ContextAccessor:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>

  <xsl:variable name="shipment" select="/s0:UniversalShipment/s0:Shipment"/>
  <xsl:variable name="consolID" select="$shipment/s0:DataContext/s0:DataSource/s0:Key/text()"/>
  <xsl:variable name="forwardingType" select="$shipment/s0:DataContext/s0:DataSource/s0:Type/text()"/>
  <xsl:variable name="shipmentType" select="$shipment/s0:ShipmentType/text()"/>

  <xsl:variable name="hirValue" select="s0:UniversalShipment/s0:Shipment/s0:SubShipmentCollection/s0:SubShipment/s0:AdditionalReferenceCollection/s0:AdditionalReference[s0:Type/text()='HIR']/s0:ReferenceNumber/text()" />
  <xsl:variable name="eHubCargowiseClientID" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedClients', '', '@senderId', $SenderID, '@ST_ID', 'CW1MSG', '@value', $hirValue)" />
  <xsl:variable name="SetRecipientID" select="ContextAccessor:SetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties', $eHubCargowiseClientID)"/>

  <xsl:variable name="InterchangeNum" select="CodeMapper:CallActionProcedureHelper('GetCounterValue','@value','@name','CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CARGOWISE2CARGOWISE','@maxlength','14')" />
  <xsl:variable name="previousConsolReference" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $eHubCargowiseClientID, '@recipientId', $SenderID , '@ST_ID', 'CW1MSG', '@value', $consolID, '@referenceType', 'JobNumber')" />
  <xsl:variable name="InboxPK" select="ContextAccessor:GetContextProperty('InternalTrackingID', 'http://cargowise.com/ehub/tracking/2010/06')"/>
  <xsl:variable name="SubscriberConsolReference">
    <xsl:choose>
      <xsl:when test="$previousConsolReference!=''">
        <xsl:value-of select="$previousConsolReference" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:variable name="formattedCounter" select='format-number($InterchangeNum, "0000000000")' />
        <xsl:variable name="NewConsolReference" select="concat('CAD', $formattedCounter)"/>
        <xsl:value-of select="$NewConsolReference" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

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
                <ns0:DocumentName>Consolidation Advice</ns0:DocumentName>
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
                <xsl:value-of select="concat('Could not found matching PartyReceiverID in the Client/Subscription Lookup.(Client Registration: CARGOWISE, Input: ', $hirValue, ')')"/>
              </ns0:Reason>
              <ns0:MessageType>Consolidation Advice</ns0:MessageType>
            </ns0:EventParameters>
            <ns0:ContextCollection>
              <ns0:Context>
                <ns0:Type>MessageReference</ns0:Type>
                <ns0:value>
                  <xsl:value-of select="$consolID"/>
                </ns0:value>
              </ns0:Context>
            </ns0:ContextCollection>
          </ns0:Event>
        </ns0:UniversalEvent>
      </ns1:Body>
    </ns1:UniversalInterchange>
  </xsl:template>

  <xsl:template name="createUniversalInterchange">
    <xsl:if test="$previousConsolReference = ''">
      <xsl:variable name="SubScriberValue1" select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $SubscriberConsolReference, $consolID, 'JobNumber')" />
      <xsl:variable name="SubScriberValue2" select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $consolID, $SubscriberConsolReference, 'JobNumber')" />
    </xsl:if>
    <xsl:variable name="SubscribeInboxPK" select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $InboxPK, $InterchangeNum)" />
    <xsl:variable name="SubscribeInterchangeNum" select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $InterchangeNum, $SubscriberConsolReference, 'InterchangeNumber')" />
    <xsl:variable name="purposeCode" select="$shipment/s0:DataContext/s0:DocumentaryOverride/s0:Purpose/text()"/>
    <xsl:variable name="SubscribePurpose" select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $SubscriberConsolReference, $purposeCode, 'ActionPurpose')" />
    <xsl:variable name="subscribeShipmentType" select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $SubscriberConsolReference, $shipmentType, 'ShipmentType')" />
    <xsl:variable name="SubscribeDocumentName" select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $SubscriberConsolReference, 'Consolidation Advice', 'DocumentName')" />
    <xsl:variable name="SubscribeForwardingType" select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $SubscriberConsolReference, $forwardingType, 'ForwardingType')" />

    <xsl:variable name="previousPartyType" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $eHubCargowiseClientID, '@recipientId', $SenderID , '@ST_ID', 'CW1MSG', '@value', $SubscriberConsolReference, '@referenceType', 'PartyType')" />
    <xsl:variable name="SubscribeNewPartyType">
      <xsl:choose>
        <xsl:when test="$previousPartyType=''">
          <xsl:value-of select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $SubscriberConsolReference, $eHubPartyType, 'PartyType')" />
        </xsl:when>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="formVersion" select="$shipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='FormVersion']/s0:Value/text()" />
    <xsl:variable name="SubscribeFormVersion">
      <xsl:if test="$formVersion !=''">
        <xsl:value-of select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $SubscriberConsolReference, $formVersion, 'FormVersion')"/>
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
                <xsl:value-of select="$SubscriberConsolReference"/>
              </ns1:Value>
            </ns1:Context>
            <ns1:Context>
              <ns1:Type>MessageReference</ns1:Type>
              <ns1:Value>
                <xsl:value-of select="$consolID"/>
              </ns1:Value>
            </ns1:Context>
            <ns1:Context>
              <ns1:Type>DocumentName</ns1:Type>
              <ns1:Value>Consolidation Advice</ns1:Value>
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

  <!--Start : Elements removed-->
  <xsl:template match ="s0:DataContext/s0:DataSource" />
  <xsl:template match ="s0:Workflow" />
  <xsl:template match ="s0:RegistrationNumberCollection" />
  <xsl:template match ="s0:AdditionalReference[s0:Type/text()='FFW']" />
  <xsl:template match ="s0:Carrier"/>
  <xsl:template match ="s0:OrganizationAddress/s0:AddressType"/>
  <xsl:template match ="s0:OrganizationAddress/s0:GovRegNum"/>
  <xsl:template match ="s0:OrganizationAddress/s0:GovRegNumType"/>
  <xsl:template match ="s0:AdditionalReferenceCollection" />

  <xsl:template match ="s0:Shipment/s0:OrganizationAddress/s0:AddressType" />

  <xsl:template match ="s0:SubShipment/s0:DataContext"/>
  <xsl:template match ="s0:SubShipment/s0:AgentsReference "/>
  <xsl:template match ="s0:SubShipment/s0:PortOfDestination "/>
  <xsl:template match ="s0:SubShipment/s0:PortOfOrigin "/>
  <!--End : Elements removed-->

  <xsl:template match="s0:Shipment/s0:DataContext">
    <ns0:DataContext>
      <xsl:apply-templates select="@*"/>
      <ns0:DataTargetCollection>
        <ns0:DataTarget>
          <ns0:Type>ForwardingConsol</ns0:Type>
        </ns0:DataTarget>
      </ns0:DataTargetCollection>
      <xsl:apply-templates select="*"/>
    </ns0:DataContext>
  </xsl:template>

  <xsl:template match="s0:SubShipment">
    <xsl:variable name ="agentsRef" select="s0:AgentsReference/text()"/>
    <xsl:variable name="additionalRefHIR" select="s0:AdditionalReferenceCollection/s0:AdditionalReference[s0:Type='HIR']/s0:ReferenceNumber/text()"/>
    <xsl:variable name="previousRef" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', $eHubCargowiseClientID, '@ST_ID', 'CW1MSG', '@value', $additionalRefHIR, '@referenceType', 'JobNumber')"/>

    <ns0:SubShipment>
      <xsl:apply-templates select="@*"/>
      <xsl:for-each select="s0:DataContext">
        <ns0:DataContext>
          <xsl:apply-templates select="@*"/>
          <ns0:DataTargetCollection>
            <ns0:DataTarget>
              <ns0:Key>
                <xsl:choose>
                  <xsl:when test="$previousRef!=''">
                    <xsl:value-of select="$previousRef"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="$agentsRef"/>
                  </xsl:otherwise>
                </xsl:choose>
              </ns0:Key>
              <ns0:Type>ForwardingShipment</ns0:Type>
            </ns0:DataTarget>
          </ns0:DataTargetCollection>
          <xsl:apply-templates select="*"/>
        </ns0:DataContext>
      </xsl:for-each>
      <xsl:apply-templates select="*"/>
    </ns0:SubShipment>
  </xsl:template>

  <xsl:template match="s0:Shipment/s0:OrganizationAddressCollection">
    <xsl:choose>
      <xsl:when test="/s0:UniversalShipment/s0:Shipment/s0:AdditionalReferenceCollection">
        <ns0:AdditionalReferenceCollection Content="Partial">
          <xsl:for-each select="/s0:UniversalShipment/s0:Shipment/s0:AdditionalReferenceCollection/s0:AdditionalReference">
            <xsl:call-template name="CreateAdditionalReference">
              <xsl:with-param name="type" select="./s0:Type/text()" />
              <xsl:with-param name="description" select="./s0:Type/@Description" />
              <xsl:with-param name="referenceNumber" select="./s0:ReferenceNumber/text()" />
            </xsl:call-template>
          </xsl:for-each>
          <xsl:call-template name="CreateAdditionalReference">
            <xsl:with-param name="type" select="'HIR'" />
            <xsl:with-param name="description" select="'eHub Interchange Reference'" />
            <xsl:with-param name="referenceNumber" select="$SubscriberConsolReference" />
          </xsl:call-template>
        </ns0:AdditionalReferenceCollection>
      </xsl:when>
      <xsl:otherwise>
        <ns0:AdditionalReferenceCollection Content="Partial">
          <xsl:call-template name="CreateAdditionalReference">
            <xsl:with-param name="type" select="'HIR'" />
            <xsl:with-param name="description" select="'eHub Interchange Reference'" />
            <xsl:with-param name="referenceNumber" select="$SubscriberConsolReference" />
          </xsl:call-template>
        </ns0:AdditionalReferenceCollection>
      </xsl:otherwise>
    </xsl:choose>

    <ns0:OrganizationAddressCollection>
      <xsl:apply-templates select="@*|*"/>
    </ns0:OrganizationAddressCollection>
  </xsl:template>

  <xsl:template name="CreateAdditionalReference">
    <xsl:param name="type"/>
    <xsl:param name="description"/>
    <xsl:param name="referenceNumber"/>

    <xsl:if test="$referenceNumber!=''">
      <ns0:AdditionalReference>
        <ns0:Type>
          <xsl:attribute name="Description">
            <xsl:value-of select="$description"/>
          </xsl:attribute>
          <xsl:value-of select="$type"/>
        </ns0:Type>
        <ns0:ReferenceNumber>
          <xsl:value-of select="$referenceNumber"/>
        </ns0:ReferenceNumber>
      </ns0:AdditionalReference>
    </xsl:if>
  </xsl:template>

  <xsl:template match="s0:Shipment/s0:OrganizationAddressCollection/s0:OrganizationAddress">
    <xsl:call-template name="CreateOrganizationAddress">
      <xsl:with-param name ="parentNode" select="'Shipment'"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="s0:SubShipment/s0:OrganizationAddressCollection/s0:OrganizationAddress">
    <xsl:call-template name="CreateOrganizationAddress">
      <xsl:with-param name ="parentNode" select="'SubShipment'"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="CreateOrganizationAddress">
    <xsl:param name="parentNode"/>

    <xsl:variable name="orgAddressType" select="s0:AddressType/text()" />
    <xsl:variable name="addressType">
      <xsl:choose>
        <xsl:when test="$orgAddressType='CurrentUser'">CoLoadWith</xsl:when>
        <xsl:when test="$orgAddressType='SendingForwarderAddress'"></xsl:when>
        <xsl:when test="$orgAddressType='BookingPartyDocumentaryAddress' and $parentNode='SubShipment'"></xsl:when>
        <xsl:when test="$orgAddressType='BookingPartyDocumentaryAddress'">SendingForwarderAddress</xsl:when>
        <xsl:when test="$orgAddressType='ReceivingForwarderAddress' and $parentNode='Shipment'"></xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$orgAddressType"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:if test="$addressType!=''">
      <ns0:OrganizationAddress>
        <xsl:apply-templates select="@*"/>
        <ns0:AddressType>
          <xsl:value-of select="$addressType"/>
        </ns0:AddressType>
        <xsl:apply-templates select="*"/>
      </ns0:OrganizationAddress>
    </xsl:if>
  </xsl:template>
</xsl:stylesheet>

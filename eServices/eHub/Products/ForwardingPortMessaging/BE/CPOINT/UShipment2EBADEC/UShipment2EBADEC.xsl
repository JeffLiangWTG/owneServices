<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 CodeMapper ContextAccessor DataModelAccessor DateMapper"
                version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2012/11/NotificationOfExportConsignment/1"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
                xmlns:DataModelAccessor="http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"
                xmlns:DateMapper="http://schemas.microsoft.com/BizTalk/2003/DateMapper">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes"/>

  <xsl:key name="groupByContainer" use="s0:ContainerNumber" match="s0:UniversalShipment/s0:Shipment/s0:SubShipmentCollection/s0:SubShipment[s0:DataContext/s0:DataSource/s0:Type/text() = 'MRN']/s0:PackingLineCollection/s0:PackingLine"/>
  <xsl:key name="groupByVIN" use="s0:ReferenceNumber" match="s0:UniversalShipment/s0:Shipment/s0:SubShipmentCollection/s0:SubShipment[s0:DataContext/s0:DataSource/s0:Type/text() = 'MRN']/s0:PackingLineCollection/s0:PackingLine"/>
  <xsl:key name="groupByMRN" use="concat(@MRN, @Key)" match="MRN"/>

  <xsl:variable name="recipientID" select="ContextAccessor:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="senderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="serviceProvider" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE', 'FORWARDING_PORT_MESSAGE', 'FPM System Configuration', 'Port Settings', 'Name', $recipientID)"/>

  <xsl:template match="/">
    <xsl:apply-templates select="s0:UniversalShipment/s0:Shipment"/>
  </xsl:template>

  <xsl:template match="s0:UniversalShipment/s0:Shipment">
    <xsl:variable name="fileSenderID" select="DataModelAccessor:GetClientRegistrationCode($senderID, s0:DataContext/s0:Workflow/s0:EventBranch/text(), $serviceProvider)"/>
    <xsl:variable name="operationPortCode" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'OperationalPort_Code']/s0:Value/text()"/>
    <xsl:variable name="consolID" select="s0:DataContext/s0:DataSource/s0:Key/text()"/>
    <xsl:variable name="purpose" select="s0:DataContext/s0:DocumentaryOverride/s0:Purpose/text()"/>
    <xsl:variable name="documentName" select="s0:DataContext/s0:DocumentaryOverride/s0:DocumentName/text()"/>

    <xsl:variable name="interchangeID" select="CodeMapper:CallActionProcedureHelper('GetCounterValue','@value','@name','CargoWise.eHub.Products.ForwardingPortMessaging.BE.CPOINT.Interchange','@maxlength','50')"/>

    <xsl:variable name="serviceProviderMSGID" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE', 'FORWARDING_PORT_MESSAGE', 'FPM System Configuration', 'Port Settings', 'MSGID', $recipientID)"/>
    <xsl:variable name="serviceProviderID" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE', 'FORWARDING_PORT_MESSAGE', 'FPM System Configuration', 'Port Settings', 'ID', $recipientID)"/>

    <xsl:variable name="previousMessageIdentifier" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $serviceProvider, '@recipientId', $senderID , '@ST_ID',  $serviceProviderMSGID , '@value', $consolID, '@referenceType', 'EBADEC')"/>
    <xsl:variable name="messageIdentifier">
      <xsl:choose>
        <xsl:when  test="$previousMessageIdentifier != '' and $purpose = 'WTH'">
          <xsl:value-of select="$previousMessageIdentifier"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:variable name="msgPrefix"  select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE', 'FORWARDING_PORT_MESSAGE', 'FPM System Configuration', 'Port Settings', 'SubscriptionPrefix', $recipientID)"/>
          <xsl:variable name="messageSetID" select="CodeMapper:CallActionProcedureHelper('GetCounterValue','@value','@name','CargoWise.eHub.Products.ForwardingPortMessaging.BE.CPOINT','@maxlength','50')"/>
          <xsl:variable name="formattedMessageID" select ="concat($msgPrefix, format-number($messageSetID, '0000000000'))"/>
          <xsl:variable name="subscribeJobNumber1" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $formattedMessageID, $consolID, 'EBADEC')"/>
          <xsl:variable name="subscribeJobNumber2" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $consolID, $formattedMessageID, 'EBADEC')"/>
          <xsl:value-of select="$formattedMessageID"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="subscribeForwardingType" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageIdentifier, s0:DataContext/s0:DataSource/s0:Type/text(), 'ForwardingType')"/>
    <xsl:variable name="subscribeActionPurpose" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageIdentifier, $purpose, 'Purpose')"/>
    <xsl:variable name="subscribeDocumentName" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageIdentifier, $documentName, 'DocumentName')"/>
    <xsl:variable name="subscribeClientID" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderID, $serviceProvider, $senderID, $fileSenderID)"/>
    <xsl:variable name="subscribeOperationPort" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageIdentifier, $operationPortCode, 'OperationPort')" />

    <xsl:variable name="InboxPK" select="ContextAccessor:GetContextProperty('InternalTrackingID', 'http://cargowise.com/ehub/tracking/2010/06')"/>
    <xsl:variable name="SubscribeInboxPK" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $InboxPK, $interchangeID)"/>
    <xsl:variable name="FileName" select="ContextAccessor:SetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06', concat('EBADEC_', $senderID, '_', $interchangeID))"/>
    <xsl:variable name="EmailSubject" select="ContextAccessor:SetContextProperty('OverrideEmailSubject', 'http://cargowise.com/ehub/processing/2010/06', concat('FPM_', $serviceProvider, '_', $operationPortCode))"/>

    <xsl:variable name="orgCurrentUserRegNumberNode" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text() = 'CurrentUser']/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text() = 'DUN' or s0:Type/text() = 'EOR' or s0:Type/text() = 'PSN']"/>
    <xsl:variable name="orgCurrentuserRegNumberType" select="$orgCurrentUserRegNumberNode/s0:Type/text()"/>
    <xsl:variable name="codeType">
      <xsl:choose>
        <xsl:when test="$orgCurrentuserRegNumberType = 'DUN'">DUNS</xsl:when>
        <xsl:when test="$orgCurrentuserRegNumberType = 'EOR'">EORI</xsl:when>
        <xsl:when test="$orgCurrentuserRegNumberType = 'PSN'">APCS</xsl:when>
        <xsl:otherwise></xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="orgCurrentuserRegNumberValue">
        <xsl:choose>
            <xsl:when test="$orgCurrentuserRegNumberType = 'EOR'">
                <xsl:value-of select="concat($orgCurrentUserRegNumberNode/s0:CountryOfIssue/text(), $orgCurrentUserRegNumberNode/s0:Value/text())"/>
            </xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="$orgCurrentUserRegNumberNode/s0:Value/text()"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:variable>

    <EBADEC xmlns="urn:EBA" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:schemaLocation="urn:EBA" VersionMajor="1" VersionMinor="0">
      <Header>
        <Sender>
          <xsl:attribute name="codeType">
            <xsl:choose>
              <xsl:when test="$codeType!=''">
                <xsl:value-of select="$codeType"/>
              </xsl:when>
              <xsl:otherwise></xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
          <xsl:value-of select="$orgCurrentuserRegNumberValue"/>
        </Sender>
        <Receiver codeType="DUNS">
          <xsl:value-of select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE', 'FORWARDING_PORT_MESSAGE', 'FPM System Configuration', 'Service Provider Settings', 'RecipientID', $serviceProvider, $operationPortCode, '')"/>
        </Receiver>
        <MessageNo>
          <xsl:value-of select="$messageIdentifier"/>
        </MessageNo>
        <xsl:if test="$previousMessageIdentifier!='' and ($purpose='AMD' or $purpose='WTH')">
          <PreviousMessageNo>
            <xsl:value-of select="$previousMessageIdentifier"/>
          </PreviousMessageNo>
        </xsl:if>
        <DateTimeStamp>
          <xsl:value-of select="DateMapper:ConvertXmlDateString(s0:DataContext/s0:Workflow/s0:TriggerDate/text(), 'yyyy-MM-ddTHH:mm:ss')"/>
        </DateTimeStamp>
      </Header>
      <Body>
        <xsl:variable name="IsFerryTerminal" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'Is_FerryTerminal']/s0:Value/text()"/>
        <xsl:variable name="containerMode" select="s0:ContainerMode/text()"/>
        <xsl:attribute name="type">
          <xsl:choose>
            <xsl:when test="$IsFerryTerminal = 'Y'">FRRY</xsl:when>
            <xsl:when test="$containerMode = 'ROR'">RORO</xsl:when>
            <xsl:otherwise>CONT</xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
        <TransactionCode>
          <xsl:choose>
            <xsl:when test="$purpose = 'ORG'">CREATE</xsl:when>
            <xsl:when test="$purpose = 'AMD'">REPLACE</xsl:when>
            <xsl:when test="$purpose = 'WTH'">CANCEL</xsl:when>
          </xsl:choose>
        </TransactionCode>

        <xsl:if test="$purpose != 'WTH'">
          <xsl:variable name="mrnSubShipment" select="s0:SubShipmentCollection/s0:SubShipment[s0:DataContext/s0:DataSource/s0:Type/text() = 'MRN']"/>

          <xsl:variable name="MRNNodes">
            <xsl:for-each select="$mrnSubShipment">
              <xsl:variable name="subShipment" select="."/>

              <xsl:for-each select="s0:PackingLineCollection/s0:PackingLine">
                <xsl:call-template name="GenerateMRNsNode">
                  <xsl:with-param name="containerMode" select="$containerMode"/>
                  <xsl:with-param name="subShipment" select="$subShipment"/>
                </xsl:call-template>
              </xsl:for-each>
            </xsl:for-each>
          </xsl:variable>

          <xsl:variable name="bookingConfirmationReference" select="s0:BookingConfirmationReference/text()"/>

          <xsl:variable name="terminal" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'Terminal_Code']/s0:Value/text()"/>
          <xsl:choose>
            <xsl:when test="$IsFerryTerminal = 'Y' and $containerMode != 'ROR'">
              <xsl:for-each select="$mrnSubShipment/s0:PackingLineCollection/s0:PackingLine[generate-id(.) = generate-id(key('groupByContainer', s0:ContainerNumber/text()))]">
                <xsl:call-template name="data">
                  <xsl:with-param name="elementName" select="'Vehicle'"/>
                  <xsl:with-param name="number" select="s0:ContainerNumber/text()"/>
                  <xsl:with-param name="MRNNodes" select="$MRNNodes"/>
                  <xsl:with-param name="terminal" select="$terminal"/>
                  <xsl:with-param name="bookingConfirmationReference" select="$bookingConfirmationReference"/>
                </xsl:call-template>
              </xsl:for-each>
            </xsl:when>
            <xsl:when test="$containerMode = 'ROR'">
              <xsl:for-each select="$mrnSubShipment/s0:PackingLineCollection/s0:PackingLine[generate-id(.) = generate-id(key('groupByVIN', s0:ReferenceNumber/text()))]">
                <xsl:call-template name="data">
                  <xsl:with-param name="elementName" select="'Vehicle'"/>
                  <xsl:with-param name="number" select="s0:ReferenceNumber/text()"/>
                  <xsl:with-param name="MRNNodes" select="$MRNNodes"/>
                  <xsl:with-param name="terminal" select="$terminal"/>
                  <xsl:with-param name="bookingConfirmationReference" select="$bookingConfirmationReference"/>
                </xsl:call-template>
              </xsl:for-each>
            </xsl:when>
            <xsl:otherwise>
              <xsl:for-each select="$mrnSubShipment/s0:PackingLineCollection/s0:PackingLine[generate-id(.) = generate-id(key('groupByContainer', s0:ContainerNumber/text()))]">
                <xsl:call-template name="data">
                  <xsl:with-param name="elementName" select="'Container'"/>
                  <xsl:with-param name="number" select="s0:ContainerNumber/text()"/>
                  <xsl:with-param name="MRNNodes" select="$MRNNodes"/>
                  <xsl:with-param name="terminal" select="$terminal"/>
                  <xsl:with-param name="bookingConfirmationReference" select="$bookingConfirmationReference"/>
                </xsl:call-template>
              </xsl:for-each>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:if>
      </Body>
    </EBADEC>
  </xsl:template>

  <xsl:template name="GenerateMRNsNode">
    <xsl:param name="containerMode"/>
    <xsl:param name="subShipment"/>

    <MRN>
      <xsl:attribute name="Entry_DocumentCode">
        <xsl:value-of select="$subShipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'Entry_DocumentCode']/s0:Value/text()"/>
      </xsl:attribute>
      <xsl:attribute name="Customs_OfficeCode">
        <xsl:value-of select="$subShipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'Customs_OfficeCode']/s0:Value/text()"/>
      </xsl:attribute>
      <xsl:choose>
        <xsl:when test="$containerMode = 'ROR'">
          <xsl:attribute name="Key">
            <xsl:value-of select="s0:ReferenceNumber/text()"/>
          </xsl:attribute>
        </xsl:when>
        <xsl:otherwise>
          <xsl:attribute name="Key">
            <xsl:value-of select="s0:ContainerNumber/text()"/>
          </xsl:attribute>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:attribute name="MRN">
        <xsl:value-of select="$subShipment/s0:DataContext/s0:DataSource[s0:Type/text() = 'MRN']/s0:Key/text()"/>
      </xsl:attribute>
    </MRN>
  </xsl:template>

  <xsl:template name="data">
    <xsl:param name="elementName"/>
    <xsl:param name="number"/>
    <xsl:param name="MRNNodes"/>
    <xsl:param name="terminal"/>
    <xsl:param name="bookingConfirmationReference"/>

    <xsl:variable name="transportToTerminalType" select="/s0:UniversalShipment/s0:Shipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'TransportToTerminalType_Code']/s0:Value/text()"/>
    <xsl:variable name="vesselType" select="/s0:UniversalShipment/s0:Shipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'VesselType_Code']/s0:Value/text()"/>

    <xsl:element name="{$elementName}" xmlns="urn:EBA">
      <xsl:attribute name="number">
        <xsl:value-of select="$number"/>
      </xsl:attribute>
      <xsl:for-each select="msxsl:node-set($MRNNodes)/MRN[@Key = $number and generate-id(.) = generate-id(key('groupByMRN', concat(@MRN, @Key)))]">
        <Document type="MRN">
          <xsl:attribute name="code">
            <xsl:value-of select="@Entry_DocumentCode"/>
          </xsl:attribute>
          <xsl:attribute name="office">
            <xsl:value-of select="@Customs_OfficeCode"/>
          </xsl:attribute>
          <xsl:value-of select="@MRN"/>
        </Document>
      </xsl:for-each>
      <BookingsReference>
        <xsl:value-of select="$bookingConfirmationReference"/>
      </BookingsReference>
      <Terminal>
        <xsl:value-of select="$terminal"/>
      </Terminal>
      <xsl:variable name="carrierType">
        <xsl:choose>
          <xsl:when test="$transportToTerminalType = 'SEA'">
            <xsl:choose>
              <xsl:when test="$vesselType = 'BA'">BG</xsl:when>
              <xsl:otherwise>VS</xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$transportToTerminalType = 'ROA'">TR</xsl:when>
          <xsl:when test="$transportToTerminalType = 'RAI'">RL</xsl:when>
        </xsl:choose>
      </xsl:variable>
      <xsl:if test="string-length($carrierType) &gt; 0">
        <CarrierType>
          <xsl:value-of select="$carrierType"/>
        </CarrierType>
      </xsl:if>
    </xsl:element>
  </xsl:template>

</xsl:stylesheet>

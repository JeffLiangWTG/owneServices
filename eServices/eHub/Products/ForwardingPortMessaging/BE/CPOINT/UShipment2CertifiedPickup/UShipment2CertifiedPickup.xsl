<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 CodeMapper ContextAccessor DataModelAccessor DateMapper"
                version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2012/11/NotificationOfCertifiedPickup/1"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
                xmlns:DataModelAccessor="http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"
                xmlns:DateMapper="http://schemas.microsoft.com/BizTalk/2003/DateMapper">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes"/>

  <xsl:template match="/">
    <xsl:apply-templates select="s0:UniversalShipment/s0:Shipment"/>
  </xsl:template>

  <xsl:variable name="recipientID" select="ContextAccessor:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="senderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="serviceProvider" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE', 'FORWARDING_PORT_MESSAGE', 'FPM System Configuration', 'Port Settings', 'Name', $recipientID)"/>
  <xsl:variable name="serviceProviderMSGID" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE', 'FORWARDING_PORT_MESSAGE', 'FPM System Configuration', 'Port Settings', 'MSGID', $recipientID)"/>
  <xsl:variable name="serviceProviderID" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE', 'FORWARDING_PORT_MESSAGE', 'FPM System Configuration', 'Port Settings', 'ID', $recipientID)"/>

  <xsl:template match="s0:UniversalShipment/s0:Shipment">
    <xsl:variable name="fileSenderID" select="DataModelAccessor:GetClientRegistrationCode($senderID, s0:DataContext/s0:Workflow/s0:EventBranch/text(), $serviceProvider)"/>
    <xsl:variable name="operationPortCode" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'OperationalPort_Code']/s0:Value/text()"/>
    <xsl:variable name="consolID" select="s0:DataContext/s0:DataSource/s0:Key/text()"/>
    <xsl:variable name="purpose" select="s0:DataContext/s0:DocumentaryOverride/s0:Purpose/text()"/>
    <xsl:variable name="documentName" select="s0:DataContext/s0:DocumentaryOverride/s0:DocumentName/text()"/>

    <xsl:variable name="interchangeID" select="CodeMapper:CallActionProcedureHelper('GetCounterValue','@value','@name','CargoWise.eHub.Products.ForwardingPortMessaging.BE.NXPORT.Interchange','@maxlength','50')"/>
    <xsl:variable name="containerNumber" select="s0:ContainerCollection/s0:Container/s0:ContainerNumber/text()"/>
    <xsl:variable name="consol_ContainerNumber" select="concat($consolID, '_', $containerNumber)"/>

    <xsl:variable name="previousMessageIdentifier" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $serviceProvider, '@recipientId', $senderID , '@ST_ID',  $serviceProviderMSGID , '@value', $consol_ContainerNumber, '@referenceType', 'CPU')"/>
    <xsl:variable name="messageIdentifier">
      <xsl:choose>
        <xsl:when  test="$previousMessageIdentifier!=''">
          <xsl:value-of select="$previousMessageIdentifier"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:variable name="msgPrefix"  select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE', 'FORWARDING_PORT_MESSAGE', 'FPM System Configuration', 'Port Settings', 'SubscriptionPrefix', $recipientID)"/>
          <xsl:variable name="messageSetID" select="CodeMapper:CallActionProcedureHelper('GetCounterValue','@value','@name','CargoWise.eHub.Products.ForwardingPortMessaging.BE.NXPORT','@maxlength','50')"/>
          <xsl:variable name="formattedMessageID" select ="concat($msgPrefix, format-number($messageSetID, '0000000000'))"/>
          <xsl:variable name="subscribeJobNumber1" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $formattedMessageID, $consol_ContainerNumber, 'CPU')"/>
          <xsl:variable name="subscribeJobNumber2" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $consol_ContainerNumber, $formattedMessageID, 'CPU')"/>
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
    <xsl:variable name="FileName" select="ContextAccessor:SetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06', concat('CPU_', $senderID, '_', $interchangeID))"/>
    <xsl:variable name="EmailSubject" select="ContextAccessor:SetContextProperty('OverrideEmailSubject', 'http://cargowise.com/ehub/processing/2010/06', concat('FPM_', $serviceProvider, '_', $operationPortCode))"/>

    <CertifiedPickUpRequest xmlns="http://www.cargowise.com/Schemas/FPM/CPOINT">
      <Header>
        <externalReferenceId>
          <xsl:value-of select="$messageIdentifier"/>
        </externalReferenceId>
        <parameters>
          <portLoCode>
            <xsl:value-of select="$operationPortCode"/>
          </portLoCode>
          <equipmentNumber>
            <xsl:value-of select="$containerNumber"/>
          </equipmentNumber>
          <Branch>
            <xsl:value-of select="s0:DataContext/s0:Workflow/s0:EventBranch/text()"/>
          </Branch>
          <endPoint>import/release-rights</endPoint>
        </parameters>
      </Header>
      <Body xmlns:json="http://james.newtonking.com/projects/json">
        <releaseIdentification>
          <xsl:value-of select="s0:ContainerCollection/s0:Container/s0:ContainerImportDORelease/text()"/>
        </releaseIdentification>
        <terminalCode>
          <xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'Terminal_Code']/s0:Value/text()"/>
        </terminalCode>
        <xsl:variable name="action">
          <xsl:value-of select="s0:ContainerCollection/s0:Container/s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'Action']/s0:Value/text()"/>
        </xsl:variable>
        <actionType>
          <xsl:choose>
            <xsl:when test="$purpose = 'ORG' and contains('Forwarder,Transporter', $action)">Transfer</xsl:when>
            <xsl:when test="$purpose = 'WTH' and contains('Forwarder,Transporter', $action)">Revoke</xsl:when>
            <xsl:when test="$action = 'Accept'">Accept</xsl:when>
            <xsl:when test="$action = 'Decline'">Decline</xsl:when>
          </xsl:choose>
        </actionType>
        <xsl:variable name="reasonNodeName">
          <xsl:choose>
            <xsl:when test="contains('Forwarder,Transporter', $action)">reasonForAction</xsl:when>
            <xsl:when test="contains('Accept,Decline', $action)">actionReason</xsl:when>
          </xsl:choose>
        </xsl:variable>
        <xsl:element name="{$reasonNodeName}">
          <xsl:value-of select="s0:ContainerCollection/s0:Container/s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'Reason']/s0:Value/text()"/>
        </xsl:element>
        <billOfLadingNumbers json:Array="true">
          <xsl:value-of select="s0:WayBillNumber/text()"/>
        </billOfLadingNumbers>

        <xsl:choose>
          <xsl:when test="contains('Forwarder,Transporter', $action)">
            <xsl:call-template name="CreateIdentificationCodeAndType">
              <xsl:with-param name="elementName" select="'carrier'" />
              <xsl:with-param name="addressType" select="'ShippingLineAddress'"/>
            </xsl:call-template>
            <xsl:call-template name="CreateIdentificationCodeAndType">
              <xsl:with-param name="elementName" select="'releaseFrom'" />
              <xsl:with-param name="addressType" select="'CurrentUser'"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="contains('Accept,Decline', $action)">
            <releaseFrom>
              <identificationType>
                <xsl:value-of select="s0:ContainerCollection/s0:Container/s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'ReleaseFromPartyIdType']/s0:Value/text()"/>
              </identificationType>
              <identificationCode xmlns="http://www.cargowise.com/Schemas/FPM/CPOINT">
                <xsl:value-of select="s0:ContainerCollection/s0:Container/s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'ReleaseFromPartyIdCode']/s0:Value/text()"/>
              </identificationCode>
            </releaseFrom>
          </xsl:when>
        </xsl:choose>

        <xsl:variable name="releaseToAddressType">
          <xsl:choose>
            <xsl:when test="$action = 'Forwarder'">ReceivingForwarderAddress</xsl:when>
            <xsl:when test="$action = 'Transporter'">ArrivalCFSLocalTransportAddress</xsl:when>
            <xsl:when test="contains('Accept,Decline', $action)">CurrentUser</xsl:when>
            <xsl:otherwise></xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:if test="$releaseToAddressType!=''">
          <xsl:call-template name="CreateIdentificationCodeAndType">
            <xsl:with-param name="elementName" select="'releaseTo'" />
            <xsl:with-param name="addressType" select="$releaseToAddressType"/>
          </xsl:call-template>
        </xsl:if>

        <equipmentNumber>
          <xsl:value-of select="$containerNumber"/>
        </equipmentNumber>
        <portLoCode>
          <xsl:value-of select="$operationPortCode"/>
        </portLoCode>
      </Body>
    </CertifiedPickUpRequest>
  </xsl:template>

  <xsl:template name="CreateIdentificationCodeAndType">
    <xsl:param name="elementName" />
    <xsl:param name="addressType" />

    <xsl:variable name="orgAddress" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text() = $addressType]"/>

    <xsl:if test="$orgAddress">
      <xsl:variable name="regNumber" select="$orgAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber[(contains('BTW,PSN', s0:Type/text()) and s0:CountryOfIssue/text()='BE') or contains('EOR,DUN', s0:Type/text())][1]" />
      <xsl:variable name="regNumberType" select="$regNumber/s0:Type/text()"  />
      <xsl:variable name="identificationType">
        <xsl:choose>
          <xsl:when test="$regNumberType='BTW'">Tin</xsl:when>
          <xsl:when test="$regNumberType='PSN'">APCS</xsl:when>
          <xsl:when test="$regNumberType='EOR'">Eori</xsl:when>
          <xsl:when test="$regNumberType='DUN'">Duns</xsl:when>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="identificationValue">
        <xsl:choose>
          <xsl:when test="$identificationType='Eori'">
            <xsl:value-of select="concat($regNumber/s0:CountryOfIssue/text(), $regNumber/s0:Value/text())"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$regNumber/s0:Value/text()"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:element name="{$elementName}" xmlns="http://www.cargowise.com/Schemas/FPM/CPOINT">
        <identificationType xmlns="http://www.cargowise.com/Schemas/FPM/CPOINT">
          <xsl:value-of select="$identificationType"/>
        </identificationType>
        <identificationCode xmlns="http://www.cargowise.com/Schemas/FPM/CPOINT">
          <xsl:value-of select="$identificationValue"/>
        </identificationCode>
      </xsl:element>
    </xsl:if>
  </xsl:template>
</xsl:stylesheet>

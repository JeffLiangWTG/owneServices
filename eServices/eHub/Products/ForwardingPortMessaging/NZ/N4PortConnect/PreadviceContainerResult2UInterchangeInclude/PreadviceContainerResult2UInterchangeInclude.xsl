<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl ns0 s0 CodeMapper ContextAccessor DateMapper" version="1.0"
                xmlns:s0="http://PortConnect.ExportPreAdvice.BizTalk.Common/201208"
                xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2012/11"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
                xmlns:DateMapper="http://schemas.microsoft.com/BizTalk/2003/DateMapper">

  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes" />

  <xsl:template match="/">
    <xsl:apply-templates select="s0:PreadviceContainerResult" />
  </xsl:template>

  <xsl:template match="s0:PreadviceContainerResult">
    <xsl:variable name="senderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')" />
    <xsl:variable name="serviceProvider" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE', 'FORWARDING_PORT_MESSAGE', 'FPM System Configuration', 'Port Settings', 'Name', $senderID)"/>
    <xsl:variable name="serviceProviderMsgID" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE', 'FORWARDING_PORT_MESSAGE', 'FPM System Configuration', 'Port Settings', 'MSGID', $senderID)"/>

    <xsl:variable name="partnerPortCanonicalID" select="s0:header/s0:PartnerPortCanonicalID/text()"/>
    <xsl:variable name="tradingPartnerCanonicalID" select="s0:header/s0:TradingPartnerCanonicalID/text()"/>

    <UniversalInterchangeInclude xmlns="http://www.cargowise.com/Schemas/Universal/2012/11">
      <Body>
        <xsl:for-each select="s0:PreAdvice/s0:equipment">
          <xsl:variable name="equipmentID" select="s0:equipmentID/text()"/>
          <xsl:variable name="documentIdentifier" select="concat($equipmentID, '_', $partnerPortCanonicalID, '_', $tradingPartnerCanonicalID)"/>
          <xsl:variable name="messageIdentifier" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $senderID, '@recipientId', '', '@ST_ID', $serviceProviderMsgID, '@value', $documentIdentifier, '@referenceType', 'PreAdvice')" />
          <xsl:variable name="subscribedDocumentName" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $senderID, '@recipientId', '', '@ST_ID', $serviceProviderMsgID, '@value', $messageIdentifier, '@referenceType', 'DocumentName')" />
          <xsl:variable name="subscribedForwardingType" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $senderID, '@recipientId', '', '@ST_ID', $serviceProviderMsgID, '@value', $messageIdentifier, '@referenceType', 'ForwardingType')" />
          <xsl:variable name="subscribedPurpose" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $senderID, '@recipientId', '', '@ST_ID', $serviceProviderMsgID, '@value', $messageIdentifier, '@referenceType', 'Purpose')" />

          <xsl:variable name="forwardingType" select="substring-before($subscribedForwardingType, '_')"/>
          <xsl:variable name="forwardingKey" select="substring-after($subscribedForwardingType, '_')"/>

          <xsl:variable name="errors" select=" s0:Errors"/>

          <xsl:if test="$forwardingKey != ''">
            <UniversalEvent xmlns="http://www.cargowise.com/Schemas/Universal/2012/11">
              <Event>
                <DataContext>
                  <DocumentaryOverride>
                    <DocumentName>
                      <xsl:value-of select="$subscribedDocumentName"/>
                    </DocumentName>
                  </DocumentaryOverride>
                  <DataTargetCollection>
                    <DataTarget>
                      <Key>
                        <xsl:value-of select="$forwardingKey"/>
                      </Key>
                      <Type>
                        <xsl:value-of select="$forwardingType"/>
                      </Type>
                    </DataTarget>
                  </DataTargetCollection>
                </DataContext>
                <EventTime>
                  <xsl:value-of select="DateMapper:CurrentDateTime('yyyy-MM-ddTHH:mm:ss')" />
                </EventTime>
                <EventType>
                  <xsl:variable name="businessAcknowledgement" select=" s0:BusinessAcknowledgement/text()"/>
                  <xsl:choose>
                    <xsl:when test="$businessAcknowledgement = 'Fail'">MRJ</xsl:when>
                    <xsl:when test="$businessAcknowledgement = 'Succeed' and $subscribedPurpose != 'WTH'">MAA</xsl:when>
                    <xsl:when test="$businessAcknowledgement = 'Succeed' and $subscribedPurpose = 'WTH'">MWA</xsl:when>
                  </xsl:choose>
                </EventType>
                <EventParameters>
                  <Department>Terminal</Department>
                  <MessageType>
                    <xsl:value-of select="$subscribedDocumentName"/>
                  </MessageType>
                  <Location>
                    <xsl:value-of select="$partnerPortCanonicalID"/>
                  </Location>
                  <xsl:if test="$equipmentID != ''">
                    <EquipmentReferenceNumber>
                      <xsl:value-of select="$equipmentID"/>
                    </EquipmentReferenceNumber>
                  </xsl:if>
                  <xsl:if test="count($errors) > 0">
                    <Reason>
                      <xsl:choose>
                        <xsl:when test="count($errors) = 1">
                          <xsl:value-of select="concat($errors[1]/s0:ErrorID/text(), '-', $errors[1]/s0:ErrorText/text())"/>
                        </xsl:when>
                        <xsl:otherwise>See ContextCollection - ErrorText</xsl:otherwise>
                      </xsl:choose>
                    </Reason>
                  </xsl:if>
                </EventParameters>
                <xsl:if test="$equipmentID != '' or count($errors) > 1">
                  <ContextCollection>
                    <xsl:if test="$equipmentID != ''">
                      <Context>
                        <Type>ContainerNumber</Type>
                        <Value>
                          <xsl:value-of select="$equipmentID"/>
                        </Value>
                      </Context>
                    </xsl:if>
                    <xsl:if test="count($errors) > 1">
                      <xsl:for-each select="$errors">
                        <Context>
                          <Type>ErorrText</Type>
                          <Value>
                            <xsl:value-of select="concat(./s0:ErrorID/text(), '-', ./s0:ErrorText/text())"/>
                          </Value>
                        </Context>
                      </xsl:for-each>
                    </xsl:if>
                  </ContextCollection>
                </xsl:if>
              </Event>
            </UniversalEvent>
          </xsl:if>
        </xsl:for-each>
      </Body>
    </UniversalInterchangeInclude>
  </xsl:template>
</xsl:stylesheet>

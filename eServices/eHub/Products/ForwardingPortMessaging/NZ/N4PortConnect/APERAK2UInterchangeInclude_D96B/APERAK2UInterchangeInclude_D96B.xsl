<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 CodeMapper ContextAccessor DateMapper" version="1.0"
                xmlns:s0="http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
                xmlns:DateMapper="http://schemas.microsoft.com/BizTalk/2003/DateMapper">

  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes" />

  <xsl:template match="/">
    <xsl:apply-templates select="s0:EFACT_D96B_APERAK" />
  </xsl:template>

  <xsl:template match="s0:EFACT_D96B_APERAK">
    <xsl:variable name="SenderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')" />
    <xsl:variable name="serviceProviderMsgID" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE', 'FORWARDING_PORT_MESSAGE', 'FPM System Configuration', 'Port Settings', 'MSGID', $SenderID)"/>
    <xsl:variable name="messageReference" select="substring-before(s0:FTX[FTX01/text()='ZZZ']/s0:C108/C10801/text(), '.')" />

    <xsl:variable name="subscribedDocumentName" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $serviceProviderMsgID, '@value', $messageReference, '@referenceType', 'DocumentName')" />
    <xsl:variable name="subscribedPurpose" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $serviceProviderMsgID, '@value', $messageReference, '@referenceType', 'Purpose')" />
    <xsl:variable name="subscribedOperationPort" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $serviceProviderMsgID, '@value', $messageReference, '@referenceType', 'OperationPort')" />
    <xsl:variable name="subscribedForwardingType" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $serviceProviderMsgID, '@value', $messageReference, '@referenceType', 'ForwardingType')" />
    <xsl:variable name="forwardingType" select="substring-before($subscribedForwardingType, '_')"/>
    <xsl:variable name="forwardingKey" select="substring-after($subscribedForwardingType, '_')"/>

    <xsl:variable name="eventType" select="CodeMapper:GetRecipientCode('FPMAPERAK', 'FPMAPERAK', 'FPM APERAK Configuration', 'Event Type', 'Event Type', s0:BGM/BGM04/text(), $subscribedPurpose)"/>
    <xsl:variable name="recipientID" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedClients', '', '@senderId', $SenderID, '@ST_ID', $serviceProviderMsgID, '@value', $messageReference)"/>

    <xsl:if test="$recipientID!=''">
      <xsl:variable name="SetRecipientID" select="ContextAccessor:SetContextProperty('DestinationParty','http://schemas.microsoft.com/BizTalk/2003/system-properties', $recipientID)"/>
    </xsl:if>

    <UniversalInterchangeInclude xmlns="http://www.cargowise.com/Schemas/Universal/2012/11">
      <Body>
        <UniversalEvent>
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
              <xsl:value-of select="$eventType" />
            </EventType>
            <EventParameters>
              <Department>Terminal</Department>
              <Location>
                <xsl:value-of select="$subscribedOperationPort"/>
              </Location>
              <MessageType>
                <xsl:value-of select="$subscribedDocumentName"/>
              </MessageType>
              <xsl:if test="$eventType = 'IRJ' or $eventType = 'MRJ' or $eventType = 'MPP'">
                <Reason>
                  <xsl:value-of select="s0:ERCLoop1/s0:FTX_2[FTX01='AAO']/s0:C108_2/C10801/text()"/>
                </Reason>
              </xsl:if>
            </EventParameters>

            <ContextCollection>
              <xsl:call-template name="MapContext">
                <xsl:with-param name="Type" select="'Response'" />
                <xsl:with-param name="Value" select="CodeMapper:GetRecipientCode('FPMAPERAK', 'FPMAPERAK', 'FPM APERAK Configuration', 'Event Type', 'Event Reference', s0:BGM/BGM04/text(), $subscribedPurpose)"/>
              </xsl:call-template>
            </ContextCollection>
          </Event>
        </UniversalEvent>
      </Body>
    </UniversalInterchangeInclude>
  </xsl:template>

  <xsl:template name="MapContext">
    <xsl:param name="Type" />
    <xsl:param name="Value" />

    <xsl:if test="$Type != '' and $Value != ''">
      <Context xmlns="http://www.cargowise.com/Schemas/Universal/2012/11">
        <Type>
          <xsl:value-of select="$Type"/>
        </Type>
        <Value>
          <xsl:value-of select="$Value"/>
        </Value>
      </Context>
    </xsl:if>
  </xsl:template>
</xsl:stylesheet>

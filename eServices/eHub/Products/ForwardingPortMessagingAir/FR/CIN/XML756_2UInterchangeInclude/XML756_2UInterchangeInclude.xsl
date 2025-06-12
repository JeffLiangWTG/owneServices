<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 CodeMapper ContextAccessor DateMapper" version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/FPMA/CIN/756"
                xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2012/11"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
                xmlns:DateMapper="http://schemas.microsoft.com/BizTalk/2003/DateMapper">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes" />

  <xsl:template match="/">
    <xsl:apply-templates select="/s0:Xml756" />
  </xsl:template>

  <xsl:template match="/s0:Xml756">
    <xsl:variable name="messageReference" select="Customs/TraderNumber/text()"/>

    <xsl:variable name="senderId" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')" />

    <xsl:variable name="serviceProviderMsgId" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE_AIR', 'FORWARDING_PORT_MESSAGE_AIR', 'FPMA System Configuration', 'Port Settings', 'MSGID', $senderId)" />

    <xsl:variable name="subscribedDocumentName" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $senderId, '@recipientId', '', '@ST_ID', $serviceProviderMsgId, '@value', $messageReference, '@referenceType', 'DocumentName')" />
    <xsl:variable name="subscribedJobNumber" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $senderId, '@recipientId', '', '@ST_ID', $serviceProviderMsgId, '@value', $messageReference, '@referenceType', 'JobNumber')" />
    <xsl:variable name="subscribedForwardingType" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $senderId, '@recipientId', '', '@ST_ID', $serviceProviderMsgId, '@value', $messageReference, '@referenceType', 'ForwardingType')" />
    <xsl:variable name="subscribedOperationPort" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $senderId, '@recipientId', '', '@ST_ID', $serviceProviderMsgId, '@value', $messageReference, '@referenceType', 'OperationPort')" />

    <xsl:variable name="recipientID" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedClients', '', '@senderId', $senderId, '@ST_ID', $serviceProviderMsgId, '@value', $messageReference)"/>
    <xsl:if test="$recipientID!=''">
      <xsl:variable name="SetDestinationParty" select="ContextAccessor:SetContextProperty('DestinationParty','http://schemas.microsoft.com/BizTalk/2003/system-properties', $recipientID)"/>
    </xsl:if>

    <ns0:UniversalInterchangeInclude>
      <ns0:Body>
        <xsl:if test="$subscribedJobNumber != ''">
          <ns0:UniversalEvent>
            <ns0:Event>
              <ns0:DataContext>
                <ns0:DocumentaryOverride>
                  <ns0:DocumentName>
                    <xsl:value-of select="$subscribedDocumentName"/>
                  </ns0:DocumentName>
                </ns0:DocumentaryOverride>
                <ns0:DataTargetCollection>
                  <ns0:DataTarget>
                    <ns0:Key>
                      <xsl:value-of select="$subscribedJobNumber"/>
                    </ns0:Key>
                    <ns0:Type>
                      <xsl:value-of select="$subscribedForwardingType"/>
                    </ns0:Type>
                  </ns0:DataTarget>
                </ns0:DataTargetCollection>
              </ns0:DataContext>
              <ns0:EventTime>
                <xsl:value-of select="DateMapper:ConvertToDateTimeString(Customs/Datetime/text(), 'yyyyMMddHHmm', 'yyyy-MM-ddTHH:mm:ss')"/>
              </ns0:EventTime>
              <ns0:EventType>MRJ</ns0:EventType>

              <xsl:variable name="errors" select="Errors/Error" />

              <ns0:EventParameters>
                <ns0:Department>Terminal</ns0:Department>
                <ns0:MessageType>
                  <xsl:value-of select="$subscribedDocumentName"/>
                </ns0:MessageType>
                <ns0:Location>
                  <xsl:value-of select="$subscribedOperationPort"/>
                </ns0:Location>
                <ns0:Reason>
                  <xsl:choose>
                    <xsl:when test="count($errors) > 1">See ContextCollection - ErrorText</xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="concat($errors[1]/Rejection/text(), ' - ', $errors[1]/Reason/text())"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </ns0:Reason>
              </ns0:EventParameters>
              <ns0:EventReference />

              <xsl:choose>
                <xsl:when test="count($errors) > 1">
                  <ns0:ContextCollection>
                    <xsl:for-each select="$errors">
                      <ns0:Context>
                        <ns0:Type>ErrorText</ns0:Type>
                        <ns0:Value>
                          <xsl:value-of select="concat(Rejection/text(), ' - ', Reason/text())"/>
                        </ns0:Value>
                      </ns0:Context>
                    </xsl:for-each>
                  </ns0:ContextCollection>
                </xsl:when>
                <xsl:otherwise>
                  <ns0:ContextCollection/>
                </xsl:otherwise>
              </xsl:choose>

            </ns0:Event>
          </ns0:UniversalEvent>
        </xsl:if>
      </ns0:Body>
    </ns0:UniversalInterchangeInclude>
  </xsl:template>
</xsl:stylesheet>

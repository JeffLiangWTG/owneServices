<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                exclude-result-prefixes="msxsl var s0 ns0 ContextAccessor CodeMapper DateMapper"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var"
                xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2012/11"
                xmlns:s0="http://wisetechglobal.com/ehub/acas/br/AsyncPolling"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:DateMapper="http://schemas.microsoft.com/BizTalk/2003/DateMapper"
                version="1.0" >
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes" />

  <xsl:template match="/">
    <xsl:apply-templates select="s0:Config" />
  </xsl:template>

  <xsl:variable name="reason" select="/*[local-name()='Config']/*[local-name()='StateDescription']" />

  <xsl:template match="s0:Config">
    <ns0:UniversalInterchangeInclude>
      <ns0:Body>
        
        <xsl:if test="$reason!=''">
          <xsl:variable name="SenderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')" />
          <xsl:variable name="DestinationParty" select="ContextAccessor:GetContextProperty('DestinationParty','http://schemas.microsoft.com/BizTalk/2003/system-properties')" />
          <xsl:variable name="subscriptionType" select="'ACASBR'" />

          <xsl:variable name="ACASRecipientID">
            <xsl:choose>
              <xsl:when test="contains($SenderID, 'TST')">ACAS_BRTest</xsl:when>
              <xsl:otherwise>ACAS_BR</xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <xsl:variable name="protocolNumber" select="s0:ProtocolNumber/text()" />
          <xsl:variable name="CCTRecipientId" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedClients', '', '@senderId', $SenderID, '@ST_ID', $subscriptionType, '@value', $protocolNumber)" />
          <xsl:variable name="messageReference" select="s0:ReferenceValue/text()" />

          <xsl:variable name="subscribedDocumentName" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', $CCTRecipientId, '@ST_ID', $subscriptionType, '@value', $messageReference, '@referenceType', 'DocumentName')" />

          <ns0:UniversalEvent>
            <ns0:Event>
              <ns0:DataContext>
                <ns0:DocumentaryOverride>
                  <ns0:DocumentName>
                    <xsl:value-of select="$subscribedDocumentName" />
                  </ns0:DocumentName>
                </ns0:DocumentaryOverride>
                <ns0:DataTargetCollection>
                  <ns0:DataTarget>
                    <ns0:Key>
                      <xsl:value-of select="s0:ContextKey/text()" />
                    </ns0:Key>
                    <ns0:Type>
                      <xsl:value-of select="s0:ContextType/text()" />
                    </ns0:Type>
                  </ns0:DataTarget>
                </ns0:DataTargetCollection>
              </ns0:DataContext>

              <ns0:EventTime>
                <xsl:value-of select="DateMapper:CurrentDateTime('s')" />
              </ns0:EventTime>
              <ns0:EventType>IRJ</ns0:EventType>
              <ns0:EventParameters>
                <Department>Customs</Department>
                <Location>BR</Location>
                <ns0:MessageType>
                  <xsl:value-of select="$subscribedDocumentName" />
                </ns0:MessageType>
                <ns0:ReferenceNumber>
                  <xsl:value-of select="$protocolNumber"/>
                </ns0:ReferenceNumber>
                <ns0:Reason>
                  <xsl:value-of select="$reason" />
                </ns0:Reason>
              </ns0:EventParameters>
              <ns0:ContextCollection />
            </ns0:Event>
          </ns0:UniversalEvent>
        </xsl:if>
      </ns0:Body>
    </ns0:UniversalInterchangeInclude>
  </xsl:template>


  <xsl:template name="GenerateContext">
    <xsl:param name="type" />
    <xsl:param name="value" />
    <xsl:if test="$value != ''">
      <ns0:Context>
        <ns0:Type>
          <xsl:value-of select="$type" />
        </ns0:Type>
        <ns0:Value>
          <xsl:value-of select="$value" />
        </ns0:Value>
      </ns0:Context>
    </xsl:if>
  </xsl:template>

</xsl:stylesheet>

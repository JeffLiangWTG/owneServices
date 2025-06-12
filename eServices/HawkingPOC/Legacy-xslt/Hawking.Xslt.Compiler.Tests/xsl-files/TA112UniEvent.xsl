<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl ns0 userCSharp ScriptNS0 ScriptNS1 "
                version="1.0"
                xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2012/11"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp"
                xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
                xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1">

  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />

  <xsl:template match="/">
    <xsl:apply-templates select="/*[local-name()='PcsDocument']" />
  </xsl:template>
  <xsl:template match="/*[local-name()='PcsDocument']" >
    <xsl:variable name="header" select="*[local-name()='PcsDocumentHeader']" />
    <xsl:variable name="previousContentReference" select="*[local-name()='PcsDocumentContent']/*[local-name()='contentHeader']/*[local-name()='previousContentReference']/text()" />
    <xsl:variable name="consolNumber" select="ScriptNS1:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', 'PORTBASE', '@recipientId', '' , '@ST_ID', 'PBSMSG', '@value', $previousContentReference)" />
    
    <ns0:UniversalInterchangeInclude>
      <ns0:Body>
        <ns0:UniversalEvent>
          <ns0:Event>
            <ns0:DataContext>
              <ns0:DocumentaryOverride>
                <ns0:DocumentName>Import Notification</ns0:DocumentName>
              </ns0:DocumentaryOverride>
              <ns0:DataTargetCollection>
                <ns0:DataTarget>
                  <ns0:Key>
                    <xsl:value-of select="$consolNumber" />
                  </ns0:Key>
                  <ns0:Type>ForwardingConsol</ns0:Type>
                </ns0:DataTarget>
              </ns0:DataTargetCollection>
            </ns0:DataContext>
            <xsl:variable name="eventTime" select="substring-before(*[local-name()='PcsDocumentHeader']/*[local-name()='documentIdentification']/*[local-name()='creationDateTime']/text(),'+')"/>
            <ns0:EventTime>
              <xsl:choose>
                <xsl:when test="$eventTime!=''">
                  <xsl:value-of select="$eventTime" />
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="ScriptNS0:CurrentDateTime('s')" />
                </xsl:otherwise>
              </xsl:choose>
            </ns0:EventTime>
            <ns0:EventType>
              <xsl:variable name="responseTypeCode" select="*[local-name()='PcsDocumentContent']/*[local-name()='contentBody']/*[local-name()='technicalResponse']/*[local-name()='responseTypeCode']/text()" />
              <xsl:variable name="eventType" select="ScriptNS1:GetRecipientCode('PORTBASE','PORTBASE','Technical Acknowledgement TA11 from Portbase','Event Type','Event Type',$responseTypeCode)" />
              <xsl:choose>
                <xsl:when test="$eventType!=''">
                  <xsl:value-of select="$eventType" />
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="concat('Unknown Code: ', $responseTypeCode)" />
                </xsl:otherwise>
              </xsl:choose>
            </ns0:EventType>
            <ns0:EventParameters>
              <ns0:Department>Portbase</ns0:Department>
              <ns0:MessageType>Import Notification</ns0:MessageType>

              <xsl:variable name="errorDescription">
                <xsl:for-each select="*[local-name()='PcsDocumentContent']/*[local-name()='contentBody']/*[local-name()='technicalResponse']/*[local-name()='responseTypeReason']">
                  <xsl:value-of select="text()" />
                  <xsl:text>;</xsl:text>
                </xsl:for-each>
              </xsl:variable>
              <xsl:if test="$errorDescription!=''">
                <xsl:call-template name="whileLoopReason">
                  <xsl:with-param name="errorDescription" select="normalize-space($errorDescription)"/>
                </xsl:call-template>
              </xsl:if>

            </ns0:EventParameters>
            <ns0:EventReference>Technical Acknowledgment</ns0:EventReference>
          </ns0:Event>
        </ns0:UniversalEvent>
      </ns0:Body>
    </ns0:UniversalInterchangeInclude>
  </xsl:template>

  <xsl:template name="whileLoopReason">
    <xsl:param name="errorDescription" />
    <xsl:variable name="left1024" select="substring($errorDescription, 1 , 1024)"/>
    <xsl:variable name="right1024" select="substring($errorDescription, 1025)"/>
    <xsl:if test="$left1024!=''">
      <ns0:Reason>
        <xsl:value-of select="$left1024" />
      </ns0:Reason>
    </xsl:if>

    <!-- evaluate and recurse -->
    <xsl:if test="$right1024!=''">
      <xsl:call-template name="whileLoopReason">
        <xsl:with-param name="errorDescription" select="$right1024"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>
  <xsl:output method="xml" indent="yes"/>
</xsl:stylesheet>

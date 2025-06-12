<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 CodeMapper ContextAccessor DataModelAccessor DateMapper"
                version="1.0"
                xmlns:s0="urn:EBA"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
                xmlns:DataModelAccessor="http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"
                xmlns:DateMapper="http://schemas.microsoft.com/BizTalk/2003/DateMapper">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes"/>

  <xsl:variable name="senderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')" />

  <xsl:template match="/">
    <SOAP-ENV:Envelope xmlns:SOAP-ENV="http://schemas.xmlsoap.org/soap/envelope/">
      <SOAP-ENV:Header>
        <wsse:Security xmlns:wsse="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd" mustUnderstand="1">
          <wsse:UsernameToken xmlns:wsu="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd" wsu:Id="UsernameToken-1">
            <wsse:Username>
              <xsl:variable name="userName" select="CodeMapper:GetRecipientCode('CPOINT', 'CPOINT', 'CPOINT System Configuration', 'Connection Details', 'Username', 'EBADEC')"/>
              <xsl:value-of select="$userName"/>
            </wsse:Username>
            <wsse:Password Type="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText">
              <xsl:variable name="password" select="CodeMapper:GetRecipientCode('CPOINT', 'CPOINT', 'CPOINT System Configuration', 'Connection Details', 'Password', 'EBADEC')"/>
              <xsl:value-of select="$password"/>
            </wsse:Password>
          </wsse:UsernameToken>
        </wsse:Security>
      </SOAP-ENV:Header>
      <SOAP-ENV:Body>
        <por:sendMessages xmlns:por="http://portcommunity.haven.antwerpen.be/">
          <messages>
            <body>
              <xsl:apply-templates select="*" mode="serialize"/>
            </body>
            <checkpoint>
              <xsl:variable name="messageIdentifier" select="s0:EBADEC/s0:Header/s0:MessageNo/text()"/>
              <xsl:variable name="eHubMessageTrackingId" select="ContextAccessor:GetContextProperty('MessageTrackingID', 'http://cargowise.com/ehub/tracking/2010/06')"/>
              <xsl:variable name="SubscribeCheckPoint" select="DataModelAccessor:InsertSubscriptionValue('CPTMSG', 'CPOINT', $senderID, $eHubMessageTrackingId, $messageIdentifier, 'CheckPoint')" />
              <xsl:value-of select="$eHubMessageTrackingId"/>
            </checkpoint>
            <header>
              <messageTypeCode>EBADEC</messageTypeCode>
              <messageTypeVersion>1.0</messageTypeVersion>
              <mailbox>
                <xsl:value-of select="concat(s0:EBADEC/s0:Header/s0:Sender/@codeType, ':', s0:EBADEC/s0:Header/s0:Sender/text())"/>
              </mailbox>
            </header>
          </messages>
        </por:sendMessages>
      </SOAP-ENV:Body>
    </SOAP-ENV:Envelope>
  </xsl:template>

  <xsl:template match="*" mode="serialize">
    <xsl:text>&lt;</xsl:text>
    <xsl:value-of select="name()"/>
    <xsl:apply-templates select="@*" mode="serialize" />
    <xsl:if test="name() = 'EBADEC'">
      <xsl:text> xmlns="urn:EBA" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"</xsl:text>
    </xsl:if>
    <xsl:choose>
      <xsl:when test="node()">
        <xsl:text>&gt;</xsl:text>
        <xsl:apply-templates mode="serialize" />
        <xsl:text>&lt;/</xsl:text>
        <xsl:value-of select="name()"/>
        <xsl:text>&gt;</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text> /&gt;</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="@*" mode="serialize">
    <xsl:text> </xsl:text>
    <xsl:value-of select="name()"/>
    <xsl:text>="</xsl:text>
    <xsl:value-of select="."/>
    <xsl:text>"</xsl:text>
  </xsl:template>

  <xsl:template match="text()" mode="serialize">
    <xsl:value-of select="."/>
  </xsl:template>

</xsl:stylesheet>

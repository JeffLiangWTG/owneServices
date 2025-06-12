<xsl:stylesheet exclude-result-prefixes="msxsl var s0" version="1.0" xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:ScriptNS3="http://schemas.microsoft.com/BizTalk/2003/ScriptNS3"
                xmlns:ScriptNS4="http://schemas.microsoft.com/BizTalk/2003/ScriptNS4"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp"
                xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:s0="http://cargowise.com/ehub/core/genericmessagedelivery"
                xmlns:gmi="http://cargowise.com/ehub/core/genericmessagedelivery">

  <xsl:output indent="no" method="xml" omit-xml-declaration="yes" version="1.0"/>
  <xsl:variable name="filename" select="ScriptNS4:GetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06')"/>
  <xsl:variable name="Body" select="/s0:GenericMessageInterchange/s0:Body"/>

  <xsl:template match="/">
    <xsl:element name="gmi:GenericMessageInterchange">
      <xsl:element name="gmi:Header">
        <xsl:element name="gmi:SenderID">
          <xsl:value-of select="/s0:GenericMessageInterchange/s0:Header/s0:SenderID"/>
        </xsl:element>
        <xsl:element name="gmi:RecipientID">
          <xsl:value-of select="/s0:GenericMessageInterchange/s0:Header/s0:RecipientID"/>
        </xsl:element>
        <xsl:element name="gmi:InterchangeType">
          <xsl:value-of select="/s0:GenericMessageInterchange/s0:Header/s0:InterchangeType"/>
        </xsl:element>
        <xsl:element name="gmi:InterchangeNumber">
          <xsl:value-of select="/s0:GenericMessageInterchange/s0:Header/s0:InterchangeNumber"/>
        </xsl:element>
      </xsl:element>
      <xsl:element name="gmi:Body">
        <xsl:element name="gmi:ZippedMessage">
          <xsl:value-of select="ScriptNS3:CreateZippedMessage($Body, $filename)"/>
        </xsl:element>
      </xsl:element>
    </xsl:element>
  </xsl:template>

</xsl:stylesheet>

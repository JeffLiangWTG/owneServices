<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 ScriptNS0 ScriptNS1 userCSharp" version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
                xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp"
                xmlns:xop="http://www.w3.org/2004/08/xop/include">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0"/>
  <xsl:template match="/">
    <xsl:variable name="SourceParty" select="ScriptNS0:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
    <xsl:variable name="RecipientID" select="ScriptNS0:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
    <xsl:variable name="ContextCollection" select="/s0:UniversalEvent/s0:Event/s0:AttachedDocumentCollection/s0:AttachedDocument/s0:ContextCollection"/>
    <xsl:variable name="EventParameters" select="/s0:UniversalEvent/s0:Event/s0:EventParameters"/>

    <xsl:variable name="CustomsRecipientID">
      <xsl:choose>
        <xsl:when test="$RecipientID='CACustomsDoc'">
          <xsl:value-of select="'CACustoms'"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="'CACustomsTest'"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="SubscribeReferenceNumber" select="ScriptNS1:InsertSubscriptionValue('CACDIF', $CustomsRecipientID, $SourceParty, $EventParameters/s0:RequestNumber/text())"/>
    <LPCOImages xsi:noNamespaceSchemaLocation="LPCO_Image_TCP.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xop="http://www.w3.org/2004/08/xop/include">
      <B2BInfo Type="CECP1">
        <VanID>WISETECH</VanID>
        <PartnerID>
          <xsl:choose>
            <xsl:when test="$RecipientID='CACustomsDoc'">WTCDIF01</xsl:when>
            <xsl:otherwise>WTCDIF02</xsl:otherwise>
          </xsl:choose>
        </PartnerID>
        <PartnerQualifier/>
        <DocID>
          <xsl:choose>
            <xsl:when test="$RecipientID='CACustomsDoc'">IIDP</xsl:when>
            <xsl:otherwise>IIDT</xsl:otherwise>
          </xsl:choose>
        </DocID>
      </B2BInfo>
      <xsl:call-template name="NodeIfNotEmpty">
        <xsl:with-param name="Node">BN</xsl:with-param>
        <xsl:with-param name="Value" select="$ContextCollection/s0:Context[s0:Type/text()='ImporterBusinessNumber']/s0:Value/text()"/>
      </xsl:call-template>
      <MessageType>LPCO</MessageType>
      <Image>
        <ImageHeader>
          <Identifier>
            <xsl:value-of select="$EventParameters/s0:RequestNumber/text()"/>
          </Identifier>
          <FunctionCode>
            <xsl:value-of select="$EventParameters/s0:MessageSubType/text()"/>
          </FunctionCode>
          <LPCOImageTypeCode>
            <xsl:value-of select="$ContextCollection/s0:Context[s0:Type/text()='CustomsDocumentType']/s0:Value/text()"/>
          </LPCOImageTypeCode>
          <xsl:call-template name="NodeIfNotEmpty">
            <xsl:with-param name="Node">LPCOReferenceNumber</xsl:with-param>
            <xsl:with-param name="Value" select="$EventParameters/s0:ReferenceNumber/text()"/>
          </xsl:call-template>
          <LPCOImageEffectiveDate>
            <xsl:value-of select="substring-before($ContextCollection/s0:Context[s0:Type/text()='EffectiveDate']/s0:Value/text(),'T')"/>
          </LPCOImageEffectiveDate>
          <LPCOImageExpiryDate>
            <xsl:value-of select="substring-before($ContextCollection/s0:Context[s0:Type/text()='ExpiryDate']/s0:Value/text(),'T')"/>
          </LPCOImageExpiryDate>
          <xsl:variable name="FileName" select="/s0:UniversalEvent/s0:Event/s0:AttachedDocumentCollection/s0:AttachedDocument/s0:FileName/text()" />
          <LPCOImageFormat>
            <xsl:value-of select="userCSharp:GetFileFormat($FileName)"/>
          </LPCOImageFormat>
        </ImageHeader>
        <xop:Include href=""></xop:Include>
      </Image>
    </LPCOImages>
  </xsl:template>

  <xsl:template name="NodeIfNotEmpty">
    <xsl:param name="Node"/>
    <xsl:param name="Value"/>
    <xsl:if test="$Value!=''">
      <xsl:element name="{$Node}">
        <xsl:value-of select="$Value"/>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[
public string GetFileFormat(string fileName)
{
	string extension = System.IO.Path.GetExtension(fileName);
	if (!string.IsNullOrWhiteSpace(extension))
		return extension.Substring(1).ToLowerInvariant();
	return extension;
}
]]>
  </msxsl:script>
</xsl:stylesheet>

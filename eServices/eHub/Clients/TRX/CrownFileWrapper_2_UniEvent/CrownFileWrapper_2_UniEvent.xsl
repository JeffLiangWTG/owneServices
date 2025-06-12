<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var"
                exclude-result-prefixes="msxsl var s0 ScriptNS0 ScriptNS1 ScriptNS2 userCSharp" version="1.0"
                xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:s0="http://cargowise.com/ehub/core/2013/08"
                xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
                xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1"
                xmlns:ScriptNS2="http://schemas.microsoft.com/BizTalk/2003/ScriptNS2"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />

  <xsl:template match="/">
    <xsl:apply-templates select="/*[local-name()='FileWrapper']" />
  </xsl:template>

  <xsl:template match="/*[local-name()='FileWrapper']">
    <ns0:UniversalInterchange>
      <Header>
        <SenderID></SenderID>
        <RecipientID></RecipientID>
      </Header>
      <Body>
        <xsl:element name="ns0:UniversalEvent">

          <xsl:variable name="Filename">
            <xsl:variable name="SourceFileName" select="*[local-name()='FileName']" />
            <xsl:variable name="Path">
              <xsl:choose>
                <xsl:when test="$SourceFileName != ''">
                  <xsl:value-of select="$SourceFileName"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="ScriptNS2:GetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06')"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <xsl:value-of select="userCSharp:GetFileName($Path)"/>
          </xsl:variable>

          <xsl:element name="ns0:Event">
            <ns0:DataContext>
              <ns0:DataTargetCollection>
                <ns0:DataTarget>
                  <ns0:Type>ForwardingShipment</ns0:Type>

                  <ns0:Key>
                    <xsl:value-of select="substring-before(substring-after($Filename, '_'), '_')"/>
                  </ns0:Key>
                </ns0:DataTarget>
              </ns0:DataTargetCollection>
            </ns0:DataContext>

            <xsl:element name="ns0:EventTime">
              <xsl:value-of select="ScriptNS0:CurrentDateTimeWithTimeZone()"/>
            </xsl:element>

            <xsl:element name="ns0:EventType">
              <xsl:value-of select="ScriptNS1:GetRecipientCodeUnkeyed('TRXELPELP_C05', 'TRXELPELP', 'Crown Data PDF &amp; TIFF - Receive eDocs', 'Defaults', 'Event Code')"/>
            </xsl:element>

            <xsl:call-template name="MapValueIfNotEmpty">
              <xsl:with-param name="NodeName" select="'ns0:EventReference'" />
              <xsl:with-param name="Value" select="ScriptNS1:GetRecipientCodeUnkeyed('TRXELPELP_C05', 'TRXELPELP', 'Crown Data PDF &amp; TIFF - Receive eDocs' , 'Defaults', 'Event Reference')" />
            </xsl:call-template>


            <xsl:element name="ns0:AttachedDocumentCollection">
              <xsl:element name="ns0:AttachedDocument">

                <xsl:element name="ns0:FileName">
                  <xsl:value-of select="$Filename"/>
                </xsl:element>

                <xsl:element name="ns0:ImageData">
                  <xsl:value-of select="./*[local-name()='FileStream']"/>
                </xsl:element>

                <xsl:element name="ns0:Type">
                  
                  <xsl:element name="ns0:Code">
                    <xsl:value-of select="ScriptNS1:GetRecipientCodeUnkeyed('TRXELPELP_C05', 'TRXELPELP', 'Crown Data PDF &amp; TIFF - Receive eDocs' , 'Defaults', 'Document Type')"/>
                  </xsl:element>

                  <xsl:element name="ns0:Description">
                    <xsl:value-of select="ScriptNS1:GetRecipientCodeUnkeyed('TRXELPELP_C05', 'TRXELPELP', 'Crown Data PDF &amp; TIFF - Receive eDocs' , 'Defaults', 'Document Description')"/>
                  </xsl:element>
                  
                </xsl:element>

              </xsl:element>
            </xsl:element>


            <!-- The following is added to bypass the validation in Enterprise.
                 If the client move to CW1, it can be removed. -->
            <ns0:ContextCollection>
              <ns0:Context>
                <ns0:Type></ns0:Type>
                <ns0:Value></ns0:Value>
              </ns0:Context>
            </ns0:ContextCollection>

          </xsl:element>
        </xsl:element>

      </Body>
    </ns0:UniversalInterchange>
  </xsl:template>


  <xsl:template name="MapValueIfNotEmpty">
    <xsl:param name="NodeName" />
    <xsl:param name="Value" />

    <xsl:if test="$Value != '' and string-length($Value) != 0">
      <xsl:choose>

        <xsl:when test="contains($NodeName, '/')">
          <xsl:element name="{substring-before($NodeName, '/')}">
            <xsl:call-template name="MapValueIfNotEmpty">
              <xsl:with-param name="NodeName" select="substring-after($NodeName, '/')" />
              <xsl:with-param name="Value" select="$Value" />
            </xsl:call-template>
          </xsl:element>
        </xsl:when>

        <xsl:otherwise>
          <xsl:element name="{$NodeName}">
            <xsl:value-of select="$Value" />
          </xsl:element>
        </xsl:otherwise>

      </xsl:choose>
    </xsl:if>

  </xsl:template>

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <msxsl:using namespace="System.IO" />
    <![CDATA[

public string GetFileName(string path)
{
  return Path.GetFileName(path);
}

]]>
  </msxsl:script>

</xsl:stylesheet>

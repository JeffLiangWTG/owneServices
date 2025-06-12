<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var" exclude-result-prefixes="msxsl var s0" version="1.0"
                xmlns:s0="http://cargowise.com/ehub/core/2013/08"
                xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
                xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1"
                xmlns:ScriptNS2="http://schemas.microsoft.com/BizTalk/2003/ScriptNS2"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />

  <xsl:template match="/">
    <xsl:apply-templates select="/*" />
  </xsl:template>

  <xsl:template match="/*">

    <xsl:element name="ns0:UniversalInterchange">

      <xsl:element name="Header">
        <xsl:element name="SenderID">
          <xsl:text></xsl:text>
        </xsl:element>
        <xsl:element name="RecipientID">
          <xsl:text></xsl:text>
        </xsl:element>
      </xsl:element>

      <xsl:element name="Body">

        <xsl:element name="UniversalEvent">

          <xsl:element name="Event">

            <xsl:element name="EventTime">
              <xsl:value-of select="ScriptNS1:CurrentDateTimeWithTimeZone()" />
            </xsl:element>

            <xsl:element name="EventType">
              <xsl:value-of select="ScriptNS0:GetRecipientCodeUnkeyed('AGCUJVUJV_HLP', 'AGCUJVUJV', 'HLS PDF-files - Receive eDocs', 'Defaults', 'Event Code')" />
            </xsl:element>

            <xsl:call-template name="MapValueIfNotEmpty">
              <xsl:with-param name="NodeName" select="'EventReference'" />
              <xsl:with-param name="Value" select="ScriptNS0:GetRecipientCodeUnkeyed('AGCUJVUJV_HLP', 'AGCUJVUJV', 'HLS PDF-files - Receive eDocs', 'Defaults', 'Event Reference')" />
            </xsl:call-template>

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
            <xsl:element name="ContextCollection">

              <xsl:variable name="DataTargetType" select="ScriptNS0:GetRecipientCode('AGCUJVUJV_HLP', 'AGCUJVUJV', 'HLS PDF-files - Receive eDocs', 'Doc Type', 'Job Type', $Filename)" />
              <xsl:variable name="Key" select="substring-before(substring-after($Filename, '_'), '_')" />
              <xsl:choose>
                <xsl:when test="$DataTargetType = 'Consol'">

                  <xsl:element name="Context">
                    <xsl:element name="Type">
                      <xsl:text>MAWBNumber</xsl:text>
                    </xsl:element>
                    <xsl:element name="Value">
                      <xsl:value-of select="$Key" />
                    </xsl:element>
                  </xsl:element>

                  <xsl:element name="Context">
                    <xsl:element name="Type">
                      <xsl:text>MBOLNumber</xsl:text>
                    </xsl:element>
                    <xsl:element name="Value">
                      <xsl:value-of select="$Key" />
                    </xsl:element>
                  </xsl:element>

                </xsl:when>
                <xsl:otherwise>

                  <xsl:element name="Context">
                    <xsl:element name="Type">
                      <xsl:text>HAWBNumber</xsl:text>
                    </xsl:element>
                    <xsl:element name="Value">
                      <xsl:value-of select="$Key" />
                    </xsl:element>
                  </xsl:element>

                  <xsl:element name="Context">
                    <xsl:element name="Type">
                      <xsl:text>HBOLNumber</xsl:text>
                    </xsl:element>
                    <xsl:element name="Value">
                      <xsl:value-of select="$Key" />
                    </xsl:element>
                  </xsl:element>

                </xsl:otherwise>
              </xsl:choose>

            </xsl:element>

            <xsl:element name="AttachedDocumentCollection">
              <xsl:element name="AttachedDocument">

                <xsl:element name="FileName">
                  <xsl:value-of select="$Filename" />
                </xsl:element>

                <xsl:element name="ImageData">
                  <xsl:value-of select="*[local-name()='FileStream']" />
                </xsl:element>

                <xsl:element name="Type">
                  <xsl:element name="Code">
                    <xsl:value-of select="substring($Filename, 1, 3)" />
                  </xsl:element>
                </xsl:element>

              </xsl:element>

            </xsl:element>

          </xsl:element>

        </xsl:element>

      </xsl:element>

    </xsl:element>

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

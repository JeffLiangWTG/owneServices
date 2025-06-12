<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                exclude-result-prefixes="msxsl userCSharp ns0" version="1.0"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2012/11"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes" />

  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="ns0:NoteText">
    <xsl:variable name="value">
      <xsl:call-template name="GetElementValue">
        <xsl:with-param name="elementValue" select="." />
        <xsl:with-param name="preserveNewLines" select="'true'" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:copy>
      <xsl:value-of select="$value"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="node()/text()">
    <xsl:call-template name="GetElementValue">
      <xsl:with-param name="elementValue" select="." />
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="GetElementValue">
    <xsl:param name="elementValue" />
    <xsl:param name="preserveNewLines" select="'false'" />

    <xsl:if test="$elementValue != ''">
      <xsl:variable name="cleanedValue" select="userCSharp:CleanUpUnicode($elementValue)"/>
      <xsl:choose>
        <xsl:when test="$preserveNewLines='true'">
          <xsl:value-of select="$cleanedValue"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="normalize-space($cleanedValue)"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>
  </xsl:template>

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[

  public string CleanUpUnicode(string inputData)
  { 
    return inputData.Replace((char)0x09, ' ')                 //Horizontal Tab
                    .Replace((char)0x00A0, ' ')               //No Break Space
                    .Replace((char)0x2012, '-')               //Figure Dash
                    .Replace((char)0x2013, '-')               //En Dash
                    .Replace((char)0x2014, '-')               //Em Dash
                    .Replace((char)0x2E3A, '-')               //Two-Em Dash
                    .Replace((char)0x2E3B, '-')               //Three-Em Dash
                    .Replace(((char)0xB0).ToString(), "")     //Degree Celsius
                    .Replace(((char)0xBA).ToString(), "")     //Latin 1 MASCULINE ORDINAL INDICATOR
                    .Replace(((char)0x00B4).ToString(), "'"); //Latin1 '
  }

]]>
  </msxsl:script>
</xsl:stylesheet>

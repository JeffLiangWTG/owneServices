<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
    <xsl:output method="xml" indent="yes"/>

      <xsl:template name="VoyageNo">
        <xsl:param name="VoyageNo"/>

        <xsl:if test="$VoyageNo != ''">
          <xsl:element name="ns0:VoyageNo">
            <xsl:variable name="LastChar" select="substring($VoyageNo, string-length($VoyageNo), 1)"/>
            <xsl:variable name="RemoveDirection" select="ScriptNS0:GetRecipientCodeUnkeyed('QUABNEBNE','PFLPNZHST','E2E - Import of Ships Agency Bills of Lading','Defaults','Remove Voyage Direction')"/>
            <xsl:choose>
              <xsl:when test="$RemoveDirection = 'Y' and ($LastChar = 'N' or $LastChar = 'S' or $LastChar = 'E' or $LastChar = 'W')">
                <xsl:value-of select="substring($VoyageNo, 1, string-length($VoyageNo) - 1)"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$VoyageNo"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:element>
        </xsl:if>
      </xsl:template>

</xsl:stylesheet>

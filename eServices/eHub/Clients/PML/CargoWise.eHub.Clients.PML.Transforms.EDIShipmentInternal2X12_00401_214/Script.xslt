<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
  <xsl:output method="xml" indent="yes"/>

  <xsl:template match="@* | node()">

    
    
    <xsl:variable name="Shipment" select="*[local-name()='Payload']/*[local-name()='Shipments']/*[local-name()='Shipment']" />
    
    <ns0:AT7Loop1 xmlns:ns0="http://schemas.microsoft.com/BizTalk/EDI/X12/2006" >
      <xsl:for-each select="$Shipment">
        <ns0:AT7>
          <AT701>
            <xsl:variable name="Purpose" select="/*[local-name()='ShipmentsInternal']/*[local-name()='InterchangeInfo']/*[local-name()='Source']/*[local-name()='Purpose']"/>
            <xsl:value-of select="ScriptNS0:GetRecipientCode(&quot;PMLDFWCAX&quot; , &quot;PMLDFWCAX_PIE&quot; , &quot;Pier 1 ANSI 214 - Export Domestic Shipment Status&quot; , &quot;Status Code&quot; , &quot;Pier1 Status Code&quot; , string($Purpose))"/>
          </AT701>
          <AT702>NS</AT702>
          
            <xsl:variable name="DateTime" select="*[local-name()='Events']/*[local-name()='Event' and *[local-name()='TriggeredBy']/text() = 'true']/*[local-name()='DateTime']"/>
            <xsl:choose>
              <xsl:when test="$DateTime">
                <AT705>
                  <xsl:value-of select="userCSharp:FormatDateTime($DateTime, &quot;yyyy-MM-ddTHH:mm:ss&quot; , &quot;yyyyMMdd&quot;)"/>
                </AT705>
                <AT706>
                  <xsl:value-of select="userCSharp:FormatDateTime($DateTime, &quot;yyyy-MM-ddTHH:mm:ss&quot; , &quot;HHmmss&quot;)"/>
                </AT706>
              </xsl:when>
              <xsl:otherwise>
                <AT705>
                  <xsl:value-of select="userCSharp:FormatDateTime(/*[local-name()='ShipmentsInternal']/*[local-name()='InterchangeInfo']/*[local-name()='Date'], &quot;yyyy-MM-ddTHH:mm:ss&quot; , &quot;yyyyMMdd&quot;)"/>
                </AT705>
                <AT706>
                  <xsl:value-of select="userCSharp:FormatDateTime(/*[local-name()='ShipmentsInternal']/*[local-name()='InterchangeInfo']/*[local-name()='Date'], &quot;yyyy-MM-ddTHH:mm:ss&quot; , &quot;HHmmss&quot;)"/>
                </AT706>
              </xsl:otherwise>
              
            </xsl:choose>
          
          <AT707>PT</AT707>
        </ns0:AT7>
      </xsl:for-each>

    </ns0:AT7Loop1>



  </xsl:template>
</xsl:stylesheet>

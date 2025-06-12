<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                exclude-result-prefixes="ScriptNS0 ScriptNS1"
                version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
                xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />
  <xsl:template match="/">
    <xsl:apply-templates select="/s0:UniversalShipment/s0:Shipment" />
  </xsl:template>

  <xsl:template match="s0:Shipment">
    <s0:UniversalInterchange>
      <s0:Body>
        <s0:UniversalEvent>
          <s0:Event>
            <s0:DataContext>
              <s0:DataTargetCollection>
                <xsl:for-each select="s0:DataContext/s0:DataSourceCollection/s0:DataSource">
                  <s0:DataTarget>
                    <s0:Key>
                      <xsl:value-of select="s0:Key" />
                    </s0:Key>
                    <s0:Type>
                      <xsl:value-of select="s0:Type" />
                    </s0:Type>
                  </s0:DataTarget>
                </xsl:for-each>
              </s0:DataTargetCollection>
            </s0:DataContext>
            <s0:EventTime>
              <xsl:value-of select="ScriptNS1:CurrentDateTime('s')" />
            </s0:EventTime>
            <s0:EventType>
              <xsl:value-of select="ScriptNS0:GetContextProperty('ErrorCode', 'http://cargowise.com/ehub/routing/2010/06')" />
            </s0:EventType>
            <s0:EventParameters>
              <xsl:call-template name="AddReferenceParameters">
                <xsl:with-param name="text" select="ScriptNS0:GetContextProperty('ErrorDescription', 'http://cargowise.com/ehub/routing/2010/06')"/>
              </xsl:call-template>
              <s0:MessageType>
                <xsl:text>eBond Message to Surety Agent</xsl:text>
              </s0:MessageType>
            </s0:EventParameters>
          </s0:Event>
        </s0:UniversalEvent>
      </s0:Body>
    </s0:UniversalInterchange>
  </xsl:template>

  <xsl:template name="AddReferenceParameters">
    <xsl:param name="text"/>
    <xsl:variable name="separator" select="'|'"/>
    <xsl:choose>
      <xsl:when test="not(contains($text, $separator))">
        <xsl:call-template name="AddParameters">
          <xsl:with-param name="text" select="normalize-space($text)"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="AddParameters">
          <xsl:with-param name="text" select="normalize-space(substring-before($text, $separator))"/>
        </xsl:call-template>
        <xsl:call-template name="AddReferenceParameters">
          <xsl:with-param name="text" select="substring-after($text, $separator)"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="AddParameters">
    <xsl:param  name="text"/>
    <xsl:variable name="separator" select="'='"/>
    <xsl:variable name="elementName" select="normalize-space(substring-before($text, $separator))"/>
    <xsl:variable name="elementValue" select="normalize-space(substring-after($text, $separator))"/>
    <xsl:if test="$elementName != '' and $elementValue != ''">
      <xsl:element name="s0:{$elementName}">
        <xsl:value-of select="$elementValue" />
      </xsl:element>
    </xsl:if>
  </xsl:template>

</xsl:stylesheet>
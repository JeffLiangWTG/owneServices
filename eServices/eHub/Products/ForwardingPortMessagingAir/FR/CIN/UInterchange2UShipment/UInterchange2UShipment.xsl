<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes" />
  <xsl:template match="/">
    <xsl:apply-templates select="*[local-name()='UniversalInterchange']/*[local-name()='Body']/*" />
  </xsl:template>
  <xsl:template match="*">
    <xsl:element name="{local-name(.)}" namespace="{namespace-uri()}" >
      <xsl:copy-of select="@*"/>
      <xsl:apply-templates/>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>
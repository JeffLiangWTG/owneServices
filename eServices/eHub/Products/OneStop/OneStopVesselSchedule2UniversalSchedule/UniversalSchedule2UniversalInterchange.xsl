<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2011/11"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
  <xsl:output method="xml" indent="yes" omit-xml-declaration="yes"/>

  <xsl:key name="voyage" match="ns0:Schedule" use="concat(ns0:Carrier/ns0:OrganizationCode, '|', ns0:Transport/ns0:Sea/ns0:Vessel/ns0:LloydsNumber, '|', ns0:Transport/ns0:Sea/ns0:VoyageNumber)"/>

  <xsl:template match="/">
    <xsl:apply-templates select="/ns0:UniversalSchedule" />
  </xsl:template>
  <xsl:template match="/ns0:UniversalSchedule">
    <ns0:UniversalInterchange>
      <Header>
        <SenderID/>
        <RecipientID/>
      </Header>
      <Body>
        <xsl:for-each select="ns0:Schedule[count(. | key('voyage', concat(ns0:Carrier/ns0:OrganizationCode, '|', ns0:Transport/ns0:Sea/ns0:Vessel/ns0:LloydsNumber, '|', ns0:Transport/ns0:Sea/ns0:VoyageNumber))[1]) = 1]">
          <xsl:sort select="ns0:Carrier/ns0:OrganizationCode"/>
          <ns0:UniversalSchedule>
            <ns0:Schedule>
              <xsl:copy-of select="ns0:DataProvider"/>
              <xsl:copy-of select="ns0:IsCancellation"/>
              <xsl:copy-of select="ns0:Carrier"/>
              <xsl:copy-of select="ns0:Transport"/>
              <ns0:DischargeCollection>
                <xsl:for-each select="key('voyage', concat(ns0:Carrier/ns0:OrganizationCode, '|', ns0:Transport/ns0:Sea/ns0:Vessel/ns0:LloydsNumber, '|', ns0:Transport/ns0:Sea/ns0:VoyageNumber))/ns0:DischargeCollection/ns0:Discharge">
                  <xsl:copy-of select="."/>
                </xsl:for-each>
              </ns0:DischargeCollection>
              <ns0:LoadingCollection>
                <xsl:for-each select="key('voyage', concat(ns0:Carrier/ns0:OrganizationCode, '|', ns0:Transport/ns0:Sea/ns0:Vessel/ns0:LloydsNumber, '|', ns0:Transport/ns0:Sea/ns0:VoyageNumber))/ns0:LoadingCollection/ns0:Loading">
                  <xsl:copy-of select="."/>
                </xsl:for-each>
              </ns0:LoadingCollection>
            </ns0:Schedule>
          </ns0:UniversalSchedule>
        </xsl:for-each>
      </Body>
    </ns0:UniversalInterchange>
  </xsl:template>

</xsl:stylesheet>

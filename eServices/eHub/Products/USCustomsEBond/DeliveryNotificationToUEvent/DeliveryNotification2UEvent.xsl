<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                exclude-result-prefixes="ns0 ns1 ScriptNS0 "
                version="1.0"
                xmlns:ns0="http://cargowise.com/ehub/core/2018/06"
                xmlns:ns1="http://www.cargowise.com/Schemas/Universal/2012/11"
                xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />
  <xsl:template match="/">
    <xsl:apply-templates select="/ns0:DeliveryNotificationMessage" />
  </xsl:template>
  <xsl:template match="ns0:DeliveryNotificationMessage">
    <xsl:apply-templates select="//*[local-name()='UniversalShipment']" />
  </xsl:template>

  <xsl:template match="//*[local-name()='UniversalShipment']">
    <xsl:variable name="consolNumber" select="//*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='DataContext']//*[local-name()='DataSource'][1]/*[local-name()='Key']/text()" />
    <xsl:variable name="dataType" select="//*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='DataContext']//*[local-name()='DataSource'][1]/*[local-name()='Type']/text()" />
    <xsl:variable name="entryFilerCode" select="substring(normalize-space(//*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][*[local-name()='Key']/text()='EntryFilerCode'][1]/*[local-name()='Value']/text()),1,3)"/>
    <xsl:variable name="transactionID">
      <xsl:value-of select="$entryFilerCode"/>
      <xsl:value-of select="substring(normalize-space(//*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='EntryNumberCollection']/*[local-name()='EntryNumber'][*[local-name()='Type']/*[local-name()='Code']/text()='ENS'][1]/*[local-name()='Number']/text()),1,8)"/>
    </xsl:variable>

    <ns1:UniversalInterchangeInclude>
      <ns1:Body>
        <ns1:UniversalEvent>
          <ns1:Event>
            <ns1:DataContext>
              <ns1:DataTargetCollection>
                <ns1:DataTarget>
                  <ns1:Key>
                    <xsl:value-of select="$consolNumber" />
                  </ns1:Key>
                  <ns1:Type>
                    <xsl:value-of select="$dataType" />
                  </ns1:Type>
                </ns1:DataTarget>
              </ns1:DataTargetCollection>
            </ns1:DataContext>
            <ns1:EventTime>
              <xsl:value-of select="ScriptNS0:CurrentDateTime('s')" />
            </ns1:EventTime>
            <ns1:EventType>IRJ</ns1:EventType>
            <ns1:EventParameters>
              <xsl:variable name="errorDescription" select="/*[local-name()='DeliveryNotificationMessage']/*[local-name()='ErrorDescription']/text()" />
              <ns1:Department>WiseTechGlobal</ns1:Department>
              <ns1:Reason>
                <xsl:value-of select="$errorDescription" />
              </ns1:Reason>
            </ns1:EventParameters>
            <ns1:ContextCollection>
              <ns1:Context>
                <xsl:value-of select="$transactionID" />
              </ns1:Context>
            </ns1:ContextCollection>
          </ns1:Event>
        </ns1:UniversalEvent>
      </ns1:Body>
    </ns1:UniversalInterchangeInclude>
    
  </xsl:template>
</xsl:stylesheet>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 CodeMapper ContextAccessor"  version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes"/>

  <xsl:template match="/">
    <xsl:variable name="shipment" select="s0:UniversalInterchange/s0:Body/s0:UniversalShipment/s0:Shipment"/>

    <xsl:variable name="transportBooking" select="$shipment/s0:SubShipmentCollection/s0:SubShipment[s0:DataContext/s0:DataSourceCollection/s0:DataSource/s0:Type='TransportBooking']"/>
    <xsl:variable name="portCode" select="$transportBooking/s0:InstructionCollection/s0:Instruction[1]/s0:Address/s0:Port/s0:Code/text()"/>

    <xsl:variable name="recipientID" select="CodeMapper:GetRecipientCode('CONTAINER_TRANSPORT_OPTIMIZATION', 'CONTAINER_TRANSPORT_OPTIMIZATION', 'CONTAINER_TRANSPORT_OPTIMIZATION Configuration', 'Port Settings', 'RecipientId', $portCode)"/>
    <xsl:if test="$recipientID != ''">
      <xsl:variable name="SetRecipientID" select="ContextAccessor:SetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties', $recipientID)"/>
    </xsl:if>
    <xsl:apply-templates select="@*|node()"/>
  </xsl:template>

  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>

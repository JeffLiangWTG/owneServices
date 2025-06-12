<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var" exclude-result-prefixes="msxsl var s0 userCSharp ScriptNS0 ScriptNS1" version="1.0" xmlns:ns0="http://cargowise.com/ehub/clients/edi/2010/12" xmlns:s0="http://www.cargowise.com/Schemas/Universal/2011/11" xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp" xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0" xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />

  <xsl:template match="@* | node()">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()"/>
    </xsl:copy>
  </xsl:template>

  <!--Scripts begins from here-->

  <xsl:template name="WriteOptionalRecords">
    <xsl:element name="OptionalRecords">
      <xsl:variable name="smallcase" select="'abcdefghijklmnopqrstuvwxyz'" />
      <xsl:variable name="uppercase" select="'ABCDEFGHIJKLMNOPQRSTUVWXYZ'" />

      <xsl:variable name="quantityDetails" select="userCSharp:GetQuantityDetails()" />

      <xsl:variable name="actionPurposeCode" select="/*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='DataContext']/*[local-name()='ActionPurpose']/*[local-name()='Code']/text()"/>
      <xsl:variable name="originalStatusCode" select="ScriptNS0:GetRecipientCode('PRSJFKJFK','CCSHMIHMI','Champ FSU - Generate Shipment Status Messages','Status','FSU Status Code',$actionPurposeCode)"/>
      <xsl:variable name="statusCode" select="translate($originalStatusCode,$smallcase,$uppercase)"/>

      <xsl:choose>
        <xsl:when test="$statusCode='DLV'">
          <xsl:variable name="actualDeliveryDateTime" select="*[local-name()='LocalProcessing']/*[local-name()='DeliveryCartageCompleted']/text()"/>
          <xsl:variable name="dayOfActualDelivery" select="ScriptNS1:FormatXmlDateTime($actualDeliveryDateTime, 'dd')"/>
          <xsl:variable name="monthOfActualDelivery" select="translate(ScriptNS1:FormatXmlDateTime($actualDeliveryDateTime, 'MMM'),$smallcase,$uppercase)"/>
          <xsl:variable name="timeOfActualDelivery" select="ScriptNS1:FormatXmlDateTime($actualDeliveryDateTime, 'HHmm')" />
          <xsl:variable name="portOfDestination" select="*[local-name()='PortOfDestination']/*[local-name()='Code']/text()"/>
          <xsl:variable name="airportOfDelivery" select="ScriptNS0:GetRecipientCode('PRSJFKJFK','CCSHMIHMI','Champ FSU - Generate Shipment Status Messages','Airport/City','IATA Code',$portOfDestination)"/>
          <xsl:variable name="localCartageDeliverToAddress" select="*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][*[local-name()='AddressType']/text()='LocalCartageDeliverToAddress']/*[local-name()='CompanyName']/text()"/>
          <xsl:element name="DLV">
            <xsl:element name="DateTimeOfDelivery">
              <xsl:element name="Day">
                <xsl:value-of select="$dayOfActualDelivery"/>
              </xsl:element>
              <xsl:element name="Month">
                <xsl:value-of select="$monthOfActualDelivery"/>
              </xsl:element>
              <xsl:element name="Time">
                <xsl:value-of select="$timeOfActualDelivery"/>
              </xsl:element>
            </xsl:element>
            <xsl:element name="AirportOfDelivery">
              <xsl:value-of select="substring($airportOfDelivery,1,3)"/>
            </xsl:element>
            <xsl:element name="QuantityDetails">
              <xsl:element name="QuantityDetails">
                <xsl:value-of select="$quantityDetails"/>
              </xsl:element>
            </xsl:element>
            <xsl:element name="DeliveryDetails">
              <xsl:value-of select="substring($localCartageDeliverToAddress,1,35)"/>
            </xsl:element>
          </xsl:element>
        </xsl:when>
      </xsl:choose>
    </xsl:element>
  </xsl:template>

</xsl:stylesheet>

<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2011/11" xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"  xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp" xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1"
  <xsl:output method="xml" indent="yes"/>

  <xsl:template match="@* | node()">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()"/>
    </xsl:copy>
  </xsl:template>

  <!--Scripts begins from here-->

  <xsl:template name="WriteTransportLegCollectionFromRouting">
    <xsl:param name="portOfOrigin"/>
    <xsl:param name="portOfDestination"/>
    <xsl:param name="carrierCode"/>
    <xsl:param name="voyageFlightNo"/>
    <xsl:element name="ns0:TransportLegCollection">
      <xsl:call-template name="WriteTransportLegRaw">
        <xsl:with-param name="portOfDischargeCode" select="$portOfDestination"/>
        <xsl:with-param name="portOfLoadingCode" select="$portOfOrigin"/>
        <xsl:with-param name="legOrder" select="'0'"/>
        <xsl:with-param name="carrierCode" select="$carrierCode"/>
        <xsl:with-param name="voyageFlightNo" select="$voyageFlightNo"/>
      </xsl:call-template>
    </xsl:element>
  </xsl:template>

  <xsl:template name="WriteTransportLegRaw">
    <xsl:param name="portOfDischargeCode"/>
    <xsl:param name="portOfLoadingCode"/>
    <xsl:param name="legOrder"/>
    <xsl:param name="carrierCode"/>
    <xsl:param name="voyageFlightNo" select="''"/>
    <xsl:element name="ns0:TransportLeg">
      <xsl:element name="ns0:PortOfDischarge">
        <xsl:element name="ns0:Code">
          <xsl:value-of select="$portOfDischargeCode"/>
        </xsl:element>
      </xsl:element>
      <xsl:element name="ns0:PortOfLoading">
        <xsl:element name="ns0:Code">
          <xsl:value-of select="$portOfLoadingCode"/>
        </xsl:element>
      </xsl:element>
      <xsl:element name="ns0:LegOrder">
        <xsl:value-of select="$legOrder"/>
      </xsl:element>
      <xsl:element name="ns0:TransportMode">Air</xsl:element>
      <xsl:if test="normalize-space($carrierCode)!=''">
        <xsl:element name="ns0:Carrier">
          <xsl:element name="ns0:AddressType">Carrier</xsl:element>
          <xsl:element name="ns0:OrganizationCode">
            <xsl:value-of select="$carrierCode"/>
          </xsl:element>
        </xsl:element>
      </xsl:if>
      <xsl:if test="normalize-space($voyageFlightNo)!=''">
        <xsl:element name="ns0:VoyageFlightNo">
          <xsl:value-of select="$voyageFlightNo"/>
        </xsl:element>
      </xsl:if>
    </xsl:element>
  </xsl:template>

  <xsl:template name="WriteOrganizationAddress">
    <xsl:param name="addressType"/>
    <xsl:param name="address1"/>
    <xsl:param name="city"/>
    <xsl:param name="companyName"/>
    <xsl:param name="countryCode"/>
    <xsl:param name="phone"/>
    <xsl:param name="postCode"/>
    <xsl:param name="state"/>
    <xsl:call-template name="WriteOrganizationAddressRaw">
      <xsl:with-param name="addressType" select="$addressType"/>
      <xsl:with-param name="address1" select="$address1"/>
      <xsl:with-param name="city" select="$city"/>
      <xsl:with-param name="companyName" select="$companyName"/>
      <xsl:with-param name="countryCode" select="$countryCode"/>
      <xsl:with-param name="phone" select="$phone"/>
      <xsl:with-param name="postCode" select="$postCode"/>
      <xsl:with-param name="state" select="$state"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteOrganizationAddressRaw">
    <xsl:param name="addressType"/>
    <xsl:param name="address1"/>
    <xsl:param name="city"/>
    <xsl:param name="companyName"/>
    <xsl:param name="countryCode"/>
    <xsl:param name="phone"/>
    <xsl:param name="postCode"/>
    <xsl:param name="state"/>
    <xsl:element name="ns0:OrganizationAddress">
      <xsl:element name="ns0:AddressType">
        <xsl:value-of select="$addressType"/>
      </xsl:element>
      <xsl:element name="ns0:Address1">
        <xsl:value-of select="$address1"/>
      </xsl:element>
      <xsl:element name="ns0:City">
        <xsl:value-of select="$city"/>
      </xsl:element>
      <xsl:element name="ns0:CompanyName">
        <xsl:value-of select="$companyName"/>
      </xsl:element>
      <xsl:element name="ns0:Country">
        <xsl:element name="ns0:Code">
          <xsl:value-of select="$countryCode"/>
        </xsl:element>
      </xsl:element>
      <xsl:element name="ns0:Phone">
        <xsl:value-of select="$phone"/>
      </xsl:element>
      <xsl:element name="ns0:Postcode">
        <xsl:value-of select="$postCode"/>
      </xsl:element>
      <xsl:element name="ns0:State">
        <xsl:value-of select="$state"/>
      </xsl:element>
    </xsl:element>
  </xsl:template>

</xsl:stylesheet>

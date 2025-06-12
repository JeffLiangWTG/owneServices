<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:output method="xml" indent="yes"/>

  <xsl:template match="@* | node()">


    <xsl:variable name="ShipFrom" select="//*[local-name()='Consol']
                  /*[local-name()='Shipments']
                  /*[local-name()='Shipment']
                  /*[local-name()='ShipmentDetails']
                  /*[local-name()='Consignor']"/>
    <xsl:call-template name="GenerateContact">
      <xsl:with-param name="Role" select="'shipFrom'"/>
      <xsl:with-param name="Contact" select="$ShipFrom"/>
    </xsl:call-template>

    <xsl:variable name="ShipTo" select="//*[local-name()='Consol']
                  /*[local-name()='Shipments']
                  /*[local-name()='Shipment']
                  /*[local-name()='ShipmentDetails']
                  /*[local-name()='Consignee']"/>
    <xsl:call-template name="GenerateContact">
      <xsl:with-param name="Role" select="'shipTo'"/>
      <xsl:with-param name="Contact" select="$ShipTo"/>
    </xsl:call-template>


    <!-- ShipControl -->
    <xsl:element name="ShipControl">
      <xsl:variable name="SCAC" select="//*[local-name()='Consol']
                            /*[local-name()='ConsolDetail']
                            /*[local-name()='Carrier']
                            /*[local-name()='OrganisationDetails']
                            /*[local-name()='RegistrationNumbers']
                            /*[local-name()='RegistrationNumber' and *[local-name()='CountryOfRegistration']/text()='US' and *[local-name()='NumberType']/text()='CCC']
                            /*[local-name()='Number']"/>
      <xsl:if test="$SCAC != ''">
        <CarrierIdentifier domain="SCAC">
          <xsl:value-of select="$SCAC"/>
        </CarrierIdentifier>
      </xsl:if>
      <CarrierIdentifier domain="companyName">
        <xsl:value-of select="//*[local-name()='Consol']
                            /*[local-name()='ConsolDetail']
                            /*[local-name()='Carrier']
                            /*[local-name()='OrganisationDetails']
                            /*[local-name()='Name']"/>
      </CarrierIdentifier>
      <ShipmentIdentifier>
        <xsl:value-of select="//*[local-name()='Consol']
                      /*[local-name()='Shipments']
                      /*[local-name()='Shipment']
                      /*[local-name()='ShipmentIdentifier'][@ShipmentIdentifierType='Housebill']"/>
      </ShipmentIdentifier>
    </xsl:element>

    <xsl:for-each select="*[local-name()='OrderLines']/*[local-name()='OrderLine']">
      <xsl:element name="Extrinsic">
        <xsl:attribute name="name">supplierPartNum</xsl:attribute>
        <xsl:value-of select="*[local-name()='OrderLineDetail']/*[local-name()='Product']"/>
      </xsl:element>
    </xsl:for-each>

    <xsl:element name="Extrinsic">
      <xsl:attribute name="name">shipmentNum</xsl:attribute>
      <xsl:value-of select="userCSharp:GetSequenceNum()"/>
    </xsl:element>

    <xsl:variable name="prefix" select="*[local-name()='OrderDetail']/*[local-name()='Custom']/*[local-name()='Text1']"/>
    <xsl:variable name="OrderNum" select="*[local-name()='OrderIdentifier']/*[local-name()='OrderNumber']"/>
    <xsl:attribute name="orderID">
      <xsl:choose>
        <xsl:when test="$prefix != ''">
          <xsl:value-of select="concat($prefix, '.', $OrderNum)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$OrderNum"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:attribute>
    
  </xsl:template>


  <xsl:template name="GenerateContact">
    <xsl:param name="Role"/>
    <xsl:param name="Contact"/>

    <xsl:element name="Contact">
      <xsl:attribute name="role">
        <xsl:value-of select="$Role"/>
      </xsl:attribute>
      <xsl:attribute name="addressID">
        <xsl:value-of select="$Contact/@*[local-name()='OwnerCode']"/>
      </xsl:attribute>
      <xsl:element name="Name">
        <xsl:value-of select="$Contact/*[local-name()='OrganisationDetails']/*[local-name()='Name']"/>
      </xsl:element>
      <xsl:element name="PostalAddress">
        <xsl:element name="Street">
          <xsl:value-of select="$Contact/*[local-name()='OrganisationDetails']/*[local-name()='Addresses']/*[local-name()='Address']/*[local-name()='AddressLine1']"/>
        </xsl:element>
        <xsl:if test="$Contact/*[local-name()='OrganisationDetails']/*[local-name()='Addresses']/*[local-name()='Address']/*[local-name()='AddressLine2']">
          <xsl:element name="Street">
            <xsl:value-of select="$Contact/*[local-name()='OrganisationDetails']/*[local-name()='Addresses']/*[local-name()='Address']/*[local-name()='AddressLine2']"/>
          </xsl:element>
        </xsl:if>
        <xsl:element name="City">
          <xsl:value-of select="$Contact/*[local-name()='OrganisationDetails']/*[local-name()='Addresses']/*[local-name()='Address']/*[local-name()='CityOrSuburb']"/>
        </xsl:element>
        <xsl:element name="State">
          <xsl:value-of select="$Contact/*[local-name()='OrganisationDetails']/*[local-name()='Addresses']/*[local-name()='Address']/*[local-name()='StateOrProvince']"/>
        </xsl:element>
        <xsl:element name="PostalCode">
          <xsl:value-of select="$Contact/*[local-name()='OrganisationDetails']/*[local-name()='Addresses']/*[local-name()='Address']/*[local-name()='PostCode']"/>
        </xsl:element>
        <xsl:element name="Country">
          <xsl:attribute name="isoCountryCode">
            <xsl:value-of select="substring($Contact/*[local-name()='OrganisationDetails']/*[local-name()='Location'], 1, 2)"/>
          </xsl:attribute>
          <xsl:value-of select="$Contact/*[local-name()='OrganisationDetails']/*[local-name()='Location']/@*[local-name()='Country']"/>
        </xsl:element>
      </xsl:element>

      <xsl:variable name="Phone" select="$Contact/*[local-name()='OrganisationDetails']
                    /*[local-name()='Addresses']
                    /*[local-name()='Address']
                    /*[local-name()='TelephoneNumbers']
                    /*[local-name()='TelephoneNumber' and @*[local-name()='NumberType']='Business']"/>
      <xsl:if test="$Phone != ''">
        <xsl:element name="Phone">
          <xsl:element name="TelephoneNumber">
            <xsl:element name="Number">
              <xsl:value-of select="$Phone"/>
            </xsl:element>
          </xsl:element>
        </xsl:element>
      </xsl:if>

    </xsl:element>
  </xsl:template>


</xsl:stylesheet>

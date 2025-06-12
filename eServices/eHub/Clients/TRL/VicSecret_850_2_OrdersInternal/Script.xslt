<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
  <xsl:output method="xml" indent="yes"/>





  <xsl:template name="generateAddress">
    <xsl:param name="Address"/>

    <xsl:element name="ns0:OrganisationDetails">

      <xsl:element name="ns0:Name">
        <xsl:value-of select="$Address/*/*[local-name()='N102']"/>
      </xsl:element>

      <xsl:element name="ns0:Addresses">
        <xsl:element name="ns0:Address">
          <xsl:element name="ns0:AddressLine1">
            <xsl:value-of select="$Address/*/*[local-name()='N301']"/>
          </xsl:element>

          <xsl:if test="$Address/*/*[local-name()='N302'] != ''">
            <xsl:element name="ns0:AddressLine2">
              <xsl:value-of select="$Address/*/*[local-name()='N302']"/>
            </xsl:element>
          </xsl:if>
          <xsl:if test="$Address/*/*[local-name()='N401'] != ''">
            <xsl:element name="ns0:CityOrSuburb">
              <xsl:value-of select="$Address/*/*[local-name()='N401']"/>
            </xsl:element>
          </xsl:if>
          <xsl:if test="$Address/*/*[local-name()='N402'] != ''">
            <xsl:element name="ns0:StateOrProvince">
              <xsl:value-of select="$Address/*/*[local-name()='N402']"/>
            </xsl:element>
          </xsl:if>
          <xsl:if test="$Address/*/*[local-name()='N403'] != ''">
            <xsl:element name="ns0:PostCode">
              <xsl:value-of select="$Address/*/*[local-name()='N403']"/>
            </xsl:element>
          </xsl:if>
        </xsl:element>
      </xsl:element>

    </xsl:element>
  </xsl:template>






  <xsl:template match="@* | node()">


    <xsl:variable name="ReqExWorksDate" select="//*[local-name()='DTM' and *[local-name()='DTM01']/text()='037']/*[local-name()='DTM02']" />
    <xsl:element name="ns0:ExWorksRequiredBy" >
      <xsl:value-of select="userCSharp:FormatDateTime($ReqExWorksDate, 'yyyyMMdd', 'yyyy-MM-ddTHH:mm:ss')"/>
    </xsl:element>


    <xsl:variable name="ReqInStoreDate" select="//*[local-name()='DTM' and *[local-name()='DTM01']/text()='002']/*[local-name()='DTM02']" />
    <xsl:element name="ns0:DeliveryRequiredBy">
      <xsl:value-of select="userCSharp:FormatDateTime($ReqInStoreDate, 'yyyyMMdd', 'yyyy-MM-ddTHH:mm:ss')"/>
    </xsl:element>


    <xsl:variable name="OriginReceivalDate" select="//*[local-name()='DTM' and *[local-name()='DTM01']/text()='038']/*[local-name()='DTM02']" />
    <xsl:element name="ns0:Estimated">
      <xsl:value-of select="userCSharp:FormatDateTime($OriginReceivalDate, 'yyyyMMdd', 'yyyy-MM-ddTHH:mm:ss')"/>
    </xsl:element>



    <!-- country of origin -->
    <xsl:variable name="OriginCountry" select="//*[local-name()='N1Loop1' and *[local-name()='N1']/*[local-name()='N101']/text() = 'CT']"/>
    <xsl:if test="$OriginCountry">
      <xsl:element name="ns0:CountryOfOrigin">
        <xsl:value-of select="$OriginCountry/*[local-name()='N1']/*[local-name()='N104']"/>
      </xsl:element>
    </xsl:if>

    

    <!-- buyer -->
    <xsl:variable name="BuyerAddress" select="//*[local-name()='N1Loop1' and *[local-name()='N1']/*[local-name()='N101']/text() = 'IM']"/>
    <xsl:if test="$BuyerAddress">
      <xsl:element name="ns0:Buyer">
        <xsl:attribute name="OwnerCode">
          <xsl:value-of select="$BuyerAddress/*[local-name()='N1']/*[local-name()='N104']"/>
        </xsl:attribute>

        <xsl:call-template name="generateAddress">
          <xsl:with-param name="Address" select="$BuyerAddress"/>
        </xsl:call-template>
      </xsl:element>
    </xsl:if>


    <!-- supplier -->
    <xsl:variable name="SupplierAddress" select="//*[local-name()='N1Loop3' and *[local-name()='N1_3']/*[local-name()='N101']/text() = 'SH']"/>
    <xsl:if test="$SupplierAddress">
      <xsl:element name="ns0:Supplier">
        <xsl:attribute name="OwnerCode">
          <xsl:value-of select="$SupplierAddress/*[local-name()='N1_3']/*[local-name()='N104']"/>
        </xsl:attribute>

        <xsl:call-template name="generateAddress">
          <xsl:with-param name="Address" select="$SupplierAddress"/>
        </xsl:call-template>
      </xsl:element>
    </xsl:if>



    <!-- delivery address -->
    <xsl:variable name="DeliveryAddress" select="//*[local-name()='N1Loop1' and *[local-name()='N1']/*[local-name()='N101']/text() = 'ST']"/>
    <xsl:if test="$DeliveryAddress">
      <xsl:element name="ns0:AddressFreeText">
        <xsl:value-of select="$DeliveryAddress/*[local-name()='N1']/*[local-name()='N104']"/>
      </xsl:element>
    </xsl:if>

    <xsl:variable name="DeliveryAddress" select="//*[local-name()='N1Loop1' and *[local-name()='N1']/*[local-name()='N101']/text() = 'ST']"/>
    <xsl:if test="$DeliveryAddress">
      <xsl:element name="ns0:AdditionalTerms">
        <xsl:value-of select="$DeliveryAddress/*[local-name()='N1']/*[local-name()='N104']"/>
      </xsl:element>
    </xsl:if>



    <!-- ultimate consignee address -->
    <xsl:variable name="UCAddress" select="//*[local-name()='N1Loop1' and *[local-name()='N1']/*[local-name()='N101']/text() = 'UC']"/>
    <xsl:if test="$UCAddress">
      <xsl:element name="ns0:Text1">
        <xsl:value-of select="$UCAddress/*[local-name()='N1']/*[local-name()='N104']"/>
      </xsl:element>
    </xsl:if>

    <xsl:if test="$UCAddress">
      <xsl:element name="ns0:Description">
        <xsl:value-of select="$UCAddress/*[local-name()='N1']/*[local-name()='N104']"/>
      </xsl:element>
    </xsl:if>
    
    
    <!-- AdditionalFieldsToUpdateCollection -->
    <xsl:element name="ns0:AdditionalFieldsToUpdateCollection">
      <xsl:element name="ns0:AdditionalFieldsToUpdate">
        <Type>JobOrderHeader.JD_IsCancelled</Type>
        <Value>true</Value>
      </xsl:element>
      <xsl:element name="ns0:AdditionalFieldsToUpdate">
        <Type>JobOrderHeader.JD_OrderStatus</Type>
        <Value>CAN</Value>
      </xsl:element>
    </xsl:element>
    
    
    

  </xsl:template>
</xsl:stylesheet>

<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
  <xsl:output method="xml" indent="yes"/>

  <xsl:template match="@* | node()">


    <xsl:element name="ns0:AddressLine1">
      <xsl:value-of select="//*[local-name()='OrderRequestHeader']/*[local-name()='ShipTo']/*[local-name()='Address']/*[local-name()='PostalAddress']/*[local-name()='Street'][1]"/>
    </xsl:element>

    <xsl:element name="ns0:AddressLine2">
      <xsl:value-of select="//*[local-name()='OrderRequestHeader']/*[local-name()='ShipTo']/*[local-name()='Address']/*[local-name()='PostalAddress']/*[local-name()='Street'][2]"/>
    </xsl:element>


    <xsl:variable name="Supplier" select="//*[local-name()='OrderRequest']/*[local-name()='ItemOut']/*[local-name()='SupplierList']/*[local-name()='Supplier']"/>
    <xsl:element name="ns0:Supplier">
      <xsl:attribute name="OwnerCode">
        <xsl:value-of select="$Supplier/*[local-name()='SupplierID']"/>
      </xsl:attribute>

      <xsl:element name="ns0:OrganisationDetails">

        <xsl:element name="ns0:Name">
          <xsl:value-of select="$Supplier/*[local-name()='Name']"/>
        </xsl:element>

        <xsl:element name="ns0:Addresses">
          <xsl:element name="ns0:Address">
            <xsl:element name="ns0:AddressLine1">
              <xsl:value-of select="$Supplier/*[local-name()='SupplierLocation']/*[local-name()='Address']/*[local-name()='PostalAddress']/*[local-name()='Street'][1]"/>
            </xsl:element>

            <xsl:variable name="Addr2" select="$Supplier/*[local-name()='SupplierLocation']/*[local-name()='Address']/*[local-name()='PostalAddress']/*[local-name()='Street'][2]"/>
            <xsl:if test="$Addr2 != ''">
              <xsl:element name="ns0:AddressLine2">
                <xsl:value-of select="$Addr2"/>
              </xsl:element>
            </xsl:if>

            <xsl:variable name="City" select="$Supplier/*[local-name()='SupplierLocation']/*[local-name()='Address']/*[local-name()='PostalAddress']/*[local-name()='City']"/>
            <xsl:if test="$City != ''">
              <xsl:element name="ns0:CityOrSuburb">
                <xsl:value-of select="$City"/>
              </xsl:element>
            </xsl:if>

            <xsl:variable name="State" select="$Supplier/*[local-name()='SupplierLocation']/*[local-name()='Address']/*[local-name()='PostalAddress']/*[local-name()='State']"/>
            <xsl:if test="$State != ''">
              <xsl:element name="ns0:StateOrProvince">
                <xsl:value-of select="$State"/>
              </xsl:element>
            </xsl:if>

            <xsl:variable name="PostCode" select="$Supplier/*[local-name()='SupplierLocation']/*[local-name()='Address']/*[local-name()='PostalAddress']/*[local-name()='PostalCode']"/>
            <xsl:if test="$PostCode != ''">
              <xsl:element name="ns0:PostCode">
                <xsl:value-of select="$PostCode"/>
              </xsl:element>
            </xsl:if>

            <xsl:variable name="Email" select="$Supplier/*[local-name()='SupplierLocation']/*[local-name()='Address']/*[local-name()='Email']"/>
            <xsl:if test="$Email != ''">
              <xsl:element name="ns0:Email">
                <xsl:value-of select="$Email"/>
              </xsl:element>
            </xsl:if>

            <xsl:variable name="Phone" select="$Supplier/*[local-name()='SupplierLocation']/*[local-name()='Address']/*[local-name()='Phone']/*[local-name()='Number']"/>
            <xsl:variable name="Fax" select="$Supplier/*[local-name()='SupplierLocation']/*[local-name()='Address']/*[local-name()='Fax']/*[local-name()='Number']"/>
            <xsl:if test="$Phone != '' or $Fax != ''">
              <xsl:element name="ns0:TelephoneNumbers">
                <xsl:if test="$Phone != ''">
                  <xsl:element name="ns0:TelephoneNumber">
                    <xsl:attribute name="NumberType">Business</xsl:attribute>
                    <xsl:value-of select="$Phone"/>
                  </xsl:element>
                </xsl:if>
                <xsl:if test="$Fax != ''">
                  <xsl:element name="ns0:TelephoneNumber">
                    <xsl:attribute name="NumberType">Fax</xsl:attribute>
                    <xsl:value-of select="$Fax"/>
                  </xsl:element>
                </xsl:if>
              </xsl:element>
            </xsl:if>

          </xsl:element>
          <!--end of Address-->
        </xsl:element>
        <!--end of Addresses-->
      </xsl:element>
      <!--end of OrganisationDetails-->
    </xsl:element>
    <!--end of Supplier-->


    <xsl:variable name="AddressCode" select="//*[local-name()='OrderRequest']/*[local-name()='ItemOut']/*[local-name()='ItemDetail']/*[local-name()='Extrinsic' and @*[local-name()='name'] = 'LINEATTRIBUTE1']"/>
    <xsl:if test="$AddressCode != ''">
      <xsl:element name="ns0:AddressCode">
        <xsl:value-of select="$AddressCode"/>
      </xsl:element>
    </xsl:if>


    <xsl:variable name="WarehouseCode" select="//*[local-name()='OrderRequest']/*[local-name()='OrderRequestHeader']/*[local-name()='Extrinsic' and @*[local-name()='name']='ATTRIBUTE1']"/>
    <xsl:if test="$WarehouseCode != ''">
      <xsl:element name="ns0:ShipmentPlanning">
        <xsl:variable name="Destination" select="ScriptNS0:GetRecipientCode('AXIDFWDFW_SHA' , 'AXIDFWDFW' , 'Shaw Order xml-file - Import Order Manager Orders' , 'Destination/Discharge' , 'Destination Port', $WarehouseCode)"/>
        <xsl:if test ="$Destination != ''">
          <xsl:element name="ns0:GoodsDestination">
            <xsl:value-of select="$Destination"/>
          </xsl:element>
        </xsl:if>

        <xsl:variable name="Discharge" select="ScriptNS0:GetRecipientCode('AXIDFWDFW_SHA' , 'AXIDFWDFW' , 'Shaw Order xml-file - Import Order Manager Orders' , 'Destination/Discharge' , 'Discharge Port', $WarehouseCode)"/>
        <xsl:if test="$Discharge != ''">
          <xsl:element name="ns0:DischargePort">
            <xsl:value-of select="$Discharge"/>
          </xsl:element>
        </xsl:if>

        <xsl:element name="ns0:GoodsDelivTo">
          <xsl:value-of select="$WarehouseCode"/>
        </xsl:element>
      </xsl:element>
    </xsl:if>

    <xsl:if test="contains(//*[local-name()='OrderRequestHeader']/@*[local-name()='orderID'], '.') or $WarehouseCode != ''">
      <xsl:element name="ns0:Custom">
        
        <xsl:if test="contains(//*[local-name()='OrderRequestHeader']/@*[local-name()='orderID'], '.')">
          <xsl:element name="ns0:Text1">
            <xsl:value-of select="substring-before(//*[local-name()='OrderRequestHeader']/@*[local-name()='orderID'], '.')"/>
          </xsl:element>
        </xsl:if>

        <xsl:if test="$WarehouseCode != ''">
          <xsl:element name="ns0:Text2">
            <xsl:value-of select="$WarehouseCode"/>
          </xsl:element>
        </xsl:if>

      </xsl:element>
    </xsl:if>



    <xsl:variable name="OrderID" select="//*[local-name()='OrderRequestHeader']/@*[local-name()='orderID']"/>
    <xsl:element name="ns0:OrderNumber">
      <xsl:choose>
        <xsl:when test="contains($OrderID, '.')">
          <xsl:value-of select="substring-after($OrderID, '.')"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$OrderID"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:element>

    <xsl:call-template name="MapValueIfNotEmpty">
      <xsl:with-param name="NodeName" select="'ns0:Text1'" />
      <xsl:with-param name="Value" select="./*[local-name()='ItemDetail']/*[local-name()='Extrinsic' and @*[local-name()='name'] = 'LINENUM']" />
    </xsl:call-template>

    <xsl:call-template name="MapValueIfNotEmpty">
      <xsl:with-param name="NodeName" select="'ns0:Text2'" />
      <xsl:with-param name="Value" select="./*[local-name()='ItemDetail']/*[local-name()='Extrinsic' and @*[local-name()='name'] = 'SHIPMENTNUM']" />
    </xsl:call-template>
    
  </xsl:template>

  <xsl:template name="MapValueIfNotEmpty">
    <xsl:param name="NodeName"/>
    <xsl:param name="Value"/>
    <xsl:if test="$Value != '' and string-length($Value) > 0">
      <xsl:element name="{$NodeName}">
        <xsl:value-of select="$Value"/>
      </xsl:element>
    </xsl:if>
  </xsl:template>
  
</xsl:stylesheet>

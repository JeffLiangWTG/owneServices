<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
    xmlns:s0="N/A"
    xmlns:ns0="N/A"
    xmlns:ScriptNS0="N/A"
    xmlns:ScriptNS1="N/A"
>
  <xsl:output method="xml" indent="yes"/>

  <xsl:template match="@* | node()">










    <xsl:if test="ManifestDetail/MF22 != '' or ManifestDetail/MF33 != ''">
      <xsl:element name="ns0:Notes">

        <xsl:if test="ManifestDetail/MF33 != ''">
          <xsl:element name="ns0:Note">
            <xsl:element name="ns0:NoteType">
              <xsl:text>MarksAndNumbers</xsl:text>
            </xsl:element>
            <xsl:element name="ns0:NoteData">
              <xsl:for-each select="ManifestDetail/MF33">
                <xsl:if test="position() != 1">
                  <xsl:text> </xsl:text>
                </xsl:if>
                <xsl:value-of select="normalize-space(MarksAndNumbers)"/>
              </xsl:for-each>
            </xsl:element>
          </xsl:element>
        </xsl:if>

        <xsl:if test="ManifestDetail/MF22 != ''">
          <xsl:element name="ns0:Note">
            <xsl:element name="ns0:NoteType">
              <xsl:text>DetailedGoodsDescription</xsl:text>
            </xsl:element>
            <xsl:element name="ns0:NoteData">
              <xsl:for-each select="ManifestDetail/MF22">
                <xsl:if test="position() != 1">
                  <xsl:text> </xsl:text>
                </xsl:if>
                <xsl:variable name="space" select="' '"/>
                <xsl:value-of select="normalize-space(concat(normalize-space(Description1), $space, normalize-space(Description2)))"/>
              </xsl:for-each>
            </xsl:element>
          </xsl:element>
        </xsl:if>

      </xsl:element>
    </xsl:if>


  <xsl:element name="GoodsDescription">
<xsl:variable name="space" select="' '"/>
<xsl:variable name="goodsdesc" select="./*[local-name()='ManifestDetail'][1]/*[local-name()='MF22']"/>
<xsl:value-of select="normalize-space(concat(normalize-space($goodsdesc/*[local-name()='Description1']), $space, normalize-space($goodsdesc/*[local-name()='Description2'])))"/>
</xsl:element>



    <xsl:variable name="consigneeCode">
      <xsl:choose>
        <xsl:when test="/*[local-name() = 'MAIN']/A/SenderCode != ''">
          <xsl:value-of select="/*[local-name() = 'MAIN']/A/SenderCode"/>
        </xsl:when>
        <xsl:when test="ManifestDetail/InvoiceHeader/IV01/ClientCode != ''">
          <xsl:value-of select="ManifestDetail/InvoiceHeader/IV01/ClientCode"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="ManifestDetail/InvoiceHeader/IV01/ImporterNumber"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:if test="$consigneeCode != ''">
      <xsl:attribute name="OwnerCode">
        <xsl:value-of select="$consigneeCode"/>
      </xsl:attribute>
    </xsl:if>







    
    <xsl:variable name="totalOuterPacks" select="sum(//ManifestDetail/MF32/ShipmentQuantity)"/>
    <xsl:variable name="totalCalculatedOuterPacks" select="sum(//ManifestDetail/InvoiceHeader//InvoiceDetail/IV22/Quantity1)"/>

    <xsl:if test ="number($totalOuterPacks) or number($totalCalculatedOuterPacks)">
    <xsl:element name="ns0:TotalOuterPacksQty">
      <xsl:choose>
        <xsl:when test="$totalOuterPacks != '' and number($totalOuterPacks) != 0 and string(number($totalOuterPacks)) != 'NaN'">
          <xsl:variable name="unit" select="normalize-space(ManifestDetail/MF32/ShipmentQuantityUnit)"/>
          <xsl:if test="$unit != ''">
            <xsl:attribute name="DimensionType">
              <xsl:value-of select="ScriptNS1:GetRecipientCode(&quot;NKINKILAX_UPS&quot; , &quot;NKINKILAX&quot; , &quot;UPS Mainx 4.01 File - Import Customs Declarations&quot; , &quot;Unit of Quantity&quot; , &quot;ediEnterprise code&quot; , string($unit))"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:value-of select="format-number(number($totalOuterPacks), '#0.000')"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:attribute name="DimensionType">
            <xsl:value-of select="ScriptNS1:GetRecipientCodeUnkeyed(&quot;NKINKILAX_UPS&quot; , &quot;NKINKILAX&quot; , &quot;UPS Mainx 4.01 File - Import Customs Declarations&quot; , &quot;Defaults&quot; , &quot;Package Type&quot;)"/>
          </xsl:attribute>
            <xsl:value-of select="format-number(number(substring($totalCalculatedOuterPacks, 1, string-length($totalCalculatedOuterPacks) - 2)) + (number(substring($totalCalculatedOuterPacks, string-length($totalCalculatedOuterPacks) - 1, 2)) div 100), '#0.0000')"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:element>
    </xsl:if>




    <xsl:variable name="TotalWeight" select="sum(//ManifestDetail/MF32/GrossWeight)"/>
    <xsl:variable name="TotalCalculatedWeight" select="sum(//ManifestDetail/InvoiceHeader//InvoiceDetail/IV22/GrossWeight)"/>
    <xsl:if test ="number($TotalWeight) or number($TotalCalculatedWeight)">
    <xsl:element name="ns0:Weight">     
      <xsl:choose>
        <xsl:when test="$TotalWeight != '' and number($TotalWeight) != 0 and string(number($TotalWeight)) != 'NaN'">
          <xsl:variable name="unit" select="normalize-space(ManifestDetail/MF32/GrossWeightUnit)"/>
          <xsl:if test="$unit != ''">
            <xsl:attribute name="DimensionType">
              <xsl:value-of select="$unit"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:value-of select="format-number(number($TotalWeight), '#0.000')"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:attribute name="DimensionType">
            <xsl:text>KG</xsl:text>
          </xsl:attribute>
          <xsl:value-of select="format-number(number(substring($TotalCalculatedWeight, 1, string-length($TotalCalculatedWeight) - 2)) + (number(substring($TotalCalculatedWeight, string-length($TotalCalculatedWeight) - 1, 2)) div 100), '#0.000')"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:element>
    </xsl:if>




    
    





    <xsl:element name="ns0:ShipmentType">
      <xsl:variable name="shipFrom" select="ManifestDetail/InvoiceHeader/IV12_14/IV14[CompanyFlag = 'S']/Country"/>
      <xsl:variable name="shipTo" select="ManifestDetail/InvoiceHeader/IV12_14/IV14[CompanyFlag = 'T']/Country"/>
      <xsl:choose>
        <xsl:when test="$shipTo = 'US'">
          <xsl:text>IMP</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>EXP</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:element>




    <xsl:variable name="IsExportJob" select ="userCSharp:xsltPage_AccessIsExport()"/>
    <xsl:element name="ns0:Invoices">
      <xsl:element name ="ns0:InvoiceHeader">
        <xsl:element name ="ns0:IsGroupInvoice">
          <xsl:text>true</xsl:text>
        </xsl:element>
        <xsl:element name ="ns0:InvoiceCharges">
          <xsl:element name ="ns0:InvoiceCharge">
            <xsl:element name ="ChargeType">
              <xsl:text>OFT</xsl:text>
            </xsl:element>
            <xsl:element name ="ChargeValue">
              <xsl:attribute name ="CurrencyCode">
                <xsl:text>USD</xsl:text>
              </xsl:attribute>
              <xsl:value-of select="ScriptNS1:GetRecipientCodeUnkeyed(&quot;NKINKILAX_UPS&quot; , &quot;NKINKILAX&quot; , &quot;UPS Mainx 4.01 File - Import Customs Declarations&quot; , &quot;Defaults&quot; , &quot;OFT Amount&quot;)"/>
            </xsl:element>
          </xsl:element>
        </xsl:element>
      </xsl:element>
      <xsl:for-each select="//InvoiceHeader">
        <xsl:element name="ns0:InvoiceHeader">
          <xsl:variable name="InvoiceQty" select ="sum(InvoiceDetail/IV22/Quantity1)"/>
          <xsl:element name ="ns0:InvoiceNumber">
            <xsl:value-of select="IV01/InvoiceNumber"/>
          </xsl:element>
          <xsl:element name="ns0:InvoiceAmount">
            <xsl:attribute name="CurrencyCode">
              <xsl:text>USD</xsl:text>
            </xsl:attribute>
            <xsl:value-of select="format-number(number(substring(IV90/TotalValue, 1, 10)) + (number(substring(IV90/TotalValue, 11, 2)) div 100), '#0.0000')"/>
          </xsl:element>
          <xsl:element name="ns0:InvoiceDate">
            <xsl:value-of select="ScriptNS0:ConvertToDateTimeString(string(IV01/InvoiceDate/text()) , &quot;yyMMdd&quot; , &quot;yyyy-MM-dd&quot;)"/>
          </xsl:element>
          <xsl:element name="ns0:Consignor">
            <xsl:attribute name="OwnerCode">
              <xsl:value-of select="string(IV02/Ship_ManfID/text())"/>
            </xsl:attribute>
          </xsl:element>

          <xsl:variable name="IV15DeliveryCode" select="IV15/TermsOfDelivery"/>
          
            <xsl:element name="ns0:Incoterm">
              <xsl:choose>
              <xsl:when test="$IV15DeliveryCode != ''">    
                <xsl:value-of select="$IV15DeliveryCode"/>
              </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="ScriptNS1:GetRecipientCodeUnkeyed(&quot;NKINKILAX_UPS&quot; , &quot;NKINKILAX&quot; , &quot;UPS Mainx 4.01 File - Import Customs Declarations&quot; , &quot;Defaults&quot; , &quot;INCO Term&quot;)"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:element>
          
          
          <xsl:element name="ns0:Weight">
            <xsl:attribute name="DimensionType">
              <xsl:text>KG</xsl:text>
            </xsl:attribute>
            <xsl:value-of select="format-number(number(substring(IV90/TotalGrossWeight, 1, 8)) + (number(substring(IV90/TotalGrossWeight, 9, 2)) div 100), '#0.000')"/>
          </xsl:element>

          <xsl:variable name="freightCharge" select="IV01/FreightCharge"/>
          <xsl:if test="$freightCharge != '' and number($freightCharge) != 0 and string(number($freightCharge)) != 'NaN'">
            <xsl:element name="ns0:InvoiceCharges">
              <xsl:element name="ns0:InvoiceCharge">
                <xsl:element name="ns0:ChargeType">
                  <xsl:text>OFT</xsl:text>
                </xsl:element>
                <xsl:element name="ns0:ChargeValue">
                  <xsl:attribute name="CurrencyCode">
                    <xsl:text>USD</xsl:text>
                  </xsl:attribute>
                  <xsl:value-of select="number($freightCharge)"/>
                </xsl:element>
              </xsl:element>
            </xsl:element>
          </xsl:if>
          
          <xsl:element name="ns0:InvoiceLines">
            <xsl:for-each select="InvoiceDetail">
              <xsl:element name="ns0:InvoiceLine">
                <xsl:element name="ns0:InvoiceQty">
                  <xsl:variable name="unit" select="normalize-space(IV22/UnitOfQuantity)"/>
                  <xsl:if test="$unit != ''">
                    <xsl:attribute name="DimensionType">
                      <xsl:value-of select="ScriptNS1:GetRecipientCode(&quot;NKINKILAX_UPS&quot; , &quot;NKINKILAX&quot; , &quot;UPS Mainx 4.01 File - Import Customs Declarations&quot; , &quot;Unit of Quantity&quot; , &quot;ediEnterprise code&quot; , string($unit))"/>
                    </xsl:attribute>
                  </xsl:if>
                  <xsl:value-of select="format-number(number(substring(IV22/Quantity1, 1, 7)) + (number(substring(IV22/Quantity1, 8, 2)) div 100), '#0.0000')"/>
                </xsl:element>
                <xsl:element name="ns0:LinePrice">
                  <xsl:attribute name="CurrencyCode">
                    <xsl:text>USD</xsl:text>
                  </xsl:attribute>
                  <xsl:value-of select="format-number(number(substring(IV22/Value1, 1, 8)) + (number(substring(IV22/Value1, 9, 2)) div 100), '#0.0000')"/>
                </xsl:element>
                <xsl:element name="ns0:ProductNumber">
                  <xsl:value-of select="IV20/PartNumber"/>
                </xsl:element>
                <xsl:element name="ns0:ProductDescription">
                  <xsl:value-of select="IV21/PartDescription"/>
                </xsl:element>

                <xsl:variable name="PONumber" select="IV20/PONumber"/>
                <xsl:if test="$PONumber != ''">
                  <xsl:element name="ns0:OrderNumber">
                    <xsl:value-of select="$PONumber"/>
                  </xsl:element>
                </xsl:if>
                
                <xsl:element name="ns0:LineClassification">
                  <xsl:element name="ns0:TariffCode">
                    <xsl:value-of select="IV22/TariffScheduleCode"/>
                  </xsl:element>
                </xsl:element>

                <xsl:variable name="weight" select="IV22/GrossWeight"/>
                <xsl:if test="$weight != '' and number($weight) != 0 and string(number($weight)) != 'NaN' ">
                  <xsl:element name="ns0:Weight">
                    <xsl:attribute name="DimensionType">
                      <xsl:text>KG</xsl:text>
                    </xsl:attribute>
                    <xsl:value-of select="format-number(number(substring($weight, 1, 7)) + (number(substring($weight, 8, 2)) div 100), '#0.000')"/>
                  </xsl:element>
                </xsl:if>

                <xsl:variable name="netWeight" select="IV22/NetWeight"/>
                <xsl:if test="$netWeight != '' and number($netWeight) != 0 and string(number($netWeight)) != 'NaN'">
                  <xsl:element name="ns0:NetWeight">
                    <xsl:attribute name="DimensionType">
                      <xsl:text>KG</xsl:text>
                    </xsl:attribute>
                    <xsl:value-of select="format-number(number(substring($netWeight, 1, 7)) + (number(substring($netWeight, 8, 2)) div 100), '#0.000')"/>
                  </xsl:element>
                </xsl:if>
                
                <xsl:element name="ns0:InvoiceLineNumber">
                  <xsl:value-of select="IV20/InvoiceLineNumber"/>
                </xsl:element>
                <xsl:element name="ns0:CountryPayload">
                  <xsl:element name="ns0:USInvoiceLine">

                    <xsl:variable name="PIRPType" select="IV20/ID1Type"/>
                    <xsl:if test="$PIRPType != ''">
                      <xsl:element name="ns0:PIRPRuling">
                        <xsl:element name="ns0:Number">
                          <xsl:value-of select="IV20/ID1Number"/>
                        </xsl:element>
                        <xsl:element name="ns0:Type">
                          <xsl:value-of select="ScriptNS1:GetRecipientCode(&quot;NKINKILAX_UPS&quot; , &quot;NKINKILAX&quot; , &quot;UPS Mainx 4.01 File - Import Customs Declarations&quot; , &quot;PIRP Ruling Type&quot; , &quot;ediEnterprise code&quot; , string($PIRPType/text()))"/>
                        </xsl:element>
                      </xsl:element>
                    </xsl:if>


                    <xsl:if test="boolean($IsExportJob)">
                      <xsl:element name="ns0:CountryOfExport">
                        <xsl:text>US</xsl:text>
                      </xsl:element>
                    </xsl:if>
                                       

                    <xsl:element name="ns0:CountryOfOrigin">
                      <xsl:value-of select="IV22/CountryOfOrigin"/>
                    </xsl:element>

                    <xsl:element name="ns0:TransactionRelated">true</xsl:element>
                    
                    <xsl:variable name="valueOf9802" select="IV24/ValueOf9802"/>
                    <xsl:if test="$valueOf9802 != '' and number($valueOf9802) != 0 and string(number($valueOf9802)) != 'NaN'">
                      <xsl:element name="ns0:USOrOriginalValueInInvCurr">
                        <xsl:value-of select="format-number(number(substring($valueOf9802, 1, 8)) + (number(substring($valueOf9802, 9, 2)) div 100), '#0.000')"/>
                      </xsl:element>
                    </xsl:if>

                    <xsl:if test ="normalize-space(IV22/SpecialProgramIndicator/text()) != ''">
                    <xsl:element name="ns0:SPI">
                      <xsl:value-of select="ScriptNS1:GetRecipientCode(&quot;NKINKILAX_UPS&quot; , &quot;NKINKILAX&quot; , &quot;UPS Mainx 4.01 File - Import Customs Declarations&quot; , &quot;SPI&quot; , &quot;ediEnterprise code&quot; , string(IV22/SpecialProgramIndicator/text()))"/>
                    </xsl:element>
                    </xsl:if>
                    
                    <xsl:if test="IV31 != '' and ((IV41 !='' and IV41/FDAMarker/text() != 'FD1') or IV43 !='')">
                      <xsl:element name="ns0:FDAs">
                        <xsl:element name="ns0:FDA">

                          <xsl:if test="IV41/ManufacturerIDNumber != ''">
                            <xsl:element name="ns0:Manufacturer">
                              <xsl:element name="ns0:MID">
                                  <xsl:value-of select="string(IV41/ManufacturerIDNumber/text())"/>
                              </xsl:element>
                            </xsl:element>
                          </xsl:if>

                          <xsl:if test="IV41/ShipperIDNumber != ''">
                            <xsl:element name="ns0:Shipper">
                              <xsl:element name="ns0:MID">
                                    <xsl:value-of select="string(IV41/ShipperIDNumber/text())"/>
                              </xsl:element>
                            </xsl:element>
                          </xsl:if>


                          <xsl:variable name="productionCountry" select="IV41/FDACountryCode"/>
                          <xsl:if test="$productionCountry != ''">
                            <xsl:element name="ns0:CountryOfProduction">
                              <xsl:value-of select="$productionCountry"/>
                            </xsl:element>
                          </xsl:if>

                          <xsl:if test ="IV21/PartDescription != ''">
                            <xsl:element name="ns0:CommercialDesc">
                              <xsl:value-of select="string(IV21/PartDescription/text())"/>
                            </xsl:element>
                          </xsl:if>
                          
                          <xsl:element name="ns0:ProductCode">
                            <xsl:value-of select="IV41/FDAProductCode"/>
                          </xsl:element>

                          <xsl:variable name="cargoStorageCode" select="IV41/FDAStorage"/>
                          <xsl:if test="$cargoStorageCode != ''">
                            <xsl:element name="ns0:CargoStorageCode">
                              <xsl:value-of select="$cargoStorageCode"/>
                            </xsl:element>
                          </xsl:if>

                          <xsl:variable name="value" select="IV43/LineValue"/>
                          <xsl:if test="$value != '' and number($value) != 0 and string(number($value)) != 'NaN'">
                            <xsl:element name="ns0:InvValue">
                              <xsl:value-of select="format-number(number($value), '#0.00')"/>
                            </xsl:element>
                          </xsl:if>

                          <xsl:if test="IV43/BrandName != ''">
                            <xsl:element name="ns0:BrandName">
                              <xsl:value-of select="IV43/BrandName"/>
                            </xsl:element>
                          </xsl:if>

                          <xsl:if test="IV42/Quantity1 != '' and number(IV42/Quantity1) != 0 and string(number(IV42/Quantity1)) != 'NaN'">
                            <xsl:element name="ns0:Quantities">

                              <xsl:element name="ns0:Quantity1">
                                <xsl:variable name="unit" select="normalize-space(IV42/Unit1)"/>
                                <xsl:variable name="quantity" select="IV42/Quantity1"/>
                                <xsl:if test="$unit != ''">
                                  <xsl:attribute name="DimensionType">
                                    <xsl:value-of select="ScriptNS1:GetRecipientCode(&quot;NKINKILAX_UPS&quot; , &quot;NKINKILAX&quot; , &quot;UPS Mainx 4.01 File - Import Customs Declarations&quot; , &quot;Unit of Quantity&quot; , &quot;ediEnterprise code&quot; , string($unit))"/>
                                  </xsl:attribute>
                                </xsl:if>
                                <xsl:value-of select="format-number(number(substring($quantity, 1, 8)) + (number(substring($quantity, 9, 2)) div 100), '#0.00')"/>
                              </xsl:element>

                              <xsl:if test="IV42/Quantity2 != '' and number(IV42/Quantity2) != 0 and string(number(IV42/Quantity2)) != 'NaN'">
                                <xsl:element name="ns0:Quantity2">
                                  <xsl:variable name="unit" select="normalize-space(IV42/Unit2)"/>
                                  <xsl:variable name="quantity" select="IV42/Quantity2"/>
                                  <xsl:if test="$unit != ''">
                                    <xsl:attribute name="DimensionType">
                                      <xsl:value-of select="ScriptNS1:GetRecipientCode(&quot;NKINKILAX_UPS&quot; , &quot;NKINKILAX&quot; , &quot;UPS Mainx 4.01 File - Import Customs Declarations&quot; , &quot;Unit of Quantity&quot; , &quot;ediEnterprise code&quot; , string($unit))"/>
                                    </xsl:attribute>
                                  </xsl:if>
                                  <xsl:value-of select="format-number(number(substring($quantity, 1, 8)) + (number(substring($quantity, 9, 2)) div 100), '#0.00')"/>
                                </xsl:element>
                              </xsl:if>

                              <xsl:if test="IV42/Quantity3 != '' and number(IV42/Quantity3) != 0 and string(number(IV42/Quantity3)) != 'NaN'">
                                <xsl:element name="ns0:Quantity3">
                                  <xsl:variable name="unit" select="normalize-space(IV42/Unit3)"/>
                                  <xsl:variable name="quantity" select="IV42/Quantity3"/>
                                  <xsl:if test="$unit != ''">
                                    <xsl:attribute name="DimensionType">
                                      <xsl:value-of select="ScriptNS1:GetRecipientCode(&quot;NKINKILAX_UPS&quot; , &quot;NKINKILAX&quot; , &quot;UPS Mainx 4.01 File - Import Customs Declarations&quot; , &quot;Unit of Quantity&quot; , &quot;ediEnterprise code&quot; , string($unit))"/>
                                    </xsl:attribute>
                                  </xsl:if>
                                  <xsl:value-of select="format-number(number(substring($quantity, 1, 8)) + (number(substring($quantity, 9, 2)) div 100), '#0.00')"/>
                                </xsl:element>
                              </xsl:if>

                              <xsl:if test="IV42/Quantity4 != '' and number(IV42/Quantity4) != 0 and string(number(IV42/Quantity4)) != 'NaN'">
                                <xsl:element name="ns0:Quantity4">
                                  <xsl:variable name="unit" select="normalize-space(IV42/Unit4)"/>
                                  <xsl:variable name="quantity" select="IV42/Quantity4"/>
                                  <xsl:if test="$unit != ''">
                                    <xsl:attribute name="DimensionType">
                                      <xsl:value-of select="ScriptNS1:GetRecipientCode(&quot;NKINKILAX_UPS&quot; , &quot;NKINKILAX&quot; , &quot;UPS Mainx 4.01 File - Import Customs Declarations&quot; , &quot;Unit of Quantity&quot; , &quot;ediEnterprise code&quot; , string($unit))"/>
                                    </xsl:attribute>
                                  </xsl:if>
                                  <xsl:value-of select="format-number(number(substring($quantity, 1, 8)) + (number(substring($quantity, 9, 2)) div 100), '#0.00')"/>
                                </xsl:element>
                              </xsl:if>

                              <xsl:if test="IV42/Quantity5 != '' and number(IV42/Quantity5) != 0 and string(number(IV42/Quantity5)) != 'NaN'">
                                <xsl:element name="ns0:Quantity5">
                                  <xsl:variable name="unit" select="normalize-space(IV42/Unit5)"/>
                                  <xsl:variable name="quantity" select="IV42/Quantity5"/>
                                  <xsl:if test="$unit != ''">
                                    <xsl:attribute name="DimensionType">
                                      <xsl:value-of select="ScriptNS1:GetRecipientCode(&quot;NKINKILAX_UPS&quot; , &quot;NKINKILAX&quot; , &quot;UPS Mainx 4.01 File - Import Customs Declarations&quot; , &quot;Unit of Quantity&quot; , &quot;ediEnterprise code&quot; , string($unit))"/>
                                    </xsl:attribute>
                                  </xsl:if>
                                  <xsl:value-of select="format-number(number(substring($quantity, 1, 8)) + (number(substring($quantity, 9, 2)) div 100), '#0.00')"/>
                                </xsl:element>
                              </xsl:if>

                            </xsl:element>
                          </xsl:if>

                          <xsl:if test="IV41/AffirmationOfComplianceCode != ''">
                            <xsl:element name="ns0:AffirmationCodes">
                              <xsl:for-each select="IV41">
                                <xsl:element name="ns0:AffirmationCode">
                                  <xsl:element name="ns0:Code">
                                    <xsl:value-of select="AffirmationOfComplianceCode"/>
                                  </xsl:element>
                                  <xsl:element name="ns0:Value">
                                    <xsl:value-of select="AffirmationOfComplianceQuanlifier"/>
                                  </xsl:element>
                                </xsl:element>
                              </xsl:for-each>
                            </xsl:element>
                          </xsl:if>

                        </xsl:element>
                      </xsl:element>
                    </xsl:if>

                    <xsl:if test="IV45/ImportConditionCode != ''">
                      <xsl:element name="ns0:FCCs">
                        <xsl:element name="ns0:FCC">
                          <xsl:element name="ns0:ImportConditionNo">
                            <xsl:value-of select="IV45/ImportConditionCode"/>
                          </xsl:element>
                          <xsl:element name="ns0:ImportConditionQtyApproved">
                            <xsl:value-of select="IV45/QuantityApproval = 'Y'"/>
                          </xsl:element>
                          <xsl:element name="ns0:ID">
                            <xsl:value-of select="IV45/FCCIdentifier"/>
                          </xsl:element>
                          <xsl:element name="ns0:TradeName">
                            <xsl:value-of select="IV45/TradeName"/>
                          </xsl:element>
                          <xsl:element name="ns0:Withhold">
                            <xsl:value-of select="IV45/Withhold = 'Y'"/>
                          </xsl:element>
                        </xsl:element>
                      </xsl:element>
                    </xsl:if>

                    <xsl:choose>
                      <xsl:when test ="IV41/FDAMarker/text() = 'FD1' and IV31/OGACode/text() = 'FDA'">
                        <xsl:element name="ns0:OGAIndicators">
                          <xsl:element name="ns0:FDAIndicator">
                            <xsl:text>Disclaimed</xsl:text>
                          </xsl:element>
                        </xsl:element>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:if test ="IV31 != '' and ((IV41 !='' and IV41/FDAMarker/text() != 'FD1') or IV43 !='')">
                          <xsl:element name="ns0:OGAIndicators">
                            <xsl:element name="ns0:FDAIndicator">
                              <xsl:text>Declared</xsl:text>
                            </xsl:element>
                          </xsl:element>
                        </xsl:if>
                      </xsl:otherwise>
                    </xsl:choose>


                    <xsl:if test="boolean($IsExportJob)">
                      <xsl:element name="OriginIndicator">
                        <xsl:variable name="ctyOfOrigin" select ="normalize-space(IV22/CountryOfOrigin/text())"/>
                        <xsl:choose>
                          <xsl:when test ="$ctyOfOrigin ='US'">
                            <xsl:text>D</xsl:text>
                          </xsl:when>
                          <xsl:otherwise>F</xsl:otherwise>
                        </xsl:choose>
                      </xsl:element>
                    </xsl:if>

                    <xsl:variable name="tariff9802" select="IV24/TariffScheduleCode"/>
                    <xsl:if test="$tariff9802 != ''">
                      <xsl:element name="ns0:Supplementary">
                        <xsl:element name="ns0:Tariff">
                          <xsl:value-of select="$tariff9802"/>
                        </xsl:element>
                      </xsl:element>
                    </xsl:if>
                    
                  </xsl:element>
                </xsl:element>
              </xsl:element>
            </xsl:for-each>
            
            <xsl:for-each select="InvoiceDetail/IV26">
              <xsl:if test="TariffScheduleCode != ''">
                <xsl:element name="ns0:InvoiceLine">
                  <xsl:element name="ns0:LinePrice">
                    <xsl:attribute name="CurrencyCode">
                      <xsl:text>USD</xsl:text>
                    </xsl:attribute>
                    <xsl:value-of select="format-number(number(substring(USPackagingValue, 1, 8)) + (number(substring(USPackagingValue, 9, 2)) div 100), '#0.0000')"/>
                  </xsl:element>
                  <xsl:element name="ns0:LineClassification">
                    <xsl:element name="ns0:TariffCode">
                      <xsl:value-of select="TariffScheduleCode"/>
                    </xsl:element>
                  </xsl:element>

                  <xsl:variable name="charge" select="AssemblyPrice"/>
                  <xsl:if test="$charge != '' and number($charge) != 0">
                    <xsl:element name="ns0:Charges">
                      <xsl:element name="ns0:Charge">
                        <xsl:element name="ns0:ChargeType">
                          <xsl:text>ADD</xsl:text>
                        </xsl:element>
                        <xsl:element name="ns0:ChargeValue">
                          <xsl:value-of select="format-number(number(substring($charge, 1, 10)) + (number(substring($charge, 11, 2)) div 100), '#0.0000')"/>
                        </xsl:element>
                      </xsl:element>
                    </xsl:element>
                  </xsl:if>

                  <xsl:variable name="quantity" select="Quantity3"/>
                  <xsl:if test="$quantity != '' and number($quantity) != 0">
                    <xsl:element name="ns0:CountryPayload">
                      <xsl:element name="ns0:USInvoiceLine">
                        <xsl:element name="ns0:ThirdQty">
                          <xsl:variable name="unit" select="normalize-space(UnitOfQuantity)"/>
                          <xsl:if test="$unit != ''">
                            <xsl:attribute name="DimensionType">
                              <xsl:value-of select="ScriptNS1:GetRecipientCode(&quot;NKINKILAX_UPS&quot; , &quot;NKINKILAX&quot; , &quot;UPS Mainx 4.01 File - Import Customs Declarations&quot; , &quot;Unit of Quantity&quot; , &quot;ediEnterprise code&quot; , string($unit))"/>
                            </xsl:attribute>
                          </xsl:if>
                          <xsl:value-of select="format-number(number(substring($quantity, 1, 7)) + (number(substring($quantity, 8, 2)) div 100), '#0.0000')"/>
                        </xsl:element>
                      </xsl:element>
                    </xsl:element>
                  </xsl:if>
                  
                </xsl:element>
              </xsl:if>
            </xsl:for-each>
          </xsl:element>
          
          
          <xsl:element name="ns0:CountryPayload">
            <xsl:element name="ns0:USInvoice">
              <xsl:element name="ns0:Organisations">
                <xsl:element name="ns0:Manufacturer">
                  <xsl:element name="ns0:MID">
                    <xsl:value-of select="string(IV02/Ship_ManfID/text())"/>
                  </xsl:element>
                </xsl:element>
              </xsl:element>



              <xsl:variable name="countryOfOrigin" select="../MF32/CountryOfOrigin"/>
              <xsl:choose>
                <xsl:when test="$countryOfOrigin != ''">
                  <xsl:element name="ns0:CountryOfOrigin">
                    <xsl:value-of select="$countryOfOrigin"/>
                  </xsl:element>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:element name="ns0:CountryOfOrigin">
                    <xsl:value-of select="InvoiceDetail[1]/IV22/CountryOfOrigin"/>
                  </xsl:element>
                </xsl:otherwise>
              </xsl:choose>


              <xsl:choose>
                <xsl:when test="not(boolean($IsExportJob))">
                  <xsl:choose>
                    <xsl:when test="IV1A != ''">
                      <xsl:element name="ns0:PaymentTerms">
                        <xsl:element name="ns0:Code">
                          <xsl:value-of select="IV1A/PaymentTermsType"/>
                        </xsl:element>
                        <xsl:element name="ns0:Desc">
                          <xsl:value-of select="IV1A/TermsOfPayment"/>
                        </xsl:element>
                      </xsl:element>
                      <xsl:element name="ns0:TransactionRelated">true</xsl:element>
                      <xsl:element name="ns0:InvoiceType">
                        <xsl:value-of select="IV1A/InvoiceType"/>
                      </xsl:element>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:element name="ns0:TransactionRelated">true</xsl:element>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:when>
              <xsl:otherwise>
                <xsl:element name="ns0:CountryOfExport">US</xsl:element>
                <xsl:element name="ns0:TransactionRelated">true</xsl:element>
              </xsl:otherwise>
              </xsl:choose>

            </xsl:element>
          </xsl:element>
        </xsl:element>
      </xsl:for-each>
    </xsl:element>











    <xsl:element name="ns0:BillContainerPacks">
      <xsl:element name="ns0:BillContainerPack">
        <xsl:element name="ns0:MasterbillNumber">
          <xsl:value-of select="MF01/ManifestNumber"/>
        </xsl:element>
      </xsl:element>
    </xsl:element>




    <xsl:if test="ManifestDetail/MF20/CarrierCode != ''">
      <xsl:element name="ns0:BillsOfLading">
        <xsl:element name="ns0:BillOfLading">
          <xsl:attribute name="BillNumber">
            <xsl:value-of select="MF01/ManifestNumber"/>
          </xsl:attribute>
          <xsl:attribute name="BillType">
            <xsl:text>MB</xsl:text>
          </xsl:attribute>
          <xsl:element name="ns0:IssuerSCAC">
            <xsl:value-of select="ManifestDetail/MF20/CarrierCode"/>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:if>







    <xsl:if test="ManifestDetail/InvoiceHeader/IV1A != ''">
      <xsl:attribute name="CertifyCargoRelease">
        <xsl:text>true</xsl:text>
      </xsl:attribute>
      <xsl:attribute name="EnableElectronicInvoice">
        <xsl:text>true</xsl:text>
      </xsl:attribute>
    </xsl:if>
    
    
    
    
    





    <xsl:if test="ManifestDetail/InvoiceHeader/IV01/ImporterNumber != ''">
      <xsl:element name="ns0:ImporterOfRecord">
        <xsl:element name="ns0:ImporterOfRecord">
          <xsl:attribute name="OwnerCode">
            <xsl:value-of select="string(ManifestDetail/InvoiceHeader/IV01/ImporterNumber/text())"/>
          </xsl:attribute>

          <xsl:variable name="company" select="ManifestDetail/InvoiceHeader/IV12_14[IV12/CompanyFlag = 'C']"/>
          <xsl:element name="ns0:OrganisationDetails">
            <xsl:element name="ns0:Name">
              <xsl:value-of select="$company/IV12/CompanyName"/>
            </xsl:element>
            <xsl:element name="ns0:Addresses">
              <xsl:element name="ns0:Address">
                <xsl:element name="ns0:AddressLine1">
                  <xsl:choose>
                    <xsl:when test="$company/IV13/POBox != ''">
                      <xsl:value-of select="$company/IV13/POBox"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="$company/IV13/Address"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:element>
                <xsl:element name="ns0:CityOrSuburb">
                  <xsl:value-of select="$company/IV14/City"/>
                </xsl:element>
                <xsl:element name="ns0:StateOrProvince">
                  <xsl:value-of select="$company/IV14/State"/>
                </xsl:element>
                <xsl:element name="ns0:PostCode">
                  <xsl:value-of select="$company/IV13/PostCode"/>
                </xsl:element>
                <xsl:element name="ns0:TelephoneNumbers">
                  <xsl:element name="ns0:TelephoneNumber">
                    <xsl:attribute name="NumberType">
                      <xsl:text>Business</xsl:text>
                    </xsl:attribute>
                    <xsl:value-of select="$company/IV14/Phone"/>
                  </xsl:element>
                </xsl:element>
              </xsl:element>
            </xsl:element>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:if>


    <xsl:element name="ns0:BranchCode">
      <xsl:variable name ="clientCode" select="ManifestHeader/MF01[1]/ClientCode/text()"/>
      <xsl:variable name ="shipTo" select="ManifestHeader/ManifestDetail[1]/InvoiceHeader[1]/IV12_14/IV14[CompanyFlag = 'T']/Country"/>
      <xsl:value-of select="ScriptNS1:GetRecipientCode(&quot;NKINKILAX_UPS&quot; , &quot;NKINKILAX&quot; , &quot;UPS Mainx 4.01 File - Import Customs Declarations&quot; , &quot;Branch Code&quot; , &quot;ediEnterprise code&quot; , string($clientCode))"/>
      <xsl:variable name="setImportExport" select ="userCSharp:SetDirection($shipTo)"/>
    </xsl:element>

    
    <!-- Export shipment related script -->

    <xsl:if test ="boolean(userCSharp:ExpPage_AccessIsExport())">
    <xsl:element name="ns0:Export">
      <xsl:variable name ="exportPort" select ="ManifestDetail[1]/InvoiceHeader[1]/IV12_14/IV14[CompanyFlag = 'T']/Port/text()" />
      <xsl:value-of select ="$exportPort"/>
    </xsl:element>
    </xsl:if>
  </xsl:template>

  
  <xsl:if test ="boolean(userCSharp:ExpPage_AccessIsExport())">
    <xsl:variable name ="destCountry" select ="ManifestDetail[1]/InvoiceHeader[1]/IV12_14/IV14[CompanyFlag = 'T']/Country/text()" />
    <xsl:if test="normalize-space($destCountry) != ''">
    <xsl:element name="ns0:CountryOfUltimateDestination">
      <xsl:value-of select="$destCountry"/>      
    </xsl:element>
    </xsl:if>
  </xsl:if>

  <xsl:if test ="boolean(userCSharp:ExpPage_AccessIsExport())">
    <xsl:variable name ="destCountry" select ="ManifestDetail[1]/InvoiceHeader[1]/IV12_14/IV14[CompanyFlag = 'T']/Country/text()" />
    <xsl:variable name ="destCity" select ="ManifestDetail[1]/InvoiceHeader[1]/IV12_14/IV14[CompanyFlag = 'T']/City/text()" />
    <xsl:if test= "normalize-space(concat($destCountry, $destCity)) != ''">
      <xsl:element name="ns0:Port">
        <xsl:value-of select="normalize-space(concat($destCountry, $destCity))"/>
      </xsl:element>
    </xsl:if>
  </xsl:if>

  <xsl:if test ="boolean(userCSharp:ExpPage_AccessIsExport())">
    <xsl:variable name ="exportCountry" select ="ManifestDetail[1]/InvoiceHeader[1]/IV12_14/IV14[CompanyFlag = 'S']/Country/text()" />
    <xsl:if test="normalize-space($exportCountry) != ''">
      <xsl:element name="ns0:CountryOfExport">
        <xsl:value-of select="$exportCountry"/>
      </xsl:element>
    </xsl:if>
  </xsl:if>

  <xsl:if test ="boolean(userCSharp:ExpPage_AccessIsExport())">
    <xsl:variable name ="clientCode" select="ManifestHeader/MF01[1]/ClientCode/text()"/>
    <xsl:variable name="branch" select="ScriptNS1:GetRecipientCode(&quot;NKINKILAX_UPS&quot; , &quot;NKINKILAX&quot; , &quot;UPS Mainx 4.01 File - Import Customs Declarations&quot; , &quot;Branch Code&quot; , &quot;ediEnterprise code&quot; , string($clientCode))"/>
    <xsl:element name="ns0:ReceivingAgent">
      <xsl:attribute name ="OwnerCode">
        <xsl:value-of select ="$branch"/>
      </xsl:attribute>
    </xsl:element>
  </xsl:if>
  
</xsl:stylesheet>






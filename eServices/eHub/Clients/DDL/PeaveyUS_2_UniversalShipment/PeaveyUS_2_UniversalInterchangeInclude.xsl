<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl ns0 ScriptNS0 ScriptNS1" version="1.0"
                xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
                xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1">
  <xsl:output omit-xml-declaration="yes" indent="no" version="1.0" method="xml" />

  <xsl:template match="/">

    <ns0:UniversalInterchangeInclude>
      <ns0:Header>
        <ns0:SenderID/>
        <ns0:RecipientID/>
      </ns0:Header>
      <ns0:Body>
        <xsl:for-each select = "//*[local-name()='Data']">
          <ns0:UniversalShipment>
            <ns0:Shipment>
              <ns0:DataContext>
                <ns0:DataProvider>
                  <xsl:value-of select="ScriptNS1:GetRecipientCodeUnkeyed('DDLHKGHKG_POO', 'DDLHKGHKG', 'Peavey US csv File - Import of OrderManager Orders', 'Defaults', 'Data Provider')"/>
                </ns0:DataProvider>
                <ns0:DataTargetCollection>
                  <ns0:DataTarget>
                    <ns0:Type>OrderManagerOrder</ns0:Type>
                  </ns0:DataTarget>
                </ns0:DataTargetCollection>
              </ns0:DataContext>
              <xsl:if test="./*[local-name()='Incoterms2'] != ''">
                <ns0:AdditionalTerms>
                  <xsl:value-of select="./*[local-name()='Incoterms2']"/>
                </ns0:AdditionalTerms>
              </xsl:if>
              <xsl:if test="./*[local-name()='Incoterms'] != ''">
                <ns0:ShipmentIncoTerm>
                  <ns0:Code>
                    <xsl:value-of select="./*[local-name()='Incoterms']"/>
                  </ns0:Code>
                </ns0:ShipmentIncoTerm>
              </xsl:if>
              <xsl:variable name="DelDate" select="ScriptNS0:ConvertToDateTimeString(./*[local-name()='DelDate'],'yyyy/MM/dd')"/>
              <xsl:if test="$DelDate != ''">
                <ns0:LocalProcessing>
                  <ns0:DeliveryRequiredBy>
                    <xsl:value-of select="$DelDate"/>
                  </ns0:DeliveryRequiredBy>
                </ns0:LocalProcessing>
              </xsl:if>
              <ns0:Order>
                <ns0:OrderNumber>
                  <xsl:value-of select="concat(./*[local-name()='PurchaseDoc'], '-', ./*[local-name()='LineNumber'])"/>
                </ns0:OrderNumber>
                <ns0:OrderLineCollection>
                  <ns0:OrderLine>
                    <xsl:if test="normalize-space(./*[local-name()='CondBasedValue']) != ''">
                      <ns0:ExtendedLinePrice>
                        <xsl:value-of select="normalize-space(./*[local-name()='CondBasedValue'])"/>
                      </ns0:ExtendedLinePrice>
                    </xsl:if>
                    <ns0:LineNumber>
                      <xsl:value-of select="./*[local-name()='LineNumber']"/>
                    </ns0:LineNumber>
                    <ns0:OrderedQty>
                      <xsl:value-of select="./*[local-name()='Quantity']"/>
                    </ns0:OrderedQty>
                    <xsl:if test="./*[local-name()='UOM'] != ''">
                      <ns0:OrderedQtyUnit>
                        <ns0:Code>
                          <xsl:value-of select="./*[local-name()='UOM']"/>
                        </ns0:Code>
                      </ns0:OrderedQtyUnit>
                    </xsl:if>
                    <xsl:if test="./*[local-name()='MaterialNumber'] != ''">
                      <ns0:Product>
                        <ns0:Code>
                          <xsl:value-of select="./*[local-name()='MaterialNumber']"/>
                        </ns0:Code>
                        <xsl:if test="./*[local-name()='ShortText'] != ''">
                          <ns0:Description>
                            <xsl:value-of select="./*[local-name()='ShortText']"/>
                          </ns0:Description>
                        </xsl:if>
                      </ns0:Product>
                    </xsl:if>
                    <xsl:if test="normalize-space(./*[local-name()='NetPrice']) > 0">
                      <ns0:UnitPriceRecommended>
                        <xsl:value-of select="normalize-space(./*[local-name()='NetPrice'])"/>
                      </ns0:UnitPriceRecommended>
                    </xsl:if>
                    <xsl:if test="./*[local-name()='SalesDoc'] != '' or ./*[local-name()='PurchasingGroup'] != ''
                            or ./*[local-name()='CommImpCodeNumber'] != '' or ./*[local-name()='MaterialGroup'] != ''">
                      <ns0:CustomizedFieldCollection>
                        <xsl:if test="./*[local-name()='SalesDoc'] != ''">
                          <ns0:CustomizedField>
                            <ns0:Key>Sales Order#</ns0:Key>
                            <ns0:DataType>String</ns0:DataType>
                            <ns0:Value>
                              <xsl:value-of select="./*[local-name()='SalesDoc']"/>
                            </ns0:Value>
                          </ns0:CustomizedField>
                        </xsl:if>
                        <xsl:if test="./*[local-name()='PurchasingGroup'] != ''">
                          <ns0:CustomizedField>
                            <ns0:Key>Buyer ID</ns0:Key>
                            <ns0:DataType>String</ns0:DataType>
                            <ns0:Value>
                              <xsl:value-of select="./*[local-name()='PurchasingGroup']"/>
                            </ns0:Value>
                          </ns0:CustomizedField>
                        </xsl:if>
                        <xsl:if test="./*[local-name()='CommImpCodeNumber'] != ''">
                          <ns0:CustomizedField>
                            <ns0:Key>HS Code</ns0:Key>
                            <ns0:DataType>String</ns0:DataType>
                            <ns0:Value>
                              <xsl:value-of select="./*[local-name()='CommImpCodeNumber']"/>
                            </ns0:Value>
                          </ns0:CustomizedField>
                        </xsl:if>
                        <xsl:if test="./*[local-name()='MaterialGroup'] != ''">
                          <ns0:CustomizedField>
                            <ns0:Key>Product Group</ns0:Key>
                            <ns0:DataType>String</ns0:DataType>
                            <ns0:Value>
                              <xsl:value-of select="./*[local-name()='MaterialGroup']"/>
                            </ns0:Value>
                          </ns0:CustomizedField>
                        </xsl:if>
                      </ns0:CustomizedFieldCollection>
                    </xsl:if>
                  </ns0:OrderLine>
                </ns0:OrderLineCollection>
              </ns0:Order>
              <xsl:if test="./*[local-name()='PaymentTerms'] != '' or ./*[local-name()='Description'] != ''">
                <ns0:CustomizedFieldCollection>
                  <xsl:if test="./*[local-name()='PaymentTerms'] != ''">
                    <ns0:CustomizedField>
                      <ns0:Key>Payment Terms</ns0:Key>
                      <ns0:DataType>String</ns0:DataType>
                      <ns0:Value>
                        <xsl:value-of select="./*[local-name()='PaymentTerms']"/>
                      </ns0:Value>
                    </ns0:CustomizedField>
                  </xsl:if>
                  <xsl:if test="./*[local-name()='Description'] != ''">
                    <ns0:CustomizedField>
                      <ns0:Key>Key Client Name</ns0:Key>
                      <ns0:DataType>String</ns0:DataType>
                      <ns0:Value>
                        <xsl:value-of select="./*[local-name()='Description']"/>
                      </ns0:Value>
                    </ns0:CustomizedField>
                  </xsl:if>
                  <xsl:if test="./*[local-name()='ShortText'] != ''">
                    <ns0:CustomizedField>
                      <ns0:Key>Product Short Text</ns0:Key>
                      <ns0:DataType>String</ns0:DataType>
                      <ns0:Value>
                        <xsl:value-of select="./*[local-name()='ShortText']"/>
                      </ns0:Value>
                    </ns0:CustomizedField>
                  </xsl:if>
                </ns0:CustomizedFieldCollection>
              </xsl:if>
              <xsl:variable name="PODate" select="ScriptNS0:ConvertToDateTimeString(./*[local-name()='PODate'],'yyyy/MM/dd')"/>
              <xsl:if test="$PODate != ''">
                <ns0:DateCollection>
                  <ns0:Date>
                    <ns0:Type>OrderDate</ns0:Type>
                    <ns0:IsEstimate>false</ns0:IsEstimate>
                    <ns0:Value>
                      <xsl:value-of select="$PODate"/>
                    </ns0:Value>
                  </ns0:Date>
                </ns0:DateCollection>
              </xsl:if>

              <ns0:OrganizationAddressCollection>
                <ns0:OrganizationAddress>
                  <ns0:AddressType>ConsignorDocumentaryAddress</ns0:AddressType>
                  <xsl:if test="./*[local-name()='VendorNumber'] != ''">
                    <ns0:OrganizationCode>
                      <xsl:value-of select="./*[local-name()='VendorNumber']"/>
                    </ns0:OrganizationCode>
                  </xsl:if>
                  <xsl:if test="./*[local-name()='VendorName'] != ''">
                    <ns0:CompanyName>
                      <xsl:value-of select="./*[local-name()='VendorName']"/>
                    </ns0:CompanyName>
                  </xsl:if>
                </ns0:OrganizationAddress>
                <ns0:OrganizationAddress>
                  <ns0:AddressType>ConsigneeDocumentaryAddress</ns0:AddressType>
                  <xsl:if test="./*[local-name()='Customer'] != ''">
                    <ns0:OrganizationCode>
                      <xsl:value-of select="./*[local-name()='Customer']"/>
                    </ns0:OrganizationCode>
                  </xsl:if>
                  <xsl:if test="./*[local-name()='Name2'] != ''">
                    <ns0:Address1>
                      <xsl:value-of select="./*[local-name()='Name2']"/>
                    </ns0:Address1>
                  </xsl:if>
                  <xsl:if test="./*[local-name()='Street'] != ''">
                    <ns0:Address2>
                      <xsl:value-of select="./*[local-name()='Street']"/>
                    </ns0:Address2>
                  </xsl:if>
                  <xsl:if test="./*[local-name()='City'] != ''">
                    <ns0:City>
                      <xsl:value-of select="./*[local-name()='City']"/>
                    </ns0:City>
                  </xsl:if>
                  <xsl:if test="./*[local-name()='Customer'] != ''">
                    <ns0:CompanyName>
                      <xsl:value-of select="./*[local-name()='Customer']"/>
                    </ns0:CompanyName>
                  </xsl:if>
                  <xsl:if test="./*[local-name()='Country'] != ''">
                    <ns0:Country>
                      <ns0:Code>
                        <xsl:value-of select="./*[local-name()='Country']"/>
                      </ns0:Code>
                    </ns0:Country>
                  </xsl:if>
                  <xsl:if test="./*[local-name()='PostalCode'] != ''">
                    <ns0:Postcode>
                      <xsl:value-of select="./*[local-name()='PostalCode']"/>
                    </ns0:Postcode>
                  </xsl:if>
                </ns0:OrganizationAddress>
              </ns0:OrganizationAddressCollection>
            </ns0:Shipment>
          </ns0:UniversalShipment>
        </xsl:for-each>
      </ns0:Body>
    </ns0:UniversalInterchangeInclude>

  </xsl:template>

</xsl:stylesheet>
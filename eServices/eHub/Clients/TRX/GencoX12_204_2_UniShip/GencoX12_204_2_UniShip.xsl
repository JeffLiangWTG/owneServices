<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl ScriptNS0 ScriptNS1" version="1.0"
                xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
								xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1">
  <xsl:output omit-xml-declaration="yes" indent="no" version="1.0" method="xml" />

  <xsl:template match="/">
    <ns0:UniversalShipment>
      <ns0:Shipment>
        <ns0:DataContext>
          <ns0:DataProvider>
            <xsl:value-of select="ScriptNS1:GetRecipientCodeUnkeyed('TRXELPELP_G01' , 'TRXELPELP' , 'Genco 204 - Receive Shipments' , 'Defaults' , 'Data Provider Code')"/>
          </ns0:DataProvider>
          <ns0:DataTargetCollection>
            <ns0:DataTarget>
              <ns0:Type>ForwardingShipment</ns0:Type>
            </ns0:DataTarget>
          </ns0:DataTargetCollection>
        </ns0:DataContext>

        <xsl:variable name="ShipperRef" select="//*[local-name()='B2']/*[local-name()='B204']"/>
        <xsl:if test="$ShipperRef">
          <ns0:BookingConfirmationReference>
            <xsl:value-of select="$ShipperRef"/>
          </ns0:BookingConfirmationReference>
        </xsl:if>

        <xsl:variable name="Quantity" select="//*[local-name()='S5Loop1']/*[local-name()='AT8']/*[local-name()='AT805']"/>
        <xsl:if test="$Quantity > 0">
          <ns0:OuterPacks>
            <xsl:value-of select="$Quantity"/>
          </ns0:OuterPacks>
          <ns0:OuterPacksPackageType>
            <ns0:Code>
              <xsl:value-of select="//*[local-name()='S5Loop1']/*[local-name()='L5Loop1']/*[local-name()='L5']/*[local-name()='L505']"/>
            </ns0:Code>
          </ns0:OuterPacksPackageType>
        </xsl:if>

        <xsl:variable name="PaymentType" select="//*[local-name()='B2']/*[local-name()='B206']"/>
        <xsl:if test="$PaymentType">
        <ns0:ShipmentIncoTerm>
          <ns0:Code>
            <xsl:value-of select="ScriptNS1:GetRecipientCode('TRXELPELP_G01' , 'TRXELPELP' , 'Genco 204 - Receive Shipments' , 'Payment Type' , 'ediEnterprise Code', $PaymentType)"/>
          </ns0:Code>
        </ns0:ShipmentIncoTerm>
        </xsl:if>

        <xsl:variable name="Weight" select="//*[local-name()='S5Loop1']/*[local-name()='AT8']/*[local-name()='AT803']"/>
        <xsl:if test="$Weight > 0">
          <ns0:TotalWeight>
            <xsl:value-of select="$Weight"/>
          </ns0:TotalWeight>
          <xsl:variable name="WeightUnitCode" select="ScriptNS1:GetRecipientCode('TRXELPELP_G01' , 'TRXELPELP' , 'Genco 204 - Receive Shipments' , 'Unit Of Measurement' , 'ediEnterprise Code', //*[local-name()='S5Loop1']/*[local-name()='AT8']/*[local-name()='AT802'])"/>
          <xsl:if test="$WeightUnitCode != ''">
            <ns0:TotalWeightUnit>
              <ns0:Code>
                <xsl:value-of select="$WeightUnitCode"/>
              </ns0:Code>
            </ns0:TotalWeightUnit>
          </xsl:if>
        </xsl:if>

        <ns0:TransportMode>
          <ns0:Code>
            <xsl:value-of select="ScriptNS1:GetRecipientCodeUnkeyed('TRXELPELP_G01' , 'TRXELPELP' , 'Genco 204 - Receive Shipments' , 'Defaults' , 'Transport Mode')"/>
          </ns0:Code>
        </ns0:TransportMode>

        <xsl:variable name="HouseBill" select="//*[local-name()='S5Loop1']/*[local-name()='L11_3' and *[local-name()='L1102'] = 'BM']/*[local-name()='L1101']"/>
        <xsl:if test="$HouseBill != ''">
          <ns0:WayBillNumber>
            <xsl:value-of select="$HouseBill"/>
          </ns0:WayBillNumber>
          <ns0:WayBillType>
            <ns0:Code>
              <xsl:text>HWB</xsl:text>
            </ns0:Code>
          </ns0:WayBillType>
        </xsl:if>

        <ns0:LocalProcessing>
          <xsl:variable name="DlvReqBy" select="//*[local-name()='S5Loop1']/*[local-name()='G62_2' and *[local-name()='G6201'] ='54']"/>
          <xsl:if test="$DlvReqBy != ''">
            <ns0:DeliveryRequiredBy>
              <xsl:value-of select="ScriptNS0:ConvertToDateTimeString($DlvReqBy/*[local-name()='G6202'], 'yyyyMMdd', $DlvReqBy/*[local-name()='G6204'], 'HHmm')"/>
            </ns0:DeliveryRequiredBy>
          </xsl:if>

          <xsl:variable name="EstDlv" select="//*[local-name()='S5Loop1']/*[local-name()='G62_2' and *[local-name()='G6201'] ='53']"/>
          <xsl:if test="$EstDlv != ''">
            <ns0:EstimatedDelivery>
              <xsl:value-of select="ScriptNS0:ConvertToDateTimeString($EstDlv/*[local-name()='G6202'], 'yyyyMMdd', $EstDlv/*[local-name()='G6204'], 'HHmm')"/>
            </ns0:EstimatedDelivery>
          </xsl:if>

          <xsl:variable name="EstPickUp" select="//*[local-name()='S5Loop1']/*[local-name()='G62_2' and *[local-name()='G6201'] ='37']"/>
          <xsl:if test="$EstPickUp != ''">
            <ns0:EstimatedPickup>
              <xsl:value-of select="ScriptNS0:ConvertToDateTimeString($EstPickUp/*[local-name()='G6202'], 'yyyyMMdd', $EstPickUp/*[local-name()='G6204'], 'HHmm')"/>
            </ns0:EstimatedPickup>
          </xsl:if>

          <xsl:variable name="PickUpReqBy" select="//*[local-name()='S5Loop1']/*[local-name()='G62_2' and *[local-name()='G6201'] ='38']"/>
          <xsl:if test="$PickUpReqBy != ''">
            <ns0:PickupRequiredBy>
              <xsl:value-of select="ScriptNS0:ConvertToDateTimeString($PickUpReqBy/*[local-name()='G6202'], 'yyyyMMdd', $PickUpReqBy/*[local-name()='G6204'], 'HHmm')"/>
            </ns0:PickupRequiredBy>
          </xsl:if>

          <xsl:if test="//*[local-name()='S5Loop1'][1]/*[local-name()='L11_3' and *[local-name()='L1102'] = 'PO']">
            <ns0:OrderNumberCollection>
              <xsl:for-each select="//*[local-name()='S5Loop1'][1]/*[local-name()='L11_3' and *[local-name()='L1102'] = 'PO']">
                <ns0:OrderNumber>
                  <ns0:OrderReference>
                    <xsl:value-of select="./*[local-name()='L1101']"/>
                  </ns0:OrderReference>
                  <ns0:Sequence>
                    <xsl:value-of select="position()"/>
                  </ns0:Sequence>
                </ns0:OrderNumber>
              </xsl:for-each>
            </ns0:OrderNumberCollection>
          </xsl:if>
        </ns0:LocalProcessing>

        <xsl:if test="$ShipperRef != ''">
          <ns0:AdditionalReferenceCollection>
            <ns0:AdditionalReference>
              <ns0:ReferenceNumber>
                <xsl:value-of select="$ShipperRef"/>
              </ns0:ReferenceNumber>
              <ns0:Type>
                <ns0:Code>OAG</ns0:Code>
              </ns0:Type>
            </ns0:AdditionalReference>
          </ns0:AdditionalReferenceCollection>
        </xsl:if>

        <xsl:variable name="GencoCustomerNumber" select="//*[local-name()='N1Loop1']/*[local-name()='N1'][./*[local-name()='N101'] = 'BT']/*[local-name()='N104']" />
        <xsl:if test="$GencoCustomerNumber != ''">
          <xsl:element name="ns0:CustomizedFieldCollection">
            <xsl:element name="ns0:CustomizedField">

              <xsl:element name="ns0:Key">
                <xsl:text>Genco Customer Number</xsl:text>
              </xsl:element>

              <xsl:element name="ns0:DataType">
                <xsl:text>String</xsl:text>
              </xsl:element>

              <xsl:element name="ns0:Value">
                <xsl:value-of select="$GencoCustomerNumber"/>
              </xsl:element>

            </xsl:element>
          </xsl:element>
        </xsl:if>

        <xsl:variable name="Note" select="//*[local-name()='NTE']/*[local-name()='NTE02']"/>
        <xsl:variable name="GoodDesc" select="//*[local-name()='L5']/*[local-name()='L502']"/>
        <xsl:variable name="MarksNos" select="//*[local-name()='L5']/*[local-name()='L506']"/>
        <xsl:if test="$Note != '' or $GoodDesc != '' or $MarksNos != ''">
          <ns0:NoteCollection>
            <xsl:if test="$Note != ''">
              <ns0:Note>
                <ns0:Description>Special Instructions</ns0:Description>
                <ns0:IsCustomDescription>false</ns0:IsCustomDescription>
                <ns0:NoteText>
                  <xsl:value-of select="$Note"/>
                </ns0:NoteText>
                <ns0:NoteContext>
                  <ns0:Code>AAA</ns0:Code>
                </ns0:NoteContext>
              </ns0:Note>
            </xsl:if>

            <xsl:if test="$GoodDesc != ''">
              <ns0:Note>
                <ns0:Description>Detailed Goods Description</ns0:Description>
                <ns0:IsCustomDescription>false</ns0:IsCustomDescription>
                <ns0:NoteText>
                  <xsl:value-of select="$GoodDesc"/>
                </ns0:NoteText>
                <ns0:NoteContext>
                  <ns0:Code>AAA</ns0:Code>
                </ns0:NoteContext>
              </ns0:Note>
            </xsl:if>

            <xsl:if test="$MarksNos != ''">
              <ns0:Note>
                <ns0:Description>Marks &amp; Numbers</ns0:Description>
                <ns0:IsCustomDescription>false</ns0:IsCustomDescription>
                <ns0:NoteText>
                  <xsl:value-of select="$MarksNos"/>
                </ns0:NoteText>
                <ns0:NoteContext>
                  <ns0:Code>AAA</ns0:Code>
                </ns0:NoteContext>
              </ns0:Note>
            </xsl:if>
          </ns0:NoteCollection>
        </xsl:if>

        <xsl:variable name="ShipFrom" select="//*[local-name()='S5Loop1']/*[local-name()='N1Loop2' and *[local-name()='N1_2']/*[local-name()='N101'] = 'SH']"/>
        <xsl:variable name="Consignee" select="//*[local-name()='S5Loop1']/*[local-name()='N1Loop2' and *[local-name()='N1_2']/*[local-name()='N101'] = 'CN']"/>
        <xsl:variable name="LocalClient" select="ScriptNS1:GetRecipientCodeUnkeyed('TRXELPELP_G01' , 'TRXELPELP' , 'Genco 204 - Receive Shipments' , 'Defaults' , 'Local Client Code')"/>
        <xsl:variable name="DefaultCountry" select="ScriptNS1:GetRecipientCodeUnkeyed('TRXELPELP_G01' , 'TRXELPELP' , 'Genco 204 - Receive Shipments' , 'Defaults' , 'Origin/Destination')"/>
        <xsl:variable name="DestinationCountry" select="$Consignee/*[local-name()='N4_2']/*[local-name()='N404']"/>
        <xsl:variable name="OriginCountry" select="$ShipFrom/*[local-name()='N4_2']/*[local-name()='N404']"/>

        <ns0:PortOfDestination>
          <ns0:Code>
            <xsl:choose>
              <xsl:when test="$DestinationCountry != ''">
                <xsl:value-of select="$DestinationCountry"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$DefaultCountry"/>
              </xsl:otherwise>
            </xsl:choose>
          </ns0:Code>
        </ns0:PortOfDestination>

        <ns0:PortOfOrigin>
          <ns0:Code>
            <xsl:choose>
              <xsl:when test="$OriginCountry != ''">
                <xsl:value-of select="$OriginCountry"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$DefaultCountry"/>
              </xsl:otherwise>
            </xsl:choose>
          </ns0:Code>
        </ns0:PortOfOrigin>

        <ns0:OrganizationAddressCollection>
          <xsl:if test="$ShipFrom">
            <ns0:OrganizationAddress>
              <ns0:AddressType>ConsignorDocumentaryAddress</ns0:AddressType>
              <xsl:call-template name="PopulateOrgAddress">
                <xsl:with-param name="OrgDetails" select="$ShipFrom"/>
              </xsl:call-template>
            </ns0:OrganizationAddress>
          </xsl:if>
          <xsl:if test="$Consignee">
            <ns0:OrganizationAddress>
              <ns0:AddressType>ConsigneeDocumentaryAddress</ns0:AddressType>
              <xsl:call-template name="PopulateOrgAddress">
                <xsl:with-param name="OrgDetails" select="$Consignee"/>
              </xsl:call-template>
            </ns0:OrganizationAddress>
          </xsl:if>
          <ns0:OrganizationAddress>
            <ns0:AddressType>LocalClient</ns0:AddressType>
            <ns0:OrganizationCode>
              <xsl:value-of select="$LocalClient"/>
            </ns0:OrganizationCode>
          </ns0:OrganizationAddress>
        </ns0:OrganizationAddressCollection>

      </ns0:Shipment>
    </ns0:UniversalShipment>

  </xsl:template>

  <xsl:template name="PopulateOrgAddress">
    <xsl:param name="OrgDetails"/>

    <xsl:if test="$OrgDetails/*[local-name()='N1_2']/*[local-name()='N104'] != ''">
      <ns0:OrganizationCode>
        <xsl:value-of select="$OrgDetails/*[local-name()='N1_2']/*[local-name()='N104']"/>
      </ns0:OrganizationCode>
    </xsl:if>

    <xsl:if test="$OrgDetails/*[local-name()='N3_2']/*[local-name()='N301'] != ''">
      <ns0:Address1>
        <xsl:value-of select="$OrgDetails/*[local-name()='N3_2']/*[local-name()='N301']"/>
      </ns0:Address1>
      <xsl:if test="$OrgDetails/*[local-name()='N3_2']/*[local-name()='N302'] != ''">
        <ns0:Address2>
          <xsl:value-of select="$OrgDetails/*[local-name()='N3_2']/*[local-name()='N302']"/>
        </ns0:Address2>
      </xsl:if>
    </xsl:if>

    <xsl:if test="$OrgDetails/*[local-name()='N4_2']/*[local-name()='N401'] != ''">
      <ns0:City>
        <xsl:value-of select="$OrgDetails/*[local-name()='N4_2']/*[local-name()='N401']"/>
      </ns0:City>
    </xsl:if>

    <xsl:if test="$OrgDetails/*[local-name()='N1_2']/*[local-name()='N102'] != ''">
      <ns0:CompanyName>
        <xsl:value-of select="$OrgDetails/*[local-name()='N1_2']/*[local-name()='N102']"/>
      </ns0:CompanyName>
    </xsl:if>

    <xsl:if test="$OrgDetails/*[local-name()='N4_2']/*[local-name()='N404'] != ''">
      <ns0:Country>
        <ns0:Code>
          <xsl:value-of select="substring($OrgDetails/*[local-name()='N4_2']/*[local-name()='N404'],1,2)"/>
        </ns0:Code>
      </ns0:Country>
    </xsl:if>

    <xsl:if test="$OrgDetails/*[local-name()='N4_2']/*[local-name()='N403'] != ''">
      <ns0:Postcode>
        <xsl:value-of select="$OrgDetails/*[local-name()='N4_2']/*[local-name()='N403']"/>
      </ns0:Postcode>
    </xsl:if>

    <xsl:if test="$OrgDetails/*[local-name()='N4_2']/*[local-name()='N402'] != ''">
      <ns0:State>
        <xsl:value-of select="$OrgDetails/*[local-name()='N4_2']/*[local-name()='N402']"/>
      </ns0:State>
    </xsl:if>
  </xsl:template>

</xsl:stylesheet>

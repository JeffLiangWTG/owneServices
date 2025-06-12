<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl ScriptNS0 ScriptNS1" version="1.0"
                xmlns:ns0="http://cargowise.com/ehub/clients/TRX/2013/09"
                xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
								xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1">
  <xsl:output omit-xml-declaration="yes" indent="no" version="1.0" method="xml" />

  <xsl:key name="ChargeCodes" match="//*[local-name()='ChargeLine']" use="ScriptNS1:GetRecipientCode('TRXELPELP' , 'TRXELPELP_CSB' , 'Cisco file - Export Shipment &amp; Billing data' , 'Charge Code Categories' , 'Output Code', *[local-name()='ChargeCode']/*[local-name()='Code'])"/>
  <xsl:template match="/">
    <xsl:variable name="Shipment" select="(//*[local-name()='Shipment'][contains(./*[local-name()='DataContext']/*[local-name()='DataSourceCollection'], 'ForwardingShipment') and 
                                                           not(contains(./*[local-name()='DataContext']/*[local-name()='DataSourceCollection'], 'ForwardingConsol'))] | 
                                  //*[local-name()='SubShipment'][contains(./*[local-name()='DataContext']/*[local-name()='DataSourceCollection'], 'ForwardingShipment')])[1]" />
    
    <xsl:variable name="Consol" select="//*[local-name()='Shipment'][contains(./*[local-name()='DataContext']/*[local-name()='DataSourceCollection'], 'ForwardingConsol')][1]"/>

    <xsl:variable name="Consignee" select="$Shipment/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress' and *[local-name()='AddressType'] = 'ConsigneeDocumentaryAddress']"/>
    <xsl:variable name="Consignor" select="$Shipment/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress' and *[local-name()='AddressType'] = 'ConsignorDocumentaryAddress']"/>
    <xsl:variable name="LocalClient" select="$Shipment/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress' and *[local-name()='AddressType'] = 'SendersLocalClient']"/>

    <ns0:ShipmentBilling>

      <DataSource>
        <xsl:value-of select="/*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='DataContext']/*[local-name()='DataProvider']"/>
      </DataSource>

      <TruckProBOLHAWBWayBill>
        <xsl:value-of select="$Shipment/*[local-name()='WayBillNumber']"/>
      </TruckProBOLHAWBWayBill>

      <xsl:variable name="Direction">
        <xsl:if test="$Consignor/*[local-name()='OrganizationCode'] = $LocalClient/*[local-name()='OrganizationCode']">
          <xsl:value-of select="'Outbound'"/>
        </xsl:if>
        <xsl:if test="$Consignee/*[local-name()='OrganizationCode'] = $LocalClient/*[local-name()='OrganizationCode']">
          <xsl:value-of select="'Inbound'"/>
        </xsl:if>

      </xsl:variable>

      <CiscoInboundOutbound>
        <xsl:value-of select="$Direction"/>
      </CiscoInboundOutbound>

      <xsl:variable name="CostString">
        <xsl:for-each select="//*[local-name()='ChargeLine'][generate-id() = generate-id(key('ChargeCodes', ScriptNS1:GetRecipientCode('TRXELPELP' , 'TRXELPELP_CSB' , 'Cisco file - Export Shipment &amp; Billing data' , 'Charge Code Categories' , 'Output Code', *[local-name()='ChargeCode']/*[local-name()='Code']))[1])]">
          <xsl:variable name="Sum" select="format-number(sum(key('ChargeCodes',
                        ScriptNS1:GetRecipientCode('TRXELPELP' , 'TRXELPELP_CSB' , 'Cisco file - Export Shipment &amp; Billing data' , 'Charge Code Categories' , 'Output Code', *[local-name()='ChargeCode']/*[local-name()='Code']) )[*[local-name()='Debtor']/*[local-name()='Key'] = $LocalClient/*[local-name()='OrganizationCode'] and *[local-name()='SellIsPosted'] = 'true']/*[local-name()='SellLocalAmount']), '0.##')"/>
          <xsl:variable name="ChargeCode" select="ScriptNS1:GetRecipientCode('TRXELPELP' , 'TRXELPELP_CSB' , 'Cisco file - Export Shipment &amp; Billing data' , 'Charge Code Categories' , 'Output Code', *[local-name()='ChargeCode']/*[local-name()='Code'])"/>

          <xsl:choose>
            <xsl:when test="$ChargeCode = 'Freight'">
              <xsl:value-of select="'Freight:'"/>
              <xsl:value-of select="$Sum"/>
              <xsl:value-of select="','"/>
            </xsl:when>
            <xsl:when test="$ChargeCode = 'Fuel'">
              <xsl:value-of select="'Fuel:'"/>
              <xsl:value-of select="$Sum"/>
              <xsl:value-of select="','"/>
            </xsl:when>
            <xsl:when test="$ChargeCode = 'Accessorial'">
              <xsl:value-of select="'Accessorial:'"/>
              <xsl:value-of select="$Sum"/>
              <xsl:value-of select="','"/>
            </xsl:when>
            <xsl:when test="$ChargeCode = 'Other'">
              <xsl:value-of select="'Other:'"/>
              <xsl:value-of select="$Sum"/>
              <xsl:value-of select="','"/>
            </xsl:when>
          </xsl:choose>
          
        </xsl:for-each>
      </xsl:variable>

      <xsl:variable name="FreightCostValue">
        <xsl:variable name="Temp" select="substring-before(substring-after($CostString, 'Freight:'), ',')" />
        <xsl:choose>

          <xsl:when test="$Temp != ''">
            <xsl:value-of select="$Temp"/>
          </xsl:when>

          <xsl:otherwise>
            <xsl:value-of select="0"/>
          </xsl:otherwise>

        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="FuelCostValue">
        <xsl:variable name="Temp" select="substring-before(substring-after($CostString, 'Fuel:'), ',')" />
        <xsl:choose>

          <xsl:when test="$Temp != ''">
            <xsl:value-of select="$Temp"/>
          </xsl:when>

          <xsl:otherwise>
            <xsl:value-of select="0"/>
          </xsl:otherwise>

        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="AccessorialCostValue">
        <xsl:variable name="Temp" select="substring-before(substring-after($CostString, 'Accessorial:'), ',')" />
        <xsl:choose>

          <xsl:when test="$Temp != ''">
            <xsl:value-of select="$Temp"/>
          </xsl:when>

          <xsl:otherwise>
            <xsl:value-of select="0"/>
          </xsl:otherwise>

        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="OtherCostValue">
        <xsl:variable name="Temp" select="substring-before(substring-after($CostString, 'Other:'), ',')" />
        <xsl:choose>

          <xsl:when test="$Temp != ''">
            <xsl:value-of select="$Temp"/>
          </xsl:when>

          <xsl:otherwise>
            <xsl:value-of select="0"/>
          </xsl:otherwise>

        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="TotalCost" select="$FreightCostValue + $FuelCostValue + $AccessorialCostValue + $OtherCostValue"/>

      <OptIn>
        <xsl:choose>
          <xsl:when test="$TotalCost = 0">
            <xsl:value-of select="'N'"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="'Y'"/>
          </xsl:otherwise>
        </xsl:choose>
      </OptIn>

      <xsl:variable name="PortOfLoading" select="$Consol/*[local-name()='PortOfLoading']/*[local-name()='Code']"/>
      <xsl:variable name="PortOfDischarge" select="$Consol/*[local-name()='PortOfDischarge']/*[local-name()='Code']"/>

      <xsl:if test="$PortOfLoading">
        <UNOriginAirport>
          <xsl:value-of select="substring($PortOfLoading,3,3)"/>
        </UNOriginAirport>

        <UNOriginCountry>
          <xsl:value-of select="substring($PortOfLoading,1,2)"/>
        </UNOriginCountry>
      </xsl:if>

      <xsl:if test="$PortOfDischarge">
        <UNDestAirportPU>
          <xsl:value-of select="substring($PortOfDischarge,3,3)"/>
        </UNDestAirportPU>

        <UNDestCountry>
          <xsl:value-of select="substring($PortOfDischarge,1,2)"/>
        </UNDestCountry>
      </xsl:if>

      <DelDestination>
        <xsl:value-of select="substring($Shipment/*[local-name()='PortOfDestination']/*[local-name()='Code'],3,3)"/>
      </DelDestination>

      <ShipLocalDateTime>
        <xsl:value-of select="ScriptNS0:FormatXmlDateTime($Shipment/*[local-name()='DateCollection']/*[local-name()='Date' and *[local-name()='Type'] = 'Departure']/*[local-name()='Value'], 'yyy-MM-ddTHH:mmzzz')"/>
      </ShipLocalDateTime>

      <DeliveryLocalDateTime>
        <xsl:value-of select="ScriptNS0:FormatXmlDateTime($Shipment/*[local-name()='DateCollection']/*[local-name()='Date' and *[local-name()='Type'] = 'Arrival']/*[local-name()='Value'], 'yyy-MM-ddTHH:mmzzz')"/>
      </DeliveryLocalDateTime>

      <Mode>
        <xsl:value-of select="ScriptNS1:GetRecipientCode('TRXELPELP' , 'TRXELPELP_CSB' , 'Cisco file - Export Shipment &amp; Billing data' , 'Transport Mode' , 'Output Code', $Shipment/*[local-name()='TransportMode']/*[local-name()='Code'])"/>
      </Mode>

      <ServiceLevel>
        <xsl:value-of select="ScriptNS1:GetRecipientCode('TRXELPELP' , 'TRXELPELP_CSB' , 'Cisco file - Export Shipment &amp; Billing data' , 'Service Level' , 'Output Code', $Shipment/*[local-name()='ServiceLevel']/*[local-name()='Code'])"/>
      </ServiceLevel>

      <xsl:if test="$Consol != '' and $Direction = 'Inbound'">
        <Forwarder>
          <xsl:value-of select="$Consol/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress' and *[local-name()='AddressType'] = 'SendingForwarderAddress']/*[local-name()='OrganizationCode']"/>
        </Forwarder>
      </xsl:if>
      <xsl:if test="$Consol != '' and $Direction = 'Outbound'">
        <Forwarder>
          <xsl:value-of select="$Consol/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress' and *[local-name()='AddressType'] = 'ReceivingForwarderAddress']/*[local-name()='OrganizationCode']"/>
        </Forwarder>
      </xsl:if>

      <xsl:if test="$Shipment/*[local-name()='OuterPacks'] > 0">
        <CartonCount>
          <xsl:value-of select="$Shipment/*[local-name()='OuterPacks']"/>
        </CartonCount>
      </xsl:if>

      <xsl:if test="$Shipment/*[local-name()='TotalVolume'] > 0">
        <Volume>
          <xsl:value-of select="$Shipment/*[local-name()='TotalVolume']"/>
        </Volume>

        <VolumeUnit>
          <xsl:value-of select="ScriptNS1:GetRecipientCode('TRXELPELP' , 'TRXELPELP_CSB' , 'Cisco file - Export Shipment &amp; Billing data' , 'Unit Of Measurement' , 'Output Code', $Shipment/*[local-name()='TotalVolumeUnit']/*[local-name()='Code'])"/>
        </VolumeUnit>
      </xsl:if>

      <xsl:if test="$Shipment/*[local-name()='TotalWeight'] > 0">
        <ActualWeight>
          <xsl:value-of select="$Shipment/*[local-name()='TotalWeight']"/>
        </ActualWeight>
      </xsl:if>

      <xsl:if test="$Shipment/*[local-name()='TotalWeight'] > 0">
        <ChargeableWeight>
          <xsl:value-of select="$Shipment/*[local-name()='ActualChargeable']"/>
        </ChargeableWeight>
        <WeightUnit>
          <xsl:value-of select="ScriptNS1:GetRecipientCode('TRXELPELP' , 'TRXELPELP_CSB' , 'Cisco file - Export Shipment &amp; Billing data' , 'Unit Of Measurement' , 'Output Code', $Shipment/*[local-name()='TotalWeightUnit']/*[local-name()='Code'])"/>
        </WeightUnit>
      </xsl:if>

      <FreightCost>
        <xsl:value-of select="$FreightCostValue"/>
      </FreightCost>
      
      <FuelCost>
        <xsl:value-of select="$FuelCostValue"/>
      </FuelCost>
      
      <AccessorialCost>
        <xsl:value-of select="$AccessorialCostValue"/>
      </AccessorialCost>
      
      <OtherCost>
        <xsl:value-of select="$OtherCostValue"/>
      </OtherCost>

      <TotalCost>
        <xsl:value-of select="$TotalCost"/>
      </TotalCost>

      <Ccy>
        <xsl:value-of select="ScriptNS1:GetRecipientCodeUnkeyed('TRXELPELP' , 'TRXELPELP_CSB' , 'Cisco file - Export Shipment &amp; Billing data' , 'Defaults' , 'Currency')"/>
      </Ccy>

      <xsl:call-template name="MapValueIfNotEmpty">
        <xsl:with-param name="NodeName" select="'ShipFromShipperID'" />
        <xsl:with-param name="Value" select="$Consignor/*[local-name()='OrganizationCode']"/>
      </xsl:call-template>

      <xsl:call-template name="MapValueIfNotEmpty">
        <xsl:with-param name="NodeName" select="'ShipFromShipper'" />
        <xsl:with-param name="Value" select="$Consignor/*[local-name()='CompanyName']"/>
      </xsl:call-template>

      <xsl:call-template name="MapValueIfNotEmpty">
        <xsl:with-param name="NodeName" select="'ShipperAddress'" />
        <xsl:with-param name="Value" select="$Consignor/*[local-name()='Address1']"/>
      </xsl:call-template>

      <xsl:call-template name="MapValueIfNotEmpty">
        <xsl:with-param name="NodeName" select="'ShipperCity'" />
        <xsl:with-param name="Value" select="$Consignor/*[local-name()='City']"/>
      </xsl:call-template>

      <xsl:call-template name="MapValueIfNotEmpty">
        <xsl:with-param name="NodeName" select="'ShipperStateProvince'" />
        <xsl:with-param name="Value" select="$Consignor/*[local-name()='State']"/>
      </xsl:call-template>

      <xsl:call-template name="MapValueIfNotEmpty">
        <xsl:with-param name="NodeName" select="'ShipperCountry'" />
        <xsl:with-param name="Value" select="$Consignor/*[local-name()='Country']/*[local-name()='Name']"/>
      </xsl:call-template>

      <xsl:call-template name="MapValueIfNotEmpty">
        <xsl:with-param name="NodeName" select="'ShipperUNCountryCode'" />
        <xsl:with-param name="Value" select="$Consignor/*[local-name()='Country']/*[local-name()='Code']"/>
      </xsl:call-template>

      <xsl:call-template name="MapValueIfNotEmpty">
        <xsl:with-param name="NodeName" select="'ShipperPostalCode'" />
        <xsl:with-param name="Value" select="$Consignor/*[local-name()='Postcode']"/>
      </xsl:call-template>

      <xsl:call-template name="MapValueIfNotEmpty">
        <xsl:with-param name="NodeName" select="'ShipToConsignee'" />
        <xsl:with-param name="Value" select="$Consignee/*[local-name()='CompanyName']"/>
      </xsl:call-template>

      <xsl:call-template name="MapValueIfNotEmpty">
        <xsl:with-param name="NodeName" select="'ConsigneeAddress'" />
        <xsl:with-param name="Value" select="$Consignee/*[local-name()='Address1']"/>
      </xsl:call-template>

      <xsl:call-template name="MapValueIfNotEmpty">
        <xsl:with-param name="NodeName" select="'ConsigneeCity'" />
        <xsl:with-param name="Value" select="$Consignee/*[local-name()='City']"/>
      </xsl:call-template>

      <xsl:call-template name="MapValueIfNotEmpty">
        <xsl:with-param name="NodeName" select="'ConsigneeStateProvince'" />
        <xsl:with-param name="Value" select="$Consignee/*[local-name()='State']"/>
      </xsl:call-template>

      <xsl:call-template name="MapValueIfNotEmpty">
        <xsl:with-param name="NodeName" select="'ConsigneeCountry'" />
        <xsl:with-param name="Value" select="$Consignee/*[local-name()='Country']/*[local-name()='Name']"/>
      </xsl:call-template>

      <xsl:call-template name="MapValueIfNotEmpty">
        <xsl:with-param name="NodeName" select="'ConsigneePostalCode'" />
        <xsl:with-param name="Value" select="$Consignee/*[local-name()='Postcode']"/>
      </xsl:call-template>
      
      <xsl:call-template name="MapValueIfNotEmpty">
        <xsl:with-param name="NodeName" select="'ConsigneeUNCountryCode'" />
        <xsl:with-param name="Value" select="$Consignee/*[local-name()='Country']/*[local-name()='Code']"/>
      </xsl:call-template>

    </ns0:ShipmentBilling>
  </xsl:template>

  <xsl:template name="MapValueIfNotEmpty">
    <xsl:param name="NodeName" />
    <xsl:param name="Value" />
    <xsl:if test="$Value != '' and string-length($Value) != 0">
      <xsl:element name="{$NodeName}">
        <xsl:value-of select="translate($Value, ':', '')" />
      </xsl:element>
    </xsl:if>
  </xsl:template>

</xsl:stylesheet>
<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 userCSharp ScriptNS0 ScriptNS1 ns0" version="1.0"
                xmlns:ns0="http://cargowise.com/ehub/clients/TRX/2013/07"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp"
                xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
                xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1">
  <xsl:output omit-xml-declaration="yes" indent="no" version="1.0" method="xml" />

  <xsl:template match="/">
    <ns0:X12_00401_110>

      <xsl:variable name="Consol" select="//*[local-name()='Shipment'][contains(./*[local-name()='DataContext']/*[local-name()='DataSourceCollection'], 'ForwardingConsol')]"/>
      <xsl:variable name="Shipment" select="(//*[local-name()='Shipment'][contains(./*[local-name()='DataContext']/*[local-name()='DataSourceCollection'], 'ForwardingShipment') and 
                                                           not(contains(./*[local-name()='DataContext']/*[local-name()='DataSourceCollection'], 'ForwardingConsol'))] | 
                                  //*[local-name()='SubShipment'][contains(./*[local-name()='DataContext']/*[local-name()='DataSourceCollection'], 'ForwardingShipment')])[1]" />
      <xsl:variable name="SCAC" select="ScriptNS0:GetRecipientCodeUnkeyed('TRXELPELP' , 'TRXELPELP_SPR' , 'Sprint Wireless 110 - Export A/R Invoices' , 'Defaults' , 'SCAC')"/>
      <xsl:variable name="IncoTerm" select="$Shipment/*[local-name()='ShipmentIncoTerm']/*[local-name()='Code']"/>

      <ST>
        <ST01>110</ST01>
        <ST02>0001</ST02>
      </ST>

      <ns0:B3>
        <B302>
          <xsl:value-of select="//*[local-name()='TransactionInfo']/*[local-name()='Number']"/>
        </B302>
        <B304>
          <xsl:value-of select="ScriptNS0:GetRecipientCode('TRXELPELP' , 'TRXELPELP_SPR' , 'Sprint Wireless 110 - Export A/R Invoices', 'Payment Method', 'X12 Code', $IncoTerm)"/>
        </B304>
        <B306>
          <xsl:value-of select="ScriptNS1:ConvertXmlDateString(//*[local-name()='TransactionInfo']/*[local-name()='TransactionDate'], 'yyyyMMdd')"/>
        </B306>
        <B307>
          <xsl:choose>
            <xsl:when test="//*[local-name()='TransactionInfo']/*[local-name()='OSTotal'] != ''">
              <xsl:value-of select="format-number(//*[local-name()='TransactionInfo']/*[local-name()='OSTotal'] * 100, '0')"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'0'"/>
            </xsl:otherwise>
          </xsl:choose>
        </B307>
        <B311>
          <xsl:value-of select="$SCAC"/>
        </B311>
      </ns0:B3>

      <xsl:variable name="OSCurrencyCode" select="//*[local-name()='TransactionInfo']/*[local-name()='OSCurrency']/*[local-name()='Code']"/>
      <xsl:if test="$OSCurrencyCode != ''">
        <ns0:C3>
          <C301>
            <xsl:value-of select="$OSCurrencyCode"/>
          </C301>
        </ns0:C3>
      </xsl:if>

      <!-- generate bill to address -->
      <xsl:variable name ="BillToAddr" select="//*[local-name()='TransactionInfo']/*[local-name()='OrganizationAddress']"/>
      <xsl:if test="$BillToAddr">
        <xsl:element name ="ns0:N1Loop1">
          <xsl:call-template name="GenerateOrgAddress">
            <xsl:with-param name="Org" select="$BillToAddr" />
            <xsl:with-param name="EntityIdCode" select="'BT'" />
          </xsl:call-template>
        </xsl:element>
      </xsl:if>


      <xsl:variable name="DischargeCode" select="$Consol/*[local-name()='PortOfDischarge']/*[local-name()='Code']"/>
      <xsl:variable name="LoadingCode" select="$Consol/*[local-name()='PortOfLoading']/*[local-name()='Code']"/>
      <xsl:variable name="TotalWeight" select="$Shipment/*[local-name()='TotalWeight']"/>
      <xsl:variable name="TotalWeightUQ" select="ScriptNS0:GetRecipientCode('TRXELPELP' , 'TRXELPELP_SPR' , 'Sprint Wireless 110 - Export A/R Invoices', 'Unit of Measurement' , 'X12 Code' , $Shipment/*[local-name()='TotalWeightUnit']/*[local-name()='Code'])"/>
      <xsl:variable name="TotalVolume" select="$Shipment/*[local-name()='TotalVolume']"/>
      <xsl:variable name="TotalVolumeUQ" select="ScriptNS0:GetRecipientCode('TRXELPELP' , 'TRXELPELP_SPR' , 'Sprint Wireless 110 - Export A/R Invoices', 'Unit of Measurement' , 'X12 Code' , $Shipment/*[local-name()='TotalVolumeUnit']/*[local-name()='Code'])"/>
      <xsl:variable name="TotalPacks" select="$Shipment/*[local-name()='OuterPacks']"/>
      <xsl:variable name="PackType" select="ScriptNS0:GetRecipientCode('TRXELPELP' , 'TRXELPELP_SPR' , 'Sprint Wireless 110 - Export A/R Invoices', 'Pack Type' , 'X12 Code' , $Shipment/*[local-name()='OuterPacksPackageType']/*[local-name()='Code'])"/>
      <xsl:variable name ="BilledWeight" select ="$Shipment/*[local-name()='ActualChargeable']"/>
      <xsl:variable name="MAWB" select ="$Consol/*[local-name()='WayBillNumber'][../*[local-name()='WayBillType'][*[local-name()='Code'] = 'MWB']]"/>

      <!-- LX Loop -->
      <xsl:element name ="ns0:LXLoop1">
        <xsl:element name ="ns0:LX">
          <LX01>1</LX01>
        </xsl:element>

        <!-- generate consignor address -->
        <xsl:variable name ="ConsignorAddr" select="$Shipment/*[local-name()='OrganizationAddressCollection']
                    /*[local-name()='OrganizationAddress' and *[local-name()='AddressType']/text()='ConsignorDocumentaryAddress']"/>

        <xsl:if test="$ConsignorAddr">
          <xsl:element name ="ns0:N1Loop2">
            <xsl:call-template name="GenerateOrgAddress">
              <xsl:with-param name="Org" select="$ConsignorAddr" />
              <xsl:with-param name="EntityIdCode" select="'SH'" />
            </xsl:call-template>
          </xsl:element>
        </xsl:if>

        <!-- generate consignee address -->
        <xsl:variable name ="ConsigneeAddr" select="$Shipment/*[local-name()='OrganizationAddressCollection']
                    /*[local-name()='OrganizationAddress' and *[local-name()='AddressType']/text()='ConsigneeDocumentaryAddress']"/>

        <xsl:if test="$ConsigneeAddr">
          <xsl:element name ="ns0:N1Loop2">
            <xsl:call-template name="GenerateOrgAddress">
              <xsl:with-param name="Org" select="$ConsigneeAddr" />
              <xsl:with-param name="EntityIdCode" select="'CN'" />
            </xsl:call-template>

            <xsl:for-each select="$Shipment/*[local-name()='LocalProcessing']/*[local-name()='OrderNumberCollection']/*[local-name()='OrderNumber']">
              <ns0:N9_2>
                <N901>PO</N901>
                <N902>
                  <xsl:value-of select="*[local-name()='OrderReference']"/>
                </N902>
              </ns0:N9_2>
            </xsl:for-each>

            <xsl:variable name="CustomerRef" select="$Shipment/*[local-name()='BookingConfirmationReference']"/>
            <xsl:if test="$CustomerRef != ''">
              <ns0:N9_2>
                <N901>CR</N901>
                <N902>
                  <xsl:value-of select="$CustomerRef"/>
                </N902>
              </ns0:N9_2>
            </xsl:if>

            <xsl:variable name="ShipmentID" select="$Shipment/*[local-name()='DataContext']/*[local-name()='DataSourceCollection']
                            /*[local-name()='DataSource' and *[local-name()='Type'] = 'ForwardingShipment']/*[local-name()='Key']"/>
            <xsl:if test="$ShipmentID != ''">
              <ns0:N9_2>
                <N901>2I</N901>
                <N902>
                  <xsl:value-of select="$ShipmentID"/>
                </N902>
              </ns0:N9_2>
            </xsl:if>

            <xsl:if test="$MAWB != ''">
              <ns0:N9_2>
                <N901>MB</N901>
                <N902>
                  <xsl:value-of select="$MAWB"/>
                </N902>
              </ns0:N9_2>
            </xsl:if>

            <xsl:if test="$IncoTerm != '' and substring($Shipment/*[local-name()='PortOfDestination']/*[local-name()='Code'],1,2) != substring($Shipment/*[local-name()='PortOfOrigin']/*[local-name()='Code'],1,2)">
              <ns0:N9_2>
                <N901>TOD</N901>
                <N902>
                  <xsl:value-of select="$IncoTerm"/>
                </N902>
              </ns0:N9_2>
            </xsl:if>

            <xsl:if test="$DischargeCode != ''">
              <ns0:N9_2>
                <N901>POD</N901>
                <N902>
                  <xsl:value-of select="$DischargeCode"/>
                </N902>
              </ns0:N9_2>
            </xsl:if>

            <xsl:if test="$LoadingCode != ''">
              <ns0:N9_2>
                <N901>POL</N901>
                <N902>
                  <xsl:value-of select="$LoadingCode"/>
                </N902>
              </ns0:N9_2>
            </xsl:if>
          </xsl:element>
        </xsl:if>

        <xsl:variable name="ActualPickUp" select="$Shipment/*[local-name()='LocalProcessing']/*[local-name()='PickupCartageCompleted']"/>
        <xsl:variable name="PickUpTime">
          <xsl:choose>
            <xsl:when test="$ActualPickUp != ''">
              <xsl:value-of select="$ActualPickUp"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$Shipment/*[local-name()='LocalProcessing']/*[local-name()='EstimatedPickup']"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <xsl:if test="$PickUpTime != ''">
          <ns0:P1>
            <P101>SD</P101>
            <P102>
              <xsl:value-of select="ScriptNS1:ConvertXmlDateString($PickUpTime, 'yyyyMMdd')"/>
            </P102>
            <P103>011</P103>
            <P104>
              <xsl:value-of select="ScriptNS1:ConvertXmlDateString($PickUpTime, 'HHmm')"/>
            </P104>
          </ns0:P1>
        </xsl:if>

        <xsl:variable name="LocalCountry" select="//*[local-name()='TransactionInfo']/*[local-name()='DataContext']/*[local-name()='Company']/*[local-name()='Country']/*[local-name()='Code']"/>

        <xsl:element name="ns0:R1">
          <R101>
            <xsl:value-of select="$SCAC"/>
          </R101>

          <xsl:variable name="OriginUNLOCO">
            <xsl:choose>
              <xsl:when test="$Consol/*[local-name()='PortOfLoading']/*[local-name()='Code'] != ''">
                <xsl:value-of select="$Consol/*[local-name()='PortOfLoading']/*[local-name()='Code']"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$Shipment/*[local-name()='PortOfOrigin']/*[local-name()='Code']"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <xsl:variable name="Origin" select="substring($OriginUNLOCO, 3, 3)"/>
          <xsl:if test="$Origin != ''">
            <R103>
              <xsl:value-of select="$Origin"/>
            </R103>
          </xsl:if>
          <xsl:variable name="AirCarrierCode" select="substring($MAWB, 1, 3)"/>
          <xsl:if test ="$AirCarrierCode != ''">
            <R104>
              <xsl:value-of select="$AirCarrierCode"/>
            </R104>
          </xsl:if>
          
          <xsl:variable name="DestinationUNLOCO">
            <xsl:choose>
              <xsl:when test="$Consol/*[local-name()='PortOfDischarge']/*[local-name()='Code'] != ''">
                <xsl:value-of select="$Consol/*[local-name()='PortOfDischarge']/*[local-name()='Code']"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$Shipment/*[local-name()='PortOfDestination']/*[local-name()='Code']"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <xsl:variable name="Destination" select="substring($DestinationUNLOCO, 3, 3)"/>
          <xsl:if test="$Destination != ''">
            <R105>
              <xsl:value-of select="$Destination"/>
            </R105>
          </xsl:if>
        </xsl:element>

        <xsl:variable name="DeliveryTime" select="$Shipment/*[local-name()='LocalProcessing']/*[local-name()='DeliveryCartageCompleted']"/>
        <xsl:if test="$DeliveryTime != ''">
          <ns0:POD>
            <POD01>
              <xsl:value-of select="ScriptNS1:ConvertXmlDateString($DeliveryTime, 'yyyyMMdd')"/>
            </POD01>
            <POD02>
              <xsl:value-of select="ScriptNS1:ConvertXmlDateString($DeliveryTime, 'HHmm')"/>
            </POD02>
            <POD03>
              <xsl:value-of select="ScriptNS0:GetRecipientCodeUnkeyed('TRXELPELP' , 'TRXELPELP_SPR' , 'Sprint Wireless 110 - Export A/R Invoices' , 'Defaults' , 'Goods Signed By')"/>
            </POD03>
          </ns0:POD>
        </xsl:if>

        <xsl:element name="ns0:L5Loop1">
          <xsl:element name="ns0:L5">
            <L501>1</L501>

            <xsl:variable name="GoodsDescNote" select ="normalize-space($Shipment/*[local-name()='NoteCollection']/*[local-name()='Note' and *[local-name()='Description']/text()='Detailed Goods Description']/*[local-name()='NoteText'])"/>
            <xsl:variable name="GoodsDesc" select ="normalize-space($Shipment/*[local-name()='GoodsDescription'])"/>
            <xsl:if test = "$GoodsDescNote != '' or $GoodsDesc!=''">
              <L502>
                <xsl:choose>
                  <xsl:when test="$GoodsDescNote != ''">
                    <xsl:value-of select="substring($GoodsDescNote, 1, 50)"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="substring($GoodsDesc, 1, 50)"/>
                  </xsl:otherwise>
                </xsl:choose>
              </L502>
            </xsl:if>

          </xsl:element>

          <xsl:element name ="ns0:L0">
            <L001>1</L001>

            <xsl:if test="$TotalWeight > 0">
              <L004>
                <xsl:value-of select="format-number($TotalWeight, '0.##')"/>
              </L004>
              <L005>G</L005>
            </xsl:if>

            <xsl:if test="$TotalVolume > 0">
              <L006>
                <xsl:value-of select="format-number($TotalVolume, '0.##')"/>
              </L006>
              <xsl:if test="$TotalVolumeUQ != ''">
                <L007>
                  <xsl:value-of select="$TotalVolumeUQ"/>
                </L007>
              </xsl:if>
            </xsl:if>
            <xsl:if test="$TotalPacks > 0">
              <L008>
                <xsl:value-of select="$TotalPacks"/>
              </L008>
              <xsl:if test="$PackType != ''">
                <L009>
                  <xsl:value-of select="$PackType"/>
                </L009>
              </xsl:if>
            </xsl:if>
            <xsl:if test="$TotalWeight > 0 and $TotalWeightUQ != ''">
              <L011>
                <xsl:value-of select="$TotalWeightUQ"/>
              </L011>
            </xsl:if>
          </xsl:element>

          <xsl:if test ="$TotalWeight > 0">
            <xsl:element name="ns0:L10">
              <L1001>
                <xsl:value-of select ="format-number($TotalWeight, '0.##')"/>
              </L1001>
              <L1002>G</L1002>
              <xsl:if test="$TotalWeightUQ != ''">
                <L1003>
                  <xsl:value-of select="$TotalWeightUQ"/>
                </L1003>
              </xsl:if>
            </xsl:element>
          </xsl:if>

          <xsl:if test ="$BilledWeight > 0">
            <xsl:element name="ns0:L10">
              <L1001>
                <xsl:value-of select ="format-number($BilledWeight, '0.##')"/>
              </L1001>
              <L1002>B</L1002>
            </xsl:element>
          </xsl:if>

          <!-- service level -->
          <xsl:variable name="ServiceLevel" select="ScriptNS0:GetRecipientCode('TRXELPELP' , 'TRXELPELP_SPR' , 'Sprint Wireless 110 - Export A/R Invoices' , 'Service Level' , 'X12 Code' , $Shipment/*[local-name()='ServiceLevel']/*[local-name()='Code'])"/>
          <xsl:if test="$ServiceLevel != ''">
            <xsl:element name ="ns0:SL1">
              <SL101>
                <xsl:value-of select="$ServiceLevel"/>
              </SL101>
            </xsl:element>
          </xsl:if>

          <xsl:for-each select="//*[local-name()='TransactionInfo']/*[local-name()='PostingJournalCollection']/*[local-name()='PostingJournal']">
            <xsl:sort select="*[local-name()='Sequence']"/>
            <xsl:element name ="ns0:L1Loop1">
              <xsl:element name ="ns0:L1">
                <L101>
                  <xsl:value-of select="*[local-name()='Sequence']" />
                </L101>
                <xsl:variable name="Charge" select="./*[local-name()='OSAmount']"/>
                <xsl:if test="$Charge > 0">
                  <L102>
                    <xsl:value-of select="format-number($Charge, '0.##')" />
                  </L102>
                  <L103>
                    <xsl:value-of select="ScriptNS0:GetRecipientCodeUnkeyed('TRXELPELP' , 'TRXELPELP_SPR' , 'Sprint Wireless 110 - Export A/R Invoices' , 'Defaults' , 'Rate Basis')"/>
                  </L103>
                  <L104>
                    <xsl:value-of select="format-number($Charge * 100, '0')"/>
                  </L104>
                </xsl:if>

                <xsl:variable name="ChargeCode" select="ScriptNS0:GetRecipientCode('TRXELPELP' , 'TRXELPELP_SPR' , 'Sprint Wireless 110 - Export A/R Invoices' , 'Special Charge Code' , 'X12 Code' , *[local-name()='ChargeCode']/*[local-name()='Code'])"/>
                <xsl:if test="$ChargeCode != ''">
                  <L108>
                    <xsl:value-of select="$ChargeCode"/>
                  </L108>

                  <xsl:if test="*[local-name()='ChargeCode']/*[local-name()='Description'] != ''">
                    <L112>
                      <xsl:value-of select="*[local-name()='ChargeCode']/*[local-name()='Description']"/>
                    </L112>
                  </xsl:if>
                </xsl:if>
              </xsl:element>

            </xsl:element>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>


      <xsl:if test="$BilledWeight != 0">
        <xsl:element name="ns0:L3">
          <xsl:if test="$BilledWeight > 0">
            <L301>
              <xsl:value-of select="format-number($BilledWeight, '0.##')"/>
            </L301>
            <L302>B</L302>
          </xsl:if>

          <L305>
            <xsl:choose>
              <xsl:when test="//*[local-name()='TransactionInfo']/*[local-name()='OSExGSTVATAmount'] > 0">
                <xsl:value-of select="format-number(//*[local-name()='TransactionInfo']/*[local-name()='OSExGSTVATAmount'] * 100, '0')"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="'0'"/>
              </xsl:otherwise>
            </xsl:choose>
          </L305>
        </xsl:element>
      </xsl:if>


    </ns0:X12_00401_110>
  </xsl:template>


  <xsl:template name="GenerateOrgAddress">
    <xsl:param name="Org"/>
    <xsl:param name="EntityIdCode"/>

    <xsl:variable name="Suffix">
      <xsl:if test="$EntityIdCode != 'BT'">
        <xsl:value-of select="'_2'"/>
      </xsl:if>
    </xsl:variable>

    <xsl:element name="ns0:N1{$Suffix}">
      <N101>
        <xsl:value-of select="$EntityIdCode"/>
      </N101>
      <xsl:variable name="orgName" select="$Org/*[local-name()='CompanyName']"/>
      <xsl:if test="$orgName">
        <N102>
          <xsl:value-of select="translate($orgName, '*', '')"/>
        </N102>
      </xsl:if>

      <xsl:if test="$EntityIdCode = 'BT'">
        <xsl:variable name="OrgCode">
          <xsl:variable name="EDRCode" select="$Org/*[local-name()='RegistrationNumberCollection']/*[local-name()='RegistrationNumber']
                        [*[local-name()='CountryOfIssue']/*[local-name()='Code'] = 'US' and *[local-name()='Type']/*[local-name()='Code'] = 'EDR']
                        /*[local-name()='Value']"/>

          <xsl:choose>
            <xsl:when test="$EDRCode != ''">
              <xsl:value-of select="$EDRCode"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="//*[local-name()='TransactionInfo']/*[local-name()='ExternalDebtorCode']"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <xsl:if test="$OrgCode!= ''">
          <N103>25</N103>
          <N104>
            <xsl:value-of select="$OrgCode"/>
          </N104>
        </xsl:if>
      </xsl:if>
    </xsl:element>

    <xsl:variable name="Address1" select="$Org/*[local-name() = 'Address1']"/>
    <xsl:if test="$Address1 != ''">
      <xsl:element name="ns0:N3{$Suffix}">
        <N301>
          <xsl:value-of select="translate($Address1, '*', '')"/>
        </N301>
        <xsl:variable name="Address2" select="$Org/*[local-name() = 'Address2']"/>
        <xsl:if test="$Address2 != ''">
          <N302>
            <xsl:value-of select="translate($Address2, '*', '')"/>
          </N302>
        </xsl:if>
      </xsl:element>
    </xsl:if>

    <xsl:variable name="City" select="$Org/*[local-name()='City']"/>
    <xsl:variable name="State" select="$Org/*[local-name()='State']"/>
    <xsl:variable name="Postcode" select="$Org/*[local-name()='Postcode']"/>
    <xsl:variable name="CountryCode" select="$Org/*[local-name()='Country']/*[local-name()='Code']"/>

    <xsl:if test="$City != '' or $State != '' or $Postcode != '' or $CountryCode != ''">
      <xsl:element name="ns0:N4{$Suffix}">
        <xsl:if test="$City != ''">
          <N401>
            <xsl:value-of select="$City"/>
          </N401>
        </xsl:if>
        <xsl:if test="$State != ''">
          <N402>
            <xsl:value-of select="substring($State, 1, 2)"/>
          </N402>
        </xsl:if>
        <xsl:if test="$Postcode != ''">
          <N403>
            <xsl:value-of select="$Postcode"/>
          </N403>
        </xsl:if>
        <xsl:if test="$CountryCode !=''">
          <N404>
            <xsl:value-of select="$CountryCode"/>
          </N404>
        </xsl:if>
      </xsl:element>
    </xsl:if>
  </xsl:template>

</xsl:stylesheet>
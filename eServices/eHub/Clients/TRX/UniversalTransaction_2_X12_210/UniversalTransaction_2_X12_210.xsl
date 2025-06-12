<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl ScriptNS0 ScriptNS1 ScriptNS2 userCSharp" version="1.0"
                xmlns:ns0="http://schemas.microsoft.com/BizTalk/EDI/X12/2006"
                xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
								xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1"
								xmlns:ScriptNS2="http://schemas.microsoft.com/BizTalk/2003/ScriptNS2"
								xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="yes" indent="no" version="1.0" method="xml" />

  <xsl:template match="/">
    <xsl:variable name="SCAC" select="ScriptNS1:GetRecipientCodeUnkeyed('TRXELPELP' , 'TRXELPELP_BIR' , 'Omnicell 210 - Export A/R Invoices' , 'Defaults' , 'SCAC Code')"/>

    <ns0:X12_00401_210>

      <xsl:variable name="Shipment" select="(//*[local-name()='Shipment'][contains(./*[local-name()='DataContext']/*[local-name()='DataSourceCollection'], 'ForwardingShipment') and 
                                                           not(contains(./*[local-name()='DataContext']/*[local-name()='DataSourceCollection'], 'ForwardingConsol'))] | 
                                  //*[local-name()='SubShipment'][contains(./*[local-name()='DataContext']/*[local-name()='DataSourceCollection'], 'ForwardingShipment')])[1]" />
      <xsl:variable name="HAWB" select ="$Shipment/*[local-name()='WayBillNumber'][../*[local-name()='WayBillType'][*[local-name()='Code'] = 'HWB']]"/>
      <xsl:variable name="WeightUQ" select="ScriptNS1:GetRecipientCode('TRXELPELP' , 'TRXELPELP_BIR' , 'Omnicell 210 - Export A/R Invoices' , 'Unit of Measurement' , 'X12 Code' , $Shipment/*[local-name()='TotalWeightUnit']/*[local-name()='Code'])"/>

      <ST>
        <ST01>210</ST01>
        <ST02>0000</ST02>
      </ST>
      <ns0:B3>
        <B302>
          <xsl:value-of select="//*[local-name()='TransactionInfo']/*[local-name()='Number']"/>
        </B302>

        <xsl:if test="$HAWB != ''">
          <B303>
            <xsl:value-of select="$HAWB"/>
          </B303>
        </xsl:if>

        <B304>
          <xsl:value-of select="ScriptNS1:GetRecipientCode('TRXELPELP' , 'TRXELPELP_BIR' , 'Omnicell 210 - Export A/R Invoices', 'Payment Method', 'X12 Code', $Shipment/*[local-name()='ShipmentIncoTerm']/*[local-name()='Code'])"/>
        </B304>

        <xsl:if test="$WeightUQ != ''">
          <B305>
            <xsl:value-of select="$WeightUQ"/>
          </B305>
        </xsl:if>

        <B306>
          <xsl:value-of select="ScriptNS0:ConvertXmlDateString(//*[local-name()='TransactionInfo']/*[local-name()='TransactionDate'], 'yyyyMMdd')"/>
        </B306>

        <B307>
          <xsl:variable name="OSTotal" select="//*[local-name()='TransactionInfo']/*[local-name()='OSTotal']"/>
          <xsl:choose>
            <xsl:when test="$OSTotal > 0">
              <xsl:value-of select="format-number($OSTotal * 100, '0')"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'0'"/>
            </xsl:otherwise>
          </xsl:choose>
        </B307>

        <xsl:variable name="ActualDelv" select="$Shipment/*[local-name()='LocalProcessing']/*[local-name()='DeliveryCartageCompleted']"/>
        <xsl:variable name="EstDelv" select="$Shipment/*[local-name()='LocalProcessing']/*[local-name()='EstimatedDelivery']"/>
        <xsl:choose>
          <xsl:when test="$ActualDelv != ''">
            <B309>
              <xsl:value-of select="ScriptNS0:ConvertXmlDateString($ActualDelv, 'yyyyMMdd')"/>
            </B309>
            <B310>035</B310>
          </xsl:when>
          <xsl:when test="$EstDelv != ''">
            <B309>
              <xsl:value-of select="ScriptNS0:ConvertXmlDateString($EstDelv, 'yyyyMMdd')"/>
            </B309>
            <B310>017</B310>
          </xsl:when>
        </xsl:choose>

        <B311>
          <xsl:value-of select="$SCAC"/>
        </B311>
        <B312>
          <xsl:value-of select="ScriptNS0:CurrentDateTime('yyyyMMdd')"/>
        </B312>
      </ns0:B3>

      <xsl:variable name="OSCurrencyCode" select="//*[local-name()='TransactionInfo']/*[local-name()='OSCurrency']/*[local-name()='Code']"/>
      <xsl:if test="$OSCurrencyCode != ''">
        <ns0:C3>
          <C301>
            <xsl:value-of select="$OSCurrencyCode"/>
          </C301>
        </ns0:C3>
      </xsl:if>

      <xsl:if test="$HAWB != ''">
        <ns0:N9>
          <N901>BM</N901>
          <N902>
            <xsl:value-of select="$HAWB"/>
          </N902>
        </ns0:N9>
      </xsl:if>

      <xsl:for-each select="$Shipment/*[local-name()='LocalProcessing']/*[local-name()='OrderNumberCollection']/*[local-name()='OrderNumber']">
        <ns0:N9>
          <N901>PO</N901>
          <N902>
            <xsl:value-of select="*[local-name()='OrderReference']"/>
          </N902>
        </ns0:N9>
      </xsl:for-each>

      <xsl:variable name ="ShipDate" select="ScriptNS0:ConvertXmlDateString($Shipment/*[local-name()='DateCollection']
   /*[local-name()='Date' and *[local-name()='Type']/text()='Departure']/*[local-name()='Value'], 'yyyyMMdd')"/>
      <xsl:if test="$ShipDate">
        <ns0:G62>
          <G6201>11</G6201>
          <G6202>
            <xsl:value-of select="$ShipDate"/>
          </G6202>
        </ns0:G62>
      </xsl:if>

      <xsl:variable name="PickupCartageCompleted" select="ScriptNS0:ConvertXmlDateString($Shipment/*[local-name()='LocalProcessing']/*[local-name()='PickupCartageCompleted'], 'yyyyMMdd')"/>
      <xsl:if test="$PickupCartageCompleted != ''">
        <ns0:G62>
          <G6201>86</G6201>
          <G6202>
            <xsl:value-of select="$PickupCartageCompleted"/>
          </G6202>
        </ns0:G62>
      </xsl:if>

      <xsl:variable name="DeliveryCartageCompleted" select="ScriptNS0:ConvertXmlDateString($Shipment/*[local-name()='LocalProcessing']/*[local-name()='DeliveryCartageCompleted'], 'yyyyMMdd')"/>
      <xsl:if test="$DeliveryCartageCompleted != ''">
        <ns0:G62>
          <G6201>35</G6201>
          <G6202>
            <xsl:value-of select="$DeliveryCartageCompleted"/>
          </G6202>
        </ns0:G62>
      </xsl:if>

      <xsl:variable name="ServiceLevel" select="ScriptNS1:GetRecipientCode('TRXELPELP' , 'TRXELPELP_BIR' , 'Omnicell 210 - Export A/R Invoices' , 'Service Level' , 'X12 Code', $Shipment/*[local-name()='ServiceLevel']/*[local-name()='Code'])"/>
      <xsl:if test="$ServiceLevel != ''">
        <ns0:R3>
          <R301>
            <xsl:value-of select="$SCAC"/>
          </R301>
          <R302>B</R302>
          <R310>
            <xsl:value-of select="$ServiceLevel"/>
          </R310>
        </ns0:R3>
      </xsl:if>

      <xsl:variable name="BillToParty" select="//*[local-name()='TransactionInfo']/*[local-name()='OrganizationAddress']"/>
      <xsl:variable name="Consignee" select="$Shipment/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress' and 
											*[local-name()='AddressType'] = 'ConsigneeDocumentaryAddress']"/>
      <xsl:variable name="Consignor" select="$Shipment/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress' and 
											*[local-name()='AddressType'] = 'ConsignorDocumentaryAddress']"/>

      <xsl:if test="$BillToParty != ''">
        <xsl:call-template name="GenerateOrgAddress">
          <xsl:with-param name="Org" select="$BillToParty"/>
          <xsl:with-param name="EntityIdCode" select="'BT'"/>
        </xsl:call-template>
      </xsl:if>

      <xsl:if test="$Consignee != ''">
        <xsl:call-template name="GenerateOrgAddress">
          <xsl:with-param name="Org" select="$Consignee"/>
          <xsl:with-param name="EntityIdCode" select="'CN'"/>
        </xsl:call-template>
      </xsl:if>

      <xsl:if test="$Consignor != ''">
        <xsl:call-template name="GenerateOrgAddress">
          <xsl:with-param name="Org" select="$Consignor"/>
          <xsl:with-param name="EntityIdCode" select="'SH'"/>
        </xsl:call-template>
      </xsl:if>

      <xsl:variable name="TotalWeight" select="$Shipment/*[local-name()='TotalWeight']"/>
      <xsl:variable name="TotalPacks" select="$Shipment/*[local-name()='OuterPacks']"/>
      <xsl:variable name="PackType" select="ScriptNS1:GetRecipientCode('TRXELPELP' , 'TRXELPELP_BIR' , 'Omnicell 210 - Export A/R Invoices' , 'Pack Type' , 'X12 Code' , $Shipment/*[local-name()='OuterPacksPackageType']/*[local-name()='Code'])"/>
      <xsl:variable name="TotalVolume" select="$Shipment/*[local-name()='TotalVolume']"/>
      <xsl:variable name="VolumeUQ" select="ScriptNS1:GetRecipientCode('TRXELPELP' , 'TRXELPELP_BIR' , 'Omnicell 210 - Export A/R Invoices' , 'Unit of Measurement' , 'X12 Code' , $Shipment/*[local-name()='TotalVolumeUnit']/*[local-name()='Code'])"/>

      <xsl:for-each select="//*[local-name()='TransactionInfo']/*[local-name()='PostingJournalCollection']/*[local-name()='PostingJournal']">
        <xsl:sort select="*[local-name()='Sequence']"/>
        <xsl:variable name="counter" select="*[local-name()='Sequence']" />

        <ns0:LXLoop1>
          <ns0:LX>
            <LX01>
              <xsl:value-of select="$counter"/>
            </LX01>
          </ns0:LX>

          <ns0:L5>
            <L501>
              <xsl:value-of select="$counter"/>
            </L501>
            <L502>
              <xsl:value-of select="./*[local-name()='ChargeCode']/*[local-name()='Description']"/>
            </L502>
          </ns0:L5>

          <xsl:if test="$counter = 1">
            <ns0:L0>
              <L001>
                <xsl:value-of select="$counter"/>
              </L001>

              <xsl:if test="$TotalWeight > 0">
                <L004>
                  <xsl:value-of select="format-number($TotalWeight, '0.##')"/>
                </L004>
                <L005>
                  <xsl:value-of select="'G'"/>
                </L005>
              </xsl:if>

              <xsl:if test="$TotalVolume > 0">
                <L006>
                  <xsl:value-of select="format-number($TotalVolume, '0.##')"/>
                </L006>
                <xsl:if test="$VolumeUQ != ''">
                  <L007>
                    <xsl:value-of  select="$VolumeUQ" />
                  </L007>
                </xsl:if>
              </xsl:if>

              <xsl:if test="$TotalPacks > 0">
                <L008>
                  <xsl:value-of select="format-number($TotalPacks, '0')"/>
                </L008>
                <xsl:if test="$PackType != ''">
                  <L009>
                    <xsl:value-of select="$PackType"/>
                  </L009>
                </xsl:if>
              </xsl:if>

              <xsl:if test="$TotalWeight > 0 and $WeightUQ != ''">
                <L011>
                  <xsl:value-of select="$WeightUQ"/>
                </L011>
              </xsl:if>
            </ns0:L0>
          </xsl:if>

          <ns0:L1>
            <L101>
              <xsl:value-of select="$counter"/>
            </L101>

            <xsl:variable name="Charge" select="./*[local-name()='OSAmount']"/>
            <xsl:if test="$Charge > 0">
              <L102>
                <xsl:value-of select="format-number($Charge, '0.##')" />
              </L102>
              <L103>
                <xsl:value-of select="ScriptNS1:GetRecipientCodeUnkeyed('TRXELPELP' , 'TRXELPELP_BIR' , 'Omnicell 210 - Export A/R Invoices' , 'Defaults' , 'Rate Basis')"/>
              </L103>
              <L104>
                <xsl:value-of select="format-number($Charge * 100, '0')"/>
              </L104>  
            </xsl:if>

            <xsl:variable name="ChargeCode" select="ScriptNS1:GetRecipientCode('TRXELPELP' , 'TRXELPELP_BIR' , 'Omnicell 210 - Export A/R Invoices' , 'Special Charge Code' , 'X12 Code' , ./*[local-name()='ChargeCode']/*[local-name()='Code'])"/>
            <xsl:if test="$ChargeCode != ''">
              <L108>
                <xsl:value-of select="$ChargeCode"/>
              </L108>

              <xsl:variable name="ChargeCodeDesc" select="./*[local-name()='ChargeCode']/*[local-name()='Description']"/>
              <xsl:if test="$ChargeCodeDesc != ''">
                <L112>
                  <xsl:value-of select="$ChargeCodeDesc"/>
                </L112>
              </xsl:if>
            </xsl:if>
          </ns0:L1>
        </ns0:LXLoop1>
      </xsl:for-each>

      <ns0:L3>
        <xsl:if test="$TotalWeight > 0">
          <L301>
            <xsl:value-of select="format-number($TotalWeight, '0.##')"/>
          </L301>
          <L302>G</L302>
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

        <xsl:if test="$TotalVolume > 0">
          <L309>
            <xsl:value-of select="format-number($TotalVolume, '0.##')"/>
          </L309>
          <xsl:if test="$VolumeUQ != ''">
            <L310>
              <xsl:value-of  select="$VolumeUQ" />
            </L310>
          </xsl:if>
        </xsl:if>

        <xsl:if test="$TotalPacks > 0">
          <L311>
            <xsl:value-of select="$TotalPacks"/>
          </L311>
        </xsl:if>

        <xsl:if test="$TotalWeight > 0 and $WeightUQ != ''">
          <L312>
            <xsl:value-of select="$WeightUQ"/>
          </L312>
        </xsl:if>
      </ns0:L3>
    </ns0:X12_00401_210>
  </xsl:template>


  <xsl:template name="GenerateOrgAddress">
    <xsl:param name="Org"/>
    <xsl:param name="EntityIdCode"/>

    <ns0:N1Loop1>
      <ns0:N1>
        <N101>
          <xsl:value-of select="$EntityIdCode"/>
        </N101>
        <xsl:variable name="orgName" select="$Org/*[local-name()='CompanyName']"/>
        <xsl:if test="$orgName">
          <N102>
            <xsl:value-of select="translate($orgName, '*', '')"/>
          </N102>
        </xsl:if>

        <xsl:variable name="OrgCode">
          <xsl:choose>
            <xsl:when test="$EntityIdCode = 'BT'">
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
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$Org/*[local-name()='OrganizationCode']" />
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <xsl:if test="$OrgCode!= ''">
          <N103>92</N103>
          <N104>
            <xsl:value-of select="$OrgCode"/>
          </N104>
        </xsl:if>
      </ns0:N1>

      <xsl:variable name="Address1" select="$Org/*[local-name() = 'Address1']"/>
      <xsl:if test="$Address1 != ''">
        <ns0:N3>
          <N301>
            <xsl:value-of select="translate($Address1, '*', '')"/>
          </N301>
          <xsl:variable name="Address2" select="$Org/*[local-name() = 'Address2']"/>
          <xsl:if test="$Address2 != ''">
            <N302>
              <xsl:value-of select="translate($Address2, '*', '')"/>
            </N302>
          </xsl:if>
        </ns0:N3>
      </xsl:if>

      <xsl:variable name="City" select="$Org/*[local-name()='City']"/>
      <xsl:variable name="State" select="$Org/*[local-name()='State']"/>
      <xsl:variable name="Postcode" select="$Org/*[local-name()='Postcode']"/>
      <xsl:variable name="CountryCode" select="ScriptNS1:GetRecipientCode('TRXELPELP' , 'TRXELPELP_BIR' , 'Omnicell 210 - Export A/R Invoices' , 'Country Code' , 'ISO 3166 Code' , $Org/*[local-name()='Country']/*[local-name()='Code'])"/>

      <xsl:if test="$City != '' or $State != '' or $Postcode != '' or $CountryCode != ''">
        <ns0:N4>
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
        </ns0:N4>
      </xsl:if>
    </ns0:N1Loop1>
  </xsl:template>

</xsl:stylesheet>
<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2011/11" xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"  xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp" xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
  <xsl:output method="xml" indent="yes"/>

  <xsl:template match="@* | node()">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()"/>
    </xsl:copy>
  </xsl:template>



  <xsl:template name="AutoSense">



    <xsl:variable name="DestinationParty" select="ScriptNS0:GetContextProperty('DestinationParty' , 'http://schemas.microsoft.com/BizTalk/2003/system-properties')" />
    <xsl:variable name="SetDestinationParty" select="ScriptNS0:SetContextProperty('DestinationParty' , 'http://schemas.microsoft.com/BizTalk/2003/system-properties', 
                  ScriptNS1:GetRecipientCode('GEOGSCGUS', 'GEOGSCGUS_ICR', 'IBM CUSRES File - Generate from Import Customs Dec', 'Recipient', 'New Recipient', 
                  //*[local-name()='OrganizationAddress'][./*[local-name()='AddressType'] = 'ImporterDocumentaryAddress']/*[local-name()='OrganizationCode'], $DestinationParty))" />
    
    
    
  </xsl:template>
  
  
  
  <!--Scripts begins from here-->

  <xsl:template name="WriteCSTLoop1FromInvoiceLineRaw">
    <xsl:param name="exchangeRateFromHeader" select="'1'"/>
    <xsl:for-each select="*[local-name()='CommercialInvoiceLineCollection']/*[local-name()='CommercialInvoiceLine']">
      <xsl:variable name="shipmentLevel" select="../../../../.."/>
      <xsl:variable name="invoiceLevel" select="../.."/>
      <xsl:variable name="invoiceLineNumber" select="*[local-name()='LineNo']/text()"/>
      <xsl:variable name="lineInvoiceQuantityUnit" select="*[local-name()='InvoiceQuantityUnit']/*[local-name()='Code']/text()"/>
      <xsl:variable name="lineHarmonisedCode" select="*[local-name()='HarmonisedCode']/text()"/>
      <xsl:element name="ns0:CSTLoop1">
        <xsl:variable name="lineCVDuty">
          <xsl:call-template name="GetAddinfoValueRaw">
            <xsl:with-param name="key" select="'CVDuty'"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:variable name="lineADDuty">
          <xsl:call-template name="GetAddinfoValueRaw">
            <xsl:with-param name="key" select="'ADDuty'"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:variable name="goodsItemNumber">
          <xsl:call-template name="GetCustomizedFieldRaw">
            <xsl:with-param name="key" select="'Case Item Count'"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:call-template name="WriteCSTRaw">
          <xsl:with-param name="goodsItemNumber" select="number($goodsItemNumber)"/>
        </xsl:call-template>
        <xsl:variable name="caseNumber">
          <xsl:call-template name="GetCustomizedFieldRaw">
            <xsl:with-param name="key" select="'Case Number'"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:call-template name="WriteFTX_4Raw">
          <xsl:with-param name="textSubjectQualifier" select="'PKG'"/>
          <xsl:with-param name="freeText1" select="$caseNumber"/>
        </xsl:call-template>
        <xsl:element name="ns0:TAXLoop2">
          <xsl:call-template name="WriteTAX_2Raw">
            <xsl:with-param name="dutyTaxFeeFunctionQualifier" select="'5'"/>
          </xsl:call-template>
          <xsl:variable name="grossWeightPerUnit">
            <xsl:call-template name="GetCustomizedFieldRaw">
              <xsl:with-param name="key" select="'Case Gross Weight'"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:call-template name="WriteMEA_2Raw">
            <xsl:with-param name="measurementPurposeQualifier" select="'WT'"/>
            <xsl:with-param name="propertyMeasuredCoded" select="'G'"/>
            <xsl:with-param name="measurementAttribute" select="$grossWeightPerUnit"/>
          </xsl:call-template>
          <xsl:variable name="netWeightPerUnit">
            <xsl:call-template name="GetCustomizedFieldRaw">
              <xsl:with-param name="key" select="'Case Net Weight'"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:call-template name="WriteMEA_2Raw">
            <xsl:with-param name="measurementPurposeQualifier" select="'WT'"/>
            <xsl:with-param name="propertyMeasuredCoded" select="'N'"/>
            <xsl:with-param name="measurementAttribute" select="$netWeightPerUnit"/>
          </xsl:call-template>
        </xsl:element>
        <xsl:element name="ns0:TAXLoop2">
          <xsl:call-template name="WriteTAX_2Raw">
            <xsl:with-param name="dutyTaxFeeFunctionQualifier" select="'2'"/>
          </xsl:call-template>
          <xsl:call-template name="WriteMOA_3Raw">
            <xsl:with-param name="monetaryAmountTypeQualifier" select="'146'"/>
            <xsl:with-param name="monetaryAmount" select="number(*[local-name()='LinePrice']) div number(*[local-name()='InvoiceQuantity'])"/>
          </xsl:call-template>
          <xsl:call-template name="WriteMOA_3Raw">
            <xsl:with-param name="monetaryAmountTypeQualifier" select="'38'"/>
            <xsl:with-param name="monetaryAmount" select="number(*[local-name()='LinePrice'])"/>
          </xsl:call-template>
          <xsl:call-template name="WriteMOA_3Raw">
            <xsl:with-param name="monetaryAmountTypeQualifier" select="'341'"/>
            <xsl:with-param name="monetaryAmount" select="number(*[local-name()='LinePrice'])"/>
          </xsl:call-template>
          <xsl:variable name="chargeADDLocal">
            <xsl:call-template name="GetCommercialChargeAmountRaw">
              <xsl:with-param name="chargeTypeCode" select="'ADD'"/>
              <xsl:with-param name="convertToLocalCurrency" select="'Y'"/>
              <xsl:with-param name="fallbackExchangeRate" select="$exchangeRateFromHeader"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:call-template name="WriteMOA_3Raw">
            <xsl:with-param name="monetaryAmountTypeQualifier" select="'205'"/>
            <xsl:with-param name="monetaryAmount" select="$chargeADDLocal"/>
          </xsl:call-template>
          <xsl:variable name="chargeADD">
            <xsl:call-template name="GetCommercialChargeAmountRaw">
              <xsl:with-param name="chargeTypeCode" select="'ADD'"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:call-template name="WriteMOA_3Raw">
            <xsl:with-param name="monetaryAmountTypeQualifier" select="'206'"/>
            <xsl:with-param name="monetaryAmount" select="$chargeADD"/>
          </xsl:call-template>
          <xsl:call-template name="WriteMOA_3Raw">
            <xsl:with-param name="monetaryAmountTypeQualifier" select="'290'"/>
            <xsl:with-param name="monetaryAmount" select="$lineADDuty"/>
          </xsl:call-template>
          <xsl:call-template name="WriteMOA_3Raw">
            <xsl:with-param name="monetaryAmountTypeQualifier" select="'298'"/>
            <xsl:with-param name="monetaryAmount" select="$lineCVDuty"/>
          </xsl:call-template>

          <xsl:call-template name="WriteMOA_3Raw">
            <xsl:with-param name="monetaryAmountTypeQualifier" select="'269'"/>
            <xsl:with-param name="monetaryAmount" select="0"/>
          </xsl:call-template>

          <xsl:variable name="EntryLineNumber" select="./*[local-name()='EntryLineNumber']" />
          <xsl:variable name="RelatedEntryLines" select="$shipmentLevel/*[local-name()='EntryHeaderCollection']
                        /*[local-name()='EntryHeader'][./*[local-name()='Type']/*[local-name()='Code'] = 'ENS']
                        /*[local-name()='EntryLineCollection']/*[local-name()='EntryLine'][./*[local-name()='LineNumber'] = $EntryLineNumber]" />
          <xsl:call-template name="WriteMOA_3Raw">
            <xsl:with-param name="monetaryAmountTypeQualifier" select="'9'"/>
            <xsl:with-param name="monetaryAmount" select="$RelatedEntryLines[not(starts-with(./*[local-name()='HarmonisedCode'], '9903'))]/*[local-name()='DutyRatePercent']"/>
          </xsl:call-template>

          <xsl:call-template name="WriteMOA_3Raw">
            <xsl:with-param name="monetaryAmountTypeQualifier" select="'54'"/>
            <xsl:with-param name="monetaryAmount" select="$RelatedEntryLines[starts-with(./*[local-name()='HarmonisedCode'], '9903')]/*[local-name()='DutyRatePercent']"/>
          </xsl:call-template>

          <xsl:variable name="Duty" select="sum(./*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key'] = 'Duty']/*[local-name()='Value'])" />
          <xsl:call-template name="WriteMOA_3Raw">
            <xsl:with-param name="monetaryAmountTypeQualifier" select="'187'"/>
            <xsl:with-param name="monetaryAmount" select="$Duty"/>
          </xsl:call-template>

          <xsl:variable name="SupDuty" select="sum(./*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key'] = 'SupDuty']/*[local-name()='Value'])" />
          <xsl:call-template name="WriteMOA_3Raw">
            <xsl:with-param name="monetaryAmountTypeQualifier" select="'122'"/>
            <xsl:with-param name="monetaryAmount" select="$SupDuty"/>
          </xsl:call-template>

          <xsl:call-template name="WriteMOA_3Raw">
            <xsl:with-param name="monetaryAmountTypeQualifier" select="'161'"/>
            <xsl:with-param name="monetaryAmount" select="$Duty + $SupDuty"/>
          </xsl:call-template>
          
          <xsl:call-template name="WriteGIS_4Raw">
            <xsl:with-param name="processingIndicatorCoded" select="'71'"/>
            <xsl:with-param name="processTypeIdentification" select="'7'"/>
          </xsl:call-template>
          <xsl:variable name="chargeIndicator">
            <xsl:call-template name="GetCustomizedFieldRaw">
              <xsl:with-param name="key" select="'Charge Indicator'"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="chargeIndicatorFinal">
            <xsl:choose>
              <xsl:when test="normalize-space($chargeIndicator)='true'">
                <xsl:value-of select="'C'"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="'N'"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <xsl:call-template name="WriteGIS_4Raw">
            <xsl:with-param name="processingIndicatorCoded" select="'72'"/>
            <xsl:with-param name="processTypeIdentification" select="$chargeIndicatorFinal"/>
          </xsl:call-template>
          <xsl:variable name="lineCVD_NA">
            <xsl:call-template name="GetAddinfoValueRaw">
              <xsl:with-param name="key" select="'CVD_NA'"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:if test="$lineCVD_NA='Y'">
            <xsl:call-template name="WriteGIS_4Raw">
              <xsl:with-param name="processingIndicatorCoded" select="'74'"/>
              <xsl:with-param name="processTypeIdentification" select="$lineCVD_NA"/>
            </xsl:call-template>
          </xsl:if>
          <xsl:variable name="lineADD_NA">
            <xsl:call-template name="GetAddinfoValueRaw">
              <xsl:with-param name="key" select="'ADD_NA'"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:if test="$lineADD_NA='Y'">
            <xsl:call-template name="WriteGIS_4Raw">
              <xsl:with-param name="processingIndicatorCoded" select="'75'"/>
              <xsl:with-param name="processTypeIdentification" select="$lineADD_NA"/>
            </xsl:call-template>
          </xsl:if>
          <xsl:variable name="taxDeferIndicator" select="/*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='AddInfoCollection']
                                                        /*[local-name()='AddInfo'][*[local-name()='Key']/text()='TaxDeferIndicator']
                                                        /*[local-name()='Value']/text()"/>
          <xsl:call-template name="WriteGIS_4Raw">
            <xsl:with-param name="processingIndicatorCoded" select="'20'"/>
            <xsl:with-param name="processTypeIdentification" select="$taxDeferIndicator"/>
          </xsl:call-template>
          <xsl:call-template name="WriteMEA_2Raw">
            <xsl:with-param name="measurementPurposeQualifier" select="'AAA'"/>
            <xsl:with-param name="propertyMeasuredCoded" select="'ACB'"/>
            <xsl:with-param name="measurementAttribute" select="*[local-name()='Description']/text()"/>
            <xsl:with-param name="measureUnitQualifier" select="$lineInvoiceQuantityUnit"/>
            <xsl:with-param name="measurementValue" select="*[local-name()='InvoiceQuantity']/text()"/>
          </xsl:call-template>
          <xsl:call-template name="WriteMEA_2Raw">
            <xsl:with-param name="measurementPurposeQualifier" select="'AAR'"/>
            <xsl:with-param name="measureUnitQualifier" select="*[local-name()='CustomsQuantityUnit']/*[local-name()='Code']/text()"/>
            <xsl:with-param name="measurementValue" select="*[local-name()='CustomsQuantity']/text()"/>
          </xsl:call-template>
          <xsl:variable name="secondUQ">
            <xsl:call-template name="GetAddinfoValueRaw">
              <xsl:with-param name="key" select="'SecondUQ'"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="secondQty">
            <xsl:call-template name="GetAddinfoValueRaw">
              <xsl:with-param name="key" select="'SecondQty'"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:call-template name="WriteMEA_2Raw">
            <xsl:with-param name="measurementPurposeQualifier" select="'AAS'"/>
            <xsl:with-param name="measureUnitQualifier" select="$secondUQ"/>
            <xsl:with-param name="measurementValue" select="$secondQty"/>
          </xsl:call-template>
          <xsl:variable name="thirdUQ">
            <xsl:call-template name="GetAddinfoValueRaw">
              <xsl:with-param name="key" select="'ThirdUQ'"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="thirdQty">
            <xsl:call-template name="GetAddinfoValueRaw">
              <xsl:with-param name="key" select="'ThirdQty'"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:call-template name="WriteMEA_2Raw">
            <xsl:with-param name="measurementPurposeQualifier" select="'AAT'"/>
            <xsl:with-param name="measureUnitQualifier" select="$thirdUQ"/>
            <xsl:with-param name="measurementValue" select="$thirdQty"/>
          </xsl:call-template>
          <xsl:variable name="assistNumber">
            <xsl:call-template name="GetCustomizedFieldRaw">
              <xsl:with-param name="key" select="'Assist Number'"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:call-template name="WriteRFF_4Raw">
            <xsl:with-param name="referenceQualifier" select="'FS'"/>
            <xsl:with-param name="referenceNumber" select="$invoiceLineNumber"/>
            <xsl:with-param name="referenceVersionNumber" select="$assistNumber"/>
          </xsl:call-template>
          <xsl:call-template name="WriteRFF_4Raw">
            <xsl:with-param name="referenceQualifier" select="'MF'"/>
            <xsl:with-param name="referenceNumber" select="*[local-name()='PartNo']/text()"/>
          </xsl:call-template>
          <xsl:call-template name="WriteRFF_4Raw">
            <xsl:with-param name="referenceQualifier" select="'ON'"/>
            <xsl:with-param name="referenceNumber" select="*[local-name()='OrderNumber']/text()"/>
          </xsl:call-template>
          <xsl:variable name="serialNumber">
            <xsl:call-template name="GetCustomizedFieldRaw">
              <xsl:with-param name="key" select="'Serial Number'"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:call-template name="WriteRFF_4Raw">
            <xsl:with-param name="referenceQualifier" select="'SE'"/>
            <xsl:with-param name="referenceNumber" select="$serialNumber"/>
          </xsl:call-template>
          <xsl:call-template name="WriteRFF_4Raw">
            <xsl:with-param name="referenceQualifier" select="'HS'"/>
            <xsl:with-param name="referenceNumber" select="$lineHarmonisedCode"/>
            <xsl:with-param name="lineNumber" select="$invoiceLineNumber"/>
          </xsl:call-template>
          
          <xsl:variable name="TSUSNumber" select="./*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key'] = 'SupTariff']
                        /*[local-name()='Value'][starts-with(., '9999') or starts-with(., '9803') or starts-with(., '9903')]" />
          <xsl:if test="$TSUSNumber != ''">
            <xsl:call-template name="WriteRFF_4Raw">
              <xsl:with-param name="referenceQualifier">
                <xsl:choose>

                  <xsl:when test="starts-with($TSUSNumber, '9903')">
                    <xsl:text>AAD</xsl:text>
                  </xsl:when>

                  <xsl:otherwise>
                    <xsl:text>ACD</xsl:text>
                  </xsl:otherwise>

                </xsl:choose>
              </xsl:with-param>
              <xsl:with-param name="referenceNumber" select="$TSUSNumber"/>
            </xsl:call-template>
          </xsl:if>

          <xsl:variable name="lineCountryOfOrigin">
            <xsl:call-template name="GetAddinfoValueRaw">
              <xsl:with-param name="key" select="'UC_NKCountryOfOrigin'"/>
              <xsl:with-param name="fallbackLevel" select="$invoiceLevel"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:call-template name="WriteRFF_4Raw">
            <xsl:with-param name="referenceQualifier" select="'LI'"/>
            <xsl:with-param name="referenceNumber" select="*[local-name()='OrderNumber']/text()"/>
            <xsl:with-param name="referenceVersionNumber" select="$lineCountryOfOrigin"/>
          </xsl:call-template>
          <xsl:variable name="sPI">
            <xsl:call-template name="GetAddinfoValueRaw">
              <xsl:with-param name="key" select="'SPI'"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="lineNAFTA">
            <xsl:choose>
              <xsl:when test="$sPI != '' and contains('CA|MX|S|S+', $sPI)">Y</xsl:when>
              <xsl:when test="$sPI='N/A'">N</xsl:when>
            </xsl:choose>
          </xsl:variable>
          <xsl:variable name="lineTextileCategoryNumber">
            <xsl:call-template name="GetAddinfoValueRaw">
              <xsl:with-param name="key" select="'TextileCategoryNumber'"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:call-template name="WriteRFF_4Raw">
            <xsl:with-param name="referenceQualifier" select="'ZZZ'"/>
            <xsl:with-param name="referenceNumber" select="$lineNAFTA"/>
            <xsl:with-param name="lineNumber" select="$lineTextileCategoryNumber"/>
          </xsl:call-template>
        </xsl:element>
        <xsl:element name="ns0:TAXLoop2">
          <xsl:call-template name="WriteTAX_2Raw">
            <xsl:with-param name="dutyTaxFeeFunctionQualifier" select="'3'"/>
          </xsl:call-template>
          <xsl:variable name="lineFCCQty">
            <xsl:call-template name="GetAddInfoGroupAddInfoValueRaw">
              <xsl:with-param name="addInfoGroupTypeCode" select="'USC'"/>
              <xsl:with-param name="key" select="'FCCQty'"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="lineFCCImpCondNo">
            <xsl:call-template name="GetAddInfoGroupAddInfoValueRaw">
              <xsl:with-param name="addInfoGroupTypeCode" select="'USC'"/>
              <xsl:with-param name="key" select="'FCCImpCondNo'"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:call-template name="WriteMEA_2Raw">
            <xsl:with-param name="measurementPurposeQualifier" select="'AAR'"/>
            <xsl:with-param name="propertyMeasuredCoded" select="'ACB'"/>
            <xsl:with-param name="measurementAttribute" select="$lineFCCQty"/>
            <xsl:with-param name="measureUnitQualifier" select="$lineInvoiceQuantityUnit"/>
            <xsl:with-param name="measurementValue" select="$lineFCCImpCondNo"/>
          </xsl:call-template>
          <xsl:variable name="lineFCCID">
            <xsl:call-template name="GetAddInfoGroupAddInfoValueRaw">
              <xsl:with-param name="addInfoGroupTypeCode" select="'USC'"/>
              <xsl:with-param name="key" select="'FCCID'"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:call-template name="WriteRFF_4Raw">
            <xsl:with-param name="referenceQualifier" select="'ACB'"/>
            <xsl:with-param name="referenceNumber" select="$lineFCCID"/>
          </xsl:call-template>
          <xsl:variable name="lineFCCLineNo">
            <xsl:call-template name="GetAddInfoGroupAddInfoValueRaw">
              <xsl:with-param name="addInfoGroupTypeCode" select="'USC'"/>
              <xsl:with-param name="key" select="'FCCLineNo'"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="lineFCCTradeName">
            <xsl:call-template name="GetAddInfoGroupAddInfoValueRaw">
              <xsl:with-param name="addInfoGroupTypeCode" select="'USC'"/>
              <xsl:with-param name="key" select="'FCCTradeName'"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:call-template name="WriteRFF_4Raw">
            <xsl:with-param name="referenceQualifier" select="'AMS'"/>
            <xsl:with-param name="referenceNumber" select="$lineFCCID"/>
            <xsl:with-param name="lineNumber" select="$lineFCCLineNo"/>
            <xsl:with-param name="referenceVersionNumber" select="$lineFCCTradeName"/>
          </xsl:call-template>
          <xsl:variable name="lineFCCModel">
            <xsl:call-template name="GetAddInfoGroupAddInfoValueRaw">
              <xsl:with-param name="addInfoGroupTypeCode" select="'USC'"/>
              <xsl:with-param name="key" select="'FCCModel'"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:call-template name="WriteRFF_4Raw">
            <xsl:with-param name="referenceQualifier" select="'ALX'"/>
            <xsl:with-param name="referenceNumber" select="$lineFCCModel"/>
          </xsl:call-template>
          <xsl:variable name="lineFCCCommercialDesc">
            <xsl:call-template name="GetAddInfoGroupAddInfoValueRaw">
              <xsl:with-param name="addInfoGroupTypeCode" select="'USC'"/>
              <xsl:with-param name="key" select="'FCCCommercialDesc'"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:call-template name="WriteRFF_4Raw">
            <xsl:with-param name="referenceQualifier" select="'AFD'"/>
            <xsl:with-param name="referenceNumber" select="$lineHarmonisedCode"/>
            <xsl:with-param name="referenceVersionNumber" select="$lineFCCCommercialDesc"/>
          </xsl:call-template>
        </xsl:element>
        <xsl:element name="ns0:TAXLoop2">
          <xsl:call-template name="WriteTAX_2Raw">
            <xsl:with-param name="dutyTaxFeeFunctionQualifier" select="'1'"/>
          </xsl:call-template>
          <xsl:call-template name="WriteMOA_3Raw">
            <xsl:with-param name="monetaryAmountTypeQualifier" select="'203'"/>
            <xsl:with-param name="monetaryAmount" select="number(*[local-name()='LinePrice']/text())"/>
          </xsl:call-template>
          <xsl:variable name="lineFDAMeasure1">
            <xsl:call-template name="GetAddInfoGroupAddInfoValueRaw">
              <xsl:with-param name="addInfoGroupTypeCode" select="'USA'"/>
              <xsl:with-param name="key" select="'FDAMeasure1'"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="lineFDAQty1">
            <xsl:call-template name="GetAddInfoGroupAddInfoValueRaw">
              <xsl:with-param name="addInfoGroupTypeCode" select="'USA'"/>
              <xsl:with-param name="key" select="'FDAQty1'"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:call-template name="WriteMEA_2Raw">
            <xsl:with-param name="measurementPurposeQualifier" select="'AAS'"/>
            <xsl:with-param name="propertyMeasuredCoded" select="'ACC'"/>
            <xsl:with-param name="measurementAttributeIdentification" select="$lineFDAMeasure1"/>
            <xsl:with-param name="measurementAttribute" select="$lineFDAQty1"/>
          </xsl:call-template>
          <xsl:variable name="lineFDASupplierOrShipperID">
            <xsl:call-template name="GetAddInfoGroupAddInfoValueRaw">
              <xsl:with-param name="addInfoGroupTypeCode" select="'USA'"/>
              <xsl:with-param name="key" select="'FDASupplierOrShipperID'"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="lineUC_NKFDAProduction">
            <xsl:call-template name="GetAddInfoGroupAddInfoValueRaw">
              <xsl:with-param name="addInfoGroupTypeCode" select="'USA'"/>
              <xsl:with-param name="key" select="'UC_NKFDAProduction'"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:call-template name="WriteRFF_4Raw">
            <xsl:with-param name="referenceQualifier" select="'ACB'"/>
            <xsl:with-param name="referenceNumber" select="$lineFDASupplierOrShipperID"/>
            <xsl:with-param name="referenceVersionNumber" select="$lineUC_NKFDAProduction"/>
          </xsl:call-template>
          <xsl:variable name="lineFDAProductCode">
            <xsl:call-template name="GetAddInfoGroupAddInfoValueRaw">
              <xsl:with-param name="addInfoGroupTypeCode" select="'USA'"/>
              <xsl:with-param name="key" select="'FDAProductCode'"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="lineFDALineNo">
            <xsl:call-template name="GetAddInfoGroupAddInfoValueRaw">
              <xsl:with-param name="addInfoGroupTypeCode" select="'USA'"/>
              <xsl:with-param name="key" select="'FDALineNo'"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:call-template name="WriteRFF_4Raw">
            <xsl:with-param name="referenceQualifier" select="'AEA'"/>
            <xsl:with-param name="referenceNumber" select="$lineFDAProductCode"/>
            <xsl:with-param name="lineNumber" select="$lineFDALineNo"/>
          </xsl:call-template>
          <xsl:variable name="lineTradeBrandName">
            <xsl:call-template name="GetAddInfoGroupAddInfoValueRaw">
              <xsl:with-param name="addInfoGroupTypeCode" select="'USA'"/>
              <xsl:with-param name="key" select="'TradeBrandName'"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:call-template name="WriteRFF_4Raw">
            <xsl:with-param name="referenceQualifier" select="'AEA'"/>
            <xsl:with-param name="referenceNumber" select="$lineTradeBrandName"/>
          </xsl:call-template>
        </xsl:element>
        <xsl:variable name="entryType">
          <xsl:call-template name="GetAddinfoValueRaw">
            <xsl:with-param name="key" select="'EntryType'"/>
            <xsl:with-param name="fallbackLevel" select="$shipmentLevel"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:if test="$entryType='03'">
          <xsl:element name="ns0:TAXLoop2">
            <xsl:call-template name="WriteTAX_2Raw">
              <xsl:with-param name="dutyTaxFeeFunctionQualifier" select="'4'"/>
            </xsl:call-template>
            <xsl:call-template name="WriteMOA_3Raw">
              <xsl:with-param name="monetaryAmountTypeQualifier" select="'55'"/>
              <xsl:with-param name="monetaryAmount" select="$lineCVDuty"/>
            </xsl:call-template>
            <xsl:variable name="lineCVDQty">
              <xsl:call-template name="GetAddinfoValueRaw">
                <xsl:with-param name="key" select="'CVDQty'"/>
              </xsl:call-template>
            </xsl:variable>
            <xsl:call-template name="WriteRFF_4Raw">
              <xsl:with-param name="referenceQualifier" select="'ACB'"/>
              <xsl:with-param name="referenceVersionNumber" select="$lineCVDQty"/>
            </xsl:call-template>
            <xsl:variable name="lineCVDCaseNo">
              <xsl:call-template name="GetAddinfoValueRaw">
                <xsl:with-param name="key" select="'CVDCaseNo'"/>
              </xsl:call-template>
            </xsl:variable>
            <xsl:call-template name="WriteRFF_4Raw">
              <xsl:with-param name="referenceQualifier" select="'ABC'"/>
              <xsl:with-param name="referenceNumber" select="$lineCVDCaseNo"/>
            </xsl:call-template>
          </xsl:element>
          <xsl:element name="ns0:TAXLoop2">
            <xsl:call-template name="WriteTAX_2Raw">
              <xsl:with-param name="dutyTaxFeeFunctionQualifier" select="'6'"/>
            </xsl:call-template>
            <xsl:call-template name="WriteMOA_3Raw">
              <xsl:with-param name="monetaryAmountTypeQualifier" select="'290'"/>
              <xsl:with-param name="monetaryAmount" select="$lineADDuty"/>
            </xsl:call-template>
            <xsl:variable name="lineADDQty">
              <xsl:call-template name="GetAddinfoValueRaw">
                <xsl:with-param name="key" select="'ADDQty'"/>
              </xsl:call-template>
            </xsl:variable>
            <xsl:call-template name="WriteRFF_4Raw">
              <xsl:with-param name="referenceQualifier" select="'ACB'"/>
              <xsl:with-param name="referenceVersionNumber" select="$lineADDQty"/>
            </xsl:call-template>
            <xsl:variable name="lineADDCaseNo">
              <xsl:call-template name="GetAddinfoValueRaw">
                <xsl:with-param name="key" select="'ADDCaseNo'"/>
              </xsl:call-template>
            </xsl:variable>
            <xsl:call-template name="WriteRFF_4Raw">
              <xsl:with-param name="referenceQualifier" select="'ABC'"/>
              <xsl:with-param name="referenceNumber" select="$lineADDCaseNo"/>
            </xsl:call-template>
          </xsl:element>
        </xsl:if>
      </xsl:element>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="WriteDOCFromInvoiceHeader">
    <xsl:for-each select="*[local-name()='Shipment']/*[local-name()='CommercialInfo']
                          /*[local-name()='CommercialInvoiceCollection']/*[local-name()='CommercialInvoice']">
      <xsl:variable name="shipmentLevel" select="../../.."/>
      <xsl:variable name="totalInvoiceAmountInShipment" select="sum($shipmentLevel/*[local-name()='CommercialInfo']
                          /*[local-name()='CommercialInvoiceCollection']/*[local-name()='CommercialInvoice']/*[local-name()='InvoiceAmount'])"/>
      <xsl:variable name="invoiceNumber" select="*[local-name()='InvoiceNumber']/text()"/>
      <xsl:variable name="invoiceAmount" select="*[local-name()='InvoiceAmount']/text()"/>
      <xsl:variable name="exchangeRate">
        <xsl:call-template name="GetExchangeRateRaw">
          <xsl:with-param name="elementName" select="'AgreedExchangeRate'"/>
          <xsl:with-param name="fallbackElementName" select="'LandedCostExchangeRate'"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:variable name="invoiceAmountInLocalCurrency">
        <xsl:call-template name="ConvertAmountToLocalCurrency">
          <xsl:with-param name="foreignAmount" select="$invoiceAmount"/>
          <xsl:with-param name="exchangeRate" select="$exchangeRate"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:variable name="vendorCode">
        <xsl:call-template name="GetCustomizedFieldRaw">
          <xsl:with-param name="key" select="'IBM Vendor Code'"/>
          <xsl:with-param name="parentLevel" select="$shipmentLevel"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:element name="ns0:DOCLoop1">
        <xsl:call-template name="WriteDOCRaw">
          <xsl:with-param name="documentMessageNameCoded" select="'935'"/>
          <xsl:with-param name="documentMessageName" select="$vendorCode"/>
        </xsl:call-template>
        <xsl:variable name="noOfPacks" select="number(*[local-name()='NoOfPacks']/text())"/>
        <xsl:call-template name="WritePACRaw">
          <xsl:with-param name="numberOfPackages" select="$noOfPacks"/>
        </xsl:call-template>
        <xsl:variable name="invoiceType">
          <xsl:call-template name="GetAddinfoValueRaw">
            <xsl:with-param name="key" select="'InvoiceType'"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:call-template name="WriteRFF_3Raw">
          <xsl:with-param name="referenceQualifier" select="'IV'"/>
          <xsl:with-param name="referenceNumber" select="$invoiceNumber"/>
          <xsl:with-param name="lineNumber" select="userCSharp:GetNextInvoiceSequenceNumber()"/>
          <xsl:with-param name="referenceVersionNumber" select="$invoiceType"/>
        </xsl:call-template>
        <xsl:variable name="manufacturerShipperID">
          <xsl:call-template name="GetRegistrationNumberOfOrganizationAddress">
            <xsl:with-param name="addressType" select="'Manufacturer'"/>
            <xsl:with-param name="registrationNumberTypeCode" select="'MID'"/>
            <xsl:with-param name="fallbackLevel" select="$shipmentLevel"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:call-template name="WriteRFF_3Raw">
          <xsl:with-param name="referenceQualifier" select="'SI'"/>
          <xsl:with-param name="referenceNumber" select="$manufacturerShipperID"/>
        </xsl:call-template>
        <xsl:call-template name="WriteRFF_3Raw">
          <xsl:with-param name="referenceQualifier" select="'SR'"/>
          <xsl:with-param name="referenceNumber" select="$manufacturerShipperID"/>
        </xsl:call-template>
        <xsl:call-template name="WritePCIRaw">
          <xsl:with-param name="markingInstructionsCoded" select="'12'"/>
          <xsl:with-param name="shippingMarks" select="$noOfPacks"/>
        </xsl:call-template>
        <xsl:variable name="transportMode" select="/*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='TransportMode']/*[local-name()='Code']/text()"/>
        <xsl:call-template name="WriteTDT_2Raw">
          <xsl:with-param name="transportStageQualifier" select="'20'"/>
          <xsl:with-param name="modeOfTransportCoded" select="ScriptNS1:GetRecipientCode('GEOGSCGUS','GEOGSCGUS_ICR','IBM CUSRES File - Generate from Import Customs Dec','Mode of Transport','IBM Code', string($transportMode))"/>
        </xsl:call-template>
        <xsl:variable name="invoiceLocation">
          <xsl:call-template name="GetNoteTextFallbackCustomizedField">
            <xsl:with-param name="noteDescription" select="'Invoice Location'"/>
            <xsl:with-param name="customizedFieldKey" select="'Invoice Location'"/>
            <xsl:with-param name="customizedFieldParentLevel" select="$shipmentLevel"/>
            <xsl:with-param name="invoiceNumber" select="$invoiceNumber"/>
            <xsl:with-param name="customizedFieldValueIndex" select="0"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:variable name="invoiceCountry">
          <xsl:call-template name="GetNoteTextFallbackCustomizedField">
            <xsl:with-param name="noteDescription" select="'Invoice Country'"/>
            <xsl:with-param name="customizedFieldKey" select="'Invoice Location'"/>
            <xsl:with-param name="customizedFieldParentLevel" select="$shipmentLevel"/>
            <xsl:with-param name="invoiceNumber" select="$invoiceNumber"/>
            <xsl:with-param name="customizedFieldValueIndex" select="1"/>
          </xsl:call-template>          
        </xsl:variable>
        <xsl:call-template name="WriteLOC_3Raw">
          <xsl:with-param name="placeLocationQualifier" select="'121'"/>
          <xsl:with-param name="placeLocationIdentification" select="$invoiceCountry"/>
          <xsl:with-param name="placeLocation" select="$invoiceLocation"/>
        </xsl:call-template>
        <xsl:variable name="shipFromLocation">
          <xsl:call-template name="GetNoteTextRaw">
            <xsl:with-param name="description" select="'Ship From Location'"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:variable name="shipFromCountry">
          <xsl:call-template name="GetNoteTextRaw">
            <xsl:with-param name="description" select="'Ship From Country'"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:call-template name="WriteLOC_3Raw">
          <xsl:with-param name="placeLocationQualifier" select="'5'"/>
          <xsl:with-param name="placeLocationIdentification" select="$shipFromCountry"/>
          <xsl:with-param name="placeLocation" select="$shipFromLocation"/>
        </xsl:call-template>
        <xsl:variable name="invoiceToLocation">
          <xsl:call-template name="GetNoteTextRaw">
            <xsl:with-param name="description" select="'Invoice To Location'"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:variable name="invoiceToCountry">
          <xsl:call-template name="GetNoteTextRaw">
            <xsl:with-param name="description" select="'Invoice To Country'"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:call-template name="WriteLOC_3Raw">
          <xsl:with-param name="placeLocationQualifier" select="'2'"/>
          <xsl:with-param name="placeLocationIdentification" select="$invoiceToCountry"/>
          <xsl:with-param name="placeLocation" select="$invoiceToLocation"/>
        </xsl:call-template>
        <xsl:variable name="shipToLocation">
          <xsl:call-template name="GetNoteTextRaw">
            <xsl:with-param name="description" select="'Ship To Location'"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:variable name="shipToCountry">
          <xsl:call-template name="GetNoteTextRaw">
            <xsl:with-param name="description" select="'Ship To Country'"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:call-template name="WriteLOC_3Raw">
          <xsl:with-param name="placeLocationQualifier" select="'7'"/>
          <xsl:with-param name="placeLocationIdentification" select="$shipToCountry"/>
          <xsl:with-param name="placeLocation" select="$shipToLocation"/>
        </xsl:call-template>
        <xsl:call-template name="WriteLOC_3Raw">
          <xsl:with-param name="placeLocationQualifier" select="'19'"/>
          <xsl:with-param name="placeLocationIdentification" select="*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][*[local-name()='AddressType']/text()='Manufacturer']
                                                                    /*[local-name()='State']/text()"/>
        </xsl:call-template>
        <xsl:call-template name="WriteDTM_3Raw">
          <xsl:with-param name="dateTimePeriodQualifier" select="'3'"/>
          <xsl:with-param name="dateTimePeriod" select="*[local-name()='InvoiceDate']/text()"/>
          <xsl:with-param name="dateTimePeriodFormatQualifier" select="'101'"/>
        </xsl:call-template>
        <xsl:variable name="dateOfExport">
          <xsl:call-template name="GetAddinfoValueRaw">
            <xsl:with-param name="key" select="'DateOfExport'"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:call-template name="WriteDTM_3Raw">
          <xsl:with-param name="dateTimePeriodQualifier" select="'129'"/>
          <xsl:with-param name="dateTimePeriod" select="$dateOfExport"/>
          <xsl:with-param name="dateTimePeriodFormatQualifier" select="'101'"/>
        </xsl:call-template>
        <xsl:variable name="chargeIndicator">
          <xsl:call-template name="GetNoteTextFallbackCustomizedField">
            <xsl:with-param name="noteDescription" select="'Charge Indicator'"/>
            <xsl:with-param name="customizedFieldKey" select="'Charge Indicator'"/>
            <xsl:with-param name="customizedFieldParentLevel" select="$shipmentLevel"/>
            <xsl:with-param name="invoiceNumber" select="$invoiceNumber"/>
            <xsl:with-param name="customizedFieldValueIndex" select="0"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:call-template name="WriteGIS_3Raw">
          <xsl:with-param name="processingIndicatorCoded" select="'32'"/>
          <xsl:with-param name="processTypeIdentification" select="$chargeIndicator"/>
          <xsl:with-param name="alwaysGenerate" select="'true'"/>
        </xsl:call-template>
        <xsl:variable name="manualElectronicIndicator">
          <xsl:call-template name="GetCustomizedFieldRaw">
            <xsl:with-param name="key" select="'ME Indicator'"/>
            <xsl:with-param name="parentLevel" select="$shipmentLevel"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:call-template name="WriteGIS_3Raw">
          <xsl:with-param name="processingIndicatorCoded" select="'85'"/>
          <xsl:with-param name="processTypeIdentification" select="$manualElectronicIndicator"/>
          <xsl:with-param name="alwaysGenerate" select="'true'"/>
        </xsl:call-template>
        <xsl:call-template name="WriteMEARaw">
          <xsl:with-param name="measurementPurposeQualifier" select="'WT'"/>
          <xsl:with-param name="measureUnitQualifier" select="'KGM'"/>
          <xsl:with-param name="measurementValue" select="*[local-name()='Weight']/text()"/>
        </xsl:call-template>
        <xsl:variable name="invoiceValueOfGoods" select="sum(*[local-name()='CommercialInvoiceLineCollection']/*[local-name()='CommercialInvoiceLine']/*[local-name()='LinePrice'])"/>
        <xsl:variable name="invoiceValueOfGoodsInLocalCurrency">
          <xsl:call-template name="ConvertAmountToLocalCurrency">
            <xsl:with-param name="foreignAmount" select="$invoiceValueOfGoods"/>
            <xsl:with-param name="exchangeRate" select="$exchangeRate"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:call-template name="WriteMOALoop1Raw">
          <xsl:with-param name="monetaryAmountTypeQualifier" select="'38'"/>
          <xsl:with-param name="monetaryAmount" select="$invoiceValueOfGoodsInLocalCurrency"/>
        </xsl:call-template>
        <xsl:call-template name="WriteMOALoop1Raw">
          <xsl:with-param name="monetaryAmountTypeQualifier" select="'66'"/>
          <xsl:with-param name="monetaryAmount" select="$invoiceValueOfGoods"/>
        </xsl:call-template>
        <xsl:call-template name="WriteMOALoop1FromCommercialChargeRaw">
          <xsl:with-param name="monetaryAmountTypeQualifier" select="'106'"/>
          <xsl:with-param name="chargeTypeCode" select="'PAC'"/>
          <xsl:with-param name="convertToLocalCurrency" select="'Y'"/>
          <xsl:with-param name="fallbackExchangeRate" select="$exchangeRate"/>
        </xsl:call-template>
        <xsl:call-template name="WriteMOALoop1FromCommercialChargeRaw">
          <xsl:with-param name="monetaryAmountTypeQualifier" select="'189'"/>
          <xsl:with-param name="chargeTypeCode" select="'PAC'"/>
        </xsl:call-template>
        <xsl:call-template name="WriteMOALoop1FromCommercialChargeRaw">
          <xsl:with-param name="monetaryAmountTypeQualifier" select="'342'"/>
          <xsl:with-param name="chargeTypeCode" select="'DED'"/>
          <xsl:with-param name="convertToLocalCurrency" select="'Y'"/>
          <xsl:with-param name="fallbackExchangeRate" select="$exchangeRate"/>
        </xsl:call-template>
        <xsl:call-template name="WriteMOALoop1FromCommercialChargeRaw">
          <xsl:with-param name="monetaryAmountTypeQualifier" select="'168'"/>
          <xsl:with-param name="chargeTypeCode" select="'DED'"/>
        </xsl:call-template>
        <xsl:call-template name="WriteMOALoop1FromCommercialChargeRaw">
          <xsl:with-param name="monetaryAmountTypeQualifier" select="'304'"/>
          <xsl:with-param name="chargeTypeCode" select="'OTH'"/>
          <xsl:with-param name="convertToLocalCurrency" select="'Y'"/>
          <xsl:with-param name="fallbackExchangeRate" select="$exchangeRate"/>
        </xsl:call-template>
        <xsl:call-template name="WriteMOALoop1FromCommercialChargeRaw">
          <xsl:with-param name="monetaryAmountTypeQualifier" select="'105'"/>
          <xsl:with-param name="chargeTypeCode" select="'OTH'"/>
        </xsl:call-template>
        <xsl:call-template name="WriteMOALoop1Raw">
          <xsl:with-param name="monetaryAmountTypeQualifier" select="'39'"/>
          <xsl:with-param name="monetaryAmount" select="$invoiceAmountInLocalCurrency"/>
        </xsl:call-template>
        <xsl:call-template name="WriteMOALoop1Raw">
          <xsl:with-param name="monetaryAmountTypeQualifier" select="'77'"/>
          <xsl:with-param name="monetaryAmount" select="$invoiceAmount"/>
        </xsl:call-template>
        <xsl:call-template name="WriteMOALoop1Raw">
          <xsl:with-param name="monetaryAmountTypeQualifier" select="'10'"/>
          <xsl:with-param name="monetaryAmount" select="$exchangeRate"/>
          <xsl:with-param name="formatMonetaryAmount" select="'false'"/>
          <xsl:with-param name="currencyDetailsQualifier" select="'6'"/>
          <xsl:with-param name="currencyRateBase" select="substring(*[local-name()='InvoiceCurrency']/*[local-name()='Code']/text(),1,2)"/>
          <xsl:with-param name="rateOfExchange" select="$exchangeRate"/>
        </xsl:call-template>
        <xsl:variable name="totalMPF">
          <xsl:call-template name="GetTotalMPF">
            <xsl:with-param name="parentLevel" select="$shipmentLevel/.."/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:variable name="userFeeForInvoice" select="$totalMPF*(number($invoiceAmount) div number($totalInvoiceAmountInShipment))"/>
        <xsl:call-template name="WriteMOALoop1Raw">
          <xsl:with-param name="monetaryAmountTypeQualifier" select="'58'"/>
          <xsl:with-param name="monetaryAmount" select="$userFeeForInvoice"/>
        </xsl:call-template>
        <xsl:call-template name="WriteMOALoop1SumOfLineCommercialChargeRaw">
          <xsl:with-param name="monetaryAmountTypeQualifier" select="'124'"/>
          <xsl:with-param name="chargeTypeCode" select="'FEE'"/>
          <xsl:with-param name="chargeSubTypeCode" select="'501'"/>
        </xsl:call-template>
        <xsl:call-template name="WriteMOALoop1FromCommercialChargeRaw">
          <xsl:with-param name="monetaryAmountTypeQualifier" select="'205'"/>
          <xsl:with-param name="chargeTypeCode" select="'ADD'"/>
          <xsl:with-param name="convertToLocalCurrency" select="'Y'"/>
          <xsl:with-param name="fallbackExchangeRate" select="$exchangeRate"/>
        </xsl:call-template>
        <xsl:call-template name="WriteMOALoop1FromCommercialChargeRaw">
          <xsl:with-param name="monetaryAmountTypeQualifier" select="'206'"/>
          <xsl:with-param name="chargeTypeCode" select="'ADD'"/>
        </xsl:call-template>
        <xsl:call-template name="WriteMOALoop1SumOfLineAddInfoRaw">
          <xsl:with-param name="monetaryAmountTypeQualifier" select="'290'"/>
          <xsl:with-param name="key" select="'ADDuty'"/>
        </xsl:call-template>
        <xsl:call-template name="WriteMOALoop1SumOfLineAddInfoRaw">
          <xsl:with-param name="monetaryAmountTypeQualifier" select="'298'"/>
          <xsl:with-param name="key" select="'CVDuty'"/>
        </xsl:call-template>
        <xsl:call-template name="WriteMOALoop1FromCommercialChargeRaw">
          <xsl:with-param name="monetaryAmountTypeQualifier" select="'313'"/>
          <xsl:with-param name="chargeTypeCode" select="'FIF'"/>
          <xsl:with-param name="convertToLocalCurrency" select="'Y'"/>
          <xsl:with-param name="fallbackExchangeRate" select="$exchangeRate"/>
        </xsl:call-template>
        <xsl:call-template name="WriteMOALoop1FromCommercialChargeRaw">
          <xsl:with-param name="monetaryAmountTypeQualifier" select="'291'"/>
          <xsl:with-param name="chargeTypeCode" select="'FIF'"/>
        </xsl:call-template>

        <xsl:variable name="Duties" select="sum(./*[local-name()='CommercialInvoiceLineCollection']/*[local-name()='CommercialInvoiceLine']
                      /*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key'] = 'Duty']/*[local-name()='Value'])" />
        <xsl:call-template name="WriteMOALoop1Raw">
          <xsl:with-param name="monetaryAmountTypeQualifier" select="'350'"/>
          <xsl:with-param name="monetaryAmount" select="$Duties"/>
        </xsl:call-template>

        <xsl:variable name="SupDuties" select="sum(./*[local-name()='CommercialInvoiceLineCollection']/*[local-name()='CommercialInvoiceLine']
                      /*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key'] = 'SupDuty']/*[local-name()='Value'])" />
        <xsl:call-template name="WriteMOALoop1Raw">
          <xsl:with-param name="monetaryAmountTypeQualifier" select="'353'"/>
          <xsl:with-param name="monetaryAmount" select="$SupDuties"/>
        </xsl:call-template>

        <xsl:call-template name="WriteMOALoop1Raw">
          <xsl:with-param name="monetaryAmountTypeQualifier" select="'55'"/>
          <xsl:with-param name="monetaryAmount" select="$Duties + $SupDuties"/>
        </xsl:call-template>
        
        <xsl:call-template name="WriteCSTLoop1FromInvoiceLineRaw">
          <xsl:with-param name="exchangeRateFromHeader" select="$exchangeRate"/>
        </xsl:call-template>
      </xsl:element>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="WriteDOCFromEntryLine">
    <xsl:for-each select="*[local-name()='Shipment']/*[local-name()='EntryHeaderCollection']
                          /*[local-name()='EntryHeader'][*[local-name()='Type']/*[local-name()='Code']/text()='ENS']
                          /*[local-name()='EntryLineCollection']/*[local-name()='EntryLine']
                          [./*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key'] = 'SupLine']/*[local-name()='Value'] = 'N']">
      <xsl:variable name="shipmentLevel" select="../../../.."/>
      <xsl:variable name="entryLineNumber" select="*[local-name()='LineNumber']/text()"/>
      <xsl:variable name="invoice" select="$shipmentLevel/*[local-name()='CommercialInfo']
                          /*[local-name()='CommercialInvoiceCollection']
                          /*[local-name()='CommercialInvoice'][*[local-name()='CommercialInvoiceLineCollection']/*[local-name()='CommercialInvoiceLine']/*[local-name()='EntryLineNumber']/text()=$entryLineNumber]"/>
      <xsl:variable name="invoiceLine" select="$shipmentLevel/*[local-name()='CommercialInfo']
                          /*[local-name()='CommercialInvoiceCollection']/*[local-name()='CommercialInvoice']
                          /*[local-name()='CommercialInvoiceLineCollection']
                          /*[local-name()='CommercialInvoiceLine'][*[local-name()='EntryLineNumber']/text()=$entryLineNumber]"/>
      <xsl:element name="ns0:DOCLoop1">
        <xsl:call-template name="WriteDOCRaw">
          <xsl:with-param name="documentMessageNameCoded" select="'789'"/>
          <xsl:with-param name="documentMessageName" select="$entryLineNumber"/>
        </xsl:call-template>
        <xsl:call-template name="WritePACRaw">
          <xsl:with-param name="numberOfPackages" select="sum($invoiceLine/*[local-name()='CustomsQuantity'])"/>
        </xsl:call-template>
        <xsl:call-template name="WriteRFF_3Raw">
          <xsl:with-param name="referenceQualifier" select="'HS'"/>
          <xsl:with-param name="referenceNumber" select="*[local-name()='HarmonisedCode']/text()"/>
        </xsl:call-template>

        <xsl:variable name="RelatedEntryLines" select="../*[local-name()='EntryLine'][./*[local-name()='LineNumber'] = $entryLineNumber]" />
        <xsl:variable name="TSUSNumber" select="$RelatedEntryLines
                      /*[local-name()='HarmonisedCode'][starts-with(., '9999') or starts-with(., '9803') or starts-with(., '9903')]" />
        <xsl:if test="$TSUSNumber != ''">
          <xsl:call-template name="WriteRFF_3Raw">
            <xsl:with-param name="referenceQualifier">
              <xsl:choose>

                <xsl:when test="starts-with($TSUSNumber, '9903')">
                  <xsl:text>ABD</xsl:text>
                </xsl:when>

                <xsl:otherwise>
                  <xsl:text>AFG</xsl:text>
                </xsl:otherwise>

              </xsl:choose>
            </xsl:with-param>
            <xsl:with-param name="referenceNumber" select="$TSUSNumber"/>
          </xsl:call-template>
        </xsl:if>

        <xsl:variable name="manufacturerShipperID">
          <xsl:call-template name="GetRegistrationNumberOfOrganizationAddress">
            <xsl:with-param name="addressType" select="'Manufacturer'"/>
            <xsl:with-param name="registrationNumberTypeCode" select="'MID'"/>
            <xsl:with-param name="fallbackLevel" select="$invoiceLine"/>
            <xsl:with-param name="fallbackLevel1" select="$invoice"/>
            <xsl:with-param name="fallbackLevel2" select="$shipmentLevel"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:call-template name="WriteRFF_3Raw">
          <xsl:with-param name="referenceQualifier" select="'SI'"/>
          <xsl:with-param name="referenceNumber" select="$manufacturerShipperID"/>
        </xsl:call-template>
        <xsl:variable name="countryOfOrigin">
          <xsl:call-template name="GetAddinfoValueRaw">
            <xsl:with-param name="key" select="'UC_NKCountryOfOrigin'"/>
            <xsl:with-param name="fallbackLevel" select="$invoiceLine"/>
            <xsl:with-param name="fallbackLevel1" select="$invoice"/>
            <xsl:with-param name="fallbackLevel2" select="$shipmentLevel"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:call-template name="WriteLOC_3Raw">
          <xsl:with-param name="placeLocationQualifier" select="'27'"/>
          <xsl:with-param name="placeLocationIdentification" select="$countryOfOrigin"/>
          <xsl:with-param name="placeLocation" select="$countryOfOrigin"/>
        </xsl:call-template>
        <xsl:call-template name="WriteGIS_3Raw">
          <xsl:with-param name="processingIndicatorCoded" select="'36'"/>
          <xsl:with-param name="processTypeIdentification" select="'7'"/>
        </xsl:call-template>
        <xsl:call-template name="WriteMEARaw">
          <xsl:with-param name="measurementPurposeQualifier" select="'WT'"/>
          <xsl:with-param name="measureUnitQualifier" select="'KGM'"/>
          <xsl:with-param name="measurementValue" select="sum($invoiceLine/*[local-name()='Weight'])"/>
        </xsl:call-template>
        <xsl:variable name="customsQuantityUnitCode" select="$invoiceLine/*[local-name()='CustomsQuantityUnit']/*[local-name()='Code']/text()"/>
        <xsl:call-template name="WriteMEARaw">
          <xsl:with-param name="measurementPurposeQualifier" select="'AAR'"/>
          <xsl:with-param name="measureUnitQualifier" select="ScriptNS1:GetRecipientCode('GEOGSCGUS','GEOGSCGUS_ICR','IBM CUSRES File - Generate from Import Customs Dec','Unit of Measurement','IBM Code', string($customsQuantityUnitCode))"/>
          <xsl:with-param name="measurementValue" select="sum($invoiceLine/*[local-name()='CustomsQuantity'])"/>
        </xsl:call-template>
        <xsl:variable name="secondUQ">
          <xsl:call-template name="GetAddinfoValueRaw">
            <xsl:with-param name="key" select="'SecondUQ'"/>
            <xsl:with-param name="fallbackLevel" select="$invoiceLine"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:variable name="secondQty">
          <xsl:call-template name="GetSumAddinfoValueRaw">
            <xsl:with-param name="key" select="'SecondQty'"/>
            <xsl:with-param name="fallbackLevel" select="$invoiceLine"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:call-template name="WriteMEARaw">
          <xsl:with-param name="measurementPurposeQualifier" select="'AAS'"/>
          <xsl:with-param name="measureUnitQualifier" select="ScriptNS1:GetRecipientCode('GEOGSCGUS','GEOGSCGUS_ICR','IBM CUSRES File - Generate from Import Customs Dec','Unit of Measurement','IBM Code', string($secondUQ))"/>
          <xsl:with-param name="measurementValue" select="$secondQty"/>
        </xsl:call-template>
        <xsl:variable name="thirdUQ">
          <xsl:call-template name="GetAddinfoValueRaw">
            <xsl:with-param name="key" select="'ThirdUQ'"/>
            <xsl:with-param name="fallbackLevel" select="$invoiceLine"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:variable name="thirdQty">
          <xsl:call-template name="GetSumAddinfoValueRaw">
            <xsl:with-param name="key" select="'ThirdQty'"/>
            <xsl:with-param name="fallbackLevel" select="$invoiceLine"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:call-template name="WriteMEARaw">
          <xsl:with-param name="measurementPurposeQualifier" select="'AAT'"/>
          <xsl:with-param name="measureUnitQualifier" select="ScriptNS1:GetRecipientCode('GEOGSCGUS','GEOGSCGUS_ICR','IBM CUSRES File - Generate from Import Customs Dec','Unit of Measurement','IBM Code', string($thirdUQ))"/>
          <xsl:with-param name="measurementValue" select="$thirdQty"/>
        </xsl:call-template>
        <xsl:call-template name="WriteMOALoop1Raw">
          <xsl:with-param name="monetaryAmountTypeQualifier" select="'38'"/>
          <xsl:with-param name="monetaryAmount" select="$invoiceLine/*[local-name()='CustomsValue']/text()"/>
        </xsl:call-template>
        <xsl:call-template name="WriteMOALoop1FromEntryLineCommercialCharge">
          <xsl:with-param name="monetaryAmountTypeQualifier" select="'106'"/>
          <xsl:with-param name="chargeTypeCode" select="'PAC'"/>
          <xsl:with-param name="fallbackLevel" select="$invoiceLine"/>
        </xsl:call-template>
        <xsl:call-template name="WriteMOALoop1FromEntryLineCommercialCharge">
          <xsl:with-param name="monetaryAmountTypeQualifier" select="'291'"/>
          <xsl:with-param name="chargeTypeCode" select="'FIF'"/>
          <xsl:with-param name="fallbackLevel" select="$invoiceLine"/>
        </xsl:call-template>
        <xsl:call-template name="WriteMOALoop1FromEntryLineCommercialCharge">
          <xsl:with-param name="monetaryAmountTypeQualifier" select="'304'"/>
          <xsl:with-param name="chargeTypeCode" select="'OTH'"/>
          <xsl:with-param name="fallbackLevel" select="$invoiceLine"/>
        </xsl:call-template>

        <xsl:call-template name="WriteMOALoop1Raw">
          <xsl:with-param name="monetaryAmountTypeQualifier" select="'339'"/>
          <xsl:with-param name="monetaryAmount" select="sum($RelatedEntryLines/*[local-name()='CustomsValue'])"/>
        </xsl:call-template>

        <xsl:call-template name="WriteMOALoop1Raw">
          <xsl:with-param name="monetaryAmountTypeQualifier" select="'55'"/>
          <xsl:with-param name="monetaryAmount" select="sum($RelatedEntryLines[not(starts-with(./*[local-name()='HarmonisedCode'], '9903'))]
                          /*[local-name()='EntryLineChargeCollection']/*[local-name()='EntryLineCharge'][./*[local-name()='Type']/*[local-name()='Code'] = 'DTY']
                          /*[local-name()='Amount'])"/>
        </xsl:call-template>

        <xsl:call-template name="WriteMOALoop1Raw">
          <xsl:with-param name="monetaryAmountTypeQualifier" select="'161'"/>
          <xsl:with-param name="monetaryAmount" select="sum($RelatedEntryLines[starts-with(./*[local-name()='HarmonisedCode'], '9903')]
                          /*[local-name()='EntryLineChargeCollection']/*[local-name()='EntryLineCharge'][./*[local-name()='Type']/*[local-name()='Code'] = 'DTY']
                          /*[local-name()='Amount'])"/>
        </xsl:call-template>
        
        <xsl:call-template name="WriteMOALoop1FromEntryLineCommercialCharge">
          <xsl:with-param name="monetaryAmountTypeQualifier" select="'205'"/>
          <xsl:with-param name="chargeTypeCode" select="'ADD'"/>
          <xsl:with-param name="fallbackLevel" select="$invoiceLine"/>
        </xsl:call-template>
        <xsl:call-template name="WriteMOALoop1FromEntryLineCommercialCharge">
          <xsl:with-param name="monetaryAmountTypeQualifier" select="'124'"/>
          <xsl:with-param name="chargeTypeCode" select="'501'"/>
          <xsl:with-param name="fallbackLevel" select="$invoiceLine"/>
        </xsl:call-template>
        <xsl:call-template name="WriteMOALoop1FromEntryLineCommercialCharge">
          <xsl:with-param name="monetaryAmountTypeQualifier" select="'58'"/>
          <xsl:with-param name="chargeTypeCode" select="'499'"/>
          <xsl:with-param name="fallbackLevel" select="$invoiceLine"/>
        </xsl:call-template>
        <xsl:element name="ns0:CSTLoop1">
          <xsl:variable name="aDDDecID">
            <xsl:call-template name="GetAddinfoValueRaw">
              <xsl:with-param name="key" select="'ADDDecID'"/>
              <xsl:with-param name="fallbackLevel" select="$invoiceLine"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="cVDDecID">
            <xsl:call-template name="GetAddinfoValueRaw">
              <xsl:with-param name="key" select="'CVDDecID'"/>
              <xsl:with-param name="fallbackLevel" select="$invoiceLine"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="aDDCVDCompCode">
            <xsl:choose>
              <xsl:when test="normalize-space($aDDDecID)!=''">
                <xsl:value-of select="$aDDDecID"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$cVDDecID"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <xsl:variable name="aDDCaseNo">
            <xsl:call-template name="GetAddinfoValueRaw">
              <xsl:with-param name="key" select="'ADDCaseNo'"/>
              <xsl:with-param name="fallbackLevel" select="$invoiceLine"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="cVDCaseNo">
            <xsl:call-template name="GetAddinfoValueRaw">
              <xsl:with-param name="key" select="'CVDCaseNo'"/>
              <xsl:with-param name="fallbackLevel" select="$invoiceLine"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="aDDCVDCaseNo">
            <xsl:choose>
              <xsl:when test="normalize-space($aDDCaseNo)!=''">
                <xsl:value-of select="$aDDCaseNo"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$cVDCaseNo"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <xsl:call-template name="WriteCSTRaw">
            <xsl:with-param name="goodsItemNumber" select="$aDDCVDCompCode"/>
            <xsl:with-param name="customsCodeIdentification" select="$aDDCVDCaseNo"/>
            <xsl:with-param name="codeListQualifier" select="'191'"/>
          </xsl:call-template>
          <xsl:element name="ns0:TAXLoop2">
            <xsl:call-template name="WriteTAX_2Raw">
              <xsl:with-param name="dutyTaxFeeFunctionQualifier" select="'5'"/>
              <xsl:with-param name="dutyTaxFeeTypeCoded" select="'ADD'"/>
            </xsl:call-template>
            <xsl:variable name="aDDDuty">
              <xsl:call-template name="GetSumAddinfoValueRaw">
                <xsl:with-param name="key" select="'ADDuty'"/>
                <xsl:with-param name="fallbackLevel" select="$invoiceLine"/>
              </xsl:call-template>
            </xsl:variable>
            <xsl:variable name="cVDDuty">
              <xsl:call-template name="GetSumAddinfoValueRaw">
                <xsl:with-param name="key" select="'CVDuty'"/>
                <xsl:with-param name="fallbackLevel" select="$invoiceLine"/>
              </xsl:call-template>
            </xsl:variable>
            <xsl:variable name="aDDCVDDutyAmount">
              <xsl:choose>
                <xsl:when test="number($aDDDuty)">
                  <xsl:value-of select="$aDDDuty"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$cVDDuty"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <xsl:call-template name="WriteMOA_3Raw">
              <xsl:with-param name="monetaryAmountTypeQualifier" select="'55'"/>
              <xsl:with-param name="monetaryAmount" select="$aDDCVDDutyAmount"/>
            </xsl:call-template>
            <xsl:variable name="aDDQty">
              <xsl:call-template name="GetSumAddinfoValueRaw">
                <xsl:with-param name="key" select="'ADDQty'"/>
                <xsl:with-param name="fallbackLevel" select="$invoiceLine"/>
              </xsl:call-template>
            </xsl:variable>
            <xsl:variable name="cVDQty">
              <xsl:call-template name="GetSumAddinfoValueRaw">
                <xsl:with-param name="key" select="'CVDQty'"/>
                <xsl:with-param name="fallbackLevel" select="$invoiceLine"/>
              </xsl:call-template>
            </xsl:variable>
            <xsl:variable name="aDDCVDQtyAmount">
              <xsl:choose>
                <xsl:when test="number($aDDQty)">
                  <xsl:value-of select="$aDDQty"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$cVDQty"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <xsl:call-template name="WriteRFF_4Raw">
              <xsl:with-param name="referenceQualifier" select="'ABC'"/>
              <xsl:with-param name="referenceVersionNumber" select="$aDDCVDQtyAmount"/>
            </xsl:call-template>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="WriteDOCFromDeclaration">
    <xsl:for-each select="*[local-name()='Shipment']">
      <xsl:element name="ns0:DOCLoop1">
        <xsl:call-template name="WriteDOCRaw">
          <xsl:with-param name="documentMessageNameCoded" select="'340'"/>
        </xsl:call-template>
        <xsl:variable name="totalNoOfPacks" select="*[local-name()='TotalNoOfPacks']/text()"/>
        <xsl:call-template name="WritePACRaw">
          <xsl:with-param name="numberOfPackages" select="$totalNoOfPacks"/>
        </xsl:call-template>
        <xsl:variable name="totalNoOfInvoices" select="count(*[local-name()='CommercialInfo']/*[local-name()='CommercialInvoiceCollection']/*[local-name()='CommercialInvoice'])"/>
        <xsl:call-template name="WriteRFF_3Raw">
          <xsl:with-param name="referenceQualifier" select="'AFL'"/>
          <xsl:with-param name="referenceNumber" select="$totalNoOfInvoices"/>
        </xsl:call-template>
        <xsl:variable name="portOfDischargeCode" select="*[local-name()='PortOfDischarge']/*[local-name()='Code']/text()"/>
        <xsl:variable name="entryTime">
          <xsl:call-template name="GetAddinfoValueRaw">
            <xsl:with-param name="key" select="'EntryDate'"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:call-template name="WriteDTM_3Raw">
          <xsl:with-param name="dateTimePeriodQualifier" select="'178'"/>
          <xsl:with-param name="dateTimePeriod" select="$entryTime"/>
          <xsl:with-param name="dateTimePeriodFormatQualifier" select="'401'"/>
        </xsl:call-template>
        <xsl:call-template name="WriteDTM_3Raw">
          <xsl:with-param name="dateTimePeriodQualifier" select="'261'"/>
          <xsl:with-param name="dateTimePeriod" select="*[local-name()='DateCollection']
                                                         /*[local-name()='Date'][*[local-name()='Type']/text()='EntryAuthorisation']
                                                         /*[local-name()='Value']/text()"/>
          <xsl:with-param name="dateTimePeriodFormatQualifier" select="'401'"/>
        </xsl:call-template>
        <xsl:variable name="totalWeight" select="*[local-name()='TotalWeight']/text()"/>
        <xsl:call-template name="WriteMEARaw">
          <xsl:with-param name="measurementPurposeQualifier" select="'WT'"/>
          <xsl:with-param name="measureUnitQualifier" select="'KGM'"/>
          <xsl:with-param name="measurementValue" select="$totalWeight"/>
        </xsl:call-template>
        <xsl:variable name="totalEnteredValue" select="sum(*[local-name()='EntryHeaderCollection']
                                                      /*[local-name()='EntryHeader'][*[local-name()='Type']/*[local-name()='Code']/text()='ENS']
                                                      /*[local-name()='EntryLineCollection']/*[local-name()='EntryLine']/*[local-name()='CustomsValue']/text())"/>
        <xsl:call-template name="WriteMOALoop1Raw">
          <xsl:with-param name="monetaryAmountTypeQualifier" select="'43'"/>
          <xsl:with-param name="monetaryAmount" select="$totalEnteredValue"/>
        </xsl:call-template>
      </xsl:element>
    </xsl:for-each>
  </xsl:template>

  <!--WriteDOC Raw 1 BEGIN-->

  <xsl:template name="ConvertAmountToLocalCurrency">
    <xsl:param name="foreignAmount"/>
    <xsl:param name="exchangeRate"/>
    <xsl:choose>
      <xsl:when test="normalize-space($exchangeRate)!='' and number($exchangeRate)!='0'">
        <xsl:value-of select="number($foreignAmount) * number($exchangeRate)"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$foreignAmount"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="WriteRFF_4Raw">
    <xsl:param name="referenceQualifier"/>
    <xsl:param name="referenceNumber" select="''"/>
    <xsl:param name="lineNumber" select="''"/>
    <xsl:param name="referenceVersionNumber" select="''"/>
    <xsl:if test="normalize-space($referenceNumber)!='' or normalize-space($lineNumber)!='' or normalize-space($referenceVersionNumber)!=''">
      <xsl:element name="ns0:RFF_4">
        <xsl:element name="ns0:C506_4">
          <xsl:element name="C50601">
            <xsl:value-of select="$referenceQualifier"/>
          </xsl:element>
          <xsl:if test="normalize-space($referenceNumber)!=''">
            <xsl:element name="C50602">
              <xsl:value-of select="$referenceNumber"/>
            </xsl:element>
          </xsl:if>
          <xsl:if test="normalize-space($lineNumber)!=''">
            <xsl:element name="C50603">
              <xsl:value-of select="$lineNumber"/>
            </xsl:element>
          </xsl:if>
          <xsl:if test="normalize-space($referenceVersionNumber)!=''">
            <xsl:element name="C50604">
              <xsl:value-of select="$referenceVersionNumber"/>
            </xsl:element>
          </xsl:if>
        </xsl:element>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="WriteMEA_2Raw">
    <xsl:param name="measurementPurposeQualifier"/>
    <xsl:param name="propertyMeasuredCoded" select="''"/>
    <xsl:param name="measurementAttributeIdentification" select="''"/>
    <xsl:param name="measurementAttribute" select="''"/>
    <xsl:param name="measureUnitQualifier" select="''"/>
    <xsl:param name="measurementValue" select="''"/>
    <xsl:if test="(normalize-space($propertyMeasuredCoded)!='' and normalize-space($measurementAttribute)!='') or (normalize-space($measureUnitQualifier)!='' and normalize-space($measurementValue)!='')">
      <xsl:element name="ns0:MEA_2">
        <xsl:element name="MEA01">
          <xsl:value-of select="$measurementPurposeQualifier"/>
        </xsl:element>
        <xsl:if test="normalize-space($propertyMeasuredCoded)!='' and normalize-space($measurementAttribute)!=''">
          <xsl:element name="ns0:C502_2">
            <xsl:element name="C50201">
              <xsl:value-of select="$propertyMeasuredCoded"/>
            </xsl:element>
            <xsl:if test="normalize-space($measurementAttributeIdentification)!=''">
              <xsl:element name="C50203">
                <xsl:value-of select="ScriptNS0:GetRecipientCode('GEOGSCGUS','GEOGSCGUS_ICR','IBM CUSRES File - Generate from Import Customs Dec','Unit of Measurement','IBM Code', string($measurementAttributeIdentification))"/>
              </xsl:element>
            </xsl:if>
            <xsl:element name="C50204">
              <xsl:value-of select="$measurementAttribute"/>
            </xsl:element>
          </xsl:element>
        </xsl:if>
        <xsl:if test="normalize-space($measureUnitQualifier)!='' and normalize-space($measurementValue)!=''">
          <xsl:element name="ns0:C174_2">
            <xsl:element name="C17401">
              <xsl:value-of select="ScriptNS0:GetRecipientCode('GEOGSCGUS','GEOGSCGUS_ICR','IBM CUSRES File - Generate from Import Customs Dec','Unit of Measurement','IBM Code', string($measureUnitQualifier))"/>
            </xsl:element>
            <xsl:element name="C17402">
              <xsl:call-template name="FormatMEAValue">
                <xsl:with-param name="measurementValue" select="$measurementValue"/>
              </xsl:call-template>
            </xsl:element>
          </xsl:element>
        </xsl:if>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="WriteGIS_4Raw">
    <xsl:param name="processingIndicatorCoded"/>
    <xsl:param name="processTypeIdentification"/>
    <xsl:if test="normalize-space($processTypeIdentification)!=''">
      <xsl:element name="ns0:GIS_4">
        <xsl:element name="ns0:C529_4">
          <xsl:element name="C52901">
            <xsl:value-of select="$processingIndicatorCoded" />
          </xsl:element>
          <xsl:element name="C52904">
            <xsl:value-of select="$processTypeIdentification" />
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="WriteGIS_3Raw">
    <xsl:param name="processingIndicatorCoded"/>
    <xsl:param name="processTypeIdentification"/>
    <xsl:param name="alwaysGenerate" select="'false'"/>
    <xsl:if test="normalize-space($alwaysGenerate)='true' or normalize-space($processTypeIdentification)!=''">
      <xsl:element name="ns0:GIS_3">
        <xsl:element name="ns0:C529_3">
          <xsl:element name="C52901">
            <xsl:value-of select="$processingIndicatorCoded" />
          </xsl:element>
          <xsl:element name="C52904">
            <xsl:value-of select="$processTypeIdentification" />
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="WriteMOA_3Raw">
    <xsl:param name="monetaryAmountTypeQualifier"/>
    <xsl:param name="monetaryAmount" select="''"/>
    <xsl:if test="normalize-space($monetaryAmountTypeQualifier)!=''">
      <xsl:variable name="monetaryAmountOrZero">
        <xsl:call-template name="NumberOrDefault">
          <xsl:with-param name="number" select="$monetaryAmount"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:element name="ns0:MOA_3">
        <xsl:element name="ns0:C516_3">
          <xsl:element name="C51601">
            <xsl:value-of select="$monetaryAmountTypeQualifier"/>
          </xsl:element>
          <xsl:element name="C51602">
            <xsl:call-template name="FormatMOAAmount">
              <xsl:with-param name="monetaryAmount" select="$monetaryAmountOrZero"/>
            </xsl:call-template>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="WriteTAX_2Raw">
    <xsl:param name="dutyTaxFeeFunctionQualifier"/>
    <xsl:param name="dutyTaxFeeTypeCoded" select="''"/>
    <xsl:param name="dutyTaxFeeType" select="''"/>
    <xsl:element name="ns0:TAX_2">
      <xsl:element name="TAX01">
        <xsl:value-of select="$dutyTaxFeeFunctionQualifier"/>
      </xsl:element>
      <xsl:if test="normalize-space($dutyTaxFeeTypeCoded)!='' or normalize-space($dutyTaxFeeType)!=''">
        <xsl:element name="ns0:C241_2">
          <xsl:if test="normalize-space($dutyTaxFeeTypeCoded)!=''">
            <xsl:element name="C24101">
              <xsl:value-of select="$dutyTaxFeeTypeCoded"/>
            </xsl:element>
          </xsl:if>
          <xsl:if test="normalize-space($dutyTaxFeeType)!=''">
            <xsl:element name="C24104">
              <xsl:value-of select="$dutyTaxFeeType"/>
            </xsl:element>
          </xsl:if>
        </xsl:element>
      </xsl:if>
    </xsl:element>
  </xsl:template>

  <xsl:template name="WriteFTX_4Raw">
    <xsl:param name="textSubjectQualifier"/>
    <xsl:param name="freeText1"/>
    <xsl:if test="normalize-space($freeText1)!=''">
      <xsl:element name="ns0:FTX_4">
        <xsl:element name="FTX01">
          <xsl:value-of select="$textSubjectQualifier"/>
        </xsl:element>
        <xsl:element name="ns0:C108_4">
          <xsl:element name="C10801">
            <xsl:value-of select="$freeText1"/>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="WriteCSTRaw">
    <xsl:param name="goodsItemNumber" select="''"/>
    <xsl:param name="customsCodeIdentification"/>
    <xsl:param name="codeListQualifier" select="''"/>
    <xsl:element name="ns0:CST">
      <xsl:if test="number($goodsItemNumber)">
        <xsl:element name="CST01">
          <xsl:value-of select="$goodsItemNumber"/>
        </xsl:element>
      </xsl:if>
      <xsl:if test="normalize-space($customsCodeIdentification)!=''">
        <xsl:element name="ns0:C246">
          <xsl:element name="C24601">
            <xsl:value-of select="$customsCodeIdentification"/>
          </xsl:element>
          <xsl:if test="normalize-space($codeListQualifier)!=''">
            <xsl:element name="C24602">
              <xsl:value-of select="$codeListQualifier"/>
            </xsl:element>
          </xsl:if>
        </xsl:element>
      </xsl:if>
    </xsl:element>
  </xsl:template>

  <xsl:template name="WriteMOALoop1SumOfLineAddInfoRaw">
    <xsl:param name="monetaryAmountTypeQualifier"/>
    <xsl:param name="key"/>
    <xsl:variable name="lineAddInfo" select="*[local-name()='CommercialInvoiceLineCollection']
                                                  /*[local-name()='CommercialInvoiceLine']
                                                  /*[local-name()='AddInfoCollection']
                                                  /*[local-name()='AddInfo'][*[local-name()='Key']/text()=$key]"/>
    <xsl:variable name="sumAmount" select="sum($lineAddInfo/*[local-name()='Value'])"/>
    <xsl:call-template name="WriteMOALoop1Raw">
      <xsl:with-param name="monetaryAmountTypeQualifier" select="$monetaryAmountTypeQualifier"/>
      <xsl:with-param name="monetaryAmount" select="$sumAmount"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteMOALoop1SumOfLineCommercialChargeRaw">
    <xsl:param name="monetaryAmountTypeQualifier"/>
    <xsl:param name="chargeTypeCode"/>
    <xsl:param name="chargeSubTypeCode"/>
    <xsl:variable name="lineCommercialCharge" select="*[local-name()='CommercialInvoiceLineCollection']
                                                  /*[local-name()='CommercialInvoiceLine']
                                                  /*[local-name()='CustomsReferenceCollection']
                                                  /*[local-name()='CustomsReference']
                                                 [*[local-name()='Type']/*[local-name()='Code']/text()=$chargeTypeCode and
                                                 *[local-name()='SubType']/*[local-name()='Code']/text()=$chargeSubTypeCode
                                                 and normalize-space(*[local-name()='Reference']) = number(*[local-name()='Reference'])]"/>
    
    <xsl:variable name="sumAmount" select="sum($lineCommercialCharge/*[local-name()='Reference'])"/>
    <xsl:call-template name="WriteMOALoop1Raw">
      <xsl:with-param name="monetaryAmountTypeQualifier" select="$monetaryAmountTypeQualifier"/>
      <xsl:with-param name="monetaryAmount" select="$sumAmount"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteMOALoop1SumOfLineCustomsReferenceRaw">
    <xsl:param name="monetaryAmountTypeQualifier"/>
    <xsl:param name="typeCode"/>
    <xsl:param name="subTypeCode"/>
    <xsl:variable name="lineCustomsReference"
                  select="*[local-name()='CommercialInvoiceLineCollection']
                          /*[local-name()='CommercialInvoiceLine']
                          /*[local-name()='CustomsReferenceCollection']
                          /*[local-name()='CustomsReference'][*[local-name()='Type']/*[local-name()='Code']/text()=$typeCode and *[local-name()='SubType']/*[local-name()='Code']/text()=$subTypeCode]"/>
    <xsl:variable name="sumAmount" select="sum($lineCustomsReference/*[local-name()='Reference'])"/>
    <xsl:call-template name="WriteMOALoop1Raw">
      <xsl:with-param name="monetaryAmountTypeQualifier" select="$monetaryAmountTypeQualifier"/>
      <xsl:with-param name="monetaryAmount" select="$sumAmount"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteMOALoop1FromEntryLineCommercialCharge">
    <xsl:param name="monetaryAmountTypeQualifier"/>
    <xsl:param name="chargeTypeCode"/>
    <xsl:param name="fallbackLevel" select="."/>
    <xsl:variable name="value" select="*[local-name()='EntryLineChargeCollection']
                                                         /*[local-name()='EntryLineCharge'][*[local-name()='Type']/*[local-name()='Code']/text()=$chargeTypeCode]
                                                         /*[local-name()='Amount']"/>
    <xsl:variable name="fallbackValue" select="sum($fallbackLevel/*[local-name()='CommercialChargeCollection']
                                                         /*[local-name()='CommercialCharge'][*[local-name()='ChargeType']/*[local-name()='Code']/text()=$chargeTypeCode]
                                                         /*[local-name()='Amount'])"/>
    <xsl:variable name="amount">
      <xsl:choose>
        <xsl:when test="number($value)">
          <xsl:value-of select="$value"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$fallbackValue"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:call-template name="WriteMOALoop1Raw">
      <xsl:with-param name="monetaryAmountTypeQualifier" select="$monetaryAmountTypeQualifier"/>
      <xsl:with-param name="monetaryAmount" select="$amount"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteMOALoop1FromCommercialChargeRaw">
    <xsl:param name="monetaryAmountTypeQualifier"/>
    <xsl:param name="chargeTypeCode"/>
    <xsl:param name="convertToLocalCurrency" select="'N'"/>
    <xsl:param name="fallbackExchangeRate" select="'1'"/>
    <xsl:variable name="amount">
      <xsl:call-template name="GetCommercialChargeAmountRaw">
        <xsl:with-param name="chargeTypeCode" select="$chargeTypeCode"/>
        <xsl:with-param name="convertToLocalCurrency" select="$convertToLocalCurrency"/>
        <xsl:with-param name="fallbackExchangeRate" select="$fallbackExchangeRate"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:call-template name="WriteMOALoop1Raw">
      <xsl:with-param name="monetaryAmountTypeQualifier" select="$monetaryAmountTypeQualifier"/>
      <xsl:with-param name="monetaryAmount" select="$amount"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteMOALoop1Raw">
    <xsl:param name="monetaryAmountTypeQualifier"/>
    <xsl:param name="monetaryAmount"/>
    <xsl:param name="formatMonetaryAmount" select="'true'"/>
    <xsl:param name="currencyDetailsQualifier" select="''"/>
    <xsl:param name="currencyRateBase" select="''"/>
    <xsl:param name="rateOfExchange" select="''"/>
    <xsl:element name="ns0:MOALoop1">
      <xsl:call-template name="WriteMOA_2Raw">
        <xsl:with-param name="monetaryAmountTypeQualifier" select="$monetaryAmountTypeQualifier"/>
        <xsl:with-param name="monetaryAmount" select="$monetaryAmount"/>
        <xsl:with-param name="formatMonetaryAmount" select="$formatMonetaryAmount"/>
      </xsl:call-template>
      <xsl:call-template name="WriteCUXRaw">
        <xsl:with-param name="currencyDetailsQualifier" select="$currencyDetailsQualifier"/>
        <xsl:with-param name="currencyRateBase" select="$currencyRateBase"/>
        <xsl:with-param name="rateOfExchange" select="$rateOfExchange"/>
      </xsl:call-template>
    </xsl:element>
  </xsl:template>

  <xsl:template name="WriteCUXRaw">
    <xsl:param name="currencyDetailsQualifier"/>
    <xsl:param name="currencyRateBase"/>
    <xsl:param name="rateOfExchange"/>
    <xsl:if test="normalize-space($currencyDetailsQualifier)!=''">
      <xsl:element name="ns0:CUX">
        <xsl:element name="ns0:C504">
          <xsl:element name="C50401">
            <xsl:value-of select="$currencyDetailsQualifier"/>
          </xsl:element>
          <xsl:element name="C50404">
            <xsl:value-of select="$currencyRateBase"/>
          </xsl:element>
        </xsl:element>
        <xsl:if test="normalize-space($rateOfExchange)!=''">
          <xsl:element name="CUX03">
            <xsl:value-of select="number($rateOfExchange)"/>
          </xsl:element>
        </xsl:if>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="WriteMOA_2Raw">
    <xsl:param name="monetaryAmountTypeQualifier"/>
    <xsl:param name="monetaryAmount" select="''"/>
    <xsl:param name="formatMonetaryAmount" select="'true'"/>
    <xsl:if test="normalize-space($monetaryAmountTypeQualifier)!=''">
      <xsl:variable name="monetaryAmountOrZero">
        <xsl:call-template name="NumberOrDefault">
          <xsl:with-param name="number" select="$monetaryAmount"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:element name="ns0:MOA_2">
        <xsl:element name="ns0:C516_2">
          <xsl:element name="C51601">
            <xsl:value-of select="$monetaryAmountTypeQualifier"/>
          </xsl:element>
          <xsl:element name="C51602">
            <xsl:choose>
              <xsl:when test="$formatMonetaryAmount='true'">
                <xsl:call-template name="FormatMOAAmount">
                  <xsl:with-param name="monetaryAmount" select="$monetaryAmountOrZero"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$monetaryAmountOrZero"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <!--WriteDOC Raw 1 END-->

  <!--WriteDOC Raw 2 Begin-->

  <xsl:template name="WriteMEARaw">
    <xsl:param name="measurementPurposeQualifier"/>
    <xsl:param name="measureUnitQualifier" select="''"/>
    <xsl:param name="measurementValue" select="''"/>
    <xsl:if test="normalize-space($measureUnitQualifier)!='' and normalize-space($measurementValue)!=''">
      <xsl:element name="ns0:MEA">
        <xsl:element name="MEA01">
          <xsl:value-of select="$measurementPurposeQualifier"/>
        </xsl:element>
        <xsl:element name="ns0:C174">
          <xsl:element name="C17401">
            <xsl:value-of select="$measureUnitQualifier"/>
          </xsl:element>
          <xsl:element name="C17402">
            <xsl:call-template name="FormatMEAValue">
              <xsl:with-param name="measurementValue" select="$measurementValue"/>
            </xsl:call-template>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="WriteDTM_3Raw">
    <xsl:param name="dateTimePeriodQualifier"/>
    <xsl:param name="dateTimePeriod"/>
    <xsl:param name="dateTimePeriodFormatQualifier"/>
    <xsl:if test="normalize-space($dateTimePeriod)!=''">
      <xsl:element name="ns0:DTM_3">
        <xsl:element name="ns0:C507_3">
          <xsl:element name="C50701">
            <xsl:value-of select="$dateTimePeriodQualifier"/>
          </xsl:element>
          <xsl:element name="C50702">
            <xsl:variable name="dateTimePeriodFormat">
              <xsl:choose>
                <xsl:when test="$dateTimePeriodFormatQualifier='401'">HHmm</xsl:when>
                <xsl:otherwise>yyMMdd</xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <xsl:value-of select="ScriptNS1:ConvertXmlDateString($dateTimePeriod, $dateTimePeriodFormat)"/>
          </xsl:element>
          <xsl:element name="C50703">
            <xsl:value-of select="$dateTimePeriodFormatQualifier"/>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="WriteLOC_3Raw">
    <xsl:param name="placeLocationQualifier"/>
    <xsl:param name="placeLocationIdentification"/>
    <xsl:param name="placeLocation"/>
    <xsl:element name="ns0:LOC_3">
      <xsl:element name="LOC01">
        <xsl:value-of select="$placeLocationQualifier"/>
      </xsl:element>
      <xsl:if test="normalize-space($placeLocationIdentification)!='' or normalize-space($placeLocation)!=''">
        <xsl:element name="ns0:C517_3">
          <xsl:if test="normalize-space($placeLocationIdentification)!=''">
            <xsl:element name="C51701">
              <xsl:value-of select="$placeLocationIdentification"/>
            </xsl:element>
          </xsl:if>
          <xsl:if test="normalize-space($placeLocation)!=''">
            <xsl:element name="C51704">
              <xsl:value-of select="$placeLocation"/>
            </xsl:element>
          </xsl:if>
        </xsl:element>
      </xsl:if>
    </xsl:element>
  </xsl:template>

  <xsl:template name="WriteTDT_2Raw">
    <xsl:param name="transportStageQualifier"/>
    <xsl:param name="modeOfTransportCoded"/>
    <xsl:element name="ns0:TDT_2">
      <xsl:element name="TDT01">
        <xsl:value-of select="$transportStageQualifier"/>
      </xsl:element>
      <xsl:element name="ns0:C220_2">
        <xsl:element name="C22001">
          <xsl:value-of select="$modeOfTransportCoded"/>
        </xsl:element>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template name="WritePCIRaw">
    <xsl:param name="markingInstructionsCoded"/>
    <xsl:param name="shippingMarks"/>
    <xsl:element name="ns0:PCI">
      <xsl:element name="PCI01">
        <xsl:value-of select="$markingInstructionsCoded"/>
      </xsl:element>
      <xsl:element name="ns0:C210">
        <xsl:element name="C21001">
          <xsl:value-of select="$shippingMarks"/>
        </xsl:element>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template name="WriteRFF_3Raw">
    <xsl:param name="referenceQualifier"/>
    <xsl:param name="referenceNumber"/>
    <xsl:param name="lineNumber" select="''"/>
    <xsl:param name="referenceVersionNumber" select="''"/>
    <xsl:if test="normalize-space($referenceNumber)!=''">
      <xsl:element name="ns0:RFF_3">
        <xsl:element name="ns0:C506_3">
          <xsl:element name="C50601">
            <xsl:value-of select="$referenceQualifier"/>
          </xsl:element>
          <xsl:element name="C50602">
            <xsl:value-of select="$referenceNumber"/>
          </xsl:element>
          <xsl:if test="normalize-space($lineNumber)!=''">
            <xsl:element name="C50603">
              <xsl:value-of select="$lineNumber"/>
            </xsl:element>
          </xsl:if>
          <xsl:if test="normalize-space($referenceVersionNumber)!=''">
            <xsl:element name="C50604">
              <xsl:value-of select="$referenceVersionNumber"/>
            </xsl:element>
          </xsl:if>
        </xsl:element>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="WritePACRaw">
    <xsl:param name="numberOfPackages"/>
    <xsl:if test="normalize-space($numberOfPackages)!=''">
      <xsl:element name="ns0:PAC">
        <xsl:element name="PAC01">
          <xsl:value-of select="$numberOfPackages"/>
        </xsl:element>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="WriteDOCRaw">
    <xsl:param name="documentMessageNameCoded"/>
    <xsl:param name="documentMessageName" select="''"/>
    <xsl:param name="documentMessageNumber" select="''"/>
    <xsl:element name="ns0:DOC">
      <xsl:element name="ns0:C002_2">
        <xsl:element name="C00201">
          <xsl:value-of select="$documentMessageNameCoded"/>
        </xsl:element>
        <xsl:if test="normalize-space($documentMessageName)!=''">
          <xsl:element name="C00204">
            <xsl:value-of select="$documentMessageName"/>
          </xsl:element>
        </xsl:if>
      </xsl:element>
      <xsl:if test="normalize-space($documentMessageNumber)!=''">
        <xsl:element name="ns0:C503">
          <xsl:element name="C50301">
            <xsl:value-of select="$documentMessageNumber"/>
          </xsl:element>
        </xsl:element>
      </xsl:if>
    </xsl:element>
  </xsl:template>

  <xsl:template name="GetNoteTextFallbackCustomizedField">
    <xsl:param name="noteDescription"/>
    <xsl:param name="noteParentLevel" select="."/>
    <xsl:param name="customizedFieldKey"/>
    <xsl:param name="customizedFieldParentLevel" select="."/>
    <xsl:param name="invoiceNumber"/>
    <xsl:param name="customizedFieldValueIndex"/>
    <xsl:variable name="noteText">
      <xsl:call-template name="GetNoteTextRaw">
        <xsl:with-param name="description" select="$noteDescription"/>
        <xsl:with-param name="parentLevel" select="$noteParentLevel"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="value">
      <xsl:choose>
        <xsl:when test="normalize-space($noteText)!=''">
          <xsl:value-of select="$noteText"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:variable name="invoiceNumberAndInvoiceLocationList">
            <xsl:call-template name="GetCustomizedFieldRaw">
              <xsl:with-param name="key" select="$customizedFieldKey"/>
              <xsl:with-param name="parentLevel" select="$customizedFieldParentLevel"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:value-of select="userCSharp:GetValueOfInvoiceNumberAndValueList($invoiceNumberAndInvoiceLocationList,$invoiceNumber,$customizedFieldValueIndex)"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select="$value"/>
  </xsl:template>
  
  <xsl:template name="GetNoteTextRaw">
    <xsl:param name="description"/>
    <xsl:param name="parentLevel" select="."/>
    <xsl:variable name="noteText" select="$parentLevel/*[local-name()='NoteCollection']
                          /*[local-name()='Note'][*[local-name()='Description']/text()=$description]
                          /*[local-name()='NoteText']/text()"/>
    <xsl:value-of select="$noteText"/>
  </xsl:template>
  
  <xsl:template name="GetCustomizedFieldRaw">
    <xsl:param name="key"/>
    <xsl:param name="parentLevel" select="."/>
    <xsl:variable name="value" select="$parentLevel/*[local-name()='CustomizedFieldCollection']
                          /*[local-name()='CustomizedField'][*[local-name()='Key']/text()=$key]
                          /*[local-name()='Value']/text()"/>
    <xsl:value-of select="$value"/>
  </xsl:template>

  <xsl:template name="GetCommercialChargeAmountRaw">
    <xsl:param name="chargeTypeCode"/>
    <xsl:param name="convertToLocalCurrency" select="'N'"/>
    <xsl:param name="parentLevel" select="."/>
    <xsl:param name="fallbackExchangeRate" select="'1'"/>
    <xsl:variable name="commercialCharge" select="$parentLevel/*[local-name()='CommercialChargeCollection']
                                                  /*[local-name()='CommercialCharge'][*[local-name()='ChargeType']/*[local-name()='Code']/text()=$chargeTypeCode]"/>
    <xsl:variable name="amount">
      <xsl:choose>
        <xsl:when test="$convertToLocalCurrency='N'">
          <xsl:value-of select="$commercialCharge/*[local-name()='Amount']/text()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:variable name="exchangeRate">
            <xsl:call-template name="GetExchangeRateRaw">
              <xsl:with-param name="elementName" select="'AgreedExchangeRate'"/>
              <xsl:with-param name="fallbackElementName" select="'LandedCostExchangeRate'"/>
              <xsl:with-param name="parentLevel" select="$commercialCharge"/>
              <xsl:with-param name="default" select="$fallbackExchangeRate"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:call-template name="ConvertAmountToLocalCurrency">
            <xsl:with-param name="foreignAmount" select="$commercialCharge/*[local-name()='Amount']/text()"/>
            <xsl:with-param name="exchangeRate" select="$exchangeRate"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="amountOrZero">
      <xsl:call-template name="NumberOrDefault">
        <xsl:with-param name="number" select="$amount"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:value-of select="$amountOrZero"/>
  </xsl:template>

  <xsl:template name="GetAddInfoGroupAddInfoValueRaw">
    <xsl:param name="addInfoGroupTypeCode"/>
    <xsl:param name="key"/>
    <xsl:variable name="value" select="*[local-name()='AddInfoGroupCollection']
                                        /*[local-name()='AddInfoGroup'][*[local-name()='Type']/*[local-name()='Code']/text()=$addInfoGroupTypeCode]
                                        /*[local-name()='AddInfoCollection']
                                        /*[local-name()='AddInfo'][*[local-name()='Key']/text()=$key]
                                        /*[local-name()='Value']/text()"/>
    <xsl:value-of select="$value"/>
  </xsl:template>

  <xsl:template name="GetSumAddinfoValueRaw">
    <xsl:param name="key"/>
    <xsl:param name="fallbackLevel" select="."/>
    <xsl:variable name="value" select="sum(*[local-name()='AddInfoCollection']
                                        /*[local-name()='AddInfo'][*[local-name()='Key']/text()=$key]
                                        /*[local-name()='Value'])"/>
    <xsl:variable name="fallbackValue" select="sum($fallbackLevel/*[local-name()='AddInfoCollection']
                                        /*[local-name()='AddInfo'][*[local-name()='Key']/text()=$key]
                                        /*[local-name()='Value'])"/>
    <xsl:choose>
      <xsl:when test="number($value)">
        <xsl:value-of select="$value"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:choose>
          <xsl:when test="number($fallbackValue)">
            <xsl:value-of select="$fallbackValue"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="'0'"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="GetAddinfoValueRaw">
    <xsl:param name="key"/>
    <xsl:param name="fallbackKey" select="''"/>
    <xsl:param name="fallbackLevel" select="."/>
    <xsl:param name="fallbackLevel1" select="."/>
    <xsl:param name="fallbackLevel2" select="."/>
    <xsl:variable name="value" select="*[local-name()='AddInfoCollection']
                                        /*[local-name()='AddInfo'][*[local-name()='Key']/text()=$key]
                                        /*[local-name()='Value']/text()"/>
    <xsl:variable name="fallbackValueFromFallbackKey" select="*[local-name()='AddInfoCollection']
                                                /*[local-name()='AddInfo'][*[local-name()='Key']/text()=$fallbackKey]
                                                /*[local-name()='Value']/text()"/>
    <xsl:variable name="fallbackValue" select="$fallbackLevel/*[local-name()='AddInfoCollection']
                                                /*[local-name()='AddInfo'][*[local-name()='Key']/text()=$key]
                                                /*[local-name()='Value']/text()"/>
    <xsl:variable name="fallbackValue1" select="$fallbackLevel1/*[local-name()='AddInfoCollection']
                                                /*[local-name()='AddInfo'][*[local-name()='Key']/text()=$key]
                                                /*[local-name()='Value']/text()"/>
    <xsl:variable name="fallbackValue2" select="$fallbackLevel2/*[local-name()='AddInfoCollection']
                                                /*[local-name()='AddInfo'][*[local-name()='Key']/text()=$key]
                                                /*[local-name()='Value']/text()"/>
    <xsl:choose>
      <xsl:when test="normalize-space($value)!=''">
        <xsl:value-of select="$value"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:choose>
          <xsl:when test="normalize-space($fallbackValueFromFallbackKey)!=''">
            <xsl:value-of select="$fallbackValueFromFallbackKey"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:choose>
              <xsl:when test="normalize-space($fallbackValue)!=''">
                <xsl:value-of select="$fallbackValue"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:choose>
                  <xsl:when test="normalize-space($fallbackValue1)!=''">
                    <xsl:value-of select="$fallbackValue1"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="$fallbackValue2"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="GetRegistrationNumberOfOrganizationAddress">
    <xsl:param name="addressType"/>
    <xsl:param name="registrationNumberTypeCode"/>
    <xsl:param name="fallbackLevel" select="."/>
    <xsl:param name="fallbackLevel1" select="."/>
    <xsl:param name="fallbackLevel2" select="."/>
    <xsl:variable name="value" select="*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][*[local-name()='AddressType']/text()=$addressType]
                                                      /*[local-name()='RegistrationNumberCollection']
                                                      /*[local-name()='RegistrationNumber'][*[local-name()='CountryOfIssue']/*[local-name()='Code']/text()='US' and *[local-name()='Type']/*[local-name()='Code']/text()=$registrationNumberTypeCode]
                                                      /*[local-name()='Value']/text()"/>
    <xsl:variable name="fallbackValue" select="$fallbackLevel/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][*[local-name()='AddressType']/text()=$addressType]
                                                      /*[local-name()='RegistrationNumberCollection']
                                                      /*[local-name()='RegistrationNumber'][*[local-name()='CountryOfIssue']/*[local-name()='Code']/text()='US' and *[local-name()='Type']/*[local-name()='Code']/text()=$registrationNumberTypeCode]
                                                      /*[local-name()='Value']/text()"/>
    <xsl:variable name="fallbackValue1" select="$fallbackLevel1/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][*[local-name()='AddressType']/text()=$addressType]
                                                      /*[local-name()='RegistrationNumberCollection']
                                                      /*[local-name()='RegistrationNumber'][*[local-name()='CountryOfIssue']/*[local-name()='Code']/text()='US' and *[local-name()='Type']/*[local-name()='Code']/text()=$registrationNumberTypeCode]
                                                      /*[local-name()='Value']/text()"/>
    <xsl:variable name="fallbackValue2" select="$fallbackLevel2/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][*[local-name()='AddressType']/text()=$addressType]
                                                      /*[local-name()='RegistrationNumberCollection']
                                                      /*[local-name()='RegistrationNumber'][*[local-name()='CountryOfIssue']/*[local-name()='Code']/text()='US' and *[local-name()='Type']/*[local-name()='Code']/text()=$registrationNumberTypeCode]
                                                      /*[local-name()='Value']/text()"/>
    <xsl:choose>
      <xsl:when test="normalize-space($value)!=''">
        <xsl:value-of select="$value"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:choose>
          <xsl:when test="normalize-space($fallbackValue)!=''">
            <xsl:value-of select="$fallbackValue"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:choose>
              <xsl:when test="normalize-space($fallbackValue1)!=''">
                <xsl:value-of select="$fallbackValue1"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$fallbackValue2"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="GetExchangeRateRaw">
    <xsl:param name="elementName"/>
    <xsl:param name="fallbackElementName" select="''"/>
    <xsl:param name="parentLevel" select="."/>
    <xsl:param name="default" select="'1'"/>
    <xsl:variable name="value">
      <xsl:call-template name="NumberOrDefault">
        <xsl:with-param name="number" select="$parentLevel/*[local-name()=$elementName]/text()"/>
      </xsl:call-template>  
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="$value!=0">
        <xsl:value-of select="$value"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:variable name="fallbackValue">
          <xsl:call-template name="NumberOrDefault">
            <xsl:with-param name="number" select="$parentLevel/*[local-name()=$fallbackElementName]/text()"/>
            <xsl:with-param name="default" select="$default"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:value-of select="$fallbackValue"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!--WriteDOC Raw 2 END-->

  <xsl:template name="WriteTAXLoop1">
    <xsl:variable name="totalDuty">
      <xsl:call-template name="SumEntryLineChargeRaw">
        <xsl:with-param name="chargeTypeCode" select="'DTY'"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="totalAntiDumpingDuty">
      <xsl:call-template name="SumEntryLineChargeRaw">
        <xsl:with-param name="chargeTypeCode" select="'ADD'"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="totalCounterVailingDuty">
      <xsl:call-template name="SumEntryLineChargeRaw">
        <xsl:with-param name="chargeTypeCode" select="'CVD'"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:call-template name="WriteTAXLoop1Raw">
      <xsl:with-param name="dutyTaxFeeFunctionQualifier" select="'5'"/>
      <xsl:with-param name="monetaryAmountTypeQualifier1" select="'55'"/>
      <xsl:with-param name="monetaryAmount1" select="$totalDuty"/>
      <xsl:with-param name="monetaryAmountTypeQualifier2" select="'290'"/>
      <xsl:with-param name="monetaryAmount2" select="$totalAntiDumpingDuty"/>
      <xsl:with-param name="monetaryAmountTypeQualifier3" select="'161'"/>
      <xsl:with-param name="monetaryAmount3" select="$totalCounterVailingDuty"/>
    </xsl:call-template>
    <xsl:variable name="totalOverseasFreight" select="*[local-name()='Shipment']/*[local-name()='CommercialInfo']/*[local-name()='CommercialChargeCollection']
                                                      /*[local-name()='CommercialCharge'][*[local-name()='ChargeType']/*[local-name()='Code']/text()='OFT']
                                                      /*[local-name()='Amount']"/>
    <xsl:variable name="totalMPF">
      <xsl:call-template name="GetTotalMPF"/>
    </xsl:variable>
    <xsl:call-template name="WriteTAXLoop1Raw">
      <xsl:with-param name="dutyTaxFeeFunctionQualifier" select="'6'"/>
      <xsl:with-param name="monetaryAmountTypeQualifier1" select="'313'"/>
      <xsl:with-param name="monetaryAmount1" select="$totalOverseasFreight"/>
      <xsl:with-param name="monetaryAmountTypeQualifier2" select="'72'"/>
      <xsl:with-param name="monetaryAmount2" select="$totalMPF"/>
      <xsl:with-param name="monetaryAmountTypeQualifier3" select="'ZZZ'"/>
      <xsl:with-param name="monetaryAmount3" select="$totalMPF"/>
    </xsl:call-template>
    <xsl:variable name="totalHMF">
      <xsl:call-template name="SumEntryHeaderChargeRaw">
        <xsl:with-param name="chargeTypeCode" select="'501'"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:call-template name="WriteTAXLoop1Raw">
      <xsl:with-param name="dutyTaxFeeFunctionQualifier" select="'7'"/>
      <xsl:with-param name="monetaryAmountTypeQualifier1" select="'282'"/>
      <xsl:with-param name="monetaryAmount1" select="$totalHMF"/>
    </xsl:call-template>
  </xsl:template>

  <!--WriteTAX Raw BEGIN-->

  <xsl:template name="WriteTAXLoop1Raw">
    <xsl:param name="dutyTaxFeeFunctionQualifier"/>
    <xsl:param name="monetaryAmountTypeQualifier1"/>
    <xsl:param name="monetaryAmount1"/>
    <xsl:param name="monetaryAmountTypeQualifier2" select="''"/>
    <xsl:param name="monetaryAmount2" select="''"/>
    <xsl:param name="monetaryAmountTypeQualifier3" select="''"/>
    <xsl:param name="monetaryAmount3" select="''"/>
    <xsl:if test="normalize-space($dutyTaxFeeFunctionQualifier)!=''">
      <xsl:element name="ns0:TAXLoop1">
        <xsl:call-template name="WriteTAXRaw">
          <xsl:with-param name="dutyTaxFeeFunctionQualifier" select="$dutyTaxFeeFunctionQualifier"/>
        </xsl:call-template>
        <xsl:call-template name="WriteMOARaw">
          <xsl:with-param name="monetaryAmountTypeQualifier" select="$monetaryAmountTypeQualifier1"/>
          <xsl:with-param name="monetaryAmount" select="$monetaryAmount1"/>
        </xsl:call-template>
        <xsl:call-template name="WriteMOARaw">
          <xsl:with-param name="monetaryAmountTypeQualifier" select="$monetaryAmountTypeQualifier2"/>
          <xsl:with-param name="monetaryAmount" select="$monetaryAmount2"/>
        </xsl:call-template>
        <xsl:call-template name="WriteMOARaw">
          <xsl:with-param name="monetaryAmountTypeQualifier" select="$monetaryAmountTypeQualifier3"/>
          <xsl:with-param name="monetaryAmount" select="$monetaryAmount3"/>
        </xsl:call-template>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="GetTotalMPF">
    <xsl:param name="parentLevel" select="."/>
    <xsl:variable name="charge499">
      <xsl:call-template name="SumEntryHeaderChargeRaw">
        <xsl:with-param name="chargeTypeCode" select="'499'"/>
        <xsl:with-param name="parentLevel" select="$parentLevel"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="charge311">
      <xsl:call-template name="SumEntryHeaderChargeRaw">
        <xsl:with-param name="chargeTypeCode" select="'311'"/>
        <xsl:with-param name="parentLevel" select="$parentLevel"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:value-of select="$charge499+$charge311"/>
  </xsl:template>
  
  <xsl:template name="SumEntryHeaderChargeRaw">
    <xsl:param name="chargeTypeCode"/>
    <xsl:param name="parentLevel" select="."/>
    <xsl:variable name="total" select="sum($parentLevel/*[local-name()='Shipment']/*[local-name()='EntryHeaderCollection']
                      /*[local-name()='EntryHeader'][*[local-name()='Type']/*[local-name()='Code']/text()='ENS']
                      /*[local-name()='EntryHeaderChargeCollection']
                      /*[local-name()='EntryHeaderCharge'][*[local-name()='Type']/*[local-name()='Code']/text()=$chargeTypeCode]
                      /*[local-name()='Amount'])"/>
    <xsl:variable name="totalOrZero">
      <xsl:call-template name="NumberOrDefault">
        <xsl:with-param name="number" select="$total"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:value-of select="$totalOrZero"/>
  </xsl:template>

  <xsl:template name="SumEntryLineChargeRaw">
    <xsl:param name="chargeTypeCode"/>
    <xsl:param name="parentLevel" select="."/>
    <xsl:variable name="total" select="sum($parentLevel/*[local-name()='Shipment']/*[local-name()='EntryHeaderCollection']
                      /*[local-name()='EntryHeader'][*[local-name()='Type']/*[local-name()='Code']/text()='ENS']
                      /*[local-name()='EntryLineCollection']/*[local-name()='EntryLine']/*[local-name()='EntryLineChargeCollection']
                      /*[local-name()='EntryLineCharge'][*[local-name()='Type']/*[local-name()='Code']/text()=$chargeTypeCode]
                      /*[local-name()='Amount'])"/>
    <xsl:variable name="totalOrZero">
      <xsl:call-template name="NumberOrDefault">
        <xsl:with-param name="number" select="$total"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:value-of select="$totalOrZero"/>
  </xsl:template>

  <xsl:template name="WriteMOARaw">
    <xsl:param name="monetaryAmountTypeQualifier"/>
    <xsl:param name="monetaryAmount"/>
    <xsl:if test="normalize-space($monetaryAmountTypeQualifier)!=''">
      <xsl:variable name="monetaryAmountOrZero">
        <xsl:call-template name="NumberOrDefault">
          <xsl:with-param name="number" select="$monetaryAmount"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:element name="ns0:MOA">
        <xsl:element name="ns0:C516">
          <xsl:element name="C51601">
            <xsl:value-of select="$monetaryAmountTypeQualifier"/>
          </xsl:element>
          <xsl:element name="C51602">
            <xsl:call-template name="FormatMOAAmount">
              <xsl:with-param name="monetaryAmount" select="$monetaryAmountOrZero"/>
            </xsl:call-template>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="WriteTAXRaw">
    <xsl:param name="dutyTaxFeeFunctionQualifier"/>
    <xsl:if test="normalize-space($dutyTaxFeeFunctionQualifier)!=''">
      <xsl:element name="ns0:TAX">
        <xsl:element name="TAX01">
          <xsl:value-of select="$dutyTaxFeeFunctionQualifier"/>
        </xsl:element>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <!--WriteTAX Raw END-->

  <!--Format/Safe Number BEGIN-->

  <xsl:template name="NumberOrDefault">
    <xsl:param name="number"/>
    <xsl:param name="default" select="'0'"/>
    <xsl:choose>
      <xsl:when test="number($number)  and $number!='Infinity'">
        <xsl:value-of select="number($number)"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$default"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="FormatMEAValue">
    <xsl:param name="measurementValue" select="''"/>
    <xsl:value-of select="format-number(number($measurementValue),'0.00')"/>
  </xsl:template>

  <xsl:template name="FormatMOAAmount">
    <xsl:param name="monetaryAmount"/>
    <xsl:value-of select="format-number(number($monetaryAmount),'0.##')"/>
  </xsl:template>

  <!--Format/Safe Number END-->

  <xsl:template name="WriteRFFLoop1_RFFOnlyFromAddInfo">
    <xsl:param name="key"/>
    <xsl:param name="referenceQualifier"/>
    <xsl:call-template name="WriteRFFLoop1_RFFOnlyFromAddInfoRaw">
      <xsl:with-param name="key" select="$key"/>
      <xsl:with-param name="referenceQualifier" select="$referenceQualifier"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteRFFLoop1_RFFOnlyFromCustomsDeclarationJobNumber">
    <xsl:variable name="customsDeclarationJobNumber" select="/*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='DataContext']/*[local-name()='DataSourceCollection']
                                                /*[local-name()='DataSource'][*[local-name()='Type']/text()='CustomsDeclaration']
                                                /*[local-name()='Key']/text()"/>
    <xsl:call-template name="WriteRFFLoop1_RFFOnlyRaw">
      <xsl:with-param name="referenceQualifier" select="'ADU'"/>
      <xsl:with-param name="referenceNumber" select="$customsDeclarationJobNumber"/>
      <xsl:with-param name="referenceVersionNumber" select="'CV'"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteRFFLoop1_RFFOnlyFromMWB">
    <xsl:variable name="waybillNumber" select="*[local-name()='Shipment']/*[local-name()='AdditionalBillCollection']
                                                /*[local-name()='AdditionalBill'][*[local-name()='BillType']/*[local-name()='Code']/text()='MWB']
                                                /*[local-name()='BillNumber']/text()"/>
    <xsl:call-template name="WriteRFFLoop1_RFFOnlyRaw">
      <xsl:with-param name="referenceQualifier" select="'MWB'"/>
      <xsl:with-param name="referenceNumber" select="$waybillNumber"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteRFFLoop1_RFFOnlyFromITNumber">
    <xsl:variable name="ITNumber" select="*[local-name()='Shipment']/*[local-name()='AdditionalBillCollection']
                                          /*[local-name()='AdditionalBill']/*[local-name()='AddInfoGroupCollection']
                                          /*[local-name()='AddInfoGroup'][*[local-name()='Type']/*[local-name()='Code']/text()='ITN']
                                          /*[local-name()='AddInfoCollection']
                                          /*[local-name()='AddInfo'][*[local-name()='Key']/text()='ITNumber']
                                          /*[local-name()='Value']/text()"/>
    <xsl:call-template name="WriteRFFLoop1_RFFOnlyRaw">
      <xsl:with-param name="referenceQualifier" select="'ZZZ'"/>
      <xsl:with-param name="referenceNumber" select="$ITNumber"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteRFFLoop1_RFFOnlyFromEntryNumberINB">
    <xsl:variable name="entryNumberINB">
      <xsl:call-template name="GetEntryNumberRaw">
        <xsl:with-param name="typeCode" select="'INB'"/>
        <xsl:with-param name="parentLevel" select="*[local-name()='Shipment']"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:call-template name="WriteRFFLoop1_RFFOnlyRaw">
      <xsl:with-param name="referenceQualifier" select="'WE'"/>
      <xsl:with-param name="referenceNumber" select="$entryNumberINB"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteRFFLoop1_RFFOnly">
    <xsl:param name="referenceQualifier"/>
    <xsl:param name="referenceNumber"/>
    <xsl:call-template name="WriteRFFLoop1_RFFOnlyRaw">
      <xsl:with-param name="referenceQualifier" select="$referenceQualifier"/>
      <xsl:with-param name="referenceNumber" select="$referenceNumber"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteRFFLoop1_RFFOnlyFromAddInfoRaw">
    <xsl:param name="key"/>
    <xsl:param name="referenceQualifier"/>
    <xsl:variable name="value" select="*[local-name()='Shipment']/*[local-name()='AddInfoCollection']
                                                /*[local-name()='AddInfo'][*[local-name()='Key']/text()=$key]
                                                /*[local-name()='Value']/text()"/>
    <xsl:call-template name="WriteRFFLoop1_RFFOnlyRaw">
      <xsl:with-param name="referenceQualifier" select="$referenceQualifier"/>
      <xsl:with-param name="referenceNumber" select="$value"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteRFFLoop1_RFFOnlyRaw">
    <xsl:param name="referenceQualifier"/>
    <xsl:param name="referenceNumber"/>
    <xsl:param name="lineNumber" select="''"/>
    <xsl:param name="referenceVersionNumber" select="''"/>
    <xsl:if test="normalize-space($referenceQualifier)!='' and normalize-space($referenceNumber)!=''">
      <xsl:element name="ns0:RFFLoop1">
        <xsl:call-template name="WriteRFFRaw">
          <xsl:with-param name="referenceQualifier" select="$referenceQualifier"/>
          <xsl:with-param name="referenceNumber" select="$referenceNumber"/>
          <xsl:with-param name="lineNumber" select="$lineNumber"/>
          <xsl:with-param name="referenceVersionNumber" select="$referenceVersionNumber"/>
        </xsl:call-template>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="WriteRFFLoop1">
    <xsl:param name="referenceQualifier"/>
    <xsl:element name="ns0:RFFLoop1">
      <xsl:variable name="referenceNumber" select="*[local-name()='Shipment']/*[local-name()='AdditionalBillCollection']
                                                /*[local-name()='AdditionalBill'][*[local-name()='BillType']/*[local-name()='Code']/text()='HWB']
                                                /*[local-name()='BillNumber']/text()"/>
      <xsl:call-template name="WriteRFFRaw">
        <xsl:with-param name="referenceQualifier" select="$referenceQualifier"/>
        <xsl:with-param name="referenceNumber" select="$referenceNumber"/>
      </xsl:call-template>
      <xsl:call-template name="WriteDTMFromDateCollectionRaw">
        <xsl:with-param name="type" select="'DischargeDate'"/>
        <xsl:with-param name="dateTimeQualifier" select="'252'"/>
      </xsl:call-template>
      <xsl:call-template name="WriteDTMFromDateCollectionRaw">
        <xsl:with-param name="type" select="'LoadingDate'"/>
        <xsl:with-param name="dateTimeQualifier" select="'110'"/>
      </xsl:call-template>
      <xsl:call-template name="WriteDTMFromAddInfoRaw">
        <xsl:with-param name="key" select="'EntryDate'"/>
        <xsl:with-param name="dateTimeQualifier" select="'178'"/>
      </xsl:call-template>
      <xsl:call-template name="WriteDTMFromDateCollectionRaw">
        <xsl:with-param name="type" select="'EntryAuthorisation'"/>
        <xsl:with-param name="dateTimeQualifier" select="'261'"/>
      </xsl:call-template>
      <xsl:call-template name="WriteDTMFromAddInfoRaw">
        <xsl:with-param name="key" select="'PresentationDate'"/>
        <xsl:with-param name="dateTimeQualifier" select="'402'"/>
      </xsl:call-template>
      <xsl:call-template name="WriteDTMFromEntryHeaderRaw">
        <xsl:with-param name="headerTypeCode" select="'ENS'"/>
        <xsl:with-param name="dateElementName" select="'EntrySubmittedDate'"/>
        <xsl:with-param name="dateTimeQualifier" select="'257'"/>
      </xsl:call-template>
      <xsl:call-template name="WriteDTMFromAddInfoRaw">
        <xsl:with-param name="key" select="'EntryDate'"/>
        <xsl:with-param name="dateTimeQualifier" select="'143'"/>
      </xsl:call-template>
      <xsl:call-template name="WriteDTMFromDateCollectionRaw">
        <xsl:with-param name="type" select="'DischargeDate'"/>
        <xsl:with-param name="dateTimeQualifier" select="'151'"/>
      </xsl:call-template>
      <xsl:call-template name="WriteLOC_2FromAddInfoRaw">
        <xsl:with-param name="addInfoKey" select="'SchDEntry'"/>
        <xsl:with-param name="placeLocationQualifier" select="'79'"/>
        <xsl:with-param name="locationCodeListQualifier" select="'139'"/>
      </xsl:call-template>
      <xsl:call-template name="WriteLOC_2FromAddInfoRaw">
        <xsl:with-param name="addInfoKey" select="'DestinationState'"/>
        <xsl:with-param name="placeLocationQualifier" select="'47'"/>
      </xsl:call-template>
    </xsl:element>
  </xsl:template>

  <xsl:template name="WriteLOC_2FromAddInfoRaw">
    <xsl:param name="addInfoKey"/>
    <xsl:param name="placeLocationQualifier"/>
    <xsl:param name="locationCodeListQualifier" select="''"/>
    <xsl:variable name="placeLocationIdentifier" select="*[local-name()='Shipment']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][*[local-name()='Key']/text()=$addInfoKey]/*[local-name()='Value']/text()"/>
    <xsl:call-template name="WriteLOC_2Raw">
      <xsl:with-param name="placeLocationQualifier" select="$placeLocationQualifier"/>
      <xsl:with-param name="placeLocationIdentifier" select="$placeLocationIdentifier"/>
      <xsl:with-param name="locationCodeListQualifier" select="$locationCodeListQualifier"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteLOC_2Raw">
    <xsl:param name="placeLocationQualifier"/>
    <xsl:param name="placeLocationIdentifier" select="''"/>
    <xsl:param name="locationCodeListQualifier" select="''"/>
    <xsl:if test="normalize-space($placeLocationQualifier)!=''">
      <xsl:element name="ns0:LOC_2">
        <xsl:element name="LOC01">
          <xsl:value-of select="$placeLocationQualifier" />
        </xsl:element>
        <xsl:if test="normalize-space($placeLocationIdentifier)!='' or normalize-space($locationCodeListQualifier)!=''">
          <xsl:element name="ns0:C517_2">
            <xsl:if test="normalize-space($placeLocationIdentifier)!=''">
              <xsl:element name="C51701">
                <xsl:value-of select="$placeLocationIdentifier" />
              </xsl:element>
            </xsl:if>
            <xsl:if test="normalize-space($locationCodeListQualifier)!=''">
              <xsl:element name="C51702">
                <xsl:value-of select="$locationCodeListQualifier" />
              </xsl:element>
            </xsl:if>
          </xsl:element>
        </xsl:if>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <!--<xsl:template name="WriteDTMFromDataContextRaw">
    <xsl:param name="eventTypeCode"/>
    <xsl:param name="dateElementName"/>
    <xsl:param name="dateTimeQualifier"/>
    <xsl:variable name="value" select="*[local-name()='Shipment']
                      /*[local-name()='DataContext'][*[local-name()='EventType']/*[local-name()='Code']/text()=$eventTypeCode]
                      /*[local-name()=$dateElementName]/text()"/>
    <xsl:call-template name="WriteDTMRaw">
      <xsl:with-param name="dateTimeQualifier" select="$dateTimeQualifier"/>
      <xsl:with-param name="dateTimePeriod" select="$value"/>
    </xsl:call-template>
  </xsl:template>-->

  <xsl:template name="WriteDTMFromEntryHeaderRaw">
    <xsl:param name="headerTypeCode"/>
    <xsl:param name="dateElementName"/>
    <xsl:param name="dateTimeQualifier"/>
    <xsl:variable name="value" select="*[local-name()='Shipment']/*[local-name()='EntryHeaderCollection']
                      /*[local-name()='EntryHeader'][*[local-name()='Type']/*[local-name()='Code']/text()=$headerTypeCode]
                      /*[local-name()=$dateElementName]/text()"/>
    <xsl:call-template name="WriteDTMRaw">
      <xsl:with-param name="dateTimeQualifier" select="$dateTimeQualifier"/>
      <xsl:with-param name="dateTimePeriod" select="$value"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteDTMFromAddInfoRaw">
    <xsl:param name="key"/>
    <xsl:param name="dateTimeQualifier"/>
    <xsl:variable name="value" select="*[local-name()='Shipment']/*[local-name()='AddInfoCollection']
                      /*[local-name()='AddInfo'][*[local-name()='Key']/text()=$key]
                      /*[local-name()='Value']/text()"/>
    <xsl:call-template name="WriteDTMRaw">
      <xsl:with-param name="dateTimeQualifier" select="$dateTimeQualifier"/>
      <xsl:with-param name="dateTimePeriod" select="$value"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteDTMFromDateCollectionRaw">
    <xsl:param name="type"/>
    <xsl:param name="dateTimeQualifier"/>
    <xsl:variable name="value" select="*[local-name()='Shipment']/*[local-name()='DateCollection']
                      /*[local-name()='Date'][*[local-name()='Type']/text()=$type]
                      /*[local-name()='Value']/text()"/>
    <xsl:call-template name="WriteDTMRaw">
      <xsl:with-param name="dateTimeQualifier" select="$dateTimeQualifier"/>
      <xsl:with-param name="dateTimePeriod" select="$value"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteDTMRaw">
    <xsl:param name="dateTimeQualifier"/>
    <xsl:param name="dateTimePeriod"/>
    <xsl:if test="normalize-space($dateTimePeriod)!=''">
      <xsl:element name="ns0:DTM_2">
        <xsl:element name="ns0:C507_2">
          <xsl:element name="C50701">
            <xsl:value-of select="$dateTimeQualifier"/>
          </xsl:element>
          <xsl:element name="C50702">
            <xsl:value-of select="ScriptNS1:ConvertXmlDateString($dateTimePeriod, 'yyMMdd')"/>
          </xsl:element>
          <xsl:element name="C50703">
            <xsl:value-of select="'101'"/>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="WriteRFFRaw">
    <xsl:param name="referenceQualifier"/>
    <xsl:param name="referenceNumber"/>
    <xsl:param name="lineNumber" select="''"/>
    <xsl:param name="referenceVersionNumber" select="''"/>
    <xsl:element name="ns0:RFF">
      <xsl:element name="ns0:C506">
        <xsl:element name="C50601">
          <xsl:value-of select="$referenceQualifier" />
        </xsl:element>
        <xsl:if test="normalize-space($referenceNumber)!=''">
          <xsl:element name="C50602">
            <xsl:value-of select="$referenceNumber" />
          </xsl:element>
        </xsl:if>
        <xsl:if test="normalize-space($lineNumber)!=''">
          <xsl:element name="C50603">
            <xsl:value-of select="$lineNumber" />
          </xsl:element>
        </xsl:if>
        <xsl:if test="normalize-space($referenceVersionNumber)!=''">
          <xsl:element name="C50604">
            <xsl:value-of select="$referenceVersionNumber" />
          </xsl:element>
        </xsl:if>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template name="WriteNADFromOrganization">
    <xsl:param name="partyQualifier"/>
    <xsl:param name="addressType"/>
    <xsl:param name="fallbackAddressType"/>
    <xsl:call-template name="WriteNADFromOrganizationAddressRaw">
      <xsl:with-param name="partyQualifier" select="$partyQualifier"/>
      <xsl:with-param name="addressType" select="$addressType"/>
      <xsl:with-param name="fallbackAddressType" select="$fallbackAddressType"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteNADFromOrganizationAddressRaw">
    <xsl:param name="partyQualifier"/>
    <xsl:param name="addressType"/>
    <xsl:param name="fallbackAddressType" select="''"/>
    <xsl:variable name="companyName" select="*[local-name()='Shipment']/*[local-name()='OrganizationAddressCollection']
                      /*[local-name()='OrganizationAddress'][*[local-name()='AddressType']/text()=$addressType]
                      /*[local-name()='CompanyName']/text()"/>
    <xsl:variable name="fallbackCompanyName" select="*[local-name()='Shipment']/*[local-name()='OrganizationAddressCollection']
                      /*[local-name()='OrganizationAddress'][*[local-name()='AddressType']/text()=$fallbackAddressType]
                      /*[local-name()='CompanyName']/text()"/>
    <xsl:call-template name="WriteNADRaw">
      <xsl:with-param name="partyQualifier" select="$partyQualifier"/>
      <xsl:with-param name="nameAndAddress">
        <xsl:choose>
          <xsl:when test="normalize-space($companyName)!=''">
            <xsl:value-of select="$companyName"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$fallbackCompanyName"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteNADRaw">
    <xsl:param name="partyQualifier"/>
    <xsl:param name="nameAndAddress" select="''"/>
    <xsl:if test="normalize-space($partyQualifier)!='' and normalize-space($nameAndAddress)!=''">
      <xsl:element name="ns0:NADLoop1">
        <xsl:element name="ns0:NAD">
          <xsl:element name="NAD01">
            <xsl:value-of select="$partyQualifier"/>
          </xsl:element>
          <xsl:element name="ns0:C058">
            <xsl:element name="C05801">
              <xsl:value-of select="$nameAndAddress"/>
            </xsl:element>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="WriteGISFromInvoiceLineSWPM">
    <xsl:for-each select="*[local-name()='Shipment']/*[local-name()='CommercialInfo']
                          /*[local-name()='CommercialInvoiceCollection']/*[local-name()='CommercialInvoice']
                          /*[local-name()='CommercialInvoiceLineCollection']/*[local-name()='CommercialInvoiceLine']">
      <xsl:variable name="SWPMIndicator" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][*[local-name()='Key']/text()='SWPMIndicator']/*[local-name()='Value']/text()"/>
      <xsl:variable name="countSWAPIndicatorList" select="userCSharp:CountSWAPIndicatorList()"/>
      <xsl:if test="$countSWAPIndicatorList&lt;10 and normalize-space($SWPMIndicator)!=''">
        <xsl:if test="userCSharp:AddToSWAPIndicatorList(string($SWPMIndicator))='true'">
          <xsl:element name="ns0:GIS">
            <xsl:element name="ns0:C529">
              <xsl:element name="C52901">
                <xsl:value-of select="'ZZZ'" />
              </xsl:element>
              <xsl:element name="C52904">
                <xsl:value-of select="$SWPMIndicator" />
              </xsl:element>
            </xsl:element>
          </xsl:element>
        </xsl:if>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="WriteLOCFromPortOfOrigin">
    <xsl:param name="placeLocationQualifier"/>
    <xsl:param name="locationCodeListQualifier"/>
    <xsl:param name="relatedPlaceLocationOneIdentifier"/>
    <xsl:variable name="countryIBMCode">
      <xsl:call-template name="GetCustomizedFieldRaw">
        <xsl:with-param name="key" select="'IBM Ship From Country'"/>
        <xsl:with-param name="parentLevel" select ="./*[local-name()='Shipment']"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="shipmentFromCountry">
      <xsl:choose>
        <xsl:when test="normalize-space($relatedPlaceLocationOneIdentifier)!=''">
          <xsl:value-of select="$relatedPlaceLocationOneIdentifier"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="'ZZ'"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="locationIBMCode">
      <xsl:call-template name="GetCustomizedFieldRaw">
        <xsl:with-param name="key" select="'IBM Ship From Location'"/>
        <xsl:with-param name="parentLevel" select ="./*[local-name()='Shipment']"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:call-template name="WriteLOCRaw">
      <xsl:with-param name="placeLocationQualifier" select="$placeLocationQualifier"/>
      <xsl:with-param name="placeLocationIdentifier" select="$countryIBMCode"/>
      <xsl:with-param name="locationCodeListQualifier" select="$locationCodeListQualifier"/>
      <xsl:with-param name="relatedPlaceLocationOneIdentifier" select="$shipmentFromCountry"/>
      <xsl:with-param name="relatedPlaceLocationOne" select="$locationIBMCode"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteLOCFromAddInfo_US_NKLocationOfGoods">
    <xsl:param name="addInfoKey"/>
    <xsl:param name="placeLocationQualifier"/>
    <xsl:param name="locationCodeListQualifier"/>
    <xsl:variable name="placeLocation" select="*[local-name()='Shipment']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][*[local-name()='Key']/text()=$addInfoKey]/*[local-name()='Value']/text()"/>
    <xsl:call-template name="WriteLOCFromAddInfoRaw">
      <xsl:with-param name="addInfoKey" select="$addInfoKey"/>
      <xsl:with-param name="placeLocationQualifier" select="$placeLocationQualifier"/>
      <xsl:with-param name="locationCodeListQualifier" select="$locationCodeListQualifier"/>
      <xsl:with-param name="placeLocation" select="$placeLocation"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteLOCFromAddInfo">
    <xsl:param name="addInfoKey"/>
    <xsl:param name="placeLocationQualifier"/>
    <xsl:param name="locationCodeListQualifier"/>
    <xsl:call-template name="WriteLOCFromAddInfoRaw">
      <xsl:with-param name="addInfoKey" select="$addInfoKey"/>
      <xsl:with-param name="placeLocationQualifier" select="$placeLocationQualifier"/>
      <xsl:with-param name="locationCodeListQualifier" select="$locationCodeListQualifier"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteLOCFromAddInfoRaw">
    <xsl:param name="addInfoKey"/>
    <xsl:param name="placeLocationQualifier"/>
    <xsl:param name="locationCodeListQualifier" select="''"/>
    <xsl:param name="placeLocation" select="''"/>
    <xsl:variable name="placeLocationIdentifier" select="*[local-name()='Shipment']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][*[local-name()='Key']/text()=$addInfoKey]/*[local-name()='Value']/text()"/>
    <xsl:if test="normalize-space($placeLocationIdentifier)!=''">
      <xsl:call-template name="WriteLOCRaw">
        <xsl:with-param name="placeLocationQualifier" select="$placeLocationQualifier"/>
        <xsl:with-param name="placeLocationIdentifier" select="$placeLocationIdentifier"/>
        <xsl:with-param name="locationCodeListQualifier" select="$locationCodeListQualifier"/>
        <xsl:with-param name="placeLocation" select="$placeLocation"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name="WriteLOCRaw">
    <xsl:param name="placeLocationQualifier"/>
    <xsl:param name="placeLocationIdentifier" select="''"/>
    <xsl:param name="locationCodeListQualifier" select="''"/>
    <xsl:param name="placeLocation" select="''"/>
    <xsl:param name="relatedPlaceLocationOneIdentifier" select="''"/>
    <xsl:param name="relatedPlaceLocationOne" select="''"/>
    <xsl:element name="ns0:LOC">
      <xsl:element name="LOC01">
        <xsl:value-of select="$placeLocationQualifier" />
      </xsl:element>
      <xsl:if test="normalize-space($placeLocationIdentifier)!='' and normalize-space($locationCodeListQualifier)!=''">
        <xsl:element name="ns0:C517">
          <xsl:element name="C51701">
            <xsl:value-of select="$placeLocationIdentifier" />
          </xsl:element>
          <xsl:element name="C51702">
            <xsl:value-of select="$locationCodeListQualifier" />
          </xsl:element>
          <xsl:if test="normalize-space($placeLocation)!=''">
            <xsl:element name="C51704">
              <xsl:value-of select="$placeLocation" />
            </xsl:element>
          </xsl:if>
        </xsl:element>
      </xsl:if>
      <xsl:if test="normalize-space($relatedPlaceLocationOneIdentifier)!='' and normalize-space($relatedPlaceLocationOne)!=''">
        <xsl:element name="ns0:C519">
          <xsl:element name="C51901">
            <xsl:value-of select="$relatedPlaceLocationOneIdentifier" />
          </xsl:element>
          <xsl:element name="C51904">
            <xsl:value-of select="$relatedPlaceLocationOne" />
          </xsl:element>
        </xsl:element>
      </xsl:if>
    </xsl:element>
  </xsl:template>

  <xsl:template name="WriteTDTFromInbondType">
    <xsl:param name="transportStageQualifier"/>
    <xsl:variable name="inboundType" select="*[local-name()='Shipment']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][*[local-name()='Key']/text()='InbondType']/*[local-name()='Value']/text()"/>
    <xsl:if test="normalize-space($inboundType)!=''">
      <xsl:call-template name="WriteTDTRaw">
        <xsl:with-param name="transportStageQualifier" select="$transportStageQualifier"/>
        <xsl:with-param name="modeOfTransportCode" select="$inboundType"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name="WriteTDTFromCarrierSCAC">
    <xsl:variable name="carrierId" select="*[local-name()='Shipment']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][*[local-name()='Key']/text()='UI_NKCarrierSCAC']/*[local-name()='Value']/text()"/>
    <xsl:if test="normalize-space($carrierId)!=''">
      <xsl:call-template name="WriteTDTRaw">
        <xsl:with-param name="transportStageQualifier" select="'21'"/>
        <xsl:with-param name="carrierId" select="$carrierId"/>
        <xsl:with-param name="carrierCodeListQualifier" select="'172'"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name="WriteTDTFromBillIssuerSCAC">
    <xsl:variable name="carrierId" select="*[local-name()='Shipment']/*[local-name()='AdditionalBillCollection']
                                          /*[local-name()='AdditionalBill'][*[local-name()='BillType']/*[local-name()='Code']/text()='MWB']
                                          /*[local-name()='AddInfoCollection']
                                          /*[local-name()='AddInfo'][*[local-name()='Key']/text()='UI_NKBillIssuerSCAC']
                                          /*[local-name()='Value']/text()"/>
    <xsl:if test="normalize-space($carrierId)!=''">
      <xsl:call-template name="WriteTDTRaw">
        <xsl:with-param name="transportStageQualifier" select="'22'"/>
        <xsl:with-param name="carrierId" select="$carrierId"/>
        <xsl:with-param name="carrierCodeListQualifier" select="'172'"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name="WriteTDTFromTransportMode">
    <xsl:param name="transportStageQualifier"/>
    <xsl:param name="modeOfTransportCode"/>
    <xsl:param name="typeOfMeansOfTransportId"/>
    <xsl:param name="typeOfMeansOfTransport"/>
    <xsl:param name="carrierCodeListQualifier"/>
    <xsl:param name="carrierName"/>
    <xsl:variable name="carrierId" select="*[local-name()='Shipment']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][*[local-name()='Key']/text()='UI_NKCarrierSCAC']/*[local-name()='Value']/text()"/>
    <xsl:call-template name="WriteTDTRaw">
      <xsl:with-param name="transportStageQualifier" select="$transportStageQualifier"/>
      <xsl:with-param name="modeOfTransportCode" select="$modeOfTransportCode"/>
      <xsl:with-param name="typeOfMeansOfTransportId" select="$typeOfMeansOfTransportId"/>
      <xsl:with-param name="typeOfMeansOfTransport" select="$typeOfMeansOfTransport"/>
      <xsl:with-param name="carrierId" select="$carrierId"/>
      <xsl:with-param name="carrierCodeListQualifier" select="$carrierCodeListQualifier"/>
      <xsl:with-param name="carrierName" select="$carrierName"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteTDTRaw">
    <xsl:param name="transportStageQualifier"/>
    <xsl:param name="modeOfTransportCode" select="''"/>
    <xsl:param name="typeOfMeansOfTransportId" select="''"/>
    <xsl:param name="typeOfMeansOfTransport" select="''"/>
    <xsl:param name="carrierId" select="''"/>
    <xsl:param name="carrierCodeListQualifier" select="''"/>
    <xsl:param name="carrierName" select="''"/>
    <xsl:element name="ns0:TDT">
      <xsl:element name="TDT01">
        <xsl:value-of select="$transportStageQualifier" />
      </xsl:element>
      <xsl:if test="normalize-space($modeOfTransportCode)!=''">
        <xsl:element name="ns0:C220">
          <xsl:element name="C22001">
            <xsl:value-of select="$modeOfTransportCode" />
          </xsl:element>
        </xsl:element>
      </xsl:if>
      <xsl:if test="normalize-space($typeOfMeansOfTransportId)!='' and normalize-space($typeOfMeansOfTransport)!=''">
        <xsl:element name="ns0:C228">
          <xsl:element name="C22801">
            <xsl:value-of select="$typeOfMeansOfTransportId" />
          </xsl:element>
          <xsl:element name="C22802">
            <xsl:value-of select="$typeOfMeansOfTransport" />
          </xsl:element>
        </xsl:element>
      </xsl:if>
      <xsl:if test="normalize-space($carrierId)!='' and normalize-space($carrierCodeListQualifier)!=''">
        <xsl:element name="ns0:C040">
          <xsl:element name="C04001">
            <xsl:value-of select="$carrierId" />
          </xsl:element>
          <xsl:element name="C04002">
            <xsl:value-of select="$carrierCodeListQualifier" />
          </xsl:element>
          <xsl:if test="$carrierName!=''">
            <xsl:element name="C04004">
              <xsl:value-of select="$carrierName" />
            </xsl:element>
          </xsl:if>
        </xsl:element>
      </xsl:if>
    </xsl:element>
  </xsl:template>

  <xsl:template name="WriteBGM_C10601">
    <xsl:for-each select="/*[local-name()='UniversalShipment']/*[local-name()='Shipment']">
      <xsl:variable name="entryNumber">
        <xsl:call-template name="GetEntryNumberRaw">
          <xsl:with-param name="typeCode" select="'ENS'"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:variable name="entryFilerCode">
        <xsl:call-template name="GetAddinfoValueRaw">
          <xsl:with-param name="key" select="'EntryFilerCode'"/>
        </xsl:call-template>
      </xsl:variable>
      <C10601>
        <xsl:value-of select="concat($entryFilerCode,$entryNumber)" />
      </C10601>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="GetEntryNumberRaw">
    <xsl:param name="typeCode"/>
    <xsl:param name="parentLevel" select="."/>
    <xsl:value-of select="$parentLevel/*[local-name()='EntryNumberCollection']
                          /*[local-name()='EntryNumber'][*[local-name()='Type']/*[local-name()='Code']/text()=$typeCode]
                          /*[local-name()='Number']/text()"/>
  </xsl:template>

</xsl:stylesheet>

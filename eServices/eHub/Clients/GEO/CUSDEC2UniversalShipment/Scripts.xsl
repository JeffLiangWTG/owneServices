<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2011/11" xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0" xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1" xmlns:ScriptNS2="http://schemas.microsoft.com/BizTalk/2003/ScriptNS2"  xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:output method="xml" indent="yes"/>

  <xsl:template match="@* | node()">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()"/>
    </xsl:copy>
  </xsl:template>

  <!--Scripts begins from here-->

  <xsl:template name="WriteAddInfoFromPACLoop3PCILoop3_21">
    <xsl:param name="countryOfOrigin"/>
    <xsl:variable name="nAFTAInd" select="*[local-name()='PACLoop3'][*[local-name()='PAC_3']/*[local-name()='C531_3']/*[local-name()='C53102']/text()='34']
                                      /*[local-name()='PCILoop3']/*[local-name()='PCI_3'][*[local-name()='PCI01']/text()='21']
                                      /*[local-name()='C210_3']/*[local-name()='C21001']/text()"/>
    <xsl:variable name="sPI">
      <xsl:choose>
        <xsl:when test="$nAFTAInd='Y' and ($countryOfOrigin='US' or $countryOfOrigin='MX' or $countryOfOrigin='CA')">
          <xsl:text>S</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="''"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:call-template name="WriteAddInfoRaw">
      <xsl:with-param name="key" select="'SPI'"/>
      <xsl:with-param name="value" select="$sPI"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteAddInfoFromGIR">
    <xsl:param name="qualifier"/>
    <xsl:param name="elementName"/>
    <xsl:param name="key"/>
    <xsl:call-template name="WriteAddInfoFromGIRRaw">
      <xsl:with-param name="qualifier" select="$qualifier"/>
      <xsl:with-param name="elementName" select="$elementName"/>
      <xsl:with-param name="key" select="$key"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteAddInfoFromGIRRaw">
    <xsl:param name="qualifier"/>
    <xsl:param name="elementName"/>
    <xsl:param name="key"/>
    <xsl:variable name="value" select="*[local-name()='GIR'][*[local-name()='GIR01']/text()=$qualifier]/*[local-name()=$elementName]/*[local-name()='C20601']/text()"/>
    <xsl:call-template name="WriteAddInfoRaw">
      <xsl:with-param name="key" select="$key"/>
      <xsl:with-param name="value" select="$value"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteAddInfoFromIMD_E_and_F">
    <xsl:for-each select="*[local-name()='IMDLoop1'][*[local-name()='IMD']/*[local-name()='IMD01']/text()='E'][1]">
      <xsl:variable name="c273" select="*[local-name()='IMD']/*[local-name()='C273']"/>
      <xsl:call-template name="WriteAddInfoRaw">
        <xsl:with-param name="key" select="'CVDCaseNo'"/>
        <xsl:with-param name="value" select="substring($c273/*[local-name()='C27304']/text(),1,10)"/>
      </xsl:call-template>
    </xsl:for-each>
    <xsl:for-each select="*[local-name()='IMDLoop1'][*[local-name()='IMD']/*[local-name()='IMD01']/text()='F']">
      <xsl:variable name="c273" select="*[local-name()='IMD']/*[local-name()='C273']"/>
      <xsl:call-template name="WriteAddInfoRaw">
        <xsl:with-param name="key" select="'ADDCaseNo'"/>
        <xsl:with-param name="value" select="substring($c273/*[local-name()='C27304']/text(),1,10)"/>
      </xsl:call-template>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="WriteAddInfoFDAOrFCCIndicatorFromPACLoop3_34_PCILoop3_ZZZ">
    <xsl:param name="key"/>
    <xsl:param name="elementName"/>
    <xsl:param name="IMDItemDescriptionTypeCoded"/>
    <xsl:call-template name="WriteAddInfoFDAOrFCCIndicatorFromPACLoop3_34_PCILoop3_ZZZRaw">
      <xsl:with-param name="key" select="$key"/>
      <xsl:with-param name="elementName" select="$elementName"/>
      <xsl:with-param name="IMDItemDescriptionTypeCoded" select="$IMDItemDescriptionTypeCoded"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteAddInfoFDAOrFCCIndicatorFromPACLoop3_34_PCILoop3_ZZZRaw">
    <xsl:param name="key"/>
    <xsl:param name="elementName"/>
    <xsl:param name="IMDItemDescriptionTypeCoded"/>
    <xsl:variable name="fDAIOrFCCnfo" select="*[local-name()='PACLoop3'][*[local-name()='PAC_3']/*[local-name()='C531_3']/*[local-name()='C53102']/text()='34']
                                                  /*[local-name()='PCILoop3']/*[local-name()='PCI_3'][*[local-name()='PCI01']/text()='ZZZ']
                                                  /*[local-name()='C210_3']/*[local-name()=$elementName]/text()"/>
    <xsl:variable name="agencyId" select="*[local-name()='IMDLoop1']/*[local-name()='IMD'][*[local-name()='IMD01']/text()=$IMDItemDescriptionTypeCoded]
                                            /*[local-name()='C273']/*[local-name()='C27301']/text()"/>
    <xsl:variable name="fDAOrFCCIndicator">
      <xsl:choose>
        <xsl:when test="$fDAIOrFCCnfo='Y'">D</xsl:when>
        <xsl:when test="$fDAIOrFCCnfo='N' and $agencyId!=''">C</xsl:when>
        <xsl:otherwise></xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:call-template name="WriteAddInfoRaw">
      <xsl:with-param name="key" select="$key"/>
      <xsl:with-param name="value" select="$fDAOrFCCIndicator"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteAddInfoGroupFromIMD_B_FTX_8_GBL">
    <xsl:for-each select="*[local-name()='IMDLoop1'][*[local-name()='IMD']/*[local-name()='IMD01']/text()='B'][1]">
      <xsl:variable name="c273" select="*[local-name()='IMD']/*[local-name()='C273']"/>
      <xsl:variable name="c108_8" select="*[local-name()='FTX_8'][*[local-name()='FTX01']/text()='GBL']
                                          /*[local-name()='C108_8']"/>
      <xsl:variable name="linePrice">
        <xsl:call-template name="GetInvoiceLinePriceRaw">
          <xsl:with-param name="parentLevel" select=".."/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:call-template name="WriteAddInfoGroupRaw">
        <xsl:with-param name="typeCode" select="'USA'"/>
        <xsl:with-param name="typeDescription" select="'FDA'"/>
        <xsl:with-param name="key1" select="'FDACommercialDesc'"/>
        <xsl:with-param name="value1" select="substring($c273/*[local-name()='C27304']/text(),1,70)"/>
        <xsl:with-param name="key2" select="'FDAProductCode'"/>
        <xsl:with-param name="value2" select="substring($c273/*[local-name()='C27305']/text(),1,7)"/>
        <xsl:with-param name="key3" select="'UC_NKFDAProduction'"/>
        <xsl:with-param name="value3" select="substring($c273/*[local-name()='C27306']/text(),1,2)"/>
        <xsl:with-param name="key4" select="'TradeBrandName'"/>
        <xsl:with-param name="value4" select="substring($c108_8/*[local-name()='C10804']/text(),1,38)"/>
        <xsl:with-param name="key5" select="'PNC'"/>
        <xsl:with-param name="value5" select="substring($c108_8/*[local-name()='C10805']/text(),1,12)"/>
        <xsl:with-param name="key6" select="'FDAValue'"/>
        <xsl:with-param name="value6" select="$linePrice"/>
      </xsl:call-template>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="WriteAddInfoGroupFromIMD_A_FTX_8_ABL">
    <xsl:variable name="invoiceLineQuantity" select="*[local-name()='QTY_2']/*[local-name()='C186_2']/*[local-name()='C18602']/text()"/>
    <xsl:for-each select="*[local-name()='IMDLoop1'][*[local-name()='IMD']/*[local-name()='IMD01']/text()='A'][1]">
      <xsl:variable name="c108_8" select="*[local-name()='FTX_8'][*[local-name()='FTX01']/text()='ABL']
                                          /*[local-name()='C108_8']"/>
      <xsl:call-template name="WriteAddInfoGroupRaw">
        <xsl:with-param name="typeCode" select="'USC'"/>
        <xsl:with-param name="typeDescription" select="'FCC'"/>
        <xsl:with-param name="key1" select="'FCCImpCondNo'"/>
        <xsl:with-param name="value1" select="substring($c108_8/*[local-name()='C10801']/text(),1,2)"/>
        <xsl:with-param name="key2" select="'FCCID'"/>
        <xsl:with-param name="value2" select="substring($c108_8/*[local-name()='C10802']/text(),1,17)"/>
        <xsl:with-param name="key3" select="'FCCTradeName'"/>
        <xsl:with-param name="value3" select="substring($c108_8/*[local-name()='C10803']/text(),1,30)"/>
        <xsl:with-param name="key4" select="'FCCModel'"/>
        <xsl:with-param name="value4" select="substring($c108_8/*[local-name()='C10804']/text(),1,17)"/>
        <xsl:with-param name="key5" select="'FCCCommercialDesc'"/>
        <xsl:with-param name="value5" select="substring($c108_8/*[local-name()='C10805']/text(),1,70)"/>
        <xsl:with-param name="key6" select="'FCCQty'"/>
        <xsl:with-param name="value6" select="$invoiceLineQuantity"/>
      </xsl:call-template>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="WriteAddInfoGroupRaw">
    <xsl:param name="typeCode"/>
    <xsl:param name="typeDescription"/>
    <xsl:param name="key1"/>
    <xsl:param name="value1"/>
    <xsl:param name="key2" select="''"/>
    <xsl:param name="value2" select="''"/>
    <xsl:param name="key3" select="''"/>
    <xsl:param name="value3" select="''"/>
    <xsl:param name="key4" select="''"/>
    <xsl:param name="value4" select="''"/>
    <xsl:param name="key5" select="''"/>
    <xsl:param name="value5" select="''"/>
    <xsl:param name="key6" select="''"/>
    <xsl:param name="value6" select="''"/>
    <xsl:param name="key7" select="''"/>
    <xsl:param name="value7" select="''"/>
    <xsl:element name="ns0:AddInfoGroup">
      <xsl:element name="ns0:Type">
        <xsl:element name="ns0:Code">
          <xsl:value-of select="$typeCode"/>
        </xsl:element>
        <xsl:element name="ns0:Description">
          <xsl:value-of select="$typeDescription"/>
        </xsl:element>
      </xsl:element>
      <xsl:element name="ns0:AddInfoCollection">
        <xsl:call-template name="WriteAddInfoRaw">
          <xsl:with-param name="key" select="$key1"/>
          <xsl:with-param name="value" select="$value1"/>
        </xsl:call-template>
        <xsl:call-template name="WriteAddInfoRaw">
          <xsl:with-param name="key" select="$key2"/>
          <xsl:with-param name="value" select="$value2"/>
        </xsl:call-template>
        <xsl:call-template name="WriteAddInfoRaw">
          <xsl:with-param name="key" select="$key3"/>
          <xsl:with-param name="value" select="$value3"/>
        </xsl:call-template>
        <xsl:call-template name="WriteAddInfoRaw">
          <xsl:with-param name="key" select="$key4"/>
          <xsl:with-param name="value" select="$value4"/>
        </xsl:call-template>
        <xsl:call-template name="WriteAddInfoRaw">
          <xsl:with-param name="key" select="$key5"/>
          <xsl:with-param name="value" select="$value5"/>
        </xsl:call-template>
        <xsl:call-template name="WriteAddInfoRaw">
          <xsl:with-param name="key" select="$key6"/>
          <xsl:with-param name="value" select="$value6"/>
        </xsl:call-template>
        <xsl:call-template name="WriteAddInfoRaw">
          <xsl:with-param name="key" select="$key7"/>
          <xsl:with-param name="value" select="$value7"/>
        </xsl:call-template>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template name="WriteCustomizedFieldFromALC_2">
    <xsl:param name="key"/>
    <xsl:param name="dataType"/>
    <xsl:variable name="originalValue" select="*[local-name()='ALCLoop2']/*[local-name()='ALC_2']/*[local-name()='ALC01']/text()"/>
    <xsl:variable name="value">
      <xsl:choose>
        <xsl:when test="$originalValue='C'">true</xsl:when>
        <xsl:otherwise>false</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:call-template name="WriteCustomizedFieldRaw">
      <xsl:with-param name="key" select="$key"/>
      <xsl:with-param name="dataType" select="$dataType"/>
      <xsl:with-param name="value" select="$value"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteCustomizedFieldFromGIR">
    <xsl:param name="qualifier"/>
    <xsl:param name="elementName"/>
    <xsl:param name="key"/>
    <xsl:param name="dataType"/>
    <xsl:call-template name="WriteCustomizedFieldFromGIRRaw">
      <xsl:with-param name="qualifier" select="$qualifier"/>
      <xsl:with-param name="elementName" select="$elementName"/>
      <xsl:with-param name="key" select="$key"/>
      <xsl:with-param name="dataType" select="$dataType"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteCustomizedFieldFromGIRRaw">
    <xsl:param name="qualifier"/>
    <xsl:param name="elementName"/>
    <xsl:param name="key"/>
    <xsl:param name="dataType"/>
    <xsl:variable name="value" select="*[local-name()='GIR'][*[local-name()='GIR01']/text()=$qualifier]/*[local-name()=$elementName]/*[local-name()='C20601']/text()"/>
    <xsl:call-template name="WriteCustomizedFieldRaw">
      <xsl:with-param name="key" select="$key"/>
      <xsl:with-param name="dataType" select="$dataType"/>
      <xsl:with-param name="value" select="$value"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteCustomizedFieldFromMEA_3">
    <xsl:param name="measurementPurposeQualifier"/>
    <xsl:param name="propertyMeasuredCoded"/>
    <xsl:param name="key"/>
    <xsl:param name="dataType"/>
    <xsl:call-template name="WriteCustomizedFieldFromMEA_3Raw">
      <xsl:with-param name="measurementPurposeQualifier" select="$measurementPurposeQualifier"/>
      <xsl:with-param name="propertyMeasuredCoded" select="$propertyMeasuredCoded"/>
      <xsl:with-param name="key" select="$key"/>
      <xsl:with-param name="dataType" select="$dataType"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteCustomizedFieldFromMEA_3Raw">
    <xsl:param name="measurementPurposeQualifier"/>
    <xsl:param name="propertyMeasuredCoded"/>
    <xsl:param name="key"/>
    <xsl:param name="dataType"/>
    <xsl:variable name="value" select="*[local-name()='MEA_3'][*[local-name()='MEA01']/text()=$measurementPurposeQualifier and *[local-name()='C502_3']/*[local-name()='C50201']/text()=$propertyMeasuredCoded]
                                      /*[local-name()='C502_3']/*[local-name()='C50204']/text()"/>
    <xsl:call-template name="WriteCustomizedFieldRaw">
      <xsl:with-param name="key" select="$key"/>
      <xsl:with-param name="dataType" select="$dataType"/>
      <xsl:with-param name="value" select="$value"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteCustomizedFieldRaw">
    <xsl:param name="key"/>
    <xsl:param name="dataType"/>
    <xsl:param name="value"/>
    <xsl:if test="normalize-space($key)!='' and normalize-space($dataType)!='' and normalize-space($value)!=''">
      <xsl:element name="ns0:CustomizedField">
        <xsl:element name="ns0:Key">
          <xsl:value-of select="$key"/>
        </xsl:element>
        <xsl:element name="ns0:DataType">
          <xsl:value-of select="$dataType"/>
        </xsl:element>
        <xsl:element name="ns0:Value">
          <xsl:value-of select="$value"/>
        </xsl:element>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="WriteCommercialCharge_FromMOA_5">
    <xsl:param name="qualifier"/>
    <xsl:param name="chargeTypeCode"/>
    <xsl:call-template name="WriteCommercialCharge_FromMOA_5Raw">
      <xsl:with-param name="qualifier" select="$qualifier"/>
      <xsl:with-param name="chargeTypeCode" select="$chargeTypeCode"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteCommercialCharge_FromMOA_5Raw">
    <xsl:param name="qualifier"/>
    <xsl:param name="chargeTypeCode"/>
    <xsl:variable name="amount" select="*[local-name()='MOA_5']/*[local-name()='C516_5'][*[local-name()='C51601']/text()=$qualifier]/*[local-name()='C51602']/text()"/>
    <xsl:call-template name="WriteCommercialChargeRaw">
      <xsl:with-param name="chargeTypeCode" select="$chargeTypeCode"/>
      <xsl:with-param name="amount" select="$amount"/>
      <xsl:with-param name="currencyCode" select="''"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteInvoiceLineWeight">
    <xsl:variable name="caseGrossWeight" select="*[local-name()='MEA_3'][*[local-name()='MEA01']/text()='WT' and *[local-name()='C502_3']/*[local-name()='C50201']/text()='G']
                                                /*[local-name()='C502_3']/*[local-name()='C50204']/text()"/>
    <xsl:variable name="caseItemCount" select="*[local-name()='GIR'][*[local-name()='GIR01']/text()='3']/*[local-name()='C206_2']/*[local-name()='C20601']/text()"/>
    <xsl:variable name="weight">
      <xsl:call-template name="NumberOrDefault">
        <xsl:with-param name="number" select="number($caseGrossWeight) div number($caseItemCount)"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="weightFinal">
      <xsl:choose>
        <xsl:when test="round($weight)=0">1</xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="round($weight)"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:element name="ns0:Weight">
      <xsl:value-of select="format-number(number($weightFinal),'0')"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="NumberOrDefault">
    <xsl:param name="number"/>
    <xsl:param name="default" select="'0'"/>
    <xsl:choose>
      <xsl:when test="number($number)  and $number!='Infinity'">
        <xsl:value-of select="$number"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$default"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="WriteInvoiceLinePrice">
    <xsl:variable name="linePrice">
      <xsl:call-template name="GetInvoiceLinePriceRaw"/>
    </xsl:variable>
    <xsl:element name="ns0:LinePrice">
      <xsl:value-of select="$linePrice"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="GetInvoiceLinePriceRaw">
    <xsl:param name="parentLevel" select="."/>
    <xsl:variable name="unitPrice" select="$parentLevel/*[local-name()='MOA_5']/*[local-name()='C516_5'][*[local-name()='C51601']/text()='146']/*[local-name()='C51602']/text()"/>
    <xsl:variable name="quantity" select="$parentLevel/*[local-name()='QTY_2']/*[local-name()='C186_2']/*[local-name()='C18602']/text()"/>
    <xsl:choose>
      <xsl:when test="number($unitPrice) and number($quantity)">
        <xsl:variable name="linePrice" select="number($unitPrice)*number($quantity)"/>
        <xsl:value-of select="format-number(number($linePrice),'0.##')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="format-number(0,'0.##')"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="WriteCommercialCharge_FromMOA_2">
    <xsl:param name="qualifier"/>
    <xsl:param name="chargeTypeCode"/>
    <xsl:call-template name="WriteCommercialCharge_FromMOA_2Raw">
      <xsl:with-param name="qualifier" select="$qualifier"/>
      <xsl:with-param name="chargeTypeCode" select="$chargeTypeCode"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteCommercialCharge_FromMOA_2Raw">
    <xsl:param name="qualifier"/>
    <xsl:param name="chargeTypeCode"/>
    <xsl:variable name="amount" select="*[local-name()='MOALoop2']/*[local-name()='MOA_2']/*[local-name()='C516_2'][*[local-name()='C51601']/text()=$qualifier]/*[local-name()='C51602']/text()"/>
    <xsl:variable name="currencyCode">
      <xsl:call-template name="GetInvoiceCurrencyCodeRaw"/>
    </xsl:variable>
    <xsl:call-template name="WriteCommercialChargeRaw">
      <xsl:with-param name="chargeTypeCode" select="$chargeTypeCode"/>
      <xsl:with-param name="amount" select="$amount"/>
      <xsl:with-param name="currencyCode" select="$currencyCode"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteCommercialChargeRaw">
    <xsl:param name="chargeTypeCode"/>
    <xsl:param name="amount"/>
    <xsl:param name="currencyCode"/>
    <xsl:if test="normalize-space($chargeTypeCode)!='' and number($amount)">
      <xsl:element name="ns0:CommercialCharge">
        <xsl:element name="ns0:ChargeType">
          <xsl:element name="ns0:Code">
            <xsl:value-of select="$chargeTypeCode"/>
          </xsl:element>
        </xsl:element>
        <xsl:element name="ns0:Amount">
          <xsl:value-of select="$amount"/>
        </xsl:element>
        <xsl:if test="normalize-space($currencyCode)!=''">
          <xsl:element name="ns0:Currency">
            <xsl:element name="ns0:Code">
              <xsl:value-of select="$currencyCode"/>
            </xsl:element>
          </xsl:element>
        </xsl:if>
        <xsl:if test="$chargeTypeCode='ADD'">
          <xsl:element name="ns0:IsDutiable">true</xsl:element>
          <xsl:element name="ns0:IsGSTApplicable">true</xsl:element>
        </xsl:if>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="WriteSupplierOrganizationCode">
    <xsl:variable name="supplierOrganizationCode">
      <xsl:call-template name="GetNADPartyIdentification">
        <xsl:with-param name="partyQualifier" select="'VN'"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="manufacturerOrganizationCode" select="*[local-name()='PATLoop1']/*[local-name()='FTX_6']/*[local-name()='C107_6']/*[local-name()='C10701']/text()"/>
    <xsl:variable name="isForCanada">
      <xsl:call-template name="IsForCanada"/>
    </xsl:variable>
    <xsl:if test="$isForCanada='true'">
      <xsl:variable name="organizationCodeToUse">
        <xsl:choose>
          <xsl:when test="normalize-space($supplierOrganizationCode)!=''">
            <xsl:value-of select="$supplierOrganizationCode"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$manufacturerOrganizationCode"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:if test="normalize-space($organizationCodeToUse)!=''">
        <xsl:element name="ns0:Supplier">
          <xsl:element name="ns0:AddressType">
            <xsl:value-of select="'Supplier'"/>
          </xsl:element>
          <xsl:element name="ns0:OrganizationCode">
            <xsl:value-of select="$organizationCodeToUse"/>
          </xsl:element>
        </xsl:element>
      </xsl:if>
    </xsl:if>
  </xsl:template>

  <xsl:template name="WriteInvoiceCurrencyCode">
    <xsl:element name="ns0:Code">
      <xsl:call-template name="GetInvoiceCurrencyCodeRaw"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="GetInvoiceCurrencyCodeRaw">
    <xsl:variable name="currencyCode" select="*[local-name()='MOALoop2'][*[local-name()='MOA_2']/*[local-name()='C516_2']/*[local-name()='C51601']/text()='206']
                                              /*[local-name()='CUXLoop2']/*[local-name()='CUX_2']/*[local-name()='C504_3']/*[local-name()='C50402']/text()"/>
    <xsl:variable name="fallbackCurrencyCode" select="*[local-name()='MOALoop2'][1]/*[local-name()='MOA_2']/*[local-name()='C516_2']/*[local-name()='C51603']/text()"/>
    <xsl:choose>
      <xsl:when test="normalize-space($currencyCode)!=''">
        <xsl:value-of select="$currencyCode"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$fallbackCurrencyCode"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="WriteInvoiceHeaderAddInfoCollection">
    <xsl:param name="countryOfExport"/>
    <xsl:param name="countryOfOrigin"/>
    <xsl:element name="ns0:AddInfoCollection">
      <xsl:call-template name="WriteAddInfoRaw">
        <xsl:with-param name="key" select="'UC_NKCountryOfExport'"/>
        <xsl:with-param name="value" select="$countryOfExport"/>
      </xsl:call-template>

      <xsl:call-template name="WriteAddInfoRaw">
        <xsl:with-param name="key" select="'UC_NKCountryOfOrigin'"/>
        <xsl:with-param name="value" select="$countryOfOrigin"/>
      </xsl:call-template>

      <xsl:variable name="dateOfExport" select="*[local-name()='MOALoop2'][*[local-name()='MOA_2']/*[local-name()='C516_2']/*[local-name()='C51601']/text()='206']
                                              /*[local-name()='CUXLoop2']/*[local-name()='DTM_6']/*[local-name()='C507_6']/*[local-name()='C50702']/text()"/>
      <xsl:variable name="formattedDateOfExport" select="ScriptNS2:ConvertToDateTimeString(string($dateOfExport), 'yyMMdd', 'yyyy-MM-dd 00:00:00.000')"/>
      <xsl:call-template name="WriteAddInfoRaw">
        <xsl:with-param name="key" select="'DateOfExport'"/>
        <xsl:with-param name="value" select="$formattedDateOfExport"/>
      </xsl:call-template>
    </xsl:element>
  </xsl:template>

  <xsl:template name="WriteInvoiceHeaderOrganizationAddress">
   
  <xsl:variable name="FDAShipper" select="*[local-name()='LINLoop1']/*[local-name()='IMDLoop1']/*[local-name()='FTX_8']
								[*[local-name()='FTX01']='GBL']/*[local-name()='C108_8']/*[local-name()='C10803']"/> 
  <xsl:call-template name="WriteOrganizationAddressRaw">
   <xsl:with-param name="addressType" select="'Seller'"/>
   <xsl:with-param name="organizationCode" select="$FDAShipper"/>
  </xsl:call-template>
		
  <xsl:variable name="ShipToOrgCode" select="*[local-name()='NADLoop2']/*[local-name()='NAD_2']
								[*[local-name()='NAD01']='ST']/*[local-name()='C082_2']/*[local-name()='C08201']"/>
  <xsl:call-template name="WriteOrganizationAddressRaw">
        <xsl:with-param name="addressType" select="'ShipToParty'"/>
        <xsl:with-param name="organizationCode" select="$ShipToOrgCode"/>
  </xsl:call-template>
  <xsl:call-template name="WriteOrganizationAddressRaw">
        <xsl:with-param name="addressType" select="'Importer'"/>
        <xsl:with-param name="organizationCode" select="$ShipToOrgCode"/>
  </xsl:call-template>	  

</xsl:template>
  <xsl:template name="GetNADPartyIdentification">
    <xsl:param name="partyQualifier"/>
    <xsl:param name="parentLevel" select="."/>
    <xsl:value-of select="$parentLevel/*[local-name()='NADLoop2']
                          /*[local-name()='NAD_2'][*[local-name()='NAD01']/text()=$partyQualifier]
                          /*[local-name()='C082_2']/*[local-name()='C08201']/text()"/>
  </xsl:template>

  <xsl:template name="GetNADCountryCoded">
    <xsl:param name="partyQualifier"/>
    <xsl:param name="parentLevel" select="."/>
    <xsl:value-of select="$parentLevel/*[local-name()='NADLoop2']
                          /*[local-name()='NAD_2'][*[local-name()='NAD01']/text()=$partyQualifier]
                          /*[local-name()='NAD09']/text()"/>
  </xsl:template>

  <xsl:template name="WriteInvoiceNoteCollection">
    <xsl:variable name="chargeIndicator" select="*[local-name()='PATLoop1']/*[local-name()='FTX_6']/*[local-name()='C108_6']/*[local-name()='C10801']/text()"/>
    <xsl:variable name="issuerLocation">
      <xsl:call-template name="GetNADPartyIdentification">
        <xsl:with-param name="partyQualifier" select="'II'"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="issuerCountry">
      <xsl:call-template name="GetNADCountryCoded">
        <xsl:with-param name="partyQualifier" select="'II'"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="shipFromLocation">
      <xsl:call-template name="GetNADPartyIdentification">
        <xsl:with-param name="partyQualifier" select="'SF'"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="shipFromCountry">
      <xsl:call-template name="GetNADCountryCoded">
        <xsl:with-param name="partyQualifier" select="'SF'"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="shipToLocation">
      <xsl:call-template name="GetNADPartyIdentification">
        <xsl:with-param name="partyQualifier" select="'ST'"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="shipToCountry">
      <xsl:call-template name="GetNADCountryCoded">
        <xsl:with-param name="partyQualifier" select="'ST'"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="invoiceToLocation">
      <xsl:call-template name="GetNADPartyIdentification">
        <xsl:with-param name="partyQualifier" select="'IV'"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="invoiceToCountry">
      <xsl:call-template name="GetNADCountryCoded">
        <xsl:with-param name="partyQualifier" select="'IV'"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:element name="ns0:NoteCollection">
      <xsl:call-template name="WriteNoteRaw">
        <xsl:with-param name="description" select="'Charge Indicator'"/>
        <xsl:with-param name="noteText" select="$chargeIndicator"/>
      </xsl:call-template>
      <xsl:call-template name="WriteNoteRaw">
        <xsl:with-param name="description" select="'Invoice Location'"/>
        <xsl:with-param name="noteText" select="$issuerLocation"/>
      </xsl:call-template>
      <xsl:call-template name="WriteNoteRaw">
        <xsl:with-param name="description" select="'Invoice Country'"/>
        <xsl:with-param name="noteText" select="$issuerCountry"/>
      </xsl:call-template>
      <xsl:call-template name="WriteNoteRaw">
        <xsl:with-param name="description" select="'Ship From Location'"/>
        <xsl:with-param name="noteText" select="$shipFromLocation"/>
      </xsl:call-template>
      <xsl:call-template name="WriteNoteRaw">
        <xsl:with-param name="description" select="'Ship From Country'"/>
        <xsl:with-param name="noteText" select="$shipFromCountry"/>
      </xsl:call-template>
      <xsl:call-template name="WriteNoteRaw">
        <xsl:with-param name="description" select="'Ship To Location'"/>
        <xsl:with-param name="noteText" select="$shipToLocation"/>
      </xsl:call-template>
      <xsl:call-template name="WriteNoteRaw">
        <xsl:with-param name="description" select="'Ship To Country'"/>
        <xsl:with-param name="noteText" select="$shipToCountry"/>
      </xsl:call-template>
      <xsl:call-template name="WriteNoteRaw">
        <xsl:with-param name="description" select="'Invoice To Location'"/>
        <xsl:with-param name="noteText" select="$invoiceToLocation"/>
      </xsl:call-template>
      <xsl:call-template name="WriteNoteRaw">
        <xsl:with-param name="description" select="'Invoice To Country'"/>
        <xsl:with-param name="noteText" select="$invoiceToCountry"/>
      </xsl:call-template>
    </xsl:element>
  </xsl:template>

  <xsl:template name="WriteNoteRaw">
    <xsl:param name="description"/>
    <xsl:param name="isCustomDescription" select="'true'"/>
    <xsl:param name="noteText"/>
    <xsl:element name="ns0:Note">
      <xsl:element name="ns0:Description">
        <xsl:value-of select="$description"/>
      </xsl:element>
      <xsl:element name="ns0:IsCustomDescription">
        <xsl:value-of select="$isCustomDescription"/>
      </xsl:element>
      <xsl:element name="ns0:NoteText">
        <xsl:value-of select="$noteText"/>
      </xsl:element>
      <xsl:element name="ns0:NoteContext">
        <xsl:element name="ns0:Code">AAA</xsl:element>
      </xsl:element>
      <xsl:element name="ns0:Visibility">
        <xsl:element name="ns0:Code">PUB</xsl:element>
      </xsl:element>
    </xsl:element>
  </xsl:template>
  
  <xsl:template name="WriteCustomizedField_IBMVendorCode_FromDMSLoop1_NAD">
    <xsl:variable name="vendorCode">
      <xsl:call-template name="GetNADPartyIdentification">
        <xsl:with-param name="partyQualifier" select="'VN'"/>
        <xsl:with-param name="parentLevel" select="./*[local-name()='DMSLoop1'][1]"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:call-template name="WriteCustomizedFieldRaw">
      <xsl:with-param name="key" select="'IBM Vendor Code'"/>
      <xsl:with-param name="dataType" select="'String'"/>
      <xsl:with-param name="value" select="$vendorCode"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteCustomizedField_IBMShipFromCountry_FromLOC">
    <xsl:variable name="isoShipFromCountryCode" select="*[local-name()='LOC'][*[local-name()='LOC01']/text()='113']
                                                        /*[local-name()='C519']/*[local-name()='C51901']/text()"/>
    <xsl:call-template name="WriteCustomizedFieldRaw">
      <xsl:with-param name="key" select="'IBM Ship From Country'"/>
      <xsl:with-param name="dataType" select="'String'"/>
      <xsl:with-param name="value" select="$isoShipFromCountryCode"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteCustomizedField_IBMShipFromLocation_FromLOC">
    <xsl:variable name="isoShipFromCountryCode" select="*[local-name()='LOC'][*[local-name()='LOC01']/text()='113']
                                                        /*[local-name()='C517']/*[local-name()='C51704']/text()"/>
    <xsl:call-template name="WriteCustomizedFieldRaw">
      <xsl:with-param name="key" select="'IBM Ship From Location'"/>
      <xsl:with-param name="dataType" select="'String'"/>
      <xsl:with-param name="value" select="$isoShipFromCountryCode"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteCustomizedFieldFromDMSLoop1AndFTX_6FirstOnly">
    <xsl:param name="key"/>
    <xsl:param name="dataType"/>
    <xsl:param name="elementName"/>
    <xsl:variable name="chargeIndicator" select="*[local-name()='DMSLoop1']/*[local-name()='PATLoop1']/*[local-name()='FTX_6']/*[local-name()='C108_6']/*[local-name()=$elementName]/text()"/>
    <xsl:call-template name="WriteCustomizedFieldRaw">
      <xsl:with-param name="key" select="$key"/>
      <xsl:with-param name="dataType" select="$dataType"/>
      <xsl:with-param name="value" select="$chargeIndicator"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteCustomizedFieldFromDMSLoop1AndFTX_6Raw">
    <xsl:param name="key"/>
    <xsl:param name="dataType"/>
    <xsl:param name="elementName"/>
    <xsl:variable name="invoiceNoAndChargeIndicators">
      <xsl:for-each select="*[local-name()='DMSLoop1']">
        <xsl:variable name="invoiceNo" select="*[local-name()='DMS']/*[local-name()='DMS01']/text()"/>
        <xsl:variable name="chargeIndicator" select="*[local-name()='PATLoop1']/*[local-name()='FTX_6']/*[local-name()='C108_6']/*[local-name()=$elementName]/text()"/>
        <xsl:value-of select="concat($invoiceNo,':',$chargeIndicator,';')"/>
      </xsl:for-each>
    </xsl:variable>
    <xsl:call-template name="WriteCustomizedFieldRaw">
      <xsl:with-param name="key" select="$key"/>
      <xsl:with-param name="dataType" select="$dataType"/>
      <xsl:with-param name="value" select="$invoiceNoAndChargeIndicators"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WritePort">
    <xsl:param name="partyQualifier"/>
    <xsl:param name="portElementName"/>
    <xsl:call-template name="WritePortRaw">
      <xsl:with-param name="partyQualifier" select="$partyQualifier"/>
      <xsl:with-param name="portElementName" select="$portElementName"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WritePortRaw">
    <xsl:param name="partyQualifier"/>
    <xsl:param name="portElementName"/>
    <xsl:variable name="NAD_2_SF" select="*[local-name()='DMSLoop1']/*[local-name()='NADLoop2']
                                          /*[local-name()='NAD_2'][*[local-name()='NAD01']/text()=$partyQualifier]" />
    <xsl:variable name="location" select="$NAD_2_SF/*[local-name()='C082_2']/*[local-name()='C08201']/text()" />
    <xsl:variable name="country" select="$NAD_2_SF/*[local-name()='NAD09']/text()" />
    <xsl:variable name="port" select="concat($country,$location)"/>
    <xsl:variable  name="portElementNameWithPrefix" select="concat('ns0:',$portElementName)"/>
    <xsl:if test="$port!=''">
      <xsl:element name="{$portElementNameWithPrefix}">
        <xsl:element name="ns0:Code">
          <xsl:value-of select="$port" />
        </xsl:element>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="WriteWayBill">
    <xsl:variable name="hwbNumber" select="*[local-name()='RFFLoop1']/*[local-name()='RFF']/*[local-name()='C506'][*[local-name()='C50601']/text()='HWB']/*[local-name()='C50602']/text()" />
    <xsl:variable name="mwbNumber" select="*[local-name()='RFFLoop1']/*[local-name()='RFF']/*[local-name()='C506'][*[local-name()='C50601']/text()='MWB']/*[local-name()='C50602']/text()" />
    <xsl:choose>
      <xsl:when test="$hwbNumber!=''">
        <xsl:call-template name="WriteWayBillRaw">
          <xsl:with-param name="number" select="$hwbNumber"/>
          <xsl:with-param name="typeCode" select="'HWB'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="WriteWayBillRaw">
          <xsl:with-param name="number" select="$mwbNumber"/>
          <xsl:with-param name="typeCode" select="'MWB'"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="WriteWayBillRaw">
    <xsl:param name="number"/>
    <xsl:param name="typeCode"/>
    <xsl:if test="normalize-space($number)!='' and normalize-space($typeCode)!=''">
      <xsl:element name="ns0:WayBillNumber">
        <xsl:value-of select="$number"/>
      </xsl:element>
      <xsl:element name="ns0:WayBillType">
        <xsl:element name="ns0:Code">
          <xsl:value-of select="$typeCode"/>
        </xsl:element>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="WriteAdditionalBill">
    <xsl:variable name="hwbNumber" select="*[local-name()='RFFLoop1']/*[local-name()='RFF']/*[local-name()='C506'][*[local-name()='C50601']/text()='HWB']/*[local-name()='C50602']/text()" />
    <xsl:variable name="mwbNumber" select="*[local-name()='RFFLoop1']/*[local-name()='RFF']/*[local-name()='C506'][*[local-name()='C50601']/text()='MWB']/*[local-name()='C50602']/text()" />
    <xsl:call-template name="WriteAdditionalBillRaw">
      <xsl:with-param name="number" select="$mwbNumber"/>
      <xsl:with-param name="typeCode" select="'MWB'"/>
    </xsl:call-template>
    <xsl:call-template name="WriteAdditionalBillRaw">
      <xsl:with-param name="number" select="$hwbNumber"/>
      <xsl:with-param name="typeCode" select="'HWB'"/>
      <xsl:with-param name="parentBillNumber" select="$mwbNumber"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteAdditionalBillRaw">
    <xsl:param name="number"/>
    <xsl:param name="typeCode"/>
    <xsl:param name="parentBillNumber" select="''"/>
    <xsl:if test="normalize-space($number)!='' and normalize-space($typeCode)!=''">
      <xsl:element name="ns0:AdditionalBill">
        <xsl:element name="ns0:BillNumber">
          <xsl:value-of select="$number"/>
        </xsl:element>
        <xsl:element name="ns0:BillType">
          <xsl:element name="ns0:Code">
            <xsl:value-of select="$typeCode"/>
          </xsl:element>
        </xsl:element>
        <xsl:if test="normalize-space($parentBillNumber)!=''">
          <xsl:element name="ns0:ParentBillNumber">
            <xsl:value-of select="$parentBillNumber"/>
          </xsl:element>
        </xsl:if>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="WriteDate_FromDTM">
    <xsl:param name="type"/>
    <xsl:param name="isEstimate"/>
    <xsl:param name="qualifier"/>
    <xsl:call-template name="WriteDate_FromDTMRaw">
      <xsl:with-param name="type" select="$type"/>
      <xsl:with-param name="isEstimate" select="$isEstimate"/>
      <xsl:with-param name="qualifier" select="$qualifier"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteDate_FromDTMRaw">
    <xsl:param name="type"/>
    <xsl:param name="isEstimate"/>
    <xsl:param name="qualifier"/>
    <xsl:variable name="inputFormat" select="'yyMMdd'"/>
    <xsl:variable name="outputFormat" select="'yyyy-MM-ddT00:00:00'"/>
    <xsl:variable name="dateOfExport" select="*[local-name()='DTM']/*[local-name()='C507'][*[local-name()='C50701']/text()=$qualifier]/*[local-name()='C50702']/text()" />
    <xsl:variable name="formattedDateOfExport" select="ScriptNS2:ConvertToDateTimeString(string($dateOfExport), $inputFormat, $outputFormat)" />
    <xsl:call-template name="WriteDateRaw">
      <xsl:with-param name="type" select="$type"/>
      <xsl:with-param name="isEstimate" select="$isEstimate"/>
      <xsl:with-param name="value" select="$formattedDateOfExport"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteDate">
    <xsl:param name="type"/>
    <xsl:param name="isEstimate"/>
    <xsl:param name="value"/>
    <xsl:call-template name="WriteDateRaw">
      <xsl:with-param name="type" select="$type"/>
      <xsl:with-param name="isEstimate" select="$isEstimate"/>
      <xsl:with-param name="value" select="$value"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteDateRaw">
    <xsl:param name="type"/>
    <xsl:param name="isEstimate"/>
    <xsl:param name="value"/>
    <xsl:if test="normalize-space($type)!='' and normalize-space($isEstimate)!='' and normalize-space($value)!=''">
      <xsl:element name="ns0:Date">
        <xsl:element name="ns0:Type">
          <xsl:value-of select="$type"/>
        </xsl:element>
        <xsl:element name="ns0:IsEstimate">
          <xsl:value-of select="$isEstimate"/>
        </xsl:element>
        <xsl:element name="ns0:Value">
          <xsl:value-of select="$value"/>
        </xsl:element>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="WriteAddInfo_FromDTM">
    <xsl:param name="key"/>
    <xsl:param name="qualifier"/>
    <xsl:variable name="inputFormat" select="'yyMMdd'"/>
    <xsl:variable name="outputFormat" select="'yyyy-MM-dd 00:00:00.000'"/>
    <xsl:variable name="dateOfExport" select="*[local-name()='DTM']/*[local-name()='C507'][*[local-name()='C50701']/text()=$qualifier]/*[local-name()='C50702']/text()" />
    <xsl:variable name="formattedDateOfExport" select="ScriptNS2:ConvertToDateTimeString(string($dateOfExport) , $inputFormat, $outputFormat)" />
    <xsl:call-template name="WriteAddInfoRaw">
      <xsl:with-param name="key" select="$key"/>
      <xsl:with-param name="value" select="$formattedDateOfExport"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteAddInfo">
    <xsl:param name="key"/>
    <xsl:param name="value"/>
    <xsl:call-template name="WriteAddInfoRaw">
      <xsl:with-param name="key" select="$key"/>
      <xsl:with-param name="value" select="$value"/>
    </xsl:call-template>
  </xsl:template>

  <!--Invoice Line country of origin-->
  <xsl:template name="WriteLineAddInfo">
    <xsl:param name="key"/>
    <xsl:param name="value"/>

    <xsl:call-template name="WriteAddInfoRaw">
      <xsl:with-param name="key" select="$key"/>
      <xsl:with-param name="value" select="$value"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteAddInfoRaw">
    <xsl:param name="key"/>
    <xsl:param name="value"/>
    <xsl:if test="normalize-space($key)!='' and normalize-space($value)!=''">
      <xsl:element name="ns0:AddInfo">
        <xsl:element name="ns0:Key">
          <xsl:value-of select="$key"/>
        </xsl:element>
        <xsl:element name="ns0:Value">
          <xsl:value-of select="$value"/>
        </xsl:element>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="WriteOrganizationAddress">
    <xsl:param name="addressType"/>
    <xsl:param name="organizationCode"/>
    <xsl:call-template name="WriteOrganizationAddressRaw">
      <xsl:with-param name="addressType" select="$addressType"/>
      <xsl:with-param name="organizationCode" select="$organizationCode"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="WriteOrganizationAddressRaw">
    <xsl:param name="addressType"/>
    <xsl:param name="organizationCode"/>
    <xsl:param name="fallbackOrganizationCode" select="''"/>
    <xsl:variable name="organizationCodeToUse">
      <xsl:choose>
        <xsl:when test="normalize-space($organizationCode)!=''">
          <xsl:value-of select="$organizationCode"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$fallbackOrganizationCode"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:if test="normalize-space($addressType)!='' and normalize-space($organizationCodeToUse)!=''">
      <xsl:element name="ns0:OrganizationAddress">
        <xsl:element name="ns0:AddressType">
          <xsl:value-of select="$addressType"/>
        </xsl:element>
        <xsl:element name="ns0:OrganizationCode">
          <xsl:value-of select="$organizationCodeToUse"/>
        </xsl:element>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="WriteDataTargetType">
    <xsl:element name="ns0:Type">
      <xsl:value-of select="'CustomsDeclaration'"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="IsForCanada">
    <xsl:variable name="placeLocationIdentification" select="/*[local-name()='EFACT_D97A_CUSDEC']/*[local-name()='LOC'][1]/*[local-name()='C517'][1]/*[local-name()='C51701']/text()"/>
    <xsl:choose>
      <xsl:when test="normalize-space($placeLocationIdentification)='CA' or normalize-space($placeLocationIdentification)='649'">
        <xsl:value-of select="'true'"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="'false'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

</xsl:stylesheet>

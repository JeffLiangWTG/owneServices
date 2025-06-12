<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt"
    xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var" exclude-result-prefixes="msxsl var s0 userCSharp" version="1.0"
    xmlns:s0="http://www.cargowise.com/Schemas/Universal/2011/11"
    xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="yes" indent="no" version="1.0" method="xml" />

  <xsl:template match="/">
    <xsl:apply-templates select="/s0:UniversalShipment/s0:Shipment" />
  </xsl:template>
  <xsl:template match="s0:Shipment">
    <xsl:variable name="serviceCode" select="normalize-space(/s0:UniversalShipment/s0:Shipment/s0:DataContext/s0:RecipientRoleCollection/s0:RecipientRole[s0:Code/text()='STA'][1]/s0:ServiceCode/text())"/>
    <xsl:variable name="entryFilerCode" select="substring(normalize-space(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='EntryFilerCode'][1]/s0:Value/text()),1,3)"/>
    <xsl:element name="BrokerToSuretyMessage">
      <xsl:element name="BondHeader">
        <xsl:element name="SuretyFilerCode">
          <xsl:value-of select="$entryFilerCode" />
        </xsl:element>
        <xsl:variable name="importerOfRecord" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='ImporterOfRecord'][1]"/>
        <xsl:element name="ImporterNumber">
          <xsl:variable name="govRegNumType" select="normalize-space($importerOfRecord/s0:GovRegNumType/s0:Code/text())" />
          <xsl:choose>
            <xsl:when test="$govRegNumType">
              <xsl:choose>
                <xsl:when test="$govRegNumType = 'EIN' or $govRegNumType = 'CBN' or $govRegNumType = 'SSN'">
                  <xsl:value-of select="substring(normalize-space($importerOfRecord/s0:GovRegNum/text()),1,12)"/>
                </xsl:when>
              </xsl:choose>
            </xsl:when>
            <xsl:otherwise>
              <xsl:variable name="einNumber" select="substring(normalize-space($importerOfRecord/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text()='EIN'][1]/s0:Value/text()),1,12)"/>
              <xsl:variable name="cbnNumber" select="substring(normalize-space($importerOfRecord/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text()='CBN'][1]/s0:Value/text()),1,12)"/>
              <xsl:variable name="ssnNumber" select="substring(normalize-space($importerOfRecord/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text()='SSN'][1]/s0:Value/text()),1,12)"/>
              <xsl:choose>
                <xsl:when test="$einNumber">
                  <xsl:value-of select="$einNumber"/>
                </xsl:when>
                <xsl:when test="$cbnNumber">
                  <xsl:value-of select="$cbnNumber"/>
                </xsl:when>
                <xsl:when test="$ssnNumber">
                  <xsl:value-of select="$ssnNumber"/>
                </xsl:when>
              </xsl:choose>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:element>
        <xsl:element name="ImporterName">
          <xsl:value-of select="substring(normalize-space($importerOfRecord/s0:CompanyName/text()),1,35)"/>
        </xsl:element>
        <xsl:element name="ImporterAddressLine1">
          <xsl:value-of select="substring(normalize-space($importerOfRecord/s0:Address1/text()),1,35)"/>
        </xsl:element>
        <xsl:variable name="address2" select="substring(normalize-space($importerOfRecord/s0:Address2/text()),1,35)"/>
        <xsl:choose>
          <xsl:when test="$address2">
            <xsl:element name="ImporterAddressLine2">
              <xsl:value-of select="$address2"/>
            </xsl:element>
          </xsl:when>
        </xsl:choose>
        <xsl:element name="ImporterAddressCity">
          <xsl:value-of select="substring(normalize-space($importerOfRecord/s0:City/text()),1,35)"/>
        </xsl:element>
        <xsl:element name="ImporterAddressState">
          <xsl:value-of select="substring(normalize-space($importerOfRecord/s0:State/text()),1,2)"/>
        </xsl:element>
        <xsl:element name="ImporterAddressZipCode">
          <xsl:value-of select="substring(normalize-space($importerOfRecord/s0:Postcode/text()),1,9)"/>
        </xsl:element>
        <xsl:element name="ImporterAddressCountry">
          <xsl:value-of select="substring(normalize-space($importerOfRecord/s0:Country/s0:Code/text()),1,2)"/>
        </xsl:element>
        <xsl:element name="BondType">
          <xsl:choose>
            <xsl:when test="$serviceCode = 'PBB'">
              <xsl:value-of select="substring(normalize-space(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='BondType'][1]/s0:Value/text()),1,1)"/>
            </xsl:when>
            <xsl:when test="$serviceCode = 'SBB'">
              <xsl:value-of select="substring(normalize-space(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='BondType2'][1]/s0:Value/text()),1,1)"/>
            </xsl:when>
          </xsl:choose>
        </xsl:element>
        <xsl:element name="BondActivityCode">1</xsl:element>
        <xsl:variable name="cbpBondNumber1" select="substring(normalize-space(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CBPBondNo'][1]/s0:Value/text()),1,9)"/>
        <xsl:variable name="cbpBondNumber2" select="substring(normalize-space(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CBPBondNo2'][1]/s0:Value/text()),1,9)"/>
        <xsl:choose>
          <xsl:when test="$cbpBondNumber1 and $serviceCode = 'PBB'">
            <xsl:element name="CBPBondNumber">
              <xsl:value-of select="$cbpBondNumber1"/>
            </xsl:element>
          </xsl:when>
          <xsl:when test="$cbpBondNumber2 and $serviceCode = 'SBB'">
            <xsl:element name="CBPBondNumber">
              <xsl:value-of select="$cbpBondNumber2"/>
            </xsl:element>
          </xsl:when>
        </xsl:choose>
        <xsl:variable name="portCode" select="substring(normalize-space(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='SchDEntry'][1]/s0:Value/text()),1,4)"/>
        <xsl:choose>
          <xsl:when test="$portCode">
            <xsl:element name="PortCode">
              <xsl:value-of select="$portCode"/>
            </xsl:element>
          </xsl:when>
        </xsl:choose>
        <xsl:element name="BondDesignationCode">
          <xsl:choose>
            <xsl:when test="$serviceCode = 'PBB'">
              <xsl:value-of select="substring(normalize-space(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='BondDesignationCode'][1]/s0:Value/text()),1,1)"/>
            </xsl:when>
            <xsl:when test="$serviceCode = 'SBB'">
              <xsl:value-of select="substring(normalize-space(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='BondDesignationCode2'][1]/s0:Value/text()),1,1)"/>
            </xsl:when>
          </xsl:choose>
        </xsl:element>
        <xsl:element name="TransactionIDType">1</xsl:element>
        <xsl:element name="TransactionID">
          <xsl:value-of select="$entryFilerCode"/>
          <xsl:value-of select="substring(normalize-space(s0:EntryNumberCollection/s0:EntryNumber[s0:Type/s0:Code/text()='ENS'][1]/s0:Number/text()),1,8)"/>
        </xsl:element>
        <xsl:element name="EntryType">
          <xsl:value-of select="substring(normalize-space(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='EntryType'][1]/s0:Value/text()),1,2)"/>
        </xsl:element>
        <xsl:element name="BondAmount">
          <xsl:choose>
            <xsl:when test="$serviceCode = 'PBB'">
              <xsl:value-of select="substring(normalize-space(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='BondAmount'][1]/s0:Value/text()),1,10)"/>
            </xsl:when>
            <xsl:when test="$serviceCode = 'SBB'">
              <xsl:value-of select="substring(normalize-space(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='BondAmount2'][1]/s0:Value/text()),1,10)"/>
            </xsl:when>
          </xsl:choose>
        </xsl:element>
        <xsl:variable name="bondProduerAccountNumber1" select="substring(normalize-space(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='BondProducerAccNo'][1]/s0:Value/text()),1,10)"/>
        <xsl:variable name="bondProduerAccountNumber2" select="substring(normalize-space(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='BondProducerAccNo2'][1]/s0:Value/text()),1,10)"/>
        <xsl:choose>
          <xsl:when test="$serviceCode = 'PBB' and $bondProduerAccountNumber1">
            <xsl:element name="STBProducerAccount">
              <xsl:value-of select="$bondProduerAccountNumber1"/>
            </xsl:element>
          </xsl:when>
          <xsl:when test="$serviceCode = 'SBB' and $bondProduerAccountNumber2">
            <xsl:element name="STBProducerAccount">
              <xsl:value-of select="$bondProduerAccountNumber2"/>
            </xsl:element>
          </xsl:when>
        </xsl:choose>
        <xsl:element name="ExceptionContactName">
          <xsl:value-of select="substring(normalize-space(s0:CustomsBroker/s0:Name/text()),1,35)"/>
        </xsl:element>
        <xsl:element name="ExceptionContactEmail">
          <xsl:value-of select="substring(normalize-space(s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='BondContact'][1]/s0:Email/text()),1,35)"/>
        </xsl:element>
        <xsl:element name="ExceptionContactPhone">
          <xsl:value-of select="substring(normalize-space(s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='BondContact'][1]/s0:Phone/text()),1,35)"/>
        </xsl:element>
        <xsl:element name="BrokerReferenceNumber">
          <xsl:value-of select="substring(normalize-space(s0:DataContext/s0:DataSourceCollection/s0:DataSource[s0:Type/text()='CustomsDeclaration'][1]/s0:Key/text()),1,25)"/>
        </xsl:element>
        <xsl:element name="EstimatedEnteredValue">
          <xsl:variable name="entryLines" select="s0:EntryHeaderCollection/s0:EntryHeader[1]/s0:EntryLineCollection/s0:EntryLine"/>
          <xsl:value-of select="sum($entryLines/s0:CustomsValue)"/>
        </xsl:element>
      </xsl:element>
      <xsl:for-each select="s0:EntryHeaderCollection/s0:EntryHeader[1]">
        <xsl:apply-templates select="s0:EntryLineCollection">
          <xsl:with-param name="index" select="'1'"/>
        </xsl:apply-templates>
      </xsl:for-each>
    </xsl:element>
  </xsl:template>

  <xsl:template name="EntryLineLoop" match="s0:EntryLineCollection">
    <xsl:param name="index"/>
    <xsl:if test="s0:EntryLine[s0:LineNumber/text()=$index]">
      <xsl:element name="EntryLines">
        <xsl:element name="LineNo">
          <xsl:value-of select="$index"/>
        </xsl:element>
        <xsl:variable name="invoiceLines" select="../../../s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice/s0:CommercialInvoiceLineCollection/s0:CommercialInvoiceLine[s0:EntryLineNumber/text()=$index]"/>
        <xsl:for-each select="s0:EntryLine[s0:LineNumber=$index]">
          <xsl:element name="EntryLineDetail">
            <xsl:element name="HTSNumber">
              <xsl:value-of select="substring(s0:HarmonisedCode,1,10)"/>
            </xsl:element>
            <xsl:element name="CountryOfOrigin">
              <xsl:value-of select="substring($invoiceLines/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='UC_NKCountryOfOrigin'][1]/s0:Value/text(),1,2)"/>
            </xsl:element>
            <xsl:element name="TradeAgreementSpecialProgramClaimCode">
              <xsl:value-of select="substring($invoiceLines/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='SPI'][1]/s0:Value/text(),1,2)"/>
            </xsl:element>
            <xsl:element name="EstimatedEnteredValue">
              <xsl:value-of select="number(s0:CustomsValue)"/>
            </xsl:element>
            <xsl:element name="LineDutyAmount">
              <xsl:value-of select="sum(s0:EntryLineChargeCollection/s0:EntryLineCharge[s0:Type/s0:Code/text()='DTY'][1]/s0:Amount/text())"/>
            </xsl:element>
            <xsl:element name="IRTaxAmount">
              <xsl:value-of select="sum(s0:EntryLineChargeCollection/s0:EntryLineCharge[s0:Type/s0:Code/text()='022' or s0:Type/s0:Code/text()='016' or s0:Type/s0:Code/text()='017' or s0:Type/s0:Code/text()='018']/s0:Amount/text())"/>
            </xsl:element>
            <xsl:element name="CommodityFeeAmount">
              <xsl:value-of select="sum(s0:EntryLineChargeCollection/s0:EntryLineCharge
                             [
                              s0:Type/s0:Code/text()='107'
                              or s0:Type/s0:Code/text()='053'
                              or s0:Type/s0:Code/text()='106'
                              or s0:Type/s0:Code/text()='056'
                              or s0:Type/s0:Code/text()='110'
                              or s0:Type/s0:Code/text()='102'
                              or s0:Type/s0:Code/text()='055'
                              or s0:Type/s0:Code/text()='108'
                              or s0:Type/s0:Code/text()='103'
                              or s0:Type/s0:Code/text()='054'
                              or s0:Type/s0:Code/text()='090'
                              or s0:Type/s0:Code/text()='057'
                              or s0:Type/s0:Code/text()='105'
                              or s0:Type/s0:Code/text()='109'
                              or s0:Type/s0:Code/text()='079'
                              or s0:Type/s0:Code/text()='104']/s0:Amount/text())"/>
            </xsl:element>
            <xsl:element name="OtherFeeAmount">
              <xsl:value-of select="sum(s0:EntryLineChargeCollection/s0:EntryLineCharge[s0:Type/s0:Code/text()='499' or s0:Type/s0:Code/text()='501']/s0:Amount/text())"/>
            </xsl:element>
            <xsl:variable name="addDutyAmount" select="number(s0:EntryLineChargeCollection/s0:EntryLineCharge[s0:Type/s0:Code/text()='ADD']/s0:Amount/text())"/>
            <xsl:choose>
              <xsl:when test="$addDutyAmount > 0">
                <xsl:variable name="addCaseNoLines" select="$invoiceLines[s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ADDCaseNo' and s0:Value/text()!='']]"/>
                <xsl:element name="EntryLinesADDCVD">
                  <xsl:element name="CaseNumber">
                    <xsl:value-of select="substring($addCaseNoLines/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ADDCaseNo'][1]/s0:Value/text(),1,10)"/>
                  </xsl:element>
                  <xsl:element name="ADDutyAmount">
                    <xsl:value-of select="$addDutyAmount"/>
                  </xsl:element>
                  <xsl:variable name="isADDBonded" select="$addCaseNoLines/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='IsADDBonded'][1]/s0:Value/text()"/>
                  <xsl:element name="BondCashClaimCode">
                    <xsl:choose>
                      <xsl:when test="$isADDBonded='Y'">B</xsl:when>
                      <xsl:otherwise>C</xsl:otherwise>
                    </xsl:choose>
                  </xsl:element>
                  <xsl:element name="CaseDepositRate">
                    <xsl:value-of select="userCSharp:StringDecimalMaxAllowed(normalize-space(*[local-name()='AddInfoCollection']/*[local-name()='AddInfo' and *[local-name()='Key']/text()='ADDCaseDepositRate'][1]/*[local-name()='Value']/text()), 6, 2)"/>
                  </xsl:element>
                  <xsl:element name="CaseRateTypeQualifier">
                    <xsl:value-of select="substring($addCaseNoLines/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ADDDepositRateIndicator'][1]/s0:Value/text(),1,1)"/>
                  </xsl:element>
                  <xsl:element name="ADCVDValueOfGoods">
                    <xsl:value-of select="sum($addCaseNoLines/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ADDDepositValue']/s0:Value/text())"/>
                  </xsl:element>
                  <xsl:element name="ADCVDQuantity">
                    <xsl:value-of select="sum($addCaseNoLines/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ADDQty']/s0:Value/text())"/>
                  </xsl:element>
                  <xsl:choose>
                    <xsl:when test="$isADDBonded='Y'">
                      <xsl:element name="ADCVDBondedDutyAmount">
                        <xsl:value-of select="$addDutyAmount"/>
                      </xsl:element>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:element name="ADCVDCashDepositAmount">
                        <xsl:value-of select="$addDutyAmount"/>
                      </xsl:element>
                    </xsl:otherwise>
                  </xsl:choose>
                  <xsl:element name="NonReimbursementIndicator">
                    <xsl:variable name="nonReimbursementInd" select="substring($addCaseNoLines/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ADCVDStat'][1]/s0:Value/text(),1,1)"/>
                    <xsl:choose>
                      <xsl:when test="$nonReimbursementInd = 'D'">Y</xsl:when>
                    </xsl:choose>
                  </xsl:element>
                  <xsl:element name="DeclarationIdentifier">
                    <xsl:value-of select="substring($addCaseNoLines/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ADDDecID'][1]/s0:Value/text(),1,10)"/>
                  </xsl:element>
                </xsl:element>
              </xsl:when>
            </xsl:choose>

            <xsl:variable name="cvdDutyAmount" select="number(s0:EntryLineChargeCollection/s0:EntryLineCharge[s0:Type/s0:Code/text()='CVD']/s0:Amount/text())"/>
            <xsl:choose>
              <xsl:when test="$cvdDutyAmount > 0">
                <xsl:variable name="cvdCaseNoLines" select="$invoiceLines[s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CVDCaseNo' and s0:Value/text()!='']]"/>
                <xsl:element name="EntryLinesADDCVD">
                  <xsl:element name="CaseNumber">
                    <xsl:value-of select="substring($cvdCaseNoLines/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CVDCaseNo'][1]/s0:Value/text(),1,10)"/>
                  </xsl:element>
                  <xsl:element name="CVDutyAmount">
                    <xsl:value-of select="$cvdDutyAmount"/>
                  </xsl:element>
                  <xsl:variable name="isCVDBonded" select="$cvdCaseNoLines/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='IsCVDBonded'][1]/s0:Value/text()"/>
                  <xsl:element name="BondCashClaimCode">
                    <xsl:choose>
                      <xsl:when test="$isCVDBonded='Y'">B</xsl:when>
                      <xsl:otherwise>C</xsl:otherwise>
                    </xsl:choose>
                  </xsl:element>
                  <xsl:element name="CaseDepositRate">
                    <xsl:value-of select="userCSharp:StringDecimalMaxAllowed(normalize-space(*[local-name()='AddInfoCollection']/*[local-name()='AddInfo' and *[local-name()='Key']/text()='ADDCaseDepositRate'][1]/*[local-name()='Value']/text()), 6, 2)"/>
                  </xsl:element>
                  <xsl:element name="CaseRateTypeQualifier">
                    <xsl:value-of select="substring($cvdCaseNoLines/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CVDDepositRateIndicator'][1]/s0:Value/text(),1,1)"/>
                  </xsl:element>
                  <xsl:element name="ADCVDValueOfGoods">
                    <xsl:value-of select="sum($cvdCaseNoLines/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CVDDepositValue'][1]/s0:Value/text())"/>
                  </xsl:element>
                  <xsl:element name="ADCVDQuantity">
                    <xsl:value-of select="sum($cvdCaseNoLines/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CVDQty'][1]/s0:Value/text())"/>
                  </xsl:element>
                  <xsl:choose>
                    <xsl:when test="$isCVDBonded='Y'">
                      <xsl:element name="ADCVDBondedDutyAmount">
                        <xsl:value-of select="$cvdDutyAmount"/>
                      </xsl:element>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:element name="ADCVDCashDepositAmount">
                        <xsl:value-of select="$cvdDutyAmount"/>
                      </xsl:element>
                    </xsl:otherwise>
                  </xsl:choose>
                  <xsl:element name="NonReimbursementIndicator">
                    <xsl:variable name="nonReimbursementInd" select="substring($cvdCaseNoLines/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ADCVDStat'][1]/s0:Value/text(),1,1)"/>
                    <xsl:choose>
                      <xsl:when test="$nonReimbursementInd = 'D'">Y</xsl:when>
                    </xsl:choose>
                  </xsl:element>
                </xsl:element>
              </xsl:when>
            </xsl:choose>

            <xsl:variable name="fdaIndicator" select="substring($invoiceLines/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='FDAIndicator'][1]/s0:Value/text(),1,1)"/>
            <xsl:variable name="fccIndicator" select="substring($invoiceLines/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='FCCIndicator'][1]/s0:Value/text(),1,1)"/>
            <xsl:if test="$fdaIndicator or $fccIndicator">
              <xsl:element name="EntryLinesPGA">
                <xsl:if test="$fdaIndicator">
                  <xsl:element name="PGACode">FDA</xsl:element>
                  <xsl:element name="PGADisclaimerCode">
                    <xsl:value-of select="substring($invoiceLines/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='FDADisclaimReason'][1]/s0:Value/text(),1,1)"/>
                  </xsl:element>
                </xsl:if>
                <xsl:if test="$fccIndicator">
                  <xsl:element name="PGACode">FCC</xsl:element>
                </xsl:if>
              </xsl:element>
            </xsl:if>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>

      <xsl:call-template name="EntryLineLoop">
        <xsl:with-param name="index">
          <xsl:number value="number($index)+1" />
        </xsl:with-param>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[

public string StringMaxAllowed(string text, int lenBytes)
{
    return (System.Text.Encoding.UTF8.GetByteCount(text) > lenBytes) ?  GetInvalidCharacter(lenBytes) : text.ToUpper();
}

public string GetInvalidCharacter(int lenBytes)
{
    return "".PadLeft(lenBytes, '*');
}

public string StringDecimalMaxAllowed(string text, int precision, int scale)
{
    if (string.IsNullOrEmpty(text)) return "";
    
    try
    {
        decimal value = decimal.Round(Convert.ToDecimal(text), scale);
        decimal maximumLimit = decimal.Parse("".PadLeft(precision - scale, '9') + "." + "".PadLeft(scale, '9'));
        return value > maximumLimit ? GetInvalidCharacter(precision + 1) : value.ToString("0." + "".PadLeft(scale,'0'));
    }
    catch
    {
        return GetInvalidCharacter(precision + (scale==0 ? 0 : 1));
    }
}


        ]]>
  </msxsl:script>
</xsl:stylesheet>

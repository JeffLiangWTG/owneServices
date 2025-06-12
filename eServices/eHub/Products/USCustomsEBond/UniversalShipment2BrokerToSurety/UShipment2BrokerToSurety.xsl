<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt"
    xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var" exclude-result-prefixes="msxsl var s0 xs ScriptNS0 ScriptNS2 ScriptNS3 ScriptNS4 ScriptNS5 userCSharp" version="1.0"
    xmlns:s0="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
                xmlns:ScriptNS2="http://schemas.microsoft.com/BizTalk/2003/ScriptNS2"
                xmlns:ScriptNS3="http://schemas.microsoft.com/BizTalk/2003/ScriptNS3"
                xmlns:ScriptNS4="http://schemas.microsoft.com/BizTalk/2003/ScriptNS4"
                xmlns:ScriptNS5="http://schemas.microsoft.com/BizTalk/2003/ScriptNS5"
    xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="yes" indent="no" version="1.0" method="xml" />

  <xsl:template match="/">
    <xsl:apply-templates select="/s0:UniversalInterchange/s0:Body/s0:UniversalShipment/s0:Shipment" />
  </xsl:template>

  <xsl:template match="s0:Shipment">
    <xsl:variable name="SenderID" select="ScriptNS2:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
    <xsl:variable name="RecipientID" select="ScriptNS2:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
    <xsl:variable name="CustomsDeclaration" select="substring(normalize-space(s0:DataContext/s0:DataSourceCollection/s0:DataSource[s0:Type/text()='CustomsDeclaration'][1]/s0:Key/text()),1,25)"/>
    <xsl:variable name="entryFilerCode" select="substring(normalize-space(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='EntryFilerCode'][1]/s0:Value/text()),1,3)"/>
    <xsl:variable name="transactionID">
      <xsl:value-of select="$entryFilerCode"/>
      <xsl:value-of select="substring(normalize-space(s0:EntryNumberCollection/s0:EntryNumber[s0:Type/s0:Code/text()='ENS'][1]/s0:Number/text()),1,8)"/>
    </xsl:variable>
    <xsl:variable name="SubscribeValue" select="ScriptNS3:InsertSubscriptionValue('USCEB', $RecipientID, $SenderID, $transactionID)" />
    <xsl:variable name="username" select="normalize-space(/s0:UniversalInterchange/s0:Header/s0:DeliveryMetadata/s0:ValueCollection/s0:Value[s0:Name/text()='UserName']/s0:Data/text())" />
    <xsl:variable name="insuranceAgent" select="normalize-space(/s0:UniversalInterchange/s0:Header/s0:DeliveryMetadata/s0:ValueCollection/s0:Value[s0:Name/text()='InsuranceAgent']/s0:Data/text())" />
    <xsl:variable name="passwordDecrypted" select="ScriptNS5:DecryptPassword(normalize-space(/s0:UniversalInterchange/s0:Header/s0:DeliveryMetadata/s0:ValueCollection/s0:Value[s0:Name/text()='Password']/s0:Data/text()))" />
    <xsl:variable name="datetime" select="concat(ScriptNS4:CurrentDateTimeUTC('s'), 'Z')" />
    <xsl:variable name="passPhrase" select="ScriptNS5:ComputeSHA1Hash(concat($transactionID, $datetime, $passwordDecrypted))"/>

    <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/">
      <soapenv:Header xmlns:wsa="http://www.w3.org/2005/08/addressing">
        <wsa:From>
          <wsa:Address>
            <xsl:value-of select="concat('urn:abi:', $entryFilerCode)"/>
          </wsa:Address>
        </wsa:From>
        <wsa:To>
          <xsl:value-of select="concat('urn:abi:', $insuranceAgent)"/>
        </wsa:To>
        <wsa:Action>BrokerToSurety</wsa:Action>
        <wsse:Security xmlns:wsse="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd" xmlns:wsu="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd">
          <wsse:UsernameToken>
            <wsse:Username>
              <xsl:value-of select="$username"/>
            </wsse:Username>
            <wsse:Password Type="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordDigest">
              <xsl:value-of select="$passPhrase"/>
            </wsse:Password>
            <wsse:Nonce  EncodingType="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary">
              <xsl:value-of select="userCSharp:Base64String($transactionID)"/>
            </wsse:Nonce>
            <wsu:Created>
              <xsl:value-of select="$datetime"/>
            </wsu:Created>
          </wsse:UsernameToken>
        </wsse:Security>
      </soapenv:Header>
      <soapenv:Body>

        <xsl:element name="BrokerToSuretyMessage">
          <xsl:element name="BondHeader">
            <xsl:variable name="suretyFilerCode" select="substring(normalize-space(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='InsuranceAgent'][1]/s0:Value/text()),1,3)"/>
            <xsl:if test="$suretyFilerCode">
              <xsl:element name="SuretyFilerCode">
                <xsl:value-of select="$suretyFilerCode"/>
              </xsl:element>
            </xsl:if>
            <xsl:variable name="importerOfRecord" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='ImporterOfRecord'][1]"/>
            <xsl:variable name="govRegNumType" select="normalize-space($importerOfRecord/s0:GovRegNumType/s0:Code/text())" />
            <xsl:variable name="einNumber" select="substring(normalize-space($importerOfRecord/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text()='EIN'][1]/s0:Value/text()),1,12)"/>
            <xsl:variable name="cbnNumber" select="substring(normalize-space($importerOfRecord/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text()='CBN'][1]/s0:Value/text()),1,12)"/>
            <xsl:variable name="ssnNumber" select="substring(normalize-space($importerOfRecord/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text()='SSN'][1]/s0:Value/text()),1,12)"/>
            <xsl:choose>
              <xsl:when test="$govRegNumType">
                <xsl:choose>
                  <xsl:when test="$govRegNumType = 'EIN' or $govRegNumType = 'CBN' or $govRegNumType = 'SSN'">
                    <xsl:variable name="importerNumber" select="substring(normalize-space($importerOfRecord/s0:GovRegNum/text()),1,12)" />
                    <xsl:if test="$importerNumber">
                      <xsl:element name="ImporterNumber">
                        <xsl:value-of select="$importerNumber"/>
                      </xsl:element>
                    </xsl:if>
                  </xsl:when>
                 <xsl:otherwise>
                   <xsl:choose>
                   <xsl:when test="$einNumber">
                   <xsl:element name="ImporterNumber">
                       <xsl:value-of select="$einNumber"/>
                   </xsl:element>
                   </xsl:when>
                   <xsl:when test="$cbnNumber">
                   <xsl:element name="ImporterNumber">
                       <xsl:value-of select="$cbnNumber"/>
                   </xsl:element>
                   </xsl:when>
                   <xsl:when test="$ssnNumber">
                   <xsl:element name="ImporterNumber">
                       <xsl:value-of select="$ssnNumber"/>
                   </xsl:element>
                   </xsl:when>
                 </xsl:choose>
              </xsl:otherwise>
            </xsl:choose>
            </xsl:when>
              <xsl:otherwise>
                <xsl:choose>
                  <xsl:when test="$einNumber">
                    <xsl:element name="ImporterNumber">
                      <xsl:value-of select="$einNumber"/>
                    </xsl:element>
                  </xsl:when>
                  <xsl:when test="$cbnNumber">
                    <xsl:element name="ImporterNumber">
                      <xsl:value-of select="$cbnNumber"/>
                    </xsl:element>
                  </xsl:when>
                  <xsl:when test="$ssnNumber">
                    <xsl:element name="ImporterNumber">
                      <xsl:value-of select="$ssnNumber"/>
                    </xsl:element>
                  </xsl:when>
                </xsl:choose>
              </xsl:otherwise>
            </xsl:choose>
            <xsl:variable name="importerName" select="substring(normalize-space($importerOfRecord/s0:CompanyName/text()),1,35)" />
            <xsl:if test="$importerName">
              <xsl:element name="ImporterName">
                <xsl:value-of select="$importerName"/>
              </xsl:element>
            </xsl:if>
            <xsl:variable name="importerAddressLine1" select="substring(normalize-space($importerOfRecord/s0:Address1/text()),1,35)" />
            <xsl:if test="$importerAddressLine1">
              <xsl:element name="ImporterAddressLine1">
                <xsl:value-of select="$importerAddressLine1"/>
              </xsl:element>
            </xsl:if>
            <xsl:variable name="importerAddressLine2" select="substring(normalize-space($importerOfRecord/s0:Address2/text()),1,35)"/>
            <xsl:choose>
              <xsl:when test="$importerAddressLine2">
                <xsl:element name="ImporterAddressLine2">
                  <xsl:value-of select="$importerAddressLine2"/>
                </xsl:element>
              </xsl:when>
            </xsl:choose>
            <xsl:variable name="importerAddressCity" select="substring(normalize-space($importerOfRecord/s0:City/text()),1,35)"/>
            <xsl:if test="$importerAddressCity">
              <xsl:element name="ImporterAddressCity">
                <xsl:value-of select="$importerAddressCity"/>
              </xsl:element>
            </xsl:if>
            <xsl:element name="ImporterAddressState">
              <xsl:value-of select="substring(normalize-space($importerOfRecord/s0:State/text()),1,2)"/>
            </xsl:element>
            <xsl:element name="ImporterAddressZipCode">
              <xsl:value-of select="substring(normalize-space($importerOfRecord/s0:Postcode/text()),1,10)"/>
            </xsl:element>
            <xsl:variable name="importerAddressCountry" select="substring(normalize-space($importerOfRecord/s0:Country/s0:Code/text()),1,2)" />
            <xsl:if test="$importerAddressCountry">
              <xsl:element name="ImporterAddressCountry">
                <xsl:value-of select="$importerAddressCountry"/>
              </xsl:element>
            </xsl:if>
            <xsl:variable name="bondType" select="substring(normalize-space(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='BondType'][1]/s0:Value/text()),1,1)" />
            <xsl:if test="$bondType">
              <xsl:element name="BondType">
                <xsl:value-of select="$bondType"/>
              </xsl:element>
            </xsl:if>
            <xsl:element name="BondActivityCode">1</xsl:element>
            <xsl:variable name="cbpBondNumber" select="substring(normalize-space(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CBPBondNo'][1]/s0:Value/text()),1,9)"/>
            <xsl:variable name="bondDesignationCode" select="substring(normalize-space(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='BondDesignationCode'][1]/s0:Value/text()),1,1)"/>
            <xsl:if test="$cbpBondNumber != '' and ($bondDesignationCode = 'V' or $bondDesignationCode = 'C')">
              <xsl:element name="CBPBondNumber">
                <xsl:value-of select="$cbpBondNumber"/>
              </xsl:element>
            </xsl:if>
            <xsl:variable name="portCode" select="substring(normalize-space(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='SchDEntry'][1]/s0:Value/text()),1,4)" />
            <xsl:if test="$portCode">
              <xsl:element name="PortCode">
                <xsl:value-of select="$portCode"/>
              </xsl:element>
            </xsl:if>
            <xsl:if test="$bondDesignationCode">
              <xsl:element name="BondDesignationCode">
                <xsl:value-of select="$bondDesignationCode"/>
              </xsl:element>
            </xsl:if>
            <xsl:element name="TransactionIDType">1</xsl:element>
            <xsl:if test="$transactionID">
              <xsl:element name="TransactionID">
                <xsl:value-of select="$transactionID"/>
              </xsl:element>
            </xsl:if>
            <xsl:variable name="entryType" select="substring(normalize-space(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='EntryType'][1]/s0:Value/text()),1,2)" />
            <xsl:if test="$entryType">
              <xsl:element name="EntryType">
                <xsl:value-of select="$entryType"/>
              </xsl:element>
            </xsl:if>
            <xsl:variable name="bondAmount" select="substring(normalize-space(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='BondAmount'][1]/s0:Value/text()),1,10)" />
            <xsl:if test="$bondAmount > 0">
              <xsl:element name="BondAmount">
                <xsl:value-of select="$bondAmount"/>
              </xsl:element>
            </xsl:if>
            <xsl:variable name="bondProducerAccountNumber" select="substring(normalize-space(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='BondProducerAccNo'][1]/s0:Value/text()),1,10)"/>
            <xsl:if test="$bondProducerAccountNumber">
              <xsl:element name="STBBondProducerAccount">
                <xsl:value-of select="$bondProducerAccountNumber"/>
              </xsl:element>
            </xsl:if>
            <xsl:variable name="bondContact" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='BondContact'][1]" />
            <xsl:variable name="firstSecondaryNotifyParty" select="substring(normalize-space($bondContact/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text()='SNP'][1]/s0:Value/text()),1,9)" />
            <xsl:if test="$firstSecondaryNotifyParty">
              <xsl:element name="FirstSecondaryNotifyParty">
                <xsl:value-of select="$firstSecondaryNotifyParty"/>
              </xsl:element>
            </xsl:if>
            <xsl:variable name="exceptionContactName" select="substring(normalize-space($bondContact/s0:Contact/text()),1,35)" />
            <xsl:if test="$exceptionContactName">
              <xsl:element name="ExceptionContactName">
                <xsl:value-of select="$exceptionContactName"/>
              </xsl:element>
            </xsl:if>
            <xsl:variable name="exceptionContactEmail" select="substring(normalize-space($bondContact/s0:Email/text()),1,100)" />
            <xsl:if test="$exceptionContactEmail">
              <xsl:element name="ExceptionContactEmail">
                <xsl:value-of select="$exceptionContactEmail"/>
              </xsl:element>
            </xsl:if>
            <xsl:variable name="exceptionContactPhone" select="substring(normalize-space($bondContact/s0:Phone/text()),1,20)" />
            <xsl:if test="$exceptionContactPhone">
              <xsl:element name="ExceptionContactPhone">
                <xsl:value-of select="$exceptionContactPhone"/>
              </xsl:element>
            </xsl:if>
            <xsl:if test="$CustomsDeclaration">
              <xsl:element name="BrokerFilerReference">
                <xsl:value-of select="$CustomsDeclaration"/>
              </xsl:element>
            </xsl:if>
            <xsl:variable name="entryLines" select="s0:EntryHeaderCollection/s0:EntryHeader[1]/s0:EntryLineCollection/s0:EntryLine"/>
            <xsl:variable name="totalEntryLineCustomsValues" select="format-number(sum($entryLines/s0:CustomsValue), '0.##')"/>
            <xsl:if test="$totalEntryLineCustomsValues > 0">
              <xsl:element name="EstimatedEnteredValue">
                <xsl:value-of select="$totalEntryLineCustomsValues"/>
              </xsl:element>
            </xsl:if>
          </xsl:element>
          <xsl:for-each select="s0:EntryHeaderCollection/s0:EntryHeader[1]">
            <xsl:apply-templates select="s0:EntryLineCollection">
              <xsl:with-param name="lineNo" select="'1'"/>
            </xsl:apply-templates>
          </xsl:for-each>
        </xsl:element>

      </soapenv:Body>
    </soapenv:Envelope>
  </xsl:template>

  <xsl:template name="EntryLineLoop" match="s0:EntryLineCollection">
    <xsl:param name="lineNo"/>
    <xsl:if test="s0:EntryLine[s0:LineNumber/text()=$lineNo and s0:HarmonisedCode/text()!='']">
      <xsl:element name="EntryLines">
        <xsl:element name="LineNo">
          <xsl:value-of select="$lineNo"/>
        </xsl:element>
        <xsl:variable name="invoiceLines" select="../../../s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice/s0:CommercialInvoiceLineCollection/s0:CommercialInvoiceLine[s0:EntryLineNumber/text()=$lineNo]"/>
        <xsl:for-each select="s0:EntryLine[s0:LineNumber=$lineNo and s0:HarmonisedCode/text()!='']">
          <xsl:element name="EntryLineDetail">
            <xsl:element name="HTSNumber">
              <xsl:value-of select="substring(s0:HarmonisedCode,1,10)"/>
            </xsl:element>
            <xsl:element name="CountryOfOrigin">
              <xsl:value-of select="substring($invoiceLines/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='UC_NKCountryOfOrigin'][1]/s0:Value/text(),1,2)"/>
            </xsl:element>
            <xsl:variable name="tradeAgreementSpecialProgramClaimCode" select="substring($invoiceLines/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='SPI'][1]/s0:Value/text(),1,3)" />
            <xsl:if test="$tradeAgreementSpecialProgramClaimCode != '' and $tradeAgreementSpecialProgramClaimCode != 'N/A'">
              <xsl:element name="TradeAgreementSpecialProgramClaimCode">
                <xsl:value-of select="$tradeAgreementSpecialProgramClaimCode"/>
              </xsl:element>
            </xsl:if>
            <xsl:variable name="totalCustomsValues" select="number(s0:CustomsValue)"/>
            <xsl:if test="$totalCustomsValues > 0">
              <xsl:element name="EstimatedEnteredValue">
                <xsl:value-of select="$totalCustomsValues"/>
              </xsl:element>
            </xsl:if>
            <xsl:element name="LineDutyAmount">
              <xsl:value-of select="format-number(sum(s0:EntryLineChargeCollection/s0:EntryLineCharge[s0:Type/s0:Code/text()='DTY'][1]/s0:Amount/text()), '0.##')"/>
            </xsl:element>
            <xsl:element name="IRTaxAmount">
              <xsl:value-of select="format-number(sum(s0:EntryLineChargeCollection/s0:EntryLineCharge[s0:Type/s0:Code/text()='022' or s0:Type/s0:Code/text()='016' or s0:Type/s0:Code/text()='017' or s0:Type/s0:Code/text()='018']/s0:Amount/text()), '0.##')"/>
            </xsl:element>
            <xsl:element name="CommodityFeeAmount">
              <xsl:value-of select="format-number(sum(s0:EntryLineChargeCollection/s0:EntryLineCharge
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
                              or s0:Type/s0:Code/text()='104']/s0:Amount/text()), '0.##')"/>
            </xsl:element>
            <xsl:element name="OtherFeeAmount">
              <xsl:value-of select="format-number(sum(s0:EntryLineChargeCollection/s0:EntryLineCharge[s0:Type/s0:Code/text()='499' or s0:Type/s0:Code/text()='501']/s0:Amount/text()), '0.##')"/>
            </xsl:element>
            <xsl:variable name="addDutyAmount" select="number(s0:EntryLineChargeCollection/s0:EntryLineCharge[s0:Type/s0:Code/text()='ADD']/s0:Amount/text())"/>
            <xsl:choose>
              <xsl:when test="$addDutyAmount > 0">
                <xsl:variable name="addCaseNoLines" select="$invoiceLines[s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ADDCaseNo' and s0:Value/text()!='']]"/>
                <xsl:element name="EntryLinesADCVD">
                  <xsl:variable name="caseNumber" select="substring($addCaseNoLines/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ADDCaseNo'][1]/s0:Value/text(),1,10)" />
                  <xsl:if test="$caseNumber">
                    <xsl:element name="CaseNumber">
                      <xsl:value-of select="$caseNumber"/>
                    </xsl:element>
                  </xsl:if>
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
                  <xsl:variable name="originalADDCaseDepositRateValue" select="$addCaseNoLines/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ADDCaseDepositRate'][1]/s0:Value/text()" />
                  <xsl:variable name="addCaseDepositRate" select="userCSharp:StringDecimalMaxAllowed(normalize-space($originalADDCaseDepositRateValue), 6, 2)" />
                  <xsl:if test="$addCaseDepositRate">
                    <xsl:element name="CaseDepositRate">
                      <xsl:value-of select="$addCaseDepositRate"/>
                    </xsl:element>
                  </xsl:if>
                  <xsl:variable name="caseRateTypeQualifier" select="substring($addCaseNoLines/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ADDDepositRateIndicator'][1]/s0:Value/text(),1,1)" />
                  <xsl:if test="$caseRateTypeQualifier">
                    <xsl:element name="CaseRateTypeQualifier">
                      <xsl:value-of select="$caseRateTypeQualifier"/>
                    </xsl:element>
                  </xsl:if>
                  <xsl:variable name="adcvdValueOfGoods" select="format-number(sum($addCaseNoLines/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ADDDepositValue']/s0:Value/text()), '0.#####')" />
                  <xsl:if test="$adcvdValueOfGoods > 0">
                    <xsl:element name="ADCVDValueOfGoods">
                      <xsl:value-of select="$adcvdValueOfGoods"/>
                    </xsl:element>
                  </xsl:if>
                  <xsl:element name="ADCVDQuanity">
                    <xsl:value-of select="format-number(sum($addCaseNoLines/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ADDQty']/s0:Value/text()), '0.#####')"/>
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
                  <xsl:element name="NonReimbursementIndicator">Y</xsl:element>
                  <xsl:variable name="declarationIdentifier" select="substring($addCaseNoLines/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ADDDecID'][1]/s0:Value/text(),1,10)" />
                  <xsl:if test="$declarationIdentifier">
                    <xsl:element name="DeclarationIdentifier">
                      <xsl:value-of select="$declarationIdentifier"/>
                    </xsl:element>
                  </xsl:if>
                </xsl:element>
              </xsl:when>
            </xsl:choose>

            <xsl:variable name="cvdDutyAmount" select="number(s0:EntryLineChargeCollection/s0:EntryLineCharge[s0:Type/s0:Code/text()='CVD']/s0:Amount/text())"/>
            <xsl:choose>
              <xsl:when test="$cvdDutyAmount > 0">
                <xsl:variable name="cvdCaseNoLines" select="$invoiceLines[s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CVDCaseNo' and s0:Value/text()!='']]"/>
                <xsl:element name="EntryLinesADCVD">
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
                  <xsl:variable name="originalCVDCaseDepositRateValue" select="$cvdCaseNoLines/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CVDCaseDepositRate'][1]/s0:Value/text()" />
                  <xsl:variable name="cvdCaseDepositRate" select="userCSharp:StringDecimalMaxAllowed(normalize-space($originalCVDCaseDepositRateValue), 6, 2)" />
                  <xsl:if test="$cvdCaseDepositRate">
                    <xsl:element name="CaseDepositRate">
                      <xsl:value-of select="$cvdCaseDepositRate"/>
                    </xsl:element>
                  </xsl:if>
                  <xsl:element name="CaseRateTypeQualifier">
                    <xsl:value-of select="substring($cvdCaseNoLines/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CVDDepositRateIndicator'][1]/s0:Value/text(),1,1)"/>
                  </xsl:element>
                  <xsl:variable name="adcvdValueOfGoods" select="number(sum($cvdCaseNoLines/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CVDDepositValue'][1]/s0:Value/text()))" />
                  <xsl:if test="$adcvdValueOfGoods > 0">
                    <xsl:element name="ADCVDValueOfGoods">
                      <xsl:value-of select="$adcvdValueOfGoods"/>
                    </xsl:element>
                  </xsl:if>
                  <xsl:element name="ADCVDQuanity">
                    <xsl:value-of select="format-number(sum($cvdCaseNoLines/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CVDQty'][1]/s0:Value/text()), '0.#####')"/>
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
                  <xsl:element name="NonReimbursementIndicator">Y</xsl:element>
                </xsl:element>
              </xsl:when>
            </xsl:choose>

            <xsl:call-template name="GetPGAElement">
              <xsl:with-param name="invoiceLines" select="$invoiceLines" />
              <xsl:with-param name="pgaKey" select="'AMSInd'" />
              <xsl:with-param name="pgaDisclaimReasonKey" select="'AMSDisclaimReason'" />
              <xsl:with-param name="pgaCode" select="'AMS'" />
            </xsl:call-template>

            <xsl:call-template name="GetPGAElement">
              <xsl:with-param name="invoiceLines" select="$invoiceLines" />
              <xsl:with-param name="pgaKey" select="'ATFInd'" />
              <xsl:with-param name="pgaCode" select="'ATF'" />
            </xsl:call-template>

            <xsl:call-template name="GetPGAElement">
              <xsl:with-param name="invoiceLines" select="$invoiceLines" />
              <xsl:with-param name="pgaKey" select="'CPSCInd'" />
              <xsl:with-param name="pgaDisclaimReasonKey" select="'CPSCDisclaimReason'" />
              <xsl:with-param name="pgaCode" select="'CPS'" />
            </xsl:call-template>

            <xsl:call-template name="GetPGAElement">
              <xsl:with-param name="invoiceLines" select="$invoiceLines" />
              <xsl:with-param name="pgaKey" select="'DDTCInd'" />
              <xsl:with-param name="pgaCode" select="'DTC'" />
            </xsl:call-template>

            <xsl:call-template name="GetPGAElement">
              <xsl:with-param name="invoiceLines" select="$invoiceLines" />
              <xsl:with-param name="pgaKey" select="'DEAInd'" />
              <xsl:with-param name="pgaDisclaimReasonKey" select="'DEADisclaimReason'" />
              <xsl:with-param name="pgaCode" select="'DEA'" />
            </xsl:call-template>

            <xsl:call-template name="GetPGAElement">
              <xsl:with-param name="invoiceLines" select="$invoiceLines" />
              <xsl:with-param name="pgaKey" select="'FCCIndicator'" />
              <xsl:with-param name="pgaCode" select="'FCC'" />
            </xsl:call-template>

            <xsl:call-template name="GetPGAElement">
              <xsl:with-param name="invoiceLines" select="$invoiceLines" />
              <xsl:with-param name="pgaKey" select="'FDAIndicator'" />
              <xsl:with-param name="pgaDisclaimReasonKey" select="'FDADisclaimReason'" />
              <xsl:with-param name="pgaCode" select="'FDA'" />
            </xsl:call-template>

            <xsl:call-template name="GetPGAElement">
              <xsl:with-param name="invoiceLines" select="$invoiceLines" />
              <xsl:with-param name="pgaKey" select="'FSISInd'" />
              <xsl:with-param name="pgaDisclaimReasonKey" select="'FSISDisclaimReason'" />
              <xsl:with-param name="pgaCode" select="'FSI'" />
            </xsl:call-template>

            <xsl:call-template name="GetPGAElement">
              <xsl:with-param name="invoiceLines" select="$invoiceLines" />
              <xsl:with-param name="pgaKey" select="'FWSInd'" />
              <xsl:with-param name="pgaDisclaimReasonKey" select="'FWSDisclaimReason'" />
              <xsl:with-param name="pgaCode" select="'FWS'" />
            </xsl:call-template>

            <xsl:variable name="aphisIndCode">
              <xsl:call-template name="GetPGADisclaimerCode">
                <xsl:with-param name="invoiceLines" select="$invoiceLines" />
                <xsl:with-param name="pgaKey" select="'APHISInd'" />
                <xsl:with-param name="pgaDisclaimReasonKey" select="'APHISDisclaimReason'" />
              </xsl:call-template>
            </xsl:variable>
            <xsl:variable name="aphisLaceyIndCode">
              <xsl:call-template name="GetPGADisclaimerCode">
                <xsl:with-param name="invoiceLines" select="$invoiceLines" />
                <xsl:with-param name="pgaKey" select="'LaceyIndicator'" />
                <xsl:with-param name="pgaDisclaimReasonKey" select="'LaceyDisclaimReason'" />
              </xsl:call-template>
            </xsl:variable>
            <xsl:choose>
              <xsl:when test="$aphisIndCode='N' or $aphisLaceyIndCode='N'">
                <xsl:element name="EntryLinesPGA">
                  <xsl:element name="PGACode">APH</xsl:element>
                  <xsl:element name="PGADisclaimerCode">N</xsl:element>
                </xsl:element>
              </xsl:when>
              <xsl:when test="$aphisIndCode='Y' and $aphisIndCode='Y'">
                <xsl:element name="EntryLinesPGA">
                  <xsl:element name="PGACode">APH</xsl:element>
                  <xsl:element name="PGADisclaimerCode">Y</xsl:element>
                </xsl:element>
              </xsl:when>
            </xsl:choose>

            <xsl:call-template name="GetPGAElement">
              <xsl:with-param name="invoiceLines" select="$invoiceLines" />
              <xsl:with-param name="pgaKey" select="'NHTSAIndicator'" />
              <xsl:with-param name="pgaDisclaimReasonKey" select="'NHTDisclaimReason'" />
              <xsl:with-param name="pgaCode" select="'NHT'" />
            </xsl:call-template>

            <xsl:call-template name="GetPGAElement">
              <xsl:with-param name="invoiceLines" select="$invoiceLines" />
              <xsl:with-param name="pgaKey" select="'OMCInd'" />
              <xsl:with-param name="pgaDisclaimReasonKey" select="'OMCDisclaimReason'" />
              <xsl:with-param name="pgaCode" select="'OMC'" />
            </xsl:call-template>

            <xsl:call-template name="GetPGAElement">
              <xsl:with-param name="invoiceLines" select="$invoiceLines" />
              <xsl:with-param name="pgaKey" select="'TTBInd'" />
              <xsl:with-param name="pgaDisclaimReasonKey" select="'TTBDisclaimReason'" />
              <xsl:with-param name="pgaCode" select="'TTB'" />
            </xsl:call-template>

            <xsl:variable name="nmfs370DisclaimerCode">
              <xsl:call-template name="GetPGADisclaimerCode">
                <xsl:with-param name="invoiceLines" select="$invoiceLines" />
                <xsl:with-param name="pgaKey" select="'NMFS370Ind'" />
                <xsl:with-param name="pgaDisclaimReasonKey" select="'NMFS370DisclaimReason'" />
              </xsl:call-template>
            </xsl:variable>
            <xsl:variable name="nmfsAMRDisclaimerCode">
              <xsl:call-template name="GetPGADisclaimerCode">
                <xsl:with-param name="invoiceLines" select="$invoiceLines" />
                <xsl:with-param name="pgaKey" select="'NMFSAMRInd'" />
                <xsl:with-param name="pgaDisclaimReasonKey" select="'NMFSAMRDisclaimReason'" />
              </xsl:call-template>
            </xsl:variable>
            <xsl:variable name="nmfsHMSDisclaimerCode">
              <xsl:call-template name="GetPGADisclaimerCode">
                <xsl:with-param name="invoiceLines" select="$invoiceLines" />
                <xsl:with-param name="pgaKey" select="'NMFSHMSInd'" />
                <xsl:with-param name="pgaDisclaimReasonKey" select="'NMFSHMSDisclaimReason'" />
              </xsl:call-template>
            </xsl:variable>
            <xsl:variable name="nmfsSIMPDisclaimerCode">
              <xsl:call-template name="GetPGADisclaimerCode">
                <xsl:with-param name="invoiceLines" select="$invoiceLines" />
                <xsl:with-param name="pgaKey" select="'NMFSSIMPInd'" />
              </xsl:call-template>
            </xsl:variable>
            <xsl:choose>
              <xsl:when test="$nmfs370DisclaimerCode='N' or $nmfsAMRDisclaimerCode='N' or $nmfsHMSDisclaimerCode='N' or $nmfsSIMPDisclaimerCode='N'">
                <xsl:element name="EntryLinesPGA">
                  <xsl:element name="PGACode">NMF</xsl:element>
                  <xsl:element name="PGADisclaimerCode">N</xsl:element>
                </xsl:element>
              </xsl:when>
              <xsl:when test="$nmfs370DisclaimerCode='Y' or $nmfsAMRDisclaimerCode='Y' or $nmfsHMSDisclaimerCode='Y'">
                <xsl:element name="EntryLinesPGA">
                  <xsl:element name="PGACode">NMF</xsl:element>
                  <xsl:element name="PGADisclaimerCode">Y</xsl:element>
                </xsl:element>
              </xsl:when>
            </xsl:choose>
          
            <xsl:variable name="odsDisclaimerCode">
              <xsl:call-template name="GetPGADisclaimerCode">
                <xsl:with-param name="invoiceLines" select="$invoiceLines" />
                <xsl:with-param name="pgaKey" select="'ODSInd'" />
                <xsl:with-param name="pgaDisclaimReasonKey" select="'ODSDisclaimReason'" />
              </xsl:call-template>
            </xsl:variable>
            <xsl:variable name="pstDisclaimerCode">
              <xsl:call-template name="GetPGADisclaimerCode">
                <xsl:with-param name="invoiceLines" select="$invoiceLines" />
                <xsl:with-param name="pgaKey" select="'PSTIndicator'" />
                <xsl:with-param name="pgaDisclaimReasonKey" select="'PSTDisclaimReason'" />
              </xsl:call-template>
            </xsl:variable>
            <xsl:variable name="tscaDisclaimerCode">
              <xsl:call-template name="GetPGADisclaimerCode">
                <xsl:with-param name="invoiceLines" select="$invoiceLines" />
                <xsl:with-param name="pgaKey" select="'TSCAInd'" />
                <xsl:with-param name="pgaDisclaimReasonKey" select="'TSCADisclaimReason'" />
              </xsl:call-template>
            </xsl:variable>
            <xsl:variable name="vneDisclaimerCode">
              <xsl:call-template name="GetPGADisclaimerCode">
                <xsl:with-param name="invoiceLines" select="$invoiceLines" />
                <xsl:with-param name="pgaKey" select="'VNEInd'" />
                <xsl:with-param name="pgaDisclaimReasonKey" select="'VNEDisclaimReason'" />
              </xsl:call-template>
            </xsl:variable>
            <xsl:choose>
              <xsl:when test="$odsDisclaimerCode='N' or $pstDisclaimerCode='N' or $tscaDisclaimerCode='N' or $vneDisclaimerCode='N'">
                <xsl:element name="EntryLinesPGA">
                  <xsl:element name="PGACode">EPA</xsl:element>
                  <xsl:element name="PGADisclaimerCode">N</xsl:element>
                </xsl:element>
              </xsl:when>
              <xsl:when test="$odsDisclaimerCode='Y' or $pstDisclaimerCode='Y' or $tscaDisclaimerCode='Y' or $vneDisclaimerCode='Y'">
                <xsl:element name="EntryLinesPGA">
                  <xsl:element name="PGACode">EPA</xsl:element>
                  <xsl:element name="PGADisclaimerCode">Y</xsl:element>
                </xsl:element>
              </xsl:when>
            </xsl:choose>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:if>

    <xsl:variable name="nextLineNo" select="number($lineNo) + 1"/>
    <xsl:if test="s0:EntryLine[s0:LineNumber/text()=$nextLineNo]">
      <xsl:call-template name="EntryLineLoop">
        <xsl:with-param name="lineNo" select="$nextLineNo"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name="GetPGADisclaimerCode">
    <xsl:param name="invoiceLines"/>
    <xsl:param name="pgaKey"/>
    <xsl:param name="pgaDisclaimReasonKey"/>
    <xsl:variable name="indicator" select="substring($invoiceLines/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()=$pgaKey][1]/s0:Value/text(),1,1)"/>
    <xsl:choose>
      <xsl:when test="$indicator='D'">
        <xsl:value-of select="'N'" />
      </xsl:when>
      <xsl:when test="$indicator='C'">
        <xsl:variable name="disclaimReason" select="substring($invoiceLines/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()=$pgaDisclaimReasonKey][1]/s0:Value/text(),1,1)"/>
        <xsl:if test="$disclaimReason='A' or $disclaimReason='B'">
          <xsl:value-of select="'Y'" />
        </xsl:if>
      </xsl:when>
    </xsl:choose>
  </xsl:template>
  
  <xsl:template name="GetPGAElement">
    <xsl:param name="invoiceLines"/>
    <xsl:param name="pgaKey"/>
    <xsl:param name="pgaDisclaimReasonKey"/>
    <xsl:param name="pgaCode"/>
    <xsl:variable name="indicator" select="substring($invoiceLines/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()=$pgaKey][1]/s0:Value/text(),1,1)"/>
    <xsl:choose>
      <xsl:when test="$indicator='D'">
        <xsl:element name="EntryLinesPGA">
          <xsl:element name="PGACode">
            <xsl:value-of select="$pgaCode"/>
          </xsl:element>
          <xsl:element name="PGADisclaimerCode">N</xsl:element>
        </xsl:element>
      </xsl:when>
      <xsl:when test="$indicator='C'">
        <xsl:variable name="disclaimReason" select="substring($invoiceLines/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()=$pgaDisclaimReasonKey][1]/s0:Value/text(),1,1)"/>
        <xsl:if test="$disclaimReason='A' or $disclaimReason='B'">
          <xsl:element name="EntryLinesPGA">
            <xsl:element name="PGACode">
              <xsl:value-of select="$pgaCode"/>
            </xsl:element>
            <xsl:element name="PGADisclaimerCode">Y</xsl:element>
          </xsl:element>
        </xsl:if>
      </xsl:when>
    </xsl:choose>
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

public string Base64String(string input)
{
  var bytes = System.Text.Encoding.UTF8.GetBytes(input);
  return Convert.ToBase64String(bytes);
}
        ]]>
  </msxsl:script>
</xsl:stylesheet>

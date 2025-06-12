<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
  xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var"
  xmlns:s0="http://www.cargowise.com/Schemas/Universal/2011/11"
  xmlns:siiLR="https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/ssii/fact/ws/SuministroLR.xsd"
  xmlns:sii="https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/ssii/fact/ws/SuministroInformacion.xsd"
  xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
  xmlns:ScriptNS3="http://schemas.microsoft.com/BizTalk/2003/ScriptNS3"
  xmlns:ScriptNS4="http://schemas.microsoft.com/BizTalk/2003/ScriptNS4"
  xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp"
  xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
  exclude-result-prefixes="msxsl var s0 ScriptNS0 ScriptNS3 ScriptNS4 userCSharp"
  version="1.0">

  <xsl:output omit-xml-declaration="yes" indent="yes" version="1.0" method="xml" />

  <!-- #region MapUniTrans2SII_Common.xsl -->
  <xsl:variable name="SII_IDVersion" select="'1.0'"/>
  <xsl:variable name="DecimalNumberFormat" select="'0.00'"/>
  <xsl:variable name="Exempt" select="'EXEMPT'"/>
  <xsl:variable name="NotReport" select="'NOTREPORT'"/>
  <xsl:variable name="SpainCountryCode" select="'ES'"/>
  <xsl:variable name="TipoComunicacion" select="'A0'"/>
  <xsl:variable name="SubscriptionType" select="'ESCMSG'"/>

  <xsl:variable name="recipientID" select="ScriptNS3:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="senderID" select="ScriptNS3:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>

  <xsl:variable name="EUCountryLookUp">
    <Country Code="SE" Name="Swede" />
    <Country Code="UK" Name="United Kingdom" />
    <Country Code="DE" Name="Germany" />
    <Country Code="BE" Name="Belgium" />
    <Country Code="FI" Name="Finland" />
    <Country Code="MT" Name="Malta" />
    <Country Code="HR" Name="Croatia" />
    <Country Code="CZ" Name="Czech Republic" />
    <Country Code="LT" Name="Lithuania" />
    <Country Code="CY" Name="Cyprus" />
    <Country Code="HU" Name="Hungary" />
    <Country Code="FR" Name="France" />
    <Country Code="SI" Name="Slovenia" />
    <Country Code="ES" Name="Spain" />
    <Country Code="EL" Name="Greece" />
    <Country Code="RO" Name="Romania" />
    <Country Code="PL" Name="Poland" />
    <Country Code="SK" Name="Slovakia" />
    <Country Code="LU" Name="Luxembourg" />
    <Country Code="LV" Name="Latvia" />
    <Country Code="IE" Name="Ireland" />
    <Country Code="AT" Name="Austria" />
    <Country Code="PT" Name="Portugal" />
    <Country Code="EE" Name="Estonia" />
    <Country Code="NL" Name="Netherlands" />
    <Country Code="BG" Name="Bulgaria" />
    <Country Code="DK" Name="Denmark" />
    <Country Code="IT" Name="Italy" />
  </xsl:variable>
  <xsl:variable name="EUCountryLookUpNodeSet" select="msxsl:node-set($EUCountryLookUp)" />

  <!-- #region Header -->
  <xsl:template name="Cabecera">
    <xsl:param name="CompanyName" />
    <xsl:param name="NIF" />

    <sii:Cabecera>
      <sii:IDVersionSii>
        <xsl:value-of select="$SII_IDVersion" />
      </sii:IDVersionSii>
      <sii:Titular>
        <sii:NombreRazon>
          <xsl:value-of select="$CompanyName" />
        </sii:NombreRazon>
        <sii:NIF>
          <xsl:value-of select="$NIF" />
        </sii:NIF>
      </sii:Titular>
      <sii:TipoComunicacion>
        <xsl:value-of select="$TipoComunicacion"/>
      </sii:TipoComunicacion>
    </sii:Cabecera>

  </xsl:template>
  <!-- #endregion Header -->

  <!-- IDFactura - Issued Invoice -->
  <!-- Issuer GST registration number, invoice number and invoice issued date -->
  <xsl:template name="IDFactura_Issued_Invoice">
    <xsl:param name="NIF" />
    <xsl:param name="InvoiceNumber" />
    <xsl:param name="TransactionDate" />
    <xsl:param name="OriginalReferenceExists" select="false()"/>

    <siiLR:IDFactura>
      <sii:IDEmisorFactura>
        <sii:NIF>
          <xsl:value-of select="$NIF" />
        </sii:NIF>
      </sii:IDEmisorFactura>

      <sii:NumSerieFacturaEmisor>
        <xsl:value-of select="$InvoiceNumber" />
      </sii:NumSerieFacturaEmisor>

      <!-- This is used when sending grouped simplified invoices.	 Not relevant to CW1.
			<sii:NumSerieFacturaEmisorResumenFin></sii:NumSerieFacturaEmisorResumenFin> -->

      <sii:FechaExpedicionFacturaEmisor>
        <xsl:call-template name="TransactionDate_2_fecha">
          <xsl:with-param name="TransactionDate" select="$TransactionDate" />
        </xsl:call-template>
      </sii:FechaExpedicionFacturaEmisor>

    </siiLR:IDFactura>
  </xsl:template>

  <!-- InvoiceType -->
  <xsl:template name="TipoFactura_InvoiceType" >
    <xsl:param name="TransactionType" />

    <!--
		When transaction type is ""INV"", set to F1.
		When transaction type is ""CRD"" set to R4.
		Else, set to F6. -->
    <sii:TipoFactura>
      <xsl:choose>
        <xsl:when test="$TransactionType='INV'">F1</xsl:when>
        <xsl:when test="$TransactionType='CRD'">R4</xsl:when>
        <xsl:otherwise>F6</xsl:otherwise>
      </xsl:choose>
    </sii:TipoFactura>

    <!-- is it an amending invoice? -->
    <xsl:if test="$TransactionType='CRD'">
      <sii:TipoRectificativa>
        <xsl:value-of select="'I'"/>
      </sii:TipoRectificativa>
    </xsl:if>
  </xsl:template>

  <!-- when the recipient is NOT in Spain, then fill out IDOtro details, NOT NIF -->
  <!-- IDOtro and NIF are exclusive of each other, only one is allowed -->
  <xsl:template name="IDFactura_IDOtro">
    <siiLR:IDFactura>
      <sii:IDOtro>
        <!-- Country Code -->
        <sii:CoddigoPais></sii:CoddigoPais>
        <!-- Type of ID code -->
        <sii:IDType></sii:IDType>
        <!-- 
			 ID Number
			 Main Registration Number of the AP Org
			 When element 'CodigoPais' has value, then retrieve Main Registration Number of AP Org in Registration Number Collection.
			 Else, leave blank. -->
        <sii:ID></sii:ID>
      </sii:IDOtro>
    </siiLR:IDFactura>
  </xsl:template>

  <!-- transaction date -->
  <xsl:template name="PeriodoImpositivo">
    <xsl:param name="TransactionDate" />

    <sii:PeriodoImpositivo>
      <!-- Transaction Date Year and Month from 2017-05-18T06:48:00 -->
      <sii:Ejercicio>
        <xsl:value-of select="substring($TransactionDate, 1, 4)" />
      </sii:Ejercicio>
      <sii:Periodo>
        <xsl:value-of select="substring($TransactionDate, 6, 2)" />
      </sii:Periodo>
    </sii:PeriodoImpositivo>
  </xsl:template>

  <!-- given a country code, look up in the European Union countries table, returns the same country code if on the list, otherwise blank string -->
  <!-- use this template to determine if a given country code is an Intra-community trade -->
  <xsl:template name="TaxRegime">
    <xsl:param name="CountryCode"/>
    <xsl:variable name="senderClientCode" select="$senderID" />
    <xsl:variable name="recipientClientCode" select="$recipientID" />
    <xsl:variable name="transformationName" select="'CW1-to-SII transformation'" />
    <xsl:variable name="codeSetName" select="'EUCountryCode'" />
    <xsl:variable name="resultFieldName" select="'CountryName'" />
    <xsl:variable name="ESCompliance" select="'ESCompliance'" />

    <xsl:variable name="matchingCountryName">
      <xsl:value-of select="ScriptNS0:GetRecipientCode($ESCompliance, $ESCompliance, $transformationName, $codeSetName, $resultFieldName, $CountryCode)"/>
      <!--<xsl:value-of select="$EUCountryLookUpNodeSet/Country[@Code=$CountryCode]/@Name" />-->
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="$matchingCountryName = 'Spain'">
        <xsl:value-of select="'ES'"/>
      </xsl:when>
      <xsl:when test="$matchingCountryName = ''">
        <xsl:value-of select="'Else'"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="Intra-Community"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- Format a text value to a number to 2 decimal point -->
  <!-- empty node will render 0.00 -->
  <xsl:template name="FormatNumber_12_2">
    <xsl:param name="value"/>

    <xsl:choose>
      <xsl:when test="string(number($value)) != 'NaN'">
        <xsl:value-of select="format-number($value, $DecimalNumberFormat)" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="format-number(0.0, $DecimalNumberFormat)" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- #region DetalleIVA for reverse tax charges -->
  <xsl:template name="DetalleIVA_RVS">
    <xsl:param name="PostingJournalCollection"/>
    <xsl:param name="taxCode" />
    <xsl:param name="Ledger" />

    <sii:DetalleIVA>
      <!-- Tax Rate -->
      <sii:TipoImpositivo>
        <xsl:call-template name="FormatNumber_12_2">
          <xsl:with-param name="value">
            <xsl:value-of select="$PostingJournalCollection/s0:PostingJournalCollection/s0:PostingJournal[s0:VATTaxID/s0:TaxCode=$taxCode][1]/s0:VATTaxID/s0:TaxRate/text()"/>
          </xsl:with-param>
        </xsl:call-template>
      </sii:TipoImpositivo>

      <!-- When there is at least one Reverse Charge Tax ID, retrieve Local Amount. -->
      <sii:BaseImponible>
        <xsl:call-template name="FormatNumber_12_2">
          <xsl:with-param name="value">
            <xsl:value-of select="sum($PostingJournalCollection/s0:PostingJournalCollection/s0:PostingJournal[s0:VATTaxID/s0:TaxCode=$taxCode]/s0:LocalAmount)"/>
          </xsl:with-param>
        </xsl:call-template>
      </sii:BaseImponible>

      <!-- When there is at least one Reverse Charge Tax ID, retrieve Local GST VAT Amount. -->
      <xsl:choose>
        <xsl:when test="$Ledger='AR'">
          <sii:CuotaRepercutida>
            <xsl:call-template name="FormatNumber_12_2">
              <xsl:with-param name="value">
                <xsl:value-of select="sum($PostingJournalCollection/s0:PostingJournalCollection/s0:PostingJournal[s0:VATTaxID/s0:TaxCode=$taxCode]/s0:LocalGSTVATAmount)"/>
              </xsl:with-param>
            </xsl:call-template>
          </sii:CuotaRepercutida>
        </xsl:when>
        <xsl:when test="$Ledger='AP'">
          <sii:CuotaSoportada>
            <xsl:call-template name="FormatNumber_12_2">
              <xsl:with-param name="value">
                <xsl:value-of select="sum($PostingJournalCollection/s0:PostingJournalCollection/s0:PostingJournal[s0:VATTaxID/s0:TaxCode=$taxCode]/s0:LocalGSTVATAmount)"/>
              </xsl:with-param>
            </xsl:call-template>
          </sii:CuotaSoportada>
        </xsl:when>
      </xsl:choose>

      <!-- Used in special retail regime.	 Not relevant to CW1 users.
				<TipoRecargoEquivalencia></TipoRecargoEquivalencia>
				<CuotaRecargoEquivalencia></CuotaRecargoEquivalencia> -->
    </sii:DetalleIVA>

  </xsl:template>
  <!-- #endregion DetalleIVA for reverse tax charges -->

  <!-- #region DetalleIVA for normal tax charges -->
  <xsl:template name="DetalleIVA">
    <xsl:param name="PostingJournalCollection" />
    <xsl:param name="taxCode" />
    <xsl:param name="Ledger" />
    <xsl:variable name="taxTypeCode" select="$PostingJournalCollection/s0:PostingJournalCollection/s0:PostingJournal[s0:VATTaxID/s0:TaxCode=$taxCode][1]/s0:VATTaxID/s0:TaxType/s0:Code/text()"/>

    <sii:DetalleIVA>
      <!-- Tax Rate -->
      <xsl:if test="$taxTypeCode!='RVS'">
        <sii:TipoImpositivo>
          <xsl:call-template name="FormatNumber_12_2">
            <xsl:with-param name="value">
              <xsl:value-of select="$PostingJournalCollection/s0:PostingJournalCollection/s0:PostingJournal[s0:VATTaxID/s0:TaxCode=$taxCode][1]/s0:VATTaxID/s0:TaxRate/text()"/>
            </xsl:with-param>
          </xsl:call-template>
        </sii:TipoImpositivo>
      </xsl:if>

      <!-- When there is at least one Reverse Charge Tax ID, retrieve Local Amount. -->
      <sii:BaseImponible>
        <xsl:call-template name="FormatNumber_12_2">
          <xsl:with-param name="value">
            <xsl:value-of select="sum($PostingJournalCollection/s0:PostingJournalCollection/s0:PostingJournal[s0:VATTaxID/s0:TaxCode=$taxCode]/s0:LocalAmount)"/>
          </xsl:with-param>
        </xsl:call-template>
      </sii:BaseImponible>

      <!-- When there is at least one Reverse Charge Tax ID, retrieve Local GST VAT Amount. -->
      <xsl:if test="$taxTypeCode!='RVS'">
        <xsl:choose>
          <xsl:when test="$Ledger='AR'">
            <sii:CuotaRepercutida>
              <xsl:call-template name="FormatNumber_12_2">
                <xsl:with-param name="value">
                  <xsl:value-of select="sum($PostingJournalCollection/s0:PostingJournalCollection/s0:PostingJournal[s0:VATTaxID/s0:TaxCode=$taxCode]/s0:LocalGSTVATAmount)"/>
                </xsl:with-param>
              </xsl:call-template>
            </sii:CuotaRepercutida>
          </xsl:when>
          <xsl:when test="$Ledger='AP'">
            <sii:CuotaSoportada>
              <xsl:call-template name="FormatNumber_12_2">
                <xsl:with-param name="value">
                  <xsl:value-of select="sum($PostingJournalCollection/s0:PostingJournalCollection/s0:PostingJournal[s0:VATTaxID/s0:TaxCode=$taxCode]/s0:LocalGSTVATAmount)"/>
                </xsl:with-param>
              </xsl:call-template>
            </sii:CuotaSoportada>
          </xsl:when>
        </xsl:choose>
      </xsl:if>

      <!-- Used in special retail regime.	 Not relevant to CW1 users.
				<TipoRecargoEquivalencia></TipoRecargoEquivalencia>
				<CuotaRecargoEquivalencia></CuotaRecargoEquivalencia> -->
    </sii:DetalleIVA>
  </xsl:template>
  <!-- #endregion DetalleIVA for normal tax charges -->

  <xsl:template name="TransactionDate_2_PeriodoImpositivo">
    <xsl:param name="TransactionDate" />

    <!-- Transaction Date -->
    <sii:PeriodoImpositivo>
      <!-- Transaction Date Year and Month from 2017-05-18T06:48:00 -->
      <sii:Ejercicio>
        <xsl:value-of select="substring($TransactionDate, 1, 4)" />
      </sii:Ejercicio>
      <sii:Periodo>
        <xsl:value-of select="substring($TransactionDate, 6, 2)" />
      </sii:Periodo>
    </sii:PeriodoImpositivo>

  </xsl:template>

  <!-- input 2017-10-11T00:00:00 -->
  <!--	<pattern value="\d{2,2}-\d{2,2}-\d{4,4}"/> -->
  <xsl:template name="TransactionDate_2_fecha">
    <xsl:param name="TransactionDate" />
    <xsl:value-of select="concat(substring($TransactionDate, 9, 2), '-', substring($TransactionDate, 6, 2), '-', substring($TransactionDate, 1, 4))" />
  </xsl:template>

  <!-- #endregion MapUniTrans2SII_Common.xsl -->

  <!-- #region AP - Account Payables - Received Invoices -->
  <xsl:key name="TaxCode" match="s0:TaxCode" use="." />
  <xsl:key name="RVS_TaxCode" match="s0:TaxCode" use="." />

  <!-- Account Payable - Received Invoices -->
  <xsl:template match="s0:UniversalTransaction" mode="AP">
    <xsl:variable name="CompanyCode">
      <xsl:value-of select="./s0:TransactionInfo/s0:DataContext/s0:Company/s0:Code/text()" />
    </xsl:variable>

    <xsl:variable name="CountryCode">
      <xsl:value-of select="./s0:TransactionInfo/s0:DataContext/s0:Company/s0:Country/s0:Code/text()" />
    </xsl:variable>

    <xsl:variable name="CompanyName">
      <xsl:value-of select="./s0:TransactionInfo/s0:BranchAddress/s0:CompanyName/text()" />
    </xsl:variable>

    <xsl:variable name="Ledger" select="./s0:TransactionInfo/s0:Ledger/text()" />
    <xsl:variable name="NIF" select="./s0:TransactionInfo/s0:BranchAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber[1]/s0:Value/text()" />
    <xsl:variable name="InvoiceNumber" select="./s0:TransactionInfo/s0:Number/text()" />
    <xsl:variable name="Description" select="./s0:TransactionInfo/s0:Description/text()" />
    <xsl:variable name="TransactionDate" select="./s0:TransactionInfo/s0:TransactionDate/text()" />
    <xsl:variable name="TransactionType" select="./s0:TransactionInfo/s0:TransactionType/text()" />

    <xsl:variable name="subscribeInvoiceNumber" select="ScriptNS4:InsertSubscriptionValue($SubscriptionType, $senderID, $recipientID, $InvoiceNumber, $Ledger)"/>
    
    <xsl:variable name="IsCreditTransaction">
      <xsl:choose>
        <xsl:when test="$TransactionType='CRD'">
          <xsl:value-of select="true()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="false()"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="OriginalReferenceExists">
      <xsl:choose>
        <xsl:when test="./s0:TransactionInfo/s0:OriginalReference">
          <xsl:value-of select="true()" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="false()" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="AmendingInvoice">
      <xsl:if test="$OriginalReferenceExists">I</xsl:if>
    </xsl:variable>

    <xsl:variable name="IsAmendingInvoice">
      <xsl:choose>
        <xsl:when test="$AmendingInvoice='I'">
          <xsl:value-of select="true()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="false()" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="ContainsAnyIGICTaxCode">
      <xsl:if test="./s0:TransactionInfo/s0:PostingJournalCollection/s0:PostingJournal/s0:VATTaxID/s0:TaxCode[text()='IGIC']">
        <xsl:value-of select="true()" />
      </xsl:if>
    </xsl:variable>

    <xsl:variable name="IsExport">
      <xsl:choose>
        <xsl:when test="$Ledger='AR' and $CountryCode!='ES'">
          <xsl:value-of select="true()" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="false()"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="TranscationCode">
      <xsl:choose>
        <xsl:when test="$ContainsAnyIGICTaxCode">'08'</xsl:when>
        <xsl:when test="$IsExport">'02'</xsl:when>
        <xsl:otherwise>'01'</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="SumLocalAmount">
      <xsl:call-template name="FormatNumber_12_2">
        <xsl:with-param name="value">
          <xsl:value-of select="sum(./s0:TransactionInfo/s0:PostingJournalCollection/s0:PostingJournal/s0:LocalAmount)" />
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="SumLocalAmountExempt">
      <xsl:call-template name="FormatNumber_12_2">
        <xsl:with-param name="value">
          <xsl:value-of select="sum(./s0:TransactionInfo/s0:PostingJournalCollection/s0:PostingJournal[s0:VATTaxID/s0:TaxCode=$Exempt]/s0:LocalAmount)" />
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="ESCounterpartRegistrationNumberNode">
      <xsl:copy-of select="./s0:TransactionInfo/s0:OrganizationAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code='NIF']" />
    </xsl:variable>

    <xsl:variable name="IsRecipientInSpain">
      <xsl:choose>
        <xsl:when test="s0:UniversalTransaction/s0:TransactionInfo/s0:OrganizationAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber/s0:Type/s0:Code/text()='NIF'">
          <xsl:value-of select="true()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="false()"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="ContainsAnyReverseCharge">
      <xsl:choose>
        <xsl:when test="./s0:TransactionInfo/s0:PostingJournalCollection/s0:PostingJournal/s0:VATTaxID[not(s0:TaxCode=$Exempt) and s0:TaxType/s0:Code='RVS']">
          <xsl:value-of select="true()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="false()"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="RecipientCountryCode">
      <!-- If there is at least one Reverse Charge Code, leave blank. -->
      <!-- 
						When there are NO REVERSE CHARGE Tax IDs, look for NIF Registration Number of AP Org under Registration Number Collection.
						If NIF is found under Registration Number Collection, leave blank.
						If no NIF is found, retrieve Country Code of Main Registration Number of AP Org on Registration Number Collection. -->
      <xsl:if test="not($ContainsAnyReverseCharge) and not($ESCounterpartRegistrationNumberNode)">
        <xsl:value-of select="./s0:TransactionInfo/s0:OrganizationAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber[1]/s0:CountryOfIssue/s0:Code/text()" />
      </xsl:if>
    </xsl:variable>

    <!-- Received Invoice Information. -->
    <siiLR:RegistroLRFacturasRecibidas>
      <!-- Transaction Date -->
      <xsl:call-template name="TransactionDate_2_PeriodoImpositivo">
        <xsl:with-param name="TransactionDate" select="$TransactionDate" />
      </xsl:call-template>

      <!-- invoice ID -->
      <siiLR:IDFactura>
        <sii:IDEmisorFactura>

          <!-- Receivable's ORG VAT Registration Number -->
          <!-- If country of Invoice Address of the AP Org is ES, then retrieve NIF Registration Number Value -->
          <xsl:choose>
            <xsl:when test="$IsRecipientInSpain='true'">
              <sii:NIF>
                <xsl:value-of select="./s0:TransactionInfo/s0:OrganizationAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code='NIF']/s0:Value/text()" />
              </sii:NIF>
            </xsl:when>
            <xsl:otherwise>
              <xsl:variable name="IssuerCountryCode">
                <xsl:value-of select="./s0:TransactionInfo/s0:OrganizationAddress/s0:Country/s0:Code/text()"/>
              </xsl:variable>

              <sii:IDOtro>
                <!-- Country code of Registration Number on Config Tab of AP Org -->
                <sii:CodigoPais>
                  <xsl:value-of select="$IssuerCountryCode"/>
                </sii:CodigoPais>

                <!-- If previous element (CodigoPais) has value, then set to 06; otherwise, set to 01. -->
                <sii:IDType>
                  <xsl:choose>
                    <xsl:when test="$IssuerCountryCode!=''">
                      <xsl:value-of select="'06'"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="'01'"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </sii:IDType>

                <!-- ID Number of issuer of invoice -->
                <!-- When element 'CodigoPais' has value, then retrieve Main Registration Number of AP Org in Registration Number Collection. -->
                <sii:ID>
                  <xsl:if test="$IssuerCountryCode!=''">
                    <xsl:value-of select="./s0:TransactionInfo/s0:OrganizationAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber[1]/s0:Value" />
                  </xsl:if>
                </sii:ID>
              </sii:IDOtro>
            </xsl:otherwise>
          </xsl:choose>

        </sii:IDEmisorFactura>

        <!-- Invoice Number -->
        <sii:NumSerieFacturaEmisor>
          <xsl:value-of select="./s0:TransactionInfo/s0:Number/text()"/>
        </sii:NumSerieFacturaEmisor>

        <!--
						Only simplified invoices (Sales tickets)	 are allowed to be grouped.	 This is not relevant to CW1. 
						<NumSerieFacturaEmisorResumenFin></NumSerieFacturaEmisorResumenFin> -->

        <sii:FechaExpedicionFacturaEmisor>
          <xsl:call-template name="TransactionDate_2_fecha">
            <xsl:with-param name="TransactionDate" select="$TransactionDate" />
          </xsl:call-template>
        </sii:FechaExpedicionFacturaEmisor>
      </siiLR:IDFactura>

      <!-- Received invoices -->
      <siiLR:FacturaRecibida>
        <!-- Invoice Type -->
        <xsl:call-template name="TipoFactura_InvoiceType">
          <xsl:with-param name="TransactionType" select="$TransactionType" />
        </xsl:call-template>

        <!-- Grouped Invoices, N/A
						<FacturasAgrupadas>
							<IDFacturaAgrupada>
							<NumSerieFacturaEmisor></NumSerieFacturaEmisor>
							<FechaExpedicionFacturaEmisor></FechaExpedicionFacturaEmisor>
							</IDFacturaAgrupada>
						</FacturasAgrupadas> -->

        <!-- Amending invoice when Original Reference element exists -->
        <!-- 
						 When Original Reference element exists in Universal Transaction XML, then retrieve UniversalTransaction/TransactionInfo/Number
						 Else, leave blank -->
        <xsl:if test="$IsCreditTransaction='true'">
          <sii:FacturasRectificadas>
            <sii:IDFacturaRectificada>
              <sii:NumSerieFacturaEmisor>
                <xsl:value-of select="$InvoiceNumber" />
              </sii:NumSerieFacturaEmisor>
              <sii:FechaExpedicionFacturaEmisor>
                <xsl:call-template name="TransactionDate_2_fecha">
                  <xsl:with-param name="TransactionDate" select="$TransactionDate" />
                </xsl:call-template>
              </sii:FechaExpedicionFacturaEmisor>
            </sii:IDFacturaRectificada>
          </sii:FacturasRectificadas>

          <!-- Amended Amounts -->
          <sii:ImporteRectificacion>
            <sii:BaseRectificada>
              <xsl:value-of select="$SumLocalAmount" />
            </sii:BaseRectificada>
            <sii:CuotaRectificada>
              <!-- Tax rate when invoice is Amendment -->
              <xsl:call-template name="FormatNumber_12_2">
                <xsl:with-param name="value">
                  <xsl:value-of select="./s0:TransactionInfo/s0:PostingJournalCollection/s0:PostingJournal[1]/s0:VATTaxID/s0:TaxRate" />
                </xsl:with-param>
              </xsl:call-template>
            </sii:CuotaRectificada>

            <!-- N/A
							<CuotaRecargoRectificado></CuotaRecargoRectificado>	 -->
          </sii:ImporteRectificacion>
        </xsl:if>

        <!-- Date in which the transaction took place -->
        <!-- ToDo: Accounting team will provide a different value with an update -->
        <sii:FechaOperacion>
          <xsl:call-template name="TransactionDate_2_fecha">
            <xsl:with-param name="TransactionDate" select="$TransactionDate" />
          </xsl:call-template>
        </sii:FechaOperacion>

        <!-- 
						 Tax Regime or Transaction code
							If transaction contains at least one Tax ID that contains IGIC on the tax Code, then set value to "08"
							When the transaction is Intra-community, set value to "09"
							Else, set value to "01" -->
        <!-- Intra-community means that the counterpart is in any country of the European Union -->
        <sii:ClaveRegimenEspecialOTrascendencia>
          <xsl:variable name="TaxRegime">
            <xsl:call-template name="TaxRegime">
              <xsl:with-param name="CountryCode" select="$CountryCode" />
            </xsl:call-template>
          </xsl:variable>

          <xsl:choose>
            <xsl:when test="$ContainsAnyIGICTaxCode">
              <xsl:value-of select="'08'" />
            </xsl:when>
            <xsl:when test="$TaxRegime='Intra-Community'">
              <xsl:value-of select="'09'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'01'"/>
            </xsl:otherwise>
          </xsl:choose>
        </sii:ClaveRegimenEspecialOTrascendencia>

        <!-- N/A
							<ClaveRegimenEspecialOTrascendenciaAdicional1></ClaveRegimenEspecialOTrascendenciaAdicional1>
							<ClaveRegimenEspecialOTrascendenciaAdicional2></ClaveRegimenEspecialOTrascendenciaAdicional2> -->

        <!-- Sum of all Local Amount Lines of the transaction -->
        <sii:ImporteTotal>
          <xsl:value-of select="$SumLocalAmount"/>
        </sii:ImporteTotal>

        <!-- Tax base used by taxpayers under the special cash register regime.	 Not applicable to CW1.
							<BaseImponibleACoste></BaseImponibleACoste>-->

        <!-- Transaction Description -->
        <sii:DescripcionOperacion>
          <xsl:value-of select="$Description"/>
        </sii:DescripcionOperacion>

        <sii:DesgloseFactura>
          <xsl:variable name="ContainsAtLeastOneReverseChargeTax">
            <xsl:choose>
              <xsl:when test="./s0:TransactionInfo/s0:PostingJournalCollection/s0:PostingJournal[s0:VATTaxID/s0:TaxType/s0:Code='RVS']">
                <xsl:value-of select="true()"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="false()"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <!-- list distinct tax codes -->
          <xsl:variable name="TaxCodes">
            <xsl:for-each select="./s0:TransactionInfo/s0:PostingJournalCollection/s0:PostingJournal[s0:VATTaxID/s0:TaxType/s0:Code != 'RVS']/s0:VATTaxID/s0:TaxCode[generate-id() = generate-id(key('TaxCode',.)[1])]">
              <li>
                <xsl:value-of select="."/>
              </li>
            </xsl:for-each>
          </xsl:variable>
          <xsl:variable name="TaxCodesNodeSet" select="msxsl:node-set($TaxCodes)/li" />

          <!-- RVS list distinct reverse charge tax codes -->
          <xsl:variable name="RVS_TaxCodes">
            <xsl:for-each select="./s0:TransactionInfo/s0:PostingJournalCollection/s0:PostingJournal[s0:VATTaxID/s0:TaxType/s0:Code = 'RVS']/s0:VATTaxID/s0:TaxCode[generate-id() = generate-id(key('RVS_TaxCode',.)[1])]">
              <li>
                <xsl:value-of select="."/>
              </li>
            </xsl:for-each>
          </xsl:variable>
          <xsl:variable name="RVS_TaxCodesCodeSet" select="msxsl:node-set($RVS_TaxCodes)/li" />

          <xsl:variable name="PostingJournalCollection">
            <xsl:copy-of select="./s0:TransactionInfo/s0:PostingJournalCollection[s0:PostingJournal/s0:VATTaxID/s0:TaxType/s0:Code!='RVS']"/>
          </xsl:variable>
          <xsl:variable name="PostingJournalCollectionNodeSet" select="msxsl:node-set($PostingJournalCollection)"/>

          <xsl:variable name="RVS_PostingJournalCollection">
            <xsl:copy-of select="./s0:TransactionInfo/s0:PostingJournalCollection[s0:PostingJournal/s0:VATTaxID/s0:TaxType/s0:Code='RVS']"/>
          </xsl:variable>
          <xsl:variable name="RVS_PostingJournalCollectionNodeSet" select="msxsl:node-set($RVS_PostingJournalCollection)"/>

          <xsl:if test="$ContainsAtLeastOneReverseChargeTax='true'">

            <!-- reverse charges -->
            <sii:InversionSujetoPasivo>

              <xsl:for-each select="$RVS_TaxCodesCodeSet">
                <xsl:variable name="taxCode" select="./text()" />
                <xsl:call-template name="DetalleIVA_RVS">
                  <xsl:with-param name="PostingJournalCollection" select="$RVS_PostingJournalCollectionNodeSet" />
                  <xsl:with-param name="taxCode" select="$taxCode" />
                  <xsl:with-param name="Ledger" select="$Ledger" />
                </xsl:call-template>
              </xsl:for-each>

            </sii:InversionSujetoPasivo>

          </xsl:if>

          <xsl:for-each select="$TaxCodesNodeSet">
            <xsl:variable name="taxCode" select="./text()" />
            <!-- ToDo: ask Sofia for confirmation -->
            <xsl:if test="$taxCode!=$NotReport">
              <sii:DesgloseIVA>
                <xsl:call-template name="DetalleIVA">
                  <xsl:with-param name="PostingJournalCollection" select="$PostingJournalCollectionNodeSet" />
                  <xsl:with-param name="taxCode" select="$taxCode"/>
                  <xsl:with-param name="Ledger" select="$Ledger"/>
                </xsl:call-template>
              </sii:DesgloseIVA>
            </xsl:if>
          </xsl:for-each>

        </sii:DesgloseFactura>

        <sii:Contraparte>
          <!-- Full Name of the AP Org issuing the invoice -->
          <sii:NombreRazon>
            <xsl:value-of select="./s0:TransactionInfo/s0:OrganizationAddress/s0:CompanyName/text()"/>
          </sii:NombreRazon>

          <!-- Legal Representative.	 Not relevant to CW1 users.
							<NIFRepresentante></NIFRepresentante> -->

          <xsl:variable name="NIF_Contraparte">
            <xsl:value-of select="./s0:TransactionInfo/s0:OrganizationAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code='NIF']/Value/text()"/>
          </xsl:variable>
          <xsl:variable name="APCountryCode">
            <xsl:value-of select="./s0:TransactionInfo/s0:OrganizationAddress/s0:Country/s0:Code/text()"/>
          </xsl:variable>

          <xsl:if test="$NIF_Contraparte!=''">
            <!-- NIF Registration Number of AP Org. -->
            <sii:NIF>
              <xsl:value-of select="$NIF_Contraparte"/>
            </sii:NIF>
          </xsl:if>

          <sii:IDOtro>
            <xsl:if test="$NIF_Contraparte=''">
              <!-- Country Code of AP Org. -->
              <sii:CodigoPais>
                <xsl:value-of select="$APCountryCode"/>
              </sii:CodigoPais>
            </xsl:if>

            <!-- Type of ID code When 'NIF' has value, set to 01; else, set to 06. -->
            <sii:IDType>
              <xsl:choose>
                <xsl:when test="$NIF_Contraparte!=''">
                  <xsl:value-of select="'01'"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="'06'"/>
                </xsl:otherwise>
              </xsl:choose>
            </sii:IDType>

            <!-- When element 'CodigoPais' has value, then retrieve Main Registration Number of AP Org in Registration Number Collection. -->
            <sii:ID>
              <xsl:if test="$APCountryCode!=''">
                <xsl:value-of select="./s0:TransactionInfo/s0:OrganizationAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber/s0:Value/text()"/>
              </xsl:if>
            </sii:ID>
          </sii:IDOtro>

        </sii:Contraparte>

        <!-- post date -->
        <sii:FechaRegContable>
          <xsl:call-template name="TransactionDate_2_fecha">
            <xsl:with-param name="TransactionDate" select="./s0:TransactionInfo/s0:PostDate/text()"/>
          </xsl:call-template>
        </sii:FechaRegContable>

        <!-- Deductible percentage of the VAT, recoverable amount of the total -->
        <!-- Calculate using OSGSTVATAmount and RecoverableGSTVATPercentage.
            If <RecoverableGSTVATPercentage> exists, multiply it by <OSGSTVATAmount>
            If <RecoverableGSTVATPercentage> does not exist, then populate with <OSGSTVATAmount>-->
        <xsl:variable
          name="OSGSTVATAmount"
          select="/s0:UniversalInterchange/s0:Body/s0:UniversalTransaction/s0:TransactionInfo/s0:PostingJournalCollection/s0:PostingJournal/s0:OSGSTVATAmount" />

        <xsl:variable
          name="RecoverableGSTVATPercentage"
          select="/s0:UniversalInterchange/s0:Body/s0:UniversalTransaction/s0:TransactionInfo/s0:PostingJournalCollection/s0:PostingJournal/s0:RecoverableGSTVATPercentage/text()"/>

        <sii:CuotaDeducible>
          <xsl:call-template name="FormatNumber_12_2">
            <xsl:with-param name="value">
              <xsl:choose>
                <xsl:when test="$RecoverableGSTVATPercentage=''">
                  <xsl:value-of select="$OSGSTVATAmount"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$OSGSTVATAmount * $RecoverableGSTVATPercentage div 100"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:with-param>
          </xsl:call-template>
        </sii:CuotaDeducible>

      </siiLR:FacturaRecibida>
    </siiLR:RegistroLRFacturasRecibidas>

  </xsl:template>
  <!-- #endregion AP - Account Payables -->

  <!-- #region AR - Account Receivables - Issued Invoices -->
  <xsl:key name="ExemptTaxMessageCode" match="s0:TaxMessageCode" use="." />
  <xsl:key name="TaxCode" match="s0:TaxCode" use="." />
  <xsl:key name="RVS_TaxCode" match="s0:TaxCode" use="." />

  <!-- Account Receivable - Issued Invoices -->
  <xsl:template match="s0:UniversalTransaction" mode="AR">

    <!-- #region Variables -->
    <xsl:variable name="CompanyCode">
      <xsl:value-of select="./s0:TransactionInfo/s0:DataContext/s0:Company/s0:Code/text()" />
    </xsl:variable>

    <xsl:variable name="CountryCode">
      <xsl:value-of select="./s0:TransactionInfo/s0:DataContext/s0:Company/s0:Country/s0:Code/text()" />
    </xsl:variable>

    <xsl:variable name="CompanyName">
      <xsl:value-of select="./s0:TransactionInfo/s0:DataContext/s0:Company/s0:Name/text()"/>
    </xsl:variable>

    <xsl:variable name="Ledger" select="./s0:TransactionInfo/s0:Ledger/text()" />
    <xsl:variable name="NIF" select="./s0:TransactionInfo/s0:BranchAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber[1]/s0:Value/text()" />
    <xsl:variable name="InvoiceNumber" select="./s0:TransactionInfo/s0:Number/text()" />
    <xsl:variable name="TransactionDate" select="./s0:TransactionInfo/s0:TransactionDate/text()" />
    <xsl:variable name="TransactionType" select="./s0:TransactionInfo/s0:TransactionType/text()" />
    <xsl:variable name="subscribeInvoiceNumber" select="ScriptNS4:InsertSubscriptionValue($SubscriptionType, $senderID, $recipientID, $InvoiceNumber, $Ledger)"/>

    <xsl:variable name="IsCreditTransaction">
      <xsl:choose>
        <xsl:when test="$TransactionType='CRD'">
          <xsl:value-of select="true()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="false()"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="OriginalReferenceExists">
      <xsl:choose>
        <xsl:when test="./s0:TransactionInfo/s0:OriginalReference">
          <xsl:value-of select="true()" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="false()" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="AmendingInvoice">
      <xsl:if test="$OriginalReferenceExists='true'">I</xsl:if>
    </xsl:variable>

    <xsl:variable name="IsAmendingInvoice">
      <xsl:choose>
        <xsl:when test="$AmendingInvoice='I'">
          <xsl:value-of select="true()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="false()" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="ContainsAnyIGICTaxCode">
      <xsl:if test="./s0:TransactionInfo/s0:PostingJournalCollection/s0:PostingJournal/s0:VATTaxID/s0:TaxCode[text()='IGIC']">
        <xsl:value-of select="true()" />
      </xsl:if>
    </xsl:variable>

    <xsl:variable name="IsExport">
      <xsl:choose>
        <xsl:when test="$Ledger='AR' and $CountryCode!='ES'">
          <xsl:value-of select="true()" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="false()"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="TranscationCode">
      <xsl:choose>
        <xsl:when test="$ContainsAnyIGICTaxCode">'08'</xsl:when>
        <xsl:when test="$IsExport">'02'</xsl:when>
        <xsl:otherwise>'01'</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="SumLocalAmount">
      <xsl:call-template name="FormatNumber_12_2">
        <xsl:with-param name="value">
          <xsl:value-of select="sum(./s0:TransactionInfo/s0:PostingJournalCollection/s0:PostingJournal/s0:LocalAmount)" />
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="SumLocalAmountExempt">
      <xsl:call-template name="FormatNumber_12_2">
        <xsl:with-param name="value">
          <xsl:value-of select="sum(./s0:TransactionInfo/s0:PostingJournalCollection/s0:PostingJournal[s0:VATTaxID/s0:TaxCode=$Exempt]/s0:LocalAmount)" />
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="SumLocalTotalAmount">
      <xsl:call-template name="FormatNumber_12_2">
        <xsl:with-param name="value">
          <xsl:value-of select="sum(./s0:TransactionInfo/s0:PostingJournalCollection/s0:PostingJournal/s0:LocalTotalAmount)" />
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="ESCounterpartRegistrationNumberNode">
      <xsl:copy-of select="./s0:TransactionInfo/s0:OrganizationAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code='NIF']" />
    </xsl:variable>

    <xsl:variable name="IsRecipientInSpain">
      <xsl:choose>
        <xsl:when test="./s0:TransactionInfo/s0:OrganizationAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code='NIF']">
          <xsl:value-of select="true()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="false()"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="ContainsAnyReverseCharge">
      <xsl:choose>
        <xsl:when test="./s0:TransactionInfo/s0:PostingJournalCollection/s0:PostingJournal/s0:VATTaxID[not(s0:TaxCode=$Exempt) and s0:TaxType/s0:Code='RVS']">
          <xsl:value-of select="true()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="false()"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="RecipientCountryCode">
      <xsl:value-of select="../s0:TransactionInfo/s0:OrganizationAddress/s0:Country/s0:Code/text()"/>
    </xsl:variable>

    <!-- #endregion Variables -->
    
    <!-- issued invoice information -->
    <siiLR:RegistroLRFacturasEmitidas>
      <!-- Transaction date -->
      <xsl:call-template name="TransactionDate_2_PeriodoImpositivo">
        <xsl:with-param name="TransactionDate" select="$TransactionDate" />
      </xsl:call-template>

      <!-- Issued Invoice -->
      <xsl:call-template name="IDFactura_Issued_Invoice">
        <!-- GST registration of the issuer company -->
        <xsl:with-param name="NIF" select="$NIF" />
        <xsl:with-param name="InvoiceNumber" select="$InvoiceNumber" />
        <xsl:with-param name="TransactionDate" select="$TransactionDate" />
      </xsl:call-template>

      <!-- issued invoice -->
      <siiLR:FacturaExpedida>
        <!-- TIpoFactura (Invoice Type) -->
        <!-- TipoREctificativa (Amending Invoice) -->
        <xsl:call-template name="TipoFactura_InvoiceType">
          <xsl:with-param name="TransactionType" select="$TransactionType" />
        </xsl:call-template>

        <!-- Grouped Invoices, N/A
							<FacturasAgrupadas>
								<IDFacturaAgrupada>
								<NumSerieFacturaEmisor></NumSerieFacturaEmisor>
								<FechaExpedicionFacturaEmisor></FechaExpedicionFacturaEmisor>
								</IDFacturaAgrupada>
							</FacturasAgrupadas> -->

        <!-- When Original Reference element exists in Universal Transaction XML -->
        <xsl:if test="$OriginalReferenceExists='true'">
          <sii:FacturasRectificadas>
            <sii:IDFacturaRectificada>
              <sii:NumSerieFacturaEmisor>
                <xsl:value-of select="$InvoiceNumber" />
              </sii:NumSerieFacturaEmisor>
              <sii:FechaExpedicionFacturaEmisor>
                <xsl:call-template name="TransactionDate_2_fecha">
                  <xsl:with-param name="TransactionDate" select="$TransactionDate" />
                </xsl:call-template>
              </sii:FechaExpedicionFacturaEmisor>
            </sii:IDFacturaRectificada>
          </sii:FacturasRectificadas>
        </xsl:if>

        <!-- Amended Amounts -->
        <!--
								This is filled out only when the type of Amending Invoice = "S".
								CW1 user base will not issue "S" type amending invoices.
								Do not map. -->
        <!--
							<sii:ImporteRectificacion>
								<sii:BaseRectificada>
									<xsl:value-of select="$SumLocalAmount" />
								</sii:BaseRectificada>
								<sii:CuotaRectificada>
									<xsl:value-of select="./s0:TransactionInfo/s0:PostingJournalCollection/s0:PostingJournal[1]/s0:VATTaxID/s0:TaxRate" />
								</sii:CuotaRectificada>
								
								<sii:CuotaRecargoRectificado />
							</sii:ImporteRectificacion> -->

        <!-- Date in which the transaction took place -->
        <sii:FechaOperacion>
          <xsl:call-template name="TransactionDate_2_fecha">
            <xsl:with-param name="TransactionDate" select="$TransactionDate" />
          </xsl:call-template>
        </sii:FechaOperacion>

        <!-- Tax Regime or Transaction code -->
        <!-- 1.	If transaction contains at least one Tax ID that contains IGIC on the tax Code, then set value to "08"
									2.	When the transaction is EXPORT, set value to "02"
									3.	Else, set value to "01" -->
        <!-- Note: Pending to identify Rent Invoices that would be classified as 11, 12 or 13. -->
        <sii:ClaveRegimenEspecialOTrascendencia>
          <xsl:choose>
            <xsl:when test="$ContainsAnyIGICTaxCode='true'">
              <xsl:value-of select="'08'" />
            </xsl:when>
            <xsl:when test="$IsExport='true'">
              <xsl:value-of select="'02'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'01'"/>
            </xsl:otherwise>
          </xsl:choose>
        </sii:ClaveRegimenEspecialOTrascendencia>

        <!-- Sum of all Local Amount Lines of the transaction -->
        <sii:ImporteTotal>
          <xsl:value-of select="$SumLocalTotalAmount"/>
        </sii:ImporteTotal>

        <!-- Tax base used by taxpayers under the special cash register regime.	Not applicable to CW1. 
								<BaseImponibleACoste></BaseImponibleACoste> -->

        <!-- Transaction Description -->
        <sii:DescripcionOperacion>
          <xsl:value-of select="./s0:TransactionInfo/s0:Description/text()" />
        </sii:DescripcionOperacion>

        <!-- Sell or rent of property, to be filled out only in cases of Rent invoice without withholding tax 
							(When element	<ClaveRegimenEspecialOTranscendencia is 12 or 13) .-->
        <!-- Temporary solution for populating Property ID Number in SII XML File - Spain -->
        <!-- When TaxMessageID begins with “RENTA”, then populate 'ReferenciaCatastral' -->
        <xsl:variable name="RENTA_TaxMessageCode">
          <xsl:value-of select="./s0:TransactionInfo/s0:PostingJournalCollection/s0:PostingJournal/s0:TaxMessageID[starts-with(s0:TaxMessageCode/text(), 'RENTA')][1]/s0:TaxMessageCode/text()"/>
        </xsl:variable>

        <xsl:if test="$RENTA_TaxMessageCode!=''">
          <xsl:variable name="IsRentalPropertyPropertyStatusValid">
            <xsl:call-template name="CheckIsRentalPropertyPropertyStatusValid">
              <xsl:with-param name="TaxMessageCode" select="$RENTA_TaxMessageCode"/>
            </xsl:call-template>            
          </xsl:variable>

          <xsl:if test="$IsRentalPropertyPropertyStatusValid='true'">
            <xsl:variable name="RentalDescription">
              <xsl:value-of select="./s0:TransactionInfo/s0:PostingJournalCollection/s0:PostingJournal/s0:TaxMessageID[s0:TaxMessageCode=$RENTA_TaxMessageCode][1]/s0:Description/text()"/>
            </xsl:variable>

            <sii:DatosInmueble>
              <sii:DetalleInmueble>
                <sii:SituacionInmueble>
                  <xsl:call-template name="RentalPropertyPropertyStatus">
                    <xsl:with-param name="TaxMessageCode" select="$RENTA_TaxMessageCode"/>
                  </xsl:call-template>
                </sii:SituacionInmueble>
                <sii:ReferenciaCatastral>
                  <xsl:value-of select="$RentalDescription"/>
                </sii:ReferenciaCatastral>
              </sii:DetalleInmueble>
            </sii:DatosInmueble>
          </xsl:if>
        </xsl:if>

        <!-- Sum of Local Amount elements -->
        <sii:ImporteTransmisionSujetoAIVA>
          <xsl:value-of select="$SumLocalAmount"/>
        </sii:ImporteTransmisionSujetoAIVA>

        <!-- Invoice issued by the third party -->
        <!-- Do not map.
							<sii:EmitidaPorTerceros></sii:EmitidaPorTerceros>-->

        <!-- N/A More than one recipient. Not relevant to CW1. -->
        <!-- Do not map.
							<VariosDestinatarios></VariosDestinatarios> -->

        <!-- N/A R5 and F4 Invoices are not applicable to CW1 users.	 Not relevant to CW1. -->
        <!-- Do not map.
							<Cupon></Cupon> -->

        <sii:Contraparte>
          <!-- Legal Name of Recipient -->
          <sii:NombreRazon>
            <xsl:value-of select="./s0:TransactionInfo/s0:OrganizationAddress/s0:CompanyName/text()"/>
          </sii:NombreRazon>

          <!-- Do not map.
								<NIFRepresentante></NIFRepresentante> -->

          <!-- Receivable's ORG VAT Registration Number -->
          <!-- When country of Invoice Address of the AR Org is ES, then retrieve NIF Registration Number Value -->
          <xsl:choose>
            <xsl:when test="$IsRecipientInSpain='true'">
              <sii:NIF>
                <xsl:value-of select="./s0:TransactionInfo/s0:OrganizationAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code='NIF']/s0:Value/text()"/>
              </sii:NIF>
            </xsl:when>
            <xsl:otherwise>

              <!-- Non-Spanish recipient -->
              <sii:IDOtro>
                <!-- country code -->
                <sii:CodigoPais>
                  <xsl:value-of select="./s0:TransactionInfo/s0:OrganizationAddress/s0:Country/s0:Code/text()"/>
                </sii:CodigoPais>

                <!-- map to "06" -->
                <sii:IDType>
                  <xsl:value-of select="'06'"/>
                </sii:IDType>

                <!-- Non-Spanish registration number of AR Org -->
                <sii:ID>
                  <xsl:value-of select="./s0:TransactionInfo/s0:OrganizationAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber/s0:Value/text()"/>
                </sii:ID>
              </sii:IDOtro>

            </xsl:otherwise>
          </xsl:choose>
        </sii:Contraparte>

        <!-- type of invoice -->
        <sii:TipoDesglose>

          <!-- invoice details -->
          <sii:DesgloseFactura>

            <!-- subject to tax -->
            <xsl:if test="./s0:TransactionInfo/s0:PostingJournalCollection/s0:PostingJournal/s0:VATTaxID/s0:TaxCode!=$NotReport">
              <sii:Sujeta>

                <!-- If there is at least one Tax Code = EXEMPT -->
                <xsl:if test="./s0:TransactionInfo/s0:PostingJournalCollection/s0:PostingJournal[s0:VATTaxID/s0:TaxCode=$Exempt]">

                  <!-- list distinct TaxMessageCode -->
                  <xsl:variable name="TaxMessageCode">
                    <xsl:for-each select="./s0:TransactionInfo/s0:PostingJournalCollection/s0:PostingJournal[s0:VATTaxID/s0:TaxCode=$Exempt]/s0:TaxMessageID/s0:TaxMessageCode[generate-id() = generate-id(key('ExemptTaxMessageCode',.)[1])]">
                      <li>
                        <xsl:value-of select="."/>
                      </li>
                    </xsl:for-each>
                  </xsl:variable>
                  <xsl:variable name="TaxMessageCodeNodeSet" select="msxsl:node-set($TaxMessageCode)/li" />

                  <xsl:variable name="Exempt_PostingJournalCollection">
                    <xsl:copy-of select="./s0:TransactionInfo/s0:PostingJournalCollection[s0:PostingJournal/s0:VATTaxID/s0:TaxCode=$Exempt]"/>
                  </xsl:variable>
                  <xsl:variable name="Exempt_PostingJournalCollectionNodeSet" select="msxsl:node-set($Exempt_PostingJournalCollection)"/>

                  <!-- exempt tax -->
                  <xsl:for-each select="$TaxMessageCodeNodeSet">
                    <xsl:variable name="messageCode" select="./text()" />

                    <sii:Exenta>
                      <!-- reason -->
                      <sii:CausaExencion>
                        <xsl:value-of select="$messageCode"/>
                      </sii:CausaExencion>

                      <!-- base tax -->
                      <!-- If there are several EXEMPT charges with different Tax messages, this element must be repeated as many times as there are tax messages. -->
                      <sii:BaseImponible>
                        <xsl:value-of select="sum($Exempt_PostingJournalCollectionNodeSet/s0:PostingJournalCollection/s0:PostingJournal[s0:TaxMessageID/s0:TaxMessageCode=$messageCode]/s0:LocalAmount)" />
                      </sii:BaseImponible>
                    </sii:Exenta>
                  </xsl:for-each>
                </xsl:if>

                <!-- list distinct TaxMessageCode -->
                <xsl:variable name="TaxCodesNotExempt">
                  <xsl:for-each select="./s0:TransactionInfo/s0:PostingJournalCollection/s0:PostingJournal[s0:VATTaxID/s0:TaxCode != $Exempt]/s0:VATTaxID/s0:TaxCode[generate-id() = generate-id(key('TaxCode',.)[1])]">
                    <li>
                      <xsl:value-of select="."/>
                    </li>
                  </xsl:for-each>
                </xsl:variable>
                <xsl:variable name="TaxCodesNotExemptNodeSet" select="msxsl:node-set($TaxCodesNotExempt)/li" />

                <xsl:variable name="PostingJournalCollectionNotExempt">
                  <xsl:copy-of select="./s0:TransactionInfo/s0:PostingJournalCollection[s0:PostingJournal/s0:VATTaxID/s0:TaxCode != $Exempt]"/>
                </xsl:variable>
                <xsl:variable name="PostingJournalCollectionNotExemptNodeSet" select="msxsl:node-set($PostingJournalCollectionNotExempt)"/>

                <!-- not exempt -->
                <sii:NoExenta>

                  <!-- obligation code -->
                  <!-- TODO: question, why not using valid values from {S1, S2, S3} -->
                  <sii:TipoNoExenta>
                    <xsl:choose>
                      <xsl:when test="$PostingJournalCollectionNotExemptNodeSet/s0:PostingJournalCollection/s0:PostingJournal/s0:VATTaxID[s0:TaxType/s0:Code='RVS']">
                        <!-- When the Tax ID is Reverse Charge (Type = RVS), map to value 02 -->
                        <xsl:value-of select="'S2'"/>
                      </xsl:when>
                      <xsl:otherwise>
                        <!-- When the Tax ID is NOT Reverse Charge and is NOT EXEMPT, then map to value 01.-->
                        <xsl:value-of select="'S1'"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </sii:TipoNoExenta>

                  <!-- breakdown -->
                  <!-- If there is more than one rate, this element must be repeated as many times as there are rates in the invoice. -->
                  <!-- under each tax code, there could be more than one tax rates -->
                  <sii:DesgloseIVA>
                    <xsl:for-each select="$TaxCodesNotExemptNodeSet">
                      <xsl:variable name="taxCode" select="./text()"/>
                      <xsl:if test="$taxCode!=$NotReport">
                        <xsl:call-template name="DetalleIVA">
                          <xsl:with-param name="PostingJournalCollection" select="$PostingJournalCollectionNotExemptNodeSet" />
                          <xsl:with-param name="taxCode" select="$taxCode" />
                          <xsl:with-param name="Ledger" select="$Ledger"/>
                        </xsl:call-template>
                      </xsl:if>
                    </xsl:for-each>
                  </sii:DesgloseIVA>
                </sii:NoExenta>
              </sii:Sujeta>
            </xsl:if>
            
            <xsl:if test="./s0:TransactionInfo/s0:PostingJournalCollection/s0:PostingJournal/s0:VATTaxID/s0:TaxCode=$NotReport">
              <sii:NoSujeta>
                <!-- Amount in EUR if the invoice is not subject to IVA under art. 7.14 -->
                <!-- Type of exemptions not applicable to logistics industry.	 Not relevant to CW1.-->
                <!-- Do not map.
											<ImportePorArticulos7_14_Otros></ImportePorArticulos7_14_Otros> -->

                <!-- Amount in EUR if the invoice is not subject to VAT due to the recipient being outside the Spanish IVA territory
											This is used when the recipient is intra-community with a Spanish NIF on transactions that are not subject to IVA.-->
                <!-- If TaxID is NOTREPORT, sum all	<LocalTotalAmount> elements of NOTREPORT TaxIDs; else, do not map.-->
                <sii:ImporteTAIReglasLocalizacion>
                  <xsl:call-template name="FormatNumber_12_2">
                    <xsl:with-param name="value">
                      <xsl:value-of select="sum(./s0:TransactionInfo/s0:PostingJournalCollection/s0:PostingJournal[s0:VATTaxID/s0:TaxCode=$NotReport]/s0:LocalTotalAmount)" />
                    </xsl:with-param>
                  </xsl:call-template>
                </sii:ImporteTAIReglasLocalizacion>
              </sii:NoSujeta>
            </xsl:if>

          </sii:DesgloseFactura>

          <!-- The above sii:DesgloseFactura is for Goods and this sii:DesgloseTipoOperacion is for Services, exclusive of each other -->
          <!-- TODO: next phase tasks -->
          <!--<sii:DesgloseTipoOperacion>
						<sii:PrestacionServicios></sii:PrestacionServicios>
				-->
          <!-- Goods. Not relevant to CW1. Do not map.
						<sii:Entrega></sii:Entrega>-->
          <!--
					</sii:DesgloseTipoOperacion>-->
        </sii:TipoDesglose>

      </siiLR:FacturaExpedida>

    </siiLR:RegistroLRFacturasEmitidas>
  </xsl:template>

  <!-- Code that identifies the property status -->
  <xsl:template name="RentalPropertyPropertyStatus">
    <xsl:param name="TaxMessageCode"/>
    <xsl:choose>
      <xsl:when test="starts-with($TaxMessageCode, 'RENTAES')">
        <xsl:value-of select="'01'" />
      </xsl:when>
      <xsl:when test="starts-with($TaxMessageCode, 'RENTAPVN')">
        <xsl:value-of select="'02'" />
      </xsl:when>
      <xsl:when test="starts-with($TaxMessageCode, 'RENTASR')">
        <xsl:value-of select="'03'" />
      </xsl:when>
      <xsl:when test="starts-with($TaxMessageCode, 'RENTAEX')">
        <xsl:value-of select="'04'" />
      </xsl:when>
    </xsl:choose>    
  </xsl:template>

  <xsl:template name="CheckIsRentalPropertyPropertyStatusValid">
    <xsl:param name="TaxMessageCode"/>
    <xsl:choose>
      <xsl:when test="starts-with($TaxMessageCode, 'RENTAES') or starts-with($TaxMessageCode, 'RENTAPVN') or starts-with($TaxMessageCode, 'RENTASR') or starts-with($TaxMessageCode, 'RENTAEX')">
        <xsl:value-of select="true()"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="false()"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- #endregion AR - Account Receivables -->

  <!-- #region Entry Point -->
  <xsl:template match="/">
    <soapenv:Envelope>
      <soapenv:Header/>
      <soapenv:Body>

        <!-- AR - Account Receivable - issued invoices -->
        <xsl:if test="count(/s0:UniversalInterchange/s0:Body/s0:UniversalTransaction[s0:TransactionInfo/s0:Ledger='AR'])>0">
          <xsl:variable name="Legder">
            <xsl:value-of select="'AR'"/>
          </xsl:variable>

          <siiLR:SuministroLRFacturasEmitidas>

            <xsl:call-template name="Cabecera">
              <xsl:with-param name="CompanyName" select="/s0:UniversalInterchange/s0:Body/s0:UniversalTransaction/s0:TransactionInfo[s0:Ledger=$Legder][1]/s0:BranchAddress/s0:CompanyName/text()" />
              <xsl:with-param name="NIF" select="/s0:UniversalInterchange/s0:Body/s0:UniversalTransaction/s0:TransactionInfo[s0:Ledger=$Legder][1]/s0:BranchAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber[1]/s0:Value/text()" />
            </xsl:call-template>

            <!-- AR invoices for-each -->
            <xsl:for-each select="/s0:UniversalInterchange/s0:Body/s0:UniversalTransaction[s0:TransactionInfo/s0:Ledger=$Legder]">
              <xsl:apply-templates select="." mode="AR" />
            </xsl:for-each>

          </siiLR:SuministroLRFacturasEmitidas>
        </xsl:if>

        <!-- AP - Account Payable - received invoices -->
        <xsl:if test="count(/s0:UniversalInterchange/s0:Body/s0:UniversalTransaction[s0:TransactionInfo/s0:Ledger='AP'])>0">
          <xsl:variable name="Legder">
            <xsl:value-of select="'AP'"/>
          </xsl:variable>

          <siiLR:SuministroLRFacturasRecibidas>

            <xsl:call-template name="Cabecera">
              <xsl:with-param name="CompanyName" select="/s0:UniversalInterchange/s0:Body/s0:UniversalTransaction/s0:TransactionInfo[s0:Ledger=$Legder][1]/s0:BranchAddress/s0:CompanyName/text()" />
              <xsl:with-param name="NIF" select="/s0:UniversalInterchange/s0:Body/s0:UniversalTransaction/s0:TransactionInfo[s0:Ledger=$Legder][1]/s0:BranchAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber[1]/s0:Value/text()" />
            </xsl:call-template>

            <!-- AP invoices for-each -->
            <xsl:for-each select="/s0:UniversalInterchange/s0:Body/s0:UniversalTransaction[s0:TransactionInfo/s0:Ledger='AP']">
              <xsl:apply-templates select="." mode="AP" />
            </xsl:for-each>

          </siiLR:SuministroLRFacturasRecibidas>
        </xsl:if>

      </soapenv:Body>
    </soapenv:Envelope>
  </xsl:template>
  <!-- #endregion Entry Point -->

  <!-- #region Scripts -->
  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[
public XPathNodeIterator EventParameters(string parameters)
{
	var doc = new XmlDocument();
	var root = doc.CreateElement("root");
	doc.AppendChild(root);

	foreach (var parameter in parameters.Split('|'))
	{
	 var id_value = parameter.Split('=');
	 if (id_value.Length == 2)
	 {
		var parameterElement = doc.CreateElement("ns0", id_value[0], "http://www.cargowise.com/Schemas/Universal/2012/11");
		root.AppendChild(parameterElement);
		
		var parameterText = doc.CreateTextNode(id_value[1]);
		parameterElement.AppendChild(parameterText);
	 }
	}

	return doc.CreateNavigator().Select("/*/*");
}
]]>
  </msxsl:script>
  <!-- #endregion Scripts -->

</xsl:stylesheet>
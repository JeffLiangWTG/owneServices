<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl ScriptNS0 ScriptNS1 userCSharp" version="1.0"
                xmlns:ns0="http://schemas.microsoft.com/BizTalk/EDI/X12/2006"
                xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
								xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1"
								xmlns:ScriptNS2="http://schemas.microsoft.com/BizTalk/2003/ScriptNS2"
								xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="yes" indent="no" version="1.0" method="xml" />

  <xsl:template match="/">
    <xsl:variable name="SCAC" select="ScriptNS1:GetRecipientCodeUnkeyed('AIGEGNEGN' , 'AIGEGNEGN_FUJ' , 'Fuji Photo 210 - Export A/R Invoices' , 'Defaults' , 'SCAC Code')"/>

    <ns0:X12_00401_210>

      <xsl:variable name="SubShipment" select="(//*[local-name()='Shipment'][contains(./*[local-name()='DataContext']//*[local-name()='DataSource']/*[local-name()='Type'], 'ForwardingShipment') and
                                                                         not(contains(./*[local-name()='DataContext']//*[local-name()='DataSource']/*[local-name()='Type'], 'ForwardingConsol'))] | 
                                          //*[local-name()='SubShipment'][*[local-name()='DataContext']//*[local-name()='DataSource']/*[local-name()='Type'] = 'ForwardingShipment'])[1]"/>

      <ST>
        <ST01>210</ST01>
        <ST02>0000</ST02>
      </ST>
      <ns0:B3>
        <B302>
          <xsl:value-of select="translate(//*[local-name()='JobInvoiceNumber'], '/', '')"/>
        </B302>
        <xsl:if test="$SubShipment/*[local-name()='BookingConfirmationReference'] != ''">
          <B303>
            <xsl:value-of select="$SubShipment/*[local-name()='BookingConfirmationReference']"/>
          </B303>
        </xsl:if>
        <B304>
          <xsl:variable name="IncoTerm" select="$SubShipment/*[local-name()='ShipmentIncoTerm']/*[local-name()='Code']"/>
          <xsl:choose>
            <xsl:when test="$IncoTerm = 'EXW' or $IncoTerm = 'FCA' or $IncoTerm = 'FAS' or $IncoTerm = 'FOB' or $IncoTerm = 'CLT' or $IncoTerm = 'C3P'">
              <xsl:value-of select="'CC'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'PP'"/>
            </xsl:otherwise>
          </xsl:choose>
        </B304>
        <B306>
          <xsl:value-of select="ScriptNS0:ConvertXmlDateString(//*[local-name()='TransactionDate'], 'yyyyMMdd')"/>
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


      <xsl:for-each select="$SubShipment/*[local-name()='LocalProcessing']
		/*[local-name()='OrderNumberCollection']
		/*[local-name()='OrderNumber']">
        <ns0:N9>
          <N901>PO</N901>
          <N902>
            <xsl:call-template name="ApplyCharacterSet">
              <xsl:with-param name="Value" select="*[local-name()='OrderReference']/text()" />
            </xsl:call-template>
          </N902>
        </ns0:N9>
      </xsl:for-each>

      <xsl:if test="$SubShipment/*[local-name()='BookingConfirmationReference']/text()!=''">
        <ns0:N9>
          <N901>PU</N901>
          <N902>
            <xsl:call-template name="ApplyCharacterSet">
              <xsl:with-param name="Value" select="$SubShipment/*[local-name()='BookingConfirmationReference']" />
            </xsl:call-template>
          </N902>
        </ns0:N9>
      </xsl:if>

      <xsl:variable name="ServiceLevel" select="$SubShipment/*[local-name()='ServiceLevel']/*[local-name()='Code']"/>
      <xsl:if test="$ServiceLevel != ''">
        <xsl:element name="ns0:R3">
          <R301>
            <xsl:value-of select="$SCAC"/>
          </R301>
          <R302>B</R302>
          <R310>
            <xsl:value-of select="ScriptNS1:GetRecipientCode('AIGEGNEGN' , 'AIGEGNEGN_FUJ' , 'Fuji Photo 210 - Export A/R Invoices' , 'Service Level' , 'X12 Code', $ServiceLevel)"/>
          </R310>
        </xsl:element>
      </xsl:if>


      <xsl:variable name="Consignee" select="$SubShipment/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress' and 
											*[local-name()='AddressType'] = 'ConsigneeDocumentaryAddress']"/>

      <xsl:variable name="Consignor" select="$SubShipment/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress' and 
											*[local-name()='AddressType'] = 'ConsignorDocumentaryAddress']"/>

      <xsl:variable name="BillToParty" select="//*[local-name()='OrganizationAddress'][1]"/>

      <ns0:N1Loop1>
        <xsl:call-template name="GenerateOrgAddress">
          <xsl:with-param name="Org" select="$Consignee"/>
          <xsl:with-param name="EntityIdCode" select="'CN'"/>
        </xsl:call-template>
      </ns0:N1Loop1>

      <ns0:N1Loop1>
        <xsl:call-template name="GenerateOrgAddress">
          <xsl:with-param name="Org" select="$Consignor"/>
          <xsl:with-param name="EntityIdCode" select="'SH'"/>
        </xsl:call-template>
      </ns0:N1Loop1>

      <ns0:N1Loop1>
        <xsl:call-template name="GenerateOrgAddress">
          <xsl:with-param name="Org" select="$BillToParty"/>
          <xsl:with-param name="EntityIdCode" select="'BT'"/>
        </xsl:call-template>
      </ns0:N1Loop1>

      <xsl:for-each select="//*[local-name()='TransactionInfo']
		/*[local-name()='PostingJournalCollection']
		/*[local-name()='PostingJournal']">
        <xsl:variable name ="counter" select="userCSharp:GetLXNumber()"/>

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
            
            <xsl:variable name="Description">
              <xsl:variable name="GoodsDesc">
                <xsl:call-template name="ApplyCharacterSet">
                  <xsl:with-param name="Value" select="normalize-space($SubShipment/*[local-name()='GoodsDescription'])" />
                </xsl:call-template>
              </xsl:variable>
              <xsl:variable name="GoodsDescNote">
                <xsl:call-template name="ApplyCharacterSet">
                  <xsl:with-param name="Value" select="normalize-space($SubShipment/*[local-name()='NoteCollection']/*[local-name()='Note' and *[local-name()='Description']/text()='Detailed Goods Description']/*[local-name()='NoteText'])" />
                </xsl:call-template>
              </xsl:variable>
              <xsl:variable name="ChargeCodeDescription">
                <xsl:call-template name="ApplyCharacterSet">
                  <xsl:with-param name="Value" select="normalize-space(./*[local-name()='ChargeCode']/*[local-name()='Description'])" />
                </xsl:call-template>
              </xsl:variable>
              
              <xsl:choose>
                
                <xsl:when test="$GoodsDesc != ''">
                  <xsl:value-of select="substring($GoodsDesc, 1, 50)"/>
                </xsl:when>
                
                <xsl:when test="$GoodsDescNote != ''">
                  <xsl:value-of select="substring($GoodsDescNote, 1, 50)"/>
                </xsl:when>
                
                <xsl:otherwise>
                  <xsl:value-of select="substring($ChargeCodeDescription, 1, 50)"/>
                </xsl:otherwise>
                
              </xsl:choose>
            </xsl:variable>
            <xsl:element name="L502">
              <xsl:value-of select="normalize-space($Description)"/>
            </xsl:element>

          </ns0:L5>

          <ns0:L0>
            <L001>
              <xsl:value-of select="$counter"/>
            </L001>

            <xsl:if test="$counter = 1">
              <xsl:variable name="TotalWeight" select="$SubShipment/*[local-name()='TotalWeight']"/>
              <xsl:variable name="WeightUQ" select="$SubShipment/*[local-name()='TotalWeightUnit']/*[local-name()='Code']"/>
              <xsl:variable name="TotalPacks" select="$SubShipment/*[local-name()='OuterPacks']"/>
              <xsl:variable name="PackType" select="$SubShipment/*[local-name()='OuterPacksPackageType']/*[local-name()='Code']"/>
              <xsl:if test="$TotalWeight != 0">
                <L004>
                  <xsl:value-of select="format-number(ScriptNS2:Convert($TotalWeight, $WeightUQ, &quot;LB&quot;), '0.###')"/>
                </L004>
                <L005>
                  <xsl:value-of select="'G'"/>
                </L005>
              </xsl:if>

              <xsl:if test="$TotalPacks > 0">
                <L008>
                  <xsl:value-of select="$TotalPacks"/>
                </L008>
                <L009>
                  <xsl:if test="$PackType != ''">
                    <xsl:value-of select="ScriptNS1:GetRecipientCode(&quot;AIGEGNEGN&quot; , &quot;AIGEGNEGN_FUJ&quot; , &quot;Fuji Photo 210 - Export A/R Invoices&quot; , &quot;Packaging Form Code&quot; , &quot;X12 Code&quot; , $PackType)"/>
                  </xsl:if>
                </L009>
              </xsl:if>
              <xsl:if test="$TotalWeight != 0 and $WeightUQ != ''">
                <L011>
                  <xsl:value-of select="'L'"/>
                </L011>
              </xsl:if>
            </xsl:if>

          </ns0:L0>

          <ns0:L1>
            <L101>
              <xsl:value-of select="$counter"/>
            </L101>
            <L102>
              <xsl:value-of select="format-number(*[local-name()='OSAmount'], '0.###')"/>
            </L102>
            <L103>FR</L103>
            <L104>
              <xsl:value-of select="format-number(./*[local-name()='OSAmount'] * 100, '0')"/>
            </L104>

            <xsl:call-template name="MapValueIfNotEmpty">
              <xsl:with-param name="NodeName" select="'L108'" />
              <xsl:with-param name="Value" select="ScriptNS1:GetRecipientCode('AIGEGNEGN' , 'AIGEGNEGN_FUJ' , 'Fuji Photo 210 - Export A/R Invoices', 'Charge Code', 
                              ./*[local-name()='ChargeCode']/*[local-name()='Code'])" />
            </xsl:call-template>

            <xsl:call-template name="MapValueIfNotEmpty">
              <xsl:with-param name="NodeName" select="'L112'" />
              <xsl:with-param name="Value" select="normalize-space(substring(*[local-name()='ChargeCode']/*[local-name()='Description'],1,25))" />
            </xsl:call-template>
            
          </ns0:L1>
        </ns0:LXLoop1>

      </xsl:for-each>

      <ns0:L3>
        <xsl:if test="$SubShipment/*[local-name()='ActualChargeable'] > 0">
          <L301>
            <xsl:value-of select="format-number($SubShipment/*[local-name()='ActualChargeable'], '0.###')"/>
          </L301>
        </xsl:if>

        <L305>
          <xsl:value-of select="format-number(sum(//*[local-name()='OSTotal']) * 100, '0')"/>
        </L305>

        <xsl:if test="$SubShipment/*[local-name()='OuterPacks'] > 0">
          <L311>
            <xsl:value-of select="$SubShipment/*[local-name()='OuterPacks']"/>
          </L311>
        </xsl:if>

      </ns0:L3>
    </ns0:X12_00401_210>
  </xsl:template>

  <xsl:template name="MapValueIfNotEmpty">
    <xsl:param name="NodeName" />
    <xsl:param name="Value" />

    <xsl:if test="$Value != '' and string-length($Value) != 0">
      <xsl:choose>

        <xsl:when test="contains($NodeName, '/')">
          <xsl:element name="{substring-before($NodeName, '/')}">
            <xsl:call-template name="MapValueIfNotEmpty">
              <xsl:with-param name="NodeName" select="substring-after($NodeName, '/')" />
              <xsl:with-param name="Value" select="$Value" />
            </xsl:call-template>
          </xsl:element>
        </xsl:when>

        <xsl:otherwise>
          <xsl:element name="{$NodeName}">
            <xsl:call-template name="ApplyCharacterSet">
              <xsl:with-param name="Value" select="$Value" />
            </xsl:call-template>
          </xsl:element>
        </xsl:otherwise>

      </xsl:choose>
    </xsl:if>

  </xsl:template>

  <xsl:template name="GenerateOrgAddress">
    <xsl:param name="Org"/>
    <xsl:param name="EntityIdCode"/>
    <ns0:N1>
      <N101>
        <xsl:value-of select="$EntityIdCode"/>
      </N101>
      <xsl:if test="$Org/*[local-name()='CompanyName']">
        <N102>
          <xsl:value-of select="translate($Org/*[local-name()='CompanyName'], ':*', '.')"/>
        </N102>
      </xsl:if>
    </ns0:N1>

    <xsl:if test="$Org/*[local-name() = 'Address1'] != ''">
      <ns0:N3>
        <N301>
          <xsl:value-of select="translate($Org/*[local-name() = 'Address1'], ':*', '.')"/>
        </N301>
        <xsl:if test="$Org/*[local-name() = 'Address2'] != ''">
          <N302>
            <xsl:value-of select="translate($Org/*[local-name() = 'Address2'], ':*', '.')"/>
          </N302>
        </xsl:if>
      </ns0:N3>
    </xsl:if>

    <xsl:variable name="City" select="normalize-space($Org/*[local-name()='City'])"/>
    <xsl:variable name="State" select="normalize-space($Org/*[local-name()='State'])"/>
    <xsl:variable name="Postcode" select="normalize-space($Org/*[local-name()='Postcode'])"/>
    
    <xsl:if test="string-length($City) >= 2 or string-length($State) >= 2 or string-length($Postcode) >= 5">
      <ns0:N4>
        <xsl:if test="string-length($City) >= 2">
          <N401>
            <xsl:value-of select="substring($City, 1, 19)"/>
          </N401>
        </xsl:if>
        <xsl:if test="string-length($State) >= 2">
          <N402>
            <xsl:value-of select="substring($State, 1, 2)"/>
          </N402>
        </xsl:if>
        <xsl:if test="string-length($Postcode) >= 5">
          <N403>
            <xsl:value-of select="substring($Postcode, 1, 9)"/>
          </N403>
        </xsl:if>
      </ns0:N4>
    </xsl:if>

  </xsl:template>



  <xsl:template name="ApplyCharacterSet">
    <xsl:param name="Value" />
    <xsl:param name="CharacterSet" select="'Basic'" />
    <xsl:param name="ReplaceWithValidCharacters" select="true()" />

    <xsl:variable name="Upper" select="'ABCDEFGHIJKLMNOPQRSTUVWXYZ'" />
    <xsl:variable name="Lower" select="'abcdefghijklmnopqrstuvwxyz'" />
    <xsl:variable name="BasicCharacterSet">
      <xsl:value-of select="$Upper"/>
      <xsl:text>0123456789!&quot;&amp;&apos;()*+,-./:;?= </xsl:text>
    </xsl:variable>
    <xsl:variable name="SelectLanguageCharacters" select="'ÀÁÂÄàáâäÈÉÊèéêëÌÍÎìíîïÒÓÔÖòóôöÙÚÛÜùúûüÇçÑñ¿¡'" />
    <xsl:variable name="CorrespondingCharacters"  select="'AAAAAAAAEEEEEEEIIIIIIIOOOOOOOOUUUUUUUUCCNN?!'" />
    <xsl:variable name="ExtendedCharacterSet">
      <xsl:value-of select="$BasicCharacterSet"/>
      <xsl:value-of select="$Lower"/>
      <xsl:text>%@[]_{}\|&lt;&gt;~#$</xsl:text>
      <xsl:value-of select="$SelectLanguageCharacters"/>
    </xsl:variable>

    <xsl:choose>

      <xsl:when test="$CharacterSet = 'Basic'">
        <xsl:choose>

          <xsl:when test="$ReplaceWithValidCharacters">
            <xsl:variable name="NewValue" select="translate($Value, concat($Lower, $SelectLanguageCharacters), concat($Upper, $CorrespondingCharacters))" />
            <xsl:value-of select="translate($NewValue, translate($NewValue, $BasicCharacterSet, ''), '')"/>
          </xsl:when>

          <xsl:otherwise>
            <xsl:value-of select="translate($Value, translate($Value, $BasicCharacterSet, ''), '')"/>
          </xsl:otherwise>

        </xsl:choose>
      </xsl:when>

      <xsl:when test="$CharacterSet = 'Extended'">
        <xsl:value-of select="translate($Value, translate($Value, $ExtendedCharacterSet, ''), '')"/>
      </xsl:when>

      <xsl:otherwise>
        <xsl:value-of select="$Value"/>
      </xsl:otherwise>

    </xsl:choose>

  </xsl:template>
  
  

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[		

public int LXNumber = 1;

public int GetLXNumber()
{
   return LXNumber++;
}

]]>
  </msxsl:script>
</xsl:stylesheet>
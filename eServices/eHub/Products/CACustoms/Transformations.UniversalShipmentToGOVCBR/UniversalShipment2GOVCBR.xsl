<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s1 ScriptNS0 ScriptNS1 ScriptNS2 ScriptNS3 ScriptNS4 ScriptNS5 userCSharp" version="1.0"
                xmlns:s1="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:ns0="http://cargowise.com/ehub/products/canadiancustoms"
                xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
                xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1"
                xmlns:ScriptNS2="http://schemas.microsoft.com/BizTalk/2003/ScriptNS2"
                xmlns:ScriptNS3="http://schemas.microsoft.com/BizTalk/2003/ScriptNS3"
                xmlns:ScriptNS4="http://schemas.microsoft.com/BizTalk/2003/ScriptNS4"
                xmlns:ScriptNS5="http://schemas.microsoft.com/BizTalk/2003/ScriptNS5"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />

  <xsl:template match="/s1:UniversalInterchange">
    <xsl:variable name="RecipientID" select="ScriptNS0:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
    <xsl:variable name="NetworkID" select="./s1:Header/s1:RecipientID"/>
    <xsl:variable name="InterchangeNumber" select="normalize-space(./s1:Header/s1:InterchangeNumber)"/>
    <ns0:IID_GOVCBR>
      <ns0:UNB>
        <ns0:TAGNAME>UNB</ns0:TAGNAME>
        <ns0:UNB1>
          <ns0:UNB1.1>UNOC</ns0:UNB1.1>
          <ns0:UNB1.2>3</ns0:UNB1.2>
        </ns0:UNB1>
        <ns0:UNB2>
          <ns0:UNB2.1>
            <xsl:variable name="SenderIDInHeader" select="normalize-space(./s1:Header/s1:SenderID)" />
            <xsl:choose>
              <xsl:when test="$SenderIDInHeader and $SenderIDInHeader != ''">
                <xsl:value-of select="$SenderIDInHeader"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="./s1:Body/*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='ClientNetworkID' and *[local-name()='Value'] and *[local-name()='Value']!='']/*[local-name()='Value']" />
              </xsl:otherwise>
            </xsl:choose>
          </ns0:UNB2.1>
        </ns0:UNB2>
        <ns0:UNB3>
          <ns0:UNB3.1>
            <xsl:value-of select="$NetworkID"/>
          </ns0:UNB3.1>
        </ns0:UNB3>
        <ns0:UNB4>
          <!-- TODO: VICTEST: Need to double check the timezone for this preperation date-->
          <ns0:UNB4.1>
            <xsl:value-of select="ScriptNS1:CurrentDateTime('yyMMdd')" />
          </ns0:UNB4.1>
          <ns0:UNB4.2>
            <xsl:value-of select="ScriptNS1:CurrentDateTime('HHmm')" />
          </ns0:UNB4.2>
        </ns0:UNB4>
        <ns0:UNB5>
          <xsl:value-of select="$InterchangeNumber"/>
        </ns0:UNB5>
        <!-- TODO: VICTEST: UNB Sequence Number to be confirmed -->
      </ns0:UNB>
      <ns0:UNG>
        <ns0:TAGNAME>UNG</ns0:TAGNAME>
        <ns0:UNG1>GOVCBR</ns0:UNG1>
        <ns0:UNG2>
          <ns0:UNG2.1>
            <xsl:choose>
              <xsl:when test="starts-with($NetworkID, 'RCCECECP')">U10207V2</xsl:when>
              <xsl:otherwise>U10207V1</xsl:otherwise>
            </xsl:choose>
          </ns0:UNG2.1>
          <!-- TODO: VICTEST: Need to double check with the value to be placed in here-->
        </ns0:UNG2>
        <ns0:UNG3>
          <ns0:UNG3.1>
            <xsl:choose>
              <xsl:when test="$RecipientID='CACustoms' or $RecipientID='CACustomsMQ'">IIDP</xsl:when>
              <xsl:otherwise>IIDT</xsl:otherwise>
            </xsl:choose>
          </ns0:UNG3.1>
        </ns0:UNG3>
        <ns0:UNG4>
          <!-- TODO: VICTEST: Need to double check the timezone for this preperation date-->
          <ns0:UNG4.1>
            <xsl:value-of select="ScriptNS1:CurrentDateTime('yyMMdd')" />
          </ns0:UNG4.1>
          <ns0:UNG4.2>
            <xsl:value-of select="ScriptNS1:CurrentDateTime('HHmm')" />
          </ns0:UNG4.2>
        </ns0:UNG4>
        <ns0:UNG5>0001</ns0:UNG5>
        <ns0:UNG6>UN</ns0:UNG6>
        <ns0:UNG7>
          <ns0:UNG7.1>D</ns0:UNG7.1>
          <ns0:UNG7.2>13A</ns0:UNG7.2>
          <ns0:UNG7.3>IID</ns0:UNG7.3>
        </ns0:UNG7>
      </ns0:UNG>
      <xsl:apply-templates select="/s1:UniversalInterchange/s1:Body/*[local-name()='UniversalShipment']" />
      <!-- TODO: VICTEST: ToBeCleaned -->
      <ns0:UNE>
        <ns0:TAGNAME>UNE</ns0:TAGNAME>
        <ns0:UNE1></ns0:UNE1>
        <ns0:UNE2>0001</ns0:UNE2>
      </ns0:UNE>
      <ns0:UNZ>
        <ns0:TAGNAME>UNZ</ns0:TAGNAME>
        <ns0:UNZ1>1</ns0:UNZ1>
        <ns0:UNZ2>
          <xsl:value-of select="$InterchangeNumber"/>
        </ns0:UNZ2>
        <!-- UNB  Sequence Number to be confirmed -->
      </ns0:UNZ>
    </ns0:IID_GOVCBR>
  </xsl:template>

  <xsl:template match="*[local-name()='UniversalShipment']/*[local-name()='Shipment']">
    <xsl:variable name="EventReference" select="userCSharp:KeyValueSplit(*[local-name()='DataContext']/*[local-name()='EventReference'])"/>
    <xsl:variable name="CurrentDateTimeUTC" select="ScriptNS1:CurrentDateTimeUTC('yyyyMMdd')"/>
    <xsl:variable name="IncDecCount" select="userCSharp:IncrementDecCount()"/>
    <xsl:variable name="DecCount" select="userCSharp:GetDecCount()"/>
    <xsl:variable name="MessageRefNumber" select="userCSharp:GenerateMessageRefNumber($CurrentDateTimeUTC, $DecCount)"/>
    <xsl:variable name="InvoicePaths" select="(*[local-name()='CommercialInfo']/*[local-name()='CommercialInvoiceCollection']/*[local-name()='CommercialInvoice'] | *[local-name()='CommercialInfo']/*[local-name()='SubGroupCollection']/*[local-name()='SubGroup']/*[local-name()='CommercialInvoiceCollection']/*[local-name()='CommercialInvoice'])"/>
    <ns0:EFACT_D13A_GOVCBR>
      <xsl:variable name="IIDVersion">
        <xsl:variable name="processCodeforVersion" select="$InvoicePaths/*[local-name()='CommercialInvoiceLineCollection']/*[local-name()='CommercialInvoiceLine']/*[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup'][./*[local-name()='Type']/*[local-name()='Code']/text()='CEC']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ProcessCode']/*[local-name()='Value']/text()" />
        <xsl:choose>
          <xsl:when test="$processCodeforVersion = 'EC01' or $processCodeforVersion = 'EC02' or $processCodeforVersion = 'EC03' or $processCodeforVersion = 'EC04'">4.00</xsl:when>
          <xsl:when test="$EventReference/RFN=4.02">4.02</xsl:when>
          <xsl:otherwise>4.01</xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
       <ns0:UNH>
        <ns0:TAGNAME>UNH</ns0:TAGNAME>
        <ns0:UNH1>
          <xsl:value-of select="$MessageRefNumber"/>
        </ns0:UNH1>
        <ns0:UNH2>
          <ns0:UNH2.1>GOVCBR</ns0:UNH2.1>
          <ns0:UNH2.2>D</ns0:UNH2.2>
          <ns0:UNH2.3>13A</ns0:UNH2.3>
          <ns0:UNH2.4>UN</ns0:UNH2.4>
          <ns0:UNH2.5>IID</ns0:UNH2.5>
        </ns0:UNH2>
        <ns0:UNH3>
          <xsl:value-of select="$IIDVersion"/>
        </ns0:UNH3>
      </ns0:UNH>

      <!-- Region: BGM Basic Actional Information -->
      <ns0:BGM>
        <ns0:TAGNAME>BGM</ns0:TAGNAME>
        <ns0:C002>
          <ns0:C00201>
            <xsl:choose>
              <xsl:when test="$EventReference/MST='IID'">929</xsl:when>
              <xsl:when test="$EventReference/MST='CID'">931</xsl:when>
            </xsl:choose>
          </ns0:C00201>
        </ns0:C002>
        <ns0:C106>
          <ns0:C10601>
            <xsl:value-of select="normalize-space(*[local-name()='EntryNumberCollection']/*[local-name()='EntryNumber'][./*[local-name()='Type']/*[local-name()='Code']='REL']/*[local-name()='Number'])"/>
          </ns0:C10601>
        </ns0:C106>
        <ns0:BGM03>
          <xsl:choose>
            <xsl:when test="$EventReference/MSB='CNL'">1</xsl:when>
            <xsl:when test="$EventReference/MSB='CHG'">4</xsl:when>
            <xsl:when test="$EventReference/MSB='ORG'">9</xsl:when>
            <xsl:when test="$EventReference/MSB='AMD'">52</xsl:when>
          </xsl:choose>
        </ns0:BGM03>
        <ns0:BGM04>
          <xsl:if test="normalize-space(*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='FrenchPreferred']/*[local-name()='Value'])='Y'">
            <xsl:value-of select="'FR'"/>
          </xsl:if>
        </ns0:BGM04>
      </ns0:BGM>

      <!-- Region: DTM DateOfArrival -->
      <xsl:variable name="FirstArrivalInCountry" select="normalize-space(*[local-name()='DateCollection']/*[local-name()='Date'][./*[local-name()='Type']='FirstArrivalInCountry']/*[local-name()='Value'])" />
      <xsl:variable name="DateOfArrival" select="normalize-space(*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='PARSETA']/*[local-name()='Value'])" />
      <xsl:variable name="Date">
        <xsl:choose>
          <xsl:when test="$FirstArrivalInCountry!=''">
            <xsl:value-of select="ScriptNS1:FormatXmlDateTime($FirstArrivalInCountry, 'yyyyMMddHHmm')"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="ScriptNS1:FormatXmlDateTime($DateOfArrival, 'yyyyMMddHHmm')"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:if test="string($Date)!=''">
        <ns0:DTM>
          <ns0:TAGNAME>DTM</ns0:TAGNAME>
          <ns0:C507>
            <ns0:C50701>132</ns0:C50701>
            <ns0:C50702>
              <xsl:value-of select="$Date"/>
            </ns0:C50702>
            <ns0:C50703>203</ns0:C50703>
          </ns0:C507>
        </ns0:DTM>
      </xsl:if>

      <!-- Region: MOA Total Transaction Value -->
      <ns0:MOA>
        <ns0:TAGNAME>MOA</ns0:TAGNAME>
        <ns0:C516>
          <ns0:C51601>134</ns0:C51601>
          <ns0:C51602>
            <xsl:value-of select="format-number(sum(*[local-name()='EntryHeaderCollection']/*[local-name()='EntryHeader'][./*[local-name()='Type']/*[local-name()='Code']='REL']/*[local-name()='EntryLineCollection']/*[local-name()='EntryLine']/*[local-name()='CustomsValue']), '0.##')"/>
          </ns0:C51602>
          <ns0:C51603>CAD</ns0:C51603>
        </ns0:C516>
      </ns0:MOA>

      <xsl:variable name="ATDExCode" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ATDExCode']/*[local-name()='Value']"/>
      <xsl:if test="$ATDExCode!=''">
        <ns0:IFD>
          <ns0:TAGNAME>IFD</ns0:TAGNAME>
          <ns0:IFD01/>
          <ns0:C009/>
          <ns0:C010/>
          <ns0:C011>
            <ns0:C01101>
              <xsl:value-of select="$ATDExCode"/>
            </ns0:C01101>
          </ns0:C011>
        </ns0:IFD>
      </xsl:if>

      <!-- Region: RFF For Declaration Number -->
      <xsl:variable name="DeclarationNumber" select="normalize-space(*[local-name()='DataContext']/*[local-name()='DataSourceCollection']/*[local-name()='DataSource'][./*[local-name()='Type']='CustomsDeclaration']/*[local-name()='Key'])"/>
      <xsl:if test="$DeclarationNumber">
        <ns0:RFF>
          <ns0:TAGNAME>RFF</ns0:TAGNAME>
          <ns0:C506>
            <ns0:C50601>ABO</ns0:C50601>
            <ns0:C50602>
              <xsl:value-of select="$DeclarationNumber"/>
            </ns0:C50602>
          </ns0:C506>
        </ns0:RFF>
      </xsl:if>

      <!-- Region: RFF_2 for CCN Number -->
      <xsl:variable name="CustomsReferenceCNN" select="*[local-name()='CustomsReferenceCollection']/*[local-name()='CustomsReference'][./*[local-name()='Type']/*[local-name()='Code']='CCN']/*[local-name()='Reference']"/>
      <xsl:if test="(count($CustomsReferenceCNN) = 1) and ($CustomsReferenceCNN != '')">
        <ns0:RFF_2>
          <ns0:TAGNAME>RFF</ns0:TAGNAME>
          <ns0:C506_2>
            <ns0:C50601>CN</ns0:C50601>
            <ns0:C50602>
              <xsl:value-of select="userCSharp:EscapeString($CustomsReferenceCNN)"/>
            </ns0:C50602>
          </ns0:C506_2>
        </ns0:RFF_2>
      </xsl:if>

      <!-- Region: SG3/GOR+LOC -->
      <ns0:SG3Loop>
        <ns0:GOR>
          <ns0:TAGNAME>GOR</ns0:TAGNAME>
          <ns0:GOR01/>
          <ns0:C232>
            <ns0:C23201>5</ns0:C23201>
          </ns0:C232>
        </ns0:GOR>
        <xsl:variable name="PortOfClearance" select="normalize-space(*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='PortOfClearance']/*[local-name()='Value'])"/>
        <xsl:if test="$PortOfClearance!=''">
          <ns0:LOC>
            <ns0:TAGNAME>LOC</ns0:TAGNAME>
            <ns0:LOC01>23</ns0:LOC01>
            <ns0:C517>
              <ns0:C51701>
                <xsl:value-of select="$PortOfClearance"/>
              </ns0:C51701>
            </ns0:C517>
            <xsl:variable name="SubLocationCode" select="normalize-space(*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='SubLocationCode']/*[local-name()='Value'])"/>
            <xsl:variable name="SubLocationName" select="normalize-space(*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='SubLocationName']/*[local-name()='Value'])"/>
            <xsl:if test="$SubLocationCode!='' or $SubLocationName!=''">
              <ns0:C519>
                <xsl:if test="$SubLocationCode!=''">
                  <ns0:C51901>
                    <xsl:value-of select="$SubLocationCode"/>
                  </ns0:C51901>
                </xsl:if>
                <xsl:if test="$SubLocationName!=''">
                  <ns0:C51904>
                    <xsl:value-of select="userCSharp:EscapeString(substring($SubLocationName, 1, 35))"/>
                  </ns0:C51904>
                </xsl:if>
              </ns0:C519>
            </xsl:if>
          </ns0:LOC>
        </xsl:if>
      </ns0:SG3Loop>
      <xsl:call-template name="SG3Loop">
        <xsl:with-param name="Shipment" select="."/>
        <xsl:with-param name="AgencyCode" select="1"/>
        <xsl:with-param name="InfoGroupCode" select="'CCF'"/>
        <xsl:with-param name="InfoCode" select="'CFIAInd'"/>
      </xsl:call-template>
      <xsl:call-template name="SG3Loop">
        <xsl:with-param name="Shipment" select="."/>
        <xsl:with-param name="AgencyCode" select="3"/>
        <xsl:with-param name="InfoGroupCode" select="'CGA'"/>
        <xsl:with-param name="InfoCode" select="'GACInd'"/>
      </xsl:call-template>
      <xsl:call-template name="SG3Loop">
        <xsl:with-param name="Shipment" select="."/>
        <xsl:with-param name="AgencyCode" select="12"/>
        <xsl:with-param name="InfoGroupCode" select="'CHC'"/>
        <xsl:with-param name="InfoCode" select="'HCInd'"/>
      </xsl:call-template>
      <xsl:call-template name="SG3Loop">
        <xsl:with-param name="Shipment" select="."/>
        <xsl:with-param name="AgencyCode" select="13"/>
        <xsl:with-param name="InfoGroupCode" select="'CTC'"/>
        <xsl:with-param name="InfoCode" select="'TCInd'"/>
      </xsl:call-template>
      <xsl:call-template name="SG3Loop">
        <xsl:with-param name="Shipment" select="."/>
        <xsl:with-param name="AgencyCode" select="20"/>
        <xsl:with-param name="InfoGroupCode" select="'CFO'"/>
        <xsl:with-param name="InfoCode" select="'DFOInd'"/>
      </xsl:call-template>
      <xsl:call-template name="SG3Loop">
        <xsl:with-param name="Shipment" select="."/>
        <xsl:with-param name="AgencyCode" select="21"/>
        <xsl:with-param name="InfoGroupCode" select="'CNR'"/>
        <xsl:with-param name="InfoCode" select="'NRCanInd'"/>
      </xsl:call-template>
      <xsl:call-template name="SG3Loop">
        <xsl:with-param name="Shipment" select="."/>
        <xsl:with-param name="AgencyCode" select="22"/>
        <xsl:with-param name="InfoGroupCode" select="'CEC'"/>
        <xsl:with-param name="InfoCode" select="'ECCCInd'"/>
      </xsl:call-template>
      <xsl:call-template name="SG3Loop">
        <xsl:with-param name="Shipment" select="."/>
        <xsl:with-param name="AgencyCode" select="23"/>
        <xsl:with-param name="InfoGroupCode" select="'CPH'"/>
        <xsl:with-param name="InfoCode" select="'PHACInd'"/>
      </xsl:call-template>
      <xsl:call-template name="SG3Loop">
        <xsl:with-param name="Shipment" select="."/>
        <xsl:with-param name="AgencyCode" select="24"/>
        <xsl:with-param name="InfoGroupCode" select="'CCN'"/>
        <xsl:with-param name="InfoCode" select="'CNSCInd'"/>
      </xsl:call-template>

      <!-- Region: Organization -->
      <!-- if ImporterOfRecord exists then use it, otherwise anything else is fine-->
      <!-- if BRM exists then use it, otherwise anything else is fine-->
      <xsl:variable name="OrganizationAddressType">
        <xsl:choose>
          <xsl:when test="*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']='ImporterOfRecord']">ImporterOfRecord</xsl:when>
          <xsl:when test="*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']='Importer']">Importer</xsl:when>
          <xsl:otherwise>ImporterDocumentaryAddress</xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:if test="*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']=$OrganizationAddressType]">
        <ns0:SG7Loop>
          <xsl:call-template name="NADSegment">
            <xsl:with-param name="Type" select="'IM'"/>
            <xsl:with-param name="OrgAddress" select="*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']=$OrganizationAddressType]"/>
            <xsl:with-param name="ContactLoopID" select="'8'"/>
            <xsl:with-param name="MandatoryCOM" select="true()"/>
          </xsl:call-template>
        </ns0:SG7Loop>
      </xsl:if>
      <xsl:if test="*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']='CFIAAccountOwner']">
        <ns0:SG7Loop>
          <xsl:call-template name="NADSegment">
            <xsl:with-param name="Type" select="'HQ'"/>
            <xsl:with-param name="ContactLoopID" select="'8'"/>
            <xsl:with-param name="OrgAddress" select="*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']='CFIAAccountOwner']"/>
            <xsl:with-param name="PartyID" select="*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']='CFIAAccountOwner']/*[local-name()='RegistrationNumberCollection']/*[local-name()='RegistrationNumber'][./*[local-name()='Type']/*[local-name()='Code']='CFI']/*[local-name()='Value']"/>
            <xsl:with-param name="MandatoryCOM" select="true()"/>
          </xsl:call-template>
        </ns0:SG7Loop>
      </xsl:if>
      <xsl:if test="*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']='CustomsBroker']">
        <ns0:SG7Loop>
          <xsl:call-template name="NADSegment">
            <xsl:with-param name="Type" select="'CB'"/>
            <xsl:with-param name="ContactLoopID" select="'8'"/>
            <xsl:with-param name="OrgAddress" select="*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']='CustomsBroker']"/>
            <xsl:with-param name="PartyID" select="*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']='CustomsBroker']/*[local-name()='RegistrationNumberCollection']/*[local-name()='RegistrationNumber'][./*[local-name()='Type']/*[local-name()='Code']='ASC']/*[local-name()='Value']"/>
            <xsl:with-param name="MandatoryCOM" select="true()"/>
          </xsl:call-template>
        </ns0:SG7Loop>
      </xsl:if>

      <!-- Region: Related Doc -->
      <xsl:variable name="Shipment" select="."/>
      <xsl:variable name="Resetsg9LoopCount" select="userCSharp:ResetSG9Count()"/>
      <xsl:for-each select="*[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup'][./*[local-name()='Type']/*[local-name()='Code']='CLP']">
      <xsl:variable name="sg9LoopCount" select="userCSharp:SG9Count()"/>
        <xsl:if test="100 > number($sg9LoopCount)">
        <ns0:SG9Loop>
          <ns0:DOC_1>
            <ns0:TAGNAME>DOC</ns0:TAGNAME>
            <ns0:C002_2>
              <ns0:C00201>916</ns0:C00201>
              <ns0:C00202>
                <xsl:value-of select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='Type']/*[local-name()='Value']"/>
              </ns0:C00202>
            </ns0:C002_2>
            <ns0:C503_1>
              <ns0:C50301>
                <xsl:value-of select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='RefNo']/*[local-name()='Value']"/>
              </ns0:C50301>
              <ns0:C50302/>
              <ns0:C50303>
                <xsl:value-of select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='DIFRefNumberOrLocation']/*[local-name()='Value']"/>
              </ns0:C50303>
            </ns0:C503_1>
          </ns0:DOC_1>
          <xsl:if test="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='SecondaryRefNo']/*[local-name()='Value']">
            <ns0:RFF_3>
              <ns0:TAGNAME>RFF</ns0:TAGNAME>
              <ns0:C506_3>
                <ns0:C50601>ABB</ns0:C50601>
                <ns0:C50602>
                  <xsl:value-of select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='SecondaryRefNo']/*[local-name()='Value']"/>
                </ns0:C50602>
              </ns0:C506_3>
            </ns0:RFF_3>
          </xsl:if>
          <xsl:if test="ScriptNS1:FormatXmlDateTime(*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='LPCOEndDate']/*[local-name()='Value'], 'yyyyMMdd')">
            <ns0:DTM_2>
              <ns0:TAGNAME>DTM</ns0:TAGNAME>
              <ns0:C507_2>
                <ns0:C50701>36</ns0:C50701>
                <ns0:C50702>
                  <xsl:value-of select="ScriptNS1:FormatXmlDateTime(*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='LPCOEndDate']/*[local-name()='Value'], 'yyyyMMdd')"/>
                </ns0:C50702>
                <ns0:C50703>102</ns0:C50703>
              </ns0:C507_2>
            </ns0:DTM_2>
          </xsl:if>
          <xsl:if test="ScriptNS1:FormatXmlDateTime(*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='LPCOIssueDate']/*[local-name()='Value'], 'yyyyMMdd')">
            <ns0:DTM_2>
              <ns0:TAGNAME>DTM</ns0:TAGNAME>
              <ns0:C507_2>
                <ns0:C50701>137</ns0:C50701>
                <ns0:C50702>
                  <xsl:value-of select="ScriptNS1:FormatXmlDateTime(*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='LPCOIssueDate']/*[local-name()='Value'], 'yyyyMMdd')"/>
                </ns0:C50702>
                <ns0:C50703>102</ns0:C50703>
              </ns0:C507_2>
            </ns0:DTM_2>
          </xsl:if>
          <xsl:if test="ScriptNS1:FormatXmlDateTime(*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='LPCOStartDate']/*[local-name()='Value'], 'yyyyMMdd')">
            <ns0:DTM_2>
              <ns0:TAGNAME>DTM</ns0:TAGNAME>
              <ns0:C507_2>
                <ns0:C50701>7</ns0:C50701>
                <ns0:C50702>
                  <xsl:value-of select="ScriptNS1:FormatXmlDateTime(*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='LPCOStartDate']/*[local-name()='Value'], 'yyyyMMdd')"/>
                </ns0:C50702>
                <ns0:C50703>102</ns0:C50703>
              </ns0:C507_2>
            </ns0:DTM_2>
          </xsl:if>
          <xsl:if test="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='Qty']/*[local-name()='Value']">
            <ns0:QTY_1>
              <ns0:TAGNAME>QTY</ns0:TAGNAME>
              <ns0:C186_1>
                <ns0:C18601>1</ns0:C18601>
                <ns0:C18602>
                  <xsl:value-of select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='Qty']/*[local-name()='Value']"/>
                </ns0:C18602>
                <ns0:C18603>
                  <xsl:value-of select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='UQ']/*[local-name()='Value']"/>
                </ns0:C18603>
              </ns0:C186_1>
            </ns0:QTY_1>
          </xsl:if>
          <xsl:call-template name="SG10ForEachLPCOkey">
            <xsl:with-param name="Shipment" select="$Shipment"/>
            <xsl:with-param name="AddInfoGroup" select="."/>
            <xsl:with-param name="PartyQualifier" select="'DFK'"/>
            <xsl:with-param name="LPCOKey" select="'LPCOHolderType'"/>
          </xsl:call-template>
          <xsl:call-template name="SG10ForEachLPCOkey">
            <xsl:with-param name="Shipment" select="$Shipment"/>
            <xsl:with-param name="AddInfoGroup" select="."/>
            <xsl:with-param name="PartyQualifier" select="'DDD'"/>
            <xsl:with-param name="LPCOKey" select="'LPCOApplicant'"/>
          </xsl:call-template>
          <xsl:variable name="CountryOfIssuance" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='CountryOfIssuance']/*[local-name()='Value']"/>
          <xsl:if test="$CountryOfIssuance">
            <ns0:SG12Loop>
              <ns0:LOC_2>
                <ns0:TAGNAME>LOC</ns0:TAGNAME>
                <ns0:LOC01>91</ns0:LOC01>
                <ns0:C517_2>
                  <ns0:C51701>
                    <xsl:value-of select="$CountryOfIssuance"/>
                  </ns0:C51701>
                </ns0:C517_2>
              </ns0:LOC_2>
            </ns0:SG12Loop>
          </xsl:if>
          <xsl:variable name="CountryOfOrigin" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='CountryOfOrigin']/*[local-name()='Value']"/>
          <xsl:if test="$CountryOfOrigin != ''">
            <ns0:SG12Loop>
              <ns0:LOC_2>
                <ns0:TAGNAME>LOC</ns0:TAGNAME>
                <ns0:LOC01>27</ns0:LOC01>
                <ns0:C517_2>
                  <ns0:C51701>
                    <xsl:value-of select="$CountryOfOrigin"/>
                  </ns0:C51701>
                  <xsl:if test="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='IsMixedCountryOfOrigin']/*[local-name()='Value']='Y'">
                    <ns0:C51702>MIX</ns0:C51702>
                  </xsl:if>
                </ns0:C517_2>
              </ns0:LOC_2>
            </ns0:SG12Loop>
          </xsl:if>
          <xsl:variable name="AuthorizationCountry" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='AuthorizationCountry']/*[local-name()='Value']"/>
          <xsl:if test="$AuthorizationCountry">
            <ns0:SG12Loop>
              <ns0:LOC_2>
                <ns0:TAGNAME>LOC</ns0:TAGNAME>
                <ns0:LOC01>44</ns0:LOC01>
                <ns0:C517_2>
                  <ns0:C51701>
                    <xsl:value-of select="$AuthorizationCountry"/>
                  </ns0:C51701>
                </ns0:C517_2>
              </ns0:LOC_2>
            </ns0:SG12Loop>
          </xsl:if>
         </ns0:SG9Loop>
        </xsl:if>
      </xsl:for-each>

      <!-- Region: Additional Inpection/Exception Details-->
      <!-- TODO: confirm CommercialInvoiceLine.AddInfoGroupCollection[AddInfoGroup.Type.Code:CEC].AddInfoCollection[AddInfo.Key:ProcessCode].Value will only be EC01, EC02, EC03, EC04 -->
      <xsl:if test="$IIDVersion='4.00'">
        <xsl:variable name="AddInfoGroups" select="$InvoicePaths/*[local-name()='CommercialInvoiceLineCollection']/*[local-name()='CommercialInvoiceLine']/*[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup']"/>
        <xsl:for-each select="$AddInfoGroups[./*[local-name()='Type']/*[local-name()='Code']='CEC']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='ProcessCode' and not(*[local-name()='Value']=preceding::*[local-name()='Value'])]">
          <xsl:if test="*[local-name()='Value']!=''">
            <xsl:call-template name="SG13Loop">
              <xsl:with-param name="Enable" select="*[local-name()='Value']"/>
              <xsl:with-param name="ExceptionCode" select="*[local-name()='Value']"/>
              <xsl:with-param name="AgencyCode" select="22"/>
            </xsl:call-template>
          </xsl:if>
        </xsl:for-each>
        <!-- TODO: as above, confirm logic is sound -->
        <xsl:for-each select="$AddInfoGroups[./*[local-name()='Type']/*[local-name()='Code']='CHC']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][contains('ExceptProcessingCode1 ExceptProcessingCode2', *[local-name()='Key']) 
                      and not(*[local-name()='Value']=preceding::*[local-name()='Value'])]">
          <xsl:if test="*[local-name()='Value']!=''">
            <xsl:call-template name="SG13Loop">
              <xsl:with-param name="Enable" select="*[local-name()='Value']"/>
              <xsl:with-param name="ExceptionCode" select="*[local-name()='Value']"/>
              <xsl:with-param name="AgencyCode" select="12"/>
            </xsl:call-template>
          </xsl:if>
        </xsl:for-each>
        <xsl:call-template name="SG13Loop">
          <xsl:with-param name="Enable" select="$AddInfoGroups[./*[local-name()='Type']/*[local-name()='Code']='CTC']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='TPRProgramInd']/*[local-name()='Value']='Y'"/>
          <xsl:with-param name="ExceptionCode" select="'TC01'"/>
          <xsl:with-param name="AgencyCode" select="13"/>
        </xsl:call-template>
        <xsl:call-template name="SG13Loop">
          <xsl:with-param name="Enable" select="$AddInfoGroups[./*[local-name()='Type']/*[local-name()='Code']='CTC']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='SubProgram']/*[local-name()='Value']='VFS'"/>
          <xsl:with-param name="ExceptionCode" select="'TC02'"/>
          <xsl:with-param name="AgencyCode" select="13"/>
        </xsl:call-template>
        <xsl:call-template name="SG13Loop">
          <xsl:with-param name="Enable" select="$AddInfoGroups[./*[local-name()='Type']/*[local-name()='Code']='CTC']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='SubProgram']/*[local-name()='Value']='VFC'"/>
          <xsl:with-param name="ExceptionCode" select="'TC03'"/>
          <xsl:with-param name="AgencyCode" select="13"/>
        </xsl:call-template>
        <xsl:call-template name="SG13Loop">
          <xsl:with-param name="Enable" select="$AddInfoGroups[./*[local-name()='Type']/*[local-name()='Code']='CTC']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='SubProgram']/*[local-name()='Value']='PIG'"/>
          <xsl:with-param name="ExceptionCode" select="'TC04'"/>
          <xsl:with-param name="AgencyCode" select="13"/>
        </xsl:call-template>
        <xsl:call-template name="SG13Loop">
          <xsl:with-param name="Enable" select="$AddInfoGroups[./*[local-name()='Type']/*[local-name()='Code']='CTC']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='SubProgram']/*[local-name()='Value']='PIL'"/>
          <xsl:with-param name="ExceptionCode" select="'TC05'"/>
          <xsl:with-param name="AgencyCode" select="13"/>
        </xsl:call-template>
        <xsl:call-template name="SG13Loop">
          <xsl:with-param name="Enable" select="$AddInfoGroups[./*[local-name()='Type']/*[local-name()='Code']='CTC']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='SubProgram']/*[local-name()='Value']='VCR'"/>
          <xsl:with-param name="ExceptionCode" select="'TC06'"/>
          <xsl:with-param name="AgencyCode" select="13"/>
        </xsl:call-template>
        <xsl:call-template name="SG13Loop">
          <xsl:with-param name="Enable" select="$AddInfoGroups[./*[local-name()='Type']/*[local-name()='Code']='CTC']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='SubProgram']/*[local-name()='Value']='VAE'"/>
          <xsl:with-param name="ExceptionCode" select="'TC07'"/>
          <xsl:with-param name="AgencyCode" select="13"/>
        </xsl:call-template>
        <xsl:call-template name="SG13Loop">
          <xsl:with-param name="Enable" select="$AddInfoGroups[./*[local-name()='Type']/*[local-name()='Code']='CTC']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='SubProgram']/*[local-name()='Value']='VUV'"/>
          <xsl:with-param name="ExceptionCode" select="'TC08'"/>
          <xsl:with-param name="AgencyCode" select="13"/>
        </xsl:call-template>
        <xsl:call-template name="SG13Loop">
          <xsl:with-param name="Enable" select="$AddInfoGroups[./*[local-name()='Type']/*[local-name()='Code']='CTC']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='SubProgram']/*[local-name()='Value']='VCC'"/>
          <xsl:with-param name="ExceptionCode" select="'TC09'"/>
          <xsl:with-param name="AgencyCode" select="13"/>
        </xsl:call-template>
        <xsl:call-template name="SG13Loop">
          <xsl:with-param name="Enable" select="$AddInfoGroups[./*[local-name()='Type']/*[local-name()='Code']='CTC']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='SubProgram']/*[local-name()='Value']='VVP'"/>
          <xsl:with-param name="ExceptionCode" select="'TC10'"/>
          <xsl:with-param name="AgencyCode" select="13"/>
        </xsl:call-template>
        <xsl:call-template name="SG13Loop">
          <xsl:with-param name="Enable" select="$AddInfoGroups[./*[local-name()='Type']/*[local-name()='Code']='CPH']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='ExceptPathogenToxin']/*[local-name()='Value']='Y'"/>
          <xsl:with-param name="ExceptionCode" select="'PH01'"/>
          <xsl:with-param name="AgencyCode" select="23"/>
        </xsl:call-template>
      </xsl:if>
      <xsl:variable name="PriorityInd" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='PriorityInd']/*[local-name()='Value']" />
      <xsl:call-template name="SG13Loop">
        <xsl:with-param name="Enable" select="$PriorityInd='1' or $PriorityInd='3'"/>
        <xsl:with-param name="ExceptionCode" select="51"/>
        <xsl:with-param name="AgencyCode" select="5"/>
      </xsl:call-template>
      <xsl:call-template name="SG13Loop">
        <xsl:with-param name="Enable" select="$PriorityInd='2' or $PriorityInd='3'"/>
        <xsl:with-param name="ExceptionCode" select="52"/>
        <xsl:with-param name="AgencyCode" select="5"/>
      </xsl:call-template>
      <xsl:call-template name="SG13Loop">
        <xsl:with-param name="Enable" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='WoodPackagingInd']/*[local-name()='Value']='Y'"/>
        <xsl:with-param name="ExceptionCode" select="56"/>
        <xsl:with-param name="AgencyCode" select="5"/>
      </xsl:call-template>
      <xsl:call-template name="SG13Loop">
        <xsl:with-param name="Enable" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='PermitApplication']/*[local-name()='Value']='Y'"/>
        <xsl:with-param name="ExceptionCode" select="57"/>
        <xsl:with-param name="AgencyCode" select="5"/>
      </xsl:call-template>
      <xsl:call-template name="SG13Loop">
        <xsl:with-param name="Enable" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='InspectionArrangementsComplete']/*[local-name()='Value']='Y'"/>
        <xsl:with-param name="ExceptionCode" select="4"/>
        <xsl:with-param name="AgencyCode" select="3"/>
      </xsl:call-template>

      <!-- Region: Amendment Reason-->
      <xsl:variable name="AmendmentReason" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='AmendReasonCode']/*[local-name()='Value']" />
      <xsl:if test="$AmendmentReason">
        <ns0:SG15Loop>
          <ns0:AJT_1>
            <ns0:TAGNAME>AJT</ns0:TAGNAME>
            <ns0:AJT01>ZZZ</ns0:AJT01>
            <ns0:AJT02>
              <xsl:value-of select="$AmendmentReason" />
            </ns0:AJT02>
          </ns0:AJT_1>
        </ns0:SG15Loop>
      </xsl:if>

      <!-- Region: PGA PaymentDetails - Future Use-->
      <!--
      <ns0:SG25Loop>
        <ns0:TAX_1>
            <ns0:TAGNAME>TAX</ns0:TAGNAME>
          <ns0:TAX01>4</ns0:TAX01>
        </ns0:TAX_1>
        <ns0:SG26Loop>
          <ns0:PAI_1>
              <ns0:TAGNAME>PAI</ns0:TAGNAME>
            <ns0:C534_1>
              <ns0:C53401>C53</ns0:C53401>
            </ns0:C534_1>
          </ns0:PAI_1>
          <ns0:RFF_4>
              <ns0:TAGNAME>RFF</ns0:TAGNAME>
            <ns0:C506_4>
              <ns0:C50601>C50</ns0:C50601>
            </ns0:C506_4>
          </ns0:RFF_4>
          <ns0:MOA_2>
              <ns0:TAGNAME>MOA</ns0:TAGNAME>
            <ns0:C516_2>
              <ns0:C51601>C51</ns0:C51601>
            </ns0:C516_2>
          </ns0:MOA_2>
        </ns0:SG26Loop>
    </ns0:SG25Loop>
    -->

      <!-- Region: UNS fixed seperator -->
      <ns0:UNS>
        <ns0:TAGNAME>UNS</ns0:TAGNAME>
        <ns0:UNS1>D</ns0:UNS1>
      </ns0:UNS>

      <!-- Region: Before SG 47, we can calculate the Packing Lines Levels-->
      <xsl:variable name="PackingLineCollection" select="*[local-name()='PackingLineCollection']"/>
      <xsl:variable name="PackingLinesLevels">
        <xsl:for-each select="$PackingLineCollection/*[local-name()='PackingLine']">
          <xsl:variable name="ResetIDs" select="userCSharp:ResetEndingPackingLineRelationships()"/>
          <xsl:call-template name="PackingLinesLevels">
            <xsl:with-param name="PackingLine" select="."/>
          </xsl:call-template>
        </xsl:for-each>
      </xsl:variable>

      <!-- Region: SG 47 Invoice Level Information-->
      <xsl:for-each select="$InvoicePaths">
        <ns0:SG47Loop>
          <xsl:variable name="IsNotLastSG47Loop" select="position() != last()"/>
          <ns0:SEQ_1>
            <ns0:TAGNAME>SEQ</ns0:TAGNAME>
            <ns0:SEQ01>1</ns0:SEQ01>
          </ns0:SEQ_1>
          <xsl:call-template name="CCNRFF"/>

          <!-- Region: SG48 Invoice parties-->
          <xsl:if test="*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='SupplierDocumentaryAddress']">
            <ns0:SG48Loop>
              <xsl:call-template name="NADSegment">
                <xsl:with-param name="Type" select="'VN'"/>
                <xsl:with-param name="ContactLoopID" select="'49'"/>
                <xsl:with-param name="OrgAddress" select="*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='SupplierDocumentaryAddress']"/>
              </xsl:call-template>
            </ns0:SG48Loop>
          </xsl:if>
          <xsl:variable name="BuyerOrganizationAddress" select="*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='BuyerDocumentaryAddress']"/>
          <xsl:if test="$BuyerOrganizationAddress">
            <ns0:SG48Loop>
              <xsl:call-template name="NADSegment">
                <xsl:with-param name="Type" select="'BY'"/>
                <xsl:with-param name="ContactLoopID" select="'49'"/>
                <xsl:with-param name="OrgAddress" select="$BuyerOrganizationAddress"/>
              </xsl:call-template>
            </ns0:SG48Loop>
          </xsl:if>
          <xsl:variable name="Exporter" select="*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='Exporter']"/>
          <xsl:if test="$Exporter!=''">
            <ns0:SG48Loop>
              <xsl:call-template name="NADSegment">
                <xsl:with-param name="Type" select="'EX'"/>
                <xsl:with-param name="OrgAddress" select="$Exporter"/>
                <xsl:with-param name="ContactLoopID" select="'49'"/>
              </xsl:call-template>
            </ns0:SG48Loop>
          </xsl:if>
          <xsl:variable name="ShipToParty" select="*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='ShipToParty']"/>
          <xsl:if test="$ShipToParty!=''">
            <ns0:SG48Loop>
              <xsl:call-template name="NADSegment">
                <xsl:with-param name="Type" select="'DP'"/>
                <xsl:with-param name="OrgAddress" select="$ShipToParty"/>
                <xsl:with-param name="ContactLoopID" select="'49'"/>
              </xsl:call-template>
            </ns0:SG48Loop>
          </xsl:if>
          <xsl:variable name="FinalConsigneeAddress" select="*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='FinalConsigneeAddress']"/>
          <xsl:if test="$FinalConsigneeAddress!=''">
            <ns0:SG48Loop>
              <xsl:call-template name="NADSegment">
                <xsl:with-param name="Type" select="'UC'"/>
                <xsl:with-param name="ContactLoopID" select="'49'"/>
                <xsl:with-param name="OrgAddress" select="$FinalConsigneeAddress"/>
              </xsl:call-template>
            </ns0:SG48Loop>
          </xsl:if>

          <!-- Region: SG50 Loop: Country/Facility of Export/Direct Shipment -->
          <ns0:SG50Loop>
            <ns0:LOC_3>
              <ns0:TAGNAME>LOC</ns0:TAGNAME>
              <ns0:LOC01>35</ns0:LOC01>
              <xsl:variable name="ExportCountry" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='RN_NKExport']/*[local-name()='Value']/text()" />
              <ns0:C517_3>
                <ns0:C51701>
                  <xsl:value-of select="$ExportCountry"/>
                </ns0:C51701>
              </ns0:C517_3>
              <xsl:variable name="StateOfExport" select="substring(*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='USStateOfExport']/*[local-name()='Value']/text(), 1, 2)"/>
              <xsl:if test="$StateOfExport!=''">
                <ns0:C519_2>
                  <ns0:C51901>
                    <xsl:value-of select="$StateOfExport"/>
                  </ns0:C51901>
                </ns0:C519_2>
              </xsl:if>
            </ns0:LOC_3>
            <xsl:variable name="LoadingDate" select="ScriptNS1:FormatXmlDateTime($Shipment/*[local-name()='DateCollection']/*[local-name()='Date'][*[local-name()='Type']/text()='LoadingDate']/*[local-name()='Value'], 'yyyyMMdd')"/>
            <xsl:if test="$LoadingDate!=''">
              <ns0:DTM_3>
                <ns0:TAGNAME>DTM</ns0:TAGNAME>
                <ns0:C507_3>
                  <ns0:C50701>757</ns0:C50701>
                  <ns0:C50702>
                    <xsl:value-of select="$LoadingDate"/>
                  </ns0:C50702>
                  <ns0:C50703>102</ns0:C50703>
                </ns0:C507_3>
              </ns0:DTM_3>
            </xsl:if>
          </ns0:SG50Loop>
          <xsl:variable name="LastPort" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='RL_NKLastPort']/*[local-name()='Value']/text()" />
          <xsl:if test="$LastPort!=''">
            <ns0:SG50Loop>
              <ns0:LOC_3>
                <ns0:TAGNAME>LOC</ns0:TAGNAME>
                <ns0:LOC01>277</ns0:LOC01>
                <ns0:C517_3>
                  <ns0:C51701>
                    <xsl:value-of select="substring($LastPort,1,2)"/>
                  </ns0:C51701>
                </ns0:C517_3>
                <xsl:variable name="USSate" select="ScriptNS5:CallActionProcedureHelper('GetStateFromUNLOCO', '', '@UNLOCO', $LastPort)"/>
                <xsl:if test="substring($LastPort,1,2)='US' and $USSate!=''">
                  <ns0:C519_2>
                    <ns0:C51901>
                      <xsl:value-of select="substring($USSate,1,2)"/>
                    </ns0:C51901>
                  </ns0:C519_2>
                </xsl:if>
              </ns0:LOC_3>
              <xsl:variable name="ValuationDateOverride" select="ScriptNS1:FormatXmlDateTime(*[local-name()='ValuationDateOverride'], 'yyyyMMdd')"/>
              <xsl:if test="$ValuationDateOverride!=''">
                <ns0:DTM_3>
                  <ns0:TAGNAME>DTM</ns0:TAGNAME>
                  <ns0:C507_3>
                    <ns0:C50701>757</ns0:C50701>
                    <ns0:C50702>
                      <xsl:value-of select="$ValuationDateOverride"/>
                    </ns0:C50702>
                    <ns0:C50703>102</ns0:C50703>
                  </ns0:C507_3>
                </ns0:DTM_3>
              </xsl:if>
            </ns0:SG50Loop>
          </xsl:if>

          <!-- Region: SG51 Invoice Detail: InvNum/Date/Amount-->
          <ns0:SG51Loop>
            <ns0:DOC_2>
              <ns0:TAGNAME>DOC</ns0:TAGNAME>
              <ns0:C002_3>
                <ns0:C00201>380</ns0:C00201>
              </ns0:C002_3>
              <ns0:C503_2>
                <ns0:C50301>
                  <xsl:value-of select="userCSharp:EscapeString(*[local-name()='InvoiceNumber'])"/>
                </ns0:C50301>
              </ns0:C503_2>
            </ns0:DOC_2>
            <ns0:SEQ_2>
              <ns0:TAGNAME>SEQ</ns0:TAGNAME>
              <ns0:SEQ01>1</ns0:SEQ01>
            </ns0:SEQ_2>
            <ns0:DTM_4>
              <ns0:TAGNAME>DTM</ns0:TAGNAME>
              <ns0:C507_4>
                <ns0:C50701>3</ns0:C50701>
                <ns0:C50702>
                  <xsl:value-of select="ScriptNS1:FormatXmlDateTime(*[local-name()='InvoiceDate'], 'yyyyMMdd')"/>
                </ns0:C50702>
                <ns0:C50703>102</ns0:C50703>
              </ns0:C507_4>
            </ns0:DTM_4>
            <ns0:MOA_3>
              <ns0:TAGNAME>MOA</ns0:TAGNAME>
              <ns0:C516_3>
                <ns0:C51601>39</ns0:C51601>
                <ns0:C51602>
                  <xsl:value-of select="format-number(userCSharp:StringDecimalMaxAllowed(*[local-name()='InvoiceAmount'], 12, 2), '0.##')"/>
                </ns0:C51602>
                <ns0:C51603>
                  <xsl:value-of select="substring(*[local-name()='InvoiceCurrency']/*[local-name()='Code'], 1, 3)"/>
                </ns0:C51603>
              </ns0:C516_3>
            </ns0:MOA_3>
          </ns0:SG51Loop>

          <!-- Region: SG68 Weight/Volumn/Quantity -->
          <xsl:variable name="WeightRounded" select="userCSharp:StringDecimalMaxAllowed(*[local-name()='Weight']/text(), 16, 4)" />
          <xsl:variable name="WeightUnit" select="*[local-name()='WeightUnit']/*[local-name()='Code']/text()" />
          <xsl:variable name="NetWeightRounded" select="userCSharp:StringDecimalMaxAllowed(*[local-name()='NetWeight']/text(), 16, 4)" />
          <xsl:variable name="NetWeightUnit" select="*[local-name()='NetWeightUQ']/*[local-name()='Code']/text()" />
          <xsl:variable name="VolumeRounded" select="userCSharp:StringDecimalMaxAllowed(*[local-name()='Volume'], 16, 4)" />
          <xsl:variable name="VolumeUnit" select="*[local-name()='VolumeUnit']/*[local-name()='Code']" />
          <xsl:variable name="NoOfPacks" select="ScriptNS2:RoundAwayFromZero(sum(*[local-name()='CommercialInvoiceLineCollection']/*[local-name()='CommercialInvoiceLine']/*[local-name()='InvoiceQuantity']))" />
          <xsl:variable name="NoOfPacksUnits" select="*[local-name()='CommercialInvoiceLineCollection']/*[local-name()='CommercialInvoiceLine']/*[local-name()='InvoiceQuantityUnit'][not(*[local-name()='Code']=preceding-sibling::*[local-name()='Code'])]/*[local-name()='Code']" />
          <xsl:variable name="NoOfPacksUnit">
            <xsl:choose>
              <xsl:when test="count($NoOfPacksUnits)>1">
                <xsl:value-of select="'PK'"/>
              </xsl:when>
              <xsl:when test="count($NoOfPacksUnits)=1">
                <xsl:choose>
                  <xsl:when test="$NoOfPacksUnits[1]/text()='NMB'">
                    <xsl:value-of select="'NAR'"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="$NoOfPacksUnits[1]/text()"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:when>
            </xsl:choose>
          </xsl:variable>
          <ns0:SG68Loop>
            <xsl:choose>
              <xsl:when test ="number($WeightRounded)=number($WeightRounded) and number($WeightRounded)>0 and $WeightUnit!=''">
                <xsl:call-template name="MEASegment">
                  <xsl:with-param name="Code" select="'AAB'"/>
                  <xsl:with-param name="Unit" select="$WeightUnit"/>
                  <xsl:with-param name="Value" select="$WeightRounded"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test ="number($NetWeightRounded)=number($NetWeightRounded) and number($NetWeightRounded)>0 and $NetWeightUnit!=''">
                <xsl:call-template name="MEASegment">
                  <xsl:with-param name="Code" select="'AAC'"/>
                  <xsl:with-param name="Unit" select="$NetWeightUnit"/>
                  <xsl:with-param name="Value" select="$NetWeightRounded"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test ="number($VolumeRounded)=number($VolumeRounded) and number($VolumeRounded)>0 and $VolumeUnit">
                <xsl:call-template name="MEASegment">
                  <xsl:with-param name="Code" select="'AAW'"/>
                  <xsl:with-param name="Unit" select="$VolumeUnit"/>
                  <xsl:with-param name="Value" select="$VolumeRounded"/>
                </xsl:call-template>
              </xsl:when>
            </xsl:choose>
            <xsl:if test="number($NoOfPacks)=number($NoOfPacks) and number($NoOfPacks)>0 and $NoOfPacksUnit">
              <ns0:QTY_2>
                <ns0:TAGNAME>QTY</ns0:TAGNAME>
                <ns0:C186_2>
                  <ns0:C18601>47</ns0:C18601>
                  <ns0:C18602>
                    <xsl:value-of select="$NoOfPacks"/>
                  </ns0:C18602>
                  <ns0:C18603>
                    <xsl:value-of select="$NoOfPacksUnit"/>
                  </ns0:C18603>
                </ns0:C186_2>
              </ns0:QTY_2>
            </xsl:if>
          </ns0:SG68Loop>

          <!-- Region: GAGI Group Generation -->
          <xsl:variable name="CommercialInvoice" select="."/>

          <!-- Region: Get all Invoice linked PackingLine GenIDs -->
          <xsl:variable name="GetInvoiceLinks_ResetLinkedsIDs" select="userCSharp:ResetLinkedIDs()"/>
          <xsl:for-each select="*[local-name()='PackingLinkCollection']/*[local-name()='PackingLink']">
            <xsl:variable name="Link" select="*[local-name()='PackingLineLink']/text()"/>
            <xsl:variable name="GetInvoiceLinks_RegisterLinkedNumber" select="userCSharp:RegisterLinkedID(concat('LinkStart-', $Link, '-LinkEnd'))"/>
          </xsl:for-each>
          <xsl:variable name="InvoiceLinks" select="userCSharp:GetAllLinkedIDs()"/>

          <xsl:variable name="ResetLinkedIDs" select="userCSharp:ResetLinkedIDs()"/>
          <xsl:variable name="ResetInvoiceLinkedPackingIDs" select="userCSharp:ResetInvoiceLinkedPackingIDs()"/>
          <xsl:call-template name="GetInvoiceLinkedPackingIDs">
            <xsl:with-param name="InvoiceLinks" select="$InvoiceLinks"/>
            <xsl:with-param name="PackingLineCollection" select="$PackingLineCollection"/>
          </xsl:call-template>
          <xsl:variable name="SetInvoiceLinkedPackingIDs" select="userCSharp:SetInvoiceLinkedPackingIDs(userCSharp:GetAllLinkedIDs())"/>
          <!-- Region: Get all Invoice Lines linked PackingLine GenIDs -->
          <xsl:variable name="ResetInvoiceLineLinkedPackingIDs" select="userCSharp:ResetInvoiceLineLinkedPackingIDs()"/>
          <xsl:for-each select="*[local-name()='CommercialInvoiceLineCollection']/*[local-name()='CommercialInvoiceLine']">
            <xsl:variable name="CommercialInvoiceLineID" select="generate-id()"/>
            <xsl:variable name="Link" select="*[local-name()='Link']/text()"/>

            <xsl:variable name="ResetIDs" select="userCSharp:ResetLinkedIDs()"/>
            <xsl:call-template name="GetInvoiceLineLinkedPackingIDs">
              <xsl:with-param name="InvoiceLineLink" select="$Link"/>
              <xsl:with-param name="PackingLineCollection" select="$PackingLineCollection"/>
            </xsl:call-template>
            <xsl:variable name="AddCommercialInvoiceLineID" select="userCSharp:AddInvoiceLineLinkedPackingIDs($CommercialInvoiceLineID, userCSharp:GetAllLinkedIDs())"/>
          </xsl:for-each>

          <xsl:variable name="SimplifiedPackingLineCollection">
            <xsl:call-template name="SimplifiedPackingLineCollection">
              <xsl:with-param name="PackingLineCollection" select="$PackingLineCollection"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name="SimplifiedCommercialInvoiceLineCollection">
            <xsl:call-template name="SimplifiedCommercialInvoiceLineCollection">
              <xsl:with-param name="CommercialInvoiceLineCollection" select="*[local-name()='CommercialInvoiceLineCollection']"/>
            </xsl:call-template>
          </xsl:variable>

          <!-- Region: Invoice will have it's own group, as once the packages assigned to invoice will never be assigned to any line. -->
          <xsl:if test="(translate($InvoiceLinks, ' ', '') != '') and (userCSharp:GetAllInvoiceLineIDs(true()) != userCSharp:GetAllInvoiceLineIDs(false()))">
            <xsl:call-template name="SG101Loop">
              <xsl:with-param name="CommercialInvoice" select="$CommercialInvoice"/>
              <xsl:with-param name="PackingLineCollection" select="$PackingLineCollection"/>
              <xsl:with-param name="IsInvoiceLoop" select="true()"/>
              <xsl:with-param name="InvoicePackingLinks" select="$InvoiceLinks"/>
              <xsl:with-param name="PackingLinesLevels" select="$PackingLinesLevels"/>
              <xsl:with-param name="IsNotLastSG47Loop" select="$IsNotLastSG47Loop"/>
            </xsl:call-template>
          </xsl:if>

          <!-- Region: Invoice lines will be grouped by their packages, only when they have exactly the same amount and same packages will be grouped together. -->
          <xsl:variable name="Groups" select="ScriptNS4:GroupByPackingLine(
                        msxsl:node-set($SimplifiedCommercialInvoiceLineCollection)/CommercialInvoiceLineCollection, 
                        msxsl:node-set($SimplifiedPackingLineCollection)/PackingLineCollection)"/>
          <xsl:for-each select="msxsl:node-set($Groups)/Group">
            <xsl:if test="(userCSharp:GetInvoiceLineLinkedPackingIDs(./CommercialInvoiceLineID[1]/text()) != '') or (translate($InvoiceLinks, ' ', '') = '')">
              <xsl:variable name="IsLastSG101Loop" select="position() = last()"/>
              <xsl:call-template name="SG101Loop">
                <xsl:with-param name="CommercialInvoice" select="$CommercialInvoice"/>
                <xsl:with-param name="PackingLineCollection" select="$PackingLineCollection"/>
                <xsl:with-param name="PackingLinesLevels" select="$PackingLinesLevels"/>
                <xsl:with-param name="IsNotLastSG47Loop" select="$IsNotLastSG47Loop"/>
                <xsl:with-param name="IsLastSG101Loop" select="$IsLastSG101Loop"/>
              </xsl:call-template>
            </xsl:if>
          </xsl:for-each>

        </ns0:SG47Loop>
      </xsl:for-each>

      <ns0:HYN>
        <ns0:TAGNAME>HYN</ns0:TAGNAME>
        <ns0:HYN1>3</ns0:HYN1>
      </ns0:HYN>
      <ns0:UNS>
        <ns0:TAGNAME>UNS</ns0:TAGNAME>
        <ns0:UNS1>S</ns0:UNS1>
      </ns0:UNS>
      <ns0:UNT>
        <ns0:TAGNAME>UNT</ns0:TAGNAME>
        <ns0:UNT1>XXX</ns0:UNT1>
        <ns0:UNT2>
          <xsl:value-of select="$MessageRefNumber"/>
        </ns0:UNT2>
      </ns0:UNT>
    </ns0:EFACT_D13A_GOVCBR>
  </xsl:template>

  <xsl:template name="PackingLinesLevels">
    <xsl:param name="PackingLine"/>
    <xsl:param name="ParentPackingLineIDs" select="''"/>

    <xsl:variable name="GenID" select="generate-id()"/>
    <xsl:variable name="SubPackingLines" select="$PackingLine/*[local-name()='PackingLineCollection']/*[local-name()='PackingLine']"/>
    <xsl:choose>
      <xsl:when test="count($SubPackingLines) > 0">
        <xsl:for-each select="$SubPackingLines">
          <xsl:call-template name="PackingLinesLevels">
            <xsl:with-param name="PackingLine" select="."/>
            <xsl:with-param name="ParentPackingLineIDs" select="concat($ParentPackingLineIDs, ' ', $GenID)"/>
          </xsl:call-template>
        </xsl:for-each>
      </xsl:when>
      <xsl:otherwise>
        <xsl:variable name="AddEndingPackingLineRelationships" select="userCSharp:AddEndingPackingLineRelationships($GenID, $ParentPackingLineIDs)"/>
      </xsl:otherwise>
    </xsl:choose>

    <xsl:variable name="EndingIDs">
      <xsl:variable name="AllEndingIDs" select="userCSharp:GetAllEndingPackingLineIDs($GenID)"/>
      <xsl:choose>
        <xsl:when test="$AllEndingIDs != ''">
          <xsl:value-of select="$AllEndingIDs"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$GenID"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <Level packingLineID="{$GenID}" endingPackingLineIDs="{normalize-space($EndingIDs)}"/>
  </xsl:template>

  <xsl:template name="GetInvoiceLinkedPackingIDs">
    <xsl:param name="InvoiceLinks"/>
    <xsl:param name="PackingLineCollection"/>

    <xsl:variable name="ResetInvoiceLinkedPackingLineFlag" select="userCSharp:ResetInvoiceLinkedPackingLineFlag()"/>

    <xsl:for-each select="$PackingLineCollection/*[local-name()='PackingLine']">
      <xsl:choose>
        <xsl:when test="contains($InvoiceLinks, concat('LinkStart-', normalize-space(*[local-name()='Link']/text()), '-LinkEnd'))">
          <xsl:variable name="SetInvoiceLinkedPackingLineFlag" select="userCSharp:SetInvoiceLinkedPackingLineFlag()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:if test="*[local-name()='PackingLineCollection']">
            <xsl:call-template name="GetInvoiceLinkedPackingIDs">
              <xsl:with-param name="InvoiceLinks" select="$InvoiceLinks"/>
              <xsl:with-param name="PackingLineCollection" select="*[local-name()='PackingLineCollection']"/>
            </xsl:call-template>
          </xsl:if>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:variable name="InvoiceLinkedPackingLineFlag" select="userCSharp:GetInvoiceLinkedPackingLineFlag()"/>
      <xsl:if test="$InvoiceLinkedPackingLineFlag">
        <xsl:variable name="RegisterThisID" select="userCSharp:RegisterLinkedID(generate-id())"/>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="GetInvoiceLineLinkedPackingIDs">
    <xsl:param name="InvoiceLineLink"/>
    <xsl:param name="PackingLineCollection"/>

    <xsl:for-each select="$PackingLineCollection/*[local-name()='PackingLine']">
      <xsl:if test="count(*[local-name()='PackedItemCollection']/*[local-name()='PackedItem'][./*[local-name()='CommercialInvoiceLineLink']/text() = $InvoiceLineLink]/*[local-name()='PackedQuantity']) > 0">
        <xsl:variable name="RegisterID" select="userCSharp:RegisterLinkedID(generate-id())"/>
        <xsl:if test="*[local-name()='PackingLineCollection']">
          <xsl:call-template name="GetInvoiceLineLinkedPackingIDs">
            <xsl:with-param name="InvoiceLineLink" select="$InvoiceLineLink"/>
            <xsl:with-param name="PackingLineCollection" select="*[local-name()='PackingLineCollection']"/>
          </xsl:call-template>
        </xsl:if>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>
    
  <xsl:template name="SimplifiedCommercialInvoiceLineCollection">
    <xsl:param name="CommercialInvoiceLineCollection"/>

    <CommercialInvoiceLineCollection>
      <xsl:for-each select="$CommercialInvoiceLineCollection/*[local-name()='CommercialInvoiceLine']">
        <xsl:variable name="HarmonisedCode" select="*[local-name()='HarmonisedCode']"/>
        <xsl:variable name="ForDummyCasualLine" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IsAutoDummyHSCodeCasualImportLine']/*[local-name()='Value']/text()='Y'"/>
        <xsl:variable name="IsCasualImportLine" select="boolean($ForDummyCasualLine) and starts-with($HarmonisedCode,'00009999')"/>
        <xsl:if test="not(boolean($IsCasualImportLine))">
        <CommercialInvoiceLine>
          <ID>
            <xsl:value-of select="generate-id()"/>
          </ID>
          <Link>
            <xsl:value-of select="*[local-name()='Link']"/>
          </Link>
        </CommercialInvoiceLine>
        </xsl:if>
      </xsl:for-each>
    </CommercialInvoiceLineCollection>

    </xsl:template>

  <xsl:template name="SimplifiedPackingLineCollection">
    <xsl:param name="PackingLineCollection"/>

    <PackingLineCollection>
      <xsl:for-each select="$PackingLineCollection/*[local-name()='PackingLine']">
        <PackingLine>
          <ID>
            <xsl:value-of select="generate-id()"/>
          </ID>
          <xsl:apply-templates mode="copy" select="*[local-name()='PackedItemCollection']"/>
          <xsl:if test="*[local-name()='PackingLineCollection']">
            <xsl:call-template name="SimplifiedPackingLineCollection">
              <xsl:with-param name="PackingLineCollection" select="*[local-name()='PackingLineCollection']"/>
            </xsl:call-template>
          </xsl:if>
        </PackingLine>
      </xsl:for-each>
    </PackingLineCollection>

  </xsl:template>

  <xsl:template match="*" mode="copy">
    <xsl:element name="{name()}">
      <xsl:apply-templates select="@*|node()" mode="copy" />
    </xsl:element>
  </xsl:template>

  <xsl:template match="@*|text()|comment()" mode="copy">
    <xsl:copy/>
  </xsl:template>

  <xsl:template name="SG3Loop">
    <xsl:param name="Shipment"/>
    <xsl:param name="AgencyCode"/>
    <xsl:param name="InfoGroupCode"/>
    <xsl:param name="InfoCode"/>

    <xsl:variable name="AddInfoGroup" select="$Shipment/*[local-name()='CommercialInfo']/*[local-name()='CommercialInvoiceCollection']/*[local-name()='CommercialInvoice']/*[local-name()='CommercialInvoiceLineCollection']/*[local-name()='CommercialInvoiceLine']/*[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup']/*[local-name()='Type'][./*[local-name()='Code']=$InfoGroupCode] | $Shipment/*[local-name()='CommercialInfo']/*[local-name()='SubGroupCollection']/*[local-name()='SubGroup']/*[local-name()='CommercialInvoiceCollection']/*[local-name()='CommercialInvoice']/*[local-name()='CommercialInfo']/*[local-name()='SubGroupCollection']/*[local-name()='SubGroup']/*[local-name()='CommercialInvoiceCollection']/*[local-name()='CommercialInvoice'] /*[local-name()='CommercialInvoiceLineCollection']/*[local-name()='CommercialInvoiceLine']/*[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup']/*[local-name()='Type'][./*[local-name()='Code']=$InfoGroupCode]"/>
    <xsl:variable name="ShouldOutput" select="$Shipment/*[local-name()='CommercialInfo']/*[local-name()='CommercialInvoiceCollection']/*[local-name()='CommercialInvoice']/*[local-name()='CommercialInvoiceLineCollection']/*[local-name()='CommercialInvoiceLine']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']=$InfoCode]/*[local-name()='Value'] | $Shipment/*[local-name()='CommercialInfo']/*[local-name()='SubGroupCollection']/*[local-name()='SubGroup']/*[local-name()='CommercialInvoiceCollection']/*[local-name()='CommercialInvoice']/*[local-name()='CommercialInvoiceLineCollection']/*[local-name()='CommercialInvoiceLine']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']=$InfoCode]/*[local-name()='Value']" />
    <xsl:if test="$AddInfoGroup and $ShouldOutput='Y'">
      <ns0:SG3Loop>
        <ns0:GOR>
          <ns0:TAGNAME>GOR</ns0:TAGNAME>
          <ns0:GOR01/>
          <ns0:C232>
            <ns0:C23201>
              <xsl:value-of select="$AgencyCode"/>
            </ns0:C23201>
          </ns0:C232>
        </ns0:GOR>
        <ns0:LOC>
          <ns0:TAGNAME>LOC</ns0:TAGNAME>
          <ns0:LOC01>274</ns0:LOC01>
          <ns0:C517/>
          <xsl:variable name="ExamLocationCode" select="$Shipment/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='ExamLocationCode']/*[local-name()='Value']"/>
          <xsl:variable name="ExamLocationName" select="$Shipment/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='ExamLocationName']/*[local-name()='Value']"/>
          <xsl:if test="$ExamLocationCode!='' or $ExamLocationName!=''">
            <ns0:C519>
              <xsl:if test="$ExamLocationCode">
                <ns0:C51901>
                  <xsl:value-of select="$ExamLocationCode"/>
                </ns0:C51901>
              </xsl:if>
              <xsl:if test="$ExamLocationName">
                <ns0:C51904>
                  <xsl:value-of select="userCSharp:EscapeString(substring($ExamLocationName, 1, 35))"/>
                </ns0:C51904>
              </xsl:if>
            </ns0:C519>
          </xsl:if>
        </ns0:LOC>
      </ns0:SG3Loop>
    </xsl:if>
  </xsl:template>

  <xsl:template name="SG13Loop">
    <xsl:param name="Enable"/>
    <xsl:param name="ExceptionCode"/>
    <xsl:param name="AgencyCode"/>
    <xsl:if test="$Enable">
      <ns0:SG13Loop>
        <ns0:RCS_1>
          <ns0:TAGNAME>RCS</ns0:TAGNAME>
          <ns0:RCS01>15</ns0:RCS01>
          <ns0:C550_1>
            <ns0:C55001>
              <xsl:value-of select="$ExceptionCode"/>
            </ns0:C55001>
            <ns0:C55003>
              <xsl:value-of select="$AgencyCode"/>
            </ns0:C55003>
          </ns0:C550_1>
        </ns0:RCS_1>
      </ns0:SG13Loop>
    </xsl:if>
  </xsl:template>

  <xsl:template name="SG125Loop">
    <xsl:param name="Enable"/>
    <xsl:param name="Code"/>
    <xsl:param name="Qualifier"/>
    <xsl:if test="$Enable">
      <ns0:SG125Loop>
        <ns0:RCS_2>
          <ns0:TAGNAME>RCS</ns0:TAGNAME>
          <ns0:RCS01>15</ns0:RCS01>
          <ns0:C550_2>
            <ns0:C55001>
              <xsl:value-of select="$Code"/>
            </ns0:C55001>
            <ns0:C55003>
              <xsl:value-of select="$Qualifier"/>
            </ns0:C55003>
          </ns0:C550_2>
        </ns0:RCS_2>
      </ns0:SG125Loop>
    </xsl:if>
  </xsl:template>
  
  <!-- Create an SG10 for each LPCO specified with the LPCOKey-->
  <xsl:template name="SG10ForEachLPCOkey">
    <xsl:param name="Shipment" />
    <xsl:param name="AddInfoGroup"/>
    <xsl:param name="LPCOKey" />
    <xsl:param name="PartyQualifier" />
    <xsl:variable name="LPCOValue" select="$AddInfoGroup/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']=$LPCOKey]/*[local-name()='Value']"/>
    <xsl:variable name="LPCOApplicantName" select="$AddInfoGroup/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='LPCOApplicantName']/*[local-name()='Value']"/>
    <xsl:variable name="LPCOHolderName" select="$AddInfoGroup/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='LPCOHolderName']/*[local-name()='Value']"/>
    <xsl:if test="$LPCOValue">
      <xsl:variable name="AddressType">
        <xsl:choose>
          <xsl:when test="$LPCOValue='IMP'">ImporterDocumentaryAddress</xsl:when>
          <xsl:when test="$LPCOValue='SUP'">SupplierPickupDeliveryAddress</xsl:when>
          <xsl:when test="$LPCOValue='IOR'">ImporterOfRecord</xsl:when>
          <xsl:when test="$LPCOValue='BRK'">CustomsBroker</xsl:when>
        </xsl:choose>
      </xsl:variable>
      <xsl:if test="$Shipment/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']=$AddressType]">
        <ns0:SG10Loop>
          <xsl:call-template name="NADSegment">
            <xsl:with-param name="OrgAddress" select="$Shipment/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']=$AddressType]"/>
            <xsl:with-param name="Type" select="$PartyQualifier"/>
            <xsl:with-param name="ContactLoopID" select="'11'"/>
            <xsl:with-param name="CLP" select="$AddInfoGroup"/>
            <xsl:with-param name="OverrideCompanyName">
              <xsl:choose>
                <xsl:when test="$LPCOKey='LPCOHolderType'">
                  <xsl:value-of select="$LPCOHolderName"/>
                </xsl:when>
                <xsl:when test="$LPCOKey='LPCOApplicant'">
                  <xsl:value-of select="$LPCOApplicantName"/>
                </xsl:when>
              </xsl:choose>
            </xsl:with-param>
          </xsl:call-template>
        </ns0:SG10Loop>
      </xsl:if>
      <xsl:if test="$LPCOValue='OTH' and $PartyQualifier='DFK'">
        <ns0:SG10Loop>
          <xsl:call-template name="NADSegment">
            <xsl:with-param name="OrgAddress" select="$AddInfoGroup/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='LPCOHolder']"/>
            <xsl:with-param name="Type" select="$PartyQualifier"/>
            <xsl:with-param name="ContactLoopID" select="'11'"/>
            <xsl:with-param name="CLP" select="$AddInfoGroup"/>
            <xsl:with-param name="OverrideCompanyName">
              <xsl:choose>
                <xsl:when test="$LPCOKey='LPCOHolderType'">
                  <xsl:value-of select="$LPCOHolderName"/>
                </xsl:when>
                <xsl:when test="$LPCOKey='LPCOApplicant'">
                  <xsl:value-of select="$LPCOApplicantName"/>
                </xsl:when>
              </xsl:choose>
            </xsl:with-param>
          </xsl:call-template>
        </ns0:SG10Loop>
      </xsl:if>
      <xsl:if test="$LPCOValue='OTH' and $PartyQualifier='DDD'">
        <ns0:SG10Loop>
          <xsl:call-template name="NADSegment">
            <xsl:with-param name="OrgAddress" select="$AddInfoGroup/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='LPCOApplicant']"/>
            <xsl:with-param name="Type" select="$PartyQualifier"/>
            <xsl:with-param name="ContactLoopID" select="'11'"/>
            <xsl:with-param name="CLP" select="$AddInfoGroup"/>
            <xsl:with-param name="OverrideCompanyName">
              <xsl:choose>
                <xsl:when test="$LPCOKey='LPCOHolderType'">
                  <xsl:value-of select="$LPCOHolderName"/>
                </xsl:when>
                <xsl:when test="$LPCOKey='LPCOApplicant'">
                  <xsl:value-of select="$LPCOApplicantName"/>
                </xsl:when>
              </xsl:choose>
            </xsl:with-param>
          </xsl:call-template>
        </ns0:SG10Loop>
      </xsl:if>
    </xsl:if>
  </xsl:template>

  <xsl:template name="CCNRFF">
    <xsl:for-each select="./*[local-name()='CustomsReferenceCollection']/*[local-name()='CustomsReference'][./*[local-name()='Type']/*[local-name()='Code']/text() = 'CCN']">
      <xsl:variable name="CCNReferenceNumber" select="*[local-name()='Reference']"/>
      <xsl:if test="$CCNReferenceNumber != ''">
        <ns0:RFF_5>
          <ns0:TAGNAME>RFF</ns0:TAGNAME>
          <ns0:C506_5>
            <ns0:C50601>CN</ns0:C50601>
            <ns0:C50602>
              <xsl:value-of select="userCSharp:EscapeString($CCNReferenceNumber)" />
            </ns0:C50602>
          </ns0:C506_5>
        </ns0:RFF_5>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="SG101Loop">
    <xsl:param name="CommercialInvoice"/>
    <xsl:param name="PackingLineCollection"/>
    <xsl:param name="IsInvoiceLoop" select="false()"/>
    <xsl:param name="InvoicePackingLinks"/>
    <xsl:param name="PackingLinesLevels"/>
    <xsl:param name="IsNotLastSG47Loop"/>
    <xsl:param name="IsLastSG101Loop" select="true()"/>

    <xsl:variable name="FirstCommercialInvoiceLineID">
      <xsl:if test="$IsInvoiceLoop = false()">
        <xsl:value-of select="./CommercialInvoiceLineID[1]/text()"/>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="FirstInvoiceLineForLineLoop" select="$CommercialInvoice/*[local-name()='CommercialInvoiceLineCollection']/*[local-name()='CommercialInvoiceLine'][generate-id()=$FirstCommercialInvoiceLineID]"/>
    <xsl:variable name="FirstInvoiceLineForInvoiceLoop" select="$CommercialInvoice/*[local-name()='CommercialInvoiceLineCollection']/*[local-name()='CommercialInvoiceLine'][1]"/>
    <ns0:SG101Loop>
      <ns0:LIN_1>
        <ns0:TAGNAME>LIN</ns0:TAGNAME>
        <ns0:LIN01>
          <xsl:number value="userCSharp:SG101Count()" />
        </ns0:LIN01>
      </ns0:LIN_1>

      <xsl:if test="$CommercialInvoice/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='Manufacturer']">
        <ns0:SG102Loop>
          <xsl:call-template name="NADSegment">
            <xsl:with-param name="Type" select="'MF'"/>
            <xsl:with-param name="OrgAddress" select="$CommercialInvoice/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='Manufacturer']"/>
            <xsl:with-param name="UseDefaultPartyID" select="false()"/>
            <xsl:with-param name="ContactLoopID" select="'103'"/>
          </xsl:call-template>
        </ns0:SG102Loop>
      </xsl:if>

      <!-- Region: Country Of Origin -->
      <ns0:SG104Loop>
        <ns0:LOC_4>
          <ns0:TAGNAME>LOC</ns0:TAGNAME>
          <ns0:LOC01>
            <xsl:value-of select="'27'"/>
          </ns0:LOC01>

          <xsl:variable name="COOInvoiceline">
            <xsl:choose>
              <xsl:when test="$IsInvoiceLoop = false()">
                <xsl:value-of select="$FirstInvoiceLineForLineLoop/*[local-name()='CountryOfOrigin']/*[local-name()='Code']/text()"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$FirstInvoiceLineForInvoiceLoop/*[local-name()='CountryOfOrigin']/*[local-name()='Code']/text()"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <xsl:variable name="COOInvoice" select="$CommercialInvoice/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='CountryOfOrigin']/*[local-name()='Value']/text()"/>
          <xsl:variable name="COOFallback">
            <xsl:choose>
              <xsl:when test="$COOInvoiceline != ''">
                <xsl:value-of select="$COOInvoiceline"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$COOInvoice"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
            <ns0:C517_4>
              <ns0:C51701>
                <xsl:value-of select="$COOFallback"/>
              </ns0:C51701>
            </ns0:C517_4>
          <xsl:variable name="ProvinceOfOrigin" select="$CommercialInvoice/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ProvinceOfOrigin']/*[local-name()='Value']/text()"/>
          <xsl:variable name="IIDRegion" select="$CommercialInvoice/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IIDRegion']/*[local-name()='Value']/text()"/>
          <xsl:variable name="ProvinceOfOriginFallback">
            <xsl:choose>
              <xsl:when test="$IsInvoiceLoop = false()">
                <xsl:value-of select="$FirstInvoiceLineForLineLoop/*[local-name()='StateOfOrigin']/*[local-name()='Code']/text()"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$FirstInvoiceLineForInvoiceLoop/*[local-name()='StateOfOrigin']/*[local-name()='Code']/text()"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <xsl:variable name="IIDRegionFallback">
            <xsl:choose>
              <xsl:when test="$IsInvoiceLoop = false()">
                <xsl:value-of select="$FirstInvoiceLineForLineLoop/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IIDRegion']/*[local-name()='Value']/text()"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$FirstInvoiceLineForInvoiceLoop/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IIDRegion']/*[local-name()='Value']/text()"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <xsl:if test="($ProvinceOfOrigin != '') or ($IIDRegion != '') or ($ProvinceOfOriginFallback != '') or ($IIDRegionFallback != '')">
            <ns0:C519_3>
              <xsl:choose>
                <xsl:when test="$ProvinceOfOrigin!=''">
                  <ns0:C51901>
                    <xsl:value-of select="$ProvinceOfOrigin"/>
                  </ns0:C51901>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:if test="$ProvinceOfOriginFallback != ''">
                    <ns0:C51901>
                      <xsl:value-of select="$ProvinceOfOriginFallback"/>
                    </ns0:C51901>
                  </xsl:if>
                </xsl:otherwise>
              </xsl:choose>
              <xsl:choose>
                <xsl:when test="$IIDRegion!=''">
                  <ns0:C51904>
                    <xsl:value-of select="$IIDRegion"/>
                  </ns0:C51904>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:if test="$IIDRegionFallback != ''">
                    <ns0:C51904>
                      <xsl:value-of select="$IIDRegionFallback"/>
                    </ns0:C51904>
                  </xsl:if>
                </xsl:otherwise>
              </xsl:choose>
            </ns0:C519_3>
          </xsl:if>
        </ns0:LOC_4>
      </ns0:SG104Loop>

      <!-- Region: Country Of Source -->
      <xsl:variable name="CountryOfSource">
      <xsl:variable name="CFIAData" select="$CommercialInvoice/*[local-name()='CommercialInvoiceLineCollection']/*[local-name()='CommercialInvoiceLine']/*[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup'][./*[local-name()='Type']/*[local-name()='Code']/text()='CCF']"/>
      <xsl:variable name="ECCCData" select="$CommercialInvoice/*[local-name()='CommercialInvoiceLineCollection']/*[local-name()='CommercialInvoiceLine']/*[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup'][./*[local-name()='Type']/*[local-name()='Code']/text()='CEC']"/>
      <xsl:if test="(boolean($CFIAData) or boolean($ECCCData))">
        <xsl:variable name="value" select="$CommercialInvoice/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='RN_NKSource']/*[local-name()='Value']/text()"/>
        <xsl:choose>
            <xsl:when test="$value!=''">
              <xsl:value-of select="$value"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:choose>
                <xsl:when test="$IsInvoiceLoop = false()">
                  <xsl:value-of select="$FirstInvoiceLineForLineLoop/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='RN_NKSource']/*[local-name()='Value']/text()"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$FirstInvoiceLineForInvoiceLoop/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='RN_NKSource']/*[local-name()='Value']/text()"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:otherwise>
        </xsl:choose>
      </xsl:if>
      </xsl:variable>
      <xsl:if test="$CountryOfSource!=''">
        <ns0:SG104Loop>
          <ns0:LOC_4>
            <ns0:TAGNAME>LOC</ns0:TAGNAME>
            <ns0:LOC01>
              <xsl:value-of select="'30'"/>
            </ns0:LOC01>
            <ns0:C517_4>
              <ns0:C51701>
                <xsl:value-of select="$CountryOfSource"/>
              </ns0:C51701>
            </ns0:C517_4>
            <xsl:variable name="StateOfSource" select="$CommercialInvoice/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='StateOfSource']/*[local-name()='Value']/text()"/>
            <xsl:variable name="StateOfSourceFallback">
              <xsl:choose>
                <xsl:when test="$IsInvoiceLoop = false()">
                  <xsl:value-of select="$FirstInvoiceLineForLineLoop/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='StateOfSource']/*[local-name()='Value']/text()"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$FirstInvoiceLineForInvoiceLoop/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='StateOfSource']/*[local-name()='Value']/text()"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <xsl:if test="($StateOfSource != '') or ($StateOfSourceFallback != '')">
              <ns0:C519_3>
                <ns0:C51901>
                  <xsl:choose>
                    <xsl:when test="$StateOfSource!=''">
                      <xsl:value-of select="$StateOfSource"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="$StateOfSourceFallback"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </ns0:C51901>
              </ns0:C519_3>
            </xsl:if>
          </ns0:LOC_4>
        </ns0:SG104Loop>
      </xsl:if>

      <!-- Region: SG115 Packages -->
      <xsl:variable name="LinkedPackingLineIDs">
        <xsl:choose>
          <xsl:when test="$IsInvoiceLoop = false()">
            <xsl:value-of select="userCSharp:GetInvoiceLineLinkedPackingIDs(./CommercialInvoiceLineID[1]/text())"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="userCSharp:GetInvoiceLinkedPackingIDs()"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="Links">
        <xsl:choose>
          <xsl:when test="$IsInvoiceLoop = false()">
            <xsl:for-each select="CommercialInvoiceLineID">
              <xsl:variable name="CurrentID" select="text()"/>
              <xsl:value-of select="$CommercialInvoice/*[local-name()='CommercialInvoiceLineCollection']/*[local-name()='CommercialInvoiceLine'][generate-id() = $CurrentID]/*[local-name()='Link']"/>
              <xsl:value-of select="' '"/>
            </xsl:for-each>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$InvoicePackingLinks"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:for-each select="$PackingLineCollection/*[local-name()='PackingLine']">
        <xsl:call-template name="SG115Loop">
          <xsl:with-param name="Links" select="msxsl:node-set($Links)/text()"/>
          <xsl:with-param name="PackingLinesLevels" select="$PackingLinesLevels"/>
          <xsl:with-param name="IsInvoiceLoop" select="$IsInvoiceLoop"/>
          <xsl:with-param name="CommercialInvoice" select="$CommercialInvoice"/>
          <xsl:with-param name="LinkedPackingLineIDs" select="$LinkedPackingLineIDs"/>
        </xsl:call-template>
      </xsl:for-each>
      <xsl:call-template name="SG115LoopPrint">
        <xsl:with-param name="count">
          <xsl:value-of select="userCSharp:GetPackLineCollectionCount()"/>
        </xsl:with-param>
        <xsl:with-param name="index">
          <xsl:value-of select="number(0)"/>
        </xsl:with-param>
      </xsl:call-template>
      <xsl:variable name="ResetIDs" select="userCSharp:ResetPackingLines()"/>

      <xsl:variable name="ResetPosition" select="userCSharp:ResetSG117Count()"/>
      <xsl:choose>
        <xsl:when test="$IsInvoiceLoop = false()">
          <xsl:for-each select="CommercialInvoiceLineID">
            <xsl:variable name="CurrentID" select="text()"/>
            <xsl:for-each select="$CommercialInvoice/*[local-name()='CommercialInvoiceLineCollection']/*[local-name()='CommercialInvoiceLine'][generate-id()=$CurrentID]">
              <xsl:variable name="IsLastSG117Loop" select="position() = last()"/>
              <xsl:call-template name="SG117Loop">
                <xsl:with-param name="ID" select="userCSharp:SG117Count()"/>
                <xsl:with-param name="InvoiceCurrency" select="$CommercialInvoice/*[local-name()='InvoiceCurrency']/*[local-name()='Code']"/>
                <xsl:with-param name="IsNotLastSG47Loop" select="$IsNotLastSG47Loop"/>
                <xsl:with-param name="IsLastSG101Loop" select="$IsLastSG101Loop"/>
                <xsl:with-param name="IsLastSG117Loop" select="$IsLastSG117Loop"/>
              </xsl:call-template>
            </xsl:for-each>
          </xsl:for-each>
        </xsl:when>
        <xsl:otherwise>
          <xsl:for-each select="$CommercialInvoice/*[local-name()='CommercialInvoiceLineCollection']/*[local-name()='CommercialInvoiceLine'][not(contains(userCSharp:GetAllInvoiceLineIDs(true()), generate-id()))]">
            <xsl:variable name="IsLastSG117Loop" select="position() = last()"/>
            <xsl:call-template name="SG117Loop">
              <xsl:with-param name="ID" select="userCSharp:SG117Count()"/>
              <xsl:with-param name="InvoiceCurrency" select="$CommercialInvoice/*[local-name()='InvoiceCurrency']/*[local-name()='Code']"/>
              <xsl:with-param name="IsNotLastSG47Loop" select="$IsNotLastSG47Loop"/>
              <xsl:with-param name="IsLastSG101Loop" select="$IsLastSG101Loop"/>
              <xsl:with-param name="IsLastSG117Loop" select="$IsLastSG117Loop"/>
            </xsl:call-template>
          </xsl:for-each>
        </xsl:otherwise>
      </xsl:choose>
    </ns0:SG101Loop>
  </xsl:template>

  <xsl:template name="SG115Loop">
    <xsl:param name="Links"/>
    <xsl:param name="LinkedPackingLineIDs"/>
    <xsl:param name="CommercialInvoice"/>
    <xsl:param name="PackingLinesLevels"/>
    <xsl:param name="IsInvoiceLoop" select="false()"/>
    <xsl:param name="CurrentLevel" select="3"/>

    <xsl:variable name="GenID" select="generate-id()"/>
    <xsl:if test="contains($LinkedPackingLineIDs, $GenID)">
      <xsl:variable name="LevelNode" select="msxsl:node-set($PackingLinesLevels)/Level[@packingLineID = $GenID]"/>

      <xsl:variable name="LevelNum">
        <xsl:choose>
          <xsl:when test="($CurrentLevel = 3 and $LevelNode/@endingPackingLineIDs = $GenID) or ($CurrentLevel &lt;= 0)">
            <xsl:value-of select="4"/>
          </xsl:when>
          <xsl:when test="$CurrentLevel = 2 and $LevelNode/@endingPackingLineIDs = $GenID">
            <xsl:value-of select="1"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$CurrentLevel"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="Qty">
        <xsl:choose>
          <xsl:when test="$GenID = $LevelNode/@endingPackingLineIDs">
            <xsl:choose>
              <xsl:when test="$IsInvoiceLoop = false()">
                <xsl:value-of select="sum(*[local-name()='PackedItemCollection']/*[local-name()='PackedItem'][contains($Links, *[local-name()='CommercialInvoiceLineLink'])]/*[local-name()='PackedQuantity'])"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:variable name="CurrentPackingLineLink" select="*[local-name()='Link']/text()"/>
                <xsl:value-of select="sum($CommercialInvoice/*[local-name()='PackingLinkCollection']/*[local-name()='PackingLink'][./*[local-name()='PackingLineLink']/text()=$CurrentPackingLineLink]/*[local-name()='PackedQuantity'])"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="*[local-name()='PackQty']"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:if test="$Qty != 0">
        <xsl:variable name="QtyNumber" select="number($Qty)"/>
        <xsl:variable name="PackTypeCode" select="*[local-name()='PackType']/*[local-name()='Code']"></xsl:variable>
        <xsl:variable name="MarksAndNos" select="normalize-space(*[local-name()='MarksAndNos']/text())"/>
        <xsl:variable name="PackingLineDetails" select="userCSharp:AddPackingLineDetailsCollection($QtyNumber, $PackTypeCode, $LevelNum, $MarksAndNos, $IsInvoiceLoop)"/>
      </xsl:if>

      <xsl:for-each select="*[local-name()='PackingLineCollection']/*[local-name()='PackingLine']">
        <xsl:call-template name="SG115Loop">
          <xsl:with-param name="Links" select="$Links"/>
          <xsl:with-param name="LinkedPackingLineIDs" select="$LinkedPackingLineIDs"/>
          <xsl:with-param name="CommercialInvoice" select="$CommercialInvoice"/>
          <xsl:with-param name="PackingLinesLevels" select="$PackingLinesLevels"/>
          <xsl:with-param name="IsInvoiceLoop" select="$IsInvoiceLoop"/>
          <xsl:with-param name="CurrentLevel" select="$CurrentLevel - 1"/>
        </xsl:call-template>
      </xsl:for-each>

    </xsl:if>
  </xsl:template>

  <xsl:template name="SG115LoopPrint">
    <xsl:param name="count"></xsl:param>
    <xsl:param name="index"></xsl:param>

    <xsl:if test="$index &lt; $count">
      <ns0:SG115Loop>
        <ns0:PAC_1>
          <ns0:TAGNAME>PAC</ns0:TAGNAME>
          <ns0:PAC01>
            <xsl:value-of select="userCSharp:GetPackLineQtyNumber($index)"/>
          </ns0:PAC01>
          <ns0:C531>
            <ns0:C53101>
              <xsl:value-of select="userCSharp:GetPackLineLevel($index)"/>
            </ns0:C53101>
          </ns0:C531>
          <ns0:C202>
            <ns0:C20201>
              <xsl:value-of select="userCSharp:GetPackLinePackTypeCode($index)"/>
            </ns0:C20201>
          </ns0:C202>
        </ns0:PAC_1>
        <ns0:SEQ_3>
          <ns0:TAGNAME>SEQ</ns0:TAGNAME>
          <ns0:SEQ01>1</ns0:SEQ01>
        </ns0:SEQ_3>
        <xsl:variable name="MarksAndNos" select="userCSharp:GetPackLineMarksAndNos($index)"/>
        <xsl:if test="$MarksAndNos != ''">
          <ns0:PCI>
            <ns0:TAGNAME>PCI</ns0:TAGNAME>
            <ns0:PCI01/>
            <ns0:C210_1>
              <ns0:C21001>
                <xsl:value-of select="userCSharp:EscapeString($MarksAndNos)"/>
              </ns0:C21001>
            </ns0:C210_1>
          </ns0:PCI>
        </xsl:if>
      </ns0:SG115Loop>
      <xsl:call-template name="SG115LoopPrint">
        <xsl:with-param name="count">
          <xsl:value-of select="$count"/>
        </xsl:with-param>
        <xsl:with-param name="index">
          <xsl:value-of select="$index+1"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name="SG117Loop">
    <xsl:param name="ID"/>
    <xsl:param name="InvoiceCurrency"/>
    <xsl:param name="IsNotLastSG47Loop"/>
    <xsl:param name="IsLastSG101Loop"/>
    <xsl:param name="IsLastSG117Loop"/>

    <xsl:variable name="HazardousMaterialCode">
      <xsl:choose>
        <xsl:when test="*[local-name()='HazardousMaterial']/*[local-name()='Code'] != ''">
          <xsl:value-of select="substring(*[local-name()='HazardousMaterial']/*[local-name()='Code'], 1, 4)"/>
        </xsl:when>
        <xsl:when test="boolean(*[local-name()='HazardousMaterial']/*[local-name()='UNDGCollection']/*[local-name()='UNDG'][*[local-name()='UNDGCode']/text() != ''])">
          <xsl:value-of select="substring(*[local-name()='HazardousMaterial']/*[local-name()='UNDGCollection']/*[local-name()='UNDG'][*[local-name()='UNDGCode']/text() != '']/*[local-name()='UNDGCode']/text(), 1, 4)"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="CFIAData" select="*[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup'][./*[local-name()='Type']/*[local-name()='Code']/text()='CCF']"/>
    <xsl:variable name="CNSCData" select="*[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup'][./*[local-name()='Type']/*[local-name()='Code']/text()='CCN']"/>
    <xsl:variable name="ECCCData" select="*[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup'][./*[local-name()='Type']/*[local-name()='Code']/text()='CEC']"/>
    <xsl:variable name="DFOData" select="*[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup'][./*[local-name()='Type']/*[local-name()='Code']/text()='CFO']"/>
    <xsl:variable name="GACData" select="*[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup'][./*[local-name()='Type']/*[local-name()='Code']/text()='CGA']"/>
    <xsl:variable name="NRCanData" select="*[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup'][./*[local-name()='Type']/*[local-name()='Code']/text()='CNR']"/>
    <xsl:variable name="PHACData" select="*[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup'][./*[local-name()='Type']/*[local-name()='Code']/text()='CPH']"/>
    <xsl:variable name="TCData" select="*[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup'][./*[local-name()='Type']/*[local-name()='Code']/text()='CTC']"/>
    <xsl:variable name="HCData" select="*[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup'][./*[local-name()='Type']/*[local-name()='Code']/text()='CHC']"/>

    <xsl:variable name="CustomsQuantity" select="substring(*[local-name()='CustomsQuantity'],1,9)"/>
    <xsl:variable name="CustomsQuantityUnit" select="*[local-name()='CustomsQuantityUnit']/*[local-name()='Code']"/>
    <xsl:variable name="NetWeightValue" select="substring(*[local-name()='NetWeight'],1,9)"/>
    <xsl:variable name="NetWeightUnit" select="*[local-name()='NetWeightUnit']/*[local-name()='Code']"/>

    <xsl:variable name="InvoiceQuantity" select="substring(*[local-name()='InvoiceQuantity'],1,9)"/>
    <xsl:variable name="InvoiceQuantityUnit" select="*[local-name()='InvoiceQuantityUnit']/*[local-name()='Code']"/>

    <xsl:variable name="LineNo" select="*[local-name()='LineNo']"/>
    <xsl:variable name="CountryOfOriginOnInvoiceLine" select="*[local-name()='CountryOfOrigin']/*[local-name()='Code']"/>
    <xsl:variable name="HarmonisedCode" select="*[local-name()='HarmonisedCode']"/>
    <xsl:variable name="IsCasualImportLine" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][*[local-name()='Key']='IsAutoDummyHSCodeCasualImportLine']/*[local-name()='Value']='Y' and starts-with($HarmonisedCode,'00009999')"/>
    <xsl:variable name="isGACWithCountQuantity" select="boolean($GACData) and number($CustomsQuantity)>0 and contains('|DZN|DPR|CEN|NMB|NAP|NPL|SET|PAR|PCE|MIL|SCO|', concat('|', $CustomsQuantityUnit, '|'))" />
    <xsl:variable name="isGACWithWeightQuantity" select="boolean($GACData) and number($CustomsQuantity)>0 and contains('|CTM|KGM|LTR|LBR|MC|KG|L|LB|', concat('|', $CustomsQuantityUnit, '|'))" />
    <xsl:variable name="IsDFODataValid" select="boolean($DFOData) and ($DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ABIProgramInd']/*[local-name()='Value']/text()='Y' or $DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='AISProgramInd']/*[local-name()='Value']/text()='Y' or $DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='TTPProgramInd']/*[local-name()='Value']/text()='Y')"/>

    <xsl:if test="not(boolean($IsCasualImportLine))">
    <ns0:SG117Loop>
      <ns0:GID_1>
        <ns0:TAGNAME>GID</ns0:TAGNAME>
        <ns0:GID01>
          <xsl:value-of select="$ID"/>
        </ns0:GID01>
      </ns0:GID_1>

      <!-- Region: UnConditional IMD -->
      <xsl:variable name="DefaultIMD" select="substring(*[local-name()='Description'], 1, 256)"/>
      <xsl:if test="$DefaultIMD!='' and not(boolean($IsCasualImportLine))">
        <ns0:IMD>
          <ns0:TAGNAME>IMD</ns0:TAGNAME>
          <ns0:IMD01/>
          <ns0:C272_1>
            <ns0:C27201>8</ns0:C27201>
          </ns0:C272_1>
          <ns0:C273_1>
            <ns0:C27304>
              <xsl:value-of select="userCSharp:EscapeString(normalize-space($DefaultIMD))"/>
            </ns0:C27304>
          </ns0:C273_1>
        </ns0:IMD>
      </xsl:if>

      <xsl:if test="boolean($CNSCData)">
        <xsl:choose>
          <xsl:when test="$CNSCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='Category']/*[local-name()='Value']/text()='NE'">
            <xsl:variable name="NNIECRSche" select="$CNSCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='NNIECRSchePartNo']/*[local-name()='Value']"/>
            <xsl:if test="$NNIECRSche!=''">
              <ns0:IMD>
                <ns0:TAGNAME>IMD</ns0:TAGNAME>
                <ns0:IMD01/>
                <ns0:C272_1>
                  <ns0:C27201>180</ns0:C27201>
                </ns0:C272_1>
                <ns0:C273_1>
                  <ns0:C27304>
                    <xsl:value-of select="$NNIECRSche"/>
                  </ns0:C27304>
                </ns0:C273_1>
              </ns0:IMD>
            </xsl:if>
          </xsl:when>
          <xsl:otherwise>
            <xsl:for-each select="$CNSCData/*[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup'][./*[local-name()='Type']/*[local-name()='Code']/text()='CCP']">
              <xsl:variable name="Name" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='Name']/*[local-name()='Value']"/>
              <xsl:if test="$Name!=''">
                <ns0:IMD>
                  <ns0:TAGNAME>IMD</ns0:TAGNAME>
                  <ns0:IMD01/>
                  <ns0:C272_1>
                    <ns0:C27201>180</ns0:C27201>
                  </ns0:C272_1>
                  <ns0:C273_1>
                    <ns0:C27304>
                      <xsl:value-of select="$Name"/>
                    </ns0:C27304>
                  </ns0:C273_1>
                </ns0:IMD>
              </xsl:if>
            </xsl:for-each>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:if>

      <xsl:if test="boolean($ECCCData)">
        <xsl:variable name="ScientificName" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ScientificName']/*[local-name()='Value']"/>
        <xsl:if test="$ScientificName!=''">
          <ns0:IMD>
            <ns0:TAGNAME>IMD</ns0:TAGNAME>
            <ns0:IMD01/>
            <ns0:C272_1>
              <ns0:C27201>250</ns0:C27201>
            </ns0:C272_1>
            <ns0:C273_1>
              <ns0:C27304>
                <xsl:value-of select="$ScientificName"/>
              </ns0:C27304>
            </ns0:C273_1>
          </ns0:IMD>
        </xsl:if>
        <xsl:variable name="Age" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='Age']/*[local-name()='Value']"/>
        <xsl:if test="$Age!=''">
          <ns0:IMD>
            <ns0:TAGNAME>IMD</ns0:TAGNAME>
            <ns0:IMD01/>
            <ns0:C272_1>
              <ns0:C27201>60</ns0:C27201>
            </ns0:C272_1>
            <ns0:C273_1>
              <ns0:C27304>
                <xsl:value-of select="$Age"/>
              </ns0:C27304>
            </ns0:C273_1>
          </ns0:IMD>
        </xsl:if>
        <xsl:variable name="LifeStage" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='LifeStage']/*[local-name()='Value']"/>
        <xsl:if test="$LifeStage!=''">
          <ns0:IMD>
            <ns0:TAGNAME>IMD</ns0:TAGNAME>
            <ns0:IMD01/>
            <ns0:C272_1>
              <ns0:C27201>212</ns0:C27201>
            </ns0:C272_1>
            <ns0:C273_1>
              <ns0:C27304>
                <xsl:value-of select="$LifeStage"/>
              </ns0:C27304>
            </ns0:C273_1>
          </ns0:IMD>
        </xsl:if>
        <xsl:variable name="VehicleOrMachine">
          <xsl:variable name="ProcessCode" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ProcessCode']/*[local-name()='Value']/text()"/>
          <xsl:choose>
            <xsl:when test="contains('EC01 EC04 XE01 XE04', $ProcessCode)">
              <xsl:value-of select="'Vehicle'"/>
            </xsl:when>
            <xsl:when test="contains('EC02 EC03 XE02 XE03', $ProcessCode)">
              <xsl:value-of select="'Machine'"/>
            </xsl:when>
          </xsl:choose>
        </xsl:variable>
        <xsl:if test="$VehicleOrMachine!=''">
          <xsl:variable name="Make" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='MakeOfMachine']/*[local-name()='Value']"/>
          <xsl:if test="$Make!=''">
            <ns0:IMD>
              <ns0:TAGNAME>IMD</ns0:TAGNAME>
              <ns0:IMD01/>
              <ns0:C272_1>
                <ns0:C27201>223</ns0:C27201>
              </ns0:C272_1>
              <ns0:C273_1>
                <ns0:C27304>
                  <xsl:value-of select="$Make"/>
                </ns0:C27304>
              </ns0:C273_1>
            </ns0:IMD>
          </xsl:if>
          <xsl:variable name="Model" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ModelOfMachine']/*[local-name()='Value']"/>
          <xsl:if test="$Model!='' and *[local-name()='Model']!=$Model">
            <ns0:IMD>
              <ns0:TAGNAME>IMD</ns0:TAGNAME>
              <ns0:IMD01/>
              <ns0:C272_1>
                <ns0:C27201>221</ns0:C27201>
              </ns0:C272_1>
              <ns0:C273_1>
                <ns0:C27304>
                  <xsl:value-of select="$Model"/>
                </ns0:C27304>
              </ns0:C273_1>
            </ns0:IMD>
          </xsl:if>
          <xsl:variable name="ModelYear" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='MachineModelYear']/*[local-name()='Value']"/>
          <xsl:if test="$ModelYear!=''">
            <ns0:IMD>
              <ns0:TAGNAME>IMD</ns0:TAGNAME>
              <ns0:IMD01/>
              <ns0:C272_1>
                <ns0:C27201>228</ns0:C27201>
              </ns0:C272_1>
              <ns0:C273_1>
                <ns0:C27304>
                  <xsl:value-of select="$ModelYear"/>
                </ns0:C27304>
              </ns0:C273_1>
            </ns0:IMD>
          </xsl:if>
        </xsl:if>
        <xsl:variable name="MakeOfEngine" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='MakeOfEngine']/*[local-name()='Value']"/>
        <xsl:if test="$MakeOfEngine!=''">
          <ns0:IMD>
            <ns0:TAGNAME>IMD</ns0:TAGNAME>
            <ns0:IMD01/>
            <ns0:C272_1>
              <ns0:C27201>135</ns0:C27201>
            </ns0:C272_1>
            <ns0:C273_1>
              <ns0:C27301>ENG501</ns0:C27301>
              <ns0:C27304>
                <xsl:value-of select="$MakeOfEngine"/>
              </ns0:C27304>
            </ns0:C273_1>
          </ns0:IMD>
        </xsl:if>
        <xsl:variable name="ModelOfEngine" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ModelOfEngine']/*[local-name()='Value']"/>
        <xsl:if test="$ModelOfEngine!=''">
          <ns0:IMD>
            <ns0:TAGNAME>IMD</ns0:TAGNAME>
            <ns0:IMD01/>
            <ns0:C272_1>
              <ns0:C27201>135</ns0:C27201>
            </ns0:C272_1>
            <ns0:C273_1>
              <ns0:C27301>ENG502</ns0:C27301>
              <ns0:C27304>
                <xsl:value-of select="$ModelOfEngine"/>
              </ns0:C27304>
            </ns0:C273_1>
          </ns0:IMD>
        </xsl:if>
        <xsl:variable name="EngineModelYear" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='EngineModelYear']/*[local-name()='Value']"/>
        <xsl:if test="$EngineModelYear!=''">
          <ns0:IMD>
            <ns0:TAGNAME>IMD</ns0:TAGNAME>
            <ns0:IMD01/>
            <ns0:C272_1>
              <ns0:C27201>135</ns0:C27201>
            </ns0:C272_1>
            <ns0:C273_1>
              <ns0:C27301>ENG503</ns0:C27301>
              <ns0:C27304>
                <xsl:value-of select="$EngineModelYear"/>
              </ns0:C27304>
            </ns0:C273_1>
          </ns0:IMD>
        </xsl:if>
        <xsl:variable name="EngineManufacturer" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='EngineManufacturer']/*[local-name()='Value']"/>
        <xsl:if test="$EngineManufacturer!=''">
          <ns0:IMD>
            <ns0:TAGNAME>IMD</ns0:TAGNAME>
            <ns0:IMD01/>
            <ns0:C272_1>
              <ns0:C27201>135</ns0:C27201>
            </ns0:C272_1>
            <ns0:C273_1>
              <ns0:C27301>ENG500</ns0:C27301>
              <ns0:C27304>
                <xsl:value-of select="$EngineManufacturer"/>
              </ns0:C27304>
            </ns0:C273_1>
          </ns0:IMD>
        </xsl:if>
        <xsl:variable name="EnginePowerRating" select="concat($ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='EnginePowerRating']/*[local-name()='Value']/text(), ' ', $ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='PowerRatingUQ']/*[local-name()='Value']/text())"/>
        <xsl:if test="$EnginePowerRating!=' '">
          <ns0:IMD>
            <ns0:TAGNAME>IMD</ns0:TAGNAME>
            <ns0:IMD01/>
            <ns0:C272_1>
              <ns0:C27201>135</ns0:C27201>
            </ns0:C272_1>
            <ns0:C273_1>
              <ns0:C27301>ENG504</ns0:C27301>
              <ns0:C27304>
                <xsl:value-of select="$EnginePowerRating"/>
              </ns0:C27304>
            </ns0:C273_1>
          </ns0:IMD>
        </xsl:if>
      </xsl:if>

      <xsl:if test ="$IsDFODataValid">
        <ns0:IMD>
        <ns0:TAGNAME>IMD</ns0:TAGNAME>
        <ns0:IMD01/>
        <ns0:C272_1>
          <ns0:C27201>211</ns0:C27201>
          </ns0:C272_1>
          <ns0:C273_1>
            <ns0:C27301>
              <xsl:choose>
                <xsl:when test ="$DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='HasGeneticModification']/*[local-name()='Value']/text()='Y'">
                  <xsl:value-of select="'FO19'"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="'FO20'"/>
                </xsl:otherwise>
              </xsl:choose>
            </ns0:C27301>
          </ns0:C273_1>
        </ns0:IMD>
        <xsl:variable name="GeneticModificationDescription" select="$DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='GeneticModificationDescription']/*[local-name()='Value']"/>
        <xsl:if test="$GeneticModificationDescription!=''">
          <ns0:IMD>
            <ns0:TAGNAME>IMD</ns0:TAGNAME>
            <ns0:IMD01/>
            <ns0:C272_1>
              <ns0:C27201>275</ns0:C27201>
            </ns0:C272_1>
            <ns0:C273_1>
              <ns0:C27304>
                <xsl:value-of select="$GeneticModificationDescription"/>
              </ns0:C27304>
            </ns0:C273_1>
          </ns0:IMD>
        </xsl:if>
        <xsl:variable name="GeneOrNucleotideSequence" select="$DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='GeneOrNucleotideSequence']/*[local-name()='Value']"/>
        <xsl:if test="$GeneOrNucleotideSequence!=''">
          <ns0:IMD>
            <ns0:TAGNAME>IMD</ns0:TAGNAME>
            <ns0:IMD01/>
            <ns0:C272_1>
              <ns0:C27201>217</ns0:C27201>
            </ns0:C272_1>
            <ns0:C273_1>
              <ns0:C27304>
                <xsl:value-of select="$GeneOrNucleotideSequence"/>
              </ns0:C27304>
            </ns0:C273_1>
          </ns0:IMD>
        </xsl:if>
        <ns0:IMD>
          <ns0:TAGNAME>IMD</ns0:TAGNAME>
          <ns0:IMD01/>
          <ns0:C272_1>
            <ns0:C27201>221</ns0:C27201>
          </ns0:C272_1>
          <ns0:C273_1>
            <xsl:choose>
              <xsl:when test ="$DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='CommonNameCode']/*[local-name()='Value']">
                <ns0:C27301>
                  <xsl:value-of select="$DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='CommonNameCode']/*[local-name()='Value']"/>
                </ns0:C27301>
              </xsl:when>
              <xsl:otherwise>
                <ns0:C27304>
                  <xsl:value-of select="*[local-name()='Model']"/>
                </ns0:C27304>
              </xsl:otherwise>
            </xsl:choose>
          </ns0:C273_1>
        </ns0:IMD>
        <ns0:IMD>
          <ns0:TAGNAME>IMD</ns0:TAGNAME>
          <ns0:IMD01/>
          <ns0:C272_1>
            <ns0:C27201>250</ns0:C27201>
          </ns0:C272_1>
          <ns0:C273_1>
            <xsl:choose>
              <xsl:when test ="$DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='SpeciesCode']/*[local-name()='Value']">
                <ns0:C27301>
                  <xsl:value-of select="$DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='SpeciesCode']/*[local-name()='Value']"/>
                </ns0:C27301>
              </xsl:when>
              <xsl:otherwise>
                <ns0:C27304>
                  <xsl:value-of select="$DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='GenusOrSpecies']/*[local-name()='Value']"/>
                </ns0:C27304>
              </xsl:otherwise>
            </xsl:choose>
          </ns0:C273_1>
        </ns0:IMD>
        <xsl:if test="$DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='LifeStagePropagate']/*[local-name()='Value']/text()='Y'">
          <ns0:IMD>
            <ns0:TAGNAME>IMD</ns0:TAGNAME>
            <ns0:IMD01/>
            <ns0:C272_1>
              <ns0:C27201>212</ns0:C27201>
            </ns0:C272_1>
            <ns0:C273_1>
              <ns0:C27301>FO01</ns0:C27301>
            </ns0:C273_1>
          </ns0:IMD>
        </xsl:if>
        <xsl:if test="$DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='LifeStageEmbryo']/*[local-name()='Value']/text()='Y'">
          <ns0:IMD>
            <ns0:TAGNAME>IMD</ns0:TAGNAME>
            <ns0:IMD01/>
            <ns0:C272_1>
              <ns0:C27201>212</ns0:C27201>
            </ns0:C272_1>
            <ns0:C273_1>
              <ns0:C27301>FO02</ns0:C27301>
            </ns0:C273_1>
          </ns0:IMD>
        </xsl:if>
        <xsl:if test="$DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='LifeStageJuvenile']/*[local-name()='Value']/text()='Y'">
          <ns0:IMD>
            <ns0:TAGNAME>IMD</ns0:TAGNAME>
            <ns0:IMD01/>
            <ns0:C272_1>
              <ns0:C27201>212</ns0:C27201>
            </ns0:C272_1>
            <ns0:C273_1>
              <ns0:C27301>FO03</ns0:C27301>
            </ns0:C273_1>
          </ns0:IMD>
        </xsl:if>
        <xsl:if test="$DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='LifeStageAdult']/*[local-name()='Value']/text()='Y'">
          <ns0:IMD>
            <ns0:TAGNAME>IMD</ns0:TAGNAME>
            <ns0:IMD01/>
            <ns0:C272_1>
              <ns0:C27201>212</ns0:C27201>
            </ns0:C272_1>
            <ns0:C273_1>
              <ns0:C27301>FO04</ns0:C27301>
            </ns0:C273_1>
          </ns0:IMD>
        </xsl:if>
        <xsl:if test="$DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='LifeStageLive']/*[local-name()='Value']/text()='Y'">
          <ns0:IMD>
            <ns0:TAGNAME>IMD</ns0:TAGNAME>
            <ns0:IMD01/>
            <ns0:C272_1>
              <ns0:C27201>212</ns0:C27201>
            </ns0:C272_1>
            <ns0:C273_1>
              <ns0:C27301>FO18</ns0:C27301>
            </ns0:C273_1>
          </ns0:IMD>
        </xsl:if>
        <xsl:if test="$DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='LifeStageDead']/*[local-name()='Value']/text()='Y'">
          <ns0:IMD>
            <ns0:TAGNAME>IMD</ns0:TAGNAME>
            <ns0:IMD01/>
            <ns0:C272_1>
              <ns0:C27201>212</ns0:C27201>
            </ns0:C272_1>
            <ns0:C273_1>
              <ns0:C27301>FO10</ns0:C27301>
            </ns0:C273_1>
          </ns0:IMD>
        </xsl:if>
        <xsl:if test="$DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='SexMale']/*[local-name()='Value']/text()='Y'">
          <ns0:IMD>
            <ns0:TAGNAME>IMD</ns0:TAGNAME>
            <ns0:IMD01/>
            <ns0:C272_1>
              <ns0:C27201>209</ns0:C27201>
            </ns0:C272_1>
            <ns0:C273_1>
              <ns0:C27301>FO05</ns0:C27301>
            </ns0:C273_1>
          </ns0:IMD>
        </xsl:if>
        <xsl:if test="$DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='SexFemale']/*[local-name()='Value']/text()='Y'">
          <ns0:IMD>
            <ns0:TAGNAME>IMD</ns0:TAGNAME>
            <ns0:IMD01/>
            <ns0:C272_1>
              <ns0:C27201>209</ns0:C27201>
            </ns0:C272_1>
            <ns0:C273_1>
              <ns0:C27301>FO06</ns0:C27301>
            </ns0:C273_1>
          </ns0:IMD>
        </xsl:if>
        <xsl:if test="$DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='SexOther']/*[local-name()='Value']/text()='Y'">
          <ns0:IMD>
            <ns0:TAGNAME>IMD</ns0:TAGNAME>
            <ns0:IMD01/>
            <ns0:C272_1>
              <ns0:C27201>209</ns0:C27201>
            </ns0:C272_1>
            <ns0:C273_1>
              <ns0:C27301>FO07</ns0:C27301>
            </ns0:C273_1>
          </ns0:IMD>
        </xsl:if>
        <xsl:if test="$DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='SexUnknown']/*[local-name()='Value']/text()='Y'">
          <ns0:IMD>
            <ns0:TAGNAME>IMD</ns0:TAGNAME>
            <ns0:IMD01/>
            <ns0:C272_1>
              <ns0:C27201>209</ns0:C27201>
            </ns0:C272_1>
            <ns0:C273_1>
              <ns0:C27301>FO08</ns0:C27301>
            </ns0:C273_1>
          </ns0:IMD>
        </xsl:if>
        <xsl:if test="$DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='SexSterile']/*[local-name()='Value']/text()='Y'">
          <ns0:IMD>
            <ns0:TAGNAME>IMD</ns0:TAGNAME>
            <ns0:IMD01/>
            <ns0:C272_1>
              <ns0:C27201>209</ns0:C27201>
            </ns0:C272_1>
            <ns0:C273_1>
              <ns0:C27301>FO09</ns0:C27301>
            </ns0:C273_1>
          </ns0:IMD>
        </xsl:if>
        <xsl:if test="$DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='SexHermaphrodite']/*[local-name()='Value']/text()='Y'">
          <ns0:IMD>
            <ns0:TAGNAME>IMD</ns0:TAGNAME>
            <ns0:IMD01/>
            <ns0:C272_1>
              <ns0:C27201>209</ns0:C27201>
            </ns0:C272_1>
            <ns0:C273_1>
              <ns0:C27301>FO15</ns0:C27301>
            </ns0:C273_1>
          </ns0:IMD>
        </xsl:if>
       </xsl:if>

      <xsl:if test="boolean($NRCanData) or boolean($TCData) or boolean($HCData) or boolean($ECCCData)">
        <xsl:if test="*[local-name()='BrandName']!=''">
          <ns0:IMD>
            <ns0:TAGNAME>IMD</ns0:TAGNAME>
            <ns0:IMD01/>
            <ns0:C272_1>
              <ns0:C27201>223</ns0:C27201>
            </ns0:C272_1>
            <ns0:C273_1>
              <ns0:C27304>
                <xsl:value-of select="*[local-name()='BrandName']"/>
              </ns0:C27304>
            </ns0:C273_1>
          </ns0:IMD>
        </xsl:if>
      </xsl:if>

      <xsl:if test ="boolean($TCData) or boolean($ECCCData)">
        <xsl:variable name="LineModelYear" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ModelYear']/*[local-name()='Value']"/>
        <xsl:variable name="TCModelYear" select="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ModelYear']/*[local-name()='Value']"/>
        <xsl:variable name="ECCCModelYear" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='VehicleModelYear']/*[local-name()='Value']"/>
        <xsl:if test="$LineModelYear!=''">
          <ns0:IMD>
            <ns0:TAGNAME>IMD</ns0:TAGNAME>
            <ns0:IMD01/>
            <ns0:C272_1>
              <ns0:C27201>228</ns0:C27201>
            </ns0:C272_1>
            <ns0:C273_1>
              <ns0:C27304>
                <xsl:value-of select="$LineModelYear"/>
              </ns0:C27304>
            </ns0:C273_1>
          </ns0:IMD>
        </xsl:if>
        <xsl:if test="boolean($TCData) and $TCModelYear!='' and (not($LineModelYear) or $TCModelYear!=$LineModelYear)">
          <ns0:IMD>
            <ns0:TAGNAME>IMD</ns0:TAGNAME>
            <ns0:IMD01/>
            <ns0:C272_1>
              <ns0:C27201>228</ns0:C27201>
            </ns0:C272_1>
            <ns0:C273_1>
              <ns0:C27304>
                <xsl:value-of select="$TCModelYear"/>
              </ns0:C27304>
            </ns0:C273_1>
          </ns0:IMD>
        </xsl:if>
        <xsl:if test="boolean($ECCCData) and $ECCCModelYear!='' and (not($TCModelYear) or $ECCCModelYear!=$TCModelYear) and (not($LineModelYear) or $ECCCModelYear!=$LineModelYear)">
          <ns0:IMD>
            <ns0:TAGNAME>IMD</ns0:TAGNAME>
            <ns0:IMD01/>
            <ns0:C272_1>
              <ns0:C27201>228</ns0:C27201>
            </ns0:C272_1>
            <ns0:C273_1>
              <ns0:C27304>
                <xsl:value-of select="$ECCCModelYear"/>
              </ns0:C27304>
            </ns0:C273_1>
          </ns0:IMD>
        </xsl:if>
      </xsl:if>

      <xsl:if test ="boolean($TCData)">
        <xsl:if test ="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='VPRProgramInd']/*[local-name()='Value']/text()='Y'">
          <xsl:variable name="ProductClass" select="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ProductClass']/*[local-name()='Value']"/>
          <xsl:if test="$ProductClass!=''">
            <ns0:IMD>
              <ns0:TAGNAME>IMD</ns0:TAGNAME>
              <ns0:IMD01/>
              <ns0:C272_1>
                <ns0:C27201>202</ns0:C27201>
              </ns0:C272_1>
              <ns0:C273_1>
                <ns0:C27301>
                  <xsl:value-of select="$ProductClass"/>
                </ns0:C27301>
                <ns0:C27304>
                  <xsl:variable name="ProductClassDescription" select="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ProductClassDescription']/*[local-name()='Value']"/>
                  <xsl:choose>
                    <xsl:when test="$ProductClassDescription!=''">
                      <xsl:value-of select="$ProductClassDescription"/>
                    </xsl:when>
                    <xsl:otherwise>Vehicle Class</xsl:otherwise>
                  </xsl:choose>
                </ns0:C27304>
              </ns0:C273_1>
            </ns0:IMD>
          </xsl:if>
        </xsl:if>
        <xsl:variable name="VehicleCondition" select="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='VehicleCondition']/*[local-name()='Value']"/>
        <xsl:if test="$VehicleCondition!=''">
          <ns0:IMD>
            <ns0:TAGNAME>IMD</ns0:TAGNAME>
            <ns0:IMD01/>
            <ns0:C272_1>
              <ns0:C27201>203</ns0:C27201>
            </ns0:C272_1>
            <ns0:C273_1>
              <ns0:C27301>
                <xsl:value-of select="$VehicleCondition"/>
              </ns0:C27301>
              <ns0:C27304>
                <xsl:variable name="VehicleConditionDescription" select="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='VehicleConditionDescription']/*[local-name()='Value']"/>
                <xsl:choose>
                  <xsl:when test="$VehicleConditionDescription!=''">
                    <xsl:value-of select="$VehicleConditionDescription"/>
                  </xsl:when>
                  <xsl:otherwise>Vehicle Condition</xsl:otherwise>
                </xsl:choose>
              </ns0:C27304>
            </ns0:C273_1>
          </ns0:IMD>
        </xsl:if>
        <xsl:variable name="ChassisManufacturerName" select="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ChassisManufacturerName']/*[local-name()='Value']"/>
        <xsl:if test="$ChassisManufacturerName!=''">
          <ns0:IMD>
            <ns0:TAGNAME>IMD</ns0:TAGNAME>
            <ns0:IMD01/>
            <ns0:C272_1>
              <ns0:C27201>140</ns0:C27201>
            </ns0:C272_1>
            <ns0:C273_1>
              <ns0:C27301>14000</ns0:C27301>
              <ns0:C27304>
                <xsl:value-of select="$ChassisManufacturerName"/>
              </ns0:C27304>
            </ns0:C273_1>
          </ns0:IMD>
        </xsl:if>
        <xsl:variable name="ChassisMake" select="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ChassisMake']/*[local-name()='Value']"/>
        <xsl:if test="$ChassisMake!=''">
          <ns0:IMD>
            <ns0:TAGNAME>IMD</ns0:TAGNAME>
            <ns0:IMD01/>
            <ns0:C272_1>
              <ns0:C27201>140</ns0:C27201>
            </ns0:C272_1>
            <ns0:C273_1>
              <ns0:C27301>14001</ns0:C27301>
              <ns0:C27304>
                <xsl:value-of select="$ChassisMake"/>
              </ns0:C27304>
            </ns0:C273_1>
          </ns0:IMD>
        </xsl:if>
        <xsl:variable name="ChassisModel" select="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ChassisModel']/*[local-name()='Value']"/>
        <xsl:if test="$ChassisModel!=''">
          <ns0:IMD>
            <ns0:TAGNAME>IMD</ns0:TAGNAME>
            <ns0:IMD01/>
            <ns0:C272_1>
              <ns0:C27201>140</ns0:C27201>
            </ns0:C272_1>
            <ns0:C273_1>
              <ns0:C27301>14002</ns0:C27301>
              <ns0:C27304>
                <xsl:value-of select="$ChassisModel"/>
              </ns0:C27304>
            </ns0:C273_1>
          </ns0:IMD>
        </xsl:if>
        <xsl:variable name="ChassisYear" select="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ChassisYear']/*[local-name()='Value']"/>
        <xsl:if test="$ChassisYear!=''">
          <ns0:IMD>
            <ns0:TAGNAME>IMD</ns0:TAGNAME>
            <ns0:IMD01/>
            <ns0:C272_1>
              <ns0:C27201>140</ns0:C27201>
            </ns0:C272_1>
            <ns0:C273_1>
              <ns0:C27301>14003</ns0:C27301>
              <ns0:C27304>
                <xsl:value-of select="$ChassisYear"/>
              </ns0:C27304>
            </ns0:C273_1>
          </ns0:IMD>
        </xsl:if>
      </xsl:if>

      <xsl:if test="boolean($ECCCData) or boolean($NRCanData) or boolean($TCData) or boolean($HCData)">
        <xsl:if test="*[local-name()='Model']!=''">
          <ns0:IMD>
            <ns0:TAGNAME>IMD</ns0:TAGNAME>
            <ns0:IMD01/>
            <ns0:C272_1>
              <ns0:C27201>221</ns0:C27201>
            </ns0:C272_1>
            <ns0:C273_1>
              <ns0:C27304>
                <xsl:value-of select="*[local-name()='Model']"/>
              </ns0:C27304>
            </ns0:C273_1>
          </ns0:IMD>
        </xsl:if>
      </xsl:if>

      <xsl:if test="boolean($ECCCData)">
        <xsl:variable name="Model" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ModelOfVehicle']/*[local-name()='Value']"/>
        <xsl:if test="$Model!='' and *[local-name()='Model']!=$Model">
          <ns0:IMD>
            <ns0:TAGNAME>IMD</ns0:TAGNAME>
            <ns0:IMD01/>
            <ns0:C272_1>
              <ns0:C27201>221</ns0:C27201>
            </ns0:C272_1>
            <ns0:C273_1>
              <ns0:C27304>
                <xsl:value-of select="$Model"/>
              </ns0:C27304>
            </ns0:C273_1>
          </ns0:IMD>
        </xsl:if>
        <xsl:variable name="Make" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='MakeOfVehicle']/*[local-name()='Value']"/>
        <xsl:if test="$Make!='' and *[local-name()='BrandName']!=$Make">
          <ns0:IMD>
            <ns0:TAGNAME>IMD</ns0:TAGNAME>
            <ns0:IMD01/>
            <ns0:C272_1>
              <ns0:C27201>223</ns0:C27201>
            </ns0:C272_1>
            <ns0:C273_1>
              <ns0:C27304>
                <xsl:value-of select="$Make"/>
              </ns0:C27304>
            </ns0:C273_1>
          </ns0:IMD>
        </xsl:if>
      </xsl:if>

      <xsl:if test="$IsDFODataValid or boolean($NRCanData) or boolean($HCData)">
        <xsl:variable name="TradeName" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='TradeName']/*[local-name()='Value']"/>
        <xsl:if test="$TradeName!=''">
          <ns0:IMD>
            <ns0:TAGNAME>IMD</ns0:TAGNAME>
            <ns0:IMD01/>
            <ns0:C272_1>
              <ns0:C27201>57</ns0:C27201>
            </ns0:C272_1>
            <ns0:C273_1>
              <ns0:C27304>
                <xsl:value-of select="$TradeName"/>
              </ns0:C27304>
            </ns0:C273_1>
          </ns0:IMD>
        </xsl:if>
      </xsl:if>

      <xsl:if test="boolean($ECCCData)">
        <xsl:variable name="IntendedUseCode" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IntendedUseCode']/*[local-name()='Value']"/>
        <xsl:if test="$IntendedUseCode!=''">
          <ns0:APP_1>
            <ns0:TAGNAME>APP</ns0:TAGNAME>
            <ns0:APP01>2</ns0:APP01>
            <ns0:C973_1>
              <ns0:C97301>ZZZ</ns0:C97301>
              <ns0:C97302>
                <xsl:value-of select="$IntendedUseCode"/>
              </ns0:C97302>
            </ns0:C973_1>
          </ns0:APP_1>
        </xsl:if>
      </xsl:if>

      <xsl:if test="$IsDFODataValid">
        <xsl:if test="$DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IUF']/*[local-name()='Value']/text()='Y'">
          <ns0:APP_1>
            <ns0:TAGNAME>APP</ns0:TAGNAME>
            <ns0:APP01>2</ns0:APP01>
            <ns0:C973_1>
              <ns0:C97301>ZZZ</ns0:C97301>
              <ns0:C97302>FO01</ns0:C97302>
            </ns0:C973_1>
          </ns0:APP_1>
        </xsl:if>
        <xsl:if test="$DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IUA']/*[local-name()='Value']/text()='Y'">
          <ns0:APP_1>
            <ns0:TAGNAME>APP</ns0:TAGNAME>
            <ns0:APP01>2</ns0:APP01>
            <ns0:C973_1>
              <ns0:C97301>ZZZ</ns0:C97301>
              <ns0:C97302>FO11</ns0:C97302>
            </ns0:C973_1>
          </ns0:APP_1>
        </xsl:if>
        <xsl:if test="$DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IURAD']/*[local-name()='Value']/text()='Y'">
          <ns0:APP_1>
            <ns0:TAGNAME>APP</ns0:TAGNAME>
            <ns0:APP01>2</ns0:APP01>
            <ns0:C973_1>
              <ns0:C97301>ZZZ</ns0:C97301>
              <ns0:C97302>FO02</ns0:C97302>
            </ns0:C973_1>
          </ns0:APP_1>
        </xsl:if>
        <xsl:if test="$DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IUSCP']/*[local-name()='Value']/text()='Y'">
          <ns0:APP_1>
            <ns0:TAGNAME>APP</ns0:TAGNAME>
            <ns0:APP01>2</ns0:APP01>
            <ns0:C973_1>
              <ns0:C97301>ZZZ</ns0:C97301>
              <ns0:C97302>FO03</ns0:C97302>
            </ns0:C973_1>
          </ns0:APP_1>
        </xsl:if>
        <xsl:if test="$DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IUEA']/*[local-name()='Value']/text()='Y'">
          <ns0:APP_1>
            <ns0:TAGNAME>APP</ns0:TAGNAME>
            <ns0:APP01>2</ns0:APP01>
            <ns0:C973_1>
              <ns0:C97301>ZZZ</ns0:C97301>
              <ns0:C97302>FO04</ns0:C97302>
            </ns0:C973_1>
          </ns0:APP_1>
        </xsl:if>
        <xsl:if test="$DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IUO']/*[local-name()='Value']/text()='Y'">
          <ns0:APP_1>
            <ns0:TAGNAME>APP</ns0:TAGNAME>
            <ns0:APP01>2</ns0:APP01>
            <ns0:C973_1>
              <ns0:C97301>ZZZ</ns0:C97301>
              <ns0:C97302>FO05</ns0:C97302>
            </ns0:C973_1>
          </ns0:APP_1>
        </xsl:if>
        <xsl:if test="$DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IUE']/*[local-name()='Value']/text()='Y'">
          <ns0:APP_1>
            <ns0:TAGNAME>APP</ns0:TAGNAME>
            <ns0:APP01>2</ns0:APP01>
            <ns0:C973_1>
              <ns0:C97301>ZZZ</ns0:C97301>
              <ns0:C97302>FO06</ns0:C97302>
            </ns0:C973_1>
          </ns0:APP_1>
        </xsl:if>
        <xsl:if test="$DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IUOTH']/*[local-name()='Value']/text()='Y'">
          <ns0:APP_1>
            <ns0:TAGNAME>APP</ns0:TAGNAME>
            <ns0:APP01>2</ns0:APP01>
            <ns0:C973_1>
              <ns0:C97301>ZZZ</ns0:C97301>
              <ns0:C97302>FO07</ns0:C97302>
            </ns0:C973_1>
          </ns0:APP_1>
        </xsl:if>
        <xsl:if test="$DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IUAIS']/*[local-name()='Value']/text()='Y'">
          <ns0:APP_1>
            <ns0:TAGNAME>APP</ns0:TAGNAME>
            <ns0:APP01>2</ns0:APP01>
            <ns0:C973_1>
              <ns0:C97301>ZZZ</ns0:C97301>
              <ns0:C97302>FO09</ns0:C97302>
            </ns0:C973_1>
          </ns0:APP_1>
        </xsl:if>
      </xsl:if>

      <xsl:if test="boolean($NRCanData)">
        <xsl:variable name="IntendedUseCode" select="$NRCanData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IntendedUseCode']/*[local-name()='Value']"/>
        <xsl:if test="$IntendedUseCode!=''">
          <ns0:APP_1>
            <ns0:TAGNAME>APP</ns0:TAGNAME>
            <ns0:APP01>2</ns0:APP01>
            <ns0:C973_1>
              <ns0:C97301>ZZZ</ns0:C97301>
              <ns0:C97302>
                <xsl:value-of select="$IntendedUseCode"/>
              </ns0:C97302>
            </ns0:C973_1>
          </ns0:APP_1>
        </xsl:if>
      </xsl:if>

      <xsl:if test="boolean($PHACData)">
        <xsl:variable name="IntendedUseCode" select="$PHACData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IntendedUseCode']/*[local-name()='Value']"/>
        <xsl:if test="$IntendedUseCode!=''">
          <ns0:APP_1>
            <ns0:TAGNAME>APP</ns0:TAGNAME>
            <ns0:APP01>2</ns0:APP01>
            <ns0:C973_1>
              <ns0:C97301>ZZZ</ns0:C97301>
              <ns0:C97302>
                <xsl:value-of select="$IntendedUseCode"/>
              </ns0:C97302>
            </ns0:C973_1>
          </ns0:APP_1>
        </xsl:if>
      </xsl:if>

      <xsl:if test="boolean($TCData)">
        <xsl:variable name="ImportReasonCode" select="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ImportReasonCode']/*[local-name()='Value']"/>
        <xsl:if test="$ImportReasonCode!=''">
          <ns0:APP_1>
            <ns0:TAGNAME>APP</ns0:TAGNAME>
            <ns0:APP01>2</ns0:APP01>
            <ns0:C973_1>
              <ns0:C97301>ZZZ</ns0:C97301>
              <ns0:C97302>
                <xsl:value-of select="$ImportReasonCode"/>
              </ns0:C97302>
            </ns0:C973_1>
          </ns0:APP_1>
        </xsl:if>
      </xsl:if>

      <xsl:if test="boolean($HCData)">
        
        <xsl:variable name="IntendedUseCodeOld" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IntendedUseCode']"/>
        <xsl:variable name="IntendedUseCodeAPI" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IntendedUseCodeAPI']"/>
        <xsl:variable name="IntendedUseCodeBBC" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IntendedUseCodeBBC']"/>
        <xsl:variable name="IntendedUseCodeCTO" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IntendedUseCodeCTO']"/>
        <xsl:variable name="IntendedUseCodeCPR" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IntendedUseCodeCPR']"/>
        <xsl:variable name="IntendedUseCodeDSE" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IntendedUseCodeDSE']"/>
        <xsl:variable name="IntendedUseCodeHDR" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IntendedUseCodeHDR']"/>
        <xsl:variable name="IntendedUseCodeOCS" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IntendedUseCodeOCS']"/>
        <xsl:variable name="IntendedUseCodeMDE" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IntendedUseCodeMDE']"/>
        <xsl:variable name="IntendedUseCodeNHP" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IntendedUseCodeNHP']"/>
        <xsl:variable name="IntendedUseCodePES" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IntendedUseCodePES']"/>
        <xsl:variable name="IntendedUseCodeVET" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IntendedUseCodeVET']"/>
        <xsl:choose>
          <xsl:when test="boolean($IntendedUseCodeAPI)">
            <xsl:variable name="IntendedUseCodeAPIValue" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IntendedUseCodeAPI']/*[local-name()='Value']"/>
            <ns0:APP_1>
                <ns0:TAGNAME>APP</ns0:TAGNAME>
                <ns0:APP01>2</ns0:APP01>
                <ns0:C973_1>
                  <ns0:C97301>ZZZ</ns0:C97301>
                  <ns0:C97302>
                    <xsl:value-of select="$IntendedUseCodeAPIValue"/>
                  </ns0:C97302>
                </ns0:C973_1>
              </ns0:APP_1>
          </xsl:when>

          <xsl:when test="boolean($IntendedUseCodeBBC)">
            <xsl:variable name="IntendedUseCodeBBCValue" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IntendedUseCodeBBC']/*[local-name()='Value']"/>
              <ns0:APP_1>
                <ns0:TAGNAME>APP</ns0:TAGNAME>
                <ns0:APP01>2</ns0:APP01>
                <ns0:C973_1>
                  <ns0:C97301>ZZZ</ns0:C97301>
                  <ns0:C97302>
                    <xsl:value-of select="$IntendedUseCodeBBCValue"/>
                  </ns0:C97302>
                </ns0:C973_1>
              </ns0:APP_1>
          </xsl:when>

          <xsl:when test="boolean($IntendedUseCodeCTO)">
            <xsl:variable name="IntendedUseCodeCTOValue" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IntendedUseCodeCTO']/*[local-name()='Value']"/>
              <ns0:APP_1>
                <ns0:TAGNAME>APP</ns0:TAGNAME>
                <ns0:APP01>2</ns0:APP01>
                <ns0:C973_1>
                  <ns0:C97301>ZZZ</ns0:C97301>
                  <ns0:C97302>
                    <xsl:value-of select="$IntendedUseCodeCTOValue"/>
                  </ns0:C97302>
                </ns0:C973_1>
              </ns0:APP_1>
          </xsl:when>

          <xsl:when test="boolean($IntendedUseCodeCPR)">
            <xsl:variable name="IntendedUseCodeCPRValue" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IntendedUseCodeCPR']/*[local-name()='Value']"/>
              <ns0:APP_1>
                <ns0:TAGNAME>APP</ns0:TAGNAME>
                <ns0:APP01>2</ns0:APP01>
                <ns0:C973_1>
                  <ns0:C97301>ZZZ</ns0:C97301>
                  <ns0:C97302>
                    <xsl:value-of select="$IntendedUseCodeCPRValue"/>
                  </ns0:C97302>
                </ns0:C973_1>
              </ns0:APP_1>
          </xsl:when>

          <xsl:when test="boolean($IntendedUseCodeDSE)">
            <xsl:variable name="IntendedUseCodeDSEValue" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IntendedUseCodeDSE']/*[local-name()='Value']"/>
              <ns0:APP_1>
                <ns0:TAGNAME>APP</ns0:TAGNAME>
                <ns0:APP01>2</ns0:APP01>
                <ns0:C973_1>
                  <ns0:C97301>ZZZ</ns0:C97301>
                  <ns0:C97302>
                    <xsl:value-of select="$IntendedUseCodeDSEValue"/>
                  </ns0:C97302>
                </ns0:C973_1>
              </ns0:APP_1>
          </xsl:when>

          <xsl:when test="boolean($IntendedUseCodeHDR)">
            <xsl:variable name="IntendedUseCodeHDRValue" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IntendedUseCodeHDR']/*[local-name()='Value']"/>
              <ns0:APP_1>
                <ns0:TAGNAME>APP</ns0:TAGNAME>
                <ns0:APP01>2</ns0:APP01>
                <ns0:C973_1>
                  <ns0:C97301>ZZZ</ns0:C97301>
                  <ns0:C97302>
                    <xsl:value-of select="$IntendedUseCodeHDRValue"/>
                  </ns0:C97302>
                </ns0:C973_1>
              </ns0:APP_1>
          </xsl:when>

          <xsl:when test="boolean($IntendedUseCodeOCS)">
            <xsl:variable name="IntendedUseCodeOCSValue" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IntendedUseCodeOCS']/*[local-name()='Value']"/>
              <ns0:APP_1>
                <ns0:TAGNAME>APP</ns0:TAGNAME>
                <ns0:APP01>2</ns0:APP01>
                <ns0:C973_1>
                  <ns0:C97301>ZZZ</ns0:C97301>
                  <ns0:C97302>
                    <xsl:value-of select="$IntendedUseCodeOCSValue"/>
                  </ns0:C97302>
                </ns0:C973_1>
              </ns0:APP_1>
          </xsl:when>

          <xsl:when test="boolean($IntendedUseCodeMDE)">
            <xsl:variable name="IntendedUseCodeMDEValue" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IntendedUseCodeMDE']/*[local-name()='Value']"/>
              <ns0:APP_1>
                <ns0:TAGNAME>APP</ns0:TAGNAME>
                <ns0:APP01>2</ns0:APP01>
                <ns0:C973_1>
                  <ns0:C97301>ZZZ</ns0:C97301>
                  <ns0:C97302>
                    <xsl:value-of select="$IntendedUseCodeMDEValue"/>
                  </ns0:C97302>
                </ns0:C973_1>
              </ns0:APP_1>
          </xsl:when>

          <xsl:when test="boolean($IntendedUseCodeNHP)">
            <xsl:variable name="IntendedUseCodeNHPValue" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IntendedUseCodeNHP']/*[local-name()='Value']"/>
              <ns0:APP_1>
                <ns0:TAGNAME>APP</ns0:TAGNAME>
                <ns0:APP01>2</ns0:APP01>
                <ns0:C973_1>
                  <ns0:C97301>ZZZ</ns0:C97301>
                  <ns0:C97302>
                    <xsl:value-of select="$IntendedUseCodeNHPValue"/>
                  </ns0:C97302>
                </ns0:C973_1>
              </ns0:APP_1>
          </xsl:when>

          <xsl:when test="boolean($IntendedUseCodePES)">
            <xsl:variable name="IntendedUseCodePESValue" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IntendedUseCodePES']/*[local-name()='Value']"/>
              <ns0:APP_1>
                <ns0:TAGNAME>APP</ns0:TAGNAME>
                <ns0:APP01>2</ns0:APP01>
                <ns0:C973_1>
                  <ns0:C97301>ZZZ</ns0:C97301>
                  <ns0:C97302>
                    <xsl:value-of select="$IntendedUseCodePESValue"/>
                  </ns0:C97302>
                </ns0:C973_1>
              </ns0:APP_1>
          </xsl:when>

          <xsl:when test="boolean($IntendedUseCodeVET)">
            <xsl:variable name="IntendedUseCodeVETValue" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IntendedUseCodeVET']/*[local-name()='Value']"/>
              <ns0:APP_1>
                <ns0:TAGNAME>APP</ns0:TAGNAME>
                <ns0:APP01>2</ns0:APP01>
                <ns0:C973_1>
                  <ns0:C97301>ZZZ</ns0:C97301>
                  <ns0:C97302>
                    <xsl:value-of select="$IntendedUseCodeVETValue"/>
                  </ns0:C97302>
                </ns0:C973_1>
              </ns0:APP_1>
          </xsl:when>

          <xsl:when test="boolean($IntendedUseCodeOld)">
            <xsl:variable name="IntendedUseCodeOldValue" select="$IntendedUseCodeOld/*[local-name()='Value']"/>
            <ns0:APP_1>
              <ns0:TAGNAME>APP</ns0:TAGNAME>
              <ns0:APP01>2</ns0:APP01>
              <ns0:C973_1>
                <ns0:C97301>ZZZ</ns0:C97301>
                <ns0:C97302>
                  <xsl:value-of select="$IntendedUseCodeOldValue"/>
                </ns0:C97302>
              </ns0:C973_1>
            </ns0:APP_1>
          </xsl:when>
        </xsl:choose>
      </xsl:if>

      <xsl:if test="boolean($TCData)">
        <xsl:variable name="ODOReading" select="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ODOReading']/*[local-name()='Value']"/>
        <xsl:if test="$ODOReading!=''">
          <ns0:RFF_6>
            <ns0:TAGNAME>RFF</ns0:TAGNAME>
            <ns0:C506_6>
              <ns0:C50601>BA</ns0:C50601>
              <ns0:C50602>
                <xsl:value-of select="$ODOReading"/>
              </ns0:C50602>
            </ns0:C506_6>
          </ns0:RFF_6>
        </xsl:if>
      </xsl:if>

      <xsl:if test="boolean($HCData)">
        <xsl:variable name="BatchLotNumber" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='BatchLotNumber']/*[local-name()='Value']"/>
        <xsl:if test="$BatchLotNumber!=''">
          <ns0:RFF_6>
            <ns0:TAGNAME>RFF</ns0:TAGNAME>
            <ns0:C506_6>
              <ns0:C50601>BT</ns0:C50601>
              <ns0:C50602>
                <xsl:value-of select="$BatchLotNumber"/>
              </ns0:C50602>
            </ns0:C506_6>
          </ns0:RFF_6>
        </xsl:if>
      </xsl:if>

     <xsl:if test="boolean($TCData)">
       <xsl:variable name="Month" select="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ManufactureMonth']/*[local-name()='Value']"/>
       <xsl:variable name="FormattedMonth" select="format-number($Month, '00')"/>
       <xsl:variable name="Year" select="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ManufactureYear']/*[local-name()='Value']"/>

       <xsl:variable name="FormattedYear">
         <xsl:value-of select="format-number($Year, '0000')"/>
       </xsl:variable>

        <xsl:variable name="Date" select="concat($FormattedYear, $FormattedMonth)"/>
        <xsl:if test="$Date!='' and number($Date)=number($Date)">
          <ns0:DTM_5>
            <ns0:TAGNAME>DTM</ns0:TAGNAME>
            <ns0:C507_5>
              <ns0:C50701>94</ns0:C50701>
              <ns0:C50702>
                <xsl:value-of select="$Date"/>
              </ns0:C50702>
              <ns0:C50703>
                <xsl:value-of select="610"/>
            </ns0:C50703>
            </ns0:C507_5>
          </ns0:DTM_5>
        </xsl:if>
      </xsl:if>

      <xsl:if test="boolean($HCData)">
        <xsl:variable name="ProductionDate" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ProductionDate']/*[local-name()='Value']"/>
        <xsl:if test="$ProductionDate!=''">
          <ns0:DTM_5>
            <ns0:TAGNAME>DTM</ns0:TAGNAME>
            <ns0:C507_5>
              <ns0:C50701>94</ns0:C50701>
              <ns0:C50702>
                <xsl:value-of select="ScriptNS1:FormatXmlDateTime($ProductionDate, 'yyyyMMdd')"/>
              </ns0:C50702>
              <ns0:C50703>102</ns0:C50703>
            </ns0:C507_5>
          </ns0:DTM_5>
        </xsl:if>
        <xsl:variable name="ExpiryDate" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ExpiryDate']/*[local-name()='Value']"/>
        <xsl:if test="$ExpiryDate!=''">
          <ns0:DTM_5>
            <ns0:TAGNAME>DTM</ns0:TAGNAME>
            <ns0:C507_5>
              <ns0:C50701>36</ns0:C50701>
              <ns0:C50702>
                <xsl:value-of select="ScriptNS1:FormatXmlDateTime($ExpiryDate, 'yyyyMMdd')"/>
              </ns0:C50702>
              <ns0:C50703>102</ns0:C50703>
            </ns0:C507_5>
          </ns0:DTM_5>
        </xsl:if>
      </xsl:if>

      <xsl:if test="boolean($ECCCData)">
        <xsl:for-each select="$ECCCData/*[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup'][./*[local-name()='Type']/*[local-name()='Code']/text()='CCP']">
          <xsl:variable name="Name" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='Name']/*[local-name()='Value']"/>
          <xsl:variable name="Type" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='Type']/*[local-name()='Value']"/>
          <xsl:if test="$Name!='' and $Type!=''">
            <ns0:GIR_1>
              <ns0:TAGNAME>GIR</ns0:TAGNAME>
              <ns0:GIR01>1</ns0:GIR01>
              <ns0:C206_1>
                <ns0:C20601>
                  <xsl:value-of select="$Name"/>
                </ns0:C20601>
                <ns0:C20602>
                  <xsl:value-of select="$Type"/>
                </ns0:C20602>
              </ns0:C206_1>
            </ns0:GIR_1>
          </xsl:if>
        </xsl:for-each>
        <xsl:variable name="EngineIDNumber" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='EngineIDNumber']/*[local-name()='Value']"/>
        <xsl:if test="$EngineIDNumber!=''">
          <ns0:GIR_1>
            <ns0:TAGNAME>GIR</ns0:TAGNAME>
            <ns0:GIR01>1</ns0:GIR01>
            <ns0:C206_1>
              <ns0:C20601>
                <xsl:value-of select="$EngineIDNumber"/>
              </ns0:C20601>
              <ns0:C20602>EE</ns0:C20602>
            </ns0:C206_1>
          </ns0:GIR_1>
        </xsl:if>
      </xsl:if>

      <xsl:variable name="AOSConformityValue" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='AOSConformity']/*[local-name()='Value']"/>
      <xsl:if test="$AOSConformityValue='ME04'">
        <xsl:variable name="ImporterOfRecord" select="ancestor::*[local-name()='Shipment']/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='ImporterOfRecord']"/>
        <xsl:variable name="ImporterDocumentaryAddress" select="ancestor::*[local-name()='Shipment']/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='ImporterDocumentaryAddress']"/>
        <ns0:GIR_1>
          <ns0:TAGNAME>GIR</ns0:TAGNAME>
          <ns0:GIR01>1</ns0:GIR01>
          <ns0:C206_1>
            <ns0:C20601>
              <xsl:choose>
                <xsl:when test="boolean($ImporterOfRecord)">
                  <xsl:value-of select="msxsl:node-set($ImporterOfRecord)/*[local-name()='RegistrationNumberCollection']/*[local-name()='RegistrationNumber'][./*[local-name()='Type']/*[local-name()='Code']/text()='ECC']/*[local-name()='Value']"/>
                </xsl:when>
                <xsl:when test="boolean($ImporterDocumentaryAddress)">
                 <xsl:value-of select="msxsl:node-set($ImporterDocumentaryAddress)/*[local-name()='RegistrationNumberCollection']/*[local-name()='RegistrationNumber'][./*[local-name()='Type']/*[local-name()='Code']/text()='ECC']/*[local-name()='Value']"/>
                </xsl:when>
              </xsl:choose>
            </ns0:C20601>
            <ns0:C20602>CW</ns0:C20602>
          </ns0:C206_1>
        </ns0:GIR_1>
      </xsl:if>

      <xsl:if test="boolean($TCData) or boolean($ECCCData)">
        <xsl:variable name="LineVINNumber" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='VINNumber']/*[local-name()='Value']"/>
        <xsl:variable name="TCVINNumber" select="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='VINNumber']/*[local-name()='Value']"/>
        <xsl:variable name="ECCCVIN" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='VIN']/*[local-name()='Value']"/>
        <xsl:if test="$LineVINNumber!=''">
          <ns0:GIR_1>
            <ns0:TAGNAME>GIR</ns0:TAGNAME>
            <ns0:GIR01>1</ns0:GIR01>
            <ns0:C206_1>
              <ns0:C20601>
                <xsl:value-of select="$LineVINNumber"/>
              </ns0:C20601>
              <ns0:C20602>VV</ns0:C20602>
            </ns0:C206_1>
          </ns0:GIR_1>
        </xsl:if>
        <xsl:if test="boolean($TCData) and $TCVINNumber!='' and (not($LineVINNumber) or $TCVINNumber!=$LineVINNumber)">
          <ns0:GIR_1>
            <ns0:TAGNAME>GIR</ns0:TAGNAME>
            <ns0:GIR01>1</ns0:GIR01>
            <ns0:C206_1>
              <ns0:C20601>
                <xsl:value-of select="$TCVINNumber"/>
              </ns0:C20601>
              <ns0:C20602>VV</ns0:C20602>
            </ns0:C206_1>
          </ns0:GIR_1>
        </xsl:if>
        <xsl:if test="boolean($ECCCData) and $ECCCVIN!='' and (not($TCVINNumber) or $ECCCVIN!=$TCVINNumber) and (not($LineVINNumber) or $ECCCVIN!=$LineVINNumber)">
          <ns0:GIR_1>
            <ns0:TAGNAME>GIR</ns0:TAGNAME>
            <ns0:GIR01>1</ns0:GIR01>
            <ns0:C206_1>
              <ns0:C20601>
                <xsl:value-of select="$ECCCVIN"/>
              </ns0:C20601>
              <ns0:C20602>VV</ns0:C20602>
            </ns0:C206_1>
          </ns0:GIR_1>
        </xsl:if>
      </xsl:if>

      <xsl:if test="boolean($CFIAData)">
        <xsl:variable name="AIRSExtensionCode" select="$CFIAData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='AIRSExtensionCode']/*[local-name()='Value']"/>
        <xsl:if test="$AIRSExtensionCode!=''">
          <ns0:GIN_1>
            <ns0:TAGNAME>GIN</ns0:TAGNAME>
            <ns0:GIN01>ZZZ</ns0:GIN01>
            <ns0:C208_1>
              <ns0:C20801>A01</ns0:C20801>
            </ns0:C208_1>
            <ns0:C208_2>
              <ns0:C20801>
                <xsl:value-of select="$AIRSExtensionCode"/>
              </ns0:C20801>
            </ns0:C208_2>
          </ns0:GIN_1>
        </xsl:if>
        <xsl:variable name="AIRSEndUse" select="$CFIAData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='AIRSEndUse']/*[local-name()='Value']"/>
        <xsl:if test="$AIRSEndUse!=''">
          <ns0:GIN_1>
            <ns0:TAGNAME>GIN</ns0:TAGNAME>
            <ns0:GIN01>ZZZ</ns0:GIN01>
            <ns0:C208_1>
              <ns0:C20801>A02</ns0:C20801>
            </ns0:C208_1>
            <ns0:C208_2>
              <ns0:C20801>
                <xsl:value-of select="$AIRSEndUse"/>
              </ns0:C20801>
            </ns0:C208_2>
          </ns0:GIN_1>
        </xsl:if>
        <xsl:variable name="AIRSMiscellaneous" select="$CFIAData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='AIRSMiscellaneous']/*[local-name()='Value']"/>
        <xsl:if test="$AIRSMiscellaneous!=''">
          <ns0:GIN_1>
            <ns0:TAGNAME>GIN</ns0:TAGNAME>
            <ns0:GIN01>ZZZ</ns0:GIN01>
            <ns0:C208_1>
              <ns0:C20801>A03</ns0:C20801>
            </ns0:C208_1>
            <ns0:C208_2>
              <ns0:C20801>
                <xsl:value-of select="$AIRSMiscellaneous"/>
              </ns0:C20801>
            </ns0:C208_2>
          </ns0:GIN_1>
        </xsl:if>
        <xsl:for-each select="$CFIAData/*[local-name()='CustomsReferenceCollection']/*[local-name()='CustomsReference'][*[local-name()='Type']/*[local-name()='Code']='RNA']">
          <xsl:if test="*[local-name()='Reference']!='' and *[local-name()='SubType']/*[local-name()='Code']!=''">
            <ns0:GIN_1>
              <ns0:TAGNAME>GIN</ns0:TAGNAME>
              <ns0:GIN01>ZZZ</ns0:GIN01>
              <ns0:C208_1>
                <ns0:C20801>A04</ns0:C20801>
              </ns0:C208_1>
              <ns0:C208_2>
                <ns0:C20801>
                  <xsl:value-of select="*[local-name()='SubType']/*[local-name()='Code']"/>
                </ns0:C20801>
                <ns0:C20802>
                  <xsl:value-of select="*[local-name()='Reference']"/>
                </ns0:C20802>
              </ns0:C208_2>
            </ns0:GIN_1>
          </xsl:if>
        </xsl:for-each>
      </xsl:if>

      <xsl:if test="(boolean($CNSCData) or boolean($NRCanData) or boolean($PHACData) or boolean($HCData)) and $HazardousMaterialCode != ''">
        <ns0:GIN_1>
          <ns0:TAGNAME>GIN</ns0:TAGNAME>
          <ns0:GIN01>ZZZ</ns0:GIN01>
          <ns0:C208_1>
            <ns0:C20801>UN1</ns0:C20801>
          </ns0:C208_1>
          <ns0:C208_2>
            <ns0:C20801>
              <xsl:value-of select="$HazardousMaterialCode"/>
            </ns0:C20801>
          </ns0:C208_2>
        </ns0:GIN_1>
      </xsl:if>

      <xsl:if test="boolean($ECCCData)">
        <xsl:variable name="CASNumber" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='CASNumber']/*[local-name()='Value']"/>
        <xsl:if test="$CASNumber!=''">
          <ns0:GIN_1>
            <ns0:TAGNAME>GIN</ns0:TAGNAME>
            <ns0:GIN01>ZZZ</ns0:GIN01>
            <ns0:C208_1>
              <ns0:C20801>CAS</ns0:C20801>
            </ns0:C208_1>
            <ns0:C208_2>
              <ns0:C20801>
                <xsl:value-of select="$CASNumber"/>
              </ns0:C20801>
            </ns0:C208_2>
          </ns0:GIN_1>
        </xsl:if>
        <xsl:variable name="TSN" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='TSN']/*[local-name()='Value']"/>
        <xsl:if test="$TSN!=''">
          <ns0:GIN_1>
            <ns0:TAGNAME>GIN</ns0:TAGNAME>
            <ns0:GIN01>ZZZ</ns0:GIN01>
            <ns0:C208_1>
              <ns0:C20801>TSN</ns0:C20801>
            </ns0:C208_1>
            <ns0:C208_2>
              <ns0:C20801>
                <xsl:value-of select="$TSN"/>
              </ns0:C20801>
            </ns0:C208_2>
          </ns0:GIN_1>
        </xsl:if>
        <xsl:variable name="AphiaID" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='AphiaID']/*[local-name()='Value']"/>
        <xsl:if test="$AphiaID!=''">
          <ns0:GIN_1>
            <ns0:TAGNAME>GIN</ns0:TAGNAME>
            <ns0:GIN01>ZZZ</ns0:GIN01>
            <ns0:C208_1>
              <ns0:C20801>APH</ns0:C20801>
            </ns0:C208_1>
            <ns0:C208_2>
              <ns0:C20801>
                <xsl:value-of select="$AphiaID"/>
              </ns0:C20801>
            </ns0:C208_2>
          </ns0:GIN_1>
        </xsl:if>
        <xsl:variable name="EngineFamilyName" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='EngineFamilyName']/*[local-name()='Value']"/>
        <xsl:if test="$EngineFamilyName!=''">
          <ns0:GIN_1>
            <ns0:TAGNAME>GIN</ns0:TAGNAME>
            <ns0:GIN01>ZZZ</ns0:GIN01>
            <ns0:C208_1>
              <ns0:C20801>EFN</ns0:C20801>
            </ns0:C208_1>
            <ns0:C208_2>
              <ns0:C20801>
                <xsl:value-of select="$EngineFamilyName"/>
              </ns0:C20801>
            </ns0:C208_2>
          </ns0:GIN_1>
        </xsl:if>
        <xsl:variable name="TestGroupName" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='TestGroupName']/*[local-name()='Value']"/>
        <xsl:if test="$TestGroupName!=''">
          <ns0:GIN_1>
            <ns0:TAGNAME>GIN</ns0:TAGNAME>
            <ns0:GIN01>ZZZ</ns0:GIN01>
            <ns0:C208_1>
              <ns0:C20801>VTG</ns0:C20801>
            </ns0:C208_1>
            <ns0:C208_2>
              <ns0:C20801>
                <xsl:value-of select="$TestGroupName"/>
              </ns0:C20801>
            </ns0:C208_2>
          </ns0:GIN_1>
        </xsl:if>
        <xsl:variable name="EvaporativeFamily" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='EvaporativeFamily']/*[local-name()='Value']"/>
        <xsl:if test="$EvaporativeFamily!=''">
          <ns0:GIN_1>
            <ns0:TAGNAME>GIN</ns0:TAGNAME>
            <ns0:GIN01>ZZZ</ns0:GIN01>
            <ns0:C208_1>
              <ns0:C20801>EVN</ns0:C20801>
            </ns0:C208_1>
            <ns0:C208_2>
              <ns0:C20801>
                <xsl:value-of select="$EvaporativeFamily"/>
              </ns0:C20801>
            </ns0:C208_2>
          </ns0:GIN_1>
        </xsl:if>
      </xsl:if>

      <xsl:if test="$IsDFODataValid">
        <xsl:variable name="TSN" select="$DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='TSN']/*[local-name()='Value']"/>
        <xsl:if test="$TSN!=''">
          <ns0:GIN_1>
            <ns0:TAGNAME>GIN</ns0:TAGNAME>
            <ns0:GIN01>ZZZ</ns0:GIN01>
            <ns0:C208_1>
              <ns0:C20801>TSN</ns0:C20801>
            </ns0:C208_1>
            <ns0:C208_2>
              <ns0:C20801>
                <xsl:value-of select="$TSN"/>
              </ns0:C20801>
            </ns0:C208_2>
          </ns0:GIN_1>
        </xsl:if>
      </xsl:if>

      <xsl:if test ="boolean($GACData)">
        <xsl:variable name="CommodityCode" select="$GACData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='CommodityCode']/*[local-name()='Value']"/>
        <xsl:if test="$CommodityCode!=''">
          <ns0:GIN_1>
            <ns0:TAGNAME>GIN</ns0:TAGNAME>
            <ns0:GIN01>ZZZ</ns0:GIN01>
            <ns0:C208_1>
              <ns0:C20801>EPA</ns0:C20801>
            </ns0:C208_1>
            <ns0:C208_2>
              <ns0:C20801>
                <xsl:value-of select="$CommodityCode"/>
              </ns0:C20801>
            </ns0:C208_2>
          </ns0:GIN_1>
        </xsl:if>
      </xsl:if>

      <xsl:if test ="boolean($NRCanData)">
        <xsl:variable name="AuthorizedProductID" select="$NRCanData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='AuthorizedProductID']/*[local-name()='Value']"/>
        <xsl:if test="$AuthorizedProductID!=''">
          <ns0:GIN_1>
            <ns0:TAGNAME>GIN</ns0:TAGNAME>
            <ns0:GIN01>ZZZ</ns0:GIN01>
            <ns0:C208_1>
              <ns0:C20801>EXP</ns0:C20801>
            </ns0:C208_1>
            <ns0:C208_2>
              <ns0:C20801>
                <xsl:value-of select="$AuthorizedProductID"/>
              </ns0:C20801>
            </ns0:C208_2>
          </ns0:GIN_1>
        </xsl:if>
      </xsl:if>

      <xsl:if test ="boolean($HCData)">
        <xsl:variable name="GTINNumber" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='GTINNumber']/*[local-name()='Value']"/>
        <xsl:if test="$GTINNumber!=''">
          <ns0:GIN_1>
            <ns0:TAGNAME>GIN</ns0:TAGNAME>
            <ns0:GIN01>ZZZ</ns0:GIN01>
            <ns0:C208_1>
              <ns0:C20801>GS1</ns0:C20801>
            </ns0:C208_1>
            <ns0:C208_2>
              <ns0:C20801>
                <xsl:value-of select="$GTINNumber"/>
              </ns0:C20801>
            </ns0:C208_2>
          </ns0:GIN_1>
        </xsl:if>
        <xsl:variable name="UniqueDeviceIDNumber" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='UniqueDeviceIDNumber']/*[local-name()='Value']"/>
        <xsl:if test="$UniqueDeviceIDNumber!=''">
          <ns0:GIN_1>
            <ns0:TAGNAME>GIN</ns0:TAGNAME>
            <ns0:GIN01>ZZZ</ns0:GIN01>
            <ns0:C208_1>
              <ns0:C20801>FDA</ns0:C20801>
            </ns0:C208_1>
            <ns0:C208_2>
              <ns0:C20801>
                <xsl:value-of select="$UniqueDeviceIDNumber"/>
              </ns0:C20801>
            </ns0:C208_2>
          </ns0:GIN_1>
        </xsl:if>
        <xsl:variable name="CASNumber" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='CASNumber']/*[local-name()='Value']"/>
        <xsl:if test="$CASNumber!=''">
          <ns0:GIN_1>
            <ns0:TAGNAME>GIN</ns0:TAGNAME>
            <ns0:GIN01>ZZZ</ns0:GIN01>
            <ns0:C208_1>
              <ns0:C20801>CAS</ns0:C20801>
            </ns0:C208_1>
            <ns0:C208_2>
              <ns0:C20801>
                <xsl:value-of select="$CASNumber"/>
              </ns0:C20801>
            </ns0:C208_2>
          </ns0:GIN_1>
        </xsl:if>
        <xsl:variable name="FDANumber" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='FDANumber']/*[local-name()='Value']"/>
        <xsl:if test="$FDANumber!=''">
          <ns0:GIN_1>
            <ns0:TAGNAME>GIN</ns0:TAGNAME>
            <ns0:GIN01>ZZZ</ns0:GIN01>
            <ns0:C208_1>
              <ns0:C20801>FDA</ns0:C20801>
            </ns0:C208_1>
            <ns0:C208_2>
              <ns0:C20801>
                <xsl:value-of select="$FDANumber"/>
              </ns0:C20801>
            </ns0:C208_2>
          </ns0:GIN_1>
        </xsl:if>
      </xsl:if>

      <xsl:if test="*[local-name()='Weight']>0 and *[local-name()='WeightUnit']/*[local-name()='Code']!=''">
        <xsl:call-template name="MEASegment">
          <xsl:with-param name="Code" select="'AAB'"/>
          <xsl:with-param name="Unit" select="*[local-name()='WeightUnit']/*[local-name()='Code']"/>
          <xsl:with-param name="Value" select="*[local-name()='Weight']"/>
        </xsl:call-template>
      </xsl:if>

      <xsl:if test="boolean($CNSCData)">
        <xsl:for-each select="$CNSCData/*[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup'][./*[local-name()='Type']/*[local-name()='Code']='CCP']">
          <xsl:call-template name="MEASegment">
            <xsl:with-param name="Code" select="'AEO'"/>
            <xsl:with-param name="Unit" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='UQ']/*[local-name()='Value']"/>
            <xsl:with-param name="Value" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='Qty']/*[local-name()='Value']"/>
          </xsl:call-template>
        </xsl:for-each>
      </xsl:if>

      <xsl:if test ="$IsDFODataValid or boolean($HCData)">
        <xsl:call-template name="MEASegment">
          <xsl:with-param name="Code" select="'BRI'"/>
          <xsl:with-param name="Unit" select="*[local-name()='VolumeUnit']/*[local-name()='Code']"/>
          <xsl:with-param name="Value" select="*[local-name()='Volume']"/>
        </xsl:call-template>
      </xsl:if>

      <!-- Invoid Line NetWeight for Steel in "SG117Loop" -->
      <xsl:variable name="SteelGeneralImportPermit" select="$GACData/*[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='Type']/*[local-name()='Value']='2006'"/>
       <xsl:choose>
        <xsl:when test ="boolean($SteelGeneralImportPermit) and number($CustomsQuantity)>0 and contains('|KGM|TNE|', concat('|', $CustomsQuantityUnit, '|'))">
           <xsl:call-template name="MEASegment">
            <xsl:with-param name="Code" select="'ABS'"/>
            <xsl:with-param name="Unit" select="$CustomsQuantityUnit"/>
            <xsl:with-param name="Value" select="$CustomsQuantity"/>
          </xsl:call-template>
        </xsl:when>
         <xsl:when test="boolean($SteelGeneralImportPermit) and number($NetWeightValue)>0 and $NetWeightUnit!=''">
           <xsl:call-template name="MEASegment">
             <xsl:with-param name="Code" select="'ABS'"/>
             <xsl:with-param name="Unit" select="'KGM'"/>
             <xsl:with-param name="Value" select="ScriptNS3:Convert($NetWeightValue, $NetWeightUnit, 'KG')"/>
           </xsl:call-template>
         </xsl:when>
         <xsl:otherwise>
          <xsl:if test="not(boolean($SteelGeneralImportPermit)) and $isGACWithWeightQuantity">
          <xsl:call-template name="MEASegment">
            <xsl:with-param name="Code" select="'ABS'"/>
            <xsl:with-param name="Unit" select="$CustomsQuantityUnit"/>
            <xsl:with-param name="Value" select="$CustomsQuantity"/>
          </xsl:call-template>
          </xsl:if>
          <xsl:if test="boolean($CNSCData) or boolean($HCData)">
            <xsl:call-template name="MEASegment">
              <xsl:with-param name="Code" select="'ABS'"/>
              <xsl:with-param name="Unit" select="$NetWeightUnit"/>
              <xsl:with-param name="Value" select="$NetWeightValue"/>
            </xsl:call-template>
          </xsl:if>
        </xsl:otherwise>
      </xsl:choose>

      <xsl:if test="boolean($NRCanData)">
        <xsl:call-template name="MEASegment">
          <xsl:with-param name="Code" select="'AAF'"/>
          <xsl:with-param name="Unit" select="'KGM'"/>
          <xsl:with-param name="Value" select="ScriptNS3:Convert($NetWeightValue, $NetWeightUnit, 'KG')"/>
        </xsl:call-template>
        <xsl:call-template name="MEASegment">
          <xsl:with-param name="Code" select="'AAB'"/>
          <xsl:with-param name="Unit" select="'KGM'"/>
          <xsl:with-param name="Value" select="ScriptNS3:Convert(*[local-name()='Weight'], *[local-name()='WeightUnit']/*[local-name()='Code'], 'KG')"/>
        </xsl:call-template>
        <xsl:call-template name="MEASegment">
          <xsl:with-param name="Code" select="'ABS'"/>
          <xsl:with-param name="Unit" select="'CTM'"/>
          <xsl:with-param name="Value" select="$NRCanData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='CaratWeight']/*[local-name()='Value']"/>
        </xsl:call-template>
      </xsl:if>

      <xsl:if test="boolean($HCData) and number(*[local-name()='Weight']) > 0 and number($InvoiceQuantity) > 0">
        <xsl:call-template name="MEASegment">
          <xsl:with-param name="Code" select="'BRG'"/>
          <xsl:with-param name="Unit" select="*[local-name()='WeightUnit']/*[local-name()='Code']"/>
          <xsl:with-param name="Value" select="round(*[local-name()='Weight'] div $InvoiceQuantity * 10000) div 10000"/>
        </xsl:call-template>
      </xsl:if>

      <xsl:variable name="InvoiceLineUnitPrice" select="*[local-name()='UnitPrice']"/>
      <xsl:if test="not(boolean($GACData)) and number($InvoiceLineUnitPrice)>0 and $InvoiceCurrency!=''">
        <ns0:MOA_4>
          <ns0:TAGNAME>MOA</ns0:TAGNAME>
          <ns0:C516_4>
            <ns0:C51601>146</ns0:C51601>
            <ns0:C51602>
              <xsl:value-of select="format-number($InvoiceLineUnitPrice, '0.##')"/>
            </ns0:C51602>
            <ns0:C51603>
              <xsl:value-of select="$InvoiceCurrency"/>
            </ns0:C51603>
          </ns0:C516_4>
        </ns0:MOA_4>
      </xsl:if>

      <xsl:variable name="InvoiceLineCustomsValue" select="*[local-name()='CustomsValue']"/>
      <xsl:if test ="not(boolean($NRCanData)) and not(boolean($GACData)) and $InvoiceLineCustomsValue!='' and not(boolean($IsCasualImportLine))">
        <ns0:MOA_4>
          <ns0:TAGNAME>MOA</ns0:TAGNAME>
          <ns0:C516_4>
            <ns0:C51601>66</ns0:C51601>
            <ns0:C51602>
              <xsl:value-of select="format-number($InvoiceLineCustomsValue, '0.##')"/>
            </ns0:C51602>
            <ns0:C51603>CAD</ns0:C51603>
          </ns0:C516_4>
        </ns0:MOA_4>
      </xsl:if>

      <xsl:variable name="ResetMOA" select="userCSharp:ResetMOACount()"/>
      <xsl:if test ="boolean($GACData)">
        <xsl:variable name="InvoiceLineQTY" select="$InvoiceQuantity"/>
        <xsl:variable name="InvoiceLineCustomsQTY" select="$CustomsQuantity"/>
        <xsl:if test="$InvoiceLineCustomsValue!='' and (($InvoiceLineQTY != '' and number($InvoiceLineQTY) != 0) or ($InvoiceLineCustomsQTY != '' and number($InvoiceLineCustomsQTY) != 0))">
          <xsl:variable name="IncrementMOA" select="userCSharp:IncrementMOACount()"/>
          <ns0:MOA_4>
            <ns0:TAGNAME>MOA</ns0:TAGNAME>
            <ns0:C516_4>
              <ns0:C51601>146</ns0:C51601>
              <ns0:C51602>
                <xsl:choose>
                  <xsl:when test="$InvoiceLineQTY != '' and number($InvoiceLineQTY) != 0">
                    <xsl:value-of select="format-number($InvoiceLineCustomsValue div $InvoiceLineQTY, '0.##')"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="format-number($InvoiceLineCustomsValue div $InvoiceLineCustomsQTY, '0.##')"/>
                  </xsl:otherwise>
                </xsl:choose>
              </ns0:C51602>
              <ns0:C51603>CAD</ns0:C51603>
            </ns0:C516_4>
          </ns0:MOA_4>
        </xsl:if>
        <xsl:if test="*[local-name()='CustomsValue']!='' and not(boolean($IsCasualImportLine))">
          <xsl:variable name="IncrementMOA" select="userCSharp:IncrementMOACount()"/>
          <ns0:MOA_4>
            <ns0:TAGNAME>MOA</ns0:TAGNAME>
            <ns0:C516_4>
              <ns0:C51601>66</ns0:C51601>
              <ns0:C51602>
                <xsl:value-of select="format-number(*[local-name()='CustomsValue'], '0.##')"/>
              </ns0:C51602>
              <ns0:C51603>CAD</ns0:C51603>
            </ns0:C516_4>
          </ns0:MOA_4>
        </xsl:if>
      </xsl:if>

      <xsl:if test="boolean($NRCanData) and 2 > userCSharp:MOACount() and not(boolean($IsCasualImportLine))">
        <xsl:variable name="CustomsValueInUSD" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='CustomsValueInUSD']/*[local-name()='Value']"/>
        <xsl:if test="$CustomsValueInUSD!=''">
          <ns0:MOA_4>
            <ns0:TAGNAME>MOA</ns0:TAGNAME>
            <ns0:C516_4>
              <ns0:C51601>66</ns0:C51601>
              <ns0:C51602>
                <xsl:value-of select="format-number($CustomsValueInUSD, '0.##')"/>
              </ns0:C51602>
              <ns0:C51603>USD</ns0:C51603>
            </ns0:C516_4>
          </ns0:MOA_4>
        </xsl:if>
      </xsl:if>

      <xsl:if test="boolean($ECCCData)">
        <xsl:variable name="SourceOfSpecimen" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='SourceOfSpecimen']/*[local-name()='Value']"/>
        <xsl:if test="$SourceOfSpecimen!=''">
          <ns0:PGI_1>
            <ns0:TAGNAME>PGI</ns0:TAGNAME>
            <ns0:PGI01>11</ns0:PGI01>
            <ns0:C288_1>
              <ns0:C28801>EC03</ns0:C28801>
              <ns0:C28804>
                <xsl:value-of select="$SourceOfSpecimen"/>
              </ns0:C28804>
            </ns0:C288_1>
          </ns0:PGI_1>
        </xsl:if>
        <xsl:variable name="VehicleClass" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='VehicleClass']/*[local-name()='Value']"/>
        <xsl:if test="$VehicleClass!=''">
          <ns0:PGI_1>
            <ns0:TAGNAME>PGI</ns0:TAGNAME>
            <ns0:PGI01>11</ns0:PGI01>
            <ns0:C288_1>
              <ns0:C28801>EC01</ns0:C28801>
              <ns0:C28804>
                <xsl:value-of select="$VehicleClass"/>
              </ns0:C28804>
            </ns0:C288_1>
          </ns0:PGI_1>
        </xsl:if>
        <xsl:variable name="EngineClass" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='EngineClass']/*[local-name()='Value']"/>
        <xsl:if test="$EngineClass!=''">
          <ns0:PGI_1>
            <ns0:TAGNAME>PGI</ns0:TAGNAME>
            <ns0:PGI01>11</ns0:PGI01>
            <ns0:C288_1>
              <ns0:C28801>EC02</ns0:C28801>
              <ns0:C28804>
                <xsl:value-of select="$EngineClass"/>
              </ns0:C28804>
            </ns0:C288_1>
          </ns0:PGI_1>
        </xsl:if>
       <xsl:variable name="AlternativeStandardOfEngineClass" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='AlternativeStandardOfEngineClass']/*[local-name()='Value']"/>
        <xsl:if test="AlternativeStandardOfEngineClass!=''">
          <ns0:PGI_1>
            <ns0:TAGNAME>PGI</ns0:TAGNAME>
            <ns0:PGI01>11</ns0:PGI01>
            <ns0:C288_1>
              <ns0:C28801>EC04</ns0:C28801>
              <ns0:C28804>
                <xsl:value-of select="$AlternativeStandardOfEngineClass"/>
              </ns0:C28804>
            </ns0:C288_1>
          </ns0:PGI_1>
        </xsl:if>
      </xsl:if>

      <xsl:if test ="$IsDFODataValid">
        <xsl:variable name="Category" select="$DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='Category']/*[local-name()='Value']"/>
        <xsl:if test="$Category!=''">
          <ns0:PGI_1>
            <ns0:TAGNAME>PGI</ns0:TAGNAME>
            <ns0:PGI01>11</ns0:PGI01>
            <ns0:C288_1>
              <ns0:C28801>FO01</ns0:C28801>
              <ns0:C28804>
                <xsl:value-of select="$Category"/>
              </ns0:C28804>
            </ns0:C288_1>
          </ns0:PGI_1>
        </xsl:if>
        <xsl:variable name="Commission" select="$DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='Commission']/*[local-name()='Value']"/>
        <xsl:if test="$Commission!=''">
          <ns0:PGI_1>
            <ns0:TAGNAME>PGI</ns0:TAGNAME>
            <ns0:PGI01>11</ns0:PGI01>
            <ns0:C288_1>
              <ns0:C28801>FO02</ns0:C28801>
              <ns0:C28804>
                <xsl:value-of select="$Commission"/>
              </ns0:C28804>
            </ns0:C288_1>
          </ns0:PGI_1>
        </xsl:if>
      </xsl:if>

      <xsl:if test ="boolean($GACData)">
        <xsl:variable name="FTACode" select="$GACData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='FTACode']/*[local-name()='Value']"/>
        <xsl:if test="$FTACode!=''">
          <ns0:PGI_1>
            <ns0:TAGNAME>PGI</ns0:TAGNAME>
            <ns0:PGI01>11</ns0:PGI01>
            <ns0:C288_1>
              <ns0:C28801>FA01</ns0:C28801>
              <ns0:C28804>
                <xsl:value-of select="$FTACode"/>
              </ns0:C28804>
            </ns0:C288_1>
          </ns0:PGI_1>
        </xsl:if>
      </xsl:if>

      <xsl:if test ="boolean($NRCanData) and $NRCanData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='Category']/*[local-name()='Value']/text()='NR01'">
        <ns0:PGI_1>
          <ns0:TAGNAME>PGI</ns0:TAGNAME>
          <ns0:PGI01>11</ns0:PGI01>
          <ns0:C288_1>
            <ns0:C28801>NR01</ns0:C28801>
            <ns0:C28804>NR01</ns0:C28804>
          </ns0:C288_1>
        </ns0:PGI_1>
      </xsl:if>

      <xsl:if test ="boolean($PHACData)">
        <xsl:variable name="Category" select="$PHACData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='Category']/*[local-name()='Value']"/>
        <xsl:if test="$Category!=''">
          <ns0:PGI_1>
            <ns0:TAGNAME>PGI</ns0:TAGNAME>
            <ns0:PGI01>11</ns0:PGI01>
            <ns0:C288_1>
              <ns0:C28801>PH01</ns0:C28801>
              <ns0:C28804>
                <xsl:value-of select="$Category"/>
              </ns0:C28804>
            </ns0:C288_1>
          </ns0:PGI_1>
        </xsl:if>
      </xsl:if>

      <xsl:if test ="boolean($TCData)">
        <xsl:if test="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='TPRProgramInd']/*[local-name()='Value']/text()='Y'">
          <xsl:variable name="ProductClass" select="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ProductClass']/*[local-name()='Value']"/>
          <xsl:if test="$ProductClass!=''">
            <ns0:PGI_1>
              <ns0:TAGNAME>PGI</ns0:TAGNAME>
              <ns0:PGI01>11</ns0:PGI01>
              <ns0:C288_1>
                <ns0:C28801>TC01</ns0:C28801>
                <ns0:C28804>
                  <xsl:value-of select="$ProductClass"/>
                </ns0:C28804>
              </ns0:C288_1>
            </ns0:PGI_1>
          </xsl:if>
          <xsl:variable name="ProductType" select="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ProductType']/*[local-name()='Value']"/>
          <xsl:if test="$ProductType!=''">
            <ns0:PGI_1>
              <ns0:TAGNAME>PGI</ns0:TAGNAME>
              <ns0:PGI01>11</ns0:PGI01>
              <ns0:C288_1>
                <ns0:C28801>TC02</ns0:C28801>
                <ns0:C28804>
                  <xsl:value-of select="$ProductType"/>
                </ns0:C28804>
              </ns0:C288_1>
            </ns0:PGI_1>
          </xsl:if>
          <xsl:variable name="ProductSize" select="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ProductSize']/*[local-name()='Value']"/>
          <xsl:if test="$ProductSize!=''">
            <ns0:PGI_1>
              <ns0:TAGNAME>PGI</ns0:TAGNAME>
              <ns0:PGI01>11</ns0:PGI01>
              <ns0:C288_1>
                <ns0:C28801>TC03</ns0:C28801>
                <ns0:C28804>
                  <xsl:value-of select="$ProductSize"/>
                </ns0:C28804>
              </ns0:C288_1>
            </ns0:PGI_1>
          </xsl:if>
        </xsl:if>
        <xsl:variable name="TitleStatus" select="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='TitleStatus']/*[local-name()='Value']"/>
        <xsl:if test="$TitleStatus!=''">
          <ns0:PGI_1>
            <ns0:TAGNAME>PGI</ns0:TAGNAME>
            <ns0:PGI01>11</ns0:PGI01>
            <ns0:C288_1>
              <ns0:C28801>TC04</ns0:C28801>
              <ns0:C28804>
                <xsl:value-of select="$TitleStatus"/>
              </ns0:C28804>
            </ns0:C288_1>
          </ns0:PGI_1>
        </xsl:if>
        <xsl:variable name="VehicleStatus" select="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='VehicleStatus']/*[local-name()='Value']"/>
        <xsl:if test="$VehicleStatus!=''">
          <ns0:PGI_1>
            <ns0:TAGNAME>PGI</ns0:TAGNAME>
            <ns0:PGI01>11</ns0:PGI01>
            <ns0:C288_1>
              <ns0:C28801>TC05</ns0:C28801>
              <ns0:C28804>
                <xsl:value-of select="$VehicleStatus"/>
              </ns0:C28804>
            </ns0:C288_1>
          </ns0:PGI_1>
        </xsl:if>
      </xsl:if>

      <xsl:if test="boolean($HCData)">
        <xsl:variable name="Category" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='Category']"/>
        <xsl:if test="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='APIProgramInd']/*[local-name()='Value']/text()='Y'">
          <xsl:variable name="CategoryAPI" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='CategoryAPI']"/>
          <xsl:choose>
            <xsl:when test="boolean($CategoryAPI)">
              <xsl:call-template name="HCCategory">
                <xsl:with-param name="Enable" select="$CategoryAPI/*[local-name()='Value']/text()!=''"/>
                <xsl:with-param name="Qualifier" select="'HC01'"/>
                <xsl:with-param name="Code" select="$CategoryAPI/*[local-name()='Value']"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:when test="boolean($Category)">
               <xsl:call-template name="HCCategory">
                 <xsl:with-param name="Enable" select="$Category/*[local-name()='Value']/text()!=''"/>
                 <xsl:with-param name="Qualifier" select="'HC01'"/>
                <xsl:with-param name="Code" select="$Category/*[local-name()='Value']"/>
              </xsl:call-template>
            </xsl:when>
          </xsl:choose>
        </xsl:if>
        <xsl:if test="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='BBCProgramInd']/*[local-name()='Value']/text()='Y'">
          <xsl:variable name="CategoryBBC" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='CategoryBBC']"/>
          <xsl:choose>
            <xsl:when test="boolean($CategoryBBC)">
              <xsl:call-template name="HCCategory">
                <xsl:with-param name="Enable" select="$CategoryBBC/*[local-name()='Value']/text()!=''"/>
                <xsl:with-param name="Qualifier" select="'HC02'"/>
                <xsl:with-param name="Code" select="$CategoryBBC/*[local-name()='Value']"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:when test="boolean($Category)">
              <xsl:call-template name="HCCategory">
                <xsl:with-param name="Enable" select="$Category/*[local-name()='Value']/text()!=''"/>
                <xsl:with-param name="Qualifier" select="'HC02'"/>
                <xsl:with-param name="Code" select="$Category/*[local-name()='Value']"/>
              </xsl:call-template>
            </xsl:when>
          </xsl:choose>
        </xsl:if>
        <xsl:if test="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='CTOProgramInd']/*[local-name()='Value']/text()='Y'">
          <xsl:variable name="CategoryCTO" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='CategoryCTO']"/>
          <xsl:choose>
            <xsl:when test="boolean($CategoryCTO)">
              <xsl:call-template name="HCCategory">
                <xsl:with-param name="Enable" select="$CategoryCTO/*[local-name()='Value']/text()!=''"/>
                <xsl:with-param name="Qualifier" select="'HC03'"/>
                <xsl:with-param name="Code" select="$CategoryCTO/*[local-name()='Value']"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:when test="boolean($Category)">
              <xsl:call-template name="HCCategory">
                <xsl:with-param name="Enable" select="$Category/*[local-name()='Value']/text()!=''"/>
                <xsl:with-param name="Qualifier" select="'HC03'"/>
                <xsl:with-param name="Code" select="$Category/*[local-name()='Value']"/>
              </xsl:call-template>
            </xsl:when>
          </xsl:choose>
        </xsl:if>
        <xsl:if test="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='CPRProgramInd']/*[local-name()='Value']/text()='Y'">
          <xsl:variable name="CategoryCPR" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='CategoryCPR']"/>
          <xsl:choose>
            <xsl:when test="boolean($CategoryCPR)">
              <xsl:call-template name="HCCategory">
                <xsl:with-param name="Enable" select="$CategoryCPR/*[local-name()='Value']/text()!=''"/>
                <xsl:with-param name="Qualifier" select="'HC11'"/>
                <xsl:with-param name="Code" select="$CategoryCPR/*[local-name()='Value']"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:when test="boolean($Category)">
              <xsl:call-template name="HCCategory">
                <xsl:with-param name="Enable" select="$Category/*[local-name()='Value']/text()!=''"/>
                <xsl:with-param name="Qualifier" select="'HC11'"/>
                <xsl:with-param name="Code" select="$Category/*[local-name()='Value']"/>
              </xsl:call-template>
            </xsl:when>
          </xsl:choose>
        </xsl:if>
        <xsl:if test="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='DSEProgramInd']/*[local-name()='Value']/text()='Y'">
          <xsl:variable name="CategoryDSE" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='CategoryDSE']"/>
          <xsl:choose>
            <xsl:when test="boolean($CategoryDSE)">
              <xsl:call-template name="HCCategory">
                <xsl:with-param name="Enable" select="$CategoryDSE/*[local-name()='Value']/text()!=''"/>
                <xsl:with-param name="Qualifier" select="'HC04'"/>
                <xsl:with-param name="Code" select="$CategoryDSE/*[local-name()='Value']"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:when test="boolean($Category)">
              <xsl:call-template name="HCCategory">
                <xsl:with-param name="Enable" select="$Category/*[local-name()='Value']/text()!=''"/>
                <xsl:with-param name="Qualifier" select="'HC04'"/>
                <xsl:with-param name="Code" select="$Category/*[local-name()='Value']"/>
              </xsl:call-template>
            </xsl:when>
          </xsl:choose>
        </xsl:if>
        <xsl:if test="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='HDRProgramInd']/*[local-name()='Value']/text()='Y'">
          <xsl:variable name="CategoryHDR" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='CategoryHDR']"/>
          <xsl:choose>
            <xsl:when test="boolean($CategoryHDR)">
              <xsl:call-template name="HCCategory">
                <xsl:with-param name="Enable" select="$CategoryHDR/*[local-name()='Value']/text()!=''"/>
                <xsl:with-param name="Qualifier" select="'HC05'"/>
                <xsl:with-param name="Code" select="$CategoryHDR/*[local-name()='Value']"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:when test="boolean($Category)">
              <xsl:call-template name="HCCategory">
                <xsl:with-param name="Enable" select="$Category/*[local-name()='Value']/text()!=''"/>
                <xsl:with-param name="Qualifier" select="'HC05'"/>
                <xsl:with-param name="Code" select="$Category/*[local-name()='Value']"/>
              </xsl:call-template>
            </xsl:when>
          </xsl:choose>
        </xsl:if>
        <xsl:if test="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='OCSProgramInd']/*[local-name()='Value']/text()='Y'">
          <xsl:variable name="CategoryOCS" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='CategoryOCS']"/>
          <xsl:choose>
            <xsl:when test="boolean($CategoryOCS)">
              <xsl:call-template name="HCCategory">
                <xsl:with-param name="Enable" select="$CategoryOCS/*[local-name()='Value']/text()!=''"/>
                <xsl:with-param name="Qualifier" select="'HC09'"/>
                <xsl:with-param name="Code" select="$CategoryOCS/*[local-name()='Value']"/>
              </xsl:call-template>
            </xsl:when>
           <xsl:when test="boolean($Category)">
              <xsl:call-template name="HCCategory">
                <xsl:with-param name="Enable" select="$Category/*[local-name()='Value']/text()!=''"/>
                <xsl:with-param name="Qualifier" select="'HC09'"/>
                <xsl:with-param name="Code" select="$Category/*[local-name()='Value']"/>
              </xsl:call-template>
            </xsl:when>
          </xsl:choose>
        </xsl:if>
        <xsl:if test="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='MDEProgramInd']/*[local-name()='Value']/text()='Y'">
          <xsl:variable name="CategoryMDE" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='CategoryMDE']"/>
          <xsl:choose>
            <xsl:when test="boolean($CategoryMDE)">
              <xsl:call-template name="HCCategory">
                <xsl:with-param name="Enable" select="$CategoryMDE/*[local-name()='Value']/text()!=''"/>
                <xsl:with-param name="Qualifier" select="'HC06'"/>
                <xsl:with-param name="Code" select="$CategoryMDE/*[local-name()='Value']"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:when test="boolean($Category)">
              <xsl:call-template name="HCCategory">
                <xsl:with-param name="Enable" select="$Category/*[local-name()='Value']/text()!=''"/>
                <xsl:with-param name="Qualifier" select="'HC06'"/>
                <xsl:with-param name="Code" select="$Category/*[local-name()='Value']"/>
              </xsl:call-template>
            </xsl:when>
          </xsl:choose>
        </xsl:if>
        <xsl:if test="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='NHPProgramInd']/*[local-name()='Value']/text()='Y'">
          <xsl:variable name="CategoryNHP" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='CategoryNHP']"/>
          <xsl:choose>
            <xsl:when test="boolean($CategoryNHP)">
              <xsl:call-template name="HCCategory">
                <xsl:with-param name="Enable" select="$CategoryNHP/*[local-name()='Value']/text()!=''"/>
                <xsl:with-param name="Qualifier" select="'HC07'"/>
                <xsl:with-param name="Code" select="$CategoryNHP/*[local-name()='Value']"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:when test="boolean($Category)">
              <xsl:call-template name="HCCategory">
                <xsl:with-param name="Enable" select="$Category/*[local-name()='Value']/text()!=''"/>
                <xsl:with-param name="Qualifier" select="'HC07'"/>
                <xsl:with-param name="Code" select="$Category/*[local-name()='Value']"/>
              </xsl:call-template>
            </xsl:when>
          </xsl:choose>
        </xsl:if>
        <xsl:if test="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='PESProgramInd']/*[local-name()='Value']/text()='Y'">
          <xsl:variable name="CategoryPES" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='CategoryPES']"/>
          <xsl:choose>
            <xsl:when test="boolean($CategoryPES)">
              <xsl:call-template name="HCCategory">
                <xsl:with-param name="Enable" select="$CategoryPES/*[local-name()='Value']/text()!=''"/>
                <xsl:with-param name="Qualifier" select="'HC13'"/>
                <xsl:with-param name="Code" select="$CategoryPES/*[local-name()='Value']"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:when test="boolean($Category)">
              <xsl:call-template name="HCCategory">
                <xsl:with-param name="Enable" select="$Category/*[local-name()='Value']/text()!=''"/>
                <xsl:with-param name="Qualifier" select="'HC13'"/>
                <xsl:with-param name="Code" select="$Category/*[local-name()='Value']"/>
              </xsl:call-template>
            </xsl:when>
          </xsl:choose>
        </xsl:if>
        <xsl:if test="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='REDProgramInd']/*[local-name()='Value']/text()='Y'">
          <xsl:variable name="CategoryRED" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='CategoryRED']"/>
          <xsl:choose>
            <xsl:when test="boolean($CategoryRED)">
              <xsl:call-template name="HCCategory">
                <xsl:with-param name="Enable" select="$CategoryRED/*[local-name()='Value']/text()!=''"/>
                <xsl:with-param name="Qualifier" select="'HC12'"/>
                <xsl:with-param name="Code" select="$CategoryRED/*[local-name()='Value']"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:when test="boolean($Category)">
              <xsl:call-template name="HCCategory">
                <xsl:with-param name="Enable" select="$Category/*[local-name()='Value']/text()!=''"/>
                <xsl:with-param name="Qualifier" select="'HC12'"/>
                <xsl:with-param name="Code" select="$Category/*[local-name()='Value']"/>
              </xsl:call-template>
            </xsl:when>
          </xsl:choose>
        </xsl:if>
        <xsl:if test="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='VETProgramInd']/*[local-name()='Value']/text()='Y'">
          <xsl:variable name="CategoryVET" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='CategoryVET']"/>
          <xsl:choose>
            <xsl:when test="boolean($CategoryVET)">
              <xsl:call-template name="HCCategory">
                <xsl:with-param name="Enable" select="$CategoryVET/*[local-name()='Value']/text()!=''"/>
                <xsl:with-param name="Qualifier" select="'HC08'"/>
                <xsl:with-param name="Code" select="$CategoryVET/*[local-name()='Value']"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:when test="boolean($Category)">
              <xsl:call-template name="HCCategory">
                <xsl:with-param name="Enable" select="$Category/*[local-name()='Value']/text()!=''"/>
                <xsl:with-param name="Qualifier" select="'HC08'"/>
                <xsl:with-param name="Code" select="$Category/*[local-name()='Value']"/>
              </xsl:call-template>
            </xsl:when>
          </xsl:choose>
        </xsl:if>
      </xsl:if>

      <xsl:if test="not(boolean($IsCasualImportLine))">
      <ns0:TCC>
        <ns0:TAGNAME>TCC</ns0:TAGNAME>
        <ns0:C200_1/>
        <ns0:C203_1>
          <ns0:C20301/>
        </ns0:C203_1>
        <ns0:C528_1>
          <ns0:C52801>
            <xsl:value-of select="$HarmonisedCode"/>
          </ns0:C52801>
          <ns0:C52802>HS</ns0:C52802>
        </ns0:C528_1>
      </ns0:TCC>
     </xsl:if>

      <xsl:choose>
        <xsl:when test="$isGACWithCountQuantity">
          <ns0:CNT_1>
            <ns0:TAGNAME>CNT</ns0:TAGNAME>
            <ns0:C270_1>
              <ns0:C27001>51</ns0:C27001>
              <ns0:C27002>
                <xsl:value-of select="number(round($CustomsQuantity))"/>
              </ns0:C27002>
              <ns0:C27003>
                <xsl:value-of select="$CustomsQuantityUnit"/>
              </ns0:C27003>
            </ns0:C270_1>
          </ns0:CNT_1>
        </xsl:when>
        <xsl:otherwise>
          <xsl:if test="not(boolean($IsCasualImportLine)) and number($InvoiceQuantity)>0 and $InvoiceQuantityUnit!=''">
            <ns0:CNT_1>
              <ns0:TAGNAME>CNT</ns0:TAGNAME>
              <ns0:C270_1>
                <ns0:C27001>51</ns0:C27001>
                <ns0:C27002>
                  <xsl:value-of select="number(round($InvoiceQuantity))"/>
                </ns0:C27002>
                <ns0:C27003>
                  <xsl:value-of select="$InvoiceQuantityUnit"/>
                </ns0:C27003>
              </ns0:C270_1>
            </ns0:CNT_1>
          </xsl:if>
 
        </xsl:otherwise>
      </xsl:choose>

      <xsl:if test="boolean($CNSCData)">
        <xsl:variable name="UnitQty" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='UnitQty']/*[local-name()='Value']"/>
        <xsl:if test="$UnitQty!='' and number(substring($UnitQty,1,9))>0">
          <ns0:CNT_1>
            <ns0:TAGNAME>CNT</ns0:TAGNAME>
            <ns0:C270_1>
              <ns0:C27001>51</ns0:C27001>
              <ns0:C27002>
                <xsl:value-of select="number(substring($UnitQty,1,9))"/>
              </ns0:C27002>
              <ns0:C27003>EA</ns0:C27003>
            </ns0:C270_1>
          </ns0:CNT_1>
        </xsl:if>
      </xsl:if>

      <xsl:if test ="$IsDFODataValid">
        <xsl:variable name="Count" select="$DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='Count']/*[local-name()='Value']"/>
        <xsl:if test="$Count!='' and number(substring($Count,1,9))>0">
          <ns0:CNT_1>
            <ns0:TAGNAME>CNT</ns0:TAGNAME>
            <ns0:C270_1>
              <ns0:C27001>51</ns0:C27001>
              <ns0:C27002>
                <xsl:value-of select="number(substring($Count,1,9))"/>
              </ns0:C27002>
              <ns0:C27003>EA</ns0:C27003>
            </ns0:C270_1>
          </ns0:CNT_1>
        </xsl:if>
      </xsl:if>

      <xsl:if test="boolean($CFIAData) or boolean($ECCCData)">
        <xsl:variable name="RN_NKSource" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='RN_NKSource']/*[local-name()='Value']"/>
        <xsl:if test="$RN_NKSource!=''">
          <ns0:SG118Loop>
            <ns0:LOC_5>
              <ns0:TAGNAME>LOC</ns0:TAGNAME>
              <ns0:LOC01>30</ns0:LOC01>
              <ns0:C517_5>
                <ns0:C51701>
                  <xsl:value-of select="$RN_NKSource"/>
                </ns0:C51701>
              </ns0:C517_5>
              <xsl:variable name="StateOfSource" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='StateOfSource']/*[local-name()='Value']"/>
              <xsl:if test="$StateOfSource != ''">
                <ns0:C519_4>
                  <ns0:C51901>
                    <xsl:value-of select="$StateOfSource"/>
                  </ns0:C51901>
                </ns0:C519_4>
              </xsl:if>
            </ns0:LOC_5>
            <ns0:SEQ_4>
              <ns0:TAGNAME>SEQ</ns0:TAGNAME>
              <ns0:SEQ01>1</ns0:SEQ01>
            </ns0:SEQ_4>
          </ns0:SG118Loop>
        </xsl:if>
      </xsl:if>

      <xsl:if test ="*[local-name()='CountryOfOrigin']/*[local-name()='Code']!=''">
        <xsl:variable name="CountryOfOrigin" select="*[local-name()='CountryOfOrigin']/*[local-name()='Code']"/>
        <xsl:if test="$CountryOfOrigin!='' or $CountryOfOriginOnInvoiceLine!=''">
          <ns0:SG118Loop>
            <ns0:LOC_5>
              <ns0:TAGNAME>LOC</ns0:TAGNAME>
              <ns0:LOC01>27</ns0:LOC01>
              <ns0:C517_5>
                <ns0:C51701>
                  <xsl:choose>
                    <xsl:when test="$CountryOfOriginOnInvoiceLine!=''">
                       <xsl:value-of select="$CountryOfOriginOnInvoiceLine"/>
                    </xsl:when>
                    <xsl:otherwise>
                       <xsl:value-of select="$CountryOfOrigin"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </ns0:C51701>
              </ns0:C517_5>
              <xsl:variable name="StateOfOrigin" select="*[local-name()='StateOfOrigin']/*[local-name()='Code']"/>
              <xsl:variable name="IIDRegion" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IIDRegion']/*[local-name()='Value']/text()"/>
              <xsl:if test="$StateOfOrigin != '' or $IIDRegion != ''">
                <ns0:C519_4>
                  <xsl:if test="$StateOfOrigin != ''">
                    <ns0:C51901>
                      <xsl:value-of select="$StateOfOrigin"/>
                    </ns0:C51901>
                  </xsl:if>
                  <xsl:if test="$IIDRegion != ''">
                    <ns0:C51904>
                      <xsl:value-of select="$IIDRegion"/>
                    </ns0:C51904>
                  </xsl:if>
                </ns0:C519_4>
              </xsl:if>
            </ns0:LOC_5>
            <ns0:SEQ_4>
              <ns0:TAGNAME>SEQ</ns0:TAGNAME>
              <ns0:SEQ01>1</ns0:SEQ01>
            </ns0:SEQ_4>
          </ns0:SG118Loop>
        </xsl:if>
      </xsl:if>

      <xsl:if test="boolean($CFIAData) or boolean($CNSCData)">
        <xsl:variable name="ConsigneeAddress" select="*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='ConsigneeAddress']"/>
        <xsl:if test="$ConsigneeAddress!=''">
          <ns0:SG119Loop>
            <xsl:call-template name="NADSegment">
              <xsl:with-param name="OrgAddress" select="$ConsigneeAddress"/>
              <xsl:with-param name="Type" select="'DP'"/>
              <xsl:with-param name="UseDefaultPartyID" select="false()"/>
              <xsl:with-param name="ContactLoopID" select="'120'"/>
            </xsl:call-template>
          </ns0:SG119Loop>
        </xsl:if>
      </xsl:if>

      <xsl:variable name="ManufacturerOrSupplier">
        <xsl:variable name="Manufacturer" select="*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='Manufacturer']"/>
        <xsl:choose>
          <xsl:when test="boolean($ECCCData) and $ECCCData/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='ECCCMachineManufacturer']!=''">
            <xsl:copy-of select="$ECCCData/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='ECCCMachineManufacturer']/*"/>
          </xsl:when>
          <xsl:when test="$Manufacturer">
            <xsl:copy-of select="$Manufacturer/*"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:variable name="TCDataHasNoTPRorVPR">
              <xsl:choose>
                <xsl:when test="boolean($TCData) = false()">
                  <xsl:value-of select="false()"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:variable name="IsTPRProgramInd" select="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='TPRProgramInd']/*[local-name()='Value']='Y'" />
                  <xsl:variable name="IsVPRProgramInd" select="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='VPRProgramInd']/*[local-name()='Value']='Y'" />
                  <xsl:value-of select="not($IsTPRProgramInd) and not($IsVPRProgramInd)"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <xsl:if test="(boolean($TCData) = false()) or ($TCDataHasNoTPRorVPR = 'true')">
              <xsl:copy-of select="../../*[local-name()='Supplier']/*"/>
            </xsl:if>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:if test="$ManufacturerOrSupplier != ''">
        <ns0:SG119Loop>
          <xsl:variable name="IsSubProgramPIL" select="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='SubProgram']/*[local-name()='Value']='PIL'"/>
          <xsl:call-template name="NADSegment">
            <xsl:with-param name="OrgAddress" select="msxsl:node-set($ManufacturerOrSupplier)"/>
            <xsl:with-param name="Type" select="'MF'"/>
            <xsl:with-param name="PartyID">
              <xsl:choose>
                <xsl:when test="boolean($IsSubProgramPIL)">
                  <xsl:value-of select="msxsl:node-set($ManufacturerOrSupplier)/*[local-name()='RegistrationNumberCollection']/*[local-name()='RegistrationNumber'][./*[local-name()='Type']/*[local-name()='Code']/text()='WMI']/*[local-name()='Value']"/>
                </xsl:when>
              </xsl:choose>
            </xsl:with-param>
            <xsl:with-param name="UseDefaultPartyID">
              <xsl:value-of select="false()"/>
            </xsl:with-param>
            <xsl:with-param name="ContactLoopID" select="'120'"/>
          </xsl:call-template>
        </ns0:SG119Loop>
      </xsl:if>

      <xsl:if test="boolean($ECCCData)">
        <xsl:if test="*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='ConsigneeAddress']">
            <ns0:SG119Loop>
            <xsl:call-template name="NADSegment">
              <xsl:with-param name="OrgAddress" select="*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='ConsigneeAddress']"/>
              <xsl:with-param name="Type" select="'UC'"/>
              <xsl:with-param name="UseDefaultPartyID" select="false()"/>
              <xsl:with-param name="ContactLoopID" select="'120'"/>
            </xsl:call-template>
          </ns0:SG119Loop>
        </xsl:if>
        <xsl:variable name="ECCCEngineLocation" select ="$ECCCData/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='ECCCEngineLocation']"/>
          <xsl:if test = "$ECCCEngineLocation!=''">
            <ns0:SG119Loop>
                <xsl:call-template name="NADSegment">
                    <xsl:with-param name="Type" select="'AT'"/>
                    <xsl:with-param name="OrgAddress" select="$ECCCEngineLocation"/>
                    <xsl:with-param name="UseDefaultPartyID" select="false()"/>
                    <xsl:with-param name="ContactLoopID" select="'120'"/>
                </xsl:call-template>
            </ns0:SG119Loop>
        </xsl:if>
        <xsl:variable name ="ECCCEvidenceOfConfirmityLocation" select ="$ECCCData/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='ECCCEvidenceOfConfirmityLocation']"/>
        <xsl:if test ="$ECCCEvidenceOfConfirmityLocation!=''">
            <ns0:SG119Loop>
                <xsl:call-template name="NADSegment">
                    <xsl:with-param name="Type" select="'AT'"/>
                    <xsl:with-param name="OrgAddress" select="$ECCCEvidenceOfConfirmityLocation"/>
                    <xsl:with-param name="UseDefaultPartyID" select="false()"/>
                    <xsl:with-param name="ContactLoopID" select="'120'"/>
                </xsl:call-template>
            </ns0:SG119Loop>
        </xsl:if>
      </xsl:if>

      <xsl:if test="$IsDFODataValid">
        <xsl:if test="$DFOData/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='HarvestingParty']">
          <ns0:SG119Loop>
            <xsl:call-template name="NADSegment">
              <xsl:with-param name="OrgAddress" select="$DFOData/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='HarvestingParty']"/>
              <xsl:with-param name="Type" select="'DFI'"/>
              <xsl:with-param name="UseDefaultPartyID" select="true()"/>
              <xsl:with-param name="ContactLoopID" select="'120'"/>
            </xsl:call-template>
          </ns0:SG119Loop>
        </xsl:if>
      </xsl:if>

      <xsl:if test="boolean($NRCanData)">
        <xsl:variable name="AuthorizedPartyAddress">
          <xsl:variable name="ShipmentOrgAddressCollection" select="ancestor::*[local-name()='Shipment']/*[local-name()='OrganizationAddressCollection']"/>
          <xsl:variable name="CommercialInvoiceOrgAddressCollection" select="ancestor::*[local-name()='CommercialInvoice']/*[local-name()='OrganizationAddressCollection']"/>
          <xsl:variable name="Manufacturer" select="ancestor::*[local-name()='CommercialInvoiceLine']/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='Manufacturer']"/>
          <xsl:variable name="AuthorizedParty" select="$NRCanData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='AuthorizedParty']/*[local-name()='Value']/text()"/>
          <xsl:choose>
            <xsl:when test="$AuthorizedParty='IMP'">
              <xsl:copy-of select="$ShipmentOrgAddressCollection/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='ImporterDocumentaryAddress']"/>
            </xsl:when>
            <xsl:when test="$AuthorizedParty='SUP'">
              <xsl:copy-of select="$ShipmentOrgAddressCollection/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='SupplierPickupDeliveryAddress']"/>
            </xsl:when>
            <xsl:when test="$AuthorizedParty='IOR'">
              <xsl:copy-of select="$ShipmentOrgAddressCollection/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='ImporterOfRecord']"/>
            </xsl:when>
            <xsl:when test="*[local-name()='Value']/text()='MAN' and boolean($Manufacturer)">
              <xsl:copy-of select="$Manufacturer"/>
            </xsl:when>
            <xsl:when test="*[local-name()='Value']/text()='MAN' and not(boolean($Manufacturer))">
              <xsl:copy-of select="$CommercialInvoiceOrgAddressCollection/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='Manufacturer']"/>
            </xsl:when>
          </xsl:choose>
        </xsl:variable>
        <xsl:if test="msxsl:node-set($AuthorizedPartyAddress)/*[local-name()='OrganizationAddress']">
          <ns0:SG119Loop>
            <xsl:call-template name="NADSegment">
              <xsl:with-param name="OrgAddress" select="msxsl:node-set($AuthorizedPartyAddress)/*[local-name()='OrganizationAddress']"/>
              <xsl:with-param name="Type" select="'WW'"/>
              <xsl:with-param name="UseDefaultPartyID" select="true()"/>
              <xsl:with-param name="ContactLoopID" select="'120'"/>
            </xsl:call-template>
          </ns0:SG119Loop>
        </xsl:if>
      </xsl:if>

      <xsl:if test="boolean($TCData)">
        <xsl:variable name="Manufacturer" select="*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='Manufacturer']"/>
        <xsl:if test="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='AssemblerName']/*[local-name()='Value']!=''">
          <ns0:SG119Loop>
            <ns0:NAD>
              <ns0:TAGNAME>NAD</ns0:TAGNAME>
              <ns0:NAD01>DFT</ns0:NAD01>
              <ns0:C082>
                <ns0:C08201/>
              </ns0:C082>
              <ns0:C058>
                <ns0:C05801/>
              </ns0:C058>
              <ns0:C080>
                <ns0:C08001>
                  <xsl:value-of select="normalize-space($TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='AssemblerName']/*[local-name()='Value'])"/>
                </ns0:C08001>
              </ns0:C080>
            </ns0:NAD>
          </ns0:SG119Loop>
        </xsl:if>
      </xsl:if>

      <xsl:if test="boolean($CFIAData)">
        <xsl:variable name="Resetsg121LoopCount" select="userCSharp:ResetSG121Count()"/>
        <xsl:for-each select="$CFIAData/*[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup'][./*[local-name()='Type']/*[local-name()='Code']/text()='CLP']">
          <xsl:call-template name="SG121Loop"/>
        </xsl:for-each>
      </xsl:if>

      <xsl:if test="boolean($CNSCData)">
        <xsl:variable name="Resetsg121LoopCount" select="userCSharp:ResetSG121Count()"/>
        <xsl:for-each select="$CNSCData/*[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup'][./*[local-name()='Type']/*[local-name()='Code']/text()='CLP']">
          <xsl:call-template name="SG121Loop"/>
        </xsl:for-each>
      </xsl:if>

      <xsl:if test="boolean($ECCCData)">
        <xsl:variable name="Resetsg121LoopCount" select="userCSharp:ResetSG121Count()"/>
        <xsl:for-each select="$ECCCData/*[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup'][./*[local-name()='Type']/*[local-name()='Code']/text()='CLP']">
          <xsl:call-template name="SG121Loop"/>
        </xsl:for-each>
      </xsl:if>

      <xsl:if test="$IsDFODataValid">
       <xsl:variable name="Resetsg121LoopCount" select="userCSharp:ResetSG121Count()"/>
        <xsl:for-each select="$DFOData/*[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup'][./*[local-name()='Type']/*[local-name()='Code']/text()='CLP']">
          <xsl:call-template name="SG121Loop"/>
        </xsl:for-each>
      </xsl:if>

      <xsl:if test ="boolean($GACData)">
        <xsl:variable name="Resetsg121LoopCount" select="userCSharp:ResetSG121Count()"/>
        <xsl:for-each select="$GACData/*[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup'][./*[local-name()='Type']/*[local-name()='Code']/text()='CLP']">
          <xsl:call-template name="SG121Loop"/>
        </xsl:for-each>
      </xsl:if>

      <xsl:if test="boolean($NRCanData)">
        <xsl:variable name="Resetsg121LoopCount" select="userCSharp:ResetSG121Count()"/>
        <xsl:for-each select="$NRCanData/*[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup'][./*[local-name()='Type']/*[local-name()='Code']/text()='CLP']">
          <xsl:call-template name="SG121Loop"/>
        </xsl:for-each>
      </xsl:if>

      <xsl:if test="boolean($PHACData)">
        <xsl:variable name="Resetsg121LoopCount" select="userCSharp:ResetSG121Count()"/>
        <xsl:for-each select="$PHACData/*[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup'][./*[local-name()='Type']/*[local-name()='Code']/text()='CLP']">
          <xsl:call-template name="SG121Loop"/>
        </xsl:for-each>
      </xsl:if>

      <xsl:if test="boolean($TCData)">
        <xsl:variable name="Resetsg121LoopCount" select="userCSharp:ResetSG121Count()"/>
        <xsl:for-each select="$TCData/*[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup'][./*[local-name()='Type']/*[local-name()='Code']/text()='CLP']">
          <xsl:call-template name="SG121Loop"/>
        </xsl:for-each>
      </xsl:if>

      <xsl:if test="boolean($HCData)">
        <xsl:variable name="Resetsg121LoopCount" select="userCSharp:ResetSG121Count()"/>
        <xsl:for-each select="$HCData/*[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup'][./*[local-name()='Type']/*[local-name()='Code']/text()='CLP']">
          <xsl:call-template name="SG121Loop"/>
        </xsl:for-each>
      </xsl:if>

      <xsl:if test="boolean($PHACData)">
        <xsl:call-template name="SG125Loop">
          <xsl:with-param name="Enable" select="$PHACData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='HAPProgramInd']/*[local-name()='Value']/text()='Y'"/>
          <xsl:with-param name="Code" select="'XP01'"/>
          <xsl:with-param name="Qualifier" select="12"/>
        </xsl:call-template>
      </xsl:if>
      
      <xsl:if test="boolean($ECCCData)">
        <xsl:if test="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='VEEProgramInd']/*[local-name()='Value']/text()='N'">
          <ns0:SG125Loop>
            <ns0:RCS_2>
              <ns0:TAGNAME>RCS</ns0:TAGNAME>
              <ns0:RCS01>15</ns0:RCS01>
              <ns0:C550_2>
                <ns0:C55001>XE99</ns0:C55001>
                <ns0:C55003>22</ns0:C55003>
              </ns0:C550_2>
            </ns0:RCS_2>
          </ns0:SG125Loop>
        </xsl:if>
        <xsl:variable name="AOSConformity" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='AOSConformity']" />
        <xsl:if test="boolean($AOSConformity)">
          <xsl:call-template name="SG125Loop">
          <xsl:with-param name="Enable" select="$AOSConformity/*[local-name()='Value']/text()!=''"/>
          <xsl:with-param name="Code" select="$AOSConformity/*[local-name()='Value']"/>
          <xsl:with-param name="Qualifier" select="22"/>
          </xsl:call-template>
        </xsl:if>
        <xsl:variable name="AOSReplacement" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='AOSReplacement']" />
        <xsl:if test="boolean($AOSReplacement)">
          <xsl:call-template name="SG125Loop">
          <xsl:with-param name="Enable" select="$AOSReplacement/*[local-name()='Value']/text()!=''"/>
          <xsl:with-param name="Code" select="$AOSReplacement/*[local-name()='Value']"/>
          <xsl:with-param name="Qualifier" select="22"/>
          </xsl:call-template>
        </xsl:if>
        <xsl:variable name="AOSEvidence" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='AOSEvidence']" />
        <xsl:if test="boolean($AOSEvidence)">
          <xsl:call-template name="SG125Loop">
          <xsl:with-param name="Enable" select="$AOSEvidence/*[local-name()='Value']/text()!=''"/>
          <xsl:with-param name="Code" select="$AOSEvidence/*[local-name()='Value']"/>
          <xsl:with-param name="Qualifier" select="22"/>
          </xsl:call-template>
        </xsl:if>
        <xsl:variable name="AOSRetention" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='AOSRetention']" />
        <xsl:if test="boolean($AOSRetention)">
          <xsl:call-template name="SG125Loop">
          <xsl:with-param name="Enable" select="$AOSRetention/*[local-name()='Value']/text()!=''"/>
          <xsl:with-param name="Code" select="$AOSRetention/*[local-name()='Value']"/>
          <xsl:with-param name="Qualifier" select="22"/>
          </xsl:call-template>
        </xsl:if>
        <xsl:variable name="processCode" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ProcessCode']/*[local-name()='Value']"/>
        <xsl:choose>
        <xsl:when test="$processCode='XE01'">
          <xsl:choose>
            <xsl:when test="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='BulkReporting']/*[local-name()='Value']/text()='Y'">
              <xsl:call-template name="SG125Loop">
                <xsl:with-param name="Enable" select="true()"/>
                <xsl:with-param name="Code" select="'XE06'"/>
                <xsl:with-param name="Qualifier" select="22"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:when test="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='NationalMark']/*[local-name()='Value']/text()='Y'">
              <xsl:call-template name="SG125Loop">
                <xsl:with-param name="Enable" select="true()"/>
                <xsl:with-param name="Code" select="'XE05'"/>
                <xsl:with-param name="Qualifier" select="22"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:when test="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='NonCommercialImport']/*[local-name()='Value']/text()='Y'">
             <xsl:call-template name="SG125Loop">
               <xsl:with-param name="Enable" select="true()"/>
               <xsl:with-param name="Code" select="'XE1N'"/>
               <xsl:with-param name="Qualifier" select="22"/>
             </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="SG125Loop">
                <xsl:with-param name="Enable" select="true()"/>
                <xsl:with-param name="Code" select="'XE01'"/>
                <xsl:with-param name="Qualifier" select="22"/>
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:when test="$processCode='XE02'">
          <xsl:choose>
             <xsl:when test="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='NonCommercialImport']/*[local-name()='Value']/text()='Y'">
             <xsl:call-template name="SG125Loop">
               <xsl:with-param name="Enable" select="true()"/>
               <xsl:with-param name="Code" select="'XE2N'"/>
               <xsl:with-param name="Qualifier" select="22"/>
             </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="SG125Loop">
                  <xsl:with-param name="Enable" select="true()"/>
                  <xsl:with-param name="Code" select="$processCode"/>
                  <xsl:with-param name="Qualifier" select="22"/>
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:when test="$processCode='XE03'">
         <xsl:choose>
           <xsl:when test="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='BulkReporting']/*[local-name()='Value']/text()='Y'">
             <xsl:call-template name="SG125Loop">
               <xsl:with-param name="Enable" select="true()"/>
               <xsl:with-param name="Code" select="'XE07'"/>
               <xsl:with-param name="Qualifier" select="22"/>
             </xsl:call-template>
            </xsl:when>
            <xsl:when test="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='NonCommercialImport']/*[local-name()='Value']/text()='Y'">
             <xsl:call-template name="SG125Loop">
               <xsl:with-param name="Enable" select="true()"/>
               <xsl:with-param name="Code" select="'XE3N'"/>
               <xsl:with-param name="Qualifier" select="22"/>
             </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="SG125Loop">
               <xsl:with-param name="Enable" select="true()"/>
               <xsl:with-param name="Code" select="$processCode"/>
               <xsl:with-param name="Qualifier" select="22"/>
              </xsl:call-template>
             </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:when test="$processCode='XE02' or $processCode='XE04'">
          <xsl:call-template name="SG125Loop">
            <xsl:with-param name="Enable" select="true()"/>
            <xsl:with-param name="Code" select="$processCode"/>
            <xsl:with-param name="Qualifier" select="22"/>
          </xsl:call-template>
        </xsl:when>
        </xsl:choose>
        <xsl:choose>
          <xsl:when test="$processCode='EC01' or $processCode='XE01'">
            <xsl:call-template name="SG125Loop">
              <xsl:with-param name="Enable" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='NationalMark']/*[local-name()='Value']/text()='Y'"/>
              <xsl:with-param name="Code" select="'EC01'"/>
              <xsl:with-param name="Qualifier" select="22"/>
            </xsl:call-template>
            <xsl:call-template name="SG125Loop">
              <xsl:with-param name="Enable" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='EPACertified']/*[local-name()='Value']/text()='Y'"/>
              <xsl:with-param name="Code" select="'EC02'"/>
              <xsl:with-param name="Qualifier" select="22"/>
            </xsl:call-template>
            <xsl:call-template name="SG125Loop">
              <xsl:with-param name="Enable" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='CanadaUnique']/*[local-name()='Value']/text()='Y'"/>
              <xsl:with-param name="Code" select="'EC03'"/>
              <xsl:with-param name="Qualifier" select="22"/>
            </xsl:call-template>
            <xsl:call-template name="SG125Loop">
              <xsl:with-param name="Enable" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='Incomplete']/*[local-name()='Value']/text()='Y'"/>
              <xsl:with-param name="Code" select="'EC04'"/>
              <xsl:with-param name="Qualifier" select="22"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$processCode='EC02' or $processCode='XE02'">
            <xsl:call-template name="SG125Loop">
              <xsl:with-param name="Enable" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='NationalMark']/*[local-name()='Value']/text()='Y'"/>
              <xsl:with-param name="Code" select="'EC05'"/>
              <xsl:with-param name="Qualifier" select="22"/>
            </xsl:call-template>
            <xsl:call-template name="SG125Loop">
              <xsl:with-param name="Enable" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='EPACertified']/*[local-name()='Value']/text()='Y'"/>
              <xsl:with-param name="Code" select="'EC06'"/>
              <xsl:with-param name="Qualifier" select="22"/>
            </xsl:call-template>
            <xsl:call-template name="SG125Loop">
              <xsl:with-param name="Enable" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='CanadaUnique']/*[local-name()='Value']/text()='Y'"/>
              <xsl:with-param name="Code" select="'EC08'"/>
              <xsl:with-param name="Qualifier" select="22"/>
            </xsl:call-template>
            <xsl:call-template name="SG125Loop">
              <xsl:with-param name="Enable" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='Transition']/*[local-name()='Value']/text()='Y'"/>
              <xsl:with-param name="Code" select="'EC09'"/>
              <xsl:with-param name="Qualifier" select="22"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$processCode='EC03' or $processCode='XE03'">
            <xsl:call-template name="SG125Loop">
              <xsl:with-param name="Enable" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='NationalMark']/*[local-name()='Value']/text()='Y'"/>
              <xsl:with-param name="Code" select="'EC05'"/>
              <xsl:with-param name="Qualifier" select="22"/>
            </xsl:call-template>
            <xsl:call-template name="SG125Loop">
              <xsl:with-param name="Enable" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='EPACertified']/*[local-name()='Value']/text()='Y'"/>
              <xsl:with-param name="Code" select="'EC10'"/>
              <xsl:with-param name="Qualifier" select="22"/>
            </xsl:call-template>
            <xsl:call-template name="SG125Loop">
              <xsl:with-param name="Enable" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='CanadaUnique']/*[local-name()='Value']/text()='Y'"/>
              <xsl:with-param name="Code" select="'EC11'"/>
              <xsl:with-param name="Qualifier" select="22"/>
            </xsl:call-template>
            <xsl:call-template name="SG125Loop">
              <xsl:with-param name="Enable" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='Incomplete']/*[local-name()='Value']/text()='Y'"/>
              <xsl:with-param name="Code" select="'EC12'"/>
              <xsl:with-param name="Qualifier" select="22"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$processCode='EC04' or $processCode='XE04'">
            <xsl:call-template name="SG125Loop">
              <xsl:with-param name="Enable" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='NationalMark']/*[local-name()='Value']/text()='Y'"/>
              <xsl:with-param name="Code" select="'EC13'"/>
              <xsl:with-param name="Qualifier" select="22"/>
            </xsl:call-template>
            <xsl:call-template name="SG125Loop">
              <xsl:with-param name="Enable" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='EPACertified']/*[local-name()='Value']/text()='Y'"/>
              <xsl:with-param name="Code" select="'EC14'"/>
              <xsl:with-param name="Qualifier" select="22"/>
            </xsl:call-template>
            <xsl:call-template name="SG125Loop">
              <xsl:with-param name="Enable" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='CanadaUnique']/*[local-name()='Value']/text()='Y'"/>
              <xsl:with-param name="Code" select="'EC15'"/>
              <xsl:with-param name="Qualifier" select="22"/>
            </xsl:call-template>
            <xsl:call-template name="SG125Loop">
              <xsl:with-param name="Enable" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='Incomplete']/*[local-name()='Value']/text()='Y'"/>
              <xsl:with-param name="Code" select="'EC16'"/>
              <xsl:with-param name="Qualifier" select="22"/>
            </xsl:call-template>
          </xsl:when>
        </xsl:choose>
        <xsl:if test="$processCode='XE04'">
          <xsl:call-template name="SG125Loop">
            <xsl:with-param name="Enable" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='NonCommercialImport']/*[local-name()='Value']/text()='Y'"/>
            <xsl:with-param name="Code" select="'XE4N'"/>
            <xsl:with-param name="Qualifier" select="22"/>
          </xsl:call-template>
          <xsl:call-template name="SG125Loop">
            <xsl:with-param name="Enable" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ENGNationalMark']/*[local-name()='Value']/text()='Y'"/>
            <xsl:with-param name="Code" select="'EC19'"/>
            <xsl:with-param name="Qualifier" select="22"/>
          </xsl:call-template>
          <xsl:call-template name="SG125Loop">
            <xsl:with-param name="Enable" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ENGEPACertified']/*[local-name()='Value']/text()='Y'"/>
            <xsl:with-param name="Code" select="'EC20'"/>
            <xsl:with-param name="Qualifier" select="22"/>
          </xsl:call-template>
          <xsl:call-template name="SG125Loop">
            <xsl:with-param name="Enable" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ENGCanadaUnique']/*[local-name()='Value']/text()='Y'"/>
            <xsl:with-param name="Code" select="'EC21'"/>
            <xsl:with-param name="Qualifier" select="22"/>
          </xsl:call-template>
          <xsl:call-template name="SG125Loop">
            <xsl:with-param name="Enable" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ENGIncomplete']/*[local-name()='Value']/text()='Y'"/>
            <xsl:with-param name="Code" select="'EC22'"/>
            <xsl:with-param name="Qualifier" select="22"/>
          </xsl:call-template>
        </xsl:if>
        <xsl:variable name="ECCCWENInd" select="$ECCCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='WENProgramInd']/*[local-name()='Value']/text()"/>
        <xsl:if test="boolean($ECCCWENInd)">
          <xsl:choose>
            <xsl:when test="$ECCCWENInd='Y'">
              <xsl:call-template name="SG125Loop">
                <xsl:with-param name="Enable" select="true()"/>
                <xsl:with-param name="Code" select="'EC17'"/>
                <xsl:with-param name="Qualifier" select="22"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="SG125Loop">
                <xsl:with-param name="Enable" select="true()"/>
                <xsl:with-param name="Code" select="'EC18'"/>
                <xsl:with-param name="Qualifier" select="22"/>
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:if>
      </xsl:if>

      <xsl:if test="$IsDFODataValid">
        <xsl:variable name="direction" select="$DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='Direction']/*[local-name()='Value']"/>
        <xsl:call-template name="SG125Loop">
          <xsl:with-param name="Enable" select="$direction!=''"/>
          <xsl:with-param name="Code" select="$direction"/>
          <xsl:with-param name="Qualifier" select="20"/>
        </xsl:call-template>
      </xsl:if>

      <xsl:call-template name="SG125Loop">
        <xsl:with-param name="Enable" select="boolean($GACData) and $GACData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ComplianceStatement']/*[local-name()='Value']/text()='Y'"/>
        <xsl:with-param name="Code" select="'FA01'"/>
        <xsl:with-param name="Qualifier" select="3"/>
      </xsl:call-template>

      <xsl:if test ="boolean($TCData)">
        <xsl:call-template name="SG125Loop">
        <xsl:with-param name="Enable" select="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='SubProgram']/*[local-name()='Value']/text()='VFS'"/>
        <xsl:with-param name="Code" select="'XT02'"/>
        <xsl:with-param name="Qualifier" select="13"/>
      </xsl:call-template>
      <xsl:call-template name="SG125Loop">
        <xsl:with-param name="Enable" select="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='SubProgram']/*[local-name()='Value']/text()='VFC'"/>
        <xsl:with-param name="Code" select="'XT03'"/>
        <xsl:with-param name="Qualifier" select="13"/>
      </xsl:call-template>
      <xsl:call-template name="SG125Loop">
        <xsl:with-param name="Enable" select="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='SubProgram']/*[local-name()='Value']/text()='PIG'"/>
        <xsl:with-param name="Code" select="'XT04'"/>
        <xsl:with-param name="Qualifier" select="13"/>
      </xsl:call-template>
      <xsl:call-template name="SG125Loop">
        <xsl:with-param name="Enable" select="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='SubProgram']/*[local-name()='Value']/text()='PIL'"/>
        <xsl:with-param name="Code" select="'XT05'"/>
        <xsl:with-param name="Qualifier" select="13"/>
      </xsl:call-template>
      <xsl:call-template name="SG125Loop">
        <xsl:with-param name="Enable" select="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='SubProgram']/*[local-name()='Value']/text()='VCR'"/>
        <xsl:with-param name="Code" select="'XT06'"/>
        <xsl:with-param name="Qualifier" select="13"/>
      </xsl:call-template>
      <xsl:call-template name="SG125Loop">
        <xsl:with-param name="Enable" select="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='SubProgram']/*[local-name()='Value']/text()='VAE'"/>
        <xsl:with-param name="Code" select="'XT07'"/>
        <xsl:with-param name="Qualifier" select="13"/>
      </xsl:call-template>
      <xsl:call-template name="SG125Loop">
        <xsl:with-param name="Enable" select="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='SubProgram']/*[local-name()='Value']/text()='VUV'"/>
        <xsl:with-param name="Code" select="'XT08'"/>
        <xsl:with-param name="Qualifier" select="13"/>
      </xsl:call-template>
      <xsl:call-template name="SG125Loop">
        <xsl:with-param name="Enable" select="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='SubProgram']/*[local-name()='Value']/text()='VCC'"/>
        <xsl:with-param name="Code" select="'XT09'"/>
        <xsl:with-param name="Qualifier" select="13"/>
      </xsl:call-template>
      <xsl:call-template name="SG125Loop">
        <xsl:with-param name="Enable" select="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='SubProgram']/*[local-name()='Value']/text()='VVP'"/>
        <xsl:with-param name="Code" select="'XT10'"/>
        <xsl:with-param name="Qualifier" select="13"/>
      </xsl:call-template>
        <xsl:variable name="ImporterDeclarationCode" select="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ImporterDeclarationCode']/*[local-name()='Value']"/>
        <xsl:variable name="ImporterDeclarationCode2" select="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ImporterDeclarationCode2']/*[local-name()='Value']"/>
        <xsl:variable name="CriteriaConformance" select="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='CriteriaConformance']/*[local-name()='Value']"/>
        <xsl:choose>
          <xsl:when test="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='TPRProgramInd']/*[local-name()='Value']/text()='Y'">
            <xsl:call-template name="SG125Loop">
              <xsl:with-param name="Enable" select="true()"/>
              <xsl:with-param name="Code" select="'XT01'"/>
              <xsl:with-param name="Qualifier" select="13"/>
            </xsl:call-template>
            <xsl:call-template name="SG125Loop">
              <xsl:with-param name="Enable" select="$ImporterDeclarationCode!=''"/>
              <xsl:with-param name="Code" select="$ImporterDeclarationCode"/>
              <xsl:with-param name="Qualifier" select="13"/>
            </xsl:call-template>
            <xsl:call-template name="SG125Loop">
              <xsl:with-param name="Enable" select="$ImporterDeclarationCode2!=''"/>
              <xsl:with-param name="Code" select="$ImporterDeclarationCode2"/>
              <xsl:with-param name="Qualifier" select="13"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$TCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='VPRProgramInd']/*[local-name()='Value']/text()='Y' and $ImporterDeclarationCode!=''">
            <xsl:call-template name="SG125Loop">
              <xsl:with-param name="Enable" select="true()"/>
              <xsl:with-param name="Code" select="$ImporterDeclarationCode"/>
              <xsl:with-param name="Qualifier" select="13"/>
            </xsl:call-template>
          </xsl:when>
        </xsl:choose>
        <xsl:call-template name="SG125Loop">
          <xsl:with-param name="Enable" select="$CriteriaConformance!=''"/>
          <xsl:with-param name="Code" select="$CriteriaConformance"/>
          <xsl:with-param name="Qualifier" select="13"/>
        </xsl:call-template>
      </xsl:if>

      <xsl:if test="boolean($HCData)">
        <xsl:if test="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='CTOProgramInd']/*[local-name()='Value']/text()='Y'">
          <ns0:SG125Loop>
            <ns0:RCS_2>
              <ns0:TAGNAME>RCS</ns0:TAGNAME>
              <ns0:RCS01>15</ns0:RCS01>
              <ns0:C550_2>
                <ns0:C55001>XH01</ns0:C55001>
                <ns0:C55003>12</ns0:C55003>
              </ns0:C550_2>
            </ns0:RCS_2>
          </ns0:SG125Loop>
        </xsl:if>
        <xsl:if test="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='MDEProgramInd']/*[local-name()='Value']/text()='Y'">
          <xsl:variable name="ExceptProcessingCode1" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ExceptProcessingCode1']/*[local-name()='Value']" />
          <xsl:if test="$ExceptProcessingCode1='HC02'">
            <ns0:SG125Loop>
              <ns0:RCS_2>
                <ns0:TAGNAME>RCS</ns0:TAGNAME>
                <ns0:RCS01>15</ns0:RCS01>
                <ns0:C550_2>
                  <ns0:C55001>XH02</ns0:C55001>
                  <ns0:C55003>12</ns0:C55003>
                </ns0:C550_2>
              </ns0:RCS_2>
            </ns0:SG125Loop>
          </xsl:if>
        </xsl:if>
        <xsl:if test="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='PESProgramInd']/*[local-name()='Value']/text()='Y'">
          <xsl:variable name="ExceptProcessingCode1" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ExceptProcessingCode1']/*[local-name()='Value']" />
          <xsl:if test="$ExceptProcessingCode1='HC03'">
            <ns0:SG125Loop>
              <ns0:RCS_2>
                <ns0:TAGNAME>RCS</ns0:TAGNAME>
                <ns0:RCS01>15</ns0:RCS01>
                <ns0:C550_2>
                  <ns0:C55001>XH03</ns0:C55001>
                  <ns0:C55003>12</ns0:C55003>
                </ns0:C550_2>
              </ns0:RCS_2>
            </ns0:SG125Loop>
          </xsl:if>
          <xsl:variable name="ExceptProcessingCode2" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ExceptProcessingCode2']/*[local-name()='Value']" />
          <xsl:if test="$ExceptProcessingCode2='HC04'">
            <ns0:SG125Loop>
              <ns0:RCS_2>
                <ns0:TAGNAME>RCS</ns0:TAGNAME>
                <ns0:RCS01>15</ns0:RCS01>
                <ns0:C550_2>
                  <ns0:C55001>XH04</ns0:C55001>
                  <ns0:C55003>12</ns0:C55003>
                </ns0:C550_2>
              </ns0:RCS_2>
            </ns0:SG125Loop>
          </xsl:if>
        </xsl:if>
      </xsl:if>
      
      <xsl:variable name="ComplianceStatement" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='ComplianceStatement']/*[local-name()='Value']"/>
      <xsl:variable name="DSEProgramInd" select="$HCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='DSEProgramInd']/*[local-name()='Value']/text()"/>
      <xsl:call-template name="SG125Loop">
        <xsl:with-param name="Enable" select="boolean($HCData) and $DSEProgramInd='Y' and $ComplianceStatement='Y'"/>
        <xsl:with-param name="Code" select="'HC01'"/>
        <xsl:with-param name="Qualifier" select="12"/>
      </xsl:call-template>

      <xsl:variable name="CNSCProgramInd" select="$CNSCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='AllProgramInd']/*[local-name()='Value']/text()"/>
      <xsl:if test ="boolean($CNSCData) and $CNSCProgramInd='Y'">
        <ns0:SG127Loop>
          <ns0:PAC_2>
            <ns0:TAGNAME>PAC</ns0:TAGNAME>
            <ns0:PAC01>
              <xsl:value-of select="number($CNSCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='PackQty']/*[local-name()='Value'])"/>
            </ns0:PAC01>
            <ns0:C531>
              <ns0:C53101>3</ns0:C53101>
            </ns0:C531>
            <ns0:C202>
              <ns0:C20201>
                <xsl:value-of select="$CNSCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='PackUQ']/*[local-name()='Value']"/>
              </ns0:C20201>
            </ns0:C202>
          </ns0:PAC_2>
          <ns0:FTX>
            <ns0:TAGNAME>FTX</ns0:TAGNAME>
            <ns0:FTX01>BME</ns0:FTX01>
            <ns0:FTX02/>
            <ns0:C107_1>
              <ns0:C10701/>
            </ns0:C107_1>
            <ns0:C108>
              <ns0:C10801>
                <xsl:value-of select="$CNSCData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='PackMarks']/*[local-name()='Value']"/>
              </ns0:C10801>
            </ns0:C108>
          </ns0:FTX>
        </ns0:SG127Loop>
      </xsl:if>

      <xsl:if test="boolean($NRCanData)">
        <xsl:choose>
          <xsl:when test="$NRCanData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='PackQty2']/*[local-name()='Value']/text()!='' or $NRCanData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='PackQty3']/*[local-name()='Value']/text()!=''">
            <xsl:variable name="qty3" select="$NRCanData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='PackQty1']/*[local-name()='Value']"/>
            <xsl:if test="$qty3!=''">
              <ns0:SG127Loop>
                <ns0:PAC_2>
                  <ns0:TAGNAME>PAC</ns0:TAGNAME>
                  <ns0:PAC01>
                    <xsl:value-of select="number($qty3)"/>
                  </ns0:PAC01>
                  <ns0:C531>
                    <ns0:C53101>1</ns0:C53101>
                  </ns0:C531>
                  <ns0:C202>
                    <ns0:C20201>
                      <xsl:value-of select="$NRCanData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='PackUQ1']/*[local-name()='Value']"/>
                    </ns0:C20201>
                  </ns0:C202>
                </ns0:PAC_2>
              </ns0:SG127Loop>
            </xsl:if>
            <xsl:variable name="qty2" select="$NRCanData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='PackQty2']/*[local-name()='Value']"/>
            <xsl:if test="$qty2!=''">
              <ns0:SG127Loop>
                <ns0:PAC_2>
                  <ns0:TAGNAME>PAC</ns0:TAGNAME>
                  <ns0:PAC01>
                    <xsl:value-of select="number($qty2)"/>
                  </ns0:PAC01>
                  <ns0:C531>
                    <ns0:C53101>2</ns0:C53101>
                  </ns0:C531>
                  <ns0:C202>
                    <ns0:C20201>
                      <xsl:value-of select="$NRCanData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='PackUQ2']/*[local-name()='Value']"/>
                    </ns0:C20201>
                  </ns0:C202>
                </ns0:PAC_2>
              </ns0:SG127Loop>
            </xsl:if>
            <xsl:variable name="qty" select="$NRCanData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='PackQty3']/*[local-name()='Value']"/>
            <xsl:if test="$qty!=''">
              <ns0:SG127Loop>
                <ns0:PAC_2>
                  <ns0:TAGNAME>PAC</ns0:TAGNAME>
                  <ns0:PAC01>
                    <xsl:value-of select="number($qty)"/>
                  </ns0:PAC01>
                  <ns0:C531>
                    <ns0:C53101>3</ns0:C53101>
                  </ns0:C531>
                  <ns0:C202>
                    <ns0:C20201>
                      <xsl:value-of select="$NRCanData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='PackUQ3']/*[local-name()='Value']"/>
                    </ns0:C20201>
                  </ns0:C202>
                </ns0:PAC_2>
              </ns0:SG127Loop>
            </xsl:if>
          </xsl:when>
          <xsl:otherwise>
            <xsl:variable name="qty" select="$NRCanData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='PackQty1']/*[local-name()='Value']"/>
            <xsl:if test="$qty!=''">
              <ns0:SG127Loop>
                <ns0:PAC_2>
                  <ns0:TAGNAME>PAC</ns0:TAGNAME>
                  <ns0:PAC01>
                    <xsl:value-of select="number($qty)"/>
                  </ns0:PAC01>
                  <ns0:C531>
                    <ns0:C53101>4</ns0:C53101>
                  </ns0:C531>
                  <ns0:C202>
                    <ns0:C20201>
                      <xsl:value-of select="$NRCanData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='PackUQ1']/*[local-name()='Value']"/>
                    </ns0:C20201>
                  </ns0:C202>
                </ns0:PAC_2>
              </ns0:SG127Loop>
            </xsl:if>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:if>

      <xsl:if test="boolean($ECCCData)">
        <xsl:for-each select="$ECCCData/*[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup'][./*[local-name()='Type']/*[local-name()='Code']/text()='CCP']">
          <xsl:variable name="QTY" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='Qty']/*[local-name()='Value']"/>
          <xsl:variable name="UQ" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='UQ']/*[local-name()='Value']"/>
          <xsl:variable name="Name" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='Name']/*[local-name()='Value']"/>
          <xsl:if test="$Name!='' or (number($QTY) > 0 and $UQ!='')">
            <ns0:SG128Loop>
              <ns0:COD_1>
                <ns0:TAGNAME>COD</ns0:TAGNAME>
                <ns0:C823_1>
                  <ns0:C82301>ODS</ns0:C82301>
                </ns0:C823_1>
                <ns0:C824_1>
                  <ns0:C82404>
                    <xsl:value-of select="$Name"/>
                  </ns0:C82404>
                </ns0:C824_1>
              </ns0:COD_1>
              <xsl:if test="number($QTY) > 0 and $UQ!=''">
                <ns0:QTY_4>
                  <ns0:TAGNAME>QTY</ns0:TAGNAME>
                  <ns0:C186_4>
                    <ns0:C18601>1</ns0:C18601>
                    <ns0:C18602>
                      <xsl:value-of select ="$QTY"/>
                    </ns0:C18602>
                    <ns0:C18603>
                      <xsl:value-of select ="$UQ"/>
                    </ns0:C18603>
                  </ns0:C186_4>
                </ns0:QTY_4>
              </xsl:if>
            </ns0:SG128Loop>
          </xsl:if>
        </xsl:for-each>
      </xsl:if>

      <xsl:if test="boolean($GACData)">
        <xsl:variable name="FibreCountryOfOrigin" select="$GACData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='FibreCountryOfOrigin']/*[local-name()='Value']"/>
        <xsl:if test="$FibreCountryOfOrigin!=''">
          <ns0:SG128Loop>
            <ns0:COD_1>
              <ns0:TAGNAME>COD</ns0:TAGNAME>
              <ns0:C823_1>
                <ns0:C82301>FI</ns0:C82301>
              </ns0:C823_1>
              <ns0:C824_1>
                <ns0:C82402>
                  <xsl:value-of select="$FibreCountryOfOrigin"/>
                </ns0:C82402>
                <ns0:C82404>Fibre</ns0:C82404>
              </ns0:C824_1>
            </ns0:COD_1>
          </ns0:SG128Loop>
        </xsl:if>
        <xsl:variable name="YarnCountryOfOrigin" select="$GACData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='YarnCountryOfOrigin']/*[local-name()='Value']"/>
        <xsl:if test="$YarnCountryOfOrigin!=''">
          <ns0:SG128Loop>
            <ns0:COD_1>
              <ns0:TAGNAME>COD</ns0:TAGNAME>
              <ns0:C823_1>
                <ns0:C82301>YA</ns0:C82301>
              </ns0:C823_1>
              <ns0:C824_1>
                <ns0:C82402>
                  <xsl:value-of select="$YarnCountryOfOrigin"/>
                </ns0:C82402>
                <ns0:C82404>Yarn</ns0:C82404>
              </ns0:C824_1>
            </ns0:COD_1>
          </ns0:SG128Loop>
        </xsl:if>
        <xsl:variable name="FabricCountryOfOrigin" select="$GACData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='FabricCountryOfOrigin']/*[local-name()='Value']"/>
        <xsl:if test="$FabricCountryOfOrigin!=''">
          <ns0:SG128Loop>
            <ns0:COD_1>
              <ns0:TAGNAME>COD</ns0:TAGNAME>
              <ns0:C823_1>
                <ns0:C82301>FA</ns0:C82301>
              </ns0:C823_1>
              <ns0:C824_1>
                <ns0:C82402>
                  <xsl:value-of select="$FabricCountryOfOrigin"/>
                </ns0:C82402>
                <ns0:C82404>Fabric</ns0:C82404>
              </ns0:C824_1>
            </ns0:COD_1>
          </ns0:SG128Loop>
        </xsl:if>
				<xsl:for-each select="$GACData/*[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup'][./*[local-name()='Type']/*[local-name()='Code']/text()='CLP']">
					<xsl:variable name="DocumentType" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='Type']/*[local-name()='Value']"/>
					<xsl:variable name="RefNo" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='RefNo']/*[local-name()='Value']"/>
					<xsl:variable name="SmeltAndPourCountryCode" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='RN_NKSmeltAndPourCountryCode']/*[local-name()='Value']"/>
					<xsl:if test="$DocumentType='2006' and ($RefNo='GIP80' or $RefNo='GIP81') and $SmeltAndPourCountryCode!=''">
						<ns0:SG128Loop>
							<ns0:COD_1>
								<ns0:TAGNAME>COD</ns0:TAGNAME>
								<ns0:C823_1>
									<ns0:C82301>COM</ns0:C82301>
								</ns0:C823_1>
								<ns0:C824_1>
									<ns0:C82402>
										<xsl:value-of select="$SmeltAndPourCountryCode"/>
									</ns0:C82402>
								</ns0:C824_1>
							</ns0:COD_1>
						</ns0:SG128Loop>
					</xsl:if>
				</xsl:for-each>
      </xsl:if>

      <xsl:if test="boolean($HCData)">
        <xsl:for-each select="$HCData/*[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup'][./*[local-name()='Type']/*[local-name()='Code']/text()='CCP']">
          <xsl:variable name="QTY" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='Qty']/*[local-name()='Value']"/>
          <xsl:variable name="PCD" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='Concentration']/*[local-name()='Value']"/>
          <xsl:variable name="Name" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='Name']/*[local-name()='Value']"/>
          <xsl:if test="$Name!='' or number($QTY) > 0 or number($PCD) > 0">
            <ns0:SG128Loop>
              <ns0:COD_1>
                <ns0:TAGNAME>COD</ns0:TAGNAME>
                <ns0:C823_1>
                  <ns0:C82301>Y</ns0:C82301>
                </ns0:C823_1>
                <ns0:C824_1>
                  <ns0:C82404>
                    <xsl:value-of select="$Name"/>
                  </ns0:C82404>
                </ns0:C824_1>
              </ns0:COD_1>
              <xsl:if test="number($QTY) > 0">
                <ns0:QTY_4>
                  <ns0:TAGNAME>QTY</ns0:TAGNAME>
                  <ns0:C186_4>
                    <ns0:C18601>1</ns0:C18601>
                    <ns0:C18602>
                      <xsl:value-of select ="$QTY"/>
                    </ns0:C18602>
                    <ns0:C18603>
                      <xsl:value-of select ="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='UQ']/*[local-name()='Value']"/>
                    </ns0:C18603>
                  </ns0:C186_4>
                </ns0:QTY_4>
              </xsl:if>
              <xsl:if test="number($PCD) > 0">
                <ns0:PCD_1>
                  <ns0:TAGNAME>PCD</ns0:TAGNAME>
                  <ns0:C501_1>
                    <ns0:C50101>6</ns0:C50101>
                    <ns0:C50102>
                      <xsl:value-of select="$PCD"/>
                    </ns0:C50102>
                  </ns0:C501_1>
                </ns0:PCD_1>
              </xsl:if>
            </ns0:SG128Loop>
          </xsl:if>
        </xsl:for-each>
      </xsl:if>

      <xsl:if test ="$IsDFODataValid and $DFOData/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='Eviscerated']/*[local-name()='Value']/text()='Y'">
        <ns0:SG142Loop>
          <ns0:PRC_1>
            <ns0:TAGNAME>PRC</ns0:TAGNAME>
            <ns0:C242_1>
              <ns0:C24201>12</ns0:C24201>
            </ns0:C242_1>
          </ns0:PRC_1>
          <xsl:variable name="FoodProcessor" select="$DFOData/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='FoodProcessor']"/>
          <xsl:if test="$IsNotLastSG47Loop = false() or $IsLastSG101Loop = false() or $IsLastSG117Loop = false() or $FoodProcessor != ''">
            <ns0:SEQ_5>
              <ns0:TAGNAME>SEQ</ns0:TAGNAME>
              <ns0:SEQ01>1</ns0:SEQ01>
            </ns0:SEQ_5>
          </xsl:if>
          <xsl:if test="$FoodProcessor!=''">
            <ns0:SG143Loop>
              <xsl:call-template name="NADSegment">
                <xsl:with-param name ="OrgAddress" select="$FoodProcessor"/>
                <xsl:with-param name="Type" select="'GN'"/>
                <xsl:with-param name="UseDefaultPartyID" select="true()"/>
                <xsl:with-param name="ContactLoopID" select="'144'"/>
              </xsl:call-template>
            </ns0:SG143Loop>
          </xsl:if>
        </ns0:SG142Loop>
      </xsl:if>
    </ns0:SG117Loop>
    </xsl:if>
  </xsl:template>

  <xsl:template name="SG121Loop">
   <xsl:variable name="CountryOfOriginOnInvoiceLine" select="*[local-name()='CountryOfOrigin']/*[local-name()='Code']"/>
   <xsl:variable name="sg121LoopCount" select="userCSharp:SG121Count()"/>
   <xsl:if test="100 > number($sg121LoopCount)">
    <ns0:SG121Loop>
      <ns0:DOC_3>
        <ns0:TAGNAME>DOC</ns0:TAGNAME>
        <ns0:C002_4>
          <ns0:C00201>916</ns0:C00201>
          <ns0:C00202>
            <xsl:value-of select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='Type']/*[local-name()='Value']"/>
          </ns0:C00202>
        </ns0:C002_4>
        <ns0:C503_3>
          <ns0:C50301>
            <xsl:value-of select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='RefNo']/*[local-name()='Value']"/>
          </ns0:C50301>
          <ns0:C50303>
            <xsl:value-of select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='DIFRefNumberOrLocation']/*[local-name()='Value']"/>
          </ns0:C50303>
        </ns0:C503_3>
      </ns0:DOC_3>
      <xsl:variable name="SecondaryRefNo" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='SecondaryRefNo']/*[local-name()='Value']"/>
      <xsl:if test="$SecondaryRefNo!=''">
        <ns0:RFF_7>
          <ns0:TAGNAME>RFF</ns0:TAGNAME>
          <ns0:C506_7>
            <ns0:C50601>ABB</ns0:C50601>
            <ns0:C50602>
              <xsl:value-of select="$SecondaryRefNo"/>
            </ns0:C50602>
          </ns0:C506_7>
        </ns0:RFF_7>
      </xsl:if>
      <xsl:variable name="LPCOEndDate" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='LPCOEndDate']/*[local-name()='Value']"/>
      <xsl:if test="$LPCOEndDate!=''">
        <ns0:DTM_6>
          <ns0:TAGNAME>DTM</ns0:TAGNAME>
          <ns0:C507_6>
            <ns0:C50701>36</ns0:C50701>
            <ns0:C50702>
              <xsl:value-of select="ScriptNS1:FormatXmlDateTime($LPCOEndDate, 'yyyyMMdd')"/>
            </ns0:C50702>
            <ns0:C50703>102</ns0:C50703>
          </ns0:C507_6>
        </ns0:DTM_6>
      </xsl:if>
      <xsl:variable name="LPCOStartDate" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='LPCOStartDate']/*[local-name()='Value']"/>
      <xsl:if test="$LPCOStartDate!=''">
        <ns0:DTM_6>
          <ns0:TAGNAME>DTM</ns0:TAGNAME>
          <ns0:C507_6>
            <ns0:C50701>7</ns0:C50701>
            <ns0:C50702>
              <xsl:value-of select="ScriptNS1:FormatXmlDateTime($LPCOStartDate, 'yyyyMMdd')"/>
            </ns0:C50702>
            <ns0:C50703>102</ns0:C50703>
          </ns0:C507_6>
        </ns0:DTM_6>
      </xsl:if>
      <xsl:variable name="LPCOIssueDate" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='LPCOIssueDate']/*[local-name()='Value']"/>
      <xsl:if test="$LPCOIssueDate!=''">
        <ns0:DTM_6>
          <ns0:TAGNAME>DTM</ns0:TAGNAME>
          <ns0:C507_6>
            <ns0:C50701>137</ns0:C50701>
            <ns0:C50702>
              <xsl:value-of select="ScriptNS1:FormatXmlDateTime($LPCOIssueDate, 'yyyyMMdd')"/>
            </ns0:C50702>
            <ns0:C50703>102</ns0:C50703>
          </ns0:C507_6>
        </ns0:DTM_6>
      </xsl:if>
      <xsl:variable name="Qty" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='Qty']/*[local-name()='Value']"/>
      <xsl:variable name="UQ" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='UQ']/*[local-name()='Value']"/>
      <xsl:if test="$Qty!='' and $UQ!=''">
        <ns0:QTY_3>
          <ns0:TAGNAME>QTY</ns0:TAGNAME>
          <ns0:C186_3>
            <ns0:C18601>1</ns0:C18601>
            <ns0:C18602>
              <xsl:value-of select="$Qty"/>
            </ns0:C18602>
            <ns0:C18603>
              <xsl:value-of select="$UQ"/>
            </ns0:C18603>
          </ns0:C186_3>
        </ns0:QTY_3>
      </xsl:if>
      <xsl:call-template name="SG122Loop"/>
      <xsl:variable name="CountryOfIssuance" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='CountryOfIssuance']/*[local-name()='Value']"/>
      <xsl:if test="$CountryOfIssuance!=''">
        <ns0:SG124Loop>
          <ns0:LOC_6>
            <ns0:TAGNAME>LOC</ns0:TAGNAME>
            <ns0:LOC01>91</ns0:LOC01>
            <ns0:C517_6>
              <ns0:C51701>
                <xsl:value-of select="$CountryOfIssuance"/>
              </ns0:C51701>
            </ns0:C517_6>
          </ns0:LOC_6>
        </ns0:SG124Loop>
      </xsl:if>
      <xsl:variable name="CountryOfOrigin" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='CountryOfOrigin']/*[local-name()='Value']"/>
      <xsl:if test="$CountryOfOriginOnInvoiceLine!='' or $CountryOfOrigin!=''">
        <ns0:SG124Loop>
          <ns0:LOC_6>
            <ns0:TAGNAME>LOC</ns0:TAGNAME>
            <ns0:LOC01>27</ns0:LOC01>
            <ns0:C517_6>
              <ns0:C51701>
            <xsl:choose>
            <xsl:when test=" $CountryOfOriginOnInvoiceLine!=''">
              <xsl:value-of select="$CountryOfOriginOnInvoiceLine"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$CountryOfOrigin"/>
            </xsl:otherwise>
            </xsl:choose>
              </ns0:C51701>
            </ns0:C517_6>
          </ns0:LOC_6>
        </ns0:SG124Loop>
      </xsl:if>
      <xsl:if test="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='IsMixedCountryOfOrigin']/*[local-name()='Value']/text()='Y'">
        <ns0:SG124Loop>
          <ns0:LOC_6>
            <ns0:TAGNAME>LOC</ns0:TAGNAME>
            <ns0:LOC01>27</ns0:LOC01>
            <ns0:C517_6>
              <ns0:C51702>MIX</ns0:C51702>
            </ns0:C517_6>
          </ns0:LOC_6>
        </ns0:SG124Loop>
      </xsl:if>
      <xsl:variable name="AuthorizationCountry" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='AuthorizationCountry']/*[local-name()='Value']"/>
      <xsl:if test="$AuthorizationCountry!=''">
        <ns0:SG124Loop>
          <ns0:LOC_6>
            <ns0:TAGNAME>LOC</ns0:TAGNAME>
            <ns0:LOC01>44</ns0:LOC01>
            <ns0:C517_6>
              <ns0:C51701>
                <xsl:value-of select="$AuthorizationCountry"/>
              </ns0:C51701>
            </ns0:C517_6>
          </ns0:LOC_6>
        </ns0:SG124Loop>
      </xsl:if>
    </ns0:SG121Loop>
   </xsl:if>
  </xsl:template>

  <xsl:template name="SG122Loop">
    <xsl:variable name="CLP" select="."/>
    <xsl:variable name="ShipmentOrgAddressCollection" select="ancestor::*[local-name()='Shipment']/*[local-name()='OrganizationAddressCollection']"/>
    <xsl:variable name="CommercialInvoiceOrgAddressCollection" select="ancestor::*[local-name()='CommercialInvoice']/*[local-name()='OrganizationAddressCollection']"/>
    <xsl:variable name="Manufacturer" select="ancestor::*[local-name()='CommercialInvoiceLine']/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='Manufacturer']"/>
    <xsl:for-each select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='LPCOHolderType'] | *[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']/text()='LPCOApplicant']">
      <xsl:variable name="Key" select="*[local-name()='Key']/text()"/>
      <xsl:variable name="Value" select="*[local-name()='Value']/text()"/>
      <xsl:variable name="LPCOApplicantName" select="../*[local-name()='AddInfo'][./*[local-name()='Key']/text()='LPCOApplicantName']/*[local-name()='Value']"/>
      <xsl:variable name="LPCOHolderName" select="../*[local-name()='AddInfo'][./*[local-name()='Key']/text()='LPCOHolderName']/*[local-name()='Value']"/>
      <xsl:variable name="LPCO">
        <Addresses>
          <xsl:choose>
            <xsl:when test="$Value='IMP'">
              <Address>
                <xsl:copy-of select="$ShipmentOrgAddressCollection/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='ImporterDocumentaryAddress']"/>
              </Address>
            </xsl:when>
            <xsl:when test="$Value='SUP'">
              <Address>
                <xsl:copy-of select="$ShipmentOrgAddressCollection/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='SupplierPickupDeliveryAddress']"/>
              </Address>
            </xsl:when>
            <xsl:when test="$Value='IOR'">
              <Address>
                <xsl:copy-of select="$ShipmentOrgAddressCollection/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='ImporterOfRecord']"/>
              </Address>
            </xsl:when>
            <xsl:when test="$Value='BRK'">
              <Address>
                <xsl:copy-of select="$ShipmentOrgAddressCollection/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='CustomsBroker']"/>
              </Address>
            </xsl:when>
            <xsl:when test="$Value='EXP'">
              <Address>
                <xsl:copy-of select="$CommercialInvoiceOrgAddressCollection/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='Exporter']"/>
              </Address>
            </xsl:when>
            <xsl:when test="$Value='MAN' and boolean($Manufacturer)">
              <Address>
                <xsl:copy-of select="$Manufacturer"/>
              </Address>
            </xsl:when>
            <xsl:when test="$Value='MAN' and not(boolean($Manufacturer))">
              <Address>
                <xsl:copy-of select="$CommercialInvoiceOrgAddressCollection/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='Manufacturer']"/>
              </Address>
            </xsl:when>
            <xsl:when test="$Value='OTH'">
              <Address>
                <xsl:copy-of select="$CLP/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='LPCOHolder']"/>
              </Address>
              <Address>
                <xsl:copy-of select="$CLP/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType']/text()='LPCOApplicant']"/>
              </Address>
            </xsl:when>
          </xsl:choose>
        </Addresses>
      </xsl:variable>
      <xsl:for-each select="msxsl:node-set($LPCO)/Addresses/Address/*[local-name()='OrganizationAddress']">
        <xsl:variable name="Type">
          <xsl:choose>
            <xsl:when test="$Value='OTH' and *[local-name()='AddressType']='LPCOHolder'">
              <xsl:value-of select="'DFK'"/>
            </xsl:when>
            <xsl:when test="$Value='OTH' and *[local-name()='AddressType']='LPCOApplicant'">
              <xsl:value-of select="'DDD'"/>
            </xsl:when>
            <xsl:when test="$Key='LPCOHolderType'">
              <xsl:value-of select="'DFK'"/>
            </xsl:when>
            <xsl:when test="$Key='LPCOApplicant'">
              <xsl:value-of select="'DDD'"/>
            </xsl:when>
          </xsl:choose>
        </xsl:variable>
        <ns0:SG122Loop>
          <xsl:call-template name="NADSegment">
            <xsl:with-param name="OrgAddress" select="."/>
            <xsl:with-param name="Type" select="$Type"/>
            <xsl:with-param name="ContactLoopID" select="'123'"/>
            <xsl:with-param name="CLP" select="$CLP"/>
            <xsl:with-param name="OverrideCompanyName">
              <xsl:choose>
                <xsl:when test="$Key='LPCOHolderType'">
                  <xsl:value-of select="$LPCOHolderName"/>
                </xsl:when>
                <xsl:when test="$Key='LPCOApplicant'">
                  <xsl:value-of select="$LPCOApplicantName"/>
                </xsl:when>
              </xsl:choose>
            </xsl:with-param>
          </xsl:call-template>
        </ns0:SG122Loop>
      </xsl:for-each>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="ContactLoop">
    <xsl:param name="LoopID"/>
    <xsl:param name="OrgAddress"/>
    <xsl:param name="CLP"/>
    <xsl:param name="MandatoryCOM"/>
    <xsl:param name="Type"/>

    <xsl:choose>
      <xsl:when test="$CLP and
                (
                  concat($CLP/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='AuthorizedPartyContactName']/*[local-name()='Value'],
                  $CLP/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='AuthorizedPartyContactEmail']/*[local-name()='Value'],
                  $CLP/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='AuthorizedPartyContactPhone']/*[local-name()='Value'],
                  $CLP/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='AuthorizedPartyContactFax']/*[local-name()='Value'])!=''
                  or
                  concat($CLP/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='ApplicantContactName']/*[local-name()='Value'], 
                  $CLP/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='ApplicantContactEmail']/*[local-name()='Value'],
                  $CLP/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='ApplicantContactPhone']/*[local-name()='Value'],
                  $CLP/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='ApplicantContactFax']/*[local-name()='Value'])!=''
                  or
                  concat($CLP/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='ContactName']/*[local-name()='Value'],
                  $CLP/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='ContactEmail']/*[local-name()='Value'],
                  $CLP/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='ContactPhone']/*[local-name()='Value'],
                  $CLP/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='ContactFax']/*[local-name()='Value'])!=''
                )">
        <xsl:variable name="CLPContactName">
          <xsl:variable name="AuthorizedPartyContactName" select="$CLP/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='AuthorizedPartyContactName']/*[local-name()='Value']"/>
          <xsl:variable name="ApplicantContactName" select="$CLP/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='ApplicantContactName']/*[local-name()='Value']"/>
          <xsl:choose>
            <xsl:when test="$Type='DFK' and $AuthorizedPartyContactName">
              <xsl:value-of select="$AuthorizedPartyContactName"/>
            </xsl:when>
            <xsl:when test="$Type='DDD' and $ApplicantContactName">
              <xsl:value-of select="$ApplicantContactName"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$CLP/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='ContactName']/*[local-name()='Value']"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:variable name="ContactName" select="userCSharp:ReturnEmptyIfHasInvalidCharacters($CLPContactName)"/>

        <xsl:variable name="CLPContactEmail">
          <xsl:variable name="AuthorizedPartyContactEmail" select="$CLP/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='AuthorizedPartyContactEmail']/*[local-name()='Value']"/>
          <xsl:variable name="ApplicantContactEmail" select="$CLP/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='ApplicantContactEmail']/*[local-name()='Value']"/>
          <xsl:choose>
            <xsl:when test="$Type='DFK' and $AuthorizedPartyContactEmail">
              <xsl:value-of select="$AuthorizedPartyContactEmail"/>
            </xsl:when>
            <xsl:when test="$Type='DDD' and $ApplicantContactEmail">
              <xsl:value-of select="$ApplicantContactEmail"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$CLP/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='ContactEmail']/*[local-name()='Value']"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <xsl:variable name="CLPContactPhone">
          <xsl:variable name="AuthorizedPartyContactPhone" select="$CLP/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='AuthorizedPartyContactPhone']/*[local-name()='Value']"/>
          <xsl:variable name="ApplicantContactPhone" select="$CLP/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='ApplicantContactPhone']/*[local-name()='Value']"/>
          <xsl:choose>
            <xsl:when test="$Type='DFK' and $AuthorizedPartyContactPhone">
              <xsl:value-of select="$AuthorizedPartyContactPhone"/>
            </xsl:when>
            <xsl:when test="$Type='DDD' and $ApplicantContactPhone">
              <xsl:value-of select="$ApplicantContactPhone"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$CLP/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='ContactPhone']/*[local-name()='Value']"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <xsl:variable name="CLPContactFax" select="$CLP/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key']='ContactFax']/*[local-name()='Value']"/>
        <xsl:element name="ns0:SG{$LoopID}Loop" namespace="http://cargowise.com/ehub/products/canadiancustoms">
          <ns0:CTA>
            <ns0:TAGNAME>CTA</ns0:TAGNAME>
            <ns0:CTA01>
              <xsl:choose>
                <xsl:when test="$OrgAddress/*[local-name()='AddressType']/text()='Manufacturer'">
                  <xsl:value-of select="'AH'"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="'IC'"/>
                </xsl:otherwise>
              </xsl:choose>
            </ns0:CTA01>
            <xsl:if test="$ContactName!=''">
              <ns0:C056_1>
                <ns0:C05602>
                  <xsl:value-of select="normalize-space(substring($ContactName, 1, 70))"/>
                </ns0:C05602>
              </ns0:C056_1>
            </xsl:if>
          </ns0:CTA>
          <xsl:if test="$CLPContactEmail!=''">
            <ns0:COM>
              <ns0:TAGNAME>COM</ns0:TAGNAME>
              <ns0:C076>
                <ns0:C07601>
                  <xsl:value-of select="normalize-space(substring($CLPContactEmail, 1, 50))"/>
                </ns0:C07601>
                <ns0:C07602>EM</ns0:C07602>
              </ns0:C076>
            </ns0:COM>
          </xsl:if>
          <xsl:if test="$CLPContactPhone!=''">
            <ns0:COM>
              <ns0:TAGNAME>COM</ns0:TAGNAME>
              <ns0:C076>
                <ns0:C07601>
                  <xsl:value-of select="normalize-space(translate(substring($CLPContactPhone, 1, 50), '+-() ', ''))"/>
                </ns0:C07601>
                <ns0:C07602>TE</ns0:C07602>
              </ns0:C076>
            </ns0:COM>
          </xsl:if>
          <xsl:if test="$CLPContactFax!=''">
            <ns0:COM>
              <ns0:TAGNAME>COM</ns0:TAGNAME>
              <ns0:C076>
                <ns0:C07601>
                  <xsl:value-of select="normalize-space(translate(substring($CLPContactFax, 1, 50), '+-() ', ''))"/>
                </ns0:C07601>
                <ns0:C07602>FX</ns0:C07602>
              </ns0:C076>
            </ns0:COM>
          </xsl:if>
        </xsl:element>
      </xsl:when>
      <xsl:otherwise>
        <xsl:variable name="ContactName" select="userCSharp:ReturnEmptyIfHasInvalidCharacters($OrgAddress/*[local-name()='Contact'])"/>
        <xsl:variable name="ContactEmail" select="$OrgAddress/*[local-name()='Email']"/>
        <xsl:variable name="ContactPhone" select="$OrgAddress/*[local-name()='Phone']"/>
        <xsl:variable name="ContactFax" select="$OrgAddress/*[local-name()='Fax']"/>
        <xsl:if test="concat($ContactName, $ContactEmail, $ContactPhone, $ContactFax)!=''">
          <xsl:element name="ns0:SG{$LoopID}Loop" namespace="http://cargowise.com/ehub/products/canadiancustoms">
            <ns0:CTA>
              <ns0:TAGNAME>CTA</ns0:TAGNAME>
              <ns0:CTA01>
                <xsl:choose>
                  <xsl:when test="$OrgAddress/*[local-name()='AddressType']/text()='Manufacturer'">
                    <xsl:value-of select="'AH'"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="'IC'"/>
                  </xsl:otherwise>
                </xsl:choose>
              </ns0:CTA01>
              <xsl:if test="$ContactName!=''">
                <ns0:C056_1>
                  <ns0:C05601/>
                  <ns0:C05602>
                    <xsl:value-of select="normalize-space(substring($ContactName, 1, 70))"/>
                  </ns0:C05602>
                </ns0:C056_1>
              </xsl:if>
            </ns0:CTA>
            <xsl:if test="$ContactEmail!=''">
              <ns0:COM>
                <ns0:TAGNAME>COM</ns0:TAGNAME>
                <ns0:C076>
                  <ns0:C07601>
                    <xsl:value-of select="normalize-space(substring($ContactEmail, 1, 50))"/>
                  </ns0:C07601>
                  <ns0:C07602>EM</ns0:C07602>
                </ns0:C076>
              </ns0:COM>
            </xsl:if>
            <xsl:if test="$ContactPhone!=''">
              <ns0:COM>
                <ns0:TAGNAME>COM</ns0:TAGNAME>
                <ns0:C076>
                  <ns0:C07601>
                    <xsl:value-of select="normalize-space(translate(substring($ContactPhone, 1, 50), '+-() ', ''))"/>
                  </ns0:C07601>
                  <ns0:C07602>TE</ns0:C07602>
                </ns0:C076>
              </ns0:COM>
            </xsl:if>
            <xsl:if test="$ContactFax!=''">
              <ns0:COM>
                <ns0:TAGNAME>COM</ns0:TAGNAME>
                <ns0:C076>
                  <ns0:C07601>
                    <xsl:value-of select="normalize-space(translate(substring($ContactFax, 1, 50), '+-() ', ''))"/>
                  </ns0:C07601>
                  <ns0:C07602>FX</ns0:C07602>
                </ns0:C076>
              </ns0:COM>
            </xsl:if>
            <xsl:if test="$MandatoryCOM=true() and $ContactName='' and $ContactPhone='' and $ContactEmail='' and $ContactFax=''">
              <ns0:COM>
                <ns0:TAGNAME>COM</ns0:TAGNAME>
                <ns0:C076>
                  <ns0:C07601></ns0:C07601>
                  <ns0:C07602>TE</ns0:C07602>
                </ns0:C076>
              </ns0:COM>
            </xsl:if>
          </xsl:element>
        </xsl:if>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="NADSegment">
    <xsl:param name="Type"/>
    <xsl:param name="OrgAddress"/>
    <xsl:param name="PartyID"/>
    <xsl:param name="UseDefaultPartyID" select="true()"/>
    <xsl:param name="ContactLoopID"/>
    <xsl:param name="CLP"/>
    <xsl:param name="MandatoryCOM"/>
    <xsl:param name="OverrideCompanyName"/>

    <xsl:variable name="CompanyName">
      <xsl:choose>
        <xsl:when test="$OverrideCompanyName=''">
          <xsl:value-of select="$OrgAddress/*[local-name()='CompanyName']/text()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$OverrideCompanyName"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="CombinedAddress" select="concat($OrgAddress/*[local-name()='Address1']/text(), ' ', $OrgAddress/*[local-name()='Address2']/text())" />
    <xsl:variable name="City" select="$OrgAddress/*[local-name()='City']/text()"/>
    <xsl:variable name="State" select="$OrgAddress/*[local-name()='State']/text()"/>
    <xsl:variable name="PostCode" select="$OrgAddress/*[local-name()='Postcode']/text()"/>
    <xsl:variable name="CountryCode" select="$OrgAddress/*[local-name()='Country']/*[local-name()='Code']/text()"/>

    <xsl:if test="$OrgAddress">
      <ns0:NAD>
        <ns0:TAGNAME>NAD</ns0:TAGNAME>
        <ns0:NAD01>
          <xsl:value-of select="$Type"/>
        </ns0:NAD01>
        <xsl:variable name="PartyIDOrDefault">
          <xsl:choose>
            <xsl:when test="$PartyID!=''">
              <xsl:value-of select="$PartyID"/>
            </xsl:when>
            <xsl:when test="$UseDefaultPartyID='true'">
              <xsl:value-of select="$OrgAddress/*[local-name()='GovRegNum']"/>
            </xsl:when>
          </xsl:choose>
        </xsl:variable>
        <ns0:C082>
          <xsl:if test="$PartyIDOrDefault != ''">
            <ns0:C08201>
              <xsl:value-of select="normalize-space($PartyIDOrDefault)" />
            </ns0:C08201>
          </xsl:if>
        </ns0:C082>
        <ns0:C058>
          <ns0:C05801/>
        </ns0:C058>
        <ns0:C080>
          <xsl:if test="$CompanyName != ''">
            <ns0:C08001>
              <xsl:value-of select="userCSharp:EscapeString(normalize-space(substring($CompanyName, 1, 70)))"/>
            </ns0:C08001>
            <xsl:variable name="CompanyName2" select="substring($CompanyName, 71, 70)"/>
            <xsl:if test="$CompanyName2!=''">
              <ns0:C08002>
                <xsl:value-of select="userCSharp:EscapeString(normalize-space($CompanyName2))"/>
              </ns0:C08002>
            </xsl:if>
            <xsl:variable name="CompanyName3" select="substring($CompanyName, 141, 70)"/>
            <xsl:if test="$CompanyName3!=''">
              <ns0:C08003>
                <xsl:value-of select="userCSharp:EscapeString(normalize-space($CompanyName3))"/>
              </ns0:C08003>
            </xsl:if>
          </xsl:if>
        </ns0:C080>
        <ns0:C059>
          <xsl:if test="$CombinedAddress != ''">
            <ns0:C05901>
              <xsl:value-of select="userCSharp:EscapeString(normalize-space(substring($CombinedAddress, 1, 35)))"/>
            </ns0:C05901>
            <xsl:variable name="CombinedAddress2" select="substring($CombinedAddress, 36, 35)"/>
            <xsl:if test="$CombinedAddress2!=''">
              <ns0:C05902>
                <xsl:value-of select="userCSharp:EscapeString(normalize-space($CombinedAddress2))"/>
              </ns0:C05902>
            </xsl:if>
            <xsl:variable name="CombinedAddress3" select="substring($CombinedAddress, 71, 35)"/>
            <xsl:if test="$CombinedAddress3!=''">
              <ns0:C05903>
                <xsl:value-of select="userCSharp:EscapeString(normalize-space($CombinedAddress3))"/>
              </ns0:C05903>
            </xsl:if>
          </xsl:if>
        </ns0:C059>
        <ns0:NAD06>
          <xsl:if test="$City!=''">
            <xsl:value-of select="userCSharp:EscapeString(normalize-space(substring($City, 1, 35)))"/>
          </xsl:if>
        </ns0:NAD06>
        <ns0:C819>
          <xsl:if test="$State!=''">
            <ns0:C81901>
              <xsl:variable name="address" select="normalize-space(ScriptNS5:CallActionProcedureHelper('GetStateCode', '@Code', '@FullName', $State))"/>
              <xsl:value-of select="normalize-space(substring($address,1,6))"/>
            </ns0:C81901>
          </xsl:if>
        </ns0:C819>
        <ns0:NAD08>
          <xsl:if test="$PostCode!=''">
            <xsl:choose>
              <xsl:when test="substring($CountryCode, 1, 2)='CA'">
                <xsl:value-of select="normalize-space(substring(translate($PostCode, ' -', ''), 1, 6))"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="normalize-space(substring($PostCode, 1, 9))"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:if>
        </ns0:NAD08>
        <ns0:NAD09>
          <xsl:if test="$CountryCode!=''">
            <xsl:value-of select="normalize-space(substring($CountryCode, 1, 2))"/>
          </xsl:if>
        </ns0:NAD09>
      </ns0:NAD>
      <xsl:call-template name="ContactLoop">
        <xsl:with-param name="LoopID" select="$ContactLoopID"/>
        <xsl:with-param name="OrgAddress" select="$OrgAddress"/>
        <xsl:with-param name="CLP" select="$CLP"/>
        <xsl:with-param name="MandatoryCOM" select="$MandatoryCOM"/>
        <xsl:with-param name="Type" select="$Type"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name="MEASegment">
    <xsl:param name="Code"/>
    <xsl:param name="Unit"/>
    <xsl:param name="Value"/>
    <xsl:if test="$Unit!='' and number($Value)=number($Value) and number($Value)>0 and string($Value)!='0' and string($Value)!=''">
      <ns0:MEA>
        <ns0:TAGNAME>MEA</ns0:TAGNAME>
        <ns0:MEA01>AAE</ns0:MEA01>
        <ns0:C502>
          <ns0:C50201>
            <xsl:value-of select="$Code"/>
          </ns0:C50201>
        </ns0:C502>
        <ns0:C174>
          <ns0:C17401>
            <xsl:choose>
              <xsl:when test="$Unit='CC'">CMQ</xsl:when>
              <xsl:when test="$Unit='CF'">FTQ</xsl:when>
              <xsl:when test="$Unit='CI'">INQ</xsl:when>
              <xsl:when test="$Unit='CY'">TDQ</xsl:when>
              <xsl:when test="$Unit='D3'">DMA</xsl:when>
              <xsl:when test="$Unit='DT'">DTN</xsl:when>
              <xsl:when test="$Unit='G'">GRM</xsl:when>
              <xsl:when test="$Unit='GA'">GLL</xsl:when>
              <xsl:when test="$Unit='GI'">GLI</xsl:when>
              <xsl:when test="$Unit='HG'">HGM</xsl:when>
              <xsl:when test="$Unit='KG'">KGM</xsl:when>
              <xsl:when test="$Unit='KT'">KTN</xsl:when>
              <xsl:when test="$Unit='L'">LTR</xsl:when>
              <xsl:when test="$Unit='LB'">LBR</xsl:when>
              <xsl:when test="$Unit='LT'">LBT</xsl:when>
              <xsl:when test="$Unit='M3'">MTQ</xsl:when>
              <xsl:when test="$Unit='MC'">CTM</xsl:when>
              <xsl:when test="$Unit='MG'">MGM</xsl:when>
              <xsl:when test="$Unit='ML'">MMQ</xsl:when>
              <xsl:when test="$Unit='OT'">APZ</xsl:when>
              <xsl:when test="$Unit='OZ'">ONZ</xsl:when>
              <xsl:when test="$Unit='T'">TNE</xsl:when>
              <xsl:when test="$Unit='TL'">LTN</xsl:when>
              <xsl:when test="$Unit='TN'">STN</xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="substring($Unit, 1, 3)"/>
              </xsl:otherwise>
            </xsl:choose>
          </ns0:C17401>
          <ns0:C17402>
            <xsl:value-of select="format-number($Value, '0.####') "/>
          </ns0:C17402>
        </ns0:C174>
      </ns0:MEA>
    </xsl:if>
  </xsl:template>
    
  <xsl:template name="HCCategory">
    <xsl:param name="Enable"/>
    <xsl:param name="Qualifier"/>
    <xsl:param name="Code"/>
    <xsl:if test="$Enable">
      <ns0:PGI_1>
        <ns0:TAGNAME>PGI</ns0:TAGNAME>
        <ns0:PGI01>11</ns0:PGI01>
        <ns0:C288_1>
          <ns0:C28801>
            <xsl:value-of select="$Qualifier"/>
          </ns0:C28801>
          <ns0:C28804>
            <xsl:value-of select="$Code"/>
          </ns0:C28804>
        </ns0:C288_1>
      </ns0:PGI_1>
    </xsl:if>
  </xsl:template>

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[
  System.Collections.Generic.List<string> uniqueBillList = new System.Collections.Generic.List<string>();
  int sg117count = 0;
  int sg101count = 0;
  int moaCount = 0;
  int sg121count = 0;
  int sg9count = 0;
  int decCount = 0;

  bool invoiceLinkedPackingLineFlag;
  string invoiceLinkedPackingIDs;
  System.Collections.Generic.Dictionary<string, string> invoiceLineLinkedPackingIDs = new System.Collections.Generic.Dictionary<string, string>();
  System.Collections.Generic.List<string> linkedIDs = new System.Collections.Generic.List<string>();
  System.Collections.Generic.Dictionary<string, string> endingPackingLineRelationships = new System.Collections.Generic.Dictionary<string, string>();
    struct PacklineDetail
    {
      public int QtyNumber;
      public string PackTypeCode;
      public string LevelNum;
      public string MarksAndNos;
      public bool IsInvoiceLoop;
    }
  System.Collections.Generic.List<PacklineDetail> packlineDetailList = new System.Collections.Generic.List<PacklineDetail>();

  public int GetDecCount()
  {
    return decCount;
  }
  
  public void IncrementDecCount()
  {
    decCount++;
  }

  public string GenerateMessageRefNumber(string currentDateTimeUTC, int count)
  {
     var result = currentDateTimeUTC;
     result += count.ToString().PadLeft(6, '0');
     return result;
  }

  public void IncrementMOACount() { moaCount++; }

  public void ResetMOACount() { moaCount = 0; }

  public int MOACount() { return moaCount; }

  public void ResetSG121Count() { sg121count = 0; }

  public int SG121Count() { return ++sg121count; }

  public void ResetSG9Count() { sg9count = 0; }

  public int SG9Count() { return ++sg9count; }

  public void ResetSG117Count() { sg117count = 0; }

  public int SG117Count() { return ++sg117count; }

  public int SG101Count() { return ++sg101count; }

  public void ResetInvoiceLinkedPackingLineFlag() { invoiceLinkedPackingLineFlag = false; }

  public void SetInvoiceLinkedPackingLineFlag() { invoiceLinkedPackingLineFlag = true; }

  public bool GetInvoiceLinkedPackingLineFlag() { return invoiceLinkedPackingLineFlag; }

  public void ResetInvoiceLinkedPackingIDs() { invoiceLinkedPackingIDs = ""; }

  public void SetInvoiceLinkedPackingIDs(string ids) { invoiceLinkedPackingIDs = ids; }

  public string GetInvoiceLinkedPackingIDs() { return invoiceLinkedPackingIDs; }

  public void ResetInvoiceLineLinkedPackingIDs() { invoiceLineLinkedPackingIDs.Clear(); }

  public void AddInvoiceLineLinkedPackingIDs(string commercialInvoiceLineID, string ids) { invoiceLineLinkedPackingIDs.Add(commercialInvoiceLineID, ids); }

  public string GetInvoiceLineLinkedPackingIDs(string commercialInvoiceLineID) { return invoiceLineLinkedPackingIDs[commercialInvoiceLineID].Trim(); }

  public string GetAllInvoiceLineIDs(bool onlyThoseWithPackingLines = false)
  {
    string result = "";
    System.Collections.Generic.List<string> ids = new System.Collections.Generic.List<string>(invoiceLineLinkedPackingIDs.Keys);
    for(int i = 0; i < ids.Count; i++)
    {
      if(invoiceLineLinkedPackingIDs[ids[i]].Trim() != "" || !onlyThoseWithPackingLines)
      {
        result += " " + ids[i];
      }
    }
    return result.Trim();
  }

  public void ResetLinkedIDs() { linkedIDs.Clear(); }

  public void RegisterLinkedID(string ID) { linkedIDs.Add(ID); }

  public string GetAllLinkedIDs() { return string.Join(" ", linkedIDs); }
  
  public void ResetEndingPackingLineRelationships() { endingPackingLineRelationships.Clear(); }

  public void AddEndingPackingLineRelationships(string currID, string parentIDs) { endingPackingLineRelationships.Add(currID, parentIDs); }

  public string GetAllEndingPackingLineIDs(string currID)
  {
    string result = "";
    foreach(System.Collections.Generic.KeyValuePair<string,string> elem in endingPackingLineRelationships)
    {
      if(elem.Value.Contains(currID))
      {
        result += " " + elem.Key;
      }
    }
    return result.Trim();
  }

  public void AddPackingLineDetailsCollection(int QtyNumber, string PackTypeCode, string LevelNum, string MarksAndNos, bool IsInvoiceLoop)
  {
    int tempQtyNumber = QtyNumber;
    bool flag = false;
    if (MarksAndNos == "")
    {
      foreach (PacklineDetail packlineDetail in packlineDetailList)
      {
        if (packlineDetail.IsInvoiceLoop == IsInvoiceLoop && packlineDetail.LevelNum == LevelNum && packlineDetail.PackTypeCode == PackTypeCode)
        {
          tempQtyNumber +=  packlineDetail.QtyNumber;
          flag = true;
          break;
        }
      }
    }
     
    if (flag)
    {
      packlineDetailList.RemoveAll(x => x.IsInvoiceLoop == IsInvoiceLoop && x.LevelNum == LevelNum && x.PackTypeCode == PackTypeCode); 
    }
    
    PacklineDetail newPacklineDetail = new PacklineDetail();
    newPacklineDetail.QtyNumber = tempQtyNumber;
    newPacklineDetail.PackTypeCode = PackTypeCode;
    newPacklineDetail.LevelNum = LevelNum;
    newPacklineDetail.MarksAndNos = MarksAndNos;
    newPacklineDetail.IsInvoiceLoop = IsInvoiceLoop;
    packlineDetailList.Add(newPacklineDetail);
  }

  public int GetPackLineCollectionCount()
  {
    return packlineDetailList.Count;
  }

  public string GetPackLineQtyNumber(int i)
  {
    return packlineDetailList[i].QtyNumber.ToString();
  }
  
  public string GetPackLineLevel(int i)
  {
    return packlineDetailList[i].LevelNum;
  }

  public string GetPackLinePackTypeCode(int i)
  {
    return packlineDetailList[i].PackTypeCode;
  }
  
  public string GetPackLineMarksAndNos(int i)
  {
    return packlineDetailList[i].MarksAndNos;
  }

  public XPathNodeIterator ConstructPackingLineContent()
  {  
      var doc = new XmlDocument();
      var root = doc.CreateElement("PackingLineCollection");

      foreach(PacklineDetail packlineDetail in packlineDetailList)
      {
        var line = doc.CreateElement("PackingLine");
        
        var lineChild = doc.CreateElement("QtyNumber");
        lineChild.InnerText = packlineDetail.QtyNumber.ToString();
        line.AppendChild(lineChild);
        
        lineChild = doc.CreateElement("LevelNum");
        lineChild.InnerText = packlineDetail.LevelNum;
        line.AppendChild(lineChild);
        
        lineChild = doc.CreateElement("PackTypeCode");
        lineChild.InnerText = packlineDetail.PackTypeCode;
        line.AppendChild(lineChild);
        
        lineChild = doc.CreateElement("MarksAndNos");
        lineChild.InnerText = packlineDetail.MarksAndNos;
        line.AppendChild(lineChild);
        
        root.AppendChild(line);
      }
      doc.AppendChild(root);

      return doc.CreateNavigator().Select("/*");
  }
  
  public void ResetPackingLines() { packlineDetailList.Clear(); }

  public string StringDecimalMaxAllowed(string text, int maxLength, int scale)
  {
    try
    {
      decimal value = decimal.Round(Convert.ToDecimal(text), scale);
      decimal maximumLimit = decimal.Parse("".PadLeft(maxLength - 1 - scale, '9') + "." + "".PadLeft(scale, '9'));
      return value > maximumLimit && scale > 0 ? StringDecimalMaxAllowed(text, maxLength, scale-1) : value.ToString("0." + "".PadLeft(scale,'#'));
    }
    catch
    {
      return "";
    }
  }

  public XPathNodeIterator KeyValueSplit(string to_split)
  {
    var doc = new XmlDocument();
    var root = doc.CreateElement("root");
    doc.AppendChild(root);

    foreach (var split in to_split.Split('|'))
    {
      var key_value = split.Split('=');
      if (key_value.Length == 2)
      {
        var child = doc.CreateElement(key_value[0]);
        child.InnerText = key_value[1];
        root.AppendChild(child);
      }
    }

    return doc.CreateNavigator().Select("/*");
  }

  public string EscapeString(string input)
  {
    var result = "";
    foreach(var ch in input.ToCharArray())
    {
      var ascii = (int)ch;
      if ( !(ascii == 33 || ascii == 124 || ascii > 126 || ascii < 32 ))
      {
        result += ch;
      }
    }
    return result;
  }
  
  public string ReturnEmptyIfHasInvalidCharacters(string input)
  {
    foreach(var c in input.ToCharArray())
    {
      var ascii = (int)c;
      if (ascii < 32 || ascii == 33 || ascii == 124 || ascii > 126)
      {
        return "";
      }
    }
    return input;
  }
]]>
  </msxsl:script>
</xsl:stylesheet>

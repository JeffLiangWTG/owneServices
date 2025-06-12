<?xml version="1.0" encoding="UTF-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var"
                exclude-result-prefixes="msxsl var s0 s1 ScriptNS0 ScriptNS1 ScriptNS2 ScriptNS3 ScriptNS4" version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2012/11"
                xmlns:s1="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:ns0="http://wisetechglobal.com/ForwardingPortMessaging/TNPA/2016/12"
                xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
                xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1"
                xmlns:ScriptNS2="http://schemas.microsoft.com/BizTalk/2003/ScriptNS2"
                xmlns:ScriptNS3="http://schemas.microsoft.com/BizTalk/2003/ScriptNS3"
                xmlns:ScriptNS4="http://schemas.microsoft.com/BizTalk/2003/ScriptNS4">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />
  <xsl:template match="/">
    <xsl:apply-templates select="/s1:UniversalInterchange/s1:Body/s0:UniversalShipment/s0:Shipment" />
  </xsl:template>

  <xsl:variable name="SenderID" select="ScriptNS4:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>

  <xsl:template match="/s1:UniversalInterchange/s1:Body/s0:UniversalShipment/s0:Shipment">

    <xsl:variable name="consolID" select="s0:DataContext/s0:DataSource/s0:Key/text()"/>
    <xsl:variable name="forwardingType" select="s0:DataContext/s0:DataSource/s0:Type/text()"/>
    <xsl:variable name="interchangeNumber" select="ScriptNS0:CallActionProcedureHelper('GetCounterValue','@value','@name','CargoWise.eHub.Products.ForwardingPortMessaging.TNPA.Transforms.UShipment2COPRAR.BGM','@maxlength','14')" />

    <xsl:variable name="formattedInterchangeNum" select='format-number($interchangeNumber, "00000000000000")' />
    <xsl:variable name="documentName" select="s0:DataContext/s0:DocumentaryOverride/s0:DocumentName/text()"/>
    <xsl:variable name="purpose" select="s0:DataContext/s0:DocumentaryOverride/s0:Purpose/text()"/>
    <xsl:variable name="dataVersion" select="s0:DataContext/s0:DocumentaryOverride/s0:DataVersion/text()" />

    <xsl:variable name="TNPAID" select="ScriptNS3:GetClientRegistrationCode($SenderID , s0:DataContext/s0:Workflow/s0:EventBranch/text(), 'TNPA')"/>
    <xsl:variable name="subscribeTNPAID" select="ScriptNS3:InsertSubscriptionValue('NPAID', 'TNPA', $SenderID, $TNPAID)" />
    <xsl:variable name="subscribeJobNumber1" select="ScriptNS3:InsertSubscriptionValue('NPAMSG', 'TNPA', $SenderID, $interchangeNumber, $consolID, 'JobNumber')" />
    <xsl:variable name="subscribeJobNumber2" select="ScriptNS3:InsertSubscriptionValue('NPAMSG', 'TNPA', $SenderID, $consolID, $interchangeNumber, $interchangeNumber)" />
    <xsl:variable name="subscribeDocumentName" select="ScriptNS3:InsertSubscriptionValue('NPAMSG', 'TNPA', $SenderID, $interchangeNumber, $documentName, 'DocumentName')" />
    <xsl:variable name="subscribeForwardingType" select="ScriptNS3:InsertSubscriptionValue('NPAMSG', 'TNPA', $SenderID, $interchangeNumber, $forwardingType, 'ForwardingType')" />
    <xsl:variable name="subscribeActionPurpose" select="ScriptNS3:InsertSubscriptionValue('NPAMSG', 'TNPA', $SenderID, $interchangeNumber, $purpose, 'Purpose')" />

    <xsl:if test="$purpose='ORG'">
      <xsl:variable name="subscribeBGM" select="ScriptNS3:InsertSubscriptionValue('NPAMSG', 'TNPA', $SenderID, concat($consolID, '_', $documentName), $interchangeNumber, 'BGM')" />
    </xsl:if>

    <xsl:variable name="overrideEDIHeader" select="ScriptNS4:SetContextProperty('OverrideEDIHeader', 'http://schemas.microsoft.com/BizTalk/2006/edi-properties', 'true')" />
    <xsl:variable name="unb2_1" select="ScriptNS4:SetContextProperty('UNB2_1', 'http://schemas.microsoft.com/BizTalk/2006/edi-properties', $TNPAID)" />
    <xsl:variable name="unb5" select="ScriptNS4:SetContextProperty('UNB5', 'http://schemas.microsoft.com/BizTalk/2006/edi-properties', $formattedInterchangeNum)" />
    <xsl:variable name="unh1" select="ScriptNS4:SetContextProperty('UNH1', 'http://schemas.microsoft.com/BizTalk/2006/edi-properties', $formattedInterchangeNum)" />
    <xsl:variable name="unb7" select="ScriptNS4:SetContextProperty('UNB7', 'http://schemas.microsoft.com/BizTalk/2006/edi-properties', 'COPRAR')" />
    <xsl:variable name="unb10" select="ScriptNS4:SetContextProperty('UNB10', 'http://schemas.microsoft.com/BizTalk/2006/edi-properties', 'NPAIG1.0')" />

    <xsl:variable name="unb9" select="ScriptNS4:SetContextProperty('UNB9', 'http://schemas.microsoft.com/BizTalk/2006/edi-properties', '')" />
    <xsl:variable name="unb11" select="ScriptNS4:SetContextProperty('UNB11', 'http://schemas.microsoft.com/BizTalk/2006/edi-properties', '')" />
    <xsl:variable name="earlyTerminateEdifactUnb" select="ScriptNS4:SetContextProperty('EarlyTerminateEdifactUnb', 'http://cargowise.com/ehub/processing/2010/06', 'true')"/>

    <xsl:variable name="una6" select='ScriptNS4:SetContextProperty("UNA6", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "&apos;")' />
    <xsl:variable name="una6Suffix" select='ScriptNS4:SetContextProperty("UNA6Suffix", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "&#13;&#10;")' />

    <xsl:variable name="bgmCode">
      <xsl:call-template name="DocumentNameSwitchCaseReturn">
        <xsl:with-param name="documentName" select="$documentName"/>
        <xsl:with-param name="consol" select="$consolID"/>
        <xsl:with-param name="returnType" select="'CODE'"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="direction">
      <xsl:choose>
        <xsl:when test="contains($documentName, 'Import')">Import</xsl:when>
        <xsl:when test="contains($documentName, 'Export')">Export</xsl:when>
        <xsl:when test="contains($documentName, 'Load Coastwise')">Load Coastwise</xsl:when>
        <xsl:when test="contains($documentName, 'Discharge Coastwise')">Discharge Coastwise</xsl:when>
        <xsl:otherwise></xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:if test="$direction!=''">
      <xsl:variable name="subscribeDirection" select="ScriptNS3:InsertSubscriptionValue('NPAMSG', 'TNPA', $SenderID, $interchangeNumber, $direction, 'Direction')" />
    </xsl:if>

    <xsl:variable name="ZATransportLegOrder">
      <xsl:choose>
        <xsl:when test="starts-with($documentName, 'Cargo Dues - Export') or starts-with($documentName, 'Cargo Dues - Load Coastwise')">
          <xsl:for-each select="s0:TransportLegCollection/s0:TransportLeg[s0:TransportMode/text()='Sea' and starts-with(s0:PortOfLoading/text(), 'ZA')]/s0:LegOrder/text()">
            <xsl:sort select="." data-type="number" order="ascending"/>
            <xsl:if test="position() = 1">
              <xsl:value-of select="."/>
            </xsl:if>
          </xsl:for-each>
        </xsl:when>
        <xsl:otherwise>
          <xsl:for-each select="s0:TransportLegCollection/s0:TransportLeg[s0:TransportMode/text()='Sea' and starts-with(s0:PortOfDischarge/text(), 'ZA')]/s0:LegOrder/text()">
            <xsl:sort select="." data-type="number" order="descending"/>
            <xsl:if test="position() = 1">
              <xsl:value-of select="."/>
            </xsl:if>
          </xsl:for-each>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="ZATransportLeg" select="s0:TransportLegCollection/s0:TransportLeg[s0:LegOrder/text()=number($ZATransportLegOrder)]"/>

    <xsl:variable name="ZAPort">
      <xsl:choose>
        <xsl:when test="$bgmCode='45'">
          <xsl:value-of select="$ZATransportLeg/s0:PortOfLoading/text()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$ZATransportLeg/s0:PortOfDischarge/text()"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:if test="$ZAPort!=''">
      <xsl:variable name="subscribePort" select="ScriptNS3:InsertSubscriptionValue('NPAMSG', 'TNPA', $SenderID, $interchangeNumber, $ZAPort, 'Port')" />
    </xsl:if>

    <ns0:EFACT_D95B_COPRAR>
      <UNH>
        <UNH1>
          <xsl:value-of select="$formattedInterchangeNum"/>
        </UNH1>
        <UNH2>
          <UNH2.1>COPRAR</UNH2.1>
          <UNH2.2>D</UNH2.2>
          <UNH2.3>95B</UNH2.3>
          <UNH2.4>UN</UNH2.4>
          <UNH2.5>ITG12</UNH2.5>
        </UNH2>
      </UNH>
      <ns0:BGM>
        <ns0:C002>
          <C00201>
            <xsl:value-of select="$bgmCode"/>
          </C00201>
          <C00204>COPRAR</C00204>
        </ns0:C002>
        <BGM02>
          <xsl:variable name="bgmValue" select ="ScriptNS0:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', 'TNPA', '@recipientId', $SenderID, '@ST_ID', 'NPAMSG', '@value',  concat($consolID, '_', $documentName), '@referenceType', 'BGM')" />
          <xsl:choose>
            <xsl:when test="($purpose='AMD' or $purpose='WTH') and $bgmValue!=''">
              <xsl:value-of select="$bgmValue"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$interchangeNumber"/>
            </xsl:otherwise>
          </xsl:choose>
        </BGM02>
        <BGM03>
          <xsl:choose>
            <xsl:when test="$purpose='WTH'">1</xsl:when>
            <xsl:when test="number($dataVersion)!=1">5</xsl:when>
            <xsl:otherwise>9</xsl:otherwise>
          </xsl:choose>
        </BGM03>
      </ns0:BGM>
      <ns0:FTX>
        <FTX01>
          <xsl:choose>
            <xsl:when test="contains($documentName, 'Quotation')">QUT</xsl:when>
            <xsl:otherwise>COI</xsl:otherwise>
          </xsl:choose>
        </FTX01>
        <ns0:C107>
          <C10701>
            <xsl:choose>
              <xsl:when test="contains($documentName, 'Coastwise')">CST</xsl:when>
              <xsl:otherwise>DPS</xsl:otherwise>
            </xsl:choose>
          </C10701>
          <C10702>128</C10702>
          <C10703>ZZZ</C10703>
        </ns0:C107>
      </ns0:FTX>
      <xsl:variable name="reasonForMessageCancellationNote" select="s0:NoteCollection/s0:Note[s0:Description/text()='ReasonForMessageCancellation']/s0:NoteText/text()"/>
      <xsl:if test="$purpose='WTH' and $reasonForMessageCancellationNote!=''">
        <ns0:FTX>
          <FTX01>ACD</FTX01>
          <ns0:C108>
            <C10801>
              <xsl:value-of select="$reasonForMessageCancellationNote"/>
            </C10801>
          </ns0:C108>
        </ns0:FTX>
      </xsl:if>
      <xsl:call-template name="RFF">
        <xsl:with-param name="code" select="'CR'"/>
        <xsl:with-param name="value" select="s0:DataContext/s0:DataSource/s0:Key/text()"/>
      </xsl:call-template>

      <xsl:call-template name="RFF">
        <xsl:with-param name="code" select="'ADE'"/>
        <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TNPAAccountNumber']/s0:Value/text()"/>
        <xsl:with-param name="fallbackValue" select="s0:DocumentaryOverride/s0:TNPAAccountNumber/text()"/>
      </xsl:call-template>

      <xsl:call-template name="RFF">
        <xsl:with-param name="code" select="'BM'"/>
        <xsl:with-param name="value" select="s0:WayBillNumber/text()"/>
      </xsl:call-template>

      <xsl:if test="number($dataVersion)!=1">
        <xsl:call-template name="RFF">
          <xsl:with-param name="code" select="'ACW'"/>
          <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TNPAOrderNumber']/s0:Value/text()"/>
          <xsl:with-param name="fallbackValue" select="s0:DocumentaryOverride/s0:TNPAOrderNumber/text()"/>
        </xsl:call-template>
      </xsl:if>

      <xsl:if test="number($dataVersion)=1">
        <xsl:call-template name="RFF">
          <xsl:with-param name="code" select="'QUT'"/>
          <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TNPAQuotationNumber']/s0:Value/text()"/>
          <xsl:with-param name="fallbackValue" select="s0:DocumentaryOverride/s0:TNPAQuotationNumber/text()"/>
        </xsl:call-template>
      </xsl:if>

      <xsl:variable name="shippingLineAddress" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='ShippingLineAddress']" />
      <xsl:variable name="tnpCode" select="$shippingLineAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='TNP' and s0:CountryOfIssue/text()='ZA']/s0:Value/text()"/>
      <xsl:variable name="carrierSCACCode">
        <xsl:choose>
          <xsl:when test="$tnpCode!=''">
            <xsl:value-of select="$tnpCode"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$shippingLineAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='CCC' and s0:CountryOfIssue/text()='ZA']/s0:Value/text()"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:call-template name="TDTLoop1">
        <xsl:with-param name="bgmCode" select="$bgmCode"/>
        <xsl:with-param name="carrierSCACCode" select="$carrierSCACCode"/>
        <xsl:with-param name="transportLeg" select="$ZATransportLeg"/>
        <xsl:with-param name="radioCallSign" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='RadioCallSign']/s0:Value/text()"/>
      </xsl:call-template>

      <xsl:if test="$carrierSCACCode!=''">
        <ns0:NADLoop1>
          <ns0:NAD>
            <NAD01>CF</NAD01>
            <ns0:C082>
              <C08201>
                <xsl:value-of select="$carrierSCACCode" />
              </C08201>
              <C08202>172</C08202>
              <C08203>87</C08203>
            </ns0:C082>
          </ns0:NAD>
        </ns0:NADLoop1>
      </xsl:if>
      <xsl:variable name="currentUser" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='CurrentUser']"/>
      <xsl:variable name="currentUserContact" select="$currentUser/s0:Contact/text()"/>
      <xsl:if test="$currentUserContact!=''">
        <ns0:NADLoop1>
          <ns0:NAD>
            <NAD01>OB</NAD01>
            <ns0:C080>
              <C08001>
                <xsl:value-of select="$currentUserContact"/>
              </C08001>
            </ns0:C080>
          </ns0:NAD>
          <xsl:call-template name="CTA">
            <xsl:with-param name="code" select="'EM'"/>
            <xsl:with-param name="value" select="$currentUser/s0:Email/text()"/>
          </xsl:call-template>
          <xsl:call-template name="CTA">
            <xsl:with-param name="code" select="'TE'"/>
            <xsl:with-param name="value" select="$currentUser/s0:Phone/text()"/>
          </xsl:call-template>
          <xsl:call-template name="CTA">
            <xsl:with-param name="code" select="'FX'"/>
            <xsl:with-param name="value" select="$currentUser/s0:Fax/text()"/>
          </xsl:call-template>
        </ns0:NADLoop1>
      </xsl:if>

      <xsl:variable name="containerMode" select="s0:ContainerMode/text()"/>
      <xsl:variable name="SubShipment" select="s0:SubShipmentCollection/s0:SubShipment"/>
      <xsl:variable name="PackingLine" select="$SubShipment/s0:PackingLineCollection/s0:PackingLine"/>
      <xsl:variable name="TransportLegCollection" select="s0:TransportLegCollection"/>
      <xsl:variable name="shipmentPlaceOfReceipt" select ="s0:PlaceOfReceipt/text()" />
      <xsl:variable name="shipmentPlaceOfDelivery" select ="s0:PlaceOfDelivery/text()" />
      <xsl:variable name="EQD01">
        <xsl:choose>
          <xsl:when test="$containerMode='BBK' or $containerMode='LQD' or $containerMode='ROR' or $containerMode='OTH'">BB</xsl:when>
          <xsl:when test="$containerMode='BLK'">B</xsl:when>
          <xsl:otherwise>CN</xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="legOrderMax">
        <!-- find max -->
        <xsl:for-each select="$TransportLegCollection/s0:TransportLeg[s0:TransportMode/text()='Sea' and number(s0:LegOrder/text()) &lt; number($ZATransportLeg/s0:LegOrder/text())]/s0:LegOrder">
          <xsl:sort select="." data-type="number" order="descending"/>
          <xsl:if test="position() = 1">
            <xsl:value-of select="."/>
          </xsl:if>
        </xsl:for-each>
      </xsl:variable>
      <xsl:variable name="lastSeaTransportLegBeforeHeader" select="$TransportLegCollection/s0:TransportLeg[s0:LegOrder/text()=$legOrderMax]"/>
      <xsl:variable name="legOrderMin">
        <!-- find min -->
        <xsl:for-each select="$TransportLegCollection/s0:TransportLeg[s0:TransportMode/text()='Sea' and number(s0:LegOrder/text()) &gt; number($ZATransportLeg/s0:LegOrder/text())]/s0:LegOrder">
          <xsl:sort select="." data-type="number" order="ascending"/>
          <xsl:if test="position() = 1">
            <xsl:value-of select="."/>
          </xsl:if>
        </xsl:for-each>
      </xsl:variable>
      <xsl:variable name="firstSeaTransportLegAfterHeader" select="$TransportLegCollection/s0:TransportLeg[s0:LegOrder/text()=$legOrderMin]"/>
      <xsl:variable name="firstSeaTransportLegLegOrder">
        <xsl:for-each select="$TransportLegCollection/s0:TransportLeg[s0:TransportMode/text()='Sea']/s0:LegOrder">
          <xsl:sort select="." data-type="number" order="ascending"/>
          <xsl:if test="position()=1">
            <xsl:value-of select="."/>
          </xsl:if>
        </xsl:for-each>
      </xsl:variable>
      <xsl:variable name="firstSeaTransportLeg" select="s0:TransportLegCollection/s0:TransportLeg[s0:LegOrder/text()=number($firstSeaTransportLegLegOrder)]"/>
      <xsl:variable name="lastSeaTransportLegLegOrder">
        <xsl:for-each select="$TransportLegCollection/s0:TransportLeg[s0:TransportMode/text()='Sea']/s0:LegOrder">
          <xsl:sort select="." data-type="number" order="descending"/>
          <xsl:if test="position()=1">
            <xsl:value-of select="."/>
          </xsl:if>
        </xsl:for-each>
      </xsl:variable>
      <xsl:variable name="lastSeaTransportLeg" select="s0:TransportLegCollection/s0:TransportLeg[s0:LegOrder/text()=number($lastSeaTransportLegLegOrder)]"/>

      <xsl:choose>
        <xsl:when test ="$EQD01='CN'">
          <xsl:for-each select="s0:ContainerCollection/s0:Container">
            <xsl:variable name="link" select="s0:Link/text()" />
            <ns0:EQDLoop1>
              <ns0:EQD>
                <EQD01>
                  <xsl:value-of select="$EQD01"/>
                </EQD01>
                <ns0:C237>
                  <C23701>
                    <xsl:value-of select="s0:ContainerNumber/text()"/>
                  </C23701>
                </ns0:C237>
                <ns0:C224>
                  <C22401>
                    <xsl:value-of select="s0:ContainerType/s0:ISOCode/text()"/>
                  </C22401>
                  <C22402>102</C22402>
                  <C22403>5</C22403>
                </ns0:C224>
                <EQD05>
                  <xsl:choose>
                    <xsl:when test="starts-with($shipmentPlaceOfReceipt,'ZA')">2</xsl:when>
                    <xsl:when test="starts-with($shipmentPlaceOfDelivery,'ZA')">3</xsl:when>
                    <xsl:otherwise>6</xsl:otherwise>
                  </xsl:choose>
                </EQD05>
                <EQD06>
                  <xsl:choose>
                    <xsl:when test="s0:IsEmptyContainer/text() = 'true'">4</xsl:when>
                    <xsl:otherwise>5</xsl:otherwise>
                  </xsl:choose>
                </EQD06>
              </ns0:EQD>
              <xsl:call-template name="LOC">
                <xsl:with-param name="code" select="27"/>
                <xsl:with-param name="ref" select="substring($shipmentPlaceOfReceipt,1,2)"/>
                <xsl:with-param name="suffix" select="'_2'"/>
              </xsl:call-template>
              <xsl:call-template name="LOC">
                <xsl:with-param name="code" select="28"/>
                <xsl:with-param name="ref" select="substring($shipmentPlaceOfDelivery,1,2)"/>
                <xsl:with-param name="suffix" select="'_2'"/>
              </xsl:call-template>
              <xsl:call-template name="LOC">
                <xsl:with-param name="code" select="9"/>
                <xsl:with-param name="ref" select="$firstSeaTransportLeg/s0:PortOfLoading/text()"/>
                <xsl:with-param name="suffix" select="'_2'"/>
              </xsl:call-template>
              <xsl:call-template name="LOC">
                <xsl:with-param name="code" select="11"/>
                <xsl:with-param name="ref" select="$lastSeaTransportLeg/s0:PortOfDischarge/text()"/>
                <xsl:with-param name="suffix" select="'_2'"/>
              </xsl:call-template>
              <xsl:if test="$carrierSCACCode!=''">
                <ns0:NAD_2>
                  <NAD01>CF</NAD01>
                  <ns0:C082_2>
                    <C08201>
                      <xsl:value-of select="$carrierSCACCode"/>
                    </C08201>
                    <C08202>160</C08202>
                    <C08203>20</C08203>
                  </ns0:C082_2>
                </ns0:NAD_2>
              </xsl:if>

              <xsl:choose>
                <xsl:when test="contains($documentName, 'Import') or contains($documentName, 'Discharge Coastwise')">
                  <xsl:if test="starts-with($lastSeaTransportLegBeforeHeader/s0:PortOfDischarge/text(),'ZA')">
                    <xsl:call-template name="TDTLoop2">
                      <xsl:with-param name="TransportLeg" select="$lastSeaTransportLegBeforeHeader" />
                      <xsl:with-param name="code" select="10"/>
                    </xsl:call-template>
                  </xsl:if>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:if test="starts-with($firstSeaTransportLegAfterHeader/s0:PortOfLoading/text(),'ZA')">
                    <xsl:call-template name="TDTLoop2">
                      <xsl:with-param name="TransportLeg" select="$firstSeaTransportLegAfterHeader" />
                      <xsl:with-param name="code" select="30"/>
                    </xsl:call-template>
                  </xsl:if>
                </xsl:otherwise>
              </xsl:choose>
            </ns0:EQDLoop1>
          </xsl:for-each>

          <xsl:call-template name="CNT">
            <xsl:with-param name="cntCount" select="count(s0:ContainerCollection/s0:Container)"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:for-each select="s0:SubShipmentCollection/s0:SubShipment">
            <ns0:EQDLoop1>
              <ns0:EQD>
                <EQD01>
                  <xsl:value-of select="$EQD01"/>
                </EQD01>
                <EQD05>
                  <xsl:choose>
                    <xsl:when test="starts-with($shipmentPlaceOfReceipt,'ZA')">2</xsl:when>
                    <xsl:when test="starts-with($shipmentPlaceOfDelivery,'ZA')">3</xsl:when>
                    <xsl:otherwise>6</xsl:otherwise>
                  </xsl:choose>
                </EQD05>
              </ns0:EQD>
              <xsl:call-template name="LOC">
                <xsl:with-param name="code" select="27"/>
                <xsl:with-param name="ref" select="substring($shipmentPlaceOfReceipt,1,2)"/>
                <xsl:with-param name="suffix" select="'_2'"/>
              </xsl:call-template>

              <xsl:call-template name="LOC">
                <xsl:with-param name="code" select="28"/>
                <xsl:with-param name="ref" select="substring($shipmentPlaceOfDelivery,1,2)"/>
                <xsl:with-param name="suffix" select="'_2'"/>
              </xsl:call-template>

              <xsl:call-template name="LOC">
                <xsl:with-param name="code" select="9"/>
                <xsl:with-param name="ref" select="$firstSeaTransportLeg/s0:PortOfLoading/text()"/>
                <xsl:with-param name="suffix" select="'_2'"/>
              </xsl:call-template>

              <xsl:call-template name="LOC">
                <xsl:with-param name="code" select="11"/>
                <xsl:with-param name="ref" select="$lastSeaTransportLeg/s0:PortOfDischarge/text()"/>
                <xsl:with-param name="suffix" select="'_2'"/>
              </xsl:call-template>

              <ns0:MEA>
                <MEA01>AAE</MEA01>
                <ns0:C502>
                  <C50201>G</C50201>
                </ns0:C502>
                <ns0:C174>
                  <C17401>KGM</C17401>
                  <C17402>
                    <xsl:value-of select="ScriptNS1:Convert(s0:TotalWeight/text(), s0:TotalWeightUnit/text(), 'KG')" />
                  </C17402>
                </ns0:C174>
              </ns0:MEA>

              <xsl:for-each select="s0:PackingLineCollection/s0:PackingLine">
                <xsl:variable name="goods" select="../../s0:GoodsDescription/text()" />
                <xsl:if test="s0:DetailedDescription/text()!='' or s0:GoodsDescription/text()!='' or $goods != ''" >
                  <xsl:if test="position() &lt;= 7">
                    <ns0:FTX_3>
                      <FTX01>
                        <xsl:text>AAA</xsl:text>
                      </FTX01>
                      <ns0:C108_3>
                        <xsl:call-template name="C10801Loop">
                          <xsl:with-param name="counter" select="1"/>
                          <xsl:with-param name="freetext">
                            <xsl:choose>
                              <xsl:when test="s0:DetailedDescription/text()!=''">
                                <xsl:value-of  select="s0:DetailedDescription/text()"/>
                              </xsl:when>
                              <xsl:when test="s0:GoodsDescription/text()!=''">
                                <xsl:value-of select="s0:GoodsDescription/text()"/>
                              </xsl:when>
                              <xsl:otherwise>
                                <xsl:value-of select="$goods"/>
                              </xsl:otherwise>
                            </xsl:choose>
                          </xsl:with-param>
                        </xsl:call-template>
                      </ns0:C108_3>
                    </ns0:FTX_3>
                  </xsl:if>
                </xsl:if>
              </xsl:for-each>
              <ns0:FTX_3>
                <FTX01>
                  <xsl:text>AAI</xsl:text>
                </FTX01>
                <ns0:C108_3>
                  <C10801>
                    <xsl:choose>
                      <xsl:when test="$containerMode='BBK'">4</xsl:when>
                      <xsl:when test="$containerMode='BLK'">2</xsl:when>
                      <xsl:when test="$containerMode='LQD'">3</xsl:when>
                      <xsl:when test="$containerMode='ROR'">5</xsl:when>
                      <xsl:otherwise>1</xsl:otherwise>
                    </xsl:choose>
                  </C10801>
                </ns0:C108_3>
              </ns0:FTX_3>
              <ns0:FTX_3>
                <FTX01>
                  <xsl:text>COI</xsl:text>
                </FTX01>
                <ns0:C108_3>
                  <C10801>
                    <xsl:value-of select="concat(s0:OuterPacks/text(),' ',s0:OuterPacksPackageType/text())"/>
                  </C10801>
                </ns0:C108_3>
              </ns0:FTX_3>

              <xsl:if test="$carrierSCACCode!=''">
                <ns0:NAD_2>
                  <NAD01>CX</NAD01>
                  <ns0:C082_2>
                    <C08201>
                      <xsl:value-of select="$carrierSCACCode"/>
                    </C08201>
                    <C08202>160</C08202>
                    <C08203>20</C08203>
                  </ns0:C082_2>
                </ns0:NAD_2>
              </xsl:if>

              <xsl:variable name="consigneeOrShipperAddressType">
                <xsl:choose>
                  <xsl:when test="contains($documentName,'Coastwise') and starts-with($lastSeaTransportLegBeforeHeader/s0:PortOfDischarge/text(),'ZA')">
                    <xsl:value-of select="'ConsigneeDocumentaryAddress'"/>
                  </xsl:when>
                  <xsl:when test="contains($documentName, 'Import')">
                    <xsl:value-of select="'ConsigneeDocumentaryAddress'"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="'ConsignorDocumentaryAddress'"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:variable>
              <xsl:variable name="orgConsigneeOrShipper" select ="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()=$consigneeOrShipperAddressType]"/>
              <xsl:if test="$orgConsigneeOrShipper">
                <ns0:NAD_2>
                  <NAD01>CN</NAD01>
                  <ns0:C080_2>
                    <C08001>
                      <xsl:value-of select ="substring(normalize-space($orgConsigneeOrShipper/s0:CompanyName/text()),1,35)"/>
                    </C08001>
                  </ns0:C080_2>
                  <xsl:variable name="Space35" select ="'                                   '"/>
                  <xsl:variable name="Street1Norm" select="normalize-space($orgConsigneeOrShipper/s0:Address1/text())"/>
                  <xsl:variable name="Street2Norm" select="normalize-space($orgConsigneeOrShipper/s0:Address2/text())"/>
                  <xsl:variable name="CityNorm" select="normalize-space($orgConsigneeOrShipper/s0:City/text())"/>
                  <xsl:variable name="StateNorm" select="normalize-space($orgConsigneeOrShipper/s0:State/text())"/>
                  <xsl:variable name="PostcodeNorm" select="normalize-space($orgConsigneeOrShipper/s0:Postcode/text())"/>
                  <xsl:variable name="CountryNorm" select="normalize-space($orgConsigneeOrShipper/s0:Country/text())"/>
                  <xsl:variable name="CityStatePostcodeNorm" select="normalize-space(concat($CityNorm,' ',$StateNorm, ' ', $PostcodeNorm))"/>
                  <xsl:variable name="Street" select="concat(
                            substring(concat($Street1Norm,$Space35),1,ceiling(string-length($Street1Norm) div 35)*35),
                            substring(concat($Street2Norm,$Space35),1,ceiling(string-length($Street2Norm) div 35)*35))"/>
                  <ns0:C059_2>
                    <C05901>
                      <xsl:value-of select="normalize-space(substring($Street,1,35))"/>
                    </C05901>
                    <xsl:if test="string-length($Street) > 35">
                      <C05902>
                        <xsl:value-of select="normalize-space(substring($Street,36,35))"/>
                      </C05902>
                    </xsl:if>
                    <xsl:if test="string-length($Street) > 70">
                      <C05903>
                        <xsl:value-of select="normalize-space(substring($Street,71,35))"/>
                      </C05903>
                    </xsl:if>
                  </ns0:C059_2>
                  <NAD06>
                    <xsl:value-of select ="normalize-space(substring($CityNorm,1,35))"/>
                  </NAD06>
                  <NAD07>
                    <xsl:value-of select ="normalize-space(substring($StateNorm,1,9))"/>
                  </NAD07>
                  <NAD08>
                    <xsl:value-of select ="normalize-space(substring($PostcodeNorm,1,9))"/>
                  </NAD08>
                  <NAD09>
                    <xsl:value-of select ="$CountryNorm"/>
                  </NAD09>
                </ns0:NAD_2>
              </xsl:if>

              <xsl:choose>
                <xsl:when test="contains($documentName, 'Import') or contains($documentName, 'Discharge Coastwise')">
                  <xsl:if test="starts-with($lastSeaTransportLegBeforeHeader/s0:PortOfDischarge/text(),'ZA')">
                    <xsl:call-template name="TDTLoop2">
                      <xsl:with-param name="TransportLeg" select="$lastSeaTransportLegBeforeHeader" />
                      <xsl:with-param name="code" select="'10'"/>
                    </xsl:call-template>
                  </xsl:if>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:if test="starts-with($firstSeaTransportLegAfterHeader/s0:PortOfLoading/text(),'ZA')">
                    <xsl:call-template name="TDTLoop2">
                      <xsl:with-param name="TransportLeg" select="$firstSeaTransportLegAfterHeader" />
                      <xsl:with-param name="code" select="'30'"/>
                    </xsl:call-template>
                  </xsl:if>
                </xsl:otherwise>
              </xsl:choose>
            </ns0:EQDLoop1>
          </xsl:for-each>

          <xsl:call-template name="CNT">
            <xsl:with-param name="cntCount" select="count(s0:SubShipmentCollection/s0:SubShipment)"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </ns0:EFACT_D95B_COPRAR>
  </xsl:template>

  <xsl:template name="CNT">
    <xsl:param name="cntCount" />
    <ns0:CNT>
      <ns0:C270>
        <C27001>16</C27001>
        <C27002>
          <xsl:value-of select="$cntCount"/>
        </C27002>
      </ns0:C270>
    </ns0:CNT>
  </xsl:template>

  <xsl:template name="RFF">
    <xsl:param name="suffix"/>
    <xsl:param name="code"/>
    <xsl:param name="value"/>
    <xsl:param name="fallbackValue"/>
    <xsl:if test="$value != '' or $fallbackValue !=''">
      <xsl:element name="ns0:RFF{$suffix}">
        <xsl:element name="ns0:C506{$suffix}">
          <xsl:element name="C50601">
            <xsl:value-of select="$code"/>
          </xsl:element>
          <xsl:element name="C50602">
            <xsl:choose>
              <xsl:when test="$value !=''">
                <xsl:value-of select="$value"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$fallbackValue"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="LOC">
    <xsl:param name="code"/>
    <xsl:param name="ref"/>
    <xsl:param name="suffix"/>
    <xsl:param name="terminalCode"/>

    <xsl:if test="$ref != ''">
      <xsl:element name="ns0:LOC{$suffix}">
        <LOC01>
          <xsl:value-of select="$code"/>
        </LOC01>
        <xsl:element name="ns0:C517{$suffix}">
          <C51701>
            <xsl:value-of select="$ref"/>
          </C51701>
          <C51702>139</C51702>
          <C51703>6</C51703>
        </xsl:element>

        <!--<xsl:variable name ="terminalCodeValue">
          <xsl:choose>
            <xsl:when test="$terminalCode!=''">
              <xsl:value-of select="$terminalCode"/>
            </xsl:when>
            <xsl:when test="$transportLegTerminalCode!= ''">
              <xsl:value-of select="$transportLegTerminalCode"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select ="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()=$orgAddressType]/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='TNP' and s0:CountryOfIssue/text()='ZA']/s0:Value/text()"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>-->

        <xsl:if test="$terminalCode!=''">
          <xsl:element name="ns0:C519{$suffix}">
            <C51901>
              <xsl:value-of select="$terminalCode"/>
            </C51901>
            <C51902>TER</C51902>
            <C51903>ZZZ</C51903>
          </xsl:element>
        </xsl:if>

      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="GetTerminalCode">
    <xsl:param name="terminalCode"/>
    <xsl:param name="transportLegNode"/>
    <xsl:param name="orgAddressType"/>

    <xsl:variable name="transportLegTerminalCode" select="$transportLegNode/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='TNP' and s0:CountryOfIssue/text()='ZA']/s0:Value/text()"/>

    <xsl:variable name ="terminalCodeValue">
      <xsl:choose>
        <xsl:when test="$terminalCode!=''">
          <xsl:value-of select="$terminalCode"/>
        </xsl:when>
        <xsl:when test="$transportLegTerminalCode!= ''">
          <xsl:value-of select="$transportLegTerminalCode"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()=$orgAddressType]/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='TNP' and s0:CountryOfIssue/text()='ZA']/s0:Value/text()"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:value-of select="$terminalCodeValue"/>
  </xsl:template>

  <xsl:template name="DocumentNameSwitchCaseReturn">
    <xsl:param name="documentName"/>
    <xsl:param name="returnType"/>
    <xsl:param name="consol"/>
    <xsl:choose>
      <xsl:when test="$returnType = 'CODE'">
        <xsl:choose>
          <xsl:when test="contains($documentName, 'Import') or contains($documentName, 'Discharge Coastwise')">43</xsl:when>
          <xsl:otherwise>45</xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="$returnType = 'SUFFIX'">
        <xsl:choose>
          <xsl:when test="$documentName='Cargo Dues - Import'">
            <xsl:value-of select="concat($consol,'_','IMPORT')"/>
          </xsl:when>
          <xsl:when test="$documentName='Cargo Dues - Load Coastwise'">
            <xsl:value-of select="concat($consol,'_','LOAD')"/>
          </xsl:when>
          <xsl:when test="$documentName='Cargo Dues - Discharge Coastwise'">
            <xsl:value-of select="concat($consol,'_','DISCHARGE')"/>
          </xsl:when>
          <xsl:when test="$documentName='Cargo Dues - Export'">
            <xsl:value-of select="concat($consol,'_','EXPORT')"/>
          </xsl:when>
          <xsl:when test="$documentName='Cargo Dues - Import (Quotation)'">
            <xsl:value-of select="concat($consol,'_','QIMPORT')"/>
          </xsl:when>
          <xsl:when test="$documentName='Cargo Dues - Load Coastwise (Quotation)'">
            <xsl:value-of select="concat($consol,'_','QLOAD')"/>
          </xsl:when>
          <xsl:when test="$documentName='Cargo Dues - Discharge Coastwise (Quotation)'">
            <xsl:value-of select="concat($consol,'_','QDISCHARGE')"/>
          </xsl:when>
          <xsl:when test="$documentName='Cargo Dues - Export (Quotation)'">
            <xsl:value-of select="concat($consol,'_','QEXPORT')"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="concat($consol,'_','UNKNOWN')"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="TDTLoop1">
    <xsl:param name="bgmCode"/>
    <xsl:param name="carrierSCACCode"/>
    <xsl:param name="transportLeg"/>
    <xsl:param name="radioCallSign"/>

    <ns0:TDTLoop1>
      <ns0:TDT>
        <TDT01>20</TDT01>
        <TDT02>
          <xsl:value-of select="$transportLeg/s0:VoyageFlightNo/text()"/>
        </TDT02>
        <ns0:C220>
          <C22001>1</C22001>
        </ns0:C220>
        <xsl:if test="$carrierSCACCode!=''">
          <ns0:C040>
            <C04001>
              <xsl:value-of select="$carrierSCACCode"/>
            </C04001>
            <C04002>172</C04002>
            <C04003>20</C04003>
          </ns0:C040>
        </xsl:if>
        <ns0:C222>
          <xsl:variable name="vesselRadioCallSign">
            <xsl:choose>
              <xsl:when test="$radioCallSign!=''">
                <xsl:value-of select="$radioCallSign"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$transportLeg/s0:DocumentaryOverride/s0:VesselRadioCallSign/text()" />
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <xsl:if test="$vesselRadioCallSign!=''">
            <C22201>
              <xsl:value-of select="$vesselRadioCallSign"/>
            </C22201>
            <C22202>103</C22202>
          </xsl:if>
          <C22204>
            <xsl:value-of select="$transportLeg/s0:VesselName/text()"/>
          </C22204>
        </ns0:C222>
      </ns0:TDT>

      <xsl:call-template name="RFF">
        <xsl:with-param name="code" select="'ATZ'"/>
        <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TNPAArrivalNumber']/s0:Value/text()"/>
        <xsl:with-param name="fallbackValue" select="s0:DocumentaryOverride/s0:TNPAArrivalNumber/text()"/>
        <xsl:with-param name="suffix" select="'_2'"/>
      </xsl:call-template>
      <xsl:call-template name="RFF">
        <xsl:with-param name="code" select="'VM'"/>
        <xsl:with-param name="value" select="$transportLeg/s0:VesselLloydsIMO/text()"/>
        <xsl:with-param name="suffix" select="'_2'"/>
      </xsl:call-template>

      <xsl:choose>
        <xsl:when test="$bgmCode='45'">

          <xsl:variable name="loc9TerminalCode">
            <xsl:call-template name="GetTerminalCode">
              <xsl:with-param name="terminalCode" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='Terminal']/s0:Value/text()"/>
              <xsl:with-param name="transportLegNode" select="$transportLeg/s0:DepartureFrom"/>
              <xsl:with-param name="orgAddressType" select="'DepartureCTOAddress'"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:call-template name="LOC">
            <xsl:with-param name="code" select="'9'"/>
            <xsl:with-param name="ref" select="$transportLeg/s0:PortOfLoading/text()"/>
            <xsl:with-param name="terminalCode" select="$loc9TerminalCode"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:variable name="loc11TerminalCode">
            <xsl:call-template name="GetTerminalCode">
              <xsl:with-param name="terminalCode" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='Terminal']/s0:Value/text()"/>
              <xsl:with-param name="transportLegNode" select="$transportLeg/s0:ArrivalAt"/>
              <xsl:with-param name="orgAddressType" select="'ArrivalCTOAddress'"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:call-template name="LOC">
            <xsl:with-param name="code" select="'11'"/>
            <xsl:with-param name="ref" select="$transportLeg/s0:PortOfDischarge/text()"/>
            <xsl:with-param name="terminalCode" select="$loc11TerminalCode"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:variable name="date">
        <xsl:choose>
          <xsl:when test="$bgmCode='45'">
            <xsl:value-of select="$transportLeg/s0:EstimatedDeparture/text()"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$transportLeg/s0:EstimatedArrival/text()"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:if test="$date!=''">
        <ns0:DTM>
          <ns0:C507>
            <C50701>
              <xsl:choose>
                <xsl:when test="$bgmCode='45'">133</xsl:when>
                <xsl:otherwise>132</xsl:otherwise>
              </xsl:choose>
            </C50701>
            <C50702>
              <xsl:value-of select="ScriptNS2:ConvertToDate($date,'yyyyMMddHHmm')"/>
            </C50702>
            <C50703>203</C50703>
          </ns0:C507>
        </ns0:DTM>
      </xsl:if>
    </ns0:TDTLoop1>
  </xsl:template>

  <xsl:template name="TDTLoop2">
    <xsl:param name="TransportLeg"/>
    <xsl:param name="code"/>
    <xsl:if test="$TransportLeg">
      <ns0:TDTLoop2>
        <ns0:TDT_2>
          <TDT01>
            <xsl:value-of select="$code"/>
          </TDT01>
          <TDT02>
            <xsl:value-of select="$TransportLeg/s0:VoyageFlightNo/text()"/>
          </TDT02>
          <ns0:C220_2>
            <C22001>1</C22001>
          </ns0:C220_2>
          <ns0:C222_2>
            <C22204>
              <xsl:value-of select="$TransportLeg/s0:VesselName/text()"/>
            </C22204>
          </ns0:C222_2>
        </ns0:TDT_2>
      </ns0:TDTLoop2>
    </xsl:if>
  </xsl:template>

  <xsl:template name="C10801Loop">
    <xsl:param name ="counter"/>
    <xsl:param name ="maxlength" select="70" />
    <xsl:param name ="maxTags" select="5" />
    <xsl:param name ="freetext"  />
    <xsl:variable name="substringbefore" select="substring($freetext,1,$maxlength)"/>
    <xsl:variable name="substringafter" select="substring($freetext,$maxlength + 1)"/>
    <xsl:if test="$counter &lt;= $maxTags">
      <xsl:element name="{concat('C1080',$counter)}">
        <xsl:value-of select="$substringbefore"/>
      </xsl:element>
      <xsl:if test ="string-length($substringafter) &gt; 0">
        <xsl:call-template name="C10801Loop">
          <xsl:with-param name="counter" select="$counter+1"/>
          <xsl:with-param name="freetext" select="$substringafter" />
        </xsl:call-template>
      </xsl:if>
    </xsl:if>
  </xsl:template>

  <xsl:template name="CTA">
    <xsl:param name="code" />
    <xsl:param name="value" />

    <xsl:if test="$value!=''">
      <ns0:CTA>
        <CTA01>
          <xsl:value-of select="$code"/>
        </CTA01>
        <ns0:C056>
          <C05601>
            <xsl:value-of select="$value"/>
          </C05601>
        </ns0:C056>
      </ns0:CTA>
    </xsl:if>
  </xsl:template>
</xsl:stylesheet>

<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var"
                exclude-result-prefixes="msxsl var s0 CodeMapper userCSharp UnitConverter ContextAccessor"
                version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:ns0="http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:UnitConverter="http://schemas.microsoft.com/BizTalk/2003/UnitConverter"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor">

  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes"/>
  <xsl:template match="/">
    <xsl:apply-templates select="/s0:UniversalInterchange" />
  </xsl:template>
  <xsl:template match="/s0:UniversalInterchange">
    <ns0:EFACT_D95B_COPRAR>
      <xsl:variable name="operationalPortCode" select="//*[local-name()='UniversalShipment'][1]/*[local-name()='Shipment']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][*[local-name()='Key']/text()='OperationalPort_Code']/*[local-name()='Value']/text()"/>
      <xsl:variable name="operationalPortCountry">
        <xsl:choose>
          <xsl:when test="$operationalPortCode != ''">
            <xsl:value-of select="substring($operationalPortCode, 1, 2)"/>
          </xsl:when>
          <xsl:otherwise>NZ</xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="PortForRouting">
        <xsl:choose>
          <xsl:when test="//*[local-name()='UniversalShipment'][1]/*[local-name()='Shipment']/*[local-name()='DataContext']/*[local-name()='Workflow']/*[local-name()='RecipientRoleCollection']/*[local-name()='RecipientRole']/text() = 'PEM'">
            <xsl:value-of select="concat(//*[local-name()='UniversalShipment'][1]/*[local-name()='Shipment']/*[local-name()='TransportLegCollection']/*[local-name()='TransportLeg'][*[local-name()='LegType']/text()='Main'][1]/*[local-name()='PortOfLoading']/text(), ' Load')"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="concat(//*[local-name()='UniversalShipment'][1]/*[local-name()='Shipment']/*[local-name()='TransportLegCollection']/*[local-name()='TransportLeg'][*[local-name()='LegType']/text()='Main'][1]/*[local-name()='PortOfDischarge']/text(), ' Discharge')"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="SourceParty" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
      <xsl:variable name="DestinationParty" select="CodeMapper:GetRecipientCode('ShippingPortMessaging', 'ShippingPortMessaging', 'ShippingPortMessaging COPRAR', 'Ports', 'DestinationParty', $PortForRouting)" />
      <xsl:variable name="SetDestinationParty" select="ContextAccessor:SetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties', $DestinationParty)"/>
      <xsl:variable name="SetDestinationPartyName" select="ContextAccessor:SetContextProperty('DestinationPartyName', 'http://schemas.microsoft.com/Edi/PropertySchema', $DestinationParty)"/>
      <xsl:variable name="OverrideEDIHeader" select="ContextAccessor:SetContextProperty('OverrideEDIHeader', 'http://schemas.microsoft.com/BizTalk/2006/edi-properties', 'true')" />
      <xsl:variable name="DestinationPartySenderIdentifier" select="ContextAccessor:SetContextProperty('UNB2_1', 'http://schemas.microsoft.com/BizTalk/2006/edi-properties', substring($SourceParty, 1, 3))" />

      <xsl:variable name="eventReference" select="//*[local-name()='TriggerReference'][1]/text()" />
      <xsl:variable name="directionCode" select="userCSharp:GetDirectionCode($eventReference)" />
      <xsl:variable name="messageTypeCode" select="userCSharp:GetMessageTypeCode($eventReference)" />
      <xsl:variable name="lloyds" select="//*[local-name()='TransportLeg'][*[local-name()='LegType']/text()='Main'][1]/*[local-name()='VesselLloydsIMO']/text()" />
      <xsl:variable name="voyageNum" select="//*[local-name()='TransportLeg'][*[local-name()='LegType']/text()='Main'][1]/*[local-name()='VoyageFlightNo']/text()" />
      <xsl:variable name="carrierOrgCode" select="//*[local-name()='TransportLeg'][*[local-name()='LegType']/text()='Main'][1]/*[local-name()='Carrier']/*[local-name()='OrganizationCode']/text()" />
      <xsl:variable name="originalDocIdentifier" select="userCSharp:GetOriginalDocIdentifier($lloyds, $voyageNum, $carrierOrgCode)" />
      <xsl:variable name="vesselName" select="//*[local-name()='TransportLeg'][*[local-name()='LegType']/text()='Main'][1]/*[local-name()='VesselName']/text()" />
      <xsl:variable name="carrierRegNum" select="//*[local-name()='TransportLeg'][*[local-name()='LegType']/text()='Main'][1]/*[local-name()='Carrier']/*[local-name()='RegistrationNumberCollection']/*[local-name()='RegistrationNumber'][*[local-name()='Type']/text()='CAR' and *[local-name()='CountryOfIssue']/text()=$operationalPortCountry][1]/*[local-name()='Value']/text()" />
      <xsl:variable name="portForMessage">
        <xsl:choose>
          <xsl:when test="$directionCode = '45'">
            <xsl:value-of select="//*[local-name()='TransportLeg'][*[local-name()='LegType']/text()='Main'][1]/*[local-name()='PortOfLoading']/text()"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="//*[local-name()='TransportLeg'][*[local-name()='LegType']/text()='Main'][1]/*[local-name()='PortOfDischarge']/text()"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <UNH>
        <xsl:variable name="unhNumber" select="CodeMapper:CallActionProcedureHelper('GetCounterValue', '@value', '@name', 'CargoWise.eHub.Products.OceanTracing.Transforms.COPRAR', '@maxlength', '14')" />
        <UNH1>
          <xsl:value-of select="$unhNumber" />
        </UNH1>
        <UNH2>
          <UNH2.1>
            <xsl:text>COPRAR</xsl:text>
          </UNH2.1>
          <UNH2.2>
            <xsl:text>D</xsl:text>
          </UNH2.2>
          <UNH2.3>
            <xsl:text>95B</xsl:text>
          </UNH2.3>
          <UNH2.4>
            <xsl:text>UN</xsl:text>
          </UNH2.4>
        </UNH2>
      </UNH>

      <BGM>
        <C002>
          <C00201>
            <xsl:value-of select="$directionCode" />
          </C00201>
        </C002>
        <BGM02>
          <xsl:choose>
            <xsl:when test="$messageTypeCode = '9'">
              <xsl:value-of select="$originalDocIdentifier" />
            </xsl:when>
            <xsl:otherwise>
              <xsl:variable name="docIdentifier" select="CodeMapper:CallActionProcedureHelper('GetCounterValue', '@value', '@name', 'CargoWise.eHub.Products.OceanTracing.Transforms.COPRAR', '@maxlength', '14')" />
              <xsl:value-of select="$docIdentifier" />
            </xsl:otherwise>
          </xsl:choose>
        </BGM02>
        <BGM03>
          <xsl:value-of select="$messageTypeCode" />
        </BGM03>
      </BGM>

      <RFF>
        <C506>
          <C50601>
            <xsl:choose>
              <xsl:when test="$messageTypeCode != '9'">ACW</xsl:when>
              <xsl:otherwise>XXX</xsl:otherwise>
            </xsl:choose>
          </C50601>
          <C50602>
            <xsl:choose>
              <xsl:when test="$messageTypeCode != '9'">
                <xsl:value-of select="$originalDocIdentifier" />
              </xsl:when>
              <xsl:otherwise>1</xsl:otherwise>
            </xsl:choose>
          </C50602>
        </C506>
      </RFF>

      <TDTLoop1>

        <TDT>
          <TDT01>
            <xsl:text>20</xsl:text>
          </TDT01>
          <TDT02>
            <xsl:value-of select="$voyageNum" />
          </TDT02>
          <C220>
            <C22001>
              <xsl:text>1</xsl:text>
            </C22001>
          </C220>
          <C040>
            <C04001>
              <xsl:value-of select="$carrierRegNum" />
            </C04001>
            <C04002>
              <xsl:text>172</xsl:text>
            </C04002>
            <C04003>
              <xsl:text>20</xsl:text>
            </C04003>
          </C040>
          <C222>
            <C22201>
              <xsl:value-of select="$lloyds" />
            </C22201>
            <C22202>
              <xsl:text>146</xsl:text>
            </C22202>
            <C22203>
              <xsl:text>11</xsl:text>
            </C22203>
            <C22204>
              <xsl:value-of select="$vesselName" />
            </C22204>
          </C222>
        </TDT>

        <LOC>
          <LOC01>
            <xsl:if test="$directionCode = '45'">
              <xsl:text>9</xsl:text>
            </xsl:if>
            <xsl:if test="$directionCode = '43'">
              <xsl:text>11</xsl:text>
            </xsl:if>
          </LOC01>
          <C517>
            <C51701>
              <xsl:value-of select="$portForMessage"/>
            </C51701>
            <C51702>
              <xsl:text>139</xsl:text>
            </C51702>
            <C51703>
              <xsl:text>6</xsl:text>
            </C51703>

            <xsl:variable name="portName">
              <xsl:choose>
                <xsl:when test="$directionCode = '45'">
                  <xsl:value-of select="//*[local-name()='TransportLeg'][*[local-name()='LegType']/text()='Main'][1]/*[local-name()='PortOfLoading']/@Name" />
                </xsl:when>
                <xsl:when test="$directionCode = '43'">
                  <xsl:value-of select="//*[local-name()='TransportLeg'][*[local-name()='LegType']/text()='Main'][1]/*[local-name()='PortOfDischarge']/@Name" />
                </xsl:when>
              </xsl:choose>
            </xsl:variable>
            <C51704>
              <xsl:call-template name="GetPortName">
                <xsl:with-param name="portName" select="$portName" />
              </xsl:call-template>
            </C51704>
          </C517>
        </LOC>

        <DTM>
          <C507>
            <C50701>
              <xsl:if test="$directionCode = '45'">
                <xsl:text>133</xsl:text>
              </xsl:if>
              <xsl:if test="$directionCode = '43'">
                <xsl:text>132</xsl:text>
              </xsl:if>
            </C50701>
            <C50702>
              <xsl:if test="$directionCode = '45'">
                <xsl:value-of select="userCSharp:FormatDateTime(//*[local-name()='TransportLeg'][*[local-name()='LegType']/text()='Main'][1]/*[local-name()='EstimatedDeparture']/text(), 'yyyy-MM-ddTHH:mm:ss', 'yyyyMMddHHmmss')" />
              </xsl:if>
              <xsl:if test="$directionCode = '43'">
                <xsl:value-of select="userCSharp:FormatDateTime(//*[local-name()='TransportLeg'][*[local-name()='LegType']/text()='Main'][1]/*[local-name()='EstimatedArrival']/text(), 'yyyy-MM-ddTHH:mm:ss', 'yyyyMMddHHmmss')" />
              </xsl:if>
            </C50702>
            <C50703>
              <xsl:text>203</xsl:text>
            </C50703>
          </C507>
        </DTM>

      </TDTLoop1>

      <NADLoop1>

        <NAD>
          <NAD01>
            <xsl:text>CA</xsl:text>
          </NAD01>
          <C082>
            <C08201>
              <xsl:value-of select="$carrierRegNum" />
            </C08201>
            <C08202>
              <xsl:text>172</xsl:text>
            </C08202>
            <C08203>
              <xsl:text>20</xsl:text>
            </C08203>
          </C082>
        </NAD>

      </NADLoop1>

      <xsl:for-each select="//*[local-name()='ContainerCollection']/*[local-name()='Container']">
        <EQDLoop1>

          <EQD>
            <EQD01>
              <xsl:text>CN</xsl:text>
            </EQD01>
            <C237>
              <C23701>
                <xsl:value-of select="./*[local-name()='ContainerNumber']" />
              </C23701>
            </C237>
            <C224>
              <C22401>
                <xsl:value-of select="./*[local-name()='ContainerType']/*[local-name()='ISOCode']" />
              </C22401>
              <C22402>
                <xsl:text>102</xsl:text>
              </C22402>
              <C22403>
                <xsl:text>5</xsl:text>
              </C22403>
            </C224>
            <EQD05>
              <xsl:variable name="containerLoad" select="../../*[local-name()='TransportLegCollection']/*[local-name()='TransportLeg'][*[local-name()='LegType']/text()='Main'][1]/*[local-name()='PortOfLoading']/text()" />
              <xsl:variable name="containerDischarge" select="../../*[local-name()='TransportLegCollection']/*[local-name()='TransportLeg'][*[local-name()='LegType']/text()='Main'][1]/*[local-name()='PortOfDischarge']/text()" />
              <xsl:value-of select="userCSharp:GetEqStatus($operationalPortCountry, $containerLoad, $containerDischarge)" />
            </EQD05>
            <EQD06>
              <xsl:choose>
                <xsl:when test="./*[local-name()='IsEmptyContainer'] = 'true'">
                  <xsl:text>4</xsl:text>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:text>5</xsl:text>
                </xsl:otherwise>
              </xsl:choose>
            </EQD06>
          </EQD>

          <RFF_3>
            <C506_3>
              <C50601>
                <xsl:text>BN</xsl:text>
              </C50601>
              <C50602>
                <xsl:value-of select="../../*[local-name()='CFSReference']/text()" />
              </C50602>
            </C506_3>
          </RFF_3>

          <RFF_3>
            <C506_3>
              <C50601>
                <xsl:text>BM</xsl:text>
              </C50601>
              <C50602>
                <xsl:value-of select="../../*[local-name()='WayBillNumber']/text()" />
              </C50602>
            </C506_3>
          </RFF_3>

          <xsl:variable name="stowagePosition" select="./*[local-name()='StowagePosition']/text()" />
          <xsl:if test="$stowagePosition != ''">
            <LOC_2>
              <LOC01>
                <xsl:text>147</xsl:text>
              </LOC01>
              <C517_2>
                <C51701>
                  <xsl:value-of select="$stowagePosition" />
                </C51701>
                <C51703>
                  <xsl:text>5</xsl:text>
                </C51703>
              </C517_2>
            </LOC_2>
          </xsl:if>

          <xsl:choose>
            <xsl:when test="$directionCode = '45'">
              <LOC_2>
                <LOC01>
                  <xsl:text>11</xsl:text>
                </LOC01>
                <C517_2>
                  <C51701>
                    <xsl:value-of select="../../*[local-name()='PortOfDischarge']/text()" />
                  </C51701>
                  <C51702>
                    <xsl:text>139</xsl:text>
                  </C51702>
                  <C51703>
                    <xsl:text>6</xsl:text>
                  </C51703>
                  <C51704>
                    <xsl:call-template name="GetPortName">
                      <xsl:with-param name="portName" select="../../*[local-name()='PortOfDischarge']/@Name" />
                    </xsl:call-template>
                  </C51704>
                </C517_2>
              </LOC_2>
            </xsl:when>
            <xsl:otherwise>
              <LOC_2>
                <LOC01>
                  <xsl:text>9</xsl:text>
                </LOC01>
                <C517_2>
                  <C51701>
                    <xsl:value-of select="../../*[local-name()='PortOfLoading']/text()" />
                  </C51701>
                  <C51702>
                    <xsl:text>139</xsl:text>
                  </C51702>
                  <C51703>
                    <xsl:text>6</xsl:text>
                  </C51703>
                  <C51704>
                    <xsl:call-template name="GetPortName">
                      <xsl:with-param name="portName" select="../../*[local-name()='PortOfLoading']/@Name" />
                    </xsl:call-template>
                  </C51704>
                </C517_2>
              </LOC_2>
            </xsl:otherwise>
          </xsl:choose>

          <LOC_2>
            <LOC01>
              <xsl:text>5</xsl:text>
            </LOC01>
            <C517_2>
              <C51701>
                <xsl:value-of select="../../*[local-name()='PortOfOrigin']/text()" />
              </C51701>
              <C51702>
                <xsl:text>139</xsl:text>
              </C51702>
              <C51703>
                <xsl:text>6</xsl:text>
              </C51703>
              <C51704>
                <xsl:call-template name="GetPortName">
                  <xsl:with-param name="portName" select="../../*[local-name()='PortOfOrigin']/@Name" />
                </xsl:call-template>
              </C51704>
            </C517_2>
          </LOC_2>

          <LOC_2>
            <LOC01>
              <xsl:text>7</xsl:text>
            </LOC01>
            <C517_2>
              <C51701>
                <xsl:value-of select="../../*[local-name()='PortOfDestination']/text()" />
              </C51701>
              <C51702>
                <xsl:text>139</xsl:text>
              </C51702>
              <C51703>
                <xsl:text>6</xsl:text>
              </C51703>
              <C51704>
                <xsl:call-template name="GetPortName">
                  <xsl:with-param name="portName" select="../../*[local-name()='PortOfDestination']/@Name" />
                </xsl:call-template>
              </C51704>
            </C517_2>
          </LOC_2>

          <MEA>
            <MEA01>
              <xsl:text>AAE</xsl:text>
            </MEA01>
            <C502>
              <C50201>
                <xsl:text>G</xsl:text>
              </C50201>
            </C502>
            <C174>
              <C17401>
                <xsl:text>KGM</xsl:text>
              </C17401>
              <C17402>
                <xsl:variable name="grossWeight" select="./*[local-name()='GrossWeight']/text()" />
                <xsl:variable name="weightUnit" select="./*[local-name()='WeightUnit']/text()" />
                <xsl:value-of select="UnitConverter:Convert($grossWeight, $weightUnit, 'KG')" />
              </C17402>
            </C174>
          </MEA>

          <xsl:call-template name="DIMSegment">
            <xsl:with-param name="codeQualifier" select="'5'" />
            <xsl:with-param name="overhangFT" select="./*[local-name()='OverhangFront']/text()" />
          </xsl:call-template>

          <xsl:call-template name="DIMSegment">
            <xsl:with-param name="codeQualifier" select="'6'" />
            <xsl:with-param name="overhangFT" select="./*[local-name()='OverhangBack']/text()" />
          </xsl:call-template>

          <xsl:call-template name="DIMSegment">
            <xsl:with-param name="codeQualifier" select="'7'" />
            <xsl:with-param name="overhangFT" select="./*[local-name()='OverhangRight']/text()" />
          </xsl:call-template>

          <xsl:call-template name="DIMSegment">
            <xsl:with-param name="codeQualifier" select="'8'" />
            <xsl:with-param name="overhangFT" select="./*[local-name()='OverhangLeft']/text()" />
          </xsl:call-template>

          <xsl:call-template name="DIMSegment">
            <xsl:with-param name="codeQualifier" select="'9'" />
            <xsl:with-param name="overhangFT" select="./*[local-name()='OverhangHeight']/text()" />
          </xsl:call-template>

          <xsl:variable name="tempUnit" select="./*[local-name()='SetPointTempUnit']/text()" />
          <xsl:if test="$tempUnit != ''">
            <TMP>
              <TMP01>
                <xsl:text>2</xsl:text>
              </TMP01>
              <C239>
                <C23901>
                  <xsl:value-of select="userCSharp:TrimTrailingZeros(./*[local-name()='SetPointTemp']/text())"/>
                </C23901>
                <C23902>
                  <xsl:if test="$tempUnit = 'C'">
                    <xsl:text>CEL</xsl:text>
                  </xsl:if>
                  <xsl:if test="$tempUnit = 'F'">
                    <xsl:text>FAH</xsl:text>
                  </xsl:if>
                </C23902>
              </C239>
            </TMP>
          </xsl:if>

          <xsl:call-template name="SEL">
            <xsl:with-param name="seal" select="./*[local-name()='Seal']/text()"/>
            <xsl:with-param name="sealPartyType" select="./*[local-name()='SealPartyType']/text()"/>
            <xsl:with-param name="isDamaged" select="./*[local-name()='IsDamaged']/text()"/>
          </xsl:call-template>
          <xsl:call-template name="SEL">
            <xsl:with-param name="seal" select="./*[local-name()='SecondSeal']/text()"/>
            <xsl:with-param name="sealPartyType" select="./*[local-name()='SecondSealPartyType']/text()"/>
            <xsl:with-param name="isDamaged" select="./*[local-name()='IsDamaged']/text()"/>
          </xsl:call-template>
          <xsl:call-template name="SEL">
            <xsl:with-param name="seal" select="./*[local-name()='ThirdSeal']/text()"/>
            <xsl:with-param name="sealPartyType" select="./*[local-name()='ThirdSealPartyType']/text()"/>
            <xsl:with-param name="isDamaged" select="./*[local-name()='IsDamaged']/text()"/>
          </xsl:call-template>
          

          <FTX_3>
            <FTX01>
              <xsl:text>AAA</xsl:text>
            </FTX01>
            <C108_3>
              <C10801>
                <xsl:value-of select="normalize-space(../../*[local-name()='GoodsDescription']/text())" />
              </C10801>
            </C108_3>
          </FTX_3>

          <xsl:variable name="containerLink" select="./*[local-name()='Link']/text()" />
          <xsl:for-each select="../../*[local-name()='PackingLineCollection']/*[local-name()='PackingLine']">
            <xsl:if test="./*[local-name()='ContainerLink']/text() = $containerLink">
              <xsl:for-each select="./*[local-name()='UNDGCollection']/*[local-name()='UNDG']">
                <DGS>
                  <DGS01>
                    <xsl:text>IMD</xsl:text>
                  </DGS01>
                  <C205>
                    <C20501>
                      <xsl:value-of select="./*[local-name()='IMOClass']/text()" />
                    </C20501>
                  </C205>
                  <C234>
                    <C23401>
                      <xsl:value-of select="userCSharp:GetUNDGCode(./*[local-name()='UNDGCode']/text())" />
                    </C23401>
                  </C234>
                  <C223>
                    <C22301>
                      <xsl:value-of select="userCSharp:GetFlashPoint(./*[local-name()='FlashPoint']/text())" />
                    </C22301>
                    <C22302>
                      <xsl:text>CEL</xsl:text>
                    </C22302>
                  </C223>
                  <xsl:variable name="packingGroupCoded" select="string-length(./*[local-name()='PackingGroup']/text())"/>
                  <xsl:if test="$packingGroupCoded > 0">
                    <DGS05>
                      <xsl:value-of select="$packingGroupCoded" />
                    </DGS05>
                  </xsl:if>
                </DGS>
              </xsl:for-each>
            </xsl:if>
          </xsl:for-each>

          <xsl:variable name="clipOnNumber" select="./*[local-name()='RefrigGeneratorID']/text()" />
          <xsl:if test="$clipOnNumber != ''">
            <EQA>
              <EQA01>
                <xsl:text>RG</xsl:text>
              </EQA01>
              <C237_2>
                <C23701>
                  <xsl:value-of select="$clipOnNumber" />
                </C23701>
              </C237_2>
            </EQA>
          </xsl:if>

          <xsl:variable name="carriagePortType">
            <xsl:choose>
              <xsl:when test="$directionCode = '45'">
                <xsl:value-of select="'PortOfDischarge'"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="'PortOfLoading'"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <xsl:if test="count(../../*[local-name()='TransportLegCollection']/*[local-name()='TransportLeg'][*[local-name()=$carriagePortType]/text()=$portForMessage]) != 0">
            <TDTLoop2>
              <TDT_2>
                <TDT01>
                  <xsl:if test="$directionCode = '45'">
                    <xsl:text>10</xsl:text>
                  </xsl:if>
                  <xsl:if test="$directionCode = '43'">
                    <xsl:text>30</xsl:text>
                  </xsl:if>
                </TDT01>
                <TDT02>
                  <xsl:value-of select="../../*[local-name()='TransportLegCollection']/*[local-name()='TransportLeg'][*[local-name()=$carriagePortType]/text()=$portForMessage][1]/*[local-name()='VoyageFlightNo']/text()" />
                </TDT02>
                <C220_2>
                  <C22001>
                    <xsl:text>1</xsl:text>
                  </C22001>
                </C220_2>
                <C040_2>
                  <C04001>
                    <xsl:value-of select="../../*[local-name()='TransportLegCollection']/*[local-name()='TransportLeg'][*[local-name()=$carriagePortType]/text()=$portForMessage][1]/*[local-name()='Carrier']/*[local-name()='RegistrationNumberCollection']/*[local-name()='RegistrationNumber'][*[local-name()='Type']/text()='CAR' and *[local-name()='CountryOfIssue']/text()=$operationalPortCountry][1]/*[local-name()='Value']/text()" />
                  </C04001>
                  <C04002>
                    <xsl:text>172</xsl:text>
                  </C04002>
                  <C04003>
                    <xsl:text>20</xsl:text>
                  </C04003>
                </C040_2>
                <C222_2>
                  <C22201>
                    <xsl:value-of select="../../*[local-name()='TransportLegCollection']/*[local-name()='TransportLeg'][*[local-name()=$carriagePortType]/text()=$portForMessage][1]/*[local-name()='VesselLloydsIMO']/text()" />
                  </C22201>
                  <C22202>
                    <xsl:text>146</xsl:text>
                  </C22202>
                  <C22204>
                    <xsl:value-of select="../../*[local-name()='TransportLegCollection']/*[local-name()='TransportLeg'][*[local-name()=$carriagePortType]/text()=$portForMessage][1]/*[local-name()='VesselName']/text()" />
                  </C22204>
                </C222_2>
              </TDT_2>
            </TDTLoop2>
          </xsl:if>

          <NAD_2>
            <xsl:variable name="principalRegNum" select="../../*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][*[local-name()='AddressType']/text()='Principal'][1]/*[local-name()='RegistrationNumberCollection']/*[local-name()='RegistrationNumber'][*[local-name()='Type']/text()='CAR' and *[local-name()='CountryOfIssue']/text()=$operationalPortCountry][1]/*[local-name()='Value']/text()" />
            <NAD01>
              <xsl:text>CF</xsl:text>
            </NAD01>
            <C082_2>
              <C08201>
                <xsl:value-of select="$principalRegNum" />
              </C08201>
              <C08202>
                <xsl:text>172</xsl:text>
              </C08202>
              <C08203>
                <xsl:text>20</xsl:text>
              </C08203>
            </C082_2>
          </NAD_2>

        </EQDLoop1>
      </xsl:for-each>

      <ns0:CNT>
        <ns0:C270>
          <C27001>
            <xsl:text>16</xsl:text>
          </C27001>
          <C27002>
            <xsl:value-of select="count(//*[local-name()='ContainerCollection']/*[local-name()='Container'])" />
          </C27002>
        </ns0:C270>
      </ns0:CNT>

    </ns0:EFACT_D95B_COPRAR>
  </xsl:template>

  <xsl:template name="SEL">
    <xsl:param name="seal"/>
    <xsl:param name="sealPartyType"/>
    <xsl:param name="isDamaged"/>
      
    <xsl:if test="$seal != ''">
      <SEL>
        <SEL01>
          <xsl:value-of select="$seal" />
        </SEL01>
        <xsl:variable name="sealPartyTypeCode">
          <xsl:choose>
            <xsl:when test="$sealPartyType = 'CAR'">CA</xsl:when>
            <xsl:when test="$sealPartyType = 'CRD'">SH</xsl:when>
            <xsl:when test="$sealPartyType = 'CUS'">CU</xsl:when>
            <xsl:when test="$sealPartyType = 'QRT'">AC</xsl:when>
            <xsl:when test="$sealPartyType = 'CTO'">TO</xsl:when>
            <xsl:otherwise></xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:if test="$sealPartyTypeCode != ''">
          <C215>
            <C21501>
              <xsl:value-of select="$sealPartyTypeCode"/>
            </C21501>
          </C215>
        </xsl:if>
        <xsl:if test="$isDamaged != ''">
          <SEL03>
            <xsl:choose>
              <xsl:when test="$isDamaged = 'true'">
                <xsl:text>2</xsl:text>
              </xsl:when>
              <xsl:otherwise>
                <xsl:text>1</xsl:text>
              </xsl:otherwise>
            </xsl:choose>
          </SEL03>
        </xsl:if>
      </SEL>
    </xsl:if>
  </xsl:template>

  <xsl:template name="GetPortName">
    <xsl:param name="portName" />

    <xsl:choose>
      <xsl:when test="$portName!=''">
        <xsl:value-of select="substring($portName, 1, 17)"/>
      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="DIMSegment">
    <xsl:param name="codeQualifier" />
    <xsl:param name="overhangFT" />

    <xsl:if test="$overhangFT !='' and number($overhangFT) != 0">
      <xsl:variable name="overhangIN" select="UnitConverter:Convert($overhangFT, 'FT', 'CM')" />
      <DIM>
        <DIM01>
          <xsl:value-of select="$codeQualifier" />
        </DIM01>
        <C211>
          <C21101>
            <xsl:text>CMT</xsl:text>
          </C21101>
          <C21102>
            <xsl:value-of select="userCSharp:TrimTrailingZeros($overhangIN)" />
          </C21102>
        </C211>
      </DIM>
    </xsl:if>
  </xsl:template>

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[
public string GetDirectionCode(string reference)
{
    var messageType = GetParamValue(reference, "MST");
    
    switch (messageType)
    {
        case "Discharge Manifest":
        case "Discharge Manifest Replacement":
        case "Discharge Manifest Cancellation":
          return "43";
    
        case "Load Manifest":
        case "Load Manifest Replacement":
        case "Load Manifest Cancellation":
          return "45";
    }
    
    return "";
}

public string GetOriginalDocIdentifier(string lloyds, string voyage, string carrierCode)
{
    return string.Format("{0}-{1}-{2}", lloyds, voyage, carrierCode);
}

public string GetMessageTypeCode(string reference)
{
    var messageType = GetParamValue(reference, "MST");
    
    switch (messageType)
    {
        case "Discharge Manifest":
        case "Load Manifest":
          return "9";
          
        case "Discharge Manifest Replacement":
        case "Load Manifest Replacement":
          return "5";
    
        case "Discharge Manifest Cancellation":
        case "Load Manifest Cancellation":
          return "1";
    }
    
    return "";
}

public string GetParamValue(string eventRef, string paramType)
{
    var parameters = eventRef.Split('|');
    
    foreach (var parameter in parameters)
    {
        if (parameter.StartsWith(paramType))
        {
            return parameter.Substring(paramType.Length + 1);
        }
    }
    
    return "";
}

public string GetEqStatus(string operationalPortCountry, string load, string discharge)
{
    bool loadLocal = load.Substring(0, 2) == operationalPortCountry;
    bool dischargeLocal = discharge.Substring(0, 2) == operationalPortCountry;

    if (loadLocal && dischargeLocal)
    {
        return "1";
    }
    else if (loadLocal && !dischargeLocal)
    {
        return "2";
    }
    else if (!loadLocal && dischargeLocal)
    {
        return "3";
    }
    else
    {
        return "6";
    }
}

public string FormatDateTime(string val, string inFmts, string outFmt)
{
	DateTime parsedDate;
	if (DateTime.TryParseExact(val, inFmts.Split(new char[] {';'}), System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out parsedDate))
	{
		return parsedDate.ToString(outFmt);
	}
	else	{
				try
				{
					parsedDate = XmlConvert.ToDateTime(val, XmlDateTimeSerializationMode.Unspecified);
				}
				catch
				{
					return string.Empty;
				};
				return parsedDate.ToString(outFmt);
	}
}

public string TrimTrailingZeros(string num)
{
	return string.Format("{0:G29}", decimal.Parse(num));
}

public string GetUNDGCode(string undgCode)
{
	string result = "";

	foreach (var c in undgCode)
	{
		if (char.IsDigit(c))
		{
			result += c;
		}
		else
		{
			break;
		}
	}

	return result;
}


public string GetFlashPoint(string flashPoint)
{
	return ((int)decimal.Parse(flashPoint)).ToString().PadLeft(3, '0');
}


public string GetPackingGroup(string packingGroup)
{
	return packingGroup.Length.ToString();
}

]]>
  </msxsl:script>
</xsl:stylesheet>
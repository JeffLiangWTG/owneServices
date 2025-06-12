<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var" exclude-result-prefixes="msxsl var s0 ScriptNS0 userCSharp" version="1.0" xmlns:ns0="http://cargowise.com/ehub/clients/HHE/2011/06" xmlns:s0="http://www.cargowise.com/Schemas/Native" xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0" xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">

  <!-- Write Shipment PortOfOrigin Begin-->
  <xsl:for-each select="./*[local-name()='LOCLoop2']">
    <xsl:variable name="shipmentLocationQualifier"  select="./*[local-name()='LOC_6']/*[local-name()='LOC01']/text()"/>
    <xsl:if test ="string($shipmentLocationQualifier)='9'">
      <xsl:element name="ns0:PortOfOrigin">
        <xsl:element name="ns0:Code">
          <xsl:value-of select="./*[local-name()='LOC_6']/*[local-name()='C517_6']/*[local-name()='C51701']/text()" />
        </xsl:element>
      </xsl:element>
    </xsl:if>
  </xsl:for-each>

  <!--if no PortOfOrigin provided, use PortOfLoading from Consol-->
  <xsl:variable name="counterShipmentPortOfOrigin" select="count(./*[local-name()='LOCLoop2']/*[local-name()='LOC_6']/*[local-name()='LOC01' and text()='9'])"/>
  <xsl:if test ="$counterShipmentPortOfOrigin = 0">
    <xsl:element name="ns0:PortOfOrigin">
      <xsl:element name="ns0:Code">
        <xsl:for-each select="../*[local-name()='TDTLoop1']">
          <xsl:for-each select="./*[local-name()='LOCLoop1']">
            <xsl:variable name="consolLocationQualifier"  select="./*[local-name()='LOC_3']/*[local-name()='LOC01']/text()"/>
            <xsl:if test ="string($consolLocationQualifier)='9'">
              <xsl:value-of select="./*[local-name()='LOC_3']/*[local-name()='C517_3']/*[local-name()='C51701']/text()" />
            </xsl:if>
          </xsl:for-each>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>
  </xsl:if>
  <!-- Write Shipment PortOfOrigin End-->

  <!-- Write Shipment OuterPacks Begin-->
  <xsl:element name="ns0:OuterPacks">
    <xsl:value-of select="sum(./*[local-name()='GIDLoop1']/*[local-name()='GID']/*[local-name()='C213_2']/*[local-name()='C21301']/text())"/>
  </xsl:element>
  <xsl:element name="ns0:OuterPacksPackageType">
    <xsl:element name="ns0:Code">
      <xsl:value-of select="./*[local-name()='GIDLoop1']/*[local-name()='GID']/*[local-name()='C213_2']/*[local-name()='C21302'][1]/text()"/>
    </xsl:element>
  </xsl:element>
  <!-- Write Shipment OuterPacks End-->

  <!-- Write Shipment TotalVolume Begin-->
  <xsl:element name="ns0:TotalVolume">
    <xsl:value-of select="sum(./*[local-name()='GIDLoop1']/*[local-name()='MEALoop4']/*[local-name()='MEA_5'][*[local-name()='MEA01']/text()='VOL']/*[local-name()='C174_5']/*[local-name()='C17402']/text())"/>
  </xsl:element>
  <xsl:element name="ns0:TotalVolumeUnit">
    <xsl:element name="ns0:Code">
      <xsl:value-of select="./*[local-name()='GIDLoop1']/*[local-name()='MEALoop4']/*[local-name()='MEA_5'][*[local-name()='MEA01']/text()='VOL'][1]/*[local-name()='C174_5']/*[local-name()='C17401']/text()"/>
    </xsl:element>
  </xsl:element>
  <!-- Write Shipment TotalVolume End-->

  <!-- Write Shipment TotalWeight Begin-->
  <xsl:element name="ns0:TotalWeight">
    <xsl:value-of select="sum(./*[local-name()='GIDLoop1']/*[local-name()='MEALoop4']/*[local-name()='MEA_5'][*[local-name()='MEA01']/text()='WT']/*[local-name()='C174_5']/*[local-name()='C17402']/text())"/>
  </xsl:element>
  <xsl:element name="ns0:TotalWeightUnit">
    <xsl:element name="ns0:Code">
      <xsl:value-of select="./*[local-name()='GIDLoop1']/*[local-name()='MEALoop4']/*[local-name()='MEA_5'][*[local-name()='MEA01']/text()='WT'][1]/*[local-name()='C174_5']/*[local-name()='C17401']/text()"/>
    </xsl:element>
  </xsl:element>
  <!-- Write Shipment TotalWeight End-->

  <!-- Write ETD Begin-->
  <!--If the ATD is provided but not ETD, default the ETD from ATD-->
  <xsl:variable name="counterETD" select="count(./*[local-name()='LOCLoop1']/*[local-name()='DTM_8']/*[local-name()='C507_8']/*[local-name()='C50701' and text()='189'])"/>
  <xsl:variable name="counterATD" select="count(./*[local-name()='LOCLoop1']/*[local-name()='DTM_8']/*[local-name()='C507_8']/*[local-name()='C50701' and text()='136'])"/>
  <xsl:choose>
    <xsl:when test="$counterETD>='1'">
      <xsl:element name="ns0:EstimatedDeparture">
        <xsl:value-of select="./*[local-name()='LOCLoop1']/*[local-name()='DTM_8']/*[local-name()='C507_8'][*[local-name()='C50701']/text()='189']/*[local-name()='C50702'][1]" />
      </xsl:element>
    </xsl:when>
    <xsl:otherwise>
      <xsl:if test="$counterATD>='1'">
        <xsl:element name="ns0:EstimatedDeparture">
          <xsl:value-of select="./*[local-name()='LOCLoop1']/*[local-name()='DTM_8']/*[local-name()='C507_8'][*[local-name()='C50701']/text()='136']/*[local-name()='C50702'][1]" />
        </xsl:element>
      </xsl:if>
    </xsl:otherwise>
  </xsl:choose>
  <!-- Write ETD End-->

  <!-- Write Consol's WayBillNumber-->
  <xsl:variable name="masterBillfromConsol" select="./*[local-name()='RFFLoop1']/*[local-name()='RFF']/*[local-name()='C506'][*[local-name()='C50601' and (text()='AWB' or text()='MWB' or text()='BM')]]/*[local-name()='C50602']/text()"/>
  <xsl:choose>
    <xsl:when test="$masterBillfromConsol!=''">
      <xsl:element name="ns0:WayBillNumber">
        <xsl:value-of select="$masterBillfromConsol"/>
      </xsl:element>
    </xsl:when>
    <xsl:otherwise>
      <xsl:element name="ns0:WayBillNumber">
        <xsl:variable name="masterBillfromShipment" select="./*[local-name()='CNILoop1']/*[local-name()='RFFLoop5']/*[local-name()='RFF_6']/*[local-name()='C506_6'][*[local-name()='C50601' and text()='BM']]/*[local-name()='C50602']/text()"/>
        <xsl:value-of select="$masterBillfromShipment"/>

      </xsl:element>
    </xsl:otherwise>
  </xsl:choose>
  <!-- Write Consol's WayBillNumber End-->

  <!--Write LocalProcessing-->
  <xsl:variable name="c50702" select="string(./*[local-name()='DTM_13']/*[local-name()='C507_13']/*[local-name()='C50702']/text())" />
  <xsl:variable name="counterShipmentOrderNumber" select="count(./*[local-name()='RFFLoop5']/*[local-name()='RFF_6']/*[local-name()='C506_6']/*[local-name()='C50601' and text()='CG'])" />
  <xsl:if test="$c50702!='' or $counterShipmentOrderNumber>0">
    <xsl:element name="ns0:LocalProcessing">
      <xsl:if test="$c50702!=''">
        <xsl:element name="ns0:DeliveryRequiredBy">
          <xsl:value-of select="$c50702" />
        </xsl:element>
      </xsl:if>
      <xsl:if test="$counterShipmentOrderNumber>0">
        <xsl:element name="ns0:OrderNumberCollection">
          <xsl:for-each select="./*[local-name()='RFFLoop5']">
            <xsl:for-each select="./*[local-name()='RFF_6']">
              <xsl:variable name="shipmentReferenceQualifier" select="./*[local-name()='C506_6']/*[local-name()='C50601']/text()" />
              <xsl:if test="string($shipmentReferenceQualifier)='CG'">
                <xsl:element name="ns0:OrderNumber">
                  <xsl:element name="ns0:OrderReference">
                    <xsl:value-of select="./*[local-name()='C506_6']/*[local-name()='C50602']/text()" />
                  </xsl:element>
                  <xsl:element name="ns0:Sequence">
                    <xsl:value-of select="userCSharp:GetNextOrderNumberSequence()" />
                  </xsl:element>
                </xsl:element>
              </xsl:if>
            </xsl:for-each>
          </xsl:for-each>
        </xsl:element>
      </xsl:if>
    </xsl:element>
  </xsl:if>
  <!--Write LocalProcessing End-->

  <!-- Write DateCollection-->
  <xsl:variable name="requiredDTMExists" select="count(./*[local-name()='LOCLoop2']/*[local-name()='DTM_14']/*[local-name()='C507_14']/*[local-name()='C50701' and (text()='189' or text()='232')])" />
  <xsl:if test="$requiredDTMExists>0">
    <xsl:element name="ns0:DateCollection">
      <xsl:for-each select="s0:LOCLoop2">
        <xsl:variable name="dtmc50701" select="./*[local-name()='DTM_14']/*[local-name()='C507_14']/*[local-name()='C50701']/text()" />
        <xsl:variable name="dtmc50702" select="./*[local-name()='DTM_14']/*[local-name()='C507_14']/*[local-name()='C50702']/text()" />
        <xsl:if test="$dtmc50701='189' or $dtmc50701='232'">
          <xsl:element name="ns0:Date">
            <xsl:element name="ns0:Type">
              <xsl:if test="$dtmc50701='189'">
                <xsl:text>Departure</xsl:text>
              </xsl:if>
              <xsl:if test="$dtmc50701='232'">
                <xsl:text>Arrival</xsl:text>
              </xsl:if>
            </xsl:element>
            <xsl:element name="ns0:IsEstimate">
              <xsl:text>true</xsl:text>
            </xsl:element>
            <xsl:element name="ns0:Value">
              <xsl:value-of select="$dtmc50702" />
            </xsl:element>
          </xsl:element>
        </xsl:if>
      </xsl:for-each>
    </xsl:element>
  </xsl:if>
  <!-- Write DateCollection End-->

</xsl:stylesheet>


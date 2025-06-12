<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 ns0 userCSharp CodeMapper ContextAccessor DataModelAccessor DateMapper" version="1.0"
                xmlns:s0="http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"
                xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2012/11"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
                xmlns:DataModelAccessor="http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"
                xmlns:DateMapper="http://schemas.microsoft.com/BizTalk/2003/DateMapper">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes" />

  <xsl:template match="/">
    <xsl:apply-templates select="/s0:EFACT_D95B_CODECO" />
  </xsl:template>

  <xsl:variable name="SenderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="UNBSenderID" select="ContextAccessor:GetContextProperty('UNB2_1', 'http://schemas.microsoft.com/Edi/PropertySchema')" />
  <xsl:variable name="UNB31" select="ContextAccessor:GetContextProperty('UNB3_1', 'http://schemas.microsoft.com/Edi/PropertySchema')"/>
  <xsl:variable name="defaultInterfaceId" select="CodeMapper:GetRecipientCode('OCEAN_CONTAINER_TRACING', 'OCEAN_CONTAINER_TRACING', 'OCT System Configuration', 'Default Interface', 'DefaultInterfaceID', $UNBSenderID)" />

  <xsl:template match="/s0:EFACT_D95B_CODECO">
    <xsl:variable name="RecipientIDBySenderID" select="DataModelAccessor:GetClientRegistrationCode($SenderID, $UNB31, 'OCT')" />
    <xsl:variable name="RecipientIDByUNBSenderID" select="DataModelAccessor:GetClientRegistrationCode($UNBSenderID, $UNB31, 'OCT')" />
    <xsl:variable name="RecipientID">
      <xsl:call-template name="GetValue">
        <xsl:with-param name="value1" select="$RecipientIDBySenderID" />
        <xsl:with-param name="fallbackValue" select="$RecipientIDByUNBSenderID" />
      </xsl:call-template>
    </xsl:variable>

    <xsl:if test="$RecipientID=''">
      <xsl:variable name="CheckRecipienID" select="userCSharp:ThrowPartyReceiverIDNotFound($UNBSenderID, $UNB31)" />
    </xsl:if>

    <xsl:variable name="SetRecipientID" select="ContextAccessor:SetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties', $RecipientID)"/>

    <UniversalInterchange xmlns="http://www.cargowise.com/Schemas/Universal/2011/11">
      <Header>
        <SenderID></SenderID>
        <RecipientID></RecipientID>
      </Header>
      <Body>
        <xsl:variable name="nadMS" select="s0:NADLoop1/s0:NAD[NAD01/text()='MS']/s0:C082/C08201/text()" />
        <xsl:variable name="eventTypePortCode">
          <xsl:call-template name="GetValue">
            <!--<xsl:with-param name="value1" select="$defaultInterfaceId" />-->
            <xsl:with-param name="value1" select="$nadMS" />
            <xsl:with-param name="fallbackValue" select="$UNBSenderID" />
          </xsl:call-template>
        </xsl:variable>
        <xsl:variable name="bgmEventCode" select="s0:BGM/s0:C002/C00201/text()" />
        <xsl:variable name="eventType" select="CodeMapper:GetRecipientCode('OCEAN_CONTAINER_TRACING', 'OCEAN_CONTAINER_TRACING', 'OCT System Configuration', 'Event Type', 'Event Type', $eventTypePortCode, $bgmEventCode)" />
        <xsl:variable name="mainTDTLeg" select="s0:TDTLoop1[s0:TDT/TDT01/text()='20']" />
        <xsl:variable name="NADLoop1" select="s0:NADLoop1" />
        <xsl:variable name="GIDLoop1" select="s0:GIDLoop1" />

        <xsl:variable name="codeMappingPortCode">
          <xsl:call-template name="GetValue">
            <xsl:with-param name="value1" select="$defaultInterfaceId" />
            <xsl:with-param name="fallbackValue" select="$SenderID" />
          </xsl:call-template>
        </xsl:variable>

        <xsl:for-each select="s0:EQDLoop1">
          <ns0:UniversalEvent>
            <ns0:Event>
              <ns0:DataContext>
                <ns0:DataTargetCollection>
                  <ns0:DataTarget>
                    <ns0:Type>ContainerStock</ns0:Type>
                  </ns0:DataTarget>
                </ns0:DataTargetCollection>
                <ns0:RecipientRoleCollection>
                  <ns0:RecipientRole>
                    <ns0:Code>CAR</ns0:Code>
                  </ns0:RecipientRole>
                </ns0:RecipientRoleCollection>
              </ns0:DataContext>
              <ns0:EventTime>
                <xsl:value-of select="userCSharp:FormatDateTime(s0:DTM_2/s0:C507_2/C50702/text(), 'yyyyMMddHHmm', 'yyyy-MM-ddTHH:mm:ss')" />
              </ns0:EventTime>

              <ns0:EventType>
                <xsl:value-of select="$eventType"/>
              </ns0:EventType>

              <ns0:CreatedTime>
                <xsl:value-of select="DateMapper:CurrentDateTime('s')"/>
              </ns0:CreatedTime>

              <xsl:variable name="eventParameters" select="CodeMapper:GetRecipientCode('OCEAN_CONTAINER_TRACING', 'OCEAN_CONTAINER_TRACING', 'OCT System Configuration', 'Event Type', 'Event Parameters', $eventTypePortCode, $bgmEventCode)" />
              <ns0:EventParameters>
                <xsl:copy-of select="userCSharp:CreateEventParameters($eventParameters)"/>
                <ns0:Location>
                  <xsl:variable name="LOC165" select="$mainTDTLeg/s0:LOC[LOC01/text()='165']/s0:C517/C51701/text()" />
                  <xsl:variable name="LOC2_165" select="s0:LOC_2[LOC01/text()='165']/s0:C517_2/C51701/text()" />
                  <xsl:variable name="LOC9" select="$mainTDTLeg/s0:LOC[LOC01/text()='9']/s0:C517/C51701/text()" />
                  <xsl:variable name="LOC2_9" select="s0:LOC_2[LOC01/text()='9']/s0:C517_2/C51701/text()" />
                  <xsl:variable name="LOC11" select="$mainTDTLeg/s0:LOC[LOC01/text()='11']/s0:C517/C51701/text()" />
                  <xsl:variable name="LOC2_11" select="s0:LOC_2[LOC01/text()='11']/s0:C517_2/C51701/text()" />
                  <xsl:choose>
                    <xsl:when test="$LOC165!=''">
                      <xsl:value-of select="$LOC165"/>
                    </xsl:when>
                    <xsl:when test="$LOC2_165!=''">
                      <xsl:value-of select="$LOC2_165" />
                    </xsl:when>
                    <xsl:when test="$eventType='GIN' and $LOC9!=''">
                      <xsl:value-of select="$LOC9"/>
                    </xsl:when>
                    <xsl:when test="$eventType='GIN' and $LOC2_9!=''">
                      <xsl:value-of select="$LOC2_9" />
                    </xsl:when>
                    <xsl:when test="$eventType='GOU' and $LOC11!=''">
                      <xsl:value-of select="$LOC11"/>
                    </xsl:when>
                    <xsl:when test="$eventType='GOU' and $LOC2_11!=''">
                      <xsl:value-of select="$LOC2_11" />
                    </xsl:when>
                  </xsl:choose>
                </ns0:Location>
              </ns0:EventParameters>
              <ns0:EventReference>
                <xsl:value-of select="CodeMapper:GetRecipientCode('OCEAN_CONTAINER_TRACING', 'OCEAN_CONTAINER_TRACING', 'OCT System Configuration', 'Event Type', 'Event Reference', $eventTypePortCode, $bgmEventCode)" />
              </ns0:EventReference>
              <ns0:IsEstimate>
                <xsl:value-of select="CodeMapper:GetRecipientCode('OCEAN_CONTAINER_TRACING', 'OCEAN_CONTAINER_TRACING', 'OCT System Configuration', 'Event Type', 'Is Estimate', $eventTypePortCode, $bgmEventCode)" />
              </ns0:IsEstimate>

              <xsl:element name="ns0:ContextCollection">

                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'VoyageNumber'" />
                  <xsl:with-param name="Value" select="$mainTDTLeg/s0:TDT/TDT02/text()" />
                </xsl:call-template>

                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'LloydsNumber'" />
                  <xsl:with-param name="Value" select="$mainTDTLeg/s0:TDT/s0:C222[C22202/text()= '146']/C22201/text()" />
                </xsl:call-template>

                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'VesselCallSign'" />
                  <xsl:with-param name="Value" select="$mainTDTLeg/s0:TDT/s0:C222[C22202/text()= '103']/C22201/text()" />
                </xsl:call-template>

                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'VesselName'" />
                  <xsl:with-param name="Value" select="$mainTDTLeg/s0:TDT/s0:C222/C22204/text()" />
                </xsl:call-template>

                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'LegOriginUNLOCO'" />
                  <xsl:with-param name="Value" select="$mainTDTLeg/s0:LOC[LOC01/text()='9']/s0:C517/C51701/text()" />
                </xsl:call-template>

                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'LegDestinationUNLOCO'" />
                  <xsl:with-param name="Value" select="$mainTDTLeg/s0:LOC[LOC01/text()='11']/s0:C517/C51701/text()" />
                </xsl:call-template>

                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'EstimatedTimeOfArrival'" />
                  <xsl:with-param name="Value" select="userCSharp:FormatDateTime($mainTDTLeg/s0:DTM/s0:C507[C50701/text()='132']/C50702/text(), 'yyyyMMddHHmm', 'yyyy-MM-ddTHH:mm:ss')" />
                </xsl:call-template>

                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'EstimatedTimeOfDeparture'" />
                  <xsl:with-param name="Value" select="userCSharp:FormatDateTime($mainTDTLeg/s0:DTM/s0:C507[C50701/text()='133']/C50702/text(), 'yyyyMMddHHmm', 'yyyy-MM-ddTHH:mm:ss')" />
                </xsl:call-template>

                <xsl:variable name="carrierACOSCode" select="$NADLoop1/s0:NAD[NAD01/text()='CA']/s0:C082[C08203/text()= '184']/C08201/text()" />
                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'CarrierACOSCode'" />
                  <xsl:with-param name="Value" select="$carrierACOSCode" />
                </xsl:call-template>

                <xsl:variable name="messageRecipientName" select="$NADLoop1/s0:NAD[NAD01/text()='MR']/s0:C082[C08203/text()= '87']/C08201/text()" />
                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'MessageRecipientName'" />
                  <xsl:with-param name="Value" select="$messageRecipientName" />
                </xsl:call-template>

                <xsl:variable name="goodsDescription" select="$GIDLoop1/s0:FTX_2[FTX01/text() = 'AAA']/s0:C108_2/C10801/text()"/>
                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'GoodsDescription'" />
                  <xsl:with-param name="Value" select="$goodsDescription" />
                </xsl:call-template>

                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'ContainerNumber'" />
                  <xsl:with-param name="Value" select="s0:EQD/s0:C237_2/C23701/text()" />
                </xsl:call-template>

                <xsl:variable name="containerISO" select="s0:EQD/s0:C224/C22401/text()"/>
                <xsl:variable name="mappedContainerISOByProvider">
                  <xsl:choose>
                    <xsl:when test="$defaultInterfaceId!=''">
                      <xsl:value-of select="CodeMapper:GetRecipientCode($defaultInterfaceId, $defaultInterfaceId, concat($defaultInterfaceId, ' System Configuration'), 'ISOCodeToContainerType', 'ediEnterprise Code', $containerISO)" />
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="CodeMapper:GetRecipientCode($UNBSenderID, $UNBSenderID, concat($UNBSenderID, ' System Configuration'), 'ISOCodeToContainerType', 'ediEnterprise Code', $containerISO)" />
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:variable>
                <xsl:variable name="mappedContainerISO">
                  <xsl:choose>
                    <xsl:when test="$mappedContainerISOByProvider!=''">
                      <xsl:value-of select="$mappedContainerISOByProvider" />
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="CodeMapper:GetRecipientCode('OCEAN_CONTAINER_TRACING', 'OCEAN_CONTAINER_TRACING', 'OCT System Configuration', 'ISOCodeToContainerType', 'ediEnterprise Code', $containerISO)" />
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:variable>
                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'ContainerISOCode'" />
                  <xsl:with-param name="Value">
                    <xsl:call-template name="GetValue">
                      <xsl:with-param name="value1" select="$mappedContainerISO" />
                      <xsl:with-param name="fallbackValue" select="$containerISO" />
                    </xsl:call-template>
                  </xsl:with-param>
                </xsl:call-template>

                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'ContainerOwnershipType'" />
                  <xsl:with-param name="Value" select="CodeMapper:GetRecipientCode('OCEAN_CONTAINER_TRACING', 'OCEAN_CONTAINER_TRACING', 'OCT System Configuration', 'Container Ownership', 'ediEnterprise Code', $codeMappingPortCode, s0:EQD/EQD04/text())" />
                </xsl:call-template>

                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'ContainerMovementType'" />
                  <xsl:with-param name="Value" select="CodeMapper:GetRecipientCode('OCEAN_CONTAINER_TRACING', 'OCEAN_CONTAINER_TRACING', 'OCT System Configuration', 'Container Movement Type', 'ediEnterprise Code', $codeMappingPortCode, s0:EQD/EQD05/text())" />
                </xsl:call-template>

                <xsl:variable name="isEmptyContainer" select="s0:EQD/EQD06/text()" />
                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'IsEmptyContainer'" />
                  <xsl:with-param name="Value">
                    <xsl:choose>
                      <xsl:when test="$isEmptyContainer = 4">true</xsl:when>
                      <xsl:when test="$isEmptyContainer = 5">false</xsl:when>
                    </xsl:choose>
                  </xsl:with-param>
                </xsl:call-template>

                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'ContainerMode'" />
                  <xsl:with-param name="Value" select="CodeMapper:GetRecipientCode('OCEAN_CONTAINER_TRACING', 'OCEAN_CONTAINER_TRACING', 'OCT System Configuration', 'Container Mode', 'ediEnterprise Code', $codeMappingPortCode, s0:TMD/s0:C219/C21901/text())" />
                </xsl:call-template>

                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'MBOLNumber'" />
                  <xsl:with-param name="Value" select="s0:RFF_3/s0:C506_3[C50601/text()='BM']/C50602/text()" />
                </xsl:call-template>

                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'CarriersBookingReference'" />
                  <xsl:with-param name="Value" select="s0:RFF_3/s0:C506_3[C50601/text()='BN']/C50602/text()" />
                </xsl:call-template>

                <xsl:variable name="ref_AAE" select="s0:RFF_3/s0:C506_3[C50601/text()='AAE']/C50602/text()" />
                <xsl:if test="$ref_AAE!=''">
                  <xsl:call-template name="GenerateContext">
                    <xsl:with-param name="Type" select="'EntryNumber'" />
                    <xsl:with-param name="Value" select="$ref_AAE" />
                  </xsl:call-template>

                  <xsl:call-template name="GenerateContext">
                    <xsl:with-param name="Type" select="'EntryNumberType'" />
                    <xsl:with-param name="Value" select="'CAN'" />
                  </xsl:call-template>

                  <xsl:call-template name="GenerateContext">
                    <xsl:with-param name="Type" select="'EntryNumberCountryOfIssue'" />
                    <xsl:with-param name="Value" select="'AU'" />
                  </xsl:call-template>
                </xsl:if>

                <xsl:for-each select="s0:LOC_2">
                  <xsl:variable name="unlocoContextName">
                    <xsl:choose>
                      <xsl:when test="LOC01/text()='8'">ContainerDestinationUNLOCO</xsl:when>
                      <xsl:when test="LOC01/text()='9'">MBOLOriginUNLOCO</xsl:when>
                      <xsl:when test="LOC01/text()='11'">MBOLDestinationUNLOCO</xsl:when>
                      <xsl:when test="LOC01/text()='147'">ContainerStowageCell</xsl:when>
                      <xsl:when test="LOC01/text()='164'">HBOLDestinationUNLOCO</xsl:when>
                    </xsl:choose>
                  </xsl:variable>

                  <xsl:if test="$unlocoContextName!=''">
                    <xsl:call-template name="GenerateContext">
                      <xsl:with-param name="Type" select="$unlocoContextName" />
                      <xsl:with-param name="Value" select="s0:C517_2/C51701/text()" />
                    </xsl:call-template>
                  </xsl:if>

                  <xsl:if test="LOC01/text()='165'">
                    <xsl:call-template name="GenerateContext">
                      <xsl:with-param name="Type" select="'ContainerTerminalID'" />
                      <xsl:with-param name="Value" select="s0:C519_2[C51902/text()='TER']/C51901/text()" />
                    </xsl:call-template>

                    <xsl:call-template name="GenerateContext">
                      <xsl:with-param name="Type" select="'ContainerStorageFacilityCode'" />
                      <xsl:with-param name="Value" select="s0:C519_2[C51902/text()='STO' and C51903/text() = '']/C51901/text()" />
                    </xsl:call-template>

                    <xsl:call-template name="GenerateContext">
                      <xsl:with-param name="Type" select="'ContainerStorageFacilityACOSCode'" />
                      <xsl:with-param name="Value" select="s0:C519_2[C51902/text()='STO' and C51903/text() = '184']/C51901/text()" />
                    </xsl:call-template>
                  </xsl:if>
                </xsl:for-each>

                <xsl:variable name="containerWeightMEA" select="s0:MEA_2[MEA01/text()='AAE' and s0:C502_2/C50201/text()='G']" />
                <xsl:variable name="containerWeight" select="$containerWeightMEA/s0:C174_2/C17402/text()" />
                <xsl:if test="$containerWeight!=''">
                  <xsl:variable name="measurementUnit" select="CodeMapper:GetRecipientCode('OCEAN_CONTAINER_TRACING', 'OCEAN_CONTAINER_TRACING', 'OCT System Configuration', 'Unit of Measurement', 'ediEnterprise Code', $codeMappingPortCode, $containerWeightMEA/s0:C174_2/C17401/text())" />
                  <xsl:call-template name="GenerateContext">
                    <xsl:with-param name="Type" select="'ContainerGrossWeight'" />
                    <xsl:with-param name="Value" select="normalize-space(concat($containerWeight, ' ', $measurementUnit))" />
                  </xsl:call-template>
                </xsl:if>

                <xsl:for-each select="s0:SEL">
                  <xsl:variable name="sealNumber" select="SEL01/text()" />

                  <xsl:if test="$sealNumber!=''">
                    <xsl:variable name="rowID" select="position()" />
                    <xsl:variable name="sealParty" select="s0:C215/C21501/text()" />

                    <xsl:variable name="containerSealContextType">
                      <xsl:choose>
                        <xsl:when test="$rowID=1">ContainerSealNo</xsl:when>
                        <xsl:otherwise>
                          <xsl:value-of select="concat('ContainerSealNo', $rowID)" />
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>
                    <xsl:call-template name="GenerateContext">
                      <xsl:with-param name="Type" select="$containerSealContextType" />
                      <xsl:with-param name="Value" select="$sealNumber" />
                    </xsl:call-template>

                    <xsl:variable name="ContainerSealIssuerContextType">
                      <xsl:choose>
                        <xsl:when test="$rowID=1">ContainerSealIssuer</xsl:when>
                        <xsl:otherwise>
                          <xsl:value-of select="concat('ContainerSealIssuer', $rowID)" />
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>
                    <xsl:call-template name="GenerateContext">
                      <xsl:with-param name="Type" select="$ContainerSealIssuerContextType" />
                      <xsl:with-param name="Value">
                        <xsl:choose>
                          <xsl:when test="$sealParty='CA'">Carrier</xsl:when>
                          <xsl:when test="$sealParty='SH'">Shipper</xsl:when>
                        </xsl:choose>
                      </xsl:with-param>
                    </xsl:call-template>
                  </xsl:if>
                </xsl:for-each>

                <xsl:variable name="containerGoodsDescription" select="s0:FTX_4[FTX01/text()='AAA']/s0:C108_4/C10801/text()" />
                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'ContainerGoodsDescription'" />
                  <xsl:with-param name="Value" select="$containerGoodsDescription" />
                </xsl:call-template>

                <xsl:variable name="ftxDAR" select="s0:FTX_4[FTX01/text()='DAR']" />
                <xsl:if test="$ftxDAR">
                  <xsl:variable name="containerDamageContextType">
                    <xsl:choose>
                      <xsl:when test="$ftxDAR/s0:C107_4/C10703/text()='184'">ContainerDamageACOSCode</xsl:when>
                      <xsl:otherwise>ContainerDamageCode</xsl:otherwise>
                    </xsl:choose>
                  </xsl:variable>

                  <xsl:call-template name="GenerateContext">
                    <xsl:with-param name="Type" select="$containerDamageContextType" />
                    <xsl:with-param name="Value" select="$ftxDAR/s0:C107_4/C10701/text()" />
                  </xsl:call-template>
                </xsl:if>

                <xsl:variable name="ftxABS" select="s0:FTX_4[FTX01/text()='ABS']" />
                <xsl:if test="$ftxABS">
                  <xsl:variable name="containerConditionContextType">
                    <xsl:choose>
                      <xsl:when test="$ftxABS/s0:C107_4/C10703/text()='184'">ContainerConditionACOSCode</xsl:when>
                      <xsl:otherwise>ContainerConditionCode</xsl:otherwise>
                    </xsl:choose>
                  </xsl:variable>

                  <xsl:call-template name="GenerateContext">
                    <xsl:with-param name="Type" select="$containerConditionContextType" />
                    <xsl:with-param name="Value" select="$ftxABS/s0:C107_4/C10701/text()" />
                  </xsl:call-template>
                </xsl:if>

                <xsl:variable name="containerOwnerName">
                  <xsl:call-template name="GetValue">
                    <xsl:with-param name="value1" select="s0:NAD_2[NAD01/text()='CF']/s0:C082_2[C08203/text()='184']/C08201/text()" />
                    <xsl:with-param name="fallbackValue" select="$NADLoop1/s0:NAD[NAD01/text()='CF']/s0:C082[C08203/text()='184']/C08201/text()" />
                  </xsl:call-template>
                </xsl:variable>
                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'ContainerOwnerName'" />
                  <xsl:with-param name="Value" select="$containerOwnerName" />
                </xsl:call-template>

                <xsl:variable name="containerConsignorName" select="s0:NAD_2[NAD01/text()='CZ']/s0:C082_2/C08201/text()" />
                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'ContainerConsignorName'" />
                  <xsl:with-param name="Value" select="$containerConsignorName" />
                </xsl:call-template>

                <xsl:variable name="nadConsignee" select="s0:NAD_2[NAD01/text()='CN']" />
                <xsl:variable name="containerConsigneeName">
                  <xsl:call-template name="GetValue">
                    <xsl:with-param name="value1" select="$nadConsignee/s0:C082_2/C08201/text()" />
                    <xsl:with-param name="value2" select="$nadConsignee/s0:C058_2/C05801/text()" />
                    <xsl:with-param name="fallbackValue" select="$nadConsignee/s0:C080_2/C08001/text()" />
                  </xsl:call-template>
                </xsl:variable>
                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'ContainerConsigneeName'" />
                  <xsl:with-param name="Value" select="$containerConsigneeName" />
                </xsl:call-template>

                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'DepotCode'" />
                  <xsl:with-param name="Value">
                    <xsl:call-template name="GetValue">
                      <xsl:with-param name="value1" select="$nadMS" />
                      <xsl:with-param name="fallbackValue" select="$UNBSenderID" />
                    </xsl:call-template>
                  </xsl:with-param>
                </xsl:call-template>

                <xsl:variable name="tdtOnCarriageRoadTruck" select="s0:TDTLoop2[s0:TDT_2/TDT01/text()='30' and s0:TDT_2/s0:C220_2/C22001/text()='3' and s0:TDT_2/s0:C228_2/C22801/text()='31']" />
                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'PlaceOfFinalOnCarriageDestination'" />
                  <xsl:with-param name="Value" select="$tdtOnCarriageRoadTruck/s0:LOC_3[LOC01/text()='7']/s0:C517_3/C51701/text()" />
                </xsl:call-template>
              </xsl:element>

            </ns0:Event>
          </ns0:UniversalEvent>
        </xsl:for-each>
      </Body>
    </UniversalInterchange>
  </xsl:template>

  <xsl:template name="GenerateContext">
    <xsl:param name="Type" />
    <xsl:param name="Value" />

    <xsl:if test="$Value != ''">
      <xsl:element name="ns0:Context">
        <xsl:element name="ns0:Type">
          <xsl:value-of select="$Type" />
        </xsl:element>
        <xsl:element name="ns0:Value">
          <xsl:value-of select="$Value" />
        </xsl:element>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="GetValue">
    <xsl:param name="value1" />
    <xsl:param name="value2" select="''" />
    <xsl:param name="fallbackValue" select="''"/>

    <xsl:choose>
      <xsl:when test="$value1!=''">
        <xsl:value-of select="$value1"/>
      </xsl:when>
      <xsl:when test="$value2!=''">
        <xsl:value-of select="$value2"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$fallbackValue"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[
public string FormatDateTime(string val, string inFmts, string outFmt)
{
    DateTime parsedDate;
    if (DateTime.TryParseExact(val, inFmts.Split(new char[] {';'}), System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out parsedDate))
    {
        return parsedDate.ToString(outFmt);
    }
    else	{
        return string.Empty;
    }
}

public string ThrowPartyReceiverIDNotFound(string unb2, string unb3)
{
  throw new ArgumentException(string.Format(@"Could not found matching PartyReceiverID in the Client Registration Lookup.(Client Registration: OCT, SenderID:[UNB2:{0}] RecipientID: [UNB3:{1}])", unb2, unb3));
}

public XPathNodeIterator CreateEventParameters(string parameters)
{
  var doc = new XmlDocument();
  var root = doc.CreateElement("root");
  doc.AppendChild(root);

  if (parameters != "")
  {
    foreach (var parameter in parameters.Split('|'))
    {
      var id_value = parameter.Split('=');
      if (id_value.Length == 2)
      {
        var child = doc.CreateElement("ns0", id_value[0], "http://www.cargowise.com/Schemas/Universal/2012/11");
        child.InnerText = id_value[1];
        root.AppendChild(child);
      }
    }
  }

  return doc.CreateNavigator().Select("/*/*");
}

]]>
  </msxsl:script>
</xsl:stylesheet>

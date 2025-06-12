<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet version="1.0"
                exclude-result-prefixes="s0 CodeMapper DateMapper"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:s0="http://cargowise.com/ehub/clients/edi/edifact/2012/03"
                xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:DateMapper="http://schemas.microsoft.com/BizTalk/2003/DateMapper">
  
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes"/>
  
  <xsl:template match="/">
    <xsl:apply-templates select="/s0:EFACT_D95B_CODECO"/>
  </xsl:template>
  
  <xsl:template match="/s0:EFACT_D95B_CODECO">
    <ns0:UniversalInterchange>
      <Header>
        <SenderID/>
        <RecipientID/>
      </Header>
      <Body>
        <xsl:for-each select="//s0:EQDLoop1">
          <ns0:UniversalEvent>
            <ns0:Event>
              <ns0:DataContext>
                <ns0:RecipientRoleCollection>
                  <ns0:RecipientRole>
                    <ns0:Code>CAR</ns0:Code>
                  </ns0:RecipientRole>
                </ns0:RecipientRoleCollection>
              </ns0:DataContext>
              <ns0:EventTime>
                <xsl:value-of select="DateMapper:ConvertToDateTimeString(s0:DTM_2/s0:C507_2/C50702/text(), 'yyyyMMddHHmm', 'yyyy-MM-ddTHH:mm:ss')"/>
              </ns0:EventTime>
              <ns0:EventType>
                <xsl:value-of select="CodeMapper:GetRecipientCode('POROFAAKL', 'POROFAAKL', 'Ocean Containers - CODECO to UniversalEvent', 'Event Type', 'ediEnterprise code', //s0:BGM/s0:C002/C00201/text())"/>
              </ns0:EventType>
              <xsl:variable name="DataProvider" select="//s0:NADLoop1/s0:NAD[NAD01/text() = 'MS']/s0:C082/C08201/text()"/>
              <xsl:if test="$DataProvider != ''">
                <xsl:element name="ns0:DataProvider">
                  <xsl:value-of select="$DataProvider"/>
                </xsl:element>
              </xsl:if>
              <xsl:element name="ns0:ContextCollection">
                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'VoyageNumber'"/>
                  <xsl:with-param name="Value" select="//s0:TDT/TDT02/text()"/>
                </xsl:call-template>
                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'LloydsNumber'"/>
                  <xsl:with-param name="Value" select="//s0:TDT/s0:C222[C22202/text() = '146']/C22201/text()"/>
                </xsl:call-template>
                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'VesselCallSign'"/>
                  <xsl:with-param name="Value" select="//s0:TDT/s0:C222[C22202/text() = '103']/C22201/text()"/>
                </xsl:call-template>
                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'VesselName'"/>
                  <xsl:with-param name="Value" select="//s0:TDT/s0:C222/C22204/text()"/>
                </xsl:call-template>
                <xsl:for-each select="//s0:LOC">
                  <xsl:variable name="LOC01" select="LOC01/text()"/>
                  <xsl:choose>
                    <xsl:when test="$LOC01 = 9">
                      <xsl:call-template name="GenerateContext">
                        <xsl:with-param name="Type" select="'LegOriginUNLOCO'"/>
                        <xsl:with-param name="Value" select="s0:C517/C51701/text()"/>
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:when test="$LOC01 = 11">
                      <xsl:call-template name="GenerateContext">
                        <xsl:with-param name="Type" select="'LegDestinationUNLOCO'"/>
                        <xsl:with-param name="Value" select="s0:C517/C51701/text()"/>
                      </xsl:call-template>
                    </xsl:when>
                  </xsl:choose>
                </xsl:for-each>
                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'EstimatedTimeOfArrival'"/>
                  <xsl:with-param name="Value" select="DateMapper:ConvertToDateTimeString(//s0:DTM/s0:C507[C50701/text() = '132']/C50702/text(), 'yyyyMMddHHmm', 'yyyy-MM-ddTHH:mm:ss')"/>
                </xsl:call-template>
                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'EstimatedTimeOfDeparture'"/>
                  <xsl:with-param name="Value" select="DateMapper:ConvertToDateTimeString(//s0:DTM/s0:C507[C50701/text() = '133']/C50702/text(), 'yyyyMMddHHmm', 'yyyy-MM-ddTHH:mm:ss')"/>
                </xsl:call-template>
                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'CarrierACOSCode'"/>
                  <xsl:with-param name="Value" select="//s0:NAD[NAD01/text() = 'CA']/s0:C082[C08203/text() = '184']/C08201/text()"/>
                </xsl:call-template>
                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'MessageRecipientName'"/>
                  <xsl:with-param name="Value" select="//s0:NAD[NAD01/text() = 'MR']/s0:C082[C08203/text() = '87']/C08201/text()"/>
                </xsl:call-template>
                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'GoodsDescription'"/>
                  <xsl:with-param name="Value" select="//s0:FTX_2[FTX01/text() = 'AAA']/s0:C108_2/C10801/text()"/>
                </xsl:call-template>
                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'ContainerNumber'"/>
                  <xsl:with-param name="Value" select="s0:EQD/s0:C237_2/C23701/text()"/>
                </xsl:call-template>
                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'ContainerISOCode'"/>
                  <xsl:with-param name="Value" select="s0:EQD/s0:C224/C22401/text()"/>
                </xsl:call-template>
                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'ContainerOwnershipType'"/>
                  <xsl:with-param name="Value" select="CodeMapper:GetRecipientCode('POROFAAKL', 'POROFAAKL', 'Ocean Containers - CODECO to UniversalEvent', 'Container Ownership', 'ediEnterprise code', s0:EQD/EQD04/text())"/>
                </xsl:call-template>
                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'ContainerMovementType'"/>
                  <xsl:with-param name="Value" select="CodeMapper:GetRecipientCode('POROFAAKL', 'POROFAAKL', 'Ocean Containers - CODECO to UniversalEvent', 'Container Movement Type', 'ediEnterprise code', s0:EQD/EQD05/text())"/>
                </xsl:call-template>
                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'IsEmptyContainer'"/>
                  <xsl:with-param name="Value">
                    <xsl:variable name="IsEmptyContainer" select="s0:EQD/EQD06/text()"/>
                    <xsl:choose>
                      <xsl:when test="$IsEmptyContainer = 4">
                        <xsl:text>true</xsl:text>
                      </xsl:when>
                      <xsl:when test="$IsEmptyContainer = 5">
                        <xsl:text>false</xsl:text>
                      </xsl:when>
                    </xsl:choose>
                  </xsl:with-param>
                </xsl:call-template>
                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'ContainerMode'"/>
                  <xsl:with-param name="Value" select="CodeMapper:GetRecipientCode('POROFAAKL', 'POROFAAKL', 'Ocean Containers - CODECO to UniversalEvent', 'Container Mode', 'ediEnterprise code', s0:TMD/s0:C219/C21901/text())"/>
                </xsl:call-template>
                <xsl:for-each select="s0:RFF_3/s0:C506_3">
                  <xsl:variable name="ReferenceQualifier" select="C50601/text()"/>
                  <xsl:if test="$ReferenceQualifier != ''">
                    <xsl:variable name="ReferenceName">
                      <xsl:choose>
                        <xsl:when test="$ReferenceQualifier = 'BM'">MBOLNumber</xsl:when>
                        <xsl:when test="$ReferenceQualifier = 'BN'">CarriersBookingReference</xsl:when>
                        <xsl:when test="$ReferenceQualifier = 'AAE'">EntryNumber</xsl:when>
                      </xsl:choose>
                    </xsl:variable>
                    <xsl:call-template name="GenerateContext">
                      <xsl:with-param name="Type" select="$ReferenceName"/>
                      <xsl:with-param name="Value" select="C50602/text()"/>
                    </xsl:call-template>
                    <xsl:if test="$ReferenceQualifier = 'AAE'">
                      <xsl:call-template name="GenerateContext">
                          <xsl:with-param name="Type" select="'EntryNumberType'"/>
                          <xsl:with-param name="Value" select="'CAN'"/>
                        </xsl:call-template>
                        <xsl:call-template name="GenerateContext">
                          <xsl:with-param name="Type" select="'EntryNumberCountryOfIssue'"/>
                          <xsl:with-param name="Value" select="'AU'"/>
                        </xsl:call-template>
                    </xsl:if>
                  </xsl:if>
                </xsl:for-each>
                <xsl:for-each select="s0:LOC_2">
                  <xsl:variable name="LOC01" select="LOC01/text()"/>
                  <xsl:variable name="LocationName">
                    <xsl:choose>
                      <xsl:when test="$LOC01 = 8">ContainerDestinationUNLOCO</xsl:when>
                      <xsl:when test="$LOC01 = 9">MBOLOriginUNLOCO</xsl:when>
                      <xsl:when test="$LOC01 = 11">MBOLDestinationUNLOCO</xsl:when>
                      <xsl:when test="$LOC01 = 147">ContainerStowageCell</xsl:when>
                      <xsl:when test="$LOC01 = 164">HBOLDestinationUNLOCO</xsl:when>
                      <xsl:when test="$LOC01 = 165">EventActionUNLOCO</xsl:when>
                    </xsl:choose>
                  </xsl:variable>
                  <xsl:call-template name="GenerateContext">
                    <xsl:with-param name="Type" select="$LocationName"/>
                    <xsl:with-param name="Value" select="s0:C517_2/C51701/text()"/>
                  </xsl:call-template>
                  <xsl:if test="$LOC01 = 165">
                    <xsl:call-template name="GenerateContext">
                      <xsl:with-param name="Type" select="'ContainerTerminalID'"/>
                      <xsl:with-param name="Value" select="s0:C519_2[C51902/text() = 'TER']/C51901/text()"/>
                    </xsl:call-template>
                    <xsl:call-template name="GenerateContext">
                      <xsl:with-param name="Type" select="'ContainerStorageFacilityCode'"/>
                      <xsl:with-param name="Value" select="s0:C519_2[C51902/text() = 'STO' and C51903/text() = '']/C51901/text()"/>
                    </xsl:call-template>
                    <xsl:call-template name="GenerateContext">
                      <xsl:with-param name="Type" select="'ContainerStorageFacilityACOSCode'"/>
                      <xsl:with-param name="Value" select="s0:C519_2[C51902/text() = 'STO' and C51903/text() = '184']/C51901/text()"/>
                    </xsl:call-template>
                  </xsl:if>
                </xsl:for-each>
                <xsl:variable name="Weight" select="s0:MEA_2/s0:C174_2/C17402/text()"/>
                <xsl:if test="$Weight != '' and s0:MEA_2/MEA01/text() = 'AAE' and s0:MEA_2/s0:C502_2/C50201/text() = 'G'">
                  <xsl:variable name="Unit" select="CodeMapper:GetRecipientCode('POROFAAKL', 'POROFAAKL', 'Ocean Containers - CODECO to UniversalEvent', 'Unit of Measurement', 'ediEnterprise code', s0:MEA_2/s0:C174_2/C17401/text())"/>
                  <xsl:call-template name="GenerateContext">
                    <xsl:with-param name="Type" select="'ContainerGrossWeight'"/>
                    <xsl:with-param name="Value" select="normalize-space(concat($Weight, ' ', $Unit))"/>
                  </xsl:call-template>
                </xsl:if>
                <xsl:for-each select="s0:SEL">
                  <xsl:variable name="count">
                    <xsl:choose>
                      <xsl:when test="position() &gt; 1">
                        <xsl:value-of select="position()"/>
                      </xsl:when>
                    </xsl:choose>
                  </xsl:variable>
                  <xsl:call-template name="GenerateContext">
                    <xsl:with-param name="Type" select="concat('ContainerSealNo', $count)"/>
                    <xsl:with-param name="Value" select="SEL01/text()"/>
                  </xsl:call-template>
                  <xsl:variable name="ContainerSealIssuer" select="s0:C215/C21501/text()"/>
                  <xsl:variable name="ContainerSealIssuerValue">
                    <xsl:choose>
                      <xsl:when test="$ContainerSealIssuer = 'CA'">Carrier</xsl:when>
                      <xsl:when test="$ContainerSealIssuer = 'SH'">Shipper</xsl:when>
                    </xsl:choose>
                  </xsl:variable>
                  <xsl:call-template name="GenerateContext">
                    <xsl:with-param name="Type" select="concat('ContainerSealIssuer', $count)"/>
                    <xsl:with-param name="Value" select="$ContainerSealIssuerValue"/>
                  </xsl:call-template>
                </xsl:for-each>
                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'ContainerGoodsDescription'"/>
                  <xsl:with-param name="Value" select="s0:FTX_4[FTX01/text() = 'AAA']/s0:C108_4/C10801/text()"/>
                </xsl:call-template>
                <xsl:for-each select="s0:FTX_4">
                  <xsl:variable name="FTX01" select="FTX01/text()"/>
                  <xsl:variable name="FTX01Code" select="s0:C107_4/C10703/text()"/>
                  <xsl:variable name="Name">
                    <xsl:choose>
                      <xsl:when test="$FTX01 = 'DAR'">
                        <xsl:choose>
                          <xsl:when test="$FTX01Code = '184'">ContainerDamageACOSCode</xsl:when>
                            <xsl:otherwise>ContainerDamageCode</xsl:otherwise>
                        </xsl:choose>
                      </xsl:when>
                      <xsl:when test="$FTX01 = 'ABS'">
                        <xsl:choose>
                         <xsl:when test="$FTX01Code = '184'">ContainerConditionACOSCode</xsl:when>
                            <xsl:otherwise>ContainerConditionCode</xsl:otherwise>
                        </xsl:choose>
                      </xsl:when>
                    </xsl:choose>
                  </xsl:variable>
                  <xsl:call-template name="GenerateContext">
                    <xsl:with-param name="Type" select="$Name"/>
                    <xsl:with-param name="Value" select="s0:C107_4/C10701/text()"/>
                  </xsl:call-template>
                </xsl:for-each>
                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'ContainerOwnerName'"/>
                  <xsl:with-param name="Value">
                    <xsl:variable name="partyCF_1" select="s0:NAD_2[NAD01/text() = 'CF']/s0:C082_2[C08203/text() = '184']/C08201/text()"/>
                    <xsl:variable name="partyCF_2" select="//s0:NAD[NAD01/text() = 'CF']/s0:C082[C08203/text() = '184']/C08201/text()"/>
                    <xsl:choose>
                      <xsl:when test="$partyCF_1 != ''">
                        <xsl:value-of select="$partyCF_1"/>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="$partyCF_2"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:with-param>
                </xsl:call-template>
                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'ContainerConsignorName'"/>
                  <xsl:with-param name="Value" select="s0:NAD_2[NAD01/text() = 'CZ']/s0:C082_2/C08201/text()"/>
                </xsl:call-template>
                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'ContainerConsigneeName'"/>
                  <xsl:with-param name="Value">
                    <xsl:variable name="partyCN_1" select="s0:NAD_2[NAD01/text() = 'CN']/s0:C082_2/C08201/text()"/>
                    <xsl:variable name="partyCN_2" select="s0:NAD_2[NAD01/text() = 'CN']/s0:C058_2/C05801/text()"/>
                    <xsl:variable name="partyCN_3" select="s0:NAD_2[NAD01/text() = 'CN']/s0:C080_2/C08001/text()"/>
                    <xsl:choose>
                      <xsl:when test="$partyCN_1 != ''">
                        <xsl:value-of select="$partyCN_1"/>
                      </xsl:when>
                      <xsl:when test="$partyCN_2 != ''">
                        <xsl:value-of select="$partyCN_2"/>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="$partyCN_3"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:with-param>
                </xsl:call-template>
                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'PlaceOfFinalOnCarriageDestination'"/>
                  <xsl:with-param name="Value" select="s0:TDTLoop2[s0:TDT_2/TDT01/text() = '30' and s0:TDT_2/s0:C220_2/C22001/text() = '3' and s0:TDT_2/s0:C228_2/C22801/text() = '31']/s0:LOC_3[LOC01/text() = '7']/s0:C517_3/C51701/text()"/>
                </xsl:call-template>
              </xsl:element>
            </ns0:Event>
          </ns0:UniversalEvent>
        </xsl:for-each>
      </Body>
    </ns0:UniversalInterchange>
  </xsl:template>
  
  <xsl:template name="GenerateContext">
    <xsl:param name="Type"/>
    <xsl:param name="Value"/>
    
    <xsl:if test="$Value != ''">
      <xsl:element name="ns0:Context">
        <xsl:element name="ns0:Type">
          <xsl:value-of select="$Type"/>
        </xsl:element>
        <xsl:element name="ns0:Value">
          <xsl:value-of select="$Value"/>
        </xsl:element>
      </xsl:element>
    </xsl:if>
  </xsl:template>
</xsl:stylesheet>
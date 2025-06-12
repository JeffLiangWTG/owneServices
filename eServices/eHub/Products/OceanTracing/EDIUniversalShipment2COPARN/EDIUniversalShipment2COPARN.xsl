<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var"
                exclude-result-prefixes="msxsl var s0 CodeMapper userCSharp DateMapper ContextAccessor"
                version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2012/11"
                xmlns:ns0="http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp"
                xmlns:DateMapper="http://schemas.microsoft.com/BizTalk/2003/DateMapper"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor">

  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes" />
  <xsl:template match="/">
    <xsl:apply-templates select="/s0:UniversalShipment" />
  </xsl:template>
  <xsl:template match="/s0:UniversalShipment">

    <xsl:variable name="shipment" select="s0:Shipment" />
    <xsl:variable name="mainLeg" select="$shipment/s0:TransportLegCollection/s0:TransportLeg[s0:LegType/text()='Main'][1]" />
    <xsl:variable name="eventReference" select="$shipment/s0:DataContext/s0:Workflow/s0:EventReference/text()" />
    <xsl:variable name="dataSourceKey" select="$shipment/s0:DataContext/s0:DataSource/s0:Key/text()" />

    <xsl:variable name="PortOfLoading" select="$mainLeg/s0:PortOfLoading/text()" />
    <xsl:variable name="PortOfDischarge" select="$mainLeg/s0:PortOfDischarge/text()" />
    <xsl:variable name="SourceParty" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
    <xsl:variable name="DestinationParty" select="CodeMapper:GetRecipientCode('ShippingPortMessaging', 'ShippingPortMessaging', 'ShippingPortMessaging COPARN', 'Ports', 'DestinationParty', $PortOfLoading)" />
    <xsl:variable name="SetDestinationParty" select="ContextAccessor:SetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties', $DestinationParty)"/>
    <xsl:variable name="SetDestinationPartyName" select="ContextAccessor:SetContextProperty('DestinationPartyName', 'http://schemas.microsoft.com/Edi/PropertySchema', $DestinationParty)"/>
    <xsl:variable name="OverrideEDIHeader" select="ContextAccessor:SetContextProperty('OverrideEDIHeader', 'http://schemas.microsoft.com/BizTalk/2006/edi-properties', 'true')" />
    <xsl:variable name="DestinationPartySenderIdentifier" select="ContextAccessor:SetContextProperty('UNB2_1' , 'http://schemas.microsoft.com/BizTalk/2006/edi-properties' , substring($SourceParty, 1, 3))" />
    <xsl:variable name="goodsDesciption"  select="normalize-space($shipment/s0:GoodsDescription/text())" />

    <xsl:variable name="OperationalPort_Code" select="$shipment/s0:AddInfoCollection/s0:AddInfo[s0:Key='OperationalPort_Code']" />
    <xsl:variable name="operationalPortCountry">
      <xsl:choose>
        <xsl:when test="$OperationalPort_Code">
          <xsl:value-of select="substring($OperationalPort_Code/s0:Value/text(), 1, 2)"/>
        </xsl:when>
        <xsl:otherwise>NZ</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="initCumulativeConcat" select="userCSharp:InitCumulativeConcat(0)" />
    <xsl:for-each select="$shipment/s0:ContainerCollection/s0:Container">
      <xsl:variable name="calculateFirstRelease" select="userCSharp:CalculateFirstRelease(s0:ReleaseNum/text())" />
      <xsl:variable name="addToCumulativeConcat" select="userCSharp:AddToCumulativeConcat(0,s0:ReleaseNum/text(),'1000')" />
    </xsl:for-each>

    <xsl:variable name="releaseNums" select="userCSharp:GetCumulativeConcat(0)" />
    <xsl:variable name="cfsReference" select="$shipment/s0:CFSReference/text()" />
    <xsl:variable name="bookingConfirmationReference" select="$shipment/s0:BookingConfirmationReference/text()" />
    <xsl:variable name="reference" select="normalize-space(userCSharp:Coalesce($cfsReference, $bookingConfirmationReference))" />
    <xsl:variable name="recipientType" select="$shipment/s0:DataContext/s0:Workflow/s0:RecipientRoleCollection/s0:RecipientRole/text()" />
    <xsl:variable name="messageType" select="userCSharp:GetTypParam($eventReference, $recipientType, $releaseNums)" />
    <xsl:variable name="releaseNum" select="userCSharp:GetReleaseNum($eventReference)" />
    <xsl:variable name="mrn" select="userCSharp:CreateMRN($dataSourceKey, $releaseNum, $recipientType)" />

    <xsl:for-each select="$shipment/s0:ContainerCollection/s0:Container">
      <xsl:variable name="includeContainer" select="userCSharp:IncludeContainer($releaseNum, s0:ReleaseNum/text(), $recipientType)" />
      <xsl:if test="$includeContainer">
        <xsl:variable name="addToCount" select="userCSharp:AddToCount(s0:ContainerNumber/text(), s0:ContainerType/s0:ISOCode/text(), s0:IsEmptyContainer/text(), s0:RemptyRequired/text(), s0:SetPointTemp/text(), s0:SetPointTempUnit/text(), s0:ContainerCount/text())" />
      </xsl:if>
    </xsl:for-each>

    <ns0:EFACT_D00B_COPARN>
      <UNH>
        <UNH1>
          <xsl:value-of select="CodeMapper:CallActionProcedureHelper('GetCounterValue', '@value', '@name', 'CargoWise.eHub.Products.OceanTracing.Transforms.COPARN', '@maxlength', '14')" />
        </UNH1>
        <UNH2>
          <UNH2.1>COPARN</UNH2.1>
          <UNH2.2>D</UNH2.2>
          <UNH2.3>00B</UNH2.3>
          <UNH2.4>UN</UNH2.4>
          <UNH2.5>SMDG20</UNH2.5>
        </UNH2>
      </UNH>
      <ns0:BGM>
        <ns0:C002>
          <C00201>104</C00201>
        </ns0:C002>
        <ns0:C106>
          <C10601>
            <xsl:choose>
              <xsl:when test="$messageType='9'">
                <xsl:value-of select="userCSharp:StringRight($mrn, '35')" />
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="CodeMapper:CallActionProcedureHelper('GetCounterValue' , '@value' , '@name' , 'CargoWise.eHub.Products.OceanTracing.Transforms.COPARN' , '@maxlength' , '14')" />
              </xsl:otherwise>
            </xsl:choose>
          </C10601>
        </ns0:C106>
        <BGM03>
          <xsl:value-of select="$messageType" />
        </BGM03>
        <BGM04>AB</BGM04>
      </ns0:BGM>
      <ns0:DTM>
        <ns0:C507>
          <C50701>137</C50701>
          <C50702>
            <xsl:value-of select="DateMapper:CurrentDateTime('yyyyMMddHHmm')" />
          </C50702>
          <C50703>203</C50703>
        </ns0:C507>
      </ns0:DTM>

      <xsl:if test="$messageType!='9'">
        <ns0:RFFLoop1>
          <ns0:RFF>
            <ns0:C506>
              <C50601>ACW</C50601>
              <C50602>
                <xsl:value-of select="userCSharp:StringRight($mrn, '35')"/>
              </C50602>
            </ns0:C506>
          </ns0:RFF>
        </ns0:RFFLoop1>
      </xsl:if>

      <ns0:RFFLoop1>
        <ns0:RFF>
          <ns0:C506>
            <C50601>ATX</C50601>
            <C50602>
              <xsl:value-of select="$reference"/>
            </C50602>
          </ns0:C506>
        </ns0:RFF>
      </ns0:RFFLoop1>

      <ns0:RFFLoop1>
        <ns0:RFF>
          <ns0:C506>
            <C50601>BN</C50601>
            <C50602>
              <xsl:value-of select="$reference"/>
            </C50602>
          </ns0:C506>
        </ns0:RFF>
      </ns0:RFFLoop1>

      <xsl:if test="$recipientType='YER'">
        <ns0:RFFLoop1>
          <ns0:RFF>
            <ns0:C506>
              <C50601>RE</C50601>
              <C50602>
                <xsl:value-of select="$releaseNum"/>
              </C50602>
            </ns0:C506>
          </ns0:RFF>
        </ns0:RFFLoop1>
      </xsl:if>

      <xsl:variable name="car_RegNumber" select="$shipment/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='ShippingLineAddress']/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='CAR' and s0:CountryOfIssue/text()=$operationalPortCountry]/s0:Value/text()" />
      <xsl:for-each select="$shipment/s0:TransportLegCollection/s0:TransportLeg[s0:LegType/text()='Main']">
        <xsl:variable name="estimatedDeparture" select="s0:EstimatedDeparture/text()" />
        <xsl:variable name="estimatedArrival" select="s0:EstimatedArrival/text()" />
        <ns0:TDTLoop1>
          <ns0:TDT>
            <TDT01>20</TDT01>
            <xsl:if test="s0:VoyageFlightNo">
              <TDT02>
                <xsl:value-of select="s0:VoyageFlightNo/text()" />
              </TDT02>
            </xsl:if>
            <ns0:C220>
              <C22001>1</C22001>
            </ns0:C220>

            <xsl:if test="$car_RegNumber!=''">
              <ns0:C040>
                <C04001>
                  <xsl:value-of select="$car_RegNumber" />
                </C04001>
                <C04002>172</C04002>
                <C04003>20</C04003>
              </ns0:C040>
            </xsl:if>
            <ns0:C222>
              <C22201>
                <xsl:value-of select="s0:VesselLloydsIMO/text()" />
              </C22201>
              <C22202>146</C22202>
              <C22203>11</C22203>
              <C22204>
                <xsl:value-of select="s0:VesselName/text()" />
              </C22204>
            </ns0:C222>
          </ns0:TDT>

          <xsl:if test="s0:PortOfLoading">
            <ns0:LOCLoop1>
              <ns0:LOC_2>
                <LOC01>9</LOC01>
                <ns0:C517_2>
                  <C51701>
                    <xsl:value-of select="s0:PortOfLoading/text()" />
                  </C51701>
                  <C51702>139</C51702>
                  <C51703>6</C51703>
                  <C51704>
                    <xsl:value-of select="s0:PortOfLoading/@Name" />
                  </C51704>
                </ns0:C517_2>
              </ns0:LOC_2>
              <ns0:DTM_4>
                <ns0:C507_4>
                  <C50701>133</C50701>
                  <C50702>
                    <xsl:value-of select="userCSharp:FormatDateTime($estimatedDeparture , 'yyyy-MM-ddTHH:mm:ss' , 'yyyyMMddHHmmss')" />
                  </C50702>
                  <C50703>203</C50703>
                </ns0:C507_4>
              </ns0:DTM_4>
            </ns0:LOCLoop1>
          </xsl:if>

          <xsl:if test="s0:PortOfDischarge">
            <ns0:LOCLoop1>
              <ns0:LOC_2>
                <LOC01>11</LOC01>
                <ns0:C517_2>
                  <C51701>
                    <xsl:value-of select="s0:PortOfDischarge/text()" />
                  </C51701>
                  <C51702>139</C51702>
                  <C51703>6</C51703>
                  <C51704>
                    <xsl:value-of select="s0:PortOfDischarge/@Name" />
                  </C51704>
                </ns0:C517_2>
              </ns0:LOC_2>
              <ns0:DTM_4>
                <ns0:C507_4>
                  <C50701>132</C50701>
                  <C50702>
                    <xsl:value-of select="userCSharp:FormatDateTime($estimatedArrival , 'yyyy-MM-ddTHH:mm:ss' , 'yyyyMMddHHmmss')" />
                  </C50702>
                  <C50703>203</C50703>
                </ns0:C507_4>
              </ns0:DTM_4>
            </ns0:LOCLoop1>
          </xsl:if>

          <ns0:LOCLoop1>
            <ns0:LOC_2>
              <LOC01>8</LOC01>
              <ns0:C517_2>
                <C51701>
                  <xsl:value-of select="$shipment/s0:PortOfDestination/text()" />
                </C51701>
                <C51702>139</C51702>
                <C51703>6</C51703>
                <C51704>
                  <xsl:value-of select="$shipment/s0:PortOfDestination/@Name" />
                </C51704>
              </ns0:C517_2>
            </ns0:LOC_2>
          </ns0:LOCLoop1>
        </ns0:TDTLoop1>
      </xsl:for-each>

      <xsl:for-each select="$shipment/s0:OrganizationAddressCollection/s0:OrganizationAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber">
        <xsl:if test="../../s0:AddressType/text()='Principal' and s0:Type/text()='CAR' and s0:CountryOfIssue/text()=$operationalPortCountry">
          <ns0:NADLoop1>
            <ns0:NAD>
              <NAD01>CA</NAD01>
              <ns0:C082>
                <C08201>
                  <xsl:value-of select="s0:Value/text()" />
                </C08201>
                <C08202>172</C08202>
                <C08203>20</C08203>
              </ns0:C082>
            </ns0:NAD>
          </ns0:NADLoop1>
        </xsl:if>
      </xsl:for-each>

      <xsl:for-each select="$shipment/s0:ContainerCollection/s0:Container">
          <xsl:variable name="includeContainer" select="userCSharp:IncludeContainer($releaseNum, s0:ReleaseNum/text() , $recipientType)" />
          <xsl:if test="$includeContainer='true'">
            <xsl:variable name="needToMapGroup" select="userCSharp:NeedToMapGroup(s0:ContainerNumber/text(), s0:ContainerType/s0:ISOCode/text(), s0:IsEmptyContainer/text(), s0:RemptyRequired/text(), s0:SetPointTemp/text(), s0:SetPointTempUnit/text())" />
            <xsl:if test="$needToMapGroup='true'">
              <xsl:variable name="isEmptyContainer" select="s0:IsEmptyContainer/text()" />
              <xsl:variable name="emptyRequired" select="s0:EmptyRequired/text()" />
              <xsl:variable name="isControlledAtmosphere" select="s0:IsControlledAtmosphere/text()" />
              <xsl:variable name="containerCount" select="userCSharp:GetContainerCount(string(s0:ContainerNumber/text()), string(s0:ContainerType/s0:ISOCode/text()), string(s0:IsEmptyContainer/text()), string(s0:RemptyRequired/text()), string(s0:SetPointTemp/text()), string(s0:SetPointTempUnit/text()))" />
              <ns0:EQDLoop1>
                <ns0:EQD>
                  <EQD01>CN</EQD01>
                  <ns0:C237_2>
                    <xsl:if test="s0:ContainerNumber">
                      <C23701>
                        <xsl:value-of select="s0:ContainerNumber/text()" />
                      </C23701>
                    </xsl:if>
                  </ns0:C237_2>

                  <ns0:C224>
                    <xsl:if test="s0:ContainerType/s0:ISOCode">
                      <C22401>
                        <xsl:value-of select="s0:ContainerType/s0:ISOCode/text()" />
                      </C22401>
                    </xsl:if>
                    <C22402>102</C22402>
                    <C22403>5</C22403>
                  </ns0:C224>

                  <xsl:if test="$mainLeg">
                    <EQD05>
                      <xsl:value-of select="userCSharp:GetEquipmentStatusCode($PortOfLoading , $PortOfDischarge, $operationalPortCountry)" />
                    </EQD05>
                  </xsl:if>
                  <EQD06>
                    <xsl:choose>
                      <xsl:when test="$isEmptyContainer='true'">4</xsl:when>
                      <xsl:otherwise>5</xsl:otherwise>
                    </xsl:choose>
                  </EQD06>
                </ns0:EQD>
                <ns0:EQN>
                  <ns0:C523>
                    <C52301>
                      <xsl:value-of select="$containerCount" />
                    </C52301>
                  </ns0:C523>
                </ns0:EQN>
                <xsl:if test="$emptyRequired!=''">
                  <ns0:DTM_8>
                    <ns0:C507_8>
                      <C50701>7</C50701>
                      <C50702>
                        <xsl:value-of select="userCSharp:FormatDateTime($emptyRequired, 'yyyy-MM-ddTHH:mm:ss', 'yyyyMMddHHmmss')" />
                      </C50702>
                      <C50703>203</C50703>
                    </ns0:C507_8>
                  </ns0:DTM_8>
                </xsl:if>
                <xsl:if test="$isControlledAtmosphere='true'">
                  <xsl:variable name="humidity" select="s0:HumidityPercent/text()" />
                  <xsl:variable name="setPointTempUnit" select="s0:SetPointTempUnit/text()" />
                  <xsl:variable name="setPointTemp" select="s0:SetPointTemp/text()" />
                  <xsl:if test="number($humidity) != 0">
                    <ns0:MEA_4>
                      <MEA01>AAE</MEA01>
                      <ns0:C502_4>
                        <C50201>AAO</C50201>
                      </ns0:C502_4>
                      <ns0:C174_4>
                        <C17401>PCT</C17401>
                        <C17402>
                          <xsl:value-of select="$humidity" />
                        </C17402>
                      </ns0:C174_4>
                    </ns0:MEA_4>
                  </xsl:if>
                  <xsl:variable name="airflow" select="s0:AirVentFlow/text()" />
                  <xsl:if test="number($airflow) != 0">
                    <ns0:MEA_4>
                      <MEA01>AAE</MEA01>
                      <ns0:C502_4>
                        <C50201>AAS</C50201>
                      </ns0:C502_4>
                      <ns0:C174_4>
                        <xsl:variable name="airflowUnit" select="s0:AirVentFlowRateUnit/text()" />
                        <xsl:variable name="airflowUnitAAS">
                          <xsl:choose>
                            <xsl:when test="$airflowUnit = 'P1'">PCT</xsl:when>
                            <xsl:otherwise>MTQ</xsl:otherwise>
                          </xsl:choose>
                        </xsl:variable>
                        <C17401>
                          <xsl:value-of select="$airflowUnitAAS" />
                        </C17401>
                        <C17402>
                          <xsl:choose>
                            <xsl:when test="$airflowUnit = '2L'">
                              <xsl:value-of select="format-number((number($airflow) * 0.0283168466 * 60), '#.###')" />
                            </xsl:when>
                            <xsl:otherwise>
                              <xsl:value-of select="$airflow" />
                            </xsl:otherwise>
                          </xsl:choose>
                        </C17402>
                      </ns0:C174_4>
                    </ns0:MEA_4>
                  </xsl:if>
                  <ns0:TMPLoop2>
                    <ns0:TMP_2>
                      <TMP01>2</TMP01>
                      <ns0:C239_2>
                        <C23901>
                          <xsl:value-of select="format-number($setPointTemp , '0.##')" />
                        </C23901>
                        <C23902>
                          <xsl:choose>
                            <xsl:when test="$setPointTempUnit = 'C'">CEL</xsl:when>
                            <xsl:when test="$setPointTempUnit = 'F'">FAH</xsl:when>
                            <xsl:otherwise></xsl:otherwise>
                          </xsl:choose>
                        </C23902>
                      </ns0:C239_2>
                    </ns0:TMP_2>
                  </ns0:TMPLoop2>
                </xsl:if>
                <xsl:if test="$goodsDesciption!=''">
                  <ns0:FTX_4>
                    <FTX01>AAA</FTX01>
                    <ns0:C108_4>
                      <C10801>
                        <xsl:value-of select="$goodsDesciption" />
                      </C10801>
                    </ns0:C108_4>
                  </ns0:FTX_4>
                </xsl:if>
                <ns0:NADLoop3>
                  <xsl:variable name="cnrAddressType">
                    <xsl:choose>
                      <xsl:when test="count($shipment/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='ConsignorDocumentaryAddress']) &gt; 0">
                        <xsl:value-of select="'ConsignorDocumentaryAddress'"/>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="'BookingPartyDocumentaryAddress'"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:variable>
                  <ns0:NAD_3>
                    <NAD01>CZ</NAD01>
                    <ns0:C082_3>
                      <C08201>
                        <xsl:value-of select="$shipment/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()=$cnrAddressType]/s0:OrganizationCode/text()" />
                      </C08201>
                      <C08202>160</C08202>
                      <C08203>ZZZ</C08203>
                    </ns0:C082_3>
                    <ns0:C058_3>
                      <C05801>
                        <xsl:variable name="cnrCompanyName" select="normalize-space($shipment/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()=$cnrAddressType]/s0:CompanyName/text())" />
                        <xsl:choose>
                          <xsl:when test="string-length($cnrCompanyName) > 35">
                            <xsl:value-of select="substring($cnrCompanyName, 1, 35)" />
                          </xsl:when>
                          <xsl:otherwise>
                            <xsl:value-of select="$cnrCompanyName" />
                          </xsl:otherwise>
                        </xsl:choose>
                      </C05801>
                    </ns0:C058_3>
                  </ns0:NAD_3>
                </ns0:NADLoop3>
              </ns0:EQDLoop1>
            </xsl:if>
          </xsl:if>
      </xsl:for-each>

      <ns0:CNT>
        <ns0:C270>
          <C27001>16</C27001>
          <C27002>
            <xsl:value-of select="userCSharp:GetNumberOfGroups()" />
          </C27002>
        </ns0:C270>
      </ns0:CNT>
    </ns0:EFACT_D00B_COPARN>
  </xsl:template>
  <msxsl:script language="C#" implements-prefix="userCSharp">
  <![CDATA[


public System.Collections.Hashtable containerCounts = new System.Collections.Hashtable();
public System.Collections.Hashtable mappedContainers = new System.Collections.Hashtable();
public string releaseSuffix;
public bool isMultipleReleases;

public string AddToCount(string containerNum, string isoCode, string isEmpty, string emptyReqDate, string temp, string tempUnit, string count)
{
	var key = containerNum + isoCode + isEmpty + emptyReqDate + temp + tempUnit;
    var containerCount = int.Parse(count);

    if (containerCounts.ContainsKey(key))
    {
        containerCounts[key] = (int)containerCounts[key] + containerCount;
    }
    else
    {
        containerCounts.Add(key, containerCount);
    }

    return "";
}


public string GetContainerCount(string containerNum, string isoCode, string isEmpty, string emptyReqDate, string temp, string tempUnit)
{
    var key = containerNum + isoCode + isEmpty + emptyReqDate + temp + tempUnit;
    return containerCounts[key].ToString();
}


public bool NeedToMapGroup(string containerNum, string isoCode, string isEmpty, string emptyReqDate, string temp, string tempUnit)
{
    var key = containerNum + isoCode + isEmpty + emptyReqDate + temp + tempUnit;

    if (mappedContainers.ContainsKey(key))
    {
        return false;
    }

    mappedContainers.Add(key, true);
    return true;
}

public string GetNumberOfGroups()
{
    return mappedContainers.Count.ToString();
}


public string CalculateFirstRelease(string releaseNum)
{
    var sections = releaseNum.Split('-');
	var suffix = sections[sections.Length - 1];

    if (!string.IsNullOrEmpty(suffix))
    {
       if (string.IsNullOrEmpty(releaseSuffix))
       {
           releaseSuffix = suffix;
       }
       else if (suffix != releaseSuffix)
       {
           isMultipleReleases = true;
       }
    }

    return String.Empty;
}


public string Coalesce(string val0, string val1)
{
	if (!string.IsNullOrEmpty(val0))
	{
		return val0;
	}
	else if (!string.IsNullOrEmpty(val1))
	{
		return val1;
	}
	else {
		return string.Empty;
	}
}


public string GetReleaseNum(string eventRef)
{
	return eventRef.Split('|')[0];
}


public string GetEquipmentStatusCode(string load, string discharge, string operationalPortCountry)
{
	bool export = load.StartsWith(operationalPortCountry);
	bool import = discharge.StartsWith(operationalPortCountry);
	bool continental = import && export;

	if (continental) return "1";
	if (export) return "2";
	if (import) return "3";

	return "6";
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



public string GetTypParam(string eventRef, string recipientType, string releaseNums)
{
    if (messageType == null)
    {
	    const string paramQualifier = "TYP=";
	    string paramValue = null;
        var eventSections = eventRef.Split('|');

        var referenceSections = eventSections[0].Split('-');
        var releaseSuffix = referenceSections[referenceSections.Length - 1];

	    foreach (var param in eventSections)
	    {
	    	if (param.StartsWith(paramQualifier))
	    	{
	    		paramValue = param.Substring(paramQualifier.Length);
	    		break;
	    	}
	    }

	    switch (paramValue)
	    {
            case "Cancellation":
	    	case "CAN": messageType = recipientType == "PER" && !string.IsNullOrEmpty(releaseNums) ? "5" : "1";
              break;
            case "Revised":
        case "REP":
	    	case "RVS": messageType = "5";
              break;
            case "Authorization":
            case "Authorisation":
	    	case "ORG": messageType = recipientType == "PER" && isMultipleReleases ? "5" : "9";
              break;
	    	default: return String.Empty;
	    }
    }

    return messageType;
}

public string messageType;

public string StringRight(string str, string count)
{
	string retval = "";
	double d = 0;
	if (str != null && IsNumeric(count, ref d))
	{
		int i = (int) d;
		if (i > 0)
		{
			if (i <= str.Length)
			{
				retval = str.Substring(str.Length-i);
			}
			else
			{
				retval = str;
			}
		}
	}
	return retval;
}


public string CreateMRN(string jobNum, string eventRef, string recipientType)
{
    if (recipientType == "YER")
    {
	    var sections = eventRef.Split('-');
	    var suffix = sections[sections.Length - 1];
        return jobNum + "-" + suffix;
    }

	return jobNum;
}


public bool IncludeContainer(string jobRelease, string containerRelease, string recipientRole)
{
	return messageType == "1" || (recipientRole == "PER" && !string.IsNullOrEmpty(containerRelease) || recipientRole == "YER" && containerRelease == jobRelease);
}

public string InitCumulativeConcat(int index)
{
	if (index >= 0)
	{
		if (index >= myCumulativeConcatArray.Count)
		{
			int i = myCumulativeConcatArray.Count;
			for (; i<=index; i++)
			{
				myCumulativeConcatArray.Add("");
			}
		}
		else
		{
			myCumulativeConcatArray[index] = "";
		}
	}
	return "";
}

public System.Collections.ArrayList myCumulativeConcatArray = new System.Collections.ArrayList();

public string AddToCumulativeConcat(int index, string val, string notused)
{
	if (index < 0 || index >= myCumulativeConcatArray.Count)
	{
		return "";
	}
	myCumulativeConcatArray[index] = (string)(myCumulativeConcatArray[index]) + val;
	return myCumulativeConcatArray[index].ToString();
}

public string GetCumulativeConcat(int index)
{
	if (index < 0 || index >= myCumulativeConcatArray.Count)
	{
		return "";
	}
	return myCumulativeConcatArray[index].ToString();
}


public bool IsNumeric(string val)
{
	if (val == null)
	{
		return false;
	}
	double d = 0;
	return Double.TryParse(val, System.Globalization.NumberStyles.AllowThousands | System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out d);
}

public bool IsNumeric(string val, ref double d)
{
	if (val == null)
	{
		return false;
	}
	return Double.TryParse(val, System.Globalization.NumberStyles.AllowThousands | System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out d);
}

public bool ValToBool(string val)
{
	if (val != null)
	{
		if (string.Compare(val, bool.TrueString, StringComparison.OrdinalIgnoreCase) == 0)
		{
			return true;
		}
		if (string.Compare(val, bool.FalseString, StringComparison.OrdinalIgnoreCase) == 0)
		{
			return false;
		}
		val = val.Trim();
		if (string.Compare(val, bool.TrueString, StringComparison.OrdinalIgnoreCase) == 0)
		{
			return true;
		}
		if (string.Compare(val, bool.FalseString, StringComparison.OrdinalIgnoreCase) == 0)
		{
			return false;
		}
		double d = 0;
		if (IsNumeric(val, ref d))
		{
			return (d > 0);
		}
	}
	return false;
}

]]></msxsl:script>
</xsl:stylesheet>

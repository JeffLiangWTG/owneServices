<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var"
                exclude-result-prefixes="msxsl var s0 CodeMapper userCSharp UnitConverter ContextAccessor"
                version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2012/11"
                xmlns:ns0="http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp"
                xmlns:UnitConverter="http://schemas.microsoft.com/BizTalk/2003/UnitConverter"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor">

  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes" />

  <xsl:template match="/">
    <xsl:apply-templates select="/s0:UniversalShipment" />
  </xsl:template>

  <xsl:template match="/s0:UniversalShipment">

    <xsl:variable name="shipment" select="s0:Shipment" />
    <xsl:variable name="mainLeg" select="$shipment/s0:TransportLegCollection/s0:TransportLeg[s0:LegType/text()='Main']" />
    <xsl:variable name="eventType" select="$shipment/s0:DataContext/s0:Workflow/s0:EventType/text()" />

    <xsl:variable name="PortOfDischarge" select="$mainLeg/s0:PortOfDischarge/text()" />

    <xsl:variable name="SourceParty" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
    <xsl:variable name="DestinationParty" select="CodeMapper:GetRecipientCode('ShippingPortMessaging', 'ShippingPortMessaging', 'ShippingPortMessaging COREOR', 'Ports', 'DestinationParty', $PortOfDischarge)" />
    <xsl:variable name="SetDestinationParty" select="ContextAccessor:SetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties', $DestinationParty)"/>
    <xsl:variable name="SetDestinationPartyName" select="ContextAccessor:SetContextProperty('DestinationPartyName', 'http://schemas.microsoft.com/Edi/PropertySchema', $DestinationParty)"/>
    <xsl:variable name="OverrideEDIHeader" select="ContextAccessor:SetContextProperty('OverrideEDIHeader', 'http://schemas.microsoft.com/BizTalk/2006/edi-properties', 'true')" />
    <xsl:variable name="DestinationPartySenderIdentifier" select="ContextAccessor:SetContextProperty('UNB2_1', 'http://schemas.microsoft.com/BizTalk/2006/edi-properties', substring($SourceParty, 1, 3))" />
    <xsl:variable name="operationPort">
      <xsl:variable name="operationalPort_Code" select="normalize-space($shipment/s0:AddInfoCollection/s0:AddInfo[s0:Key='OperationalPort_Code']/s0:Value/text())" />
      <xsl:choose>
        <xsl:when test="$operationalPort_Code != ''">
          <xsl:value-of select="substring($operationalPort_Code, 1, 2)" />
        </xsl:when>
        <xsl:otherwise>NZ</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <ns0:EFACT_D95B_COREOR>
      <UNH>
        <UNH1>
          <xsl:value-of select="CodeMapper:CallActionProcedureHelper('GetCounterValue' , '@value' , '@name' , 'CargoWise.eHub.Products.OceanTracing.Transforms.COREOR' , '@maxlength' , '14')" />
        </UNH1>
        <UNH2>
          <UNH2.1>COREOR</UNH2.1>
          <UNH2.2>D</UNH2.2>
          <UNH2.3>95B</UNH2.3>
          <UNH2.4>UN</UNH2.4>
        </UNH2>
      </UNH>
      <xsl:variable name="consolID" select="$shipment/s0:DataContext/s0:DataSource/s0:Key[1]/text()" />
      <ns0:BGM>
        <ns0:C002>
          <C00201>12</C00201>
        </ns0:C002>
        <BGM02>
          <xsl:choose>
            <xsl:when test="$eventType='MSN'">
              <xsl:value-of select="$consolID"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="CodeMapper:CallActionProcedureHelper('GetCounterValue' , '@value' , '@name' , 'CargoWise.eHub.Products.OceanTracing.Transforms.COREOR' , '@maxlength' , '14')" />
            </xsl:otherwise>
          </xsl:choose>
        </BGM02>

        <BGM03>
          <xsl:choose>
            <xsl:when test="$eventType='MSN'">9</xsl:when>
            <xsl:when test="$eventType!='MSN'">1</xsl:when>
            <xsl:otherwise></xsl:otherwise>
          </xsl:choose>
        </BGM03>
      </ns0:BGM>

      <xsl:for-each select="$shipment/s0:ContainerCollection/s0:Container[s0:ContainerImportDORelease/text()!='']">
        <ns0:RFF>
          <xsl:variable name="var:v17" select="s0:ContainerImportDORelease" />
          <ns0:C506>
            <C50601>REO</C50601>
            <C50602>
              <xsl:value-of select="s0:ContainerImportDORelease/text()" />
            </C50602>
          </ns0:C506>
        </ns0:RFF>
      </xsl:for-each>

      <xsl:if test="$eventType!='MSN'">
        <ns0:RFF>
          <ns0:C506>
            <C50601>ACW</C50601>
            <C50602>
              <xsl:value-of select="$consolID" />
            </C50602>
          </ns0:C506>
        </ns0:RFF>
      </xsl:if>

      <ns0:RFF>
        <ns0:C506>
          <C50601>BM</C50601>
          <C50602>
            <xsl:value-of select="$shipment/s0:WayBillNumber/text()" />
          </C50602>
        </ns0:C506>
      </ns0:RFF>

      <xsl:if test="$mainLeg">
        <xsl:variable name="car_RegNumber" select="$shipment/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='ShippingLineAddress']/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='CAR' and s0:CountryOfIssue/text()=$operationPort]/s0:Value/text()" />
        <ns0:TDTLoop1>
          <ns0:TDT>
            <TDT01>20</TDT01>
            <TDT02>
              <xsl:value-of select="$mainLeg/s0:VoyageFlightNo/text()" />
            </TDT02>
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
                <xsl:value-of select="$mainLeg/s0:VesselLloydsIMO/text()" />
              </C22201>
              <C22202>146</C22202>
              <C22203>11</C22203>
              <C22204>
                <xsl:value-of select="$mainLeg/s0:VesselName/text()" />
              </C22204>
            </ns0:C222>
          </ns0:TDT>

          <ns0:LOC>
            <LOC01>9</LOC01>
            <ns0:C517>
              <C51701>
                <xsl:value-of select="$mainLeg/s0:PortOfLoading/text()" />
              </C51701>
              <C51702>139</C51702>
              <C51703>6</C51703>
              <C51704>
                <xsl:value-of select="substring($mainLeg/s0:PortOfLoading/@Name, 1, 17)" />
              </C51704>
            </ns0:C517>
          </ns0:LOC>

          <ns0:LOC>
            <LOC01>11</LOC01>
            <ns0:C517>
              <C51701>
                <xsl:value-of select="$mainLeg/s0:PortOfDischarge/text()" />
              </C51701>
              <C51702>139</C51702>
              <C51703>6</C51703>
              <C51704>
                <xsl:value-of select="substring($mainLeg/s0:PortOfDischarge/@Name, 1, 17)" />
              </C51704>
            </ns0:C517>
          </ns0:LOC>

          <xsl:variable name="estimatedDeparture" select="$mainLeg/s0:EstimatedDeparture/text()" />
          <xsl:variable name="estimatedArrival" select="$mainLeg/s0:EstimatedArrival/text()" />
          <ns0:DTM>
            <ns0:C507>
              <C50701>133</C50701>
              <C50702>
                <xsl:value-of select="userCSharp:FormatDateTime($estimatedDeparture , 'yyyy-MM-ddTHH:mm:ss' , 'yyyyMMddHHmmss')" />
              </C50702>
              <C50703>203</C50703>
            </ns0:C507>
          </ns0:DTM>
          <ns0:DTM>
            <ns0:C507>
              <C50701>132</C50701>
              <C50702>
                <xsl:value-of select="userCSharp:FormatDateTime($estimatedArrival , 'yyyy-MM-ddTHH:mm:ss' , 'yyyyMMddHHmmss')" />
              </C50702>
              <C50703>203</C50703>
            </ns0:C507>
          </ns0:DTM>
        </ns0:TDTLoop1>
      </xsl:if>

      <xsl:variable name="orgPrincipal" select="$shipment/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='Principal']" />
      <xsl:if test="$orgPrincipal">
        <ns0:NADLoop1>
          <ns0:NAD>
            <NAD01>CA</NAD01>
            <ns0:C082>
              <C08201>
                <xsl:value-of select="$orgPrincipal/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='CAR' and s0:CountryOfIssue/text()=$operationPort]/s0:Value/text()" />
              </C08201>
              <C08202>172</C08202>
              <C08203>20</C08203>
            </ns0:C082>
          </ns0:NAD>
        </ns0:NADLoop1>
      </xsl:if>

      <xsl:for-each select="$shipment/s0:ContainerCollection">
        <xsl:for-each select="s0:Container">
          <ns0:EQDLoop1>
            <ns0:EQD>
              <EQD01>CN</EQD01>
              <ns0:C237_2>
                <xsl:variable name="containerNumber" select="s0:ContainerNumber/text()" />
                <xsl:if test="$containerNumber!=''">
                  <C23701>
                    <xsl:value-of select="$containerNumber" />
                  </C23701>
                </xsl:if>
              </ns0:C237_2>
              <xsl:variable name="containerISO" select="s0:ContainerType/s0:ISOCode/text()" />
              <xsl:if test="$containerISO!=''">
                <ns0:C224>
                  <C22401>
                    <xsl:value-of select="$containerISO" />
                  </C22401>
                  <C22402>102</C22402>
                  <C22403>5</C22403>
                </ns0:C224>
              </xsl:if>

              <xsl:if test="$mainLeg">
                <EQD05>
                  <xsl:value-of select="userCSharp:GetEquipmentStatusCode($mainLeg/s0:PortOfLoading/text(), $mainLeg/s0:PortOfDischarge/text(), string($operationPort))" />
                </EQD05>
              </xsl:if>

              <EQD06>
                <xsl:choose>
                  <xsl:when test="s0:IsEmptyContainer/text()='true'">4</xsl:when>
                  <xsl:otherwise>5</xsl:otherwise>
                </xsl:choose>
              </EQD06>
            </ns0:EQD>

            <ns0:RFF_5>
              <ns0:C506_5>
                <C50601>SQ</C50601>
                <C50602>1</C50602>
              </ns0:C506_5>
            </ns0:RFF_5>

            <ns0:MEA_4>
              <MEA01>AAE</MEA01>
              <ns0:C502_4>
                <C50201>G</C50201>
              </ns0:C502_4>
              <ns0:C174_4>
                <C17401>KGM</C17401>
                <C17402>
                  <xsl:value-of  select="userCSharp:ToInt(UnitConverter:Convert(string(s0:GrossWeight/text()) , string(s0:WeightUnit/text()) , 'KG'))" />
                </C17402>
              </ns0:C174_4>
            </ns0:MEA_4>

            <xsl:call-template name="EQD-DIM">
              <xsl:with-param name="code" select="'5'" />
              <xsl:with-param name="value" select="s0:OverhangFront/text()" />
            </xsl:call-template>

            <xsl:call-template name="EQD-DIM">
              <xsl:with-param name="code" select="'6'" />
              <xsl:with-param name="value" select="s0:OverhangBack/text()" />
            </xsl:call-template>

            <xsl:call-template name="EQD-DIM">
              <xsl:with-param name="code" select="'7'" />
              <xsl:with-param name="value" select="s0:OverhangRight/text()" />
            </xsl:call-template>

            <xsl:call-template name="EQD-DIM">
              <xsl:with-param name="code" select="'8'" />
              <xsl:with-param name="value" select="s0:OverhangLeft/text()" />
            </xsl:call-template>

            <xsl:call-template name="EQD-DIM">
              <xsl:with-param name="code" select="'9'" />
              <xsl:with-param name="value" select="s0:OverhangHeight/text()" />
            </xsl:call-template>

            <xsl:variable name="isControlledAtmosphere" select="s0:IsControlledAtmosphere/text()" />
            <xsl:if test="$isControlledAtmosphere='true'">
              <xsl:variable name="setPointTempUnit" select="s0:SetPointTempUnit/text()" />
              <xsl:variable name="setPointTemp" select="s0:SetPointTemp/text()" />
              <ns0:TMP>
                <TMP01>2</TMP01>
                <ns0:C239>
                  <C23901>
                    <xsl:value-of select="userCSharp:TrimTrailingZeros($setPointTemp)" />
                  </C23901>
                  <C23902>
                    <xsl:choose>
                      <xsl:when test="$setPointTempUnit = 'C'">CEL</xsl:when>
                      <xsl:when test="$setPointTempUnit = 'F'">FAH</xsl:when>
                      <xsl:otherwise></xsl:otherwise>
                    </xsl:choose>
                  </C23902>
                </ns0:C239>
              </ns0:TMP>
            </xsl:if>

            <xsl:variable name="isDamaged" select="s0:IsDamaged/text()" />

            <xsl:call-template name="EQD-SEL">
              <xsl:with-param name="seal" select="s0:Seal/text()" />
              <xsl:with-param name="sealPartyType" select="s0:SealPartyType/text()" />
              <xsl:with-param name="isDamaged" select="$isDamaged" />
            </xsl:call-template>

            <xsl:call-template name="EQD-SEL">
              <xsl:with-param name="seal" select="s0:SecondSeal/text()" />
              <xsl:with-param name="sealPartyType" select="s0:SecondSealPartyType/text()" />
              <xsl:with-param name="isDamaged" select="$isDamaged" />
            </xsl:call-template>

            <xsl:call-template name="EQD-SEL">
              <xsl:with-param name="seal" select="s0:ThirdSeal/text()" />
              <xsl:with-param name="sealPartyType" select="s0:ThirdSealPartyType/text()" />
              <xsl:with-param name="isDamaged" select="$isDamaged" />
            </xsl:call-template>

            <xsl:variable name="goodsDesciption"  select="$shipment/s0:GoodsDescription/text()" />
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

            <xsl:variable name="refrigGeneratorID" select="s0:RefrigGeneratorID/text()" />
            <xsl:if test="$refrigGeneratorID!=''">
              <ns0:EQA>
                <EQA01>RG</EQA01>
                <ns0:C237_3>
                  <C23701>
                    <xsl:value-of select="$refrigGeneratorID" />
                  </C23701>
                </ns0:C237_3>
              </ns0:EQA>
            </xsl:if>
          </ns0:EQDLoop1>
        </xsl:for-each>
      </xsl:for-each>

      <ns0:CNT>
        <ns0:C270>
          <C27001>16</C27001>
          <C27002>1</C27002>
        </ns0:C270>
      </ns0:CNT>
    </ns0:EFACT_D95B_COREOR>
  </xsl:template>

  <xsl:template name="EQD-DIM">
    <xsl:param name="code"/>
    <xsl:param name="value"/>

    <xsl:if test="userCSharp:LogicalNotEqual($value, '0')">
      <ns0:DIM>
        <DIM01>
          <xsl:value-of select="$code" />
        </DIM01>
        <ns0:C211>
          <C21101>INH</C21101>
          <C21102>
            <xsl:value-of select="userCSharp:TrimTrailingZeros(UnitConverter:Convert($value , 'FT' , 'IN'))" />
          </C21102>
        </ns0:C211>
      </ns0:DIM>
    </xsl:if>
  </xsl:template>

  <xsl:template name="EQD-SEL">
    <xsl:param name="seal"/>
    <xsl:param name="sealPartyType"/>
    <xsl:param name="isDamaged"/>

    <xsl:if test="$seal!=''">
      <ns0:SEL>
        <SEL01>
          <xsl:value-of select="$seal" />
        </SEL01>
        <ns0:C215>
          <C21501>
            <xsl:choose>
              <xsl:when test="$sealPartyType='CAR'">CA</xsl:when>
              <xsl:when test="$sealPartyType='CRD'">SH</xsl:when>
              <xsl:when test="$sealPartyType='CUS'">CU</xsl:when>
              <xsl:when test="$sealPartyType='QRT'">AC</xsl:when>
              <xsl:when test="$sealPartyType='CTO'">TO</xsl:when>
              <xsl:otherwise></xsl:otherwise>
            </xsl:choose>
          </C21501>
        </ns0:C215>
        <SEL03>
          <xsl:choose>
            <xsl:when test="$isDamaged='true'">2</xsl:when>
            <xsl:otherwise>1</xsl:otherwise>
          </xsl:choose>
        </SEL03>
      </ns0:SEL>
    </xsl:if>
  </xsl:template>

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[
public bool LogicalNotEqual(string val1, string val2)
{
	bool ret = false;
	double d1 = 0;
	double d2 = 0;
	if (IsNumeric(val1, ref d1) && IsNumeric(val2, ref d2))
	{
		ret = d1 != d2;
	}
	else
	{
		ret = String.Compare(val1, val2, StringComparison.Ordinal) != 0;
	}
	return ret;
}

public int ToInt(string num)
{
    return (int)double.Parse(num);
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

public string GetEquipmentStatusCode(string load, string discharge, string operationPort)
{
	bool export = load.StartsWith(operationPort);
	bool import = discharge.StartsWith(operationPort);
	bool continental = import && export;

	if (continental) return "1";
	if (export) return "2";
	if (import) return "3";

	return "6";
}

public string TrimTrailingZeros(string num)
{
	return string.Format("{0:G29}", decimal.Parse(num));
}

public bool IsNumeric(string val, ref double d)
{
	if (val == null)
	{
		return false;
	}
	return Double.TryParse(val, System.Globalization.NumberStyles.AllowThousands | System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out d);
}

]]>
  </msxsl:script>
</xsl:stylesheet>
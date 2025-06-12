<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt"
    xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var" exclude-result-prefixes="msxsl var s0 ScriptNS0 ScriptNS1 ScriptNS2 ScriptNS3 userCSharp" version="1.0"
    xmlns:s0="http://www.cargowise.com/Schemas/Universal/2011/11"
    xmlns:ns0="http://cargowise.com/ehub/products/jpcustoms/2013/09"
    xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
    xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1"
    xmlns:ScriptNS2="http://schemas.microsoft.com/BizTalk/2003/ScriptNS2"
    xmlns:ScriptNS3="http://schemas.microsoft.com/BizTalk/2003/ScriptNS3"
    xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
	<xsl:output omit-xml-declaration="yes" indent="no" version="1.0" method="xml" />

	<xsl:template match="/">
		<xsl:apply-templates select="/s0:UniversalShipment" />
	</xsl:template>

	<xsl:template match="/s0:UniversalShipment">
		<xsl:element name="ns0:ATDInput">
			<xsl:element name="ns0:InputCommonField">
				<xsl:element name="ns0:ProcessingControlCode">SS</xsl:element>
				<xsl:element name="ns0:ProcedureCode">ATD</xsl:element>
				<xsl:element name="ns0:ReserveArea1"/>
				<xsl:variable name="sourceParty" select="ScriptNS2:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
				<xsl:element name="ns0:UserCode">
					<xsl:value-of select="ScriptNS3:GetUsername($sourceParty)" />
				</xsl:element>
				<xsl:element name="ns0:UserID">001</xsl:element>
				<xsl:element name="ns0:UserPassword">
					<xsl:value-of select="ScriptNS3:GetPassword($sourceParty)" />
				</xsl:element>
				<xsl:element name="ns0:ReserveArea2"/>
				<xsl:element name="ns0:MessageTag"/>
				<xsl:element name="ns0:ReserveArea3"/>
				<xsl:element name="ns0:InputMessageID">
					<xsl:value-of select="userCSharp:StringLeftBytes(normalize-space(./*[local-name()='Shipment']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPInternalTransactionNumber'][1]/*[local-name()='Value']/text()),10)"/>
				</xsl:element>
				<xsl:element name="ns0:IndexTag"/>
				<xsl:element name="ns0:ReserveArea4"/>
				<xsl:element name="ns0:SystemID">2</xsl:element>
				<xsl:element name="ns0:ReserveArea5"/>
				<xsl:element name="ns0:MessageLength"/>
			</xsl:element>
			<xsl:element name="ns0:FunctionTypeCode">
				<xsl:choose>
					<xsl:when test="normalize-space(./*[local-name()='Shipment']/*[local-name()='SubShipmentCollection']/*[local-name()='SubShipment']
													/*[local-name()='DataContext']/*[local-name()='ActionPurpose']/*[local-name()='Code']) = 'REG'">9</xsl:when>
					<xsl:otherwise>5</xsl:otherwise>
				</xsl:choose>
			</xsl:element>
			<xsl:element name="ns0:SPCode">
				<xsl:element name="ns0:SPID">
					<xsl:value-of select="ScriptNS0:CallActionProcedureHelper('GetUserIDPasswordReference', '@Result' , '@ApplicationCode', 'JPC' , '@eHubID', 'eHub' , '@IsGettingPassword', '0' )" />
				</xsl:element>
				<xsl:element name="ns0:SPPassword">
					<xsl:value-of select="ScriptNS0:CallActionProcedureHelper('GetUserIDPasswordReference', '@Result' , '@ApplicationCode', 'JPC' , '@eHubID', 'eHub' , '@IsGettingPassword', '1' )" />
				</xsl:element>
			</xsl:element>
			<xsl:element name="ns0:VesselCode">
				<xsl:variable name="VesselCallSign" select="./*[local-name()='Shipment']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPVesselCallSign'][1]/*[local-name()='Value']" />
				<xsl:choose>
					<xsl:when test="$VesselCallSign">
						<xsl:value-of select="userCSharp:StringMaxAllowed(normalize-space($VesselCallSign/text()),9)"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="userCSharp:StringMaxAllowed(normalize-space(./*[local-name()='Shipment']/*[local-name()='LloydsIMO']/text()),9)"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:element>
			<xsl:element name="ns0:VoyageNumber">
				<xsl:value-of select ="userCSharp:StringMaxAllowed(normalize-space(./*[local-name()='Shipment']/*[local-name()='VoyageFlightNo']/text()),10)"/>
			</xsl:element>
			<xsl:element name="ns0:CarrierCode">
				<xsl:value-of select="userCSharp:StringMaxAllowed( normalize-space(./*[local-name()='Shipment']/*[local-name()='AddInfoCollection']
                        /*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPCarrierCode'][1]
                        /*[local-name()='Value']/text()),4)" />
			</xsl:element>
			<xsl:variable name="PortOfLoading" select="userCSharp:StringMaxAllowed(normalize-space(./*[local-name()='Shipment']/*[local-name()='PortOfLoading']/*[local-name()='Code']/text()),5)"/>
			<xsl:element name="ns0:PortOfLoadingCode">
				<xsl:value-of select="$PortOfLoading" />
			</xsl:element>
			<xsl:element name="ns0:PortOfLoadingSuffix">
				<xsl:value-of select ="userCSharp:StringMaxAllowed(
                            normalize-space(./*[local-name()='Shipment']/*[local-name()='AddInfoCollection']
                            /*[local-name()='AddInfo'
                            and *[local-name()='Key']/text()='JPPortOfLoadingSuffix'][1]
                            /*[local-name()='Value']/text()),1)"/>
			</xsl:element>
			<xsl:variable name="etd" select="./*[local-name()='Shipment']/*[local-name()='DateCollection']
                            /*[local-name()='Date'and *[local-name()='Type']/text()='Departure' and *[local-name()='IsEstimate']/text()='true'][1]
                            /*[local-name()='Value']/text()"/>
			<xsl:variable name="atd" select="./*[local-name()='Shipment']/*[local-name()='DateCollection']
                            /*[local-name()='Date'and *[local-name()='Type']/text()='Departure' and *[local-name()='IsEstimate']/text()='false'][1]
                            /*[local-name()='Value']/text()"/>
			<xsl:element name="ns0:DateOfDeparture">
				<xsl:choose>
					<xsl:when test="$etd">
						<xsl:value-of select="ScriptNS1:FormatXmlDateTime($etd, 'yyyyMMdd')"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="ScriptNS1:FormatXmlDateTime($atd, 'yyyyMMdd')"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:element>
			<xsl:element name="ns0:TimeOfDeparture">
				<xsl:choose>
					<xsl:when test="$etd">
						<xsl:value-of select="ScriptNS1:FormatXmlDateTime($etd, 'HHmm')"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="ScriptNS1:FormatXmlDateTime($atd, 'HHmm')"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:element>
			<xsl:element name="ns0:TimeDifferenceFromGMT">
				<xsl:choose>
					<xsl:when test="$etd">
						<xsl:value-of select="translate(ScriptNS0:CallActionProcedureHelper('CalculateTimeZoneOffset', '@offset', '@UNLOCO', string($PortOfLoading), '@localtime', $etd),':','')" />
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="translate(ScriptNS0:CallActionProcedureHelper('CalculateTimeZoneOffset', '@offset', '@UNLOCO', string($PortOfLoading), '@localtime', $atd),':','')" />
					</xsl:otherwise>
				</xsl:choose>
			</xsl:element>
			<xsl:element name="ns0:RelaxedApplicationAreaID">
				<xsl:if test="normalize-space(./*[local-name()='Shipment']/*[local-name()='AddInfoCollection']
                        /*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPIsDepartureFromRelaxedArea'][1]
                        /*[local-name()='Value']/text())='Y'" >
					<xsl:value-of select="'Y'"/>
				</xsl:if>
			</xsl:element>
		</xsl:element>
	</xsl:template>

	<msxsl:script language="C#" implements-prefix="userCSharp">
		<![CDATA[
public string StringLeftBytes(string text, int lenBytes)
{
    if (System.Text.Encoding.UTF8.GetByteCount(text) <= lenBytes)
        return text.ToUpper();
    byte[] outBytes = new byte[lenBytes];
    int outLen = 0;
    for (int i = 0; i < text.Length; i++)
    {
        byte[] newBytes = System.Text.Encoding.UTF8.GetBytes(text.Substring(i, 1));
        if (outLen + newBytes.Length > lenBytes)
            break;
        else
            newBytes.CopyTo(outBytes, outLen);
        outLen += newBytes.Length;
    }
    return System.Text.Encoding.UTF8.GetString(outBytes, 0, outLen).ToUpper();
}

public string StringMaxAllowed(string text, int lenBytes)
{
    return (System.Text.Encoding.UTF8.GetByteCount(text) > lenBytes) ?  GetInvalidCharacter(lenBytes) : text.ToUpper();
}

public string GetInvalidCharacter(int lenBytes)
{
    return "".PadLeft(lenBytes, '*');
}
        ]]>
	</msxsl:script>

</xsl:stylesheet>


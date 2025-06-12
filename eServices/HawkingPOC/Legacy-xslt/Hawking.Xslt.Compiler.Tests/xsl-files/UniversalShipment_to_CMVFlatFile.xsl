<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt"
    xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var" exclude-result-prefixes="msxsl var s0 ScriptNS0 ScriptNS1 ScriptNS2 ScriptNS3 userCSharp" version="1.0"
    xmlns:s0="http://www.cargowise.com/Schemas/Universal/2011/11"
    xmlns:ns0="http://cargowise.com/ehub/products/jpcustoms/2017/01"
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
		<xsl:element name="ns0:CMVFlatFileSchemaEnvelope">
			<xsl:element name="ns0:CMVInput">
				<!-- Segment: Header -->
				<xsl:element name="ns0:InputCommonField">
					<xsl:element name="ns0:ProcessingControlCode">SS</xsl:element>
					<xsl:element name="ns0:ProcedureCode">
						<xsl:choose>
							<xsl:when test="normalize-space(./*[local-name()='Shipment']/*[local-name()='DataContext']/*[local-name()='ActionPurpose']/*[local-name()='Code']) = 'CMV'">CMV</xsl:when>
							<xsl:otherwise>UNK</xsl:otherwise>
						</xsl:choose>
					</xsl:element>
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
				<xsl:element name="ns0:Reserved"/>
				<xsl:element name="ns0:SPCode">
					<xsl:element name="ns0:SPID">
						<xsl:value-of select="ScriptNS0:CallActionProcedureHelper('GetUserIDPasswordReference', '@Result' , '@ApplicationCode', 'JPC' , '@eHubID', 'eHub' , '@IsGettingPassword', '0' )" />
					</xsl:element>
					<xsl:element name="ns0:SPPassword">
						<xsl:value-of select="ScriptNS0:CallActionProcedureHelper('GetUserIDPasswordReference', '@Result' , '@ApplicationCode', 'JPC' , '@eHubID', 'eHub' , '@IsGettingPassword', '1' )" />
					</xsl:element>
				</xsl:element>

				<!-- Segment: Original -->
				<xsl:element name="ns0:VesselCode">
					<xsl:variable name="VesselCallSign" select="./*[local-name()='Shipment']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPVesselCallSign'][1]/*[local-name()='Value']" />
					<xsl:if test="$VesselCallSign">
						<xsl:value-of select="userCSharp:StringMaxAllowed(normalize-space($VesselCallSign/text()),9)"/>
					</xsl:if>
				</xsl:element>
				<xsl:element name="ns0:VoyageNumber">
					<xsl:value-of select ="userCSharp:StringMaxAllowed(normalize-space(./*[local-name()='Shipment']/*[local-name()='VoyageFlightNo']/text()),10)"/>
				</xsl:element>
				<xsl:element name="ns0:CarrierCode">
					<xsl:variable name="carrierCode" select="userCSharp:StringMaxAllowed( normalize-space(./*[local-name()='Shipment']/*[local-name()='AddInfoCollection']
                        /*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPCarrierCode'][1]
                        /*[local-name()='Value']/text()),4)" />
					<xsl:value-of select ="$carrierCode"/>
				</xsl:element>

				<xsl:element name="ns0:PortOfLoadingCode">
					<xsl:variable name="PortOfLoading" select="userCSharp:StringMaxAllowed(normalize-space(./*[local-name()='Shipment']/*[local-name()='PortOfLoading']/*[local-name()='Code']/text()),5)"/>
					<xsl:value-of select="$PortOfLoading" />
				</xsl:element>
				<xsl:element name="ns0:PortOfLoadingSuffix">
					<xsl:value-of select ="userCSharp:StringMaxAllowed(
                            normalize-space(./*[local-name()='Shipment']/*[local-name()='AddInfoCollection']
                            /*[local-name()='AddInfo'
                            and *[local-name()='Key']/text()='JPPortOfLoadingSuffix'][1]
                            /*[local-name()='Value']/text()),1)"/>
				</xsl:element>

				<xsl:element name="ns0:PortOfDischargeCode">
					<xsl:if test="./*[local-name()='Shipment']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPOperationalCarrierVoyageNo'][1]">
						<xsl:variable name="PortOfDischarge" select="userCSharp:StringMaxAllowed(normalize-space(./*[local-name()='Shipment']/*[local-name()='PortOfDischarge']/*[local-name()='Code']/text()),5)"/>
						<xsl:value-of select="$PortOfDischarge" />
					</xsl:if>
				</xsl:element>

				<!-- Segment: New -->
				<xsl:element name="ns0:VesselCodeNew">
					<xsl:variable name="VesselCodeNew" select="./*[local-name()='Shipment']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPVesselCallSignNew'][1]/*[local-name()='Value']" />
					<xsl:value-of select="userCSharp:StringMaxAllowed(normalize-space($VesselCodeNew/text()),9)"/>
				</xsl:element>
				<xsl:element name="ns0:LadenVesselNameNew">
					<xsl:variable name="LadenVesselNameNew" select="./*[local-name()='Shipment']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPVesselNameNew'][1]/*[local-name()='Value']" />
					<xsl:value-of select="userCSharp:StringMaxAllowed(normalize-space($LadenVesselNameNew/text()),35)"/>
				</xsl:element>
				<xsl:element name="ns0:NationalityCodeOfVesselNew">
					<xsl:variable name="NationalityCodeOfVesselNew" select="./*[local-name()='Shipment']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPVesselCountryNew'][1]/*[local-name()='Value']" />
					<xsl:value-of select="userCSharp:StringMaxAllowed(normalize-space($NationalityCodeOfVesselNew/text()),2)"/>
				</xsl:element>
				<xsl:element name="ns0:OperatingCarrierVoyageNumber">
					<xsl:variable name="OperatingCarrierVoyageNumber" select="./*[local-name()='Shipment']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPOperationalCarrierVoyageNoNew'][1]/*[local-name()='Value']" />
					<xsl:value-of select="userCSharp:StringMaxAllowed(normalize-space($OperatingCarrierVoyageNumber/text()),10)"/>
				</xsl:element>
				<xsl:element name="ns0:VoyageNumberNew">
					<xsl:variable name="VoyageNumberNew" select="./*[local-name()='Shipment']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPVoyageNumberNew'][1]/*[local-name()='Value']" />
					<xsl:value-of select="userCSharp:StringLeftBytes(normalize-space($VoyageNumberNew/text()), 10)"/>
				</xsl:element>
				<xsl:element name="ns0:CarrierCodeNew">
					<xsl:variable name="CarrierCodeNew" select="./*[local-name()='Shipment']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPCarrierCodeNew'][1]/*[local-name()='Value']" />
					<xsl:value-of select="userCSharp:StringLeftBytes(normalize-space($CarrierCodeNew/text()), 4)"/>
				</xsl:element>
				<xsl:variable name="PortOfLoadingCodeNew" select="./*[local-name()='Shipment']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPPortOfLoadingCodeNew'][1]/*[local-name()='Value']" />
				<xsl:element name="ns0:PortOfLoadingCodeNew">
					<xsl:value-of select="userCSharp:StringLeftBytes(normalize-space($PortOfLoadingCodeNew/text()), 5)"/>
				</xsl:element>
				<xsl:element name="ns0:NameOfPortOfLoadingNew">
					<xsl:variable name="NameOfPortOfLoadingNew" select="./*[local-name()='Shipment']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPPortOfLoadingNameNew'][1]/*[local-name()='Value']" />
					<xsl:value-of select="userCSharp:StringLeftBytes(normalize-space($NameOfPortOfLoadingNew/text()), 20)"/>
				</xsl:element>
				<xsl:element name="ns0:PortOfLoadingSuffixNew">
					<xsl:variable name="PortOfLoadingSuffixNew" select="./*[local-name()='Shipment']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPPortOfLoadingSuffixNew'][1]/*[local-name()='Value']" />
					<xsl:value-of select="userCSharp:StringLeftBytes(normalize-space($PortOfLoadingSuffixNew/text()), 1)"/>
				</xsl:element>

				<xsl:variable name="EstimatedTimeOfDepartureNew" select="./*[local-name()='Shipment']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPEstimatedDateTimeOfDepartureNew'][1]/*[local-name()='Value']" />
				<xsl:element name="ns0:EstimatedDateOfDepartureNew">
					<xsl:value-of select="ScriptNS1:FormatXmlDateTime($EstimatedTimeOfDepartureNew, 'yyyyMMdd')"/>
				</xsl:element>
				<xsl:element name="ns0:EstimatedTimeOfDepartureNew">
					<xsl:value-of select="ScriptNS1:FormatXmlDateTime($EstimatedTimeOfDepartureNew, 'HHmm')"/>
				</xsl:element>
				<xsl:element name="ns0:TimeDifferenceFromGreenwichMeanTimeNew">
					<xsl:value-of select="translate(ScriptNS0:CallActionProcedureHelper('CalculateTimeZoneOffset', '@offset', '@UNLOCO', string($PortOfLoadingCodeNew), '@localtime', $EstimatedTimeOfDepartureNew),':','')" />
				</xsl:element>
				<xsl:element name="ns0:RelaxedApplicationAreaIdentifierNew">
					<xsl:variable name="RelaxedApplicationAreaIdentifierNew" select="userCSharp:StringLeftBytes(normalize-space(./*[local-name()='Shipment']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPIsDepartureFromRelaxedAreaNew'][1]/*[local-name()='Value']/text()), 1)" />
					<xsl:if test="$RelaxedApplicationAreaIdentifierNew='Y'">
						<xsl:value-of select="$RelaxedApplicationAreaIdentifierNew"/>
					</xsl:if>
				</xsl:element>

				<xsl:variable name="BlanketChange" select="./*[local-name()='Shipment']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPBlanketChange'][1]/*[local-name()='Value']" />
				<xsl:if test="$BlanketChange!='Y'">
					<xsl:for-each select="//*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='SubShipmentCollection']/*[local-name()='SubShipment']">
						<xsl:element name="ns0:BillNumbersForVesselInformationCorrection">
							<xsl:variable name="BillOfLadingNumber" select="userCSharp:StringMaxAllowed(normalize-space(*[local-name()='WayBillNumber']/text()),35)"/>
							<xsl:value-of select ="$BillOfLadingNumber"/>
						</xsl:element>
					</xsl:for-each>
				</xsl:if>
			</xsl:element>
		</xsl:element>
	</xsl:template>

	<msxsl:script language="C#" implements-prefix="userCSharp">
		<![CDATA[

public string RemoveCountryCode(string original, string formatted)
{
	var result = string.Empty;
	if (formatted.Length > 14 && original.Trim().StartsWith("+"))
	{
		var countryCode = KeepCharsUntil(original, "0123456789", new[] { ' ' });
		if (countryCode.Length > 0 && countryCode.Length < 4)
			result = formatted.Substring(countryCode.Length);
	}
	return result;
}

public string KeepCharsUntil(string input, string charactersToKeep, char[] charactersToStopOnEncountering)
{
	int IndexOfChars = input.IndexOfAny(charactersToStopOnEncountering);
	if (IndexOfChars < 0) return KeepChars(input, charactersToKeep);
	else
	{
		string NewValue = input.Substring(0, IndexOfChars);
		string ZStringValue = NewValue;
		return KeepChars(ZStringValue, charactersToKeep);
	}
}

public string KeepChars(string input, string keepList)
{
	var Result = new System.Text.StringBuilder(input.Length);
	foreach (char C in input)
	{
		if (keepList.IndexOf(C) != -1)
		{
			Result.Append(C);
		}
	}
	return Result.ToString();
}

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

public string StringDecimalMaxAllowed(string text, int precision, int scale)
{
    try
    {
        decimal value = decimal.Round(Convert.ToDecimal(text), scale);
        decimal maximumLimit = decimal.Parse("".PadLeft(precision - scale, '9') + "." + "".PadLeft(scale, '9'));
        return value > maximumLimit ? GetInvalidCharacter(precision + 1) : value.ToString("0." + "".PadLeft(scale,'0'));
    }
    catch
    {
        return GetInvalidCharacter(precision + (scale==0 ? 0 : 1));
    }
}

public string reformatAddress1and2(string testAddress, int p)
{
    testAddress = testAddress.Trim();
    var result = string.Empty;
    var splitArray = testAddress.Split(',');
    var splitArraySize = splitArray.Length;
    bool resultFound = false;
    if (splitArraySize > 1)
    {
        for (int i = 1; i < splitArraySize; i++)
        {
            var result1 = aggregateAddressSplit(splitArray, 0, i);
            var result2 = aggregateAddressSplit(splitArray, i, splitArraySize);
            if (result1.Length < 70 && result2.Length < 35)
            {
                result = (p == 1) ? result1 : result2;
                resultFound = true;
            }
        }
    }

    if (!resultFound)
    {
        int position = (testAddress.Length > 70) ? 70 : (int)(testAddress.Length * 0.5);
        result = (p == 1) ? testAddress.Substring(0, position).Trim() : testAddress.Substring(position).Trim();
    }

    return result;
}

public string aggregateAddressSplit(string[] source, int start, int end)
{
    var result = ",";
    if (start < end && end <= source.Length)
    {
        for (int i = start; i < end; i++)
        {
            result += ("," + source[i]);
        }
    }
    return result.Substring(2).Trim();
}


public string GetContainerSizeCode(string lengthStr, string heightStr)
{
    string result = "";
    decimal length = 0m;
    decimal height = 0m;
    decimal.TryParse(lengthStr, out length);
    decimal.TryParse(heightStr, out height);

    if (length >= 10m && length < 20m)
    {
        result = "1";
    }
    else if (length >= 20m && length < 30m)
    {
        result = "2";
    }
    else if (length >= 40m && length < 50m)
    {
        result = "4";
    }
    else
    {
        result = "9";
    }

    if (height >= 4m && height <= 4.25m)
    {
        result += "8";
    }
    else if (height >= 8m && height < 8.5m)
    {
        result += "0";
    }
    else if (height >= 8.5m && height < 9m)
    {
        result += "2";
    }
    else if (height >= 9m && height < 9.5m)
    {
        result += "4";
    }
    else if (height == 9.5m)
    {
        result += "5";
    }
    else if (height > 9.5m)
    {
        result += "6";
    }
    else
    {
        result += "9";
    }
    return result;
}
        ]]>
	</msxsl:script>
</xsl:stylesheet>

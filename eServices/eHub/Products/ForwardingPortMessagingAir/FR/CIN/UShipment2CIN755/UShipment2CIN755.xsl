<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 CodeMapper ContextAccessor DataModelAccessor DateMapper UnitConverter userCSharp" version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2012/11/CIN755/1"
                xmlns:ns0="http://www.cargowise.com/Schemas/FPMA/CIN/755"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
                xmlns:DataModelAccessor="http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"
                xmlns:DateMapper="http://schemas.microsoft.com/BizTalk/2003/DateMapper"
                xmlns:UnitConverter="http://schemas.microsoft.com/BizTalk/2003/UnitConverter"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes" />

  <xsl:template match="/">
    <xsl:apply-templates select="s0:UniversalShipment/s0:Shipment" />
  </xsl:template>

  <xsl:variable name="senderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')" />
  <xsl:variable name="recipientID" select="ContextAccessor:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')" />

  <xsl:variable name="serviceProvider" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE_AIR', 'FORWARDING_PORT_MESSAGE_AIR', 'FPMA System Configuration', 'Port Settings', 'Name', $recipientID)" />
  <xsl:variable name="serviceProviderMsgID" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE_AIR', 'FORWARDING_PORT_MESSAGE_AIR', 'FPMA System Configuration', 'Port Settings', 'MSGID', $recipientID)" />

  <xsl:template match="/s0:UniversalShipment/s0:Shipment">
    <xsl:variable name="wayBillNumber" select="s0:WayBillNumber/text()" />
    <xsl:variable name="shipmentID" select="s0:DataContext/s0:DataSource/s0:Key/text()" />
    <xsl:variable name="forwardingType" select="s0:DataContext/s0:DataSource/s0:Type/text()" />
    <xsl:variable name="purposeCode" select="s0:DataContext/s0:DocumentaryOverride/s0:Purpose/text()" />
    <xsl:variable name="documentName" select="s0:DataContext/s0:DocumentaryOverride/s0:DocumentName/text()" />
    <xsl:variable name="operationPort" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='OperationalPort_Code']/s0:Value/text()" />
    <xsl:variable name="mrnReferenceCollection" select="s0:AdditionalReferenceCollection/s0:AdditionalReference[s0:Type/text() = 'MRN']" />
    <xsl:variable name="currentTime" select="userCSharp:GetFranceParisZoneTime(DateMapper:CurrentDateTimeUTC('O'))" />

    <xsl:variable name="interchangeID" select="CodeMapper:CallActionProcedureHelper('GetCounterValue', '@value', '@name', concat('CargoWise.eHub.Products.ForwardingPortMessagingAir.FR.', $serviceProvider, '.UNH'), '@maxlength', '14')" />
    <xsl:variable name="formatInterchangeID" select='format-number($interchangeID, "00000000000000")' />

    <xsl:variable name="previousMessageReference" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $serviceProvider, '@recipientId', $senderID, '@ST_ID', $serviceProviderMsgID, '@value', $shipmentID, '@referenceType', 'JobNumber')" />
    <xsl:variable name="messageReferenceNumber">
      <xsl:choose>
        <xsl:when test="($purposeCode = 'AMD' or $purposeCode = 'WTH') and $previousMessageReference!=''">
          <xsl:value-of select="$previousMessageReference" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:variable name="msgCounter" select="CodeMapper:CallActionProcedureHelper('GetCounterValue', '@value', '@name', concat('CargoWise.eHub.Products.ForwardingPortMessagingAir.FR.', $serviceProvider, '.MRN'), '@maxlength', '11')" />
          <xsl:variable name="newMessageReference" select='format-number($msgCounter, "00000000000")' />
          <xsl:variable name="SubScriberValue1" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMsgID, $serviceProvider, $senderID, $newMessageReference, $shipmentID, 'JobNumber')" />
          <xsl:variable name="SubScriberValue2" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMsgID, $serviceProvider, $senderID, $shipmentID, $newMessageReference, 'JobNumber')" />
          <xsl:value-of select="$newMessageReference" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="subscribedForwardingType" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMsgID, $serviceProvider, $senderID, $messageReferenceNumber, $forwardingType, 'ForwardingType')" />
    <xsl:variable name="subscribedDocumentName" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMsgID, $serviceProvider, $senderID, $messageReferenceNumber, $documentName, 'DocumentName')" />
    <xsl:variable name="subscribedActionPurpose" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMsgID, $serviceProvider, $senderID, $messageReferenceNumber, $purposeCode, 'ActionPurpose')" />
    <xsl:variable name="subscribedOperationPort" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMsgID, $serviceProvider, $senderID, $messageReferenceNumber, $operationPort, 'OperationPort')" />

    <xsl:variable name="fileName" select="ContextAccessor:SetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06', concat('CIN755', '_', $senderID, '_', $formatInterchangeID))" />

    <xsl:variable name="InboxPK" select="ContextAccessor:GetContextProperty('InternalTrackingID', 'http://cargowise.com/ehub/tracking/2010/06')" />
    <xsl:variable name="SubscribeInboxPK" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMsgID, $serviceProvider, $senderID, $InboxPK, $messageReferenceNumber)" />

    <ns0:CIN_755>
      <ns0:UNB>
        <ns0:TagCode>UNB</ns0:TagCode>
        <ns0:UNB1_Syntax>
          <ns0:UNG1.1>UNOC</ns0:UNG1.1>
          <ns0:UNG1.2>3</ns0:UNG1.2>
        </ns0:UNB1_Syntax>
        <ns0:UNB2_Sender>
          <xsl:value-of select="DataModelAccessor:GetClientRegistrationCode($senderID, s0:DataContext/s0:EventBranch/s0:Code/text(), $serviceProvider)" />
        </ns0:UNB2_Sender>
        <ns0:UNB3_Recipient>DGV2</ns0:UNB3_Recipient>
        <ns0:UNB4_DateTime>
          <ns0:UNB4.1>
            <xsl:value-of select="DateMapper:ConvertToDateTimeString($currentTime, 'yyyyMMddHHmm', 'yyMMdd')" />
          </ns0:UNB4.1>
          <ns0:UNG4.2>
            <xsl:value-of select="DateMapper:ConvertToDateTimeString($currentTime, 'yyyyMMddHHmm', 'HHmm')" />
          </ns0:UNG4.2>
        </ns0:UNB4_DateTime>
        <ns0:UNB5_UniqueMessageRef>
          <xsl:value-of select="$formatInterchangeID"/>
        </ns0:UNB5_UniqueMessageRef>
      </ns0:UNB>

      <ns0:UNH>
        <ns0:TagCode>UNH</ns0:TagCode>
        <ns0:UniqueMessageReference>
          <xsl:value-of select="$formatInterchangeID"/>
        </ns0:UniqueMessageReference>
        <ns0:UNH_Messageype>
          <ns0:MessageType>755</ns0:MessageType>
          <ns0:Version>2</ns0:Version>
        </ns0:UNH_Messageype>
      </ns0:UNH>

      <xsl:call-template name="BGM">
        <xsl:with-param name="messageSequenceNumber" select="normalize-space(substring($formatInterchangeID, 4, 11))" />
      </xsl:call-template>

      <xsl:call-template name="DTM">
        <xsl:with-param name="dateTime" select="$currentTime" />
      </xsl:call-template>

      <xsl:call-template name="RFF">
        <xsl:with-param name="type" select="'AWB'" />
        <xsl:with-param name="value">
          <xsl:choose>
            <xsl:when test="$purposeCode='WTH'"></xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="userCSharp:StringReplace(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='MAWB']/s0:Value/text(), '-', '')" />
            </xsl:otherwise>
          </xsl:choose>
        </xsl:with-param>
        <xsl:with-param name="generateIfEmpty" select="'Y'" />
      </xsl:call-template>

      <xsl:call-template name="RFF">
        <xsl:with-param name="type" select="'ACD'" />
        <xsl:with-param name="value" select="$messageReferenceNumber" />
        <xsl:with-param name="suffix" select="'_2'" />
      </xsl:call-template>

      <xsl:call-template name="NAD">
        <xsl:with-param name="qualifierCode" select="'GY'" />
        <xsl:with-param name="partyCode" select="normalize-space(substring(s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='CurrentUser']/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='CIN']/s0:Value/text(), 1, 3))" />
      </xsl:call-template>

      <xsl:call-template name="NAD_2">
        <xsl:with-param name="qualifierCode" select="'ST'" />
        <xsl:with-param name="partyCode" select="normalize-space(substring(s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='Carrier']/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='CIN']/s0:Value/text(), 1, 3))" />
      </xsl:call-template>

      <xsl:variable name="wayBillType" select="s0:WayBillType/text()" />
      <xsl:variable name="departureCFSAddressCIN" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='DepartureCFSAddress']/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='CIN']/s0:Value/text()" />
      <xsl:variable name="totalNoOfPacks" select="s0:TotalNoOfPacks/text()" />
      <xsl:variable name="totalWeight" select="UnitConverter:Convert(s0:TotalWeight/text(), s0:TotalWeightUnit/text(), 'KG')" />
      <xsl:variable name="cocNumber" select="s0:AdditionalReferenceCollection/s0:AdditionalReference[s0:Type/text() = 'COC']/s0:ReferenceNumber/text()" />

      <xsl:for-each select="$mrnReferenceCollection">
        <xsl:call-template name="SG01">
          <xsl:with-param name="wayBillType" select="$wayBillType" />
          <xsl:with-param name="wayBillNumber" select="$wayBillNumber" />
          <xsl:with-param name="departureCFSAddressCIN" select="$departureCFSAddressCIN" />
          <xsl:with-param name="totalNoOfPacks" select="$totalNoOfPacks" />
          <xsl:with-param name="totalWeight" select="format-number($totalWeight, '0.00')" />
          <xsl:with-param name="cocNumber" select="$cocNumber" />
          <xsl:with-param name="mrnNumber" select="s0:ReferenceNumber/text()" />
          <xsl:with-param name="dateTime" select="DateMapper:ConvertToDateTimeString($currentTime, 'yyyyMMddHHmm', 'yyyyMMdd') " />
          <xsl:with-param name="messageReferenceNumber" select="$messageReferenceNumber" />
        </xsl:call-template>
      </xsl:for-each>

      <ns0:UNT>
        <ns0:TagCode>UNT</ns0:TagCode>
        <ns0:RegistrationDate>
          <xsl:value-of select="userCSharp:GetSegCounter()" />
        </ns0:RegistrationDate>
        <ns0:UNT_UNH_UniqueMessageReference>
          <xsl:value-of select="$formatInterchangeID" />
        </ns0:UNT_UNH_UniqueMessageReference>
      </ns0:UNT>

      <ns0:UNZ>
        <ns0:TagCode>UNZ</ns0:TagCode>
        <ns0:UNZ_InterchangeCount>1</ns0:UNZ_InterchangeCount>
        <ns0:UNZ_InterchangeReference>
          <xsl:value-of select="$formatInterchangeID" />
        </ns0:UNZ_InterchangeReference>
      </ns0:UNZ>
    </ns0:CIN_755>
  </xsl:template>

  <xsl:template name="SG01">
    <xsl:param name="wayBillType" />
    <xsl:param name="wayBillNumber" />
    <xsl:param name="departureCFSAddressCIN" />
    <xsl:param name="totalNoOfPacks" />
    <xsl:param name="totalWeight" />
    <xsl:param name="cocNumber" />
    <xsl:param name="mrnNumber" />
    <xsl:param name="dateTime" />
    <xsl:param name="messageReferenceNumber" />

    <xsl:variable name="subscribedMRN" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMsgID, $serviceProvider, $senderID, $mrnNumber, $messageReferenceNumber, 'MRN')" />

    <ns0:LINLoop1>
      <xsl:call-template name="LIN" />

      <xsl:call-template name = "RFF">
        <xsl:with-param name="type" select="$wayBillType" />
        <xsl:with-param name="value" select="$wayBillNumber" />
        <xsl:with-param name="suffix" select="'_3'" />
      </xsl:call-template>

      <xsl:call-template name="LOC">
        <xsl:with-param name="locationCode" select ="$departureCFSAddressCIN" />
      </xsl:call-template>

      <xsl:call-template name="QTY">
        <xsl:with-param name="totalNoOfPacks" select ="$totalNoOfPacks" />
      </xsl:call-template>

      <xsl:call-template name="MEA">
        <xsl:with-param name="totalWeight" select="$totalWeight" />
      </xsl:call-template>

      <xsl:call-template name="GIS" />

      <ns0:VIALoop1>
        <xsl:call-template name="VIA" >
          <xsl:with-param name="mrnNumber" select="$mrnNumber" />
        </xsl:call-template>

        <xsl:call-template name="NAD_3">
          <xsl:with-param name="customExitOfficeCode" select="$cocNumber" />
        </xsl:call-template>

        <xsl:call-template name="DTM_2">
          <xsl:with-param name="dateTime" select="$dateTime" />
        </xsl:call-template>
      </ns0:VIALoop1>
    </ns0:LINLoop1>
  </xsl:template>

  <xsl:template name="BGM">
    <xsl:param name="messageSequenceNumber" />

    <xsl:variable name="segCounter" select="userCSharp:AddSegCounter()" />
    <ns0:BGM>
      <ns0:TagCode>BGM</ns0:TagCode>
      <ns0:MouvementCode>35</ns0:MouvementCode>
      <ns0:MessageSequenceNumber>
        <xsl:value-of select="$messageSequenceNumber"/>
      </ns0:MessageSequenceNumber>
      <ns0:MessageVersionNumber>01</ns0:MessageVersionNumber>
    </ns0:BGM>
  </xsl:template>

  <xsl:template name="DTM">
    <xsl:param name="dateTime" />

    <xsl:if test="$dateTime != ''">
      <xsl:variable name="segCounter" select="userCSharp:AddSegCounter()" />
      <ns0:DTM>
        <ns0:TagCode>DTM</ns0:TagCode>
        <ns0:DateTime>
          <ns0:QualifierCode>137</ns0:QualifierCode>
          <ns0:DateTimeValue>
            <xsl:value-of select="$dateTime" />
          </ns0:DateTimeValue>
          <ns0:DateTimeFormat>204</ns0:DateTimeFormat>
        </ns0:DateTime>
      </ns0:DTM>
    </xsl:if>
  </xsl:template>

  <xsl:template name="DTM_2">
    <xsl:param name="dateTime" />

    <xsl:if test="$dateTime != ''">
      <xsl:variable name="segCounter" select="userCSharp:AddSegCounter()" />
      <ns0:DTM_2>
        <ns0:TagCode>DTM</ns0:TagCode>
        <ns0:DateTime>
          <ns0:QualifierCode>254</ns0:QualifierCode>
          <ns0:DateTimeValue>
            <xsl:value-of select="$dateTime" />
          </ns0:DateTimeValue>
          <ns0:DateTimeFormat>102</ns0:DateTimeFormat>
        </ns0:DateTime>
      </ns0:DTM_2>
    </xsl:if>
  </xsl:template>

  <xsl:template name="RFF">
    <xsl:param name="type" />
    <xsl:param name="value" />
    <xsl:param name="suffix" select="''" />
    <xsl:param name="generateIfEmpty" select="'N'"/>

    <xsl:if test="$value != '' or $generateIfEmpty='Y'">
      <xsl:variable name="segCounter" select="userCSharp:AddSegCounter()" />
      <xsl:element name="ns0:RFF{$suffix}">
        <ns0:TagCode>RFF</ns0:TagCode>
        <ns0:Reference>
          <ns0:Type>
            <xsl:value-of select="$type"/>
          </ns0:Type>
          <ns0:Value>
            <xsl:value-of select="$value"/>
          </ns0:Value>
        </ns0:Reference>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="NAD">
    <xsl:param name="qualifierCode" />
    <xsl:param name="partyCode" />

    <xsl:if test="$partyCode != ''">
      <xsl:variable name="segCounter" select="userCSharp:AddSegCounter()" />
      <ns0:NAD>
        <ns0:TagCode>NAD</ns0:TagCode>
        <ns0:QualifierCode>
          <xsl:value-of select="$qualifierCode" />
        </ns0:QualifierCode>
        <ns0:PartyCode>
          <xsl:value-of select="$partyCode" />
        </ns0:PartyCode>
      </ns0:NAD>
    </xsl:if>
  </xsl:template>

  <xsl:template name="NAD_2">
    <xsl:param name="qualifierCode" />
    <xsl:param name="partyCode" />

    <xsl:if test="$partyCode != ''">
      <xsl:variable name="segCounter" select="userCSharp:AddSegCounter()" />
      <ns0:NAD_2>
        <ns0:TagCode>NAD</ns0:TagCode>
        <ns0:QualifierCode>
          <xsl:value-of select="$qualifierCode" />
        </ns0:QualifierCode>
        <ns0:PartyCode>
          <xsl:value-of select="$partyCode" />
        </ns0:PartyCode>
      </ns0:NAD_2>
    </xsl:if>
  </xsl:template>

  <xsl:template name="NAD_3">
    <xsl:param name="customExitOfficeCode" />

    <xsl:if test="$customExitOfficeCode != ''">
      <xsl:variable name="segCounter" select="userCSharp:AddSegCounter()" />
      <ns0:NAD_3>
        <ns0:TagCode>NAD</ns0:TagCode>
        <ns0:QualifierCode>AM</ns0:QualifierCode>
        <ns0:NotUsed01 />
        <ns0:NotUsed02 />
        <ns0:NotUsed03 />
        <ns0:NotUsed04 />
        <ns0:CustomExitOfficeCode>
          <xsl:value-of select="$customExitOfficeCode" />
        </ns0:CustomExitOfficeCode>
      </ns0:NAD_3>
    </xsl:if>
  </xsl:template>

  <xsl:template name="VIA">
    <xsl:param name="mrnNumber" />

    <xsl:if test="$mrnNumber != ''">
      <xsl:variable name="segCounter" select="userCSharp:AddSegCounter()" />
      <ns0:VIA>
        <ns0:TagCode>VIA</ns0:TagCode>
        <ns0:Detail>
          <ns0:DocumentType>M</ns0:DocumentType>
          <ns0:ProcedureCode>EX</ns0:ProcedureCode>
          <ns0:MRNNumber>
            <xsl:value-of select="$mrnNumber" />
          </ns0:MRNNumber>
        </ns0:Detail>
      </ns0:VIA>
    </xsl:if>
  </xsl:template>

  <xsl:template name ="GIS">
    <xsl:variable name="segCounter" select="userCSharp:AddSegCounter()" />
    <ns0:GIS>
      <ns0:TagCode>GIS</ns0:TagCode>
      <ns0:Detail>
        <ns0:GoodIdentifier>N</ns0:GoodIdentifier>
        <ns0:CustomsProcedure>117</ns0:CustomsProcedure>
        <ns0:CustomsIdentifier>106</ns0:CustomsIdentifier>
      </ns0:Detail>
    </ns0:GIS>
  </xsl:template>

  <xsl:template name="MEA">
    <xsl:param name="totalWeight" />

    <xsl:if test="$totalWeight != ''">
      <xsl:variable name="segCounter" select="userCSharp:AddSegCounter()" />
      <ns0:MEA>
        <ns0:TagCode>MEA</ns0:TagCode>
        <ns0:QualifierCode>AAF</ns0:QualifierCode>
        <ns0:Type>AAB</ns0:Type>
        <ns0:MeasurementDetail>
          <ns0:UnitCode>19</ns0:UnitCode>
          <ns0:Weight>
            <xsl:value-of select="$totalWeight" />
          </ns0:Weight>
        </ns0:MeasurementDetail>
      </ns0:MEA>
    </xsl:if>
  </xsl:template>

  <xsl:template name ="QTY">
    <xsl:param name="totalNoOfPacks" />

    <xsl:if test="$totalNoOfPacks != ''">
      <xsl:variable name="segCounter" select="userCSharp:AddSegCounter()" />
      <ns0:QTY>
        <ns0:TagCode>QTY</ns0:TagCode>
        <ns0:Inventory>
          <ns0:MovementQuantity>156</ns0:MovementQuantity>
          <ns0:Quantity>
            <xsl:value-of select="$totalNoOfPacks" />
          </ns0:Quantity>
          <ns0:UnitCode>COL</ns0:UnitCode>
        </ns0:Inventory>
      </ns0:QTY>
    </xsl:if>
  </xsl:template>

  <xsl:template name="LOC">
    <xsl:param name="locationCode" />

    <xsl:if test="$locationCode != ''">
      <xsl:variable name="segCounter" select="userCSharp:AddSegCounter()" />
      <ns0:LOC>
        <ns0:TagCode>LOC</ns0:TagCode>
        <ns0:LocationCode>
          <xsl:value-of select="$locationCode" />
        </ns0:LocationCode>
      </ns0:LOC>
    </xsl:if>
  </xsl:template>

  <xsl:template name="LIN">
    <xsl:variable name="segCounter" select="userCSharp:AddSegCounter()" />
    <ns0:LIN>
      <ns0:TagCode>LIN</ns0:TagCode>
      <ns0:LineNo>
        <xsl:value-of select="userCSharp:LINCounter()"/>
      </ns0:LineNo>
    </ns0:LIN>
  </xsl:template>

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[
int linCounter = 1;
int segCounter = 2;

public void AddSegCounter() {
  ++segCounter;
}

public int GetSegCounter() {
  return segCounter;
}

public int LINCounter()
{
  return linCounter++;
}


public string GetFranceParisZoneTime(string utcString)
{
  TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
  DateTime utc = DateTime.Parse(utcString, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind);
  return TimeZoneInfo.ConvertTimeFromUtc(utc, tz).ToString("yyyyMMddHHmm");
}

public string StringReplace(string text, string oldValue, string newValue)
{
  if (text != null && text.Length > 0)
  {
    return text.Replace(oldValue, newValue);
  }
  return "";
}
]]>
  </msxsl:script>
</xsl:stylesheet>
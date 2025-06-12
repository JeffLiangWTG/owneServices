<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 CodeMapper ContextAccessor DataModelAccessor DateMapper UnitConverter userCSharp" version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2012/11/CGN755/1"
                xmlns:ns0="http://www.cargowise.com/Schemas/FPMA/CGN/755"
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

    <xsl:variable name="forwardingKey" select="s0:DataContext/s0:DataSource/s0:Key/text()" />
    <xsl:variable name="forwardingType" select="s0:DataContext/s0:DataSource/s0:Type/text()" />
    <xsl:variable name="purposeCode" select="s0:DataContext/s0:DocumentaryOverride/s0:Purpose/text()" />
    <xsl:variable name="documentName" select="s0:DataContext/s0:DocumentaryOverride/s0:DocumentName/text()" />
    <xsl:variable name="operationPort" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='OperationalPort_Code']/s0:Value/text()" />
    <xsl:variable name="currentTime" select="userCSharp:GetGMT2Time(DateMapper:CurrentDateTimeUTC('yyyyMMddHHmm'))" />
    <xsl:variable name="mAWBNumber" select="userCSharp:StringReplace(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='MAWB']/s0:Value/text(), '-', '')" />

    <xsl:variable name="interchangeID" select="CodeMapper:CallActionProcedureHelper('GetCounterValue', '@value', '@name', concat('CargoWise.eHub.Products.ForwardingPortMessagingAir.NL.', $serviceProvider, '.UNH'), '@maxlength', '14')" />
    <xsl:variable name="formatInterchangeID" select='format-number($interchangeID, "00000000000000")' />

    <xsl:variable name="previousMessageReference" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $serviceProvider, '@recipientId', $senderID, '@ST_ID', $serviceProviderMsgID, '@value', $forwardingKey, '@referenceType', 'JobNumber')" />
    <xsl:variable name="messageReferenceNumber">
      <xsl:choose>
        <xsl:when test="($purposeCode = 'AMD' or $purposeCode = 'WTH') and $previousMessageReference!=''">
          <xsl:value-of select="$previousMessageReference" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:variable name="msgCounter" select="CodeMapper:CallActionProcedureHelper('GetCounterValue', '@value', '@name', concat('CargoWise.eHub.Products.ForwardingPortMessagingAir.NL.', $serviceProvider, '.MRN'), '@maxlength', '11')" />
          <xsl:variable name="newMessageReference" select='format-number($msgCounter, "00000000000")' />
          <xsl:variable name="SubScriberValue1" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMsgID, $serviceProvider, $senderID, $newMessageReference, $forwardingKey, 'JobNumber')" />
          <xsl:variable name="SubScriberValue2" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMsgID, $serviceProvider, $senderID, $forwardingKey, $newMessageReference, 'JobNumber')" />
          <xsl:value-of select="$newMessageReference" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="subscribedForwardingType" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMsgID, $serviceProvider, $senderID, $messageReferenceNumber, $forwardingType, 'ForwardingType')" />
    <xsl:variable name="subscribedDocumentName" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMsgID, $serviceProvider, $senderID, $messageReferenceNumber, $documentName, 'DocumentName')" />
    <xsl:variable name="subscribedActionPurpose" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMsgID, $serviceProvider, $senderID, $messageReferenceNumber, $purposeCode, 'ActionPurpose')" />
    <xsl:variable name="subscribedOperationPort" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMsgID, $serviceProvider, $senderID, $messageReferenceNumber, $operationPort, 'OperationPort')" />

    <xsl:variable name="fileName" select="ContextAccessor:SetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06', concat('CGN755', '_', $senderID, '_', $formatInterchangeID))" />

    <xsl:variable name="InboxPK" select="ContextAccessor:GetContextProperty('InternalTrackingID', 'http://cargowise.com/ehub/tracking/2010/06')" />
    <xsl:variable name="SubscribeInboxPK" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMsgID, $serviceProvider, $senderID, $InboxPK, $messageReferenceNumber)" />

    <ns0:CGN755>

      <ns0:UNB>
        <ns0:TagCode>UNB</ns0:TagCode>
        <ns0:UNB1>
          <ns0:UNB1.1>UNOC</ns0:UNB1.1>
          <ns0:UNB1.2>3</ns0:UNB1.2>
        </ns0:UNB1>
        <ns0:UNB2>
          <ns0:UNB2.1>
            <xsl:value-of select="DataModelAccessor:GetClientRegistrationCode($senderID,  s0:DataContext/s0:Workflow/s0:EventBranch/text(), $serviceProvider)" />
          </ns0:UNB2.1>
        </ns0:UNB2>
        <ns0:UNB3>
          <ns0:UNB3.1>ECB</ns0:UNB3.1>
        </ns0:UNB3>
        <ns0:UNB4>
          <ns0:UNB4.1>
            <xsl:value-of select="DateMapper:ConvertToDateTimeString($currentTime, 'yyyyMMddHHmm', 'yyMMdd')" />
          </ns0:UNB4.1>
          <ns0:UNB4.2>
            <xsl:value-of select="DateMapper:ConvertToDateTimeString($currentTime, 'yyyyMMddHHmm', 'HHmm')" />
          </ns0:UNB4.2>
        </ns0:UNB4>
        <ns0:UNB5>
          <ns0:UNB5.1>
            <xsl:value-of select="$formatInterchangeID"/>
          </ns0:UNB5.1>
        </ns0:UNB5>
      </ns0:UNB>

      <ns0:UNH>
        <ns0:TagCode>UNH</ns0:TagCode>
        <ns0:UNH1>
          <xsl:value-of select="$formatInterchangeID"/>
        </ns0:UNH1>
        <ns0:UNH2>
          <ns0:UNH2.1>755</ns0:UNH2.1>
          <ns0:UNH2.2>3</ns0:UNH2.2>
        </ns0:UNH2>
      </ns0:UNH>

      <ns0:BGM>
        <ns0:TagCode>BGM</ns0:TagCode>
        <ns0:BGM1>
          <ns0:BGM1.1>833</ns0:BGM1.1>
        </ns0:BGM1>
      </ns0:BGM>

      <ns0:DTM>
        <ns0:TagCode>DTM</ns0:TagCode>
        <ns0:DTM1>
          <ns0:DTM1.1>184</ns0:DTM1.1>
          <ns0:DTM1.2>
            <xsl:value-of select="$currentTime" />
          </ns0:DTM1.2>
          <ns0:DTM1.3>203</ns0:DTM1.3>
        </ns0:DTM1>
      </ns0:DTM>

      <ns0:RFF>
        <ns0:TagCode>RFF</ns0:TagCode>
        <ns0:RFF1>
          <ns0:RFF1.1>ACD</ns0:RFF1.1>
          <ns0:RFF1.2>
            <xsl:value-of select="$messageReferenceNumber" />
          </ns0:RFF1.2>
        </ns0:RFF1>
      </ns0:RFF>

      <ns0:RFF_2>
        <ns0:TagCode>RFF</ns0:TagCode>
        <ns0:RFF2_1>
          <ns0:RFF2_1.1>AWB</ns0:RFF2_1.1>
          <ns0:RFF2_1.2>
            <xsl:value-of select="$mAWBNumber" />
          </ns0:RFF2_1.2>
        </ns0:RFF2_1>
      </ns0:RFF_2>

      <ns0:NAD>
        <ns0:TagCode>NAD</ns0:TagCode>
        <ns0:NAD1>GY</ns0:NAD1>
        <ns0:NAD2>
          <ns0:NAD2.1>
            <xsl:value-of select="normalize-space(substring(s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='CurrentUser']/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='CGN']/s0:Value/text(), 1, 3))" />
          </ns0:NAD2.1>
        </ns0:NAD2>
      </ns0:NAD>

      <ns0:NAD_2>
        <ns0:TagCode>NAD</ns0:TagCode>
        <ns0:NAD2_1>ST</ns0:NAD2_1>
        <ns0:NAD2_2>
          <ns0:NAD2_2.1>
            <xsl:value-of select="normalize-space(substring(s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='Carrier']/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='CGN']/s0:Value/text(), 1, 3))" />
          </ns0:NAD2_2.1>
        </ns0:NAD2_2>
      </ns0:NAD_2>

      <xsl:for-each select="s0:SubShipmentCollection/s0:SubShipment">

        <xsl:variable name="mrnReferenceCollection" select="s0:AdditionalReferenceCollection/s0:AdditionalReference[s0:Type/text() = 'MRN' and s0:ReferenceNumber/text() != '']" />
        <xsl:if test="$mrnReferenceCollection != ''">
          <xsl:variable name="totalNoOfPacks" select="s0:TotalNoOfPacks/text()" />
          <xsl:variable name="totalWeight" select="UnitConverter:Convert(s0:TotalWeight/text(), s0:TotalWeightUnit/text(), 'KG', 2)" />
          <ns0:LINLoop1>
            <xsl:for-each select="$mrnReferenceCollection">

              <xsl:variable name="mrnReferenceNumber" select="s0:ReferenceNumber/text()" />
              <xsl:variable name="subscribedMRN" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMsgID, $serviceProvider, $senderID, $mrnReferenceNumber, $messageReferenceNumber, 'MRN')" />

              <ns0:LIN>
                <ns0:TagCode>LIN</ns0:TagCode>
                <ns0:LIN1>
                  <xsl:value-of select="userCSharp:LINCounter()"/>
                </ns0:LIN1>
              </ns0:LIN>

              <ns0:RFF_3>
                <ns0:TagCode>RFF</ns0:TagCode>
                <ns0:RFF3_1>
                  <ns0:RFF3_1.1>AWB</ns0:RFF3_1.1>
                  <ns0:RFF3_1.2>
                    <xsl:value-of select="$mAWBNumber" />
                  </ns0:RFF3_1.2>
                </ns0:RFF3_1>
              </ns0:RFF_3>

              <ns0:CST>
                <ns0:TagCode>CST</ns0:TagCode>
                <ns0:CST1></ns0:CST1>
                <ns0:CST2>
                  <ns0:CST2.1>
                    <xsl:value-of select="$mrnReferenceNumber" />
                  </ns0:CST2.1>
                  <ns0:CST2.2>EX</ns0:CST2.2>
                </ns0:CST2>
              </ns0:CST>

              <ns0:QTY>
                <ns0:TagCode>QTY</ns0:TagCode>
                <ns0:QTY1>
                  <ns0:QTY1.1>156</ns0:QTY1.1>
                  <ns0:QTY1.2>
                    <xsl:value-of select="$totalNoOfPacks" />
                  </ns0:QTY1.2>
                  <ns0:QTY1.3>COL</ns0:QTY1.3>
                </ns0:QTY1>
              </ns0:QTY>

              <ns0:MEA>
                <ns0:TagCode>MEA</ns0:TagCode>
                <ns0:MEA1>AAF</ns0:MEA1>
                <ns0:MEA2>
                  <ns0:MEA2.1>AAB</ns0:MEA2.1>
                </ns0:MEA2>
                <ns0:MEA3>
                  <ns0:MEA3.1>E4</ns0:MEA3.1>
                  <ns0:MEA3.2>
                    <xsl:value-of select="$totalWeight" />
                  </ns0:MEA3.2>
                </ns0:MEA3>
              </ns0:MEA>

              <ns0:GEI>
                <ns0:TagCode>GEI</ns0:TagCode>
                <ns0:GEI1>5</ns0:GEI1>
                <ns0:GEI2>
                  <ns0:GEI2.4>M</ns0:GEI2.4>
                </ns0:GEI2>
              </ns0:GEI>

            </xsl:for-each>
          </ns0:LINLoop1>
        </xsl:if>
      </xsl:for-each>

      <ns0:UNT>
        <ns0:TagCode>UNT</ns0:TagCode>
        <ns0:UNT1>
          <xsl:value-of select="userCSharp:GetSegmentsCount()" />
        </ns0:UNT1>
        <ns0:UNT2>
          <xsl:value-of select="$formatInterchangeID" />
        </ns0:UNT2>
      </ns0:UNT>

      <ns0:UNZ>
        <ns0:TagCode>UNZ</ns0:TagCode>
        <ns0:UNZ1>1</ns0:UNZ1>
        <ns0:UNZ2>
          <xsl:value-of select="$formatInterchangeID" />
        </ns0:UNZ2>
      </ns0:UNZ>
    </ns0:CGN755>

  </xsl:template>

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[

int linCounter = 0;

public int LINCounter()
{
  return ++linCounter;
}

public int GetSegmentsCount()
{
  return 8 + linCounter * 6;
}

public string GetGMT2Time(string utc)
{
  DateTime result;
  if (DateTime.TryParseExact(utc, "yyyyMMddHHmm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out result))
  {
    return result.AddHours(2).ToString("yyyyMMddHHmm");
  }
  return "";
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

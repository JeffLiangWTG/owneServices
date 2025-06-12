<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 CodeMapper XmlHelper ContextAccessor DataModelAccessor DateMapper StringMapper eHubAsyncPollingRegistrationHelper userCSharp" version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2012/11/CCTHouseCheckList/1"
                xmlns:ns2="iata:housemanifest:1"
                xmlns:ns3="iata:datamodel:3"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:XmlHelper="http://schemas.microsoft.com/BizTalk/2003/XmlHelper"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
                xmlns:DataModelAccessor="http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"
                xmlns:DateMapper="http://schemas.microsoft.com/BizTalk/2003/DateMapper"
                xmlns:StringMapper="http://schemas.microsoft.com/BizTalk/2003/StringMapper"
                xmlns:eHubAsyncPollingRegistrationHelper="http://schemas.microsoft.com/BizTalk/2003/eHubAsyncPollingRegistrationHelper"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes" />

  <xsl:template match="/">
    <xsl:apply-templates select="s0:UniversalShipment/s0:Shipment" />
  </xsl:template>

  <xsl:template match="s0:UniversalShipment/s0:Shipment">

    <xsl:variable name="RecipientID" select="ContextAccessor:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
    <xsl:variable name="SenderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>

    <!--<xsl:variable name="carrierID" select="'ACAS_BR'" />-->
    <xsl:variable name="ACASRecipientID">
      <xsl:choose>
        <xsl:when test="contains($RecipientID, 'TST')">ACAS_BRTest</xsl:when>
        <xsl:otherwise>ACAS_BR</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="subscriptionType" select="'ACASBR'" />
    <xsl:variable name="dataVersion" select="s0:DataContext/s0:DocumentaryOverride/s0:DataVersion/text()" />
    <xsl:variable name="waybillNumber" select="userCSharp:ToUpper(s0:WayBillNumber/text())"/>
    <xsl:variable name="consolNumber" select="s0:DataContext/s0:DataSource/s0:Key/text()" />
    <xsl:variable name="documentType" select="s0:DataContext/s0:DataSource/s0:Type/text()" />
    <xsl:variable name="purpose" select="s0:DataContext/s0:DocumentaryOverride/s0:Purpose/text()" />
    <xsl:variable name="eventUserID" select="s0:DataContext/s0:Workflow/s0:EventUser/text()" />

    <xsl:variable name="InterchangeNum" select="CodeMapper:CallActionProcedureHelper('GetCounterValue','@value','@name','CargoWise.eHub.Products.ACAS.BR.Transforms.CCT','@maxlength','14')" />

    <!--Jeeva: Hide this at the moment, not sure we need it. TBC during UAT-->
    <!--<xsl:variable name="previousShipmentReference" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $carrierID, '@recipientId', $SenderID, '@ST_ID', $subscriptionType, '@value', $consolNumber, '@referenceType', 'ShipmentId')" />
    <xsl:variable name="subscriberShipmentReference">
      <xsl:choose>
        <xsl:when test="$previousShipmentReference!=''">
          <xsl:value-of select="$previousShipmentReference" />
        </xsl:when>
        <xsl:when test="$dataVersion!='1'">
          <xsl:value-of select="$consolNumber" />
        </xsl:when>

        <xsl:otherwise>-->
    <xsl:variable name="FormattedCounter" select='format-number($InterchangeNum, "0000000000")' />
    <xsl:variable name="formattedInterchangeNumber" select="concat('FHL', $FormattedCounter)" />
    <xsl:variable name="subscriberShipmentReference"  select="concat($consolNumber, '_', $formattedInterchangeNumber)" />

    <xsl:variable name="subscribeShipmentID" select="DataModelAccessor:InsertSubscriptionValue($subscriptionType, $ACASRecipientID, $SenderID, $formattedInterchangeNumber, $subscriberShipmentReference, 'InterchangeNum')" />


    <xsl:if test="$consolNumber!=$waybillNumber">
      <xsl:variable name="subscribeWaybillNumber" select="DataModelAccessor:InsertSubscriptionValue($subscriptionType, $ACASRecipientID, $SenderID, $subscriberShipmentReference, $waybillNumber, 'FHL-HWB')" />
    </xsl:if>
    <!--<xsl:value-of select="$newShipmentReference" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>-->

    <xsl:variable name="SubscribeShipmentId" select="DataModelAccessor:InsertSubscriptionValue($subscriptionType, $ACASRecipientID, $SenderID, $subscriberShipmentReference, $consolNumber, 'ShipmentId')" />
    <xsl:variable name="SubscribeDocumentName" select="DataModelAccessor:InsertSubscriptionValue($subscriptionType, $ACASRecipientID, $SenderID, $subscriberShipmentReference, s0:DataContext/s0:DocumentaryOverride/s0:DocumentName, 'DocumentName')" />
    <xsl:variable name="SubscribeForwardingType" select="DataModelAccessor:InsertSubscriptionValue($subscriptionType, $ACASRecipientID, $SenderID, $subscriberShipmentReference, 'ForwardingConsol', 'ForwardingType')" />
    <xsl:variable name="SubscribeActionPurpose" select="DataModelAccessor:InsertSubscriptionValue($subscriptionType, $ACASRecipientID, $SenderID, $subscriberShipmentReference, $purpose, 'ActionPurpose')" />

    <xsl:variable name="currentDateTimeUTC" select="DateMapper:CurrentDateTimeUTC('yyyyMMddHHmm')"/>
    <xsl:variable name="messageDateTime" select="DateMapper:ConvertToDateTimeString($currentDateTimeUTC, 'yyyyMMddHHmm', 'yyyy-MM-ddTHH:mm:ss')"/>

    <xsl:variable name="receivingForwarderAddress" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='ReceivingForwarderAddress']"/>

    <xsl:variable name="PR_PK" select="eHubAsyncPollingRegistrationHelper:InsertEHubAsyncPollingRegistration($SenderID, $RecipientID, $eventUserID, userCSharp:RemoveSpecialCharacters($receivingForwarderAddress/s0:GovRegNum/text()), $consolNumber, $documentType, $subscriberShipmentReference, 'http://www.cargowise.com/Schemas/Universal/2012/11/CCTHouseCheckList/1', $messageDateTime)" />
    <xsl:variable name="overrideEmailSubject" select="ContextAccessor:SetContextProperty('OverrideEmailSubject', 'http://cargowise.com/ehub/processing/2010/06', $PR_PK)"/>

    <ns2:HouseManifest>
      <ns2:MessageHeaderDocument>
        <ns3:ID>
          <xsl:value-of select="$subscriberShipmentReference"/>
        </ns3:ID>
        <ns3:Name>Cargo Manifest</ns3:Name>
        <ns3:TypeCode>785</ns3:TypeCode>
        <ns3:IssueDateTime>
          <xsl:value-of select="concat(DateMapper:CurrentDateTimeUTC('yyyy-MM-ddTHH:mm:ss'), '+00:00')"/>
        </ns3:IssueDateTime>
        <ns3:PurposeCode>
          <xsl:choose>
            <xsl:when test="$purpose='ORG'">Creation</xsl:when>
            <xsl:when test="$purpose='AMD'">Update</xsl:when>
            <xsl:when test="$purpose='WTH'">Deletion</xsl:when>
          </xsl:choose>
        </ns3:PurposeCode>
        <ns3:VersionID>2.00</ns3:VersionID>
        <ns3:ConversationID></ns3:ConversationID>
        <ns3:SenderParty>
          <ns3:PrimaryID schemeID="0">CARGOWISE</ns3:PrimaryID>
        </ns3:SenderParty>
        <ns3:RecipientParty>
          <ns3:PrimaryID schemeID="0">BRCUSTOMS</ns3:PrimaryID>
        </ns3:RecipientParty>
      </ns2:MessageHeaderDocument>
      <ns2:BusinessHeaderDocument>
        <ns3:ID>
          <xsl:value-of select="$waybillNumber"/>
          <!-- in consol, WaybillNumber is MAWB -->
        </ns3:ID>
      </ns2:BusinessHeaderDocument>
      <ns2:MasterConsignment>
        <ns3:IncludedTareGrossWeightMeasure>
          <xsl:attribute name="unitCode">
            <xsl:choose>
              <xsl:when test="s0:TotalWeightUnit/text()='LB'">LBR</xsl:when>
              <xsl:otherwise>KGM</xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
          <xsl:value-of select="StringMapper:FormatDecimal(s0:TotalWeight/text(), '0.000', false())" />
        </ns3:IncludedTareGrossWeightMeasure>
        <ns3:TotalPieceQuantity>
          <xsl:value-of select="s0:TotalNoOfPacks/text()"/>
        </ns3:TotalPieceQuantity>
        <ns3:TransportContractDocument>
          <ns3:ID>
            <xsl:value-of select="$waybillNumber"/>
          </ns3:ID>
        </ns3:TransportContractDocument>
        <ns3:OriginLocation>
          <ns3:ID>
            <xsl:value-of select="s0:PortOfOrigin/text()"/>
          </ns3:ID>
          <ns3:Name>
            <xsl:value-of select="s0:PortOfOrigin/@Name"/>
          </ns3:Name>
        </ns3:OriginLocation>
        <ns3:FinalDestinationLocation>
          <ns3:ID>
            <xsl:value-of select="s0:PortOfDestination/text()"/>
          </ns3:ID>
          <ns3:Name>
            <xsl:value-of select="s0:PortOfDestination/@Name"/>
          </ns3:Name>
        </ns3:FinalDestinationLocation>

        <xsl:if test="$receivingForwarderAddress != ''">
          <ns3:IncludedCustomsNote>
            <ns3:ContentCode>T</ns3:ContentCode>
            <ns3:Content>
              <xsl:call-template name="GetGovRegNumber">
                <xsl:with-param name="org" select="$receivingForwarderAddress"/>
              </xsl:call-template>
            </ns3:Content>
            <ns3:SubjectCode>AGT</ns3:SubjectCode>
            <ns3:CountryID>BR</ns3:CountryID>
          </ns3:IncludedCustomsNote>
        </xsl:if>

        <xsl:for-each select="s0:SubShipmentCollection/s0:SubShipment">
          <ns3:IncludedHouseConsignment>
            <ns3:SequenceNumeric>
              <xsl:value-of select="position()"/>
            </ns3:SequenceNumeric>
            <ns3:GrossWeightMeasure>
              <xsl:attribute name="unitCode">
                <xsl:choose>
                  <xsl:when test="s0:TotalWeightUnit/text()='LB'">LBR</xsl:when>
                  <xsl:otherwise>KGM</xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
              <xsl:value-of select="StringMapper:FormatDecimal(s0:TotalWeight/text(), '0.000', false())" />
            </ns3:GrossWeightMeasure>
            <ns3:TotalPieceQuantity>
              <xsl:value-of select="s0:OuterPacks/text()"/>
            </ns3:TotalPieceQuantity>
            <ns3:SummaryDescription>
              <xsl:value-of select="s0:GoodsDescription/text()"/>
            </ns3:SummaryDescription>
            <ns3:TransportContractDocument>
              <ns3:ID>
                <xsl:value-of select="s0:WayBillNumber/text()"/>
              </ns3:ID>
            </ns3:TransportContractDocument>
            <ns3:OriginLocation>
              <ns3:ID>
                <xsl:value-of select="s0:PortOfOrigin/text()"/>
              </ns3:ID>
              <ns3:Name>
                <xsl:value-of select="s0:PortOfOrigin/@Name"/>
              </ns3:Name>
            </ns3:OriginLocation>
            <ns3:FinalDestinationLocation>
              <ns3:ID>
                <xsl:value-of select="s0:PortOfDestination/text()"/>
              </ns3:ID>
              <ns3:Name>
                <xsl:value-of select="s0:PortOfDestination/@Name"/>
              </ns3:Name>
            </ns3:FinalDestinationLocation>
          </ns3:IncludedHouseConsignment>
        </xsl:for-each>
      </ns2:MasterConsignment>
    </ns2:HouseManifest>
  </xsl:template>

  <xsl:template name="GetGovRegNumber">
    <xsl:param name="org"/>

    <xsl:variable name="govRegType" select="$org/s0:GovRegNumType/text()" />
    <xsl:variable name="govRegTypePrefix" >
      <xsl:choose>
        <xsl:when test="$govRegType='CJN'">CNPJ</xsl:when>
        <xsl:when test="$govRegType='CPF'">CPF</xsl:when>
        <xsl:when test="$govRegType='PAS'">PAS</xsl:when>
        <xsl:otherwise></xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="govRegNum" select="userCSharp:RemoveSpecialCharacters($org/s0:GovRegNum/text())"/>
    <xsl:choose>
      <xsl:when test="$govRegNum!='' and $govRegTypePrefix!=''">
        <xsl:value-of select="concat($govRegTypePrefix, $govRegNum)"/>
      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[
public string ToUpper(string input)
{
  string result = "";
  if (input != null)
  {
    result = input;
  }
  return result.ToUpper();
}

public string RemoveSpecialCharacters(string input)
{
  var result = input.Replace(".", "").Replace("/", "").Replace("-", "");
  return result;
}

]]>
  </msxsl:script>
</xsl:stylesheet>

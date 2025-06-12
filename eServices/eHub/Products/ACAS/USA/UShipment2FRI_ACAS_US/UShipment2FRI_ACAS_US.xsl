<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 CodeMapper ContextAccessor DataModelAccessor StringMapper userCSharp" version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2012/11/AirCargoAdvanceScreening/1"
                xmlns:ns0="http://wisetechglobal.com/ehub/acas/us"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
                xmlns:DataModelAccessor="http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"
                xmlns:StringMapper="http://schemas.microsoft.com/BizTalk/2003/StringMapper"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />

  <xsl:template match="/">
    <xsl:apply-templates select="s0:UniversalShipment/s0:Shipment" />
  </xsl:template>

  <xsl:template match="s0:UniversalShipment/s0:Shipment">
    <xsl:variable name="RecipientID" select="ContextAccessor:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
    <xsl:variable name="SenderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
    <xsl:variable name="ACASID" select="DataModelAccessor:GetClientRegistrationCode($SenderID, s0:DataContext/s0:Workflow/s0:EventBranch/text(), 'ACAS_US')"/>

    <xsl:variable name="bookingPartyAddress" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType='BookingPartyDocumentaryAddress']"/>
    <xsl:variable name="bookingPartyACASNumber" select="$bookingPartyAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='ACA' and s0:CountryOfIssue/text()='US']/s0:Value/text()"/>
    <xsl:variable name="ACASSenderID">
      <xsl:choose>
        <xsl:when test="$bookingPartyACASNumber!=''">
          <xsl:value-of select="$bookingPartyACASNumber"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$ACASID"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="carrierID">
      <xsl:choose>
        <xsl:when test="contains($RecipientID, 'TST')">ACAS_US_TST</xsl:when>
        <xsl:otherwise>ACAS_US</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="ACASRecipientID">
      <xsl:choose>
        <xsl:when test="contains($RecipientID, 'TST')">WASACCR</xsl:when>
        <xsl:otherwise>WASAPCR</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="SubscribeACASID" select="DataModelAccessor:InsertSubscriptionValue('ACASID', $carrierID, $SenderID, $ACASSenderID)"/>
    <xsl:variable name="InterchangeNum" select="CodeMapper:CallActionProcedureHelper('GetCounterValue','@value','@name','CargoWise.eHub.Products.ACAS.US.Transforms.ACAS_US','@maxlength','14')" />

    <xsl:variable name="dataVersion" select="s0:DataContext/s0:DocumentaryOverride/s0:DataVersion/text()" />
    <xsl:variable name="shipmentID" select="normalize-space(s0:DataContext/s0:DataSource/s0:Key/text())" />
    <xsl:variable name="shipmentType" select="normalize-space(s0:DataContext/s0:DataSource/s0:Type/text())" />
    <xsl:variable name="waybillNumber" select="userCSharp:ToUpper(normalize-space(s0:WayBillNumber/text()))"/>

    <xsl:variable name="mawbValue" select="userCSharp:ToUpper(normalize-space(s0:AddInfoCollection/s0:AddInfo[s0:Key='MAWB']/s0:Value/text()))" />

    <xsl:variable name="mawbNumber">
      <xsl:choose>
        <xsl:when test="contains($mawbValue, '-') and string-length($mawbValue)=12">
          <xsl:value-of select="$mawbValue"/>
        </xsl:when>
        <xsl:when test="not(contains($mawbValue, '-')) and string-length($mawbValue)=11">
          <xsl:value-of select="concat(substring($mawbValue, 1, 3), '-', substring($mawbValue, 4, 8))"/>
        </xsl:when>
        <xsl:otherwise></xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="previousShipmentReference" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $carrierID, '@recipientId', $SenderID , '@ST_ID', 'ACASUS', '@value', $shipmentID)" />
    <xsl:variable name="subscriberShipmentReference">
      <xsl:choose>
        <xsl:when test="$previousShipmentReference!=''">
          <xsl:value-of select="$previousShipmentReference" />
        </xsl:when>

        <xsl:otherwise>
          <xsl:variable name="FormattedCounter" select='format-number($InterchangeNum, "0000000000")' />
          <xsl:variable name="newShipmentReference" select="concat('ACAS', $FormattedCounter)" />

          <xsl:variable name="subscribeShipmentID" select="DataModelAccessor:InsertSubscriptionValue('ACASUS', $carrierID, $SenderID, $shipmentID, $newShipmentReference)" />

          <xsl:choose>
            <xsl:when test="$waybillNumber!=''">
              <xsl:variable name="subscribeWaybillNumber" select="DataModelAccessor:InsertSubscriptionValue('ACASUS', $carrierID, $SenderID, concat($ACASSenderID, substring($waybillNumber, 1, 12)), $newShipmentReference, 'FRI-HWB')" />
            </xsl:when>
            <xsl:otherwise>
              <xsl:variable name="subscribeWaybillNumber" select="DataModelAccessor:InsertSubscriptionValue('ACASUS', $carrierID, $SenderID, concat($ACASSenderID, $mawbNumber), $newShipmentReference, 'FRI-MAWB')" />
            </xsl:otherwise>
          </xsl:choose>
          <xsl:value-of select="$newShipmentReference" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="SubscribeShipmentId" select="DataModelAccessor:InsertSubscriptionValue('ACASUS', $carrierID, $SenderID, $subscriberShipmentReference, $shipmentID, 'ShipmentId')" />
    <xsl:variable name="SubscribeDocumentName" select="DataModelAccessor:InsertSubscriptionValue('ACASUS', $carrierID, $SenderID, $subscriberShipmentReference, s0:DataContext/s0:DocumentaryOverride/s0:DocumentName, 'DocumentName')" />
    <xsl:variable name="SubscribeForwardingType" select="DataModelAccessor:InsertSubscriptionValue('ACASUS', $carrierID, $SenderID, $subscriberShipmentReference, $shipmentType, 'ForwardingType')" />
    <xsl:variable name="SubscribeSubMessageType" select="DataModelAccessor:InsertSubscriptionValue('ACASUS', $carrierID, $SenderID, $subscriberShipmentReference, 'Shipment', 'SubMessageType')" />

    <xsl:variable name="InboxPK" select="ContextAccessor:GetContextProperty('InternalTrackingID', 'http://cargowise.com/ehub/tracking/2010/06')"/>
    <xsl:variable name="SubscribeInboxPK" select="DataModelAccessor:InsertSubscriptionValue('ACASUS', $carrierID, $SenderID, $InboxPK, $InterchangeNum)" />

    <ns0:FRI>
      <MessageHeader>
        <Recipient>
          <Code>
            <xsl:value-of select="$ACASRecipientID"/>
          </Code>
        </Recipient>
        <Sender>
          <Code>
            <xsl:value-of select="$ACASSenderID"/>
          </Code>
        </Sender>
      </MessageHeader>
      <MessageBody>
        <Content>
          <StandardMessageIdentifier>
            <MessageIdentifier>FRI</MessageIdentifier>
          </StandardMessageIdentifier>
          <CargoControlLocation>
            <AirportOfArrival>
              <xsl:value-of select="substring(s0:PortOfFirstArrival/text(), 3, 3)"/>
            </AirportOfArrival>

            <xsl:variable name="arrivalCTOAddress" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType='ArrivalCTOAddress']"/>
            <CargoTerminalOperator>
              <xsl:value-of select="userCSharp:ToUpper($arrivalCTOAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='FRM' and s0:CountryOfIssue/text()='US']/s0:Value/text())"/>
            </CargoTerminalOperator>
          </CargoControlLocation>
          <AirWayBill>
            <AWB>
              <Prefix>
                <xsl:value-of select="StringMapper:PadRight(substring-before($mawbNumber, '-'), 3, ' ')" />
              </Prefix>
              <SerialNumber>
                <xsl:value-of select="StringMapper:PadRight(substring-after($mawbNumber, '-'), 8, ' ')" />
              </SerialNumber>
            </AWB>
            <Consol>
              <xsl:if test="$waybillNumber=''">
                <ConsolidationIdentifier>M</ConsolidationIdentifier>
              </xsl:if>
              <HAWBNumber>
                <xsl:value-of select="substring($waybillNumber, 1, 12)"/>
              </HAWBNumber>
              <PackageInfo>
                <PackageTrackingIdentifier>
                  <xsl:value-of select="$subscriberShipmentReference"/>
                </PackageTrackingIdentifier>
              </PackageInfo>
            </Consol>
          </AirWayBill>
          <Waybill>
            <WaybillIdentifier>WBL</WaybillIdentifier>
            <Airports>
              <PortOfOrigin>
                <xsl:value-of select="userCSharp:ToUpper(substring(s0:PortOfOrigin/text(), 3, 3))"/>
              </PortOfOrigin>
              <PortOfDestination>
                <xsl:value-of select="userCSharp:ToUpper(substring(s0:PortOfDestination/text(), 3, 3))"/>
              </PortOfDestination>
            </Airports>
            <ShipmentPackageInfo>
              <PackageIdentifier>T</PackageIdentifier>
              <TotalPackages>
                <xsl:value-of select="s0:TotalNoOfPacks/text()"/>
              </TotalPackages>
            </ShipmentPackageInfo>
            <ShipmentWeightInfo>
              <WeightUnit>
                <xsl:choose>
                  <xsl:when test="s0:TotalWeightUnit/text()='LB'">L</xsl:when>
                  <xsl:otherwise>K</xsl:otherwise>
                </xsl:choose>
              </WeightUnit>
              <TotalWeights>
                <xsl:value-of select="s0:TotalWeight/text()"/>
              </TotalWeights>
            </ShipmentWeightInfo>
            <xsl:variable name="goodsDescription" select="userCSharp:ToUpper(normalize-space(s0:GoodsDescription/text()))" />
            <CargoDescriptions>
              <CargoDescription01>
                <xsl:value-of select="substring($goodsDescription, 1, 35)"/>
              </CargoDescription01>
              <AdditionalCargoDescription>
                <xsl:call-template name="CargoDescription">
                  <xsl:with-param name="text" select ="normalize-space(substring($goodsDescription, 36, 13*35))" />
                </xsl:call-template>
              </AdditionalCargoDescription>
            </CargoDescriptions>
          </Waybill>
          <Arrival>
            <RouteIdentifier>ARR</RouteIdentifier>
            <FlightNumber>
              <xsl:value-of select="userCSharp:ToUpper(substring(s0:VoyageFlightNo/text(), 1, 7))"/>
            </FlightNumber>
            <xsl:variable name="arrivalDateTime" select="userCSharp:FormatDate(s0:DateCollection/s0:Date[s0:Type/text()='Arrival']/s0:Value/text(), 'yyyy-MM-ddTHH:mm:ss', 'ddMMM')"/>
            <ArrivalDate>
              <xsl:value-of select="userCSharp:ToUpper($arrivalDateTime)"/>
            </ArrivalDate>
          </Arrival>
          <Agent>
            <AgentIdentifier>AGT</AgentIdentifier>
            <xsl:variable name="notifyPartyAddress" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType='NotifyParty']"/>
            <ACASParticipantCode>
              <xsl:value-of select="userCSharp:ToUpper($notifyPartyAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='FRM' and s0:CountryOfIssue/text()='US']/s0:Value/text())"/>
            </ACASParticipantCode>
          </Agent>
          <xsl:call-template name="Organisation">
            <xsl:with-param name="elementName" select="'Shipper'" />
            <xsl:with-param name="identifierCode" select="'SHP'" />
            <xsl:with-param name="org" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType='ConsignorDocumentaryAddress']" />
          </xsl:call-template>
          <xsl:call-template name="Organisation">
            <xsl:with-param name="elementName" select="'Consignee'" />
            <xsl:with-param name="identifierCode" select="'CNE'" />
            <xsl:with-param name="org" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType='ConsigneeDocumentaryAddress']" />
          </xsl:call-template>
          <xsl:call-template name="Organisation">
            <xsl:with-param name="elementName" select="'NotifyParty'" />
            <xsl:with-param name="identifierCode" select="'OPI'" />
            <xsl:with-param name="org" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType='NotifyParty']" />
            <xsl:with-param name="orgType" select="s0:AddInfoCollection/s0:AddInfo[s0:Key='NotifyPartyType_Code']/s0:Value/text()" />
          </xsl:call-template>
        </Content>
      </MessageBody>
    </ns0:FRI>
  </xsl:template>

  <xsl:template name="Organisation">
    <xsl:param name="identifierCode"/>
    <xsl:param name="elementName"/>
    <xsl:param name="org"/>
    <xsl:param name="orgType"/>

    <xsl:if test="$org">

      <xsl:variable name="companyname" select="userCSharp:ToUpper(normalize-space($org/s0:CompanyName/text()))"/>
      <xsl:variable name="street1" select="userCSharp:ToUpper(normalize-space($org/s0:Address1/text()))"/>
      <xsl:variable name="street2" select="userCSharp:ToUpper(normalize-space($org/s0:Address2/text()))"/>
      <xsl:variable name="city" select="userCSharp:ToUpper(normalize-space($org/s0:City/text()))"/>
      <xsl:variable name="state" select="userCSharp:ToUpper(normalize-space($org/s0:State/text()))"/>
      <xsl:variable name="postcode" select="userCSharp:ToUpper(normalize-space($org/s0:Postcode/text()))"/>
      <xsl:variable name="countryCode" select="userCSharp:ToUpper(normalize-space($org/s0:Country/text()))"/>
      <xsl:variable name="contact" select="userCSharp:ToUpper(normalize-space($org/s0:Contact/text()))"/>
      <xsl:variable name="phone" select="userCSharp:StringReplace(normalize-space($org/s0:Phone/text()), '+', '')" />
      <xsl:variable name="fax" select="userCSharp:StringReplace(normalize-space($org/s0:Fax/text()), '+', '')" />
      <xsl:variable name="email" select="userCSharp:ToUpper(normalize-space($org/s0:Email/text()))"/>

      <xsl:variable name="combinedStreet">
        <xsl:choose>
          <xsl:when test="$street2!=''">
            <xsl:value-of select="concat($street1, ' ', $street2)"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$street1"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>


      <xsl:element name="{$elementName}">
        <Organisation>
          <OrganisationHeader>
            <OrganisationIdentifier>
              <xsl:value-of select="$identifierCode"/>
            </OrganisationIdentifier>
            <xsl:if test="$identifierCode='OPI'">
              <OrganisationType>
                <xsl:value-of select="$orgType"/>
              </OrganisationType>
            </xsl:if>
            <OrganisationName>
              <xsl:value-of select="normalize-space(substring($companyname, 1, 35))"/>
            </OrganisationName>
          </OrganisationHeader>
          <OrganisationDetails>
            <Address>
              <xsl:value-of select="normalize-space(substring($combinedStreet, 1, 35))"/>
            </Address>
            <CityState>
              <City>
                <xsl:value-of select="normalize-space(substring($city, 1, 17))"/>
              </City>
              <State>
                <xsl:value-of select="normalize-space(substring($state, 1, 3))"/>
              </State>
            </CityState>
            <CountryPostalCodePhone>
              <Country>
                <xsl:value-of select="$countryCode"/>
              </Country>
              <PostalCode>
                <xsl:value-of select="$postcode"/>
              </PostalCode>
              <Phone>
                <xsl:value-of select="$phone"/>
              </Phone>
            </CountryPostalCodePhone>
            <xsl:if test="$email!=''">
              <Contacts>
                <ContactIdentifier>EML</ContactIdentifier>
                <ContactText>
                  <xsl:value-of select="$email"/>
                </ContactText>
              </Contacts>
            </xsl:if>
            <xsl:if test="$fax!=''">
              <Contacts>
                <ContactIdentifier>FAX</ContactIdentifier>
                <ContactText>
                  <xsl:value-of select="$fax"/>
                </ContactText>
              </Contacts>
            </xsl:if>
          </OrganisationDetails>
        </Organisation>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="CargoDescription">
    <xsl:param name="text"/>

    <xsl:call-template name="SplitText">
      <xsl:with-param name="text" select="userCSharp:ReplaceCRLFText($text)"/>
      <xsl:with-param name="elementName" select="'CargoDescription'"/>
      <xsl:with-param name="segmentCount" select="1"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="SplitText">
    <xsl:param name="elementName"/>
    <xsl:param name="text"/>
    <xsl:param name="segmentCount"/>
    <xsl:if test="$text!='' and number($segmentCount)&lt;=13">

      <xsl:variable name="lineLength" select="userCSharp:GetLengthForWrapping($text, 35)"/>
      <xsl:variable name="fixedLengthText" select="substring($text, 1, $lineLength)"/>
      <xsl:variable name="normText" select="normalize-space($fixedLengthText)"/>
      <xsl:variable name="remainingText" select="normalize-space(substring($text, $lineLength+1))"/>

      <xsl:choose>
        <xsl:when test="$normText!=''">
          <xsl:element name="{concat($elementName,format-number($segmentCount+1, '00'))}">
            <xsl:value-of select="$normText"/>
          </xsl:element>
          <xsl:call-template name="SplitText">
            <xsl:with-param name="elementName" select="$elementName" />
            <xsl:with-param name="text" select="$remainingText"/>
            <xsl:with-param name="segmentCount" select="$segmentCount+1"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="SplitText">
            <xsl:with-param name="elementName" select="$elementName" />
            <xsl:with-param name="text" select="$remainingText"/>
            <xsl:with-param name="segmentCount" select="$segmentCount"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>
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

public int GetLengthForWrapping(string text, string maxLengthStr)
{
  int maxLength = int.Parse(maxLengthStr);

  if (text.Substring(0,1) == "\n")
  {
    return 1;
  }

  for (int i = 1; i < text.Length && i < maxLength + 1; i++)
  {
    if (text[i] == '\n')
    {
      return i > maxLength ? i : i + 1;
    }
  }

  if (text.Length <= maxLength)
  {
    return text.Length;
  }

  return maxLength;
}

public string ReplaceCRLFText(string text)
{
  return StringReplace(text, "\r\n", "\n");
}

public string StringReplace(string text, string oldValue, string newValue)
{
  if (text != null && text.Length > 0)
  {
    return text.Replace(oldValue, newValue);
  }
  return "";
}

public string FormatDate(string val, string inFmts, string outFmt)
{
  DateTime parsedDate;
  if (DateTime.TryParseExact(val, inFmts.Split(new char[] {';'}), System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out parsedDate))
  {
      return parsedDate.ToString(outFmt);
  }
  else
  {
      return string.Empty;
  }
}

]]>
  </msxsl:script>
</xsl:stylesheet>

<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 ScriptNS0 ScriptNS1 ScriptNS3 ScriptNS4 userCSharp" version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
                xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1"
                xmlns:ScriptNS3="http://schemas.microsoft.com/BizTalk/2003/ScriptNS3"
                xmlns:ScriptNS4="http://schemas.microsoft.com/BizTalk/2003/ScriptNS4"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />

  <xsl:template match="/s0:UniversalShipment/s0:Shipment">
    <xsl:variable name="minSeaLegOrder">
      <xsl:for-each select="s0:TransportLegCollection/s0:TransportLeg[s0:TransportMode='Sea']/s0:LegOrder">
        <xsl:sort select="." data-type="number" order="ascending"/>
        <xsl:if test="position() = 1">
          <xsl:value-of select="."/>
        </xsl:if>
      </xsl:for-each>
    </xsl:variable>
    <xsl:variable name="InterchangeNum" select="ScriptNS0:CallActionProcedureHelper('GetCounterValue','@value','@name','CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.WWA.SI','@maxlength','14')" />
    <xsl:variable name="SenderID" select="ScriptNS3:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
    <xsl:variable name="WWAID" select="ScriptNS4:GetClientRegistrationCode($SenderID, s0:DataContext/s0:EventBranch/s0:Code/text(), 'WWA')"/>
    <xsl:variable name="firstSeaLeg" select="s0:TransportLegCollection/s0:TransportLeg[s0:LegOrder/text()=$minSeaLegOrder]" />
    <xsl:variable name="currentDateTime" select="ScriptNS1:CurrentDateTime('yyyyMMdd_mmssms')" />
    <xsl:variable name="FileName" select="ScriptNS3:SetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06', concat('WWA_Booking','_Cargowise','_',$currentDateTime,'_',$InterchangeNum))"/>
    <xsl:variable name="maxSeaLegOrder">
      <xsl:for-each select="s0:TransportLegCollection/s0:TransportLeg[s0:TransportMode='Sea']/s0:LegOrder">
        <xsl:sort select="." data-type="number" order="descending"/>
        <xsl:if test="position() = 1">
          <xsl:value-of select="."/>
        </xsl:if>
      </xsl:for-each>
    </xsl:variable>
    <xsl:variable name="lastSeaLeg" select="s0:TransportLegCollection/s0:TransportLeg[s0:LegOrder/text()=$maxSeaLegOrder]" />
    <xsl:variable name="consolID" select="s0:DataContext/s0:DataSourceCollection/s0:DataSource[1]/s0:Key/text()"/>
    <xsl:variable name="SubscribeInterchangeNum" select="ScriptNS4:InsertSubscriptionValue('WWAMSG', 'WWA', $SenderID, $InterchangeNum, $consolID)" />
    <xsl:variable name="previousConsolReference" select="ScriptNS0:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', 'WWA', '@recipientId', $SenderID , '@ST_ID', 'WWAMSG', '@value', $consolID)" />
    <xsl:variable name="SubscriberConsolReference">
      <xsl:choose>
        <xsl:when test="$previousConsolReference!=''">
          <xsl:value-of select="$previousConsolReference" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:variable name="formattedCounter" select='format-number($InterchangeNum, "0000000000")' />
          <xsl:variable name="NewConsolReference" select="concat('WWA', $formattedCounter)" />
          <xsl:variable name="SubScriberValue1" select="ScriptNS4:InsertSubscriptionValue('WWAMSG', 'WWA', $SenderID, $NewConsolReference, $consolID)" />
          <xsl:variable name="SubScriberValue2" select="ScriptNS4:InsertSubscriptionValue('WWAMSG', 'WWA', $SenderID, $consolID, $NewConsolReference)" />
          <xsl:value-of select="$NewConsolReference" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    
    <BookingRequest xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="http://www.wwalliance.com/wiki/images/b/bd/WWA_Booking_Request_version_1.1.0.xsd">
      <BookingEnvelope>
        <SenderID>
          <xsl:variable name="senderId" select="ScriptNS0:GetRecipientCodeUnkeyed('WWA_SI1' , 'WWA_SI1' , 'Shipping Instruction OBL to WWA' , 'Sender ID' , 'Sender')"/>
          <xsl:value-of select="$senderId"/>
        </SenderID>
        <ReceiverID>
          <xsl:variable name="receiverID" select="ScriptNS0:GetRecipientCodeUnkeyed('WWA_SI1' , 'WWA_SI1' , 'Shipping Instruction OBL to WWA' , 'Receiver ID' , 'Receiver')"/>
          <xsl:value-of select="$receiverID"/>
        </ReceiverID>
        <Password>
          <xsl:variable name="password" select="ScriptNS0:GetRecipientCodeUnkeyed('WWA_SI1' , 'WWA_SI1' , 'Shipping Instruction OBL to WWA' , 'Password' , 'WWA Password')"/>
          <xsl:value-of select="$password"/>
        </Password>
        <Type>BookingRequest</Type>
        <Version>1.1.0</Version>
        <EnvelopeID>
          <xsl:value-of select="$InterchangeNum"/>
        </EnvelopeID>
      </BookingEnvelope>
      <BookingDetails>
        <ApplicationType>WE</ApplicationType>
        <BookingType>
          <xsl:choose>
            <xsl:when test="s0:ContainerMode/s0:Code='FCL'">F</xsl:when>
            <xsl:otherwise>L</xsl:otherwise>
          </xsl:choose>
        </BookingType>
        <BookingDate>
          <xsl:value-of select="ScriptNS1:ConvertToDate(s0:DataContext/s0:TriggerDate, 'yyyy-MM-dd')"/>
        </BookingDate>
        <LastSentDate>0000-00-00</LastSentDate>
        <RequestType>
          <xsl:choose>
            <xsl:when test="s0:DataContext/s0:ActionPurpose/s0:Code = 'WTH'">C</xsl:when>
            <xsl:when test="s0:DataContext/s0:DocumentaryOverride/s0:DataVersion = '1'">N</xsl:when>
            <xsl:otherwise>U</xsl:otherwise>
          </xsl:choose>
        </RequestType>
        <xsl:if test="s0:CoLoadBookingConfirmationReference != ''">
          <BookingNumber>
            <xsl:value-of select="s0:CoLoadBookingConfirmationReference"/>
          </BookingNumber>
        </xsl:if>
        <WWAShipmentReference></WWAShipmentReference>
        <CustomerControlCode>
          <xsl:value-of select="$WWAID"/>
        </CustomerControlCode>
        <BookingOffice>
          <xsl:value-of select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType='CoLoadWith']/s0:Port/s0:Code"/>
        </BookingOffice>
        <xsl:variable name="FRT" select="s0:PaymentHandlingInstructionCollection/s0:PaymentHandlingInstruction[s0:Category/s0:Code = 'FRT']"/>
        <FPI>
          <xsl:if test="$FRT!=''">
            <xsl:choose>
              <xsl:when test="$FRT/s0:PaymentMethod/s0:Code = 'PPD'">P</xsl:when>
              <xsl:otherwise>C</xsl:otherwise>
            </xsl:choose>
          </xsl:if>
        </FPI>
        <CommunicationReference>
          <xsl:value-of select="$SubscriberConsolReference"/>
        </CommunicationReference>
        <CustomerReference>
          <xsl:value-of select="s0:DataContext/s0:DataSourceCollection/s0:DataSource/s0:Key"/>
        </CustomerReference>
        <ShipperReference>
          <xsl:value-of select="s0:AdditionalReferenceCollection/s0:AdditionalReference[s0:Type/s0:Code='OAG']/s0:ReferenceNumber"/>
        </ShipperReference>
        <ForwarderReference>
          <xsl:value-of select="s0:DataContext/s0:DataSourceCollection/s0:DataSource/s0:Key"/>
        </ForwarderReference>
        <ConsigneeReference></ConsigneeReference>
        <xsl:call-template name="Address">
          <xsl:with-param name="orgAddr" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType='BookingPartyDocumentaryAddress']"/>
          <xsl:with-param name="id" select="'FW'"/>
        </xsl:call-template>
        <xsl:call-template name="Address">
          <xsl:with-param name="orgAddr" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType='ConsignorDocumentaryAddress']"/>
          <xsl:with-param name="id" select="'SH'"/>
        </xsl:call-template>
        <xsl:call-template name="Address">
          <xsl:with-param name="orgAddr" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType='ConsigneeDocumentaryAddress']"/>
          <xsl:with-param name="id" select="'CN'"/>
        </xsl:call-template>
        <xsl:call-template name="Address">
          <xsl:with-param name="orgAddr" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType='NotifyParty']"/>
          <xsl:with-param name="id" select="'N1'"/>
        </xsl:call-template>
        <xsl:call-template name="Address">
          <xsl:with-param name="orgAddr" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType='NotifyParty2']"/>
          <xsl:with-param name="id" select="'N2'"/>
        </xsl:call-template>
        <xsl:variable name="currentUser" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='CurrentUser']" />
        <xsl:variable name="Consignor" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='ConsignorPickupDeliveryAddress']" />
        <xsl:variable name="Consignee" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='ConsigneePickupDeliveryAddress']" />
        <CustomerContact>
          <xsl:value-of select="$currentUser/s0:Contact"/>
        </CustomerContact>
        <CustomerPhone>
          <xsl:value-of select="$currentUser/s0:Phone"/>
        </CustomerPhone>
        <CustomerEmail>
          <xsl:value-of select="$currentUser/s0:Email"/>
        </CustomerEmail>
        <BUCustomerEmail>
          <xsl:value-of select="$currentUser/s0:Email"/>
        </BUCustomerEmail>
        <OnHold>N</OnHold>
        <HVC>N</HVC>
        <BondedCargo>N</BondedCargo>
        <CFSOrigin>
          <xsl:value-of select="$Consignor/s0:Port/s0:Code"/>
        </CFSOrigin>
        <PortOfLoading>
          <xsl:value-of select="$firstSeaLeg/s0:PortOfLoading/s0:Code"/>
        </PortOfLoading>
        <CFSDestination>
          <xsl:value-of select="$Consignee/s0:Port/s0:Code"/>
        </CFSDestination>
        <PortOfDischarge>
          <xsl:value-of select="$lastSeaLeg/s0:PortOfDischarge/s0:Code"/>
        </PortOfDischarge>
        <FinalDestination>
          <xsl:value-of select="s0:PortOfDestination/s0:Code"/>
        </FinalDestination>
        <FinalDestinationPlace>
          <xsl:value-of select="s0:PortOfDestination/s0:Name"/>
        </FinalDestinationPlace>
        <xsl:choose>
          <xsl:when test="s0:ContainerCollection/s0:Container[starts-with(s0:DeliveryMode[1]/text(),'CFS')]">
            <FinalDestinationType>CFS</FinalDestinationType>
          </xsl:when>
          <xsl:otherwise>
            <FinalDestinationType></FinalDestinationType>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:variable name="destCountry" select="s0:PortOfDestination/s0:Code" />
        <FinalDestinationCountry>
          <xsl:value-of select="substring($destCountry,1,2)"/>
        </FinalDestinationCountry>
        <OncarriageFlag>
          <xsl:choose>
            <xsl:when test="$lastSeaLeg/s0:LegType!='Main'">Y</xsl:when>
            <xsl:otherwise>N</xsl:otherwise>
          </xsl:choose>
        </OncarriageFlag>
        <OncarriagePlace>
          <xsl:choose>
            <xsl:when test="$lastSeaLeg/s0:LegType!='Main'">
              <xsl:value-of select="$lastSeaLeg/s0:PortOfDischarge/s0:Name"/>
            </xsl:when>
          </xsl:choose>
        </OncarriagePlace>
        <AmsFlag>
          <xsl:choose>
            <xsl:when test="$lastSeaLeg/s0:PortOfDischarge[starts-with(s0:Code, 'US')]">Y</xsl:when>
            <xsl:otherwise>N</xsl:otherwise>
          </xsl:choose>
        </AmsFlag>
        <AesFlag>
          <xsl:choose>
            <xsl:when test="s0:PortOfOrigin[starts-with(s0:Code, 'US')] and s0:GoodsValue>2500">Y</xsl:when>
            <xsl:otherwise>N</xsl:otherwise>
          </xsl:choose>
        </AesFlag>
        <ColoadCommodity />

        <xsl:variable name="Gnote" select="s0:NoteCollection/s0:Note[s0:Description ='Goods Handling Instructions']"/>
        <xsl:variable name="Fnote" select="s0:NoteCollection/s0:Note[s0:Description ='Forwarding Instruction Notes']"/>
        <xsl:variable name="Snote" select="s0:NoteCollection/s0:Note[s0:Description ='Special Instructions']"/>
        <xsl:variable name="Gtext" select="$Gnote/s0:NoteText/text()"/>
        <xsl:variable name="Ftext" select="$Fnote/s0:NoteText/text()"/>
        <xsl:variable name="Stext" select="$Snote/s0:NoteText/text()"/>
        
        <xsl:if test="$Gtext!=''">
          <xsl:call-template name="Remarks">
            <xsl:with-param name="text" select="$Gtext"/>
          </xsl:call-template>
        </xsl:if>
        <xsl:if test="$Ftext!=''">
          <xsl:call-template name="Remarks">
            <xsl:with-param name="text" select="$Ftext"/>
          </xsl:call-template>
        </xsl:if>
        <xsl:if test="$Stext!=''">
          <xsl:call-template name="Remarks">
            <xsl:with-param name="text" select="$Stext"/>
          </xsl:call-template>
        </xsl:if>
      
        <xsl:choose>
          <xsl:when test="s0:ContainerCollection/s0:Container[starts-with(s0:DeliveryMode[1]/text(),'CFS')]">
            <PickupFlag>Y</PickupFlag>
          </xsl:when>
          <xsl:otherwise>
            <PickupFlag>N</PickupFlag>
          </xsl:otherwise>
        </xsl:choose>

        <xsl:if test="s0:ContainerCollection/s0:Container[starts-with(s0:DeliveryMode[1]/text(),'CFS')]">
          <xsl:variable name="pickupAddress" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType='ConsignorPickupDeliveryAddress']"/>
          <PickupDetails>
            <PickupReference/>
            <CompanyName>
              <xsl:value-of select="$pickupAddress/s0:CompanyName"/>
            </CompanyName>
            <Address>
              <xsl:value-of select="normalize-space(concat($pickupAddress/s0:Address1, ' ', $pickupAddress/s0:Address2))"/>
            </Address>
            <City>
              <xsl:value-of select="$pickupAddress/s0:City"/>
            </City>
            <PostalCode>
              <xsl:value-of select="$pickupAddress/s0:Postcode"/>
            </PostalCode>
            <StateProvince>
              <xsl:value-of select="$pickupAddress/s0:State"/>
            </StateProvince>
            <Country>
              <xsl:value-of select="$pickupAddress/s0:Country/s0:Code"/>
            </Country>
            <Contact>
              <xsl:value-of select="$pickupAddress/s0:Contact"/>
            </Contact>
            <Phone>
              <xsl:value-of select="$pickupAddress/s0:Phone"/>
            </Phone>
            <Email>
              <xsl:value-of select="$pickupAddress/s0:Email"/>
            </Email>
            <Date>
              <xsl:value-of select="ScriptNS1:FormatXmlDateTime($firstSeaLeg/s0:LCLReceivalCommences/text(), 'yyyy-MM-dd')"/>
            </Date>
            <Time>
              <xsl:value-of select="ScriptNS1:FormatXmlDateTime($firstSeaLeg/s0:LCLReceivalCommences/text(), 'HHmm')"/>
            </Time>
            <xsl:call-template name="Remarks">
              <xsl:with-param name="text" select="s0:NoteCollection/s0:Note[s0:Description='Export Pickup Instructions']/s0:NoteText"/>
            </xsl:call-template>
          </PickupDetails>
        </xsl:if>

        <SailingDetails>
          <VesselName>
            <xsl:value-of select="s0:TransportLegCollection/s0:TransportLeg[s0:LegType='Main']/s0:VesselName"/>
          </VesselName>
          <IMONumber>
            <xsl:value-of select="s0:TransportLegCollection/s0:TransportLeg[s0:LegType='Main']/s0:VesselLloydsIMO"/>
          </IMONumber>
          <Voyage>
            <xsl:value-of select="s0:TransportLegCollection/s0:TransportLeg[s0:LegType='Main']/s0:VoyageFlightNo"/>
          </Voyage>
          <ETDCFS>
            <xsl:value-of select="ScriptNS1:ConvertToDate($firstSeaLeg/s0:LCLCutOff, 'yyyy-MM-dd')"/>
          </ETDCFS>
          <ETACFS>
            <xsl:value-of select="ScriptNS1:ConvertToDate($lastSeaLeg/s0:EstimatedArrival, 'yyyy-MM-dd')"/>
          </ETACFS>
          <ETSOrigin>
            <xsl:value-of select="ScriptNS1:ConvertToDate($firstSeaLeg/s0:EstimatedDeparture, 'yyyy-MM-dd')"/>
          </ETSOrigin>
          <ETSPoL>
            <xsl:value-of select="ScriptNS1:ConvertToDate(s0:TransportLegCollection/s0:TransportLeg[s0:LegType='Main']/s0:EstimatedDeparture, 'yyyy-MM-dd')"/>
          </ETSPoL>
        </SailingDetails>

        <xsl:for-each select="s0:PackingLineCollection/s0:PackingLine">
          <CargoDetails>
            <Pieces>
              <xsl:value-of select="s0:PackQty"/>
            </Pieces>
            <xsl:call-template name="ShippingMarks">
              <xsl:with-param name="text" select="s0:MarksAndNos"/>
            </xsl:call-template>
            <Packaging>
              <xsl:value-of select="s0:PackType/s0:Description"/>
            </Packaging>
            <xsl:call-template name="Commodity">
              <xsl:with-param name="text">
                <xsl:choose>
                  <xsl:when test="s0:DetailedDescription = ''">
                    <xsl:value-of select="s0:GoodsDescription"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="s0:DetailedDescription"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:with-param>
            </xsl:call-template>
            <HSCode>
              <xsl:value-of select="s0:HarmonisedCode"/>
            </HSCode>
            <Weight>
              <xsl:value-of select="s0:Weight"/>
            </Weight>
            <Volume>
              <xsl:value-of select="s0:Volume"/>
            </Volume>
            <UOM>
              <xsl:choose>
                <xsl:when test="s0:WeightUnit/s0:Code = 'LB'">E</xsl:when>
                <xsl:otherwise>M</xsl:otherwise>
              </xsl:choose>
            </UOM>
            <xsl:choose>
              <xsl:when test="s0:UNDGCollection!=''">
                <HazardousFlag>Y</HazardousFlag>
                <HazardousDetails>
                  <HazardousClass>
                    <xsl:value-of select="s0:UNDGCollection/s0:UNDG/s0:IMOClass"/>
                  </HazardousClass>
                  <Flashpoint>
                    <xsl:value-of select="s0:UNDGCollection/s0:UNDG/s0:FlashPoint"/>
                  </Flashpoint>
                  <FlashpointFlag>
                    <xsl:if test="s0:UNDGCollection/s0:UNDG/s0:FlashPoint!=''">C</xsl:if>
                  </FlashpointFlag>
                  <ShippingName>
                    <xsl:value-of select="s0:UNDGCollection/s0:UNDG/s0:ProperShippingName"/>
                  </ShippingName>
                  <UNNumber>
                    <xsl:value-of select="s0:UNDGCollection/s0:UNDG/s0:UNDGCode"/>
                  </UNNumber>
                  <PackingGroup>
                    <xsl:value-of select="s0:UNDGCollection/s0:UNDG/s0:PackingGroup"/>
                  </PackingGroup>
                </HazardousDetails>
              </xsl:when>
              <xsl:otherwise>
                <HazardousFlag>N</HazardousFlag>
              </xsl:otherwise>
            </xsl:choose>

            <OverDimensionFlag>N</OverDimensionFlag>
            <OverHeightFlag>
              <xsl:choose>
                <xsl:when test="(s0:LengthUnit/s0:Code='CM' and s0:Height > 230) or (s0:LengthUnit/s0:Code='IN' and s0:Height > 90)">Y</xsl:when>
                <xsl:otherwise>N</xsl:otherwise>
              </xsl:choose>
            </OverHeightFlag>
            <OverLengthFlag>
              <xsl:choose>
                <xsl:when test="(s0:LengthUnit/s0:Code='CM' and s0:Length > 590) or (s0:LengthUnit/s0:Code='IN' and s0:Length > 232)">Y</xsl:when>
                <xsl:otherwise>N</xsl:otherwise>
              </xsl:choose>
            </OverLengthFlag>
            <OverWeightFlag>
              <xsl:choose>
                <xsl:when test="(s0:LengthUnit/s0:Code='KG' and s0:Weight > 3000) or (s0:LengthUnit/s0:Code='LB' and s0:Weight > 66131)">Y</xsl:when>
                <xsl:otherwise>N</xsl:otherwise>
              </xsl:choose>
            </OverWeightFlag>
            <OverWidthFlag>
              <xsl:choose>
                <xsl:when test="(s0:LengthUnit/s0:Code='CM' and s0:Width > 230) or (s0:LengthUnit/s0:Code='IN' and s0:Width > 90)">Y</xsl:when>
                <xsl:otherwise>N</xsl:otherwise>
              </xsl:choose>
            </OverWidthFlag>
            <xsl:variable name="containerLink" select="s0:ContainerLink/text()"/>
            <xsl:variable name="linkedContainer" select="/s0:UniversalShipment/s0:Shipment/s0:ContainerCollection/s0:Container[s0:Link/text()=$containerLink]"/>
            <xsl:choose>
              <xsl:when test="$linkedContainer/s0:IsControlledAtmosphere/text()='true'">
                <xsl:variable name="temp" select="concat($linkedContainer/s0:SetPointTemp, $linkedContainer/s0:SetPointTempUnit)"/>
                <TransportTemperatureRangeFrom>
                  <xsl:value-of select="$temp"/>
                </TransportTemperatureRangeFrom>
                <TransportTemperatureRangeTo>
                  <xsl:value-of select="$temp"/>
                </TransportTemperatureRangeTo>
              </xsl:when>
              <xsl:otherwise>
                <TransportTemperatureRangeFrom></TransportTemperatureRangeFrom>
                <TransportTemperatureRangeTo></TransportTemperatureRangeTo>
              </xsl:otherwise>
            </xsl:choose>
          </CargoDetails>
        </xsl:for-each>
      </BookingDetails>
    </BookingRequest>
  </xsl:template>

  <xsl:template name="Remarks">
    <xsl:param name="text"/>

    <xsl:call-template name="Remarks-Core">
      <xsl:with-param name="text" select="userCSharp:ReplaceCRLFText($text)"/>
      <xsl:with-param name="segmentCount" select="1"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="Remarks-Core">
    <xsl:param name="text"/>
    <xsl:param name="segmentCount"/>
    <xsl:if test="$text!='' and number($segmentCount)&lt;=2048">
      <xsl:variable name="lineLength" select="userCSharp:GetLengthForWrapping($text, 2048)"/>
      <xsl:variable name="RemarksText" select="substring($text, 1, $lineLength)"/>
      <xsl:variable name="RemarksTextNorm" select="normalize-space($RemarksText)"/>
      <xsl:variable name="remainingText" select="substring($text, $lineLength+1)"/>
      <xsl:choose>
        <xsl:when test="$RemarksTextNorm!=''">
          <Remarks>
            <xsl:value-of select="$RemarksTextNorm"/>
          </Remarks>
          <xsl:call-template name="Remarks-Core">
            <xsl:with-param name="text" select="$remainingText"/>
            <xsl:with-param name="segmentCount" select="$segmentCount+1"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="Remarks-Core">
            <xsl:with-param name="text" select="$remainingText"/>
            <xsl:with-param name="segmentCount" select="$segmentCount"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>
  </xsl:template>

  <xsl:template name="ShippingMarks">
    <xsl:param name="text"/>

    <xsl:call-template name="ShippingMarks-Core">
      <xsl:with-param name="text" select="userCSharp:ReplaceCRLFText($text)"/>
      <xsl:with-param name="segmentCount" select="1"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="ShippingMarks-Core">
    <xsl:param name="text"/>
    <xsl:param name="segmentCount"/>
    <xsl:if test="$text!='' and number($segmentCount)&lt;=2048">
      <xsl:variable name="lineLength" select="userCSharp:GetLengthForWrapping($text, 2048)"/>
      <xsl:variable name="ShippingMarksText" select="substring($text, 1, $lineLength)"/>
      <xsl:variable name="ShippingMarksTextNorm" select="normalize-space($ShippingMarksText)"/>
      <xsl:variable name="remainingText" select="substring($text, $lineLength+1)"/>
      <xsl:choose>
        <xsl:when test="$ShippingMarksTextNorm!=''">
          <ShippingMarks>
            <xsl:value-of select="$ShippingMarksTextNorm"/>
          </ShippingMarks>
          <xsl:call-template name="ShippingMarks-Core">
            <xsl:with-param name="text" select="$remainingText"/>
            <xsl:with-param name="segmentCount" select="$segmentCount+1"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="ShippingMarks-Core">
            <xsl:with-param name="text" select="$remainingText"/>
            <xsl:with-param name="segmentCount" select="$segmentCount"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>
  </xsl:template>
  
  <xsl:template name="Address">
    <xsl:param name="id"/>
    <xsl:param name="orgAddr"/>

    <xsl:if test="count($orgAddr) > 0">
      <xsl:variable name="company" select="normalize-space($orgAddr/s0:CompanyName)"/>
      <xsl:variable name="address1" select="normalize-space($orgAddr/s0:Address1)"/>
      <xsl:variable name="address2" select="normalize-space($orgAddr/s0:Address2)"/>
      <xsl:variable name="city" select="normalize-space($orgAddr/s0:City)"/>
      <xsl:variable name="state" select="normalize-space($orgAddr/s0:State)"/>
      <xsl:variable name="postcode" select="normalize-space($orgAddr/s0:Postcode)"/>
      <xsl:variable name="country" select="normalize-space($orgAddr/s0:Country/s0:Name)"/>
      <xsl:variable name="phone" select="normalize-space($orgAddr/s0:Phone)"/>
      <xsl:variable name="fax" select="normalize-space($orgAddr/s0:Fax)"/>
      <xsl:variable name="email" select="normalize-space($orgAddr/s0:Email)"/>

      <xsl:variable name="cityStatePostcode" select="concat($city, ' ', $state, ' ', $postcode)"/>
      <xsl:variable name="space40" select="'                                        '"/>

      <xsl:variable name="address" select="concat(
                  substring(concat($company, $space40), 1, ceiling(string-length($company) div 40) * 40),
                  substring(concat($address1, $space40), 1, ceiling(string-length($address1) div 40) * 40),
                  substring(concat($address2, $space40), 1, ceiling(string-length($address2) div 40) * 40),
                  substring(concat($cityStatePostcode, $space40), 1, ceiling(string-length($cityStatePostcode) div 40) * 40),
                  $country)"/>
      <xsl:if test="$company!='' or $address1!=''">
      <Address>
        <AddressID>
          <xsl:value-of select="$id"/>
        </AddressID>
        <AddressLine1>
          <xsl:value-of select="normalize-space(substring($address,1,40))"/>
        </AddressLine1>
        <AddressLine2>
          <xsl:value-of select="normalize-space(substring($address,41,40))"/>
        </AddressLine2>
        <AddressLine3>
          <xsl:value-of select="normalize-space(substring($address,81,40))"/>
        </AddressLine3>
        <AddressLine4>
          <xsl:value-of select="normalize-space(substring($address,121,40))"/>
        </AddressLine4>
        <xsl:if test ="string-length($address) > 160">
          <AddressLine5>
            <xsl:value-of select="normalize-space(substring($address,161,40))"/>
          </AddressLine5>
        </xsl:if>
        <xsl:if test ="string-length($address) > 200">
          <AddressLine6>
            <xsl:value-of select="normalize-space(substring($address,201,40))"/>
          </AddressLine6>
        </xsl:if>
        <Phone>
          <xsl:value-of select="$phone"/>
        </Phone>
        <Fax>
          <xsl:value-of select="$fax"/>
        </Fax>
        <Email>
          <xsl:value-of select="$email"/>
        </Email>
      </Address>
      </xsl:if>
    </xsl:if>
  </xsl:template>
  
  
  <xsl:template name="Commodity">
    <xsl:param name="text"/>

    <xsl:call-template name="Commodity-Core">
      <xsl:with-param name="text" select="userCSharp:ReplaceCRLFText($text)"/>
      <xsl:with-param name="segmentCount" select="1"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="Commodity-Core">
    <xsl:param name="text"/>
    <xsl:param name="segmentCount"/>
    <xsl:if test="$text!='' and number($segmentCount)&lt;=50">
      <xsl:variable name="lineLength" select="userCSharp:GetLengthForWrapping($text, 50)"/>
      <xsl:variable name="CommodityText" select="substring($text, 1, $lineLength)"/>
      <xsl:variable name="CommodityTextNorm" select="normalize-space($CommodityText)"/>
      <xsl:variable name="remainingText" select="substring($text, $lineLength+1)"/>
      <xsl:choose>
        <xsl:when test="$CommodityTextNorm!=''">
          <Commodity>
            <xsl:value-of select="$CommodityTextNorm"/>
          </Commodity>
          <xsl:call-template name="Commodity-Core">
            <xsl:with-param name="text" select="$remainingText"/>
            <xsl:with-param name="segmentCount" select="$segmentCount+1"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="Commodity-Core">
            <xsl:with-param name="text" select="$remainingText"/>
            <xsl:with-param name="segmentCount" select="$segmentCount"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>
  </xsl:template>

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[
    public string ReplaceCRLFText(string text)
{
  if (text != null && text.Length > 0)
  {
    return text.Replace("\r\n", "\n");
  }
  return "";
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

  for (int i = maxLength; i >= 0; i--)
  {
    if (text[i] == ' ')
    {
      return i == 0 ? 1 : i;
    }
  }

  return maxLength;
}
    ]]>
  </msxsl:script>
</xsl:stylesheet>
<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 CodeMapper DateMapper ContextAccessor DataModelAccessor OCMHelper userCSharp stringMapper StringWrapper UnitConverter TransportLegHelper SubscriptionHelper ListHelper" version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:DateMapper="http://schemas.microsoft.com/BizTalk/2003/DateMapper"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
                xmlns:DataModelAccessor="http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"
                xmlns:OCMHelper="http://schemas.microsoft.com/BizTalk/2003/OCMHelper"
                xmlns:stringMapper="http://schemas.microsoft.com/BizTalk/2003/stringMapper"
                xmlns:StringWrapper="http://schemas.microsoft.com/BizTalk/2003/StringWrapper"
                xmlns:UnitConverter="http://schemas.microsoft.com/BizTalk/2003/UnitConverter"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp"
                xmlns:TransportLegHelper="http://schemas.microsoft.com/BizTalk/2003/TransportLegHelper"
                xmlns:SubscriptionHelper="http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper"
                xmlns:ListHelper="http://schemas.microsoft.com/BizTalk/2003/ListHelper">
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
    <xsl:variable name="InterchangeNum" select="CodeMapper:CallActionProcedureHelper('GetCounterValue','@value','@name','CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.WWA.SI','@maxlength','14')" />
    <xsl:variable name="SenderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
    <xsl:variable name="RecipientID" select="ContextAccessor:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
    <xsl:variable name="ServiceProvider" select="OCMHelper:GetServiceProvider($RecipientID)" />
    <xsl:variable name="WWAID" select="DataModelAccessor:GetClientRegistrationCode($SenderID, s0:DataContext/s0:EventBranch/s0:Code/text(), 'WWA')"/>
    <xsl:variable name="firstSeaLeg" select="s0:TransportLegCollection/s0:TransportLeg[s0:LegOrder/text()=$minSeaLegOrder]" />
    <xsl:variable name="currentDateTime" select="DateMapper:CurrentDateTime('yyyyMMdd_mmssms')" />
    <xsl:variable name="FileName" select="ContextAccessor:SetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06', concat('WWA_Booking','_Cargowise','_',$currentDateTime,'_',$InterchangeNum))"/>
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
    <xsl:variable name="SubscribeInterchangeNum" select="DataModelAccessor:InsertSubscriptionValue('WWAMSG', 'WWA', $SenderID, $InterchangeNum, $consolID)" />
    <xsl:variable name="shipmentType" select="s0:ShipmentType/s0:Code/text()"/>
    <xsl:variable name="isCoLoad" select="OCMHelper:IsCoLoad($shipmentType) = 'TRUE'" />
    <xsl:variable name="addressType">
      <xsl:choose>
        <xsl:when test="$isCoLoad">
          <xsl:value-of select="'CoLoadWith'"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="'ShippingLineAddress'"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="previousConsolReferenceWithJobNo" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', 'WWA', '@recipientId', $SenderID , '@ST_ID', 'WWAMSG', '@value', $consolID, '@referenceType', 'JobNumber')" />
    <xsl:variable name="previousConsolReference">
      <xsl:choose>
        <xsl:when test="$previousConsolReferenceWithJobNo != ''">
          <xsl:value-of select="$previousConsolReferenceWithJobNo" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', 'WWA', '@recipientId', $SenderID , '@ST_ID', 'WWAMSG', '@value', $consolID)" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="InboxPK" select="ContextAccessor:GetContextProperty('InternalTrackingID', 'http://cargowise.com/ehub/tracking/2010/06')"/>
    <xsl:variable name="SubscribeInboxPK" select="DataModelAccessor:InsertSubscriptionValue('WWAMSG', 'WWA', $SenderID, $InboxPK, $InterchangeNum)" />
    <xsl:variable name="SubscriberConsolReference">
      <xsl:choose>
        <xsl:when test="$previousConsolReference!=''">
          <xsl:value-of select="$previousConsolReference" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:variable name="formattedCounter" select='format-number($InterchangeNum, "0000000000")' />
          <xsl:variable name="NewConsolReference" select="concat('WWA', $formattedCounter)" />
          <xsl:variable name="SubScriberValue1" select="DataModelAccessor:InsertSubscriptionValue('WWAMSG', 'WWA', $SenderID, $NewConsolReference, $consolID, 'JobNumber')" />
          <xsl:variable name="SubScriberValue2" select="DataModelAccessor:InsertSubscriptionValue('WWAMSG', 'WWA', $SenderID, $consolID, $NewConsolReference, 'JobNumber')" />
          <xsl:value-of select="$NewConsolReference" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="purposeCode" select="s0:DataContext/s0:DocumentaryOverride/s0:Purpose/s0:Code/text()"/>
    <xsl:variable name="SubscribePurpose" select="DataModelAccessor:InsertSubscriptionValue('WWAMSG', 'WWA', $SenderID, $SubscriberConsolReference, $purposeCode, 'ActionPurpose')" />
    <xsl:variable name="subscribeShipmentType" select="DataModelAccessor:InsertSubscriptionValue('WWAMSG', 'WWA', $SenderID, $SubscriberConsolReference, $shipmentType, 'ShipmentType')" />

    <xsl:variable name="formVersion" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='FormVersion']/s0:Value/text()" />
    <xsl:if test="$formVersion != ''">
      <xsl:variable name="SubscribeFormVersion" select="DataModelAccessor:InsertSubscriptionValue('WWAMSG', 'WWA', $SenderID, $SubscriberConsolReference, $formVersion, 'FormVersion')" />
    </xsl:if>

    <xsl:variable name="groupingMethod" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='GroupingMethod']/s0:Value/text()"/>
    <xsl:variable name="InsertBoleroSubscription" select="SubscriptionHelper:InsertBoleroSubscription($SenderID, $WWAID, $ServiceProvider, $SubscriberConsolReference, $consolID)" />

    <BookingRequest xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="http://www.wwalliance.com/wiki/images/b/bd/WWA_Booking_Request_version_1.1.0.xsd">
      <BookingEnvelope>
        <SenderID>
          <xsl:variable name="senderId" select="CodeMapper:GetRecipientCodeUnkeyed('WWA' , 'WWA' , 'WWA Provider Configuration' , 'Sender ID' , 'Sender')"/>
          <xsl:value-of select="$senderId"/>
        </SenderID>
        <ReceiverID>
          <xsl:variable name="receiverID" select="CodeMapper:GetRecipientCodeUnkeyed('WWA' , 'WWA' , 'WWA Provider Configuration' , 'Receiver ID' , 'Receiver')"/>
          <xsl:value-of select="$receiverID"/>
        </ReceiverID>
        <Password>
          <xsl:variable name="password" select="CodeMapper:GetRecipientCodeUnkeyed('WWA' , 'WWA' , 'WWA Provider Configuration' , 'Password' , 'WWA Password')"/>
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
          <xsl:value-of select="substring-before(s0:DataContext/s0:TriggerDate, 'T')"/>
        </BookingDate>
        <LastSentDate>0000-00-00</LastSentDate>
        <RequestType>
          <xsl:choose>
            <xsl:when test="$purposeCode = 'ORG'">N</xsl:when>
            <xsl:when test="$purposeCode = 'AMD'">U</xsl:when>
            <xsl:when test="$purposeCode = 'WTH'">C</xsl:when>
          </xsl:choose>
        </RequestType>
        <BookingNumber>
          <xsl:choose>
            <xsl:when test="$isCoLoad">
              <xsl:value-of select="s0:CoLoadBookingConfirmationReference"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="s0:BookingConfirmationReference"/>
            </xsl:otherwise>
          </xsl:choose>
        </BookingNumber>
        <WWAShipmentReference></WWAShipmentReference>
        <CustomerControlCode>
          <xsl:value-of select="$WWAID"/>
        </CustomerControlCode>
        <BookingOffice>
          <xsl:variable name="carrierBookingOffice" select="s0:CarrierBookingOffice/s0:Code/text()"/>
          <xsl:choose>
            <xsl:when test="$carrierBookingOffice != ''">
              <xsl:value-of select="$carrierBookingOffice"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType=$addressType]/s0:Port/s0:Code"/>
            </xsl:otherwise>
          </xsl:choose>
        </BookingOffice>
        <xsl:variable name="freightCharge" select="s0:PaymentHandlingInstructionCollection/s0:PaymentHandlingInstruction[s0:Category/s0:Code/text()='FRT']"/>
        <FPI>
          <xsl:if test="$freightCharge!=''">
            <xsl:variable name="paymentMethod" select="$freightCharge/s0:PaymentMethod/s0:Code/text()" />
            <xsl:choose>
              <xsl:when test="$paymentMethod='PPD'">P</xsl:when>
              <xsl:otherwise>
                <xsl:variable name="payableElseWhereCode">
                  <xsl:choose>
                    <xsl:when test="$paymentMethod='ELS'">
                      <xsl:value-of select="OCMHelper:GetPayableElseWhereOutputCode($ServiceProvider)" />
                    </xsl:when>
                    <xsl:otherwise></xsl:otherwise>
                  </xsl:choose>
                </xsl:variable>

                <xsl:choose>
                  <xsl:when test="$payableElseWhereCode!=''">
                    <xsl:value-of select="$payableElseWhereCode"/>
                  </xsl:when>
                  <xsl:otherwise>C</xsl:otherwise>
                </xsl:choose>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:if>
        </FPI>
        <CommunicationReference>
          <xsl:value-of select="$SubscriberConsolReference"/>
        </CommunicationReference>
        <CustomerReference>
          <xsl:value-of select="s0:DataContext/s0:DataSourceCollection/s0:DataSource/s0:Key"/>
        </CustomerReference>

        <xsl:variable name="contractNumber">
          <xsl:variable name="cqnNumber" select="s0:AdditionalReferenceCollection/s0:AdditionalReference[s0:Type/s0:Code/text()='CQN']/s0:ReferenceNumber/text()" />
          <xsl:choose>
            <xsl:when test="$cqnNumber">
              <xsl:value-of select="$cqnNumber"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="s0:AdditionalReferenceCollection/s0:AdditionalReference[s0:Type/s0:Code/text()='CON']/s0:ReferenceNumber/text()"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:if test="$contractNumber != ''">
          <ContractNumber>
            <xsl:value-of select="$contractNumber"/>
          </ContractNumber>
        </xsl:if>

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
          <xsl:value-of select="s0:PlaceOfReceipt/s0:Code/text()"/>
        </CFSOrigin>
        <PortOfLoading>
          <xsl:value-of select="$firstSeaLeg/s0:PortOfLoading/s0:Code/text()"/>
        </PortOfLoading>
        <CFSDestination>
          <xsl:value-of select="s0:PlaceOfDelivery/s0:Code/text()"/>
        </CFSDestination>
        <PortOfDischarge>
          <xsl:value-of select="$lastSeaLeg/s0:PortOfDischarge/s0:Code/text()"/>
        </PortOfDischarge>
        <xsl:choose>
          <xsl:when test="s0:ContainerCollection/s0:Container[substring-after(s0:DeliveryMode[1]/text(), '/')='CFS']">
            <xsl:variable name="PODCode" select="s0:PortOfDestination/s0:Code/text()" />
            <FinalDestination>
              <xsl:value-of select="$PODCode"/>
            </FinalDestination>
            <FinalDestinationPlace>
              <xsl:value-of select="s0:PortOfDestination/s0:Name/text()"/>
            </FinalDestinationPlace>
            <FinalDestinationType>CFS</FinalDestinationType>
            <FinalDestinationCountry>
              <xsl:value-of select="substring($PODCode,1,2)"/>
            </FinalDestinationCountry>
          </xsl:when>
          <xsl:otherwise>
            <FinalDestination/>
            <FinalDestinationPlace/>
            <FinalDestinationType/>
            <FinalDestinationCountry/>
          </xsl:otherwise>
        </xsl:choose>
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

        <PickupFlag>
          <xsl:choose>
            <xsl:when test="s0:ContainerCollection/s0:Container[starts-with(s0:DeliveryMode[1]/text(),'CFS')]">Y</xsl:when>
            <xsl:otherwise>N</xsl:otherwise>
          </xsl:choose>
        </PickupFlag>

        <xsl:if test="s0:ContainerCollection/s0:Container[starts-with(s0:DeliveryMode[1]/text(),'CFS')]">
          <xsl:variable name="pickupAddress" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType='ConsignorPickupDeliveryAddress']"/>
          <xsl:variable name="pickupDate" select="s0:DateCollection/s0:Date[s0:Type/text()='Pickup']/s0:Value/text()" />
          <xsl:variable name="departureEstimatedPickup" select="s0:ContainerCollection/s0:Container[starts-with(s0:DeliveryMode[1]/text(),'CFS')]/s0:DepartureEstimatedPickup/text()"/>
          <xsl:variable name="pickupDateTime">
            <xsl:choose>
              <xsl:when test="$pickupDate!=''">
                <xsl:value-of select="$pickupDate" />
              </xsl:when>
              <xsl:when test="$departureEstimatedPickup!=''">
                <xsl:value-of select="$departureEstimatedPickup" />
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$firstSeaLeg/s0:LCLReceivalCommences/text()" />
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
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
              <xsl:value-of select="DateMapper:FormatXmlDateTime($pickupDateTime, 'yyyy-MM-dd')"/>
            </Date>
            <Time>
              <xsl:value-of select="DateMapper:FormatXmlDateTime($pickupDateTime, 'HH:mm')"/>
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

          <xsl:variable name="earliestDeparture" select="s0:DateCollection/s0:Date[s0:Type/text()='EarliestDeparture']/s0:Value/text()" />
          <xsl:variable name="etdCFS">
            <xsl:choose>
              <xsl:when test="$earliestDeparture!=''">
                <xsl:value-of select="$earliestDeparture"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$firstSeaLeg/s0:EstimatedDeparture/text()"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <ETDCFS>
            <xsl:value-of select="DateMapper:ConvertToDate($etdCFS, 'yyyy-MM-dd')"/>
          </ETDCFS>

          <xsl:variable name="latestDelivery" select="s0:DateCollection/s0:Date[s0:Type/text()='LatestDelivery']/s0:Value/text()" />
          <xsl:variable name="etaCFS">
            <xsl:choose>
              <xsl:when test="$latestDelivery!=''">
                <xsl:value-of select="$latestDelivery"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$lastSeaLeg/s0:EstimatedArrival/text()"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <ETACFS>
            <xsl:value-of select="DateMapper:ConvertToDate($etaCFS, 'yyyy-MM-dd')"/>
          </ETACFS>

          <ETSOrigin>
            <xsl:value-of select="DateMapper:ConvertToDate($firstSeaLeg/s0:EstimatedDeparture, 'yyyy-MM-dd')"/>
          </ETSOrigin>
          <ETSPoL>
            <xsl:value-of select="DateMapper:ConvertToDate(s0:TransportLegCollection/s0:TransportLeg[s0:LegType='Main']/s0:EstimatedDeparture, 'yyyy-MM-dd')"/>
          </ETSPoL>
        </SailingDetails>

        <xsl:for-each select="s0:TransportLegCollection/s0:TransportLeg">
          <xsl:variable name="addTransportLeg" select="TransportLegHelper:AddTransportLeg(s0:LegOrder/text(), s0:LegType/text(), s0:TransportMode/text(), s0:PortOfLoading/s0:Code/text(), s0:PortOfLoading/s0:Name/text(), s0:PortOfDischarge/s0:Code/text(), s0:PortOfDischarge/s0:Name/text(), s0:VesselName/text(), s0:VoyageFlightNo/text())" />
        </xsl:for-each>
        <xsl:variable name="calculateTransportLegs" select="TransportLegHelper:CalculateTransportLegs()" />
        <xsl:variable name="transportLegCollection" select="s0:TransportLegCollection"/>
        <xsl:for-each select="TransportLegHelper:NewTransportLegCollection()">
          <xsl:variable name="oldLegOrder" select="OldLegOrder/text()" />
          <xsl:variable name="oldTransportLeg" select="$transportLegCollection/s0:TransportLeg[s0:LegOrder/text() = $oldLegOrder]"/>

          <TransportationDetails>
            <CarriageType>
              <xsl:variable name="legType" select="LegType/text()"/>
              <xsl:choose>
                <xsl:when test="$legType = 'PreCarriage'">Pre</xsl:when>
                <xsl:when test="$legType = 'Main'">Main</xsl:when>
                <xsl:when test="$legType = 'OnForwarding'">On</xsl:when>
              </xsl:choose>
            </CarriageType>
            <Mode>
              <xsl:variable name="transportMode" select="TransportMode/text()"/>
              <xsl:choose>
                <xsl:when test="$transportMode = 'Sea'">4</xsl:when>
                <xsl:when test="$transportMode = 'Rail'">2</xsl:when>
                <xsl:when test="$transportMode = 'Road'">1</xsl:when>
                <xsl:when test="$transportMode = 'InlandWaterway'">3</xsl:when>
              </xsl:choose>
            </Mode>
            <PortOfLoading>
              <xsl:value-of select="PortOfLoadingCode/text()"/>
            </PortOfLoading>
            <EstimatedDeparture>
              <xsl:variable name="estimatedDeparture" select="$oldTransportLeg/s0:EstimatedDeparture/text()"/>
              <Date>
                <xsl:value-of select="substring-before($estimatedDeparture, 'T')"/>
              </Date>
              <Time>
                <xsl:value-of select="substring-after($estimatedDeparture, 'T')"/>
              </Time>
            </EstimatedDeparture>
            <PortOfDischarge>
              <xsl:value-of select="PortOfDischargeCode/text()"/>
            </PortOfDischarge>
            <EstimatedArrival>
              <xsl:variable name="estimatedArrival" select="$oldTransportLeg/s0:EstimatedArrival/text()"/>
              <Date>
                <xsl:value-of select="substring-before($estimatedArrival, 'T')"/>
              </Date>
              <Time>
                <xsl:value-of select="substring-after($estimatedArrival, 'T')"/>
              </Time>
            </EstimatedArrival>
            <VesselName>
              <xsl:value-of select="VesselName/text()"/>
            </VesselName>
            <VesselIMO>
              <xsl:value-of select="$oldTransportLeg/s0:VesselLloydsIMO/text()"/>
            </VesselIMO>
            <Vesselvoyage>
              <xsl:value-of select="VoyageFlightNo/text()"/>
            </Vesselvoyage>
          </TransportationDetails>
        </xsl:for-each>

        <xsl:for-each select="s0:PackingLineCollection/s0:PackingLine">
          <CargoDetails>
            <xsl:variable name="packingLinePackQty" select="s0:PackQty/text()" />
            <Pieces>
              <xsl:value-of select="$packingLinePackQty"/>
            </Pieces>
            <xsl:call-template name="ShippingMarks">
              <xsl:with-param name="text" select="s0:MarksAndNos/text()"/>
            </xsl:call-template>
            <Packaging>
              <xsl:value-of select="s0:PackType/s0:Description/text()"/>
            </Packaging>

            <xsl:variable name="goodsDescription">
              <xsl:call-template name="GetValue">
                <xsl:with-param name="value1" select="s0:DetailedDescription/text()"/>
                <xsl:with-param name="fallback" select="s0:GoodsDescription/text()"/>
              </xsl:call-template>
            </xsl:variable>
            <xsl:call-template name="Commodity">
              <xsl:with-param name="text" select="$goodsDescription" />
            </xsl:call-template>

            <xsl:variable name="harmonisedCode">
              <xsl:choose>
                <xsl:when test="s0:HarmonisedCode/text()!=''">
                  <xsl:value-of select="s0:HarmonisedCode/text()"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="s0:ClassificationCollection/s0:Classification[1]/s0:Code/text()"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <HSCode>
              <xsl:value-of select="userCSharp:StringReplace($harmonisedCode, '.', '')"/>
            </HSCode>
            <xsl:variable name="packingLineWeight" select="s0:Weight/text()" />
            <Weight>
              <xsl:value-of select="$packingLineWeight"/>
            </Weight>
            <Volume>
              <xsl:value-of select="s0:Volume/text()"/>
            </Volume>
            <UOM>
              <xsl:choose>
                <xsl:when test="s0:WeightUnit/s0:Code/text() = 'LB'">E</xsl:when>
                <xsl:otherwise>M</xsl:otherwise>
              </xsl:choose>
            </UOM>

            <HazardousFlag>
              <xsl:choose>
                <xsl:when test="$groupingMethod!='' and count(s0:PackingLineCollection/s0:PackingLine/s0:UNDGCollection/s0:UNDG)>0">Y</xsl:when>
                <xsl:when test="count(s0:UNDGCollection/s0:UNDG)>0">Y</xsl:when>
                <xsl:otherwise>N</xsl:otherwise>
              </xsl:choose>
            </HazardousFlag>

            <xsl:choose>
              <xsl:when test="$groupingMethod!=''">
                <xsl:for-each select="s0:PackingLineCollection/s0:PackingLine">
                  <xsl:variable name="count" select="count(s0:UNDGCollection/s0:UNDG[s0:UNDGCode/text()!=''])" />

                  <xsl:for-each select="s0:UNDGCollection/s0:UNDG[s0:UNDGCode/text()!='']">
                    <xsl:variable name="undgWeight" select="s0:Weight/text()" />
                    <xsl:variable name="undgPackQty" select="s0:PackQty/text()" />
                    <xsl:call-template name="UNDG-Core">
                      <xsl:with-param name="undg" select="."/>
                      <xsl:with-param name="undgPackQty">
                        <xsl:choose>
                          <xsl:when test="$undgPackQty!='' and number($undgPackQty) > 0">
                            <xsl:value-of select="$undgPackQty" />
                          </xsl:when>
                          <xsl:when test="$count=1">
                            <xsl:value-of select="$packingLinePackQty" />
                          </xsl:when>
                        </xsl:choose>
                      </xsl:with-param>

                      <xsl:with-param name="undgWeight">
                        <xsl:choose>
                          <xsl:when test="$undgWeight!='' and number($undgWeight) > 0">
                            <xsl:value-of select="$undgWeight" />
                          </xsl:when>
                          <xsl:when test="$count=1">
                            <xsl:value-of select="$packingLineWeight" />
                          </xsl:when>
                        </xsl:choose>
                      </xsl:with-param>
                    </xsl:call-template>
                  </xsl:for-each>
                </xsl:for-each>
              </xsl:when>
              <xsl:otherwise>
                <xsl:variable name="count" select="count(s0:UNDGCollection/s0:UNDG[s0:UNDGCode!=''])" />
                <xsl:for-each select="s0:UNDGCollection/s0:UNDG">
                  <xsl:variable name="undgWeight" select="s0:Weight/text()" />
                  <xsl:variable name="undgPackQty" select="s0:PackQty/text()" />

                  <xsl:call-template name="UNDG-Core">
                    <xsl:with-param name="undg" select="."/>
                    <xsl:with-param name="undgPackQty">
                      <xsl:choose>
                        <xsl:when test="$undgPackQty!='' and number($undgPackQty) > 0">
                          <xsl:value-of select="$undgPackQty" />
                        </xsl:when>
                        <xsl:when test="$count=1">
                          <xsl:value-of select="$packingLinePackQty" />
                        </xsl:when>
                      </xsl:choose>
                    </xsl:with-param>

                    <xsl:with-param name="undgWeight">
                      <xsl:choose>
                        <xsl:when test="$undgWeight!='' and number($undgWeight) > 0">
                          <xsl:value-of select="$undgWeight" />
                        </xsl:when>
                        <xsl:when test="$count=1">
                          <xsl:value-of select="$packingLineWeight" />
                        </xsl:when>
                      </xsl:choose>
                    </xsl:with-param>
                  </xsl:call-template>
                </xsl:for-each>
              </xsl:otherwise>
            </xsl:choose>

            <xsl:variable name="quantity" select="s0:PackQty/text()"/>
            <xsl:variable name="unit" select="s0:LengthUnit/s0:Code/text()"/>
            <xsl:variable name="lengthValue" select="s0:Length/text()"/>
            <xsl:variable name="length">
              <xsl:choose>
                <xsl:when test="$lengthValue!='' and number($lengthValue) > 0">
                  <xsl:value-of select="stringMapper:FormatDecimal(UnitConverter:Convert($lengthValue, $unit, 'CM'), '#', false())"/>
                </xsl:when>
                <xsl:otherwise>0</xsl:otherwise>
              </xsl:choose>
            </xsl:variable>

            <xsl:variable name="widthValue" select="s0:Width/text()"/>
            <xsl:variable name="width">
              <xsl:choose>
                <xsl:when test="$widthValue!='' and number($widthValue) > 0">
                  <xsl:value-of select="stringMapper:FormatDecimal(UnitConverter:Convert($widthValue, $unit, 'CM'), '#', false())"/>
                </xsl:when>
                <xsl:otherwise>0</xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <xsl:variable name="heightValue" select="s0:Height/text()"/>
            <xsl:variable name="height">
              <xsl:choose>
                <xsl:when test="$heightValue!='' and number($heightValue) > 0">
                  <xsl:value-of select="stringMapper:FormatDecimal(UnitConverter:Convert($heightValue, $unit, 'CM'), '#', false())"/>
                </xsl:when>
                <xsl:otherwise>0</xsl:otherwise>
              </xsl:choose>
            </xsl:variable>

            <xsl:if test="number($length) > 0 and number($width) > 0 and number($height) > 0">
              <ShipmentRelatedData>
                <xsl:if test="number($quantity) > 0">
                  <Quantity>
                    <xsl:value-of select="$quantity"/>
                  </Quantity>
                </xsl:if>
                <Length>
                  <xsl:value-of select="$length"/>
                </Length>
                <Width>
                  <xsl:value-of select="$width"/>
                </Width>
                <Height>
                  <xsl:value-of select="$height"/>
                </Height>
              </ShipmentRelatedData>
            </xsl:if>

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

  <xsl:template name="UNDG-Core">
    <xsl:param name="undg"/>
    <xsl:param name="undgPackQty"/>
    <xsl:param name="undgWeight"/>

    <HazardousDetails>
      <HazardousClass>
        <xsl:value-of select="$undg/s0:IMOClass/text()"/>
      </HazardousClass>
      <Flashpoint>
        <xsl:value-of select="$undg/s0:FlashPoint/text()"/>
      </Flashpoint>
      <FlashpointFlag>
        <xsl:if test="$undg/s0:FlashPoint/text()!=''">C</xsl:if>
      </FlashpointFlag>
      <ShippingName>
        <xsl:value-of select="$undg/s0:ProperShippingName/text()"/>
      </ShippingName>
      <UNNumber>
        <xsl:value-of select="substring($undg/s0:UNDGCode/text(), 1, 4)"/>
      </UNNumber>
      <PackingGroup>
        <xsl:value-of select="$undg/s0:PackingGroup/text()"/>
      </PackingGroup>
      <Pieces>
        <xsl:value-of select="$undgPackQty"/>
      </Pieces>
      <Weight>
        <xsl:value-of select="$undgWeight"/>
      </Weight>
    </HazardousDetails>
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
      
      <xsl:if test="$company!='' or $address1!=''">
        <Address>
          <AddressID>
            <xsl:value-of select="$id"/>
          </AddressID>
          
          <xsl:variable name="setupStringWrapper" select="StringWrapper:SetupStringWrapper(40, 6)" />
          <xsl:variable name="wrapCompanyName" select="StringWrapper:SplitStringInWrapper($company)" />
          <xsl:variable name="wrapAddress" select="StringWrapper:SplitStringInWrapper(concat($address1, ' ', $address2))" />
          <xsl:variable name="wrapCityStatePostcode" select="StringWrapper:SplitStringInWrapper(concat($cityStatePostcode, ' ', $country))" />
          <AddressLine1>
            <xsl:value-of select="normalize-space(StringWrapper:GetTextFromCurrentIndex())" />
          </AddressLine1>
          <AddressLine2>
            <xsl:value-of select="normalize-space(StringWrapper:GetTextFromCurrentIndex())" />
          </AddressLine2>
          <AddressLine3>
            <xsl:value-of select="normalize-space(StringWrapper:GetTextFromCurrentIndex())" />
          </AddressLine3>
          <AddressLine4>
            <xsl:value-of select="normalize-space(StringWrapper:GetTextFromCurrentIndex())" />
          </AddressLine4>
          
          <xsl:if test ="StringWrapper:Count() > 4">
            <AddressLine5>
              <xsl:value-of select="normalize-space(StringWrapper:GetTextFromCurrentIndex())" />
            </AddressLine5>
          </xsl:if>
          
          <xsl:if test ="StringWrapper:Count() > 5">
            <AddressLine6>
              <xsl:value-of select="normalize-space(StringWrapper:GetTextFromCurrentIndex())" />
            </AddressLine6>
          </xsl:if>
          
          <xsl:variable name="resetWrapper" select="StringWrapper:ResetStringWrapper()" />
          
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

  <xsl:template name="GetValue">
    <xsl:param name="value1"/>
    <xsl:param name="value2" select="''"/>
    <xsl:param name="fallback" select="''"/>

    <xsl:choose>
      <xsl:when test="$value1 != ''">
        <xsl:value-of select="$value1"/>
      </xsl:when>
      <xsl:when test="$value2 != ''">
        <xsl:value-of select="$value2"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$fallback"/>
      </xsl:otherwise>
    </xsl:choose>
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
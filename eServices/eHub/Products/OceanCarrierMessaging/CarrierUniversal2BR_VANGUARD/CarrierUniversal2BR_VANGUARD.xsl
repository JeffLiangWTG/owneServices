<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 CodeMapper DateMapper ContextAccessor DataModelAccessor OCMHelper StringHelper ListHelper SubscriptionHelper userCSharp" version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:DateMapper="http://schemas.microsoft.com/BizTalk/2003/DateMapper"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
                xmlns:DataModelAccessor="http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"
                xmlns:OCMHelper="http://schemas.microsoft.com/BizTalk/2003/OCMHelper"
                xmlns:StringHelper="http://schemas.microsoft.com/BizTalk/2003/StringHelper"
                xmlns:ListHelper="http://schemas.microsoft.com/BizTalk/2003/ListHelper"
                xmlns:SubscriptionHelper="http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes" />

  <xsl:variable name="SenderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>

  <xsl:template match="/s0:UniversalShipment/s0:Shipment">
    <xsl:variable name="RecipientID" select="ContextAccessor:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
    <xsl:variable name="ServiceProvider" select="OCMHelper:GetServiceProvider($RecipientID)" />
    <xsl:variable name="VanguardID" select="DataModelAccessor:GetClientRegistrationCode($SenderID, s0:DataContext/s0:EventBranch/s0:Code/text(), 'VANGUARD')" />
    <xsl:variable name="InterchangeNum" select="CodeMapper:CallActionProcedureHelper('GetCounterValue','@value','@name','CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.VANGUARD','@maxlength','14')" />

    <xsl:variable name="consolID" select="s0:DataContext/s0:DataSourceCollection/s0:DataSource/s0:Key[1]/text()" />
    <xsl:variable name="SubscribeInterchangeNum" select="DataModelAccessor:InsertSubscriptionValue('VGDMSG', 'VANGUARD', $SenderID, $InterchangeNum, $consolID)" />
    <xsl:variable name="shipmentType" select="s0:ShipmentType/s0:Code/text()"/>
    <xsl:variable name="isCoLoad" select="OCMHelper:IsCoLoad($shipmentType) = 'TRUE'" />

    <xsl:variable name="OrderedLegs">
      <xsl:for-each select="s0:TransportLegCollection/s0:TransportLeg">
        <xsl:sort select="s0:LegOrder" order="ascending"/>
        <xsl:copy-of select="."/>
      </xsl:for-each>
    </xsl:variable>
    <xsl:variable name="mainLegPosition">
      <xsl:for-each select="msxsl:node-set($OrderedLegs)/s0:TransportLeg">
        <xsl:if test="s0:LegType='Main'">
          <xsl:value-of select="position()"/>
        </xsl:if>
      </xsl:for-each>
    </xsl:variable>
    <xsl:variable name="seaLegAfterMain" select="msxsl:node-set($OrderedLegs)/s0:TransportLeg[s0:TransportMode='Sea' and position() = $mainLegPosition + 1]"/>
    <xsl:variable name="minSeaLegOrder">
      <xsl:for-each select="s0:TransportLegCollection/s0:TransportLeg[s0:TransportMode='Sea']/s0:LegOrder">
        <xsl:sort select="." data-type="number" order="ascending"/>
        <xsl:if test="position() = 1">
          <xsl:value-of select="."/>
        </xsl:if>
      </xsl:for-each>
    </xsl:variable>
    <xsl:variable name="firstSeaLeg" select="s0:TransportLegCollection/s0:TransportLeg[s0:LegOrder/text()=$minSeaLegOrder]" />
    <xsl:variable name="maxSeaLegOrder">
      <xsl:for-each select="s0:TransportLegCollection/s0:TransportLeg[s0:TransportMode='Sea']/s0:LegOrder">
        <xsl:sort select="." data-type="number" order="descending"/>
        <xsl:if test="position() = 1">
          <xsl:value-of select="."/>
        </xsl:if>
      </xsl:for-each>
    </xsl:variable>
    <xsl:variable name="lastSeaLeg" select="s0:TransportLegCollection/s0:TransportLeg[s0:LegOrder/text()=$maxSeaLegOrder]" />

    <xsl:variable name="formattedCounter" select='format-number($InterchangeNum, "0000000000")' />
    <xsl:variable name="previousConsolReferenceWithJobNo" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', 'VANGUARD', '@recipientId', $SenderID , '@ST_ID', 'VGDMSG', '@value', $consolID, '@referenceType', 'JobNumber')" />
    <xsl:variable name="previousConsolReference">
      <xsl:choose>
        <xsl:when test="$previousConsolReferenceWithJobNo != ''">
          <xsl:value-of select="$previousConsolReferenceWithJobNo" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', 'VANGUARD', '@recipientId', $SenderID , '@ST_ID', 'VGDMSG', '@value', $consolID)" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="InboxPK" select="ContextAccessor:GetContextProperty('InternalTrackingID', 'http://cargowise.com/ehub/tracking/2010/06')"/>
    <xsl:variable name="SubscribeInboxPK" select="DataModelAccessor:InsertSubscriptionValue('VGDMSG', 'VANGUARD', $SenderID, $InboxPK, $InterchangeNum)" />
    <xsl:variable name="SubscriberConsolReference">
      <xsl:choose>
        <xsl:when test="$previousConsolReference!=''">
          <xsl:value-of select="$previousConsolReference" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:variable name="NewConsolReference" select="concat('VGD', $formattedCounter)" />
          <xsl:variable name="SubScriberValue1" select="DataModelAccessor:InsertSubscriptionValue('VGDMSG', 'VANGUARD', $SenderID, $NewConsolReference, $consolID, 'JobNumber')" />
          <xsl:variable name="SubScriberValue2" select="DataModelAccessor:InsertSubscriptionValue('VGDMSG', 'VANGUARD', $SenderID, $consolID, $NewConsolReference, 'JobNumber')" />
          <xsl:value-of select="$NewConsolReference" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="purposeCode" select="s0:DataContext/s0:DocumentaryOverride/s0:Purpose/s0:Code/text()"/>
    <xsl:variable name="SubscribePurpose" select="DataModelAccessor:InsertSubscriptionValue('VGDMSG', 'VANGUARD', $SenderID, $SubscriberConsolReference, $purposeCode, 'ActionPurpose')" />
    <xsl:variable name="subscribeShipmentType" select="DataModelAccessor:InsertSubscriptionValue('VGDMSG', 'VANGUARD', $SenderID, $SubscriberConsolReference, $shipmentType, 'ShipmentType')" />
    <xsl:variable name="formVersion" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='FormVersion']/s0:Value/text()" />
    <xsl:if test="$formVersion !=''">
      <xsl:variable name="SubscribeFormVersion" select="DataModelAccessor:InsertSubscriptionValue('VGDMSG', 'VANGUARD', $SenderID, $SubscriberConsolReference, $formVersion, 'FormVersion')" />
    </xsl:if>

    <xsl:variable name="FileName" select="ContextAccessor:SetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06', concat('VGD_BR_', $formattedCounter))"/>
    <xsl:variable name="InsertBoleroSubscription" select="SubscriptionHelper:InsertBoleroSubscription($SenderID, $VanguardID, $ServiceProvider, $SubscriberConsolReference, $consolID)" />

    <ns0:BookingRequest xmlns:ns0="http://www.cargowise.com/Schemas/VANGUARD">
      <Envelope>
        <SenderID>
          <xsl:value-of select="CodeMapper:GetRecipientCodeUnkeyed('VANGUARD' , 'VANGUARD' , 'VANGUARD Provider Configuration' , 'Sender ID' , 'Sender')"/>
        </SenderID>
        <ReceiverID>
          <xsl:value-of select="CodeMapper:GetRecipientCodeUnkeyed('VANGUARD' , 'VANGUARD' , 'VANGUARD Provider Configuration' , 'Receiver ID' , 'Receiver')"/>
        </ReceiverID>
        <MessageType>BookingRequest</MessageType>
        <MessageVersion>03.07</MessageVersion>
        <EnvelopeID>
          <xsl:value-of select="$InterchangeNum"/>
        </EnvelopeID>
        <TransmissionDate>
          <xsl:value-of select="DateMapper:CurrentDateTime('yyyy-MM-dd')"/>
        </TransmissionDate>
        <TransmissionTime>
          <xsl:value-of select="DateMapper:CurrentDateTime('HH:mm:ss')"/>
        </TransmissionTime>
      </Envelope>
      <BookingDetails>
        <BookingType>
          <xsl:choose>
            <xsl:when test="s0:ContainerMode/s0:Code/text()='FCL'">F</xsl:when>
            <xsl:when test="s0:ContainerMode/s0:Code/text()='LCL'">L</xsl:when>
            <xsl:otherwise></xsl:otherwise>
          </xsl:choose>
        </BookingType>
        <BookingDate>
          <xsl:value-of select="DateMapper:ConvertToDate(s0:DataContext/s0:TriggerDate, 'yyyy-MM-dd')"/>
        </BookingDate>
        <RequestType>
          <xsl:choose>
            <xsl:when test="$purposeCode='ORG'">N</xsl:when>
            <xsl:when test="$purposeCode='AMD'">U</xsl:when>
            <xsl:when test="$purposeCode='WTH'">C</xsl:when>
          </xsl:choose>
        </RequestType>
        <BusinessType>O</BusinessType>
        <OwnerID></OwnerID>
        <ManageBy></ManageBy>
        <BookingNumber>
          <xsl:choose>
            <xsl:when test="$isCoLoad">
              <xsl:value-of select="s0:CoLoadBookingConfirmationReference/text()"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="s0:BookingConfirmationReference/text()"/>
            </xsl:otherwise>
          </xsl:choose>
        </BookingNumber>
        <CustomerControlCode>
          <xsl:value-of select="$VanguardID"/>
        </CustomerControlCode>
        <CarrierBookingOfficeCode>
          <xsl:value-of select="s0:CarrierBookingOffice/s0:Code/text()"/>
        </CarrierBookingOfficeCode>
        <CarrierBookingOfficeName>
          <xsl:value-of select="s0:CarrierBookingOffice/s0:Name/text()"/>
        </CarrierBookingOfficeName>
        <CommunicationReference>
          <xsl:value-of select="$SubscriberConsolReference"/>
        </CommunicationReference>
        <CustomerReference>
          <xsl:value-of select="$consolID"/>
        </CustomerReference>

        <xsl:variable name="cqnNumber" select="s0:AdditionalReferenceCollection/s0:AdditionalReference[s0:Type/s0:Code/text()='CQN']/s0:ReferenceNumber/text()" />
        <ContractNumber>
          <xsl:choose>
            <xsl:when test="$cqnNumber">
              <xsl:value-of select="$cqnNumber"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="s0:AdditionalReferenceCollection/s0:AdditionalReference[s0:Type/s0:Code/text()='CON']/s0:ReferenceNumber/text()"/>
            </xsl:otherwise>
          </xsl:choose>
        </ContractNumber>

        <xsl:variable name="currentUser" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='CurrentUser']" />
        <xsl:variable name="customerEmail" select="$currentUser/s0:Email/text()" />
        <CustomerContact>
          <xsl:value-of select="$currentUser/s0:Contact"/>
        </CustomerContact>
        <CustomerPhone>
          <xsl:value-of select="$currentUser/s0:Phone"/>
        </CustomerPhone>
        <CustomerEmail>
          <xsl:value-of select="$customerEmail"/>
        </CustomerEmail>
        <CFSOrigin>
          <xsl:value-of select="StringHelper:ToUpper(s0:PlaceOfReceipt/s0:Code/text())"/>
        </CFSOrigin>
        <CFSDestination>
          <xsl:value-of select="StringHelper:ToUpper(s0:PlaceOfDelivery/s0:Code/text())"/>
        </CFSDestination>
        <LoadPort>
          <xsl:value-of select="StringHelper:ToUpper($firstSeaLeg/s0:PortOfLoading/s0:Code/text())"/>
        </LoadPort>
        <TRSPort>
          <xsl:value-of select="StringHelper:ToUpper($seaLegAfterMain/s0:PortOfLoading/s0:Code/text())"/>
        </TRSPort>
        <DischargePort>
          <xsl:value-of select="StringHelper:ToUpper($lastSeaLeg/s0:PortOfDischarge/s0:Code/text())"/>
        </DischargePort>
        <ColoaderFlag>
          <xsl:variable name="subShipmentIsCoLoad">
            <xsl:for-each select="s0:SubShipmentCollection/s0:SubShipment/s0:ShipmentType">
              <IsCoLoad>
                <xsl:value-of select="OCMHelper:IsCoLoad(s0:Code/text())"/>
              </IsCoLoad>
            </xsl:for-each>
          </xsl:variable>
          <xsl:choose>
            <xsl:when test="$shipmentType = 'DRT' and count(msxsl:node-set($subShipmentIsCoLoad)/IsCoLoad[./text() = 'FALSE']) > 0">N</xsl:when>
            <xsl:otherwise>Y</xsl:otherwise>
          </xsl:choose>
        </ColoaderFlag>

        <xsl:call-template name="Remarks">
          <xsl:with-param name="text" select="s0:NoteCollection/s0:Note[s0:Description ='Goods Handling Instructions']/s0:NoteText/text()"/>
        </xsl:call-template>
        <xsl:call-template name="Remarks">
          <xsl:with-param name="text" select="s0:NoteCollection/s0:Note[s0:Description ='Forwarding Instruction Notes']/s0:NoteText/text()"/>
        </xsl:call-template>
        <xsl:call-template name="Remarks">
          <xsl:with-param name="text" select="s0:NoteCollection/s0:Note[s0:Description ='Special Instructions']/s0:NoteText/text()"/>
        </xsl:call-template>

        <xsl:if test="$customerEmail!=''">
          <xsl:call-template name="Remarks">
            <xsl:with-param name="text" select="concat('CustomerEmail: ', $customerEmail)"/>
          </xsl:call-template>
        </xsl:if>

        <xsl:variable name="deliveryMode" select="s0:ContainerCollection/s0:Container[1]/s0:DeliveryMode/text()"/>
        <TransportType>
          <xsl:choose>
            <xsl:when test="$deliveryMode = 'CY/CY'">1</xsl:when>
            <xsl:when test="$deliveryMode = 'CFS/CY'">2</xsl:when>
            <xsl:when test="$deliveryMode = 'CY/CFS'">3</xsl:when>
            <xsl:when test="$deliveryMode = 'CFS/CFS'">4</xsl:when>
          </xsl:choose>
        </TransportType>
        <xsl:variable name="freightCharge" select="s0:PaymentHandlingInstructionCollection/s0:PaymentHandlingInstruction[s0:Category/s0:Code/text()='FRT']"/>
        <FreightPaymentIndicator>
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
        </FreightPaymentIndicator>
        <PickupFlag>
          <xsl:choose>
            <xsl:when test="starts-with($deliveryMode,'CFS')">Y</xsl:when>
            <xsl:otherwise>N</xsl:otherwise>
          </xsl:choose>
        </PickupFlag>

        <xsl:if test="starts-with($deliveryMode,'CFS')">
          <PickupDetails>
            <xsl:if test="s0:ContainerCollection/s0:Container[starts-with(s0:DeliveryMode[1]/text(),'CFS')]">
              <xsl:variable name="pickupAddress" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType='ConsignorPickupDeliveryAddress']"/>
              <CompanyName>
                <xsl:value-of select="$pickupAddress/s0:CompanyName/text()"/>
              </CompanyName>
              <xsl:call-template name="PickupAddress">
                <xsl:with-param name="text" select="normalize-space($pickupAddress/s0:Address1/text())"/>
              </xsl:call-template>
              <xsl:call-template name="PickupAddress">
                <xsl:with-param name="text" select="normalize-space($pickupAddress/s0:Address2/text())"/>
              </xsl:call-template>
              <City>
                <xsl:value-of select="$pickupAddress/s0:City/text()"/>
              </City>
              <PostalCode>
                <xsl:value-of select="$pickupAddress/s0:Postcode/text()"/>
              </PostalCode>
              <StateProvince>
                <xsl:value-of select="$pickupAddress/s0:State/text()"/>
              </StateProvince>
              <Country>
                <xsl:value-of select="StringHelper:ToUpper($pickupAddress/s0:Country/s0:Code/text())"/>
              </Country>
              <Contact>
                <xsl:value-of select="$pickupAddress/s0:Contact/text()"/>
              </Contact>
              <Phone>
                <xsl:value-of select="$pickupAddress/s0:Phone/text()"/>
              </Phone>
              <Email>
                <xsl:value-of select="$pickupAddress/s0:Email/text()"/>
              </Email>
            </xsl:if>

            <xsl:variable name="pickupDate" select="s0:DateCollection/s0:Date[s0:Type/text()='Pickup']/s0:Value/text()" />
            <xsl:variable name="departureEstimatedPickup" select="s0:ContainerCollection/s0:Container/s0:DepartureEstimatedPickup/text()" />

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
            <Date>
              <xsl:value-of select="DateMapper:FormatXmlDateTime($pickupDateTime, 'yyyy-MM-dd')"/>
            </Date>
            <Time>
              <xsl:value-of select="DateMapper:FormatXmlDateTime($pickupDateTime, 'HH:mm:ss')"/>
            </Time>
            <LoadingReference>
              <xsl:value-of select="s0:NoteCollection/s0:Note[s0:Description='Export Pickup Instructions']/s0:NoteText/text()"/>
            </LoadingReference>
          </PickupDetails>
        </xsl:if>

        <xsl:variable name="mainLeg" select="s0:TransportLegCollection/s0:TransportLeg[s0:LegType='Main']"/>
        <xsl:if test="$mainLeg">
          <SailingDetails>
            <VesselVoyageID></VesselVoyageID>
            <VesselName>
              <xsl:value-of select="$mainLeg/s0:VesselName/text()"/>
            </VesselName>
            <IMONumber>
              <xsl:value-of select="$mainLeg/s0:VesselLloydsIMO/text()"/>
            </IMONumber>
            <Voyage>
              <xsl:value-of select="$mainLeg/s0:VoyageFlightNo/text()"/>
            </Voyage>

            <xsl:variable name="earliestDeparture" select="s0:DateCollection/s0:Date[s0:Type/text()='EarliestDeparture']/s0:Value/text()" />
            <xsl:variable name="etd">
              <xsl:choose>
                <xsl:when test="$earliestDeparture!=''">
                  <xsl:value-of select="$earliestDeparture"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$firstSeaLeg/s0:EstimatedDeparture/text()"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <ETD>
              <xsl:value-of select="DateMapper:ConvertToDate($etd, 'yyyy-MM-dd')"/>
            </ETD>

            <xsl:variable name="latestDelivery" select="s0:DateCollection/s0:Date[s0:Type/text()='LatestDelivery']/s0:Value/text()" />
            <xsl:variable name="eta">
              <xsl:choose>
                <xsl:when test="$latestDelivery!=''">
                  <xsl:value-of select="$latestDelivery"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$lastSeaLeg/s0:EstimatedArrival/text()"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <ETA>
              <xsl:value-of select="DateMapper:ConvertToDate($eta, 'yyyy-MM-dd')"/>
            </ETA>

            <CarrierSCAC>
              <xsl:value-of select="$mainLeg/s0:Carrier/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text()='CCC' and s0:CountryOfIssue/s0:Code/text()='US']/s0:Value/text()"/>
            </CarrierSCAC>
          </SailingDetails>
        </xsl:if>

        <xsl:if test="s0:PackingLineCollection">
          <xsl:call-template name="CargoDetails">
            <xsl:with-param name="groupingMethod" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='GroupingMethod']/s0:Value/text()"/>
            <xsl:with-param name="segmentPackingLineCollection" select="s0:PackingLineCollection"/>
          </xsl:call-template>
        </xsl:if>

        <xsl:if test="s0:ContainerMode/s0:Code/text()='FCL'">
          <xsl:for-each select="s0:ContainerCollection/s0:Container">
            <ContainerDetails>
              <ContainerNumber>
                <xsl:value-of select="s0:ContainerNumber/text()"/>
              </ContainerNumber>
              <ContainerType>
                <xsl:variable name="ProviderSpecificISOCode" select="CodeMapper:GetRecipientCode('VANGUARD' , 'VANGUARD', 'VANGUARD Provider Configuration', 'ContainerTypeToISOCode' , 'VANGUARD Code', s0:ContainerType/s0:ISOCode)" />
                <xsl:choose>
                  <xsl:when test="$ProviderSpecificISOCode!=''">
                    <xsl:value-of select="$ProviderSpecificISOCode"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:variable name="ShippingInstructionISOCode" select="CodeMapper:GetRecipientCode('SHIPPING_INSTRUCTION' , 'SHIPPING_INSTRUCTION' , 'OCM System Configuration' , 'ContainerTypeToISOCode' , 'Carrier Code', s0:ContainerType/s0:ISOCode)" />
                    <xsl:choose>
                      <xsl:when test="$ShippingInstructionISOCode!=''">
                        <xsl:value-of select="$ShippingInstructionISOCode"/>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="s0:ContainerType/s0:ISOCode"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:otherwise>
                </xsl:choose>
              </ContainerType>
            </ContainerDetails>
          </xsl:for-each>
        </xsl:if>
        <xsl:for-each select="s0:OrganizationAddressCollection/s0:OrganizationAddress">
          <xsl:variable name="partyType">
            <xsl:choose>
              <xsl:when test="s0:AddressType='BookingPartyDocumentaryAddress'">FW</xsl:when>
              <xsl:when test="s0:AddressType='ConsignorDocumentaryAddress'">SH</xsl:when>
              <xsl:when test="s0:AddressType='ConsigneeDocumentaryAddress'">CN</xsl:when>
              <xsl:when test="s0:AddressType='NotifyParty'">N1</xsl:when>
              <xsl:when test="s0:AddressType='NotifyParty2'">N2</xsl:when>
              <xsl:otherwise></xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <xsl:variable name="companyName" select="s0:CompanyName/text()" />
          <xsl:if test="$partyType!='' and $companyName!=''">
            <xsl:call-template name="Parties">
              <xsl:with-param name="orgAddr" select="."/>
              <xsl:with-param name="partyType" select="$partyType"/>
              <xsl:with-param name="companyName" select="$companyName"/>
            </xsl:call-template>
          </xsl:if>
        </xsl:for-each>
      </BookingDetails>
    </ns0:BookingRequest>
  </xsl:template>

  <xsl:template name="CargoDetails">
    <xsl:param name="groupingMethod"/>
    <xsl:param name="segmentPackingLineCollection"/>

    <xsl:choose>
      <xsl:when test="$groupingMethod != ''">
        <xsl:call-template name="CargoDetailsByGroupingMethod">
          <xsl:with-param name="packingLineCollection" select="$segmentPackingLineCollection" />
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:variable name="isSummary"  select="CodeMapper:GetRecipientCode('SHIPPING_INSTRUCTION' , 'SHIPPING_INSTRUCTION' , 'OCM System Configuration', 'Cargo Details Format', 'Is Summary', $SenderID, 'VANGUARD') " />
        <xsl:choose>
          <xsl:when test="StringHelper:ToUpper($isSummary)='TRUE'">
            <xsl:call-template name="CargoDetailsSummary">
              <xsl:with-param name="packingLineCollection" select="$segmentPackingLineCollection" />
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name="CargoDetailsDetails">
              <xsl:with-param name="packingLineCollection" select="$segmentPackingLineCollection" />
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="CargoDetailsByGroupingMethod">
    <xsl:param name="packingLineCollection" />

    <xsl:for-each select="$packingLineCollection/s0:PackingLine">
      <CargoDetails>
        <xsl:call-template name="ShippingMarks">
          <xsl:with-param name="text" select="s0:MarksAndNos/text()"/>
        </xsl:call-template>
        <Pieces>
          <xsl:value-of select="s0:PackQty/text()"/>
        </Pieces>
        <Packaging>
          <xsl:value-of select="substring(s0:PackType/s0:Description/text(), 1, 20)"/>
        </Packaging>
        <xsl:call-template name="Commodity">
          <xsl:with-param name="text" select="s0:DetailedDescription/text()"/>
        </xsl:call-template>
        <Weight>
          <xsl:value-of select="s0:Weight/text()"/>
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
        <xsl:choose>
          <xsl:when test="s0:PackingLineCollection/s0:PackingLine/s0:UNDGCollection!=''">
            <HazardousFlag>Y</HazardousFlag>
            <xsl:for-each select="s0:PackingLineCollection/s0:PackingLine/s0:UNDGCollection/s0:UNDG[s0:UNDGCode/text()!='']">
              <xsl:call-template name="HazardousDetails-Core">
                <xsl:with-param name="undgNode" select="."/>
              </xsl:call-template>
            </xsl:for-each>
          </xsl:when>
          <xsl:otherwise>
            <HazardousFlag>N</HazardousFlag>
          </xsl:otherwise>
        </xsl:choose>
      </CargoDetails>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="CargoDetailsDetails">
    <xsl:param name="packingLineCollection" />

    <xsl:for-each select="$packingLineCollection/s0:PackingLine">
      <CargoDetails>
        <xsl:call-template name="ShippingMarks">
          <xsl:with-param name="text" select="s0:MarksAndNos/text()"/>
        </xsl:call-template>
        <Pieces>
          <xsl:value-of select="s0:PackQty/text()"/>
        </Pieces>
        <Packaging>
          <xsl:value-of select="substring(s0:PackType/s0:Description/text(), 1, 20)"/>
        </Packaging>
        <xsl:call-template name="Commodity">
          <xsl:with-param name="text" select="s0:DetailedDescription/text()"/>
        </xsl:call-template>
        <Weight>
          <xsl:value-of select="s0:Weight/text()"/>
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
        <xsl:choose>
          <xsl:when test="s0:UNDGCollection!=''">
            <HazardousFlag>Y</HazardousFlag>
            <xsl:for-each select="s0:UNDGCollection/s0:UNDG[s0:UNDGCode/text()!='']">
              <xsl:call-template name="HazardousDetails-Core">
                <xsl:with-param name="undgNode" select="."/>
              </xsl:call-template>
            </xsl:for-each>
          </xsl:when>
          <xsl:otherwise>
            <HazardousFlag>N</HazardousFlag>
          </xsl:otherwise>
        </xsl:choose>
      </CargoDetails>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="CargoDetailsSummary">
    <xsl:param name="packingLineCollection" />

    <xsl:for-each select="$packingLineCollection/s0:PackingLine">
      <xsl:variable name="cargoDetailsKey" select="concat(s0:DetailedDescription/text(), '+', s0:PackType/s0:Code/text(), '+', s0:MarksAndNos/text())" />
      <xsl:variable name="shouldCreateCargoDetails" select="ListHelper:ShouldCreateItem('CD+', $cargoDetailsKey)" />

      <xsl:if test="$shouldCreateCargoDetails='true'">
        <xsl:variable name="matchedPackingLines" select="$packingLineCollection/s0:PackingLine[concat(s0:DetailedDescription/text(), '+', s0:PackType/s0:Code/text(), '+', s0:MarksAndNos/text())=$cargoDetailsKey]" />
        <xsl:variable name="totalPackQty"  select="sum($matchedPackingLines/s0:PackQty/text())" />
        <xsl:variable name="totalWeight"  select="format-number(sum($matchedPackingLines/s0:Weight/text()), '0.###')" />
        <xsl:variable name="totalVolume"  select="format-number(sum($matchedPackingLines/s0:Volume/text()), '0.###')" />

        <CargoDetails>
          <xsl:call-template name="ShippingMarks">
            <xsl:with-param name="text" select="s0:MarksAndNos/text()"/>
          </xsl:call-template>
          <Pieces>
            <xsl:value-of select="$totalPackQty"/>
          </Pieces>
          <Packaging>
            <xsl:value-of select="substring(s0:PackType/s0:Description/text(), 1, 20)"/>
          </Packaging>
          <xsl:call-template name="Commodity">
            <xsl:with-param name="text" select="s0:DetailedDescription/text()"/>
          </xsl:call-template>
          <Weight>
            <xsl:value-of select="$totalWeight"/>
          </Weight>
          <Volume>
            <xsl:value-of select="$totalVolume"/>
          </Volume>
          <UOM>
            <xsl:choose>
              <xsl:when test="s0:WeightUnit/s0:Code/text() = 'LB'">E</xsl:when>
              <xsl:otherwise>M</xsl:otherwise>
            </xsl:choose>
          </UOM>
          <xsl:choose>
            <xsl:when test="$matchedPackingLines/s0:UNDGCollection!=''">
              <HazardousFlag>Y</HazardousFlag>
              <xsl:for-each select="$matchedPackingLines/s0:UNDGCollection/s0:UNDG[s0:UNDGCode/text()!='']">
                <xsl:variable name="hazardousDetailsKey" select="concat(s0:IMOClass,'+',s0:FlashPoint,'+',s0:ProperShippingName,'+',s0:UNDGCode,'+',s0:PackingGroup,'+',s0:Contact/s0:FullName,'+',s0:Contact/s0:Phone,'+',s0:TechicalName,'+',s0:PackType/s0:Code,'+',s0:PackedInLimitedQuantity)" />
                <xsl:variable name="shouldCreateHazardousDetails" select="ListHelper:ShouldCreateItem('HD+', concat($cargoDetailsKey, $hazardousDetailsKey))" />

                <xsl:if test="$shouldCreateHazardousDetails">
                  <xsl:call-template name="HazardousDetails-Core">
                    <xsl:with-param name="undgNode" select="."/>
                  </xsl:call-template>
                </xsl:if>
              </xsl:for-each>
            </xsl:when>
            <xsl:otherwise>
              <HazardousFlag>N</HazardousFlag>
            </xsl:otherwise>
          </xsl:choose>
        </CargoDetails>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="HazardousDetails-Core">
    <xsl:param name="undgNode"/>

    <HazardousDetails>
      <HazardousClass>
        <xsl:value-of select="$undgNode/s0:IMOClass/text()"/>
      </HazardousClass>
      <xsl:variable name="flashPoint" select="$undgNode/s0:FlashPoint/text()"/>
      <xsl:if test="$flashPoint != '' and number($flashPoint) != 0">
        <Flashpoint>
          <xsl:value-of select="$flashPoint"/>
        </Flashpoint>
        <FlashpointUOM>C</FlashpointUOM>
      </xsl:if>
      <ShippingName>
        <xsl:value-of select="$undgNode/s0:ProperShippingName/text()"/>
      </ShippingName>
      <UNNumber>
        <xsl:value-of select="substring($undgNode/s0:UNDGCode/text(), 1, 4)"/>
      </UNNumber>
      <PackingGroup>
        <xsl:value-of select="$undgNode/s0:PackingGroup/text()"/>
      </PackingGroup>
      <EmergencyContact>
        <xsl:value-of select="$undgNode/s0:Contact/s0:FullName/text()"/>
      </EmergencyContact>
      <EmergencyPhoneArea></EmergencyPhoneArea>
      <EmergencyPhone>
        <xsl:value-of select="$undgNode/s0:Contact/s0:Phone/text()"/>
      </EmergencyPhone>
      <Comments>
        <xsl:value-of select="$undgNode/s0:TechicalName/text()"/>
      </Comments>
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
    <xsl:if test="$text!=''">
      <xsl:variable name="lineLength" select="userCSharp:GetLengthForWrapping($text, 40)"/>
      <xsl:variable name="RemarksText" select="substring($text, 1, $lineLength)"/>
      <xsl:variable name="RemarksTextNorm" select="normalize-space($RemarksText)"/>
      <xsl:variable name="remainingText" select="substring($text, $lineLength+1)"/>
      <xsl:choose>
        <xsl:when test="$RemarksTextNorm!=''">
          <xsl:element name="Remarks">
            <xsl:value-of select="$RemarksTextNorm"/>
          </xsl:element>
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

  <xsl:template name="PickupAddress">
    <xsl:param name="text"/>
    <xsl:call-template name="PickupAddress-Core">
      <xsl:with-param name="text" select="userCSharp:ReplaceCRLFText($text)"/>
      <xsl:with-param name="segmentCount" select="1"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="PickupAddress-Core">
    <xsl:param name="text"/>
    <xsl:param name="segmentCount"/>
    <xsl:if test="$text!=''">
      <xsl:variable name="lineLength" select="userCSharp:GetLengthForWrapping($text, 35)"/>
      <xsl:variable name="PickupAddressText" select="substring($text, 1, $lineLength)"/>
      <xsl:variable name="PickupAddressNorm" select="normalize-space($PickupAddressText)"/>
      <xsl:variable name="remainingText" select="substring($text, $lineLength+1)"/>
      <xsl:choose>
        <xsl:when test="$PickupAddressNorm!=''">
          <xsl:element name="Address">
            <xsl:value-of select="$PickupAddressNorm"/>
          </xsl:element>
          <xsl:call-template name="PickupAddress-Core">
            <xsl:with-param name="text" select="$remainingText"/>
            <xsl:with-param name="segmentCount" select="$segmentCount+1"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="PickupAddress-Core">
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
    <xsl:if test="$text!=''">
      <xsl:variable name="lineLength" select="userCSharp:GetLengthForWrapping($text, 35)"/>
      <xsl:variable name="ShippingMarksText" select="substring($text, 1, $lineLength)"/>
      <xsl:variable name="ShippingMarksTextNorm" select="normalize-space($ShippingMarksText)"/>
      <xsl:variable name="remainingText" select="substring($text, $lineLength+1)"/>
      <xsl:choose>
        <xsl:when test="$ShippingMarksTextNorm!=''">
          <xsl:element name="ShippingMarks">
            <xsl:value-of select="$ShippingMarksTextNorm"/>
          </xsl:element>
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

  <xsl:template name="Parties">
    <xsl:param name="orgAddr"/>
    <xsl:param name="partyType"/>
    <xsl:param name="companyName"/>

    <xsl:element name="Parties">
      <xsl:element name="PartyType">
        <xsl:value-of select="$partyType"/>
      </xsl:element>
      <xsl:element name="PartyID"/>
      <xsl:element name="PartyName">
        <xsl:value-of select="$companyName"/>
      </xsl:element>
      <xsl:call-template name="PartiesAddress">
        <xsl:with-param name="text" select="$orgAddr/s0:Address1/text()"/>
      </xsl:call-template>
      <xsl:call-template name="PartiesAddress">
        <xsl:with-param name="text" select="$orgAddr/s0:Address2/text()"/>
      </xsl:call-template>
      <xsl:element name="City">
        <xsl:value-of select="$orgAddr/s0:City/text()"/>
      </xsl:element>
      <xsl:element name="PostalCode">
        <xsl:value-of select="$orgAddr/s0:Postcode/text()"/>
      </xsl:element>
      <xsl:element name="StateProvince">
        <xsl:value-of select="$orgAddr/s0:State/text()"/>
      </xsl:element>
      <xsl:element name="Country">
        <xsl:value-of select="StringHelper:ToUpper($orgAddr/s0:Country/s0:Code/text())"/>
      </xsl:element>
      <xsl:element name="UNLocCode">
        <xsl:value-of select="StringHelper:ToUpper($orgAddr/s0:Port/s0:Code/text())"/>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template name="PartiesAddress">
    <xsl:param name="text"/>

    <xsl:call-template name="PartiesAddress-Core">
      <xsl:with-param name="text" select="userCSharp:ReplaceCRLFText($text)"/>
      <xsl:with-param name="segmentCount" select="1"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="PartiesAddress-Core">
    <xsl:param name="text"/>
    <xsl:param name="segmentCount"/>
    <xsl:if test="$text!='' and number($segmentCount)&lt;=5">
      <xsl:variable name="lineLength" select="userCSharp:GetLengthForWrapping($text, 35)"/>
      <xsl:variable name="PartiesAddressText" select="substring($text, 1, $lineLength)"/>
      <xsl:variable name="PartiesAddressTextNorm" select="normalize-space($PartiesAddressText)"/>
      <xsl:variable name="remainingText" select="substring($text, $lineLength+1)"/>
      <xsl:choose>
        <xsl:when test="$PartiesAddressTextNorm!=''">
          <xsl:element name="Address">
            <xsl:value-of select="$PartiesAddressTextNorm"/>
          </xsl:element>
          <xsl:call-template name="PartiesAddress-Core">
            <xsl:with-param name="text" select="$remainingText"/>
            <xsl:with-param name="segmentCount" select="$segmentCount+1"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="PartiesAddress-Core">
            <xsl:with-param name="text" select="$remainingText"/>
            <xsl:with-param name="segmentCount" select="$segmentCount"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
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
    <xsl:if test="$text!='' and number($segmentCount)&lt;=35">
      <xsl:variable name="lineLength" select="userCSharp:GetLengthForWrapping($text, 35)"/>
      <xsl:variable name="CommodityText" select="substring($text, 1, $lineLength)"/>
      <xsl:variable name="CommodityTextNorm" select="normalize-space($CommodityText)"/>
      <xsl:variable name="remainingText" select="substring($text, $lineLength+1)"/>
      <xsl:choose>
        <xsl:when test="$CommodityTextNorm!=''">
          <xsl:element name="Commodity">
            <xsl:value-of select="$CommodityTextNorm"/>
          </xsl:element>
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
public string ToUpper(string input)
{
  string result = "";
  if (input != null)
  {
    result = input;
  }
  return result.ToUpper();
}

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

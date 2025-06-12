<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 CodeMapper ContextAccessor DataModelAccessor DateMapper UnitConverter userCSharp"
                version="1.0"
                xmlns:ns0="urn:PCM"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2012/11/NotificationOfDangerousGoods/1"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
                xmlns:DataModelAccessor="http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"
                xmlns:UnitConverter="http://schemas.microsoft.com/BizTalk/2003/UnitConverter"
                xmlns:DateMapper="http://schemas.microsoft.com/BizTalk/2003/DateMapper"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes" />

  <xsl:variable name="senderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="recipientID" select="ContextAccessor:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>

  <xsl:variable name="serviceProvider" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE', 'FORWARDING_PORT_MESSAGE', 'FPM System Configuration', 'Port Settings', 'Name', $recipientID)"/>
  <xsl:variable name="serviceProviderMSGID" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE', 'FORWARDING_PORT_MESSAGE', 'FPM System Configuration', 'Port Settings', 'MSGID', $recipientID)"/>
  <xsl:variable name="serviceProviderID" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE', 'FORWARDING_PORT_MESSAGE', 'FPM System Configuration', 'Port Settings', 'ID', $recipientID)"/>

  <xsl:variable name="interchangeID" select="CodeMapper:CallActionProcedureHelper('GetCounterValue','@value','@name','CargoWise.eHub.Products.ForwardingPortMessaging.BE.CPOINT.Interchange','@maxlength','50')"/>

  <xsl:variable name="InboxPK" select="ContextAccessor:GetContextProperty('InternalTrackingID', 'http://cargowise.com/ehub/tracking/2010/06')"/>
  <xsl:variable name="SubscribeInboxPK" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $InboxPK, $interchangeID)"/>
  <xsl:variable name="OverrideFilename" select="ContextAccessor:SetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06', concat('IFTDGN_', $senderID, '_', $interchangeID))"/>

  <xsl:template match="/">
    <xsl:apply-templates select="s0:UniversalShipment/s0:Shipment"/>
  </xsl:template>

  <xsl:template match="s0:UniversalShipment/s0:Shipment">
    <xsl:variable name="consolID" select="s0:DataContext/s0:DataSource/s0:Key/text()"/>
    <xsl:variable name="documentName" select="s0:DataContext/s0:DocumentaryOverride/s0:DocumentName/text()"/>
    <xsl:variable name="documentNameType" select="normalize-space(substring-after($documentName,'-'))"/>
    <xsl:variable name="purpose" select="userCSharp:ToUpper(normalize-space(s0:DataContext/s0:DocumentaryOverride/s0:Purpose/text()))"/>

    <xsl:variable name="operationPortCode" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='OperationalPort_Code']/s0:Value/text()"/>
    <xsl:variable name="mainVesselTypeCode" select="userCSharp:ToUpper(normalize-space(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='Main_VesselType_Code']/s0:Value/text()))"/>
    <xsl:variable name="ReasonForMessage">
      <xsl:choose>
        <xsl:when test="$purpose='AMD'">
          <xsl:value-of select="s0:NoteCollection/s0:Note[s0:Description/text()='ReasonForMessageAmendmentFreeText']/s0:NoteText/text()"/>
        </xsl:when>
        <xsl:when test="$purpose='WTH'">
          <xsl:value-of select="s0:NoteCollection/s0:Note[s0:Description/text()='ReasonForMessageCancellationFreeText']/s0:NoteText/text()"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="QualifierReasonForMessage">
      <xsl:choose>
        <xsl:when test="$purpose='AMD'">
          <xsl:value-of select="s0:NoteCollection/s0:Note[s0:Description/text()='ReasonForMessageAmendment']/s0:NoteText/text()"/>
        </xsl:when>
        <xsl:when test="$purpose='WTH'">
          <xsl:value-of select="s0:NoteCollection/s0:Note[s0:Description/text()='ReasonForMessageCancellation']/s0:NoteText/text()"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="shipment" select="." />

    <xsl:variable name="forwarderAddressType">
      <xsl:choose>
        <xsl:when test="$documentNameType='Import'">ReceivingForwarderAddress</xsl:when>
        <xsl:when test="$documentNameType='Export'">SendingForwarderAddress</xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="forwarderAddress" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()=$forwarderAddressType]"/>
    <xsl:variable name="forwarderAddressPSNValue">
      <xsl:call-template name="GetPSNNumber">
        <xsl:with-param name="orgAddress" select="$forwarderAddress" />
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="transportLegType">
      <xsl:choose>
        <xsl:when test="$documentNameType='Import'">OnForwarding</xsl:when>
        <xsl:when test="$documentNameType='Export'">PreCarriage</xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="transportLeg" select="s0:TransportLegCollection/s0:TransportLeg[s0:LegType/text()=$transportLegType]" />

    <xsl:variable name="messageIdentifier">
      <xsl:variable name="formattedMessageID" select ="concat(substring($forwarderAddressPSNValue, 1, 6), format-number($interchangeID, '000000000'), '01')"/>
      <xsl:variable name="subscribeJobNumber1" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $formattedMessageID, $consolID, 'IFTDGN')"/>
      <xsl:variable name="subscribeJobNumber2" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $consolID, $formattedMessageID, 'IFTDGN')"/>
      <xsl:value-of select="$formattedMessageID"/>
    </xsl:variable>

    <xsl:variable name="OverrideEmailSubject" select="ContextAccessor:SetContextProperty('OverrideEmailSubject', 'http://cargowise.com/ehub/processing/2010/06', concat('FPM_', $serviceProvider, '_', $operationPortCode))"/>
    <xsl:variable name="fileSenderID" select="DataModelAccessor:GetClientRegistrationCode($senderID, s0:DataContext/s0:Workflow/s0:EventBranch/text(), $serviceProvider)"/>

    <xsl:variable name="ShipmentTypeSubscription" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageIdentifier, s0:DataContext/s0:DataSource/s0:Type/text(), 'ForwardingType')" />
    <xsl:variable name="SubscribeActionPurpose" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageIdentifier, $purpose, 'Purpose')" />
    <xsl:variable name="SubscribeDocumentName" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageIdentifier, $documentName, 'DocumentName')" />
    <xsl:variable name="SubscribeOperationPort" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageIdentifier, $operationPortCode, 'OperationPort')" />
    <xsl:variable name="SubscribeClientID" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderID, $serviceProvider, $senderID, $fileSenderID)" />

    <ns0:IFTDGN VersionMajor="2" VersionMinor="0">
      <ns0:MessageHeader>
        <ns0:MessageIdentifier>
          <ns0:MessageTypeIdentifier>IFTDGN</ns0:MessageTypeIdentifier>
          <ns0:MessageTypeVersionNumber>D</ns0:MessageTypeVersionNumber>
          <ns0:MessageTypeReleaseNumber>03A</ns0:MessageTypeReleaseNumber>
          <ns0:ControllingAgency>UN</ns0:ControllingAgency>
          <ns0:AssociationAssignedCode>PROT20</ns0:AssociationAssignedCode>
        </ns0:MessageIdentifier>
      </ns0:MessageHeader>
      <ns0:BeginningOfMessage>
        <ns0:DocumentMessageName>
          <ns0:DocumentNameCode>
            <xsl:choose>
              <xsl:when test="$mainVesselTypeCode='LNG' or $mainVesselTypeCode='OIL' or $mainVesselTypeCode='TNK'">89T</xsl:when>
              <xsl:otherwise>89N</xsl:otherwise>
            </xsl:choose>
          </ns0:DocumentNameCode>
        </ns0:DocumentMessageName>
        <ns0:DocumentMessageIdentification>
          <ns0:DocumentIdentifier>
            <xsl:value-of select="$messageIdentifier"/>
          </ns0:DocumentIdentifier>
        </ns0:DocumentMessageIdentification>
        <ns0:MessageFunctionCode>
          <xsl:choose>
            <xsl:when test="$purpose='ORG'">9</xsl:when>
            <xsl:when test="$purpose='AMD'">5</xsl:when>
            <xsl:when test="$purpose='WTH'">1</xsl:when>
          </xsl:choose>
        </ns0:MessageFunctionCode>
      </ns0:BeginningOfMessage>

      <xsl:call-template name="CreateDateTimePeriod">
        <xsl:with-param name="dateTimeQualifier" select="'137'" />
        <xsl:with-param name="dateTimeValue" select="s0:DataContext/s0:Workflow/s0:TriggerDate/text()" />
      </xsl:call-template>

      <xsl:call-template name="CreateFreeText">
        <xsl:with-param name="codeQualifier" select="'CHG'" />
        <xsl:with-param name="textReference" select="$QualifierReasonForMessage"/>
        <xsl:with-param name="textValue" select="$ReasonForMessage"/>
      </xsl:call-template>

      <xsl:if test="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='HandlingInstruction']/s0:Value/text()!=''">
        <ns0:HandlingInstructions>
          <ns0:HandlingInstructionDescriptionCode>
            <xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='HandlingInstruction']/s0:Value/text()"/>
          </ns0:HandlingInstructionDescriptionCode>
        </ns0:HandlingInstructions>
      </xsl:if>

      <xsl:call-template name ="CreateGroupReference">
        <xsl:with-param name="elementName" select="'GroupReference'"/>
        <xsl:with-param name="codeQualifier" select="'FF'"/>
        <xsl:with-param name="identifier" select="s0:AdditionalReferenceCollection/s0:AdditionalReference[s0:Type/text()='FFW']/s0:ReferenceNumber/text()"/>
      </xsl:call-template>

      <xsl:if test="$purpose='AMD' or $purpose='WTH'">
        <xsl:call-template name ="CreateGroupReference">
          <xsl:with-param name="codeQualifier" select="'ALG'"/>
          <xsl:with-param name="identifier" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='DgnSecurityNumber']/s0:Value/text()"/>
        </xsl:call-template>
      </xsl:if>

      <ns0:GroupDetailsOfTransport>
        <xsl:variable name="transportLegMain" select="s0:TransportLegCollection/s0:TransportLeg[s0:LegType/text()='Main']"/>
        <xsl:if test="$transportLegMain">
          <ns0:DetailsOfTransport>
            <ns0:TransportStageCodeQualifier>20</ns0:TransportStageCodeQualifier>
            <ns0:MeansOfTransportJourneyIdentifier>
              <xsl:value-of select="$transportLegMain/s0:VoyageFlightNo/text()"/>
            </ns0:MeansOfTransportJourneyIdentifier>
            <ns0:ModeOfTransport>
              <ns0:TransportModeNameCode>
                <xsl:choose>
                  <xsl:when test="$mainVesselTypeCode='BA'">8</xsl:when>
                  <xsl:otherwise>1</xsl:otherwise>
                </xsl:choose>
              </ns0:TransportModeNameCode>
            </ns0:ModeOfTransport>
            <ns0:Carrier>
              <ns0:CarrierIdentifier>
                <xsl:call-template name="GetPSNNumber">
                  <xsl:with-param name="orgAddress" select="$transportLegMain/s0:Carrier" />
                </xsl:call-template>
              </ns0:CarrierIdentifier>
            </ns0:Carrier>
            <ns0:TransportIdentification>
              <ns0:TransportMeansIdentificationNameIdentifier>
                <xsl:choose>
                  <xsl:when test="$transportLegMain/s0:VesselLloydsIMO/text()!=''">
                    <xsl:value-of select="$transportLegMain/s0:VesselLloydsIMO/text()"/>
                  </xsl:when>
                  <xsl:otherwise>TBN</xsl:otherwise>
                </xsl:choose>
              </ns0:TransportMeansIdentificationNameIdentifier>
              <ns0:TransportMeansIdentificationName>
                <xsl:choose>
                  <xsl:when test="$transportLegMain/s0:VesselName/text()!=''">
                    <xsl:value-of select="$transportLegMain/s0:VesselName/text()"/>
                  </xsl:when>
                  <xsl:otherwise>TBN</xsl:otherwise>
                </xsl:choose>
              </ns0:TransportMeansIdentificationName>
              <ns0:TransportMeansNationalityCode>
                <xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='Main_Vessel_CountryCode']/s0:Value/text()"/>
              </ns0:TransportMeansNationalityCode>
            </ns0:TransportIdentification>
          </ns0:DetailsOfTransport>
        </xsl:if>

        <xsl:call-template name ="CreateReference">
          <xsl:with-param name="codeQualifier" select="'ATZ'"/>
          <xsl:with-param name="identifier" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='VesselStayReference']/s0:Value/text()"/>
        </xsl:call-template>

        <xsl:call-template name ="CreateReference">
          <xsl:with-param name="codeQualifier" select="'VM'"/>
          <xsl:with-param name="identifier" select="$transportLegMain/s0:VesselLloydsIMO/text()"/>
        </xsl:call-template>

        <ns0:GroupPlaceLocationIdentification>
          <ns0:PlaceLocationIdentification>
            <ns0:LocationFunctionCodeQualifier>153</ns0:LocationFunctionCodeQualifier>
            <ns0:LocationIdentification>
              <ns0:LocationNameCode>
                <xsl:value-of select="$operationPortCode"/>
              </ns0:LocationNameCode>
            </ns0:LocationIdentification>
          </ns0:PlaceLocationIdentification>

          <xsl:call-template name="CreateDateTimePeriod">
            <xsl:with-param name="dateTimeQualifier" select="'132'" />
            <xsl:with-param name="dateTimeValue" select="s0:DateCollection/s0:Date[s0:Type/text()='VesselStayStartDate']/s0:Value/text()" />
          </xsl:call-template>

          <xsl:call-template name="CreateDateTimePeriod">
            <xsl:with-param name="dateTimeQualifier" select="'133'" />
            <xsl:with-param name="dateTimeValue" select="s0:DateCollection/s0:Date[s0:Type/text()='VesselStayEndDate']/s0:Value/text()" />
          </xsl:call-template>
        </ns0:GroupPlaceLocationIdentification>
      </ns0:GroupDetailsOfTransport>

      <ns0:GroupNameAndAddress>
        <ns0:NameAndAddress>
          <ns0:PartyFunctionCodeQualifier>CG</ns0:PartyFunctionCodeQualifier>
          <ns0:PartyIdentificationDetails>
            <ns0:PartyIdentifier>
              <xsl:call-template name="GetPSNNumber">
                <xsl:with-param name="orgAddress" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='ShippingLineAddress']" />
              </xsl:call-template>
            </ns0:PartyIdentifier>
          </ns0:PartyIdentificationDetails>
        </ns0:NameAndAddress>
      </ns0:GroupNameAndAddress>

      <ns0:GroupNameAndAddress>
        <ns0:NameAndAddress>
          <ns0:PartyFunctionCodeQualifier>MR</ns0:PartyFunctionCodeQualifier>
          <ns0:PartyIdentificationDetails>
            <ns0:PartyIdentifier>APCS</ns0:PartyIdentifier>
          </ns0:PartyIdentificationDetails>
        </ns0:NameAndAddress>
      </ns0:GroupNameAndAddress>

      <ns0:GroupNameAndAddress>
        <ns0:NameAndAddress>
          <ns0:PartyFunctionCodeQualifier>DF</ns0:PartyFunctionCodeQualifier>
          <ns0:PartyIdentificationDetails>
            <ns0:PartyIdentifier>
              <xsl:value-of select="$forwarderAddressPSNValue"/>
            </ns0:PartyIdentifier>
          </ns0:PartyIdentificationDetails>
          <ns0:NameAndAddress>
            <ns0:NameAndAddressDescription>
              <xsl:value-of select="userCSharp:ToUpper($forwarderAddress/s0:Contact/text())" />
            </ns0:NameAndAddressDescription>
            <ns0:NameAndAddressDescription>
              <xsl:value-of select="$forwarderAddress/s0:CompanyName/text()"/>
            </ns0:NameAndAddressDescription>
            <ns0:NameAndAddressDescription>
              <xsl:value-of select="$forwarderAddress/s0:Address1/text()"/>
            </ns0:NameAndAddressDescription>
            <ns0:NameAndAddressDescription>
              <xsl:value-of select="normalize-space(concat($forwarderAddress/s0:Postcode/text(), ' ', $forwarderAddress/s0:City/text()))"/>
            </ns0:NameAndAddressDescription>
            <ns0:NameAndAddressDescription>
              <xsl:value-of select="$forwarderAddress/s0:Country/text()"/>
            </ns0:NameAndAddressDescription>
          </ns0:NameAndAddress>
        </ns0:NameAndAddress>

        <xsl:variable name="forwarderContact" select="$forwarderAddress/s0:Contact/text()"/>
        <xsl:if test="$forwarderContact!=''">
          <ns0:GroupContactInformation>
            <ns0:ContactInformation>
              <ns0:ContactFunctionCode>IC</ns0:ContactFunctionCode>
              <ns0:DepartmentOrEmployeeDetails>
                <ns0:DepartmentOrEmployeeName>
                  <xsl:value-of select="userCSharp:ToUpper($forwarderContact)"/>
                </ns0:DepartmentOrEmployeeName>
              </ns0:DepartmentOrEmployeeDetails>
            </ns0:ContactInformation>

            <xsl:variable name="forwarderPhone" select="$forwarderAddress/s0:Phone/text()"/>
            <xsl:if test="$forwarderPhone!=''">
              <ns0:CommunicationContact>
                <ns0:CommunicationAddressIdentifier>
                  <xsl:value-of select="$forwarderPhone"/>
                </ns0:CommunicationAddressIdentifier>
                <ns0:CommunicationAddressCodeQualifier>TE</ns0:CommunicationAddressCodeQualifier>
              </ns0:CommunicationContact>
            </xsl:if>

            <xsl:variable name="forwarderEmail" select="$forwarderAddress/s0:Email/text()"/>
            <xsl:if test="$forwarderEmail!=''">
              <ns0:CommunicationContact>
                <ns0:CommunicationAddressIdentifier>
                  <xsl:value-of select="$forwarderEmail"/>
                </ns0:CommunicationAddressIdentifier>
                <ns0:CommunicationAddressCodeQualifier>EM</ns0:CommunicationAddressCodeQualifier>
              </ns0:CommunicationContact>
            </xsl:if>
          </ns0:GroupContactInformation>
        </xsl:if>
      </ns0:GroupNameAndAddress>

      <ns0:GroupNameAndAddress>
        <ns0:NameAndAddress>
          <ns0:PartyFunctionCodeQualifier>MS</ns0:PartyFunctionCodeQualifier>
          <ns0:PartyIdentificationDetails>
            <ns0:PartyIdentifier>
              <xsl:call-template name="GetPSNNumber">
                <xsl:with-param name="orgAddress" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='CurrentUser']" />
              </xsl:call-template>
            </ns0:PartyIdentifier>
          </ns0:PartyIdentificationDetails>
          <ns0:NameAndAddress>
            <ns0:NameAndAddressDescription>
              <xsl:variable name="duns" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='CurrentUser']/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='DUN']"/>
              <xsl:variable name="eori" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='CurrentUser']/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='EOR']"/>
              <xsl:variable name="apcs" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='CurrentUser']/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='PSN']"/>
              <xsl:choose>
                <xsl:when test="$duns and $duns/s0:Value/text()!=''">
                  <xsl:value-of select="concat('DUNS:', $duns/s0:Value/text())"/>
                </xsl:when>
                <xsl:when test="$eori and $eori/s0:Value/text()!=''">
                  <xsl:value-of select="concat('EORI:', $eori/s0:CountryOfIssue/text(), $eori/s0:Value/text())"/>
                </xsl:when>
                <xsl:when test="$apcs and $apcs/s0:Value/text()!=''">
                  <xsl:value-of select="concat('APCS:', $apcs/s0:Value/text())"/>
                </xsl:when>
              </xsl:choose>
            </ns0:NameAndAddressDescription>
          </ns0:NameAndAddress>
        </ns0:NameAndAddress>
      </ns0:GroupNameAndAddress>

      <xsl:for-each select="s0:PackingLineCollection/s0:PackingLine[s0:UNDGCollection/s0:UNDG]">
        <xsl:variable name="packingLine" select="." />

        <ns0:GroupConsignmentInformation>
          <ns0:ConsignmentInformation>
            <ns0:ConsolidationItemNumber>
              <xsl:value-of select="format-number(position(),'00000')"/>
            </ns0:ConsolidationItemNumber>
            <ns0:DocumentMessageDetails>
              <ns0:DocumentIdentifier>
                <xsl:choose>
                  <xsl:when test="$documentNameType='Import'">
                    <xsl:value-of select="$shipment/s0:WayBillNumber/text()"/>
                  </xsl:when>
                  <xsl:when test="$documentNameType='Export'">
                    <xsl:value-of select="$shipment/s0:BookingConfirmationReference/text()"/>
                  </xsl:when>
                </xsl:choose>
              </ns0:DocumentIdentifier>
            </ns0:DocumentMessageDetails>
          </ns0:ConsignmentInformation>

          <xsl:if test="$documentNameType='Export'">
            <xsl:call-template name="CreateDateTimePeriod">
              <xsl:with-param name="dateTimeQualifier" select="'132'" />
              <xsl:with-param name="dateTimeValue" select="$shipment/s0:DateCollection/s0:Date[s0:Type/text()='Delivery']/s0:Value/text()" />
            </xsl:call-template>
          </xsl:if>

          <xsl:call-template name="CreateDateTimePeriod">
            <xsl:with-param name="dateTimeQualifier" select="'369'" />
            <xsl:with-param name="dateTimeValue" select="$shipment/s0:DateCollection/s0:Date[s0:Type/text()='HandlingDate']/s0:Value/text()" />
          </xsl:call-template>

          <xsl:if test="$documentNameType='Import'">
            <xsl:call-template name="CreateDateTimePeriod">
              <xsl:with-param name="dateTimeQualifier" select="'133'" />
              <xsl:with-param name="dateTimeValue" select="$shipment/s0:DateCollection/s0:Date[s0:Type/text()='Pickup']/s0:Value/text()" />
            </xsl:call-template>
          </xsl:if>

          <ns0:PlaceLocationIdentification>
            <ns0:LocationFunctionCodeQualifier>200</ns0:LocationFunctionCodeQualifier>
            <ns0:LocationIdentification>
              <ns0:LocationNameCode>
                <xsl:value-of select="$operationPortCode"/>
              </ns0:LocationNameCode>
            </ns0:LocationIdentification>

            <ns0:RelatedLocationOneIdentification>
              <xsl:variable name="ctoAddressType">
                <xsl:choose>
                  <xsl:when test="$documentNameType='Import'">ArrivalCTOAddress</xsl:when>
                  <xsl:when test="$documentNameType='Export'">DepartureCTOAddress</xsl:when>
                </xsl:choose>
              </xsl:variable>
              <ns0:FirstRelatedLocationNameCode>
                <xsl:call-template name="GetPSNNumber">
                  <xsl:with-param name="orgAddress" select="$shipment/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()=$ctoAddressType]" />
                </xsl:call-template>
              </ns0:FirstRelatedLocationNameCode>

              <ns0:CodeListIdentificationCode>TER</ns0:CodeListIdentificationCode>
              <ns0:CodeListResponsibleAgencyCode>ZZZ</ns0:CodeListResponsibleAgencyCode>
            </ns0:RelatedLocationOneIdentification>
          </ns0:PlaceLocationIdentification>

          <ns0:GroupDetailsOfTransport>
            <ns0:DetailsOfTransport>
              <ns0:TransportStageCodeQualifier>
                <xsl:choose>
                  <xsl:when test="$documentNameType='Export'">10</xsl:when>
                  <xsl:otherwise>30</xsl:otherwise>
                </xsl:choose>
              </ns0:TransportStageCodeQualifier>

              <xsl:variable name="transportMode" select="userCSharp:ToUpper($shipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='Other_TransportMode']/s0:Value/text())" />
              <ns0:ModeOfTransport>
                <ns0:TransportModeNameCode>
                  <xsl:choose>
                    <xsl:when test ="$transportMode='SEA' or $transportMode='IWT'">8</xsl:when>
                    <xsl:when test ="$transportMode='ROA'">3</xsl:when>
                    <xsl:when test ="$transportMode='RAI'">2</xsl:when>
                  </xsl:choose>
                </ns0:TransportModeNameCode>
              </ns0:ModeOfTransport>

              <ns0:TransportIdentification>
                <xsl:variable name="vesselLloydsIMO" select="$shipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='Other_VesselENINumber']/s0:Value/text()" />
                <ns0:TransportMeansIdentificationNameIdentifier>
                  <xsl:choose>
                    <xsl:when test="$vesselLloydsIMO!=''">
                      <xsl:value-of select="$vesselLloydsIMO"/>
                    </xsl:when>
                    <xsl:otherwise>TBN</xsl:otherwise>
                  </xsl:choose>
                </ns0:TransportMeansIdentificationNameIdentifier>

                <xsl:variable name="vesselName" select="$shipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='Other_VesselName']/s0:Value/text()" />
                <ns0:TransportMeansIdentificationName>
                  <xsl:choose>
                    <xsl:when test="$vesselName!=''">
                      <xsl:value-of select="$vesselName"/>
                    </xsl:when>
                    <xsl:otherwise>TBN</xsl:otherwise>
                  </xsl:choose>
                </ns0:TransportMeansIdentificationName>

                <xsl:call-template name="CreateElementIfNotEmpty">
                  <xsl:with-param name="elementName" select="'TransportMeansNationalityCode'"/>
                  <xsl:with-param name="value" select="$shipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='Other_Vessel_CountryCode']/s0:Value/text()"/>
                </xsl:call-template>
              </ns0:TransportIdentification>
            </ns0:DetailsOfTransport>
          </ns0:GroupDetailsOfTransport>

          <xsl:for-each select="s0:UNDGCollection/s0:UNDG">
            <ns0:GroupGoodsItemDetails>
              <ns0:GoodsItemDetails>
                <ns0:GoodsItemNumber>
                  <xsl:value-of select="format-number(position(),'00000')"/>
                </ns0:GoodsItemNumber>
                <xsl:if test="s0:PackQty/text()!='' and s0:PackType/text()!=''">
                  <ns0:NumberAndTypeOfPackages>
                    <ns0:PackageQuantity>
                      <xsl:value-of select="format-number(s0:PackQty/text(),'0000000')"/>
                    </ns0:PackageQuantity>
                    <ns0:PackageTypeDescriptionCode>
                      <xsl:variable name="packageTypeDescriptionCode" select="CodeMapper:GetRecipientCode('CPOINT', 'CPOINT', 'CPOINT System Configuration', 'Package Type', 'CPOINT Code', s0:PackType/text())"/>
                      <xsl:value-of select="$packageTypeDescriptionCode"/>
                    </ns0:PackageTypeDescriptionCode>
                  </ns0:NumberAndTypeOfPackages>
                </xsl:if>
              </ns0:GoodsItemDetails>

              <ns0:GroupDangerousGoods>
                <ns0:DangerousGoods>
                  <xsl:variable name="dgRegulationCode" select="CodeMapper:GetRecipientCode('CPOINT', 'CPOINT', 'CPOINT System Configuration', 'DG Regulation', 'Output Code', s0:Standard/text())"/>
                  <xsl:if test ="$dgRegulationCode!=''">
                    <ns0:DangerousGoodsRegulationsCode>
                      <xsl:value-of select="$dgRegulationCode"/>
                    </ns0:DangerousGoodsRegulationsCode>
                  </xsl:if>

                  <ns0:HazardCode>
                    <ns0:HazardIdentificationCode>
                      <xsl:value-of select="s0:IMOClass/text()"/>
                    </ns0:HazardIdentificationCode>
                  </ns0:HazardCode>

                  <ns0:UndgInformation>
                    <ns0:UnitedNationsDangerousGoodsUndgIdentifier>
                      <xsl:value-of select="s0:UNDGCode/text()"/>
                    </ns0:UnitedNationsDangerousGoodsUndgIdentifier>
                  </ns0:UndgInformation>

                  <ns0:DangerousGoodsShipmentFlashpoint>
                    <ns0:ShipmentFlashpointDegree>
                      <xsl:value-of select="format-number(s0:FlashPoint/text(), '#')"/>
                    </ns0:ShipmentFlashpointDegree>
                    <ns0:MeasurementUnitCode>CEL</ns0:MeasurementUnitCode>
                  </ns0:DangerousGoodsShipmentFlashpoint>

                  <xsl:variable name="packingGroup" select="s0:PackingGroup/text()"/>
                  <xsl:if test ="$packingGroup!=''">
                    <ns0:PackagingDangerLevelCode>
                      <xsl:choose>
                        <xsl:when test="$packingGroup='I'">1</xsl:when>
                        <xsl:when test="$packingGroup='II'">2</xsl:when>
                        <xsl:when test="$packingGroup='III'">3</xsl:when>
                      </xsl:choose>
                    </ns0:PackagingDangerLevelCode>
                  </xsl:if>

                  <ns0:EmergencyProcedureForShipsIdentifier>
                    <xsl:value-of select="concat(s0:EmergencyScheduleFire/text(), s0:EmergencyScheduleSpillage/text())"/>
                  </ns0:EmergencyProcedureForShipsIdentifier>
                  <ns0:HazardMedicalFirstAidGuideIdentifier>
                    <xsl:value-of select="s0:MedicalFirstAidGuide/text()"/>
                  </ns0:HazardMedicalFirstAidGuideIdentifier>
                </ns0:DangerousGoods>

                <xsl:variable name="aadTextReference">
                  <xsl:choose>
                    <xsl:when test="s0:PackedInExceptedQuantity/text()='true'">TEQ</xsl:when>
                    <xsl:when test="s0:PackedInLimitedQuantity/text()='true'">TLQ</xsl:when>
                    <xsl:otherwise></xsl:otherwise>
                  </xsl:choose>
                </xsl:variable>
                <xsl:call-template name="CreateFreeText">
                  <xsl:with-param name="codeQualifier" select="'AAD'" />
                  <xsl:with-param name="textReference" select="$aadTextReference" />
                  <xsl:with-param name="textValue" select="s0:TechicalName/text()"/>
                </xsl:call-template>

                <xsl:call-template name="CreateFreeText">
                  <xsl:with-param name="codeQualifier" select="'AAC'" />
                  <xsl:with-param name="textReference" select="'P'" />
                  <xsl:with-param name="textValue">
                    <xsl:choose>
                      <xsl:when test="s0:MarinePollutant/text()='Y' or s0:MarinePollutant/text()='S'">P</xsl:when>
                      <xsl:otherwise>NP</xsl:otherwise>
                    </xsl:choose>
                  </xsl:with-param>
                </xsl:call-template>

                <xsl:call-template name="CreateMeasurement">
                  <xsl:with-param name="codeQualifier" select="'AAE'" />
                  <xsl:with-param name="attributeCode" select="'AAL'" />
                  <xsl:with-param name="measureUnitCode" select="s0:WeightUQ/text()"/>
                  <xsl:with-param name="measureValue" select="s0:Weight/text()"/>
                </xsl:call-template>

                <xsl:call-template name="CreateMeasurement">
                  <xsl:with-param name="codeQualifier" select="'AAE'" />
                  <xsl:with-param name="attributeCode" select="'AEN'" />
                  <xsl:with-param name="measureUnitCode" select="'NMB'"/>
                  <xsl:with-param name="measureValue" select="s0:RadioactiveTransportIndex/text()"/>
                </xsl:call-template>

                <xsl:call-template name="CreateMeasurement">
                  <xsl:with-param name="codeQualifier" select="'AAE'" />
                  <xsl:with-param name="attributeCode" select="'AEO'" />
                  <xsl:with-param name="measureUnitCode" select="s0:RadioactivityUQ/text()"/>
                  <xsl:with-param name="measureValue" select="s0:Radioactivity/text()"/>
                </xsl:call-template>

                <xsl:call-template name="CreateMeasurement">
                  <xsl:with-param name="codeQualifier" select="'AAE'" />
                  <xsl:with-param name="attributeCode" select="'T19'" />
                  <xsl:with-param name="measureUnitCode" select="s0:NetExplosiveWeightUQ/text()"/>
                  <xsl:with-param name="measureValue" select="s0:NetExplosiveWeight/text()"/>
                </xsl:call-template>

                <xsl:call-template name="CreateMeasurement">
                  <xsl:with-param name="codeQualifier" select="'AAE'" />
                  <xsl:with-param name="attributeCode" select="'T20'" />
                  <xsl:with-param name="measureUnitCode" select="'NMB'"/>
                  <xsl:with-param name="measureValue" select="s0:RadioactiveCriticalitySafetyIndex/text()"/>
                </xsl:call-template>

                <xsl:variable name="containerNumber" select="$packingLine/s0:ContainerNumber/text()"/>
                <xsl:if test="$containerNumber!=''">
                  <ns0:GroupSplitGoodsPlacement>
                    <ns0:SplitGoodsPlacement>
                      <ns0:EquipmentIdentification>
                        <ns0:EquipmentIdentifier>
                          <xsl:value-of select="$containerNumber"/>
                        </ns0:EquipmentIdentifier>
                      </ns0:EquipmentIdentification>
                      <ns0:PackageQuantity>
                        <xsl:value-of select="$shipment/s0:ContainerCollection/s0:Container[s0:ContainerNumber/text()=$containerNumber]/s0:PalletCount/text()"/>
                      </ns0:PackageQuantity>
                    </ns0:SplitGoodsPlacement>
                  </ns0:GroupSplitGoodsPlacement>
                </xsl:if>
              </ns0:GroupDangerousGoods>
            </ns0:GroupGoodsItemDetails>
          </xsl:for-each>
        </ns0:GroupConsignmentInformation>
      </xsl:for-each>
    </ns0:IFTDGN>
  </xsl:template>

  <xsl:template name="GetPSNNumber">
    <xsl:param name="orgAddress" />

    <xsl:choose>
      <xsl:when test="$orgAddress">
        <xsl:value-of select="$orgAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='PSN']/s0:Value/text()"/>
      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="CreateDateTimePeriod">
    <xsl:param name="dateTimeQualifier" />
    <xsl:param name="dateTimeValue" />

    <xsl:if test="$dateTimeValue!=''">
      <ns0:DateTimePeriod>
        <ns0:DateTimeFunctionQualifier>
          <xsl:value-of select="$dateTimeQualifier"/>
        </ns0:DateTimeFunctionQualifier>
        <ns0:DateOrTimeOrPeriodText>
          <xsl:value-of select="DateMapper:ConvertXmlDateString($dateTimeValue, 'yyyy-MM-ddTHH:mm:ss')"/>
        </ns0:DateOrTimeOrPeriodText>
      </ns0:DateTimePeriod>
    </xsl:if>
  </xsl:template>

  <xsl:template name="CreateFreeText">
    <xsl:param name="codeQualifier" />
    <xsl:param name="textReference" />
    <xsl:param name="textValue" />

    <xsl:if test="$textValue!=''">
      <ns0:FreeText>
        <ns0:TextSubjectCodeQualifier>
          <xsl:value-of select="$codeQualifier"/>
        </ns0:TextSubjectCodeQualifier>
        <xsl:if test="$textReference!=''">
          <ns0:TextReference>
            <ns0:FreeTextDescriptionCode>
              <xsl:value-of select="$textReference"/>
            </ns0:FreeTextDescriptionCode>
          </ns0:TextReference>
        </xsl:if>
        <ns0:TextLiteral>
          <ns0:FreeText>
            <xsl:value-of select="$textValue"/>
          </ns0:FreeText>
        </ns0:TextLiteral>
      </ns0:FreeText>
    </xsl:if>
  </xsl:template>

  <xsl:template name="CreateMeasurement">
    <xsl:param name="codeQualifier" />
    <xsl:param name="attributeCode" />
    <xsl:param name="measureUnitCode" />
    <xsl:param name="measureValue" />

    <xsl:if test="number($measureValue)>0">
      <ns0:Measurements>
        <ns0:MeasurementPurposeCodeQualifier>
          <xsl:value-of select="$codeQualifier"/>
        </ns0:MeasurementPurposeCodeQualifier>
        <ns0:MeasurementDetails>
          <ns0:MeasuredAttributeCode>
            <xsl:value-of select="$attributeCode"/>
          </ns0:MeasuredAttributeCode>
        </ns0:MeasurementDetails>
        <ns0:ValueRange>
          <xsl:if test="$measureUnitCode!=''">
            <ns0:MeasurementUnitCode>
              <xsl:choose>
                <xsl:when test="contains('KG', $measureUnitCode)">KGM</xsl:when>
                <xsl:when test="contains('GR', $measureUnitCode)">GRM</xsl:when>
                <xsl:when test="contains('TN', $measureUnitCode)">TNE</xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$measureUnitCode"/>
                </xsl:otherwise>
              </xsl:choose>
            </ns0:MeasurementUnitCode>
          </xsl:if>
          <ns0:Measure>
            <xsl:choose>
              <xsl:when test="$attributeCode='AAL'">
                <xsl:value-of select="UnitConverter:Convert($measureValue, $measureUnitCode, 'KG')" />
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$measureValue"/>
              </xsl:otherwise>
            </xsl:choose>
          </ns0:Measure>
        </ns0:ValueRange>
      </ns0:Measurements>
    </xsl:if>
  </xsl:template>

  <xsl:template name="CreateGroupReference">
    <xsl:param name="codeQualifier" />
    <xsl:param name="identifier" />

    <xsl:if test="$identifier!=''">
      <ns0:GroupReference>
        <xsl:call-template name="CreateReference">
          <xsl:with-param name="codeQualifier" select="$codeQualifier" />
          <xsl:with-param name="identifier" select="$identifier" />
        </xsl:call-template>
      </ns0:GroupReference>
    </xsl:if>
  </xsl:template>

  <xsl:template name="CreateReference">
    <xsl:param name="codeQualifier" />
    <xsl:param name="identifier" />

    <xsl:if test="$identifier!=''">
      <ns0:Reference>
        <ns0:ReferenceCodeQualifier>
          <xsl:value-of select="$codeQualifier" />
        </ns0:ReferenceCodeQualifier>
        <ns0:ReferenceIdentifier>
          <xsl:value-of select="$identifier" />
        </ns0:ReferenceIdentifier>
      </ns0:Reference>
    </xsl:if>
  </xsl:template>

  <xsl:template name="CreateElementIfNotEmpty">
    <xsl:param name="elementName" />
    <xsl:param name="value" />

    <xsl:if test="$value!=''">
      <xsl:element name="ns0:{$elementName}">
        <xsl:value-of select="$value"/>
      </xsl:element>
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
]]>
  </msxsl:script>
</xsl:stylesheet>
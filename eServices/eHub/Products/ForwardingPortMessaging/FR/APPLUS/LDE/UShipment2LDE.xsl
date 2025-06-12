<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 CodeMapper ContextAccessor DataModelAccessor DateMapper userCSharp" version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2012/11/LDE/1"
                xmlns:ns0="http://www.cargowise.com/Schemas/FPM/APPLUS/LDE"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
                xmlns:DataModelAccessor="http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"
                xmlns:DateMapper="http://schemas.microsoft.com/BizTalk/2003/DateMapper"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes" />

  <xsl:template match="/">
    <xsl:apply-templates select="s0:UniversalShipment/s0:Shipment" />
  </xsl:template>

  <xsl:variable name="recipientID" select="ContextAccessor:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="senderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="serviceProvider" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Port Settings' , 'Name', $recipientID)"/>
  <xsl:variable name="serviceProviderMSGID" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Port Settings' , 'MSGID', $recipientID)"/>
  <xsl:variable name="serviceProviderID" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Port Settings' , 'ID', $recipientID)"/>
  <xsl:variable name="msgPrefix"  select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Port Settings' , 'SubscriptionPrefix', $recipientID)"/>

  <xsl:variable name="shipment" select="/s0:UniversalShipment/s0:Shipment"/>
  <xsl:variable name="operationPortCode" select="$shipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='OperationalPort_Code']/s0:Value/text()"/>
  <xsl:variable name="routingPartyAttr" select="DataModelAccessor:GetClientRegistrationAttri1AsString($senderID, $operationPortCode, $serviceProvider)" />

  <xsl:variable name="routingParty">
    <xsl:choose>
      <xsl:when test="string($routingPartyAttr)!=''">
        <xsl:value-of select="$routingPartyAttr"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$serviceProvider"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="testID">
    <xsl:choose>
      <xsl:when test="contains($recipientID, 'TST')">_TST</xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="newRecipientID">
    <xsl:choose>
      <xsl:when test="$routingParty!='' and $routingParty!=$serviceProvider">
        <xsl:choose>
          <xsl:when test="$routingParty='MGI'">
            <xsl:value-of select="concat('MGI_SOGET_1', $testID)" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="concat('SOGET_MGI_1', $testID)" />
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$recipientID" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="newServiceProvider">
    <xsl:choose>
      <xsl:when test="$routingParty!=''">
        <xsl:value-of select="$routingParty"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$serviceProvider"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="registrationNumberType">
    <xsl:choose>
      <xsl:when test="$serviceProvider='SOGET'">SON</xsl:when>
      <xsl:otherwise>CI5</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="registrationNumberTypeEmetteur">
    <xsl:choose>
      <xsl:when test="$serviceProvider='SOGET'">SOW</xsl:when>
      <xsl:otherwise>CI5</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:template match="s0:UniversalShipment/s0:Shipment">
    <xsl:variable name="consolID" select="s0:DataContext/s0:DataSource/s0:Key/text()"/>
    <xsl:variable name="purpose" select="s0:DataContext/s0:DocumentaryOverride/s0:Purpose/text()" />
    <xsl:variable name="documentName" select="s0:DataContext/s0:DocumentaryOverride/s0:DocumentName/text()" />

    <xsl:variable name="parseClientRegistration" select="userCSharp:ParseClientRegistration(DataModelAccessor:GetClientRegistrationCode($senderID, $operationPortCode, $serviceProvider))" />
    <xsl:variable name="applusID" select="userCSharp:GetUser()" />
    <xsl:variable name="applusTierProf">
      <xsl:call-template name="GetRegistrationNumberWithFallBack">
        <xsl:with-param name="addressType" select="'BookingPartyDocumentaryAddress'" />
        <xsl:with-param name="fallbackAddressType" select="'CurrentUser'" />
        <xsl:with-param name="registrationNumberType" select="$registrationNumberTypeEmetteur"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="interchangeID" select="CodeMapper:CallActionProcedureHelper('GetCounterValue','@value','@name','CargoWise.eHub.Products.ForwardingPortMessaging.FR.APPLUS.Interchange','@maxlength','14')" />
    <xsl:variable name="subscribeClientID" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderID, $serviceProvider, $senderID, concat($applusID, ',', $applusTierProf))" />

    <xsl:variable name="InboxPK" select="ContextAccessor:GetContextProperty('InternalTrackingID', 'http://cargowise.com/ehub/tracking/2010/06')"/>
    <xsl:variable name="FileName" select="ContextAccessor:SetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06', concat('LDE_', $senderID, '_', $interchangeID))"/>
    <xsl:variable name="EmailSubject" select="ContextAccessor:SetContextProperty('OverrideEmailSubject', 'http://cargowise.com/ehub/processing/2010/06', concat('FPM_', $serviceProvider, '_', $operationPortCode))"/>

    <xsl:if test="$newRecipientID!=$recipientID">
      <xsl:variable name="SetRecipientID" select="ContextAccessor:SetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties', $newRecipientID)"/>
    </xsl:if>

    <ns0:Interchanges>
      <xsl:attribute name="id">
        <xsl:value-of select="$interchangeID"/>
      </xsl:attribute>
      <xsl:attribute name="from">
        <xsl:value-of select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Service Provider Settings' , 'SenderID', $serviceProvider, $operationPortCode, $routingParty)"/>
      </xsl:attribute>
      <xsl:attribute name="to">
        <xsl:value-of select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Service Provider Settings' , 'RecipientID', $serviceProvider, $operationPortCode, $routingParty)"/>
      </xsl:attribute>

      <xsl:for-each select="s0:ContainerCollection/s0:Container">
        <xsl:variable name="containerNumber" select="s0:ContainerNumber/text()" />
        <xsl:variable name="messageReference" select="concat($consolID, '_', $containerNumber, '_LDE')"/>

        <xsl:variable name="previousMessageIdentifier" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $serviceProvider, '@recipientId', $senderID , '@ST_ID',  $serviceProviderMSGID , '@value', $messageReference, '@referenceType', 'MessageReference')"/>
        <xsl:variable name="messageIdentifier">
          <xsl:choose>
            <xsl:when test="$previousMessageIdentifier!=''">
              <xsl:value-of select="$previousMessageIdentifier"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:variable name="messageSetID" select="CodeMapper:CallActionProcedureHelper('GetCounterValue','@value','@name','CargoWise.eHub.Products.ForwardingPortMessaging.FR.APPLUS','@maxlength','14')" />
              <xsl:variable name="formattedMessageIdentifier" select="concat($msgPrefix, format-number($messageSetID, '0000000000'))" />

              <xsl:variable name="subscribeJobNumber1" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $formattedMessageIdentifier, $consolID, 'JobNumber')" />
              <xsl:variable name="subscribeJobNumber2" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $consolID, $formattedMessageIdentifier, $formattedMessageIdentifier)" />

              <xsl:variable name="SubscribeMessageReference" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageReference, $formattedMessageIdentifier, 'MessageReference')" />

              <!--the following subscription will not change after ORG is sent-->
              <xsl:variable name="SubscribeForwarderRef" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $formattedMessageIdentifier, $containerNumber, 'RequestID')" />
              <xsl:variable name="SubscribeContainerNumber" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $formattedMessageIdentifier, $containerNumber, 'ContainerNumber')" />
              <xsl:variable name="subscribeDocumentName" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $formattedMessageIdentifier, $documentName, 'DocumentName')" />

              <xsl:value-of select="$formattedMessageIdentifier"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <xsl:variable name="SubscribeInboxPK" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $InboxPK, concat($interchangeID, '_', $messageIdentifier))" />
        <xsl:variable name="subscribeShipmentType" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageIdentifier, $shipment/s0:DataContext/s0:DataSource/s0:Type/text(), 'ForwardingType')" />
        <xsl:variable name="subscribeActionPurpose" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageIdentifier, $purpose, 'Purpose')" />
        <xsl:variable name="subscribeOperationPort" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageIdentifier, $operationPortCode, 'OperationPort')" />

        <xsl:variable name="isControlledAtmosphere">
          <xsl:choose>
            <xsl:when test="s0:IsControlledAtmosphere/text()='true'">Y</xsl:when>
            <xsl:otherwise>N</xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <ns0:MessageSet>
          <xsl:attribute name="id">
            <xsl:value-of select="$messageIdentifier"/>
          </xsl:attribute>

          <xsl:attribute name="icid">
            <xsl:value-of select="$interchangeID"/>
          </xsl:attribute>

          <xsl:attribute name="date">
            <xsl:value-of select="DateMapper:ConvertXmlDateString($shipment/s0:DataContext/s0:Workflow/s0:TriggerDate/text(), 'dd/MM/yyyy HH:mm:ss')"/>
          </xsl:attribute>

          <ns0:Destinataire>
            <xsl:attribute name="user">
              <xsl:value-of select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Service Provider Settings' , 'DestinationUser', $serviceProvider, $operationPortCode, $routingParty)"/>
            </xsl:attribute>
            <xsl:attribute name="tiersProf">
              <xsl:value-of select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Service Provider Settings' , 'DestinationParty', $serviceProvider, $operationPortCode, $routingParty)"/>
            </xsl:attribute>
          </ns0:Destinataire>

          <ns0:Emetteur>
            <xsl:attribute name="user">
              <xsl:value-of select="$applusID"/>
            </xsl:attribute>
            <xsl:attribute name="tiersProf">
              <xsl:value-of select="$applusTierProf" />
            </xsl:attribute>
          </ns0:Emetteur>

          <ns0:Messages>
            <ns0:Request>
              <xsl:attribute name="action">
                <xsl:choose>
                  <xsl:when test="$purpose='ORG'">CREATE</xsl:when>
                  <xsl:when test="$purpose='AMD'">UPDATE</xsl:when>
                  <xsl:when test="$purpose='WTH'">DELETE</xsl:when>
                  <xsl:otherwise></xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
              <xsl:attribute name="id">
                <xsl:value-of select="$messageIdentifier"/>
              </xsl:attribute>
              <xsl:attribute name="type">LDE</xsl:attribute>

              <ns0:empotage>
                <ns0:reference-emp>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'lde'" />
                    <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='LDEAPPlusID']/s0:Value/text()" />
                  </xsl:call-template>

                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'cbk'" />
                    <xsl:with-param name="value" select="$shipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CBK']/s0:Value/text()" />
                  </xsl:call-template>

                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'extcbk'" />
                    <xsl:with-param name="value" select="$shipment/s0:BookingConfirmationReference/text()" />
                  </xsl:call-template>

                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'statut'" />
                    <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='LDEStatus']/s0:Value/text()" />
                  </xsl:call-template>

                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'ref'" />
                    <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='AMQReference']/s0:Value/text()" />
                  </xsl:call-template>

                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'sic'" />
                    <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ECTReference']/s0:Value/text()" />
                  </xsl:call-template>

                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'extempotage'" />
                    <xsl:with-param name="value" select="concat($shipment/s0:AdditionalReferenceCollection/s0:AdditionalReference[s0:Type/text()='FFW']/s0:ReferenceNumber/text(), '_', $containerNumber)" />
                  </xsl:call-template>
                </ns0:reference-emp>

                <ns0:lieu-emp>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'zone'" />
                    <xsl:with-param name="value" select="$shipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='PortArea']/s0:Value/text()" />
                  </xsl:call-template>

                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'lieu'" />
                    <xsl:with-param name="value" select="$shipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='PortLocation']/s0:Value/text()" />
                  </xsl:call-template>
                </ns0:lieu-emp>

                <ns0:voyage-emp>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'otc'" />
                    <xsl:with-param name="value" select="$shipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='OTC']/s0:Value/text()" />
                  </xsl:call-template>

                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'ser'" />
                    <xsl:with-param name="value" select="$shipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='VoyageServiceCode']/s0:Value/text()" />
                  </xsl:call-template>

                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'atp'" />
                    <xsl:with-param name="value" select="$shipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ATP']/s0:Value/text()" />
                  </xsl:call-template>
                </ns0:voyage-emp>

                <ns0:tiers-emp>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'afret'" />
                    <xsl:with-param name="value">
                      <xsl:call-template name="GetRegistrationNumber">
                        <xsl:with-param name="addressType" select="'ShippingLineAddress'" />
                        <xsl:with-param name="registrationNumberType" select="$registrationNumberType"/>
                      </xsl:call-template>
                    </xsl:with-param>
                  </xsl:call-template>

                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'trport'" />
                    <xsl:with-param name="value">
                      <xsl:call-template name="GetRegistrationNumber">
                        <xsl:with-param name="addressType" select="'CurrentUser'" />
                        <xsl:with-param name="registrationNumberType" select="$registrationNumberType"/>
                      </xsl:call-template>
                    </xsl:with-param>
                  </xsl:call-template>
                </ns0:tiers-emp>

                <ns0:reference-annexe-emp>
                  <xsl:value-of select="concat($shipment/s0:AdditionalReferenceCollection/s0:AdditionalReference[s0:Type/text()='FFW']/s0:ReferenceNumber/text(), '_', $containerNumber)"/>
                </ns0:reference-annexe-emp>

                <ns0:deplacement-emp>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'sic'" />
                    <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TransportMode']/s0:Value/text()" />
                  </xsl:call-template>

                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'code'" />
                    <xsl:with-param name="value">
                      <xsl:call-template name="GetRegistrationNumber">
                        <xsl:with-param name="addressType" select="'DepartureCFSLocalTransportAddress'" />
                        <xsl:with-param name="registrationNumberType" select="$registrationNumberType"/>
                      </xsl:call-template>
                    </xsl:with-param>
                  </xsl:call-template>

                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'nom'" />
                    <xsl:with-param name="value" select="$shipment/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='DepartureCFSLocalTransportAddress']/s0:CompanyName/text()" />
                  </xsl:call-template>

                  <xsl:call-template name="CreateAttribute_Date">
                    <xsl:with-param name="name" select="'date'" />
                    <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='DateOfArrival']/s0:Value/text()" />
                  </xsl:call-template>

                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'id'" />
                    <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='VehicleRegistration']/s0:Value/text()" />
                  </xsl:call-template>

                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'zone'" />
                    <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='DeliveryArea']/s0:Value/text()" />
                  </xsl:call-template>

                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'lieu'" />
                    <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='DeliveryLocation']/s0:Value/text()" />
                  </xsl:call-template>
                </ns0:deplacement-emp>

                <ns0:equipement-emp>
                  <xsl:attribute name="id">
                    <xsl:value-of select="$containerNumber"/>
                  </xsl:attribute>

                  <xsl:variable name="serviceProviderContainerISO" select="CodeMapper:GetRecipientCode($serviceProvider, $serviceProvider, concat($serviceProvider, ' System Configuration'), 'ContainerTypeToISOCode' , concat($serviceProvider, ' Code'),  s0:ContainerType/s0:ISOCode/text())"/>
                  <xsl:variable name="containerTypeMappingCode">
                    <xsl:choose>
                      <xsl:when test="$serviceProviderContainerISO != ''">
                        <xsl:value-of select="$serviceProviderContainerISO"/>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration', 'ContainerTypeToISOCode' , 'Carrier Code', s0:ContainerType/s0:ISOCode/text())"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:variable>

                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'code'" />
                    <xsl:with-param name="value" select="$containerTypeMappingCode" />
                  </xsl:call-template>

                  <xsl:call-template name="CreateAttribute_Integer">
                    <xsl:with-param name="name" select="'poids'" />
                    <xsl:with-param name="value" select="round(s0:GrossWeight/text())" />
                  </xsl:call-template>

                  <xsl:call-template name="CreateAttribute_Integer">
                    <xsl:with-param name="name" select="'tare'" />
                    <xsl:with-param name="value" select="round(s0:TareWeight/text())" />
                  </xsl:call-template>

                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'dgx'" />
                    <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='HazardousCargo']/s0:Value/text()" />
                  </xsl:call-template>

                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'dim'" />
                    <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='OversizeContainer']/s0:Value/text()" />
                  </xsl:call-template>

                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'frigo'" />
                    <xsl:with-param name="value" select="$isControlledAtmosphere" />
                  </xsl:call-template>

                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'polluant'" />
                    <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='MarinePollutant']/s0:Value/text()" />
                  </xsl:call-template>

                  <xsl:if test="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='OversizeContainer']/s0:Value/text()='Y'">
                    <xsl:call-template name="CreateAttribute_Integer">
                      <xsl:with-param name="name" select="'olf'" />
                      <xsl:with-param name="value" select="round(s0:OverhangFront/text())" />
                    </xsl:call-template>

                    <xsl:call-template name="CreateAttribute_Integer">
                      <xsl:with-param name="name" select="'olb'" />
                      <xsl:with-param name="value" select="round(s0:OverhangBack/text())" />
                    </xsl:call-template>

                    <xsl:call-template name="CreateAttribute_Integer">
                      <xsl:with-param name="name" select="'owr'" />
                      <xsl:with-param name="value" select="round(s0:OverhangRight/text())" />
                    </xsl:call-template>

                    <xsl:call-template name="CreateAttribute_Integer">
                      <xsl:with-param name="name" select="'owl'" />
                      <xsl:with-param name="value" select="round(s0:OverhangLeft/text())" />
                    </xsl:call-template>

                    <xsl:call-template name="CreateAttribute_Integer">
                      <xsl:with-param name="name" select="'oh'" />
                      <xsl:with-param name="value" select="round(s0:OverhangHeight/text())" />
                    </xsl:call-template>
                  </xsl:if>

                  <xsl:if test="$isControlledAtmosphere='Y'">
                    <xsl:call-template name="CreateAttribute">
                      <xsl:with-param name="name" select="'min'" />
                      <xsl:with-param name="value">
                        <xsl:choose>
                          <xsl:when test="$shipment/s0:RequiredTemperatureMinimum/text()!=''">
                            <xsl:value-of select="round($shipment/s0:RequiredTemperatureMinimum/text())"/>
                          </xsl:when>
                          <xsl:otherwise>
                            <xsl:value-of select="round(s0:SetPointTemp/text())"/>
                          </xsl:otherwise>
                        </xsl:choose>
                      </xsl:with-param>
                    </xsl:call-template>

                    <xsl:call-template name="CreateAttribute">
                      <xsl:with-param name="name" select="'max'" />
                      <xsl:with-param name="value">
                        <xsl:choose>
                          <xsl:when test="$shipment/s0:RequiredTemperatureMaximum/text()!=''">
                            <xsl:value-of select="round($shipment/s0:RequiredTemperatureMaximum/text())"/>
                          </xsl:when>
                          <xsl:otherwise>
                            <xsl:value-of select="round(s0:SetPointTemp/text())"/>
                          </xsl:otherwise>
                        </xsl:choose>
                      </xsl:with-param>
                    </xsl:call-template>

                    <xsl:variable name="tempUnit">
                      <xsl:choose>
                        <xsl:when test="$shipment/s0:RequiredTemperatureUnit/text()!=''">
                          <xsl:value-of select="$shipment/s0:RequiredTemperatureUnit/text()"/>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:value-of select="s0:SetPointTempUnit/text()"/>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>

                    <xsl:call-template name="CreateAttribute">
                      <xsl:with-param name="name" select="'unite'" />
                      <xsl:with-param name="value">
                        <xsl:choose>
                          <xsl:when test="$tempUnit='C'">CEL</xsl:when>
                          <xsl:when test="$tempUnit='F'">FAH</xsl:when>
                          <xsl:otherwise>
                            <xsl:value-of select="$tempUnit"/>
                          </xsl:otherwise>
                        </xsl:choose>
                      </xsl:with-param>
                    </xsl:call-template>
                  </xsl:if>
                </ns0:equipement-emp>

                <xsl:variable name="regNumber">
                  <xsl:call-template name="GetRegistrationNumber">
                    <xsl:with-param name="addressType" select="'DepartureCFSLocalTransportAddress'" />
                    <xsl:with-param name="registrationNumberType" select="$registrationNumberType"/>
                  </xsl:call-template>
                </xsl:variable>
                <xsl:if test="$regNumber != ''">
                  <ns0:acheminement-emp>
                    <xsl:call-template name="CreateAttribute">
                      <xsl:with-param name="name" select="'sic'" />
                      <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TransportMode']/s0:Value/text()" />
                    </xsl:call-template>

                    <xsl:call-template name="CreateAttribute">
                      <xsl:with-param name="name" select="'code'" />
                      <xsl:with-param name="value" select="$regNumber" />
                    </xsl:call-template>

                    <xsl:call-template name="CreateAttribute">
                      <xsl:with-param name="name" select="'id'" />
                      <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='VehicleRegistration']/s0:Value/text()" />
                    </xsl:call-template>

                    <xsl:call-template name="CreateAttribute">
                      <xsl:with-param name="name" select="'nom'" />
                      <xsl:with-param name="value" select="$shipment/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='DepartureCFSLocalTransportAddress']/s0:CompanyName/text()" />
                    </xsl:call-template>

                    <xsl:call-template name="CreateAttribute">
                      <xsl:with-param name="name" select="'date'" />
                      <xsl:with-param name="value" select="DateMapper:ConvertXmlDateString(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='DateOfArrival']/s0:Value/text(), 'dd/MM/yyyy HH:mm')" />
                    </xsl:call-template>
                  </ns0:acheminement-emp>
                </xsl:if>

                <xsl:variable name="containerLink" select="s0:Link/text()" />
                <xsl:for-each select="$shipment/s0:SubShipmentCollection/s0:SubShipment">
                  <xsl:variable name="appliesToAllPacks" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='AppliesToAllPacks']/s0:Value/text()" />
                  <xsl:variable name="packingLines" select="s0:PackingLineCollection/s0:PackingLine[s0:ContainerLink/text()=$containerLink]" />

                  <xsl:if test="$packingLines">
                    <ns0:marchandise-emp>
                      <xsl:call-template name="CreateAttribute">
                        <xsl:with-param name="name" select="'sic'" />
                        <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CommodityReference']/s0:Value/text()" />
                      </xsl:call-template>

                      <xsl:attribute name="poids">
                        <xsl:value-of select="round(sum($packingLines/s0:Weight/text()))"/>
                      </xsl:attribute>

                      <xsl:call-template name="CreateAttribute">
                        <xsl:with-param name="name" select="'ref'" />
                        <xsl:with-param name="value" select="s0:AdditionalReferenceCollection/s0:AdditionalReference[s0:Type/text()='ERC']/s0:ReferenceNumber/text()" />
                        <xsl:with-param name="fallbackValue" select="s0:AdditionalReferenceCollection/s0:AdditionalReference[s0:Type/text()='FFW']/s0:ReferenceNumber/text()"/>
                      </xsl:call-template>

                      <xsl:for-each select="$packingLines">
                        <ns0:mesurage-emp>
                          <xsl:variable name="serviceProviderPackTypeCode" select="CodeMapper:GetRecipientCode($serviceProvider, $serviceProvider, concat($serviceProvider, ' System Configuration'), 'Package Type ISO' , concat($serviceProvider, ' Code'),  s0:PackType/text())"/>
                          <xsl:variable name="packTypeMappingCode">
                            <xsl:choose>
                              <xsl:when test="$serviceProviderPackTypeCode != ''">
                                <xsl:value-of select="$serviceProviderPackTypeCode"/>
                              </xsl:when>
                              <xsl:otherwise>
                                <xsl:value-of select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Package Type ISO' , 'Output Code', s0:PackType/text())"/>
                              </xsl:otherwise>
                            </xsl:choose>
                          </xsl:variable>
                          <xsl:attribute name="code">
                            <xsl:value-of select="$packTypeMappingCode"/>
                          </xsl:attribute>

                          <xsl:attribute name="qte">
                            <xsl:value-of select="round(s0:PackQty/text())"/>
                          </xsl:attribute>

                          <xsl:attribute name="vol">
                            <xsl:value-of select="round(s0:Volume/text())"/>
                          </xsl:attribute>

                          <xsl:call-template name="CreateAttribute">
                            <xsl:with-param name="name" select="'ref'" />
                            <xsl:with-param name="value" select="s0:PackingLineID/text()" />
                          </xsl:call-template>

                          <ns0:marques-emp>
                            <xsl:value-of select="s0:MarksAndNos/text()" />
                          </ns0:marques-emp>
                        </ns0:mesurage-emp>
                      </xsl:for-each>

                      <ns0:declaration-emp>
                        <xsl:attribute name="total">
                          <xsl:choose>
                            <xsl:when test="$appliesToAllPacks='Y'">Y</xsl:when>
                            <xsl:otherwise>N</xsl:otherwise>
                          </xsl:choose>
                        </xsl:attribute>

                        <xsl:attribute name="nb">
                          <xsl:value-of select="round(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='DeclaredPackCount']/s0:Value/text())"/>
                        </xsl:attribute>

                        <xsl:attribute name="poids">
                          <xsl:value-of select="round(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='DeclaredPackWeight']/s0:Value/text())"/>
                        </xsl:attribute>

                        <xsl:attribute name="dec">
                          <xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='DeclarationAPPlusID']/s0:Value/text()"/>
                        </xsl:attribute>
                      </ns0:declaration-emp>
                    </ns0:marchandise-emp>
                  </xsl:if>
                </xsl:for-each>

                <xsl:variable name="seal" select="s0:Seal/text()" />
                <xsl:if test="$seal!=''">
                  <xsl:call-template name="scelle-emp">
                    <xsl:with-param name="num" select="$seal" />
                    <xsl:with-param name="ori" select="s0:SealPartyType/text()"/>
                  </xsl:call-template>
                </xsl:if>

                <xsl:variable name="secondSeal" select="s0:SecondSeal/text()" />
                <xsl:if test="$secondSeal!=''">
                  <xsl:call-template name="scelle-emp">
                    <xsl:with-param name="num" select="$secondSeal" />
                    <xsl:with-param name="ori" select="s0:SecondSealPartyType/text()"/>
                  </xsl:call-template>
                </xsl:if>

                <xsl:variable name="thirdSeal" select="s0:ThirdSeal/text()" />
                <xsl:if test="$thirdSeal!=''">
                  <xsl:call-template name="scelle-emp">
                    <xsl:with-param name="num" select="$thirdSeal" />
                    <xsl:with-param name="ori" select="s0:ThirdSealPartyType/text()"/>
                  </xsl:call-template>
                </xsl:if>

                <xsl:for-each select="$shipment/s0:SubShipmentCollection/s0:SubShipment">
                  <xsl:variable name="packingLines" select="s0:PackingLineCollection/s0:PackingLine[s0:ContainerLink/text()=$containerLink]" />

                  <xsl:if test="$packingLines">
                    <xsl:for-each select="$packingLines">
                      <xsl:for-each select="s0:UNDGCollection/s0:UNDG">
                        <ns0:dgx-emp>
                          <xsl:attribute name="classe">
                            <xsl:value-of select="s0:IMOClass/text()"/>
                          </xsl:attribute>

                          <xsl:attribute name="un">
                            <xsl:value-of select="s0:UNDGCode/text()"/>
                          </xsl:attribute>

                          <xsl:attribute name="qtelimitee">
                            <xsl:choose>
                              <xsl:when test="s0:PackedInLimitedQuantity/text()='true'">Y</xsl:when>
                              <xsl:when test="s0:PackedInLimitedQuantity/text()='false'">N</xsl:when>
                              <xsl:otherwise></xsl:otherwise>
                            </xsl:choose>
                          </xsl:attribute>

                          <xsl:call-template name="CreateAttribute">
                            <xsl:with-param name="name" select="'groupe-emb'" />
                            <xsl:with-param name="value" select="s0:PackingGroup/text()" />
                          </xsl:call-template>

                          <xsl:call-template name="CreateAttribute">
                            <xsl:with-param name="name" select="'etat-matiere'" />
                            <xsl:with-param name="value" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'UNDG State' , 'Output Code', s0:State/text())" />
                          </xsl:call-template>
                        </ns0:dgx-emp>
                      </xsl:for-each>
                    </xsl:for-each>
                  </xsl:if>
                </xsl:for-each>

                <xsl:variable name="portDuesAmt" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='PortDuesAmount']/s0:Value/text()"/>
                <xsl:if test="$portDuesAmt!='' and number($portDuesAmt) &gt; 0">
                  <ns0:drpo-emp>
                    <xsl:attribute name="montant">
                      <xsl:value-of select="$portDuesAmt"/>
                    </xsl:attribute>

                    <xsl:attribute name="devise">
                      <xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='PortDuesCurrency']/s0:Value/text()"/>
                    </xsl:attribute>

                    <xsl:attribute name="tiers">
                      <xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='PortDuesPayingParty']/s0:Value/text()"/>
                    </xsl:attribute>

                    <xsl:attribute name="per">
                      <xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='PortDuesPortCode']/s0:Value/text()"/>
                    </xsl:attribute>
                  </ns0:drpo-emp>
                </xsl:if>
              </ns0:empotage>
            </ns0:Request>
          </ns0:Messages>
        </ns0:MessageSet>
      </xsl:for-each>
    </ns0:Interchanges>
  </xsl:template>

  <xsl:template name="GetRegistrationNumberWithFallBack">
    <xsl:param name="addressType" />
    <xsl:param name="fallbackAddressType" />
    <xsl:param name="registrationNumberType"/>

    <xsl:variable name="RegNumber">
      <xsl:call-template name="GetRegistrationNumber">
        <xsl:with-param name="addressType" select="$addressType" />
        <xsl:with-param name="registrationNumberType" select="$registrationNumberType"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test="$RegNumber!=''">
        <xsl:value-of select="$RegNumber" />
      </xsl:when>
      <xsl:when test="$fallbackAddressType!=''">
        <xsl:call-template name="GetRegistrationNumber">
          <xsl:with-param name="addressType" select="$fallbackAddressType" />
          <xsl:with-param name="registrationNumberType" select="$registrationNumberType"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="GetRegistrationNumber">
    <xsl:param name="addressType" />
    <xsl:param name="registrationNumberType"/>

    <xsl:variable name="org" select="$shipment/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType=$addressType]" />
    <xsl:choose>
      <xsl:when test="$org">
        <xsl:value-of select="$org/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()=$registrationNumberType]/s0:Value/text()" />
      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="CreateAttribute_Date">
    <xsl:param name="name" />
    <xsl:param name="value" />

    <xsl:if test="$value!=''">
      <xsl:variable name="formattedDate" select="DateMapper:ConvertXmlDateString($value, 'dd/MM/yyyy HH:mm')" />
      <xsl:variable name="dateValue">
        <xsl:choose>
          <xsl:when test="substring-after($formattedDate, ' ') ='00:00'">
            <xsl:value-of select="substring-before($formattedDate, ' ')" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$formattedDate"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:if test="$dateValue!=''">
        <xsl:attribute name="{$name}">
          <xsl:value-of select="$dateValue" />
        </xsl:attribute>
      </xsl:if>
    </xsl:if>
  </xsl:template>

  <xsl:template name="CreateAttribute_Integer">
    <xsl:param name="name" />
    <xsl:param name="value" />

    <xsl:if test="$value!='' and number($value) and round($value)!=0">
      <xsl:attribute name="{$name}">
        <xsl:value-of select="$value" />
      </xsl:attribute>
    </xsl:if>
  </xsl:template>

  <xsl:template name="CreateAttribute">
    <xsl:param name="name" />
    <xsl:param name="value" />
    <xsl:param name="fallbackValue" select="''" />

    <xsl:variable name="attributeValue">
      <xsl:choose>
        <xsl:when test="$value!=''">
          <xsl:value-of select="$value"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$fallbackValue"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:if test="$attributeValue!=''">
      <xsl:attribute name="{$name}">
        <xsl:value-of select="$attributeValue" />
      </xsl:attribute>
    </xsl:if>
  </xsl:template>

  <xsl:template name="scelle-emp">
    <xsl:param name="num" />
    <xsl:param name="ori" />

    <xsl:if test="$num!=''">
      <ns0:scelle-emp>
        <xsl:attribute name="num">
          <xsl:value-of select="$num" />
        </xsl:attribute>

        <xsl:if test="$ori!=''">
          <xsl:attribute name="ori">
            <xsl:value-of select="CodeMapper:GetRecipientCode($serviceProvider, $serviceProvider, concat($serviceProvider, ' System Configuration'), 'Seal Party Type' , concat($serviceProvider, ' Code'), $ori)"/>
          </xsl:attribute>
        </xsl:if>
      </ns0:scelle-emp>
    </xsl:if>
  </xsl:template>

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[
public string user;

public void ParseClientRegistration(string inputText)
{
  user = string.Empty;
  if (!string.IsNullOrEmpty(inputText))
  {
    inputText = inputText.Replace(" ", "");
    if (inputText.Contains(","))
    {
      user = inputText.Split(new[] { ',' })[0];
    }
    else
    {
      user = inputText;
    }
  }
}

public string GetUser()
{
  return user;
}
]]>
  </msxsl:script>
</xsl:stylesheet>
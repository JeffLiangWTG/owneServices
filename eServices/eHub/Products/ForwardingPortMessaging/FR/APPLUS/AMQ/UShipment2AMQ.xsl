<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 CodeMapper ContextAccessor DataModelAccessor DateMapper userCSharp" version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2012/11/AMQ/1"
                xmlns:ns0="http://www.cargowise.com/Schemas/FPM/APPLUS/AMQ"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
                xmlns:DataModelAccessor="http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"
                xmlns:DateMapper="http://schemas.microsoft.com/BizTalk/2003/DateMapper"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes" />

  <xsl:template match="/">
    <xsl:apply-templates select="s0:UniversalShipment/s0:Shipment" />
  </xsl:template>

  <xsl:variable name="recipientID" select="ContextAccessor:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')" />
  <xsl:variable name="senderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')" />

  <xsl:variable name="serviceProvider" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Port Settings' , 'Name', $recipientID)" />
  <xsl:variable name="serviceProviderMSGID" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Port Settings' , 'MSGID', $recipientID)" />
  <xsl:variable name="serviceProviderID" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Port Settings' , 'ID', $recipientID)" />
  <xsl:variable name="msgPrefix"  select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Port Settings' , 'SubscriptionPrefix', $recipientID)" />

  <xsl:variable name="shipment" select="/s0:UniversalShipment/s0:Shipment" />
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
      <xsl:when test="$serviceProvider='SOGET'">SOA</xsl:when>
      <xsl:otherwise>CI5</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:template match="s0:UniversalShipment/s0:Shipment">
    <xsl:variable name="consolID" select="s0:DataContext/s0:DataSource/s0:Key/text()" />
    <xsl:variable name="purpose" select="s0:DataContext/s0:DocumentaryOverride/s0:Purpose/text()" />

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

    <xsl:variable name="InboxPK" select="ContextAccessor:GetContextProperty('InternalTrackingID', 'http://cargowise.com/ehub/tracking/2010/06')" />
    <xsl:variable name="FileName" select="ContextAccessor:SetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06', concat('AMQ_', $senderID, '_', $interchangeID))" />
    <xsl:variable name="EmailSubject" select="ContextAccessor:SetContextProperty('OverrideEmailSubject', 'http://cargowise.com/ehub/processing/2010/06', concat('FPM_', $serviceProvider, '_', $operationPortCode))"/>

    <xsl:variable name="destinationUser" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Service Provider Settings' , 'DestinationUser', $serviceProvider, $operationPortCode, $routingParty)" />
    <xsl:variable name="destinationParty" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Service Provider Settings' , 'DestinationParty', $serviceProvider, $operationPortCode, $routingParty)" />

    <xsl:variable name="cbkNumber" select="$shipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='BookingConfirmationCBK']/s0:Value/text()" />
    <xsl:variable name="isContainerized" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Container Mode' , 'Containerized', s0:ContainerMode/text())" />
    <xsl:variable name="ffwRef" select="s0:AdditionalReferenceCollection/s0:AdditionalReference[s0:Type/text()='FFW']/s0:ReferenceNumber/text()" />

    <xsl:if test="$newRecipientID!=$recipientID">
      <xsl:variable name="SetRecipientID" select="ContextAccessor:SetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties', $newRecipientID)"/>
    </xsl:if>

    <ns0:Interchanges>
      <xsl:attribute name="id">
        <xsl:value-of select="$interchangeID" />
      </xsl:attribute>
      <xsl:attribute name="from">
        <xsl:value-of select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Service Provider Settings' , 'SenderID', $serviceProvider, $operationPortCode, $routingParty)" />
      </xsl:attribute>
      <xsl:attribute name="to">
        <xsl:value-of select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Service Provider Settings' , 'RecipientID', $serviceProvider, $operationPortCode, $routingParty)" />
      </xsl:attribute>

      <xsl:for-each select="s0:ContainerCollection/s0:Container">
        <xsl:variable name="containerNumber" select="s0:ContainerNumber/text()"/>
        <xsl:variable name="consolContainerNumber" select="concat($consolID, '_', $containerNumber)" />
        <xsl:variable name="messageReference" select="concat($consolContainerNumber, '_AMQ')"/>

        <xsl:variable name="previousMessageIdentifier" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $serviceProvider, '@recipientId', $senderID , '@ST_ID',  $serviceProviderMSGID , '@value', $messageReference, '@referenceType', 'MessageReference')"/>
        <xsl:variable name="messageIdentifier">
          <xsl:choose>
            <xsl:when test="$previousMessageIdentifier!=''">
              <xsl:value-of select="$previousMessageIdentifier"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:variable name="messageSetID" select="CodeMapper:CallActionProcedureHelper('GetCounterValue','@value','@name','CargoWise.eHub.Products.ForwardingPortMessaging.FR.APPLUS','@maxlength','14')" />
              <xsl:variable name="formattedMessageIdentifier" select='concat($msgPrefix, format-number($messageSetID, "0000000000"))' />
              <xsl:variable name="SubscribeJobNumber1" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $formattedMessageIdentifier, $consolID, 'JobNumber')" />
              <xsl:variable name="SubscribeJobNumber2" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $consolID, $formattedMessageIdentifier, $formattedMessageIdentifier)" />

              <xsl:variable name="SubscribeMessageReference" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageReference, $formattedMessageIdentifier, 'MessageReference')" />

              <!--the following subscription will not change after ORG is sent-->
              <xsl:variable name="SubscribeRequestID" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $formattedMessageIdentifier, $containerNumber, 'RequestID')" />
              <xsl:variable name="SubscribeContainerNumber" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $formattedMessageIdentifier, $containerNumber, 'ContainerNumber')" />
              <xsl:variable name="SubscribeDocumentName" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $formattedMessageIdentifier, $shipment/s0:DataContext/s0:DocumentaryOverride/s0:DocumentName/text(), 'DocumentName')" />

              <xsl:value-of select="$formattedMessageIdentifier"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <xsl:variable name="SubscribeInboxPK" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $InboxPK, concat($interchangeID, '_', $messageIdentifier))" />
        <xsl:variable name="ShipmentTypeSubscription" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageIdentifier, $shipment/s0:DataContext/s0:DataSource/s0:Type/text(), 'ForwardingType')" />
        <xsl:variable name="SubscribeActionPurpose" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageIdentifier, $purpose, 'Purpose')" />
        <xsl:variable name="SubscribeOperationPort" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageIdentifier, $operationPortCode, 'OperationPort')" />
        <xsl:variable name="previousContainerRank" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $serviceProvider, '@recipientId', $senderID, '@ST_ID', $serviceProviderMSGID, '@value', $consolContainerNumber, '@referenceType', 'ContainerRank')" />

        <xsl:variable name="containerRank">
          <xsl:choose>
            <xsl:when test="$previousContainerRank!=''">
              <xsl:value-of select="$previousContainerRank" />
            </xsl:when>
            <xsl:otherwise>
              <xsl:variable name="subscribedContainerRankCounter" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $serviceProvider, '@recipientId', $senderID, '@ST_ID', $serviceProviderMSGID, '@value', $consolID, '@referenceType', 'ContainerRankCounter')"/>
              <xsl:variable name="newContainerRank">
                <xsl:choose>
                  <xsl:when test="$subscribedContainerRankCounter=''">
                    <xsl:value-of select="1"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="number($subscribedContainerRankCounter) + 1" />
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:variable>
              <xsl:variable name="subscribeContainerRankCounter" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $consolID, $newContainerRank, 'ContainerRankCounter')" />
              <xsl:variable name="SubscribeContainerRank" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $consolContainerNumber, $newContainerRank, 'ContainerRank')" />
              <xsl:value-of select="$newContainerRank"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <ns0:MessageSet>
          <xsl:attribute name="id">
            <xsl:value-of select="$messageIdentifier" />
          </xsl:attribute>
          <xsl:attribute name="icid">
            <xsl:value-of select="$interchangeID" />
          </xsl:attribute>
          <xsl:attribute name="date">
            <xsl:value-of select="DateMapper:ConvertXmlDateString($shipment/s0:DataContext/s0:Workflow/s0:TriggerDate/text(), 'dd/MM/yyyy HH:mm:ss')" />
          </xsl:attribute>

          <ns0:Destinataire>
            <xsl:attribute name="user">
              <xsl:value-of select="$destinationUser" />
            </xsl:attribute>
            <xsl:attribute name="tiersProf">
              <xsl:value-of select="$destinationParty" />
            </xsl:attribute>
          </ns0:Destinataire>

          <ns0:Emetteur>
            <xsl:attribute name="user">
              <xsl:value-of select="$applusID" />
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

              <xsl:attribute name="type">AMQ</xsl:attribute>

              <ns0:amq>
                <ns0:reference-amq>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'cbk'" />
                    <xsl:with-param name="value" select="$cbkNumber" />
                  </xsl:call-template>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'rang'" />
                    <xsl:with-param name="value" select="$containerRank" />
                  </xsl:call-template>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'extcbk'" />
                    <xsl:with-param name="value" select="$shipment/s0:BookingConfirmationReference/text()" />
                  </xsl:call-template>

                  <xsl:variable name="extamqValue">
                    <xsl:choose>
                      <xsl:when  test="$isContainerized='true'">
                        <xsl:value-of select="concat($ffwRef, '_', $containerNumber)" />
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="$ffwRef" />
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:variable>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'extamq'" />
                    <xsl:with-param name="value" select="$extamqValue" />
                  </xsl:call-template>

                  <xsl:attribute name="reco">IMP</xsl:attribute>
                  <xsl:attribute name="statut">
                    <xsl:choose>
                      <xsl:when test="$purpose='ORG' or $purpose='AMD'">VAL</xsl:when>
                      <xsl:when test="$purpose='WTH'">INV</xsl:when>
                      <xsl:otherwise></xsl:otherwise>
                    </xsl:choose>
                  </xsl:attribute>
                  <ns0:commentaires>
                    <xsl:value-of select="$shipment/s0:NoteCollection/s0:Note[s0:Description/text()='Carrier Booking Notes']/s0:NoteText/text()" />
                  </ns0:commentaires>
                </ns0:reference-amq>

                <ns0:voyage-amq>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'otc'" />
                    <xsl:with-param name="value" select="$shipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='OTCReference']/s0:Value/text()" />
                  </xsl:call-template>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'atp'" />
                    <xsl:with-param name="value" select="$shipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ATPReference']/s0:Value/text()" />
                  </xsl:call-template>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'refotc'" />
                    <xsl:with-param name="value" select="$shipment/s0:VoyageFlightNo/text()" />
                  </xsl:call-template>

                  <ns0:lieux-amq>
                    <xsl:call-template name="CreateAttribute">
                      <xsl:with-param name="name" select="'fin'" />
                      <xsl:with-param name="value" select="$shipment/s0:PortOfOrigin/text()" />
                    </xsl:call-template>
                    <xsl:call-template name="CreateAttribute">
                      <xsl:with-param name="name" select="'tbt'" />
                      <xsl:with-param name="value" select="$shipment/s0:PortFirstForeign/text()" />
                    </xsl:call-template>
                    <xsl:call-template name="CreateAttribute">
                      <xsl:with-param name="name" select="'dest'" />
                      <xsl:with-param name="value" select="$shipment/s0:PortOfDestination/text()" />
                    </xsl:call-template>
                  </ns0:lieux-amq>
                </ns0:voyage-amq>

                <ns0:lieu-amq>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'zone'" />
                    <xsl:with-param name="value" select="$shipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='PortArea']/s0:Value/text()" />
                  </xsl:call-template>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'manut'" />
                    <xsl:with-param name="value">
                      <xsl:call-template name="GetRegistrationNumber">
                        <xsl:with-param name="addressType" select="'DepartureCTOAddress'" />
                        <xsl:with-param name="registrationNumberType" select="$registrationNumberType"/>
                      </xsl:call-template>
                    </xsl:with-param>
                  </xsl:call-template>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'lieu'" />
                    <xsl:with-param name="value" select="$shipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='PortLocation']/s0:Value/text()" />
                  </xsl:call-template>
                </ns0:lieu-amq>

                <ns0:tiers-amq>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'amq'" />
                    <xsl:with-param name="value">
                      <xsl:call-template name="GetRegistrationNumber">
                        <xsl:with-param name="addressType" select="'CurrentUser'" />
                        <xsl:with-param name="registrationNumberType" select="$registrationNumberType"/>
                      </xsl:call-template>
                    </xsl:with-param>
                  </xsl:call-template>

                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'afret'" />
                    <xsl:with-param name="value">
                      <xsl:call-template name="GetRegistrationNumber">
                        <xsl:with-param name="addressType" select="'ShippingLineAddress'" />
                        <xsl:with-param name="registrationNumberType" select="$registrationNumberType"/>
                      </xsl:call-template>
                    </xsl:with-param>
                  </xsl:call-template>
                </ns0:tiers-amq>

                <xsl:variable name="isControlledAtmosphere">
                  <xsl:choose>
                    <xsl:when test="s0:IsControlledAtmosphere/text()='true'">Y</xsl:when>
                    <xsl:otherwise>N</xsl:otherwise>
                  </xsl:choose>
                </xsl:variable>
                <ns0:equipement-amq>
                  <xsl:attribute name="id">
                    <xsl:value-of select="$containerNumber" />
                  </xsl:attribute>

                  <xsl:variable name="serviceProviderContainerISO" select="CodeMapper:GetRecipientCode($serviceProvider, $serviceProvider, concat($serviceProvider, ' System Configuration'), 'ContainerTypeToISOCode' , concat($serviceProvider, ' Code'),  s0:ContainerType/s0:ISOCode/text())"/>
                  <xsl:variable name="ContainerTypeMappingCode">
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
                    <xsl:with-param name="value" select="$ContainerTypeMappingCode" />
                  </xsl:call-template>

                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'poids'" />
                    <xsl:with-param name="value" select="round(s0:GrossWeight/text())" />
                  </xsl:call-template>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'tare'" />
                    <xsl:with-param name="value" select="round(s0:TareWeight/text())" />
                  </xsl:call-template>

                  <xsl:attribute name="vide">
                    <xsl:choose>
                      <xsl:when test="s0:IsEmptyContainer/text()='true'">Y</xsl:when>
                      <xsl:otherwise>N</xsl:otherwise>
                    </xsl:choose>
                  </xsl:attribute>

                  <xsl:attribute name="frigo">
                    <xsl:value-of select="$isControlledAtmosphere" />
                  </xsl:attribute>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'dgx'" />
                    <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='HazardousCargo']/s0:Value/text()" />
                  </xsl:call-template>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'polluant'" />
                    <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='MarinePollutant']/s0:Value/text()" />
                  </xsl:call-template>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'dim'" />
                    <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='OversizeContainer']/s0:Value/text()" />
                  </xsl:call-template>

                  <xsl:attribute name="haulage">
                    <xsl:choose>
                      <xsl:when test="substring-before(s0:DeliveryMode/text(), '/')='CFS'">C</xsl:when>
                      <xsl:when test="substring-before(s0:DeliveryMode/text(), '/')='CY'">M</xsl:when>
                      <xsl:otherwise></xsl:otherwise>
                    </xsl:choose>
                  </xsl:attribute>

                  <xsl:call-template name="CreateAttribute_Integer">
                    <xsl:with-param name="name" select="'oh'" />
                    <xsl:with-param name="value" select="round(s0:OverhangHeight/text())" />
                  </xsl:call-template>
                  <xsl:call-template name="CreateAttribute_Integer">
                    <xsl:with-param name="name" select="'olf'" />
                    <xsl:with-param name="value" select="round(s0:OverhangFront/text())" />
                  </xsl:call-template>
                  <xsl:call-template name="CreateAttribute_Integer">
                    <xsl:with-param name="name" select="'olb'" />
                    <xsl:with-param name="value" select="round(s0:OverhangBack/text())" />
                  </xsl:call-template>
                  <xsl:call-template name="CreateAttribute_Integer">
                    <xsl:with-param name="name" select="'owl'" />
                    <xsl:with-param name="value" select="round(s0:OverhangLeft/text())" />
                  </xsl:call-template>
                  <xsl:call-template name="CreateAttribute_Integer">
                    <xsl:with-param name="name" select="'owr'" />
                    <xsl:with-param name="value" select="round(s0:OverhangRight/text())" />
                  </xsl:call-template>

                  <xsl:if test="$isControlledAtmosphere='Y'">
                    <xsl:call-template name="CreateAttribute">
                      <xsl:with-param name="name" select="'min'" />
                      <xsl:with-param name="value">
                        <xsl:choose>
                          <xsl:when test="$shipment/s0:RequiredTemperatureMinimum/text()!=''">
                            <xsl:value-of select="round($shipment/s0:RequiredTemperatureMinimum/text())" />
                          </xsl:when>
                          <xsl:otherwise>
                            <xsl:value-of select="round(s0:SetPointTemp/text())" />
                          </xsl:otherwise>
                        </xsl:choose>
                      </xsl:with-param>
                    </xsl:call-template>

                    <xsl:call-template name="CreateAttribute">
                      <xsl:with-param name="name" select="'max'" />
                      <xsl:with-param name="value">
                        <xsl:choose>
                          <xsl:when test="$shipment/s0:RequiredTemperatureMaximum/text()!=''">
                            <xsl:value-of select="round($shipment/s0:RequiredTemperatureMaximum/text())" />
                          </xsl:when>
                          <xsl:otherwise>
                            <xsl:value-of select="round(s0:SetPointTemp/text())" />
                          </xsl:otherwise>
                        </xsl:choose>
                      </xsl:with-param>
                    </xsl:call-template>

                    <xsl:variable name="tempUnit">
                      <xsl:choose>
                        <xsl:when test="$shipment/s0:RequiredTemperatureUnit/text()!=''">
                          <xsl:value-of select="$shipment/s0:RequiredTemperatureUnit/text()" />
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:value-of select="s0:SetPointTempUnit/text()" />
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>
                    <xsl:attribute name="unite">
                      <xsl:choose>
                        <xsl:when test="$tempUnit='C'">CEL</xsl:when>
                        <xsl:when test="$tempUnit='F'">FAH</xsl:when>
                        <xsl:otherwise></xsl:otherwise>
                      </xsl:choose>
                    </xsl:attribute>
                  </xsl:if>

                  <xsl:variable name="seal" select="s0:Seal/text()" />
                  <xsl:if test="$seal!=''">
                    <ns0:scelle-amq>
                      <xsl:attribute name="num">
                        <xsl:value-of select="$seal" />
                      </xsl:attribute>
                      <xsl:attribute name="ori">
                        <xsl:value-of select="CodeMapper:GetRecipientCode($serviceProvider, $serviceProvider, concat($serviceProvider, ' System Configuration'), 'Seal Party Type' , concat($serviceProvider, ' Code'), s0:SealPartyType/text())"/>
                      </xsl:attribute>
                    </ns0:scelle-amq>
                  </xsl:if>

                  <xsl:variable name="secondSeal" select="s0:SecondSeal/text()" />
                  <xsl:if test="$secondSeal!=''">
                    <ns0:scelle-amq>
                      <xsl:attribute name="num">
                        <xsl:value-of select="$secondSeal" />
                      </xsl:attribute>
                      <xsl:attribute name="ori">
                        <xsl:value-of select="CodeMapper:GetRecipientCode($serviceProvider, $serviceProvider, concat($serviceProvider, ' System Configuration'), 'Seal Party Type' , concat($serviceProvider, ' Code'), s0:SecondSealPartyType/text())"/>
                      </xsl:attribute>
                    </ns0:scelle-amq>
                  </xsl:if>

                  <xsl:variable name="thirdSeal" select="s0:ThirdSeal/text()" />
                  <xsl:if test="$thirdSeal!=''">
                    <ns0:scelle-amq>
                      <xsl:attribute name="num">
                        <xsl:value-of select="$thirdSeal" />
                      </xsl:attribute>
                      <xsl:attribute name="ori">
                        <xsl:value-of select="CodeMapper:GetRecipientCode($serviceProvider, $serviceProvider, concat($serviceProvider, ' System Configuration'), 'Seal Party Type' , concat($serviceProvider, ' Code'), s0:ThirdSealPartyType/text())"/>
                      </xsl:attribute>
                    </ns0:scelle-amq>
                  </xsl:if>
                </ns0:equipement-amq>

                <xsl:variable name="containerLink" select="s0:Link/text()"/>
                <xsl:for-each select="$shipment/s0:PackingLineCollection/s0:PackingLine[s0:ContainerLink/text()=$containerLink]">
                  <ns0:lmarchandise-amq>
                    <xsl:call-template name="CreateAttribute">
                      <xsl:with-param name="name" select="'nb'" />
                      <xsl:with-param name="value" select="s0:PackQty/text()" />
                    </xsl:call-template>

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

                    <xsl:call-template name="CreateAttribute">
                      <xsl:with-param name="name" select="'code'" />
                      <xsl:with-param name="value" select="$packTypeMappingCode" />
                    </xsl:call-template>
                    <xsl:call-template name="CreateAttribute">
                      <xsl:with-param name="name" select="'poids'" />
                      <xsl:with-param name="value" select="round(s0:Weight/text())" />
                    </xsl:call-template>

                    <xsl:variable name="requiresTemperatureControl">
                      <xsl:choose>
                        <xsl:when test="s0:RequiresTemperatureControl/text()='true'">Y</xsl:when>
                        <xsl:otherwise>N</xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>

                    <xsl:attribute name="frigo">
                      <xsl:value-of select="$requiresTemperatureControl"/>
                    </xsl:attribute>
                    <xsl:attribute name="dgx">
                      <xsl:choose>
                        <xsl:when test="s0:UNDGCollection/s0:UNDG[s0:UNDGCode/text()!='']">Y</xsl:when>
                        <xsl:otherwise>N</xsl:otherwise>
                      </xsl:choose>
                    </xsl:attribute>
                    <xsl:attribute name="polluant">
                      <xsl:choose>
                        <xsl:when test="s0:UNDGCollection/s0:UNDG/s0:MarinePollutant/text()='true'">Y</xsl:when>
                        <xsl:otherwise>N</xsl:otherwise>
                      </xsl:choose>
                    </xsl:attribute>

                    <xsl:if test="$requiresTemperatureControl='Y'">
                      <xsl:call-template name="CreateAttribute">
                        <xsl:with-param name="name" select="'min'" />
                        <xsl:with-param name="value" select="round(s0:RequiredTemperatureMinimum/text())" />
                      </xsl:call-template>
                      <xsl:call-template name="CreateAttribute">
                        <xsl:with-param name="name" select="'max'" />
                        <xsl:with-param name="value" select="round(s0:RequiredTemperatureMaximum/text())" />
                      </xsl:call-template>

                      <xsl:variable name="tempUnitCode" select="s0:RequiredTemperatureUnit/s0:Code/text()" />
                      <xsl:attribute name="unite">
                        <xsl:choose>
                          <xsl:when test="$tempUnitCode='C'">CEL</xsl:when>
                          <xsl:when test="$tempUnitCode='F'">FAH</xsl:when>
                          <xsl:otherwise></xsl:otherwise>
                        </xsl:choose>
                      </xsl:attribute>
                    </xsl:if>

                    <ns0:designation>
                      <xsl:value-of select="s0:DetailedDescription/text()"/>
                    </ns0:designation>
                    <ns0:marques>
                      <xsl:value-of select="substring(s0:MarksAndNos/text(), 1, 350)" />
                    </ns0:marques>

                    <xsl:for-each select="s0:UNDGCollection/s0:UNDG">
                      <ns0:dgx-amq>
                        <xsl:call-template name="CreateAttribute">
                          <xsl:with-param name="name" select="'classe'" />
                          <xsl:with-param name="value" select="s0:IMOClass/text()" />
                        </xsl:call-template>
                        <xsl:call-template name="CreateAttribute">
                          <xsl:with-param name="name" select="'un'" />
                          <xsl:with-param name="value" select="s0:UNDGCode/text()" />
                        </xsl:call-template>

                        <xsl:attribute name="qtelimitee">
                          <xsl:choose>
                            <xsl:when test="s0:PackedInLimitedQuantity/text()='true'">Y</xsl:when>
                            <xsl:otherwise>N</xsl:otherwise>
                          </xsl:choose>
                        </xsl:attribute>

                        <xsl:call-template name="CreateAttribute">
                          <xsl:with-param name="name" select="'etat-matiere'" />
                          <xsl:with-param name="value" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'UNDG State' , 'Output Code', s0:State/text())"/>
                        </xsl:call-template>

                        <xsl:variable name="packingGroup" select="s0:PackingGroup/text()"/>
                        <xsl:call-template name="CreateAttribute">
                          <xsl:with-param name="name" select="'groupe-emb'" />
                          <xsl:with-param name="value">
                            <xsl:choose>
                              <xsl:when test="$packingGroup='I'">1</xsl:when>
                              <xsl:when test="$packingGroup='II'">2</xsl:when>
                              <xsl:when test="$packingGroup='III'">3</xsl:when>
                            </xsl:choose>
                          </xsl:with-param>
                        </xsl:call-template>
                      </ns0:dgx-amq>
                    </xsl:for-each>
                  </ns0:lmarchandise-amq>
                </xsl:for-each>

                <xsl:variable name="regNumber">
                  <xsl:call-template name="GetRegistrationNumber">
                    <xsl:with-param name="addressType" select="'DepartureCFSLocalTransportAddress'" />
                    <xsl:with-param name="registrationNumberType" select="$registrationNumberType"/>
                  </xsl:call-template>
                </xsl:variable>
                <xsl:if test="$regNumber != ''">
                  <ns0:acheminement-amq>
                    <xsl:call-template name="CreateAttribute">
                      <xsl:with-param name="name" select="'sic'" />
                      <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TransportMode']/s0:Value/text()" />
                    </xsl:call-template>

                    <xsl:call-template name="CreateAttribute">
                      <xsl:with-param name="name" select="'code'" />
                      <xsl:with-param name="value" select="$regNumber" />
                    </xsl:call-template>

                    <xsl:call-template name="CreateAttribute">
                      <xsl:with-param name="name" select="'nom'" />
                      <xsl:with-param name="value" select="$shipment/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='DepartureCFSLocalTransportAddress']/s0:CompanyName/text()" />
                      <xsl:with-param name="maxLength" select="75" />
                    </xsl:call-template>

                    <xsl:call-template name="CreateAttribute_Date">
                      <xsl:with-param name="name" select="'date'" />
                      <xsl:with-param name="value" select="s0:DepartureSlotDateTime/text()"/>
                    </xsl:call-template>

                  </ns0:acheminement-amq>
                </xsl:if>
              </ns0:amq>
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
    <xsl:param name="maxLength" select="'0'"/>

    <xsl:if test="$value!=''">
      <xsl:attribute name="{$name}">
        <xsl:choose>
          <xsl:when test="number($maxLength) &gt; 0">
            <xsl:value-of select="substring($value, 1, $maxLength)" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$value" />
          </xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
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
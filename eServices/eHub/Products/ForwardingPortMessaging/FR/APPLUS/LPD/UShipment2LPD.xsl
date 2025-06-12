<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 CodeMapper ContextAccessor DataModelAccessor DateMapper userCSharp" version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2012/11/LPD/1"
                xmlns:ns0="http://www.cargowise.com/Schemas/FPM/APPLUS/LPD"
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

  <xsl:template match="s0:UniversalShipment/s0:Shipment">
    <xsl:variable name="consolID" select="s0:DataContext/s0:DataSource/s0:Key/text()" />
    <xsl:variable name="purpose" select="s0:DataContext/s0:DocumentaryOverride/s0:Purpose/text()" />
    <xsl:variable name="cbkNumber" select="$shipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='BookingConfirmationCBK']/s0:Value/text()" />

    <xsl:variable name="parseClientRegistration" select="userCSharp:ParseClientRegistration(DataModelAccessor:GetClientRegistrationCode($senderID, $operationPortCode, $serviceProvider))" />
    <xsl:variable name="applusID" select="userCSharp:GetUser()" />
    <xsl:variable name="applusTierProf">
      <xsl:call-template name="GetRegistrationNumber">
        <xsl:with-param name="addressType" select="'CurrentUser'" />
        <xsl:with-param name="registrationNumberType" select="$registrationNumberType"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="interchangeID" select="CodeMapper:CallActionProcedureHelper('GetCounterValue','@value','@name','CargoWise.eHub.Products.ForwardingPortMessaging.FR.APPLUS.Interchange','@maxlength','14')" />

    <xsl:variable name="subscribeClientID" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderID, $serviceProvider, $senderID, concat($applusID, ',', $applusTierProf))" />
    <xsl:variable name="InboxPK" select="ContextAccessor:GetContextProperty('InternalTrackingID', 'http://cargowise.com/ehub/tracking/2010/06')" />
    <xsl:variable name="FileName" select="ContextAccessor:SetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06', concat('LPD_', $senderID, '_', $interchangeID))" />
    <xsl:variable name="EmailSubject" select="ContextAccessor:SetContextProperty('OverrideEmailSubject', 'http://cargowise.com/ehub/processing/2010/06', concat('FPM_', $serviceProvider, '_', $operationPortCode))"/>

    <xsl:if test="$newRecipientID!=$recipientID">
      <xsl:variable name="SetRecipientID" select="ContextAccessor:SetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties', $newRecipientID)"/>
    </xsl:if>

    <xsl:variable name="destinationUser" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Service Provider Settings' , 'DestinationUser', $serviceProvider, $operationPortCode, $routingParty)" />
    <xsl:variable name="destinationParty" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Service Provider Settings' , 'DestinationParty', $serviceProvider, $operationPortCode, $routingParty)" />

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

      <xsl:variable name="ffwReference" select="s0:AdditionalReferenceCollection/s0:AdditionalReference[s0:Type/text()='FFW']/s0:ReferenceNumber/text()" />

      <xsl:for-each select="s0:ContainerCollection/s0:Container">
        <xsl:variable name="containerNumber" select="s0:ContainerNumber/text()"/>
        <xsl:variable name="messageReference" select="concat($consolID, '_', $containerNumber, '_LPD')"/>

        <xsl:variable name="previousMessageIdentifier" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $serviceProvider, '@recipientId', $senderID , '@ST_ID',  $serviceProviderMSGID , '@value', $messageReference, '@referenceType', 'MessageReference')"/>
        <xsl:variable name="messageIdentifier">
          <xsl:choose>
            <xsl:when test="$previousMessageIdentifier!=''">
              <xsl:value-of select="$previousMessageIdentifier"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:variable name="messageSetID" select="CodeMapper:CallActionProcedureHelper('GetCounterValue','@value','@name','CargoWise.eHub.Products.ForwardingPortMessaging.FR.APPLUS','@maxlength','14')" />
              <xsl:variable name="formattedMessageIdentifier" select="concat($msgPrefix, format-number($messageSetID, '0000000000'))" />

              <xsl:variable name="SubscribeJobNumber1" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $formattedMessageIdentifier, $consolID, 'JobNumber')" />
              <xsl:variable name="SubscribeJobNumber2" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $consolID, $formattedMessageIdentifier, $formattedMessageIdentifier)" />

              <xsl:variable name="SubscribeMessageReference" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageReference, $formattedMessageIdentifier, 'MessageReference')" />

              <!--the following subscription will not change after ORG is sent-->
              <xsl:variable name="SubscribeRequestID" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $formattedMessageIdentifier, $containerNumber, 'RequestID')" />
              <xsl:variable name="SubscribeContainerNumber" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $formattedMessageIdentifier, $containerNumber, 'ContainerNumber')" />
              <xsl:variable name="SubscribeDocumentName" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $formattedMessageIdentifier, $shipment/s0:DataContext/s0:DocumentaryOverride/s0:DocumentName/text(), 'DocumentName')" />

              <xsl:value-of select="$formattedMessageIdentifier" />
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <xsl:variable name="SubscribeInboxPK" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $InboxPK, concat($interchangeID, '_', $messageIdentifier))" />
        <xsl:variable name="ForwardingTypeSubscription" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageIdentifier, $shipment/s0:DataContext/s0:DataSource/s0:Type/text(), 'ForwardingType')" />
        <xsl:variable name="SubscribeActionPurpose" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageIdentifier, $purpose, 'Purpose')" />
        <xsl:variable name="SubscribeOperationPort" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageIdentifier, $operationPortCode, 'OperationPort')" />

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
              <xsl:attribute name="type">LPD</xsl:attribute>

              <ns0:liste-depotage>
                <ns0:reference-dep>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'ext-prop'" />
                    <xsl:with-param name="value" select="concat($ffwReference, '_', $containerNumber)" />
                  </xsl:call-template>

                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'ext-gest'" />
                    <xsl:with-param name="value" select="concat($ffwReference, '_', $containerNumber)" />
                  </xsl:call-template>

                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'date-arr'" />
                    <xsl:with-param name="value" select="DateMapper:ConvertXmlDateString($shipment/s0:DateCollection/s0:Date[s0:Type/text()='Arrival']/s0:Value/text(), 'dd/MM/yyyy HH:mm:00')" />
                  </xsl:call-template>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'statut'" />
                    <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='LPDStatus']/s0:Value/text()" />
                  </xsl:call-template>

                  <ns0:commentaire-dep>
                    <xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='UnpackingNotes']/s0:Value/text()" />
                  </ns0:commentaire-dep>
                </ns0:reference-dep>

                <ns0:voyage-dep>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'atp'" />
                    <xsl:with-param name="value" select="$shipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ATPReference']/s0:Value/text()" />
                  </xsl:call-template>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'otc'" />
                    <xsl:with-param name="value" select="$shipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='OTCReference']/s0:Value/text()" />
                  </xsl:call-template>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'refotc'" />
                    <xsl:with-param name="value" select="$shipment/s0:VoyageFlightNo/text()" />
                  </xsl:call-template>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'agfret'" />
                    <xsl:with-param name="value">
                      <xsl:call-template name="GetRegistrationNumber">
                        <xsl:with-param name="addressType" select="'ShippingLineAddress'"/>
                        <xsl:with-param name="registrationNumberType" select="$registrationNumberType" />
                      </xsl:call-template>
                    </xsl:with-param>
                  </xsl:call-template>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'pch'" />
                    <xsl:with-param name="value" select="$shipment/s0:PortOfOrigin/text()" />
                  </xsl:call-template>
                </ns0:voyage-dep>

                <ns0:lieux-dep>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'zone'" />
                    <xsl:with-param name="value" select="$shipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='PortArea']/s0:Value/text()" />
                  </xsl:call-template>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'lieu'" />
                    <xsl:with-param name="value" select="$shipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='PortLocation']/s0:Value/text()" />
                  </xsl:call-template>
                </ns0:lieux-dep>

                <ns0:tiers-dep>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'creat'" />
                    <xsl:with-param name="value">
                      <xsl:call-template name="GetRegistrationNumber">
                        <xsl:with-param name="addressType" select="'CurrentUser'"/>
                        <xsl:with-param name="registrationNumberType" select="$registrationNumberType" />
                      </xsl:call-template>
                    </xsl:with-param>
                  </xsl:call-template>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'prop'" />
                    <xsl:with-param name="value">
                      <xsl:call-template name="GetRegistrationNumber">
                        <xsl:with-param name="addressType" select="'ReceivingForwarderAddress'"/>
                        <xsl:with-param name="registrationNumberType" select="$registrationNumberType" />
                      </xsl:call-template>
                    </xsl:with-param>
                  </xsl:call-template>
                </ns0:tiers-dep>

                <ns0:equipement-dep>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'id'" />
                    <xsl:with-param name="value" select="$containerNumber" />
                  </xsl:call-template>

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
                    <xsl:with-param name="value" select="s0:GrossWeight/text()" />
                  </xsl:call-template>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'tare'" />
                    <xsl:with-param name="value" select="s0:TareWeight/text()" />
                  </xsl:call-template>

                  <xsl:variable name="containerLink" select="s0:Link/text()" />
                  <xsl:for-each select="$shipment/s0:SubShipmentCollection/s0:SubShipment">
                    <xsl:variable name="subShipment" select="." />

                    <xsl:for-each select="s0:PackingLineCollection/s0:PackingLine[s0:ContainerLink/text()=$containerLink]">
                      <xsl:variable name="commodityLineRef" select="$subShipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CommodityLineRef']/s0:Value/text()" />
                      <xsl:variable name="waybillNumber" select="$subShipment/s0:WayBillNumber/text()" />

                      <ns0:marchandise-dep>
                        <xsl:call-template name="CreateAttribute">
                          <xsl:with-param name="name" select="'ext-doc'" />
                          <xsl:with-param name="value" select="$waybillNumber" />
                        </xsl:call-template>
                        <xsl:call-template name="CreateAttribute">
                          <xsl:with-param name="name" select="'ori'" />
                          <xsl:with-param name="value" select="$subShipment/s0:PortOfOrigin/text()" />
                        </xsl:call-template>
                        <xsl:call-template name="CreateAttribute">
                          <xsl:with-param name="name" select="'last'" />
                          <xsl:with-param name="value" select="$shipment/s0:PortFirstForeign/text()" />
                        </xsl:call-template>

                        <xsl:call-template name="CreateAttribute">
                          <xsl:with-param name="name" select="'ref'" />
                          <xsl:with-param name="value" select="$subShipment/s0:AdditionalReferenceCollection/s0:AdditionalReference[s0:Type/text()='FFW']/s0:ReferenceNumber/text()" />
                        </xsl:call-template>

                        <xsl:call-template name="CreateAttribute">
                          <xsl:with-param name="name" select="'rec'" />
                          <xsl:with-param name="value">
                            <xsl:call-template name="GetRegistrationNumberSubShipment">
                              <xsl:with-param name="addressType" select="'DeliveryAgent'"/>
                              <xsl:with-param name="registrationNumberType" select="$registrationNumberType" />
                              <xsl:with-param name="subShipment" select="$subShipment" />
                            </xsl:call-template>
                          </xsl:with-param>
                        </xsl:call-template>

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
                          <xsl:with-param name="value" select="s0:Weight/text()" />
                        </xsl:call-template>
                        <xsl:call-template name="CreateAttribute">
                          <xsl:with-param name="name" select="'vol'" />
                          <xsl:with-param name="value" select="s0:Volume/text()" />
                        </xsl:call-template>

                        <xsl:variable name="requiresTemperatureControl">
                          <xsl:choose>
                            <xsl:when test="$subShipment/s0:RequiresTemperatureControl/text()='true'">Y</xsl:when>
                            <xsl:otherwise>N</xsl:otherwise>
                          </xsl:choose>
                        </xsl:variable>
                        <xsl:call-template name="CreateAttribute">
                          <xsl:with-param name="name" select="'frigo'" />
                          <xsl:with-param name="value" select="$requiresTemperatureControl" />
                        </xsl:call-template>

                        <xsl:call-template name="CreateAttribute">
                          <xsl:with-param name="name" select="'dgx'" />
                          <xsl:with-param name="value" select="$subShipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='HazardousCargo']/s0:Value/text()" />
                        </xsl:call-template>

                        <xsl:if test="$requiresTemperatureControl='Y'">
                          <xsl:call-template name="CreateAttribute">
                            <xsl:with-param name="name" select="'min'" />
                            <xsl:with-param name="value" select="round($subShipment/s0:RequiredTemperatureMinimum/text())" />
                          </xsl:call-template>
                          <xsl:call-template name="CreateAttribute">
                            <xsl:with-param name="name" select="'max'" />
                            <xsl:with-param name="value" select="round($subShipment/s0:RequiredTemperatureMaximum/text())" />
                          </xsl:call-template>

                          <xsl:variable name="tempUnitCode" select="$subShipment/s0:RequiredTemperatureUnit/text()" />
                          <xsl:attribute name="unite">
                            <xsl:choose>
                              <xsl:when test="$tempUnitCode='C'">CEL</xsl:when>
                              <xsl:when test="$tempUnitCode='F'">FAH</xsl:when>
                              <xsl:otherwise></xsl:otherwise>
                            </xsl:choose>
                          </xsl:attribute>
                        </xsl:if>

                        <ns0:description-dep>
                          <xsl:value-of select="$subShipment/s0:GoodsDescription/text()" />
                        </ns0:description-dep>
                        <ns0:marques-dep>
                          <xsl:value-of select="substring($subShipment/s0:NoteCollection/s0:Note[s0:Description/text()='Marks &amp; Numbers']/s0:NoteText/text(), 1, 350)" />
                        </ns0:marques-dep>
                        <ns0:commentaire-dep>
                          <xsl:value-of select="$subShipment/s0:NoteCollection/s0:Note[s0:Description/text()='Goods Handling Notes']/s0:NoteText/text()" />
                        </ns0:commentaire-dep>

                        <xsl:for-each select="s0:UNDGCollection/s0:UNDG">
                          <ns0:dgx-dep>
                            <xsl:call-template name="CreateAttribute">
                              <xsl:with-param name="name" select="'classe'" />
                              <xsl:with-param name="value" select="s0:IMOClass/text()" />
                            </xsl:call-template>
                            <xsl:call-template name="CreateAttribute">
                              <xsl:with-param name="name" select="'onu'" />
                              <xsl:with-param name="value" select="s0:UNDGCode/text()" />
                            </xsl:call-template>
                          </ns0:dgx-dep>
                        </xsl:for-each>

                        <xsl:if test="starts-with($operationPortCode, 'RE')">
                          <xsl:call-template name="CreateParty">
                            <xsl:with-param name="elementName" select="'shipper'" />
                            <xsl:with-param name="orgAddress" select="$subShipment/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='ConsignorDocumentaryAddress']" />
                          </xsl:call-template>

                          <xsl:call-template name="CreateParty">
                            <xsl:with-param name="elementName" select="'consignee'" />
                            <xsl:with-param name="orgAddress" select="$subShipment/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='ConsigneeDocumentaryAddress']" />
                          </xsl:call-template>
                        </xsl:if>

                        <!--<xsl:call-template name="CreateParty">
                          <xsl:with-param name="elementName" select="'notifyOne'" />
                          <xsl:with-param name="orgAddress" select="$subShipment/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='NotifyParty']" />
                        </xsl:call-template>

                        <xsl:call-template name="CreateParty">
                          <xsl:with-param name="elementName" select="'notifyTwo'" />
                          <xsl:with-param name="orgAddress" select="$subShipment/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='NotifyParty2']" />
                        </xsl:call-template>

                        <ns0:tax-dep>
                          <xsl:call-template name="CreateAttribute">
                            <xsl:with-param name="name" select="'typeTaxation'" />
                            <xsl:with-param name="value" select="$subShipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TaxReference']/s0:Value/text()" />
                          </xsl:call-template>
                          <xsl:call-template name="CreateAttribute">
                            <xsl:with-param name="name" select="'amount'" />
                            <xsl:with-param name="value" select="$subShipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TaxAmount']/s0:Value/text()" />
                          </xsl:call-template>
                          <xsl:call-template name="CreateAttribute">
                            <xsl:with-param name="name" select="'currencyCode'" />
                            <xsl:with-param name="value" select="$subShipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TaxCurrency']/s0:Value/text()" />
                          </xsl:call-template>
                          <xsl:variable name="taxStatus" select="$subShipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TaxStatus']/s0:Value/text()" />
                          <xsl:call-template name="CreateAttribute">
                            <xsl:with-param name="name" select="'status'" />
                            <xsl:with-param name="value">
                              <xsl:choose>
                                <xsl:when test="$taxStatus='Prepaid'">PRE</xsl:when>
                                <xsl:when test="$taxStatus='Collect'">COL</xsl:when>
                              </xsl:choose>
                            </xsl:with-param>
                          </xsl:call-template>
                        </ns0:tax-dep>
                        -->
                      </ns0:marchandise-dep>
                    </xsl:for-each>
                  </xsl:for-each>
                </ns0:equipement-dep>

                <ns0:deplacement-dep>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'code'" />
                    <xsl:with-param name="value">
                      <xsl:call-template name="GetRegistrationNumber">
                        <xsl:with-param name="addressType" select="'ArrivalCFSLocalTransportAddress'" />
                        <xsl:with-param name="registrationNumberType" select="$registrationNumberType" />
                      </xsl:call-template>
                    </xsl:with-param>
                  </xsl:call-template>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'mode'" />
                    <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TransportMode']/s0:Value/text()" />
                  </xsl:call-template>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'nom'" />
                    <xsl:with-param name="value" select="$shipment/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='ArrivalCFSLocalTransportAddress']/s0:CompanyName/text()" />
                    <xsl:with-param name="maxLength" select="20" />
                  </xsl:call-template>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'immatr'" />
                    <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='VehicleRegistration']/s0:Value/text()" />
                  </xsl:call-template>
                </ns0:deplacement-dep>

              </ns0:liste-depotage>
            </ns0:Request>
          </ns0:Messages>
        </ns0:MessageSet>
      </xsl:for-each>
    </ns0:Interchanges>
  </xsl:template>

  <xsl:template name="CreateParty">
    <xsl:param name="elementName" />
    <xsl:param name="orgAddress" />

    <xsl:if test="$orgAddress">

      <xsl:variable name="companyName" select="$orgAddress/s0:CompanyName/text()" />
      <xsl:variable name="partyType">
        <xsl:choose>
          <xsl:when test="$elementName='shipper'">SE</xsl:when>
          <xsl:when test="$elementName='consignee'">CN</xsl:when>
          <xsl:when test="$elementName='notifyOne'">N1</xsl:when>
          <xsl:when test="$elementName='notifyTwo'">N2</xsl:when>
          <xsl:when test="$elementName='notifyThree'">N3</xsl:when>
        </xsl:choose>
      </xsl:variable>

      <xsl:element name="ns0:{$elementName}-dep">
        <xsl:call-template name="CreateAttribute">
          <xsl:with-param name="name" select="'type'" />
          <xsl:with-param name="value" select="$partyType" />
        </xsl:call-template>
        <xsl:call-template name="CreateAttribute">
          <xsl:with-param name="name" select="'name1'" />
          <xsl:with-param name="value" select="userCSharp:SubstringSafe($companyName, 0, 35)" />
        </xsl:call-template>
        <xsl:call-template name="CreateAttribute">
          <xsl:with-param name="name" select="'name2'" />
          <xsl:with-param name="value" select="userCSharp:SubstringSafe($companyName, 35, 35)" />
        </xsl:call-template>
        <xsl:call-template name="CreateAttribute">
          <xsl:with-param name="name" select="'name3'" />
          <xsl:with-param name="value" select="userCSharp:SubstringSafe($companyName, 70, 35)" />
        </xsl:call-template>
        <xsl:call-template name="CreateAttribute">
          <xsl:with-param name="name" select="'name4'" />
          <xsl:with-param name="value" select="userCSharp:SubstringSafe($companyName, 105, 35)" />
        </xsl:call-template>
        <xsl:call-template name="CreateAttribute">
          <xsl:with-param name="name" select="'name5'" />
          <xsl:with-param name="value" select="userCSharp:SubstringSafe($companyName, 140, 35)" />
        </xsl:call-template>

        <xsl:variable name="address" select="concat($orgAddress/s0:Address1/text(),' ', $orgAddress/s0:Address2/text())" />
        <ns0:party-address>
          <xsl:call-template name="CreateAttribute">
            <xsl:with-param name="name" select="'address1'" />
            <xsl:with-param name="value" select="userCSharp:SubstringSafe($address, 0, 35)" />
          </xsl:call-template>
          <xsl:call-template name="CreateAttribute">
            <xsl:with-param name="name" select="'address2'" />
            <xsl:with-param name="value" select="userCSharp:SubstringSafe($address, 35, 35)" />
          </xsl:call-template>
          <xsl:call-template name="CreateAttribute">
            <xsl:with-param name="name" select="'address3'" />
            <xsl:with-param name="value" select="userCSharp:SubstringSafe($address, 70, 35)" />
          </xsl:call-template>
          <xsl:call-template name="CreateAttribute">
            <xsl:with-param name="name" select="'address4'" />
            <xsl:with-param name="value" select="userCSharp:SubstringSafe($address, 105, 35)" />
          </xsl:call-template>
          <xsl:call-template name="CreateAttribute">
            <xsl:with-param name="name" select="'postCode'" />
            <xsl:with-param name="value" select="$orgAddress/s0:Postcode/text()" />
          </xsl:call-template>
          <xsl:call-template name="CreateAttribute">
            <xsl:with-param name="name" select="'city'" />
            <xsl:with-param name="value" select="$orgAddress/s0:City/text()" />
          </xsl:call-template>
          <xsl:call-template name="CreateAttribute">
            <xsl:with-param name="name" select="'countrysub-lib'" />
            <xsl:with-param name="value" select="$orgAddress/s0:State/text()" />
          </xsl:call-template>
          <xsl:call-template name="CreateAttribute">
            <xsl:with-param name="name" select="'country'" />
            <xsl:with-param name="value" select="$orgAddress/s0:Country/text()" />
          </xsl:call-template>
        </ns0:party-address>

        <xsl:variable name="phoneAndEmail" select="userCSharp:CombineString($orgAddress/s0:Phone/text(), $orgAddress/s0:Email/text())" />
        <xsl:if test="$phoneAndEmail!=''">
          <ns0:party-com>
            <xsl:call-template name="CreateAttribute">
              <xsl:with-param name="name" select="'number'" />
              <xsl:with-param name="value" select="$phoneAndEmail" />
            </xsl:call-template>
            <xsl:call-template name="CreateAttribute">
              <xsl:with-param name="name" select="'contName'" />
              <xsl:with-param name="value" select="$orgAddress/s0:Contact/text()" />
            </xsl:call-template>
          </ns0:party-com>
        </xsl:if>

      </xsl:element>
    </xsl:if>
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

  <xsl:template name="GetRegistrationNumberSubShipment">
    <xsl:param name="addressType" />
    <xsl:param name="registrationNumberType"/>
    <xsl:param name="subShipment" />

    <xsl:variable name="org" select="$subShipment/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType=$addressType]" />
    <xsl:choose>
      <xsl:when test="$org">
        <xsl:value-of select="$org/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()=$registrationNumberType]/s0:Value/text()" />
      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
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
    <xsl:param name="fallbackValue" select="''"/>
    <xsl:param name="maxLength" select="'0'"/>

    <xsl:variable name="attributValue">
      <xsl:choose>
        <xsl:when test="$value!=''">
          <xsl:value-of select="$value"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$fallbackValue"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:if test="$attributValue!=''">
      <xsl:attribute name="{$name}">
        <xsl:choose>
          <xsl:when test="number($maxLength) &gt; 0">
            <xsl:value-of select="substring($attributValue, 1, $maxLength)" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$attributValue" />
          </xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
    </xsl:if>
  </xsl:template>

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[
public string SubstringSafe(string inputText, int startIndex, int length)
{
  if (string.IsNullOrEmpty(inputText))
  {
    return "";
  }

  string result = "";

  if (startIndex < 0)
  {
    startIndex = 0;
  }

  int actualLength = inputText.Length - startIndex;
  if (actualLength > 0)
  {
    if (actualLength > length && length >= 0)
    {
      actualLength = length;
    }
    result = inputText.Substring(startIndex, actualLength);
  }

  return result;
}

public string CombineString(string inputText1, string inputText2)
{
 if (string.IsNullOrEmpty(inputText1) && string.IsNullOrEmpty(inputText2))
  {
    return "";
  }

  var textList = new System.Collections.Generic.List<string>();
  if (!string.IsNullOrEmpty(inputText1))
  {
    textList.Add(inputText1);
  }
  if (!string.IsNullOrEmpty(inputText2))
  {
    textList.Add(inputText2);
  };
  return String.Join(",", textList.ToArray());
}

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

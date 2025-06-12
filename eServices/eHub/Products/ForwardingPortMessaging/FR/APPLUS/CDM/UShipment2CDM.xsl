<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 CodeMapper ContextAccessor DataModelAccessor DateMapper userCSharp" version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2012/11/CDM/1"
                xmlns:ns0="http://www.cargowise.com/Schemas/FPM/APPLUS/CDM"
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
  <xsl:variable name="registrationNumberTypeEmetteurOrTiersCSTManut">
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
        <xsl:with-param name="registrationNumberType" select="$registrationNumberTypeEmetteurOrTiersCSTManut"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="interchangeID" select="CodeMapper:CallActionProcedureHelper('GetCounterValue','@value','@name','CargoWise.eHub.Products.ForwardingPortMessaging.FR.APPLUS.Interchange','@maxlength','14')" />
    <xsl:variable name="subscribeClientID" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderID, $serviceProvider, $senderID, concat($applusID, ',', $applusTierProf))" />

    <xsl:variable name="InboxPK" select="ContextAccessor:GetContextProperty('InternalTrackingID', 'http://cargowise.com/ehub/tracking/2010/06')"/>
    <xsl:variable name="FileName" select="ContextAccessor:SetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06', concat('CDM_', $senderID, '_', $interchangeID))"/>
    <xsl:variable name="EmailSubject" select="ContextAccessor:SetContextProperty('OverrideEmailSubject', 'http://cargowise.com/ehub/processing/2010/06', concat('FPM_', $serviceProvider, '_', $operationPortCode))"/>

    <xsl:if test="$newRecipientID!=$recipientID">
      <xsl:variable name="SetRecipientID" select="ContextAccessor:SetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties', $newRecipientID)"/>
    </xsl:if>

    <xsl:variable name="isContainerized" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Container Mode' , 'Containerized', s0:ContainerMode/text())" />

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
        <xsl:variable name="containerNumber" select="s0:ContainerNumber/text()"/>
        <xsl:variable name="messageReference" select="concat($consolID, '_', $containerNumber, '_CDM')"/>

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
              <xsl:variable name="subscribeDocumentName" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $formattedMessageIdentifier, $documentName, 'DocumentName')" />
              <xsl:variable name="SubscribeContainerNumber" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $formattedMessageIdentifier, $containerNumber, 'ContainerNumber')" />

              <xsl:value-of select="$formattedMessageIdentifier" />
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <xsl:variable name="lpdReference" select="s0:AddInfoCollection/s0:AddInfo[s0:Key='LPDReference']/s0:Value/text()" />
        <xsl:variable name="requestID">
          <xsl:choose>
            <xsl:when test="$lpdReference!=''">
              <xsl:value-of select="$lpdReference"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$containerNumber"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:variable name="SubscribeForwarderRef" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageIdentifier, $requestID, 'RequestID')" />

        <xsl:variable name="SubscribeInboxPK" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $InboxPK, concat($interchangeID, '_', $messageIdentifier))" />
        <xsl:variable name="subscribeShipmentType" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageIdentifier, $shipment/s0:DataContext/s0:DataSource/s0:Type/text(), 'ForwardingType')" />
        <xsl:variable name="subscribeActionPurpose" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageIdentifier, $purpose, 'Purpose')" />
        <xsl:variable name="subscribeOperationPort" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageIdentifier, $operationPortCode, 'OperationPort')" />

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
              <xsl:attribute name="type">CDM</xsl:attribute>

              <ns0:constat-depotage-masse>
                <xsl:variable name="container" select="." />
                <xsl:variable name="containerLink" select="s0:Link/text()" />
                <xsl:for-each select="$shipment/s0:SubShipmentCollection/s0:SubShipment">
                  <xsl:variable name="subShipment" select="." />

                  <xsl:for-each select="s0:PackingLineCollection/s0:PackingLine[s0:ContainerLink/text()=$containerLink]">
                    <ns0:constat-depotage>
                      <ns0:reference-cst>
                        <xsl:call-template name="CreateAttribute">
                          <xsl:with-param name="name" select="'amq'" />
                          <xsl:with-param name="value" select="s0:ImportReferenceNumber/text()"/>
                        </xsl:call-template>

                        <xsl:call-template name="CreateAttribute">
                          <xsl:with-param name="name" select="'res'" />
                          <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ReserveIndicator']/s0:Value/text()" />
                        </xsl:call-template>

                        <xsl:call-template name="CreateAttribute">
                          <xsl:with-param name="name" select="'ext'" />
                          <xsl:with-param name="value" select="$subShipment/s0:WayBillNumber/text()" />
                        </xsl:call-template>

                        <xsl:call-template name="CreateAttribute">
                          <xsl:with-param name="name" select="'en-plus'" />
                          <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='SurplusIndicator']/s0:Value/text()" />
                        </xsl:call-template>

                        <xsl:call-template name="CreateAttribute">
                          <xsl:with-param name="name" select="'non-depote'" />
                          <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='UnpackingIndicator']/s0:Value/text()" />
                        </xsl:call-template>

                        <ns0:commentaires>
                          <xsl:value-of select="$container/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='UnpackingNotes']/s0:Value/text()"/>
                        </ns0:commentaires>
                      </ns0:reference-cst>

                      <ns0:voyage-cst>
                        <xsl:call-template name="CreateAttribute">
                          <xsl:with-param name="name" select="'atp'" />
                          <xsl:with-param name="value" select="$shipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ATPReference']/s0:Value/text()"/>
                        </xsl:call-template>
                      </ns0:voyage-cst>

                      <ns0:lieu-cst>
                        <xsl:call-template name="CreateAttribute">
                          <xsl:with-param name="name" select="'zone'" />
                          <xsl:with-param name="value" select="$container/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='UnpackArea']/s0:Value/text()"/>
                        </xsl:call-template>
                        <xsl:call-template name="CreateAttribute">
                          <xsl:with-param name="name" select="'lieu'" />
                          <xsl:with-param name="value" select="$container/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='UnpackLocation']/s0:Value/text()"/>
                        </xsl:call-template>
                      </ns0:lieu-cst>

                      <ns0:tiers-cst>
                        <xsl:attribute name="manut">
                          <xsl:call-template name="GetRegistrationNumber">
                            <xsl:with-param name="addressType" select="'ArrivalCFSAddress'" />
                            <xsl:with-param name="registrationNumberType" select="$registrationNumberTypeEmetteurOrTiersCSTManut"/>
                          </xsl:call-template>
                        </xsl:attribute>
                      </ns0:tiers-cst>

                      <ns0:lmarchandise-cst>
                        <xsl:if test="$isContainerized='true'">
                          <xsl:call-template name="CreateAttribute">
                            <xsl:with-param name="name" select="'eqd'" />
                            <xsl:with-param name="value" select="$container/s0:ContainerNumber/text()" />
                          </xsl:call-template>
                        </xsl:if>

                        <xsl:call-template name="CreateAttribute_Integer">
                          <xsl:with-param name="name" select="'nb'" />
                          <xsl:with-param name="value" select="round(s0:OutturnQty/text())"/>
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
                        <xsl:attribute name="code">
                          <xsl:value-of select="$packTypeMappingCode"/>
                        </xsl:attribute>

                        <xsl:call-template name="CreateAttribute_Integer">
                          <xsl:with-param name="name" select="'poids'" />
                          <xsl:with-param name="value" select="round(s0:OutturnedWeight/text())"/>
                        </xsl:call-template>

                        <xsl:call-template name="CreateAttribute_Integer">
                          <xsl:with-param name="name" select="'vol'" />
                          <xsl:with-param name="value" select="round(s0:OutturnedVolume/text())" />
                        </xsl:call-template>

                        <xsl:if test="$isContainerized='true'">
                          <xsl:call-template name="CreateAttribute">
                            <xsl:with-param name="name" select="'eqdsic'" />
                            <xsl:with-param name="value" select="$container/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ICTReference']/s0:Value/text()" />
                          </xsl:call-template>
                        </xsl:if>

                        <xsl:if test="$isContainerized='false'">
                          <xsl:call-template name="CreateAttribute">
                            <xsl:with-param name="name" select="'marchsic'" />
                            <xsl:with-param name="value" select="s0:ImportReferenceNumber/text()" />
                          </xsl:call-template>

                          <xsl:call-template name="CreateAttribute">
                            <xsl:with-param name="name" select="'march'" />
                            <xsl:with-param name="value" select="$subShipment/s0:WayBillNumber/text()" />
                          </xsl:call-template>
                        </xsl:if>

                        <xsl:call-template name="CreateAttribute">
                          <xsl:with-param name="name" select="'ref'" />
                          <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='UnpackedReference']/s0:Value/text()" />
                        </xsl:call-template>

                        <ns0:marques>
                          <xsl:value-of select="s0:MarksAndNos/text()"/>
                        </ns0:marques>
                      </ns0:lmarchandise-cst>

                      <ns0:date-cst>
                        <xsl:attribute name="horodatage">
                          <xsl:value-of select="DateMapper:ConvertXmlDateString($shipment/s0:DataContext/s0:Workflow/s0:TriggerDate/text(), 'dd/MM/yyyy hh:mm')"/>
                        </xsl:attribute>
                      </ns0:date-cst>
                    </ns0:constat-depotage>
                  </xsl:for-each>
                </xsl:for-each>
                <ns0:constat-fin-depotage>
                  <ns0:voyage-cst>
                    <xsl:call-template name="CreateAttribute">
                      <xsl:with-param name="name" select="'atp'" />
                      <xsl:with-param name="value" select="$shipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ATPReference']/s0:Value/text()" />
                    </xsl:call-template>
                  </ns0:voyage-cst>

                  <ns0:lieu-cst>
                    <xsl:attribute name="zone">
                      <xsl:value-of select="$container/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='UnpackArea']/s0:Value/text()"/>
                    </xsl:attribute>

                    <xsl:attribute name="lieu">
                      <xsl:value-of select="$container/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='UnpackLocation']/s0:Value/text()"/>
                    </xsl:attribute>
                  </ns0:lieu-cst>

                  <ns0:tiers-cst>
                    <xsl:attribute name="manut">
                      <xsl:call-template name="GetRegistrationNumber">
                        <xsl:with-param name="addressType" select="'ArrivalCFSAddress'" />
                        <xsl:with-param name="registrationNumberType" select="$registrationNumberType"/>
                      </xsl:call-template>
                    </xsl:attribute>
                  </ns0:tiers-cst>

                  <xsl:choose>
                    <xsl:when test="$isContainerized='true'">
                      <ns0:equipement-cst>
                        <xsl:attribute name="id">
                          <xsl:value-of select="$container/s0:ContainerNumber/text()"/>
                        </xsl:attribute>
                      </ns0:equipement-cst>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:variable name="firstSubShipment" select="$shipment/s0:SubShipmentCollection/s0:SubShipment[1]" />
                      <ns0:marchandise-cst>
                        <xsl:call-template name="CreateAttribute">
                          <xsl:with-param name="name" select="'id'" />
                          <xsl:with-param name="value" select="$firstSubShipment/s0:WayBillNumber/text()" />
                        </xsl:call-template>
                      </ns0:marchandise-cst>
                    </xsl:otherwise>
                  </xsl:choose>

                  <ns0:date-cst>
                    <xsl:attribute name="horodatage">
                      <xsl:value-of select="DateMapper:ConvertXmlDateString($shipment/s0:DataContext/s0:Workflow/s0:TriggerDate/text(), 'dd/MM/yyyy hh:mm')"/>
                    </xsl:attribute>
                  </ns0:date-cst>
                </ns0:constat-fin-depotage>
              </ns0:constat-depotage-masse>
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
    <xsl:param name="fallbackValue" />

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
        <xsl:value-of select="$attributValue" />
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


public int ToInt(string inputText)
{
  if (!string.IsNullOrEmpty(inputText))
  {
    return 0;
  }
  return (int)double.Parse(inputText);
}

]]>
  </msxsl:script>
</xsl:stylesheet>
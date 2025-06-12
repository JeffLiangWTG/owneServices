<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 CodeMapper ContextAccessor DataModelAccessor DateMapper userCSharp" version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2012/11/CRESA/1"
                xmlns:ns0="http://www.cargowise.com/Schemas/FPM/APPLUS/CRESA"
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
  <xsl:variable name="registrationNumberTypeTiersCSTAgfret">
    <xsl:choose>
      <xsl:when test="$serviceProvider='SOGET'">SOA</xsl:when>
      <xsl:otherwise>CI5</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:template match="s0:UniversalShipment/s0:Shipment">
    <xsl:variable name="shipmentID" select="s0:DataContext/s0:DataSource/s0:Key/text()"/>
    <xsl:variable name="purpose" select="s0:DataContext/s0:DocumentaryOverride/s0:Purpose/text()" />
    <xsl:variable name="ffwRef" select="s0:AdditionalReferenceCollection/s0:AdditionalReference[s0:Type/text()='FFW']/s0:ReferenceNumber/text()" />

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

    <xsl:variable name="serviceProviderMSGID" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Port Settings' , 'MSGID', $recipientID)"/>
    <xsl:variable name="serviceProviderID" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Port Settings' , 'ID', $recipientID)"/>
    <xsl:variable name="msgPrefix"  select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Port Settings' , 'SubscriptionPrefix', $recipientID)"/>

    <xsl:variable name="messageReference" select="concat($shipmentID, '_CRESA')"/>

    <xsl:variable name="previousMessageIdentifier" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $serviceProvider, '@recipientId', $senderID , '@ST_ID',  $serviceProviderMSGID , '@value', $messageReference, '@referenceType', 'MessageReference')"/>
    <xsl:variable name="messageIdentifier">
      <xsl:choose>
        <xsl:when test="$previousMessageIdentifier!=''">
          <xsl:value-of select="$previousMessageIdentifier"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:variable name="messageSetID" select="CodeMapper:CallActionProcedureHelper('GetCounterValue','@value','@name','CargoWise.eHub.Products.ForwardingPortMessaging.FR.APPLUS','@maxlength','14')" />
          <xsl:variable name="formattedMessageIdentifier" select='concat($msgPrefix, format-number($messageSetID, "0000000000"))' />

          <xsl:variable name="SubScriberValue1" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $formattedMessageIdentifier, $shipmentID, 'JobNumber')" />
          <xsl:variable name="SubScriberValue2" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $shipmentID, $formattedMessageIdentifier, $formattedMessageIdentifier)" />

          <xsl:variable name="SubscribeMessageReference" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageReference, $formattedMessageIdentifier, 'MessageReference')" />

          <!--the following subscription will not change after ORG is sent-->
          <xsl:variable name="SubscribeDocumentName" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $formattedMessageIdentifier, s0:DataContext/s0:DocumentaryOverride/s0:DocumentName/text(), 'DocumentName')" />

          <xsl:value-of select="$formattedMessageIdentifier" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="SubscribeRequestID" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageIdentifier, $ffwRef, 'RequestID')" />
    <xsl:variable name="ShipmentTypeSubscription" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageIdentifier, s0:DataContext/s0:DataSource/s0:Type/text(), 'ForwardingType')" />
    <xsl:variable name="SubscribeActionPurpose" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageIdentifier, $purpose, 'Purpose')" />
    <xsl:variable name="SubscribeOperationPort" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageIdentifier, $operationPortCode, 'OperationPort')" />

    <xsl:variable name="subscribeClientID" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderID, $serviceProvider, $senderID, concat($applusID, ',', $applusTierProf))" />

    <xsl:variable name="InboxPK" select="ContextAccessor:GetContextProperty('InternalTrackingID', 'http://cargowise.com/ehub/tracking/2010/06')"/>
    <xsl:variable name="SubscribeInboxPK" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $InboxPK, concat($interchangeID, '_', $messageIdentifier))" />
    <xsl:variable name="FileName" select="ContextAccessor:SetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06', concat('CRESA_', $senderID, '_', $interchangeID))"/>
    <xsl:variable name="EmailSubject" select="ContextAccessor:SetContextProperty('OverrideEmailSubject', 'http://cargowise.com/ehub/processing/2010/06', concat('FPM_', $serviceProvider, '_', $operationPortCode))"/>

    <xsl:if test="$newRecipientID!=$recipientID">
      <xsl:variable name="SetRecipientID" select="ContextAccessor:SetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties', $newRecipientID)"/>
    </xsl:if>

    <ns0:Interchanges>
      <xsl:call-template name="CreateAttribute">
        <xsl:with-param name="name" select="'id'" />
        <xsl:with-param name="value" select="$interchangeID" />
      </xsl:call-template>
      <xsl:call-template name="CreateAttribute">
        <xsl:with-param name="name" select="'from'" />
        <xsl:with-param name="value" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Service Provider Settings' , 'SenderID', $serviceProvider, $operationPortCode, $routingParty)" />
      </xsl:call-template>
      <xsl:call-template name="CreateAttribute">
        <xsl:with-param name="name" select="'to'" />
        <xsl:with-param name="value" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Service Provider Settings' , 'RecipientID', $serviceProvider, $operationPortCode, $routingParty)" />
      </xsl:call-template>

      <ns0:MessageSet>
        <xsl:call-template name="CreateAttribute">
          <xsl:with-param name="name" select="'id'" />
          <xsl:with-param name="value" select="$messageIdentifier" />
        </xsl:call-template>
        <xsl:call-template name="CreateAttribute">
          <xsl:with-param name="name" select="'icid'" />
          <xsl:with-param name="value" select="$interchangeID" />
        </xsl:call-template>
        <xsl:attribute name="date">
          <xsl:value-of select="DateMapper:ConvertXmlDateString(s0:DataContext/s0:Workflow/s0:TriggerDate/text(), 'dd/MM/yyyy HH:mm:ss')"/>
        </xsl:attribute>

        <ns0:Destinataire>
          <xsl:call-template name="CreateAttribute">
            <xsl:with-param name="name" select="'user'" />
            <xsl:with-param name="value" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Service Provider Settings' , 'DestinationUser', $serviceProvider, $operationPortCode, $routingParty)" />
          </xsl:call-template>
          <xsl:call-template name="CreateAttribute">
            <xsl:with-param name="name" select="'tiersProf'" />
            <xsl:with-param name="value" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Service Provider Settings' , 'DestinationParty', $serviceProvider, $operationPortCode, $routingParty)" />
          </xsl:call-template>
        </ns0:Destinataire>

        <ns0:Emetteur>
          <xsl:call-template name="CreateAttribute">
            <xsl:with-param name="name" select="'user'" />
            <xsl:with-param name="value" select="$applusID" />
          </xsl:call-template>
          <xsl:call-template name="CreateAttribute">
            <xsl:with-param name="name" select="'tiersProf'" />
            <xsl:with-param name="value" select="$applusTierProf" />
          </xsl:call-template>
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
            <xsl:call-template name="CreateAttribute">
              <xsl:with-param name="name" select="'id'" />
              <xsl:with-param name="value" select="$messageIdentifier" />
            </xsl:call-template>
            <xsl:attribute name="type">CRESA</xsl:attribute>

            <ns0:constat-reception>
              <ns0:reference-cst>
                <xsl:call-template name="CreateAttribute">
                  <xsl:with-param name="name" select="'ext'" />
                  <xsl:with-param name="value" select="$ffwRef" />
                </xsl:call-template>

                <xsl:call-template name="CreateAttribute">
                  <xsl:with-param name="name" select="'agfret'" />
                  <xsl:with-param name="value" select="$ffwRef" />
                </xsl:call-template>

                <xsl:variable name="dateReceived" select="s0:DateCollection/s0:Date[s0:Type='Received']/s0:Value/text()"/>
                <xsl:call-template name="CreateAttribute_Date">
                  <xsl:with-param name="name" select="'date'" />
                  <xsl:with-param name="value" select="$dateReceived" />
                  <xsl:with-param name="alwaysShowTime" select="'N'" />
                </xsl:call-template>

                <xsl:call-template name="CreateAttribute_Date">
                  <xsl:with-param name="name" select="'lastdate'" />
                  <xsl:with-param name="value" select="$dateReceived" />
                  <xsl:with-param name="alwaysShowTime" select="'N'" />
                </xsl:call-template>

                <xsl:call-template name="CreateAttribute">
                  <xsl:with-param name="name" select="'scelles'" />
                  <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='GoodsSealed']/s0:Value/text()" />
                </xsl:call-template>

                <xsl:variable name="consigneeDocAddressRegNumber">
                  <xsl:call-template name="GetRegistrationNumber">
                    <xsl:with-param name="addressType" select="'ConsigneeDocumentaryAddress'" />
                    <xsl:with-param name="registrationNumberType" select="$registrationNumberType"/>
                  </xsl:call-template>
                </xsl:variable>
                <xsl:call-template name="CreateAttribute">
                  <xsl:with-param name="name" select="'client'" />
                  <xsl:with-param name="value">
                    <xsl:choose>
                      <xsl:when test="$consigneeDocAddressRegNumber!=''">
                        <xsl:value-of select="$consigneeDocAddressRegNumber"/>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType='ConsigneeDocumentaryAddress']/s0:CompanyName/text()"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:with-param>
                  <xsl:with-param name="maxLength" select="'30'"/>
                </xsl:call-template>

                <xsl:variable name="consignorDocAddressRegNumber">
                  <xsl:call-template name="GetRegistrationNumber">
                    <xsl:with-param name="addressType" select="'ConsignorDocumentaryAddress'" />
                    <xsl:with-param name="registrationNumberType" select="$registrationNumberType"/>
                  </xsl:call-template>
                </xsl:variable>
                <xsl:call-template name="CreateAttribute">
                  <xsl:with-param name="name" select="'fournisseur'" />
                  <xsl:with-param name="value">
                    <xsl:choose>
                      <xsl:when test="$consignorDocAddressRegNumber!=''">
                        <xsl:value-of select="$consignorDocAddressRegNumber"/>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType='ConsignorDocumentaryAddress']/s0:CompanyName/text()"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:with-param>
                  <xsl:with-param name="maxLength" select="'30'"/>
                </xsl:call-template>

                <ns0:commentaires>
                  <xsl:value-of select="s0:NoteCollection/s0:Note[s0:Description/text()='Goods Receipt Notes']/s0:NoteText/text()"/>
                </ns0:commentaires>
              </ns0:reference-cst>

              <ns0:voyage-cst>
                <xsl:call-template name="CreateAttribute_Date">
                  <xsl:with-param name="name" select="'date'" />
                  <xsl:with-param name="value" select="s0:DateCollection/s0:Date[s0:Type/text()='Arrival']/s0:Value/text()" />
                  <xsl:with-param name="alwaysShowTime" select="'Y'" />
                </xsl:call-template>

                <xsl:call-template name="CreateAttribute">
                  <xsl:with-param name="name" select="'ser'" />
                  <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='PortServiceCode']/s0:Value/text()" />
                </xsl:call-template>

                <xsl:call-template name="CreateAttribute">
                  <xsl:with-param name="name" select="'refser'" />
                  <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='PortServiceReference']/s0:Value/text()" />
                </xsl:call-template>
              </ns0:voyage-cst>

              <ns0:lieu-cst>
                <xsl:call-template name="CreateAttribute">
                  <xsl:with-param name="name" select="'zone'" />
                  <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='PortArea']/s0:Value/text()"/>
                </xsl:call-template>
                <xsl:call-template name="CreateAttribute">
                  <xsl:with-param name="name" select="'lieu'" />
                  <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='PortLocation']/s0:Value/text()"/>
                </xsl:call-template>
              </ns0:lieu-cst>

              <xsl:variable name="regNumCurrentUser">
                <xsl:call-template name="GetRegistrationNumber">
                  <xsl:with-param name="addressType" select="'CurrentUser'" />
                  <xsl:with-param name="registrationNumberType" select="$registrationNumberType"/>
                </xsl:call-template>
              </xsl:variable>
              <xsl:variable name="regNumManut">
                <xsl:call-template name="GetRegistrationNumberWithFallBack">
                  <xsl:with-param name="addressType" select="'BookingPartyDocumentaryAddress'" />
                  <xsl:with-param name="fallbackAddressType" select="'CurrentUser'" />
                  <xsl:with-param name="registrationNumberType" select="$registrationNumberTypeEmetteurOrTiersCSTManut"/>
                </xsl:call-template>
              </xsl:variable>
              <xsl:variable name="regNumAgfret">
                <xsl:call-template name="GetRegistrationNumberWithFallBack">
                  <xsl:with-param name="addressType" select="'PickupAgent'" />
                  <xsl:with-param name="fallbackAddressType" select="'CurrentUser'" />
                  <xsl:with-param name="registrationNumberType" select="$registrationNumberTypeTiersCSTAgfret"/>
                </xsl:call-template>
              </xsl:variable>

              <xsl:if test="$regNumCurrentUser!='' or $regNumManut!='' or $regNumAgfret!=''">
                <ns0:tiers-cst>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'manut'" />
                    <xsl:with-param name="value" select="$regNumManut"/>
                  </xsl:call-template>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'agfret'" />
                    <xsl:with-param name="value" select="$regNumAgfret"/>
                  </xsl:call-template>
                  <xsl:call-template name="CreateAttribute">
                    <xsl:with-param name="name" select="'amq'" />
                    <xsl:with-param name="value" select="$regNumCurrentUser"/>
                  </xsl:call-template>
                </ns0:tiers-cst>
              </xsl:if>

              <ns0:lmarchandise-cst>
                <xsl:call-template name="CreateAttribute">
                  <xsl:with-param name="name" select="'ref'" />
                  <xsl:with-param name="value" select="$ffwRef"/>
                </xsl:call-template>

                <xsl:call-template name="CreateAttribute_Integer">
                  <xsl:with-param name="name" select="'nb'" />
                  <xsl:with-param name="value" select="round(s0:TotalNoOfPacks/text())" />
                </xsl:call-template>

                <xsl:variable name="serviceProviderTotalNoPackTypeCode" select="CodeMapper:GetRecipientCode($serviceProvider, $serviceProvider, concat($serviceProvider, ' System Configuration'), 'Package Type ISO' , concat($serviceProvider, ' Code'),  s0:TotalNoOfPacksPackageType/text())"/>
                <xsl:variable name="totalNoPackTypeMappingCode">
                  <xsl:choose>
                    <xsl:when test="$serviceProviderTotalNoPackTypeCode != ''">
                      <xsl:value-of select="$serviceProviderTotalNoPackTypeCode"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Package Type ISO' , 'Output Code', s0:TotalNoOfPacksPackageType/text())"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:variable>
                <xsl:call-template name="CreateAttribute">
                  <xsl:with-param name="name" select="'code'" />
                  <xsl:with-param name="value" select="$totalNoPackTypeMappingCode"/>
                </xsl:call-template>

                <xsl:call-template name="CreateAttribute_Integer">
                  <xsl:with-param name="name" select="'poids'" />
                  <xsl:with-param name="value" select="round(s0:TotalWeight/text())" />
                </xsl:call-template>

                <xsl:call-template name="CreateAttribute_Integer">
                  <xsl:with-param name="name" select="'vol'" />
                  <xsl:with-param name="value" select="round(s0:TotalVolume/text())" />
                </xsl:call-template>

                <ns0:designation-cst>
                  <xsl:value-of select="s0:GoodsDescription/text()"/>
                </ns0:designation-cst>

                <xsl:for-each select="s0:PackingLineCollection/s0:PackingLine">
                  <ns0:mesurage-cst>
                    <xsl:call-template name="CreateAttribute_Integer">
                      <xsl:with-param name="name" select="'long'" />
                      <xsl:with-param name="value" select="round(s0:Length/text())" />
                    </xsl:call-template>

                    <xsl:call-template name="CreateAttribute_Integer">
                      <xsl:with-param name="name" select="'haut'" />
                      <xsl:with-param name="value" select="round(s0:Height/text())" />
                    </xsl:call-template>

                    <xsl:call-template name="CreateAttribute_Integer">
                      <xsl:with-param name="name" select="'larg'" />
                      <xsl:with-param name="value" select="round(s0:Width/text())" />
                    </xsl:call-template>

                    <xsl:call-template name="CreateAttribute_Integer">
                      <xsl:with-param name="name" select="'nb'" />
                      <xsl:with-param name="value" select="round(s0:PackQty/text())" />
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
                      <xsl:with-param name="value" select="$packTypeMappingCode"/>
                    </xsl:call-template>

                    <xsl:call-template name="CreateAttribute_Integer">
                      <xsl:with-param name="name" select="'vol'" />
                      <xsl:with-param name="value" select="round(s0:Volume/text())" />
                    </xsl:call-template>

                    <xsl:call-template name="CreateAttribute">
                      <xsl:with-param name="name" select="'ref'" />
                      <xsl:with-param name="value" select="s0:PackingLineID/text()" />
                    </xsl:call-template>

                    <ns0:marques>
                      <xsl:value-of select="s0:MarksAndNos/text()"/>
                    </ns0:marques>
                  </ns0:mesurage-cst>
                </xsl:for-each>
              </ns0:lmarchandise-cst>

              <ns0:date-cst>
                <xsl:variable name="dateCargoReceived" select="s0:DateCollection/s0:Date[s0:Type='CargoReceiptDate']/s0:Value/text()"/>
                <xsl:call-template name="CreateAttribute_Date">
                  <xsl:with-param name="name" select="'horodatage'" />
                  <xsl:with-param name="value" select="$dateCargoReceived" />
                  <xsl:with-param name="alwaysShowTime" select="'N'" />
                </xsl:call-template>

                <xsl:call-template name="CreateAttribute_Date">
                  <xsl:with-param name="name" select="'sic'" />
                  <xsl:with-param name="value" select="$dateCargoReceived" />
                  <xsl:with-param name="alwaysShowTime" select="'N'" />
                </xsl:call-template>
              </ns0:date-cst>

              <ns0:lieux-cst>
                <xsl:call-template name="CreateAttribute">
                  <xsl:with-param name="name" select="'tbt'" />
                  <xsl:with-param name="value" select="s0:PortFirstForeign/text()"/>
                </xsl:call-template>
                <xsl:call-template name="CreateAttribute">
                  <xsl:with-param name="name" select="'fin'" />
                  <xsl:with-param name="value" select="s0:PortOfDestination/text()"/>
                </xsl:call-template>
              </ns0:lieux-cst>

              <xsl:variable name="regNumber">
                <xsl:call-template name="GetRegistrationNumber">
                  <xsl:with-param name="addressType" select="'PickupLocalCartage'" />
                  <xsl:with-param name="registrationNumberType" select="$registrationNumberType"/>
                </xsl:call-template>
              </xsl:variable>
              <xsl:if test="$regNumber != ''">
                <ns0:acheminement-cst>
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
                    <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TransporterID']/s0:Value/text()" />
                  </xsl:call-template>
                </ns0:acheminement-cst>
              </xsl:if>
            </ns0:constat-reception>
          </ns0:Request>
        </ns0:Messages>
      </ns0:MessageSet>
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
    <xsl:param name="alwaysShowTime" />

    <xsl:if test="$value!=''">
      <xsl:variable name="formattedDate" select="DateMapper:ConvertXmlDateString($value, 'dd/MM/yyyy HH:mm')" />
      <xsl:variable name="dateValue">
        <xsl:choose>
          <xsl:when test="substring-after($formattedDate, ' ') ='00:00' and $alwaysShowTime != 'Y'">
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
    <xsl:param name="maxLength" select="'0'"/>

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

    <xsl:if test="$attributeValue != ''">
      <xsl:attribute name="{$name}">
        <xsl:choose>
          <xsl:when test="number($maxLength) &gt; 0">
            <xsl:value-of select="substring($attributeValue, 1, $maxLength)" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$attributeValue" />
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
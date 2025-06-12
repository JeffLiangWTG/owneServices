<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 CodeMapper ContextAccessor DataModelAccessor DateMapper userCSharp" version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2012/11/DOS/1"
                xmlns:ns0="http://www.cargowise.com/Schemas/FPM/APPLUS/DOS"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
                xmlns:DataModelAccessor="http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"
                xmlns:DateMapper="http://schemas.microsoft.com/BizTalk/2003/DateMapper"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes"/>

  <xsl:template match="/">
    <xsl:apply-templates select="s0:UniversalShipment/s0:Shipment" />
  </xsl:template>

  <xsl:variable name="recipientID" select="ContextAccessor:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="senderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="serviceProvider" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Port Settings' , 'Name', $recipientID)"/>

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


    <xsl:variable name="consolID" select="s0:DataContext/s0:DataSource/s0:Key/text()"/>
    <xsl:variable name="purpose" select="s0:DataContext/s0:DocumentaryOverride/s0:Purpose/text()" />
    <xsl:variable name="documentName" select="s0:DataContext/s0:DocumentaryOverride/s0:DocumentName/text()" />
    <xsl:variable name="ffwRef" select="s0:AdditionalReferenceCollection/s0:AdditionalReference[s0:Type/text()='FFW']/s0:ReferenceNumber/text()" />

    <xsl:variable name="parseClientRegistration" select="userCSharp:ParseClientRegistration(DataModelAccessor:GetClientRegistrationCode($senderID, $operationPortCode, $serviceProvider))" />
    <xsl:variable name="applusID" select="userCSharp:GetUser()" />
    <xsl:variable name="applusTierProf">
      <xsl:call-template name="GetRegistrationNumber">
        <xsl:with-param name="addressType" select="'CurrentUser'" />
        <xsl:with-param name="registrationNumberType" select="$registrationNumberType"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="interchangeID" select="CodeMapper:CallActionProcedureHelper('GetCounterValue','@value','@name','CargoWise.eHub.Products.ForwardingPortMessaging.FR.APPLUS.Interchange','@maxlength','14')" />

    <xsl:variable name="serviceProviderMSGID" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Port Settings' , 'MSGID', $recipientID)"/>
    <xsl:variable name="serviceProviderID" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Port Settings' , 'ID', $recipientID)"/>
    <xsl:variable name="msgPrefix"  select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Port Settings' , 'SubscriptionPrefix', $recipientID)"/>

    <xsl:variable name="messageReference">
      <xsl:choose>
        <xsl:when test="contains($documentName, 'Import')">
          <xsl:value-of select="concat($consolID, '_DOS_Import')" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="concat($consolID, '_DOS_Export')" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

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
          <xsl:variable name="SubscribeForwarderRef" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $formattedMessageIdentifier, $ffwRef, 'RequestID')" />
          <xsl:variable name="SubscribeDocumentName" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $formattedMessageIdentifier, $documentName, 'DocumentName')" />

          <xsl:value-of select='$formattedMessageIdentifier'/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="ShipmentTypeSubscription" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageIdentifier, s0:DataContext/s0:DataSource/s0:Type/text(), 'ForwardingType')" />
    <xsl:variable name="SubscribeActionPurpose" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageIdentifier, $purpose, 'Purpose')" />
    <xsl:variable name="SubscribeOperationPort" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageIdentifier, $operationPortCode, 'OperationPort')" />

    <xsl:variable name="subscribeClientID" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderID, $serviceProvider, $senderID, concat($applusID, ',', $applusTierProf))" />

    <xsl:variable name="InboxPK" select="ContextAccessor:GetContextProperty('InternalTrackingID', 'http://cargowise.com/ehub/tracking/2010/06')"/>
    <xsl:variable name="SubscribeInboxPK" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $InboxPK, concat($interchangeID, '_', $messageIdentifier))" />
    <xsl:variable name="FileName" select="ContextAccessor:SetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06', concat('DOS_', $senderID, '_', $interchangeID))"/>
    <xsl:variable name="EmailSubject" select="ContextAccessor:SetContextProperty('OverrideEmailSubject', 'http://cargowise.com/ehub/processing/2010/06', concat('FPM_', $serviceProvider, '_', $operationPortCode))"/>

    <xsl:if test="$newRecipientID!=$recipientID">
      <xsl:variable name="SetRecipientID" select="ContextAccessor:SetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties', $newRecipientID)"/>
    </xsl:if>

    <xsl:variable name="isImport">
      <xsl:choose>
        <xsl:when test="contains($documentName, 'Import')">true</xsl:when>
        <xsl:otherwise>false</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
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

      <ns0:MessageSet>
        <xsl:attribute name="id">
          <xsl:value-of select="$messageIdentifier"/>
        </xsl:attribute>
        <xsl:attribute name="icid">
          <xsl:value-of select="$interchangeID"/>
        </xsl:attribute>
        <xsl:attribute name="date">
          <xsl:value-of select="DateMapper:ConvertXmlDateString(s0:DataContext/s0:Workflow/s0:TriggerDate/text(), 'dd/MM/yyyy HH:mm:ss')"/>
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
              <xsl:value-of select="$messageIdentifier" />
            </xsl:attribute>
            <xsl:attribute name="type">
              <xsl:choose>
                <xsl:when test="$isImport='true'">FIN</xsl:when>
                <xsl:otherwise>DOS</xsl:otherwise>
              </xsl:choose>
            </xsl:attribute>

            <xsl:choose>
              <xsl:when test="$isImport='true'">
                <ns0:dossier-import>
                  <ns0:reference-dos>
                    <xsl:call-template name="CreateAttribute">
                      <xsl:with-param name="name" select="'ext'" />
                      <xsl:with-param name="value" select="$ffwRef"/>
                    </xsl:call-template>
                    <xsl:call-template name="CreateAttribute">
                      <xsl:with-param name="name" select="'baeti'" />
                      <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ImplicitBAET']/s0:Value/text()"/>
                    </xsl:call-template>
                    <xsl:call-template name="CreateAttribute">
                      <xsl:with-param name="name" select="'ndu'" />
                      <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='UniqueDeclaration']/s0:Value/text()"/>
                    </xsl:call-template>
                    <xsl:call-template name="CreateAttribute">
                      <xsl:with-param name="name" select="'comp'" />
                      <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='FileComplete']/s0:Value/text()"/>
                    </xsl:call-template>
                  </ns0:reference-dos>

                  <xsl:variable name="thirdPartyAddress" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='ThirdPartyAddress']" />
                  <xsl:variable name="tiersdosOrgAddressType">
                    <xsl:choose>
                      <xsl:when test="$thirdPartyAddress/s0:CompanyName/text()!=''">ThirdPartyAddress</xsl:when>
                      <xsl:otherwise>ReceivingForwarderAddress</xsl:otherwise>
                    </xsl:choose>
                  </xsl:variable>

                  <xsl:variable name="tiersdosCode">
                    <xsl:call-template name="GetRegistrationNumber">
                      <xsl:with-param name="addressType" select="$tiersdosOrgAddressType"/>
                      <xsl:with-param name="registrationNumberType" select="$registrationNumberType"/>
                    </xsl:call-template>
                  </xsl:variable>
                  <xsl:variable name="tiersdosOrg" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()=$tiersdosOrgAddressType]" />
                  <xsl:variable name="tiersdosOrgCompanyName" select="$tiersdosOrg/s0:CompanyName/text()" />

                  <xsl:if test="$tiersdosCode!='' or $tiersdosOrgCompanyName!=''">
                    <ns0:tiers-dos>
                      <xsl:call-template name="CreateAttribute">
                        <xsl:with-param name="name" select="'code'" />
                        <xsl:with-param name="value" select="$tiersdosCode" />
                      </xsl:call-template>
                      <xsl:call-template name="CreateAttribute">
                        <xsl:with-param name="name" select="'nom'" />
                        <xsl:with-param name="value" select="$tiersdosOrgCompanyName" />
                      </xsl:call-template>
                      <xsl:attribute name="type">FW</xsl:attribute>
                    </ns0:tiers-dos>
                  </xsl:if>

                  <xsl:variable name="freightAgentID">
                    <xsl:call-template name="GetRegistrationNumberWithFallBack">
                      <xsl:with-param name="addressType" select="'ShippingLineAddress'"/>
                      <xsl:with-param name="serviceProvider" select="$serviceProvider"/>
                    </xsl:call-template>
                  </xsl:variable>

                  <ns0:manifest-dos>
                    <xsl:attribute name="typ">BL</xsl:attribute>
                    <xsl:call-template name="CreateAttribute">
                      <xsl:with-param name="name" select="'agentFret'" />
                      <xsl:with-param name="value" select="$freightAgentID" />
                    </xsl:call-template>
                    <xsl:call-template name="CreateAttribute">
                      <xsl:with-param name="name" select="'refdoc'" />
                      <xsl:with-param name="value" select="s0:WayBillNumber/text()"/>
                    </xsl:call-template>
                  </ns0:manifest-dos>
                </ns0:dossier-import>
              </xsl:when>
              <xsl:otherwise>
                <ns0:dossier-export>
                  <ns0:reference-dos>
                    <xsl:call-template name="CreateAttribute">
                      <xsl:with-param name="name" select="'ext'" />
                      <xsl:with-param name="value" select="$ffwRef"/>
                    </xsl:call-template>
                    <xsl:call-template name="CreateAttribute">
                      <xsl:with-param name="name" select="'tri'" />
                      <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ImplicitAcknowledgement']/s0:Value/text()"/>
                    </xsl:call-template>
                    <xsl:call-template name="CreateAttribute">
                      <xsl:with-param name="name" select="'tre'" />
                      <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ExplicitAcknowledgement']/s0:Value/text()"/>
                    </xsl:call-template>
                    <xsl:call-template name="CreateAttribute">
                      <xsl:with-param name="name" select="'baete'" />
                      <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ExplicitBAET']/s0:Value/text()"/>
                    </xsl:call-template>
                    <xsl:call-template name="CreateAttribute">
                      <xsl:with-param name="name" select="'baeti'" />
                      <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ImplicitBAET']/s0:Value/text()"/>
                    </xsl:call-template>
                    <xsl:call-template name="CreateAttribute">
                      <xsl:with-param name="name" select="'ndu'" />
                      <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='UniqueDeclaration']/s0:Value/text()"/>
                    </xsl:call-template>
                    <xsl:call-template name="CreateAttribute">
                      <xsl:with-param name="name" select="'ndp'" />
                      <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='MultipleDeclaration']/s0:Value/text()"/>
                    </xsl:call-template>
                    <xsl:call-template name="CreateAttribute">
                      <xsl:with-param name="name" select="'last'" />
                      <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='LastDeclaration']/s0:Value/text()"/>
                    </xsl:call-template>
                    <xsl:call-template name="CreateAttribute">
                      <xsl:with-param name="name" select="'comp'" />
                      <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='FileComplete']/s0:Value/text()"/>
                    </xsl:call-template>
                    <xsl:call-template name="CreateAttribute">
                      <xsl:with-param name="name" select="'nb-decl'" />
                      <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='NumberOfDeclarations']/s0:Value/text()"/>
                    </xsl:call-template>

                    <ns0:commentaires-dos>
                      <xsl:value-of select="s0:NoteCollection/s0:Note[s0:Description/text()='Carrier Booking Notes']/s0:NoteText/text()"/>
                    </ns0:commentaires-dos>
                  </ns0:reference-dos>

                  <xsl:variable name="thirdPartyAddress" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='ThirdPartyAddress']" />
                  <xsl:variable name="tiersdosOrgAddressType">
                    <xsl:choose>
                      <xsl:when test="$thirdPartyAddress/s0:CompanyName/text()!=''">ThirdPartyAddress</xsl:when>
                      <xsl:when test="contains($documentName, 'Export')">SendingForwarderAddress</xsl:when>
                    </xsl:choose>
                  </xsl:variable>

                  <xsl:variable name="tiersdosCode">
                    <xsl:call-template name="GetRegistrationNumber">
                      <xsl:with-param name="addressType" select="$tiersdosOrgAddressType"/>
                      <xsl:with-param name="registrationNumberType" select="$registrationNumberType"/>
                    </xsl:call-template>
                  </xsl:variable>
                  <xsl:variable name="tiersdosOrg" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()=$tiersdosOrgAddressType]" />
                  <xsl:variable name="tiersdosOrgCompanyName" select="$tiersdosOrg/s0:CompanyName/text()" />

                  <xsl:if test="$tiersdosCode!='' or $tiersdosOrgCompanyName!=''">
                    <ns0:tiers-dos>
                      <xsl:call-template name="CreateAttribute">
                        <xsl:with-param name="name" select="'code'" />
                        <xsl:with-param name="value" select="$tiersdosCode" />
                      </xsl:call-template>
                      <xsl:call-template name="CreateAttribute">
                        <xsl:with-param name="name" select="'nom'" />
                        <xsl:with-param name="value" select="$tiersdosOrgCompanyName" />
                      </xsl:call-template>
                      <xsl:call-template name="CreateAttribute">
                        <xsl:with-param name="name" select="'ref'" />
                        <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ThirdParty_Reference']/s0:Value/text()"/>
                      </xsl:call-template>
                      <xsl:call-template name="CreateAttribute">
                        <xsl:with-param name="name" select="'nad'" />
                        <xsl:with-param name="value" select="$tiersdosOrg/s0:Port/text()"/>
                      </xsl:call-template>
                      <ns0:commentaires-dos>
                        <xsl:value-of select="s0:NoteCollection/s0:Note[s0:Description/text()='Third Party Notes']/s0:NoteText/text()"/>
                      </ns0:commentaires-dos>
                    </ns0:tiers-dos>
                  </xsl:if>

                  <xsl:variable name="freightAgentID">
                    <xsl:call-template name="GetRegistrationNumberWithFallBack">
                      <xsl:with-param name="addressType" select="'ShippingLineAddress'"/>
                      <xsl:with-param name="serviceProvider" select="$serviceProvider"/>
                    </xsl:call-template>
                  </xsl:variable>

                  <xsl:if test="contains($documentName, 'Export')">
                    <xsl:variable name="atpValue" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ATP']/s0:Value/text()" />
                    <xsl:variable name="armementValue">
                      <xsl:choose>
                        <xsl:when test="$atpValue!=''">
                          <xsl:value-of select="$atpValue"/>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='OTC']/s0:Value/text()"/>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>
                    <xsl:for-each select="s0:AdditionalReferenceCollection/s0:AdditionalReference[s0:Type='BKG' and s0:ReferenceNumber/text()!='']">
                      <ns0:booking-dos>
                        <xsl:call-template name="CreateAttribute">
                          <xsl:with-param name="name" select="'refBooking'" />
                          <xsl:with-param name="value" select="s0:ReferenceNumber/text()"/>
                        </xsl:call-template>
                        <xsl:call-template name="CreateAttribute">
                          <xsl:with-param name="name" select="'agentFret'" />
                          <xsl:with-param name="value" select="$freightAgentID" />
                        </xsl:call-template>
                        <xsl:call-template name="CreateAttribute">
                          <xsl:with-param name="name" select="'armement'" />
                          <xsl:with-param name="value" select="$armementValue" />
                        </xsl:call-template>
                      </ns0:booking-dos>
                    </xsl:for-each>
                  </xsl:if>
                </ns0:dossier-export>
              </xsl:otherwise>
            </xsl:choose>
          </ns0:Request>
        </ns0:Messages>

      </ns0:MessageSet>
    </ns0:Interchanges>
  </xsl:template>


  <xsl:template name="GetRegistrationNumberWithFallBack">
    <xsl:param name="addressType" />
    <xsl:param name="serviceProvider"/>

    <xsl:variable name="registrationNumberType">
      <xsl:choose>
        <xsl:when test="$serviceProvider='SOGET'">SOA</xsl:when>
        <xsl:otherwise>CI5</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="fallbackRegistrationNumberType">
      <xsl:choose>
        <xsl:when test="$serviceProvider='SOGET'">SON</xsl:when>
        <xsl:otherwise></xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:call-template name="GetRegistrationNumber">
      <xsl:with-param name="addressType" select="$addressType" />
      <xsl:with-param name="registrationNumberType" select="$registrationNumberType"/>
      <xsl:with-param name="fallbackRegistrationNumberType" select="$fallbackRegistrationNumberType"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="GetRegistrationNumber">
    <xsl:param name="addressType" />
    <xsl:param name="registrationNumberType"/>
    <xsl:param name="fallbackRegistrationNumberType"/>

    <xsl:variable name="org" select="$shipment/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType=$addressType]" />
    <xsl:choose>
      <xsl:when test="$org">
        <xsl:variable name="RegNumber" select="$org/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()=$registrationNumberType]/s0:Value/text()" />
        <xsl:choose>
          <xsl:when test="$RegNumber!=''">
            <xsl:value-of select="$RegNumber"/>
          </xsl:when>
          <xsl:when test="$fallbackRegistrationNumberType!=''">
            <xsl:value-of select="$org/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()=$fallbackRegistrationNumberType]/s0:Value/text()" />
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="CreateAttribute">
    <xsl:param name="name" />
    <xsl:param name="value" />

    <xsl:if test="$value!=''">
      <xsl:attribute name="{$name}">
        <xsl:value-of select="$value" />
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
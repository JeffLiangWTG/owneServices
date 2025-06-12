<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 CodeMapper ContextAccessor DataModelAccessor DateMapper userCSharp" version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2012/11/TRC/1"
                xmlns:ns0="http://www.cargowise.com/Schemas/FPM/APPLUS/TRC"
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

  <xsl:variable name="parseClientRegistration" select="userCSharp:ParseClientRegistration(DataModelAccessor:GetClientRegistrationCode($senderID, $operationPortCode, $serviceProvider))" />
  <xsl:variable name="applusID" select="userCSharp:GetUser()" />
  <xsl:variable name="applusTierProf" select="userCSharp:GetTiersProf()" />

  <xsl:template match="s0:UniversalShipment/s0:Shipment">
    <xsl:variable name="interchangeID" select="CodeMapper:CallActionProcedureHelper('GetCounterValue','@value','@name','CargoWise.eHub.Products.ForwardingPortMessaging.FR.APPLUS.Interchange','@maxlength','14')" />
    <xsl:variable name="InboxPK" select="ContextAccessor:GetContextProperty('InternalTrackingID', 'http://cargowise.com/ehub/tracking/2010/06')"/>

    <xsl:variable name="FileName" select="ContextAccessor:SetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06', concat('TRC_', $senderID, '_', $interchangeID))"/>
    <xsl:variable name="EmailSubject" select="ContextAccessor:SetContextProperty('OverrideEmailSubject', 'http://cargowise.com/ehub/processing/2010/06', concat('FPM_', $serviceProvider, '_', $operationPortCode))"/>

    <xsl:if test="$newRecipientID!=$recipientID">
      <xsl:variable name="SetRecipientID" select="ContextAccessor:SetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties', $newRecipientID)"/>
    </xsl:if>

    <xsl:variable name="subscribeClientID" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderID, $serviceProvider, $senderID, concat($applusID, ',', $applusTierProf))" />

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

      <xsl:variable name="shipmentContainerMode" select="s0:ContainerMode/text()"/>
      <xsl:variable name="wayBillNumber" select="s0:WayBillNumber/text()"/>
      <xsl:variable name="bookingConfirmationReference" select="s0:BookingConfirmationReference/text()"/>
      <xsl:variable name="carrierCode">
        <xsl:call-template name="GetOrgAddressRegNumber">
          <xsl:with-param name="addressType" select="'ShippingLineAddress'"/>
        </xsl:call-template>
      </xsl:variable>

      <xsl:choose>
        <xsl:when test="s0:ContainerCollection/s0:Container">
          <xsl:for-each select="s0:ContainerCollection/s0:Container">

            <xsl:call-template name="MessageSet">
              <xsl:with-param name="interchangeID" select="$interchangeID"/>
              <xsl:with-param name="containerNumber" select="s0:ContainerNumber/text()"/>
              <xsl:with-param name="shipmentContainerMode" select="$shipmentContainerMode"/>
              <xsl:with-param name="waybillNumber" select="$wayBillNumber"/>
              <xsl:with-param name="bookingConfirmationReference" select="$bookingConfirmationReference"/>
              <xsl:with-param name="carrierCode" select="$carrierCode"/>
              <xsl:with-param name="inboxPK" select="$InboxPK"/>
            </xsl:call-template>
          </xsl:for-each>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="MessageSet">
            <xsl:with-param name="interchangeID" select="$interchangeID" />
            <xsl:with-param name="containerNumber" select="''"/>
            <xsl:with-param name="shipmentContainerMode" select="$shipmentContainerMode"/>
            <xsl:with-param name="waybillNumber" select="$wayBillNumber"/>
            <xsl:with-param name="bookingConfirmationReference" select="$bookingConfirmationReference"/>
            <xsl:with-param name="carrierCode" select="$carrierCode"/>
            <xsl:with-param name="inboxPK" select="$InboxPK"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </ns0:Interchanges>
  </xsl:template>

  <xsl:template name="MessageSet">
    <xsl:param name="interchangeID"/>
    <xsl:param name="containerNumber"/>
    <xsl:param name="shipmentContainerMode"/>
    <xsl:param name="waybillNumber"/>
    <xsl:param name="bookingConfirmationReference"/>
    <xsl:param name="carrierCode"/>
    <xsl:param name="inboxPK"/>

    <xsl:variable name="consolID" select="$shipment/s0:DataContext/s0:DataSource/s0:Key/text()"/>
    <xsl:variable name="purpose" select="$shipment/s0:DataContext/s0:DocumentaryOverride/s0:Purpose/text()" />
    <xsl:variable name="documentName" select="$shipment/s0:DataContext/s0:DocumentaryOverride/s0:DocumentName/text()" />

    <xsl:variable name="messageSetID" select="CodeMapper:CallActionProcedureHelper('GetCounterValue','@value','@name','CargoWise.eHub.Products.ForwardingPortMessaging.FR.APPLUS','@maxlength','14')" />
    <xsl:variable name="formattedCounter" select='format-number($messageSetID, "0000000000")' />
    <xsl:variable name="messageIdentifier" select="concat($msgPrefix, $formattedCounter)" />
    <xsl:variable name="SubscribeInboxPK" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $inboxPK, concat($interchangeID, '_', $messageIdentifier))" />

    <xsl:variable name="newConsolReference" select="concat($consolID, '_', $containerNumber , '_', 'TRC')" />
    <xsl:variable name="previousConsolReference" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $serviceProvider, '@recipientId', $senderID , '@ST_ID', $serviceProviderMSGID, '@value', $newConsolReference, '@referenceType', 'JobNumber')" />

    <xsl:variable name="messageId">
      <xsl:choose>
        <xsl:when test="$previousConsolReference!=''">
          <xsl:value-of select="$previousConsolReference" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:variable name="newinterchangeID" select="CodeMapper:CallActionProcedureHelper('GetCounterValue','@value','@name','CargoWise.eHub.Products.ForwardingPortMessaging.FR.APPLUS','@maxlength','14')" />
          <xsl:variable name="newformattedCounter" select='format-number($newinterchangeID, "0000000000")' />
          <xsl:variable name="newformattedInterchangeId" select="concat($msgPrefix, $newformattedCounter)" />
          <xsl:variable name="subscribeJobNumber1" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $newformattedInterchangeId, $newConsolReference, 'JobNumber')" />
          <xsl:variable name="subscribeJobNumber2" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $newConsolReference, $newformattedInterchangeId, 'JobNumber')" />

          <xsl:variable name="subscribeShipmentType" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $newformattedInterchangeId, $shipment/s0:DataContext/s0:DataSource/s0:Type/text(), 'ForwardingType')" />
          <xsl:variable name="subscribeDocumentName" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $newformattedInterchangeId, $documentName, 'DocumentName')" />
          <xsl:variable name="SubscribeRequestID" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $newformattedInterchangeId, $messageIdentifier, 'RequestID')" />
          <xsl:variable name="SubscribeContainerNumber" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $newformattedInterchangeId, s0:ContainerNumber/text(), 'ContainerNumber')" />

          <xsl:value-of select="$newformattedInterchangeId" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="subscribeActionPurpose" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageId, $purpose, 'Purpose')" />
    <xsl:variable name="SubscribeBookingConfirmationReference" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageId, $bookingConfirmationReference, 'BookingConfirmationReference')" />
    <xsl:variable name="SubscribeWayBillNumber" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageId, $waybillNumber, 'WayBillNumber')" />
    <xsl:variable name="SubscribeCarrierCode" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageId, $carrierCode, 'Carrier')" />
    <xsl:variable name="subscribeOperationPort" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageId, $operationPortCode, 'OperationPort')" />

    <ns0:MessageSet>
      <xsl:attribute name="id">
        <xsl:value-of select="$messageId"/>
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
          <xsl:attribute name="id">
            <xsl:value-of select="$messageId"/>
          </xsl:attribute>
          <xsl:attribute name="type">TRC</xsl:attribute>

          <ns0:tracing>
            <ns0:reference-tracing>
              <xsl:attribute name="ref">
                <xsl:value-of select="$messageId"/>
              </xsl:attribute>
              <xsl:attribute name="statut">
                <xsl:choose>
                  <xsl:when test="$purpose='ORG' or $purpose='AMD'">ACT</xsl:when>
                  <xsl:when test="$purpose='WTH'">INA</xsl:when>
                  <xsl:otherwise></xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>

              <xsl:variable name="monitoringStartDate">
                <xsl:call-template name="GetMonitoringDate">
                  <xsl:with-param name="documentName" select="$documentName"/>
                  <xsl:with-param name="exportDepartureTime" select="'-15'"/>
                  <xsl:with-param name="exportFallbackTime" select="'-10'"/>
                  <xsl:with-param name="importArrivalTime" select="'-10'"/>
                  <xsl:with-param name="împortFallbackTime" select="'-10'"/>
                </xsl:call-template>
              </xsl:variable>

              <xsl:call-template name="CreateAttribute">
                <xsl:with-param name="name" select="'horodebut'" />
                <xsl:with-param name="value" select="$monitoringStartDate" />
              </xsl:call-template>

              <xsl:variable name="monitoringEndDate">
                <xsl:call-template name="GetMonitoringDate">
                  <xsl:with-param name="documentName" select="$documentName"/>
                  <xsl:with-param name="exportDepartureTime" select="'20'"/>
                  <xsl:with-param name="exportFallbackTime" select="'30'"/>
                  <xsl:with-param name="importArrivalTime" select="'20'"/>
                  <xsl:with-param name="împortFallbackTime" select="'30'"/>
                </xsl:call-template>
              </xsl:variable>
              <xsl:call-template name="CreateAttribute">
                <xsl:with-param name="name" select="'horofin'" />
                <xsl:with-param name="value" select="$monitoringEndDate" />
              </xsl:call-template>

              <xsl:call-template name="CreateAttribute">
                <xsl:with-param name="name" select="'refum'" />
                <xsl:with-param name="value" select="$containerNumber" />
              </xsl:call-template>
            </ns0:reference-tracing>

            <xsl:variable name="operationPortCountryCode" select="substring($operationPortCode, 0, 2)" />
            <xsl:choose>
              <xsl:when test="contains($documentName, 'Export')">
                <ns0:evenement-tracing export="Y" code="ANDCBK"/>
                <ns0:evenement-tracing export="Y" code="AMQVA"/>
                <ns0:evenement-tracing export="Y" code="RECU"/>
                <ns0:evenement-tracing export="Y" code="SORTIEUE"/>
                <ns0:evenement-tracing export="Y" code="BAED"/>
                <ns0:evenement-tracing export="Y" code="BASE"/>
                <ns0:evenement-tracing export="Y" code="VAB"/>
                <ns0:evenement-tracing export="Y" code="DEPNAV"/>
              </xsl:when>
              <xsl:when test="contains($documentName, 'Import')">
                <ns0:evenement-tracing import="Y" code="VAQ" />
                <ns0:evenement-tracing import="Y" code="ARRNAV" />
                <ns0:evenement-tracing import="Y" code="BAD" />
                <ns0:evenement-tracing import="Y" code="RECU" />
                <ns0:evenement-tracing import="Y" code="APD" />
                <ns0:evenement-tracing import="Y" code="ENLEVEMENT" />
                <xsl:choose>
                  <xsl:when test="contains('LCL;GRP', $shipmentContainerMode)">
                    <ns0:evenement-tracing import="Y" code="TRANSF" />
                  </xsl:when>
                  <xsl:otherwise>
                    <ns0:evenement-tracing import="Y" code="BAED" />
                    <ns0:evenement-tracing import="Y" code="BASI" />
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:when>
              <xsl:otherwise>
                <ns0:evenement-tracing code=""/>
              </xsl:otherwise>
            </xsl:choose>
          </ns0:tracing>
        </ns0:Request>
      </ns0:Messages>
    </ns0:MessageSet>
  </xsl:template>

  <xsl:template name="GetMonitoringDate">
    <xsl:param name="documentName"/>
    <xsl:param name="exportDepartureTime"/>
    <xsl:param name="exportFallbackTime"/>
    <xsl:param name="importArrivalTime"/>
    <xsl:param name="împortFallbackTime"/>

    <xsl:variable name="dateType">
      <xsl:choose>
        <xsl:when test="contains($documentName, 'Export')">Departure</xsl:when>
        <xsl:when test="contains($documentName, 'Import')">Arrival</xsl:when>
        <xsl:otherwise></xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="triggerDate" select="DateMapper:ConvertXmlDateString($shipment/s0:DataContext/s0:Workflow/s0:TriggerDate/text(), 'dd/MM/yyyy HH:mm')"/>
    <xsl:variable name="dateValue">
      <xsl:choose>
        <xsl:when test="$dateType!=''">
          <xsl:value-of select="DateMapper:ConvertXmlDateString($shipment/s0:DateCollection/s0:Date[s0:Type/text() = $dateType]/s0:Value/text(), 'dd/MM/yyyy HH:mm')"/>
        </xsl:when>
        <xsl:otherwise></xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test="$dateType='Departure'">
        <xsl:choose>
          <xsl:when test="$dateValue!=''">
            <xsl:value-of select="userCSharp:ConvertToDateTimeString($dateValue, 'dd/MM/yyyy HH:mm', $exportDepartureTime)"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="userCSharp:ConvertToDateTimeString($triggerDate, 'dd/MM/yyyy HH:mm', $exportFallbackTime)"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="$dateType='Arrival'">
        <xsl:choose>
          <xsl:when test="$dateValue!=''">
            <xsl:value-of select="userCSharp:ConvertToDateTimeString($dateValue, 'dd/MM/yyyy HH:mm', $importArrivalTime)"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="userCSharp:ConvertToDateTimeString($triggerDate, 'dd/MM/yyyy HH:mm', $împortFallbackTime)"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
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

  <xsl:template name="GetOrgAddressRegNumber">
    <xsl:param name="addressType"/>

    <xsl:variable name="orgAddress" select="$shipment/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()=$addressType]"/>
    <xsl:choose>
      <xsl:when test="$orgAddress">
        <xsl:variable name="USCCC_Code" select="$orgAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='CCC' and s0:CountryOfIssue/text()='US']/s0:Value/text()"/>
        <xsl:choose>
          <xsl:when test="$USCCC_Code!=''">
            <xsl:value-of select="$USCCC_Code"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$orgAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='C1C']/s0:Value/text()"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[
public string ConvertToDateTimeString(string dateVal, string dateValFmt, int daysToAdd)
{
  DateTime parsedDate = DateTime.MinValue;
  DateTime parsedTime = DateTime.MinValue;

  if (String.IsNullOrWhiteSpace(dateVal))
  {
    return String.Empty;
  }

  var parsedOk = String.IsNullOrWhiteSpace(dateValFmt) ?
    DateTime.TryParse(dateVal, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out parsedDate) :
    DateTime.TryParseExact(dateVal, dateValFmt, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out parsedDate);

  if (!parsedOk)
  {
    return String.Empty;
  }

  return parsedDate.AddDays(daysToAdd).ToString(dateValFmt, System.Globalization.CultureInfo.InvariantCulture);
}

public void ParseClientRegistration(string inputText)
{
  tiersProf = string.Empty;
  user = string.Empty;
  if (!string.IsNullOrEmpty(inputText))
  {
    inputText = inputText.Replace(" ", "");
    if (inputText.Contains(","))
    {
      user = inputText.Split(new[] { ',' })[0];
      tiersProf = inputText.Split(new[] { ',' })[1];
    }
    else
    {
      user = inputText;
    }
  }
}

public string tiersProf;
public string user;

public string GetUser()
{
  return user;
}

public string GetTiersProf()
{
  return tiersProf;
}
]]>
  </msxsl:script>
</xsl:stylesheet>

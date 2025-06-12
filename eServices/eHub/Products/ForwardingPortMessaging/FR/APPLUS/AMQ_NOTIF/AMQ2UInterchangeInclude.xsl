<?xml version="1.0" encoding="Windows-1252"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl ns0 s0 CodeMapper ContextAccessor DataModelAccessor DateMapper userCSharp" version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/FPM/APPLUS"
                xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2012/11"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
                xmlns:DataModelAccessor="http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"
                xmlns:DateMapper="http://schemas.microsoft.com/BizTalk/2003/DateMapper"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">

  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />

  <xsl:template match="/">
    <xsl:apply-templates select="s0:Interchanges" />
  </xsl:template>

  <xsl:variable name="SenderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')" />
  <xsl:variable name="RecipientID" select="ContextAccessor:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="serviceProviderID" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Port Settings' , 'ID', $SenderID)"/>
  <xsl:variable name="serviceProviderMSGID" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Port Settings' , 'MSGID', $SenderID)"/>
  <xsl:variable name="serviceProviderPrefix" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Port Settings' , 'SubscriptionPrefix', $SenderID)"/>
  <xsl:variable name="serviceProvider" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Port Settings' , 'Name', $SenderID)"/>
  <xsl:variable name="serviceProviderSTID" select="concat($serviceProviderPrefix, 'SIC')" />

  <xsl:template match="s0:Interchanges">
    <xsl:variable name="applusID" select="MessageSet/Destinataire/@user" />
    <xsl:variable name="applusTierProf" select="MessageSet/Destinataire/@tiersProf" />
    <xsl:variable name="eHubID" select="DataModelAccessor:GeteHubIDByCode(concat($applusID, ',', $applusTierProf), $SenderID)" />
    <xsl:variable name="applusRecipientId">
      <xsl:choose>
        <xsl:when test="$eHubID!=''">
          <xsl:value-of select="$eHubID"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="CodeMapper:CallActionProcedureHelper('SelectSubscribedClients', '', '@senderId', $SenderID, '@ST_ID', $serviceProviderID, '@value', concat($applusID, ',', $applusTierProf))"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <UniversalInterchangeInclude xmlns="http://www.cargowise.com/Schemas/Universal/2012/11">
      <Body>
        <xsl:for-each select="MessageSet/Messages">
          <xsl:variable name="notificationAmq" select="Notification/amq" />
          <xsl:variable name="notificationType" select="Notification/@type" />
          <xsl:variable name="notificationAction" select="Notification/@action" />
          <xsl:variable name="notificationRef" select="Notification/@ref" />
          <xsl:variable name="notificationID" select="Notification/@id" />
          <xsl:choose>
            <xsl:when test="$notificationAmq/equipement-amq">
              <xsl:variable name="clearContainerList" select="userCSharp:ClearContainerLists()" />

              <xsl:for-each select="$notificationAmq/equipement-amq">
                <xsl:call-template name="CreateUniversalEvent">
                  <xsl:with-param name="notificationAmq" select="$notificationAmq" />
                  <xsl:with-param  name="equipementAmq" select="." />
                  <xsl:with-param  name="notificationType" select="$notificationType" />
                  <xsl:with-param  name="notificationAction" select="$notificationAction" />
                  <xsl:with-param  name="notificationRef" select="$notificationRef" />
                  <xsl:with-param name="sicNumber" select="$notificationID" />
                </xsl:call-template>
              </xsl:for-each>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="CreateUniversalEvent">
                <xsl:with-param name="notificationAmq" select="$notificationAmq" />
                <xsl:with-param  name="notificationType" select="$notificationType" />
                <xsl:with-param  name="notificationAction" select="$notificationAction" />
                <xsl:with-param  name="notificationRef" select="$notificationRef" />
                <xsl:with-param name="sicNumber" select="$notificationID" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:for-each>
      </Body>
    </UniversalInterchangeInclude>

    <xsl:variable name="messageRecipientID">
      <xsl:call-template name="GetValue">
        <xsl:with-param name="value" select="userCSharp:GetRecipientID()" />
        <xsl:with-param name="value1" select="$applusRecipientId" />
      </xsl:call-template>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test="$messageRecipientID != ''">
        <xsl:variable name="SetDestinationParty" select="ContextAccessor:SetContextProperty('DestinationParty','http://schemas.microsoft.com/BizTalk/2003/system-properties', $messageRecipientID)"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="userCSharp:ThrowNoRecipientID()"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="CreateUniversalEvent">
    <xsl:param name="notificationAmq" />
    <xsl:param name="equipementAmq" />
    <xsl:param name="notificationType" />
    <xsl:param name="notificationAction" />
    <xsl:param name="notificationRef" />
    <xsl:param name="sicNumber" />

    <xsl:variable name="referenceAmq" select="$notificationAmq/reference-amq" />
    <xsl:variable name="voyageAmq" select="$notificationAmq/voyage-amq" />
    <xsl:variable name="lieuAmq" select="$notificationAmq/lieu-amq" />
    <xsl:variable name="tiersAmq" select="$notificationAmq/tiers-amq" />
    <!--<xsl:variable name="sicNumber" select="$referenceAmq/@sic"/>-->

    <xsl:variable name="amqLookupType"  select="CodeMapper:GetRecipientCode($serviceProvider, $serviceProvider, concat($serviceProvider, ' System Configuration'),  'AMQ Subscription Lookup' , 'Subscription Type', $notificationType)"/>
    <xsl:variable name="amqLookupFallbackType"  select="CodeMapper:GetRecipientCode($serviceProvider, $serviceProvider, concat($serviceProvider, ' System Configuration'),  'AMQ Subscription Lookup' , 'Fallback Type', $notificationType)"/>
    <xsl:variable name="subscribedMessageReference" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $serviceProviderSTID, '@value', concat($amqLookupType, '_', $sicNumber))"/>
    <xsl:variable name="messageReference">
      <xsl:choose>
        <xsl:when test="$subscribedMessageReference!=''">
          <xsl:variable name="SetNotificationType" select="userCSharp:SetNotificationType($amqLookupType)" />
          <xsl:value-of select="$subscribedMessageReference"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:variable name="SetNotificationType" select="userCSharp:SetNotificationType($amqLookupFallbackType)" />
          <xsl:value-of select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $serviceProviderSTID, '@value', concat($amqLookupFallbackType, '_', $sicNumber))"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="subscribedJobNo" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $serviceProviderMSGID, '@value', $messageReference, '@referenceType', 'JobNumber')"/>
    <xsl:variable name="subscribedForwardingType" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $serviceProviderMSGID, '@value', $messageReference, '@referenceType', 'ForwardingType')" />
    <xsl:variable name="subscribedDocumentName" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $serviceProviderMSGID, '@value', $messageReference, '@referenceType', 'DocumentName')" />

    <xsl:variable name="eventNotificationType" select="userCSharp:GetNotificationType()" />
    <xsl:variable name="jobType">
      <xsl:choose>
        <xsl:when test="contains($subscribedDocumentName, 'Export')">Export</xsl:when>
        <xsl:when test="$notificationType='BASE' and $eventNotificationType='AMQ'">Export</xsl:when>
        <xsl:when test="$eventNotificationType='CRESA'">Export</xsl:when>
        <xsl:otherwise>Import</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="getRecipientID" select="userCSharp:GetRecipientID()" />
    <xsl:if test="$getRecipientID=''">
      <xsl:variable name="subscriptionClient" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedClients', '', '@senderId', $SenderID, '@ST_ID', $serviceProviderMSGID, '@value', $messageReference)" />
      <xsl:variable name="setRecipientID" select="userCSharp:SetRecipientID($subscriptionClient)" />
    </xsl:if>

    <xsl:variable name="containerNumber">
      <xsl:choose>
        <xsl:when test="$equipementAmq">
          <xsl:value-of select="$equipementAmq/@id" />
        </xsl:when>
        <xsl:otherwise></xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:if test="userCSharp:ShouldCreateUniversalEvent($containerNumber)">
      <xsl:variable name="containerISO">
        <xsl:choose>
          <xsl:when test="$equipementAmq">
            <xsl:value-of select="$equipementAmq/@code" />
          </xsl:when>
          <xsl:otherwise></xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="dataTargetType">
        <xsl:choose>
          <xsl:when test="$subscribedForwardingType!=''">
            <xsl:value-of select="$subscribedForwardingType" />
          </xsl:when>
          <xsl:when test="$notificationRef=$containerNumber and $containerNumber!=''">ForwardingConsol</xsl:when>
          <xsl:when test="starts-with($notificationRef, 'S')">ForwardingShipment</xsl:when>
          <xsl:when test="starts-with($notificationRef, 'RC')">TransitReceive</xsl:when>
          <xsl:otherwise>ForwardingConsol</xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <UniversalEvent xmlns="http://www.cargowise.com/Schemas/Universal/2012/11">
        <Event>
          <DataContext>
            <xsl:if test="$subscribedDocumentName!=''">
              <DocumentaryOverride>
                <DocumentName>
                  <xsl:value-of select="$subscribedDocumentName"/>
                </DocumentName>
              </DocumentaryOverride>
            </xsl:if>
            <DataTargetCollection>
              <DataTarget>
                <Key>
                  <xsl:choose>
                    <xsl:when test="$dataTargetType='ForwardingConsol'">
                      <xsl:value-of select="$subscribedJobNo"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:call-template name="GetValue">
                        <xsl:with-param name="value" select="$subscribedJobNo" />
                        <xsl:with-param name="value1" select="$notificationRef" />
                      </xsl:call-template>
                    </xsl:otherwise>
                  </xsl:choose>
                </Key>
                <Type>
                  <xsl:value-of select="$dataTargetType"/>
                </Type>
              </DataTarget>
            </DataTargetCollection>
          </DataContext>

          <EventTime>
            <xsl:value-of select="DateMapper:ConvertXmlDateString(string($referenceAmq/@date), 'yyyy-MM-ddTHH:mm:ss')" />
          </EventTime>
          <EventType>
            <xsl:value-of select="CodeMapper:GetRecipientCode($serviceProvider, $serviceProvider, concat($serviceProvider, ' System Configuration'),  'AMQ Event Type' , 'Event Type', $notificationType, $notificationAction, $jobType)"/>
          </EventType>

          <xsl:variable name="eventParameters" select="CodeMapper:GetRecipientCode($serviceProvider, $serviceProvider, concat($serviceProvider, ' System Configuration'),  'AMQ Event Type', 'Event Parameters', $notificationType, $notificationAction, $jobType)"/>
          <EventParameters>
            <xsl:copy-of select="userCSharp:CreateEventParameters($eventParameters)"/>

            <xsl:if test="$containerNumber!=''">
              <EquipmentReferenceNumber>
                <xsl:value-of select="$containerNumber"/>
              </EquipmentReferenceNumber>
            </xsl:if>

            <xsl:if test="userCSharp:ShouldCreateParameter('CustomsReferenceNumber')">
              <CustomsReferenceNumber>
                <xsl:value-of select="$sicNumber" />
              </CustomsReferenceNumber>
            </xsl:if>

            <xsl:if test="userCSharp:ShouldCreateParameter('Location')">
              <xsl:variable name="amqZone" select="$lieuAmq/@zone" />
              <xsl:variable name="zone">
                <xsl:choose>
                  <xsl:when test="$amqZone!=''">
                    <xsl:value-of select="CodeMapper:GetRecipientCode($serviceProvider, $serviceProvider, concat($serviceProvider, ' System Configuration'),  'Zone UNLOCO' , 'Output Code', $amqZone)"/>
                  </xsl:when>
                  <xsl:otherwise></xsl:otherwise>
                </xsl:choose>
              </xsl:variable>
              <xsl:if test="$zone!=''">
                <Location>
                  <xsl:value-of select="$zone" />
                </Location>
              </xsl:if>
            </xsl:if>

            <xsl:if test="userCSharp:ShouldCreateParameter('Status')">
              <Status>
                <xsl:value-of select="$referenceAmq/@statut" />
              </Status>
            </xsl:if>
          </EventParameters>

          <xsl:variable name="eventRef" select="CodeMapper:GetRecipientCode($serviceProvider, $serviceProvider, concat($serviceProvider, ' System Configuration'),  'AMQ Event Type' , 'Event Reference', $notificationType, $notificationAction, $jobType)"/>
          <xsl:if test="$eventRef!=''">
            <EventReference>
              <xsl:value-of select="CodeMapper:GetRecipientCode($serviceProvider, $serviceProvider, concat($serviceProvider, ' System Configuration'),  'AMQ Event Type' , 'Event Reference', $notificationType, $notificationAction, $jobType)"/>
            </EventReference>
          </xsl:if>

          <IsEstimate>
            <xsl:value-of select="CodeMapper:GetRecipientCode($serviceProvider, $serviceProvider, concat($serviceProvider, ' System Configuration'),  'AMQ Event Type' , 'Is Estimate', $notificationType, $notificationAction, $jobType)"/>
          </IsEstimate>

          <ContextCollection>
            <xsl:if test="$containerNumber!=''">
              <xsl:call-template name="CreateContext">
                <xsl:with-param name="type" select="'ContainerNumber'" />
                <xsl:with-param name="value" select="$containerNumber" />
              </xsl:call-template>

              <xsl:if test="$containerISO!=''">
                <xsl:variable name="serviceProviderContainerISO" select="CodeMapper:GetRecipientCode($serviceProvider, $serviceProvider, concat($serviceProvider, ' System Configuration'), 'ContainerTypeToISOCode' , concat($serviceProvider, ' Code'), $containerISO)"/>
                <xsl:variable name="ContainerTypeMappingCode">
                  <xsl:choose>
                    <xsl:when test="$serviceProviderContainerISO != ''">
                      <xsl:value-of select="$serviceProviderContainerISO"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration', 'ContainerTypeToISOCode' , 'Carrier Code', $containerISO)"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:variable>
                <xsl:call-template name="CreateContext">
                  <xsl:with-param name="type" select="'ContainerISOCode'"/>
                  <xsl:with-param name="value" select="$ContainerTypeMappingCode"/>
                </xsl:call-template>
              </xsl:if>
            </xsl:if>

            <xsl:call-template name="CreateContext">
              <xsl:with-param name="type" select="'MBOLNumber'" />
              <xsl:with-param name="value" select="$referenceAmq/@extdoc" />
            </xsl:call-template>
            <xsl:call-template name="CreateContext">
              <xsl:with-param name="type" select="'DOCReference'" />
              <xsl:with-param name="value" select="$referenceAmq/@doc" />
            </xsl:call-template>

            <xsl:choose>
              <xsl:when test="$dataTargetType='ForwardingShipment'">
                <xsl:call-template name="CreateContext">
                  <xsl:with-param name="type" select="'ShipmentNumber'" />
                  <xsl:with-param name="value" select="$referenceAmq/@extcbk" />
                </xsl:call-template>
              </xsl:when>
              <xsl:otherwise>
                <xsl:call-template name="CreateContext">
                  <xsl:with-param name="type" select="'CarriersBookingReference'" />
                  <xsl:with-param name="value" select="$referenceAmq/@extcbk" />
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>

            <xsl:call-template name="CreateContext">
              <xsl:with-param name="type" select="'CBKReference'" />
              <xsl:with-param name="value" select="$referenceAmq/@cbk" />
            </xsl:call-template>
            <xsl:call-template name="CreateContext">
              <xsl:with-param name="type" select="'ForwarderReference'" />
              <xsl:with-param name="value" select="$referenceAmq/@extdos" />
            </xsl:call-template>

            <xsl:call-template name="CreateContext">
              <xsl:with-param name="type" select="'VoyageNumber'" />
              <xsl:with-param name="value" select="$voyageAmq/@refotc" />
            </xsl:call-template>

            <xsl:call-template name="CreateContext">
              <xsl:with-param name="type" select="'DOSReference'" />
              <xsl:with-param name="value" select="$referenceAmq/@dos" />
            </xsl:call-template>

            <xsl:call-template name="CreateContext">
              <xsl:with-param name="type" select="'VesselName'" />
              <xsl:with-param name="value" select="$voyageAmq/@nav" />
            </xsl:call-template>
            <xsl:call-template name="CreateContext">
              <xsl:with-param name="type" select="'OTCReference'" />
              <xsl:with-param name="value" select="$voyageAmq/@otc" />
            </xsl:call-template>
            <xsl:call-template name="CreateContext">
              <xsl:with-param name="type" select="'ATPReference'" />
              <xsl:with-param name="value" select="$voyageAmq/@atp" />
            </xsl:call-template>

            <xsl:call-template name="CreateContext">
              <xsl:with-param name="type" select="'ServiceCode'" />
              <xsl:with-param name="value" select="$voyageAmq/@ser" />
            </xsl:call-template>

            <xsl:variable name="carrierCode">
              <xsl:choose>
                <xsl:when test="$tiersAmq/@afret!=''">
                  <xsl:value-of select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration', 'SCAC (Inbound)' , 'SCAC', $SenderID, $tiersAmq/@afret)"/>
                </xsl:when>
                <xsl:otherwise></xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <xsl:call-template name="CreateContext">
              <xsl:with-param name="type" select="'CarrierCode'" />
              <xsl:with-param name="value" select="$carrierCode" />
            </xsl:call-template>

            <xsl:call-template name="CreateContext">
              <xsl:with-param name="type" select="'PortArea'" />
              <xsl:with-param name="value" select="$lieuAmq/@zone" />
            </xsl:call-template>
            <xsl:call-template name="CreateContext">
              <xsl:with-param name="type" select="'PortLocation'" />
              <xsl:with-param name="value" select="$lieuAmq/@lieu" />
            </xsl:call-template>
            <xsl:call-template name="CreateContext">
              <xsl:with-param name="type" select="'TerminalCode'" />
              <xsl:with-param name="value" select="$lieuAmq/@manut" />
            </xsl:call-template>
          </ContextCollection>
        </Event>
      </UniversalEvent>
    </xsl:if>
  </xsl:template>

  <xsl:template name="CreateContext">
    <xsl:param name="type" />
    <xsl:param name="value" />

    <xsl:if test="$value!=''">
      <Context xmlns="http://www.cargowise.com/Schemas/Universal/2012/11">
        <Type>
          <xsl:value-of select="$type" />
        </Type>
        <Value>
          <xsl:value-of select="$value" />
        </Value>
      </Context>
    </xsl:if>
  </xsl:template>

  <xsl:template name="GetValue">
    <xsl:param name="value"/>
    <xsl:param name="value1" select="''"/>
    <xsl:param name="value2" select="''"/>
    <xsl:choose>
      <xsl:when test="$value!=''">
        <xsl:value-of select="$value"/>
      </xsl:when>
      <xsl:when test="$value1!=''">
        <xsl:value-of select="$value1"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$value2"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[
System.Collections.Generic.List<string> containerNumbers = new System.Collections.Generic.List<string>();

public void ClearContainerLists()
{
  containerNumbers.Clear();
}

public bool ShouldCreateUniversalEvent(string containerNumber)
{

  var result = false;
  if (!containerNumbers.Contains(containerNumber))
  {
    result = true;
    containerNumbers.Add(containerNumber);
  }

  return result;
}

string recipientID = string.Empty;
public void SetRecipientID(string inputText)
{
  if (!string.IsNullOrEmpty(inputText))
  {
    recipientID = inputText;
  }
}

public string GetRecipientID()
{
  return recipientID;
}

public XPathNodeIterator CreateEventParameters(string parameters)
{
  var doc = new XmlDocument();
  var root = doc.CreateElement("root");
  doc.AppendChild(root);
parameterWithLogic = new System.Collections.Generic.List<string>();

  if (parameters != "")
  {
    foreach (var parameter in parameters.Split('|'))
    {
      var id_value = parameter.Split('=');
      if (id_value.Length == 2)
      {
        if (id_value[1]==".")
        {
          parameterWithLogic.Add(parameter);
        }
        
        if (id_value[1]!=".")
        {
          var child = doc.CreateElement("", id_value[0], "http://www.cargowise.com/Schemas/Universal/2012/11");
          child.InnerText = id_value[1];
          root.AppendChild(child);
        }
      }
    }
  }

  return doc.CreateNavigator().Select("/*/*");
}

System.Collections.Generic.List<string> parameterWithLogic = new System.Collections.Generic.List<string>();

public bool ShouldCreateParameter(string key)
{
  if (parameterWithLogic.Count > 0)
  {
    foreach (var parameter in parameterWithLogic)
    {
      if (parameter.StartsWith(key))
      {
        return parameter.EndsWith(".");
      }
    }
  }
  return false;
}

string notificationType = "";

public void SetNotificationType(string key)
{
  notificationType = key;
}

public string GetNotificationType()
{
  return notificationType;
}

public void ThrowNoRecipientID()
{
  throw new ArgumentException("Unable to resolve recipient Id, message rejected.");
}
]]>
  </msxsl:script>
</xsl:stylesheet>
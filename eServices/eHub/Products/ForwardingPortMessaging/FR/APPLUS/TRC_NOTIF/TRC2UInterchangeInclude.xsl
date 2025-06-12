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

  <xsl:variable name="destinationParty" select="ContextAccessor:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="SenderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>

  <xsl:template match="s0:Interchanges">
    <UniversalInterchangeInclude xmlns="http://www.cargowise.com/Schemas/Universal/2012/11">
      <Body>
        <xsl:for-each select="MessageSet/Messages">

          <xsl:variable name="notificationMessage" select="Notification/statut/reference" />
          <xsl:variable name="messageReference" select="Notification/@ref"/>

          <xsl:variable name="serviceProvider" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Port Settings' , 'Name', $SenderID)"/>
          <xsl:variable name="serviceProviderMSGID" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Port Settings' , 'MSGID', $SenderID)"/>

          <xsl:variable name="RecipientID">
            <xsl:choose>
              <xsl:when test="$destinationParty='CONTAINER_TRACKING'">
                <xsl:value-of select="CodeMapper:CallActionProcedureHelper('SelectSubscribedClients', '', '@senderId', $SenderID, '@value', $messageReference, '@ST_ID', $serviceProviderMSGID)"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$destinationParty"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <!--
            0 - CONTAINER TRACKING (DEFAULT)
            1 - CARGOWISE ONE
            2 - BOTH
          -->
          <xsl:variable name="messagePartyMode">
            <xsl:choose>
              <xsl:when test="$RecipientID != ''">
                <xsl:value-of  select="CodeMapper:GetRecipientCode($serviceProvider, $serviceProvider, concat($serviceProvider, ' System Configuration'), 'TRC Event Delivery Method' , 'Method', $RecipientID)"/>
              </xsl:when>
              <xsl:otherwise>0</xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <xsl:variable name="shouldGenerateMessage">
            <xsl:choose>
              <xsl:when test="$messagePartyMode = 2">true</xsl:when>
              <xsl:when test="$messagePartyMode = 0 and $destinationParty = 'CONTAINER_TRACKING'">true</xsl:when>
              <xsl:when test="$messagePartyMode = 1 and $destinationParty != 'CONTAINER_TRACKING'">true</xsl:when>
              <xsl:otherwise>false</xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <xsl:if test="$shouldGenerateMessage='true'">

            <xsl:variable name="subscribedJobNo" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $serviceProviderMSGID, '@value', $messageReference, '@referenceType', 'JobNumber')"/>
            <xsl:variable name="subscribedForwardingType" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $serviceProviderMSGID, '@value', $messageReference, '@referenceType', 'ForwardingType')" />
            <xsl:variable name="subscribedDocumentName" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $serviceProviderMSGID, '@value', $messageReference, '@referenceType', 'DocumentName')" />
            <xsl:variable name="subscribedWayBillNumber" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $serviceProviderMSGID, '@value', $messageReference, '@referenceType', 'WayBillNumber')" />
            <xsl:variable name="subscribedBookingConfirmationReference" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $serviceProviderMSGID, '@value', $messageReference, '@referenceType', 'BookingConfirmationReference')" />
            <xsl:variable name="subscribedCarrierCode" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $serviceProviderMSGID, '@value', $messageReference, '@referenceType', 'Carrier')" />

            <xsl:variable name="jobType">
              <xsl:choose>
                <xsl:when test="contains($subscribedDocumentName, 'Export')">Export</xsl:when>
                <xsl:otherwise>Import</xsl:otherwise>
              </xsl:choose>
            </xsl:variable>

            <xsl:variable name="responseEvent" select="$notificationMessage/@statut" />
            <xsl:variable name="eventType" select="CodeMapper:GetRecipientCode($serviceProvider, $serviceProvider, concat($serviceProvider, ' System Configuration'), 'Tracing Event Type' , 'Event Type',  $responseEvent, $jobType)"/>

            <xsl:if test="$eventType!='' and $subscribedJobNo!=''">
              <UniversalEvent>
                <Event>
                  <DataContext>
                    <DataTargetCollection>
                      <DataTarget>
                        <Key>
                          <xsl:value-of select="userCSharp:GetJobNo($subscribedJobNo)"/>
                        </Key>
                        <Type>
                          <xsl:value-of select="$subscribedForwardingType"/>
                        </Type>
                      </DataTarget>
                    </DataTargetCollection>
                  </DataContext>
                  <EventTime>
                    <xsl:value-of select="DateMapper:ConvertXmlDateString(string($notificationMessage/@date), 'yyyy-MM-ddTHH:mm:ss')"/>
                  </EventTime>

                  <EventType>
                    <xsl:value-of select="$eventType" />
                  </EventType>
                  <EventReference>
                    <xsl:value-of select="CodeMapper:GetRecipientCode($serviceProvider, $serviceProvider, concat($serviceProvider, ' System Configuration'),  'Tracing Event Type' , 'Event Reference', $responseEvent, $jobType)"/>
                  </EventReference>
                  <IsEstimate>
                    <xsl:value-of select="CodeMapper:GetRecipientCode($serviceProvider, $serviceProvider, concat($serviceProvider, ' System Configuration'),  'Tracing Event Type' , 'Is Estimate', $responseEvent, $jobType)"/>
                  </IsEstimate>

                  <xsl:variable name="eventParameters" select="CodeMapper:GetRecipientCode($serviceProvider, $serviceProvider, concat($serviceProvider, ' System Configuration'),  'Tracing Event Type' , 'Event Parameters', $responseEvent, $jobType)"/>
                  <EventParameters>
                    <Location>
                      <xsl:value-of select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $serviceProviderMSGID, '@value', $messageReference, '@referenceType', 'OperationPort')" />
                    </Location>
                    <xsl:copy-of select="userCSharp:CreateEventParameters($eventParameters)"/>
                  </EventParameters>

                  <ContextCollection>
                    <xsl:call-template name="CreateContext">
                      <xsl:with-param name="type" select="'ContainerNumber'"/>
                      <xsl:with-param name="value" select="$notificationMessage/@ref"/>
                    </xsl:call-template>

                    <xsl:variable name="containerISO" select="$notificationMessage/information[@libelle='CONDITIONNEMENT_MARCHANDISE']/text()" />
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

                    <xsl:call-template name="CreateContext">
                      <xsl:with-param name="type" select="'SealNumber'"/>
                      <xsl:with-param name="value" select="$notificationMessage/information[@libelle='NUM_SCELLE']/text()" />
                    </xsl:call-template>
                    <xsl:call-template name="CreateContext">
                      <xsl:with-param name="type" select="'VesselName'"/>
                      <xsl:with-param name="value" select="$notificationMessage/information[@libelle='NOM_MOYEN_TRANSPORT']/text()" />
                    </xsl:call-template>
                    <xsl:call-template name="CreateContext">
                      <xsl:with-param name="type" select="'PortLocation'"/>
                      <xsl:with-param name="value" select="$notificationMessage/information[@libelle='LIEU_RL']/text()" />
                    </xsl:call-template>
                    <xsl:call-template name="CreateContext">
                      <xsl:with-param name="type" select="'PortArea'"/>
                      <xsl:with-param name="value" select="$notificationMessage/information[@libelle='ZONE_RL']/text()" />
                    </xsl:call-template>
                    <xsl:call-template name="CreateContext">
                      <xsl:with-param name="type" select="'AMQReference'"/>
                      <xsl:with-param name="value" select="$notificationMessage/@sic" />
                    </xsl:call-template>
                    <xsl:call-template name="CreateContext">
                      <xsl:with-param name="type" select="'OTCReference'"/>
                      <xsl:with-param name="value" select="$notificationMessage/information[@libelle='OTC']/text()" />
                    </xsl:call-template>
                    <xsl:call-template name="CreateContext">
                      <xsl:with-param name="type" select="'ATPReference'"/>
                      <xsl:with-param name="value" select="$notificationMessage/information[@libelle='ATP']/text()" />
                    </xsl:call-template>
                    <xsl:call-template name="CreateContext">
                      <xsl:with-param name="type" select="'APPlusVoyageReference'"/>
                      <xsl:with-param name="value" select="$notificationMessage/information[@libelle='ID_MOYEN_TRANSPORT']/text()" />
                    </xsl:call-template>
                    <xsl:call-template name="CreateContext">
                      <xsl:with-param name="type" select="'TransportMode'"/>
                      <xsl:with-param name="value" select="$notificationMessage/information[@libelle='MOYEN_TRANSPORT']/text()" />
                    </xsl:call-template>
                    <xsl:call-template name="CreateContext">
                      <xsl:with-param name="type" select="'Warehouse'"/>
                      <xsl:with-param name="value" select="$notificationMessage/information[@libelle='MANUTENTIONNAIRE']/text()" />
                    </xsl:call-template>
                    <xsl:call-template name="CreateContext">
                      <xsl:with-param name="type" select="'CTOReleaseReference'"/>
                      <xsl:with-param name="value" select="$notificationMessage/@ref-aleatoire-bad" />
                    </xsl:call-template>
                    <xsl:call-template name="CreateContext">
                      <xsl:with-param name="type" select="'EmptyDelivery'"/>
                      <xsl:with-param name="value" select="$notificationMessage/information[@libelle='LIEU_RV']/text()" />
                    </xsl:call-template>
                    <xsl:call-template name="CreateContext">
                      <xsl:with-param name="type" select="'CarrierCode'"/>
                      <xsl:with-param name="value" select="$subscribedCarrierCode" />
                    </xsl:call-template>
                    <xsl:if test="$destinationParty='CONTAINER_TRACKING'">
                      <xsl:call-template name="CreateContext">
                        <xsl:with-param name="type" select="'eHubID'"/>
                        <xsl:with-param name="value" select="$RecipientID" />
                      </xsl:call-template>
                    </xsl:if>
                    <xsl:call-template name="CreateContext">
                      <xsl:with-param name="type" select="'CarriersBookingReference'"/>
                      <xsl:with-param name="value" select="$subscribedBookingConfirmationReference" />
                    </xsl:call-template>
                    <xsl:call-template name="CreateContext">
                      <xsl:with-param name="type" select="'MBOLNumber'"/>
                      <xsl:with-param name="value" select="$subscribedWayBillNumber" />
                    </xsl:call-template>
                  </ContextCollection>
                </Event>
              </UniversalEvent>
            </xsl:if>
          </xsl:if>
        </xsl:for-each>
      </Body>
    </UniversalInterchangeInclude>
  </xsl:template>

  <xsl:template name="CreateContext">
    <xsl:param name="type"/>
    <xsl:param name="value"/>

    <xsl:if test="$value!=''">
      <Context xmlns="http://www.cargowise.com/Schemas/Universal/2012/11">
        <Type>
          <xsl:value-of select="$type"/>
        </Type>
        <Value>
          <xsl:value-of select="$value"/>
        </Value>
      </Context>
    </xsl:if>
  </xsl:template>

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[
public XPathNodeIterator CreateEventParameters(string parameters)
{
  var doc = new XmlDocument();
  var root = doc.CreateElement("root");
  doc.AppendChild(root);

  if (parameters != "")
  {
    foreach (var parameter in parameters.Split('|'))
    {
      var id_value = parameter.Split('=');
      if (id_value.Length == 2)
      {
        var child = doc.CreateElement("", id_value[0], "http://www.cargowise.com/Schemas/Universal/2012/11");
        child.InnerText = id_value[1];
        root.AppendChild(child);
      }
    }
  }

  return doc.CreateNavigator().Select("/*/*");
}

public string GetJobNo(string inputText)
{
  if (!string.IsNullOrEmpty(inputText) && inputText.Contains("_"))
  {
    return inputText.Split('_')[0];
  }
  return inputText;
}
]]>
  </msxsl:script>
</xsl:stylesheet>

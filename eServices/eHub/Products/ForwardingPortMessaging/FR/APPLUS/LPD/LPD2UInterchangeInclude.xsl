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

    <xsl:variable name="messageSetDate" select="MessageSet/@date"/>

    <xsl:choose>
      <xsl:when test="$applusRecipientId != ''">
        <xsl:variable name="SetDestinationParty" select="ContextAccessor:SetContextProperty('DestinationParty','http://schemas.microsoft.com/BizTalk/2003/system-properties', $applusRecipientId)"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="userCSharp:ThrowNoRecipientID()"/>
      </xsl:otherwise>
    </xsl:choose>

    <UniversalInterchangeInclude xmlns="http://www.cargowise.com/Schemas/Universal/2012/11">
      <Body>
        <xsl:for-each select="MessageSet/Messages">
          <xsl:variable name="notificationType" select="Notification/@type" />
          <xsl:variable name="lpdNode" select="Notification/lpd" />

          <xsl:for-each select="Notification/lpd/contenant-amq/amq">
            <xsl:call-template name="CreateUniversalEvent">
              <xsl:with-param name="amq" select="." />
              <xsl:with-param name="dataTargetType" select="'ForwardingConsol'" />
              <xsl:with-param name="equipementAmq" select="equipement-amq" />
              <xsl:with-param name="detailAmq" select="equipement-amq" />
              <xsl:with-param name="notificationType" select="$notificationType" />
              <xsl:with-param name="lpdNode" select="$lpdNode" />
              <xsl:with-param name="eventType" select="'STU'"/>
            </xsl:call-template>
          </xsl:for-each>

          <xsl:variable name="equipmentAmq" select="Notification/lpd/contenant-amq/amq/equipement-amq" />
          <xsl:for-each select="Notification/lpd/contenu-amq/amq">
            <xsl:call-template name="CreateUniversalEvent">
              <xsl:with-param name="amq" select="." />
              <xsl:with-param name="dataTargetType" select="'ForwardingShipment'" />
              <xsl:with-param name="detailAmq" select="reference-amq" />
              <xsl:with-param name="equipementAmq" select="$equipmentAmq" />
              <xsl:with-param name="notificationType" select="$notificationType" />
              <xsl:with-param name="lpdNode" select="$lpdNode" />
              <xsl:with-param name="eventType" select="'STU'"/>
            </xsl:call-template>
            <xsl:call-template name="CreateUniversalEvent">
              <xsl:with-param name="amq" select="." />
              <xsl:with-param name="dataTargetType" select="'ForwardingShipment'" />
              <xsl:with-param name="detailAmq" select="reference-amq" />
              <xsl:with-param name="equipementAmq" select="$equipmentAmq" />
              <xsl:with-param name="notificationType" select="$notificationType" />
              <xsl:with-param name="lpdNode" select="$lpdNode" />
              <xsl:with-param name="eventType" select="'SHL'"/>
              <xsl:with-param name="messageSetDate" select="$messageSetDate"/>
            </xsl:call-template>
          </xsl:for-each>
        </xsl:for-each>
      </Body>
    </UniversalInterchangeInclude>
  </xsl:template>

  <xsl:template name="CreateUniversalEvent">
    <xsl:param name="amq" />
    <xsl:param name="dataTargetType" />
    <xsl:param name="equipementAmq" />
    <xsl:param name="detailAmq" />
    <xsl:param name="notificationType" />
    <xsl:param name="lpdNode" />
    <xsl:param name="eventType"/>
    <xsl:param name="messageSetDate"/>

    <xsl:variable name="ldp_ID" select="$lpdNode/@id" />
    <xsl:variable name="referenceAmq" select="$amq/reference-amq" />
    <xsl:variable name="lmarchandiseAmq" select="$amq/lmarchandise-amq" />
    <xsl:variable name="voyageAmq" select="$amq/voyage-amq" />
    <xsl:variable name="lieuAmq" select="$amq/lieu-amq" />

    <xsl:variable name="messageReference" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $serviceProviderSTID, '@value', concat($notificationType, '_', $ldp_ID))"/>
    <xsl:variable name="subscribedJobNo">
      <xsl:choose>
        <xsl:when test="$dataTargetType='ForwardingConsol'">
          <xsl:value-of select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $serviceProviderMSGID, '@value', $messageReference, '@referenceType', 'JobNumber')"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$lmarchandiseAmq/@ref"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="subscribedDocumentName">
      <xsl:choose>
        <xsl:when test="$dataTargetType='ForwardingConsol'">
          <xsl:value-of select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $serviceProviderMSGID, '@value', $messageReference, '@referenceType', 'DocumentName')" />
        </xsl:when>
        <xsl:otherwise>Provisional Unpacking List (LPD)</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="containerNumber">
      <xsl:choose>
        <xsl:when test="$equipementAmq">
          <xsl:value-of select="$equipementAmq/@id" />
        </xsl:when>
        <xsl:otherwise></xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="containerISO">
      <xsl:choose>
        <xsl:when test="$equipementAmq">
          <xsl:value-of select="$equipementAmq/@code" />
        </xsl:when>
        <xsl:otherwise></xsl:otherwise>
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
                <xsl:value-of select="$subscribedJobNo" />
              </Key>
              <Type>
                <xsl:value-of select="$dataTargetType"/>
              </Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>
          <xsl:choose>
            <xsl:when test="$eventType = 'SHL'">
              <xsl:value-of select="DateMapper:ConvertXmlDateString(string($messageSetDate), 'yyyy-MM-ddTHH:mm:ss')" />
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="DateMapper:ConvertXmlDateString(string($amq/reference-amq/@date), 'yyyy-MM-ddTHH:mm:ss')" />
            </xsl:otherwise>
          </xsl:choose>
        </EventTime>
        <EventType>
          <xsl:value-of select="$eventType"/>
        </EventType>

        <EventParameters>
          <xsl:choose>
            <xsl:when test="$eventType = 'SHL'">
              <Facility>CFS</Facility>
              <Location>
                <xsl:value-of select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $serviceProviderMSGID, '@value', $messageReference, '@referenceType', 'OperationPort')" />
              </Location>
              <MessageType>Port Notification Import Status</MessageType>
              <Reason>clearance pending</Reason>
            </xsl:when>
            <xsl:otherwise>
              <Department>Terminal</Department>
              <Type>LPD Notification</Type>
              <Status>
                <xsl:value-of select="CodeMapper:GetRecipientCode($serviceProvider, $serviceProvider, concat($serviceProvider, ' System Configuration'), 'Acknowledgment Status', 'Output Code', $amq/reference-amq/@statut)"/>
              </Status>
            </xsl:otherwise>
          </xsl:choose>
          <xsl:if test="$containerNumber!=''">
            <EquipmentReferenceNumber>
              <xsl:value-of select="$containerNumber"/>
            </EquipmentReferenceNumber>
          </xsl:if>
          <CustomsReferenceNumber>
            <xsl:value-of select="$detailAmq/@sic" />
          </CustomsReferenceNumber>
        </EventParameters>

        <ContextCollection>
          <xsl:if test="$containerNumber!=''">
            <xsl:choose>
              <xsl:when test="$dataTargetType='ForwardingConsol'">
                <xsl:call-template name="CreateContext">
                  <xsl:with-param name="type" select="'MBOLNumber'" />
                  <xsl:with-param name="value" select="$referenceAmq/@extdoc" />
                </xsl:call-template>
              </xsl:when>
              <xsl:otherwise>
                <xsl:call-template name="CreateContext">
                  <xsl:with-param name="type" select="'ShipmentNumber'" />
                  <xsl:with-param name="value" select="$subscribedJobNo" />
                </xsl:call-template>
                <xsl:call-template name="CreateContext">
                  <xsl:with-param name="type" select="'HBOLNumber'" />
                  <xsl:with-param name="value" select="$referenceAmq/@extdoc" />
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:if>

          <xsl:choose>
            <xsl:when test="$eventType = 'SHL'">
              <xsl:call-template name="CreateContext">
                <xsl:with-param name="type" select="'ImportCargoReference'" />
                <xsl:with-param name="value" select="$detailAmq/@sic" />
              </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <xsl:if test="$containerNumber!=''">
                <xsl:if test="$dataTargetType='ForwardingConsol' or ($eventType='STU' and $dataTargetType!='ForwardingShipment')">
                  <xsl:call-template name="CreateContext">
                    <xsl:with-param name="type" select="'ContainerNumber'" />
                    <xsl:with-param name="value" select="$containerNumber" />
                  </xsl:call-template>
                </xsl:if>

                <xsl:if test="$containerISO!='' and $dataTargetType='ForwardingConsol'">
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
                <xsl:with-param name="type" select="'DOCReference'" />
                <xsl:with-param name="value" select="$referenceAmq/@doc" />
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
                <xsl:with-param name="type" select="'LPDReference'" />
                <xsl:with-param name="value" select="$lpdNode/@id" />
              </xsl:call-template>

              <xsl:call-template name="CreateContext">
                <xsl:with-param name="type" select="'VoyageNumber'" />
                <xsl:with-param name="value" select="$voyageAmq/@refotc" />
              </xsl:call-template>

              <xsl:call-template name="CreateContext">
                <xsl:with-param name="type" select="'VesselName'" />
                <xsl:with-param name="value" select="$voyageAmq/@nav" />
              </xsl:call-template>

              <xsl:variable name="carrierCode">
                <xsl:choose>
                  <xsl:when test="$voyageAmq/@armement!=''">
                    <xsl:value-of select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration', 'SCAC (Inbound)' , 'SCAC', $SenderID, $voyageAmq/@armement)"/>
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
            </xsl:otherwise>
          </xsl:choose>
        </ContextCollection>
      </Event>
    </UniversalEvent>
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

public void ThrowNoRecipientID()
{
  throw new ArgumentException("Unable to resolve recipient Id, message rejected.");
}
]]>
  </msxsl:script>
</xsl:stylesheet>
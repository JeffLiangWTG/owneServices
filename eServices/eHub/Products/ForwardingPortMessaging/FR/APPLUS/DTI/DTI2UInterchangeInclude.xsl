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

  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes"/>

  <xsl:template match="/">
    <xsl:apply-templates select="s0:Interchanges" />
  </xsl:template>

  <xsl:variable name="SenderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')" />
  <xsl:variable name="RecipientID" select="ContextAccessor:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="serviceProviderID" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE', 'FORWARDING_PORT_MESSAGE', 'FPM System Configuration', 'Port Settings', 'ID', $SenderID)"/>
  <xsl:variable name="serviceProviderMSGID" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE', 'FORWARDING_PORT_MESSAGE', 'FPM System Configuration', 'Port Settings', 'MSGID', $SenderID)"/>
  <xsl:variable name="serviceProviderPrefix" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE', 'FORWARDING_PORT_MESSAGE', 'FPM System Configuration', 'Port Settings', 'SubscriptionPrefix', $SenderID)"/>
  <xsl:variable name="serviceProvider" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE', 'FORWARDING_PORT_MESSAGE', 'FPM System Configuration', 'Port Settings', 'Name', $SenderID)"/>
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
          <xsl:call-template name="CreateUniversalEvent">
            <xsl:with-param name="message" select="."/>
          </xsl:call-template>
        </xsl:for-each>
      </Body>
    </UniversalInterchangeInclude>

  </xsl:template>

  <xsl:template name="CreateUniversalEvent">
    <xsl:param name="message"/>

    <xsl:variable name="reference-dec" select="$message/Notification/deplacement/reference-dec" />
    <xsl:variable name="messageReference" select="$reference-dec/@ori" />

    <xsl:variable name="sicSubscribedMessageReferenceWithType" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $serviceProviderSTID, '@value',  concat($message/Notification/@type, '_', $message/Notification/@id))"/>
    <xsl:variable name="sicSubscribedMessageReference">
      <xsl:choose>
        <xsl:when test="$sicSubscribedMessageReferenceWithType!=''">
          <xsl:value-of select="$sicSubscribedMessageReferenceWithType" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $serviceProviderSTID, '@value',  concat('LPD_', $messageReference))"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="subscribedJobNo" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $serviceProviderMSGID, '@value', $sicSubscribedMessageReference, '@referenceType', 'JobNumber')"/>
    <xsl:variable name="subscribedDocumentName" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $serviceProviderMSGID, '@value', $sicSubscribedMessageReference, '@referenceType', 'DocumentName')" />
    <xsl:variable name="lmarchandise-dec" select="$message/Notification/deplacement/lmarchandise-dec" />
    <xsl:variable name="containerNumber">
      <xsl:choose>
        <xsl:when test="$lmarchandise-dec">
          <xsl:value-of select="$lmarchandise-dec/@ref" />
        </xsl:when>
        <xsl:otherwise></xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:if test="$containerNumber != ''">
      <xsl:variable name="containerISOCode">
        <xsl:choose>
          <xsl:when test="$lmarchandise-dec">
            <xsl:value-of select="$lmarchandise-dec/@cond" />
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
                  <xsl:value-of select="$subscribedJobNo"/>
                </Key>
                <Type>ForwardingConsol</Type>
              </DataTarget>
            </DataTargetCollection>
          </DataContext>
          <EventTime>
            <xsl:variable name="referenceDate" select="$reference-dec/@date"/>
            <xsl:value-of select="DateMapper:ConvertXmlDateString($referenceDate, 'yyyy-MM-ddThh:mm:ss')"/>
          </EventTime>
          <EventType>STU</EventType>
          <EventParameters>
            <Department>Terminal</Department>
            <Type>DTI Notification</Type>
            <xsl:if test="$containerNumber!=''">
              <EquipmentReferenceNumber>
                <xsl:value-of select="$containerNumber"/>
              </EquipmentReferenceNumber>
            </xsl:if>
            <ReferenceNumber>
              <xsl:value-of select="$reference-dec/@sic"/>
            </ReferenceNumber>
            <Status>
              <xsl:value-of select="CodeMapper:GetRecipientCode($serviceProvider, $serviceProvider, concat($serviceProvider, ' System Configuration'), 'Acknowledgment Status', 'Output Code', $lmarchandise-dec/@statut)"/>
            </Status>
          </EventParameters>
          <ContextCollection>
            <xsl:call-template name="CreateContext">
              <xsl:with-param name="type" select="'ContainerNumber'"/>
              <xsl:with-param name="value" select="$containerNumber"/>
            </xsl:call-template>
            <xsl:if test="$containerISOCode != ''">
              <xsl:variable name="serviceProviderContainerISO" select="CodeMapper:GetRecipientCode($serviceProvider, $serviceProvider, concat($serviceProvider, ' System Configuration'), 'ContainerTypeToISOCode', concat($serviceProvider, ' Code'), $containerISOCode)"/>
              <xsl:variable name="ContainerTypeMappingCode">
                <xsl:choose>
                  <xsl:when test="$serviceProviderContainerISO != ''">
                    <xsl:value-of select="$serviceProviderContainerISO"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE', 'FORWARDING_PORT_MESSAGE', 'FPM System Configuration', 'ContainerTypeToISOCode', 'Carrier Code', $containerISOCode)"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:variable>
              <xsl:call-template name="CreateContext">
                <xsl:with-param name="type" select="'ContainerISOCode'"/>
                <xsl:with-param name="value" select="$ContainerTypeMappingCode"/>
              </xsl:call-template>
            </xsl:if>
            <xsl:call-template name="CreateContext">
              <xsl:with-param name="type" select="'MBOLNumber'"/>
              <xsl:with-param name="value" select="$lmarchandise-dec/@extdoc"/>
            </xsl:call-template>
            <xsl:call-template name="CreateContext">
              <xsl:with-param name="type" select="'DOCReference'"/>
              <xsl:with-param name="value" select="$lmarchandise-dec/@doc"/>
            </xsl:call-template>
            <xsl:call-template name="CreateContext">
              <xsl:with-param name="type" select="'LPDReference'"/>
              <xsl:with-param name="value" select="$messageReference"/>
            </xsl:call-template>
            <xsl:call-template name="CreateContext">
              <xsl:with-param name="type" select="'ICTReference'"/>
              <xsl:with-param name="value" select="$lmarchandise-dec/@sic"/>
            </xsl:call-template>

            <xsl:variable name="voyage-dec" select="$message/Notification/deplacement/voyage-dec"/>
            <xsl:call-template name="CreateContext">
              <xsl:with-param name="type" select="'VesselName'"/>
              <xsl:with-param name="value" select="$voyage-dec/@nav"/>
            </xsl:call-template>
            <xsl:call-template name="CreateContext">
              <xsl:with-param name="type" select="'ATPReference'"/>
              <xsl:with-param name="value" select="$voyage-dec/@atp"/>
            </xsl:call-template>

            <xsl:variable name="carrierCode">
              <xsl:choose>
                <xsl:when test="$voyage-dec/@reptrans != ''">
                  <xsl:value-of select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE', 'FORWARDING_PORT_MESSAGE', 'FPM System Configuration', 'SCAC (Inbound)', 'SCAC', $SenderID, $voyage-dec/@reptrans)"/>
                </xsl:when>
                <xsl:otherwise></xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <xsl:call-template name="CreateContext">
              <xsl:with-param name="type" select="'CarrierCode'" />
              <xsl:with-param name="value" select="$carrierCode" />
            </xsl:call-template>

            <xsl:variable name="lieu-dec_1" select="$message/Notification/deplacement/lieu-dec[1]" />
            <xsl:call-template name="CreateContext">
              <xsl:with-param name="type" select="'PortAreaFrom'"/>
              <xsl:with-param name="value" select="$lieu-dec_1/@zone"/>
            </xsl:call-template>
            <xsl:call-template name="CreateContext">
              <xsl:with-param name="type" select="'PortLocationFrom'"/>
              <xsl:with-param name="value" select="$lieu-dec_1/@lieu"/>
            </xsl:call-template>
            <xsl:call-template name="CreateContext">
              <xsl:with-param name="type" select="'TerminalCode'"/>
              <xsl:with-param name="value" select="$lieu-dec_1/@manut"/>
            </xsl:call-template>

            <xsl:variable name="lieu-dec_2" select="$message/Notification/deplacement/lieu-dec[2]" />
            <xsl:call-template name="CreateContext">
              <xsl:with-param name="type" select="'PortAreaTo'"/>
              <xsl:with-param name="value" select="$lieu-dec_2/@zone"/>
            </xsl:call-template>
            <xsl:call-template name="CreateContext">
              <xsl:with-param name="type" select="'PortLocationTo'"/>
              <xsl:with-param name="value" select="$lieu-dec_2/@lieu"/>
            </xsl:call-template>
          </ContextCollection>
        </Event>
      </UniversalEvent>
    </xsl:if>
  </xsl:template>

  <xsl:template name="CreateContext">
    <xsl:param name="type"/>
    <xsl:param name="value"/>
    <xsl:if test="$value != ''">
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
public void ThrowNoRecipientID()
{
  throw new ArgumentException("Unable to resolve recipient Id, message rejected.");
}
]]>
  </msxsl:script>
</xsl:stylesheet>

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

  <xsl:variable name="destinationParty" select="ContextAccessor:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="SenderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="serviceProvider" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Port Settings' , 'Name', $SenderID)"/>
  <xsl:variable name="serviceProviderID" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Port Settings' , 'ID', $SenderID)"/>

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
          <xsl:variable name="notificationAction" select="Notification/@action"/>
          <UniversalEvent>
            <Event>
              <EventTime>
                <xsl:variable name="refDate" select="Notification/bon-a-delivrer/reference-bad/@date"/>
                <xsl:variable name="badDate">
                  <xsl:choose>
                    <xsl:when test="$refDate!=''">
                      <xsl:value-of select="$refDate"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="../@date"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:variable>
                <xsl:value-of select="DateMapper:ConvertXmlDateString(string($badDate), 'yyyy-MM-ddTHH:mm:ss')"/>
              </EventTime>
              <EventType>
                <xsl:value-of select="CodeMapper:GetRecipientCode($serviceProvider, $serviceProvider, concat($serviceProvider, ' System Configuration'),  'BAD Event Type' , 'Event Type', $notificationAction)"/>
              </EventType>

              <EventParameters>
                <Department>Terminal</Department>
                <EquipmentReferenceNumber>
                  <xsl:value-of select="Notification/bon-a-delivrer/equipement-bad/@id"/>
                </EquipmentReferenceNumber>
                <Type>Container Release</Type>
                <MessageType>Bon à délivrer Notification</MessageType>
                <ReferenceNumber>
                  <xsl:value-of select="Notification/bon-a-delivrer/equipement-bad/@random"/>
                </ReferenceNumber>
                <Facility>CTO</Facility>
                <Location>
                  <xsl:variable name="badZone" select="Notification/bon-a-delivrer/lieu-bad/@zone" />
                  <xsl:if test="$badZone!=''">
                    <xsl:value-of select="CodeMapper:GetRecipientCode($serviceProvider, $serviceProvider, concat($serviceProvider, ' System Configuration'),  'Zone UNLOCO' , 'Output Code', $badZone)"/>
                  </xsl:if>
                </Location>
                <Status>
                  <xsl:value-of select="CodeMapper:GetRecipientCode($serviceProvider, $serviceProvider, concat($serviceProvider, ' System Configuration'),  'BAD Event Type' , 'Status', $notificationAction)"/>
                </Status>
              </EventParameters>

              <EventReference/>

              <ContextCollection>
                <xsl:variable name="carrierCode" select="Notification/bon-a-delivrer/tiers-bad/@afret"/>
                <xsl:call-template name="CreateContext">
                  <xsl:with-param name="type" select="'CarrierCode'"/>
                  <xsl:with-param name="value" select="CodeMapper:GetRecipientCode($serviceProvider, $serviceProvider, concat($serviceProvider, ' System Configuration'),  'Carrier Code' , 'Output Code', $carrierCode)"/>
                </xsl:call-template>
                <xsl:call-template name="CreateContext">
                  <xsl:with-param name="type" select="'VesselName'"/>
                  <xsl:with-param name="value" select="Notification/bon-a-delivrer/voyage-bad/@nav"/>
                </xsl:call-template>
                <xsl:call-template name="CreateContext">
                  <xsl:with-param name="type" select="'VoyageNumber'"/>
                  <xsl:with-param name="value" select="Notification/bon-a-delivrer/voyage-bad/@refotc"/>
                </xsl:call-template>
                <xsl:call-template name="CreateContext">
                  <xsl:with-param name="type" select="'MBOLNumber'"/>
                  <xsl:with-param name="value" select="Notification/bon-a-delivrer/reference-bad/@extdoc"/>
                </xsl:call-template>
                <xsl:call-template name="CreateContext">
                  <xsl:with-param name="type" select="'ContainerNumber'"/>
                  <xsl:with-param name="value" select="Notification/bon-a-delivrer/equipement-bad/@id"/>
                </xsl:call-template>
                <xsl:call-template name="CreateContext">
                  <xsl:with-param name="type" select="'ContainerISOCode'"/>
                  <xsl:with-param name="value" select="Notification/bon-a-delivrer/equipement-bad/@code"/>
                </xsl:call-template>
                <xsl:call-template name="CreateContext">
                  <xsl:with-param name="type" select="'EventSource'"/>
                  <xsl:with-param name="value" select="'CI5'"/>
                </xsl:call-template>
                <xsl:call-template name="CreateContext">
                  <xsl:with-param name="type" select="'AMQReference'"/>
                  <xsl:with-param name="value" select="Notification/bon-a-delivrer/equipement-bad/@sic"/>
                </xsl:call-template>
              </ContextCollection>
            </Event>
          </UniversalEvent>
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
public void ThrowNoRecipientID()
{
  throw new ArgumentException("Unable to resolve recipient Id, message rejected.");
}
]]>
  </msxsl:script>
</xsl:stylesheet>

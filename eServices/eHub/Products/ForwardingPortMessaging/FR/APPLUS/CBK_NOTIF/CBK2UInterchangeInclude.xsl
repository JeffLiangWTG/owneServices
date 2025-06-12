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
  <xsl:variable name="serviceProviderID" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Port Settings' , 'ID', $SenderID)"/>
  <xsl:variable name="serviceProvider" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Port Settings' , 'Name', $SenderID)"/>

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
          <xsl:variable name="confirmation" select="Notification/confirmation-booking" />
          <xsl:choose>
            <xsl:when test="Notification/confirmation-booking/equipement-cbk">
              <xsl:variable name="clearContainerList" select="userCSharp:ClearContainerLists()" />

              <xsl:for-each select="Notification/confirmation-booking/equipement-cbk">
                <xsl:call-template name="CreateUniversalEvent">
                  <xsl:with-param name="confirmation" select="$confirmation" />
                  <xsl:with-param  name="equipementCbk" select="." />
                </xsl:call-template>
              </xsl:for-each>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="CreateUniversalEvent">
                <xsl:with-param name="confirmation" select="$confirmation" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:for-each>
      </Body>
    </UniversalInterchangeInclude>
  </xsl:template>

  <xsl:template name="CreateUniversalEvent">
    <xsl:param name="confirmation" />
    <xsl:param name="equipementCbk" />

    <xsl:variable name="referenceCbk" select="$confirmation/references-cbk" />
    <xsl:variable name="voyageCbk" select="$confirmation/voyage-cbk" />
    <xsl:variable name="tiersCbk" select="$confirmation/tiers-cbk" />

    <xsl:variable name="containerNumber">
      <xsl:choose>
        <xsl:when test="$equipementCbk">
          <xsl:value-of select="$equipementCbk/@id" />
        </xsl:when>
        <xsl:otherwise></xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:if test="userCSharp:ShouldCreateUniversalEvent($containerNumber)">
      <xsl:variable name="containerISO">
        <xsl:choose>
          <xsl:when test="$equipementCbk">
            <xsl:value-of select="$equipementCbk/@code" />
          </xsl:when>
          <xsl:otherwise></xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <UniversalEvent xmlns="http://www.cargowise.com/Schemas/Universal/2012/11">
        <Event>
          <EventTime>
            <xsl:value-of select="DateMapper:ConvertXmlDateString(string($referenceCbk/@date), 'yyyy-MM-ddTHH:mm:ss')" />
          </EventTime>
          <EventType>STU</EventType>

          <EventParameters>
            <Department>Terminal</Department>
            <Type>Carrier Booking Confirmation Notification</Type>
            <EquipmentReferenceNumber>
              <xsl:value-of select="$containerNumber"/>
            </EquipmentReferenceNumber>
          </EventParameters>

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
              <xsl:with-param name="type" select="'CarriersBookingReference'" />
              <xsl:with-param name="value" select="$referenceCbk/@ext" />
            </xsl:call-template>
            <xsl:call-template name="CreateContext">
              <xsl:with-param name="type" select="'CBKReference'" />
              <xsl:with-param name="value" select="$referenceCbk/@sic" />
            </xsl:call-template>
            <xsl:call-template name="CreateContext">
              <xsl:with-param name="type" select="'VoyageNumber'" />
              <xsl:with-param name="value" select="$voyageCbk/@refotc" />
            </xsl:call-template>
            <xsl:call-template name="CreateContext">
              <xsl:with-param name="type" select="'DOSReference'" />
              <xsl:with-param name="value" select="$referenceCbk/@trsic" />
            </xsl:call-template>
            <xsl:call-template name="CreateContext">
              <xsl:with-param name="type" select="'VesselName'" />
              <xsl:with-param name="value" select="$voyageCbk/@nav" />
            </xsl:call-template>
            <xsl:call-template name="CreateContext">
              <xsl:with-param name="type" select="'OTCReference'" />
              <xsl:with-param name="value" select="$voyageCbk/@otc" />
            </xsl:call-template>
            <xsl:call-template name="CreateContext">
              <xsl:with-param name="type" select="'ATPReference'" />
              <xsl:with-param name="value" select="$voyageCbk/@atp" />
            </xsl:call-template>

            <xsl:variable name="carrierCode">
              <xsl:choose>
                <xsl:when test="$tiersCbk/@creat!=''">
                  <xsl:value-of select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration', 'SCAC (Inbound)' , 'SCAC', $SenderID, $tiersCbk/@creat)"/>
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
              <xsl:with-param name="value" select="$tiersCbk/@zone" />
            </xsl:call-template>
            <xsl:call-template name="CreateContext">
              <xsl:with-param name="type" select="'PortLocation'" />
              <xsl:with-param name="value" select="$tiersCbk/@lieu" />
            </xsl:call-template>
            <xsl:call-template name="CreateContext">
              <xsl:with-param name="type" select="'TerminalCode'" />
              <xsl:with-param name="value" select="$tiersCbk/@manut" />
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

public void ThrowNoRecipientID()
{
  throw new ArgumentException("Unable to resolve recipient Id, message rejected.");
}
]]>
  </msxsl:script>
</xsl:stylesheet>
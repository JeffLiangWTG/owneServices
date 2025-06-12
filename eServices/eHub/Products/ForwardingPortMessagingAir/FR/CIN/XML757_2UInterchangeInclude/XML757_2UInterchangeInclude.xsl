<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 CodeMapper ContextAccessor DataModelAccessor DateMapper userCSharp" version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/FPMA/CIN/757"
                xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2012/11"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
                xmlns:DataModelAccessor="http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"
                xmlns:DateMapper="http://schemas.microsoft.com/BizTalk/2003/DateMapper"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">

  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes" />

  <xsl:template match="/">
    <xsl:apply-templates select="s0:Xml757" />
  </xsl:template>

  <xsl:variable name="senderId" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')" />

  <xsl:variable name="mrn" select="s0:Xml757/Shipments/Shipment/MRN/text()"/>
  <xsl:variable name="lastStatus" select="s0:Xml757/Shipments/Shipment/Status/Last/text()"/>

  <xsl:variable name="MsgID" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE_AIR' , 'FORWARDING_PORT_MESSAGE_AIR' , 'FPMA System Configuration' , 'Port Settings' , 'MSGID', $senderId)"/>

  <xsl:variable name="subscribedDocumentIdentifier" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $senderId, '@recipientId', '', '@ST_ID', $MsgID, '@value', $mrn, '@referenceType', 'MRN')" />
  <xsl:variable name="subscribedDocumentName">
    <xsl:choose>
      <xsl:when test="$subscribedDocumentIdentifier!=''">
        <xsl:value-of select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $senderId, '@recipientId', '', '@ST_ID', $MsgID, '@value', $subscribedDocumentIdentifier, '@referenceType', 'DocumentName')" />
      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="subscribedJobNumber">
    <xsl:choose>
      <xsl:when test="$subscribedDocumentIdentifier!=''">
        <xsl:value-of select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $senderId, '@recipientId', '', '@ST_ID', $MsgID, '@value', $subscribedDocumentIdentifier, '@referenceType', 'JobNumber')" />
      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="subscribedForwardingType">
    <xsl:choose>
      <xsl:when test="$subscribedDocumentIdentifier!=''">
        <xsl:value-of select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $senderId, '@recipientId', '', '@ST_ID', $MsgID, '@value', $subscribedDocumentIdentifier, '@referenceType', 'ForwardingType')" />
      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="subscribedOperationPort">
    <xsl:choose>
      <xsl:when test="$subscribedDocumentIdentifier!=''">
        <xsl:value-of select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $senderId, '@recipientId', '', '@ST_ID', $MsgID, '@value', $subscribedDocumentIdentifier, '@referenceType', 'OperationPort')" />
      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:template match="s0:Xml757">
    <xsl:variable name="recipientID" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedClients', '', '@senderId', $senderId, '@ST_ID', $MsgID, '@value', $subscribedDocumentIdentifier)"/>
    <xsl:if test="$recipientID=''">
      <xsl:value-of select="userCSharp:ThrowException(concat('RecipientID could not be found for MRN ', $mrn, '.'))" />
    </xsl:if>
    <xsl:variable name="SetDestinationParty" select="ContextAccessor:SetContextProperty('DestinationParty','http://schemas.microsoft.com/BizTalk/2003/system-properties', $recipientID)"/>

    <xsl:variable name="serviceProvider" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE_AIR' , 'FORWARDING_PORT_MESSAGE_AIR' , 'FPMA System Configuration' , 'Port Settings' , 'Name', $senderId)"/>
    <xsl:variable name="counterNumber" select="CodeMapper:CallActionProcedureHelper('GetCounterValue', '@value', '@name', concat('CargoWise.eHub.Products.ForwardingPortMessagingAir.FR.', $serviceProvider, '.757'), '@maxlength', '14')" />
    <xsl:variable name="eventType" select="CodeMapper:GetRecipientCode('CIN', 'CIN', 'CIN Provider Configuration', 'Event Type', 'EventType', $lastStatus)"/>
    <xsl:variable name="subscribeStatusUpdate" select="DataModelAccessor:InsertSubscriptionValue('CINTRC', $recipientID, $serviceProvider, concat($subscribedDocumentIdentifier, '_', $mrn, '_', $counterNumber), $lastStatus, 'StatusUpdate')" />

    <ns0:UniversalInterchangeInclude>
      <ns0:Body>
        <xsl:if test="$subscribedJobNumber != '' and $eventType != ''">
          <ns0:UniversalEvent>
            <ns0:Event>
              <ns0:DataContext>
                <ns0:DocumentaryOverride>
                  <ns0:DocumentName>
                    <xsl:value-of select="$subscribedDocumentName"/>
                  </ns0:DocumentName>
                </ns0:DocumentaryOverride>
                <ns0:DataTargetCollection>
                  <ns0:DataTarget>
                    <ns0:Key>
                      <xsl:value-of select="$subscribedJobNumber"/>
                    </ns0:Key>
                    <ns0:Type>
                      <xsl:value-of select="$subscribedForwardingType"/>
                    </ns0:Type>
                  </ns0:DataTarget>
                </ns0:DataTargetCollection>
              </ns0:DataContext>
              <ns0:EventTime>
                <xsl:value-of select="DateMapper:ConvertToDateTimeString(Customs/DateTime/text(), 'yyyyMMddHHmm', 'yyyy-MM-ddTHH:mm:ss')"/>
              </ns0:EventTime>
              <ns0:EventType>
                <xsl:value-of select="$eventType" />
              </ns0:EventType>
              <ns0:EventParameters>
                <xsl:variable name="eventParameters" select="CodeMapper:GetRecipientCode('CIN', 'CIN', 'CIN Provider Configuration', 'Event Type', 'EventParameters', $lastStatus)"/>
                <xsl:copy-of select="userCSharp:CreateEventParameters($eventParameters)"/>

                <xsl:if test="userCSharp:ShouldCreateParameter('Department')">
                  <ns0:Department>Terminal</ns0:Department>
                </xsl:if>
                <xsl:if test="userCSharp:ShouldCreateParameter('MessageType') and $subscribedDocumentName != ''">
                  <ns0:MessageType>
                    <xsl:value-of select="$subscribedDocumentName"/>
                  </ns0:MessageType>
                </xsl:if>
                <xsl:if test="userCSharp:ShouldCreateParameter('Location') and $subscribedOperationPort != ''">
                  <ns0:Location>
                    <xsl:value-of select="$subscribedOperationPort"/>
                  </ns0:Location>
                </xsl:if>
                <xsl:variable name="reason" select="Shipments/Shipment/Status/Description/text()"/>
                <xsl:if test="userCSharp:ShouldCreateParameter('Reason') and $reason != ''">
                  <ns0:Reason>
                    <xsl:value-of select="$reason"/>
                  </ns0:Reason>
                </xsl:if>
                <xsl:if test="userCSharp:ShouldCreateParameter('CustomsReferenceNumber') and $mrn != ''">
                  <ns0:CustomsReferenceNumber>
                    <xsl:value-of select="$mrn"/>
                  </ns0:CustomsReferenceNumber>
                </xsl:if>
              </ns0:EventParameters>
              <ns0:EventReference/>
              <ns0:ContextCollection>
                <xsl:call-template name="CreateContext">
                  <xsl:with-param name="type" select="'AWBNumber'"/>
                  <xsl:with-param name="value" select="Customs/AWBNumber/text()"/>
                </xsl:call-template>
                <xsl:call-template name="CreateContext">
                  <xsl:with-param name="type" select="'CarrierCode'"/>
                  <xsl:with-param name="value" select="Customs/CarrierCode/text()"/>
                </xsl:call-template>
                <xsl:call-template name="CreateContext">
                  <xsl:with-param name="type" select="'FlightDeparture'"/>
                  <xsl:with-param name="value" select="Customs/FlightDeparture/text()"/>
                </xsl:call-template>
                <xsl:call-template name="CreateContext">
                  <xsl:with-param name="type" select="'FlightDestination'"/>
                  <xsl:with-param name="value" select="Customs/FlightDestination/text()"/>
                </xsl:call-template>
                <xsl:call-template name="CreateContext">
                  <xsl:with-param name="type" select="'FlightNumber'"/>
                  <xsl:with-param name="value" select="Customs/FlightNumber/text()"/>
                </xsl:call-template>
                <xsl:call-template name="CreateContext">
                  <xsl:with-param name="type" select="'FlightDate'"/>
                  <xsl:with-param name="value" select="Customs/FlightDate/text()"/>
                </xsl:call-template>
                <xsl:call-template name="CreateContext">
                  <xsl:with-param name="type" select="'MRN'"/>
                  <xsl:with-param name="value" select="$mrn"/>
                </xsl:call-template>
                <xsl:call-template name="CreateContext">
                  <xsl:with-param name="type" select="'Status'"/>
                  <xsl:with-param name="value" select="$lastStatus"/>
                </xsl:call-template>
              </ns0:ContextCollection>
            </ns0:Event>
          </ns0:UniversalEvent>
        </xsl:if>
      </ns0:Body>
    </ns0:UniversalInterchangeInclude>
  </xsl:template>

  <xsl:template name="CreateContext">
    <xsl:param name="type" />
    <xsl:param name="value" />

    <xsl:if test="$value!=''">
      <ns0:Context>
        <ns0:Type>
          <xsl:value-of select="$type" />
        </ns0:Type>
        <ns0:Value>
          <xsl:value-of select="$value" />
        </ns0:Value>
      </ns0:Context>
    </xsl:if>
  </xsl:template>

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[
public void ThrowException(string reason) {
  throw new Exception(reason);
}

System.Collections.Generic.List<string> parameterWithLogic = new System.Collections.Generic.List<string>();

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
        if (id_value[1] == ".")
        {
          parameterWithLogic.Add(parameter);
        }
        
        if (id_value[1] != ".")
        {
          var child = doc.CreateElement("ns0", id_value[0], "http://www.cargowise.com/Schemas/Universal/2012/11");
          child.InnerText = id_value[1];
          root.AppendChild(child);
        }
      }
    }
  }

  return doc.CreateNavigator().Select("/*/*");
}

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
]]>
  </msxsl:script>
</xsl:stylesheet>
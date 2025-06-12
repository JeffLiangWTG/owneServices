<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 CodeMapper ContextAccessor DataModelAccessor DateMapper userCSharp" version="1.0"
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

  <xsl:variable name="RecipientID" select="ContextAccessor:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="SenderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="serviceProvider" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Port Settings' , 'Name', $SenderID)"/>
  <xsl:variable name="serviceProviderMSGID" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Port Settings' , 'MSGID', $SenderID)"/>
  <xsl:variable name="serviceProviderPrefix" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Port Settings' , 'SubscriptionPrefix', $SenderID)"/>

  <xsl:template match="s0:Interchanges">
    <ns0:UniversalInterchangeInclude>
      <ns0:Body>
        <xsl:for-each select="MessageSet/Messages">
          <xsl:variable name="messageReference" select="Response/@id"/>
          <xsl:variable name="subscribedJobNo" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $serviceProviderMSGID, '@value', $messageReference, '@referenceType', 'JobNumber')"/>

          <xsl:variable name="responseStatusCode" select="Response/@statut" />
          <xsl:variable name="eventType">
            <xsl:variable name="subscribedActionPurpose" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $serviceProviderMSGID, '@value', $messageReference, '@referenceType', 'Purpose')" />
            <xsl:value-of select="CodeMapper:GetRecipientCode($serviceProvider, $serviceProvider, concat($serviceProvider, ' System Configuration'), 'ACQ Event Type', 'Event Type', $responseStatusCode, $subscribedActionPurpose)"/>
          </xsl:variable>

          <xsl:if test="$eventType!='' and $subscribedJobNo!=''">
            <xsl:call-template name="createUniversalEvent">
              <xsl:with-param name="messageReference" select="$messageReference"/>
              <xsl:with-param name="subscribedJobNo" select="$subscribedJobNo"/>
              <xsl:with-param name="eventType" select="$eventType"/>
              <xsl:with-param name="responseStatusCode" select="$responseStatusCode"/>
            </xsl:call-template>
            <xsl:if test="$eventType = 'MAA' and contains('CRESA', Response/@type)">
              <xsl:call-template name="createUniversalEvent">
                <xsl:with-param name="messageReference" select="$messageReference"/>
                <xsl:with-param name="subscribedJobNo" select="$subscribedJobNo"/>
                <xsl:with-param name="eventType" select="'SHL'"/>
                <xsl:with-param name="responseStatusCode" select="$responseStatusCode"/>
              </xsl:call-template>
            </xsl:if>
          </xsl:if>
        </xsl:for-each>
      </ns0:Body>
    </ns0:UniversalInterchangeInclude>
  </xsl:template>

  <xsl:template name="createUniversalEvent">
    <xsl:param name="messageReference"/>
    <xsl:param name="subscribedJobNo"/>
    <xsl:param name="eventType"/>
    <xsl:param name="responseStatusCode"/>

    <xsl:variable name="messageType" select="Response/@type"/>
    <xsl:variable name="sicNumber" select="Response/reference/@sic"/>
    <xsl:variable name="refNumber" select="Response/reference/@ref"/>
    <xsl:variable name="reason" select="Response/reference/erreur/libelle/text()"/>

    <xsl:variable name="subscribedForwardingType" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $serviceProviderMSGID, '@value', $messageReference, '@referenceType', 'ForwardingType')" />
    <xsl:variable name="subscribedDocumentName" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $serviceProviderMSGID, '@value', $messageReference, '@referenceType', 'DocumentName')" />
    <xsl:variable name="subscribedContainerNumber" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $serviceProviderMSGID, '@value', $messageReference, '@referenceType', 'ContainerNumber')" />

    <xsl:if test="$RecipientID != '' and $subscribedJobNo != ''" >
      <xsl:choose>
        <xsl:when test="contains('AMQ CRESA', Response/@type) and $refNumber != ''">
          <xsl:variable name="subscribeSIC" select="DataModelAccessor:InsertSubscriptionValue(concat($serviceProviderPrefix, 'SIC'), $serviceProvider, $RecipientID, concat(Response/@type, '_', $refNumber), $messageReference)" />
        </xsl:when>
        <xsl:when test="not(contains('AMQ CRESA', Response/@type)) and $sicNumber != ''">
          <xsl:variable name="subscribeSIC" select="DataModelAccessor:InsertSubscriptionValue(concat($serviceProviderPrefix, 'SIC'), $serviceProvider, $RecipientID, concat(Response/@type, '_', $sicNumber), $messageReference)" />
        </xsl:when>
      </xsl:choose>
    </xsl:if>

    <xsl:if test="$eventType!='' and $subscribedJobNo!=''">
      <ns0:UniversalEvent xmlns="http://www.cargowise.com/Schemas/Universal/2012/11">
        <ns0:Event>
          <ns0:DataContext>
            <xsl:if test="$subscribedDocumentName!=''">
              <ns0:DocumentaryOverride>
                <ns0:DocumentName>
                  <xsl:value-of select="$subscribedDocumentName"/>
                </ns0:DocumentName>
              </ns0:DocumentaryOverride>
            </xsl:if>
            <ns0:DataTargetCollection>
              <ns0:DataTarget>
                <ns0:Key>
                  <xsl:value-of select="userCSharp:GetJobNo($subscribedJobNo)"/>
                </ns0:Key>
                <ns0:Type>
                  <xsl:value-of select="$subscribedForwardingType"/>
                </ns0:Type>
              </ns0:DataTarget>
            </ns0:DataTargetCollection>
          </ns0:DataContext>
          <ns0:EventTime>
            <xsl:value-of select="DateMapper:ConvertXmlDateString(Response/reference/@date, 'yyyy-MM-ddTHH:mm:ss')"/>
          </ns0:EventTime>
          <ns0:EventType>
            <xsl:value-of select="$eventType"/>
          </ns0:EventType>
          <ns0:EventParameters>
            <xsl:choose>
              <xsl:when test="$eventType = 'SHL'">
                <ns0:Facility>CFS</ns0:Facility>
                <ns0:MessageType>Port Notification Export Status</ns0:MessageType>
                <ns0:Reason>clearance pending</ns0:Reason>
              </xsl:when>
              <xsl:otherwise>
                <ns0:Department>Terminal</ns0:Department>
                <xsl:if test="$subscribedContainerNumber!=''">
                  <ns0:EquipmentReferenceNumber>
                    <xsl:value-of select="$subscribedContainerNumber"/>
                  </ns0:EquipmentReferenceNumber>
                </xsl:if>
                <ns0:MessageType>
                  <xsl:value-of select="$subscribedDocumentName"/>
                </ns0:MessageType>
                <xsl:if test="$responseStatusCode='V' and $sicNumber!=''">
                  <ns0:ReferenceNumber>
                    <xsl:value-of select="$sicNumber"/>
                  </ns0:ReferenceNumber>
                </xsl:if>
                <xsl:if test="$eventType = 'MRJ' and $reason!=''">
                  <ns0:Reason>
                    <xsl:value-of select="$reason"/>
                  </ns0:Reason>
                </xsl:if>
                <xsl:variable name="responseStatus" select="CodeMapper:GetRecipientCode($serviceProvider, $serviceProvider, concat($serviceProvider, ' System Configuration'), 'Acknowledgment Status' , 'Output Code',  Response/reference/@statut)"/>
                <xsl:if test="$responseStatus!=''">
                  <ns0:Status>
                    <xsl:value-of select="$responseStatus"/>
                  </ns0:Status>
                </xsl:if>
              </xsl:otherwise>
            </xsl:choose>
            <xsl:if test="$responseStatusCode='V' and $refNumber!='' and contains('CRESA AMQ', $messageType)">
              <ns0:CustomsReferenceNumber>
                <xsl:value-of select="$refNumber"/>
              </ns0:CustomsReferenceNumber>
            </xsl:if>
            <ns0:Location>
              <xsl:value-of select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $serviceProviderMSGID, '@value', $messageReference, '@referenceType', 'OperationPort')"/>
            </ns0:Location>
          </ns0:EventParameters>
          <ns0:EventReference/>
          <ns0:ContextCollection>
            <xsl:choose>
              <xsl:when test="$eventType = 'SHL'">
                <xsl:call-template name="CreateContext">
                  <xsl:with-param name="type" select="'ShipmentNumber'" />
                  <xsl:with-param name="value" select="$subscribedJobNo" />
                </xsl:call-template>
                <xsl:call-template name="CreateContext">
                  <xsl:with-param name="type" select="'ExportCargoReference'" />
                  <xsl:with-param name="value" select="$refNumber" />
                </xsl:call-template>
              </xsl:when>
              <xsl:otherwise>
                <xsl:call-template name="CreateContext">
                  <xsl:with-param name="type" select="'Message Reference'" />
                  <xsl:with-param name="value" select="$messageReference" />
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>
          </ns0:ContextCollection>
        </ns0:Event>
      </ns0:UniversalEvent>
    </xsl:if>
  </xsl:template>

  <xsl:template name="CreateContext">
    <xsl:param name="type" />
    <xsl:param name="value" />

    <xsl:if test="$value!=''">
      <ns0:Context xmlns="http://www.cargowise.com/Schemas/Universal/2012/11">
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
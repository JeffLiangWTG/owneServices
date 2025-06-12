<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
        xmlns:msxsl="urn:schemas-microsoft-com:xslt"
        exclude-result-prefixes="msxsl ns0 DateMapper ContextAccessor CodeMapper userCSharp"
        version="1.0"
        xmlns:ns0="http://cargowise.com/ehub/core/2018/06"
        xmlns:ns1="http://www.cargowise.com/Schemas/Universal/2012/11"
        xmlns:DateMapper="http://schemas.microsoft.com/BizTalk/2003/DateMapper"
        xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
        xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
        xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes" />

  <xsl:template match="/">
    <xsl:apply-templates select="/ns0:DeliveryNotificationMessage" />
  </xsl:template>
  <xsl:template match="ns0:DeliveryNotificationMessage">
    <xsl:apply-templates select="//*[local-name()='UniversalShipment']" />
  </xsl:template>

  <xsl:template match="//*[local-name()='UniversalShipment']">
    <xsl:variable name="consolNumber" select="//*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='DataContext']//*[local-name()='DataSource'][1]/*[local-name()='Key']/text()" />
    <xsl:variable name="dataType" select="//*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='DataContext']//*[local-name()='DataSource'][1]/*[local-name()='Type']/text()" />

    <xsl:variable name="documentName" select="//*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='DataContext']/*[local-name()='DocumentaryOverride']/*[local-name()='DocumentName']/text()" />
    <xsl:variable name="soNumber" select="//*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='SubShipmentCollection']/*[local-name()='SubShipment']/*[local-name()='DataContext']//*[local-name()='DataSource'][1]/*[local-name()='Key']/text()" />

    <xsl:variable name="containerNumber" select="//*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='ContainerCollection']/*[local-name()='Container']/*[local-name()='ContainerNumber']/text()" />
    <xsl:variable name="inboxPK" select="/*[local-name()='DeliveryNotificationMessage']/*[local-name()='InternalTrackingID']/text()" />

    <xsl:variable name="destinationParty" select="ContextAccessor:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')" />
    <xsl:variable name="outboxRecipientId" select="/*[local-name()='DeliveryNotificationMessage']/*[local-name()='OutboxRecipientId']/text()" />
    <xsl:variable name="senderId">
      <xsl:choose>
        <xsl:when test="substring-after($outboxRecipientId, '_') != ''">
          <xsl:value-of select="substring-before($outboxRecipientId, '_')" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$outboxRecipientId" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="eventType">
      <xsl:choose>
        <xsl:when test="/*[local-name()='DeliveryNotificationMessage']/*[local-name()='NotificationType']/text() != 'ACK'">IRJ</xsl:when>
        <xsl:otherwise>ISN</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="workflowData" select="//*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='DataContext']//*[local-name()='Workflow'][1]/*[local-name()='Company' or local-name()='EventBranch']"/>
    <xsl:variable name="eventBranch" select="//*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='DataContext']/*[local-name()='Workflow']/*[local-name()='EventBranch']/text()"/>
    <xsl:variable name="companyCode" select="//*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='DataContext']/*[local-name()='Workflow']/*[local-name()='Company']/*[local-name()='Code']/text()"/>
    <xsl:variable name="operationalPortCode" select="//*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][*[local-name()='Key']='OperationalPort_Code']/*[local-name()='Value']/text()"/>
    <xsl:variable name="purpose" select="//*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='DataContext']/*[local-name()='DocumentaryOverride']/*[local-name()='Purpose']/text()"/>

    <xsl:variable name="namespace" select="namespace-uri()"/>
    <xsl:variable name="generateContainerNumber">
      <xsl:choose>
        <xsl:when test="$namespace= 'http://www.cargowise.com/Schemas/Universal/2012/11/ContainerLoadPlan/1'">
          <xsl:value-of select="'true'" />
        </xsl:when>
        <xsl:when test="$namespace= 'http://www.cargowise.com/Schemas/Universal/2012/11/TRC/1'">
          <xsl:value-of select="'true'" />
        </xsl:when>
        <xsl:when test="$namespace= 'http://www.cargowise.com/Schemas/Universal/2012/11/AMQ/1'">
          <xsl:value-of select="'true'" />
        </xsl:when>
        <xsl:when test="$namespace= 'http://www.cargowise.com/Schemas/Universal/2012/11/LPD/1'">
          <xsl:value-of select="'true'" />
        </xsl:when>
        <xsl:when test="$namespace= 'http://www.cargowise.com/Schemas/Universal/2012/11/LDE/1'">
          <xsl:value-of select="'true'" />
        </xsl:when>
        <xsl:when test="$namespace= 'http://www.cargowise.com/Schemas/Universal/2012/11/CDM/1'">
          <xsl:value-of select="'true'" />
        </xsl:when>
        <xsl:when test="$namespace= 'http://www.cargowise.com/Schemas/Universal/2012/11/NotificationOfCertifiedPickup/1'">
          <xsl:value-of select="'true'" />
        </xsl:when>
        <xsl:when test="$namespace= 'http://www.cargowise.com/Schemas/Universal/2012/11/ExportPreAdviseNotification/1'">
          <xsl:value-of select="'true'" />
        </xsl:when>
        <xsl:when test="$eventType='IRJ' and userCSharp:ShouldGenerateContainerNo($documentName)">
          <xsl:value-of select="'true'" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="'false'" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="messageReferenceSubscriptionTypeID">
      <xsl:choose>
        <xsl:when test="$senderId!=''">
          <xsl:value-of select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Subscription Type ID' , 'Message Reference ST ID' , $senderId)" />
        </xsl:when>
        <xsl:otherwise></xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="messageRefereneceValue">
      <xsl:choose>
        <xsl:when test="$messageReferenceSubscriptionTypeID = 'DIRECT'">
          <xsl:value-of select="$consolNumber" />
        </xsl:when>
        <xsl:when test="$documentName='eManifest' and $soNumber!=''">
          <xsl:value-of select="concat($consolNumber,'_', $soNumber)" />
        </xsl:when>
        <xsl:when test="$generateContainerNumber = 'true' and $containerNumber!=''">
          <xsl:value-of select="concat($consolNumber,'_', $containerNumber)" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$consolNumber" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="InterchangeNumberAndMessageReference">
      <xsl:choose>
        <xsl:when test="$senderId!=''">
          <xsl:value-of select="userCSharp:GetInterchangeNumberAndMessageReference(CodeMapper:CallActionProcedureHelper('SelectSubscribedReference','@reference', '@senderId', $senderId,'@recipientId', $destinationParty ,'@ST_ID', $messageReferenceSubscriptionTypeID,'@value', $inboxPK))" />
        </xsl:when>
        <xsl:otherwise></xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="MessageReferenceSubscription" select="userCSharp:GetMessageReference()" />
    <xsl:variable name="MessageReference">
      <xsl:choose>
        <xsl:when test="$MessageReferenceSubscription != ''">
          <xsl:value-of select="$MessageReferenceSubscription" />
        </xsl:when>
        <xsl:when test="$senderId != ''">
          <xsl:value-of select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference','@reference', '@senderId', $senderId,'@recipientId', $destinationParty ,'@ST_ID', $messageReferenceSubscriptionTypeID,'@value', $messageRefereneceValue)" />
        </xsl:when>
        <xsl:otherwise>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="InterchangeNumber" select="userCSharp:GetInterchangeNumber()" />

    <ns1:UniversalInterchangeInclude>
      <ns1:Body>
        <ns1:UniversalEvent>
          <ns1:Event>
            <ns1:DataContext>
              <xsl:if test="$documentName!=''">
                <ns1:DocumentaryOverride>
                  <ns1:DocumentName>
                    <xsl:value-of select="$documentName" />
                  </ns1:DocumentName>
                </ns1:DocumentaryOverride>
              </xsl:if>
              <ns1:DataTargetCollection>
                <ns1:DataTarget>
                  <ns1:Key>
                    <xsl:value-of select="$consolNumber" />
                  </ns1:Key>
                  <ns1:Type>
                    <xsl:value-of select="$dataType" />
                  </ns1:Type>
                </ns1:DataTarget>
              </ns1:DataTargetCollection>
              <xsl:if test="$workflowData">
                <ns1:Workflow>
                  <ns1:CodesMappedToTarget>true</ns1:CodesMappedToTarget>
                  <xsl:call-template name="copyTagsAndAttributes">
                    <xsl:with-param name="tags" select="$workflowData"/>
                  </xsl:call-template>
                </ns1:Workflow>
              </xsl:if>
            </ns1:DataContext>
            <ns1:EventTime>
              <xsl:value-of select="DateMapper:CurrentDateTime('s')" />
            </ns1:EventTime>
            <ns1:EventType>
              <xsl:value-of select="$eventType"/>
            </ns1:EventType>
            <ns1:EventParameters>
              <xsl:variable name="errorDescriptionFullText" select="/*[local-name()='DeliveryNotificationMessage']/*[local-name()='ErrorDescription']/text()" />
              <xsl:variable name="tryParseErrorDescription" select="userCSharp:ParseErrorDescription($errorDescriptionFullText)" />
              <xsl:variable name="errorDescription">
                <xsl:choose>
                  <xsl:when test="$tryParseErrorDescription != ''">
                    <xsl:value-of select="$tryParseErrorDescription"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="$errorDescriptionFullText"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:variable>
              <xsl:variable name="pipeCount" select="string-length($errorDescription) - string-length(translate($errorDescription, '|', ''))" />
              <xsl:variable name="equalCount" select="string-length($errorDescription) - string-length(translate($errorDescription, '=', ''))" />
              <xsl:variable name="notificationType" select="/*[local-name()='DeliveryNotificationMessage']/*[local-name()='NotificationType']/text()"/>
              <xsl:choose>
                <xsl:when test="$errorDescription != '' and $equalCount = $pipeCount + 1 and $notificationType!= 'ACK'">
                  <xsl:call-template name="AddReferenceParameters">
                    <xsl:with-param name="text" select="$errorDescription"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:when test="$errorDescription != '' and $notificationType!= 'ACK'">
                  <ns1:Department>CargoWise</ns1:Department>
                  <ns1:Reason>
                    <xsl:value-of select="$errorDescription" />
                  </ns1:Reason>
                </xsl:when>
                <xsl:otherwise>
                  <ns1:Department>CargoWise</ns1:Department>
                </xsl:otherwise>
              </xsl:choose>
              <xsl:if test="$documentName!=''">
                <ns1:MessageType>
                  <xsl:value-of select="$documentName" />
                </ns1:MessageType>
              </xsl:if>
              <xsl:if test="$documentName='eManifest' and $soNumber!=''">
                <ns1:ReferenceNumber>
                  <xsl:value-of select="$soNumber" />
                </ns1:ReferenceNumber>
              </xsl:if>
              <xsl:if test="$generateContainerNumber='true'" >
                <ns1:EquipmentReferenceNumber>
                  <xsl:value-of select="$containerNumber" />
                </ns1:EquipmentReferenceNumber>
              </xsl:if>
              <xsl:if test="$operationalPortCode != ''">
                <ns1:Location>
                  <xsl:value-of select="$operationalPortCode"/>
                </ns1:Location>
              </xsl:if>
              <xsl:if test="$purpose != ''">
                <ns1:Status>
                  <xsl:value-of select="$purpose"/>
                </ns1:Status>
              </xsl:if>
            </ns1:EventParameters>
            <ns1:ContextCollection>
              <xsl:if test="$documentName='eManifest'">
                <xsl:call-template name="CreateContext">
                  <xsl:with-param name="type" select="'SO Number'"/>
                  <xsl:with-param name="value" select="$soNumber"/>
                </xsl:call-template>
              </xsl:if>
              <xsl:if test="$generateContainerNumber='true'">
                <xsl:call-template name="CreateContext">
                  <xsl:with-param name="type" select="'ContainerNumber'"/>
                  <xsl:with-param name="value" select="$containerNumber"/>
                </xsl:call-template>
              </xsl:if>
              <xsl:call-template name="CreateContext">
                <xsl:with-param name="type" select="'MessageReference'"/>
                <xsl:with-param name="value" select="$MessageReference"/>
                <xsl:with-param name="fallback" select="$consolNumber"/>
              </xsl:call-template>
              <xsl:call-template name="CreateContext">
                <xsl:with-param name="type" select="'Interchange Number'"/>
                <xsl:with-param name="value" select="$InterchangeNumber"/>
              </xsl:call-template>
              <xsl:call-template name="CreateContext">
                <xsl:with-param name="type" select="'EventBranch'"/>
                <xsl:with-param name="value" select="$eventBranch"/>
              </xsl:call-template>
              <xsl:call-template name="CreateContext">
                <xsl:with-param name="type" select="'Company'"/>
                <xsl:with-param name="value" select="$companyCode"/>
              </xsl:call-template>
              <xsl:call-template name="CreateContext">
                <xsl:with-param name="type" select="'OperationalPortCode'"/>
                <xsl:with-param name="value" select="$operationalPortCode"/>
              </xsl:call-template>
              <xsl:call-template name="CreateContext">
                <xsl:with-param name="type" select="'Purpose'"/>
                <xsl:with-param name="value" select="$purpose"/>
              </xsl:call-template>
            </ns1:ContextCollection>
          </ns1:Event>
        </ns1:UniversalEvent>
      </ns1:Body>
    </ns1:UniversalInterchangeInclude>
  </xsl:template>

  <xsl:template name="CreateContext">
    <xsl:param name="type"/>
    <xsl:param name="value"/>
    <xsl:param name="fallback"/>

    <xsl:if test="string-length(normalize-space($value)) &gt; 0 or string-length(normalize-space($fallback)) &gt; 0">
      <ns1:Context>
        <ns1:Type>
          <xsl:value-of select="$type"/>
        </ns1:Type>
        <ns1:Value>
          <xsl:choose>
            <xsl:when test="string-length(normalize-space($value)) &gt; 0">
              <xsl:value-of select="$value" />
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$fallback" />
            </xsl:otherwise>
          </xsl:choose>
        </ns1:Value>
      </ns1:Context>
    </xsl:if>
  </xsl:template>

  <xsl:template name="AddReferenceParameters">
    <xsl:param name="text"/>
    <xsl:variable name="separator" select="'|'"/>
    <xsl:choose>
      <xsl:when test="not(contains($text, $separator))">
        <xsl:call-template name="AddParameters">
          <xsl:with-param name="text" select="normalize-space($text)"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="AddParameters">
          <xsl:with-param name="text" select="normalize-space(substring-before($text, $separator))"/>
        </xsl:call-template>
        <xsl:call-template name="AddReferenceParameters">
          <xsl:with-param name="text" select="substring-after($text, $separator)"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="AddParameters">
    <xsl:param name="text"/>
    <xsl:variable name="separator" select="'='"/>
    <xsl:variable name="elementName" select="normalize-space(substring-before($text, $separator))"/>
    <xsl:variable name="elementValue" select="normalize-space(substring-after($text, $separator))"/>
    <xsl:if test="$elementName != '' and $elementValue != ''">
      <xsl:element name="ns1:{$elementName}">
        <xsl:value-of select="$elementValue" />
      </xsl:element>
    </xsl:if>

  </xsl:template>

  <xsl:template name="copyTagsAndAttributes">
    <xsl:param name ="tags" />
    <xsl:for-each select="$tags">
      <xsl:element name="ns1:{local-name()}">
        <xsl:for-each select="@*">
          <xsl:attribute name="{local-name()}">
            <xsl:value-of select="."/>
          </xsl:attribute>
        </xsl:for-each>
        <xsl:call-template name="copyTagsAndAttributes">
          <xsl:with-param name="tags" select="./*"/>
        </xsl:call-template>
        <xsl:value-of select="text()"/>
      </xsl:element>
    </xsl:for-each>
  </xsl:template>

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[

public string ParseErrorDescription(string errorMessage)
{
  if (!string.IsNullOrEmpty(errorMessage))
  {
    int startIndex = errorMessage.IndexOf("Error:");

    if (startIndex != -1)
    {
      int endIndex = errorMessage.IndexOf(".", startIndex);
      if (endIndex != -1)
      {
        var errorLength = "Error:".Length;
        return errorMessage.Substring(startIndex + errorLength, endIndex - (startIndex + errorLength - 1)).Trim();
      }
    }
  }
  return "";
}

public void GetInterchangeNumberAndMessageReference(string inputText)
{
 interchangeNumber = string.Empty;
 messageReference = string.Empty;
 if (!string.IsNullOrEmpty(inputText))
 {
  inputText = inputText.Replace(" ", "");
  if (inputText.Contains("_"))
  {
   interchangeNumber = inputText.Split(new[] { '_' })[0];
   messageReference = inputText.Split(new[] { '_' })[1];
  }
  else
  {
   interchangeNumber = inputText;
  }
 }
}

public string interchangeNumber = string.Empty;
public string messageReference = string.Empty;

public string GetInterchangeNumber() 
{ 
  return interchangeNumber; 
}
public string GetMessageReference()
{ 
  return messageReference; 
}

public bool ShouldGenerateContainerNo(string documentName)
{ 
  var result = false;
  if (!string.IsNullOrEmpty(documentName))
  {
    string[] documents = new string[] { "TRC", "AMQ", "DOS", "LPD", "LDE", "CDM", "Certified Pickup" , "Export Pre-Advice Notification" };
    foreach (var doc in documents)
    {
      if (documentName.Contains(doc))
      {
        result = true;
        break;
      }
    }
  }
  return result;
}  
]]>
  </msxsl:script>
</xsl:stylesheet>

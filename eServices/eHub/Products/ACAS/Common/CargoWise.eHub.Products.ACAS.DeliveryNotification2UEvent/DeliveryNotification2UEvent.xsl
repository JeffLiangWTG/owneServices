<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl i0 DateMapper ContextAccessor CodeMapper userCSharp" version="1.0"
                xmlns:i0="http://cargowise.com/ehub/core/2018/06"
                xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2012/11"
                xmlns:ns1="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:DateMapper="http://schemas.microsoft.com/BizTalk/2003/DateMapper"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes"/>
  <xsl:template match="/">
  <xsl:apply-templates select="i0:DeliveryNotificationMessage" />
  </xsl:template>
  <xsl:template match="i0:DeliveryNotificationMessage">
  <xsl:apply-templates select="//*[local-name()='UniversalShipment']" />
  </xsl:template>

  <xsl:template match="//*[local-name()='UniversalShipment']">
  <xsl:variable name="documentName" select="//*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='DataContext']/*[local-name()='DocumentaryOverride']/*[local-name()='DocumentName']/text()" />
  <xsl:variable name="key" select="//*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='DataContext']/*[local-name()='DataSource']/*[local-name()='Key']/text()" />
  <xsl:variable name="type" select="//*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='DataContext']/*[local-name()='DataSource']/*[local-name()='Type']/text()" />
  <xsl:variable name="inboxPK" select="/*[local-name()='DeliveryNotificationMessage']/*[local-name()='InternalTrackingID']/text()" />
  <xsl:variable name="destinationParty" select="ContextAccessor:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')" />
  <xsl:variable name="InterchangeNumber" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference','@reference', '@senderId', 'ACAS_US','@recipientId', $destinationParty ,'@ST_ID', 'ACASUS','@value', $inboxPK)" />
  <xsl:variable name="workflowData" select="//*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='DataContext']//*[local-name()='Workflow'][1]/*[local-name()='Company' or local-name()='EventBranch']"/>
  <xsl:variable name="eventBranch" select="//*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='DataContext']/*[local-name()='Workflow']/*[local-name()='EventBranch']/text()" />
  <xsl:variable name="company" select="//*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='DataContext']/*[local-name()='Workflow']/*[local-name()='Company']/*[local-name()='Code']/text()" />
  <xsl:variable name="operationalPortCode" select="//*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][*[local-name()='Key']/text()='OperationalPort_Code']/*[local-name()='Value']/text()" />
  <xsl:variable name="purpose" select="//*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='DataContext']/*[local-name()='DocumentaryOverride']/*[local-name()='Purpose']/text()" />
  <xsl:variable name="hasHAWBNumber">
    <xsl:choose>
    <xsl:when test="namespace-uri()= 'http://www.cargowise.com/Schemas/Universal/2012/11/HouseCheckList/1'">
      <xsl:value-of select="'false'" />
    </xsl:when>
    <xsl:otherwise>
      <xsl:value-of select="'true'" />
    </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="HAWBNumber" select="//*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='WayBillNumber']/text()" />

  <xsl:variable name="MAWBNumber">
    <xsl:choose>
    <xsl:when test="namespace-uri()= 'http://www.cargowise.com/Schemas/Universal/2012/11/HouseCheckList/1'">
      <xsl:value-of select="$HAWBNumber" />
    </xsl:when>
    <xsl:otherwise>
      <xsl:value-of select="//*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][./*[local-name()='Key'] = 'MAWB']/*[local-name()='Value']/text()" />
    </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <ns1:UniversalInterchange>
    <Header>
    <SenderID>ADVANCE_AIR_CARGO_REPORT</SenderID>
    <RecipientID>
      <xsl:value-of select="$destinationParty" />
    </RecipientID>
    </Header>
    <Body>
    <ns0:UniversalEvent>
      <ns0:Event>
      <ns0:DataContext>
        <ns0:DocumentaryOverride>
        <ns0:DocumentName>
          <xsl:value-of select="$documentName" />
        </ns0:DocumentName>
        </ns0:DocumentaryOverride>
        <ns0:DataTargetCollection>
        <ns0:DataTarget>
          <ns0:Key>
          <xsl:value-of select="$key" />
          </ns0:Key>
          <ns0:Type>
          <xsl:value-of select="$type" />
          </ns0:Type>
        </ns0:DataTarget>
        </ns0:DataTargetCollection>
        <xsl:if test="$workflowData">
        <ns0:Workflow>
          <ns0:CodesMappedToTarget>true</ns0:CodesMappedToTarget>
          <xsl:call-template name="copyTagsAndAttributes">
          <xsl:with-param name="tags" select="$workflowData"/>
          </xsl:call-template>
        </ns0:Workflow>
        </xsl:if>
      </ns0:DataContext>
      <ns0:EventTime>
        <xsl:value-of select="DateMapper:CurrentDateTime('s')" />
      </ns0:EventTime>
      <xsl:variable name="errorDescription" select="/*[local-name()='DeliveryNotificationMessage']/*[local-name()='ErrorDescription']/text()" />
      <xsl:variable name="notificationType" select="/*[local-name()='DeliveryNotificationMessage']/*[local-name()='NotificationType']/text()"/>

      <xsl:choose>
        <xsl:when test="$notificationType != 'ACK'">
        <ns0:EventType>IRJ</ns0:EventType>
        </xsl:when>
        <xsl:otherwise>
        <ns0:EventType>ISN</ns0:EventType>
        </xsl:otherwise>
      </xsl:choose>
      <ns0:EventParameters>
        <ns0:MessageType>
        <xsl:value-of select="$documentName" />
        </ns0:MessageType>
        <xsl:if test="$errorDescription != ''">
        <xsl:call-template name="AddMissingParameters">
          <xsl:with-param name="text" select="$errorDescription"/>
        </xsl:call-template>
        <xsl:call-template name="AddReferenceParameters">
          <xsl:with-param name="text" select="$errorDescription"/>
          <xsl:with-param name="operationalPortCode" select="$operationalPortCode"/>
        </xsl:call-template>
        </xsl:if>
        <xsl:if test="$operationalPortCode != ''">
        <ns0:Location>
          <xsl:value-of select="$operationalPortCode"/>
        </ns0:Location>
        </xsl:if>
        <xsl:if test="$purpose != ''">
        <ns0:Status>
          <xsl:value-of select="$purpose"/>
        </ns0:Status>
        </xsl:if>
      </ns0:EventParameters>
      <ns0:ContextCollection>
        <xsl:if test="$hasHAWBNumber='true'">
        <xsl:call-template name="AddContext">
          <xsl:with-param name="type" select="'HAWBNumber'"/>
          <xsl:with-param name="value" select="$HAWBNumber"/>
        </xsl:call-template>
        </xsl:if>
        <xsl:call-template name="AddContext">
        <xsl:with-param name="type" select="'MAWBNumber'"/>
        <xsl:with-param name="value" select="$MAWBNumber"/>
        </xsl:call-template>
        <xsl:call-template name="AddContext">
        <xsl:with-param name="type" select="'InterchangeNumber'"/>
        <xsl:with-param name="value" select="$InterchangeNumber"/>
        </xsl:call-template>
        <xsl:call-template name="AddContext">
        <xsl:with-param name="type" select="'EventBranch'"/>
        <xsl:with-param name="value" select="$eventBranch"/>
        </xsl:call-template>
        <xsl:call-template name="AddContext">
        <xsl:with-param name="type" select="'Company'"/>
        <xsl:with-param name="value" select="$company"/>
        </xsl:call-template>
        <xsl:call-template name="AddContext">
        <xsl:with-param name="type" select="'OperationPortCode'"/>
        <xsl:with-param name="value" select="$operationalPortCode"/>
        </xsl:call-template>
        <xsl:call-template name="AddContext">
        <xsl:with-param name="type" select="'Purpose'"/>
        <xsl:with-param name="value" select="$purpose"/>
        </xsl:call-template>
      </ns0:ContextCollection>
      </ns0:Event>
    </ns0:UniversalEvent>
    </Body>
  </ns1:UniversalInterchange>
  </xsl:template>

  <xsl:template name="AddMissingParameters">
  <xsl:param name="text"/>
  <xsl:variable name="separator" select="'|'"/>
  <xsl:choose>
    <xsl:when test="contains($text, $separator)">
    <xsl:choose>
      <xsl:when test="not(contains($text, 'Department='))">
      <ns0:Department>CargoWise</ns0:Department>
      </xsl:when>
    </xsl:choose>
    <xsl:choose>
      <xsl:when test="not(contains($text, 'Reason='))">
      <ns0:Reason>
        <xsl:value-of select="$text"/>
      </ns0:Reason>
      </xsl:when>
    </xsl:choose>
    </xsl:when>
  </xsl:choose>
  </xsl:template>

  <xsl:template name="AddReferenceParameters">
  <xsl:param name="text"/>
  <xsl:param name="operationalPortCode" select="''"/>

  <xsl:variable name="separator" select="'|'"/>
  <xsl:choose>
    <xsl:when test="not(contains($text, $separator))">
    <xsl:call-template name="AddParameters">
      <xsl:with-param name="text" select="normalize-space($text)"/>
      <xsl:with-param name="operationalPortCode" select="$operationalPortCode"/>
    </xsl:call-template>
    </xsl:when>
    <xsl:otherwise>
    <xsl:call-template name="AddParameters">
      <xsl:with-param name="text" select="normalize-space(substring-before($text, $separator))"/>
      <xsl:with-param name="operationalPortCode" select="$operationalPortCode"/>
    </xsl:call-template>
    <xsl:call-template name="AddReferenceParameters">
      <xsl:with-param name="text" select="substring-after($text, $separator)"/>
      <xsl:with-param name="operationalPortCode" select="$operationalPortCode"/>
    </xsl:call-template>
    </xsl:otherwise>
  </xsl:choose>
  </xsl:template>

  <xsl:template name="AddParameters">
  <xsl:param name="text"/>
  <xsl:param name="operationalPortCode"/>

  <xsl:variable name="separator" select="'='"/>
  <xsl:variable name="elementName" select="normalize-space(substring-before($text, $separator))"/>
  <xsl:choose>
    <xsl:when test="not(userCSharp:IsValidNodeName($elementName))">
    <ns0:Department>CargoWise</ns0:Department>
    <ns0:Reason>
      <xsl:value-of select="$text"/>
    </ns0:Reason>
    </xsl:when>
    <xsl:otherwise>
    <xsl:variable name="elementValue" select="normalize-space(substring-after($text, $separator))"/>
    <xsl:if test="$elementName != '' and $elementValue != '' and (($elementName = 'Location' and string-length($operationalPortCode)=0) or $elementName != 'Location')">
      <xsl:element name="ns0:{$elementName}">
      <xsl:value-of select="$elementValue" />
      </xsl:element>
    </xsl:if>
    </xsl:otherwise>
  </xsl:choose>
  </xsl:template>

  <xsl:template name="AddContext">
  <xsl:param name="type"/>
  <xsl:param name="value"/>
  <xsl:if test="$value != ''">
    <ns0:Context>
    <ns0:Type>
      <xsl:value-of select="$type"/>
    </ns0:Type>
    <ns0:Value>
      <xsl:value-of select="$value"/>
    </ns0:Value>
    </ns0:Context>
  </xsl:if>
  </xsl:template>

  <xsl:template name="copyTagsAndAttributes">
  <xsl:param name ="tags" />
  <xsl:for-each select="$tags">
    <xsl:element name="ns0:{local-name()}">
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
public bool IsValidNodeName(string input)
{
  return System.Text.RegularExpressions.Regex.IsMatch(input, @"^[a-zA-Z]+$");
}

public string SubstringSafe(string text, int startIndex, int length)
{
  string result = "";

  if (startIndex < 0)
  {
    startIndex = 0;
  }

  int actualLength = text.Length - startIndex;
  if (actualLength > 0)
  {
    if (actualLength > length && length >= 0)
    {
      actualLength = length;
    }
    result = text.Substring(startIndex, actualLength);
  }

  return result;
}
]]>
  </msxsl:script>
</xsl:stylesheet>



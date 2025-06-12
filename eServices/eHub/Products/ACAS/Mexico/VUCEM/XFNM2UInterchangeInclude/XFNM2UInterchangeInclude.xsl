<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt"
    xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var"
    exclude-result-prefixes="msxsl var ns2 s1 CodeMapper ContextAccessor DataModelAccessor DateMapper userCSharp" version="1.0"
    xmlns:s0="http://www.cargowise.com/Schemas/Universal/2012/11"
    xmlns:ns2="iata:response:3"
    xmlns:s1="iata:datamodel:3"
    xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
    xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
    xmlns:DateMapper="http://schemas.microsoft.com/BizTalk/2003/DateMapper"
    xmlns:DataModelAccessor="http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"
    xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes" />

  <xsl:template match="/">
    <xsl:apply-templates select="ns2:Response" />
  </xsl:template>

  <xsl:template match="ns2:Response">
    <xsl:variable name="SenderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')" />
    <xsl:variable name="DestinationParty" select="ContextAccessor:GetContextProperty('DestinationParty','http://schemas.microsoft.com/BizTalk/2003/system-properties')" />
    <xsl:variable name="subscriptionType" select="'ACASMX'" />

    <xsl:variable name="MessageRecipientId" select="ns2:MessageHeaderDocument/s1:RecipientParty/s1:PrimaryID/text()" />
    <xsl:variable name="subscribeReferenceValue" select="ns2:BusinessHeaderDocument/s1:ID/text()" />

    <xsl:variable name="RecipientId" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedClients', '', '@senderId', $SenderID, '@ST_ID', $subscriptionType, '@value', $subscribeReferenceValue)" />

    <xsl:variable name="subscribedDocumentName" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $subscriptionType, '@value', $subscribeReferenceValue, '@referenceType', 'DocumentName')" />
    <xsl:variable name="subscribedShipmentId" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $subscriptionType, '@value', $subscribeReferenceValue, '@referenceType', 'ShipmentId')" />
    <xsl:variable name="subscribedForwardingType" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $subscriptionType, '@value', $subscribeReferenceValue, '@referenceType', 'ForwardingType')" />

    <xsl:variable name="statusCode" select="ns2:BusinessHeaderDocument/s1:StatusCode/text()" />
    <xsl:variable name="eventType">
      <xsl:choose>
        <xsl:when test="$statusCode='Received'">MPP</xsl:when>
        <xsl:when test="$statusCode='Rejected'">MRJ</xsl:when>
        <xsl:when test="$statusCode='Processed'">MAA</xsl:when>
      </xsl:choose>
    </xsl:variable>

    <s0:UniversalInterchangeInclude>
      <s0:Body>
        <xsl:if test="$subscribedShipmentId != '' and $eventType != ''">
          <s0:UniversalEvent>
            <s0:Event>
              <s0:DataContext>
                <s0:DocumentaryOverride>
                  <s0:DocumentName>
                    <xsl:value-of select="$subscribedDocumentName" />
                  </s0:DocumentName>
                </s0:DocumentaryOverride>
                <s0:DataTargetCollection>
                  <s0:DataTarget>
                    <s0:Key>
                      <xsl:value-of select="$subscribedShipmentId" />
                    </s0:Key>
                    <s0:Type>
                      <xsl:value-of select="$subscribedForwardingType" />
                    </s0:Type>
                  </s0:DataTarget>
                </s0:DataTargetCollection>
              </s0:DataContext>

              <s0:EventTime>
                <xsl:value-of select="ns2:MessageHeaderDocument/s1:IssueDateTime/text()" />
              </s0:EventTime>

              <s0:EventType>
                <xsl:value-of select="$eventType" />
              </s0:EventType>
              <s0:EventParameters>
                <s0:Department>Customs</s0:Department>
                <s0:MessageType>
                  <xsl:value-of select="$subscribedDocumentName" />
                </s0:MessageType>
                <s0:Location>MX</s0:Location>

                <xsl:choose>
                  <xsl:when test="$eventType='MPP' or $eventType='MAA'">
                    <s0:ReferenceNumber>
                      <xsl:value-of select="ns2:ResponseStatus/s1:Reason/text()"/>
                    </s0:ReferenceNumber>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:variable name="reason">
                      <xsl:choose>
                        <xsl:when test="count(ns2:ResponseStatus[s1:ConditionCode/text()='Error']) > 1">Refer Event Context Information for errors</xsl:when>
                        <xsl:otherwise>
                          <xsl:value-of select="ns2:ResponseStatus[s1:ConditionCode/text()='Error']/s1:Reason/text()"/>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>
                    <s0:Reason>
                      <xsl:value-of select="$reason"/>
                    </s0:Reason>
                  </xsl:otherwise>
                </xsl:choose>
              </s0:EventParameters>

              <s0:ContextCollection>
                <xsl:call-template name="GenerateContext">
                  <xsl:with-param name="Type" select="'HAWBNumber'" />
                  <xsl:with-param name="Value" select="$subscribeReferenceValue" />
                </xsl:call-template>

                <xsl:if test="$eventType='MRJ'">
                  <xsl:if test="count(ns2:ResponseStatus[s1:ConditionCode/text()='Error']) > 1">
                    <xsl:for-each select="ns2:ResponseStatus[s1:ConditionCode/text()='Error']">
                      <xsl:call-template name="GenerateContext">
                        <xsl:with-param name="Type" select="'ErrorText'" />
                        <xsl:with-param name="Value" select="s1:Reason/text()" />
                      </xsl:call-template>
                    </xsl:for-each>
                  </xsl:if>
                </xsl:if>

              </s0:ContextCollection>
            </s0:Event>
          </s0:UniversalEvent>
        </xsl:if>
      </s0:Body>
    </s0:UniversalInterchangeInclude>
  </xsl:template>

  <xsl:template name="GenerateContext">
    <xsl:param name="Type" />
    <xsl:param name="Value" />
    <xsl:if test="$Value != ''">
      <s0:Context>
        <s0:Type>
          <xsl:value-of select="$Type" />
        </s0:Type>
        <s0:Value>
          <xsl:value-of select="$Value" />
        </s0:Value>
      </s0:Context>
    </xsl:if>
  </xsl:template>

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[
public string ToUpper(string input)
{
  string result = "";
  if (input != null)
  {
    result = input;
  }
  return result.ToUpper();
}
]]>
  </msxsl:script>

</xsl:stylesheet>

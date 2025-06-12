<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt"
    xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var"
    exclude-result-prefixes="msxsl var ns0 s0 CodeMapper ContextAccessor DateMapper DataModelAccessor userCSharp" version="1.0"
    xmlns:s0="http://wisetechglobal.com/ehub/acas/br/CheckStatusSchema"
    xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2012/11"
    xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
    xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
    xmlns:DateMapper="http://schemas.microsoft.com/BizTalk/2003/DateMapper"
    xmlns:DataModelAccessor="http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"
    xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes" />

  <xsl:template match="/">
    <xsl:apply-templates select="s0:CheckStatusResponse" />
  </xsl:template>

  <xsl:template match="s0:CheckStatusResponse">
    <xsl:variable name="SenderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')" />
    <xsl:variable name="DestinationParty" select="ContextAccessor:GetContextProperty('DestinationParty','http://schemas.microsoft.com/BizTalk/2003/system-properties')" />
    <xsl:variable name="subscriptionType" select="'ACASBR'" />

    <xsl:variable name="ACASRecipientID">
      <xsl:choose>
        <xsl:when test="contains($SenderID, 'TST')">ACAS_BRTest</xsl:when>
        <xsl:otherwise>ACAS_BR</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <ns0:UniversalInterchangeInclude>
      <ns0:Body>
        <xsl:for-each select="s0:CheckStatusResponse">

          <xsl:variable name="protocolNumber" select="s0:protocolNumber/text()" />

          <xsl:variable name="CCTRecipientId" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedClients', '', '@senderId', $SenderID, '@ST_ID', $subscriptionType, '@value', $protocolNumber)" />
          <xsl:variable name="messageReference" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', $CCTRecipientId, '@ST_ID', $subscriptionType, '@value', $protocolNumber, '@referenceType', 'ProtocolNumber')" />

          <xsl:variable name="subscribedShipmentId" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', $CCTRecipientId, '@ST_ID', $subscriptionType, '@value', $messageReference, '@referenceType', 'ShipmentId')" />
          <xsl:variable name="subscribedDocumentName" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', $CCTRecipientId, '@ST_ID', $subscriptionType, '@value', $messageReference, '@referenceType', 'DocumentName')" />
          <xsl:variable name="subscribedForwardingType" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', $CCTRecipientId, '@ST_ID', $subscriptionType, '@value', $messageReference, '@referenceType', 'ForwardingType')" />
          <xsl:variable name="subscribedActionType" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', $CCTRecipientId, '@ST_ID', $subscriptionType, '@value', $messageReference, '@referenceType', 'ActionPurpose')" />

          <xsl:variable name="statusCode" select="s0:status/text()" />
          <xsl:variable name="eventType">
            <xsl:choose>
              <xsl:when test="$statusCode='Received'">MPP</xsl:when>
              <xsl:when test="$statusCode='Rejected'">MRJ</xsl:when>
              <xsl:when test="$statusCode='Processed' and $subscribedActionType='WTH'">MWA</xsl:when>
              <xsl:when test="$statusCode='Processed'">MAA</xsl:when>
            </xsl:choose>
          </xsl:variable>

          <xsl:if test="$subscribedShipmentId != ''">
            <ns0:UniversalEvent>
              <ns0:Event>
                <ns0:DataContext>
                  <ns0:DocumentaryOverride>
                    <ns0:DocumentName>
                      <xsl:value-of select="$subscribedDocumentName" />
                    </ns0:DocumentName>
                  </ns0:DocumentaryOverride>
                  <ns0:DataTargetCollection>
                    <ns0:DataTarget>
                      <ns0:Key>
                        <xsl:value-of select="$subscribedShipmentId" />
                      </ns0:Key>
                      <ns0:Type>
                        <xsl:value-of select="$subscribedForwardingType" />
                      </ns0:Type>
                    </ns0:DataTarget>
                  </ns0:DataTargetCollection>
                </ns0:DataContext>

                <ns0:EventTime>
                  <xsl:value-of select="s0:dateTime/text()"/>
                </ns0:EventTime>

                <ns0:EventType>
                  <xsl:value-of select="$eventType" />
                </ns0:EventType>

                <ns0:EventParameters>
                  <ns0:Department>Customs</ns0:Department>
                  <ns0:MessageType>
                    <xsl:value-of select="$subscribedDocumentName" />
                  </ns0:MessageType>
                  <ns0:Location>BR</ns0:Location>
                  <ns0:ReferenceNumber>
                    <xsl:value-of select="$protocolNumber"/>
                  </ns0:ReferenceNumber>
                  <xsl:if test="$eventType='MRJ'">
                    <ns0:Reason>
                      <xsl:choose>
                        <xsl:when test="count(s0:errorList) > 1">Refer Event Context Information for errors</xsl:when>
                        <xsl:otherwise>
                          <xsl:value-of select="userCSharp:ReplaceAllSensitiveKeyWords(userCSharp:GetErrorText(s0:errorList/s0:description/text(), s0:errorList/s0:detail/text()), '|', ';')"/>
                        </xsl:otherwise>
                      </xsl:choose>
                    </ns0:Reason>
                  </xsl:if>
                </ns0:EventParameters>
                <ns0:ContextCollection>
                  <xsl:if test="$subscribedForwardingType = 'ForwardingShipment'">
                    <xsl:call-template name="GenerateContext">
                      <xsl:with-param name="type" select="'HAWBNumber'" />
                      <xsl:with-param name="value" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', $CCTRecipientId, '@ST_ID', $subscriptionType, '@value', $messageReference, '@referenceType', 'HAWB')" />
                    </xsl:call-template>
                    <xsl:call-template name="GenerateContext">
                      <xsl:with-param name="type" select="'MAWBNumber'" />
                      <xsl:with-param name="value" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', $CCTRecipientId, '@ST_ID', $subscriptionType, '@value', $messageReference, '@referenceType', 'MAWB')" />
                    </xsl:call-template>
                  </xsl:if>

                  <xsl:if test="$eventType='MRJ' and count(s0:errorList) > 1">
                    <xsl:for-each select="s0:errorList">

                      <xsl:call-template name="GenerateContext">
                        <xsl:with-param name="type" select="'ErrorText'" />
                        <xsl:with-param name="value" select="userCSharp:GetErrorText(s0:description/text(), s0:detail/text())"/>
                      </xsl:call-template>
                    </xsl:for-each>

                  </xsl:if>
                </ns0:ContextCollection>
              </ns0:Event>
            </ns0:UniversalEvent>
          </xsl:if>
        </xsl:for-each>
      </ns0:Body>
    </ns0:UniversalInterchangeInclude>
  </xsl:template>

  <xsl:template name="GenerateContext">
    <xsl:param name="type" />
    <xsl:param name="value" />
    <xsl:if test="$value != ''">
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
public string GetErrorText(string description, string detail)
{
  var errortxt = "";

  if (description.Length > 0)
  {
    errortxt += description + "\n";
  }

  if (detail.Length > 0)
  {
    errortxt += detail + "\n";
  }
  return errortxt.TrimEnd('\n');
}

public static string ReplaceAllSensitiveKeyWords(string source ,string targetChar, string replacement)
{
    return source.Replace(targetChar, replacement);
}
]]>
  </msxsl:script>

</xsl:stylesheet>

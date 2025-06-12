<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt"
    xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var"
    exclude-result-prefixes="msxsl var s0 ns0 CodeMapper ContextAccessor DateMapper DataModelAccessor userCSharp" version="1.0"
    xmlns:s0="http://www.cargowise.com/Schemas/FPMA/CIN/FNA"
    xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2012/11"
    xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
    xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
    xmlns:DateMapper="http://schemas.microsoft.com/BizTalk/2003/DateMapper"
    xmlns:DataModelAccessor="http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"
    xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes" />

  <xsl:template match="/">
    <xsl:apply-templates select="/s0:CIN_FNA" />
  </xsl:template>

  <xsl:template match="/s0:CIN_FNA">
    <xsl:variable name="messageReference" select="userCSharp:GetMessageReference(s0:OriginalMessage/s0:Data/text())" />

    <ns0:UniversalInterchangeInclude>
      <ns0:Body>
        <xsl:if test="$messageReference != ''">
          <xsl:variable name="senderId" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')" />
          <xsl:variable name="recipientId" select="ContextAccessor:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')" />

          <xsl:variable name="serviceProviderMsgId" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE_AIR', 'FORWARDING_PORT_MESSAGE_AIR', 'FPMA System Configuration', 'Port Settings', 'MSGID', $senderId)" />

          <xsl:variable name="subscribedDocumentName" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $senderId, '@recipientId', '', '@ST_ID', $serviceProviderMsgId, '@value', $messageReference, '@referenceType', 'DocumentName')" />
          <xsl:variable name="subscribedJobNumber" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $senderId, '@recipientId', '', '@ST_ID', $serviceProviderMsgId, '@value', $messageReference, '@referenceType', 'JobNumber')" />
          <xsl:variable name="subscribedForwardingType" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $senderId, '@recipientId', '', '@ST_ID', $serviceProviderMsgId, '@value', $messageReference, '@referenceType', 'ForwardingType')" />
          <xsl:variable name="subscribedOperationPort" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $senderId, '@recipientId', '', '@ST_ID', $serviceProviderMsgId, '@value', $messageReference, '@referenceType', 'OperationPort')" />

          <xsl:variable name="recipientID" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedClients', '', '@senderId', $senderId, '@ST_ID', $serviceProviderMsgId, '@value', $messageReference)"/>
          <xsl:if test="$recipientID!=''">
            <xsl:variable name="SetDestinationParty" select="ContextAccessor:SetContextProperty('DestinationParty','http://schemas.microsoft.com/BizTalk/2003/system-properties', $recipientID)"/>
          </xsl:if>

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
                      <xsl:value-of select="$subscribedJobNumber" />
                    </ns0:Key>
                    <ns0:Type>
                      <xsl:value-of select="$subscribedForwardingType" />
                    </ns0:Type>
                  </ns0:DataTarget>
                </ns0:DataTargetCollection>
              </ns0:DataContext>
              <ns0:EventTime>
                <xsl:value-of select="DateMapper:ConvertUTCToLocalTimeByUNLOCO(DateMapper:CurrentDateTimeUTC('yyyy-MM-ddTHH:mm:ssZ'), 'FRPAR')" />
              </ns0:EventTime>
              <ns0:EventType>IRJ</ns0:EventType>
              <ns0:EventParameters>
                <ns0:Department>Terminal</ns0:Department>
                <ns0:MessageType>
                  <xsl:value-of select="$subscribedDocumentName" />
                </ns0:MessageType>
                <ns0:Location>
                  <xsl:value-of select="$subscribedOperationPort" />
                </ns0:Location>
                <ns0:Reason>
                  <xsl:value-of select="concat(s0:Reason/s0:Data/text(), s0:Segment3/s0:Data/text())" />
                </ns0:Reason>
              </ns0:EventParameters>
              <ns0:EventReference/>
              <ns0:ContextCollection/>
            </ns0:Event>
          </ns0:UniversalEvent>
        </xsl:if>
      </ns0:Body>
    </ns0:UniversalInterchangeInclude>
  </xsl:template>

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[
    public static string GetMessageReference(string input)
    {
        string pattern = @"RFF\+ACD:(\d+)\'";
        Match match = Regex.Match(input, pattern);

        var messageReference = string.Empty;
        if (match.Success)
        {
            messageReference = match.Groups[1].Value;
        }

        return messageReference;
    }
]]>
  </msxsl:script>

</xsl:stylesheet>
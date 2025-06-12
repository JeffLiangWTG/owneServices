<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 CodeMapper ContextAccessor DataModelAccessor DateMapper userCSharp" version="1.0"
                xmlns:s0="https://www.cargowise.com/Schemas/FPMA/CARGONAUT/EDI757"
                xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2012/11"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
                xmlns:DataModelAccessor="http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"
                xmlns:DateMapper="http://schemas.microsoft.com/BizTalk/2003/DateMapper"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">

  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes" />

  <xsl:template match="/">
    <xsl:apply-templates select="s0:CGN_EDI757" />
  </xsl:template>

  <xsl:template match="s0:CGN_EDI757">
    <xsl:variable name="messageReference" select="userCSharp:GetMessageReference(s0:Message/text())" />
    <xsl:variable name="UNHinterchangeNumber" select="userCSharp:GetUNHInterchangeNumber(s0:Message/text())" />

    <xsl:variable name="senderId" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')" />
    <xsl:variable name="serviceProviderMsgId" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE_AIR', 'FORWARDING_PORT_MESSAGE_AIR', 'FPMA System Configuration' , 'Port Settings' , 'MSGID', $senderId)" />
    <xsl:variable name="subscribedJobNumber" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $senderId, '@recipientId', '', '@ST_ID', $serviceProviderMsgId, '@value', $messageReference, '@referenceType', 'JobNumber')" />

    <ns0:UniversalInterchangeInclude>
      <ns0:Body>
        <xsl:if test="$subscribedJobNumber != ''">

          <xsl:variable name="subscribedDocumentName" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $senderId, '@recipientId', '', '@ST_ID', $serviceProviderMsgId, '@value', $messageReference, '@referenceType', 'DocumentName')" />
          <xsl:variable name="subscribedForwardingType" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $senderId, '@recipientId', '', '@ST_ID', $serviceProviderMsgId, '@value', $messageReference, '@referenceType', 'ForwardingType')" />
          <xsl:variable name="subscribedOperationPort" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $senderId, '@recipientId', '', '@ST_ID', $serviceProviderMsgId, '@value', $messageReference, '@referenceType', 'OperationPort')" />

          <xsl:variable name="recipientID" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedClients', '', '@senderId', $senderId, '@ST_ID', $serviceProviderMsgId, '@value', $messageReference)"/>
          <xsl:if test="$recipientID!=''">
            <xsl:variable name="SetDestinationParty" select="ContextAccessor:SetContextProperty('DestinationParty','http://schemas.microsoft.com/BizTalk/2003/system-properties', $recipientID)"/>
          </xsl:if>

          <xsl:variable name="serviceProvider" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE_AIR', 'FORWARDING_PORT_MESSAGE_AIR', 'FPMA System Configuration', 'Port Settings', 'Name', $senderId)"/>
          <xsl:if test="$recipientID!='' and $messageReference!=''">
            <xsl:variable name="subscribeStatusUpdate" select="DataModelAccessor:InsertSubscriptionValue('CGNTRC', $recipientID, $serviceProvider, concat($subscribedJobNumber, '_', $messageReference, '_', $UNHinterchangeNumber), 'MAA', 'STU-757')" />
          </xsl:if>

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
                <xsl:value-of select="DateMapper:ConvertToDateTimeString(userCSharp:GetMessageDateTime(s0:Message/text()), 'yyyyMMddHHmm', 'yyyy-MM-ddTHH:mm:ss')"/>
              </ns0:EventTime>
              <ns0:EventType>MAA</ns0:EventType>
              <ns0:EventParameters>
                <ns0:Department>Terminal</ns0:Department>
                <ns0:MessageType>
                  <xsl:value-of select="$subscribedDocumentName"/>
                </ns0:MessageType>
                <ns0:Location>
                  <xsl:value-of select="$subscribedOperationPort"/>
                </ns0:Location>
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
    public string GetMessageReference(string input)
    {
      string pattern = @"RFF\+ACD:(.*?)(\:|\')";
      return GetRegexValue(input, pattern);
    }

    public string GetUNHInterchangeNumber(string input)
    {
      var pattern = @"UNH\+(.*?)\+";
      return GetRegexValue(input, pattern);
    }

    public string GetMessageDateTime(string input)
    {
      string pattern = @"DTM\+97:(.*?):203\'";
      Match match = Regex.Match(input, pattern);

      var messageDateTime = string.Empty;
      if (match.Success)
      {
          messageDateTime = match.Groups[1].Value;
      }

      return messageDateTime;
    }

    private string GetRegexValue(string input, string pattern)
    {
      try
      {
        var match = Regex.Match(input, pattern);
        return match.Success ? match.Groups[1].Value : string.Empty;
      }
      catch (Exception ex)
      {
        return string.Empty;
      }
    }
]]>
  </msxsl:script>
</xsl:stylesheet>

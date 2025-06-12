<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 CodeMapper ContextAccessor DataModelAccessor DateMapper userCSharp" version="1.0"
                xmlns:s0="https://www.cargowise.com/Schemas/FPMA/CARGONAUT/EDI756"
                xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2012/11"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
                xmlns:DataModelAccessor="http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"
                xmlns:DateMapper="http://schemas.microsoft.com/BizTalk/2003/DateMapper"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">

  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes" />

  <xsl:template match="/">
    <xsl:apply-templates select="s0:CGN_EDI756" />
  </xsl:template>

  <xsl:template match="s0:CGN_EDI756">
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
            <xsl:variable name="subscribeStatusUpdate" select="DataModelAccessor:InsertSubscriptionValue('CGNTRC', $recipientID, $serviceProvider, concat($subscribedJobNumber, '_', $messageReference, '_', $UNHinterchangeNumber), 'MRJ', 'STU-756')" />
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
              <ns0:EventType>MRJ</ns0:EventType>

              <xsl:variable name="errors" select="userCSharp:GetMessageErrors(s0:Message/text())" />

              <ns0:EventParameters>
                <ns0:Department>Terminal</ns0:Department>
                <ns0:MessageType>
                  <xsl:value-of select="$subscribedDocumentName"/>
                </ns0:MessageType>
                <ns0:Location>
                  <xsl:value-of select="$subscribedOperationPort"/>
                </ns0:Location>
                <xsl:if test="count($errors) > 0">
                  <ns0:Reason>
                    <xsl:choose>
                      <xsl:when test="count($errors) = 1">
                        <xsl:value-of select="$errors[1]/ErrorReason/text()"/>
                      </xsl:when>
                      <xsl:otherwise>See ContextCollection - ErrorText</xsl:otherwise>
                    </xsl:choose>
                  </ns0:Reason>
                </xsl:if>
              </ns0:EventParameters>
              <ns0:EventReference/>

              <xsl:choose>
                <xsl:when test="count($errors) > 1">
                  <ns0:ContextCollection>
                    <xsl:for-each select="$errors">
                      <ns0:Context>
                        <ns0:Type>ErrorText</ns0:Type>
                        <ns0:Value>
                          <xsl:value-of select="ErrorReason/text()"/>
                        </ns0:Value>
                      </ns0:Context>
                    </xsl:for-each>
                  </ns0:ContextCollection>
                </xsl:when>
                <xsl:otherwise>
                  <ns0:ContextCollection/>
                </xsl:otherwise>
              </xsl:choose>
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
      return GetRegexValue(input, pattern);
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

    static System.Collections.Generic.Dictionary<string, string> dictionaryErrorCodeReason = new System.Collections.Generic.Dictionary<string, string>()
    {
      {"1", "Inventory Reporting Party no ECS Participant"},
      {"2", "Ship to Party no Participant"},
      {"3", "Inventory Reporting Party not active"},
      {"4", "Ship to Party not active"},
      {"5", "Inventory Reporting Party no freight forwarder"},
      {"6", "Ship to Party no ground handler"},
      {"7", "Goods Reference Type invalid"},
      {"8", "NEU goods identification invalid"},
      {"9", "Customs document type invalid"},
      {"10", "Customs declaration type invalid"},
      {"16", "Nr of pieces invalid, should not be 0"},
      {"17", "Weight gross invalid, should not be 0"},
      {"18", "MRN-number invalid, not according to algorithm"},
      {"19", "Date Time transaction later then message receive date"},
      {"20", "Not a valid timestamp"},
      {"21", "Goods reference number not available"},
      {"22", "Information Goods reference number incomplete"},
      {"23", "No subscription received"},
      {"24", "Weight net invalid, should not be 0"},
      {"27", "Quantity discrepancy should be empty, “156” or “250”"},
      {"28", "Gross weight discrepancy should be empty or “10”"},
      {"29", "Net weight discrepancy should be empty or “10”"}
    };

    public static XPathNodeIterator GetMessageErrors(string input)
    {
      var patternCode = @"ERP\+2\+([A-Z]+):1(\:|\')";
      var patternErrorCode = @"ERC\+(.*?)(\:|\')";
      var matchCollectionCode = Regex.Matches(input, patternCode);
      var matchCollectionErrorCode = Regex.Matches(input, patternErrorCode);
      XmlDocument doc = new XmlDocument();
      var errorCollection = doc.CreateElement("ErrorCollection");
      doc.AppendChild(errorCollection);
      for (var i = 0; i < matchCollectionErrorCode.Count; i++)
      {
        var errorCode = matchCollectionErrorCode[i].Groups[1].Value;
        var errorNode = doc.CreateElement("Error");
        var errorReason = doc.CreateElement("ErrorReason");
        errorReason.InnerText = matchCollectionCode[i].Groups[1].Value + " - " + (dictionaryErrorCodeReason.ContainsKey(errorCode) ? dictionaryErrorCodeReason[errorCode] : "Unknown error code " + errorCode);
        errorNode.AppendChild(errorReason);
        errorCollection.AppendChild(errorNode);
      }
      return doc.DocumentElement.CreateNavigator().Select("Error");
    }
]]>
  </msxsl:script>
</xsl:stylesheet>

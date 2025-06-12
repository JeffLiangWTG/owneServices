<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 CodeMapper ContextAccessor DataModelAccessor DateMapper userCSharp" version="1.0"
                xmlns:s0="https://www.cargowise.com/Schemas/FPMA/CARGONAUT/EDI758"
                xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2012/11"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
                xmlns:DataModelAccessor="http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"
                xmlns:DateMapper="http://schemas.microsoft.com/BizTalk/2003/DateMapper"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">

  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes" />

  <xsl:template match="/">
    <xsl:apply-templates select="s0:CGN_EDI758" />
  </xsl:template>

  <xsl:template match="s0:CGN_EDI758">
    <xsl:variable name="messageReference" select="userCSharp:GetMessageReference(s0:Message/text())" />
    <xsl:variable name="UNHinterchangeNumber" select="userCSharp:GetUNHInterchangeNumber(s0:Message/text())" />

    <xsl:variable name="senderId" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')" />
    <xsl:variable name="serviceProviderMsgId" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE_AIR', 'FORWARDING_PORT_MESSAGE_AIR', 'FPMA System Configuration' , 'Port Settings' , 'MSGID', $senderId)" />
    <xsl:variable name="subscribedJobNumber" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $senderId, '@recipientId', '', '@ST_ID', $serviceProviderMsgId, '@value', $messageReference, '@referenceType', 'JobNumber')" />

    <ns0:UniversalInterchangeInclude>
      <ns0:Body>
        <xsl:if test="$subscribedJobNumber != ''">

          <xsl:variable name="serviceProvider" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE_AIR', 'FORWARDING_PORT_MESSAGE_AIR', 'FPMA System Configuration', 'Port Settings', 'Name', $senderId)"/>

          <xsl:variable name="recipientID" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedClients', '', '@senderId', $senderId, '@ST_ID', $serviceProviderMsgId, '@value', $messageReference)"/>
          <xsl:choose>
            <xsl:when test="$recipientID = ''">
              <xsl:variable name="UNBReference" select="userCSharp:GetUNBReference(s0:Message/text())" />
              <xsl:variable name="UNBRecipient" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedClients', '', '@senderId', $senderId, '@ST_ID', $serviceProviderMsgId, '@value', $UNBReference)"/>
              <xsl:if test="$UNBRecipient!='' and $messageReference!=''">
                <xsl:variable name="SetDestinationParty" select="ContextAccessor:SetContextProperty('DestinationParty','http://schemas.microsoft.com/BizTalk/2003/system-properties', $UNBRecipient)"/>
                <xsl:variable name="subscribeStatusUpdate" select="DataModelAccessor:InsertSubscriptionValue('CGNTRC', $UNBRecipient, $serviceProvider, concat($subscribedJobNumber, '_', $messageReference, '_', $UNHinterchangeNumber), '', 'STU-758')" />
              </xsl:if>
            </xsl:when>
            <xsl:otherwise>
              <xsl:variable name="SetDestinationParty" select="ContextAccessor:SetContextProperty('DestinationParty','http://schemas.microsoft.com/BizTalk/2003/system-properties', $recipientID)"/>
              <xsl:if test="$recipientID!='' and $messageReference!=''">
                <xsl:variable name="subscribeStatusUpdate" select="DataModelAccessor:InsertSubscriptionValue('CGNTRC', $recipientID, $serviceProvider, concat($subscribedJobNumber, '_', $messageReference, '_', $UNHinterchangeNumber), '', 'STU-758')" />
              </xsl:if>
            </xsl:otherwise>
          </xsl:choose>

          <xsl:variable name="MAWBNumber" select="userCSharp:GetMAWBNumber(s0:Message/text())" />
          <xsl:variable name="LINSegmentCount" select="userCSharp:GetLINSegmentCount(s0:Message/text())" />

          <ns0:UniversalEvent>
            <ns0:Event>
              <ns0:DataContext>
                <ns0:DocumentaryOverride>
                  <ns0:DocumentName>Exit Notification Alert</ns0:DocumentName>
                </ns0:DocumentaryOverride>
                <ns0:DataTargetCollection>
                  <ns0:DataTarget>
                    <ns0:Key>
                      <xsl:value-of select="$subscribedJobNumber"/>
                    </ns0:Key>
                    <ns0:Type>ForwardingConsol</ns0:Type>
                  </ns0:DataTarget>
                </ns0:DataTargetCollection>
              </ns0:DataContext>
              <ns0:EventTime>
                <xsl:value-of select="DateMapper:ConvertToDateTimeString(userCSharp:GetMessageDateTime(s0:Message/text()), 'yyyyMMddHHmm', 'yyyy-MM-ddTHH:mm:ss')"/>
              </ns0:EventTime>
              <ns0:EventType>STU</ns0:EventType>

              <xsl:variable name="numberOfErrors" select="userCSharp:GetERCCount()" />

              <ns0:EventParameters>
                <ns0:Department>Terminal</ns0:Department>
                <ns0:MessageType>Export Notification (755)</ns0:MessageType>
                <ns0:Location>NLAMS</ns0:Location>
                <xsl:if test="$LINSegmentCount = 1">
                  <ns0:CustomsReferenceNumber>
                    <xsl:value-of select="userCSharp:GetMRNByindex(0)"/>
                  </ns0:CustomsReferenceNumber>
                </xsl:if>
                <ns0:Type>Exit Notification Alert</ns0:Type>
                <xsl:if test="$numberOfErrors > 0">
                  <ns0:Reason>
                    <xsl:choose>
                      <xsl:when test="$numberOfErrors = 1">
                        <xsl:value-of select="userCSharp:GetFirstERCError()" />
                      </xsl:when>
                      <xsl:otherwise>See ContextCollection - ErrorText</xsl:otherwise>
                    </xsl:choose>
                  </ns0:Reason>
                </xsl:if>
              </ns0:EventParameters>
              <ns0:EventReference/>

              <xsl:if test="$MAWBNumber != '' or $LINSegmentCount > 0 or $numberOfErrors > 0">
                <ns0:ContextCollection>
                  <xsl:if test="$MAWBNumber != ''">
                    <ns0:Context>
                      <ns0:Type>MAWBNumber</ns0:Type>
                      <ns0:Value>
                        <xsl:value-of select="$MAWBNumber"/>
                      </ns0:Value>
                    </ns0:Context>
                  </xsl:if>
                  <xsl:call-template name="LINSegmentLoop">
                    <xsl:with-param name="count" select="$LINSegmentCount"/>
                  </xsl:call-template>
                </ns0:ContextCollection>
              </xsl:if>
            </ns0:Event>
          </ns0:UniversalEvent>
        </xsl:if>
      </ns0:Body>
    </ns0:UniversalInterchangeInclude>
  </xsl:template>

  <xsl:template name="LINSegmentLoop">
    <xsl:param name="count"/>
    <xsl:param name="index" select="number(0)"/>

    <xsl:if test="$index &lt; $count">
      <xsl:variable name="HAWBNumber" select="userCSharp:GetHAWBNumberByIndex($index)"/>
      <xsl:if test="$HAWBNumber != ''">
        <ns0:Context>
          <ns0:Type>
            <xsl:choose>
              <xsl:when test="$count = 1">HAWBNumber</xsl:when>
              <xsl:otherwise>
                <xsl:value-of select ="concat('HAWBNumber', $index+1)"/>
              </xsl:otherwise>
            </xsl:choose>
          </ns0:Type>
          <ns0:Value>
            <xsl:value-of select="$HAWBNumber"/>
          </ns0:Value>
        </ns0:Context>
      </xsl:if>

      <xsl:variable name="MRNNumber" select="userCSharp:GetMRNByindex($index)"/>
      <xsl:if test="$MRNNumber != ''">
        <ns0:Context>
          <ns0:Type>
            <xsl:choose>
              <xsl:when test="$count = 1">MRN</xsl:when>
              <xsl:otherwise>
                <xsl:value-of select ="concat('MRN', $index+1)"/>
              </xsl:otherwise>
            </xsl:choose>
          </ns0:Type>
          <ns0:Value>
            <xsl:value-of select="$MRNNumber"/>
          </ns0:Value>
        </ns0:Context>
      </xsl:if>

      <xsl:variable name="errors" select="userCSharp:GetERCByLINIndex($index+1)" />
      <xsl:for-each select="$errors">
        <ns0:Context>
          <ns0:Type>
            <xsl:choose>
              <xsl:when test="$count = 1">ErrorText</xsl:when>
              <xsl:otherwise>
                <xsl:value-of select ="concat('ErrorText', $index+1)"/>
              </xsl:otherwise>
            </xsl:choose>
          </ns0:Type>
          <ns0:Value>
            <xsl:value-of select="ErrorReason/text()"/>
          </ns0:Value>
        </ns0:Context>
      </xsl:for-each>

      <xsl:call-template name="LINSegmentLoop">
        <xsl:with-param name="count" select="$count"/>
        <xsl:with-param name="index" select="$index+1"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[
    System.Collections.Generic.List<string> HAWBNumberList = new System.Collections.Generic.List<string>();
    System.Collections.Generic.List<string> MRNList = new System.Collections.Generic.List<string>();
    System.Collections.Generic.Dictionary<string, XPathNodeIterator> ERCMap = new System.Collections.Generic.Dictionary<string, XPathNodeIterator>();
    static System.Collections.Generic.Dictionary<string, string> dictionaryErrorCodeReason = new System.Collections.Generic.Dictionary<string, string>()
    {
      {"1", "Inventory Reporting Party is not a ECS participant"},
      {"2", "Ship to Party is not a ECS participant"},
      {"3", "Inventory Reporting Party is not active"},
      {"4", "Ship to Party is not active"},
      {"5", "Inventory Reporting Party is not a expeditor"},
      {"6", "Ship to Party is not a handler"},
      {"7", "Goods Reference Type incorrect"},
      {"8", "NEU goods identification incorrect"},
      {"9", "Customs document type incorrect"},
      {"10", "Customs declaration type incorrect"},
      {"16", "Number of packages (colli) is incorrect, this number can't be 0."},
      {"17", "Weight gross is incorrect, this can't be 0."},
      {"18", "MRN reference is invalid, the algorithm isn't met"},
      {"19", "Date/time of the transaction is bigger than the date/time reception of the message"},
      {"20", "Invalid date/time"},
      {"21", "Goods reference number is not available"},
      {"22", "Information Goods reference number incomplete"},
      {"23", "Unknown in ECS (no announcement done)"},
      {"24", "Transaction date has been more the 7 days"},
      {"25", "MRN reference is no transit MRN reference (position 11 not equals 1)"},
      {"26", "MRN reference is no export MRN reference (position 11 not equals 2)"},
      {"27", "Reporting Party is not authorized to send an Exit Notification Light (765)"},
      {"28", "Weight net is incorrect, this can't be 0."}
    };

    public string GetMessageReference(string input)
    {
      var pattern = @"RFF\+ACD:(.*?)(\:|\')";
      return GetRegexValue(input, pattern);
    }

    public string GetUNBReference(string input)
    {
      var pattern = @"UNB\+UNOC:(.*?)(\:|\')";
      var value = GetRegexValue(input, pattern);
      var headers = value.Split('+');
      return  headers.Length > 2 ? headers[2] : string.Empty;
    }

    public string GetMessageDateTime(string input)
    {
      var pattern = @"DTM\+184:(.*?):203\'";
      return GetRegexValue(input, pattern);
    }

    public string GetMAWBNumber(string input)
    {
      var pattern = @"RFF\+AWB:(.*?)(\:|\')";
      return GetRegexValue(input, pattern);
    }

    public string GetUNHInterchangeNumber(string input)
    {
      var pattern = @"UNH\+(.*?)\+";
      return GetRegexValue(input, pattern);
    }

    public int GetLINSegmentCount(string input)
    {
      var LINpattern = @"LIN\+(.*?)(\:|\')";
      var HAWBNumberPattern = @"RFF\+HWB:(.*?)(\:|\')";
      var MRNPattern = @"VIA\+M:821:(.*?)(\:|\')";
      var ERCPattern = @"ERC\+(.*?)(\:|\')";

      var indexList = new System.Collections.Generic.List<int>();
      foreach (Match match in Regex.Matches(input, LINpattern))
      {
          if (match.Success)
          {
              indexList.Add(match.Index);
          }
      }

      indexList.Add(input.Length);

      for (int i = 1; i < indexList.Count; i++)
      {
          var LINSegment = input.Substring(indexList[i - 1], indexList[i] - indexList[i - 1]);
          HAWBNumberList.Add(GetRegexValue(LINSegment, HAWBNumberPattern));
          MRNList.Add(GetRegexValue(LINSegment, MRNPattern));
          ERCMap.Add(i.ToString(), GetMessageErrors(LINSegment, ERCPattern));
      }

      return HAWBNumberList.Count;
    }

    int ercCount = 0;
    public XPathNodeIterator GetMessageErrors(string input, string pattern)
    {
      var matchCollectionErrorCode = Regex.Matches(input, pattern);
      XmlDocument doc = new XmlDocument();
      var errorCollection = doc.CreateElement("ErrorCollection");
      doc.AppendChild(errorCollection);
      for (var i = 0; i < matchCollectionErrorCode.Count; i++)
      {
        var errorCode = matchCollectionErrorCode[i].Groups[1].Value;
        var errorNode = doc.CreateElement("Error");
        var errorReason = doc.CreateElement("ErrorReason");
        if (!string.IsNullOrEmpty(errorCode))
        {
          errorReason.InnerText = dictionaryErrorCodeReason.ContainsKey(errorCode) ? dictionaryErrorCodeReason[errorCode] : "Unknown error code " + errorCode;
          ercCount++;
        }
        errorNode.AppendChild(errorReason);
        errorCollection.AppendChild(errorNode);
      }
      return doc.DocumentElement.CreateNavigator().Select("Error");
    }

    public string GetHAWBNumberByIndex(int i)
    {
      if (i >= 0 && i < HAWBNumberList.Count)
      {
          return HAWBNumberList[i];
      }
      return string.Empty;
    }

    public string GetMRNByindex(int i)
    {
     if (i >= 0 && i < MRNList.Count)
     {
       return MRNList[i];
     }
     return string.Empty;
    }

    public XPathNodeIterator GetERCByLINIndex(string i)
    {
      return ERCMap.ContainsKey(i) ? ERCMap[i] : null;
    }

    public string GetFirstERCError()
    {
      System.Collections.Generic.List<XPathNodeIterator> errors = new System.Collections.Generic.List<XPathNodeIterator>(ERCMap.Values);
      foreach(XPathNodeIterator error in errors)
      {
        foreach (XPathNavigator errorNode in error)
        {
          var text = errorNode.SelectSingleNode("ErrorReason");
          if (text != null && !string.IsNullOrEmpty(text.Value))
          {
            return text.Value;
          }
        }
      }
      return string.Empty;
    }

    public int GetERCCount()
    {
      return ercCount;
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

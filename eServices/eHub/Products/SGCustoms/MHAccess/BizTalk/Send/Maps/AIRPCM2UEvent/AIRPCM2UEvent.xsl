<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 ScriptNS0 ScriptNS1 ScriptNS3 ScriptNS4 ScriptNS6 userCSharp" version="1.0"
                xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2012/11"
                xmlns:s0="http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"
                xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
                xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1"
                xmlns:ScriptNS3="http://schemas.microsoft.com/BizTalk/2003/ScriptNS3"
                xmlns:ScriptNS4="http://schemas.microsoft.com/BizTalk/2003/ScriptNS4"
                xmlns:ScriptNS6="http://schemas.microsoft.com/BizTalk/2003/ScriptNS6"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />

  <xsl:variable name="Root" select="/s0:EFACT_31_AIRPCM"/>
  <xsl:variable name="SenderID" select="ScriptNS3:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="RecipientID" select="ScriptNS3:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="StatusString" select="ScriptNS3:GetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06')"/>
  <xsl:variable name="MessageReferenceID" select="$Root/UNH/UNH1/text()"/>
  <xsl:variable name="IDT1_1" select="$Root/s0:IDT/s0:IDT1/IDT1.1/text()"/>
  <xsl:variable name="IDT1_2" select="$Root/s0:IDT/s0:IDT1/IDT1.2/text()"/>
  <xsl:variable name="IDT1_3" select="$Root/s0:IDT/s0:IDT1/IDT1.3/text()"/>
  <xsl:variable name="IDT" select="concat($IDT1_1, $IDT1_2, $IDT1_3)"/>
  <xsl:variable name="MAWB" select="$Root/s0:REF/REF1/text()"/>

  <xsl:variable name="Subscription" select="ScriptNS6:InitializeSubscription()" />
  <xsl:variable name="ExistingSubscription" select="ScriptNS0:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID , '@recipientId', $RecipientID, '@ST_ID', 'SGCMSG', '@value', concat($MessageReferenceID,'I'))"/>
  <xsl:variable name="LoadOrGenerateSubscription">
    <xsl:choose>
      <xsl:when test="$ExistingSubscription = ''">
        <xsl:variable name="Throw" select="userCSharp:Throw(concat('No subscription found for ', $MessageReferenceID, 'I'))"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="ScriptNS6:LoadSubscription($Subscription, $ExistingSubscription)"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="SubscribeHistoryOrder">
    <xsl:choose>
      <xsl:when test="$StatusString = 'ACKSuccess'">
        <xsl:value-of select="ScriptNS6:SubscribeHistoryOrder($Subscription, 'Acknowledgement', ScriptNS1:CurrentDateTimeUTC('s'), $IDT1_1, $IDT1_2, $IDT1_3)" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="ScriptNS6:SubscribeHistoryOrder($Subscription, 'PCMError', ScriptNS1:CurrentDateTimeUTC('s'), $IDT1_1, $IDT1_2, $IDT1_3)" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:template match="/">
    <xsl:apply-templates select="s0:EFACT_31_AIRPCM" />
    <xsl:variable name="InsertSubscription" select="ScriptNS4:InsertSubscriptionValue('SGCMSG', $SenderID, $RecipientID, concat($MessageReferenceID,'I'), ScriptNS6:ToString($Subscription))"/>
  </xsl:template>

  <xsl:template match="s0:EFACT_31_AIRPCM">
    <xsl:call-template name="ReCreateOriginalStructure"/>

    <xsl:if test="not($StatusString = '')">
      <xsl:variable name="FileName" select="ScriptNS3:SetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06', '')"/>
    </xsl:if>
    <ns0:UniversalEvent>
      <ns0:Event>
        <ns0:DataContext>
          <ns0:Workflow>
            <xsl:choose>
              <xsl:when test="$StatusString = 'ACKSuccess'">
                <ns0:ActionPurpose Description="SG ACCESS ACKNOWLEDGEMENT">ACK</ns0:ActionPurpose>
              </xsl:when>
              <xsl:otherwise>
                <ns0:ActionPurpose Description="AIRERR SG ACCESS">ERR</ns0:ActionPurpose>
              </xsl:otherwise>
            </xsl:choose>
          </ns0:Workflow>
          <ns0:DataSource>
            <ns0:DataProvider>SGA</ns0:DataProvider>
          </ns0:DataSource>
          <ns0:DataTargetCollection>
            <ns0:DataTarget>
              <ns0:Key>
                <xsl:value-of select="userCSharp:GetManifestNumber()"/>
              </ns0:Key>
              <ns0:Type>AsycudaManifest</ns0:Type>
            </ns0:DataTarget>
          </ns0:DataTargetCollection>
        </ns0:DataContext>
        <ns0:EventTime>
          <xsl:value-of select="ScriptNS1:CurrentDateTimeUTC('s')"/>
        </ns0:EventTime>
        <ns0:EventType>
          <xsl:choose>
            <xsl:when test="$StatusString = 'ACKSuccess'">MDL</xsl:when>
            <xsl:otherwise>MRJ</xsl:otherwise>
          </xsl:choose>
        </ns0:EventType>
        <ns0:EventReference>|MSB=ORG|MST=MGI</ns0:EventReference>
        <ns0:IsEstimate>false</ns0:IsEstimate>
        <ns0:ContextCollection>
          <ns0:Context>
            <ns0:Type>ManifestCountry</ns0:Type>
            <ns0:Value>SG</ns0:Value>
          </ns0:Context>
          <ns0:Context>
            <ns0:Type>MessageType</ns0:Type>
            <ns0:Value>AIRPCM</ns0:Value>
          </ns0:Context>
          <ns0:Context>
            <ns0:Type>UENNumber</ns0:Type>
            <ns0:Value>
              <xsl:value-of select="s0:IDT/s0:IDT1/IDT1.1/text()"/>
            </ns0:Value>
          </ns0:Context>
          <ns0:Context>
            <ns0:Type>OriginalCreateDate</ns0:Type>
            <ns0:Value>
              <xsl:value-of select="s0:IDT/s0:IDT1/IDT1.2/text()"/>
            </ns0:Value>
          </ns0:Context>
          <ns0:Context>
            <ns0:Type>OriginalSerialNumber</ns0:Type>
            <ns0:Value>
              <xsl:value-of select="s0:IDT/s0:IDT1/IDT1.3/text()"/>
            </ns0:Value>
          </ns0:Context>
          <ns0:Context>
            <ns0:Type>MasterBill</ns0:Type>
            <ns0:Value>
              <xsl:value-of select="userCSharp:GetMAWB()"/>
            </ns0:Value>
            <ns0:SubContextCollection>
              <xsl:call-template name="LoopSubShipmentContexts">
                <xsl:with-param name="HasNext" select="userCSharp:MoveSubShipmentContextEnumerator()"/>
              </xsl:call-template>
            </ns0:SubContextCollection>
          </ns0:Context>
        </ns0:ContextCollection>
      </ns0:Event>
    </ns0:UniversalEvent>
  </xsl:template>

  <xsl:template name="ReCreateOriginalStructure">

    <xsl:variable name="insertShipmentContext" select="userCSharp:AddShipment($IDT, $MAWB, $MessageReferenceID)"/>
    <xsl:for-each select="s0:SGLoop1">
      <xsl:variable name="HAWB" select="s0:RFF/RFF1/text()"/>
      <xsl:variable name="insertSubShipmentContext" select="userCSharp:AddSubShipment($HAWB)"/>
      <xsl:for-each select="s0:SGLoop2">
        <xsl:variable name="CST" select="s0:CST/CST1/text()"/>
        <xsl:variable name="ConsignmentReference" select="ScriptNS6:SelectPackLineBySGID($Subscription, $HAWB, $CST)/@Ref" />
        <xsl:if test="$StatusString != 'ACKSuccess'">
          <xsl:variable name="SubscribeEvent" select="ScriptNS6:SubscribeEvent($Subscription, $HAWB, $ConsignmentReference, 'PCMError', $IDT, 'PCMError')"/>
          <xsl:variable name="SubscribeErrorCode" select="ScriptNS6:SubscribeInfo($Subscription, $HAWB, $ConsignmentReference, $IDT, 'PCMError', 'ErrorCode', userCSharp:GetErrorCode($StatusString))"/>
          <xsl:variable name="SubscribeErrorMessage" select="ScriptNS6:SubscribeInfo($Subscription, $HAWB, $ConsignmentReference, $IDT, 'PCMError', 'ErrorMessage', userCSharp:GetErrorDescription($StatusString))"/>
        </xsl:if>
        <xsl:variable name="insertPackingLineContext" select="userCSharp:AddPackingLine($HAWB, $CST, $ConsignmentReference)"/>
      </xsl:for-each>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="LoopSubShipmentContexts">
    <xsl:param name="HasNext"/>

    <xsl:if test="$HasNext=true()">

      <ns0:SubContext>
        <ns0:Type>HouseBill</ns0:Type>
        <ns0:Value>
          <xsl:value-of select="userCSharp:GetHAWB()"/>
        </ns0:Value>
        <ns0:SubContextCollection>
          <xsl:variable name="ResetPackingLineEnumerator" select="userCSharp:ResetPackingLineContextEnumerator()"/>
          <xsl:call-template name="LoopPackingLineContexts">
            <xsl:with-param name="HasNext" select="userCSharp:MovePackingLineContextEnumerator()"/>
          </xsl:call-template>
        </ns0:SubContextCollection>
      </ns0:SubContext>

      <xsl:call-template name="LoopSubShipmentContexts">
        <xsl:with-param name="HasNext" select="userCSharp:MoveSubShipmentContextEnumerator()" />
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name="LoopPackingLineContexts">
    <xsl:param name="HasNext"/>

    <xsl:if test="$HasNext=true()">

      <ns0:SubContext>
        <ns0:Type>ConsignmentReference</ns0:Type>
        <ns0:Value>
          <xsl:value-of select="userCSharp:GetConsignmentReference()"/>
        </ns0:Value>
        <ns0:SubContextCollection>
          <xsl:choose>
            <xsl:when test="$StatusString = 'ACKSuccess'">
              <ns0:SubContext>
                <ns0:Type>ConsignmentNumber</ns0:Type>
                <ns0:Value>
                  <xsl:value-of select="userCSharp:GetCST()"/>
                </ns0:Value>
              </ns0:SubContext>
              <ns0:SubContext>
                <ns0:Type>MessageStatusCode</ns0:Type>
                <ns0:Value>ACK</ns0:Value>
              </ns0:SubContext>
            </xsl:when>
            <xsl:otherwise>
              <ns0:SubContext>
                <ns0:Type>MessageStatusCode</ns0:Type>
                <ns0:Value>ERR</ns0:Value>
              </ns0:SubContext>
              <ns0:SubContext>
                <ns0:Type>ConsignmentNumber</ns0:Type>
                <ns0:Value>
                  <xsl:value-of select="userCSharp:GetCST()"/>
                </ns0:Value>
              </ns0:SubContext>
              <ns0:SubContext>
                <ns0:Type>ErrorCode</ns0:Type>
                <ns0:Value>
                  <xsl:value-of select="userCSharp:GetErrorCode($StatusString)"/>
                </ns0:Value>
              </ns0:SubContext>
              <ns0:SubContext>
                <ns0:Type>ErrorDescription</ns0:Type>
                <ns0:Value>
                  <xsl:value-of select="userCSharp:GetErrorDescription($StatusString)"/>
                </ns0:Value>
              </ns0:SubContext>
            </xsl:otherwise>
          </xsl:choose>
        </ns0:SubContextCollection>
      </ns0:SubContext>

      <xsl:call-template name="LoopPackingLineContexts">
        <xsl:with-param name="HasNext" select="userCSharp:MovePackingLineContextEnumerator()" />
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[
    public ShipmentContext shipmentContext = new ShipmentContext();

    public class ShipmentContext
    {
      public System.Collections.Generic.Dictionary<string, SubShipmentContext> SubShipmentContexts = new System.Collections.Generic.Dictionary<string, SubShipmentContext>();
      public string IDT { get; set; }
      public string MAWB { get; set; }
      public string ManifestNumber { get; set; }
    }

    public class SubShipmentContext
    {
      public System.Collections.Generic.List<PackingLineContext> PackingLineContexts = new System.Collections.Generic.List<PackingLineContext>();
      public string HAWB { get; set; }
    }

    public class PackingLineContext
    {
      public string CST { get; set; }
      public string ConsignmentReference { get; set; }
    }

    public void AddShipment(string idt, string mawb, string manifestNumber)
    {
      shipmentContext = new ShipmentContext()
      {
        IDT = idt,
        MAWB = mawb,
        ManifestNumber = manifestNumber
      };
    }

    public void AddSubShipment(string hawb)
    {
      if (!shipmentContext.SubShipmentContexts.ContainsKey(hawb))
      {
        shipmentContext.SubShipmentContexts.Add(hawb, new SubShipmentContext() { HAWB = hawb });
      }
    }

    public void AddPackingLine(string hawb, string cst, string consignmentReference)
    {
      shipmentContext.SubShipmentContexts[hawb].PackingLineContexts.Add(new PackingLineContext() { CST = cst, ConsignmentReference = consignmentReference });
    }

    public string GetMAWB()
    {
      return shipmentContext.MAWB;
    }

    public string GetManifestNumber()
    {
      return shipmentContext.ManifestNumber;
    }

    public System.Collections.Generic.IEnumerator<SubShipmentContext> SubShipmentContextEnumerator;

    public bool MoveSubShipmentContextEnumerator()
    {
      if (SubShipmentContextEnumerator == null)
      {
        SubShipmentContextEnumerator = shipmentContext.SubShipmentContexts.Values.GetEnumerator();
      }
      return SubShipmentContextEnumerator.MoveNext();
    }

    public string GetHAWB()
    {
      return SubShipmentContextEnumerator.Current.HAWB;
    }

    public System.Collections.Generic.IEnumerator<PackingLineContext> PackingLineContextEnumerator;

    public bool MovePackingLineContextEnumerator()
    {
      if (PackingLineContextEnumerator == null)
      {
        ResetPackingLineContextEnumerator();
      }
      return PackingLineContextEnumerator.MoveNext();
    }

    public void ResetPackingLineContextEnumerator()
    {
      PackingLineContextEnumerator = SubShipmentContextEnumerator.Current.PackingLineContexts.GetEnumerator();
    }

    public string GetCST()
    {
      return PackingLineContextEnumerator.Current.CST;
    }

    public string GetConsignmentReference()
    {
      return PackingLineContextEnumerator.Current.ConsignmentReference;
    }

    public string GetErrorCode(string errorString)
    {
      return errorString.Split(';')[0].Split(':')[1].Trim();
    }

    public string GetErrorDescription(string errorString)
    {
      var index = errorString.IndexOf(';');
      if (index <= 0 || index >= errorString.Length - 1) return string.Empty;
      return errorString.Substring(index + 1).Trim();
    }

    public void Throw(string message)
    {
      throw new ArgumentException(message);
    }
    ]]>
  </msxsl:script>
</xsl:stylesheet>

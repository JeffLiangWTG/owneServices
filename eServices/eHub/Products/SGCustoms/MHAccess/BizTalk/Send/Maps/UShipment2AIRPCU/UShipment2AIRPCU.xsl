<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 ScriptNS0 ScriptNS1 ScriptNS3 ScriptNS4 ScriptNS5 ScriptNS6 userCSharp" version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:ns0="http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"
                xmlns:ns1="http://wisetechglobal.com/eHub/Products/SGCustoms/MHAccess/2017/08"
                xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
                xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1"
                xmlns:ScriptNS3="http://schemas.microsoft.com/BizTalk/2003/ScriptNS3"
                xmlns:ScriptNS4="http://schemas.microsoft.com/BizTalk/2003/ScriptNS4"
                xmlns:ScriptNS5="http://schemas.microsoft.com/BizTalk/2003/ScriptNS5"
                xmlns:ScriptNS6="http://schemas.microsoft.com/BizTalk/2003/ScriptNS6"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes" />

  <xsl:variable name="Root" select="/s0:UniversalShipment/s0:Shipment"/>
  <xsl:variable name="SenderID" select="ScriptNS3:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="RecipientID" select="ScriptNS3:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="MessageReferenceID" select="$Root/s0:DataContext/s0:DataSourceCollection/s0:DataSource[1]/s0:Key/text()" />
  <xsl:variable name="StaffID" select="$Root/s0:CustomsBroker/s0:Code/text()" />
  <xsl:variable name="Account" select="ScriptNS5:GetSGCustomsAccount($StaffID, $SenderID)" />
  <xsl:variable name="isAccountValid" select="ScriptNS5:IsAccountValid($StaffID, $SenderID, $Account)" />
  <xsl:variable name="FileName" select="ScriptNS3:SetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06', $Account)"/>
  <xsl:variable name="SGCID" select="ScriptNS5:GetSGCustomsSenderID($Account)" />
  <xsl:variable name="IDT1_1">
    <xsl:variable name="HasUENGovRegNum" select="$Root/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='Branch']/s0:GovRegNumType[s0:Code/text()='UEN']/s0:Code/text()"/>
    <xsl:choose>
      <xsl:when test="$HasUENGovRegNum='UEN'">
        <xsl:value-of select="$Root/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='Branch']/s0:GovRegNum/text()"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$Root/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='Branch']/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text()='UEN']/s0:Value/text()"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="IDT1_2" select="ScriptNS1:ConvertXmlDateString(ScriptNS1:ConvertUTCToLocalTimeByUNLOCO(ScriptNS1:CurrentDateTimeUTC('s'), 'SGSIN'), 'yyyyMMdd')"/>
  <xsl:variable name="MAWB" select="$Root/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ManifestNumber']/s0:Value/text()" />

  <xsl:variable name="Subscription" select="ScriptNS6:InitializeSubscription()" />
  <xsl:variable name="ExistingSubscription" select="ScriptNS0:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $RecipientID , '@recipientId', $SenderID, '@ST_ID', 'SGCMSG', '@value', concat($MessageReferenceID,'I'))"/>
  <xsl:variable name="LoadSubscriptionOrThrow">
    <xsl:choose>
      <xsl:when test="$ExistingSubscription != ''">
        <xsl:value-of select="ScriptNS6:LoadSubscription($Subscription, $ExistingSubscription)"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:variable name="Throw" select="userCSharp:Throw(concat('No subscription found for ', $MessageReferenceID, 'I'))"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:template match="/">
    <xsl:apply-templates select="s0:UniversalShipment/s0:Shipment" />
  </xsl:template>

  <xsl:template match="s0:UniversalShipment/s0:Shipment">
    <ns1:AIRPCUEnvelope>
      <xsl:call-template name="Generate_SubShipments"/>

      <xsl:call-template name="LoopMessageContexts">
        <xsl:with-param name="HasNext" select="userCSharp:HasNextMessageContext()" />
      </xsl:call-template>
    </ns1:AIRPCUEnvelope>

    <xsl:variable name="InsertSubscription" select="ScriptNS4:InsertSubscriptionValue('SGCMSG', $RecipientID, $SenderID, concat($MessageReferenceID,'I'), ScriptNS6:ToString($Subscription))"/>
  </xsl:template>

  <xsl:template name="Generate_SubShipments">
    <xsl:for-each select="s0:SubShipmentCollection/s0:SubShipment">
      <xsl:variable name ="TotalPackLines" select ="count(s0:PackingLineCollection/s0:PackingLine)"/>
      <xsl:variable name ="TotalPackLoops" select="ceiling($TotalPackLines div 50)"/>
      <xsl:call-template name="LoopSG1">
        <xsl:with-param name="Until" select="$TotalPackLoops" />
        <xsl:with-param name="Position" select="position()" />
        <xsl:with-param name="SubShipment" select="." />
      </xsl:call-template>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="LoopSG1">
    <xsl:param name="Next" select="1" />
    <xsl:param name="Until" />
    <xsl:param name="Position" />
    <xsl:param name="SubShipment" />

    <xsl:variable name="HAWB" select="$SubShipment/s0:WayBillNumber/text()"/>

    <xsl:variable name="SubscribedIDT">
      <xsl:variable name="OriginalIDT" select="ScriptNS6:SelectLatestIDTWhichDeclaredHAWB($Subscription, $HAWB)"/>
      <xsl:value-of select="concat($OriginalIDT/IDT1, '|', $OriginalIDT/IDT2, '|', $OriginalIDT/IDT3)"/>
    </xsl:variable>

    <xsl:variable name="InsertSubShipmentContext" select="userCSharp:AddContext($Position, $Next, $Until, $SubscribedIDT, $MAWB, $HAWB, generate-id($SubShipment))"/>

    <xsl:if test="$Until > $Next">
      <xsl:call-template name="LoopSG1">
        <xsl:with-param name="Next" select="$Next + 1" />
        <xsl:with-param name="Until" select="$Until" />
        <xsl:with-param name="Position" select="$Position"/>
        <xsl:with-param name="SubShipment" select="$SubShipment"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name="LoopMessageContexts">
    <xsl:param name="HasNext" />

    <xsl:if test="$HasNext = true()">
      <xsl:variable name="TotalSubShipmentsForCurrentIDT" select="userCSharp:GetTotalSubShipmentsForCurrentIDT()" />
      <xsl:variable name="TotalMessagesForCurrentIDT" select="ceiling($TotalSubShipmentsForCurrentIDT div 100)" />
      <xsl:call-template name="GenerateMessagesPerIDT">
        <xsl:with-param name="TotalMessages" select="$TotalMessagesForCurrentIDT" />
        <xsl:with-param name="TotalSubShipments" select="$TotalSubShipmentsForCurrentIDT" />
      </xsl:call-template>
      <xsl:call-template name="LoopMessageContexts">
        <xsl:with-param name="HasNext" select="userCSharp:HasNextMessageContext()" />
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name="GenerateMessagesPerIDT">
    <xsl:param name="TotalMessages"/>
    <xsl:param name="TotalSubShipments"/>
    <xsl:param name="CurrentMessage" select="1"/>

    <xsl:call-template name="Generate_Message">
      <xsl:with-param name="CurrentMessage" select="$CurrentMessage" />
      <xsl:with-param name="TotalSubShipments" select="$TotalSubShipments" />
    </xsl:call-template>

    <xsl:if test="$TotalMessages > $CurrentMessage">
      <xsl:call-template name="GenerateMessagesPerIDT">
        <xsl:with-param name="TotalMessages" select="$TotalMessages"/>
        <xsl:with-param name="TotalSubShipments" select="$TotalSubShipments"/>
        <xsl:with-param name="CurrentMessage" select="$CurrentMessage + 1"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name="Generate_Message">
    <xsl:param name="CurrentMessage" />
    <xsl:param name="TotalSubShipments" />

    <xsl:variable name="IDT1_3" select="format-number(ScriptNS0:CallActionProcedureHelper('GetCounterValue','@value','@name','CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.IDT','@maxlength','4'), '0000')" />
    <xsl:if test="$isAccountValid = 'true'">
      <xsl:variable name="SubscribeManifestNumber" select="ScriptNS4:InsertSubscriptionValue('SGCMSG', $RecipientID, $SenderID, concat($IDT1_1, $IDT1_2, $IDT1_3,'I'), $MessageReferenceID, 'ManifestNumber')"/>
    </xsl:if>
    <xsl:variable name="SetCurrentIDT" select="userCSharp:SetCurrentIDT(concat($IDT1_1, $IDT1_2, $IDT1_3))"/>
    <xsl:variable name="SubscribedCycleDate" select="ScriptNS6:SelectInfoByIDT($Subscription, translate(userCSharp:GetIDT(), '|', ''), 'PCM', 'CycleDate')" />
    <xsl:variable name="SubscribedCycleNumber" select="ScriptNS6:SelectInfoByIDT($Subscription, translate(userCSharp:GetIDT(), '|', ''), 'PCM', 'CycleNumber')" />
    <xsl:variable name="SubscribeHistoryOrder" select="ScriptNS6:SubscribeHistoryOrder($Subscription, 'PCU', ScriptNS1:CurrentDateTimeUTC('s'), $IDT1_1, $IDT1_2, $IDT1_3)" />

    <xsl:variable name="IDT" select="concat($IDT1_1, $IDT1_2, $IDT1_3)" />

    <ns0:EFACT_31_AIRPCU>
      <UNH>
        <UNH1>
          <xsl:value-of select="$MessageReferenceID" />
        </UNH1>
        <UNH2>
          <UNH2.1>AIRPCU</UNH2.1>
          <UNH2.2>3</UNH2.2>
          <UNH2.3>1</UNH2.3>
        </UNH2>
      </UNH>
      <ns0:IDT>
        <ns0:IDT1>
          <IDT1.1>
            <xsl:value-of select="$IDT1_1"/>
          </IDT1.1>
          <IDT1.2>
            <xsl:value-of select="$IDT1_2"/>
          </IDT1.2>
          <IDT1.3>
            <xsl:value-of select="$IDT1_3"/>
          </IDT1.3>
        </ns0:IDT1>
        <ns0:IDT2>
          <IDT2.1>
            <xsl:value-of select="$SGCID"/>
          </IDT2.1>
        </ns0:IDT2>
      </ns0:IDT>
      <ns0:DTM>
        <DTM1>
          <xsl:value-of select="$SubscribedCycleDate"/>
        </DTM1>
        <DTM2>
          <xsl:value-of select="$SubscribedCycleNumber"/>
        </DTM2>
        <DTM3>
          <xsl:choose>
            <xsl:when test="$Root/s0:TransportMode/s0:Code/text()='ROA'">
              <xsl:value-of select="'ROAD'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$Root/s0:VoyageFlightNo/text()"/>
            </xsl:otherwise>
          </xsl:choose>
        </DTM3>
        <DTM4>
          <xsl:value-of select="ScriptNS1:ConvertXmlDateString($Root/s0:DateCollection/s0:Date[s0:Type/text()='Arrival']/s0:Value/text(), 'yyyyMMdd')"/>
        </DTM4>
      </ns0:DTM>
      <ns0:FTX>
        <ns0:FTX1>
          <FTX1.1>
            <xsl:value-of select="userCSharp:GetIDTSection(0)"/>
          </FTX1.1>
          <FTX1.2>
            <xsl:value-of select="userCSharp:GetIDTSection(1)"/>
          </FTX1.2>
          <FTX1.3>
            <xsl:value-of select="userCSharp:GetIDTSection(2)"/>
          </FTX1.3>
        </ns0:FTX1>
      </ns0:FTX>
      <ns0:REF>
        <REF1>
          <xsl:value-of select="$MAWB"/>
        </REF1>
      </ns0:REF>
      <xsl:call-template name="LoopSubShipmentContexts">
        <xsl:with-param name="HasNext" select="userCSharp:HasNextSubShipmentContext()" />
        <xsl:with-param name="CurrentMessage" select="$CurrentMessage"/>
        <xsl:with-param name="TotalSubShipments" select="$TotalSubShipments"/>
      </xsl:call-template>
    </ns0:EFACT_31_AIRPCU>
  </xsl:template>

  <xsl:template name="LoopSubShipmentContexts">
    <xsl:param name="HasNext" />
    <xsl:param name="CurrentMessage" />
    <xsl:param name="TotalSubShipments" />

    <xsl:variable name="LastSubShipment" select="userCSharp:Min($TotalSubShipments, $CurrentMessage * 100)" />
    <xsl:if test="$HasNext = true()">
      <xsl:variable name="ID" select="userCSharp:GetNextID()" />
      <xsl:variable name="Position" select="userCSharp:GetNextPosition()" />
      <xsl:variable name="RealPosition" select="userCSharp:GetNextRealPosition()" />
      <xsl:variable name="IDT" select="userCSharp:GetNextIDT()" />
      <xsl:variable name="Repeat" select="userCSharp:GetNextRepeat()" />
      <xsl:variable name="HAWB" select="userCSharp:GetNextHAWB()" />
      <xsl:variable name="SubShipment" select="$Root/s0:SubShipmentCollection/s0:SubShipment[position() = $RealPosition]" />
      <xsl:variable name="firstPackingLine" select="50 * ($Repeat - 1) + 1" />
      <xsl:variable name="lastPackingLine" select="50 * $Repeat" />

      <xsl:if test="$LastSubShipment >= $Position">
        <ns0:SGLoop1>
          <ns0:RFF>
            <RFF1>
              <xsl:value-of select="$HAWB"/>
            </RFF1>
          </ns0:RFF>
          <xsl:for-each select="$SubShipment/s0:PackingLineCollection/s0:PackingLine[position() >= $firstPackingLine and $lastPackingLine >= position()]">
            <ns0:SGLoop2>
              <ns0:CST>
                <CST1>
                  <xsl:variable name="ConsignmentReference" select="s0:AddInfoGroupCollection/s0:AddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'ConsignmentReference']/s0:Value/text()" />
                  <xsl:variable name="SubscribeStatus">
                    <xsl:value-of select="ScriptNS6:SubscribeEvent($Subscription, $HAWB, $ConsignmentReference, 'Cancelled', userCSharp:GetCurrentIDT(), 'PCU')"/>
                  </xsl:variable>
                  <xsl:value-of select="ScriptNS6:SelectSGIDByRef($Subscription, $HAWB, $ConsignmentReference)"/>
                </CST1>
              </ns0:CST>
            </ns0:SGLoop2>
          </xsl:for-each>
        </ns0:SGLoop1>
      </xsl:if>

      <xsl:if test="$LastSubShipment > $Position">
        <xsl:call-template name="LoopSubShipmentContexts">
          <xsl:with-param name="HasNext" select="userCSharp:HasNextSubShipmentContext()" />
          <xsl:with-param name="CurrentMessage" select="$CurrentMessage"/>
          <xsl:with-param name="TotalSubShipments" select="$TotalSubShipments"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:if>
  </xsl:template>

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[
    public int position = 0;
    public string currentIDT = string.Empty;

    public string GetCurrentIDT()
    {
      return currentIDT;
    }

    public void SetCurrentIDT(string value)
    {
      currentIDT = value;
    }

    public class SubShipmentContext
    {
      public string Position { get; set; }
      public string RealPosition { get; set; }
      public string Repeat { get; set; }
      public string TotalPackLoops { get; set; }
      public string IDT { get; set; }
      public string MAWB { get; set; }
      public string HAWB { get; set; }
      public string ID { get; set; }
    }

    public class MessageContext
    {
      public System.Collections.Generic.List<SubShipmentContext> Contexts = new System.Collections.Generic.List<SubShipmentContext>();
    }

    public System.Collections.Generic.Dictionary<string, MessageContext> contextsGroupedByIDT = new System.Collections.Generic.Dictionary<string, MessageContext>();

    public int GetTotalSubShipmentsForCurrentIDT()
    {
      return contextsGroupedByIDT[messageContextEnumerator.Current.Key].Contexts.Count;
    }

    public string GetIDT()
    {
      return messageContextEnumerator.Current.Key;
    }

    public string GetIDTSection(int index)
    {
      return messageContextEnumerator.Current.Key.Split('|')[index];
    }

    public void AddContext(string RealPosition, string Repeat, string TotalPackLoops, string IDT, string MAWB, string HAWB, string ID)
    {
      var context = new SubShipmentContext()
      {
        RealPosition = RealPosition,
        Repeat = Repeat,
        TotalPackLoops = TotalPackLoops,
        IDT = IDT,
        MAWB = MAWB,
        HAWB = HAWB,
        ID = ID
      };

      if (contextsGroupedByIDT.ContainsKey(context.IDT))
      {
        context.Position = (contextsGroupedByIDT[context.IDT].Contexts.Count + 1).ToString();
        contextsGroupedByIDT[context.IDT].Contexts.Add(context);
      }
      else
      {
        var messageContext = new MessageContext();
        context.Position = "1";
        messageContext.Contexts.Add(context);
        contextsGroupedByIDT.Add(context.IDT, messageContext);
      }
    }

    public System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<string, MessageContext>> messageContextEnumerator;
    public System.Collections.Generic.IEnumerator<SubShipmentContext> subShipmentContextEnumerator;

    public bool HasNextMessageContext()
    {
      if (messageContextEnumerator == null) messageContextEnumerator = contextsGroupedByIDT.GetEnumerator();
      var next = messageContextEnumerator.MoveNext();
      if (next)
      {
        ResetSubShipmentContextEnumerator();
      }
      return next;
    }

    public System.Collections.Generic.List<SubShipmentContext> GetNextMessageContext()
    {
      return messageContextEnumerator.Current.Value.Contexts;
    }

    public bool HasNextSubShipmentContext()
    {
      if (subShipmentContextEnumerator == null) subShipmentContextEnumerator = GetNextMessageContext().GetEnumerator();
      return subShipmentContextEnumerator.MoveNext();
    }

    public void ResetSubShipmentContextEnumerator()
    {
      subShipmentContextEnumerator = GetNextMessageContext().GetEnumerator();
    }

    public SubShipmentContext GetNextSubShipmentContext()
    {
      return subShipmentContextEnumerator.Current;
    }

    public string GetNextID()
    {
      return GetNextSubShipmentContext().ID;
    }

    public string GetNextPosition()
    {
      return GetNextSubShipmentContext().Position;
    }

    public string GetNextRealPosition()
    {
      return GetNextSubShipmentContext().RealPosition;
    }

    public string GetNextIDT()
    {
      return GetNextSubShipmentContext().IDT;
    }

    public string GetNextRepeat()
    {
      return GetNextSubShipmentContext().Repeat;
    }

    public string GetNextHAWB()
    {
      return GetNextSubShipmentContext().HAWB;
    }

    public int Min(int num1, int num2)
    {
      return Math.Min(num1, num2);
    }

    public void Throw(string message)
    {
      throw new ArgumentException(message);
    }
    ]]>
  </msxsl:script>
</xsl:stylesheet>

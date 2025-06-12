<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 ScriptNS0 ScriptNS1 ScriptNS2 ScriptNS3 ScriptNS4 ScriptNS5 userCSharp" version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:ns0="http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"
                xmlns:ns1="http://wisetechglobal.com/eHub/Products/SGCustoms/MHAccess/2017/08"
                xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
                xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1"
                xmlns:ScriptNS2="http://schemas.microsoft.com/BizTalk/2003/ScriptNS2"
                xmlns:ScriptNS3="http://schemas.microsoft.com/BizTalk/2003/ScriptNS3"
                xmlns:ScriptNS4="http://schemas.microsoft.com/BizTalk/2003/ScriptNS4"
                xmlns:ScriptNS5="http://schemas.microsoft.com/BizTalk/2003/ScriptNS5"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />

  <xsl:variable name="RecipientID" select="ScriptNS3:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="SenderID" select="ScriptNS3:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="Root" select="/s0:UniversalShipment/s0:Shipment"/>
  <xsl:variable name="TotalSubShipments" select="count($Root/s0:SubShipmentCollection/s0:SubShipment)"/>
  <xsl:variable name="StaffID" select="$Root/s0:CustomsBroker/s0:Code/text()" />
  <xsl:variable name="Account" select="ScriptNS5:GetSGCustomsAccount($StaffID, $SenderID)" />
  <xsl:variable name="isAccountValid" select="ScriptNS5:IsAccountValid($StaffID, $SenderID, $Account)" />
  <xsl:variable name="FileName" select="ScriptNS3:SetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06', $Account)"/>
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
  <xsl:variable name="SGCID" select="ScriptNS5:GetSGCustomsSenderID($Account)" />
  <xsl:variable name="MessageReferenceID" select="$Root/s0:DataContext/s0:DataSourceCollection/s0:DataSource[1]/s0:Key/text()" />
  <xsl:variable name="MAWB" select="$Root/s0:AddInfoCollection/s0:AddInfo[s0:Key= 'ManifestNumber']/s0:Value/text()"/>

  <xsl:template match="/">
    <xsl:apply-templates select="s0:UniversalShipment/s0:Shipment" />
  </xsl:template>

  <xsl:template match="s0:UniversalShipment/s0:Shipment">
    <ns1:AIRAEDEnvelope>
      <xsl:call-template name="MessageLoop"/>
    </ns1:AIRAEDEnvelope>
  </xsl:template>

  <xsl:template name="MessageLoop">
    <xsl:if test="userCSharp:CheckSubShipmentCount() &lt;= $TotalSubShipments">
      <xsl:call-template name="GenerateMessage"/>
      <xsl:call-template name="MessageLoop"/>
    </xsl:if>
  </xsl:template>

  <xsl:template name="GenerateMessage">
    <xsl:variable name="IDT1_3" select="format-number(ScriptNS0:CallActionProcedureHelper('GetCounterValue','@value','@name','CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.IDT','@maxlength','4'), '0000')" />
    <xsl:variable name="IDT" select="concat($IDT1_1, $IDT1_2, $IDT1_3)" />
    <xsl:variable name="IDTPiped" select="concat($IDT1_1, '|', $IDT1_2, '|', $IDT1_3)" />

    <xsl:if test="$isAccountValid = 'true'">
      <xsl:variable name="subscribeUpdateNumberForAIRAEU" select="ScriptNS4:InsertSubscriptionValue('SGCMSG', $RecipientID, $SenderID, concat($IDTPiped, 'E'), '1', 'UpdateNumber')" />
      <xsl:variable name="SubscribeMessageReferenceIDForAIRAEP" select="ScriptNS4:InsertSubscriptionValue('SGCMSG', $RecipientID, $SenderID, concat($IDT, 'E'), $MessageReferenceID, 'ManifestNumber')" />
      <xsl:variable name="subscribeMAWBForAIRERR2UEvent" select="ScriptNS4:InsertSubscriptionValue('SGCMSG', $RecipientID, $SenderID, concat($IDT,'E'), $MAWB, 'MAWB')" />
    </xsl:if>

    <ns0:EFACT_31_AIRAED>
      <UNH>
        <UNH1>
          <xsl:value-of select="$MessageReferenceID" />
        </UNH1>
        <UNH2>
          <UNH2.1>AIRAED</UNH2.1>
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
            <xsl:value-of select="$IDT1_2" />
          </IDT1.2>
          <IDT1.3>
            <xsl:value-of select="$IDT1_3" />
          </IDT1.3>
        </ns0:IDT1>
        <ns0:IDT2>
          <IDT2.1>
            <xsl:value-of select="$SGCID" />
          </IDT2.1>
        </ns0:IDT2>
      </ns0:IDT>
      <ns0:PUR>
        <PUR1>E</PUR1>
      </ns0:PUR>
      <ns0:DTM>
        <DTM1>
          <xsl:value-of select="$IDT1_2" />
        </DTM1>
        <DTM2>
          <!--Batch Number filled by Orchestration/Links multiple AIRAED messages for same MAWB together-->
        </DTM2>
      </ns0:DTM>

      <xsl:call-template name="SG1Loop">
        <xsl:with-param name="SubShipmentCollection" select ="s0:SubShipmentCollection"/>
        <xsl:with-param name="IDTPiped" select="$IDTPiped"/>
      </xsl:call-template>

      <xsl:if test="$isAccountValid = 'true'">
        <xsl:variable name="HAWBData" select="userCSharp:GetHAWBData()"/>
        <xsl:variable name="SubscribeHAWBForAIRERR2Event" select="ScriptNS4:InsertSubscriptionValue('SGCMSG', $RecipientID, $SenderID, concat($IDT,'E'), $HAWBData, 'HAWB')" />
      </xsl:if>
      <xsl:variable name="HAWBClear" select="userCSharp:ClearHAWBData()"/>
      <xsl:variable name="ResetCSTCount" select="userCSharp:ResetCSTCount()"/>
    </ns0:EFACT_31_AIRAED>
  </xsl:template>

  <xsl:template name="SG1Loop">
    <xsl:param name="SubShipmentCollection"/>
    <xsl:param name="IDTPiped"/>

    <xsl:variable name="CurrentSubShipment" select="userCSharp:CheckSubShipmentCount()"/>
    <xsl:variable name ="CSTCount" select="userCSharp:CheckCSTCount()"/>
    <xsl:variable name="SubShipment" select="$SubShipmentCollection/s0:SubShipment[$CurrentSubShipment]"/>
    <xsl:variable name ="TotalPackLines" select ="count($SubShipment/s0:PackingLineCollection/s0:PackingLine)"/>
    <xsl:variable name="HAWB" select="$SubShipment/s0:WayBillNumber/text()"/>

    <xsl:variable name ="TotalPackLoops">
      <xsl:variable name="PackLoop" select ="$TotalPackLines div 50"/>
      <xsl:value-of select="ceiling($PackLoop)"/>
    </xsl:variable>

    <xsl:if test="($CSTCount + $TotalPackLoops) &lt;= 100 and $CurrentSubShipment &lt;= $TotalSubShipments" >
      <xsl:variable name="AddToHAWBData" select="userCSharp:AddToHAWBDATA($HAWB)"/>

      <xsl:call-template name="PackageLoops">
        <xsl:with-param name="TotalPackLoops" select="$TotalPackLoops"/>
        <xsl:with-param name ="Count" select="1"/>
        <xsl:with-param name ="SubShipment" select="$SubShipment"/>
        <xsl:with-param name="HAWB" select="$HAWB"/>
      </xsl:call-template>

      <xsl:if test="$isAccountValid = 'true'">
        <xsl:variable name="existingIDTs" select="ScriptNS0:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $RecipientID, '@recipientId', $SenderID, '@ST_ID', 'SGCMSG', '@value', concat($MAWB, $HAWB, $MessageReferenceID, 'E'), '@referenceType', 'IDT')" />
        <xsl:variable name="subscribeIDT">
          <xsl:choose>
            <xsl:when test="$existingIDTs = ''">
              <xsl:value-of select="ScriptNS4:InsertSubscriptionValue('SGCMSG', $RecipientID, $SenderID, concat($MAWB, $HAWB, $MessageReferenceID, 'E'), $IDTPiped, 'IDT')" />
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="ScriptNS4:InsertSubscriptionValue('SGCMSG', $RecipientID, $SenderID, concat($MAWB, $HAWB, $MessageReferenceID, 'E'), concat($existingIDTs, '^', $IDTPiped), 'IDT')" />
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <xsl:variable name="CSTData" select="userCSharp:GetCSTData()"/>
        <xsl:variable name="subscribeCST" select="ScriptNS4:InsertSubscriptionValue('SGCMSG', $RecipientID, $SenderID, concat($MAWB, $HAWB, $MessageReferenceID, $IDTPiped, 'E'), $CSTData, 'CST')" />
      </xsl:if>

      <xsl:variable name="CSTClear" select="userCSharp:ClearCSTData()"/>

      <xsl:variable name="IncrementSubShipment" select="userCSharp:IncrementSubShipment()"/>

      <xsl:call-template name="SG1Loop">
        <xsl:with-param name="SubShipmentCollection" select="$SubShipmentCollection"/>
        <xsl:with-param name="IDTPiped" select="$IDTPiped"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name="PackageLoops">
    <xsl:param name ="TotalPackLoops"/>
    <xsl:param name="Count"/>
    <xsl:param name="SubShipment"/>
    <xsl:param name="HAWB"/>

    <xsl:if test="$Count &lt;= $TotalPackLoops">
      <ns0:SG1Loop>
        <xsl:variable name="CSTValue" select="format-number(ScriptNS0:CallActionProcedureHelper('GetCounterValue','@value','@name','CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.Maps.AIRAED.CST','@maxlength','5'), '00000')" />
        <xsl:call-template name="CST-RFF">
          <xsl:with-param name="SubShipment" select="$SubShipment"/>
          <xsl:with-param name="CSTValue" select="$CSTValue"/>
        </xsl:call-template>
        <xsl:call-template name="SG2Loop">
          <xsl:with-param name="SubShipment" select="$SubShipment"/>
          <xsl:with-param name ="PackLoop" select="$Count"/>
          <xsl:with-param name="HAWB" select="$HAWB"/>
        </xsl:call-template>

        <xsl:variable name="CW1ReferenceData" select="userCSharp:GetCW1ReferenceData()"/>
        <xsl:variable name="subscribeCW1ReferenceData" select="ScriptNS4:InsertSubscriptionValue('SGCMSG', $RecipientID, $SenderID, concat($MAWB,$HAWB,$MessageReferenceID,$CSTValue,'E'), $CW1ReferenceData, 'CNRF')" />
        <xsl:variable name="CW1ReferenceClear" select="userCSharp:ClearCW1ReferenceData()"/>
      </ns0:SG1Loop>

      <xsl:call-template name="PackageLoops">
        <xsl:with-param name="TotalPackLoops" select="$TotalPackLoops"/>
        <xsl:with-param name="Count" select="$Count + 1"/>
        <xsl:with-param name="SubShipment" select="$SubShipment"/>
        <xsl:with-param name="HAWB" select="$HAWB"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name="CST-RFF">
    <xsl:param name="SubShipment"/>
    <xsl:param name="CSTValue"/>
    <ns0:CST>
      <CST1>
        <xsl:variable name="IncrementCST" select="userCSharp:IncrementCST()"/>
        <xsl:value-of select="$CSTValue"/>
        <xsl:variable name="AddToCSTData" select="userCSharp:AddToCSTDATA($CSTValue)"/>
      </CST1>
    </ns0:CST>
    <ns0:RFF>
      <RFF1>
        <xsl:value-of select ="$SubShipment/s0:WayBillNumber/text()"/>
      </RFF1>
    </ns0:RFF>
  </xsl:template>

  <xsl:template name="SG2Loop">
    <xsl:param name="SubShipment"/>
    <xsl:param name="PackLoop"/>
    <xsl:param name="HAWB"/>

    <ns0:SG2Loop>
      <ns0:REF>
        <REF1>
          <xsl:value-of select ="$MAWB"/>
        </REF1>
      </ns0:REF>
      <ns0:FLI>
        <FLI1>
          <xsl:choose>
            <xsl:when test="$Root/s0:TransportMode/s0:Code/text()='ROA'">ROAD</xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$Root/s0:VoyageFlightNo/text()"/>
            </xsl:otherwise>
          </xsl:choose>
        </FLI1>
      </ns0:FLI>
      <ns0:DTM_2>
        <DTM1>
          <xsl:value-of select ="ScriptNS1:ConvertXmlDateString($Root/s0:DateCollection/s0:Date[s0:Type/text() = 'Departure']/s0:Value/text(), 'yyyyMMddHHmmss')" />
        </DTM1>
      </ns0:DTM_2>
      <ns0:PAR>
        <ns0:PAR1>
          <PAR1.1>
            <xsl:value-of select= "substring($SubShipment/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text() = 'ConsignorDocumentaryAddress']/s0:CompanyName/text(), 1, 70)"/>
          </PAR1.1>
          <PAR1.2>
            <xsl:value-of select="substring($SubShipment/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text() = 'ConsigneeDocumentaryAddress']/s0:CompanyName/text(), 1, 70)"/>
          </PAR1.2>
        </ns0:PAR1>
        <ns0:PAR2>
          <PAR2.1>
            <xsl:value-of select="$SubShipment/s0:AddInfoCollection/s0:AddInfo[s0:Key = 'PartyIndicator']/s0:Value/text()"/>
          </PAR2.1>
        </ns0:PAR2>
      </ns0:PAR>
      <ns0:LOC>
        <LOC1>
          <xsl:variable name="PortOfDischarge" select="$Root/s0:CustomsDischargePort/s0:Code/text()"/>
          <xsl:choose>
            <xsl:when test="$PortOfDischarge!=''">
              <xsl:value-of select="$PortOfDischarge"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$Root/s0:PortOfDischarge/s0:Code/text()"/>
            </xsl:otherwise>
          </xsl:choose>
        </LOC1>
      </ns0:LOC>
      <ns0:EQN>
        <EQN1>
          <xsl:variable name="TotalPackages" select="$SubShipment/s0:OuterPacks/text()"/>
          <xsl:choose>
            <xsl:when test="$TotalPackages != ''">
              <xsl:value-of select="ScriptNS2:FormatDecimal($TotalPackages, '0.####', false())"/>
            </xsl:when>
            <xsl:otherwise>1.0000</xsl:otherwise>
          </xsl:choose>
        </EQN1>
        <EQN2>
          <xsl:variable name="GrossWeight" select="$SubShipment/s0:TotalWeight/text()"/>
          <xsl:choose>
            <xsl:when test="$GrossWeight != ''">
              <xsl:value-of select="ScriptNS2:FormatDecimal($GrossWeight, '0.####', false())"/>
            </xsl:when>
            <xsl:otherwise>0.0100</xsl:otherwise>
          </xsl:choose>
        </EQN2>
      </ns0:EQN>
      <xsl:variable name="Current" select="1 + ($PackLoop - 1) * 50"/>
      <xsl:variable name="PackLineCount" select="count($SubShipment/s0:PackingLineCollection/s0:PackingLine)"/>
      <xsl:variable name ="End">
        <xsl:choose>
          <xsl:when test="($Current + 50) > $PackLineCount">
            <xsl:value-of select="$PackLineCount + 1"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$PackLoop * 50 + 1"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="SG3Loop">
        <xsl:call-template name="SG3Loop" >
          <xsl:with-param name="Current" select="$Current"/>
          <xsl:with-param name="End" select="$End"/>
          <xsl:with-param name="PackingLineCollection" select="$SubShipment/s0:PackingLineCollection"/>
          <xsl:with-param name="HAWB" select="$HAWB"/>
          <xsl:with-param name="CountryOfDestination" select="substring($SubShipment/s0:PortOfDestination/s0:Code/text(),1,2)"/>
        </xsl:call-template>
      </xsl:variable>
      <ns0:MOA>
        <MOA1>
          <xsl:value-of select="ScriptNS2:FormatDecimal(
                      sum(msxsl:node-set($SG3Loop)/ns0:SG3Loop/ns0:MOA_2/MOA1/text()),
                      '0.##',
                      false())"/>
        </MOA1>
      </ns0:MOA>
      <xsl:copy-of select="$SG3Loop"/>
    </ns0:SG2Loop>
  </xsl:template>

  <xsl:template name="SG3Loop">
    <xsl:param name="Current"/>
    <xsl:param name="End"/>
    <xsl:param name="PackingLineCollection"/>
    <xsl:param name="HAWB"/>
    <xsl:param name="CountryOfDestination"/>

    <xsl:if test="$Current &lt; $End">
      <xsl:call-template name="GenerateSG3">
        <xsl:with-param name="Count" select="$Current"/>
        <xsl:with-param name="PackingLine" select ="$PackingLineCollection/s0:PackingLine[$Current]"/>
        <xsl:with-param name="HAWB" select="$HAWB"/>
        <xsl:with-param name="CountryOfDestination" select="$CountryOfDestination"/>
      </xsl:call-template>
      <xsl:call-template name="SG3Loop">
        <xsl:with-param name="Current" select="$Current + 1"/>
        <xsl:with-param name="End" select="$End"/>
        <xsl:with-param name ="PackingLineCollection" select="$PackingLineCollection"/>
        <xsl:with-param name="HAWB" select="$HAWB"/>
        <xsl:with-param name="CountryOfDestination" select="$CountryOfDestination"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name="GenerateSG3">
    <xsl:param name="Count"/>
    <xsl:param name="PackingLine"/>
    <xsl:param name="HAWB"/>
    <xsl:param name="CountryOfDestination"/>

    <ns0:SG3Loop>
      <ns0:SER>
        <SER1>
          <xsl:variable name="CW1ConsignmentReference" select="$PackingLine/s0:AddInfoGroupCollection/s0:AddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'ConsignmentReference']/s0:Value/text() "/>
          <xsl:variable name="ConsignmentItemNumber" select="format-number($Count, '00000')"/>
          <xsl:variable name="addToCW1ReferenceData" select="userCSharp:AddToCW1ReferenceData(concat($ConsignmentItemNumber,$CW1ConsignmentReference))"/>
          <xsl:value-of select="$ConsignmentItemNumber"/>
        </SER1>
        <SER2>
          <xsl:variable name="GoodsType" select="$PackingLine/s0:PackingLineCollection/s0:PackingLine/s0:AddInfoGroupCollection/s0:AddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'GoodsType']/s0:Value/text()"/>
          <xsl:choose>
            <xsl:when test="$GoodsType != ''">
              <xsl:value-of select="$GoodsType"/>
            </xsl:when>
            <xsl:otherwise>NT</xsl:otherwise>
          </xsl:choose>
        </SER2>
        <SER3>
          <xsl:value-of select="substring($PackingLine/s0:PackingLineCollection/s0:PackingLine/s0:GoodsDescription/text(), 1, 70)"/>
        </SER3>
        <SER4>
          <xsl:variable name ="HarmonisedCode" select ="$PackingLine/s0:PackingLineCollection/s0:PackingLine/s0:HarmonisedCode"/>
          <xsl:if test="$HarmonisedCode != ''">
            <xsl:value-of select="$HarmonisedCode"/>
          </xsl:if>
        </SER4>
      </ns0:SER>

      <ns0:CTY>
        <CTY1>
          <xsl:choose>
            <xsl:when test="$CountryOfDestination!=''">
              <xsl:value-of select="$CountryOfDestination"/>
            </xsl:when>
            <xsl:otherwise>XX</xsl:otherwise>
          </xsl:choose>
        </CTY1>
        <CTY2>
          <xsl:variable name="CountryOfOrign" select="$PackingLine/s0:PackingLineCollection/s0:PackingLine/s0:CountryOfOrigin/s0:Code/text()"/>
          <xsl:choose>
            <xsl:when test="$CountryOfOrign!=''">
              <xsl:value-of select="$CountryOfOrign"/>
            </xsl:when>
            <xsl:otherwise>XX</xsl:otherwise>
          </xsl:choose>
        </CTY2>
      </ns0:CTY>

      <xsl:variable name="CustomsQty" select="$PackingLine/s0:PackingLineCollection/s0:PackingLine/s0:AddInfoGroupCollection/s0:AddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'CustomsQty']/s0:Value"/>
      <xsl:variable name="CustomsUQ" select="$PackingLine/s0:PackingLineCollection/s0:PackingLine/s0:AddInfoGroupCollection/s0:AddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'CustomsUQ']/s0:Value/text()"/>

      <ns0:MEA>
        <MEA1>
          <xsl:choose>
            <xsl:when test="$CustomsUQ!=''">
              <xsl:value-of select="$CustomsUQ"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$PackingLine/s0:PackingLineCollection/s0:PackingLine/s0:PackType/s0:Code/text()"/>
            </xsl:otherwise>
          </xsl:choose>
        </MEA1>
        <MEA2>
          <xsl:choose>
            <xsl:when test="$CustomsQty!=''">
              <xsl:value-of select="ScriptNS2:FormatDecimal($CustomsQty,'0.####', false())"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="ScriptNS2:FormatDecimal($PackingLine/s0:PackingLineCollection/s0:PackingLine/s0:PackQty/text(), '0.####', false())"/>
            </xsl:otherwise>
          </xsl:choose>
        </MEA2>
      </ns0:MEA>

      <ns0:MOA_2>
        <MOA1>
          <xsl:value-of select="ScriptNS2:FormatDecimal($PackingLine/s0:PackingLineCollection/s0:PackingLine/s0:AddInfoGroupCollection/s0:AddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'CustomsValue']/s0:Value/text(), '0.##', false())"/>
        </MOA1>
      </ns0:MOA_2>

      <xsl:variable name="PermitNum" select="$PackingLine/s0:PackingLineCollection/s0:PackingLine/s0:AddInfoGroupCollection/s0:AddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'PermitNumber']/s0:Value/text()"/>
      <xsl:if test="$PermitNum != ''">
        <ns0:DOC>
          <DOC1>
            <xsl:value-of select="$PermitNum"/>
          </DOC1>
        </ns0:DOC>
      </xsl:if>
    </ns0:SG3Loop>
  </xsl:template>

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[
    int CSTCount = 0;

    public void IncrementCST()
    {
      CSTCount += 1;
    }

    public void ResetCSTCount()
    {
      CSTCount = 0;
    }

    public int CheckCSTCount()
    {
      return CSTCount;
    }

    int subShipmentCount = 1;

    public int IncrementSubShipment()
    {
      subShipmentCount += 1;
      return subShipmentCount;
    }

    public int CheckSubShipmentCount()
    {
      return subShipmentCount;
    }

    System.Collections.Generic.List<string> CSTData = new System.Collections.Generic.List<string>();
    
    public void AddToCSTDATA(string value)
    {
      CSTData.Add(value);
    }

    public string GetCSTData()
    {
      return String.Join("|", CSTData.ToArray());
    }

    public void ClearCSTData()
    {
      CSTData.Clear();
    }

    System.Collections.Generic.List<string> CW1ReferenceData = new System.Collections.Generic.List<string>();

    public void AddToCW1ReferenceData(string value)
    {
      CW1ReferenceData.Add(value);
    }

    public string GetCW1ReferenceData()
    {
      return String.Join("|", CW1ReferenceData.ToArray());
    }

    public void ClearCW1ReferenceData()
    {
      CW1ReferenceData.Clear();
    }

    System.Collections.Generic.List<string> HAWBData = new System.Collections.Generic.List<string>();

    public void AddToHAWBDATA(string value)
    {
      HAWBData.Add(value);
    }

    public string GetHAWBData()
    {
      return String.Join("|", HAWBData.ToArray());
    }

    public void ClearHAWBData()
    {
      HAWBData.Clear();
    }
]]>
  </msxsl:script>

</xsl:stylesheet>

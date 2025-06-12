<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 ScriptNS0 ScriptNS1 ScriptNS2 ScriptNS3 ScriptNS4 ScriptNS5 ScriptNS6 userCSharp" version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:ns0="http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"
                xmlns:ns1="http://wisetechglobal.com/eHub/Products/SGCustoms/MHAccess/2017/08"
                xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
                xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1"
                xmlns:ScriptNS2="http://schemas.microsoft.com/BizTalk/2003/ScriptNS2"
                xmlns:ScriptNS3="http://schemas.microsoft.com/BizTalk/2003/ScriptNS3"
                xmlns:ScriptNS4="http://schemas.microsoft.com/BizTalk/2003/ScriptNS4"
                xmlns:ScriptNS5="http://schemas.microsoft.com/BizTalk/2003/ScriptNS5"
                xmlns:ScriptNS6="http://schemas.microsoft.com/BizTalk/2003/ScriptNS6"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes" />

  <xsl:variable name="SenderID" select="ScriptNS3:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="RecipientID" select="ScriptNS3:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="Root" select="/s0:UniversalShipment/s0:Shipment"/>
  <xsl:variable name="staffID" select="$Root/s0:CustomsBroker/s0:Code/text()" />
  <xsl:variable name="account" select="ScriptNS5:GetSGCustomsAccount($staffID, $SenderID)" />
  <xsl:variable name="isAccountValid" select="ScriptNS5:IsAccountValid($staffID, $SenderID, $account)" />
  <xsl:variable name="FileName" select="ScriptNS3:SetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06', $account)"/>
  <xsl:variable name="SGCID" select="ScriptNS5:GetSGCustomsSenderID($account)" />
  <xsl:variable name="MessageReferenceID" select="$Root/s0:DataContext/s0:DataSourceCollection/s0:DataSource[1]/s0:Key/text()" />
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
  <xsl:variable name="CycleDate" select="ScriptNS1:ConvertXmlDateString($Root/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CycleDate']/s0:Value/text(), 'yyyyMMdd')"/>
  <xsl:variable name="CycleNumber" select="format-number($Root/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CycleNumber']/s0:Value/text(), '00')" />

  <xsl:variable name="Subscription" select="ScriptNS6:InitializeSubscription()" />
  <xsl:variable name="ExistingSubscription" select="ScriptNS0:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $RecipientID , '@recipientId', $SenderID, '@ST_ID', 'SGCMSG', '@value', concat($MessageReferenceID,'I'))"/>
  <xsl:variable name="LoadOrGenerateSubscription">
    <xsl:choose>
      <xsl:when test="$ExistingSubscription=''">
        <xsl:variable name="SubscribeShipment" select="ScriptNS6:SubscribeShipment($Subscription, $MessageReferenceID, $MAWB, 'Import')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="ScriptNS6:LoadSubscription($Subscription, $ExistingSubscription)"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:template match="/">
    <xsl:apply-templates select="s0:UniversalShipment/s0:Shipment" />
  </xsl:template>

  <xsl:template match="s0:UniversalShipment/s0:Shipment">
    <ns1:AIRPCMEnvelope>
      <xsl:variable name="SubShipments">
        <xsl:call-template name="Generate_SubShipments"/>
      </xsl:variable>
      <xsl:variable name="StartSubShipments" select="msxsl:node-set($SubShipments)/SubShipment[position() mod 100 = 1]" />
      <xsl:for-each select="$StartSubShipments">
        <xsl:variable name="lastNode" select="following-sibling::*[@position mod 100 = 0][1]" />
        <xsl:variable name="lastPosition">
          <xsl:choose>
            <xsl:when test="$lastNode">
              <xsl:value-of select="$lastNode/@position"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="following-sibling::*[last()]/@position"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:call-template name="Generate_Message">
          <xsl:with-param name="SubShipments" select=". | following-sibling::*[$lastPosition >= @position]" />
        </xsl:call-template>
      </xsl:for-each>
    </ns1:AIRPCMEnvelope>

    <xsl:variable name="InsertSubscription" select="ScriptNS4:InsertSubscriptionValue('SGCMSG', $RecipientID, $SenderID, concat($MessageReferenceID,'I'), ScriptNS6:ToString($Subscription))"/>
  </xsl:template>

  <xsl:template name="Generate_SubShipments">
    <xsl:for-each select="s0:SubShipmentCollection/s0:SubShipment">
      <xsl:variable name ="TotalPackLines" select ="count(s0:PackingLineCollection/s0:PackingLine)"/>
      <xsl:variable name ="TotalPackLoops" select="ceiling($TotalPackLines div 50)"/>
      <xsl:call-template name="LoopSG1">
        <xsl:with-param name="Until" select="$TotalPackLoops" />
        <xsl:with-param name="Position" select="position()" />
      </xsl:call-template>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="LoopSG1">
    <xsl:param name="Next" select="1" />
    <xsl:param name="Until" />
    <xsl:param name="Position" />

    <SubShipment>
      <xsl:attribute name="position">
        <xsl:value-of select="userCSharp:GetPosition(false())"/>
      </xsl:attribute>
      <xsl:attribute name="realPosition">
        <xsl:value-of select="$Position"/>
      </xsl:attribute>
      <xsl:attribute name="repeat">
        <xsl:value-of select="$Next"/>
      </xsl:attribute>
      <xsl:attribute name="totalPackLoops">
        <xsl:value-of select="$Until"/>
      </xsl:attribute>
    </SubShipment>

    <xsl:if test="$Until > $Next">
      <xsl:call-template name="LoopSG1">
        <xsl:with-param name="Next" select="$Next + 1" />
        <xsl:with-param name="Until" select="$Until" />
        <xsl:with-param name="Position" select="$Position"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name="Generate_Message">
    <xsl:param name="SubShipments" />

    <xsl:variable name="IDT1_3" select="format-number(ScriptNS0:CallActionProcedureHelper('GetCounterValue','@value','@name','CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.IDT','@maxlength','4'), '0000')" />
    <xsl:variable name="IDT" select="concat($IDT1_1, $IDT1_2, $IDT1_3)" />

    <xsl:variable name="SubscribeHistoryOrder" select="ScriptNS6:SubscribeHistoryOrder($Subscription, 'PCM', ScriptNS1:CurrentDateTimeUTC('s'), $IDT1_1, $IDT1_2, $IDT1_3)" />
    <xsl:variable name="SubscribeCycleDate" select="ScriptNS6:SubscribeMessageInfo($Subscription, $IDT, 'PCM', 'CycleDate', $CycleDate)" />
    <xsl:variable name="SubscribeCycleNumber" select="ScriptNS6:SubscribeMessageInfo($Subscription, $IDT, 'PCM', 'CycleNumber', $CycleNumber)" />
    <xsl:if test="$isAccountValid = 'true'">
      <xsl:variable name="SubscribeManifestNumberForAIRPIN" select="ScriptNS4:InsertSubscriptionValue('SGCMSG', $RecipientID, $SenderID, concat($IDT, 'I'), $MessageReferenceID, 'ManifestNumber')" />
    </xsl:if>

    <ns0:EFACT_31_AIRPCM>
      <UNH>
        <UNH1>
          <xsl:value-of select="$MessageReferenceID" />
        </UNH1>
        <UNH2>
          <UNH2.1>AIRPCM</UNH2.1>
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
      <ns0:FLI>
        <FLI1>
          <xsl:choose>
            <xsl:when test="$Root/s0:TransportMode/s0:Code/text()='ROA'">
              <xsl:value-of select="'ROAD'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$Root/s0:VoyageFlightNo/text()"/>
            </xsl:otherwise>
          </xsl:choose>
        </FLI1>
      </ns0:FLI>
      <ns0:DTM>
        <DTM1>
          <xsl:value-of select="ScriptNS1:ConvertXmlDateString($Root/s0:DateCollection/s0:Date[s0:Type/text()='Arrival']/s0:Value/text(), 'yyyyMMdd')"/>
        </DTM1>
      </ns0:DTM>
      <ns0:PUR>
        <PUR1>I</PUR1>
      </ns0:PUR>
      <ns0:DTM_2>
        <DTM1>
          <xsl:value-of select="$CycleDate"/>
        </DTM1>
        <DTM2>
          <xsl:value-of select="$CycleNumber" />
        </DTM2>
      </ns0:DTM_2>
      <ns0:DOC>
        <DOC1>
          <xsl:variable name="QCIBranch" select="$Root/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='Branch']/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text()='QCI']/s0:Value/text()" />
          <xsl:value-of select="concat($QCIBranch,$CycleDate,$CycleNumber)" />
        </DOC1>
      </ns0:DOC>
      <ns0:REF>
        <REF1>
          <xsl:value-of select="$MAWB"/>
        </REF1>
      </ns0:REF>
      <xsl:for-each select="$SubShipments">
        <xsl:variable name="position" select="./@realPosition" />
        <xsl:variable name="SubShipment" select="$Root/s0:SubShipmentCollection/s0:SubShipment[position() = $position]"/>
        <xsl:variable name="HAWB" select="$SubShipment/s0:WayBillNumber/text()"/>
        <xsl:variable name="PortOfLoading">
          <xsl:variable name="portOfLoading" select="$Root/s0:CustomsLoadPort/s0:Code/text()" />
          <xsl:choose>
            <xsl:when test="$portOfLoading!=''">
              <xsl:value-of select="$portOfLoading"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$Root/s0:PortOfLoading/s0:Code/text()"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <xsl:variable name="SubscribeHAWB" select="ScriptNS6:SubscribeHAWB($Subscription, $HAWB)"/>

        <xsl:call-template name="Generate_SGLoop1">
          <xsl:with-param name="SubShipment" select="$Root/s0:SubShipmentCollection/s0:SubShipment[position() = $position]"/>
          <xsl:with-param name="packingLinesLoop" select="./@repeat"/>
          <xsl:with-param name="MessageReferenceID" select="$MessageReferenceID"/>
          <xsl:with-param name="HAWB" select="$HAWB" />
          <xsl:with-param name="IDT" select="$IDT"/>
          <xsl:with-param name="PortOfLoading" select="$PortOfLoading"/>
        </xsl:call-template>
      </xsl:for-each>
    </ns0:EFACT_31_AIRPCM>
  </xsl:template>

  <xsl:template name="Generate_SGLoop1">
    <xsl:param name="SubShipment" />
    <xsl:param name="packingLinesLoop"/>
    <xsl:param name="MessageReferenceID"/>
    <xsl:param name="HAWB"/>
    <xsl:param name="IDT"/>
    <xsl:param name="PortOfLoading"/>

    <xsl:variable name="firstPackingLine" select="50 * ($packingLinesLoop - 1) + 1" />
    <xsl:variable name="lastPackingLine" select="50 * $packingLinesLoop" />

    <ns0:SGLoop1>
      <ns0:RFF>
        <RFF1>
          <xsl:value-of select="$HAWB"/>
        </RFF1>
      </ns0:RFF>
      <ns0:PAR>
        <ns0:PAR1>
          <PAR1.1>
            <xsl:value-of select="substring($SubShipment/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='ConsigneeDocumentaryAddress']/s0:CompanyName/text(), 1, 70)"/>
          </PAR1.1>
          <PAR1.2>
            <xsl:value-of select="substring($SubShipment/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='ConsignorDocumentaryAddress']/s0:CompanyName/text(), 1, 70)"/>
          </PAR1.2>
        </ns0:PAR1>
        <xsl:variable name="partyIndicator" select="$SubShipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='PartyIndicator']/s0:Value/text()"/>
        <xsl:variable name="partyStatus" select="$SubShipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='PartyStatus']/s0:Value/text()"/>
        <xsl:if test="$partyIndicator != ''">
          <ns0:PAR2>
            <PAR2.1>
              <xsl:value-of select="$partyIndicator"/>
            </PAR2.1>
            <PAR2.2>
              <xsl:choose>
                <xsl:when test="$partyStatus != ''">
                  <xsl:value-of select="$partyStatus"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="'N'"/>
                </xsl:otherwise>
              </xsl:choose>
            </PAR2.2>
          </ns0:PAR2>
        </xsl:if>
      </ns0:PAR>
      <xsl:variable name="payeeIndicator" select="$SubShipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='PayeeIndicator']/s0:Value/text()"/>
      <xsl:if test="$payeeIndicator != ''">
        <ns0:IND>
          <IND1>
            <xsl:value-of select="$payeeIndicator"/>
          </IND1>
        </ns0:IND>
      </xsl:if>
      <xsl:variable name="GSTNReferenceNo" select="$SubShipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='GSTNReferenceNo']/s0:Value/text()" />
      <xsl:if test="$GSTNReferenceNo != ''">
        <ns0:DOC_2>
          <DOC1>
            <xsl:value-of select="$GSTNReferenceNo" />
          </DOC1>
        </ns0:DOC_2>
      </xsl:if>
      <ns0:LOC>
        <LOC1>
          <xsl:value-of select="$PortOfLoading"/>
        </LOC1>
      </ns0:LOC>
      <ns0:EQN>
        <EQN1>
          <xsl:variable name="OuterPacks" select="$SubShipment/s0:OuterPacks/text()"/>
          <xsl:choose>
            <xsl:when test="$OuterPacks != ''">
              <xsl:value-of select="format-number($OuterPacks, '0.0000')"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="1.0000"/>
            </xsl:otherwise>
          </xsl:choose>
        </EQN1>
        <EQN2>
          <xsl:variable name="TotalWeight" select="format-number($SubShipment/s0:TotalWeight/text(), '0.0000')"/>
          <xsl:choose>
            <xsl:when test="$TotalWeight != ''">
              <xsl:value-of select="format-number($TotalWeight, '0.0000')"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="0.0100"/>
            </xsl:otherwise>
          </xsl:choose>
        </EQN2>
      </ns0:EQN>
      <ns0:MOA>
        <MOA1>
          <xsl:value-of select="format-number($SubShipment/s0:CommercialInfo/s0:CommercialChargeCollection/s0:CommercialCharge[s0:ChargeType/s0:Code/text()='CUS']/s0:Amount/text(), '0.00')"/>
        </MOA1>
        <MOA2>
          <xsl:value-of select="format-number($SubShipment/s0:CommercialInfo/s0:CommercialChargeCollection/s0:CommercialCharge[s0:ChargeType/s0:Code/text()='GST']/s0:Amount/text(), '0.00')"/>
        </MOA2>
        <MOA3>
          <xsl:value-of select="format-number($SubShipment/s0:CommercialInfo/s0:CommercialChargeCollection/s0:CommercialCharge[s0:ChargeType/s0:Code/text()='CDU']/s0:Amount/text(), '0.00')"/>
        </MOA3>
      </ns0:MOA>
      <xsl:for-each select="$SubShipment/s0:PackingLineCollection/s0:PackingLine[position() >= $firstPackingLine and $lastPackingLine >= position()]">
        <ns0:SGLoop2>
          <ns0:CST>
            <CST1>
              <xsl:variable name="ConsignmentReference" select="s0:AddInfoGroupCollection/s0:AddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'ConsignmentReference']/s0:Value/text()"/>

              <xsl:variable name="SubscribedCST" select="ScriptNS6:SelectSGIDByRef($Subscription, $HAWB, $ConsignmentReference)" />
              <xsl:variable name="CST">
                <xsl:choose>
                  <xsl:when test="$SubscribedCST != ''">
                    <xsl:value-of  select="$SubscribedCST"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of  select="format-number(ScriptNS0:CallActionProcedureHelper('GetCounterValue','@value','@name','CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.Maps.AIRPCM.CST','@maxlength','5'), '00000')" />
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:variable>
              <xsl:value-of select="$CST" />

              <xsl:variable name="SubscribePackingLine" select="ScriptNS6:SubscribePackLine($Subscription, $HAWB, $CST, $ConsignmentReference, 'Declared', $IDT, 'PCM')"/>
              <xsl:variable name="SubscribeInfo" select="ScriptNS6:SubscribeInfo($Subscription, $HAWB, $ConsignmentReference, $IDT, 'PCM', 'HAWBSplitSegment', $packingLinesLoop)" />
            </CST1>
            <xsl:variable name="GoodsType" select="s0:PackingLineCollection/s0:PackingLine/s0:AddInfoGroupCollection/s0:AddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'GoodsType']/s0:Value/text()"/>
            <CST2>
              <xsl:choose>
                <xsl:when test="$GoodsType != ''">
                  <xsl:value-of select="$GoodsType"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="'NT'"/>
                </xsl:otherwise>
              </xsl:choose>
            </CST2>
            <CST3>
              <xsl:value-of select="substring(s0:PackingLineCollection/s0:PackingLine/s0:GoodsDescription/text(), 1, 70)"/>
            </CST3>
            <xsl:variable name="harmonisedCode" select="s0:PackingLineCollection/s0:PackingLine/s0:HarmonisedCode/text()"/>
            <xsl:if test="$harmonisedCode != ''">
              <CST4>
                <xsl:value-of select="$harmonisedCode"/>
              </CST4>
            </xsl:if>
          </ns0:CST>
          <ns0:CTY>
            <CTY1>
              <xsl:variable name="countryOfOrign" select="s0:PackingLineCollection/s0:PackingLine/s0:CountryOfOrigin/s0:Code/text()"/>
              <xsl:choose>
                <xsl:when test="$countryOfOrign!=''">
                  <xsl:value-of select="$countryOfOrign"/>
                </xsl:when>
                <xsl:otherwise>XX</xsl:otherwise>
              </xsl:choose>
            </CTY1>
          </ns0:CTY>
          <xsl:variable name="customsQty" select="s0:PackingLineCollection/s0:PackingLine/s0:AddInfoGroupCollection/s0:AddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'CustomsQty']/s0:Value"/>
          <xsl:variable name="customsUQ" select="s0:PackingLineCollection/s0:PackingLine/s0:AddInfoGroupCollection/s0:AddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'CustomsUQ']/s0:Value/text()"/>
          <ns0:MEA>
            <MEA1>
              <xsl:choose>
                <xsl:when test="$customsUQ!=''">
                  <xsl:value-of select="$customsUQ"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="s0:PackingLineCollection/s0:PackingLine/s0:PackType/s0:Code/text()"/>
                </xsl:otherwise>
              </xsl:choose>
            </MEA1>
            <MEA2>
              <xsl:choose>
                <xsl:when test="$customsQty!=''">
                  <xsl:value-of select="ScriptNS2:FormatDecimal($customsQty,'0.####', false())"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="s0:PackingLineCollection/s0:PackingLine/s0:PackQty/text()"/>
                </xsl:otherwise>
              </xsl:choose>
            </MEA2>
          </ns0:MEA>
          <ns0:MOA_2>
            <MOA1>
              <xsl:value-of select="format-number(s0:PackingLineCollection/s0:PackingLine/s0:AddInfoGroupCollection/s0:AddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'CustomsValue']/s0:Value/text(), '0.00')"/>
            </MOA1>
            <MOA2>
              <xsl:value-of select="format-number(s0:PackingLineCollection/s0:PackingLine/s0:AddInfoGroupCollection/s0:AddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'TaxValue']/s0:Value/text(), '0.00')"/>
            </MOA2>
            <MOA3>
              <xsl:value-of select="format-number(s0:PackingLineCollection/s0:PackingLine/s0:AddInfoGroupCollection/s0:AddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'DutyValue']/s0:Value/text(), '0.00')"/>
            </MOA3>
          </ns0:MOA_2>
          <xsl:variable name="GSTPaymentIndicator" select="s0:PackingLineCollection/s0:PackingLine/s0:AddInfoGroupCollection/s0:AddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'GSTPaymentIndicator']/s0:Value/text()" />
          <xsl:if test="$GSTPaymentIndicator != ''">
            <ns0:IND_2>
              <IND1>
                <xsl:value-of select="$GSTPaymentIndicator" />
              </IND1>
            </ns0:IND_2>
          </xsl:if>
          <xsl:variable name="permitNumber" select="s0:PackingLineCollection/s0:PackingLine/s0:AddInfoGroupCollection/s0:AddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'PermitNumber']/s0:Value/text()"/>
          <xsl:if test="$permitNumber != ''">
            <ns0:DOC_3>
              <DOC1>
                <xsl:value-of select="$permitNumber"/>
              </DOC1>
            </ns0:DOC_3>
          </xsl:if>
        </ns0:SGLoop2>
      </xsl:for-each>
    </ns0:SGLoop1>

  </xsl:template>

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <msxsl:assembly name="System.Collections" />
    <msxsl:using namespace="System.Collections.Generic" />

    <![CDATA[
    public int position = 0;

    public int GetPosition(bool reset)
    {
      if (reset) position = 0;
      return ++position;
    }
    ]]>
  </msxsl:script>
</xsl:stylesheet>

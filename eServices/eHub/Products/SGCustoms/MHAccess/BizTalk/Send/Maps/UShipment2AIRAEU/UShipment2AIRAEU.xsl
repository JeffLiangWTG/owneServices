<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:xs="http://www.w3.org/2001/XMLSchema"
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

  <xsl:variable name="recipientID" select="ScriptNS3:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="senderID" select="ScriptNS3:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="staffID" select="/s0:UniversalShipment/s0:Shipment[1]/s0:CustomsBroker/s0:Code/text()" />
  <xsl:variable name="account" select="ScriptNS5:GetSGCustomsAccount($staffID, $senderID)" />
  <xsl:variable name="isAccountValid" select="ScriptNS5:IsAccountValid($staffID, $senderID, $account)" />
  <xsl:variable name="SGCID" select="ScriptNS5:GetSGCustomsSenderID($account)" />

  <xsl:template match="/">
    <xsl:apply-templates select="s0:UniversalShipment/s0:Shipment" />
  </xsl:template>

  <xsl:template match="s0:UniversalShipment/s0:Shipment">
    <xsl:variable name="SGDate" select="ScriptNS1:ConvertXmlDateString(ScriptNS1:ConvertUTCToLocalTimeByUNLOCO(ScriptNS1:CurrentDateTimeUTC('s'), 'SGSIN'), 'yyyyMMdd')"/>
    <xsl:variable name="fileName" select="ScriptNS3:SetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06', $account)"/>

    <xsl:call-template name="Construct_IDT_SubShipments">
      <xsl:with-param name="shipment" select="."/>
    </xsl:call-template>

    <xsl:if test="userCSharp:HasNextIDT() = false()">
      <xsl:variable name="throwAllCSTsWereErroredException" select="userCSharp:ThrowAllCSTsWereErroredException()" />
    </xsl:if>

    <ns1:AIRAEUEnvelope>
      <xsl:call-template name="GenerateMessagesPerIDT">
        <xsl:with-param name="SGCID" select="$SGCID"/>
        <xsl:with-param name="SGDate" select="$SGDate"/>
        <xsl:with-param name="totalSubShipments" select="userCSharp:GetTotalSubShipmentsForCurrentIDT()"/>
        <xsl:with-param name="shipment" select="." />
      </xsl:call-template>
    </ns1:AIRAEUEnvelope>
  </xsl:template>

  <xsl:template name="Construct_IDT_SubShipments">
    <xsl:param name="shipment" />
    <xsl:variable name="mawb" select="$shipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ManifestNumber']/s0:Value/text()" />
    <xsl:variable name="jobNumber" select="$shipment/s0:DataContext/s0:DataSourceCollection/s0:DataSource[1]/s0:Key/text()" />
    <xsl:for-each select="$shipment/s0:SubShipmentCollection/s0:SubShipment">
      <xsl:variable name="hawb" select="s0:WayBillNumber/text()"/>
      <xsl:variable name="subscribedIDTs" select="ScriptNS0:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $recipientID, '@recipientId', $senderID, '@ST_ID', 'SGCMSG', '@value', concat($mawb, $hawb, $jobNumber, 'E'), '@referenceType', 'IDT')"/>
      <xsl:if test="$subscribedIDTs = ''">
        <xsl:value-of select="userCSharp:ThrowSubscriptionValueNotFound(concat($mawb, $hawb, $jobNumber, 'E'))"/>
      </xsl:if>
      <xsl:for-each select="userCSharp:Split($subscribedIDTs)">
        <xsl:variable name="subscribedCSTs" select="ScriptNS0:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $recipientID, '@recipientId', $senderID, '@ST_ID', 'SGCMSG', '@value', concat($mawb, $hawb, $jobNumber, ./text(), 'E'), '@referenceType', 'CST')"/>
        <xsl:if test="$subscribedCSTs != ''">
          <xsl:variable name="insertSubShipmentContext" select="userCSharp:AddContext(./text(), $hawb, $subscribedCSTs)"/>
        </xsl:if>
      </xsl:for-each>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="GenerateMessagesPerIDT">
    <xsl:param name="SGCID"/>
    <xsl:param name="SGDate"/>
    <xsl:param name="totalSubShipments"/>
    <xsl:param name="shipment"/>

    <xsl:variable name="jobNumber" select="$shipment/s0:DataContext/s0:DataSourceCollection/s0:DataSource[1]/s0:Key/text()" />

    <xsl:call-template name="MessageLoop">
      <xsl:with-param name="SGCID" select="$SGCID"/>
      <xsl:with-param name="SGDate" select="$SGDate"/>
      <xsl:with-param name="totalSubShipments" select="$totalSubShipments"/>
      <xsl:with-param name="shipment" select="$shipment" />
      <xsl:with-param name="updateIndicator" select="$shipment/s0:DataContext/s0:ActionPurpose/s0:Code/text()"/>
    </xsl:call-template>

    <xsl:value-of select="userCSharp:ResetSubShipmentCount()"/>
    <xsl:if test="userCSharp:HasNextIDT() = true()">
      <xsl:call-template name="GenerateMessagesPerIDT">
        <xsl:with-param name="SGCID" select="$SGCID"/>
        <xsl:with-param name="SGDate" select="$SGDate"/>
        <xsl:with-param name="totalSubShipments" select="userCSharp:GetTotalSubShipmentsForCurrentIDT()"/>
        <xsl:with-param name="shipment" select="$shipment" />
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name="MessageLoop">
    <xsl:param name="SGCID" />
    <xsl:param name="SGDate" />
    <xsl:param name="totalSubShipments"/>
    <xsl:param name="shipment"/>
    <xsl:param name="updateIndicator"/>

    <xsl:if test="userCSharp:CheckSubShipmentCount() &lt;= $totalSubShipments">
      <xsl:call-template name="GenerateMessage">
        <xsl:with-param name="SGCID" select="$SGCID" />
        <xsl:with-param name="SGDate" select="$SGDate" />
        <xsl:with-param name="totalSubShipments" select="$totalSubShipments"/>
        <xsl:with-param name="shipment" select="$shipment" />
        <xsl:with-param name="updateIndicator" select="$updateIndicator" />
      </xsl:call-template>

      <xsl:if test="$updateIndicator != 'AEX'">
        <xsl:call-template name="MessageLoop">
          <xsl:with-param name="SGCID" select="$SGCID" />
          <xsl:with-param name="SGDate" select="$SGDate" />
          <xsl:with-param name="totalSubShipments" select ="$totalSubShipments"/>
          <xsl:with-param name="shipment" select="$shipment"/>
          <xsl:with-param name="updateIndicator" select="$updateIndicator" />
        </xsl:call-template>
      </xsl:if>
    </xsl:if>
  </xsl:template>

  <xsl:template name="GenerateMessage">
    <xsl:param name="SGCID"/>
    <xsl:param name="SGDate"/>
    <xsl:param name="totalSubShipments"/>
    <xsl:param name="shipment"/>
    <xsl:param name="updateIndicator"/>

    <xsl:variable name="messageReferenceID" select="$shipment/s0:DataContext/s0:DataSourceCollection/s0:DataSource[1]/s0:Key/text()" />
    <xsl:variable name="mawb" select="$shipment/s0:WayBillNumber/text()"/>
    <xsl:variable name="hasSubShipmentContext" select="userCSharp:HasNextSubShipmentContext()"/>

    <xsl:choose>
      <xsl:when test="$hasSubShipmentContext = true()">
        <xsl:variable name="IDT1_1">
          <xsl:variable name="HasUENGovRegNum" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='Branch']/s0:GovRegNumType[s0:Code/text()='UEN']/s0:Code/text()"/>
          <xsl:choose>
            <xsl:when test="$HasUENGovRegNum='UEN'">
              <xsl:value-of select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='Branch']/s0:GovRegNum/text()"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='Branch']/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text()='UEN']/s0:Value/text()"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:variable name="IDT1_3" select="format-number(ScriptNS0:CallActionProcedureHelper('GetCounterValue','@value','@name','CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.IDT','@maxlength','4'), '0000')" />
        <xsl:variable name="originalIDTPiped" select="concat(userCSharp:ExtractCurrentIDT(0), '|', userCSharp:ExtractCurrentIDT(1), '|', userCSharp:ExtractCurrentIDT(2))"/>
        <xsl:variable name="updateNumber" select="format-number(number(ScriptNS0:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $recipientID, '@recipientId', $senderID, '@ST_ID', 'SGCMSG', '@value', concat($originalIDTPiped,'E'), '@referenceType', 'UpdateNumber')), '000')"/>
        <xsl:if test="$updateNumber=''">
          <xsl:value-of select="userCSharp:ThrowSubscriptionValueNotFound(concat($originalIDTPiped,'E'))"/>
        </xsl:if>

        <xsl:variable name="linkAIRAEUWithAIRAED" select="ScriptNS4:InsertSubscriptionValue('SGCMSG', $recipientID, $senderID, concat($IDT1_1, '|', $SGDate, '|', $IDT1_3, 'E'), $originalIDTPiped, 'OriginalIDT')" />
        <xsl:variable name="messageIDT" select="concat($IDT1_1,$SGDate,$IDT1_3)"/>
        <xsl:variable name="messageMAWB" select="ScriptNS4:InsertSubscriptionValue('SGCMSG', $recipientID, $senderID, concat($messageIDT,'E'), $mawb, 'MAWB')" />
        <xsl:variable name="messageHAWB" select="ScriptNS4:InsertSubscriptionValue('SGCMSG', $recipientID, $senderID, concat($messageIDT,'E'), userCSharp:GetHAWBsForCurrentIDT(), 'HAWB')" />
        <xsl:if test="$isAccountValid = 'true'">
          <xsl:variable name="messageMAN" select="ScriptNS4:InsertSubscriptionValue('SGCMSG', $recipientID, $senderID, concat($messageIDT, 'E'), $messageReferenceID, 'ManifestNumber')" />
        </xsl:if>

        <xsl:variable name="subShipment" select="$shipment/s0:SubShipmentCollection/s0:SubShipment[s0:WayBillNumber/text() = userCSharp:GetHAWB()]"/>

        <ns0:EFACT_31_AIRAEU>
          <UNH>
            <UNH1>
              <xsl:value-of select="$messageReferenceID" />
            </UNH1>
            <UNH2>
              <UNH2.1>AIRAEU</UNH2.1>
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
                <xsl:value-of select="$SGDate" />
              </IDT1.2>
              <IDT1.3>
                <xsl:value-of select="$IDT1_3"/>
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
          <ns0:FTX>
            <ns0:FTX1>
              <FTX1.1>
                <xsl:value-of select="userCSharp:ExtractCurrentIDT(0)"/>
              </FTX1.1>
              <FTX1.2>
                <xsl:value-of select="userCSharp:ExtractCurrentIDT(1)"/>
              </FTX1.2>
              <FTX1.3>
                <xsl:value-of select="userCSharp:ExtractCurrentIDT(2)"/>
              </FTX1.3>
            </ns0:FTX1>
          </ns0:FTX>
          <ns0:UPD>
            <UPD1>
              <xsl:value-of select="$updateNumber"/>
              <xsl:if test="$isAccountValid = 'true'">
                <xsl:variable name="subscribeUpdateNumberForAIRAEU" select="ScriptNS4:InsertSubscriptionValue('SGCMSG', $recipientID, $senderID, concat($originalIDTPiped,'E'), number($updateNumber) + 1, 'UpdateNumber')" />
              </xsl:if>
            </UPD1>
            <UPD2>
              <xsl:choose>
                <xsl:when test="$updateIndicator='AEU'">U</xsl:when>
                <xsl:when test="$updateIndicator='AEC'">D</xsl:when>
                <xsl:when test="$updateIndicator='AEX'">C</xsl:when>
              </xsl:choose>
            </UPD2>
            <UPD3>
              <xsl:value-of select="ScriptNS0:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $recipientID, '@recipientId', $senderID, '@ST_ID', 'SGCMSG', '@value', concat($originalIDTPiped, 'E'), '@referenceType', 'PermitNumber')" />
            </UPD3>
          </ns0:UPD>
          <xsl:if test="$updateIndicator != 'AEX'">
            <xsl:call-template name="SG1Loop">
              <xsl:with-param name="updateIndicator" select="$updateIndicator"/>
              <xsl:with-param name="totalSubShipments" select="$totalSubShipments"/>
              <xsl:with-param name="shipment" select="$shipment"/>
              <xsl:with-param name="subShipment" select ="$subShipment"/>
              <xsl:with-param name="jobNumber" select="$messageReferenceID"/>
            </xsl:call-template>
          </xsl:if>
        </ns0:EFACT_31_AIRAEU>
      </xsl:when>
      <xsl:otherwise>
        <xsl:variable name ="incrementSubShipmentCount" select="userCSharp:IncrementSubShipmentCount()"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="SG1Loop">
    <xsl:param name="updateIndicator"/>
    <xsl:param name="totalSubShipments"/>
    <xsl:param name="shipment"/>
    <xsl:param name="subShipment"/>
    <xsl:param name="jobNumber"/>

    <xsl:variable name="subShipmentCount" select="userCSharp:CheckSubShipmentCount()"/>
    <xsl:variable name ="messageCSTCount" select="userCSharp:GetMessageCSTCount()"/>
    <xsl:variable name="subShipmentCSTTotal" select="userCSharp:SubShipmentCSTTotal()"/>

    <xsl:if test="($messageCSTCount + $subShipmentCSTTotal) &lt;= 100 and $subShipmentCount &lt;= $totalSubShipments" >
      <xsl:call-template name="SubShipmentLoop">
        <xsl:with-param name="updateIndicator" select="$updateIndicator"/>
        <xsl:with-param name ="shipment" select="$shipment"/>
        <xsl:with-param name ="subShipment" select="$subShipment"/>
        <xsl:with-param name="jobNumber" select="$jobNumber"/>
      </xsl:call-template>
      <xsl:choose>
        <xsl:when test="(userCSharp:GetOriginalCSTCount() >= ceiling(count($subShipment/s0:PackingLineCollection/s0:PackingLine) div 50)) or (userCSharp:GetOriginalCSTCount() >= userCSharp:SubShipmentCSTTotal())">
          <xsl:variable name ="incrementSubShipmentCount" select="userCSharp:IncrementSubShipmentCount()"/>
          <xsl:value-of select="userCSharp:ResetOriginalCSTCount()"/>
          <xsl:if test="userCSharp:HasNextSubShipmentContext()">
            <xsl:call-template name="SG1Loop">
              <xsl:with-param name="updateIndicator" select="$updateIndicator"/>
              <xsl:with-param name="totalSubShipments" select ="$totalSubShipments"/>
              <xsl:with-param name="shipment" select="$shipment"/>
              <xsl:with-param name="subShipment" select="$shipment/s0:SubShipmentCollection/s0:SubShipment[s0:WayBillNumber/text() = userCSharp:GetHAWB()]"/>
              <xsl:with-param name="jobNumber" select="$jobNumber"/>
            </xsl:call-template>
          </xsl:if>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="SG1Loop">
            <xsl:with-param name="updateIndicator" select="$updateIndicator"/>
            <xsl:with-param name="totalSubShipments" select ="$totalSubShipments"/>
            <xsl:with-param name="shipment" select="$shipment"/>
            <xsl:with-param name="subShipment" select="$subShipment"/>
            <xsl:with-param name="jobNumber" select="$jobNumber"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>
    <xsl:value-of select="userCSharp:ResetOriginalCSTCount()"/>
    <xsl:value-of select="userCSharp:ResetMessageCSTCount()"/>
  </xsl:template>

  <xsl:template name="SubShipmentLoop">
    <xsl:param name="updateIndicator"/>
    <xsl:param name="shipment"/>
    <xsl:param name="subShipment"/>
    <xsl:param name="jobNumber"/>

    <ns0:SG1Loop>
      <xsl:variable name="originalCST" select="userCSharp:ExtractOriginalCST(userCSharp:IncrementOriginalCSTCount() - 1)"/>
      <xsl:call-template name="CST-RFF">
        <xsl:with-param name="subShipment" select="$subShipment"/>
        <xsl:with-param name="originalCST" select="$originalCST"/>
      </xsl:call-template>
      <xsl:if test="$updateIndicator = 'AEU'">
        <xsl:variable name="hawb" select="$subShipment/s0:WayBillNumber/text()"/>
        <xsl:variable name="mawb" select="$shipment/s0:AddInfoCollection/s0:AddInfo[s0:Key= 'ManifestNumber']/s0:Value/text()"/>
        <xsl:call-template name="SG2Loop">
          <xsl:with-param name="shipment" select="$shipment"/>
          <xsl:with-param name="subShipment" select="$subShipment"/>
          <xsl:with-param name="mawb" select="$mawb"/>
          <xsl:with-param name="hawb" select="$hawb"/>
          <xsl:with-param name="jobNumber" select="$jobNumber"/>
        </xsl:call-template>
        <xsl:variable name="CW1ReferenceData" select="userCSharp:GetCW1ReferenceData()"/>
        <xsl:variable name="subscribeCW1ReferenceData" select="ScriptNS4:InsertSubscriptionValue('SGCMSG', $recipientID, $senderID, concat($mawb,$hawb,$jobNumber,$originalCST,'E'), $CW1ReferenceData, 'CNRF')" />
        <xsl:variable name="CW1ReferenceClear" select="userCSharp:ClearCW1ReferenceData()"/>
      </xsl:if>
    </ns0:SG1Loop>
  </xsl:template>

  <xsl:template name="CST-RFF">
    <xsl:param name="subShipment"/>
    <xsl:param name="originalCST"/>
    <ns0:CST>
      <CST1>
        <xsl:value-of select="$originalCST"/>
        <xsl:variable name="CSTIncremented" select="userCSharp:IncrementMessageCSTCount()"/>
      </CST1>
    </ns0:CST>
    <ns0:RFF>
      <RFF1>
        <xsl:value-of select ="$subShipment/s0:WayBillNumber/text()"/>
      </RFF1>
    </ns0:RFF>
  </xsl:template>

  <xsl:template name="SG2Loop">
    <xsl:param name="shipment"/>
    <xsl:param name="subShipment"/>
    <xsl:param name="mawb"/>
    <xsl:param name="hawb"/>
    <xsl:param name="jobNumber"/>

    <ns0:SG2Loop>
      <ns0:REF>
        <REF1>
          <xsl:value-of select ="$mawb"/>
        </REF1>
      </ns0:REF>
      <ns0:FLI>
        <FLI1>
          <xsl:choose>
            <xsl:when test="$shipment/s0:TransportMode/s0:Code/text()='ROA'">ROAD</xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$shipment/s0:VoyageFlightNo/text()"/>
            </xsl:otherwise>
          </xsl:choose>
        </FLI1>
      </ns0:FLI>
      <ns0:DTM>
        <DTM1>
          <xsl:value-of select ="ScriptNS1:ConvertXmlDateString($shipment/s0:DateCollection/s0:Date[s0:Type/text() = 'Departure']/s0:Value/text(), 'yyyyMMddHHmmss')" />
        </DTM1>
      </ns0:DTM>
      <ns0:PAR>
        <ns0:PAR1>
          <PAR1.1>
            <xsl:value-of select="substring($subShipment/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text() = 'ConsignorDocumentaryAddress']/s0:CompanyName/text(), 1, 70)"/>
          </PAR1.1>
          <PAR1.2>
            <xsl:value-of select="substring($subShipment/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text() = 'ConsigneeDocumentaryAddress']/s0:CompanyName/text(), 1, 70)"/>
          </PAR1.2>
        </ns0:PAR1>
        <ns0:PAR2>
          <PAR2.1>
            <xsl:value-of select="$subShipment/s0:AddInfoCollection/s0:AddInfo[s0:Key = 'PartyIndicator']/s0:Value/text()"/>
          </PAR2.1>
        </ns0:PAR2>
      </ns0:PAR>
      <ns0:LOC>
        <LOC1>
          <xsl:variable name="portOfDischarge" select="$shipment/s0:CustomsDischargePort/s0:Code/text()"/>
          <xsl:choose>
            <xsl:when test="$portOfDischarge != ''">
              <xsl:value-of select="$portOfDischarge"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$shipment/s0:PortOfDischarge/s0:Code/text()"/>
            </xsl:otherwise>
          </xsl:choose>
        </LOC1>
      </ns0:LOC>
      <ns0:EQN>
        <EQN1>
          <xsl:variable name="totalPackages" select="$subShipment/s0:OuterPacks/text()"/>
          <xsl:choose>
            <xsl:when test="$totalPackages != ''">
              <xsl:value-of select="ScriptNS2:FormatDecimal($totalPackages, '0.####', false())"/>
            </xsl:when>
            <xsl:otherwise>1.0000</xsl:otherwise>
          </xsl:choose>
        </EQN1>
        <EQN2>
          <xsl:variable name="grossWeight" select="$subShipment/s0:TotalWeight/text()"/>
          <xsl:choose>
            <xsl:when test="$grossWeight != ''">
              <xsl:value-of select="ScriptNS2:FormatDecimal($grossWeight, '0.####', false())"/>
            </xsl:when>
            <xsl:otherwise>0.0100</xsl:otherwise>
          </xsl:choose>
        </EQN2>
      </ns0:EQN>
      <xsl:variable name="packLoop" select="userCSharp:GetOriginalCSTCount()"/>
      <xsl:variable name="current" select="1 + ($packLoop - 1) * 50"/>
      <xsl:variable name="packLineCount" select="count($subShipment/s0:PackingLineCollection/s0:PackingLine)"/>
      <xsl:variable name ="end">
        <xsl:choose>
          <xsl:when test="($current + 50) > $packLineCount">
            <xsl:value-of select="$packLineCount + 1"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$packLoop * 50 + 1"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="SG3Loop">
        <xsl:call-template name="SG3Loop" >
          <xsl:with-param name="current" select="$current"/>
          <xsl:with-param name="end" select="$end"/>
          <xsl:with-param name="packingLineCollection" select="$subShipment/s0:PackingLineCollection"/>
          <xsl:with-param name="mawb" select="$mawb"/>
          <xsl:with-param name="hawb" select="$hawb"/>
          <xsl:with-param name="jobNumber" select="$jobNumber"/>
          <xsl:with-param name="countryOfDestination" select="substring($subShipment/s0:PortOfDestination/s0:Code/text(),1,2)"/>
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
    <xsl:param name="current"/>
    <xsl:param name="end"/>
    <xsl:param name="packingLineCollection"/>
    <xsl:param name="mawb"/>
    <xsl:param name="hawb"/>
    <xsl:param name="jobNumber"/>
    <xsl:param name="countryOfDestination"/>

    <xsl:if test="$current &lt; $end">
      <xsl:call-template name="GenerateSG3">
        <xsl:with-param name="count" select="$current"/>
        <xsl:with-param name="packingLine" select ="$packingLineCollection/s0:PackingLine[$current]"/>
        <xsl:with-param name="mawb" select="$mawb"/>
        <xsl:with-param name="hawb" select="$hawb"/>
        <xsl:with-param name="jobNumber" select="$jobNumber"/>
        <xsl:with-param name="countryOfDestination" select="$countryOfDestination"/>
      </xsl:call-template>
      <xsl:call-template name="SG3Loop">
        <xsl:with-param name="current" select="$current + 1"/>
        <xsl:with-param name="end" select="$end"/>
        <xsl:with-param name ="packingLineCollection" select="$packingLineCollection"/>
        <xsl:with-param name="mawb" select="$mawb"/>
        <xsl:with-param name="hawb" select="$hawb"/>
        <xsl:with-param name="jobNumber" select="$jobNumber"/>
        <xsl:with-param name="countryOfDestination" select="$countryOfDestination"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name="GenerateSG3">
    <xsl:param name="count"/>
    <xsl:param name="packingLine"/>
    <xsl:param name="mawb"/>
    <xsl:param name="hawb"/>
    <xsl:param name="jobNumber"/>
    <xsl:param name="countryOfDestination"/>

    <ns0:SG3Loop>
      <ns0:SER>
        <SER1>
          <xsl:variable name="CW1ConsignmentReference" select="$packingLine/s0:AddInfoGroupCollection/s0:AddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'ConsignmentReference']/s0:Value/text() "/>
          <xsl:variable name="consignmentItemNumber" select="format-number($count, '00000')"/>
          <xsl:variable name="addToCW1ReferenceData" select="userCSharp:AddToCW1ReferenceData(concat($consignmentItemNumber,$CW1ConsignmentReference))"/>
          <xsl:value-of select="$consignmentItemNumber"/>
        </SER1>
        <SER2>
          <xsl:variable name="goodsType" select="$packingLine/s0:PackingLineCollection/s0:PackingLine/s0:AddInfoGroupCollection/s0:AddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'GoodsType']/s0:Value/text()"/>
          <xsl:choose>
            <xsl:when test="$goodsType != ''">
              <xsl:value-of select="$goodsType"/>
            </xsl:when>
            <xsl:otherwise>NT</xsl:otherwise>
          </xsl:choose>
        </SER2>
        <SER3>
          <xsl:value-of select="substring($packingLine/s0:PackingLineCollection/s0:PackingLine/s0:GoodsDescription/text(), 1, 70)"/>
        </SER3>
        <SER4>
          <xsl:variable name ="harmonisedCode" select ="$packingLine/s0:PackingLineCollection/s0:PackingLine/s0:HarmonisedCode"/>
          <xsl:if test="$harmonisedCode != ''">
            <xsl:value-of select="$harmonisedCode"/>
          </xsl:if>
        </SER4>
      </ns0:SER>

      <ns0:CTY>
        <CTY1>
          <xsl:choose>
            <xsl:when test="$countryOfDestination!=''">
              <xsl:value-of select="$countryOfDestination"/>
            </xsl:when>
            <xsl:otherwise>XX</xsl:otherwise>
          </xsl:choose>
        </CTY1>
        <CTY2>
          <xsl:variable name ="countryOfOrigin" select="$packingLine/s0:PackingLineCollection/s0:PackingLine/s0:CountryOfOrigin/s0:Code/text()"/>
          <xsl:choose>
            <xsl:when test="$countryOfOrigin!=''">
              <xsl:value-of select="$countryOfOrigin"/>
            </xsl:when>
            <xsl:otherwise>XX</xsl:otherwise>
          </xsl:choose>
        </CTY2>
      </ns0:CTY>

      <xsl:variable name="customsQty" select="$packingLine/s0:PackingLineCollection/s0:PackingLine/s0:AddInfoGroupCollection/s0:AddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'CustomsQty']/s0:Value"/>
      <xsl:variable name="customsUQ" select="$packingLine/s0:PackingLineCollection/s0:PackingLine/s0:AddInfoGroupCollection/s0:AddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'CustomsUQ']/s0:Value/text()"/>

      <ns0:MEA>
        <MEA1>
          <xsl:choose>
            <xsl:when test="$customsUQ!=''">
              <xsl:value-of select="$customsUQ"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$packingLine/s0:PackingLineCollection/s0:PackingLine/s0:PackType/s0:Code/text()"/>
            </xsl:otherwise>
          </xsl:choose>
        </MEA1>
        <MEA2>
          <xsl:choose>
            <xsl:when test="$customsQty!=''">
              <xsl:value-of select="ScriptNS2:FormatDecimal($customsQty,'0.####', false())"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="ScriptNS2:FormatDecimal($packingLine/s0:PackingLineCollection/s0:PackingLine/s0:PackQty/text(), '0.####', false())"/>
            </xsl:otherwise>
          </xsl:choose>
        </MEA2>
      </ns0:MEA>

      <ns0:MOA_2>
        <MOA1>
          <xsl:value-of select="ScriptNS2:FormatDecimal($packingLine/s0:PackingLineCollection/s0:PackingLine/s0:AddInfoGroupCollection/s0:AddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'CustomsValue']/s0:Value/text(), '0.##', false())"/>
        </MOA1>
      </ns0:MOA_2>

      <xsl:variable name="permitNum" select="$packingLine/s0:PackingLineCollection/s0:PackingLine/s0:AddInfoGroupCollection/s0:AddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'PermitNumber']/s0:Value/text()"/>
      <xsl:if test="$permitNum != ''">
        <ns0:DOC>
          <DOC1>
            <xsl:value-of select="$permitNum"/>
          </DOC1>
        </ns0:DOC>
      </xsl:if>
    </ns0:SG3Loop>
  </xsl:template>


  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[

    public XPathNodeIterator Split(string toSplit)
    {
      var doc = new XmlDocument();
      var root = doc.CreateElement("root");
      doc.AppendChild(root);

      if (toSplit != "")
      {
        foreach (var value in toSplit.Split('^'))
        {
          var child = doc.CreateElement("child");

          child.InnerText = value;
          root.AppendChild(child);
        }
      }

      return doc.CreateNavigator().Select("/*/*");
    }

    public class SubShipmentContext
    {
      public string IDT { get; set; }
      public string MAWB { get; set; }
      public string HAWB { get; set; }
      public string CST { get; set; }
    }

    System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<SubShipmentContext>> subShipmentcontextsGroupedByIDT = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<SubShipmentContext>>();         
    System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<string, System.Collections.Generic.List<SubShipmentContext>>> IDTEnumerator;
    System.Collections.Generic.IEnumerator<SubShipmentContext> subShipmentContextEnumerator;

    public void AddContext(string IDT, string HAWB, string CST)
    {
      AddContext(new SubShipmentContext() 
      {
        IDT = IDT,
        HAWB = HAWB,
        CST = CST
      });
    }

    public void AddContext(SubShipmentContext context)
    {
      if (subShipmentcontextsGroupedByIDT.ContainsKey(context.IDT))
      {
        System.Collections.Generic.List<SubShipmentContext> IDTList = subShipmentcontextsGroupedByIDT[context.IDT];
        IDTList.Add(context);
      }
      else
      {
        System.Collections.Generic.List<SubShipmentContext> subShipmentContexts = new System.Collections.Generic.List<SubShipmentContext>();
        subShipmentContexts.Add(context);
        subShipmentcontextsGroupedByIDT.Add(context.IDT,  subShipmentContexts);
      }
    }

    public bool HasNextIDT()
    {
      if (IDTEnumerator == null) IDTEnumerator = subShipmentcontextsGroupedByIDT.GetEnumerator();
      var next = IDTEnumerator.MoveNext();
      if (next)
      {
        ResetSubShipmentContextEnumerator();
      }
      return next;
    }

    public int GetTotalSubShipmentsForCurrentIDT()
    {
      return subShipmentcontextsGroupedByIDT[IDTEnumerator.Current.Key].Count;
    }

    public string ExtractCurrentIDT(int index)
    {
       return IDTEnumerator.Current.Key.Split('|')[index];
    }

    public bool HasNextSubShipmentContext()
    {
      if (subShipmentContextEnumerator == null) subShipmentContextEnumerator = subShipmentcontextsGroupedByIDT[IDTEnumerator.Current.Key].GetEnumerator();
      return subShipmentContextEnumerator.MoveNext();
    }

    public void ResetSubShipmentContextEnumerator()
    {
      subShipmentContextEnumerator = null;
    }

    public string GetHAWB()
    {
      return subShipmentContextEnumerator.Current.HAWB;
    }

    public int SubShipmentCSTTotal()
    {
      return subShipmentContextEnumerator.Current.CST.Split('|').Length;
    }

    public string ExtractOriginalCST(int index)
    {
      return subShipmentContextEnumerator.Current.CST.Split('|')[index];
    }

    int messageCSTCount = 0;

    public int GetMessageCSTCount()
    {
      return messageCSTCount;
    }

    public int IncrementMessageCSTCount()
    {
      messageCSTCount += 1;
      return messageCSTCount;
    }

    public void ResetMessageCSTCount()
    {
      messageCSTCount = 0;
    }

    int subShipmentCount = 1;

    public int IncrementSubShipmentCount()
    {
      subShipmentCount += 1;
      return subShipmentCount;
    }

    public int CheckSubShipmentCount()
    {
      return subShipmentCount;
    }

    public void ResetSubShipmentCount()
    {
      subShipmentCount = 1;
    }

    int originalCSTCount = 0;

    public int GetOriginalCSTCount()
    {
      return originalCSTCount;
    }

    public int IncrementOriginalCSTCount()
    {
      originalCSTCount += 1;
      return originalCSTCount;
    }

    public void ResetOriginalCSTCount()
    {
      originalCSTCount = 0;
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

    public string GetHAWBsForCurrentIDT()
    {
      System.Collections.Generic.IEnumerator<SubShipmentContext> enumerator = subShipmentcontextsGroupedByIDT[IDTEnumerator.Current.Key].GetEnumerator();
      System.Collections.Generic.List<string> hawbs = new System.Collections.Generic.List<string>();
      while(enumerator.MoveNext())
      {
        string hawb = enumerator.Current.HAWB;
        if (!hawbs.Contains(hawb))
        {
          hawbs.Add(hawb);
        }
      }

      return String.Join("|", hawbs.ToArray());
    }

    public string ThrowSubscriptionValueNotFound(string subscriptionValue)
    {
      throw new ArgumentException(string.Format(@"Cannot find matching subscription for '{0}'.", subscriptionValue));
    }
    
    public string ThrowAllCSTsWereErroredException()
    {
      throw new InvalidOperationException("Looks like all the CSTs in the job are in an errored state, you should not submit an AIRAEU in this case.");
    }
]]>
  </msxsl:script>
</xsl:stylesheet>

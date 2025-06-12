<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:s0="http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"
                xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
                xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1"
                xmlns:ScriptNS2="http://schemas.microsoft.com/BizTalk/2003/ScriptNS2"
                xmlns:ScriptNS4="http://schemas.microsoft.com/BizTalk/2003/ScriptNS4"
                xmlns:SubscriptionHelper="http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp"
                exclude-result-prefixes="msxsl s0 ScriptNS0 ScriptNS1 ScriptNS2 ScriptNS4 SubscriptionHelper userCSharp" version="1.0">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />

  <xsl:variable name="SenderID" select="ScriptNS2:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="RecipientID" select="ScriptNS2:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="DestinationPartyReceiverIdentifierValue">
    <xsl:if test="$RecipientID='' or $SenderID =''" >
      <xsl:value-of select="userCSharp:ThrowSubscriberNotFound()"/>
    </xsl:if>
  </xsl:variable>
  <xsl:variable name="Root" select="/s0:EFACT_31_AIRERR" />
  <xsl:variable name="MessageType" select="$Root/s0:ERR/s0:ERR1/ERR1.2/text()" />
  <!-- If $errorCode = E00 or E12, it will be the only error code in the message, so we can select the 1st ERR. -->
  <xsl:variable name="errorCode" select="//s0:ERR[1]/s0:ERR3/ERR3.1/text()" />
  <xsl:variable name="ImportOrExport">
    <xsl:choose>
      <xsl:when test="contains('AIRPCM AIRPCU', $MessageType)">I</xsl:when>
      <xsl:otherwise>E</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="IDT" select="$Root/s0:ERR/s0:ERR2" />
  <xsl:variable name="IDT1_1" select="$IDT/ERR2.1" />
  <xsl:variable name="IDT1_2" select="$IDT/ERR2.2" />
  <xsl:variable name="IDT1_3" select="$IDT/ERR2.3" />
  <xsl:variable name="MessageReferenceID" select="ScriptNS0:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', $RecipientID, '@ST_ID', 'SGCMSG', '@value', concat($IDT,$ImportOrExport), '@referenceType', 'ManifestNumber')"/>
  <xsl:variable name="Subscription" select="SubscriptionHelper:InitializeSubscription()" />

  <xsl:variable name="ExistingSubscriptionForImport">
    <xsl:choose>
      <xsl:when test="$ImportOrExport = 'I'">
        <xsl:value-of select="ScriptNS0:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID , '@recipientId', $RecipientID, '@ST_ID', 'SGCMSG', '@value', concat($MessageReferenceID, 'I'))" />
      </xsl:when>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="LoadSubscriptionForImport">
    <xsl:choose>
      <xsl:when test="$ExistingSubscriptionForImport != ''">
        <xsl:value-of select="SubscriptionHelper:LoadSubscription($Subscription, $ExistingSubscriptionForImport)"/>
      </xsl:when>
      <xsl:when test="$ImportOrExport = 'I'">
        <xsl:variable name="Throw" select="userCSharp:Throw(concat('No subscription found for ', $MessageReferenceID, 'I'))"/>
      </xsl:when>
    </xsl:choose>
  </xsl:variable>

  <xsl:template match="/">
    <xsl:if test="$ImportOrExport = 'I'">
      <xsl:variable name="SubscribeHistoryOrder" select="SubscriptionHelper:SubscribeHistoryOrder($Subscription, 'ERR', ScriptNS1:CurrentDateTimeUTC('s'), $IDT1_1, $IDT1_2, $IDT1_3)" />
    </xsl:if>
    <xsl:apply-templates select="s0:EFACT_31_AIRERR" />
    <xsl:if test="$ExistingSubscriptionForImport != '' and $errorCode != 'E00' and $errorCode != 'E12'">
      <xsl:variable name="InsertSubscription" select="ScriptNS4:InsertSubscriptionValue('SGCMSG', $SenderID, $RecipientID, concat($MessageReferenceID, 'I'), SubscriptionHelper:ToString($Subscription))"/>
    </xsl:if>
  </xsl:template>

  <xsl:template match="/s0:EFACT_31_AIRERR">
    <UniversalEvent xmlns="http://www.cargowise.com/Schemas/Universal/2012/11" version="2.0">
      <Event>
        <DataContext>
          <Workflow>
            <ActionPurpose Description="AIRERR SG ACCESS">ERR</ActionPurpose>
          </Workflow>
          <DataSource>
            <DataProvider>SGA</DataProvider>
          </DataSource>
          <DataTargetCollection>
            <DataTarget>
              <Key>
                <xsl:value-of select="$MessageReferenceID" />
              </Key>
              <Type>AsycudaManifest</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>
          <xsl:value-of select="ScriptNS1:CurrentDateTimeUTC('s')" />
        </EventTime>
        <EventType>MRR</EventType>
        <EventReference>
          <xsl:choose>
            <xsl:when test="$ImportOrExport='I'">MST=MGI</xsl:when>
            <xsl:otherwise>MST=MGE</xsl:otherwise>
          </xsl:choose>
        </EventReference>
        <IsEstimate>false</IsEstimate>
        <ContextCollection>
          <Context>
            <Type>ManifestCountry</Type>
            <Value>SG</Value>
          </Context>
          <Context>
            <Type>Version</Type>
            <Value>2</Value>
          </Context>
          <Context>
            <Type>OriginalInterchangeNumber</Type>
            <Value>
              <xsl:value-of select="s0:UNI/UNI1/text()" />
            </Value>
          </Context>
          <Context>
            <Type>MessageType</Type>
            <Value>
              <xsl:value-of select="$MessageType" />
            </Value>
          </Context>
          <Context>
            <Type>UENNumber</Type>
            <Value>
              <xsl:value-of select="s0:ERR/s0:ERR2/ERR2.1/text()" />
            </Value>
          </Context>
          <Context>
            <Type>OriginalCreateDate</Type>
            <Value>
              <xsl:value-of select="s0:ERR/s0:ERR2/ERR2.2/text()" />
            </Value>
          </Context>
          <Context>
            <Type>OriginalSerialNumber</Type>
            <Value>
              <xsl:value-of select="s0:ERR/s0:ERR2/ERR2.3/text()" />
            </Value>
          </Context>

          <xsl:choose>
            <xsl:when test="$ImportOrExport = 'I'">
              <xsl:call-template name="Import" />
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="Export" />
            </xsl:otherwise>
          </xsl:choose>

        </ContextCollection>
      </Event>
    </UniversalEvent>
  </xsl:template>

  <xsl:template name="SubscribeErrorForImport">
    <xsl:param name="ERR" />
    <xsl:param name="HAWB" />
    <xsl:param name="SGID" />
    <xsl:param name="IncludeDetailsInAllPacks" />
    <xsl:variable name="ConsignmentRef" select="@Ref"/>
    <xsl:variable name="SubscribeEvent" select="SubscriptionHelper:SubscribeEvent($Subscription, $HAWB, $ConsignmentRef, 'Error', $IDT, 'ERR')" />
    <xsl:if test="$IncludeDetailsInAllPacks = true()">
      <xsl:variable name="SubscribeErrorCode" select="SubscriptionHelper:SubscribeInfo($Subscription, $HAWB, $ConsignmentRef, $IDT, 'ERR', 'ErrorCode', $ERR/s0:ERR3/ERR3.1/text())" />
      <xsl:variable name="SubscribeErrorDescription" select="SubscriptionHelper:SubscribeInfo($Subscription, $HAWB, $ConsignmentRef, $IDT, 'ERR', 'ErrorDescription', $ERR/s0:ERR3/ERR3.2/text())" />
    </xsl:if>
    <xsl:if test="@SGID=$SGID">
      <xsl:if test="$IncludeDetailsInAllPacks = false()">
        <xsl:variable name="SubscribeErrorCode" select="SubscriptionHelper:SubscribeInfo($Subscription, $HAWB, $ConsignmentRef, $IDT, 'ERR', 'ErrorCode', $ERR/s0:ERR3/ERR3.1/text())" />
        <xsl:variable name="SubscribeErrorDescription" select="SubscriptionHelper:SubscribeInfo($Subscription, $HAWB, $ConsignmentRef, $IDT, 'ERR', 'ErrorDescription', $ERR/s0:ERR3/ERR3.2/text())" />
      </xsl:if>
      <xsl:variable name="SubscribeSegmentGroup" select="SubscriptionHelper:SubscribeInfo($Subscription, $HAWB, $ConsignmentRef, $IDT, 'ERR', 'SegmentGroup', $ERR/s0:ERR5/ERR5.1/text())" />
      <xsl:variable name="SubscribeGroupOccuranceNumber1" select="SubscriptionHelper:SubscribeInfo($Subscription, $HAWB, $ConsignmentRef, $IDT, 'ERR', 'GroupOccuranceNumber1', $ERR/s0:ERR5/ERR5.2/text())" />
      <xsl:variable name="SubscribeGroupOccuranceNumber2" select="SubscriptionHelper:SubscribeInfo($Subscription, $HAWB, $ConsignmentRef, $IDT, 'ERR', 'GroupOccuranceNumber2', $ERR/s0:ERR5/ERR5.3/text())" />
      <xsl:variable name="SubscribeGroupOccuranceNumber3" select="SubscriptionHelper:SubscribeInfo($Subscription, $HAWB, $ConsignmentRef, $IDT, 'ERR', 'GroupOccuranceNumber3', $ERR/s0:ERR5/ERR5.4/text())" />
      <xsl:variable name="SubscribeSegmentTag" select="SubscriptionHelper:SubscribeInfo($Subscription, $HAWB, $ConsignmentRef, $IDT, 'ERR', 'SegmentTag', $ERR/s0:ERR6/ERR6.1/text())" />
      <xsl:variable name="SubscribeOrdinalNumber1" select="SubscriptionHelper:SubscribeInfo($Subscription, $HAWB, $ConsignmentRef, $IDT, 'ERR', 'OrdinalNumber1', $ERR/s0:ERR6/ERR6.2/text())" />
      <xsl:variable name="SubscribeSubOrdinalNumber1" select="SubscriptionHelper:SubscribeInfo($Subscription, $HAWB, $ConsignmentRef, $IDT, 'ERR', 'SubOrdinalNumber1', $ERR/s0:ERR7/ERR7.1/text())" />
      <xsl:variable name="SubscribeSubOrdinalNumber2" select="SubscriptionHelper:SubscribeInfo($Subscription, $HAWB, $ConsignmentRef, $IDT, 'ERR', 'SubOrdinalNumber2', $ERR/s0:ERR7/ERR7.2/text())" />
    </xsl:if>
  </xsl:template>

  <xsl:template name="Import">
    <xsl:if test="$MessageType = 'AIRPCM'">
      <xsl:variable name="ErrorCount" select="count(//s0:ERR/s0:ERR3/ERR3.1[starts-with(text(), 'E')])"/>
      <xsl:choose>
        <xsl:when test="$ErrorCount > 0">
          <xsl:variable name="ERR" select="s0:ERR[1]"/>
          <xsl:for-each select="SubscriptionHelper:SelectHousesByIDT($Subscription, $IDT, 'PCM')/HAWBs/House">
            <xsl:variable name="SubscribedHAWB" select="@HAWB" />
            <xsl:for-each select="SubscriptionHelper:SelectPackLinesByIDT($Subscription, $SubscribedHAWB, $IDT, 'PCM')/Packs/Pack">
              <xsl:call-template name="SubscribeErrorForImport">
                <xsl:with-param name="ERR" select="$ERR" />
                <xsl:with-param name="HAWB" select="$SubscribedHAWB" />
                <xsl:with-param name="IncludeDetailsInAllPacks" select="true()" />
              </xsl:call-template>
            </xsl:for-each>
          </xsl:for-each>
        </xsl:when>

        <xsl:otherwise>
          <xsl:for-each select="s0:ERR">
            <xsl:variable name="ERR" select="."/>
            <xsl:variable name="HAWB" select="s0:ERR4/ERR4.2/text()"/>
            <xsl:variable name="SGID" select="s0:ERR4/ERR4.3/text()"/>
            <xsl:choose>
              <xsl:when test="$HAWB != ''">
                <xsl:for-each select="SubscriptionHelper:SelectPackLinesByIDT($Subscription, $HAWB, $IDT, 'PCM')/Packs/Pack">
                  <xsl:call-template name="SubscribeErrorForImport">
                    <xsl:with-param name="ERR" select="$ERR" />
                    <xsl:with-param name="HAWB" select="$HAWB" />
                    <xsl:with-param name="SGID" select="$SGID" />
                    <xsl:with-param name="IncludeDetailsInAllPacks" select="not($SGID) or $SGID=''" />
                  </xsl:call-template>
                </xsl:for-each>
              </xsl:when>
              <xsl:otherwise>
                <xsl:for-each select="SubscriptionHelper:SelectHousesByIDT($Subscription, $IDT, 'PCM')/HAWBs/House">
                  <xsl:variable name="SubscribedHAWB" select="@HAWB" />
                  <xsl:for-each select="SubscriptionHelper:SelectPackLinesByIDT($Subscription, $SubscribedHAWB, $IDT, 'PCM')/Packs/Pack">
                    <xsl:call-template name="SubscribeErrorForImport">
                      <xsl:with-param name="ERR" select="$ERR" />
                      <xsl:with-param name="HAWB" select="$SubscribedHAWB" />
                      <xsl:with-param name="IncludeDetailsInAllPacks" select="true()" />
                    </xsl:call-template>
                  </xsl:for-each>
                </xsl:for-each>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:for-each>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>

    <xsl:variable name="MAWB" select="$Subscription/Shipment/@MAWB" />
    <Context xmlns="http://www.cargowise.com/Schemas/Universal/2012/11">
      <Type>MasterBill</Type>
      <Value>
        <xsl:value-of select="$MAWB"/>
      </Value>
      <SubContextCollection>
        <xsl:for-each select="SubscriptionHelper:SelectHousesByIDT($Subscription, $IDT, 'ERR')/HAWBs/House">
          <SubContext>
            <Type>HouseBill</Type>
            <Value>
              <xsl:value-of select="@HAWB" />
            </Value>
            <xsl:call-template name="SubContextCollectionImport" />
          </SubContext>
          <xsl:variable name="DeleteProcessedPackLinesAndHouse" select="SubscriptionHelper:DeletePackLinesAndHouseByIDTAndHAWB($Subscription, $IDT, @HAWB)" />
        </xsl:for-each>

        <xsl:call-template name="MessageStatusCode"/>
      </SubContextCollection>
    </Context>
  </xsl:template>

  <xsl:template name="Export">
    <xsl:variable name="idtOriginalPiped">
      <xsl:if test="$MessageType='AIRAEU'">
        <xsl:value-of select="ScriptNS0:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', $RecipientID, '@ST_ID', 'SGCMSG', '@value', concat($IDT1_1, '|', $IDT1_2, '|', $IDT1_3, 'E'), '@referenceType', 'OriginalIDT')" />
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="mawbERR" select="s0:ERR/s0:ERR4/ERR4.1/text()" />
    <xsl:variable name="mawb">
      <xsl:choose>
        <xsl:when test="$mawbERR != ''">
          <xsl:value-of select="$mawbERR"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="ScriptNS0:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', $RecipientID, '@ST_ID', 'SGCMSG', '@value', concat($IDT,$ImportOrExport), '@referenceType', 'MAWB')" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:if test="$MessageType = 'AIRAEU'">
      <!--E12 will not happen to AIRAEU-->
      <xsl:if test="$errorCode != 'E00'">
        <xsl:variable name="invalidSequenceNumber" select="ScriptNS0:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', $RecipientID, '@ST_ID', 'SGCMSG', '@value', concat($idtOriginalPiped, 'E'), '@referenceType', 'UpdateNumber')" />
        <xsl:variable name="decrementSequenceNumber" select="ScriptNS4:InsertSubscriptionValue('SGCMSG', $SenderID, $RecipientID, concat($idtOriginalPiped, 'E'), number($invalidSequenceNumber) - 1, 'UpdateNumber')" />
      </xsl:if>
      <xsl:variable name="E23ERR" select="s0:ERR/s0:ERR3[ERR3.1/text()='E23']" />
      <xsl:if test="$E23ERR">
        <xsl:variable name="E23ERRDescription" select="$E23ERR/ERR3.2/text()" />
        <xsl:variable name="validSequenceNumber" select="substring($E23ERRDescription, string-length($E23ERRDescription) - 2)"/>
        <xsl:variable name="updateSequenceNumber" select="ScriptNS4:InsertSubscriptionValue('SGCMSG', $SenderID, $RecipientID, concat($idtOriginalPiped, 'E'), $validSequenceNumber, 'UpdateNumber')" />
      </xsl:if>
    </xsl:if>

    <Context xmlns="http://www.cargowise.com/Schemas/Universal/2012/11">
      <Type>MasterBill</Type>
      <Value>
        <xsl:value-of select="$mawb"/>
      </Value>
      <SubContextCollection>
        <xsl:variable name="errors" select="s0:ERR"/>
        <xsl:variable name="dictionary" select="userCSharp:PopulateHawbsErrorDictionary($errors)"/>
        <xsl:variable name="hawbsAndErrors" select="userCSharp:GetHawbsAndErrors()" />
        <xsl:for-each select="$hawbsAndErrors">
          <xsl:variable name="hawb" select="./text()" />
          <xsl:variable name="errs" select="s0:ERR" />
          <xsl:choose>
            <xsl:when test="$hawb!=''">
              <xsl:call-template name="ProcessHAWBsExport">
                <xsl:with-param name="ERRs" select="$errs"/>
                <xsl:with-param name="hawb" select="$hawb"/>
                <xsl:with-param name="mawb" select="$mawb"/>
                <xsl:with-param name="idtOriginalPiped" select="$idtOriginalPiped"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <xsl:variable name="hawbs" select="ScriptNS0:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', $RecipientID, '@ST_ID', 'SGCMSG', '@value', concat($IDT,$ImportOrExport), '@referenceType', 'HAWB')" />
              <xsl:for-each select="userCSharp:Split($hawbs)">
                <xsl:call-template name="ProcessHAWBsExport">
                  <xsl:with-param name="ERRs" select="$errs"/>
                  <xsl:with-param name="hawb" select="."/>
                  <xsl:with-param name="mawb" select="$mawb"/>
                  <xsl:with-param name="idtOriginalPiped" select="$idtOriginalPiped"/>
                </xsl:call-template>
              </xsl:for-each>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:for-each>
        <xsl:call-template name="MessageStatusCode"/>
      </SubContextCollection>
    </Context>
  </xsl:template>

  <xsl:template name="ProcessHAWBsExport">
    <xsl:param name="ERRs"/>
    <xsl:param name="hawb"/>
    <xsl:param name="mawb"/>
    <xsl:param name="idtOriginalPiped"/>

    <SubContext xmlns="http://www.cargowise.com/Schemas/Universal/2012/11">
      <Type>HouseBill</Type>
      <Value>
        <xsl:value-of select="$hawb" />
      </Value>

      <xsl:variable name="IDTPiped">
        <xsl:choose>
          <xsl:when test="$idtOriginalPiped = ''">
            <xsl:value-of select="concat($IDT1_1, '|', $IDT1_2, '|', $IDT1_3)" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$idtOriginalPiped" />
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="CSTNumbers" select="ScriptNS0:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', $RecipientID, '@ST_ID', 'SGCMSG', '@value', concat($mawb, $hawb, $MessageReferenceID, $IDTPiped, 'E'), '@referenceType', 'CST')"/>

      <xsl:for-each select="$ERRs">
        <xsl:variable name="currentError" select="." />
        <xsl:variable name="ERR43" select="./s0:ERR4/ERR4.3/text()" />
        <xsl:variable name="errorCode" select="./s0:ERR3/ERR3.1/text()" />
        <xsl:choose>

          <xsl:when test="$ERR43 != ''">
            <!--Treat ERR4.3 as SER. If SER subscription not found, treat it as CST-->
            <xsl:for-each select="userCSharp:Split($CSTNumbers)">
              <xsl:variable name="exportReferences" select="ScriptNS0:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', $RecipientID, '@ST_ID', 'SGCMSG', '@value', concat($mawb, $hawb, $MessageReferenceID, ./text(), 'E'), '@referenceType', 'CNRF')" />
              <xsl:if test="$exportReferences != '' and $exportReferences != 'AIRERR'">
                <xsl:if test="contains($exportReferences,$ERR43)">
                  <xsl:call-template name="SubContextCollectionExport">
                    <xsl:with-param name="consignmentDetails" select="$exportReferences"/>
                    <xsl:with-param name="ERR" select="$currentError"/>
                    <xsl:with-param name="SER" select="$ERR43"/>
                  </xsl:call-template>
                  <!--E00 error won't have ERR4.3 field-->
                  <xsl:if test="$errorCode != 'E12'">
                    <xsl:variable name="addToErroredCSTs" select="userCSharp:AddToErroredCSTs(./text())" />
                  </xsl:if>
                </xsl:if>
              </xsl:if>
            </xsl:for-each>

            <xsl:if test="not(userCSharp:HasErroredCSTs())">
              <xsl:variable name="exportReferences" select="ScriptNS0:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', $RecipientID, '@ST_ID', 'SGCMSG', '@value', concat($mawb, $hawb, $MessageReferenceID, $ERR43, 'E'), '@referenceType', 'CNRF')" />
              <xsl:if test="$exportReferences != '' and $exportReferences != 'AIRERR'">
                <xsl:call-template name="SubContextCollectionExport">
                  <xsl:with-param name="consignmentDetails" select="$exportReferences"/>
                  <xsl:with-param name="ERR" select="$currentError"/>
                </xsl:call-template>
                <xsl:if test="$errorCode != 'E12'">
                  <xsl:variable name="addToErroredCSTs" select="userCSharp:AddToErroredCSTs($ERR43)" />
                </xsl:if>
              </xsl:if>
            </xsl:if>
          </xsl:when>

          <xsl:otherwise>
            <xsl:for-each select="userCSharp:Split($CSTNumbers)">
              <xsl:variable name="exportReferences" select="ScriptNS0:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', $RecipientID, '@ST_ID', 'SGCMSG', '@value', concat($mawb, $hawb, $MessageReferenceID, ./text(), 'E'), '@referenceType', 'CNRF')" />
              <xsl:if test="$exportReferences != '' and $exportReferences != 'AIRERR'">
                <xsl:call-template name="SubContextCollectionExport">
                  <xsl:with-param name="consignmentDetails" select="$exportReferences"/>
                  <xsl:with-param name="ERR" select="$currentError"/>
                </xsl:call-template>
                <xsl:if test="$errorCode != 'E00' and $errorCode != 'E12'">
                  <xsl:variable name="addToErroredCSTs" select="userCSharp:AddToErroredCSTs(./text())" />
                </xsl:if>
              </xsl:if>
            </xsl:for-each>
          </xsl:otherwise>

        </xsl:choose>
      </xsl:for-each>

      <xsl:variable name="setCSTs" select="userCSharp:SetCSTs($CSTNumbers)" />
      <xsl:for-each select="userCSharp:GetErroredCSTs()">
        <xsl:variable name="updateCNRFToAIRERR" select="ScriptNS4:InsertSubscriptionValueWithRetries('SGCMSG', $SenderID, $RecipientID, concat($mawb, $hawb, $MessageReferenceID, ./text(), 'E'), 'AIRERR', 'CNRF')" />
        <xsl:variable name="removeCurrentCST" select="userCSharp:RemoveCurrentCST(./text())" />
      </xsl:for-each>
      <xsl:variable name="remainingCSTs" select="userCSharp:GetRemainingCSTs()" />
      <xsl:if test="$remainingCSTs != $CSTNumbers">
        <xsl:variable name="updateCSTSubscription" select="ScriptNS4:InsertSubscriptionValueWithRetries('SGCMSG', $SenderID, $RecipientID, concat($mawb, $hawb, $MessageReferenceID, $IDTPiped, 'E'), $remainingCSTs, 'CST')" />
      </xsl:if>
      <xsl:variable name="resetErroredCSTs" select="userCSharp:ResetErroredCSTs()" />

    </SubContext>
  </xsl:template>

  <xsl:template name="SubContextCollectionExport">
    <xsl:param name="consignmentDetails"/>
    <xsl:param name="ERR"/>
    <xsl:param name="SER"/>

    <SubContextCollection xmlns="http://www.cargowise.com/Schemas/Universal/2012/11">
      <xsl:for-each select="userCSharp:Split($consignmentDetails)">
        <SubContext>
          <Type>ConsignmentReference</Type>
          <Value>
            <xsl:value-of select="substring(./text(), 6)" />
          </Value>
          <SubContextCollection>
            <xsl:call-template name="SubContext">
              <xsl:with-param name="Key">ConsignmentNumber</xsl:with-param>
              <xsl:with-param name="Value" select="substring(./text(), 1, 5)"/>
            </xsl:call-template>
            <xsl:call-template name="MessageStatusCode"/>
            <xsl:choose>
              <xsl:when test="$SER = '' or $SER = substring(./text(), 1, 5)">
                <xsl:call-template name="SubContext">
                  <xsl:with-param name="Key">ErrorCode</xsl:with-param>
                  <xsl:with-param name="Value" select="$ERR/s0:ERR3/ERR3.1/text()"/>
                </xsl:call-template>
                <xsl:call-template name="SubContext">
                  <xsl:with-param name="Key">ErrorDescription</xsl:with-param>
                  <xsl:with-param name="Value" select="$ERR/s0:ERR3/ERR3.2/text()"/>
                </xsl:call-template>
                <xsl:call-template name="SubContext">
                  <xsl:with-param name="Key">SegmentGroup</xsl:with-param>
                  <xsl:with-param name="Value" select="$ERR/s0:ERR5/ERR5.1/text()"/>
                </xsl:call-template>
                <xsl:call-template name="SubContext">
                  <xsl:with-param name="Key">GroupOccuranceNumber1</xsl:with-param>
                  <xsl:with-param name="Value" select="$ERR/s0:ERR5/ERR5.2/text()"/>
                </xsl:call-template>
                <xsl:call-template name="SubContext">
                  <xsl:with-param name="Key">GroupOccuranceNumber2</xsl:with-param>
                  <xsl:with-param name="Value" select="$ERR/s0:ERR5/ERR5.3/text()"/>
                </xsl:call-template>
                <xsl:call-template name="SubContext">
                  <xsl:with-param name="Key">GroupOccuranceNumber3</xsl:with-param>
                  <xsl:with-param name="Value" select="$ERR/s0:ERR5/ERR5.4/text()"/>
                </xsl:call-template>
                <xsl:call-template name="SubContext">
                  <xsl:with-param name="Key">SegmentTag</xsl:with-param>
                  <xsl:with-param name="Value" select="$ERR/s0:ERR6/ERR6.1/text()"/>
                </xsl:call-template>
                <xsl:call-template name="SubContext">
                  <xsl:with-param name="Key">OrdinalNumber1</xsl:with-param>
                  <xsl:with-param name="Value" select="$ERR/s0:ERR6/ERR6.2/text()"/>
                </xsl:call-template>
                <xsl:call-template name="SubContext">
                  <xsl:with-param name="Key">SubOrdinalNumber1</xsl:with-param>
                  <xsl:with-param name="Value" select="$ERR/s0:ERR7/ERR7.1/text()"/>
                </xsl:call-template>
                <xsl:call-template name="SubContext">
                  <xsl:with-param name="Key">SubOrdinalNumber2</xsl:with-param>
                  <xsl:with-param name="Value" select="$ERR/s0:ERR7/ERR7.2/text()"/>
                </xsl:call-template>
              </xsl:when>
            </xsl:choose>
          </SubContextCollection>
        </SubContext>
      </xsl:for-each>
      <xsl:call-template name="MessageStatusCode"/>
    </SubContextCollection>
  </xsl:template>

  <xsl:template name="SubContextCollectionImport">
    <xsl:variable name="HAWB" select="@HAWB"/>
    <xsl:variable name="IDTRef" select="SubscriptionHelper:SelectIDTRef($Subscription, $IDT, 'ERR')"/>
    <SubContextCollection xmlns="http://www.cargowise.com/Schemas/Universal/2012/11">
      <xsl:for-each select="SubscriptionHelper:SelectPackLinesByIDT($Subscription, $HAWB, $IDT, 'ERR')/Packs/Pack">
        <xsl:variable name="ConsignmentReference" select="@Ref"/>
        <xsl:variable name="ConsignmentNumber" select="@SGID"/>
        <xsl:for-each select="History/Event[@IDTRef=$IDTRef]">
          <xsl:call-template name="ErrorSubContextImport">
            <xsl:with-param name="ConsignmentReference" select="$ConsignmentReference"/>
            <xsl:with-param name="ConsignmentNumber" select="$ConsignmentNumber"/>
          </xsl:call-template>
        </xsl:for-each>
      </xsl:for-each>
      <xsl:if test="count(SubscriptionHelper:SelectPackLinesByStatus($Subscription, $IDT, $HAWB, 'ERR', 'Error')/Packs/Pack)!=0">
        <xsl:call-template name="MessageStatusCode"/>
      </xsl:if>
    </SubContextCollection>
  </xsl:template>

  <xsl:template name="ErrorSubContextImport">
    <xsl:param name="ConsignmentReference"/>
    <xsl:param name="ConsignmentNumber"/>

    <SubContext xmlns="http://www.cargowise.com/Schemas/Universal/2012/11">
      <Type>ConsignmentReference</Type>
      <Value>
        <xsl:value-of select="$ConsignmentReference" />
      </Value>
      <SubContextCollection>
        <xsl:call-template name="SubContext">
          <xsl:with-param name="Key">ConsignmentNumber</xsl:with-param>
          <xsl:with-param name="Value" select="$ConsignmentNumber"/>
        </xsl:call-template>
        <xsl:call-template name="MessageStatusCode"/>
        <xsl:call-template name="SubContext">
          <xsl:with-param name="Key">ErrorCode</xsl:with-param>
          <xsl:with-param name="Value" select="Info/ErrorCode/text()"/>
        </xsl:call-template>
        <xsl:call-template name="SubContext">
          <xsl:with-param name="Key">ErrorDescription</xsl:with-param>
          <xsl:with-param name="Value" select="Info/ErrorDescription/text()"/>
        </xsl:call-template>
        <xsl:call-template name="SubContext">
          <xsl:with-param name="Key">SegmentGroup</xsl:with-param>
          <xsl:with-param name="Value" select="Info/SegmentGroup/text()"/>
        </xsl:call-template>
        <xsl:call-template name="SubContext">
          <xsl:with-param name="Key">GroupOccuranceNumber1</xsl:with-param>
          <xsl:with-param name="Value" select="Info/GroupOccuranceNumber1/text()"/>
        </xsl:call-template>
        <xsl:call-template name="SubContext">
          <xsl:with-param name="Key">GroupOccuranceNumber2</xsl:with-param>
          <xsl:with-param name="Value" select="Info/GroupOccuranceNumber2/text()"/>
        </xsl:call-template>
        <xsl:call-template name="SubContext">
          <xsl:with-param name="Key">GroupOccuranceNumber3</xsl:with-param>
          <xsl:with-param name="Value" select="Info/GroupOccuranceNumber3/text()"/>
        </xsl:call-template>
        <xsl:call-template name="SubContext">
          <xsl:with-param name="Key">SegmentTag</xsl:with-param>
          <xsl:with-param name="Value" select="Info/SegmentTag/text()"/>
        </xsl:call-template>
        <xsl:call-template name="SubContext">
          <xsl:with-param name="Key">OrdinalNumber1</xsl:with-param>
          <xsl:with-param name="Value" select="Info/OrdinalNumber1/text()"/>
        </xsl:call-template>
        <xsl:call-template name="SubContext">
          <xsl:with-param name="Key">SubOrdinalNumber1</xsl:with-param>
          <xsl:with-param name="Value" select="Info/SubOrdinalNumber1/text()"/>
        </xsl:call-template>
        <xsl:call-template name="SubContext">
          <xsl:with-param name="Key">SubOrdinalNumber2</xsl:with-param>
          <xsl:with-param name="Value" select="Info/SubOrdinalNumber2/text()"/>
        </xsl:call-template>
      </SubContextCollection>
    </SubContext>
  </xsl:template>

  <xsl:template name="SubContext">
    <xsl:param name="Key"/>
    <xsl:param name="Value"/>
    <xsl:if test="$Value != ''">
      <SubContext xmlns="http://www.cargowise.com/Schemas/Universal/2012/11">
        <Type>
          <xsl:value-of select="$Key" />
        </Type>
        <Value>
          <xsl:value-of select="$Value" />
        </Value>
      </SubContext>
    </xsl:if>
  </xsl:template>

  <xsl:template name="MessageStatusCode">
    <SubContext xmlns="http://www.cargowise.com/Schemas/Universal/2012/11">
      <Type>MessageStatusCode</Type>
      <Value>ERR</Value>
    </SubContext>
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
        foreach (var value in toSplit.Split('|'))
        {
          var child = doc.CreateElement("child");

          child.InnerText = value;
          root.AppendChild(child);
        }
      }

      return doc.CreateNavigator().Select("/*/*");
    }

    public XPathNodeIterator GetHawbsAndErrors()
    {
      XmlDocument doc = new XmlDocument();
      var collection = doc.CreateElement("collection");
      doc.AppendChild(collection);
      var hawbNavigator = doc.CreateNavigator();
      hawbNavigator.MoveToChild("collection", "");

      foreach (string key in HawbsErrorDictionary.Keys)
      {
        hawbNavigator.AppendChildElement(hawbNavigator.Prefix, "Hawb", hawbNavigator.LookupNamespace(hawbNavigator.Prefix), key);
        hawbNavigator.MoveToFirstChild();
        while (hawbNavigator.MoveToNext()) { }

        foreach (XPathNavigator error in HawbsErrorDictionary[key].Errors)
        {
          hawbNavigator.AppendChild(error);
        }
        hawbNavigator.MoveToParent();
      }
      return hawbNavigator.Select("/*/*");
    }

    public void PopulateHawbsErrorDictionary(XPathNodeIterator errors)
    {
      XmlNamespaceManager manager = new XmlNamespaceManager(new NameTable());
      manager.AddNamespace("s0", "http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006");

      while (errors.MoveNext())
      {
        var currentErr = errors.Current;
        var hawbErrNode = currentErr.SelectSingleNode("s0:ERR4/ERR4.2/text()", manager);
        var hawbErr = hawbErrNode == null ? string.Empty : hawbErrNode.Value;
        if (HawbsErrorDictionary.ContainsKey(hawbErr))
        {
          var errorList = HawbsErrorDictionary[hawbErr].Errors;
          if (!errorList.Contains(currentErr))
          {
            errorList.Add(currentErr);
          }
        }
        else
        {
          var errorList = new ErrorList();
          errorList.Errors.Add(currentErr);
          HawbsErrorDictionary.Add(hawbErr, errorList);
        }
      }
    }

    System.Collections.Generic.Dictionary<string, ErrorList> HawbsErrorDictionary = new System.Collections.Generic.Dictionary<string, ErrorList>();

    string CSTs = null;

    public void SetCSTs(string value)
    {
      CSTs = value;
    }

    public void RemoveCurrentCST(string toRemove)
    {
      var startIndex = CSTs.IndexOf(toRemove);
      if (startIndex != -1)
      {
        var length = 5;
        if (startIndex >= 1 && CSTs[startIndex - 1] == '|')
        {
          startIndex--;
          length++;
        }
        else if (startIndex < (CSTs.Length - 5) && CSTs[startIndex + 5] == '|')
        {
          length++;
        }

        CSTs = CSTs.Remove(startIndex, length);
      }
    }
    
    public string GetRemainingCSTs()
    {
      return CSTs;
    }

    System.Collections.Generic.List<string> ErroredCSTs = new System.Collections.Generic.List<string>();

    public void AddToErroredCSTs(string toAdd)
    {
      if (!ErroredCSTs.Contains(toAdd))
      {
        ErroredCSTs.Add(toAdd);
      }
    }

    public bool HasErroredCSTs()
    {
      return ErroredCSTs.Count > 0;
    }

    public XPathNodeIterator GetErroredCSTs()
    {
      var doc = new XmlDocument();
      var root = doc.CreateElement("root");
      doc.AppendChild(root);

      foreach (var erroredCST in ErroredCSTs)
      {
        var child = doc.CreateElement("child");
        child.InnerText = erroredCST;
        root.AppendChild(child);
      }

      return doc.CreateNavigator().Select("/*/*");
    }

    public void ResetErroredCSTs()
    {
      ErroredCSTs.Clear();
    }

    class ErrorList
    {
      public System.Collections.Generic.List<XPathNavigator> Errors = new System.Collections.Generic.List<XPathNavigator>();
    }

    public void Throw(string message)
    {
      throw new ArgumentException(message);
    }

   public void ThrowSubscriberNotFound()
    {
      throw new ArgumentException("Unable to resolve the recipient_ Cannot found subscriber at mapping AIRERR");
    }
    ]]>
  </msxsl:script>
</xsl:stylesheet>

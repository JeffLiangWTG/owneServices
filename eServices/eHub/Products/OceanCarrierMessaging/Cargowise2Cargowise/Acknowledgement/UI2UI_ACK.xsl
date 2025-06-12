<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 CodeMapper ContextAccessor DataModelAccessor OCMHelper SubscriptionHelper i" version="1.0"
                xmlns:i="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2012/11/Acknowledgement/1"
                xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2012/11"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
                xmlns:DataModelAccessor="http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"
                xmlns:OCMHelper="http://schemas.microsoft.com/BizTalk/2003/OCMHelper"
                xmlns:SubscriptionHelper="http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />

  <xsl:template match="/">
    <xsl:apply-templates select="//*[local-name()='UniversalShipment']/*[local-name()='Shipment']" />
    <xsl:apply-templates select="//*[local-name()='UniversalEvent']/*[local-name()='Event']" />
  </xsl:template>

  <xsl:template match="//*[local-name()='UniversalShipment']/*[local-name()='Shipment']">
    <xsl:variable name="hirReference" select="s0:AdditionalReferenceCollection/s0:AdditionalReference[s0:Type/text()='HIR']/s0:ReferenceNumber/text()" />
    <xsl:variable name="purpose" select="s0:DataContext/s0:DocumentaryOverride/s0:Purpose/text()" />
    <xsl:variable name="documentName" select="s0:DataContext/s0:DocumentaryOverride/s0:DocumentName/text()" />
    <xsl:variable name="eventTime" select="s0:DataContext/s0:Workflow/s0:TriggerDate/text()" />
    <xsl:variable name="reason" select="s0:Note[s0:Description/text()='Reason for Rejection']/s0:NoteText/text()" />

    <xsl:call-template name="CreateUniversalEvent">
      <xsl:with-param name="hirReference" select="$hirReference" />
      <xsl:with-param name="purpose" select="$purpose" />
      <xsl:with-param name="documentName" select="$documentName" />
      <xsl:with-param name="eventTime" select="$eventTime" />
      <xsl:with-param name="reason" select="$reason" />
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="//*[local-name()='UniversalEvent']/*[local-name()='Event']">
    <xsl:variable name="hirReference">
      <xsl:call-template name="GetValue">
        <xsl:with-param name="value" select="i:ContextCollection/i:Context[i:Type/text()='eHub Interchange Reference']/i:Value/text()" />
        <xsl:with-param name="falbackValue" select="ns0:ContextCollection/ns0:Context[ns0:Type/text()='eHub Interchange Reference']/ns0:Value/text()" />
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="purpose2011" select="i:ContextCollection/i:Context[i:Type/text()='ProcessingResultStatus']/i:Value/text()" />
    <xsl:variable name="purpose">
      <xsl:call-template name="GetValue">
        <xsl:with-param name="value" select="i:ContextCollection/i:Context[i:Type/text()='ProcessingResultStatus']/i:Value/text()" />
        <xsl:with-param name="falbackValue" select="ns0:ContextCollection/ns0:Context[ns0:Type/text()='ProcessingResultStatus']/ns0:Value/text()" />
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="documentName">
      <xsl:call-template name="GetValue">
        <xsl:with-param name="value" select="i:ContextCollection/i:Context[i:Type/text()='DocumentName']/i:Value/text()" />
        <xsl:with-param name="falbackValue" select="ns0:ContextCollection/ns0:Context[ns0:Type/text()='DocumentName']/ns0:Value/text()" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="eventTime">
      <xsl:call-template name="GetValue">
        <xsl:with-param name="value" select="i:EventTime/text()" />
        <xsl:with-param name="falbackValue" select="ns0:EventTime/text()" />
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="reason">
      <xsl:call-template name="GetValue">
        <xsl:with-param name="value" select="OCMHelper:GetUEventProcessLog(i:ContextCollection/i:Context[i:Type/text()='DataImportLog']/i:Value/text())" />
        <xsl:with-param name="falbackValue" select="OCMHelper:GetUEventProcessLog(ns0:ContextCollection/ns0:Context[ns0:Type/text()='DataImportLog']/ns0:Value/text())" />
      </xsl:call-template>
    </xsl:variable>

    <xsl:call-template name="CreateUniversalEvent">
      <xsl:with-param name="hirReference" select="$hirReference" />
      <xsl:with-param name="purpose" select="$purpose" />
      <xsl:with-param name="documentName" select="$documentName" />
      <xsl:with-param name="eventTime" select="$eventTime" />
      <xsl:with-param name="reason" select="$reason" />
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="CreateUniversalEvent">
    <xsl:param name="hirReference" />
    <xsl:param name="purpose" />
    <xsl:param name="documentName" />
    <xsl:param name="eventTime" />
    <xsl:param name="reason" />

    <xsl:variable name="SenderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
    <xsl:variable name="DestinationParty" select="ContextAccessor:GetContextProperty('DestinationParty','http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
    <xsl:variable name="OCMBEClientID" select="SubscriptionHelper:GetOCMBEClientID($SenderID, $DestinationParty)" />
    <xsl:if test="$OCMBEClientID != ''">
      <xsl:variable name="setDestinationParty" select="ContextAccessor:SetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties', $OCMBEClientID)"/>
    </xsl:if>

    <xsl:variable name="validateSubscription" select="SubscriptionHelper:ThrowMissingValueException($SenderID, 'CW1MSG', 'HIR Reference', $hirReference)" />
    <xsl:variable name="SelectSubscriptionsByValue" select="SubscriptionHelper:SelectSubscriptionsByValue('CW1MSG', $hirReference, 'JobNumber')" />
    <xsl:variable name="RecipientID" select="SubscriptionHelper:GetSubscriber()" />
    <xsl:variable name="ProviderID" select="SubscriptionHelper:GetProvider()" />

    <xsl:variable name="subscribeJobNumber" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $ProviderID, '@recipientId', '' , '@ST_ID', 'CW1MSG', '@value', $hirReference, '@referenceType', 'JobNumber')" />
    <xsl:variable name="subscribeShipmentType" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $ProviderID, '@recipientId', '' , '@ST_ID', 'CW1MSG', '@value', $hirReference, '@referenceType', 'ShipmentType')" />
    <xsl:variable name="subscribeActionPurpose" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $ProviderID, '@recipientId', '' , '@ST_ID', 'CW1MSG', '@value', $hirReference, '@referenceType', 'ActionPurpose')" />
    <xsl:variable name="subscribeForwardingType" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $ProviderID, '@recipientId', '' , '@ST_ID', 'CW1MSG', '@value', $hirReference, '@referenceType', 'ForwardingType')" />
    <xsl:variable name="eventType" select="CodeMapper:GetRecipientCode('CARGOWISE', 'CARGOWISE', 'CARGOWISE Provider Configuration', 'Event Type', 'Event Type', 'Acknowledgement', $subscribeActionPurpose, $purpose)" />

    <xsl:variable name="consolNumber">
      <xsl:choose>
        <xsl:when test="contains($subscribeJobNumber, '_')">
          <xsl:value-of select="substring-before($subscribeJobNumber, '_')"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$subscribeJobNumber"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="containerNumber" select="substring-after($subscribeJobNumber, '_')" />

    <ns0:UniversalInterchangeInclude>
      <ns0:Header>
        <ns0:SenderID>
          <xsl:value-of select="$ProviderID"/>
        </ns0:SenderID>
        <ns0:RecipientID>
          <xsl:value-of select="$RecipientID"/>
        </ns0:RecipientID>
      </ns0:Header>

      <ns0:Body>
        <xsl:if test="$eventType!=''">
          <ns0:UniversalEvent>
            <ns0:Event>
              <ns0:DataContext>
                <ns0:DocumentaryOverride>
                  <ns0:DocumentName>
                    <xsl:value-of select="$documentName" />
                  </ns0:DocumentName>
                </ns0:DocumentaryOverride>
                <ns0:DataTargetCollection>
                  <ns0:DataTarget>
                    <ns0:Key>
                      <xsl:value-of select="$consolNumber"/>
                    </ns0:Key>
                    <ns0:Type>
                      <xsl:choose>
                        <xsl:when test="$subscribeForwardingType!=''">
                          <xsl:value-of select="$subscribeForwardingType"/>
                        </xsl:when>
                        <xsl:otherwise>ForwardingConsol</xsl:otherwise>
                      </xsl:choose>
                    </ns0:Type>
                  </ns0:DataTarget>
                </ns0:DataTargetCollection>
              </ns0:DataContext>
              <ns0:EventTime>
                <xsl:value-of select="$eventTime"/>
              </ns0:EventTime>
              <ns0:EventType>
                <xsl:value-of select="$eventType" />
              </ns0:EventType>
              <ns0:EventParameters>
                <ns0:Department>
                  <xsl:choose>
                    <xsl:when test="contains('Consolidation Advice, Cargo Receipt Advice', $documentName)">Booking Party</xsl:when>
                    <xsl:otherwise>Carrier</xsl:otherwise>
                  </xsl:choose>
                </ns0:Department>
                <ns0:MessageType>
                  <xsl:value-of select="$documentName" />
                </ns0:MessageType>
                <xsl:if test="$containerNumber != ''">
                  <ns0:EquipmentReferenceNumber>
                    <xsl:value-of select="$containerNumber"/>
                  </ns0:EquipmentReferenceNumber>
                </xsl:if>
                <xsl:if test="$eventType='IRJ' or $eventType='MRJ' or $eventType='MWA' or $eventType='IRA'">
                  <ns0:Reason>
                    <xsl:value-of select="$reason"/>
                  </ns0:Reason>
                </xsl:if>
              </ns0:EventParameters>
            </ns0:Event>
          </ns0:UniversalEvent>
        </xsl:if>
      </ns0:Body>
    </ns0:UniversalInterchangeInclude>
  </xsl:template>

  <xsl:template name="GetValue">
    <xsl:param name="value" />
    <xsl:param name="falbackValue" />
    <xsl:choose>
      <xsl:when test="$value!=''">
        <xsl:value-of select="$value"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$falbackValue"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>

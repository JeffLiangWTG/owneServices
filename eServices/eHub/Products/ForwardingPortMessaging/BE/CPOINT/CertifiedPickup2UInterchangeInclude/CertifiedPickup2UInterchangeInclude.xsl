<?xml version="1.0" encoding="Windows-1252"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
        xmlns:msxsl="urn:schemas-microsoft-com:xslt"
        exclude-result-prefixes="msxsl ns0 s0 CodeMapper ContextAccessor DataModelAccessor DateMapper"
        version="1.0"
        xmlns:s0="http://cargowise.com/ehub/core/2011/02"
        xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2012/11"
        xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
        xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
        xmlns:DataModelAccessor="http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"
        xmlns:DateMapper="http://schemas.microsoft.com/BizTalk/2003/DateMapper">

  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes"/>

  <xsl:template match="/">
    <xsl:apply-templates select="CertifiedPickUpResponse"/>
  </xsl:template>

  <xsl:variable name="SenderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')" />
  <xsl:variable name="serviceProviderID" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE', 'FORWARDING_PORT_MESSAGE', 'FPM System Configuration', 'Port Settings', 'ID', $SenderID)"/>
  <xsl:variable name="senderName" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE', 'FORWARDING_PORT_MESSAGE', 'FPM System Configuration', 'Port Settings', 'Name', $SenderID)"/>
  <xsl:variable name="senderMessageID" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE', 'FORWARDING_PORT_MESSAGE', 'FPM System Configuration', 'Port Settings', 'MSGID', $SenderID)"/>
  <xsl:variable name="documentIdentifier" select="CertifiedPickUpResponse/externalReferenceId/text()"/>

  <xsl:template match="CertifiedPickUpResponse">

    <xsl:variable name="isNotificationMessage">
      <xsl:choose>
        <xsl:when test="event/text() = 'NotValidated'">FALSE</xsl:when>
        <xsl:otherwise>TRUE</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="type" select="type/text()"/>
    <xsl:variable name="event" select="event/text()"/>

    <xsl:variable name="receiverId" select="receiverId/text()"/>
    <xsl:variable name="subscribedReceiverId" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedClients', '', '@senderId', $SenderID, '@ST_ID', $serviceProviderID, '@value', $receiverId)" />
    <xsl:if test="$subscribedReceiverId != ''">
      <xsl:variable name="SetDestinationParty" select="ContextAccessor:SetContextProperty('DestinationParty','http://schemas.microsoft.com/BizTalk/2003/system-properties', $subscribedReceiverId)"/>
    </xsl:if>

    <xsl:variable name="actionType" select="body/actionType/text()"/>
    <xsl:variable name="subscribedDocumentName">
      <xsl:if test="$documentIdentifier!=''">
        <xsl:value-of select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $senderMessageID, '@value', $documentIdentifier, '@referenceType', 'DocumentName')" />
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="documentName">
      <xsl:choose>
        <xsl:when test="$subscribedDocumentName!=''">
          <xsl:value-of select="$subscribedDocumentName"/>
        </xsl:when>
        <xsl:when test="$actionType = 'Accept' or $actionType = 'Decline'">Certified Pickup - Transfer</xsl:when>
        <xsl:otherwise>Certified Pickup - ReleaseRight</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <UniversalInterchangeInclude xmlns="http://www.cargowise.com/Schemas/Universal/2012/11">
      <Body>
        <UniversalEvent xmlns="http://www.cargowise.com/Schemas/Universal/2012/11">
          <Event>
            <DataContext>
              <DocumentaryOverride>
                <DocumentName>
                  <xsl:value-of select="$documentName"/>
                </DocumentName>
              </DocumentaryOverride>
              <xsl:if test="$documentIdentifier != ''">
                <xsl:variable name="subscribedJobNumber" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $senderMessageID, '@value', $documentIdentifier, '@referenceType', 'CPU')" />
                <xsl:if test="$subscribedJobNumber != ''">
                  <DataTargetCollection>
                    <DataTarget>
                      <Key>
                        <xsl:choose>
                          <xsl:when test="contains($subscribedJobNumber,'_')">
                            <xsl:value-of select="substring-before($subscribedJobNumber, '_')"/>
                          </xsl:when>
                          <xsl:otherwise>
                            <xsl:value-of select="$subscribedJobNumber"/>
                          </xsl:otherwise>
                        </xsl:choose>
                      </Key>
                      <Type>
                        <xsl:value-of select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $senderMessageID, '@value', $documentIdentifier, '@referenceType', 'ForwardingType')" />
                      </Type>
                    </DataTarget>
                  </DataTargetCollection>
                </xsl:if>
              </xsl:if>
            </DataContext>
            <EventTime>
              <xsl:value-of select="DateMapper:ConvertXmlDateString(timestamp/text(), 'yyyy-MM-ddThh:mm:ss')"/>
            </EventTime>

            <xsl:variable name="location">
              <xsl:choose>
                <xsl:when test="$documentIdentifier != ''">
                  <xsl:value-of select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $senderMessageID, '@value', $documentIdentifier, '@referenceType', 'OperationPort')" />
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="body/portLoCode/text()"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>

            <xsl:variable name="InboxPK" select="ContextAccessor:GetContextProperty('InternalTrackingID', 'http://cargowise.com/ehub/tracking/2010/06')"/>

            <xsl:choose>
              <xsl:when test="$isNotificationMessage = 'TRUE'">
                <xsl:variable name="equipmentNumber" select="body/equipmentNumber/text()"/>
                <xsl:variable name="releaseFromId" select="body/releaseFrom/nxtEntityId/text()"/>
                <xsl:variable name="receiverIdIsReleaseFromParty">
                  <xsl:choose>
                    <xsl:when test="receiverId/text() = $releaseFromId">TRUE</xsl:when>
                    <xsl:otherwise>FALSE</xsl:otherwise>
                  </xsl:choose>
                </xsl:variable>

                <xsl:variable name="eventType" select="CodeMapper:GetRecipientCode($senderName, $senderName, concat($senderName, ' System Configuration'), 'Notification Message', 'Event Type', $actionType, $event, $type, $receiverIdIsReleaseFromParty)"/>

                <xsl:variable name="messageType">
                  <xsl:choose>
                      <xsl:when test="contains('MAA,MPP,MWA,MRJ,ATH,ATW', $eventType)">
                        <xsl:value-of select="$documentName"/>
                      </xsl:when>
                      <xsl:otherwise></xsl:otherwise>
                  </xsl:choose>
                </xsl:variable>
                <xsl:variable name="referenceNumber">
                  <xsl:choose>
                      <xsl:when test="contains('ATH,ATW', $eventType)">
                        <xsl:value-of select="body/releaseIdentification/text()"/>
                      </xsl:when>
                      <xsl:otherwise></xsl:otherwise>
                  </xsl:choose>
                </xsl:variable>
                <xsl:variable name="SubscribeNXPTRC1" select="DataModelAccessor:InsertSubscriptionValue('NXPTRC', $subscribedReceiverId, $SenderID, $InboxPK, concat($messageType, '_', $referenceNumber, '_', body/billOfLadingNumbers/text(), '_', $equipmentNumber), $eventType)" />

                <EventType>
                  <xsl:value-of select="$eventType"/>
                </EventType>
                <EventParameters>
                  <Department>Terminal</Department>
                  <EquipmentReferenceNumber>
                    <xsl:value-of select="$equipmentNumber"/>
                  </EquipmentReferenceNumber>
                  <xsl:if test="contains('ATH,ATW', $eventType)">
                    <Type>Container Release</Type>
                    <ReferenceNumber>
                      <xsl:value-of select="body/releaseIdentification/text()"/>
                    </ReferenceNumber>
                    <Facility>CTO</Facility>
                  </xsl:if>
                  <xsl:if test="contains('MAA,MPP,MWA,MRJ,ATH,ATW', $eventType)">
                    <MessageType>
                      <xsl:value-of select="$documentName"/>
                    </MessageType>
                  </xsl:if>
                  <Status>
                    <xsl:value-of select="$event"/>
                  </Status>
                  <xsl:if test="$location != ''">
                    <Location>
                      <xsl:value-of select="$location"/>
                    </Location>
                  </xsl:if>
                </EventParameters>
                <ContextCollection>
                  <xsl:call-template name="CreateContext">
                    <xsl:with-param name="type" select="'ReleaseFromParty'"/>
                    <xsl:with-param name="value" select="body/releaseFrom/name/text()"/>
                  </xsl:call-template>
                  <xsl:call-template name="CreateContext">
                    <xsl:with-param name="type" select="'ReleaseFromPartyId'"/>
                    <xsl:with-param name="value" select="'nxtEntityId'"/>
                  </xsl:call-template>
                  <xsl:call-template name="CreateContext">
                    <xsl:with-param name="type" select="'ReleaseFromPartyCode'"/>
                    <xsl:with-param name="value" select="$releaseFromId"/>
                  </xsl:call-template>
                  <xsl:call-template name="CreateContext">
                    <xsl:with-param name="type" select="'CarrierCode'"/>
                    <xsl:with-param name="value" select="body/carrier/nxtEntityId/text()"/>
                  </xsl:call-template>
                  <xsl:call-template name="CreateContext">
                    <xsl:with-param name="type" select="'ContainerNumber'"/>
                    <xsl:with-param name="value" select="$equipmentNumber"/>
                  </xsl:call-template>
                  <xsl:call-template name="CreateContext">
                    <xsl:with-param name="type" select="'MBOLNumber'"/>
                    <xsl:with-param name="value" select="body/billOfLadingNumbers/text()"/>
                  </xsl:call-template>
                </ContextCollection>
              </xsl:when>
              <xsl:otherwise>
                <xsl:variable name="SubscribeNXPTRC2" select="DataModelAccessor:InsertSubscriptionValue('NXPTRC', $subscribedReceiverId, $SenderID, $InboxPK, concat($documentName, '_', '', '_', '', '_', id/text()), 'MRJ')" />
                <EventType>MRJ</EventType>
                <EventParameters>
                  <Department>Terminal</Department>
                  <MessageType>
                    <xsl:value-of select="$documentName"/>
                  </MessageType>
                  <EquipmentReferenceNumber>
                    <xsl:value-of select="id/text()"/>
                  </EquipmentReferenceNumber>
                  <xsl:if test="$documentIdentifier!=''">
                    <Status>
                      <xsl:value-of select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $senderMessageID, '@value', $documentIdentifier, '@referenceType', 'Purpose')" />
                    </Status>
                  </xsl:if>
                  <Reason>
                    <xsl:value-of select="body/text()"/>
                  </Reason>
                  <xsl:if test="$location != ''">
                    <Location>
                      <xsl:value-of select="$location"/>
                    </Location>
                  </xsl:if>
                </EventParameters>
                <ContextCollection>
                  <xsl:call-template name="CreateContext">
                    <xsl:with-param name="type" select="'ContainerNumber'"/>
                    <xsl:with-param name="value" select="id/text()"/>
                  </xsl:call-template>
                </ContextCollection>
              </xsl:otherwise>
            </xsl:choose>
          </Event>
        </UniversalEvent>
      </Body>
    </UniversalInterchangeInclude>
  </xsl:template>

  <xsl:template name="CreateContext">
    <xsl:param name="type"/>
    <xsl:param name="value"/>
    <xsl:if test="$value!=''">
      <Context xmlns="http://www.cargowise.com/Schemas/Universal/2012/11">
        <Type>
          <xsl:value-of select="$type"/>
        </Type>
        <Value>
          <xsl:value-of select="$value"/>
        </Value>
      </Context>
    </xsl:if>
  </xsl:template>
</xsl:stylesheet>

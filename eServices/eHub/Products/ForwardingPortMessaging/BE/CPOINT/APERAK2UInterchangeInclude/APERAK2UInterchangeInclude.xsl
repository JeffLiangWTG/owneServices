<?xml version="1.0" encoding="Windows-1252"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl ns0 s0 CodeMapper ContextAccessor DataModelAccessor DateMapper" version="1.0"
                xmlns:s0="urn:PCM"
                xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2012/11"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
                xmlns:DataModelAccessor="http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"
                xmlns:DateMapper="http://schemas.microsoft.com/BizTalk/2003/DateMapper">

  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />

  <xsl:template match="/">
    <xsl:apply-templates select="s0:APERAK" />
  </xsl:template>

  <xsl:variable name="SenderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')" />
  <xsl:variable name="RecipientID" select="ContextAccessor:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>

  <xsl:template match="s0:APERAK">
    <UniversalInterchangeInclude xmlns="http://www.cargowise.com/Schemas/Universal/2012/11">
      <Body>
        <xsl:variable name="serviceProvider" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Port Settings' , 'Name', $SenderID)"/>
        <xsl:variable name="msgID" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Port Settings' , 'MSGID', $SenderID)"/>
        <xsl:variable name="associationAssignedCode" select="s0:MessageHeader/s0:MessageIdentifier/s0:AssociationAssignedCode/text()" />
        <xsl:variable name="eblDocumentIdentifier" select="s0:GroupDocumentMessageDetails/s0:DocumentMessageDetails/s0:DocumentIdentifier/text()"/>

        <xsl:variable name="messageReference">
          <xsl:choose>
            <xsl:when test="$eblDocumentIdentifier!=''">
              <xsl:value-of select="$eblDocumentIdentifier"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="s0:GroupDocumentMessageDetails/s0:DocumentMessageDetails/s0:DocumentMessageDetails/s0:DocumentIdentifier/text()"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <xsl:variable name="documentIdentifier">
          <xsl:variable name="messageReferenceByCheckPoint" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $msgID, '@value', $messageReference, '@referenceType', 'CheckPoint')" />
          <xsl:choose>
            <xsl:when test="$messageReferenceByCheckPoint != ''">
              <xsl:value-of select="$messageReferenceByCheckPoint"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$messageReference"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <xsl:variable name="subscribedDocumentName" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $msgID, '@value', $documentIdentifier, '@referenceType', 'DocumentName')" />

        <xsl:variable name="subsirevedJobNumberIFTDGN" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $msgID, '@value', $documentIdentifier, '@referenceType', 'IFTDGN')" />
        <xsl:variable name="subscribedJobNumber">
          <xsl:choose>
            <xsl:when test="$subsirevedJobNumberIFTDGN != ''">
              <xsl:value-of select="$subsirevedJobNumberIFTDGN"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:variable name="subsirevedJobNumberEBADEC" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $msgID, '@value', $documentIdentifier, '@referenceType', 'EBADEC')" />
              <xsl:value-of select="$subsirevedJobNumberEBADEC"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:variable name="subscribedForwardingType" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $msgID, '@value', $documentIdentifier, '@referenceType', 'ForwardingType')" />

        <xsl:variable name="subscribedActionPurpose" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $msgID, '@value', $documentIdentifier, '@referenceType', 'Purpose')" />

        <xsl:variable name="responseTypeCode" select="s0:BeginningOfMessage/s0:ResponseTypeCode/text()" />
        <xsl:variable name="eventType" select="CodeMapper:GetRecipientCode($serviceProvider, $serviceProvider, concat($serviceProvider, ' System Configuration'), 'Event Type' , 'Event Type', $responseTypeCode, $subscribedActionPurpose)"/>
        <xsl:variable name="eventReference" select="CodeMapper:GetRecipientCode($serviceProvider, $serviceProvider, concat($serviceProvider, ' System Configuration'), 'Event Type' , 'Event Reference', $responseTypeCode, $subscribedActionPurpose)"/>

        <xsl:variable name="reason">
          <xsl:text></xsl:text>
          <xsl:for-each select="s0:GroupApplicationErrorInformation[s0:ApplicationErrorInformation/s0:ApplicationErrorDetail/s0:ApplicationErrorCode/text() or s0:FreeText/s0:TextLiteral/s0:FreeText/text()]">
            <xsl:if test="position() > 1">
              <xsl:text>, </xsl:text>
            </xsl:if>
            <xsl:value-of select="concat(s0:ApplicationErrorInformation/s0:ApplicationErrorDetail/s0:ApplicationErrorCode/text(), ' - ', s0:FreeText/s0:TextLiteral/s0:FreeText/text())"/>
          </xsl:for-each>
        </xsl:variable>

        <xsl:if test="$eventType != '' or $subscribedJobNumber != ''">
          <UniversalEvent xmlns="http://www.cargowise.com/Schemas/Universal/2012/11">
            <Event>
              <DataContext>
                <DocumentaryOverride>
                  <DocumentName>
                    <xsl:value-of select="$subscribedDocumentName"/>
                  </DocumentName>
                </DocumentaryOverride>
                <DataTargetCollection>
                  <DataTarget>
                    <Key>
                      <xsl:value-of select="$subscribedJobNumber"/>
                    </Key>
                    <Type>
                      <xsl:value-of select="$subscribedForwardingType"/>
                    </Type>
                  </DataTarget>
                </DataTargetCollection>
              </DataContext>
              <EventTime>
                <xsl:value-of select="DateMapper:ConvertXmlDateString(string(s0:DateTimePeriod/s0:DateOrTimeOrPeriodText/text()), 'yyyy-MM-ddTHH:mm:ss')"/>
              </EventTime>
              <EventType>
                <xsl:value-of select="$eventType"/>
              </EventType>
              <EventParameters>
                <Department>Terminal</Department>
                <MessageType>
                  <xsl:value-of select="$subscribedDocumentName"/>
                </MessageType>
                <xsl:choose>
                  <xsl:when test="$associationAssignedCode='EBL'">
                    <ReferenceNumber>
                      <xsl:value-of select="s0:BeginningOfMessage/s0:DocumentMessageIdentification/s0:DocumentIdentifier/text()"/>
                    </ReferenceNumber>
                  </xsl:when>
                  <xsl:when test="$associationAssignedCode='PROT20'">
                    <ReferenceNumber>
                      <xsl:value-of select="s0:GroupReference/s0:Reference[s0:ReferenceCodeQualifier/text()='ALG']/s0:ReferenceIdentifier/text()"/>
                    </ReferenceNumber>
                    <RequestNumber>
                      <xsl:value-of select="s0:BeginningOfMessage/s0:DocumentMessageIdentification/s0:DocumentIdentifier/text()"/>
                    </RequestNumber>
                  </xsl:when>
                  <xsl:otherwise></xsl:otherwise>
                </xsl:choose>
                <xsl:if test="$reason != ''">
                  <Reason>
                    <xsl:value-of select="$reason"/>
                  </Reason>
                </xsl:if>
                <xsl:variable name="location" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', '', '@ST_ID', $msgID, '@value', $messageReference, '@referenceType', 'OperationPort')" />
                <xsl:if test="$location != ''">
                  <Location>
                    <xsl:value-of select="$location"/>
                  </Location>
                </xsl:if>
              </EventParameters>
              <xsl:if test="$eventReference!=''">
                <EventReference>
                  <xsl:value-of select="$eventReference"/>
                </EventReference>
              </xsl:if>
              <ContextCollection>
                <xsl:choose>
                  <xsl:when test="$associationAssignedCode='EBL'">
                    <xsl:call-template name ="CreateContext">
                      <xsl:with-param name="type" select="'Message Reference'"/>
                      <xsl:with-param name="value" select="$documentIdentifier"/>
                    </xsl:call-template>
                  </xsl:when>
                  <xsl:when test="$associationAssignedCode='PROT20'">
                    <xsl:call-template name ="CreateContext">
                      <xsl:with-param name="type" select="'eHubMessageReference'"/>
                      <xsl:with-param name="value" select="$documentIdentifier"/>
                    </xsl:call-template>
                    <xsl:call-template name ="CreateContext">
                      <xsl:with-param name="type" select="'SenderMessageReference'"/>
                      <xsl:with-param name="value" select="s0:BeginningOfMessage/s0:DocumentMessageIdentification/s0:DocumentIdentifier/text()"/>
                    </xsl:call-template>
                    <xsl:call-template name ="CreateContext">
                      <xsl:with-param name="type" select="'DGNSecurityNumber'"/>
                      <xsl:with-param name="value" select="s0:GroupReference/s0:Reference[s0:ReferenceCodeQualifier/text()='ALG']/s0:ReferenceIdentifier/text()"/>
                    </xsl:call-template>
                  </xsl:when>
                  <xsl:otherwise></xsl:otherwise>
                </xsl:choose>
              </ContextCollection>
            </Event>
          </UniversalEvent>
        </xsl:if>
      </Body>
    </UniversalInterchangeInclude>
  </xsl:template>

  <xsl:template name="CreateContext">
    <xsl:param name="type" />
    <xsl:param name="value" />

    <xsl:if test="$value!=''">
      <Context xmlns="http://www.cargowise.com/Schemas/Universal/2012/11">
        <Type>
          <xsl:value-of select="$type" />
        </Type>
        <Value>
          <xsl:value-of select="$value" />
        </Value>
      </Context>
    </xsl:if>
  </xsl:template>
</xsl:stylesheet>
<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                exclude-result-prefixes="msxsl ScriptNS0 ScriptNS1 ScriptNS2 userCSharp"
                version="1.0"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
                xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1"
                xmlns:ScriptNS2="http://schemas.microsoft.com/BizTalk/2003/ScriptNS2"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />
  <xsl:template match="/">
    <xsl:apply-templates select="/SuretyToBrokerMessage" />
  </xsl:template>

  <xsl:variable name="SenderID" select="ScriptNS1:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="TransactionID" select="SuretyToBrokerMessage/MessageLevelResult/TransactionID"/>
  <xsl:variable name="BrokerReferenceNumber" select="SuretyToBrokerMessage/MessageLevelResult/BrokerReferenceNumber"/>
  <xsl:variable name="RecipientID_Current">
    <xsl:value-of select="ScriptNS2:CallActionProcedureHelper('SelectSubscribedClients', '', '@senderId', $SenderID, '@value', $TransactionID, '@ST_ID', 'USCEB')"/>
  </xsl:variable>
  <xsl:variable name="RecipientID">
    <xsl:choose>
      <xsl:when test="$RecipientID_Current != ''">
        <xsl:value-of select="$RecipientID_Current"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="ScriptNS2:CallActionProcedureHelper('SelectSubscribedClients', '', '@senderId', $SenderID, '@value', $BrokerReferenceNumber, '@ST_ID', 'USCEB')"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="SetDestinationParty" select="ScriptNS1:SetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties', $RecipientID)"/>
  <xsl:template match="SuretyToBrokerMessage">
    <s0:UniversalInterchange>
      <s0:Body>
        <s0:UniversalEvent>
          <s0:Event>
            <xsl:variable name="TransactionIDTypeCode" select="MessageLevelResult/TransactionIDTypeCode"/>
            <s0:DataContext>
              <s0:DataTargetCollection>
                <s0:DataTarget>
                  <s0:Type>
                    <xsl:choose>
                      <xsl:when test="$TransactionIDTypeCode='1'">CustomsDeclaration</xsl:when>
                      <xsl:when test="$TransactionIDTypeCode='2'">USImporterSecurityFiling</xsl:when>
                    </xsl:choose>
                  </s0:Type>
                  <s0:Key>
                    <xsl:choose>
                      <xsl:when test="$RecipientID_Current != ''">
                        <xsl:value-of select="$BrokerReferenceNumber" />
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="ScriptNS2:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderID, '@recipientId', $RecipientID, '@ST_ID', 'USCEB', '@value', $BrokerReferenceNumber)" />
                      </xsl:otherwise>
                    </xsl:choose>
                  </s0:Key>
                </s0:DataTarget>
              </s0:DataTargetCollection>
            </s0:DataContext>
            <s0:EventTime>
              <xsl:value-of select="ScriptNS0:CurrentDateTime('s')" />
            </s0:EventTime>
            <s0:EventType>STC</s0:EventType>
            <s0:ContextCollection>
              <xsl:call-template name="BuildContext">
                <xsl:with-param name="type" select="'TransactionIDTypeCode'" />
                <xsl:with-param name="value" select="$TransactionIDTypeCode" />
              </xsl:call-template>
              <xsl:if test="$TransactionIDTypeCode='1'">
                <xsl:call-template name="BuildContext">
                  <xsl:with-param name="type" select="'EntryNumber'" />
                  <xsl:with-param name="value" select="MessageLevelResult/TransactionID" />
                </xsl:call-template>
              </xsl:if>
              <xsl:call-template name="BuildContext">
                <xsl:with-param name="type" select="'BrokerReferenceNumber'" />
                <xsl:with-param name="value" select="$BrokerReferenceNumber" />
              </xsl:call-template>
              <xsl:call-template name="BuildContext">
                <xsl:with-param name="type" select="'BondDesignationCode'" />
                <xsl:with-param name="value" select="MessageLevelResult/BondDesignationCode" />
              </xsl:call-template>
              <xsl:call-template name="BuildContext">
                <xsl:with-param name="type" select="'SuretyCode'" />
                <xsl:with-param name="value" select="MessageLevelResult/SuretyCode" />
              </xsl:call-template>
              <xsl:call-template name="BuildContext">
                <xsl:with-param name="type" select="'SuretyContactName'" />
                <xsl:with-param name="value" select="MessageLevelResult/SuretyContactName" />
              </xsl:call-template>
              <xsl:call-template name="BuildContext">
                <xsl:with-param name="type" select="'SuretyContactEmail'" />
                <xsl:with-param name="value" select="MessageLevelResult/SuretyContactEmail" />
              </xsl:call-template>
              <xsl:call-template name="BuildContext">
                <xsl:with-param name="type" select="'SuretyContactPhone'" />
                <xsl:with-param name="value" select="MessageLevelResult/SuretyContactPhone" />
              </xsl:call-template>
              <xsl:call-template name="BuildContext">
                <xsl:with-param name="type" select="'DispositionCode'" />
                <xsl:with-param name="value" select="MessageLevelResult/DispositionCode" />
              </xsl:call-template>
              <xsl:call-template name="BuildContext">
                <xsl:with-param name="type" select="'SuretyResponseCode'" />
                <xsl:with-param name="value" select="MessageLevelResult/SuretyResponseCode" />
              </xsl:call-template>
              <xsl:call-template name="BuildContext">
                <xsl:with-param name="type" select="'SuretyResponseDescription'" />
                <xsl:with-param name="value" select="MessageLevelResult/SuretyResponseDescription" />
              </xsl:call-template>
              <xsl:call-template name="BuildContext">
                <xsl:with-param name="type" select="'BondAmount'" />
                <xsl:with-param name="value" select="MessageLevelResult/BondAmount" />
              </xsl:call-template>
              <xsl:call-template name="BuildContext">
                <xsl:with-param name="type" select="'SuretyReferenceNumber'" />
                <xsl:with-param name="value" select="MessageLevelResult/SuretyReferenceNumber" />
              </xsl:call-template>
              <xsl:call-template name="BuildContext">
                <xsl:with-param name="type" select="'ImporterNumber'" />
                <xsl:with-param name="value" select="MessageLevelResult/ImporterNumber" />
              </xsl:call-template>
              <xsl:call-template name="BuildContext">
                <xsl:with-param name="type" select="'ImporterName'" />
                <xsl:with-param name="value" select="MessageLevelResult/ImporterName" />
              </xsl:call-template>
              <xsl:for-each select="MessageLevelResult/MessageLevelReasonCodes">
                <s0:Context>
                  <s0:Type>MessageLevelReasonCodes</s0:Type>
                  <s0:Value />
                  <s0:SubContextCollection>
                    <s0:SubContext>
                      <s0:Type>ReasonCode</s0:Type>
                      <s0:Value>
                        <xsl:value-of select="ReasonCode" />
                      </s0:Value>
                    </s0:SubContext>
                    <s0:SubContext>
                      <s0:Type>ReasonDescription</s0:Type>
                      <s0:Value>
                        <xsl:value-of select="ReasonDescription" />
                      </s0:Value>
                    </s0:SubContext>
                  </s0:SubContextCollection>
                </s0:Context>
              </xsl:for-each>
              <xsl:for-each select="./LineLevelResults">
                <s0:Context>
                  <s0:Type>LineLevelResults</s0:Type>
                  <xsl:variable name="LineNo" select="LineNo"/>
                  <xsl:variable name="HTSNumber" select="HTSNumber"/>
                  <s0:Value>
                    <xsl:value-of select="concat($LineNo, '/', $HTSNumber)" />
                  </s0:Value>
                  <s0:SubContextCollection>
                    <s0:SubContext>
                      <s0:Type>ReasonCode</s0:Type>
                      <s0:Value>
                        <xsl:value-of select="LineLevelReasonCodes/ReasonCode" />
                      </s0:Value>
                    </s0:SubContext>
                  </s0:SubContextCollection>
                </s0:Context>
              </xsl:for-each>
            </s0:ContextCollection>
          </s0:Event>
        </s0:UniversalEvent>
      </s0:Body>
    </s0:UniversalInterchange>
  </xsl:template>

  <xsl:template name="BuildContext">
    <xsl:param name="type"/>
    <xsl:param name="value"/>
    <xsl:if test="$value != ''">
      <s0:Context>
        <s0:Type><xsl:value-of select="$type" /></s0:Type>
        <s0:Value><xsl:value-of select="$value" /></s0:Value>
      </s0:Context>
    </xsl:if>
  </xsl:template>

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[
  
  
    public string GetSenderIDAsString(string from)
    {
      var doc = new XmlDocument();
      doc.LoadXml(from);

      return (doc.GetElementsByTagName("Address","http://schemas.xmlsoap.org/ws/2005/08/addressing")[0]).InnerText;
    }
      ]]>
  </msxsl:script>
</xsl:stylesheet>
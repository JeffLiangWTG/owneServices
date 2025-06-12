<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 ScriptNS0 ScriptNS1" version="1.0"
                xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:rex="http://agriculture.gov.au/nexdoc/RexOwnershipSoap_1.0"
                xmlns:com="http://agriculture.gov.au/nexdoc/common/CommonTypes_1.0"
                xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
                xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />

  <xsl:variable name="SenderID" select="ScriptNS0:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="RecipientID" select="ScriptNS0:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="RexNumber" select="ScriptNS0:GetContextProperty('RexNumber', '')"/>
  <xsl:variable name="JobNumber" select="ScriptNS0:GetContextProperty('JobNumber', '')"/>

  <xsl:template match="/">
		<xsl:call-template name="CreateInterchange">
			<xsl:with-param name="RexOwnershipResponse" select="soapenv:Envelope/soapenv:Body/*[1]"/>
		</xsl:call-template>
  </xsl:template>
  <xsl:template name="CreateInterchange">
		<xsl:param name="RexOwnershipResponse" />
	  <s0:UniversalInterchange>
			<s0:Header>
				<s0:SenderID/>
				<s0:RecipientID/>
				<xsl:if test="local-name($RexOwnershipResponse)!='Fault'">
					<s0:DeliveryMetadata>
						<s0:ValueCollection>
							<s0:Value>
								<s0:Name>RexNumber</s0:Name>
								<s0:Type>String</s0:Type>
								<s0:Data>
									<xsl:value-of select="$RexNumber"/>
								</s0:Data>
							</s0:Value>
							<s0:Value>
								<s0:Name>JobNumber</s0:Name>
								<s0:Type>String</s0:Type>
								<s0:Data>
									<xsl:value-of select="$JobNumber"/>
								</s0:Data>
							</s0:Value>
						</s0:ValueCollection>
					</s0:DeliveryMetadata>
				</xsl:if>
			</s0:Header>
			<s0:Body>
				<xsl:choose>
				<xsl:when test="local-name($RexOwnershipResponse)='Fault' and namespace-uri($RexOwnershipResponse)='http://schemas.xmlsoap.org/soap/envelope/'">
            <xsl:variable name="FaultMessage" select="$RexOwnershipResponse/detail/com:NexdocSoapFault/com:NexdocSoapFaultItem/com:FaultMessage/text()"/>
            <xsl:variable name="FaultString" select="$RexOwnershipResponse/faultstring/text()"/>
            <s0:UniversalEvent>
              <s0:Event>
                <s0:DataContext>
                  <s0:DataProvider>NEXDOCS</s0:DataProvider>
                  <s0:DataTargetCollection>
                      <s0:DataTarget>
                        <Key>
                          <xsl:value-of select="$RexNumber"/>
                        </Key>
                        <Type>REXNotification</Type>
                      </s0:DataTarget>
                    <xsl:if test="$JobNumber!=''">
                      <s0:DataTarget>
                        <Key>
                          <xsl:value-of select="$JobNumber"/>
                        </Key>
                        <Type>CustomsDeclaration</Type>
                      </s0:DataTarget>
                    </xsl:if>
                  </s0:DataTargetCollection>
                </s0:DataContext>
                <s0:EventTime>
                  <xsl:value-of select="ScriptNS1:CurrentDateTimeUTC('s')"/>
                </s0:EventTime>
                <s0:EventType>MRR</s0:EventType>
                <s0:ContextCollection>
                  <s0:Context>
                    <s0:Type>MessageStatus</s0:Type>
                    <s0:Value>ERO</s0:Value>
                  </s0:Context>
                  <s0:Context>
                    <s0:Type>Message</s0:Type>
                    <s0:Value>Calling RexOwnership service failed</s0:Value>
                  </s0:Context>
                  <xsl:if test="$FaultMessage!=''">
                    <s0:Context>
                      <s0:Type>Message</s0:Type>
                      <s0:Value>
                        <xsl:value-of select="$FaultMessage"/>
                      </s0:Value>
                    </s0:Context>
                  </xsl:if>
                <xsl:if test="$FaultString!=''">
                    <s0:Context>
                      <s0:Type>Message</s0:Type>
                      <s0:Value>
                        <xsl:value-of select="$FaultString"/>
                      </s0:Value>
                    </s0:Context>
                  </xsl:if>
                </s0:ContextCollection>
              </s0:Event>
            </s0:UniversalEvent>
          </xsl:when>
					<xsl:otherwise>
						<xsl:element name="rex:{local-name($RexOwnershipResponse)}">
							<rex:outcome>
								<xsl:value-of select="$RexOwnershipResponse/rex:outcome/text()"/>
							</rex:outcome>
						</xsl:element>
					</xsl:otherwise>
				</xsl:choose>
		  </s0:Body>
	  </s0:UniversalInterchange>
	</xsl:template>
</xsl:stylesheet>
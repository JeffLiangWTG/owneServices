<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 ScriptNS0 ScriptNS1 ScriptNS2 userCSharp" version="1.0"
                xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:rex="http://agriculture.gov.au/nexdoc/RexOwnershipSoap_1.0"
                xmlns:com="http://agriculture.gov.au/nexdoc/common/CommonTypes_1.0"
                xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
                xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1"
                xmlns:ScriptNS2="http://schemas.microsoft.com/BizTalk/2003/ScriptNS2"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />

  <xsl:variable name="SenderID" select="ScriptNS1:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="RecipientID" select="ScriptNS1:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="SystemID" select="concat(substring($SenderID, 1, 3), substring($SenderID, 7, 3))"/>

  <xsl:template match="/">
    <xsl:variable name="Submitter" select="s0:UniversalInterchange/s0:Header/s0:DeliveryMetadata/s0:ValueCollection/s0:Value[s0:Name/text()='Submitter']/s0:Data/text()" />
    <xsl:variable name="RexNumber" select="s0:UniversalInterchange/s0:Header/s0:DeliveryMetadata/s0:ValueCollection/s0:Value[s0:Name/text()='RexNumber']/s0:Data/text()" />
    <xsl:variable name="MessageType" select="s0:UniversalInterchange/s0:Header/s0:DeliveryMetadata/s0:ValueCollection/s0:Value[s0:Name/text()='MessageType']/s0:Data/text()" />
    <soapenv:Envelope>
      <soapenv:Header>
        <nexauth:authTokens xmlns:nexauth="http://agriculture.gov.au/header/auth">
          <vendorToken>
            <xsl:value-of select="ScriptNS0:GetVendorToken($RecipientID)"/>
          </vendorToken>
          <clientGroupToken>
            <xsl:value-of select="ScriptNS2:GetClientGroupToken($SenderID)"/>
          </clientGroupToken>
          <clientToken>
            <xsl:value-of select="ScriptNS2:GetClientToken($SystemID, $Submitter)"/>
          </clientToken>
        </nexauth:authTokens>
        <wsse:Security xmlns:wsse="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd">
          <wsse:UsernameToken>
            <wsse:Username>
              <xsl:value-of select="ScriptNS0:GetInstallationToken($RecipientID)"/>
            </wsse:Username>
            <wsse:Password Type="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText">
              <xsl:value-of select="ScriptNS0:GetInstallationPassword($RecipientID)"/>
            </wsse:Password>
          </wsse:UsernameToken>
        </wsse:Security>
      </soapenv:Header>
      <soapenv:Body>
        <xsl:call-template name="SOAPBody">
          <xsl:with-param name="RexNumber" select="$RexNumber" />
          <xsl:with-param name="InterchangeBody" select="s0:UniversalInterchange/s0:Body" />
          <xsl:with-param name="MessageType" select="$MessageType" />
        </xsl:call-template>
      </soapenv:Body>
    </soapenv:Envelope>
  </xsl:template>
  <xsl:template name="SOAPBody">
    <xsl:param name="InterchangeBody" />
    <xsl:param name="RexNumber" />
    <xsl:param name="MessageType" />

    <xsl:element name="rex:{$MessageType}">
      <rex:identification>
        <com:rexNumber>
          <xsl:value-of select="$RexNumber"/>
        </com:rexNumber>
      </rex:identification>
      <xsl:choose >
        <xsl:when test="$MessageType='RexAcknowledgeOwnership'">
          <rex:isAccepted>
            <xsl:value-of select="$InterchangeBody/rex:RexAcknowledgeOwnership/rex:isAccepted/text()"/>
          </rex:isAccepted>
        </xsl:when>
        <xsl:when test="$MessageType='RexForwardOwnership'">
          <rex:clientGroup>
            <xsl:value-of select="$InterchangeBody/rex:RexForwardOwnership/rex:clientGroup/text()"/>
          </rex:clientGroup>
          <rex:requiresAcceptance>
            <xsl:value-of select="$InterchangeBody/rex:RexForwardOwnership/rex:requiresAcceptance/text()"/>
          </rex:requiresAcceptance>
          <xsl:if test="$InterchangeBody/rex:RexForwardOwnership/rex:holdUntilStatus">
            <rex:holdUntilStatus>
              <xsl:value-of select="$InterchangeBody/rex:RexForwardOwnership/rex:holdUntilStatus/text()"/>
            </rex:holdUntilStatus>
          </xsl:if>
        </xsl:when>
        <xsl:when test="$MessageType='RexTransferOwnership'">
          <rex:clientGroup>
            <xsl:value-of select="$InterchangeBody/rex:RexTransferOwnership/rex:clientGroup/text()"/>
          </rex:clientGroup>
          <rex:exporter>
            <xsl:value-of select="$InterchangeBody/rex:RexTransferOwnership/rex:exporter/text()"/>
          </rex:exporter>
        </xsl:when>
      </xsl:choose>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>
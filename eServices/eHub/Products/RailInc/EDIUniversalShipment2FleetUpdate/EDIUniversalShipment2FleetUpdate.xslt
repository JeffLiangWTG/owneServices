<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var"
                exclude-result-prefixes="msxsl var s0 ScriptNS0 ScriptNS1 ScriptNS2 userCSharp" version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/Native"
                xmlns:ns0="http://cargowise.com/ehub/products/railinc/2011/06"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp"
                xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
                xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1"
                xmlns:ScriptNS2="http://schemas.microsoft.com/BizTalk/2003/ScriptNS2">

  <xsl:output omit-xml-declaration="yes" method="xml" indent="yes" version="1.0" />

  <xsl:template match="*[local-name()='UniversalShipment']/*[local-name()='Shipment']">
    <ns0:FleetUpdate>
      <xsl:variable name="fileName" select="ScriptNS1:GetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06')"/>
      <xsl:if test="not($fileName)">
        <xsl:value-of select="userCSharp:ThrowException('Message from CW1 client did not provide a file name')" />
      </xsl:if>

      <xsl:variable name="ref" select="*[local-name()='DataContext']
                                    /*[local-name()='DataSourceCollection']
                                    /*[local-name()='DataSource'
                                      and (*[local-name()='Type']/text()='ForwardingConsol'
                                        or *[local-name()='Type']/text()='CustomsDeclaration')]
                                    /*[local-name()='Key']/text()"/>
      <xsl:if test="not($ref)">
        <xsl:value-of select="userCSharp:ThrowException('Message from CW1 client did not provide a DataSource of Type ForwardingConsol or CustomsDeclaration')" />
      </xsl:if>

      <xsl:variable name="evt" select="*[local-name()='DataContext']
                                      /*[local-name()='ActionPurpose']
                                      /*[local-name()='Code']/text()"/>

      <xsl:variable name="recipientId" select="ScriptNS1:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
      <xsl:variable name="senderId" select="ScriptNS1:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
      <xsl:variable name="fleetId" select="*[local-name()='DataContext']/*[local-name()='ActionPurpose']/*[local-name()='Description']"/>
      <CLUHeader>
        <InquiryId>
          <xsl:text>CLU</xsl:text>
        </InquiryId>
        <ContainersInd>
          <xsl:text>C</xsl:text>
        </ContainersInd>
        <To>
          <xsl:text>TO</xsl:text>
        </To>
        <SCAC>
          <xsl:text>RRDC</xsl:text>
        </SCAC>
        <FleetId>
			<xsl:value-of select="$fleetId"/>
        </FleetId>
        <FleetUpdateCode>
          <xsl:text> </xsl:text>
        </FleetUpdateCode>
      </CLUHeader>

      <xsl:choose>
        <xsl:when test=" *[local-name()='DataContext']
                        /*[local-name()='DataSourceCollection']
                        /*[local-name()='DataSource']
                        /*[local-name()='Type']/text()='ForwardingShipment'">
	        <xsl:if test="count( *[local-name()='SubShipmentCollection']
                                /*[local-name()='SubShipment']
                                /*[local-name()='ContainerCollection']
                                /*[local-name()='Container']
                                /*[local-name()='ContainerNumber']) = 0">
		        <xsl:value-of select="userCSharp:ThrowException('Subshipment must have at least one container.')" />
	        </xsl:if>
	        <xsl:for-each select=" *[local-name()='SubShipmentCollection']
                                /*[local-name()='SubShipment']
                                /*[local-name()='ContainerCollection']
                                /*[local-name()='Container']
                                /*[local-name()='ContainerNumber']">
            <xsl:call-template name="CLUDetail">
              <xsl:with-param name="ref" select="$ref"/>
              <xsl:with-param name="evt" select="$evt"/>
              <xsl:with-param name="contNum" select="text()"/>
              <xsl:with-param name="fleetId" select="$fleetId"/>
              <xsl:with-param name="senderId" select="$senderId"/>
              <xsl:with-param name="recipientId" select="$recipientId"/>
            </xsl:call-template>
          </xsl:for-each>
        </xsl:when>
        <xsl:otherwise>
	        <xsl:if test="count( *[local-name()='ContainerCollection']
                                /*[local-name()='Container']
                                /*[local-name()='ContainerNumber']) = 0">
		        <xsl:value-of select="userCSharp:ThrowException('Shipment must have at least one container.')" />
	        </xsl:if>
	        <xsl:for-each select=" *[local-name()='ContainerCollection']
                                /*[local-name()='Container']
                                /*[local-name()='ContainerNumber']">
            <xsl:call-template name="CLUDetail">
              <xsl:with-param name="ref" select="$ref"/>
              <xsl:with-param name="evt" select="$evt"/>
              <xsl:with-param name="contNum" select="text()"/>
              <xsl:with-param name="fleetId" select="$fleetId"/>
              <xsl:with-param name="senderId" select="$senderId"/>
              <xsl:with-param name="recipientId" select="$recipientId"/>
            </xsl:call-template>
          </xsl:for-each>
        </xsl:otherwise>
      </xsl:choose>

      <CLUTrailer>
        <Trailer>
          <xsl:text>EOM</xsl:text>
        </Trailer>
      </CLUTrailer>
    </ns0:FleetUpdate>
  </xsl:template>

  <xsl:template name="CLUDetail">
    <xsl:param name="ref"/>
    <xsl:param name="evt"/>
    <xsl:param name="contNum"/>
    <xsl:param name="fleetId"/>
    <xsl:param name="senderId"/>
    <xsl:param name="recipientId"/>
    <CLUDetail>
      <FleetStatusCode>
        <xsl:value-of select="ScriptNS0:GetRecipientCode('RAILINCFC', 'RAILINCFC', 'RailSight - Generate CLU messages', 'Fleet Status Code', 'Fleet Status', $evt)" />
      </FleetStatusCode>
      <EquipmentNumber>
        <xsl:value-of select="substring($contNum, 1, 10)" />
      </EquipmentNumber>
      <Separater>
        <xsl:text>*</xsl:text>
      </Separater>
      <xsl:variable name="customInformation">
        <xsl:variable name="checkDigit" select="substring($contNum, 11)" />
        <xsl:value-of select="concat($ref, '_', $checkDigit)" />
      </xsl:variable>
      <CustomInformation>
		  <xsl:value-of select="$customInformation"/>
      </CustomInformation>

      <xsl:variable name="subscription" select="concat($fleetId, '|', $customInformation)" />
      <xsl:variable name="SubscribeMessageContents" select="ScriptNS2:InsertSubscriptionValue('RLC', $recipientId, $senderId, $subscription)" />
    </CLUDetail>
  </xsl:template>

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[
      public void ThrowException(string reason) {
        throw new Exception(reason);
      }
    ]]>
  </msxsl:script>
</xsl:stylesheet>

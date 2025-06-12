<?xml version="1.0" encoding="UTF-8"?>

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								xmlns:msxsl="urn:schemas-microsoft-com:xslt"
								xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var"
								exclude-result-prefixes="msxsl var s0 ScriptNS0 ScriptNS1" version="1.0"
								xmlns:s0="http://www.cargowise.com/Schemas/Universal/2011/11"
								xmlns:ns0="http://cargowise.com/ehub/core/genericmessagedelivery"
								xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
								xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1" >

	<xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />

	<xsl:variable name="OriginalSenderId" select="ScriptNS1:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')" />
	<xsl:variable name="OriginalRecipientId" select="ScriptNS1:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')" />

	<xsl:variable name="SetSenderId" select="ScriptNS1:SetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties', $OriginalRecipientId)" />
	<xsl:variable name="SetRecipientId" select="ScriptNS1:SetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties', $OriginalSenderId)" />

	<xsl:template match="/">
		<xsl:element name="ns0:GenericMessageInterchange">
			<xsl:element name="ns0:Header">
				
				<xsl:element name="ns0:SenderID">
					<xsl:value-of select="$OriginalRecipientId" />
				</xsl:element>
				
				<xsl:element name="ns0:RecipientID">
					<xsl:value-of select="$OriginalSenderId" />
				</xsl:element>
				
				<xsl:element name="ns0:InterchangeType">
					<xsl:value-of select="'ZZA'" />
				</xsl:element>
				
				<xsl:element name="ns0:InterchangeNumber">
					<xsl:value-of select="ScriptNS0:CallActionProcedureHelper('GetCounterInterfaceValue',
																						'@StartValue',
																						'@TransformatonSetName', 'Asycuda Ushipment2AsycudaSAD',
																						'@Name', 'AsycudaGmiMsgNo',
																						'@MaxValue', 999999999,
																						'@IncrementValue', 1)" />
				</xsl:element>
			</xsl:element>
			
			<xsl:element name="ns0:Body">
				<xsl:apply-templates select="*" />
			</xsl:element>
		</xsl:element>
	</xsl:template>

	<xsl:template match="*">
		<xsl:element name="{local-name(.)}">
			<xsl:copy-of select="@*" />
			<xsl:apply-templates />
		</xsl:element>
	</xsl:template>

</xsl:stylesheet>
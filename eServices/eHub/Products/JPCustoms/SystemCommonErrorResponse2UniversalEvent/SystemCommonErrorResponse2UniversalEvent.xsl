<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt"
    xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var" exclude-result-prefixes="msxsl var s0" version="1.0"
    xmlns:s0="http://cargowise.com/ehub/products/jpcustoms/2013/09"
    xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2011/11"
    xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
    xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1"
    xmlns:ScriptNS2="http://schemas.microsoft.com/BizTalk/2003/ScriptNS2"
    xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
	<xsl:output omit-xml-declaration="yes" indent="no" version="1.0" method="xml" />

	<xsl:template match="/">
		<xsl:apply-templates select="/s0:SystemCommonOutput" />
	</xsl:template>

	<xsl:template match="s0:SystemCommonOutput">
		<ns0:UniversalInterchangeInclude>
			<ns0:Header>
				<ns0:SenderID/>
				<ns0:RecipientID/>
			</ns0:Header>
			<ns0:Body>
				<ns0:UniversalEvent>
					<ns0:Event>
						<ns0:DataContext>
							<ns0:ActionPurpose>
								<ns0:Code>
									<xsl:value-of select ="normalize-space(s0:OutputCommonField/s0:ProcedureCode)"/>
								</ns0:Code>
								<ns0:Description>
									<xsl:value-of select ="normalize-space(s0:OutputCommonField/s0:ProcedureCode)"/>
								</ns0:Description>
							</ns0:ActionPurpose>
							<ns0:DataProvider>AFR</ns0:DataProvider>
							<ns0:DataTargetCollection>
								<ns0:DataTarget>
									<ns0:Key>AFRHeader</ns0:Key>
									<ns0:Type>AFRHeader</ns0:Type>
								</ns0:DataTarget>
							</ns0:DataTargetCollection>
						</ns0:DataContext>
						<ns0:EventTime>
							<xsl:variable name="DateOfMessageReceipt" select="ScriptNS1:ConvertToDateTimeString(s0:OutputCommonField/s0:DateOfMessageReceipt, 'yyyyMMddHHmm')" />
							<xsl:if test="$DateOfMessageReceipt">
								<xsl:value-of select="ScriptNS1:ConvertLocalXmlDateTimeStringToUTC($DateOfMessageReceipt,'09:00', 'O', '')"/>
							</xsl:if>
						</ns0:EventTime>
						<ns0:EventType>MSC</ns0:EventType>
						<xsl:variable name="NotificationSubjectCode" select="s0:ProcessResultCodes/s0:ProcessResultCode[1]/s0:Code"/>
						<xsl:variable name="ProcedureCode" select="normalize-space(s0:OutputCommonField/s0:ProcedureCode)"/>
						<xsl:variable name="HouseBillOfLadingNumber" select="normalize-space(s0:HouseBillOfLadingNumber)"/>
						<ns0:EventReference>
							<xsl:value-of select ="$ProcedureCode"/>
							<xsl:value-of select ="'-'"/>
							<xsl:value-of select="'REJECTED'"/>
						</ns0:EventReference>
						<ns0:ContextCollection>
							<xsl:call-template name="ContextKeyValuePair">
								<xsl:with-param name="type" select="'InternalTransactionNumber'" />
								<xsl:with-param name="value" select="s0:OutputCommonField/s0:InputMessageID" />
							</xsl:call-template>
							<xsl:call-template name="ContextKeyValuePair">
								<xsl:with-param name="type" select="'AdditionalInformation1'" />
								<xsl:with-param name="value" select="s0:OutputCommonField/s0:AdditionalInfo1" />
							</xsl:call-template>
							<xsl:call-template name="ContextKeyValuePair">
								<xsl:with-param name="type" select="'AdditionalInformation2'" />
								<xsl:with-param name="value" select="s0:OutputCommonField/s0:AdditionalInfo2" />
							</xsl:call-template>
							<xsl:element name="ns0:Context">
								<xsl:element name="ns0:Type">
									<xsl:value-of select="'NotificationDetails'"/>
								</xsl:element>
								<xsl:element name="ns0:Value">
									<xsl:value-of select="'&lt;table width=&quot;100%&quot; border=&quot;1&quot; cellpadding=&quot;1&quot; cellspacing=&quot;0&quot; class=&quot;table&quot;&gt;'" disable-output-escaping="no"/>
									<xsl:for-each select="s0:ProcessResultCodes/s0:ProcessResultCode">
										<xsl:if test="s0:Code and s0:Code != '00000' and s0:Code != ''">
											<xsl:variable name="position" select="position()"/>
											<xsl:call-template name="HTMLTR">
												<xsl:with-param name="name" select="concat('Process Result Code - ', $position)"/>
												<xsl:with-param name="value" select="s0:Code"/>
											</xsl:call-template>
											<xsl:call-template name="HTMLTR">
												<xsl:with-param name="name" select="concat('Process Result Description - ', $position)"/>
												<xsl:with-param name="value">
													<xsl:value-of select="ScriptNS0:GetRecipientCode('JPCustoms' , 'JPCustoms' , 'JP AFR - Import System Common Error' , 'ErrorDetail' , 'ErrorDescription', s0:Code)" disable-output-escaping="no"/>
												</xsl:with-param>
											</xsl:call-template>
											<xsl:call-template name="HTMLTR">
												<xsl:with-param name="name" select="concat('Process Result Suggestion - ', $position)"/>
												<xsl:with-param name="value">
													<xsl:value-of select="ScriptNS0:GetRecipientCode('JPCustoms' , 'JPCustoms' , 'JP AFR - Import System Common Error' , 'ErrorDetail' , 'ErrorSolution', s0:Code)" disable-output-escaping="no"/>
												</xsl:with-param>
											</xsl:call-template>
										</xsl:if>
									</xsl:for-each>
									<xsl:value-of select="'&lt;/table&gt;'" disable-output-escaping="no"/>

								</xsl:element>
							</xsl:element>
						</ns0:ContextCollection>
					</ns0:Event>
				</ns0:UniversalEvent>
			</ns0:Body>
		</ns0:UniversalInterchangeInclude>
	</xsl:template>

	<xsl:template name="HTMLTR" match="*">
		<xsl:param name="name" />
		<xsl:param name="value" />
		<xsl:param name="forceOutput" select ="0"/>
		<xsl:if test="$name and ($value != '' or $forceOutput = '1')">
			<xsl:value-of select="'&lt;tr&gt;'" disable-output-escaping="no"/>
			<xsl:value-of select="'&lt;td&gt;'" disable-output-escaping="no"/>
			<xsl:value-of select="$name"/>
			<xsl:value-of select="'&lt;/td&gt;'" disable-output-escaping="no"/>
			<xsl:value-of select="'&lt;td&gt;'" disable-output-escaping="no"/>
			<xsl:value-of select="$value"/>
			<xsl:value-of select="'&lt;/td&gt;'" disable-output-escaping="no"/>
			<xsl:value-of select="'&lt;/tr&gt;'" disable-output-escaping="no"/>
		</xsl:if>
	</xsl:template>

	<xsl:template name="ContextKeyValuePair" match="*">
		<xsl:param name="type" />
		<xsl:param name="value" />
		<xsl:if test="$type and $value">
			<ns0:Context>
				<ns0:Type>
					<xsl:value-of select="normalize-space($type)" />
				</ns0:Type>
				<ns0:Value>
					<xsl:value-of select="normalize-space($value)" />
				</ns0:Value>
			</ns0:Context>
		</xsl:if>
	</xsl:template>

	<msxsl:script language="C#" implements-prefix="userCSharp">
		<![CDATA[
		]]>
	</msxsl:script>

</xsl:stylesheet>
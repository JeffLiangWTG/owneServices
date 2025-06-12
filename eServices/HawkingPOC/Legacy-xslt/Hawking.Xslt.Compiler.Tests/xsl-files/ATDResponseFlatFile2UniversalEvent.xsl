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
		<xsl:apply-templates select="/s0:ATDOutput" />
	</xsl:template>

	<xsl:template match="s0:ATDOutput">
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
								<ns0:Code>DTR</ns0:Code>
								<ns0:Description>DTR</ns0:Description>
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
						<ns0:EventReference>
							<xsl:value-of select ="normalize-space(s0:OutputCommonField/s0:ProcedureCode)"/>
							<xsl:value-of select ="'-'"/>
							<xsl:choose>
								<xsl:when test="s0:ProcessResultCodes/s0:ProcessResultCode[1]/s0:Code='00000'">
									<xsl:value-of select="'ACCEPTED'"/>
								</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="'REJECTED'"/>
								</xsl:otherwise>
							</xsl:choose>
						</ns0:EventReference>
						<ns0:ContextCollection>
							<xsl:call-template name="ContextKeyValuePair">
								<xsl:with-param name="type" select="'InternalTransactionNumber'" />
								<xsl:with-param name="value" select="s0:OutputCommonField/s0:InputMessageID" />
							</xsl:call-template>
							<xsl:call-template name="ContextKeyValuePair">
								<xsl:with-param name="type" select="'VesselCallSign'" />
								<xsl:with-param name="value" select="s0:VesselCode" />
							</xsl:call-template>
							<xsl:call-template name="ContextKeyValuePair">
								<xsl:with-param name="type" select="'VoyageNumber'" />
								<xsl:with-param name="value" select="s0:VoyageNumber" />
							</xsl:call-template>
							<xsl:call-template name="ContextKeyValuePair">
								<xsl:with-param name="type" select="'CarrierCode'" />
								<xsl:with-param name="value" select="s0:CarrierCode" />
							</xsl:call-template>
							<xsl:call-template name="ContextKeyValuePair">
								<xsl:with-param name="type" select="'PortOfLoadingUNLOCO'" />
								<xsl:with-param name="value" select="s0:PortOfLoadingCode" />
							</xsl:call-template>
							<xsl:call-template name="ContextKeyValuePair">
								<xsl:with-param name="type" select="'PortOfLoadingSuffix'" />
								<xsl:with-param name="value" select="s0:PortOfLoadingSuffix" />
							</xsl:call-template>

							<xsl:element name="ns0:Context">
								<xsl:element name="ns0:Type">
									<xsl:value-of select="'NotificationDetails'"/>
								</xsl:element>
								<xsl:element name="ns0:Value">
									<xsl:value-of select="'&lt;table width=&quot;100%&quot; border=&quot;1&quot; cellpadding=&quot;1&quot; cellspacing=&quot;0&quot; class=&quot;table&quot;&gt;'" disable-output-escaping="no"/>
									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Vessel Call Sign'" />
										<xsl:with-param name="value" select="s0:VesselCode" />
									</xsl:call-template>
									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Voyage Number'" />
										<xsl:with-param name="value" select="s0:VoyageNumber" />
									</xsl:call-template>
									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Carrier Code'" />
										<xsl:with-param name="value" select="s0:CarrierCode" />
									</xsl:call-template>
									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Port Of Loading'" />
										<xsl:with-param name="value" select="s0:PortOfLoadingCode" />
									</xsl:call-template>
									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Port Of Loading Suffix'" />
										<xsl:with-param name="value" select="s0:PortOfLoadingSuffix" />
									</xsl:call-template>

									<xsl:for-each select="s0:ProcessResultCodes/s0:ProcessResultCode">
										<xsl:if test="s0:Code and s0:Code != '00000' and s0:Code != ''">
											<xsl:variable name="ProcedureCode" select="../../s0:OutputCommonField/s0:ProcedureCode" />
											<xsl:variable name="position" select="position()"/>
											<xsl:call-template name="HTMLTR">
												<xsl:with-param name="name" select="concat('Process Result Code - ', $position)"/>
												<xsl:with-param name="value" select="s0:Code"/>
											</xsl:call-template>
											<xsl:call-template name="HTMLTR">
												<xsl:with-param name="name" select="concat('Process Result Field - ', $position)"/>
												<xsl:with-param name="value">
													<xsl:value-of select="'Error occured on Field:&quot;'" />
													<xsl:value-of select="s0:ErrorFieldID" />
													<xsl:value-of select="' - '" />
													<xsl:value-of select="ScriptNS0:GetRecipientCode('JPCustoms' , 'JPCustoms' , 'JP AFR - Import SAHR' , 'Field' , 'FieldName', s0:ErrorFieldID, $ProcedureCode)"/>
													<xsl:value-of select="'&quot;'" />
													<xsl:if test="s0:FieldSequence != ''">
														<xsl:value-of select="' on '" />
														<xsl:value-of select="s0:FieldSequence" />
														<xsl:choose>
															<xsl:when test="substring(format-number(s0:FieldSequence,'00000'), 5,1)='1'">
																<xsl:value-of select="'st'" />
															</xsl:when>
															<xsl:when test="substring(format-number(s0:FieldSequence,'00000'), 5,1)='2'">
																<xsl:value-of select="'nd'" />
															</xsl:when>
															<xsl:when test="substring(format-number(s0:FieldSequence,'00000'), 5,1)='3'">
																<xsl:value-of select="'rd'" />
															</xsl:when>
															<xsl:otherwise>
																<xsl:value-of select="'th'" />
															</xsl:otherwise>
														</xsl:choose>
														<xsl:value-of select="' occurance'" />
													</xsl:if>
												</xsl:with-param>
											</xsl:call-template>
											<xsl:call-template name="HTMLTR">
												<xsl:with-param name="name" select="concat('Process Result Description - ', $position)"/>
												<xsl:with-param name="value">
													<xsl:value-of select="ScriptNS0:GetRecipientCode('JPCustoms' , 'JPCustoms' , 'JP AFR - Import SATD' , 'ErrorDetail' , 'ErrorDescription', s0:Code)" disable-output-escaping="no"/>
													<!--Special Code Description Case Since the original text is too long-->
													<xsl:if test="s0:Code='R0001'">
														<xsl:value-of select="'&lt;BR/&gt;Date of Procedure Implementation (JST) + 7 days (except Sunday and Japanese National holidays) &lt; Date of Departure (JST) + 30 days&lt;BR/&gt;2. When Relaxed Application Area Identifier is NOT entered ;&lt;BR/&gt;Date of Procedure Implementation (JST) + 7 days (except Sunday and Japanese National holidays) &lt; Date of Departure (JST) + 90 days'" disable-output-escaping="no"/>
													</xsl:if>
												</xsl:with-param>
											</xsl:call-template>
											<xsl:call-template name="HTMLTR">
												<xsl:with-param name="name" select="concat('Process Result Suggestion - ', $position)"/>
												<xsl:with-param name="value">
													<xsl:value-of select="concat(
																				ScriptNS0:GetRecipientCode('JPCustoms' , 'JPCustoms' , 'JP AFR - Import SATD' , 'ErrorDetail' , 'ErrorSolutionPartA', s0:Code),
																				ScriptNS0:GetRecipientCode('JPCustoms' , 'JPCustoms' , 'JP AFR - Import SATD' , 'ErrorDetail' , 'ErrorSolutionPartB', s0:Code)
																				)" disable-output-escaping="no"/>
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

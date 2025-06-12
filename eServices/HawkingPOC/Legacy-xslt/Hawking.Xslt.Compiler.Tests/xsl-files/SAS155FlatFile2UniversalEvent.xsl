<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt"
    xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var" exclude-result-prefixes="msxsl var s0" version="1.0"
    xmlns:s0="http://cargowise.com/ehub/products/jpcustoms/2017/01"
    xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2011/11"
    xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
    xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1"
    xmlns:ScriptNS2="http://schemas.microsoft.com/BizTalk/2003/ScriptNS2"
    xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
	<xsl:output omit-xml-declaration="yes" indent="no" version="1.0" method="xml" />

	<xsl:template match="/">
		<xsl:apply-templates select="/s0:SAS155Output" />
	</xsl:template>

	<xsl:template match="s0:SAS155Output">
		<ns0:UniversalInterchangeInclude>
			<ns0:Header>
				<ns0:SenderID/>
				<ns0:RecipientID/>
			</ns0:Header>
			<ns0:Body>
				<ns0:UniversalEvent>
					<ns0:Event>
						<xsl:variable name="ProcedureCode" select="normalize-space(s0:OutputCommonField/s0:ProcedureCode)"/>
						<ns0:DataContext>
							<ns0:ActionPurpose>
								<ns0:Code>
									<xsl:choose>
										<xsl:when test="normalize-space(s0:OutputCommonField/s0:OutputMessageCode) = 'SAS1550'">VCR</xsl:when>
										<xsl:otherwise>UNK</xsl:otherwise>
									</xsl:choose>
								</ns0:Code>
								<ns0:Description>
									<xsl:value-of select ="normalize-space(s0:OutputCommonField/s0:OutputMessageCode)"/>
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
						<ns0:EventReference>
							<xsl:value-of select ="normalize-space(s0:PriorNotificationCode)"/>
						</ns0:EventReference>
						<ns0:ContextCollection>
							<xsl:variable name="VesselCodeOriginal" select="normalize-space(s0:VesselCodeOriginal)"/>
							<xsl:variable name="VoyageNumberOriginal" select="normalize-space(s0:VoyageNumberOriginal)"/>
							<xsl:variable name="CarrierCodeOriginal" select="normalize-space(s0:CarrierCodeOriginal)"/>
							<xsl:variable name="PortOfLoadingCodeOriginal" select="normalize-space(s0:PortOfLoadingCodeOriginal)"/>
							<xsl:variable name="PortOfLoadingSuffixOriginal" select="normalize-space(s0:PortOfLoadingSuffixOriginal)"/>
							<xsl:variable name="PortOfDischargeCodeOriginal" select="normalize-space(s0:PortOfDischargeCodeOriginal)"/>
							<xsl:variable name="VesselCodeNew" select="normalize-space(s0:VesselCodeNew)"/>
							<xsl:variable name="VoyageNumberNew" select="normalize-space(s0:VoyageNumberNew)"/>
							<xsl:variable name="CarrierCodeNew" select="normalize-space(s0:CarrierCodeNew)"/>
							<xsl:variable name="PortOfLoadingCodeNew" select="normalize-space(s0:PortOfLoadingCodeNew)"/>
							<xsl:variable name="PortOfLoadingSuffixNew" select="normalize-space(s0:PortOfLoadingSuffixNew)"/>

							<xsl:call-template name="ContextKeyValuePair">
								<xsl:with-param name="type" select="'InternalTransactionNumber'" />
								<xsl:with-param name="value" select="s0:OutputCommonField/s0:InputMessageID" />
							</xsl:call-template>
							<xsl:call-template name="ContextKeyValuePair">
								<xsl:with-param name="type" select="'VesselCallSign'" />
								<xsl:with-param name="value" select="$VesselCodeOriginal" />
							</xsl:call-template>
							<xsl:call-template name="ContextKeyValuePair">
								<xsl:with-param name="type" select="'VoyageNumber'" />
								<xsl:with-param name="value" select="$VoyageNumberOriginal" />
							</xsl:call-template>
							<xsl:call-template name="ContextKeyValuePair">
								<xsl:with-param name="type" select="'CarrierCode'" />
								<xsl:with-param name="value" select="$CarrierCodeOriginal" />
							</xsl:call-template>
							<xsl:call-template name="ContextKeyValuePair">
								<xsl:with-param name="type" select="'PortOfLoadingUNLOCO'" />
								<xsl:with-param name="value" select="$PortOfLoadingCodeOriginal" />
							</xsl:call-template>
							<xsl:call-template name="ContextKeyValuePair">
								<xsl:with-param name="type" select="'PortOfLoadingSuffix'" />
								<xsl:with-param name="value" select="$PortOfLoadingSuffixOriginal" />
							</xsl:call-template>
							<xsl:call-template name="ContextKeyValuePair">
								<xsl:with-param name="type" select="'PortOfDischargeCode'" />
								<xsl:with-param name="value" select="$PortOfDischargeCodeOriginal" />
							</xsl:call-template>
							<xsl:call-template name="ContextKeyValuePair">
								<xsl:with-param name="type" select="'VesselCallSignNew'" />
								<xsl:with-param name="value" select="$VesselCodeNew" />
							</xsl:call-template>
							<xsl:call-template name="ContextKeyValuePair">
								<xsl:with-param name="type" select="'VoyageNumberNew'" />
								<xsl:with-param name="value" select="$VoyageNumberNew" />
							</xsl:call-template>
							<xsl:call-template name="ContextKeyValuePair">
								<xsl:with-param name="type" select="'CarrierCodeNew'" />
								<xsl:with-param name="value" select="$CarrierCodeNew" />
							</xsl:call-template>
							<xsl:call-template name="ContextKeyValuePair">
								<xsl:with-param name="type" select="'PortOfLoadingUNLOCONew'" />
								<xsl:with-param name="value" select="$PortOfLoadingCodeNew" />
							</xsl:call-template>
							<xsl:call-template name="ContextKeyValuePair">
								<xsl:with-param name="type" select="'PortOfLoadingSuffixNew'" />
								<xsl:with-param name="value" select="$PortOfLoadingSuffixNew" />
							</xsl:call-template>
							<xsl:element name="ns0:Context">
								<xsl:element name="ns0:Type">
									<xsl:value-of select="'NotificationDetails'"/>
								</xsl:element>
								<xsl:element name="ns0:Value">
									<xsl:value-of select="'&lt;table width=&quot;100%&quot; border=&quot;1&quot; cellpadding=&quot;1&quot; cellspacing=&quot;0&quot; class=&quot;table&quot;&gt;'" disable-output-escaping="no"/>
									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Procedure Code'" />
										<xsl:with-param name="value" select="normalize-space(s0:ProcedureCode)" />
									</xsl:call-template>
									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Internal Procedure Code'" />
										<xsl:with-param name="value" select="normalize-space(s0:InternalProcedureCode)" />
									</xsl:call-template>
									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Vessel Call Sign (Original)'" />
										<xsl:with-param name="value" select="$VesselCodeOriginal" />
									</xsl:call-template>
									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Voyage Number (Original)'" />
										<xsl:with-param name="value" select="$VoyageNumberOriginal" />
									</xsl:call-template>
									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Carrier Code (Original)'" />
										<xsl:with-param name="value" select="$CarrierCodeOriginal" />
									</xsl:call-template>
									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Port of Loading (Original)'" />
										<xsl:with-param name="value" select="$PortOfLoadingCodeOriginal" />
									</xsl:call-template>
									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Port of Loading Suffix (Original)'" />
										<xsl:with-param name="value" select="$PortOfLoadingSuffixOriginal" />
									</xsl:call-template>
									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Port of Discharge (Original)'" />
										<xsl:with-param name="value" select="$PortOfDischargeCodeOriginal" />
									</xsl:call-template>
									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Vessel Call Sign (New)'" />
										<xsl:with-param name="value" select="$VesselCodeNew" />
									</xsl:call-template>
									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Voyage Number (New)'" />
										<xsl:with-param name="value" select="$VoyageNumberNew" />
									</xsl:call-template>
									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Carrier Code (New)'" />
										<xsl:with-param name="value" select="$CarrierCodeNew" />
									</xsl:call-template>
									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Port of Loading (New)'" />
										<xsl:with-param name="value" select="$PortOfLoadingCodeNew" />
									</xsl:call-template>
									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Port of Loading Suffix (New)'" />
										<xsl:with-param name="value" select="$PortOfLoadingSuffixNew" />
									</xsl:call-template>
									<xsl:value-of select="'&lt;/table&gt;'" disable-output-escaping="no"/>

									<!-- Bill Detail Tables -->
									<xsl:value-of select="'&lt;BR/&gt;'" disable-output-escaping="no"/>
									<xsl:value-of select="'&lt;BR/&gt;'" disable-output-escaping="no"/>
									<xsl:value-of select="'&lt;table width=&quot;100%&quot; border=&quot;1&quot; cellpadding=&quot;1&quot; cellspacing=&quot;0&quot; class=&quot;table&quot;&gt;'" disable-output-escaping="no"/>
									<!--Bill Detail Header Line-->
									<xsl:value-of select="'&lt;tr class=&quot;tableheadings&quot;&gt;'" disable-output-escaping="no"/>
									<xsl:value-of select="'&lt;th colspan=&quot;5&quot;&gt;'" disable-output-escaping="no"/>
									<xsl:value-of select="'Bill of Lading Details'" disable-output-escaping="no"/>
									<xsl:value-of select="'&lt;/th&gt;'" disable-output-escaping="no"/>
									<xsl:value-of select="'&lt;/tr&gt;'" disable-output-escaping="no"/>
									<xsl:call-template name="HTMLT-ProcessResult">
										<xsl:with-param name="houseBillName" select="'House B/L Number'" />
										<xsl:with-param name="ProcessResultCode" select="'Process Result Code'" />
										<xsl:with-param name="ProcessResultField" select="'Process Result Field'" />
										<xsl:with-param name="ProcessResultDecritption" select="'Process Result Decritption'" />
										<xsl:with-param name="ProcessResultSuggestion" select="'Process Result Suggestion'" />
									</xsl:call-template>
									<!--Bill Detail Body Line-->
									<xsl:for-each select="s0:HouseBillInfo">
										<xsl:variable name="ProcessResultCode" select="normalize-space(s0:ProcessResultCode/s0:Code)"/>
										<xsl:choose>
											<xsl:when test="$ProcessResultCode and $ProcessResultCode != '00000' and $ProcessResultCode != ''">
												<xsl:variable name="ProcessResultField" select="normalize-space(s0:ProcessResultCode/s0:ErrorFieldID)"/>
												<xsl:variable name="ProcessResultSequence" select="normalize-space(s0:ProcessResultCode/s0:FieldSequence)"/>
												<xsl:call-template name="HTMLT-ProcessResult">
													<xsl:with-param name="houseBillName" select="s0:HouseBillNumber" />
													<xsl:with-param name="ProcessResultCode" select="$ProcessResultCode" />
													<xsl:with-param name="ProcessResultField">
														<xsl:value-of select="'Error occured on Field:&quot;'" />
														<xsl:value-of select="$ProcessResultField" />
														<xsl:value-of select="' - '" />
														<xsl:value-of select="ScriptNS0:GetRecipientCode('JPCustoms' , 'JPCustoms' , 'JP AFR - Import SAS155' , 'Field' , 'FieldName', $ProcessResultField, $ProcedureCode)"/>
														<xsl:value-of select="'&quot;'" />
														<xsl:if test="$ProcessResultSequence != ''">
															<xsl:value-of select="' on '" />
															<xsl:value-of select="$ProcessResultSequence" />
															<xsl:choose>
																<xsl:when test="substring(format-number($ProcessResultSequence,'00000'), 5,1)='1'">
																	<xsl:value-of select="'st'" />
																</xsl:when>
																<xsl:when test="substring(format-number($ProcessResultSequence,'00000'), 5,1)='2'">
																	<xsl:value-of select="'nd'" />
																</xsl:when>
																<xsl:when test="substring(format-number($ProcessResultSequence,'00000'), 5,1)='3'">
																	<xsl:value-of select="'rd'" />
																</xsl:when>
																<xsl:otherwise>
																	<xsl:value-of select="'th'" />
																</xsl:otherwise>
															</xsl:choose>
															<xsl:value-of select="' occurance'" />
														</xsl:if>
													</xsl:with-param>
													<xsl:with-param name="ProcessResultDecritption">
														<xsl:value-of select="ScriptNS0:GetRecipientCode('JPCustoms' , 'JPCustoms' , 'JP AFR - Import SAS155' , 'ErrorDetail' , 'ErrorDescription', $ProcessResultCode, $ProcedureCode)" disable-output-escaping="no"/>
													</xsl:with-param>
													<xsl:with-param name="ProcessResultSuggestion">
														<xsl:value-of select="concat(
																				ScriptNS0:GetRecipientCode('JPCustoms' , 'JPCustoms' , 'JP AFR - Import SAS155' , 'ErrorDetail' , 'ErrorSolutionPartA', $ProcessResultCode, $ProcedureCode),
																				ScriptNS0:GetRecipientCode('JPCustoms' , 'JPCustoms' , 'JP AFR - Import SAS155' , 'ErrorDetail' , 'ErrorSolutionPartB', $ProcessResultCode, $ProcedureCode)
																				)" disable-output-escaping="no"/>
													</xsl:with-param>
												</xsl:call-template>
											</xsl:when>
											<xsl:otherwise>
												<xsl:call-template name="HTMLT-ProcessResult">
													<xsl:with-param name="houseBillName" select="s0:HouseBillNumber" />
													<xsl:with-param name="ProcessResultCode" select="''" />
													<xsl:with-param name="ProcessResultField" select="''" />
													<xsl:with-param name="ProcessResultDecritption" select="''" />
													<xsl:with-param name="ProcessResultSuggestion" select="''" />
												</xsl:call-template>
											</xsl:otherwise>
										</xsl:choose>
									</xsl:for-each>
									<xsl:value-of select="'&lt;/table&gt;'" disable-output-escaping="no"/>
								</xsl:element>
							</xsl:element>

							<xsl:for-each select="s0:HouseBillInfo">
								<xsl:call-template name="BillInfoContextKeyValuePair">
									<xsl:with-param name="billNo" select="s0:HouseBillNumber" />
									<xsl:with-param name="processResultCode" select ="s0:ProcessResultCode/s0:Code"/>
								</xsl:call-template>
							</xsl:for-each>
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

	<xsl:template name="HTMLT-ProcessResult" match="*">
		<xsl:param name="houseBillName" />
		<xsl:param name="ProcessResultCode" />
		<xsl:param name="ProcessResultField" />
		<xsl:param name="ProcessResultDecritption" />
		<xsl:param name="ProcessResultSuggestion" />
		<xsl:if test="$houseBillName">
			<xsl:value-of select="'&lt;tr&gt;'" disable-output-escaping="no"/>
			<xsl:value-of select="'&lt;td&gt;'" disable-output-escaping="no"/>
			<xsl:value-of select="$houseBillName"/>
			<xsl:value-of select="'&lt;/td&gt;'" disable-output-escaping="no"/>
			<xsl:value-of select="'&lt;td&gt;'" disable-output-escaping="no"/>
			<xsl:value-of select="$ProcessResultCode"/>
			<xsl:value-of select="'&lt;/td&gt;'" disable-output-escaping="no"/>
			<xsl:value-of select="'&lt;td&gt;'" disable-output-escaping="no"/>
			<xsl:value-of select="$ProcessResultField"/>
			<xsl:value-of select="'&lt;/td&gt;'" disable-output-escaping="no"/>
			<xsl:value-of select="'&lt;td&gt;'" disable-output-escaping="no"/>
			<xsl:value-of select="$ProcessResultDecritption"/>
			<xsl:value-of select="'&lt;/td&gt;'" disable-output-escaping="no"/>
			<xsl:value-of select="'&lt;td&gt;'" disable-output-escaping="no"/>
			<xsl:value-of select="$ProcessResultSuggestion"/>
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

	<xsl:template name="BillInfoContextKeyValuePair" match="*">
		<xsl:param name="billNo" />
		<xsl:param name="processResultCode" />
		<xsl:if test="$billNo!=''">
			<ns0:Context>
				<ns0:Type>
					<xsl:value-of select="'BillInformation'" />
				</ns0:Type>
				<ns0:Value></ns0:Value>
				<ns0:SubContextCollection>
					<ns0:SubContext>
						<ns0:Type>
							<xsl:value-of select="'BillNumber'" />
						</ns0:Type>
						<ns0:Value>
							<xsl:value-of select="normalize-space($billNo)" />
						</ns0:Value>
					</ns0:SubContext>
					<xsl:if test="$processResultCode!='00000' and $processResultCode!=''">
						<ns0:SubContext>
							<ns0:Type>
								<xsl:value-of select="'ProcessResultCode'" />
							</ns0:Type>
							<ns0:Value>
								<xsl:value-of select="normalize-space($processResultCode)" />
							</ns0:Value>
						</ns0:SubContext>
					</xsl:if>
				</ns0:SubContextCollection>
			</ns0:Context>
		</xsl:if>
	</xsl:template>

	<msxsl:script language="C#" implements-prefix="userCSharp">
		<![CDATA[
		]]>
	</msxsl:script>

</xsl:stylesheet>
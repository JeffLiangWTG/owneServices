<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
		xmlns:msxsl="urn:schemas-microsoft-com:xslt"
		xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var" exclude-result-prefixes="msxsl var s0" version="1.0"
		xmlns:s0="http://cargowise.com/ehub/products/jpcustoms/2017/01"
		xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2011/11"
		xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
		xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
	<xsl:output omit-xml-declaration="yes" indent="no" version="1.0" method="xml" />

	<xsl:template match="/">
		<xsl:apply-templates select="/s0:SAS157Output" />
	</xsl:template>

	<xsl:template match="s0:SAS157Output">
		<ns0:UniversalInterchangeInclude>
			<ns0:Header>
				<ns0:SenderID />
				<ns0:RecipientID />
			</ns0:Header>
			<ns0:Body>
				<ns0:UniversalEvent>
					<ns0:Event>
						<ns0:DataContext>
							<ns0:ActionPurpose>
								<ns0:Code>
									<xsl:choose>
										<xsl:when test="normalize-space(s0:OutputCommonField/s0:OutputMessageCode) = 'SAS1570'">NHS</xsl:when>
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
							<xsl:variable name="DateOfMessageReceipt" select="ScriptNS0:ConvertToDateTimeString(s0:OutputCommonField/s0:DateOfMessageReceipt, 'yyyyMMddHHmm')" />
							<xsl:if test="$DateOfMessageReceipt">
								<xsl:value-of select="ScriptNS0:ConvertLocalXmlDateTimeStringToUTC($DateOfMessageReceipt,'09:00', 'O', '')"/>
							</xsl:if>
						</ns0:EventTime>
						<ns0:EventType>MSC</ns0:EventType>
						<ns0:EventReference>
							<xsl:value-of select ="normalize-space(s0:PriorNotificationCode)"/>
						</ns0:EventReference>
						<ns0:ContextCollection>
							<xsl:call-template name="ContextKeyValuePair">
								<xsl:with-param name="type" select="'InternalTransactionNumber'" />
								<xsl:with-param name="value" select="s0:OutputCommonField/s0:InputMessageID" />
							</xsl:call-template>

							<xsl:call-template name="ContextKeyValuePair">
								<xsl:with-param name="type" select="'MBOLNumber'" />
								<xsl:with-param name="value" select="s0:MasterBillNumber" />
							</xsl:call-template>

							<xsl:call-template name="ContextKeyValuePair">
								<xsl:with-param name="type" select="'VesselCallSign'" />
								<xsl:with-param name="value" select="s0:LadenVesselCode" />
							</xsl:call-template>

							<xsl:call-template name="ContextKeyValuePair">
								<xsl:with-param name="type" select="'VesselName'" />
								<xsl:with-param name="value" select="s0:LadenVesselName" />
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

							<xsl:call-template name="ContextKeyValuePair">
								<xsl:with-param name="type" select="'NotificationDetails'" />
								<xsl:with-param name="value">
									<xsl:value-of select="'&lt;table width=&quot;100%&quot; border=&quot;1&quot; cellpadding=&quot;1&quot; cellspacing=&quot;0&quot; class=&quot;table&quot;&gt;'" disable-output-escaping="no"/>
									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Master Bill'" />
										<xsl:with-param name="value" select="s0:MasterBillNumber" />
									</xsl:call-template>
									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Vessel Call Sign'" />
										<xsl:with-param name="value" select="s0:LadenVesselCode" />
									</xsl:call-template>
									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Vessel Name'" />
										<xsl:with-param name="value" select="s0:LadenVesselName" />
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
										<xsl:with-param name="name" select="'Port of Loading'" />
										<xsl:with-param name="value" select="s0:PortOfLoadingCode" />
									</xsl:call-template>

									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Port of Loading Suffix'" />
										<xsl:with-param name="value" select="s0:PortOfLoadingSuffix" />
									</xsl:call-template>

									<xsl:if test="s0:DateOfNotification != '' and s0:TimeOfNotification != ''">
										<xsl:call-template name="HTMLTR">
											<xsl:with-param name="name" select="'Date Time of Notification'" />
											<xsl:with-param name="value">
												<xsl:value-of select="ScriptNS0:ConvertToDateTimeString(s0:DateOfNotification, 'yyyyMMdd', s0:TimeOfNotification, 'HHmm', 'yyyy-MM-dd HH:mm')"/>
											</xsl:with-param>
										</xsl:call-template>
									</xsl:if>
									<xsl:value-of select="'&lt;/table&gt;'" disable-output-escaping="no"/>
								</xsl:with-param>
							</xsl:call-template>
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
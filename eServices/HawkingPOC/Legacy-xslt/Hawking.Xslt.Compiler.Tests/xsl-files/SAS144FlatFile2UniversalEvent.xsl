<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
		xmlns:msxsl="urn:schemas-microsoft-com:xslt"
		xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var" exclude-result-prefixes="msxsl var s0" version="1.0"
		xmlns:s0="http://cargowise.com/ehub/products/jpcustoms/2017/01"
		xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2011/11"
		xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
		xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
	<xsl:output omit-xml-declaration="yes" indent="no" version="1.0" method="xml" />

	<xsl:template match="/">
		<xsl:apply-templates select="/s0:SAS144Output" />
	</xsl:template>

	<xsl:template match="s0:SAS144Output">
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
										<xsl:when test="normalize-space(s0:OutputCommonField/s0:OutputMessageCode) = 'SAS1440'">PRH</xsl:when>
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
								<xsl:with-param name="value" select="s0:VesselCode" />
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
								<xsl:with-param name="type" select="'NotificationDetails'" />
								<xsl:with-param name="value">
									<xsl:value-of select="'&lt;table width=&quot;100%&quot; border=&quot;1&quot; cellpadding=&quot;1&quot; cellspacing=&quot;0&quot; class=&quot;table&quot;&gt;'" disable-output-escaping="no"/>
									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Master Bill'" />
										<xsl:with-param name="value" select="s0:MasterBillNumber" />
									</xsl:call-template>
									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Jurisdictional Customs Office Code'" />
										<xsl:with-param name="value" select="s0:JurisdictionalCustomsOfficeCode" />
									</xsl:call-template>
									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Jurisdictional Customs Office Name'" />
										<xsl:with-param name="value" select="s0:JurisdictionalCustomsOfficeName" />
									</xsl:call-template>
									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Vessel Code'" />
										<xsl:with-param name="value" select="s0:VesselCode" />
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

									<xsl:if test="s0:DateOfAdvanceCargoInformationRegistration != '' and s0:TimeOfAdvanceCargoInformationRegistration != ''">
										<xsl:call-template name="HTMLTR">
											<xsl:with-param name="name" select="'Date Time of Advance Cargo Information Registration'" />
											<xsl:with-param name="value">
												<xsl:value-of select="ScriptNS0:ConvertToDateTimeString(s0:DateOfAdvanceCargoInformationRegistration, 'yyyyMMdd', s0:TimeOfAdvanceCargoInformationRegistration, 'HHmm', 'yyyy-MM-dd HH:mm')"/>
											</xsl:with-param>
										</xsl:call-template>
									</xsl:if>
									<xsl:if test="s0:DateOfDeparture != '' and s0:TimeOfDeparture != ''">
										<xsl:call-template name="HTMLTR">
											<xsl:with-param name="name" select="'Date Time of Departure'" />
											<xsl:with-param name="value">
												<xsl:value-of select="ScriptNS0:ConvertToDateTimeString(s0:DateOfDeparture, 'yyyyMMdd', s0:TimeOfDeparture, 'HHmm', 'yyyy-MM-dd HH:mm')"/>
											</xsl:with-param>
										</xsl:call-template>
									</xsl:if>

									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Contact Name'" />
										<xsl:with-param name="value" select="s0:NameForContact" />
									</xsl:call-template>

									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Contact Telephone Number'" />
										<xsl:with-param name="value" select="s0:TelephoneNumberForContact" />
									</xsl:call-template>

									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Contact FAX Number'" />
										<xsl:with-param name="value" select="s0:FAXNumberForContact" />
									</xsl:call-template>

									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Contact E-Mail Address'" />
										<xsl:with-param name="value" select="s0:EmailAddressForContact" />
									</xsl:call-template>

									<xsl:value-of select="'&lt;/table&gt;'" disable-output-escaping="no"/>

									<!-- Bill Detail Tables -->
									<xsl:value-of select="'&lt;BR/&gt;'" disable-output-escaping="no"/>
									<xsl:value-of select="'&lt;BR/&gt;'" disable-output-escaping="no"/>
									<xsl:value-of select="'&lt;table width=&quot;100%&quot; border=&quot;1&quot; cellpadding=&quot;1&quot; cellspacing=&quot;0&quot; class=&quot;table&quot;&gt;'" disable-output-escaping="no"/>
									<!--Bill Detail Header Line-->
									<xsl:value-of select="'&lt;tr class=&quot;tableheadings&quot;&gt;'" disable-output-escaping="no"/>
									<xsl:value-of select="'&lt;th colspan=&quot;4&quot;&gt;'" disable-output-escaping="no"/>
									<xsl:value-of select="'Bill of Lading Details'" disable-output-escaping="no"/>
									<xsl:value-of select="'&lt;/th&gt;'" disable-output-escaping="no"/>
									<xsl:value-of select="'&lt;/tr&gt;'" disable-output-escaping="no"/>
									<xsl:call-template name="HTMLTRForLadingDetails">
										<xsl:with-param name="name" select="'House B/L Number'" />
										<xsl:with-param name="value1" select="'Code'" />
										<xsl:with-param name="value2" select="'Description'" />
										<xsl:with-param name="value3" select="'Comment'" />
									</xsl:call-template>
									<!--Bill Detail Body Line-->
									<xsl:for-each select="s0:HouseBillInfo">
										<xsl:call-template name="HTMLTRForLadingDetails">
											<xsl:with-param name="name" select="s0:HouseBillNumber" />
											<xsl:with-param name="value1" select="s0:PriorNotificationCodeForTheRelevantHouseBill" />
											<xsl:with-param name="value2" select="s0:PriorNotificationStatusOfRelevantHouseBill" />
											<xsl:with-param name="value3" select="s0:SubjectOfPriorNotificationForTheRelevantHouseBill" />
										</xsl:call-template>
									</xsl:for-each>
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

	<xsl:template name="HTMLTRForLadingDetails" match="*">
		<xsl:param name="name" />
		<xsl:param name="value1" />
		<xsl:param name="value2" />
		<xsl:param name="value3" />
		<xsl:if test="$name">
			<xsl:value-of select="'&lt;tr&gt;'" disable-output-escaping="no"/>
			<xsl:value-of select="'&lt;td&gt;'" disable-output-escaping="no"/>
			<xsl:value-of select="$name"/>
			<xsl:value-of select="'&lt;/td&gt;'" disable-output-escaping="no"/>
			<xsl:value-of select="'&lt;td&gt;'" disable-output-escaping="no"/>
			<xsl:value-of select="$value1"/>
			<xsl:value-of select="'&lt;/td&gt;'" disable-output-escaping="no"/>
			<xsl:value-of select="'&lt;td&gt;'" disable-output-escaping="no"/>
			<xsl:value-of select="$value2"/>
			<xsl:value-of select="'&lt;/td&gt;'" disable-output-escaping="no"/>
			<xsl:value-of select="'&lt;td&gt;'" disable-output-escaping="no"/>
			<xsl:value-of select="$value3"/>
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
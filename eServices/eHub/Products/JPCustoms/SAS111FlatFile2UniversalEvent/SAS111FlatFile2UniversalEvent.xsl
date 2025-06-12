<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
		xmlns:msxsl="urn:schemas-microsoft-com:xslt"
		xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var" exclude-result-prefixes="msxsl var s0" version="1.0"
		xmlns:s0="http://cargowise.com/ehub/products/jpcustoms/2013/09"
		xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2011/11"
		xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
		xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
	<xsl:output omit-xml-declaration="yes" indent="no" version="1.0" method="xml" />

	<xsl:template match="/">
		<xsl:apply-templates select="/s0:SAS111Output" />
	</xsl:template>

	<xsl:template match="s0:SAS111Output">
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
										<xsl:when test="normalize-space(s0:OutputCommonField/s0:OutputMessageCode) = 'SAS1110'">RAR</xsl:when>
										<xsl:when test="normalize-space(s0:OutputCommonField/s0:OutputMessageCode) = 'SAS1120'">RAC</xsl:when>
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

							<xsl:if test ="s0:MasterBillOfLadingNumber != ''">
							<xsl:call-template name="ContextKeyValuePair">
								<xsl:with-param name="type" select="'MBOLNumber'" />
								<xsl:with-param name="value" select="s0:MasterBillOfLadingNumber" />
							</xsl:call-template>
							<xsl:call-template name="ContextKeyValuePair">
								<xsl:with-param name="type" select="'HBOLNumber'" />
								<xsl:with-param name="value" select="s0:HouseBillOfLadingNumber" />
							</xsl:call-template>
							</xsl:if>	
							<xsl:if test="s0:MasterBillOfLadingNumber = ''">
							<xsl:call-template name="ContextKeyValuePair">
								<xsl:with-param name="type" select="'MBOLNumber'" />
								<xsl:with-param name="value" select="s0:HouseBillOfLadingNumber" />
							</xsl:call-template>
							</xsl:if>

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
								<xsl:with-param name="type" select="'PriorNotificationCode'" />
								<xsl:with-param name="value" select="s0:PriorNotificationCode" />
							</xsl:call-template>

							<xsl:call-template name="ContextKeyValuePair">
								<xsl:with-param name="type" select="'PriorNotificationSubject'" />
								<xsl:with-param name="value" select="s0:PriorNotificationsubject" />
							</xsl:call-template>

							<xsl:call-template name="ContextKeyValuePair">
								<xsl:with-param name="type" select="'NotificationDetails'" />
								<xsl:with-param name="value">
									<xsl:value-of select="'&lt;table width=&quot;100%&quot; border=&quot;1&quot; cellpadding=&quot;1&quot; cellspacing=&quot;0&quot; class=&quot;table&quot;&gt;'" disable-output-escaping="no"/>
									<xsl:if test ="s0:MasterBillOfLadingNumber != ''">
										<xsl:call-template name="HTMLTR">
											<xsl:with-param name="name" select="'Master Bill of Lading Number'" />
											<xsl:with-param name="value" select="s0:MasterBillOfLadingNumber" />
										</xsl:call-template>
										<xsl:call-template name="HTMLTR">
											<xsl:with-param name="name" select="'House Bill of Lading Number'" />
											<xsl:with-param name="value" select="s0:HouseBillOfLadingNumber" />
										</xsl:call-template>
									</xsl:if>
									<xsl:if test ="s0:MasterBillOfLadingNumber = ''">
										<xsl:call-template name="HTMLTR">
											<xsl:with-param name="name" select="'Master Bill of Lading Number'" />
											<xsl:with-param name="value" select="s0:HouseBillOfLadingNumber" />
										</xsl:call-template>
									</xsl:if>

									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Prior Notification Code'" />
										<xsl:with-param name="value" select="s0:PriorNotificationCode" />
									</xsl:call-template>

									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Prior Notification Subject'" />
										<xsl:with-param name="value" select="s0:PriorNotificationsubject" />
									</xsl:call-template>

									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Jurisdictional Customs Office Code'" />
										<xsl:with-param name="value" select="s0:JurisdictionalCustomsofficeCode" />
									</xsl:call-template>

									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Jurisdictional Customs Office Name'" />
										<xsl:with-param name="value" select="s0:JurisdictionalCustomsofficeName" />
									</xsl:call-template>

									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Vessel Call Sign'" />
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

									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Date Time Of Advance Cargo Information Registration'" />
										<xsl:with-param name="value" select="ScriptNS0:ConvertToDateTimeString(s0:DateOfAdvanceCargoInformationRegistration, 'yyyyMMdd', s0:TimeOfAdvanceCargoInformationRegistration, 'HHmm', 'yyyy-MM-dd HH:mm')" />
									</xsl:call-template>

									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Date Time Of Departure'" />
										<xsl:with-param name="value" select="ScriptNS0:ConvertToDateTimeString(s0:DateOfDeparture, 'yyyyMMdd', s0:TimeOfDeparture, 'HHmm', 'yyyy-MM-dd HH:mm')" />
									</xsl:call-template>

									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Details Of Notifications Directions'" />
										<xsl:with-param name="value" select="concat(s0:DetailsOfNotificationsDirections1, s0:DetailsOfNotificationsDirections2
                ,s0:DetailsOfNotificationsDirections3, s0:DetailsOfNotificationsDirections4
                ,s0:DetailsOfNotificationsDirections5, s0:DetailsOfNotificationsDirections6
                ,s0:DetailsOfNotificationsDirections7, s0:DetailsOfNotificationsDirections8
                ,s0:DetailsOfNotificationsDirections9, s0:DetailsOfNotificationsDirections10
                )" />
									</xsl:call-template>

									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Contact Name'" />
										<xsl:with-param name="value" select="s0:ContactName" />
									</xsl:call-template>

									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Contact Telephone Number'" />
										<xsl:with-param name="value" select="s0:ContactTelephonenumber" />
									</xsl:call-template>

									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Contact Fax Number'" />
										<xsl:with-param name="value" select="s0:ContactFaxNumber" />
									</xsl:call-template>

									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Contact EMail Address'" />
										<xsl:with-param name="value" select="s0:ContactEMailAddress" />
									</xsl:call-template>
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
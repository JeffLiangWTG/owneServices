<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
		xmlns:msxsl="urn:schemas-microsoft-com:xslt"
		xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var" exclude-result-prefixes="msxsl var s0 ScriptNS0 ScriptNS1 userCSharp" version="1.0"
		xmlns:s0="http://CargoWise.com/eHub/products/INCustoms/2015/01"
		xmlns="http://www.cargowise.com/Schemas/Universal/2011/11"
		xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
		xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1"
		xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
	<xsl:output omit-xml-declaration="yes" indent="yes" version="1.0" method="xml" />
	

	<xsl:template match="/">
		<xsl:apply-templates select="/s0:CMCHI21A" />
	</xsl:template>

	<xsl:template match="s0:CMCHI21A">
		<UniversalInterchangeInclude>
			<Header>
				<SenderID />
				<RecipientID />
			</Header>
			<Body>
				<UniversalEvent>
					<Event>
						<DataContext>
							<ActionPurpose>
								<Code>CMS</Code>
								<Description>Consol Manifest Sea</Description>
							</ActionPurpose>
							<DataProvider>ICE</DataProvider>
							<DataTargetCollection>
								<DataTarget>
									<Type>INManHeader</Type>
									<Key>INManHeader</Key>
								</DataTarget>
							</DataTargetCollection>
						</DataContext>
						<EventTime>
							<xsl:variable name="DateOfMessageReceipt" select="concat(normalize-space(s0:HREC/s0:MessageDate), concat('T', normalize-space(s0:HREC/s0:MessageTime)))" />
							<xsl:if test="$DateOfMessageReceipt">
								<xsl:value-of select="ScriptNS1:ConvertLocalXmlDateTimeStringToUTC($DateOfMessageReceipt,'05:30', 'O', '')"/>
							</xsl:if>
						</EventTime>
						<EventType>MSC</EventType>
						<xsl:variable name="NotificationSubjectCode" select="s0:Acknowledgement[1]/s0:ErrorCode[1]/s0:Code"/>
						<EventReference>
							<xsl:value-of select ="'CMS-'"/>
							<xsl:choose>
								<xsl:when test="$NotificationSubjectCode='000'">
									<xsl:value-of select="'ACCEPTED'"/>
								</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="'REJECTED'"/>
								</xsl:otherwise>
							</xsl:choose>
						</EventReference>
						<ContextCollection>
							<xsl:variable name="InternalTransactionNumber" select="s0:HREC/s0:InterchangeNo" />
							<xsl:if test="$InternalTransactionNumber">
								<xsl:call-template name="ContextKeyValuePair">
									<xsl:with-param name="type" select="'InternalTransactionNumber'" />
									<xsl:with-param name="value" select="$InternalTransactionNumber" />
								</xsl:call-template>
							</xsl:if>
							<xsl:variable name="VesselCallSign" select="s0:Acknowledgement[1]/s0:VesselCallSign" />
							<xsl:if test="$VesselCallSign">
								<xsl:call-template name="ContextKeyValuePair">
									<xsl:with-param name="type" select="'VesselCallSign'" />
									<xsl:with-param name="value" select="$VesselCallSign" />
								</xsl:call-template>
							</xsl:if>
							<xsl:variable name="VoyageNumber" select="s0:Acknowledgement[1]/s0:VoyageNumber" />
							<xsl:if test="$VoyageNumber">
								<xsl:call-template name="ContextKeyValuePair">
									<xsl:with-param name="type" select="'VoyageNumber'" />
									<xsl:with-param name="value" select="$VoyageNumber" />
								</xsl:call-template>
							</xsl:if>
							<xsl:variable name="MBOLNumber" select="s0:Acknowledgement[1]/s0:MasterBillOfLadingNo" />
							<xsl:if test="$MBOLNumber">
								<xsl:call-template name="ContextKeyValuePair">
									<xsl:with-param name="type" select="'MBOLNumber'" />
									<xsl:with-param name="value" select="$MBOLNumber" />
								</xsl:call-template>
							</xsl:if>
							<xsl:variable name="CustomsHouseCode" select="s0:Acknowledgement[1]/s0:CustomsHouseCode" />
							<xsl:variable name="CARNNumber" select="s0:Acknowledgement[1]/s0:CARNNumber" />
							<xsl:variable name="TestModeIndicator" select="s0:HREC/s0:TestOrProd" />
							<xsl:variable name="IGMNumber" select="s0:Acknowledgement[1]/s0:IGMNumber" />
							<xsl:variable name="IGMDate" select="s0:Acknowledgement[1]/s0:IGMDate" />
							<xsl:variable name="MBOLCutDate" select="s0:Acknowledgement[1]/s0:MasterBillOfLadingDate" />
							<xsl:variable name="LineNumber" select="s0:Acknowledgement[1]/s0:LineNumber" />
							<xsl:element name="Context">
								<xsl:element name="Type">
									<xsl:value-of select="'NotificationDetails'"/>
								</xsl:element>
								<xsl:element name="Value">
									<xsl:value-of select="'&lt;table width=&quot;100%&quot; border=&quot;1&quot; cellpadding=&quot;1&quot; cellspacing=&quot;0&quot; class=&quot;table&quot;&gt;'" disable-output-escaping="no"/>
									<xsl:if test="$TestModeIndicator and $TestModeIndicator='T'">
										<xsl:call-template name="HTMLTR">
											<xsl:with-param name="name" select="'TestMode'" />
											<xsl:with-param name="value" select="'TestMode'" />
											<xsl:with-param name="forceOutput" select="1" />
											<xsl:with-param name="bold" select="1" />
										</xsl:call-template>
									</xsl:if>
									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Interchange'" />
										<xsl:with-param name="value" select="$InternalTransactionNumber" />
									</xsl:call-template>
									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Customs House'" />
										<xsl:with-param name="value" select="$CustomsHouseCode" />
									</xsl:call-template>
									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'CARN Number'" />
										<xsl:with-param name="value" select="$CARNNumber" />
									</xsl:call-template>
									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'IGM Number'" />
										<xsl:with-param name="value" select="$IGMNumber" />
									</xsl:call-template>
									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'IGM Date'" />
										<xsl:with-param name="value" select="$IGMDate" />
									</xsl:call-template>
									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Vessel Call Sign'" />
										<xsl:with-param name="value" select="$VesselCallSign" />
									</xsl:call-template>
									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Vessel Voyage'" />
										<xsl:with-param name="value" select="$VoyageNumber" />
									</xsl:call-template>
									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Master Bill Of Lading Number'" />
										<xsl:with-param name="value" select="$MBOLNumber" />
										<xsl:with-param name="forceOutput" select="1" />
									</xsl:call-template>
									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Master Bill Cut Date'" />
										<xsl:with-param name="value" select="$MBOLCutDate" />
									</xsl:call-template>
									<xsl:call-template name="HTMLTR">
										<xsl:with-param name="name" select="'Line Number'" />
										<xsl:with-param name="value" select="$LineNumber" />
									</xsl:call-template>
									<xsl:for-each select="s0:Acknowledgement[1]/s0:ErrorCode/s0:Code">
										<xsl:if test="text() and text() != '000' and text() != ''">
											<xsl:variable name="position" select="position()"/>
											<xsl:call-template name="HTMLTRSpan">
												<xsl:with-param name="name" select="concat('Process Result - ', $position)"/>
											</xsl:call-template>
											<xsl:call-template name="HTMLTR">
												<xsl:with-param name="name" select="'Code'"/>
												<xsl:with-param name="value" select="text()"/>
											</xsl:call-template>
											<xsl:call-template name="HTMLTR">
												<xsl:with-param name="name" select="'Description'"/>
												<xsl:with-param name="value">
													<xsl:value-of select="ScriptNS0:GetRecipientCode('INCustoms' , 'INCustoms' , 'ICES - Sea Consol Manifest' , 'ErrorDetail' , 'ErrorDescription', text())" disable-output-escaping="no"/>
												</xsl:with-param>
											</xsl:call-template>
											<xsl:call-template name="HTMLTR">
												<xsl:with-param name="name" select="'Suggestion'"/>
												<xsl:with-param name="value">
													<xsl:value-of select="ScriptNS0:GetRecipientCode('INCustoms' , 'INCustoms' , 'ICES - Sea Consol Manifest' , 'ErrorDetail' , 'ErrorSolution', text())" disable-output-escaping="no"/>
												</xsl:with-param>
											</xsl:call-template>
										</xsl:if>
									</xsl:for-each>
									<xsl:value-of select="'&lt;/table&gt;'" disable-output-escaping="no"/>
								</xsl:element>
							</xsl:element>
						</ContextCollection>
					</Event>
				</UniversalEvent>
			</Body>
		</UniversalInterchangeInclude>
	</xsl:template>

	<xsl:template name="HTMLTR" match="*">
		<xsl:param name="name" />
		<xsl:param name="value" />
		<xsl:param name="forceOutput" select ="0"/>
		<xsl:param name="bold" select ="0"/>
		<xsl:if test="$name and ($value != '' or $forceOutput = '1')">
			<xsl:value-of select="'&lt;tr&gt;'" disable-output-escaping="no"/>
			<xsl:value-of select="'&lt;td&gt;'" disable-output-escaping="no"/>
			<xsl:if test="$bold = '1'">
				<xsl:value-of select="'&lt;b&gt;'" disable-output-escaping="no"/>
			</xsl:if>
			<xsl:value-of select="$name"/>
			<xsl:if test="$bold = '1'">
				<xsl:value-of select="'&lt;/b&gt;'" disable-output-escaping="no"/>
			</xsl:if>
			<xsl:value-of select="'&lt;/td&gt;'" disable-output-escaping="no"/>
			<xsl:value-of select="'&lt;td&gt;'" disable-output-escaping="no"/>
			<xsl:if test="$bold = '1'">
				<xsl:value-of select="'&lt;b&gt;'" disable-output-escaping="no"/>
			</xsl:if>
			<xsl:value-of select="$value"/>
			<xsl:if test="$bold = '1'">
				<xsl:value-of select="'&lt;/b&gt;'" disable-output-escaping="no"/>
			</xsl:if>
			<xsl:value-of select="'&lt;/td&gt;'" disable-output-escaping="no"/>
			<xsl:value-of select="'&lt;/tr&gt;'" disable-output-escaping="no"/>
		</xsl:if>
	</xsl:template>

	<xsl:template name="HTMLTRSpan" match="*">
		<xsl:param name="name" />
		<xsl:if test="$name">
			<xsl:value-of select="'&lt;tr&gt;'" disable-output-escaping="no"/>
			<xsl:value-of select="'&lt;td colspan=2&gt;'" disable-output-escaping="no"/>
			<xsl:value-of select="$name"/>
			<xsl:value-of select="'&lt;/td&gt;'" disable-output-escaping="no"/>
			<xsl:value-of select="'&lt;/tr&gt;'" disable-output-escaping="no"/>
		</xsl:if>
	</xsl:template>

	<xsl:template name="ContextKeyValuePair" match="*">
		<xsl:param name="type" />
		<xsl:param name="value" />
		<xsl:if test="$type and $value">
			<Context>
				<Type>
					<xsl:value-of select="normalize-space($type)" />
				</Type>
				<Value>
					<xsl:value-of select="normalize-space($value)" />
				</Value>
			</Context>
		</xsl:if>
	</xsl:template>

	<msxsl:script language="C#" implements-prefix="userCSharp">
		<![CDATA[
		]]>
</msxsl:script>

</xsl:stylesheet>
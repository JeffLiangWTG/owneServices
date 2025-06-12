<?xml version="1.0" encoding="Windows-1252"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:msxsl="urn:schemas-microsoft-com:xslt"
				exclude-result-prefixes="msxsl ns0 s0 CodeMapper ContextAccessor DataModelAccessor userCSharp"
				version="1.0"
				xmlns:s0="http://cargowise.com/ehub/core/2011/02"
				xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2012/11"
				xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
				xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
				xmlns:DataModelAccessor="http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"
				xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">

	<xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes"/>

	<xsl:template match="/">
		<xsl:apply-templates select="Nallian"/>
	</xsl:template>

	<xsl:template match="Nallian">

		<xsl:variable name="SenderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
		<xsl:variable name="RecipientID_DestinationParty" select="ContextAccessor:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
		<xsl:variable name="RecipientID">
			<xsl:choose>
				<xsl:when test="RecipientID_DestinationParty = ''">
					<xsl:value-of select="$SenderID"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$RecipientID_DestinationParty"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:variable name="Reference" select="Reference/text()" />
		<xsl:variable name="TrackingID" select="ContextAccessor:GetContextProperty('MessageTrackingID', 'http://cargowise.com/ehub/tracking/2010/06')"/>

		<xsl:variable name="InsertSubscriptionValue" select="DataModelAccessor:InsertSubscriptionValue('AIRAWB',
						$SenderID, $RecipientID,
						concat(substring-before($Reference, '-'), '-', $TrackingID, '-Inbound'),
						$TrackingID, 'TrackingID')" />

		<xsl:variable name="StatusCode" select="StatusCode/text()" />

		<xsl:variable name="ReferenceNumber">
			<xsl:value-of select="concat(substring($Reference, 1, 3), '-', substring(substring-before($Reference, '-'), 4))"/>
		</xsl:variable>

		<xsl:variable name="EventType">
			<xsl:choose>
				<xsl:when test="$StatusCode = '1'">
					<xsl:value-of select="'IRA'"/>
					<xsl:variable name="Filename" select="ContextAccessor:SetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06', concat('FMA_', $ReferenceNumber, '__', CodeMapper:CallActionProcedureHelper('GetAirlineCodeFromPrefix', '', '@Prefix', substring($ReferenceNumber, 1, 3)), '_'))"/>
				</xsl:when>
				<xsl:when test="$StatusCode = '2'">
					<xsl:value-of select="'IRJ'"/>
					<xsl:variable name="Filename" select="ContextAccessor:SetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06', concat('FNA_', $ReferenceNumber, '__', CodeMapper:CallActionProcedureHelper('GetAirlineCodeFromPrefix', '', '@Prefix', substring($ReferenceNumber, 1, 3)), '_'))"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:variable name="Throw" select="userCSharp:Throw(concat('The StatusCode [', $StatusCode, '] is invalid.'))"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:variable name="Reason">
			<xsl:choose>
				<xsl:when test="$StatusCode = '1'">
					<xsl:value-of select="'FWB/FHL SUCCESSFULLY RECEIVED'"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="'FWB/FHL WAS NOT ACCEPTED'"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:variable name="EventTime">
			<xsl:variable name="DateTimeUtc" select="DateTimeUtc/text()" />
			<xsl:value-of select="substring-before($DateTimeUtc, '.')"/>
		</xsl:variable>

		<UniversalInterchange xmlns="http://www.cargowise.com/Schemas/Universal/2011/11">
			<Header>
				<SenderID/>
				<RecipientID/>
			</Header>
			<Body>
				<UniversalEvent xmlns="http://www.cargowise.com/Schemas/Universal/2012/11" version="2.0">
					<Event>
						<EventType>
							<xsl:value-of select="$EventType"/>
						</EventType>
						<EventParameters>
							<Department>Carrier</Department>
							<MessageType>FWB</MessageType>
							<Reason>
								<xsl:value-of select="$Reason"/>
							</Reason>
							<ReferenceNumber>
								<xsl:value-of select="$ReferenceNumber"/>
							</ReferenceNumber>
						</EventParameters>
						<EventTime>
							<xsl:value-of select="$EventTime"/>
						</EventTime>
						<DataContext/>
						<ContextCollection>
							<Context>
								<Type>MAWBNumber</Type>
								<Value>
									<xsl:value-of select="$ReferenceNumber"/>
								</Value>
							</Context>
						</ContextCollection>
					</Event>
				</UniversalEvent>
			</Body>
		</UniversalInterchange>
	</xsl:template>

	<msxsl:script language="C#" implements-prefix="userCSharp">
		<![CDATA[
		public void Throw(string message)
		{
			throw new ArgumentException(message);
		}
		]]>
	</msxsl:script>
</xsl:stylesheet>

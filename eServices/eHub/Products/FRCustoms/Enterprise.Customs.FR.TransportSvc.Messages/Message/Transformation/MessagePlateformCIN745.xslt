<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
	<xsl:output method="xml" indent="yes"/>
	<xsl:template match="root">
		<Message>
			<EnveloppeEasyLog>
				<Application>
					<xsl:value-of select="EnveloppeEasylog/Application"/>
				</Application>
				<CodeSite>
					<xsl:value-of select="EnveloppeEasylog/SenderId"/>
				</CodeSite>
				<TypeMessage>
					<xsl:value-of select="EnveloppeEasylog/MessageType"/>
				</TypeMessage>
				<IdDossier>
					<xsl:value-of select="EnveloppeEasylog/IdDossier"/>
				</IdDossier>
				<TransactionId>
					<xsl:value-of select="EnveloppeEasylog/TransactionId"/>
				</TransactionId>
				<PartyId>
					<xsl:value-of select="EnveloppeEasylog/PartyId"/>
				</PartyId>
				<NumMessage>
					<xsl:value-of select="EnveloppeEasylog/SequentialNumber"/>
				</NumMessage>
				<RefDossier>
					<xsl:value-of select="EnveloppeEasylog/DeclarationReference"/>
				</RefDossier>
				<RefDELTA>
					<xsl:value-of select="EnveloppeEasylog/DeltaReference"/>
				</RefDELTA>
				<Flux>
					<xsl:value-of select="EnveloppeEasylog/Flux"/>
				</Flux>
				<CodeClient>
					<xsl:value-of select="EnveloppeEasylog/CodeClient"/>
				</CodeClient>
				<CodeSociete>
					<xsl:value-of select="EnveloppeEasylog/CodeSociete"/>
				</CodeSociete>
				<Mode>
					<xsl:value-of select="EnveloppeEasylog/Mode"/>
				</Mode>
				<Date>
					<xsl:value-of select="EnveloppeEasylog/DateYYYYMMDD"/>
				</Date>
				<Heure>
					<xsl:value-of select="EnveloppeEasylog/HourLong"/>
				</Heure>
				<DonneesSpecifiques>
					<CW1Data>
						<Header>
							<xsl:copy-of select="Header/node()"/>
						</Header>
					</CW1Data>
				</DonneesSpecifiques>
			</EnveloppeEasyLog>
			<EnveloppeCIN>
				<OACI>
					<xsl:value-of select="Body/Message/EnveloppeCIN/OACI"/>
				</OACI>
				<REFERENCE>
					<xsl:value-of select="Body/Message/EnveloppeCIN/REFERENCE"/>
				</REFERENCE>
				<MRN_ECS>
					<xsl:value-of select="Body/Message/EnveloppeCIN/MRN_ECS"/>
				</MRN_ECS>
				<xsl:if test="Body/Message/EnveloppeCIN/MAGASIN">
					<MAGASIN>
						<xsl:value-of select="Body/Message/EnveloppeCIN/MAGASIN"/>
					</MAGASIN>
				</xsl:if>
				<xsl:if test="Body/Message/EnveloppeCIN/BUR_DOUANE">
					<BUR_DOUANE>
						<xsl:value-of select="Body/Message/EnveloppeCIN/BUR_DOUANE"/>
					</BUR_DOUANE>
				</xsl:if>
				<xsl:if test="Body/Message/EnveloppeCIN/DEST_OACI">
					<DEST_OACI>
						<xsl:value-of select="Body/Message/EnveloppeCIN/DEST_OACI"/>
					</DEST_OACI>
				</xsl:if>
				<xsl:if test="Body/Message/EnveloppeCIN/NUM_LTA">
					<NUM_LTA>
						<xsl:value-of select="Body/Message/EnveloppeCIN/NUM_LTA"/>
					</NUM_LTA>
				</xsl:if>
				<EDIFACTCIN>
					<xsl:copy-of select="Body/Message/EnveloppeCIN/XML_CIN/node()"/>
				</EDIFACTCIN>
			</EnveloppeCIN>
		</Message>
	</xsl:template>
</xsl:stylesheet>

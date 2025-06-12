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
				<RefDossier>
					<xsl:value-of select="EnveloppeEasylog/DeclarationReference"/>
				</RefDossier>
				<LRN>
					<xsl:value-of select="EnveloppeEasylog/LRN"/>
				</LRN>
				<MRN>
					<xsl:value-of select="EnveloppeEasylog/MRN"/>
				</MRN>
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
			<MessageJson>
				<xsl:value-of select="MessageJson"/>
			</MessageJson>
		</Message>
	</xsl:template>
</xsl:stylesheet>

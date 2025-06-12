<?xml version="1.0" encoding="iso-8859-1"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
	<xsl:output method="xml" indent="yes"/>
	<xsl:template match="root">
		<Message>
			<EnveloppeEasyLog>
				<Application>
					<xsl:value-of select="EnveloppeEasylog/Application"/>
				</Application>
				<CodeClient>
					<xsl:value-of select="EnveloppeEasylog/CodeClient"/>
				</CodeClient>
				<CodeSite>
					<xsl:value-of select="EnveloppeEasylog/SenderId"/>
				</CodeSite>
				<CodeSociete>
					<xsl:value-of select="EnveloppeEasylog/CodeSociete"/>
				</CodeSociete>
				<TypeMessage>
					<xsl:value-of select="EnveloppeEasylog/MessageType"/>
				</TypeMessage>
				<Mode>
					<xsl:value-of select="EnveloppeEasylog/Mode"/>
				</Mode>
				<TransactionId>
					<xsl:value-of select="EnveloppeEasylog/TransactionId"/>
				</TransactionId>
				<Flux>
					<xsl:value-of select="EnveloppeEasylog/Flux"/>
				</Flux>
				<IdDossier>
					<xsl:value-of select="Body/Interchanges/@id"/>
				</IdDossier>
				<RefDossier>
					<xsl:value-of select="EnveloppeEasylog/DeclarationReference"/>
				</RefDossier>
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
			<Enveloppe_APPLUS>
				<AP_destinataire_EDI>
					<xsl:value-of select="Body/Interchanges/MessageSet/Destinataire/@user"/>
				</AP_destinataire_EDI>
				<AP_emetteur_user>
					<xsl:value-of select="Body/Interchanges/MessageSet/Emetteur/@user"/>
				</AP_emetteur_user>
				<AP_emetteur_tiersprof>
					<xsl:value-of select="Body/Interchanges/MessageSet/Emetteur/@tiersProf"/>
				</AP_emetteur_tiersprof>
				<AP_destinataire_user>
					<xsl:value-of select="Body/Interchanges/MessageSet/Destinataire/@user"/>
				</AP_destinataire_user>
				<AP_destinataire_tiersprof>
					<xsl:value-of select="Body/Interchanges/MessageSet/Destinataire/@user"/>
				</AP_destinataire_tiersprof>
				<TYPE>
					<xsl:value-of select="Body/Interchanges/MessageSet/Messages/Request/@type"/>
				</TYPE>
				<NUM_RCA>
					<xsl:value-of select="Body/Interchanges/MessageSet/Messages/Request/document-accompagnement/reference-doc/@rca"/>
				</NUM_RCA>
				<BUREAU>
					<xsl:value-of select="Body/Interchanges/MessageSet/Messages/Request/document-accompagnement/lieux-doc/@bdd"/>
				</BUREAU>
			</Enveloppe_APPLUS>
			<Message_APPLUS>
				<xsl:copy-of select="Body/node()"/>
			</Message_APPLUS>
		</Message>
	</xsl:template>
</xsl:stylesheet>

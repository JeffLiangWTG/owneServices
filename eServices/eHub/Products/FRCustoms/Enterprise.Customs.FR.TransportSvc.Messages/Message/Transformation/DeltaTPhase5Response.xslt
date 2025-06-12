<?xml version="1.0" encoding="iso-8859-1"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
	<xsl:output method="xml" indent="yes"/>
	<xsl:template match="/">
		<ns0:GenericMessageInterchange xmlns:ns0="http://cargowise.com/ehub/core/genericmessagedelivery">
			<Header>
				<SenderID>
					<xsl:value-of select="Message/EnveloppeEasyLog/DonneesSpecifiques/CW1Data/Header/RecipientID"/>
				</SenderID>
				<RecipientID>
					<xsl:value-of select="Message/EnveloppeEasyLog/DonneesSpecifiques/CW1Data/Header/SenderID"/>
				</RecipientID>
				<InterchangeType>FR5</InterchangeType>
				<InterchangeNumber>
					<xsl:value-of select="Message/EnveloppeEasyLog/DonneesSpecifiques/CW1Data/Header/InterchangeNumber"/>
				</InterchangeNumber>
			</Header>
			<Body>
				<Message>
					<EnveloppeMessage>
						<xsl:copy-of select="Message/EnveloppeMessage/node()"/>
					</EnveloppeMessage>
					<MessageBody>
						<xsl:copy-of select="Message/MessageBody/node()"/>
					</MessageBody>
				</Message>
			</Body>
		</ns0:GenericMessageInterchange>
	</xsl:template>
</xsl:stylesheet>

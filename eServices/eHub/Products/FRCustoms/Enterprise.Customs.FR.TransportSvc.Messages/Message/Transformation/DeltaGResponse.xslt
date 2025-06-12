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
				<InterchangeType>FRC</InterchangeType>
				<xsl:choose>
					<xsl:when test="Message/ReponseDeclaration/ReponseDatas/Notification/Etat/etat">
						<InterchangeNumber>
							<xsl:value-of select="Message/EnveloppeEasyLog/DonneesSpecifiques/CW1Data/Header/InterchangeNumber"/>.<xsl:value-of select="Message/ReponseDeclaration/ReponseDatas/Notification/Etat/etat"/>.<xsl:value-of select="Message/InterchangeData/InterchangeTime"/>
						</InterchangeNumber>
					</xsl:when>
					<xsl:otherwise>
						<xsl:choose>
							<xsl:when test="Message/ReponseDeclaration/ReponseDatas/Notification/Etat/etatECS">
								<InterchangeNumber>
									<xsl:value-of select="Message/EnveloppeEasyLog/DonneesSpecifiques/CW1Data/Header/InterchangeNumber"/>.<xsl:value-of select="Message/ReponseDeclaration/ReponseDatas/Notification/Etat/etatECS"/>.<xsl:value-of select="Message/InterchangeData/InterchangeTime"/>
								</InterchangeNumber>
							</xsl:when>
							<xsl:otherwise>
								<xsl:if test="Message/ReponseDeclaration/ReponseDatas/Erreur">
									<InterchangeNumber>
										<xsl:value-of select="Message/EnveloppeEasyLog/DonneesSpecifiques/CW1Data/Header/InterchangeNumber"/>.ERR.<xsl:value-of select="Message/InterchangeData/InterchangeTime"/>
									</InterchangeNumber>
								</xsl:if>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:otherwise>
				</xsl:choose>
			</Header>
			<Body>
				<Message>
					<EnveloppeMessage>
						<xsl:copy-of select="Message/EnveloppeMessage/node()"/>
					</EnveloppeMessage>
					<ReponseDeclaration>
						<xsl:copy-of select="Message/ReponseDeclaration/node()"/>
					</ReponseDeclaration>
				</Message>
			</Body>
		</ns0:GenericMessageInterchange>
	</xsl:template>
</xsl:stylesheet>

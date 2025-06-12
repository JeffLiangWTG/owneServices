<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:output method="xml" indent="yes"/>
  <xsl:template match="root">
    <Message>
      <EnveloppeEasyLog>
        <Application><xsl:value-of select="EnveloppeEasylog/Application"/></Application>
        <CodeSite><xsl:value-of select="EnveloppeEasylog/SenderId"/></CodeSite>
        <TypeMessage><xsl:value-of select="EnveloppeEasylog/MessageType"/></TypeMessage>
        <IdDossier><xsl:value-of select="EnveloppeEasylog/IdDossier"/></IdDossier>
        <TransactionId><xsl:value-of select="EnveloppeEasylog/TransactionId"/></TransactionId>
        <PartyId><xsl:value-of select="EnveloppeEasylog/PartyId"/></PartyId>
        <RefDossier><xsl:value-of select="EnveloppeEasylog/DeclarationReference"/></RefDossier>
		<RefDELTA><xsl:value-of select="EnveloppeEasylog/DeltaReference"/></RefDELTA>
		<LRN><xsl:value-of select="EnveloppeEasylog/LRN"/></LRN>
        <Flux><xsl:value-of select="EnveloppeEasylog/Flux"/></Flux>
        <CodeClient><xsl:value-of select="EnveloppeEasylog/CodeClient"/></CodeClient>
        <CodeSociete><xsl:value-of select="EnveloppeEasylog/CodeSociete"/></CodeSociete>
        <Mode><xsl:value-of select="EnveloppeEasylog/Mode"/></Mode>
        <Date><xsl:value-of select="EnveloppeEasylog/DateYYYYMMDD"/></Date>
        <Heure><xsl:value-of select="EnveloppeEasylog/HourLong"/></Heure>
		<MessageId><xsl:value-of select="EnveloppeEasylog/MessageId"/></MessageId>
		  <DonneesSpecifiques>
          <CW1Data>
            <Header>
              <xsl:copy-of select="Header/node()"/>
            </Header>
          </CW1Data>
        </DonneesSpecifiques>
      </EnveloppeEasyLog>
		<Declaration>
        <xsl:copy-of select="Body/node()"/>
      </Declaration>
    </Message>
  </xsl:template>
</xsl:stylesheet>

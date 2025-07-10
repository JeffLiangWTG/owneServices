<?xml version="1.0"?>

<!--
Stylesheet to take Build.xml and output web solution BIN paths separated by newlines.
Called by QGL to remove DLLs from local BIN directory to prevent DEBUG DLLs being deployed to clients
-->

<xsl:stylesheet version = "1.0" xmlns:build="http://www.edi.com.au/build.xsd" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

	<xsl:output method="text" encoding="US-ASCII" />

	<xsl:template match="text()|@*"/>

	<xsl:template match="//build:WebSolution">
			<xsl:value-of select="@Path" />
			<xsl:text>\bin&#13;&#10;</xsl:text>
	</xsl:template>
	
	
</xsl:stylesheet>

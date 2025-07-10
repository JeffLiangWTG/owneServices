<?xml version="1.0"?>
<xsl:stylesheet version = "1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<xsl:template match="/">
	<html>
	<head>
	<style type="text/css">
		p {
			margin-top: 0em;
			margin-bottom: 0em;
		}
		
		.Call .element {
			visibility: hidden;
		}
		
		.ExceptionType, .DatabaseServerName {
			margin-top: 2em;
		}
		
		.ExceptionMessage .value {
			font-weight: bold;
		}
		
	</style>
	</head>
	<body>
	<!-- <code> -->
	<xsl:apply-templates />
	<!-- </code> -->
	</body>
	</html>
</xsl:template>

<xsl:template match="//*[count(*)=0]">
	<p>
		<xsl:attribute name="class"><xsl:value-of select="name()"/></xsl:attribute>
		<span class="element"><xsl:value-of select="name()"/>: </span><span class="value"><xsl:value-of select="."/></span>
	</p>
</xsl:template>

<xsl:template match="//DLL">
	<p>
		<xsl:attribute name="class"><xsl:value-of select="name()"/></xsl:attribute>
		<span class="element"><xsl:value-of select="name()"/>: </span><span class="value"><xsl:value-of select="@Name"/><xsl:text> </xsl:text><xsl:value-of select="@Version"/></span>
	</p>
</xsl:template>

<xsl:template match="//Calls">
</xsl:template>

</xsl:stylesheet>
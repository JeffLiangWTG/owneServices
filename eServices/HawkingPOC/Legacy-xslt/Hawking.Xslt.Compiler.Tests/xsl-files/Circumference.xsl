<xsl:stylesheet 
    version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt"
    exclude-result-prefixes="msxsl userCSharp"
    xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">

    <xsl:output
        method="xml"
        version="1.0"
        encoding="utf-8"
        omit-xml-declaration="no"
        standalone="yes"
        indent="yes"
        media-type="string"/>

    <xsl:template match="data">
        <circles>
          <xsl:for-each select="circle">
            <circle>
              <xsl:copy-of select="node()"/>
              <circumference>
                <xsl:value-of select="userCSharp:circumference(radius)"/>
              </circumference>
            </circle>
          </xsl:for-each>
        </circles>
    </xsl:template>

    <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[
        public double circumference(double radius)
        {
            double pi = 3.14;
            double circ = pi * radius * 2;
            return circ;
        }
    ]]>
    </msxsl:script >
</xsl:stylesheet>
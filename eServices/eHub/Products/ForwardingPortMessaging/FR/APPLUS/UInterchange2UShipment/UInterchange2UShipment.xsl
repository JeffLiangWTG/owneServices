<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
                exclude-result-prefixes="CodeMapper ContextAccessor userCSharp msxsl"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes" />

  <xsl:template match="/">
    <xsl:variable name="sender" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
    <xsl:variable name="recipient" select="ContextAccessor:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
    <xsl:variable name="licenceType" select="CodeMapper:CallActionProcedureHelper('GetEdiProdLicenceType', '@LicenceType', '@ClientID', $sender)" />

    <xsl:if test="$licenceType != 'PRD'">
      <xsl:variable name="setTestRecipient" select="ContextAccessor:SetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties', concat($recipient, '_TST'))"/>
    </xsl:if>

    <xsl:variable name="dataSourceKey" select="*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='DataContext']/*[local-name()='DataSource']/*[local-name()='Key']/text()"/>

    <xsl:choose>
      <xsl:when test="$dataSourceKey!=''">
        <xsl:apply-templates select="*[local-name()='UniversalInterchange']/*[local-name()='Body']/*" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="userCSharp:ThrowDataSourceKeyNotFound()"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="*">
    <xsl:element name="{local-name(.)}" namespace="{namespace-uri()}" >
      <xsl:copy-of select="@*"/>
      <xsl:apply-templates/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="node()/text()">
    <xsl:call-template name="GetElementValue">
      <xsl:with-param name="elementValue" select="." />
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="@*">
    <xsl:attribute name="{local-name()}">
      <xsl:call-template name="GetElementValue">
        <xsl:with-param name="elementValue" select="." />
      </xsl:call-template>
    </xsl:attribute>
  </xsl:template>

  <xsl:template name="GetElementValue">
    <xsl:param name="elementValue" />
    <xsl:variable name="allowedCharacters">
      ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 .,-()[]\/^_`{|}~='+:?!"%*;#$&amp;&lt;&gt;@&#10;
    </xsl:variable>
    <xsl:variable name="specialCharacters">
      &#160;&#161;&#162;&#163;&#164;&#165;&#166;&#167;&#168;&#169;
      &#170;&#171;&#172;&#173;&#174;&#175;&#176;&#177;
      &#180;&#181;&#182;&#183;&#184;&#185;&#186;&#187;&#188;&#189;
      &#190;&#191;
    </xsl:variable>
    <xsl:variable name="latinCharacters">
      &#192;&#193;&#194;&#195;&#196;&#197;&#198;&#199;
      &#200;&#201;&#202;&#203;&#204;&#205;&#206;&#207;&#208;&#209;
      &#210;&#211;&#212;&#213;&#214;&#215;&#216;&#217;&#218;&#219;
      &#220;&#221;&#222;&#223;&#224;&#225;&#226;&#227;&#228;&#229;
      &#230;&#231;&#232;&#233;&#234;&#235;&#236;&#237;&#238;&#239;
      &#240;&#241;&#242;&#243;&#244;&#245;&#246;&#247;&#248;&#249;
      &#250;&#251;&#252;&#253;&#254;&#255;
    </xsl:variable>
    <xsl:variable name="newLinesCharacter">&#10;</xsl:variable>

    <xsl:if test=". != ''">
      <xsl:variable name="value">
        <xsl:value-of select="translate(normalize-space(userCSharp:ReplaceInvalidText($elementValue)), $newLinesCharacter, ' ')"/>
      </xsl:variable>

      <xsl:variable name="newValue" select="userCSharp:CleanUpUnicode($value)" />
      <xsl:variable name="disallowedCharacters" select="translate($newValue, $allowedCharacters, '')"/>
      <xsl:choose>
        <xsl:when test="$disallowedCharacters = ''">
          <xsl:value-of select="$newValue"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:variable name="disallowedCharacters2" select="translate($disallowedCharacters, $specialCharacters, '')"/>
          <xsl:choose>
            <xsl:when test="$disallowedCharacters2 = ''">
              <xsl:value-of select="translate($newValue, $disallowedCharacters, '')"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:variable name="disallowedCharacters3" select="translate($disallowedCharacters2, $latinCharacters, '')"/>
              <xsl:choose>
                <xsl:when test="$disallowedCharacters3 = ''">
                  <xsl:variable name="disallowedChartacterList" select="concat($disallowedCharacters,$disallowedCharacters2)" />
                  <xsl:value-of select="translate($newValue, $disallowedChartacterList, '')"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:variable name="disallowedChartacterList" select="concat($disallowedCharacters,$disallowedCharacters2,$disallowedCharacters3)" />
                  <xsl:value-of select="translate($newValue, $disallowedChartacterList, '')"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>
  </xsl:template>

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[
public string ReplaceText(string inputText, string oldValue, string newValue)
{
  var result = "";
  if (!string.IsNullOrEmpty(inputText))
  {
    result = inputText.Replace(oldValue, newValue);
  }
  return result;
}

public string ReplaceInvalidText(string inputText)
{
  var result = inputText;
  var ch2enMap = new System.Collections.Generic.Dictionary<string, string>()
  {
    { "（", "(" },
    { "）", ")" },
    { "【", "[" },
    { "】", "]" },
    { "：", ":" },
    { "；", ";" },
    { "。", "." },
    { "，", ","},
    { "？", "?"},
    { "-", "-"},
    { "“", "\""},
    { "‘", "'"},
    { "”", "\""},
    { "’", "'"},
  };

  foreach (var ch2en in ch2enMap)
  {
    result = ReplaceText(result, ch2en.Key, ch2en.Value);
  }

  return result;
}

public void ThrowDataSourceKeyNotFound()
{
  throw new ArgumentException("Could not find data source key, message rejected.");
}

public string CleanUpUnicode(string inputData)
{
  return inputData.Replace((char)0x09, ' ')
                  .Replace((char)0x00A0, ' ')               //No Break Space
                  .Replace((char)0x2012, '-')               //Figure Dash
                  .Replace((char)0x2013, '-')               //En Dash
                  .Replace((char)0x2014, '-')               //Em Dash
                  .Replace((char)0x2E3A, '-')               //Two-Em Dash
                  .Replace((char)0x2E3B, '-')               //Three-Em Dash
                  .Replace(((char)0xB0).ToString(), "")     //Degree Celsius
                  .Replace(((char)0xBA).ToString(), "")     //Latin 1 MASCULINE ORDINAL INDICATOR
                  .Replace(((char)0x00B4).ToString(), "'"); //Latin1 '
}
]]>
  </msxsl:script>
</xsl:stylesheet>
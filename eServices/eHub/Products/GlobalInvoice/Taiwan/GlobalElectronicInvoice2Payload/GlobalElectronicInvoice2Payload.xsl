<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var"
                exclude-result-prefixes="msxsl var s0 ns0 ContextAccessor DataModelAccessor userCSharp"
                version="1.0"
                xmlns:s0="http://cargowise.com/ehub/products/Accounting/GlobalElectronicInvoicing"
                xmlns:ns0="http://cargowise.com/ehub/products/GlobalInvoice/Taiwan"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
                xmlns:DataModelAccessor="http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />
  <xsl:template match="/">
    <xsl:apply-templates select="/s0:GlobalElectronicInvoicing" />
  </xsl:template>
  <xsl:template match="/s0:GlobalElectronicInvoicing">
	  <xsl:variable name="fileName" select="s0:Header/s0:ElectronicInvoiceBatchRequest/s0:FileName" />
	  <xsl:variable name ="RecipientID" select="ContextAccessor:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
	  <xsl:variable name ="SenderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
	 
    <xsl:if test="$fileName = '' or not($fileName)">
      <xsl:value-of select="userCSharp:ThrowException('GlobalElectronicInvoicing/Header/ElectronicInvoiceBatchRequest/FileName')"/>
    </xsl:if>
    <xsl:variable name="batchNumber" select="s0:Header/s0:ElectronicInvoiceBatchRequest/s0:BatchNumber" />
    <xsl:if test="$batchNumber = '' or not($batchNumber)">
      <xsl:value-of select="userCSharp:ThrowException('GlobalElectronicInvoicing/Header/ElectronicInvoiceBatchRequest/BatchNumber')"/>
    </xsl:if>
	  <xsl:variable name="fileNameWithNoExtension" select="userCSharp:GetFileNameWithoutExtension($fileName)"/>
		<xsl:variable name ="subscribeFilenameAndBatchNumber" select="DataModelAccessor:InsertSubscriptionValue('GEIMSG', $RecipientID, $SenderID, $fileNameWithNoExtension, $batchNumber)" />
		<xsl:variable name="payload" select="s0:Payload" />
    <xsl:if test="$payload = '' or not($payload)">
      <xsl:value-of select="userCSharp:ThrowException('GlobalElectronicInvoicing/Payload')"/>
    </xsl:if>
    <xsl:variable name="FileName" select="ContextAccessor:SetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06', $fileName/text())"/>
    <ns0:Payload>
      <Content>
        <xsl:value-of select="s0:Payload/text()" />
      </Content>
    </ns0:Payload>
  </xsl:template>
  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[
      public void ThrowException(string param) {
        throw new Exception(string.Format("{0} is mandatory but is missing.", param));
      }
			
	public string GetFileNameWithoutExtension(string fileName)
	{
			int extensionIndex = fileName.LastIndexOf('.');
			string nameWithoutExtension = (extensionIndex >= 0) ? fileName.Substring(0, extensionIndex) : fileName;
			return nameWithoutExtension;
	}

    ]]>
  </msxsl:script>
</xsl:stylesheet>

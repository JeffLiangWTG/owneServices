<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var"
                exclude-result-prefixes="msxsl var ns0 ScriptNS0 ScriptNS1 ScriptNS2 userCSharp"
                version="1.0"
                xmlns:ns1="http://www.cargowise.com/Schemas/Universal/2012/11"
                xmlns:ns0="http://cargowise.com/ehub/products/Accounting/GlobalElectronicInvoicing"
                xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
                xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1"
                xmlns:ScriptNS2="http://schemas.microsoft.com/BizTalk/2003/ScriptNS2"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
	<xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />
	<xsl:template match="/">
		<xsl:apply-templates select="/ns0:GEITaiwanInboundEnvelope" />
	</xsl:template>
	<xsl:template match="/ns0:GEITaiwanInboundEnvelope">
		<xsl:variable name="SenderID" select="ScriptNS1:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
		<xsl:variable name="FileName" select="ScriptNS1:GetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06')" />
		<xsl:variable name="fileNameWithNoTimestamp" select="userCSharp:RemoveAdditionalDateTime($FileName)"/>
		<xsl:variable name="RecipientId" select="ScriptNS2:CallActionProcedureHelper('SelectSubscribedClients', '', '@senderId', $SenderID, '@ST_ID', 'GEIMSG', '@value', $fileNameWithNoTimestamp)" />
		<xsl:if test="$RecipientId=''">
			<xsl:variable name="CheckRecipientId" select="userCSharp:ThrowPartyRecepientIdNotFound($SenderID, $fileNameWithNoTimestamp)" />
		</xsl:if>
		<xsl:variable name="SetDestinationParty" select="ScriptNS1:SetContextProperty('DestinationParty','http://schemas.microsoft.com/BizTalk/2003/system-properties', $RecipientId)"/>
		<xsl:variable name="BatchNumber" select="ScriptNS2:CallActionProcedureHelper('SelectSubscribedReference','@reference', '@senderId', $SenderID, '@recipientId', $RecipientId, '@ST_ID', 'GEIMSG','@value', $fileNameWithNoTimestamp)" />

		<ns1:UniversalInterchangeInclude>
			<ns1:Body>
				<ns1:UniversalEvent>
					<ns1:Event>
						<ns1:DataContext>
							<ns1:DataTargetCollection>
								<ns1:DataTarget>
									<ns1:Type>AccEInvoicingBatch</ns1:Type>
									<ns1:Key>
										<xsl:value-of select ="$BatchNumber"/>
									</ns1:Key>
								</ns1:DataTarget>
							</ns1:DataTargetCollection>
						</ns1:DataContext>
						<ns1:EventTime>
							<xsl:value-of select="ScriptNS0:CurrentDateTime('s')" />
						</ns1:EventTime>
						<ns1:EventType>IAK</ns1:EventType>
						<ns1:EventParameters>
							<ns1:MessageType>TW</ns1:MessageType>
						</ns1:EventParameters>
						<ns1:ContextCollection>
							<ns1:Context>
								<ns1:Type>CompanyCode</ns1:Type>
								<ns1:Value>
									<xsl:value-of select="substring($RecipientId, 4 ,3)"/>
								</ns1:Value>
							</ns1:Context>
							<ns1:Context>
								<ns1:Type>ResponseFileResult</ns1:Type>
								<ns1:Value>
									<xsl:variable name="body" select="//*[local-name()='body']/text()" />
									<xsl:value-of select="userCSharp:Base64Encoding($body)"/>
								</ns1:Value>
							</ns1:Context>
						</ns1:ContextCollection>
					</ns1:Event>
				</ns1:UniversalEvent>
			</ns1:Body>
		</ns1:UniversalInterchangeInclude>
	</xsl:template>
	<msxsl:script language="C#" implements-prefix="userCSharp">
		<![CDATA[
    public string Base64Encoding(string input)
    {
      return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(input));
    }
		
	public string RemoveAdditionalDateTime(string input)
	{
			try
			{
					string[] parts = input.Split('/');
					string fileName = parts[parts.Length - 1];
					string[] nameParts = fileName.Split('-');

					if (nameParts.Length <= 2)
					{
							return fileName;
					}

					int numPartsToKeep = nameParts.Length - 2;
					string[] partsToKeep = new string[numPartsToKeep];
					Array.Copy(nameParts, partsToKeep, numPartsToKeep);
					string output = string.Join("-", partsToKeep);
					return output;
			}
			catch (Exception ex)
			{
					throw new ArgumentException("Could not remove additional date and time from file name.", ex);
			}
	}

	public string ThrowPartyRecepientIdNotFound(string senderID, string fileName)
	{
		throw new ArgumentException(string.Format(@"Could not found matching recepient.(SenderID:{0} File Name: [{1}])", senderID, fileName));
	}
			
]]>
	</msxsl:script>
</xsl:stylesheet>

<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 ns0 ScriptNS0 ScriptNS1 ScriptNS2 userCSharp" version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2012/11"
                xmlns:ns0="http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"
                xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
                xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1"
                xmlns:ScriptNS2="http://schemas.microsoft.com/BizTalk/2003/ScriptNS2"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="no" method="xml" version="1.0" indent="yes" />

  <xsl:variable name="RecipientId" select="ScriptNS2:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="SenderId" select="ScriptNS2:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="StatusString" select="ScriptNS2:GetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06')"/>

  <xsl:template match="ns0:EFACT_31_AIRAED">
    <xsl:variable name="jobNumber" select="UNH/UNH1"/>

    <UniversalEvent xmlns="http://www.cargowise.com/Schemas/Universal/2012/11" version="2.0">
      <Event>
        <DataContext>
          <Workflow>
            <xsl:choose>
              <xsl:when test="$StatusString = 'ACKSuccess'">
                <ActionPurpose Description="SG ACCESS ACKNOWLEDGEMENT">ACK</ActionPurpose>
              </xsl:when>
              <xsl:otherwise>
                <ActionPurpose Description="AIRERR SG ACCESS">ERR</ActionPurpose>
              </xsl:otherwise>
            </xsl:choose>
          </Workflow>
          <DataSource>
            <DataProvider>SGA</DataProvider>
          </DataSource>
          <DataTargetCollection>
            <DataTarget>
              <Key>
                <xsl:value-of select="$jobNumber"/>
              </Key>
              <Type>AsycudaManifest</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>
          <xsl:value-of select="ScriptNS1:CurrentDateTime('s')" />
        </EventTime>
        <EventType>
          <xsl:choose>
            <xsl:when test="$StatusString = 'ACKSuccess'">MDL</xsl:when>
            <xsl:otherwise>MRJ</xsl:otherwise>
          </xsl:choose>
        </EventType>
        <EventReference>MST=MGE</EventReference>
        <IsEstimate>false</IsEstimate>
        <ContextCollection>
          <Context>
            <Type>ManifestCountry</Type>
            <Value>SG</Value>
          </Context>
          <Context>
            <Type>MessageType</Type>
            <Value>AIRAED</Value>
          </Context>
          <Context>
            <Type>UENNumber</Type>
            <Value>
              <xsl:value-of select="ns0:IDT/ns0:IDT1/IDT1.1"/>
            </Value>
          </Context>
          <Context>
            <Type>OriginalCreateDate</Type>
            <Value>
              <xsl:value-of select="ns0:IDT/ns0:IDT1/IDT1.2"/>
            </Value>
          </Context>
          <Context>
            <Type>OriginalSerialNumber</Type>
            <Value>
              <xsl:value-of select="ns0:IDT/ns0:IDT1/IDT1.3"/>
            </Value>
          </Context>
          <Context>
            <Type>MasterBill</Type>
            <Value>
              <xsl:value-of select="ns0:SG1Loop/ns0:SG2Loop/ns0:REF/REF1"/>
            </Value>
            <SubContextCollection>
              <xsl:if test="not($StatusString = '')">
                <xsl:variable name="fileName" select="ScriptNS2:SetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06', '')"/>
              </xsl:if>
              <xsl:variable name="ErrorCode" select="userCSharp:GetErrorCode($StatusString)"/>
              <xsl:variable name="ErrorMessage" select="userCSharp:GetErrorDescription($StatusString)"/>
              <xsl:for-each select="ns0:SG1Loop">
                <SubContext>
                  <Type>HouseBill</Type>
                  <Value>
                    <xsl:value-of select="ns0:RFF"/>
                  </Value>
                  <SubContextCollection>
                    <xsl:variable name="Key" select="concat(ns0:SG2Loop/ns0:REF/REF1,ns0:RFF/RFF1,$jobNumber,ns0:CST/CST1,'E')"/>
                    <xsl:variable name="ConsignmentReferences" select="ScriptNS0:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $SenderId, '@recipientId', $RecipientId, '@ST_ID', 'SGCMSG', '@value', $Key, '@referenceType', 'CNRF')" />
                    <xsl:variable name="SG1" select="."/>
                    <xsl:if test="$ConsignmentReferences=''">
                      <xsl:value-of select="userCSharp:ThrowSubscriptionValueNotFound($Key)"/>
                    </xsl:if>
                    <xsl:for-each select="userCSharp:Split($ConsignmentReferences)">
                      <SubContext>
                        <Type>ConsignmentReference</Type>
                        <Value>
                          <xsl:value-of select="substring(./text(), 6)"/>
                        </Value>
                        <SubContextCollection>
                          <xsl:choose>
                            <xsl:when test="$StatusString = 'ACKSuccess'">
                              <SubContext>
                                <Type>MessageStatusCode</Type>
                                <Value>ACK</Value>
                              </SubContext>
                            </xsl:when>
                            <xsl:otherwise>
                              <SubContext>
                                <Type>ConsignmentNumber</Type>
                                <Value>
                                  <xsl:value-of select="substring(./text(), 1, 5)"/>
                                </Value>
                              </SubContext>
                              <SubContext>
                                <Type>ErrorCode</Type>
                                <Value>
                                  <xsl:value-of select="$ErrorCode"/>
                                </Value>
                              </SubContext>
                              <SubContext>
                                <Type>ErrorDescription</Type>
                                <Value>
                                  <xsl:value-of select="$ErrorMessage"/>
                                </Value>
                              </SubContext>
                            </xsl:otherwise>
                          </xsl:choose>
                        </SubContextCollection>
                      </SubContext>
                    </xsl:for-each>
                    <xsl:if test="$StatusString = 'ACKSuccess'">
                      <SubContext>
                        <Type>MessageStatusCode</Type>
                        <Value>ACK</Value>
                      </SubContext>
                    </xsl:if>
                  </SubContextCollection>
                </SubContext>
              </xsl:for-each>
              <xsl:if test="$StatusString = 'ACKSuccess'">
                <SubContext>
                  <Type>MessageStatusCode</Type>
                  <Value>ACK</Value>
                </SubContext>
              </xsl:if>
            </SubContextCollection>
          </Context>
        </ContextCollection>
      </Event>
    </UniversalEvent>
  </xsl:template>

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[
      public string GetErrorCode(string errorString)
      {
        var pair = errorString.Split(';')[0].Split(':');
        if (pair.Length != 2) return string.Empty;
        return pair[1].Trim();
      }
    
      public string GetErrorDescription(string errorString)
      {
        var index = errorString.IndexOf(';');
        if (index <= 0 || index >= errorString.Length - 1) return string.Empty;
        return errorString.Substring(index + 1).Trim();
      }

      public XPathNodeIterator Split(string to_split)
      {
        var doc = new XmlDocument();
        var root = doc.CreateElement("root");
        doc.AppendChild(root);

        if (to_split != "") {
          foreach (var value in to_split.Split('|'))
          {
            var child = doc.CreateElement("child");
            child.InnerText = value;
            root.AppendChild(child);
          }
        }

        return doc.CreateNavigator().Select("/*/*");
      }
      
    public string ThrowSubscriptionValueNotFound(string subscriptionValue)
    {
      throw new ArgumentException(string.Format(@"Cannot find matching subscription for '{0}'.", subscriptionValue));
    }
    ]]>
  </msxsl:script>
</xsl:stylesheet>

<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                exclude-result-prefixes="ScriptNS1"
                version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />
  <xsl:template match="/">
    <xsl:apply-templates select="/s0:SuretyToBrokerMessage" />
  </xsl:template>

  <xsl:template match="s0:SuretyToBrokerMessage">
    <s0:UniversalInterchange>
      <s0:Body>
        <s0:UniversalEvent>
          <s0:Event>
            <xsl:variable name="TransactionIDTypeCode" select="s0:MessageLevelResult/s0:TransactionIDTypeCode"/>
            <s0:DataContext>
              <s0:DataTargetCollection>
                <s0:DataTarget>
                  <s0:Type>
                    <xsl:choose>
                      <xsl:when test="$TransactionIDTypeCode='1'">CustomsDeclaration</xsl:when>
                      <xsl:when test="$TransactionIDTypeCode='2'">USImporterSecurityFiling</xsl:when>
                    </xsl:choose>
                  </s0:Type>
                  <s0:Key>
                    <xsl:value-of select="s0:MessageLevelResult/s0:BrokerReferenceNumber" />
                  </s0:Key>
                </s0:DataTarget>
              </s0:DataTargetCollection>
            </s0:DataContext>
            <s0:EventTime>
              <xsl:value-of select="ScriptNS1:CurrentDateTime('s')" />
            </s0:EventTime>
            <s0:EventType>STC</s0:EventType>
            <xsl:variable name="DispositionCode" select="s0:MessageLevelResult/s0:DispositionCode"/>
            <xsl:variable name="SuretyResponseCode" select="s0:MessageLevelResult/s0:SuretyResponseCode"/>
            <s0:EventReference>
              <xsl:value-of select="concat($DispositionCode, '/', $SuretyResponseCode)" />
            </s0:EventReference>
            <s0:ContextCollection>
              <xsl:if test="$TransactionIDTypeCode='1'">
                <s0:Context>
                  <s0:Type>EntryNumber</s0:Type>
                  <s0:Value>
                    <xsl:value-of select="s0:MessageLevelResult/s0:TransactionID" />
                  </s0:Value>
                </s0:Context>
              </xsl:if>
              <s0:Context>
                <s0:Type>BondDesignationCode</s0:Type>
                <s0:Value>
                  <xsl:value-of select="s0:MessageLevelResult/s0:BondDesignationCode" />
                </s0:Value>
              </s0:Context>
              <s0:Context>
                <s0:Type>SuretyCode</s0:Type>
                <s0:Value>
                  <xsl:value-of select="s0:MessageLevelResult/s0:SuretyCode" />
                </s0:Value>
              </s0:Context>
              <s0:Context>
                <s0:Type>SuretyContactName</s0:Type>
                <s0:Value>
                  <xsl:value-of select="s0:MessageLevelResult/s0:SuretyContactName" />
                </s0:Value>
              </s0:Context>
              <s0:Context>
                <s0:Type>SuretyContactEmail</s0:Type>
                <s0:Value>
                  <xsl:value-of select="s0:MessageLevelResult/s0:SuretyContactEmail" />
                </s0:Value>
              </s0:Context>
              <s0:Context>
                <s0:Type>SuretyContactPhone</s0:Type>
                <s0:Value>
                  <xsl:value-of select="s0:MessageLevelResult/s0:SuretyContactPhone" />
                </s0:Value>
              </s0:Context>
              <xsl:for-each select="./s0:LineLevelResults">
                <s0:Context>
                  <s0:Type>LineLevelResults</s0:Type>
                  <xsl:variable name="LineNo" select="s0:LineNo"/>
                  <xsl:variable name="HTSNumber" select="s0:HTSNumber"/>
                  <s0:Value>
                    <xsl:value-of select="concat($LineNo, '/', $HTSNumber)" />
                  </s0:Value>
                  <s0:SubContextCollection>
                      <s0:SubContext>
                        <s0:Type>ReasonCode</s0:Type>
                        <s0:Value>
                          <xsl:value-of select="s0:LineLevelReasonCodes/s0:ReasonCode" />
                        </s0:Value>
                      </s0:SubContext>
                  </s0:SubContextCollection>
                </s0:Context>
              </xsl:for-each>
            </s0:ContextCollection>
          </s0:Event>
        </s0:UniversalEvent>
      </s0:Body>
    </s0:UniversalInterchange>
  </xsl:template>
</xsl:stylesheet>
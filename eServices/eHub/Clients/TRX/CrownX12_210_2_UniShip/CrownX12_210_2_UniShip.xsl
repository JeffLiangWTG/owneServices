<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var"
                exclude-result-prefixes="msxsl var s0 ScriptNS0 ScriptNS1 ScriptNS2" version="1.0"
                xmlns:s0="http://schemas.microsoft.com/BizTalk/EDI/X12/2006"
                xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
                xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1"
                xmlns:ScriptNS2="http://schemas.microsoft.com/BizTalk/2003/ScriptNS2">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />
  <xsl:template match="/">
    <xsl:apply-templates select="/*[local-name()='X12_00401_210']" />
  </xsl:template>

  <xsl:template match="/*[local-name()='X12_00401_210']">
    <ns0:UniversalShipment>
      <ns0:Shipment>
        <ns0:DataContext>

          <ns0:DataProvider>
            <xsl:value-of select="ScriptNS1:GetRecipientCodeUnkeyed('TRXELPELP_C04' , 'TRXELPELP' , 'Crown Data 210 - Receive Shipment Costs' , 'Defaults' , 'Data Provider')"/>
          </ns0:DataProvider>

          <xsl:variable name="RecipientID" select="ScriptNS2:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
          <ns0:Company>
            <ns0:Code>
              <xsl:value-of select="substring($RecipientID, 4, 3)"/>
            </ns0:Code>
          </ns0:Company>
          <ns0:EnterpriseID>
            <xsl:value-of select="substring($RecipientID, 1, 3)"/>
          </ns0:EnterpriseID>
          <ns0:ServerID>
            <xsl:value-of select="substring($RecipientID, 7, 3)"/>
          </ns0:ServerID>

          <ns0:DataTargetCollection>
            <ns0:DataTarget>
              <ns0:Type>ForwardingShipment</ns0:Type>

              <xsl:call-template name="MapValueIfNotEmpty">
                <xsl:with-param name="NodeName" select="'ns0:Key'" />
                <xsl:with-param name="Value" select="*[local-name()='B3']/*[local-name()='B302']" />
              </xsl:call-template>
            </ns0:DataTarget>
          </ns0:DataTargetCollection>
        </ns0:DataContext>

        <xsl:variable name="ChargeLines" select="*[local-name()='LXLoop1']/ *[local-name()='L1' and *[local-name()='L108'] != '']"/>
        <xsl:if test="$ChargeLines != ''">

          <xsl:variable name="Instruction" select="ScriptNS1:GetRecipientCodeUnkeyed('TRXELPELP_C04' , 'TRXELPELP' , 'Crown Data 210 - Receive Shipment Costs' , 'Defaults' , 'Import Instruction')"/>
          <xsl:variable name="Currency" select="*[local-name()='C3']/*[local-name()='C301']"/>
          <xsl:variable name="Creditor" select="*[local-name()='B3']/*[local-name()='B311']"/>

          <ns0:JobCosting>
            <ns0:ChargeLineCollection>
              <xsl:for-each select="$ChargeLines">
                <ns0:ChargeLine>

                  <ns0:ChargeCode>
                    <ns0:Code>
                      <xsl:value-of select="*[local-name()='L108']"/>
                    </ns0:Code>

                    <xsl:call-template name="MapValueIfNotEmpty">
                      <xsl:with-param name="NodeName" select="'ns0:Description'" />
                      <xsl:with-param name="Value" select="*[local-name()='L112']" />
                    </xsl:call-template>
                  </ns0:ChargeCode>

                  <xsl:variable name="Amount">
                    <xsl:choose>
                      <xsl:when test="*[local-name()='L102'] >= 0">
                        <xsl:value-of select="*[local-name()='L102']"/>
                      </xsl:when>
                      <xsl:when test="*[local-name()='L104'] >= 0">
                        <xsl:value-of select="*[local-name()='L104'] div 100"/>
                      </xsl:when>
                    </xsl:choose>
                  </xsl:variable>

                  <xsl:call-template name="MapValueIfNotEmpty">
                    <xsl:with-param name="NodeName" select="'ns0:CostOSAmount'" />
                    <xsl:with-param name="Value" select="$Amount" />
                  </xsl:call-template>

                  <xsl:call-template name="MapValueIfNotEmpty">
                    <xsl:with-param name="NodeName" select="'ns0:CostOSCurrency/ns0:Code'" />
                    <xsl:with-param name="Value" select="$Currency" />
                  </xsl:call-template>

                  <xsl:if test="$Creditor != ''">
                    <ns0:Creditor>
                      <ns0:Type>Organization</ns0:Type>
                      <ns0:Key>
                        <xsl:value-of select="$Creditor"/>
                      </ns0:Key>
                    </ns0:Creditor>
                  </xsl:if>

                  <ns0:ImportMetaData>
                    <ns0:Instruction>
                      <xsl:value-of select="$Instruction"/>
                    </ns0:Instruction>

                    <ns0:MatchingCriteriaCollection>

                      <xsl:call-template name="MapMatchingCriteria">
                        <xsl:with-param name="FieldName" select="'ChargeCode'" />
                        <xsl:with-param name="Value" select="./*[local-name()='L108']" />
                      </xsl:call-template>

                    </ns0:MatchingCriteriaCollection>
                  </ns0:ImportMetaData>

                </ns0:ChargeLine>
              </xsl:for-each>
            </ns0:ChargeLineCollection>
          </ns0:JobCosting>
        </xsl:if>

      </ns0:Shipment>
    </ns0:UniversalShipment>
  </xsl:template>

  <xsl:template name="MapMatchingCriteria">
    <xsl:param name="FieldName" />
    <xsl:param name="Value" />

    <xsl:if test="$Value != ''">
      <xsl:element name="ns0:MatchingCriteria">

        <xsl:element name="ns0:FieldName">
          <xsl:value-of select="$FieldName"/>
        </xsl:element>

        <xsl:element name="ns0:Value">
          <xsl:value-of select="$Value"/>
        </xsl:element>

      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="MapValueIfNotEmpty">
    <xsl:param name="NodeName" />
    <xsl:param name="Value" />

    <xsl:if test="$Value != '' and string-length($Value) != 0">
      <xsl:choose>

        <xsl:when test="contains($NodeName, '/')">
          <xsl:element name="{substring-before($NodeName, '/')}">
            <xsl:call-template name="MapValueIfNotEmpty">
              <xsl:with-param name="NodeName" select="substring-after($NodeName, '/')" />
              <xsl:with-param name="Value" select="$Value" />
            </xsl:call-template>
          </xsl:element>
        </xsl:when>

        <xsl:otherwise>
          <xsl:element name="{$NodeName}">
            <xsl:value-of select="$Value" />
          </xsl:element>
        </xsl:otherwise>

      </xsl:choose>
    </xsl:if>
  </xsl:template>

</xsl:stylesheet>

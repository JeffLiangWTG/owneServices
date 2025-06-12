<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var"
                exclude-result-prefixes="msxsl var s0 ScriptNS0 ScriptNS1" version="1.0"
                xmlns:ns0="http://schemas.microsoft.com/BizTalk/EDI/X12/2006"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
                xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />

  <xsl:variable name="Upper" select="'ABCDEFGHIJKLMNOPQRSTUVWXYZ'" />
  <xsl:variable name="Lower" select="'abcdefghijklmnopqrstuvwxyz'" />
  <xsl:variable name="Consol" select="//*[local-name()='Shipment'][contains(./*[local-name()='DataContext']/*[local-name()='DataSourceCollection'], 'ForwardingConsol')][1]"/>
  <xsl:variable name="Shipment" select="(//*[local-name()='Shipment'][contains(./*[local-name()='DataContext']//*[local-name()='DataSource']/*[local-name()='Type'], 'ForwardingShipment') and
                                                                         not(contains(./*[local-name()='DataContext']//*[local-name()='DataSource']/*[local-name()='Type'], 'ForwardingConsol'))] | 
                                          //*[local-name()='SubShipment'][*[local-name()='DataContext']//*[local-name()='DataSource']/*[local-name()='Type'] = 'ForwardingShipment'])[1]"/>
  <xsl:variable name="SCAC" select="ScriptNS0:GetRecipientCodeUnkeyed('TRXELPELP' , 'TRXELPELP_G02' , 'Genco 214 - Send Shipment Status Events' , 'Defaults' , 'SCAC')"/>

  <xsl:template match="/">
    <xsl:element name="ns0:X12_00401_214">

      <xsl:element name="ST">

        <xsl:element name="ST01">
          <xsl:text>214</xsl:text>
        </xsl:element>

        <xsl:element name="ST02">
          <xsl:text>0000</xsl:text>
        </xsl:element>

      </xsl:element>

      <xsl:variable name="HWB">
        <xsl:call-template name="ApplyCharacterSet">
          <xsl:with-param name="Value" select="$Shipment/*[local-name()='WayBillNumber'][../*[local-name()='WayBillType']/*[local-name()='Code'] = 'HWB']" />
        </xsl:call-template>
      </xsl:variable>

      <xsl:element name="ns0:B10">

        <xsl:call-template name="MapValueIfNotEmpty">
          <xsl:with-param name="NodeName" select="'B1001'" />
          <xsl:with-param name="Value" select="$Shipment//*[local-name()='DataSource'][./*[local-name()='Type'] = 'ForwardingShipment']/*[local-name()='Key']" />
        </xsl:call-template>

        <xsl:element name="B1002">
          <xsl:value-of select="$HWB"/>
        </xsl:element>

        <xsl:element name="B1003">
          <xsl:value-of select="$SCAC"/>
        </xsl:element>

      </xsl:element>

      <xsl:call-template name="GenerateAddress">
        <xsl:with-param name="Address" select="$Shipment/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress']
                    [*[local-name()='AddressType'] = 'ConsignorDocumentaryAddress']" />
        <xsl:with-param name="Type" select="'SH'" />
      </xsl:call-template>

      <xsl:element name="ns0:LXLoop1">

        <xsl:element name="ns0:LX">
          <xsl:element name="LX01">
            <xsl:text>1</xsl:text>
          </xsl:element>
        </xsl:element>

        <xsl:variable name="ActionPurpose" select="//*[local-name()='ActionPurpose']/*[local-name()='Code']"/>
        <xsl:variable name="AT701" select="ScriptNS0:GetRecipientCode('TRXELPELP' , 'TRXELPELP_G02' , 'Genco 214 - Send Shipment Status Events' , 'Shipment Status' , 'Status Code AT701' , $ActionPurpose)"/>
        <xsl:element name="ns0:AT7Loop1">

          <xsl:element name="ns0:AT7">

            <xsl:if test="$AT701 != ''">
              <xsl:element name="AT701">
                <xsl:value-of select="$AT701" />
              </xsl:element>

              <xsl:element name="AT702">
                <xsl:value-of select="ScriptNS0:GetRecipientCodeUnkeyed('TRXELPELP' , 'TRXELPELP_G02' , 'Genco 214 - Send Shipment Status Events' , 'Defaults' , 'Reason Code AT702')"/>
              </xsl:element>
            </xsl:if>

            <xsl:call-template name="MapValueIfNotEmpty">
              <xsl:with-param name="NodeName" select="'AT703'" />
              <xsl:with-param name="Value" select="ScriptNS0:GetRecipientCode('TRXELPELP' , 'TRXELPELP_G02' , 'Genco 214 - Send Shipment Status Events' , 'Shipment Status' , 'Status Code AT703' , $ActionPurpose)" />
            </xsl:call-template>

            <xsl:call-template name="MapValueIfNotEmpty">
              <xsl:with-param name="NodeName" select="'AT704'" />
              <xsl:with-param name="Value" select="ScriptNS0:GetRecipientCode('TRXELPELP' , 'TRXELPELP_G02' , 'Genco 214 - Send Shipment Status Events' , 'Shipment Status' , 'Status Code AT704' , $ActionPurpose)" />
            </xsl:call-template>

            <xsl:variable name="DateTime" select="//*[local-name()='TriggerDate']"/>

            <xsl:element name="AT705">
              <xsl:value-of select="ScriptNS1:FormatXmlDateTime($DateTime, 'yyyyMMdd')"/>
            </xsl:element>

            <xsl:element name="AT706">
              <xsl:value-of select="ScriptNS1:FormatXmlDateTime($DateTime, 'HHmm')"/>
            </xsl:element>

          </xsl:element>

          <xsl:choose>
            <xsl:when test="$ActionPurpose = 'X3' or $ActionPurpose = 'AF'">
              <xsl:variable name="PickupFrom" select="$Shipment/*[local-name()='OrganizationAddressCollection']
                      /*[local-name()='OrganizationAddress' and *[local-name()='AddressType']='ConsignorPickupDeliveryAddress']"/>

              <xsl:choose>
                <xsl:when test="$PickupFrom != ''">
                  <xsl:call-template name="GenerateMS1">
                    <xsl:with-param name="Address" select="$PickupFrom"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name="GenerateMS1">
                    <xsl:with-param name="Address" select="$Shipment/*[local-name()='OrganizationAddressCollection']
                      /*[local-name()='OrganizationAddress' and *[local-name()='AddressType']='ConsignorDocumentaryAddress']"/>
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>

            <xsl:when test="$ActionPurpose = 'AG' or $ActionPurpose = 'X1' or $ActionPurpose = 'D1'">
              <xsl:variable name="DeliveryTo" select="$Shipment/*[local-name()='OrganizationAddressCollection']
                      /*[local-name()='OrganizationAddress' and *[local-name()='AddressType']='ConsigneePickupDeliveryAddress']"/>

              <xsl:choose>
                <xsl:when test="$DeliveryTo != ''">
                  <xsl:call-template name="GenerateMS1">
                    <xsl:with-param name="Address" select="$DeliveryTo"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name="GenerateMS1">
                    <xsl:with-param name="Address" select="$Shipment/*[local-name()='OrganizationAddressCollection']
                      /*[local-name()='OrganizationAddress' and *[local-name()='AddressType']='ConsigneeDocumentaryAddress']"/>
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
          </xsl:choose>

          <xsl:for-each select="$Consol/*[local-name()='ContainerCollection']/*[local-name()='Container'][position() &lt;= 10]">
            <xsl:element name="ns0:MS2">

              <xsl:call-template name="MapValueIfNotEmpty">
                <xsl:with-param name="NodeName" select="'MS201'" />
                <xsl:with-param name="Value" select="substring(*[local-name()='ContainerNumber'], 1, 4)" />
              </xsl:call-template>

              <xsl:call-template name="MapValueIfNotEmpty">
                <xsl:with-param name="NodeName" select="'MS202'" />
                <xsl:with-param name="Value" select="substring(*[local-name()='ContainerNumber'], 5)" />
              </xsl:call-template>

            </xsl:element>
          </xsl:for-each>

        </xsl:element>

        <xsl:element name="ns0:AT8">

          <xsl:element name="AT801">
            <xsl:text>G</xsl:text>
          </xsl:element>

          <xsl:element name="AT802">
            <xsl:value-of select="ScriptNS0:GetRecipientCode('TRXELPELP' , 'TRXELPELP_G02' , 'Genco 214 - Send Shipment Status Events' , 'Unit of Measurement', 'X12 Code', 
                            $Shipment/*[local-name()='TotalWeightUnit']/*[local-name()='Code'])"/>
          </xsl:element>

          <xsl:element name="AT803">
            <xsl:variable name="Weight" select="format-number($Shipment/*[local-name()='TotalWeight'], '.##')"/>
            <xsl:choose>
              <xsl:when test="$Weight > 0">
                <xsl:value-of select="$Weight"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="'0'"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:element>

          <xsl:call-template name="MapValueIfNotEmpty">
            <xsl:with-param name="NodeName" select="'AT804'" />
            <xsl:with-param name="Value" select="format-number($Shipment/*[local-name() = 'OuterPacks'], '0')" />
          </xsl:call-template>

        </xsl:element>

        <xsl:call-template name="MapL11">
          <xsl:with-param name="Value" select="$HWB" />
          <xsl:with-param name="Code" select="'BM'" />
        </xsl:call-template>

        <xsl:variable name="Max">
          <xsl:choose>
            <xsl:when test="$HWB != ''">
              <xsl:value-of select="9"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="10"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <xsl:for-each select="$Shipment/*[local-name()='LocalProcessing']/*[local-name()='OrderNumberCollection']/*[local-name()='OrderNumber'][position() &lt;= $Max]">
          <xsl:call-template name="MapL11">
            <xsl:with-param name="Value" select="*[local-name()='OrderReference']" />
            <xsl:with-param name="Code" select="'PO'" />
          </xsl:call-template>
        </xsl:for-each>

        <xsl:call-template name="MapK1">
          <xsl:with-param name="Instruction" select="normalize-space($Shipment/*[local-name()='NoteCollection']
                          /*[local-name()='Note' and *[local-name()='Description']/text()='Import Delivery Instructions'] /*[local-name()='NoteText'])" />
          <xsl:with-param name="Count" select="1"/>
        </xsl:call-template>

      </xsl:element>

    </xsl:element>
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
            <xsl:call-template name="ApplyCharacterSet">
              <xsl:with-param name="Value" select="$Value" />
            </xsl:call-template>
          </xsl:element>
        </xsl:otherwise>

      </xsl:choose>
    </xsl:if>

  </xsl:template>

  <xsl:template name="ApplyCharacterSet">
    <xsl:param name="Value" />
    <xsl:param name="CharacterSet" select="'Basic'" />
    <xsl:param name="ReplaceWithValidCharacters" select="true()" />

    <xsl:variable name="Upper" select="'ABCDEFGHIJKLMNOPQRSTUVWXYZ'" />
    <xsl:variable name="Lower" select="'abcdefghijklmnopqrstuvwxyz'" />
    <xsl:variable name="BasicCharacterSet">
      <xsl:value-of select="$Upper"/>
      <xsl:text>0123456789!&quot;&amp;&apos;()*+,-./:;?= </xsl:text>
    </xsl:variable>
    <xsl:variable name="SelectLanguageCharacters" select="'ÀÁÂÄàáâäÈÉÊèéêëÌÍÎìíîïÒÓÔÖòóôöÙÚÛÜùúûüÇçÑñ¿¡'" />
    <xsl:variable name="CorrespondingCharacters"  select="'AAAAAAAAEEEEEEEIIIIIIIOOOOOOOOUUUUUUUUCCNN?!'" />
    <xsl:variable name="ExtendedCharacterSet">
      <xsl:value-of select="$BasicCharacterSet"/>
      <xsl:value-of select="$Lower"/>
      <xsl:text>%@[]_{}\|&lt;&gt;~#$</xsl:text>
      <xsl:value-of select="$SelectLanguageCharacters"/>
    </xsl:variable>

    <xsl:choose>

      <xsl:when test="$CharacterSet = 'Basic'">
        <xsl:choose>

          <xsl:when test="$ReplaceWithValidCharacters">
            <xsl:variable name="NewValue" select="translate($Value, concat($Lower, $SelectLanguageCharacters), concat($Upper, $CorrespondingCharacters))" />
            <xsl:value-of select="translate($NewValue, translate($NewValue, $BasicCharacterSet, ''), '')"/>
          </xsl:when>

          <xsl:otherwise>
            <xsl:value-of select="translate($Value, translate($Value, $BasicCharacterSet, ''), '')"/>
          </xsl:otherwise>

        </xsl:choose>
      </xsl:when>

      <xsl:when test="$CharacterSet = 'Extended'">
        <xsl:value-of select="translate($Value, translate($Value, $ExtendedCharacterSet, ''), '')"/>
      </xsl:when>

      <xsl:otherwise>
        <xsl:value-of select="$Value"/>
      </xsl:otherwise>

    </xsl:choose>

  </xsl:template>

  <xsl:template name="GenerateAddress">
    <xsl:param name="Address" />
    <xsl:param name="Type" />

    <xsl:if test="$Address != '' and string-length($Address) > 0">
      <xsl:element name="ns0:N1Loop1">

        <xsl:element name="ns0:N1">

          <xsl:element name="N101">
            <xsl:value-of select="$Type"/>
          </xsl:element>

          <xsl:element name="N102">
            <xsl:call-template name="ApplyCharacterSet">
              <xsl:with-param name="Value" select="substring($Address/*[local-name()='CompanyName'], 1, 60)" />
            </xsl:call-template>
          </xsl:element>

          <xsl:element name="N103">
            <xsl:text>1</xsl:text>
          </xsl:element>

          <xsl:element name="N104">
            <xsl:call-template name="ApplyCharacterSet">
              <xsl:with-param name="Value">
                <xsl:variable name="GencoCustomerNumber" select="$Shipment/*[local-name()='CustomizedFieldCollection']
                              /*[local-name()='CustomizedField'][./*[local-name()='Key'] = 'Genco Customer Number']/*[local-name()='Value']" />
                <xsl:choose>

                  <xsl:when test="$GencoCustomerNumber != ''">
                    <xsl:value-of select="$GencoCustomerNumber"/>
                  </xsl:when>

                  <xsl:otherwise>
                    <xsl:value-of select="$Address/*[local-name()='OrganizationCode']"/>
                  </xsl:otherwise>
                  
                </xsl:choose>
              </xsl:with-param>
            </xsl:call-template>
          </xsl:element>

        </xsl:element>

      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="GenerateMS1">
    <xsl:param name="Address"/>

    <xsl:element name="ns0:MS1">

      <xsl:call-template name="MapValueIfNotEmpty">
        <xsl:with-param name="NodeName" select="'MS101'" />
        <xsl:with-param name="Value" select="substring($Address/*[local-name()='City'], 1, 30)" />
      </xsl:call-template>

      <xsl:variable name="State">
        <xsl:variable name="AddressState">
          <xsl:call-template name="ApplyCharacterSet">
            <xsl:with-param name="Value" select="$Address/*[local-name()='State']" />
          </xsl:call-template>
        </xsl:variable>

        <xsl:choose>

          <xsl:when test="$AddressState != '' and string-length($AddressState) > 0">
            <xsl:value-of select="$AddressState"/>
          </xsl:when>

          <xsl:otherwise>
            <xsl:call-template name="ApplyCharacterSet">
              <xsl:with-param name="Value" select="normalize-space(ScriptNS0:GetStateFromUNLOCO($Address/*[local-name()='Port']/*[local-name()='Code']))" />
            </xsl:call-template>
          </xsl:otherwise>

        </xsl:choose>
      </xsl:variable>
      <xsl:call-template name="MapValueIfNotEmpty">
        <xsl:with-param name="NodeName" select="'MS102'" />
        <xsl:with-param name="Value" select="substring($State, 1, 2)" />
      </xsl:call-template>

      <xsl:variable name="Country">
        <xsl:variable name="AddressCountry">
          <xsl:call-template name="ApplyCharacterSet">
            <xsl:with-param name="Value" select="$Address/*[local-name()='Country']/*[local-name()='Code']" />
          </xsl:call-template>
        </xsl:variable>

        <xsl:choose>

          <xsl:when test="$AddressCountry != '' and string-length($AddressCountry) > 0"  >
            <xsl:value-of select="$AddressCountry"/>
          </xsl:when>

          <xsl:otherwise>
            <xsl:call-template name="ApplyCharacterSet">
              <xsl:with-param name="Value" select="substring($Address/*[local-name()='Port']/*[local-name()='Code'], 1, 2)" />
            </xsl:call-template>
          </xsl:otherwise>

        </xsl:choose>
      </xsl:variable>
      <xsl:call-template name="MapValueIfNotEmpty">
        <xsl:with-param name="NodeName" select="'MS103'" />
        <xsl:with-param name="Value" select="$Country" />
      </xsl:call-template>

    </xsl:element>
  </xsl:template>

  <xsl:template name="MapL11">
    <xsl:param name="Value" />
    <xsl:param name="Code" />

    <xsl:variable name="NewValue">
      <xsl:call-template name="ApplyCharacterSet">
        <xsl:with-param name="Value" select="$Value" />
      </xsl:call-template>
    </xsl:variable>

    <xsl:if test="$NewValue != '' and string-length($NewValue) > 0">
      <xsl:element name="ns0:L11_3">

        <xsl:element name="L1101">
          <xsl:value-of select="substring($NewValue, 1, 30)"/>
        </xsl:element>

        <xsl:element name="L1102">
          <xsl:value-of select="$Code"/>
        </xsl:element>

      </xsl:element>
    </xsl:if>

  </xsl:template>

  <xsl:template name="MapK1">
    <xsl:param name="Instruction"/>
    <xsl:param name="Count"/>

    <xsl:if test="$Instruction != '' and $Count &lt;= 10">
      <xsl:element name="ns0:K1_2">
        <xsl:element name="K101">
          <xsl:value-of select="substring($Instruction, 1, 30)"/>
        </xsl:element>

        <xsl:call-template name="MapValueIfNotEmpty">
          <xsl:with-param name="NodeName" select="'K102'" />
          <xsl:with-param name="Value" select="substring($Instruction, 31, 30)" />
        </xsl:call-template>
      </xsl:element>

      <xsl:call-template name="MapK1">
        <xsl:with-param name="Instruction" select="normalize-space(substring($Instruction, 61))"/>
        <xsl:with-param name="Count" select="$Count+1"/>
      </xsl:call-template>
    </xsl:if>

  </xsl:template>

</xsl:stylesheet>

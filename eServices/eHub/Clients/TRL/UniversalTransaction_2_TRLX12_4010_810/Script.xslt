<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
  <xsl:output method="xml" indent="yes"/>




  <xsl:element name="BIG02">
    <xsl:value-of select="//*[local-name()='WayBillNumber'][../*[local-name()='WayBillType'][*[local-name()='Code'] = 'HWB']]"/>
  </xsl:element>
  
  



  <xsl:template name="generateIATACode">
    <xsl:param name="Port" />

    <xsl:variable name="IATACode" select="ScriptNS0:GetRecipientCode(&quot;TRLVSRTRI&quot; , &quot;TRLVSRTRI_L10&quot; , &quot;LTD 810 - Export A/R Invoices&quot; , &quot;IATA Code&quot; , &quot;Output Code&quot; , $Port)" />
    <xsl:choose>
      <xsl:when test="string-length($IATACode) > 3">
        <xsl:value-of select="substring($IATACode, 3, 3)"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$IATACode"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>




  <xsl:template name="generateDeptDate">
    <xsl:param name="TransLegMode"/>

    <xsl:variable name="TransportLeg" select="//*[local-name()='TransactionInfo']
                    /*[local-name()='ShipmentCollection']
                    /*[local-name()='Shipment']
                    /*[local-name()='TransportLegCollection']
                    /*[local-name()='TransportLeg' 
                    and *[local-name()='TransportMode']/text()=$TransLegMode]" />

    <xsl:for-each select="$TransportLeg">
      <xsl:sort select="*[local-name()='LegOrder']"/>
      <xsl:if test="position()=1">
        <xsl:variable name="EstimateLoading" select="*[local-name()='EstimatedDeparture']"/>
        <xsl:variable name="ActualLoading" select="*[local-name()='ActualDeparture']"/>
        <xsl:if test="$ActualLoading != '' or $EstimateLoading !=''">
          <xsl:element name="ns0:DTM_2">
            <DTM01>EXP</DTM01>
            <xsl:choose>
              <xsl:when test="$ActualLoading != ''">
                <DTM02>
                  <xsl:value-of select="userCSharp:FormatDateTime($ActualLoading, 'yyyy-MM-ddTHH:mm:ss', 'yyyyMMdd')"/>
                </DTM02>
              </xsl:when>
              <xsl:otherwise>
                <DTM02>
                  <xsl:value-of select="userCSharp:FormatDateTime($EstimateLoading, 'yyyy-MM-ddTHH:mm:ss', 'yyyyMMdd')"/>
                </DTM02>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:element>
        </xsl:if>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>





  <xsl:template match="@* | node()">


    <!-- N1 -->
    <xsl:variable name ="ConsigneeAddr" select="*[local-name()='TransactionInfo']
    /*[local-name()='ShipmentCollection']
    /*[local-name()='Shipment']
    /*[local-name()='SubShipmentCollection']
    /*[local-name()='SubShipment'
     and *[local-name()='DataContext']
    /*[local-name()='DataSourceCollection']
    /*[local-name()='DataSource']
    /*[local-name()='Type']/text()='ForwardingShipment'][1]
	/*[local-name()='OrganizationAddressCollection']
		/*[local-name()='OrganizationAddress' 
		and *[local-name()='AddressType']/text()='ConsigneeDocumentaryAddress']"
   />

    <xsl:element name ="ns0:N1">
      <N101>OB</N101>
      <N102>
        <xsl:value-of select="$ConsigneeAddr/*[local-name()='CompanyName']"/>
      </N102>
      <N103>ZZ</N103>
      <N104>
        <xsl:variable name="RegNo" select="$ConsigneeAddr/*[local-name()='RegistrationNumberCollection']/*[local-name()='RegistrationNumber'
                              and *[local-name()='CountryOfIssue']/*[local-name()='Code']/text() = 'US'
                             and *[local-name()='Type']/*[local-name()='Code']/text() = 'GTN']/*[local-name()='Value']"/>
        <xsl:choose>
          <xsl:when test="$RegNo != ''">
            <xsl:value-of select="$RegNo" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$ConsigneeAddr/*[local-name()='OrganizationCode']" />
          </xsl:otherwise>
        </xsl:choose>
      </N104>
    </xsl:element>



    <!-- R4 -->
    <xsl:variable name="Consol" select="*[local-name()='TransactionInfo']
    /*[local-name()='ShipmentCollection']
    /*[local-name()='Shipment'
  and *[local-name()='DataContext']
    /*[local-name()='DataSourceCollection']
    /*[local-name()='DataSource']
    /*[local-name()='Type']/text()='ForwardingConsol'][1]
	"/>

    <xsl:variable name="Shipment" select="*[local-name()='TransactionInfo']
    /*[local-name()='ShipmentCollection']
    /*[local-name()='Shipment']
    /*[local-name()='SubShipmentCollection']
    /*[local-name()='SubShipment'
  and *[local-name()='DataContext']
    /*[local-name()='DataSourceCollection']
    /*[local-name()='DataSource']
    /*[local-name()='Type']/text()='ForwardingShipment'][1]
	"/>

    <xsl:element name="ns0:R4">
      <R401>J</R401>
      <R404>
        <xsl:call-template name="generateIATACode">
          <xsl:with-param name="Port" select="$Consol/*[local-name()='PortOfLoading']/*[local-name()='Code']"/>
        </xsl:call-template>
      </R404>
    </xsl:element>

    <xsl:element name="ns0:R4">
      <R401>N</R401>
      <R404>
        <xsl:call-template name="generateIATACode">
          <xsl:with-param name="Port" select="$Shipment/*[local-name()='PortOfDestination']/*[local-name()='Code']"/>
        </xsl:call-template>
      </R404>
    </xsl:element>






    <!-- call templete to generate departure date -->
    <xsl:variable name="TransportMode" select="//*[local-name()='TransactionInfo']
                  /*[local-name()='ShipmentCollection']
                  /*[local-name()='Shipment']
                  /*[local-name()='TransportMode']
                  /*[local-name()='Code']" />
    <xsl:choose>
      <xsl:when test="$TransportMode = 'SEA'">
        <xsl:call-template name="generateDeptDate">
          <xsl:with-param name="TransLegMode" select="'Sea'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$TransportMode = 'AIR'">
        <xsl:call-template name="generateDeptDate">
          <xsl:with-param name="TransLegMode" select="'Air'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$TransportMode = 'ROA'">
        <xsl:call-template name="generateDeptDate">
          <xsl:with-param name="TransLegMode" select="'Road'"/>
        </xsl:call-template>
      </xsl:when>
    </xsl:choose>













    <!-- REF -->
    <xsl:variable name="Shipment" select="*[local-name()='TransactionInfo']
    /*[local-name()='ShipmentCollection']
    /*[local-name()='Shipment']
    /*[local-name()='SubShipmentCollection']
    /*[local-name()='SubShipment'
  and *[local-name()='DataContext']
    /*[local-name()='DataSourceCollection']
    /*[local-name()='DataSource']
    /*[local-name()='Type']/text()='ForwardingShipment'][1]
	"/>

    <xsl:if test="$Shipment/*[local-name()='TransportMode']/*[local-name()='Code'] = 'AIR'">
      <xsl:element name="ns0:REF_3">
        <REF01>AW</REF01>
        <REF02>
          <xsl:value-of select="$Shipment/*[local-name()='WayBillNumber']"/>
        </REF02>
      </xsl:element>
    </xsl:if>

    <xsl:if test="$Shipment/*[local-name()='TransportMode']/*[local-name()='Code'] = 'SEA'">
      <xsl:for-each select="$Shipment/*[local-name()='ContainerCollection']/*[local-name()='Container']">
        <xsl:element name="ns0:REF_3">
          <REF01>OC</REF01>
          <REF02>
            <xsl:value-of select="*[local-name()='ContainerNumber']"/>
          </REF02>
        </xsl:element>
      </xsl:for-each>
    </xsl:if>

    <xsl:for-each select ="$Shipment
					/*[local-name()='LocalProcessing']
					/*[local-name()='OrderNumberCollection']
					/*[local-name()='OrderNumber']">
      <xsl:element name="ns0:REF_3">
        <REF01>PO</REF01>
        <REF02>
          <xsl:choose>
            <xsl:when test="contains(*[local-name()='OrderReference'], '-')">
              <xsl:value-of select="substring-before(*[local-name()='OrderReference'], '-')"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="*[local-name()='OrderReference']"/>
            </xsl:otherwise>
          </xsl:choose>
        </REF02>
      </xsl:element>
    </xsl:for-each>



    <!-- IT102 -->
    <IT102>
      <xsl:value-of select="format-number(*[local-name()='TransactionInfo']
                    /*[local-name()='ShipmentCollection']
                    /*[local-name()='Shipment']
                    /*[local-name()='SubShipmentCollection']
                    /*[local-name()='SubShipment']
                    /*[local-name()='ActualChargeable'], '0.###')"/>
    </IT102>



    <!-- IT104 -->
    <xsl:variable name="ChargeableAmount" select="//*[local-name()='TransactionInfo']
                    /*[local-name()='ShipmentCollection']
                    /*[local-name()='Shipment']
                    /*[local-name()='SubShipmentCollection']
                    /*[local-name()='SubShipment']
                    /*[local-name()='ActualChargeable']"/>
    <xsl:variable name="FreightChargeCode" select="ScriptNS0:GetRecipientCodeUnkeyed('TRLVSRTRI' , 'TRLVSRTRI_L10' , 'LTD 810 - Export A/R Invoices', 'Defaults', 'Freight Charge Codes')"/>
    <xsl:variable name="AR1Value" select="//*[local-name()='TransactionInfo']
                          /*[local-name()='PostingJournalCollection']
                          /*[local-name()='PostingJournal' and contains($FreightChargeCode, *[local-name()='ChargeCode']/*[local-name()='Code']/text())]
                          /*[local-name()='OSAmount']"/>
    <xsl:variable name="UnitPrice" select="format-number($AR1Value div $ChargeableAmount, '0.###')"/>
    <xsl:if test="number($UnitPrice) = number($UnitPrice)">
      <IT104>
        <xsl:value-of select="$UnitPrice"/>
      </IT104>
    </xsl:if>


    <!-- SAC -->
    <xsl:for-each select="//*[local-name()='TransactionInfo']
                          /*[local-name()='PostingJournalCollection']
                          /*[local-name()='PostingJournal']">
      <xsl:element name ="ns0:SACLoop1">
        <xsl:element name ="ns0:SAC">
          <SAC01>C</SAC01>
          <SAC03>ZZ</SAC03>
          <SAC04>
            <xsl:variable name="ChargeCode" select="*[local-name()='ChargeCode']/*[local-name()='Code']"/>
            <xsl:value-of select="ScriptNS0:GetRecipientCode(&quot;TRLVSRTRI&quot; , &quot;TRLVSRTRI_L10&quot; , &quot;LTD 810 - Export A/R Invoices&quot; , &quot;Charge Code&quot; , &quot;Output Code&quot; , string($ChargeCode))"/>
          </SAC04>
          <SAC05>
            <xsl:value-of select="userCSharp:MathMultiply(string(*[local-name()='OSAmount']), '100')"/>
          </SAC05>
          <SAC09>
            <xsl:variable name="MeasurementCode" select="//*[local-name()='TransactionInfo']
                          /*[local-name()='ShipmentCollection']
                          /*[local-name()='Shipment']
                          /*[local-name()='SubShipmentCollection']
                          /*[local-name()='SubShipment']
                          /*[local-name()='TotalVolumeUnit']
                          /*[local-name()='Code']"/>
            <xsl:value-of select="ScriptNS0:GetRecipientCode(&quot;TRLVSRTRI&quot; , &quot;TRLVSRTRI_L10&quot; , &quot;LTD 810 - Export A/R Invoices&quot; , &quot;Measurement Unit&quot; , &quot;Output Code&quot; , string($MeasurementCode))"/>
          </SAC09>
          <SAC10>1</SAC10>

        </xsl:element>
      </xsl:element>
    </xsl:for-each>




  </xsl:template>
</xsl:stylesheet>

<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                
	xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl xsl"
>
  <xsl:output method="xml" indent="yes"/>

  <xsl:template match="@* | node()">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()"/>
    </xsl:copy>
  </xsl:template>



  <xsl:template name="Dummy">

    <xsl:variable name="IsExport" select="ScriptNS0:GetRecipientCode('AGSWORAGS' , 'AGSWORAGS_WWA' , 'WorldWideAlliance xml-File - Export Shipmnt Status' , 'Status Code' , 'Export Shipment Status' 
                        , string($purposeCode))"/>

    <xsl:if test="$IsExport = 'true'">
    <BookingNumber>
      <xsl:value-of select="//*[local-name()='ShipmentDetails']/*[local-name()='AgentReference']"/>
    </BookingNumber>
    </xsl:if>
    
    <CarrierBookingNumber>
      <xsl:variable name="BookingReference" select="//*[local-name()='ConsolDetail']/*[local-name()='BookingReference']" />
      <xsl:choose>
        <xsl:when test="$BookingReference != ''">
          <xsl:value-of select="$BookingReference"/>
        </xsl:when>
        <xsl:when test="$IsExport = 'true'">
          <xsl:value-of select="ScriptNS0:GetRecipientCodeUnkeyed('AGSWORAGS', 'AGSWORAGS_WWA', 'WorldWideAlliance xml-File - Export Shipmnt Status', 'Defaults', 'Carrier Booking No.')"/>
        </xsl:when>
      </xsl:choose>
    </CarrierBookingNumber>
    
    <xsl:variable name="Packages" select="//*[local-name()='ShipmentDetails']/*[local-name()='Packages']/*[local-name()='Package']" />
    <xsl:element name="CargoDetails">

      <xsl:element name="Pieces">
        <xsl:value-of select="sum($Packages/*[local-name()='NumberOfPacks'])"/>
      </xsl:element>

      <xsl:call-template name="SumAmount">
        <xsl:with-param name="Loop" select="$Packages" />
        <xsl:with-param name="Field" select="'Weight'" />
        <xsl:with-param name="Unit" select="'LB'" />
      </xsl:call-template>
      <xsl:element name="WeightLBS">
        <xsl:value-of select="userCSharp:GetAmount()"/>
      </xsl:element>

      <xsl:call-template name="SumAmount">
        <xsl:with-param name="Loop" select="$Packages" />
        <xsl:with-param name="Field" select="'Volume'" />
        <xsl:with-param name="Unit" select="'CF'" />
      </xsl:call-template>
      <xsl:element name="VolumeCBF">
        <xsl:value-of select="userCSharp:GetAmount()"/>
      </xsl:element>

      <xsl:call-template name="SumAmount">
        <xsl:with-param name="Loop" select="$Packages" />
        <xsl:with-param name="Field" select="'Weight'" />
        <xsl:with-param name="Unit" select="'KG'" />
      </xsl:call-template>
      <xsl:element name="WeightKG">
        <xsl:value-of select="userCSharp:GetAmount()"/>
      </xsl:element>

      <xsl:call-template name="SumAmount">
        <xsl:with-param name="Loop" select="$Packages" />
        <xsl:with-param name="Field" select="'Volume'" />
        <xsl:with-param name="Unit" select="'M3'" />
      </xsl:call-template>
      <xsl:element name="VolumeCBM">
        <xsl:value-of select="userCSharp:GetAmount()"/>
      </xsl:element>

      <xsl:variable name="IsHazardous" select="count($Packages[./*[local-name()='DangerousGoods'] != '' and string-length(./*[local-name()='DangerousGoods']) > 0]) > 0" />
      <xsl:element name="HazardousFlag">
        <xsl:choose>

          <xsl:when test="$IsHazardous">
            <xsl:text>Y</xsl:text>
          </xsl:when>

          <xsl:otherwise>
            <xsl:text>N</xsl:text>
          </xsl:otherwise>

        </xsl:choose>
      </xsl:element>

      <xsl:if test="$IsHazardous">
        <xsl:element name="HazardousDetails">

          <xsl:element name="Pieces">
            <xsl:value-of select="sum($Packages[./*[local-name()='DangerousGoods'] != '' and string-length(./*[local-name()='DangerousGoods']) > 0]/*[local-name()='NumberOfPacks'])"/>
          </xsl:element>

          <xsl:variable name="UNDG" select="$Packages/*[local-name()='DangerousGoods']/*[local-name()='UNDG']" />
          <xsl:call-template name="SumAmount">
            <xsl:with-param name="Loop" select="$UNDG" />
            <xsl:with-param name="Field" select="'Weight'" />
            <xsl:with-param name="Unit" select="'LB'" />
          </xsl:call-template>
          <xsl:element name="HazWeightLBS">
            <xsl:value-of select="userCSharp:GetAmount()"/>
          </xsl:element>

          <xsl:call-template name="SumAmount">
            <xsl:with-param name="Loop" select="$UNDG" />
            <xsl:with-param name="Field" select="'Volume'" />
            <xsl:with-param name="Unit" select="'CF'" />
          </xsl:call-template>
          <xsl:element name="HazVolumeCBF">
            <xsl:value-of select="userCSharp:GetAmount()"/>
          </xsl:element>

          <xsl:call-template name="SumAmount">
            <xsl:with-param name="Loop" select="$UNDG" />
            <xsl:with-param name="Field" select="'Weight'" />
            <xsl:with-param name="Unit" select="'KG'" />
          </xsl:call-template>
          <xsl:element name="HazWeightKG">
            <xsl:value-of select="userCSharp:GetAmount()"/>
          </xsl:element>

          <xsl:call-template name="SumAmount">
            <xsl:with-param name="Loop" select="$UNDG" />
            <xsl:with-param name="Field" select="'Volume'" />
            <xsl:with-param name="Unit" select="'M3'" />
          </xsl:call-template>
          <xsl:element name="HazVolumeCBM">
            <xsl:value-of select="userCSharp:GetAmount()"/>
          </xsl:element>

        </xsl:element>
      </xsl:if>

    </xsl:element>

    <xsl:variable name="purposeCode" select="/*[local-name()='ConsolsInternal']/*[local-name()='InterchangeInfo']/*[local-name()='Source']/*[local-name()='Purpose']/text()" />
    <xsl:variable name="IsExport" select="ScriptNS0:GetRecipientCode('AGSWORAGS' , 'AGSWORAGS_WWA' , 'WorldWideAlliance xml-File - Export Shipmnt Status' , 'Status Code' , 'Export Shipment Status' 
                        , string($purposeCode))"/>

    <xsl:if test="$IsExport = 'true'">
      <xsl:element name="ApplicationType">

        <xsl:variable name="ApplicationType" select="//*[local-name()='ShipmentDetails']/*[local-name()='CustomValues']/*[local-name()='CustomValue' and @*[local-name()='Name'] = 'ApplicationType']"/>
        <xsl:choose>
          <xsl:when test="$ApplicationType = 'WE'">
            <xsl:value-of select="'WE'"/>
          </xsl:when>
          <xsl:when test="$ApplicationType = '' and //*[local-name()='Shipment']/*[local-name()='Events']/*[local-name()='Event' and *[local-name()='Source'] = 'JobShipment' and *[local-name()='Code'] = 'DIM']">
            <xsl:value-of select="'ME'"/>
          </xsl:when>
          <xsl:when test="//*[local-name()='Shipment']/*[local-name()='Events']/*[local-name()='Event' and *[local-name()='Source'] = 'JobShipment' and *[local-name()='Code'] = 'ADD' and *[local-name()='User'] != '~BP' and *[local-name()='User'] != '~AD']">
            <xsl:value-of select="'MN'"/>
          </xsl:when>
        </xsl:choose>

      </xsl:element>
    </xsl:if>


  </xsl:template>

  <xsl:template name="MapCarrierSCAC">
    <xsl:variable name="CountryName" select="//*[local-name()='EDIOrganisation']/*[local-name()='OrganisationDetails']/*[local-name()='Location']/@Country"/>
    <xsl:variable name="CountryCode" select="ScriptNS0:CallActionProcedureHelper('GetCountryInfo', '@result', '@name', $CountryName)"/>

    <xsl:variable name="RegistrationNumber" select="//*[local-name()='Consol']/*[local-name()='ConsolDetail']/*[local-name()='Carrier']/*[local-name()='OrganisationDetails']/*[local-name()='RegistrationNumbers']/*[local-name()='RegistrationNumber'][*[local-name()='NumberType'] = 'CCC' and *[local-name()='CountryOfRegistration'] = $CountryCode][1]/*[local-name()='Number']"/>
    <xsl:if test="$RegistrationNumber">
      <CarrierSCAC>
        <xsl:value-of select="$RegistrationNumber"/>
      </CarrierSCAC>
    </xsl:if>
  </xsl:template>
  <xsl:template name="SumAmount">
    <xsl:param name="Loop" />
    <xsl:param name="Field" />
    <xsl:param name="Unit" />

    <xsl:variable name="ResetAmount" select="userCSharp:ResetAmount()" />
    <xsl:for-each select="$Loop">
      <xsl:variable name="Value" select="ScriptNS3:Convert(./*[local-name()=$Field], ./*[local-name()=$Field]/@*[local-name()='DimensionType'], $Unit)" />
      <xsl:variable name="AddAmount" select="userCSharp:AddAmount($Value)"/>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name ="WriteContainerInfo">
    <xsl:param name ="ContNum"/>
    <xsl:variable name="ContType" select ="//*[local-name()='Consol']/*[local-name()='ConsolDetail']/*[local-name()='Containers']
                  /*[local-name()='Container'][./*[local-name()='ContainerNumber'] = $ContNum]/*[local-name()='ContainerType']"/>


    <xsl:element name ="ContainerSize">
      <xsl:value-of select ="substring($ContType/*[local-name()='ContainerCode'], 1, 2)"/>
    </xsl:element>

    <xsl:element name ="ContainerType">
      <xsl:value-of select ="substring($ContType/*[local-name()='ContainerCode'], 3)"/>
    </xsl:element>

    <xsl:element name ="ContainerCode">
      <xsl:value-of select ="$ContType/@ISOCode"/>
    </xsl:element>

  </xsl:template>
  
  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[		

double amount = 0;

public double GetAmount()
{
  return amount;
}

public double AddAmount(double value)
{
  return amount += value;
}

public void ResetAmount()
{
  amount = 0;
}

public string ETAPortOfDischarge = "";

public void SetETAPortOfDischarge (string etaPortOfDischarge)
{
  ETAPortOfDischarge = etaPortOfDischarge;
}

public string GetETAPortOfDischarge()
{
return ETAPortOfDischarge;
}

]]>
  </msxsl:script>



  <xsl:template name="PopulateETAPortOfDischarge">
    <xsl:param name="DischargeETA" />
    <xsl:param name="DischargeATA" />

    <xsl:variable name="ETAPortOfDischarge">
      <xsl:choose>
        <xsl:when test= "ScriptNS1:ConvertLocalXmlDateTimeStringToUTC($DischargeATA , 'yyyy-MM-dd') != ''">
          <xsl:value-of select="ScriptNS1:ConvertLocalXmlDateTimeStringToUTC($DischargeATA , 'yyyy-MM-dd')"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="ScriptNS1:ConvertLocalXmlDateTimeStringToUTC($DischargeETA , 'yyyy-MM-dd')"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:if test="$ETAPortOfDischarge != ''">
      <xsl:element name="ETAPortOfDischarge">
        <xsl:value-of select="$ETAPortOfDischarge"/>
      </xsl:element>

      <xsl:variable name="ETASetter" select="userCSharp:SetETAPortOfDischarge($ETAPortOfDischarge)"/>

    </xsl:if>
  </xsl:template>


  <xsl:template name="PopulateETAPlaceOfDelivery">
    <xsl:param name="ShpETA" />

    <xsl:variable name="ETAPortOfDischarge" select="userCSharp:GetETAPortOfDischarge()"/>

    <xsl:variable name="FormattedShpETA" select="ScriptNS1:ConvertLocalXmlDateTimeStringToUTC($ShpETA,'yyyy-MM-dd')"/>

    <xsl:if test="$ETAPortOfDischarge != '' or $FormattedShpETA != ''">
      <xsl:element name="ETAPlaceOfDelivery">
        <xsl:choose>
          <xsl:when test ="translate(concat('0',$ETAPortOfDischarge), '-', '') &gt; translate(concat('0',$FormattedShpETA), '-', '')">
            <xsl:value-of select ="$ETAPortOfDischarge"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select ="$FormattedShpETA"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="ForIntelligentSense">
    
    <ReceivingWarehouse>
      <xsl:choose>
      <xsl:when test="$IsExport  = 'true'">
        <xsl:variable name ="PickupCFS" select="//*[local-name()='ShipmentDetails']/*[local-name()='Pickup']/*[local-name()='CFS']/*[local-name()='Address']/*[local-name()='Organisation']/*[local-name()='OrganisationDetails']/*[local-name()='Location']"/>
        <xsl:variable name ="DDepot" select="//*[local-name()='Consol']/*[local-name()='ConsolDetail']/*[local-name()='Departure']/*[local-name()='Depot']/*[local-name()='Organisation']/*[local-name()='OrganisationDetails']/*[local-name()='Location']"/>
        
        <xsl:choose>
          <xsl:when test="$PickupCFS != ''">
            <xsl:value-of select ="$PickupCFS"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select ="$DDepot"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
        
        <xsl:otherwise>
          <xsl:variable name ="ArrivalCFS" select="//*[local-name()='ShipmentDetails']/*[local-name()='Deliver']/*[local-name()='CFS']/*[local-name()='Address']/*[local-name()='Organisation']/*[local-name()='OrganisationDetails']/*[local-name()='Location']"/>
          <xsl:variable name ="ADepot" select="//*[local-name()='Consol']/*[local-name()='ConsolDetail']/*[local-name()='Arrival']/*[local-name()='Depot']/*[local-name()='Organisation']/*[local-name()='OrganisationDetails']/*[local-name()='Location']"/>

          <xsl:choose>
            <xsl:when test="$ArrivalCFS != ''">
              <xsl:value-of select ="$ArrivalCFS"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select ="$ADepot"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:otherwise>
      </xsl:choose>
    </ReceivingWarehouse>

    <xsl:if test ="$IsExport = 'true'">
      <xsl:variable name="WWAShpRef" select ="//*[local-name()='ShipmentDetails']/*[local-name()='ReferenceNumbers']/*[local-name()='ReferenceNumber'][./*[local-name()='Type'] = 'WWA']/*[local-name()='Number' and .!='']"/>
      <xsl:variable name="ShpAgentRef" select="//*[local-name()='ShipmentDetails']/*[local-name()='AgentReference']"/>
      <xsl:variable name="LoadPort" select="//*[local-name()='Consol']/*[local-name()='ConsolDetail']/*[local-name()='PortOfLoading']/*[local-name()='Port']"/>

      <xsl:variable name ="WWAShipmentReference">
        <xsl:choose>
          <xsl:when test ="$WWAShpRef !=''">
            <xsl:value-of select="$WWAShpRef"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:variable name="MemberOfficeCode" select="ScriptNS0:GetRecipientCode('AGSWORAGS' , 'AGSWORAGS_WWA' , 'WorldWideAlliance xml-File - Export Shipmnt Status' , 'Member Office Code', 'Member Office Code' , $LoadPort)"/>
            <xsl:if test ="$MemberOfficeCode != ''">
              <xsl:variable name="ConcatenatedRef" select="concat($ShpAgentRef,$MemberOfficeCode)"/>
              <xsl:value-of select="substring($ConcatenatedRef, string-length($ConcatenatedRef)-30 + 1, 30)"/>
            </xsl:if>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:if test="$WWAShipmentReference != ''">
        <xsl:element name='WWAShipmentReference'>
          <xsl:value-of select="$WWAShipmentReference"/>
        </xsl:element>
      </xsl:if>
    </xsl:if>

    <xsl:element name="CutoffReceivingWarehouse">
      <xsl:for-each select="//*[local-name()='ConsolDetail']/*[local-name()='PlannedLegs']/*[local-name()='PlannedLeg'][./*[local-name()='TransportMode'] = 
                    //*[local-name()='ConsolDetail']/*[local-name()='TransportMode']]">
        <xsl:sort select="./*[local-name()='LegOrderNumber']"/>

        <xsl:if test="position() = 1">
          <xsl:choose>

            <xsl:when test="$IsExport = 'false'">
              <xsl:choose>

                <xsl:when test="./*[local-name()='TransportMode'] = 'AIR'">
                  <xsl:call-template name="MapCutoffReceivingWarehouseDate">
                    <xsl:with-param name="Date" select="substring(./*[local-name()='RoadRailFlight']/*[local-name()='Dates']/*[local-name()='AvailableDate'], 1, 10)" />
                  </xsl:call-template>
                </xsl:when>

                <xsl:when test="./*[local-name()='TransportMode'] = 'SEA'">
                  <xsl:call-template name="MapCutoffReceivingWarehouseDate">
                    <xsl:with-param name="Date" select="substring(./*[local-name()='Vessel']/*[local-name()='LCLDates']/*[local-name()='AvailableDate'], 1, 10)" />
                  </xsl:call-template>
                </xsl:when>

              </xsl:choose>
            </xsl:when>

            <xsl:otherwise>
              <xsl:choose>

                <xsl:when test="./*[local-name()='TransportMode'] = 'AIR'">
                  <xsl:call-template name="MapCutoffReceivingWarehouseDate">
                    <xsl:with-param name="Date" select="substring(./*[local-name()='RoadRailFlight']/*[local-name()='Dates']/*[local-name()='CutOffDate'], 1, 10)" />
                  </xsl:call-template>
                </xsl:when>

                <xsl:when test="./*[local-name()='TransportMode'] = 'SEA'">
                  <xsl:call-template name="MapCutoffReceivingWarehouseDate">
                    <xsl:with-param name="Date" select="substring(./*[local-name()='Vessel']/*[local-name()='LCLDates']/*[local-name()='CutOffDate'], 1, 10)" />
                  </xsl:call-template>
                </xsl:when>

              </xsl:choose>
            </xsl:otherwise>

          </xsl:choose>
        </xsl:if>
      </xsl:for-each>
    </xsl:element>



  </xsl:template>



  <xsl:template name="MapCutoffReceivingWarehouseDate">
    <xsl:param name="Date" />

    <xsl:choose>

      <xsl:when test="$Date != '' and string-length($Date) > 0">
        <xsl:value-of select="$Date"/>
      </xsl:when>

      <xsl:otherwise>
        <xsl:value-of select="ScriptNS1:CurrentDateTime('yyyy-MM-dd')"/>
      </xsl:otherwise>

    </xsl:choose>
  </xsl:template>


  <xsl:template name="SetEventDateTime">
    <xsl:param name="purposeCode" />
    <xsl:param name="actualDateTime" />

    <xsl:variable name="interchangeDateTime" select="/*[local-name()='ConsolsInternal']/*[local-name()='InterchangeInfo']/*[local-name()='Date']/text()"/>
    <xsl:variable name="triggeredByDateTime" select="/*[local-name()='ConsolsInternal']/*[local-name()='Payload']/*[local-name()='Consols']/*[local-name()='Consol']/*[local-name()='Shipments']/*[local-name()='Shipment']/*[local-name()='Events']/*[local-name()='Event'][*[local-name()='TriggeredBy']/text()='true']/*[local-name()='DateTime']/text()"/>

    <StatusDateTimeDetails>
      <xsl:choose>
        <xsl:when test="$purposeCode='W40' and ScriptNS1:ConvertLocalXmlDateTimeStringToUTC($actualDateTime, 'yyyy-MM-dd') != ''">
          <xsl:call-template name ="PopulateEventDateTime">
            <xsl:with-param name ="DateTime" select ="$actualDateTime"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test ="$triggeredByDateTime != ''">
          <xsl:call-template name ="PopulateEventDateTime">
            <xsl:with-param name ="DateTime" select ="$triggeredByDateTime"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name ="PopulateEventDateTime">
            <xsl:with-param name ="DateTime" select ="$interchangeDateTime"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
      <TimeZone>GMT</TimeZone>
    </StatusDateTimeDetails>
  </xsl:template>


  <xsl:template name ="PopulateEventDateTime">
    <xsl:param name ="DateTime"/>
      <xsl:if test ="$DateTime != ''">
        <xsl:element name ="Date">
           <xsl:value-of select="ScriptNS1:ConvertLocalXmlDateTimeStringToUTC(string($DateTime), 'yyyy-MM-dd')"/>
        </xsl:element>
        <xsl:element name ="Time">
           <xsl:value-of select="ScriptNS1:ConvertLocalXmlDateTimeStringToUTC(string($DateTime), 'HH:mm:ss')"/>
        </xsl:element>
      </xsl:if>
  </xsl:template>
</xsl:stylesheet>

<xsl:variable name="purposeCode" select="/*[local-name()='ConsolsInternal']/*[local-name()='InterchangeInfo']/*[local-name()='Source']/*[local-name()='Purpose']/text()" />
<xsl:variable name="documentType" select="ScriptNS0:GetRecipientCode('AGSWORAGS' , 'AGSWORAGS_WWA' , 'WorldWideAlliance xml-File - Export Shipmnt Status' , 'Status Code' , 'Document Type' , string($purposeCode))" />
<xsl:variable name="counterDocument" select="count(/*[local-name()='ConsolsInternal']/*[local-name()='Payload']/*[local-name()='Consols']/*[local-name()='Consol']/*[local-name()='Shipments']/*[local-name()='Shipment']/*[local-name()='Documents']/*[local-name()='Document' and ./*[local-name()='DocumentType']/text()=$documentType])" />
<xsl:choose>
  <xsl:when test="$documentType!='' and $counterDocument &gt; 0">
    <xsl:for-each select="/*[local-name()='ConsolsInternal']/*[local-name()='Payload']/*[local-name()='Consols']/*[local-name()='Consol']/*[local-name()='Shipments']/*[local-name()='Shipment']/*[local-name()='Documents']/*[local-name()='Document' and ./*[local-name()='DocumentType']/text()=$documentType]">
      <xsl:sort select="./*[local-name()='Date']" order="descending" />
      <xsl:if test="position()=1">
        <DocumentationDetails>
          <xsl:variable name="dataType" select="./*[local-name()='DataType']" />
          <Image>
            <xsl:choose>
              <xsl:when test="$dataType='TIF'">
                <xsl:value-of select="ScriptNS4:ConvertTiff2Pdf(./*[local-name()='Data'])" />
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="./*[local-name()='Data']" />
              </xsl:otherwise>
            </xsl:choose>
          </Image>
          <ContentType>
            <xsl:choose>
              <xsl:when test="$dataType='PDF'">application/pdf</xsl:when>
              <xsl:when test="$dataType='TIF'">application/pdf</xsl:when>
              <xsl:when test="$dataType='HTM'">application/html</xsl:when>
              <xsl:when test="$dataType='PNG'">image/png</xsl:when>
              <xsl:when test="$dataType='GIF'">image/gif</xsl:when>
              <xsl:when test="$dataType='JPG'">image/jpeg</xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$dataType" />
              </xsl:otherwise>
            </xsl:choose>
          </ContentType>
        </DocumentationDetails>
      </xsl:if>
    </xsl:for-each>
  </xsl:when>
  <xsl:otherwise>
    <DocumentationDetails>
      <Image />
      <ContentType />
    </DocumentationDetails>
  </xsl:otherwise>
</xsl:choose>

<xsl:for-each select="/*[local-name()='ConsolsInternal']/*[local-name()='Payload']/*[local-name()='Consols']/*[local-name()='Consol'][1]">
  <CustomerAlias>
    
    <xsl:for-each select="./*[local-name()='Shipments']/*[local-name()='Shipment']/*[local-name()='ShipmentDetails'][1]">
      <xsl:choose>
        <xsl:when test="$IsExport = 'false'">
          <xsl:value-of select="./*[local-name()='Consignee']/*[local-name()='OrganisationDetails']/*[local-name()='RegistrationNumbers']/*[local-name()='RegistrationNumber'][*[local-name()='CountryOfRegistration']/text() = 'AU'][*[local-name()='NumberType']/text() = 'UNC'][1]/*[local-name()='Number']/text()" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="./*[local-name()='Consignor']/*[local-name()='OrganisationDetails']/*[local-name()='RegistrationNumbers']/*[local-name()='RegistrationNumber'][*[local-name()='CountryOfRegistration']/text() = 'AU'][*[local-name()='NumberType']/text() = 'UNC'][1]/*[local-name()='Number']/text()" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:for-each>
  </CustomerAlias>
</xsl:for-each>

<!--<CustomerAlias>
  <xsl:if test="substring(../../../../s0:ConsolDetail/s0:PortOfDischarge/s0:Port/text(), 1, 2) = 'AU'">
	<xsl:value-of select="../../s0:ShipmentDetails/s0:Consignee/s0:OrganisationDetails/s0:RegistrationNumbers/s0:RegistrationNumber[./s0:CountryOfRegistration/text() = 'AU' and ./s0:NumberType/text() = 'UNC'][1]/s0:Number/text()" />
  </xsl:if>
  <xsl:if test="substring(../../../../s0:ConsolDetail/s0:PortOfDischarge/s0:Port/text(), 1, 2) != 'AU'">
	<xsl:value-of select="../../s0:ShipmentDetails/s0:Consignor/s0:OrganisationDetails/s0:RegistrationNumbers/s0:RegistrationNumber[./s0:CountryOfRegistration/text() = 'AU' and ./s0:NumberType/text() = 'UNC'][1]/s0:Number/text()" />
  </xsl:if>
</CustomerAlias>-->

<xsl:variable name="counter1" select="count(../../../s0:ConsolDetail/s0:SendingAgent/s0:OrganisationDetails/s0:RegistrationNumbers/s0:RegistrationNumber[s0:CountryOfRegistration='AU' and s0:NumberType='UNC'])"/>
<xsl:choose>
  <xsl:when test="$counter1 > 0">
    <xsl:for-each select="../../../s0:ConsolDetail">
      <xsl:for-each select="s0:SendingAgent">
        <xsl:for-each select="s0:OrganisationDetails">
          <xsl:for-each select="s0:RegistrationNumbers">
            <xsl:for-each select="s0:RegistrationNumber">
              <xsl:if test="string(s0:CountryOfRegistration)='AU' and string(s0:NumberType)='UNC'">
                <SenderID>
                  <xsl:value-of select="s0:Number/text()" />
                </SenderID>
              </xsl:if>
            </xsl:for-each>
          </xsl:for-each>
        </xsl:for-each>
      </xsl:for-each>
    </xsl:for-each>
  </xsl:when>
  <xsl:otherwise>
    <SenderID/>
  </xsl:otherwise>
</xsl:choose>

<xsl:variable name="counter2" select="count(../../../s0:ConsolDetail/s0:ReceivingAgent/s0:OrganisationDetails/s0:RegistrationNumbers/s0:RegistrationNumber[s0:CountryOfRegistration='AU' and s0:NumberType='UNC'])"/>
<xsl:choose>
  <xsl:when test="$counter2 > 0">
    <xsl:for-each select="../../../s0:ConsolDetail">
      <xsl:for-each select="s0:ReceivingAgent">
        <xsl:for-each select="s0:OrganisationDetails">
          <xsl:for-each select="s0:RegistrationNumbers">
            <xsl:for-each select="s0:RegistrationNumber">
              <xsl:if test="string(s0:CountryOfRegistration)='AU' and string(s0:NumberType)='UNC'">
                <ReceiverID>
                  <xsl:value-of select="s0:Number/text()" />
                </ReceiverID>
              </xsl:if>
            </xsl:for-each>
          </xsl:for-each>
        </xsl:for-each>
      </xsl:for-each>
    </xsl:for-each>
  </xsl:when>
  <xsl:otherwise>
    <ReceiverID/>
  </xsl:otherwise>
</xsl:choose>


<xsl:variable name="counter3" select="count(../../s0:ShipmentDetails/s0:LocalClient/s0:OrganisationDetails/s0:RegistrationNumbers/s0:RegistrationNumber[s0:CountryOfRegistration='AU' and s0:NumberType='UNC'])"/>
<xsl:choose>
  <xsl:when test="$counter3 > 0">
    <xsl:for-each select="../../s0:ShipmentDetails">
      <xsl:for-each select="s0:LocalClient">
        <xsl:for-each select="s0:OrganisationDetails">
          <xsl:for-each select="s0:RegistrationNumbers">
            <xsl:for-each select="s0:RegistrationNumber">
              <xsl:if test="string(s0:CountryOfRegistration)='AU' and string(s0:NumberType)='UNC'">
                <CustomerAlias>
                  <xsl:value-of select="s0:Number/text()" />
                </CustomerAlias>
              </xsl:if>
            </xsl:for-each>
          </xsl:for-each>
        </xsl:for-each>
      </xsl:for-each>
    </xsl:for-each>
  </xsl:when>
  <xsl:otherwise>
    <CustomerAlias/>
  </xsl:otherwise>
</xsl:choose>

<xsl:template name="NormalizeSpace">
  <xsl:param name="bookingRef" />

  <xsl:call-template name="RemoveComma">
    <xsl:with-param name="ShipperRef" select="normalize-space($bookingRef)"/>
  </xsl:call-template>
</xsl:template>

<xsl:template name="RemoveComma">
  <xsl:param name="ShipperRef" />

  <xsl:variable name="LastChar" select="substring($ShipperRef, string-length($ShipperRef))"/>
  <xsl:choose>
    <xsl:when test="$LastChar = ',' or $LastChar = ' '">
      <xsl:call-template name="RemoveComma">
        <xsl:with-param name="ShipperRef" select="substring($ShipperRef, 1, string-length($ShipperRef)-1)"/>
      </xsl:call-template>
    </xsl:when>
    <xsl:otherwise>
      <xsl:element name="ShipperReference">
        <xsl:value-of select="$ShipperRef" />
      </xsl:element>
    </xsl:otherwise>
  </xsl:choose>
</xsl:template>


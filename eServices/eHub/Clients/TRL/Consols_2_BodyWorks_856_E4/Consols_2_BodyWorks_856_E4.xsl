<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var" exclude-result-prefixes="msxsl var s0 userCSharp ScriptNS0" version="1.0" xmlns:s0="http://www.edi.com.au/EnterpriseService/" xmlns:ns0="http://schemas.microsoft.com/BizTalk/EDI/X12/2006" xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp" xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0" xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />

  <xsl:key name="CPOs" match="*[local-name()='OrderLine']" use="concat(../../*[local-name()='OrderIdentifier']/*[local-name()='OrderNumber'], *[local-name()='OrderLineDetail']/*[local-name()='Custom']/*[local-name()='Text1'])"/>

  <xsl:template match="/">
    <xsl:apply-templates select="/s0:ConsolsInternal" />
  </xsl:template>
  <xsl:template match="/s0:ConsolsInternal">
    <ns0:X12_00401_856>
      <ST>
        <ST01>
          <xsl:text>856</xsl:text>
        </ST01>
        <ST02>
          <xsl:text>0001</xsl:text>
        </ST02>
      </ST>
      <xsl:variable name="Consol" select="//*[local-name()='Payload']
                              /*[local-name()='Consols']
                              /*[local-name()='Consol']"/>

      <xsl:variable name="Shipment" select="$Consol
                  /*[local-name()='Shipments']
                  /*[local-name()='Shipment']"/>

      <xsl:variable name ="Purpose" select="//*[local-name()='InterchangeInfo']/*[local-name()='Source']/*[local-name()='Purpose']"/>

      <xsl:variable name="DateTime" select="//*[local-name()='InterchangeInfo']/*[local-name()='Date']" />
      <ns0:BSN>
        
        <xsl:element name="BSN01">
          <xsl:value-of select="ScriptNS0:GetRecipientCode('TRLVSRTRI' , 'TRLVSRTRI_BW2' , 'BW 856 - Export Shipment &amp; Order data', 'Trigger Purpose', 'Purpose Code - BSN01', $Purpose)"/>
        </xsl:element>
          
        <xsl:if test="$Shipment/*[local-name()='ShipmentDetails']/*[local-name()='AgentReference']">
          <BSN02>
            <xsl:value-of select="$Shipment/*[local-name()='ShipmentDetails']/*[local-name()='AgentReference']" />
          </BSN02>
        </xsl:if>
        <BSN03>
          <xsl:value-of select="userCSharp:FormatDateTime($DateTime, 'yyyy-MM-ddTHH:mm:ss' , 'yyyyMMdd')" />
        </BSN03>
        <BSN04>
          <xsl:value-of select="userCSharp:FormatDateTime($DateTime, 'yyyy-MM-ddTHH:mm:ss' , 'HHmmss')" />
        </BSN04>
        <BSN05>
          <xsl:value-of select="'0001'" />
        </BSN05>
        <BSN06>
          <xsl:variable name="TransportMode" select="$Consol/*[local-name()='ConsolDetail']/*[local-name()='TransportMode']"/>
          <xsl:value-of select="'ZZ'"/>
        </BSN06>
      </ns0:BSN>

      <xsl:variable name="TransportMode" select="$Shipment/*[local-name()='ShipmentDetails']/*[local-name()='TransportMode']" />

      <xsl:element name ="ns0:HLLoop1">
        <xsl:element name="ns0:HL">
          <HL01>
            <xsl:value-of select="userCSharp:PutHL01()" />
          </HL01>
          <HL03>S</HL03>
        </xsl:element>

        <!--PID-->
        <xsl:if test="$Shipment/*[local-name()='ShipmentDetails']/*[local-name()='GoodsDescription'] != ''">
          <xsl:element name="ns0:PID" >
            <PID01>
              <xsl:value-of select="'F'" />
            </PID01>
            <PID05>
              <xsl:value-of select="$Shipment/*[local-name()='ShipmentDetails']/*[local-name()='GoodsDescription']" />
            </PID05>
          </xsl:element>
        </xsl:if>

        <!--TD1-->
        <xsl:variable name="Chargeable" select ="$Shipment/*[local-name()='ShipmentDetails']/*[local-name()='ChargeableWeight']"/>
        <xsl:variable name="Weight" select="$Shipment/*[local-name()='ShipmentDetails']/*[local-name()='Weight']"/>

        <xsl:if test="$Chargeable > 0">
          <xsl:element name="ns0:TD1">
            <TD106>B</TD106>
            <TD107>
              <xsl:value-of select="format-number($Chargeable, '0.###')"/>
            </TD107>
            <TD108>
              <xsl:value-of select="ScriptNS0:GetRecipientCode('TRLVSRTRI' , 'TRLVSRTRI_BW2' , 'BW 856 - Export Shipment &amp; Order data' , 'Measurement Code' , 'Output Code' , $Chargeable/@*[local-name()='DimensionType'])" />
            </TD108>
          </xsl:element>
        </xsl:if>

        <xsl:if test="$Weight > 0">
          <xsl:element name="ns0:TD1">
            <TD106>G</TD106>
            <TD107>
              <xsl:value-of select="format-number($Weight, '0.###')"/>
            </TD107>
            <TD108>
              <xsl:value-of select="ScriptNS0:GetRecipientCode('TRLVSRTRI' , 'TRLVSRTRI_BW2' , 'BW 856 - Export Shipment &amp; Order data' , 'Measurement Code' , 'Output Code' , $Weight/@*[local-name()='DimensionType'])" />
            </TD108>
          </xsl:element>
        </xsl:if>

        <xsl:if test="$Chargeable > 0">
          <xsl:element name="ns0:TD1">
            <TD106>A1</TD106>
            <TD107>
              <xsl:value-of select="format-number(sum(//*[local-name()='OrderDetail']/*[local-name()='Custom']/*[local-name()='Decimal1']), '0.###')"/>
            </TD107>
            <TD108>
              <xsl:value-of select="ScriptNS0:GetRecipientCode('TRLVSRTRI' , 'TRLVSRTRI_BW2' , 'BW 856 - Export Shipment &amp; Order data' , 'Measurement Code' , 'Output Code' , $Chargeable/@*[local-name()='DimensionType'])" />
            </TD108>
          </xsl:element>
        </xsl:if>

        <!--TD5-->


        <xsl:element name="ns0:TD5">
          <xsl:if test="$TransportMode = 'AIR'">
            <xsl:variable name="PlannedLeg" select="$Consol
                          /*[local-name()='ConsolDetail']
                          /*[local-name()='PlannedLegs']
                          /*[local-name()='PlannedLeg' and *[local-name()='TransportMode']/text()='AIR']" />


            <xsl:variable name="count" select="count($PlannedLeg)" />
            <xsl:for-each select="$PlannedLeg">
              <xsl:sort select="*[local-name()='LegOrderNumber']"/>
              <xsl:if test="position()=1">
                <xsl:variable name="FlightNo" select="*[local-name()='RoadRailFlight']/*[local-name()='FlightNoJourneyNoTruckRegNo']" />
                <xsl:if test="$FlightNo != ''">
                  <TD502>4</TD502>
                  <TD503>
                    <xsl:value-of select="substring($FlightNo, 1, 2)"/>
                  </TD503>
                </xsl:if>
              </xsl:if>
            </xsl:for-each>
          </xsl:if>

          <xsl:if test="$TransportMode = 'SEA'">
            <xsl:variable name="SCAC" select="$Consol
                            /*[local-name()='ConsolDetail']
                            /*[local-name()='Carrier']
                            /*[local-name()='OrganisationDetails']
                            /*[local-name()='RegistrationNumbers']
                            /*[local-name()='RegistrationNumber' and *[local-name()='CountryOfRegistration']/text()='US' and *[local-name()='NumberType']/text()='CCC']
                            /*[local-name()='Number']"/>
            <xsl:if test="$SCAC != ''">
              <TD502>2</TD502>
              <TD503>
                <xsl:value-of select="$SCAC"/>
              </TD503>
            </xsl:if>
          </xsl:if>

          <TD504>
            <xsl:value-of select="ScriptNS0:GetRecipientCode('TRLVSRTRI' , 'TRLVSRTRI_BW2' , 'BW 856 - Export Shipment &amp; Order data' , 'Transport Mode' , 'Output Code' , $TransportMode)" />
          </TD504>
          <TD507>OR</TD507>
          <TD508>
            <xsl:variable name="LoadingPort" select="$Consol/*[local-name()='ConsolDetail']/*[local-name()='PortOfLoading']/*[local-name()='Port']" />
            <xsl:call-template name="generateIATACode">
              <xsl:with-param name="Port" select="$LoadingPort"/>
            </xsl:call-template>
          </TD508>
        </xsl:element>

        <xsl:element name="ns0:TD5">
          <TD504>
            <xsl:value-of select="ScriptNS0:GetRecipientCode('TRLVSRTRI' , 'TRLVSRTRI_BW2' , 'BW 856 - Export Shipment &amp; Order data' , 'Transport Mode' , 'Output Code' , $TransportMode)" />
          </TD504>
          <TD507>PA</TD507>
          <TD508>
            <xsl:variable name="DischargePort" select="$Consol/*[local-name()='ConsolDetail']/*[local-name()='PortOfDischarge']/*[local-name()='Port']"/>
            <xsl:value-of select="ScriptNS0:GetRecipientCode('TRLVSRTRI' , 'TRLVSRTRI_BW2' , 'BW 856 - Export Shipment &amp; Order data' , 'Schedule D Code' , 'Output Code' , $DischargePort)" />
          </TD508>
        </xsl:element>

        <xsl:variable name="EntryPort" select="$Shipment/*[local-name()='ShipmentDetails']/*[local-name()='CustomValues']/*[local-name()='CustomValue'][@Name='Port of Entry']"/>
        <xsl:if test="$TransportMode = 'AIR' and $EntryPort != ''">
          <xsl:element name="ns0:TD5">
            <TD504>
              <xsl:value-of select="ScriptNS0:GetRecipientCode('TRLVSRTRI' , 'TRLVSRTRI_BW2' , 'BW 856 - Export Shipment &amp; Order data' , 'Transport Mode' , 'Output Code' , $TransportMode)" />
            </TD504>
            <TD507>PE</TD507>
            <TD508>
              <xsl:value-of select="ScriptNS0:GetRecipientCode('TRLVSRTRI' , 'TRLVSRTRI_BW2' , 'BW 856 - Export Shipment &amp; Order data' , 'Schedule D Code' , 'Output Code' , $EntryPort)" />
            </TD508>
          </xsl:element>
        </xsl:if>

        <xsl:element name="ns0:TD5">
          <TD504>
            <xsl:value-of select="ScriptNS0:GetRecipientCode('TRLVSRTRI' , 'TRLVSRTRI_BW2' , 'BW 856 - Export Shipment &amp; Order data' , 'Transport Mode' , 'Output Code' , $TransportMode)" />
          </TD504>
          <TD507>DE</TD507>
          <TD508>
            <xsl:variable name="Destination" select="$Shipment/*[local-name()='ShipmentDetails']/*[local-name()='PortofDestination']/*[local-name()='Port']" />
            <xsl:call-template name="generateIATACode">
              <xsl:with-param name="Port" select="$Destination"/>
            </xsl:call-template>
          </TD508>
        </xsl:element>

        <!--REF-->
        <xsl:if test="$TransportMode = 'AIR'">
          <xsl:variable name="TransportLeg" select="$Consol
                          /*[local-name()='ConsolDetail']
                          /*[local-name()='PlannedLegs']
                          /*[local-name()='PlannedLeg' and *[local-name()='TransportMode']/text()='AIR']" />

          <xsl:variable name="count" select="count($TransportLeg)" />

          <xsl:for-each select="$TransportLeg">
            <xsl:sort select="*[local-name()='LegOrderNumber']"/>
            <xsl:if test="position()=1">
              <xsl:variable name="FlightNo" select="*[local-name()='RoadRailFlight']/*[local-name()='FlightNoJourneyNoTruckRegNo']" />
              <xsl:if test="$FlightNo != ''">
                <xsl:element name="ns0:REF">
                  <REF01>DF</REF01>
                  <REF02>
                    <xsl:value-of select="substring($FlightNo, 3)"/>
                  </REF02>
                </xsl:element>
              </xsl:if>

              <xsl:variable name="MAWB" select="$Consol
                              /*[local-name()='ConsolIdentifier'][@ConsolIdentifierType='MasterWaybill']"/>
              <xsl:if test="$MAWB != ''">
                <xsl:element name="ns0:REF">
                  <REF01>DM</REF01>
                  <REF02>
                    <xsl:value-of select="$MAWB"/>
                  </REF02>
                </xsl:element>
              </xsl:if>

            </xsl:if>
            <xsl:if test="position()=$count">

              <xsl:variable name="FlightNo" select="*[local-name()='RoadRailFlight']/*[local-name()='FlightNoJourneyNoTruckRegNo']" />
              <xsl:if test="$FlightNo != ''">
                <xsl:element name="ns0:REF">
                  <REF01>AF</REF01>
                  <REF02>
                    <xsl:value-of select="substring($FlightNo, 3)"/>
                  </REF02>
                </xsl:element>
              </xsl:if>

              <xsl:variable name="MAWB" select="$Consol
                              /*[local-name()='ConsolIdentifier'][@ConsolIdentifierType='MasterWaybill']"/>
              <xsl:if test="$MAWB != ''">
                <xsl:element name="ns0:REF">
                  <REF01>AM</REF01>
                  <REF02>
                    <xsl:value-of select="$MAWB"/>
                  </REF02>
                </xsl:element>
              </xsl:if>

            </xsl:if>
          </xsl:for-each>

          <xsl:element name="ns0:REF">
            <REF01>AW</REF01>
            <REF02>
              <xsl:value-of select="$Shipment/*[local-name()='ShipmentIdentifier'][@ShipmentIdentifierType='Housebill']"/>
            </REF02>
          </xsl:element>
        </xsl:if>

        <xsl:variable name ="ServiceLevel" select ="$Shipment/*[local-name()='ShipmentDetails']/*[local-name()='ServiceLevel']"/>
        <xsl:variable name="XE" select="ScriptNS0:GetRecipientCode('TRLVSRTRI' , 'TRLVSRTRI_BW2' , 'BW 856 - Export Shipment &amp; Order data' , 'REF XE' , 'XE' , $ServiceLevel)" />
        <xsl:if test="$XE != ''">
          <xsl:element name="ns0:REF" >
            <REF01>XE</REF01>
            <REF02>
              <xsl:value-of select="$XE"/>
            </REF02>
          </xsl:element>
        </xsl:if>

        <xsl:variable name="RB" select="ScriptNS0:GetRecipientCode('TRLVSRTRI' , 'TRLVSRTRI_BW2' , 'BW 856 - Export Shipment &amp; Order data' , 'REF RB' , 'RB' , $XE,
                      $Shipment/*[local-name()='ShipmentDetails']/*[local-name()='AdditionalTerms'])" />
        <xsl:if test="$RB != ''">
          <xsl:element name="ns0:REF" >
            <REF01>RB</REF01>
            <REF02>
              <xsl:value-of select="$RB"/>
            </REF02>
          </xsl:element>
        </xsl:if>
        <!-- end of REF -->

        <!--CLD-->
        <xsl:element name="ns0:CLDLoop1">
          <xsl:element name="ns0:CLD">
            <CLD01>
              <xsl:value-of select="$Shipment/*[local-name()='ShipmentDetails']/*[local-name()='TotalOuterPacksQty']"/>
            </CLD01>
            <CLD02>
              <xsl:value-of select="$Shipment/*[local-name()='ShipmentDetails']/*[local-name()='TotalInnerPacksQty']"/>
            </CLD02>
            <CLD03>
              <xsl:value-of select="$Shipment/*[local-name()='ShipmentDetails']/*[local-name()='TotalInnerPacksQty']/@*[local-name()='DimensionType']"/>
            </CLD03>
            <xsl:variable name="WeightUnit" select="$Shipment/*[local-name()='ShipmentDetails']/*[local-name()='Weight']/@*[local-name()='DimensionType']"/>
            <CLD04>
              <xsl:value-of select="format-number(ScriptNS1:Convert($Weight, $WeightUnit, 'LB'), '0.###')"/>
            </CLD04>
            <CLD05>LB</CLD05>
          </xsl:element>
        </xsl:element>


        <!--MAN-->
        <xsl:variable name="MarksNumbers" select="$Shipment/*[local-name()='ShipmentDetails']/*[local-name()='MarksAndNumbers']"/>
        <xsl:if test="$MarksNumbers != ''">
          <xsl:element name="ns0:MAN">
            <MAN01>ZZ</MAN01>
            <MAN02>
              <xsl:value-of select="substring(normalize-space($MarksNumbers), 1, 48)"/>
            </MAN02>
          </xsl:element>
        </xsl:if>

        <!--DTM-->
        <xsl:variable name="PickupTime" select="$Shipment
                  /*[local-name()='ShipmentDetails']
                  /*[local-name()='Pickup']
                  /*[local-name()='GoodsPickup']" />
        <xsl:if test="$PickupTime">
          <xsl:call-template name="generateDTM">
            <xsl:with-param name="DateQualifier" select="'050'"/>
            <xsl:with-param name="DateTime" select="$PickupTime"/>
          </xsl:call-template>
        </xsl:if>

        <xsl:if test="$TransportMode = 'AIR'">
          <xsl:variable name="PlannedLeg" select="$Consol
                          /*[local-name()='ConsolDetail']
                          /*[local-name()='PlannedLegs']
                          /*[local-name()='PlannedLeg' and *[local-name()='TransportMode']/text()='AIR']" />

          <xsl:variable name="count" select="count($PlannedLeg)" />
          <xsl:for-each select="$PlannedLeg">
            <xsl:sort select="*[local-name()='LegOrderNumber']"/>

            <xsl:if test="position()=1">

              <xsl:variable name="EstimatedDeparture" select="*[local-name()='RoadRailFlight']/*[local-name()='ETD']" />
              <xsl:variable name="ActualDeparture" select="*[local-name()='RoadRailFlight']/*[local-name()='ATD']" />

              <xsl:if test="$EstimatedDeparture">
                <xsl:call-template name="generateDTM">
                  <xsl:with-param name="DateQualifier" select="'369'"/>
                  <xsl:with-param name="DateTime" select="$EstimatedDeparture"/>
                </xsl:call-template>
              </xsl:if>

              <xsl:if test="$ActualDeparture">
                <xsl:call-template name="generateDTM">
                  <xsl:with-param name="DateQualifier" select="'370'"/>
                  <xsl:with-param name="DateTime" select="$ActualDeparture"/>
                </xsl:call-template>
              </xsl:if>

            </xsl:if>
            <xsl:if test="position()=$count">

              <xsl:variable name="EstimatedArrival" select="*[local-name()='RoadRailFlight']/*[local-name()='ETA']" />
              <xsl:if test="$EstimatedArrival">

                <xsl:call-template name="generateDTM">
                  <xsl:with-param name="DateQualifier" select="'371'"/>
                  <xsl:with-param name="DateTime" select="$EstimatedArrival"/>
                </xsl:call-template>

                <xsl:call-template name="generateDTM">
                  <xsl:with-param name="DateQualifier" select="'096'"/>
                  <xsl:with-param name="DateTime" select="$EstimatedArrival"/>
                </xsl:call-template>

              </xsl:if>

            </xsl:if>
          </xsl:for-each>
        </xsl:if>

        <xsl:variable name="BookingTime" select="$Shipment
                          /*[local-name()='Events']
                          /*[local-name()='Event' and *[local-name()='Code']/text()='AID' and *[local-name()='Source']/text()='JobShipment']
                          /*[local-name()='DateTime']" />
        <xsl:call-template name="generateDTM">
          <xsl:with-param name="DateQualifier" select="'537'" />
          <xsl:with-param name="DateTime" select="$BookingTime" />
        </xsl:call-template>

        <!--N1-->
        <xsl:variable name="Consignee" select="$Shipment/*[local-name()='ShipmentDetails']
                    /*[local-name()='Consignee']"/>
        <xsl:element name="ns0:N1Loop1">
          <xsl:element name="ns0:N1">
            <N101>IM</N101>
            <N102>
              <xsl:value-of select="$Consignee/*[local-name()='OrganisationDetails']/*[local-name()='Name']"/>
            </N102>
            <N103>94</N103>
            <xsl:variable name="RegNo" select="$Consignee/*[local-name()='OrganisationDetails']/*[local-name()='RegistrationNumbers']/*[local-name()='RegistrationNumber'
                              and *[local-name()='CountryOfRegistration']='US'
                             and *[local-name()='NumberType'] = 'GTN']/*[local-name()='Number']"/>
            <xsl:choose>
              <xsl:when test="$RegNo">
                <N104>
                  <xsl:value-of select="$RegNo" />
                </N104>
              </xsl:when>
              <xsl:otherwise>
                <N104>
                  <xsl:value-of select="$Consignee/@*[local-name()='EDICode']" />
                </N104>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:element>
        </xsl:element>

        <xsl:variable name="Consignor" select="$Shipment/*[local-name()='ShipmentDetails']
                    /*[local-name()='Consignor']"/>
        <xsl:element name="ns0:N1Loop1">
          <xsl:element name="ns0:N1">
            <N101>SH</N101>
            <N102>
              <xsl:value-of select="$Consignor/*[local-name()='OrganisationDetails']/*[local-name()='Name']"/>
            </N102>
          </xsl:element>
        </xsl:element>
      </xsl:element>

      <!--Order Level-->
      <xsl:for-each select="$Shipment/*[local-name()='Orders']/*[local-name()='Order']">
        <xsl:variable name="OrderNumber" select="*[local-name()='OrderIdentifier']/*[local-name()='OrderNumber']"/>

        <xsl:element name ="ns0:HLLoop1">
          <xsl:element name="ns0:HL">
            <HL01>
              <xsl:variable name="ID" select="userCSharp:SetHL02O(userCSharp:PutHL01())" />
              <xsl:value-of select="userCSharp:GetHL02O()" />
            </HL01>
            <HL02>1</HL02>
            <HL03>O</HL03>
          </xsl:element>

          <!--PRF-->
          <xsl:element name="ns0:PRF">
            <PRF01>
              <xsl:value-of select="$OrderNumber"/>
            </PRF01>
          </xsl:element>

          <!--PKG-->
          <xsl:variable name="Description" select="*[local-name()='OrderDetail']/*[local-name()='Description']"/>
          <xsl:if test="$Description != ''">
            <xsl:element name="ns0:PKG">
              <PKG01>F</PKG01>
              <PKG05>
                <xsl:value-of select="$Description"/>
              </PKG05>
            </xsl:element>
          </xsl:if>

          <!--TD1-->
          <xsl:element name="ns0:TD1">
            <TD101>
              <xsl:value-of select="ScriptNS0:GetRecipientCodeUnkeyed('TRLVSRTRI' , 'TRLVSRTRI_BW2' , 'BW 856 - Export Shipment &amp; Order data' , 'Defaults' , 'Packing Code')" />
            </TD101>
            <TD102>
              <xsl:value-of select="format-number(sum(*[local-name()='OrderLines']/*[local-name()='OrderLine']/*[local-name()='OrderLineDetail']/*[local-name()='QtyOrdered' and text() > 0]), '0')"/>
            </TD102>
            <xsl:variable name="TotalWeight" select="*[local-name()='OrderDetail']/*[local-name()='ShipmentPlanning']/*[local-name()='Weight']" />
            <xsl:if test="$TotalWeight > 0">
              <TD106>G</TD106>
              <TD107>
                <xsl:value-of select="format-number($TotalWeight, '0.###')"/>
              </TD107>
              <TD108>
                <xsl:value-of select="ScriptNS0:GetRecipientCode('TRLVSRTRI' , 'TRLVSRTRI_BW2' , 'BW 856 - Export Shipment &amp; Order data' , 'Measurement Code' , 'Output Code' , $TotalWeight/@*[local-name()='DimensionType'])" />
              </TD108>
            </xsl:if>
          </xsl:element>


          <xsl:variable name="DimensionalWeight" select="*[local-name()='OrderDetail']/*[local-name()='Custom']/*[local-name()='Decimal1']"/>
          <xsl:variable name="BilledWeight" select="*[local-name()='OrderDetail']/*[local-name()='Custom']/*[local-name()='Decimal2']"/>
          <xsl:call-template name="GenerateTD1">
            <xsl:with-param name="WeightQualifier" select="'A1'"/>
            <xsl:with-param name="Weight" select="$DimensionalWeight"/>
          </xsl:call-template>

          <xsl:call-template name="GenerateTD1">
            <xsl:with-param name="WeightQualifier" select="'B'"/>
            <xsl:with-param name="Weight" select="$BilledWeight"/>
          </xsl:call-template>


          <!--REF-->
          <xsl:element name="ns0:REF">
            <REF01>55</REF01>
            <REF02>
              <xsl:value-of select="*[local-name()='OrderIdentifier']/*[local-name()='OrderNumberSplit']"/>
            </REF02>
          </xsl:element>

          <xsl:variable name="DeptCode" select="*[local-name()='OrderDetail']/*[local-name()='Custom']/*[local-name()='Text1']"/>
          <xsl:if test="$DeptCode != ''">
            <xsl:element name="ns0:REF">
              <REF01>DP</REF01>
              <REF02>
                <xsl:value-of select="$DeptCode"/>
              </REF02>
            </xsl:element>
          </xsl:if>

          <!--N1-->
          <xsl:variable name="Consignee" select="$Shipment/*[local-name()='ShipmentDetails']
                    /*[local-name()='Consignee']"/>
          <xsl:element name="ns0:N1Loop1">
            <xsl:element name="ns0:N1">
              <N101>IM</N101>
              <N102>
                <xsl:value-of select="$Consignee/*[local-name()='OrganisationDetails']/*[local-name()='Name']"/>
              </N102>
              <N103>94</N103>

              <xsl:variable name="RegNo" select="$Consignee/*[local-name()='OrganisationDetails']/*[local-name()='RegistrationNumbers']/*[local-name()='RegistrationNumber'
                              and *[local-name()='CountryOfRegistration']='US'
                             and *[local-name()='NumberType'] = 'GTN']/*[local-name()='Number']"/>
              <xsl:choose>
                <xsl:when test="$RegNo">
                  <N104>
                    <xsl:value-of select="$RegNo" />
                  </N104>
                </xsl:when>
                <xsl:otherwise>
                  <N104>
                    <xsl:value-of select="$Consignee/@*[local-name()='EDICode']" />
                  </N104>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:element>
          </xsl:element>

          <xsl:element name="ns0:N1Loop1">
            <xsl:element name="ns0:N1">
              <xsl:variable name="Buyer" select="*[local-name()='OrderDetail']
                          /*[local-name()='Buyer']" />

              <N101>OB</N101>
              <N102>
                <xsl:value-of select="$Buyer/*[local-name()='OrganisationDetails']
                          /*[local-name()='Name']"/>
              </N102>
              <N103>94</N103>

              <xsl:variable name="BuyerNo" select="$Buyer/*[local-name()='OrganisationDetails']/*[local-name()='RegistrationNumbers']/*[local-name()='RegistrationNumber'
                              and *[local-name()='CountryOfRegistration']='US'
                             and *[local-name()='NumberType'] = 'GTN']/*[local-name()='Number']"/>

              <xsl:choose>
                <xsl:when test="$BuyerNo">
                  <N104>
                    <xsl:value-of select="$BuyerNo" />
                  </N104>
                </xsl:when>
                <xsl:otherwise>
                  <N104>
                    <xsl:value-of select="$Buyer/@*[local-name()='EDICode']" />
                  </N104>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:element>
          </xsl:element>

        </xsl:element>


        <!-- Pack Level-->
        <xsl:for-each select="*[local-name()='OrderLines']/*[local-name()='OrderLine'][generate-id() = 
                      generate-id(key('CPOs', concat($OrderNumber, *[local-name()='OrderLineDetail']/*[local-name()='Custom']/*[local-name()='Text1']))[1])]">
          <xsl:element name ="ns0:HLLoop1">
            <xsl:element name="ns0:HL">
              <HL01>
                <xsl:variable name="ID" select="userCSharp:SetHL02P(userCSharp:PutHL01())" />
                <xsl:value-of select="userCSharp:GetHL02P()" />
              </HL01>
              <HL02>
                <xsl:value-of select="userCSharp:GetHL02O()" />
              </HL02>
              <HL03>P</HL03>
            </xsl:element>

            <xsl:variable name="CustomCPO" select="*[local-name()='OrderLineDetail']/*[local-name()='Custom']/*[local-name()='Text1']"/>
            <xsl:variable name="ConfirmNumber" select="../../*[local-name()='OrderDetail']/*[local-name()='ConfirmNumber']"/>
            <xsl:choose>
              <xsl:when test="$CustomCPO != ''">
                <xsl:element name="ns0:PRF">
                  <PRF01>
                    <xsl:value-of select="$CustomCPO"/>
                  </PRF01>
                </xsl:element>
              </xsl:when>
              <xsl:when test="$ConfirmNumber != ''">
                <xsl:element name="ns0:PRF">
                  <PRF01>
                    <xsl:value-of select="$ConfirmNumber"/>
                  </PRF01>
                </xsl:element>
              </xsl:when>
            </xsl:choose>

            <xsl:element name="ns0:MAN">
              <MAN01>CP</MAN01>
              <MAN02>1</MAN02>
              <MAN03>
                <xsl:value-of select="format-number(sum(key('CPOs', concat($OrderNumber, *[local-name()='OrderLineDetail']/*[local-name()='Custom']/*[local-name()='Text1']))/*[local-name()='OrderLineDetail']/*[local-name()='QtyOrdered' and text() > 0]), '0')"/>
              </MAN03>
            </xsl:element>

            <xsl:if test="../../*[local-name()='OrderDetail']/*[local-name()='Custom']/*[local-name()='Text1'] != ''">
              <xsl:element name="ns0:N1Loop1">
                <xsl:element name="ns0:N1">
                  <N101>UC</N101>
                  <N103>56</N103>
                  <N104>
                    <xsl:value-of select="../../*[local-name()='OrderDetail']/*[local-name()='Custom']/*[local-name()='Text1']"/>
                  </N104>
                </xsl:element>
              </xsl:element>
            </xsl:if>
          </xsl:element>

          <!--OrderLine Level-->
          <xsl:for-each select="key('CPOs', concat($OrderNumber, *[local-name()='OrderLineDetail']/*[local-name()='Custom']/*[local-name()='Text1']))">

            <xsl:element name ="ns0:HLLoop1">
              <xsl:element name="ns0:HL">
                <HL01>
                  <xsl:value-of select="userCSharp:PutHL01()" />
                </HL01>
                <HL02>
                  <xsl:value-of select="userCSharp:GetHL02P()" />
                </HL02>
                <HL03>I</HL03>
              </xsl:element>

              <!--LIN-->
              <xsl:element name="ns0:LIN">
                <LIN01>
                  <xsl:value-of select="*[local-name()='OrderLineNo']"/>
                </LIN01>
                <LIN02>ST</LIN02>
                <LIN03>
                  <xsl:value-of select="*[local-name()='OrderLineDetail']/*[local-name()='Product']"/>
                </LIN03>
              </xsl:element>

              <!--SN1-->
              <xsl:element name="ns0:SN1">
                <SN102>
                  <xsl:value-of select ="format-number(*[local-name()='OrderLineDetail']/*[local-name()='QtyOrdered'], '0')"/>
                </SN102>
                <SN103>EA</SN103>
              </xsl:element>

            </xsl:element>
          </xsl:for-each>
          <!-- end of orderline loop -->

        </xsl:for-each>
        <!-- end of Pack loop -->

      </xsl:for-each>
      <!-- end of order loop -->

    </ns0:X12_00401_856>
  </xsl:template>
  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[
public string DateCurrentDate()
{
	DateTime dt = DateTime.Now;
	return dt.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
}


public string DateCurrentTime()
{
	DateTime dt = DateTime.Now;
	return dt.ToString("T", System.Globalization.CultureInfo.InvariantCulture);
}


public string FormatDateTime(string val, string inFmts, string outFmt)
{
	DateTime parsedDate;
	try
	{
		parsedDate = DateTimeOffset.Parse(val).DateTime;
	}
	catch
	{
		return string.Empty;
	};
	return parsedDate.ToString(outFmt);
}



int hl01 = 0;
		public int PutHL01()
		{
			return ++hl01;
		}



public int GetHL01()
{
  return hl01;
}

public string PutHL03()
		{
			if (hl01 == 1) return "S";
			else return "O";
		}

public string PutHL02()
		{
			if (hl01 > 1) return "1";
			else return " ";
		}

int hl02O = 1;
		public int GetHL02O()
		{
			return hl02O;
		}

public void SetHL02O(int input)
{
  hl02O = input;
}

int hl02P = 1;
		public int GetHL02P()
		{
			return hl02P;
		}

public void SetHL02P(int input)
{
  hl02P = input;
}


public int REF01Number = 1;

public int GetREF01Number()
{
   return REF01Number++;
}


]]>
  </msxsl:script>
  <xsl:template name="generateDTM">
    <xsl:param name="DateQualifier" />
    <xsl:param name="DateTime" />

    <xsl:if test ="$DateTime != ''">
      <xsl:element name="ns0:DTM_2">
        <DTM01>
          <xsl:value-of select="$DateQualifier" />
        </DTM01>
        <DTM02>
          <xsl:value-of select="userCSharp:FormatDateTime($DateTime, 'yyyy-MM-ddTHH:mm:ss', 'yyyyMMdd')" />
        </DTM02>
        <DTM03>
          <xsl:value-of select="userCSharp:FormatDateTime($DateTime, 'yyyy-MM-ddTHH:mm:ss', 'HHmmss')" />
        </DTM03>
        <DTM04>ET</DTM04>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="generateIATACode">
    <xsl:param name="Port" />

    <xsl:variable name="IATACode" select="ScriptNS0:GetRecipientCode('TRLVSRTRI' , 'TRLVSRTRI_BW2' , 'BW 856 - Export Shipment &amp; Order data' , 'IATA Code' , 'Output Code' , $Port)" />
    <xsl:choose>
      <xsl:when test="string-length($IATACode) > 3">
        <xsl:value-of select="substring($IATACode, 3, 3)"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$IATACode"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="GenerateTD1">
    <xsl:param name="WeightQualifier"/>
    <xsl:param name="Weight"/>
    <xsl:variable name="Chargeable" select ="//*[local-name()='Shipments']
                  /*[local-name()='Shipment']
                  /*[local-name()='ShipmentDetails']
                  /*[local-name()='ChargeableWeight']"/>

    <xsl:element name="ns0:TD1">
      <TD106>
        <xsl:value-of select="$WeightQualifier"/>
      </TD106>
      <xsl:choose>
        <xsl:when test="$Weight > 0">
          <TD107>
            <xsl:value-of select="format-number($Weight, '0.###')"/>
          </TD107>
          <TD108>
            <xsl:value-of select="ScriptNS0:GetRecipientCode('TRLVSRTRI' , 'TRLVSRTRI_BW2' , 'BW 856 - Export Shipment &amp; Order data' , 'Measurement Code' , 'Output Code' , *[local-name()='OrderDetail']/*[local-name()='ShipmentPlanning']/*[local-name()='Weight']/@*[local-name()='DimensionType'])" />
          </TD108>
        </xsl:when>
        <xsl:otherwise>
          <TD107>
            <xsl:value-of select="format-number($Chargeable, '0.###')"/>
          </TD107>
          <TD108>
            <xsl:value-of select="ScriptNS0:GetRecipientCode('TRLVSRTRI' , 'TRLVSRTRI_BW2' , 'BW 856 - Export Shipment &amp; Order data' , 'Measurement Code' , 'Output Code' , $Chargeable/@*[local-name()='DimensionType'])" />
          </TD108>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:element>
  </xsl:template>


</xsl:stylesheet>

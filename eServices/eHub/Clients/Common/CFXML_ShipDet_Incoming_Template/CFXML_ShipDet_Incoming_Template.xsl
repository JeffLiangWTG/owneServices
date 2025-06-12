<?xml version="1.0" encoding="UTF-16"?>

<xsl:stylesheet xmlns:xsl        = "http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl      = "urn:schemas-microsoft-com:xslt"
                xmlns:var        = "http://schemas.microsoft.com/BizTalk/2003/var"
                exclude-result-prefixes="msxsl var ScriptNS0 ScriptNS1" version="1.0"
                xmlns:ns0        = "http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:ScriptNS0  = "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
                xmlns:ScriptNS1  = "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1" >
  
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />
  
  <xsl:variable name="Sender"    select="'SSSSSSSSS_SSS'" />    <!-- "Sender Name" -->
  <xsl:variable name="Recipient" select="'RRRRRRRRR'" />        <!-- "Recipient Name" -->
  <xsl:variable name="TS_Name"   select="'TS Name for this Interface'" />
  
  <xsl:variable name="LowerChars" select="'abcdefghijklmnopqrstuvwxyz'" />
  <xsl:variable name="UpperChars" select="'ABCDEFGHIJKLMNOPQRSTUVWXYZ'" />
  
  <xsl:variable name="NewLine"    select="'&#x0D;&#x0A;'" />
  
  <xsl:variable name="MASTER_CARRIER" select="'MASTER_CARRIER'" />
  <xsl:variable name="NORMAL_CARRIER" select="'NORMAL_CARRIER'" />
  
  <xsl:variable name="Master" select="/*[local-name()='ShipmentBatch']
                                      /*[local-name()='ShipmentDetails']
                                      /*[local-name()='MasterBillDetails'] [1]" />
  
  <xsl:variable name="IsImports">
    <xsl:call-template name="Determine_if_Imports" />
  </xsl:variable>
  
  <xsl:variable name="AddressType_for_LocalAgent_Data">
    <xsl:call-template name="Determine_AddressType_for_LocalAgent_Data" />
  </xsl:variable>
  
  <xsl:variable name="AddressType_for_OverseasAgent_Data">
    <xsl:call-template name="Determine_AddressType_for_OverseasAgent_Data" />
  </xsl:variable>
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template match="/">
    
    <ns0:UniversalInterchange>
      <xsl:call-template name="Map_Header" />
      
      <ns0:Body>
        <ns0:UniversalShipment>
          
          <ns0:Shipment>
            <xsl:call-template name="Map_DataContext" />
            <xsl:call-template name="Map_Consol" />
          </ns0:Shipment>
          
        </ns0:UniversalShipment>
      </ns0:Body>
      
    </ns0:UniversalInterchange>
    
  </xsl:template>  <!-- End: Main Template -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Map_Header">
    
    <ns0:Header>
      <ns0:SenderID>
        <xsl:value-of select="$Sender" />
      </ns0:SenderID>
      
      <ns0:RecipientID>
        <xsl:value-of select="$Recipient" />
      </ns0:RecipientID>
    </ns0:Header>
    
  </xsl:template>  <!-- Template: "Map_Header" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Map_DataContext">
    
    <ns0:DataContext>
      
      <xsl:variable name="DataProvider" select="ScriptNS0:GetRecipientCodeUnkeyed( $Sender, $Recipient, $TS_Name, 'Defaults', 'Data Provider' )" />
      <xsl:variable name="CompanyCode"  select="ScriptNS0:GetRecipientCodeUnkeyed( $Sender, $Recipient, $TS_Name, 'Defaults', 'Company Code'  )" />
      <xsl:variable name="EnterpriseID" select="ScriptNS0:GetRecipientCodeUnkeyed( $Sender, $Recipient, $TS_Name, 'Defaults', 'Enterprise ID' )" />
      <xsl:variable name="ServerID"     select="ScriptNS0:GetRecipientCodeUnkeyed( $Sender, $Recipient, $TS_Name, 'Defaults', 'Server ID'     )" />
      
      
      <xsl:if test="($DataProvider != '')">
        <ns0:DataProvider>
          <xsl:value-of select="$DataProvider" />
        </ns0:DataProvider>
      </xsl:if>
      
      <ns0:DataTargetCollection>
        <ns0:DataTarget>
          <ns0:Type>
            <xsl:text>ForwardingConsol</xsl:text>
          </ns0:Type>
        </ns0:DataTarget>
      </ns0:DataTargetCollection>
      
      <xsl:if test="($CompanyCode != '')">
        <ns0:Company>
          <ns0:Code>
            <xsl:value-of select="$CompanyCode" />
          </ns0:Code>
        </ns0:Company>
      </xsl:if>
      
      <xsl:if test="($EnterpriseID != '')">
        <ns0:EnterpriseID>
          <xsl:value-of select="$EnterpriseID" />
        </ns0:EnterpriseID>
      </xsl:if>
      
      <xsl:if test="($ServerID != '')">
        <ns0:ServerID>
          <xsl:value-of select="$ServerID" />
        </ns0:ServerID>
      </xsl:if>
      
    </ns0:DataContext>
    
  </xsl:template>  <!-- Template: "Map_DataContext" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Map_Consol">
    
    <!-- Customized Fields: -->
    
    <ns0:CustomizedFieldCollection>
      
      <xsl:call-template name="MapCustomizedField">
        <xsl:with-param name="Key"   select="'Unique Shipment ID'" />
        <xsl:with-param name="Value" select="normalize-space( $Master/*[local-name()='UniqueShipmentID'] )" />
      </xsl:call-template>
      
      <xsl:call-template name="MapCustomizedField">
        <xsl:with-param name="Key"   select="'External Branch Code'" />
        <xsl:with-param name="Value" select="normalize-space( $Master/*[local-name()='ExternalBranchCode'] )" />
      </xsl:call-template>
      
      <xsl:call-template name="MapCustomizedField">
        <xsl:with-param name="Key"   select="'Shipment Number'" />
        <xsl:with-param name="Value" select="normalize-space( $Master/*[local-name()='ShipmentNumber'] )" />
      </xsl:call-template>
      
      <xsl:call-template name="MapCustomizedField">
        <xsl:with-param name="Key"   select="'Sub Bill No'" />
        <xsl:with-param name="Value" select="normalize-space( $Master/*[local-name()='SubBillNo'] )" />
      </xsl:call-template>
      
    </ns0:CustomizedFieldCollection>
    
    
    <!-- Branch Code: -->
    
    <xsl:variable name="BranchCode" select="ScriptNS0:GetRecipientCode( $Sender, $Recipient, $TS_Name, 
                                            'Branch Codes', 'CW1 Branch Code', $Master/*[local-name()='Branch'] )" />
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:Branch/ns0:Code'" />
      <xsl:with-param name="Data" select="$BranchCode" />
      <xsl:with-param name="Max"  select="3" />
    </xsl:call-template>
    
    
    <!-- Transport Mode: -->
    
    <xsl:variable name="TxMode">
      <xsl:call-template name="Determine_TransportMode">
        <xsl:with-param name="DocTxMode" select="$Master/*[local-name()='TransportMethod']" />
      </xsl:call-template>
    </xsl:variable>
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:TransportMode/ns0:Code'" />
      <xsl:with-param name="Data" select="$TxMode" />
      <xsl:with-param name="Max"  select="3" />
    </xsl:call-template>
    
    
    <!-- MasterBill No: -->
    
    <xsl:variable name="MasterBillNo">
      <xsl:variable name="Prefix" select="normalize-space( $Master/*[local-name()='MasterBillPrefix'] )" />
      
      <xsl:if test="($Prefix != '')">
        <xsl:value-of select="$Prefix" />
        <xsl:value-of select="'-'" />
      </xsl:if>
      
      <xsl:value-of select="normalize-space( $Master/*[local-name()='MasterBillNo'] )" />
    </xsl:variable>
    
    <xsl:if test="($MasterBillNo != '')">
      
      <xsl:call-template name="Map_String_IfNotEmpty">
        <xsl:with-param name="Node" select="'ns0:WayBillNumber'" />
        <xsl:with-param name="Data" select="$MasterBillNo" />
        <xsl:with-param name="Max"  select="35" />
      </xsl:call-template>
      
      <ns0:WayBillType>
        <ns0:Code>
          <xsl:text>MWB</xsl:text>
        </ns0:Code>
      </ns0:WayBillType>
      
    </xsl:if>
    
    
    <!-- Additional Bill Number: -->
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:AdditionalBillCollection/ns0:AdditionalBill/ns0:BillNumber'" />
      <xsl:with-param name="Data" select="$Master/*[local-name()='SubBillNo']" />
      <xsl:with-param name="Max"  select="35" />
    </xsl:call-template>
    
    
    <!-- Is Hazardous: -->
    
    <xsl:call-template name="Map_Boolean_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:IsHazardous'" />
      <xsl:with-param name="Data" select="$Master/*[local-name()='IsHazardous']" />
    </xsl:call-template>
    
    
    <!-- Port of Loading: -->
    
    <xsl:variable name="Port_Loading">
      <xsl:call-template name="Determine_Port">
        <xsl:with-param name="Country" select="$Master/*[local-name()='CountryOfLoading']" />
        <xsl:with-param name="Port"    select="$Master/*[local-name()='PortOfLoading']" />
      </xsl:call-template>
    </xsl:variable>
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:PortOfLoading/ns0:Code'" />
      <xsl:with-param name="Data" select="$Port_Loading" />
      <xsl:with-param name="Max"  select="5" />
    </xsl:call-template>
    
    
    <!-- Port of Discharge: -->
    
    <xsl:variable name="Port_Discharge">
      <xsl:call-template name="Determine_Port">
        <xsl:with-param name="Country" select="$Master/*[local-name()='CountryOfDischarge']" />
        <xsl:with-param name="Port"    select="$Master/*[local-name()='PortOfDischarge']" />
      </xsl:call-template>
    </xsl:variable>
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:PortOfDischarge/ns0:Code'" />
      <xsl:with-param name="Data" select="$Port_Discharge" />
      <xsl:with-param name="Max"  select="5" />
    </xsl:call-template>
    
    
    <!-- Port of Destination: -->
    
    <xsl:variable name="Port_Destination">
      <xsl:call-template name="Determine_Port">
        <xsl:with-param name="Country" select="$Master/*[local-name()='CountryOfDestination']" />
        <xsl:with-param name="Port"    select="$Master/*[local-name()='PortOfDestination']" />
      </xsl:call-template>
    </xsl:variable>
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:PortOfDestination/ns0:Code'" />
      <xsl:with-param name="Data" select="$Port_Destination" />
      <xsl:with-param name="Max"  select="5" />
    </xsl:call-template>
    
    
    <!-- Vessel Name: -->
    
    <xsl:call-template name="MapValueIfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:VesselName'" />
      <xsl:with-param name="Data" select="$Master/*[local-name()='Vessel']" />
    </xsl:call-template>
    
    
    <!-- Voyage/Flight No: -->
    
    <xsl:variable name="Voyage_01" select="normalize-space( $Master/*[local-name()='FlightOrVoyageNo1'] )" />
    
    <xsl:if test="($Voyage_01 != '')">
      
      <xsl:call-template name="Map_String_IfNotEmpty">
        <xsl:with-param name="Node" select="'ns0:VoyageFlightNo'" />
        <xsl:with-param name="Data" select="$Voyage_01" />
        <xsl:with-param name="Max"  select="10" />
      </xsl:call-template>
      
    </xsl:if>
    
    
    <!-- Carrier -and- Transport Legs: -->
    
    <xsl:variable name="Carrier_to_Use">
      <xsl:call-template name="Determine_Carrier_to_Use" />
    </xsl:variable>
    
    <xsl:if test="($Voyage_01 != '') or ($Carrier_to_Use != '')">
      
      <xsl:call-template name="Map_TransportLegs">
        <xsl:with-param name="Voyage_01"      select="$Voyage_01" />
        <xsl:with-param name="Voyage_02"      select="normalize-space( $Master/*[local-name()='FlightOrVoyageNo2'] )" />
        <xsl:with-param name="Voyage_03"      select="normalize-space( $Master/*[local-name()='FlightOrVoyageNo3'] )" />
        <xsl:with-param name="Carrier_to_Use" select="$Carrier_to_Use" />
      </xsl:call-template>
      
    </xsl:if>
    
    
    <!-- No of Pieces: -->
    
    <xsl:variable name="NoOfPieces" select="$Master/*[local-name()='NoOfPieces']" />
    
    <xsl:call-template name="Map_Integer_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:OuterPacks'" />
      <xsl:with-param name="Data" select="$NoOfPieces" />
    </xsl:call-template>
    
    <xsl:call-template name="Map_Integer_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:TotalNoOfPieces'" />
      <xsl:with-param name="Data" select="$NoOfPieces" />
    </xsl:call-template>
    
    <xsl:call-template name="Map_Integer_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:TotalNoOfPacks'" />
      <xsl:with-param name="Data" select="$NoOfPieces" />
    </xsl:call-template>
    
    
    <!-- Pack Type: -->
    
    <xsl:variable name="PackType" select="ScriptNS0:GetRecipientCode( $Sender, $Recipient, $TS_Name, 
                                    'Pack Types', 'CW1 Pack Type Code', $Master/*[local-name()='PackType'] )" />
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:OuterPacksPackageType/ns0:Code'" />
      <xsl:with-param name="Data" select="$PackType" />
      <xsl:with-param name="Max"  select="3" />
    </xsl:call-template>
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:TotalNoOfPacksPackageType/ns0:Code'" />
      <xsl:with-param name="Data" select="$PackType" />
      <xsl:with-param name="Max"  select="3" />
    </xsl:call-template>
    
    
    <!-- Weight: -->
    
    <xsl:variable name="Weight" select="$Master/*[local-name()='GrossWeightKG']" />
    
    <xsl:if test="($Weight != '')">
      
      <xsl:call-template name="MapValueIfNotEmpty">
        <xsl:with-param name="Node" select="'ns0:TotalWeight'" />
        <xsl:with-param name="Data" select="$Weight" />
      </xsl:call-template>
      
      <ns0:TotalWeightUnit>
        <ns0:Code>
          <xsl:text>KG</xsl:text>
        </ns0:Code>
      </ns0:TotalWeightUnit>
      
    </xsl:if>
    
    
    <!-- Actual Chargeable: -->
    
    <xsl:call-template name="MapValueIfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:ActualChargeable'" />
      <xsl:with-param name="Data" select="$Master/*[local-name()='ChargeableWeightKG']" />
    </xsl:call-template>
    
    
    <!-- Total Volume: -->
    
    <xsl:variable name="Volume" select="$Master/*[local-name()='CubicMeters']" />
    
    <xsl:if test="($Volume != '')">
      
      <xsl:call-template name="MapValueIfNotEmpty">
        <xsl:with-param name="Node" select="'ns0:TotalVolume'" />
        <xsl:with-param name="Data" select="$Volume" />
      </xsl:call-template>
      
      <ns0:TotalVolumeUnit>
        <ns0:Code>
          <xsl:text>M3</xsl:text>
        </ns0:Code>
      </ns0:TotalVolumeUnit>
      
    </xsl:if>
    
    
    <!-- No of Containers: -->
    
    <xsl:call-template name="Map_Integer_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:ContainerCount'" />
      <xsl:with-param name="Data" select="$Master/*[local-name()='NoOfContainers']" />
    </xsl:call-template>
    
    
    
    <!-- ToDo:
    <x "FreightAmountDetails"       type="FreightAmountDetailsType"  />
    -->
    
    
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:GoodsDescription'" />
      <xsl:with-param name="Data" select="$Master/*[local-name()='NatureOfGoods']" />
      <xsl:with-param name="Max"  select="128" />
    </xsl:call-template>
    
    
    <ns0:DateCollection>
      
      <xsl:call-template name="Map_DateCollection_Date">
        <xsl:with-param name="Type"       select="'BillIssued'" />
        <xsl:with-param name="IsEstimate" select="false()" />
        <xsl:with-param name="Value"      select="$Master/*[local-name()='ExecutionDate']" />
      </xsl:call-template>
      
      <xsl:call-template name="Map_DateCollection_Date">
        <xsl:with-param name="Type"       select="'BookingConfirmed'" />
        <xsl:with-param name="IsEstimate" select="false()" />
        <xsl:with-param name="Value"      select="$Master/*[local-name()='BookingReservedOn']" />
      </xsl:call-template>
      
      <xsl:call-template name="Map_DateCollection_Date">
        <xsl:with-param name="Type"       select="'ShippedOnBoard'" />
        <xsl:with-param name="IsEstimate" select="false()" />
        <xsl:with-param name="Value"      select="$Master/*[local-name()='ShipmentDate']" />
      </xsl:call-template>
      
    </ns0:DateCollection>
    
    
    <ns0:LocalProcessing>
      
      <xsl:call-template name="Map_Date_IfNotEmpty">
        <xsl:with-param name="Node" select="'ns0:EstimatedPickup'" />
        <xsl:with-param name="Data" select="$Master/*[local-name()='DepartureDate']" />
      </xsl:call-template>
      
      <xsl:call-template name="Map_Date_IfNotEmpty">
        <xsl:with-param name="Node" select="'ns0:EstimatedDelivery'" />
        <xsl:with-param name="Data" select="$Master/*[local-name()='ArrivalDate']" />
      </xsl:call-template>
      
    </ns0:LocalProcessing>
    
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:PlaceOfIssue/ns0:Code'" />
      <xsl:with-param name="Data" select="$Master/*[local-name()='PortOfIssue']" />
      <xsl:with-param name="Max"  select="5" />
    </xsl:call-template>
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:BookingConfirmationReference'" />
      <xsl:with-param name="Data" select="$Master/*[local-name()='BookingRef']" />
      <xsl:with-param name="Max"  select="35" />
    </xsl:call-template>
    
    
    <!-- Organizations: -->
    
    <ns0:OrganizationAddressCollection>
      
      <!-- Local Agent: -->
      
      <xsl:call-template name="Map_Org">
        <xsl:with-param name="OrgType"     select="$AddressType_for_LocalAgent_Data" />
        <xsl:with-param name="OrgName"     select="$Master/*[local-name()='LocalAgentName']" />
        <xsl:with-param name="Addr1"       select="$Master/*[local-name()='LocalAgentAddr1']" />
        <xsl:with-param name="Addr2"       select="$Master/*[local-name()='LocalAgentAddr2']" />
        <xsl:with-param name="Addr3"       select="$Master/*[local-name()='LocalAgentAddr3']" />
        <xsl:with-param name="Addr4"       select="$Master/*[local-name()='LocalAgentAddr4']" />
        <xsl:with-param name="AddrCode"    select="$Master/*[local-name()='LocalAgentAddrCode']" />
        <xsl:with-param name="ShortCode"   select="$Master/*[local-name()='LocalAgentShortCode']" />
      </xsl:call-template>
      
      <xsl:if test="($Master/*[local-name()='LocalAgentShortCode'] = '')
                and ($Master/*[local-name()='LocalAgentName']      = '')" >
        <xsl:call-template name="Map_Org">
          <xsl:with-param name="OrgType"   select="$AddressType_for_LocalAgent_Data" />
          <xsl:with-param name="OrgName"   select="$Master/*[local-name()='LocalForwardingAgent']" />
        </xsl:call-template>
      </xsl:if>
      
      
      <!-- Overseas Agent: -->
      
      <xsl:call-template name="Map_Org">
        <xsl:with-param name="OrgType"     select="$AddressType_for_OverseasAgent_Data" />
        <xsl:with-param name="OrgName"     select="$Master/*[local-name()='OverseasAgentName']" />
        <xsl:with-param name="Addr1"       select="$Master/*[local-name()='OverseasAgentAddr1']" />
        <xsl:with-param name="Addr2"       select="$Master/*[local-name()='OverseasAgentAddr2']" />
        <xsl:with-param name="Addr3"       select="$Master/*[local-name()='OverseasAgentAddr3']" />
        <xsl:with-param name="Addr4"       select="$Master/*[local-name()='OverseasAgentAddr4']" />
        <xsl:with-param name="AddrCode"    select="$Master/*[local-name()='OverseasAgentAddrCode']" />
        <xsl:with-param name="CountryCode" select="$Master/*[local-name()='OverseasAgentCountryCode']" />
        <xsl:with-param name="ShortCode"   select="$Master/*[local-name()='OverseasAgentShortCode']" />
      </xsl:call-template>
      
      
      <!-- Degroup Depot: -->
      
      <xsl:variable name="AddressType_for_DegroupDepot">
        <xsl:call-template name="Determine_AddressType_for_DegroupDepot" />
      </xsl:variable>
      
      <xsl:call-template name="Map_Org">
        <xsl:with-param name="OrgType"     select="$AddressType_for_DegroupDepot" />
        <xsl:with-param name="ShortCode"   select="$Master/*[local-name()='DegroupDepot']" />
      </xsl:call-template>
      
    </ns0:OrganizationAddressCollection>
    
    
    <ns0:NoteCollection>
      
      <xsl:call-template name="Create_Note_from_Array">
        <xsl:with-param name="Desc"   select="'HazardousDescription'" />
        <xsl:with-param name="ArrStr" select="$Master/*[local-name()='HazardousDescription']" />
      </xsl:call-template>
      
      <xsl:call-template name="Create_Note_from_Array">
        <xsl:with-param name="Desc"         select="'Goods Handling Instructions'" />
        <xsl:with-param name="ArrStr"       select="$Master/*[local-name()='HandlingInstructions']" />
        <xsl:with-param name="IsCustomNote" select="false()" />
      </xsl:call-template>
      
      <xsl:call-template name="Create_Note_from_Array">
        <xsl:with-param name="Desc"   select="'AccountingInfo'" />
        <xsl:with-param name="ArrStr" select="$Master/*[local-name()='AccountingInfo']" />
      </xsl:call-template>
      
      <xsl:call-template name="Create_Note_from_Array">
        <xsl:with-param name="Desc"   select="'ExternalOverseasAgentCode'" />
        <xsl:with-param name="ArrStr" select="$Master/*[local-name()='ExternalOverseasAgentCode']" />
      </xsl:call-template>
      
      <xsl:call-template name="Create_Note_from_Array">
        <xsl:with-param name="Desc"   select="'PrepaidCollect'" />
        <xsl:with-param name="ArrStr" select="$Master/*[local-name()='PrepaidCollect']" />
      </xsl:call-template>
      
      <xsl:call-template name="Create_Note_from_Array">
        <xsl:with-param name="Desc"   select="'VesselRadioCallSign'" />
        <xsl:with-param name="ArrStr" select="$Master/*[local-name()='VesselRadioCallSign']" />
      </xsl:call-template>
      
      <xsl:call-template name="Create_Note_from_Array">
        <xsl:with-param name="Desc"   select="'DegroupDepot'" />
        <xsl:with-param name="ArrStr" select="$Master/*[local-name()='DegroupDepot']" />
      </xsl:call-template>
      
      <xsl:call-template name="Create_Note_for_ContactInfo">
        <xsl:with-param name="Desc"       select="'ContactInfo'" />
        <xsl:with-param name="ParentNode" select="$Master/*[local-name()='ContactInfo']" />
        <xsl:with-param name="Separator"  select="$NewLine" />
      </xsl:call-template>
      
      <xsl:call-template name="Create_Note_for_Carrier">
        <xsl:with-param name="Desc"       select="'Carrier'" />
        <xsl:with-param name="ParentNode" select="$Master" />
        <xsl:with-param name="Separator"  select="$NewLine" />
      </xsl:call-template>
      
      <xsl:call-template name="Create_Note_for_MasterCarrier">
        <xsl:with-param name="Desc"       select="'MasterCarrier'" />
        <xsl:with-param name="ParentNode" select="$Master" />
        <xsl:with-param name="Separator"  select="$NewLine" />
      </xsl:call-template>
      
    </ns0:NoteCollection>
    
    
    <ns0:PackingLineCollection>
      <xsl:variable name="ArrPackage" select="$Master/*[local-name()='PackageDimensions']" />
      
      <xsl:for-each select="$ArrPackage">
        <ns0:PackingLine>
          
          <xsl:call-template name="Map_Package_from_Dimensions">
            <xsl:with-param name="Package" select="." />
          </xsl:call-template>
          
        </ns0:PackingLine>
      </xsl:for-each>
      
    </ns0:PackingLineCollection>
    
    
    
    <!-- ToDo:
    <x "SundryCharge"          type="SundryChargeType"        maxOccurs="unbounded" />
    -->
    
    
    
    <!-- ToDo:
    <x "AWBLineDetails"        type="AWBLineDetailsType"      maxOccurs="unbounded" />
    -->
    
    
    
    <!-- Sub-Shipment Collection: -->
    
    <ns0:SubShipmentCollection>
      <xsl:variable name="ArrHouse" select="$Master/*[local-name()='HouseBillDetails']" />
      
      <xsl:for-each select="$ArrHouse">
        <ns0:SubShipment>
          
          <xsl:call-template name="Map_SubShipment">
            <xsl:with-param name="House" select="." />
          </xsl:call-template>
          
        </ns0:SubShipment>
      </xsl:for-each>
      
    </ns0:SubShipmentCollection>
    
  </xsl:template>  <!-- Template: "Map_Consol" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Map_SubShipment">
    <xsl:param name="House" />
    
    
    <!-- HouseBill No: -->
    
    <xsl:variable name="HouseBillNo" select="normalize-space( $House/*[local-name()='HouseBillNo'] )" />
    
    <xsl:if test="($HouseBillNo != '')">
      
      <xsl:call-template name="Map_String_IfNotEmpty">
        <xsl:with-param name="Node" select="'ns0:WayBillNumber'" />
        <xsl:with-param name="Data" select="$HouseBillNo" />
        <xsl:with-param name="Max"  select="35" />
      </xsl:call-template>
      
      <ns0:WayBillType>
        <ns0:Code>
          <xsl:text>HWB</xsl:text>
        </ns0:Code>
      </ns0:WayBillType>
      
    </xsl:if>
    
    
    <!-- Customized Fields: -->
    
    <ns0:CustomizedFieldCollection>
      
      <xsl:call-template name="MapCustomizedField">
        <xsl:with-param name="Key"   select="'HouseBill Unique No'" />
        <xsl:with-param name="Value" select="normalize-space( $House/*[local-name()='HouseBillUniqueNo'] )" />
      </xsl:call-template>
      
    </ns0:CustomizedFieldCollection>
    
    
    <!-- Is Hazardous: -->
    
    <xsl:call-template name="Map_Boolean_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:IsHazardous'" />
      <xsl:with-param name="Data" select="$House/*[local-name()='IsHazardous']" />
    </xsl:call-template>
    
    
    <!-- Organizations: -->
    
    <ns0:OrganizationAddressCollection>
      
      <xsl:call-template name="Map_Org">
        <xsl:with-param name="OrgType"     select="'ConsignorDocumentaryAddress'" />
        <xsl:with-param name="OrgName"     select="$House/*[local-name()='ShipperName']" />
        <xsl:with-param name="Addr1"       select="$House/*[local-name()='ShipperAddr1']" />
        <xsl:with-param name="Addr2"       select="$House/*[local-name()='ShipperAddr2']" />
        <xsl:with-param name="Addr3"       select="$House/*[local-name()='ShipperAddr3']" />
        <xsl:with-param name="Addr4"       select="$House/*[local-name()='ShipperAddr4']" />
        <xsl:with-param name="AddrCode"    select="$House/*[local-name()='ShipperAddrCode']" />
        <xsl:with-param name="CountryCode" select="$House/*[local-name()='ShipperCountryCode']" />
        <xsl:with-param name="ShortCode"   select="$House/*[local-name()='ShipperShortCode']" />
      </xsl:call-template>
      
      <xsl:call-template name="Map_Org">
        <xsl:with-param name="OrgType"     select="'ConsigneeDocumentaryAddress'" />
        <xsl:with-param name="OrgName"     select="$House/*[local-name()='ConsigneeName']" />
        <xsl:with-param name="Addr1"       select="$House/*[local-name()='ConsigneeAddr1']" />
        <xsl:with-param name="Addr2"       select="$House/*[local-name()='ConsigneeAddr2']" />
        <xsl:with-param name="Addr3"       select="$House/*[local-name()='ConsigneeAddr3']" />
        <xsl:with-param name="Addr4"       select="$House/*[local-name()='ConsigneeAddr4']" />
        <xsl:with-param name="AddrCode"    select="$House/*[local-name()='ConsigneeAddrCode']" />
        <xsl:with-param name="CountryCode" select="$House/*[local-name()='ConsigneeCountryCode']" />
        <xsl:with-param name="ShortCode"   select="$House/*[local-name()='ConsigneeShortCode']" />
      </xsl:call-template>
      
      <xsl:call-template name="Map_Org">
        <xsl:with-param name="OrgType"     select="'NotifyParty'" />
        <xsl:with-param name="OrgName"     select="$House/*[local-name()='NotifyPartyName']" />
        <xsl:with-param name="Addr1"       select="$House/*[local-name()='NotifyPartyAddr1']" />
        <xsl:with-param name="Addr2"       select="$House/*[local-name()='NotifyPartyAddr2']" />
        <xsl:with-param name="Addr3"       select="$House/*[local-name()='NotifyPartyAddr3']" />
        <xsl:with-param name="Addr4"       select="$House/*[local-name()='NotifyPartyAddr4']" />
        <xsl:with-param name="AddrCode"    select="$House/*[local-name()='NotifyPartyAddrCode']" />
        <xsl:with-param name="CountryCode" select="$House/*[local-name()='NotifyPartyCountryCode']" />
        <xsl:with-param name="ShortCode"   select="$House/*[local-name()='NotifyPartyShortCode']" />
      </xsl:call-template>
      
      <xsl:call-template name="Map_Org">
        <xsl:with-param name="OrgType"     select="$AddressType_for_LocalAgent_Data" />
        <xsl:with-param name="OrgName"     select="$House/*[local-name()='LocalClearingAgent']" />
      </xsl:call-template>
      
      <xsl:call-template name="Map_Org">
        <xsl:with-param name="OrgType"     select="$AddressType_for_OverseasAgent_Data" />
        <xsl:with-param name="OrgName"     select="$House/*[local-name()='OverseasAgentName']" />
        <xsl:with-param name="Addr1"       select="$House/*[local-name()='OverseasAgentAddr1']" />
        <xsl:with-param name="Addr2"       select="$House/*[local-name()='OverseasAgentAddr2']" />
        <xsl:with-param name="Addr3"       select="$House/*[local-name()='OverseasAgentAddr3']" />
        <xsl:with-param name="Addr4"       select="$House/*[local-name()='OverseasAgentAddr4']" />
        <xsl:with-param name="AddrCode"    select="$House/*[local-name()='OverseasAgentAddrCode']" />
        <xsl:with-param name="CountryCode" select="$House/*[local-name()='OverseasAgentCountryCode']" />
        <xsl:with-param name="ShortCode"   select="$House/*[local-name()='OverseasAgentShortCode']" />
      </xsl:call-template>
      
      <xsl:call-template name="Map_Org">
        <xsl:with-param name="OrgType"     select="'LocalClient'" />
        <xsl:with-param name="OrgName"     select="$House/*[local-name()='DebtorName']" />
        <xsl:with-param name="ShortCode"   select="$House/*[local-name()='DebtorCode']" />
      </xsl:call-template>
      
    </ns0:OrganizationAddressCollection>
    
    
    <ns0:AdditionalReferenceCollection>
      
      <!-- Shipper Ref No: -->
      
      <xsl:variable name="ShipperRefNo" select="$House/*[local-name()='ShipperRefNo']" />
      
      <xsl:if test="($ShipperRefNo != '')">
        <xsl:call-template name="Map_Additional_Ref_By_Label">
          <xsl:with-param name="Label" select="'ShipperRefNo'" />
          <xsl:with-param name="Data"  select="$ShipperRefNo" />
        </xsl:call-template>
      </xsl:if>
      
      
      <!-- Consignee Ref No: -->
      
      <xsl:variable name="ConsigneeRefNo" select="$House/*[local-name()='ConsigneeRefNo']" />
      
      <xsl:if test="($ConsigneeRefNo != '')">
        <xsl:call-template name="Map_Additional_Ref_By_Label">
          <xsl:with-param name="Label" select="'ConsigneeRefNo'" />
          <xsl:with-param name="Data"  select="$ConsigneeRefNo" />
        </xsl:call-template>
      </xsl:if>
      
      
      <!-- NotifyParty Ref No: -->
      
      <xsl:variable name="NotifyPartyRefNo" select="$House/*[local-name()='NotifyPartyRefNo']" />
      
      <xsl:if test="($NotifyPartyRefNo != '')">
        <xsl:call-template name="Map_Additional_Ref_By_Label">
          <xsl:with-param name="Label" select="'NotifyPartyRefNo'" />
          <xsl:with-param name="Data"  select="$NotifyPartyRefNo" />
        </xsl:call-template>
      </xsl:if>
      
    </ns0:AdditionalReferenceCollection>
    
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:AdditionalTerms'" />
      <xsl:with-param name="Data" select="$House/*[local-name()='DeliveryTerms']" />
      <xsl:with-param name="Max"  select="50" />
    </xsl:call-template>
    
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:ShipmentIncoTerm/ns0:Code'" />
      <xsl:with-param name="Data" select="$House/*[local-name()='IncoTerms']" />
      <xsl:with-param name="Max"  select="3" />
    </xsl:call-template>
    
    
    <!-- No of Pieces: -->
    
    <xsl:variable name="NoOfPieces" select="$House/*[local-name()='NoOfPieces']" />
    
    <xsl:call-template name="Map_Integer_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:OuterPacks'" />
      <xsl:with-param name="Data" select="$NoOfPieces" />
    </xsl:call-template>
    
    <xsl:call-template name="Map_Integer_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:TotalNoOfPieces'" />
      <xsl:with-param name="Data" select="$NoOfPieces" />
    </xsl:call-template>
    
    <xsl:call-template name="Map_Integer_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:TotalNoOfPacks'" />
      <xsl:with-param name="Data" select="$NoOfPieces" />
    </xsl:call-template>
    
    
    <!-- Pack Type: -->
    
    <xsl:variable name="PackType" select="ScriptNS0:GetRecipientCode( $Sender, $Recipient, $TS_Name, 
                                    'Pack Types', 'CW1 Pack Type Code', $House/*[local-name()='PackType'] )" />
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:OuterPacksPackageType/ns0:Code'" />
      <xsl:with-param name="Data" select="$PackType" />
      <xsl:with-param name="Max"  select="3" />
    </xsl:call-template>
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:TotalNoOfPacksPackageType/ns0:Code'" />
      <xsl:with-param name="Data" select="$PackType" />
      <xsl:with-param name="Max"  select="3" />
    </xsl:call-template>
    
    
    <!-- Weight: -->
    
    <xsl:variable name="Weight" select="$House/*[local-name()='GrossWeightKG']" />
    
    <xsl:if test="($Weight != '')">
      
      <xsl:call-template name="MapValueIfNotEmpty">
        <xsl:with-param name="Node" select="'ns0:TotalWeight'" />
        <xsl:with-param name="Data" select="$Weight" />
      </xsl:call-template>
      
      <ns0:TotalWeightUnit>
        <ns0:Code>
          <xsl:text>KG</xsl:text>
        </ns0:Code>
      </ns0:TotalWeightUnit>
      
    </xsl:if>
    
    
    <!-- Actual Chargeable: -->
    
    <xsl:call-template name="MapValueIfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:ActualChargeable'" />
      <xsl:with-param name="Data" select="$House/*[local-name()='ChargeableWeightKG']" />
    </xsl:call-template>
    
    
    <!-- Total Volume: -->
    
    <xsl:variable name="Volume" select="$House/*[local-name()='CubicMeters']" />
    
    <xsl:if test="($Volume != '')">
      
      <xsl:call-template name="MapValueIfNotEmpty">
        <xsl:with-param name="Node" select="'ns0:TotalVolume'" />
        <xsl:with-param name="Data" select="$Volume" />
      </xsl:call-template>
      
      <ns0:TotalVolumeUnit>
        <ns0:Code>
          <xsl:text>M3</xsl:text>
        </ns0:Code>
      </ns0:TotalVolumeUnit>
      
    </xsl:if>
    
    
    <!-- No of Containers: -->
    
    <xsl:call-template name="Map_Integer_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:ContainerCount'" />
      <xsl:with-param name="Data" select="$House/*[local-name()='NoOfContainers']" />
    </xsl:call-template>
    
    
    <!-- ContainerMode: -->
    
    <xsl:variable name="ContainerMode" select="ScriptNS0:GetRecipientCode( $Sender, $Recipient, $TS_Name, 
                                                'Container Mode', 'CW1 Container Mode', 
                                                $House/*[local-name()='ContainerTypeCode'] )" />
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:ContainerMode/ns0:Code'" />
      <xsl:with-param name="Data" select="$ContainerMode" />
      <xsl:with-param name="Max"  select="3" />
    </xsl:call-template>
    
    
    
    <!-- ToDo:
    <x "FreightAmountDetails"  type="FreightAmountDetailsType" />
    -->
    
    
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:GoodsDescription'" />
      <xsl:with-param name="Data" select="$House/*[local-name()='NatureOfGoods']" />
      <xsl:with-param name="Max"  select="128" />
    </xsl:call-template>
    
    
    <!-- Port of Loading: -->
    
    <xsl:variable name="Port_Loading">
      <xsl:call-template name="Determine_Port">
        <xsl:with-param name="Country" select="$House/*[local-name()='CountryOfLoading']" />
        <xsl:with-param name="Port"    select="$House/*[local-name()='PortOfLoading']" />
      </xsl:call-template>
    </xsl:variable>
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:PortOfLoading/ns0:Code'" />
      <xsl:with-param name="Data" select="$Port_Loading" />
      <xsl:with-param name="Max"  select="5" />
    </xsl:call-template>
    
    
    <!-- Port of Destination: -->
    
    <xsl:variable name="Port_Destination">
      <xsl:call-template name="Determine_Port">
        <xsl:with-param name="Country" select="$House/*[local-name()='FinalDestCountry']" />
        <xsl:with-param name="Port"    select="$House/*[local-name()='FinalDestPort']" />
      </xsl:call-template>
    </xsl:variable>
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:PortOfDestination/ns0:Code'" />
      <xsl:with-param name="Data" select="$Port_Destination" />
      <xsl:with-param name="Max"  select="5" />
    </xsl:call-template>
    
    
    <ns0:NoteCollection>
      
      <xsl:call-template name="Create_Note_from_Array">
        <xsl:with-param name="Desc"         select="'Goods Handling Instructions'" />
        <xsl:with-param name="ArrStr"       select="$House/*[local-name()='HandlingInstructions']" />
        <xsl:with-param name="IsCustomNote" select="false()" />
      </xsl:call-template>
      
      <xsl:call-template name="Create_Note_from_Array">
        <xsl:with-param name="Desc"         select="'Marks &amp; Numbers'" />
        <xsl:with-param name="ArrStr"       select="$House/*[local-name()='MarksAndNumbers']" />
        <xsl:with-param name="IsCustomNote" select="false()" />
      </xsl:call-template>
      
      <xsl:call-template name="Create_Note_from_Array">
        <xsl:with-param name="Desc"   select="'Endorsements'" />
        <xsl:with-param name="ArrStr" select="$House/*[local-name()='Endorsements']" />
      </xsl:call-template>
      
      <xsl:call-template name="Create_Note_from_Array">
        <xsl:with-param name="Desc"   select="'HazardousDescription'" />
        <xsl:with-param name="ArrStr" select="$House/*[local-name()='HazardousDescription']" />
      </xsl:call-template>
      
      <xsl:call-template name="Create_Note_from_Array">
        <xsl:with-param name="Desc"   select="'AccountingInfo'" />
        <xsl:with-param name="ArrStr" select="$House/*[local-name()='AccountingInfo']" />
      </xsl:call-template>
      
      <xsl:call-template name="Create_Note_from_Array">
        <xsl:with-param name="Desc"   select="'ExternalOverseasAgentCode'" />
        <xsl:with-param name="ArrStr" select="$House/*[local-name()='ExternalOverseasAgentCode']" />
      </xsl:call-template>
      
      <xsl:call-template name="Create_Note_from_Array">
        <xsl:with-param name="Desc"   select="'PrepaidCollect'" />
        <xsl:with-param name="ArrStr" select="$House/*[local-name()='PrepaidCollect']" />
      </xsl:call-template>
      
      <xsl:call-template name="Create_Note_for_External_References">
        <xsl:with-param name="Desc"       select="'ExternalReferences'" />
        <xsl:with-param name="ParentNode" select="$House" />
        <xsl:with-param name="Separator"  select="$NewLine" />
      </xsl:call-template>
      
      <xsl:call-template name="Create_Note_for_TransshipDetails">
        <xsl:with-param name="Desc"       select="'TransshipDetails'" />
        <xsl:with-param name="ParentNode" select="$House/*[local-name()='TransshipDetails']" />
        <xsl:with-param name="Separator"  select="$NewLine" />
      </xsl:call-template>
      
    </ns0:NoteCollection>
    
    
    <ns0:PackingLineCollection>
      
      <!-- PackageDimensions: -->
      
      <xsl:variable name="ArrPackage" select="$House/*[local-name()='PackageDimensions']" />
      
      <xsl:for-each select="$ArrPackage">
        <ns0:PackingLine>
          
          <xsl:call-template name="Map_Package_from_Dimensions">
            <xsl:with-param name="Package" select="." />
          </xsl:call-template>
          
        </ns0:PackingLine>
      </xsl:for-each>
      
      
      <!-- PackageDetails: -->
      
      <xsl:variable name="ArrPackDetail" select="$House/*[local-name()='PackageDetails']" />
      
      <xsl:for-each select="$ArrPackDetail">
        <ns0:PackingLine>
          
          <xsl:call-template name="Map_Package_from_Detail">
            <xsl:with-param name="Package" select="." />
          </xsl:call-template>
          
        </ns0:PackingLine>
      </xsl:for-each>
      
    </ns0:PackingLineCollection>
    
    
    <!-- Container Collection: -->
    
    <ns0:ContainerCollection>
      <xsl:variable name="ArrContainer" select="$House/*[local-name()='Container']" />
      
      <xsl:for-each select="$ArrContainer">
        <ns0:Container>
          
          <xsl:call-template name="Map_Container">
            <xsl:with-param name="Container" select="." />
          </xsl:call-template>
          
        </ns0:Container>
      </xsl:for-each>
      
    </ns0:ContainerCollection>
    
    
    
    <!-- ToDo:
    <x "SundryCharge"          type="SundryChargeType"       maxOccurs="unbounded" />
    -->
    
    
    
    <!-- ToDo:
    <x "AWBLineDetails"        type="AWBLineDetailsType"     maxOccurs="unbounded" />
    -->
    
    
  </xsl:template>  <!-- Template: "Map_SubShipment" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="MapCustomizedField">
    <xsl:param name="Key" />
    <xsl:param name="DataType" select="'String'" />
    <xsl:param name="Value" />
    
    <xsl:if test="($Value != '')">
      <ns0:CustomizedField>
        
        <ns0:Key>
          <xsl:value-of select="$Key" />
        </ns0:Key>
        
        <ns0:DataType>
          <xsl:value-of select="$DataType" />
        </ns0:DataType>
        
        <ns0:Value>
          <xsl:value-of select="$Value" />
        </ns0:Value>
        
      </ns0:CustomizedField>
    </xsl:if>
  </xsl:template>  <!-- Template: "MapCustomizedField" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Determine_TransportMode">
    <xsl:param name="DocTxMode" />
    
    <xsl:variable name="Db_TxMode" select="ScriptNS0:GetRecipientCode( $Sender, $Recipient, $TS_Name, 
                                           'Transport Modes', 'CW1 Transport Mode', $DocTxMode )" />
    
    <xsl:choose>
      <xsl:when test="($Db_TxMode != '')">
        <xsl:value-of select="$Db_TxMode" />
      </xsl:when>
      
      <xsl:otherwise>
        <xsl:value-of select="ScriptNS0:GetRecipientCodeUnkeyed( $Sender, $Recipient, $TS_Name,
                                                                 'Defaults', 'Transport Mode')" />
      </xsl:otherwise>
    </xsl:choose>
    
  </xsl:template>  <!-- Template: "Determine_TransportMode" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Determine_Port">
    <xsl:param name="Country" />
    <xsl:param name="Port" />
    
    <xsl:choose>
      
      <xsl:when test="((string-length( $Country ) = 2) and (string-length( $Port ) = 3))">
        <xsl:value-of select="concat( $Country, $Port )" />
      </xsl:when>
      
      <xsl:otherwise>
        <xsl:value-of select="$Port" />
      </xsl:otherwise>
      
    </xsl:choose>
    
  </xsl:template>  <!-- Template: "Determine_Port" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Map_TransportLegs">
    <xsl:param name="Voyage_01" />
    <xsl:param name="Voyage_02" />
    <xsl:param name="Voyage_03" />
    <xsl:param name="Carrier_to_Use" />
    
    <ns0:TransportLegCollection>
      
      <xsl:if test="($Voyage_01 != '') or ($Carrier_to_Use != '')">
        <xsl:call-template name="Map_one_TransportLeg">
          <xsl:with-param name="Seq_No"         select="1" />
          <xsl:with-param name="Voyage"         select="$Voyage_01" />
          <xsl:with-param name="Carrier_to_Use" select="$Carrier_to_Use" />
        </xsl:call-template>
      </xsl:if>
      
      <xsl:if test="($Voyage_02 != '')">
        <xsl:call-template name="Map_one_TransportLeg">
          <xsl:with-param name="Seq_No"         select="2" />
          <xsl:with-param name="Voyage"         select="$Voyage_02" />
          <xsl:with-param name="Carrier_to_Use" select="$Carrier_to_Use" />
        </xsl:call-template>
      </xsl:if>
      
      <xsl:if test="($Voyage_03 != '')">
        <xsl:call-template name="Map_one_TransportLeg">
          <xsl:with-param name="Seq_No"         select="3" />
          <xsl:with-param name="Voyage"         select="$Voyage_03" />
          <xsl:with-param name="Carrier_to_Use" select="$Carrier_to_Use" />
        </xsl:call-template>
      </xsl:if>
      
    </ns0:TransportLegCollection>
    
  </xsl:template>  <!-- Template: "Map_TransportLegs" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Map_one_TransportLeg">
    <xsl:param name="Seq_No" />
    <xsl:param name="Voyage" />
    <xsl:param name="Carrier_to_Use" />
    
    <ns0:TransportLeg>
      
      <!-- LegOrder: -->
      
      <ns0:LegOrder>
        <xsl:value-of select="$Seq_No" />
      </ns0:LegOrder>
      
      
      <!-- VoyageFlightNo: -->
      
      <xsl:call-template name="Map_String_IfNotEmpty">
        <xsl:with-param name="Node" select="'ns0:VoyageFlightNo'" />
        <xsl:with-param name="Data" select="$Voyage" />
        <xsl:with-param name="Max"  select="10" />
      </xsl:call-template>
      
      
      <!-- Carrier: -->
      
      <xsl:if test="($Carrier_to_Use != '')">
        <ns0:Carrier>
          
          <xsl:choose>
            <xsl:when test="($Carrier_to_Use = $MASTER_CARRIER)">
              
              <xsl:call-template name="Map_Org_Fields">
                <xsl:with-param name="OrgType"     select="'Carrier'" />
                <xsl:with-param name="OrgName"     select="$Master/*[local-name()='MasterCarrierName']" />
                <xsl:with-param name="ShortCode"   select="$Master/*[local-name()='MasterCarrierCode']" />
              </xsl:call-template>
              
            </xsl:when>
            
            <xsl:when test="($Carrier_to_Use = $NORMAL_CARRIER)">
              
              <xsl:call-template name="Map_Org_Fields">
                <xsl:with-param name="OrgType"     select="'Carrier'" />
                <xsl:with-param name="OrgName"     select="$Master/*[local-name()='CarrierName']" />
                <xsl:with-param name="Addr1"       select="$Master/*[local-name()='CarrierAddr1']" />
                <xsl:with-param name="Addr2"       select="$Master/*[local-name()='CarrierAddr2']" />
                <xsl:with-param name="Addr3"       select="$Master/*[local-name()='CarrierAddr3']" />
                <xsl:with-param name="Addr4"       select="$Master/*[local-name()='CarrierAddr4']" />
                <xsl:with-param name="AddrCode"    select="$Master/*[local-name()='CarrierAddrCode']" />
                <xsl:with-param name="CountryCode" select="$Master/*[local-name()='CarrierCountryCode']" />
                <xsl:with-param name="ShortCode"   select="$Master/*[local-name()='CarrierCode']" />
              </xsl:call-template>
              
            </xsl:when>
          </xsl:choose>
          
        </ns0:Carrier>
      </xsl:if>
      
      
      <!-- TransportMode: -->
      
      <xsl:if test="($Carrier_to_Use = $MASTER_CARRIER)">
        <xsl:variable name="MasterTxMode" select="$Master/*[local-name()='MasterCarrierTransportMethod']" />
        
        <xsl:if test="($MasterTxMode != '')">
          <xsl:variable name="CW_TxMode" select="ScriptNS0:GetRecipientCode( $Sender, $Recipient, $TS_Name, 
                                                   'Transport Modes', 'CW1 Transport Mode', $MasterTxMode )" />
          
          <xsl:if test="($CW_TxMode != '')">
            <xsl:variable name="TM_Desc" select="ScriptNS0:GetRecipientCode('eHub', 'eHub', 'Common Code Mappings', 
                                                   'Transport Mode', 'Description', $CW_TxMode )" />
            
            <xsl:call-template name="MapValueIfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:TransportMode'" />
              <xsl:with-param name="Data" select="$TM_Desc" />
            </xsl:call-template>
            
          </xsl:if>
        </xsl:if>
      </xsl:if>
      
    </ns0:TransportLeg>
    
  </xsl:template>  <!-- Template: "Map_one_TransportLeg" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Map_Org">
    
    <xsl:param name="OrgType"     />
    <xsl:param name="OrgName"     />
    <xsl:param name="Addr1"       />
    <xsl:param name="Addr2"       />
    <xsl:param name="Addr3"       />
    <xsl:param name="Addr4"       />
    <xsl:param name="AddrCode"    />
    <xsl:param name="CountryCode" />
    <xsl:param name="ShortCode"   />
    
    
    <xsl:if test=" ($OrgName   != '')
                or ($Addr1     != '')
                or ($AddrCode  != '')
                or ($ShortCode != '') ">
      
      <ns0:OrganizationAddress>
        
        <xsl:call-template name="Map_Org_Fields">
          <xsl:with-param name="OrgType"     select="$OrgType"     />
          <xsl:with-param name="OrgName"     select="$OrgName"     />
          <xsl:with-param name="Addr1"       select="$Addr1"       />
          <xsl:with-param name="Addr2"       select="$Addr2"       />
          <xsl:with-param name="Addr3"       select="$Addr3"       />
          <xsl:with-param name="Addr4"       select="$Addr4"       />
          <xsl:with-param name="AddrCode"    select="$AddrCode"    />
          <xsl:with-param name="CountryCode" select="$CountryCode" />
          <xsl:with-param name="ShortCode"   select="$ShortCode"   />
        </xsl:call-template>
        
      </ns0:OrganizationAddress>
    </xsl:if>
    
  </xsl:template>  <!-- Template: "Map_Org" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Map_Org_Fields">
    
    <xsl:param name="OrgType" />
    <xsl:param name="OrgName" />
    <xsl:param name="Addr1" />
    <xsl:param name="Addr2" />
    <xsl:param name="Addr3" />
    <xsl:param name="Addr4" />
    <xsl:param name="AddrCode" />
    <xsl:param name="CountryCode" />
    <xsl:param name="ShortCode" />
    
    
    <!-- AddressOverride: -->
    
    <xsl:call-template name="Map_Boolean_Always">
      <xsl:with-param name="Node" select="'ns0:AddressOverride'" />
      <xsl:with-param name="Data" select="''" />
      <xsl:with-param name="Else" select="'true'" />
    </xsl:call-template>
    
    
    <!-- AddressType: -->
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:AddressType'" />
      <xsl:with-param name="Data" select="$OrgType" />
      <xsl:with-param name="Max"  select="40" />
    </xsl:call-template>
    
    
    <!-- CompanyName: -->
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:CompanyName'" />
      <xsl:with-param name="Data" select="$OrgName" />
      <xsl:with-param name="Max"  select="100" />
    </xsl:call-template>
    
    
    <!-- Address Strings: -->
    
    <xsl:variable name="AddressStr" select="normalize-space( concat( $Addr1, ' ',
                                                                     $Addr2, ' ',
                                                                     $Addr3, ' ',
                                                                     $Addr4 ))" />
    
    <xsl:variable name="Part_01" select="substring( $AddressStr,  1, 50 )" />
    <xsl:variable name="Part_02" select="substring( $AddressStr, 51, 50 )" />
    
    
    <!-- Address1: -->
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:Address1'" />
      <xsl:with-param name="Data" select="$Part_01" />
      <xsl:with-param name="Max"  select="50" />
    </xsl:call-template>
    
    
    <!-- Address2: -->
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:Address2'" />
      <xsl:with-param name="Data" select="$Part_02" />
      <xsl:with-param name="Max"  select="50" />
    </xsl:call-template>
    
    
    <!-- Address Short Code: -->
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:AddressShortCode'" />
      <xsl:with-param name="Data" select="$AddrCode" />
      <xsl:with-param name="Max"  select="25" />
    </xsl:call-template>
    
    
    <!-- Organization Code: -->
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:OrganizationCode'" />
      <xsl:with-param name="Data" select="$ShortCode" />
      <xsl:with-param name="Max"  select="12" />
    </xsl:call-template>
    
    
    <!-- Country Code: -->
    
    <xsl:variable name="TheCountryCode" select="normalize-space( $CountryCode )" />
    
    <xsl:if test="(string-length( $TheCountryCode ) = 2)">
      <xsl:call-template name="MapValueIfNotEmpty">
        <xsl:with-param name="Node" select="'ns0:Country/ns0:Code'" />
        <xsl:with-param name="Data" select="$TheCountryCode" />
      </xsl:call-template>
    </xsl:if>
    
  </xsl:template>  <!-- Template: "Map_Org_Fields" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Map_Additional_Ref_By_Label">
    <xsl:param name="Label" />
    <xsl:param name="Data" />
    
    <xsl:variable name="RefCode" select="ScriptNS0:GetRecipientCode( $Sender, $Recipient, $TS_Name, 
                                           'Additional Ref Codes', 'CW1 Add Ref Type Code', 
                                           $Label )" />
    
    <xsl:if test="($RefCode != '')">
      <xsl:call-template name="Map_Additional_Reference">
        <xsl:with-param name="Code" select="$RefCode" />
        <xsl:with-param name="Data" select="$Data" />
      </xsl:call-template>
    </xsl:if>
    
  </xsl:template>  <!-- Template: "Map_Additional_Ref_By_Label" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Map_Additional_Reference">
    <xsl:param name="Code" />
    <xsl:param name="Data" />
    
    <xsl:if test="($Code != '') and ($Data != '')">
      
      <ns0:AdditionalReference>
        
        <xsl:call-template name="MapValueIfNotEmpty">
          <xsl:with-param name="Node" select="'ns0:Type/ns0:Code'" />
          <xsl:with-param name="Data" select="$Code" />
        </xsl:call-template>
        
        <xsl:call-template name="Map_String_IfNotEmpty">
          <xsl:with-param name="Node" select="'ns0:ReferenceNumber'" />
          <xsl:with-param name="Data" select="$Data" />
          <xsl:with-param name="Max"  select="35" />
        </xsl:call-template>
        
      </ns0:AdditionalReference>
      
    </xsl:if>
    
  </xsl:template>  <!-- Template: "Map_Additional_Reference" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Determine_if_Imports">
    
    <xsl:variable name="Direction_in_Target_System">
      <xsl:call-template name="ToUpperCase">
        <xsl:with-param name="Data" select="normalize-space( $Master/*[local-name()='ShipmentTypeInTargetSystem'] )" />
      </xsl:call-template>
    </xsl:variable>
    
    <xsl:choose>
      <xsl:when test="($Direction_in_Target_System = 'EXPORTS')">
        <xsl:value-of select="false()" />
      </xsl:when>
      
      <xsl:otherwise>
        <xsl:value-of select="true()" />
      </xsl:otherwise>
    </xsl:choose>
    
  </xsl:template>  <!-- Template: "Determine_if_Imports" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Determine_AddressType_for_LocalAgent_Data">
    
    <xsl:choose>
      <xsl:when test="($IsImports)">
        <xsl:value-of select="'ReceivingForwarderAddress'" />
      </xsl:when>
      
      <xsl:otherwise>
        <xsl:value-of select="'SendingForwarderAddress'" />
      </xsl:otherwise>
    </xsl:choose>
    
  </xsl:template>  <!-- Template: "Determine_AddressType_for_LocalAgent_Data" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Determine_AddressType_for_OverseasAgent_Data">
    
    <xsl:choose>
      <xsl:when test="($IsImports)">
        <xsl:value-of select="'SendingForwarderAddress'" />
      </xsl:when>
      
      <xsl:otherwise>
        <xsl:value-of select="'ReceivingForwarderAddress'" />
      </xsl:otherwise>
    </xsl:choose>
    
  </xsl:template>  <!-- Template: "Determine_AddressType_for_OverseasAgent_Data" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Determine_AddressType_for_DegroupDepot">
    
    <xsl:choose>
      <xsl:when test="($IsImports)">
        <xsl:value-of select="'ArrivalCFSAddress'" />
      </xsl:when>
      
      <xsl:otherwise>
        <xsl:value-of select="'DepartureCFSAddress'" />
      </xsl:otherwise>
    </xsl:choose>
    
  </xsl:template>  <!-- Template: "Determine_AddressType_for_DegroupDepot" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Map_Container">
    <xsl:param name="Container" />
    
    <!-- ContainerMode: -->
    
    <xsl:variable name="ContainerMode" select="ScriptNS0:GetRecipientCode( $Sender, $Recipient, $TS_Name, 
                                                'Container Mode', 'CW1 Container Mode', 
                                                $Container/*[local-name()='ContainerShipmentType'] )" />
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:FCL_LCL_AIR/ns0:Code'" />
      <xsl:with-param name="Data" select="$ContainerMode" />
      <xsl:with-param name="Max"  select="3" />
    </xsl:call-template>
    
    
    <!-- ContainerNumber: -->
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:ContainerNumber'" />
      <xsl:with-param name="Data" select="$Container/*[local-name()='ContainerNo']" />
      <xsl:with-param name="Max"  select="20" />
    </xsl:call-template>
    
    
    <!-- ItemCount -->
    
    <xsl:call-template name="Map_Integer_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:ItemCount'" />
      <xsl:with-param name="Data" select="$Container/*[local-name()='NoOfPackages']" />
    </xsl:call-template>
    
    
    <!-- Weight: -->
    
    <xsl:variable name="GrossWeight" select="$Container/*[local-name()='GrossWeightKG']" />
    <xsl:variable name="NetWeight"   select="$Container/*[local-name()='NetWeightKG']" />
    
    <xsl:if test="($GrossWeight != '') or ($NetWeight != '')">
      
      <xsl:call-template name="MapValueIfNotEmpty">
        <xsl:with-param name="Node" select="'ns0:GrossWeight'" />
        <xsl:with-param name="Data" select="$GrossWeight" />
      </xsl:call-template>
      
      <xsl:call-template name="MapValueIfNotEmpty">
        <xsl:with-param name="Node" select="'ns0:GoodsWeight'" />
        <xsl:with-param name="Data" select="$NetWeight" />
      </xsl:call-template>
      
      <ns0:WeightUnit>
        <ns0:Code>
          <xsl:text>KG</xsl:text>
        </ns0:Code>
      </ns0:WeightUnit>
      
    </xsl:if>
    
    
    <!-- Seal: -->
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:Seal'" />
      <xsl:with-param name="Data" select="$Container/*[local-name()='ContainerSealNo']" />
      <xsl:with-param name="Max"  select="20" />
    </xsl:call-template>
    
    
    <!-- ContainerType: -->
    
    <xsl:variable name="ContainerTypeCode" select="ScriptNS0:GetRecipientCode( $Sender, $Recipient, $TS_Name, 
                                                    'Container Type', 'CW1 Container Type Code',
                                                    $Container/*[local-name()='ContainerSize'] )" />
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:ContainerType/ns0:Code'" />
      <xsl:with-param name="Data" select="$ContainerTypeCode" />
      <xsl:with-param name="Max"  select="10" />
    </xsl:call-template>
    
    
    <!-- PalletCount: -->
    
    <xsl:call-template name="Map_Integer_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:PalletCount'" />
      <xsl:with-param name="Data" select="$Container/*[local-name()='NoOfPallets']" />
    </xsl:call-template>
    
    
    <ns0:AddInfoCollection>
      
      <xsl:call-template name="Map_Container_AddInfo">
        <xsl:with-param name="ParentNode" select="$Container" />
      </xsl:call-template>
      
    </ns0:AddInfoCollection>
    
  </xsl:template>  <!-- Template: "Map_Container" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Map_DateCollection_Date">
    <xsl:param name="Type" />
    <xsl:param name="IsEstimate" />
    <xsl:param name="Value" />
    
    <xsl:variable name="DateValue">
      <xsl:if test="($Value != '')">
        <xsl:value-of select="ScriptNS1:ConvertToXmlDate( $Value, 'yyyy-MM-dd')" />
      </xsl:if>
    </xsl:variable>
    
    <xsl:if test="($DateValue != '')">
      <ns0:Date>
        
        <ns0:Type>
          <xsl:value-of select="$Type" />
        </ns0:Type>
        
        <ns0:IsEstimate>
          <xsl:value-of select="$IsEstimate" />
        </ns0:IsEstimate>
        
        <ns0:Value>
          <xsl:value-of select="$DateValue" />
        </ns0:Value>
        
      </ns0:Date>
    </xsl:if>
    
  </xsl:template>  <!-- Template: "Map_DateCollection_Date" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Create_Note_for_External_References">
    <xsl:param name="Desc" />
    <xsl:param name="ParentNode" />
    <xsl:param name="Separator" />
    
    <xsl:variable name="ArrStr" select="$ParentNode/*
                                         [ (local-name()='ExternalReference1')
                                        or (local-name()='ExternalReference2')
                                        or (local-name()='ExternalReference3')
                                        or (local-name()='ExternalReference4')
                                        or (local-name()='ExternalReference5')
                                        or (local-name()='ExternalReference6')
                                        or (local-name()='ExternalReference7')
                                        or (local-name()='ExternalReference8')
                                        or (local-name()='ExternalReference9')
                                        or (local-name()='ExternalReference10') ]" />
    
    <xsl:call-template name="Create_Note_from_Array">
      <xsl:with-param name="Desc"      select="$Desc" />
      <xsl:with-param name="ArrStr"    select="$ArrStr" />
      <xsl:with-param name="Separator" select="$Separator" />
    </xsl:call-template>
    
  </xsl:template>  <!-- Template: "Create_Note_for_External_References" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Create_Note_for_TransshipDetails">
    <xsl:param name="Desc" />
    <xsl:param name="ParentNode" />
    <xsl:param name="Separator" />
    
    <xsl:variable name="TheText">
      
      <!-- TransshipVessel: -->
      
      <xsl:variable name="Vessel" select="$ParentNode/*[local-name()='TransshipVessel']" />
      
      <xsl:if test="($Vessel != '')">
        <xsl:value-of select="'TransshipVessel = '" />
        <xsl:value-of select="$Vessel" />
      </xsl:if>
      
      
      <!-- TransshipVoyage: -->
      
      <xsl:variable name="Voyage" select="$ParentNode/*[local-name()='TransshipVoyage']" />
      
      <xsl:if test="($Voyage != '')">
        <xsl:value-of select="$Separator" />
        <xsl:value-of select="'TransshipVoyage = '" />
        <xsl:value-of select="$Voyage" />
      </xsl:if>
      
      
      <!-- TransshipPort: -->
      
      <xsl:variable name="Port" select="$ParentNode/*[local-name()='TransshipPort']" />
      
      <xsl:if test="($Port != '')">
        <xsl:value-of select="$Separator" />
        <xsl:value-of select="'TransshipPort = '" />
        <xsl:value-of select="$Port" />
      </xsl:if>
      
      
      <!-- TransshipDate: -->
      
      <xsl:variable name="Date" select="$ParentNode/*[local-name()='TransshipDate']" />
      
      <xsl:if test="($Date != '')">
        <xsl:value-of select="$Separator" />
        <xsl:value-of select="'TransshipDate = '" />
        <xsl:value-of select="$Date" />
      </xsl:if>
      
    </xsl:variable>
    
    <xsl:call-template name="Create_Note_from_Array">
      <xsl:with-param name="Desc"      select="$Desc" />
      <xsl:with-param name="ArrStr"    select="msxsl:node-set( $TheText )" />
      <xsl:with-param name="Separator" select="$Separator" />
    </xsl:call-template>
    
  </xsl:template>  <!-- Template: "Create_Note_for_TransshipDetails" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Create_Note_for_ContactInfo">
    <xsl:param name="Desc" />
    <xsl:param name="ParentNode" />
    <xsl:param name="Separator" />
    
    <xsl:variable name="TheText">
      
      <!-- ContactPersonFirstName: -->
      
      <xsl:variable name="FirstName" select="$ParentNode/*[local-name()='ContactPersonFirstName']" />
      
      <xsl:if test="($FirstName != '')">
        <xsl:value-of select="'ContactPersonFirstName = '" />
        <xsl:value-of select="$FirstName" />
      </xsl:if>
      
      
      <!-- ContactPersonSurname: -->
      
      <xsl:variable name="Surname" select="$ParentNode/*[local-name()='ContactPersonSurname']" />
      
      <xsl:if test="($Surname != '')">
        <xsl:value-of select="$Separator" />
        <xsl:value-of select="'ContactPersonSurname = '" />
        <xsl:value-of select="$Surname" />
      </xsl:if>
      
      
      <!-- TelephoneNo: -->
      
      <xsl:variable name="Tel" select="$ParentNode/*[local-name()='TelephoneNo']" />
      
      <xsl:if test="($Tel != '')">
        <xsl:value-of select="$Separator" />
        <xsl:value-of select="'TelephoneNo = '" />
        <xsl:value-of select="$Tel" />
      </xsl:if>
      
      
      <!-- EmailAddress: -->
      
      <xsl:variable name="Email" select="$ParentNode/*[local-name()='EmailAddress']" />
      
      <xsl:if test="($Email != '')">
        <xsl:value-of select="$Separator" />
        <xsl:value-of select="'EmailAddress = '" />
        <xsl:value-of select="$Email" />
      </xsl:if>
      
    </xsl:variable>
    
    <xsl:call-template name="Create_Note_from_Array">
      <xsl:with-param name="Desc"      select="$Desc" />
      <xsl:with-param name="ArrStr"    select="msxsl:node-set( $TheText )" />
      <xsl:with-param name="Separator" select="$Separator" />
    </xsl:call-template>
    
  </xsl:template>  <!-- Template: "Create_Note_for_ContactInfo" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Create_Note_from_Array">
    <xsl:param name="Desc" />
    <xsl:param name="ArrStr" />
    <xsl:param name="Separator"    select="' '" />
    <xsl:param name="IsCustomNote" select="true()" />
    
    <xsl:variable name="TheText">
      <xsl:call-template name="Reduce_Array_to_String">
        <xsl:with-param name="ArrStr"    select="$ArrStr" />
        <xsl:with-param name="Separator" select="$Separator" />
      </xsl:call-template>
    </xsl:variable>
    
    <xsl:if test="($TheText != '')">
      <ns0:Note>
        
        <ns0:Description>
          <xsl:value-of select="$Desc" />
        </ns0:Description>
        
        <ns0:IsCustomDescription>
          <xsl:value-of select="$IsCustomNote" />
        </ns0:IsCustomDescription>
        
        <ns0:NoteText>
          <xsl:value-of select="$TheText" />
        </ns0:NoteText>
        
      </ns0:Note>
    </xsl:if>
    
  </xsl:template>  <!-- Template: "Create_Note_from_Array" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Reduce_Array_to_String">
    <xsl:param name="ArrStr" />
    <xsl:param name="Separator" />
    
    <xsl:for-each select="$ArrStr">
      <xsl:if test="(position() != 1)">
        <xsl:value-of select="$Separator" />
      </xsl:if>
      <xsl:value-of select="." />
    </xsl:for-each>
    
  </xsl:template>  <!-- Template: "Reduce_Array_to_String" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Map_Package_from_Dimensions">
    <xsl:param name="Package" />
    
    <xsl:call-template name="MapValueIfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:Length'" />
      <xsl:with-param name="Data" select="$Package/*[local-name()='Length_cm']" />
    </xsl:call-template>
    
    <xsl:call-template name="MapValueIfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:Height'" />
      <xsl:with-param name="Data" select="$Package/*[local-name()='Height_cm']" />
    </xsl:call-template>
    
    <xsl:call-template name="MapValueIfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:Width'" />
      <xsl:with-param name="Data" select="$Package/*[local-name()='Depth_cm']" />
    </xsl:call-template>
    
    <ns0:LengthUnit>
      <ns0:Code>
        <xsl:text>CM</xsl:text>
      </ns0:Code>
    </ns0:LengthUnit>
    
    <xsl:call-template name="Map_Integer_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:PackQty'" />
      <xsl:with-param name="Data" select="$Package/*[local-name()='Quantity']" />
    </xsl:call-template>
    
    <xsl:variable name="PackType" select="ScriptNS0:GetRecipientCode( $Sender, $Recipient, $TS_Name, 
                                    'Pack Types', 'CW1 Pack Type Code', $Package/*[local-name()='PackTypeCode'] )" />
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:PackType/ns0:Code'" />
      <xsl:with-param name="Data" select="$PackType" />
      <xsl:with-param name="Max"  select="3" />
    </xsl:call-template>
    
  </xsl:template>  <!-- Template: "Map_Package_from_Dimensions" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Map_Package_from_Detail">
    <xsl:param name="Package" />
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:GoodsDescription'" />
      <xsl:with-param name="Data" select="$Package/*[local-name()='Description']" />
      <xsl:with-param name="Max"  select="65" />
    </xsl:call-template>
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:ContainerNumber'" />
      <xsl:with-param name="Data" select="$Package/*[local-name()='ContainerNo']" />
      <xsl:with-param name="Max"  select="20" />
    </xsl:call-template>
    
    <xsl:call-template name="Map_Integer_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:PackQty'" />
      <xsl:with-param name="Data" select="$Package/*[local-name()='Quantity']" />
    </xsl:call-template>
    
    
    <!-- Volume: -->
    
    <xsl:variable name="Volume" select="$Package/*[local-name()='VolumeCM']" />
    
    <xsl:if test="($Volume != '')">
      
      <xsl:call-template name="MapValueIfNotEmpty">
        <xsl:with-param name="Node" select="'ns0:Volume'" />
        <xsl:with-param name="Data" select="$Volume" />
      </xsl:call-template>
      
      <ns0:VolumeUnit>
        <ns0:Code>
          <xsl:text>M3</xsl:text>
        </ns0:Code>
      </ns0:VolumeUnit>
      
    </xsl:if>
    
    
    <!-- Weight: -->
    
    <xsl:variable name="Weight" select="$Package/*[local-name()='WeightKG']" />
    
    <xsl:if test="($Weight != '')">
      
      <xsl:call-template name="MapValueIfNotEmpty">
        <xsl:with-param name="Node" select="'ns0:Weight'" />
        <xsl:with-param name="Data" select="$Weight" />
      </xsl:call-template>
      
      <ns0:WeightUnit>
        <ns0:Code>
          <xsl:text>KG</xsl:text>
        </ns0:Code>
      </ns0:WeightUnit>
      
    </xsl:if>
    
    
    <!-- Customized Fields: -->
    
    <ns0:CustomizedFieldCollection>
      
      <xsl:call-template name="MapCustomizedField">
        <xsl:with-param name="Key"   select="'Countable Quantity Code'" />
        <xsl:with-param name="Value" select="normalize-space( $Package/*[local-name()='CountableQuantityCode'] )" />
      </xsl:call-template>
      
    </ns0:CustomizedFieldCollection>
    
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:HarmonisedCode'" />
      <xsl:with-param name="Data" select="$Package/*[local-name()='TariffCode']" />
      <xsl:with-param name="Max"  select="15" />
    </xsl:call-template>
    
    
    <!-- Dangerous Goods: -->
    
    <xsl:variable name="DgCode">
      <xsl:call-template name="Determine_Dangerous_Goods_Code">
        <xsl:with-param name="ParentNode" select="$Package" />
      </xsl:call-template>
    </xsl:variable>
    
    <xsl:if test="($DgCode != '')">
      <ns0:UNDGCollection>
        <ns0:UNDG>
          
          <xsl:call-template name="Map_Package_UNDG_Info">
            <xsl:with-param name="DgCode"     select="$DgCode" />
            <xsl:with-param name="ParentNode" select="$Package" />
          </xsl:call-template>
          
        </ns0:UNDG>
      </ns0:UNDGCollection>
    </xsl:if>
    
  </xsl:template>  <!-- Template: "Map_Package_from_Detail" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Determine_Dangerous_Goods_Code">
    <xsl:param name="ParentNode" />
    
    <xsl:variable name="UNDGCode"   select="$ParentNode/*[local-name()='UNDGCode']" />
    <xsl:variable name="UNDGNumber" select="$ParentNode/*[local-name()='UNDGNumber']" />
    
    <xsl:choose>
      
      <xsl:when test="($UNDGCode != '')">
        <xsl:value-of select="$UNDGCode" />
      </xsl:when>
      
      <xsl:when test="($UNDGNumber != '')">
        <xsl:value-of select="$UNDGNumber" />
      </xsl:when>
      
      <xsl:otherwise>
        <xsl:value-of select="$ParentNode/*[local-name()='HazardCode']" />
      </xsl:otherwise>
      
    </xsl:choose>
    
  </xsl:template>  <!-- Template: "Determine_Dangerous_Goods_Code" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Map_Package_UNDG_Info">
    <xsl:param name="DgCode" />
    <xsl:param name="ParentNode" />
    
    <!-- UNDG Code: -->
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:UNDGCode'" />
      <xsl:with-param name="Data" select="$DgCode" />
      <xsl:with-param name="Max"  select="5" />
    </xsl:call-template>
    
    
    <!-- Hazard Desc: -->
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:ProperShippingName'" />
      <xsl:with-param name="Data" select="$ParentNode/*[local-name()='HazardDesc']" />
      <xsl:with-param name="Max"  select="100" />
    </xsl:call-template>
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:TechicalName'" />
      <xsl:with-param name="Data" select="$ParentNode/*[local-name()='HazardDesc']" />
      <xsl:with-param name="Max"  select="100" />
    </xsl:call-template>
    
    
    <!-- Flash Point: -->
    
    <xsl:variable name="FlashInfo">
      <xsl:value-of select="$ParentNode/*[local-name()='UNDGFlashPoint']" />
      <xsl:value-of select="$ParentNode/*[local-name()='UNDGFlashPointUnit']" />
    </xsl:variable>
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:FlashPoint'" />
      <xsl:with-param name="Data" select="$FlashInfo" />
      <xsl:with-param name="Max"  select="25" />
    </xsl:call-template>
    
    
    <!-- Packing Group: -->
    
    <xsl:call-template name="Map_String_IfNotEmpty">
      <xsl:with-param name="Node" select="'ns0:PackingGroup'" />
      <xsl:with-param name="Data" select="$ParentNode/*[local-name()='UNDGPackingGroup']" />
      <xsl:with-param name="Max"  select="3" />
    </xsl:call-template>
    
    
    <!-- Note Collection: -->
    
    <ns0:NoteCollection>
      
      <xsl:call-template name="Create_Note_for_UNDG_Info">
        <xsl:with-param name="Desc"       select="'Dangerous_Goods_Information'" />
        <xsl:with-param name="ParentNode" select="$ParentNode" />
        <xsl:with-param name="Separator"  select="$NewLine" />
      </xsl:call-template>
      
    </ns0:NoteCollection>
    
  </xsl:template>  <!-- Template: "Map_Package_UNDG_Info" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Create_Note_for_UNDG_Info">
    <xsl:param name="Desc" />
    <xsl:param name="ParentNode" />
    <xsl:param name="Separator" />
    
    <xsl:variable name="TheText">
      
      <!-- UNDGNumber: -->
      
      <xsl:variable name="DgNo" select="$ParentNode/*[local-name()='UNDGNumber']" />
      
      <xsl:if test="($DgNo != '')">
        <xsl:value-of select="'UNDGNumber = '" />
        <xsl:value-of select="$DgNo" />
      </xsl:if>
      
      
      <!-- UNDGCode: -->
      
      <xsl:variable name="DgCode" select="$ParentNode/*[local-name()='UNDGCode']" />
      
      <xsl:if test="($DgCode != '')">
        <xsl:value-of select="$Separator" />
        <xsl:value-of select="'UNDGCode = '" />
        <xsl:value-of select="$DgCode" />
      </xsl:if>
      
      
      <!-- IMOPageNo: -->
      
      <xsl:variable name="PageNo" select="$ParentNode/*[local-name()='IMOPageNo']" />
      
      <xsl:if test="($PageNo != '')">
        <xsl:value-of select="$Separator" />
        <xsl:value-of select="'IMOPageNo = '" />
        <xsl:value-of select="$PageNo" />
      </xsl:if>
      
      
      <!-- HazardCodeVerNo: -->
      
      <xsl:variable name="VerNo" select="$ParentNode/*[local-name()='HazardCodeVerNo']" />
      
      <xsl:if test="($VerNo != '')">
        <xsl:value-of select="$Separator" />
        <xsl:value-of select="'HazardCodeVerNo = '" />
        <xsl:value-of select="$VerNo" />
      </xsl:if>
      
      
      <!-- UNDGFlashPoint: -->
      
      <xsl:variable name="Point" select="$ParentNode/*[local-name()='UNDGFlashPoint']" />
      
      <xsl:if test="($Point != '')">
        <xsl:value-of select="$Separator" />
        <xsl:value-of select="'UNDGFlashPoint = '" />
        <xsl:value-of select="$Point" />
      </xsl:if>
      
      
      <!-- UNDGFlashPointUnit: -->
      
      <xsl:variable name="PointUnit" select="$ParentNode/*[local-name()='UNDGFlashPointUnit']" />
      
      <xsl:if test="($PointUnit != '')">
        <xsl:value-of select="$Separator" />
        <xsl:value-of select="'UNDGFlashPointUnit = '" />
        <xsl:value-of select="$PointUnit" />
      </xsl:if>
      
      
      <!-- UNDGPackingGroup: -->
      
      <xsl:variable name="Group" select="$ParentNode/*[local-name()='UNDGPackingGroup']" />
      
      <xsl:if test="($Group != '')">
        <xsl:value-of select="$Separator" />
        <xsl:value-of select="'UNDGPackingGroup = '" />
        <xsl:value-of select="$Group" />
      </xsl:if>
      
      
      <!-- UNDGEMSNumber: -->
      
      <xsl:variable name="EmsNo" select="$ParentNode/*[local-name()='UNDGEMSNumber']" />
      
      <xsl:if test="($EmsNo != '')">
        <xsl:value-of select="$Separator" />
        <xsl:value-of select="'UNDGEMSNumber = '" />
        <xsl:value-of select="$EmsNo" />
      </xsl:if>
      
      
      <!-- UNDGMedGuide: -->
      
      <xsl:variable name="Guide" select="$ParentNode/*[local-name()='UNDGMedGuide']" />
      
      <xsl:if test="($Guide != '')">
        <xsl:value-of select="$Separator" />
        <xsl:value-of select="'UNDGMedGuide = '" />
        <xsl:value-of select="$Guide" />
      </xsl:if>
      
      
      <!-- UNDGTremCardNo: -->
      
      <xsl:variable name="CardNo" select="$ParentNode/*[local-name()='UNDGTremCardNo']" />
      
      <xsl:if test="($CardNo != '')">
        <xsl:value-of select="$Separator" />
        <xsl:value-of select="'UNDGTremCardNo = '" />
        <xsl:value-of select="$CardNo" />
      </xsl:if>
      
      
      <!-- HazardCode: -->
      
      <xsl:variable name="HzCode" select="$ParentNode/*[local-name()='HazardCode']" />
      
      <xsl:if test="($HzCode != '')">
        <xsl:value-of select="$Separator" />
        <xsl:value-of select="'HazardCode = '" />
        <xsl:value-of select="$HzCode" />
      </xsl:if>
      
      
      <!-- HazardDesc: -->
      
      <xsl:variable name="HzDesc" select="$ParentNode/*[local-name()='HazardDesc']" />
      
      <xsl:if test="($HzDesc != '')">
        <xsl:value-of select="$Separator" />
        <xsl:value-of select="'HazardDesc = '" />
        <xsl:value-of select="$HzDesc" />
      </xsl:if>
      
    </xsl:variable>
    
    <xsl:call-template name="Create_Note_from_Array">
      <xsl:with-param name="Desc"      select="$Desc" />
      <xsl:with-param name="ArrStr"    select="msxsl:node-set( $TheText )" />
      <xsl:with-param name="Separator" select="$Separator" />
    </xsl:call-template>
    
  </xsl:template>  <!-- Template: "Create_Note_for_UNDG_Info" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Determine_Carrier_to_Use">
    
    <xsl:choose>
      <xsl:when test=" ($Master/*[local-name()='MasterCarrierCode'] != '')
                    or ($Master/*[local-name()='MasterCarrierName'] != '') ">
        
        <xsl:value-of select="$MASTER_CARRIER" />
      </xsl:when>
      
      <xsl:when test=" ($Master/*[local-name()='CarrierCode']     != '')
                    or ($Master/*[local-name()='CarrierName']     != '')
                    or ($Master/*[local-name()='CarrierAddr1']    != '')
                    or ($Master/*[local-name()='CarrierAddrCode'] != '') ">
        
        <xsl:value-of select="$NORMAL_CARRIER" />
      </xsl:when>
    </xsl:choose>
    
  </xsl:template>  <!-- Template: "Determine_Carrier_to_Use" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Create_Note_for_Carrier">
    <xsl:param name="Desc" />
    <xsl:param name="ParentNode" />
    <xsl:param name="Separator" />
    
    <xsl:variable name="TheText">
      <xsl:variable name="Col" select="$ParentNode/*
                                       [ (local-name()='CarrierCode')
                                      or (local-name()='CarrierName')
                                      or (local-name()='CarrierAddr1')
                                      or (local-name()='CarrierAddr2')
                                      or (local-name()='CarrierAddr3')
                                      or (local-name()='CarrierAddr4')
                                      or (local-name()='CarrierAddrCode')
                                      or (local-name()='CarrierCountryCode')
                                      or (local-name()='CarrierCountryName') ]" />
      
      <xsl:for-each select="$Col">
        <xsl:variable name="Node" select="." />
        
        <xsl:if test="($Node != '')">
          <xsl:value-of select="local-name( $Node )" />
          <xsl:value-of select="' = '" />
          <xsl:value-of select="$Node" />
          <xsl:value-of select="$Separator" />
        </xsl:if>
      </xsl:for-each>
      
    </xsl:variable>
    
    <xsl:call-template name="Create_Note_from_Array">
      <xsl:with-param name="Desc"      select="$Desc" />
      <xsl:with-param name="ArrStr"    select="msxsl:node-set( $TheText )" />
      <xsl:with-param name="Separator" select="$Separator" />
    </xsl:call-template>
    
  </xsl:template>  <!-- Template: "Create_Note_for_Carrier" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Create_Note_for_MasterCarrier">
    <xsl:param name="Desc" />
    <xsl:param name="ParentNode" />
    <xsl:param name="Separator" />
    
    <xsl:variable name="TheText">
      
      <!-- MasterCarrierCode: -->
      
      <xsl:variable name="Code" select="$ParentNode/*[local-name()='MasterCarrierCode']" />
      
      <xsl:if test="($Code != '')">
        <xsl:value-of select="'MasterCarrierCode = '" />
        <xsl:value-of select="$Code" />
      </xsl:if>
      
      
      <!-- MasterCarrierName: -->
      
      <xsl:variable name="TheName" select="$ParentNode/*[local-name()='MasterCarrierName']" />
      
      <xsl:if test="($TheName != '')">
        <xsl:value-of select="$Separator" />
        <xsl:value-of select="'MasterCarrierName = '" />
        <xsl:value-of select="$TheName" />
      </xsl:if>
      
      
      <!-- MasterCarrierTransportMethod: -->
      
      <xsl:variable name="TxMode" select="$ParentNode/*[local-name()='MasterCarrierTransportMethod']" />
      
      <xsl:if test="($TxMode != '')">
        <xsl:value-of select="$Separator" />
        <xsl:value-of select="'MasterCarrierTransportMethod = '" />
        <xsl:value-of select="$TxMode" />
      </xsl:if>
      
    </xsl:variable>
    
    <xsl:call-template name="Create_Note_from_Array">
      <xsl:with-param name="Desc"      select="$Desc" />
      <xsl:with-param name="ArrStr"    select="msxsl:node-set( $TheText )" />
      <xsl:with-param name="Separator" select="$Separator" />
    </xsl:call-template>
    
  </xsl:template>  <!-- Template: "Create_Note_for_MasterCarrier" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Map_Container_AddInfo">
    <xsl:param name="ParentNode" />
    
    <xsl:variable name="Arr" select="$ParentNode/*
                                     [ (local-name()='TerminalCode')
                                    or (local-name()='TerminalName')
                                    or (local-name()='DepotCode')
                                    or (local-name()='DepotName')
                                    or (local-name()='MASCode')
                                    or (local-name()='MASDescription')
                                    or (local-name()='MASType')
                                    or (local-name()='MASTypeDescription')
                                    or (local-name()='ReleasedToPartyCode')
                                    or (local-name()='ReleasedToPartyName')
                                    or (local-name()='AtmosphericControl')
                                    or (local-name()='ColdStoreLoadPoint')
                                    or (local-name()='LoadDateTime')
                                    or (local-name()='Product')
                                    or (local-name()='Vents')
                                    or (local-name()='Transporter')
                                    or (local-name()='BookingRefNo')
                                    or (local-name()='StorageApplicableDate')
                                    or (local-name()='TempCode')
                                    or (local-name()='UnpackID')
                                    or (local-name()='DeliveryDateTime')
                                    or (local-name()='NonISOContainer') ]
                                    [. != ''] " />
    
    <xsl:for-each select="$Arr">
      <ns0:AddInfo>
        
        <ns0:Key>
          <xsl:value-of select="local-name(.)" />
        </ns0:Key>
        
        <ns0:Value>
          <xsl:value-of select="." />
        </ns0:Value>
        
      </ns0:AddInfo>
    </xsl:for-each>
    
  </xsl:template>  <!-- Template: "Map_Container_AddInfo" -->
  
  
<!-- ======================================================================================================== -->
  
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="ToUpperCase">
    <xsl:param name="Data" />
    
    <xsl:value-of select="translate( $Data, $LowerChars, $UpperChars )" />
    
  </xsl:template>  <!-- Template: "ToUpperCase" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Map_Date_IfNotEmpty">
    <xsl:param name="Node" />
    <xsl:param name="Data" />
    
    <xsl:variable name="TheData">
      <xsl:if test="($Data != '')">
        <xsl:value-of select="ScriptNS1:ConvertToXmlDate( $Data, 'yyyy-MM-dd')" />
      </xsl:if>
    </xsl:variable>
    
    <xsl:if test="($TheData != '')">
      <xsl:call-template name="MapValueIfNotEmpty">
        <xsl:with-param name="Node" select="$Node" />
        <xsl:with-param name="Data" select="$TheData" />
      </xsl:call-template>
    </xsl:if>
    
  </xsl:template>  <!-- Template: "Map_Date_IfNotEmpty" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Map_String_IfNotEmpty">
    <xsl:param name="Node" />
    <xsl:param name="Data" />
    <xsl:param name="Max"  />
    
    
    <xsl:variable name="TheData" select="substring( substring( $Data, 
           string-length(
                          substring-before( $Data, 
                                            substring( normalize-space($Data), 1, 1 )
                                           )
                        ) +1
           ), 1, $Max )" />
    
    
    <xsl:if test="($TheData != '')">
      <xsl:call-template name="MapValueIfNotEmpty">
        <xsl:with-param name="Node" select="$Node" />
        <xsl:with-param name="Data" select="$TheData" />
      </xsl:call-template>
    </xsl:if>
    
  </xsl:template>  <!-- Template: "Map_String_IfNotEmpty" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Map_Integer_IfNotEmpty">
    <xsl:param name="Node" />
    <xsl:param name="Data" />
    
    <xsl:variable name="TheData">
      <xsl:if test="($Data != '')">
        <xsl:variable name="TmpNumber" select="number( $Data )" />
        
        <xsl:if test="($TmpNumber != '') and ($TmpNumber != 'NaN')">
          <xsl:value-of select="format-number( $TmpNumber,'#')" />
        </xsl:if>
      </xsl:if>
    </xsl:variable>
    
    <xsl:if test="($TheData != '')">
      <xsl:call-template name="MapValueIfNotEmpty">
        <xsl:with-param name="Node" select="$Node" />
        <xsl:with-param name="Data" select="$TheData" />
      </xsl:call-template>
    </xsl:if>
    
  </xsl:template>  <!-- Template: "Map_Integer_IfNotEmpty" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Map_Boolean_IfNotEmpty">
    <xsl:param name="Node" />
    <xsl:param name="Data" />
    
    <xsl:variable name="TheData" select="normalize-space( $Data )" />
    
    <xsl:if test="($TheData = 'false') or ($TheData = '0') 
               or ($TheData = 'true' ) or ($TheData = '1')" >
      
      <xsl:call-template name="MapValueIfNotEmpty">
        <xsl:with-param name="Node" select="$Node" />
        <xsl:with-param name="Data" select="$TheData" />
      </xsl:call-template>
      
    </xsl:if>
    
  </xsl:template>  <!-- Template: "Map_Boolean_IfNotEmpty" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Map_Boolean_Always">
    <xsl:param name="Node" />
    <xsl:param name="Data" />
    <xsl:param name="Else" />
    
    <xsl:variable name="TheData" select="normalize-space( $Data )" />
    
    <xsl:variable name="TheValue">
      <xsl:choose>
        <xsl:when test="($TheData = 'false') or ($TheData = '0') 
                     or ($TheData = 'true' ) or ($TheData = '1')" >
          <xsl:value-of select="$TheData" />
        </xsl:when>
        
        <xsl:otherwise>
          <xsl:value-of select="$Else" />
        </xsl:otherwise>
        
      </xsl:choose>
    </xsl:variable>
    
    <xsl:call-template name="MapValueIfNotEmpty">
      <xsl:with-param name="Node" select="$Node" />
      <xsl:with-param name="Data" select="$TheValue" />
    </xsl:call-template>
    
  </xsl:template>  <!-- Template: "Map_Boolean_Always" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="MapValueIfNotEmpty">
    <xsl:param name="Node" />
    <xsl:param name="Data" />
    
    <xsl:if test="($Data != '')">
      <xsl:choose>
        
        <xsl:when test="contains($Node, '/')">
          <xsl:element name="{substring-before($Node, '/')}">
            <xsl:call-template name="MapValueIfNotEmpty">
              <xsl:with-param name="Node" select="substring-after($Node, '/')" />
              <xsl:with-param name="Data" select="$Data" />
            </xsl:call-template>
          </xsl:element>
        </xsl:when>
        
        <xsl:otherwise>
          <xsl:element name="{$Node}">
            <xsl:value-of select="$Data" />
          </xsl:element>
        </xsl:otherwise>
        
      </xsl:choose>
    </xsl:if>
    
  </xsl:template>  <!-- Template: "MapValueIfNotEmpty" -->
  
  
<!-- ======================================================================================================== -->
  
  
</xsl:stylesheet>
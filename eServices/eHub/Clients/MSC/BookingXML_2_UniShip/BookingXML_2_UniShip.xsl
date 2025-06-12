<?xml version="1.0" encoding="UTF-16"?>

<xsl:stylesheet xmlns:xsl        = "http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl      = "urn:schemas-microsoft-com:xslt"
                xmlns:var        = "http://schemas.microsoft.com/BizTalk/2003/var"
                exclude-result-prefixes="msxsl var ScriptNS0" version="1.0"
                xmlns:ns0        = "http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:ScriptNS0  = "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0" >
  
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />
  
  <xsl:variable name="Sender"    select="'MSCCMBHST_XML'" />
  <xsl:variable name="Recipient" select="'MSCCMBHST'" />  <!-- "MAC Supply Chain Solutions" -->
  <xsl:variable name="TS_Name"   select="'Receive Shipment XML files as Forwarding Bookings'" />
  
  
  <xsl:template match="/">
    
    <xsl:variable name="Shipment" select="/*[local-name()='UniversalShipment']/*[local-name()='Shipment']" />
    
    
    <!-- Produce Output Message: -->
    
    <ns0:UniversalInterchange>
      
      <ns0:Header>
        <ns0:SenderID>
          <xsl:value-of select="$Sender" />
        </ns0:SenderID>
        
        <ns0:RecipientID>
          <xsl:value-of select="$Recipient" />
        </ns0:RecipientID>
      </ns0:Header>
      
      <ns0:Body>
        <ns0:UniversalShipment>
          <ns0:Shipment>
            
            
            <!-- Data Context:  -->
            
            <ns0:DataContext>
              
              <xsl:variable name="DataContext" select="$Shipment/*[local-name()='DataContext']" />
              
              <xsl:variable name="DataProvider">
                <xsl:call-template name="Apply_Order_of_Precedence">
                  <xsl:with-param name="DocValue" select="$DataContext/*[local-name()='DataProvider']" />
                  <xsl:with-param name="Fallback" select="'DATA_PROVIDER'" />
                </xsl:call-template>
              </xsl:variable>
              
              <xsl:variable name="CompanyCode">
                <xsl:call-template name="Apply_Order_of_Precedence">
                  <xsl:with-param name="DocValue" select="$DataContext/*[local-name()='Company']/*[local-name()='Code']" />
                  <xsl:with-param name="Fallback" select="'COMPANY_CODE'" />
                </xsl:call-template>
              </xsl:variable>
              
              <xsl:variable name="EnterpriseID">
                <xsl:call-template name="Apply_Order_of_Precedence">
                  <xsl:with-param name="DocValue" select="$DataContext/*[local-name()='EnterpriseID']" />
                  <xsl:with-param name="Fallback" select="'ENTERPRISE_ID'" />
                </xsl:call-template>
              </xsl:variable>
              
              <xsl:variable name="ServerID">
                <xsl:call-template name="Apply_Order_of_Precedence">
                  <xsl:with-param name="DocValue" select="$DataContext/*[local-name()='ServerID']" />
                  <xsl:with-param name="Fallback" select="'SERVER_ID'" />
                </xsl:call-template>
              </xsl:variable>
              
              
              <ns0:DataProvider>
                <xsl:value-of select="$DataProvider" />
              </ns0:DataProvider>
              
              <ns0:DataTargetCollection>
                <ns0:DataTarget>
                  <ns0:Type>
                    <xsl:text>ForwardingBooking</xsl:text>
                  </ns0:Type>
                </ns0:DataTarget>
              </ns0:DataTargetCollection>
              
              <xsl:if test="$CompanyCode != '' ">
                <ns0:Company>
                  <ns0:Code>
                    <xsl:value-of select="$CompanyCode" />
                  </ns0:Code>
                </ns0:Company>
              </xsl:if>
              
              <xsl:if test="$EnterpriseID != '' ">
                <ns0:EnterpriseID>
                  <xsl:value-of select="$EnterpriseID" />
                </ns0:EnterpriseID>
              </xsl:if>
              
              <xsl:if test="$ServerID != '' ">
                <ns0:ServerID>
                  <xsl:value-of select="$ServerID" />
                </ns0:ServerID>
              </xsl:if>
              
            </ns0:DataContext>
            
            
            <!-- ActualChargeable: -->
            
            <xsl:call-template name="MapValueIfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:ActualChargeable'" />
              <xsl:with-param name="Data" select="$Shipment/*[local-name()='ActualChargeable']" />
            </xsl:call-template>
            
            
            <!-- AdditionalTerms: -->
            
            <xsl:call-template name="Map_String_IfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:AdditionalTerms'" />
              <xsl:with-param name="Data" select="$Shipment/*[local-name()='AdditionalTerms']" />
              <xsl:with-param name="Max"  select="50" />
            </xsl:call-template>
            
            
            <!-- AWBServiceLevel: -->
            
            <xsl:call-template name="Map_String_IfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:AWBServiceLevel/ns0:Code'" />
              <xsl:with-param name="Data" select="$Shipment/*[local-name()='AWBServiceLevel']/*[local-name()='Code']" />
              <xsl:with-param name="Max"  select="3" />
            </xsl:call-template>
            
            
            <!-- BookingConfirmationReference: -->
            
            <xsl:call-template name="Map_String_IfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:BookingConfirmationReference'" />
              <xsl:with-param name="Data" select="$Shipment/*[local-name()='BookingConfirmationReference']" />
              <xsl:with-param name="Max"  select="35" />
            </xsl:call-template>
            
            
            <!-- CFSReference: -->
            
            <xsl:call-template name="Map_String_IfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:CFSReference'" />
              <xsl:with-param name="Data" select="$Shipment/*[local-name()='CFSReference']" />
              <xsl:with-param name="Max"  select="20" />
            </xsl:call-template>
            
            
            <!-- ContainerCount: -->
            
            <xsl:call-template name="MapValueIfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:ContainerCount'" />
              <xsl:with-param name="Data" select="$Shipment/*[local-name()='ContainerCount']" />
            </xsl:call-template>
            
            
            <!-- ContainerMode: -->
            
            <xsl:call-template name="Map_String_IfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:ContainerMode/ns0:Code'" />
              <xsl:with-param name="Data" select="$Shipment/*[local-name()='ContainerMode']/*[local-name()='Code']" />
              <xsl:with-param name="Max"  select="3" />
            </xsl:call-template>
            
            
            <!-- FreightRate: -->
            
            <xsl:call-template name="MapValueIfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:FreightRate'" />
              <xsl:with-param name="Data" select="$Shipment/*[local-name()='FreightRate']" />
            </xsl:call-template>
            
            
            <!-- FreightRateCurrency: -->
            
            <xsl:call-template name="Map_String_IfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:FreightRateCurrency/ns0:Code'" />
              <xsl:with-param name="Data" select="$Shipment/*[local-name()='FreightRateCurrency']/*[local-name()='Code']" />
              <xsl:with-param name="Max"  select="3" />
            </xsl:call-template>
            
            
            <!-- GoodsDescription: -->
            
            <xsl:call-template name="Map_String_IfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:GoodsDescription'" />
              <xsl:with-param name="Data" select="$Shipment/*[local-name()='GoodsDescription']" />
              <xsl:with-param name="Max"  select="128" />
            </xsl:call-template>
            
            
            <!-- GoodsValue: -->
            
            <xsl:call-template name="MapValueIfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:GoodsValue'" />
              <xsl:with-param name="Data" select="$Shipment/*[local-name()='GoodsValue']" />
            </xsl:call-template>
            
            
            <!-- GoodsValueCurrency: -->
            
            <xsl:call-template name="Map_String_IfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:GoodsValueCurrency/ns0:Code'" />
              <xsl:with-param name="Data" select="$Shipment/*[local-name()='GoodsValueCurrency']/*[local-name()='Code']" />
              <xsl:with-param name="Max"  select="3" />
            </xsl:call-template>
            
            
            <!-- HBLAWBChargesDisplay: -->
            
            <xsl:call-template name="Map_String_IfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:HBLAWBChargesDisplay/ns0:Code'" />
              <xsl:with-param name="Data" select="$Shipment/*[local-name()='HBLAWBChargesDisplay']/*[local-name()='Code']" />
              <xsl:with-param name="Max"  select="3" />
            </xsl:call-template>
            
            
            <!-- InsuranceValue: -->
            
            <xsl:call-template name="MapValueIfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:InsuranceValue'" />
              <xsl:with-param name="Data" select="$Shipment/*[local-name()='InsuranceValue']" />
            </xsl:call-template>
            
            
            <!-- InsuranceValueCurrency: -->
            
            <xsl:call-template name="Map_String_IfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:InsuranceValueCurrency/ns0:Code'" />
              <xsl:with-param name="Data" select="$Shipment/*[local-name()='InsuranceValueCurrency']/*[local-name()='Code']" />
              <xsl:with-param name="Max"  select="3" />
            </xsl:call-template>
            
            
            <!-- InterimReceiptNumber: -->
            
            <xsl:call-template name="Map_String_IfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:InterimReceiptNumber'" />
              <xsl:with-param name="Data" select="$Shipment/*[local-name()='InterimReceiptNumber']" />
              <xsl:with-param name="Max"  select="35" />
            </xsl:call-template>
            
            
            <!-- IsCancelled: -->
            
            <xsl:call-template name="Map_Boolean_IfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:IsCancelled'" />
              <xsl:with-param name="Data" select="$Shipment/*[local-name()='IsCancelled']" />
            </xsl:call-template>
            
            
            <!-- IsDirectBooking: -->
            
            <xsl:call-template name="Map_Boolean_IfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:IsDirectBooking'" />
              <xsl:with-param name="Data" select="$Shipment/*[local-name()='IsDirectBooking']" />
            </xsl:call-template>
            
            
            <!-- IsForwardRegistered: -->
            
            <xsl:call-template name="Map_Boolean_IfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:IsForwardRegistered'" />
              <xsl:with-param name="Data" select="$Shipment/*[local-name()='IsForwardRegistered']" />
            </xsl:call-template>
            
            
            <!-- LloydsIMO: -->
            
            <xsl:call-template name="Map_String_IfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:LloydsIMO'" />
              <xsl:with-param name="Data" select="$Shipment/*[local-name()='LloydsIMO']" />
              <xsl:with-param name="Max"  select="7" />
            </xsl:call-template>
            
            
            <!-- OuterPacks: -->
            
            <xsl:call-template name="MapValueIfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:OuterPacks'" />
              <xsl:with-param name="Data" select="$Shipment/*[local-name()='OuterPacks']" />
            </xsl:call-template>
            
            
            <!-- OuterPacksPackageType: -->
            
            <xsl:call-template name="Map_String_IfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:OuterPacksPackageType/ns0:Code'" />
              <xsl:with-param name="Data" select="ScriptNS0:GetRecipientCode( $Sender, $Recipient, $TS_Name, 'Package Type', 'CW1 Code',
                                                              $Shipment/*[local-name()='OuterPacksPackageType']/*[local-name()='Code'] )" />
              <xsl:with-param name="Max"  select="3" />
            </xsl:call-template>
            
            
            <!-- PackingOrder: -->
            
            <xsl:call-template name="MapValueIfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:PackingOrder'" />
              <xsl:with-param name="Data" select="$Shipment/*[local-name()='PackingOrder']" />
            </xsl:call-template>
            
            
            <!-- PortOfDestination: -->
            
            <xsl:variable name="PortOfDestination">
              <xsl:call-template name="Apply_Order_of_Precedence">
                <xsl:with-param name="DocValue" select="$Shipment/*[local-name()='PortOfDestination']/*[local-name()='Code']" />
                <xsl:with-param name="Fallback" select="'PORT_OF_DESTINATION'" />
                <xsl:with-param name="Required" select="5" />
              </xsl:call-template>
            </xsl:variable>
            
            <xsl:call-template name="Map_String_IfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:PortOfDestination/ns0:Code'" />
              <xsl:with-param name="Data" select="$PortOfDestination" />
              <xsl:with-param name="Max"  select="5" />
            </xsl:call-template>
            
            
            <!-- PortOfDischarge: -->
            
            <xsl:call-template name="Map_String_IfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:PortOfDischarge/ns0:Code'" />
              <xsl:with-param name="Data" select="$Shipment/*[local-name()='PortOfDischarge']/*[local-name()='Code']" />
              <xsl:with-param name="Max"  select="5" />
            </xsl:call-template>
            
            
            <!-- PortOfLoading: -->
            
            <xsl:call-template name="Map_String_IfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:PortOfLoading/ns0:Code'" />
              <xsl:with-param name="Data" select="$Shipment/*[local-name()='PortOfLoading']/*[local-name()='Code']" />
              <xsl:with-param name="Max"  select="5" />
            </xsl:call-template>
            
            
            <!-- PortOfOrigin: -->
            
            <xsl:variable name="PortOfOrigin">
              <xsl:call-template name="Apply_Order_of_Precedence">
                <xsl:with-param name="DocValue" select="$Shipment/*[local-name()='PortOfOrigin']/*[local-name()='Code']" />
                <xsl:with-param name="Fallback" select="'PORT_OF_ORIGIN'" />
                <xsl:with-param name="Required" select="5" />
              </xsl:call-template>
            </xsl:variable>
            
            <xsl:call-template name="Map_String_IfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:PortOfOrigin/ns0:Code'" />
              <xsl:with-param name="Data" select="$PortOfOrigin" />
              <xsl:with-param name="Max"  select="5" />
            </xsl:call-template>
            
            
            <!-- ReleaseType: -->
            
            <xsl:call-template name="Map_String_IfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:ReleaseType/ns0:Code'" />
              <xsl:with-param name="Data" select="$Shipment/*[local-name()='ReleaseType']/*[local-name()='Code']" />
              <xsl:with-param name="Max"  select="3" />
            </xsl:call-template>
            
            
            <!-- ServiceLevel: -->
            
            <xsl:call-template name="Map_String_IfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:ServiceLevel/ns0:Code'" />
              <xsl:with-param name="Data" select="$Shipment/*[local-name()='ServiceLevel']/*[local-name()='Code']" />
              <xsl:with-param name="Max"  select="3" />
            </xsl:call-template>
            
            
            <!-- ShipmentIncoTerm: -->
            
            <xsl:call-template name="Map_String_IfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:ShipmentIncoTerm/ns0:Code'" />
              <xsl:with-param name="Data" select="$Shipment/*[local-name()='ShipmentIncoTerm']/*[local-name()='Code']" />
              <xsl:with-param name="Max"  select="3" />
            </xsl:call-template>
            
            
            <!-- ShippedOnBoard: -->
            
            <xsl:call-template name="Map_String_IfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:ShippedOnBoard/ns0:Code'" />
              <xsl:with-param name="Data" select="$Shipment/*[local-name()='ShippedOnBoard']/*[local-name()='Code']" />
              <xsl:with-param name="Max"  select="3" />
            </xsl:call-template>
            
            
            <!-- TotalVolume: -->
            
            <xsl:call-template name="MapValueIfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:TotalVolume'" />
              <xsl:with-param name="Data" select="$Shipment/*[local-name()='TotalVolume']" />
            </xsl:call-template>
            
            
            <!-- TotalVolumeUnit: -->
            
            <xsl:call-template name="Map_String_IfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:TotalVolumeUnit/ns0:Code'" />
              <xsl:with-param name="Data" select="ScriptNS0:GetRecipientCode( $Sender, $Recipient, $TS_Name, 'Unit of Measurement', 
                                                              $Shipment/*[local-name()='TotalVolumeUnit']/*[local-name()='Code'] )" />
              <xsl:with-param name="Max"  select="2" />
            </xsl:call-template>
            
            
            <!-- TotalWeight: -->
            
            <xsl:call-template name="MapValueIfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:TotalWeight'" />
              <xsl:with-param name="Data" select="$Shipment/*[local-name()='TotalWeight']" />
            </xsl:call-template>
            
            
            <!-- TotalWeightUnit: -->
            
            <xsl:call-template name="Map_String_IfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:TotalWeightUnit/ns0:Code'" />
              <xsl:with-param name="Data" select="ScriptNS0:GetRecipientCode( $Sender, $Recipient, $TS_Name, 'Unit of Measurement', 
                                                              $Shipment/*[local-name()='TotalWeightUnit']/*[local-name()='Code'] )" />
              <xsl:with-param name="Max"  select="2" />
            </xsl:call-template>
            
            
            <!-- TransportMode: -->
            
            <xsl:variable name="TxMode">
              <xsl:call-template name="Apply_Order_of_Precedence">
                <xsl:with-param name="DocValue" select="$Shipment/*[local-name()='TransportMode']/*[local-name()='Code']" />
                <xsl:with-param name="Fallback" select="'TRANSPORT_MODE'" />
                <xsl:with-param name="Required" select="3" />
              </xsl:call-template>
            </xsl:variable>
            
            <xsl:call-template name="Map_String_IfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:TransportMode/ns0:Code'" />
              <xsl:with-param name="Data" select="$TxMode" />
              <xsl:with-param name="Max"  select="3" />
            </xsl:call-template>
            
            
            <!-- VesselName: -->
            
            <xsl:call-template name="Map_String_IfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:VesselName'" />
              <xsl:with-param name="Data" select="$Shipment/*[local-name()='VesselName']" />
              <xsl:with-param name="Max"  select="35" />
            </xsl:call-template>
            
            
            <!-- VoyageFlightNo: -->
            
            <xsl:call-template name="Map_String_IfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:VoyageFlightNo'" />
              <xsl:with-param name="Data" select="$Shipment/*[local-name()='VoyageFlightNo']" />
              <xsl:with-param name="Max"  select="10" />
            </xsl:call-template>
            
            
            <!-- WayBillNumber: -->
            
            <xsl:call-template name="Map_String_IfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:WayBillNumber'" />
              <xsl:with-param name="Data" select="$Shipment/*[local-name()='WayBillNumber']" />
              <xsl:with-param name="Max"  select="35" />
            </xsl:call-template>
            
            
            <!-- WayBillType: -->
            
            <xsl:call-template name="Map_String_IfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:WayBillType/ns0:Code'" />
              <xsl:with-param name="Data" select="$Shipment/*[local-name()='WayBillType']/*[local-name()='Code']" />
              <xsl:with-param name="Max"  select="3" />
            </xsl:call-template>
            
            
            <!-- Order with OrderNumber: -->
            
            <xsl:variable name="LocProc"    select="$Shipment/*[local-name()='LocalProcessing']" />
            <xsl:variable name="ArrOrderNo" select="$LocProc/*[local-name()='OrderNumberCollection']/*[local-name()='OrderNumber']" />
            
            <xsl:call-template name="Map_String_IfNotEmpty">
              <xsl:with-param name="Node" select="'ns0:Order/ns0:OrderNumber'" />
              <xsl:with-param name="Data" select="$ArrOrderNo/*[local-name()='OrderReference'] [normalize-space(.) != ''] [1]" />
              <xsl:with-param name="Max"  select="35" />
            </xsl:call-template>
            
            
            <!-- ================== -->
            <!--  LocalProcessing:  -->
            <!-- ================== -->
            
            <ns0:LocalProcessing>
              
              <!-- DeliveryRequiredBy: -->
              
              <xsl:call-template name="MapValueIfNotEmpty">
                <xsl:with-param name="Node" select="'ns0:DeliveryRequiredBy'" />
                <xsl:with-param name="Data" select="$LocProc/*[local-name()='DeliveryRequiredBy']" />
              </xsl:call-template>
              
              
              <!-- EstimatedDelivery: -->
              
              <xsl:call-template name="MapValueIfNotEmpty">
                <xsl:with-param name="Node" select="'ns0:EstimatedDelivery'" />
                <xsl:with-param name="Data" select="$LocProc/*[local-name()='EstimatedDelivery']" />
              </xsl:call-template>
              
              
              <!-- EstimatedPickup: -->
              
              <xsl:call-template name="MapValueIfNotEmpty">
                <xsl:with-param name="Node" select="'ns0:EstimatedPickup'" />
                <xsl:with-param name="Data" select="$LocProc/*[local-name()='EstimatedPickup']" />
              </xsl:call-template>
              
              
              <!-- FCLDeliveryEquipmentNeeded: -->
              
              <xsl:call-template name="Map_String_IfNotEmpty">
                <xsl:with-param name="Node" select="'ns0:FCLDeliveryEquipmentNeeded/ns0:Code'" />
                <xsl:with-param name="Data" select="$LocProc/*[local-name()='FCLDeliveryEquipmentNeeded']/*[local-name()='Code']" />
                <xsl:with-param name="Max"  select="3" />
              </xsl:call-template>
              
              
              <!-- FCLPickupEquipmentNeeded: -->
              
              <xsl:call-template name="Map_String_IfNotEmpty">
                <xsl:with-param name="Node" select="'ns0:FCLPickupEquipmentNeeded/ns0:Code'" />
                <xsl:with-param name="Data" select="$LocProc/*[local-name()='FCLPickupEquipmentNeeded']/*[local-name()='Code']" />
                <xsl:with-param name="Max"  select="3" />
              </xsl:call-template>
              
              
              <!-- InsuranceRequired: -->
              
              <xsl:call-template name="Map_Boolean_IfNotEmpty">
                <xsl:with-param name="Node" select="'ns0:InsuranceRequired'" />
                <xsl:with-param name="Data" select="$LocProc/*[local-name()='InsuranceRequired']" />
              </xsl:call-template>
              
              
              <!-- PickupRequiredBy: -->
              
              <xsl:call-template name="MapValueIfNotEmpty">
                <xsl:with-param name="Node" select="'ns0:PickupRequiredBy'" />
                <xsl:with-param name="Data" select="$LocProc/*[local-name()='PickupRequiredBy']" />
              </xsl:call-template>
              
              
              <!-- OrderNumberCollection: -->
              
              <ns0:OrderNumberCollection>
                <xsl:for-each select="$ArrOrderNo">
                  <xsl:variable name="OrderNumber" select="." />
                  
                  <xsl:call-template name="Map_String_IfNotEmpty">
                    <xsl:with-param name="Node" select="'ns0:OrderNumber/ns0:OrderReference'" />
                    <xsl:with-param name="Data" select="$OrderNumber/*[local-name()='OrderReference']" />
                    <xsl:with-param name="Max"  select="35" />
                  </xsl:call-template>
                  
                </xsl:for-each>
              </ns0:OrderNumberCollection>
              
            </ns0:LocalProcessing>
            
            
            <!-- ================== -->
            <!--  Notes:            -->
            <!-- ================== -->
            
            <ns0:NoteCollection>
              
              <xsl:attribute name="Content">
                <xsl:value-of select = "'Partial'"/>
              </xsl:attribute>
              
              <xsl:for-each select="$Shipment/*[local-name()='NoteCollection']/*[local-name()='Note']" >
                
                <xsl:call-template name="Map_Note">
                  <xsl:with-param name="Note" select="." />
                </xsl:call-template>
                
              </xsl:for-each>
            </ns0:NoteCollection>
            
            
            <!-- ================== -->
            <!--  Organizations:    -->
            <!-- ================== -->
            
            <ns0:OrganizationAddressCollection>
              <xsl:for-each select="$Shipment/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress']" >
                
                <xsl:call-template name="Map_OrgAddress">
                  <xsl:with-param name="OrgAddr" select="." />
                </xsl:call-template>
                
              </xsl:for-each>
            </ns0:OrganizationAddressCollection>
            
            
          </ns0:Shipment>
        </ns0:UniversalShipment>
      </ns0:Body>
    </ns0:UniversalInterchange>
    
  </xsl:template>  <!-- End: Main Template -->
  
  
  
  <!-- ================================================ -->
  <!--  Template: "Map_Note"                            -->
  <!-- ================================================ -->
  <xsl:template name="Map_Note">
    <xsl:param name="Note" />
    
    <ns0:Note>
      
      <!-- Description: -->
      
      <xsl:call-template name="Map_String_IfNotEmpty">
        <xsl:with-param name="Node" select="'ns0:Description'" />
        <xsl:with-param name="Data" select="$Note/*[local-name()='Description']" />
        <xsl:with-param name="Max"  select="50" />
      </xsl:call-template>
      
      
      <!-- IsCustomDescription: -->
      
      <xsl:call-template name="Map_Boolean_Always">
        <xsl:with-param name="Node" select="'ns0:IsCustomDescription'" />
        <xsl:with-param name="Data" select="$Note/*[local-name()='IsCustomDescription']" />
        <xsl:with-param name="Else" select="'true'" />
      </xsl:call-template>
      
      
      <!-- NoteText: -->
      
      <xsl:call-template name="Map_String_IfNotEmpty">
        <xsl:with-param name="Node" select="'ns0:NoteText'" />
        <xsl:with-param name="Data" select="$Note/*[local-name()='NoteText']" />
        <xsl:with-param name="Max"  select="2147483647" />
      </xsl:call-template>
      
    </ns0:Note>
    
  </xsl:template>  <!-- Template: "Map_Note" -->
  
  
  
  <!-- ================================================ -->
  <!--  Template: "Map_OrgAddress"                      -->
  <!-- ================================================ -->
  <xsl:template name="Map_OrgAddress">
    <xsl:param name="OrgAddr" />
    
    <ns0:OrganizationAddress>
      
      <!-- AddressType: -->
      
      <xsl:call-template name="Map_String_IfNotEmpty">
        <xsl:with-param name="Node" select="'ns0:AddressType'" />
        <xsl:with-param name="Data" select="$OrgAddr/*[local-name()='AddressType']" />
        <xsl:with-param name="Max"  select="40" />
      </xsl:call-template>
      
      
      <!-- Address1: -->
      
      <xsl:call-template name="Map_String_IfNotEmpty">
        <xsl:with-param name="Node" select="'ns0:Address1'" />
        <xsl:with-param name="Data" select="$OrgAddr/*[local-name()='Address1']" />
        <xsl:with-param name="Max"  select="50" />
      </xsl:call-template>
      
      
      <!-- Address2: -->
      
      <xsl:call-template name="Map_String_IfNotEmpty">
        <xsl:with-param name="Node" select="'ns0:Address2'" />
        <xsl:with-param name="Data" select="$OrgAddr/*[local-name()='Address2']" />
        <xsl:with-param name="Max"  select="50" />
      </xsl:call-template>
      
      
      <!-- AddressOverride: -->
      
      <xsl:call-template name="Map_Boolean_Always">
        <xsl:with-param name="Node" select="'ns0:AddressOverride'" />
        <xsl:with-param name="Data" select="$OrgAddr/*[local-name()='AddressOverride']" />
        <xsl:with-param name="Else" select="'true'" />
      </xsl:call-template>
      
      
      <!-- AddressShortCode: -->
      
      <xsl:call-template name="Map_String_IfNotEmpty">
        <xsl:with-param name="Node" select="'ns0:AddressShortCode'" />
        <xsl:with-param name="Data" select="$OrgAddr/*[local-name()='AddressShortCode']" />
        <xsl:with-param name="Max"  select="25" />
      </xsl:call-template>
      
      
      <!-- City: -->
      
      <xsl:call-template name="Map_String_IfNotEmpty">
        <xsl:with-param name="Node" select="'ns0:City'" />
        <xsl:with-param name="Data" select="$OrgAddr/*[local-name()='City']" />
        <xsl:with-param name="Max"  select="25" />
      </xsl:call-template>
      
      
      <!-- CompanyName: -->
      
      <xsl:call-template name="Map_String_IfNotEmpty">
        <xsl:with-param name="Node" select="'ns0:CompanyName'" />
        <xsl:with-param name="Data" select="$OrgAddr/*[local-name()='CompanyName']" />
        <xsl:with-param name="Max"  select="100" />
      </xsl:call-template>
      
    </ns0:OrganizationAddress>
    
  </xsl:template>  <!-- Template: "Map_OrgAddress" -->
  
  
  
  <!-- ================================================ -->
  <!--  Template: "Apply_Order_of_Precedence"           -->
  <!-- ================================================ -->
  <xsl:template name="Apply_Order_of_Precedence">
    <xsl:param name="DocValue" />
    <xsl:param name="Fallback" />
    <xsl:param name="Required" select="0" />
    
    <xsl:variable name="Data" select="normalize-space( $DocValue )" />
    
    <xsl:choose>
      
      <xsl:when test="($Required = 0) and ($Data != '') ">
        <xsl:value-of select="$Data" />
      </xsl:when>
      
      <xsl:when test="($Required &gt; 0) and ($Data != '') and (string-length($Data) = $Required) ">
        <xsl:value-of select="$Data" />
      </xsl:when>
      
      <xsl:otherwise>
        <xsl:call-template name="GetDefaultValue">
          <xsl:with-param name="Label" select="$Fallback" />
        </xsl:call-template>
      </xsl:otherwise>
      
    </xsl:choose>
    
  </xsl:template>  <!-- Template: "Apply_Order_of_Precedence" -->
  
  
  
  <!-- ================================================ -->
  <!--  Template: "Map_String_IfNotEmpty"               -->
  <!-- ================================================ -->
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
    
    
    <xsl:if test="$TheData != '' ">
      <xsl:call-template name="MapValueIfNotEmpty">
        <xsl:with-param name="Node" select="$Node" />
        <xsl:with-param name="Data" select="$TheData" />
      </xsl:call-template>
    </xsl:if>
    
  </xsl:template>  <!-- Template: "Map_String_IfNotEmpty" -->
  
  
  
  <!-- ================================================ -->
  <!--  Template: "Map_Boolean_IfNotEmpty"              -->
  <!-- ================================================ -->
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
  
  
  
  <!-- ================================================ -->
  <!--  Template: "Map_Boolean_Always"                  -->
  <!-- ================================================ -->
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
  
  
  
  <!-- ================================================ -->
  <!--  Template: "GetDefaultValue"                     -->
  <!-- ================================================ -->
  <xsl:template name="GetDefaultValue">
    <xsl:param name="Label" />
    
    <xsl:choose>
      
      <xsl:when test="$Label = 'DATA_PROVIDER' ">
        <xsl:value-of select="ScriptNS0:GetRecipientCodeUnkeyed( $Sender, $Recipient, $TS_Name,'Defaults','Data Provider')" />
      </xsl:when>
      
      <xsl:when test="$Label = 'COMPANY_CODE' ">
        <xsl:value-of select="ScriptNS0:GetRecipientCodeUnkeyed( $Sender, $Recipient, $TS_Name,'Defaults','Company Code')" />
      </xsl:when>
      
      <xsl:when test="$Label = 'ENTERPRISE_ID' ">
        <xsl:value-of select="ScriptNS0:GetRecipientCodeUnkeyed( $Sender, $Recipient, $TS_Name,'Defaults','Enterprise ID')" />
      </xsl:when>
      
      <xsl:when test="$Label = 'SERVER_ID' ">
        <xsl:value-of select="ScriptNS0:GetRecipientCodeUnkeyed( $Sender, $Recipient, $TS_Name,'Defaults','Server ID')" />
      </xsl:when>
      
      <xsl:when test="$Label = 'TRANSPORT_MODE' ">
        <xsl:value-of select="ScriptNS0:GetRecipientCodeUnkeyed( $Sender, $Recipient, $TS_Name,'Defaults','Transport Mode')" />
      </xsl:when>
      
      <xsl:when test="$Label = 'PORT_OF_DESTINATION' ">
        <xsl:value-of select="ScriptNS0:GetRecipientCodeUnkeyed( $Sender, $Recipient, $TS_Name,'Defaults','Port of Destination')" />
      </xsl:when>
      
      <xsl:when test="$Label = 'PORT_OF_ORIGIN' ">
        <xsl:value-of select="ScriptNS0:GetRecipientCodeUnkeyed( $Sender, $Recipient, $TS_Name,'Defaults','Port of Origin')" />
      </xsl:when>
      
    </xsl:choose>
    
  </xsl:template>  <!-- Template: "GetDefaultValue" -->
  
  
  
  <!-- ================================================ -->
  <!--  Template: "MapValueIfNotEmpty"                  -->
  <!-- ================================================ -->
  <xsl:template name="MapValueIfNotEmpty">
    <xsl:param name="Node" />
    <xsl:param name="Data" />
    
    <xsl:if test="$Data != '' ">
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

  </xsl:template>
  
</xsl:stylesheet>
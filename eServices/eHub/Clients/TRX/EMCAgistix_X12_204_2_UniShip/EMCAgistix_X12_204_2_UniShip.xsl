<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl ScriptNS0 ScriptNS1 userCSharp ns0" version="1.0"
                xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
                xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="yes" indent="no" version="1.0" method="xml" />

  <xsl:template match="/">
    <xsl:apply-templates select="/*[local-name()='X12_00401_204']" />
  </xsl:template>

  <xsl:template match="/*[local-name()='X12_00401_204']">
    <xsl:element name="ns0:UniversalInterchange">

      <xsl:element name="Header">
        <xsl:element name="SenderID">
          <xsl:text></xsl:text>
        </xsl:element>
        <xsl:element name="RecipientID">
          <xsl:text></xsl:text>
        </xsl:element>
      </xsl:element>

      <xsl:element name="Body">
        <xsl:element name ="UniversalShipment">
          <xsl:element name ="Shipment">
            <xsl:element name ="DataContext">

              <xsl:element name ="DataProvider">
                <xsl:value-of select="ScriptNS1:GetRecipientCodeUnkeyed('TRXELPELP_E01' , 'TRXELPELP' , 'EMC 204 - Receive Shipments' , 'Defaults' , 'Data Provider Code')"/>
              </xsl:element>

              <xsl:element name ="DataTargetCollection">
                <xsl:element name ="DataTarget">
                  <xsl:element name ="Type">
                    <xsl:text>ForwardingShipment</xsl:text>
                  </xsl:element>
                </xsl:element>
              </xsl:element>

            </xsl:element>

            <xsl:variable name="ShipperRef" select="*[local-name()='B2']/*[local-name()='B204']"/>
            <xsl:call-template name="MapValueIfNotEmpty">
              <xsl:with-param name="NodeName" select="'BookingConfirmationReference'" />
              <xsl:with-param name="Value" select="$ShipperRef" />
            </xsl:call-template>

            <xsl:variable name="GoodsDescriptionNodes" select="*[local-name()='L11'][*[local-name()='L1102'] = 'OR']/*[local-name() = 'L1101' and normalize-space(.) != '']" />
            <xsl:variable name="GoodsDescription">
              <xsl:value-of select="userCSharp:JoinLines($GoodsDescriptionNodes)" />
            </xsl:variable>
            <xsl:call-template name="MapValueIfNotEmpty">
              <xsl:with-param name="NodeName" select="'GoodsDescription'" />
              <xsl:with-param name="Value" select="$GoodsDescription" />
            </xsl:call-template>

            <xsl:variable name="InnerPackQty" select="*[local-name()='L3']/*[local-name()='L310']" />

            <xsl:if test="$InnerPackQty >= 0">

              <xsl:element name="TotalNoOfPacks">
                <xsl:value-of select="$InnerPackQty"/>
              </xsl:element>

              <xsl:call-template name="MapValueIfNotEmpty">
                <xsl:with-param name="NodeName" select="'TotalNoOfPacksPackageType/Code'" />
                <xsl:with-param name="Value" select="ScriptNS1:GetRecipientCodeUnkeyed('TRXELPELP_E01' , 'TRXELPELP' , 'EMC 204 - Receive Shipments' , 'Defaults' , 'Inner Pack Type')" />
              </xsl:call-template>

            </xsl:if>

            <xsl:variable name ="CL_S5Loop1" select ="*[local-name()='S5Loop1' and *[local-name()='S5']/*[local-name()='S502'] ='CL']"/>
            <xsl:variable name ="CU_S5Loop1" select ="*[local-name()='S5Loop1' and *[local-name()='S5']/*[local-name()='S502'] ='CU']"/>

            <xsl:variable name="PackQty" select="*[local-name()='L3']/*[local-name()='L311']" />
            <xsl:variable name ="PackLinePackType" select ="$CL_S5Loop1/*[local-name()='LAD']/*[local-name()='LAD01' and . != ''][1]"/>
            <xsl:variable name ="DefaultPackType" select ="ScriptNS1:GetRecipientCodeUnkeyed('TRXELPELP_E01' , 'TRXELPELP' , 'EMC 204 - Receive Shipments' , 'Defaults' , 'Pack Type')"/>

            <xsl:variable name ="PackType">
              <xsl:choose>
                <xsl:when test="$PackLinePackType != ''">
                  <xsl:value-of select ="$PackLinePackType"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$DefaultPackType" />
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>

            <xsl:if test="$PackQty >= 0">

              <xsl:element name="OuterPacks">
                <xsl:value-of select="$PackQty"/>
              </xsl:element>

              <xsl:call-template name="MapValueIfNotEmpty">
                <xsl:with-param name="NodeName" select="'OuterPacksPackageType/Code'" />
                <xsl:with-param name="Value" select="$PackType" />
              </xsl:call-template>

            </xsl:if>

            <xsl:call-template name="MapValueIfNotEmpty">
              <xsl:with-param name="NodeName" select="'ServiceLevel/Code'" />
              <xsl:with-param name="Value" select="*[local-name()='L11' and *[local-name()='L1102'] = 'SL']/*[local-name()='L1101' and . != '']" />
            </xsl:call-template>

            <xsl:variable name="Weight" select="*[local-name()='L3']/*[local-name()='L301']" />
            <xsl:if test="$Weight >= 0">
              <xsl:element name="TotalWeight">
                <xsl:value-of select="$Weight"/>
              </xsl:element>

              <xsl:call-template name="MapValueIfNotEmpty">
                <xsl:with-param name="NodeName" select="'TotalWeightUnit/Code'" />
                <xsl:with-param name="Value" select="ScriptNS1:GetRecipientCode('TRXELPELP_E01' , 'TRXELPELP' , 'EMC 204 - Receive Shipments' , 'Unit of Measurement' , //*[local-name()='L303'])" />
              </xsl:call-template>
            </xsl:if>

            <xsl:variable name="Shipper" select="$CL_S5Loop1/*[local-name()='N1Loop2' and *[local-name()='N1_2']/*[local-name()='N101'] = 'SF']"/>

            <xsl:element name="PortOfOrigin">
              <xsl:element name="Code">
                <xsl:variable name ="Country" select ="$Shipper/*[local-name()='N4_2']/*[local-name()='N404']"/>
                <xsl:variable name ="State" select ="$Shipper/*[local-name()='N4_2']/*[local-name()='N402']"/>
                <xsl:variable name="Unlocode" select="ScriptNS1:CallActionProcedureHelper('GetUNLOCOFromLocation', '@result', '@city', $Shipper/*[local-name()='N4_2']/*[local-name()='N401'], 
                          '@state', $State,
                          '@country', $Country)" />
                <xsl:choose>
                  <xsl:when test="$Unlocode != ''">
                    <xsl:value-of select="$Unlocode"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="concat($Country, '_', $State)"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:element>
            </xsl:element>

            <xsl:variable name="Consignee" select="$CU_S5Loop1/*[local-name()='N1Loop2' and *[local-name()='N1_2']/*[local-name()='N101'] = 'ST']"/>

            <xsl:element name="PortOfDestination">
              <xsl:element name="Code">
                <xsl:variable name="Unlocode" select="ScriptNS1:CallActionProcedureHelper('GetUNLOCOFromLocation', '@result', '@city', $Consignee/*[local-name()='N4_2']/*[local-name()='N401'], 
                          '@state', $Consignee/*[local-name()='N4_2']/*[local-name()='N402'],
                          '@country', $Consignee/*[local-name()='N4_2']/*[local-name()='N404'])" />
                <xsl:choose>
                  <xsl:when test="$Unlocode != ''">
                    <xsl:value-of select="$Unlocode"/>
                  </xsl:when>
                  <xsl:when test="$Consignee/*[local-name()='N4_2']/*[local-name()='N404'] != ''">
                    <xsl:value-of select="$Consignee/*[local-name()='N4_2']/*[local-name()='N404']"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="'US'"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:element>
            </xsl:element>

            <xsl:variable name ="TransportMode" select ="ScriptNS1:GetRecipientCode('TRXELPELP_E01' , 'TRXELPELP' , 'EMC 204 - Receive Shipments' , 'Transport Mode' , *[local-name()='L11' and *[local-name()='L1102'] = 'MO']/*[local-name()='L1101'])"/>
            <xsl:call-template name="MapValueIfNotEmpty">
              <xsl:with-param name="NodeName" select="'TransportMode/Code'" />
              <xsl:with-param name="Value" select="$TransportMode" />
            </xsl:call-template>

            <xsl:call-template name="MapValueIfNotEmpty">
              <xsl:with-param name="NodeName" select="'ContainerMode/Code'" />
              <xsl:with-param name="Value" select="ScriptNS1:GetRecipientCode('TRXELPELP_E01' , 'TRXELPELP' , 'EMC 204 - Receive Shipments' , 'Container Mode' , 'Container Mode', *[local-name()='L11' and *[local-name()='L1102'] = 'MO']/*[local-name()='L1101'], *[local-name()='L11' and *[local-name()='L1102'] = 'SL']/*[local-name()='L1101'])" />
            </xsl:call-template>

            <xsl:call-template name="MapValueIfNotEmpty">
              <xsl:with-param name="NodeName" select="'ShipmentIncoTerm/Code'" />
              <xsl:with-param name="Value" select="*[local-name()='B2']/*[local-name()='B206']" />
            </xsl:call-template>

            <xsl:if test="$ShipperRef != ''">
              <xsl:element name="WayBillNumber">
                <xsl:value-of select="$ShipperRef"/>
              </xsl:element>
              <xsl:element name ="WayBillType">
                <xsl:element name="Code">HWB</xsl:element>
              </xsl:element>
            </xsl:if>

            <xsl:variable name="G62_37" select ="$CL_S5Loop1/*[local-name()='G62_2' and (*[local-name()='G6201'] ='37' or *[local-name()='G6201'] ='037') and *[local-name()='G6202'] != ''] "/>
            <xsl:variable name="G62_69" select ="$CL_S5Loop1/*[local-name()='G62_2' and (*[local-name()='G6201'] ='69' or *[local-name()='G6201'] ='069') and *[local-name()='G6202'] != '']"/>
            <xsl:variable name="G62_53" select ="$CU_S5Loop1/*[local-name()='G62_2' and (*[local-name()='G6201'] ='53' or *[local-name()='G6201'] ='053') and *[local-name()='G6202'] != '']"/>
            <xsl:variable name="G62_68" select ="$CU_S5Loop1/*[local-name()='G62_2' and (*[local-name()='G6201'] ='68' or *[local-name()='G6201'] ='068') and *[local-name()='G6202'] != '']"/>

            <xsl:variable name="OrderNumbers" select="*[local-name()='L11' and not(contains('OR SI MO SL', *[local-name()='L1102'])) and normalize-space(*[local-name()='L1101']) != '']" />

            <xsl:variable name ="EstPickUp">
              <xsl:call-template name ="Select_G62Date">
                <xsl:with-param name ="Date1" select ="$G62_37"/>
                <xsl:with-param name ="Date2" select ="$G62_69"/>
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name ="PickupReqBy">
              <xsl:call-template name ="Select_G62Date">
                <xsl:with-param name ="Date1" select ="$G62_69"/>
                <xsl:with-param name ="Date2" select ="$G62_37"/>
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name ="EstDelv">
              <xsl:call-template name ="Select_G62Date">
                <xsl:with-param name ="Date1" select ="$G62_53"/>
                <xsl:with-param name ="Date2" select ="$G62_68"/>
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name ="DelvReqBy">
              <xsl:call-template name ="Select_G62Date">
                <xsl:with-param name ="Date1" select ="$G62_68"/>
                <xsl:with-param name ="Date2" select ="$G62_53"/>
              </xsl:call-template>
            </xsl:variable>

            <xsl:if test ="$EstPickUp != '' or $PickupReqBy != '' or $EstDelv != '' or  $DelvReqBy != '' or count($OrderNumbers) > 0">
              <xsl:element name ="LocalProcessing">

                <xsl:call-template name="MapValueIfNotEmpty">
                  <xsl:with-param name="NodeName" select="'EstimatedPickup'" />
                  <xsl:with-param name="Value" select="$EstPickUp" />
                </xsl:call-template>

                <xsl:call-template name="MapValueIfNotEmpty">
                  <xsl:with-param name="NodeName" select="'PickupRequiredBy'" />
                  <xsl:with-param name="Value" select="$PickupReqBy" />
                </xsl:call-template>

                <xsl:call-template name="MapValueIfNotEmpty">
                  <xsl:with-param name="NodeName" select="'EstimatedDelivery'" />
                  <xsl:with-param name="Value" select="$EstDelv" />
                </xsl:call-template>

                <xsl:call-template name="MapValueIfNotEmpty">
                  <xsl:with-param name="NodeName" select="'DeliveryRequiredBy'" />
                  <xsl:with-param name="Value" select="$DelvReqBy" />
                </xsl:call-template>

                <xsl:if test="count($OrderNumbers) > 0">
                  <xsl:element name="OrderNumberCollection">
                    <xsl:for-each select="$OrderNumbers">
                      <xsl:element name="OrderNumber">

                        <xsl:element name="OrderReference">
                          <xsl:value-of select="concat(./*[local-name()='L1102'], '-', ./*[local-name()='L1101'])"/>
                        </xsl:element>

                        <xsl:element name="Sequence">
                          <xsl:value-of select="position()"/>
                        </xsl:element>

                      </xsl:element>
                    </xsl:for-each>
                  </xsl:element>
                </xsl:if>

              </xsl:element>

            </xsl:if>

            <xsl:variable name="MarksAndNumbers" select ="*[local-name()='L11' and *[local-name()='L1102'] = 'SI']/*[local-name()='L1101' and . != '']"/>
            <xsl:variable name="TrailerNumbers" select="*[local-name()='N7Loop1']/*[local-name()='N7']
                                                        [*[local-name()='N711'] = 'FT'  or *[local-name()='N711'] = 'RT' or *[local-name()='N711'] = 'TL']/*[local-name()='N702' and . != '']"/>
            <xsl:variable name="SpecialInstructions" select="*[local-name()='NTE'][*[local-name()='NTE01'] = 'DEL']/*[local-name() = 'NTE02']" />

            <xsl:if test=" ($MarksAndNumbers !='' or $SpecialInstructions != '' or string-length($GoodsDescription) &gt; 35) or ($TransportMode = 'ROA' and $TrailerNumbers != '')">
              <xsl:element name="NoteCollection">

                <xsl:if test="$MarksAndNumbers !=''">
                  <xsl:call-template name="MapNote">
                    <xsl:with-param name="Description" select="'Marks &amp; Numbers'"/>
                    <xsl:with-param name="Value" select="$MarksAndNumbers"/>
                  </xsl:call-template>
                </xsl:if>

                <xsl:if test="$SpecialInstructions != ''">
                  <xsl:call-template name="MapNote">
                    <xsl:with-param name="Description" select="'Special Instructions'"/>
                    <xsl:with-param name="Value" select="$SpecialInstructions"/>
                  </xsl:call-template>
                </xsl:if>

                <xsl:if test="$TransportMode = 'ROA' and $TrailerNumbers != ''">
                  <xsl:call-template name="MapNote">
                    <xsl:with-param name="Description" select="'Trailer Number'"/>
                    <xsl:with-param name ="IsCustomDescription" select ="'true'"/>
                    <xsl:with-param name="Value" select="$TrailerNumbers"/>
                  </xsl:call-template>
                </xsl:if>

                <xsl:if test="string-length($GoodsDescription) &gt; 35">
                  <xsl:call-template name="MapNote">
                    <xsl:with-param name="Description" select="'Detailed Goods Description'"/>
                    <xsl:with-param name="Value" select="$GoodsDescriptionNodes"/>
                  </xsl:call-template>
                </xsl:if>
              </xsl:element>
            </xsl:if>

            <xsl:variable name="LocalClient">
              <xsl:choose>
                <xsl:when test ="//*[local-name()='N1Loop1']/*[local-name()='N1'][./*[local-name()='N101'] = 'BT']/*[local-name()='N104' and . != '']">
                  <xsl:value-of select ="//*[local-name()='N1Loop1']/*[local-name()='N1'][./*[local-name()='N101'] = 'BT']/*[local-name()='N104' and . != '']"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="ScriptNS1:GetRecipientCodeUnkeyed('TRXELPELP_E01' , 'TRXELPELP' , 'EMC 204 - Receive Shipments' , 'Defaults' , 'Local Client')"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>

            <xsl:element name ="OrganizationAddressCollection">
              <xsl:call-template name="PopulateOrgAddress">
                <xsl:with-param name="OrgDetails" select="$Shipper"/>
                <xsl:with-param name="OrgType" select="'ConsignorDocumentaryAddress'"/>
              </xsl:call-template>

              <xsl:call-template name="PopulateOrgAddress">
                <xsl:with-param name="OrgDetails" select="$Shipper"/>
                <xsl:with-param name="OrgType" select="'ConsignorPickupDeliveryAddress'"/>
              </xsl:call-template>

              <xsl:call-template name="PopulateOrgAddress">
                <xsl:with-param name="OrgDetails" select="$Consignee"/>
                <xsl:with-param name="OrgType" select="'ConsigneeDocumentaryAddress'"/>
              </xsl:call-template>

              <xsl:call-template name="PopulateOrgAddress">
                <xsl:with-param name="OrgDetails" select="$Consignee"/>
                <xsl:with-param name="OrgType" select="'ConsigneePickupDeliveryAddress'"/>
              </xsl:call-template>

              <xsl:element name="OrganizationAddress">
                <xsl:element name="AddressType">
                  <xsl:text>LocalClient</xsl:text>
                </xsl:element>
                <xsl:element name="OrganizationCode">
                  <xsl:value-of select ="$LocalClient"/>
                </xsl:element>
              </xsl:element>
            </xsl:element>

            <xsl:variable name ="PackLines" select ="$CL_S5Loop1/*[local-name()='LAD']"/>

            <xsl:if test ="count($PackLines) > 0">
              <xsl:element name ="PackingLineCollection">
                <xsl:for-each select ="$PackLines">
                  <xsl:element name ="PackingLine">

                    <xsl:call-template name="MapValueIfNotEmpty">
                      <xsl:with-param name="NodeName" select="'PackQty'" />
                      <xsl:with-param name="Value" select="*[local-name()='LAD02']" />
                    </xsl:call-template>

                    <xsl:element name ="PackType">
                      <xsl:element name ="Code">
                        <xsl:choose>
                          <xsl:when test ="*[local-name()='LAD01'] != ''">
                            <xsl:value-of select ="*[local-name()='LAD01'] "/>
                          </xsl:when>
                          <xsl:otherwise>
                            <xsl:value-of select ="$DefaultPackType"/>
                          </xsl:otherwise>
                        </xsl:choose>
                      </xsl:element>
                    </xsl:element>

                    <xsl:call-template name="MapValueIfNotEmpty">
                      <xsl:with-param name="NodeName" select="'GoodsDescription'" />
                      <xsl:with-param name="Value" select="*[local-name()='LAD12']" />
                    </xsl:call-template>

                  </xsl:element>
                </xsl:for-each>
              </xsl:element>
            </xsl:if>
          </xsl:element>
        </xsl:element>
      </xsl:element>

    </xsl:element>
  </xsl:template>

  <xsl:template name="PopulateOrgAddress">
    <xsl:param name="OrgDetails"/>
    <xsl:param name="OrgType"/>

    <xsl:if test="$OrgDetails != ''">
      <OrganizationAddress>
        <AddressType>
          <xsl:value-of select="$OrgType"/>
        </AddressType>

        <xsl:if test="$OrgType = 'ConsigneeDocumentaryAddress' or $OrgType = 'ConsigneePickupDeliveryAddress'">
          <AddressOverride>true</AddressOverride>
        </xsl:if>

        <xsl:variable name ="OrgCode">
          <xsl:choose>
            <xsl:when test ="$OrgDetails/*[local-name()='N1_2']/*[local-name()='N104'] != ''">
              <xsl:value-of select ="$OrgDetails/*[local-name()='N1_2']/*[local-name()='N104']"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select ="$OrgDetails/*[local-name()='N1_2']/*[local-name()='N103']"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:call-template name="MapValueIfNotEmpty">
          <xsl:with-param name="NodeName" select="'OrganizationCode'" />
          <xsl:with-param name="Value" select="$OrgCode" />
        </xsl:call-template>

        <xsl:call-template name="MapValueIfNotEmpty">
          <xsl:with-param name="NodeName" select="'CompanyName'" />
          <xsl:with-param name="Value" select="$OrgDetails/*[local-name()='N1_2']/*[local-name()='N102']" />
        </xsl:call-template>

        <xsl:call-template name="MapValueIfNotEmpty">
          <xsl:with-param name="NodeName" select="'Address1'" />
          <xsl:with-param name="Value" select="$OrgDetails/*[local-name()='N3_2']/*[local-name()='N301']" />
        </xsl:call-template>

        <xsl:call-template name="MapValueIfNotEmpty">
          <xsl:with-param name="NodeName" select="'Address2'" />
          <xsl:with-param name="Value" select="$OrgDetails/*[local-name()='N3_2']/*[local-name()='N302']" />
        </xsl:call-template>

        <xsl:call-template name="MapValueIfNotEmpty">
          <xsl:with-param name="NodeName" select="'City'" />
          <xsl:with-param name="Value" select="$OrgDetails/*[local-name()='N4_2']/*[local-name()='N401']" />
        </xsl:call-template>

        <xsl:call-template name="MapValueIfNotEmpty">
          <xsl:with-param name="NodeName" select="'State'" />
          <xsl:with-param name="Value" select="$OrgDetails/*[local-name()='N4_2']/*[local-name()='N402']" />
        </xsl:call-template>

        <xsl:call-template name="MapValueIfNotEmpty">
          <xsl:with-param name="NodeName" select="'Postcode'" />
          <xsl:with-param name="Value" select="$OrgDetails/*[local-name()='N4_2']/*[local-name()='N403']" />
        </xsl:call-template>

        <xsl:call-template name="MapValueIfNotEmpty">
          <xsl:with-param name="NodeName" select="'Country/Code'" />
          <xsl:with-param name="Value" select="$OrgDetails/*[local-name()='N4_2']/*[local-name()='N404']" />
        </xsl:call-template>

        <xsl:variable name="ContactName" select="$OrgDetails/*[local-name()='G61_2'][./*[local-name()='G6101'] = 'RE']/*[local-name()='G6102']"/>
        <xsl:variable name="Phone" select="$OrgDetails/*[local-name()='G61_2'][./*[local-name()='G6103'] = 'TE']/*[local-name()='G6104']"/>

        <xsl:choose>
          <xsl:when test="$OrgType = 'ConsigneeDocumentaryAddress' or $OrgType = 'ConsigneePickupDeliveryAddress'">
            <xsl:call-template name="MapValueIfNotEmpty">
              <xsl:with-param name="NodeName" select="'Contact'" />
              <xsl:with-param name="Value" select="$ContactName" />
            </xsl:call-template>

            <xsl:call-template name="MapValueIfNotEmpty">
              <xsl:with-param name="NodeName" select="'Phone'" />
              <xsl:with-param name="Value" select="$Phone" />
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name="MapValueIfNotEmpty">
              <xsl:with-param name="NodeName" select="'Contact'" />
              <xsl:with-param name="Value">
                <xsl:value-of select="$ContactName"/>
                <xsl:if test="$Phone != ''">
                  <xsl:value-of select="concat(' TEL: ', $Phone)"/>
                </xsl:if>
              </xsl:with-param>
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </OrganizationAddress>
    </xsl:if>

  </xsl:template>

  <xsl:template name ="Select_G62Date">
    <xsl:param name="Date1" />
    <xsl:param name="Date2" />
    <xsl:if test="$Date1 or $Date2">
      <xsl:choose>
        <xsl:when test ="$Date1/*[local-name()='G6202'] != '' ">
          <xsl:value-of select ="ScriptNS0:ConvertToDateTimeString($Date1/*[local-name()='G6202'], 'yyyyMMdd', $Date1/*[local-name()='G6204'], 'HHmm')"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="ScriptNS0:ConvertToDateTimeString($Date2/*[local-name()='G6202'], 'yyyyMMdd', $Date2/*[local-name()='G6204'], 'HHmm')"/>
        </xsl:otherwise>
      </xsl:choose>
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

  <xsl:template name="MapNote">

    <xsl:param name="Description" />
    <xsl:param name="IsCustomDescription" select="'false'" />
    <xsl:param name="Value" />

    <xsl:if test="count($Value) > 0">
      <xsl:element name="Note">

        <xsl:element name="Description">
          <xsl:value-of select="$Description"/>
        </xsl:element>

        <xsl:element name="IsCustomDescription">
          <xsl:value-of select="$IsCustomDescription"/>
        </xsl:element>

        <xsl:element name="NoteText">
          <xsl:for-each select="$Value">
            <xsl:value-of select="." />
            <xsl:if test="position() != last()">
              <xsl:text>&#xD;&#xA;</xsl:text>
            </xsl:if>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[

public string JoinLines(XPathNodeIterator nodes)
{
  var builder = new StringBuilder();
  while (nodes.MoveNext())
  {
    builder.AppendLine(nodes.Current.Value);
  }
  return builder.ToString().Trim();
}

    ]]>
  </msxsl:script>
</xsl:stylesheet>

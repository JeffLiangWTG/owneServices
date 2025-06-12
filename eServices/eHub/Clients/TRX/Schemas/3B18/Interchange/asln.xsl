<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns="http://xyz/"
    xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="xs" version="2.0">    
    <xsl:template match="/">
        <xsl:variable name=" timezone" select="/ROWSET/ROW/TIMEZONE"/>
        <tns:ShippingDocumentationNotification  xmlns:tns="urn:rosettanet:specification:interchange:ShippingDocumentationNotification:xsd:schema:02.07" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:ssdh="urn:rosettanet:specification:system:StandardDocumentHeader:xsd:schema:01.23" xmlns:upi="urn:rosettanet:specification:universal:PartnerIdentification:xsd:schema:01.16" xmlns:ulc="urn:rosettanet:specification:universal:Locations:xsd:schema:01.04" xmlns:udct="urn:rosettanet:specification:universal:DocumentType:xsd:codelist:01.13" xmlns:udc="urn:rosettanet:specification:universal:Document:xsd:schema:01.12" xmlns:dl="urn:rosettanet:specification:domain:Logistics:xsd:schema:02.22" xmlns:ume="urn:rosettanet:specification:universal:MonetaryExpression:xsd:schema:01.06" xmlns:ucr="urn:rosettanet:specification:universal:Currency:xsd:codelist:01.03" xmlns:sha="urn:rosettanet:specification:domain:Shared:xsd:schema:01.17" xmlns:uc="urn:rosettanet:specification:universal:Country:xsd:codelist:01.02" xmlns:dcst="urn:rosettanet:specification:domain:Logistics:CustomsType:xsd:codelist:01.03" xmlns:uci="urn:rosettanet:specification:universal:ContactInformation:xsd:schema:01.04" xmlns:ucs="urn:rosettanet:specification:universal:CountrySubdivision:xsd:codelist:01.02" xmlns:upri="urn:rosettanet:specification:universal:ProcessRoleIdentifier:xsd:codelist:01.11" xmlns:upd="urn:rosettanet:specification:universal:PhysicalDimension:xsd:schema:01.07" xmlns:rat="urn:rosettanet:specification:domain:Shared:AmountType:xsd:codelist:01.03" xmlns:dic="urn:rosettanet:specification:domain:Logistics:Incoterms:xsd:codelist:01.03" xmlns:dpts="urn:rosettanet:specification:domain:Procurement:PaymentTerms:xsd:codelist:01.04" xmlns:dp="urn:rosettanet:specification:domain:Procurement:xsd:schema:02.28" xmlns:dfpt="urn:rosettanet:specification:domain:Logistics:FreightPaymentTerms:xsd:codelist:01.03" xmlns:drl="urn:rosettanet:specification:domain:Logistics:RouteLocation:xsd:codelist:01.03" xmlns:dtrt="urn:rosettanet:specification:domain:Logistics:TrackingReferenceType:xsd:codelist:01.06" xmlns:updi="urn:rosettanet:specification:universal:ProductIdentification:xsd:schema:01.04" xmlns:uuom="urn:rosettanet:specification:universal:UnitOfMeasure:xsd:codelist:01.04" xmlns:dsd="urn:rosettanet:specification:domain:Logistics:ShippingDocument:xsd:codelist:01.02"  xmlns:pfx24="http://www.tibco.com/xmlns/ae2xsd/2002/05/ae/ADB/OTMflex" xmlns:dat="urn:rosettanet:specification:domain:Procurement:ActionType:xsd:codelist:01.04" 
           xmlns:dsm="urn:rosettanet:specification:domain:Logistics:ShipmentMode:xsd:codelist:01.05"
           xmlns:rssl="urn:rosettanet:specification:domain:Shared:ShippingServiceLevel:xsd:codelist:01.01"
            xmlns:dsh="urn:rosettanet:specification:domain:Procurement:SpecialHandling:xsd:codelist:01.04">
    <ssdh:DocumentHeader>
        <ssdh:DocumentInformation>
            <ssdh:Creation>  
                <xsl:if test="contains($timezone,'-')  and contains(substring($timezone,5,2),'0') and contains(substring($timezone,2,2),'0')">
                    <xsl:variable name=" zone" select="normalize-space(concat('-PT',substring($timezone,3,1),'H'))"/>
                    <xsl:value-of select="format-dateTime(adjust-dateTime-to-timezone(current-dateTime(),xs:dayTimeDuration($zone)),'[Y0001]-[M01]-[D01]T[h01]:[m01]:[s01][Z]')"/>
                </xsl:if>
                <xsl:if test="not(contains($timezone,'-')  and contains(substring($timezone,5,2),'0') and contains(substring($timezone,2,2),'0'))">
                    <xsl:variable name=" zone" select="normalize-space(concat('PT',substring($timezone,2,1),'H',substring($timezone,4,2),'M' ))"/>
                    <xsl:value-of select="format-dateTime(adjust-dateTime-to-timezone(current-dateTime(),xs:dayTimeDuration($zone)),'[Y0001]-[M01]-[D01]T[h01]:[m01]:[s01][Z]')"/>
                </xsl:if>
               
             </ssdh:Creation>
            <ssdh:DocumentIdentification>
                <ssdh:Identifier>
                    <xsl:value-of select="format-number(ROWSET/ROW/MESSAGE_3B18_ID,'#')"/>
                </ssdh:Identifier>
                <ssdh:StandardDocumentIdentification>
                    <ssdh:Standard>
                        <xsl:value-of select="&quot;RosettaNet&quot;"/>
                    </ssdh:Standard>
                    <ssdh:Version>
                        <xsl:value-of select="'11.11'"/>
                    </ssdh:Version>
                </ssdh:StandardDocumentIdentification>
            </ssdh:DocumentIdentification>
        </ssdh:DocumentInformation>
        <ssdh:Receiver>
            <upi:PartnerIdentification>
                <xsl:if test="not((ROWSET/ROW/PARTNER_CODE=(&quot;true&quot;)) or (ROWSET/ROW/PARTNER_CODE/@xsi:nil=(&quot;1&quot;)))">
                    <upi:PartnerName>
                        <xsl:value-of select="ROWSET/ROW/PARTNER_CODE"/>
                    </upi:PartnerName>
                </xsl:if>
                <DUNS xmlns="urn:rosettanet:specification:universal:DataType:xsd:schema:01.04"> 
                    <xsl:value-of select="ROWSET/ROW/PARTNER_DUNS_NUMBER"/>
                </DUNS>
                </upi:PartnerIdentification>
        </ssdh:Receiver>
        <ssdh:Sender>
            <upi:PartnerIdentification>
                <upi:PartnerName>
                    <xsl:value-of select="'CISCO'"/>
                </upi:PartnerName>
                <DUNS xmlns="urn:rosettanet:specification:universal:DataType:xsd:schema:01.04">
                    <xsl:value-of select="ROWSET/ROW/CISCO_DUNS_NUMBER"/>
                </DUNS>
                </upi:PartnerIdentification>
        </ssdh:Sender>
    </ssdh:DocumentHeader>
    <tns:ShippingBusinessDocument>
        <xsl:if test="not((ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/ACTION_TYPE=(&quot;true&quot;)) or (ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/ACTION_TYPE/@xsi:nil=(&quot;1&quot;)))">
        <dat:ActionType>
            <xsl:value-of select="ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/ACTION_TYPE"/>
        </dat:ActionType>
        </xsl:if>
        <tns:HeaderInformation>
           
            <tns:CommercialInvoice>
                <udct:DocumentType>
                    <xsl:value-of select="'CIN'"/>
                </udct:DocumentType>
                <!-- Mrinmay Start -->
                
                <xsl:choose>     
                    <xsl:when test="exists(/ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/INTERCOMPANY_INV)">
                        <udc:Identifier>
                            <xsl:value-of select="/ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/INTERCOMPANY_INV"/>
                        </udc:Identifier>
                    </xsl:when>
                    <xsl:otherwise>
                        <udc:Identifier>
                            <xsl:value-of select="'NONE'"/>
                        </udc:Identifier>
                    </xsl:otherwise>
                </xsl:choose>

                <xsl:if test="/ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/PR_BATCH_NUMBER">
                    <udc:Line>
                        <xsl:value-of select="format-number(/ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/PR_BATCH_NUMBER,'#')"/>
                    </udc:Line>
                </xsl:if>
                <xsl:if test="exists(/ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/PRINT_INDICATOR)">
                    <udc:Revision>
                        <xsl:value-of select="/ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/PRINT_INDICATOR"/>
                    </udc:Revision>
                </xsl:if>
                <xsl:if test="exists(/ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/PICK_SLIP_NUMBER)">
                    <udc:SubLine>
                        <xsl:value-of select="format-number(/ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/PICK_SLIP_NUMBER,'#')"/>
                    </udc:SubLine>
                </xsl:if>
            </tns:CommercialInvoice>
            
            <!-- Mrinmay end -->

            <tns:ShippingDocument>    
                <udc:DateTime>
                    <!--   <xsl:value-of select=" format-dateTime(ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/ORDER_DATE,' YYYY-MM-DDThh:mm:ssTZD' )"/> -->
                    <!--<xsl:value-of select="normalize-space(concat(substring(normalize-space(
                        ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/ORDER_DATE),1,10),'T',substring(normalize-space(
                        ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/ORDER_DATE),12)))"/>-->
                    <!--Mrinmay-->
                    
                    <xsl:if test="contains($timezone,'-')  and contains(substring($timezone,5,2),'0') and contains(substring($timezone,2,2),'0')">
                        <xsl:variable name=" zone" select="normalize-space(concat('-PT',substring($timezone,3,1),'H'))"/>
                        <xsl:value-of select="adjust-dateTime-to-timezone(xs:dateTime(normalize-space(concat(substring(normalize-space(ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/ORDER_DATE),1,10),'T',substring(normalize-space(ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/ORDER_DATE),12)))),xs:dayTimeDuration($zone))"></xsl:value-of> 
                    </xsl:if>
                    <xsl:if test="not(contains($timezone,'-')  and contains(substring($timezone,5,2),'0') and contains(substring($timezone,2,2),'0'))">
                        <xsl:variable name=" zone" select="normalize-space(concat('PT',substring($timezone,2,1),'H',substring($timezone,4,2),'M' ))"/>
                        <xsl:value-of select="adjust-dateTime-to-timezone(xs:dateTime(normalize-space(concat(substring(normalize-space(ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/ORDER_DATE),1,10),'T',substring(normalize-space(ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/ORDER_DATE),12)))),xs:dayTimeDuration($zone))"></xsl:value-of> 
                    </xsl:if>
                             
                    <!--Mrinmay End-->
                </udc:DateTime>
                <udct:DocumentType>
                    <xsl:value-of select="'SAO'"/>
                </udct:DocumentType>
                <xsl:if test="not((ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/ORDER_NUMBER/@xsi:nil=(&quot;true&quot;)) or (ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/ORDER_NUMBER/@xsi:nil=(&quot;1&quot;)))">
                <udc:Identifier>
                    <xsl:value-of select="format-number(ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/ORDER_NUMBER,'#')"/>
                </udc:Identifier>
                </xsl:if>
                <xsl:if test="not((ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/SHIP_SET_NUMBER/@xsi:nil=(&quot;true&quot;)) or (ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/SHIP_SET_NUMBER/@xsi:nil=(&quot;1&quot;)))">
                    <udc:Line>
                        <xsl:value-of select="format-number(ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/SHIP_SET_NUMBER,'#')"/>
                    </udc:Line>
                </xsl:if>
                <xsl:if test="not((ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/ORDER_TYPE_NAME/@xsi:nil=(&quot;true&quot;)) or (ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/ORDER_TYPE_NAME/@xsi:nil=(&quot;1&quot;)))">
                    <udc:Revision>
                        <xsl:value-of select="ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/ORDER_TYPE_NAME"/>
                    </udc:Revision>
                </xsl:if>
            </tns:ShippingDocument>
            <tns:ShippingOrderInformation>
                <dl:OrderInformation>
                    <ulc:AlternativeIdentifier>
                        <ulc:Authority>
                            <xsl:value-of select="'Cisco Systems'"/>
                        </ulc:Authority>
				<!-- wilson Start -->
                <xsl:if test="not((ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/SHIPMENT_PRIORITY_CODE/@xsi:nil=(&quot;true&quot;)) or (ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/SHIPMENT_PRIORITY_CODE/@xsi:nil=(&quot;1&quot;)))">
                    <ulc:Identifier>
                        <xsl:value-of select="ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/SHIPMENT_PRIORITY_CODE"/>
                    </ulc:Identifier>
                </xsl:if>
                <!-- wilson end -->
	</ulc:AlternativeIdentifier>
                    <dl:OrderAllocationDate>
                        <xsl:if test="contains($timezone,'-')  and contains(substring($timezone,5,2),'0') and contains(substring($timezone,2,2),'0')">
                            <xsl:variable name=" zone" select="normalize-space(concat('-PT',substring($timezone,3,1),'H'))"/>
                            <xsl:value-of select="adjust-dateTime-to-timezone(xs:dateTime(normalize-space(concat(substring(normalize-space(ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/PROMISE_DATE),1,10),'T',substring(normalize-space(ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/PROMISE_DATE),12)))),xs:dayTimeDuration($zone))"></xsl:value-of> 
                        </xsl:if>
                        <xsl:if test="not(contains($timezone,'-')  and contains(substring($timezone,5,2),'0') and contains(substring($timezone,2,2),'0'))">
                            <xsl:variable name=" zone" select="normalize-space(concat('PT',substring($timezone,2,1),'H',substring($timezone,4,2),'M' ))"/>
                            <xsl:value-of select="adjust-dateTime-to-timezone(xs:dateTime(normalize-space(concat(substring(normalize-space(ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/PROMISE_DATE),1,10),'T',substring(normalize-space(ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/PROMISE_DATE),12)))),xs:dayTimeDuration($zone))"></xsl:value-of> 
                        </xsl:if>
                    </dl:OrderAllocationDate>
                    <dl:OrderReference>
                        <udc:DateTime>
                        
                            <xsl:if test="contains($timezone,'-')  and contains(substring($timezone,5,2),'0') and contains(substring($timezone,2,2),'0')">
                                <xsl:variable name=" zone" select="concat('-PT',substring($timezone,3,1),'H')"/>
                                <xsl:value-of select="format-dateTime(adjust-dateTime-to-timezone(current-dateTime(),xs:dayTimeDuration($zone)),'[Y0001]-[M01]-[D01]T[h01]:[m01]:[s01][Z]')"/> 
                            </xsl:if>
                            <xsl:if test="not(contains($timezone,'-')  and contains(substring($timezone,5,2),'0') and contains(substring($timezone,2,2),'0'))">
                                <xsl:variable name=" zone" select="concat('PT',substring($timezone,2,1),'H',substring($timezone,4,2),'M' )"/>
                                <xsl:value-of select="format-dateTime(adjust-dateTime-to-timezone(current-dateTime(),xs:dayTimeDuration($zone)),'[Y0001]-[M01]-[D01]T[h01]:[m01]:[s01][Z]')"/>  
                            </xsl:if>
                        </udc:DateTime>
                        <udct:DocumentType>
                            <xsl:value-of select="'PUO'"/>
                        </udct:DocumentType>
                        <udc:Identifier>
                            <xsl:value-of select="ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/CUST_PO_NUMBER"/>
                        </udc:Identifier>
                    </dl:OrderReference>
                    <xsl:if test="exists(ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/ORDER_TOTAL)
                                       and exists(ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/CURRENCY_CODE)">
                    <dl:TotalAmount>
                        <ume:Amount>
                            <xsl:value-of select="ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/ORDER_TOTAL"/>
                        </ume:Amount>
                        <ucr:Currency>
                            <xsl:value-of select="ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/CURRENCY_CODE"/>
                        </ucr:Currency>
                    </dl:TotalAmount>
                    </xsl:if>
                </dl:OrderInformation>
				<!-- Wilson -->
<xsl:for-each select="ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/XXCMF_3B18_SHIPMENT_FRGHT_COMB/XXCMF_3B18_SHIPMENT_FRGHT_COMB_ROW">
<xsl:sort select="HEADER_SEQ"/>
                <tns:RequestingOrderInformation>
				<!-- Wilson -->
                    <udc:BusinessDocumentReference>
                        <udct:DocumentType>
                            <xsl:value-of select="'POO'"/>
                        </udct:DocumentType>
										<!-- wilson Start -->
                <xsl:if test="not((ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/INT_COMP_PO_NUM/@xsi:nil=(&quot;true&quot;)) or (ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/INT_COMP_PO_NUM/@xsi:nil=(&quot;1&quot;)))">
                    <udc:Identifier>
                        <xsl:value-of select="/ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/INT_COMP_PO_NUM"/>
                    </udc:Identifier>
                </xsl:if>
                <!-- wilson end -->
                    </udc:BusinessDocumentReference>
         
                    <!-- Mrinmay-->
                    <xsl:if test="/ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/MERGE_FLAG='F'">								<!-- wilson Start -->
                    <tns:IsOrderToBeMerged>
                        <xsl:value-of select="/ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/MERGE_FLAG/translate('F','F','0')"/>
                    </tns:IsOrderToBeMerged>
                    </xsl:if>
                    <xsl:if test="/ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/MERGE_FLAG='T'">								<!-- wilson Start -->
                        <tns:IsOrderToBeMerged>
                            <xsl:value-of select="/ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/MERGE_FLAG/translate('T','T','1')"/>
                        </tns:IsOrderToBeMerged>
                    </xsl:if>
                    <!-- Mrinmay end-->
                <!-- wilson end -->
                    <xsl:if test="exists(LOAD_ID) and exists(SHIPSET_COUNT) and exists(SHIPSET_SEQUENCE) and exists(HEADER_SEQ)">
                    <tns:OrderReference>
                        <udc:DateTime>
                          
                            <xsl:if test="contains($timezone,'-')  and contains(substring($timezone,5,2),'0') and contains(substring($timezone,2,2),'0')">
                                <xsl:variable name=" zone" select="normalize-space(concat('-PT',substring($timezone,3,1),'H'))"/>
                                <xsl:value-of select="format-dateTime(adjust-dateTime-to-timezone(current-dateTime(),xs:dayTimeDuration($zone)),'[Y0001]-[M01]-[D01]T[h01]:[m01]:[s01][Z]')"/> 
                            </xsl:if>
                            <xsl:if test="not(contains($timezone,'-')  and contains(substring($timezone,5,2),'0') and contains(substring($timezone,2,2),'0'))">
                                <xsl:variable name=" zone" select="normalize-space(concat('PT',substring($timezone,2,1),'H',substring($timezone,4,2),'M' ))"/>
                                <xsl:value-of select="format-dateTime(adjust-dateTime-to-timezone(current-dateTime(),xs:dayTimeDuration($zone)),'[Y0001]-[M01]-[D01]T[h01]:[m01]:[s01][Z]')"/> 
                            </xsl:if>
                        </udc:DateTime>
                        <udct:DocumentType>
                            <xsl:value-of select="'SNC'"/>
                        </udct:DocumentType>
                        <xsl:if test="not((LOAD_ID/@xsi:nil=(&quot;true&quot;)) or (LOAD_ID/@xsi:nil=(&quot;1&quot;)))">
                        <udc:Identifier>
                            <xsl:value-of select="LOAD_ID"/>
                        </udc:Identifier>
                        </xsl:if>
                        <xsl:if test="not((SHIPSET_COUNT/@xsi:nil=(&quot;true&quot;)) or (SHIPSET_COUNT/@xsi:nil=(&quot;1&quot;)))">
                            <udc:Line>
                                <xsl:value-of select="format-number(SHIPSET_COUNT,'#')"/>
                            </udc:Line>
                        </xsl:if>
                        <xsl:if test="not((SHIPSET_SEQUENCE/@xsi:nil=(&quot;true&quot;)) or (SHIPSET_SEQUENCE/@xsi:nil=(&quot;1&quot;)))">
                            <udc:Revision>
                                <xsl:value-of select="format-number(SHIPSET_SEQUENCE,'#')"/>
                            </udc:Revision>
                        </xsl:if>
                        <xsl:if test="not((SHIPSET_SEQUENCE/@xsi:nil=(&quot;true&quot;)) or (HEADER_SEQ/@xsi:nil=(&quot;1&quot;)))">
                            <udc:SubLine>
                                <xsl:value-of select="format-number(HEADER_SEQ,'#')"/>
                            </udc:SubLine>
                        </xsl:if>
                    </tns:OrderReference>
                    </xsl:if>
                </tns:RequestingOrderInformation>
</xsl:for-each>
            </tns:ShippingOrderInformation>
        </tns:HeaderInformation>
        <tns:ShipmentInformation>
            
            <tns:ContainerTotalCount>
                <xsl:value-of select="number(ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/CARTON_COUNT)"/>
            </tns:ContainerTotalCount>
            <dl:CustomsInformation>
                <dl:Customs>
                 
															<!-- wilson Start -->
                <xsl:if test="not((ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/PROVENANCE_COUNTRY/@xsi:nil=(&quot;true&quot;)) or (ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/PROVENANCE_COUNTRY/@xsi:nil=(&quot;1&quot;)))">
                    <uc:Country>
                        <xsl:value-of select="ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/PROVENANCE_COUNTRY"/>
                    </uc:Country>
                </xsl:if>
                <!-- wilson end -->
                    <dcst:CustomsType>
                        <xsl:value-of select="'EXP'"/>
                    </dcst:CustomsType>
                    <xsl:if test="not((ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/EXPORT_LICENSE_NUMBER/@xsi:nil=(&quot;true&quot;)) or (ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/EXPORT_LICENSE_NUMBER/@xsi:nil=(&quot;1&quot;)))">
                        <dl:EntryNumber>
                            <xsl:value-of select="ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/EXPORT_LICENSE_NUMBER"/>
                        </dl:EntryNumber>
                    </xsl:if>
                </dl:Customs>
                <dl:Customs>
     
															<!-- wilson Start -->
                <xsl:if test="not((ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/ACQUISITION_COUNTRY/@xsi:nil=(&quot;true&quot;)) or (ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/ACQUISITION_COUNTRY/@xsi:nil=(&quot;1&quot;)))">
                    <uc:Country>
                        <xsl:value-of select="ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/ACQUISITION_COUNTRY"/>
                    </uc:Country>
                </xsl:if>
                <!-- wilson end -->
                    <dcst:CustomsType>
                        <xsl:value-of select="'IMP'"/>
                    </dcst:CustomsType>
                </dl:Customs>
            </dl:CustomsInformation>
            <tns:DeclarationInformation>
                <tns:Declarant>
                    <upi:SpecifiedFullPartner>
                        <xsl:attribute name="schemaVersion">
                            <xsl:value-of select="'this choice is requried'"/>
                        </xsl:attribute>
                    </upi:SpecifiedFullPartner>
                </tns:Declarant>
                <!--Mrinmay-->
                <xsl:if test="ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/PDI_INDICATOR='F' ">
                <tns:IsInformationComplete>
                    <xsl:value-of select="ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/PDI_INDICATOR/translate('F','F','0')"/>
                </tns:IsInformationComplete>
                </xsl:if>
                <xsl:if test="ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/PDI_INDICATOR='T' ">
                    <tns:IsInformationComplete>
                        <xsl:value-of select="ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/PDI_INDICATOR/translate('T','T','1')"/>
                    </tns:IsInformationComplete>
                </xsl:if>
                <!--Mrinmay end-->
            <xsl:for-each select="ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/XXCMF_3B18_SHIPMENT_DETAILS/XXCMF_3B18_SHIPMENT_DETAILS_ROW">
                <upi:PartnerDescription>
                    <upi:FullPartner>
                        <uci:ContactInformation>
                            <uci:Contact>
                                <xsl:value-of select="CONTACT_NAME"/>
                            </uci:Contact>
                            <xsl:if test="not((PHONE_NUMBER/@xsi:nil=(&quot;true&quot;)) or (PHONE_NUMBER/@xsi:nil=(&quot;1&quot;)))">
                                <uci:Phone>
                                    <xsl:value-of select="PHONE_NUMBER"/>
                                </uci:Phone>
                            </xsl:if>
                        </uci:ContactInformation>
                        <ulc:Location>
                            <ulc:AlternativeIdentifier>
                                <ulc:Authority/>
                                <ulc:Identifier>
                                    <xsl:value-of select="LOCATION_IDENTIFIER"/>
                                </ulc:Identifier>
                            </ulc:AlternativeIdentifier>
                        </ulc:Location>
                        <upi:PartnerIdentification>
                            <xsl:if test="not((ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/XXCMF_3B18_SHIPMENT_DETAILS/XXCMF_3B18_SHIPMENT_DETAILS_ROW/NAME/@xsi:nil=(&quot;true&quot;)) or (ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/XXCMF_3B18_SHIPMENT_DETAILS/XXCMF_3B18_SHIPMENT_DETAILS_ROW/NAME/@xsi:nil=(&quot;1&quot;)))">
                                <upi:PartnerName>
                                    <xsl:value-of select="NAME"/>
                                </upi:PartnerName>
                            </xsl:if>
                            <ulc:AlternativeIdentifier>
                                <ulc:Authority>
                                    <xsl:value-of select="'Cisco Systems'"/>
                                </ulc:Authority>
                                <ulc:Identifier>
                                    <xsl:value-of select="ALTERNATE_IDENTIFIER"/>
                                </ulc:Identifier>
                            </ulc:AlternativeIdentifier>
                        </upi:PartnerIdentification>
                        <ulc:PhysicalAddress>
                            <ulc:AddressLine1>
                                <xsl:value-of select="ADDRESS_LINE1"/>
                            </ulc:AddressLine1>
                            <xsl:if test="not((ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/XXCMF_3B18_SHIPMENT_DETAILS/XXCMF_3B18_SHIPMENT_DETAILS_ROW/ADDRESS_LINE2/@xsi:nil=(&quot;true&quot;)) or (ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/XXCMF_3B18_SHIPMENT_DETAILS/XXCMF_3B18_SHIPMENT_DETAILS_ROW/ADDRESS_LINE2/@xsi:nil=(&quot;1&quot;)))">
                                <ulc:AddressLine2>
                                    <xsl:value-of select="ADDRESS_LINE2"/>
                                </ulc:AddressLine2>
                            </xsl:if>
                            <xsl:if test="not((ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/XXCMF_3B18_SHIPMENT_DETAILS/XXCMF_3B18_SHIPMENT_DETAILS_ROW/ADDRESS_LINE3/@xsi:nil=(&quot;true&quot;)) or (ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/XXCMF_3B18_SHIPMENT_DETAILS/XXCMF_3B18_SHIPMENT_DETAILS_ROW/ADDRESS_LINE3/@xsi:nil=(&quot;1&quot;)))">
                                <ulc:AddressLine3>
                                    <xsl:value-of select="ADDRESS_LINE3"/>
                                </ulc:AddressLine3>
                            </xsl:if>
                            <xsl:if test="not((ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/XXCMF_3B18_SHIPMENT_DETAILS/XXCMF_3B18_SHIPMENT_DETAILS_ROW/ADDRESS_LINE4/@xsi:nil=(&quot;true&quot;)) or (ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/XXCMF_3B18_SHIPMENT_DETAILS/XXCMF_3B18_SHIPMENT_DETAILS_ROW/ADDRESS_LINE4/@xsi:nil=(&quot;1&quot;)))">
                                <ulc:AddressLine4>
                                    <xsl:value-of select="ADDRESS_LINE4"/>
                                </ulc:AddressLine4>
                            </xsl:if>
                            <ulc:AddressLine5/>
                            <ulc:CityName>
                                <xsl:value-of select="CITY"/>
                            </ulc:CityName>
                            <uc:Country>
                                <xsl:value-of select="COUNTRY"/>
                            </uc:Country>
                            <xsl:if test="not( (ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/XXCMF_3B18_SHIPMENT_DETAILS/XXCMF_3B18_SHIPMENT_DETAILS_ROW/STATE/@xsi:nil=(&quot;true&quot;)) or (ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/XXCMF_3B18_SHIPMENT_DETAILS/XXCMF_3B18_SHIPMENT_DETAILS_ROW/STATE/@xsi:nil=(&quot;1&quot;)))">
                                <ucs:CountrySubdivision>
                                    <xsl:value-of select="STATE"/>
                                </ucs:CountrySubdivision>
                            </xsl:if>
                            <xsl:if test="not((ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/XXCMF_3B18_SHIPMENT_DETAILS/XXCMF_3B18_SHIPMENT_DETAILS_ROW/ZIP/@xsi:nil=(&quot;true&quot;)) or (ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/XXCMF_3B18_SHIPMENT_DETAILS/XXCMF_3B18_SHIPMENT_DETAILS_ROW/ZIP/@xsi:nil=(&quot;1&quot;)))">
                                <ulc:PostalCode>
                                    <xsl:value-of select="ZIP"/>
                                </ulc:PostalCode>
                            </xsl:if>
                            <ulc:PostOfficeBox/>
                        </ulc:PhysicalAddress>
                        <upri:ProcessRoleIdentifier>
                            <xsl:value-of select="ROLL_TYPE"/>
                        </upri:ProcessRoleIdentifier>
                    </upi:FullPartner>
                </upi:PartnerDescription>
                </xsl:for-each>
				</tns:DeclarationInformation>
            <tns:FreightTotalPhysicalDimension>
                <dl:MassPhysicalDimension>
                    <upd:Weight>
                        <upd:UnitOfMeasure>
                            <xsl:value-of select="'PON'"/>
                        </upd:UnitOfMeasure>
                        <upd:Value>
                            <xsl:value-of select="ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/TOTAL_WEIGHT"/>
                        </upd:Value>
                    </upd:Weight>
                </dl:MassPhysicalDimension>
            </tns:FreightTotalPhysicalDimension>
          
          
            <dl:FreightValuation>
                
                <xsl:if test="exists(/ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/FREIGHT_CHARGE)  ">
                    
                    <dl:DeclaredValue>
                        <rat:AmountType>
                            <xsl:value-of select="&quot;FRE&quot;"/>
                        </rat:AmountType>
                        <!--Mrinmay-->
                        <ume:FinancialAmount>
                            
                            <ume:Amount>
                                <xsl:value-of select="/ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/FREIGHT_CHARGE"/>
                            </ume:Amount>
                            
                            
                            <!--Mrinmay End-->
                            <ucr:Currency>
                                <xsl:value-of select="'USD'"/>
                            </ucr:Currency>
                        </ume:FinancialAmount>
                    </dl:DeclaredValue>
                </xsl:if>
                
                
                <xsl:if test="not(string-length(normalize-space(/ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/INSURANCE_AMOUNT))= 0 )">
                    <dl:InsuredValue>
                        <rat:AmountType>
                            <xsl:value-of select="'INS'"/>
                        </rat:AmountType>
                        <ume:FinancialAmount>
                            <ume:Amount>
                                <xsl:value-of select="/ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/INSURANCE_AMOUNT"/>
                            </ume:Amount>
                            <ucr:Currency>
                                <xsl:value-of select="/ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/CURRENCY_CODE"/>
                            </ucr:Currency>
                        </ume:FinancialAmount>
                    </dl:InsuredValue>
                </xsl:if>
                <!--Mrinmay-->
            </dl:FreightValuation>

              
            <xsl:if test="not((ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/XXCMF_3B18_SHIPMENT_FRGHT_COMB/XXCMF_3B18_SHIPMENT_FRGHT_COMB_ROW/INCO_TERM/@xsi:nil=(&quot;true&quot;)) or (ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/XXCMF_3B18_SHIPMENT_FRGHT_COMB/XXCMF_3B18_SHIPMENT_FRGHT_COMB_ROW/INCO_TERM/@xsi:nil=(&quot;1&quot;)))">
                <xsl:if test="not(ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/XXCMF_3B18_SHIPMENT_FRGHT_COMB/XXCMF_3B18_SHIPMENT_FRGHT_COMB_ROW[1]/INCO_TERM=ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/XXCMF_3B18_SHIPMENT_FRGHT_COMB/XXCMF_3B18_SHIPMENT_FRGHT_COMB_ROW[2]/INCO_TERM)">
                    <xsl:for-each select="ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/XXCMF_3B18_SHIPMENT_FRGHT_COMB/XXCMF_3B18_SHIPMENT_FRGHT_COMB_ROW">
                        <xsl:if test="not(string-length(normalize-space(current()/INCO_TERM))= 0 )">
                        <dic:Incoterms>
                    <xsl:value-of select="INCO_TERM"/>
                        </dic:Incoterms>
                        </xsl:if>
                </xsl:for-each>
                </xsl:if>
           
            <xsl:if test="(ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/XXCMF_3B18_SHIPMENT_FRGHT_COMB/XXCMF_3B18_SHIPMENT_FRGHT_COMB_ROW[1]/INCO_TERM=ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/XXCMF_3B18_SHIPMENT_FRGHT_COMB/XXCMF_3B18_SHIPMENT_FRGHT_COMB_ROW[2]/INCO_TERM)">
                     <dic:Incoterms>
                         <xsl:value-of select="ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/XXCMF_3B18_SHIPMENT_FRGHT_COMB/XXCMF_3B18_SHIPMENT_FRGHT_COMB_ROW[1]/INCO_TERM"/>
                    </dic:Incoterms>
            </xsl:if>
            </xsl:if>
            <!--Mrinmay End-->
            
            <!--Change for OTM 6.0-->
            
            
	                    <xs:HandbookIdentifier>
	                    
	                    <xsl:value-of select="ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/DUTY_ATTRIBUTE"/>
	                    
	                    </xs:HandbookIdentifier>


            
            
            <!--Change for OTM 6.0-->
            
           
            <xsl:for-each select="ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/XXCMF_3B18_SHIPMENT_NOTES/XXCMF_3B18_SHIPMENT_NOTES_ROW">
                <dl:Instructions>
                    <dl:Notes>
                        <xsl:value-of select="NOTES"/>
                    </dl:Notes>
                    <dl:ShippingInstructionsCode>
                        <xsl:value-of select="TYPE"/>
                    </dl:ShippingInstructionsCode>
                </dl:Instructions>
            </xsl:for-each>
            <!--  Mrinmay  -->
            <xsl:if test="ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/INSURANCE_REQUIRED='F'">
                <tns:IsInsuranceRequired>
                    <xsl:value-of select="ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/INSURANCE_REQUIRED /translate('F','F','0')"/>
                </tns:IsInsuranceRequired>
            </xsl:if>
            <xsl:if test="ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/INSURANCE_REQUIRED='T'">
                <tns:IsInsuranceRequired>
                    <xsl:value-of select="ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/INSURANCE_REQUIRED /translate('T','T','1')"/>
                </tns:IsInsuranceRequired>
            </xsl:if>
            
            <xsl:if test="ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/INTRA_COMPANY_TRANSFER_FLAG='F'">
                <tns:IsIntraCompanyTransfer>
                    <xsl:value-of select="ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/INTRA_COMPANY_TRANSFER_FLAG /translate('F','F','0')"/>
                </tns:IsIntraCompanyTransfer>
            </xsl:if>
            <xsl:if test="ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/INTRA_COMPANY_TRANSFER_FLAG='T'">
                <tns:IsIntraCompanyTransfer>
                    <xsl:value-of select="ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/INTRA_COMPANY_TRANSFER_FLAG /translate('T','T','1')"/>
                </tns:IsIntraCompanyTransfer>
            </xsl:if>
            
            <!--  Mrinmay  End -->
            <dpts:PaymentTerms>
                <xsl:value-of select="ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/PAYMENT_TERMS_NAME"/>
            </dpts:PaymentTerms>
            <xsl:if test="exists(ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/CURRENCY_CONV_RATE)
                and  exists(ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/CURRENCY_CODE)
                and  exists(ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/TO_CURRENCY_CODE)">
            <tns:PricingPaymentInformation>
                <dp:CurrencyConversion>
                    <dp:Date>
                        <xsl:value-of select="format-date(current-date(),'[Y0001]-[M01]-[D01]')"/>
                    </dp:Date>
                    <dp:Factor>
                        <xsl:value-of select="ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/CURRENCY_CONV_RATE"/>
                    </dp:Factor>
                    <dp:FromCurrency>
                        <xsl:value-of select="ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/CURRENCY_CODE"/>
                    </dp:FromCurrency>
                    <dp:Source>
                        <xsl:value-of select="&quot;required&quot;"/>
                    </dp:Source>
                    <dp:ToCurrency>
                        <xsl:value-of select="ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/TO_CURRENCY_CODE"/>
                    </dp:ToCurrency>
                </dp:CurrencyConversion>
            </tns:PricingPaymentInformation>
            </xsl:if>
            
            
            
            <!--Changes for OTM 6.0-->
            <dp:PaymentOrder>
            
            <xs:AccountNumber>
            <xsl:value-of select="ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/ACCOUNT_NUMBER"/>
            
            </xs:AccountNumber>
  
            </dp:PaymentOrder>
            <!--Changes for OTM 6.0-->
            
            
            
            
            <xsl:for-each select="ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/XXCMF_3B18_SHIPMENT_FRGHT_COMB/XXCMF_3B18_SHIPMENT_FRGHT_COMB_ROW">
                <xsl:sort select="HEADER_SEQ"/>
                <tns:RoutingInformation>
                    <xsl:variable name="seq" select="HEADER_SEQ"/>
                    <xsl:if test="not((FREIGHT_TERMS/@xsi:nil=(&quot;true&quot;)) or (FREIGHT_TERMS/@xsi:nil=(&quot;1&quot;)))">
                        <tns:Description>
                            <xsl:value-of select="FREIGHT_TERMS"/>
                        </tns:Description>
                    </xsl:if>
                    <dfpt:FreightPaymentTerms>
                        <xsl:value-of select="FRT_PAY_MTHD"/>
                    </dfpt:FreightPaymentTerms>
                    <upi:PartnerDescription>
                        <upi:KnownPartnerContact>
                            <upi:KnownPartner>
                                <upi:PartnerIdentification>
                                    <ulc:AlternativeIdentifier>
                                        <ulc:Authority>
                                            <xsl:value-of select="&quot;National Motor Freight Traffic Association&quot;"/>
                                        </ulc:Authority>
                                        <ulc:Identifier>
                                            <xsl:value-of select="SCAC_CODE"/>
                                        </ulc:Identifier>
                                    </ulc:AlternativeIdentifier>
                                    <ulc:AlternativeIdentifier>
                                        <ulc:Authority>
                                            <xsl:value-of select="&quot;Cisco Systems&quot;"/>
                                        </ulc:Authority>
                                        <ulc:Identifier>
                                            <xsl:value-of select="SHIP_VIA"/>
                                        </ulc:Identifier>
                                    </ulc:AlternativeIdentifier>
                                </upi:PartnerIdentification>
                                <upri:ProcessRoleIdentifier>
                                    <xsl:value-of select="'ALT'"/>
                                </upri:ProcessRoleIdentifier>
                            </upi:KnownPartner>
                        </upi:KnownPartnerContact>
                    </upi:PartnerDescription>
                    <xsl:if test="not((NAMED_PLACE/@xsi:nil=(&quot;true&quot;)) or (NAMED_PLACE/@xsi:nil=(&quot;1&quot;)))">
                        <drl:RouteLocation>
                            <xsl:value-of select="'POO'"/>
                        </drl:RouteLocation>
                    </xsl:if>
                    <tns:SequenceNumber>
                        <xsl:value-of select="HEADER_SEQ"/>
                    </tns:SequenceNumber>
                    
                    <!-- Change in OTM 6.0-->
                    
                    <dsm:ShipmentMode>
                      
                      <xsl:value-of select="/ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/SHIPPING_PREFERENCE"/>
                    
                    </dsm:ShipmentMode>
                    
                    <rssl:ShippingServiceLevel>
                    
                      <xsl:value-of select="/ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/SERVICE_LEVEL"/>
                    
                    </rssl:ShippingServiceLevel>
                    
                    <!-- Change in OTM 6.0-->
                    
                    
                   <dp:SpecialHandlingInstruction>
                    <dsh:SpecialHandling>OTH</dsh:SpecialHandling>
                   <dp:Text>
                       <xsl:value-of select="DESTINATION_AIRPORT"/>
                      </dp:Text>
                   </dp:SpecialHandlingInstruction>
                    <xsl:if test="exists(TRUCK_NUMBER)">
                    <dl:TrackingReference>
                        <xsl:if test="HEADER_SEQ =1">
                            <xsl:if test="not((../../TRUCK_NUMBER/@xsi:nil=(&quot;true&quot;)) or (../../TRUCK_NUMBER/@xsi:nil=(&quot;1&quot;)))">
                                <dl:ShipmentTrackingIdentifier>
                                    <xsl:value-of select="../../TRUCK_NUMBER"/>
                                </dl:ShipmentTrackingIdentifier>
                            </xsl:if>
                        </xsl:if>
                        <xsl:if test="HEADER_SEQ =1">
                            <dtrt:TrackingReferenceType>
                                <xsl:value-of select="&quot;BKN&quot;"/>
                            </dtrt:TrackingReferenceType>
                        </xsl:if>
                    </dl:TrackingReference>
                    </xsl:if>
                </tns:RoutingInformation>
            </xsl:for-each>
        </tns:ShipmentInformation>
                   
        <xsl:for-each select="ROWSET/ROW/XXCMF_3B18_SHIPMENT_HEADERS/XXCMF_3B18_SHIPMENT_HEADERS_ROW/XXCMF_3B18_SHIPMENT_LINES/XXCMF_3B18_SHIPMENT_LINES_ROW">
                 <xsl:sort select=" SHIPMENT_LINE_ID" />
                
            
            <tns:ShipmentLineItem>
                <udc:BusinessDocumentReference>
                    <udct:DocumentType>
                        <xsl:value-of select="&quot;POI&quot;"/>
                    </udct:DocumentType>
                    <udc:Identifier>
                        <xsl:value-of select="../../CUST_PO_NUMBER"/>
                    </udc:Identifier>
                    <xsl:if test="not((PO_LINE_NUMBER/@xsi:nil=(&quot;true&quot;)) or (PO_LINE_NUMBER/@xsi:nil=(&quot;1&quot;)))">
                        <udc:Line>
                            <xsl:value-of select="PO_LINE_NUMBER"/>
                        </udc:Line>
                    </xsl:if>
                </udc:BusinessDocumentReference>
                <xsl:if test="not((ROLLUP_CODE/@xsi:nil=(&quot;true&quot;)) or (ROLLUP_CODE/@xsi:nil=(&quot;1&quot;)))">
                    <tns:CustomsValuationMethod>
                        <xsl:value-of select="ROLLUP_CODE"/>
                    </tns:CustomsValuationMethod>
                </xsl:if>
                <dl:ExportLicense>
                    <dl:Description>
                        <xsl:value-of select="ECCN"/>
                    </dl:Description>
                    <dl:LicenseIdentifier>
                        <xsl:value-of select="LICENSE_NUMBER"/>
                    </dl:LicenseIdentifier>
                </dl:ExportLicense>
                <dl:HarmonizedTariffScheduleInformation>
                    <xsl:if test="exists(XXCMF_3B18_SHIPMENT_HTS_INFO/XXCMF_3B18_SHIPMENT_HTS_INFO_ROW/HTS_NUMBER)">
                        <xsl:for-each select="XXCMF_3B18_SHIPMENT_HTS_INFO/XXCMF_3B18_SHIPMENT_HTS_INFO_ROW/HTS_NUMBER">
                        <dl:Code>
                            <xsl:value-of select="."/>
                        </dl:Code>
                    </xsl:for-each>
                    </xsl:if>
                    <xsl:if test="not(exists(XXCMF_3B18_SHIPMENT_HTS_INFO/XXCMF_3B18_SHIPMENT_HTS_INFO_ROW/HTS_NUMBER))">
                        <dl:Code>
                            "not available"
                        </dl:Code>
                        
                    </xsl:if>
                        <uc:Country>
                            <xsl:value-of select="XXCMF_3B18_SHIPMENT_HTS_INFO/XXCMF_3B18_SHIPMENT_HTS_INFO_ROW/HTS_COUNTRY"/>
                    </uc:Country>
                    <xsl:for-each select="XXCMF_3B18_SHIPMENT_HTS_INFO/XXCMF_3B18_SHIPMENT_HTS_INFO_ROW/HTS_DESCRIPTION">
                        <xsl:if test="not((./@xsi:nil=(&quot;true&quot;)) or (./@xsi:nil=(&quot;1&quot;)))">
                            <dl:SubCode>
                                <xsl:value-of select="."/>
                            </dl:SubCode>
                        </xsl:if>
                    </xsl:for-each>
                </dl:HarmonizedTariffScheduleInformation>
                <tns:LineNumber>
                   <xsl:value-of select="LINE_NUMBER"/>
                </tns:LineNumber>
                <tns:PricingInformation>
                    <!--<xsl:choose>-->
                        <xsl:if test="string-length(normalize-space(current()/UNIT_ITEM_PRICE)) !=0">
						<!--<xsl:when test="string-length(normalize-space(current()/UNIT_ITEM_PRICE)) !=0">-->
                            <sha:MonetaryAmount>
                                <rat:AmountType>
                                    <xsl:value-of select="&quot;UNI&quot;"/>
                                </rat:AmountType>
                                <ume:FinancialAmount>
                                    <ume:Amount>
                                        <xsl:value-of select="UNIT_ITEM_PRICE"/>
                                    </ume:Amount>
                                    <ucr:Currency>
                                        <xsl:value-of select="../../CURRENCY_CODE"/>
                                    </ucr:Currency>
                                </ume:FinancialAmount>
                            </sha:MonetaryAmount>
                        <!--</xsl:when>-->
               <!--  Mrinmay-->             
	</xsl:if>
                    <xsl:if test="string-length(normalize-space(current()/EXTENDED_ITEM_PRICE)) !=0">
                            <sha:MonetaryAmount>
                                <rat:AmountType>
                                    <xsl:value-of select="&quot;VAD&quot;"/>
                                </rat:AmountType>
                                <ume:FinancialAmount>
                                    <ume:Amount>
                                        <xsl:value-of select="EXTENDED_ITEM_PRICE"/>
                                    </ume:Amount>
                                    <ucr:Currency>
                                        <xsl:value-of select="../../CURRENCY_CODE"/>
                                    </ucr:Currency>
                                </ume:FinancialAmount>
                            </sha:MonetaryAmount>
                    </xsl:if>
                    <!--  Mrinmay End-->             
                        <xsl:if test="string-length(normalize-space(current()/CUSTOMS_UPLIFT_PRICE)) !=0">
                            <sha:MonetaryAmount>
                                <!--newly added-->
                                <rat:AmountType>
                                    <xsl:value-of select="&quot;MIS&quot;"/>
                                </rat:AmountType>
                                <ume:FinancialAmount>
                                    <ume:Amount>
                                        <xsl:value-of select="CUSTOMS_UPLIFT_PRICE"/>
                                    </ume:Amount>
                                    <ucr:Currency>
                                        <xsl:value-of select="../../CURRENCY_CODE"/>
                                    </ucr:Currency>
                                </ume:FinancialAmount>
                            </sha:MonetaryAmount>
                        </xsl:if>
                    <!--</xsl:choose>-->
                </tns:PricingInformation>
              
                <tns:ProductDescription>
                    <xsl:if test="not((REPORT_DESCRIPTION/@xsi:nil=(&quot;true&quot;)) or (REPORT_DESCRIPTION/@xsi:nil=(&quot;1&quot;)))">
                        <updi:Detail>
                            <xsl:value-of select="REPORT_DESCRIPTION"/>
                        </updi:Detail>
                    </xsl:if>
                    <updi:Primary>
                        <xsl:value-of select="CISCO_PART_DESCRIPTION"/>
                    </updi:Primary>
					<updi:Summary>
                            <xsl:value-of select="CUSTOMS_LINE_DESCRIPTION"/>
                        </updi:Summary>
                </tns:ProductDescription>
               
                <tns:ProductIdentificationInformation>
               
                    <xsl:if test="not((CISCO_PART_NUMBER/@xsi:nil=(&quot;true&quot;)) or (CISCO_PART_NUMBER/@xsi:nil=(&quot;1&quot;)))">
                        <updi:ProductName>
                            <xsl:value-of select="CISCO_PART_NUMBER"/>
                        </updi:ProductName>
                    </xsl:if>
                    <xsl:if test="not((ITEM_IDENTIFIER/@xsi:nil=(&quot;true&quot;)) or (ITEM_IDENTIFIER/@xsi:nil=(&quot;1&quot;)))">
                        <updi:Revision>
                            <xsl:value-of select="ITEM_IDENTIFIER"/>
                        </updi:Revision>
                    </xsl:if>
            

                    <xsl:for-each select="XXCMF_3B18_XREF_ALL/XXCMF_3B18_XREF_ALL_ROW">
                        <ulc:AlternativeIdentifier>
                            <ulc:Authority>
                                <xsl:value-of select="ITEM_CROSS_REFERENCE_TYPE"/>
                            </ulc:Authority>
                            <ulc:Identifier>
                                <xsl:value-of select="ITEM_CROSS_REFERENCE"/>
                            </ulc:Identifier>
                        </ulc:AlternativeIdentifier>
                    </xsl:for-each>
                </tns:ProductIdentificationInformation>
                <sha:QuantityInformation>
                    <xsl:if test="not((ORDERED_QUANTITY/@xsi:nil=(&quot;true&quot;)) or (ORDERED_QUANTITY/@xsi:nil=(&quot;1&quot;)))">
                        <sha:RequestedQuantity>
                            <xsl:value-of select="format-number(ORDERED_QUANTITY,'#')"/>
                        </sha:RequestedQuantity>
                    </xsl:if>
                    <sha:ShippedQuantity>
                        <xsl:value-of select="format-number(SHIPPED_QUANTITY,'#')"/>
                    </sha:ShippedQuantity>
                </sha:QuantityInformation>
			<dp:RequestingOrderLineItemReference>
 <udc:BusinessDocumentReference>
                    <udct:DocumentType>PLS</udct:DocumentType>
                    <udc:Identifier><xsl:value-of select="LINE_OPTION_NUMBER"/></udc:Identifier>
                </udc:BusinessDocumentReference>
			</dp:RequestingOrderLineItemReference>
                <xsl:for-each select="XXCMF_3B18_CARTON_DETAILS/XXCMF_3B18_CARTON_DETAILS_ROW">
                    <tns:ShippingContainer>
                        <tns:ContainerPhysicalDimension>
                            <dl:Linear>
                                <dl:Height>
                                    <upd:UnitOfMeasure>
                                        <xsl:value-of select="DIM_UOM"/>
                                    </upd:UnitOfMeasure>
                                    <upd:Value>
                                        <xsl:value-of select="CARTON_HEIGHT"/>
                                    </upd:Value>
                                </dl:Height>
                                <dl:Length>
                                    <upd:UnitOfMeasure>
                                        <xsl:value-of select="DIM_UOM"/>
                                    </upd:UnitOfMeasure>
                                    <upd:Value>
                                        <xsl:value-of select="CARTON_LENGTH"/>
                                    </upd:Value>
                                </dl:Length>
                                <dl:Width>
                                    <upd:UnitOfMeasure>
                                        <xsl:value-of select="DIM_UOM"/>
                                    </upd:UnitOfMeasure>
                                    <upd:Value>
                                        <xsl:value-of select="CARTON_WIDTH"/>
                                    </upd:Value>
                                </dl:Width>
                            </dl:Linear>
                            <dl:MassPhysicalDimension>
                                <upd:Weight>
                                    <upd:UnitOfMeasure>
                                        <xsl:value-of select="CARTON_WEIGHT_UOM"/>
                                    </upd:UnitOfMeasure>
                                    <upd:Value>
                                        <xsl:value-of select="CARTON_GROSS_WEIGHT"/>
                                    </upd:Value>
                                    <upd:Type>
                                        <xsl:value-of select="&quot;GRS&quot;"/>
                                    </upd:Type>
                                </upd:Weight>
                            </dl:MassPhysicalDimension>
                        </tns:ContainerPhysicalDimension>
                        <tns:Identifier>
                            <xsl:value-of select="CARTON_NUMBER"/>
                        </tns:Identifier>
                        <xsl:if test="not((PARENT_CARTON_NUMBER/@xsi:nil=(&quot;true&quot;)) or (PARENT_CARTON_NUMBER/@xsi:nil=(&quot;1&quot;)))">
                            <tns:ShippingCartonIdentifier>
                                <xsl:value-of select="PARENT_CARTON_NUMBER"/>
                            </tns:ShippingCartonIdentifier>
                        </xsl:if>
                        <tns:ShippingContainerItem>
                            <xsl:for-each select="XXCMF_3B18_SERIAL_COO/XXCMF_3B18_SERIAL_COO_ROW">
			<dl:ManufacturerProfile>
			        <dl:CountryOfOrigin>
			<xsl:value-of select="COUNTRY_OF_ORIGIN"/>
			        </dl:CountryOfOrigin>
				 <dl:ProductSerialIdentifier>
				<xsl:value-of select="SERIAL_NUMBER"/>
				</dl:ProductSerialIdentifier>
			</dl:ManufacturerProfile>
		</xsl:for-each>
                            <tns:NumberOfItemPackages>
                                <xsl:value-of select="if((current()/PACKED_QUANTITY ) =&quot;NaN&quot;&#xA;)&#xA;then&#xA;1&#xA;else&#xA;number(current()/PACKED_QUANTITY )"/>
                            </tns:NumberOfItemPackages>
                            <sha:QuantityInformation>
                                <sha:RequestedQuantity>
                                    <xsl:value-of select="1"/>
                                </sha:RequestedQuantity>
                                <sha:ShippedQuantity>
                                    <xsl:value-of select="if((current()/PACKED_QUANTITY ) =&quot;NaN&quot;&#xA;)&#xA;then&#xA;1&#xA;else&#xA;number(current()/PACKED_QUANTITY )"/>
                                </sha:ShippedQuantity>
                            </sha:QuantityInformation>
                            <dp:RequestingOrderLineItemReference>
                                <udc:BusinessDocumentReference>
                                    <udct:DocumentType>
                                        <xsl:value-of select="&quot;CON&quot;"/>
                                    </udct:DocumentType>
                                    <udc:Identifier>
                                        <xsl:value-of select="&quot;&quot;"/>
                                    </udc:Identifier>
                                </udc:BusinessDocumentReference>
                            </dp:RequestingOrderLineItemReference>
                        </tns:ShippingContainerItem>
                    </tns:ShippingContainer>
                </xsl:for-each>
                <!--Mrinmay-->
                <uuom:UnitOfMeasure>
                    <xsl:value-of select="UOM"/>
                </uuom:UnitOfMeasure>
                <!--Mrinmay End-->
            </tns:ShipmentLineItem>
             </xsl:for-each>
	<dsd:ShippingDocument>
                            <xsl:value-of select="'CIN'"/>
                </dsd:ShippingDocument>
			 </tns:ShippingBusinessDocument>
</tns:ShippingDocumentationNotification>
    </xsl:template>
</xsl:stylesheet>

<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var"
                exclude-result-prefixes="msxsl var ScriptNS0 ScriptNS1" version="1.0"
                xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
                xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />

  <xsl:template match="/">
    <xsl:apply-templates select="/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalShipment']" />
  </xsl:template>

  <xsl:template match="/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalShipment']" >

    <xsl:variable name ="Order" select ="(//*[local-name()='RelatedShipmentCollection']/*[local-name()='RelatedShipment'][*[local-name()='DataContext']/*[local-name()='DataSourceCollection']/*[local-name()='DataSource']/*[local-name()='Type']='OrderManagerOrder']|
                  *[local-name()='Shipment'][*[local-name()='DataContext']/*[local-name()='DataSourceCollection']/*[local-name()='DataSource']/*[local-name()='Type']='OrderManagerOrder'])[1]"/>


    <xsl:element name ="cXML">
      <xsl:variable name="CurrentDateTime" select="ScriptNS0:CurrentDateTimeWithTimeZone()" />
      <xsl:variable name ="payloadID" select ="ScriptNS0:FormatXmlDateTime($CurrentDateTime, 'yyyyMMddHHmmss')"/>
      <xsl:variable name ="TriggerDate" select ="*[local-name()='Shipment']/*[local-name()='DataContext']/*[local-name()='TriggerDate']"/>
      <xsl:variable name="TimeZone" select="ScriptNS1:GetRecipientCodeUnkeyed('AXIDFWDFW' , 'AXIDFWDFW_SHC' , 'Shaw Confirmation Request xml - Export Order Data' , 'Defaults' , 'Time Zone')" />
      
      <xsl:attribute name="payloadID">
        <xsl:value-of select ="$payloadID"/>
      </xsl:attribute>

      <xsl:attribute name="timestamp">
        <xsl:value-of select ="ScriptNS0:FormatXmlDateTime($CurrentDateTime, 'yyyy-MM-ddTHH:mm:ss')"/>
      </xsl:attribute>

      <xsl:element name ="Header">
        <xsl:element name ="From">
          <xsl:element name ="Credential">
            <xsl:attribute name ="domain">
              <xsl:value-of select ="ScriptNS1:GetRecipientCodeUnkeyed('AXIDFWDFW' , 'AXIDFWDFW_SHC' , 'Shaw Confirmation Request xml - Export Order Data' , 'Defaults' , 'From Domain')"/>
            </xsl:attribute>

            <xsl:element name ="Identity">
              <xsl:value-of select ="ScriptNS1:GetRecipientCodeUnkeyed('AXIDFWDFW' , 'AXIDFWDFW_SHC' , 'Shaw Confirmation Request xml - Export Order Data' , 'Defaults' , 'From Identity')"/>
            </xsl:element>

          </xsl:element>
        </xsl:element>

        <xsl:element name ="To">
          <xsl:element name ="Credential">
            <xsl:attribute name ="domain">
              <xsl:value-of select ="ScriptNS1:GetRecipientCodeUnkeyed('AXIDFWDFW' , 'AXIDFWDFW_SHC' , 'Shaw Confirmation Request xml - Export Order Data' , 'Defaults' , 'To Domain')"/>
            </xsl:attribute>

            <xsl:element name ="Identity">
              <xsl:value-of select ="ScriptNS1:GetRecipientCodeUnkeyed('AXIDFWDFW' , 'AXIDFWDFW_SHC' , 'Shaw Confirmation Request xml - Export Order Data' , 'Defaults' , 'To Identity')"/>
            </xsl:element>

          </xsl:element>
        </xsl:element>

        <xsl:element name ="Sender">
          <xsl:element name ="Credential">
            <xsl:attribute name ="domain">
              <xsl:value-of select ="ScriptNS1:GetRecipientCodeUnkeyed('AXIDFWDFW' , 'AXIDFWDFW_SHC' , 'Shaw Confirmation Request xml - Export Order Data' , 'Defaults' , 'Sender Domain')"/>
            </xsl:attribute>

            <xsl:element name ="Identity">
              <xsl:value-of select ="ScriptNS1:GetRecipientCodeUnkeyed('AXIDFWDFW' , 'AXIDFWDFW_SHC' , 'Shaw Confirmation Request xml - Export Order Data' , 'Defaults' , 'Sender Identity')"/>
            </xsl:element>

            <xsl:element name ="SharedSecret">
              <xsl:value-of select ="ScriptNS1:GetRecipientCodeUnkeyed('AXIDFWDFW' , 'AXIDFWDFW_SHC' , 'Shaw Confirmation Request xml - Export Order Data' , 'Defaults' , 'Shared Secret')"/>
            </xsl:element>

          </xsl:element>

          <xsl:element name ="UserAgent">
            <xsl:value-of select ="ScriptNS1:GetRecipientCodeUnkeyed('AXIDFWDFW' , 'AXIDFWDFW_SHC' , 'Shaw Confirmation Request xml - Export Order Data' , 'Defaults' , 'User Agent')"/>
          </xsl:element>

        </xsl:element>
      </xsl:element>

      <xsl:element name ="Request">
        <xsl:element name ="ConfirmationRequest">
          <xsl:element name ="ConfirmationHeader">
            <xsl:attribute name ="type">
              <xsl:text>accept</xsl:text>
            </xsl:attribute>

            <xsl:attribute name="noticeDate">
              <xsl:value-of select ="concat(ScriptNS0:FormatXmlDateTime($TriggerDate,'yyyy-MM-ddTHH:mm:ss'), $TimeZone)"/>
            </xsl:attribute>

            <xsl:variable name ="ShipFrom" select ="$Order/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType'] = 'ConsignorDocumentaryAddress']"/>
            <xsl:call-template name="GenerateContact">
              <xsl:with-param name="Role" select="'shipFrom'" />
              <xsl:with-param name="Contact" select="$ShipFrom" />
            </xsl:call-template>

            <xsl:variable name ="ShipTo" select ="$Order/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][./*[local-name()='AddressType'] = 'ConsigneeDocumentaryAddress']"/>
            <xsl:call-template name="GenerateContact">
              <xsl:with-param name="Role" select="'shipTo'" />
              <xsl:with-param name="Contact" select="$ShipTo" />
            </xsl:call-template>

          </xsl:element>

          
            
            <xsl:element name ="OrderReference">
              <xsl:variable name ="Prefix" select ="$Order/*[local-name()='CustomizedFieldCollection']/*[local-name()='CustomizedField'][*[local-name()='Key'] = 'Business Unit Identifier' and *[local-name()='DataType'] = 'String']/*[local-name()='Value']/text()"/>
              <xsl:variable name ="OrderNum" select ="$Order/*[local-name()='Order']/*[local-name()='OrderNumber']/text()"/>

              <xsl:attribute name="orderID">
                <xsl:choose>
                  <xsl:when test ="$Prefix != ''">
                    <xsl:value-of select ="concat($Prefix, '.', $OrderNum)"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select ="$OrderNum"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>

              <xsl:variable name ="orderDate" select ="$Order/*[local-name()='DateCollection']/*[local-name()='Date'][*[local-name()='Type'] ='OrderDate' and *[local-name()='IsEstimate'] ='false']/*[local-name()='Value']"/>
              <xsl:if test ="$orderDate != ''">
                <xsl:attribute name ="orderDate">
                  <xsl:value-of select ="ScriptNS0:FormatXmlDateTime($orderDate, 'yyyy-MM-ddTHH:mm:ss:zzz')"/>
                </xsl:attribute>
              </xsl:if>

              <xsl:element name ="DocumentReference">
                <xsl:attribute name="payloadID">
                  <xsl:value-of select="$payloadID" />
                </xsl:attribute>
              </xsl:element>
            </xsl:element>

          
            <xsl:for-each select ="$Order/*[local-name()='Order']/*[local-name()='OrderLineCollection']/*[local-name()='OrderLine']">
              <xsl:element name ="ConfirmationItem">
                <xsl:attribute name="lineNumber">
                  <xsl:value-of select ="*[local-name()='LineNumber']"/>
                </xsl:attribute>
                <xsl:variable name ="OrderedQty" select ="*[local-name()='OrderedQty']"/>
                <xsl:attribute name="quantity">
                  <xsl:value-of select ="$OrderedQty"/>
                </xsl:attribute>

                <xsl:variable name ="UQ" select ="*[local-name()='OrderedQtyUnit']/*[local-name()='Code']"/>
                <xsl:variable name ="UnitOfMeasure">
                  <xsl:choose>
                    <xsl:when test ="$UQ != ''">
                      <xsl:value-of select ="$UQ"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:text>PCE</xsl:text>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:variable>

                <xsl:element name ="UnitOfMeasure">
                  <xsl:value-of select ="$UnitOfMeasure"/>
                </xsl:element>

                <xsl:element name ="ConfirmationStatus">
                  <xsl:attribute name="quantity">
                    <xsl:value-of select="$OrderedQty"/>
                  </xsl:attribute>

                  <xsl:attribute name="type">
                    <xsl:text>detail</xsl:text>
                  </xsl:attribute>

                  
                  <xsl:variable name="DeliveryDate" select="ScriptNS0:ConvertXmlDateString(//*[local-name()='LocalProcessing']/*[local-name()='DeliveryCartageCompleted'], 'yyyy-MM-ddTHH:mm:ss')"/>
                  <xsl:variable name="EstimateDelivery" select="ScriptNS0:ConvertXmlDateString(//*[local-name()='LocalProcessing']/*[local-name()='EstimatedDelivery'], 'yyyy-MM-ddTHH:mm:ss')"/>
                  <xsl:variable name ="AgentEstDeliveryDateCustField" select ="ScriptNS1:GetRecipientCodeUnkeyed('AXIDFWDFW' , 'AXIDFWDFW_SHC' , 'Shaw Confirmation Request xml - Export Order Data' , 'Defaults' , 'Agent Dlv Date Cust Field')"/>
                                      
                  <xsl:variable name ="AgentEstDeliveryDate" select ="ScriptNS0:ConvertXmlDateString($Order/*[local-name()='MilestoneCollection']/*[local-name()='Milestone']
                                [ *[local-name()='Description'] = $AgentEstDeliveryDateCustField ]/*[local-name()='ActualDate' and .!= ''], 'yyyy-MM-ddTHH:mm:ss')"/>

                  
                  <xsl:choose>

                    <xsl:when test="$DeliveryDate != ''">
                      <xsl:attribute name="shipmentDate">
                        <xsl:value-of select="concat($DeliveryDate,  $TimeZone)"/>
                      </xsl:attribute>

                      <xsl:attribute name="deliveryDate">
                        <xsl:value-of select="concat($DeliveryDate, $TimeZone)"/>
                      </xsl:attribute>
                    </xsl:when>

                    <xsl:when test="$EstimateDelivery != ''">
                      <xsl:attribute name="shipmentDate">
                        <xsl:value-of select="concat($EstimateDelivery,  $TimeZone)"/>
                      </xsl:attribute>

                      <xsl:attribute name="deliveryDate">
                        <xsl:value-of select="concat($EstimateDelivery, $TimeZone)"/>
                      </xsl:attribute>
                    </xsl:when>

                    <xsl:when test="$AgentEstDeliveryDate != ''">
                      <xsl:attribute name="shipmentDate">
                        <xsl:value-of select="concat($AgentEstDeliveryDate,  $TimeZone)"/>
                      </xsl:attribute>

                      <xsl:attribute name="deliveryDate">
                        <xsl:value-of select="concat($AgentEstDeliveryDate, $TimeZone)"/>
                      </xsl:attribute>
                    </xsl:when>

                    <xsl:otherwise>
                      <xsl:variable name ="FormattedTriggerDate" select ="ScriptNS0:FormatXmlDateTime($TriggerDate,'yyyy-MM-ddTHH:mm:ss')"/>
                      <xsl:attribute name="shipmentDate">
                        <xsl:value-of select="concat($FormattedTriggerDate,  $TimeZone)"/>
                      </xsl:attribute>

                      <xsl:attribute name="deliveryDate">
                        <xsl:value-of select="concat($FormattedTriggerDate, $TimeZone)"/>
                      </xsl:attribute>
                    </xsl:otherwise>
                  </xsl:choose>

                  <xsl:element name ="UnitOfMeasure">
                    <xsl:value-of select ="$UnitOfMeasure"/>
                  </xsl:element>


                  <xsl:if test ="*[local-name()='UnitPriceRecommended'] = number(*[local-name()='UnitPriceRecommended'])">
                    <xsl:element name ="UnitPrice">
                      <xsl:element name ="Money">
                        
                          <xsl:attribute name ="currency">
                            <xsl:choose>
                              <xsl:when test ="$Order/*[local-name()='FreightRateCurrency']/*[local-name()='Code'] != ''">
                                <xsl:value-of select ="$Order/*[local-name()='FreightRateCurrency']/*[local-name()='Code']"/>
                              </xsl:when>
                              <xsl:otherwise>
                                <xsl:text>USD</xsl:text>
                              </xsl:otherwise>
                            </xsl:choose>
                          </xsl:attribute>
                        
                        <xsl:value-of select ="*[local-name()='UnitPriceRecommended']"/>
                      </xsl:element>
                    </xsl:element>
                  </xsl:if>



                  <xsl:variable name ="LineNum" select ="*[local-name()='CustomizedFieldCollection']/*[local-name()='CustomizedField'][*[local-name()='Key'] ='LINENUM' and *[local-name()='DataType'] = 'String']/*[local-name()='Value' and . !='']"/>
                  <xsl:variable name ="ShipmentNum" select ="*[local-name()='CustomizedFieldCollection']/*[local-name()='CustomizedField'][*[local-name()='Key'] ='SHIPMENTNUM' and *[local-name()='DataType'] = 'String']/*[local-name()='Value' and . !='']"/>
                  <xsl:if test ="$LineNum != ''">
                    <xsl:element name ="Extrinsic">
                      <xsl:attribute name ="name">
                        <xsl:text>LINENUM</xsl:text>
                      </xsl:attribute>
                      <xsl:value-of select ="$LineNum"/>
                    </xsl:element>
                  </xsl:if>

                  <xsl:if test ="$ShipmentNum != ''">
                    <xsl:element name ="Extrinsic">
                      <xsl:attribute name ="name">
                        <xsl:text>SHIPMENTNUM</xsl:text>
                      </xsl:attribute>
                      <xsl:value-of select ="$ShipmentNum"/>
                    </xsl:element>
                  </xsl:if>

                </xsl:element>
              </xsl:element>
            </xsl:for-each>

        </xsl:element>
      </xsl:element>
    </xsl:element>
  </xsl:template>



  <xsl:template name="GenerateContact">
    <xsl:param name="Role" />
    <xsl:param name="Contact" />

    <xsl:element name="Contact">
      <xsl:attribute name="role">
        <xsl:value-of select="$Role" />
      </xsl:attribute>
      <xsl:attribute name="addressID">
        <xsl:value-of select="$Contact/*[local-name()='OrganizationCode']" />
      </xsl:attribute>
      <xsl:element name="Name">
        <xsl:value-of select="$Contact/*[local-name()='CompanyName']" />
      </xsl:element>
      <xsl:element name="PostalAddress">
        <xsl:element name="Street">
          <xsl:value-of select="$Contact/*[local-name()='Address1']" />
        </xsl:element>
        <xsl:if test="$Contact/*[local-name()='Address2'] != ''">
          <xsl:element name="Street">
            <xsl:value-of select="$Contact/*[local-name()='Address2']" />
          </xsl:element>
        </xsl:if>
        <xsl:element name="City">
          <xsl:value-of select="$Contact/*[local-name()='City']" />
        </xsl:element>
        <xsl:element name="State">
          <xsl:value-of select="$Contact/*[local-name()='State']" />
        </xsl:element>
        <xsl:element name="PostalCode">
          <xsl:value-of select="$Contact/*[local-name()='Postcode']" />
        </xsl:element>
        <xsl:element name="Country">
          <xsl:attribute name="isoCountryCode">
            <xsl:value-of select="$Contact/*[local-name()='Country']/*[local-name()='Code']" />
          </xsl:attribute>
          <xsl:value-of select="$Contact/*[local-name()='Country']/*[local-name()='Name']" />
        </xsl:element>
      </xsl:element>

      <xsl:variable name="Phone" select="normalize-space($Contact/*[local-name()='Phone'])" />
      <xsl:if test="$Phone != ''">
        <xsl:element name="Phone">
          <xsl:element name="TelephoneNumber">
            <xsl:element name="Number">
              <xsl:value-of select="$Phone" />
            </xsl:element>
          </xsl:element>
        </xsl:element>
      </xsl:if>

    </xsl:element>
  </xsl:template>
</xsl:stylesheet>
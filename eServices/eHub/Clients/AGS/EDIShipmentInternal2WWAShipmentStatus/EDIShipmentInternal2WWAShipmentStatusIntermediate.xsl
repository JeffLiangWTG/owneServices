<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var" exclude-result-prefixes="msxsl var s0 ScriptNS0 ScriptNS1 ScriptNS2 ScriptNS3 userCSharp xsl" version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp"
                xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
                xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1"
                xmlns:ScriptNS2="http://schemas.microsoft.com/BizTalk/2003/ScriptNS2"
                xmlns:ScriptNS3="http://schemas.microsoft.com/BizTalk/2003/ScriptNS3">

  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes" />

  <xsl:variable name ="SenderID" select="'AGSWORAGS'" />
  <xsl:variable name ="RecipientID" select="'AGSWORAGS_WWA'" />
  <xsl:variable name ="TS_Name" select="'WorldWideAlliance xml-File - Export Shipmnt Status'" />
  <xsl:variable name ="MessageType" select ="ScriptNS0:GetRecipientCode($SenderID, $RecipientID, $TS_Name, 'Envelope Detail', 'Type', 'Shipment')"/>
  <xsl:variable name ="MessageVersion" select ="ScriptNS0:GetRecipientCode($SenderID, $RecipientID, $TS_Name, 'Envelope Detail', 'Version', 'Shipment')"/>
  <xsl:variable name ="Password" select="ScriptNS0:GetRecipientCode($SenderID, $RecipientID, $TS_Name, 'XML Password', '')"/>
  <xsl:variable name ="TriggerPurpose" select ="//*[local-name()='InterchangeInfo']/*[local-name()='Source']/*[local-name()='Purpose']"/>
  <xsl:variable name ="IsExport" select="ScriptNS0:GetRecipientCode($SenderID , $RecipientID , $TS_Name , 'Status Code' , 'Export Shipment Status', $TriggerPurpose)" />

  <xsl:template match="/">
    <xsl:apply-templates select="/*[local-name()='ShipmentsInternal']/*[local-name()='Payload']/*[local-name()='Shipments']/*[local-name()='Shipment']" />
  </xsl:template>

  <xsl:template match="/*[local-name()='ShipmentsInternal']/*[local-name()='Payload']/*[local-name()='Shipments']/*[local-name()='Shipment']">
    
      <xsl:variable name ="ShipmentNum" select ="./*[local-name()='ShipmentDetails']/*[local-name()='AgentReference']"/>
      <xsl:variable name ="PortOfDestination" select ="./*[local-name()='ShipmentDetails']/*[local-name()='PortofDestination']/*[local-name()='Port']"/>
      <xsl:variable name="PortOfOrigin" select="./*[local-name()='ShipmentDetails']/*[local-name()='PortOfOrigin']/*[local-name()='Port']" />
      <xsl:variable name ="Packages" select ="./*[local-name()='ShipmentDetails']/*[local-name()='Packages']/*[local-name()='Package']"/>
      <xsl:variable name="BookingReference" select="./*[local-name()='ShipmentDetails']/*[local-name()='BookingReference']" />
    <xsl:element name ="ShipmentStatus">

        <xsl:element name ="Envelope">
          <xsl:element name ="SenderID">
            <xsl:text>edi_ags_prod</xsl:text>
          </xsl:element>

          <xsl:element name ="ReceiverID">
            <xsl:text>wwalliance</xsl:text>
          </xsl:element>

          <xsl:element name ="Password">
            <xsl:value-of select ="$Password"/>
          </xsl:element>

          <xsl:element name ="Type">
            <xsl:value-of select ="$MessageType"/>
          </xsl:element>

          <xsl:element name ="Version">
            <xsl:value-of select ="$MessageVersion"/>
          </xsl:element>

          <EnvelopeID>
            <xsl:value-of select="concat(ScriptNS1:CurrentDateTimeUTC('yyyyMMddHHmmss'), ' ', $ShipmentNum)"/>
          </EnvelopeID>

        </xsl:element>

        <xsl:element name ="ShipmentStatusDetails">

          <xsl:if test ="$IsExport = 'true'">

            <xsl:element name ="ApplicationType">
              <xsl:variable name="ApplicationType" select="./*[local-name()='ShipmentDetails']/*[local-name()='CustomValues']/*[local-name()='CustomValue' and @*[local-name()='Name'] = 'ApplicationType']" />
              <xsl:choose>
                <xsl:when test="$ApplicationType = 'WE'">
                  <xsl:value-of select="'WE'" />
                </xsl:when>
                <xsl:when test="$ApplicationType = '' and //*[local-name()='Shipment']/*[local-name()='Events']/*[local-name()='Event' and *[local-name()='Source'] = 'JobShipment' and *[local-name()='Code'] = 'DIM']">
                  <xsl:value-of select="'ME'" />
                </xsl:when>
                <xsl:when test="./*[local-name()='Events']/*[local-name()='Event' and *[local-name()='Source'] = 'JobShipment' and *[local-name()='Code'] = 'ADD' and *[local-name()='User'] != '~BP'  and *[local-name()='User'] != '~AD']">
                  <xsl:value-of select="'MN'" />
                </xsl:when>
              </xsl:choose>

            </xsl:element>
          </xsl:if>

          <xsl:element name ="TypeOfMove">
            <xsl:value-of select ="ScriptNS0:GetRecipientCode($SenderID , $RecipientID , $TS_Name , 'Type of Move' , 'TypeOfMove', ./*[local-name()='ShipmentDetails']/*[local-name()='PackingMode'])"/>
          </xsl:element>

          <xsl:variable name ="OrderReference" select ="./*[local-name()='ShipmentDetails']/*[local-name()='OrderReferences']/*[local-name()='OrderReference' and . != '']"/>
          <xsl:variable name ="OrderNumber" select ="./*[local-name()='Orders']/*[local-name()='Order']/*[local-name()='OrderIdentifier']/*[local-name()='OrderNumber']"/>

          <xsl:element name ="ShipperReference">
              <xsl:value-of select ="$BookingReference"/>
          </xsl:element>

          <xsl:element name ="ForwarderReference">
              <xsl:value-of select="./*[local-name()='ShipmentIdentifier'][@ShipmentIdentifierType='Housebill']"/>
          </xsl:element>

          <xsl:element name ="ConsigneeReference"/>
          <xsl:element name ="CommunicationReference"/>
          <xsl:element name ="PickupReference"/>

          <xsl:if test ="$IsExport">
            <xsl:element name ="BookingNumber">
              <xsl:value-of select ="$ShipmentNum"/>
            </xsl:element>
          </xsl:if>

          <xsl:if test ="$TriggerPurpose = 'W27'">
            <xsl:variable name="Upper" select="'ABCDEFGHIJKLMNOPQRSTUVWXYZ'" />
            <xsl:variable name="Lower" select="'abcdefghijklmnopqrstuvwxyz'" />


            <xsl:variable name ="MergedShipmentNum" select ="substring-after(normalize-space(translate(*[local-name()='Notes']/*[local-name()='Note']
                                  [./*[local-name()='NoteType'] = 'HandlingInstructions']/*[local-name()='NoteData'], $Lower, $Upper)), 'PACKAGES TRANSFERRED TO SHIPMENT ')"/>

            <xsl:element name ="PrimaryBookingNumber">
              <xsl:value-of select ="substring-before($MergedShipmentNum, ' ')"/>
            </xsl:element>

          </xsl:if>

          <xsl:if test ="$IsExport = 'true'">
            <xsl:variable name="WWAShpRef" select="./*[local-name()='ShipmentDetails']/*[local-name()='ReferenceNumbers']/*[local-name()='ReferenceNumber'][./*[local-name()='Type'] = 'WWA']/*[local-name()='Number' and .!='']" />

            <xsl:variable name ="WWAShipmentReference">
              <xsl:choose>
                <xsl:when test ="$WWAShpRef != ''">
                  <xsl:value-of select ="$WWAShpRef"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:variable name="MemberOfficeCode" select="ScriptNS0:GetRecipientCode($SenderID , $RecipientID , $TS_Name , 'Member Office Code', 'Member Office Code' , $PortOfOrigin)" />
                  <xsl:if test="$MemberOfficeCode != ''">
                    <xsl:variable name="ConcatenatedRef" select="concat($ShipmentNum,$MemberOfficeCode)" />
                    <xsl:value-of select="substring($ConcatenatedRef, string-length($ConcatenatedRef)-30 + 1, 30)" />
                  </xsl:if>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>

            <xsl:element name ="WWAShipmentReference">
              <xsl:value-of select ="$WWAShipmentReference"/>
            </xsl:element>

          </xsl:if>

          <xsl:element name ="LotNumber">
            <xsl:value-of select ="$ShipmentNum"/>
          </xsl:element>

          <xsl:element name ="HouseBillOfLadingNumber"/>

          <xsl:element name ="CarrierBookingNumber">
            <xsl:choose>
              <xsl:when test="$BookingReference != ''">
                <xsl:value-of select="$BookingReference" />
              </xsl:when>
              <xsl:when test="$IsExport = 'true'">
                <xsl:value-of select="ScriptNS0:GetRecipientCodeUnkeyed($SenderID, $RecipientID, $TS_Name, 'Defaults', 'Carrier Booking No.')" />
              </xsl:when>
            </xsl:choose>
          </xsl:element>

          <xsl:element name ="ReleaseType">
            <xsl:value-of select="ScriptNS0:GetRecipientCode($SenderID, $RecipientID, $TS_Name, 'Release Type', 'WWA Release Type', ./*[local-name()='ShipmentDetails']/*[local-name()='ReleaseType'])" />
          </xsl:element>

          <xsl:variable name ="CurrentCompanyCountry" select ="substring(//*[local-name()='InterchangeInfo']/*[local-name()='EDIOrganisation']/*[local-name()='OrganisationDetails']/*[local-name()='Location'], 1, 2)"/>

          <xsl:if test ="$CurrentCompanyCountry = substring($PortOfDestination, 1,2)">
            <xsl:element name ="ArrivalNoticeNumber">
              <xsl:value-of select ="$ShipmentNum"/>
            </xsl:element>
          </xsl:if>

          <xsl:element name ="ContainerNumber"/>
          <xsl:element name ="SealNumber"/>
          <xsl:element name ="CarrierSCAC"/>
          <xsl:element name ="OceanVessel"/>

          <xsl:element name ="CustomerAlias">
            <xsl:value-of select="./*[local-name()='ShipmentDetails']/*[local-name()='Consignor']/*[local-name()='OrganisationDetails']/*[local-name()='RegistrationNumbers']/*[local-name()='RegistrationNumber'][*[local-name()='CountryOfRegistration'] = 'AU'][*[local-name()='NumberType'] = 'UNC']/*[local-name()='Number']" />
          </xsl:element>

          <xsl:element name ="StatusCode">
            <xsl:value-of select ="ScriptNS0:GetRecipientCode($SenderID , $RecipientID , $TS_Name , 'Status Code' , 'WWA Status Code', $TriggerPurpose)"/>
          </xsl:element>

          <xsl:element name ="StatusLocationCode">
            <xsl:choose>
              <xsl:when test ="$IsExport ='true'">
                <xsl:value-of select ="$PortOfOrigin"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select ="$PortOfDestination"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:element>

          <xsl:element name ="StatusLocationName">
            <xsl:choose>
              <xsl:when test ="$IsExport ='true'">
                <xsl:value-of select ="$PortOfOrigin/@*[local-name()='City']"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select ="$PortOfDestination/@*[local-name()='City']"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:element>

          <xsl:element name ="RoutingDetails">

            <xsl:variable name ="ETD" select ="$PortOfOrigin/../*[local-name()='EstimatedDateTime']"/>
            <xsl:variable name ="ETA" select ="$PortOfDestination/../*[local-name()='EstimatedDateTime']"/>

            <xsl:element name="ReceivingWarehouse">
              <xsl:choose>
                <xsl:when test ="$IsExport ='true'">
                  <xsl:value-of select="$PortOfOrigin"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$PortOfDestination"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:element>

            <xsl:element name ="CutoffReceivingWarehouse">
              <xsl:value-of select ="userCSharp:GetCutOffRecvWhsDate($PortOfOrigin/../*[local-name()='EstimatedDateTime'])"/>
            </xsl:element>

            <xsl:element name ="PlaceOfReceipt">
              <xsl:value-of select ="$PortOfOrigin"/>
            </xsl:element>

            <xsl:element name ="ETSPlaceOfReceipt">
              <xsl:value-of select ="ScriptNS1:FormatXmlDateTime($ETD, 'yyyy-MM-dd')"/>
            </xsl:element>

            <xsl:element name ="PortOfLoading">
              <xsl:value-of select ="$PortOfOrigin"/>
            </xsl:element>

            <xsl:element name ="ETSPortOfLoading">
              <xsl:value-of select ="ScriptNS1:FormatXmlDateTime($ETD, 'yyyy-MM-dd')"/>
            </xsl:element>

            <xsl:element name ="PortOfDischarge">
              <xsl:value-of select ="$PortOfDestination"/>
            </xsl:element>

            <xsl:element name ="ETAPortOfDischarge">
              <xsl:value-of select ="ScriptNS1:FormatXmlDateTime($ETA, 'yyyy-MM-dd')"/>
            </xsl:element>

            <xsl:element name ="PlaceOfDelivery">
              <xsl:value-of select ="$PortOfDestination"/>
            </xsl:element>

            <xsl:element name ="ETAPlaceOfDelivery">
              <xsl:value-of select ="ScriptNS1:FormatXmlDateTime($ETA, 'yyyy-MM-dd')"/>
            </xsl:element>


          </xsl:element>

          <xsl:element name ="StatusDateTimeDetails">

            <xsl:variable name="InterchangeDateTime" select="//*[local-name()='InterchangeInfo']/*[local-name()='Date']" />
            <xsl:variable name="TriggeredByDateTime" select="./*[local-name()='Events']/*[local-name()='Event'][*[local-name()='TriggeredBy'] ='true']/*[local-name()='DateTime']" />

            <xsl:choose>
              <xsl:when test ="$TriggeredByDateTime != ''">
                <xsl:call-template name ="PopulateEventDateTime">
                  <xsl:with-param name ="DateTime" select ="$TriggeredByDateTime"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:otherwise>
                <xsl:call-template name ="PopulateEventDateTime">
                  <xsl:with-param name ="DateTime" select ="$InterchangeDateTime"/>
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>

            <xsl:element name ="TimeZone">
              <xsl:text>GMT</xsl:text>
            </xsl:element>
          </xsl:element>

          <xsl:element name ="CargoDetails">
            <xsl:element name="Pieces">
              <xsl:value-of select="sum($Packages/*[local-name()='NumberOfPacks'])" />
            </xsl:element>

            <xsl:variable name ="Reset" select ="userCSharp:Reset()"/>

            <xsl:for-each select ="$Packages">
              <xsl:variable name="AddWeight" select="userCSharp:AddWeight(ScriptNS2:Convert(./*[local-name()='Weight'], ./*[local-name()='Weight']/@DimensionType, 'LB'))"/>
              <xsl:variable name="AddVolume" select="userCSharp:AddVolume(ScriptNS2:Convert(./*[local-name()='Volume'], ./*[local-name()='Volume']/@DimensionType, 'CF'))"/>
            </xsl:for-each>

            <xsl:variable name ="TotalWeightInLB" select ="userCSharp:GetTotalWeight()"/>
            <xsl:variable name ="TotalVolumeInCF" select ="userCSharp:GetTotalVolume()"/>

            <xsl:element name="WeightLBS">
              <xsl:value-of select ="$TotalWeightInLB"/>
            </xsl:element>

            <xsl:element name="VolumeCBF">
              <xsl:value-of select ="$TotalVolumeInCF"/>
            </xsl:element>

            <xsl:element name="WeightKG">
              <xsl:value-of select ="ScriptNS2:Convert($TotalWeightInLB, 'LB', 'KG')"/>
            </xsl:element>

            <xsl:element name="VolumeCBM">
              <xsl:value-of select ="ScriptNS2:Convert($TotalVolumeInCF, 'CF', 'M3')"/>
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
                  <xsl:value-of select="sum($Packages[./*[local-name()='DangerousGoods'] != '' and string-length(./*[local-name()='DangerousGoods']) &gt; 0]/*[local-name()='NumberOfPacks'])" />
                </xsl:element>


                <xsl:variable name ="Reset_HazCount" select ="userCSharp:Reset()"/>

                <xsl:for-each select ="$Packages/*[local-name()='DangerousGoods']/*[local-name()='UNDG']">
                  <xsl:variable name="AddWeight" select="userCSharp:AddWeight(ScriptNS2:Convert(./*[local-name()='Weight'], ./*[local-name()='Weight']/@DimensionType, 'LB'))"/>
                  <xsl:variable name="AddVolume" select="userCSharp:AddVolume(ScriptNS2:Convert(./*[local-name()='Volume'], ./*[local-name()='Volume']/@DimensionType, 'CF'))"/>
                </xsl:for-each>

                <xsl:variable name ="TotalHazWeightInLB" select ="userCSharp:GetTotalWeight()"/>
                <xsl:variable name ="TotalHazVolumeInCF" select ="userCSharp:GetTotalVolume()"/>

                <xsl:element name="HazWeightLBS">
                  <xsl:value-of select="$TotalHazWeightInLB" />
                </xsl:element>

                <xsl:element name="HazVolumeCBF">
                  <xsl:value-of select="$TotalHazVolumeInCF" />
                </xsl:element>

                <xsl:element name="HazWeightKG">
                  <xsl:value-of select ="ScriptNS2:Convert($TotalHazWeightInLB, 'LB', 'KG')"/>
                </xsl:element>

                <xsl:element name="HazVolumeCBM">
                  <xsl:value-of select ="ScriptNS2:Convert($TotalHazVolumeInCF, 'CF', 'M3')"/>
                </xsl:element>

              </xsl:element>
            </xsl:if>

          </xsl:element>

          <xsl:variable name="DocumentType" select="ScriptNS0:GetRecipientCode($SenderID , $RecipientID , $TS_Name , 'Status Code' , 'Document Type' , $TriggerPurpose)" />

          <xsl:if test ="$DocumentType != ''">
            <xsl:for-each select ="./*[local-name()='Documents']/*[local-name()='Document' and ./*[local-name()='DocumentType']=$DocumentType]">
              <xsl:sort select ="./*[local-name()='Date']" order ="descending"/>
              <xsl:if test="position()=1">
                <DocumentationDetails>
                  <xsl:variable name="dataType" select="./*[local-name()='DataType']" />
                  <Image>
                    <xsl:choose>
                      <xsl:when test="$dataType='TIF'">
                        <xsl:value-of select="ScriptNS3:ConvertTiff2Pdf(./*[local-name()='Data'])" />
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
          </xsl:if>

        </xsl:element>

      </xsl:element>
  </xsl:template>


  <xsl:template name="PopulateEventDateTime">
    <xsl:param name="DateTime" />
    <xsl:if test="$DateTime != ''">
      <xsl:element name="Date">
        <xsl:value-of select="ScriptNS1:ConvertLocalXmlDateTimeStringToUTC(string($DateTime), 'yyyy-MM-dd')" />
      </xsl:element>
      <xsl:element name="Time">
        <xsl:value-of select="ScriptNS1:ConvertLocalXmlDateTimeStringToUTC(string($DateTime), 'HH:mm:ss')" />
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[
    
   public string GetCutOffRecvWhsDate(string origDateTime)
    {
        string dateFormat = "yyyy-MM-dd";
        DateTime parsedDate;

        if (String.IsNullOrEmpty(origDateTime) || String.IsNullOrEmpty(dateFormat))
        {
            return string.Empty;
        }

        if (DateTime.TryParse(origDateTime, out parsedDate))
        {
            return parsedDate.AddDays(-5).ToString(dateFormat);
        }

        return string.Empty;
    }
    
    double weight = 0;
		double volume = 0;
		public void AddWeight(string value)
		{
			double number;
			if (double.TryParse(value, out number))
			{
				weight += number;
			}
		}

		public double GetTotalWeight()
		{
			return weight;
		}

		public void AddVolume(string value)
		{
			double number;
			if (double.TryParse(value, out number))
			{
				volume += number;
			}
		}

		public double GetTotalVolume()
		{
			return volume;
		}

		public void Reset()
		{
			weight = 0;
			volume = 0;
		}
    
    ]]>
  </msxsl:script>



</xsl:stylesheet>

<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt"
    xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var" exclude-result-prefixes="msxsl var s0 ScriptNS0 ScriptNS1 ScriptNS2 ScriptNS3 userCSharp" version="1.0"
    xmlns:s0="http://www.cargowise.com/Schemas/Universal/2011/11"
    xmlns:ns0="http://cargowise.com/ehub/products/jpcustoms/2013/09"
    xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
    xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1"
    xmlns:ScriptNS2="http://schemas.microsoft.com/BizTalk/2003/ScriptNS2"
    xmlns:ScriptNS3="http://schemas.microsoft.com/BizTalk/2003/ScriptNS3"
    xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
	<xsl:output omit-xml-declaration="yes" indent="no" version="1.0" method="xml" />

	<xsl:template match="/">
		<xsl:apply-templates select="/s0:UniversalShipment" />
	</xsl:template>

	<xsl:template match="/s0:UniversalShipment">
		<xsl:element name="ns0:AHRFlatFileSchemaEnvelope">
      <xsl:variable name="sourceParty" select="ScriptNS2:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
      <xsl:variable name="userCode" select="ScriptNS3:GetUsername($sourceParty)" />
      <xsl:variable name="userPassword" select="ScriptNS3:GetPassword($sourceParty)" />
			<xsl:for-each select="//*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='SubShipmentCollection']/*[local-name()='SubShipment']">
				<xsl:element name="ns0:AHRInput">

					<!-- Segment: Header -->
					<xsl:element name="ns0:InputCommonField">
						<xsl:element name="ns0:ProcessingControlCode">SS</xsl:element>
						<xsl:element name="ns0:ProcedureCode">
							<xsl:choose>
								<xsl:when test="normalize-space(./*[local-name()='DataContext']/*[local-name()='ActionPurpose']/*[local-name()='Code']) = 'REG'">AHR</xsl:when>
								<xsl:otherwise>CHR</xsl:otherwise>
							</xsl:choose>
						</xsl:element>
						<xsl:element name="ns0:ReserveArea1"/>
						<xsl:element name="ns0:UserCode">
							<xsl:value-of select="$userCode" />
						</xsl:element>
						<xsl:element name="ns0:UserID">001</xsl:element>
						<xsl:element name="ns0:UserPassword">
							<xsl:value-of select="$userPassword" />
						</xsl:element>
						<xsl:element name="ns0:ReserveArea2"/>
						<xsl:element name="ns0:MessageTag"/>
						<xsl:element name="ns0:ReserveArea3"/>
						<xsl:element name="ns0:InputMessageID">
							<xsl:value-of select="userCSharp:StringLeftBytes(normalize-space(../../*[local-name()='AddInfoCollection']/*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPInternalTransactionNumber'][1]/*[local-name()='Value']/text()),10)"/>
						</xsl:element>
						<xsl:element name="ns0:IndexTag"/>
						<xsl:element name="ns0:ReserveArea4"/>
						<xsl:element name="ns0:SystemID">2</xsl:element>
						<xsl:element name="ns0:ReserveArea5"/>
						<xsl:element name="ns0:MessageLength"/>
					</xsl:element>
					<xsl:element name="ns0:FunctionTypeCode">
						<xsl:choose>
							<xsl:when test="normalize-space(./*[local-name()='DataContext']/*[local-name()='ActionPurpose']/*[local-name()='Code']) = 'REG'">9</xsl:when>
							<xsl:when test="normalize-space(./*[local-name()='DataContext']/*[local-name()='ActionPurpose']/*[local-name()='Code']) = 'DEL'">1</xsl:when>
							<xsl:when test="normalize-space(./*[local-name()='DataContext']/*[local-name()='ActionPurpose']/*[local-name()='Code']) = 'ADD'">2</xsl:when>
							<xsl:otherwise>5</xsl:otherwise>
						</xsl:choose>
					</xsl:element>
					<xsl:element name="ns0:SPCode">
						<xsl:element name="ns0:SPID">
              <xsl:value-of select="ScriptNS3:GetSPID()" />
						</xsl:element>
						<xsl:element name="ns0:SPPassword">
              <xsl:value-of select="ScriptNS3:GetSPPassword()" />
						</xsl:element>
					</xsl:element>

					<!-- Segment: AA -->
					<xsl:element name="ns0:VesselCode">
						<xsl:variable name="VesselCallSign" select="../../*[local-name()='AddInfoCollection']/*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPVesselCallSign'][1]/*[local-name()='Value']" />
						<xsl:choose>
							<xsl:when test="$VesselCallSign">
								<xsl:value-of select="userCSharp:StringMaxAllowed(normalize-space($VesselCallSign/text()),9)"/>
							</xsl:when>
							<xsl:otherwise>
								<xsl:value-of select="userCSharp:StringMaxAllowed(normalize-space(../../*[local-name()='LloydsIMO']/text()),9)"/>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:element>
					<xsl:element name="ns0:LadenVesselName">
						<xsl:value-of select ="userCSharp:StringLeftBytes(normalize-space(../../*[local-name()='VesselName']/text()),35)"/>
					</xsl:element>
					<xsl:element name="ns0:NationalityCodeOfVessel">
						<xsl:value-of select ="userCSharp:StringMaxAllowed(normalize-space(../../*[local-name()='VesselCountryOfRegistration']/*[local-name()='Code']/text()),2)"/>
					</xsl:element>
					<xsl:element name="ns0:VoyageNumber">
						<xsl:value-of select ="userCSharp:StringMaxAllowed(normalize-space(../../*[local-name()='VoyageFlightNo']/text()),10)"/>
					</xsl:element>

					<!-- Segment: AB -->
					<xsl:variable name="carrierCode" select="userCSharp:StringMaxAllowed( normalize-space(../../*[local-name()='AddInfoCollection']
                        /*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPCarrierCode'][1]
                        /*[local-name()='Value']/text()),4)" />
					<xsl:element name="ns0:CarrierCode">
						<xsl:value-of select ="$carrierCode"/>
					</xsl:element>

					<!-- Segment: AC -->
					<xsl:variable name="PortOfLoading" select="userCSharp:StringMaxAllowed(normalize-space(../../*[local-name()='PortOfLoading']/*[local-name()='Code']/text()),5)"/>
					<xsl:element name="ns0:PortOfLoadingCode">
						<xsl:value-of select="$PortOfLoading" />
					</xsl:element>
					<xsl:element name="ns0:NameOfPortOfLoading">
						<xsl:value-of select="userCSharp:StringLeftBytes(normalize-space(../../*[local-name()='PortOfLoading']/*[local-name()='Name']/text()),20)"/>
					</xsl:element>
					<xsl:element name="ns0:PortOfLoadingSuffix">
						<xsl:value-of select ="userCSharp:StringMaxAllowed(
                            normalize-space(../../*[local-name()='AddInfoCollection']
                            /*[local-name()='AddInfo'
                            and *[local-name()='Key']/text()='JPPortOfLoadingSuffix'][1]
                            /*[local-name()='Value']/text()),1)"/>
					</xsl:element>

					<!-- Segment: AD -->
					<xsl:element name="ns0:MasterBillOfLadingNumber">
						<xsl:value-of select ="userCSharp:StringMaxAllowed(normalize-space(../../*[local-name()='WayBillNumber']/text()),35)"/>
					</xsl:element>
					<xsl:variable name="AllHouseBillRegistered" select="normalize-space(*[local-name()='AddInfoCollection']
                        /*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPHouseBillRegisterCompletion'][1]
                        /*[local-name()='Value']/text())"/>
					<xsl:element name="ns0:HouseBillOfLadingRegisterCompletionID">
						<xsl:if test="$AllHouseBillRegistered ='Y'" >
							<xsl:value-of select="'E'"/>
						</xsl:if>
					</xsl:element>
					<xsl:variable name="HouseBillOfLadingNumber" select="userCSharp:StringMaxAllowed(normalize-space(*[local-name()='WayBillNumber']/text()),35)"/>
					<xsl:if test="$AllHouseBillRegistered!='Y' or $HouseBillOfLadingNumber">
						<xsl:element name="ns0:HouseBillInfo">
							<xsl:element name="ns0:HouseBillOfLadingNumber">
								<xsl:value-of select ="$HouseBillOfLadingNumber"/>
							</xsl:element>

							<!-- Segment: AE -->
							<xsl:variable name="etd" select="../../*[local-name()='DateCollection']
                            /*[local-name()='Date'and *[local-name()='Type']/text()='Departure' and *[local-name()='IsEstimate']/text()='true'][1]
                            /*[local-name()='Value']/text()"/>
							<xsl:variable name="atd" select="../../*[local-name()='DateCollection']
                            /*[local-name()='Date'and *[local-name()='Type']/text()='Departure' and *[local-name()='IsEstimate']/text()='false'][1]
                            /*[local-name()='Value']/text()"/>

							<xsl:element name="ns0:EstimatedDateOfDeparture">
								<xsl:choose>
									<xsl:when test="$etd">
										<xsl:value-of select="ScriptNS1:FormatXmlDateTime($etd, 'yyyyMMdd')"/>
									</xsl:when>
									<xsl:otherwise>
										<xsl:value-of select="ScriptNS1:FormatXmlDateTime($atd, 'yyyyMMdd')"/>
									</xsl:otherwise>
								</xsl:choose>
							</xsl:element>
							<xsl:element name="ns0:EstimatedTimeOfDeparture">
								<xsl:choose>
									<xsl:when test="$etd">
										<xsl:value-of select="ScriptNS1:FormatXmlDateTime($etd, 'HHmm')"/>
									</xsl:when>
									<xsl:otherwise>
										<xsl:value-of select="ScriptNS1:FormatXmlDateTime($atd, 'HHmm')"/>
									</xsl:otherwise>
								</xsl:choose>
							</xsl:element>

							<xsl:element name="ns0:TimeDifferenceFromGMT">
								<xsl:choose>
									<xsl:when test="$etd">
										<xsl:value-of select="translate(ScriptNS0:CallActionProcedureHelper('CalculateTimeZoneOffset', '@offset', '@UNLOCO', string($PortOfLoading), '@localtime', $etd),':','')" />
									</xsl:when>
									<xsl:otherwise>
										<xsl:value-of select="translate(ScriptNS0:CallActionProcedureHelper('CalculateTimeZoneOffset', '@offset', '@UNLOCO', string($PortOfLoading), '@localtime', $atd),':','')" />
									</xsl:otherwise>
								</xsl:choose>
							</xsl:element>

							<!-- Segment: AF -->
							<xsl:element name="ns0:RelaxedApplicationAreaID">
								<xsl:if test="normalize-space(../../*[local-name()='AddInfoCollection']
                        /*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPIsDepartureFromRelaxedArea'][1]
                        /*[local-name()='Value']/text())='Y'" >
									<xsl:value-of select="'Y'"/>
								</xsl:if>
							</xsl:element>

							<!-- Segment: AG -->
							<xsl:element name="ns0:NotificationForwardingPartyCode1">
								<xsl:value-of select="userCSharp:StringMaxAllowed(normalize-space(*[local-name()='AddInfoCollection']
                            /*[local-name()='AddInfo'
                            and *[local-name()='Key']/text()='JPNotificationForwardingPartyCode1'][1]
                            /*[local-name()='Value']/text()),5)"/>
							</xsl:element>
							<xsl:element name="ns0:NotificationForwardingPartyCode2">
								<xsl:value-of select="userCSharp:StringMaxAllowed(normalize-space(*[local-name()='AddInfoCollection']
                            /*[local-name()='AddInfo'
                            and *[local-name()='Key']/text()='JPNotificationForwardingPartyCode2'][1]
                            /*[local-name()='Value']/text()),5)"/>
							</xsl:element>
							<xsl:element name="ns0:NotificationForwardingPartyCode3">
								<xsl:value-of select="userCSharp:StringMaxAllowed(normalize-space(*[local-name()='AddInfoCollection']
                            /*[local-name()='AddInfo'
                            and *[local-name()='Key']/text()='JPNotificationForwardingPartyCode3'][1]
                            /*[local-name()='Value']/text()),5)"/>
							</xsl:element>

							<!-- Segment: AH -->
							<xsl:element name="ns0:PortOfDischargeCode">
								<xsl:value-of select="userCSharp:StringMaxAllowed(normalize-space(../../*[local-name()='PortOfDischarge']
                            /*[local-name()='Code']/text()),5)"/>
							</xsl:element>
							<xsl:element name="ns0:EstimatedDateOfArrival">
								<xsl:choose>
									<xsl:when test="../../*[local-name()='DateCollection']
                            /*[local-name()='Date'and *[local-name()='Type']/text()='Arrival' and *[local-name()='IsEstimate']/text()='true'][1]
                            /*[local-name()='Value']/text()">
										<xsl:value-of select="ScriptNS1:FormatXmlDateTime(../../*[local-name()='DateCollection']
                            /*[local-name()='Date'and *[local-name()='Type']/text()='Arrival' and *[local-name()='IsEstimate']/text()='true'][1]
                            /*[local-name()='Value']/text(), 'yyyyMMdd')"/>
									</xsl:when>
									<xsl:otherwise>
										<xsl:value-of select="ScriptNS1:FormatXmlDateTime(../../*[local-name()='DateCollection']
                            /*[local-name()='Date'and *[local-name()='Type']/text()='Arrival'][1]
                            /*[local-name()='Value']/text(), 'yyyyMMdd')"/>
									</xsl:otherwise>
								</xsl:choose>
							</xsl:element>

							<!-- Segment: AI -->
							<xsl:element name="ns0:PortOfOriginCode">
								<xsl:value-of select="userCSharp:StringMaxAllowed(normalize-space(*[local-name()='PortOfOrigin']
                            /*[local-name()='Code']/text()),5)"/>
							</xsl:element>
							<xsl:element name="ns0:PortOfOriginName">
								<xsl:value-of select="userCSharp:StringLeftBytes(normalize-space(*[local-name()='PortOfOrigin']
                            /*[local-name()='Name']/text()),20)"/>
							</xsl:element>

							<!-- Segment: AJ -->
							<xsl:element name="ns0:PlaceOfDeliveryCode">
								<xsl:value-of select="userCSharp:StringMaxAllowed(normalize-space(*[local-name()='AddInfoCollection']
                            /*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPPlaceOfDeliveryCode'][1]
                            /*[local-name()='Value']/text()),5)"/>
							</xsl:element>
							<xsl:element name="ns0:PlaceOfDeliveryName">
								<xsl:value-of select="userCSharp:StringLeftBytes(normalize-space(*[local-name()='AddInfoCollection']
                            /*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPPlaceOfDeliveryName'][1]
                            /*[local-name()='Value']/text()),20)"/>
							</xsl:element>

							<!-- Segment: AK -->
							<xsl:element name="ns0:FinalDestinationCode">
								<xsl:value-of select="userCSharp:StringMaxAllowed(normalize-space(*[local-name()='PortOfDestination']
                            /*[local-name()='Code']/text()),5)"/>
							</xsl:element>
							<xsl:element name="ns0:FinalDestinationName">
								<xsl:value-of select="userCSharp:StringLeftBytes(normalize-space(*[local-name()='PortOfDestination']
                            /*[local-name()='Name']/text()),20)"/>
							</xsl:element>

							<!-- Segment: AL -->
							<xsl:apply-templates select="*[local-name()='OrganizationAddressCollection']">
								<xsl:with-param name="targetAddressType" select="'Consignor'"/>
								<xsl:with-param name="sourceAddressType" select="'ConsignorDocumentaryAddress'"/>
							</xsl:apply-templates>

							<!-- Segment: AM -->
							<xsl:apply-templates select="*[local-name()='OrganizationAddressCollection']">
								<xsl:with-param name="targetAddressType" select="'Consignee'"/>
								<xsl:with-param name="sourceAddressType" select="'ConsigneeAddress'"/>
							</xsl:apply-templates>

							<!-- Segment: AN -->
							<xsl:apply-templates select="*[local-name()='OrganizationAddressCollection']">
								<xsl:with-param name="targetAddressType" select="'NotifyParty1'"/>
								<xsl:with-param name="sourceAddressType" select="'NotifyParty'"/>
							</xsl:apply-templates>
							<xsl:apply-templates select="*[local-name()='OrganizationAddressCollection']">
								<xsl:with-param name="targetAddressType" select="'NotifyParty2'"/>
								<xsl:with-param name="sourceAddressType" select="'NotifyParty2'"/>
							</xsl:apply-templates>

							<!-- Segment: AO -->
							<xsl:variable name="packingLine" select="*[local-name()='PackingLineCollection']/*[local-name()='PackingLine'][1]" />
							<xsl:element name="ns0:GoodsDescription">
								<xsl:value-of select="userCSharp:StringLeftBytes(normalize-space($packingLine/*[local-name()='GoodsDescription']/text()),350)"/>
							</xsl:element>
							<xsl:element name="ns0:HarmonisedCode">
								<xsl:value-of select="userCSharp:StringLeftBytes(normalize-space($packingLine/*[local-name()='HarmonisedCode']/text()),6)"/>
							</xsl:element>
							<xsl:element name="ns0:MarksAndNumbers">
								<xsl:value-of select="userCSharp:StringLeftBytes(normalize-space($packingLine/*[local-name()='MarksAndNos']/text()),140)"/>
							</xsl:element>
							<xsl:element name="ns0:NumberOfPackages">
								<xsl:value-of select="userCSharp:StringMaxAllowed(normalize-space($packingLine/*[local-name()='PackQty']/text()),8)"/>
							</xsl:element>
							<xsl:element name="ns0:NumberOfPackagesUnitCode">
								<xsl:value-of select="normalize-space($packingLine/*[local-name()='PackType']/*[local-name()='Code']/text())"/>
							</xsl:element>

							<!-- Segment: AQ -->
							<xsl:call-template name="ValueWithUnit">
								<xsl:with-param name="type" select="'Weight'"/>
								<xsl:with-param name="targetPrefix" select="'GrossWeight'"/>
								<xsl:with-param name="valueField" select="$packingLine/*[local-name()='Weight'][1]" />
								<xsl:with-param name="unitField" select="$packingLine/*[local-name()='WeightUnit'][1]"/>
							</xsl:call-template>

							<!-- Segment: AP -->
							<xsl:element name="ns0:NetWeight" />
							<xsl:element name="ns0:NetWeightUnitCode" />

							<!-- Segment: AR -->
							<xsl:call-template name="ValueWithUnit">
								<xsl:with-param name="type" select="'Volume'"/>
								<xsl:with-param name="targetPrefix" select="'Measurement'"/>
								<xsl:with-param name="valueField" select="$packingLine/*[local-name()='Volume'][1]" />
								<xsl:with-param name="unitField" select="$packingLine/*[local-name()='VolumeUnit'][1]"/>
							</xsl:call-template>

							<!-- Segment: AS -->
							<xsl:element name="ns0:CountryOfOriginCode">
								<xsl:value-of select="userCSharp:StringMaxAllowed(normalize-space($packingLine/*[local-name()='CountryOfOrigin']/*[local-name()='Code']/text()),2)"/>
							</xsl:element>

							<!-- Segment: AT -->
							<!-- TODO: VICTEST to be confirmed what's the source of this field-->
							<xsl:element name="ns0:DangerousCargoCode"/>
							<xsl:variable name="UNDGData" select ="$packingLine/*[local-name()='UNDGCollection']/*[local-name()='UNDG'][1]"/>
							<xsl:element name="ns0:IMDGClass">
								<xsl:value-of select="userCSharp:StringMaxAllowed(normalize-space($UNDGData/*[local-name()='IMOClass']/text()),4)"/>
							</xsl:element>
							<xsl:element name="ns0:UNNo">
								<xsl:value-of select="userCSharp:StringLeftBytes(normalize-space($UNDGData/*[local-name()='UNDGCode']/text()),4)"/>
							</xsl:element>

							<!-- Segment: AU -->
							<xsl:variable name="freightCharge" select ="*[local-name()='CommercialInfo']/*[local-name()='CommercialChargeCollection']
                        /*[local-name()='CommercialCharge' 
                        and *[local-name()='ChargeType']/*[local-name()='Code']/text()='OFT'][1]" />
							<xsl:variable name="freightCurrency" select="userCSharp:StringLeftBytes(normalize-space($freightCharge/*[local-name()='Currency']/*[local-name()='Code']/text()),3)"/>
							<xsl:element name="ns0:Freight">
								<xsl:variable name="freight" select="normalize-space($freightCharge/*[local-name()='Amount']/text())"/>
								<xsl:choose>
									<xsl:when test ="not(boolean($freightCurrency))">
									</xsl:when>
									<xsl:when test="($freightCurrency='JPY' or not(contains($freight,'.')))">
										<xsl:value-of select="userCSharp:StringDecimalMaxAllowed($freight,18,0)"/>
									</xsl:when>
									<xsl:otherwise>
										<xsl:value-of select="userCSharp:StringDecimalMaxAllowed($freight,17,2)"/>
									</xsl:otherwise>
								</xsl:choose>
							</xsl:element>
							<xsl:element name="ns0:FreightCurrencyCode">
								<xsl:value-of select="$freightCurrency"/>
							</xsl:element>

							<!-- Segment: AV -->
							<xsl:variable name="valueCurrency" select="userCSharp:StringLeftBytes(normalize-space(*[local-name()='GoodsValueCurrency']/*[local-name()='Code']/text()),3)"/>
							<xsl:element name="ns0:Value">
								<xsl:variable name="value" select="normalize-space(*[local-name()='GoodsValue']/text())"/>
								<xsl:choose>
									<xsl:when test ="not(boolean($valueCurrency))">
									</xsl:when>
									<xsl:when test="($valueCurrency='JPY' or not(contains($value,'.')))">
										<xsl:value-of select="userCSharp:StringDecimalMaxAllowed($value,18,0)"/>
									</xsl:when>
									<xsl:otherwise>
										<xsl:value-of select="userCSharp:StringDecimalMaxAllowed($value,17,2)"/>
									</xsl:otherwise>
								</xsl:choose>
							</xsl:element>
							<xsl:element name="ns0:ValueCurrencyCode">
								<xsl:value-of select="$valueCurrency"/>
							</xsl:element>

							<!-- Segment: AW -->
							<xsl:variable name="transhipmentReasonCode" select="userCSharp:StringMaxAllowed(normalize-space(*[local-name()='AddInfoCollection'] /*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPTranshipmentReasonCode'][1] /*[local-name()='Value']/text()),3)"/>
							<xsl:variable name="transhipmentDuration" select="userCSharp:StringMaxAllowed(normalize-space(*[local-name()='AddInfoCollection'] /*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPTranshipmentDuration'][1] /*[local-name()='Value']/text()),2)"/>
							<xsl:variable name="transhipmentESD" select="normalize-space(*[local-name()='AddInfoCollection']/*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPTranshipmentEstimatedStartDate'][1]/*[local-name()='Value']/text())"/>
							<xsl:variable name="transhipmentEFD" select="normalize-space(*[local-name()='AddInfoCollection']/*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPTranshipmentEstimatedFinishDate'][1]/*[local-name()='Value']/text())"/>
							<xsl:variable name="transhipmentTransportMode" select="userCSharp:StringMaxAllowed( normalize-space(*[local-name()='AddInfoCollection']/*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPTranshipmentTransportMode'][1]/*[local-name()='Value']/text()),2)"/>
							<xsl:variable name="transhipmentArrivalPlaceCode" select="userCSharp:StringMaxAllowed(normalize-space(*[local-name()='AddInfoCollection']/*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPTranshipmentArrivalPlaceCode'][1]/*[local-name()='Value']/text()),5)"/>
							<xsl:variable name="transhipmentArrivalPlaceName" select="userCSharp:StringLeftBytes(normalize-space(*[local-name()='AddInfoCollection']/*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPTranshipmentArrivalPlaceName'][1]/*[local-name()='Value']/text()),35)"/>
							<xsl:element name="ns0:TemporaryLandingID">
								<xsl:if test ="boolean($transhipmentReasonCode) or boolean($transhipmentESD) or boolean($transhipmentEFD) or boolean($transhipmentTransportMode) or boolean(transhipmentArrivalPlaceCode)">
									<xsl:value-of select="28"/>
								</xsl:if>
							</xsl:element>
							<xsl:element name="ns0:ReasonForTemporaryLandingCode">
								<xsl:value-of select="$transhipmentReasonCode"/>
							</xsl:element>
							<xsl:element name="ns0:DurationOfTemporaryLanding">
								<xsl:if test ="(boolean($transhipmentReasonCode) or boolean($transhipmentESD) or boolean($transhipmentEFD) or boolean($transhipmentTransportMode) or boolean(transhipmentArrivalPlaceCode))">
									<xsl:value-of select="$transhipmentDuration"/>
								</xsl:if>
							</xsl:element>
							<xsl:element name="ns0:EstimatedStartDateOfTransportation">
								<xsl:value-of select="ScriptNS1:FormatXmlDateTime($transhipmentESD, 'yyyyMMdd')"/>
							</xsl:element>
							<xsl:element name="ns0:EstimatedFinishDateOfTransportation">
								<xsl:value-of select="ScriptNS1:FormatXmlDateTime($transhipmentEFD, 'yyyyMMdd')"/>
							</xsl:element>
							<xsl:element name="ns0:CustomsTransitOfTemporaryLandingCargo">
								<xsl:value-of select="$transhipmentTransportMode"/>
							</xsl:element>
							<xsl:element name="ns0:ArrivalPlaceCode">
								<xsl:value-of select="$transhipmentArrivalPlaceCode"/>
							</xsl:element>
							<xsl:element name="ns0:ArrivalPlaceName">
								<xsl:value-of select="$transhipmentArrivalPlaceName"/>
							</xsl:element>

							<!-- Segment: AX -->
							<xsl:element name="ns0:CodeOfOtherRelevantLawsAndOrdinances1">
								<xsl:value-of select="userCSharp:StringMaxAllowed(
                            normalize-space(*[local-name()='AddInfoCollection']
                            /*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPOtherRelevantLawCode1'][1]
                            /*[local-name()='Value']/text()),2)"/>
							</xsl:element>
							<xsl:element name="ns0:CodeOfOtherRelevantLawsAndOrdinances2">
								<xsl:value-of select="userCSharp:StringMaxAllowed(
                            normalize-space(*[local-name()='AddInfoCollection']
                            /*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPOtherRelevantLawCode2'][1]
                            /*[local-name()='Value']/text()),2)"/>
							</xsl:element>
							<xsl:element name="ns0:CodeOfOtherRelevantLawsAndOrdinances3">
								<xsl:value-of select="userCSharp:StringMaxAllowed(
                            normalize-space(*[local-name()='AddInfoCollection']
                            /*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPOtherRelevantLawCode3'][1]
                            /*[local-name()='Value']/text()),2)"/>
							</xsl:element>
							<xsl:element name="ns0:CodeOfOtherRelevantLawsAndOrdinances4">
								<xsl:value-of select="userCSharp:StringMaxAllowed(
                            normalize-space(*[local-name()='AddInfoCollection']
                            /*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPOtherRelevantLawCode4'][1]
                            /*[local-name()='Value']/text()),2)"/>
							</xsl:element>
							<xsl:element name="ns0:CodeOfOtherRelevantLawsAndOrdinances5">
								<xsl:value-of select="userCSharp:StringMaxAllowed(
                            normalize-space(*[local-name()='AddInfoCollection']
                            /*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPOtherRelevantLawCode5'][1]
                            /*[local-name()='Value']/text()),2)"/>
							</xsl:element>

							<!-- Segment: AY -->
							<xsl:element name="ns0:Remark">
								<xsl:value-of select="userCSharp:StringLeftBytes(
                            normalize-space(*[local-name()='NoteCollection']
                            /*[local-name()='Note' and *[local-name()='Description']/text()='Remarks'][1]
                            /*[local-name()='NoteText']/text()),140)"/>
							</xsl:element>
							<!-- TODO: VICTEST pending deciding, for now, use the Header DataSource JobNumber-->
							<xsl:element name ="ns0:ReferenceNumberForInternalUse">
								<xsl:value-of select="userCSharp:StringLeftBytes(normalize-space(../..
													/*[local-name()='DataContext']
													/*[local-name()='DataSourceCollection']
													/*[local-name()='DataSource' and *[local-name()='Type']/text()='AFRHeader'][1]
													/*[local-name()='Key']/text()),20)"/>
							</xsl:element>

							<!-- Segment: AZ -->
							<xsl:if test="not(*[local-name()='ContainerCollection']/*[local-name()='Container'])">
								<xsl:element name="ns0:ContainerInfo">
									<xsl:element name="ns0:ContainerNumber" />
									<xsl:element name="ns0:SealNumber1" />
									<xsl:element name="ns0:SealNumber2" />
									<xsl:element name="ns0:SealNumber3" />
									<xsl:element name="ns0:SealNumber4" />
									<xsl:element name="ns0:SealNumber5" />
									<xsl:element name="ns0:SealNumber6" />
									<xsl:element name="ns0:EmptyFullContainerIdentification" />
									<xsl:element name="ns0:ContainerSizeCode" />
									<xsl:element name="ns0:ContainerTypeCode" />
									<xsl:element name="ns0:ContainerOwnershipCode" />
								</xsl:element>
							</xsl:if>
							<xsl:for-each select="*[local-name()='ContainerCollection']/*[local-name()='Container']">
								<xsl:apply-templates select="."/>
							</xsl:for-each>

						</xsl:element>
					</xsl:if>
				</xsl:element>
			</xsl:for-each>
		</xsl:element>
	</xsl:template>

	<xsl:template name="ValueWithUnit">
		<xsl:param name="type" />
		<xsl:param name="targetPrefix" />
		<xsl:param name="valueField" />
		<xsl:param name="unitField" />
		<xsl:element name="{concat('ns0:',$targetPrefix)}">
			<xsl:value-of select="userCSharp:StringDecimalMaxAllowed(normalize-space($valueField/text()),9,3)"/>
		</xsl:element>
		<xsl:element name="{concat('ns0:',$targetPrefix,'UnitCode')}">
			<xsl:variable name="universalUnit" select="userCSharp:StringMaxAllowed(normalize-space($unitField/*[local-name()='Code']/text()),3)" />
			<xsl:choose>
				<!--
				<xsl:when test="$type='Weight' and $universalUnit='KGM'">KGM</xsl:when>
				<xsl:when test="$type='Weight' and $universalUnit='TNE'">TNE</xsl:when>
				<xsl:when test="$type='Weight' and $universalUnit='LBR'">LBR</xsl:when>
				-->
				<xsl:when test="$type='Weight' and $universalUnit='KG'">KGM</xsl:when>
				<xsl:when test="$type='Weight' and $universalUnit='T'">TNE</xsl:when>
				<xsl:when test="$type='Weight' and $universalUnit='LB'">LBR</xsl:when>
				<xsl:when test="$type='Volume' and $universalUnit='M3'">MTQ</xsl:when>
				<xsl:when test="$type='Volume' and $universalUnit='CF'">FTQ</xsl:when>
				<xsl:when test="$type='Volume' and $universalUnit='BF'">BFT</xsl:when>
				<xsl:otherwise>***</xsl:otherwise>
			</xsl:choose>
		</xsl:element>
	</xsl:template>

	<xsl:template name="OrgAddress" match="*[local-name()='OrganizationAddressCollection']">
		<xsl:param name="targetAddressType" />
		<xsl:param name="sourceAddressType" />
		<xsl:variable name="sourceAddress" select="./*[local-name()='OrganizationAddress' and *[local-name()='AddressType']=$sourceAddressType][1]" />
		<xsl:element name="{concat('ns0:',$targetAddressType,'Code')}">
			<xsl:if test="($sourceAddress/*[local-name()='GovRegNumType']/*[local-name()='Code']='REG')" >
				<xsl:value-of select="userCSharp:StringMaxAllowed(normalize-space(*[local-name()='GovRegNum']/text()),12)"/>
			</xsl:if>
		</xsl:element>
		<xsl:element name="{concat('ns0:',$targetAddressType,'Name')}">
			<xsl:value-of select="userCSharp:StringLeftBytes(normalize-space($sourceAddress/*[local-name()='CompanyName']/text()),70)"/>
		</xsl:element>

		<xsl:variable name="lowercase" select="'abcdefghijklmnopqrstuvwxyz'"/>
		<xsl:variable name="uppercase" select="'ABCDEFGHIJKLMNOPQRSTUVWXYZ'"/>
		<xsl:variable name="targetAddress1" select="translate(normalize-space($sourceAddress/*[local-name()='Address1']/text()),$lowercase,$uppercase)" />
		<xsl:variable name="targetAddress2" select="translate(normalize-space($sourceAddress/*[local-name()='Address2']/text()),$lowercase, $uppercase)" />
		<xsl:variable name="targetAddress3" select="translate(normalize-space($sourceAddress/*[local-name()='City']/text()),$lowercase, $uppercase)" />
		<xsl:variable name="targetAddress4" select="translate(normalize-space($sourceAddress/*[local-name()='State']/text()),$lowercase, $uppercase)" />
		<xsl:variable name="targetAddress1and2" select="translate(normalize-space(concat($targetAddress1, ' ', $targetAddress2)),$lowercase, $uppercase)" />
		<xsl:variable name="targetAddress1to4">
			<xsl:value-of select="$targetAddress1and2"/>
			<xsl:if test="$targetAddress3 != ''" >
				<xsl:value-of select="concat(', ', $targetAddress3)"/>
			</xsl:if>
			<xsl:if test="$targetAddress4 != ''" >
				<xsl:value-of select="concat(', ', $targetAddress4)"/>
			</xsl:if>
		</xsl:variable>
		<xsl:choose>
			<xsl:when test="$targetAddress2 = '' or string-length(normalize-space($targetAddress1to4)) &lt; 105">
				<xsl:element name="{concat('ns0:',$targetAddressType,'AddressBlockEntry')}">
					<xsl:value-of select ="normalize-space($targetAddress1to4)"/>
				</xsl:element>
				<xsl:element name="{concat('ns0:',$targetAddressType,'Address1')}"/>
				<xsl:element name="{concat('ns0:',$targetAddressType,'Address2')}"/>
				<xsl:element name="{concat('ns0:',$targetAddressType,'CityName')}"/>
				<xsl:element name="{concat('ns0:',$targetAddressType,'CountrySubEntityName')}"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:variable name="formattedTargetAddress1">
					<xsl:choose>
						<xsl:when test="string-length($targetAddress2) &gt; 35">
							<xsl:value-of select="userCSharp:reformatAddress1and2($targetAddress1and2, 1)"/>
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="$targetAddress1"/>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
				<xsl:variable name="formattedTargetAddress2">
					<xsl:choose>
						<xsl:when test="string-length($targetAddress2) &gt; 35">
							<xsl:value-of select="userCSharp:reformatAddress1and2($targetAddress1and2, 2)"/>
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="$targetAddress2"/>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
				<xsl:element name="{concat('ns0:',$targetAddressType,'AddressBlockEntry')}" />
				<xsl:element name="{concat('ns0:',$targetAddressType,'Address1')}">
					<xsl:value-of select="userCSharp:StringLeftBytes($formattedTargetAddress1,70)"/>
				</xsl:element>
				<xsl:element name="{concat('ns0:',$targetAddressType,'Address2')}">
					<xsl:value-of select="userCSharp:StringLeftBytes($formattedTargetAddress2,35)"/>
				</xsl:element>
				<xsl:element name="{concat('ns0:',$targetAddressType,'CityName')}">
					<xsl:value-of select="userCSharp:StringLeftBytes($targetAddress3, 35)"/>
				</xsl:element>
				<xsl:element name="{concat('ns0:',$targetAddressType,'CountrySubEntityName')}">
					<xsl:value-of select="userCSharp:StringLeftBytes($targetAddress4, 35)"/>
				</xsl:element>
			</xsl:otherwise>
		</xsl:choose>

		<xsl:element name="{concat('ns0:',$targetAddressType,'PostalCode')}">
			<xsl:value-of select="userCSharp:StringMaxAllowed(normalize-space($sourceAddress/*[local-name()='Postcode']/text()),9)"/>
		</xsl:element>
		<xsl:element name="{concat('ns0:',$targetAddressType,'CountryCode')}">
			<xsl:value-of select="userCSharp:StringMaxAllowed(normalize-space($sourceAddress/*[local-name()='Country']/*[local-name()='Code']/text()),2)"/>
		</xsl:element>
		<xsl:element name="{concat('ns0:',$targetAddressType,'TelephoneNumber')}">
			<xsl:variable name="originalPhoneNumber" select="$sourceAddress/*[local-name()='Phone']/text()"/>
			<xsl:variable name="formattedPhoneNumber" select="userCSharp:KeepChars($originalPhoneNumber, '0123456789')" />
			<xsl:if test="string-length($formattedPhoneNumber) &lt; 14">
				<xsl:value-of select="$formattedPhoneNumber"/>
			</xsl:if>
			<xsl:if test="string-length($formattedPhoneNumber) &gt; 14">
				<xsl:value-of select="userCSharp:StringLeftBytes(userCSharp:RemoveCountryCode($originalPhoneNumber,$formattedPhoneNumber), 14)"/>
			</xsl:if>
		</xsl:element>
	</xsl:template>

	<xsl:template name="ContainerInfo" match="*[local-name()='Container']">
		<xsl:element name="ns0:ContainerInfo">
			<xsl:element name="ns0:ContainerNumber">
				<xsl:value-of select="userCSharp:StringMaxAllowed(normalize-space(*[local-name()='ContainerNumber']/text()),12)"/>
			</xsl:element>
			<xsl:variable name="SealNumber1" select="userCSharp:StringMaxAllowed(normalize-space(*[local-name()='Seal']/text()),15)" />
			<xsl:variable name="SealNumber2" select="userCSharp:StringMaxAllowed(normalize-space(*[local-name()='SecondSeal']/text()),15)" />
			<xsl:variable name="SealNumber3" select="userCSharp:StringMaxAllowed(normalize-space(*[local-name()='ThirdSeal']/text()),15)" />
			<xsl:element name="ns0:SealNumber1">
				<xsl:if test="concat($SealNumber1, $SealNumber2, $SealNumber3)=''">
					<xsl:value-of select="'NO SEAL'"/>
				</xsl:if>
				<xsl:value-of select="$SealNumber1"/>
			</xsl:element>
			<xsl:element name="ns0:SealNumber2">
				<xsl:value-of select="$SealNumber2"/>
			</xsl:element>
			<xsl:element name="ns0:SealNumber3">
				<xsl:value-of select="$SealNumber3"/>
			</xsl:element>
			<!-- Note: UniversalXML only supports up to 3 seal number, the remaining are fixed empty -->
			<xsl:element name="ns0:SealNumber4" />
			<xsl:element name="ns0:SealNumber5" />
			<xsl:element name="ns0:SealNumber6" />

			<xsl:element name="ns0:EmptyFullContainerIdentification">
				<xsl:if test="not(normalize-space(*[local-name()='IsEmptyContainer']/text())='Y')">
					<xsl:value-of select="5"/>
				</xsl:if>
			</xsl:element>
			<xsl:variable name="containerLength" select="number(normalize-space(*[local-name()='TotalLength']))"/>
			<xsl:variable name="containerHeight" select="number(normalize-space(*[local-name()='TotalHeight']))"/>
			<xsl:element name="ns0:ContainerSizeCode">
				<xsl:value-of select="userCSharp:GetContainerSizeCode($containerLength, $containerHeight)"/>
			</xsl:element>
			<xsl:variable name="containerType" select="userCSharp:StringMaxAllowed(normalize-space(*[local-name()='ContainerType']/*[local-name()='Category']/*[local-name()='Code']/text()),3)"/>
			<xsl:element name="ns0:ContainerTypeCode">
				<xsl:choose>
					<xsl:when test ="($containerType='**' or $containerType='***' or not(boolean($containerType)))">
						<xsl:value-of select="'**'"/>
					</xsl:when>
					<xsl:when test ="($containerType='GP' or $containerType='DRY')">
						<xsl:value-of select="'GP'"/>
					</xsl:when>
					<xsl:when test ="($containerType='RT' or $containerType='RFG')">
						<xsl:value-of select="'RT'"/>
					</xsl:when>
					<xsl:when test ="($containerType='UT' or $containerType='TOP')">
						<xsl:value-of select="'UT'"/>
					</xsl:when>
					<xsl:when test ="($containerType='PF' or $containerType='FLT')">
						<xsl:value-of select="'PF'"/>
					</xsl:when>
					<xsl:when test ="($containerType='PL' or $containerType='BLS')">
						<xsl:value-of select="'PL'"/>
					</xsl:when>
					<xsl:when test ="($containerType='TN' or $containerType='TNK')">
						<xsl:value-of select="'TN'"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="'SN'"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:element>
			<xsl:element name="ns0:ContainerOwnershipCode">
				<xsl:value-of select="userCSharp:StringMaxAllowed(
                            normalize-space(*[local-name()='AddInfoCollection']
                            /*[local-name()='AddInfo' and *[local-name()='Key']/text()='JPContainerOwnershipCode'][1]
                            /*[local-name()='Value']/text()),2)"/>
			</xsl:element>
		</xsl:element>
	</xsl:template>

	<msxsl:script language="C#" implements-prefix="userCSharp">
		<![CDATA[

public string RemoveCountryCode(string original, string formatted)
{
	var result = string.Empty;
	if (formatted.Length > 14 && original.Trim().StartsWith("+"))
	{
		var countryCode = KeepCharsUntil(original, "0123456789", new[] { ' ' });
		if (countryCode.Length > 0 && countryCode.Length < 4)
			result = formatted.Substring(countryCode.Length);
	}
	return result;
}

public string KeepCharsUntil(string input, string charactersToKeep, char[] charactersToStopOnEncountering)
{
	int IndexOfChars = input.IndexOfAny(charactersToStopOnEncountering);
	if (IndexOfChars < 0) return KeepChars(input, charactersToKeep);
	else
	{
		string NewValue = input.Substring(0, IndexOfChars);
		string ZStringValue = NewValue;
		return KeepChars(ZStringValue, charactersToKeep);
	}
}

public string KeepChars(string input, string keepList)
{
	var Result = new System.Text.StringBuilder(input.Length);
	foreach (char C in input)
	{
		if (keepList.IndexOf(C) != -1)
		{
			Result.Append(C);
		}
	}
	return Result.ToString();
}

public string StringLeftBytes(string text, int lenBytes)
{
    if (System.Text.Encoding.UTF8.GetByteCount(text) <= lenBytes)
        return text.ToUpper();
    byte[] outBytes = new byte[lenBytes];
    int outLen = 0;
    for (int i = 0; i < text.Length; i++)
    {
        byte[] newBytes = System.Text.Encoding.UTF8.GetBytes(text.Substring(i, 1));
        if (outLen + newBytes.Length > lenBytes)
            break;
        else
            newBytes.CopyTo(outBytes, outLen);
        outLen += newBytes.Length;
    }
    return System.Text.Encoding.UTF8.GetString(outBytes, 0, outLen).ToUpper();
}

public string StringMaxAllowed(string text, int lenBytes)
{
    return (System.Text.Encoding.UTF8.GetByteCount(text) > lenBytes) ?  GetInvalidCharacter(lenBytes) : text.ToUpper();
}

public string GetInvalidCharacter(int lenBytes)
{
    return "".PadLeft(lenBytes, '*');
}

public string StringDecimalMaxAllowed(string text, int precision, int scale)
{
    try
    {
        decimal value = decimal.Round(Convert.ToDecimal(text), scale);
        decimal maximumLimit = decimal.Parse("".PadLeft(precision - scale, '9') + "." + "".PadLeft(scale, '9'));
        return value > maximumLimit ? GetInvalidCharacter(precision + 1) : value.ToString("0." + "".PadLeft(scale,'0'));
    }
    catch
    {
        return GetInvalidCharacter(precision + (scale==0 ? 0 : 1));
    }
}

public string reformatAddress1and2(string testAddress, int p)
{
    testAddress = testAddress.Trim();
    var result = string.Empty;
    var splitArray = testAddress.Split(',');
    var splitArraySize = splitArray.Length;
    bool resultFound = false;
    if (splitArraySize > 1)
    {
        for (int i = 1; i < splitArraySize; i++)
        {
            var result1 = aggregateAddressSplit(splitArray, 0, i);
            var result2 = aggregateAddressSplit(splitArray, i, splitArraySize);
            if (result1.Length < 70 && result2.Length < 35)
            {
                result = (p == 1) ? result1 : result2;
                resultFound = true;
            }
        }
    }

    if (!resultFound)
    {
        int position = (testAddress.Length > 70) ? 70 : (int)(testAddress.Length * 0.5);
        result = (p == 1) ? testAddress.Substring(0, position).Trim() : testAddress.Substring(position).Trim();
    }

    return result;
}

public string aggregateAddressSplit(string[] source, int start, int end)
{
    var result = ",";
    if (start < end && end <= source.Length)
    {
        for (int i = start; i < end; i++)
        {
            result += ("," + source[i]);
        }
    }
    return result.Substring(2).Trim();
}


public string GetContainerSizeCode(string lengthStr, string heightStr)
{
    string result = "";
    decimal length = 0m;
    decimal height = 0m;
    decimal.TryParse(lengthStr, out length);
    decimal.TryParse(heightStr, out height);

    if (length >= 10m && length < 20m)
    {
        result = "1";
    }
    else if (length >= 20m && length < 30m)
    {
        result = "2";
    }
    else if (length >= 40m && length < 50m)
    {
        result = "4";
    }
    else
    {
        result = "9";
    }

    if (height >= 4m && height <= 4.25m)
    {
        result += "8";
    }
    else if (height >= 8m && height < 8.5m)
    {
        result += "0";
    }
    else if (height >= 8.5m && height < 9m)
    {
        result += "2";
    }
    else if (height >= 9m && height < 9.5m)
    {
        result += "4";
    }
    else if (height == 9.5m)
    {
        result += "5";
    }
    else if (height > 9.5m)
    {
        result += "6";
    }
    else
    {
        result += "9";
    }
    return result;
}
        ]]>
	</msxsl:script>

</xsl:stylesheet>

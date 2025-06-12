<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet
				exclude-result-prefixes="msxsl var s0 userCSharp"
				version="1.0"
				xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp"
				xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:msxsl="urn:schemas-microsoft-com:xslt"
				xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var"
				xmlns:s0="http://www.cargowise.com/Schemas/Universal/2011/11">

	<xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />

	<xsl:variable name="SendCountry" select="substring(/s0:UniversalShipment/s0:Shipment/s0:DataContext/s0:ActionPurpose/s0:Code/text(),2)"/>
	<xsl:variable name="EntryInstructionLink" select="/s0:UniversalShipment/s0:Shipment/s0:EntryHeaderCollection/s0:EntryHeader/s0:EntryInstructionLink/text()"/>
	<xsl:variable name="Consignee" select="/s0:UniversalShipment/s0:Shipment/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType='ImporterDocumentaryAddress']"/>
	<xsl:variable name="CRLF" select="'&#13;&#10;'"/>
	<xsl:variable name="IsVanuatu" select="$SendCountry='VU'" />
	<xsl:variable name="IsFiJian" select="$SendCountry='FJ'" />
	<xsl:variable name="CurrencyCode">
		<xsl:choose>
			<xsl:when test="$IsFiJian">
				<xsl:text>FJD</xsl:text>
			</xsl:when>
			<xsl:when test="$IsVanuatu">
				<xsl:text>VUV</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>NAD</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>
	<xsl:variable name="CurrencyName">
		<xsl:choose>
			<xsl:when test="$IsFiJian">
				<xsl:text>Fijian Dollar</xsl:text>
			</xsl:when>
			<xsl:when test="$IsVanuatu">
				<xsl:text>Vanuatu Dollar</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>Namibian Dollar</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>
	<xsl:variable name="NoForeignCurrency" select="'No foreign currency'"/>

	<xsl:template name="maximum">
		<xsl:param name="pSequence"/>
		<xsl:for-each select="$pSequence">
			<xsl:sort select="." data-type="number" order="descending"/>
			<xsl:if test="position()=1">
				<xsl:value-of select="."/>
			</xsl:if>
		</xsl:for-each>
	</xsl:template>

	<xsl:template match="/">
		<xsl:apply-templates select="/s0:UniversalShipment/s0:Shipment" />
	</xsl:template>

	<xsl:template match="/s0:UniversalShipment/s0:Shipment">
		<ASYCUDA>
			<xsl:call-template name="Export_release" />
			<xsl:call-template name="Assessment_notice"/>
			<xsl:call-template name="Global_taxes"/>
			<xsl:call-template name="Property"/>
			<xsl:call-template name="Identification"/>
			<xsl:call-template name="Traders"/>
			<xsl:call-template name="Declarant"/>
			<xsl:call-template name="General_information"/>
			<xsl:call-template name="Transport"/>
			<xsl:call-template name="Financial"/>
			<xsl:call-template name="Warehouse"/>
			<xsl:call-template name="Transit"/>
			<xsl:call-template name="Valuation"/>
			<xsl:call-template name="Containers"/>

			<xsl:variable name="HouseWaybillNo" select="./s0:WayBillNumber [../s0:WayBillType/s0:Code = 'HWB'] /text()" />

			<xsl:for-each select="s0:EntryHeaderCollection/s0:EntryHeader/s0:EntryLineCollection/s0:EntryLine">
				<xsl:call-template name="EntryItem">
					<xsl:with-param name="HouseWaybillNo" select="$HouseWaybillNo" />
				</xsl:call-template>
			</xsl:for-each>
		</ASYCUDA>
	</xsl:template>

	<xsl:template name="Export_release">
		<Export_release>
			<Date_of_exit/>
			<Time_of_exit/>
			<Actual_office_of_exit_code>
				<null/>
			</Actual_office_of_exit_code>
			<Actual_office_of_exit_name>
				<null/>
			</Actual_office_of_exit_name>
			<Exit_reference>
				<null/>
			</Exit_reference>
			<Comments>
				<null/>
			</Comments>
		</Export_release>
	</xsl:template>

	<xsl:template name="Assessment_notice">
		<Assessment_notice>
			<Item_tax_total/>
		</Assessment_notice>
	</xsl:template>

	<xsl:template name="Global_taxes">
		<Global_taxes>
			<Global_tax_item/>
		</Global_taxes>
	</xsl:template>

	<xsl:template name="Property">
		<xsl:variable name="MaxEntryLineNumber">
			<xsl:call-template name="maximum">
				<xsl:with-param name="pSequence" select="s0:EntryHeaderCollection/s0:EntryHeader[1]/s0:EntryLineCollection/s0:EntryLine/s0:LineNumber"/>
			</xsl:call-template>
		</xsl:variable>
		<Property>
			<Sad_flow>
				<xsl:variable name="EntryHeadTypeCode" select="s0:EntryHeaderCollection/s0:EntryHeader[1]/s0:Type/s0:Code/text()"/>
				<xsl:choose>
					<xsl:when test="$EntryHeadTypeCode='IMP'">I</xsl:when>
					<xsl:when test="$EntryHeadTypeCode='EXP'">E</xsl:when>
				</xsl:choose>
			</Sad_flow>
			<Date_of_declaration/>
			<Selected_page>1</Selected_page>
			<Forms>
				<Number_of_the_form>1</Number_of_the_form>
				<Total_number_of_forms>
					<xsl:value-of select="userCSharp:GetTotal_number_of_forms($MaxEntryLineNumber)"/>
				</Total_number_of_forms>
			</Forms>
			<Nbers>
				<Number_of_loading_list/>
				<Total_number_of_items>
					<xsl:value-of select="$MaxEntryLineNumber"/>
				</Total_number_of_items>
				<xsl:if test="$IsVanuatu">
					<Total_number_of_packages>
						<xsl:value-of select="s0:TotalNoOfPacks/text()"/>
					</Total_number_of_packages>
				</xsl:if>
			</Nbers>
			<Place_of_declaration>
				<xsl:choose>
					<xsl:when test="$IsVanuatu">
						<null/>
					</xsl:when>
					<xsl:otherwise>
						<Place_of_declaration/>
					</xsl:otherwise>
				</xsl:choose>
			</Place_of_declaration>
		</Property>
	</xsl:template>

	<xsl:template name="Identification">
		<Identification>
			<Manifest_reference_number>
				<xsl:value-of select="s0:ManifestNumber/text()"/>
			</Manifest_reference_number>
			<xsl:if test="not($IsVanuatu)">
				<Total_number_of_packages>
					<xsl:value-of select="s0:TotalNoOfPacks/text()"/>
				</Total_number_of_packages>
			</xsl:if>
			<Office_segment>
				<Customs_clearance_office_code>
					<xsl:value-of select="s0:CustomsOffice/s0:Code/text()"/>
				</Customs_clearance_office_code>
				<Customs_clearance_office_name>
					<xsl:value-of select="s0:CustomsOffice/s0:Description/text()"/>
				</Customs_clearance_office_name>
			</Office_segment>
			<Type>
				<xsl:variable name="EntryInstructionStyle" select="s0:EntryInstructionCollection/s0:EntryInstruction[1]/s0:Style/text()"/>
				<Type_of_declaration>
					<xsl:value-of select="substring($EntryInstructionStyle, 1, 2)"/>
				</Type_of_declaration>
				<Declaration_gen_procedure_code>
					<xsl:value-of select="substring($EntryInstructionStyle, 3, 1)"/>
				</Declaration_gen_procedure_code>
				<Type_of_transit_document>
					<null/>
				</Type_of_transit_document>
			</Type>
			<Registration>
				<Number/>
				<Date/>
				<Serial_number>
					<null/>
				</Serial_number>
			</Registration>
			<Assessment>
				<Number/>
				<Date/>
				<Serial_number>
					<null/>
				</Serial_number>
			</Assessment>
			<reciept>
				<Number/>
				<Date/>
				<Serial_number>
					<null/>
				</Serial_number>
			</reciept>
		</Identification>
	</xsl:template>

	<xsl:template name="Traders">
		<Traders>

			<Exporter>
				<xsl:variable name="Exporter" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType='SupplierDocumentaryAddress']"/>
				<Exporter_code>
					<xsl:value-of select="$Exporter/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code = 'CSC' and s0:CountryOfIssue/s0:Code = $SendCountry]/s0:Value/text()"/>
				</Exporter_code>
				<Exporter_name>
					<xsl:call-template name="ConcatPartyName">
						<xsl:with-param name="Party" select="$Exporter" />
					</xsl:call-template>
				</Exporter_name>
			</Exporter>

			<Consignee>
				<Consignee_code>
					<xsl:value-of select="$Consignee/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code = 'CCD' and s0:CountryOfIssue/s0:Code = $SendCountry]/s0:Value/text()"/>
				</Consignee_code>
				<Consignee_name>
					<xsl:call-template name="ConcatPartyName">
						<xsl:with-param name="Party" select="$Consignee" />
					</xsl:call-template>
				</Consignee_name>
			</Consignee>

			<Financial>
				<Financial_code/>
				<Financial_name/>
			</Financial>

		</Traders>
	</xsl:template>

	<xsl:template name="Declarant">
		<Declarant>
			<xsl:variable name="DeclarantInformation" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType='Declarant']"/>
			<Declarant_code>
				<xsl:value-of select="$DeclarantInformation/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code = 'AGT' and s0:CountryOfIssue/s0:Code = $SendCountry]/s0:Value/text()"/>
			</Declarant_code>
			<Declarant_name>
				<xsl:value-of select="concat($DeclarantInformation/s0:CompanyName/text(), ' ', $DeclarantInformation/s0:Address1/text(), ' ', $DeclarantInformation/s0:Address2/text(), ' ', $DeclarantInformation/s0:City/text(), ' ', $DeclarantInformation/s0:Country/s0:Name/text())"/>
			</Declarant_name>
			<Declarant_representative>
				<xsl:value-of select="s0:CustomsBroker/s0:Name/text()"/>
			</Declarant_representative>
			<Reference>
				<Number>
					<xsl:value-of select="s0:DataContext/s0:DataSourceCollection/s0:DataSource[s0:Type/text()='AsycudaDeclaration']/s0:Key/text()"/>
				</Number>
			</Reference>
		</Declarant>
	</xsl:template>

	<xsl:template name="General_information">
		<General_information>
			<Value_details/>
			<CAP/>
			<Country>
				<xsl:variable name="PortOfDischargeCode" select="substring(s0:PortOfDischarge/s0:Code/text(), 1, 2)"/>
				<Country_first_destination>
					<xsl:value-of select="$PortOfDischargeCode"/>
				</Country_first_destination>
				<Trading_country>
					<xsl:value-of select="$PortOfDischargeCode"/>
				</Trading_country>
				<Export>
					<xsl:variable name ="CountryOfOriginCode" select="substring(s0:PortOfOrigin, 1, 2)"/>
					<Export_country_code>
						<xsl:value-of select="$CountryOfOriginCode"/>
					</Export_country_code>
					<xsl:if test="$IsVanuatu">
						<Export_country_name/>
					</xsl:if>
					<Export_country_region/>
				</Export>
				<Destination>
					<xsl:variable name="PortOfDestinationCode" select="substring(s0:PortOfDestination/s0:Code/text(), 1, 2)"/>
					<Destination_country_code>
						<xsl:value-of select="$Consignee/s0:Country/s0:Code/text()"/>
					</Destination_country_code>
					<xsl:if test="$IsVanuatu">
						<Destination_country_name>
							<null/>
						</Destination_country_name>
					</xsl:if>
					<Destination_country_region/>
				</Destination>
				<xsl:if test="$IsVanuatu">
					<Country_of_origin_name/>
				</xsl:if>
			</Country>
			<Additional_information>
				<null/>
			</Additional_information>
			<Comments_free_text>
				<null/>
			</Comments_free_text>
		</General_information>
	</xsl:template>

	<xsl:template name="Transport">
		<Transport>
			<Container_flag>
				<xsl:choose>
					<xsl:when test="s0:ContainerCount = 0">false</xsl:when>
					<xsl:when test="s0:ContainerCount &gt; 0">true</xsl:when>
				</xsl:choose>
			</Container_flag>
			<Location_of_goods/>
			<Means_of_transport>
				<xsl:variable name="VoyageFlightNo" select="s0:VoyageFlightNo"/>
				<xsl:variable name="CarrierCountryCode" select="s0:TransportLegCollection/s0:TransportLeg[s0:VoyageFlightNo/text()=$VoyageFlightNo]/s0:Carrier/s0:Country/s0:Code/text()"/>
				<xsl:variable name="VesselName" select="s0:VesselName/text()"/>
				<Departure_arrival_information>
					<Identity>
						<xsl:choose>
							<xsl:when test="$VesselName != ''">
								<xsl:value-of select="concat($VesselName,' ',$VoyageFlightNo)" />
							</xsl:when>
							<xsl:otherwise>
								<xsl:value-of select="$VoyageFlightNo" />
							</xsl:otherwise>
						</xsl:choose>
					</Identity>
					<Nationality>
						<xsl:value-of select="$CarrierCountryCode"/>
					</Nationality>
				</Departure_arrival_information>
				<Border_information>
					<Identity>
						<xsl:value-of select="$VoyageFlightNo"/>
					</Identity>
					<Nationality>
						<xsl:value-of select="$CarrierCountryCode"/>
					</Nationality>
					<Mode>
					<xsl:variable name="TransportModeCode" select="s0:TransportMode/s0:Code/text()"/>
						<xsl:choose>
							<xsl:when test="$TransportModeCode = 'SEA'">1</xsl:when>
							<xsl:when test="$TransportModeCode = 'RAI'">2</xsl:when>
							<xsl:when test="$TransportModeCode = 'ROA'">3</xsl:when>
							<xsl:when test="$TransportModeCode = 'AIR'">4</xsl:when>
							<xsl:when test="$TransportModeCode = 'MAI'">5</xsl:when>
							<xsl:when test="$TransportModeCode = 'MUL'">6</xsl:when>
							<xsl:when test="$TransportModeCode = 'FIX'">7</xsl:when>
							<xsl:when test="$TransportModeCode = 'INW'">8</xsl:when>
						</xsl:choose>
					</Mode>
				</Border_information>
				<Inland_mode_of_transport>
					<null/>
				</Inland_mode_of_transport>
			</Means_of_transport>
			<Delivery_terms>
				<Code>
					<xsl:value-of select="s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice[1]/s0:IncoTerm/s0:Code/text()"/>
				</Code>
				<xsl:if test="$IsVanuatu">
					<Place/>
				</xsl:if>
				<Situation/>
			</Delivery_terms>
			<Border_office>
				<Code>
					<xsl:value-of select="s0:CustomsOffice/s0:Code/text()"/>
				</Code>
				<Name>
					<xsl:value-of select="s0:CustomsOffice/s0:Description/text()"/>
				</Name>
			</Border_office>
			<Place_of_loading>
				<Code/>
				<Name/>
				<Country/>
			</Place_of_loading>
		</Transport>
	</xsl:template>

	<xsl:template name="Financial">
		<Financial>
			<xsl:if test="$IsVanuatu">
				<Total_invoice/>
			</xsl:if>
			<Deffered_payment_reference/>
			<Mode_of_payment/>
			<Financial_transaction>
				<Code1/>
				<Code2/>
			</Financial_transaction>
			<Bank>
				<Code/>
				<Name/>
				<Branch/>
				<Reference/>
			</Bank>
			<Terms>
				<Code/>
				<Description/>
			</Terms>
			<Amounts>
				<Total_manual_taxes/>
				<Global_taxes/>
				<Totals_taxes/>
			</Amounts>
			<Guarantee>
				<Name/>
				<xsl:if test="$IsVanuatu">
					<Amount/>
					<Date/>
					<Excluded_country>
						<Code>
							<null/>
						</Code>
						<Name>
							<null/>
						</Name>
					</Excluded_country>
				</xsl:if>
			</Guarantee>
		</Financial>
	</xsl:template>

	<xsl:template name="Warehouse">
		<Warehouse>
			<Identification>
					<xsl:value-of select="./s0:OrganizationAddressCollection
																/s0:OrganizationAddress [./s0:AddressType = 'CustomsWarehouseAddress']
																/s0:RegistrationNumberCollection
																/s0:RegistrationNumber [./s0:Type/s0:Code = 'CCP']
																/s0:Value [1]" />
			</Identification>
			<Delay/>
		</Warehouse>
	</xsl:template>

	<xsl:template name="Transit">
		<Transit>
			<Principal>
				<Code/>
				<Name/>
				<Representative/>
			</Principal>
			<Signature>
				<Place/>
				<Date/>
			</Signature>
			<Destination>
				<Office/>
			</Destination>
			<Seals>
				<Number/>
				<Identity/>
			</Seals>
			<xsl:if test="$IsVanuatu">
				<Result_of_control/>
				<Time_limit/>
			</xsl:if>
			<Officer_name>
				<null/>
			</Officer_name>
		</Transit>
	</xsl:template>

	<xsl:template name="Valuation">
		<xsl:variable name="SumCustomsValueAmount" select="sum(/s0:UniversalShipment/s0:Shipment/s0:EntryHeaderCollection/s0:EntryHeader/s0:EntryLineCollection/s0:EntryLine/s0:CustomsValue)"/>
		
		<Valuation>
			<Calculation_working_mode>2</Calculation_working_mode>
			<Total_cost>0.0</Total_cost>
			<Total_CIF>
				
				<xsl:variable name="SumAmount" select="sum(s0:CommercialInfo/s0:CommercialChargeCollection/s0:CommercialCharge[s0:ChargeType/s0:Code = 'OFT' or s0:ChargeType/s0:Code = 'ONS']/s0:Amount)"/>
				<xsl:variable name="SumInvoiceAmount" select="sum(s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice/s0:InvoiceAmount)"/>
				<xsl:value-of select="$SumAmount + $SumInvoiceAmount"/>
				
			</Total_CIF>
			<Weight>
				<Gross_weight/>
			</Weight>
			<Gs_Invoice>
				<xsl:variable name="commercialInvoice" select="s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice[1]"/>
				<xsl:variable name="commercialInvoiceSumAmount" select="sum(s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice/s0:InvoiceAmount)"/>
				<Amount_national_currency>
					<xsl:value-of select="format-number($SumCustomsValueAmount,'#.')"/>
				</Amount_national_currency>
				<Amount_foreign_currency>
					<xsl:value-of select="format-number($SumCustomsValueAmount,'#.')"/>
				</Amount_foreign_currency>
				<Currency_code>
					<xsl:value-of select="$CurrencyCode"/>
				</Currency_code>
				<Currency_name>
					<xsl:value-of select="$CurrencyName"/>
				</Currency_name>
				<Currency_rate>1</Currency_rate>
			</Gs_Invoice>

			<xsl:variable name="ArrCommercialCharge" select="./s0:CommercialInfo [./s0:Name = 'All Invoices']
																												/s0:CommercialChargeCollection
																												/s0:CommercialCharge" />

			<Gs_external_freight>
				<xsl:call-template name="MapForeignAmount_and_Currency">
					<xsl:with-param name="ArrCommercialCharge" select="$ArrCommercialCharge" />
					<xsl:with-param name="ChargeTypeCode" select="'OFT'" />
				</xsl:call-template>
			</Gs_external_freight>

			<Gs_internal_freight>
				<xsl:call-template name="MapForeignAmount_and_Currency">
					<xsl:with-param name="ArrCommercialCharge" select="$ArrCommercialCharge" />
					<xsl:with-param name="ChargeTypeCode" select="'LCH'" />
				</xsl:call-template>
			</Gs_internal_freight>

			<Gs_insurance>
				<xsl:call-template name="MapForeignAmount_and_Currency">
					<xsl:with-param name="ArrCommercialCharge" select="$ArrCommercialCharge" />
					<xsl:with-param name="ChargeTypeCode" select="'ONS'" />
				</xsl:call-template>
			</Gs_insurance>

			<Gs_other_cost>
				<xsl:call-template name="MapForeignAmount_and_Currency">
					<xsl:with-param name="ArrCommercialCharge" select="$ArrCommercialCharge" />
					<xsl:with-param name="ChargeTypeCode" select="'OTH'" />
				</xsl:call-template>
			</Gs_other_cost>

			<Gs_deduction>
				<xsl:call-template name="MapForeignAmount_and_Currency">
					<xsl:with-param name="ArrCommercialCharge" select="$ArrCommercialCharge" />
					<xsl:with-param name="ChargeTypeCode" select="'DED'" />
				</xsl:call-template>
			</Gs_deduction>

			<xsl:if test="$IsVanuatu">
				<Total>
					<Total_invoice/>
					<Total_weight/>
				</Total>
			</xsl:if>
		</Valuation>
	</xsl:template>

	<xsl:template name="MapForeignAmount_and_Currency">
		<xsl:param name="ArrCommercialCharge" />
		<xsl:param name="ChargeTypeCode" />
		<xsl:variable name="Charge" select="$ArrCommercialCharge [./s0:ChargeType/s0:Code = $ChargeTypeCode]" />
		<xsl:variable name="ChargeAmount" select="sum(/s0:UniversalShipment/s0:Shipment/s0:CommercialInfo/s0:CommercialInvoiceCollection
											/s0:CommercialInvoice/s0:CommercialInvoiceLineCollection/s0:CommercialInvoiceLine[./s0:EntryInstructionLink = $EntryInstructionLink]
											/s0:CommercialChargeCollection/s0:CommercialCharge[s0:ChargeType
											/s0:Code = $ChargeTypeCode]/s0:Amount)"/>
		<xsl:variable name="ChargeCurrencyDesc" select="/s0:UniversalShipment/s0:Shipment/s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice/s0:CommercialInvoiceLineCollection
												/s0:CommercialInvoiceLine[./s0:EntryInstructionLink = $EntryInstructionLink]/s0:CommercialChargeCollection
												/s0:CommercialCharge[./s0:ChargeType/s0:Code=$ChargeTypeCode]/s0:Currency/s0:Description/text()"/>
		<xsl:variable name="ChargeCurrency" select="/s0:UniversalShipment/s0:Shipment/s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice/s0:CommercialInvoiceLineCollection
												/s0:CommercialInvoiceLine[./s0:EntryInstructionLink = $EntryInstructionLink]/s0:CommercialChargeCollection
												/s0:CommercialCharge[./s0:ChargeType/s0:Code=$ChargeTypeCode]/s0:Currency/s0:Code/text()"/>
		
		<Amount_national_currency>
			<xsl:value-of select="format-number( $ChargeAmount, '#0.00')" />
		</Amount_national_currency>
		<Amount_foreign_currency>
			<xsl:value-of select="format-number( $ChargeAmount, '#0.00')" />
		</Amount_foreign_currency>
		<Currency_code>
			<xsl:value-of select="$ChargeCurrency" />
		</Currency_code>
		<Currency_name>
			<xsl:choose>
				<xsl:when test="$ChargeCurrency=$CurrencyCode">
					<xsl:value-of select="$NoForeignCurrency" /></xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$ChargeCurrencyDesc" />
				</xsl:otherwise>
			</xsl:choose>
		</Currency_name>
		<Currency_rate>1.0</Currency_rate>
	</xsl:template>

	<xsl:template name="Containers"> 
		<xsl:for-each select="s0:PackingLineCollection/s0:PackingLine">
			<xsl:variable name="PackageType" select="./s0:PackType/s0:Code" />
			<xsl:variable name="ContainerNumber" select="s0:ContainerNumber/text()" />
			<xsl:variable name="InvoiceLineLink" select="s0:PackedItemCollection/s0:PackedItem[1]/s0:CommercialInvoiceLineLink" />
			<xsl:variable name="RelatedContainer" select="../../s0:ContainerCollection/s0:Container[s0:ContainerNumber=$ContainerNumber][1]" />
			<xsl:variable name="EntryLine" select="/s0:UniversalShipment/s0:Shipment/s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice/s0:CommercialInvoiceLineCollection
												/s0:CommercialInvoiceLine[./s0:EntryInstructionLink = $EntryInstructionLink and ./s0:Link = $InvoiceLineLink]/s0:EntryLineNumber"/>
			<xsl:variable name="GoodsDescription" select="/s0:UniversalShipment/s0:Shipment/s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice/s0:CommercialInvoiceLineCollection
												/s0:CommercialInvoiceLine[./s0:EntryInstructionLink = $EntryInstructionLink and ./s0:Link = $InvoiceLineLink]/s0:Description"/>
			<xsl:variable name="ContainerType" select="$RelatedContainer/s0:ContainerType/s0:Code/text()" />
			<xsl:variable name="ContainerFull" select="$RelatedContainer/s0:FCL_LCL_AIR/s0:Code" />
			<xsl:if test="$EntryLine != ''">
				<Container>
					<Item_Number>
						<xsl:value-of select="$EntryLine"/>
					</Item_Number>
					<Container_identity>
						<xsl:value-of select="$ContainerNumber"/>
					</Container_identity>
					<Container_type>
						<xsl:value-of select="$ContainerType"/>
					</Container_type>
					<Empty_full_indicator>
						<xsl:choose>
							<xsl:when test="$ContainerFull='FCL'">1/1</xsl:when>
							<xsl:when test="$ContainerFull='FCX'">1/1</xsl:when>
							<xsl:when test="$ContainerFull='LCL'">1/2</xsl:when>
							<xsl:when test="$ContainerFull='EMP'">E/E</xsl:when>
						</xsl:choose>
					</Empty_full_indicator>
					<Gross_weight>
						<xsl:value-of select="$RelatedContainer/s0:TareWeight/text()"/>
					</Gross_weight>
					<Goods_description>
						<xsl:value-of select="$GoodsDescription/text()"/>
					</Goods_description>
					<Packages_type>
						<xsl:value-of select="$PackageType/text()"/>
					</Packages_type>
					<Packages_number>
						<xsl:value-of select="./s0:PackQty/text()"/>
					</Packages_number>
					<Packages_weight>
						<xsl:value-of select="./s0:NetWeight/text()"/>
					</Packages_weight>
				</Container>
			</xsl:if>
		</xsl:for-each>	 
	</xsl:template>
	
	<xsl:template name="EntryItem">
		<xsl:param name="HouseWaybillNo" />
		
		<xsl:variable name="EntryLineNumber" select="./s0:LineNumber/text()"/>
		
		<xsl:variable name="InvoiceLine" select="/s0:UniversalShipment/s0:Shipment/s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice/s0:CommercialInvoiceLineCollection
												/s0:CommercialInvoiceLine[./s0:EntryInstructionLink = $EntryInstructionLink and ./s0:EntryLineNumber = $EntryLineNumber]"/>
		
		<Item>
			<xsl:call-template name="Packages">
				<xsl:with-param name="EntryLineNumber" select="$EntryLineNumber" />
			</xsl:call-template>
			
			<xsl:call-template name="IncoTerms"/>
			<xsl:call-template name="Tarification">
				<xsl:with-param name="InvoiceLineObject" select="$InvoiceLine" />
			</xsl:call-template>
			<xsl:call-template name="write_off_unit"/>

			<xsl:call-template name="Goods_description">
				<xsl:with-param name="InvoiceLineObject" select="$InvoiceLine" />
			</xsl:call-template>

			<xsl:call-template name="Previous_doc">
				<xsl:with-param name="HouseWaybillNo" select="$HouseWaybillNo" />
			</xsl:call-template>

			<xsl:call-template name="Licence_number"/>
			<xsl:call-template name="Free_text_1"/>
			<xsl:call-template name="Free_text_2"/>
			<xsl:call-template name="Taxation"/>
			
			<xsl:call-template name="Valuation_item">
				<xsl:with-param name="EntryLineNumber" select="$EntryLineNumber" />
			</xsl:call-template>
		</Item>
	</xsl:template>
	
	<xsl:template name="Packages">
		<xsl:param name="EntryLineNumber" />

		<xsl:variable name="InvoiceLineLink" select="/s0:UniversalShipment/s0:Shipment/s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice/s0:CommercialInvoiceLineCollection
							/s0:CommercialInvoiceLine[./s0:EntryInstructionLink = $EntryInstructionLink and ./s0:EntryLineNumber = $EntryLineNumber]/s0:Link/text()"/>

		<xsl:variable name="PackingLineLink" select="/s0:UniversalShipment/s0:Shipment/s0:PackingLineCollection/s0:PackingLine[./s0:PackedItemCollection/s0:PackedItem/s0:CommercialInvoiceLineLink=$InvoiceLineLink]"/>

		<Packages>
			<Number_of_packages>
				<xsl:value-of select="$PackingLineLink/s0:PackQty/text()"/>
			</Number_of_packages>
			<Marks1_of_packages>
				<xsl:value-of select="$PackingLineLink/s0:MarksAndNos/text()"/>
			</Marks1_of_packages>
			<Marks2_of_packages/>
			<Kind_of_packages_code>
				<xsl:value-of select="$PackingLineLink/s0:PackType/s0:Code/text()"/>
			</Kind_of_packages_code>
			<Kind_of_packages_name/>
		</Packages>
	</xsl:template>

	<xsl:template name="IncoTerms">
		<IncoTerms>
			<Code>
				<xsl:value-of select="../../s0:IncoTerm/s0:Code/text()"/>
			</Code>
			<Place/>
		</IncoTerms>
	</xsl:template>

	<xsl:template name="Tarification">
		<xsl:param name="InvoiceLineObject" />

		<Tarification>
			<xsl:variable name="CustomsProcedureCode" select="$InvoiceLineObject/s0:Procedure/text()" />

			<Extended_customs_procedure>
				<xsl:choose>
					<xsl:when test="string-length($CustomsProcedureCode) &gt; 4">
						<xsl:value-of select="substring($CustomsProcedureCode, 1, 4)" />
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="$CustomsProcedureCode" />
					</xsl:otherwise>
				</xsl:choose>
			</Extended_customs_procedure>

			<National_customs_procedure>
				<xsl:choose>
					<xsl:when test="string-length($CustomsProcedureCode) &gt; 6">
						<xsl:value-of select="substring($CustomsProcedureCode, 5, 3)" />
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="''" />
					</xsl:otherwise>
				</xsl:choose>
			</National_customs_procedure>

			<Valuation_method_code>
				<xsl:value-of select="$InvoiceLineObject/s0:ValuationCode/s0:Code/text()" />
			</Valuation_method_code>

			<xsl:if test="$IsVanuatu">
				<Tarification_data>
					<null/>
				</Tarification_data>
				<Quota_code>
					<null/>
				</Quota_code>
				<Value_item/>
				<Attached_doc_item/>
				<A.I._code>
					<null/>
				</A.I._code>
			</xsl:if>
			
			<HScode>
			
				<Commodity_code>
					<xsl:value-of select="userCSharp:PadRightZero(s0:HarmonisedCode/text(), 8)"/>
				</Commodity_code>
				<Precision_1>000</Precision_1>
				<Precision_2/>
				<Precision_3/>
				<Precision_4/>
			</HScode>
			<Preference_code>
				<xsl:value-of select="$InvoiceLineObject/s0:PrimaryPreference/text()"/>
			</Preference_code>
			<Supplementary_unit>
				<Suppplementary_unit_code>
					<xsl:value-of select="$InvoiceLineObject/s0:CustomsSecondQuantityUnit/s0:Code/text()"/>
				</Suppplementary_unit_code>
				<Supplementary_unit_name/>
				<Suppplementary_unit_quantity>
					<xsl:value-of select="$InvoiceLineObject/s0:CustomsSecondQuantity/text()"/>
				</Suppplementary_unit_quantity>
			</Supplementary_unit>
			<Item_price>
				<xsl:value-of select="./s0:CustomsValue/text()"/>
			</Item_price>
			<Quota>
				<Quota_code/>
			</Quota>
		</Tarification>
	</xsl:template>

	<xsl:template name="write_off_unit">
		<write_off_unit>
			<write_off_unit_code/>
			<write_off_unit_qty/>
		</write_off_unit>
	</xsl:template>

	<xsl:template name="Goods_description">
		<xsl:param name="InvoiceLineObject" />
		<Goods_description>
			<Country_of_origin_code>
				<xsl:value-of select="$InvoiceLineObject/s0:CountryOfOrigin/s0:Code/text()"/>
			</Country_of_origin_code>
			<Country_of_origin_region/>
			<Description_of_goods>
				<xsl:value-of select="s0:Description/text()"/>
			</Description_of_goods>
			<Commercial_Description>
				<xsl:value-of select="./s0:Description/text()"/>
			</Commercial_Description>
		</Goods_description>
	</xsl:template>

	<xsl:template name="Previous_doc">
		<xsl:param name="HouseWaybillNo" />

		<Previous_doc>
			<Summary_declaration>
				<xsl:value-of select="$HouseWaybillNo" />
			</Summary_declaration>
			<Summary_declaration_sl/>
			<Previous_document_reference/>
			<Previous_warehouse_code/>
		</Previous_doc>
	</xsl:template>

	<xsl:template name="Licence_number">
		<xsl:choose>
			<xsl:when test="$IsVanuatu">
				<Licence_number/>
				<Amount_deducted_from_licence/>
				<Quantity_deducted_from_licence/>
			</xsl:when>
			<xsl:otherwise>
				<Licence_number>
					<Licence_number/>
					<Amount_deducted_from_licence/>
					<Quantity_deducted_from_licence/>
				</Licence_number>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="Free_text_1">
		<Free_text_1>
			<null/>
		</Free_text_1>
	</xsl:template>

	<xsl:template name="Free_text_2">
		<Free_text_2>
			<null/>
		</Free_text_2>
	</xsl:template>

	<xsl:template name="Taxation">
		<Taxation>
			<Item_taxes_amount/>
			<Item_taxes_guaranted_amount/>
			<Counter_of_normal_mode_of_payment/>
			<Displayed_item_taxes_amount/>
			<Item_taxes_mode_of_payment>
				<null/>
			</Item_taxes_mode_of_payment>
			<Taxation_line>
				<Duty_tax_Base/>
				<Duty_tax_rate/>
				<Duty_tax_amount/>
				<Duty_tax_code>
					<null/>
				</Duty_tax_code>
				<Duty_tax_MP>
					<null/>
				</Duty_tax_MP>
				<Duty_tax_Type_of_calculation>
					<null/>
				</Duty_tax_Type_of_calculation>
			</Taxation_line>
		</Taxation>
	</xsl:template>

	<xsl:template name="Valuation_item">
		<xsl:param name="EntryLineNumber" />
		
		<xsl:variable name="EntryLineGrossWeight" select="sum(/s0:UniversalShipment/s0:Shipment/s0:CommercialInfo/s0:CommercialInvoiceCollection
											/s0:CommercialInvoice/s0:CommercialInvoiceLineCollection/s0:CommercialInvoiceLine[./s0:EntryInstructionLink = $EntryInstructionLink and ./s0:EntryLineNumber = $EntryLineNumber]
											/s0:Weight)"/>
		<xsl:variable name="EntryLineNetWeight" select="sum(/s0:UniversalShipment/s0:Shipment/s0:CommercialInfo/s0:CommercialInvoiceCollection
											/s0:CommercialInvoice/s0:CommercialInvoiceLineCollection/s0:CommercialInvoiceLine[./s0:EntryInstructionLink = $EntryInstructionLink and ./s0:EntryLineNumber = $EntryLineNumber]
											/s0:NetWeight)"/>

		<Valuation_item>
			<Rate_of_adjustment/>
			<Weight_itm>
				<Gross_weight_itm>
					<xsl:value-of select="$EntryLineGrossWeight"/>					
				</Gross_weight_itm>
				<Net_weight_itm>
					<xsl:value-of select="$EntryLineNetWeight"/>
				</Net_weight_itm>
			</Weight_itm>
			<xsl:if test="$IsVanuatu">
				<Total_cost_itm/>
				<Total_CIF_itm/>
				<Statistical_value/>
				<Alpha_coeficient_of_apportionment/>
			</xsl:if>
			<Item_Invoice>
				<Amount_national_currency>
					<xsl:value-of select="./s0:CustomsValue/text()"/>
				</Amount_national_currency>
				<Amount_foreign_currency>
					<xsl:value-of select="./s0:CustomsValue/text()"/>
				</Amount_foreign_currency>
				<Currency_code>
					<xsl:value-of select="$CurrencyCode"/>
				</Currency_code>
				<xsl:if test="$IsVanuatu">
					<Currency_name>
						<null/>
					</Currency_name>
				</xsl:if>
				<Currency_rate>1.0</Currency_rate>
			</Item_Invoice>
			<item_external_freight>
				<xsl:call-template name="MapForeignAmount_and_Currency_Item">
					<xsl:with-param name="EntryLineNumber" select="$EntryLineNumber" />
					<xsl:with-param name="ChargeTypeCode" select="'OFT'" />
				</xsl:call-template>
			</item_external_freight>
			<item_internal_freight>
				<xsl:call-template name="MapForeignAmount_and_Currency_Item">
					<xsl:with-param name="EntryLineNumber" select="$EntryLineNumber" />
					<xsl:with-param name="ChargeTypeCode" select="'FIF'" />
				</xsl:call-template>
			</item_internal_freight>
			<item_insurance>
				<xsl:call-template name="MapForeignAmount_and_Currency_Item">
					<xsl:with-param name="EntryLineNumber" select="$EntryLineNumber" />
					<xsl:with-param name="ChargeTypeCode" select="'ONS'" />
				</xsl:call-template>
			</item_insurance>
			<item_other_cost>
				<xsl:call-template name="MapForeignAmount_and_Currency_Item">
					<xsl:with-param name="EntryLineNumber" select="$EntryLineNumber" />
					<xsl:with-param name="ChargeTypeCode" select="'OTH'" />
				</xsl:call-template>
			</item_other_cost>
			<item_deduction>
				<xsl:call-template name="MapForeignAmount_and_Currency_Item">
					<xsl:with-param name="EntryLineNumber" select="$EntryLineNumber" />
					<xsl:with-param name="ChargeTypeCode" select="'DED'" />
				</xsl:call-template>
			</item_deduction>
			<xsl:if test="$IsVanuatu">
				<Market_valuer>
					<Rate/>
					<Currency_code>
						<null/>
					</Currency_code>
					<Currency_amount/>
					<Basis_description>
						<null/>
					</Basis_description>
					<Basis_amount/>
				</Market_valuer>
			</xsl:if>
		</Valuation_item>
	</xsl:template>

	<xsl:template name="MapForeignAmount_and_Currency_Item">
		<xsl:param name="EntryLineNumber" />
		<xsl:param name="ChargeTypeCode" />

		<xsl:variable name="ChargeAmount" select="sum(/s0:UniversalShipment/s0:Shipment/s0:CommercialInfo/s0:CommercialInvoiceCollection
											/s0:CommercialInvoice/s0:CommercialInvoiceLineCollection/s0:CommercialInvoiceLine[./s0:EntryInstructionLink = $EntryInstructionLink and ./s0:EntryLineNumber = $EntryLineNumber]
											/s0:CommercialChargeCollection/s0:CommercialCharge[s0:ChargeType
											/s0:Code = $ChargeTypeCode]/s0:Amount)"/>
		<xsl:variable name="ChargeCurrencyDesc" select="/s0:UniversalShipment/s0:Shipment/s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice/s0:CommercialInvoiceLineCollection
												/s0:CommercialInvoiceLine[./s0:EntryInstructionLink = $EntryInstructionLink]/s0:CommercialChargeCollection
												/s0:CommercialCharge[./s0:ChargeType/s0:Code=$ChargeTypeCode]/s0:Currency/s0:Description/text()"/>
		<xsl:variable name="ChargeCurrency" select="/s0:UniversalShipment/s0:Shipment/s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice/s0:CommercialInvoiceLineCollection
												/s0:CommercialInvoiceLine[./s0:EntryInstructionLink = $EntryInstructionLink]/s0:CommercialChargeCollection
												/s0:CommercialCharge[./s0:ChargeType/s0:Code=$ChargeTypeCode]/s0:Currency/s0:Code/text()"/>
		
		<Amount_national_currency>
			<xsl:value-of select="format-number( $ChargeAmount, '#0.00')" />
		</Amount_national_currency>
		<Amount_foreign_currency>
			<xsl:value-of select="format-number( $ChargeAmount, '#0.00')" />
		</Amount_foreign_currency>
		<Currency_code>
			<xsl:value-of select="$ChargeCurrency" />
		</Currency_code>
		<Currency_name>
			<xsl:choose>
				<xsl:when test="$ChargeCurrency='NAD'">
					<xsl:value-of select="$NoForeignCurrency" />
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$ChargeCurrencyDesc" />
				</xsl:otherwise>
			</xsl:choose>
		</Currency_name>
		<Currency_rate>1.0</Currency_rate>
	</xsl:template>
	<xsl:template name="ConcatPartyName">
		<xsl:param name="Party" />
		
		<xsl:variable name="CompanyName" select="normalize-space($Party/s0:CompanyName/text())" />
		<xsl:variable name="Address1"    select="normalize-space($Party/s0:Address1/text())" />
		<xsl:variable name="Address2"    select="normalize-space($Party/s0:Address2/text())" />
		<xsl:variable name="City"        select="normalize-space($Party/s0:City/text())" />
		<xsl:variable name="CountryName" select="normalize-space($Party/s0:Country/s0:Name/text())" />

		<xsl:variable name="PartyName">
			<xsl:value-of select="$CompanyName" />

			<xsl:choose>
				<xsl:when test="$Address1 != ''">
					<xsl:value-of select="concat($CRLF, $Address1)" />
				</xsl:when>
			</xsl:choose>

			<xsl:choose>
				<xsl:when test="$Address2 != ''">
					<xsl:value-of select="concat($CRLF, $Address2)" />
				</xsl:when>
			</xsl:choose>

			<xsl:choose>
				<xsl:when test="$City != ''">
					<xsl:value-of select="concat($CRLF, $City)" />
				</xsl:when>
			</xsl:choose>

			<xsl:choose>
				<xsl:when test="$CountryName != ''">
					<xsl:value-of select="concat($CRLF, $CountryName)" />
				</xsl:when>
			</xsl:choose>
		</xsl:variable>

		<xsl:value-of select="$PartyName" />
	</xsl:template>

	<msxsl:script implements-prefix="userCSharp" language="C#">
		public int GetTotal_number_of_forms(string value)
		{
		int lineNumber = 0;
		if (int.TryParse(value.Trim(), out lineNumber))
		{
		if(lineNumber != 0)
		{
		return (lineNumber + 4) / 3;
		}
		}
		return 0;
		}

		public string PadRightZero(string input, int totalWidth)
		{
		return input.PadRight(totalWidth, '0');
		}
	</msxsl:script>
</xsl:stylesheet>

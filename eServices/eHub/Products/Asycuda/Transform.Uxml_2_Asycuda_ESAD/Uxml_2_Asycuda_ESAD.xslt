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

	<xsl:variable name="EntryInstructionLink" select="/s0:UniversalShipment/s0:Shipment/s0:EntryHeaderCollection/s0:EntryHeader/s0:EntryInstructionLink/text()"/>
	<xsl:variable name="CRLF" select="'&#13;&#10;'"/>
	<xsl:variable name="Consignee" select="/s0:UniversalShipment/s0:Shipment/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType='ImporterDocumentaryAddress']"/>
	<xsl:variable name="MessageType" select="/s0:UniversalShipment/s0:Shipment/s0:MessageType/s0:Code/text()"/>
	<xsl:variable name="MaxTransportLeg">
		<xsl:call-template name="maximum">
			<xsl:with-param name="pSequence" select="/s0:UniversalShipment/s0:Shipment/s0:TransportLegCollection/s0:TransportLeg/s0:LegOrder"/>
		</xsl:call-template>
	</xsl:variable>
	<xsl:variable name="MinTransportLeg">
		<xsl:call-template name="minimum">
			<xsl:with-param name="pSequence" select="/s0:UniversalShipment/s0:Shipment/s0:TransportLegCollection/s0:TransportLeg/s0:LegOrder"/>
		</xsl:call-template>
	</xsl:variable>
	<xsl:variable name="MaxTransport" select="/s0:UniversalShipment/s0:Shipment/s0:TransportLegCollection/s0:TransportLeg[s0:LegOrder=$MaxTransportLeg]"/>
	<xsl:variable name="CurrentTransportLeg">
		<xsl:choose>
			<xsl:when test="$MessageType='IMP'">
				<xsl:value-of select="$MaxTransportLeg"/>
			</xsl:when>
			<xsl:when test="$MessageType='EXP'">
				<xsl:value-of select="$MaxTransportLeg"/>
			</xsl:when>
		</xsl:choose>
	</xsl:variable>
	<xsl:variable name="CurrentTransport" select="/s0:UniversalShipment/s0:Shipment/s0:TransportLegCollection/s0:TransportLeg[s0:LegOrder=$CurrentTransportLeg]"/>

	<xsl:template match="/">
		<xsl:apply-templates select="/s0:UniversalShipment/s0:Shipment" />
	</xsl:template>

	<xsl:template match="/s0:UniversalShipment/s0:Shipment">
		<ESAD>
			<xsl:call-template name="Header_info"/>
			<xsl:call-template name="Traders"/>
			<xsl:call-template name="Transport"/>
			<xsl:call-template name="Financial"/>
			<xsl:call-template name="Items"/>
			<xsl:call-template name="Vehicles"/>
		</ESAD>
	</xsl:template>

	<xsl:template name="Header_info">
		<xsl:variable name="EntryInstructionStyle" select="s0:EntryInstructionCollection/s0:EntryInstruction[1]/s0:Style/text()"/>
		<xsl:variable name="GoodsDescription" select="./s0:GoodsDescription/text()"/>
		<xsl:variable name= "DeclarationKey" select="s0:DataContext/s0:DataSourceCollection/s0:DataSource[s0:Type/text()='AsycudaDeclaration']/s0:Key/text()"/>

		<header>
			<type_of_declaration>
				<xsl:value-of select="substring($EntryInstructionStyle, 1, 2)"/>
			</type_of_declaration>
			<general_procedure_code>
				<xsl:value-of select="substring($EntryInstructionStyle, 3, 1)"/>
			</general_procedure_code>
			<office_of_dispatch_code>
				<xsl:value-of select="s0:CustomsOffice/s0:Code/text()"/>
			</office_of_dispatch_code>
			<tvf_number/>
			<tvf_date/>
			<fcvr_number/>
			<manifest_number>
				<xsl:value-of select="./s0:ManifestNumber/text()"/>
			</manifest_number>
			<declarant>
				<reference_number>
					<xsl:value-of select="$DeclarationKey"/>
				</reference_number>
				<reference_year/>
			</declarant>
			<pst_number/>
			<xsl:call-template name="Header_Totals"/>
			<comments>
				<xsl:value-of select="$GoodsDescription"/>
			</comments>
		</header>
	</xsl:template>

	<xsl:template name="Header_Totals">
		<xsl:variable name="EntryLineNetWeight" select="sum(/s0:UniversalShipment/s0:Shipment/s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice/s0:CommercialInvoiceLineCollection/s0:CommercialInvoiceLine[./s0:EntryInstructionLink = $EntryInstructionLink]/s0:NetWeight)"/>
		<xsl:variable name="MaxEntryLineNumber">
			<xsl:call-template name="maximum">
				<xsl:with-param name="pSequence" select="s0:EntryHeaderCollection/s0:EntryHeader[1]/s0:EntryLineCollection/s0:EntryLine/s0:LineNumber"/>
			</xsl:call-template>
		</xsl:variable>

		<totals>
			<total_number_of_packages/>
			<total_number_of_items>
				<xsl:value-of select="$MaxEntryLineNumber"/>
			</total_number_of_items>
			<total_number_of_containers>
				<xsl:value-of select="s0:ContainerCount/text()"/>
			</total_number_of_containers>
			<total_net_weight>
				<xsl:value-of select="$EntryLineNetWeight"/>
			</total_net_weight>
			<total_gross_weight>
				<xsl:value-of select="$EntryLineNetWeight"/>
			</total_gross_weight>
		</totals>
	</xsl:template>

	<xsl:template name="Traders">
		<xsl:variable name="Exporter" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType='SupplierDocumentaryAddress']"/>

		<names_and_parties>
			<exporter>
				<code>
					<xsl:value-of select="$Exporter/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text() = 'CSC']/s0:Value/text()"/>
				</code>
				<name>
					<xsl:call-template name="ConcatPartyName">
						<xsl:with-param name="Party" select="$Exporter" />
					</xsl:call-template>
				</name>
			</exporter>
			<consignee>
				<code>
					<xsl:value-of select="$Consignee/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text() = 'CCD']/s0:Value/text()"/>
				</code>
				<name>
					<xsl:call-template name="ConcatPartyName">
						<xsl:with-param name="Party" select="$Consignee" />
					</xsl:call-template>
				</name>
			</consignee>
			<declarant>
				<reference_number/>
			</declarant>
			<reference_year/>
			<beneficiary>
				<code/>
				<name/>
			</beneficiary>
		</names_and_parties>
	</xsl:template>

	<xsl:template name="Transport">
		<xsl:variable name="FirstInvoice" select="./s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice[./s0:CommercialInvoiceLineCollection/s0:CommercialInvoiceLine/s0:EntryInstructionLink = $EntryInstructionLink][1]"/>
		<xsl:variable name="CountryOfLastConsignment" select="$MaxTransport/s0:PortOfDischarge/s0:Code/text()"/>

		<transport>
			<shipping_information>
				<country_of_last_consignment>
					<xsl:value-of select="substring($CountryOfLastConsignment, 1, 2)"/>
				</country_of_last_consignment>
				<trading_country>
					<xsl:value-of select="$FirstInvoice/s0:Supplier/s0:Country/s0:Code/text()"/>
				</trading_country>
				<trading_country_name/>
				<country_of_export>
					<xsl:value-of select="$FirstInvoice/s0:Supplier/s0:Country/s0:Code/text()"/>
				</country_of_export>
				<country_of_export_region/>
				<country_of_destination>
					<xsl:value-of select="$Consignee/s0:Country/s0:Code/text()"/>
				</country_of_destination>
				<country_of_destination_region/>
				<place_of_loading_code>
					<xsl:value-of select="./s0:PortOfLoading/s0:Code/text()"/>
				</place_of_loading_code>
				<terms_of_delivery>
					<xsl:value-of select="./s0:ShipmentIncoTerm/s0:Code/text()"/>
				</terms_of_delivery>
				<place_of_delivery/>
			</shipping_information>
			<xsl:call-template name="TransportBorderInformation"/>
			<xsl:call-template name="TransportInlandInformation"/>
			<xsl:call-template name="TransportStorageInformation"/>
			<xsl:call-template name="TransportTransitInformation"/>
		</transport>
	</xsl:template>

	<xsl:template name="TransportBorderInformation">
		<xsl:variable name="VoyageFlightNo" select="$CurrentTransport/s0:VoyageFlightNo/text()"/>
		<xsl:variable name="CarrierCountryCode" select="$CurrentTransport/s0:Carrier/s0:Country/s0:Code/text()"/>
		<xsl:variable name="VesselName" select="$CurrentTransport/s0:VesselName/text()"/>

		<border_information>
			<identity_of_means_of_transport_at_border>
				<xsl:choose>
					<xsl:when test="$VesselName != ''">
						<xsl:value-of select="concat($VesselName,' ',$VoyageFlightNo)" />
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="$VoyageFlightNo" />
					</xsl:otherwise>
				</xsl:choose>
			</identity_of_means_of_transport_at_border>
			<nationality_of_means_of_transport_at_border>
				<xsl:value-of select="$CarrierCountryCode"/>
			</nationality_of_means_of_transport_at_border>
			<nationality_of_means_of_transport_at_border_name/>
			<modeOfTransportAtBorderName>
				<xsl:call-template name="TransportTypeAtBorder"/>
			</modeOfTransportAtBorderName>
			<office_of_entry>
				<xsl:value-of select="./s0:CustomsOffice/s0:Code/text()"/>
			</office_of_entry>
		</border_information>
	</xsl:template>

	<xsl:template name="TransportTypeAtBorder">
		<xsl:variable name="TransportModeCode" select="$CurrentTransport/s0:TransportMode/text()"/>
		<xsl:variable name="TransportModeCodeUpper" select="translate($TransportModeCode, 'abcdefghijklmnopqrstuvwxyz','ABCDEFGHIJKLMNOPQRSTUVWXYZ')"/>

		<xsl:choose>
			<xsl:when test="$TransportModeCodeUpper = 'SEA'">1</xsl:when>
			<xsl:when test="$TransportModeCodeUpper = 'RAI'">2</xsl:when>
			<xsl:when test="$TransportModeCodeUpper = 'ROA'">3</xsl:when>
			<xsl:when test="$TransportModeCodeUpper = 'AIR'">4</xsl:when>
			<xsl:when test="$TransportModeCodeUpper = 'MAI'">5</xsl:when>
			<xsl:when test="$TransportModeCodeUpper = 'MUL'">6</xsl:when>
			<xsl:when test="$TransportModeCodeUpper = 'FIX'">7</xsl:when>
			<xsl:when test="$TransportModeCodeUpper = 'INW'">8</xsl:when>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="TransportInlandInformation">
		<inland_information>
			<inland_mode_of_transport/>
			<inland_mode_of_transport_name/>
			<identity_of_means_of_transport_at_departure/>
			<nationality_of_means_of_transport_at_departure/>
		</inland_information>
	</xsl:template>

	<xsl:template name="TransportStorageInformation">
		<storage_information>
			<location_of_goods>
				<xsl:value-of select="./s0:LocationAtClearance/s0:Code/text()"/>
			</location_of_goods>
			<location_of_goods_name>
				<xsl:value-of select="./s0:LocationAtClearance/s0:Description/text()"/>
			</location_of_goods_name>
			<warehouse_code>
				<xsl:value-of select="./s0:OrganizationAddressCollection/s0:OrganizationAddress[./s0:AddressType='CustomsWarehouseAddress']/s0:RegistrationNumberCollection/s0:RegistrationNumber[./s0:Type/s0:Code='CCP']/s0:Value/text()" />
			</warehouse_code>
			<warehouse_name/>
			<suspense_delay/>
			<origin_warehouse_for_transfer/>
			<origin_warehouse_for_transfer_name/>
			<depreciationInYears/>
			<depreciationInMonths/>
		</storage_information>
	</xsl:template>

	<xsl:template name="TransportTransitInformation">
		<principal>
			<code/>
		</principal>
		<place_of_transit_signature/>
		<date_of_transit_signature/>
		<office_of_destination/>
		<office_of_destination_country/>
		<guarantee_reference_code/>
	</xsl:template>

	<xsl:template name="Financial">
		<financial>
			<xsl:call-template name="FinancialBankingData"/>
			<xsl:call-template name="FinancialValuation"/>
		</financial>
	</xsl:template>

	<xsl:template name="FinancialBankingData">
		<financial_banking_data>
			<nature_of_transaction_code1/>
			<nature_of_transaction_code2/>
			<bank_code/>
			<bank_branch/>
			<bank_file_number/>
			<terms_of_payment/>
			<terms_of_payment_name/>
			<credit_or_prepayment_code/>
		</financial_banking_data>
	</xsl:template>

	<xsl:template name="FinancialValuation">
		<xsl:variable name="SumCustomsValueAmount" select="sum(/s0:UniversalShipment/s0:Shipment/s0:EntryHeaderCollection/s0:EntryHeader/s0:EntryLineCollection/s0:EntryLine/s0:CustomsValue)"/>

		<valuation_note>
			<working_mode>2</working_mode>
			<total_gross_weight/>
			<invoice>
				<amount_in_foreign_currency>
					<xsl:value-of select="format-number($SumCustomsValueAmount,'#.')"/>
				</amount_in_foreign_currency>
				<currency_code>CFA</currency_code>
			</invoice>

			<external_freight>
				<xsl:call-template name="MapForeignAmount_and_Currency">
					<xsl:with-param name="ChargeTypeCode" select="'OFT'" />
				</xsl:call-template>
			</external_freight>

			<internal_freight>
				<xsl:call-template name="MapForeignAmount_and_Currency">
					<xsl:with-param name="ChargeTypeCode" select="'LCH'" />
				</xsl:call-template>
			</internal_freight>
			<insurance>
				<xsl:call-template name="MapForeignAmount_and_Currency">
					<xsl:with-param name="ChargeTypeCode" select="'ONS'" />
				</xsl:call-template>
			</insurance>

			<otherCosts>
				<xsl:call-template name="MapForeignAmount_and_Currency">
					<xsl:with-param name="ChargeTypeCode" select="'OTH'" />
				</xsl:call-template>
			</otherCosts>
			<deductions>
				<xsl:call-template name="MapForeignAmount_and_Currency">
					<xsl:with-param name="ChargeTypeCode" select="'DED'" />
				</xsl:call-template>
			</deductions>
			<amount_of_total_costs/>
			<cost_insurance_freight_total_amount/>
		</valuation_note>
	</xsl:template>

	<xsl:template name="MapForeignAmount_and_Currency">
		<xsl:param name="ChargeTypeCode" />

		<xsl:variable name="ChargeAmount" select="sum(/s0:UniversalShipment/s0:Shipment/s0:CommercialInfo/s0:CommercialInvoiceCollection
											/s0:CommercialInvoice/s0:CommercialInvoiceLineCollection/s0:CommercialInvoiceLine[./s0:EntryInstructionLink = $EntryInstructionLink]
											/s0:CommercialChargeCollection/s0:CommercialCharge[s0:ChargeType
											/s0:Code = $ChargeTypeCode]/s0:Amount)"/>

		<xsl:variable name="ChargeCurrency" select="/s0:UniversalShipment/s0:Shipment/s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice/s0:CommercialInvoiceLineCollection
												/s0:CommercialInvoiceLine[./s0:EntryInstructionLink = $EntryInstructionLink]/s0:CommercialChargeCollection
												/s0:CommercialCharge[./s0:ChargeType/s0:Code=$ChargeTypeCode]/s0:Currency/s0:Code/text()"/>

		<amount_in_foreign_currency>
			<xsl:value-of select="format-number($ChargeAmount, '#0.00')" />
		</amount_in_foreign_currency>
		<currency_code>
			<xsl:value-of select="$ChargeCurrency" />
		</currency_code>
	</xsl:template>

	<xsl:template name="Items">
		<items>
			<xsl:for-each select="s0:EntryHeaderCollection/s0:EntryHeader/s0:EntryLineCollection/s0:EntryLine">
				<xsl:call-template name="Item"/>
			</xsl:for-each>
			<xsl:call-template name="ScannedDocuments"/>
		</items>
	</xsl:template>

	<xsl:template name="Item">
		<xsl:variable name="EntryLineNumber" select="./s0:LineNumber/text()"/>
		<xsl:variable name="InvoiceLineLink" select="/s0:UniversalShipment/s0:Shipment/s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice/s0:CommercialInvoiceLineCollection/s0:CommercialInvoiceLine[./s0:EntryInstructionLink = $EntryInstructionLink and ./s0:EntryLineNumber = $EntryLineNumber]"/>

		<item>
			<item_number/>
			<link_of_transport_document/>

			<xsl:call-template name="ItemDescription">
				<xsl:with-param name="EntryLineNumber" select="$EntryLineNumber" />
				<xsl:with-param name="InvoiceLineLink" select="$InvoiceLineLink" />
			</xsl:call-template>

			<xsl:call-template name="ItemProcedure">
				<xsl:with-param name="EntryLineNumber" select="$EntryLineNumber" />
				<xsl:with-param name="InvoiceLineLink" select="$InvoiceLineLink" />
			</xsl:call-template>

			<xsl:call-template name="ItemValuation">
				<xsl:with-param name="EntryLineNumber" select="$EntryLineNumber" />
				<xsl:with-param name="InvoiceLineLink" select="$InvoiceLineLink" />
			</xsl:call-template>

			<xsl:call-template name="ItemDocuments">
				<xsl:with-param name="EntryLineNumber" select="$EntryLineNumber" />
				<xsl:with-param name="InvoiceLineLink" select="$InvoiceLineLink" />
			</xsl:call-template>

			<xsl:call-template name="ItemContainers">
				<xsl:with-param name="EntryLineNumber" select="$EntryLineNumber" />
			</xsl:call-template>
		</item>
	</xsl:template>

	<xsl:template name="ItemDescription">
		<xsl:param name="EntryLineNumber" />
		<xsl:param name="InvoiceLineLink" />

		<xsl:variable name="EntryLineGrossWeight" select="sum(/s0:UniversalShipment/s0:Shipment/s0:CommercialInfo/s0:CommercialInvoiceCollection
											/s0:CommercialInvoice/s0:CommercialInvoiceLineCollection/s0:CommercialInvoiceLine[s0:EntryInstructionLink = $EntryInstructionLink and s0:EntryLineNumber = $EntryLineNumber]
											/s0:Weight)"/>
		<xsl:variable name="EntryLineNetWeight" select="sum(/s0:UniversalShipment/s0:Shipment/s0:CommercialInfo/s0:CommercialInvoiceCollection
											/s0:CommercialInvoice/s0:CommercialInvoiceLineCollection/s0:CommercialInvoiceLine[s0:EntryInstructionLink = $EntryInstructionLink and s0:EntryLineNumber = $EntryLineNumber]
											/s0:NetWeight)"/>
		<xsl:variable name="EntryLineCustomsSecondQuantity" select="sum(/s0:UniversalShipment/s0:Shipment/s0:CommercialInfo/s0:CommercialInvoiceCollection
											/s0:CommercialInvoice/s0:CommercialInvoiceLineCollection/s0:CommercialInvoiceLine[s0:EntryInstructionLink = $EntryInstructionLink and s0:EntryLineNumber = $EntryLineNumber]
											/s0:CustomsSecondQuantity)"/>

		<description_of_goods>
			<commodity_code>
				<xsl:value-of select="userCSharp:PadRightZero(s0:HarmonisedCode/text(), 8)"/>
			</commodity_code>
			<commodity_code_national_precision>000</commodity_code_national_precision>
			<commodity_code_national_precision5/>
			<commodity_code_national_precision9/>
			<commercial_description>
				<xsl:value-of select="./s0:Description/text()"/>
			</commercial_description>
			<xsl:call-template name="ItemPackages">
				<xsl:with-param name="EntryLineNumber" select="$EntryLineNumber" />
				<xsl:with-param name="InvoiceLineLink" select="$InvoiceLineLink" />
			</xsl:call-template>
			<gross_mass>
				<xsl:value-of select="$EntryLineGrossWeight"/>
			</gross_mass>
			<net_mass>
				<xsl:value-of select="$EntryLineNetWeight"/>
			</net_mass>
			<supplementary_amount>
				<amount>
					<xsl:value-of select="$EntryLineCustomsSecondQuantity"/>
				</amount>
				<amount1/>
				<amount2/>
			</supplementary_amount>
		</description_of_goods>
	</xsl:template>

	<xsl:template name="ItemPackages">
		<xsl:param name="EntryLineNumber" />
		<xsl:param name="InvoiceLineLink" />

		<xsl:variable name="InvoiceLineNumber" select="$InvoiceLineLink/s0:Link/text()"/>
		<xsl:variable name="PackingLineLink" select="/s0:UniversalShipment/s0:Shipment/s0:PackingLineCollection/s0:PackingLine[s0:PackedItemCollection/s0:PackedItem/s0:CommercialInvoiceLineLink=$InvoiceLineNumber]"/>

		<marks_and_numbers1>
			<xsl:value-of select="$PackingLineLink/s0:MarksAndNos/text()"/>
		</marks_and_numbers1>
		<marks_and_numbers2/>
		<package_number>
			<xsl:value-of select="$PackingLineLink/s0:PackQty/text()"/>
		</package_number>
		<package_type_code>
			<xsl:value-of select="$PackingLineLink/s0:PackType/s0:Code/text()"/>
		</package_type_code>
	</xsl:template>

	<xsl:template name="ItemProcedure">
		<xsl:param name="EntryLineNumber" />
		<xsl:param name="InvoiceLineLink" />

		<procedure>
			<country_of_origin_code>
				<xsl:value-of select="$InvoiceLineLink/s0:CountryOfOrigin/s0:Code/text()"/>
			</country_of_origin_code>
			<region/>
			<preference_code>
				<xsl:value-of select="$InvoiceLineLink/s0:PrimaryPreference/text()"/>
			</preference_code>
			<procedure_code>
				<xsl:value-of select="substring($InvoiceLineLink/s0:Procedure/text(), 1, 7)" />
			</procedure_code>
			<procedure_desc/>
			<vm_code>
				<xsl:value-of select="$InvoiceLineLink/s0:ValuationCode/s0:Code/text()"/>
			</vm_code>
			<quota/>
			<required_document_list/>
			<license_number/>
			<d_val/>
			<d_quantity/>
			<previous_document>
				<year/>
				<office_code/>
				<customs_reference_serial/>
				<customs_reference/>
				<customs_reference_item/>
				<customs_registration_date/>
			</previous_document>
			<additional_information/>
			<processing_programs/>
		</procedure>
	</xsl:template>

	<xsl:template name="ItemValuation">
		<xsl:param name="EntryLineNumber" />
		<xsl:param name="InvoiceLineLink" />

		<xsl:variable name="EntryLineForeignValue" select="sum(/s0:UniversalShipment/s0:Shipment/s0:CommercialInfo/s0:CommercialInvoiceCollection
											/s0:CommercialInvoice/s0:CommercialInvoiceLineCollection/s0:CommercialInvoiceLine[s0:EntryInstructionLink = $EntryInstructionLink and s0:EntryLineNumber = $EntryLineNumber]
											/s0:CustomsValue)"/>

		<item_valuation_note>
			<invoice>
				<foreign_currency>
					<xsl:value-of select="format-number($EntryLineForeignValue,'#.')"/>
				</foreign_currency>
				<currency_code>
					<xsl:value-of select="$InvoiceLineLink/../../s0:InvoiceCurrency/s0:Code/text()"/>
				</currency_code>
			</invoice>
			<external_freight>
				<xsl:call-template name="MapForeignAmount_and_Currency_Item">
					<xsl:with-param name="EntryLineNumber" select="$EntryLineNumber" />
					<xsl:with-param name="ChargeTypeCode" select="'OFT'" />
				</xsl:call-template>
			</external_freight>
			<internal_freight>
				<xsl:call-template name="MapForeignAmount_and_Currency_Item">
					<xsl:with-param name="EntryLineNumber" select="$EntryLineNumber" />
					<xsl:with-param name="ChargeTypeCode" select="'LCH'" />
				</xsl:call-template>
			</internal_freight>
			<insurance>
				<xsl:call-template name="MapForeignAmount_and_Currency_Item">
					<xsl:with-param name="EntryLineNumber" select="$EntryLineNumber" />
					<xsl:with-param name="ChargeTypeCode" select="'ONS'" />
				</xsl:call-template>
			</insurance>

			<otherCosts>
				<xsl:call-template name="MapForeignAmount_and_Currency_Item">
					<xsl:with-param name="EntryLineNumber" select="$EntryLineNumber" />
					<xsl:with-param name="ChargeTypeCode" select="'OTH'" />
				</xsl:call-template>
			</otherCosts>
			<deductions>
				<xsl:call-template name="MapForeignAmount_and_Currency_Item">
					<xsl:with-param name="EntryLineNumber" select="$EntryLineNumber" />
					<xsl:with-param name="ChargeTypeCode" select="'DED'" />
				</xsl:call-template>
			</deductions>

			<total_costs>
				<xsl:value-of select="sum(/s0:UniversalShipment/s0:Shipment/s0:CommercialInfo/s0:CommercialInvoiceCollection
											/s0:CommercialInvoice/s0:CommercialInvoiceLineCollection/s0:CommercialInvoiceLine[s0:EntryInstructionLink = $EntryInstructionLink and s0:EntryLineNumber = $EntryLineNumber]
											/s0:CommercialChargeCollection/s0:CommercialCharge/s0:Amount)"/>
			</total_costs>
			<cif_value/>
			<adj_rate/>
			<market_value>
				<price/>
				<basis/>
				<mv_amount/>
			</market_value>
		</item_valuation_note>
	</xsl:template>

	<xsl:template name="MapForeignAmount_and_Currency_Item">
		<xsl:param name="EntryLineNumber" />
		<xsl:param name="ChargeTypeCode" />

		<xsl:variable name="ChargeAmount" select="sum(/s0:UniversalShipment/s0:Shipment/s0:CommercialInfo/s0:CommercialInvoiceCollection
											/s0:CommercialInvoice/s0:CommercialInvoiceLineCollection/s0:CommercialInvoiceLine[s0:EntryInstructionLink = $EntryInstructionLink and s0:EntryLineNumber = $EntryLineNumber]
											/s0:CommercialChargeCollection/s0:CommercialCharge[s0:ChargeType
											/s0:Code = $ChargeTypeCode]/s0:Amount)"/>
		<xsl:variable name="ChargeCurrency" select="/s0:UniversalShipment/s0:Shipment/s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice/s0:CommercialInvoiceLineCollection
												/s0:CommercialInvoiceLine[s0:EntryInstructionLink = $EntryInstructionLink]/s0:CommercialChargeCollection
												/s0:CommercialCharge[s0:ChargeType/s0:Code=$ChargeTypeCode]/s0:Currency/s0:Code/text()"/>

		<foreign_currency>
			<xsl:value-of select="format-number($ChargeAmount,'#.')"/>
		</foreign_currency>
		<currency_code>
			<xsl:value-of select="$ChargeCurrency"/>
		</currency_code>
	</xsl:template>

	<xsl:template name="ItemDocuments">
		<xsl:param name="EntryLineNumber" />
		<xsl:param name="ChargeTypeCode" />

		<attached_documents>
			<attached_document>
				<item_number>
					<xsl:value-of select="$EntryLineNumber"/>
				</item_number>
				<document_code/>
				<reference_no/>
				<date/>
			</attached_document>
		</attached_documents>

	</xsl:template>

	<xsl:template name="ItemContainers">
		<xsl:param name="EntryLineNumber" />

		<containers>
			<!-- loop through all invoice lines for this entry line-->
			<xsl:for-each select="/s0:UniversalShipment/s0:Shipment/s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice/s0:CommercialInvoiceLineCollection
												/s0:CommercialInvoiceLine[./s0:EntryInstructionLink = $EntryInstructionLink and ./s0:EntryLineNumber = $EntryLineNumber]">


				<xsl:call-template name="ItemContainerDetails">
					<xsl:with-param name="InvoiceLineLink" select="./s0:Link/text()" />
				</xsl:call-template>

			</xsl:for-each>
		</containers>
	</xsl:template>

	<xsl:template name="ItemContainerDetails">
		<xsl:param name="InvoiceLineLink" />
		<!-- get all containers linked to invoice line-->
		<!--<xsl:value-of select="name()"/>-->
		<xsl:for-each select="/s0:UniversalShipment/s0:Shipment/s0:PackingLineCollection/s0:PackingLine/s0:PackedItemCollection/s0:PackedItem">
			<xsl:variable name="PackItemInvoiceLine" select="s0:CommercialInvoiceLineLink/text()"/>
			<xsl:if test="$PackItemInvoiceLine = $InvoiceLineLink">
				<xsl:variable name="ContainerNumber" select="../../s0:ContainerNumber/text()" />

				<xsl:variable name="RelatedContainer" select="../../../../s0:ContainerCollection/s0:Container[s0:ContainerNumber=$ContainerNumber][1]" />
				<xsl:variable name="ContainerType" select="$RelatedContainer/s0:ContainerType/s0:Code/text()" />
				<xsl:variable name="ContainerFull" select="$RelatedContainer/s0:FCL_LCL_AIR/s0:Code" />
				<xsl:variable name="TareWeight" select="$RelatedContainer/s0:TareWeight/text()" />
				<xsl:variable name="GoodsDescription" select="/s0:UniversalShipment/s0:Shipment/s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice/s0:CommercialInvoiceLineCollection
												/s0:CommercialInvoiceLine[./s0:EntryInstructionLink = $EntryInstructionLink and ./s0:Link = $InvoiceLineLink]/s0:Description/text()"/>

				<reference>
					<xsl:value-of select="$ContainerNumber"/>
				</reference>
				<container_type_code>
					<xsl:value-of select="$ContainerType"/>
				</container_type_code>
				<efIndicator>
					<xsl:choose>
						<xsl:when test="$ContainerFull='FCL'">1/1</xsl:when>
						<xsl:when test="$ContainerFull='FCX'">1/1</xsl:when>
						<xsl:when test="$ContainerFull='LCL'">1/2</xsl:when>
						<xsl:when test="$ContainerFull='EMP'">E/E</xsl:when>
					</xsl:choose>
				</efIndicator>
				<seals_no/>
				<mark1/>
				<mark2/>
				<goods>
					<xsl:value-of select="$GoodsDescription"/>
				</goods>
				<empty_wgt/>
				<goods_wgt>
					<xsl:value-of select="$TareWeight"/>
				</goods_wgt>
			</xsl:if>

		</xsl:for-each>
	</xsl:template>

	<xsl:template name="ScannedDocuments">
		<scanned_documents>
			<scanned_document>
				<rank/>
				<scanned_document_code/>
				<scanned_document_name/>
				<scanned_document_reference/>
				<scanned_document_date/>
			</scanned_document>
		</scanned_documents>
	</xsl:template>

	<xsl:template name="Vehicles">
		<vehicles>
			<vehicle>
				<rank/>
				<vehicleNumber/>
				<vinNumber/>
				<mark/>
				<typeOfVehicle/>
				<ownerOfVehicle/>
			</vehicle>
		</vehicles>
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

	<xsl:template name="maximum">
		<xsl:param name="pSequence"/>

		<xsl:for-each select="$pSequence">
			<xsl:sort select="." data-type="number" order="descending"/>
			<xsl:if test="position()=1">
				<xsl:value-of select="."/>
			</xsl:if>
		</xsl:for-each>

	</xsl:template>

	<xsl:template name="minimum">
		<xsl:param name="pSequence"/>

		<xsl:for-each select="$pSequence">
			<xsl:sort select="." data-type="number" order="ascending"/>
			<xsl:if test="position()=1">
				<xsl:value-of select="."/>
			</xsl:if>
		</xsl:for-each>
	</xsl:template>

	<msxsl:script implements-prefix="userCSharp" language="C#">
		public string PadRightZero(string input, int totalWidth)
		{
		return input.PadRight(totalWidth, '0');
		}
	</msxsl:script>
</xsl:stylesheet>

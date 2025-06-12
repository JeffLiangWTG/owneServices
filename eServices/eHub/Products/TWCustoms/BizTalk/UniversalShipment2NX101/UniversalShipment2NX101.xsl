<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var" exclude-result-prefixes="msxsl var s0 ScriptNS0 ScriptNS1 ScriptNS2 ScriptNS3 userCSharp" version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
                xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1"
                xmlns:ScriptNS2="http://schemas.microsoft.com/BizTalk/2003/ScriptNS2"
                xmlns:ScriptNS3="http://schemas.microsoft.com/BizTalk/2003/ScriptNS3"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp"
                xmlns="urn:wco:datamodel:TW:NX101:R-00-01">
	<xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes" />

	<xsl:variable name="Shipment" select="(//*[local-name()='Shipment' or local-name()='SubShipment']
                                                              [contains(./*[local-name()='DataContext']/*[local-name()='DataSourceCollection'], 'ForwardingShipment') and 
                                                           not(contains(./*[local-name()='DataContext']/*[local-name()='DataSourceCollection'], 'ForwardingConsol'))] | 
                                  //*[local-name()='Shipment'][contains(./*[local-name()='DataContext']/*[local-name()='DataSourceCollection'], 'CustomsDeclaration') and 
                                                           not(contains(./*[local-name()='DataContext']/*[local-name()='DataSourceCollection'], 'ForwardingConsol')) and 
                                                           not(contains(./*[local-name()='DataContext']/*[local-name()='DataSourceCollection'], 'ForwardingShipment'))] )[1]"/>

	<xsl:variable name="Invoices" select="$Shipment/*[local-name()='CommercialInfo']/*[local-name()='CommercialInvoiceCollection']/*[local-name()='CommercialInvoice']" />
	<xsl:variable name="InvoiceLines" select="$Invoices/*[local-name()='CommercialInvoiceLineCollection']/*[local-name()='CommercialInvoiceLine']" />
	<xsl:variable name="EntryInstructions" select="$Shipment/*[local-name()='EntryInstructionCollection']/*[local-name()='EntryInstruction']" />
	<xsl:variable name="CMAddInfoGroupAddInfo" select="$EntryInstructions/*[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup'][*[local-name()='Type']/*[local-name()='Code']/text() = 'CM']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo']"/>
	<xsl:variable name="CMOrganizationAddress" select="$EntryInstructions/*[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup'][*[local-name()='Type']/*[local-name()='Code']/text() = 'CM']/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress']"/>
	<xsl:variable name="CertificateType" select="$CMAddInfoGroupAddInfo[*[local-name()='Key'] = 'CertificateType']/*[local-name()='Value']/text()"/>
	<xsl:variable name="ProcessingUnit" select="$CMAddInfoGroupAddInfo[*[local-name()='Key'] = 'ProcessingUnit']/*[local-name()='Value']/text()"/>
	<xsl:variable name="IsEstimatedLoadingDate" select="$CMAddInfoGroupAddInfo[*[local-name()='Key'] = 'IsEstimatedLoadingDate']/*[local-name()='Value']/text()"/>
	<xsl:variable name="PreviousPermitNumber" select="$CMAddInfoGroupAddInfo[*[local-name()='Key'] = 'PreviousPermitNumber']/*[local-name()='Value']/text()"/>
	<xsl:variable name="IsSpecialApplication" select="$CMAddInfoGroupAddInfo[*[local-name()='Key'] = 'IsSpecialApplication']/*[local-name()='Value']/text()"/>
	<xsl:variable name="SpecialApplicationId" select="$CMAddInfoGroupAddInfo[*[local-name()='Key'] = 'SpecialApplicationId']/*[local-name()='Value']/text()"/>
	<xsl:variable name="CopyQuantity" select="$CMAddInfoGroupAddInfo[*[local-name()='Key'] = 'CopyQuantity']/*[local-name()='Value']/text()"/>
	<xsl:variable name="ECFAPrintedRemarks" select="userCSharp:NormalizeSpaceAndKeepLineBreaks($CMAddInfoGroupAddInfo[*[local-name()='Key'] = 'ECFA Printed Remarks']/*[local-name()='Value']/text())"/>
	<xsl:variable name="EUSteelProductNo" select="$CMAddInfoGroupAddInfo[*[local-name()='Key'] = 'EUSteelProductNo']/*[local-name()='Value']/text()"/>
	<xsl:variable name="EUSteelProductPhase" select="$CMAddInfoGroupAddInfo[*[local-name()='Key'] = 'EUSteelProductPhase']/*[local-name()='Value']/text()"/>
	<xsl:variable name="ManufacturerPrintingCode" select="$CMAddInfoGroupAddInfo[*[local-name()='Key'] = 'ManufacturerPrintingCode']/*[local-name()='Value']/text()"/>
	<xsl:variable name="Observations" select="$CMAddInfoGroupAddInfo[*[local-name()='Key'] = 'Observations']/*[local-name()='Value']/text()"/>
	<xsl:variable name="NX101_Notes" select="$CMAddInfoGroupAddInfo[*[local-name()='Key'] = 'NX101_Notes']/*[local-name()='Value']/text()"/>
	<xsl:variable name="OriginalQuantity" select="$CMAddInfoGroupAddInfo[*[local-name()='Key'] = 'OriginalQuantity']/*[local-name()='Value']/text()"/>
	<xsl:variable name="ReturnPreviousCOO" select="$CMAddInfoGroupAddInfo[*[local-name()='Key'] = 'ReturnPreviousCOO']/*[local-name()='Value']/text()"/>
	<xsl:variable name="PrintingCode" select="$CMAddInfoGroupAddInfo[*[local-name()='Key'] = 'PrintingCode']/*[local-name()='Value']/text()"/>
	<xsl:variable name="IsTriangularTrade" select="$CMAddInfoGroupAddInfo[*[local-name()='Key'] = 'IsTriangularTrade']/*[local-name()='Value']/text()"/>
	<xsl:variable name="CM_NX101_LinkNumber" select="$CMAddInfoGroupAddInfo[*[local-name()='Key'] = 'Link']/*[local-name()='Value']/text()"/>
	<xsl:variable name="CM_InvoiceLines" select ="$InvoiceLines[*[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup']/*[local-name()='Type']/*[local-name()='Code']/text() = 'CML' and *[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][*[local-name()='Key'] = 'ControllingMessageLink']/*[local-name()='Value']/text() = '1']" />
	<xsl:variable name="EXPEntryNumbers" select="$Shipment/*[local-name()='EntryHeaderCollection']/*[local-name()='EntryHeader']/*[local-name()='EntryNumberCollection']/*[local-name()='EntryNumber'][*[local-name()='Type']/*[local-name()='Code']/text() = 'EXP']" />
	<xsl:variable name="AddInfos" select="$Shipment/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo']" />
	<xsl:variable name="ShippingMarks">
		<xsl:variable name="DistinctInvoiceMarksNumbers">
			<xsl:for-each select="$Shipment/*[local-name()='CommercialInfo']/*[local-name()='CommercialInvoiceCollection']/*[local-name()='CommercialInvoice']/*[local-name()='MarksAndNumbers']">
				<xsl:value-of select="concat(text()[not(.=preceding::*)], ' ')"/>
			</xsl:for-each>
		</xsl:variable>
		<xsl:variable name="InvoiceMarksNumbers" select="userCSharp:NormalizeSpaceAndKeepLineBreaks($DistinctInvoiceMarksNumbers)" />
		<xsl:choose>
			<xsl:when test="$InvoiceMarksNumbers != ''">
				<xsl:value-of select="$InvoiceMarksNumbers"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="userCSharp:NormalizeSpaceAndKeepLineBreaks($Shipment/*[local-name()='NoteCollection']/*[local-name()='Note'][*[local-name()='Description']/text() = 'Marks &amp; Numbers']/*[local-name()='NoteText']/text())"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>
	<xsl:variable name="Remarks">
		<xsl:variable name="CMRemarks" select="$CMAddInfoGroupAddInfo[*[local-name()='Key'] = 'Remarks']/*[local-name()='Value']/text()"/>
		<xsl:variable name="OuterPacks" select="$Shipment/*[local-name()='OuterPacks']/text()"/>
		<xsl:variable name="OuterPacksPackageType" select="$Shipment/*[local-name()='OuterPacksPackageType']/*[local-name()='Code']/text()"/>
		<xsl:value-of select="userCSharp:GetRemarks($CMRemarks, $OuterPacks, $OuterPacksPackageType)"/>
	</xsl:variable>
	<xsl:variable name="Sender" select="'TWCustoms'" />
	<xsl:variable name="Recipient" select="'TWCustoms'" />
	<xsl:variable name="TS_Name" select="'TW NCATK X101'" />

	<xsl:variable name="var:source-party" select="ScriptNS3:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
	<xsl:variable name="var:recipient" select="concat($var:source-party, '_TCA')"/>
	<xsl:variable name="var:new-destination">
		<xsl:choose>
			<xsl:when test="ScriptNS2:CallActionProcedureHelper('SelectClientExists', '', '@ID', $var:recipient) = 'True'">
				<xsl:value-of select="ScriptNS3:SetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties', $var:recipient)"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="userCSharp:ThrowPartyReceiverIDNotFound($var:recipient)"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>

	<xsl:key name="OriginCountryCode" match="*[local-name()='CommercialInvoiceLine']" use="*[local-name()='CountryOfOrigin']/*[local-name()='Code']" />
	<xsl:key name="UnitCode" match="*[local-name()='CommercialInvoiceLine']" use="*[local-name()='InvoiceQuantityUnit']/*[local-name()='Code']" />

	<xsl:template match="/">
		<xsl:apply-templates select="/*[local-name()='UniversalInterchange']" />
	</xsl:template>
	<xsl:template match="/*[local-name()='UniversalInterchange']">

		<xsl:element name="Declaration">
			<FunctionalReferenceID></FunctionalReferenceID>
			<xsl:element name="FunctionCode">
				<xsl:text>9</xsl:text>
			</xsl:element>
			<xsl:call-template name="Consignment" />
			<xsl:call-template name="GoodsShipment" />
			<xsl:call-template name="GovernmentProcedure" />
			<xsl:call-template name="Packaging" />
			<xsl:call-template name="PreviousDocument" />
			<xsl:call-template name ="tw_Application" />
			<xsl:call-template name ="tw_COImporter" />
		</xsl:element>
	</xsl:template>

	<xsl:template name="Consignment">
		<xsl:element name="Consignment">
			<xsl:call-template name="Consignment_AdditionalDocument" />
			<xsl:element name="GovernmentAgencyGoodsItem">
				<xsl:call-template name="Consignment_GovernmentAgencyGoodsItem_Manufacturer" />
				<xsl:call-template name="Consignment_GovernmentAgencyGoodsItem_Origin" />
				<xsl:call-template name="Consignment_GovernmentAgencyGoodsItem_PreviousDocument" />
			</xsl:element>
			<xsl:call-template name="Consignment_UnloadingLocation" />
		</xsl:element>
	</xsl:template>

	<xsl:template name="Consignment_AdditionalDocument">
		<xsl:for-each select="$EntryInstructions/*[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup'][*[local-name()='Type']/*[local-name()='Code']/text() = 'CON']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo']/*[local-name()='Value']">
			<xsl:variable name="DocumentNumber" select="text()[not(.=preceding::*)]"/>
			<xsl:if test="$DocumentNumber != ''">
				<xsl:element name="AdditionalDocument">
					<xsl:element name ="ID">
						<xsl:value-of select="$DocumentNumber"/>
					</xsl:element>
				</xsl:element>
			</xsl:if>
		</xsl:for-each>
	</xsl:template>

	<xsl:template name="Consignment_GovernmentAgencyGoodsItem_Manufacturer">
		<xsl:variable name="LocalProcessor" select="$CMOrganizationAddress[*[local-name()='AddressType'] = 'LocalProcessorAddress']" />
		<xsl:variable name="LocalProcessorTranslated" select="$CMOrganizationAddress[*[local-name()='AddressType'] = 'LocalProcessorTranslatedDocAddress']" />
		<xsl:if test="$LocalProcessor">
			<xsl:call-template name="GenerateManufacturer">
				<xsl:with-param name="Party" select="$LocalProcessor" />
				<xsl:with-param name="PartyTranslated" select="$LocalProcessorTranslated" />
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template name="Consignment_GovernmentAgencyGoodsItem_Origin">
		<xsl:for-each select="$CM_InvoiceLines[generate-id(.) = generate-id(key('OriginCountryCode', *[local-name()='CountryOfOrigin']/*[local-name()='Code'])[1])]">
			<xsl:variable name="CountryOfOrigin" select="normalize-space(*[local-name()='CountryOfOrigin']/*[local-name()='Code']/text())" />
			<xsl:element name="Origin">
				<xsl:element name="CountryCode">
					<xsl:value-of select="$CountryOfOrigin"/>
				</xsl:element>
			</xsl:element>
		</xsl:for-each>
	</xsl:template>

	<xsl:template name="Consignment_GovernmentAgencyGoodsItem_PreviousDocument">
		<xsl:for-each select="$EntryInstructions/*[local-name()='AddInfoGroupCollection']/*[local-name()='AddInfoGroup'][*[local-name()='Type']/*[local-name()='Code']/text() = 'PDN']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo']/*[local-name()='Value']">
			<xsl:variable name="DocumentNumber" select="text()[not(.=preceding::*)]"/>
			<xsl:if test="$DocumentNumber != ''">
				<xsl:element name="PreviousDocument">
					<xsl:element name ="ID">
						<xsl:value-of select="$DocumentNumber"/>
					</xsl:element>
				</xsl:element>
			</xsl:if>
		</xsl:for-each>
	</xsl:template>

	<xsl:template name="Consignment_UnloadingLocation">
		<xsl:element name="UnloadingLocation">
			<xsl:variable name="PortOfDestination" select="$Shipment/*[local-name()='PortOfDestination']" />
			<xsl:element name ="ID">
				<xsl:value-of select="normalize-space($PortOfDestination/*[local-name()='Code']/text())"/>
			</xsl:element>
			<xsl:element name ="Name">
				<xsl:value-of select="normalize-space($PortOfDestination/*[local-name()='Name']/text())"/>
			</xsl:element>
		</xsl:element>
	</xsl:template>

	<xsl:template name="GoodsShipment">
		<xsl:element name="GoodsShipment">
			<xsl:call-template name="GoodsShipment_Consignment" />
			<xsl:call-template name="GoodsShipment_Exporter" />
			<xsl:call-template name="GoodsShipment_GoodsMeasure" />
			<xsl:call-template name="GoodsShipment_GovernmentAgencyGoodsItem" />
			<xsl:if test="$CertificateType != '15'">
				<xsl:for-each select="$CM_InvoiceLines">
					<xsl:call-template name="Generate_tw_AdditionalDeclaration">
						<xsl:with-param name="InvoiceLine" select="." />
					</xsl:call-template>
				</xsl:for-each>
			</xsl:if>
		</xsl:element>
	</xsl:template>

	<xsl:template name="GovernmentProcedure">
		<xsl:if test="$Remarks != ''">
			<xsl:for-each select="userCSharp:DivideString($Remarks, 256, 4)">
				<xsl:variable name="ds" select="./text()" />
				<xsl:element name="GovernmentProcedure">
					<xsl:element name="Description">
						<xsl:value-of select="$ds"/>
					</xsl:element>
				</xsl:element>
			</xsl:for-each>
		</xsl:if>
	</xsl:template>

	<xsl:template name="GoodsShipment_Consignment">
		<xsl:element name="Consignment">

			<xsl:if test="$NX101_Notes != ''">
				<xsl:for-each select="userCSharp:DivideString($NX101_Notes, 256, 2)">
					<xsl:variable name="ds" select="./text()" />
					<xsl:element name="AdditionalInformation">
						<xsl:element name="StatementDescription">
							<xsl:value-of select="$ds"/>
						</xsl:element>
					</xsl:element>
				</xsl:for-each>
			</xsl:if>

			<xsl:variable name="VoyageFlightNo" select="normalize-space($Shipment/*[local-name()='VoyageFlightNo']/text())" />
			<xsl:if test="$VoyageFlightNo != ''">
				<xsl:element name="BorderTransportMeans">
					<xsl:element name="JourneyID">
						<xsl:value-of select="$VoyageFlightNo"/>
					</xsl:element>
				</xsl:element>
			</xsl:if>

			<xsl:variable name="VesselName" select="normalize-space($Shipment/*[local-name()='VesselName']/text())" />
			<xsl:if test="$VesselName != ''">
				<xsl:element name="DepartureTransportMeans">
					<xsl:element name="Name">
						<xsl:value-of select="$VesselName"/>
					</xsl:element>
				</xsl:element>
			</xsl:if>

			<xsl:element name="LoadingLocation">
				<xsl:variable name="PortOfOriginCode" select="normalize-space($Shipment/*[local-name()='PortOfOrigin']/*[local-name()='Code']/text())" />
				<xsl:if test="$PortOfOriginCode != ''">
					<xsl:element name="ID">
						<xsl:value-of select="$PortOfOriginCode"/>
					</xsl:element>
				</xsl:if>
				<xsl:variable name="LoadingDate" select="ScriptNS1:FormatXmlDateTime(normalize-space($Shipment/*[local-name()='DateCollection']/*[local-name()='Date'][*[local-name()='Type']/text() = 'LoadingDate']/*[local-name()='Value']/text()), 'yyyy-MM-dd')"/>
				<xsl:call-template name="GenerateNodesIfNotEmpty">
					<xsl:with-param name="NodeName" select="'LoadingDateTime'" />
					<xsl:with-param name="Value" select="$LoadingDate" />
				</xsl:call-template>
				<xsl:if test="$PortOfOriginCode != ''">
					<xsl:variable name="Z99" select="'Z99'"/>
					<xsl:choose>
						<xsl:when test="$Z99 = substring($PortOfOriginCode, string-length($PortOfOriginCode) - string-length($Z99) + 1)">
							<xsl:call-template name="GenerateNodesIfNotEmpty">
								<xsl:with-param name="NodeName" select="'Name'" />
								<xsl:with-param name="Value" select="$AddInfos[*[local-name()='Key'] = 'Z99PortOfOrigin']/*[local-name()='Value']/text()" />
							</xsl:call-template>
						</xsl:when>
						<xsl:otherwise>
							<xsl:call-template name="GenerateNodesIfNotEmpty">
								<xsl:with-param name="NodeName" select="'Name'" />
								<xsl:with-param name="Value" select="$Shipment/*[local-name()='PortOfOrigin']/*[local-name()='Name']/text()" />
							</xsl:call-template>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:if>
				<xsl:if test="$CertificateType = '15'">
					<xsl:element name="tw_EstimatedLoadingCode">
						<xsl:choose>
							<xsl:when test="$IsEstimatedLoadingDate= 'Y'">
								<xsl:value-of select="'Y'"/>
							</xsl:when>
							<xsl:otherwise>
								<xsl:value-of select="'N'"/>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:element>
				</xsl:if>
			</xsl:element>

			<xsl:for-each select="$Shipment/*[local-name()='ContainerCollection']/*[local-name()='Container']/*[local-name()='ContainerNumber']">
				<xsl:variable name="ContainerNumber" select="text()[not(.=preceding::*)]"/>
				<xsl:if test="$ContainerNumber != ''">
					<xsl:element name="TransportEquipment">
						<xsl:element name ="ID">
							<xsl:value-of select="$ContainerNumber"/>
						</xsl:element>
					</xsl:element>
				</xsl:if>
			</xsl:for-each>

			<xsl:variable name="PortOfDestination" select="normalize-space($Shipment/*[local-name()='PortOfDestination']/*[local-name()='Code']/text())" />
			<xsl:if test="$PortOfDestination != ''">
				<xsl:element name="UnloadingLocation">
					<xsl:element name="ID">
						<xsl:value-of select="$PortOfDestination"/>
					</xsl:element>
				</xsl:element>
			</xsl:if>

		</xsl:element>
	</xsl:template>

	<xsl:template name="GoodsShipment_Exporter">
		<xsl:variable name="Exporter" select="$CMOrganizationAddress[*[local-name()='AddressType'] = 'SupplierDocumentaryAddress']" />
		<xsl:variable name="ExporterTranslated" select="$CMOrganizationAddress[*[local-name()='AddressType'] = 'SupplierTranslatedDocumentaryAddress']" />
		<xsl:if test="$Exporter">
			<xsl:call-template name="GeneratePartyInFull">
				<xsl:with-param name="ElementName" select="'Exporter'" />
				<xsl:with-param name="Party" select="$Exporter" />
				<xsl:with-param name="PartyTranslated" select="$ExporterTranslated" />
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template name="GoodsShipment_GoodsMeasure">
		<xsl:for-each select="$CM_InvoiceLines[generate-id(.) = generate-id(key('UnitCode', *[local-name()='InvoiceQuantityUnit']/*[local-name()='Code'])[1])]">
			<xsl:variable name="InvoiceQuantityUnit" select="normalize-space(*[local-name()='InvoiceQuantityUnit']/*[local-name()='Code']/text())" />
			<xsl:if test="$InvoiceQuantityUnit">
				<xsl:element name="GoodsMeasure">
					<xsl:element name="TariffQuantity">
						<xsl:value-of select="sum($CM_InvoiceLines[*[local-name()='InvoiceQuantityUnit']/*[local-name()='Code'] = $InvoiceQuantityUnit]/*[local-name()='InvoiceQuantity'])"/>
					</xsl:element>
					<xsl:element name="tw_CustomUnitCode">
						<xsl:choose>
							<xsl:when test="$CertificateType = '15'">
								<xsl:value-of select="ScriptNS2:GetRecipientCode($Sender, $Recipient, $TS_Name, 'Measure Unit', 'Measure Unit - X101', $InvoiceQuantityUnit)"/>
							</xsl:when>
							<xsl:otherwise>
								<xsl:value-of select="$InvoiceQuantityUnit"/>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:element>
				</xsl:element>
			</xsl:if>
		</xsl:for-each>
	</xsl:template>
	
	<xsl:template name="GoodsShipment_GovernmentAgencyGoodsItem">
		<xsl:variable name="SequenceNumber" select="1"/>
		<xsl:for-each select="$CM_InvoiceLines">
			<xsl:sort select="../../*[local-name()='InvoiceNumber']" order="ascending"/>
			<xsl:sort select="*[local-name()='LineNo']" order="ascending" data-type="number"/>
			<xsl:variable name="InvoiceLineGroupingAddInfo" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][*[local-name()='Key']/text() = 'Group']" />
			<xsl:variable name="CM_InvoiceLineAddInfo" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo']"/>
			<xsl:variable name="PrintingTariffCode" select="$CM_InvoiceLineAddInfo[*[local-name()='Key'] = 'TariffPrintLength']/*[local-name()='Value']/text()"/>
			<xsl:variable name="IMPTariff" select="$CM_InvoiceLineAddInfo[*[local-name()='Key'] = 'IMPTariff']/*[local-name()='Value']/text()"/>
			<xsl:variable name="PermitUQ" select="$CM_InvoiceLineAddInfo[*[local-name()='Key'] = 'PermitUQ']/*[local-name()='Value']/text()"/>
			<!--Group-->
			<xsl:if test="$InvoiceLineGroupingAddInfo != ''">
				<xsl:call-template name="GenerateGoodsShipment_GovernmentAgencyGoodsItemGroup">
					<xsl:with-param name="Group" select="$InvoiceLineGroupingAddInfo/*[local-name()='Value']/text()"/>
					<xsl:with-param name="PrintingTariffCode" select="$PrintingTariffCode"/>
					<xsl:with-param name="IMPTariff" select="$IMPTariff"/>
					<xsl:with-param name="PermitUQ" select="$PermitUQ"/>
				</xsl:call-template>
			</xsl:if>
			<!--Item-->
			<xsl:call-template name="GenerateGoodsShipment_GovernmentAgencyGoodsItem">
				<xsl:with-param name="CM_InvoiceLineAddInfo" select="$CM_InvoiceLineAddInfo"/>
				<xsl:with-param name="PrintingTariffCode" select="$PrintingTariffCode"/>
				<xsl:with-param name="IMPTariff" select="$IMPTariff"/>
				<xsl:with-param name="PermitUQ" select="$PermitUQ"/>
			</xsl:call-template>
		</xsl:for-each>
	</xsl:template>

	<xsl:template name="GenerateGoodsShipment_GovernmentAgencyGoodsItemGroup">
		<xsl:param name="Group"/>
		<xsl:param name="PrintingTariffCode"/>
		<xsl:param name="IMPTariff"/>
		<xsl:param name="PermitUQ"/>
		<xsl:element name="GovernmentAgencyGoodsItem">
			<xsl:element name="SequenceNumeric">
				<xsl:value-of select="userCSharp:GetSequenceNumber()"/>
			</xsl:element>
			<!--Commodity-->
			<xsl:element name="Commodity">
				<xsl:element name="Description">
					<xsl:value-of select="$Group"/>
				</xsl:element>
				<xsl:element name="tw_PrintingTariffCode">
					<xsl:value-of select="$PrintingTariffCode"/>
				</xsl:element>
				<xsl:element name="Classification">
					<xsl:element name="ID">
						<xsl:value-of select="normalize-space(*[local-name()='HarmonisedCode']/text())"/>
					</xsl:element>
					<xsl:element name="IdentificationTypeCode">
						<xsl:text>HS</xsl:text>
					</xsl:element>
				</xsl:element>
				<xsl:if test="$IMPTariff != ''">
					<xsl:element name="Classification">
						<xsl:element name="ID">
							<xsl:value-of select="$IMPTariff"/>
						</xsl:element>
						<xsl:element name="IdentificationTypeCode">
							<xsl:text>ZZZ</xsl:text>
						</xsl:element>
					</xsl:element>
				</xsl:if>
			</xsl:element>
			<!--GoodsMeasure-->
			<xsl:element name="GoodsMeasure">
				<xsl:element name="TariffQuantity">
					<xsl:value-of select="0"/>
				</xsl:element>
				<xsl:element name="tw_UnitCode">
					<xsl:value-of select="$PermitUQ"/>
				</xsl:element>
			</xsl:element>
		</xsl:element>
	</xsl:template>
	
	<xsl:template name="GenerateGoodsShipment_GovernmentAgencyGoodsItem">
		<xsl:param name="CM_InvoiceLineAddInfo"/>
		<xsl:param name="PrintingTariffCode"/>
		<xsl:param name="IMPTariff"/>
		<xsl:param name="PermitUQ"/>
		<xsl:variable name="InvoiceNumber" select="normalize-space(../../*[local-name()='InvoiceNumber']/text())" />
		<xsl:variable name="InvoiceDate" select="ScriptNS1:FormatXmlDateTime(normalize-space(../../*[local-name()='InvoiceDate']/text()), 'yyyy-MM-dd')" />
		<xsl:variable name="InvoiceCurrencyCode" select="normalize-space(../../*[local-name()='InvoiceCurrency']/*[local-name()='Code']/text())" />
		<xsl:variable name="InvoiceLineInvoiceQuantity" select="normalize-space(*[local-name()='InvoiceQuantity']/text())" />
		<xsl:variable name="InvoiceLineInvoiceQuantityUnitCode" select="normalize-space(*[local-name()='InvoiceQuantityUnit']/*[local-name()='Code']/text())" />
		<xsl:variable name="InvoiceLineManufacturerID" select="normalize-space(*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][*[local-name()='AddressType'] = 'Manufacturer']/*[local-name()='RegistrationNumberCollection']/*[local-name()='RegistrationNumber'][*[local-name()='Type']/*[local-name()='Code'] = 'FRI']/*[local-name()='Value']/text())" />
		<xsl:variable name="CriteriaCode" select="$CM_InvoiceLineAddInfo[*[local-name()='Key'] = 'OriginCriteria']/*[local-name()='Value']/text()"/>
		<xsl:variable name="PreferentialCriteria" select="$CM_InvoiceLineAddInfo[*[local-name()='Key'] = 'PTCriteria']/*[local-name()='Value']/text()"/>
		<xsl:variable name="ProducerCode" select="$CM_InvoiceLineAddInfo[*[local-name()='Key'] = 'ManufacturerRelationship']/*[local-name()='Value']/text()"/>
		<xsl:variable name="OtherCriteria" select="$CM_InvoiceLineAddInfo[*[local-name()='Key'] = 'PTCriteria2']/*[local-name()='Value']/text()"/>
		<xsl:variable name="CustomPermitUQ" select="$CM_InvoiceLineAddInfo[*[local-name()='Key'] = 'CustomPermitUQ']/*[local-name()='Value']/text()"/>
		<xsl:element name="GovernmentAgencyGoodsItem">
			<xsl:element name="SequenceNumeric">
				<xsl:value-of select="userCSharp:GetSequenceNumber()"/>
			</xsl:element>
			<xsl:if test="$CertificateType = '01' or $CertificateType = '07' or $CertificateType = '10'">
				<xsl:call-template name="GenerateNodesIfNotEmpty">
					<xsl:with-param name="NodeName" select="'tw_CriteriaCode'" />
					<xsl:with-param name="Value" select="$CriteriaCode" />
				</xsl:call-template>
			</xsl:if>
			<xsl:if test="$CertificateType = '09' or $CertificateType = '11' or $CertificateType = '13' or $CertificateType = '14' or $CertificateType = '15'">
				<xsl:call-template name="GenerateNodesIfNotEmpty">
					<xsl:with-param name="NodeName" select="'tw_PreferentialCriteria'" />
					<xsl:with-param name="Value" select="$PreferentialCriteria" />
				</xsl:call-template>
			</xsl:if>
			<xsl:if test="$CertificateType = '09' or $CertificateType = '11' or $CertificateType = '13' or $CertificateType = '14'">
				<xsl:call-template name="GenerateNodesIfNotEmpty">
					<xsl:with-param name="NodeName" select="'tw_ProducerCode'" />
					<xsl:with-param name="Value" select="$ProducerCode" />
				</xsl:call-template>
			</xsl:if>
			<xsl:if test="$CertificateType = '09' or $CertificateType = '11' or $CertificateType = '13' or $CertificateType = '14' or $CertificateType = '15'">
				<xsl:call-template name="GenerateNodesIfNotEmpty">
					<xsl:with-param name="NodeName" select="'tw_OtherCriteria'" />
					<xsl:with-param name="Value" select="$OtherCriteria" />
				</xsl:call-template>
			</xsl:if>
			<!--Commodity-->
			<xsl:element name="Commodity">
				<xsl:variable name="InvoiceLineModel" select="normalize-space(*[local-name()='Model']/text())" />
				<xsl:call-template name="GenerateNodesIfNotEmpty">
					<xsl:with-param name="NodeName" select="'CommercialCategorizationID'" />
					<xsl:with-param name="Value" select="$InvoiceLineModel" />
				</xsl:call-template>
				<xsl:element name="Description">
					<xsl:value-of select="userCSharp:NormalizeSpaceAndKeepLineBreaks(*[local-name()='DetailedDescription']/text())"/>
				</xsl:element>
				<xsl:element name="tw_PrintingTariffCode">
					<xsl:value-of select="$PrintingTariffCode"/>
				</xsl:element>
				<xsl:element name="Classification">
					<xsl:element name="ID">
						<xsl:value-of select="normalize-space(*[local-name()='HarmonisedCode']/text())"/>
					</xsl:element>
					<xsl:element name="IdentificationTypeCode">
						<xsl:text>HS</xsl:text>
					</xsl:element>
				</xsl:element>
				<xsl:if test="$IMPTariff != ''">
					<xsl:element name="Classification">
						<xsl:element name="ID">
							<xsl:value-of select="$IMPTariff"/>
						</xsl:element>
						<xsl:element name="IdentificationTypeCode">
							<xsl:text>ZZZ</xsl:text>
						</xsl:element>
					</xsl:element>
				</xsl:if>
				<xsl:element name="CommodityRelatedPackaging">
					<xsl:element name="tw_Specification">
						<xsl:value-of select="normalize-space(*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][*[local-name()='Key']/text() = 'InnerPackDescription']/*[local-name()='Value']/text())" />
					</xsl:element>
				</xsl:element>
				<xsl:element name="Constituent">
					<xsl:element name="ElementDescription">
						<xsl:value-of select="normalize-space(*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][*[local-name()='Key']/text() = 'Compositions']/*[local-name()='Value']/text())" />
					</xsl:element>
				</xsl:element>
				
				<xsl:if test="$CertificateType = '02' or $CertificateType = '09' or $CertificateType = '11' or $CertificateType = '12' or $CertificateType = '13' or $CertificateType = '14' or $CertificateType = '15'">
					<xsl:element name="Invoice">
						<xsl:call-template name="GenerateNodesIfNotEmpty">
							<xsl:with-param name="NodeName" select="'ID'" />
							<xsl:with-param name="Value" select="$InvoiceNumber" />
						</xsl:call-template>
						<xsl:call-template name="GenerateNodesIfNotEmpty">
							<xsl:with-param name="NodeName" select="'IssueDateTime'" />
							<xsl:with-param name="Value" select="$InvoiceDate" />
						</xsl:call-template>
					</xsl:element>
					<xsl:if test="$CertificateType = '15'">
						<xsl:element name="InvoiceLine">
							<xsl:variable name="InvoiceLinePrice" select="normalize-space(*[local-name()='LinePrice']/text())" />
							<xsl:call-template name="GenerateNodesIfNotEmpty">
								<xsl:with-param name="NodeName" select="'ItemChargeAmount'" />
								<xsl:with-param name="Value" select="$InvoiceLinePrice" />
							</xsl:call-template>

							<xsl:call-template name="GenerateNodesIfNotEmpty">
								<xsl:with-param name="NodeName" select="'tw_CurrencyTypeCode'" />
								<xsl:with-param name="Value" select="$InvoiceCurrencyCode" />
							</xsl:call-template>
						</xsl:element>
					</xsl:if>
				</xsl:if>
			</xsl:element>
			<!--Above is Commodity-->

			<!--GoodsMeasure-->
			<xsl:element name="GoodsMeasure">
				<xsl:element name="TariffQuantity">
					<xsl:value-of select="$InvoiceLineInvoiceQuantity"/>
				</xsl:element>
				<xsl:element name="tw_UnitCode">
					<xsl:value-of select="$PermitUQ"/>
				</xsl:element>
				<xsl:choose>
					<xsl:when test="$CertificateType = '15'">
						<xsl:call-template name="GenerateNodesIfNotEmpty">
							<xsl:with-param name="NodeName" select="'tw_CustomUnitCode'" />
							<xsl:with-param name="Value" select="$CustomPermitUQ" />
						</xsl:call-template>
					</xsl:when>
					<xsl:otherwise>
						<xsl:element name="tw_CustomUnitCode">
							<xsl:value-of select="$CustomPermitUQ"/>
						</xsl:element>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:element>
			<!--Above is GoodsMeasure-->

			<!--Manufacturer-->
			<xsl:if test="$InvoiceLineManufacturerID != ''">
				<xsl:element name="Manufacturer">
					<xsl:element name="ID">
						<xsl:value-of select="$InvoiceLineManufacturerID"/>
					</xsl:element>
				</xsl:element>
			</xsl:if>
			<!--Above is Manufacturer-->

			<!--Packaging-->
			<xsl:variable name="GovernmentAgencyGoodsItemPackagingMarksNumbers" select="userCSharp:NormalizeSpaceAndKeepLineBreaks($CM_InvoiceLineAddInfo[*[local-name()='Key'] = 'NX101ShippingMarks']/*[local-name()='Value']/text())"/>
			<xsl:if test="$CertificateType = '15' and $GovernmentAgencyGoodsItemPackagingMarksNumbers != ''">
				<xsl:element name="Packaging">
					<xsl:element name="MarksNumbers">
						<xsl:value-of select="$GovernmentAgencyGoodsItemPackagingMarksNumbers"/>
					</xsl:element>
				</xsl:element>
			</xsl:if>
			<!--Above is Packaging-->

			<!--tw_AdditionalDeclaration-->
			<xsl:if test="$CertificateType = '15'">
				<xsl:call-template name="Generate_tw_AdditionalDeclaration">
					<xsl:with-param name="InvoiceLine" select="." />
				</xsl:call-template>
			</xsl:if>
			<!--Above is tw_AdditionalDeclaration-->

		</xsl:element>
	</xsl:template>
	
	<xsl:template name="Generate_tw_AdditionalDeclaration">
		<xsl:param name="InvoiceLine" />

		<xsl:element name="tw_AdditionalDeclaration">
			<xsl:variable name ="EXPEntryNumber" select="$EXPEntryNumbers[*[local-name()='Number'] = $InvoiceLine/*[local-name()='EntryNumber']/text()]/*[local-name()='Number']/text()" />
			<xsl:call-template name="GenerateNodesIfNotEmpty">
				<xsl:with-param name="NodeName" select="'tw_ID'" />
				<xsl:with-param name="Value" select="$EXPEntryNumber" />
			</xsl:call-template>
			<xsl:call-template name="GenerateNodesIfNotEmpty">
				<xsl:with-param name="NodeName" select="'tw_SequenceNumeric'" />
				<xsl:with-param name="Value" select="normalize-space($InvoiceLine/*[local-name()='EntryLineNumber']/text())" />
			</xsl:call-template>
		</xsl:element>
	</xsl:template>

	<xsl:template name="Packaging">
		<xsl:if test="$CertificateType != '15'">
			<xsl:if test="$ShippingMarks != ''">
				<xsl:element name="Packaging">
					<xsl:element name="MarksNumbers">
						<xsl:value-of select="$ShippingMarks"/>
					</xsl:element>
				</xsl:element>
			</xsl:if>
		</xsl:if>
	</xsl:template>

	<xsl:template name="PreviousDocument">
		<xsl:if test="$PreviousPermitNumber != ''">
			<xsl:element name="PreviousDocument">
				<xsl:element name="ID">
					<xsl:value-of select="$PreviousPermitNumber"/>
				</xsl:element>
			</xsl:element>
		</xsl:if>
	</xsl:template>

	<xsl:template name="tw_Application">
		<xsl:element name="tw_Application">
			<xsl:element name="tw_AdhocCode">
				<xsl:choose>
					<xsl:when test="$IsSpecialApplication= 'Y'">
						<xsl:value-of select="'Y'"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="'N'"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:element>
			<xsl:call-template name="GenerateNodesIfNotEmpty">
				<xsl:with-param name="NodeName" select="'tw_AdhocProcessNumber'" />
				<xsl:with-param name="Value" select="$SpecialApplicationId" />
			</xsl:call-template>
			<xsl:call-template name="GenerateNodesIfNotEmpty">
				<xsl:with-param name="NodeName" select="'tw_CopyQuantity'" />
				<xsl:with-param name="Value" select="$CopyQuantity" />
			</xsl:call-template>

			<xsl:for-each select="$CM_InvoiceLines">
				<xsl:variable name="InvoiceLineGroupingAddInfo" select="*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][*[local-name()='Key']/text() = 'Group']" />
				<xsl:variable name="InvoiceLineDetailedDescription" select="normalize-space(*[local-name()='DetailedDescription']/text())" />
				<xsl:choose>
					<xsl:when test="$InvoiceLineGroupingAddInfo">
						<xsl:variable name="description" select="concat(concat($InvoiceLineGroupingAddInfo/*[local-name()='Value']/text(), '&#x000A;'), $InvoiceLineDetailedDescription)" />
						<xsl:if test="userCSharp:ShouldPrintDescriptionTooLong($description)">
							<xsl:element name="tw_DescriptionTooLong">
								<xsl:text>Y</xsl:text>
							</xsl:element>
						</xsl:if>
					</xsl:when>
					<xsl:otherwise>
						<xsl:if test="userCSharp:ShouldPrintDescriptionTooLong($InvoiceLineDetailedDescription)">
							<xsl:element name="tw_DescriptionTooLong">
								<xsl:text>Y</xsl:text>
							</xsl:element>
						</xsl:if>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:for-each>

			<xsl:call-template name="GenerateNodesIfNotEmpty">
				<xsl:with-param name="NodeName" select="'tw_ECFAPrintingDescription'" />
				<xsl:with-param name="Value" select="$ECFAPrintedRemarks" />
			</xsl:call-template>
			<xsl:call-template name="GenerateNodesIfNotEmpty">
				<xsl:with-param name="NodeName" select="'tw_EUSteelDeclarationCode'" />
				<xsl:with-param name="Value" select="$EUSteelProductNo" />
			</xsl:call-template>
			<xsl:call-template name="GenerateNodesIfNotEmpty">
				<xsl:with-param name="NodeName" select="'tw_EUSteelPhaseCode'" />
				<xsl:with-param name="Value" select="$EUSteelProductPhase" />
			</xsl:call-template>
			<xsl:call-template name="GenerateNodesIfNotEmpty">
				<xsl:with-param name="NodeName" select="'tw_ManufacturerPrintingCode'" />
				<xsl:with-param name="Value" select="$ManufacturerPrintingCode" />
			</xsl:call-template>
			<xsl:call-template name="GenerateNodesIfNotEmpty">
				<xsl:with-param name="NodeName" select="'tw_Observations'" />
				<xsl:with-param name="Value" select="$Observations" />
			</xsl:call-template>
			<xsl:call-template name="GenerateNodesIfNotEmpty">
				<xsl:with-param name="NodeName" select="'tw_OriginalCopyQuantity'" />
				<xsl:with-param name="Value" select="$OriginalQuantity" />
			</xsl:call-template>
			<xsl:if test="$ReturnPreviousCOO">
				<xsl:element name="tw_PreviousCORenderCode">
					<xsl:choose>
						<xsl:when test="$ReturnPreviousCOO= 'Y'">
							<xsl:value-of select="'Y'"/>
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="'N'"/>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:element>
			</xsl:if>
			<xsl:call-template name="GenerateNodesIfNotEmpty">
				<xsl:with-param name="NodeName" select="'tw_PrintingCode'" />
				<xsl:with-param name="Value" select="$PrintingCode" />
			</xsl:call-template>
			<xsl:if test="$IsTriangularTrade">
				<xsl:element name="tw_TriangularTradeCode">
					<xsl:choose>
						<xsl:when test="$IsTriangularTrade= 'Y'">
							<xsl:value-of select="'Y'"/>
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="'N'"/>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:element>
			</xsl:if>
			<xsl:element name="tw_TypeCode">
				<xsl:value-of select="$CertificateType"/>
			</xsl:element>

			<xsl:variable name="Agent" select="$Shipment/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][*[local-name()='AddressType']/text() = 'Declarant']"/>
			<xsl:if test="$Agent">
				<xsl:call-template name="GeneratePartyInFull">
					<xsl:with-param name="ElementName" select="'Agent'" />
					<xsl:with-param name="Party" select="$Agent" />
				</xsl:call-template>
			</xsl:if>
			<xsl:if test="$ProcessingUnit != ''">
				<xsl:element name="ContactOffice">
					<xsl:element name="ID">
						<xsl:value-of select="$ProcessingUnit"/>
					</xsl:element>
				</xsl:element>
			</xsl:if>
			<xsl:variable name="tw_Applicant" select="$CMOrganizationAddress[*[local-name()='AddressType'] = 'Applicant']" />
			<xsl:variable name="tw_ApplicantTranslated" select="$CMOrganizationAddress[*[local-name()='AddressType'] = 'ApplicantTranslatedDocumentaryAddress']" />
			<xsl:if test="$tw_Applicant">
				<xsl:call-template name="GeneratePartyInFull">
					<xsl:with-param name="ElementName" select="'tw_Applicant'" />
					<xsl:with-param name="Party" select="$tw_Applicant" />
					<xsl:with-param name="PartyTranslated" select="$tw_ApplicantTranslated" />
				</xsl:call-template>
			</xsl:if>
		</xsl:element>
	</xsl:template>

	<xsl:template name="tw_COImporter">
		<xsl:variable name="tw_COImporter" select="$CMOrganizationAddress[*[local-name()='AddressType'] = 'ImporterDocumentaryAddress']" />
		<xsl:variable name="tw_COImporterTranslated" select="$CMOrganizationAddress[*[local-name()='AddressType'] = 'ImporterTranslatedDocumentaryAddress']" />
		<xsl:if test="$tw_COImporter">
			<xsl:call-template name="GeneratePartyInFull">
				<xsl:with-param name="ElementName" select="'tw_COImporter'" />
				<xsl:with-param name="Party" select="$tw_COImporter" />
				<xsl:with-param name="PartyTranslated" select="$tw_COImporterTranslated" />
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template name="GeneratePartyInFull">
		<xsl:param name="ElementName" />
		<xsl:param name="Party"/>
		<xsl:param name="PartyTranslated"/>

		<xsl:variable name="Party_AddressOverride" select="$Party/*[local-name()='AddressOverride']/text() = 'true'"/>
		<xsl:variable name="Party_CountryCode" select="normalize-space($Party/*[local-name()='Country']/*[local-name()='Code']/text())"/>
		<xsl:variable name="Party_CountryName" select="normalize-space($Party/*[local-name()='Country']/*[local-name()='Name']/text())"/>
		<xsl:variable name="Party_LocalAddress">
			<xsl:choose>
				<xsl:when test="$Party_AddressOverride and $PartyTranslated">
					<xsl:copy-of select="$PartyTranslated/node()"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:copy-of select="$Party/*[local-name()='LocalAddressCollection']/*[local-name()='LocalAddress'][*[local-name()='Language']/*[local-name()='Code']/text()='ZH-TW']/node()"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="Party_ChineseName" select="substring(normalize-space(msxsl:node-set($Party_LocalAddress)/*[local-name()='CompanyName']/text()), 1, 70)"/>
		<xsl:variable name="Party_EnglishName" select="substring(normalize-space($Party/*[local-name()='CompanyName']/text()), 1, 80)"/>
		<xsl:variable name="GovRegNumType" select="normalize-space($Party/*[local-name()='GovRegNumType']/*[local-name()='Code']/text())"/>
		<xsl:variable name="GovRegNum" select="normalize-space($Party/*[local-name()='GovRegNum']/text())"/>
		<xsl:variable name="RegistrationNumberVAT" select="$Party/*[local-name()='RegistrationNumberCollection']/*[local-name()='RegistrationNumber'][*[local-name()='CountryOfIssue']/*[local-name()='Code']/text() = 'TW' and *[local-name()='Type']/*[local-name()='Code']/text() = 'VAT']/*[local-name()='Value']/text()"/>
		<xsl:variable name="RegistrationNumberPID" select="$Party/*[local-name()='RegistrationNumberCollection']/*[local-name()='RegistrationNumber'][*[local-name()='CountryOfIssue']/*[local-name()='Code']/text() = 'TW' and *[local-name()='Type']/*[local-name()='Code']/text() = 'PID']/*[local-name()='Value']/text()"/>
		<xsl:variable name="RegistrationNumberPAS" select="$Party/*[local-name()='RegistrationNumberCollection']/*[local-name()='RegistrationNumber'][*[local-name()='CountryOfIssue']/*[local-name()='Code']/text() = 'TW' and *[local-name()='Type']/*[local-name()='Code']/text() = 'PAS']/*[local-name()='Value']/text()"/>
		<xsl:variable name="RegNumType">
			<xsl:choose>
				<xsl:when test="($GovRegNumType = 'VAT' and $GovRegNum != '') or $RegistrationNumberVAT != ''">
					<xsl:value-of select="'VAT'"/>
				</xsl:when>
				<xsl:when test="($GovRegNumType = 'PID' and $GovRegNum != '') or $RegistrationNumberPID != ''">
					<xsl:value-of select="'PID'"/>
				</xsl:when>
				<xsl:when test="($GovRegNumType = 'PAS' and $GovRegNum != '') or $RegistrationNumberPAS != ''">
					<xsl:value-of select="'PAS'"/>
				</xsl:when>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="RegNum">
			<xsl:choose>
				<xsl:when test="$GovRegNumType = 'VAT' and $GovRegNum != ''">
					<xsl:value-of select="$GovRegNum"/>
				</xsl:when>
				<xsl:when test="$RegistrationNumberVAT != ''">
					<xsl:value-of select="$RegistrationNumberVAT"/>
				</xsl:when>
				<xsl:when test="$GovRegNumType = 'PID' and $GovRegNum != ''">
					<xsl:value-of select="$GovRegNum"/>
				</xsl:when>
				<xsl:when test="$RegistrationNumberPID != ''">
					<xsl:value-of select="$RegistrationNumberPID"/>
				</xsl:when>
				<xsl:when test="$GovRegNumType = 'PAS' and $GovRegNum != ''">
					<xsl:value-of select="$GovRegNum"/>
				</xsl:when>
				<xsl:when test="$RegistrationNumberPAS != ''">
					<xsl:value-of select="$RegistrationNumberPAS"/>
				</xsl:when>
			</xsl:choose>
		</xsl:variable>

		<xsl:variable name="Party_Phone" select="normalize-space($Party/*[local-name()='Phone']/text())"/>
		<xsl:variable name="Party_Fax" select="normalize-space($Party/*[local-name()='Fax']/text())"/>
		<xsl:variable name="Party_Email" select="substring(normalize-space($Party/*[local-name()='Email']/text()), 1, 60)"/>

		<xsl:element name="{$ElementName}">

			<xsl:if test="$ElementName = 'tw_Applicant' or ($ElementName = 'tw_COImporter'  and $CertificateType = '15')">
				<xsl:element name="tw_ChineseName">
					<xsl:value-of select="$Party_ChineseName"/>
				</xsl:element>
			</xsl:if>

			<xsl:choose>
				<xsl:when test="$ElementName = 'tw_Applicant' or $ElementName = 'tw_COImporter'">
					<xsl:element name="tw_ID">
						<xsl:value-of select="$RegNum"/>
					</xsl:element>
				</xsl:when>
				<xsl:otherwise>
					<xsl:element name="ID">
						<xsl:value-of select="$RegNum"/>
					</xsl:element>
				</xsl:otherwise>
			</xsl:choose>

			<xsl:if test="$ElementName = 'Exporter' and $CertificateType != '15'">
				<xsl:element name="Name">
					<xsl:value-of select="$Party_EnglishName"/>
				</xsl:element>
			</xsl:if>

			<xsl:if test="$ElementName = 'tw_COImporter' and $CertificateType != '15'">
				<xsl:element name="tw_Name">
					<xsl:value-of select="$Party_EnglishName"/>
				</xsl:element>
			</xsl:if>

			<xsl:if test="$ElementName = 'Agent'">
				<xsl:element name="Name">
					<xsl:value-of select="$Party_ChineseName"/>
				</xsl:element>
			</xsl:if>

			<xsl:if test="$ElementName = 'Exporter' and $CertificateType = '15'">
				<xsl:element name="tw_ChineseName">
					<xsl:value-of select="$Party_ChineseName"/>
				</xsl:element>
			</xsl:if>

			<xsl:if test="$ElementName = 'Exporter' or $ElementName = 'Agent' or $ElementName = 'tw_Applicant'">
				<xsl:element name="tw_TypeCode">
					<xsl:choose>
						<xsl:when test="$RegNumType = 'VAT'">
							<xsl:text>58</xsl:text>
						</xsl:when>
						<xsl:when test="$RegNumType = 'PID'">
							<xsl:text>174</xsl:text>
						</xsl:when>
						<xsl:when test="$RegNumType = 'PAS'">
							<xsl:text>53</xsl:text>
						</xsl:when>
					</xsl:choose>
				</xsl:element>
			</xsl:if>

			<xsl:if test="$ElementName = 'tw_Applicant'">
				<tw_UndertakeCode>Y</tw_UndertakeCode>
			</xsl:if>

			<xsl:element name="Address">
				<xsl:if test="($ElementName = 'Exporter' or $ElementName = 'tw_COImporter') and $CertificateType != '15'">
					<xsl:call-template name="GenerateEnglishAddressLine">
						<xsl:with-param name="Party" select="$Party" />
						<xsl:with-param name="CountryName" select="$Party_CountryName" />
						<xsl:with-param name="CountryCode" select="$Party_CountryCode" />
						<xsl:with-param name="AddressOverride" select="$Party_AddressOverride" />
					</xsl:call-template>
				</xsl:if>
				<xsl:if test="($ElementName != 'Exporter' and $ElementName != 'tw_COImporter') or $CertificateType = '15'">
					<xsl:call-template name="GenerateChineseAddressLine">
						<xsl:with-param name="ElementName" select="$ElementName" />
						<xsl:with-param name="Party_LocalAddress" select="$Party_LocalAddress" />
						<xsl:with-param name="CountryName" select="$Party_CountryName" />
						<xsl:with-param name="CountryCode" select="$Party_CountryCode" />
						<xsl:with-param name="AddressOverride" select="$Party_AddressOverride" />
					</xsl:call-template>
				</xsl:if>
			</xsl:element>

			<xsl:if test="$Party_Phone != ''">
				<xsl:element name="Communication">
					<xsl:element name="ID">
						<xsl:value-of select="$Party_Phone"/>
					</xsl:element>
					<xsl:element name="TypeID">
						<xsl:text>TE</xsl:text>
					</xsl:element>
				</xsl:element>
			</xsl:if>
			<xsl:if test="$Party_Email != ''">
				<xsl:element name="Communication">
					<xsl:element name="ID">
						<xsl:value-of select="$Party_Email"/>
					</xsl:element>
					<xsl:element name="TypeID">
						<xsl:text>MA</xsl:text>
					</xsl:element>
				</xsl:element>
			</xsl:if>
			<xsl:if test="$Party_Fax != ''">
				<xsl:element name="Communication">
					<xsl:element name="ID">
						<xsl:value-of select="$Party_Fax"/>
					</xsl:element>
					<xsl:element name="TypeID">
						<xsl:text>FX</xsl:text>
					</xsl:element>
				</xsl:element>
			</xsl:if>
		</xsl:element>
	</xsl:template>

	<xsl:template name="GenerateManufacturer">
		<xsl:param name="Party"/>
		<xsl:param name="PartyTranslated"/>

		<xsl:variable name="Party_AddressOverride" select="$Party/s0:AddressOverride/text() = 'true'"/>
		<xsl:variable name="Party_CountryCode" select="normalize-space($Party/s0:Country/s0:Code/text())"/>
		<xsl:variable name="Party_CountryName" select="normalize-space($Party/s0:Country/s0:Name/text())"/>
		<xsl:variable name="Party_LocalAddress">
			<xsl:choose>
				<xsl:when test="$Party_AddressOverride">
					<xsl:copy-of select="$PartyTranslated/node()"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:copy-of select="$Party/s0:LocalAddressCollection/s0:LocalAddress[s0:Language/s0:Code/text()='ZH-TW']/node()"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="Party_ChineseName" select="normalize-space(msxsl:node-set($Party_LocalAddress)/s0:CompanyName/text())"/>
		<xsl:variable name="Party_ChineseAddress">
			<xsl:variable name="Party_LocalAddress_PostCode" select="normalize-space(msxsl:node-set($Party_LocalAddress)/s0:Postcode/text())"/>
			<xsl:variable name="Party_LocalAddress_State" select="normalize-space(msxsl:node-set($Party_LocalAddress)/s0:State/text())"/>
			<xsl:variable name="Party_LocalAddress_City" select="normalize-space(msxsl:node-set($Party_LocalAddress)/s0:City/text())"/>
			<xsl:variable name="Party_LocalAddress_Address1" select="normalize-space(msxsl:node-set($Party_LocalAddress)/s0:Address1/text())"/>
			<xsl:variable name="Party_LocalAddress_Address2" select="normalize-space(msxsl:node-set($Party_LocalAddress)/s0:Address2/text())"/>
			<xsl:variable name="Party_LocalAddress_AdditionalAddressInformation" select="normalize-space(msxsl:node-set($Party_LocalAddress)/s0:AdditionalAddressInformation/text())"/>
			<xsl:variable name="Party_ChineseAddress_TW_Normal">
				<xsl:value-of select="normalize-space(concat($Party_LocalAddress_PostCode, $Party_LocalAddress_City, $Party_LocalAddress_Address1, $Party_LocalAddress_Address2))"/>
			</xsl:variable>
			<xsl:variable name="Party_ChineseAddress_TW_Override">
				<xsl:value-of select="normalize-space(concat($Party_LocalAddress_PostCode, $Party_LocalAddress_City, $Party_LocalAddress_Address1, $Party_LocalAddress_Address2, $Party_LocalAddress_AdditionalAddressInformation))"/>
			</xsl:variable>
			<xsl:variable name="Party_ChineseAddress_NONTW_Normal">
				<xsl:value-of select="normalize-space(concat($Party_LocalAddress_PostCode, $Party_LocalAddress_State, $Party_LocalAddress_City, $Party_LocalAddress_Address1, $Party_LocalAddress_Address2))"/>
			</xsl:variable>
			<xsl:variable name="Party_ChineseAddress_NONTW_Override">
				<xsl:value-of select="normalize-space(concat($Party_LocalAddress_PostCode, $Party_LocalAddress_State, $Party_LocalAddress_City, $Party_LocalAddress_Address1, $Party_LocalAddress_Address2, $Party_LocalAddress_AdditionalAddressInformation))"/>
			</xsl:variable>
			<xsl:choose>
				<xsl:when test="$Party_LocalAddress_Address1 = ''">
					<xsl:value-of select="''"/>
				</xsl:when>
				<xsl:when test="$Party_CountryCode = 'TW' and $Party_AddressOverride = false()">
					<xsl:value-of select="$Party_ChineseAddress_TW_Normal"/>
				</xsl:when>
				<xsl:when test="$Party_CountryCode = 'TW' and $Party_AddressOverride">
					<xsl:value-of select="$Party_ChineseAddress_TW_Override"/>
				</xsl:when>
				<xsl:when test="$Party_CountryCode != 'TW' and $Party_AddressOverride = false()">
					<xsl:value-of select="$Party_ChineseAddress_NONTW_Normal"/>
				</xsl:when>
				<xsl:when test="$Party_CountryCode != 'TW' and $Party_AddressOverride">
					<xsl:value-of select="$Party_ChineseAddress_NONTW_Override"/>
				</xsl:when>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="Party_EnglishName" select="normalize-space($Party/s0:CompanyName/text())"/>
		<xsl:variable name="Party_EnglishAddressIncludingCountryName">
			<xsl:variable name="Party_PostCode" select="normalize-space($Party/s0:Postcode/text())"/>
			<xsl:variable name="Party_State" select="normalize-space($Party/s0:State/text())"/>
			<xsl:variable name="Party_City" select="normalize-space($Party/s0:City/text())"/>
			<xsl:variable name="Party_Address1" select="normalize-space($Party/s0:Address1/text())"/>
			<xsl:variable name="Party_Address2" select="normalize-space($Party/s0:Address2/text())"/>
			<xsl:variable name="Party_AdditionalAddressInformation" select="normalize-space($Party/s0:AdditionalAddressInformation/text())"/>
			<xsl:variable name="EnglishAddress_TW_Normal" select="userCSharp:GetEnglishAddress($Party_Address1, $Party_Address2, $Party_City, $Party_PostCode, $Party_CountryName, '', '')"/>
			<xsl:variable name="EnglishAddress_TW_Override" select="userCSharp:GetEnglishAddress($Party_Address1, $Party_Address2, $Party_AdditionalAddressInformation, $Party_City, $Party_PostCode, $Party_CountryName, '')"/>
			<xsl:variable name="EnglishAddress_NONTW_Normal">
				<xsl:value-of select="userCSharp:GetEnglishAddress($Party_Address1, $Party_Address2, $Party_City, $Party_State, $Party_PostCode, $Party_CountryName, '')"/>
			</xsl:variable>
			<xsl:variable name="EnglishAddress_NONTW_Override">
				<xsl:value-of select="userCSharp:GetEnglishAddress($Party_Address1, $Party_Address2, $Party_AdditionalAddressInformation, $Party_City, $Party_State, $Party_PostCode, '')"/>
			</xsl:variable>

			<xsl:choose>
				<xsl:when test="$Party_Address1 = ''">
					<xsl:value-of select="''"/>
				</xsl:when>
				<xsl:when test="$Party_CountryCode = 'TW' and $Party_AddressOverride = false()">
					<xsl:value-of select="$EnglishAddress_TW_Normal"/>
				</xsl:when>
				<xsl:when test="$Party_CountryCode = 'TW' and $Party_AddressOverride">
					<xsl:value-of select="$EnglishAddress_TW_Override"/>
				</xsl:when>
				<xsl:when test="$Party_CountryCode != 'TW' and $Party_AddressOverride = false()">
					<xsl:value-of select="$EnglishAddress_NONTW_Normal"/>
				</xsl:when>
				<xsl:when test="$Party_CountryCode != 'TW' and $Party_AddressOverride">
					<xsl:value-of select="$EnglishAddress_NONTW_Override"/>
				</xsl:when>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="Party_EnglishAddress">
			<xsl:variable name="Party_PostCode" select="normalize-space($Party/s0:Postcode/text())"/>
			<xsl:variable name="Party_State" select="normalize-space($Party/s0:State/text())"/>
			<xsl:variable name="Party_City" select="normalize-space($Party/s0:City/text())"/>
			<xsl:variable name="Party_Address1" select="normalize-space($Party/s0:Address1/text())"/>
			<xsl:variable name="Party_Address2" select="normalize-space($Party/s0:Address2/text())"/>
			<xsl:variable name="Party_AdditionalAddressInformation" select="normalize-space($Party/s0:AdditionalAddressInformation/text())"/>
			<xsl:variable name="EnglishAddress_TW_Normal" select="userCSharp:GetEnglishAddress($Party_Address1, $Party_Address2, $Party_City, $Party_PostCode, '', '', '')"/>
			<xsl:variable name="EnglishAddress_TW_Override" select="userCSharp:GetEnglishAddress($Party_Address1, $Party_Address2, $Party_AdditionalAddressInformation, $Party_City, $Party_PostCode, '', '')"/>
			<xsl:variable name="EnglishAddress_NONTW_Normal">
				<xsl:value-of select="userCSharp:GetEnglishAddress($Party_Address1, $Party_Address2, $Party_City, $Party_State, $Party_PostCode, '', '')"/>
			</xsl:variable>
			<xsl:variable name="EnglishAddress_NONTW_Override">
				<xsl:value-of select="userCSharp:GetEnglishAddress($Party_Address1, $Party_Address2, $Party_AdditionalAddressInformation, $Party_City, $Party_State, $Party_PostCode, '')"/>
			</xsl:variable>

			<xsl:choose>
				<xsl:when test="$Party_Address1 = ''">
					<xsl:value-of select="''"/>
				</xsl:when>
				<xsl:when test="$Party_CountryCode = 'TW' and $Party_AddressOverride = false()">
					<xsl:value-of select="$EnglishAddress_TW_Normal"/>
				</xsl:when>
				<xsl:when test="$Party_CountryCode = 'TW' and $Party_AddressOverride">
					<xsl:value-of select="$EnglishAddress_TW_Override"/>
				</xsl:when>
				<xsl:when test="$Party_CountryCode != 'TW' and $Party_AddressOverride = false()">
					<xsl:value-of select="$EnglishAddress_NONTW_Normal"/>
				</xsl:when>
				<xsl:when test="$Party_CountryCode != 'TW' and $Party_AddressOverride">
					<xsl:value-of select="$EnglishAddress_NONTW_Override"/>
				</xsl:when>
			</xsl:choose>
		</xsl:variable>

		<xsl:variable name="Party_RegistrationNumberCollection" select="$Party/s0:RegistrationNumberCollection[s0:RegistrationNumber/s0:CountryOfIssue/s0:Code/text() = 'TW']"/>
		<xsl:variable name="GovRegNumType" select="normalize-space($Party/s0:GovRegNumType/s0:Code/text())"/>
		<xsl:variable name="GovRegNum" select="normalize-space($Party/s0:GovRegNum/text())"/>
		<xsl:variable name="RegistrationNumberFRI" select="$Party_RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text() = 'FRI']/s0:Value/text()"/>
		<xsl:variable name="RegistrationNumberVAT" select="$Party_RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text() = 'VAT']/s0:Value/text()"/>
		<xsl:variable name="RegistrationNumberPID" select="$Party_RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text() = 'PID']/s0:Value/text()"/>
		<xsl:variable name="RegistrationNumberPAS" select="$Party_RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text() = 'PAS']/s0:Value/text()"/>
		<xsl:variable name="RegNumType">
			<xsl:choose>
				<xsl:when test="$RegistrationNumberFRI != ''">
					<xsl:value-of select="'FRI'"/>
				</xsl:when>
				<xsl:when test="($GovRegNumType = 'VAT' and $GovRegNum != '') or $RegistrationNumberVAT != ''">
					<xsl:value-of select="'VAT'"/>
				</xsl:when>
				<xsl:when test="($GovRegNumType = 'PID' and $GovRegNum != '') or $RegistrationNumberPID != ''">
					<xsl:value-of select="'PID'"/>
				</xsl:when>
				<xsl:when test="($GovRegNumType = 'PAS' and $GovRegNum != '') or $RegistrationNumberPAS != ''">
					<xsl:value-of select="'PAS'"/>
				</xsl:when>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="RegNum">
			<xsl:choose>
				<xsl:when test="$RegistrationNumberFRI != ''">
					<xsl:value-of select="$RegistrationNumberFRI"/>
				</xsl:when>
				<xsl:when test="$GovRegNumType = 'VAT' and $GovRegNum != ''">
					<xsl:value-of select="$GovRegNum"/>
				</xsl:when>
				<xsl:when test="$RegistrationNumberVAT != ''">
					<xsl:value-of select="$RegistrationNumberVAT"/>
				</xsl:when>
				<xsl:when test="$GovRegNumType = 'PID' and $GovRegNum != ''">
					<xsl:value-of select="$GovRegNum"/>
				</xsl:when>
				<xsl:when test="$RegistrationNumberPID != ''">
					<xsl:value-of select="$RegistrationNumberPID"/>
				</xsl:when>
				<xsl:when test="$GovRegNumType = 'PAS' and $GovRegNum != ''">
					<xsl:value-of select="$GovRegNum"/>
				</xsl:when>
				<xsl:when test="$RegistrationNumberPAS != ''">
					<xsl:value-of select="$RegistrationNumberPAS"/>
				</xsl:when>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="Party_Phone" select="normalize-space($Party/s0:Phone/text())"/>
		<xsl:variable name="Party_Fax" select="normalize-space($Party/s0:Fax/text())"/>
		<xsl:variable name="Party_Email" select="normalize-space($Party/s0:Email/text())"/>
		<xsl:element name="Manufacturer">
			<xsl:element name="ID">
				<xsl:value-of select="$RegNum"/>
			</xsl:element>
			<xsl:if test="$Party_EnglishName !='' and not(userCSharp:ContainChinese($Party_EnglishName))">
				<xsl:element name="Name">
					<xsl:value-of select="substring($Party_EnglishName, 1, 80)"/>
				</xsl:element>
			</xsl:if>
			<xsl:element name="tw_ChineseName">
				<xsl:choose>
					<xsl:when test="$Party_ChineseName = '' and userCSharp:ContainChinese($Party_EnglishName)">
						<xsl:value-of select="substring($Party_EnglishName, 1, 70)"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="substring($Party_ChineseName, 1, 70)"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:element>
			<xsl:element name="tw_MainManufacturer">
				<xsl:value-of select="'Y'"/>
			</xsl:element>
			<xsl:element name="tw_TypeCode">
				<xsl:choose>
					<xsl:when test="$RegNumType = 'FRI'">
						<xsl:text>160</xsl:text>
					</xsl:when>
					<xsl:when test="$RegNumType = 'VAT'">
						<xsl:text>58</xsl:text>
					</xsl:when>
					<xsl:when test="$RegNumType = 'PID'">
						<xsl:text>174</xsl:text>
					</xsl:when>
					<xsl:when test="$RegNumType = 'PAS'">
						<xsl:text>53</xsl:text>
					</xsl:when>
				</xsl:choose>
			</xsl:element>
			<xsl:element name="Address">
				<xsl:if test="$Party_EnglishAddressIncludingCountryName != '' and normalize-space($Party_EnglishAddressIncludingCountryName) and not(userCSharp:ContainChinese($Party_EnglishAddressIncludingCountryName))">
					<xsl:element name="Line">
						<xsl:value-of select="substring($Party_EnglishAddressIncludingCountryName, 1, 120)"/>
					</xsl:element>
				</xsl:if>
				<xsl:choose>
					<xsl:when test="$Party_ChineseAddress = '' and userCSharp:ContainChinese($Party_EnglishAddress)">
						<xsl:element name="tw_ChineseLine">
							<xsl:value-of select="substring($Party_EnglishAddress, 1, 100)"/>
						</xsl:element>
					</xsl:when>
					<xsl:otherwise>
						<xsl:if test="$Party_ChineseAddress != ''">
							<xsl:element name="tw_ChineseLine">
								<xsl:value-of select="substring($Party_ChineseAddress, 1, 100)"/>
							</xsl:element>
						</xsl:if>
					</xsl:otherwise>
				</xsl:choose>

			</xsl:element>
			<xsl:if test="$Party_Phone != ''">
				<xsl:element name="Communication">
					<xsl:element name="ID">
						<xsl:value-of select="$Party_Phone"/>
					</xsl:element>
					<xsl:element name="TypeID">
						<xsl:text>TE</xsl:text>
					</xsl:element>
				</xsl:element>
			</xsl:if>
			<xsl:if test="$Party_Email != ''">
				<xsl:element name="Communication">
					<xsl:element name="ID">
						<xsl:value-of select="substring($Party_Email, 1, 60)"/>
					</xsl:element>
					<xsl:element name="TypeID">
						<xsl:text>MA</xsl:text>
					</xsl:element>
				</xsl:element>
			</xsl:if>
			<xsl:if test="$Party_Fax != ''">
				<xsl:element name="Communication">
					<xsl:element name="ID">
						<xsl:value-of select="$Party_Fax"/>
					</xsl:element>
					<xsl:element name="TypeID">
						<xsl:text>FX</xsl:text>
					</xsl:element>
				</xsl:element>
			</xsl:if>
		</xsl:element>
	</xsl:template>

	<xsl:template name="GenerateEnglishAddressLine">
		<xsl:param name="Party" />
		<xsl:param name="CountryName" />
		<xsl:param name="CountryCode" />
		<xsl:param name="AddressOverride" />
		<xsl:variable name="Party_EnglishAddress">
			<xsl:variable name="Party_PostCode" select="normalize-space($Party/*[local-name()='Postcode']/text())"/>
			<xsl:variable name="Party_State" select="normalize-space($Party/*[local-name()='State']/text())"/>
			<xsl:variable name="Party_City" select="normalize-space($Party/*[local-name()='City']/text())"/>
			<xsl:variable name="Party_Address1" select="normalize-space($Party/*[local-name()='Address1']/text())"/>
			<xsl:variable name="Party_Address2" select="normalize-space($Party/*[local-name()='Address2']/text())"/>
			<xsl:variable name="Party_AdditionalAddressInformation" select="normalize-space($Party/*[local-name()='AdditionalAddressInformation']/text())"/>
			<xsl:variable name="EnglishAddress_TW_Normal" select="userCSharp:GetEnglishAddress($Party_Address1, $Party_Address2, $Party_City, $Party_PostCode, $CountryName, '', '')"/>
			<xsl:variable name="EnglishAddress_TW_Override" select="userCSharp:GetEnglishAddress($Party_Address1, $Party_Address2, $Party_AdditionalAddressInformation, $Party_City, $Party_PostCode, $CountryName, '')"/>
			<xsl:variable name="EnglishAddress_NONTW_Normal">
				<xsl:value-of select="userCSharp:GetEnglishAddress($Party_Address1, $Party_Address2, $Party_City, $Party_State, $Party_PostCode, $CountryName, '')"/>
			</xsl:variable>
			<xsl:variable name="EnglishAddress_NONTW_Override">
				<xsl:value-of select="userCSharp:GetEnglishAddress($Party_Address1, $Party_Address2, $Party_AdditionalAddressInformation, $Party_City, $Party_State, $Party_PostCode, $CountryName)"/>
			</xsl:variable>

			<xsl:choose>
				<xsl:when test="$Party_Address1 = ''">
					<xsl:value-of select="''"/>
				</xsl:when>
				<xsl:when test="$CountryCode = 'TW' and $AddressOverride = false()">
					<xsl:value-of select="$EnglishAddress_TW_Normal"/>
				</xsl:when>
				<xsl:when test="$CountryCode = 'TW' and $AddressOverride">
					<xsl:value-of select="$EnglishAddress_TW_Override"/>
				</xsl:when>
				<xsl:when test="$CountryCode != 'TW' and $AddressOverride = false()">
					<xsl:value-of select="$EnglishAddress_NONTW_Normal"/>
				</xsl:when>
				<xsl:when test="$CountryCode != 'TW' and $AddressOverride">
					<xsl:value-of select="$EnglishAddress_NONTW_Override"/>
				</xsl:when>
			</xsl:choose>
		</xsl:variable>
		<xsl:call-template name="GenerateNodesIfNotEmpty">
			<xsl:with-param name="NodeName" select="'Line'" />
			<xsl:with-param name="Value" select="substring($Party_EnglishAddress, 1, 120)" />
		</xsl:call-template>
	</xsl:template>

	<xsl:template name="GenerateChineseAddressLine">
		<xsl:param name="ElementName" />
		<xsl:param name="Party_LocalAddress" />
		<xsl:param name="CountryName" />
		<xsl:param name="CountryCode" />
		<xsl:param name="AddressOverride" />

		<xsl:variable name="Party_ChineseAddress">
			<xsl:variable name="Party_LocalAddress_PostCode" select="normalize-space(msxsl:node-set($Party_LocalAddress)/*[local-name()='Postcode']/text())"/>
			<xsl:variable name="Party_LocalAddress_State" select="normalize-space(msxsl:node-set($Party_LocalAddress)/*[local-name()='State']/text())"/>
			<xsl:variable name="Party_LocalAddress_City" select="normalize-space(msxsl:node-set($Party_LocalAddress)/*[local-name()='City']/text())"/>
			<xsl:variable name="Party_LocalAddress_Address1" select="normalize-space(msxsl:node-set($Party_LocalAddress)/*[local-name()='Address1']/text())"/>
			<xsl:variable name="Party_LocalAddress_Address2" select="normalize-space(msxsl:node-set($Party_LocalAddress)/*[local-name()='Address2']/text())"/>
			<xsl:variable name="Party_LocalAddress_AdditionalAddressInformation" select="normalize-space(msxsl:node-set($Party_LocalAddress)/*[local-name()='AdditionalAddressInformation']/text())"/>
			<xsl:variable name="Party_ChineseAddress_TW_Normal">
				<xsl:choose>
					<xsl:when test="($ElementName = 'Exporter' or $ElementName = 'tw_COImporter') and $CertificateType = '15'">
						<xsl:value-of select="normalize-space(concat($Party_LocalAddress_City, $Party_LocalAddress_Address1, $Party_LocalAddress_Address2))"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="normalize-space(concat($Party_LocalAddress_PostCode, $Party_LocalAddress_City, $Party_LocalAddress_Address1, $Party_LocalAddress_Address2))"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:variable>
			<xsl:variable name="Party_ChineseAddress_TW_Override">
				<xsl:choose>
					<xsl:when test="($ElementName = 'Exporter' or $ElementName = 'tw_COImporter') and $CertificateType = '15'">
						<xsl:value-of select="normalize-space(concat($Party_LocalAddress_City, $Party_LocalAddress_Address1, $Party_LocalAddress_Address2, $Party_LocalAddress_AdditionalAddressInformation))"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="normalize-space(concat($Party_LocalAddress_PostCode, $Party_LocalAddress_City, $Party_LocalAddress_Address1, $Party_LocalAddress_Address2, $Party_LocalAddress_AdditionalAddressInformation))"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:variable>
			<xsl:variable name="Party_ChineseAddress_NONTW_Normal">
				<xsl:choose>
					<xsl:when test="($ElementName = 'Exporter' or $ElementName = 'tw_COImporter') and $CertificateType = '15'">
						<xsl:value-of select="normalize-space(concat($Party_LocalAddress_State, $Party_LocalAddress_City, $Party_LocalAddress_Address1, $Party_LocalAddress_Address2))"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="normalize-space(concat($Party_LocalAddress_PostCode, $CountryName, $Party_LocalAddress_State, $Party_LocalAddress_City, $Party_LocalAddress_Address1, $Party_LocalAddress_Address2))"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:variable>
			<xsl:variable name="Party_ChineseAddress_NONTW_Override">
				<xsl:choose>
					<xsl:when test="($ElementName = 'Exporter' or $ElementName = 'tw_COImporter') and $CertificateType = '15'">
						<xsl:value-of select="normalize-space(concat($Party_LocalAddress_State, $Party_LocalAddress_City, $Party_LocalAddress_Address1, $Party_LocalAddress_Address2, $Party_LocalAddress_AdditionalAddressInformation))"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="normalize-space(concat($Party_LocalAddress_PostCode, $CountryName, $Party_LocalAddress_State, $Party_LocalAddress_City, $Party_LocalAddress_Address1, $Party_LocalAddress_Address2, $Party_LocalAddress_AdditionalAddressInformation))"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:variable>
			<xsl:choose>
				<xsl:when test="$Party_LocalAddress_Address1 = ''">
					<xsl:value-of select="''"/>
				</xsl:when>
				<xsl:when test="$CountryCode = 'TW' and $AddressOverride = false()">
					<xsl:value-of select="$Party_ChineseAddress_TW_Normal"/>
				</xsl:when>
				<xsl:when test="$CountryCode = 'TW' and $AddressOverride">
					<xsl:value-of select="$Party_ChineseAddress_TW_Override"/>
				</xsl:when>
				<xsl:when test="$CountryCode != 'TW' and $AddressOverride = false()">
					<xsl:value-of select="$Party_ChineseAddress_NONTW_Normal"/>
				</xsl:when>
				<xsl:when test="$CountryCode != 'TW' and $AddressOverride">
					<xsl:value-of select="$Party_ChineseAddress_NONTW_Override"/>
				</xsl:when>
			</xsl:choose>
		</xsl:variable>
		<xsl:call-template name="GenerateNodesIfNotEmpty">
			<xsl:with-param name="NodeName" select="'tw_ChineseLine'" />
			<xsl:with-param name="Value" select="substring($Party_ChineseAddress, 1, 100)" />
		</xsl:call-template>
	</xsl:template>


	<xsl:template name="GenerateNodesIfNotEmpty">
		<xsl:param name="NodeName" />
		<xsl:param name="Value" />

		<xsl:if test="$Value != '' and string-length($Value) != 0">
			<xsl:choose>
				<xsl:when test="contains($NodeName, '/')">
					<xsl:element name="{substring-before($NodeName, '/')}">
						<xsl:call-template name="GenerateNodesIfNotEmpty">
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

	<msxsl:script language="C#" implements-prefix="userCSharp">
		<msxsl:using namespace="System.Collections.Generic" />
		<msxsl:assembly name="System.Core" />
		<msxsl:using namespace="System.Linq" />
		<![CDATA[
static readonly Regex cjkCharRegex = new Regex(@"\p{IsCJKUnifiedIdeographs}");
public bool ContainChinese(string text)
{
	foreach (var x in text)
	{
		if (cjkCharRegex.IsMatch(x.ToString())) return true;
	}

	return false;
}

public int SequenceNumber = 1;

public int GetSequenceNumber()
{
	return SequenceNumber++;
}

bool HasDescriptionTooLong;

public bool ShouldPrintDescriptionTooLong(string description)
{
	return !HasDescriptionTooLong && CheckDescriptionLength(description);
}

bool CheckDescriptionLength(string description)
{
  bool result = description.Length > 512;

	if (!HasDescriptionTooLong && result)
	{
		HasDescriptionTooLong = result;
	}

	return result;
}

public XPathNodeIterator DivideString(string input, int elementMaxLength, int elementCount)
{
	XmlDocument doc = new XmlDocument();
	doc.AppendChild(doc.CreateElement("root"));
	using (XmlWriter writer = doc.DocumentElement.CreateNavigator().AppendChild())
	{
		if (input.Length > 0)
		{
			int count = input.Length / elementMaxLength;
			if (count >= elementCount)
			{
				count = elementCount - 1;
			}

			for (int i = 0; i < count; i++)
			{
				writer.WriteElementString("item", input.Substring(i * elementMaxLength, elementMaxLength));
			}

			var last = input.Substring(count * elementMaxLength);
			if (last.Length > 0)
			{
				writer.WriteElementString("item", last);
			}
		}
	}

	return doc.DocumentElement.CreateNavigator().Select("item");
}

public string NormalizeSpaceAndKeepLineBreaks(string input)
{
	return string.IsNullOrEmpty(input) ? "" : input.Trim();
}

public string ThrowPartyReceiverIDNotFound(string destinationPartyTCA)
{
	throw new ArgumentException(string.Format(@"Could not found matching DestinationParty.ID:{0}", destinationPartyTCA));
}

public string GetEnglishAddress(string addressInfo1, string addressInfo2, string addressInfo3, string addressInfo4, string addressInfo5, string addressInfo6, string addressInfo7)
{
	return ConcatAddresses(addressInfo1, addressInfo2, addressInfo3, addressInfo4, addressInfo5, addressInfo6, addressInfo7);
}

public string ConcatAddresses(params string[] addressInfos)
{
	return string.Join(" ", addressInfos.Where(x => !string.IsNullOrEmpty(x)));
}

public string GetRemarks(string tradersRemarks, string outerPacks, string outerPacksPackageType)
{
	string result;
	var sayTotal = "";
	long number = 0;
	if (long.TryParse(outerPacks, out number) && number > 0)
	{
		string numberToString = ConvertNumberToString(number);
		if (numberToString != "")
		{
			sayTotal = string.Format("SAY TOTAL {0} ({1}) {2}{3} ONLY.", numberToString, number, outerPacksPackageType, number > 1 ? "S" : "");
		}
	}

	if (!string.IsNullOrEmpty(tradersRemarks))
	{
		result = string.Format("{0}\r\n{1}", tradersRemarks, sayTotal);
	}
	else
	{
		result = sayTotal;
	}
	return result;
}

string ConvertNumberToString(long number)
		{
			string result = "";
			if (number < primitiveNumbers.Length)
			{
				result = primitiveNumbers[number];
			}
			else if (number < 100)
			{
				result = number % 10 == 0 ? tens[number / 10] : string.Format(tensPattern, tens[number / 10], primitiveNumbers[number % 10]);
			}
			else if (number < 1000)
			{
				result = number % 100 == 0 ? GetHundredsPlace(number) : string.Format(hundredsPattern, GetHundredsPlace(number), ConvertNumberToString(number % 100));
			}
			else if (number < million)
			{
				string hundredsPlace = string.Format(thosandPlacePattern, ConvertNumberToString(number / 1000));
				result = number % 1000 > 0 ? string.Format(thousandsPattern, hundredsPlace, ConvertNumberToString(number % 1000)) : hundredsPlace;
			}
			else if (number < billion)
			{
				string millionsPlace = string.Format(millionPlacePattern, ConvertNumberToString(number / million));
				result = number % million > 0 ? string.Format(millionsPattern, millionsPlace, ConvertNumberToString(number % million)) : millionsPlace;
			}
			else if (number < trillion)
			{
				string billionsPlace = string.Format(billionPlacePattern, ConvertNumberToString(number / billion));
				result = number % billion > 0 ? string.Format(billionsPattern, billionsPlace, ConvertNumberToString(number % billion)) : billionsPlace;
			}
			else
			{
				throw new NotSupportedException(String.Format("The number {0} is over one trillion. This cannot be supported by the current version of the document engine.", number));
			}
			return result.ToUpper().Trim();
		}

		string GetHundredsPlace(long number)
		{
			return string.Format(hundredsPlacePattern, ConvertNumberToString(number / 100));
		}

		const long million = 1000000;
		const long billion = 1000000000;
		const long trillion = 1000000000000;

		string[] primitiveNumbers = new string[] {
				"zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine",
				"ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };

		string[] tens = new string[] { "", "", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };
		string hundredsPlacePattern = "{0} hundred";

		string tensPattern = "{0} {1}";
		string hundredsPattern = "{0} and {1}";
		string thosandPlacePattern = "{0} thousand";
		string thousandsPattern = "{0}, {1}";
		string millionPlacePattern = "{0} million";
		string millionsPattern = "{0}, {1}";
		string billionPlacePattern = "{0} billion";
		string billionsPattern = "{0}, {1}";

]]>
	</msxsl:script>

</xsl:stylesheet>

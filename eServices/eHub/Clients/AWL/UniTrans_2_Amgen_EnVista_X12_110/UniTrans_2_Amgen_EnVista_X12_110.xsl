<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var" exclude-result-prefixes="msxsl var s0 ScriptNS0 ScriptNS1 ScriptNS2 ScriptNS3" version="1.0"
                xmlns:ns0="http://cargowise.com/ehub/clients/awl/2022/05"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
                xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1"
                xmlns:ScriptNS2="http://schemas.microsoft.com/BizTalk/2003/ScriptNS2"
                xmlns:ScriptNS3="http://schemas.microsoft.com/BizTalk/2003/ScriptNS3">
	<xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />

	<xsl:variable name="Sender"    select="'AWLORDORD'"/>
	<xsl:variable name="Recipient" select="'AWLORDORD_ENV'"/>
	<xsl:variable name="TS_Name"   select="'EnVista 110 - Send A/R Invoices'"/>

	<xsl:variable name="Shipment" select="(//*[local-name()='Shipment' or local-name()='SubShipment']
                                                              [contains(./*[local-name()='DataContext']/*[local-name()='DataSourceCollection'], 'ForwardingShipment') and 
                                                           not(contains(./*[local-name()='DataContext']/*[local-name()='DataSourceCollection'], 'ForwardingConsol'))] | 
                                  //*[local-name()='Shipment'][contains(./*[local-name()='DataContext']/*[local-name()='DataSourceCollection'], 'CustomsDeclaration') and 
                                                           not(contains(./*[local-name()='DataContext']/*[local-name()='DataSourceCollection'], 'ForwardingConsol')) and 
                                                           not(contains(./*[local-name()='DataContext']/*[local-name()='DataSourceCollection'], 'ForwardingShipment'))] )"/>
	<xsl:variable name="OrgCode" select="/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalTransaction']/*[local-name()='TransactionInfo']
                /*[local-name()='OrganizationAddress']/*[local-name()='OrganizationCode']" />

	<xsl:variable name="SCAC" select="ScriptNS0:GetRecipientCodeUnkeyed($Sender, $Recipient, $TS_Name, 'Defaults', 'SCAC')"/>
	<xsl:variable name="B3A01" select="ScriptNS0:GetRecipientCodeUnkeyed($Sender, $Recipient, $TS_Name, 'Defaults', 'Transaction Type - B3A01')"/>
	<xsl:variable name="FRT" select="ScriptNS0:GetRecipientCodeUnkeyed($Sender, $Recipient, $TS_Name, 'Defaults', 'Freight Charge Code')"/>
	<xsl:variable name="L103" select="ScriptNS0:GetRecipientCodeUnkeyed($Sender, $Recipient, $TS_Name , 'Defaults' , 'Rate Basis - L103')"/>

	<xsl:variable name="HWB">
		<xsl:call-template name="ApplyCharacterSet">
			<xsl:with-param name="Value" select="//*[contains(local-name(), 'BillNumber')][../*[contains(local-name(), 'BillType')][*[local-name()='Code'] = 'HWB']  and . != '']" />
		</xsl:call-template>
	</xsl:variable>

	<xsl:template match="/">
		<xsl:apply-templates select="/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalTransaction']/*[local-name()='TransactionInfo']" />
	</xsl:template>

	<xsl:template match="/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalTransaction']/*[local-name()='TransactionInfo']">
		<xsl:variable name="OverrideEDIHeader" select="ScriptNS2:SetContextProperty('OverrideEDIHeader', 'http://schemas.microsoft.com/BizTalk/2006/edi-properties', 'true')"/>
		<xsl:variable name="ISA07" select="ScriptNS2:SetContextProperty('ISA07', 'http://schemas.microsoft.com/BizTalk/2006/edi-properties', 
                ScriptNS0:GetRecipientCode($Sender, $Recipient, $TS_Name, 'ISA/GS Receiver ID', 'ISA Receiver Qualifier', $OrgCode))" />
		<xsl:variable name="ISA08" select="ScriptNS2:SetContextProperty('ISA08', 'http://schemas.microsoft.com/BizTalk/2006/edi-properties', 
                ScriptNS0:GetRecipientCode($Sender, $Recipient, $TS_Name, 'ISA/GS Receiver ID', 'ISA Receiver ID', $OrgCode))" />
		<xsl:variable name="GS03" select="ScriptNS2:SetContextProperty('GS03', 'http://schemas.microsoft.com/BizTalk/2006/edi-properties', 
                ScriptNS0:GetRecipientCode($Sender, $Recipient, $TS_Name, 'ISA/GS Receiver ID', 'GS Receiver ID', $OrgCode))" />
		<ns0:X12_00401_110>

			<xsl:element name="ST">

				<xsl:element name="ST01">
					<xsl:text>110</xsl:text>
				</xsl:element>

				<xsl:element name="ST02">
					<xsl:text>0000</xsl:text>
				</xsl:element>

			</xsl:element>

			<xsl:variable name="Total">
				<xsl:variable name="OSTotal" select="./*[local-name()='OSTotal']"/>
				<xsl:choose>

					<xsl:when test="$OSTotal = number($OSTotal)">
						<xsl:value-of select="format-number($OSTotal * 100, '0')"/>
					</xsl:when>

					<xsl:otherwise>
						<xsl:value-of select="'0'"/>
					</xsl:otherwise>

				</xsl:choose>
			</xsl:variable>
			<xsl:element name="ns0:B3">

				<xsl:variable name="InvoiceNumber">
					<xsl:call-template name="ApplyCharacterSet">
						<xsl:with-param name="Value" select="translate(*[local-name()='JobInvoiceNumber'], '/', '')" />
					</xsl:call-template>
				</xsl:variable>
				<xsl:element name="B302">
					<xsl:value-of select="$InvoiceNumber"/>
				</xsl:element>

				<xsl:element name="B303">
					<xsl:choose>

						<xsl:when test="$HWB != ''">
							<xsl:value-of select="$HWB"/>
						</xsl:when>

						<xsl:otherwise>
							<xsl:value-of select="$InvoiceNumber"/>
						</xsl:otherwise>

					</xsl:choose>
				</xsl:element>


				<xsl:variable name="IncoTerm" select="$Shipment/*[local-name()='ShipmentIncoTerm']/*[local-name()='Code']" />
				<xsl:variable name="PaymentMethod" select="ScriptNS0:GetRecipientCode('eHub', 'eHub', 'Common Code Mappings', 'X12 B304 - Payment Method', 'Payment Method - B304', $IncoTerm)" />
				<xsl:element name="B304">
					<xsl:value-of select="$PaymentMethod"/>
				</xsl:element>

				<xsl:element name="B306">
					<xsl:value-of select="ScriptNS1:ConvertXmlDateString(./*[local-name()='TransactionDate'], 'yyyyMMdd')"/>
				</xsl:element>

				<xsl:element name="B307">
					<xsl:value-of select="$Total"/>
				</xsl:element>

				<xsl:element name="B311">
					<xsl:value-of select="$SCAC"/>
				</xsl:element>

				<xsl:call-template name="MapValueIfNotEmpty">
					<xsl:with-param name="NodeName" select="'B314'" />
					<xsl:with-param name="Value"    select="$IncoTerm" />
				</xsl:call-template>

			</xsl:element>

			<xsl:if test="$B3A01 != ''">
				<xsl:element name="ns0:B3A">

					<xsl:element name="B3A01">
						<xsl:value-of select="$B3A01"/>
					</xsl:element>

					<xsl:element name="B3A02">
						<xsl:value-of select="count($Shipment)"/>
					</xsl:element>

				</xsl:element>
			</xsl:if>

			<xsl:call-template name="MapValueIfNotEmpty">
				<xsl:with-param name="NodeName" select="'ns0:C3/C301'" />
				<xsl:with-param name="Value" select="./*[local-name()='OSCurrency']/*[local-name()='Code']" />
			</xsl:call-template>

			<xsl:variable name="PickupDate">
				<xsl:variable name="PickupCartageCompleted" select="$Shipment/*[local-name()='LocalProcessing']/*[local-name()='PickupCartageCompleted']" />
				<xsl:choose>

					<xsl:when test="$PickupCartageCompleted != ''">
						<xsl:value-of select="$PickupCartageCompleted"/>
					</xsl:when>

					<xsl:otherwise>
						<xsl:value-of select="$Shipment/*[local-name()='DateCollection']/*[local-name()='Date'][./*[local-name()='Type'] = 'Departure']/*[local-name()='Value']"/>
					</xsl:otherwise>

				</xsl:choose>
			</xsl:variable>
			<xsl:if test="$PickupDate != ''">
				<xsl:element name="ns0:P1_2">

					<xsl:element name="P101">
						<xsl:text>SD</xsl:text>
					</xsl:element>

					<xsl:element name="P102">
						<xsl:value-of select="ScriptNS1:FormatXmlDateTime($PickupDate, 'yyyyMMdd')"/>
					</xsl:element>

					<xsl:element name="P103">
						<xsl:text>11</xsl:text>
					</xsl:element>

				</xsl:element>
			</xsl:if>

			<xsl:variable name="GoodsDesc">
				<xsl:call-template name="ApplyCharacterSet">
					<xsl:with-param name="Value" select="normalize-space($Shipment/*[local-name()='GoodsDescription'])" />
				</xsl:call-template>
			</xsl:variable>

			<xsl:call-template name="MapN9">
				<xsl:with-param name="NodeName" select="'ns0:N9_3'" />
				<xsl:with-param name="N901" select="'4B'" />
				<xsl:with-param name="N902" select="$Shipment/*[local-name()='PortOfOrigin']/*[local-name()='Code']" />
			</xsl:call-template>

			<xsl:call-template name="MapN9">
				<xsl:with-param name="NodeName" select="'ns0:N9_3'" />
				<xsl:with-param name="N901" select="'4C'" />
				<xsl:with-param name="N902" select="$Shipment/*[local-name()='PortOfDestination']/*[local-name()='Code']" />
			</xsl:call-template>

			<xsl:call-template name="MapN9">
				<xsl:with-param name="NodeName" select="'ns0:N9_3'" />
				<xsl:with-param name="N901" select="'ZZ'" />
				<xsl:with-param name="N902" select="$Shipment/*[local-name()='CustomizedFieldCollection']
                        /*[local-name()='CustomizedField'][./*[local-name()='Key'] = 'AMGEN WBS Reference']/*[local-name()='Value']" />
			</xsl:call-template>

			<xsl:call-template name="MapN9">
				<xsl:with-param name="NodeName" select="'ns0:N9_3'" />
				<xsl:with-param name="N901" select="'IT'" />
				<xsl:with-param name="N902">
					<xsl:variable name="GTN">
						<xsl:call-template name="ApplyCharacterSet">
							<xsl:with-param name="Value" select="./*[local-name()='OrganizationAddress']/*[local-name()='RegistrationNumberCollection']/*[local-name()='RegistrationNumber']
                      [*[local-name()='CountryOfIssue']/*[local-name()='Code'] = 'US' and *[local-name()='Type']/*[local-name()='Code'] = 'GTN']/*[local-name()='Value']" />
						</xsl:call-template>
					</xsl:variable>
					<xsl:choose>

						<xsl:when test="$GTN != ''">
							<xsl:value-of select="$GTN"/>
						</xsl:when>

						<xsl:otherwise>
							<xsl:value-of select="$OrgCode"/>
						</xsl:otherwise>

					</xsl:choose>
				</xsl:with-param>
			</xsl:call-template>

			<xsl:call-template name="MapN9">
				<xsl:with-param name="NodeName" select="'ns0:N9_3'" />
				<xsl:with-param name="N901" select="'Z5'" />
				<xsl:with-param name="N902" select="substring($GoodsDesc, 1, 30)" />
			</xsl:call-template>

			<xsl:for-each select="$Shipment/*[local-name()='LocalProcessing']/*[local-name()='OrderNumberCollection']/*[local-name()='OrderNumber']">
				<xsl:if test="position() &lt;= 5">
					<xsl:call-template name="MapN9">
						<xsl:with-param name="NodeName" select="'ns0:N9_3'" />
						<xsl:with-param name="N901" select="'PO'" />
						<xsl:with-param name="N902" select="./*[local-name()='OrderReference']" />
					</xsl:call-template>
				</xsl:if>
			</xsl:for-each>

			<xsl:variable name="R103">
				<xsl:call-template name="GetIATA">
					<xsl:with-param name="Port" select="$Shipment/*[local-name()='PortOfOrigin']/*[local-name()='Code']" />
				</xsl:call-template>
			</xsl:variable>
			<xsl:variable name="R105">
				<xsl:call-template name="GetIATA">
					<xsl:with-param name="Port" select="$Shipment/*[local-name()='PortOfDestination']/*[local-name()='Code']" />
				</xsl:call-template>
			</xsl:variable>
			<xsl:if test="$R103 != '' and $R105 != ''">
				<xsl:element name="ns0:R1_2">

					<xsl:element name="R103">
						<xsl:value-of select="$R103"/>
					</xsl:element>

					<xsl:element name="R105">
						<xsl:value-of select="$R105"/>
					</xsl:element>

				</xsl:element>
			</xsl:if>

			<xsl:call-template name="MapN1">
				<xsl:with-param name="Org" select="./*[local-name()='OrganizationAddress']" />
				<xsl:with-param name="OrgType" select="'BT'" />
			</xsl:call-template>

			<xsl:call-template name="MapN1">
				<xsl:with-param name="Org" select="$Shipment/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress']
                        [*[local-name()='AddressType'] = 'ConsignorDocumentaryAddress'  or *[local-name()='AddressType'] = 'SupplierDocumentaryAddress'][1]" />
				<xsl:with-param name="OrgType" select="'SH'" />
			</xsl:call-template>

			<xsl:call-template name="MapN1">
				<xsl:with-param name="Org" select="$Shipment/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress']
                        [*[local-name()='AddressType'] = 'ConsigneeDocumentaryAddress' or *[local-name()='AddressType'] = 'ImporterDocumentaryAddress'][1]" />
				<xsl:with-param name="OrgType" select="'CN'" />
			</xsl:call-template>



			<xsl:call-template name="MapValueIfNotEmpty">
				<xsl:with-param name="NodeName" select="'ns0:SL1_2/SL101'" />
				<xsl:with-param name="Value" select="substring(ScriptNS0:GetRecipientCode($Sender, $Recipient, $TS_Name, 'Service Level', 'Service Level - SL101', 
                        $Shipment/*[local-name()='ServiceLevel']/*[local-name()='Code']), 1, 2)" />
			</xsl:call-template>

			<xsl:call-template name="MapValueIfNotEmpty">
				<xsl:with-param name="NodeName" select="'ns0:POD_2/POD01'" />
				<xsl:with-param name="Value">

					<xsl:variable name="DeliveryDate">
						<xsl:variable name="DeliveryCartageCompleted" select="$Shipment/*[local-name()='LocalProcessing']/*[local-name()='DeliveryCartageCompleted']" />
						<xsl:choose>

							<xsl:when test="$DeliveryCartageCompleted != ''">
								<xsl:value-of select="$DeliveryCartageCompleted"/>
							</xsl:when>

							<xsl:otherwise>
								<xsl:value-of select="$Shipment/*[local-name()='DateCollection']/*[local-name()='Date'][./*[local-name()='Type'] = 'Arrival']/*[local-name()='Value']"/>
							</xsl:otherwise>

						</xsl:choose>
					</xsl:variable>

					<xsl:value-of select="ScriptNS1:FormatXmlDateTime($DeliveryDate, 'yyyyMMdd')"/>

				</xsl:with-param>
			</xsl:call-template>

			<xsl:variable name="Packs">
				<xsl:variable name="OuterPacks" select="$Shipment/*[local-name()='OuterPacks']"/>
				<xsl:variable name="TotalNoOfPacks" select="$Shipment/*[local-name()='TotalNoOfPacks']"/>
				<xsl:choose>

					<xsl:when test="$OuterPacks > 0">
						<xsl:value-of select="format-number($OuterPacks, '0')"/>
					</xsl:when>

					<xsl:when test="$TotalNoOfPacks > 0">
						<xsl:value-of select="format-number($TotalNoOfPacks, '0')"/>
					</xsl:when>

					<xsl:otherwise>
						<xsl:value-of select="'0'"/>
					</xsl:otherwise>

				</xsl:choose>
			</xsl:variable>

			<xsl:variable name="TotalWeightUnit" select="$Shipment/*[local-name()='TotalWeightUnit']/*[local-name()='Code']" />

			<xsl:variable name="ChargeableWeight">
				<xsl:variable name="ActualChargeableUnit">
					<xsl:variable name="TotalVolumeUnit" select="$Shipment/*[local-name()='TotalVolumeUnit']/*[local-name()='Code']" />
					<xsl:variable name="UseImperialUnit">
						<xsl:if test="($TotalWeightUnit = 'LB' or $TotalWeightUnit = 'OZ' or $TotalWeightUnit = 'LT' or $TotalWeightUnit = 'OT') and 
                          ($TotalVolumeUnit = 'CF' or $TotalVolumeUnit = 'CI' or $TotalVolumeUnit = 'CY')">
							<xsl:value-of select="true()"/>
						</xsl:if>
					</xsl:variable>
					<xsl:choose>

						<xsl:when test="$UseImperialUnit != ''">
							<xsl:text>LB</xsl:text>
						</xsl:when>

						<xsl:otherwise>
							<xsl:text>KG</xsl:text>
						</xsl:otherwise>

					</xsl:choose>
				</xsl:variable>
				<xsl:variable name="Value" select="ScriptNS3:Convert($Shipment/*[local-name()='ActualChargeable'], $ActualChargeableUnit, 'KG')" />
				<xsl:choose>

					<xsl:when test="$Value > 0">
						<xsl:value-of select="format-number($Value, '.##')"/>
					</xsl:when>

					<xsl:otherwise>
						<xsl:text>0</xsl:text>
					</xsl:otherwise>

				</xsl:choose>
			</xsl:variable>

			<xsl:element name="ns0:LXLoop1">

				<xsl:element name ="ns0:LX">
					<xsl:element name="LX01">
						<xsl:text>1</xsl:text>
					</xsl:element>
				</xsl:element>

				<xsl:element name="ns0:L5Loop1">

					<xsl:element name="ns0:L5">

						<xsl:element name="L501">
							<xsl:text>1</xsl:text>
						</xsl:element>

						<xsl:call-template name="MapValueIfNotEmpty">
							<xsl:with-param name="NodeName" select="'L502'" />
							<xsl:with-param name="Value" select="$GoodsDesc" />
						</xsl:call-template>

					</xsl:element>

					<xsl:element name ="ns0:L0">

						<xsl:element name="L001">
							<xsl:text>1</xsl:text>
						</xsl:element>

						<xsl:element name="L004">
							<xsl:value-of select="$ChargeableWeight"/>
						</xsl:element>

						<xsl:element name="L005">
							<xsl:value-of select="'B'"/>
						</xsl:element>

						<xsl:element name="L008">
							<xsl:value-of select="$Packs"/>
						</xsl:element>

						<xsl:element name="L009">
							<xsl:value-of select="ScriptNS0:GetRecipientCode($Sender, $Recipient, $TS_Name, 'Pack Type', 'Packaging Code - L009', 
                            $Shipment/*[local-name()='OuterPacksPackageType' or local-name()='TotalNoOfPacksPackageType']/*[local-name()='Code'])"/>
						</xsl:element>

					</xsl:element>


					<xsl:if test="$Shipment/*[local-name()='ActualChargeable'] != ''">

						<xsl:element name="ns0:L10">

							<xsl:element name="L1001">
								<xsl:value-of select="$ChargeableWeight"/>
							</xsl:element>

							<xsl:element name="L1002">
								<xsl:text>B</xsl:text>
							</xsl:element>

						</xsl:element>

						<xsl:element name="ns0:L10">

							<xsl:element name="L1001">
								<xsl:value-of select="$ChargeableWeight"/>
							</xsl:element>

							<xsl:element name="L1002">
								<xsl:text>A1</xsl:text>
							</xsl:element>

						</xsl:element>

						<xsl:element name="ns0:L10">

							<xsl:element name="L1001">
								<xsl:value-of select="$ChargeableWeight"/>
							</xsl:element>

							<xsl:element name="L1002">
								<xsl:text>A3</xsl:text>
							</xsl:element>

						</xsl:element>

					</xsl:if>

					<xsl:element name="ns0:L10">

						<xsl:element name="L1001">
							<xsl:variable name="Value" select="ScriptNS3:Convert($Shipment/*[local-name()='TotalWeight'], $TotalWeightUnit, 'KG')" />

							<xsl:choose>
								<xsl:when test="$Value > 0">
									<xsl:value-of select="format-number($Value, '.##')"/>
								</xsl:when>

								<xsl:otherwise>
									<xsl:text>0</xsl:text>
								</xsl:otherwise>
							</xsl:choose>

						</xsl:element>

						<xsl:element name="L1002">
							<xsl:text>G</xsl:text>
						</xsl:element>

					</xsl:element>



					<xsl:variable name="ChargeLines" select="./*[local-name()='PostingJournalCollection']/*[local-name()='PostingJournal']
                        [ScriptNS0:GetRecipientCode($Sender, $Recipient, $TS_Name, 'Charge Code', 'Charge Code - L108', 
                        ./*[local-name()='ChargeCode']/*[local-name()='Code']) != '']" />
					<xsl:variable name="FRTLine" select="$ChargeLines[./*[local-name()='ChargeCode']/*[local-name()='Code'] = $FRT]" />
					<xsl:for-each select="$FRTLine">
						<xsl:call-template name="MapL1" />
					</xsl:for-each>

					<xsl:for-each select="$ChargeLines[./*[local-name()='ChargeCode']/*[local-name()='Code'] != $FRT]">
						<xsl:call-template name="MapL1">
							<xsl:with-param name="Offset" select="count($FRTLine)" />
						</xsl:call-template>
					</xsl:for-each>

				</xsl:element>


			</xsl:element>



			<xsl:element name="ns0:L3">

				<xsl:element name="L301">
					<xsl:value-of select="$ChargeableWeight"/>
				</xsl:element>

				<xsl:element name="L302">
					<xsl:value-of select="'B'"/>
				</xsl:element>

				<xsl:element name="L305">
					<xsl:value-of select="$Total"/>
				</xsl:element>

				<xsl:element name="L311">
					<xsl:value-of select="$Packs"/>
				</xsl:element>

			</xsl:element>

		</ns0:X12_00401_110>
	</xsl:template>



	<xsl:template name="ApplyCharacterSet">
		<xsl:param name="Value" />
		<xsl:param name="CharacterSet" select="'Basic'" />
		<xsl:param name="ReplaceWithValidCharacters" select="true()" />

		<xsl:variable name="Lower" select="'abcdefghijklmnopqrstuvwxyz'" />
		<xsl:variable name="Upper" select="'ABCDEFGHIJKLMNOPQRSTUVWXYZ'" />
		<xsl:variable name="BasicCharacterSet">
			<xsl:value-of select="$Upper"/>
			<xsl:text>0123456789!&quot;&amp;&apos;()*+,-./:;?= </xsl:text>
		</xsl:variable>
		<xsl:variable name="SelectLanguageCharacters" select="'ÀÁÂÄàáâäÈÉÊèéêëÌÍÎìíîïÒÓÔÖòóôöÙÚÛÜùúûüÇçÑñ¿¡'" />
		<xsl:variable name="CorrespondingCharacters"  select="'AAAAAAAAEEEEEEEIIIIIIIOOOOOOOOUUUUUUUUCCNN?!'" />
		<xsl:variable name="ExtendedCharacterSet">
			<xsl:value-of select="$BasicCharacterSet"/>
			<xsl:value-of select="$Lower"/>
			<xsl:text>%@[]_{}\|&lt;&gt;~#$</xsl:text>
			<xsl:value-of select="$SelectLanguageCharacters"/>
		</xsl:variable>

		<xsl:choose>

			<xsl:when test="$CharacterSet = 'Basic'">
				<xsl:choose>

					<xsl:when test="$ReplaceWithValidCharacters">
						<xsl:variable name="NewValue" select="translate($Value, concat($Lower, $SelectLanguageCharacters), concat($Upper, $CorrespondingCharacters))" />
						<xsl:value-of select="translate($NewValue, translate($NewValue, $BasicCharacterSet, ''), '')"/>
					</xsl:when>

					<xsl:otherwise>
						<xsl:value-of select="translate($Value, translate($Value, $BasicCharacterSet, ''), '')"/>
					</xsl:otherwise>

				</xsl:choose>
			</xsl:when>

			<xsl:when test="$CharacterSet = 'Extended'">
				<xsl:value-of select="translate($Value, translate($Value, $ExtendedCharacterSet, ''), '')"/>
			</xsl:when>

			<xsl:otherwise>
				<xsl:value-of select="$Value"/>
			</xsl:otherwise>

		</xsl:choose>

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
						<xsl:call-template name="ApplyCharacterSet">
							<xsl:with-param name="Value" select="$Value" />
						</xsl:call-template>
					</xsl:element>
				</xsl:otherwise>

			</xsl:choose>
		</xsl:if>

	</xsl:template>



	<xsl:template name="MapN9">
		<xsl:param name="NodeName" select="'ns0:N9'" />
		<xsl:param name="N901" />
		<xsl:param name="N902" />

		<xsl:variable name="NewN902">
			<xsl:call-template name="ApplyCharacterSet">
				<xsl:with-param name="Value" select="$N902" />
			</xsl:call-template>
		</xsl:variable>
		<xsl:if test="$N901 != '' and $NewN902 != ''">
			<xsl:element name="{$NodeName}">

				<xsl:element name="N901">
					<xsl:value-of select="$N901"/>
				</xsl:element>

				<xsl:element name="N902">
					<xsl:value-of select="normalize-space(substring($NewN902, 1, 30))"/>
				</xsl:element>

			</xsl:element>
		</xsl:if>
	</xsl:template>



	<xsl:template name="GetIATA">
		<xsl:param name="Port" />
		<xsl:variable name="IATA" select="normalize-space(ScriptNS0:CallActionProcedureHelper('GetIATAfromUNLOCO', '@IATACode', '@UNLOCOCode', $Port))" />
		<xsl:choose>

			<xsl:when test="$IATA != ''">
				<xsl:value-of select="$IATA"/>
			</xsl:when>

			<xsl:otherwise>
				<xsl:value-of select="substring($Port, 3, 3)"/>
			</xsl:otherwise>

		</xsl:choose>
	</xsl:template>



	<xsl:template name="MapN1">
		<xsl:param name="Org"/>
		<xsl:param name="OrgType"/>

		<xsl:if test="$Org != ''">
			<xsl:element name ="ns0:N1Loop1">
				<xsl:element name ="ns0:N1">

					<xsl:element name="N101">
						<xsl:value-of select="$OrgType"/>
					</xsl:element>

					<xsl:element name="N102">
						<xsl:call-template name="ApplyCharacterSet">
							<xsl:with-param name="Value" select="$Org/*[local-name()='CompanyName']" />
						</xsl:call-template>
					</xsl:element>

				</xsl:element>

				<xsl:element name ="ns0:N3">

					<xsl:element name="N301">
						<xsl:call-template name="ApplyCharacterSet">
							<xsl:with-param name="Value" select="$Org/*[local-name()='Address1']" />
						</xsl:call-template>
					</xsl:element>

					<xsl:call-template name="MapValueIfNotEmpty">
						<xsl:with-param name="NodeName" select="'N302'" />
						<xsl:with-param name="Value" select="$Org/*[local-name()='Address2']" />
					</xsl:call-template>

				</xsl:element>

				<xsl:element name ="ns0:N4">

					<xsl:element name="N401">
						<xsl:variable name="City">
							<xsl:call-template name="ApplyCharacterSet">
								<xsl:with-param name="Value" select="$Org/*[local-name()='City'][string-length(.) >= 2]" />
							</xsl:call-template>
						</xsl:variable>
						<xsl:choose>

							<xsl:when test="$City != ''">
								<xsl:value-of select="$City"/>
							</xsl:when>

							<xsl:otherwise>
								<xsl:value-of select="'ZZ'"/>
							</xsl:otherwise>

						</xsl:choose>
					</xsl:element>

					<xsl:variable name="Country" select="$Org/*[local-name()='Country']/*[local-name()='Code'][string-length(.) >= 2]" />
					<xsl:element name="N402">
						<xsl:variable name="State">
							<xsl:call-template name="ApplyCharacterSet">
								<xsl:with-param name="Value" select="substring($Org/*[local-name()='State'][string-length(.) >= 2], 1, 2)" />
							</xsl:call-template>
						</xsl:variable>
						<xsl:choose>

							<xsl:when test="$State != ''">
								<xsl:value-of select="$State"/>
							</xsl:when>

							<xsl:when test="$Country != ''">
								<xsl:value-of select="$Country"/>
							</xsl:when>

							<xsl:otherwise>
								<xsl:value-of select="'ZZ'"/>
							</xsl:otherwise>

						</xsl:choose>
					</xsl:element>


					<xsl:element name="N403">
						<xsl:variable name="Postcode">
							<xsl:call-template name="ApplyCharacterSet">
								<xsl:with-param name="Value" select="translate($Org/*[local-name()='Postcode'], '-', '')" />
							</xsl:call-template>
						</xsl:variable>
						<xsl:variable name="N403" select="normalize-space($Postcode)" />
						<xsl:choose>

							<xsl:when test="$N403 != '' and string-length($N403) >= 3">
								<xsl:value-of select="$N403"/>
							</xsl:when>

							<xsl:otherwise>
								<xsl:value-of select="'ZZZ'"/>
							</xsl:otherwise>

						</xsl:choose>
					</xsl:element>

					<xsl:element name="N404">
						<xsl:choose>

							<xsl:when test="$Country != ''">
								<xsl:value-of select="$Country"/>
							</xsl:when>

							<xsl:otherwise>
								<xsl:value-of select="'ZZ'"/>
							</xsl:otherwise>

						</xsl:choose>
					</xsl:element>

				</xsl:element>

				<xsl:if test="$OrgType = 'BT'">

					<xsl:call-template name="MapN9">
						<xsl:with-param name="N901" select="'BM'" />
						<xsl:with-param name="N902" select="$HWB" />
					</xsl:call-template>

					<xsl:call-template name="MapN9">
						<xsl:with-param name="N901" select="'AW'" />
						<xsl:with-param name="N902" select="$HWB" />
					</xsl:call-template>

					<xsl:call-template name="MapN9">
						<xsl:with-param name="N901" select="'MB'" />
						<xsl:with-param name="N902" select="//*[contains(local-name(), 'BillNumber')][../*[contains(local-name(), 'BillType')][*[local-name()='Code'] = 'MWB']  and . != '']" />
					</xsl:call-template>

					<xsl:call-template name="MapN9">
						<xsl:with-param name="N901" select="'RMT'" />
						<xsl:with-param name="N902" select="concat($Org/../*[local-name()='JobInvoiceNumber'], ' ', $Org/../*[local-name()='Number'])" />
					</xsl:call-template>

					<xsl:call-template name="MapN9">
						<xsl:with-param name="N901" select="'VX'" />
						<xsl:with-param name="N902" select="ScriptNS0:GetRecipientCode($Sender, $Recipient, $TS_Name, 'VAT No', 'VAT No - N9*VX', $OrgCode)" />
					</xsl:call-template>

				</xsl:if>

			</xsl:element>
		</xsl:if>
	</xsl:template>



	<xsl:template name="MapL1">
		<xsl:param name="Offset" select="0" />

		<xsl:element name="ns0:L1Loop1">
			<xsl:element name="ns0:L1">

				<xsl:element name="L101">
					<xsl:value-of select="position() + $Offset"/>
				</xsl:element>

				<xsl:variable name="ChargeCode" select="./*[local-name()='ChargeCode']/*[local-name()='Code']" />
				<xsl:variable name="OSTotalAmount" select="./*[local-name()='OSTotalAmount']" />
				<xsl:if test="$ChargeCode = $FRT">

					<xsl:element name="L102">
						<xsl:choose>

							<xsl:when test="$OSTotalAmount = number($OSTotalAmount)">
								<xsl:value-of select="format-number($OSTotalAmount, '.##')"/>
							</xsl:when>

							<xsl:otherwise>
								<xsl:text>0</xsl:text>
							</xsl:otherwise>

						</xsl:choose>
					</xsl:element>

					<xsl:element name="L103">
						<xsl:value-of select="$L103"/>
					</xsl:element>

				</xsl:if>

				<xsl:element name="L104">
					<xsl:choose>

						<xsl:when test="$OSTotalAmount = number($OSTotalAmount)">
							<xsl:value-of select="format-number($OSTotalAmount * 100, '0')"/>
						</xsl:when>

						<xsl:otherwise>
							<xsl:text>0</xsl:text>
						</xsl:otherwise>

					</xsl:choose>
				</xsl:element>

				<xsl:element name="L108">
					<xsl:value-of select="ScriptNS0:GetRecipientCode($Sender, $Recipient, $TS_Name, 'Charge Code', 'Charge Code - L108', $ChargeCode)"/>
				</xsl:element>

				<xsl:call-template name="MapValueIfNotEmpty">
					<xsl:with-param name="NodeName" select="'L112'" />
					<xsl:with-param name="Value" select="normalize-space(substring(./*[local-name()='ChargeCode']/*[local-name()='Description'][string-length(.) >= 2], 1, 25))" />
				</xsl:call-template>

			</xsl:element>
		</xsl:element>
	</xsl:template>



</xsl:stylesheet>

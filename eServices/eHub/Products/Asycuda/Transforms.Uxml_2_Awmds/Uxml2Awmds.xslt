<?xml version="1.0" encoding="utf-8"?>

<!-- August 2016
This one template is suitable for UniversalShipment 2011 or 2012.  We defo do not want to have to maintain two mappings.
The most importnat XPath change was to the ActionPurpose, used to grab the country code.
From:	//s0:UniversalShipment/s0:Shipment/s0:DataContext/s0:ActionPurpose/s0:Code/text()
To:		//s0:UniversalShipment/s0:Shipment/s0:DataContext/s0:Workflow/s0:ActionPurpose/text()
No problem.
But someone, in their infinite wisdom, decided that
	<Foo>
		<Code>b</Code>
		<Description>bar</Description>
	</Foo>
was not good enough for 2012 and changed it to be
	<Foo Description="bar">b</Foo>
So we've had to change all our XPath to look for both Foo/Code/text() and Foo/text().
-->

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:msxsl="urn:schemas-microsoft-com:xslt"
				xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var"
				exclude-result-prefixes="msxsl var s0 ScriptNS2 userCSharp"
				version="1.0"
				xmlns:s0="http://www.cargowise.com/Schemas/Universal/2011/11"
				xmlns:ScriptNS2="http://schemas.microsoft.com/BizTalk/2003/ScriptNS2"
				xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">

	<xsl:output omit-xml-declaration="yes" method="xml" version="1.0"  />

	<xsl:variable name="smallcase" select="'abcdefghijklmnopqrstuvwxyz'" />
	<xsl:variable name="uppercase" select="'ABCDEFGHIJKLMNOPQRSTUVWXYZ'" />

	<xsl:variable name="CountryCode" select="substring(//s0:UniversalShipment/s0:Shipment/s0:DataContext/s0:ActionPurpose/s0:Code/text()
														|
														//s0:UniversalShipment/s0:Shipment/s0:DataContext/s0:Workflow/s0:ActionPurpose/text()
														, 2, 2)" />
	<xsl:variable name="IsFiji"		select="$CountryCode = 'FJ'" />
	<xsl:variable name="IsSolomon"	select="$CountryCode = 'SB'" />
	<xsl:variable name="IsSriLanka"	select="$CountryCode = 'LK'" />
	<xsl:variable name="IsBangladesh"	select="$CountryCode = 'BD'" />
	<xsl:variable name="IsMadagascar"	select="$CountryCode = 'MG'" />
	<xsl:variable name="IsVanuatu"	select="$CountryCode = 'VU'" />
	<xsl:variable name="IsCookIslands"	select="$CountryCode = 'CK'" />
	<xsl:variable name="IsTuvalu"	select="$CountryCode = 'TV'" />
	<xsl:variable name="IsKiribati"	select="$CountryCode = 'KI'" />
	<xsl:variable name="IsNiue"	select="$CountryCode = 'NU'" />
	<xsl:variable name="IsNauru"	select="$CountryCode = 'NR'" />
	<xsl:variable name="IsTonga"	select="$CountryCode = 'TO'" />

	<xsl:variable name="NatureCode" select="s0:UniversalShipment/s0:Shipment/s0:EntryInstructionCollection/s0:EntryInstruction/s0:AddInfoCollection/s0:AddInfo[s0:Key='AHC_Nature']/s0:Value/text()"/>
	
	<xsl:template match="s0:UniversalShipment">
		<AsycudaWorldPack>
			<xsl:call-template name="Awmds" />
			<xsl:call-template name="Awbolds" />
			<xsl:if test="$IsBangladesh and $NatureCode = 'EXP'">
				<xsl:call-template name="Awbolegmds" />
			</xsl:if>
			<xsl:if test="$IsCookIslands or $IsTonga">
				<xsl:call-template name="Awmcds" />
			</xsl:if>
		</AsycudaWorldPack>
	</xsl:template>

	<xsl:template name="Awbolegmds">
		
		<xsl:comment>
			Country: <xsl:value-of select="$CountryCode"/>
			Nature: <xsl:value-of select="$NatureCode"/>
		</xsl:comment>

		<Awbolegmds>
			<General_segment>
				<Customs_office_code>
					<xsl:variable name="CustomsOfficeCode" select="s0:Shipment/s0:CustomsOffice/s0:Code/text()"/>
					<xsl:value-of select="$CustomsOfficeCode"/>
				</Customs_office_code>
				<Voyage_number>
					<xsl:value-of select="substring(normalize-space(s0:Shipment/s0:VoyageFlightNo/text()), 1, 17)"/>
				</Voyage_number>
				<Date_of_departure>
					<xsl:value-of select="substring(s0:Shipment/s0:DateCollection/s0:Date[s0:Type='Departure']/s0:Value/text(), 1, 10)"/>
				</Date_of_departure>
				<Nature>22</Nature>
			</General_segment>

			<xsl:for-each select="s0:Shipment/s0:SubShipmentCollection/s0:SubShipment">
				<Bol_segment>
					<Bol_id>
						<Bol_reference>
							<xsl:value-of select="substring(s0:WayBillNumber/text(), 1, 17)"/>
						</Bol_reference>
						<Bol_type_code>HSB</Bol_type_code>
						<Nature>
							<xsl:variable name="ABL_ShipmentType" select="s0:AddInfoCollection/s0:AddInfo[s0:Key='ABL_ShipmentType']/s0:Value/text()"/>
							<xsl:choose>
								<xsl:when test="$ABL_ShipmentType = 'IMP'">23</xsl:when>
								<xsl:when test="$ABL_ShipmentType = 'EXP'">22</xsl:when>
								<xsl:when test="$ABL_ShipmentType = 'TSS'">28</xsl:when>
								<xsl:when test="$ABL_ShipmentType = 'TRN'">24</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="substring($ABL_ShipmentType, 1, 2)"/>
								</xsl:otherwise>
							</xsl:choose>
						</Nature>
						<Master_bol_ref_number>
							<xsl:value-of select="substring(normalize-space(../../../s0:Shipment/s0:WayBillNumber/text()), 1, 17)"/>
						</Master_bol_ref_number>

						<!-- TO DO <Carrier_code> </Carrier_code> -->
						<xsl:variable name="CarrierAddress" select="//s0:UniversalShipment/s0:Shipment/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType='Carrier']"/>
						<Carrier_code>
							<xsl:value-of select="substring(normalize-space($CarrierAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text()='CCC' and s0:CountryOfIssue/s0:Code/text()=$CountryCode]/s0:Value
																			|
																			$CarrierAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='CCC' and s0:CountryOfIssue=$CountryCode]/s0:Value), 1, 17)" />
						</Carrier_code>

						<Num_of_ctn_for_this_bol>
							<xsl:variable name="ThisBillsPackingLinesContainerNumbers">
								<xsl:for-each select="s0:PackingLineCollection/s0:PackingLine">
									,<xsl:value-of select="s0:ContainerNumber/text()"/>,
								</xsl:for-each>
							</xsl:variable>
							<xsl:value-of select ="count(//s0:UniversalShipment/s0:Shipment/s0:ContainerCollection/s0:Container[contains($ThisBillsPackingLinesContainerNumbers, concat(',', s0:ContainerNumber/text(), ',' ))]
												|										  
												s0:ContainerCollection/s0:Container
												)"/>
						</Num_of_ctn_for_this_bol>


					</Bol_id>

					<xsl:call-template name="Sad_segment" >
						<xsl:with-param name="EntryNumbers" select="s0:EntryNumberCollection"/>
					</xsl:call-template>
					
					<xsl:call-template name="EGMPackedContainers" >
						<xsl:with-param name="CountryCode" select="$CountryCode"/>
						<xsl:with-param name="PackedLines" select="s0:PackingLineCollection"/>
					</xsl:call-template>

				</Bol_segment>
			</xsl:for-each>

			<xsl:comment>
				<!-- Don't remove this, it's the consol number and is used to put the consol num into the email subject-->
				<xsl:value-of select="s0:Shipment/s0:DataContext/s0:DataSourceCollection/s0:DataSource[s0:Type='AsycudaManifest']/s0:Key"/>
				<xsl:value-of select="s0:Shipment/s0:DataContext/s0:DataSource[s0:Type='AsycudaManifest']/s0:Key"/>
			</xsl:comment>
		</Awbolegmds>

	</xsl:template>


	<!-- End Awbolegmds, begin  AWBOLDS -->
	
	
	<xsl:template name="Awbolds">

		<xsl:comment>
			Country: <xsl:value-of select="$CountryCode"/>
		</xsl:comment>

		<Awbolds>
			<Master_bol>
				<xsl:variable name="TransportMode" select="//s0:UniversalShipment/s0:Shipment/s0:TransportMode/s0:Code/text() | //s0:UniversalShipment/s0:Shipment/s0:TransportMode/text()"/>
				<Customs_office_code>
					<xsl:variable name="CustomsOfficeCode" select="s0:Shipment/s0:CustomsOffice/s0:Code/text()"/>
					<xsl:value-of select="$CustomsOfficeCode"/>
					<xsl:if test="$IsSriLanka and $CustomsOfficeCode = ''">
					<!-- If CW1 has not sent an office code - use business rules here-->
					<xsl:choose>
							<xsl:when test="$TransportMode='SEA'">SECMB</xsl:when>
							<xsl:when test="$TransportMode='AIR'">ARKTM</xsl:when>
						</xsl:choose>
					</xsl:if>
				</Customs_office_code>
				<Voyage_number>
					<xsl:value-of select="substring(normalize-space(s0:Shipment/s0:VoyageFlightNo/text()), 1, 17)"/>
				</Voyage_number>
				<Date_of_departure>
					<xsl:value-of select="substring(s0:Shipment/s0:DateCollection/s0:Date[s0:Type='Departure']/s0:Value/text(), 1, 10)"/>
				</Date_of_departure>
				<Reference_number>
					<xsl:value-of select="substring(s0:Shipment/s0:AddInfoCollection/s0:AddInfo[s0:Key='ManifestNumber']/s0:Value/text(), 1, 17)"/>
				</Reference_number>
				<xsl:if test="$IsCookIslands and ($TransportMode='AIR' or $TransportMode='SEA')">
					<xsl:variable name="shippingAgentNode" select="s0:Shipment/s0:OrganizationAddressCollection/s0:OrganizationAddress[contains(s0:AddressType, 'ControllingAgent')]" />
					<xsl:variable name="agentCode">
						<xsl:value-of select="substring($shippingAgentNode/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text()='CCD' and s0:CountryOfIssue/s0:Code/text()=$CountryCode]/s0:Value
															|
															$shippingAgentNode/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='CCD' and s0:CountryOfIssue/text()=$CountryCode]/s0:Value
															, 1, 17)"/>
					</xsl:variable>
					<xsl:choose>
						<xsl:when test="$agentCode != ''">
							<Shipping_Agent_code>
								<xsl:value-of select ="$agentCode"/>
							</Shipping_Agent_code>
						</xsl:when>
						<xsl:otherwise>
							<Shipping_Agent_code/>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:if>
			</Master_bol>

			<xsl:for-each select="s0:Shipment/s0:SubShipmentCollection/s0:SubShipment">

				<Bol_segment>
					<Bol_id>
						<Bol_reference>
							<xsl:value-of select="substring(s0:WayBillNumber/text(), 1, 17)"/>
						</Bol_reference>
						<Line_number>
							<xsl:value-of select="position()"/>
						</Line_number>
						<Bol_nature>
							<xsl:variable name="ABL_ShipmentType" select="s0:AddInfoCollection/s0:AddInfo[s0:Key='ABL_ShipmentType']/s0:Value/text()"/>
							<xsl:choose>
								<xsl:when test="$ABL_ShipmentType = 'IMP'">23</xsl:when>
								<xsl:when test="$ABL_ShipmentType = 'EXP'">22</xsl:when>
								<xsl:when test="$ABL_ShipmentType = 'TSS'">28</xsl:when>
								<xsl:when test="$ABL_ShipmentType = 'TRN'">24</xsl:when>
								<xsl:when test="$ABL_ShipmentType = 'TR1'">31</xsl:when>
								<xsl:when test="$ABL_ShipmentType = 'TR2'">32</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="substring($ABL_ShipmentType, 1, 2)"/>
								</xsl:otherwise>
							</xsl:choose>
						</Bol_nature>
						<Bol_type_code>
							<xsl:variable name="TransportMode" select="../../../s0:Shipment/s0:TransportMode/s0:Code/text() | ../../../s0:Shipment/s0:TransportMode/text()" />
							<xsl:choose>
								<xsl:when test="$IsSriLanka">HSB</xsl:when>
								<xsl:when test="$IsBangladesh">HSB</xsl:when>
								<xsl:when test="$IsMadagascar">CTR</xsl:when>
								<xsl:when test="$IsCookIslands and $TransportMode='SEA'">HSB</xsl:when>
								<xsl:when test="$IsTuvalu and $TransportMode='SEA'">HSB</xsl:when>
								<xsl:when test="$IsKiribati and $TransportMode='SEA'">HSB</xsl:when>
								<xsl:when test="$IsNiue and $TransportMode='SEA'">HSB</xsl:when>
								<xsl:when test="$IsNauru and $TransportMode='SEA'">HBL</xsl:when>
								<xsl:when test="$IsTonga and $TransportMode='SEA'">HBL</xsl:when>
								<xsl:when test="$TransportMode='SEA'">BOL</xsl:when>
								<xsl:when test="$TransportMode='AIR'">AWB</xsl:when>
							</xsl:choose>
						</Bol_type_code>
						<xsl:if test="not($IsFiji)">
							<xsl:choose>
								<xsl:when test="$IsSriLanka">
									<Master_bol_ref_number />
								</xsl:when>
								<xsl:otherwise>
									<Master_bol_ref_number>
										<xsl:value-of select="substring(normalize-space(../../../s0:Shipment/s0:WayBillNumber/text()), 1, 17)"/>
									</Master_bol_ref_number>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:if>
						<xsl:if test="$IsSolomon">
							<Unique_carrier_reference/>
						</xsl:if>
						<xsl:if test="$IsSriLanka"> <!-- NB - maybe one day this 'if-is-SriLanka' part just becomes and 'otherwise' to the 'if-is-Solomon' part, i.e. maybe evryone but Solomon has a populated UCR -->
							<xsl:variable name="CarrierAddress" select="//s0:UniversalShipment/s0:Shipment/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType='Carrier']"/>
							<Unique_carrier_reference>
								<xsl:value-of select="substring(normalize-space($CarrierAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text()='CCC' and s0:CountryOfIssue/s0:Code/text()=$CountryCode]/s0:Value
																					|
																				$CarrierAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='CCC' and s0:CountryOfIssue=$CountryCode]/s0:Value), 1, 17)" />
							</Unique_carrier_reference>
						</xsl:if>
						<xsl:if test="$IsBangladesh">
							<DG_status/>
						</xsl:if>
					</Bol_id>

					<xsl:if test="$IsFiji">
						<xsl:call-template name="Transport" >
							<xsl:with-param name="node" select="../.."/>
						</xsl:call-template>
					</xsl:if>

					<xsl:choose>
						<xsl:when test="$IsBangladesh">
							<Consolidated_Cargo>0</Consolidated_Cargo>
						</xsl:when>
						<xsl:when test="$IsSriLanka">
							<xsl:variable name="NumberOfHouseBills" select="count(//s0:UniversalShipment/s0:Shipment/s0:SubShipmentCollection/s0:SubShipment)" />
							<xsl:choose>
								<xsl:when test="$NumberOfHouseBills &gt; 1">
									<Consolidated_Cargo>1</Consolidated_Cargo>
								</xsl:when>
								<xsl:otherwise>
									<Consolidated_Cargo>0</Consolidated_Cargo>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:when>
					</xsl:choose>

					<xsl:variable name="originPort" select="s0:PortOfOrigin/s0:Code/text() | s0:PortOfOrigin/text()" />
					<xsl:variable name="destinationPort" select="s0:PortOfDestination/s0:Code/text() | s0:PortOfDestination/text()" />
					<xsl:call-template name="bol_LoadUnloadPlace">
						<xsl:with-param name="originPort" select="$originPort"/>
						<xsl:with-param name="destinationPort" select="$destinationPort"/>
					</xsl:call-template>
					<xsl:if test="$IsFiji">
						<xsl:call-template name="Country_Info" >
							<xsl:with-param name="originPort" select="$originPort"/>
							<xsl:with-param name="destinationPort" select="$destinationPort"/>
						</xsl:call-template>
					</xsl:if>
					<xsl:call-template name="Traders_segment" />

					<xsl:call-template name="PackedContainers" >
						<xsl:with-param name="CountryCode" select="$CountryCode"/>
						<xsl:with-param name="PackedLines" select="s0:PackingLineCollection"/>
					</xsl:call-template>

					<xsl:call-template name="Goods_segment" >
						<xsl:with-param name="CalledFrom" select="'Awbolds'"/>
					</xsl:call-template>

					<xsl:call-template name="Value_segment" />

					<xsl:if test="not($IsSriLanka)">
						<!-- Shed code - location of goods box 30 on SAD -->
						<Location>
							<Location_code>
								<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key='ABL_LocationOfGoods']/s0:Value/text()"/>
							</Location_code>
							<Location_info>
								<xsl:value-of select="substring(normalize-space(s0:AddInfoCollection/s0:AddInfo[s0:Key='ABL_LocationInformation']/s0:Value/text()), 1, 35)"/>
							</Location_info>
						</Location>
					</xsl:if>


				</Bol_segment>


			</xsl:for-each>


			<xsl:comment> <!-- Don't remove this, it's the consol number and is used to put the consol num into the email subject-->
				<xsl:value-of select="s0:Shipment/s0:DataContext/s0:DataSourceCollection/s0:DataSource[s0:Type='AsycudaManifest']/s0:Key"/>
				<xsl:value-of select="s0:Shipment/s0:DataContext/s0:DataSource[s0:Type='AsycudaManifest']/s0:Key"/>
			</xsl:comment>
		</Awbolds>

	</xsl:template>


	<!-- End Awbolegmds, begin  Awmcds -->


	<xsl:template name="Awmcds">

		<xsl:comment>
			Country: <xsl:value-of select="$CountryCode"/>
		</xsl:comment>

		<Awmcds>
			<Manifest_identification>
				<xsl:variable name="TransportMode" select="//s0:UniversalShipment/s0:Shipment/s0:TransportMode/s0:Code/text() | //s0:UniversalShipment/s0:Shipment/s0:TransportMode/text()"/>
				<Customs_office_code>
					<xsl:variable name="CustomsOfficeCode" select="s0:Shipment/s0:CustomsOffice/s0:Code/text()"/>
					<xsl:value-of select="$CustomsOfficeCode"/>
					<xsl:if test="$IsSriLanka and $CustomsOfficeCode = ''">
						<!-- If CW1 has not sent an office code - use business rules here-->
						<xsl:choose>
							<xsl:when test="$TransportMode='SEA'">SECMB</xsl:when>
							<xsl:when test="$TransportMode='AIR'">ARKTM</xsl:when>
						</xsl:choose>
					</xsl:if>
				</Customs_office_code>
				<Voyage_number>
					<xsl:value-of select="substring(normalize-space(s0:Shipment/s0:VoyageFlightNo/text()), 1, 17)"/>
				</Voyage_number>
				<Date_of_departure>
					<xsl:value-of select="substring(s0:Shipment/s0:DateCollection/s0:Date[s0:Type='Departure']/s0:Value/text(), 1, 10)"/>
				</Date_of_departure>
				<Coloader_code>
					<xsl:value-of select="substring(s0:Shipment/s0:AddInfoCollection/s0:AddInfo[s0:Key='ManifestNumber']/s0:Value/text(), 1, 17)"/>
				</Coloader_code>
				<xsl:if test="$IsCookIslands and ($TransportMode='AIR' or $TransportMode='SEA')">
					<xsl:variable name="shippingAgentNode" select="s0:Shipment/s0:OrganizationAddressCollection/s0:OrganizationAddress[contains(s0:AddressType, 'ControllingAgent')]" />
					<xsl:variable name="agentCode">
						<xsl:value-of select="substring($shippingAgentNode/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text()='CCD' and s0:CountryOfIssue/s0:Code/text()=$CountryCode]/s0:Value
															|
															$shippingAgentNode/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='CCD' and s0:CountryOfIssue/text()=$CountryCode]/s0:Value
															, 1, 17)"/>
					</xsl:variable>
					<xsl:choose>
						<xsl:when test="$agentCode != ''">
							<Shipping_Agent_code>
								<xsl:value-of select ="$agentCode"/>
							</Shipping_Agent_code>
						</xsl:when>
						<xsl:otherwise>
							<Shipping_Agent_code/>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:if>
			</Manifest_identification>

			<xsl:for-each select="s0:Shipment/s0:SubShipmentCollection/s0:SubShipment">

				<Bol_segment>
					<Bol_id>
						<Bol_reference>
							<xsl:value-of select="substring(s0:WayBillNumber/text(), 1, 17)"/>
						</Bol_reference>
						<Line_number>
							<xsl:value-of select="position()"/>
						</Line_number>
						<Bol_nature>
							<xsl:variable name="ABL_ShipmentType" select="s0:AddInfoCollection/s0:AddInfo[s0:Key='ABL_ShipmentType']/s0:Value/text()"/>
							<xsl:choose>
								<xsl:when test="$ABL_ShipmentType = 'IMP'">23</xsl:when>
								<xsl:when test="$ABL_ShipmentType = 'EXP'">22</xsl:when>
								<xsl:when test="$ABL_ShipmentType = 'TSS'">28</xsl:when>
								<xsl:when test="$ABL_ShipmentType = 'TRN'">24</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="substring($ABL_ShipmentType, 1, 2)"/>
								</xsl:otherwise>
							</xsl:choose>
						</Bol_nature>
						<Bol_type_code>
							<xsl:variable name="TransportMode" select="../../../s0:Shipment/s0:TransportMode/s0:Code/text() | ../../../s0:Shipment/s0:TransportMode/text()" />
							<xsl:choose>
								<xsl:when test="$IsSriLanka">HSB</xsl:when>
								<xsl:when test="$IsBangladesh">HSB</xsl:when>
								<xsl:when test="$IsMadagascar">CTR</xsl:when>
								<xsl:when test="$IsCookIslands and $TransportMode='SEA'">HSB</xsl:when>
								<xsl:when test="$IsTuvalu and $TransportMode='SEA'">HSB</xsl:when>
								<xsl:when test="$IsKiribati and $TransportMode='SEA'">HSB</xsl:when>
								<xsl:when test="$IsNiue and $TransportMode='SEA'">HSB</xsl:when>
								<xsl:when test="$IsNauru and $TransportMode='SEA'">HBL</xsl:when>
								<xsl:when test="$IsTonga and $TransportMode='SEA'">HBL</xsl:when>
								<xsl:when test="$TransportMode='SEA'">BOL</xsl:when>
								<xsl:when test="$TransportMode='AIR'">AWB</xsl:when>
							</xsl:choose>
						</Bol_type_code>
						<xsl:if test="not($IsFiji)">
							<xsl:choose>
								<xsl:when test="$IsSriLanka">
									<Master_bol_ref_number />
								</xsl:when>
								<xsl:otherwise>
									<Master_bol_ref_number>
										<xsl:value-of select="substring(normalize-space(../../../s0:Shipment/s0:WayBillNumber/text()), 1, 17)"/>
									</Master_bol_ref_number>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:if>
						<xsl:if test="$IsSolomon">
							<Unique_carrier_reference/>
						</xsl:if>
						<xsl:if test="$IsSriLanka">
							<!-- NB - maybe one day this 'if-is-SriLanka' part just becomes and 'otherwise' to the 'if-is-Solomon' part, i.e. maybe evryone but Solomon has a populated UCR -->
							<xsl:variable name="CarrierAddress" select="//s0:UniversalShipment/s0:Shipment/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType='Carrier']"/>
							<Unique_carrier_reference>
								<xsl:value-of select="substring(normalize-space($CarrierAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text()='CCC' and s0:CountryOfIssue/s0:Code/text()=$CountryCode]/s0:Value
																					|
																				$CarrierAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='CCC' and s0:CountryOfIssue=$CountryCode]/s0:Value), 1, 17)" />
							</Unique_carrier_reference>
						</xsl:if>
						<xsl:if test="$IsBangladesh">
							<DG_status/>
						</xsl:if>
					</Bol_id>

					<xsl:if test="$IsFiji">
						<xsl:call-template name="Transport" >
							<xsl:with-param name="node" select="../.."/>
						</xsl:call-template>
					</xsl:if>

					<xsl:choose>
						<xsl:when test="$IsBangladesh">
							<Consolidated_Cargo>0</Consolidated_Cargo>
						</xsl:when>
						<xsl:when test="$IsSriLanka">
							<xsl:variable name="NumberOfHouseBills" select="count(//s0:UniversalShipment/s0:Shipment/s0:SubShipmentCollection/s0:SubShipment)" />
							<xsl:choose>
								<xsl:when test="$NumberOfHouseBills &gt; 1">
									<Consolidated_Cargo>1</Consolidated_Cargo>
								</xsl:when>
								<xsl:otherwise>
									<Consolidated_Cargo>0</Consolidated_Cargo>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:when>
					</xsl:choose>

					<xsl:variable name="originPort" select="s0:PortOfOrigin/s0:Code/text() | s0:PortOfOrigin/text()" />
					<xsl:variable name="destinationPort" select="s0:PortOfDestination/s0:Code/text() | s0:PortOfDestination/text()" />
					<xsl:call-template name="bol_LoadUnloadPlace">
						<xsl:with-param name="originPort" select="$originPort"/>
						<xsl:with-param name="destinationPort" select="$destinationPort"/>
					</xsl:call-template>
					<xsl:if test="$IsFiji">
						<xsl:call-template name="Country_Info" >
							<xsl:with-param name="originPort" select="$originPort"/>
							<xsl:with-param name="destinationPort" select="$destinationPort"/>
						</xsl:call-template>
					</xsl:if>
					<xsl:call-template name="Traders_segment" />

					<xsl:call-template name="PackedContainers" >
						<xsl:with-param name="CountryCode" select="$CountryCode"/>
						<xsl:with-param name="PackedLines" select="s0:PackingLineCollection"/>
					</xsl:call-template>

					<xsl:call-template name="Goods_segment" >
						<xsl:with-param name="CalledFrom" select="'Awmcds'"/>
					</xsl:call-template>

					<xsl:call-template name="Value_segment" />

					<xsl:if test="not($IsSriLanka)">
						<!-- Shed code - location of goods box 30 on SAD -->
						<Location>
							<Location_code>
								<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key='ABL_LocationOfGoods']/s0:Value/text()"/>
							</Location_code>
							<Location_info>
								<xsl:value-of select="substring(normalize-space(s0:AddInfoCollection/s0:AddInfo[s0:Key='ABL_LocationInformation']/s0:Value/text()), 1, 35)"/>
							</Location_info>
						</Location>
					</xsl:if>


				</Bol_segment>


			</xsl:for-each>


			<xsl:comment>
				<!-- Don't remove this, it's the consol number and is used to put the consol num into the email subject-->
				<xsl:value-of select="s0:Shipment/s0:DataContext/s0:DataSourceCollection/s0:DataSource[s0:Type='AsycudaManifest']/s0:Key"/>
				<xsl:value-of select="s0:Shipment/s0:DataContext/s0:DataSource[s0:Type='AsycudaManifest']/s0:Key"/>
			</xsl:comment>
		</Awmcds>

	</xsl:template>

	<!-- End Awmcds, begin  Awmds -->


	<xsl:template name="Awmds">
		<Awmds>
			<General_segment>
				<General_segment_id>
					<Customs_office_code>
						<xsl:variable name="CustomsOfficeCode" select="s0:Shipment/s0:CustomsOffice/s0:Code/text()"/>
						<xsl:value-of select="$CustomsOfficeCode"/>
						<xsl:if test="$IsSriLanka and $CustomsOfficeCode = ''">
							<!-- If CW1 has not sent an office code - use business rules here-->
							<xsl:variable name="TransportMode" select="//s0:UniversalShipment/s0:Shipment/s0:TransportMode/s0:Code/text() | //s0:UniversalShipment/s0:Shipment/s0:TransportMode/text()"/>
							<xsl:choose>
								<xsl:when test="$TransportMode='SEA'">SECMB</xsl:when>
								<xsl:when test="$TransportMode='AIR'">ARKTM</xsl:when>
							</xsl:choose>
						</xsl:if>
					</Customs_office_code>

					<Voyage_number>
						<xsl:value-of select="substring(normalize-space(s0:Shipment/s0:VoyageFlightNo/text()), 1, 17)"/>
					</Voyage_number>
					<Date_of_departure>
						<xsl:value-of select="substring(s0:Shipment/s0:DateCollection/s0:Date[s0:Type='Departure']/s0:Value/text(), 1, 10)"/>
					</Date_of_departure>
					<Date_of_arrival>
						<xsl:value-of select="substring(s0:Shipment/s0:DateCollection/s0:Date[s0:Type='Arrival']/s0:Value/text(), 1, 10)"/>
					</Date_of_arrival>
					<Time_of_arrival>
						<xsl:value-of select="substring(s0:Shipment/s0:DateCollection/s0:Date[s0:Type='Arrival']/s0:Value/text(), 12)"/>
					</Time_of_arrival>
					<!--
					<xsl:if test="$IsBangladesh">
						<Date_of_last_discharge>
							What shall we put here?   Nothing for now. DJC June 2016
						</Date_of_last_discharge>
					</xsl:if>
					-->
				</General_segment_id>

				<Totals_segment>
					<Total_number_of_bols>
						<xsl:value-of select="count(s0:Shipment/s0:SubShipmentCollection/s0:SubShipment)"/>
					</Total_number_of_bols>
					<Total_number_of_packages>
						<xsl:value-of select="sum(s0:Shipment/s0:SubShipmentCollection/s0:SubShipment/s0:PackingLineCollection/s0:PackingLine/s0:PackQty)"/>
					</Total_number_of_packages>
					<Total_number_of_containers>
						<xsl:value-of select="count(s0:Shipment/s0:ContainerCollection/s0:Container | s0:Shipment/s0:SubShipmentCollection/s0:SubShipment/s0:ContainerCollection/s0:Container)"/>
					</Total_number_of_containers>
					<Total_gross_mass>
						<xsl:choose>
							<xsl:when test="$IsSriLanka">
								<xsl:value-of select="format-number(sum(s0:Shipment/s0:SubShipmentCollection/s0:SubShipment/s0:TotalWeight), '0.00')"/>
							</xsl:when>
							<xsl:otherwise>
								<xsl:value-of select="format-number(sum(s0:Shipment/s0:SubShipmentCollection/s0:SubShipment/s0:TotalWeight), '0.000')"/>
							</xsl:otherwise>
						</xsl:choose>
					</Total_gross_mass>
				</Totals_segment>


				<xsl:call-template name="Transport" >
					<xsl:with-param name="node" select="s0:Shipment"/>
				</xsl:call-template>


				<Load_unload_place>
					<Place_of_departure_code>
						<xsl:value-of select="s0:Shipment/s0:PortOfLoading/s0:Code/text() | s0:Shipment/s0:PortOfLoading/text()"/>
					</Place_of_departure_code>
					<Place_of_destination_code>
						<xsl:value-of select="s0:Shipment/s0:PortOfDischarge/s0:Code/text() | s0:Shipment/s0:PortOfDischarge/text()"/>
					</Place_of_destination_code>
				</Load_unload_place>

			</General_segment>


			<xsl:for-each select="s0:Shipment/s0:SubShipmentCollection/s0:SubShipment">

				<Bol_segment>
					<Bol_id>
						<Bol_reference>
							<xsl:value-of select="substring(s0:WayBillNumber/text(), 1, 17)"/>
						</Bol_reference>
						<Line_number>
							<xsl:value-of select="position()"/>
						</Line_number>
						<Bol_nature>
							<xsl:variable name="ABL_ShipmentType" select="s0:AddInfoCollection/s0:AddInfo[s0:Key='ABL_ShipmentType']/s0:Value/text()"/>
							<xsl:choose>
								<xsl:when test="$ABL_ShipmentType = 'IMP'">23</xsl:when>
								<xsl:when test="$ABL_ShipmentType = 'EXP'">22</xsl:when>
								<xsl:when test="$ABL_ShipmentType = 'TSS'">28</xsl:when>
								<xsl:when test="$ABL_ShipmentType = 'TRN'">24</xsl:when>
								<xsl:when test="$ABL_ShipmentType = 'TR1'">31</xsl:when>
								<xsl:when test="$ABL_ShipmentType = 'TR2'">32</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="substring($ABL_ShipmentType, 1, 2)"/>
								</xsl:otherwise>
							</xsl:choose>
						</Bol_nature>
						<Bol_type_code>
							<xsl:variable name="TransportMode" select="../../../s0:Shipment/s0:TransportMode/s0:Code/text() | ../../../s0:Shipment/s0:TransportMode/text()"/>
							<xsl:choose>
								<xsl:when test="$IsSriLanka">HSB</xsl:when>
								<xsl:when test="$IsBangladesh">HSB</xsl:when>
								<xsl:when test="$IsMadagascar">CTR</xsl:when>
								<xsl:when test="$IsCookIslands and $TransportMode='SEA'">HSB</xsl:when>
								<xsl:when test="$IsTuvalu and $TransportMode='SEA'">HSB</xsl:when>
								<xsl:when test="$IsKiribati and $TransportMode='SEA'">HSB</xsl:when>
								<xsl:when test="$IsNiue and $TransportMode='SEA'">HSB</xsl:when>
								<xsl:when test="$IsNauru and $TransportMode='SEA'">HBL</xsl:when>
								<xsl:when test="$IsTonga and $TransportMode='SEA'">HBL</xsl:when>
								<xsl:when test="$TransportMode='SEA'">BOL</xsl:when>
								<xsl:when test="$TransportMode='AIR'">AWB</xsl:when>
							</xsl:choose>
						</Bol_type_code>
					</Bol_id>

					<xsl:if test="$IsFiji">
						<xsl:call-template name="Transport" >
							<xsl:with-param name="node" select="../.."/>
						</xsl:call-template>
					</xsl:if>

					<xsl:if test="$IsBangladesh">
						<Consolidated_Cargo>0</Consolidated_Cargo>
					</xsl:if>

					<xsl:variable name="originPort" select="s0:PortOfOrigin/s0:Code/text() |  s0:PortOfOrigin/text()" />
					<xsl:variable name="destinationPort" select="s0:PortOfDestination/s0:Code/text() | s0:PortOfDestination/text()" />
					<xsl:call-template name="bol_LoadUnloadPlace">
						<xsl:with-param name="originPort" select="$originPort"/>
						<xsl:with-param name="destinationPort" select="$destinationPort"/>
					</xsl:call-template>
					<xsl:if test="$IsFiji">
						<xsl:call-template name="Country_Info" >
							<xsl:with-param name="originPort" select="$originPort"/>
							<xsl:with-param name="destinationPort" select="$destinationPort"/>
						</xsl:call-template>
					</xsl:if>
					<xsl:call-template name="Traders_segment" />

					<xsl:call-template name="PackedContainers" >
						<xsl:with-param name="CountryCode" select="$CountryCode"/>
						<xsl:with-param name="PackedLines" select="s0:PackingLineCollection"/>
					</xsl:call-template>

					<xsl:call-template name="Goods_segment" >
						<xsl:with-param name="CalledFrom" select="'Awmds'"/>
					</xsl:call-template>

					<xsl:call-template name="Value_segment" />

					<!-- Shed code - location of goods box 30 on SAD -->
					<Location>
						<Location_code>
							<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key='ABL_LocationOfGoods']/s0:Value/text()"/>
						</Location_code>
						<Location_info>
							<xsl:value-of select="substring(normalize-space(s0:AddInfoCollection/s0:AddInfo[s0:Key='ABL_LocationInformation']/s0:Value/text()), 1, 35)"/>
						</Location_info>
					</Location>

				</Bol_segment>
			</xsl:for-each>

			<xsl:variable name="BD_CCD_RegistrationNumber" select="s0:Shipment/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='Branch' and s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text()='CCD' and s0:CountryOfIssue/s0:Code/text()='BD']]/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text()='CCD' and s0:CountryOfIssue/s0:Code/text()='BD']/s0:Value/text()" />
			<xsl:variable name="BD_CCC_RegistrationNumber" select="s0:Shipment/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='Branch' and s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text()='CCC' and s0:CountryOfIssue/s0:Code/text()='BD']]/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text()='CCC' and s0:CountryOfIssue/s0:Code/text()='BD']/s0:Value/text()" />
			<xsl:variable name ="GeneralReference1" select="s0:Shipment/s0:DataContext/s0:DataSourceCollection/s0:DataSource[s0:Type='AsycudaManifest']/s0:Key" />
			<xsl:variable name ="GeneralReference2" select="s0:Shipment/s0:DataContext/s0:DataSource[s0:Type='AsycudaManifest']/s0:Key" />
			<xsl:variable name ="UseColoader" select="$IsCookIslands or $IsTonga" />

			<xsl:choose>
				<xsl:when test="$IsBangladesh">
					<xsl:comment>
						<xsl:value-of select="userCSharp:GetFirstAttachmentName_BD($BD_CCD_RegistrationNumber, $BD_CCC_RegistrationNumber)"/>
					</xsl:comment>
					<xsl:comment>
						<xsl:value-of select="userCSharp:GetSecondAttachmentName_BD($BD_CCD_RegistrationNumber, $BD_CCC_RegistrationNumber)" />
					</xsl:comment>
					<xsl:comment>
						<xsl:value-of select="userCSharp:GetEmailSubject($GeneralReference1, $GeneralReference2)"/>
					</xsl:comment>
					<xsl:if test= "$NatureCode = 'EXP'">
						<xsl:comment>
							<xsl:value-of select="userCSharp:GetThirdAttachmentName_BD($BD_CCD_RegistrationNumber, $BD_CCC_RegistrationNumber)"/>
						</xsl:comment>
					</xsl:if>
				</xsl:when>
				<xsl:otherwise>
					<xsl:comment>
						<xsl:value-of select="userCSharp:GetFirstAttachmentName_General($GeneralReference1, $GeneralReference2)"/>
					</xsl:comment>
					<xsl:comment>
						<xsl:value-of select="userCSharp:GetSecondAttachmentName_General($GeneralReference1, $GeneralReference2)" />
					</xsl:comment>
					<xsl:comment>
						<xsl:value-of select="userCSharp:GetEmailSubject($GeneralReference1, $GeneralReference2)"/>
					</xsl:comment>
					<xsl:if test="$UseColoader">
						<xsl:comment>
							<xsl:value-of select="userCSharp:GetColoaderAttachmentName_General($GeneralReference1, $GeneralReference2)"/>
						</xsl:comment>
					</xsl:if>
				</xsl:otherwise>
			</xsl:choose>
		</Awmds>

	</xsl:template>


	<!-- End Awmds, begin Transport_Information -->


	<xsl:template name="Transport">

		<xsl:param name="node" />
		<Transport_information>
			<xsl:call-template name="Carrier" >
				<xsl:with-param name="node" select="$node"/>
			</xsl:call-template>

			<xsl:variable name="shippingAgentNode" select="$node/s0:OrganizationAddressCollection/s0:OrganizationAddress[contains(s0:AddressType, 'ControllingAgent')]" />
			<xsl:variable name="TransportMode" select="$node/s0:TransportMode/s0:Code/text() | $node/s0:TransportMode/text()" />
			<xsl:choose>
				<xsl:when test="$IsBangladesh or $IsMadagascar">
					<Shipping_Agent>
						<Shipping_Agent_code/>
						<Shipping_Agent_name/>
					</Shipping_Agent>
				</xsl:when>
				<xsl:when test="$shippingAgentNode != '' and not($IsVanuatu)">
					<Shipping_Agent>
						<xsl:variable name="agentCode">
							<xsl:value-of select="substring($shippingAgentNode/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text()='CCD' and s0:CountryOfIssue/s0:Code/text()=$CountryCode]/s0:Value
															|
															$shippingAgentNode/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='CCD' and s0:CountryOfIssue/text()=$CountryCode]/s0:Value
															, 1, 17)"/>
						</xsl:variable>
						<xsl:choose>
							<xsl:when test="$agentCode != ''">
								<Shipping_Agent_code>
									<xsl:value-of select ="$agentCode"/>
								</Shipping_Agent_code>
							</xsl:when>
							<xsl:otherwise>
								<xsl:if test="$IsCookIslands and ($TransportMode='AIR' or $TransportMode='SEA')">
									<Shipping_Agent_code/>
								</xsl:if>
							</xsl:otherwise>
						</xsl:choose>
						<Shipping_Agent_name>
							<xsl:value-of select="substring($shippingAgentNode/s0:CompanyName, 1, 35)" />
						</Shipping_Agent_name>
					</Shipping_Agent>
				</xsl:when>
			</xsl:choose>

			<Mode_of_transport_code>
				<xsl:choose>
					<xsl:when test="$TransportMode='SEA'">1</xsl:when>
					<xsl:when test="$TransportMode='AIR'">4</xsl:when>
					<xsl:when test="$TransportMode='MAI'">5</xsl:when>
				</xsl:choose>
			</Mode_of_transport_code>
			<xsl:variable name="CarrierAddress" select="$node/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType='Carrier']"/>
			<xsl:choose>
				<xsl:when test="$IsFiji and $TransportMode='AIR'">
					<Identity_of_transporter>
						<xsl:value-of select="substring(normalize-space($node/s0:VoyageFlightNo/text()), 1, 17)"/>
					</Identity_of_transporter>
					<Nationality_of_transporter_code>
						<xsl:value-of select="substring($CarrierAddress/s0:Country/s0:Code/text() | $CarrierAddress/s0:Country/text(), 1, 2)"/>
					</Nationality_of_transporter_code>
				</xsl:when>
				<xsl:otherwise>
					<Identity_of_transporter>
						<xsl:value-of select="substring(normalize-space($node/s0:VesselName/text()), 1, 27)"/>
						<!-- XSD says 27, not 37.  Stet. -->
					</Identity_of_transporter>
					<Nationality_of_transporter_code>
						<xsl:value-of select="$node/s0:AddInfoCollection/s0:AddInfo[s0:Key='ConveyanceNationality']/s0:Value/text()"/>
					</Nationality_of_transporter_code>
				</xsl:otherwise>
			</xsl:choose>
			<!-- Bangladesh optional node we will omit: Place_of_transporter -->
			<xsl:if test="$node/s0:LloydsIMO/text() != ''">
				<Registration_number_of_transport_code>
					<xsl:value-of select="$node/s0:LloydsIMO/text()"/>
				</Registration_number_of_transport_code>
			</xsl:if>
			<xsl:if test="$IsBangladesh or $IsVanuatu">
				<xsl:variable name="DateOfRegistration" select="//s0:UniversalShipment/s0:Shipment/s0:EntryNumberCollection/s0:EntryNumber[s0:CountryOfIssue/s0:Code/text()=$CountryCode and s0:Type/s0:Code/text()='ASY']/s0:IssueDate
																|
																//s0:UniversalShipment/s0:Shipment/s0:EntryNumberCollection/s0:EntryNumber[s0:CountryOfIssue/text()=$CountryCode and s0:Type/text()='ASY']/s0:IssueDate"/>
				<xsl:if test="$DateOfRegistration != ''">
					<Date_of_registration>
						<xsl:value-of select="$DateOfRegistration"/>
					</Date_of_registration>
				</xsl:if>
				<Master_information>
					<xsl:value-of select="substring(normalize-space(//s0:UniversalShipment/s0:Shipment/s0:AddInfoCollection/s0:AddInfo[s0:Key='MasterInformation']/s0:Value/text()), 1, 70)"/>
				</Master_information>
			</xsl:if>
		</Transport_information>

	</xsl:template>


	<!-- End transport, begin (Packed)Containers (used by both Awmds and Abolds).
	Note, the current context will be a PackingLineCollection node! -->

	<xsl:template name="PackedContainers">
		<xsl:param name="CountryCode" />
		<xsl:param name="PackedLines"/>



		<xsl:variable name="AllContainers" select="//s0:UniversalShipment/s0:Shipment/s0:ContainerCollection/s0:Container
														|
															s0:ContainerCollection/s0:Container"/>

		<xsl:variable name="UsingOldStyleContainersAtBillSchema" select="count(s0:ContainerCollection/s0:Container) &gt; 0" />

		<xsl:for-each select="$AllContainers">
			<xsl:variable name="HeaderContainerNumber" select="s0:ContainerNumber" />
			<xsl:variable name="PackedLinesRelevantToThisContainer" select="$PackedLines/s0:PackingLine[s0:ContainerNumber = $HeaderContainerNumber ]" />

			<xsl:if test="(count($PackedLinesRelevantToThisContainer) &gt; 0) or ($UsingOldStyleContainersAtBillSchema = true())">
				<ctn_segment>
					<Ctn_reference>
						<xsl:value-of select="$HeaderContainerNumber"/>
					</Ctn_reference>
					<Number_of_packages>
						<xsl:choose>
							<xsl:when test="$UsingOldStyleContainersAtBillSchema = true() and s0:AddInfoCollection/s0:AddInfo[s0:Key='ACN_NumberOfPackages']/s0:Value/text() != ''">
								<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key='ACN_NumberOfPackages']/s0:Value/text()"/>
							</xsl:when>
							<xsl:otherwise>
								<xsl:value-of select="sum($PackedLinesRelevantToThisContainer/s0:PackQty/text())"/>
							</xsl:otherwise>
						</xsl:choose>
					</Number_of_packages>

					<Type_of_container>
						<xsl:variable name="WtgContType" select="s0:ContainerType/s0:Code/text() | s0:ContainerType/text()"/>
						<xsl:variable name="IsoContType" select="s0:ContainerType/s0:ISOCode/text()"/>
						<xsl:choose>
							<xsl:when test="$IsFiji">
								<xsl:choose>
									<xsl:when test="substring($WtgContType,3, 1)='R'">REFR</xsl:when>
									<xsl:when test="substring($WtgContType,3, 1)='U'">FLR</xsl:when>
									<xsl:otherwise>FCL</xsl:otherwise>
								</xsl:choose>
							</xsl:when>
							<xsl:otherwise>
								<xsl:value-of select="$IsoContType"/>
							</xsl:otherwise>
						</xsl:choose>
					</Type_of_container>

					<xsl:variable name="EmptyFullIndicator" select="s0:AddInfoCollection/s0:AddInfo[s0:Key='ACN_EmptyFullIndicator']/s0:Value/text()"/>
					<xsl:choose>
						<xsl:when test="$IsBangladesh">
							<Status>
								<xsl:choose>
									<xsl:when test="$EmptyFullIndicator = 'FCL'">FCL</xsl:when>
									<xsl:when test="$EmptyFullIndicator = 'LCL'">LCL</xsl:when>
									<xsl:when test="$EmptyFullIndicator = 'MT' ">ETY</xsl:when>
									<xsl:when test="$EmptyFullIndicator = 'EMP' ">ETY</xsl:when>
								</xsl:choose>
							</Status>
						</xsl:when>
						<xsl:otherwise>
							<Empty_Full>
								<xsl:choose>
									<xsl:when test="$IsFiji or $IsVanuatu">
										<xsl:choose>
											<xsl:when test="$EmptyFullIndicator = 'FCL'">FS</xsl:when>
											<xsl:when test="$EmptyFullIndicator = 'LCL'">FM</xsl:when>
											<xsl:when test="$EmptyFullIndicator = 'MT' ">MT</xsl:when>
											<xsl:when test="$EmptyFullIndicator = 'EMP'">MT</xsl:when>
										</xsl:choose>
									</xsl:when>
									<xsl:when test="$IsMadagascar">
										<xsl:choose>
											<xsl:when test="$EmptyFullIndicator = 'FCL'">1/1</xsl:when>
											<xsl:when test="$EmptyFullIndicator = 'LCL'">PTI</xsl:when>
											<xsl:when test="$EmptyFullIndicator = 'MT' ">0/0</xsl:when>
											<xsl:when test="$EmptyFullIndicator = 'EMP'">0/0</xsl:when>
										</xsl:choose>
									</xsl:when>
									<xsl:when test="$IsSriLanka">
										<xsl:choose>
											<xsl:when test="$EmptyFullIndicator = 'FCL'">01</xsl:when>
											<xsl:when test="$EmptyFullIndicator = 'LCL'">02</xsl:when>
											<xsl:when test="$EmptyFullIndicator = 'MT' ">00</xsl:when>
											<xsl:when test="$EmptyFullIndicator = 'EMP'">00</xsl:when>
										</xsl:choose>
									</xsl:when>
									<xsl:when test="$IsCookIslands or $IsTuvalu or $IsKiribati or $IsNiue or $IsNauru or $IsTonga">
										<xsl:choose>
											<xsl:when test="$EmptyFullIndicator = 'FCL'">FCL</xsl:when>
											<xsl:when test="$EmptyFullIndicator = 'LCL'">LCL</xsl:when>
											<xsl:when test="$EmptyFullIndicator = 'MT' ">ETY</xsl:when>
											<xsl:when test="$EmptyFullIndicator = 'EMP'">ETY</xsl:when>
										</xsl:choose>
									</xsl:when>
									<xsl:otherwise>
										<xsl:choose>
											<xsl:when test="$EmptyFullIndicator = 'FCL'">FCL</xsl:when>
											<xsl:when test="$EmptyFullIndicator = 'LCL'">LCL</xsl:when>
											<xsl:when test="$EmptyFullIndicator = 'MT' ">EMP</xsl:when>
											<xsl:when test="$EmptyFullIndicator = 'EMP'">EMP</xsl:when>
										</xsl:choose>
									</xsl:otherwise>
								</xsl:choose>
							</Empty_Full>
						</xsl:otherwise>
					</xsl:choose>

					<xsl:choose>
						<xsl:when test="$IsBangladesh">
							<Seal_number>
								<xsl:value-of select="substring(normalize-space(s0:Seal), 1, 10)"/>
							</Seal_number>

							<!-- just grab the FIRST on the relevant pack line -->
							<IMCO>
								<xsl:value-of select="$PackedLinesRelevantToThisContainer/s0:UNDGCollection/s0:UNDG/s0:IMOClass"/>
							</IMCO>
							<UN>
								<xsl:value-of select="$PackedLinesRelevantToThisContainer/s0:UNDGCollection/s0:UNDG/s0:UNDGCode"/>
							</UN>


							<Ctn_location>
								<xsl:value-of select="substring(s0:StowagePosition, 1, 7)"/>
							</Ctn_location>
							<Commodity_code>
								<xsl:value-of select="substring(s0:Commodity/s0:Code/text() | s0:Commodity/text(), 1, 2)"/>
							</Commodity_code>
							<Gross_weight>
								<xsl:value-of select="sum($PackedLinesRelevantToThisContainer/s0:Weight)"/>
							</Gross_weight>
							<!-- CW1 already sends in KG -->
						</xsl:when>
						<xsl:otherwise>
							<xsl:if test="not($IsSriLanka)">
								<xsl:if test="s0:Seal!=''">
									<Marks1>
										<xsl:value-of select="substring(normalize-space(s0:Seal), 1, 10)"/>
									</Marks1>
								</xsl:if>
								<xsl:if test="s0:SecondSeal!=''">
									<Marks2>
										<xsl:value-of select="substring(normalize-space(s0:SecondSeal), 1, 10)"/>
									</Marks2>
								</xsl:if>
								<xsl:if test="s0:ThirdSeal!=''">
									<Marks3>
										<xsl:value-of select="substring(normalize-space(s0:ThirdSeal), 1, 10)"/>
									</Marks3>
								</xsl:if>
								<Sealing_Party>
									<xsl:value-of select="substring(s0:AddInfoCollection/s0:AddInfo[s0:Key='ACN_SealingPartyType']/s0:Value/text(), 1, 3)"/>
								</Sealing_Party>
							</xsl:if>
						</xsl:otherwise>
					</xsl:choose>

				</ctn_segment>
			</xsl:if>
		</xsl:for-each>
	</xsl:template>

	<!-- End transport, begin EGM(Packed)Containers (used by abolegmds).
	Note, the current context will be a PackingLineCollection node! -->

	<xsl:template name="EGMPackedContainers">
		<xsl:param name="CountryCode" />
		<xsl:param name="PackedLines"/>

		<xsl:variable name="AllContainers" select="//s0:UniversalShipment/s0:Shipment/s0:ContainerCollection/s0:Container
														| s0:ContainerCollection/s0:Container"/>

		<xsl:variable name="UsingOldStyleContainersAtBillSchema" select="count(s0:ContainerCollection/s0:Container) &gt; 0" />

		<xsl:for-each select="$AllContainers">
			<xsl:variable name="HeaderContainerNumber" select="s0:ContainerNumber" />
			<xsl:variable name="PackedLinesRelevantToThisContainer" select="$PackedLines/s0:PackingLine[s0:ContainerNumber = $HeaderContainerNumber ]" />

			<xsl:if test="(count($PackedLinesRelevantToThisContainer) &gt; 0) or ($UsingOldStyleContainersAtBillSchema = true())">
				<ctn_segment>
					<Ctn_reference>
						<xsl:value-of select="$HeaderContainerNumber"/>
					</Ctn_reference>
					<Number_of_packages>
						<xsl:choose>
							<xsl:when test="$UsingOldStyleContainersAtBillSchema = true() and s0:AddInfoCollection/s0:AddInfo[s0:Key='ACN_NumberOfPackages']/s0:Value/text() != ''">
								<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key='ACN_NumberOfPackages']/s0:Value/text()"/>
							</xsl:when>
							<xsl:otherwise>
								<xsl:value-of select="sum($PackedLinesRelevantToThisContainer/s0:PackQty/text())"/>
							</xsl:otherwise>
						</xsl:choose>
					</Number_of_packages>

					<Type_of_container>
						<xsl:variable name="IsoContType" select="s0:ContainerType/s0:ISOCode/text()"/>
						<xsl:value-of select="$IsoContType"/>
					</Type_of_container>

					<xsl:variable name="EmptyFullIndicator" select="s0:AddInfoCollection/s0:AddInfo[s0:Key='ACN_EmptyFullIndicator']/s0:Value/text()"/>
					<Empty_Full>
						<xsl:choose>
							<xsl:when test="$EmptyFullIndicator = 'FCL'">FCL</xsl:when>
							<xsl:when test="$EmptyFullIndicator = 'LCL'">LCL</xsl:when>
							<xsl:when test="$EmptyFullIndicator = 'MT' ">ETY</xsl:when>
							<xsl:when test="$EmptyFullIndicator = 'EMP'">ETY</xsl:when>
						</xsl:choose>
					</Empty_Full>

					<xsl:if test="s0:Seal!=''">
						<Marks1>
							<xsl:value-of select="substring(normalize-space(s0:Seal), 1, 10)"/>
						</Marks1>
					</xsl:if>
					<xsl:if test="s0:SecondSeal!=''">
						<Marks2>
							<xsl:value-of select="substring(normalize-space(s0:SecondSeal), 1, 10)"/>
						</Marks2>
					</xsl:if>
					<xsl:if test="s0:ThirdSeal!=''">
						<Marks3>
							<xsl:value-of select="substring(normalize-space(s0:ThirdSeal), 1, 10)"/>
						</Marks3>
					</xsl:if>
					<xsl:variable name="SealingParty" select="s0:AddInfoCollection/s0:AddInfo[s0:Key='ACN_SealingPartyType']/s0:Value/text()"/>
					<xsl:if test="$SealingParty!=''">
						<Sealing_Party>
							<xsl:value-of select="substring($SealingParty, 1, 3)"/>
						</Sealing_Party>
					</xsl:if>
					<Goods_weight>
						<xsl:value-of select="sum($PackedLinesRelevantToThisContainer/s0:Weight)"/>
					</Goods_weight>

				</ctn_segment>
			</xsl:if>
		</xsl:for-each>
	</xsl:template>

	<xsl:template name="SealsForContainers">
		<xsl:param name="PackingLines"  />
		<xsl:for-each select="$PackingLines">
			<xsl:variable name="ContNo" select="s0:ContainerNumber"/>
			<xsl:for-each select="//s0:UniversalShipment/s0:Shipment/s0:ContainerCollection/s0:Container[s0:ContainerNumber=$ContNo]
									|
									../../s0:ContainerCollection/s0:Container">
				<xsl:if test="s0:Seal/text() != ''"><xsl:value-of select="substring(normalize-space(s0:Seal), 1, 10)"/>,</xsl:if><xsl:if test="s0:SecondSeal/text() != ''"><xsl:value-of select="substring(normalize-space(s0:SecondSeal), 1, 10)"/>,</xsl:if><xsl:if test="s0:ThirdSeal/text() != ''"><xsl:value-of select="substring(normalize-space(s0:ThirdSeal), 1, 10)"/>,</xsl:if>
			</xsl:for-each>
		</xsl:for-each>
	</xsl:template>

	<xsl:template name="SealingPartyTypesForContainers">
		<xsl:param name="PackingLines"  />
		<xsl:for-each select="$PackingLines">
			<xsl:variable name="ContNo" select="s0:ContainerNumber"/>
			<xsl:for-each select="//s0:UniversalShipment/s0:Shipment/s0:ContainerCollection/s0:Container[s0:ContainerNumber=$ContNo]
									|
									../../s0:ContainerCollection/s0:Container">
				<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key='ACN_SealingPartyType']/s0:Value/text()"/>
			</xsl:for-each>
		</xsl:for-each>
	</xsl:template>

	<xsl:template name="ShippingMarks">
		<Shipping_marks>
			<xsl:variable name="maxLengthForMarks">
				<xsl:choose>
					<xsl:when test="$IsSriLanka">512</xsl:when>
					<xsl:when test="$IsBangladesh">512</xsl:when>
					<xsl:otherwise>70</xsl:otherwise> <!-- The XSD does not support this claim, but BP says it should be 70.-->
				</xsl:choose>
			</xsl:variable>
			<xsl:variable name="ShippingMarks" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ABL_MarksAndNumbers']/s0:Value
																	|
																	s0:PackingLineCollection/s0:PackingLine/s0:MarksAndNos"/>
			<xsl:value-of select="substring($ShippingMarks, 1, number($maxLengthForMarks))"/>
		</Shipping_marks>
	</xsl:template>

	<xsl:template name="GoodsDescription">
		<Goods_description>
			<xsl:variable name="maxLengthForDescription">
				<xsl:choose>
					<xsl:when test="$IsSriLanka">512</xsl:when>
					<xsl:when test="$IsBangladesh">512</xsl:when>
					<xsl:otherwise>70</xsl:otherwise> <!-- The XSD does not support this claim, but BP says it should be 70.-->
				</xsl:choose>
			</xsl:variable>
			<xsl:value-of select="substring(s0:GoodsDescription, 1, number($maxLengthForDescription))"/>
		</Goods_description>
	</xsl:template>

	<xsl:template name="bol_LoadUnloadPlace">
		<xsl:param name="originPort" />
		<xsl:param name="destinationPort" />
		<Load_unload_place>
			<xsl:choose>
				<xsl:when test="$IsBangladesh">
					<Port_of_origin_code>
						<xsl:value-of select="$originPort"/>
					</Port_of_origin_code>
				</xsl:when>
				<xsl:otherwise>
					<Place_of_loading_code>
						<xsl:value-of select="$originPort"/>
					</Place_of_loading_code>
				</xsl:otherwise>
			</xsl:choose>
			<Place_of_unloading_code>
				<xsl:value-of select="$destinationPort"/>
			</Place_of_unloading_code>
		</Load_unload_place>
	</xsl:template>

	<xsl:template name="Country_Info">
		<xsl:param name="originPort" />
		<xsl:param name="destinationPort" />
		<Country_info>
			<Country>
				<xsl:value-of select="substring($originPort, 1, 2)"/>
			</Country>
			<Next_port>
				<xsl:value-of select="$destinationPort"/>
			</Next_port>
		</Country_info>
	</xsl:template>

	<xsl:template name="Exporter">
		<Exporter>
			<xsl:variable name="ExporterAddress"	select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType='ConsignorDocumentaryAddress']"/>
			<Exporter_name>
				<xsl:value-of select="substring($ExporterAddress/s0:CompanyName/text(), 1, 35)" />
			</Exporter_name>
			<Exporter_address>
				<xsl:variable name="ExporterCountryCodeOrName">
					<xsl:choose>
						<xsl:when test="$IsSriLanka">
							<xsl:value-of select="$ExporterAddress/s0:Country/s0:Name/text()"/>
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="substring($ExporterAddress/s0:Country/s0:Code/text() | $ExporterAddress/s0:Country/text(), 1, 2)"/>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
				<xsl:variable name="maxLengthForExporterAddress">
					<xsl:choose>
						<xsl:when test="$IsSriLanka">175</xsl:when>
						<xsl:when test="$IsBangladesh">175</xsl:when>
						<xsl:otherwise>70</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
				<xsl:value-of select="substring(
													normalize-space(
																	concat(
																			substring($ExporterAddress/s0:Address1/text(), 1, 50),
																			' ',
																			substring($ExporterAddress/s0:Address2/text(), 1, 50),
																			' ',
																			substring($ExporterAddress/s0:City/text(), 1, 50),
																			' ',
																			substring($ExporterAddress/s0:Postcode/text(), 1, 50),
																			' ',
																			translate($ExporterCountryCodeOrName, $smallcase, $uppercase)
																			)
																	)
													 , 1, number($maxLengthForExporterAddress))
												"/>
			</Exporter_address>
		</Exporter>
	</xsl:template>

	<xsl:template name="Notify">
		<Notify>
			<xsl:variable name="NotifyAddress"	select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType='NotifyParty']"/>
			<xsl:variable name="notifyCode">
				<xsl:value-of select="substring($NotifyAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text()='CCD' and s0:CountryOfIssue/s0:Code/text()=$CountryCode]/s0:Value
												|
												$NotifyAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='CCD' and s0:CountryOfIssue/text()=$CountryCode]/s0:Value
												, 1, 17)"/>
			</xsl:variable>
			<xsl:if test="$notifyCode != ''">
				<Notify_code>
					<xsl:value-of select ="$notifyCode"/>
				</Notify_code>
			</xsl:if>
			<Notify_name>
				<xsl:value-of select="substring($NotifyAddress/s0:CompanyName/text(), 1, 35)" />
			</Notify_name>
			<Notify_address>
				<xsl:variable name="NotifyCountryCodeOrName">
					<xsl:choose>
						<xsl:when test="$IsSriLanka">
							<xsl:value-of select="$NotifyAddress/s0:Country/s0:Name/text()"/>
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="substring($NotifyAddress/s0:Country/s0:Code/text() | $NotifyAddress/s0:Country/text(), 1, 2)"/>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
				<xsl:variable name="maxLengthForNotifyAddress">
					<xsl:choose>
						<xsl:when test="$IsSriLanka">175</xsl:when>
						<xsl:when test="$IsBangladesh">175</xsl:when>
						<xsl:otherwise>70</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
				<xsl:value-of select="substring(
													normalize-space(
																	concat(
																			substring($NotifyAddress/s0:Address1/text(), 1, 50),
																			' ',
																			substring($NotifyAddress/s0:Address2/text(), 1, 50),
																			' ',
																			substring($NotifyAddress/s0:City/text(), 1, 50),
																			' ',
																			substring($NotifyAddress/s0:Postcode/text(), 1, 50),
																			' ',
																			translate($NotifyCountryCodeOrName, $smallcase, $uppercase)
																			)
																)
													 , 1, number($maxLengthForNotifyAddress))
									"/>
			</Notify_address>
		</Notify>
	</xsl:template>

	<xsl:template name="Consignee">
		<Consignee>
			<xsl:variable name="ConsigneeAddress"	select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType='ConsigneeDocumentaryAddress']"/>
			<Consignee_code>
				<xsl:value-of select="substring($ConsigneeAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text()='CCD' and s0:CountryOfIssue/s0:Code/text()=$CountryCode]/s0:Value
														|
														$ConsigneeAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='CCD' and s0:CountryOfIssue/text()=$CountryCode]/s0:Value
														 , 1, 17)"/>
			</Consignee_code>
			<Consignee_name>
				<xsl:value-of select="substring(normalize-space($ConsigneeAddress/s0:CompanyName/text()), 1, 35)" />
			</Consignee_name>
			<Consignee_address>
				<xsl:variable name="ConsigneeCountryCodeOrName">
					<xsl:choose>
						<xsl:when test="$IsSriLanka">
							<xsl:value-of select="$ConsigneeAddress/s0:Country/s0:Name/text()"/>
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="substring($ConsigneeAddress/s0:Country/s0:Code/text() | $ConsigneeAddress/s0:Country/text(), 1, 2)"/>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
				<xsl:variable name="maxLengthForConsigneeAddress">
					<xsl:choose>
						<xsl:when test="$IsSriLanka">175</xsl:when>
						<xsl:when test="$IsBangladesh">175</xsl:when>
						<xsl:otherwise>70</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
				<xsl:value-of select="substring(
													normalize-space(
																	concat(
																			substring($ConsigneeAddress/s0:Address1/text(), 1, 50),
																			' ',
																			substring($ConsigneeAddress/s0:Address2/text(), 1, 50),
																			' ',
																			substring($ConsigneeAddress/s0:City/text(), 1, 50),
																			' ',
																			substring($ConsigneeAddress/s0:Postcode/text(), 1, 50),
																			' ',
																			translate($ConsigneeCountryCodeOrName, $smallcase, $uppercase)
																			)
																)
													, 1, number($maxLengthForConsigneeAddress))
									"/>
			</Consignee_address>
		</Consignee>
	</xsl:template>

	<xsl:template name="Carrier">
		<xsl:param name="node" />
		<xsl:variable name="CarrierAddress"	select="$node/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType='Carrier']"/>
		<Carrier>
			<xsl:variable name="CarrierCode" select ="substring(normalize-space($CarrierAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text()='CCC' and s0:CountryOfIssue/s0:Code/text()=$CountryCode]/s0:Value
																		|
																		$CarrierAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='CCC' and s0:CountryOfIssue/text()=$CountryCode]/s0:Value
																		), 1, 17)" />
			<xsl:if test="not($IsSolomon) or $CarrierCode != ''">
				<Carrier_code>
					<xsl:value-of select ="$CarrierCode"/>
				</Carrier_code>
			</xsl:if>
			<Carrier_name>
				<xsl:value-of select="substring(normalize-space($CarrierAddress/s0:CompanyName/text()), 1, 35)" />
			</Carrier_name>
			<Carrier_address>
				<xsl:variable name="CarrierCountryCodeOrName">
					<xsl:choose>
						<xsl:when test="$IsSriLanka">
							<xsl:value-of select="$CarrierAddress/s0:Country/s0:Name/text()"/>
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="substring($CarrierAddress/s0:Country/s0:Code/text() | $CarrierAddress/s0:Country/text(), 1, 2)"/>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
				<xsl:value-of select="substring(
														 normalize-space(
																			concat(
																					substring($CarrierAddress/s0:Address1/text(), 1, 50),
																					' ',
																					substring($CarrierAddress/s0:Address2/text(), 1, 50),
																					' ',
																					substring($CarrierAddress/s0:City/text(), 1, 50),
																					' ',
																					substring($CarrierAddress/s0:Postcode/text(), 1, 50),
																					' ',
																					translate($CarrierCountryCodeOrName, $smallcase, $uppercase)
																					)
																		)
														 , 1, 70)
											"/>
			</Carrier_address>
		</Carrier>
	</xsl:template>

	<xsl:template name="Traders_segment">
		<Traders_segment>
			<xsl:if test ="$IsSriLanka">
				<xsl:call-template name="Carrier" >
					<xsl:with-param name="node" select="//s0:UniversalShipment/s0:Shipment"/>
				</xsl:call-template>
				<!-- needs to pull from the consol'level Shipment node, not from this BOL-level SubShipment-->
			</xsl:if>

			<xsl:if test="$IsBangladesh">
				<xsl:call-template name="Carrier" >
					<xsl:with-param name="node" select="//s0:UniversalShipment/s0:Shipment"/>
				</xsl:call-template>

				<Shipping_Agent>
					<Shipping_Agent_code/>
					<Shipping_Agent_name/>
				</Shipping_Agent>
			</xsl:if>
			<!-- End is Bangladesh -->

			<xsl:call-template name="Exporter" />
			<xsl:call-template name="Notify" />
			<xsl:call-template name="Consignee" />

		</Traders_segment>
	</xsl:template>

	<xsl:template name="Value_segment">
		<Value_segment>
			<xsl:variable name="EXW" select="s0:CommercialInfo/s0:CommercialChargeCollection/s0:CommercialCharge[s0:ChargeType/s0:Code/text()='EXW']
											|
											s0:CommercialInfo/s0:CommercialChargeCollection/s0:CommercialCharge[s0:ChargeType/text()='EXW']" />
			<xsl:if test="$EXW/s0:Amount &gt; 0">
				<Freight_segment>
					<PC_indicator>
						<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key='ABL_PrepaidCollect']/s0:Value/text()"/>
						<!-- We have no code list for this. So using WTG's "PPD" and "CLT". -->
					</PC_indicator>
					<Freight_value>
						<xsl:value-of select="format-number($EXW/s0:Amount, '#.00')"/>
					</Freight_value>
					<Freight_currency>
						<xsl:value-of select="$EXW/s0:Currency/s0:Code/text() | $EXW/s0:Currency/text()"/>
					</Freight_currency>
				</Freight_segment>
			</xsl:if>

			<xsl:variable name="CUS" select="s0:CommercialInfo/s0:CommercialChargeCollection/s0:CommercialCharge[s0:ChargeType/s0:Code/text()='CUS']
											|
											s0:CommercialInfo/s0:CommercialChargeCollection/s0:CommercialCharge[s0:ChargeType/text()='CUS']" />
			<xsl:if test="$CUS/s0:Amount &gt; 0">
				<Customs_segment>
					<Customs_value>
						<xsl:value-of select="format-number($CUS/s0:Amount, '#.00')"/>
					</Customs_value>
					<Customs_currency>
						<xsl:value-of select="$CUS/s0:Currency/s0:Code/text() | $CUS/s0:Currency/text()"/>
					</Customs_currency>
				</Customs_segment>
			</xsl:if>


			<xsl:variable name="ONS" select="s0:CommercialInfo/s0:CommercialChargeCollection/s0:CommercialCharge[s0:ChargeType/s0:Code/text()='ONS']
											|
											s0:CommercialInfo/s0:CommercialChargeCollection/s0:CommercialCharge[s0:ChargeType/text()='ONS']" />
			<xsl:if test="$ONS/s0:Amount &gt; 0">
				<Insurance_segment>
					<Insurance_value>
						<xsl:value-of select="format-number($ONS/s0:Amount, '#.00')"/>
					</Insurance_value>
					<Insurance_currency>
						<xsl:value-of select="$ONS/s0:Currency/s0:Code/text() | $ONS/s0:Currency/text()"/>
					</Insurance_currency>
				</Insurance_segment>
			</xsl:if>

			<xsl:variable name="OFT" select="s0:CommercialInfo/s0:CommercialChargeCollection/s0:CommercialCharge[s0:ChargeType/s0:Code/text()='OFT']
											|
											s0:CommercialInfo/s0:CommercialChargeCollection/s0:CommercialCharge[s0:ChargeType/text()='OFT']" />
			<xsl:if test="$OFT/s0:Amount &gt; 0">
				<Transport_segment>
					<Transport_value>
						<xsl:value-of select="format-number($OFT/s0:Amount, '#.00')"/>
					</Transport_value>
					<Transport_currency>
						<xsl:value-of select="$OFT/s0:Currency/s0:Code/text() | $OFT/s0:Currency/text()"/>
					</Transport_currency>
				</Transport_segment>
			</xsl:if>
		</Value_segment>
	</xsl:template>

	<xsl:template name="Goods_segment">
		<xsl:param name="CalledFrom" select="'Awmds'"/>

		<Goods_segment>
			<Number_of_packages>
				<xsl:value-of select="sum(s0:PackingLineCollection/s0:PackingLine/s0:PackQty)"/>
			</Number_of_packages>
			<Package_type_code>
				<xsl:variable name="customsPackType" select="s0:PackingLineCollection/s0:PackingLine/s0:CustomsPackType/s0:Code/text() | s0:PackingLineCollection/s0:PackingLine/s0:CustomsPackType/text()"/>
				<xsl:variable name="packType" select="s0:PackingLineCollection/s0:PackingLine/s0:PackType/s0:Code/text() | s0:PackingLineCollection/s0:PackingLine/s0:PackType/text()"/>
				<xsl:choose>
					<xsl:when test="$customsPackType != ''">
						<xsl:value-of select="$customsPackType"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="$packType"/>
					</xsl:otherwise>
				</xsl:choose>
			</Package_type_code>
			<Gross_mass>
				<xsl:choose>
					<xsl:when test="$IsSriLanka">
						<xsl:value-of select="format-number(s0:TotalWeight, '0.00')"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="format-number(s0:TotalWeight, '0.000')"/>	<!-- CW1 already converts to KG -->
					</xsl:otherwise>
				</xsl:choose>
			</Gross_mass>

			<xsl:call-template name="ShippingMarks" />
			<xsl:call-template name="GoodsDescription" />

			<!--Show Seals segment by default unless one of the conditions below is met-->
			<xsl:choose>
				<xsl:when test="($CalledFrom = 'Awbolds' or $CalledFrom = 'Awmcds') and $IsSriLanka"/>	<!--Dont show seals for LK Awbolds segment-->
				<xsl:when test="($CalledFrom = 'Awbolds' or $CalledFrom = 'Awmcds') and $IsFiji"/>		<!--Dont show seals for FJ Awbolds segment-->
				<xsl:when test="$IsBangladesh"/>										<!--Dont show seals for BD-->
				<xsl:otherwise>
					<xsl:call-template name="Seals" />
				</xsl:otherwise>
			</xsl:choose>

			<Volume_in_cubic_meters>
				<xsl:choose>
					<xsl:when test="$IsSriLanka">
						<xsl:value-of select="format-number(s0:TotalVolume, '0.00')"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="s0:TotalVolume"/>
					</xsl:otherwise>
				</xsl:choose>
			</Volume_in_cubic_meters>
			<Num_of_ctn_for_this_bol>
				<xsl:variable name="ThisBillsPackingLinesContainerNumbers">
					<xsl:for-each select="s0:PackingLineCollection/s0:PackingLine">
						,<xsl:value-of select="s0:ContainerNumber/text()"/>,
					</xsl:for-each>
				</xsl:variable>
				<xsl:value-of select ="count(//s0:UniversalShipment/s0:Shipment/s0:ContainerCollection/s0:Container[contains($ThisBillsPackingLinesContainerNumbers, concat(',', s0:ContainerNumber/text(), ',' ))]
												|
												s0:ContainerCollection/s0:Container
												)"/>
			</Num_of_ctn_for_this_bol>

			<xsl:if test="$IsSriLanka and ($CalledFrom = 'Awbolds' or $CalledFrom = 'Awmcds')">
				<Information>
					<xsl:value-of select="s0:NoteCollection/s0:Note[s0:Description='Remarks']/s0:NoteText/text()"/>  <!-- comes from ABL_Remarks-->
				</Information>
			</xsl:if>
			<xsl:if test="$IsBangladesh">
				<Remarks>
					<xsl:value-of select="substring(s0:NoteCollection/s0:Note[s0:Description='Remarks']/s0:NoteText/text(), 0, 70)"/>
				</Remarks>
			</xsl:if>

		</Goods_segment>
	</xsl:template>

	<xsl:template name="Seals">
		<xsl:variable name="seals" >
			<xsl:call-template name="SealsForContainers">
				<xsl:with-param name="PackingLines" select="s0:PackingLineCollection/s0:PackingLine" />
			</xsl:call-template>
		</xsl:variable>
		<xsl:if test ="string-length($seals) &gt; 0">
			<Seals_segment>
				<Number_of_seals>
					<xsl:value-of select="string-length($seals) - string-length(translate($seals, ',', ''))"/>
				</Number_of_seals>
				<Marks_of_seals>
					<xsl:value-of select="substring(normalize-space($seals), 1, 20)"/>
				</Marks_of_seals>
				<Sealing_party_code>
					<xsl:variable name="AllSealingPartyCodes">
						<xsl:call-template name="SealingPartyTypesForContainers">
							<xsl:with-param name="PackingLines" select="s0:PackingLineCollection/s0:PackingLine" />
						</xsl:call-template>
					</xsl:variable>
					<xsl:value-of select="substring($AllSealingPartyCodes, 1, 3)"/>
				</Sealing_party_code>
			</Seals_segment>
		</xsl:if>
	</xsl:template>

	<xsl:template name="Sad_segment">
		<xsl:param name="EntryNumbers" />
		
		<Sad_segment>
			<Sad_office_code>
				<xsl:value-of select="$EntryNumbers/s0:EntryNumber[s0:Type/s0:Code/text()='SAD']/s0:EntryLineReference/text()"/>
			</Sad_office_code>
			<Registration_year>
				<xsl:value-of select="substring($EntryNumbers/s0:EntryNumber[s0:Type/s0:Code/text()='SAD']/s0:IssueDate/text(), 1, 4)"/>
			</Registration_year>
			<Registration_serial>
				<xsl:value-of select="$EntryNumbers/s0:EntryNumber[s0:Type/s0:Code/text()='SAD']/s0:Category/text()"/>
			</Registration_serial>
			<Registration_number>
				<xsl:value-of select="$EntryNumbers/s0:EntryNumber[s0:Type/s0:Code/text()='SAD']/s0:Number/text()"/>
			</Registration_number>
		</Sad_segment>
	</xsl:template>
	
	<msxsl:script language="C#" implements-prefix="userCSharp">
<![CDATA[
public string GetFirstAttachmentName_BD(string namePart1, string namePart2)
{
	return GetAttachmentName_BD(namePart1, namePart2, "MAN");
}

public string GetSecondAttachmentName_BD(string namePart1, string namePart2)
{
	return GetAttachmentName_BD(namePart1, namePart2, "DEG");
}

public string GetThirdAttachmentName_BD(string namePart1, string namePart2)
{
	return GetAttachmentName_BD(namePart1, namePart2, "SAD");
}

string GetAttachmentName_BD(string namePart1, string namePart2, string prefix)
{
	return prefix + GetEmailSubject(namePart1, namePart2) + "[%DATE_FORMAT_FOR_BD%]"+ ".xml";
}

public string GetFirstAttachmentName_General(string namePart1, string namePart2)
{
	return GetAttachmentName_General(namePart1, namePart2, "Manifest");
}

public string GetColoaderAttachmentName_General(string namePart1, string namePart2)
{
	return GetAttachmentName_General(namePart1, namePart2, "Coloader");
}

public string GetSecondAttachmentName_General(string namePart1, string namePart2)
{
	return GetAttachmentName_General(namePart1, namePart2, "Groupage");
}

string GetAttachmentName_General(string namePart1, string namePart2, string suffix)
{
	return GetEmailSubject(namePart1, namePart2) + "[%DATE_FORMAT_FOR_GENERAL%]" + suffix + ".xml";
}

public string GetEmailSubject(string namePart1, string namePart2)
{
	return namePart1 + (System.String.IsNullOrEmpty(namePart1) ? namePart2 : "");
}
]]>
	</msxsl:script>

</xsl:stylesheet>

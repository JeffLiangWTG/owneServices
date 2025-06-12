<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var" exclude-result-prefixes="msxsl var s0 ScriptNS0 userCSharp" version="1.0" xmlns:ns0="http://cargowise.com/ehub/clients/HHE/2011/06" xmlns:s0="http://www.cargowise.com/Schemas/Native" xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0" xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">

	<!-- Write Shipment OrganizationAddressCollection Begin-->
	<xsl:element name="ns0:OrganizationAddressCollection">
		<xsl:call-template name="WriteOrganizationAddress">
			<xsl:with-param name="addressType" select="'ConsignorDocumentaryAddress'"/>
			<xsl:with-param name="organizationCode" select="./*[local-name()='ShipperName']/text()"/>
			<xsl:with-param name="address1" select="./*[local-name()='ShipperAddress1']/text()"/>
		</xsl:call-template>
		<xsl:call-template name="WriteOrganizationAddress">
			<xsl:with-param name="addressType" select="'ConsigneeDocumentaryAddress'"/>
			<xsl:with-param name="organizationCode" select="./*[local-name()='ConsigneeName']/text()"/>
			<xsl:with-param name="address1" select="./*[local-name()='ConsigneeAddress1']/text()"/>
		</xsl:call-template>
		<xsl:variable name="notify1Name" select="./*[local-name()='Notify1Name']/text()"/>
		<xsl:if test="$notify1Name!='SAME AS CONSIGNEE'">
			<xsl:call-template name="WriteOrganizationAddress">
				<xsl:with-param name="addressType" select="'NotifyParty'"/>
				<xsl:with-param name="organizationCode" select="$notify1Name"/>
				<xsl:with-param name="address1" select="./*[local-name()='Notify1Address1']/text()"/>
			</xsl:call-template>
		</xsl:if>
	</xsl:element>
	<!-- Write Shipment OrganizationAddressCollection End-->

	<!-- Write Shipment OrganizationAddress Begin-->
	<xsl:template name="WriteOrganizationAddress">
		<xsl:param name="addressType" />
		<xsl:param name="organizationCode" />
		<xsl:param name="address1" />
		<xsl:element name="ns0:OrganizationAddress">
			<xsl:element name="ns0:AddressType">
				<xsl:value-of select="$addressType" />
			</xsl:element>
			<xsl:element name="ns0:OrganizationCode">
				<xsl:value-of select="ScriptNS0:GetRecipientCode('OSPAKLAKL_GGL', 'OSPAKLAKL', 'GGL xml-File - Import Consols + Shipments', 'Party Identification', string($organizationCode))" />
			</xsl:element>
			<xsl:element name="ns0:Address1">
				<xsl:value-of select="$address1" />
			</xsl:element>
			<xsl:element name="ns0:CompanyName">
				<xsl:value-of select="$organizationCode" />
			</xsl:element>
		</xsl:element>
	</xsl:template>
	<!-- Write Shipment OrganizationAddress End-->

	<!-- Write Shipment Pack+Volume+Weight Begin-->
	<xsl:variable name="packTypeCodeForShipment" select="ScriptNS0:GetRecipientCode('OSPAKLAKL_GGL', 'OSPAKLAKL', 'GGL xml-File - Import Consols + Shipments', 'Package Type', string(./*[local-name()='Package']/text()))" />
	<xsl:variable name="volumeUnitCodeForShipment" select="'CF'"/>
	<xsl:variable name="weightUnitCodeForShipment" select="'KG'"/>
	<xsl:variable name="houseBLNumberForShipment" select="./*[local-name()='HouseBLNum']/text()"/>
	<xsl:element name="ns0:OuterPacks">
		<xsl:value-of select="sum(../../*[local-name()='HouseContainers']/*[local-name()='HouseContainer'][*[local-name()='HouseBLNum']/text()=$houseBLNumberForShipment]/*[local-name()='Packages'])"/>
	</xsl:element>
	<xsl:element name="ns0:OuterPacksPackageType">
		<xsl:element name="ns0:Code">
			<xsl:value-of select="$packTypeCodeForShipment"/>
		</xsl:element>
	</xsl:element>
	<xsl:element name="ns0:TotalVolume">
		<xsl:value-of select="sum(../../*[local-name()='HouseContainers']/*[local-name()='HouseContainer'][*[local-name()='HouseBLNum']/text()=$houseBLNumberForShipment]/*[local-name()='Volume'])"/>
	</xsl:element>
	<xsl:element name="ns0:TotalVolumeUnit">
		<xsl:element name="ns0:Code">
			<xsl:value-of select="$volumeUnitCodeForShipment"/>
		</xsl:element>
	</xsl:element>
	<xsl:element name="ns0:TotalWeight">
		<xsl:value-of select="sum(../../*[local-name()='HouseContainers']/*[local-name()='HouseContainer'][*[local-name()='HouseBLNum']/text()=$houseBLNumberForShipment]/*[local-name()='Weight'])"/>
	</xsl:element>
	<xsl:element name="ns0:TotalWeightUnit">
		<xsl:element name="ns0:Code">
			<xsl:value-of select="$weightUnitCodeForShipment"/>
		</xsl:element>
	</xsl:element>
	<!-- Write Shipment Pack+Volume+Weight End-->
	
	<!-- Write Shipment PackingLineCollection Begin-->
	<xsl:variable name="goodsDescription" select="./*[local-name()='Commodity']/text()"/>
	<xsl:variable name="harmonisedCode" select="./*[local-name()='HSCode']/text()"/>
	<xsl:variable name="marksAndNos" select="./*[local-name()='Marks_no']/text()"/>
	<xsl:variable name="packTypeCode" select="ScriptNS0:GetRecipientCode('OSPAKLAKL_GGL', 'OSPAKLAKL', 'GGL xml-File - Import Consols + Shipments', 'Package Type', string(./*[local-name()='Package']/text()))" />
	<xsl:variable name="volumeUnitCode" select="'CF'"/>
	<xsl:variable name="weightUnitCode" select="'KG'"/>
	<xsl:variable name="uNDGCode" select="./*[local-name()='UNNum']/text()"/>
	<xsl:variable name="flashPoint" select="./*[local-name()='FlashPoint']/text()"/>
	<xsl:variable name="houseBLNumber" select="./*[local-name()='HouseBLNum']/text()"/>
	<xsl:element name="ns0:PackingLineCollection">
		<xsl:for-each select="../../*[local-name()='HouseContainers']/*[local-name()='HouseContainer']">
			<xsl:if test="./*[local-name()='HouseBLNum']/text()=$houseBLNumber">
				<xsl:element name="ns0:PackingLine">
					<xsl:variable name="containerNumber" select="./*[local-name()='ContainerNum']/text()" />
					<xsl:element name="ns0:ContainerNumber">
						<xsl:value-of select="$containerNumber" />
					</xsl:element>
					<xsl:element name="ns0:HarmonisedCode">
						<xsl:value-of select="$harmonisedCode" />
					</xsl:element>
					<xsl:element name="ns0:PackQty">
						<xsl:value-of select="./*[local-name()='Packages']/text()" />
					</xsl:element>
					<xsl:element name="ns0:PackType">
						<xsl:element name="Code">
							<xsl:value-of select="$packTypeCode" />
						</xsl:element>
					</xsl:element>					
					<xsl:element name="ns0:Volume">
						<xsl:value-of select="./*[local-name()='Volume']/text()" />
					</xsl:element>
					<xsl:element name="ns0:Weight">
						<xsl:value-of select="./*[local-name()='Weight']/text()" />
					</xsl:element>
					<xsl:if test="$uNDGCode!=''">
						<xsl:element name="ns0:UNDGCollection">
							<xsl:element name="ns0:UNDG">
								<xsl:element name="ns0:UNDGCode">
									<xsl:value-of select="$uNDGCode" />
								</xsl:element>
								<xsl:element name="ns0:FlashPoint">
									<xsl:value-of select="$flashPoint" />
								</xsl:element>
							</xsl:element>
						</xsl:element>
					</xsl:if>
				</xsl:element>
			</xsl:if>
		</xsl:for-each>
	</xsl:element>
	<!-- Write Shipment PackingLineCollection End-->
	
</xsl:stylesheet>


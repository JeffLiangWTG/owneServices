<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 ScriptNS0 ScriptNS1 ScriptNS2 ScriptNS3 ScriptNS4 ScriptNS5 userCSharp" version="1.0"
                xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:ns1="http://agriculture.gov.au/nexdoc/common/CommonTypes_1.0"
                xmlns:ns2="http://agriculture.gov.au/nexdoc/common/rex/CommonTypes_1.0"
                xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
                xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1"
                xmlns:ScriptNS2="http://schemas.microsoft.com/BizTalk/2003/ScriptNS2"
                xmlns:ScriptNS3="http://schemas.microsoft.com/BizTalk/2003/ScriptNS3"
                xmlns:ScriptNS4="http://schemas.microsoft.com/BizTalk/2003/ScriptNS4"
                xmlns:ScriptNS5="http://schemas.microsoft.com/BizTalk/2003/ScriptNS5"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
	<xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />

	<xsl:template match="/">
		<xsl:apply-templates select="s0:UniversalShipment/s0:Shipment" />
	</xsl:template>

	<xsl:variable name="smallcase" select="'abcdefghijklmnopqrstuvwxyz'" />
	<xsl:variable name="uppercase" select="'ABCDEFGHIJKLMNOPQRSTUVWXYZ'" />
	<xsl:variable name="SenderID" select="ScriptNS1:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
	<xsl:variable name="RecipientID" select="ScriptNS1:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
	<xsl:variable name="SystemID" select="concat(substring($SenderID, 1, 3), substring($SenderID, 7, 3))"/>
	<xsl:variable name="ExporterReference" select="/s0:UniversalShipment/s0:Shipment/s0:DataContext/s0:DataSourceCollection/s0:DataSource[s0:Type/text()='CustomsDeclaration']/s0:Key/text()" />
	<xsl:variable name="CalculatedExporterReference">
		<xsl:choose>
			<xsl:when test="/s0:UniversalShipment/s0:Shipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='UseOwnerRefAsQuarantineRef']/s0:Value/text() ='Y'">
				<xsl:value-of select="/s0:UniversalShipment/s0:Shipment/s0:OwnerRef/text()" />
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$ExporterReference" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>
	<xsl:variable name="MstType" select="userCSharp:GetMst(/s0:UniversalShipment/s0:Shipment/s0:DataContext/s0:EventReference/text())"/>
	<xsl:variable name="IsEU" select="userCSharp:IsEU(/s0:UniversalShipment/s0:Shipment/s0:DataContext/s0:EventReference/text())"/>
	<xsl:variable name="UseNEXDOCStaging" select="/s0:UniversalShipment/s0:Shipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='UseNEXDOCStaging']/s0:Value/text()='Y'" />
	<xsl:variable name="DepartureDate" select="/s0:UniversalShipment/s0:Shipment/s0:DateCollection/s0:Date[s0:Type='Departure']/s0:Value" />
	<xsl:key name="manufacturer" match="s0:CommercialInvoiceLine[s0:OrganizationAddressCollection/s0:OrganizationAddress/s0:AddressType/text()='Manufacturer']"
           use="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='Manufacturer']" />
	<xsl:key name="QuarantineExDocHeaderAddInfo" match="/s0:UniversalShipment/s0:Shipment/s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice/s0:AddInfoGroupCollection/s0:AddInfoGroup[s0:Type/s0:Code/text()='QH']/s0:AddInfoCollection/s0:AddInfo" use="s0:Key"/>

	<xsl:template match="s0:UniversalShipment/s0:Shipment">
		<xsl:variable name="StaffCode" select="s0:DataContext/s0:EventUser/s0:Code/text()" />
		<xsl:variable name="EmailSubject" select="ScriptNS1:SetContextProperty('OverrideEmailSubject', 'http://cargowise.com/ehub/processing/2010/06', concat($ExporterReference,'_',$MstType))"/>
		<xsl:variable name="FileName" select="ScriptNS1:SetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06', concat($SystemID,'_',$StaffCode))"/>

		<soapenv:Envelope>
			<soapenv:Header>
				<nexauth:authTokens xmlns:nexauth="http://agriculture.gov.au/header/auth">
					<vendorToken>
						<xsl:value-of select="ScriptNS0:GetVendorToken($RecipientID)"/>
					</vendorToken>
					<clientGroupToken>
						<xsl:value-of select="ScriptNS4:GetClientGroupToken($SenderID)"/>
					</clientGroupToken>
					<clientToken>
						<xsl:value-of select="ScriptNS4:GetClientToken($SystemID, $StaffCode)"/>
					</clientToken>
				</nexauth:authTokens>
				<wsse:Security xmlns:wsse="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd">
					<wsse:UsernameToken>
						<wsse:Username>
							<xsl:value-of select="ScriptNS0:GetInstallationToken($RecipientID)"/>
						</wsse:Username>
						<wsse:Password Type="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText">
							<xsl:value-of select="ScriptNS0:GetInstallationPassword($RecipientID)"/>
						</wsse:Password>
					</wsse:UsernameToken>
				</wsse:Security>
			</soapenv:Header>
			<soapenv:Body>
				<xsl:variable name="SOAPBody">
					<xsl:call-template name="SOAPBody"/>
				</xsl:variable>
				<xsl:apply-templates select="msxsl:node-set($SOAPBody)" mode="normalize"/>
			</soapenv:Body>
		</soapenv:Envelope>
	</xsl:template>

	<xsl:template match="@*|node()" mode="normalize">
		<xsl:if test="normalize-space(.) != '' or ./@* != '' or name(.) = 'ns1:contactName'">
			<xsl:copy>
				<xsl:copy-of select = "@*"/>
				<xsl:apply-templates mode="normalize"/>
			</xsl:copy>
		</xsl:if>
	</xsl:template>

	<xsl:template name="SOAPBody">
		<xsl:variable name="rootName">
			<xsl:choose>
				<xsl:when test="$MstType='LODGE'">LodgeRex</xsl:when>
				<xsl:when test="$MstType='ORDER'">OrderRex</xsl:when>
				<xsl:when test="$MstType='AMEND'">AmendRex</xsl:when>
				<xsl:when test="$MstType='WITHDRAW'">WithdrawalRex</xsl:when>
				<xsl:when test="$MstType='REPLACE'">ReplaceCertificate</xsl:when>
				<xsl:when test="$MstType='TRFEDN'">TransferRexEDN</xsl:when>
				<xsl:when test="$MstType='CANEDN'">CancelRexEDN</xsl:when>
				<xsl:when test="$MstType='CANREX'">CancelRex</xsl:when>
				<xsl:when test="$MstType='REISSUE'">ReissueCertificate</xsl:when>
				<xsl:when test="$MstType='PREVIEW'">ReadCertificate</xsl:when>
				<xsl:when test="$MstType='READREX'">ReadRex</xsl:when>
				<xsl:otherwise>
					<xsl:variable name="throwInvalidMst" select="userCSharp:ThrowInvalidMstType($MstType)"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="namespace">
			<xsl:choose>
				<xsl:when test="$MstType='LODGE'">http://agriculture.gov.au/nexdoc/LodgeRexSoap_1.0</xsl:when>
				<xsl:when test="$MstType='ORDER'">http://agriculture.gov.au/nexdoc/OrderRexSoap_1.0</xsl:when>
				<xsl:when test="$MstType='AMEND'">http://agriculture.gov.au/nexdoc/AmendRexSoap_1.0</xsl:when>
				<xsl:when test="$MstType='WITHDRAW'">http://agriculture.gov.au/nexdoc/WithdrawalRexSoap_1.0</xsl:when>
				<xsl:when test="$MstType='REPLACE'">http://agriculture.gov.au/nexdoc/RexCertificateSoap_1.0</xsl:when>
				<xsl:when test="$MstType='TRFEDN'">http://agriculture.gov.au/nexdoc/CustomsSoap_1.0</xsl:when>
				<xsl:when test="$MstType='CANEDN'">http://agriculture.gov.au/nexdoc/CustomsSoap_1.0</xsl:when>
				<xsl:when test="$MstType='CANREX'">http://agriculture.gov.au/nexdoc/CancelRexSoap_1.0</xsl:when>
				<xsl:when test="$MstType='REISSUE'">http://agriculture.gov.au/nexdoc/RexCertificateSoap_1.0</xsl:when>
				<xsl:when test="$MstType='PREVIEW'">http://agriculture.gov.au/nexdoc/ReadCertificateSoap_1.0</xsl:when>
				<xsl:when test="$MstType='READREX'">http://agriculture.gov.au/nexdoc/ReadRexSoap_1.0</xsl:when>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="QHAddInfoGroup" select="s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice/s0:AddInfoGroupCollection/s0:AddInfoGroup[s0:Type/s0:Code/text()='QH']"/>
		<xsl:variable name="rexNumber" select="$QHAddInfoGroup/s0:CustomsReferenceCollection/s0:CustomsReference[s0:Type/s0:Code/text()='RFS']/s0:Reference/text()"/>
		<xsl:variable name="lastAmendDateTimeFromEhub" select="ScriptNS2:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $RecipientID, '@recipientId', $SenderID , '@ST_ID', 'NEXDOC', '@value', $rexNumber)" />
		<xsl:if test="$rootName='ReadCertificate'">
			<xsl:variable name="subscribeJobNumber" select="ScriptNS5:InsertSubscriptionValue('NEXJOB', $RecipientID, $SenderID, $rexNumber, $ExporterReference)"/>
		</xsl:if>
		<xsl:variable name="lastAmendDateTime">
			<xsl:variable name="lastAmendDateTimeFromUXML" select="$QHAddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='LastAmendDateTime']/s0:Value/text()" />
			<xsl:choose>
				<xsl:when test="$lastAmendDateTimeFromUXML!=''">
					<xsl:value-of select="$lastAmendDateTimeFromUXML" />
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$lastAmendDateTimeFromEhub" />
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:choose>
			<xsl:when test="($MstType='WITHDRAW') or ($MstType='CANEDN')">
				<xsl:element name="{$rootName}" namespace="{$namespace}">
					<xsl:element name="identification" namespace="{$namespace}">
						<ns1:rexNumber>
							<xsl:value-of select="$rexNumber"/>
						</ns1:rexNumber>
						<ns2:lastAmendDateTime>
							<xsl:value-of select="$lastAmendDateTime"/>
						</ns2:lastAmendDateTime>
					</xsl:element>
				</xsl:element>
			</xsl:when>
			<xsl:when test="($MstType='REISSUE')">
				<xsl:element name="{$rootName}" namespace="{$namespace}">
					<xsl:variable name="certificateNumber" select="$QHAddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ReissueCertificateName']/s0:Value/text()"/>
					<xsl:variable name="reason" select="$QHAddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ReissueCertificateReason']/s0:Value/text()"/>
					<xsl:element name="certificateNumber" namespace="{$namespace}">
						<xsl:value-of select="$certificateNumber"/>
					</xsl:element>
					<xsl:element name="reason" namespace="{$namespace}">
						<xsl:value-of select="$reason"/>
					</xsl:element>
				</xsl:element>
			</xsl:when>
			<xsl:when test="($MstType='CANREX')">
				<xsl:element name="{$rootName}" namespace="{$namespace}">
					<xsl:element name="identification" namespace="{$namespace}">
						<ns1:rexNumber>
							<xsl:value-of select="$rexNumber"/>
						</ns1:rexNumber>
						<ns2:lastAmendDateTime>
							<xsl:value-of select="$lastAmendDateTime"/>
						</ns2:lastAmendDateTime>
					</xsl:element>
					<xsl:variable name="cancelReason" select="s0:NoteCollection/s0:Note[s0:Description/text()='NEXDOC Cancellation Reason']/s0:NoteText/text()" />
					<xsl:element name="reason" namespace="{$namespace}">
						<xsl:value-of select="$cancelReason"/>
					</xsl:element>
				</xsl:element>
			</xsl:when>
			<xsl:when test="$MstType='TRFEDN'">
				<xsl:element name="{$rootName}" namespace="{$namespace}">
					<xsl:element name="transferEDNFrom" namespace="{$namespace}">
						<ns1:rexNumber>
							<xsl:value-of select="$rexNumber"/>
						</ns1:rexNumber>
						<ns2:lastAmendDateTime>
							<xsl:value-of select="$lastAmendDateTime"/>
						</ns2:lastAmendDateTime>
					</xsl:element>

					<xsl:variable name="trfREX" select="s0:NoteCollection/s0:Note[s0:Description/text()='EXDOC trf REX']/s0:NoteText/text()" />
					<xsl:variable name="trfTime" select="s0:NoteCollection/s0:Note[s0:Description/text()='EXDOC trf LastAmendTime']/s0:NoteText/text()" />

					<xsl:element name="transferEDNTo" namespace="{$namespace}">
						<ns1:rexNumber>
							<xsl:value-of select="$trfREX"/>
						</ns1:rexNumber>
						<ns2:lastAmendDateTime>
							<xsl:value-of select="$trfTime"/>
						</ns2:lastAmendDateTime>
					</xsl:element>
				</xsl:element>
			</xsl:when>
			<xsl:when test="$MstType='REPLACE'">
				<xsl:element name="{$rootName}" namespace="{$namespace}">
					<xsl:variable name="replaceReason" select="s0:NoteCollection/s0:Note[s0:Description/text()='EXDOC Amendment Reason']/s0:NoteText/text()" />
					<xsl:element name="reason" namespace="{$namespace}">
						<xsl:value-of select="$replaceReason"/>
					</xsl:element>
					<xsl:element name="rexDetails" namespace="{$namespace}">
						<xsl:choose>
							<xsl:when test="$UseNEXDOCStaging">
								<xsl:call-template name="StagingMAINBody">
									<xsl:with-param name="rootName" select="$rootName"/>
									<xsl:with-param name="namespace" select="$namespace"/>
									<xsl:with-param name="rexNumber" select="$rexNumber"/>
									<xsl:with-param name="lastAmendDateTime" select="$lastAmendDateTime"/>
									<xsl:with-param name="QHAddInfoGroup" select="$QHAddInfoGroup"/>
								</xsl:call-template>
							</xsl:when>
							<xsl:otherwise>
								<xsl:call-template name="PRODMAINBody">
									<xsl:with-param name="rootName" select="$rootName"/>
									<xsl:with-param name="namespace" select="$namespace"/>
									<xsl:with-param name="rexNumber" select="$rexNumber"/>
									<xsl:with-param name="lastAmendDateTime" select="$lastAmendDateTime"/>
									<xsl:with-param name="QHAddInfoGroup" select="$QHAddInfoGroup"/>
								</xsl:call-template>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:element>
				</xsl:element>
			</xsl:when>
			<xsl:when test="$MstType='PREVIEW'">
				<xsl:element name="{$rootName}" namespace="{$namespace}">
					<xsl:element name="identification" namespace="{$namespace}">
						<ns1:rexNumber>
							<xsl:value-of select="$rexNumber"/>
						</ns1:rexNumber>
					</xsl:element>
				</xsl:element>
			</xsl:when>
			<xsl:when test="$MstType='READREX'">
				<xsl:element name="{$rootName}" namespace="{$namespace}">
					<xsl:element name="identification" namespace="{$namespace}">
						<ns1:rexNumber>
							<xsl:value-of select="$rexNumber"/>
						</ns1:rexNumber>
					</xsl:element>
				</xsl:element>
			</xsl:when>
			<xsl:otherwise>
				<xsl:element name="{$rootName}" namespace="{$namespace}">
					<xsl:choose>
						<xsl:when test="$UseNEXDOCStaging">
							<xsl:call-template name="StagingMAINBody">
								<xsl:with-param name="rootName" select="$rootName"/>
								<xsl:with-param name="namespace" select="$namespace"/>
								<xsl:with-param name="rexNumber" select="$rexNumber"/>
								<xsl:with-param name="lastAmendDateTime" select="$lastAmendDateTime"/>
								<xsl:with-param name="QHAddInfoGroup" select="$QHAddInfoGroup"/>
							</xsl:call-template>
						</xsl:when>
						<xsl:otherwise>
							<xsl:call-template name="PRODMAINBody">
								<xsl:with-param name="rootName" select="$rootName"/>
								<xsl:with-param name="namespace" select="$namespace"/>
								<xsl:with-param name="rexNumber" select="$rexNumber"/>
								<xsl:with-param name="lastAmendDateTime" select="$lastAmendDateTime"/>
								<xsl:with-param name="QHAddInfoGroup" select="$QHAddInfoGroup"/>
							</xsl:call-template>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:element>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="PRODMAINBody">
		<xsl:param name="rootName"/>
		<xsl:param name="namespace"/>
		<xsl:param name="rexNumber"/>
		<xsl:param name="lastAmendDateTime"/>
		<xsl:param name="QHAddInfoGroup"/>
		<xsl:variable name="Attachments" select="/s0:UniversalShipment/s0:Shipment/s0:AttachedDocumentCollection"/>
		<xsl:variable name="QLAddInfoGroup" select="s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice/s0:CommercialInvoiceLineCollection/s0:CommercialInvoiceLine/s0:AddInfoGroupCollection/s0:AddInfoGroup[s0:Type/s0:Code/text()='QL']"/>
		<xsl:variable name="invoiceCurrency" select="s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice/s0:InvoiceCurrency/s0:Code/text()"/>
		<xsl:variable name="commodityType" select="key('QuarantineExDocHeaderAddInfo','ProduceType')/s0:Value" />
		<xsl:variable name="productUse" select="key('QuarantineExDocHeaderAddInfo','ProductUse')/s0:Value" />
		<xsl:variable name="customsAgentIndicator">
			<xsl:choose>
				<xsl:when test="$QHAddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ObtainExportCustomsPermit']/s0:Value/text()='Y'">Y</xsl:when>
				<xsl:otherwise>N</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:element name="identification" namespace="{$namespace}">
			<ns1:rexNumber>
				<xsl:value-of select="$rexNumber"/>
			</ns1:rexNumber>
			<ns2:lastAmendDateTime>
				<xsl:value-of select="$lastAmendDateTime"/>
			</ns2:lastAmendDateTime>
		</xsl:element>
		<xsl:element name="exportDetails" namespace="{$namespace}">
			<xsl:variable name="transportModeOrigin" select="translate(s0:TransportMode/s0:Code/text(), $smallcase, $uppercase)" />
			<xsl:variable name="transportMode">
				<xsl:choose>
					<xsl:when test="$transportModeOrigin = 'AIR'">A</xsl:when>
					<xsl:when test="$transportModeOrigin = 'SEA'">S</xsl:when>
					<xsl:otherwise>M</xsl:otherwise>
				</xsl:choose>
			</xsl:variable>
			<xsl:choose>
				<xsl:when test="$commodityType='OTH'">
					<ns2:commodityType>X</ns2:commodityType>
				</xsl:when>
				<xsl:otherwise>
					<ns2:commodityType>
						<xsl:value-of select="substring($commodityType, 1, 1)"/>
					</ns2:commodityType>
				</xsl:otherwise>
			</xsl:choose>
			<ns2:priority>
				<xsl:choose>
					<xsl:when test="$transportMode = 'A'">1</xsl:when>
					<xsl:otherwise>9</xsl:otherwise>
				</xsl:choose>
			</ns2:priority>
			<ns2:departureDate>
				<xsl:value-of select="ScriptNS3:ConvertToDate($DepartureDate,'yyyy-MM-dd')"/>
			</ns2:departureDate>
			<ns2:transportDetails>
				<xsl:if test="$IsEU">
					<xsl:variable name="AQISEUPlaceOfDestination" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='AQISEUPlaceOfDestination']" />
					<ns2:euPlaceOfDestinationDetail>
						<ns2:name>
							<xsl:value-of select="$AQISEUPlaceOfDestination/s0:CompanyName/text()"/>
						</ns2:name>
						<ns2:address>
							<ns1:streetAddress>
								<ns1:streetLine>
									<xsl:value-of select="$AQISEUPlaceOfDestination/s0:Address1/text()"/>
								</ns1:streetLine>
								<ns1:streetLine>
									<xsl:value-of select="$AQISEUPlaceOfDestination/s0:Address2/text()"/>
								</ns1:streetLine>
							</ns1:streetAddress>
							<ns1:city>
								<xsl:value-of select="$AQISEUPlaceOfDestination/s0:City/text()"/>
							</ns1:city>
							<ns1:state>
								<xsl:value-of select="$AQISEUPlaceOfDestination/s0:State/text()"/>
							</ns1:state>
							<ns1:country>
								<xsl:value-of select="$AQISEUPlaceOfDestination/s0:Country/s0:Code/text()"/>
							</ns1:country>
							<ns1:postalCode>
								<xsl:value-of select="$AQISEUPlaceOfDestination/s0:Postcode/text()"/>
							</ns1:postalCode>
						</ns2:address>
						<ns2:approvalNumber>
							<xsl:value-of select="$AQISEUPlaceOfDestination/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text()='ETI' and s0:CountryOfIssue/s0:Code/text()='AU']/s0:Value/text()"/>
						</ns2:approvalNumber>
					</ns2:euPlaceOfDestinationDetail>
				</xsl:if>
				<ns2:transportMode>
					<xsl:value-of select="$transportMode"/>
				</ns2:transportMode>
				<ns2:voyageOrFlightNumber>
					<xsl:variable name="voyageOrFlightNumber" select="s0:VoyageFlightNo/text()" />
					<xsl:if test="not($voyageOrFlightNumber!='')">
						<xsl:attribute name="xsi:nil" namespace="http://www.w3.org/2001/XMLSchema-instance">true</xsl:attribute>
					</xsl:if>
					<xsl:value-of select="$voyageOrFlightNumber"/>
				</ns2:voyageOrFlightNumber>
				<ns2:vesselName>
					<xsl:variable name="vesselName" select="s0:VesselName/text()"/>
					<xsl:if test="not($vesselName!='')">
						<xsl:attribute name="xsi:nil" namespace="http://www.w3.org/2001/XMLSchema-instance">true</xsl:attribute>
					</xsl:if>
					<xsl:value-of select="$vesselName"/>
				</ns2:vesselName>
				<xsl:variable name="companyName" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='ShippingLine']/s0:CompanyName/text()" />
				<xsl:if test="$companyName != ''">
					<ns2:shippingCompany>
						<xsl:value-of select="$companyName"/>
					</ns2:shippingCompany>
				</xsl:if>
				<xsl:if test="not($companyName!='')">
					<ns2:shippingCompany>
						<xsl:attribute name="xsi:nil" namespace="http://www.w3.org/2001/XMLSchema-instance">true</xsl:attribute>
					</ns2:shippingCompany>
				</xsl:if>
				<xsl:choose>
					<xsl:when test="$QHAddInfoGroup">
						<xsl:variable name="temperatureUnit" select="key('QuarantineExDocHeaderAddInfo','TemperatureUnit')/s0:Value" />
						<xsl:variable name="storeTransportTemperature" select="userCSharp:If($commodityType = 'FSH', key('QuarantineExDocHeaderAddInfo','MaximumTemperature')/s0:Value, key('QuarantineExDocHeaderAddInfo','AbsoluteTemperature')/s0:Value)"/>
						<xsl:choose>
							<xsl:when test="not($storeTransportTemperature!='')">
								<ns2:storeTransportTemperature>
									<xsl:attribute name="xsi:nil" namespace="http://www.w3.org/2001/XMLSchema-instance">true</xsl:attribute>
								</ns2:storeTransportTemperature>
							</xsl:when>
							<xsl:otherwise>
								<ns2:storeTransportTemperature unit="{$temperatureUnit}">
									<xsl:value-of select="$storeTransportTemperature"/>
								</ns2:storeTransportTemperature>
							</xsl:otherwise>
						</xsl:choose>
						<xsl:variable name="sealStartNumber" select="key('QuarantineExDocHeaderAddInfo','StartHoldSeal')/s0:Value" />
						<xsl:variable name="sealEndNumber" select="key('QuarantineExDocHeaderAddInfo','EndHoldSeal')/s0:Value" />
						<xsl:choose>
							<xsl:when test="not($sealStartNumber!='') and not($sealEndNumber!='')">
								<ns2:vesselHoldSeals>
									<xsl:attribute name="xsi:nil" namespace="http://www.w3.org/2001/XMLSchema-instance">true</xsl:attribute>
								</ns2:vesselHoldSeals>
							</xsl:when>
							<xsl:otherwise>
								<ns2:vesselHoldSeals>
									<ns2:sealStartNumber>
										<xsl:value-of select="$sealStartNumber"/>
									</ns2:sealStartNumber>
									<ns2:sealEndNumber>
										<xsl:value-of select="$sealEndNumber"/>
									</ns2:sealEndNumber>
								</ns2:vesselHoldSeals>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:when>
					<xsl:otherwise>
						<ns2:storeTransportTemperature>
							<xsl:attribute name="xsi:nil" namespace="http://www.w3.org/2001/XMLSchema-instance">true</xsl:attribute>
						</ns2:storeTransportTemperature>
						<ns2:vesselHoldSeals>
							<xsl:attribute name="xsi:nil" namespace="http://www.w3.org/2001/XMLSchema-instance">true</xsl:attribute>
						</ns2:vesselHoldSeals>
					</xsl:otherwise>
				</xsl:choose>
			</ns2:transportDetails>
			<ns2:destinationCity>
				<xsl:variable name="destinationCity" select="s0:PortOfDestination/s0:Name/text()" />
				<xsl:if test="not($destinationCity!='')">
					<xsl:attribute name="xsi:nil" namespace="http://www.w3.org/2001/XMLSchema-instance">true</xsl:attribute>
				</xsl:if>
				<xsl:value-of select="substring($destinationCity, 1, 25)"/>
			</ns2:destinationCity>
			<xsl:variable name="dischargePort" select="s0:PortOfDischarge/s0:Code/text()" />
			<xsl:choose>
				<xsl:when test="not($dischargePort!='')">
					<ns2:dischargePorts>
						<ns2:removeExistingSet>true</ns2:removeExistingSet>
					</ns2:dischargePorts>
				</xsl:when>
				<xsl:otherwise>
					<ns2:dischargePorts>
						<ns2:dischargePort>
							<xsl:value-of select="$dischargePort"/>
						</ns2:dischargePort>
					</ns2:dischargePorts>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:variable name="destinationCountryCode" select="substring(s0:PortOfDestination/s0:Code/text(), 1, 2)" />
			<ns2:destinationCountry>
				<xsl:value-of select="$destinationCountryCode"/>
			</ns2:destinationCountry>
			<ns2:transitCountries>
				<xsl:variable name="countryCodes" select="s0:TransportLegCollection/s0:TransportLeg/s0:PortOfDischarge[substring(s0:Code/text(),1,2) != $destinationCountryCode]" />
				<xsl:choose>
					<xsl:when test="not($countryCodes!='')">
						<ns2:removeExistingSet>true</ns2:removeExistingSet>
					</xsl:when>
					<xsl:otherwise>
						<xsl:for-each select="$countryCodes">
							<xsl:variable name="countryCode" select="substring(self::node()/s0:Code/text(),1,2)" />
							<xsl:variable name="nextCountryCode" select="substring(../following-sibling::*[1]/s0:PortOfDischarge[s0:Code/text()],1,2)" />
							<xsl:if test="$countryCode != $nextCountryCode">
								<ns2:transitCountry>
									<xsl:value-of select="$countryCode"/>
								</ns2:transitCountry>
							</xsl:if>
						</xsl:for-each>
					</xsl:otherwise>
				</xsl:choose>
			</ns2:transitCountries>
			<ns2:borderInspectionPort>
				<xsl:variable name="borderInspectionPort" select="key('QuarantineExDocHeaderAddInfo','BorderInspectionPort')/s0:Value" />
				<xsl:if test="not($borderInspectionPort!='')">
					<xsl:attribute name="xsi:nil" namespace="http://www.w3.org/2001/XMLSchema-instance">true</xsl:attribute>
				</xsl:if>
				<xsl:value-of select="$borderInspectionPort"/>
			</ns2:borderInspectionPort>
			<xsl:variable name="loadingPort" select="s0:PortOfLoading/s0:Code/text()" />
			<xsl:choose>
				<xsl:when test="not($loadingPort!='')">
					<ns2:loadingPorts>
						<ns2:removeExistingSet>true</ns2:removeExistingSet>
					</ns2:loadingPorts>
				</xsl:when>
				<xsl:otherwise>
					<ns2:loadingPorts>
						<ns2:loadingPort>
							<xsl:value-of select="$loadingPort"/>
						</ns2:loadingPort>
					</ns2:loadingPorts>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:variable name="importPermits" select="s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice/s0:CommercialInvoiceLineCollection/s0:CommercialInvoiceLine/s0:AddInfoCollection[s0:AddInfo[s0:Key/text()='TemporaryImportNumbers_Hidden']/s0:Value/text()!='' and s0:AddInfo[s0:Key/text()='TemporaryImportDate_Hidden']/s0:Value/text()!='']"/>
			<xsl:choose>
				<xsl:when test="not($importPermits)">
					<ns2:importPermits>
						<ns2:removeExistingSet>true</ns2:removeExistingSet>
					</ns2:importPermits>
				</xsl:when>
				<xsl:otherwise>
					<ns2:importPermits>
						<xsl:for-each select="$importPermits[count(.|key('importPermits-group-by-number-and-date', concat(s0:AddInfo[s0:Key/text()='TemporaryImportNumbers_Hidden']/s0:Value/text(), s0:AddInfo[s0:Key/text()='TemporaryImportDate_Hidden']/s0:Value/text()))[1])=1]">
							<xsl:variable name="importPermitNumber" select="s0:AddInfo[s0:Key/text()='TemporaryImportNumbers_Hidden']/s0:Value/text()"/>
							<xsl:variable name="importPermitDateStr" select="s0:AddInfo[s0:Key/text()='TemporaryImportDate_Hidden']/s0:Value/text()"/>
							<ns2:importPermit>
								<ns2:importPermitNumber>
									<xsl:value-of select="$importPermitNumber"/>
								</ns2:importPermitNumber>
								<ns2:importPermitDate>
									<xsl:value-of select="ScriptNS3:ConvertToDate($importPermitDateStr,'yyyy-MM-dd')"/>
								</ns2:importPermitDate>
							</ns2:importPermit>
						</xsl:for-each>
					</ns2:importPermits>
				</xsl:otherwise>
			</xsl:choose>
			<ns2:sew>
				<ns2:customsAgentIndicator>
					<xsl:value-of select="$customsAgentIndicator"/>
				</ns2:customsAgentIndicator>
				<xsl:if test="$customsAgentIndicator='N'">
					<ns2:edn>
						<xsl:value-of select="s0:EntryNumberCollection/s0:EntryNumber[s0:Type/s0:Code/text()='CAN']/s0:Number/text()"/>
					</ns2:edn>
				</xsl:if>
				<ns2:currency>
					<xsl:value-of select="$invoiceCurrency"/>
				</ns2:currency>
				<ns2:consigneeName>
					<xsl:value-of select="key('QuarantineExDocHeaderAddInfo','CustomsConsigneeName')/s0:Value"/>
				</ns2:consigneeName>
			</ns2:sew>
			<ns2:ownerExporterId>
				<xsl:value-of select="s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice/s0:Supplier/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text()='NEN' and s0:CountryOfIssue/s0:Code/text()='AU']/s0:Value/text()"/>
			</ns2:ownerExporterId>
			<xsl:variable name="exporterDeclaration" select="key('QuarantineExDocHeaderAddInfo','ExporterDeclaration')/s0:Value"/>
			<xsl:variable name="exporterDeclarationCode" select="$QHAddInfoGroup/s0:CustomsReferenceCollection/s0:CustomsReference[s0:Type/s0:Code/text()='DEC' and s0:Reference/text()!='CDD03' and s0:Reference/text()!='CDD04' and s0:Reference/text()!='CDD05']"/>
			<ns2:exporterDeclaration>
				<ns2:exporterDeclaration>
					<xsl:value-of select="$exporterDeclaration"/>
				</ns2:exporterDeclaration>
				<xsl:choose>
					<xsl:when test="not($exporterDeclarationCode)">
						<ns2:removeExistingSet>true</ns2:removeExistingSet>
					</xsl:when>
					<xsl:otherwise>
						<xsl:for-each select="$exporterDeclarationCode">
							<ns2:exporterDeclarationCode>
								<xsl:value-of select="s0:Reference/text()"/>
							</ns2:exporterDeclarationCode>
						</xsl:for-each>
					</xsl:otherwise>
				</xsl:choose>
			</ns2:exporterDeclaration>
			<xsl:if test="$commodityType = 'DAI'">
				<ns2:imaDetailsList>
					<xsl:choose>
						<xsl:when test="not($QLAddInfoGroup)">
							<ns2:removeExistingSet>true</ns2:removeExistingSet>
						</xsl:when>
						<xsl:otherwise>
							<xsl:for-each select="$QLAddInfoGroup">
								<xsl:variable name="InvoiceLine" select="../.."/>
								<xsl:variable name="CommercialInvoice" select="$InvoiceLine/../.."/>
								<xsl:variable name="serialNumber" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='IMA1SerialNumber']/s0:Value/text()"/>
								<xsl:if test="$serialNumber">
									<ns2:imaDetails>
										<ns2:serialNumber>
											<xsl:value-of select="$serialNumber"/>
										</ns2:serialNumber>
										<xsl:choose>
											<xsl:when test="$transportMode = 'S'">
												<xsl:variable name="ContainerNumber" select="/s0:UniversalShipment/s0:Shipment/s0:PackingLineCollection/s0:PackingLine[s0:PackedItemCollection/s0:PackedItem/s0:CommercialInvoiceLineLink=$InvoiceLine/s0:Link][1]/s0:ContainerNumber" />
												<xsl:if test="$ContainerNumber">
													<xsl:variable name="RelatedContainer" select="/s0:UniversalShipment/s0:Shipment/s0:ContainerCollection/s0:Container[s0:ContainerNumber=$ContainerNumber][1]" />
													<xsl:if test="$RelatedContainer">
														<ns2:containerNumber>
															<xsl:value-of select="$ContainerNumber"/>
														</ns2:containerNumber>
													</xsl:if>
												</xsl:if>
											</xsl:when>
											<xsl:otherwise>
												<ns2:containerNumber>AIRFREIGHT</ns2:containerNumber>
											</xsl:otherwise>
										</xsl:choose>
										<ns2:invoiceDate>
											<xsl:value-of select="ScriptNS3:ConvertToDate($CommercialInvoice/s0:InvoiceDate/text(), 'yyyy-MM-dd')"/>
										</ns2:invoiceDate>
										<ns2:invoiceNumber>
											<xsl:value-of select="$CommercialInvoice/s0:InvoiceNumber/text()"/>
										</ns2:invoiceNumber>
										<ns2:grossWeight>
											<xsl:attribute name="unit">
												<xsl:value-of select="userCSharp:GetMappedWeightUnit($InvoiceLine/s0:WeightUnit/s0:Code/text())"/>
											</xsl:attribute>
											<xsl:value-of select="userCSharp:GetMappedWeight($InvoiceLine/s0:Weight/text(), $InvoiceLine/s0:WeightUnit/s0:Code/text())"/>
										</ns2:grossWeight>
										<ns2:netWeight>
											<xsl:attribute name="unit">
												<xsl:value-of select="userCSharp:GetMappedWeightUnit($InvoiceLine/s0:NetWeightUnit/s0:Code/text())"/>
											</xsl:attribute>
											<xsl:value-of select="userCSharp:GetMappedWeight($InvoiceLine/s0:NetWeight/text(), $InvoiceLine/s0:NetWeightUnit/s0:Code/text())"/>
										</ns2:netWeight>
										<ns2:productDescription>
											<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='IMA1ProductDescription']/s0:Value/text()"/>
										</ns2:productDescription>
										<ns2:quotaYear>
											<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='IMA1QuotaYear']/s0:Value/text()"/>
										</ns2:quotaYear>
									</ns2:imaDetails>
								</xsl:if>
							</xsl:for-each>
						</xsl:otherwise>
					</xsl:choose>
				</ns2:imaDetailsList>
			</xsl:if>
			<ns2:exporterReference>
				<xsl:if test="not($CalculatedExporterReference!='')">
					<xsl:attribute name="xsi:nil" namespace="http://www.w3.org/2001/XMLSchema-instance">true</xsl:attribute>
				</xsl:if>
				<xsl:value-of select="$CalculatedExporterReference"/>
			</ns2:exporterReference>
			<xsl:variable name="importerDocumentaryAddress" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='ImporterDocumentaryAddress' or s0:AddressType/text()='Importer']"/>
			<ns2:consigneeDetails>
				<ns1:consigneeName>
					<xsl:value-of select="$importerDocumentaryAddress/s0:CompanyName/text()"/>
				</ns1:consigneeName>
				<ns1:consigneeAddress>
					<ns1:streetAddress>
						<xsl:variable name="address1" select="$importerDocumentaryAddress/s0:Address1/text()" />
						<xsl:variable name="address2" select="$importerDocumentaryAddress/s0:Address2/text()" />
						<xsl:if test="$address1 != ''">
							<ns1:streetLine>
								<xsl:value-of select="substring($address1, 1, 35)"/>
							</ns1:streetLine>
						</xsl:if>
						<xsl:if test="$address2 != ''">
							<ns1:streetLine>
								<xsl:value-of select="substring($address2, 1, 35)"/>
							</ns1:streetLine>
						</xsl:if>
					</ns1:streetAddress>
					<ns1:city>
						<xsl:choose>
							<xsl:when test="$commodityType = 'DAI'">
								<xsl:value-of select="substring($importerDocumentaryAddress/s0:City/text(), 1, 25)"/>
							</xsl:when>
							<xsl:otherwise>
								<xsl:value-of select="$importerDocumentaryAddress/s0:City/text()"/>
							</xsl:otherwise>
						</xsl:choose>
					</ns1:city>
					<ns1:state>
						<xsl:choose>
							<xsl:when test="$importerDocumentaryAddress/s0:State/@Description != ''">
								<xsl:value-of select="substring($importerDocumentaryAddress/s0:State/@Description, 1, 20)"/>
							</xsl:when>
							<xsl:otherwise>
								<xsl:value-of select="substring($importerDocumentaryAddress/s0:State/text(), 1, 20)"/>
							</xsl:otherwise>
						</xsl:choose>
					</ns1:state>
					<ns1:country>
						<xsl:value-of select="$importerDocumentaryAddress/s0:Country/s0:Code/text()"/>
					</ns1:country>
					<ns1:postalCode>
						<xsl:value-of select="$importerDocumentaryAddress/s0:Postcode/text()"/>
					</ns1:postalCode>
				</ns1:consigneeAddress>
				<xsl:variable name="consigneePhoneNumber" select="$importerDocumentaryAddress/s0:Phone/text()"/>
				<xsl:if test="$consigneePhoneNumber != ''">
					<ns1:consigneePhoneNumber>
						<xsl:value-of select="$consigneePhoneNumber"/>
					</ns1:consigneePhoneNumber>
				</xsl:if>
				<xsl:if test="$commodityType != 'DAI'">
					<ns1:consigneeRepresentative>
						<xsl:value-of select="$QHAddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ConsigneeAgentName']/s0:Value/text()"/>
					</ns1:consigneeRepresentative>
				</xsl:if>
			</ns2:consigneeDetails>
			<xsl:variable name="ackCustomsReferences" select="$QHAddInfoGroup/s0:CustomsReferenceCollection/s0:CustomsReference[s0:Type/s0:Code/text()='ACK' and s0:Reference/text()!='']"/>
			<xsl:if test="$ackCustomsReferences">
				<ns2:messageAcknowledgements>
					<xsl:for-each select="$ackCustomsReferences[count(.|key('ackCustomsReferences-group-by-Reference', s0:Reference/text())[1])=1]">
						<ns1:messageId>
							<xsl:value-of select="s0:Reference/text()"/>
						</ns1:messageId>
					</xsl:for-each>
				</ns2:messageAcknowledgements>
			</xsl:if>
			<ns2:tracesApprovalId>
				<xsl:value-of select="$importerDocumentaryAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text()='ETI']/s0:Value/text()"/>
			</ns2:tracesApprovalId>
			<xsl:if test="$productUse!=''">
				<ns2:productUseIndicator>
					<xsl:value-of select="$productUse"/>
				</ns2:productUseIndicator>
			</xsl:if>
			<ns2:importedProductFlag>
				<xsl:value-of select="substring($QHAddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ImportedProductFlag']/s0:Value/text(), 1, 1)"/>
			</ns2:importedProductFlag>
			<ns2:manufacturedTreatedPackagedLabelledInAustralia>
				<xsl:value-of select="substring($QHAddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ManufacturedTreatedPackagedLabelledInAustralia']/s0:Value/text(), 1, 1)"/>
			</ns2:manufacturedTreatedPackagedLabelledInAustralia>
			<ns2:legallyImported>
				<xsl:value-of select="substring($QHAddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='LegallyImportedFlag']/s0:Value/text(), 1, 1)"/>
			</ns2:legallyImported>
			<ns2:lotNumber>
				<xsl:value-of select="key('QuarantineExDocHeaderAddInfo','LotNumber')/s0:Value"/>
			</ns2:lotNumber>
			<ns2:storageEstablishmentNumber>
				<xsl:value-of select="key('QuarantineExDocHeaderAddInfo','StorageEstablishment')/s0:Value"/>
			</ns2:storageEstablishmentNumber>
			<ns2:quotaYear>
				<xsl:value-of select="key('QuarantineExDocHeaderAddInfo','AMLCQuotaYear')/s0:Value"/>
			</ns2:quotaYear>
			<ns2:quotaFlag></ns2:quotaFlag>
			<ns2:quotaType>
				<xsl:value-of select="key('QuarantineExDocHeaderAddInfo', 'QuotaType')/s0:Value"/>
			</ns2:quotaType>
			<ns2:shipStoresFlag>
				<xsl:value-of select="key('QuarantineExDocHeaderAddInfo','ShipsStores')/s0:Value"/>
			</ns2:shipStoresFlag>
			<ns2:exemptionCode>
				<xsl:value-of select="key('QuarantineExDocHeaderAddInfo','ExemptionCode')/s0:Value"/>
			</ns2:exemptionCode>
			<xsl:if test="$commodityType != 'WOL' and $commodityType != 'SKN'">
				<xsl:variable name="authorisationDate" select="key('QuarantineExDocHeaderAddInfo','AuthorisationDate')/s0:Value"/>
				<xsl:variable name="authorisingEstablishmentNumber" select="key('QuarantineExDocHeaderAddInfo','AuthorisationEstablishment')/s0:Value"/>
				<xsl:variable name="comments" select="key('QuarantineExDocHeaderAddInfo','AuthorisationComments')/s0:Value"/>
				<xsl:variable name="authorisationFlag" select="$QHAddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='AuthorisationFlag']/s0:Value"/>
				<xsl:choose>
					<xsl:when test="$authorisationFlag != ''">
						<ns2:authorisationFlag>
							<xsl:value-of select ="$authorisationFlag"/>
						</ns2:authorisationFlag>
					</xsl:when>
					<xsl:otherwise>
						<xsl:if test="$authorisationDate != '' or $authorisingEstablishmentNumber != '' or $comments != ''">
							<ns2:authorisationFlag>Y</ns2:authorisationFlag>
						</xsl:if>
					</xsl:otherwise>
				</xsl:choose>
				<ns2:authorisationDetails>
					<ns2:authorisationDate>
						<xsl:value-of select="$authorisationDate"/>
					</ns2:authorisationDate>
					<ns2:authorisingEstablishmentNumber>
						<xsl:value-of select="$authorisingEstablishmentNumber"/>
					</ns2:authorisingEstablishmentNumber>
					<ns2:comments>
						<xsl:value-of select="$comments"/>
					</ns2:comments>
				</ns2:authorisationDetails>
			</xsl:if>
			<xsl:variable name="EUContactPersonAddress" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='AQISEUContactPerson']"/>
			<xsl:if test="$EUContactPersonAddress">
				<ns2:euContactInformation>
					<ns2:contactName>
						<xsl:value-of select="$EUContactPersonAddress/s0:Contact"/>
					</ns2:contactName>
					<ns2:contactPhone>
						<xsl:value-of select="$EUContactPersonAddress/s0:Phone"/>
					</ns2:contactPhone>
					<ns2:comments>
						<xsl:value-of select="key('QuarantineExDocHeaderAddInfo','EUComments')/s0:Value"/>
					</ns2:comments>
					<ns2:testResultRequired>
						<xsl:value-of select="key('QuarantineExDocHeaderAddInfo','EUTestResultRequired')/s0:Value"/>
					</ns2:testResultRequired>
				</ns2:euContactInformation>
			</xsl:if>
			<xsl:if test="$commodityType = 'FSH'">
				<ns2:fishExportDetails xmlns:fis="http://agriculture.gov.au/nexdoc/common/rex/FishTypes_1.0">
					<fis:transportStorageMinimumTemperature unit="{key('QuarantineExDocHeaderAddInfo','TemperatureUnit')/s0:Value}">
						<xsl:value-of select="key('QuarantineExDocHeaderAddInfo','MinimumTemperature')/s0:Value"/>
					</fis:transportStorageMinimumTemperature>
					<xsl:variable name="catchingZones" select="$QHAddInfoGroup/s0:CustomsReferenceCollection/s0:CustomsReference[s0:Type/s0:Code/text()='OCZ' and s0:Reference/text()!='']"/>
					<xsl:if test="$catchingZones">
						<fis:catchingZones>
							<xsl:for-each select="$catchingZones">
								<fis:catchingZone>
									<xsl:value-of select="s0:Reference/text()"/>
								</fis:catchingZone>
							</xsl:for-each>
						</fis:catchingZones>
					</xsl:if>
				</ns2:fishExportDetails>
			</xsl:if>
			<xsl:if test="$commodityType = 'WOL'">
				<ns2:woolExportDetails xmlns:wool="http://agriculture.gov.au/nexdoc/common/rex/WoolTypes_1.0">
					<wool:packDate>
						<xsl:value-of select="ScriptNS3:ConvertToDate(key('QuarantineExDocHeaderAddInfo','PackDate')/s0:Value,'yyyy-MM-dd')"/>
					</wool:packDate>
				</ns2:woolExportDetails>
			</xsl:if>
			<xsl:if test="$commodityType = 'SKN'">
				<ns2:skinsAndHidesExportDetails xmlns:skn="http://agriculture.gov.au/nexdoc/common/rex/SkinsAndHidesTypes_1.0">
					<skn:loadingDate>
						<xsl:value-of select="ScriptNS3:ConvertToDate(key('QuarantineExDocHeaderAddInfo','LoadingDate')/s0:Value,'yyyy-MM-dd')"/>
					</skn:loadingDate>
					<skn:loadingEstablishment>
						<xsl:value-of select="key('QuarantineExDocHeaderAddInfo','LoadingEstablishment')/s0:Value"/>
					</skn:loadingEstablishment>
					<skn:packDate>
						<xsl:value-of select="ScriptNS3:ConvertToDate(key('QuarantineExDocHeaderAddInfo','PackDate')/s0:Value,'yyyy-MM-dd')"/>
					</skn:packDate>
				</ns2:skinsAndHidesExportDetails>
			</xsl:if>
			<xsl:if test="$commodityType = 'IME'">
				<xsl:variable name="temperatureUnit" select="key('QuarantineExDocHeaderAddInfo','TemperatureUnit')/s0:Value"></xsl:variable>
				<xsl:if test="$temperatureUnit != ''">
					<ns2:inedibleMeatExportDetails xmlns:ime="http://agriculture.gov.au/nexdoc/common/rex/InedibleMeatTypes_1.0">
						<ime:transportStorageMinimumTemperature unit="{$temperatureUnit}">
							<xsl:value-of select="key('QuarantineExDocHeaderAddInfo','MinimumTemperature')/s0:Value"></xsl:value-of>
						</ime:transportStorageMinimumTemperature>
					</ns2:inedibleMeatExportDetails>
				</xsl:if>
			</xsl:if>
		</xsl:element>
		<xsl:element name="certificateDetails" namespace="{$namespace}">
			<xsl:variable name="certificatePrintRegion" select="key('QuarantineExDocHeaderAddInfo','CertificateRequiredLocation')/s0:Value"/>
			<xsl:if test="$QHAddInfoGroup">
				<ns2:certificatePrintControls>
					<ns2:certificatePrintIndicator>
						<xsl:value-of select="key('QuarantineExDocHeaderAddInfo','CertificatePrintIndicator')/s0:Value"/>
					</ns2:certificatePrintIndicator>
					<xsl:variable name="printLocation" select="key('QuarantineExDocHeaderAddInfo','PrintLocation')/s0:Value"/>
					<xsl:choose>
						<xsl:when test="$printLocation='AQIS PLACE'">
							<ns2:certificatePrintRegion>
								<xsl:value-of select="$certificatePrintRegion"/>
							</ns2:certificatePrintRegion>
						</xsl:when>
						<xsl:otherwise>
							<xsl:if test="$printLocation='ORGANISATION' or $certificatePrintRegion!=''">
								<ns2:certificateRequiredClientGroup>
									<xsl:value-of select="$certificatePrintRegion"/>
								</ns2:certificateRequiredClientGroup>
							</xsl:if>
						</xsl:otherwise>
					</xsl:choose>
					<xsl:variable name="splitHealthCertByContainer" select="$QHAddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='SplitHealthCertByContainer']"/>
					<xsl:variable name="splitHealthCertByPacker" select="$QHAddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='SplitHealthCertByPacker']"/>
					<xsl:variable name="splitHealthCertByMarks" select="$QHAddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='SplitHealthCertByMarks']"/>
					<ns2:separateBy>
						<xsl:choose>
							<xsl:when test="$splitHealthCertByContainer">CONTAINER</xsl:when>
							<xsl:when test="$splitHealthCertByPacker">PACKING_ESTABLISHMENT</xsl:when>
							<xsl:when test="$splitHealthCertByMarks">SHIPPING_MARK</xsl:when>
						</xsl:choose>
					</ns2:separateBy>
				</ns2:certificatePrintControls>
			</xsl:if>
			<xsl:choose>
				<xsl:when test="not($QLAddInfoGroup and $QLAddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='FormatRequested' and s0:Value/text()!=''] and $certificatePrintRegion!='')">
					<ns2:certificates>
						<ns2:removeExistingSet>true</ns2:removeExistingSet>
					</ns2:certificates>
				</xsl:when>
				<xsl:otherwise>
					<ns2:certificates>
						<xsl:for-each select="$QLAddInfoGroup[count(.|key('certificateLines-group-by-template-and-endorsement',concat(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='FormatRequested']/s0:Value/text(), s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ExtraFormatRequested']/s0:Value/text()))[1])=1]">
							<xsl:variable name="current-grouping-key" select="concat(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='FormatRequested']/s0:Value/text(), s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ExtraFormatRequested']/s0:Value/text())"/>
							<xsl:variable name="current-group" select="key('certificateLines-group-by-template-and-endorsement', $current-grouping-key)"/>
							<xsl:variable name="certificateTemplate" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='FormatRequested']/s0:Value/text()" />
							<xsl:variable name="certificateEndorsement" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ExtraFormatRequested']/s0:Value/text()"/>
							<ns2:certificate>
								<ns2:lineNumbers>
									<xsl:for-each select="$current-group">
										<ns2:lineNumber>
											<xsl:variable name="InvoiceLine" select="../.."/>
											<xsl:value-of select="$InvoiceLine/s0:LineNo/text()"/>
										</ns2:lineNumber>
									</xsl:for-each>
								</ns2:lineNumbers>
								<ns2:certificateDetails>
									<ns2:certificateTemplate>
										<xsl:value-of select="$certificateTemplate"/>
									</ns2:certificateTemplate>
									<ns2:certificateEndorsement>
										<xsl:value-of select="$certificateEndorsement"/>
									</ns2:certificateEndorsement>
								</ns2:certificateDetails>
							</ns2:certificate>
						</xsl:for-each>
					</ns2:certificates>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:element>

		<xsl:element name="productLines" namespace="{$namespace}">
			<xsl:for-each select="$QLAddInfoGroup">
				<xsl:variable name="InvoiceLine" select="../.."/>
				<xsl:variable name="InvoiceLineLineNo" select="$InvoiceLine/s0:LineNo/text()"/>
				<ns2:productLine>
					<ns2:lineNumber>
						<xsl:value-of select="$InvoiceLineLineNo"/>
					</ns2:lineNumber>
					<ns2:productDetails>
						<ns2:productType>
							<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ProductType']/s0:Value/text()"/>
						</ns2:productType>
						<ns2:category>
							<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='Category']/s0:Value/text()"/>
						</ns2:category>
						<ns2:packType>
							<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='PackType']/s0:Value/text()"/>
						</ns2:packType>
						<ns2:preservationType>
							<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='PreservationType']/s0:Value/text()"/>
						</ns2:preservationType>
						<ns2:cutType>
							<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CutCode']/s0:Value/text()"/>
						</ns2:cutType>
						<ns2:suppCode>
							<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='SupplementaryCode']/s0:Value/text()"/>
						</ns2:suppCode>
						<ns2:outerProductPackaging>
							<xsl:variable name="outerPackType" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='OuterPackType']/s0:Value/text()"/>
							<ns2:quantity packageType="{$outerPackType}">
								<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='OuterPackCount']/s0:Value/text()"/>
							</ns2:quantity>
							<xsl:variable name="outerPackWeightUnit" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='OuterPackWeightUnit']/s0:Value/text()"/>
							<ns2:unitAmount unit="{$outerPackWeightUnit}">
								<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='OuterPackWeight']/s0:Value/text()"/>
							</ns2:unitAmount>
							<xsl:variable name="packageMeasureAccuracy" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='OuterPackAccuracy']/s0:Value/text()" />
							<ns2:packageMeasureAccuracy>
								<xsl:choose>
									<xsl:when test="$packageMeasureAccuracy = '3'">A</xsl:when>
									<xsl:when test="$packageMeasureAccuracy = '4'">E</xsl:when>
								</xsl:choose>
							</ns2:packageMeasureAccuracy>
							<ns2:shippingMarks>
								<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ShippingMarks']/s0:Value/text()"/>
							</ns2:shippingMarks>
						</ns2:outerProductPackaging>
						<ns2:intermediateProductPackaging>
							<xsl:variable name="intermediatePackType" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='IntermediatePackType']/s0:Value/text()"/>
							<ns2:quantity packageType="{$intermediatePackType}">
								<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='IntermediatePackCount']/s0:Value/text()"/>
							</ns2:quantity>
							<xsl:variable name="intermediatePackWeightUnit" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='IntermediatePackWeightUnit']/s0:Value/text()"/>
							<ns2:unitAmount unit="{$intermediatePackWeightUnit}">
								<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='IntermediatePackWeight']/s0:Value/text()"/>
							</ns2:unitAmount>
							<xsl:variable name="packageMeasureAccuracy" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='IntermediatePackAccuracy']/s0:Value/text()" />
							<ns2:packageMeasureAccuracy>
								<xsl:choose>
									<xsl:when test="$packageMeasureAccuracy = '3'">A</xsl:when>
									<xsl:when test="$packageMeasureAccuracy = '4'">E</xsl:when>
								</xsl:choose>
							</ns2:packageMeasureAccuracy>
						</ns2:intermediateProductPackaging>
						<ns2:innerProductPackaging>
							<xsl:variable name="innerPackType" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='InnerPackType']/s0:Value/text()"/>
							<ns2:quantity packageType="{$innerPackType}">
								<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='InnerPackCount']/s0:Value/text()"/>
							</ns2:quantity>
							<xsl:variable name="innerPackWeightUnit" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='InnerPackWeightUnit']/s0:Value/text()"/>
							<ns2:unitAmount unit="{$innerPackWeightUnit}">
								<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='InnerPackWeight']/s0:Value/text()"/>
							</ns2:unitAmount>
							<xsl:variable name="packageMeasureAccuracy" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='InnerPackAccuracy']/s0:Value/text()" />
							<ns2:packageMeasureAccuracy>
								<xsl:choose>
									<xsl:when test="$packageMeasureAccuracy = '3'">A</xsl:when>
									<xsl:when test="$packageMeasureAccuracy = '4'">E</xsl:when>
								</xsl:choose>
							</ns2:packageMeasureAccuracy>
						</ns2:innerProductPackaging>
						<xsl:variable name="netQuantityUnit" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='NetQuantityUnit']/s0:Value/text()"/>
						<ns2:netMetricWeight unit="{$netQuantityUnit}">
							<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='NetQuantity']/s0:Value/text()"/>
						</ns2:netMetricWeight>
						<xsl:variable name="imperialNetWeightUnit" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ImperialNetWeightUnit']/s0:Value/text()"/>
						<ns2:netImperialWeight unit="{$imperialNetWeightUnit}">
							<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ImperialNetWeight']/s0:Value/text()"/>
						</ns2:netImperialWeight>
						<ns2:manualCertificateProductDescription>
							<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='InspectionDescription']/s0:Value/text()"/>
						</ns2:manualCertificateProductDescription>
						<ns2:additionalDescription>
							<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='AdditionalProductDescription']/s0:Value/text()"/>
						</ns2:additionalDescription>
						<ns2:farmCode>
							<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='FarmCode']/s0:Value/text()"/>
						</ns2:farmCode>
						<ns2:farmType>
							<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='FarmType']/s0:Value/text()"/>
						</ns2:farmType>
					</ns2:productDetails>

					<xsl:variable name="InvoiceLineLink" select="$InvoiceLine/s0:Link/text()"/>
					<xsl:variable name="RelatedPackingLines" select="/s0:UniversalShipment/s0:Shipment/s0:PackingLineCollection/s0:PackingLine[s0:PackedItemCollection/s0:PackedItem/s0:CommercialInvoiceLineLink=$InvoiceLineLink and s0:ContainerNumber/text()!='']" />
					<xsl:choose>
						<xsl:when test="not($RelatedPackingLines)">
							<ns2:containers>
								<ns2:removeExistingSet>true</ns2:removeExistingSet>
							</ns2:containers>
						</xsl:when>
						<xsl:otherwise>
							<ns2:containers>
								<xsl:for-each select="$RelatedPackingLines">
									<xsl:variable name="ContainerNumber" select="s0:ContainerNumber/text()" />
									<ns2:container>
										<ns2:containerNumber>
											<xsl:value-of select="$ContainerNumber"/>
										</ns2:containerNumber>
										<xsl:variable name="RelatedContainer" select="/s0:UniversalShipment/s0:Shipment/s0:ContainerCollection/s0:Container[s0:ContainerNumber=$ContainerNumber][1]" />
										<xsl:if test="$RelatedContainer">
											<xsl:variable name="SealNumber" select="$RelatedContainer/s0:Seal/text()"/>
											<xsl:variable name="SealStartNumber" select="$RelatedContainer/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='AQISSealStart_Hidden']/s0:Value/text()"/>
											<xsl:variable name="SealEndNumber" select="$RelatedContainer/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='AQISSealEnd_Hidden']/s0:Value/text()"/>
											<xsl:if test="$SealNumber!='' or $SealStartNumber!='' or $SealEndNumber!=''">
												<ns2:containerSeals>
													<ns2:containerSeal>
														<xsl:if test="$SealNumber!=''">
															<ns2:sealNumber>
																<xsl:value-of select="$SealNumber"/>
															</ns2:sealNumber>
														</xsl:if>
														<xsl:if test="$SealStartNumber!=''">
															<ns2:sealStartNumber>
																<xsl:value-of select="$SealStartNumber"/>
															</ns2:sealStartNumber>
														</xsl:if>
														<xsl:if test="$SealEndNumber!=''">
															<ns2:sealEndNumber>
																<xsl:value-of select="$SealEndNumber"/>
															</ns2:sealEndNumber>
														</xsl:if>
													</ns2:containerSeal>
												</ns2:containerSeals>
											</xsl:if>
										</xsl:if>
									</ns2:container>
								</xsl:for-each>
							</ns2:containers>
						</xsl:otherwise>
					</xsl:choose>
					<xsl:variable name="NPDAddInfoGroup" select="s0:AddInfoGroupCollection/s0:AddInfoGroup[s0:Type/s0:Code/text()='NPD']" />
					<xsl:variable name="NPDAddInfoTRGroup" select="$NPDAddInfoGroup[s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TreatmentCode' or s0:Key/text()='TreatmentInformation']/s0:Value/text()!='']" />
					<xsl:variable name="NPDAddInfoHAGroup" select="$NPDAddInfoGroup[s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ProcessingType']/s0:Value/text()='HA']" />
					<xsl:variable name="NPDAddInfoEstablishmentGroup" select="$NPDAddInfoGroup[count(.|$NPDAddInfoTRGroup) != count($NPDAddInfoTRGroup) and count(.|$NPDAddInfoHAGroup) != count($NPDAddInfoHAGroup)]" />
					<xsl:variable name="NPDAddInfoFreeTextEstablishmentGroup" select="$NPDAddInfoEstablishmentGroup[not(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='EstablishmentID']/s0:Value/text()!='') and s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='EstablishmentIndicator']/s0:Value/text()!='']" />
					<xsl:variable name="NPDAddInfoRegisteredEstablishmentGroup" select="$NPDAddInfoEstablishmentGroup[count(.|$NPDAddInfoFreeTextEstablishmentGroup) != count($NPDAddInfoFreeTextEstablishmentGroup)]" />

					<xsl:choose>
						<xsl:when test="not($NPDAddInfoTRGroup)">
							<ns2:treatments>
								<ns2:removeExistingSet>true</ns2:removeExistingSet>
							</ns2:treatments>
						</xsl:when>
						<xsl:otherwise>
							<ns2:treatments>
								<xsl:for-each select="$NPDAddInfoTRGroup">
									<xsl:variable name="treatmentCode" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TreatmentCode']/s0:Value/text()"/>
									<xsl:variable name="treatmentInformation" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TreatmentInformation']/s0:Value/text()"/>
									<xsl:if test="$treatmentCode != '' or $treatmentInformation != ''">
										<ns2:treatmentType>
											<xsl:if test="$treatmentCode != ''">
												<ns2:treatmentCode>
													<xsl:value-of select="$treatmentCode"/>
												</ns2:treatmentCode>
												<ns2:treatmentStartDate>
													<xsl:value-of select="ScriptNS3:ConvertToDate(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='StartDate']/s0:Value/text(),'yyyy-MM-dd')"/>
												</ns2:treatmentStartDate>
												<ns2:treatmentEndDate>
													<xsl:value-of select="ScriptNS3:ConvertToDate(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='EndDate']/s0:Value/text(),'yyyy-MM-dd')"/>
												</ns2:treatmentEndDate>
											</xsl:if>
											<xsl:if test="$treatmentInformation != ''">
												<ns2:treatmentInformation>
													<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TreatmentInformation']/s0:Value/text()"/>
												</ns2:treatmentInformation>
											</xsl:if>
										</ns2:treatmentType>
									</xsl:if>
								</xsl:for-each>
							</ns2:treatments>
						</xsl:otherwise>
					</xsl:choose>
					<xsl:choose>
						<xsl:when test="not($NPDAddInfoRegisteredEstablishmentGroup)">
							<ns2:productionProcesses>
								<ns2:removeExistingSet>true</ns2:removeExistingSet>
							</ns2:productionProcesses>
						</xsl:when>
						<xsl:otherwise>
							<ns2:productionProcesses>
								<xsl:for-each select="$NPDAddInfoRegisteredEstablishmentGroup">
									<xsl:variable name="treatmentCode" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TreatmentCode']/s0:Value/text()"/>
									<xsl:variable name="treatmentInformation" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TreatmentInformation']/s0:Value/text()"/>
									<xsl:if test="not($treatmentCode != '') and not($treatmentInformation != '')">
										<ns2:productionProcess>
											<ns2:processingStartDate>
												<xsl:value-of select="ScriptNS3:ConvertToDate(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='StartDate']/s0:Value/text(),'yyyy-MM-dd')"/>
											</ns2:processingStartDate>
											<ns2:processingEndDate>
												<xsl:value-of select="ScriptNS3:ConvertToDate(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='EndDate']/s0:Value/text(),'yyyy-MM-dd')"/>
											</ns2:processingEndDate>
											<xsl:variable name="establishmentIndicator" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='EstablishmentIndicator']/s0:Value/text()"/>
											<ns2:establishmentIndicator>
												<xsl:choose>
													<xsl:when test="$establishmentIndicator != ''">
														<xsl:value-of select="$establishmentIndicator"/>
													</xsl:when>
													<xsl:otherwise>PC</xsl:otherwise>
												</xsl:choose>
											</ns2:establishmentIndicator>
											<xsl:variable name="processingEstablishmentNumber" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='EstablishmentID']/s0:Value/text()" />
											<xsl:if test="$processingEstablishmentNumber != ''">
												<ns2:processingEstablishmentNumber>
													<xsl:value-of select="$processingEstablishmentNumber"/>
												</ns2:processingEstablishmentNumber>
											</xsl:if>
											<ns2:removeEntry>
												<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='RemoveEntry']/s0:Value/text()"/>
											</ns2:removeEntry>
										</ns2:productionProcess>
									</xsl:if>
								</xsl:for-each>
							</ns2:productionProcesses>
						</xsl:otherwise>
					</xsl:choose>
					<ns2:sew>
						<xsl:variable name="AIQSCustomsWtUQ_Hidden" select="$InvoiceLine/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='AQISCustomsWtUQ_Hidden']/s0:Value/text()"/>
						<ns2:netCustomsWeight unit="{$AIQSCustomsWtUQ_Hidden}">
							<xsl:value-of select="$InvoiceLine/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='AQISCustomsWt_Hidden']/s0:Value/text()"/>
						</ns2:netCustomsWeight>
						<xsl:variable name="innerPackWeightUnit" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='GrossMetricWeightUnit']/s0:Value/text()"/>
						<ns2:grossMetricWeight unit="{$innerPackWeightUnit}">
							<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='GrossMetricWeight']/s0:Value/text()"/>
						</ns2:grossMetricWeight>
						<xsl:if test="$invoiceCurrency != ''">
							<ns2:fobAmount>
								<xsl:value-of select="userCSharp:GetRoundStr($InvoiceLine/s0:CustomsValue/text())"/>
							</ns2:fobAmount>
						</xsl:if>
						<ns2:productSourceState>
							<xsl:value-of select="$InvoiceLine/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='AUState_Hidden']/s0:Value/text()"/>
						</ns2:productSourceState>
						<ns2:relatedPermitType>
							<xsl:value-of select="$InvoiceLine/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='RelatedExportPermitAuthority_Hidden']/s0:Value/text()"/>
						</ns2:relatedPermitType>
						<ns2:relatedPermitNumber>
							<xsl:value-of select="$InvoiceLine/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='RelatedExportPermitNumber_Hidden']/s0:Value/text()"/>
						</ns2:relatedPermitNumber>
						<ns2:relatedPermitDate>
							<xsl:value-of select="ScriptNS3:ConvertToDate($InvoiceLine/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='RelatedExportPermitDate_Hidden']/s0:Value/text(), 'yyyy-MM-dd')"/>
						</ns2:relatedPermitDate>
					</ns2:sew>
					<ns2:durabilityStartDate>
						<xsl:value-of select="ScriptNS3:ConvertToDate(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='UseByStart']/s0:Value/text(),'yyyy-MM-dd')"/>
					</ns2:durabilityStartDate>
					<ns2:durabilityEndDate>
						<xsl:value-of select="ScriptNS3:ConvertToDate(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='UseByEnd']/s0:Value/text(),'yyyy-MM-dd')"/>
					</ns2:durabilityEndDate>
					<ns2:aheccCode>
						<xsl:value-of select="userCSharp:RemoveDot($InvoiceLine/s0:HarmonisedCode/text())"/>
					</ns2:aheccCode>
					<xsl:if test="$IsEU">
						<ns2:cnCode>
							<xsl:value-of select="userCSharp:RemoveDot(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CombinedNomenclature']/s0:Value/text())"/>
						</ns2:cnCode>
						<ns2:finalConsumerFlag>
							<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='FinalConsumer']/s0:Value/text()"/>
						</ns2:finalConsumerFlag>
					</xsl:if>
					<ns2:batchCode>
						<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='BatchCode']/s0:Value/text()"/>
					</ns2:batchCode>
					<ns2:additionalTexts>
						<xsl:variable name="statementNumbers" select="self::node()/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='StatementNumber1' or s0:Key/text()='StatementNumber2' or s0:Key/text()='StatementNumber3' or s0:Key/text()='StatementNumber4' or s0:Key/text()='StatementNumber5']"/>
						<ns2:additionalText>
							<xsl:if test ="count($statementNumbers) > 0">
								<ns2:code>ZZZ</ns2:code>
							</xsl:if>
							<xsl:for-each select="$statementNumbers">
								<xsl:variable name="addInfoValue" select="self::node()/s0:Value/text()"/>
								<xsl:call-template name="SplitTexts">
									<xsl:with-param name="AdditionalText" select="$addInfoValue"/>
								</xsl:call-template>
							</xsl:for-each>
						</ns2:additionalText>
						<xsl:for-each select="self::node()/s0:AddInfoCollection/s0:AddInfo">
							<xsl:variable name="addInfoKey" select="self::node()/s0:Key/text()"/>
							<xsl:variable name="addInfoValue" select="self::node()/s0:Value/text()"/>
							<ns2:additionalText>
								<xsl:call-template name="GetAdditionalTextFromAddInfo">
									<xsl:with-param name="KeyForCode" select="$addInfoKey"/>
									<xsl:with-param name="AdditionalText" select="$addInfoValue"/>
								</xsl:call-template>
							</ns2:additionalText>
						</xsl:for-each>
					</ns2:additionalTexts>
					<xsl:variable name="productSource" select="key('QuarantineExDocHeaderAddInfo','ProductSource')/s0:Value"/>
					<xsl:choose>
						<xsl:when test="not($productSource!='')">
							<ns2:productSourceCountries>
								<ns2:removeExistingSet>true</ns2:removeExistingSet>
							</ns2:productSourceCountries>
						</xsl:when>
						<xsl:otherwise>
							<ns2:productSourceCountries>
								<ns2:productSourceCountry>
									<xsl:value-of select="$productSource"/>
								</ns2:productSourceCountry>
							</ns2:productSourceCountries>
						</xsl:otherwise>
					</xsl:choose>
					<xsl:variable name="QLAAddInfoGroup" select="$InvoiceLine/s0:AddInfoGroupCollection/s0:AddInfoGroup[s0:Type/s0:Code/text()='QLA']"/>
					<xsl:if test="$Attachments and $QLAAddInfoGroup">
						<ns2:attachments>
							<xsl:for-each select="$QLAAddInfoGroup">
								<xsl:variable name="fileName" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='FileName']/s0:Value/text()"/>
								<ns1:attachment>
									<ns1:name>
										<xsl:value-of select="$fileName"/>
									</ns1:name>
									<ns1:attachmentType>
										<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='AttachmentType']/s0:Value/text()"/>
									</ns1:attachmentType>
									<ns1:description>
										<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='Description']/s0:Value/text()"/>
									</ns1:description>
									<ns1:mimeType>
										<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='mimeType']/s0:Value/text()"/>
									</ns1:mimeType>
									<ns1:data>
										<xsl:value-of select="$Attachments/s0:AttachedDocument[s0:FileName/text()=$fileName]/s0:ImageData/text()"/>
									</ns1:data>
								</ns1:attachment>
							</xsl:for-each>
						</ns2:attachments>
					</xsl:if>
					<ns2:importAuthorityCode>
						<xsl:value-of select="$InvoiceLine/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ImportAuthorityCode_Hidden']/s0:Value/text()"/>
					</ns2:importAuthorityCode>
					<xsl:choose>
						<xsl:when test="$commodityType = 'SKN'">
							<ns2:skinsAndHidesProductLineDetails xmlns:skn="http://agriculture.gov.au/nexdoc/common/rex/SkinsAndHidesTypes_1.0">
								<skn:saltingDate>
									<xsl:value-of select="ScriptNS3:ConvertToDate(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='SaltingDate']/s0:Value/text(),'yyyy-MM-dd')"/>
								</skn:saltingDate>
							</ns2:skinsAndHidesProductLineDetails>
						</xsl:when>
						<xsl:otherwise>
							<ns2:fishProductLineDetails xmlns:fis="http://agriculture.gov.au/nexdoc/common/rex/FishTypes_1.0">
								<fis:catchStartDate>
									<xsl:value-of select="ScriptNS3:ConvertToDate(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CatchStartDate']/s0:Value/text(),'yyyy-MM-dd')"/>
								</fis:catchStartDate>
								<fis:catchEndDate>
									<xsl:value-of select="ScriptNS3:ConvertToDate(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CatchEndDate']/s0:Value/text(),'yyyy-MM-dd')"/>
								</fis:catchEndDate>
								<fis:fishWaterIndicator>
									<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='FishWaterIndicator']/s0:Value/text()"/>
								</fis:fishWaterIndicator>
								<fis:fishEstablishments>
									<xsl:if test="$NPDAddInfoFreeTextEstablishmentGroup">
										<xsl:for-each select="$NPDAddInfoFreeTextEstablishmentGroup">
											<xsl:variable name="FreeTextAddInfoCollection" select="s0:AddInfoCollection" />
											<xsl:variable name="FreeTextAddInfoOrganizationAddress" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='AQISProcessingEstablishment']" />
											<fis:freeTextEstablishment>
												<fis:startDate>
													<xsl:value-of select="ScriptNS3:ConvertToDate($FreeTextAddInfoCollection/s0:AddInfo[s0:Key/text()='StartDate']/s0:Value/text(),'yyyy-MM-dd')" />
												</fis:startDate>
												<fis:endDate>
													<xsl:value-of select="ScriptNS3:ConvertToDate($FreeTextAddInfoCollection/s0:AddInfo[s0:Key/text()='EndDate']/s0:Value/text(),'yyyy-MM-dd')" />
												</fis:endDate>
												<fis:establishmentIndicator>
													<xsl:value-of select="$FreeTextAddInfoCollection/s0:AddInfo[s0:Key/text()='EstablishmentIndicator']/s0:Value/text()" />
												</fis:establishmentIndicator>
												<fis:establishmentName>
													<xsl:value-of select="$FreeTextAddInfoOrganizationAddress/s0:CompanyName/text()" />
												</fis:establishmentName>
												<fis:postCode>
													<xsl:value-of select="$FreeTextAddInfoOrganizationAddress/s0:Postcode/text()" />
												</fis:postCode>
												<fis:removeEntry>
													<xsl:value-of select="$FreeTextAddInfoCollection/s0:AddInfo[s0:Key/text()='RemoveEntry']/s0:Value/text()" />
												</fis:removeEntry>
											</fis:freeTextEstablishment>
										</xsl:for-each>
									</xsl:if>
									<xsl:if test="$NPDAddInfoHAGroup">
										<xsl:for-each select="$NPDAddInfoHAGroup">
											<xsl:variable name="HarvestAreaAddInfo" select="s0:AddInfoCollection" />
											<fis:harvestArea>
												<xsl:variable name="onshore" select="$HarvestAreaAddInfo/s0:AddInfo[s0:Key/text()='HarvestArea']/s0:Value/text()!=''" />
												<xsl:choose>
													<xsl:when test="$onshore">
														<fis:harvestAreaDetails>
															<fis:harvestAreaDate>
																<xsl:value-of select="ScriptNS3:ConvertToDate($HarvestAreaAddInfo/s0:AddInfo[s0:Key/text()='StartDate']/s0:Value/text(),'yyyy-MM-dd')" />
															</fis:harvestAreaDate>
															<fis:harvestAreaName>
																<xsl:value-of select="$HarvestAreaAddInfo/s0:AddInfo[s0:Key/text()='HarvestArea']/s0:Value/text()" />
															</fis:harvestAreaName>
															<fis:leaseNumber>
																<xsl:value-of select="$HarvestAreaAddInfo/s0:AddInfo[s0:Key/text()='LeaseNumber']/s0:Value/text()" />
															</fis:leaseNumber>
														</fis:harvestAreaDetails>
													</xsl:when>
													<xsl:otherwise>
														<fis:offshore>
															<fis:startDate>
																<xsl:value-of select="ScriptNS3:ConvertToDate($HarvestAreaAddInfo/s0:AddInfo[s0:Key/text()='StartDate']/s0:Value/text(),'yyyy-MM-dd')" />
															</fis:startDate>
															<fis:endDate>
																<xsl:value-of select="ScriptNS3:ConvertToDate($HarvestAreaAddInfo/s0:AddInfo[s0:Key/text()='EndDate']/s0:Value/text(),'yyyy-MM-dd')" />
															</fis:endDate>
														</fis:offshore>
													</xsl:otherwise>
												</xsl:choose>
												<fis:depuration>
													<fis:startDate>
														<xsl:value-of select="ScriptNS3:ConvertToDate($HarvestAreaAddInfo/s0:AddInfo[s0:Key/text()='DepurationDate']/s0:Value/text(),'yyyy-MM-dd')" />
													</fis:startDate>
													<fis:establishmentNumber>
														<xsl:value-of select="$HarvestAreaAddInfo/s0:AddInfo[s0:Key/text()='EstablishmentID']/s0:Value/text()" />
													</fis:establishmentNumber>
												</fis:depuration>
												<fis:removeEntry>
													<xsl:value-of select="$HarvestAreaAddInfo/s0:AddInfo[s0:Key/text()='RemoveEntry']/s0:Value/text()" />
												</fis:removeEntry>
											</fis:harvestArea>
										</xsl:for-each>
									</xsl:if>
								</fis:fishEstablishments>
							</ns2:fishProductLineDetails>
						</xsl:otherwise>
					</xsl:choose>
					<ns2:natureOfCommodity>
						<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='NatureOfCommodity']/s0:Value/text()"/>
					</ns2:natureOfCommodity>
					<ns2:euTreatmentType>
						<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TreatmentType']/s0:Value/text()"/>
					</ns2:euTreatmentType>
					<ns2:quotaExporter>
						<xsl:value-of select="$InvoiceLine/../../s0:Supplier/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text()='QEN' and s0:CountryOfIssue/s0:Code/text()='AU']/s0:Value/text()"/>
					</ns2:quotaExporter>
				</ns2:productLine>
				<xsl:if test="position()=last()">
					<ns2:retainProductLines>true</ns2:retainProductLines>
				</xsl:if>
			</xsl:for-each>
		</xsl:element>
		<xsl:element name="manufacturers"  namespace="{$namespace}">
			<xsl:variable name="manufacturerOrganizationAddresses" select="s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice/s0:CommercialInvoiceLineCollection/s0:CommercialInvoiceLine/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='Manufacturer']"/>
			<xsl:variable name="commercialInvoiceLines" select="s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice/s0:CommercialInvoiceLineCollection/s0:CommercialInvoiceLine"/>
			<xsl:for-each select="$commercialInvoiceLines[generate-id() =  generate-id(key('manufacturer', s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='Manufacturer']))]">
				<ns2:manufacturer>
					<ns2:lineNumbers>
						<xsl:for-each select="key('manufacturer', s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='Manufacturer'])">
							<ns2:lineNumber>
								<xsl:value-of select="s0:LineNo/text()"/>
							</ns2:lineNumber>
						</xsl:for-each>
					</ns2:lineNumbers>
					<ns2:manufacturerDetails>
						<xsl:variable name="manufacturerOrg" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='Manufacturer']"/>
						<ns2:name>
							<xsl:value-of select="$manufacturerOrg/s0:CompanyName/text()"/>
						</ns2:name>
						<ns2:address>
							<ns1:streetAddress>
								<ns1:streetLine>
									<xsl:value-of select="$manufacturerOrg/s0:Address1/text()"/>
								</ns1:streetLine>
								<ns1:streetLine>
									<xsl:value-of select="$manufacturerOrg/s0:Address2/text()"/>
								</ns1:streetLine>
							</ns1:streetAddress>
							<ns1:city>
								<xsl:value-of select="$manufacturerOrg/s0:City/text()"/>
							</ns1:city>
							<ns1:state>
								<xsl:value-of select="$manufacturerOrg/s0:State/text()"/>
							</ns1:state>
							<ns1:country>
								<xsl:value-of select="$manufacturerOrg/s0:Country/s0:Code/text()"/>
							</ns1:country>
							<ns1:postalCode>
								<xsl:value-of select="$manufacturerOrg/s0:Postcode/text()"/>
							</ns1:postalCode>
						</ns2:address>
					</ns2:manufacturerDetails>
				</ns2:manufacturer>
			</xsl:for-each>
		</xsl:element>

		<xsl:variable name="QHAAddInfoGroup" select="s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice/s0:AddInfoGroupCollection/s0:AddInfoGroup[s0:Type/s0:Code/text()='QHA']"/>
		<xsl:if test="$Attachments and $QHAAddInfoGroup">
			<xsl:element name="attachments" namespace="{$namespace}">
				<xsl:for-each select="$QHAAddInfoGroup">
					<xsl:variable name="fileName" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='FileName']/s0:Value/text()"/>
					<ns1:attachment>
						<ns1:name>
							<xsl:value-of select="$fileName"/>
						</ns1:name>
						<ns1:attachmentType>
							<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='AttachmentType']/s0:Value/text()"/>
						</ns1:attachmentType>
						<ns1:description>
							<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='Description']/s0:Value/text()"/>
						</ns1:description>
						<ns1:mimeType>
							<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='mimeType']/s0:Value/text()"/>
						</ns1:mimeType>
						<ns1:data>
							<xsl:value-of select="$Attachments/s0:AttachedDocument[s0:FileName/text()=$fileName]/s0:ImageData/text()"/>
						</ns1:data>
					</ns1:attachment>
				</xsl:for-each>
			</xsl:element>
		</xsl:if>

		<xsl:element name="additionalTexts" namespace="{$namespace}">
			<xsl:for-each select="s0:NoteCollection/s0:Note">
				<xsl:variable name="noteDescription" select="self::node()/s0:Description/text()"/>
				<xsl:variable name="noteText" select="self::node()/s0:NoteText/text()"/>
				<ns2:additionalText>
					<xsl:call-template name="GetAdditionalTextFromNote">
						<xsl:with-param name="KeyForCode" select="$noteDescription"/>
						<xsl:with-param name="AdditionalText" select="$noteText"/>
					</xsl:call-template>
				</ns2:additionalText>
			</xsl:for-each>
			<xsl:for-each select="$QHAddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CatchZone']">
				<xsl:variable name="addInfoValue" select="self::node()/s0:Value/text()"/>
				<ns2:additionalText>
					<ns2:code>OCZ</ns2:code>
					<xsl:call-template name="SplitTexts">
						<xsl:with-param name="AdditionalText" select="$addInfoValue"/>
					</xsl:call-template>
				</ns2:additionalText>
			</xsl:for-each>
		</xsl:element>

		<xsl:variable name="ResponsiblePersonAddress" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='AQISResponsiblePerson']"/>
		<xsl:variable name="PersonName" select="normalize-space($ResponsiblePersonAddress/s0:Contact)"/>
		<xsl:variable name="TransitDestinationAddress" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='AQISTransitDestination']"/>
		<xsl:if test="($ResponsiblePersonAddress and $PersonName != '') or $TransitDestinationAddress">
			<xsl:element name="euTransit" namespace="{$namespace}">
				<xsl:if test="$ResponsiblePersonAddress and $PersonName != ''">
					<ns2:personResponsible>
						<ns2:address>
							<xsl:call-template name="GetAddressTypeNode">
								<xsl:with-param name="OrganizationAddress" select="$ResponsiblePersonAddress"/>
							</xsl:call-template>
						</ns2:address>
						<ns2:firstName>
							<xsl:value-of select="userCSharp:GetFirstName($PersonName)"/>
						</ns2:firstName>
						<ns2:lastName>
							<xsl:value-of select="userCSharp:GetLastName($PersonName)"/>
						</ns2:lastName>
						<ns2:phoneNumber>
							<xsl:variable name ="ResponsiblePersonPhone" select="$ResponsiblePersonAddress/s0:Phone"/>
							<xsl:choose>
								<xsl:when test="$ResponsiblePersonPhone != ''">
									<xsl:value-of select="$ResponsiblePersonPhone"/>
								</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="$ResponsiblePersonAddress/s0:Mobile"/>
								</xsl:otherwise>
							</xsl:choose>
						</ns2:phoneNumber>
					</ns2:personResponsible>
				</xsl:if>
				<xsl:if test="$TransitDestinationAddress">
					<ns2:placeOfDestinationDetails>
						<ns2:name>
							<xsl:value-of select="$TransitDestinationAddress/s0:CompanyName"/>
						</ns2:name>
						<ns2:address>
							<xsl:call-template name="GetAddressTypeNode">
								<xsl:with-param name="OrganizationAddress" select="$TransitDestinationAddress"/>
							</xsl:call-template>
						</ns2:address>
						<ns2:phoneNumber>
							<xsl:variable name ="TransitDestinationPhone" select="$TransitDestinationAddress/s0:Phone"/>
							<xsl:choose>
								<xsl:when test="$TransitDestinationPhone != ''">
									<xsl:value-of select="$TransitDestinationPhone"/>
								</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="$TransitDestinationAddress/s0:Mobile"/>
								</xsl:otherwise>
							</xsl:choose>
						</ns2:phoneNumber>
						<ns2:approvalNumber>
							<xsl:value-of select="key('QuarantineExDocHeaderAddInfo', 'ApprovalNumber')/s0:Value" />
						</ns2:approvalNumber>
						<ns2:transitLocationType>
							<xsl:value-of select="key('QuarantineExDocHeaderAddInfo', 'TransitLocationType')/s0:Value" />
						</ns2:transitLocationType>
					</ns2:placeOfDestinationDetails>
				</xsl:if>
			</xsl:element>
		</xsl:if>

		<xsl:if test="$MstType='AMEND'">
			<xsl:variable name="SubmitAmendmentRequest" select="$QHAddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='SubmitAmendmentRequest']/s0:Value/text()='Y'"/>
			<xsl:if test="$SubmitAmendmentRequest='Y'">
				<xsl:element name="submitAmendmentRequest" namespace="{$namespace}">true</xsl:element>
				<xsl:element name="amendmentReason" namespace="{$namespace}">
					<xsl:value-of select="$QHAddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='RequestAmendReason']/s0:Value/text()"/>
				</xsl:element>
			</xsl:if>
		</xsl:if>
	</xsl:template>


	<xsl:template name="StagingMAINBody">
		<xsl:param name="rootName"/>
		<xsl:param name="namespace"/>
		<xsl:param name="rexNumber"/>
		<xsl:param name="lastAmendDateTime"/>
		<xsl:param name="QHAddInfoGroup"/>
		<xsl:variable name="Attachments" select="/s0:UniversalShipment/s0:Shipment/s0:AttachedDocumentCollection"/>
		<xsl:variable name="QLAddInfoGroup" select="s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice/s0:CommercialInvoiceLineCollection/s0:CommercialInvoiceLine/s0:AddInfoGroupCollection/s0:AddInfoGroup[s0:Type/s0:Code/text()='QL']"/>
		<xsl:variable name="invoiceCurrency" select="s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice/s0:InvoiceCurrency/s0:Code/text()"/>
		<xsl:variable name="commodityType" select="key('QuarantineExDocHeaderAddInfo','ProduceType')/s0:Value" />
		<xsl:variable name="productUse" select="key('QuarantineExDocHeaderAddInfo','ProductUse')/s0:Value" />
		<xsl:variable name="customsAgentIndicator">
			<xsl:choose>
				<xsl:when test="$QHAddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ObtainExportCustomsPermit']/s0:Value/text()='Y'">Y</xsl:when>
				<xsl:otherwise>N</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:element name="identification" namespace="{$namespace}">
			<ns1:rexNumber>
				<xsl:value-of select="$rexNumber"/>
			</ns1:rexNumber>
			<ns2:lastAmendDateTime>
				<xsl:value-of select="$lastAmendDateTime"/>
			</ns2:lastAmendDateTime>
		</xsl:element>
		<xsl:element name="exportDetails" namespace="{$namespace}">
			<xsl:variable name="transportModeOrigin" select="translate(s0:TransportMode/s0:Code/text(), $smallcase, $uppercase)" />
			<xsl:variable name="transportMode">
				<xsl:choose>
					<xsl:when test="$transportModeOrigin = 'AIR'">A</xsl:when>
					<xsl:when test="$transportModeOrigin = 'SEA'">S</xsl:when>
					<xsl:otherwise>M</xsl:otherwise>
				</xsl:choose>
			</xsl:variable>
			<xsl:choose>
				<xsl:when test="$commodityType='OTH'">
					<ns2:commodityType>X</ns2:commodityType>
				</xsl:when>
				<xsl:otherwise>
					<ns2:commodityType>
						<xsl:value-of select="substring($commodityType, 1, 1)"/>
					</ns2:commodityType>
				</xsl:otherwise>
			</xsl:choose>
			<ns2:priority>
				<xsl:choose>
					<xsl:when test="$transportMode = 'A'">1</xsl:when>
					<xsl:otherwise>9</xsl:otherwise>
				</xsl:choose>
			</ns2:priority>
			<ns2:departureDate>
				<xsl:value-of select="ScriptNS3:ConvertToDate($DepartureDate,'yyyy-MM-dd')"/>
			</ns2:departureDate>
			<ns2:transportDetails>
				<xsl:if test="$IsEU">
					<xsl:variable name="AQISEUPlaceOfDestination" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='AQISEUPlaceOfDestination']" />
					<ns2:euPlaceOfDestinationDetail>
						<ns2:name>
							<xsl:value-of select="$AQISEUPlaceOfDestination/s0:CompanyName/text()"/>
						</ns2:name>
						<ns2:address>
							<ns1:streetAddress>
								<ns1:streetLine>
									<xsl:value-of select="$AQISEUPlaceOfDestination/s0:Address1/text()"/>
								</ns1:streetLine>
								<ns1:streetLine>
									<xsl:value-of select="$AQISEUPlaceOfDestination/s0:Address2/text()"/>
								</ns1:streetLine>
							</ns1:streetAddress>
							<ns1:city>
								<xsl:value-of select="$AQISEUPlaceOfDestination/s0:City/text()"/>
							</ns1:city>
							<ns1:state>
								<xsl:value-of select="$AQISEUPlaceOfDestination/s0:State/text()"/>
							</ns1:state>
							<ns1:country>
								<xsl:value-of select="$AQISEUPlaceOfDestination/s0:Country/s0:Code/text()"/>
							</ns1:country>
							<ns1:postalCode>
								<xsl:value-of select="$AQISEUPlaceOfDestination/s0:Postcode/text()"/>
							</ns1:postalCode>
						</ns2:address>
						<ns2:approvalNumber>
							<xsl:value-of select="$AQISEUPlaceOfDestination/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text()='ETI' and s0:CountryOfIssue/s0:Code/text()='AU']/s0:Value/text()"/>
						</ns2:approvalNumber>
					</ns2:euPlaceOfDestinationDetail>
				</xsl:if>
				<ns2:transportMode>
					<xsl:value-of select="$transportMode"/>
				</ns2:transportMode>
				<ns2:voyageOrFlightNumber>
					<xsl:variable name="voyageOrFlightNumber" select="s0:VoyageFlightNo/text()" />
					<xsl:if test="not($voyageOrFlightNumber!='')">
						<xsl:attribute name="xsi:nil" namespace="http://www.w3.org/2001/XMLSchema-instance">true</xsl:attribute>
					</xsl:if>
					<xsl:value-of select="$voyageOrFlightNumber"/>
				</ns2:voyageOrFlightNumber>
				<ns2:vesselName>
					<xsl:variable name="vesselName" select="s0:VesselName/text()"/>
					<xsl:if test="not($vesselName!='')">
						<xsl:attribute name="xsi:nil" namespace="http://www.w3.org/2001/XMLSchema-instance">true</xsl:attribute>
					</xsl:if>
					<xsl:value-of select="$vesselName"/>
				</ns2:vesselName>
				<xsl:variable name="companyName" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='ShippingLine']/s0:CompanyName/text()" />
				<xsl:if test="$companyName != ''">
					<ns2:shippingCompany>
						<xsl:value-of select="$companyName"/>
					</ns2:shippingCompany>
				</xsl:if>
				<xsl:if test="not($companyName!='')">
					<ns2:shippingCompany>
						<xsl:attribute name="xsi:nil" namespace="http://www.w3.org/2001/XMLSchema-instance">true</xsl:attribute>
					</ns2:shippingCompany>
				</xsl:if>
				<xsl:choose>
					<xsl:when test="$QHAddInfoGroup">
						<xsl:variable name="temperatureUnit" select="key('QuarantineExDocHeaderAddInfo','TemperatureUnit')/s0:Value" />
						<xsl:variable name="storeTransportTemperature" select="userCSharp:If($commodityType = 'FSH', key('QuarantineExDocHeaderAddInfo','MaximumTemperature')/s0:Value, key('QuarantineExDocHeaderAddInfo','AbsoluteTemperature')/s0:Value)"/>
						<xsl:choose>
							<xsl:when test="not($storeTransportTemperature!='')">
								<ns2:storeTransportTemperature>
									<xsl:attribute name="xsi:nil" namespace="http://www.w3.org/2001/XMLSchema-instance">true</xsl:attribute>
								</ns2:storeTransportTemperature>
							</xsl:when>
							<xsl:otherwise>
								<ns2:storeTransportTemperature unit="{$temperatureUnit}">
									<xsl:value-of select="$storeTransportTemperature"/>
								</ns2:storeTransportTemperature>
							</xsl:otherwise>
						</xsl:choose>
						<xsl:variable name="sealStartNumber" select="key('QuarantineExDocHeaderAddInfo','StartHoldSeal')/s0:Value" />
						<xsl:variable name="sealEndNumber" select="key('QuarantineExDocHeaderAddInfo','EndHoldSeal')/s0:Value" />
						<xsl:choose>
							<xsl:when test="not($sealStartNumber!='') and not($sealEndNumber!='')">
								<ns2:vesselHoldSeals>
									<xsl:attribute name="xsi:nil" namespace="http://www.w3.org/2001/XMLSchema-instance">true</xsl:attribute>
								</ns2:vesselHoldSeals>
							</xsl:when>
							<xsl:otherwise>
								<ns2:vesselHoldSeals>
									<ns2:sealStartNumber>
										<xsl:value-of select="$sealStartNumber"/>
									</ns2:sealStartNumber>
									<ns2:sealEndNumber>
										<xsl:value-of select="$sealEndNumber"/>
									</ns2:sealEndNumber>
								</ns2:vesselHoldSeals>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:when>
					<xsl:otherwise>
						<ns2:storeTransportTemperature>
							<xsl:attribute name="xsi:nil" namespace="http://www.w3.org/2001/XMLSchema-instance">true</xsl:attribute>
						</ns2:storeTransportTemperature>
						<ns2:vesselHoldSeals>
							<xsl:attribute name="xsi:nil" namespace="http://www.w3.org/2001/XMLSchema-instance">true</xsl:attribute>
						</ns2:vesselHoldSeals>
					</xsl:otherwise>
				</xsl:choose>
			</ns2:transportDetails>
			<ns2:destinationCity>
				<xsl:variable name="destinationCity" select="s0:PortOfDestination/s0:Name/text()" />
				<xsl:if test="not($destinationCity!='')">
					<xsl:attribute name="xsi:nil" namespace="http://www.w3.org/2001/XMLSchema-instance">true</xsl:attribute>
				</xsl:if>
				<xsl:value-of select="substring($destinationCity, 1, 25)"/>
			</ns2:destinationCity>
			<xsl:variable name="dischargePort" select="s0:PortOfDischarge/s0:Code/text()" />
			<xsl:choose>
				<xsl:when test="not($dischargePort!='')">
					<ns2:dischargePorts>
						<ns2:removeExistingSet>true</ns2:removeExistingSet>
					</ns2:dischargePorts>
				</xsl:when>
				<xsl:otherwise>
					<ns2:dischargePorts>
						<ns2:dischargePort>
							<xsl:value-of select="$dischargePort"/>
						</ns2:dischargePort>
					</ns2:dischargePorts>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:variable name="destinationCountryCode" select="substring(s0:PortOfDestination/s0:Code/text(), 1, 2)" />
			<ns2:destinationCountry>
				<xsl:value-of select="$destinationCountryCode"/>
			</ns2:destinationCountry>
			<ns2:transitCountries>
				<xsl:variable name="countryCodes" select="s0:TransportLegCollection/s0:TransportLeg/s0:PortOfDischarge[substring(s0:Code/text(),1,2) != $destinationCountryCode]" />
				<xsl:choose>
					<xsl:when test="not($countryCodes!='')">
						<ns2:removeExistingSet>true</ns2:removeExistingSet>
					</xsl:when>
					<xsl:otherwise>
						<xsl:for-each select="$countryCodes">
							<xsl:variable name="countryCode" select="substring(self::node()/s0:Code/text(),1,2)" />
							<xsl:variable name="nextCountryCode" select="substring(../following-sibling::*[1]/s0:PortOfDischarge[s0:Code/text()],1,2)" />
							<xsl:if test="$countryCode != $nextCountryCode">
								<ns2:transitCountry>
									<xsl:value-of select="$countryCode"/>
								</ns2:transitCountry>
							</xsl:if>
						</xsl:for-each>
					</xsl:otherwise>
				</xsl:choose>
			</ns2:transitCountries>
			<ns2:borderInspectionPort>
				<xsl:variable name="borderInspectionPort" select="key('QuarantineExDocHeaderAddInfo','BorderInspectionPort')/s0:Value" />
				<xsl:if test="not($borderInspectionPort!='')">
					<xsl:attribute name="xsi:nil" namespace="http://www.w3.org/2001/XMLSchema-instance">true</xsl:attribute>
				</xsl:if>
				<xsl:value-of select="$borderInspectionPort"/>
			</ns2:borderInspectionPort>
			<xsl:variable name="loadingPort" select="s0:PortOfLoading/s0:Code/text()" />
			<xsl:choose>
				<xsl:when test="not($loadingPort!='')">
					<ns2:loadingPorts>
						<ns2:removeExistingSet>true</ns2:removeExistingSet>
					</ns2:loadingPorts>
				</xsl:when>
				<xsl:otherwise>
					<ns2:loadingPorts>
						<ns2:loadingPort>
							<xsl:value-of select="$loadingPort"/>
						</ns2:loadingPort>
					</ns2:loadingPorts>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:variable name="importPermits" select="s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice/s0:CommercialInvoiceLineCollection/s0:CommercialInvoiceLine/s0:AddInfoCollection[s0:AddInfo[s0:Key/text()='TemporaryImportNumbers_Hidden']/s0:Value/text()!='' and s0:AddInfo[s0:Key/text()='TemporaryImportDate_Hidden']/s0:Value/text()!='']"/>
			<xsl:choose>
				<xsl:when test="not($importPermits)">
					<ns2:importPermits>
						<ns2:removeExistingSet>true</ns2:removeExistingSet>
					</ns2:importPermits>
				</xsl:when>
				<xsl:otherwise>
					<ns2:importPermits>
						<xsl:for-each select="$importPermits[count(.|key('importPermits-group-by-number-and-date', concat(s0:AddInfo[s0:Key/text()='TemporaryImportNumbers_Hidden']/s0:Value/text(), s0:AddInfo[s0:Key/text()='TemporaryImportDate_Hidden']/s0:Value/text()))[1])=1]">
							<xsl:variable name="importPermitNumber" select="s0:AddInfo[s0:Key/text()='TemporaryImportNumbers_Hidden']/s0:Value/text()"/>
							<xsl:variable name="importPermitDateStr" select="s0:AddInfo[s0:Key/text()='TemporaryImportDate_Hidden']/s0:Value/text()"/>
							<ns2:importPermit>
								<ns2:importPermitNumber>
									<xsl:value-of select="$importPermitNumber"/>
								</ns2:importPermitNumber>
								<ns2:importPermitDate>
									<xsl:value-of select="ScriptNS3:ConvertToDate($importPermitDateStr,'yyyy-MM-dd')"/>
								</ns2:importPermitDate>
							</ns2:importPermit>
						</xsl:for-each>
					</ns2:importPermits>
				</xsl:otherwise>
			</xsl:choose>
			<ns2:sew>
				<ns2:customsAgentIndicator>
					<xsl:value-of select="$customsAgentIndicator"/>
				</ns2:customsAgentIndicator>
				<xsl:if test="$customsAgentIndicator='N'">
					<ns2:edn>
						<xsl:value-of select="s0:EntryNumberCollection/s0:EntryNumber[s0:Type/s0:Code/text()='CAN']/s0:Number/text()"/>
					</ns2:edn>
				</xsl:if>
				<ns2:currency>
					<xsl:value-of select="$invoiceCurrency"/>
				</ns2:currency>
				<ns2:consigneeName>
					<xsl:value-of select="key('QuarantineExDocHeaderAddInfo','CustomsConsigneeName')/s0:Value"/>
				</ns2:consigneeName>
			</ns2:sew>
			<ns2:ownerExporterId>
				<xsl:value-of select="s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice/s0:Supplier/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text()='NEN' and s0:CountryOfIssue/s0:Code/text()='AU']/s0:Value/text()"/>
			</ns2:ownerExporterId>
			<xsl:variable name="exporterDeclaration" select="key('QuarantineExDocHeaderAddInfo','ExporterDeclaration')/s0:Value"/>
			<xsl:variable name="exporterDeclarationCode" select="$QHAddInfoGroup/s0:CustomsReferenceCollection/s0:CustomsReference[s0:Type/s0:Code/text()='DEC' and s0:Reference/text()!='CDD03' and s0:Reference/text()!='CDD04' and s0:Reference/text()!='CDD05']"/>
			<ns2:exporterDeclaration>
				<ns2:exporterDeclaration>
					<xsl:value-of select="$exporterDeclaration"/>
				</ns2:exporterDeclaration>
				<xsl:choose>
					<xsl:when test="not($exporterDeclarationCode)">
						<ns2:removeExistingSet>true</ns2:removeExistingSet>
					</xsl:when>
					<xsl:otherwise>
						<xsl:for-each select="$exporterDeclarationCode">
							<ns2:exporterDeclarationCode>
								<xsl:value-of select="s0:Reference/text()"/>
							</ns2:exporterDeclarationCode>
						</xsl:for-each>
					</xsl:otherwise>
				</xsl:choose>
			</ns2:exporterDeclaration>
			<xsl:if test="$commodityType = 'DAI'">
				<ns2:imaDetailsList>
					<xsl:choose>
						<xsl:when test="not($QLAddInfoGroup)">
							<ns2:removeExistingSet>true</ns2:removeExistingSet>
						</xsl:when>
						<xsl:otherwise>
							<xsl:for-each select="$QLAddInfoGroup">
								<xsl:variable name="InvoiceLine" select="../.."/>
								<xsl:variable name="CommercialInvoice" select="$InvoiceLine/../.."/>
								<xsl:variable name="serialNumber" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='IMA1SerialNumber']/s0:Value/text()"/>
								<xsl:if test="$serialNumber">
									<ns2:imaDetails>
										<ns2:serialNumber>
											<xsl:value-of select="$serialNumber"/>
										</ns2:serialNumber>
										<xsl:choose>
											<xsl:when test="$transportMode = 'S'">
												<xsl:variable name="ContainerNumber" select="/s0:UniversalShipment/s0:Shipment/s0:PackingLineCollection/s0:PackingLine[s0:PackedItemCollection/s0:PackedItem/s0:CommercialInvoiceLineLink=$InvoiceLine/s0:Link][1]/s0:ContainerNumber" />
												<xsl:if test="$ContainerNumber">
													<xsl:variable name="RelatedContainer" select="/s0:UniversalShipment/s0:Shipment/s0:ContainerCollection/s0:Container[s0:ContainerNumber=$ContainerNumber][1]" />
													<xsl:if test="$RelatedContainer">
														<ns2:containerNumber>
															<xsl:value-of select="$ContainerNumber"/>
														</ns2:containerNumber>
													</xsl:if>
												</xsl:if>
											</xsl:when>
											<xsl:otherwise>
												<ns2:containerNumber>AIRFREIGHT</ns2:containerNumber>
											</xsl:otherwise>
										</xsl:choose>
										<ns2:invoiceDate>
											<xsl:value-of select="ScriptNS3:ConvertToDate($CommercialInvoice/s0:InvoiceDate/text(), 'yyyy-MM-dd')"/>
										</ns2:invoiceDate>
										<ns2:invoiceNumber>
											<xsl:value-of select="$CommercialInvoice/s0:InvoiceNumber/text()"/>
										</ns2:invoiceNumber>
										<ns2:grossWeight>
											<xsl:attribute name="unit">
												<xsl:value-of select="userCSharp:GetMappedWeightUnit($InvoiceLine/s0:WeightUnit/s0:Code/text())"/>
											</xsl:attribute>
											<xsl:value-of select="userCSharp:GetMappedWeight($InvoiceLine/s0:Weight/text(), $InvoiceLine/s0:WeightUnit/s0:Code/text())"/>
										</ns2:grossWeight>
										<ns2:netWeight>
											<xsl:attribute name="unit">
												<xsl:value-of select="userCSharp:GetMappedWeightUnit($InvoiceLine/s0:NetWeightUnit/s0:Code/text())"/>
											</xsl:attribute>
											<xsl:value-of select="userCSharp:GetMappedWeight($InvoiceLine/s0:NetWeight/text(), $InvoiceLine/s0:NetWeightUnit/s0:Code/text())"/>
										</ns2:netWeight>
										<ns2:productDescription>
											<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='IMA1ProductDescription']/s0:Value/text()"/>
										</ns2:productDescription>
										<ns2:quotaYear>
											<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='IMA1QuotaYear']/s0:Value/text()"/>
										</ns2:quotaYear>
									</ns2:imaDetails>
								</xsl:if>
							</xsl:for-each>
						</xsl:otherwise>
					</xsl:choose>
				</ns2:imaDetailsList>
			</xsl:if>
			<ns2:exporterReference>
				<xsl:if test="not($CalculatedExporterReference!='')">
					<xsl:attribute name="xsi:nil" namespace="http://www.w3.org/2001/XMLSchema-instance">true</xsl:attribute>
				</xsl:if>
				<xsl:value-of select="$CalculatedExporterReference"/>
			</ns2:exporterReference>
			<xsl:variable name="importerDocumentaryAddress" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='ImporterDocumentaryAddress' or s0:AddressType/text()='Importer']"/>
			<ns2:consigneeDetails>
				<ns1:consigneeName>
					<xsl:value-of select="$importerDocumentaryAddress/s0:CompanyName/text()"/>
				</ns1:consigneeName>
				<ns1:consigneeAddress>
					<ns1:streetAddress>
						<xsl:variable name="address1" select="$importerDocumentaryAddress/s0:Address1/text()" />
						<xsl:variable name="address2" select="$importerDocumentaryAddress/s0:Address2/text()" />
						<xsl:if test="$address1 != ''">
							<ns1:streetLine>
								<xsl:value-of select="substring($address1, 1, 35)"/>
							</ns1:streetLine>
						</xsl:if>
						<xsl:if test="$address2 != ''">
							<ns1:streetLine>
								<xsl:value-of select="substring($address2, 1, 35)"/>
							</ns1:streetLine>
						</xsl:if>
					</ns1:streetAddress>
					<ns1:city>
						<xsl:choose>
							<xsl:when test="$commodityType = 'DAI'">
								<xsl:value-of select="substring($importerDocumentaryAddress/s0:City/text(), 1, 25)"/>
							</xsl:when>
							<xsl:otherwise>
								<xsl:value-of select="$importerDocumentaryAddress/s0:City/text()"/>
							</xsl:otherwise>
						</xsl:choose>
					</ns1:city>
					<ns1:state>
						<xsl:choose>
							<xsl:when test="$importerDocumentaryAddress/s0:State/@Description != ''">
								<xsl:value-of select="substring($importerDocumentaryAddress/s0:State/@Description, 1, 20)"/>
							</xsl:when>
							<xsl:otherwise>
								<xsl:value-of select="substring($importerDocumentaryAddress/s0:State/text(), 1, 20)"/>
							</xsl:otherwise>
						</xsl:choose>
					</ns1:state>
					<ns1:country>
						<xsl:value-of select="$importerDocumentaryAddress/s0:Country/s0:Code/text()"/>
					</ns1:country>
					<ns1:postalCode>
						<xsl:value-of select="$importerDocumentaryAddress/s0:Postcode/text()"/>
					</ns1:postalCode>
				</ns1:consigneeAddress>
				<xsl:variable name="consigneePhoneNumber" select="$importerDocumentaryAddress/s0:Phone/text()"/>
				<xsl:if test="$consigneePhoneNumber != ''">
					<ns1:consigneePhoneNumber>
						<xsl:value-of select="$consigneePhoneNumber"/>
					</ns1:consigneePhoneNumber>
				</xsl:if>
				<xsl:if test="$commodityType != 'DAI'">
					<ns1:consigneeRepresentative>
						<xsl:value-of select="$QHAddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ConsigneeAgentName']/s0:Value/text()"/>
					</ns1:consigneeRepresentative>
				</xsl:if>
			</ns2:consigneeDetails>
			<xsl:variable name="ackCustomsReferences" select="$QHAddInfoGroup/s0:CustomsReferenceCollection/s0:CustomsReference[s0:Type/s0:Code/text()='ACK' and s0:Reference/text()!='']"/>
			<xsl:if test="$ackCustomsReferences">
				<ns2:messageAcknowledgements>
					<xsl:for-each select="$ackCustomsReferences[count(.|key('ackCustomsReferences-group-by-Reference', s0:Reference/text())[1])=1]">
						<ns1:messageId>
							<xsl:value-of select="s0:Reference/text()"/>
						</ns1:messageId>
					</xsl:for-each>
				</ns2:messageAcknowledgements>
			</xsl:if>
			<ns2:tracesApprovalId>
				<xsl:value-of select="$importerDocumentaryAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text()='ETI']/s0:Value/text()"/>
			</ns2:tracesApprovalId>
			<xsl:if test="$productUse!=''">
				<ns2:productUseIndicator>
					<xsl:value-of select="$productUse"/>
				</ns2:productUseIndicator>
			</xsl:if>
			<ns2:importedProductFlag>
				<xsl:value-of select="substring($QHAddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ImportedProductFlag']/s0:Value/text(), 1, 1)"/>
			</ns2:importedProductFlag>
			<ns2:manufacturedTreatedPackagedLabelledInAustralia>
				<xsl:value-of select="substring($QHAddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ManufacturedTreatedPackagedLabelledInAustralia']/s0:Value/text(), 1, 1)"/>
			</ns2:manufacturedTreatedPackagedLabelledInAustralia>
			<ns2:legallyImported>
				<xsl:value-of select="substring($QHAddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='LegallyImportedFlag']/s0:Value/text(), 1, 1)"/>
			</ns2:legallyImported>
			<ns2:lotNumber>
				<xsl:value-of select="key('QuarantineExDocHeaderAddInfo','LotNumber')/s0:Value"/>
			</ns2:lotNumber>
			<ns2:storageEstablishmentNumber>
				<xsl:value-of select="key('QuarantineExDocHeaderAddInfo','StorageEstablishment')/s0:Value"/>
			</ns2:storageEstablishmentNumber>
			<ns2:quotaYear>
				<xsl:value-of select="key('QuarantineExDocHeaderAddInfo','AMLCQuotaYear')/s0:Value"/>
			</ns2:quotaYear>
			<ns2:quotaFlag></ns2:quotaFlag>
			<ns2:quotaType>
				<xsl:value-of select="key('QuarantineExDocHeaderAddInfo', 'QuotaType')/s0:Value"/>
			</ns2:quotaType>
			<ns2:shipStoresFlag>
				<xsl:value-of select="key('QuarantineExDocHeaderAddInfo','ShipsStores')/s0:Value"/>
			</ns2:shipStoresFlag>
			<ns2:exemptionCode>
				<xsl:value-of select="key('QuarantineExDocHeaderAddInfo','ExemptionCode')/s0:Value"/>
			</ns2:exemptionCode>
			<xsl:if test="$commodityType != 'WOL' and $commodityType != 'SKN'">
				<xsl:variable name="authorisationDate" select="key('QuarantineExDocHeaderAddInfo','AuthorisationDate')/s0:Value"/>
				<xsl:variable name="authorisingEstablishmentNumber" select="key('QuarantineExDocHeaderAddInfo','AuthorisationEstablishment')/s0:Value"/>
				<xsl:variable name="comments" select="key('QuarantineExDocHeaderAddInfo','AuthorisationComments')/s0:Value"/>
				<xsl:variable name="authorisationFlag" select="$QHAddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='AuthorisationFlag']/s0:Value"/>
				<xsl:choose>
					<xsl:when test="$authorisationFlag != ''">
						<ns2:authorisationFlag>
							<xsl:value-of select ="$authorisationFlag"/>
						</ns2:authorisationFlag>
					</xsl:when>
					<xsl:otherwise>
						<xsl:if test="$authorisationDate != '' or $authorisingEstablishmentNumber != '' or $comments != ''">
							<ns2:authorisationFlag>Y</ns2:authorisationFlag>
						</xsl:if>
					</xsl:otherwise>
				</xsl:choose>
				<ns2:authorisationDetails>
					<ns2:authorisationDate>
						<xsl:value-of select="$authorisationDate"/>
					</ns2:authorisationDate>
					<ns2:authorisingEstablishmentNumber>
						<xsl:value-of select="$authorisingEstablishmentNumber"/>
					</ns2:authorisingEstablishmentNumber>
					<ns2:comments>
						<xsl:value-of select="$comments"/>
					</ns2:comments>
				</ns2:authorisationDetails>
			</xsl:if>
			<xsl:variable name="EUContactPersonAddress" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='AQISEUContactPerson']"/>
			<xsl:if test="$EUContactPersonAddress">
				<ns2:euContactInformation>
					<ns2:contactName>
						<xsl:value-of select="$EUContactPersonAddress/s0:Contact"/>
					</ns2:contactName>
					<ns2:contactPhone>
						<xsl:value-of select="$EUContactPersonAddress/s0:Phone"/>
					</ns2:contactPhone>
					<ns2:comments>
						<xsl:value-of select="key('QuarantineExDocHeaderAddInfo','EUComments')/s0:Value"/>
					</ns2:comments>
					<ns2:testResultRequired>
						<xsl:value-of select="key('QuarantineExDocHeaderAddInfo','EUTestResultRequired')/s0:Value"/>
					</ns2:testResultRequired>
				</ns2:euContactInformation>
			</xsl:if>
			<xsl:if test="$commodityType = 'FSH'">
				<ns2:fishExportDetails xmlns:fis="http://agriculture.gov.au/nexdoc/common/rex/FishTypes_1.0">
					<fis:transportStorageMinimumTemperature unit="{key('QuarantineExDocHeaderAddInfo','TemperatureUnit')/s0:Value}">
						<xsl:value-of select="key('QuarantineExDocHeaderAddInfo','MinimumTemperature')/s0:Value"/>
					</fis:transportStorageMinimumTemperature>
					<xsl:variable name="catchingZones" select="$QHAddInfoGroup/s0:CustomsReferenceCollection/s0:CustomsReference[s0:Type/s0:Code/text()='OCZ' and s0:Reference/text()!='']"/>
					<xsl:if test="$catchingZones">
						<fis:catchingZones>
							<xsl:for-each select="$catchingZones">
								<fis:catchingZone>
									<xsl:value-of select="s0:Reference/text()"/>
								</fis:catchingZone>
							</xsl:for-each>
						</fis:catchingZones>
					</xsl:if>
				</ns2:fishExportDetails>
			</xsl:if>
			<xsl:if test="$commodityType = 'WOL'">
				<ns2:woolExportDetails xmlns:wool="http://agriculture.gov.au/nexdoc/common/rex/WoolTypes_1.0">
					<wool:packDate>
						<xsl:value-of select="ScriptNS3:ConvertToDate(key('QuarantineExDocHeaderAddInfo','PackDate')/s0:Value,'yyyy-MM-dd')"/>
					</wool:packDate>
				</ns2:woolExportDetails>
			</xsl:if>
			<xsl:if test="$commodityType = 'SKN'">
				<ns2:skinsAndHidesExportDetails xmlns:skn="http://agriculture.gov.au/nexdoc/common/rex/SkinsAndHidesTypes_1.0">
					<skn:loadingDate>
						<xsl:value-of select="ScriptNS3:ConvertToDate(key('QuarantineExDocHeaderAddInfo','LoadingDate')/s0:Value,'yyyy-MM-dd')"/>
					</skn:loadingDate>
					<skn:loadingEstablishment>
						<xsl:value-of select="key('QuarantineExDocHeaderAddInfo','LoadingEstablishment')/s0:Value"/>
					</skn:loadingEstablishment>
					<skn:packDate>
						<xsl:value-of select="ScriptNS3:ConvertToDate(key('QuarantineExDocHeaderAddInfo','PackDate')/s0:Value,'yyyy-MM-dd')"/>
					</skn:packDate>
				</ns2:skinsAndHidesExportDetails>
			</xsl:if>
			<xsl:if test="$commodityType = 'IME'">
				<xsl:variable name="temperatureUnit" select="key('QuarantineExDocHeaderAddInfo','TemperatureUnit')/s0:Value"></xsl:variable>
				<xsl:if test="$temperatureUnit != ''">
					<ns2:inedibleMeatExportDetails xmlns:ime="http://agriculture.gov.au/nexdoc/common/rex/InedibleMeatTypes_1.0">
						<ime:transportStorageMinimumTemperature unit="{$temperatureUnit}">
							<xsl:value-of select="key('QuarantineExDocHeaderAddInfo','MinimumTemperature')/s0:Value"></xsl:value-of>
						</ime:transportStorageMinimumTemperature>
					</ns2:inedibleMeatExportDetails>
				</xsl:if>
			</xsl:if>
		</xsl:element>
		<xsl:element name="certificateDetails" namespace="{$namespace}">
			<xsl:variable name="certificatePrintRegion" select="key('QuarantineExDocHeaderAddInfo','CertificateRequiredLocation')/s0:Value"/>
			<xsl:if test="$QHAddInfoGroup">
				<ns2:certificatePrintControls>
					<ns2:certificatePrintIndicator>
						<xsl:value-of select="key('QuarantineExDocHeaderAddInfo','CertificatePrintIndicator')/s0:Value"/>
					</ns2:certificatePrintIndicator>
					<xsl:variable name="printLocation" select="key('QuarantineExDocHeaderAddInfo','PrintLocation')/s0:Value"/>
					<xsl:choose>
						<xsl:when test="$printLocation='AQIS PLACE'">
							<ns2:certificatePrintRegion>
								<xsl:value-of select="$certificatePrintRegion"/>
							</ns2:certificatePrintRegion>
						</xsl:when>
						<xsl:otherwise>
							<xsl:if test="$printLocation='ORGANISATION' or $certificatePrintRegion!=''">
								<ns2:certificateRequiredClientGroup>
									<xsl:value-of select="$certificatePrintRegion"/>
								</ns2:certificateRequiredClientGroup>
							</xsl:if>
						</xsl:otherwise>
					</xsl:choose>
					<xsl:variable name="splitHealthCertByContainer" select="$QHAddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='SplitHealthCertByContainer']"/>
					<xsl:variable name="splitHealthCertByPacker" select="$QHAddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='SplitHealthCertByPacker']"/>
					<xsl:variable name="splitHealthCertByMarks" select="$QHAddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='SplitHealthCertByMarks']"/>
					<ns2:separateBy>
						<xsl:choose>
							<xsl:when test="$splitHealthCertByContainer">CONTAINER</xsl:when>
							<xsl:when test="$splitHealthCertByPacker">PACKING_ESTABLISHMENT</xsl:when>
							<xsl:when test="$splitHealthCertByMarks">SHIPPING_MARK</xsl:when>
						</xsl:choose>
					</ns2:separateBy>
				</ns2:certificatePrintControls>
			</xsl:if>
			<xsl:choose>
				<xsl:when test="not($QLAddInfoGroup and $QLAddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='FormatRequested' and s0:Value/text()!=''] and $certificatePrintRegion!='')">
					<ns2:certificates>
						<ns2:removeExistingSet>true</ns2:removeExistingSet>
					</ns2:certificates>
				</xsl:when>
				<xsl:otherwise>
					<ns2:certificates>
						<xsl:for-each select="$QLAddInfoGroup[count(.|key('certificateLines-group-by-template-and-endorsement',concat(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='FormatRequested']/s0:Value/text(), s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ExtraFormatRequested']/s0:Value/text()))[1])=1]">
							<xsl:variable name="current-grouping-key" select="concat(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='FormatRequested']/s0:Value/text(), s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ExtraFormatRequested']/s0:Value/text())"/>
							<xsl:variable name="current-group" select="key('certificateLines-group-by-template-and-endorsement', $current-grouping-key)"/>
							<xsl:variable name="certificateTemplate" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='FormatRequested']/s0:Value/text()" />
							<xsl:variable name="certificateEndorsement" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ExtraFormatRequested']/s0:Value/text()"/>
							<ns2:certificate>
								<ns2:lineNumbers>
									<xsl:for-each select="$current-group">
										<ns2:lineNumber>
											<xsl:variable name="InvoiceLine" select="../.."/>
											<xsl:value-of select="$InvoiceLine/s0:LineNo/text()"/>
										</ns2:lineNumber>
									</xsl:for-each>
								</ns2:lineNumbers>
								<ns2:certificateDetails>
									<ns2:certificateTemplate>
										<xsl:value-of select="$certificateTemplate"/>
									</ns2:certificateTemplate>
									<ns2:certificateEndorsement>
										<xsl:value-of select="$certificateEndorsement"/>
									</ns2:certificateEndorsement>
								</ns2:certificateDetails>
							</ns2:certificate>
						</xsl:for-each>
					</ns2:certificates>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:element>

		<xsl:element name="productLines" namespace="{$namespace}">
			<xsl:for-each select="$QLAddInfoGroup">
				<xsl:variable name="InvoiceLine" select="../.."/>
				<xsl:variable name="InvoiceLineLineNo" select="$InvoiceLine/s0:LineNo/text()"/>
				<ns2:productLine>
					<ns2:lineNumber>
						<xsl:value-of select="$InvoiceLineLineNo"/>
					</ns2:lineNumber>
					<ns2:productDetails>
						<ns2:productType>
							<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ProductType']/s0:Value/text()"/>
						</ns2:productType>
						<ns2:category>
							<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='Category']/s0:Value/text()"/>
						</ns2:category>
						<ns2:packType>
							<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='PackType']/s0:Value/text()"/>
						</ns2:packType>
						<ns2:preservationType>
							<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='PreservationType']/s0:Value/text()"/>
						</ns2:preservationType>
						<ns2:cutType>
							<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CutCode']/s0:Value/text()"/>
						</ns2:cutType>
						<ns2:suppCode>
							<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='SupplementaryCode']/s0:Value/text()"/>
						</ns2:suppCode>
						<ns2:outerProductPackaging>
							<xsl:variable name="outerPackType" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='OuterPackType']/s0:Value/text()"/>
							<ns2:quantity packageType="{$outerPackType}">
								<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='OuterPackCount']/s0:Value/text()"/>
							</ns2:quantity>
							<xsl:variable name="outerPackWeightUnit" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='OuterPackWeightUnit']/s0:Value/text()"/>
							<ns2:unitAmount unit="{$outerPackWeightUnit}">
								<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='OuterPackWeight']/s0:Value/text()"/>
							</ns2:unitAmount>
							<xsl:variable name="packageMeasureAccuracy" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='OuterPackAccuracy']/s0:Value/text()" />
							<ns2:packageMeasureAccuracy>
								<xsl:choose>
									<xsl:when test="$packageMeasureAccuracy = '3'">A</xsl:when>
									<xsl:when test="$packageMeasureAccuracy = '4'">E</xsl:when>
								</xsl:choose>
							</ns2:packageMeasureAccuracy>
							<ns2:shippingMarks>
								<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ShippingMarks']/s0:Value/text()"/>
							</ns2:shippingMarks>
						</ns2:outerProductPackaging>
						<ns2:intermediateProductPackaging>
							<xsl:variable name="intermediatePackType" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='IntermediatePackType']/s0:Value/text()"/>
							<ns2:quantity packageType="{$intermediatePackType}">
								<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='IntermediatePackCount']/s0:Value/text()"/>
							</ns2:quantity>
							<xsl:variable name="intermediatePackWeightUnit" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='IntermediatePackWeightUnit']/s0:Value/text()"/>
							<ns2:unitAmount unit="{$intermediatePackWeightUnit}">
								<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='IntermediatePackWeight']/s0:Value/text()"/>
							</ns2:unitAmount>
							<xsl:variable name="packageMeasureAccuracy" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='IntermediatePackAccuracy']/s0:Value/text()" />
							<ns2:packageMeasureAccuracy>
								<xsl:choose>
									<xsl:when test="$packageMeasureAccuracy = '3'">A</xsl:when>
									<xsl:when test="$packageMeasureAccuracy = '4'">E</xsl:when>
								</xsl:choose>
							</ns2:packageMeasureAccuracy>
						</ns2:intermediateProductPackaging>
						<ns2:innerProductPackaging>
							<xsl:variable name="innerPackType" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='InnerPackType']/s0:Value/text()"/>
							<ns2:quantity packageType="{$innerPackType}">
								<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='InnerPackCount']/s0:Value/text()"/>
							</ns2:quantity>
							<xsl:variable name="innerPackWeightUnit" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='InnerPackWeightUnit']/s0:Value/text()"/>
							<ns2:unitAmount unit="{$innerPackWeightUnit}">
								<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='InnerPackWeight']/s0:Value/text()"/>
							</ns2:unitAmount>
							<xsl:variable name="packageMeasureAccuracy" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='InnerPackAccuracy']/s0:Value/text()" />
							<ns2:packageMeasureAccuracy>
								<xsl:choose>
									<xsl:when test="$packageMeasureAccuracy = '3'">A</xsl:when>
									<xsl:when test="$packageMeasureAccuracy = '4'">E</xsl:when>
								</xsl:choose>
							</ns2:packageMeasureAccuracy>
						</ns2:innerProductPackaging>
						<xsl:variable name="netQuantityUnit" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='NetQuantityUnit']/s0:Value/text()"/>
						<ns2:netMetricWeight unit="{$netQuantityUnit}">
							<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='NetQuantity']/s0:Value/text()"/>
						</ns2:netMetricWeight>
						<xsl:variable name="imperialNetWeightUnit" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ImperialNetWeightUnit']/s0:Value/text()"/>
						<ns2:netImperialWeight unit="{$imperialNetWeightUnit}">
							<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ImperialNetWeight']/s0:Value/text()"/>
						</ns2:netImperialWeight>
						<ns2:manualCertificateProductDescription>
							<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='InspectionDescription']/s0:Value/text()"/>
						</ns2:manualCertificateProductDescription>
						<ns2:additionalDescription>
							<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='AdditionalProductDescription']/s0:Value/text()"/>
						</ns2:additionalDescription>
						<ns2:farmCode>
							<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='FarmCode']/s0:Value/text()"/>
						</ns2:farmCode>
						<ns2:farmType>
							<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='FarmType']/s0:Value/text()"/>
						</ns2:farmType>
					</ns2:productDetails>

					<xsl:variable name="InvoiceLineLink" select="$InvoiceLine/s0:Link/text()"/>
					<xsl:variable name="RelatedPackingLines" select="/s0:UniversalShipment/s0:Shipment/s0:PackingLineCollection/s0:PackingLine[s0:PackedItemCollection/s0:PackedItem/s0:CommercialInvoiceLineLink=$InvoiceLineLink and s0:ContainerNumber/text()!='']" />
					<xsl:choose>
						<xsl:when test="not($RelatedPackingLines)">
							<ns2:containers>
								<ns2:removeExistingSet>true</ns2:removeExistingSet>
							</ns2:containers>
						</xsl:when>
						<xsl:otherwise>
							<ns2:containers>
								<xsl:for-each select="$RelatedPackingLines">
									<xsl:variable name="ContainerNumber" select="s0:ContainerNumber/text()" />
									<ns2:container>
										<ns2:containerNumber>
											<xsl:value-of select="$ContainerNumber"/>
										</ns2:containerNumber>
										<xsl:variable name="RelatedContainer" select="/s0:UniversalShipment/s0:Shipment/s0:ContainerCollection/s0:Container[s0:ContainerNumber=$ContainerNumber][1]" />
										<xsl:if test="$RelatedContainer">
											<xsl:variable name="SealNumber" select="$RelatedContainer/s0:Seal/text()"/>
											<xsl:variable name="SealStartNumber" select="$RelatedContainer/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='AQISSealStart_Hidden']/s0:Value/text()"/>
											<xsl:variable name="SealEndNumber" select="$RelatedContainer/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='AQISSealEnd_Hidden']/s0:Value/text()"/>
											<xsl:if test="$SealNumber!='' or $SealStartNumber!='' or $SealEndNumber!=''">
												<ns2:containerSeals>
													<ns2:containerSeal>
														<xsl:if test="$SealNumber!=''">
															<ns2:sealNumber>
																<xsl:value-of select="$SealNumber"/>
															</ns2:sealNumber>
														</xsl:if>
														<xsl:if test="$SealStartNumber!=''">
															<ns2:sealStartNumber>
																<xsl:value-of select="$SealStartNumber"/>
															</ns2:sealStartNumber>
														</xsl:if>
														<xsl:if test="$SealEndNumber!=''">
															<ns2:sealEndNumber>
																<xsl:value-of select="$SealEndNumber"/>
															</ns2:sealEndNumber>
														</xsl:if>
													</ns2:containerSeal>
												</ns2:containerSeals>
											</xsl:if>
										</xsl:if>
									</ns2:container>
								</xsl:for-each>
							</ns2:containers>
						</xsl:otherwise>
					</xsl:choose>
					<xsl:variable name="NPDAddInfoGroup" select="s0:AddInfoGroupCollection/s0:AddInfoGroup[s0:Type/s0:Code/text()='NPD']" />
					<xsl:variable name="NPDAddInfoTRGroup" select="$NPDAddInfoGroup[s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TreatmentCode' or s0:Key/text()='TreatmentInformation']/s0:Value/text()!='']" />
					<xsl:variable name="NPDAddInfoHAGroup" select="$NPDAddInfoGroup[s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ProcessingType']/s0:Value/text()='HA']" />
					<xsl:variable name="NPDAddInfoEstablishmentGroup" select="$NPDAddInfoGroup[count(.|$NPDAddInfoTRGroup) != count($NPDAddInfoTRGroup) and count(.|$NPDAddInfoHAGroup) != count($NPDAddInfoHAGroup)]" />
					<xsl:variable name="NPDAddInfoFreeTextEstablishmentGroup" select="$NPDAddInfoEstablishmentGroup[not(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='EstablishmentID']/s0:Value/text()!='') and s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='EstablishmentIndicator']/s0:Value/text()!='']" />
					<xsl:variable name="NPDAddInfoRegisteredEstablishmentGroup" select="$NPDAddInfoEstablishmentGroup[count(.|$NPDAddInfoFreeTextEstablishmentGroup) != count($NPDAddInfoFreeTextEstablishmentGroup)]" />

					<xsl:choose>
						<xsl:when test="not($NPDAddInfoTRGroup)">
							<ns2:treatments>
								<ns2:removeExistingSet>true</ns2:removeExistingSet>
							</ns2:treatments>
						</xsl:when>
						<xsl:otherwise>
							<ns2:treatments>
								<xsl:for-each select="$NPDAddInfoTRGroup">
									<xsl:variable name="treatmentCode" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TreatmentCode']/s0:Value/text()"/>
									<xsl:variable name="treatmentInformation" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TreatmentInformation']/s0:Value/text()"/>
									<xsl:if test="$treatmentCode != '' or $treatmentInformation != ''">
										<ns2:treatmentType>
											<xsl:if test="$treatmentCode != ''">
												<ns2:treatmentCode>
													<xsl:value-of select="$treatmentCode"/>
												</ns2:treatmentCode>
												<ns2:treatmentStartDate>
													<xsl:value-of select="ScriptNS3:ConvertToDate(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='StartDate']/s0:Value/text(),'yyyy-MM-dd')"/>
												</ns2:treatmentStartDate>
												<ns2:treatmentEndDate>
													<xsl:value-of select="ScriptNS3:ConvertToDate(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='EndDate']/s0:Value/text(),'yyyy-MM-dd')"/>
												</ns2:treatmentEndDate>
											</xsl:if>
											<xsl:if test="$treatmentInformation != ''">
												<ns2:treatmentInformation>
													<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TreatmentInformation']/s0:Value/text()"/>
												</ns2:treatmentInformation>
											</xsl:if>
										</ns2:treatmentType>
									</xsl:if>
								</xsl:for-each>
							</ns2:treatments>
						</xsl:otherwise>
					</xsl:choose>
					<xsl:choose>
						<xsl:when test="not($NPDAddInfoRegisteredEstablishmentGroup)">
							<ns2:productionProcesses>
								<ns2:removeExistingSet>true</ns2:removeExistingSet>
							</ns2:productionProcesses>
						</xsl:when>
						<xsl:otherwise>
							<ns2:productionProcesses>
								<xsl:for-each select="$NPDAddInfoRegisteredEstablishmentGroup">
									<xsl:variable name="treatmentCode" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TreatmentCode']/s0:Value/text()"/>
									<xsl:variable name="treatmentInformation" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TreatmentInformation']/s0:Value/text()"/>
									<xsl:if test="not($treatmentCode != '') and not($treatmentInformation != '')">
										<ns2:productionProcess>
											<ns2:processingStartDate>
												<xsl:value-of select="ScriptNS3:ConvertToDate(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='StartDate']/s0:Value/text(),'yyyy-MM-dd')"/>
											</ns2:processingStartDate>
											<ns2:processingEndDate>
												<xsl:value-of select="ScriptNS3:ConvertToDate(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='EndDate']/s0:Value/text(),'yyyy-MM-dd')"/>
											</ns2:processingEndDate>
											<xsl:variable name="establishmentIndicator" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='EstablishmentIndicator']/s0:Value/text()"/>
											<ns2:establishmentIndicator>
												<xsl:choose>
													<xsl:when test="$establishmentIndicator != ''">
														<xsl:value-of select="$establishmentIndicator"/>
													</xsl:when>
													<xsl:otherwise>PC</xsl:otherwise>
												</xsl:choose>
											</ns2:establishmentIndicator>
											<xsl:variable name="processingEstablishmentNumber" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='EstablishmentID']/s0:Value/text()" />
											<xsl:if test="$processingEstablishmentNumber != ''">
												<ns2:processingEstablishmentNumber>
													<xsl:value-of select="$processingEstablishmentNumber"/>
												</ns2:processingEstablishmentNumber>
											</xsl:if>
											<ns2:removeEntry>
												<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='RemoveEntry']/s0:Value/text()"/>
											</ns2:removeEntry>
										</ns2:productionProcess>
									</xsl:if>
								</xsl:for-each>
							</ns2:productionProcesses>
						</xsl:otherwise>
					</xsl:choose>
					<ns2:sew>
						<xsl:variable name="AIQSCustomsWtUQ_Hidden" select="$InvoiceLine/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='AQISCustomsWtUQ_Hidden']/s0:Value/text()"/>
						<ns2:netCustomsWeight unit="{$AIQSCustomsWtUQ_Hidden}">
							<xsl:value-of select="$InvoiceLine/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='AQISCustomsWt_Hidden']/s0:Value/text()"/>
						</ns2:netCustomsWeight>
						<xsl:variable name="innerPackWeightUnit" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='GrossMetricWeightUnit']/s0:Value/text()"/>
						<ns2:grossMetricWeight unit="{$innerPackWeightUnit}">
							<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='GrossMetricWeight']/s0:Value/text()"/>
						</ns2:grossMetricWeight>
						<xsl:if test="$invoiceCurrency != ''">
							<ns2:fobAmount>
								<xsl:value-of select="userCSharp:GetRoundStr($InvoiceLine/s0:CustomsValue/text())"/>
							</ns2:fobAmount>
						</xsl:if>
						<ns2:productSourceState>
							<xsl:value-of select="$InvoiceLine/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='AUState_Hidden']/s0:Value/text()"/>
						</ns2:productSourceState>
						<ns2:relatedPermitType>
							<xsl:value-of select="$InvoiceLine/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='RelatedExportPermitAuthority_Hidden']/s0:Value/text()"/>
						</ns2:relatedPermitType>
						<ns2:relatedPermitNumber>
							<xsl:value-of select="$InvoiceLine/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='RelatedExportPermitNumber_Hidden']/s0:Value/text()"/>
						</ns2:relatedPermitNumber>
						<ns2:relatedPermitDate>
							<xsl:value-of select="ScriptNS3:ConvertToDate($InvoiceLine/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='RelatedExportPermitDate_Hidden']/s0:Value/text(), 'yyyy-MM-dd')"/>
						</ns2:relatedPermitDate>
					</ns2:sew>
					<ns2:durabilityStartDate>
						<xsl:value-of select="ScriptNS3:ConvertToDate(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='UseByStart']/s0:Value/text(),'yyyy-MM-dd')"/>
					</ns2:durabilityStartDate>
					<ns2:durabilityEndDate>
						<xsl:value-of select="ScriptNS3:ConvertToDate(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='UseByEnd']/s0:Value/text(),'yyyy-MM-dd')"/>
					</ns2:durabilityEndDate>
					<ns2:aheccCode>
						<xsl:value-of select="userCSharp:RemoveDot($InvoiceLine/s0:HarmonisedCode/text())"/>
					</ns2:aheccCode>
					<xsl:if test="$IsEU">
						<ns2:cnCode>
							<xsl:value-of select="userCSharp:RemoveDot(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CombinedNomenclature']/s0:Value/text())"/>
						</ns2:cnCode>
						<ns2:finalConsumerFlag>
							<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='FinalConsumer']/s0:Value/text()"/>
						</ns2:finalConsumerFlag>
					</xsl:if>
					<ns2:batchCode>
						<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='BatchCode']/s0:Value/text()"/>
					</ns2:batchCode>
					<ns2:additionalTexts>
						<xsl:variable name="statementNumbers" select="self::node()/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='StatementNumber1' or s0:Key/text()='StatementNumber2' or s0:Key/text()='StatementNumber3' or s0:Key/text()='StatementNumber4' or s0:Key/text()='StatementNumber5']"/>
						<ns2:additionalText>
							<xsl:if test ="count($statementNumbers) > 0">
								<ns2:code>ZZZ</ns2:code>
							</xsl:if>
							<xsl:for-each select="$statementNumbers">
								<xsl:variable name="addInfoValue" select="self::node()/s0:Value/text()"/>
								<xsl:call-template name="SplitTexts">
									<xsl:with-param name="AdditionalText" select="$addInfoValue"/>
								</xsl:call-template>
							</xsl:for-each>
						</ns2:additionalText>
						<xsl:for-each select="self::node()/s0:AddInfoCollection/s0:AddInfo">
							<xsl:variable name="addInfoKey" select="self::node()/s0:Key/text()"/>
							<xsl:variable name="addInfoValue" select="self::node()/s0:Value/text()"/>
							<ns2:additionalText>
								<xsl:call-template name="GetAdditionalTextFromAddInfo">
									<xsl:with-param name="KeyForCode" select="$addInfoKey"/>
									<xsl:with-param name="AdditionalText" select="$addInfoValue"/>
								</xsl:call-template>
							</ns2:additionalText>
						</xsl:for-each>
					</ns2:additionalTexts>
					<xsl:variable name="productSource" select="key('QuarantineExDocHeaderAddInfo','ProductSource')/s0:Value"/>
					<xsl:choose>
						<xsl:when test="not($productSource!='')">
							<ns2:productSourceCountries>
								<ns2:removeExistingSet>true</ns2:removeExistingSet>
							</ns2:productSourceCountries>
						</xsl:when>
						<xsl:otherwise>
							<ns2:productSourceCountries>
								<ns2:productSourceCountry>
									<xsl:value-of select="$productSource"/>
								</ns2:productSourceCountry>
							</ns2:productSourceCountries>
						</xsl:otherwise>
					</xsl:choose>
					<xsl:variable name="QLAAddInfoGroup" select="$InvoiceLine/s0:AddInfoGroupCollection/s0:AddInfoGroup[s0:Type/s0:Code/text()='QLA']"/>
					<xsl:if test="$Attachments and $QLAAddInfoGroup">
						<ns2:attachments>
							<xsl:for-each select="$QLAAddInfoGroup">
								<xsl:variable name="fileName" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='FileName']/s0:Value/text()"/>
								<ns1:attachment>
									<ns1:name>
										<xsl:value-of select="$fileName"/>
									</ns1:name>
									<ns1:attachmentType>
										<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='AttachmentType']/s0:Value/text()"/>
									</ns1:attachmentType>
									<ns1:description>
										<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='Description']/s0:Value/text()"/>
									</ns1:description>
									<ns1:mimeType>
										<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='mimeType']/s0:Value/text()"/>
									</ns1:mimeType>
									<ns1:data>
										<xsl:value-of select="$Attachments/s0:AttachedDocument[s0:FileName/text()=$fileName]/s0:ImageData/text()"/>
									</ns1:data>
								</ns1:attachment>
							</xsl:for-each>
						</ns2:attachments>
					</xsl:if>
					<ns2:importAuthorityCode>
						<xsl:value-of select="$InvoiceLine/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ImportAuthorityCode_Hidden']/s0:Value/text()"/>
					</ns2:importAuthorityCode>
					<xsl:choose>
						<xsl:when test="$commodityType = 'SKN'">
							<ns2:skinsAndHidesProductLineDetails xmlns:skn="http://agriculture.gov.au/nexdoc/common/rex/SkinsAndHidesTypes_1.0">
								<skn:saltingDate>
									<xsl:value-of select="ScriptNS3:ConvertToDate(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='SaltingDate']/s0:Value/text(),'yyyy-MM-dd')"/>
								</skn:saltingDate>
							</ns2:skinsAndHidesProductLineDetails>
						</xsl:when>
						<xsl:otherwise>
							<ns2:fishProductLineDetails xmlns:fis="http://agriculture.gov.au/nexdoc/common/rex/FishTypes_1.0">
								<fis:catchStartDate>
									<xsl:value-of select="ScriptNS3:ConvertToDate(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CatchStartDate']/s0:Value/text(),'yyyy-MM-dd')"/>
								</fis:catchStartDate>
								<fis:catchEndDate>
									<xsl:value-of select="ScriptNS3:ConvertToDate(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CatchEndDate']/s0:Value/text(),'yyyy-MM-dd')"/>
								</fis:catchEndDate>
								<fis:fishWaterIndicator>
									<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='FishWaterIndicator']/s0:Value/text()"/>
								</fis:fishWaterIndicator>
								<fis:fishEstablishments>
									<xsl:if test="$NPDAddInfoFreeTextEstablishmentGroup">
										<xsl:for-each select="$NPDAddInfoFreeTextEstablishmentGroup">
											<xsl:variable name="FreeTextAddInfoCollection" select="s0:AddInfoCollection" />
											<xsl:variable name="FreeTextAddInfoOrganizationAddress" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='AQISProcessingEstablishment']" />
											<fis:freeTextEstablishment>
												<fis:startDate>
													<xsl:value-of select="ScriptNS3:ConvertToDate($FreeTextAddInfoCollection/s0:AddInfo[s0:Key/text()='StartDate']/s0:Value/text(),'yyyy-MM-dd')" />
												</fis:startDate>
												<fis:endDate>
													<xsl:value-of select="ScriptNS3:ConvertToDate($FreeTextAddInfoCollection/s0:AddInfo[s0:Key/text()='EndDate']/s0:Value/text(),'yyyy-MM-dd')" />
												</fis:endDate>
												<fis:establishmentIndicator>
													<xsl:value-of select="$FreeTextAddInfoCollection/s0:AddInfo[s0:Key/text()='EstablishmentIndicator']/s0:Value/text()" />
												</fis:establishmentIndicator>
												<fis:establishmentName>
													<xsl:value-of select="$FreeTextAddInfoOrganizationAddress/s0:CompanyName/text()" />
												</fis:establishmentName>
												<fis:postCode>
													<xsl:value-of select="$FreeTextAddInfoOrganizationAddress/s0:Postcode/text()" />
												</fis:postCode>
												<fis:removeEntry>
													<xsl:value-of select="$FreeTextAddInfoCollection/s0:AddInfo[s0:Key/text()='RemoveEntry']/s0:Value/text()" />
												</fis:removeEntry>
											</fis:freeTextEstablishment>
										</xsl:for-each>
									</xsl:if>
									<xsl:if test="$NPDAddInfoHAGroup">
										<xsl:for-each select="$NPDAddInfoHAGroup">
											<xsl:variable name="HarvestAreaAddInfo" select="s0:AddInfoCollection" />
											<fis:harvestArea>
												<xsl:variable name="onshore" select="$HarvestAreaAddInfo/s0:AddInfo[s0:Key/text()='HarvestArea']/s0:Value/text()!=''" />
												<xsl:choose>
													<xsl:when test="$onshore">
														<fis:harvestAreaDetails>
															<fis:harvestAreaDate>
																<xsl:value-of select="ScriptNS3:ConvertToDate($HarvestAreaAddInfo/s0:AddInfo[s0:Key/text()='StartDate']/s0:Value/text(),'yyyy-MM-dd')" />
															</fis:harvestAreaDate>
															<fis:harvestAreaName>
																<xsl:value-of select="$HarvestAreaAddInfo/s0:AddInfo[s0:Key/text()='HarvestArea']/s0:Value/text()" />
															</fis:harvestAreaName>
															<fis:leaseNumber>
																<xsl:value-of select="$HarvestAreaAddInfo/s0:AddInfo[s0:Key/text()='LeaseNumber']/s0:Value/text()" />
															</fis:leaseNumber>
														</fis:harvestAreaDetails>
													</xsl:when>
													<xsl:otherwise>
														<fis:offshore>
															<fis:startDate>
																<xsl:value-of select="ScriptNS3:ConvertToDate($HarvestAreaAddInfo/s0:AddInfo[s0:Key/text()='StartDate']/s0:Value/text(),'yyyy-MM-dd')" />
															</fis:startDate>
															<fis:endDate>
																<xsl:value-of select="ScriptNS3:ConvertToDate($HarvestAreaAddInfo/s0:AddInfo[s0:Key/text()='EndDate']/s0:Value/text(),'yyyy-MM-dd')" />
															</fis:endDate>
														</fis:offshore>
													</xsl:otherwise>
												</xsl:choose>
												<fis:depuration>
													<fis:startDate>
														<xsl:value-of select="ScriptNS3:ConvertToDate($HarvestAreaAddInfo/s0:AddInfo[s0:Key/text()='DepurationDate']/s0:Value/text(),'yyyy-MM-dd')" />
													</fis:startDate>
													<fis:establishmentNumber>
														<xsl:value-of select="$HarvestAreaAddInfo/s0:AddInfo[s0:Key/text()='EstablishmentID']/s0:Value/text()" />
													</fis:establishmentNumber>
												</fis:depuration>
												<fis:removeEntry>
													<xsl:value-of select="$HarvestAreaAddInfo/s0:AddInfo[s0:Key/text()='RemoveEntry']/s0:Value/text()" />
												</fis:removeEntry>
											</fis:harvestArea>
										</xsl:for-each>
									</xsl:if>
								</fis:fishEstablishments>
							</ns2:fishProductLineDetails>
						</xsl:otherwise>
					</xsl:choose>
					<ns2:natureOfCommodity>
						<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='NatureOfCommodity']/s0:Value/text()"/>
					</ns2:natureOfCommodity>
					<ns2:euTreatmentType>
						<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TreatmentType']/s0:Value/text()"/>
					</ns2:euTreatmentType>
					<ns2:quotaExporter>
						<xsl:value-of select="$InvoiceLine/../../s0:Supplier/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code/text()='QEN' and s0:CountryOfIssue/s0:Code/text()='AU']/s0:Value/text()"/>
					</ns2:quotaExporter>
				</ns2:productLine>
				<xsl:if test="position()=last()">
					<ns2:retainProductLines>true</ns2:retainProductLines>
				</xsl:if>
			</xsl:for-each>
		</xsl:element>
		<xsl:element name="manufacturers"  namespace="{$namespace}">
			<xsl:variable name="manufacturerOrganizationAddresses" select="s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice/s0:CommercialInvoiceLineCollection/s0:CommercialInvoiceLine/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='Manufacturer']"/>
			<xsl:variable name="commercialInvoiceLines" select="s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice/s0:CommercialInvoiceLineCollection/s0:CommercialInvoiceLine"/>
			<xsl:for-each select="$commercialInvoiceLines[generate-id() =  generate-id(key('manufacturer', s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='Manufacturer']))]">
				<ns2:manufacturer>
					<ns2:lineNumbers>
						<xsl:for-each select="key('manufacturer', s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='Manufacturer'])">
							<ns2:lineNumber>
								<xsl:value-of select="s0:LineNo/text()"/>
							</ns2:lineNumber>
						</xsl:for-each>
					</ns2:lineNumbers>
					<ns2:manufacturerDetails>
						<xsl:variable name="manufacturerOrg" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='Manufacturer']"/>
						<ns2:name>
							<xsl:value-of select="$manufacturerOrg/s0:CompanyName/text()"/>
						</ns2:name>
						<ns2:address>
							<ns1:streetAddress>
								<ns1:streetLine>
									<xsl:value-of select="$manufacturerOrg/s0:Address1/text()"/>
								</ns1:streetLine>
								<ns1:streetLine>
									<xsl:value-of select="$manufacturerOrg/s0:Address2/text()"/>
								</ns1:streetLine>
							</ns1:streetAddress>
							<ns1:city>
								<xsl:value-of select="$manufacturerOrg/s0:City/text()"/>
							</ns1:city>
							<ns1:state>
								<xsl:value-of select="$manufacturerOrg/s0:State/text()"/>
							</ns1:state>
							<ns1:country>
								<xsl:value-of select="$manufacturerOrg/s0:Country/s0:Code/text()"/>
							</ns1:country>
							<ns1:postalCode>
								<xsl:value-of select="$manufacturerOrg/s0:Postcode/text()"/>
							</ns1:postalCode>
						</ns2:address>
					</ns2:manufacturerDetails>
				</ns2:manufacturer>
			</xsl:for-each>
		</xsl:element>

		<xsl:variable name="QHAAddInfoGroup" select="s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice/s0:AddInfoGroupCollection/s0:AddInfoGroup[s0:Type/s0:Code/text()='QHA']"/>
		<xsl:if test="$Attachments and $QHAAddInfoGroup">
			<xsl:element name="attachments" namespace="{$namespace}">
				<xsl:for-each select="$QHAAddInfoGroup">
					<xsl:variable name="fileName" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='FileName']/s0:Value/text()"/>
					<ns1:attachment>
						<ns1:name>
							<xsl:value-of select="$fileName"/>
						</ns1:name>
						<ns1:attachmentType>
							<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='AttachmentType']/s0:Value/text()"/>
						</ns1:attachmentType>
						<ns1:description>
							<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='Description']/s0:Value/text()"/>
						</ns1:description>
						<ns1:mimeType>
							<xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='mimeType']/s0:Value/text()"/>
						</ns1:mimeType>
						<ns1:data>
							<xsl:value-of select="$Attachments/s0:AttachedDocument[s0:FileName/text()=$fileName]/s0:ImageData/text()"/>
						</ns1:data>
					</ns1:attachment>
				</xsl:for-each>
			</xsl:element>
		</xsl:if>

		<xsl:element name="additionalTexts" namespace="{$namespace}">
			<xsl:for-each select="s0:NoteCollection/s0:Note">
				<xsl:variable name="noteDescription" select="self::node()/s0:Description/text()"/>
				<xsl:variable name="noteText" select="self::node()/s0:NoteText/text()"/>
				<ns2:additionalText>
					<xsl:call-template name="GetAdditionalTextFromNote">
						<xsl:with-param name="KeyForCode" select="$noteDescription"/>
						<xsl:with-param name="AdditionalText" select="$noteText"/>
					</xsl:call-template>
				</ns2:additionalText>
			</xsl:for-each>
			<xsl:for-each select="$QHAddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CatchZone']">
				<xsl:variable name="addInfoValue" select="self::node()/s0:Value/text()"/>
				<ns2:additionalText>
					<ns2:code>OCZ</ns2:code>
					<xsl:call-template name="SplitTexts">
						<xsl:with-param name="AdditionalText" select="$addInfoValue"/>
					</xsl:call-template>
				</ns2:additionalText>
			</xsl:for-each>
		</xsl:element>

		<xsl:variable name="ResponsiblePersonAddress" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='AQISResponsiblePerson']"/>
		<xsl:variable name="PersonName" select="normalize-space($ResponsiblePersonAddress/s0:Contact)"/>
		<xsl:variable name="TransitDestinationAddress" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='AQISTransitDestination']"/>
		<xsl:if test="($ResponsiblePersonAddress and $PersonName != '') or $TransitDestinationAddress">
			<xsl:element name="euTransit" namespace="{$namespace}">
				<xsl:if test="$ResponsiblePersonAddress and $PersonName != ''">
					<ns2:personResponsible>
						<ns2:address>
							<xsl:call-template name="GetAddressTypeNode">
								<xsl:with-param name="OrganizationAddress" select="$ResponsiblePersonAddress"/>
							</xsl:call-template>
						</ns2:address>
						<ns2:firstName>
							<xsl:value-of select="userCSharp:GetFirstName($PersonName)"/>
						</ns2:firstName>
						<ns2:lastName>
							<xsl:value-of select="userCSharp:GetLastName($PersonName)"/>
						</ns2:lastName>
						<ns2:phoneNumber>
							<xsl:variable name ="ResponsiblePersonPhone" select="$ResponsiblePersonAddress/s0:Phone"/>
							<xsl:choose>
								<xsl:when test="$ResponsiblePersonPhone != ''">
									<xsl:value-of select="$ResponsiblePersonPhone"/>
								</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="$ResponsiblePersonAddress/s0:Mobile"/>
								</xsl:otherwise>
							</xsl:choose>
						</ns2:phoneNumber>
					</ns2:personResponsible>
				</xsl:if>
				<xsl:if test="$TransitDestinationAddress">
					<ns2:placeOfDestinationDetails>
						<ns2:name>
							<xsl:value-of select="$TransitDestinationAddress/s0:CompanyName"/>
						</ns2:name>
						<ns2:address>
							<xsl:call-template name="GetAddressTypeNode">
								<xsl:with-param name="OrganizationAddress" select="$TransitDestinationAddress"/>
							</xsl:call-template>
						</ns2:address>
						<ns2:phoneNumber>
							<xsl:variable name ="TransitDestinationPhone" select="$TransitDestinationAddress/s0:Phone"/>
							<xsl:choose>
								<xsl:when test="$TransitDestinationPhone != ''">
									<xsl:value-of select="$TransitDestinationPhone"/>
								</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="$TransitDestinationAddress/s0:Mobile"/>
								</xsl:otherwise>
							</xsl:choose>
						</ns2:phoneNumber>
						<ns2:approvalNumber>
							<xsl:value-of select="key('QuarantineExDocHeaderAddInfo', 'ApprovalNumber')/s0:Value" />
						</ns2:approvalNumber>
						<ns2:transitLocationType>
							<xsl:value-of select="key('QuarantineExDocHeaderAddInfo', 'TransitLocationType')/s0:Value" />
						</ns2:transitLocationType>
					</ns2:placeOfDestinationDetails>
				</xsl:if>
			</xsl:element>
		</xsl:if>

		<xsl:if test="$MstType='AMEND'">
			<xsl:variable name="SubmitAmendmentRequest" select="$QHAddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='SubmitAmendmentRequest']/s0:Value/text()='Y'"/>
			<xsl:if test="$SubmitAmendmentRequest='Y'">
				<xsl:element name="submitAmendmentRequest" namespace="{$namespace}">true</xsl:element>
				<xsl:element name="amendmentReason" namespace="{$namespace}">
					<xsl:value-of select="$QHAddInfoGroup/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='RequestAmendReason']/s0:Value/text()"/>
				</xsl:element>
			</xsl:if>
		</xsl:if>
	</xsl:template>


	<xsl:key name="certificateLines-group-by-template-and-endorsement" match="s0:AddInfoGroup" use="concat(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='FormatRequested']/s0:Value/text(), s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ExtraFormatRequested']/s0:Value/text())"/>
	<xsl:key name="importPermits-group-by-number-and-date" match="s0:AddInfoCollection" use="concat(s0:AddInfo[s0:Key/text()='TemporaryImportNumbers_Hidden']/s0:Value/text(), s0:AddInfo[s0:Key/text()='TemporaryImportDate_Hidden']/s0:Value/text())"/>
	<xsl:key name="ackCustomsReferences-group-by-Reference" match="s0:CustomsReference" use="s0:Reference/text()"/>


	<xsl:template name="SplitTexts">
		<xsl:param name="AdditionalText"/>
		<xsl:variable name="newLine" select="userCSharp:GetNewLine()" />
		<xsl:choose>
			<xsl:when test="contains($AdditionalText, $newLine)">
				<xsl:call-template name="SplitTextsShort">
					<xsl:with-param name="AdditionalText" select="substring-before($AdditionalText, $newLine)"/>
				</xsl:call-template>
				<xsl:call-template name="SplitTexts">
					<xsl:with-param name="AdditionalText" select="substring-after($AdditionalText, $newLine)"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="SplitTextsShort">
					<xsl:with-param name="AdditionalText" select="$AdditionalText"/>
				</xsl:call-template>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="SplitTextsShort">
		<xsl:param name="AdditionalText"/>
		<xsl:choose>
			<xsl:when test="string-length($AdditionalText) > 50">
				<ns2:text>
					<xsl:value-of select="substring($AdditionalText, 1, 50)"/>
				</ns2:text>
				<xsl:call-template name="SplitTextsShort">
					<xsl:with-param name="AdditionalText" select="normalize-space(substring($AdditionalText, 51))"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<ns2:text>
					<xsl:value-of select="normalize-space($AdditionalText)"/>
				</ns2:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="GetAdditionalTextFromNote">
		<xsl:param name="KeyForCode"/>
		<xsl:param name="AdditionalText"/>
		<xsl:choose>
			<xsl:when test="$KeyForCode = 'EXDOC Notify Text'">
				<ns2:code>AAG</ns2:code>
				<xsl:call-template name="SplitTexts">
					<xsl:with-param name="AdditionalText" select="$AdditionalText"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:when test="$KeyForCode = 'EXDOC Letter Of Credit'">
				<ns2:code>AAW</ns2:code>
				<xsl:call-template name="SplitTexts">
					<xsl:with-param name="AdditionalText" select="$AdditionalText"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:when test="$KeyForCode = 'EXDOC Additional Information'">
				<ns2:code>ACB</ns2:code>
				<xsl:call-template name="SplitTexts">
					<xsl:with-param name="AdditionalText" select="$AdditionalText"/>
				</xsl:call-template>
			</xsl:when>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="GetAdditionalTextFromAddInfo">
		<xsl:param name="KeyForCode"/>
		<xsl:param name="AdditionalText"/>
		<xsl:choose>
			<xsl:when test="$KeyForCode = 'AdditionalDeclarationComments'">
				<ns2:code>AAZ</ns2:code>
				<xsl:call-template name="SplitTexts">
					<xsl:with-param name="AdditionalText" select="$AdditionalText"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:when test="$KeyForCode = 'StatementText'">
				<ns2:code>AAY</ns2:code>
				<xsl:call-template name="SplitTexts">
					<xsl:with-param name="AdditionalText" select="$AdditionalText"/>
				</xsl:call-template>
			</xsl:when>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="GetAddressTypeNode">
		<xsl:param name="OrganizationAddress"/>
		<ns1:streetAddress>
			<ns1:streetLine>
				<xsl:variable name="Address1">
					<xsl:value-of select="$OrganizationAddress/s0:Address1"/>
				</xsl:variable>
				<xsl:choose>
					<xsl:when test="$Address1">
						<xsl:value-of select="$Address1"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="$OrganizationAddress/s0:Address2"/>
					</xsl:otherwise>
				</xsl:choose>
			</ns1:streetLine>
		</ns1:streetAddress>
		<ns1:city>
			<xsl:value-of select="$OrganizationAddress/s0:City"/>
		</ns1:city>
		<ns1:state>
			<xsl:choose>
				<xsl:when test="$OrganizationAddress/s0:State/@Description != ''">
					<xsl:value-of select="$OrganizationAddress/s0:State/@Description"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$OrganizationAddress/s0:State"/>
				</xsl:otherwise>
			</xsl:choose>
		</ns1:state>
		<ns1:country>
			<xsl:value-of select="$OrganizationAddress/s0:Country/s0:Code"/>
		</ns1:country>
		<ns1:postalCode>
			<xsl:value-of select="$OrganizationAddress/s0:Postcode"/>
		</ns1:postalCode>
	</xsl:template>

	<msxsl:script language="C#" implements-prefix="userCSharp">
		<![CDATA[
	public string If(bool test, string forThen, string forElse) {
		return test ? forThen : forElse;
	}

    public string GetMst(string eventReference)
    {
        return ParseEventReference(eventReference, "MST");
    }

    public bool IsEU(string eventReference)
    {
        return string.Equals(GetLoc(eventReference), "EU", StringComparison.OrdinalIgnoreCase);
    }

    string GetLoc(string eventReference)
    {
        return ParseEventReference(eventReference, "LOC");
    }

    string ParseEventReference(string eventReference, string key)
    {
        var splitted = eventReference.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in splitted)
        {
            var keyValue = part.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
            if (keyValue.Length == 2)
            {
                if (string.Equals(keyValue[0].Trim(), key, StringComparison.OrdinalIgnoreCase))
                    return keyValue[1].Trim();
            }
        }
        return string.Empty;
    }

    public void ThrowInvalidMstType(string mstType)
    {
      throw new System.ArgumentException(mstType + " MST Type is invalid. Could not find the related maping with this EventReference.");
    }
    
    public string RemoveDot(string value)
    {
      return value.Replace(".", "");
    }

    public string GetFirstName(string name)
    {
      var splitted = name.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
      return splitted.Length > 0 ? splitted[0] : string.Empty;
    }

    public string GetLastName(string name)
    {
      var splitted = name.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
      return splitted.Length > 1 ? splitted[splitted.Length - 1] : string.Empty;
    }
    
    public string GetNewLine()
    {
      return "\n";
    }

    public string IsNull(string expression, string condidate)
    {
      return !string.IsNullOrWhiteSpace(expression) ? expression : condidate;
    }

    public string GetRoundStr(string input)
    {
      decimal d;
      var result = decimal.TryParse(input, out d) ? d : 0m;
      return Math.Round(result).ToString();
    }

    public decimal GetMappedWeight(decimal weight, string uom)
    {
        return uom.ToUpper() == "MG" ? weight / 1000 : weight;
    }

    public string GetMappedWeightUnit(string uom)
    {
        switch (uom.ToUpper())
        {
            case "DT":
                return "DTN";
            case "G":
            case "MG":
                return "GRM";
            case "HG":
                return "HGM";
            case "KG":
                return "KGM";
            case "OZ":
                return "ONZ";
            case "T":
                return "TNE";
            case "TN":
                return "STN";
            default:
                return uom.ToUpper();
        }
    }
]]>
	</msxsl:script>
</xsl:stylesheet>

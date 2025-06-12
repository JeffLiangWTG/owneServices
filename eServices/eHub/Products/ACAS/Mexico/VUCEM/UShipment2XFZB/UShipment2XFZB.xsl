<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xsd="http://www.w3.org/2001/XMLSchema"
    xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
    xsi:schemaLocation="iata:housewaybill:1 HouseWaybill_1.xsd"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt"
    exclude-result-prefixes="msxsl s0 ns0 CodeMapper ContextAccessor DataModelAccessor DateMapper StringMapper UnitConverter userCSharp"
    version="1.0"
    xmlns:s0="http://www.cargowise.com/Schemas/Universal/2012/11"
    xmlns:ns0="mx:iata:housewaybill:1"
    xmlns:ccts="urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"
    xmlns:udt="urn:un:unece:uncefact:data:standard:UnqualifiedDataType:8"
    xmlns:ram="iata:datamodel:3"
    xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
    xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
    xmlns:DataModelAccessor="http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"
    xmlns:UnitConverter="http://schemas.microsoft.com/BizTalk/2003/UnitConverter"
    xmlns:DateMapper="http://schemas.microsoft.com/BizTalk/2003/DateMapper"
    xmlns:StringMapper="http://schemas.microsoft.com/BizTalk/2003/StringMapper"
    xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">

  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes"/>

  <xsl:variable name="recipientID" select="ContextAccessor:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="senderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>


  <xsl:template match="/">
    <xsl:apply-templates select="s0:UniversalShipment/s0:Shipment"/>
  </xsl:template>

  <xsl:template match="s0:UniversalShipment/s0:Shipment">
    <xsl:variable name="ACASRecipientID">
      <xsl:choose>
        <xsl:when test="contains($recipientID, 'TST')">ACAS_MXTest</xsl:when>
        <xsl:otherwise>ACAS_MX</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="serviceProviderMSGID" select="'ACASMX'"/>

    <xsl:variable name="waybillNumber" select="userCSharp:ToUpper(s0:WayBillNumber/text())"/>
    <xsl:variable name="fileSenderID" select="DataModelAccessor:GetClientRegistrationCode($senderID, s0:DataContext/s0:Workflow/s0:EventBranch/text(), $ACASRecipientID)"/>
    <xsl:variable name="operationPortCode" select="s0:PortOfOrigin/text()"/>
    <xsl:variable name="shipmentID" select="s0:DataContext/s0:DataSource/s0:Key/text()"/>
    <xsl:variable name="purpose" select="s0:DataContext/s0:DocumentaryOverride/s0:Purpose/text()"/>
    <xsl:variable name="documentName" select="s0:DataContext/s0:DocumentaryOverride/s0:DocumentName/text()"/>

    <xsl:variable name="interchangeID" select="CodeMapper:CallActionProcedureHelper('GetCounterValue','@value','@name','CargoWise.eHub.Products.ACAS.MX.VUCEM.Interchange','@maxlength','7')"/>

    <xsl:variable name="previousMessageIdentifier" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $ACASRecipientID, '@recipientId', $senderID , '@ST_ID', $serviceProviderMSGID , '@value', $shipmentID, '@referenceType', 'XFZB')"/>
    <xsl:variable name="messageIdentifier">
      <xsl:choose>
        <xsl:when test="$previousMessageIdentifier != ''">
          <xsl:value-of select="$previousMessageIdentifier"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:variable name="formattedMessageID" select ="concat('FZB', format-number($interchangeID, '0000000'))"/>
          <xsl:variable name="subscribeJobNumber1" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $ACASRecipientID, $senderID, $formattedMessageID, $shipmentID, 'XFZB')"/>
          <xsl:variable name="subscribeJobNumber2" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $ACASRecipientID, $senderID, $shipmentID, $formattedMessageID, 'XFZB')"/>
          <xsl:value-of select="$formattedMessageID"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="subscribeForwardingType" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $ACASRecipientID, $senderID, $messageIdentifier, s0:DataContext/s0:DataSource/s0:Type/text(), 'ForwardingType')"/>
    <xsl:variable name="subscribeActionPurpose" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $ACASRecipientID, $senderID, $messageIdentifier, $purpose, 'Purpose')"/>
    <xsl:variable name="subscribeDocumentName" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $ACASRecipientID, $senderID, $messageIdentifier, $documentName, 'DocumentName')"/>
    <xsl:if test="$shipmentID != $waybillNumber">
      <xsl:variable name="subscribeWaybillNumber" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $ACASRecipientID, $senderID, $messageIdentifier, $waybillNumber, 'FZB-HWB')" />
    </xsl:if>
    <xsl:variable name="subscribeClientID" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $ACASRecipientID, $senderID, $fileSenderID)"/>
    <xsl:variable name="subscribeOperationPort" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $ACASRecipientID, $senderID, $messageIdentifier, $operationPortCode, '')" />

    <xsl:variable name="InboxPK" select="ContextAccessor:GetContextProperty('InternalTrackingID', 'http://cargowise.com/ehub/tracking/2010/06')"/>
    <xsl:variable name="SubscribeInboxPK" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $ACASRecipientID, $senderID, $InboxPK, $interchangeID)"/>
    <xsl:variable name="FileName" select="ContextAccessor:SetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06', concat('VUCEM_XFZB_', $senderID, '_', $interchangeID))"/>
    <xsl:variable name="EmailSubject" select="ContextAccessor:SetContextProperty('OverrideEmailSubject', 'http://cargowise.com/ehub/processing/2010/06', concat('ACAS_', $ACASRecipientID, '_', $operationPortCode))"/>

    <rsm:HouseWaybill xmlns:rsm="mx:iata:housewaybill:1">
      <rsm:MessageHeaderDocument>
        <ram:ID>
          <xsl:value-of select="$messageIdentifier"/>
        </ram:ID>
        <ram:Name>House waybill</ram:Name>
        <ram:TypeCode>703</ram:TypeCode>
        <ram:IssueDateTime>
          <xsl:value-of select="DateMapper:ConvertXmlDateString(s0:DataContext/s0:Workflow/s0:TriggerDate/text(), 'yyyy-MM-ddTHH:mm:ss')"/>
        </ram:IssueDateTime>
        <ram:PurposeCode>
          <xsl:choose>
            <xsl:when test="$purpose = 'ORG'">Creation</xsl:when>
            <xsl:when test="$purpose = 'AMD'">Update</xsl:when>
            <xsl:when test="$purpose = 'WTH'">Deletion</xsl:when>
          </xsl:choose>
        </ram:PurposeCode>
        <ram:VersionID>3.00</ram:VersionID>
        <ram:SenderParty>
          <ram:PrimaryID schemeID="C">
            <xsl:value-of select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text() = 'BookingPartyDocumentaryAddress']/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text() = 'PSN' and s0:CountryOfIssue/text() = 'MX']/s0:Value/text()"/>
          </ram:PrimaryID>
        </ram:SenderParty>
        <ram:RecipientParty>
          <ram:PrimaryID schemeID="C">VU</ram:PrimaryID>
        </ram:RecipientParty>
      </rsm:MessageHeaderDocument>

      <rsm:BusinessHeaderDocument>
        <ram:ID>
          <xsl:value-of select="$waybillNumber"/>
        </ram:ID>
        <ram:SignatoryConsignorAuthentication>
          <ram:Signatory>
            <xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'ShippersSignature']/s0:Value/text()"/>
          </ram:Signatory>
        </ram:SignatoryConsignorAuthentication>
        <ram:SignatoryCarrierAuthentication>
          <ram:ActualDateTime>
            <xsl:value-of select="DateMapper:ConvertXmlDateString(s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'IssueDate']/s0:Value/text(), 'yyyy-MM-ddTHH:mm:ss')"/>
          </ram:ActualDateTime>
          <ram:Signatory>
            <xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'AgentsSignature']/s0:Value/text()"/>
          </ram:Signatory>
          <ram:IssueAuthenticationLocation>
            <ram:Name>
              <xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'IssuePlace']/s0:Value/text()"/>
            </ram:Name>
          </ram:IssueAuthenticationLocation>
        </ram:SignatoryCarrierAuthentication>
      </rsm:BusinessHeaderDocument>

      <rsm:MasterConsignment>
        <ram:TransportContractDocument>
          <ram:ID>
            <xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'MAWB']/s0:Value/text()"/>
          </ram:ID>
        </ram:TransportContractDocument>
        <xsl:variable name="portOfOrigin" select="s0:PortOfOrigin"/>
        <ram:OriginLocation>
          <ram:ID>
            <xsl:value-of select="$portOfOrigin/text()"/>
          </ram:ID>
          <ram:Name>
            <xsl:value-of select="$portOfOrigin/@Name"/>
          </ram:Name>
        </ram:OriginLocation>
        <xsl:variable name="portOfDestination" select="s0:PortOfDestination"/>
        <ram:FinalDestinationLocation>
          <ram:ID>
            <xsl:value-of select="$portOfDestination/text()"/>
          </ram:ID>
          <ram:Name>
            <xsl:value-of select="$portOfDestination/@Name"/>
          </ram:Name>
        </ram:FinalDestinationLocation>

        <ram:IncludedHouseConsignment>
          <ram:ID>
            <xsl:value-of select="$waybillNumber"/>
          </ram:ID>

          <xsl:variable name="goodsValue" select="s0:GoodsValue/text()"/>
          <xsl:variable name="goodsValueCurrency" select="s0:GoodsValueCurrency/text()"/>
          <xsl:choose>
            <xsl:when test="$goodsValue = 0">
              <ram:NilCarriageValueIndicator>true</ram:NilCarriageValueIndicator>
            </xsl:when>
            <xsl:otherwise>
              <ram:NilCarriageValueIndicator>false</ram:NilCarriageValueIndicator>
              <xsl:call-template name="CreateCurrencyAmountElement">
                <xsl:with-param name="elementName" select="'DeclaredValueForCarriageAmount'" />
                <xsl:with-param name="currencyID" select="$goodsValueCurrency" />
                <xsl:with-param name="value" select="$goodsValue" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>

          <xsl:variable name="customsValueAmount" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'CustomsValueAmount']/s0:Value/text()"/>
          <xsl:variable name="customsValueCurrencyCode" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'CustomsValueCurrencyCode']/s0:Value/text()"/>
          <xsl:choose>
            <xsl:when test="$customsValueAmount = 0">
              <ram:NilCustomsValueIndicator>true</ram:NilCustomsValueIndicator>
            </xsl:when>
            <xsl:otherwise>
              <ram:NilCustomsValueIndicator>false</ram:NilCustomsValueIndicator>
              <xsl:call-template name="CreateCurrencyAmountElement">
                <xsl:with-param name="elementName" select="'DeclaredValueForCustomsAmount'" />
                <xsl:with-param name="currencyID" select="$customsValueCurrencyCode" />
                <xsl:with-param name="value" select="$customsValueAmount" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>

          <xsl:variable name="insuranceValue" select="s0:InsuranceValue/text()"/>
          <xsl:variable name="insuranceValueCurrency" select="s0:InsuranceValueCurrency/text()"/>
          <xsl:choose>
            <xsl:when test="$insuranceValue = 0">
              <ram:NilInsuranceValueIndicator>true</ram:NilInsuranceValueIndicator>
            </xsl:when>
            <xsl:otherwise>
              <ram:NilInsuranceValueIndicator>false</ram:NilInsuranceValueIndicator>
              <xsl:call-template name="CreateCurrencyAmountElement">
                <xsl:with-param name="elementName" select="'InsuranceValueAmount'" />
                <xsl:with-param name="currencyID" select="$insuranceValueCurrency" />
                <xsl:with-param name="value" select="$insuranceValue" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>

          <xsl:variable name="weightPrepaidCollectCode" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'WeightPrepaidCollectCode']/s0:Value/text()"/>
          <xsl:variable name="currencyCode" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'CurrencyCode']/s0:Value/text()"/>
          <xsl:choose>
            <xsl:when test="$weightPrepaidCollectCode = 'PPD'">
              <ram:TotalChargePrepaidIndicator>true</ram:TotalChargePrepaidIndicator>
              <xsl:call-template name="CreateCurrencyAmountElement">
                <xsl:with-param name="elementName" select="'WeightTotalChargeAmount'" />
                <xsl:with-param name="currencyID" select="$currencyCode" />
                <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TotalWeightPPD']/s0:Value/text()" />
              </xsl:call-template>

              <xsl:call-template name="CreateCurrencyAmountElement">
                <xsl:with-param name="elementName" select="'ValuationTotalChargeAmount'" />
                <xsl:with-param name="currencyID" select="$currencyCode" />
                <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ValuationPPD']/s0:Value/text()" />
              </xsl:call-template>

              <xsl:call-template name="CreateCurrencyAmountElement">
                <xsl:with-param name="elementName" select="'TaxTotalChargeAmount'" />
                <xsl:with-param name="currencyID" select="$currencyCode" />
                <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TaxesPPD']/s0:Value/text()" />
              </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <ram:TotalChargePrepaidIndicator>false</ram:TotalChargePrepaidIndicator>
              <xsl:call-template name="CreateCurrencyAmountElement">
                <xsl:with-param name="elementName" select="'WeightTotalChargeAmount'" />
                <xsl:with-param name="currencyID" select="$currencyCode" />
                <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TotalWeightCOL']/s0:Value/text()" />
              </xsl:call-template>

              <xsl:call-template name="CreateCurrencyAmountElement">
                <xsl:with-param name="elementName" select="'ValuationTotalChargeAmount'" />
                <xsl:with-param name="currencyID" select="$currencyCode" />
                <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ValuationCOL']/s0:Value/text()" />
              </xsl:call-template>

              <xsl:call-template name="CreateCurrencyAmountElement">
                <xsl:with-param name="elementName" select="'TaxTotalChargeAmount'" />
                <xsl:with-param name="currencyID" select="$currencyCode" />
                <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TaxesCOL']/s0:Value/text()" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>

          <xsl:variable name="otherPrepaidCollectCode" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'OtherPrepaidCollectCode']/s0:Value/text()"/>
          <xsl:choose>
            <xsl:when test="$otherPrepaidCollectCode = 'PPD'">
              <ram:TotalDisbursementPrepaidIndicator>true</ram:TotalDisbursementPrepaidIndicator>
              <xsl:call-template name="CreateCurrencyAmountElement">
                <xsl:with-param name="elementName" select="'AgentTotalDisbursementAmount'" />
                <xsl:with-param name="currencyID" select="$currencyCode" />
                <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='OtherChargesDueAgentPPD']/s0:Value/text()" />
              </xsl:call-template>

              <xsl:call-template name="CreateCurrencyAmountElement">
                <xsl:with-param name="elementName" select="'CarrierTotalDisbursementAmount'" />
                <xsl:with-param name="currencyID" select="$currencyCode" />
                <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='OtherChargesDueCarrierPPD']/s0:Value/text()" />
              </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <ram:TotalDisbursementPrepaidIndicator>false</ram:TotalDisbursementPrepaidIndicator>
              <xsl:call-template name="CreateCurrencyAmountElement">
                <xsl:with-param name="elementName" select="'AgentTotalDisbursementAmount'" />
                <xsl:with-param name="currencyID" select="$currencyCode" />
                <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='OtherChargesDueAgentCOL']/s0:Value/text()" />
              </xsl:call-template>

              <xsl:call-template name="CreateCurrencyAmountElement">
                <xsl:with-param name="elementName" select="'CarrierTotalDisbursementAmount'" />
                <xsl:with-param name="currencyID" select="$currencyCode" />
                <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='OtherChargesDueCarrierCOL']/s0:Value/text()" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>

          <xsl:call-template name="CreateCurrencyAmountElement">
            <xsl:with-param name="elementName" select="'TotalPrepaidChargeAmount'" />
            <xsl:with-param name="currencyID" select="$currencyCode" />
            <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TotalPPD']/s0:Value/text()" />
          </xsl:call-template>

          <xsl:call-template name="CreateCurrencyAmountElement">
            <xsl:with-param name="elementName" select="'TotalCollectChargeAmount'" />
            <xsl:with-param name="currencyID" select="$currencyCode" />
            <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TotalCOL']/s0:Value/text()" />
          </xsl:call-template>

          <ram:IncludedTareGrossWeightMeasure>
            <xsl:attribute name="unitCode">
              <xsl:call-template name="getWeightUnit">
                <xsl:with-param name="unit" select="s0:TotalWeightUnit/text()"/>
              </xsl:call-template>
            </xsl:attribute>
            <xsl:value-of select="format-number(s0:TotalWeight/text(), '#.00')"/>
          </ram:IncludedTareGrossWeightMeasure>
          <ram:TotalPieceQuantity>
            <xsl:value-of select="s0:TotalNoOfPacks/text()"/>
          </ram:TotalPieceQuantity>
          <ram:SummaryDescription>
            <xsl:value-of select="s0:GoodsDescription/text()"/>
          </ram:SummaryDescription>

          <ram:ConsignorParty>
            <xsl:call-template name="getParty">
              <xsl:with-param name="addressType" select="'ConsignorDocumentaryAddress'"/>
            </xsl:call-template>
          </ram:ConsignorParty>
          <ram:ConsigneeParty>
            <xsl:call-template name="getParty">
              <xsl:with-param name="addressType" select="'ConsigneeDocumentaryAddress'"/>
            </xsl:call-template>
          </ram:ConsigneeParty>
          <ram:FreightForwarderParty>
            <xsl:call-template name="getParty">
              <xsl:with-param name="addressType" select="'SendingForwarderAddress'"/>
            </xsl:call-template>
          </ram:FreightForwarderParty>

          <ram:OriginLocation>
            <ram:ID>
              <xsl:value-of select="$portOfOrigin/text()"/>
            </ram:ID>
            <ram:Name>
              <xsl:value-of select="$portOfOrigin/@Name"/>
            </ram:Name>
          </ram:OriginLocation>
          <ram:FinalDestinationLocation>
            <ram:ID>
              <xsl:value-of select="$portOfDestination/text()"/>
            </ram:ID>
            <ram:Name>
              <xsl:value-of select="$portOfDestination/@Name"/>
            </ram:Name>
          </ram:FinalDestinationLocation>

          <xsl:for-each select="s0:CarrierDocumentsOverride/s0:AWBHeader/s0:SpecialHandlingCollection/s0:SpecialHandling">
            <ram:HandlingSPHInstructions>
              <ram:Description>
                <xsl:value-of select="@Description"/>
              </ram:Description>
              <ram:DescriptionCode>
                <xsl:value-of select="./text()"/>
              </ram:DescriptionCode>
            </ram:HandlingSPHInstructions>
          </xsl:for-each>

          <xsl:for-each select="s0:PackingLineCollection/s0:PackingLine">
            <ram:IncludedHouseConsignmentItem>
              <ram:SequenceNumeric>
                <xsl:value-of select="position()"/>
              </ram:SequenceNumeric>
              <ram:GrossWeightMeasure>
                <xsl:attribute name="unitCode">
                  <xsl:call-template name="getWeightUnit">
                    <xsl:with-param name="unit" select="s0:WeightUnit/text()"/>
                  </xsl:call-template>
                </xsl:attribute>
                <xsl:value-of select="s0:Weight/text()"/>
              </ram:GrossWeightMeasure>
              <ram:PieceQuantity>
                <xsl:value-of select="s0:PackQty/text()"/>
              </ram:PieceQuantity>
              <ram:NatureIdentificationTransportCargo>
                <ram:Identification>
                  <xsl:value-of select="s0:GoodsDescription/text()"/>
                </ram:Identification>
              </ram:NatureIdentificationTransportCargo>
              <ram:OriginCountry>
                <ram:ID>
                  <xsl:value-of select="substring($portOfOrigin/text(), 1, 2)"/>
                </ram:ID>
              </ram:OriginCountry>
              <ram:ApplicableFreightRateServiceCharge>
                <ram:CategoryCode>
                  <xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'RateClass']/s0:Value/text()"/>
                </ram:CategoryCode>
                <ram:CommodityItemID>
                  <xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'CommodityItemNumber']/s0:Value/text()"/>
                </ram:CommodityItemID>
                <ram:ChargeableWeightMeasure>
                  <xsl:attribute name="unitCode">
                    <xsl:call-template name="getWeightUnit">
                      <xsl:with-param name="unit" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'ChargeableWeightUnitCode']/s0:Value/text()"/>
                    </xsl:call-template>
                  </xsl:attribute>
                  <xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'ChargeableWeightValue']/s0:Value/text()"/>
                </ram:ChargeableWeightMeasure>
                <ram:AppliedRate>
                  <xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'RateChargeOrDiscount']/s0:Value/text()"/>
                </ram:AppliedRate>
                <xsl:call-template name="CreateCurrencyAmountElement">
                  <xsl:with-param name="elementName" select="'AppliedAmount'" />
                  <xsl:with-param name="currencyID" select="$currencyCode" />
                  <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='Total']/s0:Value/text()" />
                </xsl:call-template>
              </ram:ApplicableFreightRateServiceCharge>
            </ram:IncludedHouseConsignmentItem>
          </xsl:for-each>
        </ram:IncludedHouseConsignment>
      </rsm:MasterConsignment>
    </rsm:HouseWaybill>
  </xsl:template>

  <xsl:template name="getWeightUnit">
    <xsl:param name="unit"/>

    <xsl:choose>
      <xsl:when test="$unit = 'LB'">LBR</xsl:when>
      <xsl:otherwise>KGM</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="getParty">
    <xsl:param name="addressType"/>

    <xsl:variable name="party" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text() = $addressType]"/>

    <xsl:if test="$party">
      <xsl:if test="$addressType = 'SendingForwarderAddress'">
        <ram:PrimaryID>
          <xsl:value-of select="$party/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text() = 'PSN' and s0:CountryOfIssue/text() = 'MX']/s0:Value/text()"/>
        </ram:PrimaryID>
      </xsl:if>
      <ram:Name>
        <xsl:value-of select="$party/s0:CompanyName/text()"/>
      </ram:Name>
      <ram:PostalStructuredAddress>
        <ram:PostcodeCode>
          <xsl:value-of select="$party/s0:Postcode/text()"/>
        </ram:PostcodeCode>
        <ram:StreetName>
          <xsl:value-of select="concat($party/s0:Address1/text(), ', ', $party/s0:Address2/text())"/>
        </ram:StreetName>
        <ram:CityName>
          <xsl:value-of select="$party/s0:City/text()"/>
        </ram:CityName>
        <ram:CountryID>
          <xsl:value-of select="$party/s0:Country/text()"/>
        </ram:CountryID>
        <ram:CountryName>
          <xsl:value-of select="$party/s0:Country/@Name"/>
        </ram:CountryName>
        <ram:CountrySubDivisionName>
          <xsl:value-of select="$party/s0:State/text()"/>
        </ram:CountrySubDivisionName>
        <ram:PostOfficeBox>
          <xsl:value-of select="$party/s0:POBox/text()"/>
        </ram:PostOfficeBox>
      </ram:PostalStructuredAddress>
      <ram:DefinedTradeContact>
        <ram:PersonName>
          <xsl:value-of select="$party/s0:Contact/text()"/>
        </ram:PersonName>
        <ram:DepartmentName>
          <xsl:value-of select="$party/s0:CompanyName/text()"/>
        </ram:DepartmentName>
        <ram:DirectTelephoneCommunication>
          <ram:CompleteNumber>
            <xsl:value-of select="$party/s0:Phone/text()"/>
          </ram:CompleteNumber>
        </ram:DirectTelephoneCommunication>
        <ram:FaxCommunication>
          <ram:CompleteNumber>
            <xsl:value-of select="$party/s0:Fax/text()"/>
          </ram:CompleteNumber>
        </ram:FaxCommunication>
        <ram:URIEmailCommunication>
          <ram:URIID>
            <xsl:value-of select="$party/s0:Email/text()"/>
          </ram:URIID>
        </ram:URIEmailCommunication>
        <ram:TelexCommunication>
          <ram:CompleteNumber>
            <xsl:value-of select="$party/s0:Telex/text()"/>
          </ram:CompleteNumber>
        </ram:TelexCommunication>
      </ram:DefinedTradeContact>
    </xsl:if>
  </xsl:template>

  <xsl:template name="CreateCurrencyAmountElement">
    <xsl:param name="elementName" />
    <xsl:param name="currencyID" />
    <xsl:param name="value" />

    <xsl:variable name="formattedValue" select="StringMapper:FormatDecimal($value, '0.00', false())" />
    <xsl:element name="ram:{$elementName}">
      <xsl:attribute name="currencyID">
        <xsl:value-of select="$currencyID" />
      </xsl:attribute>
      <xsl:choose>
        <xsl:when test="$formattedValue = ''">0</xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$formattedValue" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:element>
  </xsl:template>

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[
public string ToUpper(string input)
{
  string result = "";
  if (input != null)
  {
    result = input;
  }
  return result.ToUpper();
}
public string RemoveSpecialCharacters(string input)
{
  var result = input.Replace(".", "").Replace("/", "").Replace("-", "");
  return result;
}
]]>
  </msxsl:script>
</xsl:stylesheet>

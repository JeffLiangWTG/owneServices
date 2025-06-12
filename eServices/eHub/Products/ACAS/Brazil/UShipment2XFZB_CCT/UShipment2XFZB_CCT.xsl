<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 CodeMapper XmlHelper ContextAccessor DataModelAccessor DateMapper StringMapper eHubAsyncPollingRegistrationHelper userCSharp" version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2012/11/CCTShipmentReport/1"
                xmlns:ns0="iata:housewaybill:1"
                xmlns:ns1="iata:datamodel:3"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:XmlHelper="http://schemas.microsoft.com/BizTalk/2003/XmlHelper"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
                xmlns:DataModelAccessor="http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"
                xmlns:DateMapper="http://schemas.microsoft.com/BizTalk/2003/DateMapper"
                xmlns:StringMapper="http://schemas.microsoft.com/BizTalk/2003/StringMapper"
                xmlns:eHubAsyncPollingRegistrationHelper="http://schemas.microsoft.com/BizTalk/2003/eHubAsyncPollingRegistrationHelper"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes" />

  <xsl:template match="/">
    <xsl:apply-templates select="s0:UniversalShipment/s0:Shipment" />
  </xsl:template>

  <xsl:template match="s0:UniversalShipment/s0:Shipment">

    <xsl:variable name="SenderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
    <xsl:variable name="RecipientID" select="ContextAccessor:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>

    <xsl:variable name="mawb" select="userCSharp:ToUpper(normalize-space(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='MAWB']/s0:Value/text()))" />
    <xsl:variable name="shipmentNumber" select="s0:DataContext/s0:DataSource[s0:Type/text()='ForwardingShipment']/s0:Key/text()" />
    <xsl:variable name="ACASRecipientID">
      <xsl:choose>
        <xsl:when test="contains($RecipientID, 'TST')">ACAS_BRTest</xsl:when>
        <xsl:otherwise>ACAS_BR</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="subscriptionType" select="'ACASBR'" />

    <xsl:variable name="waybillNumber" select="userCSharp:ToUpper(s0:WayBillNumber/text())"/>
    <xsl:variable name="purpose" select="s0:DataContext/s0:DocumentaryOverride/s0:Purpose/text()" />
    <xsl:variable name="eventUserID" select="s0:DataContext/s0:Workflow/s0:EventUser/text()" />
    <xsl:variable name="documentType" select="s0:DataContext/s0:DataSource/s0:Type/text()" />

    <xsl:variable name="currentDateTimeUTC" select="DateMapper:CurrentDateTimeUTC('yyyyMMddHHmm')"/>
    <xsl:variable name="messageDateTime" select="DateMapper:ConvertToDateTimeString($currentDateTimeUTC, 'yyyyMMddHHmm', 'yyyy-MM-ddTHH:mm:ss')"/>

    <xsl:variable name="InterchangeNum" select="CodeMapper:CallActionProcedureHelper('GetCounterValue','@value','@name','CargoWise.eHub.Products.ACAS.BR.Transforms.CCT','@maxlength','14')" />
    <xsl:variable name="FormattedCounter" select='format-number($InterchangeNum, "0000000000")' />
    <xsl:variable name="formattedInterchangeNumber" select="concat('FZB', $FormattedCounter)" />
    <xsl:variable name="subscriberShipmentReference"  select="concat($shipmentNumber, '_', $formattedInterchangeNumber)" />

    <xsl:variable name="subscribeShipmentID" select="DataModelAccessor:InsertSubscriptionValue($subscriptionType, $ACASRecipientID, $SenderID, $formattedInterchangeNumber, $subscriberShipmentReference, 'InterchangeNum')" />

    <xsl:if test="$shipmentNumber!=$waybillNumber">
      <xsl:variable name="subscribeWaybillNumber" select="DataModelAccessor:InsertSubscriptionValue($subscriptionType, $ACASRecipientID, $SenderID, $subscriberShipmentReference, $waybillNumber, 'FZB-HWB')" />
    </xsl:if>

    <xsl:variable name="SubscribeShipmentId" select="DataModelAccessor:InsertSubscriptionValue($subscriptionType, $ACASRecipientID, $SenderID, $subscriberShipmentReference, $shipmentNumber, 'ShipmentId')" />
    <xsl:variable name="SubscribeDocumentName" select="DataModelAccessor:InsertSubscriptionValue($subscriptionType, $ACASRecipientID, $SenderID, $subscriberShipmentReference, s0:DataContext/s0:DocumentaryOverride/s0:DocumentName, 'DocumentName')" />
    <xsl:variable name="SubscribeWaybillNumber" select="DataModelAccessor:InsertSubscriptionValue($subscriptionType, $ACASRecipientID, $SenderID, $subscriberShipmentReference, 'ForwardingShipment', 'ForwardingType')" />
    <xsl:variable name="SubscribeActionPurpose" select="DataModelAccessor:InsertSubscriptionValue($subscriptionType, $ACASRecipientID, $SenderID, $subscriberShipmentReference, $purpose, 'ActionPurpose')" />

    <xsl:variable name="receivingForwarderAddress" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='ReceivingForwarderAddress']"/>
    <xsl:variable name="PR_PK" select="eHubAsyncPollingRegistrationHelper:InsertEHubAsyncPollingRegistration($SenderID, $RecipientID, $eventUserID, userCSharp:RemoveSpecialCharacters($receivingForwarderAddress/s0:GovRegNum/text()), $shipmentNumber, $documentType, $subscriberShipmentReference, 'http://www.cargowise.com/Schemas/Universal/2012/11/CCTShipmentReport/1', $messageDateTime)" />
    <xsl:variable name="OverrideEmailSubject" select="ContextAccessor:SetContextProperty('OverrideEmailSubject', 'http://cargowise.com/ehub/processing/2010/06', $PR_PK)" />

    <ns0:HouseWaybill>
      <ns0:MessageHeaderDocument>
        <ns1:ID>
          <xsl:value-of select="$subscriberShipmentReference" />
        </ns1:ID>
        <ns1:Name>HouseWaybill</ns1:Name>
        <ns1:TypeCode>703</ns1:TypeCode>
        <ns1:IssueDateTime>
          <xsl:value-of select="concat(DateMapper:CurrentDateTimeUTC('yyyy-MM-ddTHH:mm:ss'), '+00:00')"/>
        </ns1:IssueDateTime>
        <xsl:variable name="purposeCode">
          <xsl:choose>
            <xsl:when test="$purpose = 'ORG'">Creation</xsl:when>
            <xsl:when test="$purpose = 'AMD'">Update</xsl:when>
            <xsl:otherwise>Deletion</xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <ns1:PurposeCode>
          <xsl:value-of select="$purposeCode"/>
        </ns1:PurposeCode>

        <ns1:VersionID>3.00</ns1:VersionID>
        <ns1:SenderParty>
          <ns1:PrimaryID schemeID="O">CARGOWISE</ns1:PrimaryID>
        </ns1:SenderParty>
        <ns1:RecipientParty>
          <ns1:PrimaryID schemeID="O">BRCUSTOMS</ns1:PrimaryID>
        </ns1:RecipientParty>
      </ns0:MessageHeaderDocument>

      <ns0:BusinessHeaderDocument>
        <ns1:ID>
          <xsl:value-of select="$waybillNumber" />
        </ns1:ID>

        <ns1:SignatoryConsignorAuthentication>
          <ns1:Signatory>
            <xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ShippersSignature']/s0:Value/text()"/>
          </ns1:Signatory>
        </ns1:SignatoryConsignorAuthentication>

        <ns1:SignatoryCarrierAuthentication>
          <xsl:variable name="actualDateTime" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='IssueDate']/s0:Value/text()"/>
          <ns1:ActualDateTime>
            <xsl:value-of select="DateMapper:ConvertToDateTimeString($actualDateTime, 'yyyy-MM-ddTHH:mm:ss', 'yyyy-MM-ddTHH:mm:sszzz')"/>
          </ns1:ActualDateTime>
          <ns1:Signatory>
            <xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='AgentsSignature']/s0:Value/text()"/>
          </ns1:Signatory>
          <ns1:IssueAuthenticationLocation>
            <ns1:Name>
              <xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='IssuePlace']/s0:Value/text()"/>
            </ns1:Name>
          </ns1:IssueAuthenticationLocation>
        </ns1:SignatoryCarrierAuthentication>
      </ns0:BusinessHeaderDocument>

      <ns0:MasterConsignment>
        <ns1:TransportContractDocument>
          <ns1:ID>
            <xsl:value-of select="$mawb"/>
          </ns1:ID>
        </ns1:TransportContractDocument>

        <ns1:IncludedHouseConsignment>
          <ns1:ID>
            <xsl:value-of select="s0:WayBillNumber/text()"/>
          </ns1:ID>

          <xsl:variable name="goodsValue" select="s0:GoodsValue/text()"/>
          <xsl:variable name="goodsValueCurrency" select="s0:GoodsValueCurrency/text()"/>

          <xsl:choose>
            <xsl:when test="$goodsValue = 0">
              <ns1:NilCarriageValueIndicator>true</ns1:NilCarriageValueIndicator>
            </xsl:when>
            <xsl:otherwise>
              <ns1:NilCarriageValueIndicator>false</ns1:NilCarriageValueIndicator>
              <xsl:call-template name="CreateCurrencyAmountElement">
                <xsl:with-param name="elementName" select="'DeclaredValueForCarriageAmount'" />
                <xsl:with-param name="currencyID" select="$goodsValueCurrency" />
                <xsl:with-param name="value" select="$goodsValue" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>

          <xsl:variable name="customsValueAmount" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CustomsValueAmount']/s0:Value/text()"/>
          <xsl:variable name="customsValueCurrencyCode" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CustomsValueCurrencyCode']/s0:Value/text()"/>

          <xsl:choose>
            <xsl:when test="$customsValueAmount = 0">
              <ns1:NilCustomsValueIndicator>true</ns1:NilCustomsValueIndicator>
            </xsl:when>
            <xsl:otherwise>
              <ns1:NilCustomsValueIndicator>false</ns1:NilCustomsValueIndicator>
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
              <ns1:NilInsuranceValueIndicator>true</ns1:NilInsuranceValueIndicator>
            </xsl:when>
            <xsl:otherwise>
              <ns1:NilInsuranceValueIndicator>false</ns1:NilInsuranceValueIndicator>
              <xsl:call-template name="CreateCurrencyAmountElement">
                <xsl:with-param name="elementName" select="'InsuranceValueAmount'" />
                <xsl:with-param name="currencyID" select="$insuranceValueCurrency" />
                <xsl:with-param name="value" select="$insuranceValue" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>

          <xsl:variable name="weightPrepaidCollectCode" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='WeightPrepaidCollectCode']/s0:Value/text()"/>
          <xsl:variable name="currencyCode" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CurrencyCode']/s0:Value/text()"/>

          <xsl:choose>
            <xsl:when test="$weightPrepaidCollectCode = 'PPD'">
              <ns1:TotalChargePrepaidIndicator>true</ns1:TotalChargePrepaidIndicator>
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
              <ns1:TotalChargePrepaidIndicator>false</ns1:TotalChargePrepaidIndicator>
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

          <xsl:variable name="otherPrepaidCollectCode" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='OtherPrepaidCollectCode']/s0:Value/text()"/>
          <xsl:variable name="totalPPD" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TotalPPD']/s0:Value/text()" />
          <xsl:variable name="totalCOL" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TotalCOL']/s0:Value/text()" />
          <xsl:variable name="totalChargeAmount">
            <xsl:choose>
                <xsl:when test="$totalPPD and $totalCOL">
                  <xsl:value-of select="$totalPPD + $totalCOL" />
                </xsl:when>
                <xsl:when test="$totalPPD">
                    <xsl:value-of select="$totalPPD" />
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$totalCOL" />
                </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <xsl:choose>
            <xsl:when test="$otherPrepaidCollectCode = 'PPD'">
              <ns1:TotalDisbursementPrepaidIndicator>true</ns1:TotalDisbursementPrepaidIndicator>
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
              <ns1:TotalDisbursementPrepaidIndicator>false</ns1:TotalDisbursementPrepaidIndicator>
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
            <xsl:with-param name="value" select="$totalPPD" />
          </xsl:call-template>

          <xsl:call-template name="CreateCurrencyAmountElement">
            <xsl:with-param name="elementName" select="'TotalCollectChargeAmount'" />
            <xsl:with-param name="currencyID" select="$currencyCode" />
            <xsl:with-param name="value" select="$totalCOL" />
          </xsl:call-template>

          <xsl:variable name="totalWeightUnit">
            <xsl:choose>
              <xsl:when test="s0:TotalWeightUnit/text() = 'LB'">LBR</xsl:when>
              <xsl:otherwise>KGM</xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <ns1:IncludedTareGrossWeightMeasure>
            <xsl:attribute name="unitCode">
              <xsl:value-of select="$totalWeightUnit" />
            </xsl:attribute>
            <xsl:value-of select="StringMapper:FormatDecimal(number(s0:TotalWeight/text()), '0.000', false())" />
          </ns1:IncludedTareGrossWeightMeasure>

          <ns1:TotalPieceQuantity>
            <xsl:value-of select="s0:TotalNoOfPacks/text()" />
          </ns1:TotalPieceQuantity>

          <ns1:SummaryDescription>
            <xsl:value-of select="s0:GoodsDescription/text()" />
          </ns1:SummaryDescription>

          <xsl:variable name="consignorDocumentaryAddress" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='ConsignorDocumentaryAddress']"/>
          <xsl:if test="$consignorDocumentaryAddress != ''">
            <xsl:call-template name="address">
              <xsl:with-param name="elementName" select="'ConsignorParty'"/>
              <xsl:with-param name="org" select="$consignorDocumentaryAddress"/>
            </xsl:call-template>
          </xsl:if>

          <xsl:variable name="consigneeDocumentaryAddress" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='ConsigneeDocumentaryAddress']"/>
          <xsl:if test="$consigneeDocumentaryAddress != ''">
            <xsl:call-template name="address">
              <xsl:with-param name="elementName" select="'ConsigneeParty'"/>
              <xsl:with-param name="org" select="$consigneeDocumentaryAddress"/>
            </xsl:call-template>
          </xsl:if>

          <xsl:variable name="sendingForwarderAddress" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='SendingForwarderAddress']"/>
          <xsl:choose>
            <xsl:when test="$sendingForwarderAddress!=''">
              <xsl:call-template name="address">
                <xsl:with-param name="elementName" select="'FreightForwarderParty'"/>
                <xsl:with-param name="org" select="$sendingForwarderAddress"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="address">
                <xsl:with-param name="elementName" select="'FreightForwarderParty'"/>
                <xsl:with-param name="org" select="$receivingForwarderAddress"/>
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>

          <xsl:variable name="notifyParty" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='NotifyParty']"/>
          <xsl:if test="$notifyParty != ''">
            <xsl:call-template name="address">
              <xsl:with-param name="elementName" select="'AssociatedParty'"/>
              <xsl:with-param name="org" select="$notifyParty"/>
            </xsl:call-template>
          </xsl:if>

          <ns1:OriginLocation>
            <ns1:ID>
              <xsl:value-of select="s0:PortOfOrigin/text()" />
            </ns1:ID>
            <ns1:Name>
              <xsl:value-of select="s0:PortOfOrigin/@Name" />
            </ns1:Name>
          </ns1:OriginLocation>

          <ns1:FinalDestinationLocation>
            <ns1:ID>
              <xsl:value-of select="s0:PortOfDestination/text()" />
            </ns1:ID>
            <ns1:Name>
              <xsl:value-of select="s0:PortOfDestination/@Name" />
            </ns1:Name>
          </ns1:FinalDestinationLocation>

          <xsl:for-each select="s0:CarrierDocumentsOverride/s0:AWBHeader/s0:SpecialHandlingCollection/s0:SpecialHandling">
            <ns1:HandlingSPHInstructions>
              <ns1:Description>
                <xsl:value-of select="substring(normalize-space(@Description), 1, 70)"/>
              </ns1:Description>
              <ns1:DescriptionCode>
                <xsl:value-of select="."/>
              </ns1:DescriptionCode>
            </ns1:HandlingSPHInstructions>
          </xsl:for-each>

          <xsl:variable name="specialServiceRequest" select="normalize-space(s0:CarrierDocumentsOverride/s0:AWBHeader/s0:SpecialServiceRequest/text())"/>
          <xsl:if test="$specialServiceRequest != ''">
            <ns1:HandlingSSRInstructions>
              <ns1:Description>
                <xsl:value-of select="substring($specialServiceRequest, 1, 70)"/>
              </ns1:Description>
            </ns1:HandlingSSRInstructions>
          </xsl:if>

          <xsl:variable name="otherServiceInformation" select="normalize-space(s0:CarrierDocumentsOverride/s0:AWBHeader/s0:OtherServiceInformation/text())"/>
          <xsl:if test="$otherServiceInformation != ''">
            <ns1:HandlingOSIInstructions>
              <ns1:Description>
                <xsl:value-of select="substring($otherServiceInformation, 1, 70)"/>
              </ns1:Description>
            </ns1:HandlingOSIInstructions>
          </xsl:if>

          <xsl:if test="contains('True;Y', s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='WoodenParts']/s0:Value/text())">
            <ns1:IncludedCustomsNote>
              <ns1:ContentCode>DI</ns1:ContentCode>
              <ns1:Content>WOOD PARTS</ns1:Content>
              <ns1:SubjectCode>OCI</ns1:SubjectCode>
              <ns1:CountryID>BR</ns1:CountryID>
            </ns1:IncludedCustomsNote>
          </xsl:if>

          <xsl:call-template name="IncludedCustomsNote">
            <xsl:with-param name="orgAddress" select="$consigneeDocumentaryAddress" />
            <xsl:with-param name="subjectCode" select="'CNE'" />
          </xsl:call-template>

          <xsl:call-template name="IncludedCustomsNote">
            <xsl:with-param name="orgAddress" select="$receivingForwarderAddress" />
            <xsl:with-param name="subjectCode" select="'AGT'" />
          </xsl:call-template>

          <xsl:for-each select="s0:AdditionalReferenceCollection/s0:AdditionalReference[s0:Type/text()='RUC']">
            <ns1:IncludedCustomsNote>
              <ns1:ContentCode>U</ns1:ContentCode>
              <ns1:Content>
                <xsl:value-of select="concat('UCR', s0:ReferenceNumber/text())" />
              </ns1:Content>
              <ns1:SubjectCode>IMP</ns1:SubjectCode>
              <ns1:CountryID>BR</ns1:CountryID>
            </ns1:IncludedCustomsNote>
          </xsl:for-each>

          <xsl:variable name="customsWarehouseValue" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CustomsWarehouse']/s0:Value/text()" />
          <xsl:if test="$customsWarehouseValue!=''">
            <ns1:IncludedCustomsNote>
              <ns1:ContentCode>M</ns1:ContentCode>
              <ns1:Content>
                <xsl:value-of select="concat('CUSTOMSWAREHOUSE', $customsWarehouseValue)" />
              </ns1:Content>
              <ns1:SubjectCode>CCL</ns1:SubjectCode>
              <ns1:CountryID>BR</ns1:CountryID>
            </ns1:IncludedCustomsNote>
          </xsl:if>

          <xsl:for-each select="s0:PackingLineCollection/s0:PackingLine">
            <xsl:if test="s0:WeightUnit/text() != ''">
              <ns1:IncludedHouseConsignmentItem>
                <ns1:SequenceNumeric>
                  <xsl:value-of select="position()"/>
                </ns1:SequenceNumeric>

                <!--<xsl:for-each select="s0:ClassificationCollection/s0:Classification">
                  <ns1:TypeCode listAgencyID="1">
                    <xsl:value-of select="s0:Code/text()"/>
                  </ns1:TypeCode>
                </xsl:for-each>-->

                <xsl:variable name="weightUnit">
                  <xsl:choose>
                    <xsl:when test="s0:WeightUnit/text() = 'LB'">LBR</xsl:when>
                    <xsl:otherwise>KGM</xsl:otherwise>
                  </xsl:choose>
                </xsl:variable>
                <ns1:GrossWeightMeasure>
                  <xsl:attribute name="unitCode">
                    <xsl:value-of select="$weightUnit" />
                  </xsl:attribute>
                  <xsl:value-of select="StringMapper:FormatDecimal(s0:Weight/text(), '0.000', false())" />
                </ns1:GrossWeightMeasure>

                <xsl:call-template name="CreateCurrencyAmountElement">
                   <xsl:with-param name="elementName" select="'TotalChargeAmount'" />
                   <xsl:with-param name="currencyID" select="$currencyCode" />
                   <xsl:with-param name="value" select="$totalChargeAmount" />
                 </xsl:call-template>

                <ns1:PieceQuantity>
                  <xsl:value-of select="s0:PackQty/text()" />
                </ns1:PieceQuantity>
                <ns1:Information>NDA</ns1:Information>

                <ns1:NatureIdentificationTransportCargo>
                  <ns1:Identification>
                    <xsl:value-of select="s0:GoodsDescription/text()" />
                  </ns1:Identification>
                </ns1:NatureIdentificationTransportCargo>

                <xsl:variable name="rateClass" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='RateClass']/s0:Value/text()" />
                <xsl:variable name="commodityItemNumber" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CommodityItemNumber']/s0:Value/text()" />
                <xsl:variable name="chargeableWeightValue" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ChargeableWeightValue']/s0:Value/text()" />
                <xsl:variable name="rateChargeOrDiscount" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='RateChargeOrDiscount']/s0:Value/text()" />
                <xsl:variable name="totalAmount" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='Total']/s0:Value/text()" />

                <ns1:TransportLogisticsPackage>
                  <ns1:ItemQuantity>
                    <xsl:value-of select="s0:PackQty/text()" />
                  </ns1:ItemQuantity>
                  <ns1:GrossWeightMeasure>
                    <xsl:attribute name="unitCode">
                      <xsl:value-of select="$weightUnit" />
                    </xsl:attribute>
                    <xsl:value-of select="StringMapper:FormatDecimal(s0:Weight/text(), '0.000', false())" />
                  </ns1:GrossWeightMeasure>
                </ns1:TransportLogisticsPackage>

                <xsl:if test="$rateClass!='' or $commodityItemNumber!='' or $chargeableWeightValue!='' or $rateChargeOrDiscount!='' or $totalAmount!=''">
                  <ns1:ApplicableFreightRateServiceCharge>
                    <xsl:call-template name="CreateElement">
                      <xsl:with-param name="elementName" select="'CategoryCode'" />
                      <xsl:with-param name="value" select="$rateClass" />
                    </xsl:call-template>

                    <xsl:call-template name="CreateElement">
                      <xsl:with-param name="elementName" select="'CommodityItemID'" />
                      <xsl:with-param name="value" select="$commodityItemNumber" />
                    </xsl:call-template>

                    <xsl:if test="$chargeableWeightValue!=''">
                      <ns1:ChargeableWeightMeasure>
                        <xsl:attribute name="unitCode">
                          <xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='ChargeableWeightUnitCode']/s0:Value/text()" />
                        </xsl:attribute>
                        <xsl:value-of select="StringMapper:FormatDecimal($chargeableWeightValue, '0.000', false())" />
                      </ns1:ChargeableWeightMeasure>
                    </xsl:if>

                    <xsl:call-template name="CreateElement">
                      <xsl:with-param name="elementName" select="'AppliedRate'" />
                      <xsl:with-param name="value" select="$rateChargeOrDiscount" />
                    </xsl:call-template>

                    <xsl:call-template name="CreateCurrencyAmountElement">
                      <xsl:with-param name="elementName" select="'AppliedAmount'" />
                      <xsl:with-param name="currencyID" select="$currencyCode" />
                      <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='Total']/s0:Value/text()" />
                    </xsl:call-template>
                  </ns1:ApplicableFreightRateServiceCharge>
                </xsl:if>
              </ns1:IncludedHouseConsignmentItem>

            </xsl:if>
          </xsl:for-each>

        </ns1:IncludedHouseConsignment>

      </ns0:MasterConsignment>

    </ns0:HouseWaybill>
  </xsl:template>

  <xsl:template name="IncludedCustomsNote">
    <xsl:param name="orgAddress" />
    <xsl:param name="subjectCode" />

    <xsl:if test="$orgAddress!=''">
      <xsl:variable name="govRegNumber">
        <xsl:call-template name="GetGovRegNumber">
          <xsl:with-param name="org" select="$orgAddress"/>
        </xsl:call-template>
      </xsl:variable>

      <xsl:if test="$govRegNumber!=''">
        <ns1:IncludedCustomsNote>
          <ns1:ContentCode>T</ns1:ContentCode>
          <ns1:Content>
            <xsl:value-of select="$govRegNumber"/>
          </ns1:Content>
          <ns1:SubjectCode>
            <xsl:value-of select="$subjectCode"/>
          </ns1:SubjectCode>
          <ns1:CountryID>BR</ns1:CountryID>
        </ns1:IncludedCustomsNote>
      </xsl:if>
    </xsl:if>
  </xsl:template>

  <xsl:template name="GetGovRegNumber">
    <xsl:param name="org"/>

    <xsl:variable name="govRegType" select="$org/s0:GovRegNumType/text()" />
    <xsl:variable name="govRegTypePrefix" >
      <xsl:choose>
        <xsl:when test="$govRegType='CJN'">CNPJ</xsl:when>
        <xsl:when test="$govRegType='CPF'">CPF</xsl:when>
        <xsl:when test="$govRegType='PAS'">PAS</xsl:when>
        <xsl:otherwise></xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="govRegNum" select="userCSharp:RemoveSpecialCharacters($org/s0:GovRegNum/text())"/>
    <xsl:choose>
      <xsl:when test="$govRegNum!='' and $govRegTypePrefix!=''">
        <xsl:value-of select="concat($govRegTypePrefix, $govRegNum)"/>
      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="address">
    <xsl:param name="elementName"/>
    <xsl:param name="org"/>

    <xsl:variable name="companyName" select="$org/s0:CompanyName/text()"/>
    <xsl:variable name="postcode" select="$org/s0:Postcode/text()"/>
    <xsl:variable name="address1" select="$org/s0:Address1/text()"/>
    <xsl:variable name="address2" select="$org/s0:Address2/text()"/>
    <xsl:variable name="city" select="$org/s0:City/text()"/>
    <xsl:variable name="country" select="$org/s0:Country/text()"/>
    <xsl:variable name="countryName" select="$org/s0:Country/@Name"/>
    <xsl:variable name="state" select="$org/s0:State/text()"/>
    <xsl:variable name="contact" select="$org/s0:Contact/text()"/>
    <xsl:variable name="phone" select="$org/s0:Phone/text()"/>
    <xsl:variable name="fax" select="$org/s0:Fax/text()"/>
    <xsl:variable name="email" select="$org/s0:Email/text()"/>

    <xsl:element name="ns1:{$elementName}">
      <ns1:Name>
        <xsl:value-of select="$companyName" />
      </ns1:Name>
      <ns1:PostalStructuredAddress>
        <ns1:PostcodeCode>
          <xsl:value-of select="$postcode" />
        </ns1:PostcodeCode>
        <ns1:StreetName>
          <xsl:variable name="address" select="concat($address1, ', ', $address2)"/>
          <xsl:choose>
            <xsl:when test="$elementName = 'ConsignorParty' or $elementName = 'ConsigneeParty' or $elementName = 'FreightForwarderParty'">
              <xsl:value-of select="substring($address, 1, 68)" />
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$address" />
            </xsl:otherwise>
          </xsl:choose>
        </ns1:StreetName>
        <ns1:CityName>
          <xsl:value-of select="$city" />
        </ns1:CityName>
        <ns1:CountryID>
          <xsl:value-of select="$country" />
        </ns1:CountryID>
        <ns1:CountryName>
          <xsl:value-of select="$countryName" />
        </ns1:CountryName>
        <ns1:CountrySubDivisionName>
          <xsl:value-of select="$state" />
        </ns1:CountrySubDivisionName>
      </ns1:PostalStructuredAddress>
      <ns1:DefinedTradeContact>
        <ns1:PersonName>
          <xsl:value-of select="$contact" />
        </ns1:PersonName>
        <ns1:DirectTelephoneCommunication>
          <ns1:CompleteNumber>
            <xsl:value-of select="$phone" />
          </ns1:CompleteNumber>
        </ns1:DirectTelephoneCommunication>
        <ns1:FaxCommunication>
          <ns1:CompleteNumber>
            <xsl:value-of select="$fax" />
          </ns1:CompleteNumber>
        </ns1:FaxCommunication>
        <ns1:URIEmailCommunication>
          <ns1:URIID>
            <xsl:value-of select="$email" />
          </ns1:URIID>
        </ns1:URIEmailCommunication>
      </ns1:DefinedTradeContact>
    </xsl:element>
  </xsl:template>

  <xsl:template name="CreateElement">
    <xsl:param name="elementName" />
    <xsl:param name="value" />

    <xsl:if test="$value!=''">
      <xsl:element name="ns1:{$elementName}">
        <xsl:value-of select="$value" />
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="CreateCurrencyAmountElement">
    <xsl:param name="elementName" />
    <xsl:param name="currencyID" />
    <xsl:param name="value" />

    <xsl:variable name="formattedValue" select="StringMapper:FormatDecimal($value, '0.00', false())" />
    <xsl:element name="ns1:{$elementName}">
      <xsl:attribute name="currencyID">
        <xsl:value-of select="$currencyID" />
      </xsl:attribute>
      <xsl:choose>
        <xsl:when test="$formattedValue=''">0</xsl:when>
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

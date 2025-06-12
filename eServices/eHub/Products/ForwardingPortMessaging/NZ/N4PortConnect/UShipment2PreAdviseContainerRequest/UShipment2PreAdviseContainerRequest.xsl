<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 ContextAccessor DataModelAccessor StringHelper CodeMapper UnitConverter DateMapper"
                version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2012/11/ExportPreAdviseNotification/1"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
                xmlns:DataModelAccessor="http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"
                xmlns:StringHelper="http://schemas.microsoft.com/BizTalk/2003/StringHelper"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:UnitConverter="http://schemas.microsoft.com/BizTalk/2003/UnitConverter"
                xmlns:DateMapper="http://schemas.microsoft.com/BizTalk/2003/DateMapper">

  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes"/>

  <xsl:template match="/">
    <xsl:apply-templates select="s0:UniversalShipment/s0:Shipment"/>
  </xsl:template>

  <xsl:template match="s0:UniversalShipment/s0:Shipment">
    <xsl:variable name="shipment" select="."/>
    <xsl:variable name="dataSourceKey" select="s0:DataContext/s0:DataSource/s0:Key/text()"/>
    <xsl:variable name="purpose" select="s0:DataContext/s0:DocumentaryOverride/s0:Purpose/text()"/>
    <xsl:variable name="shipperReference" select="s0:AdditionalReferenceCollection/s0:AdditionalReference[s0:Type/text() = 'FFW']/s0:ReferenceNumber/text()"/>
    <xsl:variable name="operationalPortCode" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'OperationalPort_Code']/s0:Value/text()"/>
    <xsl:variable name="otherTransportMode" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'Other_TransportMode']/s0:Value/text()"/>
    <xsl:variable name="portOfOrigin" select="s0:PortOfOrigin/text()"/>
    <xsl:variable name="branch" select="s0:DataContext/s0:Workflow/s0:EventBranch/text()"/>
    <xsl:variable name="forwardingType" select="concat(s0:DataContext/s0:DataSource/s0:Type/text(), '_', s0:DataContext/s0:DataSource/s0:Key/text())"/>
    <xsl:variable name="documentName" select="s0:DataContext/s0:DocumentaryOverride/s0:DocumentName/text()"/>

    <xsl:variable name="senderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
    <xsl:variable name="recipientID" select="ContextAccessor:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
    <xsl:variable name="serviceProvider" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE', 'FORWARDING_PORT_MESSAGE', 'FPM System Configuration', 'Port Settings', 'Name', $recipientID)"/>
    <xsl:variable name="serviceProviderMSGID" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE', 'FORWARDING_PORT_MESSAGE', 'FPM System Configuration', 'Port Settings', 'MSGID', $recipientID)"/>
    <xsl:variable name="serviceProviderID" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE', 'FORWARDING_PORT_MESSAGE', 'FPM System Configuration', 'Port Settings', 'ID', $recipientID)"/>
    <xsl:variable name="serviceProviderSubscriptionPrefix" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE', 'FORWARDING_PORT_MESSAGE', 'FPM System Configuration', 'Port Settings', 'SubscriptionPrefix', $recipientID)"/>

    <xsl:variable name="portConnectID" select="DataModelAccessor:GetClientRegistrationCode($senderID, $branch, $serviceProvider)"/>
    <xsl:variable name="portConnectAttribute1" select="DataModelAccessor:GetClientRegistrationAttri1AsString($senderID, $branch, $serviceProvider)"/>

    <!--message should only contain 1 container, previous mapping is splitting the UXML so it only contains 1 container-->
    <xsl:variable name="preAdvice" select="concat(s0:ContainerCollection/s0:Container[1]/s0:ContainerNumber/text(), '_', $operationalPortCode, '_', $portConnectID)"/>

    <xsl:variable name="previousReference" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $serviceProvider, '@recipientId', '', '@ST_ID', $serviceProviderMSGID, '@value', $preAdvice, '@referenceType', 'PreAdvice')" />
    <xsl:variable name="subscriberReference">
      <xsl:choose>
        <xsl:when test="$previousReference != ''">
          <xsl:value-of select="$previousReference" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:variable name="msgCounter" select="CodeMapper:CallActionProcedureHelper('GetCounterValue', '@value', '@name', concat('CargoWise.eHub.Products.ForwardingPortMessaging.NZ.', $serviceProvider, '.PreAdviseContainerRequest'), '@maxlength', '14')" />
          <xsl:variable name="newMessageReference" select="concat($serviceProviderSubscriptionPrefix, format-number($msgCounter, '0000000000'))" />
          <xsl:variable name="subscribePreAdvice1" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $newMessageReference, $preAdvice, 'PreAdvice')" />
          <xsl:variable name="subscribePreAdvice2" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $preAdvice, $newMessageReference, 'PreAdvice')" />
          <xsl:value-of select="$newMessageReference"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="subscribeOperationalPort" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $subscriberReference, $operationalPortCode, 'OperationPort')" />
    <xsl:variable name="subscribePurpose" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $subscriberReference, $purpose, 'Purpose')" />
    <xsl:variable name="subscribeDocumentName" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $subscriberReference, $documentName, 'DocumentName')" />
    <xsl:variable name="subscribeForwardingType" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $subscriberReference, $forwardingType, 'ForwardingType')" />

    <xsl:variable name="currentDateTime" select="DateMapper:CurrentDateTimeUTC('yyyyMMddHHmmssfff')"/>
    <xsl:variable name="overrideFilename">
      <xsl:choose>
        <xsl:when test="$serviceProvider='N4'">
          <xsl:value-of select="ContextAccessor:SetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06', concat($subscriberReference, '.xml'))"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="ContextAccessor:SetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06', concat($senderID, '_', $dataSourceKey, '_', $currentDateTime, '.xml'))"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <PreadviseContainerRequest xmlns="http://PortConnect.ExportPreAdvice.BizTalk.GenericXML/201404">
      <header>
        <MessageType>ExportPreAdvice</MessageType>
        <TradingPartnerCode>
          <xsl:value-of select="$portConnectID"/>
        </TradingPartnerCode>
        <PartnerPortCode>
          <xsl:value-of select="$operationalPortCode"/>
        </PartnerPortCode>
        <BusinessNotificationEmailList/>
        <xsl:if test="$purpose != 'WTH'">
          <UserName>
            <xsl:value-of select="$portConnectAttribute1"/>
          </UserName>
          <UserPassword/>
        </xsl:if>
      </header>
      <PreAdvice>
        <xsl:choose>
          <xsl:when test="$purpose = 'WTH'">
            <Shipper>
              <shipperReference>
                <xsl:value-of select="$shipperReference"/>
              </shipperReference>
            </Shipper>
            <xsl:for-each select="s0:ContainerCollection/s0:Container">
              <Equipment equipmentType="CONTAINER">
                <equipmentID>
                  <xsl:value-of select="s0:ContainerNumber/text()"/>
                </equipmentID>
                <MessageAction>Cancel</MessageAction>
                <IMEX/>
              </Equipment>
            </xsl:for-each>
          </xsl:when>
          <xsl:otherwise>
            <Shipper>
              <name>
                <xsl:variable name="sendingForwarder" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text() = 'SendingForwarderAddress']/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text() = 'PSN' and s0:CountryOfIssue/text() = 'NZ']/s0:Value/text()"/>
                <xsl:choose>
                  <xsl:when test="$sendingForwarder != ''">
                    <xsl:value-of select="$sendingForwarder"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text() = 'SendingForwarderAddress']/CompanyName/text()"/>
                  </xsl:otherwise>
                </xsl:choose>
              </name>
              <shipperReference>
                <xsl:value-of select="$shipperReference"/>
              </shipperReference>
            </Shipper>

            <xsl:for-each select="s0:ContainerCollection/s0:Container">
              <Equipment equipmentType="CONTAINER">
                <equipmentID>
                  <xsl:value-of select="s0:ContainerNumber/text()"/>
                </equipmentID>
                <isoTypeCode>
                  <xsl:value-of select="s0:ContainerType/s0:ISOCode/text()"/>
                </isoTypeCode>
                <isFull>
                  <xsl:choose>
                    <xsl:when test="StringHelper:ToUpper(s0:IsEmptyContainer/text()) = 'FALSE'">true</xsl:when>
                    <xsl:otherwise>false</xsl:otherwise>
                  </xsl:choose>
                </isFull>
                <commodityCode>
                  <xsl:value-of select="s0:Commodity/text()"/>
                </commodityCode>
                <LoadPortFacility>
                  <xsl:value-of select="../../s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text() = 'DepartureCTOAddress']/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text() = 'PSN' and s0:CountryOfIssue/text() = 'NZ']/s0:Value/text()"/>
                </LoadPortFacility>
                <MessageAction>Create</MessageAction>

                <refrigeration>
                  <xsl:choose>
                    <xsl:when test="StringHelper:ToUpper(s0:IsControlledAtmosphere/text()) = 'TRUE' and number(s0:SetPointTemp/text()) &gt; 0">
                      <xsl:attribute name="refrigerationType">CHILLED</xsl:attribute>
                    </xsl:when>
                    <xsl:when test="StringHelper:ToUpper(s0:IsControlledAtmosphere/text()) = 'TRUE' and number(s0:SetPointTemp/text()) &lt; 0">
                      <xsl:attribute name="refrigerationType">FROZEN</xsl:attribute>
                    </xsl:when>
                    <xsl:otherwise></xsl:otherwise>
                  </xsl:choose>
                  <isFantainer>
                    <xsl:value-of select="s0:IsControlledAtmosphere/text()"/>
                  </isFantainer>
                  <xsl:if test="StringHelper:ToUpper(s0:IsControlledAtmosphere/text()) = 'TRUE'">
                    <requiredTemperature>
                      <xsl:value-of select="s0:SetPointTemp/text()"/>
                    </requiredTemperature>
                    <humidityPercent>
                      <xsl:value-of select="s0:HumidityPercent/text()"/>
                    </humidityPercent>
                  </xsl:if>
                </refrigeration>

                <xsl:if test="s0:AirVentFlowRateUnit/text() != ''">
                  <vent>
                    <VentSettingType>
                      <xsl:choose>
                        <xsl:when test="contains('MQH;2L', s0:AirVentFlowRateUnit/text())">FlowM3PerHour</xsl:when>
                        <xsl:when test="contains('P1', s0:AirVentFlowRateUnit/text())">PercentageOpen</xsl:when>
                      </xsl:choose>
                    </VentSettingType>
                    <VentSetting>
                      <xsl:choose>
                        <xsl:when test="contains('MQH;2L', s0:AirVentFlowRateUnit/text())">
                          <xsl:value-of select="s0:AirVentFlow/text()"/>
                        </xsl:when>
                        <xsl:when test="contains('P1', s0:AirVentFlowRateUnit/text())">
                          <xsl:value-of select="number(s0:AirVentFlow/text()) * 1.6990108"/>
                        </xsl:when>
                      </xsl:choose>
                    </VentSetting>
                  </vent>
                </xsl:if>

                <IMEX>
                  <LineOperatorCode>
                    <xsl:value-of select="$shipment/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text() = 'ShippingLineAddress']/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text() = 'CCC' and s0:CountryOfIssue/text() = 'US']/s0:Value/text()"/>
                  </LineOperatorCode>
                  <shipName>
                    <xsl:value-of select="$shipment/s0:VesselName/text()"/>
                  </shipName>
                  <voyageNumber>
                    <xsl:value-of select="$shipment/s0:VoyageFlightNo/text()"/>
                  </voyageNumber>
                  <partnerPortshippingReference/>
                  <loadPortCode>
                    <xsl:value-of select="$shipment/s0:PortOfLoading/text()"/>
                  </loadPortCode>
                  <portOfDischarge>
                    <xsl:value-of select="$shipment/s0:PortOfDischarge/text()"/>
                  </portOfDischarge>
                  <bookingReference>
                    <xsl:value-of select="$shipment/s0:BookingConfirmationReference/text()"/>
                  </bookingReference>
                </IMEX>

                <CargoWeight>
                  <xsl:value-of select="s0:GoodsWeight/text()"/>
                </CargoWeight>
                <xsl:if test="$operationalPortCode != '' and contains('NZAKL;NZTRG;NZMKL;NZTIU', $operationalPortCode)">
                  <TotalWeight>
                    <xsl:value-of select="s0:GrossWeight/text()"/>
                  </TotalWeight>
                </xsl:if>
                <xsl:if test="$operationalPortCode != '' and contains('NZLYT', $operationalPortCode)">
                  <VerifiedGrossMass>
                    <xsl:value-of select="s0:GrossWeight/text()"/>
                  </VerifiedGrossMass>
                  <WeighingMethod>
                    <xsl:value-of select="s0:GrossWeightVerificationType/text()"/>
                  </WeighingMethod>
                  <WeighingDate>
                    <xsl:value-of select="s0:GrossWeightVerificationDateTime/text()"/>
                  </WeighingDate>
                  <VGMContact>
                    <xsl:variable name="grossWeightVerifiedBy" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text() = 'GrossWeightVerifiedBy']"/>
                    <Name>
                      <xsl:value-of select="$grossWeightVerifiedBy/s0:CompanyName/text()"/>
                    </Name>
                    <Address1>
                      <xsl:value-of select="$grossWeightVerifiedBy/s0:Address1/text()"/>
                    </Address1>
                    <Address2>
                      <xsl:value-of select="$grossWeightVerifiedBy/s0:Address2/text()"/>
                    </Address2>
                    <CityName>
                      <xsl:value-of select="$grossWeightVerifiedBy/s0:City/text()"/>
                    </CityName>
                    <Country>
                      <xsl:value-of select="$grossWeightVerifiedBy/s0:Country/@Name/text()"/>
                    </Country>
                    <PostalCode>
                      <xsl:value-of select="$grossWeightVerifiedBy/s0:Postcode/text()"/>
                    </PostalCode>
                    <Email>
                      <xsl:value-of select="$grossWeightVerifiedBy/s0:Email/text()"/>
                    </Email>
                    <Phone>
                      <xsl:value-of select="$grossWeightVerifiedBy/s0:Phone/text()"/>
                    </Phone>
                  </VGMContact>
                </xsl:if>

                <xsl:for-each select="s0:UNDGCollection/s0:UNDG">
                  <hazardous>
                    <hazardousClass>
                      <xsl:value-of select="s0:IMOClass/text()"/>
                    </hazardousClass>
                    <UNNumber>
                      <xsl:value-of select="s0:UNDGCode/text()"/>
                    </UNNumber>
                    <quantity>
                      <xsl:value-of select="concat(s0:PackQty/text(), ' ', s0:PackType/text())"/>
                    </quantity>
                    <packagingGroup>
                      <xsl:choose>
                        <xsl:when test="s0:PackingGroup/text() = 'I'">1</xsl:when>
                        <xsl:when test="s0:PackingGroup/text() = 'II'">2</xsl:when>
                        <xsl:when test="s0:PackingGroup/text() = 'III'">3</xsl:when>
                        <xsl:otherwise>0</xsl:otherwise>
                      </xsl:choose>
                    </packagingGroup>
                    <limitedQuantities>
                      <xsl:value-of select="s0:PackedInLimitedQuantity/text()"/>
                    </limitedQuantities>
                    <marinePollutant>
                      <xsl:choose>
                        <xsl:when test="s0:MarinePollutant/text() != ''">true</xsl:when>
                        <xsl:otherwise>false</xsl:otherwise>
                      </xsl:choose>
                    </marinePollutant>
                    <hazardousWeight>
                      <xsl:value-of select="s0:Weight/text()"/>
                    </hazardousWeight>
                    <emsCode>
                      <xsl:value-of select="concat(s0:EmergencyScheduleFire/text(), s0:EmergencyScheduleSpillage/text())"/>
                    </emsCode>
                    <flashPoint>
                      <xsl:value-of select="s0:FlashPoint/text()"/>
                    </flashPoint>
                    <contactPhone>
                      <xsl:value-of select="s0:Contact/s0:Phone/text()"/>
                    </contactPhone>
                    <contactName>
                      <xsl:value-of select="s0:Contact/s0:FullName/text()"/>
                    </contactName>
                  </hazardous>
                </xsl:for-each>

                <xsl:call-template name="overGauge">
                  <xsl:with-param name="area" select="'Back'"/>
                  <xsl:with-param name="value" select="s0:OverhangBack/text()"/>
                  <xsl:with-param name="fromUnit" select="s0:LengthUnit/text()"/>
                </xsl:call-template>
                <xsl:call-template name="overGauge">
                  <xsl:with-param name="area" select="'Front'"/>
                  <xsl:with-param name="value" select="s0:OverhangFront/text()"/>
                  <xsl:with-param name="fromUnit" select="s0:LengthUnit/text()"/>
                </xsl:call-template>
                <xsl:call-template name="overGauge">
                  <xsl:with-param name="area" select="'Top'"/>
                  <xsl:with-param name="value" select="s0:OverhangHeight/text()"/>
                  <xsl:with-param name="fromUnit" select="s0:LengthUnit/text()"/>
                </xsl:call-template>
                <xsl:call-template name="overGauge">
                  <xsl:with-param name="area" select="'Right'"/>
                  <xsl:with-param name="value" select="s0:OverhangRight/text()"/>
                  <xsl:with-param name="fromUnit" select="s0:LengthUnit/text()"/>
                </xsl:call-template>
                <xsl:call-template name="overGauge">
                  <xsl:with-param name="area" select="'Left'"/>
                  <xsl:with-param name="value" select="s0:OverhangLeft/text()"/>
                  <xsl:with-param name="fromUnit" select="s0:LengthUnit/text()"/>
                </xsl:call-template>

                <xsl:call-template name="containerSeals">
                  <xsl:with-param name="partyType" select="s0:SealPartyType/text()"/>
                  <xsl:with-param name="seal" select="s0:Seal/text()"/>
                </xsl:call-template>
                <xsl:call-template name="containerSeals">
                  <xsl:with-param name="partyType" select="s0:SecondSealPartyType/text()"/>
                  <xsl:with-param name="seal" select="s0:SecondSeal/text()"/>
                </xsl:call-template>
                <xsl:call-template name="containerSeals">
                  <xsl:with-param name="partyType" select="s0:ThirdSealPartyType/text()"/>
                  <xsl:with-param name="seal" select="s0:ThirdSeal/text()"/>
                </xsl:call-template>
                <xsl:if test="normalize-space(s0:Seal/text()) = '' and normalize-space(s0:SecondSeal/text()) = '' and normalize-space(s0:ThirdSeal/text()) = ''">
                  <xsl:call-template name="containerSeals">
                    <xsl:with-param name="seal" select="'NA'"/>
                  </xsl:call-template>
                </xsl:if>

                <arrivalCarrierType>
                  <xsl:choose>
                    <xsl:when test="$otherTransportMode = 'RAI'">Rail</xsl:when>
                    <xsl:when test="$otherTransportMode = 'ROA'">Truck</xsl:when>
                    <xsl:otherwise></xsl:otherwise>
                  </xsl:choose>
                </arrivalCarrierType>
                <pointOfOriginCode>
                  <xsl:value-of select="$portOfOrigin"/>
                </pointOfOriginCode>
              </Equipment>
            </xsl:for-each>
          </xsl:otherwise>
        </xsl:choose>
      </PreAdvice>
    </PreadviseContainerRequest>
  </xsl:template>

  <xsl:template name="overGauge">
    <xsl:param name="area"/>
    <xsl:param name="value"/>
    <xsl:param name="fromUnit"/>

    <xsl:if test="number($value) != 0">
      <overGauge xmlns="http://PortConnect.ExportPreAdvice.BizTalk.GenericXML/201404">
        <OverDimensionArea>
          <xsl:value-of select="$area"/>
        </OverDimensionArea>
        <OverDimensionUOM>CENTIMETER</OverDimensionUOM>
        <OverDimensionMeasure>
          <xsl:value-of select="UnitConverter:Convert($value, $fromUnit, 'CM', 3)"/>
        </OverDimensionMeasure>
      </overGauge>
    </xsl:if>
  </xsl:template>

  <xsl:template name="containerSeals">
    <xsl:param name="partyType" select="''"/>
    <xsl:param name="seal"/>

    <xsl:if test="$seal != ''">
      <containerSeals xmlns="http://PortConnect.ExportPreAdvice.BizTalk.GenericXML/201404">
        <SealType>
          <xsl:choose>
            <xsl:when test="$partyType = 'CAR'">LineOperator</xsl:when>
            <xsl:when test="$partyType = 'CRD'">Shipper</xsl:when>
            <xsl:otherwise>Other</xsl:otherwise>
          </xsl:choose>
        </SealType>
        <SealCode>
          <xsl:value-of select="$seal"/>
        </SealCode>
      </containerSeals>
    </xsl:if>
  </xsl:template>

</xsl:stylesheet>

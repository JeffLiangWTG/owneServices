<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl s0 CodeMapper ContextAccessor DataModelAccessor DateMapper StringMapper UnitConverter StringHelper userCSharp" version="1.0"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2012/11/ALPO_Auftrag/1"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
                xmlns:DataModelAccessor="http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"
                xmlns:DateMapper="http://schemas.microsoft.com/BizTalk/2003/DateMapper"
                xmlns:StringMapper="http://schemas.microsoft.com/BizTalk/2003/StringMapper"
                xmlns:UnitConverter="http://schemas.microsoft.com/BizTalk/2003/UnitConverter"
                xmlns:StringHelper="http://schemas.microsoft.com/BizTalk/2003/StringHelper"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" indent="yes" />

  <xsl:key name="groupByContainerAndCommodity" match="//s0:PackingLine" use="concat(./s0:ContainerNumber/text(), ./s0:Commodity/text())"/>
  <xsl:key name="groupByCommodity" match="//s0:PackingLine" use="./s0:Commodity/text()"/>
  <xsl:key name="groupByContainerAndCargoItem" match="//s0:PackingLine" use="concat(./s0:ContainerNumber/text(), ./s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CargoItem']/s0:Value/text())"/>
  <xsl:key name="groupByCargoItem" match="//s0:PackingLine" use="./s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CargoItem']/s0:Value/text()"/>

  <xsl:variable name="senderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')" />
  <xsl:variable name="recipientID" select="ContextAccessor:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')" />

  <xsl:variable name="serviceProvider" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Port Settings' , 'Name', $recipientID)" />
  <xsl:variable name="serviceProviderMSGID" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Port Settings' , 'MSGID', $recipientID)" />
  <xsl:variable name="serviceProviderID" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Port Settings' , 'ID', $recipientID)" />

  <xsl:variable name="msgPrefix"  select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE', 'FORWARDING_PORT_MESSAGE', 'FPM System Configuration', 'Port Settings', 'SubscriptionPrefix', $recipientID)"/>
  <xsl:variable name="interchangeID" select="CodeMapper:CallActionProcedureHelper('GetCounterValue','@value','@name','CargoWise.eHub.Products.ForwardingPortMessaging.DE.ALPO.Interchange','@maxlength','14')"/>
  <xsl:variable name="formattedMessageID" select ="concat($msgPrefix, format-number($interchangeID, '0000000000'))"/>

  <xsl:variable name="InboxPK" select="ContextAccessor:GetContextProperty('InternalTrackingID', 'http://cargowise.com/ehub/tracking/2010/06')" />
  <xsl:variable name="subscribeInboxPK" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $InboxPK, $interchangeID)" />

  <xsl:variable name="shipment" select="s0:UniversalShipment/s0:Shipment" />

  <xsl:template match="/">
    <xsl:apply-templates select="s0:UniversalShipment/s0:Shipment" />
  </xsl:template>

  <xsl:template match="s0:UniversalShipment/s0:Shipment">
    <xsl:variable name="isFormVersion200" select="StringHelper:IsNewFormMessage(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='FormVersion']/s0:Value/text(), '2.0.0')"/>
    <xsl:variable name="consolID" select="s0:DataContext/s0:DataSource/s0:Key/text()"/>
    <xsl:variable name="purpose" select="userCSharp:ToUpper(normalize-space(s0:DataContext/s0:DocumentaryOverride/s0:Purpose/text()))"/>
    <xsl:variable name="documentName" select="s0:DataContext/s0:DocumentaryOverride/s0:DocumentName/text()"/>
    <xsl:variable name="operationalPortCode" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='OperationalPort_Code']/s0:Value/text()"/>
    <xsl:variable name="triggerDate" select="DateMapper:ConvertXmlDateString(s0:DataContext/s0:Workflow/s0:TriggerDate/text(), 'yyyyMMddHHmmss')"/>
    <xsl:variable name="direction" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='Direction']/s0:Value/text()"/>
    <xsl:variable name="dirShort" select="userCSharp:ToUpper(substring($direction,1,3))"/>
    <xsl:variable name="bookingConfirmationReference" select="s0:BookingConfirmationReference/text()"/>

    <xsl:variable name="fileSenderID" select="DataModelAccessor:GetClientRegistrationCode($senderID, s0:DataContext/s0:Workflow/s0:EventBranch/text(), $serviceProvider)"/>

    <xsl:variable name="subscribeJobNumber1" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $formattedMessageID, $consolID, 'JobNumber')"/>
    <xsl:variable name="subscribeJobNumber2" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $consolID, $formattedMessageID, 'JobNumber')"/>
    <xsl:variable name="subscribeForwardingType" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $formattedMessageID, s0:DataContext/s0:DataSource/s0:Type/text(), 'ForwardingType')"/>
    <xsl:variable name="subscribeActionPurpose" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $formattedMessageID, $purpose, 'Purpose')"/>
    <xsl:variable name="subscribeDocumentName" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $formattedMessageID, $documentName, 'DocumentName')"/>
    <xsl:variable name="subscribeOperationPort" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $formattedMessageID, $operationalPortCode, 'OperationPort')" />
    <xsl:variable name="subscribeClientID" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderID, $serviceProvider, $senderID, $fileSenderID)"/>

    <xsl:variable name="subShipmentMode">
      <xsl:choose>
        <xsl:when test="count(s0:SubShipmentCollection/s0:SubShipment/s0:PackingLineCollection/s0:PackingLine) > 0">TRUE</xsl:when>
        <xsl:otherwise>FALSE</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="FileName" select="ContextAccessor:SetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06', concat('ORDER_', $dirShort, '_ALPO_', $fileSenderID, '_', $consolID, '_', $senderID, '_', $triggerDate))" />

    <xsl:variable name="communicationType">
      <xsl:choose>
        <xsl:when test="$operationalPortCode='DEHAM'">ZAPP</xsl:when>
        <xsl:otherwise>BHT</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="containerMode" select="s0:ContainerMode/text()"/>

    <xsl:variable name="warehouseCode">
      <xsl:call-template name="GetValueWithFallback">
        <xsl:with-param name="value" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='Warehouse']/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='CWC']/s0:Value/text()" />
        <xsl:with-param name="fallbackValue" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='Warehouse']/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='EID']/s0:Value/text()"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="packingLineUNDGCount">
      <xsl:choose>
        <xsl:when test="count(//s0:PackingLine/s0:UNDGCollection/s0:UNDG) > 0">True</xsl:when>
        <xsl:otherwise>False</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <AdvantageEnterpriseVersion0.1 xmlns:noNamespaceSchemaLocation="ALPO-AUFTRAG_V1_03.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
      <SENDER>
        <ID_SENDERSYSTEM>
          <xsl:value-of select="$fileSenderID"/>
        </ID_SENDERSYSTEM>
        <SENDER_REFERENZ>
          <xsl:value-of select="$formattedMessageID"/>
        </SENDER_REFERENZ>
        <Benutzer_ID>
          <xsl:value-of select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='AlpoUserId']/s0:Value/text()"/>
        </Benutzer_ID>
      </SENDER>
      <EMPFANGSSYSTEM>
        <ID_EMPFANGSSYSTEM>
          <xsl:value-of select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Service Provider Settings' , 'RecipientID', $serviceProvider, $operationalPortCode, '')"/>
        </ID_EMPFANGSSYSTEM>
      </EMPFANGSSYSTEM>
      <NachrichtenZeitstempel>
        <xsl:call-template name="createTagZeit">
          <xsl:with-param name="dateTimeValue" select="s0:DataContext/s0:Workflow/s0:TriggerDate/text()"/>
        </xsl:call-template>
      </NachrichtenZeitstempel>
      <Hafenauftrag>
        <KopfDaten>
          <Bearbeitungszustand>
            <xsl:choose>
              <xsl:when test="$purpose='ORG'">NewAndSend</xsl:when>
              <xsl:when test="$purpose='AMD'">Replace</xsl:when>
              <xsl:when test="$purpose='WTH'">Cancelation</xsl:when>
            </xsl:choose>
          </Bearbeitungszustand>
          <Kommunikationsart>
            <xsl:value-of select="$communicationType"/>
          </Kommunikationsart>
          <Auftragsart>
            <xsl:choose>
              <xsl:when test="$communicationType='ZAPP'">HDS</xsl:when>
              <xsl:when test="$communicationType='BHT' and $containerMode='FCL' and $direction='Import'">138</xsl:when>
              <xsl:when test="$communicationType='BHT' and $containerMode='FCL' and $direction='Export'">125</xsl:when>
              <xsl:when test="$communicationType='BHT' and $containerMode='ROR'">023</xsl:when>
              <xsl:otherwise>022</xsl:otherwise>
            </xsl:choose>
          </Auftragsart>
          <Kundenreferenz>
            <xsl:value-of select="$consolID"/>
          </Kundenreferenz>
          <xsl:call-template name="CreateElementIfNotEmpty">
            <xsl:with-param name="elementName" select="concat($communicationType,'-Referenz')"/>
            <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='AlpoReference']/s0:Value/text()"/>
          </xsl:call-template>
          <Containerauftrag>
            <xsl:choose>
              <xsl:when test="$containerMode='FCL'">True</xsl:when>
              <xsl:otherwise>False</xsl:otherwise>
            </xsl:choose>
          </Containerauftrag>
          <Warenrichtung>
            <xsl:value-of select="$direction"/>
          </Warenrichtung>
          <Gefahrgut>
            <xsl:value-of select="$packingLineUNDGCount"/>
          </Gefahrgut>
          <AdresseAG>
            <Kundennummer/>
            <Name>
              <xsl:value-of select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='CurrentUser']/s0:CompanyName/text()"/>
            </Name>
            <Referenzen>
              <Sender>
                <xsl:value-of select="$fileSenderID"/>
              </Sender>
              <Abteilung>
                <xsl:value-of select="s0:DataContext/s0:Workflow/s0:EventDepartment/text()"/>
              </Abteilung>
              <SpeditionsbuchNr>
                <xsl:value-of select="$consolID"/>
              </SpeditionsbuchNr>
            </Referenzen>
          </AdresseAG>
          <Locationen>
            <xsl:if test="$warehouseCode!=''">
              <SchuppenCode>
                <xsl:value-of select="$warehouseCode"/>
              </SchuppenCode>
            </xsl:if>
            <Hafen>
              <LadeHafen>
                <xsl:value-of select="s0:PortOfLoading/text()"/>
              </LadeHafen>
              <LoeschHafen>
                <xsl:value-of select="s0:PortOfDischarge/text()"/>
              </LoeschHafen>
              <EndbestimmungsHafen>
                <xsl:value-of select="s0:PortOfDestination/text()"/>
              </EndbestimmungsHafen>
            </Hafen>
          </Locationen>
          <Schiffidentifikation>
            <xsl:call-template name="CreateElementIfNotEmpty">
              <xsl:with-param name="elementName" select="'SIS-NR'"/>
              <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='SIS_Number']/s0:Value/text()"/>
            </xsl:call-template>
            <SchiffsName>
              <xsl:value-of select="s0:VesselName/text()"/>
            </SchiffsName>
            <xsl:variable name="shippingLineAddress" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='ShippingLineAddress']" />
            <AdresseCA>
              <xsl:variable name="carrierRegNumber" select="$shippingLineAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()=substring($communicationType,1,3)]/s0:Value/text()"/>
              <xsl:if test="$carrierRegNumber!=''">
                <Kundennummer>
                  <Nummer>
                    <xsl:value-of select="userCSharp:PrependZero($carrierRegNumber)"/>
                  </Nummer>
                </Kundennummer>
              </xsl:if>
              <Name>
                <xsl:value-of select="$shippingLineAddress/s0:CompanyName/text()"/>
              </Name>
            </AdresseCA>
            <SchiffsDaten>
              <xsl:call-template name="CreateElementIfNotEmpty">
                <xsl:with-param name="elementName" select="'LloydsNr'"/>
                <xsl:with-param name="value" select="s0:LloydsIMO/text()"/>
              </xsl:call-template>
              <DatumETS>
                <xsl:call-template name="createTagZeit">
                  <xsl:with-param name="dateTimeValue" select="s0:DateCollection/s0:Date[s0:Type='Departure']/s0:Value/text()"/>
                </xsl:call-template>
              </DatumETS>
              <DatumETA>
                <xsl:call-template name="createTagZeit">
                  <xsl:with-param name="dateTimeValue" select="s0:DateCollection/s0:Date[s0:Type='Arrival']/s0:Value/text()"/>
                </xsl:call-template>
              </DatumETA>
            </SchiffsDaten>
          </Schiffidentifikation>
          <DivAngaben>
            <xsl:call-template name="CreateElementIfNotEmpty">
              <xsl:with-param name="elementName" select="'Buchungsnummer'"/>
              <xsl:with-param name="value" select="$bookingConfirmationReference"/>
            </xsl:call-template>
            <xsl:call-template name="CreateElementIfNotEmpty">
              <xsl:with-param name="elementName" select="'BLNummer'"/>
              <xsl:with-param name="value" select="s0:WayBillNumber/text()"/>
            </xsl:call-template>
            <xsl:call-template name="CreateElementIfNotEmpty">
              <xsl:with-param name="elementName" select="'Hauptmarkierung'"/>
              <xsl:with-param name="value" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='MarksAndNumbers']/s0:Value/text()"/>
            </xsl:call-template>
          </DivAngaben>

          <xsl:variable name="otherTransportMode" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='Other_TransportMode']/s0:Value/text()"/>
          <xsl:variable name="transportMode">
            <xsl:choose>
              <xsl:when test="$otherTransportMode='ROA'">Truck</xsl:when>
              <xsl:when test="$otherTransportMode='RAI'">Rail</xsl:when>
              <xsl:when test="$otherTransportMode='SEA'">Ship</xsl:when>
              <xsl:otherwise></xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <xsl:variable name="otherTransportID" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='Other_TransportID']/s0:Value/text()"/>
          <xsl:if test="$transportMode!=''">
            <VorNachTransport>
              <Verkehrstraeger>
                <xsl:value-of select="$transportMode"/>
              </Verkehrstraeger>

              <xsl:choose>
                <xsl:when test="$transportMode='Truck'">
                  <LKWDaten>
                    <VTKennzeichen>
                      <xsl:value-of select="$otherTransportID"/>
                    </VTKennzeichen>
                  </LKWDaten>
                </xsl:when>
                <xsl:when test="$transportMode='Rail'">
                  <BahnDaten>
                    <VTKennzeichen>
                      <xsl:value-of select="$otherTransportID"/>
                    </VTKennzeichen>
                  </BahnDaten>
                </xsl:when>
                <xsl:when test="$transportMode='Ship'">
                  <BischiDaten>
                    <VTKennzeichen>
                      <xsl:value-of select="$otherTransportID"/>
                    </VTKennzeichen>
                  </BischiDaten>
                </xsl:when>
              </xsl:choose>
            </VorNachTransport>
          </xsl:if>
        </KopfDaten>
        <PositionsDaten>
          <xsl:variable name="sendingForwarderNodeOfRootShipment" select="s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='SendingForwarderAddress']"/>
          <xsl:choose>
            <xsl:when test="count(s0:ContainerCollection/s0:Container) > 0">
              <xsl:for-each select="s0:ContainerCollection/s0:Container">
                <xsl:variable name="containerNumber" select="s0:ContainerNumber/text()"/>
                <xsl:if test="$containerNumber!=''">
                  <xsl:variable name="formattedPosition" select='format-number(position(), "0000")' />
                  <xsl:variable name="messageIdentifier" select="concat($formattedMessageID, '-', $formattedPosition)" />
                  <xsl:variable name="subscribeContainerNumber" select="DataModelAccessor:InsertSubscriptionValue($serviceProviderMSGID, $serviceProvider, $senderID, $messageIdentifier, $containerNumber, 'ContainerNumber')"/>
                </xsl:if>
                <WarenVerpackungsDaten>
                  <Zaehler>
                    <xsl:value-of select="position()"/>
                  </Zaehler>
                  <Gefahrgut>
                    <xsl:value-of select="count(//s0:PackingLine[s0:ContainerNumber/text() = $containerNumber]/s0:UNDGCollection/s0:UNDG) > 0"/>
                  </Gefahrgut>
                  <Spezifikation>
                    <Identifikation>
                      <xsl:value-of select="$containerNumber"/>
                    </Identifikation>
                    <xsl:call-template name="CreateElementIfNotEmpty">
                      <xsl:with-param name="elementName" select="'ContainerType'"/>
                      <xsl:with-param name="value" select="s0:ContainerType/s0:ISOCode/text()"/>
                    </xsl:call-template>
                    <xsl:call-template name="CreateElementIfNotEmpty">
                      <xsl:with-param name="elementName" select="'ShipperOwned'"/>
                      <xsl:with-param name="value" select="s0:IsShipperOwned/text()"/>
                    </xsl:call-template>
                    <xsl:call-template name="CreateElementIfNotEmpty">
                      <xsl:with-param name="elementName" select="'LeerContainer'"/>
                      <xsl:with-param name="value" select="s0:IsEmptyContainer/text()"/>
                    </xsl:call-template>
                    <MasseGewichte>
                      <Gewichte>
                        <xsl:call-template name="CreateElementIfNotEmpty">
                          <xsl:with-param name="elementName" select="'Tara-Einheit'"/>
                          <xsl:with-param name="value" select="s0:WeightUnit/text()"/>
                        </xsl:call-template>
                        <xsl:call-template name="CreateElementIfNotEmpty">
                          <xsl:with-param name="elementName" select="'Tara-Gewicht'"/>
                          <xsl:with-param name="value" select="s0:TareWeight/text()"/>
                        </xsl:call-template>
                        <xsl:call-template name="CreateElementIfNotEmpty">
                          <xsl:with-param name="elementName" select="'Brutto-Einheit'"/>
                          <xsl:with-param name="value" select="s0:WeightUnit/text()"/>
                        </xsl:call-template>
                        <xsl:call-template name="CreateElementIfNotEmpty">
                          <xsl:with-param name="elementName" select="'Brutto-Gewicht'"/>
                          <xsl:with-param name="value" select="s0:GrossWeight/text()"/>
                        </xsl:call-template>
                        <xsl:call-template name="CreateElementIfNotEmpty">
                          <xsl:with-param name="elementName" select="'Netto-Einheit'"/>
                          <xsl:with-param name="value" select="s0:WeightUnit/text()"/>
                        </xsl:call-template>
                        <xsl:call-template name="CreateElementIfNotEmpty">
                          <xsl:with-param name="elementName" select="'Netto-Gewicht'"/>
                          <xsl:with-param name="value" select="s0:GoodsWeight/text()"/>
                        </xsl:call-template>
                      </Gewichte>
                      <Volumen>
                        <Einheit>cbm</Einheit>
                        <Wert>
                          <xsl:value-of select="sum(//s0:PackingLine[s0:ContainerNumber/text() = $containerNumber]/s0:Volume/text())"/>
                        </Wert>
                      </Volumen>
                    </MasseGewichte>
                    <ContainerZusatzangaben>
                      <xsl:call-template name="CreateElementIfNotEmpty">
                        <xsl:with-param name="elementName" select="'Buchungsnummer'"/>
                        <xsl:with-param name="value" select="$bookingConfirmationReference"/>
                      </xsl:call-template>
                      <xsl:call-template name="CreateElementIfNotEmpty">
                        <xsl:with-param name="elementName" select="'Siegelnummer1'"/>
                        <xsl:with-param name="value" select="s0:Seal/text()"/>
                      </xsl:call-template>
                      <xsl:call-template name="CreateElementIfNotEmpty">
                        <xsl:with-param name="elementName" select="'Siegelnummer2'"/>
                        <xsl:with-param name="value" select="s0:SecondSeal/text()"/>
                      </xsl:call-template>
                      <xsl:call-template name="CreateElementIfNotEmpty">
                        <xsl:with-param name="elementName" select="'Siegelnummer3'"/>
                        <xsl:with-param name="value" select="s0:ThirdSeal/text()"/>
                      </xsl:call-template>
                    </ContainerZusatzangaben>
                  </Spezifikation>
                  <WarenPositionen>
                    <xsl:choose>
                      <xsl:when test="$isFormVersion200 = 'TRUE'">
                        <xsl:for-each select="//s0:PackingLine[generate-id(.) = generate-id(key('groupByContainerAndCargoItem', concat($containerNumber, s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CargoItem']/s0:Value/text())))]">
                          <xsl:variable name="groupedPackingLines" select="key('groupByContainerAndCargoItem', concat(./s0:ContainerNumber/text(), ./s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CargoItem']/s0:Value/text()))"/>
                          <xsl:variable name="sortedPackingLines">
                            <xsl:for-each select ="$groupedPackingLines">
                                <xsl:sort select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CargoItem']/s0:Value/text()" order="ascending"/>
                                <xsl:sort select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='PackageNumber']/s0:Value/text()" order="ascending"/>
                                <xsl:copy-of select="."/>
                            </xsl:for-each>
                          </xsl:variable>

                          <xsl:call-template name="createWarenDaten">
                            <xsl:with-param name="position" select="position()"/>
                            <xsl:with-param name="packLineNodes" select="msxsl:node-set($sortedPackingLines)/s0:PackingLine"/>
                            <xsl:with-param name="subShipmentMode" select="$subShipmentMode"/>
                            <xsl:with-param name="sendingForwarderNodeOfRootShipment" select="$sendingForwarderNodeOfRootShipment"/>
                            <xsl:with-param name="sendingForwarderNode" select="../../s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='SendingForwarderAddress']"/>
                            <xsl:with-param name="consignorNode" select="../../s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='ConsignorDocumentaryAddress']"/>
                            <xsl:with-param name="consigneeNode" select="../../s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='ConsigneeDocumentaryAddress']"/>
                            <xsl:with-param name="countryOfOrigin" select="substring(../../s0:PortOfOrigin/text(), 1, 2)"/>
                            <xsl:with-param name="countryOfDestination" select="substring(../../s0:PortOfDestination/text(), 1, 2)"/>
                            <xsl:with-param name="isFormVersion200" select="$isFormVersion200"/>
                          </xsl:call-template>
                      </xsl:for-each>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:for-each select="//s0:PackingLine[generate-id(.) = generate-id(key('groupByContainerAndCommodity', concat($containerNumber, s0:Commodity/text())))]">
                          <xsl:variable name="groupedPackingLines" select="key('groupByContainerAndCommodity', concat(./s0:ContainerNumber/text(), ./s0:Commodity/text()))"/>
                          <xsl:call-template name="createWarenDaten">
                            <xsl:with-param name="position" select="position()"/>
                            <xsl:with-param name="packLineNodes" select="$groupedPackingLines"/>
                            <xsl:with-param name="subShipmentMode" select="$subShipmentMode"/>
                            <xsl:with-param name="sendingForwarderNodeOfRootShipment" select="$sendingForwarderNodeOfRootShipment"/>
                            <xsl:with-param name="sendingForwarderNode" select="../../s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='SendingForwarderAddress']"/>
                            <xsl:with-param name="consignorNode" select="../../s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='ConsignorDocumentaryAddress']"/>
                            <xsl:with-param name="consigneeNode" select="../../s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='ConsigneeDocumentaryAddress']"/>
                            <xsl:with-param name="countryOfOrigin" select="substring(../../s0:PortOfOrigin/text(), 1, 2)"/>
                            <xsl:with-param name="countryOfDestination" select="substring(../../s0:PortOfDestination/text(), 1, 2)"/>
                            <xsl:with-param name="isFormVersion200" select="$isFormVersion200"/>
                          </xsl:call-template>
                        </xsl:for-each>
                      </xsl:otherwise>
                    </xsl:choose>
                  </WarenPositionen>
                </WarenVerpackungsDaten>
              </xsl:for-each>
            </xsl:when>
            <xsl:otherwise>
              <WarenVerpackungsDaten>
                <Zaehler>0</Zaehler>
                <Gefahrgut>
                  <xsl:value-of select="$packingLineUNDGCount"/>
                </Gefahrgut>
                <WarenPositionen>
                  <xsl:choose>
                    <xsl:when test="$isFormVersion200 = 'TRUE'">
                      <xsl:for-each select="//s0:PackingLine[generate-id(.) = generate-id(key('groupByCargoItem', s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CargoItem']/s0:Value/text()))]">
                        <xsl:variable name="groupedPackingLines" select="key('groupByCargoItem', ./s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CargoItem']/s0:Value/text())"/>
                        <xsl:call-template name="createWarenDaten">
                          <xsl:with-param name="position" select="position()"/>
                          <xsl:with-param name="packLineNodes" select="$groupedPackingLines"/>
                          <xsl:with-param name="subShipmentMode" select="$subShipmentMode"/>
                          <xsl:with-param name="sendingForwarderNodeOfRootShipment" select="$sendingForwarderNodeOfRootShipment"/>
                          <xsl:with-param name="sendingForwarderNode" select="../../s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='SendingForwarderAddress']"/>
                          <xsl:with-param name="consignorNode" select="../../s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='ConsignorDocumentaryAddress']"/>
                          <xsl:with-param name="consigneeNode" select="../../s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='ConsigneeDocumentaryAddress']"/>
                          <xsl:with-param name="countryOfOrigin" select="substring(../../s0:PortOfOrigin/text(), 1, 2)"/>
                          <xsl:with-param name="countryOfDestination" select="substring(../../s0:PortOfDestination/text(), 1, 2)"/>
                          <xsl:with-param name="isFormVersion200" select="$isFormVersion200"/>
                        </xsl:call-template>
                      </xsl:for-each>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:for-each select="//s0:PackingLine[generate-id(.) = generate-id(key('groupByCommodity', s0:Commodity/text()))]">
                        <xsl:variable name="groupedPackingLines" select="key('groupByCommodity', ./s0:Commodity/text())"/>
                        <xsl:call-template name="createWarenDaten">
                          <xsl:with-param name="position" select="position()"/>
                          <xsl:with-param name="packLineNodes" select="$groupedPackingLines"/>
                          <xsl:with-param name="subShipmentMode" select="$subShipmentMode"/>
                          <xsl:with-param name="sendingForwarderNodeOfRootShipment" select="$sendingForwarderNodeOfRootShipment"/>
                          <xsl:with-param name="sendingForwarderNode" select="../../s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='SendingForwarderAddress']"/>
                          <xsl:with-param name="consignorNode" select="../../s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='ConsignorDocumentaryAddress']"/>
                          <xsl:with-param name="consigneeNode" select="../../s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='ConsigneeDocumentaryAddress']"/>
                          <xsl:with-param name="countryOfOrigin" select="substring(../../s0:PortOfOrigin/text(), 1, 2)"/>
                          <xsl:with-param name="countryOfDestination" select="substring(../../s0:PortOfDestination/text(), 1, 2)"/>
                          <xsl:with-param name="isFormVersion200" select="$isFormVersion200"/>
                        </xsl:call-template>
                      </xsl:for-each>
                    </xsl:otherwise>
                  </xsl:choose>
                </WarenPositionen>
              </WarenVerpackungsDaten>
            </xsl:otherwise>
          </xsl:choose>
        </PositionsDaten>
      </Hafenauftrag>

    </AdvantageEnterpriseVersion0.1>
  </xsl:template>

  <xsl:template name="CreateElementIfNotEmpty">
    <xsl:param name="elementName" />
    <xsl:param name="value" />

    <xsl:if test="$value!=''">
      <xsl:element name="{$elementName}">
        <xsl:value-of select="$value"/>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="GetValueWithFallback">
    <xsl:param name="value"/>
    <xsl:param name="fallbackValue" select="''"/>

    <xsl:choose>
      <xsl:when test="$value !=''">
        <xsl:value-of select="$value"/>
      </xsl:when>
      <xsl:when test="$fallbackValue !=''">
        <xsl:value-of select="$fallbackValue"/>
      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="createTagZeit">
    <xsl:param name="dateTimeValue"/>

    <xsl:if test="$dateTimeValue!=''">
      <Tag>
        <xsl:value-of select="substring-before($dateTimeValue,'T')"/>
      </Tag>
      <Zeit>
        <xsl:value-of select="substring(substring-after($dateTimeValue,'T'),1,8)"/>
      </Zeit>
    </xsl:if>
  </xsl:template>

  <xsl:template name="GetPackType">
    <xsl:param name="packType"/>
    <xsl:param name="fallbackValue" select="''"/>

    <xsl:variable name="serviceProviderPackTypeCode" select="CodeMapper:GetRecipientCode($serviceProvider, $serviceProvider, concat($serviceProvider, ' System Configuration'), 'Package Type ISO' , concat($serviceProvider, ' Code'),  $packType)"/>
    <xsl:choose>
      <xsl:when test="$serviceProviderPackTypeCode != ''">
        <xsl:value-of select="$serviceProviderPackTypeCode"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:variable name="FPMPackTypeCode" select="CodeMapper:GetRecipientCode('FORWARDING_PORT_MESSAGE' , 'FORWARDING_PORT_MESSAGE' , 'FPM System Configuration' , 'Package Type ISO' , 'Output Code', $packType)"/>
        <xsl:choose>
          <xsl:when test="$FPMPackTypeCode != ''">
            <xsl:value-of select="$FPMPackTypeCode"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$fallbackValue"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="createWarenDaten">
    <xsl:param name="position"/>
    <xsl:param name="packLineNodes"/>
    <xsl:param name="subShipmentMode"/>
    <xsl:param name="sendingForwarderNodeOfRootShipment"/>
    <xsl:param name="sendingForwarderNode"/>
    <xsl:param name="consignorNode"/>
    <xsl:param name="consigneeNode"/>
    <xsl:param name="countryOfOrigin"/>
    <xsl:param name="countryOfDestination"/>
    <xsl:param name="isFormVersion200"/>

    <xsl:variable name="packTypeMappingCode">
      <xsl:call-template name="GetPackType">
        <xsl:with-param name="packType" select="$packLineNodes[1]/s0:PackType/text()"/>
      </xsl:call-template>
    </xsl:variable>

    <WarenDaten>
      <Zaehler>
        <xsl:value-of select="$position"/>
      </Zaehler>
      <xsl:call-template name="CreateElementIfNotEmpty">
        <xsl:with-param name="elementName" select="'Bemerkungen'"/>
        <xsl:with-param name="value" select="$packLineNodes[1]/s0:OutturnComment/text()"/>
      </xsl:call-template>
      <Gefahrgut>
        <xsl:value-of select="count($packLineNodes/s0:UNDGCollection/s0:UNDG) > 0"/>
      </Gefahrgut>
      <xsl:call-template name="CreateElementIfNotEmpty">
        <xsl:with-param name="elementName" select="'Markierung'"/>
        <xsl:with-param name="value" select="$packLineNodes[1]/s0:MarksAndNos/text()"/>
      </xsl:call-template>
      <WarenAngaben>
        <xsl:call-template name="CreateElementIfNotEmpty">
          <xsl:with-param name="elementName" select="'Anzahl'"/>
          <xsl:with-param name="value" select="sum($packLineNodes/s0:PackQty/text())"/>
        </xsl:call-template>
        <xsl:call-template name="CreateElementIfNotEmpty">
          <xsl:with-param name="elementName" select="'VerpackungsArt'"/>
          <xsl:with-param name="value" select="$packTypeMappingCode"/>
        </xsl:call-template>
        <xsl:call-template name="CreateElementIfNotEmpty">
          <xsl:with-param name="elementName" select="'Warenbeschreibung'"/>
          <xsl:with-param name="value">
            <xsl:variable name="commodityDescription" select="$packLineNodes[1]/s0:Commodity/@Description/text()"/>
            <xsl:choose>
              <xsl:when test="$commodityDescription != ''">
                <xsl:value-of select="$commodityDescription"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:variable name="detailedDescription" select="$packLineNodes[1]/s0:DetailedDescription/text()"/>
                <xsl:choose>
                  <xsl:when test="$detailedDescription != ''">
                    <xsl:value-of select="$detailedDescription"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="$packLineNodes[1]/s0:GoodsDescription/text()"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:with-param>
        </xsl:call-template>
        <xsl:call-template name="CreateElementIfNotEmpty">
          <xsl:with-param name="elementName" select="'BLGWaCo'"/>
          <xsl:with-param name="value" select="$packLineNodes[1]/s0:Commodity/text()"/>
        </xsl:call-template>
        <xsl:call-template name="CreateElementIfNotEmpty">
          <xsl:with-param name="elementName" select="'MarksNos'"/>
          <xsl:with-param name="value" select="$packLineNodes[1]/s0:MarksAndNos/text()"/>
        </xsl:call-template>
        <MasseGewichte>
          <Gewichte>
            <xsl:variable name="firstWeightUnit" select="$packLineNodes[1]/s0:WeightUnit/text()"/>
            <xsl:variable name="areWeightUnitsTheSame" select="count($packLineNodes[s0:WeightUnit/text() = $firstWeightUnit]) = count($packLineNodes)"/>
            <xsl:call-template name="CreateElementIfNotEmpty">
              <xsl:with-param name="elementName" select="'Brutto-Einheit'"/>
              <xsl:with-param name="value">
                <xsl:choose>
                  <xsl:when test="$areWeightUnitsTheSame = 'true'">
                    <xsl:value-of select="$firstWeightUnit"/>
                  </xsl:when>
                  <xsl:otherwise>KGM</xsl:otherwise>
                </xsl:choose>
              </xsl:with-param>
            </xsl:call-template>
            <xsl:call-template name="CreateElementIfNotEmpty">
              <xsl:with-param name="elementName" select="'Brutto-Gewicht'"/>
              <xsl:with-param name="value">
                <xsl:choose>
                  <xsl:when test="$areWeightUnitsTheSame = 'true'">
                    <xsl:value-of select="sum($packLineNodes/s0:Weight/text())"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:variable name="weightInKGM">
                      <xsl:for-each select="$packLineNodes">
                        <weight>
                          <xsl:value-of select="StringMapper:FormatDecimal(UnitConverter:Convert(s0:Weight/text(), s0:WeightUnit/text(), 'KG'), '0.000', true())"/>
                        </weight>
                      </xsl:for-each>
                    </xsl:variable>
                    <xsl:value-of select="sum(msxsl:node-set($weightInKGM)/weight/text())"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:with-param>
            </xsl:call-template>
          </Gewichte>
          <Volumen>
            <Einheit>cbm</Einheit>
            <xsl:call-template name="CreateElementIfNotEmpty">
              <xsl:with-param name="elementName" select="'Wert'"/>
              <xsl:with-param name="value" select="sum($packLineNodes/s0:Volume/text())"/>
            </xsl:call-template>
          </Volumen>
        </MasseGewichte>

        <xsl:if test="$packLineNodes/s0:UNDGCollection/s0:UNDG">
          <GefahrgutPositionen>
            <xsl:for-each select="$packLineNodes/s0:UNDGCollection/s0:UNDG">
              <xsl:variable name="undgPackTypeMappingCode">
                <xsl:call-template name="GetPackType">
                  <xsl:with-param name="packType" select="s0:PackType/text()"/>
                  <xsl:with-param name="fallbackValue" select="$packTypeMappingCode"/>
                </xsl:call-template>
              </xsl:variable>

              <GefahrgutDaten>
                <Zaehler>
                  <xsl:value-of select="position()"/>
                </Zaehler>
                <xsl:call-template name="CreateElementIfNotEmpty">
                  <xsl:with-param name="elementName" select="'Aussteller'"/>
                  <xsl:with-param name="value" select="$sendingForwarderNode/s0:CompanyName/text()"/>
                </xsl:call-template>
                <Anzahl>
                  <xsl:value-of select="s0:PackQty/text()"/>
                </Anzahl>
                <xsl:call-template name="CreateElementIfNotEmpty">
                  <xsl:with-param name="elementName" select="'VerpackungsArt'"/>
                  <xsl:with-param name="value" select="$undgPackTypeMappingCode"/>
                </xsl:call-template>
                <Klasse>
                  <xsl:value-of select="s0:IMOClass/text()"/>
                </Klasse>
                <UN-NR>
                  <xsl:value-of select="substring(s0:UNDGCode/text(), 1, 4)"/>
                </UN-NR>
                <xsl:call-template name="CreateElementIfNotEmpty">
                  <xsl:with-param name="elementName" select="'PrimaerLabel'"/>
                  <xsl:with-param name="value" select="s0:SubLabel1/text()"/>
                </xsl:call-template>
                <xsl:call-template name="CreateElementIfNotEmpty">
                  <xsl:with-param name="elementName" select="'VPGGruppe'"/>
                  <xsl:with-param name="value" select="s0:PackingGroup/text()"/>
                </xsl:call-template>
                <xsl:variable name="FlashPoint" select="s0:FlashPoint/text()"/>
                <xsl:if test="$FlashPoint!='' and number($FlashPoint)!='NaN'">
                  <EinheitFP>C</EinheitFP>
                  <Flammpunkt>
                    <xsl:value-of select="format-number(floor($FlashPoint),'+0;-0')"/>
                  </Flammpunkt>
                </xsl:if>
                <GGBrutto>
                  <xsl:value-of select="s0:Weight/text()"/>
                </GGBrutto>
                <xsl:variable name="netExplosiveWeight" select="s0:NetExplosiveWeight/text()"/>
                <xsl:if test="starts-with(s0:IMOClass/text(), '1') and $netExplosiveWeight != ''">
                  <ExplosivGewicht>
                    <xsl:value-of select="StringMapper:FormatDecimal(UnitConverter:Convert($netExplosiveWeight, s0:NetExplosiveWeightUQ/text(), 'KG'), '0.0', true())"/>
                  </ExplosivGewicht>
                </xsl:if>
                <xsl:call-template name="CreateElementIfNotEmpty">
                  <xsl:with-param name="elementName" select="'TechnischeBezeichnung'"/>
                  <xsl:with-param name="value" select="s0:ProperShippingName/text()"/>
                </xsl:call-template>
                <xsl:call-template name="CreateElementIfNotEmpty">
                  <xsl:with-param name="elementName" select="'Gefahrausloeser'"/>
                  <xsl:with-param name="value" select="s0:TechicalName/text()"/>
                </xsl:call-template>
                <xsl:call-template name="CreateElementIfNotEmpty">
                  <xsl:with-param name="elementName" select="'LimitedQuantities'"/>
                  <xsl:with-param name="value" select="s0:PackedInLimitedQuantity/text()"/>
                </xsl:call-template>
              </GefahrgutDaten>
            </xsl:for-each>
          </GefahrgutPositionen>
        </xsl:if>

        <xsl:variable name="direction" select="$shipment/s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'Direction']/s0:Value/text()" />
        <xsl:if test="$direction!='Import'">
          <ZollPositionen>
            <xsl:for-each select="$packLineNodes">
              <xsl:variable name="cargoItem" select="normalize-space(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CargoItem']/s0:Value/text())"/>
              <xsl:variable name="entryType" select="s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='EntryType']/s0:Value/text()"/>
              <ZollDaten>
                <ZollPosZaehler>1</ZollPosZaehler>
                <xsl:if test="$subShipmentMode = 'TRUE'">
                  <xsl:variable name="exportReferenceNumber" select="s0:ExportReferenceNumber/text()"/>
                  <xsl:variable name="customsStatusCompleteValue" select="userCSharp:ToUpper(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='CustomsStatusComplete']/s0:Value/text())"/>
                  <xsl:variable name="shortageValue" select="userCSharp:ToUpper(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='Shortage']/s0:Value/text())"/>
                  <xsl:variable name="shortage">
                    <xsl:choose>
                      <xsl:when test="$shortageValue = 'Y' or $shortageValue = 'TRUE'">True</xsl:when>
                      <xsl:otherwise>False</xsl:otherwise>
                    </xsl:choose>
                  </xsl:variable>
                  <xsl:variable name="packageNumber" select="normalize-space(s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='PackageNumber']/s0:Value/text())"/>
                  <xsl:variable name="customsStatusComplete">
                    <xsl:choose>
                      <xsl:when test="$customsStatusCompleteValue = 'Y' or $customsStatusCompleteValue = 'TRUE' or $customsStatusCompleteValue = ''">True</xsl:when>
                      <xsl:otherwise>False</xsl:otherwise>
                    </xsl:choose>
                  </xsl:variable>
                  <xsl:variable name="isMRNComplete">
                    <xsl:choose>
                      <xsl:when test="$shortageValue = '' and (count(//s0:PackingLine[s0:ExportReferenceNumber/text() = $exportReferenceNumber and (s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'CustomsStatusComplete']/s0:Value/text() = 'Y' or s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'CustomsStatusComplete']/s0:Value/text() = 'TRUE' or s0:AddInfoCollection/s0:AddInfo[s0:Key/text() = 'CustomsStatusComplete']/s0:Value/text() = '')]) > 0)">True</xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="$customsStatusComplete"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:variable>
                  <xsl:variable name="packLineNodesWithSameMRN" select="//s0:PackingLine[s0:ExportReferenceNumber/text() = $exportReferenceNumber]"/>
                  <xsl:variable name="commodity" select="s0:Commodity/text()"/>

                  <xsl:if test="$entryType = 'AE1' and $exportReferenceNumber != ''">
                    <AESZollDaten>
                      <AESZaehler>
                        <xsl:value-of select="position()"/>
                      </AESZaehler>
                      <LRN>
                        <xsl:value-of select="$exportReferenceNumber"/>
                      </LRN>
                      <xsl:variable name="lrnKomplett">
                        <xsl:choose>
                          <xsl:when test="$isFormVersion200 = 'TRUE'">
                            <xsl:choose>
                              <xsl:when test="$cargoItem=''">
                                <xsl:value-of select="$customsStatusComplete"/>
                              </xsl:when>
                              <xsl:otherwise>False</xsl:otherwise>
                            </xsl:choose>
                          </xsl:when>
                          <xsl:otherwise>
                            <xsl:choose>
                              <xsl:when test="$customsStatusCompleteValue = ''">True</xsl:when>
                              <xsl:when test="count($packLineNodesWithSameMRN) > 1">False</xsl:when>
                              <xsl:otherwise>
                                <xsl:value-of select="$isMRNComplete"/>
                              </xsl:otherwise>
                            </xsl:choose>
                          </xsl:otherwise>
                        </xsl:choose>
                      </xsl:variable>
                      <LRNKomplett>
                        <xsl:value-of select="$lrnKomplett"/>
                      </LRNKomplett>
                        <xsl:choose>
		                      <xsl:when test="$isFormVersion200 = 'TRUE'">
                            <xsl:call-template name="AESZollDaten_Extra_FormVersion200">
                              <xsl:with-param name="packLine" select="."/>
                              <xsl:with-param name="komplett" select="$lrnKomplett"/>
                              <xsl:with-param name="cargoItem" select="$cargoItem"/>
                              <xsl:with-param name="packageNumber" select="$packageNumber"/>
                              <xsl:with-param name="customsStatusComplete" select="$customsStatusComplete"/>
                              <xsl:with-param name="shortage" select="$shortage"/>
                            </xsl:call-template>
		                      </xsl:when>
		                      <xsl:otherwise>
                            <xsl:call-template name="AESZollDaten_Extra">
                              <xsl:with-param name="packLine" select="."/>
                              <xsl:with-param name="position" select="position()"/>
                              <xsl:with-param name="commodityPosition" select="$position"/>
                              <xsl:with-param name="packLineNodesWithSameMRN" select="$packLineNodesWithSameMRN"/>
                              <xsl:with-param name="commodity" select="$commodity"/>
                              <xsl:with-param name="shortageValue" select="$shortageValue"/>
                              <xsl:with-param name="customsStatusComplete" select="$customsStatusComplete"/>
                              <xsl:with-param name="komplett" select="$lrnKomplett"/>
                            </xsl:call-template>
		                      </xsl:otherwise>
	                      </xsl:choose>

                      <xsl:variable name="consignorEORValue" select="normalize-space($consignorNode/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type='EOR']/s0:Value/text())" />
                      <xsl:variable name="consignorEBSValue" select="normalize-space($consignorNode/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type='EBS']/s0:Value/text())" />

                      <xsl:choose>
                        <xsl:when test="$consignorEORValue!='' and $consignorEBSValue!=''">
                          <AnmelderIdentifikationsNr>
                            <xsl:value-of select="concat($consignorNode/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type='EOR']/s0:CountryOfIssue/text(), $consignorEORValue)"/>
                          </AnmelderIdentifikationsNr>
                          <AnmelderNiederlassungsNr>
                            <xsl:value-of select="$consignorEBSValue"/>
                          </AnmelderNiederlassungsNr>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:variable name="sendingForwarderEORValueOfShipment" select="normalize-space($sendingForwarderNodeOfRootShipment/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type='EOR']/s0:Value/text())" />
                          <xsl:variable name="sendingForwarderEBSValueOfShipment" select="normalize-space($sendingForwarderNodeOfRootShipment/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type='EBS']/s0:Value/text())" />
                          <xsl:variable name="sendingForwarderEORCountryOfShipment" select="normalize-space($sendingForwarderNodeOfRootShipment/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type='EOR']/s0:CountryOfIssue/text())" />
                          <xsl:if test="$sendingForwarderEORValueOfShipment != '' and $sendingForwarderEBSValueOfShipment!= ''">
                            <VertreterIdentifikationsNr>
                              <xsl:value-of select="concat($sendingForwarderEORCountryOfShipment, $sendingForwarderEORValueOfShipment)"/>
                            </VertreterIdentifikationsNr>
                            <VertreterNiederlassungsNr>
                              <xsl:value-of select="$sendingForwarderEBSValueOfShipment"/>
                            </VertreterNiederlassungsNr>
                          </xsl:if>
                        </xsl:otherwise>
                      </xsl:choose>
                    </AESZollDaten>
                  </xsl:if>

                  <xsl:if test="$entryType = 'AES' and $exportReferenceNumber != ''">
                    <AESZollDaten>
                      <AESZaehler>
                        <xsl:value-of select="position()"/>
                      </AESZaehler>
                      <MRN>
                        <xsl:value-of select="$exportReferenceNumber"/>
                      </MRN>
                      <xsl:variable name="mrnKomplett">
                        <xsl:choose>
                          <xsl:when test="$isFormVersion200 = 'TRUE'">
                            <xsl:choose>
                              <xsl:when test="$cargoItem=''">
                                <xsl:value-of select="$customsStatusComplete"/>
                              </xsl:when>
                              <xsl:otherwise>False</xsl:otherwise>
                            </xsl:choose>
                          </xsl:when>
                          <xsl:otherwise>
                            <xsl:choose>
                              <xsl:when test="$customsStatusComplete = ''">True</xsl:when>
                              <xsl:when test="count($packLineNodesWithSameMRN) > 1">False</xsl:when>
                              <xsl:otherwise>
                                <xsl:value-of select="$customsStatusComplete"/>
                              </xsl:otherwise>
                            </xsl:choose>
                          </xsl:otherwise>
                        </xsl:choose>
                      </xsl:variable>
                      <MRNKomplett>
                        <xsl:value-of select="$mrnKomplett"/>
                      </MRNKomplett>
                        <xsl:choose>
		                      <xsl:when test="$isFormVersion200 = 'TRUE'">
                            <xsl:call-template name="AESZollDaten_Extra_FormVersion200">
                              <xsl:with-param name="packLine" select="."/>
                              <xsl:with-param name="komplett" select="$mrnKomplett"/>
                              <xsl:with-param name="cargoItem" select="$cargoItem"/>
                              <xsl:with-param name="packageNumber" select="$packageNumber"/>
                              <xsl:with-param name="customsStatusComplete" select="$customsStatusComplete"/>
                              <xsl:with-param name="shortage" select="$shortage"/>
                            </xsl:call-template>
		                      </xsl:when>
		                      <xsl:otherwise>
                            <xsl:call-template name="AESZollDaten_Extra">
                              <xsl:with-param name="packLine" select="."/>
                              <xsl:with-param name="position" select="position()"/>
                              <xsl:with-param name="commodityPosition" select="$position"/>
                              <xsl:with-param name="packLineNodesWithSameMRN" select="$packLineNodesWithSameMRN"/>
                              <xsl:with-param name="commodity" select="$commodity"/>
                              <xsl:with-param name="shortageValue" select="$shortageValue"/>
                              <xsl:with-param name="customsStatusComplete" select="$customsStatusComplete"/>
                              <xsl:with-param name="komplett" select="$mrnKomplett"/>
                            </xsl:call-template>
		                      </xsl:otherwise>
	                      </xsl:choose>
                    </AESZollDaten>
                  </xsl:if>

                  <xsl:if test="$entryType != 'AE1' and $entryType != 'AES' and $entryType != 'NA'">
                    <ZollDatenBHT>
                      <ZollZaehler>1</ZollZaehler>
                      <VersandStatus>
                        <xsl:value-of select="$entryType"/>
                      </VersandStatus>
                      <AusfuehrerName>
                        <xsl:value-of select="$consignorNode/s0:CompanyName/text()"/>
                      </AusfuehrerName>
                      <Herkunftsland>
                        <xsl:value-of select="$countryOfOrigin"/>
                      </Herkunftsland>
                      <Bestimmungsland>
                        <xsl:value-of select="$countryOfDestination"/>
                      </Bestimmungsland>
                      <Warenbeschreibung>
                        <xsl:value-of select="substring(s0:DetailedDescription/text(), 1, 240)"/>
                      </Warenbeschreibung>
                      <StatWarennr>
                        <xsl:value-of select="s0:HarmonisedCode/text()"/>
                      </StatWarennr>
                      <Vorpapier>
                        <xsl:value-of select="$exportReferenceNumber"/>
                      </Vorpapier>
                      <Warenempfaenger>
                        <xsl:value-of select="$consigneeNode/s0:CompanyName/text()"/>
                      </Warenempfaenger>
                    </ZollDatenBHT>
                  </xsl:if>
                </xsl:if>
              </ZollDaten>
            </xsl:for-each>
          </ZollPositionen>
        </xsl:if>
      </WarenAngaben>
    </WarenDaten>
  </xsl:template>

  <xsl:template name="AESZollDaten_Extra">
    <xsl:param name="packLine"/>
    <xsl:param name="position"/>
    <xsl:param name="commodityPosition"/>
    <xsl:param name="packLineNodesWithSameMRN"/>
    <xsl:param name="commodity"/>
    <xsl:param name="shortageValue"/>
    <xsl:param name="customsStatusComplete"/>
    <xsl:param name="komplett"/>

    <xsl:variable name="containerNumber" select="$packLine/s0:ContainerNumber/text()"/>

    <xsl:if test="$komplett = 'False'">
      <WarenPos>
        <xsl:value-of select="$commodityPosition"/>
      </WarenPos>
      <xsl:variable name="warenPosKomplett">
        <xsl:choose>
          <xsl:when test="count($packLineNodesWithSameMRN[s0:ContainerNumber/text() = $containerNumber and s0:Commodity/text() = $commodity]) > 1">False</xsl:when>
          <xsl:otherwise>
            <xsl:choose>
              <xsl:when test="$shortageValue = ''">
                <xsl:value-of select="$customsStatusComplete"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:choose>
                  <xsl:when test="$shortageValue = 'Y'">False</xsl:when>
                  <xsl:when test="$shortageValue = 'N'">True</xsl:when>
                  <xsl:otherwise></xsl:otherwise>
                </xsl:choose>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <WarenPosKomplett>
        <xsl:value-of select="$warenPosKomplett"/>
      </WarenPosKomplett>
      <xsl:if test="$warenPosKomplett = 'False'">
        <PackStueckLfdNr>
          <xsl:value-of select="$position"/>
        </PackStueckLfdNr>
        <xsl:variable name="packstueckKomplett">
          <xsl:choose>
            <xsl:when test="$shortageValue = ''">
              <xsl:value-of select="$customsStatusComplete"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:choose>
                <xsl:when test="$shortageValue = 'Y'">False</xsl:when>
                <xsl:when test="$shortageValue = 'N'">True</xsl:when>
                <xsl:otherwise></xsl:otherwise>
              </xsl:choose>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <PackStueckKomplett>
          <xsl:value-of select="$packstueckKomplett"/>
        </PackStueckKomplett>
        <PackStueckAnzahl>
          <xsl:value-of select="$packLine/s0:PackQty/text()"/>
        </PackStueckAnzahl>
        <xsl:if test="$packstueckKomplett = 'False'">
          <Mindermenge>
            <xsl:choose>
              <xsl:when test="$shortageValue = ''">
                <xsl:choose>
                  <xsl:when test="$customsStatusComplete = 'True'">False</xsl:when>
                  <xsl:when test="$customsStatusComplete = 'False'">True</xsl:when>
                  <xsl:otherwise></xsl:otherwise>
                </xsl:choose>
              </xsl:when>
              <xsl:otherwise>
                <xsl:choose>
                  <xsl:when test="$shortageValue = 'Y'">True</xsl:when>
                  <xsl:when test="$shortageValue = 'N'">False</xsl:when>
                  <xsl:otherwise></xsl:otherwise>
                </xsl:choose>
              </xsl:otherwise>
            </xsl:choose>
          </Mindermenge>
          <RestgewichtNetto>
            <xsl:value-of select="$packLine/s0:Weight/text()"/>
          </RestgewichtNetto>
          <BruttoGewicht>
            <xsl:value-of select="$packLine/s0:Weight/text()"/>
          </BruttoGewicht>
        </xsl:if>
      </xsl:if>
    </xsl:if>
  </xsl:template>

  <xsl:template name="AESZollDaten_Extra_FormVersion200">
    <xsl:param name="packLine"/>
    <xsl:param name="komplett"/>
    <xsl:param name="cargoItem"/>
    <xsl:param name="packageNumber"/>
    <xsl:param name="customsStatusComplete"/>
    <xsl:param name="shortage"/>

    <xsl:if test="$komplett = 'False'">
      <WarenPos>
        <xsl:value-of select="$cargoItem"/>
      </WarenPos>

      <xsl:variable name="warenPosKomplett">
        <xsl:choose>
          <xsl:when test="$packageNumber=''">
            <xsl:value-of select="$customsStatusComplete"/>
          </xsl:when>
          <xsl:otherwise>False</xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <WarenPosKomplett>
        <xsl:value-of select="$warenPosKomplett"/>
      </WarenPosKomplett>

      <xsl:if test="$warenPosKomplett = 'False'">

        <xsl:variable name="packStueckKomplett" select="$customsStatusComplete"/>

        <PackStueckLfdNr>
          <xsl:value-of select="$packageNumber"/>
        </PackStueckLfdNr>
        <PackStueckKomplett>
          <xsl:value-of select="$packStueckKomplett"/>
        </PackStueckKomplett>
        <PackStueckAnzahl>
          <xsl:value-of select="$packLine/s0:PackQty/text()"/>
        </PackStueckAnzahl>

        <xsl:if test="$packStueckKomplett = 'False' and $shortage = 'True'">
          <Mindermenge>
            <xsl:value-of select="$shortage"/>
          </Mindermenge>
          <RestgewichtNetto>
            <xsl:value-of select="$packLine/s0:Weight/text()"/>
          </RestgewichtNetto>
          <BruttoGewicht>
            <xsl:value-of select="$packLine/s0:Weight/text()"/>
          </BruttoGewicht>
        </xsl:if>
      </xsl:if>
    </xsl:if>
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

   public string PrependZero(string inputValue)
   {
      int prependZeroCount = 10 - inputValue.Length;
      var zeros = new StringBuilder();
      for (int i = 0; i < prependZeroCount; i++)
      {
         zeros.Append("0");
      }
      return zeros.ToString() + inputValue;
   }
]]>
  </msxsl:script>
</xsl:stylesheet>

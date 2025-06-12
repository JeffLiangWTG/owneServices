<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet xmlns:xsl        = "http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl      = "urn:schemas-microsoft-com:xslt"
                xmlns:var        = "http://schemas.microsoft.com/BizTalk/2003/var" exclude-result-prefixes="msxsl var s0 ScriptNS0 ScriptNS1" version="1.0"
                xmlns:s0         = "http://CargoWise.eHub.Clients.Common.CFXML_Schemas.CustomsEDIResponse"
                xmlns            = "http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:ScriptNS0  = "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
                xmlns:ScriptNS1  = "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1">

  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" encoding="UTF-8" indent="yes"/>

  <xsl:variable name="Sender"     select="'CFCUSTRESPONSE'"/>
  <xsl:variable name="Recipient"  select="'CFCUSTRESPONSE'"/>
  <xsl:variable name="TS_Name"    select="'CFCUSTRESPONSE'"/>
  <xsl:variable name="ClientCode" select="'CFCUSTRESPONSE'"/>

  <xsl:variable name="Interchange" select="/*[local-name()='UniversalInterchange']
                                           /*[local-name()='Body']
                                           /*[local-name()='UniversalShipment']"/>

  <xsl:variable name="Declaration" select="$Interchange/*[local-name()='Shipment']
                                                       /*[local-name()='SubShipmentCollection']
                                                       /*[local-name()='SubShipment']
                                                         [contains(./*[local-name()='DataContext']/*[local-name()='DataSourceCollection'],'CustomsDeclaration')] |
                                           $Interchange/*[local-name()='Shipment']
                                                         [contains(./*[local-name()='DataContext']/*[local-name()='DataSourceCollection'],'CustomsDeclaration')]"/>
  <!--###################-->
  <!--## Main Template ##-->
  <!--###################-->

  <xsl:template match="/">

    <Customs_EDI_Response>

      <xsl:call-template name="MessageHeader"/>

      <xsl:call-template name="MessageDetail"/>

    </Customs_EDI_Response>

  </xsl:template>

  <!--####################-->
  <!--## Message Header ##-->
  <!--####################-->

  <xsl:template name="MessageHeader">

    <xsl:variable name="Timestamp" select="ScriptNS1:ConvertToDateTimeString($Declaration/*[local-name()='AddInfoCollection']
                                                                                         /*[local-name()='AddInfo']
                                                                                       [./*[local-name()='Key'] = 'MasterBillIssuedDate']
                                                                                         /*[local-name()='Value'])"/>
    <MessageHeader>
      <SenderID>
        <xsl:value-of select="$Sender"/>
      </SenderID>

      <RecipientID>
        <xsl:value-of select="$Recipient"/>
      </RecipientID>

      <MessageID>
        <xsl:text>0</xsl:text>
      </MessageID>

      <MessageType>
        <xsl:text>CFXML_Customs_EDI_Response</xsl:text>
      </MessageType>

      <Timestamp>
        <xsl:value-of select="$Timestamp"/>
      </Timestamp>

      <Action>
        <xsl:text>Add</xsl:text>
      </Action>
    </MessageHeader>

  </xsl:template>

  <!--####################-->
  <!--## Message Detail ##-->
  <!--####################-->

  <xsl:template name="MessageDetail">

    <File_Number>
      <xsl:value-of select="$Declaration/*[local-name()='DataContext']
                                        /*[local-name()='DataSourceCollection']
                                        /*[local-name()='DataSource']
                                      [./*[local-name()='Type'] = 'CustomsDeclaration']
                                        /*[local-name()='Key']"/>
    </File_Number>

    <SequenceNo>
      <xsl:text>1</xsl:text>
    </SequenceNo>

    <IsVOC>
      <xsl:value-of select="not(count($Declaration/*[local-name()='EntryHeaderCollection']
                                                  /*[local-name()='EntryHeader']
                                                  /*[local-name()='MessageStatus']
                                                [./*[local-name()='Code'] = 'ACO']) > 0)"/>
    </IsVOC>

    <BranchCode>
      <xsl:value-of select="$Declaration/*[local-name()='DataContext']
                                        /*[local-name()='EventDepartment']
                                        /*[local-name()='Code']"/>
    </BranchCode>

    <xsl:variable name="MRN" select="$Declaration/*[local-name()='EntryHeaderCollection']
                                                 /*[local-name()='EntryHeader']
                                                 /*[local-name()='EntryNumberCollection']
                                                 /*[local-name()='EntryNumber']
                                               [./*[local-name()='Type']
                                                 /*[local-name()='Code']='MRN']
                                                 /*[local-name()='Number']"/>

    <HouseBOENo>
      <xsl:value-of select="$MRN"/>
    </HouseBOENo>

    <MessageNotificationNo>
      <xsl:text>0</xsl:text>
    </MessageNotificationNo>

    <OLMessageReceivedNo>
      <xsl:text>0</xsl:text>
    </OLMessageReceivedNo>

    <UniqueSerialNo>
      <xsl:value-of select="$MRN"/>
    </UniqueSerialNo>

    <xsl:variable name="LRN" select="$Declaration/*[local-name()='EntryHeaderCollection']
                                                 /*[local-name()='EntryHeader']
                                                 [*[local-name()='Type']/*[local-name()='Code'] = 'IMP']
                                                 /*[local-name()='Reference']"/>
    <LRN>
      <xsl:value-of select="$LRN"/>
    </LRN>

    <xsl:choose>
      <xsl:when test="count($Declaration/*[local-name()='EntryHeaderCollection']
                                        /*[local-name()='EntryHeader']
                                        /*[local-name()='MessageStatus']
                                      [./*[local-name()='Code']='ACO']) > 0">
        <TransactionType>
          <xsl:text>9</xsl:text>
        </TransactionType>
      </xsl:when>

      <xsl:otherwise>
        <TransactionType>
          <xsl:text>4</xsl:text>
        </TransactionType>
      </xsl:otherwise>
    </xsl:choose>

    <xsl:if test="$Declaration/*[local-name()='MessageType']
                              /*[local-name()='Code']='IMP'">
      <xsl:choose>
        <xsl:when test="count($Declaration/*[local-name()='CommercialInfo']
                                          /*[local-name()='CommercialInvoiceCollection']
                                          /*[local-name()='CommercialInvoice']
                                          /*[local-name()='CommercialInvoiceLineCollection']
                                          /*[local-name()='CommercialInvoiceLine']
                                        [./*[local-name()='Procedure'] ='4000']) > 0">
          <DocType>
            <xsl:text>DA600</xsl:text>
          </DocType>
        </xsl:when>

        <xsl:otherwise>
          <DocType>
            <xsl:text>DA500</xsl:text>
          </DocType>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>

    <EDI_Sent>
      <xsl:value-of select="ScriptNS1:ConvertToDateTimeString($Declaration/*[local-name()='DateCollection']
                                                                          /*[local-name()='Date']
                                                                        [./*[local-name()='Type'] = 'EntrySubmitted']
                                                                          /*[local-name()='Value'])"/>
    </EDI_Sent>

    <CustomsPurposeCode>
      <xsl:value-of select="$Declaration/*[local-name()='EntryInstructionCollection']
                                        /*[local-name()='EntryInstruction']
                                        /*[local-name()='Style']"/>
    </CustomsPurposeCode>

    <CustomsPurposeName/>
    <Category/>
    <PCDescription/>

    <RequestedProcedure>
      <xsl:value-of select="$Declaration/*[local-name()='CommercialInfo']
                                        /*[local-name()='CommercialInvoiceCollection']
                                        /*[local-name()='CommercialInvoice']
                                        /*[local-name()='CommercialInvoiceLineCollection']
                                        /*[local-name()='CommercialInvoiceLine']
                                        /*[local-name()='Procedure']"/>
    </RequestedProcedure>

    <CustomsProcDesc>
      <xsl:value-of select="substring(normalize-space($Declaration/*[local-name()='CommercialInfo']
                                                        /*[local-name()='CommercialInvoiceCollection']
                                                        /*[local-name()='CommercialInvoice']
                                                        /*[local-name()='CommercialInvoiceLineCollection']
                                                        /*[local-name()='CommercialInvoiceLine']
                                                        /*[local-name()='Description']),1,255)"/>
    </CustomsProcDesc>

    <AlphabeticDistrictOfficeCode>
      <xsl:value-of select="$Declaration/*[local-name()='CustomsOffice']
                                        /*[local-name()='Code']"/>
    </AlphabeticDistrictOfficeCode>

    <DistrictOfficeName>
      <xsl:value-of select="$Declaration/*[local-name()='CustomsOffice']
                                        /*[local-name()='Description']"/>
    </DistrictOfficeName>

    <xsl:variable name="RecipientAddressDetails" select="$Declaration/*[local-name()='OrganizationAddressCollection']
                                                                     /*[local-name()='OrganizationAddress']
                                                                   [./*[local-name()='AddressType'] = 'ImporterDocumentaryAddress']"/>
    <ImporterCustomsCode>
      <xsl:value-of select="$Declaration/*[local-name()='OrganizationAddressCollection']
                                        /*[local-name()='OrganizationAddress']
                                        /*[local-name()='RegistrationNumberCollection']
                                        /*[local-name()='RegistrationNumber']
                                      [./*[local-name()='Type']/*[local-name()='Code'] = 'CCD']
                                        /*[local-name()='Value']"/>
    </ImporterCustomsCode>

    <ImporterCode>
      <xsl:value-of select="$RecipientAddressDetails/*[local-name()='OrganizationCode']"/>
    </ImporterCode>

    <ImporterName>
      <xsl:value-of select="$RecipientAddressDetails/*[local-name()='CompanyName']"/>
    </ImporterName>

    <xsl:variable name="SenderAddressDetails" select="$Declaration/*[local-name()='OrganizationAddressCollection']
                                                                  /*[local-name()='OrganizationAddress']
                                                                [./*[local-name()='AddressType'] = 'SupplierDocumentaryAddress']"/>
    <ExporterCustomsCode>
      <xsl:value-of select="$Declaration/*[local-name()='OrganizationAddressCollection']
                                        /*[local-name()='OrganizationAddress']
                                        /*[local-name()='RegistrationNumberCollection']
                                        /*[local-name()='RegistrationNumber']
                                      [./*[local-name()='Type']/*[local-name()='Code'] = 'EXP']
                                        /*[local-name()='Value']"/>
    </ExporterCustomsCode>

    <ExporterCode>
      <xsl:value-of select="$SenderAddressDetails/*[local-name()='OrganizationCode']"/>
    </ExporterCode>

    <ExporterName>
      <xsl:value-of select="$SenderAddressDetails/*[local-name()='CompanyName']"/>
    </ExporterName>

    <AgentCode>
      <xsl:value-of select="$Declaration/*[local-name()='OrganizationAddressCollection']
                                        /*[local-name()='OrganizationAddress']
                                        /*[local-name()='RegistrationNumberCollection']
                                        /*[local-name()='RegistrationNumber']
                                      [./*[local-name()='Type']/*[local-name()='Code'] = 'AGT']
                                        /*[local-name()='Value']"/>
    </AgentCode>

    <LocalAgentName>
      <xsl:value-of select="$Declaration/*[local-name()='OrganizationAddressCollection']
                                        /*[local-name()='OrganizationAddress']
                                      [./*[local-name()='AddressType'] = 'Declarant']
                                        /*[local-name()='CompanyName']"/>
    </LocalAgentName>

    <CountryFrom>
      <xsl:value-of select="$SenderAddressDetails/*[local-name()='Country']
                                                 /*[local-name()='Code']"/>
    </CountryFrom>

    <CountryName>
      <xsl:value-of select="$SenderAddressDetails/*[local-name()='Country']
                                                 /*[local-name()='Name']"/>
    </CountryName>

    <CountryTo>
      <xsl:value-of select="$RecipientAddressDetails/*[local-name()='Country']
                                                    /*[local-name()='Code']"/>
    </CountryTo>

    <DestCountry>
      <xsl:value-of select="$RecipientAddressDetails/*[local-name()='Country']
                                                    /*[local-name()='Name']"/>
    </DestCountry>

    <TransportCode>
      <xsl:call-template name="DetermineTransportCode">
        <xsl:with-param name="TransportTextCode" select="translate($Declaration/*[local-name()='TransportMode']
                                                                               /*[local-name()='Code'],
                                                                                 'abcdefghijklmnopqrstuvwxyz',
                                                                                 'ABCDEFGHIJKLMNOPQRSTUVWXYZ')"/>
      </xsl:call-template>
    </TransportCode>

    <xsl:variable name="MWB" select="$Declaration/*[local-name()='AdditionalBillCollection']
                                                 /*[local-name()='AdditionalBill']
                                               [./*[local-name()='BillType']/*[local-name()='Code']='MWB']"/>
    <TransportDocNo>
      <xsl:value-of select="$MWB/*[local-name()='BillNumber']"/>
    </TransportDocNo>

    <TransportDocDate>
      <xsl:value-of select="$MWB/*[local-name()='IssueDate']"/>
    </TransportDocDate>

    <ShipName>
      <xsl:value-of select="$Declaration/*[local-name()='VesselName']"/>
    </ShipName>

    <VoyageFlightNo>
      <xsl:value-of select="$Declaration/*[local-name()='VoyageFlightNo']"/>
    </VoyageFlightNo>

    <ActualDepartureDate>
      <xsl:value-of select="$Declaration/*[local-name()='DateCollection']
                                        /*[local-name()='Date']
                                      [./*[local-name()='Type']='Departure']
                                        /*[local-name()='Value']"/>
    </ActualDepartureDate>

    <HouseWaybillDate>
      <xsl:value-of select="$MWB/*[local-name()='IssueDate']"/>
    </HouseWaybillDate>

    <TotNoPackages>
      <xsl:value-of select="$Declaration/*[local-name()='EntryHeaderCollection']
                                        /*[local-name()='EntryHeader']
                                        /*[local-name()='AddInfoCollection']
                                        /*[local-name()='AddInfo']
                                      [./*[local-name()='Key']='Packages']
                                        /*[local-name()='Value']"/>
    </TotNoPackages>

    <GrossMass>
      <xsl:value-of select="sum($Declaration/*[local-name()='CommercialInfo']
                                            /*[local-name()='CommercialInvoiceCollection']
                                            /*[local-name()='CommercialInvoice']
                                            /*[local-name()='CommercialInvoiceLineCollection']
                                            /*[local-name()='CommercialInvoiceLine']                                        
                                            /*[local-name()='Weight'])"/>
    </GrossMass>

    <CIFCValue/>

    <CustomsValue>
      <xsl:text>0</xsl:text>
    </CustomsValue>

    <TotCustomsValue>
      <xsl:value-of select="sum($Declaration/*[local-name()='CommercialInfo']
                                            /*[local-name()='CommercialInvoiceCollection']
                                            /*[local-name()='CommercialInvoice']
                                            /*[local-name()='CommercialInvoiceLineCollection']
                                            /*[local-name()='CommercialInvoiceLine']
                                            /*[local-name()='CustomsValue'])"/>
    </TotCustomsValue>

    <F178UCRNumber>
      <xsl:value-of select="$Declaration/*[local-name()='AdditionalReferenceCollection']
                                        /*[local-name()='AdditionalReference']
                                      [./*[local-name()='Type']/*[local-name()='Code']='UCR']
                                        /*[local-name()='ReferenceNumber']"/>
    </F178UCRNumber>

    <TotDuty>
      <xsl:value-of select="sum($Declaration/*[local-name()='EntryHeaderCollection']
                                            /*[local-name()='EntryHeader']
                                            /*[local-name()='EntryLineCollection']
                                            /*[local-name()='EntryLine']
                                            /*[local-name()='EntryLineChargeCollection']
                                            /*[local-name()='EntryLineCharge']
                                          [./*[local-name()='Type']/*[local-name()='Code'] != 'VAT']
                                            /*[local-name()='Amount'])"/>
    </TotDuty>

    <xsl:variable name="VAT" select="sum($Declaration/*[local-name()='EntryHeaderCollection']
                                                     /*[local-name()='EntryHeader']
                                                     /*[local-name()='EntryLineCollection']
                                                     /*[local-name()='EntryLine']
                                                     /*[local-name()='EntryLineChargeCollection']
                                                     /*[local-name()='EntryLineCharge']
                                                   [./*[local-name()='Type']/*[local-name()='Code'] = 'VAT']
                                                     /*[local-name()='Amount'])"/>
    <TotVAT>
      <xsl:choose>
        <xsl:when test="$VAT > 0">
          <xsl:value-of select="$VAT"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>0</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </TotVAT>

    <PaymentCode>
      <xsl:value-of select="$Declaration/*[local-name()='EntryHeaderCollection']
                                        /*[local-name()='EntryHeader']
                                        /*[local-name()='AddInfoCollection']
                                        /*[local-name()='AddInfo']
                                      [./*[local-name()='Key']='PaymentMethod']
                                        /*[local-name()='Value']"/>
    </PaymentCode>

    <TotBOEAmount>
      <xsl:value-of select="$Declaration/*[local-name()='EntryHeaderCollection']
                                        /*[local-name()='EntryHeader']
                                        /*[local-name()='TotalAmountPaid']"/>
    </TotBOEAmount>

    <UserCode>
      <xsl:value-of select="$Declaration/*[local-name()='DataContext']
                                        /*[local-name()='EventUser']
                                        /*[local-name()='Code']"/>
    </UserCode>

    <UserFirstName>
      <xsl:value-of select="$Declaration/*[local-name()='DataContext']
                                        /*[local-name()='EventUser']
                                        /*[local-name()='Name']"/>
    </UserFirstName>

    <UserSurname/>

    <xsl:variable name="BOEDate" select="$Declaration/*[local-name()='EntryHeaderCollection']
                                                     /*[local-name()='EntryHeader']
                                                     /*[local-name()='EntryReleaseDate']"/>
    <DateTimeReceived>
      <xsl:value-of select="$BOEDate"/>
    </DateTimeReceived>

    <ResponseCode>
      <xsl:value-of select="$Declaration/*[local-name()='EntryStatus']
                                        /*[local-name()='Code']"/>
    </ResponseCode>

    <ResponseDesc>
      <xsl:value-of select="$Declaration/*[local-name()='EntryStatus']
                                        /*[local-name()='Description']"/>
    </ResponseDesc>

    <CustomsStatus>
      <xsl:value-of select="$Declaration/*[local-name()='EntryStatus']
                                        /*[local-name()='Code']"/>
    </CustomsStatus>

    <CustomsStatusDesc>
      <xsl:value-of select="$Declaration/*[local-name()='EntryStatus']
                                        /*[local-name()='Description']"/>
    </CustomsStatusDesc>

    <PrintIndicator>
      <xsl:text>N</xsl:text>
    </PrintIndicator>

    <CustomsVersionNo>
      <xsl:text>0</xsl:text>
    </CustomsVersionNo>

    <ExternalUniqueID>
      <xsl:value-of select="$Declaration/*[local-name()='DataContext']
                                        /*[local-name()='DataSourceCollection']
                                        /*[local-name()='DataSource']
                                      [./*[local-name()='Type'] = 'CustomsDeclaration']
                                        /*[local-name()='Key']"/>
    </ExternalUniqueID>

    <ExternalReference1>
      <xsl:value-of select="$Declaration/*[local-name()='AgentsReference']"/>
    </ExternalReference1>

    <xsl:variable name="Depot" select="$Declaration/*[local-name()='LocationAtClearance']"/>

    <DepotList>
      <Depot>
        <DepotCode>
          <xsl:value-of select="$Depot/*[local-name()='Code']"/>
        </DepotCode>
        <DepotName>
          <xsl:value-of select="$Depot/*[local-name()='Description']"/>
        </DepotName>
      </Depot>
    </DepotList>

  </xsl:template>

  <!--##############################-->
  <!--## Determine Transport Code ##-->
  <!--##############################-->

  <xsl:template name="DetermineTransportCode">
    <xsl:param name="TransportTextCode" />

    <xsl:choose>
      <xsl:when test="$TransportTextCode = 'SEA'">
        <xsl:text>1</xsl:text>
      </xsl:when>

      <xsl:when test="$TransportTextCode = 'RAIL'">
        <xsl:text>2</xsl:text>
      </xsl:when>

      <xsl:when test="$TransportTextCode = 'ROAD'">
        <xsl:text>3</xsl:text>
      </xsl:when>

      <xsl:when test="$TransportTextCode = 'AIR'">
        <xsl:text>4</xsl:text>
      </xsl:when>

      <xsl:when test="$TransportTextCode = 'MAI'">
        <xsl:text>5</xsl:text>
      </xsl:when>

      <xsl:otherwise>
        <xsl:text>6</xsl:text>
      </xsl:otherwise>

    </xsl:choose>
  </xsl:template>

  <!--#########################-->
  <!--## Apply Character Set ##-->
  <!--#########################-->

  <xsl:template name="ApplyCharacterSet">
    <xsl:param name="Value" />
    <xsl:param name="CharacterSet" select="'Extended'"/>
    <xsl:param name="ReplaceWithValidCharacters" select="true()"/>

    <xsl:variable name="Lower" select="'abcdefghijklmnopqrstuvwxyz'"/>
    <xsl:variable name="Upper" select="'ABCDEFGHIJKLMNOPQRSTUVWXYZ'"/>

    <xsl:variable name="BasicCharacterSet">
      <xsl:value-of select="$Upper"/>
      <xsl:text>0123456789!&quot;&amp;&apos;()*+,-./:;?= </xsl:text>
    </xsl:variable>

    <xsl:variable name="SelectLanguageCharacters" select="'ÀÁÂÄàáâäÈÉÊèéêëÌÍÎìíîïÒÓÔÖòóôöÙÚÛÜùúûüÇçÑñ¿¡'"/>
    <xsl:variable name="CorrespondingCharacters"  select="'AAAAAAAAEEEEEEEIIIIIIIOOOOOOOOUUUUUUUUCCNN?!'"/>

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
            <xsl:variable name="NewValue" select="translate($Value, concat($Lower, $SelectLanguageCharacters), concat($Upper, $CorrespondingCharacters))"/>
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

  <!--############################-->
  <!--## Map Value If Not Empty ##-->
  <!--############################-->

  <xsl:template name="MapValueIfNotEmpty">
    <xsl:param name="NodeName" />
    <xsl:param name="Value" />

    <xsl:if test="$Value != ''">
      <xsl:choose>

        <xsl:when test="contains($NodeName, '/')">
          <xsl:element name="{substring-before($NodeName, '/')}">
            <xsl:call-template name="MapValueIfNotEmpty">
              <xsl:with-param name="NodeName" select="substring-after($NodeName, '/')"/>
              <xsl:with-param name="Value" select="$Value"/>
            </xsl:call-template>
          </xsl:element>
        </xsl:when>

        <xsl:otherwise>
          <xsl:element name="{$NodeName}">
            <xsl:call-template name="ApplyCharacterSet">
              <xsl:with-param name="Value" select="$Value"/>
            </xsl:call-template>
          </xsl:element>
        </xsl:otherwise>

      </xsl:choose>
    </xsl:if>
  </xsl:template>

</xsl:stylesheet>

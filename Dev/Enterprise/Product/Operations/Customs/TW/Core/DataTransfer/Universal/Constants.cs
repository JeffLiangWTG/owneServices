namespace Enterprise.Customs.TW.DataTransfer
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "No need to be translated")]
	public static class Constants
	{
		public static class AddInfoKeys
		{
			public static class InvoiceLine
			{
				public const string NX101ShippingMarks = "NX101ShippingMarks";
			}

			public static class CusEntryHeader
			{
				public const string TotalEXPDisbursedAmountInInvoiceCurrency = "TotalEXPDisbursedAmountInInvoiceCurrency";
				public const string TotalIMPFOBAmountInInvoiceCurrency = "TotalIMPFOBAmountInInvoiceCurrency";
				public const string TotalInternationalFreightAmountInInvoiceCurrency = "TotalInternationalFreightAmountInInvoiceCurrency";
				public const string TotalInternationalInsuranceAmountInInvoiceCurrency = "TotalInternationalInsuranceAmountInInvoiceCurrency";
				public const string TotalAdditionsInInvoiceCurrency = "TotalAdditionsInInvoiceCurrency";
				public const string TotalDeductionsInInvoiceCurrency = "TotalDeductionsInInvoiceCurrency";
				public const string TotalCustomsValueInInvoiceCurrency = "TotalCustomsValueInInvoiceCurrency";
				public const string TotalCustomsValueInLocalCurrency = "TotalCustomsValueInLocalCurrency";
				public const string BusinessTaxBaseAmount = "BusinessTaxBaseAmount";
				public const string TotalCashTaxAmount = "TotalCashTaxAmount";
				public const string TotalNonCashTaxAmount = "TotalNonCashTaxAmount";
				public const string ConfirmedBusinessTaxBaseAmount = "ConfirmedBusinessTaxBaseAmount";
				public const string ConfirmedTotalCashTaxAmount = "ConfirmedTotalCashTaxAmount";
				public const string ConfirmedTotalNonCashTaxAmount = "ConfirmedTotalNonCashTaxAmount";
			}

			public static class ControllingMessage
			{
				public const string MessageNumber = "MessageNumber";
				public const string PermitNumber = "PermitNumber";
				public const string ControllingAgency = "ControllingAgency";
				public const string BusinessType = "BusinessType";
				public const string ProcessingUnit = "ProcessingUnit";
				public const string PaymentMethod = "PaymentMethod";
				public const string ProofOfPaper = "ProofOfPaper";
				public const string ElectronicReceipt = "ElectronicReceipt";
				public const string AppointmentDate = "AppointmentDate";
				public const string AppointmentPeriod = "AppointmentPeriod";
				public const string Request = "Request";
				public const string ControllingMessageType = "ControllingMessageType";
				public const string Purpose = "Purpose";
				public const string PreviousPermitNumber = "PreviousPermitNumber";
				public const string InspectionRegistrationNumber = "InspectionRegistrationNumber";
				public const string PreviousWineInspectionStatus = "PreviousWineInspectionStatus";
				public const string ApplyForSampleReturn = "ApplyForSampleReturn";
				public const string SamplingReductionReason = "SamplingReductionReason";
				public const string SampleReturnAddress = "SampleReturnAddress";
				public const string CertificateType = "CertificateType";
				public const string Link = "Link";
				public const string ControllingMessageLink = "ControllingMessageLink";
				public const string NX101_Notes = "NX101_Notes";

				public static class ProductLabelRages
				{
					public const string Status = "Status";
					public const string EndNumber = "EndNumber";
					public const string StartNumber = "StartNumber";
					public const string RunNumber = "RunNumber";
					public const string Year = "Year";
				}

				public static class EthanolPermitNumbers
				{
					public const string EthanolPermitNumber = "EthanolPermitNumbers";
				}

				public static class PreviousDocumentNumbers
				{
					public const string PreviousDocumentNumber = "PreviousDocumentNumber";
				}

				public static class CertificateOfOrigins
				{
					public const string CertificateOfOrigin = "CertificateOfOrigin";
				}
			}

			public static class GOVUniformInvoice
			{
				public const string GovernmentUniformInvoiceNumber = "GovernmentUniformInvoiceNumber";
				public const string GovernmentUniformInvoiceAmount = "GovernmentUniformInvoiceAmount";
			}

			public static class Declaration
			{
				public const string BrokerLicense = "BrokerLicense";
			}
		}

		public static class EntryInstruction
		{
			public static class Codes
			{
				public const string CM = "CM";
				public const string LB = "LB";
				public const string EPN = "EPN";
			}

			public static class Descriptions
			{
				public const string CM = "Controlling Message";
				public const string LB = "Label";
				public const string EPN = "Ethanol Permit Numbers";
			}
		}

		public static class AddressTypes
		{
			public const string PreviousBondedFactory = "PreviousBondedFactory";
			public const string LocalProcessor = "Local Processor";
			public const string NotifyParty = "NotifyParty";
		}

		public static class AddInfoGroupTypeCodes
		{
			public const string CarChassisNumberType = "CCN";
			public const string ControllingMessageLinksType = "CML";
		}

		public static class WayBillTypeCode
		{
			public const string ContainerNote = "CNN";
			public const string ContainerNoteDescription = "Container Note Number";
		}

		#region ShipmentDeclarationWithSupplierImporterDocumentaryDocAddressAndTheAddressOverrideIsTrue
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public const string ShipmentDeclarationWithSupplierImporterDocumentaryDocAddressAndTheAddressOverrideIsTrue = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
   <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>CustomsDeclaration</Type>
          <Key></Key>
        </DataTarget>
      </DataTargetCollection>

      <Company>
        <Code>DTW</Code>
        <Country>
          <Code>TW</Code>
          <Name>Taiwan</Name>
        </Country>
        <Name>EDI Demonstration System TW</Name>
      </Company>
      <DataProvider>WTLMXEDTW</DataProvider>
      <EnterpriseID>WTL</EnterpriseID>
      <EventBranch>
        <Code>TPE</Code>
        <Name>Taipei</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code></Code>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>MXE</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2020-12-23T13:51:14.563</TriggerDate>
      <TriggerDescription></TriggerDescription>
      <TriggerType>Manual</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ORP</Code>
          <Description>Organization Proxy</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <AdditionalTerms></AdditionalTerms>
    <AgentsReference></AgentsReference>
    <Branch>
      <Code>TPE</Code>
      <Name>Taipei</Name>
    </Branch>
    <CommercialInfo>
      <Name>All Invoices</Name>

      <CommercialChargeCollection>
        <CommercialCharge>
          <ChargeType>
            <Code>OFT</Code>
            <Description>International Freight</Description>
          </ChargeType>
          <AdjustedCharge>false</AdjustedCharge>
          <Amount>0.0000</Amount>
          <ApportionmentType>
            <Code>PAA</Code>
            <Description>Partial Apportionment: Apportion this amount to the invoices that don't have thi</Description>
          </ApportionmentType>
          <Currency>
            <Code></Code>
          </Currency>
          <DistributeBy>
            <Code>VAL</Code>
            <Description>Value</Description>
          </DistributeBy>
          <ExchangeRateType>
            <Code></Code>
          </ExchangeRateType>
          <IsApportionedCharge>false</IsApportionedCharge>
          <IsDutiable>true</IsDutiable>
          <IsGSTApplicable>true</IsGSTApplicable>
          <IsIncludedInITOT>false</IsIncludedInITOT>
          <IsNotIncludedInInvoice>false</IsNotIncludedInInvoice>
          <IsStatisticalValueApplicable>false</IsStatisticalValueApplicable>
          <PercentageOfLinePrice>0.00000</PercentageOfLinePrice>
          <PrepaidCollect>
            <Code>CCX</Code>
            <Description>Collect</Description>
          </PrepaidCollect>
        </CommercialCharge>
        <CommercialCharge>
          <ChargeType>
            <Code>ONS</Code>
            <Description>International Insurance</Description>
          </ChargeType>
          <AdjustedCharge>false</AdjustedCharge>
          <Amount>0.0000</Amount>
          <ApportionmentType>
            <Code>PAA</Code>
            <Description>Partial Apportionment: Apportion this amount to the invoices that don't have thi</Description>
          </ApportionmentType>
          <Currency>
            <Code></Code>
          </Currency>
          <DistributeBy>
            <Code>VAL</Code>
            <Description>Value</Description>
          </DistributeBy>
          <ExchangeRateType>
            <Code></Code>
          </ExchangeRateType>
          <IsApportionedCharge>false</IsApportionedCharge>
          <IsDutiable>true</IsDutiable>
          <IsGSTApplicable>true</IsGSTApplicable>
          <IsIncludedInITOT>false</IsIncludedInITOT>
          <IsNotIncludedInInvoice>false</IsNotIncludedInInvoice>
          <IsStatisticalValueApplicable>false</IsStatisticalValueApplicable>
          <PercentageOfLinePrice>0.00000</PercentageOfLinePrice>
          <PrepaidCollect>
            <Code>CCX</Code>
            <Description>Collect</Description>
          </PrepaidCollect>
        </CommercialCharge>
      </CommercialChargeCollection>

      <CommercialInvoiceCollection>
        <CommercialInvoice>
          <InvoiceNumber>TEST</InvoiceNumber>
          <AdditionalTerms>Australia</AdditionalTerms>
          <AgreedExchangeRate>28.550000000</AgreedExchangeRate>
          <BillNumber></BillNumber>
          <ExchangeRateType>
            <Code></Code>
          </ExchangeRateType>
          <IncoTerm>
            <Code>FOB</Code>
            <Description>Free On Board</Description>
          </IncoTerm>
          <InvoiceAmount>0.0000</InvoiceAmount>
          <InvoiceCurrency>
            <Code>USD</Code>
            <Description>United States Dollar</Description>
          </InvoiceCurrency>
          <InvoiceDate>2020-12-11T00:00:00</InvoiceDate>
          <LandedCostExchangeRate>28.550000000</LandedCostExchangeRate>
          <MessageStatus>
            <Code></Code>
          </MessageStatus>
          <NetWeight>0.000</NetWeight>
          <NetWeightUQ>
            <Code>KG</Code>
            <Description>Kilograms</Description>
          </NetWeightUQ>
          <NoOfPacks>0.000</NoOfPacks>
          <PaymentAmount>0.0000</PaymentAmount>
          <PaymentDate>2020-12-14T09:56:00</PaymentDate>
          <PaymentExchangeRate>0.000000000</PaymentExchangeRate>
          <PaymentNumber></PaymentNumber>
          <RelatedIndicator>
            <Code>N</Code>
            <Description>無相關</Description>
          </RelatedIndicator>
          <ValuationCode>
            <Code></Code>
          </ValuationCode>
          <ValuationDateOverride></ValuationDateOverride>
          <Volume>0.000</Volume>
          <VolumeUnit>
            <Code></Code>
          </VolumeUnit>
          <Weight>0.000</Weight>
          <WeightUnit>
            <Code>KG</Code>
            <Description>Kilograms</Description>
          </WeightUnit>

          <AddInfoCollection>
            <AddInfo>
              <Key>BillOfMaterials</Key>
              <Value>N</Value>
            </AddInfo>
            <AddInfo>
              <Key>CalculateTPF</Key>
              <Value>N</Value>
            </AddInfo>
            <AddInfo>
              <Key>DutyRefund</Key>
              <Value>N</Value>
            </AddInfo>
            <AddInfo>
              <Key>IsCoPackaged</Key>
              <Value>N</Value>
            </AddInfo>
            <AddInfo>
              <Key>IsPart</Key>
              <Value>N</Value>
            </AddInfo>
            <AddInfo>
              <Key>PrintDutyMemo</Key>
              <Value>N</Value>
            </AddInfo>
            <AddInfo>
              <Key>SplitMark</Key>
              <Value>N</Value>
            </AddInfo>
            <AddInfo>
              <Key>UseOneTenthCV</Key>
              <Value>N</Value>
            </AddInfo>
            <AddInfo>
              <Key>WaiverOfExemption</Key>
              <Value>N</Value>
            </AddInfo>
          </AddInfoCollection>

          <CommercialInvoiceLineCollection>
            <CommercialInvoiceLine>
              <LineNo>1</LineNo>
              <BondedWarehouseQuantity>0.00000</BondedWarehouseQuantity>
              <BondedWarehouseQuantityUnit>
                <Code></Code>
              </BondedWarehouseQuantityUnit>
              <BrandName></BrandName>
              <ClassificationCode></ClassificationCode>
              <ClassUsageComment></ClassUsageComment>
              <Commodity>
                <Code></Code>
              </Commodity>
              <ConcessionOrder></ConcessionOrder>
              <ContainerMode>
                <Code></Code>
              </ContainerMode>
              <CountryOfExport>
                <Code></Code>
              </CountryOfExport>
              <CountryOfOrigin>
                <Code>TW</Code>
                <Name>Taiwan</Name>
              </CountryOfOrigin>
              <CustomsQuantity>0.00200</CustomsQuantity>
              <CustomsQuantityUnit>
                <Code>TNE</Code>
                <Description>Tonne (Metric Ton) (10^3kg)</Description>
              </CustomsQuantityUnit>
              <CustomsSecondQuantity>0.00200</CustomsSecondQuantity>
              <CustomsSecondQuantityUnit>
                <Code>TNE</Code>
                <Description>Tonne (Metric Ton) (10^3kg)</Description>
              </CustomsSecondQuantityUnit>
              <CustomsThirdQuantity>0.00000</CustomsThirdQuantity>
              <CustomsThirdQuantityUnit>
                <Code></Code>
              </CustomsThirdQuantityUnit>
              <CustomsValue>0</CustomsValue>
              <DataImportMatchingKey></DataImportMatchingKey>
              <Description></Description>
              <EntryInstructionLink>1</EntryInstructionLink>
              <HarmonisedCode>27111200002</HarmonisedCode>
              <HazardousMaterial>
                <Code></Code>
                <CodeType>
                  <Code></Code>
                </CodeType>
              </HazardousMaterial>
              <InvoiceQuantity>2.00000</InvoiceQuantity>
              <InvoiceQuantityUnit>
                <Code></Code>
              </InvoiceQuantityUnit>
              <LinePrice>0.0000</LinePrice>
              <Link>1</Link>
              <LocalDescription></LocalDescription>
              <Model></Model>
              <NetWeight>2.000</NetWeight>
              <NetWeightUnit>
                <Code>KG</Code>
                <Description>Kilograms</Description>
              </NetWeightUnit>
              <OrderLineLink>0</OrderLineLink>
              <OrderNumber></OrderNumber>
              <ParentLineNo>0</ParentLineNo>
              <PartNo></PartNo>
              <PreviousEntryLineNumber>0</PreviousEntryLineNumber>
              <PreviousEntryNumber></PreviousEntryNumber>
              <PrimaryPreference>PR1</PrimaryPreference>
              <Procedure>50</Procedure>
              <RelatedIndicator>
                <Code></Code>
              </RelatedIndicator>
              <SecondaryPreference></SecondaryPreference>
              <StateOfOrigin>
                <Code></Code>
              </StateOfOrigin>
              <TaxType>
                <Code>DDF</Code>
                <Description>滯報費</Description>
              </TaxType>
              <UnitPrice>0</UnitPrice>
              <ValuationCode>
                <Code></Code>
              </ValuationCode>
              <ValuationMarkup>0.000</ValuationMarkup>
              <Volume>0.000</Volume>
              <VolumeUnit>
                <Code>M3</Code>
                <Description>Cubic Meters</Description>
              </VolumeUnit>
              <Weight>0.000</Weight>
              <WeightUnit>
                <Code>KG</Code>
                <Description>Kilograms</Description>
              </WeightUnit>

              <AddInfoCollection>
                <AddInfo>
                  <Key>BillOfMaterials</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>CalculateTPF</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>DutyRefund</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>IsCoPackaged</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>IsPart</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>PHValue</Key>
                  <Value>0</Value>
                </AddInfo>
                <AddInfo>
                  <Key>PrintDutyMemo</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>SplitMark</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>StorageShippingCondition</Key>
                  <Value>1</Value>
                </AddInfo>
                <AddInfo>
                  <Key>UseOneTenthCV</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>WaiverOfExemption</Key>
                  <Value>N</Value>
                </AddInfo>
                <AddInfo>
                  <Key>PreviousPermitNumber</Key>
                  <Value></Value>
                </AddInfo>
              </AddInfoCollection>

              <AdditionalLineTariffDetailCollection>
                <AdditionalLineTariffDetail>
                  <CustomsQuantity>0.0020</CustomsQuantity>
                  <CustomsQuantityUnit>
                    <Code>TNE</Code>
                  </CustomsQuantityUnit>
                  <Tariff>LiquefiedPetroleumGas</Tariff>
                  <Type>
                    <Code>CT</Code>
                    <Description>Commodity Tax</Description>
                  </Type>
                  <Value>0.0000</Value>
                </AdditionalLineTariffDetail>
              </AdditionalLineTariffDetailCollection>

              <CustomsSupportingInformationCollection>
                <CustomsSupportingInformation>
                  <Category>
                    <Code>COO</Code>
                    <Description>CertificateOfOriginType</Description>
                  </Category>
                  <LineNo>0</LineNo>
                  <ReferenceNumber></ReferenceNumber>
                </CustomsSupportingInformation>
                <CustomsSupportingInformation>
                  <Category>
                    <Code>MIF</Code>
                    <Description>FoodDrugType</Description>
                  </Category>
                  <Type>
                    <Code></Code>
                    <Description></Description>
                  </Type>

                  <ReferenceNumberCollection>
                    <ReferenceNumber>
                      <Type>
                        <Code>RN1</Code>
                      </Type>
                      <ReferenceNumber></ReferenceNumber>
                    </ReferenceNumber>
                    <ReferenceNumber>
                      <Type>
                        <Code>RN2</Code>
                      </Type>
                      <ReferenceNumber></ReferenceNumber>
                    </ReferenceNumber>
                  </ReferenceNumberCollection>
                </CustomsSupportingInformation>
                <CustomsSupportingInformation>
                  <Category>
                    <Code>TAC</Code>
                    <Description>TypeApprovalType</Description>
                  </Category>
                  <Description></Description>
                  <Type>
                    <Code></Code>
                    <Description></Description>
                  </Type>

                  <ReferenceNumberCollection>
                    <ReferenceNumber>
                      <Type>
                        <Code>RN1</Code>
                      </Type>
                      <ReferenceNumber></ReferenceNumber>
                    </ReferenceNumber>
                    <ReferenceNumber>
                      <Type>
                        <Code>RN2</Code>
                      </Type>
                      <ReferenceNumber></ReferenceNumber>
                    </ReferenceNumber>
                  </ReferenceNumberCollection>
                </CustomsSupportingInformation>
                <CustomsSupportingInformation>
                  <Category>
                    <Code>PBN</Code>
                    <Description>Previous Bonded Entry Number</Description>
                  </Category>
                  <LineNo>0</LineNo>
                  <ReferenceNumber></ReferenceNumber>
                </CustomsSupportingInformation>
              </CustomsSupportingInformationCollection>
            </CommercialInvoiceLine>
          </CommercialInvoiceLineCollection>
        </CommercialInvoice>
      </CommercialInvoiceCollection>
    </CommercialInfo>
    <ConsolidatedCargoStatus>
      <Code></Code>
    </ConsolidatedCargoStatus>
    <ContainerCount>0</ContainerCount>
    <CustomsBroker>
      <Code>MX</Code>
      <Name>Milo Xie2</Name>
    </CustomsBroker>
    <CustomsContainerMode>
      <Code>CNT</Code>
      <Description>Containerized</Description>
    </CustomsContainerMode>
    <CustomsOffice>
      <Code>BF</Code>
    </CustomsOffice>
    <CustomsProfileIdentifier>
      <Type>UserName</Type>
      <Value>TFD0378-A</Value>
    </CustomsProfileIdentifier>
    <CustomsValuationPort>
      <Code></Code>
    </CustomsValuationPort>
    <DeclarantType>
      <Code></Code>
    </DeclarantType>
    <DefermentAccountNumber></DefermentAccountNumber>
    <EFTMode>
      <Code></Code>
    </EFTMode>
    <EntryStatus>
      <Code></Code>
    </EntryStatus>
    <ExportGoodsType>
      <Code></Code>
    </ExportGoodsType>
    <Folio></Folio>
    <GoodsDescription></GoodsDescription>
    <GoodsOrigin>
      <Code></Code>
    </GoodsOrigin>
    <IsPersonalEffects>false</IsPersonalEffects>
    <JobCosting>
      <AccrualNotRecognized>0</AccrualNotRecognized>
      <AccrualRecognized>0</AccrualRecognized>
      <AgentRevenue>0</AgentRevenue>
      <Branch>
        <Code>TPE</Code>
        <Name>Taipei</Name>
      </Branch>
      <Currency>
        <Code>TWD</Code>
        <Description>New Taiwan Dollar</Description>
      </Currency>
      <Department>
        <Code>CEA</Code>
        <Name>Clearance Export Air</Name>
      </Department>
      <LocalClientRevenue>0</LocalClientRevenue>
      <OperationsStaff>
        <Code>~AD</Code>
        <Name>Automated Data Import</Name>
      </OperationsStaff>
      <OtherDebtorRevenue>0</OtherDebtorRevenue>
      <TotalAccrual>0</TotalAccrual>
      <TotalCost>0</TotalCost>
      <TotalJobProfit>0</TotalJobProfit>
      <TotalRevenue>0</TotalRevenue>
      <TotalWIP>0</TotalWIP>
      <WIPNotRecognized>0</WIPNotRecognized>
      <WIPRecognized>0</WIPRecognized>
    </JobCosting>
    <LloydsIMO></LloydsIMO>
    <LocationAtClearance>
      <Code></Code>
      <Description></Description>
    </LocationAtClearance>
    <MergeBy>
      <Code>NON</Code>
      <Description>No Merge</Description>
    </MergeBy>
    <MessageStatus>
      <Code></Code>
      <Description>未傳送</Description>
    </MessageStatus>
    <MessageSubType>
      <Code></Code>
    </MessageSubType>
    <MessageType>
      <Code>IMP</Code>
      <Description>Import</Description>
    </MessageType>
    <MessagingApplicationCode>
      <Code>BLT</Code>
      <Description>Submit entry using built-in messaging system</Description>
    </MessagingApplicationCode>
    <OperationalStatus>
      <Code></Code>
    </OperationalStatus>
    <OuterPacks>0</OuterPacks>
    <OuterPacksPackageType>
      <Code>CTN</Code>
      <Description>Carton</Description>
    </OuterPacksPackageType>
    <OwnerRef></OwnerRef>
    <PaymentMethod>
      <Code>1</Code>
    </PaymentMethod>
    <PortOfDestination>
      <Code>TWTPE</Code>
      <Name>Taipei</Name>
    </PortOfDestination>
    <PortOfDischarge>
      <Code>TWTPE</Code>
      <Name>Taipei</Name>
    </PortOfDischarge>
    <PortOfFirstArrival>
      <Code></Code>
    </PortOfFirstArrival>
    <PortOfLoading>
      <Code>AUSYD</Code>
      <Name>Sydney</Name>
    </PortOfLoading>
    <PortOfOrigin>
      <Code>AUSYD</Code>
      <Name>Sydney</Name>
    </PortOfOrigin>
    <ScreeningStatus>
      <Code>UNK</Code>
      <Description>Unknown</Description>
    </ScreeningStatus>
    <ServiceLevel>
      <Code>STD</Code>
      <Description>Standard</Description>
    </ServiceLevel>
    <ShipmentIncoTerm>
      <Code>FOB</Code>
      <Description>Free On Board</Description>
    </ShipmentIncoTerm>
    <SubLocationAtClearance>
      <Code></Code>
      <Description></Description>
    </SubLocationAtClearance>
    <TotalNoOfPacksDecimal>0.0000</TotalNoOfPacksDecimal>
    <TotalNoOfPieces>0</TotalNoOfPieces>
    <TotalNoOfPiecesLanded>0</TotalNoOfPiecesLanded>
    <TotalVolume>0.000</TotalVolume>
    <TotalVolumeUnit>
      <Code>M3</Code>
      <Description>Cubic Meters</Description>
    </TotalVolumeUnit>
    <TotalWeight>0.000</TotalWeight>
    <TotalWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </TotalWeightUnit>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <TransportNationality>
      <Code></Code>
    </TransportNationality>
    <VesselName></VesselName>
    <VoyageFlightNo></VoyageFlightNo>
    <WarehouseReleaseStatus>
      <Code></Code>
    </WarehouseReleaseStatus>

    <LocalProcessing>
      <ArrivalCartageRef></ArrivalCartageRef>
      <DeliveryCartageAdvised></DeliveryCartageAdvised>
      <DeliveryCartageCompleted></DeliveryCartageCompleted>
      <DeliveryLabourCharge>0.0000</DeliveryLabourCharge>
      <DeliveryLabourTime></DeliveryLabourTime>
      <DeliveryRequiredBy></DeliveryRequiredBy>
      <DeliveryRequiredFrom></DeliveryRequiredFrom>
      <DeliveryTruckWaitCharge>0.0000</DeliveryTruckWaitCharge>
      <DeliveryTruckWaitTime></DeliveryTruckWaitTime>
      <DemurrageOnDeliveryCharge>0.0000</DemurrageOnDeliveryCharge>
      <DemurrageOnDeliveryTime></DemurrageOnDeliveryTime>
      <DemurrageOnPickupCharge>0.0000</DemurrageOnPickupCharge>
      <DemurrageOnPickupTime></DemurrageOnPickupTime>
      <EstimatedDelivery></EstimatedDelivery>
      <EstimatedPickup></EstimatedPickup>
      <ExportStatement>
        <Code></Code>
      </ExportStatement>
      <FCLAvailable></FCLAvailable>
      <FCLDeliveryDetentionCharge>0.0000</FCLDeliveryDetentionCharge>
      <FCLDeliveryDetentionDays>0</FCLDeliveryDetentionDays>
      <FCLDeliveryDetentionFreeDays>0</FCLDeliveryDetentionFreeDays>
      <FCLDeliveryEquipmentNeeded>
        <Code></Code>
      </FCLDeliveryEquipmentNeeded>
      <FCLPickupDetentionCharge>0.0000</FCLPickupDetentionCharge>
      <FCLPickupDetentionDays>0</FCLPickupDetentionDays>
      <FCLPickupDetentionFreeDays>0</FCLPickupDetentionFreeDays>
      <FCLPickupEquipmentNeeded>
        <Code></Code>
      </FCLPickupEquipmentNeeded>
      <FCLStorageCommences></FCLStorageCommences>
      <HasProhibitedPackaging>false</HasProhibitedPackaging>
      <InsuranceRequired>false</InsuranceRequired>
      <IsContingencyRelease>false</IsContingencyRelease>
      <LCLAirStorageCharge>0.0000</LCLAirStorageCharge>
      <LCLAirStorageDaysOrHours>0</LCLAirStorageDaysOrHours>
      <LCLAvailable></LCLAvailable>
      <LCLDatesOverrideConsol>false</LCLDatesOverrideConsol>
      <LCLStorageCommences></LCLStorageCommences>
      <PickupCartageAdvised></PickupCartageAdvised>
      <PickupCartageCompleted></PickupCartageCompleted>
      <PickupLabourCharge>0.0000</PickupLabourCharge>
      <PickupLabourTime></PickupLabourTime>
      <PickupRequiredBy></PickupRequiredBy>
      <PickupRequiredFrom></PickupRequiredFrom>
      <PickupTruckWaitCharge>0.0000</PickupTruckWaitCharge>
      <PickupTruckWaitTime></PickupTruckWaitTime>
      <PrintOptionForPackagesOnAWB>
        <Code>DEF</Code>
        <Description>Default (Dims, fallback to Vol)</Description>
      </PrintOptionForPackagesOnAWB>
    </LocalProcessing>

    <AddInfoCollection>
      <AddInfo>
        <Key>BillOfMaterials</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>CalculateTPF</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>DutyRefund</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>IsCoPackaged</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>IsPart</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>PrintDutyMemo</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>SplitMark</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>UseOneTenthCV</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>WaiverOfExemption</Key>
        <Value>N</Value>
      </AddInfo>
    </AddInfoCollection>

    <AdditionalReferenceCollection>
    </AdditionalReferenceCollection>

    <ContainerCollection>
    </ContainerCollection>

    <DateCollection>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>true</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>LoadingDate</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>FirstArrivalInCountry</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>DischargeDate</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>true</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>EntrySubmitted</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>EntryAuthorisation</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>WarehouseRelease</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>EntryDate</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
    </DateCollection>

    <EntryInstructionCollection>
      <EntryInstruction>
        <DateOfValuation>2020-12-11T00:00:00</DateOfValuation>
        <Description></Description>
        <Link>1</Link>
        <MergeBy>
          <Code>NON</Code>
          <Description>No Merge</Description>
        </MergeBy>
        <Style>G1</Style>
        <SubStyle>
          <Code></Code>
        </SubStyle>

        <AddInfoCollection>
          <AddInfo>
            <Key>BillOfMaterials</Key>
            <Value>N</Value>
          </AddInfo>
          <AddInfo>
            <Key>BoxNumber</Key>
            <Value>456</Value>
          </AddInfo>
          <AddInfo>
            <Key>CalculateTPF</Key>
            <Value>N</Value>
          </AddInfo>
          <AddInfo>
            <Key>CustomsOffice</Key>
            <Value>CA</Value>
          </AddInfo>
          <AddInfo>
            <Key>DutyRefund</Key>
            <Value>N</Value>
          </AddInfo>
          <AddInfo>
            <Key>IsCoPackaged</Key>
            <Value>N</Value>
          </AddInfo>
          <AddInfo>
            <Key>IsPart</Key>
            <Value>N</Value>
          </AddInfo>
          <AddInfo>
            <Key>PrintDutyMemo</Key>
            <Value>N</Value>
          </AddInfo>
          <AddInfo>
            <Key>SplitMark</Key>
            <Value>N</Value>
          </AddInfo>
          <AddInfo>
            <Key>UseOneTenthCV</Key>
            <Value>N</Value>
          </AddInfo>
          <AddInfo>
            <Key>WaiverOfExemption</Key>
            <Value>N</Value>
          </AddInfo>
          <AddInfo>
            <Key>TW Traders Remarks</Key>
            <Value></Value>
          </AddInfo>
        </AddInfoCollection>

        <AddInfoGroupCollection>
        </AddInfoGroupCollection>
      </EntryInstruction>
    </EntryInstructionCollection>

    <MilestoneCollection>
      <Milestone>
        <Description>Customs Commenced</Description>
        <EventCode>CCC</EventCode>
        <Sequence>1</Sequence>
        <ActualDate></ActualDate>
        <ConditionReference></ConditionReference>
        <ConditionType></ConditionType>
        <EstimatedDate></EstimatedDate>
      </Milestone>
      <Milestone>
        <Description>Customs Cleared</Description>
        <EventCode>CLR</EventCode>
        <Sequence>2</Sequence>
        <ActualDate></ActualDate>
        <ConditionReference></ConditionReference>
        <ConditionType></ConditionType>
        <EstimatedDate></EstimatedDate>
      </Milestone>
    </MilestoneCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>Exporter</AddressType>
        <Address1>ADDRESS 121</Address1>
        <Address2>ADDRESS 2223</Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>7 FRIST ROAD</AddressShortCode>
        <City>SYN</City>
        <CompanyName>4B ELEVATOR COMPONENTS LIMITED</CompanyName>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email>XXT1@212.COM</Email>
        <Fax></Fax>
        <GovRegNum>42521663</GovRegNum>
        <GovRegNumType>
          <Code>VAT</Code>
          <Description>Government VAT Code</Description>
        </GovRegNumType>
        <OrganizationCode>4BELEVSYD</OrganizationCode>
        <Phone>+61225253235</Phone>
        <Port>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </Port>
        <Postcode>000000</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>NSW</State>

        <LocalAddressCollection>
          <LocalAddress>
            <Address1>測試地址1</Address1>
            <Address2>測試地址2</Address2>
            <City>TAIPEI</City>
            <CompanyName>MILO 測試</CompanyName>
            <Language>
              <Code>ZH-TW</Code>
              <Description>Chinese - Traditional</Description>
            </Language>
            <Postcode>105</Postcode>
            <State>QLD</State>
          </LocalAddress>
          <LocalAddress>
            <Address1>TEST111</Address1>
            <Address2>TEST122</Address2>
            <City></City>
            <CompanyName>ENLIGSH US</CompanyName>
            <Language>
              <Code>EN-US</Code>
              <Description>English (American)</Description>
            </Language>
            <Postcode></Postcode>
            <State></State>
          </LocalAddress>
        </LocalAddressCollection>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>TPC</Code>
              <Description>Tax payment on account business ide</Description>
            </Type>
            <CountryOfIssue>
              <Code>TW</Code>
              <Name>Taiwan</Name>
            </CountryOfIssue>
            <Value>555555</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>AEO</Code>
              <Description>Authorized Economic Operator</Description>
            </Type>
            <CountryOfIssue>
              <Code>TW</Code>
              <Name>Taiwan</Name>
            </CountryOfIssue>
            <Value>222222222</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>IntermediateConsignee</AddressType>
        <Address1>ADDRESS 121</Address1>
        <Address2>ADDRESS 2223</Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>7 FRIST ROAD</AddressShortCode>
        <City>SYN</City>
        <CompanyName>4B ELEVATOR COMPONENTS LIMITED</CompanyName>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email>XXT1@212.COM</Email>
        <Fax></Fax>
        <GovRegNum>42521663</GovRegNum>
        <GovRegNumType>
          <Code>VAT</Code>
          <Description>Government VAT Code</Description>
        </GovRegNumType>
        <OrganizationCode>4BELEVSYD</OrganizationCode>
        <Phone>+61225253235</Phone>
        <Port>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </Port>
        <Postcode>000000</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>NSW</State>

        <LocalAddressCollection>
          <LocalAddress>
            <Address1>測試地址1</Address1>
            <Address2>測試地址2</Address2>
            <City>TAIPEI</City>
            <CompanyName>MILO 測試</CompanyName>
            <Language>
              <Code>ZH-TW</Code>
              <Description>Chinese - Traditional</Description>
            </Language>
            <Postcode>105</Postcode>
            <State>QLD</State>
          </LocalAddress>
          <LocalAddress>
            <Address1>TEST111</Address1>
            <Address2>TEST122</Address2>
            <City></City>
            <CompanyName>ENLIGSH US</CompanyName>
            <Language>
              <Code>EN-US</Code>
              <Description>English (American)</Description>
            </Language>
            <Postcode></Postcode>
            <State></State>
          </LocalAddress>
        </LocalAddressCollection>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>TPC</Code>
              <Description>Tax payment on account business ide</Description>
            </Type>
            <CountryOfIssue>
              <Code>TW</Code>
              <Name>Taiwan</Name>
            </CountryOfIssue>
            <Value>555555</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>AEO</Code>
              <Description>Authorized Economic Operator</Description>
            </Type>
            <CountryOfIssue>
              <Code>TW</Code>
              <Name>Taiwan</Name>
            </CountryOfIssue>
            <Value>222222222</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>SupplierTranslatedDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>額外地址 LOCAL</AdditionalAddressInformation>
        <Address1>測試地址1 LOCAL1</Address1>
        <Address2>測試地址2</Address2>
        <AddressOverride>true</AddressOverride>
        <City>TAIPEI</City>
        <CompanyName>MXXX 測試3</CompanyName>
        <Contact></Contact>
        <Country>
          <Code>TW</Code>
          <Name>Taiwan</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Mobile></Mobile>
        <Phone></Phone>
        <Postcode>105</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>QLD</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>SupplierDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>FFF1222</AdditionalAddressInformation>
        <Address1>ADDRESS 121</Address1>
        <Address2>ADDRESS 2223</Address2>
        <AddressOverride>true</AddressOverride>
        <City>SYN</City>
        <CompanyName>4B ELEVATOR COMPONENTS LIMITED</CompanyName>
        <Contact>JOHN CHATFIELD</Contact>
        <Country>
          <Code>TW</Code>
          <Name>Taiwan</Name>
        </Country>
        <Email>XXT1@234.COM</Email>
        <Fax></Fax>
        <GovRegNum>12348881</GovRegNum>
        <GovRegNumType>
          <Code>VAT</Code>
          <Description>Government VAT Code</Description>
        </GovRegNumType>
        <Mobile></Mobile>
        <Phone>+61225253235</Phone>
        <Postcode>000000</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>NSW</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>AEO</Code>
              <Description>Authorized Economic Operator</Description>
            </Type>
            <CountryOfIssue>
              <Code>TW</Code>
              <Name>Taiwan</Name>
            </CountryOfIssue>
            <Value>123456</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>SupplierPickupDeliveryAddress</AddressType>
        <AdditionalAddressInformation>FFF2333</AdditionalAddressInformation>
        <Address1>ADDRESS 121</Address1>
        <Address2>ADDRESS 2223</Address2>
        <AddressOverride>true</AddressOverride>
        <City>SYN</City>
        <CompanyName>4B ELEVATOR COMPONENTS LIMITED</CompanyName>
        <Contact>JOHN CHATFIELD</Contact>
        <Country>
          <Code>TW</Code>
          <Name>Taiwan</Name>
        </Country>
        <Email>XXT1@212.COM</Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Mobile></Mobile>
        <Phone>+61225253235</Phone>
        <Postcode>000000</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>NSW</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ImporterDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>TAIPEI3</AdditionalAddressInformation>
        <Address1>TAIBEI MINSHENG3</Address1>
        <Address2>NO.92</Address2>
        <AddressOverride>true</AddressOverride>
        <City>LAX</City>
        <CompanyName>WISETECH GLOBAL4</CompanyName>
        <Contact>THE IMPORT MANAGER1</Contact>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email>XXX@SW2.COM</Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Mobile></Mobile>
        <Phone>156591685250</Phone>
        <Postcode>106</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>AL</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>VAT</Code>
              <Description>Government VAT Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>TW</Code>
              <Name>Taiwan</Name>
            </CountryOfIssue>
            <Value>11122225</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>AEO</Code>
              <Description>Authorized Economic Operator</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>1112585222</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>TPC</Code>
              <Description>Tax payment on account business ide</Description>
            </Type>
            <CountryOfIssue>
              <Code>TW</Code>
              <Name>Taiwan</Name>
            </CountryOfIssue>
            <Value>344455</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ImporterPickupDeliveryAddress</AddressType>
        <AdditionalAddressInformation>TAIPEI3</AdditionalAddressInformation>
        <Address1>TAIBEI MINSHENG3</Address1>
        <Address2>NO.92</Address2>
        <AddressOverride>true</AddressOverride>
        <City>LAX</City>
        <CompanyName>WISETECH GLOBAL4</CompanyName>
        <Contact>THE IMPORT MANAGER1</Contact>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email>XXX@SW2.COM</Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Mobile></Mobile>
        <Phone>156591685250</Phone>
        <Postcode>106</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>AL</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>VAT</Code>
              <Description>Government VAT Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>TW</Code>
              <Name>Taiwan</Name>
            </CountryOfIssue>
            <Value>96944490</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>AEO</Code>
              <Description>Authorized Economic Operator</Description>
            </Type>
            <CountryOfIssue>
              <Code>TW</Code>
              <Name>Taiwan</Name>
            </CountryOfIssue>
            <Value>999999999</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ImporterTranslatedDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>XX54</AdditionalAddressInformation>
        <Address1>台北市民生東路4段133號3F-2</Address1>
        <Address2>XX2</Address2>
        <AddressOverride>true</AddressOverride>
        <City>鎮4</City>
        <CompanyName>慧咨環球1</CompanyName>
        <Contact></Contact>
        <Country>
          <Code>TW</Code>
          <Name>Taiwan</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Mobile></Mobile>
        <Phone></Phone>
        <Postcode>503</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>CYQ</State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";
		#endregion
	}
}

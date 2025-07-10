using System;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing
{
	sealed class SeaShipmentBookingRequestDataObjectWriterTest : DataObjectWriterTest
	{
		readonly CodeDescriptionPairList emptyCodeDescriptionPairList = new CodeDescriptionPairList();

		public void TestPopulateDataObject()
		{
			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.EnableBookingConfirmation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				CombineAssertions("FreightDataRegistry.Instance.EnableChinaCustomsTaxNumberTable", () =>
				{
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableChinaCustomsTaxNumberTable, true, "2.5.0", true);
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableChinaCustomsTaxNumberTable, false, "2.5.0", true);
				});
				CombineAssertions("FreightDataRegistry.Instance.EnableBookingConfirmation", () =>
				{
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableBookingConfirmation, true, "3.0.0", true);
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableBookingConfirmation, false, "2.5.0", true);
				});

				CombineAssertions("FreightDataRegistry.Instance.EnableChinaCustomsTaxNumberTable", () =>
				{
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableChinaCustomsTaxNumberTable, true, "2.5.0", false);
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableChinaCustomsTaxNumberTable, false, "2.5.0", false);
				});
				CombineAssertions("FreightDataRegistry.Instance.EnableBookingConfirmation", () =>
				{
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableBookingConfirmation, true, "3.0.0", false);
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableBookingConfirmation, false, "2.5.0", false);
				});
			}
		}

		public void TestPopulateAttachedDocuments()
		{
			var hasFlashPoint = false;
			var shippingOrder = PrepareData(hasFlashPoint);

			var document = new DummyDocument();

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new SeaShipmentBookingRequestDataObjectWriter(manager, document);

			shippingOrder.IsRequiredSendAttachment = false;
			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var dataObject = writer.GetDataObject(shippingOrder))
			{
				AssertNull(dataObject.AttachedDocumentCollection);
			}

			shippingOrder.IsRequiredSendAttachment = true;

			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var dataObject = writer.GetDataObject(shippingOrder))
			{
				var attachments = dataObject.AttachedDocumentCollection;
				AssertEquals(1, attachments.Count);

				DataObjectWriterHelperTest.AssertPDFAttachedDocumentsFileAttributes(attachments[0], new DataObjectWriterHelper.FileAttributes()
				{
					Name = "Booking Request",
					Description = "Booking Request",
					Code = "BKG",
					IsPublished = false
				});
			}
		}

		void AssertPopulateDataObject(BooleanRegistryItem registryItem, bool enabled, string version, bool hasFlashPoint)
		{
			using (registryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enabled))
			{
				string flashPoint = null;
				var seaShipmentBookingRequest = PrepareData(hasFlashPoint);
				if (hasFlashPoint)
				{
					flashPoint = "<FlashPoint>10</FlashPoint>".PadLeft(39, ' ');
				}

				var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
				var writer = new SeaShipmentBookingRequestDataObjectWriter(manager);

				var dataObject = writer.GetDataObject(seaShipmentBookingRequest);
				var expectedXml = GetExpectedXml(version, flashPoint);

				AssertUXml(dataObject, expectedXml);
			}
		}

		#region Prepare Data

		SeaShipmentBookingRequest PrepareData(bool hasFlashPoint)
		{
			var seaShipmentBookingRequest = new SeaShipmentBookingRequest(nameof(ForwardingShipment), "S00001499");

			PopulateGeneralInfo(seaShipmentBookingRequest);
			PopulateTotals(seaShipmentBookingRequest);
			PopulatePorts(seaShipmentBookingRequest);
			PopulateOrganizatons(seaShipmentBookingRequest);
			PopulatePackLines(seaShipmentBookingRequest, hasFlashPoint);

			return seaShipmentBookingRequest;
		}

		void PopulateGeneralInfo(SeaShipmentBookingRequest seaShipmentBookingRequest)
		{
			seaShipmentBookingRequest.BookingReference = "BKG0001";
			seaShipmentBookingRequest.MasterBillNumber = "bill of lading number";

			seaShipmentBookingRequest.ContainerMode = new CodeDescription(emptyCodeDescriptionPairList)
			{
				Code = "FCL",
				Description = "Full Container Load"
			};
			seaShipmentBookingRequest.IsDoorPickup = true;
			seaShipmentBookingRequest.IsDoorDelivery = true;
			seaShipmentBookingRequest.EarliestDepartureDate = new ZDateTime(2019, 10, 20);
			seaShipmentBookingRequest.LatestDeliveryDate = new ZDateTime(2019, 10, 21);
			seaShipmentBookingRequest.EstCargoPickupDateTime = new ZDateTime(2019, 10, 25);
			seaShipmentBookingRequest.BookingReference = "123";
			seaShipmentBookingRequest.PaymentTerms = new OptionalCharge()
			{
				IsPrepaid = true,
				IsCollect = false
			};
			seaShipmentBookingRequest.AdditionalTerms = "123";
			seaShipmentBookingRequest.ShipperReference = "123";

			var carrierContractNumber = new ReferenceNumber
			{
				Value = "23456",
				CountryOfIssue = new DocumentVisualizer.DocDataObjects.Country(context.Factory, context.Countries)
				{
					Code = Core.Constants.CountryCodes.Brazil
				},
				Type = new CodeDescription(emptyCodeDescriptionPairList)
				{
					Code = DocDataConstants.AdditionalReferences.Codes.CarrierContractNumber,
					Description = DocDataConstants.AdditionalReferences.Descriptions.CarrierContractNumber
				}
			};

			var carrierBookingReference = new ReferenceNumber
			{
				Value = "12345",
				CountryOfIssue = new DocumentVisualizer.DocDataObjects.Country(context.Factory, context.Countries)
				{
					Code = Core.Constants.CountryCodes.Brazil
				},
				Type = new CodeDescription(emptyCodeDescriptionPairList)
				{
					Code = DocDataConstants.AdditionalReferences.Codes.CarrierBookingReference,
					Description = DocDataConstants.AdditionalReferences.Descriptions.CarrierBookingReference
				}
			};

			seaShipmentBookingRequest.Numbers = new[] { carrierContractNumber, carrierBookingReference };

			seaShipmentBookingRequest.GoodsHandlingInstructions = "123";

			seaShipmentBookingRequest.TransportMode = new CodeDescription(emptyCodeDescriptionPairList)
			{
				Code = "SEA",
				Description = "Sea"
			};
			seaShipmentBookingRequest.ReleaseType = new CodeDescription(emptyCodeDescriptionPairList)
			{
				Code = "SWB",
				Description = "SeaWayBill"
			};
			seaShipmentBookingRequest.VesselName = "VesselName";
			seaShipmentBookingRequest.LloydsIMO = "1001";
			seaShipmentBookingRequest.VoyageNumber = "001";
			seaShipmentBookingRequest.ETD = new ZDateTime(2021, 01, 01, 00, 00, 00);
			seaShipmentBookingRequest.ETA = new ZDateTime(2021, 01, 02, 00, 00, 00);
			seaShipmentBookingRequest.LegTransportMode = Core.Constants.TransportModes.Sea;
			seaShipmentBookingRequest.LegOrder = 1;
			seaShipmentBookingRequest.LegType = Core.Constants.TransportPlanningType.MainVessel;
		}

		void PopulateTotals(SeaShipmentBookingRequest seaShipmentBookingRequest)
		{
			seaShipmentBookingRequest.TotalPacks = 10;

			seaShipmentBookingRequest.TotalCargoWeight = new Measurement
			{
				Value = 36,
				Unit = new CodeDescription(emptyCodeDescriptionPairList)
				{
					Code = "KG",
					Description = "Kilogram"
				}
			};

			seaShipmentBookingRequest.TotalCargoVolume = new Measurement
			{
				Value = 46,
				Unit = new CodeDescription(emptyCodeDescriptionPairList)
				{
					Code = "M3",
					Description = "Cubic Meters"
				}
			};
		}

		void PopulatePackLines(SeaShipmentBookingRequest seaShipmentBookingRequest, bool hasFlashPoint)
		{
			var packLine1 = CreatePackLine(hasFlashPoint);
			var packLine2 = CreatePackLine(hasFlashPoint);

			seaShipmentBookingRequest.GoodsAndEquipmentDetails = new[] { packLine1, packLine2 };
		}

		SeaShipmentBookingRequestPackLine CreatePackLine(bool hasFlashPoint)
		{
			var packLine = new SeaShipmentBookingRequestPackLine();

			packLine.GoodsDescription = "PackLine";
			packLine.MarksAndNumbersOnPackages = "PackLine1";
			packLine.PacksQuantity = 5;

			packLine.PackType = new CodeDescription(emptyCodeDescriptionPairList)
			{
				Code = "PLT",
				Description = "Pallet"
			};

			packLine.CargoWeight = new Measurement
			{
				Value = 18,
				Unit = new CodeDescription(emptyCodeDescriptionPairList)
				{
					Code = "KG",
					Description = "Kilogram"
				}
			};

			packLine.CargoWeight = new Measurement
			{
				Value = 18,
				Unit = new CodeDescription(emptyCodeDescriptionPairList)
				{
					Code = "KG",
					Description = "Kilogram"
				}
			};

			packLine.CargoVolume = new Measurement
			{
				Value = 23,
				Unit = new CodeDescription(emptyCodeDescriptionPairList)
				{
					Code = "M3",
					Description = "Cubic Meters"
				}
			};

			var harmonizedCode = new HarmonizedCode
			{
				Code = "342546",
				Country = new DocumentVisualizer.DocDataObjects.Country(context.Factory, context.Countries)
				{
					Code = Core.Constants.CountryCodes.Brazil
				},
			};

			var harmonizedCode2 = new HarmonizedCode
			{
				Code = "210908",
				Country = new DocumentVisualizer.DocDataObjects.Country(context.Factory, context.Countries)
				{
					Code = Core.Constants.CountryCodes.China
				}
			};

			packLine.HarmonizedCodesCollection = new[] { harmonizedCode, harmonizedCode2 };

			var dangerousGood = new DangerousGood();

			if (hasFlashPoint)
			{
				dangerousGood.FlashPoint = new Measurement
				{
					Value = 10,
					Unit = new CodeDescription(context.TemperatureUnits)
					{
						Code = "C"
					}
				};
			}

			packLine.DangerousGoods = new[] { dangerousGood };

			return packLine;
		}

		void PopulateOrganizatons(SeaShipmentBookingRequest seaShipmentBookingRequest)
		{
			seaShipmentBookingRequest.CurrentUser = CreateAddress("CurrentUser");
			seaShipmentBookingRequest.Shipper = CreateAddress("Shipper");
			seaShipmentBookingRequest.Consignee = CreateAddress("Consignee");
			seaShipmentBookingRequest.Recipient = CreateAddress("Carrier");
			seaShipmentBookingRequest.PickupFrom = CreateAddress("PickupFrom");
			seaShipmentBookingRequest.DeliverTo = CreateAddress("DeliverTo");
		}

		void PopulatePorts(SeaShipmentBookingRequest seaShipmentBookingRequest)
		{
			seaShipmentBookingRequest.CarrierBookingOffice = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "AUSYD",
				Name = "Sydney"
			};

			seaShipmentBookingRequest.PortOfLoading = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "AUBNE",
				Name = "Brisbane"
			};

			seaShipmentBookingRequest.PortOfDischarge = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "SGSIN",
				Name = "Singapore"
			};

			seaShipmentBookingRequest.Origin = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "AUSYD",
				Name = "Sydney"
			};

			seaShipmentBookingRequest.Destination = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "CNSHA",
				Name = "Shanghai"
			};

			seaShipmentBookingRequest.PlaceOfReceipt = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "AUBNE",
				Name = "Brisbane"
			};
			seaShipmentBookingRequest.PlaceOfDelivery = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "AUPER",
				Name = "Perth"
			};

			seaShipmentBookingRequest.OperationalPort = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "AUSYD",
				Name = "Sydney"
			};
			seaShipmentBookingRequest.FreightPayableAt = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "BEANR",
				Name = "Antwerp"
			};
		}

		#endregion

		#region Expected XML

		string GetExpectedXml(string version, string flashpoint) => $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>S00001499</Key>
        <Type>ForwardingShipment</Type>
      </DataSource>

      <DocumentaryOverride>
        <DocumentName>BookingRequest</DocumentName>
      </DocumentaryOverride>
    </DataContext>

    <AdditionalTerms>123</AdditionalTerms>
    <CoLoadBookingConfirmationReference>123</CoLoadBookingConfirmationReference>
    <ContainerMode Description=""Full Container Load"">FCL</ContainerMode>
    <DeliveryMode Description=""Door To Door"">DTD</DeliveryMode>
    <LloydsIMO>1001</LloydsIMO>
    <PlaceOfDelivery Name=""Perth"">AUPER</PlaceOfDelivery>
    <PlaceOfReceipt Name=""Brisbane"">AUBNE</PlaceOfReceipt>
    <PortOfDestination Name=""Shanghai"">CNSHA</PortOfDestination>
    <PortOfDischarge Name=""Singapore"">SGSIN</PortOfDischarge>
    <PortOfLoading Name=""Brisbane"">AUBNE</PortOfLoading>
    <PortOfOrigin Name=""Sydney"">AUSYD</PortOfOrigin>
    <ReleaseType Description=""SeaWayBill"">SWB</ReleaseType>
    <ShipmentIncoTerm Description=""Cost And Freight"">CFR</ShipmentIncoTerm>
    <ShipmentType Description=""Co-Load Master"">CLD</ShipmentType>
    <TotalNoOfPacks>10</TotalNoOfPacks>
    <TotalVolume>46</TotalVolume>
    <TotalVolumeUnit Description=""Cubic Meters"">M3</TotalVolumeUnit>
    <TotalWeight>36</TotalWeight>
    <TotalWeightUnit Description=""Kilogram"">KG</TotalWeightUnit>
    <TransportMode Description=""Sea"">SEA</TransportMode>
    <VesselName>VesselName</VesselName>
    <VoyageFlightNo>001</VoyageFlightNo>

    <AddInfoCollection>
      <AddInfo>
        <Key>CarrierBookingOffice_Code</Key>
        <Value>AUSYD</Value>
      </AddInfo>
      <AddInfo>
        <Key>CarrierBookingOffice_Name</Key>
        <Value>Sydney</Value>
      </AddInfo>
      <AddInfo>
        <Key>FormVersion</Key>
        <Value>{version}</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>AUSYD</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Sydney</Value>
      </AddInfo>
      <AddInfo>
        <Key>FreightPayableAt_Code</Key>
        <Value>BEANR</Value>
      </AddInfo>
      <AddInfo>
        <Key>FreightPayableAt_Name</Key>
        <Value>Antwerp</Value>
      </AddInfo>
    </AddInfoCollection>

    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Shipper Reference"">SHP</Type>
        <ReferenceNumber>123</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Freight Forwarder Reference"">FFW</Type>
        <ReferenceNumber>S00001499</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Contract Number"">CON</Type>
        <ReferenceNumber>23456</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Booking Reference"">BKG</Type>
        <ReferenceNumber>12345</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>

    <DateCollection>
      <Date>
        <Type>EarliestDeparture</Type>
        <Value>2019-10-20T00:00:00</Value>
      </Date>
      <Date>
        <Type>LatestDelivery</Type>
        <Value>2019-10-21T00:00:00</Value>
      </Date>
      <Date>
        <Type>Pickup</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2019-10-25T00:00:00</Value>
      </Date>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2021-01-01T00:00:00</Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2021-01-02T00:00:00</Value>
      </Date>
    </DateCollection>

    <NoteCollection>
      <Note>
        <Description>Goods Handling Instructions</Description>
        <NoteText>123</NoteText>
      </Note>
    </NoteCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>CurrentUser</AddressType>
        <AdditionalAddressInformation>CurrentUser additional info</AdditionalAddressInformation>
        <Address1>CURRENTUSER ADDRESS LINE 1</Address1>
        <Address2>CURRENTUSER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CURRENTUSER CITY</City>
        <CompanyName>CURRENTUSER</CompanyName>
        <Contact>CurrentUser contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>CurrentUser email</Email>
        <Fax>CurrentUser fax</Fax>
        <GovRegNum>CurrentUser tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>CurrentUser phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CURRENTUSE</Postcode>
        <State>CURRENTUSER STATE</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>Forwarder</AddressType>
        <AdditionalAddressInformation>Shipper additional info</AdditionalAddressInformation>
        <Address1>SHIPPER ADDRESS LINE 1</Address1>
        <Address2>SHIPPER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>SHIPPER CITY</City>
        <CompanyName>SHIPPER</CompanyName>
        <Contact>Shipper contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Shipper email</Email>
        <Fax>Shipper fax</Fax>
        <GovRegNum>Shipper tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Shipper phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>SHIPPER PO</Postcode>
        <State>SHIPPER STATE</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>Shipper additional info</AdditionalAddressInformation>
        <Address1>SHIPPER ADDRESS LINE 1</Address1>
        <Address2>SHIPPER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>SHIPPER CITY</City>
        <CompanyName>SHIPPER</CompanyName>
        <Contact>Shipper contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Shipper email</Email>
        <Fax>Shipper fax</Fax>
        <GovRegNum>Shipper tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Shipper phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>SHIPPER PO</Postcode>
        <State>SHIPPER STATE</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>Consignee additional info</AdditionalAddressInformation>
        <Address1>CONSIGNEE ADDRESS LINE 1</Address1>
        <Address2>CONSIGNEE ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CONSIGNEE CITY</City>
        <CompanyName>CONSIGNEE</CompanyName>
        <Contact>Consignee contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Consignee email</Email>
        <Fax>Consignee fax</Fax>
        <GovRegNum>Consignee tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Consignee phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CONSIGNEE </Postcode>
        <State>CONSIGNEE STATE</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>CoLoadWith</AddressType>
        <AdditionalAddressInformation>Carrier additional info</AdditionalAddressInformation>
        <Address1>CARRIER ADDRESS LINE 1</Address1>
        <Address2>CARRIER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CARRIER CITY</City>
        <CompanyName>CARRIER</CompanyName>
        <Contact>Carrier contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Carrier email</Email>
        <Fax>Carrier fax</Fax>
        <GovRegNum>Carrier tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Carrier phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CARRIER PO</Postcode>
        <State>CARRIER STATE</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsignorPickupDeliveryAddress</AddressType>
        <AdditionalAddressInformation>PickupFrom additional info</AdditionalAddressInformation>
        <Address1>PICKUPFROM ADDRESS LINE 1</Address1>
        <Address2>PICKUPFROM ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>PICKUPFROM CITY</City>
        <CompanyName>PICKUPFROM</CompanyName>
        <Contact>PickupFrom contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>PickupFrom email</Email>
        <Fax>PickupFrom fax</Fax>
        <GovRegNum>PickupFrom tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>PickupFrom phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>PICKUPFROM</Postcode>
        <State>PICKUPFROM STATE</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneePickupDeliveryAddress</AddressType>
        <AdditionalAddressInformation>DeliverTo additional info</AdditionalAddressInformation>
        <Address1>DELIVERTO ADDRESS LINE 1</Address1>
        <Address2>DELIVERTO ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>DELIVERTO CITY</City>
        <CompanyName>DELIVERTO</CompanyName>
        <Contact>DeliverTo contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>DeliverTo email</Email>
        <Fax>DeliverTo fax</Fax>
        <GovRegNum>DeliverTo tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>DeliverTo phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>DELIVERTO </Postcode>
        <State>DELIVERTO STATE</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>

    <PackingLineCollection>
      <PackingLine>
        <DetailedDescription>PackLine</DetailedDescription>
        <GoodsDescription>PackLine</GoodsDescription>
        <HarmonisedCode>342546, 210908</HarmonisedCode>
        <MarksAndNos>PackLine1</MarksAndNos>
        <PackQty>5</PackQty>
        <PackType Description=""Pallet"">PLT</PackType>
        <Volume>23</Volume>
        <VolumeUnit Description=""Cubic Meters"">M3</VolumeUnit>
        <Weight>18</Weight>
        <WeightUnit Description=""Kilogram"">KG</WeightUnit>

        <ClassificationCollection>
          <Classification>
            <Code>342546</Code>
            <Country Name=""Brazil"">BR</Country>
            <Type Description=""Harmonized Code"">HSC</Type>
          </Classification>
          <Classification>
            <Code>210908</Code>
            <Country Name=""China"">CN</Country>
            <Type Description=""Harmonized Code"">HSC</Type>
          </Classification>
        </ClassificationCollection>

        <UNDGCollection>
          <UNDG>
{flashpoint}
            <IMOClass></IMOClass>
            <PackedInLimitedQuantity>false</PackedInLimitedQuantity>
            <PackingGroup></PackingGroup>
            <PackQty>0</PackQty>
            <ProperShippingName></ProperShippingName>
            <Standard></Standard>
            <SubLabel1></SubLabel1>
            <SubLabel2></SubLabel2>
            <TechicalName></TechicalName>
            <UNDGCode></UNDGCode>
          </UNDG>
        </UNDGCollection>
      </PackingLine>
      <PackingLine>
        <DetailedDescription>PackLine</DetailedDescription>
        <GoodsDescription>PackLine</GoodsDescription>
        <HarmonisedCode>342546, 210908</HarmonisedCode>
        <MarksAndNos>PackLine1</MarksAndNos>
        <PackQty>5</PackQty>
        <PackType Description=""Pallet"">PLT</PackType>
        <Volume>23</Volume>
        <VolumeUnit Description=""Cubic Meters"">M3</VolumeUnit>
        <Weight>18</Weight>
        <WeightUnit Description=""Kilogram"">KG</WeightUnit>

        <ClassificationCollection>
          <Classification>
            <Code>342546</Code>
            <Country Name=""Brazil"">BR</Country>
            <Type Description=""Harmonized Code"">HSC</Type>
          </Classification>
          <Classification>
            <Code>210908</Code>
            <Country Name=""China"">CN</Country>
            <Type Description=""Harmonized Code"">HSC</Type>
          </Classification>
        </ClassificationCollection>

        <UNDGCollection>
          <UNDG>
{flashpoint}
            <IMOClass></IMOClass>
            <PackedInLimitedQuantity>false</PackedInLimitedQuantity>
            <PackingGroup></PackingGroup>
            <PackQty>0</PackQty>
            <ProperShippingName></ProperShippingName>
            <Standard></Standard>
            <SubLabel1></SubLabel1>
            <SubLabel2></SubLabel2>
            <TechicalName></TechicalName>
            <UNDGCode></UNDGCode>
          </UNDG>
        </UNDGCollection>
      </PackingLine>
    </PackingLineCollection>

    <TransportLegCollection Content=""Complete"">
      <TransportLeg>
        <PortOfDischarge Name=""Singapore"">SGSIN</PortOfDischarge>
        <PortOfLoading Name=""Brisbane"">AUBNE</PortOfLoading>
        <LegOrder>1</LegOrder>
        <EstimatedArrival>2021-01-02T00:00:00</EstimatedArrival>
        <EstimatedDeparture>2021-01-01T00:00:00</EstimatedDeparture>
        <LegType>Main</LegType>
        <TransportMode>Sea</TransportMode>
        <VesselLloydsIMO>1001</VesselLloydsIMO>
        <VesselName>VesselName</VesselName>
        <VoyageFlightNo>001</VoyageFlightNo>
      </TransportLeg>
    </TransportLegCollection>
  </Shipment>
</UniversalShipment>";

		#endregion

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			context = new CommonContext(Factory);
		}

		CommonContext context;

		#endregion
	}
}

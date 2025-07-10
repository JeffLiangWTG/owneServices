using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.CN.Testing
{
	sealed class ShippingOrderDataObjectWriterTest : DataObjectWriterTest
	{
		CommonContext context;

		public void TestPopulateDataObject()
		{
			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (FreightDataRegistry.Instance.EnableBookingConfirmation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				CombineAssertions("FreightDataRegistry.Instance.EnableChinaCustomsTaxNumberTable", () =>
				{
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableChinaCustomsTaxNumberTable, true, "2.5.0", false, false);
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableChinaCustomsTaxNumberTable, false, "2.5.0", false, false);
				});
				CombineAssertions("FreightDataRegistry.Instance.EnableBookingConfirmation", () =>
				{
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableBookingConfirmation, true, "3.0.0", false, false);
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableBookingConfirmation, false, "2.5.0", false, false);
				});
			}
		}

		public void TestPopulateDataObject_IsGroupAndConsolidatePackingLines()
		{
			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.EnableBookingConfirmation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				CombineAssertions("FreightDataRegistry.Instance.EnableChinaCustomsTaxNumberTable", () =>
				{
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableChinaCustomsTaxNumberTable, true, "4.0.0", true, false);
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableChinaCustomsTaxNumberTable, false, "4.0.0", true, false);
				});
				CombineAssertions("FreightDataRegistry.Instance.EnableBookingConfirmation", () =>
				{
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableBookingConfirmation, true, "4.0.0", true, false);
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableBookingConfirmation, false, "4.0.0", true, false);
				});
			}
		}

		public void TestPopulateDataObject_DoNotGroup()
		{
			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.EnableBookingConfirmation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				CombineAssertions("FreightDataRegistry.Instance.EnableChinaCustomsTaxNumberTable", () =>
				{
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableChinaCustomsTaxNumberTable, true, "4.0.0", false, true);
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableChinaCustomsTaxNumberTable, false, "4.0.0", false, true);
				});
				CombineAssertions("FreightDataRegistry.Instance.EnableBookingConfirmation", () =>
				{
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableBookingConfirmation, true, "4.0.0", false, true);
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableBookingConfirmation, false, "4.0.0", false, true);
				});
			}
		}

		public void TestPopulateDataObject_PickupFromAndDeliverTo()
		{
			var shippingOrder = PrepareData();
			shippingOrder.PickupFrom = CreateAddress("PickupFrom");
			shippingOrder.IsDoorPickup = true;
			shippingOrder.DeliverTo = CreateAddress("DeliverTo");
			shippingOrder.IsDoorDelivery = false;

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new ShippingOrderDataObjectWriter(manager);

			var dataObject = writer.GetDataObject(shippingOrder);
			Assert("Should exist PickupFrom", dataObject.OrganizationAddressCollection.Any(org => (org.CompanyName ?? ZString.Empty) == "PICKUPFROM"));
			Assert("Shouldn't exist DeliverTo", !dataObject.OrganizationAddressCollection.Any(org => (org.CompanyName ?? ZString.Empty) == "DELIVERTO"));

			shippingOrder.IsDoorPickup = false;
			shippingOrder.IsDoorDelivery = true;
			dataObject = writer.GetDataObject(shippingOrder);
			Assert("Shouldn't exist PickupFrom", !dataObject.OrganizationAddressCollection.Any(org => (org.CompanyName ?? ZString.Empty) == "PICKUPFROM"));
			Assert("Should exist DeliverTo", dataObject.OrganizationAddressCollection.Any(org => (org.CompanyName ?? ZString.Empty) == "DELIVERTO"));
		}

		public void TestPopulateAttachedDocuments()
		{
			var shippingOrder = PrepareData();

			var document = new DummyDocument();

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new ShippingOrderDataObjectWriter(manager, document);

			shippingOrder.IsRequiredSendAttachment = false;
			using (var dataObject = writer.GetDataObject(shippingOrder))
			{
				AssertNull(dataObject.AttachedDocumentCollection);
			}

			shippingOrder.IsRequiredSendAttachment = true;
			using (var dataObject = writer.GetDataObject(shippingOrder))
			{
				var attachments = dataObject.AttachedDocumentCollection;
				AssertEquals(1, attachments.Count);

				DataObjectWriterHelperTest.AssertPDFAttachedDocumentsFileAttributes(attachments[0], new DataObjectWriterHelper.FileAttributes()
				{
					Name = "Shipping Order",
					Description = "Shipping Order",
					Code = "SHO",
					IsPublished = false
				});
			}
		}

		void AssertPopulateDataObject(BooleanRegistryItem registryItem, bool enabled, string version, bool enablePackageGrouping, bool isDoNotGroup)
		{
			using (registryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enabled))
			{
				var shippingOrder = PrepareData(enablePackageGrouping && !isDoNotGroup);

				var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
				var writer = new ShippingOrderDataObjectWriter(manager);

				var dataObject = writer.GetDataObject(shippingOrder);
				var expectedXml = GetExpectedXml(enablePackageGrouping, isDoNotGroup, version);

				AssertUXml(dataObject, expectedXml);
			}
		}

		string GetExpectedXml(bool enablePackageGrouping, bool isDoNotGroup, string version)
		{
			if (isDoNotGroup)
			{
				return GetExpectedXmlDoNoGroup();
			}
			else if (enablePackageGrouping)
			{
				return GetExpectedXmlIsGroupAndConsolidatePackingLines();
			}
			else
			{
				return GetExpectedXml(version);
			}
		}

		#region Prepare Data

		ShippingOrder PrepareData(bool isGroupAndConsolidatePackingLines = false)
		{
			var shippingOrder = new ShippingOrder
			(
				"ForwardingConsol",
				"C00001000",
				"ShippingOrder"
			);

			PopulateShippingOrder(shippingOrder);
			PopulateTransports(shippingOrder);
			PopulateOrganizations(shippingOrder);
			PopulateAdditionalReferenceNumbers(shippingOrder);

			if (isGroupAndConsolidatePackingLines)
			{
				shippingOrder.PackageGrouping = new CodeDescription(FreightCodePairLists.PackageGroupingList())
				{
					Code = Core.Constants.PackageGrouping.Codes.GroupByShipment
				};

				PopulateContainersAndGroupedAndConsolidatedPackingLines(shippingOrder);
			}
			else
			{
				shippingOrder.PackageGrouping = new CodeDescription(FreightCodePairLists.PackageGroupingList())
				{
					Code = Core.Constants.PackageGrouping.Codes.DoNotGroup
				};

				PopulateContainers(shippingOrder);
			}

			return shippingOrder;
		}

		#region PopulateShippingOrder

		void PopulateShippingOrder(ShippingOrder shippingOrder)
		{
			shippingOrder.IsFreightCollect = true;
			shippingOrder.CarrierBookingReference = "BKG0001";
			shippingOrder.NumberOfOriginals = 1;
			shippingOrder.NumberOfCopies = 2;
			shippingOrder.IsDoorPickup = true;
			shippingOrder.IsDoorDelivery = false;
			shippingOrder.ContainerMode = new DummyCodeDescription
			{
				Code = "FCL",
				Description = "Full Container Load"
			};
			shippingOrder.ShipmentType = new DummyCodeDescription
			{
				Code = "AGT",
				Description = "Agent"
			};
			shippingOrder.ReleaseType = new DummyCodeDescription
			{
				Code = "SWB",
				Description = "Sea Waybill"
			};

			var context = new CommonContext(Factory);
			shippingOrder.PortOfLoading = Unloco.Create(context, new RefUNLOCO.Loader(Factory).Load("AUBNE"));
			shippingOrder.PortOfDischarge = Unloco.Create(context, new RefUNLOCO.Loader(Factory).Load("SGSIN"));
			shippingOrder.PlaceOfIssue = Unloco.Create(context, new RefUNLOCO.Loader(Factory).Load("AUMEL"));
			shippingOrder.PlaceOfReceipt = Unloco.Create(context, new RefUNLOCO.Loader(Factory).Load("AUBNE"));
			shippingOrder.PlaceOfDelivery = Unloco.Create(context, new RefUNLOCO.Loader(Factory).Load("AUPER"));

			shippingOrder.CarrierBookingOffice = new DummyUnloco
			{
				Code = "CRAPO",
				Name = "Pital Con Desvio",
				IATACode = "APO"
			};
			shippingOrder.FreightPayableAt = new DummyUnloco
			{
				Code = "AMEVN",
				Name = "Yerevan",
				IATACode = "EVN"
			};
			shippingOrder.OperationalPort = new DummyUnloco
			{
				Code = "CNSHA",
				Name = "Shanghai",
				IATACode = "SHA"
			};
			shippingOrder.Vessel = new DummyVessel
			{
				Name = "Titanic",
				LloydsIMO = "12345"
			};
			shippingOrder.NVOCCReference = CreateRegNumber(OrgCusCode.CodeTypes.NVOCCReference, "NVOCC111");
			shippingOrder.VoyageFlightNumber = "1234567";
			shippingOrder.RequestedDateOfIssue = new ZDateTime(2018, 5, 20);

			shippingOrder.ForwardingInstructions = "forwarding instructions";
			shippingOrder.GoodsHandlingInstructions = "goods handling instructions";
			shippingOrder.SpecialInstructions = "special instructions";

			shippingOrder.IssueFreightedBillOfLading = ZBool.True;
			shippingOrder.OtherCharges = new OtherCharges { IsFree = ZBool.True };
			shippingOrder.OptionalChargeDestinationHaulage = new OptionalCharge { IsPrepaid = ZBool.True };
			shippingOrder.OptionalChargeDestinationPort = new OptionalCharge { IsPrepaid = ZBool.True };
			shippingOrder.OptionalChargeOriginHaulage = new OptionalCharge { IsCollect = ZBool.True };
			shippingOrder.OptionalChargeOriginPort = new OptionalCharge { IsCollect = ZBool.True };
		}

		RegistrationNumber CreateRegNumber(string type, string value)
		{
			return new RegistrationNumber()
			{
				CountryOfIssue = new DocumentVisualizer.DocDataObjects.Country(context.Factory, context.Countries)
				{
					Code = Core.Constants.CountryCodes.China
				},
				Type = new CodeDescription(new OrgCodeLists().CustomsCodes_List(Core.Constants.CountryCodes.China))
				{
					Code = type
				},
				Value = value
			};
		}

		#endregion

		#region PopulateTransports

		void PopulateTransports(ShippingOrder shippingOrder)
		{
			var transports = new DummyTransports();

			var main = new DummyTransport
			{
				LegOrder = 1,

				Mode = new DummyCodeDescription
				{
					Code = "SEA",
					Description = "Sea"
				},
				AdditionalTransportMode = new DummyCodeDescription
				{
					Code = "ROA",
				},
				Type = new DummyCodeDescription
				{
					Code = "MAI",
					Description = "Main"
				},
				VoyageFlightNumber = "AAAA",
				Vessel = new DummyVessel
				{
					Name = "Fudge Fixtures",
					LloydsIMO = "IMO111"
				},
				ETD = new ZDateTime(2018, 6, 10),
				ETA = new ZDateTime(2018, 7, 10),
				ATD = new ZDateTime(2018, 6, 10),
				ATA = new ZDateTime(2018, 7, 10),
				PortOfLoading = new DummyUnloco
				{
					Code = "AUSYD",
					Name = "Sydney",
					Country = new DummyCountry
					{
						Code = "AU",
						Name = "Australia"
					},
					IATACode = "SYD"
				},
				PortOfDischarge = new DummyUnloco
				{
					Code = "NZAKL",
					Name = "Auckland",
					Country = new DummyCountry
					{
						Code = "NZ",
						Name = "Kiwi land"
					},
					IATACode = "AKL"
				}
			};

			transports.Elements.Add(main);

			shippingOrder.Transports = transports;
		}

		#endregion

		#region PopulateOrganizations

		void PopulateOrganizations(ShippingOrder shippingOrder)
		{
			shippingOrder.Shipper = CreateAddress("Shipper");
			shippingOrder.Carrier = CreateAddress("Carrier");
			shippingOrder.Consignee = CreateAddress("Consignee");
			shippingOrder.CarrierHandlingAgent = CreateAddress("CarrierHandlingAgent");
			shippingOrder.CarrierBookingAgent = CreateAddress("CarrierBookingAgent");
			shippingOrder.NotifyParty = CreateAddress("NotifyParty");
			shippingOrder.NotifyParty2 = CreateAddress("NotifyParty2");
			shippingOrder.Forwarder = CreateAddress("Forwarder");
			shippingOrder.PickupFrom = CreateAddress("PickupFrom");
			shippingOrder.DeliverTo = CreateAddress("DeliverTo");
			shippingOrder.CurrentUser = CreateAddress("CurrentUser");
		}

		#endregion

		#region PopulateAdditionalReferenceNumbers

		void PopulateAdditionalReferenceNumbers(ShippingOrder shippingOrder)
		{
			shippingOrder.BillOfLadingNumber = "bill of lading number";
			shippingOrder.ShipperReference = "shipper reference number";
			shippingOrder.FreightForwarderReference = "freight forwarder reference number";
			shippingOrder.CarrierContractNumber = "carrier contract number";
			shippingOrder.CarrierContractNumberIsQuotationNumber = true;
			shippingOrder.ContractNamedAccount = "contract named account";
			shippingOrder.IsDischargeInCanadaUSOrUSTerritory = true;
			shippingOrder.USCanadaManifestSelfFilerID = "1111";
			shippingOrder.CarrierBookingPrefix = "1234";
		}

		#endregion

		#region PopulateContainers

		void PopulateContainers(ShippingOrder shippingOrder)
		{
			var factory = new CargoWise.EntityFramework.BusinessObjectFactory();
			var context = new CommonContext(factory);

			const string bookingNumber1 = "SL001";
			const string bookingNumber2 = "SL002";

			var container1 = CreateContainer(context, "AAA");
			container1.IsNonOperativeReefer = true;
			var container2 = CreateContainer(context, "BBB");
			container2.IsNonOperativeReefer = false;

			var packline1 = CreatePackingLine("AAA packline 1", bookingNumber1, packLineId: "Test001");
			var packline2 = CreatePackingLine("AAA packline 2", bookingNumber2, packLineId: "Test002");
			var packline3 = CreatePackingLine("BBB packline 1", bookingNumber1, packLineId: "Test003");

			container1.PackingLines = new[]
			{
				packline1,
				packline2
			};

			container2.PackingLines = new[]
			{
				packline3
			};

			shippingOrder.Containers = new[]
			{
				container1,
				container2
			};
		}

		void PopulateContainersAndGroupedAndConsolidatedPackingLines(ShippingOrder shippingOrder)
		{
			var factory = new CargoWise.EntityFramework.BusinessObjectFactory();
			var context = new CommonContext(factory);

			var container1Identifier = ZGuid.NewZGuid();
			var container1 = CreateContainer(context, "AAA", container1Identifier);
			container1.IsNonOperativeReefer = true;
			var container2Identifier = ZGuid.NewZGuid();
			var container2 = CreateContainer(context, "BBB", container2Identifier);
			container2.IsNonOperativeReefer = false;

			var groupedPackingLine1 = CreatePackingLine("AAA packline 1", "SL001", packLineId: "Test001");
			var consolidatedPacklineG11 = CreatePackingLine("AAA packline 2", "SL002", true, container1Identifier.ToString() + "1", packLineId: "Test002");
			var consolidatedPacklineG12 = CreatePackingLine("AAA packline 2", "SL003", true, container1Identifier.ToString() + "2", packLineId: "Test003");

			groupedPackingLine1.PackingLines = new[] { consolidatedPacklineG11, consolidatedPacklineG12 };

			var groupedPackingLine2 = CreatePackingLine("BBB packline 1", "SL001", packLineId: "Test004");
			var consolidatedPacklineG21 = CreatePackingLine("BBB packline 2", "SL002", true, container2Identifier.ToString() + "1", packLineId: "Test005");
			var consolidatedPacklineG22 = CreatePackingLine("BBB packline 2", "SL003", true, container2Identifier.ToString() + "2", packLineId: "Test006");

			groupedPackingLine2.PackingLines = new[] { consolidatedPacklineG21, consolidatedPacklineG22 };

			container1.PackingLines = new[] { consolidatedPacklineG11, consolidatedPacklineG21 };
			container2.PackingLines = new[] { consolidatedPacklineG12, consolidatedPacklineG22 };

			shippingOrder.Containers = new[] { container1, container2 };

			var asmShipment = CreateShipments("S0000009", "ASM", new[] { groupedPackingLine1 });
			var subASMShipment1 = CreateShipments("S00000011", "STD", Array.Empty<PackingLine>());
			var subASMShipment2 = CreateShipments("S00000012", "STD", Array.Empty<PackingLine>());
			var subShipment4 = CreateShipments("S00000013", "STD", new[] { groupedPackingLine2 });

			shippingOrder.Shipments = new[] { asmShipment, subShipment4 };
			asmShipment.Shipments = new[] { subASMShipment1, subASMShipment2 };
		}

		Shipment CreateShipments(string shipmentID, string shipmentType, PackingLine[] packingLines)
		{
			var shipment = new Shipment(ZGuid.NewZGuid());
			shipment.ShipmentID = shipmentID;
			shipment.HouseBillNumber = $"H{shipmentID}";
			shipment.ContainerPackingMode = new CodeDescription(FreightCodePairLists.JS_PackingModeList("SEA")) { Code = "LCL" };
			shipment.ShipperReference = "K00123";
			shipment.PickRequestedByDate = new ZDateTime(2019, 9, 15);
			shipment.DeliveryRequiredByDate = new ZDateTime(2019, 9, 20);
			shipment.ShipmentType = new CodeDescription(new CodeDescriptionPairList()) { Code = shipmentType };
			shipment.Consignor = CreateAddress("Consignor");
			shipment.Consignee = CreateAddress("Consignee");
			shipment.PickupFrom = CreateAddress("PickupFrom");
			shipment.PickupCFS = CreateAddress("PickupCFS");
			shipment.DeliveryTo = CreateAddress("DeliveryTo");
			shipment.DeliveryCFS = CreateAddress("DeliveryCFS");
			shipment.PackingLines = packingLines;

			return shipment;
		}

		Container CreateContainer(IContext context, string containerNumber, object identifier = null)
		{
			var container = new Container(identifier ?? DefaultDataObjectWriterStrategy.TestInstance);

			container.Number = containerNumber;
			container.Type = new ContainerType(context.ContainerTypes)
			{
				Code = "20FR"
			};
			container.AirVentFlow = new Measurement
			{
				Value = 12,
				Unit = new CodeDescription(context.AirVentFlow)
				{
					Code = "2L"
				}
			};

			container.ContainerCount = 1;
			container.PackCount = 3;
			container.IsEmpty = false;
			container.IsPartOf = false;
			container.IsShipperOwned = true;
			container.Seal = "SEAL1";
			container.SealPartyType = new DummyCodeDescription
			{
				Code = "CAR",
				Description = "Carrier"
			};
			container.SecondSeal = "SEAL2";
			container.SecondSealPartyType = new DummyCodeDescription
			{
				Code = "CUS",
				Description = "Customs"
			};
			container.ThirdSeal = "SEAL3";
			container.ThirdSealPartyType = new DummyCodeDescription
			{
				Code = "CTP",
				Description = "Terminal"
			};
			container.GoodsWeight = new Measurement()
			{
				Value = 200,
				Unit = new DummyCodeDescription
				{
					Code = "KG"
				}
			};
			container.TareWeight = new Measurement()
			{
				Value = 20,
				Unit = new DummyCodeDescription
				{
					Code = "KG"
				}
			};
			container.Dunnage = new Measurement()
			{
				Value = 30,
				Unit = new DummyCodeDescription
				{
					Code = "KG"
				}
			};
			container.GrossWeight = new Measurement()
			{
				Value = 272,
				Unit = new DummyCodeDescription
				{
					Code = "KG"
				}
			};
			container.Volume = new Measurement()
			{
				Value = 320,
				Unit = new DummyCodeDescription
				{
					Code = "M3"
				}
			};
			container.ContainerQuality = new DummyCodeDescription
			{
				Code = "FOD",
				Description = "Food"
			};

			return container;
		}

		PackingLine CreatePackingLine(string goodsDescription, string exportRefNumber, bool hasDangerousGoods = false, object identifier = null, string packLineId = "")
		{
			var packingLine = new PackingLine(identifier ?? ZGuid.NewZGuid(), Factory);

			packingLine.Quantity = 3;
			packingLine.PackageType = new DummyCodeDescription
			{
				Code = "PLT",
				Description = "Pallet"
			};
			packingLine.Weight = new Measurement()
			{
				Value = 88,
				Unit = new DummyCodeDescription
				{
					Code = "KG"
				}
			};
			packingLine.Volume = new Measurement()
			{
				Value = 55,
				Unit = new DummyCodeDescription
				{
					Code = "M3",
				}
			};
			packingLine.GoodsDescription = goodsDescription;
			packingLine.MarksAndNumbers = "marks & nums";
			packingLine.HarmonizedCode = new HarmonizedCode() { Code = "HC12345" };
			packingLine.ReferenceNumber = "reference number";
			packingLine.ImportReferenceNumber = "import reference number";
			packingLine.ExportReferenceNumber = exportRefNumber;
			packingLine.PackingLineID = packLineId;

			var hc = new HarmonizedCode();
			hc.Country = new DocumentVisualizer.DocDataObjects.Country(Factory, new RefCountryCollection(Factory)) { Code = "CN" };
			hc.Code = "1234.56";

			packingLine.HarmonizedCodes = new List<HarmonizedCode>() { hc };

			if (hasDangerousGoods)
			{
				var dangerousGoods = new List<DangerousGood>();

				var dangerousGood = new DangerousGood()
				{
					Code = "0001C",
					Unno = "0001",
					Quantity = 11,
					Variant = "C",
					ProperShippingName = "Danger",
					TechnicalName = "Technicals",
					IMOClass = "A",
					Standard = "IAT",
					PackedInLimitedQuantity = true,
				};

				dangerousGoods.Add(dangerousGood);
				packingLine.DangerousGoods = dangerousGoods;
			}

			return packingLine;
		}

		#endregion

		#endregion

		#region Expected Xml

		string GetExpectedXml(string version) => $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>C00001000</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>

    </DataContext>

    <BookingConfirmationReference>BKG0001</BookingConfirmationReference>
    <CarrierBookingOffice Name=""Pital Con Desvio"">CRAPO</CarrierBookingOffice>
    <ContainerMode Description=""Full Container Load"">FCL</ContainerMode>
    <DeliveryMode Description=""Door To Peer"">DTP</DeliveryMode>
    <LloydsIMO>12345</LloydsIMO>
    <NoCopyBills>2</NoCopyBills>
    <NoOriginalBills>1</NoOriginalBills>
    <PlaceOfDelivery Name=""Perth"">AUPER</PlaceOfDelivery>
    <PlaceOfIssue Name=""Melbourne"">AUMEL</PlaceOfIssue>
    <PlaceOfReceipt Name=""Brisbane"">AUBNE</PlaceOfReceipt>
    <PortOfDischarge Name=""Singapore"">SGSIN</PortOfDischarge>
    <PortOfLoading Name=""Brisbane"">AUBNE</PortOfLoading>
    <ReleaseType Description=""Sea Waybill"">SWB</ReleaseType>
    <ShipmentType Description=""Agent"">AGT</ShipmentType>
    <VesselName>Titanic</VesselName>
    <VoyageFlightNo>1234567</VoyageFlightNo>
    <WayBillNumber>bill of lading number</WayBillNumber>

    <AddInfoCollection>
      <AddInfo>
        <Key>CarrierBookingOffice_Code</Key>
        <Value>CRAPO</Value>
      </AddInfo>
      <AddInfo>
        <Key>CarrierBookingOffice_Name</Key>
        <Value>Pital Con Desvio</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>CNSHA</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Shanghai</Value>
      </AddInfo>
      <AddInfo>
        <Key>FreightPayableAt_Code</Key>
        <Value>AMEVN</Value>
      </AddInfo>
      <AddInfo>
        <Key>FreightPayableAt_Name</Key>
        <Value>Yerevan</Value>
      </AddInfo>
      <AddInfo>
        <Key>NVOCC_Registration_Reference</Key>
        <Value>NVOCC111</Value>
      </AddInfo>
      <AddInfo>
        <Key>FormVersion</Key>
        <Value>{version}</Value>
      </AddInfo>
    </AddInfoCollection>

    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Bill Of Lading Number"">BOL</Type>
        <ReferenceNumber>bill of lading number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Shipper Reference"">SHP</Type>
        <ReferenceNumber>shipper reference number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Freight Forwarder Reference"">FFW</Type>
        <ReferenceNumber>freight forwarder reference number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Contract Number"">CON</Type>
        <ReferenceNumber>carrier contract number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Quote Number"">CQN</Type>
        <ReferenceNumber>carrier contract number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Contract Named Account"">NAC</Type>
        <ReferenceNumber>contract named account</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Shipping Order/Shi Lian Dan"">SLD</Type>
        <ReferenceNumber>1234</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>

    <BillOfLadingClauseCollection>
      <BillOfLadingClause>
        <Type Description=""Freight Collect"">FCL</Type>
      </BillOfLadingClause>
    </BillOfLadingClauseCollection>

    <ContainerCollection>
      <Container>
        <AirVentFlow>12</AirVentFlow>
        <AirVentFlowRateUnit Description=""Cubic feet per minute"">2L</AirVentFlowRateUnit>
        <ArrivalDeliveryRequiredBy></ArrivalDeliveryRequiredBy>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>AAA</ContainerNumber>
        <ContainerQuality Description=""Food"">FOD</ContainerQuality>
        <ContainerType>
          <Code>20FR</Code>
          <Category Description=""Flat Rack"">FLT</Category>
          <Description>Twenty foot flatrack</Description>
          <ISOCode>22P1</ISOCode>
        </ContainerType>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <DunnageWeight>30</DunnageWeight>
        <EmptyRequired></EmptyRequired>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <GoodsWeight>200</GoodsWeight>
        <GrossWeight>272</GrossWeight>
        <GrossWeightVerificationDateTime></GrossWeightVerificationDateTime>
        <HumidityPercent>0</HumidityPercent>
        <ImportDepotCustomsReference></ImportDepotCustomsReference>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>true</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <Link>1</Link>
        <NonOperatingReefer>true</NonOperatingReefer>
        <Seal>SEAL1</Seal>
        <SealPartyType Description=""Carrier"">CAR</SealPartyType>
        <SecondSeal>SEAL2</SecondSeal>
        <SecondSealPartyType Description=""Customs"">CUS</SecondSealPartyType>
        <TareWeight>20</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal>SEAL3</ThirdSeal>
        <ThirdSealPartyType Description=""Terminal"">CTP</ThirdSealPartyType>
        <WeightUnit>KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>Genset</Key>
            <Value>false</Value>
          </AddInfo>
        </AddInfoCollection>
        <OrganizationAddressCollection>
        </OrganizationAddressCollection>
      </Container>
      <Container>
        <AirVentFlow>12</AirVentFlow>
        <AirVentFlowRateUnit Description=""Cubic feet per minute"">2L</AirVentFlowRateUnit>
        <ArrivalDeliveryRequiredBy></ArrivalDeliveryRequiredBy>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>BBB</ContainerNumber>
        <ContainerQuality Description=""Food"">FOD</ContainerQuality>
        <ContainerType>
          <Code>20FR</Code>
          <Category Description=""Flat Rack"">FLT</Category>
          <Description>Twenty foot flatrack</Description>
          <ISOCode>22P1</ISOCode>
        </ContainerType>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <DunnageWeight>30</DunnageWeight>
        <EmptyRequired></EmptyRequired>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <GoodsWeight>200</GoodsWeight>
        <GrossWeight>272</GrossWeight>
        <GrossWeightVerificationDateTime></GrossWeightVerificationDateTime>
        <HumidityPercent>0</HumidityPercent>
        <ImportDepotCustomsReference></ImportDepotCustomsReference>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>true</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <Link>2</Link>
        <NonOperatingReefer>false</NonOperatingReefer>
        <Seal>SEAL1</Seal>
        <SealPartyType Description=""Carrier"">CAR</SealPartyType>
        <SecondSeal>SEAL2</SecondSeal>
        <SecondSealPartyType Description=""Customs"">CUS</SecondSealPartyType>
        <TareWeight>20</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal>SEAL3</ThirdSeal>
        <ThirdSealPartyType Description=""Terminal"">CTP</ThirdSealPartyType>
        <WeightUnit>KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>Genset</Key>
            <Value>false</Value>
          </AddInfo>
        </AddInfoCollection>
        <OrganizationAddressCollection>
        </OrganizationAddressCollection>
      </Container>
    </ContainerCollection>

    <DateCollection>
      <Date>
        <Type>BillRequiredBy</Type>
        <Value>2018-05-20T00:00:00</Value>
      </Date>
    </DateCollection>

    <NoteCollection>
      <Note>
        <Description>Forwarding Instruction Notes</Description>
        <NoteText>forwarding instructions</NoteText>
      </Note>
      <Note>
        <Description>Goods Handling Instructions</Description>
        <NoteText>goods handling instructions</NoteText>
      </Note>
      <Note>
        <Description>Special Instructions</Description>
        <NoteText>special instructions</NoteText>
      </Note>
      <Note>
        <Description>USCanadaManifestSelfFilerID</Description>
        <NoteText>1111</NoteText>
      </Note>
      <Note>
        <Description>ChargesFreighted</Description>
        <NoteText>Y</NoteText>
      </Note>
    </NoteCollection>

    <OrganizationAddressCollection>
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
        <AddressType>ShippingLineAddress</AddressType>
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
        <AddressType>CarrierHandlingAgent</AddressType>
        <AdditionalAddressInformation>CarrierHandlingAgent additional info</AdditionalAddressInformation>
        <Address1>CARRIERHANDLINGAGENT ADDRESS LINE 1</Address1>
        <Address2>CARRIERHANDLINGAGENT ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CARRIERHANDLINGAGENT CITY</City>
        <CompanyName>CARRIERHANDLINGAGENT</CompanyName>
        <Contact>CarrierHandlingAgent contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>CarrierHandlingAgent email</Email>
        <Fax>CarrierHandlingAgent</Fax>
        <GovRegNum>CarrierHandlingAgent tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>CarrierHandlingAgent</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CARRIERHAN</Postcode>
        <State>CARRIERHANDLINGAGENT STAT</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>CarrierBookingAgent</AddressType>
        <AdditionalAddressInformation>CarrierBookingAgent additional info</AdditionalAddressInformation>
        <Address1>CARRIERBOOKINGAGENT ADDRESS LINE 1</Address1>
        <Address2>CARRIERBOOKINGAGENT ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CARRIERBOOKINGAGENT CITY</City>
        <CompanyName>CARRIERBOOKINGAGENT</CompanyName>
        <Contact>CarrierBookingAgent contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>CarrierBookingAgent email</Email>
        <Fax>CarrierBookingAgent </Fax>
        <GovRegNum>CarrierBookingAgent tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>CarrierBookingAgent </Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CARRIERBOO</Postcode>
        <State>CARRIERBOOKINGAGENT STATE</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>NotifyParty</AddressType>
        <AdditionalAddressInformation>NotifyParty additional info</AdditionalAddressInformation>
        <Address1>NOTIFYPARTY ADDRESS LINE 1</Address1>
        <Address2>NOTIFYPARTY ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>NOTIFYPARTY CITY</City>
        <CompanyName>NOTIFYPARTY</CompanyName>
        <Contact>NotifyParty contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>NotifyParty email</Email>
        <Fax>NotifyParty fax</Fax>
        <GovRegNum>NotifyParty tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>NotifyParty phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>NOTIFYPART</Postcode>
        <State>NOTIFYPARTY STATE</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>NotifyParty2</AddressType>
        <AdditionalAddressInformation>NotifyParty2 additional info</AdditionalAddressInformation>
        <Address1>NOTIFYPARTY2 ADDRESS LINE 1</Address1>
        <Address2>NOTIFYPARTY2 ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>NOTIFYPARTY2 CITY</City>
        <CompanyName>NOTIFYPARTY2</CompanyName>
        <Contact>NotifyParty2 contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>NotifyParty2 email</Email>
        <Fax>NotifyParty2 fax</Fax>
        <GovRegNum>NotifyParty2 tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>NotifyParty2 phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>NOTIFYPART</Postcode>
        <State>NOTIFYPARTY2 STATE</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>BookingPartyDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>Forwarder additional info</AdditionalAddressInformation>
        <Address1>FORWARDER ADDRESS LINE 1</Address1>
        <Address2>FORWARDER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>FORWARDER CITY</City>
        <CompanyName>FORWARDER</CompanyName>
        <Contact>Forwarder contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Forwarder email</Email>
        <Fax>Forwarder fax</Fax>
        <GovRegNum>Forwarder tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Forwarder phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>FORWARDER </Postcode>
        <State>FORWARDER STATE</State>

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
            <Type Description=""NVOCC Reference"">NVO</Type>
            <CountryOfIssue Name=""China"">CN</CountryOfIssue>
            <Value>NVOCC111</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>

    <PaymentHandlingInstructionCollection>
      <PaymentHandlingInstruction>
        <Category Description=""Freight"">FRT</Category>
        <PaymentMethod Description=""Free"">FRE</PaymentMethod>
      </PaymentHandlingInstruction>
      <PaymentHandlingInstruction>
        <Category Description=""Destination Haulage"">DHC</Category>
        <PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
      </PaymentHandlingInstruction>
      <PaymentHandlingInstruction>
        <Category Description=""Destination Port"">DPC</Category>
        <PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
      </PaymentHandlingInstruction>
      <PaymentHandlingInstruction>
        <Category Description=""Origin Haulage"">OHC</Category>
        <PaymentMethod Description=""Collect"">CCX</PaymentMethod>
      </PaymentHandlingInstruction>
      <PaymentHandlingInstruction>
        <Category Description=""Origin Port"">OPC</Category>
        <PaymentMethod Description=""Collect"">CCX</PaymentMethod>
      </PaymentHandlingInstruction>
    </PaymentHandlingInstructionCollection>

    <SubShipmentCollection>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>SL001</Key>
            <Type>Booking</Type>
          </DataSource>

          <Workflow>
            <ActionPurpose Description=""As Per Payload"">APP</ActionPurpose>
          </Workflow>
        </DataContext>

        <BookingConfirmationReference>SL001</BookingConfirmationReference>

        <PackingLineCollection>
          <PackingLine>
            <ContainerLink>1</ContainerLink>
            <ContainerNumber></ContainerNumber>
            <DetailedDescription>AAA packline 1</DetailedDescription>
            <ExportReferenceNumber>SL001</ExportReferenceNumber>
            <GoodsDescription>AAA packline 1</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID>Test001</PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiresTemperatureControl>false</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>

            <ClassificationCollection>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
            </ClassificationCollection>
          </PackingLine>
          <PackingLine>
            <ContainerLink>2</ContainerLink>
            <ContainerNumber></ContainerNumber>
            <DetailedDescription>BBB packline 1</DetailedDescription>
            <ExportReferenceNumber>SL001</ExportReferenceNumber>
            <GoodsDescription>BBB packline 1</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID>Test003</PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiresTemperatureControl>false</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>

            <ClassificationCollection>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
            </ClassificationCollection>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>SL002</Key>
            <Type>Booking</Type>
          </DataSource>

          <Workflow>
            <ActionPurpose Description=""As Per Payload"">APP</ActionPurpose>
          </Workflow>
        </DataContext>

        <BookingConfirmationReference>SL002</BookingConfirmationReference>

        <PackingLineCollection>
          <PackingLine>
            <ContainerLink>1</ContainerLink>
            <ContainerNumber></ContainerNumber>
            <DetailedDescription>AAA packline 2</DetailedDescription>
            <ExportReferenceNumber>SL002</ExportReferenceNumber>
            <GoodsDescription>AAA packline 2</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID>Test002</PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiresTemperatureControl>false</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>

            <ClassificationCollection>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
            </ClassificationCollection>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
    </SubShipmentCollection>

    <TransportLegCollection Content=""Complete"">
      <TransportLeg>
        <PortOfDischarge Name=""Auckland"">NZAKL</PortOfDischarge>
        <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
        <LegOrder>1</LegOrder>
        <ActualArrival>2018-07-10T00:00:00</ActualArrival>
        <ActualDeparture>2018-06-10T00:00:00</ActualDeparture>
        <EstimatedArrival>2018-07-10T00:00:00</EstimatedArrival>
        <EstimatedDeparture>2018-06-10T00:00:00</EstimatedDeparture>
        <LCLCutOff></LCLCutOff>
        <LCLReceivalCommences></LCLReceivalCommences>
        <LegType>Main</LegType>
        <TransportMode>Sea</TransportMode>
        <VesselLloydsIMO>IMO111</VesselLloydsIMO>
        <VesselName>Fudge Fixtures</VesselName>
        <VoyageFlightNo>AAAA</VoyageFlightNo>

        <AdditionalTransportModeCollection>
          <AdditionalTransportMode>
            <TransportMode>Road</TransportMode>
          </AdditionalTransportMode>
        </AdditionalTransportModeCollection>
      </TransportLeg>
    </TransportLegCollection>
  </Shipment>
</UniversalShipment>";

		string GetExpectedXmlIsGroupAndConsolidatePackingLines() => $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>C00001000</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>
    </DataContext>

    <BookingConfirmationReference>BKG0001</BookingConfirmationReference>
    <CarrierBookingOffice Name=""Pital Con Desvio"">CRAPO</CarrierBookingOffice>
    <ContainerMode Description=""Full Container Load"">FCL</ContainerMode>
    <DeliveryMode Description=""Door To Peer"">DTP</DeliveryMode>
    <LloydsIMO>12345</LloydsIMO>
    <NoCopyBills>2</NoCopyBills>
    <NoOriginalBills>1</NoOriginalBills>
    <PlaceOfDelivery Name=""Perth"">AUPER</PlaceOfDelivery>
    <PlaceOfIssue Name=""Melbourne"">AUMEL</PlaceOfIssue>
    <PlaceOfReceipt Name=""Brisbane"">AUBNE</PlaceOfReceipt>
    <PortOfDischarge Name=""Singapore"">SGSIN</PortOfDischarge>
    <PortOfLoading Name=""Brisbane"">AUBNE</PortOfLoading>
    <ReleaseType Description=""Sea Waybill"">SWB</ReleaseType>
    <ShipmentType Description=""Agent"">AGT</ShipmentType>
    <VesselName>Titanic</VesselName>
    <VoyageFlightNo>1234567</VoyageFlightNo>
    <WayBillNumber>bill of lading number</WayBillNumber>
    <AddInfoCollection>
      <AddInfo>
        <Key>CarrierBookingOffice_Code</Key>
        <Value>CRAPO</Value>
      </AddInfo>
      <AddInfo>
        <Key>CarrierBookingOffice_Name</Key>
        <Value>Pital Con Desvio</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>CNSHA</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Shanghai</Value>
      </AddInfo>
      <AddInfo>
        <Key>FreightPayableAt_Code</Key>
        <Value>AMEVN</Value>
      </AddInfo>
      <AddInfo>
        <Key>FreightPayableAt_Name</Key>
        <Value>Yerevan</Value>
      </AddInfo>
      <AddInfo>
        <Key>NVOCC_Registration_Reference</Key>
        <Value>NVOCC111</Value>
      </AddInfo>
      <AddInfo>
        <Key>FormVersion</Key>
        <Value>4.0.0</Value>
      </AddInfo>
      <AddInfo>
        <Key>GroupingMethod</Key>
        <Value>SHP</Value>
      </AddInfo>
    </AddInfoCollection>
    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Bill Of Lading Number"">BOL</Type>
        <ReferenceNumber>bill of lading number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Shipper Reference"">SHP</Type>
        <ReferenceNumber>shipper reference number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Freight Forwarder Reference"">FFW</Type>
        <ReferenceNumber>freight forwarder reference number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Contract Number"">CON</Type>
        <ReferenceNumber>carrier contract number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Quote Number"">CQN</Type>
        <ReferenceNumber>carrier contract number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Contract Named Account"">NAC</Type>
        <ReferenceNumber>contract named account</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Shipping Order/Shi Lian Dan"">SLD</Type>
        <ReferenceNumber>1234</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>
    <BillOfLadingClauseCollection>
      <BillOfLadingClause>
        <Type Description=""Freight Collect"">FCL</Type>
      </BillOfLadingClause>
    </BillOfLadingClauseCollection>
    <ContainerCollection>
      <Container>
        <AirVentFlow>12</AirVentFlow>
        <AirVentFlowRateUnit Description=""Cubic feet per minute"">2L</AirVentFlowRateUnit>
        <ArrivalDeliveryRequiredBy></ArrivalDeliveryRequiredBy>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>AAA</ContainerNumber>
        <ContainerQuality Description=""Food"">FOD</ContainerQuality>
        <ContainerType>
          <Code>20FR</Code>
          <Category Description=""Flat Rack"">FLT</Category>
          <Description>Twenty foot flatrack</Description>
          <ISOCode>22P1</ISOCode>
        </ContainerType>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <DunnageWeight>30</DunnageWeight>
        <EmptyRequired></EmptyRequired>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <GoodsWeight>200</GoodsWeight>
        <GrossWeight>272</GrossWeight>
        <GrossWeightVerificationDateTime></GrossWeightVerificationDateTime>
        <HumidityPercent>0</HumidityPercent>
        <ImportDepotCustomsReference></ImportDepotCustomsReference>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>true</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <Link>1</Link>
        <NonOperatingReefer>true</NonOperatingReefer>
        <Seal>SEAL1</Seal>
        <SealPartyType Description=""Carrier"">CAR</SealPartyType>
        <SecondSeal>SEAL2</SecondSeal>
        <SecondSealPartyType Description=""Customs"">CUS</SecondSealPartyType>
        <TareWeight>20</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal>SEAL3</ThirdSeal>
        <ThirdSealPartyType Description=""Terminal"">CTP</ThirdSealPartyType>
        <WeightUnit>KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>Genset</Key>
            <Value>false</Value>
          </AddInfo>
        </AddInfoCollection>
        <OrganizationAddressCollection>
        </OrganizationAddressCollection>
      </Container>
      <Container>
        <AirVentFlow>12</AirVentFlow>
        <AirVentFlowRateUnit Description=""Cubic feet per minute"">2L</AirVentFlowRateUnit>
        <ArrivalDeliveryRequiredBy></ArrivalDeliveryRequiredBy>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>BBB</ContainerNumber>
        <ContainerQuality Description=""Food"">FOD</ContainerQuality>
        <ContainerType>
          <Code>20FR</Code>
          <Category Description=""Flat Rack"">FLT</Category>
          <Description>Twenty foot flatrack</Description>
          <ISOCode>22P1</ISOCode>
        </ContainerType>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <DunnageWeight>30</DunnageWeight>
        <EmptyRequired></EmptyRequired>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <GoodsWeight>200</GoodsWeight>
        <GrossWeight>272</GrossWeight>
        <GrossWeightVerificationDateTime></GrossWeightVerificationDateTime>
        <HumidityPercent>0</HumidityPercent>
        <ImportDepotCustomsReference></ImportDepotCustomsReference>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>true</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <Link>2</Link>
        <NonOperatingReefer>false</NonOperatingReefer>
        <Seal>SEAL1</Seal>
        <SealPartyType Description=""Carrier"">CAR</SealPartyType>
        <SecondSeal>SEAL2</SecondSeal>
        <SecondSealPartyType Description=""Customs"">CUS</SecondSealPartyType>
        <TareWeight>20</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal>SEAL3</ThirdSeal>
        <ThirdSealPartyType Description=""Terminal"">CTP</ThirdSealPartyType>
        <WeightUnit>KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>Genset</Key>
            <Value>false</Value>
          </AddInfo>
        </AddInfoCollection>
        <OrganizationAddressCollection>
        </OrganizationAddressCollection>
      </Container>
    </ContainerCollection>
    <DateCollection>
      <Date>
        <Type>BillRequiredBy</Type>
        <Value>2018-05-20T00:00:00</Value>
      </Date>
    </DateCollection>
    <NoteCollection>
      <Note>
        <Description>Forwarding Instruction Notes</Description>
        <NoteText>forwarding instructions</NoteText>
      </Note>
      <Note>
        <Description>Goods Handling Instructions</Description>
        <NoteText>goods handling instructions</NoteText>
      </Note>
      <Note>
        <Description>Special Instructions</Description>
        <NoteText>special instructions</NoteText>
      </Note>
      <Note>
        <Description>USCanadaManifestSelfFilerID</Description>
        <NoteText>1111</NoteText>
      </Note>
      <Note>
        <Description>ChargesFreighted</Description>
        <NoteText>Y</NoteText>
      </Note>
    </NoteCollection>
    <OrganizationAddressCollection>
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
        <AddressType>ShippingLineAddress</AddressType>
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
        <AddressType>CarrierHandlingAgent</AddressType>
        <AdditionalAddressInformation>CarrierHandlingAgent additional info</AdditionalAddressInformation>
        <Address1>CARRIERHANDLINGAGENT ADDRESS LINE 1</Address1>
        <Address2>CARRIERHANDLINGAGENT ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CARRIERHANDLINGAGENT CITY</City>
        <CompanyName>CARRIERHANDLINGAGENT</CompanyName>
        <Contact>CarrierHandlingAgent contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>CarrierHandlingAgent email</Email>
        <Fax>CarrierHandlingAgent</Fax>
        <GovRegNum>CarrierHandlingAgent tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>CarrierHandlingAgent</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CARRIERHAN</Postcode>
        <State>CARRIERHANDLINGAGENT STAT</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>CarrierBookingAgent</AddressType>
        <AdditionalAddressInformation>CarrierBookingAgent additional info</AdditionalAddressInformation>
        <Address1>CARRIERBOOKINGAGENT ADDRESS LINE 1</Address1>
        <Address2>CARRIERBOOKINGAGENT ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CARRIERBOOKINGAGENT CITY</City>
        <CompanyName>CARRIERBOOKINGAGENT</CompanyName>
        <Contact>CarrierBookingAgent contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>CarrierBookingAgent email</Email>
        <Fax>CarrierBookingAgent </Fax>
        <GovRegNum>CarrierBookingAgent tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>CarrierBookingAgent </Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CARRIERBOO</Postcode>
        <State>CARRIERBOOKINGAGENT STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>NotifyParty</AddressType>
        <AdditionalAddressInformation>NotifyParty additional info</AdditionalAddressInformation>
        <Address1>NOTIFYPARTY ADDRESS LINE 1</Address1>
        <Address2>NOTIFYPARTY ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>NOTIFYPARTY CITY</City>
        <CompanyName>NOTIFYPARTY</CompanyName>
        <Contact>NotifyParty contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>NotifyParty email</Email>
        <Fax>NotifyParty fax</Fax>
        <GovRegNum>NotifyParty tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>NotifyParty phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>NOTIFYPART</Postcode>
        <State>NOTIFYPARTY STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>NotifyParty2</AddressType>
        <AdditionalAddressInformation>NotifyParty2 additional info</AdditionalAddressInformation>
        <Address1>NOTIFYPARTY2 ADDRESS LINE 1</Address1>
        <Address2>NOTIFYPARTY2 ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>NOTIFYPARTY2 CITY</City>
        <CompanyName>NOTIFYPARTY2</CompanyName>
        <Contact>NotifyParty2 contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>NotifyParty2 email</Email>
        <Fax>NotifyParty2 fax</Fax>
        <GovRegNum>NotifyParty2 tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>NotifyParty2 phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>NOTIFYPART</Postcode>
        <State>NOTIFYPARTY2 STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>BookingPartyDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>Forwarder additional info</AdditionalAddressInformation>
        <Address1>FORWARDER ADDRESS LINE 1</Address1>
        <Address2>FORWARDER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>FORWARDER CITY</City>
        <CompanyName>FORWARDER</CompanyName>
        <Contact>Forwarder contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Forwarder email</Email>
        <Fax>Forwarder fax</Fax>
        <GovRegNum>Forwarder tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Forwarder phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>FORWARDER </Postcode>
        <State>FORWARDER STATE</State>
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
            <Type Description=""NVOCC Reference"">NVO</Type>
            <CountryOfIssue Name=""China"">CN</CountryOfIssue>
            <Value>NVOCC111</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>
    <PaymentHandlingInstructionCollection>
      <PaymentHandlingInstruction>
        <Category Description=""Freight"">FRT</Category>
        <PaymentMethod Description=""Free"">FRE</PaymentMethod>
      </PaymentHandlingInstruction>
      <PaymentHandlingInstruction>
        <Category Description=""Destination Haulage"">DHC</Category>
        <PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
      </PaymentHandlingInstruction>
      <PaymentHandlingInstruction>
        <Category Description=""Destination Port"">DPC</Category>
        <PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
      </PaymentHandlingInstruction>
      <PaymentHandlingInstruction>
        <Category Description=""Origin Haulage"">OHC</Category>
        <PaymentMethod Description=""Collect"">CCX</PaymentMethod>
      </PaymentHandlingInstruction>
      <PaymentHandlingInstruction>
        <Category Description=""Origin Port"">OPC</Category>
        <PaymentMethod Description=""Collect"">CCX</PaymentMethod>
      </PaymentHandlingInstruction>
    </PaymentHandlingInstructionCollection>
    <SubShipmentCollection>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>S0000009</Key>
            <Type>Booking</Type>
          </DataSource>

          <Workflow>
            <ActionPurpose Description=""As Per Payload"">APP</ActionPurpose>
          </Workflow>
        </DataContext>

        <BookingConfirmationReference>S0000009</BookingConfirmationReference>
        <PackingLineCollection>
          <PackingLine>
            <ContainerNumber></ContainerNumber>
            <DetailedDescription>AAA packline 1</DetailedDescription>
            <ExportReferenceNumber>SL001</ExportReferenceNumber>
            <GoodsDescription>AAA packline 1</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID>Test001</PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiresTemperatureControl>false</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>
            <ClassificationCollection>
              <Classification>
                <Code>HC12345</Code>
                <Country></Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
            </ClassificationCollection>
            <PackingLineCollection>
              <PackingLine>
                <ContainerLink>1</ContainerLink>
                <ContainerNumber></ContainerNumber>
                <DetailedDescription>AAA packline 1</DetailedDescription>
                <ExportReferenceNumber>SL002</ExportReferenceNumber>
                <GoodsDescription>AAA packline 1</GoodsDescription>
                <HarmonisedCode>HC12345</HarmonisedCode>
                <ImportReferenceNumber>import reference number</ImportReferenceNumber>
                <MarksAndNos>marks &amp; nums</MarksAndNos>
                <OutturnComment></OutturnComment>
                <PackingLineID>Test002</PackingLineID>
                <PackQty>3</PackQty>
                <PackType Description=""Pallet"">PLT</PackType>
                <ReferenceNumber>reference number</ReferenceNumber>
                <RequiresTemperatureControl>false</RequiresTemperatureControl>
                <Volume>55</Volume>
                <VolumeUnit>M3</VolumeUnit>
                <Weight>88</Weight>
                <WeightUnit>KG</WeightUnit>
                <ClassificationCollection>
                  <Classification>
                    <Code>HC12345</Code>
                    <Country></Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                  <Classification>
                    <Code>1234.56</Code>
                    <Country Name=""China"">CN</Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                </ClassificationCollection>
                <UNDGCollection>
                  <UNDG>
                    <IMOClass>A</IMOClass>
                    <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                    <PackingGroup></PackingGroup>
                    <PackQty>11</PackQty>
                    <ProperShippingName>Danger</ProperShippingName>
                    <Standard>IAT</Standard>
                    <SubLabel1></SubLabel1>
                    <SubLabel2></SubLabel2>
                    <TechicalName>Technicals</TechicalName>
                    <UNDGCode>0001</UNDGCode>
                  </UNDG>
                </UNDGCollection>
              </PackingLine>
              <PackingLine>
                <ContainerLink>1</ContainerLink>
                <ContainerNumber></ContainerNumber>
                <DetailedDescription>AAA packline 1</DetailedDescription>
                <ExportReferenceNumber>SL003</ExportReferenceNumber>
                <GoodsDescription>AAA packline 1</GoodsDescription>
                <HarmonisedCode>HC12345</HarmonisedCode>
                <ImportReferenceNumber>import reference number</ImportReferenceNumber>
                <MarksAndNos>marks &amp; nums</MarksAndNos>
                <OutturnComment></OutturnComment>
                <PackingLineID>Test003</PackingLineID>
                <PackQty>3</PackQty>
                <PackType Description=""Pallet"">PLT</PackType>
                <ReferenceNumber>reference number</ReferenceNumber>
                <RequiresTemperatureControl>false</RequiresTemperatureControl>
                <Volume>55</Volume>
                <VolumeUnit>M3</VolumeUnit>
                <Weight>88</Weight>
                <WeightUnit>KG</WeightUnit>
                <ClassificationCollection>
                  <Classification>
                    <Code>HC12345</Code>
                    <Country></Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                  <Classification>
                    <Code>1234.56</Code>
                    <Country Name=""China"">CN</Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                </ClassificationCollection>
                <UNDGCollection>
                  <UNDG>
                    <IMOClass>A</IMOClass>
                    <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                    <PackingGroup></PackingGroup>
                    <PackQty>11</PackQty>
                    <ProperShippingName>Danger</ProperShippingName>
                    <Standard>IAT</Standard>
                    <SubLabel1></SubLabel1>
                    <SubLabel2></SubLabel2>
                    <TechicalName>Technicals</TechicalName>
                    <UNDGCode>0001</UNDGCode>
                  </UNDG>
                </UNDGCollection>
              </PackingLine>
            </PackingLineCollection>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>S00000013</Key>
            <Type>Booking</Type>
          </DataSource>

          <Workflow>
            <ActionPurpose Description=""As Per Payload"">APP</ActionPurpose>
          </Workflow>
        </DataContext>

        <BookingConfirmationReference>S00000013</BookingConfirmationReference>
        <PackingLineCollection>
          <PackingLine>
            <ContainerNumber></ContainerNumber>
            <DetailedDescription>BBB packline 1</DetailedDescription>
            <ExportReferenceNumber>SL001</ExportReferenceNumber>
            <GoodsDescription>BBB packline 1</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID>Test004</PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiresTemperatureControl>false</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>
            <ClassificationCollection>
              <Classification>
                <Code>HC12345</Code>
                <Country></Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
            </ClassificationCollection>
            <PackingLineCollection>
              <PackingLine>
                <ContainerLink>2</ContainerLink>
                <ContainerNumber></ContainerNumber>
                <DetailedDescription>BBB packline 1</DetailedDescription>
                <ExportReferenceNumber>SL002</ExportReferenceNumber>
                <GoodsDescription>BBB packline 1</GoodsDescription>
                <HarmonisedCode>HC12345</HarmonisedCode>
                <ImportReferenceNumber>import reference number</ImportReferenceNumber>
                <MarksAndNos>marks &amp; nums</MarksAndNos>
                <OutturnComment></OutturnComment>
                <PackingLineID>Test005</PackingLineID>
                <PackQty>3</PackQty>
                <PackType Description=""Pallet"">PLT</PackType>
                <ReferenceNumber>reference number</ReferenceNumber>
                <RequiresTemperatureControl>false</RequiresTemperatureControl>
                <Volume>55</Volume>
                <VolumeUnit>M3</VolumeUnit>
                <Weight>88</Weight>
                <WeightUnit>KG</WeightUnit>
                <ClassificationCollection>
                  <Classification>
                    <Code>HC12345</Code>
                    <Country></Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                  <Classification>
                    <Code>1234.56</Code>
                    <Country Name=""China"">CN</Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                </ClassificationCollection>
                <UNDGCollection>
                  <UNDG>
                    <IMOClass>A</IMOClass>
                    <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                    <PackingGroup></PackingGroup>
                    <PackQty>11</PackQty>
                    <ProperShippingName>Danger</ProperShippingName>
                    <Standard>IAT</Standard>
                    <SubLabel1></SubLabel1>
                    <SubLabel2></SubLabel2>
                    <TechicalName>Technicals</TechicalName>
                    <UNDGCode>0001</UNDGCode>
                  </UNDG>
                </UNDGCollection>
              </PackingLine>
              <PackingLine>
                <ContainerLink>2</ContainerLink>
                <ContainerNumber></ContainerNumber>
                <DetailedDescription>BBB packline 1</DetailedDescription>
                <ExportReferenceNumber>SL003</ExportReferenceNumber>
                <GoodsDescription>BBB packline 1</GoodsDescription>
                <HarmonisedCode>HC12345</HarmonisedCode>
                <ImportReferenceNumber>import reference number</ImportReferenceNumber>
                <MarksAndNos>marks &amp; nums</MarksAndNos>
                <OutturnComment></OutturnComment>
                <PackingLineID>Test006</PackingLineID>
                <PackQty>3</PackQty>
                <PackType Description=""Pallet"">PLT</PackType>
                <ReferenceNumber>reference number</ReferenceNumber>
                <RequiresTemperatureControl>false</RequiresTemperatureControl>
                <Volume>55</Volume>
                <VolumeUnit>M3</VolumeUnit>
                <Weight>88</Weight>
                <WeightUnit>KG</WeightUnit>
                <ClassificationCollection>
                  <Classification>
                    <Code>HC12345</Code>
                    <Country></Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                  <Classification>
                    <Code>1234.56</Code>
                    <Country Name=""China"">CN</Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                </ClassificationCollection>
                <UNDGCollection>
                  <UNDG>
                    <IMOClass>A</IMOClass>
                    <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                    <PackingGroup></PackingGroup>
                    <PackQty>11</PackQty>
                    <ProperShippingName>Danger</ProperShippingName>
                    <Standard>IAT</Standard>
                    <SubLabel1></SubLabel1>
                    <SubLabel2></SubLabel2>
                    <TechicalName>Technicals</TechicalName>
                    <UNDGCode>0001</UNDGCode>
                  </UNDG>
                </UNDGCollection>
              </PackingLine>
            </PackingLineCollection>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
    </SubShipmentCollection>
    <TransportLegCollection Content=""Complete"">
      <TransportLeg>
        <PortOfDischarge Name=""Auckland"">NZAKL</PortOfDischarge>
        <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
        <LegOrder>1</LegOrder>
        <ActualArrival>2018-07-10T00:00:00</ActualArrival>
        <ActualDeparture>2018-06-10T00:00:00</ActualDeparture>
        <EstimatedArrival>2018-07-10T00:00:00</EstimatedArrival>
        <EstimatedDeparture>2018-06-10T00:00:00</EstimatedDeparture>
        <LCLCutOff></LCLCutOff>
        <LCLReceivalCommences></LCLReceivalCommences>
        <LegType>Main</LegType>
        <TransportMode>Sea</TransportMode>
        <VesselLloydsIMO>IMO111</VesselLloydsIMO>
        <VesselName>Fudge Fixtures</VesselName>
        <VoyageFlightNo>AAAA</VoyageFlightNo>
        <AdditionalTransportModeCollection>
          <AdditionalTransportMode>
            <TransportMode>Road</TransportMode>
          </AdditionalTransportMode>
        </AdditionalTransportModeCollection>
      </TransportLeg>
    </TransportLegCollection>
  </Shipment>
</UniversalShipment>
";

		string GetExpectedXmlDoNoGroup() => $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>C00001000</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>
    </DataContext>

    <BookingConfirmationReference>BKG0001</BookingConfirmationReference>
    <CarrierBookingOffice Name=""Pital Con Desvio"">CRAPO</CarrierBookingOffice>
    <ContainerMode Description=""Full Container Load"">FCL</ContainerMode>
    <DeliveryMode Description=""Door To Peer"">DTP</DeliveryMode>
    <LloydsIMO>12345</LloydsIMO>
    <NoCopyBills>2</NoCopyBills>
    <NoOriginalBills>1</NoOriginalBills>
    <PlaceOfDelivery Name=""Perth"">AUPER</PlaceOfDelivery>
    <PlaceOfIssue Name=""Melbourne"">AUMEL</PlaceOfIssue>
    <PlaceOfReceipt Name=""Brisbane"">AUBNE</PlaceOfReceipt>
    <PortOfDischarge Name=""Singapore"">SGSIN</PortOfDischarge>
    <PortOfLoading Name=""Brisbane"">AUBNE</PortOfLoading>
    <ReleaseType Description=""Sea Waybill"">SWB</ReleaseType>
    <ShipmentType Description=""Agent"">AGT</ShipmentType>
    <VesselName>Titanic</VesselName>
    <VoyageFlightNo>1234567</VoyageFlightNo>
    <WayBillNumber>bill of lading number</WayBillNumber>
    <AddInfoCollection>
      <AddInfo>
        <Key>CarrierBookingOffice_Code</Key>
        <Value>CRAPO</Value>
      </AddInfo>
      <AddInfo>
        <Key>CarrierBookingOffice_Name</Key>
        <Value>Pital Con Desvio</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>CNSHA</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Shanghai</Value>
      </AddInfo>
      <AddInfo>
        <Key>FreightPayableAt_Code</Key>
        <Value>AMEVN</Value>
      </AddInfo>
      <AddInfo>
        <Key>FreightPayableAt_Name</Key>
        <Value>Yerevan</Value>
      </AddInfo>
      <AddInfo>
        <Key>NVOCC_Registration_Reference</Key>
        <Value>NVOCC111</Value>
      </AddInfo>
      <AddInfo>
        <Key>FormVersion</Key>
        <Value>4.0.0</Value>
      </AddInfo>
      <AddInfo>
        <Key>GroupingMethod</Key>
        <Value>DNG</Value>
      </AddInfo>
    </AddInfoCollection>
    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Bill Of Lading Number"">BOL</Type>
        <ReferenceNumber>bill of lading number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Shipper Reference"">SHP</Type>
        <ReferenceNumber>shipper reference number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Freight Forwarder Reference"">FFW</Type>
        <ReferenceNumber>freight forwarder reference number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Contract Number"">CON</Type>
        <ReferenceNumber>carrier contract number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Quote Number"">CQN</Type>
        <ReferenceNumber>carrier contract number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Contract Named Account"">NAC</Type>
        <ReferenceNumber>contract named account</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Shipping Order/Shi Lian Dan"">SLD</Type>
        <ReferenceNumber>1234</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>
    <BillOfLadingClauseCollection>
      <BillOfLadingClause>
        <Type Description=""Freight Collect"">FCL</Type>
      </BillOfLadingClause>
    </BillOfLadingClauseCollection>
    <ContainerCollection>
      <Container>
        <AirVentFlow>12</AirVentFlow>
        <AirVentFlowRateUnit Description=""Cubic feet per minute"">2L</AirVentFlowRateUnit>
        <ArrivalDeliveryRequiredBy></ArrivalDeliveryRequiredBy>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>AAA</ContainerNumber>
        <ContainerQuality Description=""Food"">FOD</ContainerQuality>
        <ContainerType>
          <Code>20FR</Code>
          <Category Description=""Flat Rack"">FLT</Category>
          <Description>Twenty foot flatrack</Description>
          <ISOCode>22P1</ISOCode>
        </ContainerType>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <DunnageWeight>30</DunnageWeight>
        <EmptyRequired></EmptyRequired>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <GoodsWeight>200</GoodsWeight>
        <GrossWeight>272</GrossWeight>
        <GrossWeightVerificationDateTime></GrossWeightVerificationDateTime>
        <HumidityPercent>0</HumidityPercent>
        <ImportDepotCustomsReference></ImportDepotCustomsReference>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>true</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <Link>1</Link>
        <NonOperatingReefer>true</NonOperatingReefer>
        <Seal>SEAL1</Seal>
        <SealPartyType Description=""Carrier"">CAR</SealPartyType>
        <SecondSeal>SEAL2</SecondSeal>
        <SecondSealPartyType Description=""Customs"">CUS</SecondSealPartyType>
        <TareWeight>20</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal>SEAL3</ThirdSeal>
        <ThirdSealPartyType Description=""Terminal"">CTP</ThirdSealPartyType>
        <WeightUnit>KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>Genset</Key>
            <Value>false</Value>
          </AddInfo>
        </AddInfoCollection>
        <OrganizationAddressCollection>
        </OrganizationAddressCollection>
      </Container>
      <Container>
        <AirVentFlow>12</AirVentFlow>
        <AirVentFlowRateUnit Description=""Cubic feet per minute"">2L</AirVentFlowRateUnit>
        <ArrivalDeliveryRequiredBy></ArrivalDeliveryRequiredBy>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>BBB</ContainerNumber>
        <ContainerQuality Description=""Food"">FOD</ContainerQuality>
        <ContainerType>
          <Code>20FR</Code>
          <Category Description=""Flat Rack"">FLT</Category>
          <Description>Twenty foot flatrack</Description>
          <ISOCode>22P1</ISOCode>
        </ContainerType>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <DunnageWeight>30</DunnageWeight>
        <EmptyRequired></EmptyRequired>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <GoodsWeight>200</GoodsWeight>
        <GrossWeight>272</GrossWeight>
        <GrossWeightVerificationDateTime></GrossWeightVerificationDateTime>
        <HumidityPercent>0</HumidityPercent>
        <ImportDepotCustomsReference></ImportDepotCustomsReference>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>true</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <Link>2</Link>
        <NonOperatingReefer>false</NonOperatingReefer>
        <Seal>SEAL1</Seal>
        <SealPartyType Description=""Carrier"">CAR</SealPartyType>
        <SecondSeal>SEAL2</SecondSeal>
        <SecondSealPartyType Description=""Customs"">CUS</SecondSealPartyType>
        <TareWeight>20</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal>SEAL3</ThirdSeal>
        <ThirdSealPartyType Description=""Terminal"">CTP</ThirdSealPartyType>
        <WeightUnit>KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>Genset</Key>
            <Value>false</Value>
          </AddInfo>
        </AddInfoCollection>
        <OrganizationAddressCollection>
        </OrganizationAddressCollection>
      </Container>
    </ContainerCollection>
    <DateCollection>
      <Date>
        <Type>BillRequiredBy</Type>
        <Value>2018-05-20T00:00:00</Value>
      </Date>
    </DateCollection>
    <NoteCollection>
      <Note>
        <Description>Forwarding Instruction Notes</Description>
        <NoteText>forwarding instructions</NoteText>
      </Note>
      <Note>
        <Description>Goods Handling Instructions</Description>
        <NoteText>goods handling instructions</NoteText>
      </Note>
      <Note>
        <Description>Special Instructions</Description>
        <NoteText>special instructions</NoteText>
      </Note>
      <Note>
        <Description>USCanadaManifestSelfFilerID</Description>
        <NoteText>1111</NoteText>
      </Note>
      <Note>
        <Description>ChargesFreighted</Description>
        <NoteText>Y</NoteText>
      </Note>
    </NoteCollection>
    <OrganizationAddressCollection>
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
        <AddressType>ShippingLineAddress</AddressType>
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
        <AddressType>CarrierHandlingAgent</AddressType>
        <AdditionalAddressInformation>CarrierHandlingAgent additional info</AdditionalAddressInformation>
        <Address1>CARRIERHANDLINGAGENT ADDRESS LINE 1</Address1>
        <Address2>CARRIERHANDLINGAGENT ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CARRIERHANDLINGAGENT CITY</City>
        <CompanyName>CARRIERHANDLINGAGENT</CompanyName>
        <Contact>CarrierHandlingAgent contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>CarrierHandlingAgent email</Email>
        <Fax>CarrierHandlingAgent</Fax>
        <GovRegNum>CarrierHandlingAgent tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>CarrierHandlingAgent</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CARRIERHAN</Postcode>
        <State>CARRIERHANDLINGAGENT STAT</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>CarrierBookingAgent</AddressType>
        <AdditionalAddressInformation>CarrierBookingAgent additional info</AdditionalAddressInformation>
        <Address1>CARRIERBOOKINGAGENT ADDRESS LINE 1</Address1>
        <Address2>CARRIERBOOKINGAGENT ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CARRIERBOOKINGAGENT CITY</City>
        <CompanyName>CARRIERBOOKINGAGENT</CompanyName>
        <Contact>CarrierBookingAgent contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>CarrierBookingAgent email</Email>
        <Fax>CarrierBookingAgent </Fax>
        <GovRegNum>CarrierBookingAgent tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>CarrierBookingAgent </Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CARRIERBOO</Postcode>
        <State>CARRIERBOOKINGAGENT STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>NotifyParty</AddressType>
        <AdditionalAddressInformation>NotifyParty additional info</AdditionalAddressInformation>
        <Address1>NOTIFYPARTY ADDRESS LINE 1</Address1>
        <Address2>NOTIFYPARTY ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>NOTIFYPARTY CITY</City>
        <CompanyName>NOTIFYPARTY</CompanyName>
        <Contact>NotifyParty contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>NotifyParty email</Email>
        <Fax>NotifyParty fax</Fax>
        <GovRegNum>NotifyParty tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>NotifyParty phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>NOTIFYPART</Postcode>
        <State>NOTIFYPARTY STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>NotifyParty2</AddressType>
        <AdditionalAddressInformation>NotifyParty2 additional info</AdditionalAddressInformation>
        <Address1>NOTIFYPARTY2 ADDRESS LINE 1</Address1>
        <Address2>NOTIFYPARTY2 ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>NOTIFYPARTY2 CITY</City>
        <CompanyName>NOTIFYPARTY2</CompanyName>
        <Contact>NotifyParty2 contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>NotifyParty2 email</Email>
        <Fax>NotifyParty2 fax</Fax>
        <GovRegNum>NotifyParty2 tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>NotifyParty2 phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>NOTIFYPART</Postcode>
        <State>NOTIFYPARTY2 STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>BookingPartyDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>Forwarder additional info</AdditionalAddressInformation>
        <Address1>FORWARDER ADDRESS LINE 1</Address1>
        <Address2>FORWARDER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>FORWARDER CITY</City>
        <CompanyName>FORWARDER</CompanyName>
        <Contact>Forwarder contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Forwarder email</Email>
        <Fax>Forwarder fax</Fax>
        <GovRegNum>Forwarder tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Forwarder phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>FORWARDER </Postcode>
        <State>FORWARDER STATE</State>
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
            <Type Description=""NVOCC Reference"">NVO</Type>
            <CountryOfIssue Name=""China"">CN</CountryOfIssue>
            <Value>NVOCC111</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>
    <PaymentHandlingInstructionCollection>
      <PaymentHandlingInstruction>
        <Category Description=""Freight"">FRT</Category>
        <PaymentMethod Description=""Free"">FRE</PaymentMethod>
      </PaymentHandlingInstruction>
      <PaymentHandlingInstruction>
        <Category Description=""Destination Haulage"">DHC</Category>
        <PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
      </PaymentHandlingInstruction>
      <PaymentHandlingInstruction>
        <Category Description=""Destination Port"">DPC</Category>
        <PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
      </PaymentHandlingInstruction>
      <PaymentHandlingInstruction>
        <Category Description=""Origin Haulage"">OHC</Category>
        <PaymentMethod Description=""Collect"">CCX</PaymentMethod>
      </PaymentHandlingInstruction>
      <PaymentHandlingInstruction>
        <Category Description=""Origin Port"">OPC</Category>
        <PaymentMethod Description=""Collect"">CCX</PaymentMethod>
      </PaymentHandlingInstruction>
    </PaymentHandlingInstructionCollection>
    <SubShipmentCollection>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>SL001</Key>
            <Type>Booking</Type>
          </DataSource>

          <Workflow>
            <ActionPurpose Description=""As Per Payload"">APP</ActionPurpose>
          </Workflow>
        </DataContext>

        <BookingConfirmationReference>SL001</BookingConfirmationReference>
        <PackingLineCollection>
          <PackingLine>
            <ContainerLink>1</ContainerLink>
            <ContainerNumber></ContainerNumber>
            <DetailedDescription>AAA packline 1</DetailedDescription>
            <ExportReferenceNumber>SL001</ExportReferenceNumber>
            <GoodsDescription>AAA packline 1</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID>Test001</PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiresTemperatureControl>false</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>
            <ClassificationCollection>
              <Classification>
                <Code>HC12345</Code>
                <Country></Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
            </ClassificationCollection>
            <PackingLineCollection>
              <PackingLine>
                <ContainerLink>1</ContainerLink>
                <ContainerNumber></ContainerNumber>
                <DetailedDescription>AAA packline 1</DetailedDescription>
                <ExportReferenceNumber>SL001</ExportReferenceNumber>
                <GoodsDescription>AAA packline 1</GoodsDescription>
                <HarmonisedCode>HC12345</HarmonisedCode>
                <ImportReferenceNumber>import reference number</ImportReferenceNumber>
                <MarksAndNos>marks &amp; nums</MarksAndNos>
                <OutturnComment></OutturnComment>
                <PackingLineID>Test001</PackingLineID>
                <PackQty>3</PackQty>
                <PackType Description=""Pallet"">PLT</PackType>
                <ReferenceNumber>reference number</ReferenceNumber>
                <RequiresTemperatureControl>false</RequiresTemperatureControl>
                <Volume>55</Volume>
                <VolumeUnit>M3</VolumeUnit>
                <Weight>88</Weight>
                <WeightUnit>KG</WeightUnit>
                <ClassificationCollection>
                  <Classification>
                    <Code>HC12345</Code>
                    <Country></Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                  <Classification>
                    <Code>1234.56</Code>
                    <Country Name=""China"">CN</Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                </ClassificationCollection>
              </PackingLine>
            </PackingLineCollection>
          </PackingLine>
          <PackingLine>
            <ContainerLink>2</ContainerLink>
            <ContainerNumber></ContainerNumber>
            <DetailedDescription>BBB packline 1</DetailedDescription>
            <ExportReferenceNumber>SL001</ExportReferenceNumber>
            <GoodsDescription>BBB packline 1</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID>Test003</PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiresTemperatureControl>false</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>
            <ClassificationCollection>
              <Classification>
                <Code>HC12345</Code>
                <Country></Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
            </ClassificationCollection>
            <PackingLineCollection>
              <PackingLine>
                <ContainerLink>2</ContainerLink>
                <ContainerNumber></ContainerNumber>
                <DetailedDescription>BBB packline 1</DetailedDescription>
                <ExportReferenceNumber>SL001</ExportReferenceNumber>
                <GoodsDescription>BBB packline 1</GoodsDescription>
                <HarmonisedCode>HC12345</HarmonisedCode>
                <ImportReferenceNumber>import reference number</ImportReferenceNumber>
                <MarksAndNos>marks &amp; nums</MarksAndNos>
                <OutturnComment></OutturnComment>
                <PackingLineID>Test003</PackingLineID>
                <PackQty>3</PackQty>
                <PackType Description=""Pallet"">PLT</PackType>
                <ReferenceNumber>reference number</ReferenceNumber>
                <RequiresTemperatureControl>false</RequiresTemperatureControl>
                <Volume>55</Volume>
                <VolumeUnit>M3</VolumeUnit>
                <Weight>88</Weight>
                <WeightUnit>KG</WeightUnit>
                <ClassificationCollection>
                  <Classification>
                    <Code>HC12345</Code>
                    <Country></Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                  <Classification>
                    <Code>1234.56</Code>
                    <Country Name=""China"">CN</Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                </ClassificationCollection>
              </PackingLine>
            </PackingLineCollection>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>SL002</Key>
            <Type>Booking</Type>
          </DataSource>

          <Workflow>
            <ActionPurpose Description=""As Per Payload"">APP</ActionPurpose>
          </Workflow>
        </DataContext>

        <BookingConfirmationReference>SL002</BookingConfirmationReference>
        <PackingLineCollection>
          <PackingLine>
            <ContainerLink>1</ContainerLink>
            <ContainerNumber></ContainerNumber>
            <DetailedDescription>AAA packline 2</DetailedDescription>
            <ExportReferenceNumber>SL002</ExportReferenceNumber>
            <GoodsDescription>AAA packline 2</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID>Test002</PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiresTemperatureControl>false</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>
            <ClassificationCollection>
              <Classification>
                <Code>HC12345</Code>
                <Country></Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
            </ClassificationCollection>
            <PackingLineCollection>
              <PackingLine>
                <ContainerLink>1</ContainerLink>
                <ContainerNumber></ContainerNumber>
                <DetailedDescription>AAA packline 2</DetailedDescription>
                <ExportReferenceNumber>SL002</ExportReferenceNumber>
                <GoodsDescription>AAA packline 2</GoodsDescription>
                <HarmonisedCode>HC12345</HarmonisedCode>
                <ImportReferenceNumber>import reference number</ImportReferenceNumber>
                <MarksAndNos>marks &amp; nums</MarksAndNos>
                <OutturnComment></OutturnComment>
                <PackingLineID>Test002</PackingLineID>
                <PackQty>3</PackQty>
                <PackType Description=""Pallet"">PLT</PackType>
                <ReferenceNumber>reference number</ReferenceNumber>
                <RequiresTemperatureControl>false</RequiresTemperatureControl>
                <Volume>55</Volume>
                <VolumeUnit>M3</VolumeUnit>
                <Weight>88</Weight>
                <WeightUnit>KG</WeightUnit>
                <ClassificationCollection>
                  <Classification>
                    <Code>HC12345</Code>
                    <Country></Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                  <Classification>
                    <Code>1234.56</Code>
                    <Country Name=""China"">CN</Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                </ClassificationCollection>
              </PackingLine>
            </PackingLineCollection>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
    </SubShipmentCollection>
    <TransportLegCollection Content=""Complete"">
      <TransportLeg>
        <PortOfDischarge Name=""Auckland"">NZAKL</PortOfDischarge>
        <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
        <LegOrder>1</LegOrder>
        <ActualArrival>2018-07-10T00:00:00</ActualArrival>
        <ActualDeparture>2018-06-10T00:00:00</ActualDeparture>
        <EstimatedArrival>2018-07-10T00:00:00</EstimatedArrival>
        <EstimatedDeparture>2018-06-10T00:00:00</EstimatedDeparture>
        <LCLCutOff></LCLCutOff>
        <LCLReceivalCommences></LCLReceivalCommences>
        <LegType>Main</LegType>
        <TransportMode>Sea</TransportMode>
        <VesselLloydsIMO>IMO111</VesselLloydsIMO>
        <VesselName>Fudge Fixtures</VesselName>
        <VoyageFlightNo>AAAA</VoyageFlightNo>
        <AdditionalTransportModeCollection>
          <AdditionalTransportMode>
            <TransportMode>Road</TransportMode>
          </AdditionalTransportMode>
        </AdditionalTransportModeCollection>
      </TransportLeg>
    </TransportLegCollection>
  </Shipment>
</UniversalShipment>
";

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			context = new CommonContext(Factory);
		}
	}
}

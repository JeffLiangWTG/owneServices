using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.eTail.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.eTail.DataTransfer.Testing
{
	public class HVLVItemDataObjectReaderTest : TestCaseWithUniversalObjectFactory
	{
		public void TestReadIntoBusinessObject_ShouldPopulateForeignKeyToConsignment()
		{
			var universalShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var packingLine = new PackingLine();
			universalShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });
			var consignment = Factory.New<HVLVConsignment>();
			var reader = new HVLVItemDataObjectReaderForTest(universalShipment, consignment, packingLine, new DummyLogger(), Factory);
			var newItem = reader.ReadIntoBusinessObject();

			AssertEquals(consignment.PK, newItem.HVI_HVC_Consignment);
		}

		public void TestWhenHVI_ManifestedVolumeIsZero_ThenHVI_ManifestedVolumeIsCalculated()
		{
			var factory = new BusinessObjectFactory();

			var bookingHeader = factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_WaybillNumber = "CONSIGN1";
			var item = consignment.Items.AddNew();
			item.HVI_ShipperReference = "HVI001";
			item.HVI_ManifestedVolume = 0;
			factory.Save();

			var subShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipment.DataContext = DataContextFactory.New();
			subShipment.DataContext.AddDataTarget(DataContextType.HVLVConsignment, "CONSIGN1");
			subShipment.TotalVolumeUnit = new UnitOfVolume { Code = Volume.CubicMetres };

			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ManifestedVolume = 0,
				Height = 3m,
				Length = 4m,
				Width = 5m,
				OrderReference = "HVI001"
			};

			subShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });

			var itemFromUXML = new HVLVItemDataObjectReader(subShipment, consignment, packingLine, new DummyLogger(), Factory).ReadIntoBusinessObject();
			itemFromUXML.Factory.Save();

			var newFactory = new BusinessObjectFactory();
			item = newFactory.Load<HVLVItem>(item.PK);

			AssertEquals("Item HVI_ManifestedVolume is changed to 60", (ZDecimal)60, item.HVI_ManifestedVolume);
		}

		public void TestWhenHVI_ManifestedVolumeIsNotZero_ThenHVI_ManifestedVolumeRemainsTheSame()
		{
			var factory = new BusinessObjectFactory();

			var bookingHeader = factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_WaybillNumber = "CONSIGN1";
			var item = consignment.Items.AddNew();
			item.HVI_ShipperReference = "HVI001";
			item.HVI_ManifestedVolume = 10;
			factory.Save();

			var subShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipment.DataContext = DataContextFactory.New();
			subShipment.DataContext.AddDataTarget(DataContextType.HVLVConsignment, "CONSIGN1");
			subShipment.TotalVolumeUnit = new UnitOfVolume { Code = Volume.CubicMetres };

			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ManifestedVolume = 10,
				Height = 3m,
				Length = 4m,
				Width = 5m,
				OrderReference = "HVI001"
			};

			subShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });

			var itemFromUXML = new HVLVItemDataObjectReader(subShipment, consignment, packingLine, new DummyLogger(), Factory).ReadIntoBusinessObject();
			itemFromUXML.Factory.Save();

			var newFactory = new BusinessObjectFactory();
			item = newFactory.Load<HVLVItem>(item.PK);

			AssertEquals("Item HVI_ManifestedVolume remains 10", (ZDecimal)10, item.HVI_ManifestedVolume);
		}

		public void TestWhenHVI_ActualVolumeIsZero_ThenHVI_ActualVolumeIsCalculated()
		{
			var factory = new BusinessObjectFactory();

			var bookingHeader = factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_WaybillNumber = "CONSIGN1";
			var item = consignment.Items.AddNew();
			item.HVI_ShipperReference = "HVI001";
			item.HVI_ActualVolume = 0;
			factory.Save();

			var subShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipment.DataContext = DataContextFactory.New();
			subShipment.DataContext.AddDataTarget(DataContextType.HVLVConsignment, "CONSIGN1");
			subShipment.TotalVolumeUnit = new UnitOfVolume { Code = Volume.CubicMetres };

			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Volume = 0,
				Height = 3m,
				Length = 4m,
				Width = 5m,
				OrderReference = "HVI001"
			};

			subShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });

			var itemFromUXML = new HVLVItemDataObjectReader(subShipment, consignment, packingLine, new DummyLogger(), Factory).ReadIntoBusinessObject();
			itemFromUXML.Factory.Save();

			var newFactory = new BusinessObjectFactory();
			item = newFactory.Load<HVLVItem>(item.PK);

			AssertEquals("Item HVI_ActualVolume is changed to 60", (ZDecimal)60, item.HVI_ActualVolume);
		}

		public void TestWhenHVI_ActualVolumeIsNotZero_ThenHVI_ActualVolumeRemainsTheSame()
		{
			var factory = new BusinessObjectFactory();

			var bookingHeader = factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_WaybillNumber = "CONSIGN1";
			var item = consignment.Items.AddNew();
			item.HVI_ShipperReference = "HVI001";
			item.HVI_ActualVolume = 20;
			factory.Save();

			var subShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipment.DataContext = DataContextFactory.New();
			subShipment.DataContext.AddDataTarget(DataContextType.HVLVConsignment, "CONSIGN1");
			subShipment.TotalVolumeUnit = new UnitOfVolume { Code = Volume.CubicMetres };

			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Volume = 20,
				Height = 3m,
				Length = 4m,
				Width = 5m,
				OrderReference = "HVI001"
			};

			subShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });

			var itemFromUXML = new HVLVItemDataObjectReader(subShipment, consignment, packingLine, new DummyLogger(), Factory).ReadIntoBusinessObject();
			itemFromUXML.Factory.Save();

			var newFactory = new BusinessObjectFactory();
			item = newFactory.Load<HVLVItem>(item.PK);

			AssertEquals("Item HVI_ActualVolume remains 20", (ZDecimal)20, item.HVI_ActualVolume);
		}

		public void TestGivenCompletePackingLineCollection_WhenReadingHVLVItemLines_ThenOverwriteAllExistingHVLVItemLines()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_WaybillNumber = "CONSIGN1";

			var item = consignment.Items.AddNew();
			item.HVI_ShipperReference = "HVI001";

			var line1 = item.Lines.AddNew();
			line1.HVS_GoodsDescription = "Item Line Description 1";
			line1.HVS_Quantity = 3;

			var line2 = item.Lines.AddNew();
			line2.HVS_GoodsDescription = "Item Line Description 2";
			line2.HVS_Quantity = 3;

			Factory.SaveForTesting();

			var subShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipment.DataContext = DataContextFactory.New();
			subShipment.DataContext.AddDataTarget(DataContextType.HVLVConsignment, "CONSIGN1");

			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Volume = 20,
				Height = 3m,
				Length = 4m,
				Width = 5m,
				OrderReference = "HVI001"
			};

			packingLine.SetPackedItemCollection(() =>
			{
				var list = new List<PackedItem>();

				list.Add(new PackedItem()
				{
					Description = "Packed Item 1",
					GoodsValue = 100,
					GrossWeight = 1,
					GrossWeightUnit = new UnitOfWeight() { Code = "KG" },
					PackedQuantity = 10,
				});

				return list;
			});

			subShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });
			subShipment.PackingLineCollection.Content = CollectionContent.Complete;

			var itemFromUXML = new HVLVItemDataObjectReader(subShipment, consignment, packingLine, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			item = Factory.LoadTop1<HVLVItem>(new ZQuery(HVLVItemSchema.HVI_ShipperReference, "HVI001"));

			var lineUXML = item.Lines.Cast<HVLVItemLine>().FirstOrDefault();

			AssertEquals("Expected item to have only one item line, and the two item lines to be removed since PackingLineCollection has XML attribute Content='Complete'", 1, item.Lines.Count);

			CombineAssertions("Expected item's previous collection of item lines to be overwritten with the item line in the UXML", () =>
			{
				AssertEquals("Packed Item 1", lineUXML.HVS_GoodsDescription);
				AssertEquals((ZDecimal)100, lineUXML.HVS_IntrinsicValue);
				AssertEquals((ZDecimal)1, lineUXML.HVS_GrossWeight);
				AssertEquals("KG", lineUXML.HVS_WeightUnit);
				AssertEquals((ZShort)10, lineUXML.HVS_Quantity);
			});
		}

		public void TestGivenPartialPackingLineCollection_WhenReadingHVLVItemLines_ThenOverwriteAllExistingHVLVItemLines()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_WaybillNumber = "CONSIGN1";

			var item = consignment.Items.AddNew();
			item.HVI_ShipperReference = "HVI001";

			var line1 = item.Lines.AddNew();
			line1.HVS_GoodsDescription = "Item Line Description 1";
			line1.HVS_Quantity = 3;

			var line2 = item.Lines.AddNew();
			line2.HVS_GoodsDescription = "Item Line Description 2";
			line2.HVS_Quantity = 3;

			Factory.SaveForTesting();

			var subShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipment.DataContext = DataContextFactory.New();
			subShipment.DataContext.AddDataTarget(DataContextType.HVLVConsignment, "CONSIGN1");

			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Volume = 20,
				Height = 3m,
				Length = 4m,
				Width = 5m,
				OrderReference = "HVI001"
			};

			packingLine.SetPackedItemCollection(() =>
			{
				var list = new List<PackedItem>();

				list.Add(new PackedItem()
				{
					Description = "Packed Item 1",
					GoodsValue = 100,
					GrossWeight = 1,
					GrossWeightUnit = new UnitOfWeight() { Code = "KG" },
					PackedQuantity = 10,
				});

				return list;
			});

			subShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });
			subShipment.PackingLineCollection.Content = CollectionContent.Partial;

			var itemFromUXML = new HVLVItemDataObjectReader(subShipment, consignment, packingLine, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			item = Factory.LoadTop1<HVLVItem>(new ZQuery(HVLVItemSchema.HVI_ShipperReference, "HVI001"));

			var lineUXML = item.Lines.Cast<HVLVItemLine>().FirstOrDefault();

			AssertEquals("Expected item to have only one item line, and the two item lines to be removed since PackingLineCollection has XML attribute Content='Partial' and PackedItemCollection is not empty", 1, item.Lines.Count);

			CombineAssertions("Expected item's previous collection of item lines to be overwritten with the item line in the UXML", () =>
			{
				AssertEquals("Packed Item 1", lineUXML.HVS_GoodsDescription);
				AssertEquals((ZDecimal)100, lineUXML.HVS_IntrinsicValue);
				AssertEquals((ZDecimal)1, lineUXML.HVS_GrossWeight);
				AssertEquals("KG", lineUXML.HVS_WeightUnit);
				AssertEquals((ZShort)10, lineUXML.HVS_Quantity);
			});
		}

		public void TestGivenPartialPackingLineCollectionAndEmptyPackedItemCollection_WhenReadingHVLVItemLines_ThenDoNotChangeAnyHVLVItemLines()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_WaybillNumber = "CONSIGN1";

			var item = consignment.Items.AddNew();
			item.HVI_ShipperReference = "HVI001";

			var line1 = item.Lines.AddNew();
			line1.HVS_GoodsDescription = "Item Line Description 1";
			line1.HVS_IntrinsicValue = 10;
			line1.HVS_GrossWeight = 15;
			line1.HVS_WeightUnit = "KG";
			line1.HVS_Quantity = 3;

			var line2 = item.Lines.AddNew();
			line2.HVS_GoodsDescription = "Item Line Description 2";
			line2.HVS_IntrinsicValue = 20;
			line2.HVS_GrossWeight = 25;
			line2.HVS_WeightUnit = "KG";
			line2.HVS_Quantity = 7;

			Factory.SaveForTesting();

			var subShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipment.DataContext = DataContextFactory.New();
			subShipment.DataContext.AddDataTarget(DataContextType.HVLVConsignment, "CONSIGN1");

			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Volume = 20,
				Height = 3m,
				Length = 4m,
				Width = 5m,
				OrderReference = "HVI001"
			};

			packingLine.SetPackedItemCollection(() =>
			{
				var list = new List<PackedItem>();

				return list;
			});

			subShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });
			subShipment.PackingLineCollection.Content = CollectionContent.Partial;

			var itemFromUXML = new HVLVItemDataObjectReader(subShipment, consignment, packingLine, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			item = Factory.Load<HVLVItem>(item.PK);

			var lineUXML1 = item.Lines.Cast<HVLVItemLine>().FirstOrDefault(l => l.HVS_GoodsDescription == "Item Line Description 1");
			var lineUXML2 = item.Lines.Cast<HVLVItemLine>().FirstOrDefault(l => l.HVS_GoodsDescription == "Item Line Description 2");

			AssertEquals("Expected item to have two item lines, we do not touch HVLVItemLines when PackingLineCollection has XML attribute Content='Partial' and PackedItemCollction is empty", 2, item.Lines.Count);

			CombineAssertions("Expected item's previous collection of item lines to be overwritten with the item line in the UXML", () =>
			{
				AssertEquals("lineUXML1.HVS_GoodsDescription:", "Item Line Description 1", lineUXML1.HVS_GoodsDescription);
				AssertEquals("lineUXML1.HVS_IntrinsicValue:", (ZDecimal)10, lineUXML1.HVS_IntrinsicValue);
				AssertEquals("lineUXML1.HVS_GrossWeight:", (ZDecimal)15, lineUXML1.HVS_GrossWeight);
				AssertEquals("lineUXML1.HVS_WeightUnit:", "KG", lineUXML1.HVS_WeightUnit);
				AssertEquals("lineUXML1.HVS_Quantity", (ZShort)3, lineUXML1.HVS_Quantity);

				AssertEquals("lineUXML2.HVS_GoodsDescription:", "Item Line Description 2", lineUXML2.HVS_GoodsDescription);
				AssertEquals("lineUXML2.HVS_IntrinsicValue:", (ZDecimal)20, lineUXML2.HVS_IntrinsicValue);
				AssertEquals("lineUXML2.HVS_GrossWeight:", (ZDecimal)25, lineUXML2.HVS_GrossWeight);
				AssertEquals("lineUXML2.HVS_WeightUnit:", "KG", lineUXML2.HVS_WeightUnit);
				AssertEquals("lineUXML2.HVS_Quantity:", (ZShort)7, lineUXML2.HVS_Quantity);
			});
		}

		public void TestWhenReadingXUS_GetHVS_OriginGoodsDescriptionFromCommercialInvoiceLineLocalDescription()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_WaybillNumber = "CONSIGN1";

			Factory.SaveForTesting();

			var subShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipment.DataContext = DataContextFactory.New();
			subShipment.DataContext.AddDataTarget(DataContextType.HVLVConsignment, "CONSIGN1");

			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				OrderReference = "HVI001"
			};
			packingLine.SetPackedItemCollection(() =>
			{
				var list = new List<PackedItem>();
				list.Add(new PackedItem()
				{
					Description = "WiseTech",
					PackedQuantity = 13,
					CommercialInvoiceLineLink = 1
				});
				return list;
			});

			subShipment.SetAddInfoCollection(() => new List<AddInfo>() { AddInfo.New("IsGSTPrePaid", "Y") });

			subShipment.CommercialInfo = new CommercialInfo()
			{
				CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>()
					{
						new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>()
							{
								new CommercialInvoiceLine()
								{
									HarmonisedCode = "654321",
									LocalDescription = "慧咨",
									Link = 1,
								}
							}))
					}
			};

			subShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });
			subShipment.PackingLineCollection.Content = CollectionContent.Complete;

			var itemFromUXML = new HVLVItemDataObjectReader(subShipment, consignment, packingLine, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var item = Factory.LoadTop1<HVLVItem>(new ZQuery(HVLVItemSchema.HVI_ShipperReference, "HVI001"));

			var lineUXML = item.Lines.Cast<HVLVItemLine>().FirstOrDefault();
			AssertEquals("lineUXML.HVS_GoodsDescription:", "WiseTech", lineUXML.HVS_GoodsDescription);
			AssertEquals("lineUXML.HVS_OriginGoodsDescription:", "慧咨", lineUXML.HVS_OriginGoodsDescription);
		}

		public void TestReadItemLineWithBadTariffData_ThrowError()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_WaybillNumber = "CONSIGN1";

			var billToParty = Factory.NewWithValidTestData<OrgHeader>();
			billToParty.OH_Code = "BILLTOPARTY";
			var billToPartyContact = billToParty.Contacts.AddNew();
			billToPartyContact.OC_ContactName = "LARRY";

			Factory.SaveForTesting();

			var subShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipment.DataContext = DataContextFactory.New();
			subShipment.DataContext.AddDataTarget(DataContextType.HVLVConsignment, "CONSIGN1");

			subShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress { AddressType = AddressTypes.SendersLocalClient, OrganizationCode = "BILLTOPARTY", Contact = "LARRY" }
			});

			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ManifestedVolume = 0,
				Height = 3m,
				Length = 4m,
				Width = 5m,
				OrderReference = "HVI001"
			};

			packingLine.SetPackedItemCollection(() =>
			{
				var list = new List<PackedItem>();

				list.Add(new PackedItem()
				{
					Description = "Packed Item 1",
					GoodsValue = 100,
					GrossWeight = 1,
					GrossWeightUnit = new UnitOfWeight() { Code = "KG" },
					PackedQuantity = 10,
					CommercialInvoiceLineLink = 1
				});

				return list;
			});

			subShipment.SetAddInfoCollection(() => new List<AddInfo>() { AddInfo.New("IsGSTPrePaid", "Y") });

			subShipment.CommercialInfo = new CommercialInfo()
			{
				CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>()
				{
					new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
					.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>()
						{
							new CommercialInvoiceLine()
							{
								HarmonisedCode = "654321",
								LocalDescription = "慧咨",
								Link = 1,
								CustomsSupportingInformationCollection = new List<CustomsSupportingInformation>()
								{
									new CustomsSupportingInformation()
									{
										Tariff = "807410"
									}
								}
							}
						}))
				}
			};

			subShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });
			subShipment.PackingLineCollection.Content = CollectionContent.Complete;

			var newFactory = new UniversalObjectFactory();
			var reader = new HVLVItemDataObjectReader(subShipment, consignment, packingLine, new DummyLogger(), Factory);

			var expectedErrorMessage = "There is an HVLVItemLine with Origin HS Code: 807410 but no Origin Country";
			AssertExceptionThrown<DataObjectReadFailureException>("Should throw exception", expectedErrorMessage, () => { reader.ReadIntoBusinessObject(); });
		}

		public void TestReadHVIUsageTypeForUnitedStateDestinationWhenShipmentCountryIsNull_ResultValueShouldBeP()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var consolBO = Factory.NewWithValidTestData<ForwardingConsol>();
				Factory.SaveForTesting();

				var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentDataObject.ShipmentType = new CodeDescriptionPair() { Code = "HVL" };
				shipmentDataObject.PortOfDestination = new UNLOCO() { Code = "US237", Name = "Great Mills" };

				var subShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					ManifestedVolume = 0,
					Height = 3m,
					Length = 4m,
					Width = 5m,
					OrderReference = "HVI001"
				};

				subShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });

				shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
				shipmentDataObject.SubShipmentCollection.Add(subShipment);

				var newFactory = new UniversalObjectFactory();
				var loadedConsol = newFactory.BOFactory.Load<ForwardingConsol>(consolBO.PK);
				var reader = new ShipmentDataObjectReader(shipmentDataObject, new DummyLogger(), newFactory, ChildShipmentsParent.ToChildShipmentsParent(loadedConsol));
				reader.ReadIntoBusinessObject();
				newFactory.SaveForTesting();

				var importedIShipments = Factory.Load<HVLVItem>(new ZQuery());

				AssertEquals("1 item has been imported", 1, importedIShipments.Length);
				AssertEquals("Imported item should have usage type Plus", "P", importedIShipments[0].HVI_UsageType);
			}
		}

		public void TestReadHVIUsageTypeForNonUnitedStateDestinationWhenShipmentCountryIsNull_ResultValueShouldBeS()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var consolBO = Factory.NewWithValidTestData<ForwardingConsol>();
				Factory.SaveForTesting();

				var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentDataObject.ShipmentType = new CodeDescriptionPair() { Code = "HVL" };
				shipmentDataObject.PortOfDestination = new UNLOCO() { Code = "ADALV", Name = "Andorra la Vella" };

				var subShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

				var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					ManifestedVolume = 0,
					Height = 3m,
					Length = 4m,
					Width = 5m,
					OrderReference = "HVI001"
				};

				subShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });

				shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
				shipmentDataObject.SubShipmentCollection.Add(subShipment);

				var newFactory = new UniversalObjectFactory();
				var loadedConsol = newFactory.BOFactory.Load<ForwardingConsol>(consolBO.PK);
				var reader = new ShipmentDataObjectReader(shipmentDataObject, new DummyLogger(), newFactory, ChildShipmentsParent.ToChildShipmentsParent(loadedConsol));
				reader.ReadIntoBusinessObject();
				newFactory.SaveForTesting();

				var importedIShipments = Factory.Load<HVLVItem>(new ZQuery());

				AssertEquals("1 item has been imported", 1, importedIShipments.Length);
				AssertEquals("Imported item should have usage type Standard", "S", importedIShipments[0].HVI_UsageType);
			}
		}

		public void TestWhenPackedQuantityOfPackedItemHasInvalidValue_ThenThrowError()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_WaybillNumber = "CONSIGN1";

			var item = consignment.Items.AddNew();
			item.HVI_ShipperReference = "HVI001";

			var itemLine = item.Lines.AddNew();
			itemLine.HVS_GoodsDescription = "Item Line Description 1";
			itemLine.HVS_IntrinsicValue = 10;
			itemLine.HVS_GrossWeight = 15;
			itemLine.HVS_WeightUnit = "KG";
			itemLine.HVS_Quantity = 3;

			Factory.SaveForTesting();

			var subShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipment.DataContext = DataContextFactory.New();
			subShipment.DataContext.AddDataTarget(DataContextType.HVLVConsignment, "CONSIGN1");

			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				OrderReference = "HVI001"
			};

			packingLine.SetPackedItemCollection(() =>
			{
				var list = new List<PackedItem>();

				list.Add(new PackedItem()
				{
					Description = "Packed Item 1",
					GoodsValue = 100,
					GrossWeight = 1,
					GrossWeightUnit = new UnitOfWeight() { Code = "KG" },
					PackedQuantity = 0,
				});

				return list;
			});

			subShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });
			subShipment.PackingLineCollection.Content = CollectionContent.Complete;

			var reader = new HVLVItemDataObjectReader(subShipment, consignment, packingLine, new DummyLogger(), Factory);

			var expectedErrorMessage = "0 is an invalid HVLV Item Line quantity, value must be equal to or greater than 1.";
			AssertExceptionThrown<DataObjectReadFailureException>("Should throw exception", expectedErrorMessage, () => { reader.ReadIntoBusinessObject(); });
		}

		public void TestReadHVLVItemLine_PopulateProductCodeSuccessfully()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_WaybillNumber = "HVC0000001";

			var universalShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataTarget(DataContextType.HVLVConsignment, "HVC0000001");

			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				OrderReference = "HVI001"
			};
			packingLine.SetPackedItemCollection(() =>
			{
				return new List<PackedItem>
				{
					new PackedItem()
					{
						Product = new Product()
						{
							Code = "A-657776807"
						}
					}
				};
			});

			universalShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });

			var itemFromUXML = new HVLVItemDataObjectReader(universalShipment, consignment, packingLine, new DummyLogger(), Factory).ReadIntoBusinessObject();
			var item = Factory.LoadTop1<HVLVItem>(new ZQuery(HVLVItemSchema.HVI_ShipperReference, "HVI001"));
			var itemLine = item.Lines.Cast<HVLVItemLine>().First();

			AssertEquals("Item line product code populate successfully", "A-657776807", itemLine.HVS_ProductCode);
		}
	}

	class HVLVItemDataObjectReaderForTest : HVLVItemDataObjectReader
	{
		public HVLVItemDataObjectReaderForTest(UniversalShipment consignmentDataObject, HVLVConsignment consignmentBO, PackingLine dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(consignmentDataObject, consignmentBO, dataObject, logger, factory)
		{
		}

		public new HVLVItem GetNewBusinessObject() => base.GetNewBusinessObject();
	}
}

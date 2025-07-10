using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.eTail.Business;
using Enterprise.eTail.Business.Testing;
using Enterprise.eTail.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using static Enterprise.Core.Constants;
using UniversalIncoTerm = Enterprise.UniversalDataBuss.DataObjects.Universal.IncoTerm;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.eTail.DataTransfer.Testing
{
	public class HVLVBookingHeaderDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		[UseSnapshotProtection(true)]
		public void TestReadJobDocsAndCartageFromDataObject()
		{
			var billToParty = Factory.NewWithValidTestData<OrgHeader>();
			billToParty.OH_Code = "BILLTOPARTY";
			Factory.SaveForTesting();

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.IsLastMileDeliverySelfBooked = true;
			shipment.ServiceLevel = new ServiceLevel { Code = "STD" };
			shipment.BookingConfirmationReference = Freight.Integration.ShipmentStatusList.Codes.Booked;

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress { AddressType = AddressTypes.SendersLocalClient, OrganizationCode = "BILLTOPARTY", Contact = "LARRY" }
			});

			shipment.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.LocalProcessing.PickupRequiredBy = new ZDateTime(2004, 12, 10);
			shipment.LocalProcessing.PickupRequiredFrom = new ZDateTime(2004, 12, 1);
			shipment.LocalProcessing.PickupCartageAdvised = new ZDateTime(2004, 12, 12);
			shipment.LocalProcessing.EstimatedPickup = new ZDateTime(2004, 12, 10);
			shipment.LocalProcessing.PickupCartageCompleted = new ZDateTime(2005, 1, 10);

			var bookingHeader = new HVLVBookingHeaderDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var newFactory = new BusinessObjectFactory();
			bookingHeader = newFactory.Load<HVLVBookingHeader>(bookingHeader.PK);

			AssertEquals("M00000001", bookingHeader.HVH_BookingReference);
			AssertEquals(new ZDateTime(2004, 12, 10), bookingHeader.DocsAndCartage.JP_PickupRequiredBy);
			AssertEquals(new ZDateTime(2004, 12, 1), bookingHeader.DocsAndCartage.JP_PickupRequiredFrom);
			AssertEquals(new ZDateTime(2004, 12, 12), bookingHeader.DocsAndCartage.JP_PickupCartageAdvised);
			AssertEquals(new ZDateTime(2004, 12, 10), bookingHeader.DocsAndCartage.JP_EstimatedPickup);
			AssertEquals(new ZDateTime(2005, 1, 10), bookingHeader.DocsAndCartage.JP_PickupCartageCompleted);
		}

		[UseSnapshotProtection(true)]
		public void TestReadFromDataObject()
		{
			var factory = new BusinessObjectFactory();
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				#region Setup

				#region Organizations

				HVLVTestHelper.SetGS1FountainOnOrgProxy(factory, "1234567");

				var billToParty = factory.NewWithValidTestData<OrgHeader>();
				billToParty.OH_Code = "BILLTOPARTY";
				var billToPartyContact = billToParty.Contacts.AddNew();
				billToPartyContact.OC_ContactName = "LARRY";

				var dispatchOrg = factory.NewWithValidTestData<OrgHeader>();
				dispatchOrg.OH_Code = "DISPATCHORG";

				var freightAgent = factory.NewWithValidTestData<OrgHeader>();
				freightAgent.OH_Code = "FREIGHTAGENT";

				var bookingParty = factory.NewWithValidTestData<OrgHeader>();
				bookingParty.OH_Code = "BOOKINGPARTY";
				var bookingPartyContact = bookingParty.Contacts.AddNew();
				bookingPartyContact.OC_ContactName = "FRANK";

				var originDepot = factory.NewWithValidTestData<OrgHeader>();
				originDepot.OH_Code = "ORIGINDEPOT";

				var destinationDepot1 = factory.NewWithValidTestData<OrgHeader>();
				destinationDepot1.OH_Code = "DESTDEPOT1";

				var destinationDepot2 = factory.NewWithValidTestData<OrgHeader>();
				destinationDepot2.OH_Code = "DESTDEPOT2";

				var lastMileDelivery = factory.NewWithValidTestData<OrgHeader>();
				lastMileDelivery.OH_Code = "LCLDLVRY";
				lastMileDelivery.OH_FullName = "Local Delivery";

				var lastMileCarrierBookingAgent = factory.NewWithValidTestData<OrgHeader>();
				lastMileCarrierBookingAgent.OH_Code = "BKGAGT1";
				lastMileCarrierBookingAgent.OH_FullName = "BookingAgent1";

				var jobDeclaration1 = factory.NewWithValidTestData<BaseJobDeclaration>();
				jobDeclaration1.JE_DeclarationReference = "InnocuousWhitePowder";

				var jobDeclaration2 = factory.NewWithValidTestData<BaseJobDeclaration>();
				jobDeclaration2.JE_DeclarationReference = "HaltInTheNameOfTheLaw";

				factory.Save();

				#endregion

				var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipment.IsLastMileDeliverySelfBooked = true;
				shipment.ServiceLevel = new ServiceLevel { Code = "STD" };
				shipment.BookingConfirmationReference = Freight.Integration.ShipmentStatusList.Codes.Booked;

				shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress { AddressType = AddressTypes.SendersLocalClient, OrganizationCode = "BILLTOPARTY", Contact = "LARRY" },
					new OrganizationAddress { AddressType = nameof(DocAddressType.ConsignorPickupDeliveryAddress), OrganizationCode = "DISPATCHORG" },
					new OrganizationAddress { AddressType = nameof(DocAddressType.ExportBroker), OrganizationCode = "FREIGHTAGENT" },
					new OrganizationAddress { AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress), OrganizationCode = "BOOKINGPARTY", Contact = "FRANK" },
					new OrganizationAddress { AddressType = nameof(DocAddressType.DepartureCFSAddress), OrganizationCode = "ORIGINDEPOT" }
				});

				var subShipment1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					WayBillNumber = "ABC4747",
					OwnerRef = "XYZ456",
					GoodsValue = 50,
					GoodsValueCurrency = new Currency { Code = "USD" },
					ManifestedWeight = 10,
					TotalWeight = 10,
					TotalWeightUnit = new UnitOfWeight { Code = Weight.Kilograms },
					ManifestedVolume = 0.5,
					TotalVolume = 1,
					TotalVolumeUnit = new UnitOfVolume { Code = Volume.CubicMetres },
					GoodsDescription = "Biscuits",
					IsHazardous = false,
					IsSignatureRequired = false,
					IsAuthorizedToLeave = false,
					IsTracked = false,
					VendorIdentifier = "VID0001",
					CarrierAccount = new CarrierAccount() { AccountNumber = "123456" },
					CarrierServiceLevel = new ServiceLevel() { Code = "EXP" },
					ShipmentIncoTerm = new UniversalIncoTerm() { Code = IncoTerms.FreeOnBoard },
					ServiceLevel = new ServiceLevel() { Code = "STD" }
				};

				subShipment1.SetInstructionCollection(() => new DataObjectList<Instruction>
				{
					new Instruction { ServiceInstruction = "Leave at back" }
				});

				subShipment1.SetCustomsReferenceCollection(() => new List<CustomsReference>()
				{
					new CustomsReference()
					{
						Type = new CodeDescriptionPair { Code = nameof(DataContext.Declaration) },
						SubType = new CodeDescriptionPair35Char { Code = FreightShipmentDirection.Code.Import },
						Reference = "InnocuousWhitePowder"
					}
				});

				subShipment1.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress { AddressType = nameof(DocAddressType.ArrivalCFSAddress), OrganizationCode = "DESTDEPOT1" },
					new OrganizationAddress { AddressType = nameof(DocAddressType.CarrierBookingAgent), OrganizationCode = "BKGAGT1" },
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = nameof(DocAddressType.ConsigneeDocumentaryAddress),
						CompanyName = "Bobs Company",
						Address1 = "12 Something St",
						City = "Sydney",
						State = "NSW",
						Postcode = "2000",
						Country = new Country { Code = "AU" },
						Contact = "Bob",
						Email = "bob@bob.com"
					},
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = nameof(DocAddressType.ConsignorDocumentaryAddress),
						CompanyName = "Another Company",
						Address1 = "23 Another St",
						City = "Melbourne",
						State = "VIC",
						Postcode = "3000",
						Country = new Country { Code = "NZ" },
						Contact = "Vic",
						Email = "victor@fakedomain.com"
					},
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = AddressTypes.DeliveryLocalCartage,
						OrganizationCode = "LCLDLVRY",
						CompanyName = "Local Delivery",
					}
				});

				var packingLine1_1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					OrderReference = "GHI44999",
					Barcode = "12345678901234567890123456789012345",
					ManifestedWeight = 10,
					Weight = 10,
					ManifestedVolume = 0.5,
					Volume = 1,
					ReferenceNumber = "X0012931292",
					PackType = new PackageType { Code = "PKG" },
					RequiresFumigationCertificate = true,
					IsPersonalEffects = true,
					IsTimber = true,
					IsPerishable = true,
					ContainerNumber = "CTNR123456",
					Height = 3m,
					Length = 4m,
					Width = 5m,
					LengthUnit = new UnitOfLength { Code = Length.Metres, Description = Length.GetDescription(Length.Metres, PluralState.Plural) },
					GoodsDescription = "Foods"
				};
				packingLine1_1.SetPackedItemCollection(() =>
				{
					var list = new List<PackedItem>();
					list.Add(new PackedItem()
					{
						CIFValue = 12.345,
						GoodsValue = 54.321,
						GrossWeight = 1.23,
						GrossWeightUnit = new UnitOfWeight() { Code = "KG" },
						NetWeight = 3.21,
						NetWeightUnit = new UnitOfWeight() { Code = "OZ" },
						Product = new Product() { Code = "STUFF1" },
						Description = "Stuff No.1",
						PackedQuantity = 13,
						ItemSpecificationUrl = "http://www.google.com/nicestuff",
						CommercialInvoiceLineLink = 1
					});
					return list;
				});

				subShipment1.SetAddInfoCollection(() => new List<AddInfo>() { AddInfo.New("IsGSTPrePaid", "Y") });

				subShipment1.CommercialInfo = new CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>()
					{
						new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>()
							{
								new CommercialInvoiceLine()
								{
									HarmonisedCode = "654321",
									Link = 1,
									CustomsSupportingInformationCollection = new List<CustomsSupportingInformation>()
									{
										new CustomsSupportingInformation()
										{
											Tariff = "123456",
											Country = new Country() { Code = "CN" }
										}
									}
								}
							}))
					}
				};

				subShipment1.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine1_1 });
				subShipment1.PackingLineCollection.Content = CollectionContent.Complete;

				var subShipment2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					OwnerRef = "ABC123",
					GoodsValue = 200,
					GoodsValueCurrency = new Currency { Code = "AUD" },
					ManifestedWeight = 15,
					TotalWeight = 20,
					TotalWeightUnit = new UnitOfWeight { Code = Weight.Kilograms },
					ManifestedVolume = 1,
					TotalVolume = 1,
					TotalVolumeUnit = new UnitOfVolume { Code = Volume.CubicMetres },
					GoodsDescription = "Explosives",
					IsHazardous = true,
					IsSignatureRequired = true,
					IsAuthorizedToLeave = true,
					IsTracked = true,
					ShipmentIncoTerm = new UniversalIncoTerm() { Code = IncoTerms.DeliveredAtPlace },
					ServiceLevel = new ServiceLevel() { Code = "D2D" }
				};

				subShipment2.SetAddInfoCollection(() => new List<AddInfo>() { AddInfo.New("IsGSTPrePaid", "N") });

				subShipment2.SetInstructionCollection(() => new DataObjectList<Instruction>
				{
					new Instruction { ServiceInstruction = "Leave at front" }
				});

				subShipment2.SetCustomsReferenceCollection(() => new List<CustomsReference>()
				{
					new CustomsReference()
					{
						Type = new CodeDescriptionPair { Code = nameof(DataContext.Declaration) },
						SubType = new CodeDescriptionPair35Char { Code = FreightShipmentDirection.Code.Export },
						Reference = "HaltInTheNameOfTheLaw"
					}
				});

				subShipment2.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>()
				{
					new AdditionalReference()
					{
						Type = new EntryType { Code = "ISF", Description = "Importer Security Filing" },
						ReferenceNumber = "ISF00000001"
					}
				});

				subShipment2.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress { AddressType = nameof(DocAddressType.ArrivalCFSAddress), OrganizationCode = "DESTDEPOT2" },
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = nameof(DocAddressType.ConsigneeDocumentaryAddress),
						CompanyName = "Murray",
						Address1 = "99 Consignee Road",
						Address2 = "Downtown",
						City = "New York",
						State = "NY",
						Postcode = "12345",
						Country = new Country { Code = "US" },
						Contact = "Moo ray",
						Email = "murray.hewitt@usconsulate.gov.nz",
						Phone = "7",
						Mobile = "+1234567890",
						Fax = "+0987654321"
					},
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = nameof(DocAddressType.ConsignorDocumentaryAddress),
						CompanyName = "Some Company",
						Address1 = "22 Shipper Street",
						City = "Wellington",
						State = "WLG",
						Postcode = "54321",
						Country = new Country { Code = "AU" },
						Contact = "Randy",
						Email = "randy@randysdomain.com",
						Phone = "01189998819991197253",
						Mobile = "+555 5555",
						Fax = "8"
					}
				});

				var packingLine2_1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					ManifestedWeight = 5,
					Weight = 10,
					ManifestedVolume = 0.5,
					Volume = 0.5,
					ReferenceNumber = "D9901239028",
					PackType = new PackageType { Code = "BOX" },
					RequiresFumigationCertificate = false,
					IsPersonalEffects = false,
					IsTimber = false,
					IsPerishable = false,
				};
				packingLine2_1.SetUNDGCollection(() => new List<UNDG> { new UNDG(DefaultDataObjectWriterStrategy.TestInstance) { IMOClass = "1.1D" } });

				var packingLine2_2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					ManifestedWeight = 10,
					Weight = 10,
					ManifestedVolume = 0.5,
					Volume = 0.5,
					ReferenceNumber = "D5465421481",
					PackType = new PackageType { Code = "BOX" },
					RequiresFumigationCertificate = false,
					IsPersonalEffects = false,
					IsTimber = false,
					IsPerishable = false,
				};
				packingLine2_2.SetUNDGCollection(() => new List<UNDG> { new UNDG(DefaultDataObjectWriterStrategy.TestInstance) { IMOClass = "1.1D" } });

				subShipment2.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine2_1, packingLine2_2 });
				subShipment2.PackingLineCollection.Content = CollectionContent.Complete;
				shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { subShipment1, subShipment2 });

				#endregion

				var bookingHeader = new HVLVBookingHeaderDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				#region Assertions

				CombineAssertions(() =>
				{
					var newFactory = new BusinessObjectFactory();
					bookingHeader = newFactory.Load<HVLVBookingHeader>(bookingHeader.PK);

					AssertEquals("M00000001", bookingHeader.HVH_BookingReference);
					AssertEquals(true, bookingHeader.HVH_UseShipperDeliveryAccount);
					AssertEquals("STD", bookingHeader.HVH_RS_NKBookingServiceLevel);
					AssertEquals(3, bookingHeader.HVH_ItemCount);
					AssertEquals((ZDecimal)30, bookingHeader.HVH_GrossWeight);
					AssertEquals(Weight.Kilograms, bookingHeader.HVH_GrossWeightUQ);
					AssertEquals((ZDecimal)2, bookingHeader.HVH_GrossVolume);
					AssertEquals(Volume.CubicMetres, bookingHeader.HVH_GrossVolumeUQ);
					AssertEquals(false, bookingHeader.HVH_IsBookingConfirmed);

					AssertEquals(billToParty.PK, bookingHeader.BillToParty.OA_OH);
					AssertEquals(billToPartyContact.PK, bookingHeader.HVH_OC_BillToPartyContact);
					AssertEquals(dispatchOrg.PK, bookingHeader.DispatchAddress.OA_OH);
					AssertEquals(freightAgent.PK, bookingHeader.HVH_OH_FreightAgent);
					AssertEquals(bookingPartyContact.PK, bookingHeader.HVH_OC_BookedBy);
					AssertEquals(originDepot.PK, bookingHeader.OriginDepot.OA_OH);

					var consignments = bookingHeader.Consignments.Cast<HVLVConsignment>().ToArray();
					AssertEquals(2, consignments.Length);

					var consignment1 = consignments.Where(x => x.HVC_WaybillNumber == "ABC4747").Single();
					Assert(consignment1.HVC_IsTaxPrePaid);
					AssertEquals("ABC4747", consignment1.HVC_ConsignmentId);
					AssertEquals("XYZ456", consignment1.HVC_ShipperReference);
					AssertEquals((ZShort)1, consignment1.HVC_ItemCount);
					AssertEquals((ZDecimal)50, consignment1.HVC_GoodsValue);
					AssertEquals("USD", consignment1.HVC_RX_NKGoodsValueCurrency);
					AssertEquals(Weight.Kilograms, consignment1.HVC_WeightUQ);
					AssertEquals(Volume.CubicMetres, consignment1.HVC_VolumeUQ);
					AssertEquals("Biscuits", consignment1.HVC_GoodsDescription);
					AssertEquals("Leave at back", consignment1.HVC_ConsigneeInstructions);
					AssertEquals(false, consignment1.HVC_IsHazardous);
					AssertEquals(false, consignment1.HVC_IsSignatureRequired);
					AssertEquals(false, consignment1.HVC_AuthorityToLeave);
					AssertEquals(false, consignment1.HVC_IsTracked);
					AssertEquals(true, consignment1.HVC_RequiresFumigation);
					AssertEquals(true, consignment1.HVC_IsPersonalEffects);
					AssertEquals(true, consignment1.HVC_IsTimber);
					AssertEquals(true, consignment1.HVC_IsPerishable);
					AssertEquals("123456", consignment1.HVC_CarrierAccountNumber);
					AssertEquals("EXP", consignment1.HVC_PL_NKLastMileCarrierServiceLevel);
					AssertEquals(IncoTerms.FreeOnBoard, consignment1.HVC_INCO);
					AssertEquals("STD", consignment1.HVC_RS_NKServiceLevel);

					AssertEquals(destinationDepot1.PK, consignment1.DestinationDepot.OA_OH);
					AssertEquals(lastMileDelivery.PK, consignment1.LastMileCarrier.PK);
					AssertEquals(lastMileCarrierBookingAgent.PK, consignment1.LastMileCarrierBookingAgent.PK);

					AssertEquals((ZShort)1, consignment1.HVC_ItemCount);
					AssertEquals(1, consignment1.Items.Count);
					AssertEquals("InnocuousWhitePowder", consignment1.DeclarationReferenceForDisplay);
					AssertEquals("Chargeable not calculated because it is suspended while XML importing/exporting. Refer to HVLVConsignment.IsBeingImportedFromUniversalXml property", "Not Calculated", consignment1.ChargeableForDisplay);

					var item1 = consignment1.Items[0];
					AssertEquals("GHI44999", item1.HVI_ShipperReference);
					AssertEquals("12345678901234567890123456789012345", item1.HVI_CurrentBarcode);
					AssertEquals("12345678901234567890123456789012345", item1.HVI_ItemId);
					AssertEquals("PKG", item1.HVI_F3_NKPackType);
					AssertEquals((ZDecimal)1.23, item1.HVI_ManifestedWeight);
					AssertEquals((ZDecimal)10, item1.HVI_ActualWeight);
					AssertEquals((ZDecimal)0.5, item1.HVI_ManifestedVolume);
					AssertEquals((ZDecimal)1, item1.HVI_ActualVolume);
					AssertEquals(3m, item1.HVI_Height);
					AssertEquals(4m, item1.HVI_Length);
					AssertEquals(5m, item1.HVI_Width);
					AssertEquals(Length.Metres, item1.HVI_UnitOfDimension);
					AssertEquals("Foods", item1.HVI_GoodsDescription);
					AssertEquals("CTNR123456", item1.HVI_ContainerNumber);

					AssertEquals("VID0001", consignment1.HVC_VendorIdentifier);

					AssertEquals(1, item1.Lines.Count);
					var hc2 = item1.Lines.OfType<HVLVItemLine>().Single();
					AssertEquals("123456", hc2.HVS_OriginTariff);
					AssertEquals("654321", hc2.HVS_DestinationTariff);
					AssertEquals("CN", hc2.HVS_RN_NKOriginCountryCode);
					AssertEquals(12.345M, hc2.HVS_CustomsValue);
					AssertEquals(54.321M, hc2.HVS_IntrinsicValue);
					AssertEquals(1.23M, hc2.HVS_GrossWeight);
					AssertEquals("KG", hc2.HVS_WeightUnit);
					AssertEquals(3.21M, hc2.HVS_NetWeight);
					AssertEquals(13, hc2.HVS_Quantity.ToZInt());

					var consignment2 = consignments.Where(x => x.HVC_WaybillNumber == "ABC123").Single();
					Assert(!consignment2.HVC_IsTaxPrePaid);
					AssertEquals("ABC123", consignment2.HVC_ConsignmentId);
					AssertEquals("ABC123", consignment2.HVC_ShipperReference);
					AssertEquals((ZShort)2, consignment2.HVC_ItemCount);
					AssertEquals((ZDecimal)200, consignment2.HVC_GoodsValue);
					AssertEquals("AUD", consignment2.HVC_RX_NKGoodsValueCurrency);
					AssertEquals(Weight.Kilograms, consignment2.HVC_WeightUQ);
					AssertEquals(Volume.CubicMetres, consignment2.HVC_VolumeUQ);
					AssertEquals("Explosives", consignment2.HVC_GoodsDescription);
					AssertEquals("Leave at front", consignment2.HVC_ConsigneeInstructions);
					AssertEquals(true, consignment2.HVC_IsHazardous);
					AssertEquals(true, consignment2.HVC_IsSignatureRequired);
					AssertEquals(true, consignment2.HVC_AuthorityToLeave);
					AssertEquals(true, consignment2.HVC_IsTracked);
					AssertEquals(false, consignment2.HVC_RequiresFumigation);
					AssertEquals(false, consignment2.HVC_IsPersonalEffects);
					AssertEquals(false, consignment2.HVC_IsTimber);
					AssertEquals(false, consignment2.HVC_IsPerishable);
					AssertEquals("1.1D", consignment2.HVC_UndgClass);
					AssertEquals(IncoTerms.DeliveredAtPlace, consignment2.HVC_INCO);
					AssertEquals("D2D", consignment2.HVC_RS_NKServiceLevel);

					AssertEquals(destinationDepot2.PK, consignment2.DestinationDepot.OA_OH);
					AssertNull(consignment2.LastMileCarrier);

					AssertEquals((ZShort)2, consignment2.HVC_ItemCount);
					AssertEquals(2, consignment2.Items.Count);
					AssertEquals("HaltInTheNameOfTheLaw", consignment2.DeclarationReferenceForDisplay);

					var referenceNumbers = consignment2.CustomsReferenceNumbers;
					var reference = referenceNumbers.GetAllReferenceNumbersByType("ISF").Single();
					AssertEquals("ISF number is correct", "ISF00000001", reference);

					var item2 = consignment2.Items.Where(x => x.HVI_ItemId == "ABC123").Single();
					var item3 = consignment2.Items.Where(x => x.HVI_ItemId == "012345670000000015").Single();
					AssertEquals("BOX", item2.HVI_F3_NKPackType);
					AssertEquals((ZDecimal)5, item2.HVI_ManifestedWeight);
					AssertEquals((ZDecimal)10, item2.HVI_ActualWeight);
					AssertEquals((ZDecimal)0.5, item2.HVI_ManifestedVolume);
					AssertEquals((ZDecimal)0.5, item2.HVI_ActualVolume);
					AssertEquals(1, item2.UNDGs.Count);
					AssertEquals("1.1D", item2.UNDGs[0].DI_IMOClass);
					AssertEquals("BOX", item3.HVI_F3_NKPackType);
					AssertEquals((ZDecimal)10, item3.HVI_ManifestedWeight);
					AssertEquals((ZDecimal)10, item3.HVI_ActualWeight);
					AssertEquals((ZDecimal)0.5, item3.HVI_ManifestedVolume);
					AssertEquals((ZDecimal)0.5, item3.HVI_ActualVolume);
					AssertEquals(1, item3.UNDGs.Count);
					AssertEquals("1.1D", item3.UNDGs[0].DI_IMOClass);
				});

				#endregion
			}
		}

		public void TestReadFromDataObject_NoMeasurementsOnPackingLine()
		{
			var billToParty = Factory.NewWithValidTestData<OrgHeader>();
			billToParty.OH_Code = "BILLTOPARTY";
			var billToPartyContact = billToParty.Contacts.AddNew();
			billToPartyContact.OC_ContactName = "LARRY";

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress { AddressType = AddressTypes.SendersLocalClient, OrganizationCode = "BILLTOPARTY", Contact = "LARRY" }
			});

			var subShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipment.ManifestedWeight = 20;
			subShipment.TotalWeight = 30;
			subShipment.ManifestedVolume = 3;
			subShipment.TotalVolume = 2;

			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packingLine.PackQty = 1;

			subShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });
			subShipment.PackingLineCollection.Content = CollectionContent.Complete;
			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { subShipment });

			var bookingHeader = new HVLVBookingHeaderDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();

			var item = bookingHeader.Consignments.Cast<HVLVConsignment>().SelectMany(x => x.Items).Cast<HVLVItem>().Single();
			AssertEquals((ZDecimal)20, item.HVI_ManifestedWeight);
			AssertEquals((ZDecimal)30, item.HVI_ActualWeight);
			AssertEquals((ZDecimal)3, item.HVI_ManifestedVolume);
			AssertEquals((ZDecimal)2, item.HVI_ActualVolume);
		}

		public void TestReadFromDataObject_UnableToSetMeasurements()
		{
			var billToParty = Factory.NewWithValidTestData<OrgHeader>();
			billToParty.OH_Code = "BILLTOPARTY";
			var billToPartyContact = billToParty.Contacts.AddNew();
			billToPartyContact.OC_ContactName = "LARRY";

			Factory.SaveForTesting();

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress { AddressType = AddressTypes.SendersLocalClient, OrganizationCode = "BILLTOPARTY", Contact = "LARRY" }
			});

			var subShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipment.ManifestedWeight = 20;
			subShipment.TotalWeight = 30;
			subShipment.ManifestedVolume = 3;
			subShipment.TotalVolume = 2;

			var packingLine1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packingLine1.ReferenceNumber = "ITEM1";
			var packingLine2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packingLine2.ReferenceNumber = "ITEM2";

			subShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine1, packingLine2 });
			subShipment.PackingLineCollection.Content = CollectionContent.Complete;
			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { subShipment });

			var logger = new TestErrorLogger();
			var bookingHeader = new HVLVBookingHeaderDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();

			AssertContains("Warning - HVLV Item (ITEM1): Could not import measurements as they were not specified on the PackingLine, and the Consignment is multi-piece.", logger.Logs);
			AssertContains("Warning - HVLV Item (ITEM2): Could not import measurements as they were not specified on the PackingLine, and the Consignment is multi-piece.", logger.Logs);

			var items = bookingHeader.Consignments.Cast<HVLVConsignment>().SelectMany(x => x.Items).Cast<HVLVItem>().ToArray();
			AssertEquals(2, items.Length);

			foreach (var item in items)
			{
				AssertEquals((ZDecimal)0, item.HVI_ManifestedWeight);
				AssertEquals((ZDecimal)0, item.HVI_ActualWeight);
				AssertEquals((ZDecimal)0, item.HVI_ManifestedVolume);
				AssertEquals((ZDecimal)0, item.HVI_ActualVolume);
			}
		}

		public void TestReadFromDataObject_InvokesNumberAllocation()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				HVLVTestHelper.SetGS1FountainOnOrgProxy(new BusinessObjectFactory(), "1234567");

				var billToParty = Factory.NewWithValidTestData<OrgHeader>();
				billToParty.OH_Code = "BILLTOPARTY";
				var billToPartyContact = billToParty.Contacts.AddNew();
				billToPartyContact.OC_ContactName = "LARRY";

				var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

				shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress { AddressType = AddressTypes.SendersLocalClient, OrganizationCode = "BILLTOPARTY", Contact = "LARRY" }
				});

				var subShipment1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					OwnerRef = "CONSIGN1",
				};
				subShipment1.SetPackingLineCollection(() => new DataObjectList<PackingLine>
					{
						new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ReferenceNumber = "ITEM1" }
					});

				var subShipment2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					OwnerRef = "CONSIGN2",
				};
				subShipment2.SetPackingLineCollection(() => new DataObjectList<PackingLine>
					{
						new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ReferenceNumber = "ITEM1" }
					});

				shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { subShipment1, subShipment2 });

				var bookingHeader = new HVLVBookingHeaderDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
				bookingHeader.Consignments[0].HVC_WaybillNumber = "WAYBILL123"; // to prevent unique-index failure
				Factory.SaveForTesting();

				var itemIDs = bookingHeader.Consignments.Cast<HVLVConsignment>().SelectMany(x => x.Items).Cast<HVLVItem>().Select(x => x.HVI_ItemId);
				AssertContainsExactElementsInAnyOrder(new[] { "WAYBILL123", "CONSIGN2" }, itemIDs);
			}
		}

		public void TestReadFromDataObject_MatchingExistingObjects()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				#region Setup Bizo's

				var creationFactory = new BusinessObjectFactory();

				var existingBookingHeader = creationFactory.New<HVLVBookingHeader>();
				existingBookingHeader.HVH_BookingReference = "HEADERBOOKINGREF";
				existingBookingHeader.HVH_UseShipperDeliveryAccount = true;
				existingBookingHeader.HVH_RS_NKBookingServiceLevel = "STD";

				var billToParty = creationFactory.NewWithValidTestData<OrgHeader>();
				billToParty.OH_Code = "BILLTOPARTY";
				billToParty.MainAddress.Address1 = "45 Bill To Street";
				billToParty.Contacts.AddNew().OC_ContactName = "Bill";

				existingBookingHeader.HVH_OA_BillToParty = billToParty.MainAddress.PK;
				existingBookingHeader.HVH_OC_BillToPartyContact = billToParty.Contacts[0].PK;

				var existingConsignment1 = existingBookingHeader.Consignments.AddNew();
				existingConsignment1.HVC_WaybillNumber = "CONSIGN1";
				existingConsignment1.HVC_GoodsDescription = "Ice-cream";

				var existingItem11 = existingConsignment1.Items.AddNew();
				existingItem11.HVI_CurrentBarcode = "ITEM11";
				existingItem11.HVI_ShipperReference = "SHIPREF11";

				var existingItem12 = existingConsignment1.Items.AddNew();
				existingItem12.HVI_CurrentBarcode = "ITEM12";
				existingItem12.HVI_ShipperReference = "SHIPREF12";

				var existingItemForFallback = existingConsignment1.Items.AddNew();
				existingItemForFallback.HVI_CurrentBarcode = "FALLBACKITEM";

				var existingItemLine11_1 = existingItem11.Lines.AddNew();
				existingItemLine11_1.HVS_OriginTariff = "HC1111";
				existingItemLine11_1.HVS_RN_NKOriginCountryCode = "AU";
				existingItemLine11_1.HVS_CustomsValue = 10m;
				existingItemLine11_1.HVS_DestinationTariff = "TARIFF1111";
				existingItemLine11_1.HVS_ProductCode = "PROD1111";
				existingItemLine11_1.HVS_Quantity = 1;
				existingItemLine11_1.HVS_WeightUnit = "KG";
				existingItemLine11_1.HVS_GoodsDescription = "This is 1111.";
				existingItemLine11_1.HVS_GrossWeight = 2m;
				existingItemLine11_1.HVS_IntrinsicValue = 10m;
				existingItemLine11_1.HVS_ItemURL = "http://www.amazon.com/1111";
				existingItemLine11_1.HVS_NetWeight = 2m;

				var existingItemLine11_2 = existingItem11.Lines.AddNew();
				existingItemLine11_2.HVS_OriginTariff = "HC2222";
				existingItemLine11_2.HVS_RN_NKOriginCountryCode = "FR";
				existingItemLine11_2.HVS_CustomsValue = 10m;
				existingItemLine11_2.HVS_DestinationTariff = "TARIFF2222";
				existingItemLine11_2.HVS_ProductCode = "PROD2222";
				existingItemLine11_2.HVS_Quantity = 1;
				existingItemLine11_2.HVS_WeightUnit = "KG";
				existingItemLine11_2.HVS_GoodsDescription = "This is 2222.";
				existingItemLine11_2.HVS_GrossWeight = 2m;
				existingItemLine11_2.HVS_IntrinsicValue = 10m;
				existingItemLine11_2.HVS_ItemURL = "http://www.amazon.com/2222";
				existingItemLine11_2.HVS_NetWeight = 2m;

				var existingItemLine12_1 = existingItem12.Lines.AddNew();
				existingItemLine12_1.HVS_OriginTariff = "HC3333";
				existingItemLine12_1.HVS_RN_NKOriginCountryCode = "US";
				existingItemLine12_1.HVS_CustomsValue = 10m;
				existingItemLine12_1.HVS_DestinationTariff = "TARIFF3333";
				existingItemLine12_1.HVS_ProductCode = "PROD3333";
				existingItemLine12_1.HVS_Quantity = 1;
				existingItemLine12_1.HVS_WeightUnit = "KG";
				existingItemLine12_1.HVS_GoodsDescription = "This is 3333.";
				existingItemLine12_1.HVS_GrossWeight = 2m;
				existingItemLine12_1.HVS_IntrinsicValue = 10m;
				existingItemLine12_1.HVS_ItemURL = "http://www.amazon.com/3333";
				existingItemLine12_1.HVS_NetWeight = 2m;

				var existingConsignment2 = existingBookingHeader.Consignments.AddNew();
				existingConsignment2.HVC_WaybillNumber = "CONSIGN2";
				existingConsignment2.HVC_GoodsDescription = "Biscuits";

				var existingItem21 = existingConsignment2.Items.AddNew();
				existingItem21.HVI_CurrentBarcode = "ITEM21";
				existingItem21.HVI_ShipperReference = "SHIPREF21";

				var existingConsignment3 = existingBookingHeader.Consignments.AddNew();
				existingConsignment3.HVC_WaybillNumber = "CONSIGN3";
				existingConsignment3.HVC_GoodsDescription = "Surprises";

				var existingItem31 = existingConsignment3.Items.AddNew();
				existingItem31.HVI_CurrentBarcode = "ITEM31";
				existingItem31.HVI_ShipperReference = "SHIPREF31";

				creationFactory.Save();

				CombineAssertions("Prerequisite: consignmentID's defaulted from waybills", () =>
				{
					AssertEquals("CONSIGN1", existingConsignment1.HVC_ConsignmentId);
					AssertEquals("CONSIGN2", existingConsignment2.HVC_ConsignmentId);
					AssertEquals("CONSIGN3", existingConsignment3.HVC_ConsignmentId);
				});

				existingConsignment1.HVC_WaybillNumber = "WAYBILL1";
				existingConsignment2.HVC_WaybillNumber = "WAYBILL2";
				existingConsignment3.HVC_WaybillNumber = "WAYBILL3";

				CombineAssertions("Prerequisite: itemID's defaulted from Shipper References", () =>
				{
					AssertEquals("ITEM11", existingItem11.HVI_ItemId);
					AssertEquals("ITEM12", existingItem12.HVI_ItemId);
					AssertEquals("ITEM21", existingItem21.HVI_ItemId);
					AssertEquals("ITEM31", existingItem31.HVI_ItemId);
					AssertEquals("FALLBACKITEM", existingItemForFallback.HVI_ItemId);
				});
				creationFactory.Save();

				#endregion

				#region Setup UXML

				var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipment.DataContext = DataContextFactory.New();
				shipment.DataContext.AddDataTarget(DataContextType.HVLVBookingHeader, "HEADERBOOKINGREF");

				shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress { AddressType = AddressTypes.SendersLocalClient, OrganizationCode = "BILLTOPARTY", Contact = "LARRY" },
				});

				var subShipment1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					WayBillNumber = "SOMETHING1",
					GoodsDescription = "More Goodness",
				};

				subShipment1.SetPackingLineCollection(() => new DataObjectList<PackingLine>
					{
						new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
						{
							OrderReference = "SHIPREF11",
							GoodsDescription = "Chocolate",
						},

						new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
						{
							OrderReference = "SHIPREF12",
							GoodsDescription = "Strawberry",
						},

						new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
						{
							ReferenceNumber = "FALLBACKITEM",
							GoodsDescription = "Elden Ring",
						}
					});

				subShipment1.PackingLineCollection.Content = CollectionContent.Complete;

				subShipment1.DataContext = DataContextFactory.New();
				subShipment1.DataContext.AddDataTarget(DataContextType.HVLVConsignment, "CONSIGN1");

				var subShipment2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					WayBillNumber = "WAYBILL2",
					GoodsDescription = "More Buscuits",
				};
				subShipment2.SetPackingLineCollection(() => new DataObjectList<PackingLine>
					{
						new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
						{
							OrderReference = "SHIPREF21",
							GoodsDescription = "Apples",
						},

						new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
						{
							OrderReference = "SHIPREF22",
							GoodsDescription = "Oranges",
						}
					});

				subShipment2.PackingLineCollection.Content = CollectionContent.Complete;

				var subShipment3 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					WayBillNumber = "WAYBILL4",
					GoodsDescription = "Books",
				};
				subShipment3.SetPackingLineCollection(() => new DataObjectList<PackingLine>
					{
						new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
						{
							OrderReference = "SHIPREF41",
							GoodsDescription = "1984",
						},
					});

				subShipment3.PackingLineCollection.Content = CollectionContent.Complete;

				shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>
				{
					subShipment1,
					subShipment2,
					subShipment3
				});

				#endregion

				#region Import

				var bookingHeader = new HVLVBookingHeaderDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				CombineAssertions(() =>
				{
					bookingHeader = new BusinessObjectFactory().Load<HVLVBookingHeader>(bookingHeader.PK);
					AssertEquals("Existing booking header was matched by DataTarget Key", existingBookingHeader.PK, bookingHeader.PK);

					var consignments = bookingHeader.Consignments.Cast<HVLVConsignment>().ToArray();
					AssertEquals("Consignments imported", 4, consignments.Length);

					// ONE

					var consignment1 = consignments[0];
					AssertEquals("Existing consignment matched by DataTarget Key == ConsignmentID", existingConsignment1.PK, consignment1.PK);

					AssertEquals("ConsignmentID unchanged", "CONSIGN1", consignment1.HVC_ConsignmentId);

					AssertEquals("Consignment updated from UXML", "SOMETHING1", consignment1.HVC_WaybillNumber);
					AssertEquals("Consignment updated from UXML", "More Goodness", consignment1.HVC_GoodsDescription);

					var items1 = consignment1.Items.Cast<HVLVItem>().ToArray();
					var item11 = items1.Single(item => item.HVI_ShipperReference == "SHIPREF11");
					var item12 = items1.Single(item => item.HVI_ShipperReference == "SHIPREF12");
					var fallBackItem = items1.Single(item => item.HVI_ItemId == "FALLBACKITEM");

					AssertEquals("Items imported", 3, items1.Length);

					AssertEquals("Existing item (11) matched by Shipper Ref.", existingItem11.PK, item11.PK);
					AssertEquals("Existing item (12) matched by Shipper Ref.", existingItem12.PK, item12.PK);
					AssertEquals("Item without Shipper Ref. is matched by Item ID as fallback.", existingItemForFallback.PK, fallBackItem.PK);

					AssertEquals("Existing item line deleted", 0, items1[0].Lines.Count);

					// TWO

					var consignment2 = consignments[1];
					AssertEquals("Existing consignment matched by WaybillNumber", existingConsignment2.PK, consignment2.PK);

					AssertEquals("ConsignmentID unchanged", "CONSIGN2", consignment2.HVC_ConsignmentId);
					AssertEquals("Consignment updated from UXML", "More Buscuits", consignment2.HVC_GoodsDescription);

					var items2 = consignment2.Items.Cast<HVLVItem>().ToArray();
					var item21 = items2.Single(item => item.HVI_ShipperReference == "SHIPREF21");
					var item22 = items2.Single(item => item.HVI_ShipperReference == "SHIPREF22");
					AssertEquals("Items imported", 2, items2.Length);

					AssertEquals("Existing item (21) matched by Shipper Ref.", existingItem21.PK, item21.PK);
					AssertEquals("Existing item (21) updated from UXML", "Apples", item21.HVI_GoodsDescription);

					AssertEquals("New item (22) imported", "Oranges", item22.HVI_GoodsDescription);

					// THREE

					var consignment3 = consignments[2];
					AssertEquals("Existing consignment not matched, but not deleted", existingConsignment3.PK, consignment3.PK);
					AssertEquals("ConsignmentID unchanged", "CONSIGN3", consignment3.HVC_ConsignmentId);

					var items3 = consignment3.Items.Cast<HVLVItem>().ToArray();
					AssertEquals("Existing item (31) unchanged", existingItem31.PK, items3.Single(item => item.HVI_ShipperReference == "SHIPREF31").PK);

					// FOUR

					var consignment4 = consignments[3];
					AssertEquals("New consignment imported", "WAYBILL4", consignment4.HVC_WaybillNumber);

					var items4 = consignment4.Items.Cast<HVLVItem>().ToArray();
					AssertEquals("Items imported", 1, items4.Length);

					AssertEquals("New item (41) imported",
						true,
						items4.Any(item => item.HVI_ShipperReference == "SHIPREF41" && item.HVI_GoodsDescription == "1984"));
				});

				#endregion

				#region Re-Import

				var consignmentItemsSnapshot = new Dictionary<ZGuid, ZGuid[]>();
				var itemHarmonisedCodeSnapshot = new Dictionary<ZGuid, ZGuid[]>();
				foreach (var consignment in bookingHeader.Consignments.Cast<HVLVConsignment>().ToArray())
				{
					consignmentItemsSnapshot.Add(consignment.PK, consignment.Items.Select(item => item.PK).ToArray());

					foreach (var item in consignment.Items.Cast<HVLVItem>().ToArray())
					{
						itemHarmonisedCodeSnapshot.Add(item.PK, item.Lines.Select(codes => codes.PK).ToArray());
					}
				}

				bookingHeader = new HVLVBookingHeaderDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				CombineAssertions("Re-importing the same UXML", () =>
				{
					bookingHeader = new BusinessObjectFactory().Load<HVLVBookingHeader>(bookingHeader.PK);
					AssertEquals("Existing booking header was matched", existingBookingHeader.PK, bookingHeader.PK);

					foreach (var consignment in bookingHeader.Consignments.Cast<HVLVConsignment>().ToArray())
					{
						var expectedItemsPKs = consignmentItemsSnapshot[consignment.PK];
						var currentItemPKs = consignment.Items.Select(item => item.PK).ToArray();

						AssertContainsExactElementsInAnyOrder("All consignments and items stay unchanged",
							expectedItemsPKs,
							currentItemPKs);

						foreach (var item in consignment.Items.Cast<HVLVItem>().ToArray())
						{
							var expectedCodePKs = itemHarmonisedCodeSnapshot[item.PK];
							var currentCodePKs = Enumerable.Select(item.Lines.OfType<HVLVItemLine>(), codes => codes.PK).ToArray();
							AssertContainsExactElementsInAnyOrder("All items and harmonised codes stay unchanged",
								expectedCodePKs,
								currentCodePKs);
						}
					}
				});
				#endregion
			}
		}

		public void TestReadFromDataObject_UpdateHVLVItemLine()
		{
			#region Setup Bizo's

			var creationFactory = new BusinessObjectFactory();

			var existingBookingHeader = creationFactory.New<HVLVBookingHeader>();
			existingBookingHeader.HVH_BookingReference = "HEADERBOOKINGREF";
			existingBookingHeader.HVH_UseShipperDeliveryAccount = true;
			existingBookingHeader.HVH_RS_NKBookingServiceLevel = "STD";

			var billToParty = creationFactory.NewWithValidTestData<OrgHeader>();
			billToParty.OH_Code = "BILLTOPARTY";
			billToParty.MainAddress.Address1 = "45 Bill To Street";
			billToParty.Contacts.AddNew().OC_ContactName = "Bill";

			existingBookingHeader.HVH_OA_BillToParty = billToParty.MainAddress.PK;
			existingBookingHeader.HVH_OC_BillToPartyContact = billToParty.Contacts[0].PK;

			var existingConsignment = existingBookingHeader.Consignments.AddNew();
			existingConsignment.HVC_WaybillNumber = "CONSIGN1";
			existingConsignment.HVC_GoodsDescription = "Ice-cream";

			var existingItem = existingConsignment.Items.AddNew();
			existingItem.HVI_ShipperReference = "SHIPREF11";

			var existingItemLine = existingItem.Lines.AddNew();
			existingItemLine.HVS_OriginTariff = "HC1111";
			existingItemLine.HVS_RN_NKOriginCountryCode = "AU";
			existingItemLine.HVS_CustomsValue = 10m;
			existingItemLine.HVS_DestinationTariff = "TARIFF1111";
			existingItemLine.HVS_ProductCode = "PROD1111";
			existingItemLine.HVS_Quantity = 1;
			existingItemLine.HVS_WeightUnit = "KG";
			existingItemLine.HVS_GoodsDescription = "This is 1111.";
			existingItemLine.HVS_GrossWeight = 2m;
			existingItemLine.HVS_IntrinsicValue = 10m;
			existingItemLine.HVS_ItemURL = "http://www.amazon.com/1111";
			existingItemLine.HVS_NetWeight = 2m;

			creationFactory.Save();

			#endregion

			#region Setup UXML

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataTarget(DataContextType.HVLVBookingHeader, "HEADERBOOKINGREF");

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress { AddressType = AddressTypes.SendersLocalClient, OrganizationCode = "BILLTOPARTY", Contact = "LARRY" },
			});

			var subShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				WayBillNumber = "SOMETHING1",
				GoodsDescription = "More Goodness",
			};

			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				OrderReference = "SHIPREF11",
				GoodsDescription = "Chocolate",
			};
			var packedItem = new PackedItem()
			{
				CIFValue = 12.345,
				GoodsValue = 54.321,
				GrossWeight = 1.23,
				GrossWeightUnit = new UnitOfWeight() { Code = "KG" },
				NetWeight = 3.21,
				NetWeightUnit = new UnitOfWeight() { Code = "OZ" },
				Product = new Product() { Code = "STUFF1" },
				Description = "Stuff No.1",
				PackedQuantity = 13,
				ItemSpecificationUrl = "http://www.google.com/nicestuff",
				CommercialInvoiceLineLink = 1
			};
			packingLine.SetPackedItemCollection(() =>
			{
				var list = new List<PackedItem>();
				list.Add(packedItem);
				return list;
			});

			subShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });
			subShipment.PackingLineCollection.Content = CollectionContent.Complete;

			subShipment.DataContext = DataContextFactory.New();
			subShipment.DataContext.AddDataTarget(DataContextType.HVLVConsignment, existingConsignment.HVC_ConsignmentId);

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { subShipment });

			#endregion

			#region Import

			var bookingHeader = new HVLVBookingHeaderDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			bookingHeader = new BusinessObjectFactory().Load<HVLVBookingHeader>(bookingHeader.PK);

			var itemLines = bookingHeader.Consignments.Cast<HVLVConsignment>().SelectMany(x => x.Items).Cast<HVLVItem>().Single().Lines;
			AssertEquals(1, itemLines.Count);

			var itemLine = itemLines[0];
			CombineAssertions("item lines Updated", () =>
			{
				AssertEquals(packedItem.CIFValue, itemLine.HVS_CustomsValue);
				AssertEquals(packedItem.GoodsValue, itemLine.HVS_IntrinsicValue);
				AssertEquals(packedItem.GrossWeight, itemLine.HVS_GrossWeight);
				AssertEquals(packedItem.GrossWeightUnit.Code, itemLine.HVS_WeightUnit);
				AssertEquals(packedItem.NetWeight, itemLine.HVS_NetWeight);
				AssertEquals(packedItem.Description, itemLine.HVS_GoodsDescription);
				AssertEquals(packedItem.PackedQuantity, new ZDecimal(itemLine.HVS_Quantity));
				AssertEquals(packedItem.ItemSpecificationUrl, itemLine.HVS_ItemURL);
			});

			#endregion

			#region Re-Import with no item lines

			packingLine.PackedItemCollection.Clear();
			subShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });
			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { subShipment });

			new HVLVBookingHeaderDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			bookingHeader = new BusinessObjectFactory().Load<HVLVBookingHeader>(bookingHeader.PK);
			itemLines = bookingHeader.Consignments.Cast<HVLVConsignment>().SelectMany(x => x.Items).Cast<HVLVItem>().Single().Lines;
			AssertEquals("item Lines deleted", 0, itemLines.Count);

			#endregion
		}

		public void TestReadFromDataObject_WhenUpdateExistingItem_ShouldClearHVLVItemLineCollection()
		{
			var existingBookingHeader = Factory.New<HVLVBookingHeader>();
			existingBookingHeader.HVH_BookingReference = "HEADERBOOKINGREF";
			existingBookingHeader.HVH_UseShipperDeliveryAccount = true;
			existingBookingHeader.HVH_RS_NKBookingServiceLevel = "STD";

			var billToParty = Factory.NewWithValidTestData<OrgHeader>();
			billToParty.OH_Code = "BILLTOPARTY";
			billToParty.MainAddress.Address1 = "45 Bill To Street";
			billToParty.Contacts.AddNew().OC_ContactName = "Bill";

			existingBookingHeader.HVH_OA_BillToParty = billToParty.MainAddress.PK;
			existingBookingHeader.HVH_OC_BillToPartyContact = billToParty.Contacts[0].PK;

			var existingConsignment = existingBookingHeader.Consignments.AddNew();
			existingConsignment.HVC_WaybillNumber = "CONSIGN1";
			existingConsignment.HVC_GoodsDescription = "Ice-cream";

			var existingItem = existingConsignment.Items.AddNew();
			existingItem.HVI_CurrentBarcode = "ITEM11";
			existingItem.HVI_ShipperReference = "SHIPREF11";

			var existingItemLine1 = existingItem.Lines.AddNew();
			existingItemLine1.HVS_OriginTariff = "HC1111";
			existingItemLine1.HVS_RN_NKOriginCountryCode = "AU";
			existingItemLine1.HVS_CustomsValue = 10m;
			existingItemLine1.HVS_DestinationTariff = "TARIFF1111";
			existingItemLine1.HVS_ProductCode = "PROD1111";
			existingItemLine1.HVS_Quantity = 1;
			existingItemLine1.HVS_WeightUnit = "KG";
			existingItemLine1.HVS_GoodsDescription = "This is 1111.";
			existingItemLine1.HVS_GrossWeight = 2m;
			existingItemLine1.HVS_IntrinsicValue = 10m;
			existingItemLine1.HVS_ItemURL = "http://www.amazon.com/1111";
			existingItemLine1.HVS_NetWeight = 2m;

			var existingItemLine2 = existingItem.Lines.AddNew();
			existingItemLine2.HVS_OriginTariff = "HC2222";
			existingItemLine2.HVS_RN_NKOriginCountryCode = "FR";
			existingItemLine2.HVS_CustomsValue = 10m;
			existingItemLine2.HVS_DestinationTariff = "TARIFF2222";
			existingItemLine2.HVS_ProductCode = "PROD2222";
			existingItemLine2.HVS_Quantity = 1;
			existingItemLine2.HVS_WeightUnit = "KG";
			existingItemLine2.HVS_GoodsDescription = "This is 2222.";
			existingItemLine2.HVS_GrossWeight = 2m;
			existingItemLine2.HVS_IntrinsicValue = 10m;
			existingItemLine2.HVS_ItemURL = "http://www.amazon.com/2222";
			existingItemLine2.HVS_NetWeight = 2m;

			Factory.SaveForTesting();

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataTarget(DataContextType.HVLVBookingHeader, "HEADERBOOKINGREF");

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress { AddressType = AddressTypes.SendersLocalClient, OrganizationCode = "BILLTOPARTY", Contact = "LARRY" },
			});

			var subShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				WayBillNumber = "SOMETHING1",
				GoodsDescription = "More Goodness",
			};

			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				OrderReference = "SHIPREF11",
				Barcode = "12345678901234567890123456789012345",
				ManifestedWeight = 10,
				Weight = 10,
				ManifestedVolume = 0.5,
				Volume = 1,
				ReferenceNumber = "X0012931292",
				PackType = new PackageType { Code = "PKG" },
				RequiresFumigationCertificate = true,
				IsPersonalEffects = true,
				IsTimber = true,
				IsPerishable = true,
				ContainerNumber = "CTNR123456",
				Height = 3m,
				Length = 4m,
				Width = 5m,
				LengthUnit = new UnitOfLength { Code = Length.Metres, Description = Length.GetDescription(Length.Metres, PluralState.Plural) },
				GoodsDescription = "Foods"
			};
			packingLine.SetPackedItemCollection(() =>
			{
				var list = new List<PackedItem>();
				list.Add(new PackedItem()
				{
					CIFValue = 12.345,
					GoodsValue = 54.321,
					GrossWeight = 1.23,
					GrossWeightUnit = new UnitOfWeight() { Code = "KG" },
					NetWeight = 3.21,
					NetWeightUnit = new UnitOfWeight() { Code = "OZ" },
					Product = new Product() { Code = "PACKED1111" },
					Description = "This is added packed item 1.",
					PackedQuantity = 13,
					ItemSpecificationUrl = "http://www.google.com/nicestuff",
					CommercialInvoiceLineLink = 1
				});
				return list;
			});

			subShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });
			subShipment.PackingLineCollection.Content = CollectionContent.Complete;

			subShipment.DataContext = DataContextFactory.New();
			subShipment.DataContext.AddDataTarget(DataContextType.HVLVConsignment, existingConsignment.HVC_ConsignmentId);

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>
			{
				subShipment
			});

			var itemLinePKsBeforeImport = existingBookingHeader.Consignments.Cast<HVLVConsignment>()
										.SelectMany(x => x.Items).Cast<HVLVItem>()
										.SelectMany(x => x.Lines).Cast<HVLVItemLine>()
										.Select(x => x.PK)
										.ToArray();

			var bookingHeader = new HVLVBookingHeaderDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();

			var itemLinePKsAfterImport = bookingHeader.Consignments.Cast<HVLVConsignment>()
									.SelectMany(x => x.Items).Cast<HVLVItem>()
									.SelectMany(x => x.Lines).Cast<HVLVItemLine>()
									.Select(x => x.PK);
			AssertEquals("Only one item line after import", 1, itemLinePKsAfterImport.Count());
			AssertCollectionNotContains("Existing item line 1 should be removed", itemLinePKsBeforeImport[0], itemLinePKsAfterImport);
			AssertCollectionNotContains("Existing item line 2 should be removed", itemLinePKsBeforeImport[1], itemLinePKsAfterImport);
		}

		public void TestReadFromDataObject_WhenImportNewItem_ShouldNotClearHVLVItemLineCollection()
		{
			var existingBookingHeader = Factory.New<HVLVBookingHeader>();
			existingBookingHeader.HVH_BookingReference = "HEADERBOOKINGREF";
			existingBookingHeader.HVH_UseShipperDeliveryAccount = true;
			existingBookingHeader.HVH_RS_NKBookingServiceLevel = "STD";

			var billToParty = Factory.NewWithValidTestData<OrgHeader>();
			billToParty.OH_Code = "BILLTOPARTY";
			billToParty.MainAddress.Address1 = "45 Bill To Street";
			billToParty.Contacts.AddNew().OC_ContactName = "Bill";

			existingBookingHeader.HVH_OA_BillToParty = billToParty.MainAddress.PK;
			existingBookingHeader.HVH_OC_BillToPartyContact = billToParty.Contacts[0].PK;

			var existingConsignment = existingBookingHeader.Consignments.AddNew();
			existingConsignment.HVC_WaybillNumber = "CONSIGN1";
			existingConsignment.HVC_GoodsDescription = "Ice-cream";

			Factory.SaveForTesting();

			var existingItem = existingConsignment.Items.AddNew();
			existingItem.HVI_CurrentBarcode = "ITEM11";
			existingItem.HVI_ShipperReference = "SHIPREF11";

			var existingItemLine1 = existingItem.Lines.AddNew();
			existingItemLine1.HVS_OriginTariff = "HC1111";
			existingItemLine1.HVS_RN_NKOriginCountryCode = "AU";
			existingItemLine1.HVS_CustomsValue = 10m;
			existingItemLine1.HVS_DestinationTariff = "TARIFF1111";
			existingItemLine1.HVS_ProductCode = "PROD1111";
			existingItemLine1.HVS_Quantity = 1;
			existingItemLine1.HVS_WeightUnit = "KG";
			existingItemLine1.HVS_GoodsDescription = "This is 1111.";
			existingItemLine1.HVS_GrossWeight = 2m;
			existingItemLine1.HVS_IntrinsicValue = 10m;
			existingItemLine1.HVS_ItemURL = "http://www.amazon.com/1111";
			existingItemLine1.HVS_NetWeight = 2m;

			var existingItemLine2 = existingItem.Lines.AddNew();
			existingItemLine2.HVS_OriginTariff = "HC2222";
			existingItemLine2.HVS_RN_NKOriginCountryCode = "FR";
			existingItemLine2.HVS_CustomsValue = 10m;
			existingItemLine2.HVS_DestinationTariff = "TARIFF2222";
			existingItemLine2.HVS_ProductCode = "PROD2222";
			existingItemLine2.HVS_Quantity = 1;
			existingItemLine2.HVS_WeightUnit = "KG";
			existingItemLine2.HVS_GoodsDescription = "This is 2222.";
			existingItemLine2.HVS_GrossWeight = 2m;
			existingItemLine2.HVS_IntrinsicValue = 10m;
			existingItemLine2.HVS_ItemURL = "http://www.amazon.com/2222";
			existingItemLine2.HVS_NetWeight = 2m;

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataTarget(DataContextType.HVLVBookingHeader, "HEADERBOOKINGREF");

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress { AddressType = AddressTypes.SendersLocalClient, OrganizationCode = "BILLTOPARTY", Contact = "LARRY" },
			});

			var subShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				WayBillNumber = "SOMETHING1",
				GoodsDescription = "More Goodness",
			};

			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				OrderReference = "SHIPREF11",
				Barcode = "12345678901234567890123456789012345",
				ManifestedWeight = 10,
				Weight = 10,
				ManifestedVolume = 0.5,
				Volume = 1,
				ReferenceNumber = "X0012931292",
				PackType = new PackageType { Code = "PKG" },
				RequiresFumigationCertificate = true,
				IsPersonalEffects = true,
				IsTimber = true,
				IsPerishable = true,
				ContainerNumber = "CTNR123456",
				Height = 3m,
				Length = 4m,
				Width = 5m,
				LengthUnit = new UnitOfLength { Code = Length.Metres, Description = Length.GetDescription(Length.Metres, PluralState.Plural) },
				GoodsDescription = "Foods"
			};
			packingLine.SetPackedItemCollection(() =>
			{
				var list = new List<PackedItem>();
				list.Add(new PackedItem()
				{
					CIFValue = 12.345,
					GoodsValue = 54.321,
					GrossWeight = 1.23,
					GrossWeightUnit = new UnitOfWeight() { Code = "KG" },
					NetWeight = 3.21,
					NetWeightUnit = new UnitOfWeight() { Code = "OZ" },
					Product = new Product() { Code = "PACKED1111" },
					Description = "This is added packed item 1.",
					PackedQuantity = 13,
					ItemSpecificationUrl = "http://www.google.com/nicestuff",
					CommercialInvoiceLineLink = 1
				});
				return list;
			});

			subShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });
			subShipment.PackingLineCollection.Content = CollectionContent.Complete;

			subShipment.DataContext = DataContextFactory.New();
			subShipment.DataContext.AddDataTarget(DataContextType.HVLVConsignment, existingConsignment.HVC_ConsignmentId);

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>
			{
				subShipment
			});

			var itemLinePKsBeforeImport = existingBookingHeader.Consignments.Cast<HVLVConsignment>()
										.SelectMany(x => x.Items).Cast<HVLVItem>()
										.SelectMany(x => x.Lines).Cast<HVLVItemLine>()
										.Select(x => x.PK)
										.ToArray();

			var bookingHeader = new HVLVBookingHeaderDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();

			var itemLinePKsAfterImport = bookingHeader.Consignments.Cast<HVLVConsignment>()
									.SelectMany(x => x.Items).Cast<HVLVItem>()
									.SelectMany(x => x.Lines).Cast<HVLVItemLine>()
									.Select(x => x.PK);
			AssertEquals("Should have three item lines after import", 3, itemLinePKsAfterImport.Count());
			AssertCollectionContains("Existing item line 1 should not be removed", itemLinePKsBeforeImport[0], itemLinePKsAfterImport);
			AssertCollectionContains("Existing item line 2 should not be removed", itemLinePKsBeforeImport[1], itemLinePKsAfterImport);
		}

		public void TestReadFromDataObject_WhenShipmentIsNull_IsBookingDeterminedCalculatedFromBookingConfirmationReference()
		{
			var billToParty = Factory.NewWithValidTestData<OrgHeader>();
			billToParty.OH_Code = "BILLTOPARTY";
			var billToPartyContact = billToParty.Contacts.AddNew();
			billToPartyContact.OC_ContactName = "LARRY";

			var shipmentWithReference = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentWithReference.BookingConfirmationReference = Freight.Integration.ShipmentStatusList.Codes.Amendment;

			shipmentWithReference.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress { AddressType = AddressTypes.SendersLocalClient, OrganizationCode = "BILLTOPARTY", Contact = "LARRY" }
			});

			var bookingHeader = new HVLVBookingHeaderDataObjectReader(shipmentWithReference, new DummyLogger(), Factory).ReadIntoBusinessObject();
			CombineAssertions(() =>
			{
				Assert("HVH_IsBookingConfirmed is set to False as BookingConfirmationReference is not CNF", !bookingHeader.HVH_IsBookingConfirmed);
			});

			shipmentWithReference.BookingConfirmationReference = Freight.Integration.ShipmentStatusList.Codes.Confirmed;
			bookingHeader = new HVLVBookingHeaderDataObjectReader(shipmentWithReference, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Assert("HVH_IsBookingConfirmed is set to True as BookingConfirmationReference is CNF", bookingHeader.HVH_IsBookingConfirmed);
		}
	}
}

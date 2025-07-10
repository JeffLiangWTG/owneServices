using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.Customs.US.LVS.DataTransfer.Universal;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.US.DataTransfer.Universal.Constants;
using DummyLogger = Enterprise.UniversalDataBuss.Integration.DummyLogger;

namespace Enterprise.Customs.US.LVS.DataTransfer.Test
{
	public class USLVClearanceDataReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestStringValueCharacterCaseIsUpper()
		{
			var characterCasingProperty = typeof(USLVClearanceDataReader).GetProperty(
				"StringValueCharacterCase",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty);
			var reader = new USLVClearanceDataReader(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), new DummyLogger(), Factory);
			var characterCasingValue = characterCasingProperty.GetValue(reader);

			AssertEquals(ZArchitecture.Environment.CharacterCase.Upper, characterCasingValue);
		}

		public void TestGetMatchingOrgAddressFallback()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.MainAddress.Address1 = "12 Wisetech St";
			var shipment = SetupShipment(out var _);
			shipment.OrganizationAddressCollection.ForEach(a =>
			{
				if (a.AddressType.Equals(AddressTypes.SendersLocalClient))
				{
					a.AddressType = "SomeOtherType";
				}
			});
			var subShipment = SetupShipment(out var _);
			subShipment.OrganizationAddressCollection.First(a => a.AddressType.Equals(AddressTypes.SendersLocalClient)).OrganizationCode = client.OH_Code;
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { subShipment });

			var clearance = new USLVClearanceDataReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			AssertEquals("12 Wisetech St", clearance.Client.MainAddress.Address1);
		}

		public void TestReadIntoBusinessObject()
		{
			var shipment = SetupShipment(out var pks);
			var uslvClearance = new USLVClearanceDataReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();
			Assert(uslvClearance.IsInDatabase);
			AssertULSVClearanceValue(uslvClearance, pks);
		}

		public void TestWhenReadIntoBusinessObject_WillMatchAnotherUSLVClearanceByDataTargetKey()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			Factory.SaveForTesting();

			var shipmentDataObject = SetupShipment(out var _);
			shipmentDataObject.DataContext.AddDataTarget(DataContextType.USCustomsLowValueEntriesClearance, clearance.ULH_JobNumber);
			shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>());

			var newClearanceThatFindsTheOldClerance = new USLVClearanceDataReader(shipmentDataObject, new DummyLogger(), Factory).ReadIntoBusinessObject();
			AssertEquals("Should find match", clearance.PK, newClearanceThatFindsTheOldClerance.PK);
		}

		public void TestPortOfEntry()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_PortOfEntry = "Orig";
			Factory.SaveForTesting();

			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "US000";
			unloco.RL_PortName = "USTestPort";

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataTarget(DataContextType.USCustomsLowValueEntriesClearance, clearance.ULH_JobNumber);
			shipment.TransportMode = new CodeDescriptionPair { Code = TransportTypeList.Codes.Air };
			shipment.PortOfFirstArrival = new UNLOCO { Code = "US000" };
			shipment.SetAddInfoCollection(() => new List<AddInfo> { new AddInfo() });

			var uslvClearance = new USLVClearanceDataReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			AssertEquals("Fallback shipment.PortOfFirstArrival: Port of Entry is not set if no Sch D port code is found for shipment.PortOfFirstArrival", "Orig", uslvClearance.ULH_PortOfEntry);

			CreateLocoMapIfNotExists("3786", "US000", USLocoMapSystemUsageList.Codes.Air, isSystem: true);
			CreateLocoMapIfNotExists("4444", "US000", USLocoMapSystemUsageList.Codes.SCD, isSystem: true);

			uslvClearance = new USLVClearanceDataReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			AssertEquals("Fallback shipment.PortOfFirstArrival: Use Sch D port code found for shipment.TransportMode", "3786", uslvClearance.ULH_PortOfEntry);

			clearance.ULH_PortOfEntry = "Orig";
			CreateLocoMapIfNotExists("999", "US000", USLocoMapSystemUsageList.Codes.Air, isSystem: true);
			uslvClearance = new USLVClearanceDataReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			AssertEquals("Fallback shipment.PortOfFirstArrival: If too many SchD port codes exist, do not set", "Orig", uslvClearance.ULH_PortOfEntry);

			shipment.AddInfoCollection.Add(new AddInfo
			{
				Key = LVSConstants.AddInfoConstants.SchDEntry,
				Value = ""
			});

			clearance.ULH_PortOfEntry = "Orig";
			uslvClearance = new USLVClearanceDataReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			AssertEquals("Preferred Value in shipment.AddInfoCollection: If port is specified in AddInfoCollection, use that even if blank", "", uslvClearance.ULH_PortOfEntry);
		}

		public void TestMultipleBillsWithSameBillNumberError()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			consignment1.ULB_HouseBill = "WBN0000";
			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			consignment2.ULB_HouseBill = "WBN0000";
			Factory.SaveForTesting();

			var shipmentDataObject = SetupShipment(out var _);
			shipmentDataObject.DataContext.AddDataTarget(DataContextType.USCustomsLowValueEntriesClearance, clearance.ULH_JobNumber);

			var logger = new LoggerForTest();
			var newClearanceThatFindsTheOldClerance = new USLVClearanceDataReader(shipmentDataObject, logger, Factory).ReadIntoBusinessObject();
			AssertEndsWith("", $"There are multiple bills with same bill number (WBN0000) matched in Low Value Entries {clearance.ULH_JobNumber}.", logger.LogsString);
		}

		#region Implementation

		void AssertULSVClearanceValue(CusUSLVClearance uslvClearance, List<ZGuid> pks)
		{
			CombineAssertions(() =>
			{
				AssertContains("Auto generate the job number rather than use TESTUSLV001", "SEC", uslvClearance.ULH_JobNumber);
				AssertEquals("123456", uslvClearance.ULH_MasterBill);
				AssertEquals(TransportTypeList.Codes.Air, uslvClearance.ULH_TransportMode);
				AssertEquals(Core.Constants.ContainerModes.BreakBulk, uslvClearance.ULH_ContainerMode);
				AssertEquals("USCHI", uslvClearance.ULH_RL_NKPortOfLoading);
				AssertEquals("USLAX", uslvClearance.ULH_RL_NKPortOfDischarge);
				AssertEquals("VES002", uslvClearance.ULH_ConveyanceName);
				AssertEquals("VFN002", uslvClearance.ULH_VoyageFlightNo);
				AssertEquals(pks[0], uslvClearance.ULH_GB);

				AssertEquals(pks[1], uslvClearance.ULH_OH_Client);
				AssertEquals(pks[2], uslvClearance.ULH_OH_Importer);

				AssertEquals(new ZDateTime(2019, 11, 12), uslvClearance.ULH_DepartureDate);
				AssertEquals(new ZDateTime(2019, 11, 13), uslvClearance.ULH_DischargeDate);

				AssertEquals("AAA", uslvClearance.ULH_EntryFilerCode);
				AssertEquals("B815", uslvClearance.ULH_US_NKLocationOfGoods);
				AssertEquals("B815", uslvClearance.ULH_US_NKCentralizedExamSite);
				AssertEquals("EIN", uslvClearance.ULH_IORType);
				AssertEquals("CCC", uslvClearance.ULH_IORReference);
				AssertEquals("ABCD", uslvClearance.ULH_MasterBillIssuerSCAC);
				AssertEquals("DCBA", uslvClearance.ULH_CarrierSCAC);
				AssertEquals("4901", uslvClearance.ULH_PortOfLoading);
				AssertEquals("1113", uslvClearance.ULH_PortOfEntry);
				AssertEquals("4121", uslvClearance.ULH_PortOfDischarge);
				AssertEquals("WOHAHA", uslvClearance.ULH_ContactName);
				AssertEquals("1008610086", uslvClearance.ULH_ContactPhone);
				AssertEquals(true, uslvClearance.ULH_RemoteLocationFiling);
				AssertEquals(new ZDate(2019, 11, 25), uslvClearance.ULH_EntryDate);
			});

			AssertConsignments(uslvClearance.CusUSLVConsignments.Cast<CusUSLVConsignment>().ToList(), pks);
		}

		void AssertConsignments(List<CusUSLVConsignment> cusUSLVConsignments, List<ZGuid> pks)
		{
			AssertEquals(2, cusUSLVConsignments.Count);
			var consignment1 = cusUSLVConsignments.First(u => u.ULB_OwnerReferenceNumber == "OR0000");
			var consignment2 = cusUSLVConsignments.First(u => u.ULB_OwnerReferenceNumber == "OR0001");
			AssertConsignment(consignment1, 0, "KG", pks);
			AssertConsignment(consignment2, 1, "PKG", pks);
			AssertCusUSLVItems(consignment1.CusUSLVItems.Cast<CusUSLVItem>().ToList(), 0);
			AssertCusUSLVItems(consignment2.CusUSLVItems.Cast<CusUSLVItem>().ToList(), 1);
		}

		void AssertConsignment(CusUSLVConsignment cusUSLVConsignment, int index, string packType, List<ZGuid> pks)
		{
			CombineAssertions(() =>
			{
				AssertEquals("WBN000" + index, cusUSLVConsignment.ULB_HouseBill);
				AssertEquals("CN001", cusUSLVConsignment.ULB_EquipmentNumber);
				AssertEquals(3, cusUSLVConsignment.ULB_NumberOfPacks.ToZInt());
				AssertEquals(packType, cusUSLVConsignment.ULB_PackType);
				AssertNotEquals("Entry number is unique and calculated by system", "MC123", cusUSLVConsignment.CE_EntryNum);

				AssertEquals("123A", cusUSLVConsignment.ULB_HouseBillIssuerSCAC);
				AssertEquals(index == 0, cusUSLVConsignment.ULB_NonAMSIndicator);
				AssertEquals("ULB_ConsigneeQualifier been cleared when ULB_OA_Consignee has value", index != 0 ? "BOB" : string.Empty, cusUSLVConsignment.ULB_ConsigneeQualifier);
				AssertEquals("ULB_ConsigneeIdentifier been cleared when ULB_OA_Consignee has value", index != 0 ? "DYLAN" : string.Empty, cusUSLVConsignment.ULB_ConsigneeIdentifier);

				if (index == 0)
				{
					AssertEquals(pks[3], cusUSLVConsignment.ULB_OA_Consignee);

					AssertEquals("S NAME", cusUSLVConsignment.ULB_SellerName);
					AssertEquals("S ADDRESS 1", cusUSLVConsignment.ULB_SellerAddress1);
					AssertEquals("S ADDRESS 2", cusUSLVConsignment.ULB_SellerAddress2);
					AssertEquals("CITY 2", cusUSLVConsignment.ULB_SellerCity);
					AssertEquals("A0002", cusUSLVConsignment.ULB_SellerPostCode);
					AssertEquals("NSW", cusUSLVConsignment.ULB_SellerState);
					AssertEquals("AU", cusUSLVConsignment.ULB_RN_NKSellerCountry);
				}
				else
				{
					AssertEquals(pks[4], cusUSLVConsignment.ULB_OA_Seller);

					AssertEquals("C NAME", cusUSLVConsignment.ULB_ConsigneeName);
					AssertEquals("C ADDRESS 1", cusUSLVConsignment.ULB_ConsigneeAddress1);
					AssertEquals("C ADDRESS 2", cusUSLVConsignment.ULB_ConsigneeAddress2);
					AssertEquals("CITY 1", cusUSLVConsignment.ULB_ConsigneeCity);
					AssertEquals("A0001", cusUSLVConsignment.ULB_ConsigneePostCode);
					AssertEquals("CHI", cusUSLVConsignment.ULB_ConsigneeState);
					AssertEquals("US", cusUSLVConsignment.ULB_RN_NKConsigneeCountry);
				}
			});
		}

		void AssertCusUSLVItems(List<CusUSLVItem> cusUSLVItems, int index)
		{
			CombineAssertions(() =>
			{
				if (index == 0)
				{
					AssertEquals(3, cusUSLVItems.Count);
					AssertCusUSLVItem(cusUSLVItems.First(u => u.ULI_Tariff == "01"), 0);
					AssertCusUSLVItem(cusUSLVItems.First(u => u.ULI_Tariff == "02"), 1);
					AssertCusUSLVItem(cusUSLVItems.First(u => u.ULI_Tariff == "03"), 2);
				}
				else
				{
					AssertEquals(0, cusUSLVItems.Count);
				}
			});
		}

		void AssertCusUSLVItem(CusUSLVItem cusUSLVItem, int index)
		{
			if (index == 0)
			{
				AssertEquals("AAA", cusUSLVItem.ULI_GoodsDescription);
				AssertEquals(10m, cusUSLVItem.ULI_GoodsValue);
				AssertEquals("USD", cusUSLVItem.ULI_RX_NKCurrency);
				AssertEquals("US", cusUSLVItem.ULI_RN_NKCountryOfOrigin);
			}
			else if (index == 1)
			{
				AssertEquals("BBB", cusUSLVItem.ULI_GoodsDescription);
				AssertEquals(20m, cusUSLVItem.ULI_GoodsValue);
				AssertEquals("AUD", cusUSLVItem.ULI_RX_NKCurrency);
				AssertEquals("AU", cusUSLVItem.ULI_RN_NKCountryOfOrigin);
			}
			else if (index == 2)
			{
				AssertEquals("CCC", cusUSLVItem.ULI_GoodsDescription);
				AssertEquals(30m, cusUSLVItem.ULI_GoodsValue);
				AssertEquals("AUD", cusUSLVItem.ULI_RX_NKCurrency);
				AssertEquals("US", cusUSLVItem.ULI_RN_NKCountryOfOrigin);
			}

			AssertEquals(index == 0, cusUSLVItem.ULI_AntiDumping);
			AssertEquals(index == 0, cusUSLVItem.ULI_Countervailing);
			AssertEquals(index == 0 ? 2 : 0, cusUSLVItem.CusUSLVItemPGAs.Count);

			if (index == 0)
			{
				var oga1 = cusUSLVItem.CusUSLVItemPGAs.Cast<CusUSLVItemPGA>().First(u => u.ULP_DisclaimReason == "A");
				var oga2 = cusUSLVItem.CusUSLVItemPGAs.Cast<CusUSLVItemPGA>().First(u => u.ULP_DisclaimReason == "B");
				AssertEquals("NHT", oga1.ULP_Agency);
				AssertEquals("FWS", oga2.ULP_Agency);
				AssertEquals("OFF", oga1.ULP_AgencyProgram);
				AssertEquals("FWS", oga2.ULP_AgencyProgram);
			}
		}

		#region Setup

		Shipment SetupShipment(out List<ZGuid> pks)
		{
			pks = new List<ZGuid>();

			var currentBranch = Factory.NewWithValidTestData<GlbBranch>();
			pks.Add(currentBranch.PK);
			currentBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.MainAddress.Address1 = "45 Bill To Street";
			client.Contacts.AddNew().OC_ContactName = "Bill 0";

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.MainAddress.Address1 = "87 Bill To Street";
			importer.Contacts.AddNew().OC_ContactName = "Bill 1";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.MainAddress.Address1 = "88 Bill To Street";
			consignee.Contacts.AddNew().OC_ContactName = "Bill 2";

			var seller = Factory.NewWithValidTestData<OrgHeader>();
			seller.MainAddress.Address1 = "89 Bill To Street";
			seller.Contacts.AddNew().OC_ContactName = "Bill 3";

			Factory.SaveForTesting();

			pks.Add(client.PK);
			pks.Add(importer.PK);
			pks.Add(consignee.MainAddress.PK);
			pks.Add(seller.MainAddress.PK);

			#region Setup Basic Info

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.USCustomsLowValueEntriesClearance, "TESTUSLV001");
			shipment.WayBillType = new WayBillType
			{
				Code = WayBillTypeList.Codes.Master
			};
			shipment.WayBillNumber = "123456";
			shipment.TransportMode = new CodeDescriptionPair { Code = TransportTypeList.Codes.Air };
			shipment.CustomsContainerMode = new ContainerMode { Code = Core.Constants.ContainerModes.BreakBulk };
			shipment.PortOfLoading = new UNLOCO { Code = "USCHI" };
			shipment.PortOfDischarge = new UNLOCO { Code = "USLAX" };
			shipment.VesselName = "VES002";
			shipment.VoyageFlightNo = "VFN002";
			shipment.Branch = Branch.New(currentBranch);

			#endregion

			#region Setup Address Collection

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = AddressTypes.SendersLocalClient,
					OrganizationCode = client.OH_Code,
					Contact = "Bill 0"
				},
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = nameof(DocAddressType.ImporterOfRecord),
					OrganizationCode = importer.OH_Code,
					Contact = "Bill 1"
				}
			});

			#endregion

			#region Setup Add Info Collection

			shipment.SetAddInfoCollection(() => new List<AddInfo>
			{
				new AddInfo
				{
					Key = LVSConstants.AddInfoConstants.EntryFilerCode,
					Value = "AAA"
				},
				new AddInfo
				{
					Key = LVSConstants.AddInfoConstants.US_NKLocationOfGoods,
					Value = "B815"
				},
				new AddInfo
				{
					Key = LVSConstants.AddInfoConstants.US_NKCentralizedExamSite,
					Value = "B815"
				},
				new AddInfo
				{
					Key = LVSConstants.AddInfoConstants.MasterWayBillIssuerSCAC,
					Value = "ABCD"
				},
				new AddInfo
				{
					Key = LVSConstants.AddInfoConstants.UI_NKCarrierSCAC,
					Value = "DCBA"
				},
				new AddInfo
				{
					Key =  LVSConstants.AddInfoConstants.SchDLoading,
					Value = "4901"
				},
				new AddInfo
				{
					Key =  LVSConstants.AddInfoConstants.SchDEntry,
					Value = "1113"
				},
				new AddInfo
				{
					Key = LVSConstants.AddInfoConstants.SchDArrival,
					Value = "4121"
				},
				new AddInfo
				{
					Key = LVSConstants.AddInfoConstants.EntryMode,
					Value = YesNoList.Codes.Yes
				},
				new AddInfo
				{
					Key = LVSConstants.AddInfoConstants.EntryDate,
					Value = new ZDate(2019, 11, 25).ToString()
				},
				new AddInfo
				{
					Key = LVSConstants.AddInfoConstants.IORType,
					Value = "EIN"
				},
				new AddInfo
				{
					Key = LVSConstants.AddInfoConstants.IORReference,
					Value = "CCC"
				},
				new AddInfo
				{
					Key = LVSConstants.AddInfoConstants.FilerName,
					Value = "WOHAHA"
				},
				new AddInfo
				{
					Key = LVSConstants.AddInfoConstants.FilerPhoneNumber,
					Value = "1008610086"
				}
			});

			#endregion

			#region Setup Date Collection

			shipment.SetDateCollection(() => new List<Date>
			{
				new Date
				{
					Type = DateType.LoadingDate,
					Value = new ZDateTime(2019, 11, 12)
				},
				new Date
				{
					Type = DateType.DischargeDate,
					Value = new ZDateTime(2019, 11, 13)
				}
			});

			#endregion

			#region Setup USLVConsignment Collection

			SetupUSLVConsignmentCollection(shipment, consignee, seller);

			#endregion

			return shipment;
		}

		void SetupUSLVConsignmentCollection(Shipment shipment, OrgHeader consignee, OrgHeader seller)
		{
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>());

			SetupUSLVConsignment(shipment.SubShipmentCollection, 0, "KG", consignee, null);
			SetupUSLVConsignment(shipment.SubShipmentCollection, 1, "PKG", null, seller);
		}

		void SetupUSLVConsignment(DataObjectList<Shipment> subShipmentCollection, int index, string packType, OrgHeader consignee, OrgHeader seller)
		{
			#region Setup Basic Info

			var subShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipment.OwnerRef = "OR000" + index;
			subShipment.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House };
			subShipment.WayBillNumber = "WBN000" + index;
			subShipment.TotalNoOfPacks = 3;
			subShipment.TotalNoOfPacksPackageType = new PackageType() { Code = packType };

			#endregion

			#region Setup Commercial Info

			if (index == 0)
			{
				subShipment.CommercialInfo = GetCommercialInfo();
			}
			else
			{
				subShipment.CommercialInfo = new CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>()
				};
			}

			#endregion

			#region Setup Address Collection

			subShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());

			if (consignee == null)
			{
				subShipment.OrganizationAddressCollection?.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = Constants.AddressTypes.UltimateConsignee,
					CompanyName = "C Name",
					Address1 = "C Address 1",
					Address2 = "C Address 2",
					City = "City 1",
					Postcode = "A0001",
					State = "CHI",
					Country = Country.New(Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedStates))
				});
			}
			else
			{
				subShipment.OrganizationAddressCollection?.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = Constants.AddressTypes.UltimateConsignee,
					OrganizationCode = consignee.OH_Code,
					Contact = consignee.Contacts[0].OC_ContactName
				});
			}

			if (seller == null)
			{
				subShipment.OrganizationAddressCollection?.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = AddressType.Seller,
					CompanyName = "S Name",
					Address1 = "S Address 1",
					Address2 = "S Address 2",
					City = "City 2",
					Postcode = "A0002",
					State = "NSW",
					Country = Country.New(Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia))
				});
			}
			else
			{
				subShipment.OrganizationAddressCollection?.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = AddressType.Seller,
					OrganizationCode = seller.OH_Code,
					Contact = seller.Contacts[0].OC_ContactName
				});
			}

			#endregion

			#region Setup Container Collection

			subShipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new Container
				{
					ContainerNumber = "CN001"
				}
			});

			#endregion

			#region Setup Add Info Collection

			subShipment.SetAddInfoCollection(() => new List<AddInfo>
			{
				new AddInfo
				{
					Key = LVSConstants.AddInfoConstants.WayBillIssuerSCAC,
					Value = "123A"
				},
				new AddInfo
				{
					Key = LVSConstants.AddInfoConstants.NonAMS,
					Value = index == 0 ? YesNoList.Codes.Yes : YesNoList.Codes.No
				},
				new AddInfo
				{
					Key = LVSConstants.AddInfoConstants.ConsigneeType,
					Value = "Bob"
				},
				new AddInfo
				{
					Key = LVSConstants.AddInfoConstants.ConsigneeReference,
					Value = "Dylan"
				}
			});

			#endregion

			#region Setup Entry Number Collection

			subShipment.SetEntryNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.EntryNumber>
			{
				new UniversalDataBuss.DataObjects.Universal.EntryNumber
				{
					Number = "MC123"
				}
			});

			#endregion

			subShipmentCollection.Add(subShipment);
		}

		CommercialInfo GetCommercialInfo()
		{
			var commercialInfo = new CommercialInfo();
			var commercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>();
			var invoiceHeader1 = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceCurrency = new Currency
				{
					Code = "USD"
				}
			};

			var invoiceHeader2 = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceCurrency = new Currency
				{
					Code = "AUD"
				}
			};

			commercialInvoiceCollection.Add(invoiceHeader1);
			commercialInvoiceCollection.Add(invoiceHeader2);

			invoiceHeader1.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>()
			{
				new CommercialInvoiceLine()
				{
					CountryOfOrigin = new Country { Code = "US" },
					Description = "AAA",
					HarmonisedCode = "TIF01",
					LinePrice = 10,
					LineNo = 1,
					AddInfoCollection = GetInvoiceLineAddInfo(0)
				}
			});

			invoiceHeader2.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>()
			{
				new CommercialInvoiceLine
				{
					CountryOfOrigin = new Country { Code = "AU" },
					Description = "BBB",
					HarmonisedCode = "TIF02",
					LinePrice = 20,
					LineNo = 2,
					AddInfoCollection = GetInvoiceLineAddInfo(1)
				},
				new CommercialInvoiceLine
				{
					CountryOfOrigin = new Country { Code = "US" },
					Description = "CCC",
					HarmonisedCode = "TIF03",
					LinePrice = 30,
					LineNo = 3,
					AddInfoCollection = GetInvoiceLineAddInfo(1)
				}
			});

			commercialInfo.CommercialInvoiceCollection = commercialInvoiceCollection;
			return commercialInfo;
		}

		List<AddInfo> GetInvoiceLineAddInfo(int index)
		{
			var listAddInfo = new List<AddInfo>
			{
				new AddInfo
				{
					Key = LVSConstants.AddInfoConstants.ADD_NA,
					Value = index == 0 ? YesNoList.Codes.Yes : YesNoList.Codes.No,
				},
				new AddInfo
				{
					Key = LVSConstants.AddInfoConstants.CVD_NA,
					Value = index == 0 ? YesNoList.Codes.Yes : YesNoList.Codes.No,
				}
			};

			if (index == 0)
			{
				var agencyAnalysis = new List<AddInfo>
				{
					new AddInfo
					{
						Key = LVSConstants.AddInfoConstants.NHTDisclaimReason,
						Value = "A"
					},
					new AddInfo
					{
						Key = LVSConstants.AddInfoConstants.FWSDisclaimReason,
						Value = "B"
					}
				};

				listAddInfo.AddRange(agencyAnalysis);
			}

			return listAddInfo;
		}

		#endregion

		void CreateLocoMapIfNotExists(string localPort, string unLoco, string usage, bool isSystem = false)
		{
			var codeFilter = new ZQuery(RefLocoMapSchema.RY_LocalPortCode, localPort);
			codeFilter.AddToFilter(RefLocoMapSchema.RY_RL_NKLocoPort, unLoco);
			codeFilter.AddToFilter(RefLocoMapSchema.RY_SystemUsage, usage);
			if (isSystem)
			{
				codeFilter.AddToFilter(RefLocoMapSchema.RY_IsSystem, isSystem);
			}

			var locoMap = Factory.LoadTop1<RefLocoMap>(codeFilter);
			if (locoMap == null)
			{
				locoMap = Factory.NewWithValidTestData<RefLocoMap>();
				locoMap.RY_LocalPortCode = localPort;
				locoMap.RY_RL_NKLocoPort = unLoco;
				locoMap.RY_SystemUsage = usage;
				locoMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
				locoMap.RY_IsSystem = isSystem;
			}
		}

		#endregion
	}

	class LoggerForTest : IXmlImportLogger
	{
		readonly List<DummyLog> notifications = new();

		public void Log(LogType type, string message)
		{
			notifications.Add(new DummyLog(type, message));
		}

		public void LogBoth(LogType type, string message)
		{
			notifications.Add(new DummyLog(type, message));
		}

		public string LogsString => string.Join("\r\n", notifications.Select(log => log.Type.ToString() + " - " + log.Message));

		public bool OrgMatchingDisabled => false;

		#region Not Implemented

		public bool IsUpdatingConsol { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public bool HasIgnoredModule { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public IEnumerable<IValidationRule> ValidationRuleCollection { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public ITopLevelDataObject TopLevelDataObject => throw new NotImplementedException();
		IDataContextDataObject topLevelDataContext;
		public IDataContextDataObject TopLevelDataContext => topLevelDataContext ?? (topLevelDataContext = DataContextFactory.New());
		public IEnumerable<ISimpleLog> Logs => notifications;
		public void FireDataImportedToBusinessObject(BusinessObject targetBO) { }
		public void LogErrorToServiceTaskOnly(string message) => throw new NotImplementedException();
		public void LogTopLevelDataContextKey(GetDataContextKey getDataContextKey) => throw new NotImplementedException();

		#endregion
	}

	class DummyLog : ISimpleLog
	{
		public DummyLog(LogType type, string message)
		{
			_type = type;
			_message = message;
		}

		readonly LogType _type;
		readonly string _message;

		public LogType Type => _type;

		public string Message => _message;
	}
}


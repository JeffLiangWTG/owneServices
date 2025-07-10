using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.ManifestBase;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.ASYCUDA;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	sealed class ExtensionsTest : TestCaseWithFactory
	{
		[AsycudaCustomsCountries("CG")]
		public void TestGetUniversalCustomsDataObjectProvider()
		{
			var erProvider = Factory.GetUniversalCustomsDataObjectProvider(Core.Constants.CountryCodes.Eritrea);
			AssertNull(erProvider);

			var auProvider = Factory.GetUniversalCustomsDataObjectProvider(Core.Constants.CountryCodes.Australia);
			AssertEquals("Enterprise.Customs.AU.Declaration.Business.UniversalCustomsDataObjectProvider", auProvider.GetType().FullName);

			var euProvider = Factory.GetUniversalCustomsDataObjectProvider(Core.Constants.CountryCodes.Latvia);
			AssertEquals("Enterprise.Customs.EU.DataTransfer.Universal.UniversalCustomsDataObjectProvider", euProvider.GetType().FullName);

			var asycudaProvider = Factory.GetUniversalCustomsDataObjectProvider(Core.Constants.CountryCodes.Congo);
			AssertEquals("Enterprise.Customs.AsycudaCustoms.DataTransfer.Universal.UniversalCustomsDataObjectProvider", asycudaProvider.GetType().FullName);

			var spainProvider = Factory.GetUniversalCustomsDataObjectProvider(Core.Constants.CountryCodes.Spain);
			AssertEquals("Enterprise.Customs.ES.DataTransfer.Universal.UniversalCustomsDataObjectProvider", spainProvider.GetType().FullName);

			var turkeyProvider = Factory.GetUniversalCustomsDataObjectProvider(Core.Constants.CountryCodes.Turkey);
			AssertEquals("For now TR use Enterprise.Customs.EU.DataTransfer.Universal.UniversalCustomsDataObjectProvider", "Enterprise.Customs.EU.DataTransfer.Universal.UniversalCustomsDataObjectProvider", turkeyProvider.GetType().FullName);
		}

		public void TestGetApplicationSpecificUniversalCustomsDataObjectProvider()
		{
			var euDataTransferAssembly = AssemblyLoader.LoadAssembly("Enterprise.Customs.EU.EMCS.DataTransfer");
			var typeOfEMCSUniversalCustomsDataObjectProvider = euDataTransferAssembly.GetType("Enterprise.Customs.EU.EMCS.DataTransfer.EMCSUniversalCustomsDataObjectProvider");
			Assert(typeOfEMCSUniversalCustomsDataObjectProvider.IsInstanceOfType(Factory.GetApplicationSpecificUniversalCustomsDataObjectProvider("EMC")));

			var zaaProvider = Factory.GetApplicationSpecificUniversalCustomsDataObjectProvider(ApplicationCodeList.Codes.ZATransactionOrders);
			AssertEquals("Enterprise.Customs.ZA.DataTransfer.Universal.ZAAUniversalCustomsDataObjectProvider", zaaProvider?.GetType()?.FullName);
		}

		public void TestGetLoadingAndDischargeDate()
		{
			var shipment1 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var leg = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance)
			{
				EstimatedArrival = ZDate.Today.AddDays(1),
				EstimatedDeparture = ZDate.Today.AddDays(-1)
			};
			CombineAssertions("", () =>
			{
				AssertEquals("Air Discharge Date", ZDate.Today.AddDays(1), shipment1.GetDischargeDateForAir(leg));
				AssertEquals("Air Loading Date", ZDate.Today.AddDays(-1), shipment1.GetLoadingDateForAir(leg));
				AssertEquals("Sea Discharge Date", ZDate.Today.AddDays(1), shipment1.GetDischargeDateForSea(leg));
				AssertEquals("Sea Loading Date", ZDate.Today.AddDays(-1), shipment1.GetLoadingDateForSea(leg));
			});

			leg.ActualArrival = ZDate.Today.AddDays(2);
			leg.ActualDeparture = ZDate.Today.AddDays(-2);
			CombineAssertions("", () =>
			{
				AssertEquals("Air Discharge Date", ZDate.Today.AddDays(2), shipment1.GetDischargeDateForAir(leg));
				AssertEquals("Air Loading Date", ZDate.Today.AddDays(-2), shipment1.GetLoadingDateForAir(leg));
				AssertEquals("Sea Discharge Date", ZDate.Today.AddDays(2), shipment1.GetDischargeDateForSea(leg));
				AssertEquals("Sea Loading Date", ZDate.Today.AddDays(-2), shipment1.GetLoadingDateForSea(leg));
			});

			shipment1.SetDateCollection(() => new List<Date>());
			shipment1.DateCollection.Add(new Date() { Type = DateType.DischargeDate, Value = ZDate.Today.AddDays(3) });
			shipment1.DateCollection.Add(new Date() { Type = DateType.LoadingDate, Value = ZDate.Today.AddDays(-3) });
			shipment1.DateCollection.Add(new Date() { Type = DateType.Departure, Value = ZDate.Today.AddDays(-3) });
			CombineAssertions("", () =>
			{
				AssertEquals("Air Discharge Date", ZDate.Today.AddDays(3), shipment1.GetDischargeDateForAir(leg));
				AssertEquals("Air Loading Date", ZDate.Today.AddDays(-3), shipment1.GetLoadingDateForAir(leg));
				AssertEquals("Sea Discharge Date", ZDate.Today.AddDays(3), shipment1.GetDischargeDateForSea(leg));
				AssertEquals("Sea Loading Date", ZDate.Today.AddDays(-3), shipment1.GetLoadingDateForSea(leg));
			});
		}

		public void TestAddOrUpdate()
		{
			List<AddInfo> addInfos = null;
			AssertNull(addInfos.AddOrUpdate("HELLO", "WORLD"));
			addInfos = new List<AddInfo>();
			var addInfo1 = addInfos.AddOrUpdate("HELLO", "WORLD");
			AssertEquals(1, addInfos.Count);
			AssertEquals("addInfo1.Key", "HELLO", addInfo1.Key);
			AssertEquals("addInfo1.Value", "WORLD", addInfo1.Value);
			var addInfo2 = addInfos.AddOrUpdate("HI", "BOB");
			AssertEquals(2, addInfos.Count);
			AssertEquals("addInfo1.Key", "HELLO", addInfo1.Key);
			AssertEquals("addInfo1.Value", "WORLD", addInfo1.Value);
			AssertEquals("addInfo2.Key", "HI", addInfo2.Key);
			AssertEquals("addInfo2.Value", "BOB", addInfo2.Value);
			AssertEquals(addInfo1, addInfos.AddOrUpdate("HELLO", "BOB"));
			AssertEquals(2, addInfos.Count);
			AssertEquals("addInfo1.Key", "HELLO", addInfo1.Key);
			AssertEquals("addInfo1.Value", "BOB", addInfo1.Value);
			AssertEquals("addInfo2.Key", "HI", addInfo2.Key);
			AssertEquals("addInfo2.Value", "BOB", addInfo2.Value);

			AssertNull(addInfos.AddOrUpdate("HELLO", ZString.Empty));
			AssertEquals(1, addInfos.Count);
			addInfo2 = addInfos[0];
			AssertEquals("addInfo2.Key", "HI", addInfo2.Key);
			AssertEquals("addInfo2.Value", "BOB", addInfo2.Value);

			AssertNull(addInfos.AddOrUpdate("BYE", ZString.Empty));
			AssertEquals(1, addInfos.Count);
			addInfo2 = addInfos[0];
			AssertEquals("addInfo2.Key", "HI", addInfo2.Key);
			AssertEquals("addInfo2.Value", "BOB", addInfo2.Value);

			addInfo1 = addInfos.AddOrUpdate("BYE", ZString.Empty, false);
			AssertEquals(2, addInfos.Count);
			AssertEquals("addInfo1.Key", "BYE", addInfo1.Key);
			AssertEquals("addInfo1.Value", "", addInfo1.Value);
			AssertEquals(addInfo2, addInfos.FirstOrDefault(x => x.Key.Value == "HI"));
			AssertEquals("addInfo2.Value", "BOB", addInfo2.Value);

			AssertEquals(addInfo2, addInfos.AddOrUpdate("HI", ZString.Empty, false));
			AssertEquals("addInfo2.Value", "", addInfo2.Value);
			AssertEquals(addInfo1, addInfos.FirstOrDefault(x => x.Key.Value == "BYE"));
			AssertEquals("addInfo1.Value", "", addInfo2.Value);
		}

		public void TestFromCodes()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "OH1";
			var address1 = org.MainAddress;
			address1.OA_Code = "OA1";
			var address2 = org.Addresses.AddNew();
			address2.OA_Code = "OA2";
			var orgAddressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			orgAddressData.OrganizationCode = "XXX";
			orgAddressData.AddressShortCode = "XXX";
			AssertNull(orgAddressData.GetMatchedUsingCodes(Factory));
			orgAddressData.OrganizationCode = "OH1";
			orgAddressData.AddressShortCode = "OA2";
			AssertEquals(address2.PK, orgAddressData.GetMatchedUsingCodes(Factory).PK);
			orgAddressData.AddressShortCode = "";
			AssertEquals(address1.PK, orgAddressData.GetMatchedUsingCodes(Factory).PK);
		}

		public void TestAddDummyOrganizationForCodeOnly()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.AddDummyOrganizationForCodeOnly(DefaultDataObjectWriterStrategy.TestInstance, DocAddressType.Carrier, "HELLO BOB", Factory.Load<RefCountry>(Core.Constants.CountryGuids.UnitedStates), OrgCusCode.CodeTypes.CarrierCode, "A!");
			AssertEquals("shipment.OrganizationAddressCollection.Count", 1, shipment.OrganizationAddressCollection.Count);
			var orgAddressData = shipment.OrganizationAddressCollection[0];
			AssertEquals("orgAddressData.AddressType", nameof(DocAddressType.Carrier), orgAddressData.AddressType);
			AssertEquals("orgAddressData.CompanyName", "HELLO BOB", orgAddressData.CompanyName);
			AssertEquals("orgAddressData.RegistrationNumberCollection.Count", 1, orgAddressData.RegistrationNumberCollection.Count);
			var registrationNumberData = orgAddressData.RegistrationNumberCollection[0];
			AssertEquals("registrationNumberData.CountryOfIssue.Code", Core.Constants.CountryCodes.UnitedStates, registrationNumberData.CountryOfIssue.Code);
			AssertEquals("registrationNumberData.Type.Code", OrgCusCode.CodeTypes.CarrierCode, registrationNumberData.Type.Code);
			AssertEquals("registrationNumberData.Type.Description", "Standard Carrier Alpha Code (Sea)", registrationNumberData.Type.Description);
			AssertEquals("registrationNumberData.Value", "A!", registrationNumberData.Value);
		}

		public void TestIsUSCATAIRMessageEvent()
		{
			var eventDataObject = new Event();
			eventDataObject.DataContext = new DataContext()
			{
				DataSourceCollection = new List<DataSource>(),
				DataTargetCollection = new List<DataTarget>(),
			};
			var dataContext = (DataContext)eventDataObject.DataContext;
			eventDataObject.DataContext.AddDataTarget(DataContextType.USImporterSecurityFiling, null);
			dataContext.DataProvider = Constants.CATAIRMessage.DataProvider;
			dataContext.ActionPurpose = new CodeDescriptionPair() { Code = "BS", Description = "" };

			AssertEquals(true, eventDataObject.IsUSCATAIRMessageEvent(DataContextType.USImporterSecurityFiling));

			var eventDataObject2 = new Event();
			eventDataObject2.DataContext = new DataContext()
			{
				DataSourceCollection = new List<DataSource>(),
				DataTargetCollection = new List<DataTarget>(),
			};
			var dataContext2 = (DataContext)eventDataObject2.DataContext;
			eventDataObject2.DataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
			dataContext2.DataProvider = Constants.CATAIRMessage.DataProvider;
			dataContext2.ActionPurpose = new CodeDescriptionPair() { Code = "BS", Description = "" };

			AssertEquals(true, eventDataObject2.IsUSCATAIRMessageEvent(DataContextType.CustomsDeclaration));

			var eventDataObject3 = new Event();
			eventDataObject3.DataContext = new DataContext()
			{
				DataSourceCollection = new List<DataSource>(),
				DataTargetCollection = new List<DataTarget>(),
			};
			var dataContext3 = (DataContext)eventDataObject3.DataContext;
			eventDataObject3.DataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
			dataContext3.ActionPurpose = new CodeDescriptionPair() { Code = "BS", Description = "" };

			AssertEquals(false, eventDataObject3.IsUSCATAIRMessageEvent(DataContextType.CustomsDeclaration));

			var eventDataObject4 = new Event();
			eventDataObject4.DataContext = new DataContext()
			{
				DataSourceCollection = new List<DataSource>(),
				DataTargetCollection = new List<DataTarget>(),
			};
			var dataContext4 = (DataContext)eventDataObject4.DataContext;
			eventDataObject4.DataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
			dataContext4.DataProvider = Constants.CATAIRMessage.DataProvider;

			AssertEquals(false, eventDataObject4.IsUSCATAIRMessageEvent(DataContextType.CustomsDeclaration));
		}

		public void TestIsAirImportCustomsMessageEvent()
		{
			var eventDataObject = new Event();
			eventDataObject.DataContext = new DataContext()
			{
				DataSourceCollection = new List<DataSource>(),
				DataTargetCollection = new List<DataTarget>(),
			};
			eventDataObject.EventType = ZArchitecture.Business.Events.ReceivedCode;

			var mawbNumberContext = new Context();
			mawbNumberContext.Type = new ContextType() { Type = "MAWBNumber" };
			mawbNumberContext.Value = "1233";

			eventDataObject.ContextCollection = new List<Context>();
			eventDataObject.ContextCollection.Add(mawbNumberContext);

			var eventParameters = new EventParameters();
			eventParameters.Quantity = 2;
			eventParameters.Department = "Customs";

			eventDataObject.EventParameters = eventParameters;

			AssertEquals(true, eventDataObject.IsAirImportCustomsMessageEvent());

			eventDataObject.EventType = ZArchitecture.Business.Events.RecordArchivedCode;

			AssertEquals(false, eventDataObject.IsAirImportCustomsMessageEvent());

			eventDataObject.EventType = ZArchitecture.Business.Events.ReceivedCode;
			mawbNumberContext.Value = "1233";
			eventParameters.Department = "BRN";

			AssertEquals(false, eventDataObject.IsAirImportCustomsMessageEvent());

			eventParameters.Department = "Customs";
			eventParameters.Quantity = 0;

			AssertEquals(false, eventDataObject.IsAirImportCustomsMessageEvent());

			var eventDataObject2 = new Event();
			eventDataObject2.DataContext = new DataContext()
			{
				DataSourceCollection = new List<DataSource>(),
				DataTargetCollection = new List<DataTarget>(),
			};
			eventDataObject2.EventType = ZArchitecture.Business.Events.ReceivedCode;
			eventDataObject2.ContextCollection = new List<Context>();
			eventDataObject2.ContextCollection.Add(mawbNumberContext);
			eventDataObject2.EventParameters = eventParameters;
			mawbNumberContext.Value = "";

			AssertEquals(false, eventDataObject.IsAirImportCustomsMessageEvent());
		}

		public void TestGetColoadBill_NoExceptionWhenNoWaybillForHVLV()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var hvlvShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			AssertNoExceptionThrown("No exception should be thrown when there is no waybill for HVLV", () => shipment.GetColoadBill(hvlvShipment));
		}

		public void TestGetColoadBill_HVLVOnlyReturnForHWB()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var hvlvShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			hvlvShipment.WayBillNumber = "123";
			hvlvShipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master };

			AssertNull("Coload Bill should not be set for WayBillType MWB", shipment.GetColoadBill(hvlvShipment));

			hvlvShipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };

			AssertEquals("Coload Bill should be set for WayBillType HWB", hvlvShipment.WayBillNumber, shipment.GetColoadBill(hvlvShipment));
		}

		public void TestIsHVLV()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>()
				{
					new Shipment(DefaultDataObjectWriterStrategy.TestInstance),
					new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				});

			Assert(!shipment.IsHVLV());

			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.ForwardingConsol, "C00029310");
			dataContext.AddDataSource(DataContextType.ForwardingShipment, "S900053111");
			shipment.DataContext = dataContext;
			shipment.SubShipmentCollection.Clear();
			shipment.SubShipmentCollection.Add(new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ShipmentType = new CodeDescriptionPair()
				{
					Code = Core.Constants.ShipmentTypes.HighVolumeLowValue
				}
			});

			Assert(shipment.IsHVLV());
		}

		public void TestIsTWH()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>()
				{
					new Shipment(DefaultDataObjectWriterStrategy.TestInstance),
					new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				});

			Assert(!shipment.IsTWH());

			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.TransitReceiveHeader, "TRH12345");
			shipment.DataContext = dataContext;

			Assert("Should be true when the DataContextType is TransitReceiveHeader", shipment.IsTWH());

			var dataContext2 = DataContextFactory.New();
			dataContext2.AddDataSource(DataContextType.TransitReceive, "TR12345");
			shipment.DataContext = dataContext2;

			Assert("Should be true when the DataContextType is TransitReceive", shipment.IsTWH());
		}

		public void TestFindMatchingCommercialInvoiceLine()
		{
			var item1 = new PackedItem { CommercialInvoiceLineLink = 1 };
			var item2 = new PackedItem { CommercialInvoiceLineLink = 2 };
			var item3 = new PackedItem { CommercialInvoiceLineLink = 3 };

			var line1 = new CommercialInvoiceLine { Link = 2 };
			var line2 = new CommercialInvoiceLine { Link = 3 };
			var line3 = new CommercialInvoiceLine { Link = 1 };

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var info = shipment.CommercialInfo = new CommercialInfo();
			var commercialInvoiceHeader = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			info.CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>() { commercialInvoiceHeader };

			commercialInvoiceHeader.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>
			{
				line1,
				line2,
				line3
			});

			CombineAssertions("Should find matching invoice lines based on Link", () =>
			{
				AssertEquals("Link 1: Item 1 -> Line 3", line3, item1.FindMatchingCommercialInvoiceLine(shipment));
				AssertEquals("Link 2: Item 3 -> Line 1", line1, item2.FindMatchingCommercialInvoiceLine(shipment));
				AssertEquals("Link 3: Item 2 -> Line 2", line2, item3.FindMatchingCommercialInvoiceLine(shipment));
			});
		}

		public void TestFindMatchingCommercialInvoiceLine_UsesAllInvoiceLines()
		{
			var item1 = new PackedItem { CommercialInvoiceLineLink = 1 };
			var item2 = new PackedItem { CommercialInvoiceLineLink = 2 };
			var item3 = new PackedItem { CommercialInvoiceLineLink = 3 };
			var item4 = new PackedItem { CommercialInvoiceLineLink = 4 };

			var line1 = new CommercialInvoiceLine { Link = 2 };
			var line2 = new CommercialInvoiceLine { Link = 3 };
			var line3 = new CommercialInvoiceLine { Link = 1 };
			var line4 = new CommercialInvoiceLine { Link = 4 };

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var info = shipment.CommercialInfo = new CommercialInfo();

			var commercialInvoiceHeader1 = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			var commercialInvoiceHeader2 = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			var commercialInvoiceHeader3 = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);

			info.CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>()
			{
				commercialInvoiceHeader1,
				commercialInvoiceHeader2,
				commercialInvoiceHeader3,
			};

			commercialInvoiceHeader1.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>
			{
				line1,
				line2
			});

			commercialInvoiceHeader2.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>
			{
				line4
			});

			commercialInvoiceHeader3.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>
			{
				line3,
			});

			CombineAssertions("Should find matching invoice lines based on Link", () =>
			{
				AssertEquals("Link 1: Item 1 -> Line 3", line3, item1.FindMatchingCommercialInvoiceLine(shipment));
				AssertEquals("Link 2: Item 3 -> Line 1", line1, item2.FindMatchingCommercialInvoiceLine(shipment));
				AssertEquals("Link 3: Item 2 -> Line 2", line2, item3.FindMatchingCommercialInvoiceLine(shipment));
				AssertEquals("Link 4: Item 4 -> Line 4", line4, item4.FindMatchingCommercialInvoiceLine(shipment));
			});
		}

		public void TestFindMatchingCommercialInvoiceLine_ReturnsNullWhenNotFound()
		{
			var item1 = new PackedItem { CommercialInvoiceLineLink = 10 };

			var line1 = new CommercialInvoiceLine { Link = 2 };
			var line2 = new CommercialInvoiceLine { Link = 3 };
			var line3 = new CommercialInvoiceLine { Link = 1 };

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var info = shipment.CommercialInfo = new CommercialInfo();

			var commercialInvoiceHeader = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			info.CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>() { commercialInvoiceHeader };

			commercialInvoiceHeader.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>
			{
				line1,
				line2,
				line3
			});

			AssertNull("Linked Line Not Found", item1.FindMatchingCommercialInvoiceLine(shipment));
		}

		public void TestFindRelatedCustomsJobFromHVLVShipment()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;

			var dummyJob = Factory.NewWithValidTestData<DummyBaseBusinessObject>();

			var genPivot = Factory.New<IGenPivot>();
			genPivot.XX_RelationType = Core.Constants.GenPivotTypes.HighVolumeLowValue;
			genPivot.XX_Relation1ID = shipment.HVLVConsignmentHeader.PK;
			genPivot.XX_Relation1TableCode = HVLVConsignmentHeaderSchema.Constants.Prefix;
			genPivot.XX_Relation2ID = dummyJob.PK;
			genPivot.XX_Relation2TableCode = DummyBizoSchema.Constants.Prefix;

			Factory.Save();

			var consolDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			consolDataObject.DataContext = DataContextFactory.New();
			consolDataObject.DataContext.AddDataSource(DataContextType.ForwardingConsol, consol.JK_MasterBillNum);
			consolDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, shipment.JS_UniqueConsignRef);

			var shipmentDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, shipment.JS_UniqueConsignRef);
			shipmentDataObject.ShipmentType = new CodeDescriptionPair() { Code = "HVL" };

			consolDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment> { shipmentDataObject });

			AssertEquals("Data object is HVLV", true, consolDataObject.IsHVLV());

			var newFactory = new BusinessObjectFactory();
			var foundDummyJob = consolDataObject.FindRelatedCustomsJobFromHVLVShipment<DummyBaseBusinessObject>(newFactory);

			AssertEquals("Should find existing dummy job", dummyJob.PK, foundDummyJob.PK);
		}

		public void TestFindRelatedCustomsJobFromHVLVShipment_ForMultipleAsycudaHeaders()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;

			var etrAsycudaHeader = Factory.New<IAsycudaManifestHeader>() as BusinessObject;
			etrAsycudaHeader.FillWithValidTestData();
			etrAsycudaHeader[AsycudaManifestHeaderSchema.AMA_JobReference] = "M00000001";
			etrAsycudaHeader[AsycudaManifestHeaderSchema.AMA_ApplicationCode] = "ETR";

			var vocAsycudaHeader = Factory.New<IAsycudaManifestHeader>() as BusinessObject;
			vocAsycudaHeader.FillWithValidTestData();
			vocAsycudaHeader[AsycudaManifestHeaderSchema.AMA_JobReference] = "M00000002";
			vocAsycudaHeader[AsycudaManifestHeaderSchema.AMA_ApplicationCode] = "VOC";

			var nvcAsycudaHeader = Factory.New<IAsycudaManifestHeader>() as BusinessObject;
			nvcAsycudaHeader.FillWithValidTestData();
			nvcAsycudaHeader[AsycudaManifestHeaderSchema.AMA_JobReference] = "M00000003";
			nvcAsycudaHeader[AsycudaManifestHeaderSchema.AMA_ApplicationCode] = "NVC";

			var etrGenPivot = Factory.New<IGenPivot>();
			etrGenPivot.XX_RelationType = Core.Constants.GenPivotTypes.HighVolumeLowValue;
			etrGenPivot.XX_Relation1ID = shipment.HVLVConsignmentHeader.PK;
			etrGenPivot.XX_Relation1TableCode = HVLVConsignmentHeaderSchema.Constants.Prefix;
			etrGenPivot.XX_Relation2ID = etrAsycudaHeader.PK;
			etrGenPivot.XX_Relation2TableCode = AsycudaManifestHeaderSchema.Constants.Prefix;

			var vocGenPivot = Factory.New<IGenPivot>();
			vocGenPivot.XX_RelationType = Core.Constants.GenPivotTypes.HighVolumeLowValue;
			vocGenPivot.XX_Relation1ID = shipment.HVLVConsignmentHeader.PK;
			vocGenPivot.XX_Relation1TableCode = HVLVConsignmentHeaderSchema.Constants.Prefix;
			vocGenPivot.XX_Relation2ID = vocAsycudaHeader.PK;
			vocGenPivot.XX_Relation2TableCode = AsycudaManifestHeaderSchema.Constants.Prefix;

			var nvcGenPivot = Factory.New<IGenPivot>();
			vocGenPivot.XX_RelationType = Core.Constants.GenPivotTypes.HighVolumeLowValue;
			vocGenPivot.XX_Relation1ID = shipment.HVLVConsignmentHeader.PK;
			vocGenPivot.XX_Relation1TableCode = HVLVConsignmentHeaderSchema.Constants.Prefix;
			vocGenPivot.XX_Relation2ID = nvcAsycudaHeader.PK;
			vocGenPivot.XX_Relation2TableCode = AsycudaManifestHeaderSchema.Constants.Prefix;

			Factory.Save();

			var consolDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			consolDataObject.DataContext = DataContextFactory.New();
			consolDataObject.DataContext.AddDataSource(DataContextType.ForwardingConsol, consol.JK_MasterBillNum);
			consolDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, shipment.JS_UniqueConsignRef);
			consolDataObject.MessagingApplicationCode = new CodeDescriptionPair() { Code = "ETR" };

			var shipmentDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, shipment.JS_UniqueConsignRef);
			shipmentDataObject.ShipmentType = new CodeDescriptionPair() { Code = "HVL" };

			consolDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment> { shipmentDataObject });

			AssertEquals("Data object is HVLV", true, consolDataObject.IsHVLV());

			var newFactory = new BusinessObjectFactory();
			var foundAsycudaHeader = consolDataObject.FindRelatedCustomsJobFromHVLVShipment<AsycudaManifestHeader>(newFactory);

			AssertEquals("Should find correct Asycuda Header", etrAsycudaHeader.PK, foundAsycudaHeader.PK);
		}

		public void TestFindRelatedCustomsJobFromHVLVShipment_ReturnsNullWhenDataObjectIsNotHVLV()
		{
			var consolDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			AssertEquals("Data object is not HVLV", false, consolDataObject.IsHVLV());

			var newFactory = new BusinessObjectFactory();
			var foundDummyJob = consolDataObject.FindRelatedCustomsJobFromHVLVShipment<DummyBaseBusinessObject>(newFactory);

			AssertNull("Should find no existing dummy job when universal shipment is not HVLV", foundDummyJob);
		}

		public void TestFindRelatedCustomsJobFromHVLVShipment_ReturnsNullWhenNoMatchingShipment()
		{
			var consolDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			consolDataObject.DataContext = DataContextFactory.New();
			consolDataObject.DataContext.AddDataSource(DataContextType.ForwardingConsol, "TESTCONSOL");
			consolDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, "TESTSHIPMENT");

			var shipmentDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.ShipmentType = new CodeDescriptionPair() { Code = Core.Constants.ShipmentTypes.HighVolumeLowValue };

			consolDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment> { shipmentDataObject });

			AssertEquals("Data object is HVLV", true, consolDataObject.IsHVLV());

			var newFactory = new BusinessObjectFactory();
			var foundDummyJob = consolDataObject.FindRelatedCustomsJobFromHVLVShipment<DummyBaseBusinessObject>(newFactory);

			AssertNull("Should find no existing dummy job when no shipment job number provided in data source", foundDummyJob);

			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, "TESTSHIPMENT");

			foundDummyJob = consolDataObject.FindRelatedCustomsJobFromHVLVShipment<DummyBaseBusinessObject>(newFactory);
			AssertNull("Should find no existing dummy job when no matching shipment found with job number in data source", foundDummyJob);
		}

		public void TestFindRelatedCustomsJobFromHVLVShipment_ReturnsNullWhenNoRelatedCustomsJob()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;

			Factory.Save();

			var consolDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			consolDataObject.DataContext = DataContextFactory.New();
			consolDataObject.DataContext.AddDataSource(DataContextType.ForwardingConsol, consol.JK_MasterBillNum);
			consolDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, shipment.JS_UniqueConsignRef);

			var shipmentDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, shipment.JS_UniqueConsignRef);
			shipmentDataObject.ShipmentType = new CodeDescriptionPair() { Code = Core.Constants.ShipmentTypes.HighVolumeLowValue };

			consolDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment> { shipmentDataObject });

			AssertEquals("Data object is HVLV", true, consolDataObject.IsHVLV());

			var newFactory = new BusinessObjectFactory();
			BusinessObject foundCustomsJob = consolDataObject.FindRelatedCustomsJobFromHVLVShipment<DummyBaseBusinessObject>(newFactory);

			AssertNull("Should find no existing dummy job when no customs job linked to consignment header", foundCustomsJob);

			var dummyJob = Factory.NewWithValidTestData<DummyBaseBusinessObject>();

			var genPivot = Factory.New<IGenPivot>();
			genPivot.XX_RelationType = Core.Constants.GenPivotTypes.HighVolumeLowValue;
			genPivot.XX_Relation1ID = shipment.HVLVConsignmentHeader.PK;
			genPivot.XX_Relation1TableCode = HVLVConsignmentHeaderSchema.Constants.Prefix;
			genPivot.XX_Relation2ID = dummyJob.PK;
			genPivot.XX_Relation2TableCode = DummyBizoSchema.Constants.Prefix;

			foundCustomsJob = consolDataObject.FindRelatedCustomsJobFromHVLVShipment<CusInBondHeader>(newFactory);

			AssertNull("Should find no existing dummy job when type mismatch", foundCustomsJob);
		}

		public void TestFindRelatedCustomsJobFromHVLVShipment_DifferentManifestType_ShouldConsiderNoRelatedCustomsJob()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;

			Factory.Save();

			var consolDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			consolDataObject.DataContext = DataContextFactory.New();
			consolDataObject.DataContext.AddDataSource(DataContextType.ForwardingConsol, consol.JK_MasterBillNum);
			consolDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, shipment.JS_UniqueConsignRef);
			consolDataObject.MessageType = new CodeDescriptionPair { Code = ManifestTypeList.Codes.DepartureReport };

			var shipmentDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, shipment.JS_UniqueConsignRef);
			shipmentDataObject.ShipmentType = new CodeDescriptionPair() { Code = Core.Constants.ShipmentTypes.HighVolumeLowValue };

			consolDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment> { shipmentDataObject });

			AssertEquals("Data object is HVLV", true, consolDataObject.IsHVLV());

			var dummyJob = Factory.New<IAsycudaManifestHeader>();
			dummyJob.AMA_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			dummyJob.AMA_RN_NKCountry = Core.Constants.CountryCodes.Australia;

			var genPivot = Factory.New<IGenPivot>();
			genPivot.XX_RelationType = Core.Constants.GenPivotTypes.HighVolumeLowValue;
			genPivot.XX_Relation1ID = shipment.HVLVConsignmentHeader.PK;
			genPivot.XX_Relation1TableCode = HVLVConsignmentHeaderSchema.Constants.Prefix;
			genPivot.XX_Relation2ID = dummyJob.PK;
			genPivot.XX_Relation2TableCode = AsycudaManifestHeaderSchema.Constants.Prefix;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var foundCustomsJob = consolDataObject.FindRelatedCustomsJobFromHVLVShipment<AsycudaManifestHeader>(newFactory);

			AssertNull("Should find no existing dummy job when manifest type mismatch", foundCustomsJob);
		}

		public void TestUniversalCustomsMessagingEntryHeaders()
		{
			var declarationBO = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var entry1 = declarationBO.ActiveEntryHeaders.AddNew();
			entry1.CH_BGMReference = "REF1";
			var entry2 = declarationBO.ActiveEntryHeaders.AddNew();
			entry2.CH_BGMReference = "REF2";

			var writerConfiguration = new DeclarationDataObjectWriterConfigurationForTest();
			writerConfiguration.AdditionalWriterSettingActions += (write) => { write.SetEntryHeaderPKsToPopulate(writerConfiguration.PKsToPopulate); };

			var writer1 = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, declarationBO)));
			writerConfiguration.PKsToPopulate = null;
			var dataObject = declarationBO.GetUniversalShipment(writerConfiguration);
			AssertEquals(2, dataObject.EntryHeaderCollection.Count);

			writer1 = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, declarationBO)));
			writerConfiguration.PKsToPopulate = new List<ZGuid> { entry1.PK };
			dataObject = declarationBO.GetUniversalShipment(writerConfiguration);
			AssertEquals(1, dataObject.EntryHeaderCollection.Count);

			writerConfiguration.AdditionalWriterSettingActions += (write) => { write.SetEntryHeaderPKsToPopulate(null); };
			writer1 = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, declarationBO)));
			writerConfiguration.PKsToPopulate = new List<ZGuid> { entry1.PK };
			dataObject = declarationBO.GetUniversalShipment(writerConfiguration);
			AssertEquals(2, dataObject.EntryHeaderCollection.Count);
		}

		public class DeclarationDataObjectWriterConfigurationForTest : DeclarationDataObjectWriterConfiguration
		{
			public IEnumerable<ZGuid> PKsToPopulate { get; set; } = Enumerable.Empty<ZGuid>();
		}
	}
}

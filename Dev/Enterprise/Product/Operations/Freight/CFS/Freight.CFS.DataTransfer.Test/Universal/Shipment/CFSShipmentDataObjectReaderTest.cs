using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using CodeDescPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;

namespace Enterprise.Freight.CFS.DataTransfer.Universal.Testing
{
	public class CFSShipmentDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestSubShipmentWouldNotBeAddedToColoadCollectionIfParentExixt()
		{
			var coloadShipment = Factory.BOFactory.NewWithValidTestData<CFSShipment>();
			coloadShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			var shipment = Factory.BOFactory.NewWithValidTestData<CFSShipment>();
			shipment.JS_UniqueConsignRef = "S00002016";
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.JS_HouseBill = "HBL99955";
			shipment.JS_JS_ColoadMasterShipment = coloadShipment.PK;

			Factory.SaveForTesting();

			var subShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataSource = DataContextFactory.New();
			dataSource.AddDataSource(DataContextType.CFSShipment, "S00002016");
			subShipment.DataContext = dataSource;
			subShipment.WayBillNumber = "HBL99955";
			subShipment.WayBillType = new WayBillType { Code = "HWB", Description = "House Waybill" };

			var shipmentDataObject = CreateDataObject();
			shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment> { subShipment });

			var reader = new CFSShipmentDataObjectReader(shipmentDataObject, new TestErrorLogger(), Factory, null, null);

			AssertExceptionThrown<DataObjectReadFailureException>("Shipment (S00002016) is already linked to another job. XML rejected as an invalid link would be created between CLD and STD shipments."
				, () => reader.ReadIntoBusinessObject());
		}

		public void TestImportShipmentWithSameSubShipmentKey()
		{
			var shipment = Factory.BOFactory.NewWithValidTestData<CFSShipment>();
			shipment.JS_UniqueConsignRef = "S00001000";
			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			shipment.JS_HouseBill = "HBL99944";

			Factory.SaveForTesting();

			var subShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataSource = DataContextFactory.New();
			dataSource.AddDataSource(DataContextType.CFSShipment, "S00001000");
			subShipment.DataContext = dataSource;
			subShipment.WayBillNumber = "HBL99944";
			subShipment.WayBillType = new WayBillType { Code = "HWB", Description = "House Waybill" };

			var shipmentDataObject = CreateDataObject();
			shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment> { subShipment });

			var reader = new CFSShipmentDataObjectReader(shipmentDataObject, new TestErrorLogger(), Factory, null, null);

			AssertExceptionThrown<DataObjectReadFailureException>("Shipment (S00001000) cannot import itself as a sub shipment."
				, () => reader.ReadIntoBusinessObject());
		}

		public void TestReading()
		{
			var dataObject = CreateDataObject();
			var shipment = new CFSShipmentDataObjectReader(dataObject, new TestErrorLogger(), Factory, null, null).ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("CLD", shipment.JS_ShipmentType);
				AssertEquals("SEA", shipment.JS_TransportMode);
				AssertEquals("HBL99944", shipment.JS_HouseBill);
				AssertEquals("AUMEL", shipment.JS_RL_NKOrigin);
				AssertEquals("USCHI", shipment.JS_RL_NKDestination);
				AssertEquals("AGREF88882", shipment.JS_ConsolReference);
				AssertEquals("ABCD9292", shipment.JS_InterimReceipt);
				AssertEquals("CTG444444", shipment.JS_CartageWaybill);
				AssertEquals("DRUGS", shipment.JS_GoodsDescription);
				AssertEquals(Factory.Load<RefServiceLevel>(Env.Registry.ServiceLevel).RS_Code, shipment.JS_RS_NKServiceLevel);
				AssertEquals("DOWNTHERD", shipment.JS_WarehouseLocation);
				AssertEquals(4, shipment.JS_OuterPacks);
				AssertEquals("BOX", shipment.JS_F3_NKPackType);
				AssertEquals(new ZDecimal(5), shipment.JS_ActualVolume);
				AssertEquals("M3", shipment.JS_UnitOfVolume);
				AssertEquals(new ZDecimal(6), shipment.JS_ActualWeight);
				AssertEquals("MG", shipment.JS_UnitOfWeight);
				AssertEquals(true, shipment.JS_TranshipToOtherCFS);

				AssertEquals(new ZDateTime(2015, 1, 4), shipment.JS_A_BKD);

				AssertEquals("27", shipment.CustomsEntryNumber);

				AssertEquals(new ZDateTime(2015, 1, 20), shipment.DocsAndCartage.JP_LCLAvailable);
				AssertEquals(new ZDateTime(2015, 1, 25), shipment.DocsAndCartage.JP_LCLStorageCommences);
				AssertEquals(2, shipment.DocsAndCartage.Services.Count);
				AssertEquals("CLN", shipment.DocsAndCartage.Services.Cast<JobService>().ToArray()[0].ES_ServiceCode);
				AssertEquals("FUM", shipment.DocsAndCartage.Services.Cast<JobService>().ToArray()[1].ES_ServiceCode);

				AssertEquals(forwarder.PK, shipment.JS_OH_HandledOnBehalfOfForwarder);
				AssertEquals(cnrDocumentary.PK, shipment.DocAddresses.FindByDocAddressType(DocAddressType.ConsignorDocumentaryAddress).OrganisationPK);
				AssertEquals(cneDocumentary.PK, shipment.DocAddresses.FindByDocAddressType(DocAddressType.ConsigneeDocumentaryAddress).OrganisationPK);
				AssertEquals(cartageCo.MainAddress.PK, shipment.JS_OA_CartageCoAddr);
				AssertEquals(cnrPicDlv.PK, shipment.DocAddresses.FindByDocAddressType(DocAddressType.ConsignorPickupDeliveryAddress).OrganisationPK);
				AssertEquals(cnePicDlv.PK, shipment.DocAddresses.FindByDocAddressType(DocAddressType.ConsigneePickupDeliveryAddress).OrganisationPK);

				AssertEquals(2, shipment.OuterPackLines.Count);
				AssertEquals("PACK1", shipment.OuterPackLines[0].JL_MarksAndNumbers);
				AssertEquals("PACK2", shipment.OuterPackLines[1].JL_MarksAndNumbers);

				AssertEquals(2, shipment.CoLoadShipments.Count);
				AssertEquals("HBL111", shipment.CoLoadShipments[0].JS_HouseBill);
				AssertEquals("HBL222", shipment.CoLoadShipments[1].JS_HouseBill);
			});
		}

		Shipment CreateDataObject()
		{
			var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			dataObject.ShipmentType = new CodeDescPair { Code = "CLD", Description = "CoLoad Master" };
			dataObject.TransportMode = new CodeDescPair { Code = "SEA", Description = "Sea Transport" };
			dataObject.WayBillNumber = "HBL99944";
			dataObject.PortOfOrigin = new UNLOCO { Code = "AUMEL", Name = "Smellbourne" };
			dataObject.PortOfDestination = new UNLOCO { Code = "USCHI", Name = "Chicago" };
			dataObject.AgentsReference = "AGREF88882";
			dataObject.InterimReceiptNumber = "ABCD9292";
			dataObject.CartageWaybillNumber = "CTG444444";
			dataObject.GoodsDescription = "DRUGS";
			dataObject.WarehouseLocation = "DOWNTHERD";
			dataObject.OuterPacks = 4;
			dataObject.OuterPacksPackageType = new PackageType { Code = "BOX", Description = "Magical Box" };
			dataObject.TotalVolume = 5;
			dataObject.TotalVolumeUnit = new UnitOfVolume { Code = "M3", Description = "Metres Cubed" };
			dataObject.TotalWeight = 6;
			dataObject.TotalWeightUnit = new UnitOfWeight { Code = "MG", Description = "Milligrams" };
			dataObject.TranshipToOtherCFS = true;

			dataObject.SetDateCollection(() => new List<Date> { new Date { Type = DateType.BookingConfirmed, IsEstimate = false, Value = new ZDateTime(2015, 1, 4) } });

			dataObject.SetEntryNumberCollection(() => new List<EntryNumber> { new EntryNumber { Number = "27" } });

			dataObject.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance) { LCLAvailable = new ZDateTime(2015, 1, 20), LCLStorageCommences = new ZDateTime(2015, 1, 25) };
			dataObject.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>
			{
				new AdditionalService { ServiceCode = new CodeDescPair { Code = "CLN", Description = "Cleaning" } },
				new AdditionalService { ServiceCode = new CodeDescPair { Code = "FUM", Description = "Fumigation" } }
			});

			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress { AddressType = AddressTypes.Forwarder, OrganizationCode = "FWDORG", Address1 = "84 Forwarder St" },
				new OrganizationAddress { AddressType = nameof(DocAddressType.ConsignorDocumentaryAddress), OrganizationCode = "CNRDOC", Address1 = "14 Consignor Lane" },
				new OrganizationAddress { AddressType = nameof(DocAddressType.ConsigneeDocumentaryAddress), OrganizationCode = "CNEDOC", Address1 = "15 Consignee Rd" },
				new OrganizationAddress { AddressType = nameof(DocAddressType.LocalCartageAddress1), OrganizationCode = "CTGORG", Address1 = "162 Cartage Ave" },
				new OrganizationAddress { AddressType = nameof(DocAddressType.ConsignorPickupDeliveryAddress), OrganizationCode = "CNRPICDLV", Address1 = "2 Consignor Boulevarde" },
				new OrganizationAddress { AddressType = nameof(DocAddressType.ConsigneePickupDeliveryAddress), OrganizationCode = "CNEPICDLV", Address1 = "3 Consignee Crescent" }
			});

			dataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>
			{
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { MarksAndNos = "PACK1" },
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { MarksAndNos = "PACK2" }
			});
			dataObject.PackingLineCollection.Content = CollectionContent.Complete;

			dataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>
			{
				new Shipment { WayBillNumber = "HBL111" },
				new Shipment { WayBillNumber = "HBL222" }
			});

			return dataObject;
		}

		public void TestReading_ContainerPacking()
		{
			var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.WayBillNumber = "HBL123";
			dataObject.SetContainerCollection(() => new DataObjectList<Container> { new Container { ContainerNumber = "CONT1111111", Link = 1 } });
			dataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { MarksAndNos = "PACK1", ContainerLink = 1 } });
			dataObject.PackingLineCollection.Content = CollectionContent.Complete;

			var shipment = new CFSShipmentDataObjectReader(dataObject, new TestErrorLogger(), Factory, null, null).ReadIntoBusinessObject();

			AssertEquals(1, shipment.Consols.Count);
			AssertEquals(1, shipment.Consols[0].Containers.Count);
			AssertEquals("CONT1111111", shipment.Consols[0].Containers[0].JC_ContainerNum);
			AssertEquals(1, shipment.Consols[0].Containers[0].PackLines.Count);
			AssertEquals("PACK1", shipment.Consols[0].Containers[0].PackLines[0].JL_MarksAndNumbers);

			dataObject.ContainerCollection.Add(new Container { ContainerNumber = "CONT2222222", Link = 2 });
			dataObject.PackingLineCollection.Add(new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { MarksAndNos = "PACK2", ContainerLink = 2 });
			shipment = new CFSShipmentDataObjectReader(dataObject, new TestErrorLogger(), Factory, null, null).ReadIntoBusinessObject();

			AssertEquals("Re-import should use existing consol", 1, shipment.Consols.Count);
			AssertEquals(2, shipment.Consols[0].Containers.Count);
			AssertEquals("CONT1111111", shipment.Consols[0].Containers[0].JC_ContainerNum);
			AssertEquals("CONT2222222", shipment.Consols[0].Containers[1].JC_ContainerNum);
			AssertEquals(1, shipment.Consols[0].Containers[0].PackLines.Count);
			AssertEquals("PACK1", shipment.Consols[0].Containers[0].PackLines[0].JL_MarksAndNumbers);
			AssertEquals(1, shipment.Consols[0].Containers[1].PackLines.Count);
			AssertEquals("PACK2", shipment.Consols[0].Containers[1].PackLines[0].JL_MarksAndNumbers);
		}

		public void TestAdditionalServicesComplete()
		{
			var resultShipment = PrepareAndReadShipmentObject(addCollectionContent: true, CollectionContent.Complete);

			AssertEquals(2, resultShipment.DocsAndCartage.Services.Count);
			AssertEquals(Constants.FreightServiceType.Codes.Fumigation, resultShipment.DocsAndCartage.Services.Cast<JobService>().ToArray()[0].ES_ServiceCode);
			AssertEquals(Constants.FreightServiceType.Codes.Washing, resultShipment.DocsAndCartage.Services.Cast<JobService>().ToArray()[1].ES_ServiceCode);
		}

		public void TestAdditionalServicesPartial()
		{
			var resultShipment = PrepareAndReadShipmentObject(addCollectionContent: true, CollectionContent.Partial);

			AssertEquals(3, resultShipment.DocsAndCartage.Services.Count);
			AssertEquals(Constants.FreightServiceType.Codes.Fumigation, resultShipment.DocsAndCartage.Services.Cast<JobService>().ToArray()[0].ES_ServiceCode);
			AssertEquals(Constants.FreightServiceType.Codes.Cleaning, resultShipment.DocsAndCartage.Services.Cast<JobService>().ToArray()[1].ES_ServiceCode);
			AssertEquals(Constants.FreightServiceType.Codes.Washing, resultShipment.DocsAndCartage.Services.Cast<JobService>().ToArray()[2].ES_ServiceCode);
		}

		public void TestAdditionalServicesWithoutContentType_ShouldWorkLikePartial()
		{
			var resultShipment = PrepareAndReadShipmentObject(addCollectionContent: false);

			AssertEquals(3, resultShipment.DocsAndCartage.Services.Count);
			AssertEquals(Constants.FreightServiceType.Codes.Fumigation, resultShipment.DocsAndCartage.Services.Cast<JobService>().ToArray()[0].ES_ServiceCode);
			AssertEquals(Constants.FreightServiceType.Codes.Cleaning, resultShipment.DocsAndCartage.Services.Cast<JobService>().ToArray()[1].ES_ServiceCode);
			AssertEquals(Constants.FreightServiceType.Codes.Washing, resultShipment.DocsAndCartage.Services.Cast<JobService>().ToArray()[2].ES_ServiceCode);
		}

		CFSShipment PrepareAndReadShipmentObject(bool addCollectionContent, CollectionContent collectionContent = CollectionContent.Complete)
		{
			var shipment = Factory.BOFactory.NewWithValidTestData<CFSShipment>();
			shipment.JS_UniqueConsignRef = "S00001000";
			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			shipment.JS_HouseBill = "HBL99944";

			var fumService = shipment.DocsAndCartage.Services.AddNew();
			fumService.ShouldPopulateServiceId = false;
			fumService.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;

			var clnService = shipment.DocsAndCartage.Services.AddNew();
			clnService.ShouldPopulateServiceId = false;
			clnService.ES_ServiceCode = Constants.FreightServiceType.Codes.Cleaning;

			Factory.SaveForTesting();

			var shipmentToImport = CreateDataObject();
			var dataSource = DataContextFactory.New();
			dataSource.AddDataSource(DataContextType.CFSShipment, "S00001000");
			shipmentToImport.DataContext = dataSource;

			var fumAdditionalService = new AdditionalService
			{
				ServiceCode = new CodeDescPair { Code = Core.Constants.FreightServiceType.Codes.Fumigation, Description = Core.Constants.FreightServiceType.Descriptions.Fumigation },
				Booked = new ZDateTime(2011, 6, 1),
				Completed = new ZDateTime(2011, 6, 2),
				Contractor = null,
			};

			var wshAdditionalService = new AdditionalService
			{
				ServiceCode = new CodeDescPair { Code = Core.Constants.FreightServiceType.Codes.Washing, Description = Core.Constants.FreightServiceType.Descriptions.Washing },
				Booked = new ZDateTime(2011, 6, 1),
				Completed = new ZDateTime(2011, 6, 2),
				Contractor = null,
			};

			var additionalServicesDataObject = new DataObjectList<AdditionalService>();
			additionalServicesDataObject.Add(fumAdditionalService);
			additionalServicesDataObject.Add(wshAdditionalService);

			if (addCollectionContent)
			{
				additionalServicesDataObject.Content = collectionContent;
			}

			shipmentToImport.LocalProcessing.SetAdditionalServiceCollection(() => additionalServicesDataObject);
			var reader = new CFSShipmentDataObjectReader(shipmentToImport, new TestErrorLogger(), Factory, null, null);
			var resultShipment = reader.ReadIntoBusinessObject();
			return resultShipment;
		}

		public void TestImportShipmentWithDuplicatedAddressType()
		{
			var shipmentDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			var address1 = new OrganizationAddress();
			address1.AddressType = "ConsignorDocumentaryAddress";

			var address2 = new OrganizationAddress();
			address2.AddressType = "ConsignorDocumentaryAddress";

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { address1, address2 });

			var reader = new CFSShipmentDataObjectReader(shipmentDataObject, new TestErrorLogger(), Factory, null, null);

			AssertExceptionThrown<DataObjectReadFailureException>("One OrganizationAddressCollection with same AddressType shoud have exception thrown", () => reader.ReadIntoBusinessObject());
		}

		#region Implementation

		OrgHeader forwarder;
		OrgHeader cnrDocumentary;
		OrgHeader cneDocumentary;
		OrgHeader cartageCo;
		OrgHeader cnrPicDlv;
		OrgHeader cnePicDlv;

		protected override void SetUp()
		{
			base.SetUp();

			forwarder = CreateOrg("FWDORG", "84 Forwarder St");
			cnrDocumentary = CreateOrg("CNRDOC", "14 Consignor Lane");
			cneDocumentary = CreateOrg("CNEDOC", "15 Consignee Rd");
			cartageCo = CreateOrg("CTGORG", "162 Cartage Ave");
			cnrPicDlv = CreateOrg("CNRPICDLV", "2 Consignor Boulevarde");
			cnePicDlv = CreateOrg("CNEPICDLV", "3 Consignee Crescent");

			Factory.SaveForTesting();
		}

		OrgHeader CreateOrg(string companyCode, string address)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = companyCode;
			org.MainAddress.OA_Address1 = address;

			return org;
		}

		#endregion
	}
}

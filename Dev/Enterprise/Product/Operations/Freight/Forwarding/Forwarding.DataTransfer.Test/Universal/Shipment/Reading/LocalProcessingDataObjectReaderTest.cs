using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Core.Constants;
using Order = Enterprise.Freight.Forwarding.Orders.Business.Order;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class LocalProcessingDataObjectReaderTest : OrganizationAddressTestHelper
	{
		public void TestPickupRequiredByDateIsTakenFromDeliveryRequiredByWhenDataSourceIsWarehouseOrder()
		{
			var shipmentDataObject = new UniversalShipment { DataContext = DataContextFactory.New() };
			shipmentDataObject.DataContext.AddDataSource(DataContextType.WarehouseOrder, "");
			shipmentDataObject.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.LocalProcessing.DeliveryRequiredBy = new ZDateTime(2013, 1, 1);
			shipmentDataObject.LocalProcessing.PickupRequiredBy = new ZDateTime(2013, 1, 2);

			var shipment = Factory.New<ForwardingShipment>();
			var reader1 = new LocalProcessingDataObjectReader(shipmentDataObject, Logger, Factory);
			var jobDocsAndCartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(shipment);
			reader1.PopulateBusinessObject(jobDocsAndCartage);
			AssertEquals(new ZDateTime(2013, 1, 1), jobDocsAndCartage.JP_PickupRequiredBy);
			AssertEquals(ZDateTime.Empty, jobDocsAndCartage.JP_DeliveryRequiredBy);

			shipmentDataObject.DataContext = DataContextFactory.New();
			var reader2 = new LocalProcessingDataObjectReader(shipmentDataObject, Logger, Factory);
			reader2.PopulateBusinessObject(jobDocsAndCartage);
			AssertEquals(new ZDateTime(2013, 1, 2), jobDocsAndCartage.JP_PickupRequiredBy);
			AssertEquals(new ZDateTime(2013, 1, 1), jobDocsAndCartage.JP_DeliveryRequiredBy);
		}

		public void TestUpdateOrderItemsAndSortBySequence()
		{
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			var orderNumberDataObject1 = new OrderNumber { Sequence = 1, OrderReference = "FF" };
			var orderNumberDataObject2 = new OrderNumber { Sequence = 2, OrderReference = "AA" };
			var orderNumberDataObject3 = new OrderNumber { Sequence = 3, OrderReference = "AA" };
			var orderNumberDataObject4 = new OrderNumber { Sequence = 4, OrderReference = "AA" };
			shipmentDataObject.LocalProcessing.SetOrderNumberCollection(() => new DataObjectList<OrderNumber>(new[] { orderNumberDataObject1, orderNumberDataObject2, orderNumberDataObject3, orderNumberDataObject4 }));

			var shipment = Factory.New<ForwardingShipment>();
			var item1 = shipment.DocsAndCartage.OrderItems.AddNew();
			item1.JT_OrderReference = "AA";
			item1.JT_Sequence = 1;
			var item2 = shipment.DocsAndCartage.OrderItems.AddNew();
			item2.JT_OrderReference = "AA";
			item2.JT_Sequence = 2;
			var item3 = shipment.DocsAndCartage.OrderItems.AddNew();
			item3.JT_OrderReference = "BB";
			item3.JT_Sequence = 3;
			var item4 = shipment.DocsAndCartage.OrderItems.AddNew();
			item4.JT_OrderReference = "CC";
			item4.JT_Sequence = 4;

			var reader1 = new LocalProcessingDataObjectReader(shipmentDataObject, Logger, Factory);
			var jobDocsAndCartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(shipment);
			reader1.PopulateBusinessObject(jobDocsAndCartage);
			AssertEquals(4, jobDocsAndCartage.OrderItems.Count);
			AssertEquals("AA", jobDocsAndCartage.OrderItems[0].JT_OrderReference);
			AssertEquals("2", jobDocsAndCartage.OrderItems[0].JT_Sequence.ToString());
			AssertEquals("AA", jobDocsAndCartage.OrderItems[1].JT_OrderReference);
			AssertEquals("3", jobDocsAndCartage.OrderItems[1].JT_Sequence.ToString());
			AssertEquals("AA", jobDocsAndCartage.OrderItems[2].JT_OrderReference);
			AssertEquals("4", jobDocsAndCartage.OrderItems[2].JT_Sequence.ToString());
			AssertEquals("FF", jobDocsAndCartage.OrderItems[3].JT_OrderReference);
			AssertEquals("1", jobDocsAndCartage.OrderItems[3].JT_Sequence.ToString());
		}

		public void TestUpdateOrderItemsPartialCollectionNewOrderRefNoSequence()
		{
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			var orderNumberDataObject1 = new OrderNumber { OrderReference = "DD" };
			var orderNumberList = new DataObjectList<OrderNumber>(new[] { orderNumberDataObject1 });
			orderNumberList.Content = CollectionContent.Partial;
			shipmentDataObject.LocalProcessing.SetOrderNumberCollection(() => orderNumberList);

			var shipment = Factory.New<ForwardingShipment>();
			var item1 = shipment.DocsAndCartage.OrderItems.AddNew();
			item1.JT_OrderReference = "AA";
			item1.JT_Sequence = 1;
			var item2 = shipment.DocsAndCartage.OrderItems.AddNew();
			item2.JT_OrderReference = "FF";
			item2.JT_Sequence = 2;

			var reader1 = new LocalProcessingDataObjectReader(shipmentDataObject, Logger, Factory);
			var jobDocsAndCartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(shipment);
			reader1.PopulateBusinessObject(jobDocsAndCartage);
			AssertEquals(3, jobDocsAndCartage.OrderItems.Count);
			AssertEquals("AA", jobDocsAndCartage.OrderItems[0].JT_OrderReference);
			AssertEquals("1", jobDocsAndCartage.OrderItems[0].JT_Sequence.ToString());
			AssertEquals("DD", jobDocsAndCartage.OrderItems[1].JT_OrderReference);
			AssertEquals("0", jobDocsAndCartage.OrderItems[1].JT_Sequence.ToString());
			AssertEquals("FF", jobDocsAndCartage.OrderItems[2].JT_OrderReference);
			AssertEquals("2", jobDocsAndCartage.OrderItems[2].JT_Sequence.ToString());
		}

		public void TestUpdateOrderItemsPartialCollectionNewOrderRefWithSequence()
		{
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			var orderNumberDataObject1 = new OrderNumber { Sequence = 1, OrderReference = "DD" };
			var orderNumberList = new DataObjectList<OrderNumber>(new[] { orderNumberDataObject1 });
			orderNumberList.Content = CollectionContent.Partial;
			shipmentDataObject.LocalProcessing.SetOrderNumberCollection(() => orderNumberList);

			var shipment = Factory.New<ForwardingShipment>();
			var item1 = shipment.DocsAndCartage.OrderItems.AddNew();
			item1.JT_OrderReference = "AA";
			item1.JT_Sequence = 1;
			var item2 = shipment.DocsAndCartage.OrderItems.AddNew();
			item2.JT_OrderReference = "FF";
			item2.JT_Sequence = 2;

			var reader1 = new LocalProcessingDataObjectReader(shipmentDataObject, Logger, Factory);
			var jobDocsAndCartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(shipment);
			reader1.PopulateBusinessObject(jobDocsAndCartage);
			AssertEquals(3, jobDocsAndCartage.OrderItems.Count);
			AssertEquals("AA", jobDocsAndCartage.OrderItems[0].JT_OrderReference);
			AssertEquals("1", jobDocsAndCartage.OrderItems[0].JT_Sequence.ToString());
			AssertEquals("DD", jobDocsAndCartage.OrderItems[1].JT_OrderReference);
			AssertEquals("1", jobDocsAndCartage.OrderItems[1].JT_Sequence.ToString());
			AssertEquals("FF", jobDocsAndCartage.OrderItems[2].JT_OrderReference);
			AssertEquals("2", jobDocsAndCartage.OrderItems[2].JT_Sequence.ToString());
		}

		public void TestUpdateOrderItemsPartialCollectionSameOrderReference()
		{
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			var orderNumberDataObject1 = new OrderNumber { Sequence = 1, OrderReference = "AA" };
			var orderNumberDataObject2 = new OrderNumber { Sequence = 2, OrderReference = "BB" };
			var orderNumberDataObject3 = new OrderNumber { Sequence = 3, OrderReference = "FF" };
			var orderNumberList = new DataObjectList<OrderNumber>(new[] { orderNumberDataObject1, orderNumberDataObject2, orderNumberDataObject3 });
			orderNumberList.Content = CollectionContent.Partial;
			shipmentDataObject.LocalProcessing.SetOrderNumberCollection(() => orderNumberList);

			var shipment = Factory.New<ForwardingShipment>();
			var item1 = shipment.DocsAndCartage.OrderItems.AddNew();
			item1.JT_OrderReference = "AA";
			item1.JT_Sequence = 1;
			var item2 = shipment.DocsAndCartage.OrderItems.AddNew();
			item2.JT_OrderReference = "FF";
			item2.JT_Sequence = 2;

			var reader1 = new LocalProcessingDataObjectReader(shipmentDataObject, Logger, Factory);
			var jobDocsAndCartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(shipment);
			reader1.PopulateBusinessObject(jobDocsAndCartage);
			AssertEquals(3, jobDocsAndCartage.OrderItems.Count);
			AssertEquals("AA", jobDocsAndCartage.OrderItems[0].JT_OrderReference);
			AssertEquals("1", jobDocsAndCartage.OrderItems[0].JT_Sequence.ToString());
			AssertEquals("BB", jobDocsAndCartage.OrderItems[1].JT_OrderReference);
			AssertEquals("2", jobDocsAndCartage.OrderItems[1].JT_Sequence.ToString());
			AssertEquals("FF", jobDocsAndCartage.OrderItems[2].JT_OrderReference);
			AssertEquals("2", jobDocsAndCartage.OrderItems[2].JT_Sequence.ToString());
		}

		public void TestUpdateService_CompleteAdditionalServices()
		{
			var jobDocsAndCartage = PrepareAndReadAdditionalServices(addCollectionContent: true, CollectionContent.Complete);

			AssertEquals("jobDocsAndCartage.Services.Count", 3, jobDocsAndCartage.Services.Count);

			AssertServiceBO(jobDocsAndCartage.Services[0], "AAA1", new ZDateTime(2016, 08, 14), new ZDateTime(2016, 9, 2), "AAA", "Note to others2");
			AssertServiceBO(jobDocsAndCartage.Services[1], "KKK1", new ZDateTime(2016, 08, 16), new ZDateTime(2016, 9, 3), "KKK", "Note to others3");
			AssertServiceBO(jobDocsAndCartage.Services[2], "DDD1", new ZDateTime(2016, 08, 17), new ZDateTime(2016, 9, 1), "DDD", "Note to others1");
		}

		public void TestUpdateService_PartialAdditionalServices()
		{
			var jobDocsAndCartage = PrepareAndReadAdditionalServices(addCollectionContent: true, CollectionContent.Partial);

			AssertEquals("jobDocsAndCartage.Services.Count", 4, jobDocsAndCartage.Services.Count);

			AssertServiceBO(jobDocsAndCartage.Services[0], "AAA1", new ZDateTime(2016, 08, 14), new ZDateTime(2016, 9, 2), "AAA", "Note to others2");
			AssertServiceBO(jobDocsAndCartage.Services[1], "BBB1", new ZDateTime(2016, 08, 13), new ZDateTime(2017, 01, 02), "BBB", "Two");
			AssertServiceBO(jobDocsAndCartage.Services[2], "KKK1", new ZDateTime(2016, 08, 16), new ZDateTime(2016, 9, 3), "KKK", "Note to others3");
			AssertServiceBO(jobDocsAndCartage.Services[3], "DDD1", new ZDateTime(2016, 08, 17), new ZDateTime(2016, 9, 1), "DDD", "Note to others1");
		}

		public void TestUpdateService_ByDefaultShouldBePartial()
		{
			var jobDocsAndCartage = PrepareAndReadAdditionalServices(addCollectionContent: false);

			AssertEquals("jobDocsAndCartage.Services.Count", 4, jobDocsAndCartage.Services.Count);

			AssertServiceBO(jobDocsAndCartage.Services[0], "AAA1", new ZDateTime(2016, 08, 14), new ZDateTime(2016, 9, 2), "AAA", "Note to others2");
			AssertServiceBO(jobDocsAndCartage.Services[1], "BBB1", new ZDateTime(2016, 08, 13), new ZDateTime(2017, 01, 02), "BBB", "Two");
			AssertServiceBO(jobDocsAndCartage.Services[2], "KKK1", new ZDateTime(2016, 08, 16), new ZDateTime(2016, 9, 3), "KKK", "Note to others3");
			AssertServiceBO(jobDocsAndCartage.Services[3], "DDD1", new ZDateTime(2016, 08, 17), new ZDateTime(2016, 9, 1), "DDD", "Note to others1");
		}

		JobDocsAndCartage PrepareAndReadAdditionalServices(bool addCollectionContent, CollectionContent collectionContent = CollectionContent.Complete)
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var locationAddress = new OrganisationDataObjectReader(GetLocalClientAddress(), Logger, Factory).GetMatchedOrNewForTesting();
			var contractorAddress = new OrganisationDataObjectReader(GetNewAddressData_INTHEMSYD(nameof(DocAddressType.Contractor)), Logger, Factory).GetMatchedOrNewForTesting();
			Factory.SaveForTesting();

			var serviceDataObject1 = new AdditionalService()
			{
				Booked = new ZDate(2016, 08, 17),
				ServiceCode = new CodeDescriptionPair { Code = "DDD", Description = "DDD1" },
				Completed = new ZDateTime(2016, 09, 01),
				References = "Refer to the reference 1",
				ServiceCount = 45.1m,
				ServiceNote = "Note to others1"
			};
			var serviceDataObject2 = new AdditionalService()
			{
				Booked = new ZDate(2016, 08, 14),
				ServiceCode = new CodeDescriptionPair { Code = "AAA", Description = "AAA1" },
				Completed = new ZDateTime(2016, 09, 02),
				References = "Refer to the reference 2",
				ServiceCount = 45.2m,
				ServiceNote = "Note to others2"
			};
			var serviceDataObject3 = new AdditionalService()
			{
				Booked = new ZDate(2016, 08, 16),
				ServiceCode = new CodeDescriptionPair { Code = "KKK", Description = "KKK1" },
				Completed = new ZDateTime(2016, 09, 03),
				References = "Refer to the reference 3",
				ServiceCount = 45.3m,
				ServiceNote = "Note to others3"
			};

			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipmentDataObject.LocalProcessing = SetupLocalProcessing(new CodeDescriptionPair() { Code = "AAA", Description = "AAA1" }, new CodeDescriptionPair() { Code = "KKK", Description = "KKK1" }, new CodeDescriptionPair() { Code = "DDD", Description = "CCC1" }, new CodeDescriptionPair() { Code = "BBB", Description = "BBB1" });
			var additionalServicesDataObject = new DataObjectList<AdditionalService>(new[] { serviceDataObject1, serviceDataObject2, serviceDataObject3 });

			if (addCollectionContent)
			{
				additionalServicesDataObject.Content = collectionContent;
			}

			shipmentDataObject.LocalProcessing.SetAdditionalServiceCollection(() => additionalServicesDataObject);

			Logger.ClearLogs();

			var shipment = Factory.New<ForwardingShipment>();
			var service1 = shipment.DocsAndCartage.Services.AddNew();
			service1.ES_Booked = new ZDate(2016, 08, 15);
			service1.ES_Completed = new ZDate(2017, 01, 01);
			service1.ES_References = "ShipmentService1";
			service1.ES_ServiceCount = 11;
			service1.ES_ServiceCode = "AAA";
			service1.ES_ServiceNote = "One";
			service1.ShouldPopulateServiceId = false;
			var service2 = shipment.DocsAndCartage.Services.AddNew();
			service2.ES_Booked = new ZDate(2016, 08, 13);
			service2.ES_Completed = new ZDate(2017, 01, 02);
			service2.ES_References = "ShipmentService2";
			service2.ES_ServiceCount = 22;
			service2.ES_ServiceCode = "BBB";
			service2.ES_ServiceNote = "Two";
			service2.ShouldPopulateServiceId = false;
			var service3 = shipment.DocsAndCartage.Services.AddNew();
			service3.ES_Booked = new ZDate(2016, 08, 14);
			service3.ES_Completed = new ZDate(2017, 01, 03);
			service3.ES_References = "ShipmentService3";
			service3.ES_ServiceCount = 33;
			service3.ES_ServiceCode = "KKK";
			service3.ES_ServiceNote = "Three";
			service3.ShouldPopulateServiceId = false;

			var reader = new LocalProcessingDataObjectReader(shipmentDataObject, Logger, Factory);
			var jobDocsAndCartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(shipment);
			reader.PopulateBusinessObject(jobDocsAndCartage);
			return jobDocsAndCartage;
		}

		void AssertServiceBO(MasterFiles.Business.JobService serviceBO, string desc, ZDateTime booked, ZDateTime completed, string code, string note)
		{
			AssertEquals(desc + " Booked", serviceBO.ES_Booked, booked);
			AssertEquals(desc + " Completed", serviceBO.ES_Completed, completed);
			AssertEquals(desc + " Reference", serviceBO.ES_ServiceCode, code);
			AssertEquals(desc + " Note", serviceBO.ES_ServiceNote, note);
		}

		public void TestOrderItemsAreAddedEvenWhenShipmentHasAttachedOrders()
		{
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			var orderNumberDataObject1 = new OrderNumber { Sequence = 1, OrderReference = "REFERME1" };
			var orderNumberDataObject2 = new OrderNumber { Sequence = 2, OrderReference = "REFERME2" };
			shipmentDataObject.LocalProcessing.SetOrderNumberCollection(() => new DataObjectList<OrderNumber>(new[] { orderNumberDataObject1, orderNumberDataObject2 }));

			var shipment = Factory.New<ForwardingShipment>();
			var item1 = shipment.DocsAndCartage.OrderItems.AddNew();
			item1.JT_OrderReference = "REFERME1";
			item1.JT_Sequence = 1;
			var item2 = shipment.DocsAndCartage.OrderItems.AddNew();
			item2.JT_OrderReference = "REFERME2";
			item2.JT_Sequence = 2;

			var reader = new LocalProcessingDataObjectReader(shipmentDataObject, Logger, Factory);
			var jobDocsAndCartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(shipment);
			reader.PopulateBusinessObject(jobDocsAndCartage);
			AssertEquals("REFERME1,REFERME2", jobDocsAndCartage.JP_OrderItemsAsString);

			jobDocsAndCartage.OrderItems.RemoveAndDeleteAll();
			shipment.GenericOrders.Add(Factory.New<Order>());

			var newReader = new LocalProcessingDataObjectReader(shipmentDataObject, Logger, Factory);
			newReader.PopulateBusinessObject(jobDocsAndCartage);
			AssertEquals("REFERME1,REFERME2", jobDocsAndCartage.JP_OrderItemsAsString);
		}

		public void TestBasicLocalProcessingFieldMappings()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var locationAddress = new OrganisationDataObjectReader(GetLocalClientAddress(), Logger, Factory).GetMatchedOrNewForTesting();
			var contractorAddress = new OrganisationDataObjectReader(GetNewAddressData_INTHEMSYD(nameof(DocAddressType.Contractor)), Logger, Factory).GetMatchedOrNewForTesting();
			Factory.SaveForTesting();

			var serviceDataObject1 = AdditionalServiceDataObjectReaderTest.SetupAdditionalService();
			var serviceDataObject2 = new AdditionalService()
			{
				Booked = new ZDateTime(2012, 1, 1),
				Completed = new ZDateTime(2012, 1, 2),
				Contractor = serviceDataObject1.Contractor,
				Duration = TimeSpan.FromDays(2),
				Location = serviceDataObject1.Location,
				References = "Refer to the reference 2",
				ServiceCode = new CodeDescriptionPair { Code = "NOM", Description = "Not Manage" },
				ServiceCount = 45.6m,
				ServiceNote = "Note to others",
				SubLocation = "Sub Location 2"
			};
			var orderNumberDataObject1 = new OrderNumber { Sequence = 1, OrderReference = "REFERME1" };
			var orderNumberDataObject2 = new OrderNumber { Sequence = 2, OrderReference = "REFERME2" };

			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipmentDataObject.LocalProcessing = SetupLocalProcessing(new CodeDescriptionPair() { Code = "BOB", Description = "THE BUILDER" }, new CodeDescriptionPair() { Code = "WEN", Description = "THE DESTROYER" }, new CodeDescriptionPair() { Code = "JOE", Description = "THE PEACEMAKER" }, new CodeDescriptionPair() { Code = "JAY", Description = "THE LAZY" });
			shipmentDataObject.LocalProcessing.SetOrderNumberCollection(() => new DataObjectList<OrderNumber>(new[] { orderNumberDataObject1, orderNumberDataObject2 }));
			shipmentDataObject.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>(new[] { serviceDataObject1, serviceDataObject2 }));
			Logger.ClearLogs();
			var reader = new LocalProcessingDataObjectReader(shipmentDataObject, Logger, Factory);
			var jobDocsAndCartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(Factory.New<ForwardingShipment>());
			reader.PopulateBusinessObject(jobDocsAndCartage);

			CombineAssertions(delegate
			{
				AssertContents(jobDocsAndCartage, "BOB", "WEN", "JOE", "JAY");

				AssertEquals("jobDocsAndCartage.Services.Count", 2, jobDocsAndCartage.Services.Count);
				var serviceBO1 = jobDocsAndCartage.Services[0];
				var serviceBO2 = jobDocsAndCartage.Services[1];
				if (serviceBO1.ES_Booked == new ZDateTime(2012, 1, 1))
				{
					serviceBO1 = jobDocsAndCartage.Services[1];
					serviceBO2 = jobDocsAndCartage.Services[0];
				}
				AdditionalServiceDataObjectReaderTest.AssertContents(serviceBO1);
				AdditionalServiceDataObjectReaderTest.AssertContents(serviceBO2, new ZDateTime(2012, 1, 1), new ZDateTime(2012, 1, 2), TimeSpan.FromDays(2), "Refer to the reference 2", "NOM", 45.6m, "Note to others", serviceBO1.ES_OA_Location, serviceBO1.ES_OH_Contractor, ZString.Empty, ZString.Empty, shouldPopulateServiceId: false, "Sub Location 2");

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching OrderItem found, creating new OrderItem.
Information - Populating OrderItem...
Information - No matching OrderItem found, creating new OrderItem.
Information - Populating OrderItem...
Information - No matching JobService found, creating new JobService.
Information - Populating JobService...
Information - Matching 'LocalClient':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'Contractor':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - No matching JobService found, creating new JobService.
Information - Populating JobService...
Information - Matching 'LocalClient':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'Contractor':- Matched to 'INTHEMSYD' by code, address '' (only address).
".Trim(), Logger.Logs);
			});
		}

		public void TestBasicLocalProcessingFieldMappingsForShipmentPenalty()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var locationAddress = new OrganisationDataObjectReader(GetLocalClientAddress(), Logger, Factory).GetMatchedOrNewForTesting();
			var contractorAddress = new OrganisationDataObjectReader(GetNewAddressData_INTHEMSYD(nameof(DocAddressType.Contractor)), Logger, Factory).GetMatchedOrNewForTesting();
			Factory.SaveForTesting();

			var serviceDataObject1 = AdditionalServiceDataObjectReaderTest.SetupAdditionalService();
			var serviceDataObject2 = new AdditionalService()
			{
				Booked = new ZDateTime(2012, 1, 1),
				Completed = new ZDateTime(2012, 1, 2),
				Contractor = serviceDataObject1.Contractor,
				Duration = TimeSpan.FromDays(2),
				Location = serviceDataObject1.Location,
				References = "Refer to the reference 2",
				ServiceCode = new CodeDescriptionPair { Code = "NOM", Description = "Not Manage" },
				ServiceCount = 45.6m,
				ServiceNote = "Note to others"
			};
			var orderNumberDataObject1 = new OrderNumber { Sequence = 1, OrderReference = "REFERME1" };
			var orderNumberDataObject2 = new OrderNumber { Sequence = 2, OrderReference = "REFERME2" };

			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipmentDataObject.LocalProcessing = SetupLocalProcessing(new CodeDescriptionPair() { Code = "BOB", Description = "THE BUILDER" }, new CodeDescriptionPair() { Code = "WEN", Description = "THE DESTROYER" }, new CodeDescriptionPair() { Code = "JOE", Description = "THE PEACEMAKER" }, new CodeDescriptionPair() { Code = "JAY", Description = "THE LAZY" });
			shipmentDataObject.LocalProcessing.SetOrderNumberCollection(() => new DataObjectList<OrderNumber>(new[] { orderNumberDataObject1, orderNumberDataObject2 }));
			shipmentDataObject.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>(new[] { serviceDataObject1, serviceDataObject2 }));
			Logger.ClearLogs();
			var reader = new LocalProcessingDataObjectReader(shipmentDataObject, Logger, Factory);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			shipment.Consols.Add(consol);
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "PXGR1037347";
			SetupContainerLCLOverridesForUXMLDefaulting(container);
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(consol, container);
			Factory.SaveForTesting();

			var jobDocsAndCartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(shipment);
			reader.PopulateBusinessObject(jobDocsAndCartage);

			AssertContents(jobDocsAndCartage, "BOB", "WEN", "JOE", "JAY");
			Assert(container.DeliveryPenalties.Any(x => x.CPY_CreditorType == (ZString)"CAR" && x.CPY_PenaltyType == (ZString)"DET" && x.DurationAsDays == (ZByte)4 && x.FreeTimeAsDays == (ZByte)5 && x.CPY_PerUnitCost == (ZDecimal)32.10));
			Assert(container.DeliveryPenalties.Any(x => x.CPY_CreditorType == (ZString)"CAR" && x.CPY_PenaltyType == (ZString)"STO" && x.DurationAsDays == (ZByte)12 && x.FreeTimeAsDays == (ZByte)0 && x.CPY_PerUnitCost == (ZDecimal)6.78));
			Assert(container.PickupPenalties.Any(x => x.CPY_CreditorType == (ZString)"CAR" && x.CPY_PenaltyType == (ZString)"DET" && x.DurationAsDays == (ZByte)7 && x.FreeTimeAsDays == (ZByte)8 && x.CPY_PerUnitCost == (ZDecimal)65.43));

			void SetupContainerLCLOverridesForUXMLDefaulting(CommonContainer cont)
			{
				cont.JC_LCLAvailable = new DateTime(2011, 1, 10);
				cont.JC_LCLStorageCommences = new DateTime(2011, 1, 11);
			}
		}

		public void TestRenameDemurrageAndStorage()
		{
			var localProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			localProcessing.DemurrageOnPickupTime = TimeSpan.FromDays(5);
			localProcessing.DemurrageOnPickupCharge = 895.45m;
			localProcessing.DemurrageOnDeliveryTime = TimeSpan.FromDays(15);
			localProcessing.DemurrageOnDeliveryCharge = 15.98m;
			localProcessing.PickupTruckWaitTime = new ZDateTime(2019, 11, 1);
			localProcessing.PickupTruckWaitCharge = 500.03m;
			localProcessing.DeliveryTruckWaitTime = new ZDateTime(2019, 11, 20);
			localProcessing.DeliveryTruckWaitCharge = 47.29m;

			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.LocalProcessing = localProcessing;

			var reader = new LocalProcessingDataObjectReader(shipmentDataObject, Logger, Factory);
			var jobDocsAndCartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(Factory.New<ForwardingShipment>());
			reader.PopulateBusinessObject(jobDocsAndCartage);

			CombineAssertions(delegate
			{
				AssertEquals("jobDocsAndCartage.JP_PickupTruckWaitTime", new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 11, 1), jobDocsAndCartage.JP_PickupTruckWaitTime);
				AssertEquals("jobDocsAndCartage.JP_PickupTruckWaitCharge", 500.03m, jobDocsAndCartage.JP_PickupTruckWaitCharge);
				AssertEquals("jobDocsAndCartage.JP_DeliveryTruckWaitTime", new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 11, 20), jobDocsAndCartage.JP_DeliveryTruckWaitTime);
				AssertEquals("jobDocsAndCartage.JP_DeliveryTruckWaitCharge", 47.29m, jobDocsAndCartage.JP_DeliveryTruckWaitCharge);
			});
		}

		public void TestRenameDemurrageAndStorage_Fallback()
		{
			var localProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			localProcessing.DemurrageOnPickupTime = TimeSpan.FromDays(5);
			localProcessing.DemurrageOnPickupCharge = 895.45m;
			localProcessing.DemurrageOnDeliveryTime = TimeSpan.FromDays(15);
			localProcessing.DemurrageOnDeliveryCharge = 15.98m;

			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.LocalProcessing = localProcessing;

			var reader = new LocalProcessingDataObjectReader(shipmentDataObject, Logger, Factory);
			var jobDocsAndCartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(Factory.New<ForwardingShipment>());
			reader.PopulateBusinessObject(jobDocsAndCartage);

			CombineAssertions(delegate
			{
				AssertEquals("jobDocsAndCartage.JP_PickupTruckWaitTime", (ZDateTime)TimeSpan.FromDays(5), jobDocsAndCartage.JP_PickupTruckWaitTime);
				AssertEquals("jobDocsAndCartage.JP_PickupTruckWaitCharge", 895.45m, jobDocsAndCartage.JP_PickupTruckWaitCharge);
				AssertEquals("jobDocsAndCartage.JP_DeliveryTruckWaitTime", (ZDateTime)TimeSpan.FromDays(15), jobDocsAndCartage.JP_DeliveryTruckWaitTime);
				AssertEquals("jobDocsAndCartage.JP_DeliveryTruckWaitCharge", 15.98m, jobDocsAndCartage.JP_DeliveryTruckWaitCharge);
			});
		}

		public static LocalProcessing SetupLocalProcessing(CodeDescriptionPair fCLPickupEquipmentNeeded, CodeDescriptionPair printOptionForPackagesOnAWB, CodeDescriptionPair fCLDeliveryEquipmentNeeded, CodeDescriptionPair exportStatement)
		{
			return SetupLocalProcessing(
				fCLPickupEquipmentNeeded,
				new ZDateTime(2011, 1, 1),
				new ZDateTime(2011, 1, 2),
				new ZDateTime(2011, 1, 3),
				new ZDateTime(2011, 1, 4),
				"ARRCARREF1",
				new ZDateTime(2011, 1, 5),
				TimeSpan.FromDays(5),
				125.65m,
				TimeSpan.FromDays(6),
				895.45m,
				printOptionForPackagesOnAWB,
				fCLDeliveryEquipmentNeeded,
				new ZDateTime(2011, 1, 8),
				new ZDateTime(2011, 1, 9),
				new ZDateTime(2011, 1, 10),
				new ZDateTime(2011, 1, 11),
				12,
				6.78m,
				new ZDateTime(2011, 1, 12),
				new ZDateTime(2011, 1, 13),
				new ZDateTime(2011, 1, 14),
				new ZDateTime(2011, 1, 15),
				new ZDateTime(2011, 1, 16),
				TimeSpan.FromDays(16),
				89.65m,
				TimeSpan.FromDays(17),
				15.98m,
				ZBool.True,
				ZBool.False,
				ZBool.True,
				ZBool.False,
				exportStatement,
				8,
				7,
				65.43m,
				5,
				4,
				32.10m);
		}

		public static LocalProcessing SetupLocalProcessing(
			CodeDescriptionPair fCLPickupEquipmentNeeded,
			ZDateTime? estimatedPickup,
			ZDateTime? pickupRequiredBy,
			ZDateTime? pickupRequiredFrom,
			ZDateTime? pickupCartageAdvised,
			ZString? arrivalCartageRef,
			ZDateTime? pickupCartageCompleted,
			ZDateTime? pickupLabourTime,
			ZDecimal? pickupLabourCharge,
			ZDateTime? demurrageOnPickupTime,
			ZDecimal? demurrageOnPickupCharge,
			CodeDescriptionPair printOptionForPackagesOnAWB,
			CodeDescriptionPair fCLDeliveryEquipmentNeeded,
			ZDateTime? fCLAvailable,
			ZDateTime? fCLStorageCommences,
			ZDateTime? lCLAvailable,
			ZDateTime? lCLStorageCommences,
			ZByte? lCLAirStorageDaysOrHours,
			ZDecimal? lCLAirStorageCharge,
			ZDateTime? estimatedDelivery,
			ZDateTime? deliveryRequiredBy,
			ZDateTime? deliveryRequiredFrom,
			ZDateTime? deliveryCartageAdvised,
			ZDateTime? deliveryCartageCompleted,
			ZDateTime? deliveryLabourTime,
			ZDecimal? deliveryLabourCharge,
			ZDateTime? demurrageOnDeliveryTime,
			ZDecimal? demurrageOnDeliveryCharge,
			ZBool? hasProhibitedPackaging,
			ZBool? insuranceRequired,
			ZBool? isContingencyRelease,
			ZBool? lCLDatesOverrideConsol,
			CodeDescriptionPair exportStatement,
			ZByte? fclPickupDetentionFreeDays,
			ZByte? fclPickupDetentionDays,
			ZDecimal? fclPickupDetentionCharge,
			ZByte? fclDeliveryDetentionFreeDays,
			ZByte? fclDeliveryDetentionDays,
			ZDecimal? fclDeliveryDetentionCharge)
		{
			var dataObject = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.FCLPickupEquipmentNeeded = fCLPickupEquipmentNeeded;
			dataObject.EstimatedPickup = estimatedPickup;
			dataObject.PickupRequiredBy = pickupRequiredBy;
			dataObject.PickupRequiredFrom = pickupRequiredFrom;
			dataObject.PickupCartageAdvised = pickupCartageAdvised;
			dataObject.ArrivalCartageRef = arrivalCartageRef;
			dataObject.PickupCartageCompleted = pickupCartageCompleted;
			dataObject.PickupLabourTime = pickupLabourTime;
			dataObject.PickupLabourCharge = pickupLabourCharge;
			dataObject.DemurrageOnPickupTime = demurrageOnPickupTime;
			dataObject.DemurrageOnPickupCharge = demurrageOnPickupCharge;
			dataObject.PrintOptionForPackagesOnAWB = printOptionForPackagesOnAWB;
			dataObject.FCLDeliveryEquipmentNeeded = fCLDeliveryEquipmentNeeded;
			dataObject.FCLAvailable = fCLAvailable;
			dataObject.FCLStorageCommences = fCLStorageCommences;
			dataObject.LCLAvailable = lCLAvailable;
			dataObject.LCLStorageCommences = lCLStorageCommences;
			dataObject.LCLAirStorageDaysOrHours = lCLAirStorageDaysOrHours;
			dataObject.LCLAirStorageCharge = lCLAirStorageCharge;
			dataObject.EstimatedDelivery = estimatedDelivery;
			dataObject.DeliveryRequiredBy = deliveryRequiredBy;
			dataObject.DeliveryRequiredFrom = deliveryRequiredFrom;
			dataObject.DeliveryCartageAdvised = deliveryCartageAdvised;
			dataObject.DeliveryCartageCompleted = deliveryCartageCompleted;
			dataObject.DeliveryLabourTime = deliveryLabourTime;
			dataObject.DeliveryLabourCharge = deliveryLabourCharge;
			dataObject.DemurrageOnDeliveryTime = demurrageOnDeliveryTime;
			dataObject.DemurrageOnDeliveryCharge = demurrageOnDeliveryCharge;
			dataObject.HasProhibitedPackaging = hasProhibitedPackaging;
			dataObject.InsuranceRequired = insuranceRequired;
			dataObject.IsContingencyRelease = isContingencyRelease;
			dataObject.LCLDatesOverrideConsol = lCLDatesOverrideConsol;
			dataObject.ExportStatement = exportStatement;
			dataObject.FCLPickupDetentionFreeDays = fclPickupDetentionFreeDays;
			dataObject.FCLPickupDetentionDays = fclPickupDetentionDays;
			dataObject.FCLPickupDetentionCharge = fclPickupDetentionCharge;
			dataObject.FCLDeliveryDetentionFreeDays = fclDeliveryDetentionFreeDays;
			dataObject.FCLDeliveryDetentionDays = fclDeliveryDetentionDays;
			dataObject.FCLDeliveryDetentionCharge = fclDeliveryDetentionCharge;
			return dataObject;
		}

		public static void AssertContents(JobDocsAndCartage jobDocsAndCartage, ZString fCLPickupEquipmentNeeded, ZString printOptionForPackagesOnAWB, ZString fCLDeliveryEquipmentNeeded, ZString exportStatement)
		{
			AssertContents(
				jobDocsAndCartage,
				fCLPickupEquipmentNeeded,
				new ZDateTime(2011, 1, 1),
				new ZDateTime(2011, 1, 2),
				new ZDateTime(2011, 1, 3),
				new ZDateTime(2011, 1, 4),
				"ARRCARREF1",
				new ZDateTime(2011, 1, 5),
				TimeSpan.FromDays(5),
				125.65m,
				TimeSpan.FromDays(6),
				895.45m,
				printOptionForPackagesOnAWB,
				fCLDeliveryEquipmentNeeded,
				new ZDateTime(2011, 1, 8),
				new ZDateTime(2011, 1, 9),
				new ZDateTime(2011, 1, 10),
				new ZDateTime(2011, 1, 11),
				12,
				6.78m,
				new ZDateTime(2011, 1, 12),
				new ZDateTime(2011, 1, 13),
				new ZDateTime(2011, 1, 14),
				new ZDateTime(2011, 1, 15),
				new ZDateTime(2011, 1, 16),
				TimeSpan.FromDays(16),
				89.65m,
				TimeSpan.FromDays(17),
				15.98m,
				ZBool.True,
				ZBool.False,
				ZBool.True,
				ZBool.False,
				exportStatement,
				8,
				7,
				65.43m,
				5,
				4,
				32.10m);
		}

		public static void AssertContents(
			JobDocsAndCartage jobDocsAndCartage,
			ZString fCLPickupEquipmentNeeded,
			ZDateTime estimatedPickup,
			ZDateTime pickupRequiredBy,
			ZDateTime pickupRequiredFrom,
			ZDateTime pickupCartageAdvised,
			ZString arrivalCartageRef,
			ZDateTime pickupCartageCompleted,
			ZDateTime pickupLabourTime,
			ZDecimal pickupLabourCharge,
			ZDateTime demurrageOnPickupTime,
			ZDecimal demurrageOnPickupCharge,
			ZString printOptionForPackagesOnAWB,
			ZString fCLDeliveryEquipmentNeeded,
			ZDateTime fCLAvailable,
			ZDateTime fCLStorageCommences,
			ZDateTime lCLAvailable,
			ZDateTime lCLStorageCommences,
			ZByte lCLAirStorageDaysOrHours,
			ZDecimal lCLAirStorageCharge,
			ZDateTime estimatedDelivery,
			ZDateTime deliveryRequiredBy,
			ZDateTime deliveryRequiredFrom,
			ZDateTime deliveryCartageAdvised,
			ZDateTime deliveryCartageCompleted,
			ZDateTime deliveryLabourTime,
			ZDecimal deliveryLabourCharge,
			ZDateTime demurrageOnDeliveryTime,
			ZDecimal demurrageOnDeliveryCharge,
			ZBool hasProhibitedPackaging,
			ZBool insuranceRequired,
			ZBool isContingencyRelease,
			ZBool lCLDatesOverrideConsol,
			ZString exportStatement,
			ZByte fclPickupDetentionFreeDays,
			ZByte fclPickupDetentionDays,
			ZDecimal fclPickupDetentionCharge,
			ZByte fclDeliveryDetentionFreeDays,
			ZByte fclDeliveryDetentionDays,
			ZDecimal fclDeliveryDetentionCharge)
		{
			AssertEquals("jobDocsAndCartage.JP_FCLPickupEquipmentNeeded", fCLPickupEquipmentNeeded, jobDocsAndCartage.JP_FCLPickupEquipmentNeeded);
			AssertEquals("jobDocsAndCartage.JP_EstimatedPickup", estimatedPickup, jobDocsAndCartage.JP_EstimatedPickup);
			AssertEquals("jobDocsAndCartage.JP_PickupRequiredBy", pickupRequiredBy, jobDocsAndCartage.JP_PickupRequiredBy);
			AssertEquals("jobDocsAndCartage.JP_PickupRequiredFrom", pickupRequiredFrom, jobDocsAndCartage.JP_PickupRequiredFrom);
			AssertEquals("jobDocsAndCartage.JP_PickupCartageAdvised", pickupCartageAdvised, jobDocsAndCartage.JP_PickupCartageAdvised);
			AssertEquals("jobDocsAndCartage.JP_ArrivalCartageRef", arrivalCartageRef, jobDocsAndCartage.JP_ArrivalCartageRef);
			AssertEquals("jobDocsAndCartage.JP_PickupCartageCompleted", pickupCartageCompleted, jobDocsAndCartage.JP_PickupCartageCompleted);
			AssertEquals("jobDocsAndCartage.JP_PickupLabourTime", pickupLabourTime, jobDocsAndCartage.JP_PickupLabourTime);
			AssertEquals("jobDocsAndCartage.JP_PickupLabourCharge", pickupLabourCharge, jobDocsAndCartage.JP_PickupLabourCharge);
			AssertEquals("jobDocsAndCartage.JP_PickupTruckWaitTime", demurrageOnPickupTime, jobDocsAndCartage.JP_PickupTruckWaitTime);
			AssertEquals("jobDocsAndCartage.JP_PickupTruckWaitCharge", demurrageOnPickupCharge, jobDocsAndCartage.JP_PickupTruckWaitCharge);
			AssertEquals("jobDocsAndCartage.JP_PrintOptionForPackagesOnAWB", printOptionForPackagesOnAWB, jobDocsAndCartage.JP_PrintOptionForPackagesOnAWB);
			AssertEquals("jobDocsAndCartage.JP_FCLDeliveryEquipmentNeeded", fCLDeliveryEquipmentNeeded, jobDocsAndCartage.JP_FCLDeliveryEquipmentNeeded);
			AssertEquals("jobDocsAndCartage.JP_FCLAvailable", fCLAvailable, jobDocsAndCartage.JP_FCLAvailable);
			AssertEquals("jobDocsAndCartage.JP_FCLStorageCommences", fCLStorageCommences, jobDocsAndCartage.JP_FCLStorageCommences);
			AssertEquals("jobDocsAndCartage.JP_LCLAvailable", lCLAvailable, jobDocsAndCartage.JP_LCLAvailable);
			AssertEquals("jobDocsAndCartage.JP_LCLStorageCommences", lCLStorageCommences, jobDocsAndCartage.JP_LCLStorageCommences);
			AssertEquals("jobDocsAndCartage.JP_LCLAirStorageDaysOrHours", lCLAirStorageDaysOrHours, jobDocsAndCartage.JP_LCLAirStorageDaysOrHours);
			AssertEquals("jobDocsAndCartage.JP_LCLAirStorageCharge", lCLAirStorageCharge, jobDocsAndCartage.JP_LCLAirStorageCharge);
			AssertEquals("jobDocsAndCartage.JP_EstimatedDelivery", estimatedDelivery, jobDocsAndCartage.JP_EstimatedDelivery);
			AssertEquals("jobDocsAndCartage.JP_DeliveryRequiredBy", deliveryRequiredBy, jobDocsAndCartage.JP_DeliveryRequiredBy);
			AssertEquals("jobDocsAndCartage.JP_DeliveryRequiredFrom", deliveryRequiredFrom, jobDocsAndCartage.JP_DeliveryRequiredFrom);
			AssertEquals("jobDocsAndCartage.JP_DeliveryCartageAdvised", deliveryCartageAdvised, jobDocsAndCartage.JP_DeliveryCartageAdvised);
			AssertEquals("jobDocsAndCartage.JP_DeliveryCartageCompleted", deliveryCartageCompleted, jobDocsAndCartage.JP_DeliveryCartageCompleted);
			AssertEquals("jobDocsAndCartage.JP_DeliveryLabourTime", deliveryLabourTime, jobDocsAndCartage.JP_DeliveryLabourTime);
			AssertEquals("jobDocsAndCartage.JP_DeliveryLabourCharge", deliveryLabourCharge, jobDocsAndCartage.JP_DeliveryLabourCharge);
			AssertEquals("jobDocsAndCartage.JP_DeliveryTruckWaitTime", demurrageOnDeliveryTime, jobDocsAndCartage.JP_DeliveryTruckWaitTime);
			AssertEquals("jobDocsAndCartage.JP_DeliveryTruckWaitCharge", demurrageOnDeliveryCharge, jobDocsAndCartage.JP_DeliveryTruckWaitCharge);
			AssertEquals("jobDocsAndCartage.JP_HasProhibitedPackaging", hasProhibitedPackaging, jobDocsAndCartage.JP_HasProhibitedPackaging);
			AssertEquals("jobDocsAndCartage.JP_InsuranceRequired", insuranceRequired, jobDocsAndCartage.JP_InsuranceRequired);
			AssertEquals("jobDocsAndCartage.JP_IsContingencyRelease", isContingencyRelease, jobDocsAndCartage.JP_IsContingencyRelease);
			AssertEquals("jobDocsAndCartage.JP_LCLDatesOverrideConsol", lCLDatesOverrideConsol, jobDocsAndCartage.JP_LCLDatesOverrideConsol);
			AssertEquals("jobDocsAndCartage.JP_ExportStatement", exportStatement, jobDocsAndCartage.JP_ExportStatement);
			AssertEquals("jobDocsAndCartage.JP_FCLPickupDetentionFreeDays", fclPickupDetentionFreeDays, jobDocsAndCartage.JP_FCLPickupDetentionFreeDays);
			AssertEquals("jobDocsAndCartage.JP_FCLPickupDetentionDays", fclPickupDetentionDays, jobDocsAndCartage.JP_FCLPickupDetentionDays);
			AssertEquals("jobDocsAndCartage.JP_FCLPickupDetentionCharge", fclPickupDetentionCharge, jobDocsAndCartage.JP_FCLPickupDetentionCharge);
			AssertEquals("jobDocsAndCartage.JP_FCLDeliveryDetentionFreeDays", fclDeliveryDetentionFreeDays, jobDocsAndCartage.JP_FCLDeliveryDetentionFreeDays);
			AssertEquals("jobDocsAndCartage.JP_FCL_DeliveryDetentionDays", fclDeliveryDetentionDays, jobDocsAndCartage.JP_FCLDeliveryDetentionDays);
			AssertEquals("jobDocsAndCartage.JP_FCL_DeliveryDetentionCharge", fclDeliveryDetentionCharge, jobDocsAndCartage.JP_FCLDeliveryDetentionCharge);
		}
	}
}

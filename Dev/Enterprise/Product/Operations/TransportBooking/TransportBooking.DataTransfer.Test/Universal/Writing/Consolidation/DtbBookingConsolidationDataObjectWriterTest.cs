using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportBookings.DataTransfer.Universal.Testing
{
	public class DtbBookingConsolidationDataObjectWriterTest : DtbBookingTestCaseWithFactory
	{
		public void TestAdditionalReferences()
		{
			var bookingBO = Helper.CreateBooking();
			bookingBO.ConsolidationSingleJob.AdditionalReferenceNumbers.Add(AdditionalReferenceDataObjectWriterTest.SetupAdditionalReference(Factory, "TRF", "CON-TRF"));
			bookingBO.ConsolidationSingleJob.AdditionalReferenceNumbers.Add(AdditionalReferenceDataObjectWriterTest.SetupAdditionalReference(Factory, "HSB", "CON-HSB"));
			bookingBO.ConsolidationSingleJob.AdditionalReferenceNumbers.Add(AdditionalReferenceDataObjectWriterTest.SetupAdditionalReference(Factory, "CLR", "CON-CLR"));

			bookingBO.AdditionalReferenceNumbers.Add(AdditionalReferenceDataObjectWriterTest.SetupAdditionalReference(Factory, "TRF", "BK-TRF"));
			bookingBO.AdditionalReferenceNumbers.Add(AdditionalReferenceDataObjectWriterTest.SetupAdditionalReference(Factory, "CIN"));
			bookingBO.AdditionalReferenceNumbers.Add(AdditionalReferenceDataObjectWriterTest.SetupAdditionalReference(Factory, "UCR"));
			bookingBO.AdditionalReferenceNumbers.Add(AdditionalReferenceDataObjectWriterTest.SetupAdditionalReference(Factory, "CLR"));

			var writer = new DtbBookingConsolidationDataObjectWriter(new DataWritingManager(new DummyActionInfo()), true);
			var consoldiationDataObject = writer.GetDataObject(bookingBO.ConsolidationSingleJob);
			AssertNotNull("Precondition", consoldiationDataObject);
			AssertNotNull("Precondition", consoldiationDataObject.AdditionalReferenceCollection);

			CombineAssertions(delegate
			{
				AssertEquals(3, consoldiationDataObject.AdditionalReferenceCollection.Count);
				var sortedAdditionalReferences = consoldiationDataObject.AdditionalReferenceCollection.Cast<AdditionalReference>().OrderBy(e => e.Type.Code).ToArray();

				AdditionalReferenceDataObjectWriterTest.AssertContents(sortedAdditionalReferences[0], "CLR", "Customer Reference Number", "CON-CLR");
				AdditionalReferenceDataObjectWriterTest.AssertContents(sortedAdditionalReferences[1], "HSB", "House Bill", "CON-HSB");
				AdditionalReferenceDataObjectWriterTest.AssertContents(sortedAdditionalReferences[2], "TRF", "Transport Reference Number", "CON-TRF");
			});
		}

		public void TestPopulateAddresses()
		{
			var consol = Helper.CreateConsolidationMultiJob(OrganizationAddressTestHelper.GetOrganizationBO_CRAHOLSYD(Factory));
			var dataObject1 = new DtbBookingConsolidationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, consol)), true).GetDataObject(consol);

			var address = dataObject1.OrganizationAddressCollection.FirstOrDefault();
			OrganizationAddressTestHelper.AssertOrganizationBO_CRAHOLSYD("TransportCompanyDocumentaryAddress", address, "TransportCompanyDocumentaryAddress", true);
		}

		public void TestPopulateAddresses_Booking()
		{
			var consol = Helper.CreateConsolidation();
			consol.BookedByAddress.E2_OA_Address = OrganizationAddressTestHelper.GetOrganizationBO_CRAHOLSYD(Factory).MainAddress.PK;
			var dataObject1 = new DtbBookingConsolidationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, consol)), true).GetDataObject(consol);

			var address = dataObject1.OrganizationAddressCollection.FirstOrDefault();
			OrganizationAddressTestHelper.AssertOrganizationBO_CRAHOLSYD("BookingPartyDocumentaryAddress", address, "BookingPartyDocumentaryAddress", true);
		}

		public void TestPopulateGoodsDescription()
		{
			var consol = Helper.CreateConsolidation();
			consol.KB_GoodsDescription = "ABC";

			var dataObject1 = new DtbBookingConsolidationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, consol)), true).GetDataObject(consol);
			AssertEquals("Should have populated GoodsDescription.", "ABC", dataObject1.GoodsDescription);
		}

		public void TestPopulateChildConsolidations()
		{
			var multiJobConsol = Helper.CreateConsolidationMultiJob();
			var booking1 = Helper.CreateBooking();
			booking1.KM_JobID = "1";
			var booking2 = Helper.CreateBooking();
			booking2.KM_JobID = "2";
			multiJobConsol.Bookings.AddRange(new[] { booking1, booking2 });

			var dataObject1 = new DtbBookingConsolidationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, multiJobConsol)), true).GetDataObject(multiJobConsol);
			AssertEquals("Should be multijob consol", TransportConsolidationJobTypes.Codes.BookingTransportConsolidation, dataObject1.ShipmentType.Code);
			AssertEquals("Should have two child consolidations", 2, dataObject1.SubShipmentCollection.Count);

			CombineAssertions("Checking Child 1", delegate
			{
				var shipmentDataObject = dataObject1.SubShipmentCollection[0];
				AssertEquals("Should be singlejob consol", TransportConsolidationJobTypes.Codes.Booking, shipmentDataObject.ShipmentType.Code);
				AssertEquals("Should have a child booking", 1, shipmentDataObject.SubShipmentCollection.Count);
				AssertEquals("Child should be first booking", "TransportBooking [1]", shipmentDataObject.SubShipmentCollection[0].DataContext.GetDataSources());
			});

			CombineAssertions("Checking Child 2", delegate
			{
				var shipmentDataObject = dataObject1.SubShipmentCollection[1];
				AssertEquals("Should be singlejob consol", TransportConsolidationJobTypes.Codes.Booking, shipmentDataObject.ShipmentType.Code);
				AssertEquals("Should have a child booking", 1, shipmentDataObject.SubShipmentCollection.Count);
				AssertEquals("Child should be second booking", "TransportBooking [2]", shipmentDataObject.SubShipmentCollection[0].DataContext.GetDataSources());
			});
		}

		public void TestPopulateChildBookings()
		{
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var dataObject = new DtbBookingConsolidationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, consolidation)), false).GetDataObject(consolidation);
			AssertNull(dataObject.SubShipmentCollection);

			var booking1 = Helper.CreateBooking();
			booking1.KM_JobID = "1";
			consolidation.Bookings.AddRange(new[] { booking1 });
			dataObject = new DtbBookingConsolidationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, consolidation)), true).GetDataObject(consolidation);
			AssertType<DataObjectList<UniversalShipment>>(dataObject.SubShipmentCollection);
			AssertEquals("Should have one child consolidation", 1, dataObject.SubShipmentCollection.Count);
		}

		public void TestNamespace2012IncludesRelatedForwardingShipmentInDataWriting()
		{
			var shipmentBO = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipmentBO[JobShipmentSchema.JS_UniqueConsignRef] = "S00001234";
			shipmentBO[JobShipmentSchema.JS_HouseBill] = "HB31278903";

			var consolBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingConsol>());
			consolBO[JobConsolSchema.JK_UniqueConsignRef] = "C00001432";
			consolBO[JobConsolSchema.JK_AgentType] = "AGT";
			consolBO[JobConsolSchema.JK_TransportMode] = "SEA";
			consolBO[JobConsolSchema.JK_MasterBillNum] = "MB234890232";

			var consolShipmentLinkBO = Factory.New(ObjectFactory.GetType<Freight.Integration.IJobConShipLink>());
			consolShipmentLinkBO[JobConShipLinkSchema.JN_JK] = consolBO.PK;
			consolShipmentLinkBO[JobConShipLinkSchema.JN_JS] = shipmentBO.PK;

			var bookingConsolidationBO = Factory.New<DtbBookingConsolidation>();
			bookingConsolidationBO.KB_ParentID = shipmentBO.PK;
			bookingConsolidationBO.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			bookingConsolidationBO.KB_JobDirection = nameof(DtbBookingDirection.DLV);

			AssertEquals("Precondition: bookingConsolidationBO.Parent should be the Forwarding Shipment.", shipmentBO, bookingConsolidationBO.Parent.ParentWithWorkflow);

			var bookingBO = bookingConsolidationBO.Bookings.AddNew();
			bookingBO.KM_KT_NKBookingTemplate = "ABC";
			bookingBO.KM_Description = "DESC123";
			bookingBO.KM_Direction = "IMP";
			bookingBO.KM_TransportReference = "TRANS123";

			Factory.Save();

			UniversalShipment topLevelDataObject;
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				topLevelDataObject = new DtbBookingConsolidationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, bookingBO)), true).GetDataObject(bookingConsolidationBO);
			}

			CombineAssertions(() =>
			{
				AssertEquals(string.Format("TransportBookingConsolidation [{0}]", bookingConsolidationBO.KB_JobID), topLevelDataObject.DataContext.GetDataSources());
				AssertNotNull(topLevelDataObject.SubShipmentCollection);
				AssertEquals(1, topLevelDataObject.SubShipmentCollection.Count);
			});

			CombineAssertions(() =>
			{
				var transportJobDataObject = topLevelDataObject.SubShipmentCollection[0];
				AssertEquals(string.Format("TransportBooking [{0}]", bookingBO.KM_JobID), transportJobDataObject.DataContext.GetDataSources());
				AssertNull(topLevelDataObject.PostCarriageShipmentCollection);
				AssertNotNull(topLevelDataObject.PreCarriageShipmentCollection);
				AssertEquals(1, topLevelDataObject.PreCarriageShipmentCollection.Count);
			});

			var shipmentDataObject = topLevelDataObject.PreCarriageShipmentCollection[0];
			AssertEquals("ForwardingShipment [S00001234]", shipmentDataObject.DataContext.GetDataSources());
		}

		public void TestRelatedAgencyShipmentNOTIncludedInDataWriting()
		{
			var shipmentBO = Factory.NewWithValidTestData(ObjectFactory.GetType<Freight.Integration.Agency.IBillOfLading>());
			shipmentBO[JobShipmentSchema.JS_UniqueConsignRef] = "V00001234";
			shipmentBO[JobShipmentSchema.JS_HouseBill] = "HB31278903";

			var bookingConsolidationBO = Factory.New<DtbBookingConsolidation>();
			bookingConsolidationBO.KB_ParentID = shipmentBO.PK;
			bookingConsolidationBO.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			AssertEquals("Precondition: bookingConsolidationBO.Parent should be the Agency Bill Of Lading.", shipmentBO, bookingConsolidationBO.Parent.ParentWithWorkflow);

			var bookingBO = bookingConsolidationBO.Bookings.AddNew();
			bookingBO.KM_KT_NKBookingTemplate = "ABC";
			bookingBO.KM_Description = "DESC123";
			bookingBO.KM_Direction = "IMP";
			bookingBO.KM_TransportReference = "TRANS123";

			Factory.Save();

			var bookingConsolidationDataObject = new DtbBookingDataObjectWriter(null, new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, bookingBO)), true).GetDataObject(bookingBO);

			CombineAssertions("Checking bookingConsolidationDataObject", delegate
			{
				AssertEquals("DataContext.DataSourceCollection",
					string.Format("TransportBookingConsolidation [{0}], TransportBooking [{1}]", bookingConsolidationBO.KB_JobID, bookingBO.KM_JobID),
					bookingConsolidationDataObject.DataContext.GetDataSources());

				AssertNotNull("SubShipmentCollection", bookingConsolidationDataObject.SubShipmentCollection);
				AssertEquals("SubShipmentCollection.Count", 2, bookingConsolidationDataObject.SubShipmentCollection.Count);
			});

			CombineAssertions("Checking Child shipmentDataObject", delegate
			{
				var shipmentDataObject = bookingConsolidationDataObject.SubShipmentCollection[0];
				AssertEquals("DataContext.DataSourceCollection", "BillOfLading [V00001234]", shipmentDataObject.DataContext.GetDataSources());
				AssertEquals("WayBillNumber", "HB31278903", shipmentDataObject.WayBillNumber);
			});

			CombineAssertions("Checking Child bookingDataObject", delegate
			{
				var bookingDataObject = bookingConsolidationDataObject.SubShipmentCollection[1];
				AssertEquals("DataContext.DataSourceCollection", string.Format("TransportBooking [{0}]", bookingBO.KM_JobID), bookingDataObject.DataContext.GetDataSources());
				AssertEquals("TransportBookingDirection", "IMP - Import", bookingDataObject.TransportBookingDirection.ToStringContents());
			});
		}

		public void TestRelatedForwardingShipmentGetsIncludedInDataWriting()
		{
			var shipmentBO = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipmentBO[JobShipmentSchema.JS_UniqueConsignRef] = "S00001234";
			shipmentBO[JobShipmentSchema.JS_HouseBill] = "HB31278903";

			var bookingConsolidationBO = Factory.New<DtbBookingConsolidation>();
			bookingConsolidationBO.KB_ParentID = shipmentBO.PK;
			bookingConsolidationBO.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			AssertEquals("Precondition: bookingConsolidationBO.Parent should be the Forwarding Shipment.", shipmentBO, bookingConsolidationBO.Parent.ParentWithWorkflow);

			var bookingBO = bookingConsolidationBO.Bookings.AddNew();
			bookingBO.KM_KT_NKBookingTemplate = "ABC";
			bookingBO.KM_Description = "DESC123";
			bookingBO.KM_Direction = "IMP";
			bookingBO.KM_TransportReference = "TRANS123";

			Factory.Save();

			var bookingConsolidationDataObject = new DtbBookingDataObjectWriter(null, new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, bookingBO)), true).GetDataObject(bookingBO);

			CombineAssertions("Checking bookingConsolidationDataObject", delegate
			{
				AssertEquals("DataContext.DataSourceCollection",
					string.Format("TransportBookingConsolidation [{0}], TransportBooking [{1}]", bookingConsolidationBO.KB_JobID, bookingBO.KM_JobID),
					bookingConsolidationDataObject.DataContext.GetDataSources());

				AssertNotNull("SubShipmentCollection", bookingConsolidationDataObject.SubShipmentCollection);
				AssertEquals("SubShipmentCollection.Count", 2, bookingConsolidationDataObject.SubShipmentCollection.Count);
			});

			CombineAssertions("Checking Child shipmentDataObject", delegate
			{
				var shipmentDataObject = bookingConsolidationDataObject.SubShipmentCollection[0];
				AssertEquals("DataContext.DataSourceCollection", "ForwardingShipment [S00001234]", shipmentDataObject.DataContext.GetDataSources());
				AssertEquals("WayBillNumber", "HB31278903", shipmentDataObject.WayBillNumber);
				AssertEquals("WayBillType", "HWB", shipmentDataObject.WayBillType.GetCodeAsUpperCase());
			});

			CombineAssertions("Checking Child bookingDataObject", delegate
			{
				var bookingDataObject = bookingConsolidationDataObject.SubShipmentCollection[1];
				AssertEquals("DataContext.DataSourceCollection", string.Format("TransportBooking [{0}]", bookingBO.KM_JobID), bookingDataObject.DataContext.GetDataSources());
				AssertEquals("TransportBookingDirection", "IMP - Import", bookingDataObject.TransportBookingDirection.ToStringContents());
			});
		}

		public void TestRelatedForwardingShipmentWithParentConsolGetsIncludedInDataWriting()
		{
			var shipmentBO = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipmentBO[JobShipmentSchema.JS_UniqueConsignRef] = "S00001234";
			shipmentBO[JobShipmentSchema.JS_HouseBill] = "HB31278903";

			var consolBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingConsol>());
			consolBO[JobConsolSchema.JK_UniqueConsignRef] = "C00001432";
			consolBO[JobConsolSchema.JK_AgentType] = "AGT";
			consolBO[JobConsolSchema.JK_TransportMode] = "SEA";
			consolBO[JobConsolSchema.JK_MasterBillNum] = "MB234890232";

			((Forwarding.IForwardingConsol)consolBO).AddShipment((Forwarding.IForwardingShipment)shipmentBO);

			var bookingConsolidationBO = Factory.New<DtbBookingConsolidation>();
			bookingConsolidationBO.KB_ParentID = shipmentBO.PK;
			bookingConsolidationBO.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			AssertEquals("Precondition: bookingConsolidationBO.Parent should be the Forwarding Shipment.", shipmentBO, bookingConsolidationBO.Parent.ParentWithWorkflow);

			var bookingBO = bookingConsolidationBO.Bookings.AddNew();
			bookingBO.KM_KT_NKBookingTemplate = "ABC";
			bookingBO.KM_Description = "DESC123";
			bookingBO.KM_Direction = "IMP";
			bookingBO.KM_TransportReference = "TRANS123";

			Factory.Save();

			var bookingConsolidationDataObject = new DtbBookingDataObjectWriter(null, new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, bookingBO)), true).GetDataObject(bookingBO);

			CombineAssertions("Checking bookingConsolidationDataObject", delegate
			{
				AssertEquals("DataContext.DataSourceCollection",
					string.Format("TransportBookingConsolidation [{0}], TransportBooking [{1}]", bookingConsolidationBO.KB_JobID, bookingBO.KM_JobID),
					bookingConsolidationDataObject.DataContext.GetDataSources());

				AssertNotNull("SubShipmentCollection", bookingConsolidationDataObject.SubShipmentCollection);
				AssertEquals("SubShipmentCollection.Count", 2, bookingConsolidationDataObject.SubShipmentCollection.Count);
			});

			var consolDataObject = bookingConsolidationDataObject.SubShipmentCollection[0];
			CombineAssertions("Checking consolDataObject", delegate
			{
				AssertEquals("DataContext.DataSourceCollection", "ForwardingConsol [C00001432], ForwardingShipment [S00001234]", consolDataObject.DataContext.GetDataSources());
				AssertEquals("WayBillNumber", "MB234890232", consolDataObject.WayBillNumber);
				AssertEquals("WayBillType", "MWB", consolDataObject.WayBillType.GetCodeAsUpperCase());
				AssertNotNull("SubShipmentCollection", consolDataObject.SubShipmentCollection);
				AssertEquals("SubShipmentCollection.Count", 1, consolDataObject.SubShipmentCollection.Count);
			});

			CombineAssertions("Checking shipmentDataObject", delegate
			{
				var shipmentDataObject = consolDataObject.SubShipmentCollection[0];
				AssertEquals("DataContext.DataSourceCollection", "ForwardingShipment [S00001234]", shipmentDataObject.DataContext.GetDataSources());
				AssertEquals("WayBillNumber", "HB31278903", shipmentDataObject.WayBillNumber);
				AssertEquals("WayBillType", "HWB", shipmentDataObject.WayBillType.GetCodeAsUpperCase());
			});

			CombineAssertions("Checking Child bookingDataObject", delegate
			{
				var bookingDataObject = bookingConsolidationDataObject.SubShipmentCollection[1];
				AssertEquals("DataContext.DataSourceCollection", string.Format("TransportBooking [{0}]", bookingBO.KM_JobID), bookingDataObject.DataContext.GetDataSources());
				AssertEquals("TransportBookingDirection", "IMP - Import", bookingDataObject.TransportBookingDirection.ToStringContents());
			});
		}

		public void TestSchedules()
		{
			var consol = Helper.CreateConsolidation();

			var writer1 = new DtbBookingConsolidationDataObjectWriter(new DataWritingManager(new DummyActionInfo()), false);
			var bookingDataObject1 = writer1.GetDataObject(consol);
			AssertNull(bookingDataObject1.TransportLegCollection);

			var transport1 = Factory.New<ITransport>();
			transport1.ParentType = typeof(DtbBookingConsolidation);
			transport1.JW_ParentGUID = consol.PK;
			transport1.JW_RL_NKDiscPort = "AUSYD";
			transport1.JW_RL_NKLoadPort = "NZAKL";
			transport1.JW_VoyageFlight = "999";

			var transport2 = Factory.New<ITransport>();
			transport2.ParentType = typeof(DtbBookingConsolidation);
			transport2.JW_ParentGUID = consol.PK;
			transport2.JW_RL_NKDiscPort = "NZAKL";
			transport2.JW_RL_NKLoadPort = "USSFO";
			transport2.JW_VoyageFlight = "002";

			var writer2 = new DtbBookingConsolidationDataObjectWriter(new DataWritingManager(new DummyActionInfo()), false);
			var bookingDataObject2 = writer2.GetDataObject(consol);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"NZAKL|AUSYD|999", "USSFO|NZAKL|002"
			},
			FormatTransports(bookingDataObject2.TransportLegCollection));
		}

		public void TestSchedules_FromConsol()
		{
			var shipmentBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipmentBO[JobShipmentSchema.JS_TransportMode] = "SEA"; // Sea Freight
			shipmentBO[JobShipmentSchema.JS_PackingMode] = "LCL";
			shipmentBO[JobShipmentSchema.JS_ShipmentType] = "STD"; // Standard House

			var consolBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingConsol>());
			consolBO[JobConsolSchema.JK_AgentType] = "AGT";
			consolBO[JobConsolSchema.JK_TransportMode] = "SEA";
			consolBO[JobConsolSchema.JK_ConsolMode] = "FCL";

			shipmentBO[JobShipmentSchema.JS_RL_NKOrigin] = "NZDUD"; // Dunedin
			consolBO[JobConsolSchema.JK_RL_NKLoadPort] = "NZAKL"; // Christchurch
			consolBO[JobConsolSchema.JK_RL_NKPortOfFirstArrival] = "AUNTL"; // Newcastle
			consolBO[JobConsolSchema.JK_RL_NKDischargePort] = "AUSYD"; // Sydney
			shipmentBO[JobShipmentSchema.JS_RL_NKDestination] = "AUBDG"; // Bendigo

			var transportLegConsol = ((Forwarding.IForwardingConsol)consolBO).Transports_Get(0);
			transportLegConsol.ParentType = ObjectFactory.GetType<Forwarding.IForwardingConsol>();
			transportLegConsol.JW_RL_NKLoadPort = "NZAKL";
			transportLegConsol.JW_RL_NKDiscPort = "AUSYD";
			transportLegConsol.JW_VoyageFlight = "789";

			((Forwarding.IForwardingConsol)consolBO).AddShipment((Forwarding.IForwardingShipment)shipmentBO);

			var bookingConsolidationBO = Factory.New<DtbBookingConsolidation>();
			bookingConsolidationBO.KB_ParentID = shipmentBO.PK;
			bookingConsolidationBO.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			AssertEquals("Precondition: bookingConsolidationBO.Parent should be the Forwarding Shipment.", shipmentBO, bookingConsolidationBO.Parent.ParentWithWorkflow);

			var bookingBO = bookingConsolidationBO.Bookings.AddNew();
			bookingBO.KM_KT_NKBookingTemplate = "ABC";
			bookingBO.KM_Description = "DESC123";
			bookingBO.KM_Direction = "IMP";
			bookingBO.KM_TransportReference = "TRANS123";

			var action = new DummyActionInfo() { FactoryForProcessing = Factory };
			var bookingConsolidationDataObjectUsingTheConsolidationWriter = new DtbBookingConsolidationDataObjectWriter(new DataWritingManager(action), false);
			var bookingDataObject1 = bookingConsolidationDataObjectUsingTheConsolidationWriter.GetDataObject(bookingConsolidationBO);

			AssertContainsExactElementsInAnyOrder(new[] { "NZAKL|AUSYD|789" }, FormatTransports(bookingDataObject1.TransportLegCollection));
		}

		public void TestSchedules_FromShipment()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var shipmentBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
				shipmentBO[JobShipmentSchema.JS_TransportMode] = "SEA"; // Sea Freight
				shipmentBO[JobShipmentSchema.JS_PackingMode] = "LCL";
				shipmentBO[JobShipmentSchema.JS_ShipmentType] = "STD"; // Standard House

				var consolBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingConsol>());
				consolBO[JobConsolSchema.JK_AgentType] = "AGT";
				consolBO[JobConsolSchema.JK_TransportMode] = "SEA";
				consolBO[JobConsolSchema.JK_ConsolMode] = "FCL";

				shipmentBO[JobShipmentSchema.JS_RL_NKOrigin] = "NZDUD"; // Dunedin
				consolBO[JobConsolSchema.JK_RL_NKLoadPort] = "NZAKL"; // Christchurch
				consolBO[JobConsolSchema.JK_RL_NKPortOfFirstArrival] = "AUNTL"; // Newcastle
				consolBO[JobConsolSchema.JK_RL_NKDischargePort] = "AUSYD"; // Sydney
				shipmentBO[JobShipmentSchema.JS_RL_NKDestination] = "AUBDG"; // Bendigo

				var transportLegShipment1 = ((Forwarding.IForwardingShipment)shipmentBO).Transports_AddNew();
				transportLegShipment1.ParentType = ObjectFactory.GetType<Forwarding.IForwardingShipment>();
				transportLegShipment1.JW_RL_NKLoadPort = "NZCHC";
				transportLegShipment1.JW_RL_NKDiscPort = "AUSYD";
				transportLegShipment1.JW_VoyageFlight = "123";

				var transportLegShipment2 = ((Forwarding.IForwardingShipment)shipmentBO).Transports_AddNew();
				transportLegShipment2.ParentType = ObjectFactory.GetType<Forwarding.IForwardingShipment>();
				transportLegShipment2.JW_RL_NKLoadPort = "AUSYD";
				transportLegShipment2.JW_RL_NKDiscPort = "AUBNE";
				transportLegShipment2.JW_VoyageFlight = "456";

				((Forwarding.IForwardingConsol)consolBO).AddShipment((Forwarding.IForwardingShipment)shipmentBO);

				var bookingConsolidationBO = Factory.New<DtbBookingConsolidation>();
				bookingConsolidationBO.KB_ParentID = shipmentBO.PK;
				bookingConsolidationBO.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				AssertEquals("Precondition: bookingConsolidationBO.Parent should be the Forwarding Shipment.", shipmentBO, bookingConsolidationBO.Parent.ParentWithWorkflow);

				var bookingBO = bookingConsolidationBO.Bookings.AddNew();
				bookingBO.KM_KT_NKBookingTemplate = "ABC";
				bookingBO.KM_Description = "DESC123";
				bookingBO.KM_Direction = "IMP";
				bookingBO.KM_TransportReference = "TRANS123";

				var action = new DummyActionInfo() { FactoryForProcessing = Factory };
				var bookingConsolidationDataObjectUsingTheConsolidationWriter = new DtbBookingConsolidationDataObjectWriter(new DataWritingManager(action), false);
				var bookingDataObject1 = bookingConsolidationDataObjectUsingTheConsolidationWriter.GetDataObject(bookingConsolidationBO);

				AssertContainsExactElementsInAnyOrder(new[] { "NZCHC|AUSYD|123", "AUSYD|AUBNE|456" }, FormatTransports(bookingDataObject1.TransportLegCollection));
			}
		}

		public void TestSchedules_TransportMode_WithoutParent()
		{
			var consol = Helper.CreateConsolidation();
			var writer1 = new DtbBookingConsolidationDataObjectWriter(new DataWritingManager(new DummyActionInfo()), false);
			var consolDataObject1 = writer1.GetDataObject(consol);
			AssertNull("Precondition: No Parent", consol.Parent);
			AssertNull("No Schedules", consolDataObject1.TransportLegCollection);
			AssertNull("No Schedules", consolDataObject1.TransportMode);

			var transport1 = Factory.NewWithValidTestData<Transport>();
			transport1.ParentType = typeof(DtbBookingConsolidation);
			transport1.JW_ParentGUID = consol.PK;
			transport1.JW_LegOrder = 1;

			var transport2 = Factory.NewWithValidTestData<Transport>();
			transport2.ParentType = typeof(DtbBookingConsolidation);
			transport2.JW_ParentGUID = consol.PK;
			transport2.JW_LegOrder = 2;

			var writer2 = new DtbBookingConsolidationDataObjectWriter(new DataWritingManager(new DummyActionInfo()), false);
			var consolDataObject2 = writer2.GetDataObject(consol);
			AssertNotNull("Has Schedules", consolDataObject2.TransportLegCollection);
			AssertEquals("Has Schedules", 2, consolDataObject2.TransportLegCollection.Count);
			AssertNull("Has Schedules without TransportMode", consolDataObject2.TransportMode);

			transport1.JW_TransportMode = Constants.TransportModes.Sea;
			transport2.JW_TransportMode = Constants.TransportModes.Air;
			var writer3 = new DtbBookingConsolidationDataObjectWriter(new DataWritingManager(new DummyActionInfo()), false);
			var consolDataObject3 = writer3.GetDataObject(consol);
			AssertNotNull("Has Schedules", consolDataObject3.TransportLegCollection);
			AssertEquals("Has Schedules", 2, consolDataObject3.TransportLegCollection.Count);
			AssertNotNull("Has Schedules with TransportMode", consolDataObject3.TransportMode);
			AssertEquals("Should get TransportMode from first Schedule info ", "SEA", consolDataObject3.TransportMode.Code);
		}

		string[] FormatTransports(IEnumerable<TransportLeg> collection)
		{
			return collection
				.Select(t => string.Format("{0}|{1}|{2}", t.PortOfLoading.Code, t.PortOfDischarge.Code, t.VoyageFlightNo))
				.ToArray();
		}

		public void TestShipmentType()
		{
			var bookingConsolidation = Helper.CreateConsolidation();
			var bookingTransportConsolidation = Helper.CreateConsolidationMultiJob();

			var dataObject1 = new DtbBookingConsolidationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, bookingConsolidation)), true).GetDataObject(bookingConsolidation);
			AssertEquals(TransportConsolidationJobTypes.Codes.Booking, dataObject1.ShipmentType.Code);
			AssertEquals(TransportConsolidationJobTypes.Descriptions.Booking, dataObject1.ShipmentType.Description);

			var dataObject2 = new DtbBookingConsolidationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, bookingTransportConsolidation)), true).GetDataObject(bookingTransportConsolidation);
			AssertEquals(TransportConsolidationJobTypes.Codes.BookingTransportConsolidation, dataObject2.ShipmentType.Code);
			AssertEquals(TransportConsolidationJobTypes.Descriptions.BookingTransportConsolidation, dataObject2.ShipmentType.Description);
		}

		public void TestGetDataObject()
		{
			var consolidation = Factory.New<DtbBookingConsolidation>();
			consolidation.KB_JobID = "BOOKING123";
			consolidation.KB_JobDirection = "PIC";

			var lightTransportBookingDataObject = new DtbBookingConsolidationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, consolidation)), true).GetDataObject(consolidation);
			CombineAssertions(delegate
			{
				AssertNotNull("DataContext should be created", lightTransportBookingDataObject.DataContext);
				AssertEquals("lightTransportBookingDataObject.DataContext.DataSourceCollection", string.Format("TransportBookingConsolidation [{0}]", consolidation.KB_JobID), lightTransportBookingDataObject.DataContext.GetDataSources());

				AssertEquals("TransportBooking Direction Code is incorrect.", "PIC", lightTransportBookingDataObject.TransportBookingDirection.Code);
				AssertEquals("TransportBooking Direction Description is incorrect.", "Pickup", lightTransportBookingDataObject.TransportBookingDirection.Description);

				AssertNull("No Movements were set, so no SubShipments should be created.", lightTransportBookingDataObject.SubShipmentCollection);
			});

			// Set Transport Booking Parent
			var transportBookingParent = Factory.New<DummyWithDtbBooking>();
			consolidation.KB_ParentID = transportBookingParent.PK;
			consolidation.KB_ParentTableCode = transportBookingParent.TablePrefix;

			// Set transport Booking PackageJob
			var packageJob = consolidation.PackageJob;
			packageJob.KJ_JobID = "PJ00000001";
			packageJob.KJ_ParentID = consolidation.PK;
			packageJob.KJ_ParentTableCode = consolidation.TablePrefix;

			// Set transport Bookings
			consolidation.Bookings.AddNew();
			consolidation.Bookings.AddNew();

			var heavyTransportBookingDataObject = new DtbBookingConsolidationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, consolidation)), true).GetDataObject(consolidation);
			CombineAssertions(delegate
			{
				AssertNotNull("DataSource should be created", heavyTransportBookingDataObject.DataContext);
				AssertEquals("heavyTransportBookingDataObject.DataContext.DataSourceCollection", string.Format("TransportBookingConsolidation [{0}]", consolidation.KB_JobID), heavyTransportBookingDataObject.DataContext.GetDataSources());

				// TransportBooking Direction
				AssertEquals("TransportBooking Direction Code is incorrect.", "PIC", heavyTransportBookingDataObject.TransportBookingDirection.Code);
				AssertEquals("TransportBooking Direction Description is incorrect.", "Pickup", heavyTransportBookingDataObject.TransportBookingDirection.Description);

				AssertEquals("2 Movements were set, so 2 SubShipments should be created + the parent.", 3, heavyTransportBookingDataObject.SubShipmentCollection.Count);
			});
		}

		public void TestNotes()
		{
			var consolidationBO = Helper.CreateConsolidation();
			var noteBO1 = consolidationBO.Notes.AddNew(false, "Goods Handling Instructions", "Highly explosive");
			var noteBO2 = consolidationBO.Notes.AddNew(true, "Test Description", "Test note text");
			noteBO2.ST_NoteType = nameof(CargoWise.Definitions.StmNoteVisibility.PUB);

			var consolidationDataObject = new DtbBookingConsolidationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, consolidationBO)), true).GetDataObject(consolidationBO);
			AssertNotNull("consolidationDataObject", consolidationDataObject);
			AssertNotNull("consolidationDataObject.NoteCollection", consolidationDataObject.NoteCollection);
			AssertEquals("consolidationDataObject.NoteCollection.Count", 2, consolidationDataObject.NoteCollection.Count);

			var note1 = consolidationDataObject.NoteCollection[0];

			CombineAssertions(delegate
			{
				AssertEquals("note1.Description", "Goods Handling Instructions", note1.Description);
				AssertEquals("note1.IsCustomDescription", false, note1.IsCustomDescription);
				AssertEquals("note1.NoteText", "Highly explosive", note1.NoteText);
				AssertEquals("note1.NoteContext.Description", "Module: A - All, Direction: A - All, Freight: A - All", note1.NoteContext.Description);
				AssertEquals("note1.Visibility.Code", "PUB", note1.Visibility.Code);
				AssertEquals("note1.Visibility.Description", "CLIENT-VISIBLE", note1.Visibility.Description);
			});

			var note2 = consolidationDataObject.NoteCollection[1];

			CombineAssertions(delegate
			{
				AssertEquals("note2.Description", "Test Description", note2.Description);
				AssertEquals("note2.IsCustomDescription", true, note2.IsCustomDescription);
				AssertEquals("note2.NoteText", "Test note text", note2.NoteText);
				AssertEquals("note2.NoteContext.Description", "Module: A - All, Direction: A - All, Freight: A - All", note2.NoteContext.Description);
				AssertEquals("note2.Visibility.Code", "PUB", note2.Visibility.Code);
				AssertEquals("note2.Visibility.Description", "CLIENT-VISIBLE", note2.Visibility.Description);
			});
		}
	}
}

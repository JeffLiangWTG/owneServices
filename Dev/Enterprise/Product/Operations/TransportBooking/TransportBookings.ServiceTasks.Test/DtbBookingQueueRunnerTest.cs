using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.Integration;
using Enterprise.Integration.LandTransport;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Freight.Integration.Agency;
using static Enterprise.Freight.Integration.CFS;
using static Enterprise.Integration.Customs;
using CoreConstants = Enterprise.Core.Constants;
using IForwardingShipment = Enterprise.Integration.Forwarding.IForwardingShipment;

namespace Enterprise.TransportBookings.ServiceTasks.Test
{
	public class DtbBookingQueueRunnerTest : TestCaseWithFactory
	{
		public void TestRunQueueOfOneRecordSuccessfully_ShipmentParentPickupWithCombineContainers()
		{
			var logger = new TestServiceLogger();
			var shipment = CreateTestForwardingShipmentWith2Containers();
			var queueRecord = CreateTestDtbBookingQueueRecord(shipment.PK, JobShipmentSchema.Constants.Prefix, Env.CurrentBranch.Code, "PIC", true);
			Factory.Save();

			var deliveryManagerFactory = new TestDtbDeliveryManagerFactory();
			using (ObjectFactory.Substitute<IDtbDeliveryManagerFactory>(deliveryManagerFactory))
			{
				var runner = new DtbBookingQueueRunner();
				runner.Run(CancellationToken.None, logger);
				Factory.Save();

				var queryQueue = new ZDBOnlyQuery(typeof(DtbBookingQueue));
				queryQueue.AddToFilter(DtbBookingQueueSchema.PK, queueRecord.PK);
				var currentQueueRecords = Factory.Load<DtbBookingQueue>(queryQueue);
				AssertEquals("DtbBookingQueue record should have been deleted", 0, currentQueueRecords.Length);

				AssertContainsExactElementsInAnyOrder(
					"Should have called DtbDeliveryManager.DeliverTransportBookings() once with correct parameters from queue",
					new TestDtbDeliveryManagerDeliverTransportBookingCall[]
					{
						new TestDtbDeliveryManagerDeliverTransportBookingCall()
					},
					deliveryManagerFactory.SuccessfulCallsToDeliverTransportBooking);
				var expectedLogOutput = FormattableString.Invariant($@"Information|[{Env.CurrentBranch.Code}]: Started processing Transport Booking creation request for Parent Forwarding Shipment, job number {shipment.JS_UniqueConsignRef}.
Information|[{Env.CurrentBranch.Code}]: Successfully created 1 Transport Booking(s) for Parent Forwarding Shipment, job number {shipment.JS_UniqueConsignRef}.
Information|[{Env.CurrentBranch.Code}]: Ended processing Transport Booking creation request for Parent Forwarding Shipment, job number {shipment.JS_UniqueConsignRef}.");
				AssertMultilineASCIIEquals("Should log creation of Transport Booking.", expectedLogOutput, logger.ToString());
			}
		}

		public void TestRunQueueOfOneRecordSuccessfully_ShipmentParentPickupWithoutCombineContainers()
		{
			var logger = new TestServiceLogger();
			var shipment = CreateTestForwardingShipmentWith2Containers();
			var queueRecord = CreateTestDtbBookingQueueRecord(shipment.PK, JobShipmentSchema.Constants.Prefix, Env.CurrentBranch.Code, "PIC", false);
			Factory.Save();

			var deliveryManagerFactory = new TestDtbDeliveryManagerFactory();
			// in this case, with the real DtbDeliveryManager it should create two bookings, so have the test manager create that many bookings
			deliveryManagerFactory.ManagerCreatedBookings = 2;
			using (ObjectFactory.Substitute<IDtbDeliveryManagerFactory>(deliveryManagerFactory))
			{
				var runner = new DtbBookingQueueRunner();
				runner.Run(CancellationToken.None, logger);
				Factory.Save();

				var queryQueue = new ZDBOnlyQuery(typeof(DtbBookingQueue));
				queryQueue.AddToFilter(DtbBookingQueueSchema.PK, queueRecord.PK);
				var currentQueueRecords = Factory.Load<DtbBookingQueue>(queryQueue);
				AssertEquals("DtbBookingQueue record should have been deleted", 0, currentQueueRecords.Length);

				AssertContainsExactElementsInAnyOrder(
					"Should have called DtbDeliveryManager.DeliverTransportBookings() once with correct parameters from queue",
					new TestDtbDeliveryManagerDeliverTransportBookingCall[]
					{
						new TestDtbDeliveryManagerDeliverTransportBookingCall()
					},
					deliveryManagerFactory.SuccessfulCallsToDeliverTransportBooking);

				var expectedLogOutput = FormattableString.Invariant($@"Information|[{Env.CurrentBranch.Code}]: Started processing Transport Booking creation request for Parent Forwarding Shipment, job number {shipment.JS_UniqueConsignRef}.
Information|[{Env.CurrentBranch.Code}]: Successfully created 2 Transport Booking(s) for Parent Forwarding Shipment, job number {shipment.JS_UniqueConsignRef}.
Information|[{Env.CurrentBranch.Code}]: Ended processing Transport Booking creation request for Parent Forwarding Shipment, job number {shipment.JS_UniqueConsignRef}.");
				AssertMultilineASCIIEquals("Should log creation of Transport Bookings.", expectedLogOutput, logger.ToString());
			}
		}

		public void TestRunQueueOfOneRecordSuccessfully_ShipmentParentDeliveryWithCombineContainers()
		{
			var logger = new TestServiceLogger();
			var shipment = CreateTestForwardingShipmentWith2Containers();
			var queueRecord = CreateTestDtbBookingQueueRecord(shipment.PK, JobShipmentSchema.Constants.Prefix, Env.CurrentBranch.Code, "DLV", true);
			Factory.Save();

			var deliveryManagerFactory = new TestDtbDeliveryManagerFactory();
			using (ObjectFactory.Substitute<IDtbDeliveryManagerFactory>(deliveryManagerFactory))
			{
				var runner = new DtbBookingQueueRunner();
				runner.Run(CancellationToken.None, logger);
				Factory.Save();

				var queryQueue = new ZDBOnlyQuery(typeof(DtbBookingQueue));
				queryQueue.AddToFilter(DtbBookingQueueSchema.PK, queueRecord.PK);
				var currentQueueRecords = Factory.Load<DtbBookingQueue>(queryQueue);
				AssertEquals("DtbBookingQueue record should have been deleted", 0, currentQueueRecords.Length);

				AssertContainsExactElementsInAnyOrder(
					"Should have called DtbDeliveryManager.DeliverTransportBookings() once with correct parameters from queue",
					new TestDtbDeliveryManagerDeliverTransportBookingCall[]
					{
						new TestDtbDeliveryManagerDeliverTransportBookingCall()
					},
					deliveryManagerFactory.SuccessfulCallsToDeliverTransportBooking);

				var expectedLogOutput = FormattableString.Invariant($@"Information|[{Env.CurrentBranch.Code}]: Started processing Transport Booking creation request for Parent Forwarding Shipment, job number {shipment.JS_UniqueConsignRef}.
Information|[{Env.CurrentBranch.Code}]: Successfully created 1 Transport Booking(s) for Parent Forwarding Shipment, job number {shipment.JS_UniqueConsignRef}.
Information|[{Env.CurrentBranch.Code}]: Ended processing Transport Booking creation request for Parent Forwarding Shipment, job number {shipment.JS_UniqueConsignRef}.");
				AssertMultilineASCIIEquals("Should log creation of Transport Booking.", expectedLogOutput, logger.ToString());
			}
		}

		public void TestRunQueueOfOneRecordSuccessfully_ConsolParent()
		{
			var logger = new TestServiceLogger();

			var cnr = Helper.CreateOrganisation("CNRORG");
			var cyd = Helper.CreateOrganisation("CYDORG");
			var cfs = Helper.CreateOrganisation("CFSORG");
			var cto = Helper.CreateOrganisation("CTOORG");

			var shipment = Helper.CreateForwardingShipment("S00001234", "HB31278903", "SEA", "FCL");
			Helper.SetupForwardingShipmentAddresses(shipment, cnr, null, cfs, null);

			var consol = Helper.CreateForwardingConsol(shipment, "C0004321", "SEA", "MB99876543");
			Helper.SetupForwardingConsolAddresses(consol, cto, cfs, cyd, null, null, null);

			var queueRecord = CreateTestDtbBookingQueueRecord(consol.PK, JobConsolSchema.Constants.Prefix, Env.CurrentBranch.Code, "PIC", true);

			Factory.Save();

			var deliveryManagerFactory = new TestDtbDeliveryManagerFactory();
			using (ObjectFactory.Substitute<IDtbDeliveryManagerFactory>(deliveryManagerFactory))
			{
				var runner = new DtbBookingQueueRunner();
				runner.Run(CancellationToken.None, logger);
				Factory.Save();

				var queryQueue = new ZDBOnlyQuery(typeof(DtbBookingQueue));
				queryQueue.AddToFilter(DtbBookingQueueSchema.PK, queueRecord.PK);
				var currentQueueRecords = Factory.Load<DtbBookingQueue>(queryQueue);
				AssertEquals("DtbBookingQueue record should have been deleted", 0, currentQueueRecords.Length);

				AssertContainsExactElementsInAnyOrder(
					"Should have called DtbDeliveryManager.DeliverTransportBookings() once with correct parameters from queue",
					new TestDtbDeliveryManagerDeliverTransportBookingCall[]
					{
						new TestDtbDeliveryManagerDeliverTransportBookingCall()
					},
					deliveryManagerFactory.SuccessfulCallsToDeliverTransportBooking);

				var expectedLogOutput = FormattableString.Invariant($@"Information|[{Env.CurrentBranch.Code}]: Started processing Transport Booking creation request for Parent Forwarding Consolidation, job number {consol.JK_UniqueConsignRef}.
Information|[{Env.CurrentBranch.Code}]: Successfully created 1 Transport Booking(s) for Parent Forwarding Consolidation, job number {consol.JK_UniqueConsignRef}.
Information|[{Env.CurrentBranch.Code}]: Ended processing Transport Booking creation request for Parent Forwarding Consolidation, job number {consol.JK_UniqueConsignRef}.");
				AssertMultilineASCIIEquals("Should log creation of Transport Booking.", expectedLogOutput, logger.ToString());
			}
		}

		public void TestRunQueueOfOneRecordSuccessfully_CustomsDeclarationParent()
		{
			var logger = new TestServiceLogger();

			var customsDeclaration = (BusinessObject)Factory.New<IBaseJobDeclaration>();
			customsDeclaration.FillWithValidTestData();
			customsDeclaration[JobDeclarationSchema.JE_DeclarationReference] = "BX0000001";

			var queueRecord = CreateTestDtbBookingQueueRecord(customsDeclaration.PK, JobDeclarationSchema.Constants.Prefix, Env.CurrentBranch.Code, "PIC", true);

			Factory.Save();

			var runner = new DtbBookingQueueRunner();
			runner.Run(CancellationToken.None, logger);
			Factory.Save();

			var queryQueue = new ZDBOnlyQuery(typeof(DtbBookingQueue));
			queryQueue.AddToFilter(DtbBookingQueueSchema.PK, queueRecord.PK);
			var currentQueueRecords = Factory.Load<DtbBookingQueue>(queryQueue);
			AssertEquals("DtbBookingQueue record should have been deleted", 0, currentQueueRecords.Length);

			var queryBooking = new ZDBOnlyQuery(typeof(DtbBooking));
			var subQueryBookingConsolidation = new ZDBOnlySubQuery(typeof(DtbBookingConsolidation), DtbBookingConsolidationSchema.PK, DtbBookingSchema.KM_KB_Booking);
			subQueryBookingConsolidation.AddToFilter(DtbBookingConsolidationSchema.KB_ParentID, customsDeclaration.PK);
			queryBooking.AddSubQuery(subQueryBookingConsolidation, JoinCondition.And);
			var bookingRecordsCreated = Factory.Load<DtbBooking>(queryBooking);
			AssertEquals("DtbBooking record should have been created", 1, bookingRecordsCreated.Length);

			var newBooking = bookingRecordsCreated[0];
			CombineAssertions(() =>
			{
				AssertEquals("ORG", newBooking.KM_Direction);
			});

			var expectedLogOutput = FormattableString.Invariant($@"Information|[{Env.CurrentBranch.Code}]: Started processing Transport Booking creation request for Parent Customs Declaration, job number {customsDeclaration[JobDeclarationSchema.JE_DeclarationReference]}.
Information|[{Env.CurrentBranch.Code}]: Successfully created 1 Transport Booking(s) for Parent Customs Declaration, job number {customsDeclaration[JobDeclarationSchema.JE_DeclarationReference]}.
Information|[{Env.CurrentBranch.Code}]: Ended processing Transport Booking creation request for Parent Customs Declaration, job number {customsDeclaration[JobDeclarationSchema.JE_DeclarationReference]}.");
			AssertMultilineASCIIEquals("Should log creation of Transport Booking.", expectedLogOutput, logger.ToString());
		}

		public void TestRunQueueOfOneRecordSuccessfully_WhsOrderParent()
		{
			var logger = new TestServiceLogger();

			var cnr = Helper.CreateOrganisation("CNRORG");
			var cne = Helper.CreateOrganisation("CNEORG");

			var whsOrder = (BusinessObject)Factory.New<IWhsOrder>();
			whsOrder.FillWithValidTestData();
			whsOrder[WhsDocketSchema.WD_DocketID] = "BX0000001";
			whsOrder[WhsDocketSchema.WD_DocketSubType] = "ORD";
			whsOrder[WhsDocketSchema.WD_TotalUnits] = 1;
			whsOrder[WhsDocketSchema.WD_TotalWeight] = 1;
			whsOrder[WhsDocketSchema.WD_TotalWeightUnit] = "KG";
			whsOrder[WhsDocketSchema.WD_TotalCubic] = 1;
			whsOrder[WhsDocketSchema.WD_TotalCubicUnit] = "M3";

			var consignorJobDocAddress = Factory.New<IJobDocAddress>();
			consignorJobDocAddress.E2_OA_Address = cnr.MainAddress.PK;
			consignorJobDocAddress.E2_ParentID = whsOrder.PK;
			consignorJobDocAddress.E2_ParentTableCode = whsOrder.TablePrefix;
			consignorJobDocAddress.E2_AddressType = "SUD";

			var consigneeJobDocAddress = Factory.New<IJobDocAddress>();
			consigneeJobDocAddress.E2_OA_Address = cne.MainAddress.PK;
			consigneeJobDocAddress.E2_ParentID = whsOrder.PK;
			consigneeJobDocAddress.E2_ParentTableCode = whsOrder.TablePrefix;
			consigneeJobDocAddress.E2_AddressType = "CEA";

			var queueRecord = CreateTestDtbBookingQueueRecord(whsOrder.PK, WhsDocketSchema.Constants.Prefix, Env.CurrentBranch.Code, "DLV", true);

			Factory.Save();

			var deliveryManagerFactory = new TestDtbDeliveryManagerFactory();
			using (ObjectFactory.Substitute<IDtbDeliveryManagerFactory>(deliveryManagerFactory))
			{
				var runner = new DtbBookingQueueRunner();
				runner.Run(CancellationToken.None, logger);
				Factory.Save();

				var queryQueue = new ZDBOnlyQuery(typeof(DtbBookingQueue));
				queryQueue.AddToFilter(DtbBookingQueueSchema.PK, queueRecord.PK);
				var currentQueueRecords = Factory.Load<DtbBookingQueue>(queryQueue);
				AssertEquals("DtbBookingQueue record should have been deleted", 0, currentQueueRecords.Length);

				AssertContainsExactElementsInAnyOrder(
					"Should have called DtbDeliveryManager.DeliverTransportBookings() once with correct parameters from queue",
					new TestDtbDeliveryManagerDeliverTransportBookingCall[]
					{
						new TestDtbDeliveryManagerDeliverTransportBookingCall()
					},
					deliveryManagerFactory.SuccessfulCallsToDeliverTransportBooking);

				var expectedLogOutput = FormattableString.Invariant($@"Information|[{Env.CurrentBranch.Code}]: Started processing Transport Booking creation request for Parent Warehouse Order, job number {whsOrder[WhsDocketSchema.WD_DocketID]}.
Information|[{Env.CurrentBranch.Code}]: Successfully created 1 Transport Booking(s) for Parent Warehouse Order, job number {whsOrder[WhsDocketSchema.WD_DocketID]}.
Information|[{Env.CurrentBranch.Code}]: Ended processing Transport Booking creation request for Parent Warehouse Order, job number {whsOrder[WhsDocketSchema.WD_DocketID]}.");
				AssertMultilineASCIIEquals("Should log creation of Transport Booking.", expectedLogOutput, logger.ToString());
			}
		}

		public void TestRunQueueOfOneRecordSuccessfully_AgencyShipmentParent()
		{
			var logger = new TestServiceLogger();

			var agencyShipment = CreateTestAgencyShipmentWith2Containers();

			var queueRecord = CreateTestDtbBookingQueueRecord(agencyShipment.PK, JobShipmentSchema.Constants.Prefix, Env.CurrentBranch.Code, "PIC", false);

			Factory.Save();

			var deliveryManagerFactory = new TestDtbDeliveryManagerFactory();
			deliveryManagerFactory.ManagerCreatedBookings = 2;
			using (ObjectFactory.Substitute<IDtbDeliveryManagerFactory>(deliveryManagerFactory))
			{
				var runner = new DtbBookingQueueRunner();
				runner.Run(CancellationToken.None, logger);
				Factory.Save();

				var queryQueue = new ZDBOnlyQuery(typeof(DtbBookingQueue));
				queryQueue.AddToFilter(DtbBookingQueueSchema.PK, queueRecord.PK);
				var currentQueueRecords = Factory.Load<DtbBookingQueue>(queryQueue);
				AssertEquals("DtbBookingQueue record should have been deleted", 0, currentQueueRecords.Length);

				AssertContainsExactElementsInAnyOrder(
					"Should have called DtbDeliveryManager.DeliverTransportBookings() once with correct parameters from queue",
					new TestDtbDeliveryManagerDeliverTransportBookingCall[]
					{
						new TestDtbDeliveryManagerDeliverTransportBookingCall()
					},
					deliveryManagerFactory.SuccessfulCallsToDeliverTransportBooking);

				var expectedLogOutput = FormattableString.Invariant($@"Information|[{Env.CurrentBranch.Code}]: Started processing Transport Booking creation request for Parent Agency Shipment, job number {agencyShipment[JobShipmentSchema.JS_UniqueConsignRef]}.
Information|[{Env.CurrentBranch.Code}]: Successfully created 2 Transport Booking(s) for Parent Agency Shipment, job number {agencyShipment[JobShipmentSchema.JS_UniqueConsignRef]}.
Information|[{Env.CurrentBranch.Code}]: Ended processing Transport Booking creation request for Parent Agency Shipment, job number {agencyShipment[JobShipmentSchema.JS_UniqueConsignRef]}.");
				AssertMultilineASCIIEquals("Should log creation of Transport Bookings.", expectedLogOutput, logger.ToString());
			}
		}

		public void TestRunQueueOfOneRecordSuccessfully_BillOfLadingParent()
		{
			var logger = new TestServiceLogger();

			var billOfLading = CreateTestBillOfLadingWith2ContainersAnd1LoosePackline();

			var queueRecord = CreateTestDtbBookingQueueRecord(billOfLading.PK, JobShipmentSchema.Constants.Prefix, Env.CurrentBranch.Code, "PIC", false);

			Factory.Save();

			var deliveryManagerFactory = new TestDtbDeliveryManagerFactory();
			deliveryManagerFactory.ManagerCreatedBookings = 2;
			using (ObjectFactory.Substitute<IDtbDeliveryManagerFactory>(deliveryManagerFactory))
			{
				var runner = new DtbBookingQueueRunner();
				runner.Run(CancellationToken.None, logger);
				Factory.Save();

				var queryQueue = new ZDBOnlyQuery(typeof(DtbBookingQueue));
				queryQueue.AddToFilter(DtbBookingQueueSchema.PK, queueRecord.PK);
				var currentQueueRecords = Factory.Load<DtbBookingQueue>(queryQueue);
				AssertEquals("DtbBookingQueue record should have been deleted", 0, currentQueueRecords.Length);

				AssertContainsExactElementsInAnyOrder(
					"Should have called DtbDeliveryManager.DeliverTransportBookings() once with correct parameters from queue",
					new TestDtbDeliveryManagerDeliverTransportBookingCall[]
					{
						new TestDtbDeliveryManagerDeliverTransportBookingCall()
					},
					deliveryManagerFactory.SuccessfulCallsToDeliverTransportBooking);

				var expectedLogOutput = FormattableString.Invariant($@"Information|[{Env.CurrentBranch.Code}]: Started processing Transport Booking creation request for Parent Bill of Lading, job number {billOfLading[JobShipmentSchema.JS_UniqueConsignRef]}.
Information|[{Env.CurrentBranch.Code}]: Successfully created 2 Transport Booking(s) for Parent Bill of Lading, job number {billOfLading[JobShipmentSchema.JS_UniqueConsignRef]}.
Information|[{Env.CurrentBranch.Code}]: Ended processing Transport Booking creation request for Parent Bill of Lading, job number {billOfLading[JobShipmentSchema.JS_UniqueConsignRef]}.");
				AssertMultilineASCIIEquals("Should log creation of Transport Bookings.", expectedLogOutput, logger.ToString());
			}
		}

		public void TestRunQueueOfOneRecordSuccessfully_BillOfLadingParent_Integration()
		{
			var logger = new TestServiceLogger();

			var billOfLading = CreateTestBillOfLadingWith2ContainersAnd1LoosePackline();

			var queueRecord = CreateTestDtbBookingQueueRecord(billOfLading.PK, JobShipmentSchema.Constants.Prefix, Env.CurrentBranch.Code, "PIC", false);

			Factory.Save();

			var runner = new DtbBookingQueueRunner();
			runner.Run(CancellationToken.None, logger);
			Factory.Save();

			var queryQueue = new ZDBOnlyQuery(typeof(DtbBookingQueue));
			queryQueue.AddToFilter(DtbBookingQueueSchema.PK, queueRecord.PK);
			var currentQueueRecords = Factory.Load<DtbBookingQueue>(queryQueue);
			AssertEquals("DtbBookingQueue record should have been deleted", 0, currentQueueRecords.Length);

			var expectedLogOutput = FormattableString.Invariant($@"Information|[{Env.CurrentBranch.Code}]: Started processing Transport Booking creation request for Parent Bill of Lading, job number {billOfLading[JobShipmentSchema.JS_UniqueConsignRef]}.
Information|[{Env.CurrentBranch.Code}]: Successfully created 2 Transport Booking(s) for Parent Bill of Lading, job number {billOfLading[JobShipmentSchema.JS_UniqueConsignRef]}.
Information|[{Env.CurrentBranch.Code}]: Ended processing Transport Booking creation request for Parent Bill of Lading, job number {billOfLading[JobShipmentSchema.JS_UniqueConsignRef]}.");
			AssertMultilineASCIIEquals("Should log creation of Transport Bookings.", expectedLogOutput, logger.ToString());
		}

		public void TestRunQueueOfOneRecord_NonDtbBookingShipmentParent_ShouldFail()
		{
			var logger = new TestServiceLogger();

			var cfsShipment = (BusinessObject)Factory.New<ICFSShipment>();
			cfsShipment.FillWithValidTestData();
			cfsShipment[JobShipmentSchema.JS_IsForwardRegistered] = false;
			cfsShipment[JobShipmentSchema.JS_IsCFSRegistered] = true;
			cfsShipment[JobShipmentSchema.JS_IsShipping] = false;
			cfsShipment[JobShipmentSchema.JS_IsBooking] = false;
			cfsShipment[JobShipmentSchema.JS_UniqueConsignRef] = "CFS99999999";
			var queueRecord = CreateTestDtbBookingQueueRecord(cfsShipment.PK, JobShipmentSchema.Constants.Prefix, Env.CurrentBranch.Code, "PIC", false);

			Factory.Save();

			var runner = new DtbBookingQueueRunner();
			runner.Run(CancellationToken.None, logger);
			Factory.Save();

			var queryQueue = new ZDBOnlyQuery(typeof(DtbBookingQueue));
			queryQueue.AddToFilter(DtbBookingQueueSchema.PK, queueRecord.PK);
			var currentQueueRecords = new BusinessObjectFactory().Load<DtbBookingQueue>(queryQueue);
			AssertEquals("DtbBookingQueue record should have been deleted", 0, currentQueueRecords.Length);

			var expectedLogOutput = FormattableString.Invariant($@"Information|[{Env.CurrentBranch.Code}]: Started processing Transport Booking creation request for Parent CFSShipment, job number {cfsShipment[JobShipmentSchema.JS_UniqueConsignRef]}.
Error|[{Env.CurrentBranch.Code}]: Exception occurred during processing for Parent CFSShipment, job number CFS99999999, queue record deleted: Shipment type CFSShipment is not a valid Transport Booking parent
Information|[{Env.CurrentBranch.Code}]: Ended processing Transport Booking creation request for Parent CFSShipment, job number {cfsShipment[JobShipmentSchema.JS_UniqueConsignRef]}.");
			AssertMultilineASCIIEquals("Should log failure to create Transport Booking.", expectedLogOutput, logger.ToString());
		}

		public void TestRunQueueUnsuccessfulRecordRetryFalseAndDoNotSendToErrorReporter()
		{
			ErrorReporter.Clear();

			var deliveryManagerFactory = new TestDtbDeliveryManagerFactory();
			deliveryManagerFactory.ManagerDeliverErrorType = GetDtbBookingCreationErrorType(false, false);

			using (ObjectFactory.Substitute<IDtbDeliveryManagerFactory>(deliveryManagerFactory))
			{
				var logger = new TestServiceLogger();

				var cnr = Helper.CreateOrganisation("CNRORG");
				var cfs = Helper.CreateOrganisation("CFSORG");

				var shipment = Helper.CreateForwardingShipment("S00001234", "HB31278903", "SEA", "FCL");
				Helper.SetupForwardingShipmentAddresses(shipment, cnr, null, cfs, null);

				var queueRecord = CreateTestDtbBookingQueueRecord(shipment.PK, JobShipmentSchema.Constants.Prefix, Env.CurrentBranch.Code, "PIC", true);

				Factory.Save();

				var runner = new DtbBookingQueueRunner();
				runner.Run(CancellationToken.None, logger);
				Factory.Save();

				var queryQueue = new ZDBOnlyQuery(typeof(DtbBookingQueue));
				queryQueue.AddToFilter(DtbBookingQueueSchema.PK, queueRecord.PK);

				var currentQueueRecords = new BusinessObjectFactory().Load<DtbBookingQueue>(queryQueue);
				AssertEquals("DtbBookingQueue record should have been deleted", 0, currentQueueRecords.Length);

				AssertEquals(
					"Should have successfully called DtbDeliveryManager.DeliverTransportBookings() but with collected error",
					1,
					deliveryManagerFactory.SuccessfulCallsToDeliverTransportBooking.Count());

				var expectedLogOutput = FormattableString.Invariant($@"Information|[{Env.CurrentBranch.Code}]: Started processing Transport Booking creation request for Parent Forwarding Shipment, job number {shipment.JS_UniqueConsignRef}.
Error|[{Env.CurrentBranch.Code}]: Exception occurred during processing for Parent Forwarding Shipment, job number {shipment.JS_UniqueConsignRef}, queue record deleted: Test Deliver Transport Booking error.
Information|[{Env.CurrentBranch.Code}]: Ended processing Transport Booking creation request for Parent Forwarding Shipment, job number {shipment.JS_UniqueConsignRef}.");
				AssertMultilineASCIIEquals("Should log failure to create Transport Booking.", expectedLogOutput, logger.ToString());
				AssertEquals("Should have not sent error to ErrorReporter", 0, ErrorReporter.TotalErrorCount);

				ErrorReporter.Clear();
			}
		}

		public void TestRunQueueUnsuccessfulRecordRetryTrueLessThanMaxIterationsAndDoNotSendToErrorReporter()
		{
			ErrorReporter.Clear();

			var deliveryManagerFactory = new TestDtbDeliveryManagerFactory();
			deliveryManagerFactory.ManagerDeliverErrorType = GetDtbBookingCreationErrorType(true, false);

			using (ObjectFactory.Substitute<IDtbDeliveryManagerFactory>(deliveryManagerFactory))
			{
				var logger = new TestServiceLogger();

				var cnr = Helper.CreateOrganisation("CNRORG");
				var cfs = Helper.CreateOrganisation("CFSORG");

				var shipment = Helper.CreateForwardingShipment("S00001234", "HB31278903", "SEA", "FCL");
				Helper.SetupForwardingShipmentAddresses(shipment, cnr, null, cfs, null);

				var queueRecord = CreateTestDtbBookingQueueRecord(shipment.PK, JobShipmentSchema.Constants.Prefix, Env.CurrentBranch.Code, "PIC", true);

				Factory.Save();

				var runner = new DtbBookingQueueRunner();
				runner.Run(CancellationToken.None, logger);
				Factory.Save();

				var queryQueue = new ZDBOnlyQuery(typeof(DtbBookingQueue));
				queryQueue.AddToFilter(DtbBookingQueueSchema.PK, queueRecord.PK);

				var currentQueueRecords = new BusinessObjectFactory().Load<DtbBookingQueue>(queryQueue);
				AssertEquals("DtbBookingQueue record should have not been deleted", 1, currentQueueRecords.Length);
				AssertEquals("DtbBookingQueue record should have iteration field set to 1", (short)1, currentQueueRecords[0].Iteration);

				AssertEquals(
					"Should have successfully called DtbDeliveryManager.DeliverTransportBookings() but with collected error",
					1,
					deliveryManagerFactory.SuccessfulCallsToDeliverTransportBooking.Count());

				var expectedLogOutput = FormattableString.Invariant($@"Information|[{Env.CurrentBranch.Code}]: Started processing Transport Booking creation request for Parent Forwarding Shipment, job number {shipment.JS_UniqueConsignRef}.
Error|[{Env.CurrentBranch.Code}]: Exception occurred during processing for Parent Forwarding Shipment, job number {shipment.JS_UniqueConsignRef}, queue record has been iterated: Test Deliver Transport Booking error.
Information|[{Env.CurrentBranch.Code}]: Ended processing Transport Booking creation request for Parent Forwarding Shipment, job number {shipment.JS_UniqueConsignRef}.");
				AssertMultilineASCIIEquals("Should log failure to create Transport Booking.", expectedLogOutput, logger.ToString());
				AssertEquals("Should have not sent error to ErrorReporter", 0, ErrorReporter.TotalErrorCount);

				ErrorReporter.Clear();
			}
		}

		[UseSnapshotProtection]
		public void TestRunQueueUnsuccessfulRecordRetryTrueMaxIterationsAndDoNotSendToErrorReporter()
		{
			ErrorReporter.Clear();

			var deliveryManagerFactory = new TestDtbDeliveryManagerFactory();
			deliveryManagerFactory.ManagerDeliverErrorType = GetDtbBookingCreationErrorType(true, false);

			using (ObjectFactory.Substitute<IDtbDeliveryManagerFactory>(deliveryManagerFactory))
			{
				var logger = new TestServiceLogger();

				var cnr = Helper.CreateOrganisation("CNRORG");
				var cfs = Helper.CreateOrganisation("CFSORG");

				var shipment = Helper.CreateForwardingShipment("S00001234", "HB31278903", "SEA", "FCL");
				Helper.SetupForwardingShipmentAddresses(shipment, cnr, null, cfs, null);

				var queueRecord = CreateTestDtbBookingQueueRecord(shipment.PK, JobShipmentSchema.Constants.Prefix, Env.CurrentBranch.Code, "PIC", true, expectedMaxRetries);

				Factory.Save();

				var runner = new DtbBookingQueueRunner();
				runner.Run(CancellationToken.None, logger);
				Factory.Save();

				var queryQueue = new ZDBOnlyQuery(typeof(DtbBookingQueue));
				queryQueue.AddToFilter(DtbBookingQueueSchema.PK, queueRecord.PK);

				var currentQueueRecords = new BusinessObjectFactory().Load<DtbBookingQueue>(queryQueue);
				AssertEquals("DtbBookingQueue record should have been deleted", 0, currentQueueRecords.Length);

				AssertEquals(
					"Should have successfully called DtbDeliveryManager.DeliverTransportBookings() but with collected error",
					1,
					deliveryManagerFactory.SuccessfulCallsToDeliverTransportBooking.Count());

				var expectedLogOutput = FormattableString.Invariant($@"Information|[{Env.CurrentBranch.Code}]: Started processing Transport Booking creation request for Parent Forwarding Shipment, job number {shipment.JS_UniqueConsignRef}.
Error|[{Env.CurrentBranch.Code}]: Exception occurred during processing for Parent Forwarding Shipment, job number {shipment.JS_UniqueConsignRef} and has failed the maximum number of times, queue record deleted: Test Deliver Transport Booking error.
Information|[{Env.CurrentBranch.Code}]: Ended processing Transport Booking creation request for Parent Forwarding Shipment, job number {shipment.JS_UniqueConsignRef}.");
				AssertMultilineASCIIEquals("Should log failure to create Transport Booking.", expectedLogOutput, logger.ToString());
				AssertEquals("Should have not sent error to ErrorReporter", 0, ErrorReporter.TotalErrorCount);

				ErrorReporter.Clear();
			}
		}

		public void TestRunQueueUnsuccessfulRecordRetryTrueLessThanMaxIterationsAndSendToErrorReporter()
		{
			ErrorReporter.Clear();

			var deliveryManagerFactory = new TestDtbDeliveryManagerFactory();
			deliveryManagerFactory.ManagerDeliverErrorType = GetDtbBookingCreationErrorType(true, true);

			using (ObjectFactory.Substitute<IDtbDeliveryManagerFactory>(deliveryManagerFactory))
			{
				var logger = new TestServiceLogger();

				var cnr = Helper.CreateOrganisation("CNRORG");
				var cfs = Helper.CreateOrganisation("CFSORG");

				var shipment = Helper.CreateForwardingShipment("S00001234", "HB31278903", "SEA", "FCL");
				Helper.SetupForwardingShipmentAddresses(shipment, cnr, null, cfs, null);

				var queueRecord = CreateTestDtbBookingQueueRecord(shipment.PK, JobShipmentSchema.Constants.Prefix, Env.CurrentBranch.Code, "PIC", true);

				Factory.Save();

				var runner = new DtbBookingQueueRunner();
				runner.Run(CancellationToken.None, logger);
				Factory.Save();

				var queryQueue = new ZDBOnlyQuery(typeof(DtbBookingQueue));
				queryQueue.AddToFilter(DtbBookingQueueSchema.PK, queueRecord.PK);

				var currentQueueRecords = new BusinessObjectFactory().Load<DtbBookingQueue>(queryQueue);
				AssertEquals("DtbBookingQueue record should not have been deleted", 1, currentQueueRecords.Length);
				AssertEquals("DtbBookingQueue record should have iteration field set to 1", (short)1, currentQueueRecords[0].Iteration);

				AssertEquals(
					"Should have successfully called DtbDeliveryManager.DeliverTransportBookings() but with collected error",
					1,
					deliveryManagerFactory.SuccessfulCallsToDeliverTransportBooking.Count());

				var expectedLogOutput = FormattableString.Invariant($@"Information|[{Env.CurrentBranch.Code}]: Started processing Transport Booking creation request for Parent Forwarding Shipment, job number {shipment.JS_UniqueConsignRef}.
Error|[{Env.CurrentBranch.Code}]: Exception occurred during processing for Parent Forwarding Shipment, job number {shipment.JS_UniqueConsignRef}, queue record has been iterated: Test Deliver Transport Booking error.
Information|[{Env.CurrentBranch.Code}]: Ended processing Transport Booking creation request for Parent Forwarding Shipment, job number {shipment.JS_UniqueConsignRef}.");
				AssertMultilineASCIIEquals("Should log failure to create Transport Booking.", expectedLogOutput, logger.ToString());
				Assert("Should have also reported as Developer Error", ErrorReporter.LastMessageReported.Contains("Exception occurred during processing for Parent Forwarding Shipment, job number " + shipment.JS_UniqueConsignRef + ", queue record has been iterated: Test Deliver Transport Booking error"));
				AssertNull("Developer Error does not have an exception to include", ErrorReporter.LastExceptionReported);

				ErrorReporter.Clear();
			}
		}

		public void TestRunQueueUnsuccessfulRecordRetryTrueMaxIterationsAndSendToErrorReporter()
		{
			ErrorReporter.Clear();

			var deliveryManagerFactory = new TestDtbDeliveryManagerFactory();
			deliveryManagerFactory.ManagerDeliverErrorType = GetDtbBookingCreationErrorType(true, true);

			using (ObjectFactory.Substitute<IDtbDeliveryManagerFactory>(deliveryManagerFactory))
			{
				var logger = new TestServiceLogger();

				var cnr = Helper.CreateOrganisation("CNRORG");
				var cfs = Helper.CreateOrganisation("CFSORG");

				var shipment = Helper.CreateForwardingShipment("S00001234", "HB31278903", "SEA", "FCL");
				Helper.SetupForwardingShipmentAddresses(shipment, cnr, null, cfs, null);

				var queueRecord = CreateTestDtbBookingQueueRecord(shipment.PK, JobShipmentSchema.Constants.Prefix, Env.CurrentBranch.Code, "PIC", true, expectedMaxRetries);

				Factory.Save();

				var runner = new DtbBookingQueueRunner();
				runner.Run(CancellationToken.None, logger);
				Factory.Save();

				var queryQueue = new ZDBOnlyQuery(typeof(DtbBookingQueue));
				queryQueue.AddToFilter(DtbBookingQueueSchema.PK, queueRecord.PK);

				var currentQueueRecords = new BusinessObjectFactory().Load<DtbBookingQueue>(queryQueue);
				AssertEquals("DtbBookingQueue record should have been deleted", 0, currentQueueRecords.Length);

				AssertEquals(
					"Should have successfully called DtbDeliveryManager.DeliverTransportBookings() but with collected error",
					1,
					deliveryManagerFactory.SuccessfulCallsToDeliverTransportBooking.Count());

				var expectedLogOutput = FormattableString.Invariant($@"Information|[{Env.CurrentBranch.Code}]: Started processing Transport Booking creation request for Parent Forwarding Shipment, job number {shipment.JS_UniqueConsignRef}.
Error|[{Env.CurrentBranch.Code}]: Exception occurred during processing for Parent Forwarding Shipment, job number {shipment.JS_UniqueConsignRef} and has failed the maximum number of times, queue record deleted: Test Deliver Transport Booking error.
Information|[{Env.CurrentBranch.Code}]: Ended processing Transport Booking creation request for Parent Forwarding Shipment, job number {shipment.JS_UniqueConsignRef}.");
				AssertMultilineASCIIEquals("Should log failure to create Transport Booking.", expectedLogOutput, logger.ToString());
				Assert("Should have also reported as Developer Error", ErrorReporter.LastMessageReported.Contains("Exception occurred during processing for Parent Forwarding Shipment, job number " + shipment.JS_UniqueConsignRef + " and has failed the maximum number of times, queue record deleted: Test Deliver Transport Booking error"));
				AssertNull("Developer Error does not have an exception to include", ErrorReporter.LastExceptionReported);

				ErrorReporter.Clear();
			}
		}

		public void TestRunQueueOfOneRecordUnsuccessfully_WhenDeliverThrows_ExceptionCaughtAndQueueRecordIterated()
		{
			ErrorReporter.Clear();

			var deliveryManagerFactory = new TestDtbDeliveryManagerFactory();
			deliveryManagerFactory.ManagerCreatedToThrowException = 1;
			var errorManagerFactory = new ErrorManagerFactory();

			using (ObjectFactory.Substitute<IDtbDeliveryManagerFactory>(deliveryManagerFactory))
			using (ObjectFactory.Substitute(errorManagerFactory.GetNewErrorManager))
			{
				var logger = new TestServiceLogger();

				var cnr = Helper.CreateOrganisation("CNRORG");
				var cfs = Helper.CreateOrganisation("CFSORG");

				var shipment = Helper.CreateForwardingShipment("S00001234", "HB31278903", "SEA", "FCL");
				Helper.SetupForwardingShipmentAddresses(shipment, cnr, null, cfs, null);

				var queueRecord = CreateTestDtbBookingQueueRecord(shipment.PK, JobShipmentSchema.Constants.Prefix, Env.CurrentBranch.Code, "PIC", true);

				Factory.Save();

				var runner = new DtbBookingQueueRunner();
				runner.Run(CancellationToken.None, logger);
				Factory.Save();

				var queryQueue = new ZDBOnlyQuery(typeof(DtbBookingQueue));
				queryQueue.AddToFilter(DtbBookingQueueSchema.PK, queueRecord.PK);

				var currentQueueRecords = new BusinessObjectFactory().Load<DtbBookingQueue>(queryQueue);
				AssertEquals("DtbBookingQueue record should still be there", 1, currentQueueRecords.Length);
				AssertEquals("DtbBookingQueue record should have iteration field set to 1", (short)1, currentQueueRecords[0].Iteration);

				AssertContainsExactElementsInAnyOrder(
					"Should have not successfully called DtbDeliveryManager.DeliverTransportBookings() at all",
					Enumerable.Empty<TestDtbDeliveryManagerDeliverTransportBookingCall>(),
					deliveryManagerFactory.SuccessfulCallsToDeliverTransportBooking);

				var expectedLogOutput = FormattableString.Invariant($@"Information|[{Env.CurrentBranch.Code}]: Started processing Transport Booking creation request for Parent Forwarding Shipment, job number {shipment.JS_UniqueConsignRef}.
Error|[{Env.CurrentBranch.Code}]: Exception occurred during processing for Parent Forwarding Shipment, job number {shipment.JS_UniqueConsignRef}, queue record has been iterated: Test DeliverTransportBooking Exception.
Information|[{Env.CurrentBranch.Code}]: Ended processing Transport Booking creation request for Parent Forwarding Shipment, job number {shipment.JS_UniqueConsignRef}.");
				AssertMultilineASCIIEquals("Should log failure to create Transport Booking.", expectedLogOutput, logger.ToString());
				Assert("Should have also reported as Developer Error", ErrorReporter.LastMessageReported.Contains("Exception occurred during processing for Parent Forwarding Shipment, job number " + shipment.JS_UniqueConsignRef + ", queue record has been iterated: Test DeliverTransportBooking Exception"));
				AssertEquals("Developer Error should include exception", "Test DeliverTransportBooking Exception.", ErrorReporter.LastExceptionReported?.Message);

				var errorManager = errorManagerFactory.FirstErrorManager;
				AssertEquals("Should have collected error", 1, errorManager.ErrorList.Count);
				var errorManagerError = errorManager.ErrorList.Single();
				CombineAssertions("Error Manager should have collected correct details", () =>
				{
					AssertEquals("Error Manager should have collected error type as 'UnknownError'", DtbBookingCreationErrorType.UnknownError, errorManagerError.ErrorType);
					AssertEquals("Error Manager should have collected error message", "Test DeliverTransportBooking Exception.", errorManagerError.Message);
				});

				ErrorReporter.Clear();
			}
		}

		public void TestRunQueueOfOneRecordUnsuccessfully_WhenDeliverThrows_ExceptionCaughtAfterMaximumIterations()
		{
			ErrorReporter.Clear();

			var deliveryManagerFactory = new TestDtbDeliveryManagerFactory();
			deliveryManagerFactory.ManagerCreatedToThrowException = 1;
			var errorManagerFactory = new ErrorManagerFactory();

			using (ObjectFactory.Substitute<IDtbDeliveryManagerFactory>(deliveryManagerFactory))
			using (ObjectFactory.Substitute(errorManagerFactory.GetNewErrorManager))
			{
				var logger = new TestServiceLogger();

				var cnr = Helper.CreateOrganisation("CNRORG");
				var cfs = Helper.CreateOrganisation("CFSORG");

				var shipment = Helper.CreateForwardingShipment("S00001234", "HB31278903", "SEA", "FCL");
				Helper.SetupForwardingShipmentAddresses(shipment, cnr, null, cfs, null);

				var queueRecord = CreateTestDtbBookingQueueRecord(shipment.PK, JobShipmentSchema.Constants.Prefix, Env.CurrentBranch.Code, "PIC", true, expectedMaxRetries);

				Factory.Save();

				var runner = new DtbBookingQueueRunner();
				runner.Run(CancellationToken.None, logger);
				Factory.Save();

				var queryQueue = new ZDBOnlyQuery(typeof(DtbBookingQueue));
				queryQueue.AddToFilter(DtbBookingQueueSchema.PK, queueRecord.PK);
				var currentQueueRecords = Factory.Load<DtbBookingQueue>(queryQueue);
				AssertEquals("DtbBookingQueue record should now be deleted", 0, currentQueueRecords.Length);

				AssertContainsExactElementsInAnyOrder(
					"Should have not successfully called DtbDeliveryManager.DeliverTransportBookings() at all",
					Enumerable.Empty<TestDtbDeliveryManagerDeliverTransportBookingCall>(),
					deliveryManagerFactory.SuccessfulCallsToDeliverTransportBooking);

				var expectedLogOutput = FormattableString.Invariant($@"Information|[{Env.CurrentBranch.Code}]: Started processing Transport Booking creation request for Parent Forwarding Shipment, job number {shipment.JS_UniqueConsignRef}.
Error|[{Env.CurrentBranch.Code}]: Exception occurred during processing for Parent Forwarding Shipment, job number {shipment.JS_UniqueConsignRef} and has failed the maximum number of times, queue record deleted: Test DeliverTransportBooking Exception.
Information|[{Env.CurrentBranch.Code}]: Ended processing Transport Booking creation request for Parent Forwarding Shipment, job number {shipment.JS_UniqueConsignRef}.");
				AssertMultilineASCIIEquals("Should log failure to create Transport Booking.", expectedLogOutput, logger.ToString());
				Assert("Should have also reported as Developer Error", ErrorReporter.LastMessageReported.Contains("Exception occurred during processing for Parent Forwarding Shipment, job number " + shipment.JS_UniqueConsignRef + " and has failed the maximum number of times, queue record deleted: Test DeliverTransportBooking Exception"));
				AssertEquals("Developer Error should include exception", "Test DeliverTransportBooking Exception.", ErrorReporter.LastExceptionReported?.Message);

				var errorManager = errorManagerFactory.ErrorManagers[1];
				AssertEquals("Should have collected error", 1, errorManager.ErrorList.Count);
				var errorManagerError = errorManager.ErrorList.Single();
				CombineAssertions("Error Manager should have collected correct details", () =>
				{
					AssertEquals("Error Manager should have collected error type as 'UnknownError'", DtbBookingCreationErrorType.UnknownError, errorManagerError.ErrorType);
					AssertEquals("Error Manager should have collected error message", "Test DeliverTransportBooking Exception.", errorManagerError.Message);
				});

				ErrorReporter.Clear();
			}
		}

		public void TestRunQueueOfOneRecordSuccessfully_DeletesRecordOnFailure()
		{
			ErrorReporter.Clear();

			var logger = new TestServiceLogger();

			var targetModule = AutoCreatorTargetModules.Codes.LandTransportConsignment;
			var booking = Helper.CreateBooking();
			var queueRecord = CreateTestDtbBookingQueueRecord(booking.PK, DtbBookingSchema.Constants.Prefix, Env.CurrentBranch.Code, "", false, expectedMaxRetries, targetModule);

			Factory.Save();

			var runner = new DtbBookingQueueRunner();
			runner.Run(CancellationToken.None, logger);
			Factory.Save();

			var queryQueue = new ZDBOnlyQuery(typeof(DtbBookingQueue));
			queryQueue.AddToFilter(DtbBookingQueueSchema.PK, queueRecord.PK);

			var currentQueueRecords = new BusinessObjectFactory().Load<DtbBookingQueue>(queryQueue);
			AssertEquals("DtbBookingQueue record should have been deleted", 0, currentQueueRecords.Length);

			var expectedLogOutput = FormattableString.Invariant($@"Information|[{Env.CurrentBranch.Code}]: Started processing Land Transport Consignment creation request for Parent Transport Booking, job number {booking.KM_JobID}.
Error|[{Env.CurrentBranch.Code}]: Exception occurred during processing for Parent Transport Booking, job number {booking.KM_JobID}, queue record deleted: Land Transport module is not enabled, cannot create Land Transport Consignment from Transport Booking
Information|[{Env.CurrentBranch.Code}]: Ended processing Land Transport Consignment creation request for Parent Transport Booking, job number {booking.KM_JobID}.");
			AssertMultilineASCIIEquals("Should log failure to create Land Transport Consignment.", expectedLogOutput, logger.ToString());
			AssertEquals("Should have not sent error to ErrorReporter", 0, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		public void TestRunQueueOfOneRecordSuccessfully_CreatesLTConsignment()
		{
			var logger = new TestServiceLogger();
			var targetModule = AutoCreatorTargetModules.Codes.LandTransportConsignment;

			var now = new ZDateTime(ZDateTime.Now, DateTimeKind.Utc);
			var branchTransportCompany = Factory.NewWithValidTestData<OrgHeader>();
			var branch = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).Branches.AddNew();
			branch.GB_Code = "~ZZ";
			branch.GB_OH_OrgProxy = branchTransportCompany.PK;
			var validConfirmationDate = now.AddHours(1);
			var validJobCreatedDate = now.AddDays(-10).AddMinutes(-30);

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper);
			BookingToTransportJobCreatorTestHelper.SetDatesRelatedToAutomaticTransportJobCreationForBooking(Factory, booking, validJobCreatedDate, validConfirmationDate);

			var queueRecord = CreateTestDtbBookingQueueRecord(booking.PK, DtbBookingSchema.Constants.Prefix, Env.CurrentBranch.Code, "", false, 0, targetModule);
			Factory.Save();
			var queryConsignments = new ZDBOnlyQuery(typeof(IDtbConsignment));
			var currentConsignmentRecords = Factory.Load<IDtbConsignment>(queryConsignments);
			AssertEquals("Should be no land transport consignments", 0, currentConsignmentRecords.Length);

			using (TransportRegistry.Instance.EnableLandTransport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var runner = new DtbBookingQueueRunner();
				runner.Run(CancellationToken.None, logger);
				Factory.Save();
			}

			currentConsignmentRecords = Factory.Load<IDtbConsignment>(queryConsignments);
			AssertEquals("Should have created a land transport consignment", 1, currentConsignmentRecords.Length);
			var queryQueue = new ZDBOnlyQuery(typeof(DtbBookingQueue));
			queryQueue.AddToFilter(DtbBookingQueueSchema.PK, queueRecord.PK);
			var currentQueueRecords = Factory.Load<DtbBookingQueue>(queryQueue);
			AssertEquals("DtbBookingQueue record should have been deleted", 0, currentQueueRecords.Length);
			var logs = logger.ToString();
			AssertMultilineASCIIEquals("Should log creation of Consignments.", @"
Information|[BNE]: Started processing Land Transport Consignment creation request for Parent Transport Booking, job number TB00000001.
Information|[BNE]: Successfully created 1 Land Transport Consignment(s) for Parent Transport Booking, job number TB00000001.
Information|[BNE]: Ended processing Land Transport Consignment creation request for Parent Transport Booking, job number TB00000001.
".Trim(), string.Join("\n", logs));
		}

		public void TestRunQueueOfOneRecordSuccessfully_CreatesPortTransportJob()
		{
			var logger = new TestServiceLogger();
			var targetModule = AutoCreatorTargetModules.Codes.PortTransport;

			var now = new ZDateTime(ZDateTime.Now, DateTimeKind.Utc);
			var branchTransportCompany = Factory.NewWithValidTestData<OrgHeader>();
			var branch = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).Branches.AddNew();
			branch.GB_Code = "~ZZ";
			branch.GB_OH_OrgProxy = branchTransportCompany.PK;
			var validConfirmationDate = now.AddHours(1);
			var validJobCreatedDate = now.AddDays(-10).AddMinutes(-30);

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper);
			BookingToTransportJobCreatorTestHelper.SetDatesRelatedToAutomaticTransportJobCreationForBooking(Factory, booking, validJobCreatedDate, validConfirmationDate);

			var queueRecord = CreateTestDtbBookingQueueRecord(booking.PK, DtbBookingSchema.Constants.Prefix, Env.CurrentBranch.Code, "", false, 0, targetModule);
			Factory.Save();
			var queryPortTransportJobs = new ZDBOnlyQuery(typeof(ICommonCartage));
			var currentPortTransportJobRecords = Factory.Load<ICommonCartage>(queryPortTransportJobs);
			AssertEquals("Should be no port transports", 0, currentPortTransportJobRecords.Length);

			var runner = new DtbBookingQueueRunner();
			runner.Run(CancellationToken.None, logger);
			Factory.Save();

			currentPortTransportJobRecords = Factory.Load<ICommonCartage>(queryPortTransportJobs);
			AssertEquals("Should have created a port transport", 1, currentPortTransportJobRecords.Length);
			var queryQueue = new ZDBOnlyQuery(typeof(DtbBookingQueue));
			queryQueue.AddToFilter(DtbBookingQueueSchema.PK, queueRecord.PK);
			var currentQueueRecords = Factory.Load<DtbBookingQueue>(queryQueue);
			AssertEquals("DtbBookingQueue record should have been deleted", 0, currentQueueRecords.Length);
			var logs = logger.ToString();
			AssertMultilineASCIIEquals("Should log creation of Consignments.", @"
Information|[BNE]: Started processing Port Transport creation request for Parent Transport Booking, job number TB00000001.
Information|[BNE]: Successfully created 1 Port Transport(s) for Parent Transport Booking, job number TB00000001.
Information|[BNE]: Ended processing Port Transport creation request for Parent Transport Booking, job number TB00000001.
".Trim(), string.Join("\n", logs));
		}

		public void TestRunQueueUntilCancellationTokenSetAndThrowsExceptionWhenTokenCancelled()
		{
			var deliveryManagerFactory = new TestDtbDeliveryManagerFactory();
			deliveryManagerFactory.ManagerCreatedToSetCancellation = 3;
			deliveryManagerFactory.TokenSource = new CancellationTokenSource();
			using (ObjectFactory.Substitute<IDtbDeliveryManagerFactory>(deliveryManagerFactory))
			{
				var logger = new TestServiceLogger();

				var cnr = Helper.CreateOrganisation("CNRORG");
				var cfs = Helper.CreateOrganisation("CFSORG");

				var shipment = Helper.CreateForwardingShipment("S00001234", "HB31278903", "SEA", "FCL");
				Helper.SetupForwardingShipmentAddresses(shipment, cnr, null, cfs, null);

				var queueRecord = CreateTestDtbBookingQueueRecord(shipment.PK, JobShipmentSchema.Constants.Prefix, Env.CurrentBranch.Code, "PIC", true);

				var shipment2 = Helper.CreateForwardingShipment("S00001235", "HB31278904", "SEA", "FCL");
				Helper.SetupForwardingShipmentAddresses(shipment2, cnr, null, cfs, null);

				var queueRecord2 = CreateTestDtbBookingQueueRecord(shipment2.PK, JobShipmentSchema.Constants.Prefix, Env.CurrentBranch.Code, "DLV", true);

				var shipment3 = Helper.CreateForwardingShipment("S00001236", "HB31278905", "SEA", "FCL");
				Helper.SetupForwardingShipmentAddresses(shipment3, cnr, null, cfs, null);

				var queueRecord3 = CreateTestDtbBookingQueueRecord(shipment3.PK, JobShipmentSchema.Constants.Prefix, Env.CurrentBranch.Code, "PIC", false);

				var shipment4 = Helper.CreateForwardingShipment("S00001237", "HB31278906", "SEA", "FCL");
				Helper.SetupForwardingShipmentAddresses(shipment4, cnr, null, cfs, null);

				var queueRecord4 = CreateTestDtbBookingQueueRecord(shipment4.PK, JobShipmentSchema.Constants.Prefix, Env.CurrentBranch.Code, "DLV", false);

				var shipment5 = Helper.CreateForwardingShipment("S00001238", "HB31278907", "SEA", "FCL");
				Helper.SetupForwardingShipmentAddresses(shipment5, cnr, null, cfs, null);

				var queueRecord5 = CreateTestDtbBookingQueueRecord(shipment5.PK, JobShipmentSchema.Constants.Prefix, Env.CurrentBranch.Code, "PIC", true);

				Factory.Save();

				var runner = new DtbBookingQueueRunner();
				AssertExceptionThrown<OperationCanceledException>("Should throw an error on cancellation of token", () => runner.Run(deliveryManagerFactory.TokenSource.Token, logger));
				Factory.Save();

				var queryQueue = new ZDBOnlyQuery(typeof(DtbBookingQueue));
				queryQueue.AddToFilter(DtbBookingQueueSchema.PK, new ZGuid[] { queueRecord4.PK, queueRecord5.PK });
				var currentQueueRecords = Factory.Load<DtbBookingQueue>(queryQueue);
				AssertEquals("Should have 2 DtbBookingQueue records left after cancelling after processing 3", 2, currentQueueRecords.Length);

				var expectedSuccessfulCalls = new TestDtbDeliveryManagerDeliverTransportBookingCall[]
				{
					new TestDtbDeliveryManagerDeliverTransportBookingCall(),
					new TestDtbDeliveryManagerDeliverTransportBookingCall(),
					new TestDtbDeliveryManagerDeliverTransportBookingCall(),
				};
				AssertContainsExactElementsInAnyOrder(
					"Should have called DtbDeliveryManager.DeliverTransportBookings() once with correct parameters from queue",
					expectedSuccessfulCalls,
					deliveryManagerFactory.SuccessfulCallsToDeliverTransportBooking);

				var expectedLogOutput = FormattableString.Invariant($@"Information|[{Env.CurrentBranch.Code}]: Started processing Transport Booking creation request for Parent Forwarding Shipment, job number S00001234.
Information|[{Env.CurrentBranch.Code}]: Successfully created 1 Transport Booking(s) for Parent Forwarding Shipment, job number S00001234.
Information|[{Env.CurrentBranch.Code}]: Ended processing Transport Booking creation request for Parent Forwarding Shipment, job number S00001234.
Information|[{Env.CurrentBranch.Code}]: Started processing Transport Booking creation request for Parent Forwarding Shipment, job number S00001235.
Information|[{Env.CurrentBranch.Code}]: Successfully created 1 Transport Booking(s) for Parent Forwarding Shipment, job number S00001235.
Information|[{Env.CurrentBranch.Code}]: Ended processing Transport Booking creation request for Parent Forwarding Shipment, job number S00001235.
Information|[{Env.CurrentBranch.Code}]: Started processing Transport Booking creation request for Parent Forwarding Shipment, job number S00001236.
Information|[{Env.CurrentBranch.Code}]: Successfully created 1 Transport Booking(s) for Parent Forwarding Shipment, job number S00001236.
Information|[{Env.CurrentBranch.Code}]: Ended processing Transport Booking creation request for Parent Forwarding Shipment, job number S00001236.
Error|Service task was cancelled by request.");
				AssertMultilineASCIIEquals("Should log creation of Transport Bookings and also the fact that cancellation was requested.", expectedLogOutput, logger.ToString());
			}
		}

		[UseSnapshotProtection]
		public void TestRunQueueOfRecords_FailsOnFetchingNextRecord()
		{
			var logger = new TestServiceLogger();
			var shipment = CreateTestForwardingShipmentWith2Containers();
			var queueRecord = CreateTestDtbBookingQueueRecord(shipment.PK, JobShipmentSchema.Constants.Prefix, Env.CurrentBranch.Code, "PIC", true);
			Factory.Save();

			using (var adminConnection = Db.NewAdminConnection())
			{
				var mainLoopRecordsProcessed = 0;

				var runner = new DtbBookingQueueRunner();
				runner.BeginProcessRecordInMainLoop += Runner_BeginProcessRecordInMainLoop;
				runner.EndProcessRecordInMainLoop += Runner_EndProcessRecordInMainLoop;
				runner.Run(CancellationToken.None, logger);

				void Runner_BeginProcessRecordInMainLoop(object sender, EventArgs e)
				{
					if (mainLoopRecordsProcessed == 0)
					{
						adminConnection.ExecuteNonQuery(@"DENY SELECT ON OBJECT::dbo.DtbBookingQueue TO " + TestConnection.UserLogin);
					}
				}

				void Runner_EndProcessRecordInMainLoop(object sender, EventArgs e)
				{
					if (mainLoopRecordsProcessed == 0)
					{
						adminConnection.ExecuteNonQuery(@"REVOKE SELECT ON OBJECT::dbo.DtbBookingQueue TO " + TestConnection.UserLogin);
					}

					mainLoopRecordsProcessed++;
				}
			}

			var expectedLogOutputStart = @"Error|Exception occurred while getting next queue record to process: The SELECT permission was denied on the object 'DtbBookingQueue'";
			AssertStartsWith("When error occurs during getting of next queue record to process, check that error message displays in correct format", expectedLogOutputStart, logger.ToString());
		}

		public void TestRunQueueOfRecordsSuccessfully_FromDifferentBranches()
		{
			var otherProxyOrg = Helper.CreateOrganisation("OTHPXY1");
			GlbCompany otherCompany = Factory.New<GlbCompany>();
			otherCompany.GC_Code = "OTH";
			otherCompany.GC_RN_NKCountryCode = CoreConstants.CountryCodes.Australia;
			otherCompany.GC_OH_OrgProxy = otherProxyOrg.PK;
			GlbBranch otherBranch = otherCompany.Branches.AddNew();
			otherBranch.GB_Code = "XYZ";
			otherBranch.GB_RL_NKHomePort = "AU2CO";
			otherBranch.GB_IsActive = true;

			var otherProxyOrg2 = Helper.CreateOrganisation("OTHPXY2");
			GlbCompany otherCompany2 = Factory.New<GlbCompany>();
			otherCompany2.GC_Code = "OT2";
			otherCompany2.GC_RN_NKCountryCode = CoreConstants.CountryCodes.Australia;
			otherCompany2.GC_OH_OrgProxy = otherProxyOrg2.PK;
			GlbBranch otherBranch2 = otherCompany2.Branches.AddNew();
			otherBranch2.GB_Code = "XY2";
			otherBranch2.GB_RL_NKHomePort = "AU2IC";
			otherBranch2.GB_IsActive = true;

			Factory.Save();

			var logger = new TestServiceLogger();

			var cnr1 = Helper.CreateOrganisation("CNR1ORG");
			var cfs1 = Helper.CreateOrganisation("CFS1ORG");

			var shipment1 = Helper.CreateForwardingShipment("S00001234", "HB31278903", "SEA", "FCL");
			Helper.SetupForwardingShipmentAddresses(shipment1, cnr1, null, cfs1, null);

			var queueRecord1 = CreateTestDtbBookingQueueRecord(shipment1.PK, JobShipmentSchema.Constants.Prefix, otherBranch.GB_Code, "PIC", true);
			Factory.Save();

			var cnr2 = Helper.CreateOrganisation("CNR2ORG");
			var cfs2 = Helper.CreateOrganisation("CFS2ORG");

			var shipment2 = Helper.CreateForwardingShipment("S00001235", "HB31278905", "SEA", "FCL");
			Helper.SetupForwardingShipmentAddresses(shipment2, cnr2, null, cfs2, null);

			var queueRecord2 = CreateTestDtbBookingQueueRecord(shipment2.PK, JobShipmentSchema.Constants.Prefix, otherBranch2.GB_Code, "PIC", true);
			Factory.Save();

			var runner = new DtbBookingQueueRunner();
			runner.Run(CancellationToken.None, logger);
			Factory.Save();

			var queryQueue = new ZDBOnlyQuery(typeof(DtbBookingQueue));
			queryQueue.AddToFilter(DtbBookingQueueSchema.PK, new ZGuid[] { queueRecord1.PK, queueRecord2.PK });
			var currentQueueRecords = Factory.Load<DtbBookingQueue>(queryQueue);
			AssertEquals("DtbBookingQueue records should have been deleted", 0, currentQueueRecords.Length);

			var expectedLogOutput = FormattableString.Invariant($@"Information|[{otherBranch.GB_Code}]: Started processing Transport Booking creation request for Parent Forwarding Shipment, job number S00001234.
Information|[{otherBranch.GB_Code}]: Successfully created 1 Transport Booking(s) for Parent Forwarding Shipment, job number S00001234.
Information|[{otherBranch.GB_Code}]: Ended processing Transport Booking creation request for Parent Forwarding Shipment, job number S00001234.
Information|[{otherBranch2.GB_Code}]: Started processing Transport Booking creation request for Parent Forwarding Shipment, job number S00001235.
Information|[{otherBranch2.GB_Code}]: Successfully created 1 Transport Booking(s) for Parent Forwarding Shipment, job number S00001235.
Information|[{otherBranch2.GB_Code}]: Ended processing Transport Booking creation request for Parent Forwarding Shipment, job number S00001235.");
			AssertMultilineASCIIEquals("Should log creation of Transport Bookings in the correct branches.", expectedLogOutput, logger.ToString());
		}

		public void TestRunQueueForInvalidDtbBookingParent()
		{
			ErrorReporter.Clear();

			var errorManagerFactory = new ErrorManagerFactory();

			using (ObjectFactory.Substitute(errorManagerFactory.GetNewErrorManager))
			{
				var logger = new TestServiceLogger();

				var disableConstraintSQL = "ALTER TABLE dbo.DtbBookingQueue NOCHECK CONSTRAINT Constraint_KMQ_ParentTableCode";
				TestConnection.ExecuteNonQuery(disableConstraintSQL);

				var dummyPK = Guid.NewGuid();
				var queueRecord = CreateTestDtbBookingQueueRecord(dummyPK, DummyBizoSchema.Constants.Prefix, Env.CurrentBranch.Code, "PIC", true);
				Factory.Save();

				var runner = new DtbBookingQueueRunner();
				runner.Run(CancellationToken.None, logger);
				Factory.Save();

				var expectedLogOutput = FormattableString.Invariant($@"Information|[{Env.CurrentBranch.Code}]: Started processing Transport Booking creation request for Parent Prefix: {DummyBizoSchema.Constants.Prefix}, job number PK: {dummyPK}.
Error|[{Env.CurrentBranch.Code}]: Exception occurred during processing for Parent Prefix: {DummyBizoSchema.Constants.Prefix}, job number PK: {dummyPK}, queue record deleted: Invalid DtbBooking Parent Table Code
Information|[{Env.CurrentBranch.Code}]: Ended processing Transport Booking creation request for Parent Prefix: {DummyBizoSchema.Constants.Prefix}, job number PK: {dummyPK}.");
				AssertMultilineASCIIEquals("Should log failure to create Transport Booking.", expectedLogOutput, logger.ToString());

				var errorManager = errorManagerFactory.FirstErrorManager;
				AssertEquals("Should have collected error", 1, errorManager.ErrorList.Count);
				var errorManagerError = errorManager.ErrorList.Single();
				CombineAssertions("Error Manager should have collected correct details", () =>
				{
					AssertEquals("Error Manager should have collected error type as 'InvalidParentError'", DtbBookingCreationErrorType.InvalidParentError, errorManagerError.ErrorType);
					AssertEquals("Error Manager should have collected error message", "Invalid DtbBooking Parent Table Code", errorManagerError.Message);
				});
				AssertEquals("Should have not sent error to ErrorReporter", 0, ErrorReporter.TotalErrorCount);

				ErrorReporter.Clear();
			}
		}

		public void TestRunQueueOfOneRecord_ShipmentParentInactive_ShouldFail()
		{
			ErrorReporter.Clear();

			var errorManagerFactory = new ErrorManagerFactory();

			using (ObjectFactory.Substitute(errorManagerFactory.GetNewErrorManager))
			{
				var logger = new TestServiceLogger();
				var shipment = CreateTestForwardingShipmentWith2Containers();
				shipment.JS_IsCancelled = true;
				var queueRecord = CreateTestDtbBookingQueueRecord(shipment.PK, JobShipmentSchema.Constants.Prefix, Env.CurrentBranch.Code, "PIC", true);
				Factory.Save();

				var runner = new DtbBookingQueueRunner();
				runner.Run(CancellationToken.None, logger);
				Factory.Save();

				var queryQueue = new ZDBOnlyQuery(typeof(DtbBookingQueue));
				queryQueue.AddToFilter(DtbBookingQueueSchema.PK, queueRecord.PK);
				var currentQueueRecords = Factory.Load<DtbBookingQueue>(queryQueue);
				AssertEquals("DtbBookingQueue record should have been deleted", 0, currentQueueRecords.Length);

				var expectedLogOutput = FormattableString.Invariant($@"Information|[{Env.CurrentBranch.Code}]: Started processing Transport Booking creation request for Parent Forwarding Shipment, job number {shipment.JS_UniqueConsignRef}.
Error|[{Env.CurrentBranch.Code}]: Failed to Create Transport Booking:
{((IDtbBookingParent)shipment).HumanReadableName} is deactivated and cannot create new Transport Bookings.
Error|[{Env.CurrentBranch.Code}]: Exception occurred during processing for Parent Forwarding Shipment, job number {shipment.JS_UniqueConsignRef}, queue record deleted: {((IDtbBookingParent)shipment).HumanReadableName} is deactivated and cannot create new Transport Bookings.
Information|[{Env.CurrentBranch.Code}]: Ended processing Transport Booking creation request for Parent Forwarding Shipment, job number {shipment.JS_UniqueConsignRef}.");
				AssertMultilineASCIIEquals("Should log failure to create Transport Booking.", expectedLogOutput, logger.ToString());
				AssertEquals("Should have not sent error to ErrorReporter", 0, ErrorReporter.TotalErrorCount);
			}
		}

		public void TestRunQueueOfOneRecord_ConsolParentInactive_ShouldFail()
		{
			ErrorReporter.Clear();

			var errorManagerFactory = new ErrorManagerFactory();

			using (ObjectFactory.Substitute(errorManagerFactory.GetNewErrorManager))
			{
				var logger = new TestServiceLogger();

				var cnr = Helper.CreateOrganisation("CNRORG");
				var cyd = Helper.CreateOrganisation("CYDORG");
				var cfs = Helper.CreateOrganisation("CFSORG");
				var cto = Helper.CreateOrganisation("CTOORG");

				var shipment = Helper.CreateForwardingShipment("S00001234", "HB31278903", "SEA", "FCL");
				Helper.SetupForwardingShipmentAddresses(shipment, cnr, null, cfs, null);

				var consol = Helper.CreateForwardingConsol(shipment, "C0004321", "SEA", "MB99876543");
				Helper.SetupForwardingConsolAddresses(consol, cto, cfs, cyd, null, null, null);
				consol.JK_IsCancelled = true;

				var queueRecord = CreateTestDtbBookingQueueRecord(consol.PK, JobConsolSchema.Constants.Prefix, Env.CurrentBranch.Code, "PIC", true);
				Factory.Save();

				var runner = new DtbBookingQueueRunner();
				runner.Run(CancellationToken.None, logger);
				Factory.Save();

				var queryQueue = new ZDBOnlyQuery(typeof(DtbBookingQueue));
				queryQueue.AddToFilter(DtbBookingQueueSchema.PK, queueRecord.PK);
				var currentQueueRecords = Factory.Load<DtbBookingQueue>(queryQueue);
				AssertEquals("DtbBookingQueue record should have been deleted", 0, currentQueueRecords.Length);

				var expectedLogOutput = FormattableString.Invariant($@"Information|[{Env.CurrentBranch.Code}]: Started processing Transport Booking creation request for Parent Forwarding Consolidation, job number {consol.JK_UniqueConsignRef}.
Error|[{Env.CurrentBranch.Code}]: Failed to Create Transport Booking:
{((IDtbBookingParent)consol).HumanReadableName} is deactivated and cannot create new Transport Bookings.
Error|[{Env.CurrentBranch.Code}]: Exception occurred during processing for Parent Forwarding Consolidation, job number {consol.JK_UniqueConsignRef}, queue record deleted: {((IDtbBookingParent)consol).HumanReadableName} is deactivated and cannot create new Transport Bookings.
Information|[{Env.CurrentBranch.Code}]: Ended processing Transport Booking creation request for Parent Forwarding Consolidation, job number {consol.JK_UniqueConsignRef}.");
				AssertMultilineASCIIEquals("Should log failure to create Transport Booking.", expectedLogOutput, logger.ToString());
				AssertEquals("Should have not sent error to ErrorReporter", 0, ErrorReporter.TotalErrorCount);
			}
		}

		public void TestRunQueueOfOneRecord_CustomsDeclarationParentInactive_ShouldFail()
		{
			ErrorReporter.Clear();

			var errorManagerFactory = new ErrorManagerFactory();

			using (ObjectFactory.Substitute(errorManagerFactory.GetNewErrorManager))
			{
				var logger = new TestServiceLogger();
				var customsDeclaration = (BusinessObject)Factory.New<IBaseJobDeclaration>();
				customsDeclaration.FillWithValidTestData();
				customsDeclaration[JobDeclarationSchema.JE_DeclarationReference] = "BX0000001";
				customsDeclaration[JobDeclarationSchema.JE_IsCancelled] = true;
				var queueRecord = CreateTestDtbBookingQueueRecord(customsDeclaration.PK, JobDeclarationSchema.Constants.Prefix, Env.CurrentBranch.Code, "PIC", true);
				Factory.Save();

				var runner = new DtbBookingQueueRunner();
				runner.Run(CancellationToken.None, logger);
				Factory.Save();

				var queryQueue = new ZDBOnlyQuery(typeof(DtbBookingQueue));
				queryQueue.AddToFilter(DtbBookingQueueSchema.PK, queueRecord.PK);
				var currentQueueRecords = Factory.Load<DtbBookingQueue>(queryQueue);
				AssertEquals("DtbBookingQueue record should have been deleted", 0, currentQueueRecords.Length);

				var expectedLogOutput = FormattableString.Invariant($@"Information|[{Env.CurrentBranch.Code}]: Started processing Transport Booking creation request for Parent Customs Declaration, job number {customsDeclaration[JobDeclarationSchema.JE_DeclarationReference]}.
Error|[{Env.CurrentBranch.Code}]: Failed to Create Transport Booking:
{((IDtbBookingParent)customsDeclaration).HumanReadableName} is deactivated and cannot create new Transport Bookings.
Error|[{Env.CurrentBranch.Code}]: Exception occurred during processing for Parent Customs Declaration, job number {customsDeclaration[JobDeclarationSchema.JE_DeclarationReference]}, queue record deleted: {((IDtbBookingParent)customsDeclaration).HumanReadableName} is deactivated and cannot create new Transport Bookings.
Information|[{Env.CurrentBranch.Code}]: Ended processing Transport Booking creation request for Parent Customs Declaration, job number {customsDeclaration[JobDeclarationSchema.JE_DeclarationReference]}.");
				AssertMultilineASCIIEquals("Should log failure to create Transport Booking.", expectedLogOutput, logger.ToString());
				AssertEquals("Should have not sent error to ErrorReporter", 0, ErrorReporter.TotalErrorCount);
			}
		}

		public void TestRunQueueOfOneRecord_WhsOrderParentInactive_ShouldFail()
		{
			ErrorReporter.Clear();

			var errorManagerFactory = new ErrorManagerFactory();

			using (ObjectFactory.Substitute(errorManagerFactory.GetNewErrorManager))
			{
				var logger = new TestServiceLogger();

				var cnr = Helper.CreateOrganisation("CNRORG");
				var cne = Helper.CreateOrganisation("CNEORG");

				var whsOrder = (BusinessObject)Factory.New<IWhsOrder>();
				whsOrder.FillWithValidTestData();
				whsOrder[WhsDocketSchema.WD_DocketID] = "BX0000001";
				whsOrder[WhsDocketSchema.WD_DocketSubType] = "ORD";
				whsOrder[WhsDocketSchema.WD_TotalUnits] = 1;
				whsOrder[WhsDocketSchema.WD_TotalWeight] = 1;
				whsOrder[WhsDocketSchema.WD_TotalWeightUnit] = "KG";
				whsOrder[WhsDocketSchema.WD_TotalCubic] = 1;
				whsOrder[WhsDocketSchema.WD_TotalCubicUnit] = "M3";
				((ICancellable)whsOrder).IsCancelled = true;

				var consignorJobDocAddress = Factory.New<IJobDocAddress>();
				consignorJobDocAddress.E2_OA_Address = cnr.MainAddress.PK;
				consignorJobDocAddress.E2_ParentID = whsOrder.PK;
				consignorJobDocAddress.E2_ParentTableCode = whsOrder.TablePrefix;
				consignorJobDocAddress.E2_AddressType = "SUD";

				var consigneeJobDocAddress = Factory.New<IJobDocAddress>();
				consigneeJobDocAddress.E2_OA_Address = cne.MainAddress.PK;
				consigneeJobDocAddress.E2_ParentID = whsOrder.PK;
				consigneeJobDocAddress.E2_ParentTableCode = whsOrder.TablePrefix;
				consigneeJobDocAddress.E2_AddressType = "CEA";

				var queueRecord = CreateTestDtbBookingQueueRecord(whsOrder.PK, WhsDocketSchema.Constants.Prefix, Env.CurrentBranch.Code, "DLV", true);
				Factory.Save();

				var runner = new DtbBookingQueueRunner();
				runner.Run(CancellationToken.None, logger);
				Factory.Save();

				var queryQueue = new ZDBOnlyQuery(typeof(DtbBookingQueue));
				queryQueue.AddToFilter(DtbBookingQueueSchema.PK, queueRecord.PK);
				var currentQueueRecords = Factory.Load<DtbBookingQueue>(queryQueue);
				AssertEquals("DtbBookingQueue record should have been deleted", 0, currentQueueRecords.Length);

				var expectedLogOutput = FormattableString.Invariant($@"Information|[{Env.CurrentBranch.Code}]: Started processing Transport Booking creation request for Parent Warehouse Order, job number {whsOrder[WhsDocketSchema.WD_DocketID]}.
Error|[{Env.CurrentBranch.Code}]: Failed to Create Transport Booking:
{((IDtbBookingParent)whsOrder).HumanReadableName} is deactivated and cannot create new Transport Bookings.
Error|[{Env.CurrentBranch.Code}]: Exception occurred during processing for Parent Warehouse Order, job number {whsOrder[WhsDocketSchema.WD_DocketID]}, queue record deleted: {((IDtbBookingParent)whsOrder).HumanReadableName} is deactivated and cannot create new Transport Bookings.
Information|[{Env.CurrentBranch.Code}]: Ended processing Transport Booking creation request for Parent Warehouse Order, job number {whsOrder[WhsDocketSchema.WD_DocketID]}.");
				AssertMultilineASCIIEquals("Should log failure to create Transport Booking.", expectedLogOutput, logger.ToString());
				AssertEquals("Should have not sent error to ErrorReporter", 0, ErrorReporter.TotalErrorCount);
			}
		}

		public void TestRunQueueOfOneRecord_AgencyShipmentParentInactive_ShouldFail()
		{
			ErrorReporter.Clear();

			var errorManagerFactory = new ErrorManagerFactory();

			using (ObjectFactory.Substitute(errorManagerFactory.GetNewErrorManager))
			{
				var logger = new TestServiceLogger();
				var agencyShipment = CreateTestAgencyShipmentWith2Containers();
				agencyShipment[JobShipmentSchema.JS_IsCancelled] = true;

				var queueRecord = CreateTestDtbBookingQueueRecord(agencyShipment.PK, JobShipmentSchema.Constants.Prefix, Env.CurrentBranch.Code, "PIC", false);
				Factory.Save();

				var runner = new DtbBookingQueueRunner();
				runner.Run(CancellationToken.None, logger);
				Factory.Save();

				var queryQueue = new ZDBOnlyQuery(typeof(DtbBookingQueue));
				queryQueue.AddToFilter(DtbBookingQueueSchema.PK, queueRecord.PK);
				var currentQueueRecords = Factory.Load<DtbBookingQueue>(queryQueue);
				AssertEquals("DtbBookingQueue record should have been deleted", 0, currentQueueRecords.Length);

				var expectedLogOutput = FormattableString.Invariant($@"Information|[{Env.CurrentBranch.Code}]: Started processing Transport Booking creation request for Parent Agency Shipment, job number {agencyShipment[JobShipmentSchema.JS_UniqueConsignRef]}.
Error|[{Env.CurrentBranch.Code}]: Failed to Create Transport Booking:
{((IDtbBookingParent)agencyShipment).HumanReadableName} is deactivated and cannot create new Transport Bookings.
Error|[{Env.CurrentBranch.Code}]: Exception occurred during processing for Parent Agency Shipment, job number {agencyShipment[JobShipmentSchema.JS_UniqueConsignRef]}, queue record deleted: {((IDtbBookingParent)agencyShipment).HumanReadableName} is deactivated and cannot create new Transport Bookings.
Information|[{Env.CurrentBranch.Code}]: Ended processing Transport Booking creation request for Parent Agency Shipment, job number {agencyShipment[JobShipmentSchema.JS_UniqueConsignRef]}.");
				AssertMultilineASCIIEquals("Should log failure to create Transport Booking.", expectedLogOutput, logger.ToString());
				AssertEquals("Should have not sent error to ErrorReporter", 0, ErrorReporter.TotalErrorCount);
			}
		}

		public void TestRunQueueOfOneRecord_BillOfLadingParentInactive_ShouldFail()
		{
			ErrorReporter.Clear();

			var errorManagerFactory = new ErrorManagerFactory();

			using (ObjectFactory.Substitute(errorManagerFactory.GetNewErrorManager))
			{
				var logger = new TestServiceLogger();
				var billOfLading = CreateTestBillOfLadingWith2ContainersAnd1LoosePackline();
				billOfLading[JobShipmentSchema.JS_IsCancelled] = true;

				var queueRecord = CreateTestDtbBookingQueueRecord(billOfLading.PK, JobShipmentSchema.Constants.Prefix, Env.CurrentBranch.Code, "PIC", false);

				Factory.Save();

				var runner = new DtbBookingQueueRunner();
				runner.Run(CancellationToken.None, logger);
				Factory.Save();

				var queryQueue = new ZDBOnlyQuery(typeof(DtbBookingQueue));
				queryQueue.AddToFilter(DtbBookingQueueSchema.PK, queueRecord.PK);
				var currentQueueRecords = Factory.Load<DtbBookingQueue>(queryQueue);
				AssertEquals("DtbBookingQueue record should have been deleted", 0, currentQueueRecords.Length);

				var expectedLogOutput = FormattableString.Invariant($@"Information|[{Env.CurrentBranch.Code}]: Started processing Transport Booking creation request for Parent Bill of Lading, job number {billOfLading[JobShipmentSchema.JS_UniqueConsignRef]}.
Error|[{Env.CurrentBranch.Code}]: Failed to Create Transport Booking:
{((IDtbBookingParent)billOfLading).HumanReadableName} is deactivated and cannot create new Transport Bookings.
Error|[{Env.CurrentBranch.Code}]: Exception occurred during processing for Parent Bill of Lading, job number {billOfLading[JobShipmentSchema.JS_UniqueConsignRef]}, queue record deleted: {((IDtbBookingParent)billOfLading).HumanReadableName} is deactivated and cannot create new Transport Bookings.
Information|[{Env.CurrentBranch.Code}]: Ended processing Transport Booking creation request for Parent Bill of Lading, job number {billOfLading[JobShipmentSchema.JS_UniqueConsignRef]}.");
				AssertMultilineASCIIEquals("Should log failure to create Transport Booking.", expectedLogOutput, logger.ToString());
				AssertEquals("Should have not sent error to ErrorReporter", 0, ErrorReporter.TotalErrorCount);
			}
		}

		public void TestRunQueueOfOneRecord_ShipmentParent_InServiceTaskContextDoesNotReportError()
		{
			ErrorReporter.Clear();

			var logger = new TestServiceLogger();
			var shipment = CreateTestForwardingShipmentWith2Containers();
			var queueRecord = CreateTestDtbBookingQueueRecord(shipment.PK, JobShipmentSchema.Constants.Prefix, Env.CurrentBranch.Code, "PIC", true);
			Factory.Save();

			var deliveryManagerFactory = new TestDtbDeliveryManagerFactory();
			using (ObjectFactory.Substitute<IDtbDeliveryManagerFactory>(deliveryManagerFactory))
			{
				var runner = new DtbBookingQueueRunner();
				using (Env.Instance.TemporaryServiceTaskContext("KMQ", true))
				{
					runner.Run(CancellationToken.None, logger);
				}
				Factory.Save();

				var queryQueue = new ZDBOnlyQuery(typeof(DtbBookingQueue));
				queryQueue.AddToFilter(DtbBookingQueueSchema.PK, queueRecord.PK);
				var currentQueueRecords = Factory.Load<DtbBookingQueue>(queryQueue);
				AssertEquals("DtbBookingQueue record should have been deleted", 0, currentQueueRecords.Length);

				AssertContainsExactElementsInAnyOrder(
					"Should have called DtbDeliveryManager.DeliverTransportBookings() once with correct parameters from queue",
					new TestDtbDeliveryManagerDeliverTransportBookingCall[]
					{
						new TestDtbDeliveryManagerDeliverTransportBookingCall()
					},
					deliveryManagerFactory.SuccessfulCallsToDeliverTransportBooking);
				var expectedLogOutput = FormattableString.Invariant($@"Information|[{Env.CurrentBranch.Code}]: Started processing Transport Booking creation request for Parent Forwarding Shipment, job number {shipment.JS_UniqueConsignRef}.
Information|[{Env.CurrentBranch.Code}]: Successfully created 1 Transport Booking(s) for Parent Forwarding Shipment, job number {shipment.JS_UniqueConsignRef}.
Information|[{Env.CurrentBranch.Code}]: Ended processing Transport Booking creation request for Parent Forwarding Shipment, job number {shipment.JS_UniqueConsignRef}.");
				AssertMultilineASCIIEquals("Should log creation of Transport Booking.", expectedLogOutput, logger.ToString());
				AssertEquals("Should have not sent to ErrorReporter message from BaseEnvironment 'Service Task KMQ accesses environment current branch without setting the environment first...'", 0, ErrorReporter.TotalErrorCount);
			}
		}

		public void TestFailureInOneQueueRecordDoesNotCarryOverDefectiveFactoryToNextQueueRecord()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ZZZ";

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "AAB";
			company.GC_OH_OrgProxy = orgHeader.PK;

			var department = Factory.New<GlbDepartment>();
			department.GE_Code = "FDP";

			var inactiveBranch = Factory.New<GlbBranch>();
			inactiveBranch.GB_Code = "XYZ";
			inactiveBranch.GB_GC = company.PK;
			inactiveBranch.GB_OH_OrgProxy = orgHeader.PK;
			// when current branch is inactive this causes UniversalXmlWorkflowProcessor.ProcessAsInboundDataObjectWithRetry() to return result.ImportResults empty and hence send UniversalEventDeliveryFailureException
			inactiveBranch.GB_IsActive = false;

			var shipmentParent = CreateTestForwardingShipmentWith2Containers();
			var shipmentParentBO = shipmentParent as BusinessObject;
			Factory.Save();

			var queueOnInactiveBranch = CreateTestDtbBookingQueueRecord(shipmentParent.PK, JobShipmentSchema.Constants.Prefix, inactiveBranch.GB_Code, "PIC", true);
			var queueOnActiveBranch = CreateTestDtbBookingQueueRecord(shipmentParent.PK, JobShipmentSchema.Constants.Prefix, Env.CurrentBranch.Code, "PIC", true);

			Factory.Save();

			var logger = new TestServiceLogger();
			var runner = new DtbBookingQueueRunner();
			runner.Run(CancellationToken.None, logger);
			Factory.Save();

			var queryQueue = new ZDBOnlyQuery(typeof(DtbBookingQueue));
			queryQueue.AddToFilter(DtbBookingQueueSchema.PK, queueOnActiveBranch.PK);
			queryQueue.AddToFilter(JoinCondition.Or, DtbBookingQueueSchema.PK, queueOnInactiveBranch.PK);
			var currentQueueRecords = Factory.Load<DtbBookingQueue>(queryQueue);
			AssertEquals("Both (failing and successful) DtbBookingQueue records should have been deleted", 0, currentQueueRecords.Length);

			var expectedLogOutput = FormattableString.Invariant($@"Information|[{inactiveBranch.GB_Code}]: Started processing Transport Booking creation request for Parent Forwarding Shipment, job number {shipmentParentBO[JobShipmentSchema.JS_UniqueConsignRef]}.
Error|[{inactiveBranch.GB_Code}]: Failed to Create Transport Booking:
Error - Message Rejected as Company '{company.GC_Code}' has no active branches.
Error|[{inactiveBranch.GB_Code}]: Exception occurred during processing for Parent Forwarding Shipment, job number {shipmentParentBO[JobShipmentSchema.JS_UniqueConsignRef]}, queue record deleted: Failed to Create Transport Booking:
Error - Message Rejected as Company '{company.GC_Code}' has no active branches.
Information|[{inactiveBranch.GB_Code}]: Ended processing Transport Booking creation request for Parent Forwarding Shipment, job number {shipmentParentBO[JobShipmentSchema.JS_UniqueConsignRef]}.
Information|[{Env.CurrentBranch.Code}]: Started processing Transport Booking creation request for Parent Forwarding Shipment, job number {shipmentParentBO[JobShipmentSchema.JS_UniqueConsignRef]}.
Information|[{Env.CurrentBranch.Code}]: Successfully created 1 Transport Booking(s) for Parent Forwarding Shipment, job number {shipmentParentBO[JobShipmentSchema.JS_UniqueConsignRef]}.
Information|[{Env.CurrentBranch.Code}]: Ended processing Transport Booking creation request for Parent Forwarding Shipment, job number {shipmentParentBO[JobShipmentSchema.JS_UniqueConsignRef]}.
");
			AssertMultilineASCIIEquals("Should log failed creation of Transport Booking followed by successful creation of Transport Booking.", expectedLogOutput, logger.ToString());
		}

		DtbBookingQueue CreateTestDtbBookingQueueRecord(ZGuid parentID, ZString parentTableCode, ZString branchCode, ZString direction, bool combineContainers, short iteration = 0, string targetModule = "TB")
		{
			var queueRecord = Factory.New<DtbBookingQueue>();

			queueRecord.KMQ_ParentID = parentID;
			queueRecord.KMQ_ParentTableCode = parentTableCode;
			queueRecord.KMQ_GB_NKBranch = branchCode;
			queueRecord.KMQ_Direction = direction;
			queueRecord.KMQ_CombineContainers = combineContainers;
			queueRecord.KMQ_TargetModule = targetModule;
			queueRecord.Iteration = iteration;
			queueRecord.KMQ_SystemCreateTimeUtc = queueRecord.KMQ_SystemLastEditTimeUtc = DateTime.UtcNow;
			queueRecord.KMQ_SystemCreateUser = queueRecord.KMQ_SystemLastEditUser = "XXX";

			return queueRecord;
		}

		IForwardingShipment CreateTestForwardingShipmentWith2Containers()
		{
			var cnr = Helper.CreateOrganisation("CNRORG");
			var cyd = Helper.CreateOrganisation("CYDORG");
			var cfs = Helper.CreateOrganisation("CFSORG");
			var cto = Helper.CreateOrganisation("CTOORG");

			var shipment = Helper.CreateForwardingShipment("S00001234", "HB31278903", "SEA", "FCL");
			Helper.SetupForwardingShipmentAddresses(shipment, cnr, null, cfs, null);
			var consol = Helper.CreateForwardingConsol(shipment, "C00001432", "SEA", "MB234890232");
			Helper.SetupForwardingConsolAddresses(consol, cto, cfs, cyd, null, null, null);
			var container1 = Helper.CreateForwardingContainer(consol, "CX00001", "20GP", "SEAL1");
			var container2 = Helper.CreateForwardingContainer(consol, "CX00002", "20GP", "SEAL2");
			Helper.CreateForwardingPackline(shipment, container1, 10);
			Helper.CreateForwardingPackline(shipment, container2, 5);

			return shipment;
		}

		BusinessObject CreateTestAgencyShipmentWith2Containers()
		{
			var consignorOrg = Helper.CreateOrganisation("CNRORG");
			var consignorPickupAddress = Helper.AddAddressToOrganisation(consignorOrg, "1 Pickup Place", OrgAddressType.PickupAndDelivery);
			consignorPickupAddress.OA_RL_NKRelatedPortCode = "NZAKL";
			var consigneeOrg = Helper.CreateOrganisation("CNEORG");
			var consigneeDeliveryAddress = Helper.AddAddressToOrganisation(consigneeOrg, "2 Delivery Lane", OrgAddressType.PickupAndDelivery);
			consigneeDeliveryAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			var consigneeOfficeAddress = Helper.AddAddressToOrganisation(consigneeOrg, "1 Office Square", OrgAddressType.Office);
			var consigneeContact = consigneeOrg.Contacts.AddNew();
			consigneeContact.OC_ContactName = "Mr Mister";
			var principalOrg = Helper.CreateOrganisation("PRIORG");
			Helper.AddAddressToOrganisation(principalOrg, "2 Office Road", OrgAddressType.Office);
			var carrierOrg = Helper.CreateOrganisation("CARORG");
			var carrierAddress = Helper.AddAddressToOrganisation(carrierOrg, "10 Carry Crescent", OrgAddressType.Office);
			var bookingParty = Helper.CreateOrganisation("BKPORG");
			var bookingPartyAddress = Helper.AddAddressToOrganisation(bookingParty, "20 Book Street", OrgAddressType.Office);

			var agencyShipment = (BusinessObject)Factory.New<IAgencyShipment>();
			agencyShipment.FillWithValidTestData();
			agencyShipment[JobShipmentSchema.JS_UniqueConsignRef] = "AG000001";
			agencyShipment[JobShipmentSchema.JS_PackingMode] = "FCL";
			agencyShipment[JobShipmentSchema.JS_GoodsDescription] = "goods";
			agencyShipment[JobShipmentSchema.JS_IsForwardRegistered] = false;
			agencyShipment[JobShipmentSchema.JS_IsCFSRegistered] = false;
			agencyShipment[JobShipmentSchema.JS_IsShipping] = true;
			agencyShipment[JobShipmentSchema.JS_IsBooking] = false;

			((JobDocAddress)agencyShipment["ConsignorDocumentaryAddress"]).E2_OA_Address = consignorPickupAddress.PK;
			((JobDocAddress)agencyShipment["ConsigneeDocumentaryAddress"]).E2_OA_Address = consigneeDeliveryAddress.PK;
			((JobDocAddress)agencyShipment["BookingPartyDocumentaryAddress"]).E2_OA_Address = bookingPartyAddress.PK;
			agencyShipment[JobShipmentSchema.JS_OH_DeliveryAgent] = principalOrg.PK;
			agencyShipment[JobShipmentSchema.JS_OA_BookedShippingLineAddress] = carrierAddress.PK;
			agencyShipment[JobShipmentSchema.JS_RL_NKOrigin] = "NZAKL";
			agencyShipment[JobShipmentSchema.JS_RL_NKDestination] = "AUSYD";

			var container1 = Helper.CreateFCLAgencyShipmentContainer(agencyShipment, "CX00001", "SEAL1");
			var container2 = Helper.CreateFCLAgencyShipmentContainer(agencyShipment, "CX00002", "SEAL2");

			return agencyShipment;
		}

		BusinessObject CreateTestBillOfLadingWith2ContainersAnd1LoosePackline()
		{
			var consignorOrg = Helper.CreateOrganisation("CNRORG");
			var consignorPickupAddress = Helper.AddAddressToOrganisation(consignorOrg, "1 Pickup Place", OrgAddressType.PickupAndDelivery);
			consignorPickupAddress.OA_RL_NKRelatedPortCode = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			var consigneeOrg = Helper.CreateOrganisation("CNEORG");
			var consigneeDeliveryAddress = Helper.AddAddressToOrganisation(consigneeOrg, "2 Delivery Lane", OrgAddressType.PickupAndDelivery);
			consigneeDeliveryAddress.OA_RL_NKRelatedPortCode = "NZAKL";
			var consigneeOfficeAddress = Helper.AddAddressToOrganisation(consigneeOrg, "1 Office Square", OrgAddressType.Office);
			var consigneeContact = consigneeOrg.Contacts.AddNew();
			consigneeContact.OC_ContactName = "Mr Mister";
			var principalOrg = Helper.CreateOrganisation("PRIORG");
			Helper.AddAddressToOrganisation(principalOrg, "2 Office Road", OrgAddressType.Office);

			var billOfLading = (BusinessObject)Factory.New<IBillOfLading>();
			billOfLading.FillWithValidTestData();
			billOfLading[JobShipmentSchema.JS_UniqueConsignRef] = "VX000001";
			billOfLading[JobShipmentSchema.JS_PackingMode] = "FCL";
			billOfLading[JobShipmentSchema.JS_GoodsDescription] = "goods";
			billOfLading[JobShipmentSchema.JS_IsForwardRegistered] = false;
			billOfLading[JobShipmentSchema.JS_IsCFSRegistered] = false;
			billOfLading[JobShipmentSchema.JS_IsShipping] = true;
			billOfLading[JobShipmentSchema.JS_IsBooking] = false;
			billOfLading[JobShipmentSchema.JS_ShipmentStatus] = ShipmentStatusList.Codes.Confirmed;

			((JobDocAddress)billOfLading["ConsignorDocumentaryAddress"]).E2_OA_Address = consignorPickupAddress.PK;
			((JobDocAddress)billOfLading["ConsigneeDocumentaryAddress"]).E2_OA_Address = consigneeDeliveryAddress.PK;
			((JobDocAddress)billOfLading["NotifyPartyDocumentaryAddress"]).E2_OA_Address = consigneeDeliveryAddress.PK;
			billOfLading[JobShipmentSchema.JS_OH_DeliveryAgent] = principalOrg.PK;
			billOfLading[JobShipmentSchema.JS_RL_NKOrigin] = consignorPickupAddress.OA_RL_NKRelatedPortCode;
			billOfLading[JobShipmentSchema.JS_RL_NKDestination] = consigneeDeliveryAddress.OA_RL_NKRelatedPortCode;

			var container1 = Helper.CreateFCLBillOfLadingContainer(billOfLading, "CX00001", "SEAL1");
			var container2 = Helper.CreateFCLBillOfLadingContainer(billOfLading, "CX00002", "SEAL2");

			// as per UAT test case in WI00423255
			var loosePackLine1 = (BusinessObject)Factory.New<IAgencyPackLine>();
			loosePackLine1[JobPackLinesSchema.JL_PackageCount] = 1;
			loosePackLine1[JobPackLinesSchema.JL_F3_NKPackType] = "PLT";
			loosePackLine1[JobPackLinesSchema.JL_RH_NKCommodityCode] = "GEN";
			loosePackLine1[JobPackLinesSchema.JL_DetailedDescription] = "goods";
			loosePackLine1[JobPackLinesSchema.JL_JS] = billOfLading.PK;

			Factory.Save();

			return billOfLading;
		}

		DtbBookingCreationErrorType GetDtbBookingCreationErrorType(bool withRetry, bool withSendToErrorReporter)
		{
			if (!withRetry && !withSendToErrorReporter)
			{
				return DtbBookingCreationErrorType.ServiceHasCommencedError;
			}
			else if (withRetry && !withSendToErrorReporter)
			{
				return DtbBookingCreationErrorType.ZSaveConcurrencyError;
			}
			else if (withRetry && withSendToErrorReporter)
			{
				return DtbBookingCreationErrorType.UnknownError;
			}
			else
			{
				// if (!withRetry && withSendToErrorReporter)
				throw new NotSupportedException("Currently no such error returns such a combination, when it does we will add that test");
			}
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}
		TransportBookingTestHelper helper;

		protected override void SetUp()
		{
			base.SetUp();
			savedGlobalIsUserInteractive = Globals.IsUserInteractive;
			Globals.IsUserInteractive = false;
		}

		protected override void TearDown()
		{
			base.TearDown();
			Globals.IsUserInteractive = savedGlobalIsUserInteractive;
		}

		bool savedGlobalIsUserInteractive;
		readonly short expectedMaxRetries = 3;

		class ErrorManagerFactory
		{
			public IAutomatedDtbBookingCreationErrorManager FirstErrorManager = ObjectFactory.Get<IAutomatedDtbBookingCreationErrorManager>();
			public List<IAutomatedDtbBookingCreationErrorManager> ErrorManagers = new List<IAutomatedDtbBookingCreationErrorManager>();

			public IAutomatedDtbBookingCreationErrorManager GetNewErrorManager()
			{
				IAutomatedDtbBookingCreationErrorManager newErrorManager = null;
				if (ErrorManagers.Count == 0)
				{
					newErrorManager = FirstErrorManager;
				}
				else
				{
					newErrorManager = (IAutomatedDtbBookingCreationErrorManager)FirstErrorManager.GetType().GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>());
				}
				ErrorManagers.Add(newErrorManager);

				return newErrorManager;
			}
		}
	}
}

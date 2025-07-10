using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Packing.DataTransfer.Testing
{
	[TestedType(typeof(PkgPackageDataContextManager))]
	class PkgPackageDataContextManagerTest : DataContextManagerTestCase<PkgPackageDataContextManager, PkgPackage>
	{
		#region TestDataContextProperties

		public void TestDataContextProperties()
		{
			var package = Factory.New<PkgPackage>();
			package.KP_PackageID = "P1";
			var dataContextManager = (package.GetUniversalDataContextManager() as IEventDataContextManager);
			AssertEquals("P1", dataContextManager.DataContextKey);
			AssertEquals(DataContextType.PkgPackage, dataContextManager.DataContextType);
			AssertNull(dataContextManager.DefaultOutputDirectory);
		}

		#endregion

		#region TestEventContextValues

		public void TestEventContextValues()
		{
			DummyWithWorkflow.TypeDecider.TypeForLoadOverride = typeof(DummyBizOWithPackingAndTransportCompany);
			var dummyWithTransportComapny = Factory.New<DummyBizOWithPackingAndTransportCompany>();
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = dummyWithTransportComapny.PK;
			packageJob.KJ_ParentTableCode = "Z0";
			var package = packageJob.Packages.AddNew();
			package.KP_PackageID = "P1";
			package.KP_Sequence = 2;
			package.KP_F3_NKPackType = "BOX";
			package.KP_Length = 1m;
			package.KP_Width = 2m;
			package.KP_DimensionUQ = "M";
			package.KP_Weight = 3.5m;
			package.KP_WeightUQ = "KG";
			package.KP_Volume = 0.8m;
			package.KP_VolumeUQ = "M3";
			package.KP_PackageQty = 1;

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("BOX")).F3_UOMType = "AAA";

			AssertMultilineASCIIEquals("managerGetEventContextValues()", @"
TransportBookingPackageID - P1
PackageSequence - 2
PackageType - BOX
DimensionUnit - M
Length - 1.000
Width - 2.000
Height - 0.000
WeightOfGoods - 3.500 KG
VolumeOfGoods - 0.800 M3
PackageTypeUOM - AAA
NumberOfPieces - 1
			".Trim(), (package.GetUniversalDataContextManager() as IEventDataContextManager).EventContextValues.ToStringContents(e => e.Key + " - " + e.Value));
		}

		public void TestEventContextValues_NullPackType()
		{
			DummyWithWorkflow.TypeDecider.TypeForLoadOverride = typeof(DummyBizOWithPackingAndTransportCompany);
			var dummyWithTransportComapny = Factory.New<DummyBizOWithPackingAndTransportCompany>();

			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = dummyWithTransportComapny.PK;
			packageJob.KJ_ParentTableCode = "Z0";

			var package = packageJob.Packages.AddNew();
			package.KP_F3_NKPackType = "NAH";

			AssertNull("Precondition.", Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("NAH")));

			AssertMultilineASCIIEquals("managerGetEventContextValues()", @"
PackageSequence - 1
PackageType - NAH
NumberOfPieces - 1
			".Trim(), (package.GetUniversalDataContextManager() as IEventDataContextManager).EventContextValues.ToStringContents(e => e.Key + " - " + e.Value));
		}

		public void TestEventContextValues_TransportBookingJobID()
		{
			var booking = Factory.New<DtbBooking>();
			booking.KM_JobID = "TB00001";
			var consignment = Factory.New<DtbConsignment>();
			consignment.LTC_KM_Booking = booking.PK;
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = consignment.PK;
			packageJob.KJ_ParentTableCode = "LTC";
			var package = packageJob.Packages.AddNew();
			package.KP_PackageID = "P1";
			package.KP_Sequence = 2;
			package.KP_F3_NKPackType = "BOX";

			AssertMultilineASCIIEquals("managerGetEventContextValues()", $@"
TransportBookingJobID - {booking.KM_JobID}
TransportBookingPackageID - P1
PackageSequence - 2
PackageType - BOX
NumberOfPieces - 1
			".Trim(), (package.GetUniversalDataContextManager() as IEventDataContextManager).EventContextValues.ToStringContents(e => e.Key + " - " + e.Value));
		}

		public void TestEventContextValues_NoDimensionValues()
		{
			DummyWithWorkflow.TypeDecider.TypeForLoadOverride = typeof(DummyBizOWithPackingAndTransportCompany);
			var dummyWithTransportComapny = Factory.New<DummyBizOWithPackingAndTransportCompany>();
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = dummyWithTransportComapny.PK;
			packageJob.KJ_ParentTableCode = "Z0";
			var package = packageJob.Packages.AddNew();
			package.KP_PackageID = "P1";
			package.KP_Sequence = 2;
			package.KP_F3_NKPackType = "BOX";

			AssertMultilineASCIIEquals("managerGetEventContextValues()", @"
TransportBookingPackageID - P1
PackageSequence - 2
PackageType - BOX
NumberOfPieces - 1
			".Trim(), (package.GetUniversalDataContextManager() as IEventDataContextManager).EventContextValues.ToStringContents(e => e.Key + " - " + e.Value));
		}

		public void TestEventContextValues_GetValuesFromPackageJobParent()
		{
			DummyWithWorkflow.TypeDecider.TypeForLoadOverride = typeof(DummyBizOWithPackingHasAdditionalEventContext);
			var dummyBizO = Factory.New<DummyBizOWithPackingHasAdditionalEventContext>();
			dummyBizO.OrderNumber = "W0001";
			dummyBizO.PickNumber = "P0001";

			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = dummyBizO.PK;
			packageJob.KJ_ParentTableCode = "Z0";
			var package = packageJob.Packages.AddNew();
			package.KP_PackageID = "P1";
			package.KP_Sequence = 2;
			package.KP_F3_NKPackType = "BOX";

			AssertMultilineASCIIEquals("managerGetEventContextValues()", @"
TransportBookingPackageID - P1
PackageSequence - 2
PackageType - BOX
NumberOfPieces - 1
OrderNumber - W0001
PickNumber - P0001
			".Trim(), (package.GetUniversalDataContextManager() as IEventDataContextManager).EventContextValues.ToStringContents(e => e.Key + " - " + e.Value));
		}

		public class DummyBizOWithPackingHasAdditionalEventContext : DummyWithPacking
		{
			public DummyBizOWithPackingHasAdditionalEventContext(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZString OrderNumber { get; set; }
			public ZString PickNumber { get; set; }

			protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetAdditionalEventContextValuesFromParentCore()
			{
				var contextValues = new List<KeyValuePair<TypeWithDescription, IZType>>();

				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.OrderNumber, OrderNumber);
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.PickNumber, PickNumber);

				return contextValues;
			}
		}

		#endregion

		#region TestVolCam

		#region TestVolCam_PackagesShouldNotUpdateIfEventIsInvalid

		public void TestVolCam_PackagesShouldNotUpdateIfEventIsInvalid()
		{
			var eventDeserializer = new XmlEventDeserializer();

			var originalPackage = Factory.New<PkgPackage>();
			originalPackage.KP_Weight = 1;

			originalPackage.KP_Width = 2;
			originalPackage.KP_Height = 3;
			originalPackage.KP_Length = 4;

			originalPackage.KP_Volume = 5;

			var dataContextManager = (IEventDataContextManager)(originalPackage.GetUniversalDataContextManager());

			var xmlEvent = eventDeserializer.Parse(InvalidVolCamEvent);
			dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);

			AssertEquals("Original package shouldn't have changed", 1m, originalPackage.KP_Weight);
			AssertEquals("Original package shouldn't have changed", 2m, originalPackage.KP_Width);
			AssertEquals("Original package shouldn't have changed", 3m, originalPackage.KP_Height);
			AssertEquals("Original package shouldn't have changed", 4m, originalPackage.KP_Length);
			AssertEquals("Original package shouldn't have changed", 5m, originalPackage.KP_Volume);
		}

		#endregion

		#region TestVolCam_PackagesShouldNotUpdateIfEventIsNotScannedEvent

		public void TestVolCam_PackagesShouldNotUpdateIfEventIsNotScannedEvent()
		{
			var eventDeserializer = new XmlEventDeserializer();

			var originalPackage = Factory.New<PkgPackage>();
			originalPackage.KP_Weight = 1;
			originalPackage.KP_Width = 2;
			originalPackage.KP_Height = 3;
			originalPackage.KP_Length = 4;
			originalPackage.KP_Volume = 5;

			var dataContextManager = (IEventDataContextManager)(originalPackage.GetUniversalDataContextManager());

			var xmlEvent = eventDeserializer.Parse(RandomEventWithValidVolCamData);
			dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);

			AssertEquals("Original package shouldn't have changed", 1m, originalPackage.KP_Weight);
			AssertEquals("Original package shouldn't have changed", 2m, originalPackage.KP_Width);
			AssertEquals("Original package shouldn't have changed", 3m, originalPackage.KP_Height);
			AssertEquals("Original package shouldn't have changed", 4m, originalPackage.KP_Length);
			AssertEquals("Original package shouldn't have changed", 5m, originalPackage.KP_Volume);
		}

		#endregion

		#region TestVolCam_PackagesShouldMaintainOriginalUnitsWhenUpdated

		public void TestVolCam_PackagesShouldMaintainOriginalUnitsWhenUpdated()
		{
			var eventDeserializer = new XmlEventDeserializer();

			var originalPackage = Factory.New<PkgPackage>();
			originalPackage.KP_Weight = 1;
			originalPackage.KP_WeightUQ = "LB";

			originalPackage.KP_Width = 10;
			originalPackage.KP_Height = 10;
			originalPackage.KP_Length = 10;
			originalPackage.KP_DimensionUQ = "IN";

			originalPackage.KP_Volume = 0.021952m;
			originalPackage.KP_VolumeUQ = "CY";

			var dataContextManager = (IEventDataContextManager)(originalPackage.GetUniversalDataContextManager());

			using (PackingRegistry.Instance.VolumeUnit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "CC"))
			using (PackingRegistry.Instance.DimensionUnit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "CM"))
			using (PackingRegistry.Instance.WeightUnit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "KG"))
			{
				var xmlEvent = eventDeserializer.Parse(ValidVolCamEvent);
				dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);

				AssertPackageIsUpdated(originalPackage);
			}
		}

		#endregion

		#region TestVolCam_PackagesShouldUsePackageRegistryDefaults

		public void TestVolCam_PackagesShouldUsePackageRegistryDefaults()
		{
			var eventDeserializer = new XmlEventDeserializer();

			using (PackingRegistry.Instance.VolumeUnit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "CY"))
			using (PackingRegistry.Instance.DimensionUnit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "IN"))
			using (PackingRegistry.Instance.WeightUnit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "LB"))
			{
				var originalPackage = Factory.New<PkgPackage>();

				originalPackage.KP_VolumeUQ = string.Empty;
				originalPackage.KP_DimensionUQ = string.Empty;
				originalPackage.KP_WeightUQ = string.Empty;

				AssertEquals("Precondition - Volume unit should be empty", string.Empty, originalPackage.KP_VolumeUQ);
				AssertEquals("Precondition - Dimension unit should be empty", string.Empty, originalPackage.KP_DimensionUQ);
				AssertEquals("Precondition - Weight unit should be empty", string.Empty, originalPackage.KP_WeightUQ);

				var dataContextManager = (IEventDataContextManager)(originalPackage.GetUniversalDataContextManager());

				var xmlEvent = eventDeserializer.Parse(ValidVolCamEvent);
				dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);

				AssertPackageIsUpdated(originalPackage);
			}
		}

		#endregion

		#region VolCam Assert

		void AssertPackageIsUpdated(PkgPackage package)
		{
			AssertEquals("Package should update to Metric UoM - Weight", "KG", package.KP_WeightUQ);
			AssertEquals("Package should update to Metric value - Weight", 1.200m, package.KP_Weight);

			AssertEquals("Package should maintain the same unit - Dimension", "CM", package.KP_DimensionUQ);
			AssertEquals("Package should update to Metric value - Length", 10m, package.KP_Length);
			AssertEquals("Package should update to Metric value - Width", 20m, package.KP_Width);
			AssertEquals("Package should update to Metric value - Height", 30m, package.KP_Height);

			AssertEquals("Package should update to Imperial UoM - Volume", "CC", package.KP_VolumeUQ);
			AssertEquals("Package should update to Imperial value - Volume", 6000m, package.KP_Volume);
		}

		#endregion

		#endregion

		#region TestStatusUpdate

		#region TestStatusUpdate_PackageUpdated

		public void TestStatusUpdate_PackageUpdated()
		{
			var eventDeserializer = new XmlEventDeserializer();
			Data.CreatePackingData();
			var originalPackage = Helper.CreatePackage(Data.PackageJob, "P0001", 1, "BOX");
			var originalPackageID = originalPackage.KP_PackageID;

			var dataContextManager = (IEventDataContextManager)(originalPackage.GetUniversalDataContextManager());
			var xmlEvent = eventDeserializer.Parse(STUEvent);
			dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);

			AssertEquals("KP_PackageID is updated with the value from the RFN parameter of the event.", "132132132", originalPackage.KP_PackageID);
			AssertEquals("KP_PreviousPackageID is updated with the value of the previous KP_PackageID", originalPackageID, originalPackage.KP_PreviousPackageID);
		}

		#endregion

		#region TestStatusUpdate_ParentJobFinalised

		public void TestStatusUpdate_ParentJobFinalised()
		{
			var eventDeserializer = new XmlEventDeserializer();
			Data.CreatePackingData();
			Data.PackageJob.KJ_IsFinalized = true;
			var originalPackage = Helper.CreatePackage(Data.PackageJob, "P0001", 1, "BOX");
			var originalPackageID = originalPackage.KP_PackageID;

			AssertEquals("Precondition: parent package job is finalized.", true, Data.PackageJob.KJ_IsFinalized);
			var dataContextManager = (IEventDataContextManager)(originalPackage.GetUniversalDataContextManager());

			var xmlEvent = eventDeserializer.Parse(STUEvent);
			dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);

			AssertEquals("KP_PackageID is updated with the value from the RFN parameter of the event.", "132132132", originalPackage.KP_PackageID);
			AssertEquals("KP_PreviousPackageID is updated with the value of the previous KP_PackageID", originalPackageID, originalPackage.KP_PreviousPackageID);
		}

		#endregion

		#region TestStatusUpdate_ParentJobUnFinalised

		public void TestStatusUpdate_ParentJobUnFinalised()
		{
			var eventDeserializer = new XmlEventDeserializer();
			Data.CreatePackingData();
			var originalPackage = Helper.CreatePackage(Data.PackageJob, "P0001", 1, "BOX");
			var originalPackageID = originalPackage.KP_PackageID;

			AssertEquals("Precondition: parent package job is not finalized.", false, Data.PackageJob.KJ_IsFinalized);
			var dataContextManager = (IEventDataContextManager)(originalPackage.GetUniversalDataContextManager());

			var xmlEvent = eventDeserializer.Parse(STUEvent);
			dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);

			AssertEquals("KP_PackageID is updated with the value from the RFN parameter of the event.", "132132132", originalPackage.KP_PackageID);
			AssertEquals("KP_PreviousPackageID is updated with the value of the previous KP_PackageID", originalPackageID, originalPackage.KP_PreviousPackageID);
		}

		#endregion

		#region TestStatusUpdate_PackageWithExistingPreviousPackageIDValue

		public void TestStatusUpdate_PackageWithExistingPreviousPackageIDValue()
		{
			var eventDeserializer = new XmlEventDeserializer();
			Data.CreatePackingData();
			var originalPackage = Helper.CreatePackage(Data.PackageJob, "P0001", 1, "BOX");
			originalPackage.KP_PackageID = "P0001";
			var originalPackageID = originalPackage.KP_PackageID;

			var dataContextManager = (IEventDataContextManager)(originalPackage.GetUniversalDataContextManager());

			var xmlEvent = eventDeserializer.Parse(STUEvent);
			dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);

			AssertEquals("KP_PackageID is updated with the value from the RFN parameter of the event.", "132132132", originalPackage.KP_PackageID);
			AssertEquals("KP_PreviousPackageID is updated with the value of the previous KP_PackageID", originalPackageID, originalPackage.KP_PreviousPackageID);
		}

		#endregion

		#region TestStatusUpdate_EventWithNoEventReference

		public void TestStatusUpdate_EventWithNoEventReference()
		{
			var eventDeserializer = new XmlEventDeserializer();
			Data.CreatePackingData();
			var originalPackage = Helper.CreatePackage(Data.PackageJob, "123456789", 1, "BOX");
			originalPackage.KP_PreviousPackageID = "987654321";

			var dataContextManager = (IEventDataContextManager)(originalPackage.GetUniversalDataContextManager());

			var xmlEvent = eventDeserializer.Parse(STUEventWithNoEventReference);
			dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);

			AssertEquals("KP_PackageID is not updated.", "123456789", originalPackage.KP_PackageID);
			AssertEquals("KP_PreviousPackageID is not updated.", "987654321", originalPackage.KP_PreviousPackageID);
		}

		#endregion

		#region TestStatusUpdate_EventWithWrongReferenceType

		public void TestStatusUpdate_EventWithWrongReferenceType()
		{
			var eventDeserializer = new XmlEventDeserializer();
			Data.CreatePackingData();
			var originalPackage = Helper.CreatePackage(Data.PackageJob, "123456789", 1, "BOX");
			originalPackage.KP_PreviousPackageID = "987654321";

			var dataContextManager = (IEventDataContextManager)(originalPackage.GetUniversalDataContextManager());

			var xmlEvent = eventDeserializer.Parse(STUEventWithWrongReferenceType);
			dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);

			AssertEquals("KP_PackageID is not updated.", "123456789", originalPackage.KP_PackageID);
			AssertEquals("KP_PreviousPackageID is not updated.", "987654321", originalPackage.KP_PreviousPackageID);
		}

		#endregion

		#region TestStatusUpdate_EventWithNoReferenceNumber

		public void TestStatusUpdate_EventWithNoReferenceNumber()
		{
			var eventDeserializer = new XmlEventDeserializer();
			Data.CreatePackingData();
			var originalPackage = Helper.CreatePackage(Data.PackageJob, "123456789", 1, "BOX");
			originalPackage.KP_PreviousPackageID = "987654321";

			var dataContextManager = (IEventDataContextManager)(originalPackage.GetUniversalDataContextManager());

			var xmlEvent = eventDeserializer.Parse(STUEventWithNoEventReferenceNumber);
			AssertExceptionThrown<DataObjectReadFailureException>(() => dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent));

			AssertEquals("KP_PackageID is not updated.", "123456789", originalPackage.KP_PackageID);
			AssertEquals("KP_PreviousPackageID is not updated.", "987654321", originalPackage.KP_PreviousPackageID);
		}

		#endregion

		#region TestStatusUpdate_CorrectEventDisplayReference

		public void TestStatusUpdate_CorrectEventDisplayReference()
		{
			Data.CreatePackingData();
			var package = Helper.CreatePackage(Data.PackageJob, "P0001", 1, "BOX");
			package.KP_PackageID = "P0001";
			var originalPackageID = package.KP_PackageID;
			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(STUEvent);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var logEvent = package.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode).First();
			AssertNotNull("There should be an 'STU' event log on the package.", logEvent);
			AssertEquals("Status Updated: Carrier Barcode No., Reference No. 132132132, because Carrier Label was generated", logEvent.DisplayEventReference);
			AssertEquals("KP_PackageID is updated with value from the RFN parameter of the event.", "132132132", package.KP_PackageID);
			AssertEquals("KP_PreviousPackageID is updated with value from RFN parameter of the event.", originalPackageID, package.KP_PreviousPackageID);
		}

		#endregion

		#region TestEventAdded_NotStatusUpdatedEvent

		public void TestEventAdded_NotStatusUpdatedEvent()
		{
			var eventDeserializer = new XmlEventDeserializer();
			Data.CreatePackingData();
			var originalPackage = Helper.CreatePackage(Data.PackageJob, "987654321", 1, "BOX");

			var dataContextManager = (IEventDataContextManager)(originalPackage.GetUniversalDataContextManager());
			var xmlEvent = eventDeserializer.Parse(OtherEvent);
			dataContextManager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);

			AssertEquals("KP_PackageID is not updated with the value from the RFN parameter of the event.", "987654321", originalPackage.KP_PackageID);
		}

		#endregion

		#region TestStatusUpdate_EventRejected_EmptyPackageId

		public void TestStatusUpdate_EventRejected_EmptyPackageId()
		{
			Data.CreatePackingData();
			var package = Helper.CreatePackage(Data.PackageJob, "P0001", 1, "BOX");
			package.KP_PackageID = "P0001";
			package.KP_PreviousPackageID = "987654321";
			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(STUEventWithEmptyReferenceNumber);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals("KP_PackageID is not updated.", "P0001", package.KP_PackageID);
			AssertEquals("KP_PreviousPackageID is not updated.", "987654321", package.KP_PreviousPackageID);
			AssertEquals("There should be no 'STU' event log propagated to the parent job.", false, package.PackageJob.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode).Any());
			AssertEquals("Update status event rejected log is added.", true,
				manager.Logger.Logs.Any(log => log.Message == "Package ID has not been updated because the Carrier Barcode Number is either empty or is already assigned to another package."));
		}

		#endregion

		#region TestStatusUpdate_EventRejected_DuplicatePackageId()

		public void TestStatusUpdate_EventRejected_DuplicatePackageId()
		{
			Data.CreatePackingData();
			var package = Helper.CreatePackage(Data.PackageJob, "P0001", 1, "BOX");
			package.KP_PackageID = "P0001";
			package.KP_PreviousPackageID = "987654321";

			Helper.CreatePackage(Data.PackageJob, "132132132", 1, "BOX");
			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(STUEvent);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals("KP_PackageID is not updated.", "P0001", package.KP_PackageID);
			AssertEquals("KP_PreviousPackageID is not updated.", "987654321", package.KP_PreviousPackageID);
			AssertEquals("There should be no 'STU' event log propagated to the parent job.", false, package.PackageJob.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode).Any());
			AssertEquals("Update status event rejected log is added.", true,
				manager.Logger.Logs.Any(log => log.Message == "Package ID has not been updated because the Carrier Barcode Number is either empty or is already assigned to another package."));
		}

		#endregion

		#endregion

		#region Implementation

		#region VolCamXML

		readonly string ValidVolCamEvent = @"<UniversalEvent>
	<Event>
		<EventTime>2016-12-25T01:02:03.001</EventTime>
		<EventType>SSC</EventType>
		<EventReference>FAC=TWS|LOC=AUSYD|WGT=1.200KG|LEN=10CM|WID=20CM|HGT=30CM|VOL=6000CC</EventReference>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>TransitReceive</Type>
				</DataTarget>
			</DataTargetCollection>
			<Company>
				<Code>TMS</Code>
			</Company>
			<EnterpriseID>EDI</EnterpriseID>
			<ServerID>EVP</ServerID>
		</DataContext>
		<ContextCollection>
			<Context>
				<Type>GoodsItemID</Type>
				<Value>TESTBARCODE123456789</Value>
			</Context>
			<Context>
				<Type>WarehouseCode</Type>
				<Value>TRA</Value>
			</Context>
			<Context>
				<Type>VolCamPlatformID</Type>
				<Value>00123</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		readonly string InvalidVolCamEvent = @"<UniversalEvent>
	<Event>
		<EventTime>2016-12-25T01:02:03.001</EventTime>
		<EventType>SSC</EventType>
		<EventReference>FAC=TWS|LOC=AUSYD|WGT=1.200|LEN=10|WID=20|HGT=30|VOL=6000</EventReference>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>TransitReceive</Type>
				</DataTarget>
			</DataTargetCollection>
			<Company>
				<Code>TMS</Code>
			</Company>
			<EnterpriseID>EDI</EnterpriseID>
			<ServerID>EVP</ServerID>
		</DataContext>
		<ContextCollection>
			<Context>
				<Type>GoodsItemID</Type>
				<Value>TESTBARCODE123456789</Value>
			</Context>
			<Context>
				<Type>WarehouseCode</Type>
				<Value>TRA</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		readonly string RandomEventWithValidVolCamData = @"<UniversalEvent>
	<Event>
		<EventTime>2016-12-25T01:02:03.001</EventTime>
		<EventType>XXX</EventType>
		<EventReference>FAC=TWS|LOC=AUSYD|WGT=1.200KG|LEN=10CM|WID=20CM|HGT=30CM|VOL=6000CC</EventReference>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>TransitReceive</Type>
				</DataTarget>
			</DataTargetCollection>
			<Company>
				<Code>TMS</Code>
			</Company>
			<EnterpriseID>EDI</EnterpriseID>
			<ServerID>EVP</ServerID>
		</DataContext>
		<ContextCollection>
			<Context>
				<Type>GoodsItemID</Type>
				<Value>TESTBARCODE123456789</Value>
			</Context>
			<Context>
				<Type>WarehouseCode</Type>
				<Value>TRA</Value>
			</Context>
			<Context>
				<Type>VolCamPlatformID</Type>
				<Value>00123</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		#endregion

		#region STUEventXML

		const string STUEvent = @"<UniversalEvent>
	<Event>
		<EventTime>2018-04-01T01:02:03.001</EventTime>
		<EventType>STU</EventType>
		<EventReference>|TYP=CBN|RFN=132132132|RES=Carrier Label was generated</EventReference>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>PkgPackage</Type>
				</DataTarget>
			</DataTargetCollection>
			<Company>
				<Code>TMS</Code>
			</Company>
			<EnterpriseID>EDI</EnterpriseID>
			<ServerID>EVP</ServerID>
		</DataContext>
		<ContextCollection>
			<Context>
				<Type>ClientReference</Type>
				<Value>O123</Value>
			</Context>
			<Context>
				<Type>TransportBookingPackageID</Type>
				<Value>P0001</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		const string STUEventWithNoEventReference = @"<UniversalEvent>
	<Event>
		<EventTime>2018-04-01T01:02:03.001</EventTime>
		<EventType>STU</EventType>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>PkgPackage</Type>
				</DataTarget>
			</DataTargetCollection>
			<Company>
				<Code>TMS</Code>
			</Company>
			<EnterpriseID>EDI</EnterpriseID>
			<ServerID>EVP</ServerID>
		</DataContext>
		<ContextCollection>
			<Context>
				<Type>ClientReference</Type>
				<Value>O123</Value>
			</Context>
			<Context>
				<Type>TransportBookingPackageID</Type>
				<Value>P0001</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		const string STUEventWithWrongReferenceType = @"<UniversalEvent>
	<Event>
		<EventTime>2018-04-01T01:02:03.001</EventTime>
		<EventType>STU</EventType>
		<EventReference>|TYP=GAR|RFN=132132132|RES=Carrier Label was generated</EventReference>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>PkgPackage</Type>
				</DataTarget>
			</DataTargetCollection>
			<Company>
				<Code>TMS</Code>
			</Company>
			<EnterpriseID>EDI</EnterpriseID>
			<ServerID>EVP</ServerID>
		</DataContext>
		<ContextCollection>
			<Context>
				<Type>ClientReference</Type>
				<Value>O123</Value>
			</Context>
			<Context>
				<Type>TransportBookingPackageID</Type>
				<Value>P0001</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		const string STUEventWithEmptyReferenceNumber = @"<UniversalEvent>
	<Event>
		<EventTime>2018-04-01T01:02:03.001</EventTime>
		<EventType>STU</EventType>
		<EventReference>|TYP=CBN|RFN= |RES=Carrier Label was generated</EventReference>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>PkgPackage</Type>
				</DataTarget>
			</DataTargetCollection>
			<Company>
				<Code>TMS</Code>
			</Company>
			<EnterpriseID>EDI</EnterpriseID>
			<ServerID>EVP</ServerID>
		</DataContext>
		<ContextCollection>
			<Context>
				<Type>ClientReference</Type>
				<Value>O123</Value>
			</Context>
			<Context>
				<Type>TransportBookingPackageID</Type>
				<Value>P0001</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		const string STUEventWithNoEventReferenceNumber = @"<UniversalEvent>
	<Event>
		<EventTime>2018-04-01T01:02:03.001</EventTime>
		<EventType>STU</EventType>
		<EventReference>|TYP=CBN|RES=Carrier Label was generated</EventReference>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>PkgPackage</Type>
				</DataTarget>
			</DataTargetCollection>
			<Company>
				<Code>TMS</Code>
			</Company>
			<EnterpriseID>EDI</EnterpriseID>
			<ServerID>EVP</ServerID>
		</DataContext>
		<ContextCollection>
			<Context>
				<Type>ClientReference</Type>
				<Value>O123</Value>
			</Context>
			<Context>
				<Type>TransportBookingPackageID</Type>
				<Value>P0001</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		#endregion

		#region OtherEventXML

		const string OtherEvent = @"<UniversalEvent>
	<Event>
		<EventTime>2018-04-01T01:02:03.001</EventTime>
		<EventType>ACT</EventType>
		<EventReference>|TYP=CBN|RFN=132132132|RES=Carrier Label was generated</EventReference>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>PkgPackage</Type>
				</DataTarget>
			</DataTargetCollection>
			<Company>
				<Code>TMS</Code>
			</Company>
			<EnterpriseID>EDI</EnterpriseID>
			<ServerID>EVP</ServerID>
		</DataContext>
		<ContextCollection>
			<Context>
				<Type>PackageID</Type>
				<Value>P0001</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPacking);
			DummyDependantBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackableItemParent);
		}

		PackingTestHelper Helper => helper ?? (helper = new PackingTestHelper(Factory.BOFactory));
		PackingTestHelper helper;

		TestDataForPacking Data => data ?? (data = new TestDataForPacking(Factory.BOFactory));
		TestDataForPacking data;

		#endregion
	}
}

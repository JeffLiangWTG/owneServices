using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Options;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportBookings.DataTransfer.Universal.Testing
{
	class DtbBookingParentDataObjectWriterTest : DtbBookingTestCaseWithFactory
	{
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

		public void TestDtbBookingParentDataObjectWriter_ContainarOptions()
		{
			var shipment = Helper.CreateForwardingShipment("S00001234", "HB31278903", "SEA", "FCL");
			var consol = Helper.CreateForwardingConsol(shipment, "C00001432", "SEA", "MB234890232");

			Factory.Save();

			var parentBO = (BusinessObject)shipment;
			var parentManager = parentBO.GetUniversalDataContextManager() as IShipmentDataContextManager;
			var parentWriter = parentManager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, parentBO)));
			var topLevelDO = (UniversalShipment)parentWriter.GetDataObject(parentBO);

			var options = new TransportBookingDocumentOptions((IDtbBookingParent)parentBO, Factory, DataContextType.ForwardingShipment, DtbBookingDirection.PIC, false, "SEA", "FCL", true);
			options.Containers.Add(new DtbDocumentContainerOption("123", "ABC", "SEAL", 1, "RELEASENUM123") { DeliverContainer = true });
			var writer = new DtbBookingParentDataObjectWriter(options, topLevelDO, new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, parentBO)));
			var dataObject = writer.GetDataObject(parentBO);
			var container = dataObject.SubShipmentCollection.Single().SubShipmentCollection.Single().ContainerCollection.Single();
			AssertEquals("123", container.ContainerNumber);
			AssertEquals("ABC", container.ContainerType.Code);
			AssertEquals("SEAL", container.Seal);
			AssertEquals(1, container.Link);
			AssertEquals("RELEASENUM123", container.ReleaseNum);
		}

		public void TestDtbBookingParentDataObjectWriter_Branch_GUI()
		{
			TestDtbBookingParentDataObjectWriter_Branch_Core(true);
		}

		public void TestDtbBookingParentDataObjectWriter_Branch_ServiceTask()
		{
			TestDtbBookingParentDataObjectWriter_Branch_Core(false);
		}

		void TestDtbBookingParentDataObjectWriter_Branch_Core(bool isUserInteractive)
		{
			var shipment = Helper.CreateForwardingShipment("S00001234", "HB31278903", "SEA", "FCL");
			var consol = Helper.CreateForwardingConsol(shipment, "C00001432", "SEA", "MB234890232");

			Factory.Save();

			var parentBO = (BusinessObject)shipment;
			var parentManager = parentBO.GetUniversalDataContextManager() as IShipmentDataContextManager;
			var parentWriter = parentManager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, parentBO)));
			var topLevelDO = (UniversalShipment)parentWriter.GetDataObject(parentBO);

			var options = new TransportBookingDocumentOptions((IDtbBookingParent)parentBO, Factory, DataContextType.ForwardingShipment, DtbBookingDirection.PIC, false, "SEA", "FCL", true);
			var writer = new DtbBookingParentDataObjectWriter(options, topLevelDO, new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, parentBO)));

			Globals.IsUserInteractive = isUserInteractive;

			var dataObject = writer.GetDataObject(parentBO);
			if (isUserInteractive)
			{
				AssertEquals("", GlbBranch.CurrentBranch.GB_Code, dataObject.Branch.Code);
			}
			else
			{
				AssertNull("", dataObject.Branch);
			}
		}

		public void TestDataObjectSubShipmentCollectionType()
		{
			var parentBO = (BusinessObject)Helper.CreateForwardingShipment("S00001234", "HB31278903", "SEA", "FCL");
			var parentManager = parentBO.GetUniversalDataContextManager() as IShipmentDataContextManager;
			var topLevelDO = (UniversalShipment)parentManager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, parentBO))).GetDataObject(parentBO);

			var options = new TransportBookingDocumentOptions((IDtbBookingParent)parentBO, Factory, DataContextType.ForwardingShipment, DtbBookingDirection.PIC, false, "SEA", "FCL", true);
			options.Containers.Add(new DtbDocumentContainerOption("123", "ABC", "SEAL", 1, "RELEASENUM123") { DeliverContainer = true });
			var dataObject = new DtbBookingParentDataObjectWriter(options, topLevelDO, new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, parentBO))).GetDataObject(parentBO);
			AssertNotNull(dataObject.SubShipmentCollection);
			AssertType<DataObjectList<UniversalShipment>>(dataObject.SubShipmentCollection);
			AssertEquals(1, dataObject.SubShipmentCollection.Count);
		}
	}
}

using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.eTail.DataTransfer.Universal;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Core.Constants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.eTail.DataTransfer.Testing
{
	class HVLShipmentWritingHelperTest : TestCaseWithFactory
	{
		public void TestSkipPopulatingMilestones()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			var milestone = consignment.WorkflowItems.AddNew();
			milestone.IsMilestone = true;

			Factory.Save();

			Assert("precondition: consignment has Milestones", consignment.WorkflowItems.Milestones.Any());

			//Load from another factory so consignment.WorkflowItems can be reinitialised
			var shipmentFromOtherFactory = new BusinessObjectFactory().ImportFromAnotherFactory(shipment) as ForwardingShipment;

			var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(null, shipmentFromOtherFactory)), true, true);
			var dataObject = writer.GetDataObject(shipmentFromOtherFactory);

			var dataObjectForConsignment = dataObject.SubShipmentCollection.Single();

			AssertNull("Process tasks should not be exported to XML for HVLVConsignment", dataObjectForConsignment.MilestoneCollection);
		}

		public void TestPopulateGoodsLocation()
		{
			var shipment = Factory.New<ForwardingShipment>();
			HVLVConsignmentHeader.GetOrCreate(shipment);
			var shipmentCFSAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			var consolCFSAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress;

			shipment.JS_E_DEP = ZDateTime.BrettsBirthday;
			shipment.JS_E_ARV = ZDateTime.BrettsBirthday.AddDays(1);
			shipment.JS_OA_ImportReleaseDepot = shipmentCFSAddress.PK;
			var consol = shipment.Consols.AddNew();
			consol.JK_OA_UnpackDepotAddress = consolCFSAddress.PK;

			Factory.Save();

			var shipmentWriter = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(null, shipment)), true, true);
			var universalShipment = shipmentWriter.GetDataObject(shipment);
			var writer = new HVLShipmentWritingHelper(new DataWritingManager(new ActionInfo(RecipientRoleType.DCA, shipment)), shipment, universalShipment, false);
			((IHVLShipmentWritingHelper)writer).WriteConsignmentsAsSubShipments();

			var orgAddressCollections = universalShipment.OrganizationAddressCollection;
			AssertEquals("Shipment's CFS Address is written.", shipmentCFSAddress.Header.OH_Code, orgAddressCollections.FirstOrDefault(nameof(DocAddressType.GoodsLocation))?.OrganizationCode);

			shipment.JS_OA_ImportReleaseDepot = ZGuid.Empty;
			Factory.Save();

			universalShipment = shipmentWriter.GetDataObject(shipment);
			writer = new HVLShipmentWritingHelper(new DataWritingManager(new ActionInfo(RecipientRoleType.DCA, shipment)), shipment, universalShipment, false);
			((IHVLShipmentWritingHelper)writer).WriteConsignmentsAsSubShipments();

			orgAddressCollections = universalShipment.OrganizationAddressCollection;
			AssertEquals("Consol's CFS Address is written.", consolCFSAddress.Header.OH_Code, orgAddressCollections.FirstOrDefault(nameof(DocAddressType.GoodsLocation))?.OrganizationCode);
		}

		#region TestGetConsignmentWriter

		public void TestGetConsignmentWriter_UseDefaultHVLVConsignmentDataExportStrategy_NotPublishingInternally()
		{
			(var writerManager, var writer) = GetWriterManagerAndWriter(false);

			AssertType<HVLVConsignmentDataObjectWriter>(writer);
			var consignmentWriter = writer as HVLVConsignmentDataObjectWriter;

			CombineAssertions("use DefaultHVLVConsignmentDataExportStrategy to write consignment data when writerManager.IsPublishingInternally is false", () =>
			{
				Assert("Precondition : writerManager.IsPublishingInternally is false", !writerManager.IsPublishingInternally);
				AssertType<DefaultHVLVConsignmentDataExportStrategy>(consignmentWriter.DataExportStrategy);
			});
		}

		public void TestGetConsignmentWriter_UseHVLVConsignmentToCargoReportDataExportStrategy_IsPublishingInternally()
		{
			(var writerManager, var writer) = GetWriterManagerAndWriter(true, includeAdditionalReferenceCollectionOverride: false);

			AssertType<HVLVConsignmentDataObjectWriter>(writer);
			var consignmentWriter = writer as HVLVConsignmentDataObjectWriter;

			CombineAssertions("use HVLVConsignmentToCargoReportDataExportStrategy to write consignment data when writerManager.IsPublishingInternally is true", () =>
			{
				Assert("Precondition : writerManager.IsPublishingInternally is true", writerManager.IsPublishingInternally);
				AssertType<HVLVConsignmentToCargoReportDataExportStrategy>(consignmentWriter.DataExportStrategy);
			});
		}

		public void TestGetConsignmentWriter_UseHVLVConsignmentToCargoReportWithAdditionalReferencesDataExportStrategy_IsPublishingInternally_AndIncludeAdditionalReferenceCollectionOverrideTrue()
		{
			(var writerManager, var writer) = GetWriterManagerAndWriter(true, includeAdditionalReferenceCollectionOverride: true);

			AssertType<HVLVConsignmentDataObjectWriter>(writer);
			var consignmentWriter = writer as HVLVConsignmentDataObjectWriter;

			CombineAssertions("use HVLVConsignmentToCargoReportWithAdditionalReferencesDataExportStrategy to write consignment data when writerManager.IsPublishingInternally is true", () =>
			{
				Assert("Precondition : writerManager.IsPublishingInternally is true", writerManager.IsPublishingInternally);
				AssertType<HVLVConsignmentToCargoReportWithAdditionalReferencesDataExportStrategy>(consignmentWriter.DataExportStrategy);
			});
		}

		(DataWritingManager writerManager, DataObjectWriter<HVLVConsignment, UniversalShipment> writer) GetWriterManagerAndWriter(bool isPublishingInternally, bool includeAdditionalReferenceCollectionOverride = false)
		{
			var shipmentBO = Factory.NewWithValidTestData<ForwardingShipment>();

			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var shipmentInNewFactory = newFactory.Load<ForwardingShipment>(shipmentBO.PK);
			var universalShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };

			var writerManager = new DataWritingManager(new ActionInfo(RecipientRoleType.DCA, shipmentInNewFactory));

			if (isPublishingInternally)
			{
				writerManager.SetIsPublishingInternally();
			}

			var writer = new HVLVShipmentWritingHelperForTest(writerManager, shipmentInNewFactory, universalShipment, includeAdditionalReferenceCollectionOverride).ConsignmentDataObjectWriterForTest;

			return (writerManager, writer);
		}

		public class HVLVShipmentWritingHelperForTest : HVLShipmentWritingHelper
		{
			public HVLVShipmentWritingHelperForTest(IDataWritingManager writeManager, ForwardingShipment parentShipmentBO, UniversalShipment parentShipmentData, bool includeAdditionalReferenceCollectionOverride)
				: base(writeManager, parentShipmentBO, parentShipmentData, includeAdditionalReferenceCollectionOverride)
			{
			}

			public DataObjectWriter<HVLVConsignment, UniversalShipment> ConsignmentDataObjectWriterForTest => ConsignmentDataObjectWriter;
		}

		#endregion
	}
}

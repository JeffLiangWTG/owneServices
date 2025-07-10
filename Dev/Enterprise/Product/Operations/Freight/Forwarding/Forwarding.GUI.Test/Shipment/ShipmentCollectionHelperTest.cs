using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ShipmentCollectionHelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCreateInstance()
		{
			ShipmentCollectionHelper helper = new ShipmentCollectionHelper(null);
			helper = new ShipmentCollectionHelper(new ConsolCollection(Factory.New<ForwardingShipment>()));
			helper = new ShipmentCollectionHelper(new ShipmentCollection(Factory));
		}

		public void TestExisingShipmentsFiresEvent()
		{
			Env.Security.MaintainShipmentAllowDetachingSubShipments.IsAllowed = false;

			try
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.CoLoadShipments.AddNew();

				ShipmentCollectionHelper helper = new ShipmentCollectionHelper(shipment.CoLoadShipments);
				shipment.CoLoadShipments[0].JS_JS_ColoadMasterShipmentForBinding = ZGuid.Empty;

				AssertEquals(Env.Security.GetErrorMessageForNotAllowed(Env.Security.MaintainShipmentAllowDetachingSubShipments),
					UnitTestUserNotification.Instance.LastMessage.Text);
			}
			finally
			{
				Env.Security.MaintainShipmentAllowDetachingSubShipments.IsAllowed = true;
			}
		}

		public void TestAddedShipmentsFiresEvent()
		{
			Env.Security.MaintainShipmentAllowDetachingSubShipments.IsAllowed = false;

			try
			{
				var shipment = Factory.New<ForwardingShipment>();

				ShipmentCollectionHelper helper = new ShipmentCollectionHelper(shipment.CoLoadShipments);

				shipment.CoLoadShipments.AddNew();
				shipment.CoLoadShipments[0].JS_JS_ColoadMasterShipmentForBinding = ZGuid.Empty;

				AssertEquals(Env.Security.GetErrorMessageForNotAllowed(Env.Security.MaintainShipmentAllowDetachingSubShipments),
					UnitTestUserNotification.Instance.LastMessage.Text);
			}
			finally
			{
				Env.Security.MaintainShipmentAllowDetachingSubShipments.IsAllowed = true;
			}
		}

		public void TestRemovedShipmentsDoesNotFireEvent()
		{
			Env.Security.MaintainShipmentAllowDetachingSubShipments.IsAllowed = false;

			try
			{
				var shipment = Factory.New<ForwardingShipment>();
				var coLoadShipment = shipment.CoLoadShipments.AddNew();

				ShipmentCollectionHelper helper = new ShipmentCollectionHelper(shipment.CoLoadShipments);
				coLoadShipment.JS_JS_ColoadMasterShipmentForBinding = ZGuid.Empty;

				AssertEquals(Env.Security.GetErrorMessageForNotAllowed(Env.Security.MaintainShipmentAllowDetachingSubShipments),
					UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				shipment.CoLoadShipments.Remove(coLoadShipment);

				coLoadShipment.JS_JS_ColoadMasterShipmentForBinding = ZGuid.Empty;

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
			finally
			{
				Env.Security.MaintainShipmentAllowDetachingSubShipments.IsAllowed = true;
			}
		}

		public void TestRemovedShipmentsUnlockJobHeader()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var coLoadShipment = shipment.CoLoadShipments.AddNew();
			ShipmentCollectionHelper helper = new ShipmentCollectionHelper(shipment.CoLoadShipments);
			AssertNull("Prereq: ShipmentJobHeader", coLoadShipment.ShipmentJobHeader);

			coLoadShipment.CreateShipmentJobHeaderWithMutex();
			var job = coLoadShipment.Job;
			var mutex = JobHeader.GetMutex_ForTestOnly(coLoadShipment.PK);
			AssertNotNull("Job Header Mutex", mutex);

			AssertEquals(true, mutex.IsLocked);
			AssertEquals(false, job.IsDeleted);

			shipment.CoLoadShipments.Remove(coLoadShipment);

			AssertEquals(false, mutex.IsLocked);
			AssertEquals(true, job.IsDeleted);
		}

		public void TestRemovedShipmentsUnlockJobHeader_SkipUnlockWhenShipmentRelatedToCost()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipmentMaster = consol.Shipments.AddNew();
			var shipmentSub = Factory.New<ForwardingShipment>();
			shipmentSub.JS_JS_ColoadMasterShipment = shipmentMaster.PK;
			shipmentSub.JS_HouseBill = "HB123";
			consol.ShowSubHouseBillShipments = true;

			var helper = new ShipmentCollectionHelper(consol.GridShipments, consol);
			AssertNull(shipmentSub.ShipmentJobHeader);

			shipmentSub.CreateShipmentJobHeaderWithMutex();
			AssertNotNull(shipmentSub.ShipmentJobHeader);
			AssertEquals(false, shipmentSub.JobHeader.IsDisposed);

			Factory.Save();

			var apps = consol.GetApportionments();
			var cost = apps.CostsCollection.TryAddNew();
			cost.E6_ApportionToRelatedShipments = true;
			var charge = cost.ApportionmentCharges.Find(x => x.JR_HouseBill == shipmentSub.JS_HouseBill).Single();
			charge.JR_IsUsedForApportionment = true;

			consol.ShowSubHouseBillShipments = false;
			AssertNotNull(shipmentSub.ShipmentJobHeader);
			AssertEquals(false, shipmentSub.JobHeader.IsDisposed);

			apps.ReleaseMutexes();
		}

		public void TestMasterChanged()
		{
			var oldMaster = Factory.New<ForwardingShipment>();
			var subShipment = oldMaster.CoLoadShipments.AddNew();

			var newMaster = Factory.New<ForwardingShipment>();
			var consol = Factory.New<ForwardingConsol>();
			consol.Shipments.AddRange(oldMaster, newMaster);

			Factory.Save();

			var helper = new Mock<IShipmentVsConsolGUIMessageHelper>(MockBehavior.Strict);

			using (ShipmentVsConsolGUIMessageHelper.OverrideHelperInstance(helper.Object))
			{
				helper
					.Setup(m => m.OnShipmentMasterChanged(
						FreightShipmentVsConsolMessageHelper.Instance,
						subShipment,
						consol,
						It.Is<MasterChangedEventArgs>(x =>
							x.OldMasterPK == oldMaster.PK && x.NewMasterPK == newMaster.PK)))
					.Callback(() => Assert(true));

				var collectionHelper = new ShipmentCollectionHelper(consol.Shipments, consol);
				subShipment.JS_JS_ColoadMasterShipment = newMaster.PK;
			}
		}

		#region ExportBroker/ImportBroker

		public void TestAddedShipmentsFiresEvent_ImportBroker()
		{
			Env.Security.MaintainShipmentAllowDetachingSubShipments.IsAllowed = true;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var shipment = Factory.New<ForwardingShipment>();
			var helper = new ShipmentCollectionHelper(shipment.CoLoadShipments);
			shipment.JS_RL_NKOrigin = "USCHI";
			shipment.JS_RL_NKDestination = "AUSYD";

			var coloadShipment = shipment.CoLoadShipments.AddNew();
			coloadShipment.JS_RL_NKOrigin = "USCHI";
			coloadShipment.JS_RL_NKDestination = "AUSYD";

			Factory.Save();

			var testPKs = FreightTestHelper.CreateImportOrgMiscServInDB();

			shipment.CoLoadShipments[0].ConsigneePK = testPKs.OrgHeader;
			AssertEquals("Consignee has been changed. Do you wish to update the Import Broker?", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestRemovedShipmentsDoesNotFireEvent_ImportBroker()
		{
			Env.Security.MaintainShipmentAllowDetachingSubShipments.IsAllowed = true;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "USCHI";
			shipment.JS_RL_NKDestination = "AUSYD";

			var coLoadShipment = shipment.CoLoadShipments.AddNew();
			coLoadShipment.JS_RL_NKOrigin = "USCHI";
			coLoadShipment.JS_RL_NKDestination = "AUSYD";

			Factory.Save();

			var helper = new ShipmentCollectionHelper(shipment.CoLoadShipments);
			var testPKs = FreightTestHelper.CreateImportOrgMiscServInDB();

			coLoadShipment.ConsigneePK = testPKs.OrgHeader;
			AssertEquals("Consignee has been changed. Do you wish to update the Import Broker?", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			shipment.CoLoadShipments.Remove(coLoadShipment);

			coLoadShipment.ConsigneePK = ZGuid.Empty;
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
		}

		public void TestAddedShipmentsFiresEvent_ExportBroker()
		{
			Env.Security.MaintainShipmentAllowDetachingSubShipments.IsAllowed = true;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var shipment = Factory.New<ForwardingShipment>();
			var helper = new ShipmentCollectionHelper(shipment.CoLoadShipments);
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USCHI";

			var coloadShipment = shipment.CoLoadShipments.AddNew();
			coloadShipment.JS_RL_NKOrigin = "AUSYD";
			coloadShipment.JS_RL_NKDestination = "USCHI";

			Factory.Save();

			var testPKs = FreightTestHelper.CreateExportOrgMiscServInDB();

			shipment.CoLoadShipments[0].ConsignorPK = testPKs.OrgHeader;
			AssertEquals("Consignor has been changed. Do you wish to update the Export Broker?", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestRemovedShipmentsDoesNotFireEvent_ExportBroker()
		{
			Env.Security.MaintainShipmentAllowDetachingSubShipments.IsAllowed = true;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USCHI";

			var coLoadShipment = shipment.CoLoadShipments.AddNew();
			coLoadShipment.JS_RL_NKOrigin = "AUSYD";
			coLoadShipment.JS_RL_NKDestination = "USCHI";

			Factory.Save();

			var helper = new ShipmentCollectionHelper(shipment.CoLoadShipments);
			var testPKs = FreightTestHelper.CreateExportOrgMiscServInDB();

			coLoadShipment.ConsignorPK = testPKs.OrgHeader;
			AssertEquals("Consignor has been changed. Do you wish to update the Export Broker?", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			shipment.CoLoadShipments.Remove(coLoadShipment);

			coLoadShipment.ConsignorPK = ZGuid.Empty;
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
		}

		#endregion

		#region GetReasonChangingSecurityInspectionStatusEventHandler

		public void TestGetReasonChangingSecurityInspectionStatusEventHandler()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var ra = GlbCompany.CurrentCompany.OrgProxy.MainAddress.KnownShipperDetails.AddNew();
			ra.OV_OH_OrgHeader = GlbCompany.CurrentCompany.OrgProxy.PK;
			ra.OV_RN_NKClientCountryRelation = "AU";
			ra.OV_EXApprovedOrMajorExporter = "RA";
			ra.OV_EXApprovalNumber = "48484";
			ra.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(5);

			GlbCompany.CurrentCompany.Factory.Save();

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var kc = consignor.MainAddress.KnownShipperDetails.AddNew();
			kc.OV_OH_OrgHeader = consignor.PK;
			kc.OV_RN_NKClientCountryRelation = "AU";
			kc.OV_EXApprovedOrMajorExporter = "KC";
			kc.OV_EXApprovalNumber = "22344";
			kc.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(10);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			_ = new ShipmentCollectionHelper(consol.Shipments);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			consol.Shipments.Add(shipment);
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USCHI";

			AssertEquals("APP", shipment.JS_InspectionTypeCode);

			Factory.Save();

			shipment.JS_InspectionTypeCode = "LFS";
			AssertEquals("Enter the reason for changing the Security Inspection Status.\r\nThe reason will be recorded on the SEC-Security Modified event for future reference.",
				UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			Factory.Save();

			consol.Shipments.Remove(shipment);

			shipment.JS_InspectionTypeCode = "PHS";
			Assert("No prompt as shipment has been removed from collection", UnitTestUserNotification.Instance.LastMessage.WasNone);
		}

		#endregion

		#region GetReasonChangingSecurityAdditionalInspectionStatusEventHandler

		public void TestGetReasonChangingSecurityAdditionalInspectionStatusEventHandler()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var ra = GlbCompany.CurrentCompany.OrgProxy.MainAddress.KnownShipperDetails.AddNew();
			ra.OV_OH_OrgHeader = GlbCompany.CurrentCompany.OrgProxy.PK;
			ra.OV_RN_NKClientCountryRelation = "AU";
			ra.OV_EXApprovedOrMajorExporter = "RA";
			ra.OV_EXApprovalNumber = "48484";
			ra.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(5);

			GlbCompany.CurrentCompany.Factory.Save();

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var kc = consignor.MainAddress.KnownShipperDetails.AddNew();
			kc.OV_OH_OrgHeader = consignor.PK;
			kc.OV_RN_NKClientCountryRelation = "AU";
			kc.OV_EXApprovedOrMajorExporter = "KC";
			kc.OV_EXApprovalNumber = "22344";
			kc.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(10);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			_ = new ShipmentCollectionHelper(consol.Shipments);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			consol.Shipments.Add(shipment);
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USCHI";

			AssertEquals("UNK", shipment.JS_AdditionalInspectionTypeCode);
			shipment.JS_AdditionalInspectionTypeCode = "XRY";
			Factory.Save();

			shipment.JS_AdditionalInspectionTypeCode = "EDS";
			AssertEquals("Enter the reason for changing the Security Additional Inspection Status.\r\nThe reason will be recorded on the SEC-Security Modified event for future reference.",
				UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			Factory.Save();

			consol.Shipments.Remove(shipment);

			shipment.JS_AdditionalInspectionTypeCode = "PHS";
			Assert("No prompt as shipment has been removed from collection", UnitTestUserNotification.Instance.LastMessage.WasNone);
		}

		#endregion
	}
}

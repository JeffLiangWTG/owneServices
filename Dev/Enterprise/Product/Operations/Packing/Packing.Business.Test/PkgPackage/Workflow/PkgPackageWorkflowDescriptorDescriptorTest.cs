using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PkgPackageWorkflowDescriptor))]
	class PkgPackageWorkflowDescriptorDescriptorTest : WorkflowDescriptorTestCase<PkgPackageWorkflowDescriptor>
	{
		#region TestAreTasksCompanySpecific

		protected override bool ExpectingTasksToBeCompanySpecific
		{
			get { return false; }
		}

		#endregion

		#region TestID

		public override void TestID()
		{
			AssertEquals(WorkflowDescriptors.PkgPackageWorkflowDecriptorCode, WorkflowDescriptor.Code);
		}

		#endregion

		#region TestDescription

		public override void TestDescription()
		{
			AssertEquals("Package", WorkflowDescriptor.Description);
		}

		#endregion

		#region TestGetWorkflowTriggerAction_ForNotificationEmailDelivery

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			DummyWithWorkflow.TypeDecider.TypeForLoadOverride = typeof(DummyBizOWithPackingAndTransportCompany);

			var dummy = Factory.New<DummyBizOWithPackingAndTransportCompany>();
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = dummy.PK;
			packageJob.KJ_ParentTableCode = "Z0";

			return new IWorkflowProvider[] { dummy };
		}

		public override void TestGetWorkflowTriggerActionForUniversalEventXML()
		{
			// This test does not work in base as DummyBizOWithPackingAndTransportCompany returned from GetParentsWithConfiguredOrganisationPartiesForTest is not a DummyWithWorkflow
			// and PkgPackages support line triggers only
			// Also this workflow descriptor is for PkgPackages so anytime GetParentsWithConfiguredOrganisationPartiesForTest() this descriptor is not being tested correctly
			var dummy = GetParentsWithConfiguredOrganisationPartiesForTest()[0] as DummyBizOWithPackingAndTransportCompany;
			var transportCo = Factory.New<OrgHeader>();
			var transportCoMode = transportCo.EDICommunicationsModes.AddNew();
			transportCoMode.EK_Module = WorkflowDescriptors.PkgPackageWorkflowDecriptorCode;
			transportCoMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
			transportCoMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			transportCoMode.EK_Destination = "ANYWHERE";

			dummy.TransportCompanyForTest = transportCo.MainAddress;

			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = dummy.PK;
			packageJob.KJ_ParentTableCode = "Z0";
			var package = packageJob.Packages.AddNew();

			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			trigger.P9_LineTriggerType = TriggerLineTypes.Codes.PkgPackage;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;

			AssertNotNull(
					"You must implement GetUniversalEventCollectionDataObjectWriter(IDataWritingManager) when you override SupportsWorkflowTriggerActionUniversalEventCollectionXML to 'true'.",
					new PkgPackageWorkflowDescriptor().GetUniversalEventDataObjectWriter(ObjectFactory.New<IDataWritingManager>(new ActionWrapper(action, package, null), null, null, null)));
		}

		protected override BusinessObject SetupLineTriggerIfRequired(IWorkflowProvider parent, ProcessTask trigger)
		{
			var dummy = (DummyBizOWithPackingAndTransportCompany)parent;
			var transportCo = Factory.New<OrgHeader>();
			var transportCoMode = transportCo.EDICommunicationsModes.AddNew();
			transportCoMode.EK_Module = WorkflowDescriptors.PkgPackageWorkflowDecriptorCode;
			transportCoMode.EK_FileFormat = "NTF";
			transportCoMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			transportCoMode.EK_Destination = "@notificationemail.cargowise.com";

			var bookingParty = Factory.New<OrgHeader>();
			var bookingPartyMode = bookingParty.EDICommunicationsModes.AddNew();
			bookingPartyMode.EK_Module = WorkflowDescriptors.PkgPackageWorkflowDecriptorCode;
			bookingPartyMode.EK_FileFormat = "NTF";
			bookingPartyMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			bookingPartyMode.EK_Destination = "@notificationemail.cargowise.com";

			dummy.TransportCompanyForTest = transportCo.MainAddress;
			dummy.BookingPartyForTest = bookingParty.MainAddress;

			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = dummy.PK;
			packageJob.KJ_ParentTableCode = "Z0";
			var package = packageJob.Packages.AddNew();

			trigger.P9_LineTriggerType = TriggerLineTypes.Codes.PkgPackage;
			return package;
		}

		#endregion

		#region TestGetMessageRecipientParty

		public void TestGetMessageRecipientParty()
		{
			DummyWithWorkflow.TypeDecider.TypeForLoadOverride = typeof(DummyWithPacking);
			var dummyWithPacking = Factory.New<DummyWithPacking>();
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = dummyWithPacking.PK;
			packageJob.KJ_ParentTableCode = "Z0";
			var package = packageJob.Packages.AddNew();

			var descriptor = new PkgPackageWorkflowDescriptor();
			AssertEquals("Since BizO is null must not return recipient parties.", 0, descriptor.GetMessageRecipientParty(null, MessageRecipientPartyTypeList.Codes.ArrivalCFS).Count());
			AssertEquals("Since BizO is null must not return recipient parties.", 0, descriptor.GetMessageRecipientParty(null, MessageRecipientPartyTypeList.Codes.DepartureCFS).Count());
			AssertEquals("Packing parent does not have a transport company therefore must not return recipient parties.", 0, descriptor.GetMessageRecipientParty(package, MessageRecipientPartyTypeList.Codes.ArrivalCFS).Count());
			AssertEquals("Packing parent does not have a transport company therefore must not return recipient parties.", 0, descriptor.GetMessageRecipientParty(package, MessageRecipientPartyTypeList.Codes.DepartureCFS).Count());
		}

		public void TestGetMessageRecipientParty_WithTransportComapnyAndBookingParty()
		{
			DummyWithWorkflow.TypeDecider.TypeForLoadOverride = typeof(DummyBizOWithPackingAndTransportCompany);
			var transportOrg = Factory.NewWithValidTestData<OrgHeader>();
			var bookingParty = Factory.NewWithValidTestData<OrgHeader>();
			var dummyWithTransportComapnyAndBookingParty = Factory.New<DummyBizOWithPackingAndTransportCompany>();
			dummyWithTransportComapnyAndBookingParty.TransportCompanyForTest = null;
			dummyWithTransportComapnyAndBookingParty.BookingPartyForTest = null;
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = dummyWithTransportComapnyAndBookingParty.PK;
			packageJob.KJ_ParentTableCode = "Z0";
			var package = packageJob.Packages.AddNew();

			var descriptor = new PkgPackageWorkflowDescriptor();
			AssertEquals(0, descriptor.GetMessageRecipientParty(package, MessageRecipientPartyTypeList.Codes.DeliveryCartage).Count());
			AssertEquals(0, descriptor.GetMessageRecipientParty(package, MessageRecipientPartyTypeList.Codes.PickupCartage).Count());
			AssertEquals(0, descriptor.GetMessageRecipientParty(null, MessageRecipientPartyTypeList.Codes.DeliveryCartage).Count());
			AssertEquals(0, descriptor.GetMessageRecipientParty(null, MessageRecipientPartyTypeList.Codes.PickupCartage).Count());

			dummyWithTransportComapnyAndBookingParty.TransportCompanyForTest = transportOrg.MainAddress;
			AssertEquals(transportOrg, descriptor.GetMessageRecipientParty(package, MessageRecipientPartyTypeList.Codes.DeliveryCartage).Single().Party);
			AssertEquals(transportOrg, descriptor.GetMessageRecipientParty(package, MessageRecipientPartyTypeList.Codes.PickupCartage).Single().Party);
			AssertEquals(0, descriptor.GetMessageRecipientParty(null, MessageRecipientPartyTypeList.Codes.DeliveryCartage).Count());
			AssertEquals(0, descriptor.GetMessageRecipientParty(null, MessageRecipientPartyTypeList.Codes.PickupCartage).Count());

			dummyWithTransportComapnyAndBookingParty.BookingPartyForTest = bookingParty.MainAddress;
			AssertEquals(bookingParty, descriptor.GetMessageRecipientParty(package, MessageRecipientPartyTypeList.Codes.BookingParty).Single().Party);
			AssertEquals(0, descriptor.GetMessageRecipientParty(null, MessageRecipientPartyTypeList.Codes.BookingParty).Count());
		}

		#endregion

		#region TestWorkflowProviderType

		public override void TestWorkflowProviderType()
		{
			AssertEquals(typeof(PkgPackage), WorkflowDescriptor.WorkflowProviderType);
		}

		#endregion

		#region TestRequiresBranch

		public override void TestRequiresBranch()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresBranch);
		}

		#endregion

		#region TestRequiresClient

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		#endregion

		#region TestRequiresDepartment

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		#endregion

		#region TestRequiresPorts

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		#endregion

		#region TestSupportsWorkflowTemplates

		public override void TestSupportsWorkflowTemplates()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsWorkflowTemplates);
		}

		public void TestSupportsApplyWorkflowTemplate()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsApplyWorkflowTemplate);
		}

		#endregion

		#region TestSubTypes

		public override void TestSubTypes()
		{
			AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		#endregion

		#region  TestSupportsBufferManagement

		public void TestSupportsBufferManagement()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsBufferManagement);
		}

		#endregion

		#region TestSupportsEventTracking

		public override void TestSupportsEventTracking()
		{
			AssertEquals("Should be true because we want to send universal events from package line level triggers.", true, WorkflowDescriptor.SupportsEventTracking);
		}

		#endregion

		#region TestSupportedMessageRecipientParties

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get { return MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.DeliveryCartage | MessageRecipientPartyType.PickupCartage | MessageRecipientPartyType.BookingParty; }
		}

		#endregion
	}
}

using CargoWise.Definitions;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	[TestedType(typeof(CartageLegWorkflowDescriptor))]
	class CartageLegWorkflowDescriptorTest : WorkflowDescriptorTestCase<CartageLegWorkflowDescriptor>
	{
		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return MessageRecipientPartyType.Consignor | MessageRecipientPartyType.Consignee | MessageRecipientPartyType.BillToParty | MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Email | MessageRecipientPartyType.BookingParty;
			}
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var tbBookingParty = Factory.New<OrgHeader>();
			var localClient = Factory.New<OrgHeader>();
			var consignee = Factory.New<OrgHeader>();
			var consignor = Factory.New<OrgHeader>();
			SetupOrgCommunication(tbBookingParty);
			SetupOrgCommunication(localClient);
			SetupOrgCommunication(consignee);
			SetupOrgCommunication(consignor);
			var parentBooking = Factory.New<IDtbBooking>();
			var parentBookingConsolidation = Factory.New<IDtbBookingConsolidation>();
			parentBooking.KM_KB_Booking = parentBookingConsolidation.PK;
			var iParentBookingConsolidation = (IDocAddresses)parentBookingConsolidation;
			iParentBookingConsolidation.DocAddresses.AddNew(tbBookingParty.MainAddress, DocAddressType.BookingPartyDocumentaryAddress);
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ParentTableCode = DtbBookingSchema.Constants.Prefix;
			cartage.JJ_ParentID = parentBooking.PK;
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLImport;
			new JobHeader.Loader(cartage).TryLoadOrCreate();
			cartage.LocalClientPK = localClient.PK;
			var cartageLeg = cartage.LooseBookedMoves.AddNew().CartageLegs.AddNew();
			var consigneeAddr = cartage.DocAddresses.FindDocAddressesByType(DocAddressType.LocalCartageImporter);
			consigneeAddr[0].OrganisationPK = consignee.PK;
			cartageLeg.JU_E2DeliveryAddressID = consignee.MainAddress.PK;
			var consignorAddr = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageExporter);
			consignorAddr.OrganisationPK = consignor.PK;
			cartageLeg.JU_E2PickupAddressID = consignor.MainAddress.PK;
			return new IWorkflowProvider[] { cartageLeg };
		}

		void SetupOrgCommunication(OrgHeader org)
		{
			var orgMode = org.EDICommunicationsModes.AddNew();
			orgMode.EK_Module = "LTL";
			orgMode.EK_FileFormat = "NTF";
			orgMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			orgMode.EK_Destination = "@notificationemail.cargowise.com";
		}

		public override void TestID()
		{
			AssertEquals("Correct Code", WorkflowDescriptors.CartageLegWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "Port Transport Leg", WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.ContainerLeg, WorkflowDescriptor.DocumentBusinessContext[0]);
		}

		protected override bool ExpectingTasksToBeCompanySpecific
		{
			get
			{
				return false;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
		}
	}
}

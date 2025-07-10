using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public abstract class TransportProcessTaskProviderTestBase<TCartage, TWorkflowDescriptor> : WorkflowDescriptorTestCase<TWorkflowDescriptor> where TCartage : CommonCartage where TWorkflowDescriptor : TransportWorkflowDescriptor, new()
	{
		public override void TestID()
		{
			AssertEquals("Correct Code", JobInvoicingConsumerTypes.LocalCartage.Code, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", JobInvoicingConsumerTypes.LocalCartage.Description, WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			AssertEquals("1 sub type", 1, WorkflowDescriptor.SubTypeInformation.Length);
			AssertEquals("Sub Type 1 is Transport Job Type", "Job Type", WorkflowDescriptor.SubTypeInformation[0].Description);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[0].List);
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

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			var consigneeAddr = cartage.DocAddresses.FindDocAddressesByType(DocAddressType.LocalCartageImporter);
			consigneeAddr[0].OrganisationPK = GetNewConfiguredOrgHeader().PK;
			new JobHeader.Loader(cartage).TryLoadOrCreate();
			cartage.LocalClientAddressPK = GetNewConfiguredOrgHeader().MainAddress.PK;
			var tbBookingParty = GetNewConfiguredOrgHeader();
			var parentBooking = Factory.New<IDtbBooking>();
			var parentBookingConsolidation = Factory.New<IDtbBookingConsolidation>();
			parentBooking.KM_KB_Booking = parentBookingConsolidation.PK;
			var iParentBookingConsolidation = (IDocAddresses)parentBookingConsolidation;
			iParentBookingConsolidation.DocAddresses.AddNew(tbBookingParty.MainAddress, DocAddressType.BookingPartyDocumentaryAddress);
			cartage.JJ_ParentTableCode = DtbBookingSchema.Constants.Prefix;
			cartage.JJ_ParentID = parentBooking.PK;
			return new IWorkflowProvider[] { cartage };
		}

		OrgHeader GetNewConfiguredOrgHeader()
		{
			var mode = Factory.New<EDICommunicationsMode>();
			mode.EK_Module = "TRN";
			mode.EK_FileFormat = "NTF";
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			mode.EK_Destination = "@notificationemail.cargowise.com";
			var org = Factory.New<OrgHeader>();
			org.EDICommunicationsModes.Add(mode);
			return org;
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return MessageRecipientPartyType.Consignee | MessageRecipientPartyType.BillToParty | MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Email | MessageRecipientPartyType.BookingParty;
			}
		}

		protected override bool ExpectingTasksToBeCompanySpecific
		{
			get
			{
				return false;
			}
		}

		public void TestGetFormCustomisationSettingsProvider()
		{
			AssertEquals(typeof(LocalTransportFormCustomisationSettingsProvider), WorkflowDescriptor.FormCustomisationSettings.GetType());
		}
	}
}

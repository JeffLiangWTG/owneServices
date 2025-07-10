using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(JobSupplierBookingWorkflowDescriptor))]
	public class JobSupplierBookingWorkflowDescriptorTest : WorkflowDescriptorTestCase<JobSupplierBookingWorkflowDescriptor>
	{
		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "Supplier Booking", WorkflowDescriptor.Description);
		}

		public override void TestID()
		{
			AssertEquals("Correct Code", WorkflowDescriptors.JobSupplierBookingWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public void TestValidationToolSettings()
		{
			AssertType<JobSupplierBookingValidationToolSettings>(WorkflowDescriptor.ValidationToolSettings);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresPort1);
			AssertEquals(true, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestSupportsScreenLayout()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsScreenLayout);
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[]
			{
				SupplierBookingWithOrganisationParties,
			};
		}

		protected JobSupplierBooking SupplierBookingWithOrganisationParties
		{
			get { return supplierBookingWithOrganisationParties ?? (supplierBookingWithOrganisationParties = GetSupplierBookingWithOrgInfo()); }
		}
		JobSupplierBooking supplierBookingWithOrganisationParties;

		JobSupplierBooking GetSupplierBookingWithOrgInfo()
		{
			var supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
			supplierBooking.JSB_OH_BookingParty = BookingPartyOrg.PK;
			supplierBooking.ControllingCustomerAddress.OrganisationPK = ControllingCustomerOrg.PK;
			supplierBooking.SupplierAddress.OrganisationPK = GetOrgWithCommunicationModes("Supplier").PK;

			return supplierBooking;
		}

		public override void TestSubTypes()
		{
			AssertEquals("1 sub types", 1, WorkflowDescriptor.SubTypeInformation.Length);
			AssertEquals("Sub Type 1 is Transport Mode", "Transport Mode", WorkflowDescriptor.SubTypeInformation[0].Description);
		}

		public override void TestSupportsWorkflowTemplates()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsWorkflowTemplates);
		}

		#region TestSupportedMessageRecipientParties

		protected override CodeDescriptionPair[] ExpectedAdditionalWorkflowTriggerActionTypes
		{
			get
			{
				return new CodeDescriptionPair[] {
					new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SetField, WorkflowTriggerActionTypeConstants.Descriptions.SetField) };
			}
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return
					MessageRecipientPartyType.BookingParty |
					MessageRecipientPartyType.ControllingCustomer |
					MessageRecipientPartyType.Consignor |
					MessageRecipientPartyType.Email;
			}
		}

		#endregion
	}
}

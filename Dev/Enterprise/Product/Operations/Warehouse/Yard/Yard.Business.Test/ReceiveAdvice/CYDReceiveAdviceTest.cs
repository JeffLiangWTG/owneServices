using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDReceiveAdvice))]
	public class CYDReceiveAdviceTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<CYDReceiveAdvice>();
		}

		public void TestNoteTypes()
		{
			var receiveAdvice = Factory.New<CYDReceiveAdvice>();
			AssertContainsExactElementsInAnyOrder(new PredefinedNoteType[] {
					PredefinedNoteTypes.Instance.HandlingInstructions,
					PredefinedNoteTypes.Instance.DeliveryInstructionsNote },
				receiveAdvice.NoteTypes);
		}

		#region eDocs Provider

		public void TestGetEDocsProviderSupport()
		{
			IEDocsProvider businessObj = (IEDocsProvider)GetNewBusinessObject();
			AssertEquals("GetEDocsProviderSupporter().GetType()", typeof(JobInvoicingEDocsProviderSupporter), businessObj.GetEDocsProviderSupporter().GetType());
		}

		#endregion

		#region TestIDocManagerSupport

		public void TestIDocManagerSupport()
		{
			var header = Factory.New<CYDReceiveAdvice>();
			AssertEquals(Constants.DocManagerCodes.CYDReceiveAdvice, ((IDocManagerSupport)header).DocManagerInfo.DocManagerCode);
		}

		#endregion

		public void DocAddressesIsEmpty()
		{
			var receiveAdvice = Factory.New<CYDReceiveAdvice>();
			var docAddresses = receiveAdvice.DocAddresses;
			AssertEquals(0, docAddresses.Count);
		}

		public void TestSupportedAddressTypes()
		{
			var docAdresses = (IDocAddresses)Factory.New<CYDReceiveAdvice>();
			var supportedAddressTypes = docAdresses.SupportedAddressTypes;
			var expectedSupportedAddressTypes = new[]
				{
					DocAddressType.ShippingLineAddress,
					DocAddressType.ConsigneeAddress,
					DocAddressType.ConsignorDocumentaryAddress,
					DocAddressType.TransportCompanyDocumentaryAddress,
					DocAddressType.Forwarder,
					DocAddressType.ControllingCustomer
				};
			AssertSequencesEqual(expectedSupportedAddressTypes, supportedAddressTypes);
		}

		#region Client

		public void TestClientIsPopulatedCorrectly()
		{
			var receiveAdvice = (CYDReceiveAdvice)GetNewBusinessObject();
			AssertEquals(DocAddressType.BookingPartyDocumentaryAddress, receiveAdvice.Client.DocAddressType);
			AssertEquals(ContactType.LocalClient, receiveAdvice.Client.DefaultContactType);
			AssertEquals(receiveAdvice, receiveAdvice.Client.Parent);
		}

		#endregion

		#region Lessee

		public void TestLesseeIsPopulatedCorrectly()
		{
			var receiveAdvice = (CYDReceiveAdvice)GetNewBusinessObject();
			AssertEquals(DocAddressType.ControllingCustomer, receiveAdvice.Lessee.DocAddressType);
			AssertEquals(ContactType.LocalClient, receiveAdvice.Lessee.DefaultContactType);
			AssertEquals(receiveAdvice, receiveAdvice.Lessee.Parent);
		}

		#endregion
	}

	#region WorkflowProviderTests

	[TestedType(typeof(CYDReceiveAdvice))]
	public class CYDReceiveAdviceWorkflowProviderTest : WorkflowProviderTest<CYDReceiveAdvice, CYDReceiveAdviceProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.CYDReceiveAdviceWorkflowDescriptorCode;

		public void TestTemplateIsAppliedToCorrectClients()
		{
			var clientWithTemplate = Factory.NewWithValidTestData<OrgHeader>();
			var clientWithoutTemplate = Factory.NewWithValidTestData<OrgHeader>();

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = ExpectedWorkflowType;
			template.P0_OH_Client = clientWithTemplate.PK;

			var milestone = template.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = "XXX";

			Factory.Save();

			var receiveAdvice1 = Factory.NewWithValidTestData<CYDReceiveAdvice>();
			var jobDocAddress1 = receiveAdvice1.DocAddresses.AddNew();
			jobDocAddress1.OrganisationPK = clientWithTemplate.PK;
			jobDocAddress1.E2_AddressType = AutoDocAddressTypes.Codes.BookingPartyDocumentaryAddress;

			var receiveAdvice2 = Factory.NewWithValidTestData<CYDReceiveAdvice>();
			var jobDocAddress2 = receiveAdvice2.DocAddresses.AddNew();
			jobDocAddress2.OrganisationPK = clientWithoutTemplate.PK;
			jobDocAddress2.E2_AddressType = AutoDocAddressTypes.Codes.BookingPartyDocumentaryAddress;

			Factory.Save();

			AssertEquals("Template tasks created when client matches", 1, receiveAdvice1.WorkflowItems.Milestones.Count);
			AssertEquals("Template tasks NOT created when client doesn't match", 0, receiveAdvice2.WorkflowItems.Milestones.Count);
		}
	}

	#endregion
}

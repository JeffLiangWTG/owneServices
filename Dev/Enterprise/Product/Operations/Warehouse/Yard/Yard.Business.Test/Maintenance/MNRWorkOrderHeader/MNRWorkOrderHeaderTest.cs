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
	[TestedType(typeof(MNRWorkOrderHeader))]
	class MNRWorkOrderHeaderTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<MNRWorkOrderHeader>();
		}

		#region WorkflowProviderTests

		[TestedType(typeof(MNRWorkOrderHeader))]
		public class MNRWorkOrderHeaderWorkflowProviderTest : WorkflowProviderTest<MNRWorkOrderHeader, MNRWorkOrderHeaderProcessTaskCollection>
		{
			protected override ZString ExpectedWorkflowType => WorkflowDescriptors.MNRWorkOrderHeaderWorkflowDescriptorCode;

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

				var mnrWorkOrderHeader1 = Factory.NewWithValidTestData<MNRWorkOrderHeader>();
				var jobDocAddress1 = mnrWorkOrderHeader1.DocAddresses.AddNew();
				jobDocAddress1.OrganisationPK = clientWithTemplate.PK;
				jobDocAddress1.E2_AddressType = AutoDocAddressTypes.Codes.BookingPartyDocumentaryAddress;

				var mnrWorkOrderHeader2 = Factory.NewWithValidTestData<MNRWorkOrderHeader>();
				var jobDocAddress2 = mnrWorkOrderHeader2.DocAddresses.AddNew();
				jobDocAddress2.OrganisationPK = clientWithoutTemplate.PK;
				jobDocAddress2.E2_AddressType = AutoDocAddressTypes.Codes.BookingPartyDocumentaryAddress;

				Factory.Save();

				AssertEquals("Template tasks created when client matches", 1, mnrWorkOrderHeader1.WorkflowItems.Milestones.Count);
				AssertEquals("Template tasks NOT created when client doesn't match", 0, mnrWorkOrderHeader2.WorkflowItems.Milestones.Count);
			}
		}

		#endregion

		#region TestNoteTypes

		public void TestNoteTypes()
		{
			var workOrderHeader = Factory.New<MNRWorkOrderHeader>();
			AssertContainsExactElementsInAnyOrder(new PredefinedNoteType[] {
					PredefinedNoteTypes.Instance.ContainerComment,
					PredefinedNoteTypes.Instance.SurveyInstruction },
				workOrderHeader.NoteTypes);
		}

		#endregion

		#region TestDocAddresses

		public void DocAddressesIsEmpty()
		{
			var workOrder = Factory.New<MNRWorkOrderHeader>();
			var docAddresses = workOrder.DocAddresses;
			AssertEquals(0, docAddresses.Count);
		}

		public void TestSupportedAddressTypes()
		{
			var docAdresses = (IDocAddresses)Factory.New<MNRWorkOrderHeader>();
			var supportedAddressTypes = docAdresses.SupportedAddressTypes;
			var expectedSupportedAddressTypes = new[]
				{
					DocAddressType.ShippingLineAddress,
					DocAddressType.ConsigneeAddress,
					DocAddressType.ConsignorDocumentaryAddress,
					DocAddressType.TransportCompanyDocumentaryAddress,
					DocAddressType.Forwarder
				};
			AssertSequencesEqual(expectedSupportedAddressTypes, supportedAddressTypes);
		}

		#endregion

		#region TestIEDocsProvider

		public void TestGetEDocsProviderSupport()
		{
			var businessObj = (IEDocsProvider)GetNewBusinessObject();
			AssertEquals("GetEDocsProviderSupporter().GetType()", typeof(JobInvoicingEDocsProviderSupporter), businessObj.GetEDocsProviderSupporter().GetType());
		}

		#endregion

		#region TestIDocManagerSupport

		public void TestIDocManagerSupport()
		{
			var header = Factory.New<MNRWorkOrderHeader>();
			AssertEquals(Constants.DocManagerCodes.MNRWorkOrderHeader, ((IDocManagerSupport)header).DocManagerInfo.DocManagerCode);
		}

		#endregion
	}
}

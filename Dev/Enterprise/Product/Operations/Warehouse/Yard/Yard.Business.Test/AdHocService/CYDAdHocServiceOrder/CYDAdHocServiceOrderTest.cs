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
	[TestedType(typeof(CYDAdHocServiceOrder))]
	class CYDAdHocServiceOrderTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<CYDAdHocServiceOrder>();
		}

		public void TestNoteTypes()
		{
			var adHocServiceOrder = Factory.New<CYDAdHocServiceOrder>();

			AssertContainsExactElementsInAnyOrder(new PredefinedNoteType[] {
				PredefinedNoteTypes.Instance.BookingNotes },
				adHocServiceOrder.NoteTypes);
		}

		#region WorkflowProviderTests

		[TestedType(typeof(CYDAdHocServiceOrder))]
		public class CYDAdHocServiceOrderWorkflowProviderTest : WorkflowProviderTest<CYDAdHocServiceOrder, CYDAdHocServiceOrderProcessTaskCollection>
		{
			protected override ZString ExpectedWorkflowType => WorkflowDescriptors.CYDAdHocServiceOrderWorkflowDescriptorCode;

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

				var serviceOrder1 = Factory.NewWithValidTestData<CYDAdHocServiceOrder>();
				var jobDocAddress1 = Factory.NewWithValidTestData<JobDocAddress>();
				jobDocAddress1.E2_ParentID = serviceOrder1.PK;
				jobDocAddress1.OrganisationPK = clientWithTemplate.PK;
				jobDocAddress1.E2_AddressType = AutoDocAddressTypes.Codes.BookingPartyDocumentaryAddress;

				var serviceOrder2 = Factory.NewWithValidTestData<CYDAdHocServiceOrder>();
				var jobDocAddress2 = Factory.NewWithValidTestData<JobDocAddress>();
				jobDocAddress2.E2_ParentID = serviceOrder2.PK;
				jobDocAddress2.OrganisationPK = clientWithoutTemplate.PK;
				jobDocAddress2.E2_AddressType = AutoDocAddressTypes.Codes.BookingPartyDocumentaryAddress;

				Factory.Save();

				AssertEquals("Template tasks created when client matches", 1, serviceOrder1.WorkflowItems.Milestones.Count);
				AssertEquals("Template tasks NOT created when client doesn't match", 0, serviceOrder2.WorkflowItems.Milestones.Count);
			}
		}

		#endregion

		#region TestIDocManagerSupport

		public void TestIDocManagerSupport()
		{
			var adHocServiceOrder = Factory.New<CYDAdHocServiceOrder>();
			AssertEquals(Constants.DocManagerCodes.CYDAdHocServiceOrder, ((IDocManagerSupport)adHocServiceOrder).DocManagerInfo.DocManagerCode);
		}

		#endregion

		#region eDocs Provider

		public void TestGetEDocsProviderSupport()
		{
			IEDocsProvider businessObj = (IEDocsProvider)GetNewBusinessObject();
			AssertEquals("GetEDocsProviderSupporter().GetType()", typeof(JobInvoicingEDocsProviderSupporter), businessObj.GetEDocsProviderSupporter().GetType());
		}

		#endregion
	}
}

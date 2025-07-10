using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDPickupHeader))]
	public class CYDPickupHeaderTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<CYDPickupHeader>();
		}

		#region Test IDocAddress

		public void TestHumanReadableName()
		{
			var pickupHeader = (IDocAddresses)Factory.New<CYDPickupHeader>();
			AssertEquals("", pickupHeader.HumanReadableName);
		}

		public void DocAddressesIsEmpty()
		{
			var pickupHeader = Factory.New<CYDPickupHeader>();
			var docAddresses = pickupHeader.DocAddresses;
			AssertEquals(0, docAddresses.Count);
		}

		public void TestSupportedAddressTypes()
		{
			var pickupHeader = (IDocAddresses)Factory.New<CYDPickupHeader>();
			AssertSequencesEqual(new DocAddressType[] { DocAddressType.TransportCompanyDocumentaryAddress }, pickupHeader.SupportedAddressTypes);
		}

		public void TestGetCanOverrideCheckpoint()
		{
			var pickupHeader = (IDocAddresses)Factory.New<CYDPickupHeader>();
			var jobDocAddress = Factory.New<JobDocAddress>();
			AssertEquals(Env.Security.None, pickupHeader.GetCanOverrideCheckpoint(jobDocAddress));
		}

		public void TestCanDeleteAddress()
		{
			var pickupHeader = (IDocAddresses)Factory.New<CYDPickupHeader>();
			var jobDocAddress = Factory.New<JobDocAddress>();
			AssertEquals(true, pickupHeader.CanDeleteAddress(jobDocAddress));
		}

		public void TestGetDocAddressRequirement()
		{
			var pickupHeader = (IDocAddresses)Factory.New<CYDPickupHeader>();
			AssertNull("GetDocAddressRequirement", pickupHeader.GetDocAddressRequirement(DocAddressType.TransportCompanyDocumentaryAddress));
		}

		public void TestGetOrgHeaderList()
		{
			var pickupHeader = (IDocAddresses)Factory.New<CYDPickupHeader>();
			AssertNull("GetOrgHeaderList", pickupHeader.GetOrgHeaderList(DocAddressType.TransportCompanyDocumentaryAddress));
		}

		public void TestPiggyBackedDocAddressValidation()
		{
			var pickupHeader = (IDocAddresses)Factory.New<CYDPickupHeader>();
			var jobDocAddress = Factory.New<JobDocAddress>();
			AssertNull("PiggyBackedDocAddressValidation", pickupHeader.PiggyBackedDocAddressValidation(jobDocAddress));
		}

		#endregion

		public void TestNoteTypes()
		{
			var pickupHeader = Factory.New<CYDPickupHeader>();
			var expectedNoteTypes = new PredefinedNoteType[]
			{
				PredefinedNoteTypes.Instance.HandlingInstructions,
				PredefinedNoteTypes.Instance.PickupInstructionsNote
			};

			AssertContainsExactElementsInAnyOrder(expectedNoteTypes, pickupHeader.NoteTypes);
		}

		#region TestYard

		public void TestYard()
		{
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			var cydPickupHeader = (CYDPickupHeader)GetNewBusinessObject();
			cydPickupHeader.YPH_WW_Yard = warehouse.PK;
			AssertEquals(warehouse, cydPickupHeader.Yard);
		}

		#endregion

		#region eDocs Provider

		public void TestGetEDocsProviderSupport()
		{
			var businessObj = (IEDocsProvider)GetNewBusinessObject();
			AssertEquals("GetEDocsProviderSupporter().GetType()", typeof(JobInvoicingEDocsProviderSupporter), businessObj.GetEDocsProviderSupporter().GetType());
		}

		#endregion

		#region TestIDocManagerSupport

		public void TestIDocManagerSupport()
		{
			var header = Factory.New<CYDPickupHeader>();
			AssertEquals(DocManagerCodes.CYDPickupHeader, ((IDocManagerSupport)header).DocManagerInfo.DocManagerCode);
		}

		#endregion

		#region TestIJobNumber

		public void TestIJobNumber()
		{
			var header = Factory.New<CYDPickupHeader>();
			header.YPH_JobNumber = "ThisIsValid";
			AssertEquals("ThisIsValid", ((IJobNumber)header).JobNumber);
		}

		#endregion
	}

	#region WorkflowProviderTests

	[TestedType(typeof(CYDPickupHeader))]
	public class CYDPickupHeaderWorkflowProviderTest : WorkflowProviderTest<CYDPickupHeader, CYDPickupHeaderProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.CYDPickupHeaderWorkflowDescriptorCode;

		public void TestTemplateIsAppliedToCorrectClients()
		{
			var yardWithTemplate = Factory.NewWithValidTestData<WhsWarehouse>();
			var yardWithoutTemplate = Factory.NewWithValidTestData<WhsWarehouse>();

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = ExpectedWorkflowType;
			template.P0_WW = yardWithTemplate.PK;

			var milestone = template.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = "XXX";

			Factory.Save();

			var matchingPickupHeader = Factory.NewWithValidTestData<CYDPickupHeader>();
			matchingPickupHeader.YPH_WW_Yard = yardWithTemplate.PK;

			var unMatchingPickupHeader = Factory.NewWithValidTestData<CYDPickupHeader>();
			unMatchingPickupHeader.YPH_WW_Yard = yardWithoutTemplate.PK;

			Factory.Save();

			AssertEquals("Template tasks created when yard matches", 1, matchingPickupHeader.WorkflowItems.Milestones.Count);
			AssertEquals("Template tasks NOT created when yard doesn't match", 0, unMatchingPickupHeader.WorkflowItems.Milestones.Count);
		}
	}

	#endregion
}

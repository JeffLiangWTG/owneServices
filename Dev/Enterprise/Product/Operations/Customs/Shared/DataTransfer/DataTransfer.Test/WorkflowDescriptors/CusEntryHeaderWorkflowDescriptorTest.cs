using CargoWise.EntityFramework;
using Enterprise.Customs.Common.Shared;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusEntryHeaderWorkflowDescriptor))]
	public class CusEntryHeaderWorkflowDescriptorTest : WorkflowDescriptorTestCase<CusEntryHeaderWorkflowDescriptor>
	{
		public override void TestDescription()
		{
			AssertEquals("Entry Header Line Trigger", WorkflowDescriptor.Description);
		}

		public override void TestID()
		{
			AssertEquals("Correct Code", "CEH", WorkflowDescriptor.Code);
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

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestSubTypes()
		{
			AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		public void TestSupportsBufferManagement()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsBufferManagement);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals("Should be true because we want to send universal events from line level triggers.", true, WorkflowDescriptor.SupportsEventTracking);
		}
		public override void TestSupportsWorkflowTemplates()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsWorkflowTemplates);
		}

		public void TestSupportsApplyWorkflowTemplate()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsApplyWorkflowTemplate);
		}

		public override void TestWorkflowProviderType()
		{
			AssertEquals(typeof(CusEntryHeader), WorkflowDescriptor.WorkflowProviderType);
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return
					MessageRecipientPartyType.Email;
			}
		}

		protected override bool ExpectingTasksToBeCompanySpecific => true;

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[]
			{
				GetCusEntryHeaderOnDeclarationWithOrganisations(true),
				GetCusEntryHeaderOnDeclarationWithOrganisations(false),
			};
		}

		public new void TestValidDocumentActionTypes()
		{
			var testProviders = GetParentsWithConfiguredOrganisationPartiesForTest();
			var trigger = testProviders[0].WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "DUMMY TASK";
			trigger.ReferenceCode = "REF";
			var line = (CusEntryHeader)SetupLineTriggerIfRequired(testProviders[0], trigger);

			AssertEquals(((IDocumentSupportable)line).DocumentSupporter.BusinessContext, WorkflowDescriptor.DocumentBusinessContext[0]);
		}

		protected override BusinessObject SetupLineTriggerIfRequired(IWorkflowProvider parent, ProcessTask trigger)
		{
			var declaration = (BaseJobDeclaration)parent;

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();

			cusEntryHeader.CH_CEI_Instruction = Factory.NewWithValidTestData<CusEntryInstruction>().PK;

			cusEntryHeader.CH_MessageType = declaration.JE_MessageType;

			trigger.P9_LineTriggerType = TriggerLineTypes.Codes.CusEntryHeader;
			return cusEntryHeader;
		}

		BaseJobDeclaration GetCusEntryHeaderOnDeclarationWithOrganisations(bool isImport)
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = isImport ? SharedJobMessageTypeList.Codes.Import : SharedJobMessageTypeList.Codes.Export;

			var jobLoader = new JobHeader.Loader(declaration);
			var job = jobLoader.TryCreate();
			job.JH_OA_LocalChargesAddr = BillToPartyOrg.MainAddress.PK;

			declaration.JE_OH_Importer = ConsigneeOrg.PK;
			declaration.JE_OH_Supplier = ConsignorOrg.PK;

			declaration.DocsAndCartage.PickupCartageCoPK = PickupCartageOrg.PK;
			declaration.DocsAndCartage.DeliveryCartageCoPK = DeliveryCartageOrg.PK;

			declaration.JE_OH_Forwarder = Forwarder.PK;

			declaration.JE_OH_ExternalBroker = ExternalBrokerOrg.PK;

			return declaration;
		}
	}
}

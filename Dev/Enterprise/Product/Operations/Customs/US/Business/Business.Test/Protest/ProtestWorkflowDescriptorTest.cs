using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Protest.Testing
{
	[TestedType(typeof(ProtestWorkflowDescriptor))]
	sealed class ProtestWorkflowDescriptorTest : WorkflowDescriptorTestCase<ProtestWorkflowDescriptor>
	{
		public void TestFieldColumnsCount()
		{
			AssertEquals(6, WorkflowDescriptor.GetWorkflowTriggerFieldColumns().Length);
		}

		public new void TestGetFieldColumnDescription()
		{
			AssertEquals("Period Base Date", WorkflowDescriptor.GetFieldColumnDescription(Factory, USAddInfoSchema.US_P_PeriodBaseDate));
			AssertEquals("Further Review?", WorkflowDescriptor.GetFieldColumnDescription(Factory, USAddInfoSchema.US_P_ApplicationFurtherReview));
			AssertEquals("Accelerated Disposition?", WorkflowDescriptor.GetFieldColumnDescription(Factory, USAddInfoSchema.US_P_AcceleratedDispositionInd));
			AssertEquals("Hard Copy Sent?", WorkflowDescriptor.GetFieldColumnDescription(Factory, USAddInfoSchema.US_P_HardCopySent));
			AssertEquals("Sample Sent?", WorkflowDescriptor.GetFieldColumnDescription(Factory, USAddInfoSchema.US_P_SampleSent));
			AssertEquals("FAX Sent?", WorkflowDescriptor.GetFieldColumnDescription(Factory, USAddInfoSchema.US_P_FaxSent));

			foreach (var fieldColumn in WorkflowDescriptor.GetWorkflowTriggerFieldColumns())
			{
				var description = WorkflowDescriptor.GetFieldColumnDescription(Factory, fieldColumn);
				AssertEquals("A description for field column '" + fieldColumn.Name + "' must be specified", false, description.Contains("_"));
			}
		}

		public override void TestDescription()
		{
			AssertEquals("Protest declaration job", WorkflowDescriptor.Description);
		}

		public override void TestID()
		{
			AssertEquals("PRO", WorkflowDescriptor.Code);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresBranch);
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

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		public override void TestSubTypes()
		{
			AssertEquals("0 sub type", 0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		public override void TestWorkflowProviderType()
		{
			AssertEquals(typeof(JobDeclaration), WorkflowDescriptor.WorkflowProviderType);
		}

		public override string GetPrefixFromColumnName(string fieldColumnName) => Factory.New<JobDeclaration>().TablePrefix;

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest() => new IWorkflowProvider[] { new Protest(Factory.New<JobDeclaration>()) };

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties => MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Email;

		protected override SchemaColumn[] ExpectedWorkflowTriggerFieldColumns
		{
			get
			{
				return new SchemaColumn[]
				{
					USAddInfoSchema.US_P_PeriodBaseDate,
					USAddInfoSchema.US_P_ApplicationFurtherReview,
					USAddInfoSchema.US_P_AcceleratedDispositionInd,
					USAddInfoSchema.US_P_HardCopySent,
					USAddInfoSchema.US_P_SampleSent,
					USAddInfoSchema.US_P_FaxSent
				};
			}
		}

		protected override BusinessObject NewBusinessObjectInTable(ITableSchema table) => new Protest(Factory.New<JobDeclaration>());
	}
}

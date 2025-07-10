using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	[TestedType(typeof(eManifestWorkflowDescriptor))]
	sealed class eManifestWorkflowDescriptorTest : WorkflowDescriptorTestCase<eManifestWorkflowDescriptor>
	{
		public new void TestGetFieldColumnDescription()
		{
			AssertEquals("BH_ReleaseStatus.Description", "e-Manifest Release Status", WorkflowDescriptor.GetFieldColumnDescription(Factory, CusInBondHeaderSchema.BH_ReleaseStatus));
			AssertEquals("B0_ReleaseStatus.Description", "Shipment Release Status", WorkflowDescriptor.GetFieldColumnDescription(Factory, CusInBondBillSchema.B0_ReleaseStatus));
			foreach (var fieldColumn in WorkflowDescriptor.GetWorkflowTriggerFieldColumns())
			{
				var description = WorkflowDescriptor.GetFieldColumnDescription(Factory, fieldColumn);
				AssertEquals("A description for field column '" + fieldColumn.Name + "' must be specified", false, description.Contains("_"));
			}
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals("SupportsEventTracking", true, WorkflowDescriptor.SupportsEventTracking);
		}

		public override void TestID()
		{
			AssertEquals("Code", JobInvoicingConsumerTypes.eManifest.Code, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Description", JobInvoicingConsumerTypes.eManifest.Description, WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			AssertEquals("0 sub type", 0, WorkflowDescriptor.SubTypeInformation.Length);
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
			AssertEquals(true, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties => MessageRecipientPartyType.Email;

		protected override SchemaColumn[] ExpectedWorkflowTriggerFieldColumns => new SchemaColumn[] { CusInBondHeaderSchema.BH_ReleaseStatus, CusInBondBillSchema.B0_ReleaseStatus };

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest() => new IWorkflowProvider[] { Factory.NewWithValidTestData<Trip>() };

		protected override BusinessObject NewBusinessObjectInTable(ITableSchema table) => table.TableName == Trip.Schema.TableName ? Factory.NewWithValidTestData<Trip>()
			: table.TableName == Shipment.Schema.TableName ? Factory.NewWithValidTestData<Shipment>() : base.NewBusinessObjectInTable(table);
	}
}

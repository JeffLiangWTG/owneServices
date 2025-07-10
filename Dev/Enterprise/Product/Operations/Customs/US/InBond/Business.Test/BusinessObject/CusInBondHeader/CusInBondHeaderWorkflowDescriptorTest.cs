using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(CusInBondHeaderWorkflowDescriptor))]
	sealed class CusInBondHeaderWorkflowDescriptorTest : WorkflowDescriptorTestCase<CusInBondHeaderWorkflowDescriptor>
	{
		public new void TestGetFieldColumnDescription()
		{
			AssertEquals("BM_CustomsStatus.Description", "Message Status", WorkflowDescriptor.GetFieldColumnDescription(Factory, CusInBondMoveHeaderSchema.BM_CustomsStatus));
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
			AssertEquals("Code", WorkflowDescriptors.CusInBondHeaderWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Description", "US InBond", WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			AssertEquals("0 sub type", 0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresPort1);
			AssertEquals("Port Of Loading", WorkflowDescriptor.Port1Name);
			AssertEquals(true, WorkflowDescriptor.RequiresPort2);
			AssertEquals("Port Of Arrival", WorkflowDescriptor.Port2Name);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
			AssertEquals("Importer", WorkflowDescriptor.ClientName);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.CusInBondHeader, WorkflowDescriptor.DocumentBusinessContext[0]);
		}

		public void TestControllerID()
		{
			AssertEquals(ControllerIDs.Customs.US.InBond, WorkflowDescriptor.ControllerID);
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties => MessageRecipientPartyType.Email | MessageRecipientPartyType.OrgProxy;

		protected override SchemaColumn[] ExpectedWorkflowTriggerFieldColumns => new SchemaColumn[] { CusInBondMoveHeaderSchema.BM_CustomsStatus };

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest() => new IWorkflowProvider[] { Factory.NewWithValidTestData<CusInBondHeader>() };

		protected override BusinessObject NewBusinessObjectInTable(ITableSchema table)
		{
			BusinessObject result = null;
			switch (table.TableName)
			{
				case CusInBondHeader.Schema.TableName:
					result = Factory.NewWithValidTestData<CusInBondHeader>();
					break;
				case CusInBondMoveHeader.Schema.TableName:
					var header = Factory.NewWithValidTestData<CusInBondHeader>();
					result = header.MovementHeaders.AddNew();
					break;
				default:
					result = base.NewBusinessObjectInTable(table);
					break;
			}

			return result;
		}
	}
}

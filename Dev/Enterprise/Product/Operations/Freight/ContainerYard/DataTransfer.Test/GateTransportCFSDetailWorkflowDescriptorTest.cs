
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.ContainerYard.Business.Testing
{
	[TestedType(typeof(GateTransportCFSDetailWorkflowDescriptor))]
	sealed class GateTransportCFSDetailWorkflowDescriptorTest : WorkflowDescriptorTestCase<GateTransportCFSDetailWorkflowDescriptor>
	{
		#region Implementation

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return MessageRecipientPartyType.Client | MessageRecipientPartyType.TransportCo;
			}
		}

		public override void TestDescription()
		{
			AssertEquals("Gate Transport CFS", WorkflowDescriptor.Description);
		}

		public override void TestID()
		{
			AssertEquals(WorkflowDescriptors.GateTransportCFSWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestRequiresBranch()
		{
			Assert(WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresClient()
		{
			Assert(WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresDepartment()
		{
			Assert(!WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestRequiresPorts()
		{
			Assert(!WorkflowDescriptor.RequiresPort1);
			Assert(!WorkflowDescriptor.RequiresPort2);
		}

		public override void TestSubTypes()
		{
			AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		public override void TestSupportsTasks()
		{
			Assert(!WorkflowDescriptor.SupportsTasks);
		}

		public override void TestSupportsEventTracking()
		{
			Assert(WorkflowDescriptor.SupportsEventTracking);
		}

		protected override bool RequiresWarehouseExpectedResult => true;

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var detail = Factory.New<GateTransportCFSDetail>();
			detail.GTF_OH_Owner = ClientOrg.PK;

			return new IWorkflowProvider[] { detail };
		}

		//protected override SchemaColumn[] ExpectedWorkflowTriggerFieldColumns
		//{
		//	get
		//	{
		//		return new SchemaColumn[]
		//			{
		//				GateTransportCFSDetailSchema.GTF_IsPickup,
		//			};
		//	}
		//}

		#endregion
	}
}

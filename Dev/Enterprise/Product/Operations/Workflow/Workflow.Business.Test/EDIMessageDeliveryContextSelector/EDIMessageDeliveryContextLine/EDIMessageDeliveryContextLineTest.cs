using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(EDIMessageDeliveryContextLine))]
	public class EDIMessageDeliveryContextLineTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var selector = Factory.New<EDIMessageDeliveryContextSelector>();
			selector.ECS_Code = "ABC";
			selector.ECS_ProcessType = "ABC";
			selector.ECS_Description = "ABC";
			return selector.Lines.AddNew();
		}

		public void TestIWorkflowTypeProvider()
		{
			var line = GetNewBusinessObject() as EDIMessageDeliveryContextLine;
			var typeProvider = line as IWorkflowTypeProvider;
			line.Parent.ECS_ProcessType = "";
			AssertEquals(false, typeProvider.IsTemplate);
			AssertEquals("", typeProvider.WorkflowProcessType);

			line.Parent.ECS_ProcessType = WorkflowDescriptors.DummyWorkflowDescriptorCode;
			AssertEquals(WorkflowDescriptors.DummyWorkflowDescriptorCode, typeProvider.WorkflowProcessType);
		}

		public void TestIRootTypeProvider()
		{
			//IRootTypeProvider is used to define the business objects available in the ZMacrosFindBoxColumnStyleInfo find box
			var line = GetNewBusinessObject() as EDIMessageDeliveryContextLine;
			var typeProvider = line as IRootTypeProvider;

			AssertEquals(0, typeProvider.Roots.Length);
			AssertEquals(0, typeProvider.RootTypes.Length);

			line.Parent.ECS_ProcessType = WorkflowDescriptors.GlbStaffDescriptorCode;
			AssertEquals(0, typeProvider.Roots.Length);
			AssertEquals(1, typeProvider.RootTypes.Length);
			AssertEquals(typeof(GlbStaff), typeProvider.RootTypes[0]);

			line.Parent.ECS_ProcessType = "POO";
			AssertEquals(0, typeProvider.Roots.Length);
			AssertEquals(0, typeProvider.RootTypes.Length);
		}
	}
}

using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(EDIMessageDeliveryContextSelectorCollection))]
	public class EDIMessageDeliveryContextSelectorCollectionTest : ActiveBusinessObjectCollectionTestCase<EDIMessageDeliveryContextSelectorCollection>
	{
		protected override EDIMessageDeliveryContextSelectorCollection GetCollectionToTest() => new EDIMessageDeliveryContextSelectorCollection(Factory);

		public void TestCollectionByProcessType()
		{
			var item = Factory.New<EDIMessageDeliveryContextSelector>();
			item.ECS_Code = "ABC";
			item.ECS_Description = "ABC";
			item.ECS_ProcessType = WorkflowDescriptors.DummyWorkflowDescriptorCode;

			var item2 = Factory.New<EDIMessageDeliveryContextSelector>();
			item2.ECS_Code = "ABD";
			item2.ECS_Description = "ABD";
			item2.ECS_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			Factory.Save();

			var collection = new EDIMessageDeliveryContextSelectorCollection(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			AssertEquals(1, collection.Count);
			AssertEquals(true, collection.Contains(item));
		}
	}
}

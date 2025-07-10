using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Freight;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class WorkflowMacroDataContextManagerTest : TestCaseWithFactory
	{
		public void TestProxyModelConversion()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var task = dummy.WorkflowItems.Triggers.AddNew();
			var dataModel = TriggerConditionEvaluator.GetConditionsDataContext(new TriggerDataModel(Lazy.Create(() => task.P9_ActualDate)), new LogEventDataModel(new ExampleLog(task)), task.ParentBusinessObject, () => (task.ParentBusinessObject, task.WorkflowDescriptor));
			var curProperties = WorkflowMacroDataContextManager.GetProxyModel(dataModel).GetType().GetProperties();
			var preProperties = dataModel.GetType().GetProperties();
			Assert("Missing Properties", preProperties.All(pre =>
											curProperties.Any(cur =>
												pre.Name == cur.Name &&
												pre.PropertyType == cur.PropertyType)));
			Assert("Missing Source", curProperties.Any(property => property.Name == "Source" && property.PropertyType == dummy.GetType()));
			Assert("Missing TriggerSource", curProperties.Any(property => property.Name == "TriggerSource" && property.PropertyType == dummy.GetType()));
		}

		public void TestProxyModelConversionWithDifferentTypes()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var task = dummy.WorkflowItems.Triggers.AddNew();
			var shipment = (BusinessObject)Factory.New<ICommonShipment>();
			var glHeader = Factory.New<AccGLHeader>();
			var dataModel = TriggerConditionEvaluator.GetConditionsDataContext(new TriggerDataModel(Lazy.Create(() => task.P9_ActualDate)), new LogEventDataModel(new ExampleLog(task)), task.ParentBusinessObject, () => (task.ParentBusinessObject, task.WorkflowDescriptor));
			var model = WorkflowMacroDataContextManager.GetProxyModel(dataModel, shipment.GetType(), glHeader.GetType());
			var curProperties = model.GetType().GetProperties();
			var preProperties = dataModel.GetType().GetProperties();
			AssertEquals(dummy.GetType(), model.Parent_DebugOnly.GetType());
			Assert("Missing Properties", preProperties.All(pre =>
											curProperties.Any(cur =>
												pre.Name == cur.Name &&
												pre.PropertyType == cur.PropertyType)));
			Assert("Missing Source", curProperties.Any(property => property.Name == "Source" && property.PropertyType == shipment.GetType()));
			Assert("Missing TriggerSource", curProperties.Any(property => property.Name == "TriggerSource" && property.PropertyType == glHeader.GetType()));
		}

		public void TestProxyModelConversionWithSameTypes()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var task = dummy.WorkflowItems.Triggers.AddNew();
			var shipment = (BusinessObject)Factory.New<ICommonShipment>();
			var dataModel = TriggerConditionEvaluator.GetConditionsDataContext(new TriggerDataModel(Lazy.Create(() => task.P9_ActualDate)), new LogEventDataModel(new ExampleLog(task)), task.ParentBusinessObject, () => (task.ParentBusinessObject, task.WorkflowDescriptor));
			var model = WorkflowMacroDataContextManager.GetProxyModel(dataModel, shipment.GetType(), shipment.GetType());
			var curProperties = model.GetType().GetProperties();
			var preProperties = dataModel.GetType().GetProperties();
			AssertEquals(dummy.GetType(), model.Parent_DebugOnly.GetType());
			Assert("Missing Properties", preProperties.All(pre =>
											curProperties.Any(cur =>
												pre.Name == cur.Name &&
												pre.PropertyType == cur.PropertyType)));
			Assert("Missing Source", curProperties.Any(property => property.Name == "Source" && property.PropertyType == shipment.GetType()));
			Assert("Missing TriggerSource", curProperties.Any(property => property.Name == "TriggerSource" && property.PropertyType == shipment.GetType()));
		}
	}
}

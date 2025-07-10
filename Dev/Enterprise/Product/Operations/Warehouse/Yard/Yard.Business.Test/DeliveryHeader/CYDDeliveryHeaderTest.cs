using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDDeliveryHeader))]
	class CYDDeliveryHeaderTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<CYDDeliveryHeader>();
		}

		#region WorkflowProviderTests

		[TestedType(typeof(CYDDeliveryHeader))]
		public class CYDDeliveryHeaderWorkflowProviderTest : WorkflowProviderTest<CYDDeliveryHeader, CYDDeliveryHeaderProcessTaskCollection>
		{
			protected override ZString ExpectedWorkflowType => WorkflowDescriptors.CYDDeliveryHeaderWorkflowDescriptorCode;

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

				var matchingDeliveryHeader = Factory.NewWithValidTestData<CYDDeliveryHeader>();
				matchingDeliveryHeader.YDH_WW_Yard = yardWithTemplate.PK;

				var unMatchingDeliveryHeader = Factory.NewWithValidTestData<CYDDeliveryHeader>();
				unMatchingDeliveryHeader.YDH_WW_Yard = yardWithoutTemplate.PK;

				Factory.Save();

				AssertEquals("Template tasks created when yard matches", 1, matchingDeliveryHeader.WorkflowItems.Milestones.Count);
				AssertEquals("Template tasks NOT created when yard doesn't match", 0, unMatchingDeliveryHeader.WorkflowItems.Milestones.Count);
			}
		}

		#endregion
	}
}

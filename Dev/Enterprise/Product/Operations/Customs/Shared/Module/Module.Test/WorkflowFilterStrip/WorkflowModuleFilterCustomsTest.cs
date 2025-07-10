using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(WorkflowModuleFilterCustoms))]
	sealed class WorkflowModuleFilterCustomsTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMilestoneDate()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			ProcessTask milestone1 = declaration.WorkflowItems.Milestones.AddNew();
			milestone1.P9_Type = Core.Constants.Workflow.MilestoneType;
			milestone1.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 2));
			BaseJobDeclaration declaration2 = Factory.New<BaseJobDeclaration>();
			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			declaration2.JE_JS = shipment2.PK;
			ProcessTask milestone2 = shipment2.WorkflowItems.Milestones.AddNew();
			milestone2.P9_Type = Core.Constants.Workflow.MilestoneType;
			milestone2.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 3));
			Factory.Save();
			DummyFilterStripBizOWithWorkflowFilters milestone1Filter = new DummyFilterStripBizOWithWorkflowFilters();
			((WorkflowModuleFilter)milestone1Filter["Milestone Date"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((WorkflowModuleFilter)milestone1Filter["Milestone Date"]).Property1 = new ZDateTime(2000, 1, 1);
			((WorkflowModuleFilter)milestone1Filter["Milestone Date"]).Property2 = new ZDateTime(2000, 1, 3);
			((WorkflowModuleFilter)milestone1Filter["Milestone Date"]).IsActive = true;
			BaseJobDeclarationCollection dummyCollection = new BaseJobDeclarationCollection(Factory, milestone1Filter.Filter);
			dummyCollection.Load();
			Assert(dummyCollection.Contains(declaration));
			Assert(dummyCollection.Contains(declaration2));
		}

		public void TestMilestoneDateChecksParentTableCode()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var milestone1 = declaration1.WorkflowItems.Milestones.AddNew();
			milestone1.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 2));
			var declaration2 = Factory.New<BaseJobDeclaration>();
			var shipment2 = Factory.New<ForwardingShipment>();
			declaration2.JE_JS = shipment2.PK;
			var milestone2 = shipment2.WorkflowItems.Milestones.AddNew();
			milestone2.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 3));
			var declaration3 = Factory.New<BaseJobDeclaration>();
			var milestone3 = declaration3.WorkflowItems.Milestones.AddNew();
			milestone3.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 2));
			var declaration4 = Factory.New<BaseJobDeclaration>();
			var shipment4 = Factory.New<ForwardingShipment>();
			declaration4.JE_JS = shipment4.PK;
			var milestone4 = shipment4.WorkflowItems.Milestones.AddNew();
			milestone4.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 3));
			milestone3.P9_ParentTableCode = "P0";
			milestone4.P9_ParentTableCode = "P0";
			Factory.Save();
			var allFilters = new DummyFilterStripBizOWithWorkflowFilters();
			var milestoneFilter = (WorkflowModuleFilter)allFilters["Milestone Date"];
			milestoneFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			milestoneFilter.Property1 = new ZDateTime(2000, 1, 1);
			milestoneFilter.Property2 = new ZDateTime(2000, 1, 3);
			milestoneFilter.IsActive = true;
			var dummyCollection = new BaseJobDeclarationCollection(Factory, allFilters.Filter);
			dummyCollection.Load();
			Assert(dummyCollection.Contains(declaration1));
			Assert(dummyCollection.Contains(declaration2));
			Assert(!dummyCollection.Contains(declaration3));
			Assert(!dummyCollection.Contains(declaration4));
		}

		public void TestNextMilestone()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			ProcessTask milestone1 = declaration.WorkflowItems.Milestones.AddNew();
			milestone1.TriggerConditions.TriggerEventCode = "AID";
			milestone1.P9_Type = Core.Constants.Workflow.MilestoneType;
			milestone1.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2000, 1, 2)));
			milestone1.SetMilestoneActualDateForTest(ZDateTime.Empty);
			BaseJobDeclaration declaration2 = Factory.New<BaseJobDeclaration>();
			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			declaration2.JE_JS = shipment2.PK;
			ProcessTask milestone2 = shipment2.WorkflowItems.Milestones.AddNew();
			milestone2.TriggerConditions.TriggerEventCode = "AID";
			milestone2.P9_Type = Core.Constants.Workflow.MilestoneType;
			milestone2.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2000, 1, 3)));
			milestone2.SetMilestoneActualDateForTest(ZDateTime.Empty);
			Factory.Save();
			DummyFilterStripBizOWithWorkflowFilters milestone1Filter = new DummyFilterStripBizOWithWorkflowFilters();
			((WorkflowModuleFilter)milestone1Filter["Next Milestone"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((WorkflowModuleFilter)milestone1Filter["Next Milestone"]).Property1 = new ZDateTime(2000, 1, 1);
			((WorkflowModuleFilter)milestone1Filter["Next Milestone"]).Property2 = new ZDateTime(2000, 1, 3);
			((WorkflowModuleFilter)milestone1Filter["Next Milestone"]).IsActive = true;
			BaseJobDeclarationCollection dummyCollection = new BaseJobDeclarationCollection(Factory, milestone1Filter.Filter);
			dummyCollection.Load();
			Assert(dummyCollection.Contains(declaration));
			Assert(dummyCollection.Contains(declaration2));
		}

		public void TestNextMilestoneChecksForParentTableCode()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var milestone1 = declaration1.WorkflowItems.Milestones.AddNew();
			milestone1.TriggerConditions.TriggerEventCode = "AID";
			milestone1.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2000, 1, 2)));
			milestone1.SetMilestoneActualDateForTest(ZDateTime.Empty);
			var declaration2 = Factory.New<BaseJobDeclaration>();
			var shipment2 = Factory.New<ForwardingShipment>();
			declaration2.JE_JS = shipment2.PK;
			var milestone2 = shipment2.WorkflowItems.Milestones.AddNew();
			milestone2.TriggerConditions.TriggerEventCode = "AID";
			milestone2.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2000, 1, 3)));
			milestone2.SetMilestoneActualDateForTest(ZDateTime.Empty);
			var declaration3 = Factory.New<BaseJobDeclaration>();
			var milestone3 = declaration3.WorkflowItems.Milestones.AddNew();
			milestone3.TriggerConditions.TriggerEventCode = "AID";
			milestone3.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2000, 1, 2)));
			milestone3.SetMilestoneActualDateForTest(ZDateTime.Empty);
			var declaration4 = Factory.New<BaseJobDeclaration>();
			var shipment4 = Factory.New<ForwardingShipment>();
			declaration4.JE_JS = shipment4.PK;
			var milestone4 = shipment4.WorkflowItems.Milestones.AddNew();
			milestone4.TriggerConditions.TriggerEventCode = "AID";
			milestone4.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2000, 1, 3)));
			milestone4.SetMilestoneActualDateForTest(ZDateTime.Empty);
			milestone3.P9_ParentTableCode = "P0";
			milestone4.P9_ParentTableCode = "P0";
			Factory.Save();
			var allFilters = new DummyFilterStripBizOWithWorkflowFilters();
			var milestoneFilter = (WorkflowModuleFilter)allFilters["Next Milestone"];
			milestoneFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			milestoneFilter.Property1 = new ZDateTime(2000, 1, 1);
			milestoneFilter.Property2 = new ZDateTime(2000, 1, 3);
			milestoneFilter.IsActive = true;
			var dummyCollection = new BaseJobDeclarationCollection(Factory, allFilters.Filter);
			dummyCollection.Load();
			Assert(dummyCollection.Contains(declaration1));
			Assert(dummyCollection.Contains(declaration2));
			Assert(!dummyCollection.Contains(declaration3));
			Assert(!dummyCollection.Contains(declaration4));
		}

		public void TestLastCompletedMilestone()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			ProcessTask milestone1 = declaration.WorkflowItems.Milestones.AddNew();
			milestone1.P9_Type = Core.Constants.Workflow.MilestoneType;
			milestone1.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 2));
			BaseJobDeclaration declaration2 = Factory.New<BaseJobDeclaration>();
			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			declaration2.JE_JS = shipment2.PK;
			ProcessTask milestone2 = shipment2.WorkflowItems.Milestones.AddNew();
			milestone2.P9_Type = Core.Constants.Workflow.MilestoneType;
			milestone2.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 3));
			Factory.Save();
			DummyFilterStripBizOWithWorkflowFilters milestone1Filter = new DummyFilterStripBizOWithWorkflowFilters();
			((WorkflowModuleFilter)milestone1Filter["Last Completed Milestone"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((WorkflowModuleFilter)milestone1Filter["Last Completed Milestone"]).Property1 = new ZDateTime(2000, 1, 1);
			((WorkflowModuleFilter)milestone1Filter["Last Completed Milestone"]).Property2 = new ZDateTime(2000, 1, 3);
			((WorkflowModuleFilter)milestone1Filter["Last Completed Milestone"]).IsActive = true;
			BaseJobDeclarationCollection dummyCollection = new BaseJobDeclarationCollection(Factory, milestone1Filter.Filter);
			dummyCollection.Load();
			Assert(dummyCollection.Contains(declaration));
			Assert(dummyCollection.Contains(declaration2));
		}

		public void TestLastCompletedMilestoneChecksParentTableCode()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var milestone1 = declaration1.WorkflowItems.Milestones.AddNew();
			milestone1.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 2));
			var declaration2 = Factory.New<BaseJobDeclaration>();
			var shipment2 = Factory.New<ForwardingShipment>();
			declaration2.JE_JS = shipment2.PK;
			var milestone2 = shipment2.WorkflowItems.Milestones.AddNew();
			milestone2.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 3));
			var declaration3 = Factory.New<BaseJobDeclaration>();
			var milestone3 = declaration3.WorkflowItems.Milestones.AddNew();
			milestone3.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 2));
			var declaration4 = Factory.New<BaseJobDeclaration>();
			var shipment4 = Factory.New<ForwardingShipment>();
			declaration4.JE_JS = shipment4.PK;
			var milestone4 = shipment4.WorkflowItems.Milestones.AddNew();
			milestone4.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 3));
			milestone3.P9_ParentTableCode = "P0";
			milestone4.P9_ParentTableCode = "P0";
			Factory.Save();
			var allFilters = new DummyFilterStripBizOWithWorkflowFilters();
			var milestoneFilter = (WorkflowModuleFilter)allFilters["Last Completed Milestone"];
			milestoneFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			milestoneFilter.Property1 = new ZDateTime(2000, 1, 1);
			milestoneFilter.Property2 = new ZDateTime(2000, 1, 3);
			milestoneFilter.IsActive = true;
			var dummyCollection = new BaseJobDeclarationCollection(Factory, allFilters.Filter);
			dummyCollection.Load();
			Assert(dummyCollection.Contains(declaration1));
			Assert(dummyCollection.Contains(declaration2));
			Assert(!dummyCollection.Contains(declaration3));
			Assert(!dummyCollection.Contains(declaration4));
		}

		public void TestModuleFilterEmpty()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var shipment2 = Factory.New<ForwardingShipment>();
			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.JE_JS = shipment2.PK;
			Factory.Save();
			var allFilters = new DummyFilterStripBizOWithWorkflowFilters();
			var milestoneFilter = (WorkflowModuleFilter)allFilters["Last Completed Milestone"];
			milestoneFilter.IsActive = true;
			var collection = new BaseJobDeclarationCollection(Factory, allFilters.Filter);
			collection.Load();
			CombineAssertions(() =>
			{
				Assert(collection.Contains(declaration1));
				Assert(collection.Contains(declaration2));
			});
		}

		sealed class DummyFilterStripBizOWithWorkflowFilters : DummyFilterStripBusinessObject
		{
			public DummyFilterStripBizOWithWorkflowFilters()
			{
				QueryObjectType = typeof(BaseJobDeclaration);
			}

			protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
			{
				var helpers = base.GetCustomFilterStripsHelpersCore();
				helpers.Add(new WorkflowFilterStripsHelperCustoms(typeof(BaseJobDeclaration), ZString.Empty, Factory));
				return helpers;
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new WorkflowModuleFilterCustoms("TEST", typeof(BaseJobDeclaration), WorkflowModuleFilterTypes.MilestoneDate, "");
		}
	}
}

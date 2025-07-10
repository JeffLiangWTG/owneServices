using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(WorkflowModuleTextFilterCustoms))]
	sealed class WorkflowModuleTextFilterCustomsTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMilestoneCompleted()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var milestone1 = declaration1.WorkflowItems.Milestones.AddNew();
			milestone1.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 2));
			var trigger = declaration1.WorkflowItems.Triggers.AddNew();
			trigger.SetMilestoneActualDateForTest(ZDateTime.Empty);
			var shipment = Factory.New<ForwardingShipment>();
			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.JE_JS = shipment.PK;
			var milestone2 = shipment.WorkflowItems.Milestones.AddNew();
			milestone2.SetMilestoneActualDateForTest(ZDateTime.Empty);
			var exception = shipment.WorkflowItems.Exceptions.AddNew();
			exception.SetMilestoneActualDateForTest(new ZDateTimeOffset(new ZDateTime(2000, 1, 2)));
			Factory.Save();
			var allFilters = new DummyFilterStripBizOWithWorkflowFilters();
			var milestoneFilter = (ModuleTextFilter)allFilters["Milestone Completed"];
			milestoneFilter.Property = "Completed";
			milestoneFilter.IsActive = true;
			var collection = new BaseJobDeclarationCollection(Factory, allFilters.Filter);
			collection.Load();
			Assert(collection.Contains(declaration1));
			Assert(!collection.Contains(declaration2));
			milestoneFilter.Property = "Not Completed";
			milestoneFilter.IsActive = true;
			collection = new BaseJobDeclarationCollection(Factory, allFilters.Filter);
			collection.Load();
			Assert(!collection.Contains(declaration1));
			Assert(collection.Contains(declaration2));
		}

		public void TestMilestoneCompletedChecksParentTableCode()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var milestone1 = declaration1.WorkflowItems.Milestones.AddNew();
			milestone1.SetMilestoneActualDateForTest(new ZDateTime(2012, 1, 2));
			var trigger1 = declaration1.WorkflowItems.Triggers.AddNew();
			trigger1.SetMilestoneActualDateForTest(ZDateTime.Empty);
			var shipment2 = Factory.New<ForwardingShipment>();
			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.JE_JS = shipment2.PK;
			var milestone2 = shipment2.WorkflowItems.Milestones.AddNew();
			milestone2.SetMilestoneActualDateForTest(new ZDateTime(2012, 1, 2));
			var trigger2 = declaration2.WorkflowItems.Triggers.AddNew();
			trigger2.SetMilestoneActualDateForTest(ZDateTime.Empty);
			Factory.Save();
			var allFilters = new DummyFilterStripBizOWithWorkflowFilters();
			var milestoneFilter = (ModuleTextFilter)allFilters["Milestone Completed"];
			milestoneFilter.Property = "Completed";
			milestoneFilter.IsActive = true;
			var collection = new BaseJobDeclarationCollection(Factory, allFilters.Filter);
			collection.Load();
			Assert(collection.Contains(declaration1));
			Assert(collection.Contains(declaration2));
			milestone1.P9_ParentTableCode = "P0";
			milestone2.P9_ParentTableCode = "P0";
			Factory.Save();
			collection = new BaseJobDeclarationCollection(Factory, allFilters.Filter);
			collection.Load();
			Assert(!collection.Contains(declaration1));
			Assert(!collection.Contains(declaration2));
		}

		public void TestModuleFilterEmpty()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var shipment2 = Factory.New<ForwardingShipment>();
			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.JE_JS = shipment2.PK;
			Factory.Save();
			var allFilters = new DummyFilterStripBizOWithWorkflowFilters();
			var milestoneFilter = (ModuleTextFilter)allFilters["Milestone Completed"];
			milestoneFilter.Property = ZString.Empty;
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
			return new WorkflowModuleTextFilterCustoms("TEST", delegate
			{
				return new ZDBOnlySubQuery(typeof(BaseJobDeclaration), JobDeclarationSchema.JE_JS);
			}, new List<ZString>(), typeof(BaseJobDeclaration), "");
		}
	}
}

using System;
using System.ComponentModel;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Control = System.Windows.Forms.Control;
using WhsDocket = Enterprise.Warehouse.Transactions.Business.WhsDocket;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	public class WhsWorkflowFilterStripTest : WhsTestCaseWithFactory
	{
		#region TestIJobManagementAmountFilter

		public void TestIJobManagementAmountFilter()
		{
			var testJobManagementAmountFilter = new TestJobManagementAmountFilter("moo", AccTransactionHeaderSchema.AH_ExchangeRate);

			using (var strip = new TestWhsWorkflowFilterStrip())
			{
				Control[] controls = strip.GetCurrentFilterControls(testJobManagementAmountFilter);
				try
				{
					AssertType<JobManagementAmountFilterControl>(controls[0]);
				}
				finally
				{
					DisposeControls(controls);
				}
			}
		}

		#endregion

		#region TestINumberRangeByUnitFilter

		public void TestINumberRangeByUnitFilter()
		{
			var numberRangeByUnitFilter = new NumberRangeByUnitFilter("moo", (v1, v2, unit) => new ZDBOnlyQuery(typeof(WhsDocket)), new CodeDescriptionPairList());

			using (var strip = new TestWhsWorkflowFilterStrip())
			{
				Control[] controls = strip.GetCurrentFilterControls(numberRangeByUnitFilter);

				try
				{
					AssertEquals("Should be 3 control for " + numberRangeByUnitFilter.GetType().Name, 3, controls.Length);
					AssertControl<ZDropEditWithFixedWidth>(strip, controls[0], "Property");
					AssertControl<ZCalcEdit>(strip, controls[1], "Property1");
					AssertControl<ZCalcEdit>(strip, controls[2], "Property2");
				}
				finally
				{
					DisposeControls(controls);
				}
			}
		}

		#endregion

		#region TestServiceTypeDateFilter

		public void TestServiceTypeDateFilter()
		{
			var filter = new ServiceTypeDateFilter(new FilterStripBusinessObjectForTest(), typeof(WhsDocket), true);

			using (TestWhsWorkflowFilterStrip strip = new TestWhsWorkflowFilterStrip())
			{
				Control[] controls = strip.GetCurrentFilterControls(filter);

				try
				{
					AssertEquals("Should be 1 control for " + filter.GetType().Name, 1, controls.Length);
					AssertControl<ServiceTypeDateFilterControl>(strip, controls[0], ".");
					AssertEquals(strip.PreferredHeightForTest, controls[0].Height);
				}
				finally
				{
					DisposeControls(controls);
				}
			}
		}

		#endregion

		#region Implementation

		void AssertControl<T>(TestWhsWorkflowFilterStrip strip, Control control, string bindingMember)
		{
			AssertType("Control's Type", typeof(T), control);
			AssertEquals("Control's BindingMember", bindingMember, strip.FilterControlBindingSource.GetBindingMember(control));
		}

		void DisposeControls(Control[] controls)
		{
			foreach (Control control in controls)
			{
				control.Dispose();
			}
		}

		#endregion

		#region Test Classes

		class TestWhsWorkflowFilterStrip : WhsWorkflowFilterStrip
		{
			public new ZBindingSource FilterControlBindingSource
			{
				get { return base.FilterControlBindingSource; }
			}

			public new Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
			{
				return base.GetCurrentFilterControls(currentModuleFilter);
			}

			public int PreferredHeightForTest => PreferredHeight;
		}

		public class TestJobManagementAmountFilter : ModuleFilter, IJobManagementAmountFilter
		{
			public TestJobManagementAmountFilter(ZString description, SchemaColumn filterColumn) : base(description, filterColumn)
			{
			}

			public override bool IsExpensiveQuery => false;

			protected override bool IsEmptyCore => false;

			protected override FilterCategory DefaultCategory
			{
				get { return FilterCategories.Other; }
			}

			protected override object[] QueryDelegateParameters
			{
				get { return Array.Empty<object>(); }
			}

			public IComponent GetFilterControl() => new JobManagementAmountFilterControl();

			protected override void ClearCore()
			{
			}

			protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
			{
			}

			protected override void DeserializePropertiesFromXml(XmlReader reader)
			{
			}

			protected override void FillWithValidTestFilterValueCore()
			{		
			}

			protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			{
				return new ModuleTextFilter("someFilterThatShouldBeAdded", delegate
				{ return new ZQuery(); });
			}

			protected override ModuleFilterValidation GetNewValidation()
			{
				return null;
			}

			protected override ZQuery GetQueryUsingFilterColumns()
			{
				return new ZQuery();
			}

			protected override void SerializePropertiesToXml(XmlWriter writer)
			{
			}
		}
		#endregion
	}

	internal class JobManagementAmountFilterControl : ZUserControl
	{
		public JobManagementAmountFilterControl()
		{
		}
	}
}

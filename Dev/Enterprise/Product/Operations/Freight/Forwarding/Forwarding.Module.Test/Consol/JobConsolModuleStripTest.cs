using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common.Module;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CarbonEmissions.Business.Testing;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	public class JobConsolModuleStripTest : BaseFreightTest
	{
		[RequiresSTA]
		public void TestCo2eNumberRangeControlsIsReadOnlyWhenCO2eStatusIsNotCurrent()
		{
			var co2eStatusControlName = "CO2eStatusDropEdit";
			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			using (var form = new ZForm())
			using (var jobConsolModuleStrip = new JobConsolModuleStrip())
			using (var co2eControl = new CO2eStatusAndCO2eKgRangeNumberFilterControl(jobConsolModuleStrip))
			{
				var co2efilter = new CO2eStatusAndCO2eKgRangeNumberFilter("CO2e", null);
				co2eControl.SetDataBinding(co2efilter, string.Empty);
				form.Controls.Add(co2eControl);
				form.Show();

				var co2eStatusDropEdit = co2eControl.FindSingleOrDefault<ZDropEdit>(x => x.Name == co2eStatusControlName);
				AssertNotNull(co2eStatusDropEdit);

				AssertControlsReadOnlyness(co2eControl, shouldBeReadOnly: false);

				var co2eStatusCodes = new string[] { CO2eStatusList.Codes.NotCalculated, CO2eStatusList.Codes.NotCurrent, CO2eStatusList.Codes.Pending, CO2eStatusList.Codes.Rejected };
				foreach (var code in co2eStatusCodes)
				{
					co2eStatusDropEdit.SelectItem(code);
					Application.DoEvents();
					UserIdleWorker.Flush();
					AssertControlsReadOnlyness(co2eControl, shouldBeReadOnly: true);
				}
			}

			void AssertControlsReadOnlyness(CO2eStatusAndCO2eKgRangeNumberFilterControl co2eControl, bool shouldBeReadOnly)
			{
				var messageReadonlyness = shouldBeReadOnly ? "" : "not";
				foreach (Control control in co2eControl.Controls)
				{
					if (control.Name != co2eStatusControlName)
					{
						AssertEquals($"Control should {messageReadonlyness} be readonly: {control.Name}", shouldBeReadOnly, control.GetReadOnly());
					}
				}
			}
		}

		public void TestCo2eNumberRangeControlsIsNotReadOnlyWhenCO2eRegistryIsOff()
		{
			var filter = new CO2eStatusAndCO2eKgRangeNumberFilter("CO2e", null);
			using (CO2eTestHelper.MockCO2eFeatureControl(false))
			using (var strip = new TestJobConsolModuleStrip())
			{
				AssertExceptionThrown("Co2eNumberRangeControlsIsNotVisble", typeof(NullReferenceException), () => strip.GetCurrentFilterControls(filter));
			}
		}

		public void TestHandlesCusEntryNumberFilters()
		{
			AssertProvidesControlsFor(new CusEntryNumTextFilter("Blah", typeof(DynamicBusinessObject)));
			AssertProvidesControlsFor(new CusEntryNumDateFilter("Blah", typeof(DynamicBusinessObject)));
		}

		public void TestHandlesReferenceNumbers()
		{
			AssertProvidesControlsFor(new ReferenceNumberFilter("description", delegate
			{ return new ZQuery(); }, new RefCountryCollection(Factory)));
		}

		public void TestHandlesOrgRelatedPartiesModuleFilter()
		{
			OrgRelatedPartiesModuleFilter filter = new OrgRelatedPartiesModuleFilter("Blah", GetFilter);
			using (TestJobConsolModuleStrip strip = new TestJobConsolModuleStrip())
			{
				Control[] controls = strip.GetCurrentFilterControls(filter);

				try
				{
					AssertEquals("Should be 1 control", 1, controls.Length);
					AssertControl<OrgRelatedPartiesFilterControl>(strip, controls[0], "");
				}
				finally
				{
					DisposeControls(controls);
				}
			}
		}

		public void TestHandlesDateLocationFilter()
		{
			DateLocationFilter filter = new DateLocationFilter("description", SailingFilterBuilder.Dates.ETD, BindToLists.GetCachedLists(Factory).RefLocation_List, DateLocationFilter.LocationTypes.Load, DateLocationFilter.TargetFilterTypes.Consol, Factory);

			using (TestJobConsolModuleStrip strip = new TestJobConsolModuleStrip())
			{
				Control[] controls = strip.GetCurrentFilterControls(filter);

				try
				{
					AssertEquals("Should be 1 control", 1, controls.Length);
					AssertControl<DateLocationFilterControl>(strip, controls[0], ".");
				}
				finally
				{
					DisposeControls(controls);
				}
			}
		}

		public void TestHandlesDateOrganizationFilter()
		{
			var filter = new DateOrganizationFilter(
				"description",
				JobConsolSchema.JK_PackDepotReceiptRequested,
				(SQLComparisonOperator comparisonOperator, object pK) => null,
				BindToLists.GetCachedLists(Factory).PackDepot_List,
				null
			);

			using (var strip = new TestJobConsolModuleStrip())
			{
				var controls = strip.GetCurrentFilterControls(filter);

				try
				{
					AssertEquals("Should be 1 control", 1, controls.Length);
					AssertControl<DateOrganizationFilterControl>(strip, controls[0], ".");
				}
				finally
				{
					DisposeControls(controls);
				}
			}
		}

		public void TestHandlesDgClassDGSubstanceFilter()
		{
			DGClassDGSubstanceFilter filter = new DGClassDGSubstanceFilter("description", delegate
			{ return new ZQuery(); });

			using (TestJobConsolModuleStrip strip = new TestJobConsolModuleStrip())
			{
				Control[] controls = strip.GetCurrentFilterControls(filter);

				try
				{
					AssertEquals("Should be 1 control", 1, controls.Length);
					AssertControl<DGClassDGSubstanceFilterControl>(strip, controls[0], "");
				}
				finally
				{
					DisposeControls(controls);
				}
			}
		}

		#region Test Classes

		class TestJobConsolModuleStrip : JobConsolModuleStrip
		{
			public new ZBindingSource FilterControlBindingSource
			{
				get { return base.FilterControlBindingSource; }
			}

			public new Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
			{
				return base.GetCurrentFilterControls(currentModuleFilter);
			}
		}

		#endregion

		#region Implementation

		void AssertProvidesControlsFor(ModuleFilter filter)
		{
			using (TestJobConsolModuleStrip strip = new TestJobConsolModuleStrip())
			{
				Control[] controls = strip.GetCurrentFilterControls(filter);
				Assert("Should provide controls for " + filter.GetType().Name, controls.Length > 0);

				foreach (Control control in controls)
				{
					if (!IsExemptFromBindingRequirement(control))
					{
						bool isBound = !string.IsNullOrEmpty(strip.FilterControlBindingSource.GetBindingMember(control));
						AssertEquals("BindingMember set on FilterControlBindingSource", true, isBound);
					}

					control.Dispose();
				}
			}
		}

		bool IsExemptFromBindingRequirement(Control control)
		{
			return control is ZLabel;
		}

		void AssertControl<T>(TestJobConsolModuleStrip strip, Control control, string bindingMember)
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

		ZQuery GetFilter(ZQuery filter)
		{
			return new ZQuery();
		}

		#endregion
	}
}

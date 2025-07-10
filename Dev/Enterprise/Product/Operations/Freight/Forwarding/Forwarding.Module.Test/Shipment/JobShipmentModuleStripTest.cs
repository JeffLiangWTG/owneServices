using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common.Module;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CarbonEmissions.Business.Testing;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	public class JobShipmentModuleStripTest : BaseFreightTest
	{
		public void TestHandlesReferenceNumberFilter()
		{
			ReferenceNumberFilter filter = new ReferenceNumberFilter("description", delegate
			{ return new ZQuery(); }, new RefCountryCollection(Factory));
			AssertProvidesControlsFor(filter);
		}

		public void TestHandlesModuleWarehouseLocationFilter()
		{
			AssertProvidesControlsFor(new ModuleWarehouseLocationFilter("Blah", GetFilter, null));
		}

		public void TestHandlesCusEntryNumberFilters()
		{
			AssertProvidesControlsFor(new CusEntryNumTextFilter("Blah", typeof(DynamicBusinessObject)));
			AssertProvidesControlsFor(new CusEntryNumDateFilter("Blah", typeof(DynamicBusinessObject)));
		}

		public void TestHandlesOrgRelatedPartiesModuleFilter()
		{
			OrgRelatedPartiesModuleFilter filter = new OrgRelatedPartiesModuleFilter("Blah", GetFilter);
			using (TestJobShipmentModuleStrip strip = new TestJobShipmentModuleStrip())
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

		public void TestHandlesOrgClientAssignedStaffModuleFilter()
		{
			OrgClientAssignedStaffModuleFilter filter = new OrgClientAssignedStaffModuleFilter("Blah", GetFilter);
			using (TestJobShipmentModuleStrip strip = new TestJobShipmentModuleStrip())
			{
				Control[] controls = strip.GetCurrentFilterControls(filter);

				try
				{
					AssertEquals("Should be 1 control", 1, controls.Length);
					AssertControl<OrgClientAssignedStaffFilterStrip>(strip, controls[0], "");
				}
				finally
				{
					DisposeControls(controls);
				}
			}
		}

		public void TestHandlesDateLocationFilter()
		{
			DateLocationFilter filter = new DateLocationFilter("description", SailingFilterBuilder.Dates.ETD, BindToLists.GetCachedLists(Factory).RefLocation_List, DateLocationFilter.LocationTypes.Load, DateLocationFilter.TargetFilterTypes.Shipment, Factory);

			using (TestJobShipmentModuleStrip strip = new TestJobShipmentModuleStrip())
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
				JobShipmentSchema.JS_ImportReleaseDepotReceiptRequested,
				(SQLComparisonOperator comparisonOperator, object pK) => null,
				BindToLists.GetCachedLists(Factory).PackDepot_List,
				null
			);

			using (var strip = new TestJobShipmentModuleStrip())
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

			using (TestJobShipmentModuleStrip strip = new TestJobShipmentModuleStrip())
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

		public void TestHandlesServiceTypeDateFilterControl()
		{
			var filter = new ServiceTypeDateFilter(new FilterStripBusinessObjectForTest(), typeof(ForwardingShipment), true);

			using (TestJobShipmentModuleStrip strip = new TestJobShipmentModuleStrip())
			{
				Control[] controls = strip.GetCurrentFilterControls(filter);

				try
				{
					AssertEquals("Should be 1 control", 1, controls.Length);
					AssertControl<ServiceTypeDateFilterControl>(strip, controls[0], ".");
				}
				finally
				{
					DisposeControls(controls);
				}
			}
		}

		public void TestCo2eNumberRangeControlsIsReadOnlyWhenCO2eStatusIsNotCurrent()
		{
			var co2eStatusControlName = "CO2eStatusDropEdit";

			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			using (var form = new ZForm())
			using (var jobShipmentModuleStrip = new JobShipmentModuleStrip())
			using (var co2eControl = new CO2eStatusAndCO2eKgRangeNumberFilterControl(jobShipmentModuleStrip))
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
					DoEvents();
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

		void DoEvents()
		{
			var before = DateTime.Now;
			while (DateTime.Now.Subtract(before).TotalMilliseconds < 500)
			{
				Application.DoEvents();
			}
		}

		[RequiresSTA]
		public void TestCo2eNumberRangeControlsIsNotReadOnlyWhenCO2eRegistryIsOff()
		{
			var filter = new CO2eStatusAndCO2eKgRangeNumberFilter("CO2e", null);
			using (CO2eTestHelper.MockCO2eFeatureControl(false))
			using (var strip = new TestJobShipmentModuleStrip())
			{
				AssertExceptionThrown("Co2eNumberRangeControlsIsNotVisble", typeof(NullReferenceException), () => strip.GetCurrentFilterControls(filter));
			}
		}

		#region Test Classes

		class TestJobShipmentModuleStrip : JobShipmentModuleStrip
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

		ZQuery GetFilter(ZQuery filter)
		{
			return new ZQuery();
		}

		void AssertProvidesControlsFor(ModuleFilter filter)
		{
			using (TestJobShipmentModuleStrip strip = new TestJobShipmentModuleStrip())
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

		void AssertControl<T>(TestJobShipmentModuleStrip strip, Control control, string bindingMember)
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
	}
}

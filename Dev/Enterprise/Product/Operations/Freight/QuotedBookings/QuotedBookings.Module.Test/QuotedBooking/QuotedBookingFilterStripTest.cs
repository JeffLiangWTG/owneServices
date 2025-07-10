using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CarbonEmissions.Business.Testing;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.Freight.Module;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Freight.QuotedBookings.Module.Test
{
	internal sealed class QuotedBookingFilterStripTest : BaseFreightTest
	{
		public void TestCo2eNumberRangeControlsIsReadOnlyWhenCO2eStatusIsNotCurrent()
		{
			var co2eStatusControlName = "CO2eStatusDropEdit";

			using (var form = new ZForm())
			using (var quotedBookingFilterStrip = new QuotedBookingFilterStrip())
			using (var co2eControl = new CO2eStatusAndCO2eKgRangeNumberFilterControl(quotedBookingFilterStrip))
			using (CO2eTestHelper.MockCO2eFeatureControl(true))
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
			using (var strip = new TestQuotedBookingFilterStrip())
			{
				AssertExceptionThrown("Co2eNumberRangeControlsIsNotVisble", typeof(NullReferenceException), () => strip.GetCurrentFilterControls(filter));
			}
		}

		public void TestOrgRelatedPartiesModuleFilter()
		{
			AssertProvidesControlsFor(new OrgRelatedPartiesModuleFilter("Related Parties Test", GetSomeFilter));
		}

		public void TestReferenceNumberFilter()
		{
			AssertProvidesControlsFor(new ReferenceNumberFilter("description", delegate
			{ return new ZQuery(); }, new RefCountryCollection(Factory)));
		}

		public void TestHandlesDgClassDGSubstanceFilter()
		{
			AssertProvidesControlsFor(new DGClassDGSubstanceFilter("description", delegate
			{ return new ZQuery(); }));
		}

		public void TestServiceTypeDate()
		{
			AssertProvidesControlsFor(new ServiceTypeDateFilter(new FilterStripBusinessObjectForTest(), typeof(ViewQuotedBooking), false));
		}

		#region Implementation

		void AssertProvidesControlsFor(ModuleFilter filter)
		{
			using (QuotedBookingFilterStrip strip = new QuotedBookingFilterStrip())
			{
				Control[] controls = GetCurrentFilterControls(strip, filter);
				Assert("Should provide controls for " + filter.GetType().Name, controls.Length > 0);

				foreach (Control control in controls)
				{
					control.Dispose();
				}
			}
		}

		Control[] GetCurrentFilterControls(WorkflowFilterStrip strip, ModuleFilter filter)
		{
			MethodInfo info = typeof(QuotedBookingFilterStrip).GetMethod(
				"GetCurrentFilterControls", BindingFlags.Instance | BindingFlags.NonPublic);

			return (Control[])info.Invoke(strip, new object[] { filter });
		}

		ZQuery GetSomeFilter(ZQuery orgHeaderFilter)
		{
			return new ZQuery();
		}

		#endregion

		#region Test Classes

		class TestQuotedBookingFilterStrip : QuotedBookingFilterStrip
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
	}
}

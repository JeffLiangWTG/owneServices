using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	public class DocumentTrackingFilterStripTest : BaseFreightTest
	{
		[RequiresSTA]
		public void TestHandlesReferenceNumberFilter()
		{
			var filter = new ReferenceNumberFilter("description", delegate
			{ return new ZQuery(); }, new RefCountryCollection(Factory));
			AssertProvidesControlsFor(filter);
		}

		public void TestHandlesVoyageVesselModuleFilter()
		{
			var filter = new VoyageVesselModuleFilter("description", delegate
			{ return new ZQuery(); }, new RefVesselCollection(Factory));
			AssertProvidesControlsFor(filter);
		}

		void AssertProvidesControlsFor(ModuleFilter filter)
		{
			using (var strip = new TestDocumentTrackingFilterStrip())
			{
				var controls = strip.GetCurrentFilterControls(filter);

				using (new DisposableAction(() => DisposeControls(controls)))
				{
					Assert("Should provide controls for " + filter.GetType().Name, controls.Length > 0);
				}
			}
		}

		void DisposeControls(Control[] controls)
		{
			foreach (var control in controls)
			{
				control.Dispose();
			}
		}

		class TestDocumentTrackingFilterStrip : DocumentTrackingFilterStrip
		{
			public new Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
			{
				return base.GetCurrentFilterControls(currentModuleFilter);
			}
		}
	}
}

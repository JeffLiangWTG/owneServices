using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Module.Testing
{
	sealed class CommercialInvoiceFilterStripTest : TestCaseWithFactory
	{
		public void TestHandlesModuleOrgForwarderFilter()
		{
			AssertProvidesControlsFor(new ReferenceModuleFilter(CommercialInvoiceFilterConstants.References));
		}

		void AssertProvidesControlsFor(ModuleFilter filter)
		{
			using (CommercialInvoiceFilterStrip strip = new CommercialInvoiceFilterStrip())
			{
				Control[] controls = GetCurrentFilterControls(strip, filter);
				Assert("Should provide controls for " + filter.GetType().Name, controls.Length > 0);
				foreach (Control control in controls)
				{
					control.Dispose();
				}
			}
		}

		Control[] GetCurrentFilterControls(CommercialInvoiceFilterStrip strip, ModuleFilter filter)
		{
			MethodInfo info = typeof(CommercialInvoiceFilterStrip).GetMethod("GetCurrentFilterControls", BindingFlags.Instance | BindingFlags.NonPublic);
			return (Control[])info.Invoke(strip, new object[] { filter });
		}
	}
}

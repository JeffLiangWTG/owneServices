using System.Windows.Forms;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.GUI.Test
{
	[TestedType(typeof(CYDAdHocServiceOrderForm))]
	public class CYDAdHocServiceOrderFormTest : ZFormBasherTest
	{
		#region ZFormBasherTest

		protected override Form GetFormToBashCore()
		{
			var adHocServiceOrder = Factory.New<CYDAdHocServiceOrder>();

			var form = new CYDAdHocServiceOrderForm(adHocServiceOrder);
			form.ControllerID = ControllerIDs.CYDAdHocServiceOrder;

			return form;
		}

		#endregion

		public void TestBillingPlugIns()
		{
			using (var form = (CYDAdHocServiceOrderForm)GetFormToBash())
			{
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing));
			}
		}
	}
}

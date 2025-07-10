using System.Windows.Forms;
using Enterprise.OceanCarrier.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.OceanCarrier.GUI.Testing
{
	[TestedType(typeof(CarrierShipmentHeaderForm))]
	sealed class CarrierShipmentHeaderFormTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestPlugIns()
		{
			using (var form = (CarrierShipmentHeaderForm)GetFormToBash())
			{
				AssertNotNull("form has JobInvoicing plugin", form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing));
			}
		}

		protected override Form GetFormToBashCore()
		{
			var carrierShipmentHeader = Factory.New<CarrierShipmentHeader>();

			var form = new CarrierShipmentHeaderForm(carrierShipmentHeader);
			form.ControllerID = ControllerIDs.CarrierShipmentHeader;
			return form;
		}
	}
}

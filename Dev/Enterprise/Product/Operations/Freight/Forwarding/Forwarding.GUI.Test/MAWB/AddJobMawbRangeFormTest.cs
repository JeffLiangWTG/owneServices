using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(AddJobMawbRangeForm))]
	public class AddJobMawbRangeFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new AddJobMawbRangeForm(Factory.New<RangeJobMawb>());
		}

		public void TestAirlinePrefixForAlphanumericValues()
		{
			RangeJobMawb rangeJobMawb = Factory.New<RangeJobMawb>();
			using (var mawbForm = new AddJobMawbRangeForm(rangeJobMawb))
			{
				mawbForm.Show();

				var control = mawbForm.Controls.Find("AirlinePrefixTextBox", true)[0];
				KeySender.SendKeyPress(control, control.Handle, 'A');
				KeySender.SendKeyPress(control, control.Handle, 'B');
				KeySender.SendKeyPress(control, control.Handle, '1');

				AssertEquals(new ZString("AB1"), control.Text);
			}
		}
	}
}

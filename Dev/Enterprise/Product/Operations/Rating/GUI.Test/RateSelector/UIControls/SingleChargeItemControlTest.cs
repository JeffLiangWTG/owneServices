using System.Linq;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.Rating.GUI.RateSelector.UIControls;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing.RateSelector.UIControls
{
	public class SingleChargeItemControlTest : TestCase
	{
		public void TestNoIcon()
		{
			using (var form = new FormForTest())
			{
				var ctrl = form.ControlForTest;

				var vm = new CW1RateViewModelSample();
				var chargeWithNoError = vm.OtherCharges.Charges.First(ch => ch.ChargeCodeErrorLevel == ErrorLevel.None);

				ctrl.DataBind(chargeWithNoError);
				form.Show();

				Assert(!ctrl.ImageVislble);
			}
		}

		public void TestIconError()
		{
			using (var form = new FormForTest())
			{
				var ctrl = form.ControlForTest;

				var vm = new CW1RateViewModelSample();
				var chargeWithError = vm.OtherCharges.Charges.First(ch => ch.ChargeCodeErrorLevel == ErrorLevel.Error);

				ctrl.DataBind(chargeWithError);
				form.Show();

				Assert(ctrl.ImageVislble);
			}
		}

		public void TestIconWarning()
		{
			using (var form = new FormForTest())
			{
				var ctrl = form.ControlForTest;

				var vm = new CW1RateViewModelSample();
				var chargeWithError = vm.OtherCharges.Charges.OfType<ChargeViewModelSample>().First(ch => ch.ChargeCodeErrorLevel == ErrorLevel.Error);

				chargeWithError.ChargeCodeErrorLevel = ErrorLevel.Warning;
				ctrl.DataBind(chargeWithError);
				form.Show();

				Assert(ctrl.ImageVislble);
			}
		}
	}

	class FormForTest : ZForm
	{
		public FormForTest()
		{
			ControlForTest = new SingleChargeItemControlForTest();
			Controls.Add(ControlForTest);
		}

		public SingleChargeItemControlForTest ControlForTest { get; set; }
	}

	class SingleChargeItemControlForTest : ChargeToggleButton
	{
		public bool ImageVislble => picErrorWarning.Visible && picErrorWarning.Image is object;
	}
}

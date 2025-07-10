using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Integration;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.Rating.GUI.RateSelector.UIControls;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing.RateSelector.UIControls
{
	public class RateSelectorControlTest : TestCase
	{
		public void TestVariedWidthResultsExpandToMaximumSize()
		{
			var shortResult = new BookingEngineRateViewModelSample();
			shortResult.MainCharge.ChargeCode = "";
			shortResult.MainCharge.ChargeCodeDescription = "";

			var longResult = new BookingEngineRateViewModelSample();

			var samples = new BookingEngineRateViewModel[]
			{
				shortResult,
				longResult
			};
			var vm = new BookingRatesViewModel(samples);

			using (var tf = new RateSelectorControlTestForm(vm))
			{
				tf.Show();
				Application.DoEvents();

				var itemsContainer = tf.FindSingle<RatesCardControl>().Controls[0];

				var shortControl = itemsContainer.Controls[0];
				var longControl = itemsContainer.Controls[1];

#if WINZOR
				AssertEquals("Controls should be equal width", 1007, longControl.Size.Width);
				AssertEquals("Controls should be equal width", 1007, shortControl.Size.Width);
#else
				AssertEquals("Controls should be equal width", 1013, longControl.Size.Width);
				AssertEquals("Controls should be equal width", 1013, shortControl.Size.Width);
#endif
			}
		}

		public void TestLogVisibility()
		{
			var samples = new BookingEngineRateViewModel[]
			{
				new BookingEngineRateViewModelSample(),
				new BookingEngineRateViewModelSample()
			};
			var vm = new BookingRatesViewModel(samples);
			vm.Logger.Warning("This is a warning");

			using (var tf = new RateSelectorControlTestForm(vm))
			{
				tf.Show();
				Application.DoEvents();

				var btnWarnings = tf.RateSelectorControl.FindSingle<ZButton>(c => c.Name == "btnWarnings");
				var tbLogs = tf.RateSelectorControl.FindSingle<ZTextBox>(c => c.Name == "tbLogs");

				AssertEquals("Errors / Warnings (1)", btnWarnings.Text);

				btnWarnings.PerformClick();
				Application.DoEvents();
				Assert(tbLogs.Visible);
				AssertEquals("Warning : This is a warning", tbLogs.Text);

				vm.Logger.Error("This is an error");
				Application.DoEvents();
				AssertEquals("Adding error message dynamically should update UI", "Errors / Warnings (2)", btnWarnings.Text);
				AssertEquals("Adding error message dynamically should update UI", "Warning : This is a warning\r\n\r\nError : This is an error", tbLogs.Text);

				vm.Logger.Information("This is an information message");
				Application.DoEvents();
				AssertEquals("Adding information-level message should not update UI", "Errors / Warnings (2)", btnWarnings.Text);
				AssertEquals("Adding information-level message should not update UI", "Warning : This is a warning\r\n\r\nError : This is an error", tbLogs.Text);

				vm.Logger.Clear();
				Application.DoEvents();
				AssertEquals("Clearing logs should be reflected in UI", "Errors / Warnings (0)", btnWarnings.Text);
				AssertEquals("Clearing logs should be reflected in UI", "", tbLogs.Text);
			}
		}

		class RateSelectorControlTestForm : ZForm
		{
			public RateSelectorControlTestForm(BookingRatesViewModel vm)
			{
				AutoScaleMode = AutoScaleMode.None;
				ViewModel = vm;
				RateSelectorControl = new RateSelectorControl(ViewModel);
				RateSelectorControl.AutoScaleMode = AutoScaleMode.None;
				RateSelectorControl.Dock = DockStyle.Fill;

				Controls.Add(RateSelectorControl);

				Size = ControlDpiScalingHelper.NewScaledSize(2048, 500);
			}

			public BookingRatesViewModel ViewModel { get; set; }
			public RateSelectorControl RateSelectorControl { get; set; }

			protected override void Dispose(bool disposing)
			{
				ViewModel?.Dispose();
				base.Dispose(disposing);
			}
		}
	}
}

using System.Collections.ObjectModel;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.Rating.GUI.RateSelection.SpotUIControls;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.Rating.GUI.RateSelector.UIControls;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing.RateSelection.RateSelector.UIControls
{
	[TestedType(typeof(Form))]
	public class BookingEngineRateCardControlTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new TestForm(new BookingEngineRateViewModelSample());

		public void TestAdditionalDetailsShownWhenPopulated()
		{
			TestHiddenDetails(additionalDetailsPopulated: true, additionalDetailsPanelVisible: true);
		}

		public void TestAdditionalDetailsHiddenWhenEmpty()
		{
			TestHiddenDetails(additionalDetailsPopulated: false, additionalDetailsPanelVisible: false);
		}

		void TestHiddenDetails(bool additionalDetailsPopulated, bool additionalDetailsPanelVisible)
		{
			var vm = new BookingEngineRateViewModelSample();
			if (!additionalDetailsPopulated)
			{
				vm.AdditionalDetails = new ObservableCollection<AdditionalDetailsViewModel>();
			}

			using (var tf = new TestForm(vm))
			{
				tf.Show();
				Application.DoEvents();

				AssertEquals("Additional details panel does not match expected state", additionalDetailsPanelVisible, tf.RatesCardControl.pnlAdditionalDetailsContainerForTest.Visible);
			}
		}

		public void TestControlResizesWhenCollapses()
		{
			var vm = new BookingEngineRateViewModelSample();
			vm.IsExpanded = true;

			using (var tf = new TestForm(vm))
			{
				tf.Show();
				Application.DoEvents();
				var currentSize = tf.RatesCardControl.Size;

				vm.IsExpanded = false;
				Application.DoEvents();
				var newSize = tf.RatesCardControl.Size;

				var expectedSize =
#if WINZOR
					ControlDpiScalingHelper.NewScaledSize(1007, 115);
#else
					ControlDpiScalingHelper.NewScaledSize(1013, 115);
#endif
				Assert("Width remains constant", currentSize.Width == newSize.Width);
				Assert("Height is less", currentSize.Height > newSize.Height);
				AssertEquals("Size is collapsed", expectedSize, newSize);
			}
		}

		public void TestTotalPriceWarningIconIsVisible()
		{
			TestTotalPriceWarningIcon(warningSpecified: true);
		}

		public void TestTotalPriceWarningIconHidden()
		{
			TestTotalPriceWarningIcon(warningSpecified: false);
		}

		void TestTotalPriceWarningIcon(bool warningSpecified)
		{
			var vm = new BookingEngineRateViewModelSample();
			if (!warningSpecified)
			{
				vm.FreightCharges.Charges.ForEach(c => c.LocalAmountError = null);
			}

			using (var tf = new TestForm(vm))
			{
				tf.Show();
				Application.DoEvents();

				var hasTotalPriceError = warningSpecified;
				AssertEquals("TotalPriceError does not match expected", hasTotalPriceError, !string.IsNullOrEmpty(vm.TotalPriceError));

				var warningVisible = warningSpecified;
				AssertEquals("Icon visibility does not match expected", warningVisible, tf.RatesCardControl.totalPriceLargeDisplayForTest.warningPictureBoxForTest.Visible);
			}
		}

		public void TestAdditionalDetailsPanelHiddenWhenDetailsEmpty()
		{
			var vm = new BookingEngineRateViewModelSample();
			vm.AdditionalDetails = new ObservableCollection<AdditionalDetailsViewModel>();

			using (var tf = new TestForm(vm))
			{
				tf.Show();
				Application.DoEvents();

				Assert("Additional details panel should be hidden when details empty", !tf.RatesCardControl.pnlAdditionalDetailsContainerForTest.Visible);
			}
		}

		public void TestAirlineLogoImageSupplied()
		{
			TestAirlineLogoImage(supplied: true);
		}

		public void TestAirlineLogoImageNotSupplied()
		{
			TestAirlineLogoImage(supplied: false);
		}

		void TestAirlineLogoImage(bool supplied)
		{
			var vm = new BookingEngineRateViewModelSample();
			vm.AirlineIconBitmap = null;
			if (supplied)
			{
				vm.AirlineIconBitmap = Properties.Resources.PlaneDrawing;
			}

			using (var tf = new TestForm(vm))
			{
				tf.Show();
				Application.DoEvents();

				var expectedLogoImageVisible = supplied;
				AssertEquals("Logo image display does not match expected", expectedLogoImageVisible, tf.RatesCardControl.airlineLogoForTest.LogoVisibleForTest);
				var expectedFallbackImageVisible = !supplied;
				AssertEquals("Fallback image display does not match expected", expectedFallbackImageVisible, tf.RatesCardControl.airlineLogoForTest.FallbackLogoVisibleForTest);
			}
		}

		public void TestTruckingLegs()
		{
			var vm = new BookingEngineRateViewModelSample();
			vm.TransportLegs[1].TransportMode = TransportMode.Truck;

			using (var tf = new TestForm(vm))
			{
				tf.Show();
				Application.DoEvents();

				var icons = tf.RatesCardControl.routeViewIconsModeForTest.TransportIconsForTest.ToList();
				var expectedIcons = new[] { true, false, true };
				AssertArrayEqualsByElements("Displayed icons should match expected", expectedIcons, icons.Select(i => i.Image != null).ToArray());
			}
		}

		public void TestSelection_PropertyChange_Checked()
		{
			TestSelection_PropertyChange(true);
		}

		public void TestSelection_PropertyChange_Unchecked()
		{
			TestSelection_PropertyChange(false);
		}

		void TestSelection_PropertyChange(bool isSelected)
		{
			var vm = new BookingEngineRateViewModelSample();

			using (var tf = new TestForm(vm))
			{
				tf.Show();
				Application.DoEvents();

				tf.RatesCardControl.IsSelected = isSelected;
				Application.DoEvents();

				var expectedChecked = isSelected;
				var actualChecked = tf.RatesCardControl.selectedCheckBoxForTest.Checked;
				AssertEquals("Checkbox should reflect IsSelected property change", expectedChecked, actualChecked);
			}
		}

		public void TestSelection_UIChange_Checked()
		{
			TestSelection_UIChange(true);
		}

		public void TestSelection_UIChange_Unchecked()
		{
			TestSelection_UIChange(false);
		}

		void TestSelection_UIChange(bool isSelected)
		{
			var vm = new BookingEngineRateViewModelSample();

			using (var tf = new TestForm(vm))
			{
				tf.Show();
				Application.DoEvents();

				tf.RatesCardControl.selectedCheckBoxForTest.Checked = isSelected;
				Application.DoEvents();

				var expectedChecked = isSelected;
				var actualChecked = tf.RatesCardControl.IsSelected;
				AssertEquals("IsSelected property should reflect checkbox change", expectedChecked, actualChecked);
			}
		}

		public void TestDetailsDoNotExpandWhenWidthExpands()
		{
			var vm = new BookingEngineRateViewModelSample();

			using (var tf = new TestForm(vm))
			{
				tf.Show();
				Application.DoEvents();

#if WINZOR
				AssertEquals("Result control is in expanded state", 1007, tf.RatesCardControl.Size.Width);
#else
				AssertEquals("Result control is in expanded state", 1013, tf.RatesCardControl.Size.Width);
#endif

				var routing = tf.FindSingle<RoutesControl>().Controls[0];
				AssertEquals("Routing control should not expand when control expands, so that alignment is preserved", 640, routing.Size.Width);

				var additionalDetails = tf.FindSingle<AdditionalDetailsControl>().Controls[0];
				AssertEquals("Additional details control should not expand when control expands, so that alignment is preserved", 500, additionalDetails.Size.Width);
			}
		}

		public void TestVeryLongRemarksHaveEllipsis()
		{
			var vm = new BookingEngineRateViewModelSample();
			vm.Remarks = "This is some very long remarks that we don't want to wrap longer than a certain length as it seems sometimes the API results have quite longs remarks in them";

			using (var tf = new TestForm(vm))
			{
				tf.Show();
				Application.DoEvents();

				var remarksLabel = tf.RatesCardControl.Controls.Find("lblRemarks", true)[0];

				AssertGreaterThan(remarksLabel.PreferredSize.Width, remarksLabel.Size.Width);
				AssertEquals(remarksLabel.PreferredSize.Height, remarksLabel.Size.Height);
			}
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			return control.Name == "selectedCheckBox";
		}
	}

#if !WINZOR
	[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
#endif
	class TestForm : Form
	{
		public TestForm(BookingEngineRateViewModel vm)
		{
			AutoScaleMode = AutoScaleMode.None;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			RatesCardControl = new BookingEngineRateCardControl(vm);

			Controls.Clear();
			Controls.Add(RatesCardControl);
			Height = 600;
			Width = 2000;
		}

		public BookingEngineRateCardControl RatesCardControl { get; set; }
	}
}

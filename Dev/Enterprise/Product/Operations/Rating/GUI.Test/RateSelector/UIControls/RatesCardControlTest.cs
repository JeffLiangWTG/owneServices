using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.GUI.RateSelection.BaseControls;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.Rating.GUI.RateSelector.UIControls;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing.RateSelector.UIControls
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Some manual convenience tests added for manual visual inspection of controls")]
	class RatesCardControlTest : TestCase
	{
		public void TestVaryingSizeDatesAreSameSize()
		{
			var vm = new TestViewModel();
			AddVaryingSizeDates(vm);

			using (var form = new TestForm(vm))
			{
				form.Show();
				Application.DoEvents();

				// See note above
				AssertEqualWidth(form.RateCardChildren);
			}
		}

		public void TestVaryingSizeOptionalChargesAreSameSize()
		{
			var vm = new TestViewModel();
			AddVaryingSizeOptionalCharges(vm);

			using (var form = new TestForm(vm))
			{
				form.Show();
				Application.DoEvents();

				// See note above
				AssertEqualWidth(form.RateCardChildren);
			}
		}

		public void TestAddingLargeNumbersOfOptionalCharges()
		{
			var vm = new TestViewModel();
			AddLotsOfOptionalCharges(vm);

			using (var form = new TestForm(vm))
			{
				form.Show();
				Application.DoEvents();

				var col11 = form.RateCardChildren[0].FindSingle<Control>("pnlCol11");
				var mcOptionalCharges = form.RateCardChildren[0].FindSingle<Control>("mcOptionalCharges");

				AssertGreaterThanOrEqualTo(col11.Size.Width, mcOptionalCharges.Size.Width);
			}
		}

		public void TestWarningLabelsOnlyPresentWhenWarningsDefined()
		{
			var vm = new TestViewModel();
			vm.AddRate(new VisuallyMinimalRateViewModelSample());

			using (var form = new TestForm(vm))
			{
				form.Show();
				Application.DoEvents();

				var frtValidationImae = form.RateCardChildren[0].FindSingle<Control>("tbvFreightCharges").FindSingle<Control>("pbValidationImage");
				var optionalImage = form.RateCardChildren[0].FindSingle<Control>("tbvSubjectToCharges").FindSingle<Control>("pbValidationImage");

				Assert("No errors for freight charges, icon should not be visible", !frtValidationImae.Visible);
				Assert("Error defined for optional charges, icon shoudl be visible", optionalImage.Visible);
			}
		}

		public void TestMixedControlsAreSameSize()
		{
			var vm = new TestViewModel();
			AddVaryingSizeDates(vm);
			AddVaryingSizeOptionalCharges(vm);
			AddCW1(vm);

			using (var form = new TestForm(vm))
			{
				form.Show();
				Application.DoEvents();

				// See note above
				AssertEqualWidth(form.RateCardChildren);
			}
		}

		public void TestResultItemsUpdateOnRateChange()
		{
			var vm = new TestViewModel();
			AddCW1(vm);

			using (var form = new TestForm(vm))
			{
				form.Show();
				Application.DoEvents();

				vm.ClearRates();
				var secondItem = new CW1RateViewModelSample();
				secondItem.ServiceProviderName = "Second item";
				vm.AddRate(secondItem);
				Application.DoEvents();

				var lblServiceProviderName = form.RateCardChildren[0].Controls.Find("lblServiceProviderName", searchAllChildren: true)[0] as ZLabel;
				AssertEquals("Second item", lblServiceProviderName.Text);
			}
		}

		public void TestResultItemsShouldUpdateCorrectly_FromRatesIncludingSubjectChargesToRatesWithoutSubjectCharges()
		{
			var vm = new TestViewModel();
			var rateIncludeSubjectCharges = new CargoguideRateViewModelSample(includeSubjectCharges: true);
			var rateWithoutSubjectCharges = new CargoguideRateViewModelSample(includeSubjectCharges: false);
			vm.AddRate(rateWithoutSubjectCharges);

			using (var form = new TestForm(vm))
			{
				form.Show();
				Application.DoEvents();

				vm.ClearRates();
				vm.AddRate(rateIncludeSubjectCharges);
				Application.DoEvents();

				vm.ClearRates();

				AssertNoExceptionThrown(() => vm.AddRate(rateWithoutSubjectCharges));
				Application.DoEvents();
			}
		}

		public void TestTotalPriceWarningIcon()
		{
			var cg = new CargoguideRateViewModelSample();
			var abcdCharge = cg.SubjectToCharges.Charges.Single(ch => ch.ChargeCode == "ABCD");

			abcdCharge.LocalAmountError = null;
			AssertNullOrEmpty(cg.TotalPriceError);

			var vm = new TestViewModel();
			vm.AddRate(cg);

			using (var form = new TestForm(vm))
			{
				form.Show();
				Application.DoEvents();

				Assert("Warning icon not shown", !form.TotalPriceValidationImage.Visible);

				abcdCharge.LocalAmountError = "ABCD Error";
				AssertNotNullOrEmpty(cg.TotalPriceError);

				Application.DoEvents();

				Assert("Warning icon is shown", form.TotalPriceValidationImage.Visible);
			}
		}

		[DeveloperOnlyTest]
		public void TestLongCW1DateDoesNotWrap()
		{
			var vm = new TestViewModel();
			AddCW1(vm);

			var largeCw1 = new CW1RateViewModelSample
			{
				StartDate = new DateTime(2024, 02, 24),
				ExpiryDate = new DateTime(2024, 02, 24)
			};
			vm.AddRate(largeCw1);

			using (var form = new TestForm(vm))
			{
				form.Show();
				Application.DoEvents();

				var firstResult = form.RateCardChildren[0];
				var secondResult = form.RateCardChildren[1];

				AssertEquals("Controls are same width", firstResult.Size.Width, secondResult.Size.Width);

				var lblEffectiveFrom = secondResult.Controls.Find("lblEffectiveFrom", searchAllChildren: true)[0] as ZLabel;
				var lblEffectiveTo = secondResult.Controls.Find("lblEffectiveTo", searchAllChildren: true)[0] as ZLabel;

				AssertEquals("Long effective date has not wrapped off the screen", 13, lblEffectiveFrom.Size.Height);
				AssertEquals("Long effective date has not wrapped off the screen", 13, lblEffectiveTo.Size.Height);
			}

			Assert(true);
		}

		public void TestRebindOnNavigation_CargoGuide()
		{
			Func<string, RateViewModel> viewModelFunc = (carrier) =>
			{
				var cg1 = new CargoguideRateViewModelSample();
				cg1.CarrierName = carrier;
				return cg1;
			};

			Func<Control, string> getCarrier = (control) =>
			{
				return control.Controls.Find("lblCarrierName", searchAllChildren: true)[0].Text;
			};

			AssertRebindOnNavigation(viewModelFunc, getCarrier);
		}

		public void TestRebindOnNavigation_CW1()
		{
			Func<string, RateViewModel> viewModelFunc = (carrier) =>
			{
				var cw1 = new CW1RateViewModelSample();
				cw1.ServiceProviderName = carrier;
				return cw1;
			};

			Func<Control, string> getCarrier = (control) =>
			{
				return control.Controls.Find("lblServiceProviderName", searchAllChildren: true)[0].Text;
			};

			AssertRebindOnNavigation(viewModelFunc, getCarrier);
		}

		public void TestRebindOnNavigation_BookingEngine()
		{
			Func<string, RateViewModel> viewModelFunc = (carrier) =>
			{
				var be = new BookingEngineRateViewModelSample();
				be.CarrierName = carrier;
				be.IsExpanded = false;
				return be;
			};

			Func<Control, string> getCarrier = (control) =>
			{
				return control.Controls.Find("lblCarrierName", searchAllChildren: true)[0].Text;
			};

			AssertRebindOnNavigation(viewModelFunc, getCarrier);
		}

		void AssertRebindOnNavigation(Func<string, RateViewModel> viewModelFunc, Func<Control, string> getCarrier)
		{
			var vm = new TestViewModel();

			foreach (var i in Enumerable.Range(1, 40))
			{
				var c = viewModelFunc($"Carrier {i}");
				vm.AddRate(c);
			}

			using (var form = new TestForm(vm))
			{
				form.Show();
				Application.DoEvents();

				Func<string> getFirstControlCarrier = () =>
				{
					var allControls = form.Controls.Find("pnlItemsContainer", searchAllChildren: true)[0].Controls;
					var firstControl = allControls[0];
					var firstCarrier = getCarrier(firstControl);
					return firstCarrier;
				};

				var nextButton = form.Controls.Find("btnNextPage", searchAllChildren: true)[0] as ZButton;
				var prevButton = form.Controls.Find("btnPreviousPage", searchAllChildren: true)[0] as ZButton;

				var page1Carrier = getFirstControlCarrier();
				AssertEquals("First load should display page 1 results", "Carrier 1", page1Carrier);

				nextButton.PerformClick();
				Application.DoEvents();

				var page2Carrier = getFirstControlCarrier();
				AssertEquals("Navigating to second page should display page 2 results", "Carrier 21", page2Carrier);

				prevButton.PerformClick();
				Application.DoEvents();

				page1Carrier = getFirstControlCarrier();
				AssertEquals("Navigating back to first page should re-display page 1 results", "Carrier 1", page1Carrier);
			}
		}

		void AddVaryingSizeDates(TestViewModel vm)
		{
			vm.AddRate(new VisuallyMinimalRateViewModelSample());

			var expandedDate = new VisuallyMinimalRateViewModelSample()
			{
				IssueDate = new DateTime(2023, 12, 12, 12, 00, 00)
			};
			vm.AddRate(expandedDate);
		}

		void AddVaryingSizeOptionalCharges(TestViewModel vm)
		{
			vm.AddRate(new VisuallyMinimalRateViewModelSample());

			var expandedOptionalCharges = new VisuallyMinimalRateViewModelSample();
			expandedOptionalCharges.OptionalCharges.Charges.ForEach(ch =>
			{
				ch.IsSelected = true;
				ch.LocalAmount = 500;
			});
			expandedOptionalCharges.OptionalCharges.Charges.Last().LocalAmountError = "This is a test";
			vm.AddRate(expandedOptionalCharges);
		}

		void AddLotsOfOptionalCharges(TestViewModel vm)
		{
			var lotsOfOptionalCharges = new VisuallyMinimalRateViewModelSample();

			var codes = new[] { "First", "Second", "Third", "Fourth" };
			foreach (var code in codes)
			{
				lotsOfOptionalCharges.OptionalCharges.Add(new ChargeViewModelSample
				{
					ChargeCode = code,
					Amount = 100,
					Currency = "EUR",
					LocalAmount = 150,
					LocalCurrency = "AUD",
					ChargeCodeDescription = "War",
					IsOptional = true,
					IsSelected = false,
					DisplayPrice = true
				});
			}

			lotsOfOptionalCharges.FreightCharges.Clear();
			lotsOfOptionalCharges.SubjectToCharges.Clear();

			vm.AddRate(lotsOfOptionalCharges);
		}

		void AddCW1(TestViewModel vm)
		{
			vm.AddRate(new CW1RateViewModelSample());
		}

		static void AssertEqualWidth(IEnumerable<Control> controls)
		{
			var firstControlWidth = controls.FirstOrDefault()?.Width;
			if (firstControlWidth.HasValue)
			{
				Assert(controls.All(c => c.Width == firstControlWidth.Value));
			}

			Assert(true);
		}
	}

	class TestForm : Form
	{
		public TestForm(TestViewModel vm)
		{
			AutoScaleMode = AutoScaleMode.None;
			ViewModel = vm;
			RatesCardControl = new RatesCardControl(ViewModel);
			RatesCardControl.AutoScaleMode = AutoScaleMode.None;
			RatesCardControl.Dock = DockStyle.Fill;

			Controls.Clear();
			Controls.Add(RatesCardControl);

			Size = ControlDpiScalingHelper.NewScaledSize(2048, 500);
		}

		public TestViewModel ViewModel { get; set; }

		public RatesCardControl RatesCardControl { get; set; }

		public List<ItemTemplateControlBase> RateCardChildren => RatesCardControl.Controls[0].Controls.OfType<ItemTemplateControlBase>().ToList();

		public TextWithValidation tbvTotalPrice => (TextWithValidation)RatesCardControl.Controls.Find("tbvTotalPrice", searchAllChildren: true).Single();

		public ZPictureBox TotalPriceValidationImage => (ZPictureBox)tbvTotalPrice.Controls.Find("pbValidationImage", searchAllChildren: true).Single();

		protected override void Dispose(bool disposing)
		{
			ViewModel?.Dispose();

			base.Dispose(disposing);
		}
	}

	class TestViewModel : SortableRatesViewModel
	{
		readonly List<RateViewModel> ratesList = new List<RateViewModel>();

		public void AddRate(RateViewModel rate)
		{
			ratesList.Add(rate);
			ReSort();
		}

		public void ClearRates()
		{
			ratesList.Clear();
			ReSort();
		}

		protected override object GetRatesSource() => ratesList;

		protected override object GetSortProperty(object o, SortOptionViewModel selectedSort)
		{
			var vm = (RateViewModel)o;
			switch (selectedSort.PropertyName)
			{
				case nameof(vm.TotalPriceAmount):
					return vm.TotalPriceAmount;
				default:
					throw new ArgumentException($"Unexpected {nameof(selectedSort.PropertyName)}: {selectedSort.PropertyName}", nameof(selectedSort));
			}
		}
	}

	class VisuallyMinimalRateViewModelSample : CargoguideRateViewModelSample
	{
		public VisuallyMinimalRateViewModelSample() : base()
		{
			IssueDate = new DateTime(1111, 3, 11);
			StartDate = new DateTime(1111, 3, 11);
			ExpiryDate = null;

			MinimiseChargesVisually(FreightCharges);
			MinimiseChargesVisually(SubjectToCharges);
		}

		void MinimiseChargesVisually(ChargesViewModel cvm)
		{
			foreach (var ch in cvm.Charges)
			{
				if (ch.LocalAmount > 0)
				{
					ch.LocalAmount = 1;
				}
			}
		}
	}
}

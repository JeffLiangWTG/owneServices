using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RefExchangeRateForm))]
	sealed class RefExchangeRateFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var exRate = Factory.New<RefExchangeRate>();
			exRate.RE_StartDate = ZDateTime.BrettsBirthday;
			exRate.RE_ExpiryDate = ZDateTime.BrettsBirthday;
			exRate.HasChanges = false;
			return new RefExchangeRateForm(exRate);
		}

		public void TestLocalClientField()
		{
			using (var exRateForm = new RefExchangeRateForm(Factory.New<RefExchangeRate>()))
			{
				exRateForm.Show();
				Assert("RE_OH_ClientGuidFindBox should not be visible", !exRateForm.RE_OH_ClientGuidFindBox.Visible);
			}
			var exRate = Factory.New<RefExchangeRate>();
			exRate.RE_OH_Client = OrgHeader.DefaultOrg.PK;
			Factory.ClearCachedValue<bool>("IsLocalClientExchangeRatefieldNeeded");
			using (var exRateForm = new RefExchangeRateForm(Factory.New<RefExchangeRate>()))
			{
				exRateForm.Show();
				Assert("RE_OH_ClientGuidFindBox should be visible", exRateForm.RE_OH_ClientGuidFindBox.Visible);
			}
		}

		public void TestFormControls()
		{
			using (var exRateForm = new RefExchangeRateForm(Factory.New<RefExchangeRate>()))
			{
				exRateForm.Show();
				CombineAssertions(() =>
				{
					AssertNotNull("RE_RX_NKExCurrencyDropDownEdit should exist", (ZCodeFindBox)exRateForm.Controls.Find("RE_RX_NKExCurrencyDropDownEdit", true).Single());
					AssertNotNull("RE_ExpiryDateDateTime should exist", (ZDateEdit)exRateForm.Controls.Find("RE_ExpiryDateDateTime", true).Single());
					AssertNotNull("RE_StartDateDateTime should exist", (ZDateEdit)exRateForm.Controls.Find("RE_StartDateDateTime", true).Single());
					AssertNotNull("RE_ExRateTypeDropEdit should exist", (ZDropEdit)exRateForm.Controls.Find("RE_ExRateTypeDropEdit", true).Single());
					AssertNotNull("RE_SellRateCalcEdit should exist", (ZCalcEdit)exRateForm.Controls.Find("RE_SellRateCalcEdit", true).Single());
				});
			}
		}

		[RequiresSTA]
		public void TestDecimalPlacesFromCountry()
		{
			var rate = RefExchangeRate.New(Factory);
			using var exRateForm = new RefExchangeRateForm(rate);
			exRateForm.Show();
			var sellRate = (ZCalcEdit)exRateForm.Controls.Find("RE_SellRateCalcEdit", true).First();
			CombineAssertions(() =>
			{
				rate.Company.SetCountry(Core.Constants.CountryCodes.Poland);

				rate.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.Indonesia;
				AssertEquals("Exchange rate should have 9 decimal places with Currency IDR in Poland", 9, sellRate.Decimals);

				rate.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.India;
				AssertEquals("Exchange rate should have 6 decimal places with Currency INR in Poland", 6, sellRate.Decimals);

				rate.Company.SetCountry(Core.Constants.CountryCodes.Iceland);
				rate.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.Indonesia;
				AssertEquals("Exchange rate should have 6 decimal places with Currency IDR not in Poland", 6, sellRate.Decimals);
			});
		}

		public void TestNumOfDecimalsByCompany()
		{
			ZBool originalReciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;

			try
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = false;

				AssertDecimalsForExchangeRateColumn(6);

				GlbCompany.CurrentCompany.GC_IsReciprocal = true;

				AssertDecimalsForExchangeRateColumn(6);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = originalReciprocal;
			}
		}

		void AssertDecimalsForExchangeRateColumn(int expectedDecimals)
		{
			using (var exRateForm = new RefExchangeRateForm(Factory.New<RefExchangeRate>()))
			{
				exRateForm.Show();
				var sellRate = (ZCalcEdit)exRateForm.Controls.Find("RE_SellRateCalcEdit", true).FirstOrDefault();
				AssertEquals("Exchange rate should have 9 decimal places", expectedDecimals, sellRate.Decimals);
			}
		}

		public void TestVisibilityOfRE_AsPublishedTextBox_RE_ExRateTypeValueChanged()
		{
			var exchangeRate = Factory.New<RefExchangeRate>();
			CombineAssertions(() =>
			{
				using (var exchangeRateForm = new RefExchangeRateForm(exchangeRate))
				{
					exchangeRateForm.Show();

					var asPublishedTextBox = exchangeRateForm.RE_AsPublishedTextBox;
					AssertEquals("BUY by default, RE_AsPublishedTextBox should not be visible", false, asPublishedTextBox.Visible);

					exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
					AssertEquals("CUS, RE_AsPublishedTextBox should be visible", true, asPublishedTextBox.Visible);

					exchangeRate.RE_ExRateType = string.Empty;
					AssertEquals("Not CUS, RE_AsPublishedTextBox should not be visible", false, asPublishedTextBox.Visible);
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
				{
					exchangeRate = Factory.New<RefExchangeRate>();
					exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary;
					using (var exchangeRateForm = new RefExchangeRateForm(exchangeRate))
					{
						exchangeRateForm.Show();
						var asPublishedTextBox = exchangeRateForm.RE_AsPublishedTextBox;
						AssertEquals("CUE for India, RE_AsPublishedTextBox should be visible", true, asPublishedTextBox.Visible);
					}
				}
			});
		}

		public void TestVisibilityOfRefExchangeRateCalculatorButton_RE_ExRateTypeValueChanged()
		{
			var exchangeRate = Factory.New<RefExchangeRate>();
			CombineAssertions(() =>
			{
				using (var exchangeRateForm = new RefExchangeRateForm(exchangeRate))
				{
					exchangeRateForm.Show();

					var refExchangeRateCalculatorButton = exchangeRateForm.RefExchangeRateCalculatorButton;
					exchangeRate.RE_ExRateType = string.Empty;
					AssertEquals("RefExchangeRateCalculatorButton should not be visible", false, refExchangeRateCalculatorButton.Visible);
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
				{
					exchangeRate = Factory.New<RefExchangeRate>();
					exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary;
					using (var exchangeRateForm = new RefExchangeRateForm(exchangeRate))
					{
						exchangeRateForm.Show();
						var refExchangeRateCalculatorButton = exchangeRateForm.RefExchangeRateCalculatorButton;
						AssertEquals("CUE for India, RefExchangeRateCalculatorButton should be visible", true, refExchangeRateCalculatorButton.Visible);
						exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
						AssertEquals("CUS for India, RefExchangeRateCalculatorButton should be visible", true, refExchangeRateCalculatorButton.Visible);
						exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.C99Rate;
						AssertEquals("Others for India, RefExchangeRateCalculatorButton should be invisible", false, refExchangeRateCalculatorButton.Visible);
					}
				}
			});
		}

#if !WINZOR
		public void TestVisibilityOfRE_AsPublishedTextBox_RE_ExRateTypeDropEditTextChanged()
		{
			var exchangeRate = Factory.New<RefExchangeRate>();
			CombineAssertions(() =>
			{
				using (var exchangeRateForm = new RefExchangeRateForm(exchangeRate))
				{
					exchangeRateForm.Show();

					var asPublishedTextBox = exchangeRateForm.RE_AsPublishedTextBox;
					AssertEquals("BUY by default, RE_AsPublishedTextBox should not be visible", false, asPublishedTextBox.Visible);

					SetRE_ExRateTypeDropEditText(Core.Constants.ExchangeRateTypes.Code.CustomsRate);
					AssertEquals("CUS, RE_AsPublishedTextBox should be visible", true, asPublishedTextBox.Visible);

					SetRE_ExRateTypeDropEditText(string.Empty);
					AssertEquals("Not CUS, RE_AsPublishedTextBox should not be visible", false, asPublishedTextBox.Visible);

					void SetRE_ExRateTypeDropEditText(string rateType)
					{
						var exRateTypeDropEdit = exchangeRateForm.RE_ExRateTypeDropEdit;
						exRateTypeDropEdit.Text = rateType;
						KeySender.PostKeyDown(exRateTypeDropEdit.CodeBox, exRateTypeDropEdit.CodeBox.Handle, Keys.Tab);
						Application.DoEvents();
					}
				}
			});
		}
#endif

		public void TestSaveAndNew()
		{
			var exRate = Factory.New<RefExchangeRate>();
			exRate.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			exRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			exRate.RE_StartDate = new ZDateTime(2021, 1, 1);
			exRate.RE_ExpiryDate = new ZDateTime(2021, 1, 2);
			exRate.RE_SellRate = 1.0;
			exRate.RE_OH_Client = OrgHeader.DefaultOrg.PK;

			using (var exRateForm = new RefExchangeRateForm(exRate))
			{
				exRateForm.Show();
				CombineAssertions(() =>
				{
					AssertEquals("before save", false, exRate.IsInDatabase);
					exRateForm.GetControl<ZToolStrip>("toolStrip").Items.Find("SaveAndNewButton", true).First().PerformClick();
					AssertEquals("after save", true, exRate.IsInDatabase);

					Application.DoEvents();
					using (var newForm = Application.OpenForms.OfType<RefExchangeRateForm>().SingleOrDefault())
					{
						var exRateNew = (RefExchangeRate)newForm.DataSource;
						AssertEquals("RE_SellRate", exRate.RE_SellRate, exRateNew.RE_SellRate);
						AssertEquals("RE_StartDate", exRate.RE_StartDate, exRateNew.RE_StartDate);
						AssertEquals("RE_ExpiryDate", exRate.RE_ExpiryDate, exRateNew.RE_ExpiryDate);
						AssertEquals("RE_ExRateType", exRate.RE_ExRateType, exRateNew.RE_ExRateType);
						AssertEquals("RE_OH_Client", exRate.RE_OH_Client, exRateNew.RE_OH_Client);
						AssertNullOrEmpty("ExCurrency", exRateNew.RE_RX_NKExCurrency);
					}
				});
			}
		}

		public void TestNew_NewFormWithDefaultValues()
		{
			var exRate = Factory.New<RefExchangeRate>();
			exRate.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			exRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			exRate.RE_StartDate = new ZDateTime(2021, 1, 1);
			exRate.RE_ExpiryDate = new ZDateTime(2021, 1, 2);
			exRate.RE_SellRate = 1.0;
			exRate.RE_OH_Client = OrgHeader.DefaultOrg.PK;
			Factory.Save();

			using (var exRateForm = new RefExchangeRateForm(exRate))
			{
				exRateForm.Show();
				CombineAssertions(() =>
				{
					(exRateForm as IPostingButtonsProvider).CommandButtonApply.PerformClick();
					Application.DoEvents();
					using (var newForm = Application.OpenForms.OfType<RefExchangeRateForm>().SingleOrDefault())
					{
						var exRateNew = (RefExchangeRate)newForm.DataSource;
						AssertEquals("RE_SellRate", exRate.RE_SellRate, exRateNew.RE_SellRate);
						AssertEquals("RE_StartDate", exRate.RE_StartDate, exRateNew.RE_StartDate);
						AssertEquals("RE_ExpiryDate", exRate.RE_ExpiryDate, exRateNew.RE_ExpiryDate);
						AssertEquals("RE_ExRateType", exRate.RE_ExRateType, exRateNew.RE_ExRateType);
						AssertEquals("RE_OH_Client", exRate.RE_OH_Client, exRateNew.RE_OH_Client);
						AssertNullOrEmpty("ExCurrency", exRateNew.RE_RX_NKExCurrency);
					}
				});
			}
		}

		public void TestSaveAndNewButton_Visibility()
		{
			var exRate = Factory.New<RefExchangeRate>();
			using (var exRateForm = new RefExchangeRateForm(exRate))
			{
				exRateForm.Show();
				CombineAssertions(() =>
				{
					var saveAndNewButton = exRateForm.GetControl<ZToolStrip>("toolStrip").Items
						.Find("SaveAndNewButton", true).First();
					AssertEquals("Should be visible when showing existing RefExchangeRate", true, saveAndNewButton.Visible);
					AssertEquals("Should be disable when showing existing RefExchangeRate", false, saveAndNewButton.Enabled);

					exRate.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.UnitedStates;
					AssertEquals("Should be visible after hasChanges", true, saveAndNewButton.Visible);
					AssertEquals("Should be enabled after hasChanges", true, saveAndNewButton.Enabled);
				});
			}
		}

		public void TestRefExchangeRateCalculatorButton_Click()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var helper = ObjectFactory.Get<Enterprise.Integration.Customs.Shared.Universal.IUniversalReferenceTestDataHelper>("Universal.IUniversalReferenceTestDataHelper", Factory);
				helper.CreateNewOrGetExistingDataGrouping("IN", "India");
				helper.CreateNewOrGetExistingCusCodeType("SDCUR", "IN Customs Standard Currency List", "IN");
				var codeList2PK = helper.CreateNewOrGetExistingCusCodeList("IN", "SDCUR", "KRW", "South Korean Won", new ZDateTime(2019, 1, 1), new ZDateTime(2079, 6, 6)).PK;
				Factory.Save();
				helper.CreateNewOrGetExistingCusCodeListAttribute(codeList2PK, "Multiplier", "100");
				Factory.Save();

				var exRate = Factory.New<RefExchangeRate>();
				exRate.Company.GC_RX_NKLocalCurrency = "INR";
				exRate.RE_SellRate = ZDecimal.Zero;
				exRate.RE_RX_NKExCurrency = ZString.Empty;
				using var exRateForm = new RefExchangeRateForm(exRate);
				exRateForm.Show();
				Assert($"RefExchangeRateCalculatorButton should not be shown when Rate Type is {exRate.RE_ExRateType}", !exRateForm.RefExchangeRateCalculatorButton.Visible);

				exRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
				Assert($"RefExchangeRateCalculatorButton should not be shown when Rate Type is {exRate.RE_ExRateType}", exRateForm.RefExchangeRateCalculatorButton.Visible);

				exRateForm.RefExchangeRateCalculatorButton.PerformClick();
				Application.DoEvents();
				AssertNull("Should not show dialog when RE_RX_NKExCurrency is empty", ZFormModaliser.LastFormShownDialogForTest);

				exRate.RE_RX_NKExCurrency = "KRW";

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormShown(x =>
				{
					if (x is CalculateExchangeRateForm calculateForm && calculateForm.BusinessEntity is RefExchangeRateCalculator calculator)
					{
						calculator.BaseCurrencyValue = 56.11111m;
						calculateForm.ButtonOK.PerformClick();
					}
				});
				exRateForm.RefExchangeRateCalculatorButton.PerformClick();
				AssertType<CalculateExchangeRateForm>("Should show Exchange Rate Calculate dialog", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Should calculate RE_SellRate when click ok button", 0.561111m, exRate.RE_SellRate);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				ZFormModaliser.SetDelegateToCallOnFormShown(x =>
				{
					if (x is CalculateExchangeRateForm calculateForm)
					{
						((RefExchangeRateCalculator)calculateForm.BusinessEntity).BaseCurrencyValue = 300m;
						calculateForm.ButtonCancel.PerformClick();
					}
				});
				exRateForm.RefExchangeRateCalculatorButton.PerformClick();
				AssertType<CalculateExchangeRateForm>("Should show Exchange Rate Calculate dialog", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Should not calculate RE_SellRate when click cancel button", 0.561111m, exRate.RE_SellRate);
			}
		}
	}
}

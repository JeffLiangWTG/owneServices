using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class CommercialInvoiceDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestInvoiceNumberTextBox()
		{
			AssertType<ZTextBox>(control.InvoiceNumberTextBox);
		}

		public void TestGroupInvoiceDropEdit()
		{
			AssertType<ZDropEdit>(control.GroupInvoiceDropEdit);
		}

		public void TestInvoiceAmountConvertToLocalCurrencyControl()
		{
			AssertType<ConvertToLocalCurrencyControl>(control.InvoiceAmountConvertToLocalCurrencyControl);
		}

		public void TestInvoiceCurrExRateCalcEdit()
		{
			AssertType<ZCalcEdit>(control.InvoiceCurrExRateCalcEdit);
		}

		public void TestIncoTermsUserControl()
		{
			AssertType<CommercialInvoiceDetailsIncoTermsUserControl>(control.IncoTermsUserControl);
		}

		public void TestIncoTermPlaceTextBox()
		{
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Control Type", control.IncoTermPlaceTextBox);
				AssertEquals("Character Casing", CharacterCasing.Normal, control.IncoTermPlaceTextBox.CharacterCasing);
			});
		}

		public void TestAdditionalTermsTextBox()
		{
			AssertType<ZTextBox>(control.AdditionalTermsTextBox);
		}

		public void TestGrossWeightCalcDropEdit()
		{
			AssertType<ZCalcDropEdit>(control.GrossWeightCalcDropEdit);
		}

		public void TestNetWeightCalcDropEdit()
		{
			AssertType<ZCalcDropEdit>(control.NetWeightCalcDropEdit);
		}

		public void TestInvoiceCurrLandedCostExRateCalcEdit()
		{
			AssertType<ZCalcEdit>(control.InvoiceCurrLandedCostExRateCalcEdit);
		}

		public void TestNoOfPacksCalcDropEdit()
		{
			AssertType<ZCalcDropEdit>(control.NoOfPacksCalcDropEdit);
		}

		public void TestInvoiceDateEdit()
		{
			AssertType<ZDateEdit>(control.InvoiceDateEdit);
		}

		public void TestValuationCodeDropEdit()
		{
			AssertType<ZDropEdit>(control.ValuationCodeDropEdit);
		}

		public void TestUCRTextBox()
		{
			AssertType<ZTextBox>(control.UCRTextBox);
		}

		public void TestCaptionRenderingEnabled()
		{
			AssertEquals(true, control.CaptionRenderingEnabled);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new CommercialInvoiceDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		CommercialInvoiceDetailsUserControl control;
	}
}

using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI.CommercialInvoice;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(InvoiceHeaderDetailsLayout))]
	sealed class InvoiceHeaderDetailsLayoutTest : LayoutsAbstractTest
	{
		public void TestControlsVisibility()
		{
			var invoiceHeader = Factory.New<BaseJobComInvoiceHeader>();

			AssertEquals("Should be visible InvoiceNumberBoundTextBox", true, Layout.IsVisible(InvoiceHeaderDetailsControlBag.Instance.InvoiceNumberBoundTextBox, invoiceHeader));
			AssertEquals("Should be visible InvoiceDateEdit", true, Layout.IsVisible(InvoiceHeaderDetailsControlBag.Instance.InvoiceDateEdit, invoiceHeader));
			AssertEquals("Should be visible InvoiceAmountCalcFindBox", true, Layout.IsVisible(InvoiceHeaderDetailsControlBag.Instance.InvoiceAmountCalcFindBox, invoiceHeader));
			AssertEquals("Should be visible InvoiceCurrExRateCalcEdit", true, Layout.IsVisible(InvoiceHeaderDetailsControlBag.Instance.InvoiceCurrExRateCalcEdit, invoiceHeader));
			AssertEquals("Should be visible IncotermAndIncotermPlaceUserControl", true, Layout.IsVisible(InvoiceHeaderDetailsControlBag.Instance.IncotermAndIncotermPlaceUserControl, invoiceHeader));
			AssertEquals("Should be visible ValuationCodeDropEdit", true, Layout.IsVisible(InvoiceHeaderDetailsControlBag.Instance.ValuationCodeDropEdit, invoiceHeader));
			AssertEquals("Should be visible GrossWeightCalcDropEdit", true, Layout.IsVisible(InvoiceHeaderDetailsControlBag.Instance.GrossWeightCalcDropEdit, invoiceHeader));
			AssertEquals("Should be visible NetWeightCalcDropEdit", true, Layout.IsVisible(InvoiceHeaderDetailsControlBag.Instance.NetWeightCalcDropEdit, invoiceHeader));
			AssertEquals("Should be visible InvoiceCurrLandedCostExRateCalcEdit", true, Layout.IsVisible(InvoiceHeaderDetailsControlBag.Instance.InvoiceCurrLandedCostExRateCalcEdit, invoiceHeader));
			AssertEquals("Should be visible NoOfPacksCalcDropEdit", true, Layout.IsVisible(InvoiceHeaderDetailsControlBag.Instance.NoOfPacksCalcDropEdit, invoiceHeader));
		}

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (InvoiceHeaderDetailsControlBag.Instance.InvoiceNumberBoundTextBox, ControlWidthClass.Auto);
				yield return (InvoiceHeaderDetailsControlBag.Instance.InvoiceDateEdit, ControlWidthClass.Auto);
				yield return (InvoiceHeaderDetailsControlBag.Instance.InvoiceAmountCalcFindBox, ControlWidthClass.Auto);
				yield return (InvoiceHeaderDetailsControlBag.Instance.InvoiceCurrExRateCalcEdit, ControlWidthClass.Auto);
				yield return (InvoiceHeaderDetailsControlBag.Instance.IncotermAndIncotermPlaceUserControl, ControlWidthClass.Auto);
				yield return (InvoiceHeaderDetailsControlBag.Instance.ValuationCodeDropEdit, ControlWidthClass.Auto);
				yield return (InvoiceHeaderDetailsControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (InvoiceHeaderDetailsControlBag.Instance.NetWeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (InvoiceHeaderDetailsControlBag.Instance.InvoiceCurrLandedCostExRateCalcEdit, ControlWidthClass.Auto);
				yield return (InvoiceHeaderDetailsControlBag.Instance.NoOfPacksCalcDropEdit, ControlWidthClass.Auto);
			}
		}

		protected override int ControlBagCount => 2;

		PanelLayout Layout => layout ?? (layout = ((IPanelLayoutProvider)new InvoiceHeaderDetailsLayout()).Layout);

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new CommercialInvoiceDetailsLayoutBuilder<BaseJobComInvoiceHeader>();

		PanelLayout layout;
	}
}

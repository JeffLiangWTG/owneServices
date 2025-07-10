using System.Collections.Generic;
using Enterprise.Customs.GUI;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(ImportInvoiceDetailsLayout))]
sealed class ImportInvoiceDetailsLayoutTest : LayoutsAbstractTest
{
	public void TestBehaviour()
	{
		CombineAssertions(() =>
		{
			var layout = GetPanelLayout();
			AssertEquals("ValuationCodeDropEdit", true, layout.HasBehaviourByBehaviourType(CommercialInvoiceDetailsControlBag.Instance.ValuationCodeDropEdit, typeof(ZDropEditSizeBehaviour)));
			AssertEquals("ValuationMethodDropEdit", true, layout.HasBehaviourByBehaviourType(ImportInvoiceDetailsControlBag.Instance.ValuationMethodDropEdit, typeof(ZDropEditSizeBehaviour)));
			AssertEquals("IncoTermsUserControl", true, layout.HasBehaviourByBehaviourType(CommercialInvoiceDetailsControlBag.Instance.IncoTermsUserControl, typeof(ZUserControlAllZDropEditSizeBehaviour)));
		});
	}

	public void TestVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		var invoiceHeader = declaration.Invoices.AddNew();
		var layout = GetPanelLayout();
		CombineAssertions(() =>
		{
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
			AssertEquals("Invisible", false, layout.IsVisible(EU.GUI.InvoiceDetailsControlBag.Instance.AgreedPlaceCodeFindBox, invoiceHeader));
			invoiceHeader.JZ_IncoTerm = "ABC";
			AssertEquals("Visible", true, layout.IsVisible(EU.GUI.InvoiceDetailsControlBag.Instance.AgreedPlaceCodeFindBox, invoiceHeader));
		});
	}

	public void TestValuationCodeDropEdit_Captions()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		var commonBag = new CommercialInvoiceDetailsLayoutBuilder<JobComInvoiceHeader>().CommonBag;

		var layout = ((IPanelLayoutProvider)new ImportInvoiceDetailsLayout()).Layout;
		layout.TryGetCaption(commonBag.ValuationCodeDropEdit, invoiceHeader, out var resourceStringData);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "[8/5] Transaction Nature", resourceStringData.Caption);
			AssertEquals("ShortCaption", "Tran. Nature", resourceStringData.ShortCaption);
			AssertEquals("MediumCaption", "[8/5] Tran. Nature", resourceStringData.MediumCaption);
			AssertEquals("FullDescription", "The nature of the transaction.", resourceStringData.FullDescription);
		});
	}

	protected override int ControlBagCount => 3;

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.InvoiceNumberTextBox, ControlWidthClass.Auto);
			yield return (Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.InvoiceDateEdit, ControlWidthClass.Auto);
			yield return (Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.InvoiceAmountConvertToLocalCurrencyControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.InvoiceCurrExRateCalcEdit, ControlWidthClass.Auto);
			yield return (Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.IncoTermsUserControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.IncoTermPlaceTextBox, ControlWidthClass.Auto);
			yield return (EU.GUI.InvoiceDetailsControlBag.Instance.AgreedPlaceCodeFindBox, ControlWidthClass.Auto);
			yield return (PL.GUI.ImportInvoiceDetailsControlBag.Instance.ValuationMethodDropEdit, ControlWidthClass.Auto);
			yield return (Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.ValuationCodeDropEdit, ControlWidthClass.Auto);
			yield return (PL.GUI.ImportInvoiceDetailsControlBag.Instance.TranCircumstanceUserControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
			yield return (Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.NetWeightCalcDropEdit, ControlWidthClass.Auto);
			yield return (Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.InvoiceCurrLandedCostExRateCalcEdit, ControlWidthClass.Auto);
			yield return (Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.NoOfPacksCalcDropEdit, ControlWidthClass.Auto);
		}
	}

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new CommercialInvoiceDetailsLayoutBuilder<JobComInvoiceHeader>();

	static PanelLayout GetPanelLayout() => ((IPanelLayoutProvider)new ImportInvoiceDetailsLayout()).Layout;
}

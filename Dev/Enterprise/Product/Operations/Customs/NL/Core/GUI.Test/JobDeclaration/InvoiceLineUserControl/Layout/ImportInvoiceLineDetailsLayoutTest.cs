using System.Collections.Generic;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(ImportInvoiceLineDetailsLayout))]
sealed class ImportInvoiceLineDetailsLayoutTest : LayoutsAbstractTest
{
	protected override int ControlBagCount => 2;

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
			yield return ThirdColumnControls;
		}
	}

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new CommonInvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>();

	public void TestDestinationUsingZZRefCusCodeListCodeFindBox_Visibility()
	{
		var layout = ((IPanelLayoutProvider)new ImportInvoiceLineDetailsLayout()).Layout;
		var invoiceLineNoDec = Factory.New<JobComInvoiceLine>();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLineWithDec = invoiceHeader.InvoiceLines.AddNew();
		CombineAssertions(() =>
		{
			AssertEquals("Visible - No Declaration", true, layout.IsVisible(EU.GUI.InvoiceLineDetailsControlBag.Instance.DestinationUsingZZRefCusCodeListCodeFindBox, invoiceLineNoDec));
			AssertEquals("Visible - Declaration", true, layout.IsVisible(EU.GUI.InvoiceLineDetailsControlBag.Instance.DestinationUsingZZRefCusCodeListCodeFindBox, invoiceLineWithDec));
		});
	}

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (CommonInvoiceLineDetailsControlBag.Instance.EntryInstructionGuidDropEdit, ControlWidthClass.Long);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.FormattedProcedureCodeFindBox, ControlWidthClass.Long);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.AdditionalProcedureCodesUserControl, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.WithDescriptionTariffFindBox, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.DescriptionLongTextControl, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CountryOfOriginDropEdit, ControlWidthClass.Long);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.CountryOfSupplyCodeFindBox, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.WithDescriptionPrimaryPreferenceDropEdit, ControlWidthClass.Long);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.DestinationUsingZZRefCusCodeListCodeFindBox, ControlWidthClass.Long);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.DispatchCodeFindBox, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.ValuationCodeDropEdit, ControlWidthClass.Auto);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.ValuationAdjustmentPercentageCalcEdit, ControlWidthClass.Auto);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (CommonInvoiceLineDetailsControlBag.Instance.InvoiceQuantityCalcDropEdit, ControlWidthClass.Medium);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsQuantityCalcDropEdit, ControlWidthClass.Medium);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Medium);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsThirdQuantityCalcDropEdit, ControlWidthClass.Medium);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.LinePriceCurrencyCalcFindBox, ControlWidthClass.Medium);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.TaxTypeDropEdit, ControlWidthClass.Medium);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.SupplementaryCode1DropEdit, ControlWidthClass.Medium);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.SupplementaryCode2DropEdit, ControlWidthClass.Medium);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.AdditionalSupplementaryCodesAndGDMUserControl, ControlWidthClass.Long);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
	{
		get
		{
			yield return (CommonInvoiceLineDetailsControlBag.Instance.WeightCalcDropEdit, ControlWidthClass.Medium);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.NetWeightCalcDropEdit, ControlWidthClass.Medium);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.VolumeCalcDropEdit, ControlWidthClass.Medium);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CommodityCodeFindBox, ControlWidthClass.Medium);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.QuotaWithCheckLinkUserControl, ControlWidthClass.Auto);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.TransactionNatureDropEdit, ControlWidthClass.Long);
		}
	}
}

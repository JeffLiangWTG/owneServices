using System.Collections.Generic;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(ExportInvoiceLineDetailsLayout))]
sealed class ExportInvoiceLineDetailsLayoutTest : LayoutsAbstractTest
{
	protected override int ControlBagCount => 3;

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

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (CommonInvoiceLineDetailsControlBag.Instance.EntryInstructionGuidDropEdit, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.PartNoCodeFindBox, ControlWidthClass.Auto);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.FormattedProcedureCodeFindBox, ControlWidthClass.Long);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.AdditionalProcedureCodesUserControl, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.WithDescriptionTariffFindBox, ControlWidthClass.Long);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.CusNumberCodeFindBox, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.DescriptionLongTextControl, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CountryOfOriginDropEdit, ControlWidthClass.Long);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.DestinationUsingZZRefCusCodeListCodeFindBox, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CountryOfExportCodeFindBox, ControlWidthClass.Long);
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
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.SupplementaryCode1DropEdit, ControlWidthClass.Medium);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.SupplementaryCode2DropEdit, ControlWidthClass.Medium);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.AdditionalSupplementaryCodesAndGDMUserControl, ControlWidthClass.Long);
			yield return (InvoiceLineDetailsControlBag.Instance.ECCNCodesUserControl, ControlWidthClass.Long);
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
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.TransactionNatureDropEdit, ControlWidthClass.Long);
		}
	}
}

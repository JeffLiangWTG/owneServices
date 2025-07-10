using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(InvoiceLineDetailsLayout))]
sealed class InvoiceLineDetailsLayoutTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
		}
	}

	protected override int ControlBagCount => 2;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new CommonInvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>();

	protected override bool ShouldAssertThatIncludedControlsAreSorted => true;

	static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (CommonInvoiceLineDetailsControlBag.Instance.EntryInstructionGuidDropEdit, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.PartNoCodeFindBox, ControlWidthClass.Medium);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.DescriptionLongTextControl, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CountryOfOriginCodeFindBox, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.StateOrRegionOfOriginDropEdit, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.TariffFindBox, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.PrimaryPreferenceDropEdit, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.LinePriceCurrencyCalcFindBox, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.TaxTypeDropEdit, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.WeightCalcDropEdit, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.NetWeightCalcDropEdit, ControlWidthClass.Auto);
			yield return (InvoiceLineDetailsControlBag.Instance.GoodsMarksLongTextControl, ControlWidthClass.Auto);
			yield return (InvoiceLineDetailsControlBag.Instance.ProcedureCodeDropEdit, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.ValuationCodeDropEdit, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CommodityCodeFindBox, ControlWidthClass.Auto);
		}
	}

	static IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (CommonInvoiceLineDetailsControlBag.Instance.InvoiceQuantityCalcDropEdit, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsQuantityCalcDropEdit, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsThirdQuantityCalcDropEdit, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsFourthQuantityCalcDropEdit, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsFifthQuantityCalcDropEdit, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.VolumeCalcDropEdit, ControlWidthClass.Auto);
			yield return (InvoiceLineDetailsControlBag.Instance.PackageTypeDropEdit, ControlWidthClass.Auto);
			yield return (InvoiceLineDetailsControlBag.Instance.SupplementaryCode1DropEdit, ControlWidthClass.Auto);
			yield return (InvoiceLineDetailsControlBag.Instance.SupplementaryCode2DropEdit, ControlWidthClass.Auto);
			yield return (InvoiceLineDetailsControlBag.Instance.AdditionalSupplementaryCodesUserControl, ControlWidthClass.Auto);
			yield return (InvoiceLineDetailsControlBag.Instance.ReducedCustomsFlagDropEdit, ControlWidthClass.Auto);
			yield return (InvoiceLineDetailsControlBag.Instance.CustomsRateOverrideUserControl, ControlWidthClass.Long);
			yield return (InvoiceLineDetailsControlBag.Instance.RtRateOverrideCalcEdit, ControlWidthClass.Auto);
			yield return (InvoiceLineDetailsControlBag.Instance.MergeOverrideTextBox, ControlWidthClass.Long);
		}
	}

	public void TestInvoiceLineDetailsFieldsVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("CountyOfOrigin visible - Export", expected: true, LayoutForTesting.IsVisible(CommonInvoiceLineDetailsControlBag.Instance.StateOrRegionOfOriginDropEdit, invoiceLine));
			AssertEquals("VAT NOT visible - Export", expected: false, LayoutForTesting.IsVisible(CommonInvoiceLineDetailsControlBag.Instance.TaxTypeDropEdit, invoiceLine));
			AssertEquals("Reduced custom NOT visible - Export", expected: false, LayoutForTesting.IsVisible(InvoiceLineDetailsControlBag.Instance.ReducedCustomsFlagDropEdit, invoiceLine));
			AssertEquals("Custom rate NOT visible - Export", expected: false, LayoutForTesting.IsVisible(InvoiceLineDetailsControlBag.Instance.CustomsRateOverrideUserControl, invoiceLine));
			AssertEquals("RTO rate NOT visible - Export", expected: false, LayoutForTesting.IsVisible(InvoiceLineDetailsControlBag.Instance.RtRateOverrideCalcEdit, invoiceLine));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("CountyOfOrigin NOT visible - Import", expected: false, LayoutForTesting.IsVisible(CommonInvoiceLineDetailsControlBag.Instance.StateOrRegionOfOriginDropEdit, invoiceLine));
			AssertEquals("VAT visible - Import", expected: true, LayoutForTesting.IsVisible(CommonInvoiceLineDetailsControlBag.Instance.TaxTypeDropEdit, invoiceLine));
			AssertEquals("Reduced custom visible - Import", expected: true, LayoutForTesting.IsVisible(InvoiceLineDetailsControlBag.Instance.ReducedCustomsFlagDropEdit, invoiceLine));
			AssertEquals("Custom rate visible - Import", expected: true, LayoutForTesting.IsVisible(InvoiceLineDetailsControlBag.Instance.CustomsRateOverrideUserControl, invoiceLine));
			AssertEquals("RTO rate visible - Import", expected: true, LayoutForTesting.IsVisible(InvoiceLineDetailsControlBag.Instance.RtRateOverrideCalcEdit, invoiceLine));
		});
	}

	public void TestPackageTypeDropEditVisibility()
	{
		var invoiceLine = Factory.New<JobComInvoiceLineForTesting>();
		CombineAssertions(() =>
		{
			invoiceLine.PackageTypeVisible = false;
			invoiceLine.JI_Tariff = "11111111";
			AssertEquals("when PackageTypeVisible is false", expected: false, LayoutForTesting.IsVisible(InvoiceLineDetailsControlBag.Instance.PackageTypeDropEdit, invoiceLine));

			invoiceLine.PackageTypeVisible = true;
			invoiceLine.JI_Tariff = "22222222";
			AssertEquals("when PackageTypeVisible is true", expected: true, LayoutForTesting.IsVisible(InvoiceLineDetailsControlBag.Instance.PackageTypeDropEdit, invoiceLine));
		});
	}

	public void TestSetDateForDutyRateBehaviour() => CombineAssertions(() =>
	{
		var behaviours = LayoutForTesting.GetControlBehaviours(CommonInvoiceLineDetailsControlBag.Instance.TariffFindBox);
		AssertEquals("Number of Behaviours", 1, behaviours.Count);
		var behaviourContainer = behaviours.First().Value;
		AssertNotNull("Behaviour Container", behaviourContainer);
		AssertNotNull("Behaviour", behaviourContainer.ControlBehaviour);
		var setDateForDutyRateBehaviour = behaviourContainer.ControlBehaviour;
		AssertContains("Behavior Type", "InternalCustomizableControlBehaviour", setDateForDutyRateBehaviour.GetType().Name);

		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
		using var control = new TariffFindBox();

		AssertEquals("DateForDutyRate uses the invoice line's EffectiveDateForDutyAndRate", invoiceLine?.EffectiveDateForDutyAndRate, control.TariffInfo.DateForDutyRate);
	});

	class JobComInvoiceLineForTesting : JobComInvoiceLine
	{
		public JobComInvoiceLineForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public bool PackageTypeVisible { get; set; }

		protected override bool GetNO_PackageTypeVisibility() => PackageTypeVisible;
	}
}

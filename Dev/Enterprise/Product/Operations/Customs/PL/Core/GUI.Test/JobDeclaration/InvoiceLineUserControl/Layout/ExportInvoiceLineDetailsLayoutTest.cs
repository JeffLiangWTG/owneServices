using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(ExportInvoiceLineDetailsLayout))]
sealed class ExportInvoiceLineDetailsLayoutTest : LayoutsAbstractTest
{
	public void TestCustomsSecondQuantityCaption()
	{
		SetupTariffData();

		CombineAssertions(() =>
		{
			invoiceLine.JI_Tariff = "44444444";
			layout.TryGetCaption(CommonInvoiceLineDetailsControlBag.Instance.CustomsSecondQuantityCalcDropEdit, invoiceLine, out var resourceStringDataCU1);
			AssertEquals("Caption :", "Additional Qty 1", resourceStringDataCU1.Caption);

			invoiceLine.JI_Tariff = "55555555";
			invoiceLine.JI_CustomsUnitQty = "KGM";
			layout.TryGetCaption(CommonInvoiceLineDetailsControlBag.Instance.CustomsSecondQuantityCalcDropEdit, invoiceLine, out var resourceStringDataCU2);
			AssertEquals("Caption :", "[41] Supp. Qty", resourceStringDataCU2.Caption);
		});
	}

	void SetupTariffData()
	{
		var groupingCode = Core.Constants.CountryCodes.Poland;
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var tariffTypeExport = helper.CreateTariffType(groupingCode, TariffTypes.Export);
		Factory.Save();

		var tariffNotCU2 = helper.CreateTariff(groupingCode, tariffTypeExport.PK, "44444444", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateTariffUOM(tariffNotCU2, UnitOfMeasureTypes.CustomsUOM3Type, "CU1");

		var tariffCU2 = helper.CreateTariff(groupingCode, tariffTypeExport.PK, "55555555", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateTariffUOM(tariffCU2, UnitOfMeasureTypes.AdditionalUOMType, "CU2");

		Factory.Save();
	}

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
		}
	}
	static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (CommonInvoiceLineDetailsControlBag.Instance.UnicodeDescriptionLongTextControl, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.DescriptionLongTextControl, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.InvoiceQuantityCalcDropEdit, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CountryOfOriginDropEdit, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.TariffFindBox, ControlWidthClass.Auto);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.SupplementaryCode1DropEdit, ControlWidthClass.Medium);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.SupplementaryCode2DropEdit, ControlWidthClass.Medium);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.AdditionalSupplementaryCodesAndGDMUserControl, ControlWidthClass.Long);
			yield return (InvoiceLineDetailsControlBag.Instance.CPCUserControl, ControlWidthClass.Auto);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.AdditionalProcedureCodesUserControl, ControlWidthClass.Long);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.CusNumberCodeFindBox, ControlWidthClass.Auto);
		}
	}

	static IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (CommonInvoiceLineDetailsControlBag.Instance.LinePriceCurrencyCalcFindBox, ControlWidthClass.Auto);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.DestinationCodeFindBox, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.EntryInstructionGuidDropEdit, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.WeightCalcDropEdit, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsQuantityCalcDropEdit, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsThirdQuantityCalcDropEdit, ControlWidthClass.Auto);
		}
	}

	protected override int ControlBagCount => 3;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new InvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>();

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		layout = ((IPanelLayoutProvider)new ImportInvoiceLineDetailsLayout()).Layout;
	}

	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine;
	PanelLayout layout;
}

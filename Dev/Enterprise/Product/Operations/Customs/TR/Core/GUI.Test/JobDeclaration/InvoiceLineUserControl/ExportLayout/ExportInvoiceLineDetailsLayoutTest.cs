using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.GUI;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.GUI.Testing
{
	[TestedType(typeof(ExportInvoiceLineDetailsLayout))]
	sealed class ExportInvoiceLineDetailsLayoutTest : LayoutsAbstractTest
	{
		#region Visibility
		public void TestVisibility()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateRefCusProcedure("TR", "", "22", "11", "", "des", "EXP");
			procedure.ZZ6_OutOfInwardProcessing = "Y";

			var refCusProcedure3151 = helper.CreateRefCusProcedure("TR", ZString.Empty, "31", "51", ZString.Empty, ZString.Empty, "EXP");
			var refCusProcedure5300 = helper.CreateRefCusProcedure("TR", ZString.Empty, "53", "00", ZString.Empty, ZString.Empty, "IMP");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			invoiceLine.JI_Procedure = "2211";

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_Procedure = "3151";

			CombineAssertions("Below assertions are for the visibilities with true!", () =>
			{
				AssertEquals("Visible when IsPreviousEntryAvailable is true", true, Layout.IsVisible(CommonInvoiceLineDetailsControlBag.Instance.PreviousEntryNumberTextBox, invoiceLine));
				AssertEquals("Visible when IsPreviousEntryAvailable is true", true, Layout.IsVisible(CommonInvoiceLineDetailsControlBag.Instance.PreviousEntryLineNumberCalcEdit, invoiceLine));
				AssertEquals("Visible when IsPreviousEntryAvailable is true", true, Layout.IsVisible(EU.GUI.InvoiceLineDetailsControlBag.Instance.ProcessingDescriptionLongTextControl, invoiceLine));
			});

			invoiceLine.JI_Procedure = "";
			invoiceLine2.JI_Procedure = "1111";

			CombineAssertions("Below assertions are for the visibilities with false!", () =>
			{
				AssertEquals("Invisible when IsPreviousEntryAvailable is false", false, Layout.IsVisible(CommonInvoiceLineDetailsControlBag.Instance.PreviousEntryNumberTextBox, invoiceLine));
				AssertEquals("Invisible when IsPreviousEntryAvailable is false", false, Layout.IsVisible(CommonInvoiceLineDetailsControlBag.Instance.PreviousEntryLineNumberCalcEdit, invoiceLine));
				AssertEquals("Invisible when IsPreviousEntryAvailable is false", false, Layout.IsVisible(EU.GUI.InvoiceLineDetailsControlBag.Instance.ProcessingDescriptionLongTextControl, invoiceLine));
			});
		}

		public void TestVisibilityDependencies()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();

			CombineAssertions("Below assertions are for the visibilities with dependencies!", () =>
			{
				AssertEquals(invoiceLine.JI_FormattedProcedureInfo, Layout.GetVisibilityDependencies(CommonInvoiceLineDetailsControlBag.Instance.PreviousEntryNumberTextBox, invoiceLine).First());
				AssertEquals(invoiceLine.JI_FormattedProcedureInfo, Layout.GetVisibilityDependencies(CommonInvoiceLineDetailsControlBag.Instance.PreviousEntryLineNumberCalcEdit, invoiceLine).First());
				AssertEquals(invoiceLine.JI_FormattedProcedureInfo, Layout.GetVisibilityDependencies(EU.GUI.InvoiceLineDetailsControlBag.Instance.ProcessingDescriptionLongTextControl, invoiceLine).First());
			});
		}
		#endregion

		public void TestEntryExitPurposeCodeCaptionsDependingMessageType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.MiscellaneousCustoms;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			CombineAssertions("Below assertions are for type related captions!", () =>
			{
				Layout.TryGetCaption(EU.GUI.InvoiceLineDetailsControlBag.Instance.EntryExitPurposeCodeDropEdit, invoiceLine, out var resourceStringDataForMisc);
				AssertEquals("Entry Purpose Code", resourceStringDataForMisc.Caption);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;

				Layout.TryGetCaption(EU.GUI.InvoiceLineDetailsControlBag.Instance.EntryExitPurposeCodeDropEdit, invoiceLine, out var resourceStringDataForExport);
				AssertEquals("Exit Purpose Code", resourceStringDataForExport.Caption);
			});
		}

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

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (InvoiceLineDetailsControlBag.Instance.PartNoCodeFindBox, ControlWidthClass.Long);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.FormattedProcedureCodeFindBox, ControlWidthClass.Long);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.FormattedWithDescriptionTariffFindBox, ControlWidthClass.Long);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.SupplementaryCode1AndGDMUserControl, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.SupplementaryCode2DropEdit, ControlWidthClass.Long);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.AdditionalSupplementaryCodesUserControl, ControlWidthClass.Long);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.CommercialPaymentCodeDropEdit, ControlWidthClass.Long);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.DescriptionLongTextControl, ControlWidthClass.Long);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.UnicodeDescriptionLongTextControl, ControlWidthClass.Long);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CountryOfOriginDropEdit, ControlWidthClass.Long);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.CusNumberCodeFindBox, ControlWidthClass.Auto);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.ValuationCodeFindBox, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonInvoiceLineDetailsControlBag.Instance.ManufacturerAddressControl, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsQuantityCalcDropEdit, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.LinePriceCurrencyCalcFindBox, ControlWidthClass.Auto);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.PriceTypeDropEdit, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.NetWeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.WeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.InwardProcessingLicenseLineNumberTextBox, ControlWidthClass.Long);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.ReturningGoodsReasonCodeDropEdit, ControlWidthClass.Long);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.ReturningGoodsReasonDetailTextBox, ControlWidthClass.Long);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.EntryExitPurposeCodeDropEdit, ControlWidthClass.Long);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.EntryExitPurposeDetailTextBox, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (InvoiceLineDetailsControlBag.Instance.BrandNameTextBox, ControlWidthClass.Medium);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.BorderTradeStateCodeFindBox, ControlWidthClass.Medium);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.ExportUnionAdditionalTariffCodeFindBox, ControlWidthClass.Medium);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.ExportUnionDeferredInstallmentTextBox, ControlWidthClass.Medium);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.ExportUnionEcologicalCheckBox, ControlWidthClass.Medium);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.PreviousEntryNumberTextBox, ControlWidthClass.Medium);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.PreviousEntryLineNumberCalcEdit, ControlWidthClass.Medium);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.ProcessingDescriptionLongTextControl, ControlWidthClass.Medium);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.ExportUnionPackCodeFindBox, ControlWidthClass.Long);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.ExportUnionThreadCodeFindBox, ControlWidthClass.Long);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.ExportUnionProductionYearCalcEdit, ControlWidthClass.Auto);
			}
		}

		public PanelLayout Layout => layout ?? (layout = ((IPanelLayoutProvider)new ExportInvoiceLineDetailsLayout()).Layout);

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ExportInvoiceLineDetailsLayoutBuilder();

		PanelLayout layout;
	}
}

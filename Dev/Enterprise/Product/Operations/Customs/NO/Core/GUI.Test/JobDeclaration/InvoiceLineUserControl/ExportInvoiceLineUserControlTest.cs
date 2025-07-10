using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(ExportInvoiceLineUserControl))]
sealed class ExportInvoiceLineUserControlTest : BaseInvoiceLineUserControlTest<ExportInvoiceLineUserControl>
{
	public void TestInvoiceLineDetails_CommonDetailsAvailable()
	{
		using var invLinesForm = new ExportInvoiceLineUserControlForTest();
		AssertNotNull(invLinesForm.LayoutIncludedControls);
		AssertNotEquals(0, invLinesForm.LayoutIncludedControls.Count);
	}

	public void TestSupportingDocumentTab()
	{
		using var control = new ExportInvoiceLineUserControlForTest();
		Assert("SupportingDocumentTab should be visible", control.FindSingle<ZTabPage>("SupportingDocumentsTabPage").TabVisible);
	}

	public void TestInvoiceLineDetailsPanelLayout()
	{
		using var invLinesForm = new ExportInvoiceLineUserControlForTest();
		var layout = invLinesForm.GetNewInvoiceLineDetailsPanelLayout_Exposed().Layout;
		var definedLayout = ((IPanelLayoutProvider)new InvoiceLineDetailsLayout()).Layout;
		AssertContainsExactElementsInAnyOrder("Included Controls", layout.IncludedControls, definedLayout.IncludedControls);
	}

	protected override ZString GetJE_MessageType() => Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;

	protected override IEnumerable<ColumnProperties> ExpectedInvoiceLineGrid_DefaultColumns
	{
		get
		{
			yield return JobComInvoiceLine.Schema.JI_LineNo;
			yield return JobComInvoiceLine.Schema.MergedLineNumber;
			yield return JobComInvoiceLine.Schema.JI_Calc_Invoice;
			yield return JobComInvoiceLine.Schema.JI_CEI;
			yield return JobComInvoiceLine.Schema.EntryInstructionDescription;
			yield return JobComInvoiceLine.Schema.JI_PartNo;
			yield return JobComInvoiceLine.Schema.JI_Description;
			yield return JobComInvoiceLine.Schema.JI_CountryOfOrigin;
			yield return JobComInvoiceLine.Schema.JI_StateOrRegionOfOrigin;
			yield return JobComInvoiceLine.Schema.JI_Tariff;
			yield return JobComInvoiceLine.Schema.JI_PrimaryPreference;
			yield return JobComInvoiceLine.Schema.JI_LinePrice;
			yield return JobComInvoiceLine.Schema.JI_RX_NKLinePriceCurr;
			yield return JobComInvoiceLine.Schema.JI_Weight;
			yield return JobComInvoiceLine.Schema.JI_WeightUQ;
			yield return JobComInvoiceLine.Schema.JI_NetWeight;
			yield return JobComInvoiceLine.Schema.JI_NetWeightUQ;
			yield return JobComInvoiceLine.Schema.JI_Procedure;
			yield return JobComInvoiceLine.Schema.JI_InvoiceQuantity;
			yield return JobComInvoiceLine.Schema.JI_InvoiceUQ;
			yield return JobComInvoiceLine.Schema.JI_CustomsQuantity;
			yield return JobComInvoiceLine.Schema.JI_CustomsUnitQty;
		}
	}

	protected override IEnumerable<ColumnProperties> ExpectedInvoiceLineGrid_AllColumns
	{
		get
		{
			yield return JobComInvoiceLine.Schema.JI_LineNo;
			yield return JobComInvoiceLine.Schema.MergedLineNumber;
			yield return JobComInvoiceLine.Schema.JI_Calc_Invoice;
			yield return JobComInvoiceLine.Schema.JI_CEI;
			yield return JobComInvoiceLine.Schema.EntryInstructionDescription;
			yield return JobComInvoiceLine.Schema.JI_PartNo;
			yield return JobComInvoiceLine.Schema.JI_Description;
			yield return JobComInvoiceLine.Schema.JI_CountryOfOrigin;
			yield return JobComInvoiceLine.Schema.JI_StateOrRegionOfOrigin;
			yield return JobComInvoiceLine.Schema.JI_Tariff;
			yield return JobComInvoiceLine.Schema.JI_InvoiceQuantity;
			yield return JobComInvoiceLine.Schema.JI_InvoiceUQ;
			yield return JobComInvoiceLine.Schema.JI_CustomsQuantity;
			yield return JobComInvoiceLine.Schema.JI_CustomsUnitQty;
			yield return JobComInvoiceLine.Schema.JI_LinePrice;
			yield return JobComInvoiceLine.Schema.JI_RX_NKLinePriceCurr;
			yield return JobComInvoiceLine.Schema.JI_Weight;
			yield return JobComInvoiceLine.Schema.JI_WeightUQ;
			yield return JobComInvoiceLine.Schema.JI_NetWeight;
			yield return JobComInvoiceLine.Schema.JI_NetWeightUQ;
			yield return JobComInvoiceLine.Schema.JI_Procedure;
			yield return JobComInvoiceLine.Schema.JI_PrimaryPreference;
			yield return JobComInvoiceLine.Schema.JI_ValuationCode;
			yield return JobComInvoiceLine.Schema.JI_CC;
			yield return JobComInvoiceLine.Schema.JI_RH_NKCommodity_Code;
			yield return JobComInvoiceLine.Schema.JI_ContainerMode;
			yield return JobComInvoiceLine.Schema.JI_CustomAttrib1;
			yield return JobComInvoiceLine.Schema.JI_CustomAttrib2;
			yield return JobComInvoiceLine.Schema.JI_CustomAttrib3;
			yield return JobComInvoiceLine.Schema.JI_CustomAttrib4;
			yield return JobComInvoiceLine.Schema.JI_CustomAttrib5;
			yield return JobComInvoiceLine.Schema.JI_CustomAttrib6;
			yield return JobComInvoiceLine.Schema.JI_CustomTextBlob1;
			yield return JobComInvoiceLine.Schema.JI_MatchingKey;
			yield return JobComInvoiceLine.Schema.JI_Calc_OrderLineNumberAndSubLine;
			yield return JobComInvoiceLine.Schema.JI_OrderNumber;
			yield return JobComInvoiceLine.Schema.JI_PartAttrib1;
			yield return JobComInvoiceLine.Schema.JI_PartAttrib2;
			yield return JobComInvoiceLine.Schema.JI_PartAttrib3;
			yield return JobComInvoiceLine.Schema.JI_SerialNumber;
			yield return JobComInvoiceLine.Schema.UnitPrice;
			yield return JobComInvoiceLine.Schema.JI_ClassUsageComment;
			yield return JobComInvoiceLine.Schema.JI_GS_NKClassUsageCommentReviewer;
			yield return JobComInvoiceLine.Schema.JI_IsClassUsageCommentRead;
			yield return JobComInvoiceLine.Schema.JI_Volume;
			yield return JobComInvoiceLine.Schema.JI_VolumeUQ;
			yield return "InvoiceHeader+JZ_InvoiceDisplaySequence";
		}
	}
}

class ExportInvoiceLineUserControlForTest : ExportInvoiceLineUserControl
{
	public IReadOnlyCollection<ControlReference> LayoutIncludedControls { get; set; }

	public IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout_Exposed() => base.GetNewInvoiceLineDetailsPanelLayout();

	protected override IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout()
	{
		var layout = base.GetNewInvoiceLineDetailsPanelLayout();
		LayoutIncludedControls = layout.Layout.IncludedControls;
		return layout;
	}
}

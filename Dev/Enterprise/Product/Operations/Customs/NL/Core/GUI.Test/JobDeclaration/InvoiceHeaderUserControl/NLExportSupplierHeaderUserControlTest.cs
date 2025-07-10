using System.Collections.Generic;
using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.GUI.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(NLExportSupplierHeaderUserControl))]
sealed class NLExportSupplierHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<NLExportSupplierHeaderUserControl, JobDeclaration>
{
	public void TestSupportingDocumentsTabPageCaptionAndUserControlType()
	{
		EUDynamicControlTestHelper.AssertEUSupplierHeaderUserControlTabPageCaptionAndUserControlType<NLExportSupplierHeaderUserControl>(Factory.New<JobDeclaration>(), "SupportingDocumentsTabPage", "SupportingDocumentsUserControl", "Supporting Documents", typeof(NLSupportingDocumentsUserControl));
	}

	public void TestAdditionalInfoTabPageCaptionAndUserControlType()
	{
		EUDynamicControlTestHelper.AssertEUSupplierHeaderUserControlTabPageCaptionAndUserControlType<NLExportSupplierHeaderUserControl>(Factory.New<JobDeclaration>(), "AdditionalInfoTabPage", "additionalInfosUserControl1", "[44] Additional Documents", typeof(InvoiceLineAdditionalInfosUserControlWithGrid));
	}

	public void TestColumnsAdded()
	{
		using (var control = new NLExportSupplierHeaderUserControl())
		{
			control.InitializeGridLayout();
			control.Show();
			var columns = control.JobComInvoiceHeadersBoundGrid.ColumnStyles.OfType<ZGridColumnInfo>().Select(x => x.ColumnName);
			AssertCollectionContains("JZ_UCR", columns);
		}
	}

	public void TestAgreedPlaceControlsVisibility()
	{
		var declaration = GetDeclaration();
		declaration.Invoices.AddNew();
		using (var form = new ZForm(declaration))
		{
			using (var userControl = new NLExportSupplierHeaderUserControl())
			{
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				var codeFindBox = userControl.FindSingle<ZCodeFindBox>("AgreedPlaceCodeFindBox");
				AssertEquals("AgreedPlaceCodeFindBox visibility", true, codeFindBox.Visible);
			}
		}
	}

	protected override IEnumerable<string> ExpectedControlList => DefaultControlList.Except(new[] { "GroupInvoiceDropEdit" }).Union(new[] { "AgreedPlaceCodeFindBox", "TransportChargesMethodOfPaymentDropEdit" });

	JobDeclaration GetDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.Invoices.AddNew();
		return declaration;
	}
}

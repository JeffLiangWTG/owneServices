using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.PL.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(ExportSupplierHeaderUserControl))]
sealed class ExportSupplierHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<ExportSupplierHeaderUserControl, JobDeclaration>
{
	public void TestGetAdditionalInfosUserControlType()
	{
		using (var control = new ExportSupplierHeaderUserControlTestForTest())
		{
			AssertEquals(typeof(AdditionalInfosUserControlWithGrid), control.GetAdditionalInfosUserControlType_Exposed());
		}
	}

	public void TestGetPreviousDocumentsUserControlType()
	{
		using (var control = new ExportSupplierHeaderUserControlTestForTest())
		{
			AssertEquals(typeof(LayoutPreviousDocumentsUserControl), control.GetPreviousDocumentsUserControlType_Exposed());
		}
	}

	public void TestGetSupportingDocumentsUserControlType()
	{
		using (var control = new ExportSupplierHeaderUserControlTestForTest())
		{
			AssertEquals("Export type", typeof(LayoutSupportingDocumentsUserControl), control.GetSupportingDocumentsUserControlType_Exposed());
		}
	}

	public void TestAdditionalInfosTabPage()
	{
		using (var userControl = new ExportSupplierHeaderUserControl())
		{
			var tabPage = userControl.FindSingle<ZTabPage>("AdditionalInfoTabPage");
			AssertEquals("AdditionalInfosTabPage visible", true, tabPage.TabVisible);
		}
	}

	protected override IEnumerable<string> ExpectedControlList => DefaultControlList.Except(new[] { "GroupInvoiceDropEdit" });
}

sealed class ExportSupplierHeaderUserControlTestForTest : ExportSupplierHeaderUserControl
{
	public Type GetAdditionalInfosUserControlType_Exposed() => base.GetAdditionalInfosUserControlType();
	public Type GetPreviousDocumentsUserControlType_Exposed() => base.GetPreviousDocumentsUserControlType();
	public Type GetSupportingDocumentsUserControlType_Exposed() => base.GetSupportingDocumentsUserControlType();
}

using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.PL.GUI.PlugIn;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(ImportSupplierHeaderUserControl))]
sealed class ImportSupplierHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<ImportSupplierHeaderUserControl, JobDeclaration>
{
	public void TestGetPreviousDocumentsUserControlType()
	{
		using (var control = new ImportSupplierHeaderUserControlForTest())
		{
			AssertEquals(typeof(LayoutPreviousDocumentsUserControl), control.GetPreviousDocumentsUserControlType_Exposed());
		}
	}

	public void TestGetSupportingDocumentsUserControlType()
	{
		using (var control = new ImportSupplierHeaderUserControlForTest())
		{
			AssertEquals(typeof(LayoutSupportingDocumentsUserControl), control.GetSupportingDocumentsUserControlType_Exposed());
		}
	}

	public void TestGetAdditionalInfosUserControlType()
	{
		using (var control = new ImportSupplierHeaderUserControlForTest())
		{
			AssertEquals("Export type", typeof(AdditionalInfosUserControlWithGrid), control.GetAdditionalInfosUserControlType_Exposed());
		}
	}

	protected override IEnumerable<string> ExpectedControlList => DefaultControlList.Except(new[] { "GroupInvoiceDropEdit" });

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;

		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.JobComInvoiceLines.AddNew();
	}

	JobDeclaration declaration;
}

sealed class ImportSupplierHeaderUserControlForTest : ImportSupplierHeaderUserControl
{
	public Type GetPreviousDocumentsUserControlType_Exposed() => base.GetPreviousDocumentsUserControlType();
	public Type GetSupportingDocumentsUserControlType_Exposed() => base.GetSupportingDocumentsUserControlType();
	public Type GetAdditionalInfosUserControlType_Exposed() => base.GetAdditionalInfosUserControlType();
}

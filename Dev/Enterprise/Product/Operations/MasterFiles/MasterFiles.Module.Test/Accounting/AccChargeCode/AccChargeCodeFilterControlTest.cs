using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class AccChargeCodeFilterControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestAdjustColumnsForGlobalOrLocal()
		{
			using (var control = new AccChargeCodeFilterControl(new AccChargeCodeCollection(Factory), new AccChargeCodeFilterBusinessObject(), AccChargeCodeFilterControl.Mode.Normal))
			{
				AssertEquals("Linked to global related field 'IsLinkedToGlobalChargeCode' is shown", true, GetColumnStyle(control, "IsLinkedToGlobalChargeCode").IsVisible);
				AssertEquals("Linked to global related field 'IsDifferentToGlobalChargeCode' is shown", true, GetColumnStyle(control, "IsDifferentToGlobalChargeCode").IsVisible);
				AssertEquals("Consolidation related field 'Company+GC_Name' is not shown", null, GetColumnStyle(control, "Company+GC_Name"));
				AssertEquals("Consolidation related field 'Company+GC_Code' is not shown", null, GetColumnStyle(control, "Company+GC_Code"));
				AssertEquals("Local field 'AC_AT_GSTRate' is shown", true, GetColumnStyle(control, "AC_AT_GSTRate").IsVisible);
				AssertEquals("Local field 'AC_AW_WithholdingTaxRate' is shown", true, GetColumnStyle(control, "AC_AW_WithholdingTaxRate").IsVisible);
			}
			using (var control = new AccChargeCodeFilterControl(new AccChargeCodeCollection(Factory), new AccChargeCodeFilterBusinessObject(), AccChargeCodeFilterControl.Mode.Global))
			{
				AssertEquals("Linked to global related field 'IsLinkedToGlobalChargeCode' is not shown", null, GetColumnStyle(control, "IsLinkedToGlobalChargeCode"));
				AssertEquals("Linked to global related field 'IsDifferentToGlobalChargeCode' is not shown", null, GetColumnStyle(control, "IsDifferentToGlobalChargeCode"));
				AssertEquals("Consolidation related field 'Company+GC_Name' is not shown", null, GetColumnStyle(control, "Company+GC_Name"));
				AssertEquals("Consolidation related field 'Company+GC_Code' is not shown", null, GetColumnStyle(control, "Company+GC_Code"));
				AssertEquals("Local field 'AC_AT_GSTRate' is not shown", null, GetColumnStyle(control, "AC_AT_GSTRate"));
				AssertEquals("Local field 'AC_AW_WithholdingTaxRate' is not shown", null, GetColumnStyle(control, "AC_AW_WithholdingTaxRate"));
			}
			using (var control = new AccChargeCodeFilterControl(new AccChargeCodeCollection(Factory), new AccChargeCodeFilterBusinessObject(), AccChargeCodeFilterControl.Mode.Consolidation))
			{
				AssertEquals("Linked to global related field 'IsLinkedToGlobalChargeCode' is shown", true, GetColumnStyle(control, "IsLinkedToGlobalChargeCode").IsVisible);
				AssertEquals("Linked to global related field 'IsDifferentToGlobalChargeCode' is shown", true, GetColumnStyle(control, "IsDifferentToGlobalChargeCode").IsVisible);
				AssertEquals("Consolidation field 'Company+GC_Name' is shown", true, GetColumnStyle(control, "Company+GC_Name").IsVisible);
				AssertEquals("Consolidation field 'Company+GC_Code' is shown", true, GetColumnStyle(control, "Company+GC_Code").IsVisible);
				AssertEquals("Local field 'AC_AT_GSTRate' is shown", true, GetColumnStyle(control, "AC_AT_GSTRate").IsVisible);
				AssertEquals("Local field 'AC_AW_WithholdingTaxRate' is shown", true, GetColumnStyle(control, "AC_AW_WithholdingTaxRate").IsVisible);
			}

			foreach (bool regValue in new bool[] { true, false })
			{
				using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, regValue))
				{
					var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
					using (var control = new AccChargeCodeFilterControl(new AccChargeCodeCollection(Factory), new AccChargeCodeFilterBusinessObject(), AccChargeCodeFilterControl.Mode.Normal))
					{
						AssertEquals("AC_GovtChargeCode is shown", regValue, (GetColumnStyle(control, "AC_GovtChargeCode") != null));
					}

					using (var control = new AccChargeCodeFilterControl(new AccChargeCodeCollection(Factory), new AccChargeCodeFilterBusinessObject(), AccChargeCodeFilterControl.Mode.Global))
					{
						AssertEquals("AC_GovtChargeCode is shown", true, (GetColumnStyle(control, "AC_GovtChargeCode") == null));
					}
				}
			}
		}

		ZGridColumnInfo GetColumnStyle(AccChargeCodeFilterControl control, string columnName)
		{
			return control.Grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(s => s.ColumnName == columnName);
		}
	}
}

using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.TR.Business.Declaration;

namespace Enterprise.Customs.TR.GUI.Testing
{
	public class InvoiceLineTaxUserControlTest : TestCaseWithFactory
	{
		public void TestReadOnlyColumns()
		{
			using (var userControl = new InvoiceLineTaxUserControl())
			{
				AssertEquals(true, userControl.InvoiceLineTaxGrid.GetColumnStyle("JLT_TypeDescription").IsReadOnly);
				AssertEquals(false, userControl.InvoiceLineTaxGrid.GetColumnStyle("NationalType").IsReadOnly);
			}
		}

		public void TestNationalType()
		{
			using (var userControl = new InvoiceLineTaxUserControl())
			{
				AssertEquals(true, userControl.InvoiceLineTaxGrid.GetColumnStyle("NationalType").IsVisible);
				AssertEquals(false, userControl.InvoiceLineTaxGrid.GetColumnStyle("NationalType").IsReadOnly);
			}
		}

		public void TestVisibleAndColumnsOrder()
		{
			using (var userControl = new InvoiceLineTaxUserControl())
			{
				for (int i = 0; i < ExpectedColumnOrderList.Count; i++)
				{
					ZGridColumnInfo columnInfo = userControl.InvoiceLineTaxGrid.ColumnStyles[i] as ZGridColumnInfo;
					AssertNotNull(columnInfo);
					ZString expectedColumnName = ExpectedColumnOrderList[i];
					AssertEquals("Expected", expectedColumnName, columnInfo.ColumnName);
					AssertEquals("Visible", true, columnInfo.IsVisible);
				}
			}
		}

		List<string> ExpectedColumnOrderList
		{
			get
			{
				if (fExpectedColumnOrderList == null)
				{
					fExpectedColumnOrderList = new List<string>();
					fExpectedColumnOrderList.Add(JobComInvoiceLineTax.Schema.NationalType);
					fExpectedColumnOrderList.Add(nameof(JobComInvoiceLineTax.NationalTypeDescription));
					fExpectedColumnOrderList.Add(JobComInvoiceLineTax.Schema.JLT_RateOverrideReasonCode);
					fExpectedColumnOrderList.Add(JobComInvoiceLineTax.Schema.JLT_BaseValue);
					fExpectedColumnOrderList.Add(JobComInvoiceLineTax.Schema.JLT_MethodOfCalculation);
					fExpectedColumnOrderList.Add(JobComInvoiceLineTax.Schema.JLT_Rate);
					fExpectedColumnOrderList.Add(JobComInvoiceLineTax.Schema.JLT_Amount);
					fExpectedColumnOrderList.Add(JobComInvoiceLineTax.Schema.JLT_MethodOfPayment);
				}
				return fExpectedColumnOrderList;
			}
		}
		List<string> fExpectedColumnOrderList;
	}
}

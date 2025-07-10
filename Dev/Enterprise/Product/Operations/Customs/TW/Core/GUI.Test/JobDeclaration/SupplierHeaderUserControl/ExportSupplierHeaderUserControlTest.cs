using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common;
using Enterprise.Customs.GUI;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI.Testing
{
	sealed class ExportSupplierHeaderUserControlTest : TestCaseWithFactory
	{
		public void TestColumnLayoutContext()
		{
			using (ExportSupplierHeaderUserControl control = new ExportSupplierHeaderUserControl())
			{
				AssertEquals("Column layout context should have been set", nameof(Customs.GUI.DeclarationType.Export), control.InvoiceHeadersBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestVisiblChargesGridControls()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				using (var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl?.DeclarationUserControlForTesting)
				{
					if (jobDeclarationUserControl != null)
					{
						brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
						var invoiceChargesGrid = brokerageControl.SupplierHeaderUserControl.InvoiceChargesGrid;
						var baseGroupChargesGrid = brokerageControl.SupplierHeaderUserControl.BaseGroupChargesGrid;
						var apportionedChargesGrid = brokerageControl.SupplierHeaderUserControl.ApportionedChargesGrid;
						AssertEquals("J7_IsDutiable column is Visible", true, invoiceChargesGrid.GetColumnStyle("J7_IsDutiable").IsVisible);
						AssertEquals("Caption of J7_IsDutiable column is 'Incl. in FOB'", "Incl. in FOB", invoiceChargesGrid.GetColumnStyle("J7_IsDutiable").Caption);
						AssertEquals("J7_IsDutiable column is Visible", true, baseGroupChargesGrid.GetColumnStyle("J7_IsDutiable").IsVisible);
						AssertEquals("Caption of J7_IsDutiable column is 'Incl. in FOB'", "Incl. in FOB", baseGroupChargesGrid.GetColumnStyle("J7_IsDutiable").Caption);
						AssertEquals("J7_IsDutiable column is Visible", true, apportionedChargesGrid.GetColumnStyle("J7_IsDutiable").IsVisible);
						AssertEquals("Caption of J7_IsDutiable column is 'Incl. in FOB'", "Incl. in FOB", apportionedChargesGrid.GetColumnStyle("J7_IsDutiable").Caption);
						AssertNull("InvoiceChargesGrid should not have the column 'J7_IsStatisticalValueApplicable'.", invoiceChargesGrid.GetColumnStyle("J7_IsStatisticalValueApplicable"));
						AssertNull("InvoiceChargesGrid should not have the column 'J7_IsStatisticalValueApplicable'.", baseGroupChargesGrid.GetColumnStyle("J7_IsStatisticalValueApplicable"));
						AssertNull("InvoiceChargesGrid should not have the column 'J7_IsStatisticalValueApplicable'.", apportionedChargesGrid.GetColumnStyle("J7_IsStatisticalValueApplicable"));
						AssertNull("BaseGroupChargesGrid should not have the column 'J7_Percentage'.", baseGroupChargesGrid.GetColumnStyle(AutoJobComInvHeaderCharge.Schema.J7_Percentage));
					}
				}
			}
		}

		public void TestControlProperties()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				using (var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl?.DeclarationUserControlForTesting)
				{
					if (jobDeclarationUserControl != null)
					{
						brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
						var control = brokerageControl.SupplierHeaderUserControl.Controls.Find("IsFixedRateCheckBox", true)[0];
						AssertEquals("Show Fixed Rate(IsJZ_InvoiceCurrExRateUserEnterable) is true", true, control.Visible);
						var marksAndNumbersLongTextBox = brokerageControl.SupplierHeaderUserControl.FindSingleOrDefault<LongTextControl>(x => x.Name == "TW_MarksAndNumbersLongTextBox");
						AssertEquals(marksAndNumbersLongTextBox.GetExtension<ILabelCaptionRenderer>().Caption, "Marks & Numbers");
					}
				}
			}
		}

		public void TestChangeGridColumnsVisibility()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				using (var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting)
				{
					if (jobDeclarationUserControl != null)
					{
						brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
						var jobComInvoiceHeadersBoundGrid = brokerageControl.SupplierHeaderUserControl.JobComInvoiceHeadersBoundGrid.InnerGrid;
						AssertNull("Invoice Header Supplier is not relevant for TW", jobComInvoiceHeadersBoundGrid.Columns[JobComInvoiceHeader.Schema.JZ_OH_Supplier]);
						AssertNull("Invoice Header Importer is not relevant for TW", jobComInvoiceHeadersBoundGrid.Columns[JobComInvoiceHeader.Schema.JZ_OH_Buyer]);
						AssertNull(jobComInvoiceHeadersBoundGrid.Columns[JobComInvoiceHeader.Schema.JZ_RelatedIndicator]);
					}
				}
			}
		}

		public void TestChangeGridColumnsOrder()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				using (var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting)
				{
					if (jobDeclarationUserControl != null)
					{
						brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
						var invoiceChargesGrid = brokerageControl.SupplierHeaderUserControl.InvoiceChargesGrid;
						Assert("invoiceCharges grid count must have at least " + ExpectedColumnNamesInSortOrderList.Count.ToString(), ExpectedColumnNamesInSortOrderList.Count <= invoiceChargesGrid.Columns.Count);
						int index = 0;
						foreach (var expectedColumnName in ExpectedColumnNamesInSortOrderList.Keys)
						{
							var column = invoiceChargesGrid.Columns[index];
							AssertNotNull(column);
							ExpectedColumnNamesInSortOrderList.TryGetValue(expectedColumnName, out bool isVisible);
							AssertEquals("Expected Column Name", expectedColumnName, column.ColumnStyle.MappingName);
							AssertEquals("Expected IsVisible", isVisible, column.IsVisible);
							index++;
						}
					}
				}
			}
		}

		Dictionary<ZString, bool> ExpectedColumnNamesInSortOrderList
		{
			get
			{
				if (expectedColumnNamesInSortOrderList == null)
				{
					expectedColumnNamesInSortOrderList = new Dictionary<ZString, bool>();
					expectedColumnNamesInSortOrderList.Add(JobComInvCharge.Schema.J7_ChargeType, true);
					expectedColumnNamesInSortOrderList.Add(JobComInvCharge.Schema.J7_ChargeDescription, true);
					expectedColumnNamesInSortOrderList.Add(JobComInvCharge.Schema.J7_Amount, true);
					expectedColumnNamesInSortOrderList.Add(JobComInvCharge.Schema.J7_RX_NKCurrency, true);
					expectedColumnNamesInSortOrderList.Add(JobComInvCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount, true);
					expectedColumnNamesInSortOrderList.Add(JobComInvCharge.Schema.J7_IsIncludedInITOT, true);
					expectedColumnNamesInSortOrderList.Add(JobComInvCharge.Schema.J7_IsDutiable, true);
					expectedColumnNamesInSortOrderList.Add(JobComInvCharge.Schema.J7_IsGSTApplicable, true);
					expectedColumnNamesInSortOrderList.Add(JobComInvCharge.Schema.J7_DistributeBy, true);
					expectedColumnNamesInSortOrderList.Add(JobComInvCharge.Schema.IsJ7_ExchangeRateUserEnterable, false);
					expectedColumnNamesInSortOrderList.Add(JobComInvCharge.Schema.J7_ExchangeRate, false);
					expectedColumnNamesInSortOrderList.Add(JobComInvCharge.Schema.J7_PrepaidCollect, false);
					expectedColumnNamesInSortOrderList.Add(JobComInvCharge.Schema.J7_Percentage, false);
				}

				return expectedColumnNamesInSortOrderList;
			}
		}

		Dictionary<ZString, bool> expectedColumnNamesInSortOrderList;
		public void TestControlCaption()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				using (var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting)
				{
					if (jobDeclarationUserControl != null)
					{
						brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
						var fOBAmountBoundCurrencyControl = brokerageControl.SupplierHeaderUserControl.Controls.Find("JZ_FOBAmountBoundCurrencyControl", true)[0] as ConvertToLocalCurrencyControl;
						AssertEquals("FOB", fOBAmountBoundCurrencyControl.CaptionResourceString.Caption);
					}
				}
			}
		}

		public void TestIsGSTApplicableCaption()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				using (var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl?.DeclarationUserControlForTesting)
				{
					if (jobDeclarationUserControl != null)
					{
						brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
						var invoiceChargesIsGSTApplicableCaptionResourceString = brokerageControl.SupplierHeaderUserControl.InvoiceChargesGrid.GetColumnStyle("J7_IsGSTApplicable").CaptionResourceString;
						var baseGroupChargesIsGSTApplicableCaptionResourceString = brokerageControl.SupplierHeaderUserControl.BaseGroupChargesGrid.GetColumnStyle("J7_IsGSTApplicable").CaptionResourceString;
						var apportionedChargesIsGSTApplicableCaptionResourceString = brokerageControl.SupplierHeaderUserControl.ApportionedChargesGrid.GetColumnStyle("J7_IsGSTApplicable").CaptionResourceString;
						CombineAssertions(() =>
						{
							AssertEquals("Caption of J7_IsGSTApplicable column is 'Included in Declaration Total Invoice Amount (16)'", "Included in Declaration Total Invoice Amount (16)", invoiceChargesIsGSTApplicableCaptionResourceString.Caption);
							AssertEquals("ShortCaption of J7_IsGSTApplicable column is 'Incl. in Total Inv. Amt. (16)'", "Incl. in Total Inv. Amt. (16)", invoiceChargesIsGSTApplicableCaptionResourceString.ShortCaption);
							AssertEquals("FullDescription of J7_IsGSTApplicable column is 'It indicates whether the charge is included in the invoice total amount. The Incoterm and charge code determine whether the charge is included by default.'", "It indicates whether the charge is included in the invoice total amount. The Incoterm and charge code determine whether the charge is included by default.", invoiceChargesIsGSTApplicableCaptionResourceString.FullDescription);
							AssertEquals("Caption of J7_IsGSTApplicable column is 'Included in Declaration Total Invoice Amount (16)'", "Included in Declaration Total Invoice Amount (16)", baseGroupChargesIsGSTApplicableCaptionResourceString.Caption);
							AssertEquals("ShortCaption of J7_IsGSTApplicable column is 'Incl. in Total Inv. Amt. (16)'", "Incl. in Total Inv. Amt. (16)", baseGroupChargesIsGSTApplicableCaptionResourceString.ShortCaption);
							AssertEquals("FullDescription of J7_IsGSTApplicable column is 'It indicates whether the charge is included in the invoice total amount. The Incoterm and charge code determine whether the charge is included by default.'", "It indicates whether the charge is included in the invoice total amount. The Incoterm and charge code determine whether the charge is included by default.", baseGroupChargesIsGSTApplicableCaptionResourceString.FullDescription);
							AssertEquals("Caption of J7_IsGSTApplicable column is 'Included in Declaration Total Invoice Amount (16)'", "Included in Declaration Total Invoice Amount (16)", apportionedChargesIsGSTApplicableCaptionResourceString.Caption);
							AssertEquals("ShortCaption of J7_IsGSTApplicable column is 'Incl. in Total Inv. Amt. (16)'", "Incl. in Total Inv. Amt. (16)", apportionedChargesIsGSTApplicableCaptionResourceString.ShortCaption);
							AssertEquals("FullDescription of J7_IsGSTApplicable column is 'It indicates whether the charge is included in the invoice total amount. The Incoterm and charge code determine whether the charge is included by default.'", "It indicates whether the charge is included in the invoice total amount. The Incoterm and charge code determine whether the charge is included by default.", apportionedChargesIsGSTApplicableCaptionResourceString.FullDescription);
						});
					}
				}
			}
		}
	}
}

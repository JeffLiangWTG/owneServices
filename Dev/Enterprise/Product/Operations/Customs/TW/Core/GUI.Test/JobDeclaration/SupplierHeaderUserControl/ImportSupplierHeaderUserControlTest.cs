using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI.Testing
{
	sealed class ImportSupplierHeaderUserControlTest : TestCaseWithFactory
	{
		public void TestColumnLayoutContext()
		{
			using (ImportSupplierHeaderUserControl control = new ImportSupplierHeaderUserControl())
			{
				AssertEquals("Column layout context should have been set", nameof(Customs.GUI.DeclarationType.Import), control.InvoiceHeadersBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestVisiblChargesGridControls()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
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
						CombineAssertions(() =>
						{
							Assert("J7_IsDutiable column is Visible", invoiceChargesGrid.GetColumnStyle("J7_IsDutiable").IsVisible);
							AssertEquals("Caption of J7_IsDutiable column is 'Incl. in CIF'", "Incl. in CIF", invoiceChargesGrid.GetColumnStyle("J7_IsDutiable").Caption);
							Assert("J7_IsGSTApplicable column is Unavailable", invoiceChargesGrid.GetColumnStyle("J7_IsGSTApplicable").IsUnavailable);
							Assert("J7_IsStatisticalValueApplicable column is Visible", invoiceChargesGrid.GetColumnStyle("J7_IsStatisticalValueApplicable").IsVisible);
							AssertEquals("Caption of J7_IsStatisticalValueApplicable column is 'Incl. in FOB'", "Incl. in FOB", invoiceChargesGrid.GetColumnStyle("J7_IsStatisticalValueApplicable").CaptionResourceString.Caption);
							AssertEquals("FullDescription of J7_IsStatisticalValueApplicable column is 'It indicates whether the charge is included in the FOB price. The Incoterm and charge code determine whether the charge is included by default.'", "It indicates whether the charge is included in the FOB price. The Incoterm and charge code determine whether the charge is included by default.", invoiceChargesGrid.GetColumnStyle("J7_IsStatisticalValueApplicable").CaptionResourceString.FullDescription);
							Assert("J7_IsDutiable column is Visible", baseGroupChargesGrid.GetColumnStyle("J7_IsDutiable").IsVisible);
							AssertEquals("Caption of J7_IsDutiable column is Incl. in CIF", "Incl. in CIF", baseGroupChargesGrid.GetColumnStyle("J7_IsDutiable").Caption);
							Assert("J7_IsGSTApplicable column is Unavailable", baseGroupChargesGrid.GetColumnStyle("J7_IsGSTApplicable").IsUnavailable);
							Assert("J7_IsStatisticalValueApplicable column is Visible", baseGroupChargesGrid.GetColumnStyle("J7_IsStatisticalValueApplicable").IsVisible);
							AssertEquals("Caption of J7_IsStatisticalValueApplicable column is 'Incl. in FOB'", "Incl. in FOB", baseGroupChargesGrid.GetColumnStyle("J7_IsStatisticalValueApplicable").CaptionResourceString.Caption);
							AssertEquals("FullDescription of J7_IsStatisticalValueApplicable column is 'It indicates whether the charge is included in the FOB price. The Incoterm and charge code determine whether the charge is included by default.'", "It indicates whether the charge is included in the FOB price. The Incoterm and charge code determine whether the charge is included by default.", baseGroupChargesGrid.GetColumnStyle("J7_IsStatisticalValueApplicable").CaptionResourceString.FullDescription);
							Assert("J7_IsDutiable column is Visible", apportionedChargesGrid.GetColumnStyle("J7_IsDutiable").IsVisible);
							AssertEquals("Caption of J7_IsDutiable column is Incl. in CIF", "Incl. in CIF", apportionedChargesGrid.GetColumnStyle("J7_IsDutiable").Caption);
							Assert("J7_IsGSTApplicable column is Unavailable", apportionedChargesGrid.GetColumnStyle("J7_IsGSTApplicable").IsUnavailable);
							Assert("J7_IsStatisticalValueApplicable column is Visible", apportionedChargesGrid.GetColumnStyle("J7_IsStatisticalValueApplicable").IsVisible);
							AssertEquals("Caption of J7_IsStatisticalValueApplicable column is 'Incl. in FOB'", "Incl. in FOB", apportionedChargesGrid.GetColumnStyle("J7_IsStatisticalValueApplicable").CaptionResourceString.Caption);
							AssertEquals("FullDescription of J7_IsStatisticalValueApplicable column is 'It indicates whether the charge is included in the FOB price. The Incoterm and charge code determine whether the charge is included by default.'", "It indicates whether the charge is included in the FOB price. The Incoterm and charge code determine whether the charge is included by default.", apportionedChargesGrid.GetColumnStyle("J7_IsStatisticalValueApplicable").CaptionResourceString.FullDescription);
							AssertNull("BaseGroupChargesGrid should not have the column 'J7_Percentage'.", baseGroupChargesGrid.GetColumnStyle(Common.AutoJobComInvHeaderCharge.Schema.J7_Percentage));
						});
					}
				}
			}
		}

		public void TestControlProperties()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
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

		public void TestGridId()
		{
			using (var control = new ImportSupplierHeaderUserControl())
			{
				AssertEquals("GridLayoutmzSsmLeeORtZAZNEEQiqsA==", control.JobComInvoiceHeadersBoundGrid.InnerGrid.GridId);
			}
		}

		public void TestChangeGridColumnsVisibility()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
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
						Assert(!jobComInvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_RelatedIndicator).IsVisible);
					}
				}
			}
		}

		public void TestChangeGridColumnsOrder()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
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
					expectedColumnNamesInSortOrderList.Add(BaseJobComInvHeaderCharge.Schema.J7_ChargeType, true);
					expectedColumnNamesInSortOrderList.Add(BaseJobComInvHeaderCharge.Schema.J7_ChargeDescription, true);
					expectedColumnNamesInSortOrderList.Add(BaseJobComInvHeaderCharge.Schema.J7_Amount, true);
					expectedColumnNamesInSortOrderList.Add(BaseJobComInvHeaderCharge.Schema.J7_RX_NKCurrency, true);
					expectedColumnNamesInSortOrderList.Add(BaseJobComInvHeaderCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount, true);
					expectedColumnNamesInSortOrderList.Add(BaseJobComInvHeaderCharge.Schema.J7_IsIncludedInITOT, true);
					expectedColumnNamesInSortOrderList.Add(BaseJobComInvHeaderCharge.Schema.J7_IsStatisticalValueApplicable, true);
					expectedColumnNamesInSortOrderList.Add(BaseJobComInvHeaderCharge.Schema.J7_IsDutiable, true);
					expectedColumnNamesInSortOrderList.Add(BaseJobComInvHeaderCharge.Schema.J7_DistributeBy, true);
					expectedColumnNamesInSortOrderList.Add(BaseJobComInvHeaderCharge.Schema.IsJ7_ExchangeRateUserEnterable, false);
					expectedColumnNamesInSortOrderList.Add(BaseJobComInvHeaderCharge.Schema.J7_ExchangeRate, false);
					expectedColumnNamesInSortOrderList.Add(BaseJobComInvHeaderCharge.Schema.J7_PrepaidCollect, false);
					expectedColumnNamesInSortOrderList.Add(BaseJobComInvHeaderCharge.Schema.J7_Percentage, false);
				}

				return expectedColumnNamesInSortOrderList;
			}
		}

		Dictionary<ZString, bool> expectedColumnNamesInSortOrderList;
		public void TestControlCaption()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
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
						AssertEquals("CIF", fOBAmountBoundCurrencyControl.CaptionResourceString.Caption);
					}
				}
			}
		}
	}
}

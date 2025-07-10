using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class BaseCustomsSupplierHeaderUserControlBaseOnlyTest : TestCaseWithFactory
	{
		public void TestGridId()
		{
			using (var control = new BaseCustomsSupplierHeaderUserControl())
			{
				AssertEquals("GridLayoutUU5vFvxYnT4ITmuJkyIQgg==", control.JobComInvoiceHeadersBoundGrid.InnerGrid.GridId);
			}
		}

		public void TestGetColumnOrderForInvoiceHeaderGrid()
		{
			var testDec = BaseJobDeclaration.New(Factory);
			testDec.Invoices.AddNew();

			using (var testForm = new DeclarationFormForTesting(testDec))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AssertEquals("Number of Columns", 39, testForm.CustomsBrokerageUserControl.SupplierHeaderUserControl.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns.Count);
			}
		}

		public void TestApportionmentPendingLabelShownOnLoadIfDirty()
		{
			var testDec = BaseJobDeclaration.New(Factory);
			testDec.Invoices.AddNew();

			using (var testForm = new DeclarationFormForTesting(testDec))
			{
				testForm.Show();
				testDec.ApportionmentDirty = true;

				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AssertEquals("PreCondition:Apportionment is dirty", true, testDec.ApportionmentDirty);
				AssertEquals("Pending lable is shown", true, testForm.CustomsBrokerageUserControl.SupplierHeaderUserControl.ApportionmentPendingLabel.Visible);
			}
		}

		public void TestInvCustomFieldsDisplayControl_Binding()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			declaration.Invoices.AddNew();

			using (var control = new BaseCustomsSupplierHeaderUserControl())
			{
				control.SetDataBinding(declaration, "");
				var invCustomFieldsDisplayControl = control.FindSingle<InvoiceHeaderCustomFieldsUserControl>("InvCustomFieldsDisplayControl");
				AssertEquals("InvCustomFieldsDisplayControl data binding.", nameof(BaseJobDeclaration.Invoices), control.BindingSource.GetBindingMember(invCustomFieldsDisplayControl));
			}
		}

		public void TestApportionmentPendingLabelHiddenOnLoadIfNotDirty()
		{
			var testDec = BaseJobDeclaration.New(Factory);
			testDec.Invoices.AddNew();

			using (var testForm = new DeclarationFormForTesting(testDec))
			{
				testForm.Show();
				testDec.ApportionmentDirty = false;

				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				var testUserControl = testForm.CustomsBrokerageUserControl.SupplierHeaderUserControl;

				AssertEquals("PreCondition:Apportionment is dirty", false, testDec.ApportionmentDirty);
				AssertEquals("Pending lable is not shown", false, testUserControl.ApportionmentPendingLabel.Visible);
				AssertEquals("Control is shown", true, testUserControl.JZ_FOBAmountBoundCurrencyControl.Visible);
				AssertEquals("Control is shown", true, testUserControl.JZ_CIFAmountBoundCurrencyControl.Visible);
				AssertEquals("Control is shown", true, testUserControl.JZ_Calc_TNIBoundInvoiceCurrencyControl.Visible);
			}
		}

		public void TestApportionmentPendingLabelShownWhenDirtyGetsChangedToTrue()
		{
			var testDec = BaseJobDeclaration.New(Factory);
			testDec.Invoices.AddNew();

			using (var testForm = new DeclarationFormForTesting(testDec))
			{
				testDec.ApportionmentDirty = false;
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				var testUserControl = testForm.CustomsBrokerageUserControl.SupplierHeaderUserControl;

				AssertEquals("PreCondition:Apportionment is dirty", false, testDec.ApportionmentDirty);
				AssertEquals("Pending lable is not shown", false, testUserControl.ApportionmentPendingLabel.Visible);

				testDec.ApportionmentDirty = true;
				AssertEquals("Apportionment is dirty", true, testDec.ApportionmentDirty);
				AssertEquals("Pending lable is shown", true, testUserControl.ApportionmentPendingLabel.Visible);
			}
		}

		public void TestApportionmentPendingLabelHiddenWhenDirtyGetsChangedToFalse()
		{
			var testDec = BaseJobDeclaration.New(Factory);
			testDec.Invoices.AddNew();

			using (var testForm = new DeclarationFormForTesting(testDec))
			{
				testDec.ApportionmentDirty = true;
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				var testUserControl = testForm.CustomsBrokerageUserControl.SupplierHeaderUserControl;

				AssertEquals("PreCondition:Apportionment is dirty", true, testDec.ApportionmentDirty);
				AssertEquals("Pending lable is not shown", true, testUserControl.ApportionmentPendingLabel.Visible);

				testDec.ApportionmentDirty = false;
				AssertEquals("Apportionment is dirty", false, testDec.ApportionmentDirty);
				AssertEquals("Pending lable is shown", false, testUserControl.ApportionmentPendingLabel.Visible);
			}
		}

		public void TestLockingGrid()
		{
			var jobDecBizObj = BaseJobDeclaration.New(Factory);
			jobDecBizObj.JE_IsCancelled = true;
			jobDecBizObj.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			using (var testForm = new DeclarationFormForTesting(jobDecBizObj))
			{
				testForm.Show();
				var decUserControl = testForm.DeclarationUserControl as BaseCustomsDeclarationUserControl;
				testForm.CustomsBrokerageUserControl.SetDeclarationReadOnly(true);
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AssertEquals("JobComInvoiceHeadersBoundGrid ReadOnly", true, testForm.CustomsBrokerageUserControl.SupplierHeaderUserControl.JobComInvoiceHeadersBoundGrid.InnerGrid.ReadOnly);
			}
		}

		public void TestJobComInvoiceHeadersGridSkipsGroupInvoiceColumn()
		{
			using (var testForm = new DeclarationFormForTesting(Factory.New<BaseJobDeclaration>()))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AssertEquals("JobComInvoiceHeadersBoundGrid should has columns to skip", 1, testForm.SupplierUserControl.JobComInvoiceHeadersBoundGrid.InvoiceInnerGrid.ColumnsToSkip.Count);
			}
		}

		public void TestRemoveColumns()
		{
			var testDec = BaseJobDeclaration.New(Factory);
			testDec.Invoices.AddNew();

			using (var testForm = new DeclarationFormForTesting(testDec))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;

				var userControl = testForm.SupplierUserControl;
				var column = userControl.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[BaseJobComInvoiceHeader.Schema.JZ_InvoiceNumber];
				AssertNotNull("Column exists", column);

				userControl.JobComInvoiceHeadersBoundGrid.InnerGrid.RemoveFromAvailableColumns(BaseJobComInvoiceHeader.Schema.JZ_InvoiceNumber);
				AssertEquals("Column shouldn't be there", false, userControl.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns.Contains(BaseJobComInvoiceHeader.Schema.JZ_InvoiceNumber));
			}
		}

		public void TestCustomFieldsTabPage()
		{
			var testDec = BaseJobDeclaration.New(Factory);
			testDec.Invoices.AddNew();

			using (var testForm = new DeclarationFormForTesting(testDec))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;

				var tabPagesCount = testForm.CustomsBrokerageUserControl.SupplierHeaderUserControl.InvoiceTabControl.TabPages.Count;
				var customFieldsTabPage = testForm.CustomsBrokerageUserControl.SupplierHeaderUserControl.InvoiceTabControl.TabPages[tabPagesCount - 1];

				AssertEquals("Custom Fields tab should be the last tab", "Custom Fields", customFieldsTabPage.Text);
				AssertEquals("Custom Fields tab should have an InvoiceHeaderCustomFieldsUserControl", typeof(InvoiceHeaderCustomFieldsUserControl), customFieldsTabPage.Controls[0].GetType());
			}
		}

		public void TestGridExpectedInvoiceLineTotalAbbreviatedTile()
		{
			using (var control = new BaseCustomsSupplierHeaderUserControl())
			{
				var grid = control.JobComInvoiceHeadersBoundGrid;
				var invoiceLineTotalColumn = grid.ColumnStyles.Cast<Core.Forms.ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == "InvoiceLineTotal");

				AssertNotNull(invoiceLineTotalColumn);
				AssertNotNull(invoiceLineTotalColumn.CaptionResourceString);
				AssertEquals("Expected Invoice Line Total", invoiceLineTotalColumn.CaptionResourceString.Caption);
				AssertEquals("Expected Line Total", invoiceLineTotalColumn.CaptionResourceString.MediumCaption);
				AssertEquals("Exp. Total", invoiceLineTotalColumn.CaptionResourceString.ShortCaption);
			}
		}

		public void TestDutiableColumnsCaption()
		{
			const string expectedDutiableCaptionForExport = "Add to FOB?";
			const string expectedDutiableCaptionForImport = "Dutiable";

			var declaration = BaseJobDeclaration.New(Factory);
			using (var control = new BaseCustomsSupplierHeaderUserControl())
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				control.SetDataBinding(declaration, "");
				CombineAssertions(() =>
				{
					AssertEquals("ApportionedChargesGrid", expectedDutiableCaptionForExport, control.ApportionedChargesGrid.GetColumnStyle(JobComInvCharge.Schema.J7_IsDutiable).Caption);
					AssertEquals("BaseGroupChargesGrid", expectedDutiableCaptionForExport, control.BaseGroupChargesGrid.GetColumnStyle(JobComInvCharge.Schema.J7_IsDutiable).Caption);
					AssertEquals("InvoiceChargesGrid", expectedDutiableCaptionForExport, control.InvoiceChargesGrid.GetColumnStyle(JobComInvCharge.Schema.J7_IsDutiable).Caption);
				});
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				CombineAssertions(() =>
				{
					AssertEquals("ApportionedChargesGrid", expectedDutiableCaptionForImport, control.ApportionedChargesGrid.GetColumnStyle(JobComInvCharge.Schema.J7_IsDutiable).Caption);
					AssertEquals("BaseGroupChargesGrid", expectedDutiableCaptionForImport, control.BaseGroupChargesGrid.GetColumnStyle(JobComInvCharge.Schema.J7_IsDutiable).Caption);
					AssertEquals("InvoiceChargesGrid", expectedDutiableCaptionForImport, control.InvoiceChargesGrid.GetColumnStyle(JobComInvCharge.Schema.J7_IsDutiable).Caption);
				});
			}
			using (var control = new BaseCustomsSupplierHeaderUserControl())
			{
				control.SetDataBinding(declaration, "");
				CombineAssertions(() =>
				{
					AssertEquals("ApportionedChargesGrid", expectedDutiableCaptionForImport, control.ApportionedChargesGrid.GetColumnStyle(JobComInvCharge.Schema.J7_IsDutiable).Caption);
					AssertEquals("BaseGroupChargesGrid", expectedDutiableCaptionForImport, control.BaseGroupChargesGrid.GetColumnStyle(JobComInvCharge.Schema.J7_IsDutiable).Caption);
					AssertEquals("InvoiceChargesGrid", expectedDutiableCaptionForImport, control.InvoiceChargesGrid.GetColumnStyle(JobComInvCharge.Schema.J7_IsDutiable).Caption);
				});
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				CombineAssertions(() =>
				{
					AssertEquals("ApportionedChargesGrid", expectedDutiableCaptionForExport, control.ApportionedChargesGrid.GetColumnStyle(JobComInvCharge.Schema.J7_IsDutiable).Caption);
					AssertEquals("BaseGroupChargesGrid", expectedDutiableCaptionForExport, control.BaseGroupChargesGrid.GetColumnStyle(JobComInvCharge.Schema.J7_IsDutiable).Caption);
					AssertEquals("InvoiceChargesGrid", expectedDutiableCaptionForExport, control.InvoiceChargesGrid.GetColumnStyle(JobComInvCharge.Schema.J7_IsDutiable).Caption);
				});
			}
		}

		public void TestInvoiceChargesGridAmountGroup()
		{
			using (var control = new BaseCustomsSupplierHeaderUserControl())
			{
				const string amountGroupId = "BaseCustomsSupplierHeaderUserControl|6606e909-6bf0-4962-a09a-99123cbca1c6";
				var chargesGrid = control.InvoiceChargesGrid;
				CombineAssertions(() =>
				{
					AssertEquals("Amount", amountGroupId, chargesGrid.GetColumnStyle(JobComInvCharge.Schema.J7_Amount).GroupName.Key);
					AssertEquals("Currency", amountGroupId, chargesGrid.GetColumnStyle(JobComInvCharge.Schema.J7_RX_NKCurrency).GroupName.Key);
				});
			}
		}

		public void TestApportionedChargesGridChargeAmountGroup()
		{
			using (var control = new BaseCustomsSupplierHeaderUserControl())
			{
				const string chargeAmountGroupId = "BaseCustomsSupplierHeaderUserControl|f597adb6-79af-45a5-a7f9-69373a0910e6";
				var apportionedChargesGrid = control.ApportionedChargesGrid;
				CombineAssertions(() =>
				{
					AssertEquals("Amount", chargeAmountGroupId, apportionedChargesGrid.GetColumnStyle(JobComInvCharge.Schema.J7_Amount).GroupName.Key);
					AssertEquals("Currency", chargeAmountGroupId, apportionedChargesGrid.GetColumnStyle(JobComInvCharge.Schema.J7_RX_NKCurrency).GroupName.Key);
				});
			}
		}

		public void TestJobComInvoiceHeadersBoundGridInvoiceAmountGroup()
		{
			using (var control = new BaseCustomsSupplierHeaderUserControl())
			{
				const string invoiceAmountGroupId = "InvoiceModuleButtonGrid|474e3252-b0ca-4077-b698-9e8fde384bda";
				var invoiceHeadersGrid = control.JobComInvoiceHeadersBoundGrid.InnerGrid;
				CombineAssertions(() =>
				{
					AssertEquals("Amount", invoiceAmountGroupId, invoiceHeadersGrid.GetColumnStyle(BaseJobComInvoiceHeader.Schema.JZ_InvoiceAmount).GroupName.Key);
					AssertEquals("Currency", invoiceAmountGroupId, invoiceHeadersGrid.GetColumnStyle(BaseJobComInvoiceHeader.Schema.JZ_RX_NKInvoice_Currency).GroupName.Key);
				});
			}
		}

		public void TestJobComInvoiceHeadersBoundGridFOBGroup()
		{
			using (var control = new BaseCustomsSupplierHeaderUserControl())
			{
				const string fobGroupId = "InvoiceModuleButtonGrid|3272c7e3-05c2-4e5b-adf0-53a800a64dcf";
				var invoiceHeadersGrid = control.JobComInvoiceHeadersBoundGrid.InnerGrid;
				CombineAssertions(() =>
				{
					AssertEquals("Amount", fobGroupId, invoiceHeadersGrid.GetColumnStyle(BaseJobComInvoiceHeader.Schema.JZ_Calc_FOBAmount).GroupName.Key);
					AssertEquals("Currency", fobGroupId, invoiceHeadersGrid.GetColumnStyle(BaseJobComInvoiceHeader.Schema.JZ_Calc_FOBCurrency).GroupName.Key);
				});
			}
		}

		public void TestJobComInvoiceHeadersBoundGridCIFGroup()
		{
			using (var control = new BaseCustomsSupplierHeaderUserControl())
			{
				const string cifGroupId = "InvoiceModuleButtonGrid|ee81ec6a-bfef-42d7-80bd-10cb5537ba11";
				var invoiceHeadersGrid = control.JobComInvoiceHeadersBoundGrid.InnerGrid;
				CombineAssertions(() =>
				{
					AssertEquals("Amount", cifGroupId, invoiceHeadersGrid.GetColumnStyle(BaseJobComInvoiceHeader.Schema.JZ_Calc_CIFAmount).GroupName.Key);
					AssertEquals("Currency", cifGroupId, invoiceHeadersGrid.GetColumnStyle(BaseJobComInvoiceHeader.Schema.JZ_Calc_CIFCurrency).GroupName.Key);
				});
			}
		}

		public void TestJobComInvoiceHeadersBoundGridVolumeGroup()
		{
			using (var control = new BaseCustomsSupplierHeaderUserControl())
			{
				const string volumeGroupId = "InvoiceModuleButtonGrid|eb156704-7455-4f1f-b114-3cb0e1e867c2";
				var invoiceHeadersGrid = control.JobComInvoiceHeadersBoundGrid.InnerGrid;
				CombineAssertions(() =>
				{
					AssertEquals("Volumne", volumeGroupId, invoiceHeadersGrid.GetColumnStyle(BaseJobComInvoiceHeader.Schema.JZ_Volume).GroupName.Key);
					AssertEquals("Unit Qty.", volumeGroupId, invoiceHeadersGrid.GetColumnStyle(BaseJobComInvoiceHeader.Schema.JZ_VolumeUQ).GroupName.Key);
				});
			}
		}

		public void TestJobComInvoiceHeadersBoundGridWeightGroup()
		{
			using (var control = new BaseCustomsSupplierHeaderUserControl())
			{
				const string weightGroupId = "InvoiceModuleButtonGrid|93574e92-0e40-4839-97a4-bb7ede668e81";
				var invoiceHeadersGrid = control.JobComInvoiceHeadersBoundGrid.InnerGrid;
				CombineAssertions(() =>
				{
					AssertEquals("Weight", weightGroupId, invoiceHeadersGrid.GetColumnStyle(BaseJobComInvoiceHeader.Schema.JZ_Weight).GroupName.Key);
					AssertEquals("Unit Qty.", weightGroupId, invoiceHeadersGrid.GetColumnStyle(BaseJobComInvoiceHeader.Schema.JZ_WeightUQ).GroupName.Key);
					AssertEquals("Net Weight", weightGroupId, invoiceHeadersGrid.GetColumnStyle(BaseJobComInvoiceHeader.Schema.JZ_NetWeight).GroupName.Key);
					AssertEquals("Net Weight Unit Qty.", weightGroupId, invoiceHeadersGrid.GetColumnStyle(BaseJobComInvoiceHeader.Schema.JZ_NetWeightUQ).GroupName.Key);
				});
			}
		}

		public void TestJobComInvoiceHeadersBoundGridNumberOfPacksGroup()
		{
			using (var control = new BaseCustomsSupplierHeaderUserControl())
			{
				const string numberOfPacksGroupId = "InvoiceModuleButtonGrid|edea4137-d376-404c-a597-6e86de4f0eec";
				var invoiceHeadersGrid = control.JobComInvoiceHeadersBoundGrid.InnerGrid;
				CombineAssertions(() =>
				{
					AssertEquals("Packages", numberOfPacksGroupId, invoiceHeadersGrid.GetColumnStyle(BaseJobComInvoiceHeader.Schema.JZ_NoOfPacks).GroupName.Key);
					AssertEquals("Unit Qty.", numberOfPacksGroupId, invoiceHeadersGrid.GetColumnStyle(BaseJobComInvoiceHeader.Schema.NoOfPacksPackType).GroupName.Key);
				});
			}
		}

		public void TestJobComInvoiceHeadersBoundGridLetterOfCreditNumberColumn()
		{
			using (var control = new BaseCustomsSupplierHeaderUserControl())
			{
				var invoiceHeadersGrid = control.JobComInvoiceHeadersBoundGrid.InnerGrid;
				CombineAssertions(() =>
				{
					var letterOfCreditNumberColumnStyle = invoiceHeadersGrid.GetColumnStyle(BaseJobComInvoiceHeader.Schema.JZ_LetterOfCreditNumber);
					AssertEquals("Column JZ_LetterOfCreditNumber should be hidden by default.", false, letterOfCreditNumberColumnStyle.IsVisible);
					AssertEquals("Letter of Credit Number", letterOfCreditNumberColumnStyle.CaptionResourceString.Caption);
				});
			}
		}

		public void TestJobComInvoiceHeadersBoundGridLetterOfCreditDateColumn()
		{
			using (var control = new BaseCustomsSupplierHeaderUserControl())
			{
				var invoiceHeadersGrid = control.JobComInvoiceHeadersBoundGrid.InnerGrid;
				CombineAssertions(() =>
				{
					var letterOfCreditDateColumnStyle = invoiceHeadersGrid.GetColumnStyle(BaseJobComInvoiceHeader.Schema.JZ_LetterOfCreditDate);
					AssertEquals("Column JZ_LetterOfCreditDate should be hidden by default.", false, letterOfCreditDateColumnStyle.IsVisible);
					AssertEquals("Letter of Credit Date", letterOfCreditDateColumnStyle.CaptionResourceString.Caption);
				});
			}
		}

		public void TestJobComInvoiceHeadersBoundGridExportersBankNameColumn()
		{
			using (var control = new BaseCustomsSupplierHeaderUserControl())
			{
				var invoiceHeadersGrid = control.JobComInvoiceHeadersBoundGrid.InnerGrid;
				CombineAssertions(() =>
				{
					var exporterBankNameColumnStyle = invoiceHeadersGrid.GetColumnStyle(BaseJobComInvoiceHeader.Schema.JZ_ExporterBankName);
					AssertEquals("Column JZ_ExporterBankName should be hidden by default.", false, exporterBankNameColumnStyle.IsVisible);
					AssertEquals("Exporters Bank Name", exporterBankNameColumnStyle.CaptionResourceString.Caption);
				});
			}
		}

		public void TestJobComInvoiceHeadersBoundGridExportersBankAccountNoColumn()
		{
			using (var control = new BaseCustomsSupplierHeaderUserControl())
			{
				var invoiceHeadersGrid = control.JobComInvoiceHeadersBoundGrid.InnerGrid;
				CombineAssertions(() =>
				{
					var exporterBankAccountNumberColumnStyle = invoiceHeadersGrid.GetColumnStyle(BaseJobComInvoiceHeader.Schema.JZ_ExporterBankAccountNumber);
					AssertEquals("Column JZ_ExporterBankAccountNumber should be hidden by default.", false, exporterBankAccountNumberColumnStyle.IsVisible);
					AssertEquals("Exporters Bank Account No", exporterBankAccountNumberColumnStyle.CaptionResourceString.Caption);
				});
			}
		}

		public void TestJobComInvoiceHeadersBoundGridExportersBankSWIFTCodeColumn()
		{
			using (var control = new BaseCustomsSupplierHeaderUserControl())
			{
				var invoiceHeadersGrid = control.JobComInvoiceHeadersBoundGrid.InnerGrid;
				CombineAssertions(() =>
				{
					var exporterBankSWIFTCodeColumnStyle = invoiceHeadersGrid.GetColumnStyle(BaseJobComInvoiceHeader.Schema.JZ_ExporterBankSWIFTCode);
					AssertEquals("Column JZ_ExporterBankSWIFTCode should be hidden by default.", false, exporterBankSWIFTCodeColumnStyle.IsVisible);
					AssertEquals("Exporters Bank SWIFT Code", exporterBankSWIFTCodeColumnStyle.CaptionResourceString.Caption);
				});
			}
		}

		public void TestJobComInvoiceHeadersBoundGridRemarksColumn()
		{
			using (var control = new BaseCustomsSupplierHeaderUserControl())
			{
				var invoiceHeadersGrid = control.JobComInvoiceHeadersBoundGrid.InnerGrid;
				CombineAssertions(() =>
				{
					var exporterBankSWIFTCodeColumnStyle = invoiceHeadersGrid.GetColumnStyle(BaseJobComInvoiceHeader.Schema.JZ_Remarks);
					AssertEquals("Column JZ_Remarks should be shown by default.", true, exporterBankSWIFTCodeColumnStyle.IsVisible);
					AssertEquals("Remarks", exporterBankSWIFTCodeColumnStyle.CaptionResourceString.Caption);
				});
			}
		}

		class DeclarationFormForTesting : BaseJobDeclarationForm
		{
			public DeclarationFormForTesting(BaseJobDeclaration jobDeclaration) : base(jobDeclaration) { }

			public BaseCustomsEntryUserControl DeclarationUserControl => CustomsBrokerageUserControl.DeclarationUserControlForTesting;

			public BaseCustomsSupplierHeaderUserControl SupplierUserControl => CustomsBrokerageUserControl.SupplierHeaderUserControl;

			protected override BaseCustomsBrokerageUserControl GetBrokerageUserControl()
			{
				return new BrokerageUserControlForTesting();
			}
		}

		sealed class BrokerageUserControlForTesting : BaseCustomsBrokerageUserControl
		{
			protected override BaseCustomsSupplierHeaderUserControl GetSupplierHeaderUserControl()
			{
				return new BaseCustomsSupplierHeaderUserControl();
			}
		}
	}
}

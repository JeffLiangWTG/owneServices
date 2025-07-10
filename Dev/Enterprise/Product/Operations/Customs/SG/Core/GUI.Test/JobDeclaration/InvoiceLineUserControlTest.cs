using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.GUI;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	sealed class InvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestTariffColumn()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var grid = form.CustomsBrokerageUserControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid;
				var column = grid.Columns[JobComInvoiceLineSchema.Constants.JI_Tariff];
				AssertNotNull(column);
				AssertType<TariffColumnStyle>(column.ColumnStyle);
			}
		}

		public void TestTariffFindBox()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			var previousValue = Env.Registry.ExternalBorderComplianceTool;
			var previousUmpApiBaseAddress = Env.Registry.BorderWiseUmpApiBaseAddress;
			Enterprise.ZArchitecture.Environment.DataRegistry.Instance.BorderWiseEnableWebSocketClient = false;

			void AssertTariffFindBox<T>(string externalBorderComplianceTool)
			{
				using (new DisposableAction(() =>
				{
					Env.Registry.ExternalBorderComplianceTool = externalBorderComplianceTool;
					Env.Registry.BorderWiseUmpApiBaseAddress = string.Empty;
				}, () =>
				{
					Env.Registry.ExternalBorderComplianceTool = previousValue;
					Env.Registry.BorderWiseUmpApiBaseAddress = previousUmpApiBaseAddress;
				}))
				using (var form = new JobDeclarationForm(declaration))
				{
					form.Show();
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					var control = form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
					var tariffGridFindBox = control.FindSingle<TariffFindBox>();
					tariffGridFindBox.PopupButton.PerformClick();
					AssertType<T>(((IFindBox)tariffGridFindBox).PopupForm);
				}
			}

			AssertTariffFindBox<EmbeddedModulePopup>(ExternalBorderComplianceToolList.Codes.None);
			AssertTariffFindBox<FindBoxWrapperForBorderWise>(ExternalBorderComplianceToolList.Codes.BorderWiseWeb);
		}

		public void TestChargeColumns()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				form.CustomsBrokerageUserControl.InvoiceLinesUserControl.LineDetailTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesUserControl.LineChargesTabPage;
				var grid = form.CustomsBrokerageUserControl.InvoiceLinesUserControl.InvoiceLineCharges.ApportionedChargesGrid;
				var dutiableColumn = grid.Columns[JobComInvHeaderChargeSchema.Constants.J7_IsDutiable];
				AssertNull("J7_IsDutiable column removed", dutiableColumn);
				var gSTApplicableColumn = grid.Columns[JobComInvHeaderChargeSchema.Constants.J7_IsGSTApplicable];
				AssertNull("J7_GSTApplicable column removed", gSTApplicableColumn);
				grid = form.CustomsBrokerageUserControl.InvoiceLinesUserControl.InvoiceLineCharges.ChargesGrid;
				dutiableColumn = grid.Columns[JobComInvHeaderChargeSchema.Constants.J7_IsDutiable];
				AssertNull("J7_IsDutiable column removed", dutiableColumn);
				gSTApplicableColumn = grid.Columns[JobComInvHeaderChargeSchema.Constants.J7_IsGSTApplicable];
				AssertNull("J7_GSTApplicable column removed", gSTApplicableColumn);
			}
		}

		public void TestContainersTabIsHidden()
		{
			using (SGInvoiceLineUserControl invoiceLineUserControl = new SGInvoiceLineUserControl())
			{
				invoiceLineUserControl.JobDeclaration = Factory.New<JobDeclaration>();
				AssertEquals(false, invoiceLineUserControl.ContainersTabPage.TabVisible);
			}
		}

		public void TestCOTabIsHiddenOnCondition()
		{
			Declaration.JE_MessageType = "";
			using (ZForm form = new ZForm(Declaration))
			{
				using (SGInvoiceLineUserControl invoiceLineUserControl = new SGInvoiceLineUserControl())
				{
					invoiceLineUserControl.JobDeclaration = Declaration;
					form.Controls.Add(invoiceLineUserControl);
					form.Show();
					AssertNull(invoiceLineUserControl.LineDetailTabControl.TabPages[invoiceLineUserControl.CertificateOfOriginTabPage.Name]);
					invoiceLineUserControl.Visible = false;
					Declaration.JE_MessageType = MessageTypeCodeList.Codes.COO;
					invoiceLineUserControl.Visible = true;
					AssertNotNull(invoiceLineUserControl.LineDetailTabControl.TabPages[invoiceLineUserControl.CertificateOfOriginTabPage.Name]);
					invoiceLineUserControl.Visible = false;
					Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
					invoiceLineUserControl.Visible = true;
					AssertNotNull(invoiceLineUserControl.LineDetailTabControl.TabPages[invoiceLineUserControl.CertificateOfOriginTabPage.Name]);
					invoiceLineUserControl.Visible = false;
					Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
					Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKO;
					invoiceLineUserControl.Visible = true;
					AssertNull(invoiceLineUserControl.LineDetailTabControl.TabPages[invoiceLineUserControl.CertificateOfOriginTabPage.Name]);
					invoiceLineUserControl.Visible = false;
					Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
					invoiceLineUserControl.Visible = true;
					AssertNull(invoiceLineUserControl.LineDetailTabControl.TabPages[invoiceLineUserControl.CertificateOfOriginTabPage.Name]);
					invoiceLineUserControl.Visible = false;
					Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
					invoiceLineUserControl.Visible = true;
					AssertNotNull(invoiceLineUserControl.LineDetailTabControl.TabPages[invoiceLineUserControl.CertificateOfOriginTabPage.Name]);
				}
			}
		}

		public void TestInvoiceLineGrid_Columns()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				ZGrid grid = ((SGInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl).CustomsInvoiceLinesBoundGrid;
				ZGridColumns columns = grid.Columns;
				AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_LineNo, columns[0]);
				AssertEquals(true, columns[0].ColumnStyle.ReadOnly);
				AssertDefaultColumn(Customs.Business.BaseJobComInvoiceLine.Schema.JI_Calc_Invoice, columns[1]);
				AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_PartNo, columns[2]);
				AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_CC, columns[3]);
				AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_Tariff, columns[4]);
				AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_InvoiceQuantity, columns[5]);
				AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_InvoiceUQ, columns[6]);
				AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_CustomsQuantity, columns[7]);
				AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_CustomsUnitQty, columns[8]);
				AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_LinePrice, columns[9]);
				AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_Description, columns[10]);
				AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_CountryOfOrigin, columns[11]);
				AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_RH_NKCommodity_Code, columns[12]);
				//ensure all other columns are not visible by default
				var expectedRemainingColumns = 14;
				for (int i = expectedRemainingColumns; i < columns.Count; i++)
				{
					AssertEquals(string.Format("Column '{0}' should be not visible by default", columns[i].ColumnStyle.HeaderText), false, columns[i].IsVisible);
				}
			}
		}

		public void TestControlsVisibility()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var control = form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				var drops = control.FindAll<ZDropEdit>();
				Assert("Preference list", control.FindSingle<ZDropEdit>(x => x.Name == "zDropEditPref").Visible);
			}
		}

		public void TestPerUnitFieldsHave4Decimals()
		{
			Declaration.JE_MessageType = "";
			using (ZForm form = new ZForm(Declaration))
			{
				using (SGInvoiceLineUserControl invoiceLineUserControl = new SGInvoiceLineUserControl())
				{
					AssertEquals("OtherTax PerUnitRate should have 4 decimals", 4, invoiceLineUserControl.OtherTaxUnitRateCalcEdit.DecimalPlaces);
					AssertEquals("OtherTax Percentage Rate should remain 2 decimals", 2, invoiceLineUserControl.OtherTaxPercentageRateCalcEdit.DecimalPlaces);
					AssertEquals("Duty PerUnitRate should have 4 decimals", 4, invoiceLineUserControl.DutyUnitRateCalcEdit.DecimalPlaces);
					AssertEquals("Duty Percentage Rate should remain 2 decimals", 2, invoiceLineUserControl.DutyPercentageRateCalcEdit.DecimalPlaces);
					AssertEquals("Excise PerUnitRate should have 4 decimals", 4, invoiceLineUserControl.ExciseUnitRateCalcEdit.DecimalPlaces);
					AssertEquals("Excise Percentage Rate should remain 2 decimals", 2, invoiceLineUserControl.ExcisePercentageRateCalcEdit.DecimalPlaces);
				}
			}
		}

		void AssertDefaultColumn(string expectedName, ZGridColumn column)
		{
			AssertEquals(expectedName, column.ColumnStyle.MappingName);
			AssertEquals(true, column.IsVisible);
		}

		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
		JobDeclaration declaration;
	}
}

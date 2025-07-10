using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.TR.GUI.PlugIn;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.TR.GUI.Testing
{
	[TestedType(typeof(ExportSupplierHeaderUserControl))]
	class ExportSupplierHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<ExportSupplierHeaderUserControl, JobDeclaration>
	{
		public void TestJZ_RelatedIndicatorLocationAndTabIndex()
		{
			using (var control = new ExportSupplierHeaderUserControl())
			{
				var rlatedIndicator = control.FindSingleOrDefault<ZDropEdit>("JZ_RelatedIndicator");
				AssertNotNull("JZ_RelatedIndicator should not be null", rlatedIndicator);
				AssertEquals("TabIndex of JZ_RelatedIndicator should be 11", 11, rlatedIndicator.TabIndex);
				AssertEquals("Location of JZ_RelatedIndicator should be (118, 279)", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 279, true), rlatedIndicator.Location);
			}
		}

		public void TestControlsRemovedForCountry()
		{
			using (var frm = new ZForm(declaration))
			using (var control = new ExportSupplierHeaderUserControl())
			{
				AssertNotEquals("Inco Term Place", control.FindSingleOrDefault<ZTextBox>("JZ_IncoTermPlace"));
				AssertNotEquals("Valuation Code", control.FindSingleOrDefault<ZDropEdit>("JZ_ValuationCode"));
			}
		}

		public void TestGetAdditionalInfosUserControlType()
		{
			using (var control = new ExportSupplierHeaderUserControlForTest())
			{
				AssertEquals(typeof(AdditionalInfosUserControl), control.GetAdditionalInfosUserControlType_Exposed());
			}
		}

		public void TestLocalAndForeignControls()
		{
			using (var control = new ImportSupplierHeaderUserControl())
			{
				AssertNotNull(control.FindSingleOrDefault<ZGroupBox>("LocalChargesGroupBox"));
				AssertNotNull(control.FindSingleOrDefault<ZTabPage>("LocalChargesTabPage"));
				CombineAssertions("LocalChargesGrid", () =>
				{
					var localChargesGrid = control.FindSingleOrDefault<ZGrid>("LocalChargesGrid");
					AssertNotNull("Exist", localChargesGrid);
					AssertEquals("Binding", nameof(JobDeclaration.Invoices) + "." + nameof(JobComInvoiceHeader.LocalCharges), localChargesGrid.BindTo);
				});
				AssertNotNull(control.FindSingleOrDefault<ZGrid>("LocalChargesGrid"));
				CombineAssertions("LocalChargesExpectedConvertToLocalCurrencyControl", () =>
				{
					var currencyControl = ZControlExtensions.FindSingleOrDefault<ConvertToLocalCurrencyControl>(control, "LocalChargesExpectedConvertToLocalCurrencyControl");
					AssertNotNull("Control exists", currencyControl);
					AssertEquals("BindToAmount", nameof(JobDeclaration.Invoices) + "." + nameof(JobComInvoiceHeader.LocalChargesExpected), currencyControl.BindToAmount);
					AssertEquals("BindToUnit", nameof(JobDeclaration.Invoices) + "." + nameof(JobComInvoiceHeader.LocalChargesCurrency), currencyControl.BindToUnit);
				});
				CombineAssertions("LocalChargesEnteredConvertToLocalCurrencyControl", () =>
				{
					var currencyControl = control.FindSingleOrDefault<ConvertToLocalCurrencyControl>("LocalChargesEnteredConvertToLocalCurrencyControl");
					AssertNotNull("Control exists", currencyControl);
					AssertEquals("BindToAmount", nameof(JobDeclaration.Invoices) + "." + nameof(JobComInvoiceHeader.LocalChargesEntered), currencyControl.BindToAmount);
					AssertEquals("BindToUnit", nameof(JobDeclaration.Invoices) + "." + nameof(JobComInvoiceHeader.LocalChargesCurrency), currencyControl.BindToUnit);
				});
				CombineAssertions("LocalChargesBalanceConvertToLocalCurrencyControl", () =>
				{
					var currencyControl = control.FindSingleOrDefault<ConvertToLocalCurrencyControl>("LocalChargesBalanceConvertToLocalCurrencyControl");
					AssertNotNull("Control exists", currencyControl);
					AssertEquals("BindToAmount", nameof(JobDeclaration.Invoices) + "." + nameof(JobComInvoiceHeader.LocalChargesBalance), currencyControl.BindToAmount);
					AssertEquals("BindToUnit", nameof(JobDeclaration.Invoices) + "." + nameof(JobComInvoiceHeader.LocalChargesCurrency), currencyControl.BindToUnit);
				});

				AssertNotNull(control.FindSingleOrDefault<ZGroupBox>("ForeignChargesGroupBox"));
				AssertNotNull(control.FindSingleOrDefault<ZTabPage>("ForeignChargesTabPage"));
				CombineAssertions("ForeignChargesGrid", () =>
				{
					var foreignChargesGrid = control.FindSingleOrDefault<ZGrid>("ForeignChargesGrid");
					AssertNotNull("Exist", foreignChargesGrid);
					AssertEquals("Binding", nameof(JobDeclaration.Invoices) + "." + nameof(JobComInvoiceHeader.ForeignCharges), foreignChargesGrid.BindTo);
				});
				CombineAssertions("ForeignChargesExpectedConvertToLocalCurrencyControl", () =>
				{
					var currencyControl = ZControlExtensions.FindSingleOrDefault<ConvertToLocalCurrencyControl>(control, "ForeignChargesExpectedConvertToLocalCurrencyControl");
					AssertNotNull("Control exists", currencyControl);
					AssertEquals("BindToAmount", nameof(JobDeclaration.Invoices) + "." + nameof(JobComInvoiceHeader.ForeignChargesExpected), currencyControl.BindToAmount);
					AssertEquals("BindToUnit", nameof(JobDeclaration.Invoices) + "." + nameof(JobComInvoiceHeader.ForeignChargesCurrency), currencyControl.BindToUnit);
				});
				CombineAssertions("ForeignChargesEnteredConvertToLocalCurrencyControl", () =>
				{
					var currencyControl = control.FindSingleOrDefault<ConvertToLocalCurrencyControl>("ForeignChargesEnteredConvertToLocalCurrencyControl");
					AssertNotNull("Control exists", currencyControl);
					AssertEquals("BindToAmount", nameof(JobDeclaration.Invoices) + "." + nameof(JobComInvoiceHeader.ForeignChargesEntered), currencyControl.BindToAmount);
					AssertEquals("BindToUnit", nameof(JobDeclaration.Invoices) + "." + nameof(JobComInvoiceHeader.ForeignChargesCurrency), currencyControl.BindToUnit);
				});
				CombineAssertions("ForeignChargesBalanceConvertToForeignCurrencyControl", () =>
				{
					var currencyControl = control.FindSingleOrDefault<ConvertToLocalCurrencyControl>("ForeignChargesBalanceConvertToLocalCurrencyControl");
					AssertNotNull("Control exists", currencyControl);
					AssertEquals("BindToAmount", nameof(JobDeclaration.Invoices) + "." + nameof(JobComInvoiceHeader.ForeignChargesBalance), currencyControl.BindToAmount);
					AssertEquals("BindToUnit", nameof(JobDeclaration.Invoices) + "." + nameof(JobComInvoiceHeader.ForeignChargesCurrency), currencyControl.BindToUnit);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = MessageTypeList.Codes.Export;

				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JobComInvoiceLines.AddNew();
			}
		}

		JobDeclaration declaration;

		protected override IEnumerable<string> ExpectedControlList => DefaultControlList;

		class ExportSupplierHeaderUserControlForTest : ExportSupplierHeaderUserControl
		{
			public Type GetAdditionalInfosUserControlType_Exposed() => base.GetAdditionalInfosUserControlType();
		}
	}
}
